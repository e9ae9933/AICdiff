using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NAI
	{
		public NAI(NelEnemy _En)
		{
			this.En = _En;
			this.ATicket = new NaTicket[3];
			this.FirstPos = new Vector2(_En.x, _En.y);
			this.CFlags = new FlagCounterR<NAI.FLAG>();
			this.CTypeLock = new FlagCounterR<NAI.TYPE>();
			this.DeclCon = new TimeDeclineAreaContainer(4);
			this.ran_n = X.xors();
			this.ran_a = X.xors();
		}

		public NAI initWalkRegionAir()
		{
			if (this.En.Summoner != null)
			{
				this.WRC_Air = this.En.Summoner.Summoner.getRegionCheckerAir();
			}
			return this;
		}

		public NAI initDummy()
		{
			this.RayDummy = this.En.nM2D.MGC.makeRay(this.En, false, true, false, 0f, false, false);
			this.RayDummy.projectile_power = -1;
			this.RayDummy.hittype_to_week_projectile = HITTYPE.NONE;
			return this;
		}

		public void destruct()
		{
			try
			{
				this.spliceTicket(0, this.ticket_cnt, true);
			}
			catch
			{
			}
			if (this.RayDummy != null)
			{
				this.En.nM2D.MGC.destructRay(this.RayDummy);
				this.RayDummy = null;
			}
			this.AimPr = null;
		}

		public void consider(float fcnt)
		{
			if (this.AimPr_ == null && this.En.is_alive)
			{
				Bench.P("Consider Sleep");
				if (this.delay >= 0f)
				{
					if (this.fnSearchPlayer == null)
					{
						this.fnSearchPlayer = new NAI.FnSearchPlayer(this.SearchPlayerDefault);
					}
					M2Attackable m2Attackable = this.fnSearchPlayer((float)(this.CFlags.Has(NAI.FLAG.SUMMON_APPEARED) ? 9999 : 0));
					if (m2Attackable != null)
					{
						if (!this.CFlags.Has(NAI.FLAG.AWAKEN))
						{
							this.delay = 0f;
						}
						this.CFlags.Add(NAI.FLAG.AWAKEN, 180f);
						this.delay += fcnt;
						if (this.delay >= 14f || this.CFlags.Has(NAI.FLAG.SUMMON_APPEARED))
						{
							this.awakeInit(m2Attackable);
						}
					}
					else
					{
						this.CFlags.Rem(NAI.FLAG.AWAKEN);
						this.delay = (float)(-14 - (this.En.index + (int)this.Mp.floort) % 11);
					}
				}
				else
				{
					this.delay = X.Mn(0f, this.delay + fcnt);
				}
				if (this.AimPr_ == null && (!this.consider_only_onfoot || this.En.hasFoot()))
				{
					this.considerSleepLogicBase();
				}
				Bench.Pend("Consider Sleep");
			}
			if (this.AimPr_ != null)
			{
				Bench.P("Consider normal");
				this.garaaki_state = 2;
				Bench.P("Consider normal 1");
				if (this.delay > 0f)
				{
					if (this.ticket_cnt == 0 || this.can_progress_delay_if_ticket_exists)
					{
						this.delay = X.Mx(0f, this.delay - fcnt);
					}
					this.CFlags.Add(NAI.FLAG.RECHECK_PLAYER, 180f);
				}
				Bench.Pend("Consider normal 1");
				NAI.aware_speed_ratio = 1f;
				bool flag = true;
				bool flag2 = true;
				Bench.P("Consider normal 2");
				if (this.busy_consider_intv > 0f)
				{
					if (this.ticket_cnt > 0 && !this.hasPriorityTicket(128, false, false))
					{
						this.busycsd_t += fcnt;
						if (this.busycsd_t >= this.busy_consider_intv)
						{
							this.busycsd_t -= this.busy_consider_intv;
							NAI.aware_speed_ratio = this.busy_consider_intv;
						}
						else
						{
							flag2 = false;
							flag = this.RayDummy != null;
						}
					}
					else
					{
						this.busycsd_t = 0f;
					}
				}
				Bench.Pend("Consider normal 2");
				Bench.P("Consider normal 3");
				if (this.AimPr_ is M2MoverPr && ((this.AimPr_ as M2MoverPr).target_calced != 1 && flag))
				{
					if (this.autotargetted_me)
					{
						this.dummy_res |= NAI.DUMMY_RES.HERE_DANGEROUS;
					}
					else
					{
						this.dummy_res &= (NAI.DUMMY_RES)(-3);
					}
					if (this.RayDummy != null && (this.RayDummy.hittype & HITTYPE._TEMP_REFLECT) != HITTYPE.NONE)
					{
						this.dummyray_auto_targetted = true;
					}
					this.dummyray_auto_targetted = false;
					this.CurrentCheckRoot = null;
				}
				Bench.Pend("Consider normal 3");
				this.fineTargetPosition(fcnt);
				if (this.delay <= 0f)
				{
					Bench.P("Consider normal main 1");
					if (this.CFlags.Has(NAI.FLAG.RECHECK_PLAYER))
					{
						this.CFlags.Rem(NAI.FLAG.RECHECK_PLAYER);
						if (this.fnSearchPlayer == null)
						{
							this.fnSearchPlayer = new NAI.FnSearchPlayer(this.SearchPlayerDefault);
						}
						this.AimPr = this.fnSearchPlayer(0f);
						if (this.AimPr_ == null)
						{
							this.CFlags.Add(NAI.FLAG.SLEEPED, 180f);
						}
					}
					Bench.Pend("Consider normal main 1");
					if (flag2 && this.AimPr_ != null && (!this.consider_only_onfoot || this.En.hasFoot()))
					{
						Bench.P("Consider normal main 3");
						this.considerAwakeLogicBase();
						if (this.autotargetted_me)
						{
							this.autotarget_hitted_t = X.Mx(this.autotarget_hitted_t - NAI.aware_speed_ratio, 0f);
						}
						Bench.Pend("Consider normal main 3");
					}
				}
				if (this.autotarget_hitted_t < 0f)
				{
					this.autotarget_hitted_t = X.Mn(this.autotarget_hitted_t + fcnt, 0f);
				}
				Bench.Pend("Consider normal");
			}
			NAI.aware_speed_ratio = 1f;
			if (this.Chaser != null)
			{
				Bench.P("Chaser Progress");
				this.Chaser.chaseProgress();
				Bench.Pend("Chaser Progress");
			}
			if (this.RayDummy != null)
			{
				this.RayDummy.hittype = (this.RayDummy.hittype | HITTYPE.TARGET_CHECKER) & ~(HITTYPE.REFLECTED | HITTYPE.REFLECT_BROKEN | HITTYPE.REFLECT_KILLED);
			}
			if (this.En.getState() == NelEnemy.STATE.STAND)
			{
				int i = 0;
				bool flag3 = true;
				this.ticket_cnt = X.MMX(0, this.ticket_cnt, this.ATicket.Length);
				while (i < this.ticket_cnt)
				{
					NaTicket naTicket = this.ATicket[i];
					if (flag3)
					{
						flag3 = false;
						if ((naTicket.prog & PROG.QUIT) != PROG.PREPARE)
						{
							naTicket.after_delay -= fcnt;
							if (naTicket.after_delay <= 0f)
							{
								this.spliceTicket(i, true, false);
								continue;
							}
						}
						else
						{
							string text = Bench.P(FEnum<NAI.TYPE>.ToStr(naTicket.type));
							bool flag4 = this.En.readTicket(naTicket);
							Bench.Pend(text);
							if (!flag4)
							{
								naTicket.quit();
								if (naTicket.after_delay <= 0f)
								{
									this.spliceTicket(i, true, false);
									continue;
								}
							}
							else
							{
								naTicket.t += fcnt;
							}
						}
					}
					else
					{
						if ((naTicket.prog & PROG.QUIT) != PROG.PREPARE)
						{
							this.spliceTicket(i, true, false);
							continue;
						}
						if (!naTicket.isPrepare())
						{
							naTicket.prog = PROG.PREPARE_RECREATED;
						}
					}
					i++;
				}
			}
		}

		public void fineTargetPosition(float fcnt)
		{
			if (this.fine_target_pos_lock_ > 0f)
			{
				this.fine_target_pos_lock_ = X.Mx(0f, this.fine_target_pos_lock_ - fcnt);
				return;
			}
			if (this.AimPr_ != null)
			{
				this.En.fineTargetPos(this.AimPr_, ref this.target_x, ref this.target_y);
			}
		}

		public void flagRun(float fcnt)
		{
			this.DeclCon.run(fcnt);
			this.CTypeLock.run(fcnt);
			this.RCmagic_awake.Update(fcnt);
			this.CFlags.run(fcnt);
		}

		private void spliceTicket(int i, int count, bool error_quit_check = true)
		{
			while (--count >= 0)
			{
				this.spliceTicket(i, error_quit_check, false);
			}
		}

		private void spliceTicket(int i, bool error_quit_check = true, bool do_not_splice = false)
		{
			this.ticket_cnt = X.MMX(0, this.ticket_cnt, this.ATicket.Length);
			if (i >= this.ticket_cnt || this.ticket_cnt <= 0)
			{
				return;
			}
			NaTicket naTicket = this.ATicket[i];
			if (!do_not_splice)
			{
				X.shiftEmpty<NaTicket>(this.ATicket, 1, i, this.ticket_cnt);
				NaTicket[] aticket = this.ATicket;
				int num = this.ticket_cnt - 1;
				this.ticket_cnt = num;
				aticket[num] = naTicket;
			}
			if (!naTicket.isQuitFinlized() && !naTicket.isPrepare())
			{
				this.quitTicket(naTicket, error_quit_check);
			}
			this.action_count++;
		}

		public void quitTicket(NaTicket Tk, bool error_quit_check = true)
		{
			this.En.quitTicket(Tk);
			if (Tk.isAttack(false))
			{
				this.En.getPhysic().remLockMoverHitting(HITLOCK.SPECIAL_ATTACK).remLockGravity(HITLOCK.SPECIAL_ATTACK);
			}
			this.ran_tk = X.xors();
			this.En.can_hold_tackle = false;
			this.CFlags.Add(NAI.FLAG.RECHECK_PLAYER, 180f).Add(NAI.FLAG.TICKET_REFINED, 180f);
			if (this.Chaser != null)
			{
				this.Chaser = null;
			}
			if (Tk.check_nearplace_error > 1 || (Tk.check_nearplace_error == 1 && error_quit_check))
			{
				if (X.LENGTHXYS(this.En.x, this.En.y, this.ticket_start_x_, this.ticket_start_y_) < 0.8f)
				{
					if (Tk.check_nearplace_error > 1)
					{
						this.CTypeLock.Add(Tk.type, 190f);
					}
					this.DeclCon.Add(((Tk.depx >= 0f) ? Tk.depx : this.En.x) - 1.5f, ((Tk.depy >= 0f) ? Tk.depy : this.En.y) - 1f, 3f, 2f, 120f);
				}
				if (!Tk.isQuit())
				{
					this.CTypeLock.Add(Tk.type, 100f);
					this.DeclCon.Add(((Tk.depx >= 0f) ? Tk.depx : this.En.x) - 1.5f, ((Tk.depy >= 0f) ? Tk.depy : this.En.y) - 1.5f, 3f, 3f, 120f);
				}
			}
			this.CurrentCheckRoot = null;
			this.dummy_res &= NAI.DUMMY_RES.HERE_DANGEROUS;
			Tk.quitFinalize();
		}

		public void awakeInit(M2Attackable _AimPr)
		{
			if (_AimPr == null)
			{
				return;
			}
			this.AimPr = _AimPr;
			this.delay = 0f;
			this.En.awakeInit();
		}

		public M2Attackable SearchPlayerDefault(float fix_awake_length = 0f)
		{
			int count_players = this.Mp.count_players;
			if ((this.En.isOverDrive() && this.AimPr_ != null) || this.awake_length == 0f)
			{
				return this.AimPr_;
			}
			float num = -1f;
			M2MoverPr m2MoverPr = null;
			for (int i = 0; i < count_players; i++)
			{
				M2MoverPr pr = this.Mp.getPr(i);
				float num2 = ((fix_awake_length > 0f) ? fix_awake_length : (this.awake_length * (float)((pr == this.AimPr_) ? 5 : 1) * (float)(pr.is_alive ? 1 : (pr.overkill ? 6 : 1))));
				bool flag;
				if (pr == this.AimPr_ || fix_awake_length > 0f)
				{
					flag = X.LENGTHXYS(this.x, this.y, pr.x, pr.y) <= num2;
				}
				else if (X.Abs(pr.x - this.x) <= 2f)
				{
					flag = X.LENGTHXYS(this.x, this.y, pr.x, pr.y) <= num2;
				}
				else
				{
					flag = X.LENGTHXYS(this.x, this.y, pr.x, pr.y) <= num2 / 3f;
					if (!flag && this.x < pr.x == CAim._XD(this.aim, 1) >= 0)
					{
						float num3 = X.Abs(pr.y - this.y) / X.Abs(pr.x - this.x);
						if (X.BTW(0f, num3, 1f))
						{
							flag = X.chkLEN(this.x, this.y, pr.x, pr.y, num2);
						}
					}
				}
				if (flag)
				{
					float num4 = X.LENGTHXY2(this.x, this.y, pr.x, pr.y);
					if (num < 0f || num4 < num)
					{
						m2MoverPr = pr;
						num = num4;
					}
				}
			}
			return m2MoverPr;
		}

		public static M2MoverPr SearchNoel(float fix_awake_length = 0f)
		{
			Map2d curMap = M2DBase.Instance.curMap;
			if (curMap == null)
			{
				return null;
			}
			int count_players = curMap.count_players;
			for (int i = 0; i < count_players; i++)
			{
				PRNoel prnoel = curMap.getPr(i) as PRNoel;
				if (prnoel != null)
				{
					return prnoel;
				}
			}
			return null;
		}

		public M2ChaserAir initChaserAir(Comparison<M2ChaserRootBase<M2AirRegion>> FnChaserSort = null, bool force_init = false)
		{
			return null;
		}

		public M2ChaserBaseFuncs.CHSRES walkChaserDepert(float fcnt, M2ChaserAir.FnProgressWalk fnProgress)
		{
			return M2ChaserBaseFuncs.CHSRES.ERROR;
		}

		private bool chaserAirSortPrepare(M2ChaserAir Chaser, List<M2ChaserAir.ChaserRootAir> ADepert)
		{
			return true;
		}

		private void initDummyCheckingLast(M2ChaserAir Chaser, M2ChaserAir.ChaserRootAir Rt)
		{
		}

		public void initOverdrive()
		{
			this.AddF(NAI.FLAG.RECHECK_PLAYER, 180f);
			this.delay = 0f;
			this.CFlags.Clear();
			this.clearTicket(-1, true);
		}

		protected bool considerSleepLogicBase()
		{
			if (this.hasPriorityTicket(0, false, false))
			{
				return false;
			}
			if (this.CFlags.Has(NAI.FLAG.SLEEPED))
			{
				this.clearTicket(-1, true);
				this.CFlags.Rem(NAI.FLAG.SLEEPED);
				this.AddTicket(NAI.TYPE.SLEEP, 0, true);
				return true;
			}
			return this.fnSleepLogic != null && this.fnSleepLogic(this);
		}

		public bool fnSleepSlide(NAI Nai)
		{
			if (this.hasPriorityTicket(0, false, false))
			{
				return false;
			}
			if (this.En.base_gravity != 0f)
			{
				int num = X.xors(2) + 1;
				for (int i = 1; i <= num; i++)
				{
					uint ran = X.GETRAN2(this.En.index + this.action_count * 43 + i, this.En.index % 7 + this.action_count % 31);
					Vector2 vector = new Vector2(this.FirstPos.x + 8f * (-1f + 2f * X.RAN(ran, 1227)), this.En.mbottom);
					NaTicket naTicket = this.AddTicket(NAI.TYPE.WALK, 0, true).Dep(vector, null);
					if (i == num)
					{
						naTicket.after_delay = (float)(40 + X.xors(110));
					}
				}
				return true;
			}
			float num2 = X.NIXP(2f, 3.5f);
			float num3 = X.XORSP() * 6.2831855f;
			float num4 = this.FirstPos.x + num2 * X.Cos(num3);
			float num5 = this.FirstPos.y - num2 * X.Sin(num3);
			if (!this.En.canStand((int)num4, (int)num5))
			{
				return true;
			}
			this.AddTicket(NAI.TYPE.WALK, 0, true).Dep(num4, num5, null);
			return true;
		}

		protected bool considerAwakeLogicBase()
		{
			if (this.CFlags.Has(NAI.FLAG.AWAKEN))
			{
				this.clearTicket(-1, false);
				this.CFlags.Rem(NAI.FLAG.AWAKEN);
				if (this.CFlags.Has2(NAI.FLAG.SUMMON_APPEARED, NAI.FLAG.OVERDRIVED))
				{
					this.CFlags.Rem(NAI.FLAG.SUMMON_APPEARED);
				}
				else
				{
					this.AddTicket(NAI.TYPE.AWAKE, 0, true);
				}
			}
			if (this.hasPriorityTicket(128, false, false))
			{
				return false;
			}
			if (!Map2d.can_handle && !this.considerable_in_event)
			{
				return !this.hasPriorityTicket(0, false, false) && this.AddTicketB(NAI.TYPE.GAZE, 0, true);
			}
			if (this.En.isOverDrive() && this.fnOverDriveLogic != null)
			{
				return this.fnOverDriveLogic(this);
			}
			return this.fnAwakeLogic != null && this.fnAwakeLogic(this);
		}

		public bool fnAwakeBasicHead(NAI Nai, NAI.TYPE gazing_type = NAI.TYPE.GAZE)
		{
			if (this.cant_access_to_pr())
			{
				this.CFlags.Add(NAI.FLAG.GAZE, 60f).Add(NAI.FLAG.GAZE_CANNOT_ACCESS, 60f);
			}
			if (this.En.cannot_move)
			{
				if (this.HasF(NAI.FLAG.GAZE, false))
				{
					this.AddTicket(gazing_type, 0, true);
					return true;
				}
			}
			else
			{
				if (this.HasF(NAI.FLAG.ESCAPE, false))
				{
					float num = (this.En.isOverDrive() ? this.suit_distance_overdrive : this.suit_distance);
					float num2 = this.x - (float)X.MPF(this.x < this.target_x) * X.Mx(4f, X.Abs(num - X.Abs(this.x - this.target_x)));
					float mbottom = this.En.mbottom;
					if (this.AddMoveTicketFor(num2, mbottom, this.TargetLastBcc, 1, true, NAI.TYPE.WALK) != null)
					{
						return true;
					}
				}
				if (this.HasF(NAI.FLAG.GAZE, false))
				{
					float num3 = this.target_x + (float)X.MPF(this.x > this.target_x) * (this.sizex * (1.1f + 2.7f * this.RANa(7613)));
					if (X.Abs(num3 - this.x) < 0.125f)
					{
						this.AddTicket(gazing_type, 0, true);
						return true;
					}
					if (this.AddMoveTicketFor(num3, this.y, this.TargetLastBcc, 0, true, NAI.TYPE.WALK) != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool fnBasicPunch(NAI Nai)
		{
			return this.fnBasicPunch(Nai, 16, 100f, 0f, 0f, 0f, 8841, false);
		}

		private NAI.TYPE basicStateCheck(NAI.TYPE t, NAI.TYPE t0, NAI.TYPE t1, NAI.TYPE t2, float rate, float rate0, float rate1, float rate2, float ran_hash, float vmax = 100f)
		{
			float num = ran_hash * vmax;
			if (num < rate)
			{
				if (!this.hasTypeLock(t))
				{
					return t;
				}
				return this.basicStateCheck(t, t0, t1, t2, 0f, rate0, rate1, rate2, ran_hash, rate0 + rate1 + rate2);
			}
			else
			{
				num -= rate;
				if (num < rate0)
				{
					if (!this.hasTypeLock(t0))
					{
						return t0;
					}
					return this.basicStateCheck(t, t0, t1, t2, rate, 0f, rate1, rate2, ran_hash, rate + rate1 + rate2);
				}
				else
				{
					num -= rate0;
					if (num < rate1)
					{
						if (!this.hasTypeLock(t1))
						{
							return t1;
						}
						return this.basicStateCheck(t, t0, t1, t2, rate, rate0, 0f, rate2, ran_hash, rate + rate0 + rate2);
					}
					else
					{
						num -= rate1;
						if (num >= rate2)
						{
							return NAI.TYPE.NOTHING;
						}
						if (!this.hasTypeLock(t2))
						{
							return t2;
						}
						return this.basicStateCheck(t, t0, t1, t2, rate, rate0, rate1, 0f, ran_hash, rate + rate0 + rate1);
					}
				}
			}
		}

		public bool fnBasicPunch(NAI Nai, int priority, float rate, float rate0 = 0f, float rate1 = 0f, float rate2 = 0f, int ran_hash = 8841, bool ignore_length = false)
		{
			if (this.hasPriorityTicket(priority, false, false))
			{
				return false;
			}
			if (ignore_length || this.isAttackableLength(false))
			{
				NAI.TYPE type = this.basicStateCheck(NAI.TYPE.PUNCH, NAI.TYPE.PUNCH_0, NAI.TYPE.PUNCH_1, NAI.TYPE.PUNCH_2, rate, rate0, rate1, rate2, this.RANtk(ran_hash), 100f);
				if (type != NAI.TYPE.NOTHING)
				{
					return this.AddTicketB(type, priority, true);
				}
			}
			return false;
		}

		public bool fnBasicMagic(NAI Nai, int priority, float rate, float rate0 = 0f, float rate1 = 0f, float rate2 = 0f, int ran_hash = 7145, bool ignore_length = false)
		{
			if (this.hasPriorityTicket(priority, false, false))
			{
				return false;
			}
			if (ignore_length || this.isAttackableLength(false))
			{
				NAI.TYPE type = this.basicStateCheck(NAI.TYPE.MAG, NAI.TYPE.MAG_0, NAI.TYPE.MAG_1, NAI.TYPE.MAG_2, rate, rate0, rate1, rate2, this.RANtk(ran_hash), 100f);
				if (type != NAI.TYPE.NOTHING)
				{
					return this.AddTicketB(type, priority, true);
				}
			}
			return false;
		}

		public bool fnBasicGuard(NAI Nai, int priority, float rate, float rate0 = 0f, float rate1 = 0f, float rate2 = 0f, int ran_hash = 6864)
		{
			if (this.hasPriorityTicket(priority, false, false))
			{
				return false;
			}
			if (this.isAttackableLength(false))
			{
				NAI.TYPE type = this.basicStateCheck(NAI.TYPE.GUARD, NAI.TYPE.GUARD_0, NAI.TYPE.GUARD_1, NAI.TYPE.GUARD_2, rate, rate0, rate1, rate2, this.RANtk(ran_hash), 100f);
				if (type != NAI.TYPE.NOTHING)
				{
					return this.AddTicketB(type, priority, true);
				}
			}
			return false;
		}

		public bool fnBasicMove(NAI Nai)
		{
			if (this.hasPriorityTicket(0, false, false))
			{
				return false;
			}
			if (this.En.cannot_move)
			{
				this.AddTicket(NAI.TYPE.WAIT, 0, true);
			}
			else
			{
				this.AddMoveTicketToTarget(0f, 0f, 0, true, NAI.TYPE.WALK);
			}
			return true;
		}

		public bool isAttackableLength(bool calc_lastfoot = false)
		{
			return this.isAttackableLength(this.attackable_length_x, this.attackable_length_top, this.attackable_length_bottom, calc_lastfoot);
		}

		public bool isAttackableLength(float x, float y, float xrange, float ymin, float ymax, bool calc_lastfoot = false)
		{
			return this.AimPr_ != null && X.BTW(x - xrange - this.sizex - this.AimPr_.sizex, this.target_x, x + xrange + this.sizex + this.AimPr_.sizex) && this.isTargetYCovering(ymin, ymax, calc_lastfoot);
		}

		public bool isAttackableLength(float xrange, float ymin, float ymax, bool calc_lastfoot = false)
		{
			return this.isAttackableLength(this.x, this.y, xrange, ymin, ymax, calc_lastfoot);
		}

		public bool isTargetYCovering(float ymin, float ymax, bool calc_lastfoot = false)
		{
			if (this.AimPr_ == null)
			{
				return false;
			}
			float num = (calc_lastfoot ? (this.target_lastfoot_bottom - this.AimPr_.sizey) : this.target_y);
			return X.BTW(this.y + ymin - this.sizey - this.AimPr_.sizey, num, this.y + ymax + this.sizey + this.AimPr_.sizey);
		}

		public bool fnAwakeDoNothing(NAI Nai)
		{
			return false;
		}

		public NaTicket AddMoveTicketToTarget(float randomize_x = 0f, float randomize_y = 0f, int priority = 0, bool check_declining = true, NAI.TYPE settype = NAI.TYPE.WALK)
		{
			return this.AddMoveTicketFor(this.target_x + ((randomize_x != 0f) ? (this.RANtk(3148) * randomize_x * (float)X.MPF(this.RANtk(4338) < 0.5f)) : 0f), this.En.floating ? this.target_mbottom : this.target_lastfoot_bottom, this.TargetLastBcc, priority, check_declining, settype);
		}

		public NaTicket AddMoveTicketFor(float depx, float depy, M2BlockColliderContainer.BCCLine TargetBcc = null, int priority = 0, bool check_declining = true, NAI.TYPE settype = NAI.TYPE.WALK)
		{
			if (this.hasPriorityTicket(priority, false, false) || this.En.cannot_move || (check_declining && this.CTypeLock.Has(settype)))
			{
				return null;
			}
			int num = (int)depx;
			int num2 = (int)depy;
			check_declining = check_declining && this.ticket_cnt == 0;
			float num3 = X.Mx(6f, this.attackable_length_bottom);
			float num4 = ((settype == NAI.TYPE.WALK_TO_WEED) ? 0f : ((this.En.isOverDrive() ? this.suit_distance_overdrive : this.suit_distance) * this.NIRANtk(0.6f, 1.1f, 7712)));
			if (this.suit_distance_garaaki_ratio >= 0f && this.isPrGaraakiState())
			{
				num4 = num4 * this.suit_distance_garaaki_ratio + this.sizex;
			}
			M2LpSummon m2LpSummon = ((this.En.Summoner != null) ? this.En.Summoner.Lp : null);
			if (!this.En.is_flying)
			{
				if (TargetBcc == null)
				{
					TargetBcc = this.Mp.getFallableBcc(depx, depy, this.sizex, num3, -1f, true, true, this.LastBcc);
				}
				if (TargetBcc != null)
				{
					float mbottom = this.En.mbottom;
					M2BlockColliderContainer.BCCLine bccline = TargetBcc;
					Vector2 vector = TargetBcc.linePosition(depx, depy, 0f, 0f);
					depx = vector.x;
					depy = vector.y;
					bool flag = check_declining && this.isDecliningXy(depx, depy, 2f, 2f);
					if (X.LENGTHXYS(depx, depy * 3f, this.target_x, this.target_lastfoot_bottom * 3f) < num4 || flag)
					{
						TargetBcc = null;
						for (int i = 0; i < 2; i++)
						{
							float num5 = (float)((this.x < depx == (i == 0)) ? (-1) : 1);
							float num6 = depx + num5 * num4;
							bool flag2 = true;
							if (m2LpSummon != null)
							{
								float num7 = X.MMX((float)m2LpSummon.mapx + this.sizex + 0.25f, num6, (float)(m2LpSummon.mapx + m2LpSummon.mapw) - this.sizex - 0.25f);
								if (num7 != num6)
								{
									num6 = num7;
									flag2 = false;
								}
							}
							float num8 = bccline.slopeBottomY(num6);
							if (flag2 && bccline.isNear(num6, num8, 0f, 1f, -1, false) && (!check_declining || !this.isDecliningXy(num6, num8, 2f, 2f)))
							{
								TargetBcc = bccline;
								depx = num6;
								depy = num8;
								break;
							}
							M2BlockColliderContainer.BCCLine fallableBcc = this.Mp.getFallableBcc(num6, depy, this.sizex, num3, -1f, true, true, bccline);
							if (fallableBcc != null)
							{
								num8 = fallableBcc.slopeBottomY(num6);
								if ((!check_declining || !this.isDecliningXy(num6, num8, 2f, 2f)) && X.LENGTHXYS(num6, num8 * 0.35f, this.target_x, this.target_lastfoot_bottom * 0.35f) >= num4 * 0.6f && ((!this.walk_only_linear && i != 1) || bccline.isLinearWalkableTo(fallableBcc, i == 0) != 0))
								{
									float shifted_y = fallableBcc.shifted_y;
									if (X.BTW(shifted_y - 0.5f, mbottom, shifted_y + fallableBcc.height + 0.5f) || ((shifted_y + fallableBcc.height * 0.5f < mbottom) ? (X.Abs(shifted_y + fallableBcc.height - mbottom) <= this.moveable_upper) : (X.Abs(shifted_y - mbottom) <= this.moveable_lower)))
									{
										depx = num6;
										TargetBcc = fallableBcc;
										break;
									}
								}
							}
						}
						if (TargetBcc == null)
						{
							return null;
						}
					}
					if (bccline != TargetBcc)
					{
						Vector2 vector2 = TargetBcc.linePosition(depx, depy, 0f, 0f);
						depx = vector2.x;
						depy = vector2.y;
					}
					M2BlockColliderContainer.BCCLine lastBcc = this.LastBcc;
					if (lastBcc != null && lastBcc.foot_aim == AIM.B && TargetBcc != lastBcc && TargetBcc.shifted_y > lastBcc.shifted_y)
					{
						if (!lastBcc.is_lift)
						{
							if (!this.no_check_move_fall_slide)
							{
								float num9 = -1f;
								M2BlockColliderContainer.BCCLine bccline2 = null;
								float num10 = depy;
								float num11 = depx;
								for (int j = 0; j < 2; j++)
								{
									float num12 = (float)((j == 0 == this.En.mpf_is_right < 0f) ? (-1) : 1);
									float num13 = ((num12 < 0f) ? (lastBcc.shifted_x - 0.125f - this.En.sizex) : (lastBcc.shifted_right + 0.125f + this.En.sizex));
									if (m2LpSummon == null || (num13 >= (float)m2LpSummon.mapx && num13 < (float)(m2LpSummon.mapx + m2LpSummon.mapw)))
									{
										M2BlockColliderContainer.BCCLine fallableBcc2 = this.Mp.getFallableBcc(num13, lastBcc.shifted_cy, this.sizex, num3, -1f, true, true, this.LastBcc);
										if (fallableBcc2 != null)
										{
											float num14 = ((num12 < 0f) ? lastBcc.shifted_left_y : lastBcc.shifted_right_y);
											if (X.Mn(depy, fallableBcc2.shifted_bottom) >= num14 - 1f)
											{
												float num15 = 0f;
												if (fallableBcc2 == TargetBcc)
												{
													num15 += 200f;
													num12 = 0f;
													num13 = X.MMX2(TargetBcc.shifted_x + this.sizex * 0.5f, num11, TargetBcc.shifted_right - this.sizex * 0.5f);
													num14 = depy;
												}
												else
												{
													if (this.walk_only_linear && this.LastBcc.isLinearWalkableTo(fallableBcc2, true) == 0)
													{
														goto IL_05FC;
													}
													num15 += X.Mx(7f - X.Abs(depy - fallableBcc2.shifted_cy), 0f) * 4f;
												}
												num15 += X.Mx(7f - X.Abs(depx - num13), 0f) + X.Mx(7f - X.Abs(num13 - this.x), 0f);
												if (num9 < 0f || num15 > num9)
												{
													num9 = num15;
													num11 = num13 + num12 * this.sizex;
													num10 = num14;
													bccline2 = fallableBcc2;
												}
											}
										}
									}
									IL_05FC:;
								}
								if (bccline2 != null)
								{
									TargetBcc = bccline2;
									depx = num11;
									depy = num10;
								}
							}
						}
						else if (X.BTW(TargetBcc.shifted_x, this.x, TargetBcc.shifted_right) && TargetBcc.slopeBottomY(depx) >= mbottom + 1f && (this.En.getFootManager() == null || TargetBcc != this.En.getFootManager().get_FootBCC()))
						{
							return this.AddTicket(settype, priority, true).Dep(depx, depy + this.footshift, TargetBcc).SetAim(3);
						}
					}
				}
			}
			else
			{
				TargetBcc = null;
				bool flag3 = check_declining && this.isDecliningXy(depx, depy, 2f, 2f);
				if (X.LENGTHXYS(depx, depy, this.target_x, this.target_lastfoot_bottom) < num4 || flag3)
				{
					float mbottom2 = this.En.mbottom;
					for (int k = 0; k < 2; k++)
					{
						float num16 = (float)((depx < this.target_x == (k == 0)) ? (-1) : 1);
						float num17 = depx + num16 * num4;
						if (!check_declining || !this.isDecliningXy(num17, depy, 2f, 2f))
						{
							M2BlockColliderContainer.BCCLine fallableBcc3 = this.Mp.getFallableBcc(num17, depy, this.sizex, num3, -1f, true, true, null);
							if (fallableBcc3 != null)
							{
								float shifted_y2 = fallableBcc3.shifted_y;
								if (X.BTW(shifted_y2 - 0.5f, mbottom2, shifted_y2 + fallableBcc3.height + 0.5f) || ((shifted_y2 + fallableBcc3.height * 0.5f < mbottom2) ? (X.Abs(shifted_y2 + fallableBcc3.height - mbottom2) <= 3.5f) : (X.Abs(shifted_y2 - mbottom2) <= 5.5f)))
								{
									Vector2 vector3 = fallableBcc3.linePosition(num17, depy, 0f, 0f);
									depx = vector3.x;
									depy = X.Mn(depy, vector3.y - 0.5f);
									break;
								}
							}
						}
					}
				}
				num = (int)depx;
				num2 = (int)depy;
				M2BlockColliderContainer.BCCLine bccline3 = null;
				Vector3 vector4 = M2BlockColliderContainer.extractFromStuck(this.Mp, (float)num, (float)num2, ref bccline3, 15, X.Mx(this.sizex, this.sizey) + 0.5f, -1f, false);
				if (vector4.z >= 0f)
				{
					depx = vector4.x;
					depy = vector4.y;
				}
			}
			return this.AddTicket(settype, priority, true).Dep(depx, depy + this.footshift, TargetBcc);
		}

		public NaTicket AddTicketSearchAndGetManaWeed(int priority, float moveable_x, float moveable_y_min, float moveable_y_max, float atk_x, float atk_y_min, float atk_y_max, bool set_move_ticket = true)
		{
			if (this.En.Summoner == null || this.hasPriorityTicket(priority, false, false))
			{
				return null;
			}
			M2LpSummon lp = this.En.Summoner.Lp;
			if (lp == null)
			{
				return null;
			}
			List<M2ManaWeed> aactiveWeed = lp.AActiveWeed;
			if (aactiveWeed == null)
			{
				return null;
			}
			int count = aactiveWeed.Count;
			if (count == 0)
			{
				return null;
			}
			float x = this.En.x;
			float y = this.En.y;
			float num = x - moveable_x;
			float num2 = x + moveable_x;
			float num3 = y + moveable_y_min;
			float num4 = y + moveable_y_max;
			float num5 = x - atk_x;
			float num6 = x + atk_x;
			float num7 = y + atk_y_min;
			float num8 = y + atk_y_max;
			M2ManaWeed m2ManaWeed = null;
			M2ManaWeed m2ManaWeed2 = null;
			float num9 = 0f;
			for (int i = 0; i < count; i++)
			{
				M2ManaWeed m2ManaWeed3 = aactiveWeed[i];
				if (m2ManaWeed3.isActive() && X.isCovering(num, num2, m2ManaWeed3.mleft, m2ManaWeed3.mright, 0f) && X.isCovering(num3, num4, m2ManaWeed3.mtop, m2ManaWeed3.mbottom, 0f))
				{
					float num10 = X.LENGTHXY2(m2ManaWeed3.mapcx, m2ManaWeed3.mapcy, x, y);
					if (!this.isDecliningXy(m2ManaWeed3.mapcx, m2ManaWeed3.mapcy, 1f, 1f) && (m2ManaWeed2 == null || num10 < num9))
					{
						num9 = num10;
						m2ManaWeed2 = m2ManaWeed3;
						if (atk_x > 0f && X.isCovering(num5, num6, m2ManaWeed3.mleft, m2ManaWeed3.mright, 0f) && X.isCovering(num7, num8, m2ManaWeed3.mtop, m2ManaWeed3.mbottom, 0f))
						{
							m2ManaWeed = m2ManaWeed3;
						}
					}
				}
			}
			if (m2ManaWeed != null)
			{
				return this.AddTicket(NAI.TYPE.PUNCH_WEED, priority, true).Dep(m2ManaWeed.mapcx, m2ManaWeed.mbottom - 0.002f + this.footshift, m2ManaWeed2.GetBcc());
			}
			if (!(m2ManaWeed2 != null) || !set_move_ticket || this.En.cannot_move)
			{
				return null;
			}
			return this.AddMoveTicketFor(m2ManaWeed2.mapcx, m2ManaWeed2.mbottom - 0.002f + this.footshift, m2ManaWeed2.GetBcc(), priority, false, NAI.TYPE.WALK_TO_WEED);
		}

		public NaTicket AddTicketBackStep(int priority, float away_len_min, float away_len_max, bool ignore_prioritycheck = false)
		{
			if ((!ignore_prioritycheck && this.hasPriorityTicket(priority, false, false)) || this.CTypeLock.Has(NAI.TYPE.BACKSTEP))
			{
				return null;
			}
			float num = X.NI(away_len_min, away_len_max, this.RANtk(7742));
			float num2 = this.target_x - num - this.x;
			float num3 = this.target_x + num - this.x;
			float num4 = this.En.canGoToSideL((num2 > 0f) ? AIM.R : AIM.L, X.Abs(num2), -X.Mn(this.sizey * 0.85f, 0.25f), false, false, false);
			float num5 = this.En.canGoToSideL((num3 > 0f) ? AIM.R : AIM.L, X.Abs(num3), -X.Mn(this.sizey * 0.85f, 0.25f), false, false, false);
			float num6 = X.Abs(this.x + num4 * (float)X.MPF(num2 > 0f));
			float num7 = X.Abs(this.x + num5 * (float)X.MPF(num3 > 0f));
			bool flag = this.isDecliningXy(num6, this.y, 4f, 2f);
			bool flag2 = this.isDecliningXy(num7, this.y, 4f, 2f);
			if (flag && flag2)
			{
				return null;
			}
			return this.AddTicket(NAI.TYPE.BACKSTEP, priority, true).DepX(flag ? num7 : (flag2 ? num6 : ((X.Abs(num6 - this.target_x) + (float)X.MPF(this.target_x > this.x) * 1.5f > X.Abs(num7 - this.target_x)) ? num6 : num7)));
		}

		public bool isDangerousWalk(AIM aim, bool consider_cannotstand = false)
		{
			int num = CAim._XD(aim, 1);
			int num2 = ((num >= 0) ? ((int)(this.En.mright + 0.25f)) : ((int)this.En.mleft));
			if ((float)num2 == ((num >= 0) ? ((float)((int)this.En.mright) - 0.125f) : ((float)((int)(this.En.mleft + 0.125f)))))
			{
				return false;
			}
			int num3 = (this.En.hasFoot() ? ((int)(this.En.mbottom + 0.3f)) : ((int)(this.En.mbottom - 0.2f)));
			int num4 = (int)(this.En.mtop + 0.2f);
			if (this.Mp.isDangerous(num2, (int)(this.En.mbottom + 0.2f)))
			{
				num3--;
			}
			for (int i = num3; i >= num4; i--)
			{
				M2Pt pointPuts = this.Mp.getPointPuts(num2, i, false, false);
				if (pointPuts != null)
				{
					if (pointPuts.isDangerous())
					{
						return true;
					}
					if (consider_cannotstand && i < num3 - 1 && !pointPuts.canStand())
					{
						return true;
					}
				}
			}
			return false;
		}

		public AIM getEnoughRoomAim(float x, float y, bool lr = true)
		{
			int num = (int)x;
			int num2 = (int)y;
			if (lr)
			{
				M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc(num, num2, AIM.L);
				M2BlockColliderContainer.BCCLine sideBcc2 = this.Mp.getSideBcc(num, num2, AIM.R);
				if (sideBcc == null || sideBcc2 == null)
				{
					if (sideBcc != null)
					{
						return AIM.L;
					}
					if (sideBcc2 != null)
					{
						return AIM.R;
					}
					if (this.En.mpf_is_right <= 0f)
					{
						return AIM.R;
					}
					return AIM.L;
				}
				else
				{
					if (X.Abs(sideBcc.shifted_right - x) >= X.Abs(sideBcc2.shifted_x - x))
					{
						return AIM.L;
					}
					return AIM.R;
				}
			}
			else
			{
				M2BlockColliderContainer.BCCLine sideBcc3 = this.Mp.getSideBcc(num, num2, AIM.T);
				M2BlockColliderContainer.BCCLine sideBcc4 = this.Mp.getSideBcc(num, num2, AIM.B);
				if (sideBcc3 == null || sideBcc4 == null)
				{
					if (sideBcc3 != null)
					{
						return AIM.T;
					}
					if (sideBcc4 != null)
					{
						return AIM.B;
					}
					if (!this.En.hasFoot())
					{
						return AIM.B;
					}
					return AIM.T;
				}
				else
				{
					if (X.Abs(sideBcc3.shifted_bottom - y) >= X.Abs(sideBcc4.shifted_y - y))
					{
						return AIM.T;
					}
					return AIM.B;
				}
			}
		}

		public bool hasPT(int _priority, bool remove_if_after_delaying = false)
		{
			return this.hasPriorityTicket(_priority, remove_if_after_delaying, false);
		}

		public bool hasPriorityTicket(int _priority, bool remove_if_after_delaying = false, bool only_active = false)
		{
			this.ticket_cnt = X.MMX(0, this.ticket_cnt, this.ATicket.Length);
			while (this.ticket_cnt > 0)
			{
				NaTicket naTicket = this.ATicket[0];
				if (naTicket.priority < _priority)
				{
					return false;
				}
				if (only_active && naTicket.isQuit())
				{
					return false;
				}
				if (!remove_if_after_delaying || !naTicket.isQuit())
				{
					return true;
				}
				this.spliceTicket(0, true, false);
			}
			return false;
		}

		public NaTicket AddTicket(NAI.TYPE type, int priority_, bool clear_min_priority = true)
		{
			int i;
			NaTicket naTicket;
			for (i = 0; i < this.ticket_cnt; i++)
			{
				naTicket = this.ATicket[i];
				if (naTicket == null || naTicket.isQuitFinlized() || naTicket.priority < priority_)
				{
					break;
				}
				if (naTicket.priority == priority_ && naTicket.type == type && (naTicket.isPrepare() || i == 0))
				{
					return naTicket;
				}
			}
			if (i >= this.ATicket.Length)
			{
				Array.Resize<NaTicket>(ref this.ATicket, i + 4);
			}
			if (i < this.ticket_cnt && !clear_min_priority && this.ticket_cnt < this.ATicket.Length)
			{
				X.unshiftEmpty<NaTicket>(this.ATicket, this.ATicket[this.ticket_cnt], i, 1, this.ticket_cnt);
				this.ticket_cnt++;
			}
			if (this.ATicket[i] == null)
			{
				naTicket = (this.ATicket[i] = new NaTicket(type, priority_));
			}
			else
			{
				this.spliceTicket(i, true, true);
				naTicket = this.ATicket[i];
				naTicket.clear().Recreate(type, priority_, true, null);
			}
			this.ticket_cnt = X.Mx(i + 1, this.ticket_cnt);
			if (clear_min_priority && i < this.ticket_cnt - 1)
			{
				this.spliceTicket(i + 1, this.ticket_cnt - i - 1, false);
			}
			return naTicket;
		}

		public bool AddTicketB(NAI.TYPE type, int priority_, bool clear_min_priority = true)
		{
			this.AddTicket(type, priority_, clear_min_priority);
			return true;
		}

		public NAI clearTicket(int less_priority = -1, bool clear_flag = true)
		{
			if (less_priority < 0)
			{
				if (this.ticket_cnt > 0)
				{
					this.spliceTicket(0, this.ticket_cnt, false);
				}
				if (clear_flag)
				{
					this.CFlags.Clear().Add(NAI.FLAG.RECHECK_PLAYER, 180f);
				}
			}
			else
			{
				int num = this.ticket_cnt;
				for (int i = 0; i < num; i++)
				{
					if (this.ATicket[i].priority <= less_priority)
					{
						this.spliceTicket(i, this.ticket_cnt - i, false);
						this.ran_tk = X.xors();
						break;
					}
				}
			}
			return this;
		}

		public bool isAreaSafe(float cx, float cy, float margin_x, float margin_y, bool no_slope = true, bool no_water = true, bool ensure_all_canstand = true)
		{
			if (this.En.Summoner != null)
			{
				float num = X.Mx(margin_x, margin_y) * this.Mp.CLEN;
				if (!(ensure_all_canstand ? this.En.Summoner.AreaContainsMv(this.En, num) : this.En.Summoner.AreaCoveringMv(this.En, num)))
				{
					return false;
				}
			}
			return this.Mp.getSafeRoomPosition(cx, cy, this.sizex + margin_x, this.sizey + margin_y, no_slope, no_water, true, ensure_all_canstand) > 0f;
		}

		public float isAreaSafeF(float cx, float cy, float margin_x, float margin_y, bool no_slope = true, bool no_water = true)
		{
			if (this.En.Summoner != null)
			{
				float num = X.Mx(margin_x, margin_y) * this.Mp.CLEN;
				if (!this.En.Summoner.AreaContainsMv(this.En, num))
				{
					return 0f;
				}
			}
			return this.Mp.getSafeRoomPosition(cx, cy, this.sizex + margin_x, this.sizey + margin_y, no_slope, no_water, true, false);
		}

		public float getMpfPlayerSide(float shiftx, float margin_x, float margin_y, float distance, float current_side_ratio = 1f, bool no_water = true)
		{
			float num = this.target_lastfoot_bottom - this.sizey;
			bool flag = this.isAreaSafe(this.target_x + shiftx - distance, num, margin_x, margin_y + 0.05f, false, no_water, false);
			bool flag2 = this.isAreaSafe(this.target_x + shiftx + distance, num, margin_x, margin_y + 0.05f, false, no_water, false);
			if (flag && flag2)
			{
				if (current_side_ratio >= 1f || this.RANtk(6713) < current_side_ratio)
				{
					return this.mpf_from_target;
				}
				return (float)X.MPF(this.RANtk(8644) < 0.5f);
			}
			else
			{
				if (flag)
				{
					return -1f;
				}
				if (flag2)
				{
					return 1f;
				}
				return this.mpf_from_target;
			}
		}

		public float getMpfEscapeSide(float shiftx, float shifty, float margin_x, float margin_y, float distance, float current_side_ratio = 1f, bool no_water = true, bool escaping = true)
		{
			float num = this.y + shifty;
			float num2 = this.isAreaSafeF(this.x + shiftx - distance, num, margin_x - this.sizex, margin_y - this.sizey + 0.05f, false, no_water);
			float num3 = this.isAreaSafeF(this.x + shiftx + distance, num, margin_x - this.sizex, margin_y - this.sizey + 0.05f, false, no_water);
			if (num2 > 0f && num3 > 0f)
			{
				if (num2 > 1f && num3 > 1f && (current_side_ratio >= 1f || this.RANtk(6713) < current_side_ratio))
				{
					return this.mpf_from_target * (float)X.MPF(escaping);
				}
				return (float)((num2 == num3) ? X.MPF(this.RANtk(8644) < 0.5f) : ((num2 > num3) ? (-1) : 1));
			}
			else
			{
				if (num2 > 0f)
				{
					return -1f;
				}
				if (num3 > 0f)
				{
					return 1f;
				}
				return this.mpf_from_target * (float)X.MPF(escaping);
			}
		}

		public float mpf_from_target
		{
			get
			{
				return (float)((this.x < this.target_x) ? (-1) : 1);
			}
		}

		public float mpf_to_target
		{
			get
			{
				return (float)((this.x < this.target_x) ? 1 : (-1));
			}
		}

		public bool cant_access_to_pr()
		{
			PR pr = this.AimPr_ as PR;
			return pr != null && (pr.hasD(M2MoverPr.DECL.THROW_RAY) || pr.isWormTrappedCannotAccessByEnemy());
		}

		public bool isDecliningXy(float x, float y, float margin_x = 0f, float margin_y = 0f)
		{
			return this.DeclCon.isinXy(x, y, margin_x, margin_y);
		}

		public NAI addDeclineArea(float x, float y, float w, float h, float time)
		{
			this.DeclCon.Add(x, y, w, h, time);
			return this;
		}

		public float dep_dx
		{
			get
			{
				if (this.ticket_cnt == 0)
				{
					return this.target_x - this.En.x;
				}
				return ((this.ATicket[0].depx < 0f) ? this.target_x : this.ATicket[0].depx) - this.En.x;
			}
		}

		public float dep_dy
		{
			get
			{
				if (this.ticket_cnt == 0)
				{
					return this.target_y - this.En.y;
				}
				return ((this.ATicket[0].depy < 0f) ? this.target_y : this.ATicket[0].depy) - (this.En.mbottom + this.footshift);
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.En.Mp;
			}
		}

		public float x
		{
			get
			{
				return this.En.x;
			}
		}

		public float y
		{
			get
			{
				return this.En.y;
			}
		}

		public Vector2 getRunExpectPos()
		{
			Vector2 vector = new Vector2(this.target_x, this.target_y);
			if (this.AimPr == null)
			{
				return vector;
			}
			vector.x += X.absMn(this.AimPr.vx * 50f, 4.5f);
			vector.y += X.absMn(this.AimPr.vy * 10f, 1.5f);
			return vector;
		}

		public float sizex
		{
			get
			{
				return this.En.sizex;
			}
		}

		public float sizey
		{
			get
			{
				return this.En.sizey;
			}
		}

		public AIM aim
		{
			get
			{
				return this.En.aim;
			}
		}

		public bool is_awaken
		{
			get
			{
				return this.AimPr_ != null;
			}
		}

		public NAI AddF(NAI.FLAG f, float maxt = 180f)
		{
			this.CFlags.Add(f, maxt);
			return this;
		}

		public NAI RemF(NAI.FLAG f)
		{
			this.CFlags.Rem(f);
			return this;
		}

		public NAI RemFMulti(NAI.FLAG f = NAI.FLAG._ADDITIONAL)
		{
			for (int i = 0; i < 64; i++)
			{
				NAI.FLAG flag = (NAI.FLAG)(1 << i);
				if (flag >= NAI.FLAG._MAX)
				{
					break;
				}
				if ((f & flag) != NAI.FLAG.NONE)
				{
					this.CFlags.Rem(f);
				}
			}
			return this;
		}

		public bool HasF(NAI.FLAG f, bool remove_if_exist = false)
		{
			bool flag = this.CFlags.Has(f);
			if (flag && remove_if_exist)
			{
				this.CFlags.Rem(f);
			}
			return flag;
		}

		public bool tkC(int v, float level01, bool otherflag = true)
		{
			return otherflag && X.RAN(this.ran_tk, v) < level01;
		}

		public float RANtk(int v)
		{
			return X.RAN(this.ran_tk, v);
		}

		public float RANStk(int v)
		{
			return -1f + X.RAN(this.ran_tk, v) * 2f;
		}

		public float RANSHtk(int v)
		{
			return -0.5f + X.RAN(this.ran_tk, v);
		}

		public float RANf(int v = 322)
		{
			return X.RAN(X.GETRAN2((int)this.Mp.floort, 3 + ((int)this.Mp.floort & 13)), v);
		}

		public float RANn(int v)
		{
			return X.RAN(this.ran_n, v);
		}

		public float RANa(int v)
		{
			return X.RAN(this.ran_a, v);
		}

		public float NIRANtk(float a, float b, int v)
		{
			return X.NI(a, b, this.RANtk(v));
		}

		public float fine_target_pos_lock
		{
			get
			{
				return this.fine_target_pos_lock_;
			}
			set
			{
				this.fine_target_pos_lock_ = X.Mx(this.fine_target_pos_lock_, value);
			}
		}

		public M2Attackable AimPr
		{
			get
			{
				return this.AimPr_;
			}
			set
			{
				if (this.AimPr_ == value)
				{
					return;
				}
				if (this.AimPr_ is M2MoverPr && this.add_enemy_target_count)
				{
					(this.AimPr_ as M2MoverPr).enemy_targetted--;
				}
				this.AimPr_ = value;
				if (this.AimPr_ is M2MoverPr && this.add_enemy_target_count)
				{
					(this.AimPr_ as M2MoverPr).enemy_targetted++;
				}
			}
		}

		public bool target_hasfoot
		{
			get
			{
				return this.AimPr_ != null && this.AimPr_.hasFoot();
			}
		}

		public float target_mbottom
		{
			get
			{
				return this.target_y + ((this.AimPr_ != null) ? this.AimPr_.sizey : 1f);
			}
		}

		public float target_lastfoot_bottom
		{
			get
			{
				if (!(this.AimPr_ != null))
				{
					return this.target_y;
				}
				return this.AimPr_.last_foot_bottom_y;
			}
		}

		public float target_sizex
		{
			get
			{
				if (!(this.AimPr_ != null))
				{
					return 0f;
				}
				return this.AimPr_.sizex;
			}
		}

		public float target_sizey
		{
			get
			{
				if (!(this.AimPr_ != null))
				{
					return 0f;
				}
				return this.AimPr_.sizey;
			}
		}

		public float target_lastfoot_bottom_mx
		{
			get
			{
				return X.Mx(this.target_lastfoot_bottom, this.target_mbottom);
			}
		}

		public AIM to_pr_aim
		{
			get
			{
				if (this.x >= this.target_x)
				{
					return AIM.L;
				}
				return AIM.R;
			}
		}

		public int xd_to_pr_aim
		{
			get
			{
				if (this.x >= this.target_x)
				{
					return -1;
				}
				return 1;
			}
		}

		public float target_sight_x(float min_x = 0.5f, float max_x = 9f)
		{
			if (this.En.mpf_is_right <= 0f)
			{
				return X.MMX(this.x - max_x, this.target_x, this.x - min_x);
			}
			return X.MMX(this.x + min_x, this.target_x, this.x + max_x);
		}

		public float target_near_x(float far_len_min = 2f, float far_len_max = -1f, int ran_hash = 3928)
		{
			float num = far_len_min;
			if (far_len_max >= 0f && far_len_min != far_len_max)
			{
				num = this.NIRANtk(far_len_min, far_len_max, ran_hash);
			}
			if (X.Abs(this.En.x - this.target_x) <= num)
			{
				if (this.En.x >= this.target_x)
				{
					return this.target_x + num;
				}
				return this.target_x - num;
			}
			else
			{
				if (this.En.x >= this.target_x)
				{
					return X.Mn(this.En.x, this.target_x + num);
				}
				return X.Mx(this.En.x, this.target_x - num);
			}
		}

		public bool isTargetAbove(float xdif, float ydif, float air_upper = 2f)
		{
			return !(this.AimPr_ == null) && this.target_sxdif < xdif && X.isCovering(this.En.mtop - ydif - (this.AimPr_.hasFoot() ? 0f : air_upper), this.En.mbottom, this.AimPr_.mtop, this.AimPr_.mbottom, 0f);
		}

		public float target_len
		{
			get
			{
				return X.LENGTHXYS(this.x, this.y, this.target_x, this.target_y);
			}
		}

		public float target_slen
		{
			get
			{
				return X.LENGTH_SIZE(this.x, this.sizex, this.target_x, (this.AimPr != null) ? this.AimPr.sizex : 0f) + X.LENGTH_SIZE(this.y, this.sizey, this.target_mbottom, (this.AimPr != null) ? this.AimPr.sizey : 0f);
			}
		}

		public float target_slen_n
		{
			get
			{
				return X.Mx(X.LENGTH_SIZE(this.x, this.sizex, this.target_x, (this.AimPr != null) ? this.AimPr.sizex : 0f), X.LENGTH_SIZE(this.y, this.sizey, this.target_mbottom, (this.AimPr != null) ? this.AimPr.sizey : 0f));
			}
		}

		public float target_foot_slen
		{
			get
			{
				return X.LENGTH_SIZE(this.x, this.sizex, this.target_x, (this.AimPr != null) ? this.AimPr.sizex : 0f) + X.LENGTH_SIZE(this.En.mbottom, this.sizey, this.target_mbottom, (this.AimPr != null) ? this.AimPr.sizey : 0f);
			}
		}

		public float target_xdif
		{
			get
			{
				return X.Abs(this.x - this.target_x);
			}
		}

		public float target_sxdif
		{
			get
			{
				return X.LENGTH_SIZE(this.x, this.sizex, this.target_x, (this.AimPr != null) ? this.AimPr.sizex : 0f);
			}
		}

		public float target_sydif
		{
			get
			{
				return X.LENGTH_SIZE(this.y, this.sizey, this.target_y, (this.AimPr != null) ? this.AimPr.sizey : 0f);
			}
		}

		public float target_ydif
		{
			get
			{
				return X.Abs(this.y - this.target_y);
			}
		}

		public float ticket_start_x
		{
			get
			{
				return this.ticket_start_x_;
			}
		}

		public float ticket_start_y
		{
			get
			{
				return this.ticket_start_y_;
			}
		}

		public float target_lastfoot_len
		{
			get
			{
				return X.LENGTHXYS(this.x, this.En.mbottom, this.target_x, this.target_lastfoot_bottom);
			}
		}

		public float target_foot_len
		{
			get
			{
				return X.LENGTHXYS(this.x, this.En.mbottom, this.target_x, this.target_mbottom);
			}
		}

		public M2BlockColliderContainer.BCCLine TargetLastBcc
		{
			get
			{
				if (!(this.AimPr_ != null))
				{
					return null;
				}
				M2FootManager footManager = this.AimPr_.getFootManager();
				if (footManager == null)
				{
					return null;
				}
				return footManager.get_LastBCC();
			}
		}

		public M2BlockColliderContainer.BCCLine LastBcc
		{
			get
			{
				M2FootManager footManager = this.En.getFootManager();
				if (footManager == null)
				{
					return null;
				}
				return footManager.get_LastBCC();
			}
		}

		public bool autotargetted_me
		{
			get
			{
				return this.autotarget_hitted_t > 0f;
			}
			set
			{
				if (value && this.autotarget_hitted_t >= 0f)
				{
					this.autotarget_hitted_t = 15f;
				}
			}
		}

		public float autotargetted_lock
		{
			set
			{
				if (value > 0f)
				{
					this.autotarget_hitted_t = -value;
				}
			}
		}

		public bool isFrontType(NAI.TYPE type, PROG min_prog = PROG.ACTIVE)
		{
			if (this.ticket_cnt == 0)
			{
				return false;
			}
			NaTicket naTicket = this.ATicket[0];
			return naTicket.prog >= min_prog && naTicket.type == type;
		}

		public bool isCoveringToPr(float margin_x = 0f, float margin_y = 0f)
		{
			return !(this.AimPr_ == null) && this.En.isCoveringMv(this.AimPr_, margin_x, margin_y);
		}

		public NaTicket getCurTicket()
		{
			if (this.ticket_cnt <= 0)
			{
				return null;
			}
			return this.ATicket[0];
		}

		public bool isAttacking()
		{
			NaTicket curTicket = this.getCurTicket();
			return curTicket != null && curTicket.isAttack(false);
		}

		public bool isPrGaraakiState()
		{
			if (this.garaaki_state == 2)
			{
				this.garaaki_state = ((this.AimPr_ != null && this.AimPr_.isGaraaakiForNAI()) ? 1 : 0);
			}
			return this.garaaki_state == 1;
		}

		public bool isPrAbsorbed()
		{
			return this.AimPr_ is PR && (this.AimPr_ as PR).isAbsorbState();
		}

		public int getPrAbsorbedPriority()
		{
			if (this.AimPr_ is PR)
			{
				PR pr = this.AimPr_ as PR;
				if (pr.isAbsorbState())
				{
					return pr.getAbsorbContainer().current_pose_priority;
				}
			}
			return -2;
		}

		public bool isPrGacharingFrozen()
		{
			if (this.AimPr_ is PR)
			{
				PR pr = this.AimPr_ as PR;
				return pr.isNormalState() && pr.Ser.has(SER.FROZEN) && !pr.Skill.isBusyTime(true, true, true, true);
			}
			return false;
		}

		public bool isPrChantCompleted(float ratio = 0.7f)
		{
			return this.AimPr_ is PR && (this.AimPr_ as PR).Skill.getChantCompletedRatio() >= ratio;
		}

		public bool isPrAbsorbedCannotMove()
		{
			if (this.AimPr_ is PR)
			{
				PR pr = this.AimPr_ as PR;
				return pr.isAbsorbState() && pr.getAbsorbContainer().cannot_move;
			}
			return false;
		}

		public bool isPrAlive()
		{
			return this.AimPr_ is PR && this.AimPr_.is_alive;
		}

		public bool isPrMagicChanting(float notice_time_ratio = 1f)
		{
			return this.AimPr is PR && (this.AimPr as PR).magic_chanting_or_preparing && this.canNoticeThat(notice_time_ratio);
		}

		public bool isPrMagicChantCompleted(float notice_time_ratio = 1f, bool include_explode = true)
		{
			if (this.AimPr is PR)
			{
				PR pr = this.AimPr as PR;
				if (include_explode && this.isPrMagicExploded(notice_time_ratio))
				{
					return true;
				}
				if (pr.getCurMagic() == null)
				{
					return false;
				}
				if (pr.getSkillManager().magic_chant_completed)
				{
					return this.canNoticeThat(notice_time_ratio);
				}
			}
			return false;
		}

		public bool isPrShieldOpening(int notice_time = 20)
		{
			return this.AimPr is PR && (this.AimPr as PR).isShieldOpening() && (notice_time <= 0 || this.RCmagic_awake.Add(this.En.TS, true, false) >= (float)notice_time);
		}

		public bool isPrAttacking(float time_ratio = 1f)
		{
			return this.AimPr is PR && (this.AimPr as PR).isPunchState() && this.canNoticeThat(time_ratio);
		}

		public bool isPrSpecialAttacking()
		{
			return this.AimPr_ != null && this.AimPr is PR && (this.AimPr as PR).isSpecialPunchState() && this.canNoticeThat(1f);
		}

		public bool isPrShotGunEnable(float notice_time_ratio = 1f, bool include_explode = true)
		{
			if (this.AimPr is PR)
			{
				PR pr = this.AimPr as PR;
				if (this.isPrMagicChantCompleted(0f, include_explode) && (pr.isNormalState() || pr.isPunchState() || pr.isSpecialPunchState()))
				{
					return this.canNoticeThat(notice_time_ratio);
				}
			}
			return false;
		}

		public bool isPrCometTargetting(bool ignore_sxdif = false)
		{
			return this.AimPr is PR && (this.AimPr as PR).isSpecialPunchComet() && (ignore_sxdif || this.target_sxdif < 0.8f);
		}

		public bool isPrMagicExploded(float notice_time_ratio = 1f)
		{
			return this.AimPr is PR && (this.AimPr as PR).magic_exploded && this.canNoticeThat(notice_time_ratio);
		}

		public bool isPrMagicChantingOrPreparing(float notice_time_ratio = 1f)
		{
			return this.AimPr is PR && (this.AimPr as PR).magic_chanting_or_preparing && this.canNoticeThat(notice_time_ratio);
		}

		public bool isPrMagicChantingOrPreparingOrExploded(float notice_time_ratio = 1f)
		{
			return this.AimPr is PR && ((this.AimPr as PR).magic_chanting_or_preparing || (this.AimPr as PR).magic_exploded) && this.canNoticeThat(notice_time_ratio);
		}

		public bool isPrTortured()
		{
			return this.AimPr is PR && (this.AimPr as PR).getAbsorbContainer().use_torture;
		}

		public float GARtoPr(float shiftx = 0f, float shifty = 0f)
		{
			return this.Mp.GAR(this.x, this.y, this.target_x + shiftx, this.target_y + shifty);
		}

		public float GARtoPrinAir(float shiftx, float shifty)
		{
			float num = this.target_x + shiftx;
			float num2 = this.target_y + shifty;
			M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc((int)num, (int)num2, AIM.B);
			if (sideBcc != null)
			{
				num2 = X.Mn(num2, sideBcc.slopeBottomY(num) - 0.5f);
			}
			return this.Mp.GAR(this.x, this.y, num, num2);
		}

		public float GARfromPr()
		{
			return this.Mp.GAR(this.target_x, this.target_y, this.x, this.y);
		}

		public float target_real_x
		{
			get
			{
				if (!(this.AimPr_ != null))
				{
					return this.target_x;
				}
				return this.AimPr_.x;
			}
		}

		public float target_real_y
		{
			get
			{
				if (!(this.AimPr_ != null))
				{
					return this.target_y;
				}
				return this.AimPr_.y;
			}
		}

		public bool canNoticeThat(float time_ratio = 1f)
		{
			return time_ratio <= 0f || this.RCmagic_awake.Add(this.En.TS * NAI.aware_speed_ratio, true, false) >= this.NIRANtk(this.pl_magic_awake_time_min, this.pl_magic_awake_time_max, 493) * time_ratio;
		}

		public bool hasPrSer(SER ser)
		{
			return this.AimPr_ != null && this.AimPr is PR && (this.AimPr as PR).Ser.has(ser);
		}

		public bool is_aiming_target
		{
			get
			{
				return this.En.mpf_is_right > 0f == this.x < this.target_x;
			}
		}

		public bool here_dangerous
		{
			get
			{
				return (this.dummy_res & NAI.DUMMY_RES.HERE_DANGEROUS) > (NAI.DUMMY_RES)0;
			}
		}

		public void fineTicketStartPos()
		{
			this.ticket_start_x_ = this.En.x;
			this.ticket_start_y_ = this.En.y;
		}

		public NAI.FLAG current_flags
		{
			get
			{
				Dictionary<NAI.FLAG, float> rawObject = this.CFlags.getRawObject();
				NAI.FLAG flag = NAI.FLAG.NONE;
				foreach (KeyValuePair<NAI.FLAG, float> keyValuePair in rawObject)
				{
					flag |= keyValuePair.Key;
				}
				return flag;
			}
		}

		public bool isNoticePlayer()
		{
			return this.AimPr_ != null;
		}

		public bool hasTypeLock(NAI.TYPE f)
		{
			return this.CTypeLock.Has(f);
		}

		public void remTypeLock(NAI.TYPE f)
		{
			this.CTypeLock.Rem(f);
		}

		public NAI addTypeLock(NAI.TYPE f, float maxt)
		{
			this.CTypeLock.Add(f, maxt);
			return this;
		}

		public NAI clearTypeLock()
		{
			this.CTypeLock.Clear();
			return this;
		}

		public RAYHIT hitChecked(M2Ray Ray)
		{
			if ((Ray.hittype & HITTYPE.AUTO_TARGET) != HITTYPE.NONE)
			{
				this.dummyray_auto_targetted = true;
			}
			return RAYHIT.NONE;
		}

		public float footshift
		{
			get
			{
				if (this.En.base_gravity != 0f)
				{
					return 0f;
				}
				return -this.En.sizey;
			}
		}

		public override string ToString()
		{
			return "<NAI>" + this.getTicketInfoForDebug();
		}

		public M2AirRegionContainer getRegionCheckerAir()
		{
			return this.WRC_Air;
		}

		public string getTicketInfoForDebug()
		{
			return ((this.AimPr_ == null) ? "Sleep:" : (this.En.isOverDrive() ? "Od:" : "Awaken:")) + ((this.ticket_cnt > 0) ? this.ATicket[0].getTicketInfoForDebug() : " (no ticket) ");
		}

		public readonly NelEnemy En;

		private M2Attackable AimPr_;

		public float delay;

		public bool can_progress_delay_if_ticket_exists;

		public bool add_enemy_target_count = true;

		private NaTicket[] ATicket;

		private int ticket_cnt;

		public uint ran_tk;

		public uint ran_n;

		public uint ran_a;

		private FlagCounterR<NAI.FLAG> CFlags;

		private FlagCounterR<NAI.TYPE> CTypeLock;

		public const int player_search_delay = 14;

		public NAI.FnSearchPlayer fnSearchPlayer;

		public NAI.FnNaiLogic fnSleepLogic;

		public NAI.FnNaiLogic fnAwakeLogic;

		public NAI.FnNaiLogic fnOverDriveLogic;

		public float target_x;

		public float target_y;

		private float ticket_start_x_;

		private float ticket_start_y_;

		private static float aware_speed_ratio = 1f;

		public float pl_magic_awake_time_min = 10f;

		public float pl_magic_awake_time_max = 20f;

		public RevCounter RCmagic_awake = new RevCounter();

		public const int PRI_DONT_INJECTION = 128;

		public float busy_consider_intv = 5f;

		private float busycsd_t;

		public bool considerable_in_event;

		public bool consider_only_onfoot = true;

		public float attackable_length_x = 4f;

		public float attackable_length_top = -4f;

		public float attackable_length_bottom = 0.5f;

		public float suit_distance = 3.3f;

		public float suit_distance_overdrive = -1f;

		public float suit_distance_garaaki_ratio = 0.4f;

		public bool walk_only_linear;

		public bool no_check_move_fall_slide;

		public float awake_length = 10f;

		public const int AWAKE_LENGTH_DEFAULT = 10;

		public const int AWAKE_MARGIN_DEFAULT = 14;

		private float fine_target_pos_lock_;

		public float moveable_upper = 4.5f;

		public float moveable_lower = 5.5f;

		protected int action_count;

		private Vector2 FirstPos;

		private M2AirRegionContainer WRC_Air;

		private M2Ray RayDummy;

		public M2ChaserBaseFuncs Chaser;

		private NAI.DUMMY_RES dummy_res;

		private float autotarget_hitted_t;

		private bool dummyray_auto_targetted;

		private M2ChaserAir.ChaserRootAir CurrentCheckRoot;

		private TimeDeclineAreaContainer DeclCon;

		public static NAI.FnNaiLogic FD_SleepOnlyNearMana = delegate(NAI Nai)
		{
			if (Nai.hasPriorityTicket(0, false, false))
			{
				return false;
			}
			Nai.AddTicket(NAI.TYPE.WAIT, 0, true);
			return true;
		};

		public static NAI.FnNaiLogic FD_SleepOnly = delegate(NAI Nai)
		{
			if (Nai.hasPriorityTicket(0, false, false))
			{
				return false;
			}
			Nai.AddTicket(NAI.TYPE.WAIT, 0, true);
			return true;
		};

		private byte garaaki_state = 2;

		public delegate M2Attackable FnSearchPlayer(float fix_awake_length);

		public delegate bool FnTicketRun(NaTicket Tk);

		public delegate bool FnNaiLogic(NAI Nai);

		public enum TYPE
		{
			NOTHING,
			AWAKE,
			SLEEP,
			WALK,
			WALK_TO_WEED,
			PUNCH,
			PUNCH_0,
			PUNCH_1,
			PUNCH_2,
			PUNCH_WEED,
			MAG,
			MAG_0,
			MAG_1,
			MAG_2,
			GUARD,
			GUARD_0,
			GUARD_1,
			GUARD_2,
			APPEAL_0,
			BACKSTEP,
			WARP,
			GAZE,
			WAIT
		}

		public enum FLAG
		{
			NONE,
			SUMMON_APPEARED,
			AWAKEN,
			SLEEPED = 4,
			RECHECK_PLAYER = 8,
			JUMPED = 16,
			FOOTED = 32,
			OVERDRIVED = 64,
			ESCAPE = 128,
			GAZE = 256,
			TICKET_REFINED = 512,
			GAZE_CANNOT_ACCESS = 1024,
			ABSORB_FINISHED = 2048,
			STUN_FINISHED = 4096,
			ATTACKED = 65536,
			INJECTED = 131072,
			POWERED = 262144,
			BOTHERED = 524288,
			WANDERING = 1048576,
			_MAX = 2097152,
			_ADDITIONAL = 2031616
		}

		public enum DUMMY_RES
		{
			HERE_DANGEROUS = 2,
			CHECKED_LAST = 32,
			LAST_DANGEROUS = 64,
			CHECKING_NEXT = 256,
			CHECKED_NEXT = 512,
			NEXT_DANGEROUS = 1024,
			CALCED_ALL_DANGEROUS = 65536
		}
	}
}
