using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNPig : NelEnemy
	{
		public float run_speed
		{
			get
			{
				return this.NasJmp.run_speed;
			}
		}

		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			ENEMYID id = this.id;
			this.id = ENEMYID.PIG_0;
			NOD.BasicData basicData = NOD.getBasicData("PIG_0");
			base.appear(_Mp, basicData);
			base.base_gravity = 0.67f;
			this.consider_damaging_tired_ser = true;
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 7f;
			this.Nai.attackable_length_top = -4f;
			this.Nai.attackable_length_bottom = 4f;
			this.Nai.suit_distance = 2.4f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.absorb_weight = 3;
			this.NasJmp = new NASSmallJumper(this, 0.14f, 3f)
			{
				FD_RunPrepare = new NAI.FnTicketRun(this.FD_RunPrepare),
				FD_JumpPrepare = new NAI.FnTicketRun(this.FD_JumpPrepare),
				FD_JumpInit = new NASSmallJumper.FnJumpInit(this.FD_JumpInit),
				FD_RunJumping = new NAI.FnTicketRun(this.FD_RunJumping),
				FD_Land = new NAI.FnTicketRun(this.FD_Land),
				jump_pose = "jump0_1",
				walk_pose = "attack_running",
				MAX_JUMP_COUNT_ON_ONETICKET = 1,
				jump_height_margin = 0.25f,
				jump_height_min = 1.5f,
				jump_height_max = 3.6f,
				jumpable_x_range_max = 4.5f
			};
			this.AtkTacklePress.Prepare(this, false);
			this.AtkPunchLift.Prepare(this, false);
			this.AtkAbsorbPiston.Prepare(this, true);
			this.AtkAbsorbPiston2.Prepare(this, true);
			this.AtkAbsorbPistonFinish.Prepare(this, true);
			this.AtkTackleWeed.Prepare(this, true);
			this.AtkAbsorbLicking.Prepare(this, false);
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			NelEnemy.STATE state = this.state;
			base.changeState(st);
			if (state == NelEnemy.STATE.ABSORB)
			{
				if (this.NasInj != null)
				{
					this.NasInj.quitAbsorb();
				}
				this.Anm.showToFront(false, false);
				this.Nai.delay += 110f;
			}
			if (this.state == NelEnemy.STATE.STAND)
			{
				if (state == NelEnemy.STATE.DAMAGE && this.Nai.HasF(NAI.FLAG.ATTACKED, false))
				{
					this.Nai.AddF(NAI.FLAG.ATTACKED, 140f);
				}
				else
				{
					this.Nai.RemF(NAI.FLAG.ATTACKED);
				}
			}
			return this;
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai, NAI.TYPE.GAZE))
			{
				return true;
			}
			if (!Nai.hasPriorityTicket(131, false, false))
			{
				if (Nai.HasF(NAI.FLAG.ATTACKED, true) && Nai.isAttackableLength(base.x, base.y, 2.5f, -1f, 2.5f, true))
				{
					Nai.AddTicket(NAI.TYPE.GUARD, 133, true).Dep(Nai.target_x, Nai.target_lastfoot_bottom, Nai.TargetLastBcc);
					return true;
				}
				int prAbsorbedPriority = Nai.getPrAbsorbedPriority();
				if (prAbsorbedPriority < -1)
				{
					if (Nai.HasF(NAI.FLAG.BOTHERED, false))
					{
						if (!Nai.hasTypeLock(NAI.TYPE.PUNCH))
						{
							return Nai.AddTicketB(NAI.TYPE.PUNCH, 133, true);
						}
						if (Nai.AddMoveTicketFor(base.x + Nai.NIRANtk(3f, 5.3f, 3115) * (float)X.MPFXP(Nai.RANtk(2387)), base.mbottom, null, 131, true, NAI.TYPE.BACKSTEP) != null)
						{
							return true;
						}
					}
					if (!Nai.hasTypeLock(NAI.TYPE.PUNCH_0) && Nai.RANtk(315) < 0.5f && Nai.isAttackableLength(true))
					{
						M2BlockColliderContainer.BCCLine targetLastBcc = Nai.TargetLastBcc;
						if (targetLastBcc != null && this.FootD.get_LastBCC().isLinearWalkableTo(targetLastBcc, 5) != 0)
						{
							Nai.AddTicket(NAI.TYPE.PUNCH_0, 133, true).Dep(new Vector2(Nai.target_x, Nai.target_y), null);
							return true;
						}
					}
				}
				if (base.mp_ratio < 0.44f && Nai.AddTicketSearchAndGetManaWeed(133, 5f, -5f, 5f, 1.15f, -1.15f, 1.15f, true) != null)
				{
					return true;
				}
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				M2BlockColliderContainer.BCCLine targetLastBcc2 = Nai.TargetLastBcc;
				if (prAbsorbedPriority >= 99)
				{
					if (Nai.RANtk(435) < 0.5f && footBCC != null && targetLastBcc2 != null && footBCC.isLinearWalkableTo(targetLastBcc2, false) != 0)
					{
						Nai.AddTicket(NAI.TYPE.MAG, 133, true).Dep(new Vector2(Nai.target_x, Nai.target_y), null);
						return true;
					}
					if (Nai.RANtk(897) < 0.35f)
					{
						return Nai.fnBasicMove(Nai);
					}
				}
				if (targetLastBcc2 != null && footBCC != null && X.Abs(base.x - Nai.target_x) < this.NasJmp.jumpable_x_range_max + 1f && footBCC.isLinearWalkableTo(targetLastBcc2, 0) != 0)
				{
					float num = (float)X.MPF(base.x < Nai.target_x);
					Nai.AddTicket(NAI.TYPE.PUNCH_1, 131, true).Dep(Nai.target_x + Nai.NIRANtk(-0.3f, 2.1f, 331) * num * (Nai.isPrGaraakiState() ? 0.15f : 1f), Nai.target_lastfoot_bottom, targetLastBcc2);
					return true;
				}
				if (Nai.AddMoveTicketFor(Nai.target_x + Nai.NIRANtk(-3f, 3f, 2397) * (Nai.isPrGaraakiState() ? 0.15f : 1f), Nai.target_lastfoot_bottom, null, 131, true, NAI.TYPE.WALK) != null)
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

		public override bool readTicket(NaTicket Tk)
		{
			bool flag = false;
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
			case NAI.TYPE.WALK_TO_WEED:
			case NAI.TYPE.BACKSTEP:
				flag = false;
				if (Tk.initProgress(this))
				{
					flag = true;
					this.walk_st = 0;
					this.walk_time = 0f;
					this.t = 0f;
					if (this.Nai.HasF(NAI.FLAG.INJECTED, false))
					{
						this.walk_st = 1;
						this.NasJmp.jump_270 = this.Nai.RANtk(438) < 0.2f;
					}
					else
					{
						this.NasJmp.jump_270 = this.Nai.RANtk(438) < 0.5f;
					}
				}
				if (!this.NasJmp.runWalk(flag, Tk, ref this.t))
				{
					NAI.TYPE type2 = Tk.type;
					return false;
				}
				return true;
			case NAI.TYPE.PUNCH:
			case NAI.TYPE.PUNCH_0:
				return this.runPunch_LiftUp(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_1:
				if (Tk.initProgress(this))
				{
					flag = true;
					this.walk_st = 1;
					this.walk_time = 0f;
					this.NasJmp.forceJumpPrepareInit(Tk, Tk.DepBCC, ref this.t);
				}
				return this.NasJmp.runWalk(flag, Tk, ref this.t);
			case NAI.TYPE.PUNCH_2:
			case NAI.TYPE.MAG_0:
			case NAI.TYPE.MAG_1:
			case NAI.TYPE.MAG_2:
			case NAI.TYPE.GUARD_0:
			case NAI.TYPE.GUARD_1:
			case NAI.TYPE.GUARD_2:
			case NAI.TYPE.APPEAL_0:
				break;
			case NAI.TYPE.PUNCH_WEED:
				return this.runPunch_PunchWeed(Tk.initProgress(this), Tk);
			case NAI.TYPE.MAG:
				return this.runPunch_Licking(Tk.initProgress(this), Tk);
			case NAI.TYPE.GUARD:
				return this.runPunch_Counter(Tk.initProgress(this), Tk);
			default:
				if (type == NAI.TYPE.WAIT)
				{
					base.AimToLr((X.xors(2) == 0) ? 0 : 2);
					Tk.after_delay = 30f + this.Nai.RANtk(840) * 40f;
					return false;
				}
				break;
			}
			return base.readTicket(Tk);
		}

		public bool FD_RunPrepare(NaTicket Tk)
		{
			return this.walking_jumpPrepare(Tk, 1);
		}

		public bool FD_JumpPrepare(NaTicket Tk)
		{
			return this.walking_jumpPrepare(Tk, 0);
		}

		public bool walking_jumpPrepare(NaTicket Tk, int ignore_walkst)
		{
			if (this.walk_st == ignore_walkst)
			{
				return false;
			}
			if (!this.Anm.poseIs("jump0_0", true))
			{
				this.SpSetPose("jump0_0", -1, null, false);
				this.t = 0f;
				base.PtcST("pig_jump_prepare", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
			}
			return this.t < 50f;
		}

		public bool FD_JumpInit(NaTicket Tk, ref Vector4 V)
		{
			this.walk_st = 2;
			if (V.z <= 0f)
			{
				return true;
			}
			this.walk_st = 0;
			bool flag = true;
			if (this.NasJmp.JumpTargetTo != null)
			{
				float num = ((base.mpf_is_right < 0f) ? this.NasJmp.JumpTargetTo.shifted_right_y : this.NasJmp.JumpTargetTo.shifted_left_y);
				flag = num < base.mbottom - 0.05f;
				if (base.mbottom - num > this.NasJmp.jump_height_max)
				{
					this.Nai.AddF(NAI.FLAG.BOTHERED, 240f);
					return false;
				}
			}
			if (flag)
			{
				V.x = X.Mn(X.Abs(Tk.depx - base.x) / V.z, this.NasJmp.run_speed) * base.mpf_is_right;
			}
			this.Phy.addLockGravityFrame((int)V.z);
			this.Phy.addFoc(FOCTYPE.WALK, V.x, 0f, -1f, 0, (int)X.Mx(V.z - 3f, 1f), 4, 1, 0);
			this.walk_time = X.Mx(1f, V.z - 15f);
			base.PtcVar("by", (double)(base.y - this.sizey * 0.4f)).PtcST("pig_jump_init", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
			V.x = 0f;
			return true;
		}

		private bool FD_RunJumping(NaTicket Tk)
		{
			bool footBCC = this.FootD.get_FootBCC() != null;
			this.FootD.lockPlayFootStamp(2);
			this.sink_ratio = 100f;
			if (footBCC)
			{
				base.PtcVar("sizex", (double)((0.5f + this.sizex) * this.Mp.CLENB)).PtcVar("bx", (double)base.x).PtcVar("by", (double)(base.y - this.sizey * 0.4f))
					.PtcST("enemy_ground_bump", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Phy.quitSoftFall(0f);
				if (this.walk_st == 0)
				{
					this.walk_st = 1;
				}
				return false;
			}
			if (this.walk_time > 0f)
			{
				this.walk_time -= this.TS;
				if (this.walk_time <= 0f)
				{
					this.walk_time = -100f;
					this.t = 0f;
					this.Phy.addLockGravityFrame(25);
					base.playSndPos("pig_attack1", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW, null);
				}
			}
			else if (this.walk_time == -100f)
			{
				this.SpSetPose("jump0_2", -1, null, false);
				float num = base.mpf_is_right * 0.5654867f;
				this.Anm.rotationR = X.NI(num, 0f, X.ZSIN(this.t, 15f));
				if (this.t >= 25f)
				{
					this.Phy.initSoftFall(0.88f, 10f);
				}
			}
			return true;
		}

		private bool FD_Land(NaTicket Tk)
		{
			if (this.t < 5f)
			{
				if (this.walk_st == 1)
				{
					this.AtkTacklePress.parryable = false;
					base.tackleInit(this.AtkTacklePress, this.TkPress, MGHIT.AUTO);
					this.walk_st = 2;
					EnemyAttr.Splash(this, 1.25f);
				}
			}
			else
			{
				this.can_hold_tackle = false;
				this.sink_ratio = 1f;
			}
			return this.t < 50f;
		}

		private bool runPunch_LiftUp(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				base.PtcST("pig_liftup_attack0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				base.PtcST("pig_dangerous_star", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.SpSetPose("grawl", -1, null, false);
				this.walk_st = 0;
				this.t = 0f;
				base.addF(NelEnemy.FLAG.HAS_SUPERARMOR);
				this.LiftupTarget = null;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 45, true))
			{
				this.walk_st = 0;
				float num = -1000f;
				if (!this.Ser.has(SER.CONFUSE))
				{
					int num2 = this.Mp.count_players;
					for (int i = 0; i < num2; i++)
					{
						M2Attackable m2Attackable;
						if (this.getLiftupTarget(this.Mp.getPr(i), out m2Attackable) && (num < -1000f || X.Abs(base.x - num) > X.Abs(base.x - m2Attackable.x)))
						{
							num = m2Attackable.x;
							this.LiftupTarget = m2Attackable;
						}
					}
				}
				if (num == -1000f)
				{
					int num2 = this.Mp.count_movers;
					for (int j = 0; j < num2; j++)
					{
						M2Attackable m2Attackable2;
						if (this.getLiftupTarget(this.Mp.getMv(j), out m2Attackable2) && (num < -1000f || X.Abs(base.x - num) > X.Abs(base.x - m2Attackable2.x)))
						{
							num = m2Attackable2.x;
							this.LiftupTarget = m2Attackable2;
						}
					}
				}
				if (num == -1000f)
				{
					this.LiftupTarget = null;
					this.setAim((base.mpf_is_right > 0f) ? AIM.L : AIM.R, false);
					this.walk_st = 0;
					Tk.Dep(base.x + base.mpf_is_right * 10f, base.mbottom, null);
				}
				else
				{
					this.setAim((num < base.x) ? AIM.L : AIM.R, false);
					this.walk_st = 1;
					Tk.Dep(this.LiftupTarget.x + base.mpf_is_right * 2.4f, this.LiftupTarget.y, null);
				}
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.SpSetPose("attack_running", -1, null, false);
				base.PtcST("pig_liftup_running", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.walk_time = 0f;
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_st == 1)
				{
					if (this.LiftupTarget == null || !this.LiftupTarget.is_alive)
					{
						this.LiftupTarget = null;
						this.walk_st = 0;
					}
					else
					{
						Tk.Dep(this.LiftupTarget.x + base.mpf_is_right * 2.4f, this.LiftupTarget.y, null);
						float num3 = base.x + (this.sizex + this.Nai.NIRANtk(0.1f, 0.5f, 3314)) * base.mpf_is_right;
						if (this.LiftupTarget.isCovering(num3 - 0.25f, num3 + 0.25f, base.y - 1.8f, base.y + 0.2f, 0f) || this.LiftupTarget.isCovering(base.mleft + 0.125f, base.mright + 0.125f, base.y - 1.8f, base.y + 0.2f, 0f))
						{
							this.t = 150f;
						}
					}
				}
				this.Phy.setWalkXSpeed(0.21f * base.mpf_is_right, true, false);
				if (this.t >= 100f)
				{
					this.SpSetPose("attack1", -1, null, false);
					Tk.prog = PROG.PROG1;
					base.PtcST("pig_liftup_attack1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
					base.killPtc("pig_liftup_running", false);
					base.killPtc("pig_dangerous_star", true);
					this.t = 0f;
					base.tackleInit(this.AtkPunchLift, this.TkLiftup, MGHIT.BERSERK | base.mg_hit).Ray.hit_target_max = 4;
				}
				else
				{
					if (base.wallHittedA())
					{
						base.remF(NelEnemy.FLAG.HAS_SUPERARMOR);
						this.can_hold_tackle = false;
						this.MpConsume(this.McsHitWall, null, 1f, 1f);
						base.PtcST("pig_attack1_wallhit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.changeState(NelEnemy.STATE.DAMAGE);
						EnemyAttr.Splash(this, base.x + this.sizex * 0.4f * base.mpf_is_right, base.y, 0.7f, 1f, 1f);
						this.Phy.addFoc(FOCTYPE.DAMAGE, -base.mpf_is_right * X.NI(0.22f, 0.08f, this.enlarge_level - 1f), -0.1f, -1f, 0, 2, 40, 20, 0);
						this.SpSetPose("damage", -1, null, false);
						if (!this.Nai.HasF(NAI.FLAG.POWERED, false) && X.XORSP() < 0.3f)
						{
							this.Ser.Add(SER.EATEN, 120, 99, false);
							this.Nai.AddF(NAI.FLAG.POWERED, 440f);
						}
						return false;
					}
					if ((base.mpf_is_right > 0f) ? (base.x > Tk.depx) : (base.x < Tk.depx))
					{
						this.t += this.TS * 6f;
					}
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.t >= 8f && base.hasF(NelEnemy.FLAG.HAS_SUPERARMOR))
				{
					base.remF((NelEnemy.FLAG)2097216);
					this.PtcHld.killPtc("pig_liftup_attack0", true);
					this.can_hold_tackle = false;
				}
				this.Phy.setWalkXSpeed(0.21f * (1f - X.ZSIN(this.t, 25f)) * base.mpf_is_right, true, false);
				if (Tk.Progress(ref this.t, 65, true))
				{
					this.SpSetPose("attack2", -1, null, false);
					this.PtcHld.killPtc("pig_liftup_attack1", true);
					Tk.after_delay = X.NIXP(50f, 90f);
					return false;
				}
			}
			return true;
		}

		public bool getLiftupTarget(M2Mover Mv0, out M2Attackable Ret)
		{
			Ret = null;
			M2Attackable m2Attackable = Mv0 as M2Attackable;
			if (!(m2Attackable != null))
			{
				return false;
			}
			if (m2Attackable is NelEnemy)
			{
				NelEnemy nelEnemy = m2Attackable as NelEnemy;
				if (nelEnemy.throw_ray || nelEnemy.getState() != NelEnemy.STATE.STAND)
				{
					return false;
				}
			}
			if (m2Attackable is PR && (m2Attackable as PR).throw_ray)
			{
				return false;
			}
			Ret = m2Attackable;
			return true;
		}

		public override bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			if (Atk == this.AtkPunchLift && HitMv != null && HitMv.Mv is M2Attackable)
			{
				if (HitMv.Mv is M2MoverPr || base.AimPr == null)
				{
					Atk.burst_vx = (float)((X.XORSP() > 0.5f) ? 1 : (-1)) * 0.04f;
					Atk.burst_vy = -0.7f;
				}
				else
				{
					M2Attackable m2Attackable = HitMv.Mv as M2Attackable;
					if (HitMv.Mv is NelNPig)
					{
						NelNPig nelNPig = HitMv.Mv as NelNPig;
						if (nelNPig.getState() == NelEnemy.STATE.STAND)
						{
							nelNPig.remF(NelEnemy.FLAG.HAS_SUPERARMOR);
						}
					}
					Atk.burst_vx = 0f;
					Atk.burst_vy = 0f;
					if (m2Attackable.getPhysic() != null && base.AimPr != null)
					{
						m2Attackable.jumpInit(base.AimPr.x - HitMv.Mv.x, base.AimPr.y - HitMv.Mv.y, X.Mx(0f, HitMv.Mv.y - base.AimPr.y) + 8f, false);
					}
				}
				this.can_hold_tackle = false;
			}
			return true;
		}

		private bool runPunch_Counter(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				base.PtcST("pig_counter0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				base.addF((NelEnemy.FLAG)6291520);
				this.SizeW(60f, 140f, ALIGN.CENTER, ALIGNY.BOTTOM);
				this.SpSetPose("attack_grab1", -1, null, false);
				this.sink_ratio = 100f;
				this.t = 0f;
				this.walk_st = 0;
				this.walk_time = 0f;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 40, true))
			{
				base.playSndPos("pig_charge", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW, null);
				this.SpSetPose("attack_grab2", -1, null, false);
				base.AimToPlayer();
			}
			if (Tk.prog >= PROG.PROG0 && this.walk_time < 26f)
			{
				this.Phy.setWalkXSpeed(base.mpf_is_right * 0.24f * (1f - X.ZSINV(this.walk_time, 26f)), true, false);
				this.walk_time += this.TS;
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 8f && this.walk_st == 0)
				{
					this.walk_st = 1;
					this.AtkTacklePress.parryable = true;
					base.tackleInit(this.AtkTacklePress, this.TkCounter, MGHIT.AUTO);
					this.fineEnlargeScale(-1f, false, false);
				}
				if (Tk.Progress(ref this.t, 16, true))
				{
					this.walk_st = 0;
					base.PtcST("pig_counter_bump", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.t >= 3f && base.hasF(NelEnemy.FLAG.HAS_SUPERARMOR))
				{
					this.can_hold_tackle = false;
					this.sink_ratio = 1f;
					base.remF((NelEnemy.FLAG)6291520);
					base.killPtc("pig_counter0", true);
				}
				if (Tk.Progress(ref this.t, 75, true))
				{
					this.SpSetPose("attack_grab3", -1, null, false);
					Tk.AfterDelay(X.NIXP(20f, 35f));
					return false;
				}
			}
			return true;
		}

		private bool runPunch_PunchWeed(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				base.playSndPos("pig_grawl2", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW, null);
				this.SpSetPose("attack_small", -1, null, false);
				this.sink_ratio = 100f;
				this.t = 0f;
				this.walk_st = 0;
				this.walk_time = 0f;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 24, true))
			{
				base.tackleInit(this.AtkTackleWeed, this.TkCounter, MGHIT.AUTO);
			}
			if (Tk.prog >= PROG.PROG0 && this.walk_time < 20f)
			{
				this.Phy.setWalkXSpeed(base.mpf_is_right * 0.13f * (1f - X.ZSINV(this.walk_time, 20f)), true, false);
				this.walk_time += this.TS;
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 8f && this.walk_st == 0)
				{
					this.walk_st = 1;
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(ref this.t, 40, true))
				{
					Tk.AfterDelay(X.NIXP(20f, 35f));
					return false;
				}
			}
			return true;
		}

		private bool runPunch_Licking(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				if (!this.Nai.isPrAbsorbed())
				{
					return false;
				}
				this.SpSetPose("attack_running", -1, null, false);
				this.sink_ratio = 100f;
				this.t = 0f;
				this.walk_st = 0;
				this.walk_time = 0f;
				if (X.XORSP() < 0.88f)
				{
					this.walk_st = ((base.AimPr.mpf_is_right > 0f) ? 0 : 2);
				}
				else
				{
					this.walk_st = (int)base.AimPr.aim;
				}
				this.Anm.showToFront(base.AimPr.mpf_is_right == 1f != (this.walk_st == 2), false);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (!this.Nai.isPrAbsorbed())
				{
					return false;
				}
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				M2BlockColliderContainer.BCCLine targetLastBcc = this.Nai.TargetLastBcc;
				if (base.wallHitted((this.walk_st == 2) ? AIM.L : AIM.R))
				{
					float num = this.walk_time + 1f;
					this.walk_time = num;
					if (num >= 2f)
					{
						return false;
					}
					this.walk_st = ((this.walk_st == 2) ? 0 : 2);
				}
				float num2 = (float)((this.walk_st == 2) ? 1 : (-1));
				if (footBCC == null || targetLastBcc == null || this.t >= 140f || (footBCC.isLinearWalkableTo(targetLastBcc, 0) & ((num2 <= 0f || 3 != 0) ? 1 : 0)) == 0)
				{
					return false;
				}
				float num3 = this.Nai.target_x - num2 * this.sizex * 0.7f;
				float num4 = this.Nai.NIRANtk(num3 + this.Nai.target_sizex * 0.15f * num2, num3 - this.Nai.target_sizex * 0.2f * num2, 3176) - num2 * this.sizex * 0.7f;
				this.Phy.setWalkXSpeed(X.absMn(num4 - base.x, 0.06f * ((X.Abs(base.x - num4) < 1.1f) ? 0.5f : 1f)), true, false);
				if (X.Abs(base.x - num4) <= 0.03f)
				{
					this.t = 0f;
					this.walk_time = 0f;
					Tk.prog = PROG.PROG0;
					base.playSndPos("pig_grawl2", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW, null);
					this.SpSetPose("attack_lick", -1, null, false);
					this.setAim((AIM)this.walk_st, false);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (!this.Nai.isPrAbsorbed())
				{
					return false;
				}
				if (Tk.Progress(ref this.t, 30, true))
				{
					this.t = 200f;
					this.walk_st = 0;
				}
			}
			if (Tk.prog >= PROG.PROG1 && this.t >= 200f)
			{
				PR pr = base.AimPr as PR;
				if (pr == null || !this.canAbsorbContinue() || !pr.isAbsorbState())
				{
					return false;
				}
				float num5 = base.x + base.mpf_is_right * (this.sizex * 0.7f + 0.25f);
				float num6 = base.y + this.sizey * 0.3f;
				base.PtcVar("cx", (double)num5).PtcVar("cy", (double)num6).PtcST("pig_licking", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				if (pr.isCovering(num5 - 0.45f, num5 + 0.45f, num6 - 0.3f, num6 + 0.3f, 0f))
				{
					float num7 = X.NIXP(4f, 6f) * (float)X.MPFXP();
					float num8 = X.NIXP(20f, 25f);
					if (X.XORSP() < 0.7f)
					{
						pr.TeCon.setQuakeSinH(num7, 44, num8, 0f, 0);
						this.TeCon.setQuakeSinH(num7 * 0.7f, 44, num8, 0f, 0);
					}
					if (X.XORSP() < 0.3f || !pr.is_alive)
					{
						base.runAbsorb();
						base.applyAbsorbDamageTo(pr, this.AtkAbsorbLicking, true, false, false, 0f, false, null, false, true);
					}
					else
					{
						base.applyAbsorbDamageTo(pr, this.AtkAbsorbLicking, false, false, false, 0f, false, null, false, true);
					}
				}
				else
				{
					this.walk_time += 1f;
					if (this.walk_time >= 4f)
					{
						return false;
					}
				}
				this.walk_st++;
				this.Anm.randomizeFrame(0.5f, 0.5f);
				this.t = (float)(200 - (18 + X.xors(18)) + ((this.walk_st % 6 == 2) ? 5 : 0));
			}
			return true;
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.WALK || Tk.type == NAI.TYPE.WALK_TO_WEED)
				{
					this.NasJmp.quitTicket(Tk);
					this.Nai.AddF(NAI.FLAG.INJECTED, 200f);
					this.Anm.rotationR = 0f;
					this.Nai.delay += X.NIXP(10f, 22f);
					if (Tk.isError())
					{
						this.Nai.AddF(NAI.FLAG.BOTHERED, 240f);
					}
				}
				if (Tk.type == NAI.TYPE.BACKSTEP)
				{
					this.NasJmp.quitTicket(Tk);
					this.Anm.rotationR = 0f;
					this.Nai.delay += X.NIXP(60f, 100f);
				}
				if (Tk.type == NAI.TYPE.PUNCH_1)
				{
					this.NasJmp.quitTicket(Tk);
					this.Anm.rotationR = 0f;
					this.Nai.delay += X.NIXP(10f, 22f);
				}
				if (Tk.type == NAI.TYPE.PUNCH_0)
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 160f);
				}
				if (Tk.type == NAI.TYPE.PUNCH && X.XORSP() < 0.3f)
				{
					this.Nai.RemF(NAI.FLAG.BOTHERED);
				}
				if (Tk.type == NAI.TYPE.MAG)
				{
					this.Anm.showToFront(false, false);
					this.Nai.delay += X.NIXP(70f, 100f);
				}
				this.SpSetPose("stand", -1, null, false);
			}
			this.sink_ratio = 1f;
			base.remF((NelEnemy.FLAG)6291520);
			this.can_hold_tackle = false;
			this.Phy.quitSoftFall(0f);
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			base.quitTicket(Tk);
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			A.Add("torture_ketsudasi");
			Aattr.Add(EnemyAttr.atk_attr(this, MGATTR.NORMAL));
			Aattr.Add(MGATTR.ABSORB_V);
			Aattr.Add(MGATTR.ABSORB);
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (this.state != NelEnemy.STATE.STAND || this.Absorb != null || Abm.Con.current_pose_priority >= 99)
			{
				return false;
			}
			PR pr = MvTarget as PR;
			if (pr == null || !base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			Abm.kirimomi_release = true;
			this.Phy.killSpeedForce(true, true, true, false, false);
			Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 10, 63U);
			Abm.get_Gacha().SoloPositionPixel = new Vector3(0f, 1f, 0f);
			Abm.changeTorturePose("torture_pig_0", false, false, -1, -1);
			Abm.release_from_publish_count = true;
			Abm.publish_float = true;
			Abm.no_shuffle_aim = true;
			this.Anm.showToFront(true, false);
			pr.fine_frozen_replace = true;
			Abm.uipicture_fade_key = "torture_ketsudasi";
			this.Phy.addLockWallHitting(this, 140f);
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.NasInj == null)
			{
				this.NasInj = new NASStandardInjection(this, "torture_pig_0", "torture_pig_1", "torture_pig_2", "torture_pig_3", this.AtkAbsorbPiston, this.AtkAbsorbPiston2, this.AtkAbsorbPistonFinish)
				{
					posename_after = "torture_pig_4",
					snd_breathe_piston = "pig_breath",
					fadekey_prepare = "torture_ketsudasi",
					fadekey_after = "torture_ketsudasi_finish",
					first_time_ratio = 2f,
					ep_add_ratio = 2f,
					ep_add_ratio_ep_high = 1.25f,
					orgazm_time_ratio = 0.5f,
					egg_plant_val = 0.34f,
					eggcateg = PrEggManager.CATEG.PIG
				};
			}
			this.NasInj.finishratio_laying_base = (this.NasInj.finishratio_base = (pr.is_alive ? 0.33f : 1f));
			bool flag;
			return this.NasInj.runAbsorb(ref this.t, pr, this.Absorb, out flag);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			float num = base.applyHpDamageRatio(Atk);
			if (Atk is NelAttackInfo)
			{
				NelAttackInfo nelAttackInfo = Atk as NelAttackInfo;
				if (nelAttackInfo.PublishMagic != null && nelAttackInfo.PublishMagic.is_normal_attack)
				{
					num *= 0.66f;
				}
			}
			return num;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			if (Atk != null && Atk.PublishMagic != null)
			{
				if (Atk.PublishMagic.is_normal_attack)
				{
					base.addF(NelEnemy.FLAG.DMG_EFFECT_SHIELD);
				}
				else if (Atk.PublishMagic.is_chanted_magic && Atk.tired_time_to_super_armor > 20f)
				{
					base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
					base.remF(NelEnemy.FLAG.HAS_SUPERARMOR);
				}
			}
			int num = base.applyDamage(Atk, force);
			if (num > 0)
			{
				if (Atk != null && Atk.Caster is M2MoverPr)
				{
					this.Nai.AimPr = Atk.Caster as M2MoverPr;
					this.Nai.AddF(NAI.FLAG.ATTACKED, 170f);
				}
				if (this.state == NelEnemy.STATE.DAMAGE && Atk != null && Atk.PublishMagic != null && Atk.PublishMagic.is_chanted_magic && !Atk.PublishMagic.is_normal_attack)
				{
					int num2 = (int)X.MMX(30f, 10f + Atk.tired_time_to_super_armor * 1.5f, 140f);
					this.Ser.Add(SER.TIRED, num2, 99, false);
					this.TeCon.setQuake(4f, num2, 1.5f, 0);
					this.setDmgBlink(Atk.attr, (float)num2, 0.7f, 0);
				}
			}
			return num;
		}

		protected EnAttackInfo AtkTacklePress = new EnAttackInfo(0.2f, 0.3f)
		{
			hpdmg0 = 18,
			is_grab_attack = true,
			knockback_len = 0.2f,
			parryable = false
		};

		protected EnAttackInfo AtkPunchLift = new EnAttackInfo(0.1f, 0.3f)
		{
			hpdmg0 = 25,
			knockback_len = 1.2f,
			parryable = true
		};

		protected EnAttackInfo AtkTackleWeed = new EnAttackInfo(0.01f, 0.01f)
		{
			hpdmg0 = 4,
			knockback_len = 0.8f,
			parryable = true,
			burst_vx = 0.04f
		};

		protected EnAttackInfo AtkAbsorbLicking = new EnAttackInfo
		{
			split_mpdmg = 9,
			mpdmg0 = 2,
			attr = MGATTR.ABSORB,
			Beto = BetoInfo.Normal.Pow(20, false),
			EpDmg = new EpAtk(4, "pig")
			{
				other = 3
			}
		};

		protected EnAttackInfo AtkAbsorbPiston = new EnAttackInfo(0.004f, 0.004f)
		{
			split_mpdmg = 7,
			hpdmg0 = 6,
			attr = MGATTR.ABSORB_V,
			Beto = BetoInfo.Normal.Pow(20, false),
			EpDmg = new EpAtk(7, "pig")
			{
				vagina = 3,
				canal = 9,
				gspot = 2
			}
		};

		protected EnAttackInfo AtkAbsorbPiston2 = new EnAttackInfo(0.003f, 0.003f)
		{
			split_mpdmg = 16,
			hpdmg0 = 2,
			attr = MGATTR.ABSORB_V,
			Beto = BetoInfo.Normal.Pow(10, false),
			EpDmg = new EpAtk(14, "pig")
			{
				vagina = 1,
				canal = 9,
				gspot = 6
			}
		};

		protected EnAttackInfo AtkAbsorbPistonFinish = new EnAttackInfo
		{
			split_mpdmg = 22,
			hpdmg0 = 0,
			attr = MGATTR.ABSORB_V,
			Beto = BetoInfo.Sperm2,
			EpDmg = new EpAtk(90, "pig")
			{
				canal = 6,
				gspot = 7
			}
		};

		private NOD.TackleInfo TkPress = NOD.getTackle("pig_press");

		private NOD.TackleInfo TkLiftup = NOD.getTackle("pig_liftup");

		private NOD.TackleInfo TkCounter = NOD.getTackle("pig_counter");

		private NOD.MpConsume McsHitWall = NOD.getMpConsume("pig_tackle_hitwall");

		private NASSmallJumper NasJmp;

		private const float T_JUMP_PREPARE = 50f;

		private const float T_LAND_DELAY = 50f;

		private const float T_LAND_ATTACK_MAXT = 5f;

		private const int T_LIFTUP_STUN = 120;

		private const float RUN_SPEED_LIFTUP = 0.21f;

		public float pr_high_height;

		public const float dmg_ratio_shotgun = 0.66f;

		private const NAI.TYPE NTYPE_JUMPATTACK = NAI.TYPE.PUNCH_1;

		private const NAI.TYPE NTYPE_PUNCHLIFT = NAI.TYPE.PUNCH;

		private const NAI.TYPE NTYPE_PUNCHLIFT_NEAR = NAI.TYPE.PUNCH_0;

		private const NAI.TYPE NTYPE_LICK = NAI.TYPE.MAG;

		private const NAI.TYPE NTYPE_COUNTER = NAI.TYPE.GUARD;

		private const int PRI_ATK = 133;

		private const int PRI_WALK = 131;

		private M2Attackable LiftupTarget;

		private NASStandardInjection NasInj;
	}
}
