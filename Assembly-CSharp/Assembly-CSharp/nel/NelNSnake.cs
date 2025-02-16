using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNSnake : NelEnemy
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 128f;
			NOD.BasicData basicData;
			if (this.id == ENEMYID.SNAKE_TUTORIAL)
			{
				basicData = NOD.getBasicData("SNAKE_TUTORIAL");
			}
			else
			{
				this.id = ENEMYID.SNAKE_0;
				basicData = NOD.getBasicData("SNAKE_0");
			}
			this.SizeW(35f, 104f, ALIGN.CENTER, ALIGNY.MIDDLE);
			base.appear(_Mp, basicData);
			this.Anm.checkframe_on_drawing = false;
			this.cannot_move = true;
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 128f;
			this.Nai.attackable_length_top = -128f;
			this.Nai.attackable_length_bottom = 128f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.GClimb = new NASGroundClimber(this, 0f);
			this.GClimb.addChangedFn(new NASGroundClimber.FnClimbEvent(this.fnChangedBcc));
			this.GClimb.alloc_jump_air = false;
			this.absorb_weight = 2;
			this.FD_fnFineAbsorbAttack = new EnemyAnimator.FnFineFrame(this.fnFineAbsorbAttack);
		}

		public override void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			base.quitSummonAndAppear(clearlock_on_summon);
			this.softfall_t = 90f;
		}

		public override void runPre()
		{
			if (this.Mp == null)
			{
				return;
			}
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (this.softfall_t_ > 0f)
			{
				this.softfall_t_ -= this.TS;
				if (this.softfall_t_ <= 0f)
				{
					this.softfall_t_ = 0f;
					this.Phy.quitSoftFall(20f);
				}
			}
		}

		public float softfall_t
		{
			get
			{
				return this.softfall_t_;
			}
			set
			{
				if (value == this.softfall_t_)
				{
					return;
				}
				if (this.softfall_t_ <= 0f && value > 0f)
				{
					this.Phy.initSoftFall(0.45f, 0f);
				}
				else if (this.softfall_t_ > 0f && value <= 0f)
				{
					this.Phy.quitSoftFall(20f);
				}
				this.softfall_t_ = value;
			}
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai))
			{
				return true;
			}
			if (!base.Useable(this.TkiGrab, X.Mx(1f, Nai.RANtk(4313) * 2.3f)) && Nai.AddTicketSearchAndGetManaWeed(200, 15f, -15f, 15f, 15f, -15f, 15f, true) != null)
			{
				return true;
			}
			if (Nai.HasF(NAI.FLAG.ABSORB_FINISHED, true))
			{
				return Nai.AddTicketB(NAI.TYPE.WAIT, 200, true);
			}
			if (Nai.HasF(NAI.FLAG.STUN_FINISHED, true) && Nai.fnBasicPunch(Nai, 200, 100f, 0f, 0f, 0f, 7111, false))
			{
				return true;
			}
			if (!Nai.isPrAlive())
			{
				bool flag = Nai.HasF(NAI.FLAG.BOTHERED, true);
				if (Nai.target_slen_n < 3f && !flag)
				{
					if (Nai.fnBasicPunch(Nai, 200, 10f, 60f, 0f, 0f, 8841, false))
					{
						return true;
					}
				}
				else if (Nai.fnBasicPunch(Nai, 200, (float)(flag ? 80 : 50), 0f, 0f, 0f, 8841, false))
				{
					return true;
				}
				return Nai.AddTicketB(NAI.TYPE.WAIT, 200, true);
			}
			if (Nai.isPrSpecialAttacking())
			{
				if (Nai.fnBasicPunch(Nai, 200, 15f, 85f, 0f, 0f, 7111, false))
				{
					return true;
				}
			}
			else if (Nai.isPrMagicExploded(1f))
			{
				if (Nai.fnBasicPunch(Nai, 200, 4f, (float)(Nai.here_dangerous ? 90 : 30), 0f, 0f, 7111, false))
				{
					return true;
				}
			}
			else if (Nai.here_dangerous && Nai.canNoticeThat(2.4f))
			{
				if (Nai.fnBasicPunch(Nai, 200, 100f, 0f, 0f, 0f, 8841, false))
				{
					return true;
				}
			}
			else
			{
				if (base.AimPr != null && Nai.target_slen_n < 3f && X.Mx(X.Abs(Nai.target_lastfoot_bottom - base.AimPr.sizey * 1.2f - base.mtop), 0f) < 3.5f && Nai.target_lastfoot_bottom > base.mtop - 1f && Nai.target_lastfoot_bottom < base.mbottom + 0.1f && Nai.fnBasicPunch(Nai, 200, 0f, 0f, 80f, 0f, 8841, false))
				{
					return true;
				}
				if (Nai.fnBasicPunch(Nai, 200, 25f, 0f, 0f, 0f, 8841, false))
				{
					return true;
				}
			}
			return Nai.fnBasicMove(Nai);
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.GUARD_0)
			{
				switch (type)
				{
				case NAI.TYPE.WALK:
					break;
				case NAI.TYPE.WALK_TO_WEED:
				case NAI.TYPE.PUNCH_2:
					goto IL_00E9;
				case NAI.TYPE.PUNCH:
				case NAI.TYPE.PUNCH_WEED:
					return this.runDiggingAttack(Tk.initProgress(this), Tk);
				case NAI.TYPE.PUNCH_0:
				case NAI.TYPE.PUNCH_1:
					return this.runReflectionAttack(Tk.initProgress(this), Tk);
				default:
					if (type != NAI.TYPE.GUARD_0)
					{
						goto IL_00E9;
					}
					return this.runEventAbsorb0(Tk.initProgress(this), Tk);
				}
			}
			else
			{
				if (type == NAI.TYPE.GUARD_1)
				{
					return this.runEventAbsorb1(Tk.initProgress(this), Tk);
				}
				if (type - NAI.TYPE.GAZE > 1)
				{
					goto IL_00E9;
				}
			}
			this.SpSetPose(base.SpPoseIs("fall", "appear") ? "land" : "stand", -1, null, false);
			base.AimToLr((X.xors(2) == 0) ? 0 : 2);
			Tk.after_delay = 30f + this.Nai.RANtk(840) * 20f;
			return false;
			IL_00E9:
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.PUNCH)
				{
					base.disappearing = (base.throw_ray = false);
					this.Phy.remLockWallHitting(this);
					this.Phy.remLockGravity(this);
					this.Phy.remLockMoverHitting(HITLOCK.SPECIAL_ATTACK);
					this.GClimb.quitClimbWalk();
					base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					this.fineFootType();
					this.Anm.rotationR = 0f;
					this.Anm.timescale = 1f;
					this.cannot_move = true;
				}
				if (Tk.type == NAI.TYPE.GUARD_1)
				{
					base.remF((NelEnemy.FLAG)2097280);
				}
			}
			this.MgRelectAttack0 = (this.MgRelectAttack1 = null);
			this.EatBombMg = null;
			if (this.SndLoopWalk != null)
			{
				this.SndLoopWalk.destruct();
			}
			this.SndLoopWalk = null;
		}

		protected override bool initDeathEffect()
		{
			if (this.SndLoopAbsorb != null)
			{
				this.SndLoopAbsorb.destruct();
				this.SndLoopAbsorb = null;
			}
			return base.initDeathEffect();
		}

		private bool runDiggingAttack(bool init_flag, NaTicket Tk)
		{
			bool flag = Tk.type == NAI.TYPE.PUNCH_WEED;
			if (init_flag)
			{
				this.t = 0f;
				if (flag && Tk.DepBCC == null)
				{
					return false;
				}
				this.turn_t = 0f;
				this.walk_st = 0;
				this.eff_t = 0f;
				this.SpSetPose((this.ixia_hold_level > 0) ? "ixia_hold2dive" : (this.SpPoseIs("stun") ? "damage_land2dive" : "dive"), -1, null, false);
				base.PtcVar("by", (double)base.mbottom).PtcST("snake_dig_start", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.EatBombMg = null;
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.Nai.RemF(NAI.FLAG.ABSORB_FINISHED);
				this.cannot_move = true;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.Anm.cframe >= 7 && this.walk_st == 0)
				{
					base.PtcVar("by", (double)base.mbottom).PtcST("snake_dig_start_2", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.walk_st = 1;
				}
				if (this.t >= 31f)
				{
					base.throw_ray = true;
				}
				if (!base.disappearing)
				{
					if (this.Anm.poseIs("dive_2", false) && this.Anm.isAnimEnd())
					{
						base.disappearing = true;
					}
				}
				else if (Tk.Progress(ref this.t, 90, true))
				{
					this.walk_st = 0;
					this.eff_t = 0f;
					this.turn_t = -1f;
					this.walk_time = (flag ? 20f : this.Nai.NIRANtk(40f, 80f, 2911));
					this.digging_t = -this.Nai.NIRANtk(250f, 340f, 1753);
					this.Phy.addLockWallHitting(this, -1f);
					this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, -1f);
					this.softfall_t_ = 0f;
					base.disappearing = (base.throw_ray = true);
					this.cannot_move = false;
					this.fineFootType();
					base.AimToPlayer();
					this.GClimb.initClimbWalk(15U);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (base.AimPr == null)
				{
					Tk.Progress(ref this.t, 0, true);
					this.walk_st = 0;
				}
				else if (this.ixia_hold_level == 0)
				{
					float num = (flag ? 1.6f : 1f);
					if (!base.AimPr.hasFoot())
					{
						this.Nai.fine_target_pos_lock = 4f;
					}
					if (this.walk_st == 2 && base.AimPr.hasFoot())
					{
						this.walk_st = 0;
						this.turn_t = 0f;
					}
					if (this.walk_st == 0)
					{
						if (this.turn_t <= 0f)
						{
							this.createSndWalk();
							M2BlockColliderContainer.BCCLine bccline;
							float num2;
							if (flag)
							{
								float depx = Tk.depx;
								float depy = Tk.depy;
								bccline = Tk.DepBCC;
								num2 = Tk.depx;
							}
							else
							{
								bccline = this.GClimb.targetBCC;
								num2 = this.Nai.target_x;
							}
							if (bccline != null)
							{
								int num3 = this.GClimb.FixAim(num2, bccline, false, 1f);
								if (num3 < 0)
								{
									this.Phy.addLockGravity(this, 0f, -1f);
									this.walk_st = -1;
								}
								else if (this.GClimb.getPreBcc() != null && this.turn_t >= 0f)
								{
									if (this.aim != (AIM)num3)
									{
										this.turn_t = 1f;
									}
									else if (this.GClimb.initClimbWalk(15U))
									{
										this.walk_st = 1;
									}
								}
								else
								{
									this.setAim((AIM)num3, false);
									if (this.GClimb.initClimbWalk(15U))
									{
										this.walk_st = 1;
									}
								}
							}
						}
						else
						{
							this.turn_t += this.TS;
							if (this.turn_t >= 20f)
							{
								this.GClimb.Turn();
								if (this.GClimb.initClimbWalk(15U))
								{
									this.walk_st = 1;
									this.turn_t = -21f;
								}
							}
						}
					}
					if (this.turn_t < -1f)
					{
						this.turn_t = X.VALWALK(this.turn_t, -1f, this.TS);
					}
					if (this.walk_st == 1 && !flag && !base.AimPr.hasFoot())
					{
						this.walk_st = 2;
					}
					bool flag2 = false;
					float num4 = (float)((this.digging_t < -40f) ? 1 : 0);
					this.eff_t -= this.TS;
					if (this.walk_st < 0)
					{
						this.FootD.footable_bits = 8U;
						Vector2 vector = Vector2.zero;
						if (flag)
						{
							vector.Set(Tk.depx, Tk.depy - 0.125f);
						}
						else
						{
							M2BlockColliderContainer.BCCLine targetBCC = this.GClimb.targetBCC;
							if (targetBCC != null)
							{
								vector = targetBCC.linePosition(this.Nai.target_x, this.Nai.target_y - 0.125f, 0f, 0f);
							}
						}
						if (vector.x != 0f)
						{
							if (X.Abs(base.x - vector.x) < 1.2f && X.BTW(vector.y - 0.08f, base.mbottom, vector.y + 0.0625f))
							{
								this.walk_st = 0;
								this.turn_t = -2f;
								this.walk_time = (flag ? 20f : this.Nai.NIRANtk(14f, 20f, 2911));
								this.t = 0f;
								this.Phy.remLockGravity(this);
								this.Phy.killSpeedForce(true, true, true, false, false);
							}
							else
							{
								this.Phy.addFoc(FOCTYPE.WALK, X.VALWALK(base.x, vector.x, 0.118f * num) - base.x, X.VALWALK(base.mbottom, vector.y, 0.118f * num) - base.mbottom, -1f, -1, 1, 0, -1, 0);
							}
						}
						else if (flag)
						{
							flag2 = true;
						}
					}
					else if (this.GClimb.getPreBcc() != null)
					{
						if (!this.GClimb.progressClimbWalk(0.09f * num * X.ZLINE(this.t, 40f) * (1f - X.ZLINE(X.Abs(this.turn_t) - (float)((this.turn_t < 0f) ? 1 : 0), 20f))) || this.GClimb.getPreBcc() == null)
						{
							flag2 = true;
						}
						else if (base.hasFoot())
						{
							M2BlockColliderContainer.BCCLine preBcc = this.GClimb.getPreBcc();
							AIM foot_aim = preBcc.foot_aim;
							float num5 = 1f;
							float num6 = 1f;
							Vector2 vector2;
							if (flag)
							{
								vector2 = new Vector2(Tk.depx, Tk.depy);
							}
							else
							{
								vector2 = preBcc.linePosition(this.Nai.target_x, this.Nai.target_y, 0f, 0f);
								if (preBcc._yd != 0)
								{
									num5 *= 3f;
								}
								else
								{
									num6 *= 3f;
								}
							}
							byte b = 0;
							if (X.LENGTHXYS(vector2.x * num5, vector2.y * num6, (base.x + (float)CAim._XD(foot_aim, 1) * this.sizex) * num5, (base.y - (float)CAim._YD(foot_aim, 1) * this.sizey) * num6) < 2.4f)
							{
								if (b == 0)
								{
									b = ((!this.Mp.isDangerousCfg((int)(base.x + (float)CAim._XD(foot_aim, 1) * (this.sizex * 0.25f)), (int)(base.y - (float)CAim._YD(foot_aim, 1) * (this.sizey + 0.25f)))) ? 1 : 2);
								}
								if (b == 1)
								{
									this.walk_time -= this.TS;
								}
								else
								{
									this.walk_time -= this.TS * 0.12f;
								}
							}
							if (num4 == 0f)
							{
								if (b == 0)
								{
									b = ((!this.Mp.isDangerousCfg((int)(base.x + (float)CAim._XD(foot_aim, 1) * (this.sizex * 0.25f)), (int)(base.y - (float)CAim._YD(foot_aim, 1) * (this.sizey + 0.25f)))) ? 1 : 2);
								}
								num4 = ((b == 1) ? 1f : 0.33f);
							}
						}
						else
						{
							num4 = 0.33f;
						}
					}
					else
					{
						this.walk_time -= this.TS;
					}
					if (num4 <= 0f)
					{
						num4 = 0.04f;
					}
					this.digging_t += num4 * this.TS * (float)((this.digging_t < -40f && !flag && !base.Useable(this.TkiGrab, 1f)) ? 2 : 1);
					if (this.digging_t >= 0f || this.walk_time <= 0f || flag2)
					{
						Tk.Progress(ref this.t, 0, true);
						this.walk_st = 0;
					}
					if (this.eff_t <= 0f)
					{
						M2Mover baseMover = base.M2D.Cam.getBaseMover();
						this.eff_t = 10f;
						if (baseMover != null)
						{
							M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
							AIM aim = ((footBCC != null) ? footBCC.foot_aim : AIM.B);
							base.PtcVar("cx", (double)(base.x + this.sizex * (float)CAim._XD(aim, 1))).PtcVar("cy", (double)(base.y - this.sizey * (float)CAim._YD(aim, 1)));
							if (footBCC != null)
							{
								base.PtcVar("agR", (double)(this.Anm.rotationR + 1.5707964f));
								base.PtcST("snake_dig_in_ground", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							}
							else
							{
								base.PtcVar("agR", (double)this.Mp.GAR(0f, 0f, -base.vx, -base.vy)).PtcST("snake_dig_in_air", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							}
							if (this.SndLoopWalk != null)
							{
								this.SndLoopWalk.setVolManual(0.125f + 0.875f * X.ZPOW(1f - X.ZLINE(X.Abs(baseMover.x - base.x) + 0.25f * X.Abs(baseMover.mbottom - base.mbottom), 8f)), true);
							}
						}
						if (this.walk_st == 1)
						{
							M2BlockColliderContainer.BCCLine bccline2;
							if (flag)
							{
								bccline2 = Tk.DepBCC;
							}
							else
							{
								bccline2 = this.GClimb.targetBCC;
							}
							if (bccline2 != null && bccline2 == this.FootD.get_FootBCC())
							{
								this.walk_st = 0;
							}
						}
					}
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.walk_st == 0)
				{
					this.walk_st = 1;
					base.PtcVar("by", (double)base.mbottom).PtcST("snake_dig_getout_prepare", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				if (Tk.Progress(ref this.t, 13, true))
				{
					this.SpSetPose("dig", -1, null, false);
					base.PtcVar("by", (double)base.mbottom).PtcST("snake_dig_getout", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					base.disappearing = (base.throw_ray = false);
					this.FootD.initJump(false, true, false);
					this.fineFootType();
					this.Phy.remLockWallHitting(this);
					this.Phy.remLockGravity(this);
					this.Phy.remLockMoverHitting(HITLOCK.SPECIAL_ATTACK);
					this.softfall_t = 190f;
					this.Phy.addLockGravityFrame(100);
					float num7 = 1.5707964f;
					bool flag3 = true;
					M2BlockColliderContainer.BCCLine preBcc2 = this.GClimb.getPreBcc();
					if (preBcc2 != null)
					{
						flag3 = preBcc2._yd == -1;
						num7 = preBcc2.housenagR;
					}
					this.Phy.addFoc(FOCTYPE.JUMP, 0.07f * X.Cos(num7), -0.07f * X.Sin(num7) - (flag3 ? 0.02f : 0.06f), -1f, 0, 8, 30, -1, 0);
					float num8 = base.x - this.sizex * X.Cos(num7);
					float num9 = base.y + this.sizey * X.Sin(num7);
					this.EatBombMg = base.nM2D.MGC.FindMg(num8, num9, this.sizey, num7, this.sizey * 0.5f, this.sizex * 2.3f, (MagicItem Mg, M2MagicCaster Caster) => Mg.Other is IMgBombListener && (Mg.Other as IMgBombListener).isEatableBomb(Mg, Caster, true), this);
					this.GClimb.quitClimbWalk();
					if (this.EatBombMg != null)
					{
						this.Anm.timescale = 0.5f;
						Tk.prog = PROG.PROG3;
					}
					else
					{
						base.tackleInit(this.AtkAbsorbGrab, (Tk.type == NAI.TYPE.PUNCH_WEED) ? this.TkiGrabManaWeed : this.TkiGrab).dy = -1.1f * this.Anm.scaleY;
					}
				}
				else if (base.AimPr is PR && base.isCoveringMv(base.AimPr, -this.sizex * 0.3f, -this.sizey * 0.3f))
				{
					(base.AimPr as PR).addEnemySink(10f, false, 0f);
				}
			}
			if (this.EatBombMg != null)
			{
				float num10 = this.Anm.rotationR + 1.5707964f;
				this.EatBombMg.sx = base.x + this.sizex * X.Cos(num10) * 0.68f;
				this.EatBombMg.sy = base.y - this.sizey * X.Sin(num10) * 0.68f;
				this.EatBombMg.sa = 1.5707964f;
			}
			if (Tk.prog == PROG.PROG2 || Tk.prog == PROG.PROG3)
			{
				if (this.t >= 16f)
				{
					this.can_hold_tackle = false;
					base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					this.Anm.rotationR = X.VALWALKANGLER(this.Anm.rotationR, 0f, 0.12566371f);
				}
				if (this.t >= (float)((Tk.prog == PROG.PROG2) ? 40 : 80))
				{
					Tk.after_delay = this.Nai.NIRANtk(146f, 176f, 1339) - this.t;
					this.Anm.timescale = 1f;
					return false;
				}
			}
			return true;
		}

		private bool fnChangedBcc(M2BlockColliderContainer.BCCLine Bcc)
		{
			if (this.walk_st == 1)
			{
				this.walk_st = 0;
			}
			return true;
		}

		private void createSndWalk()
		{
			if (this.SndLoopWalk == null)
			{
				this.SndLoopWalk = this.Mp.M2D.Snd.createInterval(this.snd_key, "snake_in_ground", 110f, this, 0f, 128);
			}
		}

		private bool runReflectionAttack(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("guard_1", -1, null, false);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 18, true))
			{
				this.t = 10f;
				this.walk_st = (5 + X.xors(3)) * ((Tk.type == NAI.TYPE.PUNCH_1) ? 2 : 1);
				this.SpSetPose("guard_2", -1, null, false);
				this.MgRelectAttack0 = base.tackleInit(this.AtkReflect, this.TackleReflect0);
				this.reflect_attack_id0 = this.MgRelectAttack0.id;
				this.MgRelectAttack1 = base.tackleInit(this.AtkReflect, this.TackleReflect1);
				this.reflect_attack_id1 = this.MgRelectAttack1.id;
				this.MgRelectAttack1.Ray.SyncHitLock(this.MgRelectAttack0.Ray);
			}
			if (Tk.prog == PROG.PROG0 && this.t >= 10f)
			{
				this.t = 0f;
				bool flag = !base.Useable(this.TackleReflect1, 1f);
				if (flag)
				{
					this.walk_st = X.Mn(this.walk_st, 2);
				}
				if (this.MgRelectAttack0 == null || !this.MgRelectAttack0.isActive(this, this.reflect_attack_id0) || this.MgRelectAttack1 == null || !this.MgRelectAttack1.isActive(this, this.reflect_attack_id1))
				{
					this.walk_st = -1000;
				}
				if (this.Nai.HasF(NAI.FLAG.BOTHERED, false))
				{
					this.walk_st = -1000;
				}
				if (this.walk_st > -1000)
				{
					if (flag || !this.Nai.here_dangerous)
					{
						int num = this.walk_st - 1;
						this.walk_st = num;
						if (num <= 0)
						{
							goto IL_01C2;
						}
					}
					base.PtcST("snake_guard_attack", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					return true;
				}
				IL_01C2:
				this.SpSetPose("guard_3", -1, null, false);
				Tk.after_delay = 80f;
				this.can_hold_tackle = false;
				this.Nai.addTypeLock(NAI.TYPE.PUNCH_1, 120f);
				if (flag)
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 80f);
				}
				return false;
			}
			return true;
		}

		private bool isPenetrate(NelAttackInfo Atk)
		{
			return this.state == NelEnemy.STATE.STUN || this.isDamagingOrKo() || (this.EatBombMg != null && Atk == this.EatBombMg.Atk0) || (Atk != null && Atk.huttobi_ratio >= 5f);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return base.applyHpDamageRatio(Atk) * ((this.EatBombMg != null && Atk == this.EatBombMg.Atk0) ? 3f : (this.isPenetrate(Atk as NelAttackInfo) ? 1.5f : 0.25f));
		}

		public override bool hasSuperArmor(NelAttackInfo Atk)
		{
			return Atk == null || this.EatBombMg == null || Atk != this.EatBombMg.Atk0;
		}

		public override int getMpDamageValue(NelAttackInfo Atk, int val)
		{
			return (int)((float)base.getMpDamageValue(Atk, val) * (base.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD) ? 0.25f : 1f));
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			bool cannot_move = this.cannot_move;
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			this.cannot_move = !base.isDamagingOrKoOrStun();
			if (!this.isPenetrate(Atk))
			{
				base.addF((NelEnemy.FLAG)327680);
				if (this.cannot_move && Atk != null && Atk.isPlayerShotgun())
				{
					this.Nai.AddF(NAI.FLAG.BOTHERED, 240f);
				}
			}
			else if (this.EatBombMg != null && Atk == this.EatBombMg.Atk0)
			{
				this.NoDamage.Clear();
				base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
				this.Phy.clearLockGravity();
				this.Phy.killSpeedForce(true, true, true, false, false);
				this.softfall_t = 0f;
				this.Anm.timescale = 1f;
				this.Ser.Add(SER.EATEN, (int)this.stun_time, 99, false);
				base.PtcST("bomb_hit_critical", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.playSndPos("snake_damage_0", 1);
			}
			else
			{
				this.playSndPos("snake_damage_1", 1);
			}
			int num = base.applyDamage(Atk, force);
			this.cannot_move = cannot_move;
			return num;
		}

		public override bool fineFootType()
		{
			if (!base.fineFootType())
			{
				return false;
			}
			if (this.Anm.poseIs("damage_1", "damage", "stun"))
			{
				this.FootD.footstamp_type = FOOTSTAMP.BIG;
			}
			return true;
		}

		public override bool runDamageSmall()
		{
			if (this.t <= 0f)
			{
				this.fineFootType();
				this.Anm.timescale = 1f;
				this.Phy.addFoc(FOCTYPE.JUMP, 0f, -0.01f, -1f, 0, 2, 13, -1, 0);
				this.softfall_t = 3f;
				this.EatBombMg = null;
			}
			if (!base.runDamageSmall() && base.hasFoot())
			{
				this.Anm.rotationR = 0f;
				if (!this.Anm.poseIs("damage_1", false))
				{
					base.FixSizeW(133f, 44f);
					this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._RELEASE, 0f, -0.11f, -1f, -1, 1, 0, -1, 0);
					this.FootD.initJump(false, false, false);
					this.softfall_t = 0f;
					this.SpSetPose("damage_1", -1, null, false);
					if (this.Ser.has(SER.EATEN))
					{
						this.changeState(NelEnemy.STATE.STUN);
						return true;
					}
					return false;
				}
			}
			return true;
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if ((Ray.hittype & HITTYPE.GUARD_IGNORE) != HITTYPE.NONE && !this.Ser.has(SER.EATEN))
			{
				return RAYHIT.NONE;
			}
			if (this.ixia_hold_level != 0 && this.ixia_hold_level != 2)
			{
				return RAYHIT.NONE;
			}
			return base.can_hit(Ray);
		}

		public override void changeStateToDie()
		{
			if (this.ixia_hold_level > 0)
			{
				this.hp = X.Mx(1, this.hp);
				return;
			}
			base.changeStateToDie();
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			NelEnemy.STATE state = this.state;
			if (state == NelEnemy.STATE.STUN)
			{
				base.FixSizeW(35f, 104f);
			}
			this.Anm.rotationR = 0f;
			base.changeState(st);
			if (st == NelEnemy.STATE.STUN)
			{
				base.FixSizeW(133f, 44f);
				this.fineFootType();
			}
			if (base.isAbsorbState(state) && !base.isAbsorbState(st))
			{
				this.Anm.fnFineFrame = null;
				base.FixSizeW(35f, 104f);
				if (this.SndLoopAbsorb != null)
				{
					this.SndLoopAbsorb.destruct();
					this.SndLoopAbsorb = null;
				}
			}
			bool flag = base.isDamagingOrKoOrStun(state);
			bool flag2 = base.isDamagingOrKoOrStun(st);
			if (flag != flag2)
			{
				this.EatBombMg = null;
				if (flag2)
				{
					base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					this.cannot_move = false;
				}
				else
				{
					base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					this.cannot_move = true;
				}
			}
			return this;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			A.Add("torture_snake_0");
			Aattr.Add(MGATTR.ABSORB);
			Aattr.Add(MGATTR.NORMAL);
			if (!X.SENSITIVE)
			{
				A.Add("torture_snake_1");
			}
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (Abm.Con.use_torture || !base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			Abm.kirimomi_release = true;
			Abm.release_from_publish_count = true;
			Abm.get_Gacha().activate(PrGachaItem.TYPE.SEQUENCE, 5, KEY.getRandomKeyBitsLRTB(2)).SoloPositionPixel = new Vector3(65f, -35f, 0f);
			this.softfall_t = X.Mn(this.softfall_t_, 40f);
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.t <= 0f)
			{
				if (this.Absorb.Con.isTortureByOther(this))
				{
					return false;
				}
				this.Absorb.uipicture_fade_key = "torture_snake_0";
				this.Absorb.changeTorturePose("torture_snake_0", false, false, this.Anm.cframe, -1);
				this.Anm.timescale = 1f;
				this.walk_st = 0;
				this.walk_time = 100f;
				this.Anm.fnFineFrame = this.FD_fnFineAbsorbAttack;
				base.FixSizeW(35f, 188f);
				if (this.SndLoopAbsorb != null)
				{
					this.SndLoopAbsorb.destruct();
				}
				this.SndLoopAbsorb = this.Mp.M2D.Snd.createInterval(this.snd_key, "snake_grab_tentacle", 720f, this, 0f, 128);
			}
			if (this.walk_time < 0f)
			{
				this.walk_time += this.TS;
				if (this.walk_time >= 0f)
				{
					return false;
				}
			}
			else if (this.walk_st == 1)
			{
				this.walk_st = 2;
				this.walk_time += 1f;
				base.PtcVar("cy", (double)pr.y).PtcST("ground_pound_attacked", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				if (!pr.EpCon.isOrgasmInitTime())
				{
					if (this.Anm.timescale != 1f)
					{
						if ((float)pr.ep < 700f && X.XORSP() < 0.08f)
						{
							this.Anm.timescale = 1f;
						}
					}
					else if ((float)pr.ep >= 650f && X.XORSP() < 0.03f)
					{
						this.Anm.timescale = 1.44f;
					}
				}
				string target_pose = this.Absorb.target_pose;
				if (target_pose != null)
				{
					if (!(target_pose == "torture_snake_0"))
					{
						if (target_pose == "torture_snake_2")
						{
							if (this.walk_time >= 92f && X.XORSP() < 0.004f)
							{
								this.walk_time = -14f;
							}
							if (X.SENSITIVE || (this.walk_time >= 133f && (float)pr.ep < 700f && X.XORSP() < 0.013f))
							{
								this.Absorb.uipicture_fade_key = "torture_snake_0";
								this.Absorb.changeTorturePose("torture_snake_0");
								base.PtcST("snake_torture_to_2nd_phase", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
								pr.PtcST("absorb_atk_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
								this.walk_time = 0f;
							}
						}
					}
					else if (!X.SENSITIVE && this.walk_time >= 114f && X.XORSP() < 0.12f)
					{
						this.Absorb.uipicture_fade_key = "torture_snake_1";
						this.Absorb.changeTorturePose("torture_snake_2");
						base.PtcST("snake_torture_to_2nd_phase", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.walk_time = 0f;
					}
				}
				if (this.Absorb.target_pose == "torture_snake_0")
				{
					base.applyAbsorbDamageTo(pr, this.AtkAbsorbMain, true, X.XORSP() < 0.26f, false, 0f, false, null, false);
					this.playSndPos("absorb_guchu", 1);
				}
				else
				{
					base.applyAbsorbDamageTo(pr, this.AtkAbsorb2, true, X.XORSP() < 0.7f, false, 0f, false, null, false);
					this.playSndPos("absorb_kiss_2", 1);
				}
			}
			return true;
		}

		private void fnFineAbsorbAttack(EnemyFrameDataBasic nF, PxlFrame F)
		{
			if (TX.isStart(F.name, "attack", 0))
			{
				if (this.walk_st == 0)
				{
					this.walk_st = 1;
					return;
				}
			}
			else if (this.walk_st >= 2)
			{
				this.walk_st = 0;
			}
		}

		public void initIxiaHold()
		{
			if (this.state == NelEnemy.STATE.SUMMONED)
			{
				this.quitSummonAndAppear(true);
			}
			this.ixia_hold_level = 1;
			float footableY = this.Mp.getFootableY(base.x, (int)base.y, 12, false, -1f, false, true, true, 0f);
			this.setTo(base.x, footableY - this.sizey);
			base.addF((NelEnemy.FLAG)2097344);
			if (this.state != NelEnemy.STATE.STAND)
			{
				this.changeState(NelEnemy.STATE.STAND);
			}
			this.Nai.AimPr = this.Mp.Pr;
			this.Nai.clearTicket(-1, true);
			this.Nai.AddTicket(NAI.TYPE.GUARD_0, 200, true);
			this.Anm.showToFront(false, false);
			this.SpSetPose("ixia_hold", -1, null, false);
		}

		public void initIxiaWaitingNoelAttack()
		{
			this.ixia_hold_level = 2;
			this.hp = this.maxhp;
			base.addF(NelEnemy.FLAG.FINE_HPMP_BAR);
			base.remF(NelEnemy.FLAG.EVENT_SHOW);
		}

		public void initIxiaHitEvent()
		{
			base.getAnimator().timescale = 1.125f;
			this.ixia_hold_level = 3;
		}

		public void initIxiaPreThrow()
		{
			this.Nai.AddTicket(NAI.TYPE.GUARD_1, 201, true);
		}

		private bool runEventAbsorb0(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.setAim(AIM.R, false);
			}
			return true;
		}

		private bool runEventAbsorb1(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.playSndPos("en_mouth_open", 1);
				this.SpSetPose("ixia_hold1", -1, null, false);
				this.setAim(AIM.R, false);
			}
			if (this.Anm.isAnimEnd() && this.ixia_hold_level == 3)
			{
				this.ixia_hold_level = 4;
			}
			if (this.Anm.isAnimEnd() && this.ixia_hold_level == 5)
			{
				this.Nai.AddTicket(NAI.TYPE.PUNCH, 202, true);
			}
			return true;
		}

		public void quitIxiaHold()
		{
			this.ixia_hold_level = 0;
			if (!this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE))
			{
				this.Nai.AddTicket(NAI.TYPE.PUNCH, 202, true);
			}
		}

		public override bool isTortureUsing()
		{
			return base.isTortureUsing() || (0 < this.ixia_hold_level && this.ixia_hold_level < 5);
		}

		public bool initIxiaAlreadyThrown(bool progress = false)
		{
			if (this.ixia_hold_level >= 4 && progress)
			{
				if (this.ixia_hold_level < 5)
				{
					this.ixia_hold_level = 5;
					this.SpSetPose("ixia_hold2", -1, null, false);
				}
				return true;
			}
			return false;
		}

		public bool isDigging()
		{
			return this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE);
		}

		public override bool showFlashEatenEffect(bool for_effect = false)
		{
			return !for_effect && this.state != NelEnemy.STATE.STUN;
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle && !this.Ser.has(SER.TIRED);
		}

		protected NelAttackInfo AtkAbsorbGrab = new NelAttackInfo
		{
			split_mpdmg = 1,
			hpdmg0 = 1,
			Beto = BetoInfo.Grab,
			huttobi_ratio = -1000f,
			absorb_replace_prob_both = 100f,
			parryable = false
		};

		protected NelAttackInfo AtkReflect = new NelAttackInfo
		{
			split_mpdmg = 5,
			hpdmg0 = 11,
			Beto = BetoInfo.Normal,
			huttobi_ratio = 0.5f,
			burst_vx = 0.08f,
			knockback_len = 0.7f,
			parryable = true
		}.Torn(0.05f, 0.09f);

		protected static EpAtk EpAbsorb = new EpAtk(3, "snake")
		{
			cli = 1,
			vagina = 10,
			other = 2,
			multiple_orgasm = 0.2f
		};

		protected static EpAtk EpAbsorb2 = new EpAtk(8, "snake_2")
		{
			vagina = 4,
			cli = 3,
			mouth = 3,
			breast = 6,
			other = 2,
			multiple_orgasm = 0.4f
		};

		protected NelAttackInfo AtkAbsorbMain = new NelAttackInfo
		{
			split_mpdmg = 1,
			attr = MGATTR.ABSORB_V,
			Beto = BetoInfo.Normal,
			EpDmg = NelNSnake.EpAbsorb,
			SerDmg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 20f)
		};

		protected NelAttackInfo AtkAbsorb2 = new NelAttackInfo
		{
			mpdmg0 = 1,
			split_mpdmg = 4,
			hit_ptcst_name = "player_absorbed_basic",
			attr = MGATTR.ABSORB,
			Beto = BetoInfo.Normal,
			EpDmg = NelNSnake.EpAbsorb2,
			SerDmg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 20f)
		};

		protected NOD.TackleInfo TackleReflect0 = NOD.getTackle("snake_reflect_0");

		protected NOD.TackleInfo TackleReflect1 = NOD.getTackle("snake_reflect_1");

		protected NOD.TackleInfo TkiGrab = NOD.getTackle("snake_grab");

		protected NOD.TackleInfo TkiGrabManaWeed = NOD.getTackle("snake_grab_manaweed");

		private M2SndInterval SndLoopWalk;

		private float eff_t;

		private float digging_t;

		private float turn_t;

		private MagicItem MgRelectAttack0;

		private int reflect_attack_id0;

		private MagicItem MgRelectAttack1;

		private int reflect_attack_id1;

		private const float dig_timeout_min = 250f;

		private const float dig_timeout_max = 340f;

		private const float dig_speed = 0.09f;

		private const float dig_speed_a = 0.0013f;

		private const float dig_worp_speed = 0.118f;

		private float softfall_t_;

		private const float REFLEC_INTV_T = 10f;

		private M2SndInterval SndLoopAbsorb;

		private const float size_w_px = 35f;

		private const float size_h_px_absorb = 188f;

		private const float size_h_px = 104f;

		private const float size_w_stun_px = 133f;

		private const float size_h_stun_px = 44f;

		private MagicItem EatBombMg;

		private int ixia_hold_level;

		private NASGroundClimber GClimb;

		private const int PRI_PUNCH = 200;

		private EnemyAnimator.FnFineFrame FD_fnFineAbsorbAttack;
	}
}
