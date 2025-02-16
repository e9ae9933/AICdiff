using System;
using System.Collections.Generic;
using m2d;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public class M2Ser : RBase<M2SerItem>
	{
		public M2Ser(M2Attackable _Mv, NelM2Attacker _Mva, bool _apply_post_effect = false)
			: base(38, true, false, false)
		{
			this.Mv = _Mv;
			this.Mva = _Mva;
			this.M2D = M2DBase.Instance as NelM2DBase;
		}

		public override M2SerItem Create()
		{
			return new M2SerItem(this);
		}

		public override void clear()
		{
			base.clear();
			this.ser_bits = (this.pre_ser_bits = 0UL);
			this.need_check_ser = true;
			this.resetFlags();
		}

		public float overchargeable_ratio
		{
			get
			{
				if ((this.ser_bits & 2UL) != 0UL)
				{
					return 1f;
				}
				if ((this.ser_bits & 512UL) != 0UL)
				{
					return 0.33f;
				}
				return 0f;
			}
		}

		public bool isShamed()
		{
			return (this.ser_bits & 9185795840UL) > 0UL;
		}

		public bool isStun()
		{
			return (this.ser_bits & 720896UL) > 0UL;
		}

		public bool isSleepDown()
		{
			return (this.ser_bits & 655360UL) > 0UL;
		}

		public bool isSleepDownBursted()
		{
			return (this.ser_bits & 589824UL) > 0UL;
		}

		public bool isHalfBgm()
		{
			if (this.M2D.isCenterPlayer(this.Mv))
			{
				if ((this.ser_bits & 137472507905UL) != 0UL)
				{
					return true;
				}
				if ((this.frozen_state_ & NoelAnimator.FRZ_STATE.STONE) != NoelAnimator.FRZ_STATE.NORMAL)
				{
					return true;
				}
			}
			return false;
		}

		public bool has(SER ser)
		{
			return (this.ser_bits & (1UL << (int)ser)) > 0UL;
		}

		public int getLevel(SER ser)
		{
			if ((this.ser_bits & (1UL << (int)ser)) <= 0UL)
			{
				return -1;
			}
			M2SerItem m2SerItem = this.Find(ser);
			if (m2SerItem != null)
			{
				return (int)m2SerItem.level;
			}
			return 0;
		}

		public bool hasBit(ulong bits)
		{
			return (this.ser_bits & bits) > 0UL;
		}

		public M2SerItem Find(SER ser)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				M2SerItem m2SerItem = this.AItems[i];
				if (m2SerItem.id == ser)
				{
					return m2SerItem;
				}
			}
			return null;
		}

		public M2SerItem Fine(SER ser, int time)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				M2SerItem m2SerItem = this.AItems[i];
				if (m2SerItem.id == ser && m2SerItem.isActive())
				{
					m2SerItem.fineSer(time, true, false);
					return m2SerItem;
				}
			}
			return null;
		}

		public M2Ser CureAll(bool after_check_ser = true)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				M2SerItem m2SerItem = this.AItems[i];
				try
				{
					m2SerItem.deactivate(true);
				}
				catch
				{
				}
			}
			if (after_check_ser)
			{
				ulong num = this.ser_bits;
				this.checkSerExecute(true, true);
				this.ser_bits |= num;
			}
			return null;
		}

		public M2SerItem Cure(SER ser)
		{
			if (this.has(ser))
			{
				M2SerItem m2SerItem = this.Find(ser);
				if (m2SerItem != null)
				{
					m2SerItem.deactivate(true);
				}
			}
			return null;
		}

		public M2Ser CureB(ulong cure_ser_bits)
		{
			for (int i = 37; i >= 0; i--)
			{
				if ((cure_ser_bits & (1UL << i)) != 0UL)
				{
					this.Cure((SER)((long)i));
				}
			}
			return this;
		}

		public M2Ser CureTime(SER ser, int time, bool no_multiply_speed = false)
		{
			M2SerItem m2SerItem = (this.has(ser) ? this.Find(ser) : null);
			if (m2SerItem != null)
			{
				m2SerItem.CureTime((int)((float)time * (no_multiply_speed ? 1f : this.progress_speed)));
			}
			return this;
		}

		public M2Ser CureFrozen3(bool fine_log_row = true)
		{
			M2SerItem m2SerItem = this.Find(SER.FROZEN);
			if (m2SerItem != null && m2SerItem.level >= 2 && m2SerItem.isActive())
			{
				m2SerItem.LevelCheckForceSet(2, fine_log_row);
			}
			m2SerItem = this.Find(SER.STONE);
			if (m2SerItem != null && m2SerItem.level >= 2 && m2SerItem.isActive())
			{
				m2SerItem.LevelCheckForceSet(4, fine_log_row);
			}
			return this;
		}

		public M2SerItem Add(SER ser, int __maxt = -1, int max_level = 99, bool add_to_pre_bits = false)
		{
			if (this.AItems == null)
			{
				return null;
			}
			M2SerItem m2SerItem = this.Find(ser);
			if (m2SerItem != null)
			{
				if (!m2SerItem.isActive())
				{
					this.resetFlags();
				}
				else
				{
					m2SerItem.LevelCheck(1, max_level);
				}
				this.ser_bits |= 1UL << (int)ser;
				return m2SerItem.fineSer(__maxt, (262144L & (1L << (int)ser)) != 0L, true);
			}
			this.ser_bits |= 1UL << (int)ser;
			if (add_to_pre_bits)
			{
				this.pre_ser_bits |= 1UL << (int)ser;
			}
			this.resetFlags();
			m2SerItem = base.Pop(64).registerSer(ser, __maxt, max_level);
			m2SerItem.fineLogRow();
			return m2SerItem;
		}

		public M2Ser AddB(ulong apply_ser_bits, int _level = 1)
		{
			for (int i = 37; i >= 0; i--)
			{
				ulong num = 1UL << i;
				if ((apply_ser_bits & num) != 0UL)
				{
					bool flag = this.hasBit(num);
					M2SerItem m2SerItem = this.Add((SER)((long)i), -1, 99, false);
					if (m2SerItem != null && !flag && (long)i == 30L)
					{
						_level++;
					}
					if (_level > 1)
					{
						m2SerItem.LevelCheck(_level - 1, 99);
					}
				}
			}
			return this;
		}

		public static bool applyAllBits(ulong apply_ser_bits, Func<SER, bool> Fn)
		{
			int num = 38;
			bool flag = true;
			for (int i = 0; i < num; i++)
			{
				if ((apply_ser_bits & (1UL << i)) != 0UL)
				{
					flag = Fn((SER)((long)i)) && flag;
				}
			}
			return flag;
		}

		public static STB listupAllTitle(ulong apply_ser_bits, STB Stb)
		{
			int num = 38;
			for (int i = 0; i < num; i++)
			{
				if ((apply_ser_bits & (1UL << i)) != 0UL)
				{
					Stb.AppendTxA("SerTitle_" + ((SER)((long)i)).ToString().ToLower(), ", ");
				}
			}
			return Stb;
		}

		public static bool applyAllBitsDescend(ulong apply_ser_bits, Func<SER, bool> Fn)
		{
			bool flag = true;
			for (int i = 37; i >= 0; i--)
			{
				if ((apply_ser_bits & (1UL << i)) != 0UL)
				{
					flag = Fn((SER)((long)i)) && flag;
				}
			}
			return flag;
		}

		public void removeBit(SER ser)
		{
			ulong num = ~(1UL << (int)ser);
			this.ser_bits &= num;
			this.resetFlags();
		}

		public void checkSer()
		{
			this.need_check_ser = true;
		}

		public ulong checkSerExecute(bool force = false, bool apply = true)
		{
			if (!force && !this.need_check_ser)
			{
				return 0UL;
			}
			this.need_check_ser = false;
			ulong num = 0UL;
			int num2 = 38;
			for (int i = 0; i < num2; i++)
			{
				int num3 = M2SerItem.canApplySer(this, (SER)((long)i), this.Mv, this.Mva);
				if (num3 >= 1)
				{
					num |= 1UL << i;
					if (apply)
					{
						M2SerItem m2SerItem;
						if ((this.ser_bits & (1UL << i)) == 0UL)
						{
							m2SerItem = this.Add((SER)((long)i), -1, num3, false);
						}
						else
						{
							if (num3 == 1)
							{
								goto IL_0092;
							}
							m2SerItem = this.Get((SER)((long)i));
						}
						if (num3 > 1)
						{
							m2SerItem.LevelCheck(num3 - 1, num3 - 1);
						}
					}
				}
				else if (num3 == 0)
				{
					this.Cure((SER)((long)i));
				}
				IL_0092:;
			}
			return num;
		}

		public void CureBench(ulong decline_ser = 0UL)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				M2SerItem m2SerItem = this.AItems[i];
				if ((decline_ser & (1UL << (int)m2SerItem.id)) == 0UL && m2SerItem.CureableOnBench() && m2SerItem != null)
				{
					m2SerItem.deactivate(true);
				}
			}
		}

		public override bool run(float fcnt)
		{
			this.apply_pe = this.M2D.isCenterPlayer(this.Mv);
			this.checkSerExecute(false, true);
			if (this.bgm_half_mem < 0)
			{
				bool flag = -1 - this.bgm_half_mem > 0;
				this.bgm_half_mem = (this.isHalfBgm() ? 1 : 0);
				if (this.bgm_half_mem > 0 != flag)
				{
					if (this.bgm_half_mem > 0)
					{
						BGM.addHalfFlag("PR");
					}
					else
					{
						BGM.remHalfFlag("PR");
					}
				}
			}
			this.pre_ser_bits = 0UL;
			bool flag2 = base.run(fcnt);
			this.ser_bits = this.pre_ser_bits;
			if (this.ALogDecline != null)
			{
				if (this.DeclineLogFine(ref this.ALogDecline[0], this.run_decline_ser))
				{
					this.DeclineLogFineChecking(ref this.ALogDecline[0], this.cannotRun());
				}
				if (this.DeclineLogFine(ref this.ALogDecline[1], this.evade_decline_ser))
				{
					this.DeclineLogFineChecking(ref this.ALogDecline[1], this.cannotEvade());
				}
				if (this.EfDecline != null)
				{
					if (this.EfDecline.index == this.efdecline_index)
					{
						this.EfDecline.x = this.Mv.x;
						this.EfDecline.y = this.Mv.mtop - 0.25f;
						return flag2;
					}
					this.EfDecline = null;
				}
			}
			return flag2;
		}

		private bool DeclineLogFine(ref UILogRow R, ulong declining_ser)
		{
			if (R == null)
			{
				return false;
			}
			if (R.isHiding())
			{
				R = null;
			}
			else if (declining_ser == 0UL)
			{
				return true;
			}
			return false;
		}

		private void DeclineLogFineChecking(ref UILogRow R, bool declining)
		{
			if (R == null)
			{
				return;
			}
			if (!declining)
			{
				R.deactivate(false);
				R = null;
			}
		}

		public bool applySerDamage(FlagCounter<SER> Apl, float apply_ratio0 = 1f, int ser_maxt = -1)
		{
			if (Apl == null)
			{
				return false;
			}
			bool flag = false;
			foreach (KeyValuePair<SER, float> keyValuePair in Apl.getRawObject())
			{
				SER key = keyValuePair.Key;
				float num = apply_ratio0;
				bool flag2 = true;
				float num2 = keyValuePair.Value;
				if (key != SER.SLEEP)
				{
					if (key == SER.STONE)
					{
						if (num2 > 0f && (this.frozen_state_ & NoelAnimator.FRZ_STATE.STONE) != NoelAnimator.FRZ_STATE.NORMAL)
						{
							flag2 = false;
							num = 100f;
						}
					}
				}
				else
				{
					if (this.has(SER.BURNED))
					{
						continue;
					}
					if (this.has(SER.SLEEP))
					{
						continue;
					}
				}
				if (this.Regist != null && flag2)
				{
					float time = this.Regist.getTime(key, 0f);
					if (time >= 255f)
					{
						continue;
					}
					num2 = X.Mx(0f, num2 - time / 2f) * X.ZLINE(100f - time, 100f);
				}
				if (X.XORSP() * 100f < num2 * num)
				{
					this.Add(key, ser_maxt, 99, false);
					flag = true;
				}
				else if (this.has(key))
				{
					M2SerItem m2SerItem = this.Get(key);
					if (m2SerItem != null && (!this.Mv.is_alive || m2SerItem.overwrite_attach) && m2SerItem.isActive())
					{
						m2SerItem.fineSer(2000, false, false);
					}
				}
			}
			return flag;
		}

		public void releaseEffect(bool delete_instance = false)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItems[i].releaseEffect(delete_instance);
			}
		}

		public void releasePtcST(bool do_not_kill_stock_effect = false)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItems[i].releasePtcST(do_not_kill_stock_effect);
			}
		}

		public ulong get_bits()
		{
			return this.ser_bits;
		}

		public ulong get_pre_bits()
		{
			return this.pre_ser_bits;
		}

		public void resetPtcSt()
		{
			for (int i = 0; i < this.LEN; i++)
			{
				M2SerItem m2SerItem = this.AItems[i];
				if (m2SerItem.isActive())
				{
					m2SerItem.finePtcProcess(false);
				}
			}
		}

		public M2SerItem Get(int i)
		{
			return this.AItems[i];
		}

		public M2SerItem Get(SER _ser)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				M2SerItem m2SerItem = this.AItems[i];
				if (m2SerItem.id == _ser)
				{
					return m2SerItem;
				}
			}
			return null;
		}

		public float getRestTime(SER _ser)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				M2SerItem m2SerItem = this.AItems[i];
				if (m2SerItem.id == _ser)
				{
					return (float)m2SerItem.maxt - m2SerItem.af;
				}
			}
			return 0f;
		}

		public void declineRunningEffect()
		{
			if (this.run_decline_ser != 0UL)
			{
				if (this.ALogDecline == null)
				{
					this.ALogDecline = new UILogRow[2];
				}
				if (this.ALogDecline[0] == null)
				{
					this.ALogDecline[0] = UILog.Instance.AddAlertTX("SerReject_run", UILogRow.TYPE.ALERT_HUNGER);
				}
				else
				{
					this.ALogDecline[0].hold();
				}
				if (this.EfDecline != null && this.EfDecline.index == this.efdecline_index)
				{
					if (this.EfDecline.af >= 40f)
					{
						this.EfDecline.af = 0f;
						return;
					}
				}
				else
				{
					this.EfDecline = this.Mv.Mp.setE("ser_decline", this.Mv.x, this.Mv.mtop - 0.25f, 0f, (int)this.run_decline_ser, 0);
					if (this.EfDecline != null)
					{
						this.efdecline_index = this.EfDecline.index;
					}
				}
			}
		}

		public void declineEvadeEffect()
		{
			if (this.evade_decline_ser != 0UL)
			{
				if (this.ALogDecline == null)
				{
					this.ALogDecline = new UILogRow[2];
				}
				if (this.ALogDecline[1] == null)
				{
					this.ALogDecline[1] = UILog.Instance.AddAlertTX("SerReject_evade", UILogRow.TYPE.ALERT_HUNGER);
				}
				else
				{
					this.ALogDecline[1].hold();
				}
				if (this.EfDecline != null && this.EfDecline.index == this.efdecline_index)
				{
					if (this.EfDecline.af >= 40f)
					{
						this.EfDecline.af = 0f;
						return;
					}
				}
				else
				{
					this.EfDecline = this.Mv.Mp.setE("ser_decline", this.Mv.x, this.Mv.mtop - 0.25f, 0f, (int)this.evade_decline_ser, 0);
					if (this.EfDecline != null)
					{
						this.efdecline_index = this.EfDecline.index;
					}
				}
			}
		}

		public void resetFlags()
		{
			this.cannot_evade = (this.cannot_run = (this.weak_pose = (this.wet_pose = -1)));
			if (this.bgm_half_mem >= 0)
			{
				this.bgm_half_mem = -1 - this.bgm_half_mem;
			}
			this.frozen_state_ = NoelAnimator.FRZ_STATE._MAX;
			this.base_timescale = (this.base_timescale_rev = (this.stomach_apply_ratio = -1f));
			this.ep_addition_ratio = -1000f;
			this.xspeed_rate = (this.mpgage_crack_rate = (this.mana_drain_rate = (this.jump_rate = (this.enemysink_rate = (this.chantmp_split_rate = (this.hpdamage_rate = (this.atk_rate = (this.chant_atk_rate = (this.chant_speed_rate = (this.gacha_release_rate = (this.burst_consume_ratio = -1f)))))))))));
			this.punch_allow = 2;
			this.orgasm_lock = 2;
			this.evade_decline_ser = (this.run_decline_ser = 0UL);
		}

		public bool cannotRun()
		{
			if (this.cannot_run == -1)
			{
				this.cannot_run = 0;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					if (m2SerItem.isActive() && m2SerItem.cannot_run)
					{
						this.cannot_run = 1;
						int iconImageId = m2SerItem.getIconImageId();
						if (iconImageId >= 0)
						{
							this.run_decline_ser |= 1UL << iconImageId;
						}
					}
				}
			}
			return this.cannot_run == 1;
		}

		public bool cannotEvade()
		{
			if (this.cannot_evade == -1)
			{
				this.cannot_evade = 0;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					if (m2SerItem.isActive() && m2SerItem.cannot_evade)
					{
						this.cannot_evade = 1;
						int iconImageId = m2SerItem.getIconImageId();
						if (iconImageId >= 0)
						{
							this.evade_decline_ser |= 1UL << iconImageId;
						}
					}
				}
			}
			return this.cannot_evade == 1;
		}

		public bool isWeakPose()
		{
			if (this.weak_pose == -1)
			{
				this.weak_pose = 0;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					if (m2SerItem.isActive() && m2SerItem.weak_pose)
					{
						this.weak_pose = 1;
						break;
					}
				}
			}
			return this.weak_pose == 1;
		}

		public bool isWetPose()
		{
			if (this.wet_pose == -1)
			{
				this.wet_pose = 0;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					if (m2SerItem.isActive() && m2SerItem.wet_pose)
					{
						this.wet_pose = 1;
						break;
					}
				}
			}
			return this.wet_pose == 1;
		}

		public float xSpeedRate()
		{
			if (this.xspeed_rate == -1f)
			{
				this.xspeed_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.xspeed_rate = X.Mn(m2SerItem.isActive() ? m2SerItem.xspeed_rate : 1f, this.xspeed_rate);
				}
			}
			return this.xspeed_rate;
		}

		public float enemySinkRate()
		{
			if (this.enemysink_rate == -1f)
			{
				this.enemysink_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.enemysink_rate *= (m2SerItem.isActive() ? m2SerItem.enemysink_rate : 1f);
				}
			}
			return this.enemysink_rate;
		}

		public float jumpSpeedRate()
		{
			if (this.jump_rate == -1f)
			{
				this.jump_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.jump_rate *= (m2SerItem.isActive() ? m2SerItem.jump_rate : 1f);
				}
			}
			return this.jump_rate;
		}

		public float mpGageCrackRate()
		{
			if (this.mpgage_crack_rate == -1f)
			{
				this.mpgage_crack_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.mpgage_crack_rate *= (m2SerItem.isActive() ? m2SerItem.mpgage_crack_rate : 1f);
				}
			}
			return this.mpgage_crack_rate;
		}

		public float chantMpSplitRate()
		{
			if (this.chantmp_split_rate == -1f)
			{
				this.chantmp_split_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.chantmp_split_rate *= (m2SerItem.isActive() ? m2SerItem.chantmp_split_rate : 1f);
				}
			}
			return this.chantmp_split_rate;
		}

		public float HpDamageRate()
		{
			if (this.hpdamage_rate == -1f)
			{
				this.hpdamage_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.hpdamage_rate *= (m2SerItem.isActive() ? m2SerItem.hpdamage_rate : 1f);
				}
			}
			return this.hpdamage_rate;
		}

		public float AtkRate()
		{
			if (this.atk_rate == -1f)
			{
				this.atk_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.atk_rate *= (m2SerItem.isActive() ? m2SerItem.atk_rate : 1f);
				}
			}
			return this.atk_rate;
		}

		public float ChantAtkRate()
		{
			if (this.chant_atk_rate == -1f)
			{
				this.chant_atk_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.chant_atk_rate *= (m2SerItem.isActive() ? m2SerItem.chant_atk_rate : 1f);
				}
			}
			return this.chant_atk_rate;
		}

		public float ChantSpeedRate()
		{
			if (this.chant_speed_rate == -1f)
			{
				this.chant_speed_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.chant_speed_rate *= (m2SerItem.isActive() ? m2SerItem.chant_speed_rate : 1f);
				}
			}
			return this.chant_speed_rate;
		}

		public float GachaReleaseRate()
		{
			if (this.gacha_release_rate == -1f)
			{
				this.gacha_release_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					if (m2SerItem.isActive())
					{
						this.gacha_release_rate = X.Mn(m2SerItem.gacha_release_rate, this.gacha_release_rate);
					}
				}
			}
			return this.gacha_release_rate;
		}

		public float EpApplyRatio()
		{
			if (this.ep_addition_ratio == -1000f)
			{
				this.ep_addition_ratio = 0f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.ep_addition_ratio += (m2SerItem.isActive() ? m2SerItem.ep_addition_ratio : 0f);
				}
			}
			return X.Mx(0f, 1f + this.ep_addition_ratio);
		}

		public float getMpDrainRate()
		{
			if (this.mana_drain_rate == -1f)
			{
				this.mana_drain_rate = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.mana_drain_rate *= (m2SerItem.isActive() ? m2SerItem.mana_drain_rate : 1f);
				}
			}
			return this.mana_drain_rate;
		}

		public bool punchAllow()
		{
			if (this.punch_allow == 2)
			{
				this.punch_allow = 1;
				for (int i = 0; i < this.LEN; i++)
				{
					if (!this.AItems[i].punch_allow)
					{
						this.punch_allow = 0;
						break;
					}
				}
			}
			return this.punch_allow == 1;
		}

		public bool isOrgasmLocked()
		{
			if (this.orgasm_lock == 2)
			{
				this.orgasm_lock = 0;
				for (int i = 0; i < this.LEN; i++)
				{
					if (this.AItems[i].orgasm_locked)
					{
						this.orgasm_lock = 1;
						break;
					}
				}
			}
			return this.orgasm_lock == 1;
		}

		public NoelAnimator.FRZ_STATE frozen_state
		{
			get
			{
				if (this.frozen_state_ == NoelAnimator.FRZ_STATE._MAX)
				{
					this.frozen_state_ = NoelAnimator.FRZ_STATE.NORMAL;
					for (int i = 0; i < this.LEN; i++)
					{
						M2SerItem m2SerItem = this.AItems[i];
						this.frozen_state_ |= m2SerItem.frozen_state;
					}
				}
				return this.frozen_state_;
			}
		}

		public float burstConsumeRatio()
		{
			if (this.burst_consume_ratio == -1f)
			{
				this.burst_consume_ratio = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.burst_consume_ratio *= (m2SerItem.isActive() ? m2SerItem.burst_consume_ratio : 1f);
				}
			}
			return this.burst_consume_ratio;
		}

		private void fineTimeScale()
		{
			if (this.base_timescale == -1f)
			{
				this.base_timescale = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.base_timescale *= (m2SerItem.isActive() ? m2SerItem.base_timescale : 1f);
				}
				this.base_timescale_rev = 1f / this.base_timescale;
			}
		}

		public float getStomachApplyRatio()
		{
			if (this.stomach_apply_ratio == -1f)
			{
				this.stomach_apply_ratio = 1f;
				for (int i = 0; i < this.LEN; i++)
				{
					M2SerItem m2SerItem = this.AItems[i];
					this.stomach_apply_ratio *= (m2SerItem.isActive() ? m2SerItem.stomach_apply_ratio : 1f);
				}
			}
			return this.stomach_apply_ratio;
		}

		public float baseTimeScale()
		{
			this.fineTimeScale();
			return this.base_timescale;
		}

		public float baseTimeScaleRev()
		{
			this.fineTimeScale();
			return this.base_timescale_rev;
		}

		public void readBinaryFrom(ByteReader Ba, bool read_level = true)
		{
			this.clear();
			int num = (int)Ba.readUByte();
			for (int i = 0; i < num; i++)
			{
				int len = this.LEN;
				SER ser = (SER)((long)Ba.readByte());
				M2SerItem m2SerItem = this.Add(ser, -1, 99, false);
				m2SerItem.readBinaryFrom(Ba, read_level);
				if (!m2SerItem.isActive())
				{
					this.Cure(ser);
				}
			}
			this.resetFlags();
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(this.LEN);
			for (int i = 0; i < this.LEN; i++)
			{
				M2SerItem m2SerItem = this.Get(i);
				Ba.writeByte((int)((byte)m2SerItem.id));
				m2SerItem.writeBinaryTo(Ba);
			}
		}

		public void checkDamageSpecial(ref int val, NelAttackInfo Atk)
		{
			if (Atk == null)
			{
				return;
			}
			if (Atk.isPhysical() && Atk.attr != MGATTR.ICE && Atk.attr != MGATTR.ACME && Atk.attr != MGATTR.SPERMA)
			{
				if (this.has(SER.STONE) && val > 0)
				{
					M2SerItem m2SerItem = this.Find(SER.STONE);
					if (m2SerItem != null)
					{
						val = (int)((float)val * X.NIL(0.75f, 0.125f, (float)m2SerItem.level, 2f));
						this.Mv.PtcST("stone_apply_physic_damage", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						return;
					}
				}
				else if (this.has(SER.FROZEN) && val > 0)
				{
					M2SerItem m2SerItem2 = this.Find(SER.FROZEN);
					if (m2SerItem2 != null)
					{
						val = (int)((float)val * X.NIL(1.5f, 2.5f, (float)m2SerItem2.level, 2f));
						this.Mv.PtcST("frozen_apply_physic_damage", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						if (this.Mv.is_alive)
						{
							m2SerItem2.CureTime(240);
						}
					}
				}
			}
		}

		public readonly M2Attackable Mv;

		public readonly NelM2Attacker Mva;

		public const int SER_TIME_DEFAULT = 120;

		private ulong ser_bits;

		public ulong pre_ser_bits;

		public readonly NelM2DBase M2D;

		public bool apply_pe;

		public M2SerResist Regist;

		public UILogRow[] ALogDecline;

		public EffectItem EfDecline;

		public uint efdecline_index;

		public float progress_speed = 1f;

		public bool need_check_ser = true;

		private NoelAnimator.FRZ_STATE frozen_state_;

		public const ulong can_split_mp_bits = 176547383360UL;

		public const ulong shamed_bits = 9185795840UL;

		public const ulong half_bgm_bits = 137472507905UL;

		public const ulong stun_bits = 720896UL;

		public const ulong gacha_manual_cannot_release_bits = 51052608UL;

		public const ulong sleepdown_bits = 655360UL;

		public const ulong sleepdown_bursted_bits = 589824UL;

		public const ulong auto_adding_maxt_fine_bits = 262144UL;

		public const ulong emstate_ser_bits = 110804820220UL;

		public const ulong orgasm_bits = 50331648UL;

		public const ulong use_sp_beto_bits = 51808043008UL;

		private int cannot_evade = -1;

		private int cannot_run = -1;

		private byte punch_allow = 2;

		private int weak_pose = -1;

		private int wet_pose = -1;

		private float hpdamage_rate = -1f;

		private float xspeed_rate = -1f;

		private float jump_rate = -1f;

		private float mpgage_crack_rate = -1f;

		private float enemysink_rate = -1f;

		private float chantmp_split_rate = -1f;

		private float gacha_release_rate = -1f;

		private float atk_rate = 1f;

		private float burst_consume_ratio = -1f;

		private float chant_speed_rate = 1f;

		private float chant_atk_rate = 1f;

		private float ep_addition_ratio = -1000f;

		private float mana_drain_rate = -1f;

		private float base_timescale = -1f;

		private float base_timescale_rev = 1f;

		private float stomach_apply_ratio = -1f;

		private byte orgasm_lock = 2;

		private int bgm_half_mem;

		private ulong evade_decline_ser;

		private ulong run_decline_ser;
	}
}
