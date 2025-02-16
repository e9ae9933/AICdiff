using System;
using System.Collections.Generic;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNPuppy : NelEnemy
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			NOD.BasicData basicData;
			if (this.id == ENEMYID.PUPPY_EVENT_0)
			{
				basicData = NOD.getBasicData("PUPPY_EVENT_0");
			}
			else
			{
				this.id = ENEMYID.PUPPY_0;
				basicData = NOD.getBasicData("PUPPY_0");
			}
			this.SizeW(40f, 90f, ALIGN.CENTER, ALIGNY.MIDDLE);
			base.appear(_Mp, basicData);
			if (NelNPuppy.OposeN2S == null)
			{
				NelNPuppy.OposeN2S = new BDic<string, string>(16);
				NelNPuppy.OposeS2N = new BDic<string, string>(16);
				this.PLn("stand", "stand_small").PLn("gaze", "stand_small").PLn("gaze2stand", "stand_small")
					.PLn("walk", "walk_escape")
					.PLn("walk", "walk_escape")
					.PLn("walk_running", "walk_escape")
					.PLn("walk2stand", "walk_escape2stand_small")
					.PLn("turn", "walk_escape_turn")
					.PLn("fall", "fall_small")
					.PLn("land", "land_small")
					.PLn("damage", "damage_small")
					.PLn("atk_0", "atk_small")
					.PLn("atk_1", "atk_small")
					.PLn("atk_2", "atk_small")
					.PLn("atk_3", "atk_small")
					.PLn("atk_end", "atk_small")
					.PLn("atk_2_cramp", "atk_small");
			}
			this.FlgSmall = new Flagger(delegate(FlaggerT<string> V)
			{
				if (!this.is_small)
				{
					base.FixSizeW(45f, 30f);
				}
			}, delegate(FlaggerT<string> V)
			{
				if (!this.is_small)
				{
					base.FixSizeW(40f, 90f);
				}
			});
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 6f;
			this.Nai.attackable_length_top = -3f;
			this.Nai.attackable_length_bottom = 4f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.suit_distance = -1f;
			NelAttackInfoBase atkAbsorb = this.AtkAbsorb;
			NelAttackInfoBase atkAbsorbDead = this.AtkAbsorbDead;
			EpAtk epAtk = new EpAtk(9, "tentacle");
			epAtk.vagina = 3;
			epAtk.canal = 6;
			epAtk.anal = 2;
			epAtk.cli = 2;
			EpAtk epAtk2 = epAtk;
			atkAbsorbDead.EpDmg = epAtk;
			atkAbsorb.EpDmg = epAtk2;
			this.AtkAbsorbStab.EpDmg = new EpAtk(6, "tentacle_2")
			{
				vagina = 2,
				anal = 4,
				urethra = 2,
				mouth = 2
			};
			this.FD_findNextLeader = new Func<AbsorbManager, bool>(this.findNextLeader);
			this.FD_findNextLeaderDef = new Func<AbsorbManager, bool>(this.findNextLeaderDef);
			this.exist_land_pose = (this.exist_fall_pose = true);
			this.GClimb = new NASGroundClimber(this, 4f);
			this.GClimb.addChangedFn(new NASGroundClimber.FnClimbEvent(this.fnChangedBcc));
			this.GClimb.leave_allocate_time = 120;
			this.absorb_weight = 1;
			this.AtkSmallPunch.Prepare(this, true);
			this.AtkAbsorb.Prepare(this, true);
			this.AtkAbsorbDead.Prepare(this, true);
			this.AtkAbsorbGrab.Prepare(this, false);
			this.AtkAbsorbWip.Prepare(this, false);
			this.AtkAbsorbStab.Prepare(this, false);
		}

		private NelNPuppy PLn(string n, string s)
		{
			NelNPuppy.OposeN2S[n] = s;
			NelNPuppy.OposeS2N[s] = n;
			return this;
		}

		public override void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			base.quitSummonAndAppear(clearlock_on_summon);
			float num = X.NI(100, 220, NightController.XORSP());
			this.Nai.addTypeLock(NAI.TYPE.PUNCH, num);
			this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, num);
			this.Nai.addTypeLock(NAI.TYPE.PUNCH_1, num);
			this.Nai.addTypeLock(NAI.TYPE.PUNCH_2, num);
		}

		protected virtual bool considerNormal(NAI Nai)
		{
			if (this.is_small)
			{
				if (!Nai.isPrGaraakiState() && Nai.target_len < 4f && !base.hasPT(40, false, false) && !Nai.isPrMagicChanting(1f) && this.TkMove(Nai.RANtk(782) < 0.2f, 40, false, false) != null)
				{
					return true;
				}
				if (!base.hasPT(5, false, false))
				{
					if (Nai.RANtk(413) < 0.78f && Nai.target_len > 5.5f && base.mp_ratio >= 0.375f)
					{
						Nai.AddTicket(NAI.TYPE.MAG_0, 5, true);
						return true;
					}
					if (Nai.RANtk(385) < 0.75f && Nai.AddTicketSearchAndGetManaWeed(5, 8f, -2f, 5f, 8f, -2f, 5f, false) != null)
					{
						return true;
					}
					if (this.TkMove(false, 5, false, false) != null)
					{
						return true;
					}
				}
			}
			else
			{
				if (base.AimPr != null && !base.AimPr.is_alive && base.AimPr is PR && Nai.hasTypeLock(NAI.TYPE.PUNCH))
				{
					if (base.hasPT(100, false, false))
					{
						return Nai.fnBasicMove(Nai);
					}
					bool flag = (int)(Nai.target_lastfoot_bottom - 0.25f) == (int)(base.footbottom - 0.25f);
					if (flag)
					{
						if (Nai.target_len <= 2.3f + Nai.RANtk(4189) * 1.65f && flag)
						{
							this.TkMove(X.XORSP() < 0.5f, 100, false, true);
							return true;
						}
						if ((base.AimPr as PR).getAbsorbTorturePublisher() != null)
						{
							Nai.AddTicket(NAI.TYPE.GAZE, 100, true);
							return true;
						}
						if (Nai.RANtk(1418) < 0.78f)
						{
							Nai.remTypeLock(NAI.TYPE.PUNCH);
							Nai.remTypeLock(NAI.TYPE.PUNCH_0);
						}
					}
				}
				if (Nai.fnAwakeBasicHead(Nai, NAI.TYPE.GAZE))
				{
					return true;
				}
				bool flag2 = Nai.isPrGaraakiState() || Nai.isPrMagicExploded(1f) || (Nai.HasF(NAI.FLAG.BOTHERED, false) && Nai.RANtk(3581) < 0.75f);
				if (!base.hasPT(100, false, false) && flag2 && this.TkCaptureAtk(80, 100, NAI.TYPE.PUNCH_0, true) != null)
				{
					return true;
				}
				if (!base.hasPT(5, false, false))
				{
					if (Nai.target_len <= 4f && !flag2 && !Nai.isPrMagicChanting(1f))
					{
						this.TkMove(false, 5, false, false);
						return true;
					}
					if ((flag2 || X.Abs(base.x - Nai.target_x) >= 3f) && this.TkCaptureAtk(20, 100, NAI.TYPE.PUNCH, true) != null)
					{
						return true;
					}
					if (base.mp_ratio < 0.5f && Nai.AddTicketSearchAndGetManaWeed(100, 2.5f, -0.25f, 0.25f, 2.5f, -0.25f, 0.25f, false) != null)
					{
						return true;
					}
				}
				if (!base.hasPT(2, false, false) && X.BTW(4f, Nai.target_len, 8f) && !flag2)
				{
					float num = Nai.RANtk(4483);
					if (num < 0.5f && this.TkMove(false, 40, true, false) != null)
					{
						return true;
					}
					if (num < 0.75f && this.TkMove(true, 5, true, false) != null)
					{
						return true;
					}
					Nai.AddTicket(NAI.TYPE.GAZE, 2, true);
					return true;
				}
				else if (Nai.target_len >= 7.5f && this.TkMove(Nai.RANtk(3281) < 0.5f, 1, false, false) != null)
				{
					return true;
				}
			}
			if (Nai.HasF(NAI.FLAG.BOTHERED, false) && Nai.RANtk(1967) < 0.5f && !base.hasPT(5, false, false))
			{
				return Nai.AddTicketB(NAI.TYPE.WALK, 5, true);
			}
			if (!base.hasPT(0, false, false))
			{
				Nai.AddTicketB(NAI.TYPE.GAZE, 0, true);
			}
			return false;
		}

		private NaTicket TkMove(bool forward, int priority, bool check_declining = true, bool do_not_wall_dash = false)
		{
			NaTicket naTicket = this.Nai.AddMoveTicketFor(base.x + (float)(4 * this.Nai.xd_to_pr_aim * X.MPF(forward)), base.y, null, priority, check_declining, NAI.TYPE.WALK);
			if (naTicket != null && do_not_wall_dash && naTicket.type == NAI.TYPE.WALK)
			{
				naTicket.type = NAI.TYPE.BACKSTEP;
			}
			return naTicket;
		}

		private NaTicket TkCaptureAtk(int ratio100, int priority, NAI.TYPE ntype = NAI.TYPE.PUNCH, bool check_declining = true)
		{
			if (base.hasPT(priority, false, false) || (ratio100 > 0 && this.Nai.RANtk(1414) * 100f >= (float)ratio100))
			{
				return null;
			}
			if (!this.Useable(this.McsCapture, 1f, (float)this.maxmp * 0.08f + 5f))
			{
				return null;
			}
			int num = -1;
			if (check_declining)
			{
				if (this.Nai.hasTypeLock(ntype))
				{
					return null;
				}
				if (X.Abs(base.x - this.Nai.target_x) >= 9f)
				{
					return null;
				}
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				M2BlockColliderContainer.BCCLine targetLastBcc = this.Nai.TargetLastBcc;
				if (footBCC == null || targetLastBcc == null || !X.BTW(targetLastBcc.x, this.Nai.target_x, targetLastBcc.right))
				{
					return null;
				}
				if (targetLastBcc != footBCC)
				{
					if (this.Nai.getCurTicket() != null && this.linearcheck_lock_t > this.Mp.floort)
					{
						return null;
					}
					this.linearcheck_lock_t = this.Mp.floort + 30f;
					int num2 = footBCC.isLinearWalkableTo(targetLastBcc, 4);
					if (num2 == 0)
					{
						if (!this.is_small && this.Nai.RANtk(4747) < 0.4f)
						{
							this.Nai.AddF(NAI.FLAG.BOTHERED, 180f);
						}
						return null;
					}
					num = ((num2 == 1) ? 0 : 2);
				}
			}
			if (ratio100 == 0)
			{
				return this.Nai.getCurTicket();
			}
			NaTicket naTicket = this.Nai.AddTicket(ntype, priority, true);
			if (num >= 0)
			{
				naTicket.SetAim(num);
			}
			return naTicket;
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
			case NAI.TYPE.WALK_TO_WEED:
				break;
			case NAI.TYPE.PUNCH:
			case NAI.TYPE.PUNCH_0:
			case NAI.TYPE.PUNCH_2:
				return !this.is_small && this.runCapturePunch(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_1:
			case NAI.TYPE.MAG:
				goto IL_00D2;
			case NAI.TYPE.PUNCH_WEED:
				return this.runPunchWeed(Tk.initProgress(this), Tk);
			case NAI.TYPE.MAG_0:
				return this.is_small && this.runTransformFromSmall(Tk.initProgress(this), Tk);
			default:
				if (type != NAI.TYPE.BACKSTEP)
				{
					if (type != NAI.TYPE.GAZE)
					{
						goto IL_00D2;
					}
					if (this.is_small)
					{
						return base.readTicket(Tk);
					}
					return this.runGaze(Tk.initProgress(this), Tk);
				}
				break;
			}
			bool flag = Tk.initProgress(this);
			int num = base.walkThroughLift(flag, Tk, 20);
			if (num >= 0)
			{
				return num == 0;
			}
			return this.runWalk(flag, Tk, false, Tk.type == NAI.TYPE.BACKSTEP);
			IL_00D2:
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
			this.quitTicketWalk();
			base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			this.Nai.RemF(NAI.FLAG.INJECTED);
			if (Tk != null)
			{
				if (Tk.isMove() && !base.hasFoot())
				{
					this.SpSetPose("fall", -1, null, false);
				}
				if (!this.is_small && (Tk.type == NAI.TYPE.GAZE || Tk.type == NAI.TYPE.PUNCH_WEED || (Tk.type == NAI.TYPE.WALK && this.Nai.RANtk(3775) <= 40f)) && !this.Nai.HasF(NAI.FLAG.BOTHERED, false) && this.Nai.RANtk(1871) <= 40f)
				{
					M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
					M2BlockColliderContainer.BCCLine targetLastBcc = this.Nai.TargetLastBcc;
					if (footBCC != null && (targetLastBcc == null || footBCC.isLinearWalkableTo(targetLastBcc, 4) == 0))
					{
						this.Nai.AddF(NAI.FLAG.BOTHERED, 180f);
					}
				}
			}
			if (this.is_small)
			{
				this.Nai.RemF(NAI.FLAG.BOTHERED);
			}
		}

		public void quitTicketWalk()
		{
			this.FlgSmall.Rem("ATK");
			this.Anm.rotationR = 0f;
			this.GClimb.quitClimbWalk();
			this.Anm.after_offset_x = 0f;
			if (this.SndLoopWalkF != null)
			{
				this.SndLoopWalkF.destruct();
				this.SndLoopWalk.destruct();
			}
			this.SndLoopWalk = null;
			this.SndLoopWalkF = null;
		}

		protected override bool initDeathEffect()
		{
			this.quitTicketWalk();
			return base.initDeathEffect();
		}

		protected void createWalkSnd()
		{
			if (!X.DEBUGNOSND)
			{
				this.SndLoopWalk = this.Mp.M2D.Snd.createInterval(this.snd_key, "tentacle_move", 90f, this, 0f, 128).TimeAbsolute(false);
				this.SndLoopWalkF = this.Mp.M2D.Snd.createInterval(this.snd_key, "tentacle_foot", (float)(this.is_small ? 3 : 7), this, 0f, 128).TimeAbsolute(false);
			}
		}

		public void deactivateWalkSnd()
		{
			if (this.SndLoopWalk != null)
			{
				this.SndLoopWalk.active = (this.SndLoopWalkF.active = false);
			}
		}

		private bool runWalk(bool init_flag, NaTicket Tk, bool stop_near_dep = false, bool do_not_wall_dash = false)
		{
			if (init_flag)
			{
				if (!this.GClimb.initClimbWalk(15U))
				{
					this.Nai.addTypeLock(Tk.type, 120f);
					return false;
				}
				this.t = 0f;
				this.walk_time = ((!do_not_wall_dash) ? (-40f - this.Nai.RANtk(394) * 40f - (float)(stop_near_dep ? 90 : 0)) : (-25f - this.Nai.RANtk(481) * 34f));
				this.walk_st = -1;
				this.SpSetPose("walk", -1, null, false);
				this.FlgSmall.Add("ATK");
				Tk.after_delay = (this.is_small ? 0f : (9f + this.Nai.RANtk(855) * 20f));
				this.createWalkSnd();
			}
			float num = X.ZLINE(this.t + 1f, 18f);
			if (this.walk_time >= 0f)
			{
				if (base.hasFoot() && !base.SpPoseIs("walk2stand", "land"))
				{
					this.SpSetPose("walk2stand", -1, null, false);
				}
				this.FootD.footable_bits = 8U;
				num *= 1f - X.ZLINE(this.walk_time, 18f);
				if (num <= 0f)
				{
					return false;
				}
			}
			bool flag = this.GClimb.progressClimbWalk(num * (this.is_small ? 0.21f : X.NI(0.23f, 0.17f, this.enlarge_level - 1f)));
			if (this.Nai.HasF(NAI.FLAG.INJECTED, true) && this.TkCaptureAtk(0, 128, NAI.TYPE.PUNCH_2, true) != null)
			{
				this.quitTicketWalk();
				Tk.Recreate(NAI.TYPE.PUNCH_2, 128, false, null);
				this.Nai.AddF(NAI.FLAG.INJECTED, 180f);
				return true;
			}
			if (flag && CAim._YD(this.GClimb.getPreBcc().aim, 1) < 0)
			{
				this.walk_time += this.TS;
			}
			if (!base.hasFoot())
			{
				if (!this.canJump())
				{
					this.setFallPose();
				}
				if (this.is_small && this.walk_time < 0f && this.walk_time > -60f)
				{
					this.walk_time = (float)(-40 - X.xors(40));
				}
				if (this.SndLoopWalk != null)
				{
					this.SndLoopWalk.active = (this.SndLoopWalkF.active = false);
				}
			}
			else
			{
				this.FootD.lockPlayFootStamp(4);
				if (base.hasFoot() && !base.SpPoseIs("walk", "walk_running"))
				{
					this.SpSetPose("walk", 2, null, false);
				}
				if (this.SndLoopWalk != null)
				{
					this.SndLoopWalk.active = (this.SndLoopWalkF.active = true);
				}
				if (stop_near_dep && this.walk_time < 0f && X.LENGTHXYS(base.x, base.y, Tk.depx, Tk.depy) < 1.25f)
				{
					this.walk_time = 0f;
				}
			}
			return flag;
		}

		private bool fnChangedBcc(M2BlockColliderContainer.BCCLine Bcc)
		{
			int num = CAim._YD(Bcc.aim, 1);
			M2BlockColliderContainer.BCCLine preBcc = this.GClimb.getPreBcc();
			if (preBcc == null)
			{
				return false;
			}
			if (num == 0)
			{
				if (((preBcc.SideL == Bcc) ? preBcc.L_is_270 : preBcc.R_is_270) && X.XORSP() < 0.6f)
				{
					base.initJump();
					this.Phy.addFoc(FOCTYPE.WALK, this.GClimb.getVelocityDir().x * X.NI(0.14f, 0.08f, this.is_small ? 0f : (this.enlarge_level - 1f)), 0f, -1f, 0, 90, 50, 5, 0);
					return false;
				}
				M2BlockColliderContainer.BCCLine targetBCC = this.GClimb.targetBCC;
				if (this.is_small || this.walk_time >= 0f || (X.XORSP() < 0.4f && (targetBCC == null || targetBCC == preBcc)))
				{
					this.GClimb.Turn();
					this.t = 0f;
					this.SpSetPose("turn", -1, null, false);
					return false;
				}
			}
			else if (num > 0)
			{
				if (X.XORSP() < 0.4f)
				{
					this.GClimb.Turn();
					this.t = 0f;
					this.SpSetPose("turn", -1, null, false);
					return false;
				}
				this.SpSetPose("fall", -1, null, false);
				if (preBcc != null)
				{
					base.initJump();
					float num2 = X.NI(0.14f, 0.08f, this.is_small ? 0f : (this.enlarge_level - 1f));
					if ((preBcc.SideL == Bcc) ? preBcc.L_is_90 : preBcc.R_is_90)
					{
						this.Phy.addFoc(FOCTYPE.WALK, (float)(-(float)CAim._XD(preBcc.aim, 1)) * num2, 0f, -1f, 0, 90, 50, 5, 0);
					}
					else
					{
						Vector3 velocityDir = this.GClimb.getVelocityDir();
						this.Phy.addFoc(FOCTYPE.WALK, velocityDir.x * num2, velocityDir.y * num2, -1f, 0, 90, 50, 5, 0);
					}
				}
				this.GClimb.quitClimbWalk();
				return false;
			}
			else if (num < 0 && !this.is_small && this.Nai.TargetLastBcc == Bcc && X.XORSP() < (this.Nai.HasF(NAI.FLAG.BOTHERED, false) ? X.NI(0.5f, 0.9f, this.enlarge_level - 1f) : X.NI(0.07f, 0.25f, this.enlarge_level - 1f)))
			{
				this.Nai.AddF(NAI.FLAG.INJECTED, 180f);
			}
			return true;
		}

		private bool runGaze(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_time = -50f - this.Nai.RANtk(285) * 30f;
				this.FlgSmall.Add("ATK");
			}
			this.walk_time += this.TS;
			if (this.walk_time >= 0f)
			{
				if (this.SpPoseIs("gaze"))
				{
					this.SpSetPose("gaze2stand", -1, null, false);
				}
				this.FlgSmall.Rem("ATK");
				return this.walk_time < 20f;
			}
			if (base.hasFoot() && !base.SpPoseIs("gaze", "land"))
			{
				this.SpSetPose("gaze", -1, null, false);
			}
			return true;
		}

		public bool runPunchWeed(bool init_flag, NaTicket Tk)
		{
			if (Tk.prog < PROG.PROG4)
			{
				if (base.hasFoot() && (int)(Tk.depy + 0.2f) == (int)(base.mbottom + 0.2f) && X.LENGTHXYS(base.x, 0f, Tk.depx, 0f) < 0.6f)
				{
					Tk.prog = PROG.PROG4;
					this.t = 0f;
					if (this.is_small)
					{
						Tk.priority = X.Mx(Tk.priority, 100);
					}
					if (this.SndLoopWalk != null)
					{
						this.SndLoopWalk.active = (this.SndLoopWalkF.active = false);
					}
					this.quitTicketWalk();
				}
				else
				{
					if (!this.runWalk(init_flag, Tk, true, false))
					{
						this.Nai.addDeclineArea(Tk.depx - 1f, Tk.depy - 1f, 2f, 2f, 600f);
						return false;
					}
					return true;
				}
			}
			else
			{
				if (this.t >= 20f && Tk.prog == PROG.PROG4)
				{
					Tk.prog = PROG.PROG5;
					base.tackleInit(this.AtkSmallPunch, 0.3f, 0f, this.is_small ? 0.25f : 0.4f, false, false, MGHIT.AUTO);
					this.jumpInit(base.mpf_is_right * 1.4f, 0f, this.is_small ? 0.2f : 0.16f, false);
					this.SpSetPose("atk_end", -1, null, false);
				}
				if (this.t >= 28f)
				{
					this.can_hold_tackle = false;
				}
				if (this.t >= (float)(28 + (this.is_small ? 58 : 36)))
				{
					return false;
				}
			}
			return true;
		}

		public bool runTransformFromSmall(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("land", -1, null, false);
				base.AimToPlayer();
			}
			if (this.t >= 12f && Tk.prog == PROG.ACTIVE)
			{
				Tk.prog = PROG.PROG0;
				this.t = 0f;
				this.SpSetPose("small2stand", -1, null, false);
				base.PtcST("tentacle_guard_gather", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
			}
			if (this.t >= 40f && Tk.prog == PROG.PROG0)
			{
				base.PtcST("tentacle_guard_gather_quit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.is_small = false;
				if (this.FlgSmall.isActive())
				{
					base.FixSizeW(45f, 30f);
				}
				else
				{
					base.FixSizeW(40f, 90f);
				}
				Tk.after_delay = 50f;
				return false;
			}
			return true;
		}

		public bool runCapturePunch(bool init_flag, NaTicket Tk)
		{
			if (!init_flag)
			{
				if (Tk.prog == PROG.ACTIVE && base.hasFoot())
				{
					if (!this.SpPoseIs("atk_2") && this.t >= 20f)
					{
						this.SpSetPose("atk_2", -1, null, false);
					}
					if (this.t >= 40f)
					{
						base.AimToPlayer();
						Tk.prog = PROG.PROG0;
						this.SpSetPose("atk_2_cramp", -1, null, false);
						this.t = 0f;
						this.Anm.rotationR = 0f;
						this.walk_st = 3;
						this.walk_time = (float)((this.walk_st > 0) ? (-3) : 40);
					}
				}
				if (Tk.prog == PROG.PROG0)
				{
					this.walk_time += this.TS;
					if (this.walk_time >= 20f)
					{
						int num = this.walk_st - 1;
						this.walk_st = num;
						if (num <= 0)
						{
							this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, 180f);
							this.walk_time = 0f;
							this.walk_st = 0;
							this.Phy.remFoc(FOCTYPE.WALK | FOCTYPE.JUMP, true);
							this.t = 0f;
							Tk.prog = PROG.PROG1;
							this.Anm.rotationR = 0f;
							this.SpSetPose("atk_3", -1, null, false);
							this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, 0.26f * base.mpf_is_right, 0f, -1f, 0, 80, 0, -1, 0);
							base.PtcVar("ydf", (double)(this.sizey * 2.3f * this.Mp.CLENB)).PtcST("puppy_attack_1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
						}
						else
						{
							this.SpSetPose("atk_2_cramp", 1, null, false);
							this.walk_time = 0f;
						}
					}
				}
				if (Tk.prog == PROG.PROG1)
				{
					MagicItem magicItem = null;
					if (base.hasFoot())
					{
						if (this.t >= 8f && this.Nai.target_mbottom > base.mbottom + 1.5f)
						{
							this.skip_lift_mapy = (int)(this.Nai.target_mbottom - 0.25f);
							if (this.FootD.FootIsLift())
							{
								this.FootD.initJump(false, true, false);
							}
						}
						else
						{
							this.skip_lift_mapy = 0;
						}
					}
					if (base.wallHittedA() && !this.Mp.canStand((int)(base.x + base.mpf_is_right * (this.sizex + 0.25f)), (int)base.y))
					{
						this.Phy.remFoc(FOCTYPE.WALK, true);
						this.jumpInit(-base.mpf_is_right * 2.6f, 0f, X.NI(1.5f, 1.1f, this.enlarge_level - 1f), false);
						Tk.prog = PROG.PROG2;
						base.PtcST("puppy_attack_hit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						EnemyAttr.Splash(this, 0.66f);
						this.SpSetPose("fall", -1, null, false);
					}
					else if (this.t >= 80f)
					{
						this.Phy.remFoc(FOCTYPE.WALK, true);
						this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, 0.26f * base.mpf_is_right * 0.3f, 0f, -1f, 0, 0, 40, -1, 0);
						Tk.prog = PROG.PROG2;
						this.SpSetPose("atk_end", -1, null, false);
						this.can_hold_tackle = false;
					}
					else if (this.Nai.target_len < 4.5f + this.sizex)
					{
						magicItem = base.tackleInit(this.AtkCapture, this.TkiCapture, MGHIT.AUTO);
						base.PtcST("puppy_attack_tackle", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
						this.Phy.remFoc(FOCTYPE.WALK, true);
						this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, 0.26f * base.mpf_is_right, 0f, -1f, 0, 25, 20, -1, 0);
						Tk.prog = PROG.PROG2;
						this.SpSetPose("atk_end", -1, null, false);
					}
					else
					{
						this.Anm.rotationR = this.Mp.GAR(0f, 0f, base.vx, base.vy) + ((base.mpf_is_right < 0f) ? 3.1415927f : 0f);
					}
					if (Tk.prog == PROG.PROG2)
					{
						this.t = 0f;
						this.walk_st = 0;
						this.skip_lift_mapy = 0;
						this.Anm.rotationR = 0f;
						this.MpConsume(this.McsCapture, magicItem, 1f, 1f);
						base.killPtc("puppy_attack_1", false);
						if (!base.hasFoot())
						{
							this.SpSetPose("fall", -1, null, false);
						}
						base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					}
				}
				if (Tk.prog == PROG.PROG2)
				{
					if (this.t >= 24f && this.walk_st == 0)
					{
						this.can_hold_tackle = false;
						this.FlgSmall.Rem("ATK");
						this.walk_st = 1;
					}
					if (this.t >= 70f)
					{
						this.can_hold_tackle = false;
						return false;
					}
				}
				return true;
			}
			this.t = 0f;
			M2BlockColliderContainer.BCCLine bccline = (this.Nai.HasF(NAI.FLAG.INJECTED, true) ? this.Nai.TargetLastBcc : this.FootD.get_FootBCC());
			this.Nai.RemF(NAI.FLAG.BOTHERED);
			if (bccline == null || this.is_small)
			{
				return false;
			}
			this.walk_st = 0;
			this.walk_time = 0f;
			this.FlgSmall.Add("ATK");
			base.PtcST("puppy_attack_0", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			float num2 = ((bccline.SideL != null) ? bccline.SideL.left : bccline.left) + this.sizex;
			float num3 = ((bccline.SideR != null) ? bccline.SideR.right : bccline.right) - this.sizex;
			float num4 = X.MMX(num2, base.x - X.NIXP(0.8f, 1.2f), num3);
			float num5 = X.MMX(num2, base.x + X.NIXP(0.8f, 1.2f), num3);
			float num6 = ((X.Abs(X.Abs(num4 - this.Nai.target_x) - 5.5f) < X.Abs(X.Abs(num5 - this.Nai.target_x) - 5.5f)) ? num4 : num5);
			this.jumpInit(num6 - base.x, 0f, X.NI(1.6f, 1.2f, this.enlarge_level - 1f), false);
			this.SpSetPose("atk_0", -1, null, false);
			base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			this.FootD.lockPlayFootStamp(80);
			this.can_hold_tackle = false;
			return true;
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle;
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			NelEnemy.STATE state = this.state;
			this.quitTicketWalk();
			if (state == NelEnemy.STATE.STAND)
			{
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
				this.Nai.RemF(NAI.FLAG.BOTHERED);
			}
			bool flag = this.Absorb != null && this.Absorb.isTortureUsing();
			string text = null;
			AbsorbManagerContainer absorbManagerContainer = ((this.Absorb != null) ? this.Absorb.Con : null);
			this.NextLeader = null;
			if (base.isAbsorbState(state) && !base.isAbsorbState(st))
			{
				int num = 180;
				if (base.AimPr != null && !base.AimPr.is_alive)
				{
					num = 320 + X.xors(2300);
				}
				this.Nai.addTypeLock(NAI.TYPE.PUNCH, (float)num);
				this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, (float)num);
				if (!base.isDamagingOrKo(st))
				{
					this.jumpInit(-base.mpf_is_right * 0.35f, 0f, X.NI(1.6f, 1.2f, this.enlarge_level - 1f), false);
					text = "fall";
				}
				if (this.AbsorbLeader != null)
				{
					this.AbsorbLeader.progressTorture(-1);
					this.AbsorbLeader = null;
				}
				else if (flag)
				{
					this.Absorb.Con.countTortureItem(this.FD_findNextLeader, false);
				}
			}
			base.changeState(st);
			if (text != null)
			{
				this.SpSetPose(text, -1, null, false);
			}
			if (this.NextLeader != null && absorbManagerContainer != null)
			{
				absorbManagerContainer.countTortureItem(this.FD_findNextLeaderDef, false);
			}
			return this;
		}

		public bool findNextLeader(AbsorbManager _Abm)
		{
			if (this.NextLeader == null && _Abm.getPublishMover() is NelNPuppy)
			{
				NelNPuppy nelNPuppy = _Abm.getPublishMover() as NelNPuppy;
				if (nelNPuppy != this)
				{
					this.NextLeader = nelNPuppy.progressTorture(-1000);
				}
			}
			return false;
		}

		public bool findNextLeaderDef(AbsorbManager _Abm)
		{
			if (_Abm.getPublishMover() is NelNPuppy)
			{
				NelNPuppy nelNPuppy = _Abm.getPublishMover() as NelNPuppy;
				if (nelNPuppy == this.NextLeader)
				{
					this.NextLeader.progressTorture(0);
				}
				else
				{
					nelNPuppy.AbsorbLeader = this.NextLeader;
				}
			}
			return false;
		}

		protected override bool canCheckEnlargeState(NelEnemy.STATE state)
		{
			return base.canCheckEnlargeState(state) && !this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE) && !this.Nai.isFrontType(NAI.TYPE.PUNCH_0, PROG.ACTIVE);
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			if (A.IndexOf("torture_tentacle_1") == -1)
			{
				A.Add("torture_tentacle_1");
			}
			else if (A.IndexOf("torture_tentacle_2") == -1)
			{
				A.Add("torture_tentacle_2");
			}
			else
			{
				A.Add("torture_tentacle_" + (1 + X.xors(2)).ToString());
			}
			Aattr.Add(MGATTR.STAB);
			Aattr.Add(MGATTR.WIP);
			Aattr.Add(MGATTR.ABSORB_V);
			Aattr.Add(MGATTR.ABSORB);
			Aattr.Add(MGATTR.GRAB);
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Absorb = null, bool penetrate = false)
		{
			if (this.is_small)
			{
				return false;
			}
			AbsorbManager torturePublisher = Absorb.Con.getTorturePublisher();
			this.AbsorbLeader = null;
			if (torturePublisher != null)
			{
				M2Attackable publishMover = torturePublisher.getPublishMover();
				if (!(publishMover is NelNPuppy))
				{
					return false;
				}
				this.AbsorbLeader = (publishMover as NelNPuppy).progressTorture(1);
			}
			if (!base.initAbsorb(Atk, MvTarget, Absorb, penetrate))
			{
				return false;
			}
			base.killPtc("puppy_attack_1", true);
			Absorb.target_pose = null;
			Absorb.kirimomi_release = false;
			Absorb.release_from_publish_count = true;
			PrGachaItem gacha = Absorb.get_Gacha();
			if (X.XORSP() < 0.5f)
			{
				gacha.activate(PrGachaItem.TYPE.REP, 4, 63U);
			}
			else
			{
				gacha.activate(PrGachaItem.TYPE.SEQUENCE, 3, KEY.getRandomKeyBitsLRTB(2));
			}
			this.absorb_pos_fix_maxt = 11;
			Absorb.publish_float = true;
			return true;
		}

		public override bool runAbsorb()
		{
			if (this.Absorb == null)
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
				this.walk_time = 0f;
				this.Phy.remFoc(FOCTYPE.WALK, true);
				this.walk_st = -1;
				this.auto_absorb_lock_mover_hitting = false;
				if (this.AbsorbLeader == null)
				{
					this.t = 185f - (float)X.xors(4);
					this.progressTorture(0);
					this.Phy.addFoc(FOCTYPE.WALK, base.mpf_is_right * 0.12f, 0f, -1f, 0, 3, 30, -1, 0);
				}
				else
				{
					this.walk_st = -1 - X.xors(3);
					this.t = 180f - (float)X.xors(24);
					this.AbsorbLeader.Phy.addFoc(FOCTYPE.WALK, base.mpf_is_right * 0.11f, 0f, -1f, 0, 3, 24, -1, 0);
					this.SpSetPose("absorb_normal", -1, null, false);
					this.Phy.addLockGravity(NelEnemy.STATE.ABSORB, 0f, -1f);
					this.Anm.showToFront(X.XORSP() < 0.7f, false);
					this.setAim(CAim.get_aim2(0f, 0f, -this.Absorb.getPublishXD(), (float)(-1 + X.xors(3)), false), false);
				}
			}
			if (this.walk_st >= 0 != (this.AbsorbLeader == null))
			{
				return false;
			}
			if (this.AbsorbLeader != null)
			{
				AbsorbManager absorbManager = this.AbsorbLeader.getAbsorbManager();
				if (absorbManager == null)
				{
					return false;
				}
				this.Absorb.setAbmPose(absorbManager.abm_pose, true);
			}
			if (this.t >= 200f)
			{
				bool flag = true;
				X.MMX(0, (this.walk_st >= 0) ? this.walk_st : this.AbsorbLeader.walk_st, 2);
				if (!pr.is_alive && this.Absorb.uipicture_fade_key == "torture_tentacle_0")
				{
					this.Absorb.uipicture_fade_key = "torture_tentacle_1";
				}
				NelAttackInfo nelAttackInfo;
				if (this.walk_st >= 0)
				{
					if (!pr.is_alive && X.XORSP() < 0.05f)
					{
						return false;
					}
					nelAttackInfo = this.AtkAbsorbGrab;
					this.t = 130f - (float)X.xors(35);
					string text = "EP_other_tentacle_grab_0";
					if (this.walk_st >= 1 && X.XORSP() < 0.6f)
					{
						text = "EP_other_tentacle_grab_" + (X.xors(this.walk_st) + 1).ToString();
					}
					EpManager.callOtherAlert(text, 0f, 0f, 0.76f);
				}
				else if (this.walk_st == -1)
				{
					flag = false;
					nelAttackInfo = (pr.is_alive ? this.AtkAbsorb : this.AtkAbsorbDead);
					float num = this.walk_time - 1f;
					this.walk_time = num;
					if (num <= 0f)
					{
						this.walk_time = (float)(4 + X.xors(2));
						this.t = 70f - (float)X.xors(20);
						if (X.XORSP() < (pr.is_alive ? 0.2f : 0.4f))
						{
							this.walk_st = -1 - X.xors(3);
						}
					}
					else
					{
						this.t = 178f - (float)X.xors(5);
					}
				}
				else
				{
					nelAttackInfo = ((this.walk_st == -2) ? this.AtkAbsorbWip : this.AtkAbsorbStab);
					float num = this.walk_time - 1f;
					this.walk_time = num;
					if (num <= 0f)
					{
						this.walk_time = (float)X.xors(2);
						this.t = 200f - (float)((140 + X.xors(43)) / (pr.is_alive ? 1 : 2));
						if (X.XORSP() < (pr.is_alive ? 0.12f : 0.2f))
						{
							this.walk_st = -1 - X.xors(3);
						}
					}
					else
					{
						this.t = 200f - (float)((57 + X.xors(35)) / (pr.is_alive ? 1 : 2));
					}
					if (nelAttackInfo == this.AtkAbsorbWip)
					{
						EpManager.callOtherAlert("EP_other_tentacle_wip", 0f, 0f, 0.55f);
					}
				}
				float num2 = X.NIXP(4f, 6f) * (float)X.MPFXP();
				float num3 = X.NIXP(20f, 25f);
				if (X.XORSP() < 0.5f)
				{
					pr.TeCon.setQuakeSinH(num2, 44, num3, 0f, 0);
					this.TeCon.setQuakeSinH(num2 * 0.7f, 44, num3, 0f, 0);
				}
				if (!flag)
				{
					base.runAbsorb();
				}
				base.applyAbsorbDamageTo(pr, nelAttackInfo, true, false, flag, (this.walk_st >= 0) ? 1f : 0.7f, false, null, false, true);
				this.Anm.randomizeFrame(0.5f, 0.5f);
			}
			return true;
		}

		private NelNPuppy progressTorture(int prog = 1)
		{
			if (base.destructed || !this.is_alive || this.Absorb == null || !base.isAbsorbState())
			{
				return null;
			}
			if (prog == -1000)
			{
				return this;
			}
			if (this.walk_st < 0)
			{
				this.walk_st = this.Absorb.Con.countTortureItem((AbsorbManager _Abm) => _Abm.getPublishMover() is NelNPuppy, false) - 1;
				this.Absorb.publish_float = false;
				this.AbsorbLeader = null;
				this.Phy.remLockGravity(NelEnemy.STATE.ABSORB);
				this.Anm.showToFront(false, false);
				this.Absorb.changeTorturePose("torture_tentacle_0", false, true, -1, -1);
				this.Absorb.uipicture_fade_key = "torture_tentacle_0";
				this.auto_absorb_lock_mover_hitting = true;
			}
			else
			{
				this.walk_st = X.Mx(0, this.walk_st + prog);
			}
			if (this.walk_st >= 1 && X.XORSP() < 0.15f)
			{
				this.setAim((base.mpf_is_right > 0f) ? AIM.L : AIM.R, false);
			}
			int num = X.MMX(0, this.walk_st, 2);
			this.Absorb.changeTorturePose("torture_tentacle_" + num.ToString(), false, true, -1, -1);
			this.Absorb.setAbmPose((this.walk_st == 0) ? AbsorbManager.ABM_POSE.STAND : ((this.walk_st == 1) ? AbsorbManager.ABM_POSE.DOWN : AbsorbManager.ABM_POSE.CROUCH), false);
			this.Absorb.uipicture_fade_key = "torture_tentacle_" + num.ToString();
			M2Attackable targetMover = this.Absorb.getTargetMover();
			if (this.Absorb.uipicture_fade_key == "torture_tentacle_0" && targetMover != null && !targetMover.is_alive)
			{
				this.Absorb.uipicture_fade_key = "torture_tentacle_1";
			}
			return this;
		}

		public override bool SpPoseIs(string pose)
		{
			if (this.is_small)
			{
				return base.SpPoseIs(X.GetS<string, string>(NelNPuppy.OposeN2S, pose, pose));
			}
			return base.SpPoseIs(pose);
		}

		public override void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			base.SpSetPose(this.is_small ? X.GetS<string, string>(NelNPuppy.OposeN2S, nPose, nPose) : nPose, reset_anmf, fix_change, sprite_force_aim_set);
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			this.mpgurded = false;
			int num = base.applyDamage(Atk, force || !this.is_small);
			if (this.mpgurded)
			{
				float num2 = this.Phy.calcFocVelocityX(FOCTYPE.KNOCKBACK | FOCTYPE.DAMAGE, true);
				if (num2 == 0f)
				{
					num2 = (float)X.MPF(this.Nai.target_x < base.x);
				}
				this.Phy.addFoc(FOCTYPE.DAMAGE, (float)X.MPF(num2 > 0f) * 0.13f, 0f, -1f, 0, 5, 35, 20, 10);
				if (base.mp_ratio < 0.08f || (Atk != null && Atk.isPrBurst()))
				{
					base.PtcST("tentacle_guard_break", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.SpSetPose("damage_to_small", -1, null, false);
					this.Nai.delay = 0f;
					this.is_small = true;
					base.FixSizeW(28f, 28f);
				}
			}
			if (num > 0 || this.mpgurded)
			{
				this.mana_lock_until = this.Mp.floort + 150f;
			}
			return num;
		}

		public override int applyHpDamage(int val, ref int mpdmg, bool force, NelAttackInfo Atk)
		{
			if (!this.is_small)
			{
				if (Atk == null || EnemyAttr.applyHpDamageRatio(this, Atk) <= 0f)
				{
					val = 0;
					mpdmg = 0;
					return 0;
				}
				int num = X.Mx(0, val + (int)X.Mx(0f, (float)this.mp - (float)this.maxmp * 0.5f));
				if (num > 0)
				{
					this.mpgurded = true;
					val -= X.Mn(val, num);
					mpdmg += num;
				}
			}
			return base.applyHpDamage(val, force, Atk);
		}

		public override void addMpFromMana(M2Mana Mana, float val)
		{
			if (this.Mp != null)
			{
				base.addMpFromMana(Mana, ((this.Mp.floort < this.mana_lock_until) ? 0.25f : 1f) * val);
			}
		}

		public override float getMpDesireRatio(int add_mp)
		{
			return X.Scr(base.getMpDesireRatio(add_mp), (this.Mp.floort < this.mana_lock_until) ? 0.75f : 0f);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			if (this.is_small)
			{
				return base.applyHpDamageRatio(Atk);
			}
			if (Atk != null)
			{
				return EnemyAttr.applyHpDamageRatio(this, Atk);
			}
			return 0f;
		}

		public override int getMpDamageValue(NelAttackInfo Atk, int val)
		{
			float num = (this.is_small ? 2f : 0.25f);
			if (Atk != null)
			{
				num *= EnemyAttr.applyHpDamageRatio(this, Atk);
				if (num == 0f)
				{
					return 0;
				}
			}
			return X.IntC((float)base.getMpDamageValue(Atk, val) * num);
		}

		public override bool checkDamageStun(NelAttackInfo Atk, float level = 1f)
		{
			return this.is_small && base.checkDamageStun(Atk, 3f);
		}

		protected EnAttackInfo AtkSmallPunch = new EnAttackInfo
		{
			hpdmg0 = 4,
			split_mpdmg = 5,
			huttobi_ratio = -100f,
			burst_vx = 0.004f,
			knockback_len = 0.5f,
			Beto = BetoInfo.NormalS,
			parryable = true
		};

		protected NelAttackInfo AtkCapture = new NelAttackInfo
		{
			hpdmg0 = 4,
			absorb_replace_prob_both = 1f,
			split_mpdmg = 5,
			huttobi_ratio = -100f,
			Beto = BetoInfo.NormalS,
			parryable = true
		};

		protected NOD.TackleInfo TkiCapture = NOD.getTackle("puppy_capture");

		protected NOD.MpConsume McsCapture = NOD.getMpConsume("puppy_capture");

		protected EnAttackInfo AtkAbsorb = new EnAttackInfo
		{
			split_mpdmg = 1,
			attr = MGATTR.ABSORB,
			hit_ptcst_name = "player_absorbed_basic",
			Beto = BetoInfo.Absorbed
		};

		protected EnAttackInfo AtkAbsorbDead = new EnAttackInfo
		{
			split_mpdmg = 2,
			attr = MGATTR.ABSORB,
			hit_ptcst_name = "player_absorbed_basic",
			Beto = BetoInfo.Absorbed
		};

		protected EnAttackInfo AtkAbsorbGrab = new EnAttackInfo(0.02f, 0.025f)
		{
			split_mpdmg = 1,
			hpdmg0 = 4,
			attr = MGATTR.GRAB,
			Beto = BetoInfo.Grab,
			huttobi_ratio = -1000f
		};

		protected EnAttackInfo AtkAbsorbWip = new EnAttackInfo(0.04f, 0.08f)
		{
			split_mpdmg = 4,
			hpdmg0 = 4,
			attr = MGATTR.WIP,
			Beto = BetoInfo.Blood,
			huttobi_ratio = -1000f
		};

		protected EnAttackInfo AtkAbsorbStab = new EnAttackInfo(0.02f, 0.09f)
		{
			split_mpdmg = 0,
			hpdmg0 = 3,
			attr = MGATTR.STAB,
			Beto = BetoInfo.Blood,
			huttobi_ratio = -1000f
		};

		private const float size_default_w = 40f;

		private const float size_default_h = 90f;

		private const float size_sml_w = 45f;

		private const float size_sml_h = 30f;

		private const float size_vsml_w = 28f;

		private const float size_vsml_h = 28f;

		protected const float walk_speed = 0.23f;

		protected const float walk_speed_big = 0.17f;

		protected const float walk_speed_small = 0.21f;

		private const float tackle_speed = 0.26f;

		private const int tackle_time = 80;

		private const int tackle_end_delay = 70;

		private const int capture_attack_cramp = 3;

		private const int capture_cramp_start_fnum = 1;

		private const int capture_cramp_start_time = 3;

		private const int capture_cramp_loop_time = 20;

		private const float mp_guardable_threshold = 0.08f;

		private const float create_guard_mp_ratio = 0.375f;

		private const float transform_init_delay = 12f;

		private const float transform_complete_delay = 40f;

		private const float transform_after_wait = 50f;

		private float mana_lock_until;

		private const float DAMAGE_MANA_LOCK_TIME = 150f;

		private bool is_small;

		protected Flagger FlgSmall;

		private float linearcheck_lock_t;

		private M2SndInterval SndLoopWalk;

		private M2SndInterval SndLoopWalkF;

		private NASGroundClimber GClimb;

		private NelNPuppy AbsorbLeader;

		private static BDic<string, string> OposeN2S;

		private static BDic<string, string> OposeS2N;

		private const int PRI_MOVE_S = 1;

		private const int PRI_GAZE = 2;

		private const int PRI_MOVE = 5;

		private const int PRI_ESCAPE = 40;

		private const int PRI_PUNCH = 100;

		private const float mana_search_x = 2.5f;

		private NelNPuppy NextLeader;

		private Func<AbsorbManager, bool> FD_findNextLeader;

		private Func<AbsorbManager, bool> FD_findNextLeaderDef;

		private bool mpgurded;
	}
}
