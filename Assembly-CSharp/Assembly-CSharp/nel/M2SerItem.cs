using System;
using evt;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public class M2SerItem : IRunAndDestroy
	{
		public M2SerItem(M2Ser _Con)
		{
			this.Con = _Con;
			this.EfHn = new EffectHandlerPE(2);
		}

		public M2SerItem registerSer(SER ser, int time, int max_level = 99)
		{
			this.id = ser;
			this.maxt = ((time <= 0) ? (-120) : time);
			this.ptc_id = 0U;
			this.af = 0f;
			this.ef_time = 0f;
			this.level_count = (this.level = 0);
			this.LevelCheckIndividual((int)this.level_count, false, max_level, false, true);
			this.need_init = true;
			return this;
		}

		public M2SerItem fineSer(int __maxt, bool fine_maxt = true, bool max_calc_maxt = false)
		{
			if (this.maxt == 0)
			{
				this.registerSer(this.id, __maxt, 99);
			}
			else
			{
				this.af = ((__maxt <= 0) ? 0f : X.Mx(this.af - (float)__maxt, 0f));
				if (fine_maxt)
				{
					if (max_calc_maxt)
					{
						this.maxt = X.Mx(this.maxt, (__maxt <= 0) ? (-120) : __maxt);
					}
					else
					{
						this.maxt = X.Mx(this.maxt, (__maxt <= 0) ? (-120) : 0);
					}
				}
			}
			return this;
		}

		public M2SerItem fineLogRow()
		{
			if (this.Mv.Mp == null)
			{
				return this;
			}
			if (this.Mv.M2D.isCenterPlayer(this.Mv))
			{
				SER ser = this.id;
				if (ser != SER.NEAR_PEE)
				{
					if (ser == SER.ORGASM_STACK)
					{
						return this;
					}
				}
				else if (this.level >= 2)
				{
					return this;
				}
				string title = this.getTitle(true, false);
				if (title != "" && this.getIconImageId() != -1)
				{
					string text = TX.Get("Stat_Condition", "") + ": " + title;
					if (this.UiRow != null)
					{
						this.UiRow.fineTextTo(text, true);
					}
					else
					{
						this.UiRow = UILog.Instance.AddAlert(text, (this.id == SER.HP_REDUCE || this.id == SER.MP_REDUCE) ? UILogRow.TYPE.ALERT_HUNGER : UILogRow.TYPE.ALERT_GRAY);
					}
				}
			}
			return this;
		}

		public M2SerItem CureTime(int time)
		{
			if (this.af >= 0f)
			{
				if (this.maxt < 0)
				{
					this.maxt = X.Mn(-this.maxt + time, -1);
				}
				else if (this.maxt > 0)
				{
					this.maxt = X.Mx(this.maxt - time, 1);
				}
			}
			return this;
		}

		public bool LevelCheck(int add_level = 1, int max_level = 99)
		{
			if (this.level_count >= 255)
			{
				return false;
			}
			this.level_count = (byte)X.Mn(255, (int)this.level_count + add_level);
			return this.LevelCheckIndividual((int)this.level_count, true, max_level, add_level > 0, true);
		}

		public bool LevelCheckForceSet(int set_level = 1, bool fine_log_row = true)
		{
			if (this.level_count >= 255)
			{
				return false;
			}
			byte b = (byte)X.MMX(0, set_level, 255);
			byte b2 = this.level_count;
			this.level_count = b;
			this.level = 0;
			return this.LevelCheckIndividual((int)this.level_count, true, 99, true, fine_log_row);
		}

		public M2Attackable Mv
		{
			get
			{
				return this.Con.Mv;
			}
		}

		public NelM2Attacker Mva
		{
			get
			{
				return this.Con.Mva;
			}
		}

		public int canApplySer()
		{
			return M2SerItem.canApplySer(this.Con, this.id, this.Mv, this.Mva);
		}

		private PostEffectItem setPE(POSTM postm, float z_maxt, float x_level = 1f, int saf = 0)
		{
			if (!this.Con.apply_pe)
			{
				return null;
			}
			PostEffectItem postEffectItem = this.Con.M2D.PE.setPE(postm, z_maxt, x_level, saf);
			this.EfHn.Set(postEffectItem);
			return postEffectItem;
		}

		public void releaseEffect(bool delete_instance = false)
		{
			this.EfHn.deactivate(delete_instance);
			this.releasePtcST(false);
			if (this.Te != null)
			{
				this.Te.destruct();
				this.Te = null;
			}
		}

		public void destruct()
		{
			this.deactivate(false);
			this.maxt = 0;
			this.af = 0f;
			this.level_count = 0;
			this.level = 0;
			this.ef_time = 0f;
			this.Con.removeBit(this.id);
			if (this.Snd != null)
			{
				this.Snd.Stop();
				this.Snd = null;
			}
			if (this.UiRow != null)
			{
				this.UiRow.hideProgress();
			}
			this.UiRow = null;
			this.releaseEffect(true);
		}

		public bool isActive()
		{
			return this.maxt != 0 && this.af >= 0f;
		}

		public void deactivate(bool immediate = false)
		{
			if (this.isActive())
			{
				SER ser = this.id;
				if (ser <= SER.EATEN)
				{
					if (ser != SER.CONFUSE)
					{
						if (ser != SER.EATEN)
						{
							goto IL_00C8;
						}
						if (this.Mv is NelEnemy)
						{
							NelEnemy nelEnemy = this.Mv as NelEnemy;
							nelEnemy.getAnimator().TempStop.Rem("OD_EATEN");
							nelEnemy.killPtc("od_eaten_freeze", false);
							goto IL_00C8;
						}
						goto IL_00C8;
					}
				}
				else if (ser != SER.DRUNK)
				{
					if (ser != SER.WEB_TRAPPED)
					{
						goto IL_00C8;
					}
					this.Mv.PtcHld.killPtc("pr_web_trapped", false);
					goto IL_00C8;
				}
				this.Mv.PtcVar("z", (double)((int)(this.Mv.sizey * this.Mv.CLENM + 20f))).PtcST("ser_awake", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
				IL_00C8:
				this.Con.removeBit(this.id);
				this.af = X.Mn(-1f, -22f + this.af);
				this.level_count = 0;
				this.level = 0;
				this.releaseEffect(true);
				if (this.UiRow != null)
				{
					this.UiRow.hideProgress();
				}
				this.UiRow = null;
				if (immediate)
				{
					this.af = -22f;
				}
			}
		}

		public void setReleaseTime(int f)
		{
			if (this.isActive() && (float)this.maxt - this.af > (float)f)
			{
				this.maxt = (int)this.af + f;
			}
		}

		public void releasePtcST(bool do_not_kill_stock_effect = false)
		{
			this.checkPtcST();
			if (this.PtcStProcess != null)
			{
				this.PtcStProcess.kill(do_not_kill_stock_effect);
				this.PtcStProcess = null;
			}
		}

		public void checkPtcST()
		{
			if (this.PtcStProcess != null && this.PtcStProcess.id != this.ptc_id)
			{
				this.PtcStProcess = null;
			}
		}

		public bool run(float fcnt)
		{
			if (this.maxt == 0)
			{
				return false;
			}
			if (this.af >= 0f)
			{
				if (this.need_init)
				{
					this.Con.resetFlags();
					this.initSer();
				}
				int num = this.runSer(fcnt);
				if (num == 0)
				{
					this.deactivate(false);
				}
				else
				{
					if (num == -1)
					{
						this.af += fcnt * this.Con.progress_speed;
						bool flag = false;
						while (this.maxt >= 0 && this.af >= (float)this.maxt)
						{
							if (this.level == 0 || this.deactivate_on_level_down)
							{
								this.deactivate(false);
								return true;
							}
							if (this.level >= 2)
							{
								int num2 = (int)(this.level - 1);
								int num3 = (int)this.level_count;
								this.level = 0;
								for (int i = 0; i < num3; i++)
								{
									if (this.LevelCheckIndividual((int)(this.level_count = (byte)i), false, 99, false, true) && (int)this.level >= num2)
									{
										break;
									}
								}
								if (this.level == 0 || (int)this.level > num2)
								{
									this.level = 0;
									this.level_count = 0;
								}
							}
							else
							{
								if (this.level == 1)
								{
									this.level = 0;
									this.level_count = 0;
									this.need_init = true;
									flag = true;
									break;
								}
								this.level = 0;
								this.level_count = 0;
							}
							this.af -= (float)this.maxt;
							flag = true;
						}
						if (flag && UIStatus.isPr(this.Mv as PR))
						{
							UIStatus.Instance.levelupStatusImage(this, this.id, true);
						}
					}
					if (this.UiRow != null && this.UiRow.isHiding())
					{
						this.UiRow = null;
					}
					if (this.Con.apply_pe)
					{
						this.EfHn.fine(100);
					}
					else
					{
						this.EfHn.deactivate(false);
					}
					this.Con.pre_ser_bits |= 1UL << (int)this.id;
				}
			}
			else
			{
				this.af -= fcnt;
				if (this.Snd != null)
				{
					float num4 = X.ZLINE(-this.af, 22f);
					if (num4 >= 1f)
					{
						this.Snd.Stop();
						this.Snd = null;
					}
					else
					{
						this.Snd.setVolManual(1f - num4, true);
					}
				}
				if (this.af < -22f)
				{
					this.destruct();
					return false;
				}
			}
			return true;
		}

		public static int canApplySer(M2Ser Con, SER ser, M2Attackable Mv, NelM2Attacker Mva)
		{
			bool flag = false;
			if (ser <= SER.SHAMED_EP)
			{
				if (ser == SER.HP_REDUCE)
				{
					flag = Mv.is_alive && Mv.hp_ratio < 0.2f;
					goto IL_011D;
				}
				if (ser == SER.MP_REDUCE)
				{
					flag = Mv.mp_ratio < 0.15f;
					goto IL_011D;
				}
				if (ser == SER.SHAMED_EP)
				{
					flag = Mv is PR && !Con.has(SER.ORGASM_AFTER) && (Mv as PR).ep >= MDAT.Apr_ep_threshold[0];
					goto IL_011D;
				}
			}
			else if (ser <= SER.NEAR_PEE)
			{
				if (ser == SER.EGGED_2)
				{
					flag = Mv is PR && Con.has(SER.EGGED) && (Mv as PR).EggCon.isEgged2Active();
					goto IL_011D;
				}
				if (ser == SER.NEAR_PEE)
				{
					if (Mv is PR)
					{
						return (Mv as PR).JuiceCon.ser_near_pee_level;
					}
					goto IL_011D;
				}
			}
			else if (ser != SER.CLT_BROKEN)
			{
				if (ser == SER.DEATH)
				{
					flag = !Mv.is_alive;
					goto IL_011D;
				}
			}
			else
			{
				if (Mv is PR)
				{
					flag = (Mv as PR).BetoMng.is_torned;
					goto IL_011D;
				}
				goto IL_011D;
			}
			return -1;
			IL_011D:
			if (!flag || !Mva.canApplySer(ser))
			{
				return 0;
			}
			return 1;
		}

		private void initSer()
		{
			this.need_init = false;
			this.cannot_run = (this.cannot_evade = (this.weak_pose = (this.wet_pose = false)));
			this.punch_allow = true;
			this.overwrite_attach = false;
			this.xspeed_rate = (this.jump_rate = (this.mpgage_crack_rate = (this.chantmp_split_rate = (this.enemysink_rate = (this.gage_broken_split_rate = (this.hpdamage_rate = (this.chant_speed_rate = (this.atk_rate = (this.chant_atk_rate = (this.gacha_release_rate = (this.mana_drain_rate = (this.base_timescale = (this.stomach_apply_ratio = (this.burst_consume_ratio = 1f))))))))))))));
			this.orgasm_locked = false;
			this.ep_addition_ratio = 0f;
			this.frozen_state = NoelAnimator.FRZ_STATE.NORMAL;
			bool flag = false;
			SER ser = this.id;
			SER ser2 = ser;
			if (ser2 <= SER.DEATH)
			{
				switch ((uint)ser2)
				{
				case 0U:
					this.maxt = 52;
					break;
				case 1U:
					this.cannot_run = (this.cannot_evade = (this.weak_pose = true));
					this.xspeed_rate = 0.75f;
					this.mpgage_crack_rate = 3f;
					this.enemysink_rate = 0.5f;
					this.maxt = 102;
					this.gacha_release_rate = 0.875f;
					break;
				case 2U:
					this.wet_pose = true;
					if (this.Mv is PR)
					{
						this.maxt = ((this.level == 0) ? 1800 : 100);
					}
					this.mpgage_crack_rate = 0.75f;
					this.ep_addition_ratio = ((this.level == 0) ? 0.33f : 2.5f);
					this.mana_drain_rate = 1.25f;
					break;
				case 3U:
					this.maxt = 400;
					this.gacha_release_rate = 0.75f;
					if (this.Mv.M2D.isCenterPlayer(this.Mv))
					{
						IN.getCurrentKeyAssignObject().turn_lrtb_input = true;
					}
					break;
				case 5U:
					this.maxt = 550;
					this.overwrite_attach = true;
					this.weak_pose = true;
					this.gacha_release_rate = 0.75f;
					if (this.level >= 1)
					{
						this.gacha_release_rate = 0.6f;
						if (this.level >= 2)
						{
							this.gacha_release_rate = 0.45f;
							this.cannot_run = (this.cannot_evade = true);
						}
					}
					break;
				case 6U:
					this.cannot_run = (this.cannot_evade = (this.weak_pose = true));
					flag = 240 != this.maxt;
					this.maxt = 240;
					this.Con.Cure(SER.FROZEN);
					this.Con.Cure(SER.SLEEP);
					this.Con.Cure(SER.WEB_TRAPPED);
					break;
				case 7U:
					this.wet_pose = true;
					this.weak_pose = true;
					this.maxt = 30;
					break;
				case 8U:
				case 9U:
				case 10U:
					this.maxt = 900;
					this.enemysink_rate = 0.5f;
					this.chantmp_split_rate = 1.25f;
					this.wet_pose = true;
					this.weak_pose = true;
					this.mana_drain_rate = 1.25f;
					break;
				case 11U:
					this.maxt = 32;
					this.enemysink_rate = 0.5f;
					this.chantmp_split_rate = 1.5f;
					this.ep_addition_ratio = 0.11f;
					this.wet_pose = true;
					this.mana_drain_rate = 1.25f;
					break;
				case 12U:
					this.maxt = 1000;
					this.wet_pose = true;
					this.chant_atk_rate = 1.25f;
					this.chant_speed_rate = 0.85f;
					this.atk_rate = 0.5f;
					this.ep_addition_ratio = 0.15f;
					break;
				case 13U:
					this.maxt = 60;
					this.wet_pose = true;
					this.chantmp_split_rate = 1.5f;
					this.mpgage_crack_rate = 4f;
					this.ep_addition_ratio = 0.2f;
					break;
				case 14U:
					this.maxt = X.Mx(this.maxt, 60);
					this.wet_pose = true;
					this.punch_allow = false;
					this.burst_consume_ratio = 0f;
					this.gacha_release_rate = 0.125f;
					break;
				case 15U:
					this.maxt = 10;
					this.xspeed_rate = (this.gacha_release_rate = 0.25f);
					this.jump_rate = 0.25f;
					break;
				case 16U:
					this.maxt = 540;
					this.cannot_run = (this.cannot_evade = (this.weak_pose = true));
					this.gacha_release_rate = 0.5f;
					this.punch_allow = false;
					this.burst_consume_ratio = 0f;
					this.hpdamage_rate = 1.5f;
					this.mana_drain_rate = 0.25f;
					break;
				case 17U:
					this.maxt = 200;
					this.ep_addition_ratio = -0.75f;
					this.gacha_release_rate = 0.05f;
					this.mana_drain_rate = 0.05f;
					this.hpdamage_rate = 1.5f;
					this.punch_allow = false;
					this.burst_consume_ratio = 0f;
					break;
				case 18U:
					this.weak_pose = true;
					break;
				case 19U:
					if (this.Mv is PR)
					{
						this.maxt = X.Mx(this.maxt, 120);
						this.gacha_release_rate = 0.125f;
						this.mana_drain_rate = 0.01f;
						this.punch_allow = false;
						this.burst_consume_ratio = 0f;
						this.hpdamage_rate = 1.5f;
					}
					break;
				case 20U:
					if (this.Mv is NelEnemy)
					{
						NelEnemy nelEnemy = this.Mv as NelEnemy;
						this.maxt = (int)X.Mx((float)X.Abs(this.maxt), nelEnemy.stun_time);
						EnemyAnimator animator = nelEnemy.getAnimator();
						if (nelEnemy.showFlashEatenEffect(true) && !animator.TempStop.hasKey("OD_EATEN"))
						{
							animator.TempStop.Add("OD_EATEN");
							nelEnemy.PtcVar("sizex", (double)(nelEnemy.sizex * nelEnemy.CLENM)).PtcVar("sizey", (double)(nelEnemy.sizey * nelEnemy.CLENM)).PtcST("od_eaten_freeze", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
						}
					}
					else
					{
						this.maxt = X.Mx(this.maxt, 12);
					}
					break;
				case 21U:
					this.maxt = X.Mx(this.maxt, 12);
					break;
				case 22U:
					this.maxt = X.Mx(120, this.maxt);
					break;
				case 23U:
					this.maxt = 1200;
					this.mana_drain_rate = 3f;
					if (this.Con.has(SER.FORBIDDEN_ORGASM))
					{
						this.mana_drain_rate *= 4f;
					}
					break;
				case 24U:
					this.maxt = 540;
					this.cannot_run = (this.cannot_evade = (this.weak_pose = true));
					if (!this.Con.has(SER.EGGED_2))
					{
						this.mpgage_crack_rate = 0.0675f;
					}
					this.ep_addition_ratio = 0f;
					this.gacha_release_rate = 0.125f;
					this.wet_pose = true;
					this.mana_drain_rate = 0.05f;
					break;
				case 25U:
				{
					float num = X.ZLINE((float)this.level, 4f);
					this.maxt = (int)(60f * X.NI(9f, 4.5f, num));
					this.cannot_run = (this.weak_pose = true);
					this.xspeed_rate = 0.75f;
					if (!this.Con.has(SER.EGGED_2))
					{
						this.mpgage_crack_rate = 0.0675f;
					}
					this.ep_addition_ratio = -0.25f;
					this.wet_pose = true;
					this.gacha_release_rate = X.NI(0.8f, 0.6f, num);
					break;
				}
				case 26U:
					this.maxt = 900;
					break;
				case 27U:
					this.maxt = 40;
					this.chant_speed_rate = ((this.level == 0) ? 0.5f : ((this.level == 1) ? 0.33333334f : 0.25f));
					break;
				case 28U:
				case 35U:
					this.weak_pose = true;
					this.maxt = 500 + (int)this.level * 400;
					this.base_timescale = ((this.level == 0) ? 0.875f : ((this.level == 1) ? 0.75f : 0.5f));
					this.xspeed_rate = ((this.level == 0) ? 0.75f : ((this.level == 1) ? 0.3f : 0f)) / this.base_timescale;
					this.jump_rate = 0.75f;
					this.gacha_release_rate = ((this.level == 0) ? 0.6f : ((this.level == 1) ? 0.3f : 0.05f));
					this.chant_speed_rate = ((this.level == 0) ? 0.6666667f : ((this.level == 1) ? 0.33333334f : 0f));
					if (this.level >= 2)
					{
						this.punch_allow = false;
					}
					if (this.level >= 1)
					{
						this.jump_rate = 0.65f;
						this.cannot_run = (this.cannot_evade = (this.weak_pose = true));
					}
					if (this.id == SER.FROZEN)
					{
						this.Con.Cure(SER.BURNED);
						if (this.level >= 2)
						{
							this.frozen_state |= NoelAnimator.FRZ_STATE.FROZEN;
						}
					}
					else if (this.level >= 2)
					{
						this.chantmp_split_rate *= 2f;
						this.burst_consume_ratio = 0f;
						this.mana_drain_rate *= 0.25f;
						this.frozen_state |= NoelAnimator.FRZ_STATE.STONE;
						if (this.level >= 3)
						{
							this.frozen_state |= NoelAnimator.FRZ_STATE.STONEOVER;
						}
					}
					else
					{
						this.chantmp_split_rate *= 1.5f;
					}
					if (this.Mv is PR)
					{
						(this.Mv as PR).fineFrozenAppearance();
						if (this.id == SER.FROZEN)
						{
							this.maxt = (int)X.NIL(1600f, 3200f, (float)this.level, 2f);
						}
						else
						{
							if (this.level >= 2)
							{
								this.setPE(POSTM.STONEOVER, 60f, 0f, 0);
							}
							else
							{
								this.EfHn.deactivateSpecific(POSTM.STONEOVER, false);
							}
							this.maxt = (int)X.NIL(1600f, 1800f, (float)this.level, 2f);
						}
					}
					else
					{
						this.maxt = 480;
					}
					break;
				case 29U:
					this.mpgage_crack_rate = 3f;
					this.enemysink_rate = 1.5f;
					this.maxt = 32;
					this.gacha_release_rate = 1.5f;
					if (this.level >= 1)
					{
						this.wet_pose = true;
					}
					break;
				case 30U:
					this.maxt = 120;
					this.gacha_release_rate = X.Mx(0.1f, 0.7f - (float)this.level * 0.2f);
					this.mpgage_crack_rate = 0.66f;
					this.mana_drain_rate = 0.5f;
					this.stomach_apply_ratio = ((this.level == 0) ? 1f : ((float)(100 - X.Mn((int)(this.level * 10), 90)) / 100f));
					break;
				case 31U:
					this.maxt = 60;
					this.weak_pose = true;
					this.ep_addition_ratio = 1.1f;
					if (CFGSP.cloth_broken_debuff)
					{
						this.hpdamage_rate = 1.5f;
					}
					break;
				case 32U:
					this.maxt = X.Mx(4000, this.maxt);
					this.gacha_release_rate = X.NIL(0.8f, 0.4f, (float)this.level, 5f);
					this.chant_speed_rate = X.NIL(1f, 0.75f, (float)(this.level + 1), 6f);
					this.hpdamage_rate = X.NIL(1.25f, 2f, (float)this.level, 8f);
					this.mpgage_crack_rate = X.NIL(1.5f, 5f, (float)this.level, 8f);
					this.enemysink_rate = X.NIL(2f, 4f, (float)this.level, 5f);
					this.weak_pose = true;
					break;
				case 33U:
					this.maxt = X.Mx(150, this.maxt);
					this.weak_pose = true;
					break;
				case 34U:
					this.weak_pose = true;
					this.punch_allow = false;
					this.cannot_run = true;
					this.maxt = 200;
					this.jump_rate = 0.75f;
					this.xspeed_rate = ((this.level == 0) ? 0.4f : ((this.level == 1) ? 0.25f : 0.125f));
					this.burst_consume_ratio = 1f + (float)this.level * 0.125f;
					this.gacha_release_rate = ((this.level == 0) ? 0.66f : ((this.level == 1) ? 0.55f : 0.333f));
					this.chant_speed_rate = 0.75f;
					if (this.Mv is PR)
					{
						(this.Mv as PR).fineFrozenAppearance();
					}
					break;
				case 36U:
					this.maxt = X.Mx(30, this.maxt);
					this.weak_pose = true;
					break;
				case 37U:
					this.punch_allow = false;
					this.burst_consume_ratio = 0f;
					break;
				}
			}
			this.finePtcProcess(flag);
			if (this.maxt < 0)
			{
				this.maxt = -this.maxt;
			}
		}

		public void finePtcProcess(bool initialize)
		{
			this.checkPtcST();
			SER ser = this.id;
			ulong num = ser - SER.CONFUSE;
			if (num <= 3UL)
			{
				switch ((uint)num)
				{
				case 0U:
					this.setProcessPtcSt("process_ser_confuse");
					return;
				case 1U:
					return;
				case 2U:
					this.setProcessPtcSt("process_ser_paralysis");
					return;
				case 3U:
					this.ef_time = 60f;
					if (this.PtcStProcess != null)
					{
						return;
					}
					if (this.Mv is PR)
					{
						PR pr = this.Mv as PR;
						pr.playSndPos("burned_juu_one", 1);
						if (!pr.getAbsorbContainer().no_ser_burned_effect)
						{
							this.setProcessPtcSt("process_ser_burned");
							return;
						}
						return;
					}
					else
					{
						if (this.Mv is NelEnemy)
						{
							this.setProcessPtcSt("process_ser_burned_en");
							return;
						}
						this.setProcessPtcSt("process_ser_burned_en");
						return;
					}
					break;
				}
			}
			if (ser == SER.SLEEP)
			{
				this.setProcessPtcSt("process_ser_sleep");
				return;
			}
			if (ser != SER.FROZEN)
			{
				return;
			}
			this.setProcessPtcSt("process_ser_frozen");
		}

		private bool LevelCheckIndividual(int _levelcnt, bool fine_ui = true, int max_level = 99, bool adding = false, bool fine_log_row = true)
		{
			bool flag = false;
			SER ser = this.id;
			ulong num = ser - SER.SEXERCISE;
			if (num <= 5UL)
			{
				switch ((uint)num)
				{
				case 0U:
					if (this.level < 1 && max_level >= 1 && _levelcnt >= 14)
					{
						this.level = 1;
						flag = (this.need_init = true);
						goto IL_05CC;
					}
					goto IL_05CC;
				case 1U:
					if (this.level < 1 && max_level >= 1 && _levelcnt >= 3)
					{
						this.level = 1;
						flag = true;
					}
					if (this.level < 2 && max_level >= 2 && _levelcnt >= 7)
					{
						this.level = 2;
						flag = true;
					}
					if (flag && this.level >= 0)
					{
						if (this.EfHn.Count == 0)
						{
							this.EfHn.Set(PostEffect.IT.setPE(POSTM.CONFUSED_CAMERA, 60f, 0.5f, 0));
							this.EfHn.Set(PostEffect.IT.setPE(POSTM.FINAL_ALPHA, 180f, 0.5f, 0));
						}
						this.EfHn.setXLevel(POSTM.CONFUSED_CAMERA, (this.level <= 1) ? 0.5f : 1f);
						this.EfHn.setXLevel(POSTM.FINAL_ALPHA, (this.level == 2) ? 0.5f : 0.25f);
						goto IL_05CC;
					}
					goto IL_05CC;
				case 2U:
				case 4U:
					goto IL_05CC;
				case 3U:
					if (this.level < 1 && max_level >= 1 && _levelcnt >= 5)
					{
						this.level = 1;
						flag = true;
					}
					if (this.level < 2 && max_level >= 2 && _levelcnt >= 10)
					{
						this.level = 2;
						flag = (this.need_init = true);
						goto IL_05CC;
					}
					goto IL_05CC;
				case 5U:
					if (max_level >= 1 && max_level < 3 && (int)this.level < max_level)
					{
						this.level = (byte)max_level;
						flag = true;
						goto IL_05CC;
					}
					goto IL_05CC;
				}
			}
			ulong num2 = ser - SER.ORGASM_AFTER;
			if (num2 <= 11UL)
			{
				switch ((uint)num2)
				{
				case 0U:
					if (max_level >= 1 && max_level < 99 && (int)this.level < max_level)
					{
						if (this.level < 5)
						{
							this.need_init = true;
						}
						this.level = (byte)max_level;
						flag = true;
					}
					break;
				case 3U:
					if (this.level < 1 && max_level >= 1 && _levelcnt >= 2)
					{
						this.level = 1;
						flag = (this.need_init = true);
					}
					if (this.level < 2 && max_level >= 2 && _levelcnt >= 3)
					{
						this.level = 2;
						flag = (this.need_init = true);
					}
					break;
				case 4U:
					if (this.level < 2 && max_level >= 2 && _levelcnt >= 2)
					{
						this.level = (byte)X.Mn(2, max_level);
						flag = true;
					}
					else if (this.level < 1 && max_level >= 1 && _levelcnt >= 1)
					{
						this.level = (byte)X.Mn(1, max_level);
						flag = true;
					}
					this.level_count = X.Mn(this.level, this.level_count);
					break;
				case 5U:
				{
					int num3 = X.Mn(X.Mn((int)this.level_count, max_level), 8);
					if ((int)this.level < num3)
					{
						this.level = (byte)num3;
						flag = true;
						this.need_init = true;
					}
					if (flag && this.level > 0)
					{
						if (this.EfHn.Count == 0)
						{
							this.EfHn.Set(PostEffect.IT.setPE(POSTM.CONFUSED_CAMERA, 60f, 0.05f, 0));
							this.EfHn.Set(PostEffect.IT.setPE(POSTM.FINAL_ALPHA, 180f, 0.75f, 0));
						}
						this.EfHn.setXLevel(POSTM.FINAL_ALPHA, (this.level >= 2) ? 0.75f : 0.25f);
					}
					break;
				}
				case 7U:
					this.level_count = (byte)X.Mn((int)this.level_count, X.Mn(max_level, 8));
					if (this.level_count > this.level)
					{
						this.level = this.level_count;
						flag = (this.need_init = true);
					}
					break;
				case 9U:
					if (this.level < 1 && max_level >= 1 && _levelcnt >= 3)
					{
						this.level = 1;
						flag = (this.need_init = true);
					}
					if (this.level < 2 && max_level >= 2 && _levelcnt >= 6)
					{
						this.level = 2;
						flag = (this.need_init = true);
					}
					break;
				case 10U:
					if (this.level < 1 && max_level >= 1 && _levelcnt >= 3)
					{
						this.level = 1;
						flag = (this.need_init = true);
					}
					if (this.level < 2 && max_level >= 2 && _levelcnt >= 5)
					{
						this.level = 2;
						flag = (this.need_init = true);
					}
					if (this.Con.apply_pe && adding && max_level >= 3 && _levelcnt > 5 && _levelcnt < 20 && this.Mv != null)
					{
						this.Mv.PtcST("stone_ser_crack", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					if (this.level < 3 && max_level >= 3 && _levelcnt >= 20)
					{
						this.level = 3;
						flag = (this.need_init = true);
						if (adding && this.Mv is PR)
						{
							(this.Mv as PR).PtcST("stone_ser_complete", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						}
					}
					if (adding && !this.Mv.is_alive && this.Mv is PR && this.level >= 2 && this.Con.apply_pe)
					{
						PR pr = this.Mv as PR;
						if ((pr.UP.getCurrentState() & UIPictureBase.EMSTATE.STONEOVER) == UIPictureBase.EMSTATE.NORMAL)
						{
							string currentFadeKey = pr.UP.getCurrentFadeKey(true);
							pr.UP.setFade(TX.noe(currentFadeKey) ? "damage" : currentFadeKey, UIPictureBase.EMSTATE.NORMAL, true, true, false);
						}
					}
					break;
				case 11U:
					this.level_count = (byte)X.Mn((int)this.level_count, X.Mn(max_level, 99));
					break;
				}
			}
			IL_05CC:
			if (fine_ui && flag && UIStatus.isPr(this.Mv as PR))
			{
				UIStatus.Instance.levelupStatusImage(this, this.id, false);
				if (fine_log_row)
				{
					this.fineLogRow();
				}
			}
			return flag;
		}

		public bool CureableOnBench()
		{
			SER ser = this.id;
			return ser == SER.FROZEN || ser - SER.WEB_TRAPPED <= 1UL || this.runSer(0f) <= 0;
		}

		private int runSer(float fcnt)
		{
			int num = -1;
			SER ser = this.id;
			SER ser2 = ser;
			if (ser2 <= SER.DEATH)
			{
				switch ((uint)ser2)
				{
				case 0U:
				case 1U:
				case 37U:
					if (this.canApplySer() == 1)
					{
						this.af = X.Mn(this.af, 22f);
						num = 1;
					}
					else if (this.af >= (float)this.maxt)
					{
						return 0;
					}
					if (this.Con.apply_pe && !this.EfHn.isActive())
					{
						if (this.id == SER.DEATH)
						{
							this.setPE(POSTM.HP_REDUCE, 40f, 1f, 0);
						}
						else
						{
							this.EfHn.Set(this.setPE((this.id == SER.MP_REDUCE) ? POSTM.MP_REDUCE : POSTM.HP_REDUCE, 150f, 1f, 20));
						}
					}
					break;
				case 2U:
					if (this.Mv is PR && this.level >= 1)
					{
						PR pr = this.Mv as PR;
						if (pr.isMasturbateState() || this.Con.has(SER.ORGASM_AFTER) || this.Con.has(SER.ORGASM_INITIALIZE) || (float)pr.ep >= 450f)
						{
							this.af = X.Mn(this.af, 4f);
							num = 1;
						}
					}
					break;
				case 3U:
					if (this.Mv.M2D.isCenterPlayer(this.Mv))
					{
						IN.getCurrentKeyAssignObject().turn_lrtb_input = true;
					}
					if (this.level == 0 && this.EfHn.Count > 0)
					{
						this.releaseEffect(true);
					}
					break;
				case 5U:
					if (this.Mv is PR)
					{
						PR pr2 = this.Mv as PR;
						if (!EnemySummoner.isActiveBorder() && pr2.is_alive)
						{
							this.ef_time = 0f;
							this.af = X.Mx(this.af, (float)(this.maxt - 20));
						}
						else
						{
							if (this.level >= 2 || pr2.isAbsorbState())
							{
								this.af = X.Mn(this.af, 1f);
								num = 1;
							}
							if (pr2.canApplyParalysisAttack())
							{
								this.ef_time += fcnt;
								if (this.ef_time >= (float)M2SerItem.Aparalysis_stop_time[(int)this.level])
								{
									this.ef_time = 0f;
									pr2.applyParalysisDamage();
								}
							}
							else
							{
								this.ef_time = X.Mn(this.ef_time, (float)(M2SerItem.Aparalysis_stop_time[(int)this.level] - 90));
							}
						}
					}
					break;
				case 6U:
					if (this.Mv is PR)
					{
						PR pr3 = this.Mv as PR;
						if (!pr3.isBurnedState())
						{
							if (pr3.TeCon != null && !pr3.TeCon.existSpecific(TEKIND.DMG_BLINK))
							{
								pr3.DMGE.setBurnedEffect(false, !pr3.EggCon.isLaying() && (!pr3.isDamagingOrKo() || X.XORSP() < 0.0625f), false);
							}
							if (pr3.isBenchOrGoRecoveryState())
							{
								return 0;
							}
							this.ef_time -= fcnt;
							if (this.ef_time <= 0f)
							{
								this.ef_time += 50f;
								pr3.applyDamage(MDAT.AtkSlipDmgBurnedPr, true);
							}
						}
					}
					else if (this.Mv is NelEnemy)
					{
						this.ef_time -= fcnt;
						if (this.ef_time <= 0f)
						{
							this.ef_time += 30f;
							(this.Mv as NelEnemy).applySlipDamage(6, 0, true, MGATTR.FIRE);
						}
					}
					break;
				case 7U:
					if (this.Mv is PR && (this.Mv as PR).getYdrgLevel() >= (int)this.level)
					{
						num = 1;
					}
					break;
				case 11U:
				case 13U:
					if (this.canApplySer() == 1)
					{
						this.af = X.Mn(this.af, 22f);
						num = 1;
					}
					else if (this.af >= (float)this.maxt)
					{
						return 0;
					}
					break;
				case 12U:
					if (this.maxt >= 1000)
					{
						if (this.af > 4f)
						{
							this.af = 4f;
						}
						num = 1;
					}
					break;
				case 16U:
					if (this.Mv.isDamagingOrKo())
					{
						num = 1;
					}
					break;
				case 19U:
					if (this.Mv is PR && (this.Mv as PR).isAbsorbState())
					{
						num = 1;
					}
					break;
				case 20U:
					if (this.Mv is PR)
					{
						if (this.Mv.M2D.isCenterPlayer(this.Mv))
						{
							if (this.Snd == null)
							{
								this.Snd = this.Mv.M2D.Snd.play("ser_eaten");
							}
							if (!this.EfHn.isActive())
							{
								this.setPE(POSTM.ZOOM2_EATEN, 40f, 1f, 0);
							}
						}
						else
						{
							if (this.Snd != null)
							{
								this.Snd.Stop();
								this.Snd = null;
							}
							if (this.EfHn.isActive())
							{
								this.EfHn.deactivate(false);
							}
						}
						if (this.af > 4f)
						{
							this.af = 4f;
						}
						if (!(this.Mv as PR).isAbsorbState())
						{
							return 0;
						}
						num = 1;
					}
					break;
				case 21U:
					if (this.Mv.M2D.isCenterPlayer(this.Mv))
					{
						if (!this.EfHn.isActive())
						{
							this.setPE(POSTM.ZOOM2_EATEN, 40f, 1f, 0);
						}
					}
					else if (this.EfHn.isActive())
					{
						this.EfHn.deactivate(false);
					}
					if (this.Mv is PR)
					{
						if (this.af > 4f)
						{
							this.af = 4f;
						}
						if (!(this.Mv as PR).isAbsorbState())
						{
							return 0;
						}
						num = 1;
					}
					break;
				case 23U:
					if (this.Mv is PR && (this.Mv as PR).isMasturbateState())
					{
						this.af = X.Mn(this.af, 4f);
						num = 1;
					}
					break;
				case 25U:
					if (this.af > 4f && this.Mv is PR && (this.Mv as PR).EpCon.hold_orgasm_after)
					{
						this.af = X.Mn(this.af, 4f);
						num = 1;
					}
					break;
				case 27U:
					if (this.Con.apply_pe && !this.EfHn.isActive())
					{
						this.setPE(POSTM.JAMMING, 30f, 1f, 0);
					}
					break;
				case 29U:
					if (this.level >= 2)
					{
						if (this.af >= 60f)
						{
							return 0;
						}
					}
					else if (this.canApplySer() >= 1)
					{
						this.af = X.Mn(this.af, 22f);
						num = 1;
					}
					else if (this.af >= (float)this.maxt)
					{
						return 0;
					}
					break;
				case 30U:
					if (this.level == 0)
					{
						return -1;
					}
					if (this.ef_time <= -100f)
					{
						this.af = (float)this.maxt;
						this.ef_time += 100f;
						return -1;
					}
					if (this.level >= 2 && this.Mv is PR)
					{
						PR pr4 = this.Mv as PR;
						if (this.ef_time < 100f)
						{
							this.ef_time = 1000f - X.NIXP(160f, 350f);
						}
						if (pr4.isNormalState() && !pr4.isMoveScriptActive(false) && Map2d.can_handle && !pr4.on_ladder)
						{
							this.ef_time += fcnt;
							if (this.ef_time >= 1000f && pr4.hasFoot())
							{
								this.ef_time = 0f;
								pr4.addEnemySink(100f, true, 1f);
								float walk_xspeed = pr4.get_walk_xspeed();
								if (walk_xspeed != 0f)
								{
									pr4.getPhysic().addFoc(FOCTYPE.WALK, walk_xspeed * 0.45f, 0f, -1f, 0, 2, 30, 12, 0);
								}
							}
						}
						else if (this.ef_time >= 800f)
						{
							this.ef_time = 800f;
						}
					}
					return 1;
				case 31U:
					if (this.Mv as PR && (this.Mv as PR).BetoMng.is_torned)
					{
						this.af = 0f;
						return 1;
					}
					break;
				case 33U:
					if (!EV.isActive(false))
					{
						return 1;
					}
					break;
				case 34U:
					if (this.Mv is PR)
					{
						num = ((this.maxt == 1 || this.af >= (float)this.maxt) ? 0 : 1);
					}
					break;
				case 35U:
					if (this.Mv as PR)
					{
						PR pr5 = this.Mv as PR;
						if (this.maxt > 1)
						{
							if (this.af >= (float)this.maxt)
							{
								pr5.fineFrozenAppearance();
								return 0;
							}
							if (this.level >= 2)
							{
								PostEffectItem byType = this.EfHn.getByType(POSTM.STONEOVER);
								if (byType != null)
								{
									byType.x = X.VALWALK(byType.x, (float)(this.level_count - 5) * 0.06666667f, 0.008f);
								}
								if (this.level >= 3 && pr5.is_alive)
								{
									pr5.applyHpDamage(9999, true, null);
									pr5.GSaver.reduceHp(9999, false, true);
									pr5.GSaver.reduceMp(9999, false, true);
									(this.Mv as PR).DMGE.PtcSTDead(null, true);
								}
							}
							return 1;
						}
					}
					return -1;
				case 36U:
					if (this.Mv is PR)
					{
						PR pr6 = this.Mv as PR;
						if (!pr6.isOrgasmLocked(true))
						{
							if (this.af < (float)(this.maxt - 10))
							{
								return -1;
							}
							if ((float)pr6.ep < 400f)
							{
								return 0;
							}
							bool flag;
							if (this.ef_time < 0f)
							{
								this.ef_time = X.Mn(this.ef_time + fcnt, 0f);
								flag = this.ef_time >= 0f;
							}
							else
							{
								this.ef_time += fcnt;
								flag = this.ef_time >= 3f;
								if (this.UiRow != null)
								{
									this.UiRow.hold();
								}
							}
							if (flag)
							{
								bool flag2 = this.ef_time <= 0f;
								this.ef_time = 0f;
								if (flag2)
								{
									if (this.UiRow != null)
									{
										this.UiRow.hideProgress();
									}
									this.UiRow = UILog.Instance.AddAlert(TX.Get("EP_preparing_orgasm_stack", ""), UILogRow.TYPE.ALERT_EP2);
								}
								bool flag3;
								if (!pr6.EpCon.progressOrgasmStack(flag2, out flag3))
								{
									return 0;
								}
								if (flag3)
								{
									if (this.level_count == 0)
									{
										this.UiRow = null;
										return 0;
									}
									this.level_count -= 1;
								}
							}
						}
						else
						{
							this.ef_time = X.Mn(this.ef_time, -80f);
							this.af = 0f;
						}
						return 1;
					}
					break;
				}
			}
			return num;
		}

		public int getIconImageId()
		{
			SER ser = this.id;
			ulong num = ser - SER.MP_REDUCE;
			if (num <= 34UL)
			{
				switch ((uint)num)
				{
				case 0U:
					return 1;
				case 1U:
					if (this.level != 0)
					{
						return 34;
					}
					return 5;
				case 2U:
					if (this.level == 0)
					{
						return 10;
					}
					if (this.level != 1)
					{
						return 18;
					}
					return 17;
				case 3U:
					if (this.level == 0)
					{
						return 11;
					}
					if (this.level != 1)
					{
						return 13;
					}
					return 12;
				case 4U:
					if (this.level == 0)
					{
						return 19;
					}
					if (this.level != 1)
					{
						return 21;
					}
					return 20;
				case 5U:
					return 15;
				case 6U:
					if (this.level == 0)
					{
						return 24;
					}
					if (this.level != 1)
					{
						return 26;
					}
					return 25;
				case 7U:
				case 9U:
					return 3;
				case 10U:
					return 4;
				case 11U:
					return 6;
				case 12U:
					return 14;
				case 15U:
					return 7;
				case 16U:
					return 16;
				case 18U:
					return 27;
				case 22U:
					return 22;
				case 24U:
					if (this.level != 0)
					{
						return 8;
					}
					return 9;
				case 25U:
					return 23;
				case 26U:
					if (this.level == 0)
					{
						return 28;
					}
					if (this.level != 1)
					{
						return 30;
					}
					return 29;
				case 27U:
					if (this.level == 0)
					{
						return 31;
					}
					if (this.level != 1)
					{
						return 33;
					}
					return 32;
				case 28U:
					if (this.level != 0)
					{
						return 36;
					}
					return 35;
				case 29U:
					if (this.level > 1)
					{
						return 38;
					}
					return 37;
				case 30U:
					return 40;
				case 31U:
					return 39;
				case 33U:
					if (this.level == 0)
					{
						return 41;
					}
					if (this.level != 1)
					{
						return 43;
					}
					return 42;
				case 34U:
					if (this.level == 0)
					{
						return 44;
					}
					if (this.level == 1)
					{
						return 45;
					}
					if (this.level != 2)
					{
						return 47;
					}
					return 46;
				}
			}
			return -1;
		}

		public bool deactivate_on_level_down
		{
			get
			{
				SER ser = this.id;
				return ser == SER.SEXERCISE || ser == SER.FROZEN;
			}
		}

		public void progressTime(float _af)
		{
			this.af = X.Mn((float)this.maxt, this.af + _af);
		}

		public bool TimeIs(float _af)
		{
			return this.af == _af;
		}

		public float getAf()
		{
			return this.af;
		}

		public PxlFrame getIconImage()
		{
			int iconImageId = this.getIconImageId();
			if (iconImageId >= 0)
			{
				return MTRX.AUiSerIcon[iconImageId];
			}
			return null;
		}

		public string getTitle(bool with_title = false, bool level_sizing = true)
		{
			int num = (int)this.level;
			SER ser = this.id;
			if (ser <= SER.DRUNK)
			{
				if (ser != SER.HP_REDUCE)
				{
					if (ser == SER.DRUNK)
					{
						num = X.Mx(num - 1, 0);
					}
				}
				else if (!this.Mv.is_alive)
				{
					return "";
				}
			}
			else if (ser != SER.STONE)
			{
				if (ser == SER.ORGASM_STACK)
				{
					num = (int)this.level_count;
				}
			}
			else if (num >= 3)
			{
				return (with_title ? this.icon_html : "") + TX.Get("SerTitle_stone_complete", "");
			}
			string text = (with_title ? this.icon_html : "");
			text += TX.Get("SerTitle_" + this.id.ToString().ToLower(), "");
			if (num > 0)
			{
				text = text + " " + TX.GetA(level_sizing ? "Stat_Lv_Sizing" : "Stat_Lv", (num + 1).ToString() ?? "");
			}
			return text;
		}

		public string icon_html
		{
			get
			{
				return this.getIconHtml(this.getIconImageId());
			}
		}

		private string getIconHtml(int _id)
		{
			if (_id < 0)
			{
				return "";
			}
			return "<img mesh=\"AUiSerIcon." + _id.ToString() + "\" size=\"2\" margin=\"4\" />";
		}

		public STB getDesc(STB Stb)
		{
			SER ser = this.id;
			ulong num = ser - SER.JAMMING;
			if (num <= 8UL)
			{
				switch ((uint)num)
				{
				case 0U:
					Stb.AddTxA("SerDesc_" + this.id.ToString().ToLower() + ((this.level >= 2) ? "_3" : ((this.level >= 1) ? "_2" : "")), false);
					if (this.id == SER.FROZEN)
					{
						Stb.Ret("\n");
						Stb.AddTxA("SerDesc_frozen_suffix", false);
					}
					return Stb;
				case 1U:
				case 8U:
					if (this.level == 3 && this.id == SER.STONE)
					{
						Stb.AddTxA("SerDesc_stone_complete_suffix", false);
					}
					else
					{
						Stb.AddTxA("SerDesc_frozen" + ((this.level >= 2) ? "_3" : ((this.level >= 1) ? "_2" : "")), false);
						if (this.id == SER.FROZEN)
						{
							Stb.Ret("\n");
							Stb.AddTxA("SerDesc_frozen_suffix", false);
						}
					}
					return Stb;
				case 3U:
					Stb.AddTxA("SerDesc_drunk", false).Ret("\n");
					Stb.AddTxA("SerDesc_drunk_1", false).TxRpl(X.Mn(90, (int)(10 * this.level))).Ret("\n");
					if (this.level >= 2)
					{
						Stb.Add("(", TX.GetA("Stat_Lv", "2"), ") ");
						Stb.AddTxA("SerDesc_drunk_2", false).Ret("\n");
					}
					Stb.AddTxA("SerDesc_drunk_0", false);
					if (this.level >= 3)
					{
						Stb.Ret("\n");
						Stb.AddTxA("SerDesc_drunk_3", false);
					}
					return Stb;
				case 4U:
					Stb.AddTxA("SerDesc_clt_broken", false);
					if (CFGSP.cloth_broken_debuff)
					{
						Stb.Ret("\n");
						Stb.AddTxA("SerDesc_clt_broken_detail", false);
					}
					return Stb;
				}
			}
			string text = "SerDesc_" + this.id.ToString().ToLower();
			Stb.AddTxA(text, false);
			for (int i = 1; i <= (int)this.level; i++)
			{
				string text2 = TX.Get(text + "_" + (i + 1).ToString(), "");
				if (TX.valid(text2))
				{
					Stb.Ret("\n").Add("(", TX.GetA("Stat_Lv", (i + 1).ToString() ?? ""), ") ").Add(text2);
				}
			}
			return Stb;
		}

		public void setProcessPtcSt(string key)
		{
			this.checkPtcST();
			if (this.PtcStProcess != null)
			{
				if (this.PtcStProcess.key == key)
				{
					return;
				}
				this.PtcStProcess.kill(false);
			}
			this.PtcStProcess = this.Mv.PtcST(key, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
			this.ptc_id = this.PtcStProcess.id;
		}

		public float time_level_ui
		{
			get
			{
				if (this.isActive())
				{
					return (float)this.level + ((this.maxt < 0) ? 1f : X.ZLINE((float)this.maxt - this.af, (float)X.Mn(180, this.maxt)));
				}
				return 0f;
			}
		}

		public void drinkingOnWater()
		{
			if (this.id == SER.DRUNK && this.level_count >= 1)
			{
				if (this.ef_time > -100f)
				{
					this.ef_time = -100f;
					return;
				}
				this.ef_time -= 100f;
			}
		}

		public void orgasmStackLockProgress(float delay)
		{
			if (this.id == SER.ORGASM_STACK)
			{
				this.ef_time = X.Mn(X.Mn(-delay, -1f), this.ef_time);
			}
		}

		public void readBinaryFrom(ByteReader Ba, bool read_level = true)
		{
			this.af = Ba.readFloat();
			this.maxt = Ba.readInt();
			if (read_level)
			{
				this.level = (this.level_count = 0);
				int num = (int)Ba.readUByte();
				if (this.isActive() && num > 0)
				{
					this.LevelCheck(num, 99);
				}
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			if (!this.isActive())
			{
				Ba.writeFloat(0f);
				Ba.writeInt(0);
			}
			else
			{
				Ba.writeFloat(this.af);
				Ba.writeInt(this.maxt);
			}
			Ba.writeByte((int)this.level_count);
		}

		public const int sleep_cure_on_large_damage = 80;

		public const int sleep_cure_on_small_damage = 30;

		public const int drunk_reduce_stomach_power100 = 10;

		public const int drunk_reduce_stomach_power100_max = 90;

		public const int drunk_cure_on_drinking_water_ratio100 = 15;

		public const int UI_CONFUSION_LEVEL = 2;

		public const int OVERRUN_TIRED_MAX_LEVEL = 8;

		public static readonly int[] Aparalysis_stop_time = new int[] { 170, 150, 140 };

		public readonly M2Ser Con;

		public const float XSPD_MP_REDUCE = 0.75f;

		public SER id;

		public float af;

		public float ef_time;

		public int maxt;

		public const int FADE_T = 22;

		public EffectHandlerPE EfHn;

		public M2SoundPlayerItem Snd;

		public TransEffecterItem Te;

		public UILogRow UiRow;

		public bool need_init;

		public PTCThread PtcStProcess;

		public uint ptc_id;

		private byte level_count;

		public byte level;

		public bool cannot_evade;

		public bool cannot_run;

		public bool weak_pose;

		public bool wet_pose;

		public float xspeed_rate = 1f;

		public float jump_rate = 1f;

		public float hpdamage_rate = 1f;

		public float mpgage_crack_rate = 1f;

		public float chantmp_split_rate = 1f;

		public float enemysink_rate = 1f;

		public float atk_rate = 1f;

		public float chant_speed_rate = 1f;

		public float gacha_release_rate = 1f;

		public float chant_atk_rate = 1f;

		public float burst_consume_ratio = 1f;

		public float gage_broken_split_rate = 1f;

		public float ep_addition_ratio;

		public float mana_drain_rate = 1f;

		public float base_timescale = 1f;

		public float stomach_apply_ratio = 1f;

		public bool punch_allow = true;

		public bool orgasm_locked;

		public const int shield_break_maxt = 540;

		public bool overwrite_attach;

		public NoelAnimator.FRZ_STATE frozen_state;
	}
}
