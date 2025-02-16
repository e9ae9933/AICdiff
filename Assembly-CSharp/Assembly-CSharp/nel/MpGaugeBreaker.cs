using System;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class MpGaugeBreaker
	{
		public MpGaugeBreaker(PR _Pr)
		{
			this.Pr = _Pr;
			this.Agage_breaked = new byte[3];
			this.Agage_cured = new byte[3];
			this.Agage_reduce_dep_max = new float[] { 0.125f, 0.25f, 0.375f };
			this.Agage_reduce_speed = new float[] { 0.06666667f, 0.08888889f, 0.16f };
		}

		public void reset()
		{
			this.break_cnt = (int)(this.Agage_breaked[0] = (this.Agage_breaked[1] = (this.Agage_breaked[2] = 0)));
			this.Agage_cured[0] = (this.Agage_cured[1] = (this.Agage_cured[2] = 0));
			this.safe_holdable_ratio_ = 1f;
			this.damage = 0f;
			this.active_flag = 0;
			this.cured_count = 0;
			this.break_delayed_stack = -1;
			this.t_break_delay = 0f;
			this.omorasi_delay = 0f;
			this.mp_damage_counter = 0f;
			this.mana_counter = 0;
			this.secure_split_time = (this.secure_split_mphold_time = 0f);
			this.stopeffect_delay = 0f;
		}

		public bool check(float mp_dmg)
		{
			if (mp_dmg <= 0f)
			{
				return false;
			}
			float num = global::XX.X.Mn(global::XX.X.Mx(this.Pr.GSaver.GsMp.saved_gauge_value, this.Pr.getCastableMp()) + (float)this.Pr.EggCon.total_real, this.Pr.get_maxmp());
			float num2 = global::XX.X.NI(0.5f, 0.125f, global::XX.X.ZSIN((float)(this.break_cnt - this.cured_count - 6), 15f));
			float num3 = ((this.Pr.NM2D.GameOver != null && this.Pr.NM2D.GameOver.isGivingUp()) ? 2f : this.Pr.Ser.mpGageCrackRate());
			float num4 = 0f;
			if (this.cured_count > 0)
			{
				num3 = global::XX.X.Mx(num3, 0.2f);
				num4 += 0.02f * global::XX.X.ZLINE(1f - this.Pr.mp_ratio - 0.5f, 0.4f);
			}
			float num5 = num4 + global::XX.X.Mn(num2, num3 * mp_dmg / global::XX.X.Mx(this.Pr.get_maxmp() * 0.15f, mp_dmg + num));
			if (num < this.Pr.get_maxmp() * (0.6f * (float)((this.cured_count > 0) ? 2 : 1)))
			{
				this.applyDamage(num5);
			}
			else
			{
				this.damage += num5;
			}
			return false;
		}

		public int Cure(int val = 1, bool set_ui_effect = true)
		{
			if (this.break_cnt - this.cured_count <= 0 || val <= 0)
			{
				return 0;
			}
			val = global::XX.X.Mn(val, this.break_cnt - this.cured_count);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < 3; i++)
			{
				int num5 = (int)(this.Agage_breaked[i] - this.Agage_cured[i]);
				if (num5 > num4)
				{
					num3 = (num2 = 0);
					num4 = num5;
				}
				if (num5 == num4)
				{
					num2 |= 1 << i;
					num3++;
				}
			}
			if (num3 > 0)
			{
				num3 = ((num3 <= 1) ? 0 : global::XX.X.xors(num3));
				for (int j = 2; j >= 0; j--)
				{
					if ((int)(this.Agage_breaked[j] - this.Agage_cured[j]) == num4 && --num3 < 0)
					{
						this.last_breaked_index = (int)((byte)j);
						val = global::XX.X.Mn((int)this.Agage_breaked[j], val);
						byte[] agage_cured = this.Agage_cured;
						int num6 = j;
						agage_cured[num6] += (byte)val;
						num += val;
						val = 0;
						break;
					}
				}
			}
			this.safe_holdable_ratio_ = -1f;
			if (num > 0)
			{
				this.mana_counter = 0;
				this.mp_damage_counter = 0f;
				this.active_flag &= -7;
				this.secureSplitTime(40f);
				this.cured_count += num;
				if (set_ui_effect)
				{
					SND.Ui.play("orgasm_cure_crack", false);
					UIStatus.Instance.initCrackCure(num);
				}
			}
			return num;
		}

		public void applyDamage(float val = 1f)
		{
			if (val <= 0f)
			{
				return;
			}
			this.damage += val;
			int num = 0;
			while (this.break_cnt + num - this.cured_count < 21)
			{
				float num2 = global::XX.X.NI(0, 3, global::XX.X.ZSIN((float)(this.break_cnt - this.cured_count + num - 6 - ((this.cured_count > 0) ? 8 : 0)), 15f));
				if (global::XX.X.XORSP() * (num2 + 2f) / 1f >= this.damage - 0.25f)
				{
					break;
				}
				this.damage = global::XX.X.Mx(0f, global::XX.X.Mn(2f, this.damage) - (0.5f + num2));
				num++;
			}
			if (num >= 1)
			{
				if (this.break_delayed_stack < 0)
				{
					this.t_break_delay = 0f;
					this.gageDamage(false);
					num--;
				}
				this.break_delayed_stack += num;
			}
		}

		public float cureNotHunger()
		{
			return this.cureToRatio(0.15f);
		}

		public float cureToRatio(float target_ratio)
		{
			bool flag = this.break_cnt != 0;
			this.break_delayed_stack = -1;
			if (!flag)
			{
				return -1f;
			}
			for (int i = 0; i < 3; i++)
			{
				if (this.Agage_breaked[i] != 0)
				{
					while (this.getBreakDep(i, true) < target_ratio)
					{
						byte[] agage_breaked = this.Agage_breaked;
						int num = i;
						byte b = agage_breaked[num] - 1;
						agage_breaked[num] = b;
						byte b2 = b;
						this.Agage_cured[i] = global::XX.X.Mn(this.Agage_cured[i], b2);
						if (b2 <= 0)
						{
							break;
						}
					}
				}
			}
			this.break_cnt = 0;
			this.cured_count = 0;
			float num2 = 1f;
			this.safe_holdable_ratio_ = 1f;
			for (int j = 0; j < 3; j++)
			{
				float breakDep = this.getBreakDep(j, true);
				this.safe_holdable_ratio_ = global::XX.X.Mn(breakDep, this.safe_holdable_ratio_);
				this.cured_count += (int)this.Agage_cured[j];
				this.break_cnt += (int)this.Agage_breaked[j];
				num2 = global::XX.X.Mn(num2, breakDep);
			}
			return global::XX.X.Mn(target_ratio, num2);
		}

		public int cureToRatioTemp(float target_ratio)
		{
			bool flag = true;
			int num = 0;
			while (flag)
			{
				flag = false;
				for (int i = 0; i < 3; i++)
				{
					if (this.Agage_breaked[i] - this.Agage_cured[i] != 0 && this.getBreakDep(i, true) < target_ratio)
					{
						num += this.Cure(1, false);
						flag = true;
						break;
					}
				}
			}
			if (num > 0)
			{
				SND.Ui.play("orgasm_cure_crack", false);
				UIStatus.Instance.initCrackCure(num);
			}
			return num;
		}

		public void cureByItem(int val)
		{
			byte b = 0;
			int num = -1;
			while (val > 0)
			{
				int num2 = global::XX.X.xors(3);
				int i;
				for (i = 0; i < 3; i++)
				{
					int num3 = (num2 + i) % 3;
					if (this.Agage_breaked[num3] - this.Agage_cured[num3] > 0)
					{
						i = num3;
						break;
					}
				}
				if (i >= 3)
				{
					break;
				}
				byte[] agage_cured = this.Agage_cured;
				int num4 = i;
				agage_cured[num4] += 1;
				num = i;
				b += 1;
				val--;
			}
			if (b != 0 && num >= 0)
			{
				this.safe_holdable_ratio_ = -1f;
				this.mana_counter = 0;
				this.mp_damage_counter = 0f;
				this.secureSplitTime(40f);
				this.active_flag &= -7;
				this.last_breaked_index = (int)((byte)num);
				this.cured_count += (int)b;
				SND.Ui.play("orgasm_cure_crack", false);
				UIStatus.Instance.initCrackCure((int)b);
			}
		}

		public int gageDamage(bool no_effect = false)
		{
			int num = 0;
			while (num < 3 && this.Agage_breaked[num] >= 7)
			{
				num++;
			}
			if (this.cured_count > 0)
			{
				int num2 = 0;
				int num3 = 0;
				for (int i = 2; i >= 0; i--)
				{
					int num4 = (int)this.Agage_cured[i];
					if (num4 > num3)
					{
						num2 = i;
						num3 = num4;
					}
				}
				if (num3 > 0)
				{
					byte[] agage_cured = this.Agage_cured;
					int num5 = num2;
					agage_cured[num5] -= 1;
					if (!this.Pr.isFacingEnemy() && this.getBreakDep(num2, true) < 0.15f)
					{
						byte[] agage_cured2 = this.Agage_cured;
						int num6 = num2;
						agage_cured2[num6] += 1;
						return -1;
					}
					this.cured_count--;
					this.last_breaked_index = (int)((byte)num2);
					this.Pr.Mp.playSnd("gage_crack");
					this.Pr.Mp.setET("mp_crack", 4f, 30f, 10f, 5, 0);
					this.active_flag |= 1;
					UIStatus.Instance.initCrack();
					this.safe_holdable_ratio_ = -1f;
					return -1;
				}
				else
				{
					this.cured_count = 0;
					this.Agage_cured[0] = (this.Agage_cured[1] = (this.Agage_cured[2] = 0));
					this.safe_holdable_ratio_ = -1f;
				}
			}
			if (num >= 3)
			{
				this.break_cnt = 21;
				return -1;
			}
			switch (this.break_cnt % 13)
			{
			case 4:
			case 7:
			case 9:
			case 12:
				num++;
				break;
			case 11:
				num += 2;
				break;
			}
			int num7 = -1;
			for (int j = global::XX.X.Mn(2, num); j < 3; j++)
			{
				if (this.Agage_breaked[j] < 7)
				{
					num7 = j;
					break;
				}
			}
			if (num7 == -1)
			{
				for (int k = global::XX.X.Mn(2, num - 1); k >= 0; k--)
				{
					if (this.Agage_breaked[k] < 7)
					{
						num7 = k;
						break;
					}
				}
			}
			if (num7 == -1)
			{
				return -1;
			}
			if (!this.Pr.isFacingEnemy() && this.getBreakDep(num7, true) < 0.15f)
			{
				return -1;
			}
			byte[] agage_breaked = this.Agage_breaked;
			int num8 = num7;
			agage_breaked[num8] += 1;
			this.break_cnt++;
			this.last_breaked_index = num7;
			this.active_flag |= 1;
			this.safe_holdable_ratio_ = global::XX.X.Mn(this.safe_holdable_ratio_, this.getBreakDep(num7, true));
			UIStatus.Instance.initCrack();
			if (!no_effect)
			{
				this.Pr.Mp.playSnd("gage_crack");
				if (this.stopeffect_delay <= 0f)
				{
					this.stopeffect_delay = 400f;
					PostEffect.IT.setSlow(10f, 0f, 0);
					PostEffect.IT.addTimeFixedEffect(this.Pr.Mp.setET("mp_crack", 9f, 30f, 10f, 30, 0), 1f);
				}
				else
				{
					this.Pr.Mp.setET("mp_crack", 5f, 30f, 10f, 5, 0);
				}
			}
			return num7;
		}

		public int last_breaked_index
		{
			get
			{
				return (this.active_flag >> 8) & 3;
			}
			set
			{
				this.active_flag = (this.active_flag & -769) | ((value & 3) << 8);
			}
		}

		public void secureSplitTime(float t)
		{
			this.secure_split_time = global::XX.X.Mx(this.secure_split_time, t);
		}

		public void secureSplitMpHoldTime(float t)
		{
			this.secure_split_mphold_time = global::XX.X.Mx(this.secure_split_mphold_time, t);
		}

		public int getReducePlayerMpValue(float fcnt = 0f)
		{
			if (fcnt > 0f && this.break_delayed_stack >= 0)
			{
				this.t_break_delay += Map2d.TScur;
				if (this.t_break_delay >= 20f)
				{
					int num = this.break_delayed_stack;
					this.break_delayed_stack = num - 1;
					if (num > 0)
					{
						this.t_break_delay -= 20f + global::XX.X.NIXP(0f, 5f);
						this.gageDamage(false);
					}
					else
					{
						this.break_delayed_stack = -1;
						this.t_break_delay = 0f;
					}
				}
			}
			if (this.stopeffect_delay > 0f)
			{
				this.stopeffect_delay -= fcnt;
			}
			if ((this.active_flag & 1) == 0 || this.Pr.isPuzzleManagingMp())
			{
				return 0;
			}
			this.active_flag &= -7;
			bool flag = false;
			if (this.secure_split_mphold_time > 0f)
			{
				this.secure_split_mphold_time = global::XX.X.Mx(this.secure_split_mphold_time - fcnt, 0f);
			}
			if (this.secure_split_time > 0f)
			{
				this.secure_split_time = global::XX.X.Mx(this.secure_split_time - fcnt, 0f);
				flag = true;
			}
			if (this.omorasi_delay < 40f)
			{
				this.omorasi_delay += fcnt;
				flag = true;
			}
			if (flag)
			{
				return 0;
			}
			this.omorasi_delay = 0f;
			M2ManaContainer mana = (this.Pr.M2D as NelM2DBase).Mana;
			float num2 = (this.Pr.getCastableMp() + (float)((this.secure_split_mphold_time > 0f) ? 0 : this.Pr.Skill.getOverHoldingMp(false))) / this.Pr.get_maxmp();
			int i = 2;
			while (i >= 0)
			{
				bool flag2 = false;
				int num3 = (int)(this.Agage_breaked[i] - this.Agage_cured[i]);
				float num4 = 1f - (float)num3 * (1f - this.Agage_reduce_dep_max[i]) / 7f;
				if (num2 < num4 && !this.Pr.magic_chanting && this.Pr.mp_ratio >= num4)
				{
					this.mp_damage_counter += this.Agage_reduce_speed[i] * 0.25f * fcnt;
					this.active_flag |= 4;
					flag2 = true;
				}
				else if (num2 >= num4)
				{
					this.mp_damage_counter += this.Agage_reduce_speed[i] * fcnt;
				}
				else
				{
					if (i != 0 || this.mana_counter <= 0)
					{
						i--;
						continue;
					}
					this.mp_damage_counter = global::XX.X.Mn(this.mp_damage_counter + 1f, 1f);
				}
				this.omorasi_delay = 40f;
				this.active_flag |= 2;
				if (this.mp_damage_counter >= 1f)
				{
					int num5 = (int)this.mp_damage_counter;
					this.mp_damage_counter -= (float)num5;
					this.mana_counter += num5;
					if (this.mana_counter >= 4)
					{
						int num6 = this.mana_counter / 4;
						Vector2 vector = (flag2 ? this.Pr.getTargetPos() : this.Pr.getCenter());
						mana.AddMulti(vector.x, vector.y - (flag2 ? 0f : (this.Pr.sizey * global::XX.X.NIXP(0f, 0.5f))), (float)(4 * num6), (MANA_HIT)2058);
						this.mana_counter -= num6 * 4;
					}
					if (!this.Pr.Ser.has(SER.SHAMED_SPLIT))
					{
						this.Pr.Ser.Add(SER.SHAMED_SPLIT, 120, 99, false);
					}
					return num5;
				}
				break;
			}
			return 0;
		}

		public bool isReducing()
		{
			return (this.active_flag & 2) > 0;
		}

		public bool isReduceFromTarget()
		{
			return (this.active_flag & 4) > 0;
		}

		public bool isActive()
		{
			return (this.active_flag & 1) > 0;
		}

		public int getBreakLevel(int i)
		{
			if (this.break_cnt == 0)
			{
				return 0;
			}
			return (int)this.Agage_breaked[i];
		}

		public int getCureCount(int i)
		{
			return (int)this.Agage_cured[i];
		}

		public int getCurrentCuredIndex()
		{
			return this.last_breaked_index;
		}

		public float getBreakDep(int i = -1, bool consider_cure = true)
		{
			if (this.break_cnt == 0)
			{
				return 1f;
			}
			if (i < 0)
			{
				i = this.last_breaked_index;
			}
			return 1f - (float)(this.Agage_breaked[i] - (consider_cure ? this.Agage_cured[i] : 0)) * (1f - this.Agage_reduce_dep_max[i]) / 7f;
		}

		public float getBreakWidth(int i = -1)
		{
			if (i < 0)
			{
				i = this.last_breaked_index;
			}
			return (1f - this.Agage_reduce_dep_max[i]) / 7f;
		}

		public bool breakable_more
		{
			get
			{
				return this.break_cnt < 21;
			}
		}

		public float safe_holdable_ratio
		{
			get
			{
				if (this.safe_holdable_ratio_ < 0f)
				{
					this.safe_holdable_ratio_ = 1f;
					for (int i = 0; i < 3; i++)
					{
						this.safe_holdable_ratio_ = global::XX.X.Mn(this.getBreakDep(i, true), this.safe_holdable_ratio_);
					}
				}
				return this.safe_holdable_ratio_;
			}
		}

		public void readBinaryFrom(ByteArray Ba, bool apply_break_cnt)
		{
			this.reset();
			this.damage = Ba.readFloat();
			this.break_delayed_stack = (int)Ba.readShort();
			if (!apply_break_cnt)
			{
				this.break_delayed_stack = -1;
			}
			this.safe_holdable_ratio_ = -1f;
			for (int i = 0; i < 3; i++)
			{
				this.break_cnt += (int)(this.Agage_breaked[i] = (byte)Ba.readByte());
				this.cured_count += (int)(this.Agage_cured[i] = (byte)Ba.readByte());
			}
			if (this.break_cnt > 0)
			{
				this.active_flag |= 1;
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeFloat(this.damage);
			Ba.writeShort((short)this.break_delayed_stack);
			for (int i = 0; i < 3; i++)
			{
				Ba.writeByte((int)this.Agage_breaked[i]);
				Ba.writeByte((int)this.Agage_cured[i]);
			}
		}

		public const float DEBUG_DAMAGE_RATIO = 1f;

		public readonly PR Pr;

		private float damage;

		private const float maxbreak_thresh_mp = 0.6f;

		private byte[] Agage_breaked;

		private byte[] Agage_cured;

		private readonly float[] Agage_reduce_dep_max;

		private readonly float[] Agage_reduce_speed;

		private float t_break_delay;

		private int break_delayed_stack;

		private float mp_damage_counter;

		private float safe_holdable_ratio_ = 1f;

		private int mana_counter;

		private float secure_split_time;

		private float secure_split_mphold_time;

		public const int ARRAY_MAX = 3;

		private const int BREAK_MAX = 7;

		private int break_cnt;

		private int active_flag;

		private float stopeffect_delay;

		private int cured_count;

		private float omorasi_delay;

		public const int DELAY_CRACK_TIME = 20;

		public const int OMORASI_DELAY_TIME = 40;

		public const float HOLD_NOT_CHANTING_RATE = 0.25f;
	}
}
