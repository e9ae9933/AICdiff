using System;
using UnityEngine;

namespace XX
{
	public abstract class RippleDoorDrawer
	{
		public abstract int putVertice(MeshDrawer Md, float w, float h);

		public RippleDoorDrawer(float _w, float _h, int _count)
		{
			this.ran0 = (uint)X.xors(16777215);
			this.count = _count;
			this.WH(_w, _h);
		}

		public virtual RippleDoorDrawer WH(float _w, float _h)
		{
			if (_w != this.w0 || _h != this.h0)
			{
				this.w0 = _w;
				this.h0 = _h;
				this.need_check_dir = -1;
			}
			return this;
		}

		public int drawTo(MeshDrawer Md, float time, float x, float y)
		{
			Md.Col = Md.ColGrd.Set(this.BaseColor).setA1(1f).C;
			float num = Md.base_x;
			float num2 = Md.base_y;
			Md.base_x = (RippleDoorDrawer.base_x = Md.base_x + x * 0.015625f);
			Md.base_y = (RippleDoorDrawer.base_y = Md.base_y + y * 0.015625f);
			int vertexMax = Md.getVertexMax();
			int num3 = this.putVertice(Md, this.w0, this.h0);
			if (this.need_check_dir != num3)
			{
				this.recheckDirection(Md, num3);
			}
			if (this.BaseColor.a == 255 || this.ripple_thick <= 0f)
			{
				this.makeFillTri(Md, num3, 0f, 0f);
			}
			else
			{
				float num4 = this.ripple_thick;
				Color32 c = Md.ColGrd.Set(this.LineColorIn).blend(this.BaseColor, (float)this.BaseColor.a / 255f).C;
				Color32 c2 = Md.ColGrd.Set(this.LineColorOut).blend(this.BaseColor, (float)this.BaseColor.a / 255f).C;
				Md.Col = c2;
				num3 = this.putVertice(Md, this.w0 - num4 * 4f, this.h0 - num4 * 4f);
				this.verticeShuffle(Md, time, num3, this.w0 * 2f, -1);
				this.makeLineTri(Md, num3);
				if (this.BaseColor.a > 0)
				{
					this.Copy(Md, vertexMax, num3, Md.ColGrd.Set(this.BaseColor));
					this.makeFillTri(Md, num3, 0f, 0f);
				}
				float num5 = this.maxt / (float)this.count;
				for (int i = 0; i < this.count; i++)
				{
					float num6 = time - num5 * (float)i;
					if (num6 >= 0f)
					{
						int num7 = (int)(num6 / this.maxt);
						float num8 = X.ZLINE(num6 - this.maxt * (float)num7, this.maxt);
						float num9 = this.w0 * num8;
						float num10 = this.h0 * num8;
						if (num9 > num4 * 2f && num10 > num4 * 2f)
						{
							Md.Col = Md.ColGrd.Set(c).blend(c2, X.ZPOW(num8)).C;
							Md.Col = Md.ColGrd.Set(this.BaseColor).setA((float)Md.Col.a).blend(Md.Col, 1f - X.ZLINE(num8 - 0.875f, 0.125f))
								.C;
							int num11 = i + num7 * 64;
							num3 = this.putVertice(Md, num9, num10);
							this.verticeShuffle(Md, time, num3, num9, num11);
							num3 = this.putVertice(Md, num9 - num4 * 2f, num10 - num4 * 2f);
							this.verticeShuffle(Md, time, num3, num9, num11);
							this.makeLineTri(Md, num3);
						}
					}
				}
			}
			Md.base_x = num;
			Md.base_y = num2;
			return Md.getVertexMax() - vertexMax;
		}

		private void recheckDirection(MeshDrawer Md, int cnt)
		{
			if (this.AdirR == null || this.AdirR.Length != cnt)
			{
				this.AdirR = new float[cnt];
			}
			this.need_check_dir = cnt;
			int num = Md.getVertexMax() - cnt;
			Vector3[] vertexArray = Md.getVertexArray();
			for (int i = 0; i < cnt; i++)
			{
				Vector3 vector = vertexArray[num++];
				this.AdirR[i] = X.GAR2(RippleDoorDrawer.base_x, RippleDoorDrawer.base_y, vector.x, vector.y);
			}
		}

		private void Copy(MeshDrawer Md, int start_i, int cnt, C32 Color = null)
		{
			Vector3[] vertexArray = Md.getVertexArray();
			int num = start_i + cnt;
			Md.allocVer(num, 64);
			for (int i = start_i; i < num; i++)
			{
				Vector3 vector = vertexArray[i];
				Md.Pos(vector.x - RippleDoorDrawer.base_x, vector.y - RippleDoorDrawer.base_y, Color);
			}
			if (Color != null)
			{
				Md.Col = Color.C;
			}
		}

		private void verticeShuffle(MeshDrawer Md, float time, int cnt, float wdraw, int ripple_i)
		{
			if (wdraw <= 0f)
			{
				return;
			}
			wdraw /= this.w0;
			float num = this.w0 * 0.015625f;
			float num2 = this.h0 * 0.015625f;
			Vector3[] vertexArray = Md.getVertexArray();
			int num3 = Md.getVertexMax() - cnt;
			float num4 = X.ZLINE(wdraw - 1f, 0.25f);
			float num5;
			if (wdraw < 0.75f)
			{
				num5 = X.ZSIN(wdraw, 0.75f);
			}
			else
			{
				num5 = 1f - X.ZPOW(wdraw - 0.75f, 0.25f);
			}
			for (int i = 0; i < cnt; i++)
			{
				uint ran = X.GETRAN2((int)((this.ran0 & 255U) + (uint)(i * 13) + (uint)(ripple_i * 7) + 772U), 3 + i % 4 + ripple_i % 9);
				float num6 = num5 * 0.125f * 0.25f * X.COSI(time + X.RAN(ran, 928) * 1024f, 70f + X.RAN(ran, 1580) * 58f);
				if (num4 > 0f)
				{
					num6 = X.NAIBUN_I(num6, 0.03125f * (-1f + X.COSI(time + X.RAN(ran, 463) * 1024f, 64f + X.RAN(ran, 1973) * 91f)), num4);
				}
				if (num6 == 0f)
				{
					num3++;
				}
				else
				{
					float num7 = this.AdirR[i];
					Vector3 vector = vertexArray[num3];
					vector.x += num6 * num * X.Cos(num7);
					vector.y += num6 * num2 * X.Sin(num7);
					vertexArray[num3++] = vector;
				}
			}
		}

		public virtual int makeFillTri(MeshDrawer Md, int cnt, float posx_u = 0f, float posy_u = 0f)
		{
			int num = -cnt;
			for (int i = 0; i < cnt; i++)
			{
				Md.Tri(0, num + i, num + (i + 1) % cnt, false);
			}
			Md.Pos(posx_u, posy_u, null);
			return 1;
		}

		public void makeLineTri(MeshDrawer Md, int cnt)
		{
			int num = -cnt * 2;
			int num2 = -cnt;
			for (int i = 0; i < cnt; i++)
			{
				int num3 = (i + 1) % cnt;
				Md.Tri(num + i, num + num3, num2 + i, false);
				Md.Tri(num2 + i, num + num3, num2 + num3, false);
			}
		}

		public int getDirectionMax()
		{
			return this.AdirR.Length;
		}

		public float getDirectionR(int i)
		{
			return this.AdirR[i];
		}

		public uint ran0;

		public float maxt = 90f;

		public int count = 3;

		protected float w0;

		protected float h0;

		public Color32 BaseColor = MTRX.ColTrnsp;

		public Color32 LineColorIn = MTRX.ColWhite;

		public Color32 LineColorOut = MTRX.ColGray;

		public int need_check_dir = -1;

		public float ripple_thick = 1.5f;

		private float[] AdirR;

		private static float base_x;

		private static float base_y;
	}
}
