using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNRoaper : NelEnemy, ITortureListener
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			ENEMYID id = this.id;
			this.id = ENEMYID.ROAPER_0;
			NOD.BasicData basicData = NOD.getBasicData("ROAPER_0");
			base.appear(_Mp, basicData);
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 1.8f;
			this.Nai.attackable_length_top = -2.3f;
			this.Nai.attackable_length_bottom = 2.3f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.suit_distance = 4f;
			this.walkspd_awake = 0.06f;
			this.walkspd_sleep = 0.04f;
			this.walkspd_od = 0.025f;
			this.absorb_weight = 2;
			this.NasClm = new NASGroundClimber(this, 0f)
			{
				auto_reset_animR = true
			};
			this.NasClm.addChangedFn(new NASGroundClimber.FnClimbEvent(this.fnChangedBcc));
			this.NasClm.alloc_jump_air = false;
			this.AtkAbsorbFall.Prepare(this, false);
			this.AtkAbsorbSmall.Prepare(this, true);
			this.AtkPunch.Prepare(this, true);
			this.AtkRot.Prepare(this, true);
		}

		public override void destruct()
		{
			if (this.AnmB != null)
			{
				this.AnmB.destruct();
				this.AnmB = null;
			}
			base.destruct();
		}

		public override void runPost()
		{
			if (this.state == NelEnemy.STATE.STAND || this.state == NelEnemy.STATE.ABSORB || this.state == NelEnemy.STATE.STUN)
			{
				this.stunable_in_checkstun = false;
			}
			base.runPost();
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			this.torture_absorb_reverse = false;
			this.t = -1f;
			NelEnemy.STATE state = this.state;
			if (state == NelEnemy.STATE.ABSORB)
			{
				this.Nai.addTypeLock(NAI.TYPE.PUNCH, 160f);
				this.Nai.addTypeLock(NAI.TYPE.PUNCH_1, (float)((this.walk_st >= 1000) ? 240 : 50));
			}
			base.changeState(st);
			if (state == NelEnemy.STATE.ABSORB)
			{
				this.Nai.delay += 16f;
				if (this.Anm.poseIs("torture_roaper_stabbing", true))
				{
					this.SpSetPose("attack_stab2", -1, null, false);
				}
				this.Phy.remLockWallHitting(this);
				this.Anm.showToFront(false, false);
				this.Anm.fnFineFrame = null;
				this.Nai.AddF(NAI.FLAG.WANDERING, 100f);
				if (this.AnmB != null)
				{
					this.AnmB.alpha = 0f;
				}
			}
			if (this.state == NelEnemy.STATE.STUN)
			{
				base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
				this.SpSetPose(this.posename_damage = "damage_critical", -1, null, false);
			}
			if (this.state == NelEnemy.STATE.ABSORB || this.state == NelEnemy.STATE.STAND)
			{
				base.remF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
				this.t_lock_dive = -1f;
			}
			if (state == NelEnemy.STATE.DAMAGE || state == NelEnemy.STATE.DAMAGE_HUTTOBI)
			{
				this.Nai.AddF(NAI.FLAG.BOTHERED, 100f);
			}
			return this;
		}

		public override void fineEnlargeScale(float r = -1f, bool set_effect = false, bool resize_moveby = false)
		{
			base.fineEnlargeScale(r, set_effect, resize_moveby);
			if (this.AnmB != null)
			{
				this.AnmB.fineAnimatorOffset(-1f);
			}
		}

		private bool considerNormal(NAI Nai)
		{
			if (this.considerNormalInner(Nai))
			{
				base.remF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
				return true;
			}
			return false;
		}

		private bool considerNormalInner(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai, NAI.TYPE.GAZE))
			{
				return true;
			}
			if (!Nai.hasPriorityTicket(138, false, false))
			{
				if (Nai.HasF(NAI.FLAG.WANDERING, true))
				{
					float num = (float)(-(float)X.MPF(base.x < Nai.target_x));
					NaTicket naTicket = null;
					if (this.Useable(this.McsDive, 1.5f, 0f))
					{
						if (Nai.RANtk(379) < 0.68f)
						{
							naTicket = Nai.AddTicket(NAI.TYPE.BACKSTEP, 138, true).Dep(base.x + num * Nai.NIRANtk(2.4f, 4.7f, 3178), base.mbottom, null);
						}
						else
						{
							naTicket = Nai.AddTicket(NAI.TYPE.BACKSTEP, 138, true).Dep(Nai.target_x - num * Nai.NIRANtk(0.4f, 2f, 3113), base.mbottom, null);
						}
					}
					if (naTicket == null)
					{
						naTicket = Nai.AddTicket(NAI.TYPE.WALK, 138, true).Dep(base.x + num * 3f, base.mbottom, null);
					}
					return true;
				}
				if (Nai.HasF(NAI.FLAG.BOTHERED, false))
				{
					if (this.Useable(this.McsDive, 1.5f, 0f) && Nai.RANtk(4381) < 0.5f && Nai.isAttackableLength(5f, -4f, 4f, true))
					{
						Nai.suit_distance = 2f;
						NaTicket naTicket = Nai.AddMoveTicketFor(Nai.target_x, Nai.target_lastfoot_bottom, null, 138, true, NAI.TYPE.BACKSTEP);
						if (naTicket != null)
						{
							return true;
						}
					}
					Nai.RemF(NAI.FLAG.BOTHERED);
				}
				if (Nai.isAttackableLength(true) && this.Useable(this.McsAtk, 1.25f, 0f) && Nai.fnBasicPunch(Nai, 138, 35f, 26f, 0f, 0f, 4156, true))
				{
					return true;
				}
				if (this.Useable(this.McsDive, 2.2f, 0f) && Nai.RANtk(2275) < X.NI(0.4f, 0.75f, base.mp_ratio) && ((Nai.autotargetted_me && Nai.isPrMagicChantingOrPreparingOrExploded(1f)) || (Nai.isAttackableLength(4f, -5f, 5f, false) && Nai.isPrAttacking(0.2f))))
				{
					float num2 = (float)(-(float)X.MPF(base.x < Nai.target_x));
					if (Nai.RANtk(379) < 0.68f)
					{
						NaTicket naTicket = Nai.AddTicket(NAI.TYPE.GUARD, 138, true).Dep(Nai.target_x - num2 * Nai.NIRANtk(0.4f, 2f, 2153), base.mbottom, null);
					}
					else
					{
						NaTicket naTicket = Nai.AddTicket(NAI.TYPE.BACKSTEP, 138, true).Dep(base.x + num2 * Nai.NIRANtk(2.4f, 4.7f, 3178), base.mbottom, null);
					}
					return true;
				}
			}
			if (!Nai.hasPriorityTicket(5, false, false))
			{
				NaTicket naTicket;
				if (Nai.RANtk(431) < 0.4f)
				{
					M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
					M2BlockColliderContainer.BCCLine targetLastBcc = Nai.TargetLastBcc;
					if (footBCC != null && footBCC.isLinearWalkableTo(targetLastBcc, 1) == 0)
					{
						if (this.Useable(this.McsDive, 1.5f, 0f))
						{
							Nai.suit_distance = 0f;
							naTicket = Nai.AddMoveTicketFor(Nai.target_x, Nai.target_lastfoot_bottom, null, 138, true, NAI.TYPE.GUARD);
						}
						else
						{
							naTicket = Nai.AddMoveTicketFor(Nai.target_x + Nai.NIRANtk(-8f, 8f, 3178), Nai.target_lastfoot_bottom, null, 5, true, NAI.TYPE.WALK);
						}
						if (naTicket != null)
						{
							return true;
						}
					}
				}
				if (base.mp_ratio < 0.33f)
				{
					naTicket = Nai.AddTicketSearchAndGetManaWeed(138, 8f, -0.5f, 4f, 1.5f, -0.5f, 0.5f, true);
					if (naTicket != null)
					{
						return true;
					}
				}
				naTicket = Nai.AddMoveTicketFor(Nai.target_x + Nai.NIRANtk(-2.2f, 2.2f, 4334), Nai.target_lastfoot_bottom, null, 5, true, NAI.TYPE.WALK);
				if (naTicket != null)
				{
					return true;
				}
			}
			return Nai.fnBasicMove(Nai);
		}

		private bool considerOverDrive(NAI Nai)
		{
			return true;
		}

		protected override bool setLandPose()
		{
			if (this.exist_land_pose && !base.hasF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET))
			{
				this.SpSetPose("land", -1, null, false);
				this.Nai.delay += 35f;
				return true;
			}
			return false;
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.GUARD)
			{
				switch (type)
				{
				case NAI.TYPE.WALK:
				{
					bool flag = Tk.initProgress(this);
					int num = base.walkThroughLift(flag, Tk, 20);
					if (num >= 0)
					{
						return num == 0;
					}
					return this.runWalk(flag, Tk);
				}
				case NAI.TYPE.WALK_TO_WEED:
				case NAI.TYPE.PUNCH_2:
					goto IL_00DF;
				case NAI.TYPE.PUNCH:
					return this.runPunchAbsorb(Tk.initProgress(this), Tk);
				case NAI.TYPE.PUNCH_0:
				case NAI.TYPE.PUNCH_WEED:
					return this.runPunchRotate(Tk.initProgress(this), Tk);
				case NAI.TYPE.PUNCH_1:
					return this.runPunchGrabMaden(Tk.initProgress(this), Tk);
				default:
					if (type != NAI.TYPE.GUARD)
					{
						goto IL_00DF;
					}
					break;
				}
			}
			else if (type != NAI.TYPE.BACKSTEP)
			{
				if (type == NAI.TYPE.WAIT)
				{
					base.AimToLr((X.xors(2) == 0) ? 0 : 2);
					Tk.after_delay = 30f + this.Nai.RANtk(840) * 40f;
					return false;
				}
				goto IL_00DF;
			}
			return this.runCrawlGround(Tk.initProgress(this), Tk);
			IL_00DF:
			return base.readTicket(Tk);
		}

		private bool runWalk(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = X.IntR(X.NIL(56f, 72f, this.enlarge_level - 1f, 1f));
				this.walk_time = this.Mp.floort + (float)this.walk_st;
				this.SpSetPose("walk", -1, null, false);
			}
			if (this.walk_time <= this.Mp.floort || !base.hasFoot())
			{
				Tk.AfterDelay(X.NIXP(4f, 15f));
				return false;
			}
			float num = this.walk_time - this.Mp.floort;
			this.setWalkXSpeed((float)CAim._XD(this.aim, 1) * (1f - X.ZCOS(num, (float)this.walk_st) - (1f - X.ZSIN(num, (float)this.walk_st * 0.33f))) * base.walkspd_default, true, false);
			return true;
		}

		private bool runCrawlGround(bool init_flag, NaTicket Tk)
		{
			if (this.Nai.HasF(NAI.FLAG.BOTHERED, false))
			{
				this.Nai.AddF(NAI.FLAG.BOTHERED, 2f + this.TS);
			}
			if (init_flag)
			{
				this.t = 0f;
				this.fall_attack_only = X.XORSP() < 0.25f && this.Useable(this.McsMaden, 1.15f, 0f);
				base.PtcST("roaper_dive0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				base.addF((NelEnemy.FLAG)4194496);
				this.walk_st = 0;
				this.walk_time = 0f;
				base.Size(this.sizex * base.CLENM, 0.14f * base.CLENM, ALIGN.CENTER, ALIGNY.BOTTOM, false);
				this.SpSetPose("dive", -1, null, false);
				this.fineFootType();
				base.throw_ray = true;
				this.t_lock_dive = -1f;
				if (this.FD_BombCheck == null)
				{
					this.FD_BombCheck = new MagicItem.FnCheckMagicFrom(this.fnCheckBomb);
				}
			}
			bool flag = Tk.type == NAI.TYPE.GUARD || this.fall_attack_only;
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 50, true))
			{
				this.setAim((base.x < Tk.depx == this.Nai.RANtk(418) < 0.8f) ? AIM.R : AIM.L, false);
				this.Phy.addLockWallHitting(this, -1f);
				this.walk_st = 0;
				this.walk_time = -1f;
				base.throw_ray = false;
				this.NasClm.footable_bits = 15U;
				this.NasClm.initClimbWalk(15U);
				this.sink_ratio = 3f;
				this.t_lock_dive = 0f;
			}
			if (Tk.prog == PROG.PROG0)
			{
				float num = X.NI(1f, 0.6f, this.enlarge_level - 1f);
				if (this.walk_time < 0f && base.AimPr == null)
				{
					this.walk_time = 0f;
				}
				M2BlockColliderContainer.BCCLine preBcc = this.NasClm.getPreBcc();
				if (preBcc == null || !this.NasClm.progressClimbWalk(0.16f * num * X.ZLINE(this.t, 20f)) || this.NasClm.getPreBcc() == null)
				{
					this.walk_time = X.Mx(this.walk_time, 0f);
				}
				else if (!this.fall_attack_only && ((this.t >= 180f && CAim._YD(preBcc.foot_aim, 1) != 0) || (base.hasFoot() && base.isCovering(Tk.depx, Tk.depx, Tk.depy - 0.5f, Tk.depy + 0.7f, 0f))))
				{
					this.walk_time = X.Mx(this.walk_time, 0f);
					this.NasClm.footable_bits = 1U << (int)preBcc.foot_aim;
				}
				if (preBcc != null)
				{
					base.nM2D.MGC.FindMg(this.FD_BombCheck, this);
					if (preBcc.foot_aim == AIM.T && flag && X.Abs(base.x - this.Nai.target_x) < this.Nai.NIRANtk(0.4f, 1.2f, 4311) + this.sizex && base.y < this.Nai.target_y)
					{
						this.walk_st = 1;
						this.walk_time = X.Mx(this.walk_time, 0f);
					}
				}
				if (this.t_lock_dive > 0f)
				{
					this.NasClm.footable_bits = 15U;
					this.t_lock_dive = X.VALWALK(this.t_lock_dive, 0f, this.TS);
				}
				if (this.walk_time >= 0f)
				{
					this.walk_time += this.TS;
					if (preBcc == null || preBcc.foot_aim == AIM.B || (this.walk_st == 1 && preBcc.foot_aim == AIM.T))
					{
						this.walk_time = 900f;
					}
					if (this.walk_time >= 80f && this.t >= 40f && (this.t_lock_dive <= 0f || this.t >= 340f))
					{
						this.t_lock_dive = -1f;
						float num2 = 1.5707964f;
						if (preBcc != null)
						{
							num2 = preBcc.housenagR;
						}
						this.Phy.remLockWallHitting(this);
						this.MpConsume(this.McsDive, null, 1f, 1f);
						this.walk_time = 0f;
						this.sink_ratio = 1f;
						this.NasClm.quitClimbWalk();
						Tk.Progress(ref this.t, 0, true);
						this.t = 0f;
						base.killPtc("roaper_dive0", false);
						base.throw_ray = true;
						this.fineEnlargeScale(-1f, false, true);
						if (this.walk_st == 1 && preBcc != null && preBcc.foot_aim == AIM.T)
						{
							base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
							this.walk_st = 100;
							this.SpSetPose("dive_fall", -1, null, false);
							base.PtcVar("cylR", (double)num2).PtcST("roaper_dive_fall", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
							this.Phy.initSoftFall(0.55f, 0f);
							this.Phy.initSoftFall(0.08f, 40f);
						}
						else
						{
							this.walk_st = 0;
							this.SpSetPose("dive_appear", -1, null, false);
							base.PtcVar("cylR", (double)num2).PtcST("roaper_dive_appear", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
							this.fineFootType();
						}
					}
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.t >= 20f)
				{
					base.throw_ray = false;
				}
				if (this.walk_st >= 100)
				{
					if (Tk.Progress(ref this.t, 0, (this.t >= 65f && base.hasFoot()) || (this.walk_st == 101 && !this.can_hold_tackle)))
					{
						this.can_hold_tackle = false;
						base.remF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
						EnemyAttr.Splash(this, 1.25f);
						base.killPtc("roaper_dive_fall", true);
						this.Phy.quitSoftFall(40f);
						base.remF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
						this.SpSetPose("dive_land", -1, null, false);
						this.fineFootType();
						Tk.AfterDelay(X.NIXP(70f, 98f));
						return false;
					}
					if (this.t >= 50f && this.walk_st == 100)
					{
						this.walk_st = 101;
						base.tackleInit(this.AtkAbsorbFall, this.TkFall, MGHIT.AUTO);
					}
					if (this.t >= 70f && this.walk_time == 0f)
					{
						this.walk_time += 1f;
						this.Phy.quitSoftFall(0f);
					}
				}
				else
				{
					if ((this.walk_st & 1) == 0 && this.t >= 30f && base.AimPr != null && this.Nai.isAttackableLength(1.4f, -2f, 3f, true))
					{
						this.walk_st |= 1;
						if (!this.Nai.hasTypeLock(NAI.TYPE.PUNCH) && this.Useable(this.McsAtk, 1.5f, 0f) && this.Nai.RANtk(3382) < X.NI(0.33f, 0.65f, this.enlarge_level - 1f) * (this.Nai.HasF(NAI.FLAG.BOTHERED, false) ? 2.2f : 1f))
						{
							Tk.Recreate(NAI.TYPE.PUNCH, 138, false, this.Nai).Dep(this.Nai.target_x, this.Nai.target_y, null);
							this.runPunchAbsorb(Tk.initProgress(this), Tk);
							base.AimToPlayer();
							this.walk_st = 1;
							return true;
						}
					}
					if ((this.walk_st & 2) == 0 && this.t >= 15f && base.AimPr != null && this.Nai.isAttackableLength(0.8f, -1f, 1f, true))
					{
						this.walk_st |= 2;
						if (!this.Nai.hasTypeLock(NAI.TYPE.PUNCH_1) && this.Useable(this.McsMaden, 1.15f, 0f) && (!this.Nai.isPrAlive() || this.Nai.RANtk(1865) < X.NI(0.16f, 0.8f, this.enlarge_level - 1f) * (this.Nai.HasF(NAI.FLAG.BOTHERED, false) ? 1.5f : 1f) * (float)(this.fall_attack_only ? 2 : 1)))
						{
							Tk.Recreate(NAI.TYPE.PUNCH_1, 138, false, this.Nai).Dep(this.Nai.target_x, this.Nai.target_y, null);
							this.runPunchGrabMaden(Tk.initProgress(this), Tk);
							base.AimToPlayer();
							return true;
						}
					}
					if (Tk.Progress(ref this.t, 78, true))
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool fnCheckBomb(MagicItem Mg, M2MagicCaster CheckedBy)
		{
			if (Mg.Ray != null && Mg.Other is IMgBombListener)
			{
				Vector2 mapPos = Mg.Ray.getMapPos(0f);
				if (base.isCovering(mapPos.x, mapPos.x, mapPos.y, mapPos.y, 0.13f))
				{
					return (Mg.Other as IMgBombListener).forceExplode(Mg);
				}
			}
			return false;
		}

		public override bool fineFootType()
		{
			if (this.Anm.poseIs("dive", true))
			{
				this.FootD.footstamp_type = FOOTSTAMP.NONE;
				return false;
			}
			return base.fineFootType();
		}

		private bool fnChangedBcc(M2BlockColliderContainer.BCCLine Bcc)
		{
			if (this.walk_st == 1)
			{
				this.walk_st = 0;
			}
			return true;
		}

		private bool runPunchAbsorb(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = ((X.XORSP() < 0.3f) ? (-1) : 1);
				base.PtcST("roaper_atk0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.SpSetPose("attack_stab0", -1, null, false);
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, (this.walk_st == 1) ? 32 : ((this.walk_st == -1) ? 70 : 52), true))
			{
				base.PtcST("roaper_atk1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.SpSetPose("attack_stab1", -1, null, false);
				this.walk_st = 0;
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 8f && this.walk_st == 0)
				{
					this.walk_st = 1;
					MagicItem magicItem = base.tackleInit(this.AtkPunch, this.TkAtk, MGHIT.AUTO);
					this.MpConsume(this.McsAtk, magicItem, 1f, 1f);
					base.tackleInit(this.AtkPunch, this.TkAtkB, MGHIT.AUTO);
				}
				if (this.t >= 22f)
				{
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(ref this.t, 35, true))
				{
					this.SpSetPose("attack_stab2", -1, null, false);
					Tk.AfterDelay(X.NIXP(30f, 60f));
					return false;
				}
			}
			return true;
		}

		private bool runPunchRotate(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				base.PtcST("roaper_rot0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.SpSetPose("attack_rotate0", -1, null, false);
				this.setAim((base.x < Tk.depx) ? AIM.R : AIM.L, false);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 64, true))
			{
				base.PtcST("roaper_rot1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.SpSetPose("attack_rotate1", -1, null, false);
				this.walk_st = 0;
				this.t = 200f - X.NIXP(70f, 100f) * ((Tk.type == NAI.TYPE.PUNCH_WEED) ? 0.5f : 1f);
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 8f && this.walk_st == 0)
				{
					this.walk_st = 1;
					MagicItem magicItem = base.tackleInit(this.AtkRot, this.TkRot, MGHIT.AUTO);
					this.MpConsume(this.McsRot, magicItem, 1f, 1f);
				}
				if (this.walk_st > 0 && !this.can_hold_tackle)
				{
					this.t = 300f;
				}
				this.setWalkXSpeed(base.mpf_is_right * 0.06f, true, false);
				if (!this.Useable(this.McsRot, 1f, 0f))
				{
					this.t = X.Mx(182f, this.t);
				}
				if (Tk.Progress(ref this.t, 200, true))
				{
					base.killPtc("roaper_rot1", true);
					base.remF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
					this.can_hold_tackle = false;
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
					this.SpSetPose("attack_rotate2", -1, null, false);
					Tk.AfterDelay(X.NIXP(60f, 90f));
					return false;
				}
			}
			return true;
		}

		private bool runPunchGrabMaden(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				base.PtcST("roaper_grabmaden0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.SpSetPose("dive_absorb0", -1, null, false);
				this.setAim((base.x < Tk.depx) ? AIM.R : AIM.L, false);
				base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.t >= 62f)
				{
					this.SpSetPose("dive_absorb1", -1, null, false);
				}
				if (Tk.Progress(ref this.t, 75, true))
				{
					this.SpSetPose("dive_absorb2", -1, null, false);
					base.killPtc("roaper_grabmaden0", true);
					base.PtcST("roaper_grabmaden1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 4f && this.walk_st == 0)
				{
					this.walk_st++;
					MagicItem magicItem = base.tackleInit(this.AtkAbsorbFall, this.TkMaden, MGHIT.AUTO);
					this.MpConsume(this.McsMaden, magicItem, 1f, 1f);
				}
				if (this.t >= 18f)
				{
					base.remF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(ref this.t, 80, true))
				{
					base.killPtc("roaper_grabmaden1", true);
					this.SpSetPose("dive_absorb3", -1, null, false);
					Tk.AfterDelay(X.NIXP(50f, 68f));
					return false;
				}
			}
			return true;
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.GUARD || Tk.type == NAI.TYPE.BACKSTEP)
				{
					this.NasClm.quitClimbWalk();
					this.Nai.RemF(NAI.FLAG.BOTHERED);
					if (this.state == NelEnemy.STATE.STAND || this.state == NelEnemy.STATE.ABSORB)
					{
						base.killPtc("roaper_dive_appear", true);
						base.killPtc("roaper_dive_fall", true);
					}
					if (this.Anm.poseIs("dive", true))
					{
						this.SpSetPose("stand", -1, null, false);
					}
					this.fineFootType();
				}
				if (Tk.type == NAI.TYPE.PUNCH && (this.state == NelEnemy.STATE.STAND || this.state == NelEnemy.STATE.ABSORB))
				{
					base.killPtc("roaper_atk1", true);
				}
				if (Tk.type == NAI.TYPE.WALK)
				{
					if (this.Anm.poseIs("walk", true))
					{
						this.SpSetPose("stand", -1, null, false);
					}
					if (X.XORSP() < 0.36f)
					{
						this.Nai.suit_distance = X.NIXP(0.4f, 1f) * 4f * (this.Nai.isPrGaraakiState() ? 0.5f : 1f);
					}
				}
				else
				{
					this.Nai.suit_distance = 4f;
				}
				if (Tk.type == NAI.TYPE.PUNCH_1)
				{
					if (this.state == NelEnemy.STATE.STAND || this.state == NelEnemy.STATE.ABSORB)
					{
						base.killPtc("roaper_grabmaden0", true);
						base.killPtc("roaper_grabmaden1", true);
					}
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_1, 240f);
				}
			}
			base.throw_ray = false;
			this.t_lock_dive = -1f;
			this.sink_ratio = 1f;
			if (!this.Anm.nextPoseJumpToIs("stand"))
			{
				this.SpSetPose("stand", -1, null, false);
			}
			base.remF((NelEnemy.FLAG)4194496);
			this.Phy.quitSoftFall(0f);
			this.can_hold_tackle = false;
			this.fineFootType();
			this.Phy.remLockWallHitting(this);
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			base.quitTicket(Tk);
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			if (this.Nai == null || !this.is_alive)
			{
				return false;
			}
			if (Mg.kind != MGKIND.TACKLE)
			{
				return false;
			}
			if (!this.canAbsorbContinue())
			{
				this.can_hold_tackle = false;
				return false;
			}
			return this.can_hold_tackle;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			A.Add("torture_snake_1");
			Aattr.Add(EnemyAttr.atk_attr(this, MGATTR.NORMAL));
			Aattr.Add(MGATTR.ABSORB_V);
			Aattr.Add(MGATTR.ABSORB);
		}

		public override bool isTortureUsingForAnim()
		{
			return this.Absorb != null && (this.Absorb.isTortureUsing() || this.torture_absorb_reverse);
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (this.state != NelEnemy.STATE.STAND || this.Absorb != null)
			{
				return false;
			}
			PR pr = MvTarget as PR;
			bool flag = this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE) || Abm.Con.current_pose_priority >= 99;
			this.torture_absorb_reverse = false;
			if (this.FD_EfAbsorbSphere == null)
			{
				this.FD_EfAbsorbSphere = new FnEffectRun(this.fnEfAbsorbSphere);
			}
			if (flag)
			{
				if (pr == null || !base.initAbsorb(Atk, MvTarget, Abm, penetrate))
				{
					return false;
				}
				this.initAnmBforAbsorb();
				Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 2, 63U);
				this.walk_st = 0;
				if (Abm.Con.current_pose_priority < 99)
				{
					Abm.no_wall_hit_ignore_on_torture = true;
					Abm.changeTorturePose("torture_roaper_stabbing", true, false, -1, -1);
				}
				else
				{
					this.Phy.addLockWallHitting(this, 120f);
					this.walk_st = 200;
					this.torture_absorb_reverse = true;
					this.setAim((pr.x < base.x) ? AIM.L : AIM.R, false);
					this.SpSetPose("torture_roaper_stabbing", -1, null, false);
					Abm.target_pose = "torture_roaper_stabbing";
					Abm.pose_priority = 20;
					int length = Abm.Con.Length;
					for (int i = 0; i < length; i++)
					{
						NelNRoaper nelNRoaper = Abm.Con.GetManagerItem(i).getPublishMover() as NelNRoaper;
						if (nelNRoaper != null && nelNRoaper != this && nelNRoaper.getState() == NelEnemy.STATE.ABSORB && nelNRoaper.walk_st < 1000)
						{
							nelNRoaper.walk_st -= X.IntR(X.NIXP(4f, 8f));
						}
					}
				}
				Abm.mouth_is_covered = X.XORSP() < 0.33f;
				Abm.release_from_publish_count = true;
				Abm.no_shuffle_aim = true;
				Abm.uipicture_fade_key = "torture_snake_1";
				Abm.kirimomi_release = false;
				return true;
			}
			else
			{
				if (Abm.Con.current_pose_priority >= 99)
				{
					return false;
				}
				if (pr == null || !base.initAbsorb(Atk, MvTarget, Abm, penetrate))
				{
					return false;
				}
				this.initAnmBforAbsorb();
				Abm.get_Gacha().activate(PrGachaItem.TYPE.SEQUENCE, 6, 63U);
				this.walk_st = 1000;
				Abm.changeTorturePose("torture_roaper_smt0", false, false, -1, -1);
				Abm.uipicture_fade_key = "torture_smt";
				Abm.kirimomi_release = true;
				return true;
			}
		}

		private void initAnmBforAbsorb()
		{
			if (this.AnmB == null)
			{
				this.AnmB = new EnemyAnimator(this, null, null, false);
				this.FD_fnFineAbsorbFrame = new EnemyAnimator.FnFineFrame(this.fnFineAbsorbFrame);
				this.AnmB.initS(this.Anm);
			}
			this.Anm.showToFront(true, false);
			this.Anm.fnFineFrame = this.FD_fnFineAbsorbFrame;
			this.AnmB.showToFront(false, false);
			this.AnmB.alpha = 1f;
		}

		bool ITortureListener.setTortureAnimation(string pose_name, int cframe, int loop_to)
		{
			return this.is_alive && this.torture_absorb_reverse;
		}

		void ITortureListener.setToTortureFix(float x, float y)
		{
			base.initDrawAssist(3, false);
			this.drawx_ = x * base.CLEN;
			this.drawy_ = base.y * base.CLEN;
			if (X.LENGTHXYS(base.x, base.y, x, y) > 0.125f && base.hasFoot())
			{
				float num = X.VALWALK(base.x, x, 0.03125f);
				this.Phy.addFoc(FOCTYPE.ABSORB, num - base.x, 0f, -1f, -1, 1, 0, -1, 0);
			}
		}

		void ITortureListener.runPostTorture()
		{
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 5f) || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.walk_st < 1000)
			{
				if (this.t <= 0f)
				{
					this.walk_st = this.walk_st + 100 - X.IntR(X.NIXP(3f, 6f));
					this.t = 200f;
					this.walk_time = X.NIXP(0.8f, 1.7f);
					this.checkPeeApply(this.AtkAbsorbSmall, 0.35f);
				}
				int num = 100;
				if (this.walk_st >= 200)
				{
					num = 300;
					float num2;
					float num3;
					if (!AbsorbManager.syncTorturePositionS(pr, this, this.Phy, null, out num2, out num3, CAim._XD(pr.aim, 1) != CAim._XD(this.aim, 1)))
					{
						return false;
					}
				}
				if (this.t >= 200f)
				{
					int num4 = this.walk_st;
					this.walk_st = num4 + 1;
					if (num4 >= num)
					{
						if (pr.is_alive)
						{
							this.SpSetPose("attack_stab2", -1, null, false);
							return false;
						}
						this.walk_st -= X.IntR(X.NIXP(4f, 7f));
						this.walk_time = X.NIXP(0.8f, 1.7f);
						this.checkPeeApply(this.AtkAbsorbSmall, 0.15f);
					}
					this.t = 200f - X.NIXP(24f, 30f) * this.walk_time * ((this.walk_st == num) ? 1.7f : 1f);
					float num5 = X.NIXP(4f, 6f) * (float)X.MPFXP();
					float num6 = X.NIXP(15f, 20f);
					if (X.XORSP() < 0.4f)
					{
						pr.TeCon.setQuakeSinV(num5, 44, num6, 0f, 0);
					}
					if (X.XORSP() < 0.4f)
					{
						this.TeCon.setQuakeSinH(num5 * 0.7f, 44, num6, 0f, 0);
					}
					pr.PtcVar("ax", (double)(-(double)base.mpf_is_right)).PtcST("roaper_absorb_small", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					base.runAbsorb();
					this.Anm.animReset(X.xors(8), false);
					base.applyAbsorbDamageTo(pr, this.AtkAbsorbSmall, true, false, false, 0f, false, null, false, true);
				}
			}
			else
			{
				if (this.t <= 0f)
				{
					this.t = 880f;
					this.walk_st = 1000;
					this.walk_time = 0f;
				}
				if (this.t >= 1000f)
				{
					bool flag = false;
					bool flag2;
					if (this.walk_st == 1000)
					{
						this.Absorb.emstate_attach = UIPictureBase.EMSTATE.PROG0;
						this.walk_st = 1000 + X.IntR(X.NIXP(4f, 8f) * ((X.XORSP() < 0.25f) ? 2.2f : 1f));
						float num7 = X.NIXP(4f, 6f) * (float)X.MPFXP();
						float num8 = X.NIXP(15f, 20f);
						base.nM2D.PE.setSlowFading(30f, X.NIXP(35f, 55f), (X.XORSP() < 0.25f) ? 0.08f : 0.2f, -30);
						base.nM2D.PE.addTimeFixedEffect(this.Anm, 0.125f);
						if (X.XORSP() < 0.4f)
						{
							base.nM2D.PE.addTimeFixedEffect(pr.TeCon.setQuakeSinH(num7, 44, num8, 0f, 0), 0.5f);
						}
						if (X.XORSP() < 0.4f)
						{
							base.nM2D.PE.addTimeFixedEffect(this.TeCon.setQuakeSinH(num7 * 0.7f, 44, num8, 0f, 0), 0.5f);
						}
						this.Absorb.changeTorturePose("torture_roaper_smt1", X.XORSP() < 0.5f, false, -1, -1);
						this.Anm.animReset(X.xors(8), false);
						flag2 = (flag = true);
						this.walk_time = (float)((X.XORSP() < 0.33f) ? 1 : 0);
						this.checkPeeApply(this.AtkAbsorbMaden, 0.25f);
					}
					else
					{
						flag2 = this.walk_st % 4 == 0;
						if (X.XORSP() < 0.3f)
						{
							this.Anm.randomizeFrame(0.12f, 0.23f);
						}
					}
					if (X.XORSP() < 0.2f)
					{
						base.nM2D.PE.addTimeFixedEffect(this.TeCon.setQuakeSinV(X.NIXP(3f, 5f), (int)X.NIXP(10f, 22f), X.NIXP(8f, 15f), 0f, 0), 0.5f);
					}
					base.runAbsorb();
					float num9 = X.NIXP(-0.2f, 0.2f);
					FlagCounter<SER> serDmg = this.AtkAbsorbMaden.SerDmg;
					if (!flag)
					{
						this.AtkAbsorbMaden.SerDmg = null;
					}
					float num10 = X.NIXP(-0.8f, 0.5f);
					base.nM2D.PE.addTimeFixedEffect(this.Mp.getEffect().setEffectWithSpecificFn("absorb_sphere", pr.drawx_map + num9, pr.drawy_map + num10, this.Mp.GAR(pr.drawx_map, pr.drawy_map, pr.drawx_map + num9, pr.drawy_map + num10), X.MPF(num10 < 0f) * X.MPF(num9 < 0f), 0, this.FD_EfAbsorbSphere), 0.4f);
					pr.PtcVar("first", (double)(flag2 ? 1 : 0)).PtcVar("cx", (double)(pr.drawx_map + num9)).PtcVar("cy", (double)(pr.drawy_map + num10))
						.PtcVar("ax", (double)X.MPF(num9 > 0f))
						.PtcST("roaper_absorb_maden", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					base.applyAbsorbDamageTo(pr, this.AtkAbsorbMaden, X.XORSP() < 0.5f, this.walk_time == 1f, false, 0f, false, null, false, true);
					this.AtkAbsorbMaden.SerDmg = serDmg;
					int num4 = this.walk_st - 1;
					this.walk_st = num4;
					if (num4 == 1000)
					{
						this.t = 1000f - X.NIXP(50f, 130f);
					}
					else
					{
						this.t = 1000f - X.NIXP(1.8f, 2.6f);
					}
				}
			}
			return true;
		}

		private void checkPeeApply(NelAttackInfo Atk, float ratio)
		{
			if (X.XORSP() < ratio)
			{
				if (Atk.SerDmg != null)
				{
					Atk.SerDmg.Add(SER.MILKY, 200f);
				}
				Atk.pee_apply100 = 50f;
				return;
			}
			if (Atk.SerDmg != null)
			{
				Atk.SerDmg.Rem(SER.MILKY);
			}
			Atk.pee_apply100 = 0f;
		}

		private void fnFineAbsorbFrame(EnemyFrameDataBasic nF, PxlFrame F)
		{
			if (this.AnmB != null && this.AnmB.alpha > 0f)
			{
				this.AnmB.layer_mask = 1U;
				this.Anm.layer_mask = 4294967294U;
				this.AnmB.fineCurrentFrameData();
			}
		}

		private bool fnEfAbsorbSphere(EffectItem Ef)
		{
			if (base.destructed || !this.is_alive || !base.isAbsorbState())
			{
				return false;
			}
			uint ran = X.GETRAN2(Ef.index + 23U, 8U);
			float num = X.NI(35, 50, X.RAN(ran, 1389));
			if (Ef.af >= num)
			{
				return false;
			}
			float num2 = Ef.z - 1.5707964f;
			float num3 = X.NI(0.8f, 1.1f, X.RAN(ran, 2334));
			float num4 = -X.NI(7, 10, X.RAN(ran, 2310)) * (float)Ef.time;
			float num5 = -X.NI(24, 33, X.RAN(ran, 2667)) * (float)Ef.time;
			float num6 = X.NI(20, 35, X.RAN(ran, 2667));
			float num7 = X.NI(34, 45, X.RAN(ran, 2184)) + num6;
			float num8 = num5 - X.NI(2, 6, X.RAN(ran, 2667)) * (float)Ef.time;
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
			MeshDrawer meshImg2 = Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
			float num9 = X.ZLINE(Ef.af, num);
			float num10 = X.ZSIN2(num9);
			float num11 = X.ZLINE(num9 - 0.4f, 0.6f);
			meshImg.Scale(X.NI(0.66f, 1f, X.ZSINV(num9 - 0.25f, 0.75f)), 1f, false).Rotate(num2, false);
			meshImg2.setCurrentMatrix(meshImg.getCurrentMatrix(), false);
			float num12 = X.BEZIER_I(0f, num4, num8, num5, num10);
			float num13 = X.BEZIER_I(0f, num7, num7, num6, num10);
			meshImg.Col = meshImg.ColGrd.Set(4282515265U).blend(4294446377U, X.ZSIN(num11, 0.6f)).mulA(1f - X.ZLINE(num9 - 0.6f, 0.39999998f))
				.C;
			meshImg2.Col = meshImg2.ColGrd.Set(4288551284U).C;
			float num14 = (13f + 5f * X.COSI(Ef.af, 3.7f) + 2.3f * X.COSI(Ef.af, 11.3f)) * (1f - num11 * 0.3f) * num3;
			float num15 = (31f + 18f * X.COSI(Ef.af, 4.1f) + 2.4f * X.COSI(Ef.af, 9.73f)) * (1f - num11) * num3;
			float num16 = X.NI(num14, num15, 0.66f);
			float num17 = num15 * 1.4f;
			meshImg.initForImg(MTRX.EffBlurCircle245, 0).Rect(num12, num13, num16, num16, false);
			meshImg.initForImg(MTRX.EffCircle128, 0).Rect(num12, num13, num14, num14, false);
			meshImg2.initForImg(MTRX.EffBlurCircle245, 0).Rect(num12, num13, num17, num17, false);
			meshImg2.initForImg(MTRX.EffBlurCircle245, 0).Rect(num12, num13, num15, num15, false);
			return true;
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return base.applyHpDamageRatio(Atk) * (float)(base.hasF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL) ? 2 : 1);
		}

		public override AttackInfo applyDamageFromMap(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo _Atk, float efx, float efy, bool apply_execute = true)
		{
			if (this.Anm.poseIs("dive", true) && this.t_lock_dive >= 0f)
			{
				this.t_lock_dive += 8f;
				return null;
			}
			return base.applyDamageFromMap(MDI, _Atk, efx, efy, apply_execute) as NelAttackInfo;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			this.posename_damage = (this.posename_damage_huttobi = (base.hasF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL) ? "damage_critical" : "damage"));
			bool flag = this.Anm.poseIs("dive", true) && this.t_lock_dive >= 0f;
			if (flag || (this.state == NelEnemy.STATE.STAND && base.hasF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL)))
			{
				this.stunable_in_checkstun = true;
			}
			this.TeCon.hasSpecific(TEKIND.DMG_BLINK);
			int num = base.applyDamage(Atk, force);
			if (num > 0)
			{
				if (flag && X.XORSP() < X.NI(0.6f, 0.125f, this.enlarge_level - 1f))
				{
					this.Ser.Add(SER.EATEN, 150, 99, false);
				}
				if (this.Ser.has(SER.EATEN) && this.Anm.poseIs("damage", false))
				{
					this.posename_damage = (this.posename_damage_huttobi = "damage_critical");
					this.SpSetPose("damage_critical", -1, null, false);
				}
				if (this.t_lock_critical_snd < this.Mp.floort && this.Anm.poseIs("damage_critical", false))
				{
					this.playSndPos("roaper_critical", 1);
					this.t_lock_critical_snd = this.Mp.floort + 30f;
				}
			}
			return num;
		}

		public override bool checkDamageStun(NelAttackInfo Atk, float level = 1f)
		{
			if (base.isOverDrive())
			{
				return false;
			}
			if (this.stunable_in_checkstun)
			{
				level *= this.basic_stun_ratio;
				if (level > 0f)
				{
					return X.XORSP() < level * X.NI(0.95f, 0.6f, base.mp_ratio);
				}
			}
			return false;
		}

		private bool fall_attack_only = true;

		protected EnAttackInfo AtkAbsorbFall = new EnAttackInfo(0.02f, 0.03f)
		{
			hpdmg0 = 4,
			split_mpdmg = 4,
			is_grab_attack = true,
			knockback_len = 0.2f,
			parryable = true
		};

		protected EnAttackInfo AtkPunch = new EnAttackInfo(0.01f, 0.02f)
		{
			hpdmg0 = 4,
			split_mpdmg = 8,
			is_grab_attack = true,
			knockback_len = 0.2f,
			parryable = true
		};

		protected EnAttackInfo AtkRot = new EnAttackInfo(0.04f, 0.09f)
		{
			hpdmg0 = 4,
			split_mpdmg = 3,
			knockback_len = 0.5f,
			parryable = true,
			attr = MGATTR.WIP,
			Beto = BetoInfo.Blood,
			huttobi_ratio = -1000f
		};

		protected EnAttackInfo AtkAbsorbSmall = new EnAttackInfo(0.004f, 0.006f)
		{
			hpdmg0 = 5,
			mpdmg0 = 10,
			attr = MGATTR.ABSORB,
			Beto = BetoInfo.Absorbed,
			EpDmg = new EpAtk(20, "roaper")
			{
				vagina = 4,
				breast = 8,
				mouth = 10,
				other = 5
			},
			SerDmg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 2f)
		};

		protected EnAttackInfo AtkAbsorbMaden = new EnAttackInfo(0.004f, 0.006f)
		{
			hpdmg0 = 1,
			mpdmg0 = 3,
			split_mpdmg = 3,
			attr = MGATTR.ABSORB,
			Beto = BetoInfo.Absorbed,
			EpDmg = new EpAtk(5, "roaper")
			{
				vagina = 4,
				breast = 8,
				mouth = 4,
				anal = 4,
				other = 5
			}.MultipleOrgasm(3f),
			SerDmg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 30f)
		};

		private const NAI.TYPE NTYPE_CRAWL_GROUND = NAI.TYPE.GUARD;

		private const NAI.TYPE NTYPE_PUNCH_ABSORB = NAI.TYPE.PUNCH;

		private const NAI.TYPE NTYPE_PUNCH_ROTATE = NAI.TYPE.PUNCH_0;

		private const NAI.TYPE NTYPE_PUNCH_GRABMADEN = NAI.TYPE.PUNCH_1;

		private NASGroundClimber NasClm;

		private const float dig_speed = 0.16f;

		private const float suit_distance_0 = 4f;

		private NOD.TackleInfo TkAtk = NOD.getTackle("roaper_atk");

		private NOD.TackleInfo TkAtkB = NOD.getTackle("roaper_atk_b");

		private NOD.TackleInfo TkFall = NOD.getTackle("roaper_fall");

		private NOD.TackleInfo TkRot = NOD.getTackle("roaper_rot");

		private NOD.TackleInfo TkMaden = NOD.getTackle("roaper_maden");

		private NOD.MpConsume McsDive = NOD.getMpConsume("roaper_dive");

		private NOD.MpConsume McsAtk = NOD.getMpConsume("roaper_atk");

		private NOD.MpConsume McsRot = NOD.getMpConsume("roaper_rot");

		private NOD.MpConsume McsMaden = NOD.getMpConsume("roaper_maden");

		private bool torture_absorb_reverse;

		private float t_lock_dive = -1f;

		private bool stunable_in_checkstun;

		private const float XSPD_ROTATING = 0.06f;

		private const float stun_in_diving_ratio_min = 0.125f;

		private const float stun_in_diving_ratio_max = 0.6f;

		private const int T_STUN_IN_DIVING = 150;

		private EnemyAnimator AnmB;

		private EnemyAnimator.FnFineFrame FD_fnFineAbsorbFrame;

		private const int PRI_WALK = 5;

		private const int PRI_ATK = 138;

		private MagicItem.FnCheckMagicFrom FD_BombCheck;

		private FnEffectRun FD_EfAbsorbSphere;

		private float t_lock_critical_snd;
	}
}
