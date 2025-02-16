using System;
using UnityEngine;

namespace XX
{
	public class ThornDrawer
	{
		public ThornDrawer(float _resolution_h = 10f)
		{
			this.resolution_h = _resolution_h * 0.015625f;
			this.Md = new MeshDrawer(null, 4, 6);
			this.Md.draw_gl_only = true;
			this.Md.activate("", MTRX.MtrMeshNormal, true, MTRX.ColWhite, null);
			this.FCol = MTRX.ColWhite;
			this.BCol = C32.d2c(4283782485U);
		}

		public ThornDrawer Set(int _count, float starty_min, float starty_max, float totalheight_min, float totalheight_max, float width_min, float width_max, float wavewidth_min, float wavewidth_max, float wavelen_min = 4.8f, float wavelen_max = 12.8f)
		{
			this.count = _count;
			this.starty_min = starty_min * 0.015625f;
			this.starty_max = starty_max * 0.015625f;
			this.totalheight_min = totalheight_min * 0.015625f;
			this.totalheight_max = totalheight_max * 0.015625f;
			this.width_min = width_min * 0.015625f;
			this.width_max = width_max * 0.015625f;
			this.wavewidth_min = wavewidth_min * 0.015625f;
			this.wavewidth_max = wavewidth_max * 0.015625f;
			this.wavelen_min = wavelen_min;
			this.wavelen_max = wavelen_max;
			return this.randomize();
		}

		public ThornDrawer randomize()
		{
			this.ran = X.xors();
			this.created_level = -1f;
			return this;
		}

		public MeshDrawer RemakeMesh(float level, bool force = false)
		{
			if (level == this.created_level && !force)
			{
				return this.Md;
			}
			this.created_level = level;
			this.Md.clear(false, false);
			this.Md.Col = this.FCol;
			this.Md.ColGrd.Set((this.gradation_behind >= 1f) ? this.BCol : this.Md.ColGrd.Set(this.FCol).blend(this.BCol, this.gradation_behind).C);
			float base_z = this.Md.base_z;
			for (int i = 0; i < this.count; i++)
			{
				uint ran = X.GETRAN2((int)((ulong)(this.ran % 256U) + (ulong)((long)(i * 13))), (int)((ulong)(2U + this.ran % 7U) + (ulong)((long)(i * 3 % 11))));
				float num = X.NI(this.starty_min, this.starty_max, X.RAN(ran, 1945));
				float num2 = num;
				float num3 = X.NI(this.width_min, this.width_max, X.RAN(ran, 927));
				float num4 = X.NI(this.wavewidth_min, this.wavewidth_max, X.RAN(ran, 2627));
				float num5 = X.NI(this.wavelen_min, this.wavelen_max, X.RAN(ran, 2742)) * num4;
				float num6 = this.resolution_h / num5 * 6.2831855f * (float)X.MPF(X.RAN(ran, 2531) < 0.5f);
				float num7 = level * X.NI(1f, 1.2f, X.RAN(ran, 1894));
				float num8 = X.NI(this.startagR_min, this.startagR_max, X.RAN(ran, 1690)) + X.NI(this.agR_level_slide_min, this.agR_level_slide_max, X.RAN(ran, 2428)) * (float)X.MPF(X.RAN(ran, 810) < 0.5f) * num7;
				float num9 = X.NI(this.smallen_len_min, this.smallen_len_max, X.RAN(ran, 1387)) * num4;
				float num10 = X.NI(this.smallen_len_min, this.smallen_len_max, X.RAN(ran, 2803)) * num4;
				bool flag = X.RAN(ran, 2218) < 0.5f;
				float num11 = X.NI(this.totalheight_min, this.totalheight_max, X.RAN(ran, 1820)) * num7 - num;
				int num12 = (int)(num11 / this.resolution_h);
				float num13 = num11 - num10;
				bool flag2 = false;
				float num14 = num4 * X.Sin(num8);
				float num15 = 0f;
				float num16 = num2;
				float num17 = num2;
				float num18 = num14;
				float num19 = num14;
				int num20 = 2;
				int num21 = 0;
				uint num22 = ran;
				for (int j = 0; j <= num12; j++)
				{
					num22 = X.GETRAN2((int)((ulong)(num22 % 149U) + (ulong)((long)(j * 49))), (int)((ulong)(3U + num22 % 5U) + (ulong)((long)(j * 3))));
					float num23 = ((j == num12) ? (num + num11) : (num2 + this.resolution_h));
					num15 += this.resolution_h;
					num8 += ((j == num12) ? (num6 * (num23 - num2) / this.resolution_h) : num6) * X.NI(0.5f, 1.5f, X.RAN(num22, 2811)) * (float)((X.RAN(num22, 1030) < 0.2f) ? 3 : 1);
					num8 = X.correctangleR(num8);
					if (X.RAN(num22, 404) < 0.35f)
					{
						num6 = (float)X.MPF(num6 > 0f) * this.resolution_h / (X.NI(this.wavelen_min, this.wavelen_max, X.RAN(num22, 3911)) * num4) * 6.2831855f;
					}
					float num24 = num4 * X.Sin(num8);
					float num25 = num3 * X.NI(0.5f, 1f, X.RAN(num22, 1981));
					float num26 = 1f;
					if (num15 < num9)
					{
						num26 = X.ZSIN(num15, num9);
					}
					if (num15 > num13)
					{
						num26 = X.Mn(num26, 1f - X.ZSINV(num15 - num13, num10));
					}
					float num27 = X.GAR2(num2, num14, num23, num24);
					bool flag3 = X.BTW(-1.5707964f, num8, 1.5707964f);
					C32 c = (flag3 ? null : this.Md.ColGrd);
					this.Md.base_z = base_z + (flag3 ? 0f : this.z_bottom_shift);
					float num28 = num24 + X.Sin(num27 + 1.5707964f) * num25 * 0.5f * num26;
					float num29 = 2f * num24 - num28;
					float num30 = num23 + X.Cos(num27 + 1.5707964f) * num25 * 0.5f * num26;
					float num31 = 2f * num23 - num30;
					if (num23 > 0f)
					{
						if (!flag2)
						{
							if (num2 < 0f)
							{
								Vector3 vector = X.crosspoint(num16, num18, num30, num28, 0f, 0f, 0f, 1f);
								Vector3 vector2 = X.crosspoint(num17, num19, num31, num29, 0f, 0f, 0f, 1f);
								this.Md.Tri(0, 3, 1, false).Tri(0, 2, 3, false);
								this.Md.Pos(vector.x, vector.y, c).Pos(vector2.x, vector2.y, c).Pos(num30, num28, c)
									.Pos(num31, num29, c);
							}
							else
							{
								this.Md.Tri(0, 1, 2, false);
								this.Md.Pos(num2, num14, c).Pos(num30, num28, c).Pos(num31, num29, c);
							}
							flag2 = true;
						}
						else
						{
							this.Md.Tri(-num20, 1, -num20 + 1, false).Tri(-num20, 0, 1, false);
							this.Md.Pos(num30, num28, c).Pos(num31, num29, c);
							num20 = 2;
							if (--num21 <= 0)
							{
								num21 = this.thorn_intv;
								if (num26 > 0.4f)
								{
									float num32 = num25 * 0.675f * X.ZLINE(num26 - 0.4f, 0.6f) / 1.7320508f * 2f;
									float num33 = num32 * 1.5f;
									num20 += 2;
									if (flag)
									{
										float num34 = num30 + num32 * X.Cos(num27 + 2.0943952f);
										float num35 = num28 + num32 * X.Sin(num27 + 2.0943952f);
										this.Md.Tri(-2, 1, 0, false).Pos(num34, num35, c).Pos(num34 + num33 * X.Cos(num27 - 2.0943952f), num35 + num33 * X.Sin(num27 - 2.0943952f), c);
									}
									else
									{
										float num36 = num31 + num32 * X.Cos(num27 - 2.0943952f);
										float num37 = num29 + num32 * X.Sin(num27 - 2.0943952f);
										this.Md.Tri(-1, 0, 1, false).Pos(num36, num37, c).Pos(num36 + num33 * X.Cos(num27 + 2.0943952f), num37 + num33 * X.Sin(num27 + 2.0943952f), c);
									}
									flag = !flag;
								}
							}
						}
					}
					num2 = num23;
					num16 = num30;
					num17 = num31;
					num14 = num24;
					num18 = num28;
					num19 = num29;
				}
			}
			this.Md.base_z = base_z;
			return this.Md;
		}

		private int count;

		private uint ran;

		public float resolution_h;

		public float z_bottom_shift = 1f;

		public float startagR_min = -3.1415927f;

		public float startagR_max = 3.1415927f;

		public float starty_min = -0.78125f;

		public float starty_max = -1.25f;

		public float totalheight_min = 3.125f;

		public float totalheight_max = 3.828125f;

		public float width_min = 0.0625f;

		public float width_max = 0.09375f;

		public float wavewidth_min = 0.46875f;

		public float wavewidth_max = 0.546875f;

		public float wavelen_min = 3.8f;

		public float wavelen_max = 4.8f;

		public float agR_level_slide_min = 0.5654867f;

		public float agR_level_slide_max = 0.7853982f;

		public float smallen_len_min = 3.25f;

		public float smallen_len_max = 5f;

		public int thorn_intv = 2;

		private float created_level = -1f;

		public float gradation_behind;

		public Color32 FCol;

		public Color32 BCol;

		private MeshDrawer Md;
	}
}
