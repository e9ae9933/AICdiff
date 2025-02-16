using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNMush : NelEnemy
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			NOD.BasicData basicData = null;
			if (this.id == ENEMYID.MUSH_0_FLW)
			{
				this.id = ENEMYID.MUSH_0_FLW;
				basicData = NOD.getBasicData("MUSH_0_FLW");
				this.shot_after_time += 160;
			}
			else
			{
				this.id = ENEMYID.MUSH_0;
			}
			if (this.Amist_index == null)
			{
				if ((this.nattr & ENATTR.ICE) != ENATTR.NORMAL)
				{
					this.Amist_index = NelNMush.Amist_index_frozen;
				}
				else if ((this.nattr & ENATTR.ACME) != ENATTR.NORMAL)
				{
					this.Amist_index = NelNMush.Amist_index_acme;
				}
				else if ((this.nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
				{
					this.FnMistMushShotBreak = new MagicItem.FnMagicRun(this.shotThunderSplash);
				}
				else if ((this.nattr & ENATTR._MATTR) != ENATTR.NORMAL)
				{
					this.FnMistMushShotBreak = EnemyAttr.AFnSplash[EnemyAttr.mattrIndex(this.nattr & ENATTR._MATTR)];
				}
				else
				{
					this.Amist_index = NelNMush.Amist_index_base;
				}
			}
			if (basicData == null)
			{
				basicData = NOD.getBasicData("MUSH_0");
			}
			this.auto_absorb_lock_mover_hitting = false;
			base.appear(_Mp, basicData);
			this.cannot_move = (this.no_apply_gas_damage = true);
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 9f;
			this.Nai.attackable_length_top = -5f;
			this.Nai.attackable_length_bottom = 4f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.absorb_weight = 1;
			this.FD_MgRunMushShot = new MagicItem.FnMagicRun(this.MgRunMushShot);
			this.FD_MgDrawMushShot = new MagicItem.FnMagicRun(this.MgDrawMushShot);
			this.AtkOdAbsorb.Prepare(this, true);
		}

		public override void initOverDriveAppear()
		{
			base.initOverDriveAppear();
			this.cannot_move = (this.no_apply_map_damage = false);
			this.absorb_weight = 2;
		}

		public override void quitOverDrive()
		{
			base.quitOverDrive();
			this.releaseChargeCount();
			this.APtcPowerHolder = null;
			this.cannot_move = true;
			this.absorb_weight = 1;
		}

		protected override bool initDeathEffect()
		{
			this.releaseChargeCount();
			this.APtcPowerHolder = null;
			return base.initDeathEffect();
		}

		protected override bool runSummoned()
		{
			if (this.t <= 0f && !this.Od.pre_overdrive)
			{
				base.moveToFootablePosition();
			}
			return base.runSummoned() || this.t < 60f + X.NI(33, 66, this.Nai.RANn(5690));
		}

		public override void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			base.quitSummonAndAppear(clearlock_on_summon);
			if (!this.Od.pre_overdrive)
			{
				this.SpSetPose("awake", -1, null, false);
				return;
			}
			this.SpSetPose(base.hasFoot() ? "od_land" : "od_jump_3", -1, null, false);
		}

		public void releaseChargeCount()
		{
			if (this.APtcPowerHolder == null)
			{
				return;
			}
			for (int i = this.APtcPowerHolder.Count - 1; i >= 0; i--)
			{
				this.APtcPowerHolder[i].kill(false);
			}
			this.APtcPowerHolder.Clear();
		}

		public int getMistChargeCount()
		{
			if (this.APtcPowerHolder == null)
			{
				this.APtcPowerHolder = new List<PTCThread>(3);
			}
			return this.APtcPowerHolder.Count;
		}

		public void initChargeMist()
		{
			if (this.APtcPowerHolder == null)
			{
				this.APtcPowerHolder = new List<PTCThread>(3);
			}
			PTCThread ptcthread = base.PtcST("od_mush_charge_power", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
			if (ptcthread != null)
			{
				this.APtcPowerHolder.Add(ptcthread);
			}
		}

		public bool shiftMistChargeEffect()
		{
			if (this.APtcPowerHolder == null || this.APtcPowerHolder.Count == 0)
			{
				return false;
			}
			this.APtcPowerHolder[0].kill(false);
			this.APtcPowerHolder.RemoveAt(0);
			return this.APtcPowerHolder.Count > 0;
		}

		public override bool runOverDriveActivate()
		{
			bool flag = base.runOverDriveActivate();
			if (this.t <= 0f)
			{
				this.Anm.setDuration(this.Od.transform_duration);
			}
			if (flag)
			{
				this.Nai.delay = 45f;
			}
			return flag;
		}

		public bool runOverDriveAppeal(NaTicket Tk)
		{
			return base.runOverDriveAppealBasic(Tk.initProgress(this), Tk, "od_appeal", "stand", 100, 130);
		}

		private bool canAttackSmallPunch()
		{
			float num = (this.TkSmallPunch.x_reachable + X.Mx(0f, this.sizex - this.sizex0)) * 0.85f;
			return this.Nai.isAttackableLength(num, -2.4f, 0.25f, false);
		}

		private bool isPrNearLengthForGuard(float margin_x = 0f)
		{
			return this.Nai.isAttackableLength(this.Nai.isPrMagicChanting(1f) ? (1.9f + margin_x * 0.5f) : (4f + margin_x), -2.4f, 0.25f, false);
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai, NAI.TYPE.GAZE))
			{
				return true;
			}
			if (!base.isOverDrive())
			{
				bool flag = false;
				bool flag2 = !this.Od.near_overdrive && base.mp_ratio < 0.5f;
				if (Nai.isPrGaraakiState())
				{
					if (this.canAttackSmallPunch() && Nai.fnBasicPunch(Nai, 6, 15f + X.ZLINE(base.mp_ratio - 0.2f, 0.5f) * 20f, 0f, 0f, 0f, 8841, false))
					{
						return true;
					}
					flag = true;
				}
				else if (this.isPrNearLengthForGuard(0f))
				{
					if (Nai.RANtk(389) < 0.8f && this.guard_hitted >= 0 && !Nai.hasPriorityTicket(6, false, false))
					{
						return Nai.AddTicketB(NAI.TYPE.GUARD, 100, false);
					}
				}
				else
				{
					if (this.guard_hitted < 0)
					{
						this.guard_hitted = 0;
					}
					flag = true;
				}
				if (this.Useable(this.McsShot, 1f, 0f) && !base.hasPT(6, false, false))
				{
					int num = (Nai.isPrGaraakiState() ? 40 : (Nai.isPrMagicChanting(1f) ? 20 : 0));
					float num2 = ((base.mp_ratio >= 0.5f) ? ((float)(33 + num)) : ((float)(10 + num) + 38f * (base.mp_ratio * 2f)));
					if (this.Amist_index == null)
					{
						X.ALL0(NelNMush.Aratio_buf);
						NelNMush.Aratio_buf[0] = 50;
					}
					else
					{
						for (int i = 0; i < 4; i++)
						{
							NelNMush.Aratio_buf[i] = ((i >= this.Amist_index.Length) ? 0 : ((int)(num2 * (float)(this.Amist_index.Length - i) * 0.5f)));
						}
						if (this.Amist_index.Length > 1)
						{
							for (int j = this.Amist_index.Length - 1; j >= 0; j--)
							{
								SER first = NelNMush.AMistKind[this.Amist_index[j]].AAtk[0].SerDmg.GetFirst();
								if (Nai.hasPrSer(first))
								{
									NelNMush.Aratio_buf[j] = (int)((float)NelNMush.Aratio_buf[j] * 0.04f);
								}
							}
						}
					}
					if (Nai.fnBasicMagic(Nai, 6, (float)NelNMush.Aratio_buf[0], (float)NelNMush.Aratio_buf[1], (float)NelNMush.Aratio_buf[2], (float)NelNMush.Aratio_buf[3], 7145, false))
					{
						return true;
					}
				}
				if (flag && flag2)
				{
					float num3 = (this.TkSmallPunch.x_reachable + X.Mx(0f, this.sizex - this.sizex0)) * 0.85f;
					if (Nai.AddTicketSearchAndGetManaWeed(1, num3, -0.4f, 0.4f, num3, -0.4f, 0.4f, true) != null)
					{
						return true;
					}
				}
			}
			return Nai.fnBasicMove(Nai);
		}

		public override bool readTicket(NaTicket Tk)
		{
			if (base.isOverDrive())
			{
				return this.readTicketOd(Tk);
			}
			switch (Tk.type)
			{
			case NAI.TYPE.WALK:
			case NAI.TYPE.WALK_TO_WEED:
			case NAI.TYPE.WAIT:
				this.SpSetPose("stand", -1, null, false);
				base.AimToLr((X.xors(2) == 0) ? 0 : 2);
				Tk.after_delay = 20f + this.Nai.RANtk(840) * 20f;
				return false;
			case NAI.TYPE.PUNCH:
				if (Tk.initProgress(this))
				{
					this.t = 0f;
					this.walk_st = 0;
					this.SpSetPose("awake", -1, null, false);
				}
				if (Tk.prog == PROG.ACTIVE)
				{
					if (this.t < 40f)
					{
						return true;
					}
					Tk.prog = PROG.PROG0;
				}
				return this.runSmallPunch(-1);
			case NAI.TYPE.PUNCH_WEED:
				return this.runSmallPunch(-1);
			case NAI.TYPE.MAG:
			case NAI.TYPE.MAG_0:
			case NAI.TYPE.MAG_1:
			case NAI.TYPE.MAG_2:
				return this.runShot(Tk.initProgress(this), Tk);
			case NAI.TYPE.GUARD:
				return this.runGuard(Tk.initProgress(this), Tk);
			case NAI.TYPE.GAZE:
				base.readTicket(Tk);
				return Tk.t < 20f + this.Nai.RANtk(445) * 40f;
			}
			return base.readTicket(Tk);
		}

		private bool considerOverDrive(NAI Nai)
		{
			if (Nai.HasF(NAI.FLAG.OVERDRIVED, true))
			{
				Nai.AddTicket(NAI.TYPE.APPEAL_0, 128, true);
				return true;
			}
			if (Nai.HasF(NAI.FLAG.POWERED, true) || (Nai.HasF(NAI.FLAG.ABSORB_FINISHED, true) && !Nai.hasTypeLock(NAI.TYPE.BACKSTEP)))
			{
				NaTicket naTicket = Nai.AddTicketBackStep(1000, 7f, 9f, false);
				if (naTicket != null)
				{
					naTicket.CheckNearPlaceError(2);
					Nai.addTypeLock(NAI.TYPE.BACKSTEP, 160f);
					return true;
				}
				return true;
			}
			else
			{
				if (!base.hasFoot() && !Nai.hasTypeLock(NAI.TYPE.WALK))
				{
					Nai.AddTicket(NAI.TYPE.WALK, 0, true).CheckNearPlaceError(2);
					return true;
				}
				if (!Nai.hasPriorityTicket(159, false, false))
				{
					bool flag = Nai.isPrGaraakiState();
					if (flag && Nai.isPrTortured())
					{
						if (this.getMistChargeCount() < 3)
						{
							Nai.AddTicket(NAI.TYPE.GUARD_0, 160, true);
							return true;
						}
						if (Nai.RANtk(491) < 0.5f)
						{
							Nai.AddTicket(NAI.TYPE.GAZE, 0, true);
							return true;
						}
						Nai.AddTicket(NAI.TYPE.MAG, 160, true);
						return true;
					}
					else
					{
						if (!Nai.hasTypeLock(NAI.TYPE.PUNCH) && Nai.RANtk(499) < (flag ? 0.8f : 0.4f) && X.BTW(1.5f, X.Abs(Nai.dep_dx), 9.5f))
						{
							Nai.AddTicket(NAI.TYPE.PUNCH, 160, true);
						}
						if (!Nai.hasTypeLock(NAI.TYPE.GUARD) && Nai.target_foot_slen > Nai.NIRANtk(4f, 7f, 4811) && Nai.RANtk(9291) < 0.6f)
						{
							Nai.AddTicket(NAI.TYPE.GUARD, 160, true);
						}
					}
				}
				if (!Nai.hasPriorityTicket(1, false, false) && Nai.target_xdif < 3.2f && !Nai.isPrGaraakiState() && Nai.RANtk(711) < 0.7f)
				{
					NaTicket naTicket2 = Nai.AddTicketBackStep(1000, 7f, 9f, false);
					if (naTicket2 != null)
					{
						naTicket2.CheckNearPlaceError(2);
						return true;
					}
				}
				if (!Nai.hasPriorityTicket(0, false, false) && X.Abs(Nai.dep_dx) > 8f && Nai.RANtk(7451) < 0.7f && !Nai.hasTypeLock(NAI.TYPE.WALK))
				{
					Nai.AddTicket(NAI.TYPE.WALK, 0, true);
					return true;
				}
				if (!Nai.hasPriorityTicket(159, false, false) && X.Abs(Nai.dep_dx) <= 8f)
				{
					if (this.getMistChargeCount() > 0 && Nai.RANtk(3188) < 0.25f)
					{
						Nai.AddTicket(NAI.TYPE.MAG, 160, true);
						return true;
					}
					if (Nai.RANtk(3059) < 0.85f)
					{
						Nai.AddTicket(NAI.TYPE.GUARD, 160, true);
						return true;
					}
				}
				if (!Nai.hasPriorityTicket(1, false, false))
				{
					if (Nai.HasF(NAI.FLAG.ESCAPE, true) || Nai.target_foot_slen < 5f || Nai.RANtk(2314) < 0.25f)
					{
						NaTicket naTicket3 = Nai.AddTicketBackStep(1000, 7f, 9f, false);
						if (naTicket3 != null)
						{
							naTicket3.CheckNearPlaceError(2);
							return true;
						}
					}
					Nai.AddTicket(NAI.TYPE.WALK, 1, true).CheckNearPlaceError(2);
				}
				return true;
			}
		}

		public bool readTicketOd(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.PUNCH)
			{
				if (type != NAI.TYPE.WALK)
				{
					if (type == NAI.TYPE.PUNCH)
					{
						return this.runOdJumpAttack(Tk);
					}
				}
				else
				{
					bool flag = Tk.initProgress(this);
					int num = base.walkThroughLift(flag, Tk, 20);
					if (num >= 0)
					{
						return num == 0;
					}
					this.SpSetPose("stand", -1, null, false);
					Tk.initProgress(this);
					float num2 = 10f + this.Nai.RANtk(498) * 15f;
					this.Phy.addFoc(FOCTYPE.WALK, 0.035714287f * base.mpf_is_right, 0f, -1f, 0, (int)num2, 0, -1, 0);
					this.Nai.delay = num2;
					return false;
				}
			}
			else
			{
				if (type == NAI.TYPE.MAG)
				{
					return this.runOdMistBreathe(Tk.initProgress(this), Tk);
				}
				switch (type)
				{
				case NAI.TYPE.GUARD:
				case NAI.TYPE.GUARD_0:
					return this.runOdCharge(Tk.initProgress(this), Tk, Tk.type == NAI.TYPE.GUARD);
				case NAI.TYPE.APPEAL_0:
					return this.runOverDriveAppeal(Tk);
				case NAI.TYPE.BACKSTEP:
					return this.runBackStep(Tk);
				case NAI.TYPE.GAZE:
					base.readTicket(Tk);
					return Tk.t < 20f + this.Nai.RANtk(445) * 40f;
				}
			}
			return base.readTicket(Tk);
		}

		protected bool runGuard(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.walk_time = 0f;
				this.walk_st = 256;
				this.guard_hitted = 0;
				base.AimToPlayer();
			}
			if (Tk.prog != PROG.PROG0)
			{
				if (Tk.prog == PROG.PROG1)
				{
					if (this.runSmallPunch(-1))
					{
						return true;
					}
					this.walk_st = 256;
					this.walk_time = 0f;
					Tk.prog = PROG.ACTIVE;
				}
				if (this.walk_time <= 0f)
				{
					if (this.Nai.isPrGaraakiState() || !this.isPrNearLengthForGuard(-1f + 3f * ((float)this.walk_st / 256f)))
					{
						base.AimToPlayer();
						this.t = 0f;
						this.walk_time = 0f;
						Tk.prog = PROG.PROG0;
						if (this.SpPoseIs("guard"))
						{
							this.SpSetPose("appear", -1, null, false);
						}
						return true;
					}
					if (init_flag)
					{
						this.SpSetPose("awake", 1, null, false);
						base.PtcST("mush_guard_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.walk_time = 20f;
					}
					else
					{
						if (this.SpPoseIs("awake"))
						{
							this.SpSetPose("guard", -1, null, false);
							base.PtcST("mush_guard", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						}
						else
						{
							this.walk_st = X.xors(256);
						}
						this.walk_time = 34f;
					}
				}
				if (this.guard_hitted >= X.IntR(1f + this.Nai.RANtk(4471) * 2.8f) && this.walk_time <= 10f && this.isPrNearLengthForGuard(0.25f))
				{
					this.guard_hitted = -1;
					this.walk_time = (float)(this.walk_st = 0);
					Tk.prog = PROG.PROG1;
					base.AimToPlayer();
					return true;
				}
				this.walk_time -= this.TS;
			}
			else if (this.t >= 60f)
			{
				if (this.guard_hitted >= 0)
				{
					this.guard_hitted = 0;
				}
				this.SpSetPose("stand", -1, null, false);
				return false;
			}
			return true;
		}

		public bool runSmallPunch(int default_aim)
		{
			if (this.walk_st >= 0)
			{
				this.walk_st = -1;
				this.t = 0f;
				if (default_aim >= 0)
				{
					base.AimToLr(default_aim);
				}
				this.SpSetPose("punch_0", 1, null, false);
			}
			int num = -this.walk_st % 2;
			if (this.t >= 18f && num == 1)
			{
				this.walk_st--;
				base.tackleInit(this.AtkSmallPunch, this.TkSmallPunch, MGHIT.AUTO);
				base.PtcVar("scl", (double)this.Anm.scaleX).PtcST("mush_small_punch_swing", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			else
			{
				if (this.can_hold_tackle && this.t >= 22f)
				{
					this.can_hold_tackle = false;
				}
				if (this.t >= 40f)
				{
					int num2 = this.walk_st - 1;
					this.walk_st = num2;
					if (num2 <= -4)
					{
						if (default_aim >= 0)
						{
							base.AimToLr(default_aim);
						}
						else
						{
							base.AimToPlayer();
						}
						this.walk_st = 0;
						return false;
					}
					this.t = 0f;
					base.AimToLr((this.aim == AIM.L) ? 2 : 0);
					this.SpSetPose("punch_0", 1, null, false);
				}
			}
			return true;
		}

		public bool runShot(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				if (!this.Useable(this.McsShot, 1f, 0f))
				{
					return false;
				}
				this.SpSetPose("shot_0", -1, null, false);
				this.playSndPos("mush_shot_0", 1);
				this.t = 0f;
			}
			if (this.t >= 5f && this.t < 57f)
			{
				this.Nai.fine_target_pos_lock = 10f;
			}
			if (this.t >= 57f && Tk.prog == PROG.ACTIVE)
			{
				if (!this.Useable(this.McsShot, 1f, 0f))
				{
					return false;
				}
				this.SpSetPose("shot_1", -1, null, false);
				Tk.prog = PROG.PROG0;
				MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunMushShot, this.FD_MgDrawMushShot);
				if (this.Amist_index == null)
				{
					magicItem.Other = this.FnMistMushShotBreak;
				}
				else
				{
					int num = this.Amist_index[Tk.type - NAI.TYPE.MAG];
					if (num >= 0)
					{
						magicItem.Other = NelNMush.AMistKind[num];
					}
					else
					{
						ENATTR enattr = (ENATTR)(-(ENATTR)num);
						magicItem.Other = EnemyAttr.AFnSplash[EnemyAttr.mattrIndex(enattr)];
					}
				}
				this.MpConsume(this.McsShot, magicItem, 1f, 1f);
				this.Nai.delay = 30f;
			}
			return this.t < (float)this.shot_after_time || Tk.prog != PROG.PROG0;
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			if (this.Nai == null || !this.is_alive)
			{
				return false;
			}
			if (Mg.kind == MGKIND.TACKLE)
			{
				return this.canAbsorbContinue() && this.can_hold_tackle && !this.Ser.has(SER.TIRED);
			}
			return Mg.kind == MGKIND.BASIC_SHOT;
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			NelEnemy.STATE state = this.state;
			if (state == NelEnemy.STATE.STAND)
			{
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
			}
			base.changeState(st);
			if (base.isAbsorbState(state))
			{
				this.Nai.addTypeLock(NAI.TYPE.PUNCH, 400f);
				this.Nai.remTypeLock(NAI.TYPE.BACKSTEP);
				NaTicket naTicket = this.Nai.AddTicketBackStep(162, 6f, 15f, false);
				if (naTicket != null)
				{
					naTicket.AfterDelay(80f);
				}
			}
			return this;
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
			base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
		}

		private bool runBackStep(NaTicket Tk)
		{
			if (Tk.initProgress(this))
			{
				this.setAim((base.x < Tk.depx) ? AIM.L : AIM.R, false);
				this.SpSetPose("od_backstep_0", -1, null, false);
				this.t = 0f;
				this.walk_time = base.x;
			}
			if (Tk.prog == PROG.ACTIVE && this.t >= 15f)
			{
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.t = 0f;
				Tk.prog = PROG.PROG0;
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK, 0f, -0.19f, -1f, 0, 4, 8, -1, 0);
				this.Phy.addFoc(FOCTYPE.WALK, (Tk.depx - this.walk_time) * 0.75f / 12f, 0f, -1f, 0, 12, 0, -1, 0);
				base.PtcVar("ydf", (double)(this.sizey * 2.3f * this.Mp.CLENB)).PtcST("enemy_backstep", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Phy.addLockMoverHitting(HITLOCK.AIR, 12f);
				base.tackleInit(this.AtkTackleBackStep, this.TkOdBackStep, MGHIT.AUTO);
			}
			if (Tk.prog == PROG.PROG0 && this.t >= 12f)
			{
				this.SpSetPose("od_backstep_1", -1, null, false);
				base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.t = 0f;
				Tk.prog = PROG.PROG1;
				float num = 16f;
				float num2 = (Tk.depx - this.walk_time) * 0.25f;
				float num3 = 2f * num2 / num;
				this.Phy.addFoc(FOCTYPE.WALK, num3, 0f, -1f, 0, 0, (int)num, -1, 0);
				this.can_hold_tackle = false;
			}
			if (Tk.prog == PROG.PROG1 && this.t >= 16f && base.hasFoot())
			{
				if (!this.Nai.hasTypeLock(NAI.TYPE.BACKSTEP) && (this.Nai.HasF(NAI.FLAG.ESCAPE, true) || this.Nai.target_foot_slen < 5f))
				{
					if (this.getMistChargeCount() > 0 && this.Nai.RANtk(3214) < 0.1f + 0.25f * (float)this.getMistChargeCount() && (this.getMistChargeCount() > 1 || this.Nai.target_sxdif < 3f))
					{
						Tk.Recreate(NAI.TYPE.MAG, -1, true, null);
						return this.runOdMistBreathe(Tk.initProgress(this), Tk);
					}
					if (this.Nai.RANtk(4357) < 0.4f)
					{
						Tk.Recreate(NAI.TYPE.PUNCH, -1, true, null);
						return this.runOdJumpAttack(Tk);
					}
				}
				this.Nai.delay = 25f;
				Tk.prog = PROG.PROG2;
			}
			return Tk.prog != PROG.PROG2;
		}

		private bool runOdJumpAttack(NaTicket Tk)
		{
			if (Tk.initProgress(this))
			{
				this.playSndPos("overdrive_mush_atk0", 1);
				this.SpSetPose("od_jump_0", -1, null, false);
				this.t = 0f;
			}
			if (Tk.prog == PROG.ACTIVE && this.t >= 30f)
			{
				this.t = 0f;
				Tk.prog = PROG.PROG1;
				base.PtcST("od_bigjump", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Phy.addLockMoverHitting(HITLOCK.AIR, 50f);
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.jumpInit(X.NIXP(9f, 13f) * base.mpf_is_right, 0f, X.NIXP(4f, 6f), false);
				this.SpSetPose((Tk.type == NAI.TYPE.MAG) ? "od_jump_diffusion" : "od_jump_1", -1, null, false);
				this.Nai.fine_target_pos_lock = 80f;
				if (Tk.type == NAI.TYPE.PUNCH)
				{
					base.tackleInit(this.AtkTackleOdMisogi, this.TkOdMisogi, MGHIT.AUTO).Ray.hittype &= ~HITTYPE.EN;
				}
			}
			if (Tk.prog >= PROG.PROG1)
			{
				if (Tk.type == NAI.TYPE.MAG)
				{
					if (base.hasFoot())
					{
						base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
						this.t = 0f;
						this.Phy.remLockMoverHitting(HITLOCK.AIR);
						Tk.prog = PROG.PROG0;
						this.SpSetPose("od_land", -1, null, false);
					}
				}
				else
				{
					if (Tk.prog == PROG.PROG1 && this.t >= 20f)
					{
						float num = Tk.depx + base.mpf_is_right * ((!this.Nai.isPrGaraakiState()) ? X.NI(-0.34f, 3.2f, this.Nai.RANtk(4154)) : X.NI(-0.24f, 0.24f, this.Nai.RANtk(4154)));
						if (X.Abs(base.x - num) < 0.2f)
						{
							Tk.prog = PROG.PROG4;
							this.t = 0f;
							this.playSndPos("od_fallstart", 1);
							this.SpSetPose("od_jump_2", -1, null, false);
							this.Phy.killSpeedForce(true, true, true, false, false).remFoc(FOCTYPE.WALK, true);
							this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK, 0f, -0.12f, -1f, 0, 8, 16, -1, 0);
							this.Phy.addLockMoverHitting(HITLOCK.AIR, 50f);
							this.can_hold_tackle = false;
						}
					}
					if (Tk.prog == PROG.PROG4 && this.t >= 24f)
					{
						Tk.prog = PROG.PROG5;
						this.t = 0f;
						base.PtcVar("xdf", (double)(this.sizex * 2.2f * this.Mp.CLENB)).PtcST("enemy_bigfall", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.SpSetPose("od_jump_3", -1, null, false);
						this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._RELEASE | FOCTYPE._GRAVITY_LOCK | FOCTYPE._CHECK_WALL, 0f, 0.6f, -1f, 0, 36, 0, -1, 0);
						base.tackleInit(this.AtkTackleOdMisogi, this.TkOdMisogiFall, MGHIT.AUTO);
					}
					if (base.hasFoot())
					{
						this.t = 0f;
						this.Phy.remLockMoverHitting(HITLOCK.AIR);
						bool flag = Tk.prog == PROG.PROG5;
						Tk.prog = PROG.PROG0;
						this.can_hold_tackle = false;
						base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
						if (flag)
						{
							EnemyAttr.Splash(this, 2.5f);
							this.SpSetPose("od_land_jump3", -1, null, false);
							base.PtcVar("by", (double)base.mbottom).PtcST("od_explode", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						}
						else
						{
							this.SpSetPose("od_land_from_bigjump", -1, null, false);
						}
					}
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 4f)
				{
					this.can_hold_tackle = false;
				}
				if (this.t >= 40f && base.hasFoot())
				{
					base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					return false;
				}
			}
			return true;
		}

		public bool runOdCharge(bool init_flag, NaTicket Tk, bool connect_to_other_state)
		{
			if (init_flag)
			{
				if (this.getMistChargeCount() >= 3)
				{
					Tk.Recreate(NAI.TYPE.MAG, -1, true, null);
					return this.runOdMistBreathe(Tk.initProgress(this), Tk);
				}
				this.walk_time = 100f;
				base.PtcST("od_mush_charge_mist", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				this.SpSetPose("od_appeal", -1, null, false);
				this.t = 0f;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				this.walk_time -= ((!this.Useable(this.McsOdCharge, 1f, 0f)) ? 0.5f : 1f) * this.TS;
				if (this.walk_time <= 0f)
				{
					this.t = 0f;
					Tk.prog = PROG.PROG0;
					base.PtcST("od_mush_charge_mist_end", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.SpSetPose("od_charge_end", -1, null, false);
					this.initChargeMist();
					this.MpConsume(this.McsOdCharge, null, 1f, 1f);
				}
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, 30, true))
			{
				if (connect_to_other_state)
				{
					if (this.getMistChargeCount() >= 3 && (double)this.Nai.RANtk(3449) < 0.8 && this.Nai.target_sxdif < 8f)
					{
						Tk.Recreate(NAI.TYPE.MAG, -1, true, null);
						return this.runOdMistBreathe(Tk.initProgress(this), Tk);
					}
					if (this.Nai.HasF(NAI.FLAG.ESCAPE, true) || this.Nai.target_foot_slen < 4f)
					{
						if (this.Nai.RANtk(3214) < 0.1f + 0.25f * (float)this.getMistChargeCount() && (this.getMistChargeCount() > 1 || this.Nai.target_sxdif < 3f))
						{
							Tk.Recreate(NAI.TYPE.MAG, -1, true, null);
							return this.runOdMistBreathe(Tk.initProgress(this), Tk);
						}
						this.Nai.AddF(NAI.FLAG.POWERED, 180f);
						return false;
					}
				}
				Tk.AfterDelay(40f);
				return false;
			}
			return true;
		}

		public bool runOdMistBreathe(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				base.AimToPlayer();
				this.SpSetPose("od_land_from_bigjump_1", -1, null, false);
				base.PtcST("mist_breathe_init", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				this.t = 65f;
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.t >= 92f)
				{
					this.SpSetPose("od_jump_0", -1, null, false);
				}
				this.can_hold_tackle = false;
				if (Tk.Progress(ref this.t, 100, true))
				{
					base.AimToPlayer();
					base.addF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT);
					base.PtcVar("by", (double)(base.y + this.sizey * 0.75f)).PtcST("overdrive_mush_jump_gas_burst", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.SpSetPose("od_jump_diffusion", -1, null, false);
					bool flag = !this.shiftMistChargeEffect();
					this.jumpInit(X.NIXP(7f, 9f) * (flag ? 0.7f : 1f) * (float)CAim._XD(this.aim, 1), 0f, X.NIXP(2f, 4.4f) + (float)(flag ? 2 : 0), false);
					this.walk_time = 0f;
					if (flag)
					{
						Tk.prog = PROG.PROG5;
					}
					base.tackleInit(this.AtkTackleBackStep, this.TkOdMisogi, MGHIT.AUTO);
				}
			}
			else if (Tk.prog == PROG.PROG0 || Tk.prog == PROG.PROG5)
			{
				if (this.walk_time <= 0f)
				{
					this.walk_time = 20f;
					NelNMush.MkOd.fnApply = new MistManager.FnMistApply(this.fnOdMistApply);
					MistManager.MistKind mistKind = NelNMush.MkOd;
					if (base.nattr_has_mattr)
					{
						if (this.Amist_index != null)
						{
							int num = this.Amist_index[0];
							if (num >= 0)
							{
								mistKind = NelNMush.AMistKind[num];
							}
						}
						else if (this.FnMistMushShotBreak is MagicItem.FnMagicRun)
						{
							(this.FnMistMushShotBreak as MagicItem.FnMagicRun)(null, X.XORSP() * 6.2831855f);
						}
					}
					base.nM2D.MIST.addMistGenerator(mistKind, mistKind.calcAmount(140, 1.5f), (int)base.x, (int)base.y - 1, false);
				}
				this.walk_time -= this.TS;
				if (this.t >= 10f)
				{
					base.remF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT);
					if (base.hasFoot())
					{
						this.can_hold_tackle = false;
						this.SpSetPose("od_land_from_bigjump_1", -1, null, false);
						if (base.nattr_has_mattr && this.Amist_index == null && this.FnMistMushShotBreak != null && this.FnMistMushShotBreak is EnemyAttr.FnDelegateSetSplash)
						{
							(this.FnMistMushShotBreak as EnemyAttr.FnDelegateSetSplash)(this, base.x, base.y + this.sizey * 0.66f, 1.6f, 1f);
						}
						if (Tk.prog == PROG.PROG5)
						{
							if (this.Nai.target_slen < 4f)
							{
								this.Nai.remTypeLock(NAI.TYPE.BACKSTEP);
								NaTicket naTicket = this.Nai.AddTicketBackStep(Tk.priority - 2, 6f, 15f, true);
								if (naTicket != null)
								{
									Tk.AfterDelay(25f);
									naTicket.AfterDelay(60f);
									return false;
								}
							}
							Tk.AfterDelay(this.Nai.NIRANtk(90f, 120f, 8515));
							return false;
						}
						Tk.prog = PROG.ACTIVE;
						this.t = (float)(100 - ((this.getMistChargeCount() == 1) ? 30 : 20));
					}
				}
			}
			return true;
		}

		private int fnOdMistApply(MistManager.MistKind K, NelM2Attacker Atk, float level01)
		{
			if (!(Atk is PR) || !this.is_alive)
			{
				return 0;
			}
			if (!(Atk as PR).attachYdrgListener(this, new YdrgManager.FnYdrgApplyDamage(this.fnYdrgApplyDamage), 0.25f, 5, 13))
			{
				return 0;
			}
			return 1;
		}

		public bool fnYdrgApplyDamage(NelEnemy En, PR Pr, int level)
		{
			if (!this.is_alive)
			{
				return true;
			}
			this.AtkOdYdrg.AttackFrom = this;
			this.AtkOdYdrg.Caster = this;
			Pr.applyYdrgDamage(this.AtkOdYdrg);
			this.addHpWithAbsorbing(this.AtkOdYdrg._hpdmg);
			return true;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			if (base.isOverDrive())
			{
				A.Add("shrimp");
				Aattr.Add(MGATTR.ABSORB);
			}
			Aattr.Add(MGATTR.POISON);
			Aattr.Add(MGATTR.NORMAL);
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (!base.isOverDrive() || (!base.Useable(this.TkOdMisogi, 1f) && (this.Nai.isPrAlive() || X.XORSP() < 0.4f)))
			{
				return false;
			}
			if (Abm.Con.current_pose_priority > 99)
			{
				return false;
			}
			if (!base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			Abm.target_pose = "torture_mush_0";
			Abm.kirimomi_release = false;
			Abm.get_Gacha().activate(PrGachaItem.TYPE.SEQUENCE, 4, 15U).SoloPositionPixel = new Vector3(0f, 30f, 0f);
			return true;
		}

		public bool plantEggToPr(PR Pr, float check_ratio = 1f)
		{
			PrEggManager.CATEG categ = PrEggManager.CATEG.MUSH;
			if (Pr.EggCon.applyEggPlantDamage(0.08f, categ, true, check_ratio) > 0)
			{
				this.TeCon.setQuakeSinH(15f, 40, X.NI(19f, 38f, 0.5f), 0f, 0);
				return true;
			}
			return false;
		}

		public override bool runAbsorb()
		{
			if (!base.isOverDrive())
			{
				return false;
			}
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.t <= 0f)
			{
				this.Absorb.changeTorturePose("torture_mush_0", false, true, -1, -1);
				this.Anm.showToFront(false, false);
				this.walk_st = 0;
				this.Phy.killSpeedForce(true, true, true, false, false).remFoc(FOCTYPE.WALK | FOCTYPE.JUMP, true);
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK, 0f, -0.1f, -1f, 0, 13, 0, -1, 0);
				this.Nai.remTypeLock(NAI.TYPE.BACKSTEP);
			}
			if (this.walk_st == 0 && this.t >= 32f && base.hasFoot())
			{
				this.walk_st = 1;
				this.t = 1f;
				this.Absorb.changeTorturePose("torture_mush_1", false, false, -1, -1);
				base.PtcVar("by", (double)base.mbottom).PtcVar("xdf", (double)(this.sizex * 2.3f * this.Mp.CLENB)).PtcST("od_absorbing_land", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.plantEggToPr(pr, 0.6f);
				if (X.XORSP() < 0.4f)
				{
					pr.attachYdrgListener(this, new YdrgManager.FnYdrgApplyDamage(this.fnYdrgApplyDamage), 1f, 5, 13);
				}
			}
			if (this.walk_st == 1 && this.t >= 24f && base.hasFoot())
			{
				this.walk_st = 106 + X.xors(4);
				this.walk_time = 25f;
				this.Absorb.changeTorturePose("torture_mush_2", false, false, -1, -1);
				base.PtcST("mush_od_absorb_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (this.walk_st >= 2)
			{
				pr.Ser.Add(SER.STRONG_HOLD, -1, 99, false);
				if (this.walk_time > 0f)
				{
					this.walk_time -= this.TS;
				}
				else
				{
					this.Absorb.uipicture_fade_key = "shrimp";
					int num = 3;
					int num2 = this.walk_st - 1;
					this.walk_st = num2;
					if (num2 <= 100)
					{
						if (this.walk_st <= 95 || (this.walk_st < 100 && X.XORSP() < 0.5f))
						{
							this.walk_st = 104 + X.xors(19);
							this.walk_time = 2f + X.XORSP() * 7f;
						}
						else
						{
							if (!this.SpPoseIs("od_torture_mush_2"))
							{
								this.Absorb.changeTorturePose("torture_mush_2", false, false, -1, -1);
							}
							this.walk_time = 30f + X.XORSP() * 60f;
						}
						num = 12;
					}
					else
					{
						if (!this.SpPoseIs("od_torture_mush_3"))
						{
							this.Absorb.changeTorturePose("torture_mush_3", false, false, -1, -1);
						}
						this.walk_time = 6f + X.XORSP() * 15f;
					}
					if (X.XORSP() < 0.8f)
					{
						this.playSndPos("absorb_kiss", 1);
					}
					if (X.XORSP() < 0.8f)
					{
						this.playSndPos("absorb_guchu", 1);
					}
					if (X.XORSP() < 0.4f)
					{
						this.Anm.randomizeFrame(0.5f, 0.5f);
					}
					base.applyAbsorbDamageTo(pr, this.AtkOdAbsorb, X.XORSP() < 0.68f, false, false, 0f, false, null, false, true);
					if (X.XORSP() < 0.67f)
					{
						this.Mp.DropCon.setLoveJuice(base.AimPr, num, uint.MaxValue, 1f, false);
					}
					this.plantEggToPr(pr, 0.03f);
					if (X.XORSP() < 0.05f)
					{
						pr.attachYdrgListener(this, new YdrgManager.FnYdrgApplyDamage(this.fnYdrgApplyDamage), 1f, 5, 13);
					}
				}
				if (this.walk_st <= 100 && X.XORSP() < 0.03f)
				{
					this.playSndPos("kuchul", 1);
				}
			}
			return true;
		}

		public bool MgRunMushShot(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (Mg.t <= 0f)
			{
				float num = 1.5707964f - base.mpf_is_right * 0.14f * 3.1415927f;
				Mg.sx = base.x + X.Cos(num) * 1.2f * this.enlarge_level;
				Mg.sy = base.y - X.Sin(num) * 1.2f * this.enlarge_level;
				X.ZLINE(X.Abs(this.Nai.target_x - Mg.sx), 3.3f);
				Vector4 jumpVelocity = M2Mover.getJumpVelocity(this.Mp, X.absMx(this.Nai.target_x - Mg.sx, 3f) + X.NIXP(-0.5f, 1.5f), this.Nai.target_y - 1.5f - Mg.sy, 5f, 0.23f, 0f);
				Mg.createDropper(jumpVelocity.x * 1.25f, jumpVelocity.y, 0.125f, -1f, -1f);
				Mg.sa = X.GAR2(jumpVelocity);
				Mg.Atk0 = this.AtkShotHit;
				Mg.Ray.projectile_power = 10;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.REFLECTED;
			}
			return fcnt == 0f || NelNMush.MgRunMushShotS(Mg, fcnt, "mush_shot_init", 140, 0.23f, 0.8f, 0.4f, 1f, -1, 0.007f);
		}

		public static bool MgRunMushShotS(MagicItem Mg, float fcnt, string init_ptcst_key, int _mist_amount_frm, float _shot_gravity_scale, float bounce_vx, float bounce_vy, float rotate_spd_ratio = 1f, int explode_center_shift_y = -1, float reduce_ax_in_ground = 0.007f)
		{
			if (Mg.t <= 0f)
			{
				Mg.Ray.RadiusM(0.12f).HitLock(40f, null);
				Mg.wind_apply_s_level = 1f;
				Mg.efpos_s = (Mg.raypos_s = true);
				float sa = Mg.sa;
				object other = Mg.Other;
				Mg.sa = X.NIXP(0f, 6.2831855f);
				Mg.da = X.NIXP(70f, 100f);
				Mg.PtcVar("agR", (double)sa).PtcVar("hagR", (double)(sa + 1.5707964f)).PtcST(init_ptcst_key, PTCThread.StFollow.NO_FOLLOW, false);
				Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true);
				if (Mg.Dro == null)
				{
					Mg.createDropper(Mg.dx, Mg.dy, 0.125f, -1f, -1f);
				}
				Mg.Dro.type |= (DROP_TYPE)384;
			}
			Mg.Dro.gravity_scale = _shot_gravity_scale;
			Mg.Dro.bounce_x_reduce = bounce_vx;
			Mg.Dro.bounce_y_reduce = bounce_vy;
			int num = (int)(Mg.t / 15f);
			float num2 = X.NI(0.02f, 0.4f, X.ZLINE(X.LENGTHXY(0f, 0f, Mg.dx, Mg.dy), 0.3f)) * 3.1415927f / 180f * (float)X.MPF(Mg.dx > 0f) * fcnt * rotate_spd_ratio;
			Mg.sa += num2;
			if (Mg.sz < (float)num)
			{
				Mg.sz = (float)num;
				if (NelNMush.PtcMagicGas == null)
				{
					NelNMush.PtcMagicGas = EfParticle.Get("mush_shot_living_gas", false);
				}
				Mg.Mp.PtcN(NelNMush.PtcMagicGas, Mg.sx, Mg.sy, 0f, 0, 0);
			}
			if (Mg.Dro.on_ground)
			{
				Mg.Dro.vx = X.VALWALK(Mg.Dro.vx, 0f, reduce_ax_in_ground * fcnt);
				Mg.da -= fcnt;
				Mg.Ray.check_mv_hit = false;
				if (Mg.da <= 0f)
				{
					Mg.PtcST("mush_shot_bomb_gas", PTCThread.StFollow.NO_FOLLOW, false);
					Mg.explode(false);
					if (Mg.Other is MistManager.MistKind)
					{
						MistManager.MistKind mistKind = Mg.Other as MistManager.MistKind;
						Mg.M2D.MIST.addMistGenerator(mistKind, mistKind.calcAmount(_mist_amount_frm, 1.4f), (int)Mg.sx, (int)(Mg.sy - 0.2f + (float)explode_center_shift_y), false);
					}
					else if (Mg.Other is EnemyAttr.FnDelegateSetSplash)
					{
						if (Mg.Caster is NelEnemy)
						{
							(Mg.Other as EnemyAttr.FnDelegateSetSplash)(Mg.Caster as NelEnemy, Mg.sx, Mg.sy, 0.25f, 0.4f);
						}
					}
					else if (Mg.Other is MagicItem.FnMagicRun)
					{
						(Mg.Other as MagicItem.FnMagicRun)(Mg, 1.5707964f);
					}
					return false;
				}
			}
			float num3 = (((Mg.Ray.hittype & HITTYPE.TEMP_OFFLINE) == HITTYPE.NONE) ? 0.015f : 0f);
			if (num3 > 0f)
			{
				Mg.calcAimPos(false);
			}
			Mg.MnSetRay(Mg.Ray, 0, Mg.aim_agR, 0f);
			Mg.Ray.LenM(num3);
			if (Mg.Atk0 != null)
			{
				if ((Mg.Ray.hittype & HITTYPE.REFLECTED) != HITTYPE.NONE)
				{
					Mg.reflectV(Mg.Ray, ref Mg.Dro.vx, ref Mg.Dro.vy, 0.24f, 0.25f, true);
					Mg.Ray.clearTempReflect();
					Mg.Dro.vy -= 0.125f;
				}
				HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(X.absmin(Mg.Dro.vx, 0.25f), 0f), HITTYPE.NONE);
				if ((hittype & (HITTYPE.KILLED | HITTYPE.REFLECT_BROKEN)) != HITTYPE.NONE || Mg.t >= 280f)
				{
					Mg.kill(0.125f);
					return false;
				}
				if ((hittype & HITTYPE.BREAK) != HITTYPE.NONE)
				{
					Mg.Dro.vx *= -0.7f;
				}
			}
			return true;
		}

		public bool MgDrawMushShot(MagicItem Mg, float fcnt)
		{
			EffectItem ef = Mg.Ef;
			int num = 4;
			MeshDrawer meshDrawer = null;
			if (Mg.da <= 50f && (int)Mg.da / 10 % 2 == 0)
			{
				meshDrawer = ef.GetMesh("", MTRX.getMtr(BLEND.ADD, -1), false);
				meshDrawer.Col = C32.d2c(2865219527U);
			}
			MeshDrawer mesh = ef.GetMesh("", MTRX.getMtr(BLEND.NORMAL, -1), false);
			mesh.ColGrd.Set(EnemyAttr.get_mcolor2(this, 4289335742U));
			MTRX.cola.Set(EnemyAttr.get_mcolor(this, 2007749066U));
			if (Mg.da <= 50f)
			{
				MTRX.cola.blend(2868903935U, 0.5f);
			}
			Color32 c = MTRX.cola.C;
			mesh.Rotate(Mg.sa, false);
			if (meshDrawer != null)
			{
				meshDrawer.setCurrentMatrix(mesh.getCurrentMatrix(), false);
			}
			for (int i = 0; i < num; i++)
			{
				uint ran = X.GETRAN2(this.index + 77 + i, this.index % 3 + 2);
				float num2 = X.COSI(Mg.t, 29f + X.RAN(ran, 2019) * 20f) * 4f;
				float num3 = X.SINI(Mg.t, 14f + X.RAN(ran, 2019) * 20f) * 4f;
				if (meshDrawer == null)
				{
					mesh.Col = c;
				}
				(meshDrawer ?? mesh).Poly(num2, num3, 13f, X.RAN(ran, 1052) * 6.2831855f, 5, 0f, false, 0f, 0f);
				mesh.Col = mesh.ColGrd.C;
				mesh.Poly(num2, num3, 10f, 0f, 9, 2.5f, false, 0f, 0f);
			}
			return true;
		}

		private bool shotThunderSplash(MagicItem Mg, float agR)
		{
			if (!this.is_alive || base.destructed)
			{
				return false;
			}
			float num = base.x;
			float num2 = base.y;
			if (Mg != null)
			{
				num = Mg.sx;
				num2 = Mg.sy;
			}
			int num3 = (base.isOverDrive() ? 2 : 2);
			for (int i = 0; i < num3; i++)
			{
				float num4 = agR + X.XORSPS() * 3.1415927f * 0.35f;
				MgNThunderBallShot.addThunderBallShot(base.nM2D, this, base.mg_hit, num, num2, num4);
			}
			return true;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			if (!base.isOverDrive() && Atk != null)
			{
				if (Atk.isPlayerShotgun())
				{
					this.guard_hitted = 100;
					base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
					this.SpSetPose("damage", 1, null, false);
				}
				else if (this.SpPoseIs("guard"))
				{
					base.addF((NelEnemy.FLAG)327680);
					if (Atk.Caster is M2MoverPr && Atk.Caster as M2MoverPr == base.AimPr && this.canAttackSmallPunch())
					{
						this.guard_hitted = ((this.guard_hitted < 0) ? ((X.XORSP() < 0.4f) ? 100 : this.guard_hitted) : (X.Mx(this.guard_hitted, 0) + 1));
					}
				}
			}
			return base.applyDamage(Atk, force);
		}

		public override AttackInfo applyDamageFromMap(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo _Atk, float efx, float efy, bool apply_execute = true)
		{
			NelAttackInfo nelAttackInfo = base.applyDamageFromMap(MDI, _Atk, efx, efy, false) as NelAttackInfo;
			if (nelAttackInfo == null)
			{
				return null;
			}
			if (MDI.kind == MAPDMG.SPIKE)
			{
				return null;
			}
			if (!apply_execute)
			{
				return nelAttackInfo;
			}
			nelAttackInfo.shuffleHpMpDmg(this, 1f, 1f, -1000, -1000);
			if (this.applyDamage(nelAttackInfo, false) <= 0)
			{
				return null;
			}
			return nelAttackInfo;
		}

		protected override void autoTargetRayHitted(M2Ray Ray)
		{
			base.autoTargetRayHitted(Ray);
			this.Nai.AddF(NAI.FLAG.ESCAPE, 50f);
		}

		public override bool hasSuperArmor(NelAttackInfo Atk)
		{
			return base.hasSuperArmor(Atk) || base.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return base.applyHpDamageRatio(Atk) * (base.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD) ? 0.2f : 1f);
		}

		public override int getMpDamageValue(NelAttackInfo Atk, int val)
		{
			if (base.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD))
			{
				return 0;
			}
			return base.getMpDamageValue(Atk, val);
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if ((Ray.hittype & HITTYPE.GUARD_IGNORE) != HITTYPE.NONE && this.SpPoseIs("guard"))
			{
				return RAYHIT.NONE;
			}
			return base.can_hit(Ray);
		}

		protected NelAttackInfo AtkSmallPunch = new NelAttackInfo
		{
			hpdmg0 = 4,
			split_mpdmg = 5,
			huttobi_ratio = -100f,
			knockback_len = 0.5f,
			Beto = BetoInfo.NormalS,
			parryable = true
		};

		protected NelAttackInfo AtkShotHit = new NelAttackInfo
		{
			hpdmg0 = 4,
			burst_vx = 0.03f,
			huttobi_ratio = -100f
		}.Torn(0.01f, 0.02f);

		public static MistManager.MistKind MkSleep = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.SLEEP, 85f)
				}
			},
			color0 = C32.d2c(4291805391U),
			color1 = C32.d2c(4289498284U),
			max_influence = 3,
			damage_cooltime = 30
		};

		public static MistManager.MistKind MkSleepW = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.SLEEP, 85f)
				}
			},
			color0 = C32.d2c(4291805391U),
			color1 = C32.d2c(4289498284U),
			max_influence = 5,
			damage_cooltime = 30
		};

		public static MistManager.MistKind MkConfuse = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.CONFUSE, 85f)
				}
			},
			color0 = C32.d2c(4290278565U),
			color1 = C32.d2c(4288366235U),
			max_influence = 3,
			damage_cooltime = 90
		};

		protected static MistManager.MistKind MkPalalyse = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.PARALYSIS, 85f)
				}
			},
			color0 = C32.d2c(4291351417U),
			color1 = C32.d2c(4287464816U),
			max_influence = 3,
			damage_cooltime = 100
		};

		protected static MistManager.MistKind MkFrozen = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.FROZEN, 85f)
				}
			},
			color0 = C32.d2c(4289197567U),
			color1 = C32.d2c(4286363094U),
			max_influence = 3,
			damage_cooltime = 150,
			apply_o2 = 50
		};

		protected static MistManager.MistKind MkOd = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.CONFUSE, 60f).Add(SER.SLEEP, 60f)
				}
			},
			color0 = C32.d2c(4289528105U),
			color1 = C32.d2c(4282190101U),
			max_influence = 4,
			damage_cooltime = 160
		};

		public static MistManager.MistKind MkAcme = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 80f),
					EpDmg = new EpAtk(45, "smoke")
					{
						other = 10,
						mouth = 3
					},
					no_cough_move = true,
					corrupt_gacha = true,
					attr = MGATTR.ACME,
					mpdmg0 = 8
				}
			},
			color0 = C32.d2c(4293869240U),
			color1 = C32.d2c(4288811102U),
			max_influence = 4,
			damage_cooltime = 45,
			apply_o2 = 50
		};

		public static MistManager.MistKind MkAcmeS = new MistManager.MistKind(NelNMush.MkAcme)
		{
			max_influence = 3,
			damage_cooltime = 55
		};

		private const float misogi_mist_amount_ratio = 1.5f;

		protected static MistManager.MistKind[] AMistKind = new MistManager.MistKind[]
		{
			NelNMush.MkSleep,
			NelNMush.MkConfuse,
			NelNMush.MkPalalyse,
			NelNMush.MkSleepW,
			NelNMush.MkFrozen,
			NelNMush.MkAcme
		};

		protected static int[] Amist_index_base = new int[] { 0, 1, 2, 3 };

		protected static int[] Amist_index_frozen = new int[] { 4 };

		protected static int[] Amist_index_acme = new int[] { 5 };

		protected static int[] Aratio_buf = new int[4];

		protected int[] Amist_index;

		protected NelAttackInfo AtkTackleOdMisogi = new NelAttackInfo
		{
			hpdmg0 = 5,
			mpdmg0 = 28,
			absorb_replace_prob_both = 1f,
			shield_break_ratio = -10f,
			parryable = true
		};

		protected NelAttackInfo AtkTackleBackStep = new NelAttackInfo
		{
			hpdmg0 = 2,
			absorb_replace_prob_both = 0f,
			Beto = BetoInfo.BigBite.Level(12f, false),
			parryable = true
		};

		protected NOD.MpConsume McsOdCharge = NOD.getMpConsume("mush_od_charge");

		protected NOD.TackleInfo TkOdMisogi = NOD.getTackle("mush_od_misogi");

		protected NOD.TackleInfo TkOdMisogiFall = NOD.getTackle("mush_od_misogi_fall");

		protected NOD.TackleInfo TkOdBackStep = NOD.getTackle("mush_od_backstep_foot");

		private NOD.TackleInfo TkSmallPunch = NOD.getTackle("mush_small_punch");

		private NOD.MpConsume McsShot = NOD.getMpConsume("mush_shot");

		private MagicItem.FnMagicRun FD_MgRunMushShot;

		private MagicItem.FnMagicRun FD_MgDrawMushShot;

		protected EnAttackInfo AtkOdAbsorb = new EnAttackInfo
		{
			mpdmg0 = 1,
			split_mpdmg = 2,
			attr = MGATTR.ABSORB_V,
			hit_ptcst_name = "",
			EpDmg = new EpAtk(9, "mush")
			{
				cli = 10,
				vagina = 50,
				urethra = 15,
				other = 5,
				multiple_orgasm = 0.2f
			},
			SerDmg = new FlagCounter<SER>(4).Add(SER.PARALYSIS, 3f).Add(SER.SEXERCISE, 20f),
			Beto = BetoInfo.Sperm.Pow(1, false)
		};

		protected NelAttackInfo AtkOdYdrg = new NelAttackInfo
		{
			hpdmg0 = 5,
			attr = MGATTR.POISON,
			huttobi_ratio = -100f,
			shield_break_ratio = 0f
		};

		private const float ydrg_apply_mist = 0.25f;

		private const float ydrg_apply_absorb_init = 0.4f;

		private const float ydrg_apply_absorb_playing = 0.05f;

		private const int ydrg_thresh_lvl1 = 5;

		private const int ydrg_thresh_lvl2 = 13;

		private int guard_hitted;

		private const int small_punch_apply_time = 18;

		private const int delay_normal_punch = 40;

		private const int shot_time = 57;

		private int shot_after_time = 127;

		private const float od_plant_val = 0.08f;

		private const float od_walk_spd = 0.035714287f;

		private const float mushshot_bomb_time_min = 70f;

		private const float mushshot_bomb_time_max = 100f;

		public const float odjump_init_delay = 30f;

		public const float odjump_min_high_y = 4f;

		public const float odjump_max_high_y = 6f;

		public const float odjump_min_xlen = 9f;

		public const float odjump_max_xlen = 13f;

		private const int od_mist_charge_time = 100;

		public const int odjump_misogi_flytime = 24;

		public const float odjump_misogi_vy = 0.6f;

		private const int mist_amount_frm = 140;

		private const int OD_MIST_CHARGE_MAX = 3;

		private List<PTCThread> APtcPowerHolder;

		private static EfParticle PtcMagicGas;

		private object FnMistMushShotBreak;

		private const int PRI_SHOT = 6;

		private const int PRI_GUARD = 100;

		private const int PRI_BACKSTEP = 1;

		private const int PRI_MOVE = 0;

		private const int PRI_OD = 160;

		public const float shot_gravity_scale = 0.23f;
	}
}
