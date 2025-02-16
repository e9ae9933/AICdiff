using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2FillingMistDrawer : IRunAndDestroy
	{
		public M2FillingMistDrawer(Map2d Mp, PxlSequence _Sq)
		{
			this.Sq = _Sq;
			if (Mp != null)
			{
				this.assignTo(Mp);
			}
			this.ran0 = X.xors();
		}

		public void destruct()
		{
			this.destruct(true);
		}

		private void destruct(bool deassign_runner)
		{
			if (this.Mp != null)
			{
				if (deassign_runner)
				{
					this.Mp.remRunnerObject(this);
				}
				this.Mp = null;
			}
			if (this.Gob != null)
			{
				if (!this.cache_gob)
				{
					IN.DestroyE(this.Gob);
					this.Gob = null;
					this.Md.destruct();
					this.Md = null;
					return;
				}
				this.Gob.SetActive(false);
			}
		}

		public void assignTo(Map2d _Mp)
		{
			if (_Mp == null || this.Mp == _Mp)
			{
				return;
			}
			if (this.Mp != null)
			{
				this.Mp.remRunnerObject(this);
			}
			this.Mp = _Mp;
			this.Mp.addRunnerObject(this);
			if (this.Acount_and_agR == null)
			{
				float seek_radius = this.seek_radius;
				float num = X.LENGTHXY(0f, 0f, IN.wh, IN.hh);
				using (BList<int> blist = ListBuffer<int>.Pop(0))
				{
					int i = 0;
					for (;;)
					{
						float num2 = (float)(++i) * seek_radius;
						if (num2 >= num + seek_radius)
						{
							break;
						}
						float num3 = num2 * 2f * 3.1415927f;
						int num4 = X.Mx(1, X.IntR(num3 / (seek_radius * 2f)));
						blist.Add(num4);
					}
					this.Acount_and_agR = new Vector3[blist.Count];
					int count = blist.Count;
					for (i = 0; i < count; i++)
					{
						this.Acount_and_agR[i] = new Vector3((float)blist[i], 0f, 1f / (float)blist[i]);
					}
				}
			}
			if (this.Gob == null)
			{
				this.Gob = IN.CreateGob(this.Mp.M2D.Cam.getGameObject(), "-Mist");
				this.Gob.layer = (this.assign_to_final_render ? this.Mp.M2D.Cam.getFinalRenderedLayer() : this.Mp.M2D.Cam.getFinalSourceRenderedLayer());
				this.Md = MeshDrawer.prepareMeshRenderer(this.Gob, MTRX.getMI(this.Sq.pChar, false).getMtr(this.blend, -1), 0f, -1, null, true, true);
				IN.setZ(this.Gob.transform, this.assign_to_final_render ? 35.5f : 118.5f);
				this.Valot = this.Gob.GetComponent<ValotileRenderer>();
				this.redraw();
			}
			else
			{
				this.Valot.ReleaseBinding(true, true, false);
			}
			this.Mp.M2D.Cam.assignRenderFunc(this.Valot, this.Gob.layer, false, this.Valot);
			this.Gob.SetActive(true);
			this.t = (this.t_fine = 0f);
		}

		public void fine()
		{
			if (this.t >= 0f)
			{
				if (this.t < this.maxt)
				{
					this.t = this.maxt;
					this.need_reset_alpha = true;
					return;
				}
			}
			else if (this.t > -this.maxt)
			{
				this.t = -this.maxt;
				this.need_reset_alpha = true;
			}
		}

		public void deactivate()
		{
			if (this.t >= 0f)
			{
				this.t = X.Mn(-1f, -this.maxt + this.t);
			}
		}

		public float seek_radius
		{
			get
			{
				return X.NI((float)this.Sq.width, (float)this.Sq.height, 0.5f) * 0.5f * this.draw_scale * 1.2f;
			}
		}

		public bool run(float fcnt)
		{
			if (this.t >= 0f)
			{
				if (this.t < this.maxt)
				{
					this.need_reset_alpha = true;
				}
				this.t += fcnt;
			}
			else
			{
				this.t -= fcnt;
				this.need_reset_alpha = true;
				if (this.t <= -this.maxt)
				{
					this.destruct(false);
					return false;
				}
			}
			this.t_fine += fcnt;
			if (this.t_fine >= 4f)
			{
				this.t_fine -= 4f;
				this.need_reset_alpha = true;
			}
			if (this.need_reset_alpha)
			{
				this.resetAlpha();
			}
			return true;
		}

		public void redraw()
		{
			if (this.Md == null)
			{
				return;
			}
			this.need_reset_alpha = true;
			this.Md.clear(false, false);
			int num = this.Acount_and_agR.Length;
			float seek_radius = this.seek_radius;
			this.Md.Col = this.C;
			for (int i = -1; i < num; i++)
			{
				Vector3 vector = ((i == -1) ? new Vector3(1f, 0f, 1f) : this.Acount_and_agR[i]);
				int num2 = (int)vector.x;
				uint ran = X.GETRAN2((int)((this.ran0 & 1048575U) + (uint)(i * 13)), 2 + i);
				float num3 = X.RAN(ran, 529) * 6.2831855f;
				float z = vector.z;
				if (i >= 0)
				{
					vector.y = num3;
					this.Acount_and_agR[i] = vector;
				}
				float num4 = (float)(i + 1) * seek_radius;
				for (int j = 0; j < num2; j++)
				{
					uint ran2 = X.GETRAN2((int)((ran & 1048575U) + 33U + (uint)(j * 7)), 4 + j);
					float num5 = ((float)j - 0.1f + 0.2f * X.RAN(ran2, 532)) * z * 6.2831855f + num3;
					PxlFrame frame = this.Sq.getFrame((int)((ulong)ran2 % (ulong)((long)this.Sq.countFrames())));
					float num6 = X.RAN(ran2, 774) * 6.2831855f;
					float num7 = X.NI(0.9f, 1.1f, X.RAN(ran2, 2992)) * this.draw_scale;
					this.Md.RotaPF(num4 * X.Cos(num5), num4 * X.Sin(num5), num7, num7, num6, frame, false, false, false, uint.MaxValue, false, 0);
				}
			}
		}

		public void resetAlpha()
		{
			if (this.Md == null)
			{
				return;
			}
			this.need_reset_alpha = false;
			M2Mover baseMover = this.Mp.M2D.Cam.getBaseMover();
			Vector2 vector;
			if (baseMover != null)
			{
				vector = new Vector2(this.Mp.map2globalux(baseMover.x), this.Mp.map2globaluy(baseMover.y)) - this.Mp.M2D.Cam.PosMainTransform;
			}
			else
			{
				vector = Vector2.zero;
			}
			vector *= 64f;
			int num = this.Acount_and_agR.Length;
			float seek_radius = this.seek_radius;
			int num2 = 0;
			float num3 = ((this.t >= 0f) ? X.ZLINE(this.t, this.maxt) : X.ZLINE(this.maxt + this.t, this.maxt));
			Color32[] colorArray = this.Md.getColorArray();
			for (int i = -1; i < num; i++)
			{
				Vector3 vector2 = ((i == -1) ? new Vector3(1f, 0f, 1f) : this.Acount_and_agR[i]);
				float z = vector2.z;
				int num4 = (int)vector2.x;
				float num5 = (float)(i + 1) * seek_radius;
				float y = vector2.y;
				for (int j = 0; j < num4; j++)
				{
					float num6 = y + (float)j * z * 6.2831855f;
					float num7 = num5 * X.Cos(num6);
					float num8 = num5 * X.Sin(num6);
					float num9 = X.saturate((X.NI(X.LENGTHXYS(num7, num8, vector.x, vector.y), X.LENGTHXYN(num7, num8, vector.x, vector.y), 0.5f) - this.thresh_len_pixel) * this.len_divide);
					int num10 = num2++ * 4;
					Color32 color = colorArray[num10];
					color.a = this.C.a;
					color = C32.MulA(color, num9 * num3);
					colorArray[num10++] = color;
					for (int k = 0; k < 3; k++)
					{
						colorArray[num10++] = color;
					}
				}
			}
			this.Md.updateForMeshRenderer(true);
		}

		public bool assigned
		{
			get
			{
				return this.Mp != null;
			}
		}

		public override string ToString()
		{
			return "FillingMist";
		}

		public PxlSequence Sq;

		public BLEND blend;

		public uint ran0;

		public Color32 C = MTRX.ColWhite;

		public bool assign_to_final_render;

		public float draw_scale = 2f;

		public float thresh_len_pixel = 50f;

		public float len_divide = 0.02f;

		public bool need_reset_alpha;

		private Map2d Mp;

		public float maxt = 70f;

		private float t;

		private float t_fine;

		private Vector3[] Acount_and_agR;

		public bool cache_gob;

		private MeshDrawer Md;

		private GameObject Gob;

		private ValotileRenderer Valot;
	}
}
