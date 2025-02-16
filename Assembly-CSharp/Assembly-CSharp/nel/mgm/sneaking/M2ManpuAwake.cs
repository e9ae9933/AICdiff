using System;
using m2d;
using PixelLiner;
using XX;

namespace nel.mgm.sneaking
{
	public class M2ManpuAwake
	{
		public M2ManpuAwake(M2Mover _Mv)
		{
			this.Mv = _Mv;
			this.Mp = this.Mv.Mp;
			this.FD_EfDraw = delegate(EffectItem Ef)
			{
				if (!this.efDraw(Ef))
				{
					if (Ef == this.Ef)
					{
						this.Ef = null;
					}
					return false;
				}
				return true;
			};
		}

		public void destruct()
		{
			if (this.Ef != null)
			{
				this.Ef.destruct();
				this.Ef = null;
			}
		}

		public void run(float fcnt)
		{
			if (this.Ef != null && X.BTW(0f, this.Ef.z, 1f) && this.Mp.floort > (float)this.Ef.time)
			{
				this.Ef.z = X.VALWALK(this.Ef.z, 0f, fcnt * 0.012f);
			}
		}

		public void activateCheck(float check_level = 0f, float fcnt = 1f)
		{
			if (this.Ef == null)
			{
				this.Ef = this.Mp.getEffectTop().setEffectWithSpecificFn("awake_hkds", 0f, 0f, -1f, 0, 0, this.FD_EfDraw);
				if (this.SqHk == null)
				{
					PxlPose poseByName = MTRX.PxlIcon.getPoseByName("nel_npc_question");
					this.SqHk = poseByName.getSequence(0);
					this.SqCol = poseByName.getSequence(1);
					this.SqExc = poseByName.getSequence(2);
				}
			}
			this.Ef.time = (int)this.Mp.floort + 40;
			if (check_level < 1f)
			{
				check_level = X.Mx(0f, this.Ef.z) + check_level * this.check_level_add_speed * fcnt;
			}
			if (check_level >= 1f)
			{
				if (this.Ef.z < 1f)
				{
					this.Ef.z = 1f;
					this.Ef.af = 0f;
					this.Mv.playSndPos("sneaking_aware", 1);
					return;
				}
			}
			else
			{
				if (this.Ef.z < 0f || this.Ef.z >= 1f)
				{
					this.Ef.af = 0f;
					this.Mv.playSndPos("sneaking_check", 1);
				}
				this.Ef.z = check_level;
			}
		}

		public bool is_awared
		{
			get
			{
				return this.Ef != null && this.Ef.z >= 1f;
			}
		}

		public void deactivate()
		{
			if (this.Ef != null && this.Ef.z >= 0f)
			{
				this.Ef.z = -1f;
				this.Ef.af = 0f;
			}
		}

		private bool efDraw(EffectItem E)
		{
			if (this.SqHk == null)
			{
				return false;
			}
			E.x = this.Mv.x;
			E.y = this.Mv.mtop - 0.25f;
			if (!this.Mp.M2D.Cam.isCoveringMp(E.x, E.y, E.x, E.y, 30f))
			{
				return true;
			}
			int num = this.SqHk.countFrames();
			MeshDrawer meshImg = E.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			if (E.z < 0f)
			{
				int num2 = num - 2 - (int)(E.af / 4f);
				if (num2 < 0)
				{
					return false;
				}
				meshImg.RotaPF(0f, 0f, 1f, 1f, 0f, this.SqHk.getFrame(num2), false, false, false, 1U, false, 0);
			}
			else if (E.z < 1f)
			{
				int num3 = ((E.af >= (float)(4 * (num - 1))) ? (num - 1) : X.Mn(num - 1, (int)(E.af / 4f)));
				meshImg.Col = meshImg.ColGrd.White().mulA(0.75f + 0.25f * X.COSIT(150f)).C;
				meshImg.RotaPF(0f, 0f, 1f, 1f, 0f, this.SqHk.getFrame(num3), false, false, false, uint.MaxValue, false, 0);
				if (E.z > 0f)
				{
					PxlLayer layer = this.SqCol.getFrame(0).getLayer(0);
					float num4 = -layer.y - (float)layer.Img.height * 0.5f;
					float num5 = layer.x - (float)layer.Img.width * 0.5f;
					float num6 = (float)layer.Img.width;
					float num7 = (float)layer.Img.height * E.z;
					meshImg.uvRect(num5 * 0.015625f, num4 * 0.015625f, (float)layer.Img.width * 0.015625f, (float)layer.Img.height * 0.015625f, layer.Img, false, true);
					meshImg.TriRectBL(0);
					meshImg.PosD(num5, num4, null).PosD(num5, num4 + num7, null).PosD(num5 + num6, num4 + num7, null)
						.PosD(num5 + num6, num4, null);
				}
			}
			else
			{
				float num8 = (1f - X.ZPOW(E.af, 25f)) * 4.5f;
				float num9 = 0f;
				float num10 = 0f;
				if (num8 > 0f)
				{
					num9 = X.COSIT(3.71f) * num8;
					num10 = X.COSIT(4.29f) * num8;
				}
				meshImg.RotaPF(num9, num10, 1f, 1f, 0f, this.SqExc.getFrame(0), false, false, false, uint.MaxValue, false, 0);
			}
			return true;
		}

		public readonly Map2d Mp;

		public readonly M2Mover Mv;

		private EffectItem Ef;

		private FnEffectRun FD_EfDraw;

		private const float Z_DEACTIVATE = -1f;

		private const float Z_CHECK = 0f;

		private const float Z_AWARE = 1f;

		private PxlSequence SqHk;

		private PxlSequence SqCol;

		private PxlSequence SqExc;

		public float check_level_add_speed = 0.1f;
	}
}
