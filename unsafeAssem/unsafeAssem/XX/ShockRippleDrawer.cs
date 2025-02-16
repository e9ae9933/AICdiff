using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ShockRippleDrawer
	{
		public ShockRippleDrawer(PxlImage _BaseSpr, MImage MI)
		{
			this.ran0 = X.xors();
			if (_BaseSpr != null)
			{
				this.setBaseSpr(_BaseSpr, MI);
			}
		}

		public ShockRippleDrawer setBaseSpr(PxlImage BaseSpr, MImage MI)
		{
			this.texture_w = (float)BaseSpr.get_I().width;
			this.texture_h = (float)BaseSpr.get_I().height;
			this.RcUv = BaseSpr.RectIUv;
			this.TargetMI = MI;
			return this;
		}

		public ShockRippleDrawer Ran(uint _ran)
		{
			this.ran0 = _ran;
			return this;
		}

		public ShockRippleDrawer DivideCount(int _cnt)
		{
			this.divide_count = _cnt;
			return this;
		}

		public ShockRippleDrawer drawTo(MeshDrawer Md, float cx, float cy, float radius, float thick, float grd_level_out = 0f, float grd_level_in = 0f)
		{
			float width = this.RcUv.width;
			float height = this.RcUv.height;
			float x = this.RcUv.x;
			float y = this.RcUv.y;
			float num = 0.015625f;
			cx *= num;
			cy *= num;
			thick *= num;
			float num2 = thick * this.thick_randomize;
			float num3 = this.texture_w * width * this.texture_h_scale;
			float num4 = 1f / num3;
			bool flag = X.BTWS(0f, this.gradation_focus, 1f);
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? Md.ColGrd : MeshDrawer.ColBuf0.Set(Md.Col).blend(Md.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? Md.ColGrd : MeshDrawer.ColBuf1.Set(Md.Col).blend(Md.ColGrd, grd_level_in));
			}
			C32 c3 = (flag ? c : c2);
			for (int i = 0; i < this.divide_count; i++)
			{
				uint ran = X.GETRAN2((int)(this.ran0 % 7751U + (uint)(i * 7)), (int)((long)(i * 5) + (long)((ulong)(this.ran0 % 13U))));
				float num5 = this.sagR + ((float)i + this.angle_randomize * (-0.5f + X.RAN(ran, 2928))) * 6.2831855f / (float)this.divide_count;
				float num6 = (1f - this.angle_randomize * (0.6f + 0.4f * X.RAN(ran, 9454))) * 6.2831855f / (float)this.divide_count;
				float num7 = X.NI(this.height_min_h, this.height_max_h, X.RAN(ran, 1769));
				float num8 = (1f - num7) * X.RAN(ran, 2734);
				float num9 = X.Mx(0f, radius - this.radius_randomize_px * (-0.5f + X.RAN(ran, 1292)));
				float num10 = num9 * num;
				float num11 = num9 * 2f * num6;
				float num12 = X.RAN(ran, 1054);
				float num13 = num11 * num4;
				num12 = X.frac(num12 + (float)((int)num13) + 128f - num13 / 2f);
				bool flag2 = false;
				int num14 = 0;
				while (!flag2)
				{
					float num15 = 1f - num12;
					flag2 = num15 >= num13;
					if (flag2)
					{
						num15 = num13;
					}
					float num16 = num15 * num3;
					float num17 = num16 / num9;
					int num18 = X.IntR(num16 / 25f);
					if (num18 > 0 && num15 > 0f)
					{
						float num19 = num17 / (float)num18;
						float num20 = num15 / (float)num18;
						for (int j = 0; j <= num18; j++)
						{
							float num21 = num10;
							float num22 = num10 + thick;
							float num23 = 0f;
							float num24 = 0f;
							if (this.thick_randomize > 0f)
							{
								uint ran2 = X.GETRAN2((int)(ran % 6212U + (uint)num14), (int)((long)(i * 4 + num14 % 13) + (long)((ulong)(ran % 9U))));
								num21 += num2 * X.RAN(ran2, 1851);
								num22 += num2 * X.RAN(ran2, 3007);
								if (flag)
								{
									num24 = this.gradation_focus + this.thick_randomize * 0.5f * X.RAN(ran2, 1730);
									num23 = num10 + thick * num24;
								}
							}
							else if (flag)
							{
								num24 = this.gradation_focus;
								num23 = num10 + thick * this.gradation_focus;
							}
							float num25 = X.Cos(num5);
							float num26 = X.Sin(num5);
							Md.uvRectN(x + width * num12, y + height * num8).Pos(cx + num21 * num25, cy + num21 * num26, c3);
							if (flag)
							{
								Md.uvRectN(x + width * num12, y + height * (num8 + num7 * num24)).Pos(cx + num23 * num25, cy + num23 * num26, c2);
							}
							Md.uvRectN(x + width * num12, y + height * (num8 + num7)).Pos(cx + num22 * num25, cy + num22 * num26, c);
							if (j < num18)
							{
								if (flag)
								{
									Md.Tri(1, -2, 0, false).Tri(0, -2, -3, false).Tri(2, -2, 1, false)
										.Tri(2, -1, -2, false);
								}
								else
								{
									Md.Tri(-2, 0, -1, false).Tri(0, 1, -1, false);
								}
								num5 += num19;
								num12 += num20;
								num14++;
							}
						}
					}
					num13 -= num15;
					num12 = 0f;
				}
			}
			return this;
		}

		public uint ran0;

		public float sagR;

		public int divide_count = 6;

		public float texture_h_scale = 2f;

		public float angle_randomize = 0.5f;

		public float thick_randomize = 0.15f;

		public float radius_randomize_px = 40f;

		public float height_min_h = 0.45f;

		public float height_max_h = 0.6f;

		public float gradation_focus = 0.75f;

		private float texture_w;

		private float texture_h;

		private Rect RcUv = new Rect(0f, 0f, 1f, 1f);

		public MImage TargetMI;
	}
}
