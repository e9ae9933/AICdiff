using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class NelNSlime : NelEnemy
	{
		protected float tackle_burst
		{
			get
			{
				return X.absMx(base.vx, 0.14f) * 0.3f;
			}
		}

		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 7f;
			this.posename_damage_huttobi = "huttobi";
			NAI.FnNaiLogic fnNaiLogic = new NAI.FnNaiLogic(this.considerNormal);
			NOD.BasicData basicData;
			switch (this.id)
			{
			case ENEMYID.SLIME_TUTORIAL:
				this.id = ENEMYID.SLIME_TUTORIAL;
				basicData = NOD.getBasicData("SLIME_TUTORIAL");
				num = 0f;
				break;
			case ENEMYID.SLIME_0_FLW:
				this.id = ENEMYID.SLIME_0_FLW;
				basicData = NOD.getBasicData("SLIME_0_FLW");
				break;
			case ENEMYID.SLIME_TUTORIAL_GARAGE:
				this.id = ENEMYID.SLIME_TUTORIAL_GARAGE;
				basicData = NOD.getBasicData("SLIME_TUTORIAL_GARAGE");
				num = 0f;
				this.gazing_type = NAI.TYPE.WAIT;
				break;
			default:
				this.id = ENEMYID.SLIME_0;
				basicData = NOD.getBasicData("SLIME_0");
				break;
			}
			this.auto_rot_on_damage = true;
			base.appear(_Mp, basicData);
			this.Nai.moveable_upper = (this.Nai.moveable_lower = 15f);
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = this.jump_alloc - 2f;
			this.Nai.attackable_length_bottom = 0.5f;
			if (this.id == ENEMYID.SLIME_TUTORIAL)
			{
				this.Nai.fnSleepLogic = new NAI.FnNaiLogic(this.Nai.fnAwakeDoNothing);
			}
			else
			{
				this.Nai.fnSleepLogic = new NAI.FnNaiLogic(this.Nai.fnSleepSlide);
			}
			this.Nai.fnAwakeLogic = fnNaiLogic;
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.absorb_weight = 1;
			this.AtkTackleP.Prepare(this, true);
			this.AtkTackleP0.Prepare(this, true);
			this.AtkAbsorb.Prepare(this, true);
			this.AtkOdPunch.Prepare(this, true);
			this.AtkOdTsuppari.Prepare(this, true);
			this.AtkOdAbsorb.Prepare(this, true);
			this.AtkOdAbsorbFatal.Prepare(this, true);
		}

		public override void initOverDriveAppear()
		{
			base.initOverDriveAppear();
			this.auto_absorb_lock_mover_hitting = false;
			this.absorb_weight = 4;
		}

		public override void quitOverDrive()
		{
			base.quitOverDrive();
			this.auto_absorb_lock_mover_hitting = true;
			this.absorb_weight = 1;
		}

		public override bool readTicket(NaTicket Tk)
		{
			switch (Tk.type)
			{
			case NAI.TYPE.AWAKE:
				if (Tk.initProgress(this))
				{
					this.SpSetPose("awake", -1, null, false);
					this.jumpInit(0f, 0f, 30f / base.CLEN, false);
					Tk.quit();
					this.walk_time = (float)(this.walk_st = 0);
				}
				return !base.hasFoot();
			case NAI.TYPE.WALK:
			case NAI.TYPE.BACKSTEP:
			{
				bool flag = Tk.initProgress(this);
				if (flag)
				{
					this.walk_time = (float)(this.walk_st = 0);
					if (Tk.aim != 3 && Tk.type != NAI.TYPE.BACKSTEP)
					{
						Tk.aim = (int)CAim.get_aim(base.x * 1.5f, 0f, Tk.depx * 1.5f, (float)X.IntR(-this.Nai.target_lastfoot_bottom + base.mbottom), false);
					}
					Tk.check_nearplace_error = 0;
				}
				if (CAim._YD(Tk.aim, 1) > 0 && !base.isOverDrive() && Tk.type != NAI.TYPE.BACKSTEP && this.Nai.RANtk(4813) < this.climb_up_ratio)
				{
					Tk.Recreate(NAI.TYPE.PUNCH, -1, true, null);
					return true;
				}
				int num = base.walkThroughLift(flag, Tk, 20);
				if (num >= 0)
				{
					if (num == 0 && !base.hasFoot())
					{
						this.SpSetPose("walk", -1, null, false);
					}
					return num == 0;
				}
				return this.runSlimeWalk(flag);
			}
			case NAI.TYPE.PUNCH:
			case NAI.TYPE.PUNCH_0:
			case NAI.TYPE.PUNCH_WEED:
				if (base.isOverDrive())
				{
					Tk.type = NAI.TYPE.WALK;
					Tk.initProgress(this);
					Tk.aim = (int)this.aim;
					return true;
				}
				return this.runSlimeJump(Tk, (Tk.type == NAI.TYPE.PUNCH_0) ? this.AtkTackleP0 : this.AtkTackleP);
			case NAI.TYPE.PUNCH_1:
				return this.runSlimeOverDrivePunch(Tk);
			case NAI.TYPE.PUNCH_2:
				return this.runSlimeOverDriveTsuppari(Tk);
			case NAI.TYPE.APPEAL_0:
				return this.runOverDriveAppeal(Tk);
			case NAI.TYPE.WARP:
				if (this.id == ENEMYID.SLIME_TUTORIAL_GARAGE)
				{
					this.event_throw_ray = !this.event_throw_ray;
					if (this.event_throw_ray)
					{
						this.Phy.addLockMoverHitting(HITLOCK.EVENT, -1f);
					}
					else
					{
						this.Phy.remLockMoverHitting(HITLOCK.EVENT);
					}
				}
				break;
			case NAI.TYPE.GAZE:
				if (!base.isOverDrive())
				{
					if (this.gazing_type != NAI.TYPE.GAZE)
					{
						Tk.type = NAI.TYPE.WAIT;
						return true;
					}
					base.readTicket(Tk);
					if (base.hasFoot() && this.Nai.RANtk(883) < 0.8f)
					{
						this.runAppeal();
					}
					return Tk.t >= 60f;
				}
				break;
			}
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
			this.FootD.footable_bits = 8U;
			base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			if (Tk != null && Tk.type == NAI.TYPE.PUNCH_1)
			{
				base.remF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT);
			}
		}

		private bool runSlimeWalk(bool init_flag)
		{
			int num = (base.isOverDrive() ? 75 : (base.is_awaken ? ((int)X.NIL(28f, 40f, this.enlarge_level - 1f, 1f)) : 45));
			if (init_flag)
			{
				this.walk_time = this.Mp.floort + (float)num;
				this.SpSetPose("walk", -1, null, false);
			}
			if (this.walk_time <= this.Mp.floort || !base.hasFoot())
			{
				return false;
			}
			float num2 = this.walk_time - this.Mp.floort;
			this.setWalkXSpeed((float)CAim._XD(this.aim, 1) * (1f - X.ZCOS(num2, (float)num) - (1f - X.ZSIN(num2, (float)num * 0.33f))) * base.walkspd_default, true, false);
			return true;
		}

		private bool runSlimeJump(NaTicket Tk, NelAttackInfo Atk)
		{
			if (base.isOverDrive())
			{
				return false;
			}
			bool flag = Atk == this.AtkTackleP0;
			if (Tk.initProgress(this) || Tk.prog < PROG.PROG0)
			{
				Tk.prog = PROG.PROG0;
				this.SpSetPose((Atk == null || !flag) ? "jump_pre" : "attack", -1, null, false);
				this.walk_st = 0;
				if (Atk == null)
				{
					this.walk_time = this.jump_simple_time;
				}
				else if (flag)
				{
					base.PtcST("slime_attack_charge", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
					this.walk_time = (float)((int)X.NAIBUN_I(this.grab_attack_prepare_time_min, this.grab_attack_prepare_time_max, X.XORSP()));
				}
				else
				{
					this.walk_time = (float)((int)X.NAIBUN_I(this.tackle_attack_prepare_time_min, this.tackle_attack_prepare_time_max, X.XORSP()));
				}
				this.setAim((this.Nai.dep_dx > 0f) ? AIM.R : AIM.L, false);
				this.t = 0f;
			}
			if (Tk.prog == PROG.PROG0)
			{
				string text = ((Atk == null || !flag) ? "jump" : "attack_2");
				if (this.t >= this.walk_time - 10f && !this.Anm.poseIs(text, false))
				{
					this.SpSetPose(text, -1, null, false);
				}
				if (this.t >= this.walk_time)
				{
					this.t = 0f;
					NOD.TackleInfo tackleInfo = (flag ? this.TkAbsorb : ((Tk.type == NAI.TYPE.PUNCH_WEED) ? this.TkPunchWeed : this.TkNormal));
					Tk.prog = PROG.PROG1;
					this.walk_time = 0f;
					this.SpSetPose(text, -1, null, false);
					float num = ((Atk == null || !flag) ? this.jump_high_max : this.grab_jump_high_max) * X.NIL(1f, 0.5f, this.enlarge_level - 1f, 1f);
					float num2 = X.absmin(this.Nai.dep_dx, this.jump_max_x_distance);
					float num3 = 0f;
					float num4 = X.Mn(-2f, this.Nai.dep_dy);
					if (Atk == null)
					{
						num3 = num4;
						num = X.MMX(base.NIENL(2.2f, 0.15f), num, -num4 + 1.8f);
					}
					else
					{
						num *= X.NAIBUN_I(1f, 1.3f, X.XORSP());
						if (flag)
						{
							num2 = base.mpf_is_right * this.jump_alloc * X.NAIBUN_I(0.8f, 1.1f, X.XORSP());
						}
						else if (Tk.type == NAI.TYPE.PUNCH_WEED)
						{
							num2 = 1.7f * (float)CAim._XD(this.aim, 1);
							num3 = 0f;
							num = 0.5f;
						}
						else
						{
							num2 *= X.NAIBUN_I(0.9f, 1.3f, X.XORSP()) * X.NIL(1f, 0.6f, this.enlarge_level - 1f, 1f);
						}
						base.killPtc("slime_attack_charge", false);
						base.PtcST("slime_attack_start", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
					}
					if (Tk.type != NAI.TYPE.PUNCH_WEED && -num3 >= num)
					{
						num3 = -num;
					}
					float num5 = ((tackleInfo.Mcs != null && !base.Useable(tackleInfo, 1f)) ? 0.25f : 1f);
					base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					this.jumpInit(X.absmin(num2, this.jump_alloc) * num5, num3, num * num5, false);
					if (Atk != null)
					{
						base.tackleInit(Atk.Burst(this.tackle_burst, 0f), tackleInfo, MGHIT.AUTO);
					}
				}
			}
			else if (Tk.prog == PROG.PROG1)
			{
				if (this.Anm.poseIs("jump", false))
				{
					if (base.vy < -0.24f)
					{
						if (this.walk_time == 1f)
						{
							this.walk_time += 1f;
						}
						else if (CAim._YD(this.aim, 1) != 1)
						{
							this.setAim(base.is_right ? AIM.TR : AIM.LT, false);
						}
					}
					else
					{
						if (CAim._YD(this.aim, 1) != -1)
						{
							this.setAim(base.is_right ? AIM.RB : AIM.BL, false);
							this.Anm.animReset(0, false);
						}
						if (this.walk_time == 0f)
						{
							this.walk_time = 1f;
						}
					}
				}
				if (!base.hasFoot())
				{
					this.Nai.delay = X.Mx(this.Nai.delay, 20f);
				}
				if ((base.hasFoot() && this.t >= 20f) || this.walk_time >= 2f)
				{
					if (this.walk_time >= 2f)
					{
						this.Phy.remFoc(FOCTYPE.WALK | FOCTYPE.JUMP, true);
					}
					base.is_right = base.is_right;
					base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					this.SpSetPose("atk_land", -1, null, false);
					if (base.hasFoot())
					{
						EnemyAttr.Splash(this, 1.25f * this.nattr_splash_ratio);
					}
					this.t = 0f;
					Tk.prog = PROG.PROG2;
					this.can_hold_tackle = false;
				}
			}
			else if (Tk.prog == PROG.PROG2)
			{
				if (this.t >= (float)((Atk == null) ? 30 : 60))
				{
					return false;
				}
			}
			else if (Tk.prog == PROG.PROG3 && this.t >= 40f)
			{
				return false;
			}
			return true;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			Aattr.Add(MGATTR.NORMAL);
			Aattr.Add(MGATTR.ABSORB);
			if (base.isOverDrive())
			{
				Aattr.Add(MGATTR.EATEN);
				A.Add("torture_slime_0");
				A.Add("torture_slime_1");
			}
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (!base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			if (base.isOverDrive())
			{
				if (Abm.Con.current_pose_priority > 99)
				{
					return false;
				}
				Abm.target_pose = "torture_slime_0";
				Abm.kirimomi_release = true;
				Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 10, 63U);
				Abm.uipicture_fade_key = "torture_slime_0";
			}
			else
			{
				Abm.kirimomi_release = false;
				this.Phy.killSpeedForce(true, true, true, false, false);
				this.absorb_pos_fix_maxt = 11;
				Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 3, 63U);
				Abm.publish_float = true;
			}
			return true;
		}

		public override bool releaseAbsorb(AbsorbManager Absorb)
		{
			if (base.releaseAbsorb(Absorb) && !base.isOverDrive())
			{
				this.SpSetPose("awake", -1, null, false);
				this.jumpInit(-base.mpf_is_right * Absorb.getPublish0XD() * X.NIXP(0.6f, 1.1f), 0f, base.NIENL(2.2f, 1f), false);
				return true;
			}
			return false;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.canAbsorbContinue())
			{
				return false;
			}
			if (base.isOverDrive())
			{
				return this.runAbsorbOverDrive();
			}
			if (this.t <= 0f)
			{
				this.SpSetPose("absorb", -1, null, false);
				this.setAim(CAim.get_aim2(0f, 0f, -this.Absorb.getPublishXD(), (float)(-1 + X.xors(3)), false), false);
				this.Anm.showToFront(X.xors(2) == 0, false);
				this.walk_st = (4 + X.xors(4)) * 6 + 1;
				this.walk_time = (float)(30 + X.xors(12));
				this.Phy.addLockGravity(NelEnemy.STATE.ABSORB, 0f, -1f);
			}
			this.walk_time -= this.TS;
			if (this.walk_time <= 0f)
			{
				int walk_st = this.walk_st;
				this.walk_st = walk_st - 1;
				if (walk_st <= 0)
				{
					return false;
				}
				base.PtcST("absorb_atk_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				float num = X.NIXP(4f, 6f) * (float)X.MPFXP();
				float num2 = X.NIXP(20f, 25f);
				if (X.XORSP() < 0.7f)
				{
					pr.TeCon.setQuakeSinH(num, 44, num2, 0f, 0);
					this.TeCon.setQuakeSinH(num * 0.7f, 44, num2, 0f, 0);
				}
				if (this.walk_st % 2 == 0 || !pr.is_alive)
				{
					base.runAbsorb();
					base.applyAbsorbDamageTo(pr, this.AtkAbsorb, true, false, false, 0f, false, null, false, true);
				}
				this.Anm.randomizeFrame(0.5f, 0.5f);
				this.walk_time += (float)(18 + X.xors(9) + ((this.walk_st % 6 == 2) ? 44 : 0));
			}
			return true;
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai, this.gazing_type))
			{
				return true;
			}
			if (!base.isOverDrive())
			{
				if (Nai.RANtk(4988) < 0.6f && Nai.target_sxdif > 0.5f && base.y - Nai.target_y > 1.5f && !base.canGoToSide((base.x < Nai.target_x) ? AIM.R : AIM.L, this.sizex + 0.6f, -0.11f, false, false, false))
				{
					return Nai.AddTicketB(NAI.TYPE.PUNCH, 128, true);
				}
				int num = (((this.nattr & ENATTR.ACME) != ENATTR.NORMAL) ? 70 : 0);
				if (Nai.isPrGaraakiState())
				{
					if (Nai.fnBasicPunch(Nai, 130, (float)(30 + ((Nai.target_lastfoot_bottom < base.mbottom - 0.5f) ? 60 : 0)), (float)(40 + num), 0f, 0f, 8841, false))
					{
						return true;
					}
				}
				else
				{
					bool flag = true;
					if (Nai.AimPr is PR)
					{
						flag = (Nai.AimPr as PR).isAlreadyAbsorbed(this);
					}
					if (base.y - Nai.AimPr.y > 1.5f)
					{
						if (Nai.fnBasicPunch(Nai, 130 + (flag ? 20 : 0), (float)(20 - (flag ? 10 : 0)), (float)(num / 2), 0f, 0f, 8841, false))
						{
							return true;
						}
					}
					else
					{
						float num2 = 1f - X.ZLINE(this.enlarge_level - 1f);
						if (Nai.fnBasicPunch(Nai, 130, 60f - num2 * 70f, 2f + 40f * num2 + (float)num, 0f, 0f, 8841, false))
						{
							return true;
						}
					}
				}
			}
			return Nai.fnBasicMove(Nai);
		}

		private bool considerOverDrive(NAI Nai)
		{
			if (Nai.HasF(NAI.FLAG.FOOTED, true))
			{
				Nai.RemF(NAI.FLAG.JUMPED);
				Nai.clearTicket(0, true);
				Nai.delay = (float)(Nai.HasF(NAI.FLAG.OVERDRIVED, false) ? 20 : 40);
				return true;
			}
			if (Nai.HasF(NAI.FLAG.OVERDRIVED, true))
			{
				Nai.AddTicket(NAI.TYPE.APPEAL_0, 128, true);
				return true;
			}
			if (Nai.HasF(NAI.FLAG.GAZE_CANNOT_ACCESS, false) || (Nai.RANa(3146) < 0.8f && Nai.AimPr is PR && !Nai.AimPr.is_alive && (Nai.AimPr as PR).hasNearlyLayingEgg(PrEggManager.CATEG.SLIME)))
			{
				Nai.AddF(NAI.FLAG.GAZE, 60f);
				return false;
			}
			if (Nai.HasF(NAI.FLAG.ESCAPE, false) || (Nai.HasF(NAI.FLAG.GAZE, false) && Nai.RANa(3146) < 0.8f))
			{
				return false;
			}
			if (Nai.isCoveringToPr(3f, 1.2f))
			{
				if (X.XORSP() < 0.8f)
				{
					Nai.AddTicket(NAI.TYPE.PUNCH_1, 128, true);
				}
				else
				{
					Nai.AddTicket(NAI.TYPE.PUNCH_2, 128, true);
				}
				return true;
			}
			if (!Nai.hasPriorityTicket(0, false, false))
			{
				if (X.XORSP() < 0.08f)
				{
					Nai.AddTicket(NAI.TYPE.PUNCH_2, 128, true);
					return true;
				}
				NaTicket naTicket = Nai.AddMoveTicketToTarget(0f, 0f, 0, true, NAI.TYPE.WALK);
				if (naTicket != null && naTicket.depy < base.mbottom - 1.5f && X.XORSP() < (Nai.AimPr.is_alive ? 0.2f : 0.33f))
				{
					naTicket.Recreate(NAI.TYPE.PUNCH_2, 128, false, null);
					return true;
				}
			}
			return false;
		}

		public bool runOverDriveAppeal(NaTicket Tk)
		{
			return base.runOverDriveAppealBasic(Tk.initProgress(this), Tk, "od_attack", "od_growl2stand", 100, 130);
		}

		public bool runSlimeOverDrivePunch(NaTicket Tk)
		{
			if (!base.isOverDrive())
			{
				return false;
			}
			if (Tk.initProgress(this))
			{
				this.SpSetPose("od_eat_0", -1, null, false);
				base.PtcVar("sizex", (double)this.sizex).PtcST("slime_overdrive_eat_open", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.absorb_weight = 5;
				this.t = (this.walk_time = 0f);
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			}
			if (this.t >= this.overdrive_eat_pre_delay && this.walk_time == 0f)
			{
				this.SpSetPose("od_eat_1", -1, null, false);
				base.PtcVar("sizex", (double)this.sizex).PtcST("slime_overdrive_eat_attack", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.walk_time = 1f;
				this.t = 0f;
				Tk.prog = PROG.PROG1;
				base.tackleInit(this.AtkOdPunch, this.TkOdPunch, MGHIT.AUTO);
				this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, 60f);
			}
			else if (this.walk_time >= 1f)
			{
				if (this.t >= 10f && this.walk_time == 1f)
				{
					this.walk_time = 2f;
					base.PtcVar("sizex", (double)this.sizex).PtcST("slime_overdrive_eat_close", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					Tk.prog = PROG.PROG2;
					this.Phy.remLockMoverHitting(HITLOCK.SPECIAL_ATTACK);
					this.can_hold_tackle = false;
					EnemyAttr.Splash(this, 2f);
					base.M2D.Cam.Qu.SinV(11f, 40f, 0f, 0);
					base.M2D.Cam.Qu.Vib(3f, 40f, 1f, 10);
				}
				if (this.t <= 22f)
				{
					float num = 0.36363637f;
					float num2 = -num / 22f;
					this.walkBy(FOCTYPE.WALK, base.mpf_is_right * (num + num2 * this.t), 0f, false);
				}
				if (this.t >= this.overdrive_eat_post_delay)
				{
					return false;
				}
			}
			return true;
		}

		public override IFootable checkSkipLift(M2BlockColliderContainer.BCCLine _P)
		{
			if (this.Nai != null && this.Nai.isFrontType(NAI.TYPE.PUNCH_2, PROG.PROG1))
			{
				return null;
			}
			return base.checkSkipLift(_P);
		}

		public bool runSlimeOverDriveTsuppari(NaTicket Tk)
		{
			if (!base.isOverDrive())
			{
				return false;
			}
			if (Tk.initProgress(this))
			{
				this.SpSetPose("od_attack", -1, null, false);
				base.playSndPos("en_overdrive_voice", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW, null);
				this.t = 0f;
				this.walk_st = (int)X.NIXP(3f, 11f);
				this.walk_time = 0f;
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.t >= 50f)
				{
					M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
					if (footBCC != null && CAim._XD(this.aim, 1) > 0 != base.x < this.Nai.target_x && footBCC.foot_aim == AIM.B && !this.Mp.canThroughBcc(base.x + (float)CAim._XD(this.aim, 1) * (this.sizex - 0.1f), base.y, base.x + (float)CAim._XD(this.aim, 1) * (this.sizex + 0.2f), base.y, 0.1f, 0.1f, (int)this.aim, true, false, null, true, null))
					{
						this.aim = ((base.mpf_is_right > 0f) ? AIM.L : AIM.R);
					}
					this.FootD.initJump(false, true, false);
					Tk.prog = PROG.PROG1;
					Tk.t = 0f;
					this.walk_time += 1f;
					bool flag = (int)this.walk_time % 2 == 1;
					this.SpSetPose("od_attack_2", -1, null, false);
					this.setAim(CAim.get_aim(0f, 0f, (float)CAim._XD(this.aim, 1), (float)(flag ? 1 : 0), false), true);
					this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, 200f).addLockGravity(HITLOCK.SPECIAL_ATTACK, 0f, 200f);
					this.t = 0f;
					this.FootD.lockPlayFootStamp(4);
					base.tackleInit(this.AtkOdTsuppari, this.TkOdTsuppari, MGHIT.AUTO);
					this.Phy.addFocXy(FOCTYPE.SPECIAL_ATTACK, base.mpf_is_right * 0.1f, 0f, -1f, 0, 80, 140, 0, 0);
					this.Phy.addFocXy(FOCTYPE.SPECIAL_ATTACK, 0f, -0.46f * (float)X.MPF(flag), -1f, 0, 80, 140, -1, 0);
					this.FootD.footable_bits = (flag ? 2U : 8U);
				}
			}
			else if (Tk.prog == PROG.PROG1)
			{
				bool flag2 = (int)this.walk_time % 2 == 1;
				if (this.t >= 3f)
				{
					this.t = 0f;
					if (base.hasFoot() || Tk.t >= 70f)
					{
						this.Phy.remLockMoverHitting(HITLOCK.SPECIAL_ATTACK);
						this.Phy.remFoc(FOCTYPE.SPECIAL_ATTACK, true);
						this.t = 0f;
						Tk.t = 0f;
						this.SpSetPose("land", -1, null, false);
						base.M2D.Cam.Qu.SinH(4f, 20f, 0f, 0);
						base.M2D.Cam.Qu.SinV(9f, 30f, 0f, 0);
						this.can_hold_tackle = false;
						EnemyAttr.Splash(this, base.x, base.y, 2f, 1f, 1f);
						if (!flag2)
						{
							this.Phy.remLockGravity(HITLOCK.SPECIAL_ATTACK);
							Tk.prog = PROG.PROG5;
						}
						else if (this.walk_time >= (float)this.walk_st || (!base.Useable(this.TkOdTsuppari, 2f) && this.walk_time >= 2f))
						{
							Tk.prog = PROG.PROG5;
							this.walk_time = (float)this.walk_st;
						}
						else
						{
							Tk.prog = PROG.ACTIVE;
							this.t = 30f;
						}
					}
				}
			}
			else if (Tk.prog == PROG.PROG5)
			{
				if (this.walk_time >= (float)this.walk_st && this.t >= 30f)
				{
					this.FootD.initJump(false, true, false);
					this.FootD.footable_bits = 8U;
					this.Phy.remLockMoverHitting(HITLOCK.SPECIAL_ATTACK).remLockGravity(HITLOCK.SPECIAL_ATTACK);
					this.setAim((base.mpf_is_right > 0f) ? AIM.R : AIM.L, false);
					if (!base.hasFoot())
					{
						this.SpSetPose("fall", -1, null, false);
					}
					this.Nai.delay = 50f;
					return false;
				}
				if (base.hasFoot() && this.walk_time < (float)this.walk_st && this.t >= 20f)
				{
					this.t = 50f;
					Tk.prog = PROG.ACTIVE;
					Tk.t = 0f;
					return this.runSlimeOverDriveTsuppari(Tk);
				}
			}
			return true;
		}

		public bool runAbsorbOverDrive()
		{
			PR pr = base.AimPr as PR;
			if (this.t <= 0f)
			{
				this.Absorb.changeTorturePose("torture_slime_0", true, true, -1, -1);
				this.Anm.showToFront(true, false);
				this.walk_st = ((pr.get_hp() > 0f) ? 7 : 100);
				this.walk_time = (float)(70 + X.xors(12));
				pr.Ser.Add(SER.DO_NOT_LAY_EGG, 240, 99, false);
				this.Absorb.setKirimomiReleaseDir((int)this.aim);
			}
			if (this.walk_st < 500 && pr.Ser.has(SER.LAYING_EGG) && this.walk_st >= 100 && this.t >= X.NI(200, 830, X.ZPOW(this.Nai.RANtk(1672))) * (float)((this.Nai.RANtk(8861) < 0.15f) ? 8 : 1))
			{
				this.walk_st = 500;
				this.walk_time = 90f;
			}
			if (this.walk_st < 100)
			{
				this.walk_time -= this.TS;
				if (this.walk_time <= 0f)
				{
					int walk_st = this.walk_st;
					this.walk_st = walk_st - 1;
					float num = X.NIXP(4f, 6f) * (float)X.MPFXP();
					float num2 = X.NIXP(20f, 25f);
					if (X.XORSP() < 0.5f)
					{
						this.TeCon.setQuakeSinH(num, 44, num2, 0f, 0);
					}
					pr.defineParticlePreVariable();
					pr.PtcVar("rot_agR", (double)(X.NIXP(-0.09f, 0.09f) * 3.1415927f)).PtcST("absorb_dmg_bite", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					bool flag = this.walk_st >= 0 && (this.walk_st % 3 == 0 || !pr.is_alive);
					base.applyAbsorbDamageTo(pr, this.AtkOdAbsorb, flag, true, false, 0f, false, null, false, true);
					this.Mp.DropCon.setBlood(pr, flag ? 7 : 23, MTR.col_blood, 0f, true);
					this.Anm.randomizeFrame(0.5f, 0.5f);
					pr.Ser.Add(SER.DO_NOT_LAY_EGG, 240, 99, false);
					this.walk_time += (float)(21 + X.xors(7));
					if ((pr.get_hp() <= 0f && this.walk_st <= 0) || this.walk_st <= -15)
					{
						this.walk_time = 20f;
						this.walk_st = 100;
						this.t = 1f;
					}
				}
			}
			else if (this.walk_st < 500)
			{
				if (this.walk_st == 100)
				{
					this.walk_time -= this.TS;
					if (this.walk_time <= 0f)
					{
						this.walk_st = 101;
						this.Absorb.changeTorturePose("torture_slime_1", true, false, -1, -1);
						this.walk_time += 44f;
						pr.Ser.Add(SER.DO_NOT_LAY_EGG, 240, 99, false);
						this.Absorb.uipicture_fade_key = "torture_slime_1";
					}
				}
				else if (this.walk_st == 101)
				{
					this.walk_time -= this.TS;
					if (this.walk_time <= 0f)
					{
						this.walk_st = 102;
						this.Absorb.penetrate_decline = true;
						this.Absorb.normal_UP_fade_injectable = 0.03f;
						base.playSndPos("slime_od_eaten", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW, null);
						pr.defineParticlePreVariable();
						pr.PtcST("absorb_atk_in_stomack", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						pr.Ser.Add(SER.EATEN, -1, 99, false);
						this.walk_time += (float)((pr.get_hp() <= 0f) ? (120 + X.xors(40)) : (30 + X.xors(30)));
						pr.Ser.Add(SER.DO_NOT_LAY_EGG, 240, 99, false);
					}
				}
				else
				{
					float num3 = 0f;
					this.walk_time -= this.TS;
					if (this.walk_time <= 0f)
					{
						bool flag2 = pr.is_alive && (DIFF.I == 0 || (DIFF.I == 1 && X.XORSP() < 0.5f));
						this.Absorb.setKirimomiReleaseDir((int)this.aim);
						if (X.XORSP() < 0.4f)
						{
							pr.defineParticlePreVariable();
							pr.PtcST("absorb_atk_in_stomack", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						}
						if (X.XORSP() < 0.4f)
						{
							pr.defineParticlePreVariable();
							pr.PtcST("player_absorbed_fatal", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							this.Mp.DropCon.setBlood(pr, 22, (pr.get_mp() > 0f) ? MTR.col_pr_mana : MTR.col_pr_no_mana, 0f, false);
							if (!X.SENSITIVE && !flag2 && this.Anm.poseIs("torture_slime_1", false))
							{
								this.Absorb.changeTorturePose("torture_slime_2", true, false, -1, -1);
								this.Anm.showToFront(false, false);
							}
							this.Anm.randomizeFrame(0.5f, 0.5f);
						}
						base.applyAbsorbDamageTo(pr, (!flag2) ? this.AtkOdAbsorbFatal : this.AtkAbsorb, true, true, false, 0f, false, null, false, true);
						pr.Ser.Add(SER.DO_NOT_LAY_EGG, 240, 99, false);
						if (X.XORSP() < 0.13f || pr.get_mp() > 0f)
						{
							this.walk_time += (float)(15 + X.xors(20));
						}
						else
						{
							this.walk_time += (float)(25 + X.xors(20));
							if (X.XORSP() < 0.16f && !flag2)
							{
								num3 = 0.15f;
							}
						}
						if (flag2 && this.walk_st >= 102 && this.walk_st < 105)
						{
							this.walk_st++;
						}
						if (this.walk_st == 105)
						{
							this.walk_st = 106;
							num3 = 8f;
							this.walk_time = 120f;
						}
						else if (this.walk_st == 106)
						{
							return false;
						}
						if (num3 > 0f)
						{
							PrEggManager.CATEG categ = PrEggManager.CATEG.SLIME;
							if (pr.EggCon.applyEggPlantDamage(this.od_plant_val, categ, true, num3) > 0)
							{
								if (this.walk_st >= 105 && !X.SENSITIVE && this.Anm.poseIs("torture_slime_1", false))
								{
									this.Absorb.changeTorturePose("torture_slime_2", true, false, -1, -1);
									this.Anm.showToFront(false, false);
								}
							}
							else if (this.walk_st >= 105)
							{
								return false;
							}
						}
					}
				}
			}
			else
			{
				this.walk_time -= this.TS;
				if (this.walk_time <= 0f)
				{
					this.Nai.AddF(NAI.FLAG.GAZE, 180f);
					return false;
				}
			}
			return true;
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if ((Ray.hittype & HITTYPE.AUTO_TARGET) == HITTYPE.NONE && this.event_throw_ray)
			{
				return RAYHIT.NONE;
			}
			return base.can_hit(Ray);
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			this.Anm.rotationR_speed = 0f;
			this.Anm.rotationR = 0f;
			if (this.state == NelEnemy.STATE.ABSORB)
			{
				this.Nai.delay = (float)(base.isOverDrive() ? 120 : 60);
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
			}
			base.changeState(st);
			return this;
		}

		public override bool runDamageHuttobi()
		{
			if (this.t <= 0f && this.is_alive)
			{
				this.playSndPos("en_damaged", 1);
			}
			bool flag = base.runDamageHuttobi();
			if (flag)
			{
				if (this.walk_st >= 2 && !this.Anm.poseIs("huttobi_falling", false))
				{
					this.SpSetPose("huttobi_falling", -1, null, false);
					this.Anm.rotationR_speed = (float)X.MPF(base.vx < 0f) * X.NIXP(0.055f, 0.1f) * 6.2831855f;
				}
				return flag;
			}
			if (base.hasFoot() && X.LENGTHXYS(0f, 0f, base.vx, base.vy) < 0.012f)
			{
				this.runAppeal();
				return false;
			}
			return true;
		}

		public override void runAppeal()
		{
			base.runAppeal();
			if (base.hasFoot() && !base.isOverDrive())
			{
				this.jumpInit(0f, 0f, 30f / base.CLEN, false);
			}
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle && !this.Ser.has(SER.TIRED);
		}

		protected float jump_alloc = 4f;

		protected float jump_max_x_distance = 5f;

		protected float jump_simple_time = 34f;

		protected float tackle_attack_prepare_time_min = 30f;

		protected float tackle_attack_prepare_time_max = 40f;

		protected float jump_high_max = 4f;

		protected float grab_attack_prepare_time_min = 80f;

		protected float grab_attack_prepare_time_max = 120f;

		protected float grab_jump_high_max = 1.7f;

		protected int jump_duration = 19;

		protected EnAttackInfo AtkTackleP = new EnAttackInfo(0.012f, 0.05f)
		{
			hpdmg0 = 7,
			split_mpdmg = 13,
			absorb_replace_prob = 0f,
			absorb_replace_prob_ondamage = 0f,
			huttobi_ratio = 0.12f,
			knockback_len = 0.7f,
			parryable = true,
			Beto = BetoInfo.Normal
		};

		protected NOD.TackleInfo TkNormal = NOD.getTackle("slime_normal");

		protected NOD.TackleInfo TkPunchWeed = NOD.getTackle("slime_punch_weed");

		protected NOD.TackleInfo TkAbsorb = NOD.getTackle("slime_absorb");

		protected EnAttackInfo AtkTackleP0 = new EnAttackInfo
		{
			hpdmg0 = 7,
			is_grab_attack = true,
			knockback_len = 0.2f,
			parryable = true
		};

		protected EnAttackInfo AtkAbsorb = new EnAttackInfo
		{
			split_mpdmg = 2,
			attr = MGATTR.ABSORB,
			hit_ptcst_name = "player_absorbed_basic",
			EpDmg = new EpAtk(15, "slime")
			{
				vagina = 4,
				mouth = 2,
				cli = 4
			},
			Beto = BetoInfo.Absorbed
		};

		protected NOD.TackleInfo TkOdPunch = NOD.getTackle("slime_od_punch");

		protected EnAttackInfo AtkOdPunch = new EnAttackInfo(0.15f, 0.25f)
		{
			hpdmg0 = 24,
			attr = MGATTR.BITE,
			split_mpdmg = 53,
			huttobi_ratio = 2f,
			shield_break_ratio = 9999f,
			burst_vx = 0.4f,
			burst_vy = -0.1f,
			is_penetrate_grab_attack = true,
			parryable = true
		};

		protected NOD.TackleInfo TkOdTsuppari = NOD.getTackle("slime_od_tsuppari");

		protected EnAttackInfo AtkOdTsuppari = new EnAttackInfo(0.03f, 0.5f)
		{
			hpdmg0 = 16,
			split_mpdmg = 4,
			huttobi_ratio = 2f,
			shield_break_ratio = 2f,
			burst_vx = 0.29f,
			burst_vy = -0.1f,
			Beto = BetoInfo.Normal,
			parryable = true
		};

		protected EnAttackInfo AtkOdAbsorb = new EnAttackInfo(0.06f, -1000f)
		{
			hpdmg0 = 11,
			split_mpdmg = 9,
			attr = MGATTR.BITE,
			Beto = BetoInfo.BigBite
		};

		protected EnAttackInfo AtkOdAbsorbFatal = new EnAttackInfo(0.03f, -1000f)
		{
			hpdmg0 = 8,
			mpdmg0 = 1,
			split_mpdmg = 15,
			attr = MGATTR.EATEN,
			hit_ptcst_name = "",
			EpDmg = new EpAtk(40, "slime")
			{
				vagina = 4,
				mouth = 3,
				anal = 2,
				ear = 1,
				cli = 2,
				canal = 8,
				gspot = 4,
				other = 5
			},
			Beto = BetoInfo.Normal.Pow(6, false)
		};

		protected float od_plant_val = 0.33f;

		protected float overdrive_eat_pre_delay = 50f;

		protected float overdrive_eat_post_delay = 90f;

		public float climb_up_ratio = 0.25f;

		private bool event_throw_ray;

		private NAI.TYPE gazing_type = NAI.TYPE.GAZE;
	}
}
