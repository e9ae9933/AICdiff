using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2DmgCounterItem : IRunAndDestroy
	{
		public M2DmgCounterItem(M2DmgCounterContainer _Con)
		{
			this.Con = _Con;
		}

		public bool addAnotherCache(M2Attackable _Mv)
		{
			return _Mv == this.MvAnother;
		}

		public float map_center_y_shift_checking
		{
			get
			{
				int num = this.countLines(true);
				return (float)(-14 * this.max_scale * num) * 0.5f * this.Mp.rCLENB;
			}
		}

		public bool isCovering(float cx, float cy, M2DmgCounterItem CI, float scaler)
		{
			float num = -this.map_center_y_shift_checking * scaler;
			float num2 = ((this.f0_move > 0) ? this.dx : this.x) + this.Mv.drawx_map;
			float num3 = ((this.f0_move > 0) ? this.dy : this.y) + this.Mv.drawy_map - num;
			num *= 0.75f;
			return X.isCovering(num2, num2, cx, cx, scaler * 0.35f) && X.isCovering(num3 - num, num3 + num, cy, cy, scaler * 0.0625f);
		}

		public void SetPos(float _x, float _y)
		{
			this.x = _x - this.Mv.drawx_map;
			this.y = _y - this.Mv.drawy_map;
		}

		public M2DmgCounterItem FineMv()
		{
			this.is_pr = this.Mv is M2MoverPr;
			this.f0 = IN.totalframe;
			if (this.MvAnother != null)
			{
				this.et |= M2DmgCounterItem.DC.ABSORBED;
			}
			return this;
		}

		public int countLines(bool no_consier_string = false)
		{
			return X.Mx(1, ((this.hp != 0) ? 1 : 0) + ((this.mp != 0) ? 1 : 0)) + (no_consier_string ? 0 : ((this.is_critical ? 1 : 0) + (this.is_guard ? 1 : 0)));
		}

		public int countLinesFlipped(bool no_consier_string = false)
		{
			return X.Mx(1, ((this.hp != 0) ? 1 : 0) + ((this.mp != 0) ? 1 : 0));
		}

		public int countLinesA(bool no_consier_string = false)
		{
			return X.Mx(1, ((this.hpa != 0) ? 1 : 0) + ((this.mpa != 0) ? 1 : 0)) + (no_consier_string ? 0 : ((this.is_critical ? 1 : 0) + (this.is_guard ? 1 : 0)));
		}

		public int base_maxt
		{
			get
			{
				return (int)((float)(this.is_pr ? 90 : 75) * ((this.MvAnother != null) ? 0.66f : 1f));
			}
		}

		public int maxt
		{
			get
			{
				return this.base_maxt + this.countLines(false) * 18 + (this.is_absorb_flipped ? 18 : 0);
			}
		}

		public bool run(float fcnt)
		{
			if (IN.totalframe - this.f0 >= this.maxt)
			{
				if (this.MvAnother == null || (this.hpa == 0 && this.mpa == 0))
				{
					return false;
				}
				int num = this.hp;
				this.et |= M2DmgCounterItem.DC.ABSORB_FLIPPED;
				this.hp = this.hpa;
				this.hpa = num;
				num = this.mp;
				this.mp = this.mpa;
				this.mpa = num;
				this.sx = this.Mv.drawx_map + this.x;
				this.sy = this.Mv.drawy_map + this.y;
				this.Mv = this.MvAnother;
				this.MvAnother = null;
				this.FineMv();
				Vector2 damageCounterShiftMapPos = this.Mv.getDamageCounterShiftMapPos();
				Vector2 vector = this.Con.reposit(this, this.Mv.drawx_map + damageCounterShiftMapPos.x, this.Mv.drawy_map + damageCounterShiftMapPos.y, false);
				this.f0_move = IN.totalframe;
				this.dx = vector.x - this.Mv.drawx_map;
				this.dy = vector.y - this.Mv.drawy_map;
				this.sx = (this.x = this.sx - this.Mv.drawx_map);
				this.sy = (this.y = this.sy - this.Mv.drawy_map);
			}
			if (this.f0_move > 0)
			{
				int num2 = IN.totalframe - this.f0_move;
				if (num2 >= 36)
				{
					this.x = this.dx;
					this.y = this.dy;
					this.f0_move = 0;
				}
				else
				{
					float num3 = X.ZSIN((float)num2, 36f);
					this.x = X.NI(this.sx, this.dx, num3);
					this.y = X.NI(this.sy, this.dy, num3);
				}
			}
			return true;
		}

		public void draw(EffectItem Ef, M2DrawBinder Ed, ref MeshDrawer MdN, ref MeshDrawer MdB, ref int anm_a, bool on_ui = false)
		{
			this.draw(Ef, Ed, ref MdN, ref MdB, ref anm_a, this.Mv.drawx_map, this.Mv.drawy_map, this.x, this.y, 1f, -1000f, on_ui);
		}

		public void draw(EffectItem Ef, M2DrawBinder Ed, ref MeshDrawer MdN, ref MeshDrawer MdB, ref int anm_a, float mvx, float mvy, float difx, float dify, float pos_scale = 1f, float base_z = -1000f, bool on_ui = false)
		{
			float num2;
			float num;
			if (on_ui)
			{
				num = (num2 = 1f);
			}
			else
			{
				num2 = this.Mp.M2D.Cam.getScaleRev();
				num = this.Mp.M2D.Cam.getScale(true);
				difx *= num2;
				dify *= num2;
			}
			float num3 = X.NI(X.Mx(0.8f, num2), 1f, 0.5f);
			float num4 = difx + mvx;
			float num5 = dify + mvy;
			int num6 = IN.totalframe - this.f0;
			int maxt = this.maxt;
			bool is_absorbed = this.is_absorbed;
			if (is_absorbed)
			{
				float num7 = 0.5f;
				float num8;
				if (this.MvAnother != null)
				{
					num8 = X.ZSIN((float)num6, 24f);
				}
				else
				{
					num8 = X.ZSINV((float)(maxt - num6), 40f);
					num7 = 0.375f;
				}
				num4 = X.NI(mvx, num4, num7 + num8 * (1f - num7));
				num5 = X.NI(mvy, num5, num7 + num8 * (1f - num7));
			}
			float num9;
			float num10;
			if (!on_ui)
			{
				num4 = (num4 - mvx) * num3;
				num5 = (num5 - mvy) * num3;
				num4 += mvx;
				num5 += mvy;
				if (Ed != null && !Ed.isinCamera(num4, num5, 3f, 6f))
				{
					return;
				}
				num9 = this.Mp.ux2effectScreenx(this.Mp.map2ux(num4));
				num10 = this.Mp.uy2effectScreeny(this.Mp.map2uy(num5));
			}
			else
			{
				num9 = (num4 - mvx) * this.Mp.CLENB * 0.015625f + mvx;
				num10 = (num5 - mvy) * this.Mp.CLENB * 0.015625f + mvy;
			}
			int num11;
			int num12;
			M2DmgCounterItem.DC dc;
			bool flag;
			float num13;
			int num14;
			bool flag2;
			int i;
			if (this.is_absorb_flipped && num6 < 18)
			{
				num11 = this.hpa;
				num12 = this.mpa;
				dc = this.et;
				flag = !this.is_pr;
				num13 = 1f - X.ZSINV((float)num6, 18f);
				num14 = maxt;
				flag2 = false;
				i = this.countLinesA(false) - 1;
			}
			else
			{
				num11 = this.hp;
				num12 = this.mp;
				flag = this.is_pr;
				if (this.is_absorb_flipped)
				{
					num14 = num6 - 18;
					num13 = X.ZSIN((float)num14, 18f);
					dc = M2DmgCounterItem.DC.NORMAL;
					flag2 = true;
					i = this.countLinesFlipped(false) - 1;
				}
				else
				{
					num14 = num6;
					dc = this.et;
					num13 = 1f;
					flag2 = !is_absorbed || (this.hpa == 0 && this.mpa == 0);
					i = this.countLines(false) - 1;
				}
			}
			bool flag3 = (dc & M2DmgCounterItem.DC.DULL) > M2DmgCounterItem.DC.NORMAL;
			float num15;
			if (flag)
			{
				num15 = X.NIL(3f, 4f, num - 1f, 1f) * num2;
			}
			else
			{
				num15 = 2f * num2;
			}
			bool flag4 = false;
			bool flag5 = false;
			float num16 = 14f * num15;
			float num17 = -0.5f * num16 + (is_absorbed ? 0f : (80f * X.ZLINE((float)num6, (float)maxt) - num16 * (float)(i + 1)));
			C32 cola = MTRX.cola;
			STB stb = TX.PopBld(null, 0);
			while (i >= 0)
			{
				stb.Clear();
				int num18 = 0;
				bool flag6 = false;
				bool flag7 = false;
				float num19 = num15;
				float num20 = num15;
				float num21 = 0f;
				int num22 = num14 - i * 18;
				BMListChars bmlistChars;
				if ((dc & M2DmgCounterItem.DC.GUARD) != M2DmgCounterItem.DC.NORMAL)
				{
					dc &= (M2DmgCounterItem.DC)(-5);
					bmlistChars = MTRX.ChrM;
					stb.Set("GUARD");
					if (flag)
					{
						num19 -= 1f;
						num20 -= 1f;
					}
					flag6 = true;
				}
				else if ((dc & M2DmgCounterItem.DC.CRITICAL) != M2DmgCounterItem.DC.NORMAL)
				{
					dc &= (M2DmgCounterItem.DC)(-2);
					flag4 = true;
					flag5 = X.ANM(num6, 2, 6f) == 1;
					stb.Set("Weak!");
					bmlistChars = MTRX.ChrM;
					flag6 = true;
				}
				else if (num12 != 0)
				{
					bmlistChars = MTRX.ChrL;
					num18 = num12;
					if (num12 > 0)
					{
						stb.Add("+").Add(num12).Add("p");
					}
					else
					{
						stb.Add(num12).Add("p");
					}
					num12 = 0;
					flag7 = true;
				}
				else
				{
					bmlistChars = (((dc & M2DmgCounterItem.DC.REDUCED) != M2DmgCounterItem.DC.NORMAL && num22 >= 8) ? MTRX.ChrLb : MTRX.ChrL);
					num18 = num11;
					if (num11 > 0)
					{
						stb.Add("+").Add(num11);
					}
					else
					{
						stb.Add(num11);
					}
					num11 = 0;
				}
				i--;
				if (!is_absorbed)
				{
					num17 += num16 * 1f;
				}
				if (num22 >= 0)
				{
					float num23 = X.ZSIN((float)num22, 16f);
					if (is_absorbed)
					{
						num17 += num16 * num23;
					}
					float num24 = (flag2 ? X.ZLINE((float)(this.base_maxt - num22), 25f) : 1f);
					if (num24 > 0f)
					{
						MeshDrawer meshDrawer;
						if (flag6)
						{
							if (MdB == null)
							{
								MdB = Ef.GetMesh("dmg_counter", this.Con.MtrCharBorder, false);
							}
							meshDrawer = MdB;
							if (base_z != -1000f)
							{
								meshDrawer.base_z = base_z;
							}
						}
						else
						{
							if (MdN == null)
							{
								MdN = Ef.GetMesh("dmg_counter", this.Con.MtrChar, false);
							}
							meshDrawer = MdN;
							if (base_z != -1000f)
							{
								meshDrawer.base_z = base_z - 0.001f;
							}
						}
						meshDrawer.base_x = num9;
						meshDrawer.base_y = num10;
						C32 colGrd = meshDrawer.ColGrd;
						if (flag)
						{
							if (anm_a == -1)
							{
								anm_a = X.ANMT(2, 8f);
							}
							if (flag7)
							{
								if (num18 <= 0)
								{
									colGrd.Set((anm_a == 0) ? 4292234426U : 4291133880U);
									if (flag5)
									{
										colGrd.blend(4285886595U, 0.75f);
									}
									if (flag6)
									{
										cola.Set(4286963094U);
									}
								}
								else
								{
									colGrd.Set((anm_a == 0) ? 4288610253U : 4286111914U);
									if (flag6)
									{
										cola.Set(3137374912U);
									}
								}
							}
							else if (num18 == 0 && !flag6)
							{
								colGrd.White();
								if (flag6)
								{
									cola.Set(4286089873U);
								}
							}
							else if (num18 <= 0)
							{
								if (stb.Equals("GUARD"))
								{
									colGrd.Set(4291281334U);
								}
								else
								{
									colGrd.Set((anm_a == 0) ? 4294924890U : 4291369241U);
								}
								if (flag5)
								{
									colGrd.blend(4283903791U, 0.75f);
								}
								if (flag6)
								{
									cola.Set(4278190080U);
								}
							}
							else
							{
								colGrd.Set((anm_a == 0) ? 4286488563U : 4287672048U);
								if (flag6)
								{
									cola.Set(3144714492U);
								}
							}
						}
						else if (flag7)
						{
							if (num18 <= 0)
							{
								colGrd.Set(4292261064U);
								if (flag6)
								{
									cola.Set(3147772596U);
								}
							}
							else
							{
								colGrd.Set(4288731312U);
								if (flag6)
								{
									cola.Set(3150936229U);
								}
							}
						}
						else if (num18 <= 0)
						{
							colGrd.Set(uint.MaxValue);
							if (flag6)
							{
								cola.Set(3137339392U);
							}
						}
						else
						{
							colGrd.Set(4286827007U);
							if (flag6)
							{
								cola.Set(3152157620U);
							}
						}
						if (flag4)
						{
							num19 += 2.5f * (X.ZSIN2((float)num22, 7f) - X.ZCOS((float)(num22 - 6), 18f));
							num20 *= X.ZSIN2((float)num22, 15f);
						}
						else if (num18 <= 0 && !flag3)
						{
							float num25 = 2.5f * (1f - num23);
							num19 += num25;
							num20 += num25;
						}
						if (flag3)
						{
							colGrd.blend(4286151033U, 0.6f);
							if (flag6)
							{
								cola.Set(4280625483U);
							}
							num21 = num19 * 18f * (1f - X.ZLINE((float)(num22 - 15), 35f)) * X.SINI((float)num22, 15f);
							float num26 = X.ZLINE((float)(num22 + 7), 20f);
							num24 *= num26;
							num20 *= num26;
							num19 *= num26;
						}
						if (flag6)
						{
							meshDrawer.Uv23(cola.mulA(num24).C, true);
						}
						meshDrawer.Col = colGrd.mulA(num24).C;
						bmlistChars.DrawScaleStringTo(meshDrawer, stb, num21 * pos_scale, num17 * pos_scale, num19 * num13 * pos_scale, num20 * pos_scale, ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
						if (flag6)
						{
							meshDrawer.allocUv23(0, true);
						}
					}
				}
			}
			TX.ReleaseBld(stb);
		}

		public void addHpa(ref int delta_hp)
		{
			this.addHpa(this.hp, ref this.hpa, ref delta_hp);
		}

		public void addMpa(ref int delta_mp)
		{
			this.addHpa(this.mp, ref this.mpa, ref delta_mp);
		}

		private void addHpa(int hp, ref int hpa, ref int delta_hp)
		{
			if (hp * delta_hp >= 0)
			{
				return;
			}
			int num = ((delta_hp >= 0) ? X.Mn(delta_hp, -hp - hpa) : X.Mx(-hp - hpa, delta_hp));
			hpa += num;
			delta_hp -= num;
		}

		public Map2d Mp
		{
			get
			{
				return this.Con.Mp;
			}
		}

		public bool is_critical
		{
			get
			{
				return (this.et & M2DmgCounterItem.DC.CRITICAL) > M2DmgCounterItem.DC.NORMAL;
			}
		}

		public bool is_guard
		{
			get
			{
				return (this.et & M2DmgCounterItem.DC.GUARD) > M2DmgCounterItem.DC.NORMAL;
			}
		}

		public bool is_absorbed
		{
			get
			{
				return (this.et & M2DmgCounterItem.DC.ABSORBED) > M2DmgCounterItem.DC.NORMAL;
			}
		}

		public bool is_absorb_flipped
		{
			get
			{
				return (this.et & M2DmgCounterItem.DC.ABSORB_FLIPPED) > M2DmgCounterItem.DC.NORMAL;
			}
		}

		public int max_scale
		{
			get
			{
				if ((this.et & (M2DmgCounterItem.DC)12288) != M2DmgCounterItem.DC.NORMAL)
				{
					return 3;
				}
				if (!this.is_pr)
				{
					return 2;
				}
				return 3;
			}
		}

		public bool reserved_absorb
		{
			get
			{
				return this.MvAnother != null;
			}
		}

		public void destruct()
		{
		}

		public readonly M2DmgCounterContainer Con;

		public float x;

		public float y;

		private float dx;

		private float dy;

		private float sx;

		private float sy;

		public M2DmgCounterItem.DC et;

		public int hp;

		public int mp;

		public int hpa;

		public int mpa;

		public bool is_pr;

		public int f0;

		public int f0_move;

		public M2Attackable Mv;

		public M2Attackable MvAnother;

		private const int line_intv_t = 18;

		private const int line_intv_y_px = 14;

		private const int normal_scroll_y_px_scaled = 80;

		private const int move_maxt = 36;

		private const int maxt_flip = 18;

		public enum DC
		{
			NORMAL,
			CRITICAL,
			DULL,
			GUARD = 4,
			REDUCED = 8,
			ABSORBED = 4096,
			ABSORB_FLIPPED = 8192
		}
	}
}
