using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2Mana : IRunAndDestroy
	{
		public M2Mana(M2ManaContainer _Con)
		{
			this.Con = _Con;
			this.Stroker = new TrajectoryDrawer(8);
			this.Stroker.FD_PosConvert = delegate(Vector2 V)
			{
				M2DBase instance = M2DBase.Instance;
				if (instance != null && instance.curMap != null)
				{
					return new Vector2(instance.curMap.map2meshx(V.x), instance.curMap.map2meshy(V.y)) * instance.curMap.base_scale;
				}
				return V;
			};
		}

		public M2Mana clear(float _appearx, float _appeary, float dx, float dy, float _start_agR, MANA_HIT _mana_hit, int index)
		{
			this.appearx = _appearx;
			this.appeary = _appeary;
			this.mana_hit = _mana_hit;
			this.sx = dx;
			this.x = dx;
			this.sy = dy;
			this.y = dy;
			this.start_agR = _start_agR;
			this.start_float_t = X.NIXP(23f, 30f);
			this.Target = null;
			this.TargetM = null;
			this.faf = -1f;
			this.af = 0f;
			this.y_vib = 0f;
			this.index = index;
			this.anim_additional = X.ANMP(index, 4, 1f);
			this.v = ((index % 2 == 1) ? 1.8f : 2.2f);
			if ((this.mana_hit & MANA_HIT.PR) != MANA_HIT.NOUSE)
			{
				this.mana_hit |= MANA_HIT.OC_ADDABLE;
			}
			if ((this.mana_hit & MANA_HIT.FALL) != MANA_HIT.NOUSE)
			{
				this.v = X.NIXP(0.1f, 0.24f);
				this.sx = X.absMx(this.v * X.Cos(this.start_agR), 0.07f);
				this.sy = -this.v * X.Sin(this.start_agR) * X.NIXP(1f, 1.4f);
				this.start_float_t = 120f;
			}
			this.Stroker.Clear();
			return this;
		}

		public bool run(float fcnt)
		{
			try
			{
				bool flag = true;
				if (this.af < this.start_float_t && this.immediate_collect && this.Target != null && (this.mana_hit & MANA_HIT.FALL) == MANA_HIT.NOUSE)
				{
					this.af = this.start_float_t;
				}
				if (this.af < this.start_float_t)
				{
					if ((this.mana_hit & MANA_HIT.FALL) != MANA_HIT.NOUSE)
					{
						this.af = X.Mn(this.af, 60f);
						if (this.Mp.simulateDropItem(ref this.x, ref this.y, ref this.sx, ref this.sy, 0.3f, 1f, 0.67f, fcnt, 0f, false, true) != 0 || (this.immediate_collect && this.af >= (float)(14 + this.index % 13) && this.Target != null) || (!this.only_effect && this.af >= 52f))
						{
							if ((this.mana_hit & MANA_HIT.ALL) == MANA_HIT.NOUSE)
							{
								this.destruct();
								return false;
							}
							if ((this.mana_hit & MANA_HIT.FROM_DAMAGE_SPLIT) != MANA_HIT.NOUSE)
							{
								this.start_float_t = X.NIXP(12f, 18f);
							}
							else
							{
								this.start_float_t = X.NIXP(44f, 55f);
							}
							if (!this.immediate_collect)
							{
								this.mana_hit &= (MANA_HIT)(-57);
							}
							else
							{
								this.af = this.start_float_t;
							}
							this.start_agR = X.GAR(0f, 0f, this.sx * 4f, X.Abs(this.sy));
							this.sx = this.x;
							this.sy = this.y;
							this.v = X.NIXP(0.3f, 0.7f);
						}
					}
					else
					{
						float num = X.ZSIN(this.af, this.start_float_t);
						this.x = this.sx + num * X.Cos(this.start_agR) * this.v * 0.45f;
						this.y = this.sy - num * X.Sin(this.start_agR) * this.v;
					}
				}
				else
				{
					if (this.immediate_collect && this.Target == null)
					{
						this.mana_hit &= (MANA_HIT)(-1801);
					}
					if (this.Target != null)
					{
						if (this.TargetM.destructed || (this.TargetM.get_mp() >= this.TargetM.get_maxmp() && !this.immediate_collect) || (!this.TargetM.is_alive && !this.TargetM.overkill))
						{
							this.Con.fineRecheckTarget(10f);
							this.mana_hit &= (MANA_HIT)(-129);
							this.Target = null;
							this.TargetM = null;
							this.mana_hit &= (MANA_HIT)(-1849);
							this.faf = -1f;
						}
						else if (this.faf < 0f)
						{
							this.faf += fcnt;
							if (this.faf >= 0f)
							{
								this.sx = 1f;
								this.v = (this.faf = 0f);
								this.start_agR = this.Mp.GAR(this.x, this.y, this.TargetM.x, this.TargetM.y);
								this.Stroker.Clear();
								this.Stroker.Add(new Vector2(this.x, this.y));
								this.Mp.playSnd("mana_targetting", this.snd_key, this.x, this.y, 1);
							}
						}
						else
						{
							flag = false;
							float num2 = this.Mp.GAR(this.x, this.y, this.TargetM.x, this.TargetM.y);
							this.start_agR = X.VALWALKANGLER(this.start_agR, num2, (0.03f + X.ZLINE(this.faf - 10f, 40f) * 0.2f) * 6.2831855f * fcnt);
							this.v = X.VALWALK(this.v, 0.5f, X.NI(0.045f, 0.023f, X.ZLINE(this.desire)) * fcnt);
							this.x += X.Cos(this.start_agR) * this.v * fcnt;
							this.y -= X.Sin(this.start_agR) * this.v * fcnt;
							this.faf += fcnt;
							if ((float)((int)this.faf / 4 + 1) >= this.sx)
							{
								this.sx += 1f;
								this.Stroker.Add(new Vector2(this.x, this.y));
							}
							this.y_vib = X.VALWALK(this.y_vib, 0f, 0.01f);
						}
					}
					else
					{
						this.Con.fineRecheckTarget((float)(((this.mana_hit & MANA_HIT.PR) != MANA_HIT.NOUSE) ? 14 : 50));
					}
				}
				if (((this.mana_hit & MANA_HIT.FALL) == MANA_HIT.NOUSE || (this.af >= this.start_float_t && this.immediate_collect) || (this.mana_hit & MANA_HIT.EN) == MANA_HIT.NOUSE) && this.TargetM != null && !this.TargetM.destructed)
				{
					if (this.Target.canGetMana(this, true))
					{
						this.Target.addMpFromMana(this, 4f);
						this.destruct();
						return false;
					}
					if (!this.Target.canGetMana(this, false) && (this.mana_hit & MANA_HIT.EN) != MANA_HIT.NOUSE && (this.mana_hit & MANA_HIT.NO_FARE\uFF3FHIT) == MANA_HIT.NOUSE && this.af % 4f == (float)(this.index % 4))
					{
						int mover_count = this.Mp.mover_count;
						for (int i = 0; i < mover_count; i++)
						{
							NelM2Attacker nelM2Attacker = this.Mp.getMv(i) as NelM2Attacker;
							if (nelM2Attacker != null && nelM2Attacker != this.Target && nelM2Attacker.canGetMana(this, false))
							{
								this.Target = nelM2Attacker;
								this.TargetM = nelM2Attacker as M2Attackable;
								this.mana_hit |= MANA_HIT.NO_FARE\uFF3FHIT;
								break;
							}
						}
					}
				}
				int num3 = (((this.mana_hit & MANA_HIT.FROM_GAGE_SPLIT) != MANA_HIT.NOUSE) ? 300 : (((this.mana_hit & MANA_HIT.PR) == MANA_HIT.NOUSE && this.Con.can_collect_en_mana_immediately == 1) ? 30 : 300));
				if (this.af >= (float)num3 && this.Target == null)
				{
					if ((this.mana_hit & MANA_HIT.ALL) == MANA_HIT.ALL || (this.mana_hit & MANA_HIT.ALL) == MANA_HIT.NOUSE || (this.mana_hit & (MANA_HIT)10244) != MANA_HIT.NOUSE)
					{
						this.destruct();
						return false;
					}
					this.mana_hit |= MANA_HIT.ALL;
					this.mana_hit &= (MANA_HIT)(-185);
					this.af = this.start_float_t + 10f;
					this.Con.fineRecheckTarget(10f);
				}
				if (flag)
				{
					this.y_vib = -X.SINI(this.af, 180f) * 0.23f;
				}
				this.af += fcnt;
			}
			catch
			{
				return false;
			}
			return true;
		}

		public void draw(MeshDrawer MdS, MeshDrawer MdA, MeshDrawer MdStroke, float fcnt)
		{
			C32 c = EffectItem.Col1.Set(3432935825U);
			MdS.Col = c.C;
			if ((this.mana_hit & MANA_HIT.FALL) != MANA_HIT.NOUSE && (this.mana_hit & MANA_HIT.CRYSTAL) == MANA_HIT.NOUSE)
			{
				MdS.Col.a = byte.MaxValue;
				uint ran = X.GETRAN2(this.index, this.index % 7);
				float num = X.NI(4, 19, X.ZPOW(X.RAN(ran, 2103))) * (((this.mana_hit & MANA_HIT.ALL) == MANA_HIT.NOUSE) ? X.NI(0.25f, 0.5f, X.RAN(ran, 1667)) : 1f) * (0.7f + 0.25f * X.COSIT(3.7f + X.RAN(ran, 1630) * 2.2f));
				MdS.RotaGraph(0f, 0f, num * 1.5f * 0.055555556f, 0f, null, false);
				MdS.ColGrd.Set(MdS.Col).setA1(0f);
				float num2 = X.ZSIN(this.af - 20f, 40f);
				MdA.initForImg(MTRX.EffCircle128, 0);
				MdA.Col = c.Set(MTR.col_pr_mana).blend(MTR.col_en_mana, ((this.mana_hit & (MANA_HIT)34) == MANA_HIT.NOUSE) ? (1f - num2) : num2).C;
				MdA.Rect(0f, 0f, num * 2f, num * 2f, false);
				MdA.initForImg(MTRX.EffBlurCircle245, 0);
				float num3 = num * 2f + 8f;
				MdA.Rect(0f, 0f, num3 * 2f, num3 * 2f, false);
				return;
			}
			MdS.RotaGraph(0f, 0f, ((float)((this.faf >= 0f) ? 25 : 20) + (0.7f + 0.3f * X.Cos0(this.Mp.floort / 11.7f + this.anim_additional))) * 0.055555556f, 0f, null, false);
			bool flag;
			bool flag2;
			if (this.faf >= 0f && X.ANMT(2, 2f) == 1)
			{
				flag = this.pr_target;
				flag2 = this.en_target;
			}
			else
			{
				flag = this.pr_hit;
				flag2 = this.en_hit;
			}
			float anmp = this.Con.anmp;
			if (this.special_attract)
			{
				MdA.Col = C32.d2c(4292999091U);
			}
			else if (flag && flag2)
			{
				MdA.Col = C32.d2c(2863264710U);
			}
			else if (flag)
			{
				MdA.Col = C32.d2c(4288401407U);
			}
			else if (flag2)
			{
				if (anmp < 0.5f && (this.from_player_damage || this.from_player_gage))
				{
					MdA.Col = C32.d2c(2296645702U);
				}
				else
				{
					MdA.Col = C32.d2c(3454273606U);
				}
			}
			else
			{
				MdA.Col = C32.d2c(2575399542U);
			}
			MdA.initForImg(MTR.SqEfManaLight.getImage((int)(4f * (anmp + this.anim_additional)) % 4, 0), 0).DrawCen(0f, 0f, null);
			if (this.faf >= 0f)
			{
				PxlSequence aefManaTargetting = MTR.AEfManaTargetting;
				MdA.RotaPF(0f, 0f, 1f, 1f, 0f, aefManaTargetting[X.ANM((int)this.faf, aefManaTargetting.Length, 4f)], false, false, false, uint.MaxValue, false, 0);
				this.Stroker.BlurLineWTo(MdStroke, -MdStroke.base_x * 64f, -MdStroke.base_y * 64f, 1f, 12f, 20f, MdA.Col, C32.MulA(MdA.Col, 0f), 0.5f);
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Con.Mp;
			}
		}

		public bool isActive()
		{
			return this.af >= 0f;
		}

		public void setTarget(NelM2Attacker Targ, int set_index, float desire)
		{
			this.faf = (float)(-1 - set_index * 5);
			this.desire = desire / 9f;
			this.Target = Targ;
			this.TargetM = Targ as M2Attackable;
			this.mana_hit &= (MANA_HIT)(-769);
			if (Targ is PR)
			{
				this.mana_hit |= MANA_HIT.TARGET_PR;
				return;
			}
			if (Targ is NelEnemy)
			{
				this.mana_hit |= MANA_HIT.TARGET_EN;
			}
		}

		public void transformManaHitType(MANA_HIT _mana_hit)
		{
			this.mana_hit = _mana_hit | (this.mana_hit & MANA_HIT.FALL);
			this.mana_hit &= (MANA_HIT)(-769);
			this.af = X.Mn(this.start_float_t + 10f, this.af);
			this.Target = null;
			this.TargetM = null;
		}

		public NelM2Attacker getTarget()
		{
			return this.Target;
		}

		public bool pr_hit
		{
			get
			{
				return (this.mana_hit & MANA_HIT.PR) > MANA_HIT.NOUSE;
			}
		}

		public bool en_hit
		{
			get
			{
				return (this.mana_hit & MANA_HIT.EN) > MANA_HIT.NOUSE;
			}
		}

		public bool pr_target
		{
			get
			{
				return (this.mana_hit & MANA_HIT.TARGET_PR) > MANA_HIT.NOUSE;
			}
		}

		public bool en_target
		{
			get
			{
				return (this.mana_hit & MANA_HIT.TARGET_EN) > MANA_HIT.NOUSE;
			}
		}

		public bool special_attract
		{
			get
			{
				return (this.mana_hit & MANA_HIT.SPECIAL) > MANA_HIT.NOUSE;
			}
		}

		public bool immediate_collect
		{
			get
			{
				return (this.mana_hit & MANA_HIT.IMMEDIATE_COLLECTABLE) > MANA_HIT.NOUSE;
			}
		}

		public bool from_player_damage
		{
			get
			{
				return (this.mana_hit & MANA_HIT.FROM_DAMAGE_SPLIT) > MANA_HIT.NOUSE && this.en_hit && !this.pr_hit;
			}
		}

		public bool from_damage
		{
			get
			{
				return (this.mana_hit & MANA_HIT.FROM_DAMAGE_SPLIT) > MANA_HIT.NOUSE;
			}
		}

		public bool from_player_gage
		{
			get
			{
				return (this.mana_hit & MANA_HIT.FROM_GAGE_SPLIT) > MANA_HIT.NOUSE;
			}
		}

		public bool from_supplier
		{
			get
			{
				return (this.mana_hit & MANA_HIT.FROM_SUPPLIER) > MANA_HIT.NOUSE;
			}
		}

		public bool from_enemy_supplier
		{
			get
			{
				return (this.mana_hit & MANA_HIT.FROM_SUPPLIER) > MANA_HIT.NOUSE && this.en_hit && !this.pr_hit;
			}
		}

		public bool only_effect
		{
			get
			{
				return (this.mana_hit & MANA_HIT.FALL) > MANA_HIT.NOUSE && (this.mana_hit & MANA_HIT.ALL) == MANA_HIT.NOUSE;
			}
		}

		public string snd_key
		{
			get
			{
				return "__nel_mana";
			}
		}

		public void destruct()
		{
			this.Target = null;
			this.mana_hit &= (MANA_HIT)(-129);
			this.TargetM = null;
			this.TargetM = null;
			this.af = -1f;
		}

		public readonly M2ManaContainer Con;

		public MANA_HIT mana_hit;

		public float x;

		public float y;

		public float y_vib;

		public float sx;

		public float sy;

		public float start_agR;

		public float af;

		public float v = 1f;

		public float faf;

		public float desire;

		private int index;

		private float start_float_t;

		private float anim_additional;

		public NelM2Attacker Target;

		public M2Attackable TargetM;

		public float appearx;

		public float appeary;

		private TrajectoryDrawer Stroker;

		private const int STROKE_MAX = 8;

		private const float catch_len = 0.25f;

		private const float ball_div = 0.055555556f;
	}
}
