using System;
using m2d;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public sealed class PrGaugeSaver : M2PrAssistant
	{
		public PrGaugeSaver(PR Pr)
			: base(Pr)
		{
			this.AGs = new PrGaugeSaver.GsItem[2];
			this.AGs[0] = new PrGaugeSaver.GsItem(this, true);
			this.AGs[1] = new PrGaugeSaver.GsItem(this, false);
		}

		public override void newGame()
		{
			base.newGame();
			int num = 2;
			this.t_hp_damage_reduce = 0f;
			for (int i = 0; i < num; i++)
			{
				this.AGs[i].newGame();
			}
		}

		public int applyHpDamage(int val, NelAttackInfoBase Atk, out M2DmgCounterItem.DC dmgcounter_v, out int reduced)
		{
			dmgcounter_v = M2DmgCounterItem.DC.NORMAL;
			reduced = 0;
			if (val <= 0)
			{
				return val;
			}
			PrGaugeSaver.GsItem gsHp = this.GsHp;
			int num = val;
			float num2 = 0f;
			bool flag = false;
			float num3;
			float num4;
			float num5;
			if (this.Pr.is_alive && DIFF.reduceApplyingHp(this.Pr, Atk, ref this.t_hp_damage_reduce, ref val, out num3, out num4, out num5))
			{
				gsHp.LockTimeAdd(num3, num4);
				num2 = global::XX.X.Mx(0f, (float)(num - val) * num5);
				flag = num2 > 0f;
				dmgcounter_v |= M2DmgCounterItem.DC.REDUCED;
			}
			float num6 = (float)((int)((float)val - this.Pr.get_hp() + (float)(this.Pr.is_alive ? 1 : 0)));
			if (num6 > 0f)
			{
				flag = true;
				float hp1_secure_damage_ratio = DIFF.hp1_secure_damage_ratio;
				num6 = global::XX.X.Mx(0f, num6 * hp1_secure_damage_ratio);
				num6 = global::XX.X.Mn(gsHp.saved_gauge_value - num2, num6);
				val = global::XX.X.Mx(0, val - global::XX.X.IntR(num6 / hp1_secure_damage_ratio));
				if (val <= 0 && this.Pr.is_alive)
				{
					dmgcounter_v |= M2DmgCounterItem.DC.REDUCED;
				}
				num6 += num2;
			}
			else
			{
				num6 = (float)val - (float)val * DIFF.recover_hp_gsave(Atk) + num2;
			}
			if (num6 > 0f)
			{
				reduced = global::XX.X.IntC(num6);
				gsHp.Reduce(num6, flag).LockTime(DIFF.lock_hp_gsaver_time(this.Pr, Atk, global::XX.X.Mx(0, (int)this.Pr.get_hp() - val)), true);
			}
			return val;
		}

		public void addSavedHp(float v, bool fine_flag = false)
		{
			if (v <= 0f)
			{
				return;
			}
			this.GsHp.Add(v);
			if (fine_flag)
			{
				this.GsMp.Fine(true);
				return;
			}
			this.GsHp.fineMinusDelay();
		}

		public void addSavedMp(float v, bool fine_flag = false)
		{
			if (this.Pr.isPuzzleManagingMp() || v <= 0f)
			{
				return;
			}
			this.GsMp.Add(v);
			if (fine_flag)
			{
				this.GsMp.Fine(true);
				return;
			}
			this.GsMp.fineMinusDelay();
		}

		public void reduceMp(int val, bool lock_time = false, bool touch_ui = false)
		{
			if (this.Pr.isPuzzleManagingMp() || val <= 0)
			{
				return;
			}
			this.GsMp.Reduce((float)val, touch_ui);
			if (lock_time)
			{
				this.GsMp.LockTime();
			}
		}

		public void applyMpDamage(float mpval, AttackInfo Atk, ref float gauge_break, float pre_mphold = -1f, bool use_quake = false)
		{
			if (mpval <= 0f || this.Pr.isPuzzleManagingMp())
			{
				return;
			}
			if (pre_mphold > 0f)
			{
				mpval -= global::XX.X.Mx(1f, DIFF.recover_holded_mp_gsave(this.Pr, Atk) * pre_mphold);
			}
			else
			{
				mpval -= global::XX.X.Mx(1f, DIFF.recover_mp_gsave(this.Pr, Atk) * mpval);
			}
			this.GsMp.Reduce(mpval, use_quake).LockTime(DIFF.lock_mp_gsaver_time(this.Pr, Atk), true);
		}

		public void LockTime(AttackInfo Atk)
		{
			this.GsHp.LockTime(DIFF.lock_hp_gsaver_time(this.Pr, Atk, -1000), true);
			this.GsHp.LockTime(DIFF.lock_mp_gsaver_time(this.Pr, Atk), true);
		}

		public void FineAll(bool fine_bottom = true)
		{
			for (int i = 0; i < 2; i++)
			{
				this.AGs[i].Fine(fine_bottom);
			}
		}

		public void run(float fcnt)
		{
			if (this.t_hp_damage_reduce > 0f)
			{
				this.t_hp_damage_reduce = global::XX.X.Mx(0f, this.t_hp_damage_reduce - (this.Pr.isGSHpDamageSlowDown() ? 0.25f : fcnt));
			}
			if (!base.is_alive || this.Pr.isTrappedState() || fcnt <= 0f)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				this.AGs[i].run(fcnt);
			}
		}

		public void fineMinusDelay()
		{
			for (int i = 0; i < 2; i++)
			{
				this.AGs[i].fineMinusDelay();
			}
		}

		public void removeHeadDelay()
		{
			for (int i = 0; i < 2; i++)
			{
				this.AGs[i].removeHeadDelay();
			}
		}

		public void penetrateHpDamageReduce()
		{
			this.t_hp_damage_reduce = 0f;
		}

		public void readBinaryFrom(ByteArray Ba)
		{
			int num = Ba.readByte();
			for (int i = 0; i < 2; i++)
			{
				this.AGs[i].readBinaryFrom(Ba, num);
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(0);
			for (int i = 0; i < 2; i++)
			{
				this.AGs[i].writeBinaryTo(Ba);
			}
		}

		public PrGaugeSaver.GsItem GsHp
		{
			get
			{
				return this.AGs[0];
			}
		}

		public PrGaugeSaver.GsItem GsMp
		{
			get
			{
				return this.AGs[1];
			}
		}

		public bool hp_damage_reduce_active
		{
			get
			{
				return this.t_hp_damage_reduce > 0f;
			}
		}

		public float shp
		{
			get
			{
				return this.GsHp.saved_gauge_value;
			}
		}

		public float smp
		{
			get
			{
				return this.GsMp.saved_gauge_value;
			}
		}

		public readonly PrGaugeSaver.GsItem[] AGs;

		public float t_hp_damage_reduce;

		public const float MAXT_HP_DAMAGE_REDUCE = 60f;

		public class GsItem
		{
			public GsItem(PrGaugeSaver _Con, bool _is_hp)
			{
				this.Con = _Con;
				this.is_hp = _is_hp;
				this.newGame();
			}

			public PrGaugeSaver.GsItem newGame()
			{
				this.sval = 0f;
				this.t_lock = 0f;
				this.frame_first = false;
				return this;
			}

			public PrGaugeSaver.GsItem Fine(bool fine_bottom = true)
			{
				if (fine_bottom)
				{
					this.sval = global::XX.X.Mx(this.sval, this.is_hp ? this.Pr.get_hp() : this.Pr.get_mp());
				}
				this.sval = global::XX.X.Mx(0f, global::XX.X.Mn(this.sval, this.is_hp ? this.Pr.get_maxhp() : (this.Pr.get_maxmp() - (float)this.Pr.EggCon.total)));
				return this.fineMinusDelay();
			}

			public PrGaugeSaver.GsItem Reduce(float val, bool touch_ui = true)
			{
				this.sval = global::XX.X.Mx(this.sval - global::XX.X.Mx(0f, val), 0f);
				if (touch_ui && UIStatus.isPr(this.Pr))
				{
					if (this.is_hp)
					{
						UIStatus.Instance.fineGSaverHpHit();
					}
					else
					{
						UIStatus.Instance.fineGSaverMpHit();
					}
				}
				return this.fineMinusDelay();
			}

			public PrGaugeSaver.GsItem Add(float val)
			{
				this.sval += global::XX.X.Mx(0f, val);
				return this.Fine(false);
			}

			public PrGaugeSaver.GsItem LockTime(float t, bool _frame_first = true)
			{
				this.t_lock = global::XX.X.Mx(t, this.t_lock);
				if (_frame_first)
				{
					this.frame_first = _frame_first;
				}
				return this;
			}

			public PrGaugeSaver.GsItem LockTime()
			{
				return this.LockTime(this.is_hp ? DIFF.lock_hp_gsaver_time(this.Pr, null, -1000) : DIFF.lock_mp_gsaver_time(this.Pr, null), true);
			}

			public PrGaugeSaver.GsItem LockTimeAdd(float t_add, float _max = -1f)
			{
				if (_max < 0f)
				{
					this.t_lock += t_add;
				}
				else
				{
					_max = global::XX.X.Mx(_max, this.t_lock);
					this.t_lock = global::XX.X.Mn(_max, this.t_lock + t_add);
				}
				return this;
			}

			public void run(float fcnt)
			{
				if (this.t_lock < 0f)
				{
					this.t_lock = global::XX.X.Mn(this.t_lock + fcnt, 0f);
					return;
				}
				if (this.frame_first)
				{
					fcnt *= (this.is_hp ? DIFF.TS_hp_frame_first(this.Pr) : DIFF.TS_mp_frame_first(this.Pr));
				}
				this.t_lock -= fcnt;
				if (this.t_lock <= 0f)
				{
					this.frame_first = false;
					if (this.is_hp)
					{
						if (DIFF.cureHpFromGSaver(this.Pr, ref this.sval, ref this.t_lock))
						{
							this.Pr.cureHp(1, false, false);
							return;
						}
					}
					else if (DIFF.cureMpFromGSaver(this.Pr, ref this.sval, ref this.t_lock))
					{
						if ((float)((int)(this.Pr.GaugeBrk.safe_holdable_ratio * this.Pr.get_maxmp() - 4f)) > this.Pr.get_mp())
						{
							this.Pr.cureMp(1, false, false, false);
							return;
						}
						this.t_lock = global::XX.X.Mx(20f, this.t_lock);
						if (UIStatus.isPr(this.Pr))
						{
							UIStatus.Instance.redraw_mp = true;
						}
					}
				}
			}

			public PrGaugeSaver.GsItem fineMinusDelay()
			{
				if (this.t_lock < 0f)
				{
					this.t_lock = 1f;
				}
				return this;
			}

			public PrGaugeSaver.GsItem removeHeadDelay()
			{
				if (this.frame_first)
				{
					this.t_lock = 0f;
					this.frame_first = false;
				}
				return this.Fine(true);
			}

			public PrGaugeSaver.GsItem debugSetValue(float v)
			{
				this.sval = v;
				return this.Fine(true);
			}

			public PR Pr
			{
				get
				{
					return this.Con.Pr;
				}
			}

			public float saved_gauge_value
			{
				get
				{
					return this.sval;
				}
			}

			public void readBinaryFrom(ByteArray Ba, int vers)
			{
				this.sval = (float)Ba.readUShort();
				this.t_lock = 0f;
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeUShort((ushort)this.sval);
			}

			private float sval;

			public readonly PrGaugeSaver Con;

			private bool is_hp;

			private float t_lock;

			private bool frame_first;
		}
	}
}
