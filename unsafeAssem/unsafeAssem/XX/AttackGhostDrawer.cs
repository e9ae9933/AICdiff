using System;
using Better;
using UnityEngine;

namespace XX
{
	public class AttackGhostDrawer
	{
		public AttackGhostDrawer()
		{
			this.ran0 = X.xors(16777215);
			this.PosPx(30f, 30f, -80f, -25f, 35f, 0f, -2.3561945f);
			this.RandomizePx(20f, 63f, 0f, 13f, 58f);
			this.HeightPx(5f, 30f).BezierExtendPx(30f, 35f);
			this.FD_EfDraw = new FnEffectRun(this.EfDrawInner);
		}

		public AttackGhostDrawer(CsvReader CR)
		{
			this.ran0 = X.xors(16777215);
			this.initCR();
			this.FD_EfDraw = new FnEffectRun(this.EfDrawInner);
		}

		public AttackGhostDrawer copyFrom(string _key)
		{
			AttackGhostDrawer agd = EfParticleManager.GetAGD(_key);
			if (agd == null)
			{
				return this;
			}
			return this.copyFrom(agd);
		}

		public AttackGhostDrawer copyFrom(AttackGhostDrawer Src)
		{
			this.time_center = Src.time_center;
			this.bezier_agR = Src.bezier_agR;
			this.bezier_agR_hrhb = Src.bezier_agR_hrhb;
			this.zd_agR = Src.zd_agR;
			this.count = Src.count;
			this.draw_len = Src.draw_len;
			this.draw_center_lvl = Src.draw_center_lvl;
			this.resolution_sz = Src.resolution_sz;
			this.resolution_zd = Src.resolution_zd;
			this.ColFS = Src.ColFS;
			this.ColBS = Src.ColBS;
			this.ColFE = Src.ColFE;
			this.ColBE = Src.ColBE;
			this.s_ux = Src.s_ux;
			this.s_uy = Src.s_uy;
			this.d_ux = Src.d_ux;
			this.d_uy = Src.d_uy;
			this.z_ux = Src.z_ux;
			this.z_uy = Src.z_uy;
			this.posS_i_b = Src.posS_i_b;
			this.posS_i_h = Src.posS_i_h;
			this.posS_rndm_b = Src.posS_rndm_b;
			this.posS_rndm_h = Src.posS_rndm_h;
			this.posD_rndm_b = Src.posD_rndm_b;
			this.posD_rndm_h = Src.posD_rndm_h;
			this.posD_rndm_zd = Src.posD_rndm_zd;
			this.height_min = Src.height_min;
			this.height_max = Src.height_max;
			this.z_bext_min = Src.z_bext_min;
			this.z_bext_max = Src.z_bext_max;
			return this;
		}

		public AttackGhostDrawer PosPx(float sx, float sy, float dx, float dy, float zx, float zy, float zagR)
		{
			this.s_ux = sx * 0.015625f;
			this.s_uy = sy * 0.015625f;
			this.d_ux = dx * 0.015625f;
			this.d_uy = dy * 0.015625f;
			this.z_ux = zx * 0.015625f;
			this.z_uy = zy * 0.015625f;
			this.bezier_agR = zagR;
			this.zd_agR = X.GAR2(zx, zy, dx, dy);
			return this;
		}

		public AttackGhostDrawer RandomizePx(float sb, float sh, float db, float dh, float zd = 0f)
		{
			this.posS_rndm_b = sb * 0.015625f;
			this.posS_rndm_h = sh * 0.015625f;
			this.posD_rndm_b = db * 0.015625f;
			this.posD_rndm_h = dh * 0.015625f;
			this.posD_rndm_zd = zd * 0.015625f;
			return this;
		}

		public AttackGhostDrawer HeightPx(float hmin, float hmax)
		{
			this.height_min = hmin * 0.015625f;
			this.height_max = hmax * 0.015625f;
			return this;
		}

		public AttackGhostDrawer ProgressPx(float b, float h)
		{
			this.posS_i_b = b * 0.015625f;
			this.posS_i_h = h * 0.015625f;
			return this;
		}

		public AttackGhostDrawer BezierExtendPx(float bmin, float bmax = -1000f)
		{
			this.z_bext_min = bmin * 0.015625f;
			this.z_bext_max = ((bmax == -1000f) ? this.z_bext_min : (bmax * 0.015625f));
			return this;
		}

		public AttackGhostDrawer Scale(float scl)
		{
			this.s_ux *= scl;
			this.s_uy *= scl;
			this.d_ux *= scl;
			this.d_uy *= scl;
			this.z_ux *= scl;
			this.z_uy *= scl;
			this.posS_i_b *= scl;
			this.posS_i_h *= scl;
			this.posS_rndm_b *= scl;
			this.posS_rndm_h *= scl;
			this.posD_rndm_b *= scl;
			this.posD_rndm_h *= scl;
			this.posD_rndm_zd *= scl;
			this.height_min *= scl;
			this.height_max *= scl;
			this.z_bext_min *= scl;
			this.z_bext_max *= scl;
			return this;
		}

		public AttackGhostDrawer Col(uint colfs, uint colbs)
		{
			return this.Col(colfs, colbs, colfs & 16777215U, colbs & 16777215U);
		}

		public AttackGhostDrawer Col(uint colfs, uint colbs, uint colfe, uint colbe)
		{
			this.ColFS = C32.d2c(colfs);
			this.ColFE = C32.d2c(colfe);
			this.ColBS = C32.d2c(colbs);
			this.ColBE = C32.d2c(colbe);
			return this;
		}

		public AttackGhostDrawer drawTo(MeshDrawer Md, float basex, float basey, float t_lvl, Func<MeshDrawer, int, uint, bool> FnUv2 = null)
		{
			float base_x = Md.base_x;
			float base_y = Md.base_y;
			Md.base_x += basex * 0.015625f;
			Md.base_y += basey * 0.015625f;
			float num;
			if (t_lvl < this.time_center)
			{
				num = X.ZSIN(t_lvl, this.time_center);
			}
			else
			{
				num = 1f + X.ZSIN(t_lvl - this.time_center, 1f - this.time_center);
			}
			float num2 = 1f - X.ZPOW(1f - t_lvl);
			AttackGhostDrawer.Cf.Set(this.ColFS).blend(this.ColFE, num2);
			AttackGhostDrawer.Cb.Set(this.ColBS).blend(this.ColBE, num2);
			num *= (2f + this.draw_len) / 2f;
			float num3 = num - this.draw_len;
			float num4 = X.Mn(num, 2f);
			float num5 = 1f / this.draw_len;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			float num9 = 0f;
			if (this.bezier_agR_hrhb == 0f)
			{
				num6 = X.Cos(this.bezier_agR);
				num7 = X.Sin(this.bezier_agR);
				num9 = -num6;
				num8 = num7;
			}
			float num10 = 1.0000012f / (float)this.resolution_sz;
			float num11 = 1.0000012f / (float)this.resolution_zd;
			C32 colGrd = Md.ColGrd;
			if (this.use_center_col)
			{
				colGrd.Set(this.ColCS).blend(this.ColCE, t_lvl);
			}
			else
			{
				colGrd.Set(AttackGhostDrawer.Cf).blend(AttackGhostDrawer.Cb, 0.5f);
			}
			if (FnUv2 != null)
			{
				FnUv2(Md, -this.count, (uint)this.ran0);
			}
			float num12 = 1f / (float)this.count;
			for (int i = 0; i < this.count; i++)
			{
				uint ran = X.GETRAN2(this.ran0 + i * 13, (this.ran0 & 7) + 4 + i % 5);
				if (this.bezier_agR_hrhb != 0f)
				{
					float num13 = this.bezier_agR + (-0.5f + X.RAN(ran, 2788)) * 2f * this.bezier_agR_hrhb;
					num6 = X.Cos(num13);
					num7 = X.Sin(num13);
					num9 = -num6;
					num8 = num7;
				}
				float num14 = X.RAN(ran, 670) * this.posS_rndm_b;
				float num15 = -X.RAN(ran, 2758) * this.posS_rndm_h;
				float num16 = X.RAN(ran, 1765) * this.posD_rndm_b;
				float num17 = X.RAN(ran, 820) * this.posD_rndm_h;
				float num18 = num12 * (float)i;
				float num19 = num6 * num14 + num6 * (num18 * this.posS_i_b) + num8 * num15 + num8 * (num18 * this.posS_i_h);
				float num20 = num7 * num14 + num7 * (num18 * this.posS_i_b) + num9 * num15 + num9 * (num18 * this.posS_i_h);
				float num21 = X.NI(this.z_bext_min, this.z_bext_max, X.RAN(ran, 980));
				float num22 = num6 * num21;
				float num23 = num7 * num21;
				float num24 = this.posD_rndm_zd * X.RAN(ran, 2720);
				float num25 = this.d_ux + X.Cos(this.zd_agR) * num24 + num6 * num16 + num8 * num17;
				float num26 = this.d_uy + X.Sin(this.zd_agR) * num24 + num7 * num16 + num9 * num17;
				float num27 = X.NI(this.height_min, this.height_max, X.RAN(ran, 2666)) * 0.5f;
				float num28 = 0f;
				float num29 = 0f;
				int num30 = 0;
				float num31 = X.Mx(0f, num3);
				bool flag = num31 < 1f;
				float num32 = (flag ? num10 : num11);
				float num33;
				float num34;
				for (;;)
				{
					num33 = num19;
					num34 = num20;
					bool flag2 = num31 >= num4;
					if (!flag2 || num31 < 2f)
					{
						if (flag)
						{
							num33 += X.BEZIER_I(this.s_ux, this.s_ux, this.z_ux - num22, this.z_ux, num31);
							num34 += X.BEZIER_I(this.s_uy, this.s_uy, this.z_uy - num23, this.z_uy, num31);
						}
						else
						{
							num33 += X.BEZIER_I(this.z_ux, this.z_ux + num22, num25, num25, num31 - 1f);
							num34 += X.BEZIER_I(this.z_uy, this.z_uy + num23, num26, num26, num31 - 1f);
						}
					}
					if (flag2)
					{
						break;
					}
					if (num30 > 0)
					{
						float num35 = X.GAR2(num28, num29, num33, num34) + 1.5707964f;
						float num36 = X.Sin(num35);
						float num37 = X.Cos(num35);
						float num38 = X.ZLINE(num31 - num3, this.draw_len);
						float num39 = X.ZLINE(num31 - num3, 0.25f) * (0.125f + 0.875f * X.ZLINE(num4 - num31, 0.25f));
						if (num38 < this.draw_center_lvl)
						{
							num39 *= X.ZSIN2(num38, this.draw_center_lvl);
						}
						else
						{
							num39 *= 1f - X.ZPOW(num38 - this.draw_center_lvl, 1f - this.draw_center_lvl);
						}
						float num40 = num37 * num27 * num39;
						float num41 = num36 * num27 * num39;
						if (this.use_center_col)
						{
							if (num30 == 1)
							{
								Md.Tri(-1, 1, 0, false).Tri(-1, 0, 2, false);
							}
							else
							{
								Md.Tri(-3, -2, 1, false).Tri(-3, 1, 0, false).Tri(-3, 0, 1, false)
									.Tri(0, 2, -1, false);
							}
							Md.Pos(num33, num34, colGrd).Pos(num33 + num40, num34 + num41, AttackGhostDrawer.Cf).Pos(num33 - num40, num34 - num41, AttackGhostDrawer.Cb);
						}
						else
						{
							if (num30 == 1)
							{
								Md.Tri(-1, 0, 1, false);
							}
							else
							{
								Md.Tri(0, 1, -1, false).Tri(0, -1, -2, false);
							}
							Md.Pos(num33 + num40, num34 + num41, AttackGhostDrawer.Cf).Pos(num33 - num40, num34 - num41, AttackGhostDrawer.Cb);
						}
					}
					else
					{
						Md.Pos(num33, num34, colGrd);
					}
					num28 = num33;
					num29 = num34;
					num30++;
					num31 += num32;
					if (flag && num31 >= 1f)
					{
						flag = false;
						num32 = num11;
					}
				}
				if (num30 > 1)
				{
					if (num31 >= 2f)
					{
						num33 = num25 + num19;
						num34 = num26 + num20;
					}
					if (this.use_center_col)
					{
						Md.Tri(0, -1, -3, false).Tri(0, -3, -2, false).Pos(num33, num34, colGrd);
					}
					else
					{
						Md.Tri(0, -1, -2, false).Pos(num33, num34, colGrd);
					}
				}
				if (FnUv2 != null)
				{
					FnUv2(Md, i, ran);
				}
			}
			Md.base_x = base_x;
			Md.base_y = base_y;
			return this;
		}

		public AttackGhostDrawer initCR()
		{
			this.zd_agR = -1000f;
			this.ColFS = (this.ColFE = C32.d2c(uint.MaxValue));
			this.ColBS = (this.ColBE = C32.d2c(4278190080U));
			return this;
		}

		public AttackGhostDrawer endCR()
		{
			if (this.zd_agR == -1000f)
			{
				this.zd_agR = X.GAR2(this.z_ux, this.z_uy, this.d_ux, this.d_uy);
			}
			return null;
		}

		public bool readCR(CsvReader CR)
		{
			string cmd = CR.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 967958004U)
				{
					if (num <= 488725647U)
					{
						if (num <= 465518633U)
						{
							if (num != 19619318U)
							{
								if (num == 465518633U)
								{
									if (cmd == "srand")
									{
										this.posS_rndm_b = CR.NE1() * 0.015625f;
										this.posS_rndm_h = CR.NE2() * 0.015625f;
									}
								}
							}
							else if (cmd == "zd_agR")
							{
								this.zd_agR = CR.slice_eval(1, " ");
							}
						}
						else if (num != 470150008U)
						{
							if (num == 488725647U)
							{
								if (cmd == "resolution")
								{
									this.resolution_sz = (int)CR.NE1();
									this.resolution_zd = ((CR.clength <= 2) ? this.resolution_sz : ((int)CR.NE1()));
								}
							}
						}
						else if (cmd == "spos")
						{
							this.s_ux = CR.NE1() * 0.015625f;
							this.s_uy = CR.NE2() * 0.015625f;
						}
					}
					else if (num <= 765810840U)
					{
						if (num != 691698746U)
						{
							if (num == 765810840U)
							{
								if (cmd == "col_b")
								{
									this.ColBS = C32.d2c(X.NmUI(CR._1, 4278190080U, true, true));
									this.ColBE = ((CR.clength <= 2) ? this.ColBS : C32.d2c(X.NmUI(CR._2, 4278190080U, true, true)));
								}
							}
						}
						else if (cmd == "zagR_hrhb")
						{
							this.bezier_agR_hrhb = CR.slice_eval(1, " ") * 0.015625f;
						}
					}
					else if (num != 832921316U)
					{
						if (num == 967958004U)
						{
							if (cmd == "count")
							{
								this.count = (int)CR.slice_eval(1, " ");
							}
						}
					}
					else if (cmd == "col_f")
					{
						this.ColFS = C32.d2c(X.NmUI(CR._1, uint.MaxValue, true, true));
						this.ColFE = ((CR.clength <= 2) ? this.ColFS : C32.d2c(X.NmUI(CR._2, uint.MaxValue, true, true)));
					}
				}
				else if (num <= 1843605105U)
				{
					if (num <= 1422428337U)
					{
						if (num != 1208764063U)
						{
							if (num == 1422428337U)
							{
								if (cmd == "zagR")
								{
									this.bezier_agR = CR.slice_eval(1, " ");
								}
							}
						}
						else if (cmd == "drand_zd")
						{
							this.posD_rndm_zd = CR.slice_eval(1, " ") * 0.015625f;
						}
					}
					else if (num != 1691464118U)
					{
						if (num == 1843605105U)
						{
							if (cmd == "zpos")
							{
								this.z_ux = CR.NE1() * 0.015625f;
								this.z_uy = CR.NE2() * 0.015625f;
							}
						}
					}
					else if (cmd == "progress_i")
					{
						this.posS_i_b = CR.NE1() * 0.015625f;
						this.posS_i_h = CR.NE2() * 0.015625f;
					}
				}
				else if (num <= 2681754135U)
				{
					if (num != 2109080643U)
					{
						if (num == 2681754135U)
						{
							if (cmd == "z_bext")
							{
								this.z_bext_min = CR.NE1() * 0.015625f;
								this.z_bext_max = ((CR.clength <= 2) ? this.z_bext_min : (CR.NE2() * 0.015625f));
							}
						}
					}
					else if (cmd == "dpos")
					{
						this.d_ux = CR.NE1() * 0.015625f;
						this.d_uy = CR.NE2() * 0.015625f;
					}
				}
				else if (num != 2720010832U)
				{
					if (num != 3045723460U)
					{
						if (num == 3585981250U)
						{
							if (cmd == "height")
							{
								this.height_min = CR.NE1() * 0.015625f;
								this.height_max = ((CR.clength <= 2) ? this.height_min : (CR.NE2() * 0.015625f));
							}
						}
					}
					else if (cmd == "drand")
					{
						this.posD_rndm_b = CR.NE1() * 0.015625f;
						this.posD_rndm_h = CR.NE2() * 0.015625f;
					}
				}
				else if (cmd == "time_center")
				{
					this.time_center = CR.slice_eval(1, " ") * 0.015625f;
				}
			}
			return false;
		}

		private bool EfDrawInner(EffectItem Ef)
		{
			if (AttackGhostDrawer.OAgdFn == null)
			{
				return false;
			}
			if (this.ef_pre_key != Ef.title)
			{
				this.CurFn = X.Get<string, AttackGhostDrawer.FnAgdEfDraw>(AttackGhostDrawer.OAgdFn, this.ef_pre_key = Ef.title);
			}
			if (this.CurFn == null)
			{
				X.de("AGD キーが登録されていない: " + Ef.title, null);
				return false;
			}
			return this.CurFn(Ef, this);
		}

		public static void AddGhostEfFunc(string _key, AttackGhostDrawer.FnAgdEfDraw _Fn)
		{
			if (AttackGhostDrawer.OAgdFn == null)
			{
				AttackGhostDrawer.OAgdFn = new BDic<string, AttackGhostDrawer.FnAgdEfDraw>(1);
			}
			AttackGhostDrawer.OAgdFn[_key] = _Fn;
		}

		public Color32 ColFS = C32.d2c(uint.MaxValue);

		public Color32 ColBS = C32.d2c(4294377472U);

		public Color32 ColFE = C32.d2c(14357010U);

		public Color32 ColBE = C32.d2c(6684672U);

		public static C32 Cf = new C32();

		public static C32 Cb = new C32();

		public bool use_center_col;

		public Color32 ColCS = C32.d2c(uint.MaxValue);

		public Color32 ColCE = C32.d2c(16777215U);

		public float time_center = 0.125f;

		public float bezier_agR = -2.3561945f;

		public float bezier_agR_hrhb = 0.12566371f;

		public float zd_agR;

		public float s_ux;

		public float s_uy;

		public float d_ux;

		public float d_uy;

		public float z_ux;

		public float z_uy;

		public float posS_i_b;

		public float posS_i_h;

		public float posS_rndm_b;

		public float posS_rndm_h;

		public float posD_rndm_b;

		public float posD_rndm_h;

		public float posD_rndm_zd;

		public float z_bext_min;

		public float z_bext_max;

		public int count = 5;

		public float draw_len = 2f;

		public float draw_center_lvl = 0.85f;

		public float height_min;

		public float height_max;

		public int ran0;

		public int resolution_sz = 14;

		public int resolution_zd = 14;

		public readonly FnEffectRun FD_EfDraw;

		private string ef_pre_key;

		private AttackGhostDrawer.FnAgdEfDraw CurFn;

		private static BDic<string, AttackGhostDrawer.FnAgdEfDraw> OAgdFn;

		public delegate bool FnAgdEfDraw(EffectItem Ef, AttackGhostDrawer Agd);
	}
}
