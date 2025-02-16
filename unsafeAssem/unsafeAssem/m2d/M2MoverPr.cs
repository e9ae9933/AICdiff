using System;
using System.Collections.Generic;
using evt;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2MoverPr : M2Attackable
	{
		public bool need_check_event
		{
			set
			{
				if (value)
				{
					this.checkevent |= M2MoverPr.CHECKEV._CHECK;
				}
			}
		}

		public bool need_check_talk_event_target
		{
			set
			{
				if (value)
				{
					this.checkevent |= M2MoverPr.CHECKEV.TALK_EXECUTE;
					return;
				}
				this.checkevent &= (M2MoverPr.CHECKEV)239;
			}
		}

		public bool check_event_all
		{
			set
			{
				if (value)
				{
					this.checkevent |= M2MoverPr.CHECKEV._ALL;
				}
			}
		}

		private float running
		{
			get
			{
				return this.running_;
			}
			set
			{
				if (this.running_ != value)
				{
					this.running_ = value;
				}
			}
		}

		protected virtual void Awake()
		{
			this.FindBccLadderClimbOut = (M2BlockColliderContainer.BCCLine _C) => M2MoverPr.FindBccLadderClimbOutInner(this, _C);
		}

		public virtual void newGame()
		{
			if (this.base_gravity0 == 0f)
			{
				this.base_gravity0 = base.base_gravity;
			}
			this.recheck_force_crouch = true;
			this.pre_jump_velocity_y = 0f;
			this.stopRunning(false, false);
			base.killSpeedForce(true, true, false);
			if (this.FootD != null)
			{
				this.FootD.rideInitTo(null, false);
			}
		}

		public override void appear(Map2d _Mp)
		{
			this.AATalkable = new List<List<IM2TalkableObject>>(4);
			this.AAEvStand = new List<List<M2EventItem>>(2);
			this.AAEvStand.Add(new List<M2EventItem>(4));
			this.AAEvStand.Add(new List<M2EventItem>(4));
			if (this.base_gravity0 == 0f)
			{
				this.base_gravity0 = base.base_gravity;
			}
			this.falling_camera_shift = 0;
			base.appear(_Mp);
			if (this.Phy != null)
			{
				this.Phy.damage_foc_decline_when_move_script_attached = true;
				this.fineFootStampType();
				this.Phy.corner_slip_alloc_bits = 13U;
				this.FootD.auto_search_ladder = true;
			}
			this.NCM = new NearCheckerM(this, base.key, (NearManager.NCK)3);
			this.recheck_force_crouch = true;
			this.finePositionFromTransform();
			this.need_check_event = true;
			this.last_foot_bottom_y_ = 0f;
			this.pre_camera_y = -1000f;
			this.pre_camera_shift_x = 0f;
			this.need_check_foot_camera_scroll = true;
			this.enemy_targetted = 0;
			this.lock_stick_run = 0f;
			this.has_talktarget_nearby = 2;
			if (this.running <= -2f || this.running >= 2f)
			{
				this.running = 0f;
				this.run_continue_time_ = 0f;
			}
			if (this.size_y_default_pixel == 0)
			{
				this.size_y_default_pixel = (int)(this.sizey * base.CLEN);
			}
		}

		public virtual float event_cx
		{
			get
			{
				return base.x;
			}
		}

		public virtual float event_cy
		{
			get
			{
				return base.y + ((base.base_gravity > 0f) ? (-0.01f) : 0f);
			}
		}

		public virtual float event_sizex
		{
			get
			{
				return this.sizex;
			}
		}

		public virtual float event_sizey
		{
			get
			{
				return this.sizey;
			}
		}

		public virtual void fineFootStampType()
		{
			this.Phy.getFootManager().footstamp_type = FOOTSTAMP.SHOES;
		}

		public override void deactivateFromMap()
		{
			if (this.MyLight != null)
			{
				this.MyLight.Mp.remLight(this.MyLight);
				this.MyLight = null;
			}
			base.deactivateFromMap();
		}

		public override void runPre()
		{
			base.runPre();
			this.refineMoveKey();
			if (this.lock_stick_run != 0f)
			{
				this.lock_stick_run = X.VALWALK(this.lock_stick_run, 0f, Map2d.TScur);
			}
		}

		public virtual bool continue_crouch
		{
			get
			{
				return (this.isNormalState() && !base.on_ladder && ((this.move_key & M2MoverPr.MOVEK._TO) == (M2MoverPr.MOVEK)0 || this.view_crouching) && this.isBO(0)) || this.forceCrouch(false, false);
			}
		}

		public virtual void appearToCenterPr()
		{
		}

		protected virtual void refineMoveKey()
		{
			Bench.P("Pr refine move key");
			bool flag = this.isNormalState();
			bool flag2 = base.on_ladder;
			bool flag3 = false;
			bool flag4 = false;
			bool continue_crouch = this.continue_crouch;
			if (flag2)
			{
				this.move_key = (M2MoverPr.MOVEK)0;
			}
			else if (this.view_crouching)
			{
				bool flag5 = (this.manip & M2MoverPr.PR_MNP.NO_MOVE_INIT) == (M2MoverPr.PR_MNP)0;
				bool flag6 = (this.manip & M2MoverPr.PR_MNP.NO_MOVING_B) == (M2MoverPr.PR_MNP)0;
				this.move_key &= M2MoverPr.MOVEK._CRAWL;
				this.move_key |= ((flag5 && this.isRP(1)) ? ((M2MoverPr.MOVEK)3) : ((flag6 && this.isRO(0)) ? M2MoverPr.MOVEK.KEYD_R : ((M2MoverPr.MOVEK)0))) | ((flag5 && this.isLP(1)) ? ((M2MoverPr.MOVEK)12) : ((flag6 && this.isLO(0)) ? M2MoverPr.MOVEK.KEYD_L : ((M2MoverPr.MOVEK)0)));
				if ((this.move_key & M2MoverPr.MOVEK.KEYP_R) != (M2MoverPr.MOVEK)0 || (!Map2d.can_handle && (this.move_key & M2MoverPr.MOVEK.KEYD_R) != (M2MoverPr.MOVEK)0))
				{
					this.move_key |= M2MoverPr.MOVEK.CRAWL_R;
				}
				else if ((this.move_key & M2MoverPr.MOVEK.KEYD_R) == (M2MoverPr.MOVEK)0)
				{
					this.move_key &= (M2MoverPr.MOVEK)(-1025);
				}
				if ((this.move_key & M2MoverPr.MOVEK.KEYP_L) != (M2MoverPr.MOVEK)0 || (!Map2d.can_handle && (this.move_key & M2MoverPr.MOVEK.KEYD_L) != (M2MoverPr.MOVEK)0))
				{
					this.move_key |= M2MoverPr.MOVEK.CRAWL_L;
				}
				else if ((this.move_key & M2MoverPr.MOVEK.KEYD_L) == (M2MoverPr.MOVEK)0)
				{
					this.move_key &= (M2MoverPr.MOVEK)(-2049);
				}
				if ((this.move_key & M2MoverPr.MOVEK.CRAWL_R) != (M2MoverPr.MOVEK)0 && (this.move_key & M2MoverPr.MOVEK.CRAWL_L) == (M2MoverPr.MOVEK)0)
				{
					this.move_key |= M2MoverPr.MOVEK.TO_R;
				}
				else if ((this.move_key & M2MoverPr.MOVEK.CRAWL_L) != (M2MoverPr.MOVEK)0 && (this.move_key & M2MoverPr.MOVEK.CRAWL_R) == (M2MoverPr.MOVEK)0)
				{
					this.move_key |= M2MoverPr.MOVEK.TO_L;
				}
			}
			else
			{
				flag3 = (this.manip & (M2MoverPr.PR_MNP)12) == (M2MoverPr.PR_MNP)0 && (!this.isRunningCurrent() || (this.move_key & M2MoverPr.MOVEK._TO) == (M2MoverPr.MOVEK)0 || this.pre_ladder_shifting || (!this.canJump() && this.isTP(1)));
				this.runnormalMK();
				if ((this.manip & (M2MoverPr.PR_MNP)12) == (M2MoverPr.PR_MNP)0 && (this.isRO(0) || this.isLO(0)))
				{
					flag3 = flag3 && this.isTP(10);
				}
				if (flag3)
				{
					flag3 = false;
					if (flag)
					{
						bool flag7 = (this.jump_hold_lock_ ? (this.pre_ladder_shifting || this.isTP(10)) : this.isTO(0));
						if (this.FootD.LadderBCC != null && flag7 && X.BTW(this.FootD.LadderBCC.shifted_y + (base.hasFoot() ? 2.2f : (this.sizey + 0.1f - 0.4f)), this.Phy.move_depert_tstack_y, this.FootD.LadderBCC.sbottom + ((base.hasFoot() && (this.isLO(0) || this.isRO(0))) ? (-0.5f) : (this.sizey + 0.2f))) && this.canClimbToLadder())
						{
							float num = this.FootD.LadderBCC.shifted_x - base.x;
							float num2 = X.Abs(num);
							if (num2 < 1f)
							{
								this.jump_hold_lock_ = false;
								flag4 = (flag3 = true);
								if (num2 > 0.012f)
								{
									if (base.hasFoot())
									{
										this.move_key = ((num < 0f) ? M2MoverPr.MOVEK.TO_L : M2MoverPr.MOVEK.TO_R);
									}
									if (X.Abs(this.running) >= 2f)
									{
										this.stopRunning(false, false);
									}
								}
							}
						}
					}
				}
			}
			if (flag)
			{
				M2MoverPr.PR_MNP pr_MNP = this.manip;
				if ((this.manip & M2MoverPr.PR_MNP.NO_CROUCH) == (M2MoverPr.PR_MNP)0 && continue_crouch)
				{
					if (this.canJump())
					{
						if (this.crouching <= 0f)
						{
							this.crouching = 0f;
							this.need_check_talk_event_target = true;
						}
						this.crouching = X.Mx(this.crouching + this.TS, 1f);
						if (!this.FootD.FootIsLadder() && this.isBO(0) && (this.move_key & M2MoverPr.MOVEK._CRAWL) == (M2MoverPr.MOVEK)0)
						{
							this.move_key &= (M2MoverPr.MOVEK)(-241);
							if ((this.move_key & M2MoverPr.MOVEK._TO_WALK) != (M2MoverPr.MOVEK)0)
							{
								this.move_key |= M2MoverPr.MOVEK.THROUGH_LIFT;
							}
						}
						else
						{
							this.crouching = 1f;
							this.move_key &= (M2MoverPr.MOVEK)(-4097);
						}
						this.running = (float)(this.run_delay = 0);
						this.run_continue_time_ = 0f;
						this.move_key &= (M2MoverPr.MOVEK)(-193);
					}
					else if (this.isBO(0))
					{
						this.move_key |= M2MoverPr.MOVEK.THROUGH_LIFT;
					}
					else
					{
						this.move_key &= (M2MoverPr.MOVEK)(-4097);
						this.crouching = (float)((this.crouching > 0f) ? 1 : 0);
					}
				}
				else
				{
					if (!this.isBO(0) || this.FootD.FootIsLadder())
					{
						this.move_key &= (M2MoverPr.MOVEK)(-4097);
					}
					if (this.crouching > 0f)
					{
						this.quitCrouch(true, false, false);
						if (!this.canJump())
						{
							this.crouching = -10f;
						}
					}
				}
			}
			else if (this.isCalcableThroughLiftState())
			{
				if (!this.isBO(0) || this.FootD.FootIsLadder())
				{
					this.move_key &= (M2MoverPr.MOVEK)(-4097);
				}
				else
				{
					this.move_key |= M2MoverPr.MOVEK.THROUGH_LIFT;
				}
			}
			int num3 = 0;
			if ((this.manip & M2MoverPr.PR_MNP.NO_MOVING_B) == (M2MoverPr.PR_MNP)0)
			{
				M2MoverPr.MOVEK movek = this.move_key & M2MoverPr.MOVEK._TO_WALK;
				if (movek != M2MoverPr.MOVEK.TO_R)
				{
					if (movek == M2MoverPr.MOVEK.TO_L)
					{
						num3 = -1;
					}
				}
				else
				{
					num3 = 1;
				}
			}
			int num4 = num3;
			if (num3 != 0 && flag)
			{
				if (Map2d.can_handle && base.wallHittedVX((float)num3))
				{
					if (num3 > 0)
					{
						this.move_key &= (M2MoverPr.MOVEK)(-65);
					}
					else
					{
						this.move_key &= (M2MoverPr.MOVEK)(-129);
					}
					this.run_delay = 0;
					if (this.walk_auto_assisting == 2)
					{
						base.endDrawAssist(1);
					}
					this.run_continue_time_ = 0f;
				}
				else if (this.run_continue_time_ < 0f)
				{
					this.run_continue_time_ = 1f;
				}
			}
			else
			{
				this.lock_stick_run = 0f;
			}
			if ((this.move_key & M2MoverPr.MOVEK.TO_R_RUN) != (M2MoverPr.MOVEK)0 && this.run_continue_time_ >= 0f)
			{
				if (this.FootD.canStartRunning() && this.Phy.walk_xspeed < 0f && this.running < 0f)
				{
					num4 = (num3 = 0);
					this.running = X.Mn(this.running, -2f);
				}
				else if (this.running <= 0f)
				{
					this.running = 1f;
					this.run_continue_time_ = 1f;
				}
				if (this.running > 0f && this.cannotRun(this.run_continue_time_ == 1f))
				{
					this.declineRunningEffect();
					if (this.run_delay >= 0)
					{
						this.run_delay = 18;
					}
					this.move_key &= (M2MoverPr.MOVEK)(-321);
					this.running = 0f;
					this.run_continue_time_ = 0f;
				}
			}
			else
			{
				if (this.running == 1f)
				{
					this.running = 0f;
					if (flag && (this.manip & M2MoverPr.PR_MNP.NO_MOVING) == (M2MoverPr.PR_MNP)0 && (this.move_key & M2MoverPr.MOVEK.TO_L) <= (M2MoverPr.MOVEK)0 && this.run_continue_time_ >= 0f && (this.run_continue_time_ >= 13f || (!this.canJump() && (this.move_key & M2MoverPr.MOVEK.TO_R_DT_RUN) != (M2MoverPr.MOVEK)0)))
					{
						this.running = 2f;
					}
					this.run_continue_time_ = 0f;
				}
				this.move_key &= (M2MoverPr.MOVEK)(-321);
				if (this.lock_stick_run > 0f)
				{
					this.lock_stick_run = 0f;
				}
			}
			if ((this.move_key & M2MoverPr.MOVEK.TO_L_RUN) != (M2MoverPr.MOVEK)0 && this.run_continue_time_ >= 0f)
			{
				if (this.FootD.canStartRunning() && this.Phy.walk_xspeed > 0f && this.running > 0f)
				{
					num4 = (num3 = 0);
					this.running = X.Mx(this.running, 2f);
				}
				else if (this.running >= 0f)
				{
					this.running = -1f;
					this.run_continue_time_ = 1f;
				}
				if (this.running < 0f && this.cannotRun(this.run_continue_time_ == 1f))
				{
					this.declineRunningEffect();
					if (this.run_delay >= 0)
					{
						this.run_delay = 18;
					}
					this.move_key &= (M2MoverPr.MOVEK)(-641);
					this.running = 0f;
					this.run_continue_time_ = 0f;
				}
			}
			else
			{
				if (this.running == -1f)
				{
					this.running = 0f;
					if (flag && (this.manip & M2MoverPr.PR_MNP.NO_MOVING) == (M2MoverPr.PR_MNP)0 && (this.move_key & M2MoverPr.MOVEK.TO_R) <= (M2MoverPr.MOVEK)0 && (this.run_continue_time_ >= 13f || (!this.canJump() && (this.move_key & M2MoverPr.MOVEK.TO_L_DT_RUN) != (M2MoverPr.MOVEK)0)))
					{
						this.running = -2f;
					}
					this.run_continue_time_ = 0f;
				}
				this.move_key &= (M2MoverPr.MOVEK)(-641);
				if (this.lock_stick_run < 0f)
				{
					this.lock_stick_run = 0f;
				}
			}
			if (this.running == 0f)
			{
				this.run_continue_time_ = 0f;
			}
			if (flag && !this.isMoveScriptActive(false))
			{
				float num5 = this.calcWalkSpeed(num3);
				this.Phy.walk_xspeed = num5;
			}
			if (num4 != 0 && flag)
			{
				this.setAim((num4 > 0) ? AIM.R : AIM.L, false);
			}
			if (this.run_delay != 0)
			{
				this.run_delay = X.VALWALK(this.run_delay, 0, 1);
			}
			if (this.running >= 2f)
			{
				if (this.running == 2f && this.canJump())
				{
					this.SpSetPose("run_stop", -1, null, false);
					this.playSndPos("run_slip", 1);
					this.Mp.PtcN("run_end_sunabokori", base.x, base.footbottom, 0f, 0, 0);
				}
				this.running += this.TS;
				if (this.running >= 48f)
				{
					this.running = 0f;
				}
			}
			if (this.running <= -2f)
			{
				if (this.running == -2f && this.canJump())
				{
					this.SpSetPose("run_stop", -1, null, false);
					this.playSndPos("run_slip", 1);
					this.Mp.PtcN("run_end_sunabokori", base.x, base.footbottom, 3.1415927f, 0, 0);
				}
				this.running -= this.TS;
				if (this.running <= -48f)
				{
					this.running = 0f;
				}
			}
			if (this.run_continue_time_ != 0f)
			{
				if (flag)
				{
					if (this.run_continue_time_ >= 0f && this.run_continue_time_ == 1f && this.canJump())
					{
						this.Mp.PtcN("run_start_sunabokori", base.x, base.footbottom, (this.running > 0f) ? 0f : 3.1415927f, 0, 0);
					}
					this.addRunningContinueTime(ref this.run_continue_time_);
				}
				else if (this.run_continue_time_ > 0f)
				{
					this.run_continue_time_ = X.Mn(this.run_continue_time_, 2f);
				}
				else
				{
					this.stopRunning(false, false);
				}
			}
			this.pre_ladder_shifting = false;
			if (this.jump_hold_lock_ && !this.isJumpO(0))
			{
				this.jump_hold_lock_ = false;
			}
			if (flag && (this.manip & M2MoverPr.PR_MNP.NO_JUMP) == (M2MoverPr.PR_MNP)0)
			{
				bool flag8 = false;
				M2BlockColliderContainer.BCCLine bccline = null;
				if (flag2)
				{
					bccline = this.FootD.get_FootBCC();
					if (bccline == null || !bccline.is_ladder)
					{
						bccline = null;
						flag2 = false;
					}
				}
				int num6 = 0;
				if (flag2)
				{
					this.last_foot_bottom_y_ = base.mbottom;
					if (this.isTO(0))
					{
						num6 = -1;
						flag4 = !this.isLP(40) && !this.isRP(40) && (!this.isJumpPD(1) || this.isTP(10));
						if (!flag4)
						{
							flag8 = true;
						}
						if (this.jump_pushing >= -2f)
						{
							if (base.mtop < bccline.shifted_y + 0.1f)
							{
								M2BlockColliderContainer.BCCLine bccline2 = this.FootD.findBcc(this.FindBccLadderClimbOut);
								if (bccline2 != null)
								{
									this.climbToTopLiftFromLadder(bccline2, false);
								}
								else if (this.isTP(1) && this.Mp.canStand((int)base.x, (int)(base.mtop - 1f)))
								{
									flag4 = false;
								}
							}
							else
							{
								float sbottom = bccline.sbottom;
								if (this.climbLadder(false, 7))
								{
									this.jump_pushing = -9f;
								}
							}
						}
					}
					else if (this.isBO(0))
					{
						num6 = 1;
						flag4 = !this.isLP(20) && !this.isRP(20) && (!this.isJumpPD(1) || this.isBP());
						if (!flag4)
						{
							flag8 = true;
						}
						if (base.wallHitted(AIM.B))
						{
							this.FootD.initJump(false, true, false);
							if (this.FootD.get_FootBCC() == null)
							{
								this.FootD.playFootStampEffect(false);
							}
						}
						else if (this.jump_pushing >= -2f)
						{
							float sbottom2 = bccline.sbottom;
							if (base.y > sbottom2 + 0.25f)
							{
								this.FootD.initJump(true, false, false);
							}
							else if (this.climbLadder(true, 7))
							{
								this.jump_pushing = -9f;
							}
						}
					}
					if (this.jump_pushing < -2f)
					{
						this.jump_pushing = X.VALWALK(this.jump_pushing, -2f, this.TS);
					}
				}
				else if (flag3 && this.FootD.LadderBCC != null)
				{
					float num7 = this.FootD.LadderBCC.shifted_x - this.Phy.move_depert_tstack_x_w;
					if (base.mtop >= this.FootD.LadderBCC.shifted_y + 0.1f + 0.03f && X.Abs(num7) < ((!base.hasFoot() && (this.isTP(10) || this.pre_ladder_shifting)) ? 0.88f : ((base.hasFoot() && (this.isTP(10) || this.isLO(0) || this.isRO(0))) ? 0.38f : 0.16f)))
					{
						this.pre_ladder_shifting = true;
						if (X.Abs(num7) < 0.34f)
						{
							this.Phy.addFoc(FOCTYPE.RESIZE, num7, 0f, -1f, -1, 1, 0, -1, 0);
							if (this.FootD.LadderBCC.isCarryable(this.FootD, 0f, true) == this.FootD.LadderBCC)
							{
								this.FootD.rideInitTo(this.FootD.LadderBCC, false);
								this.Phy.walk_xspeed = 0f;
								flag3 = false;
								float num8 = base.mtop + 0.25f - this.FootD.LadderBCC.shifted_bottom;
								if (num8 > 0f)
								{
									this.Phy.addFoc(FOCTYPE.RESIZE, 0f, -num8, -1f, -1, 1, 0, -1, 0);
								}
							}
							else
							{
								this.Phy.addFoc(FOCTYPE.RESIZE, -num7, 0f, -1f, -1, 1, 0, -1, 0);
							}
						}
					}
					if (flag3 && !base.hasFoot() && X.Abs(num7) >= 0.083f)
					{
						this.Phy.addFoc(FOCTYPE.RESIZE, X.absMn(num7, 0.08f), 0f, -1f, -1, 1, 0, -1, 0);
					}
				}
				if (num6 != 0 && this.Phy.main_updated_count <= 0 && this.pre_camera_y != -1000f)
				{
					if (num6 < 0)
					{
						this.pre_camera_y = X.Mx(this.drawy - 4f * base.CLEN, this.pre_camera_y - 0.071428575f);
					}
					else
					{
						this.pre_camera_y = X.Mn(this.drawy + 4f * base.CLEN, this.pre_camera_y + 0.071428575f);
					}
				}
				if (!this.jump_hold_lock_ && !flag4 && this.canJump() && this.isJumpO(0) && !this.is_crouch)
				{
					this.jump_pushing = (float)(flag8 ? 20001 : 20000);
					if (flag2)
					{
						IN.clearArrowPD(15U, false, 50);
					}
				}
			}
			if (this.falling_camera_shift > -38)
			{
				this.falling_camera_shift--;
			}
			if (this.t_force_crouch > 0f)
			{
				this.t_force_crouch -= this.TS;
				if (this.t_force_crouch <= 0f)
				{
					this.t_force_crouch = 15f;
					this.recheck_force_crouch = true;
				}
			}
			else
			{
				this.t_force_crouch += this.TS;
				if (this.t_force_crouch >= 0f)
				{
					this.t_force_crouch = 0f;
				}
			}
			Bench.Pend("Pr refine move key");
			this.ladder_check = flag3;
		}

		protected virtual void addRunningContinueTime(ref float run_continue_time_)
		{
			run_continue_time_ += 1f;
			if (!this.canJump() || this.Phy.isin_water)
			{
				this.fixRunningContinueTimeToBottom();
			}
		}

		protected void fixRunningContinueTimeToBottom()
		{
			if (this.run_continue_time_ >= 2f)
			{
				this.run_continue_time_ = 2f;
			}
		}

		private static bool FindBccLadderClimbOutInner(M2MoverPr Pr, M2BlockColliderContainer.BCCLine _C)
		{
			if (!_C.isUseableDir(Pr.FootD) || !_C.is_lift)
			{
				return false;
			}
			float num = -Pr.FootD.get_FootBCC().shifted_x;
			Vector3 vector = _C.crosspointL(1f, 0f, num, 1f, -0.25f, 2.5f);
			return vector.z >= 2f && X.BTW(Pr.mtop - 1.5f, vector.y, Pr.y + 0.75f);
		}

		private void runnormalMK()
		{
			bool flag = (this.manip & M2MoverPr.PR_MNP.NO_MOVE_INIT) == (M2MoverPr.PR_MNP)0;
			bool flag2 = (this.manip & M2MoverPr.PR_MNP.NO_MOVING_B) == (M2MoverPr.PR_MNP)0;
			this.move_key &= (M2MoverPr.MOVEK)1008 | ((flag2 && this.isBO(0)) ? M2MoverPr.MOVEK._CRAWL : ((M2MoverPr.MOVEK)0));
			this.move_key |= ((flag && this.isRP(1)) ? ((M2MoverPr.MOVEK)3) : ((flag2 && this.isRO(0)) ? M2MoverPr.MOVEK.KEYD_R : ((M2MoverPr.MOVEK)0))) | ((flag && this.isLP(1)) ? ((M2MoverPr.MOVEK)12) : ((flag2 && this.isLO(0)) ? M2MoverPr.MOVEK.KEYD_L : ((M2MoverPr.MOVEK)0)));
			if ((this.move_key & M2MoverPr.MOVEK.KEYD_R) == (M2MoverPr.MOVEK)0)
			{
				this.move_key &= (M2MoverPr.MOVEK)(-337);
				if ((this.move_key & M2MoverPr.MOVEK.KEYD_L) != (M2MoverPr.MOVEK)0)
				{
					this.move_key |= M2MoverPr.MOVEK.TO_L;
				}
			}
			else
			{
				this.move_key &= (M2MoverPr.MOVEK)(-641);
			}
			if ((this.move_key & M2MoverPr.MOVEK.KEYD_L) == (M2MoverPr.MOVEK)0)
			{
				this.move_key &= (M2MoverPr.MOVEK)(-673);
				if ((this.move_key & M2MoverPr.MOVEK.KEYD_R) != (M2MoverPr.MOVEK)0)
				{
					this.move_key |= M2MoverPr.MOVEK.TO_R;
				}
			}
			else
			{
				this.move_key &= (M2MoverPr.MOVEK)(-321);
			}
			if ((this.move_key & M2MoverPr.MOVEK.KEYP_R) != (M2MoverPr.MOVEK)0 && (this.move_key & M2MoverPr.MOVEK.KEYP_L) == (M2MoverPr.MOVEK)0)
			{
				this.move_key = (this.move_key & (M2MoverPr.MOVEK)(-241)) | M2MoverPr.MOVEK.TO_R | M2MoverPr.MOVEK.KEYP_R;
				if (X.Abs(this.running) >= 2f)
				{
					this.running = 0f;
				}
				if (M2MoverPr.double_tap_running && (this.manip & M2MoverPr.PR_MNP.NO_RUN) == (M2MoverPr.PR_MNP)0)
				{
					if (this.run_delay <= 0)
					{
						this.run_delay = 18;
					}
					else
					{
						this.run_delay = 0;
						this.move_key |= (M2MoverPr.MOVEK)320;
					}
				}
			}
			else if ((this.move_key & M2MoverPr.MOVEK.KEYP_L) != (M2MoverPr.MOVEK)0 && (this.move_key & M2MoverPr.MOVEK.KEYP_R) == (M2MoverPr.MOVEK)0)
			{
				this.move_key = (this.move_key & (M2MoverPr.MOVEK)(-241)) | M2MoverPr.MOVEK.TO_L | M2MoverPr.MOVEK.KEYP_L;
				if (X.Abs(this.running) >= 2f)
				{
					this.running = 0f;
				}
				if (M2MoverPr.double_tap_running && (this.manip & M2MoverPr.PR_MNP.NO_RUN) == (M2MoverPr.PR_MNP)0)
				{
					if (this.run_delay >= 0)
					{
						this.run_delay = -18;
					}
					else
					{
						this.run_delay = 0;
						this.move_key |= (M2MoverPr.MOVEK)640;
					}
				}
			}
			else
			{
				switch (this.move_key & (M2MoverPr.MOVEK)5)
				{
				case M2MoverPr.MOVEK.KEYP_R:
					this.move_key |= M2MoverPr.MOVEK.TO_R;
					break;
				case M2MoverPr.MOVEK.KEYP_L:
					this.move_key |= M2MoverPr.MOVEK.TO_L;
					break;
				case (M2MoverPr.MOVEK)5:
					this.move_key |= ((base.mpf_is_right >= 0f) ? M2MoverPr.MOVEK.TO_R : M2MoverPr.MOVEK.TO_L);
					break;
				}
			}
			if ((this.move_key & M2MoverPr.MOVEK.TO_R) == (M2MoverPr.MOVEK)0)
			{
				this.move_key &= (M2MoverPr.MOVEK)(-1025);
			}
			else if (this.startToRunInput(true))
			{
				if (this.cannotRun(false) || (this.manip & M2MoverPr.PR_MNP.NO_RUN) != (M2MoverPr.PR_MNP)0)
				{
					this.stopRunning(false, false);
					this.declineRunningEffect();
				}
				else
				{
					this.run_delay = 0;
					this.move_key |= M2MoverPr.MOVEK.TO_R_RUN;
				}
			}
			else if ((this.move_key & M2MoverPr.MOVEK.TO_R_DT_RUN) == (M2MoverPr.MOVEK)0)
			{
				this.move_key &= (M2MoverPr.MOVEK)(-65);
				if (this.running > 0f)
				{
					this.run_continue_time_ = 1f;
				}
			}
			if ((this.move_key & M2MoverPr.MOVEK.TO_L) == (M2MoverPr.MOVEK)0)
			{
				this.move_key &= (M2MoverPr.MOVEK)(-2049);
				return;
			}
			if (!this.startToRunInput(false))
			{
				if ((this.move_key & M2MoverPr.MOVEK.TO_L_DT_RUN) == (M2MoverPr.MOVEK)0)
				{
					this.move_key &= (M2MoverPr.MOVEK)(-129);
					if (this.running < 0f)
					{
						this.run_continue_time_ = 1f;
					}
				}
				return;
			}
			if (this.cannotRun(false) || (this.manip & M2MoverPr.PR_MNP.NO_RUN) != (M2MoverPr.PR_MNP)0)
			{
				this.stopRunning(false, false);
				this.declineRunningEffect();
				return;
			}
			this.run_delay = 0;
			this.move_key |= M2MoverPr.MOVEK.TO_L_RUN;
		}

		public virtual bool canClimbToLadder()
		{
			return true;
		}

		public virtual bool climbLadder(bool to_under, int ascend_time)
		{
			M2BlockColliderContainer.BCCLine ladderBCC = this.FootD.LadderBCC;
			if (ladderBCC == null)
			{
				return false;
			}
			float sbottom = ladderBCC.sbottom;
			this.Phy.addFoc(FOCTYPE.RESIZE, ladderBCC.x - base.x, 0f, -1f, -1, 1, 0, -1, 0);
			float num = (float)X.IntR((base.mbottom - sbottom) * 2f) * 0.5f + sbottom - this.sizey + 0.5f * (float)X.MPF(to_under);
			this.Phy.addFocToSmooth(FOCTYPE.WALK | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, base.x, num, ascend_time - 2, -1, 0, -1f);
			this.FootD.playFootStampEffect(false);
			this.jump_pushing = -2f;
			return true;
		}

		public virtual void climbToTopLiftFromLadder(M2BlockColliderContainer.BCCLine RideTo, bool to_under = false)
		{
			this.Phy.walk_xspeed = 0f;
			if (!to_under)
			{
				this.Phy.addFoc(FOCTYPE.RESIZE, 0f, RideTo.shifted_y - base.mbottom, -1f, -1, 1, 0, -1, 0);
				this.jump_hold_lock_ = true;
			}
			else
			{
				this.quitCrouch(true, false, true);
				this.Phy.addFoc(FOCTYPE.RESIZE, this.FootD.LadderBCC.shifted_x - this.Phy.move_depert_tstack_x, X.Mx(RideTo.shifted_y - 0.15f - base.mtop, 1.25f), -1f, -1, 1, 0, -1, 0);
			}
			this.FootD.rideInitTo(RideTo, false);
		}

		public void jumpRaisingQuit(bool strong = false)
		{
			if (strong)
			{
				if (this.jump_pushing >= 1f || this.ladder_moving_lock)
				{
					this.jump_pushing = -2f;
					return;
				}
			}
			else
			{
				this.jump_pushing = ((this.jump_pushing >= 11000f) ? 11000f : ((this.jump_pushing >= 1f) ? 1f : this.jump_pushing));
			}
		}

		public bool ladder_moving_lock
		{
			get
			{
				return this.jump_pushing < -2f;
			}
		}

		public bool jump_raising
		{
			get
			{
				if (this.jump_pushing < 11000f)
				{
					return this.jump_pushing > 1f;
				}
				return this.jump_pushing > 11000f;
			}
		}

		public bool jump_starting
		{
			get
			{
				return X.BTW(20000f, this.jump_pushing, 20100f);
			}
		}

		public bool jumping_in_water
		{
			get
			{
				return this.jump_pushing >= 11000f;
			}
		}

		public virtual float jump_speed_ratio
		{
			get
			{
				return 1f;
			}
		}

		public bool jump_occuring
		{
			get
			{
				return this.jump_pushing < 20000f && this.jump_pushing >= -1f;
			}
		}

		public bool jump_hold_lock
		{
			get
			{
				return this.jump_hold_lock_;
			}
			set
			{
				if (this.jump_hold_lock_ == value)
				{
					return;
				}
				this.jump_hold_lock_ = value;
				if (value && this.jump_starting)
				{
					this.jump_pushing = -2f;
				}
			}
		}

		protected virtual void jumpInitialize()
		{
			this.jump_pushing = (float)(10000 + (this.Phy.isin_water ? (5000 + this.JUMP_HOLD_MAXT_WATER) : 2));
		}

		public override void runPhysics(float fcnt)
		{
			bool flag = false;
			if (this.jump_pushing > 0f)
			{
				int num = 0;
				bool flag2 = this.jump_pushing == 20001f;
				bool flag3 = false;
				bool flag4 = false;
				if (this.jump_pushing == 20000f || flag2)
				{
					this.FootD.fineFootStampType();
					this.jumpInitialize();
					this.runnormalMK();
					this.PadVib("jump", 1f);
					flag3 = true;
					flag4 = this.Phy.is_on_web;
					if ((this.move_key & M2MoverPr.MOVEK.TO_R_RUN) != (M2MoverPr.MOVEK)0)
					{
						this.Phy.setWalkXSpeed(this.runSpeed, true, !this.Phy.is_on_ice);
					}
					if ((this.move_key & M2MoverPr.MOVEK.TO_L_RUN) != (M2MoverPr.MOVEK)0)
					{
						this.Phy.setWalkXSpeed(-this.runSpeed, true, !this.Phy.is_on_ice);
					}
					if (flag2 && this.isBO(0))
					{
						this.jump_pushing = (float)(this.Phy.isin_water ? 11000 : 1);
					}
				}
				else
				{
					num = (((this.manip & M2MoverPr.PR_MNP.NO_JUMP) != (M2MoverPr.PR_MNP)0 || !this.isJumpO(0)) ? 1 : 0);
					if (base.wallHitted(AIM.T))
					{
						num = 2;
						M2BlockColliderContainer.BCCLine bccline;
						float num2;
						if (!base.canGoToSideLB(out bccline, out num2, AIM.T, 0.2f, -0.15f, false, false, false) && bccline != null)
						{
							bool flag5 = true;
							this.checkBCCEvent(bccline, ref flag5, bccline.BCC.base_shift_x, bccline.BCC.base_shift_y + X.Mn(base.vy * 0.4f, 0f));
						}
					}
				}
				if (this.canJump())
				{
					this.FootD.initJump(false, false, false);
				}
				if (num > 0)
				{
					bool flag6 = this.jump_pushing >= 11000f;
					this.jump_pushing = -1f;
					if (base.vy < -0.10712f && num == 1)
					{
						float num3 = X.Mx(base.vy, -0.06695f);
						this.Phy.addFocFallingY(FOCTYPE.JUMP | FOCTYPE._CHECK_WALL, num3, 0.66f * (flag6 ? this.Phy.water_speed_scale : 1f), 0);
					}
				}
				else if (this.jump_pushing >= 11000f)
				{
					if (!this.Phy.isin_water)
					{
						this.jump_pushing = 1f;
						if (base.vy < 0f && base.vy < this.ySpeedKeyReleased_water * 0.25f)
						{
							float num4 = (this.ySpeedKeyReleased_water - base.vy) * this.Phy.water_speed_scale;
							this.Phy.addFocFallingY(FOCTYPE.JUMP | FOCTYPE._CHECK_WALL, num4, 0.66f * this.Phy.water_speed_scale, 0);
						}
					}
					if (this.jump_pushing > 11000f)
					{
						this.falling_camera_shift = 40;
						float num5 = this.ySpeedStart_water * this.jump_speed_ratio;
						if (flag3)
						{
							if (flag4)
							{
								num5 *= 0.5f;
							}
						}
						else
						{
							num5 = X.Mx(num5, this.Phy.pre_force_velocity_y);
						}
						if (this.jump_pushing > 15000f)
						{
							this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._CHECK_WALL, 0f, num5, -1f, -1, 1, 0, -1, 0);
							this.jump_pushing -= this.TS * (float)((this.Phy.calcFocVelocity(FOCTYPE.KNOCKBACK, true, false).y > 0f) ? 3 : 1);
						}
						else
						{
							float num6 = this.jump_pushing;
							num5 += this.Phy.gravity_apply_velocity(this.TS * this.Phy.water_speed_scale);
							if (num5 >= 0f)
							{
								this.jump_pushing = 11000f;
							}
							else
							{
								this.jump_pushing -= this.TS;
								this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK | FOCTYPE._CHECK_WALL, 0f, num5, -1f, -1, 1, 0, -1, 0);
							}
						}
					}
				}
				else
				{
					float num7 = this.ySpeedStart * this.jump_speed_ratio;
					if (flag3)
					{
						if (flag4)
						{
							num7 *= 0.5f;
						}
					}
					else
					{
						num7 = X.Mx(num7, this.pre_jump_velocity_y);
					}
					if (this.jump_pushing > 10000f)
					{
						this.falling_camera_shift = 20;
						this.pre_jump_velocity_y = num7;
						this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK | FOCTYPE._CHECK_WALL, 0f, num7, -1f, -1, 1, 0, -1, 0);
						this.jump_pushing -= this.TS;
						flag = true;
					}
					else if (this.jump_pushing > 1f)
					{
						this.falling_camera_shift = 20;
						float num8 = this.jump_pushing;
						num7 += this.Phy.gravity_apply_velocity(this.TS);
						if (num7 >= 0f)
						{
							this.jump_pushing = 1f;
						}
						else
						{
							this.jump_pushing -= this.TS;
							this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK | FOCTYPE._CHECK_WALL, 0f, num7, -1f, -1, 1, 0, -1, 0);
							this.pre_jump_velocity_y = num7;
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				this.pre_jump_velocity_y = 0f;
			}
			bool flag7 = true;
			if (this.jump_raising)
			{
				float y = this.Phy.calcFocVelocity(FOCTYPE.WALK, true, false).y;
				if (y < 0f)
				{
					flag7 = false;
					this.jump_cam_shift_y = X.Mx(-5.5f, this.jump_cam_shift_y + y);
				}
			}
			if (flag7 && this.jump_cam_shift_y != 0f)
			{
				this.jump_cam_shift_y = X.VALWALK(this.jump_cam_shift_y, 0f, base.hasFoot() ? 0.044f : 0.018f);
			}
			base.runPhysics(fcnt);
		}

		public override void addFocFallWaterVelocity(float __vy, int duration)
		{
			if (15000f <= this.jump_pushing && this.jump_pushing <= (float)(15000 + this.JUMP_HOLD_MAXT_WATER))
			{
				__vy *= 0.0625f;
			}
			base.addFocFallWaterVelocity(__vy, duration);
		}

		public override IFootable checkFootObject(float pre_fall_y)
		{
			if (this.jump_pushing > 1f && this.jump_pushing != 11000f)
			{
				return null;
			}
			return base.checkFootObject(pre_fall_y);
		}

		public bool startToRunInput(bool is_right)
		{
			if (M2MoverPr.jump_press_reverse && Map2d.can_handle)
			{
				return !this.isRunO();
			}
			bool flag = false;
			bool flag2 = false;
			if (M2MoverPr.running_thresh < 1f)
			{
				flag = (is_right ? this.isRunningRO(out flag2) : this.isRunningLO(out flag2));
				if (flag2)
				{
					this.lock_stick_run = (float)(X.MPF(is_right) * 6);
				}
			}
			return flag || this.isRunO();
		}

		protected virtual float calcWalkSpeed(int move_aim_ex)
		{
			float num = this.Phy.walk_xspeed;
			if (move_aim_ex == 0)
			{
				num = ((this.isRunning() && this.run_continue_time_ >= 0f) ? X.VALWALK(num, 0f, (!this.canJump()) ? this.accel_run_break_air : this.accel_run_break) : 0f);
			}
			else if (this.view_crouching)
			{
				num = (float)((move_aim_ex > 0) ? 1 : (-1)) * this.walkSpeed * 0.75f;
			}
			else
			{
				num = (float)((move_aim_ex > 0) ? 1 : (-1)) * (this.isRunning() ? this.runSpeed : this.walkSpeed);
				base.M2D.Cam.setCamWalkSpeedX(num * base.CLEN);
			}
			return num;
		}

		public override M2Mover assignMoveScript(string st, bool soft_touch = false)
		{
			if (this.MScr == null || !this.MScr.isActive())
			{
				this.Phy.setWalkXSpeed(0f, false, true);
			}
			return base.assignMoveScript(st, soft_touch);
		}

		public override void IntPositionChanged(int prex, int prey)
		{
			base.IntPositionChanged(prex, prey);
			this.checkevent |= M2MoverPr.CHECKEV._ALL;
		}

		public override void positionChanged(float prex, float prey)
		{
			base.positionChanged(prex, prey);
			this.checkevent |= M2MoverPr.CHECKEV._EXECUTE;
		}

		public virtual bool setAbsorbAnimation(string p, bool set_default = false, bool frozen_replacable = false)
		{
			if (TX.valid(p))
			{
				this.SpSetPose(p, -1, null, false);
			}
			return true;
		}

		public virtual bool runUi()
		{
			bool flag = false;
			if (!base.M2D.pre_map_active)
			{
				return false;
			}
			if (this.Mp.getKeyPr() == this)
			{
				if (Map2d.can_handle && this.MScr == null)
				{
					this.move_script_attached = false;
				}
				if (Map2d.can_handle && !EV.isActive(false))
				{
					if (!this.cannotAccessToCheckEvent())
					{
						if ((this.checkevent & M2MoverPr.CHECKEV._CHECK) != (M2MoverPr.CHECKEV)0)
						{
							this.checkCurrentPoint(true, true, true, true);
						}
						if (this.executeCheckEvent(this.Mp.TalkTarget) && this.Mp.talkCurrentFocus(this))
						{
							flag = true;
							this.talkToFocusExecuted();
						}
						if ((this.checkevent & M2MoverPr.CHECKEV._EXECUTE) != (M2MoverPr.CHECKEV)0)
						{
							flag = this.executeCheckStandEvent(true, true, true, true) || flag;
						}
					}
					else
					{
						if (this.Mp.TalkTarget != null)
						{
							this.Mp.setTalkTarget(null, false);
							this.checkevent |= M2MoverPr.CHECKEV.TALK_CHECK;
						}
						if ((this.checkevent & M2MoverPr.CHECKEV.STAND_CHECK) != (M2MoverPr.CHECKEV)0)
						{
							this.checkCurrentPoint(true, false, true, true);
						}
						if ((this.checkevent & M2MoverPr.CHECKEV.STAND_EXECUTE) != (M2MoverPr.CHECKEV)0)
						{
							flag = this.executeCheckStandEvent(true, false, true, true) || flag;
						}
					}
				}
				else
				{
					if (this.Mp.TalkTarget != null)
					{
						this.Mp.setTalkTarget(null, false);
						this.checkevent |= M2MoverPr.CHECKEV.TALK_CHECK;
					}
					if ((this.checkevent & M2MoverPr.CHECKEV.OTHER_BASEPR_CHECK) != (M2MoverPr.CHECKEV)0)
					{
						this.checkCurrentPoint(false, false, true, true);
					}
					if ((this.checkevent & M2MoverPr.CHECKEV.OTHER_BASEPR_CHECK) != (M2MoverPr.CHECKEV)0)
					{
						flag = this.executeCheckStandEvent(false, false, true, true) || flag;
					}
				}
			}
			else
			{
				if (this.Mp.TalkTarget != null)
				{
					this.Mp.setTalkTarget(null, false);
					this.checkevent |= M2MoverPr.CHECKEV.TALK_CHECK;
				}
				if ((this.checkevent & M2MoverPr.CHECKEV.OTHER_CHECK) != (M2MoverPr.CHECKEV)0)
				{
					this.checkCurrentPoint(false, false, false, true);
				}
				if ((this.checkevent & M2MoverPr.CHECKEV.OTHER_CHECK) != (M2MoverPr.CHECKEV)0)
				{
					flag = this.executeCheckStandEvent(false, false, false, true) || flag;
				}
			}
			return flag;
		}

		protected virtual void talkToFocusExecuted()
		{
			this.jump_hold_lock = true;
		}

		public bool forceCrouch(bool fine_flag = false, bool fine_size = false)
		{
			if (fine_flag || this.recheck_force_crouch)
			{
				int num = (int)base.x;
				int num2 = (int)(base.mbottom - 0.2f);
				this.recheck_force_crouch = false;
				if (base.canStand(num, num2) && this.checkForceCrouch(num))
				{
					this.t_force_crouch = 60f;
				}
				else if (this.t_force_crouch > 0f)
				{
					this.t_force_crouch = -15f;
				}
			}
			if (fine_size)
			{
				if (this.t_force_crouch == 0f)
				{
					if (this.bounds_ == M2MoverPr.BOUNDS_TYPE.CROUCH && !this.is_crouch)
					{
						this.setBounds(M2MoverPr.BOUNDS_TYPE.NORMAL, false);
					}
				}
				else if (this.t_force_crouch != 0f)
				{
					if (this.bounds_ == M2MoverPr.BOUNDS_TYPE.NORMAL)
					{
						this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
					}
					if (this.isNormalState())
					{
						if ((this.move_key & (M2MoverPr.MOVEK)40) != (M2MoverPr.MOVEK)0)
						{
							this.move_key |= M2MoverPr.MOVEK.CRAWL_L;
						}
						if ((this.move_key & (M2MoverPr.MOVEK)18) != (M2MoverPr.MOVEK)0)
						{
							this.move_key |= M2MoverPr.MOVEK.CRAWL_R;
						}
					}
					if (base.on_ladder)
					{
						this.FootD.initJump(false, true, true);
					}
					this.change_crouch = false;
				}
			}
			return this.t_force_crouch != 0f;
		}

		protected virtual bool checkForceCrouch(int cx)
		{
			if (this.Mp == null)
			{
				return false;
			}
			int num = (int)(base.mbottom - (float)this.size_y_default_pixel * this.Mp.rCLEN);
			if (base.canStand(cx, num))
			{
				return false;
			}
			if (base.hasFoot())
			{
				return this.FootD.get_FootBCC() != null;
			}
			float num2 = (float)(num + 1);
			return !CCON.canStandAndNoBlockSlope(this.Mp.getConfig(cx, (int)(num2 + (float)this.size_y_default_pixel * this.Mp.rCLEN)));
		}

		protected virtual void declineRunningEffect()
		{
		}

		public void recheckForceCrouch()
		{
			this.recheck_force_crouch = true;
		}

		public void stopRunning(bool check_key = false, bool from_event_close = false)
		{
			if ((!check_key || (from_event_close ? (!IN.isLO(0)) : (!this.isLO(0)))) && (this.running < 0f || (this.move_key & M2MoverPr.MOVEK.TO_L_RUN) != (M2MoverPr.MOVEK)0))
			{
				this.running = 0f;
				this.run_continue_time_ = 0f;
				this.run_delay = 0;
				this.lock_stick_run = 0f;
				this.Phy.clipWalkXSpeed();
				this.move_key &= (M2MoverPr.MOVEK)(-641);
			}
			if ((!check_key || (from_event_close ? (!IN.isRO(0)) : (!this.isRO(0)))) && (this.running > 0f || (this.move_key & M2MoverPr.MOVEK.TO_R_RUN) != (M2MoverPr.MOVEK)0))
			{
				this.run_delay = 0;
				this.running = 0f;
				this.run_continue_time_ = 0f;
				this.lock_stick_run = 0f;
				this.Phy.clipWalkXSpeed();
				this.move_key &= (M2MoverPr.MOVEK)(-321);
			}
		}

		public virtual void quitCrouch(bool delaying = false, bool no_reset_time = false, bool no_set_anim = false)
		{
			if (!this.canJump())
			{
				delaying = false;
			}
			if (this.crouching > 0f)
			{
				this.need_check_talk_event_target = true;
				this.recheck_force_crouch = true;
			}
			if (this.bounds_ == M2MoverPr.BOUNDS_TYPE.CROUCH)
			{
				this.setBounds(M2MoverPr.BOUNDS_TYPE.NORMAL, false);
			}
			if (delaying && this.crouching > 0f)
			{
				if (!no_set_anim)
				{
					this.SpSetPose("crouch2stand", -1, null, false);
				}
				this.crouching = 0f;
				return;
			}
			if (!no_reset_time)
			{
				this.crouching = 0f;
			}
		}

		public void clipCrouchTime(float t)
		{
			if (this.crouching > t)
			{
				this.crouching = t;
			}
		}

		protected virtual bool setBounds(M2MoverPr.BOUNDS_TYPE _bounds_, bool force = false)
		{
			if (this.t_force_crouch > 0f && _bounds_ == M2MoverPr.BOUNDS_TYPE.NORMAL)
			{
				_bounds_ = M2MoverPr.BOUNDS_TYPE.CROUCH;
			}
			if (_bounds_ == this.bounds_ && !force)
			{
				return false;
			}
			this.bounds_ = _bounds_;
			base.endDrawAssist(1);
			return true;
		}

		public override M2Mover setTo(float _x, float _y)
		{
			this.need_check_event = true;
			this.recheckForceCrouch();
			return base.setTo(_x, _y);
		}

		public virtual void checkCurrentPoint(bool check_stand, bool check_talk, bool check_basepr, bool check_other)
		{
			if (check_other)
			{
				this.checkevent = (this.checkevent & (M2MoverPr.CHECKEV)247) | M2MoverPr.CHECKEV.OTHER_EXECUTE;
			}
			int num = (int)this.event_cx;
			int num2 = (int)this.event_cy;
			if (check_basepr)
			{
				this.checkevent = (this.checkevent & (M2MoverPr.CHECKEV)251) | M2MoverPr.CHECKEV.OTHER_BASEPR_EXECUTE;
			}
			if (check_stand || check_talk)
			{
				this.Mp.getEventContainer().getNearEvents(this, num, num2, 0.8f, check_talk ? this.AATalkable : null, check_stand ? this.AAEvStand : null, 13U);
				if (check_stand)
				{
					this.checkevent = (this.checkevent & (M2MoverPr.CHECKEV)253) | M2MoverPr.CHECKEV.STAND_EXECUTE;
				}
				if (check_talk)
				{
					this.checkevent = (this.checkevent & (M2MoverPr.CHECKEV)254) | M2MoverPr.CHECKEV.TALK_EXECUTE;
				}
			}
		}

		public bool hasTalkableEventItem(IM2TalkableObject Tk)
		{
			for (int i = this.AATalkable.Count - 1; i >= 0; i--)
			{
				List<IM2TalkableObject> list = this.AATalkable[i];
				if (list != null && list.IndexOf(Tk) >= 0)
				{
					return true;
				}
			}
			return false;
		}

		public void blurEventTarget()
		{
		}

		protected bool executeCheckStandEvent(bool check_stand, bool check_talk, bool check_basepr, bool check_other)
		{
			if (check_basepr)
			{
				this.checkevent &= (M2MoverPr.CHECKEV)191;
			}
			if (check_other)
			{
				this.checkevent &= (M2MoverPr.CHECKEV)127;
			}
			bool flag = false;
			if (check_stand || check_talk)
			{
				if (check_talk)
				{
					this.has_talktarget_nearby = 2;
				}
				this.checkevent &= (M2MoverPr.CHECKEV)223;
				this.need_check_talk_event_target = this.Mp.getEventContainer().checkNearEventsExecution(this, out flag, (this.crouching > 0f) ? 3 : ((base.mpf_is_right > 0f) ? 2 : 0), 0.8f, check_talk ? this.AATalkable : null, check_stand ? this.AAEvStand : null, this.isFacingEnemy());
			}
			return flag;
		}

		public bool hasTalkTargetNearby()
		{
			if (this.has_talktarget_nearby >= 2)
			{
				this.has_talktarget_nearby = 0;
				if (this.AATalkable != null)
				{
					int num = this.AATalkable.Count - 1;
					while (num >= 0 && this.has_talktarget_nearby == 0)
					{
						List<IM2TalkableObject> list = this.AATalkable[num];
						if (list != null)
						{
							for (int i = list.Count - 1; i >= 0; i--)
							{
								if (list[i] != null && list[i] is M2EventItem && (list[i] as M2EventItem).hasTalk())
								{
									this.has_talktarget_nearby = 1;
									break;
								}
							}
						}
						num--;
					}
				}
			}
			return this.has_talktarget_nearby == 1;
		}

		public override void changeRiding(IFootable _PD, FOOTRES footres)
		{
			if (footres == FOOTRES.KEEP_FOOT || footres == FOOTRES.FOOTED)
			{
				if (_PD is M2BlockColliderContainer.BCCLine && (_PD as M2BlockColliderContainer.BCCLine).is_ladder && this.Mp.Pr == this)
				{
					IN.clearArrowPD(5U, false, 50);
				}
				if (this.last_foot_bottom_y_ != base.mbottom)
				{
					this.last_foot_bottom_y_ = base.mbottom;
					this.need_check_foot_camera_scroll = true;
				}
				if (footres == FOOTRES.FOOTED)
				{
					this.jump_pushing = -2f;
				}
				this.falling_camera_shift = -1000;
				if (footres == FOOTRES.FOOTED && this.Mp.floort >= 20f)
				{
					this.PadVib("foot", 1f);
				}
			}
			else
			{
				this.falling_camera_shift = 0;
			}
			base.changeRiding(_PD, footres);
		}

		public virtual void PadVib(string vib_key, float level = 1f)
		{
		}

		public virtual void eventGreeting(M2EventItem Ev, int aim, int shift_pixel_x)
		{
		}

		public float camera_shift_y
		{
			get
			{
				return (float)X.IntR((float)(-(float)this.size_y_default_pixel + 78) * ((this.Mp == null) ? 1f : this.Mp.M2D.Cam.getScaleRev()));
			}
		}

		public override void getCameraCenterPos(ref float posx, ref float posy, float shiftx, float shifty, bool immediate, ref float follow_speed)
		{
			M2Camera cam = this.Mp.M2D.Cam;
			shifty += (float)X.IntR((float)(-(float)this.size_y_default_pixel + 78) * cam.getScaleRev());
			float num = 1f;
			if ((cam.FocusTo != null && cam.FocusTo.focus_level_x > 0f) || this.isMoveScriptActive(false))
			{
				num = 0.33f;
			}
			if (this.isTrappedState() || !this.is_alive || this.isDamagingOrKo())
			{
				cam.blurCenterIfFocusing(this);
				this.pre_camera_shift_x = X.MULWALKMNA(this.pre_camera_shift_x, 0f, 0.025f * (float)X.AF, 1.2f * (float)X.AF);
				cam.blurCenterIfFocusing(this);
				base.getCameraCenterPos(ref posx, ref posy, shiftx, shifty + 30f * cam.getScaleRev(), immediate, ref follow_speed);
				posx += this.pre_camera_shift_x;
				this.pre_camera_y = posy;
				return;
			}
			if (this.pre_camera_y != -1000f)
			{
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC != null && !footBCC.is_ladder && this.Phy.walk_xspeed != 0f)
				{
					this.pre_camera_y += footBCC.line_a * this.Phy.walk_xspeed * base.CLEN * (float)X.AF;
				}
			}
			this.pre_camera_shift_x = X.MULWALKMNA(this.pre_camera_shift_x, this.Phy.walk_xspeed * 18f * num * base.CLEN, ((this.Phy.walk_xspeed != 0f) ? 0.035f : 0.01f) * (float)X.AF, ((this.Phy.walk_xspeed != 0f) ? 2.8f : 1f) * (float)X.AF);
			if (cam.FocusTo != null && cam.FocusTo.focus_level_y > 0f)
			{
				base.getCameraCenterPos(ref posx, ref posy, shiftx, shifty, immediate, ref follow_speed);
				posx += this.pre_camera_shift_x;
			}
			else
			{
				float num2 = this.drawy + shifty;
				float num3 = this.last_foot_bottom_y_ * base.CLEN - (float)this.size_y_default_pixel;
				shifty = 0f;
				float num4 = 0.03f;
				if (num3 > 0f)
				{
					if (num2 - 12f > num3)
					{
						num4 = 0.05f;
						num2 = X.Mn((num2 - num3) * 1.24f + num3, num2 + 150f * cam.getScaleRev());
					}
					else
					{
						num2 = (num2 - num3) * 0.5f + num3;
					}
					if (this.need_check_foot_camera_scroll)
					{
						X.Abs(cam.depy - num2);
						this.need_check_foot_camera_scroll = false;
					}
					if (this.falling_camera_shift != -1000 && this.falling_camera_shift < 0)
					{
						float num5 = X.ZLINE((float)(-(float)this.falling_camera_shift), 38f);
						num2 += num5 * base.CLEN * 1.4f;
						num4 = X.NI(1, 5, num5) * num4;
					}
				}
				posx = this.drawx + shiftx + this.pre_camera_shift_x;
				posy = ((this.pre_camera_y == -1000f || immediate) ? num2 : X.MULWALKMNA(this.pre_camera_y, num2, num4 * (float)X.AF, 1.6f * (float)X.AF));
			}
			if (this.crouching > 80f)
			{
				posy += X.ZLINE(this.crouching - 80f, 100f) * 6f * cam.getScaleRev();
			}
			posy += this.jump_cam_shift_y;
			this.pre_camera_y = posy;
		}

		public void vanishLink(M2EventItem Ev)
		{
			for (int i = 0; i < 2; i++)
			{
				List<M2EventItem> list = this.AAEvStand[i];
				for (int j = list.Count - 1; j >= 0; j--)
				{
					list.Remove(Ev);
				}
			}
		}

		public void vanishTalkableLink(IM2TalkableObject Ev)
		{
			if (this.AATalkable != null)
			{
				for (int i = this.AATalkable.Count - 1; i >= 0; i--)
				{
					List<IM2TalkableObject> list = this.AATalkable[i];
					if (list != null)
					{
						list.Remove(Ev);
					}
				}
			}
		}

		public override void evInit()
		{
			this.simulate_key = 0U;
			if (this.jump_starting)
			{
				this.jump_pushing = -2f;
			}
			base.evInit();
		}

		public override void evQuit()
		{
			this.simulate_key = 0U;
			if (this.MScr != null)
			{
				base.quitMoveScript(false);
			}
			base.evQuit();
		}

		public virtual Vector3 getHipPos()
		{
			return new Vector3(base.x, base.y + this.sizey * 0.3f, this.aim);
		}

		public override bool initDeath()
		{
			this.Phy.addLockMoverHitting(HITLOCK.DEATH, -1f);
			if (base.M2D.isCenterPlayer(this))
			{
				base.M2D.Cam.use_focus_area = false;
			}
			return true;
		}

		public virtual void simulateKeyDown(string k, bool flag = true)
		{
			if (flag)
			{
				this.simulate_key |= this.key2Bit(k);
				return;
			}
			if (k == "@")
			{
				this.simulate_key &= ~(this.key2Bit("L") | this.key2Bit("R"));
				return;
			}
			this.simulate_key &= ~this.key2Bit(k);
		}

		public void clearSimulateKeys()
		{
			this.simulate_key = 0U;
		}

		public bool hasSimulateKey()
		{
			return this.simulate_key > 0U;
		}

		public uint key2Bit(string k)
		{
			if (k == "0")
			{
				return 1U;
			}
			if (k == "@")
			{
				if (base.mpf_is_right <= 0f)
				{
					return 1U;
				}
				return 2U;
			}
			else
			{
				KEY.SIMKEY simkey;
				if (FEnum<KEY.SIMKEY>.TryParse(k.ToUpper(), out simkey, true))
				{
					return (uint)simkey;
				}
				return 0U;
			}
		}

		public override M2Mover setAim(AIM n, bool sprite_force_aim_set = false)
		{
			this.need_check_talk_event_target = true;
			return base.setAim(n, sprite_force_aim_set);
		}

		public virtual void cureEp(int val)
		{
			this.ep = X.Mx(this.ep - val, 0);
		}

		public virtual bool canOpenCheckEvent(IM2TalkableObject Tk)
		{
			return Map2d.can_handle && (!(Tk is M2EventItem) || base.hasFoot());
		}

		public virtual bool isRO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.R, Map2d.can_handle, false))
			{
				return (this.simulate_key & 2U) > 0U;
			}
			return IN.isRO(press);
		}

		public virtual bool isLO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.L, Map2d.can_handle, false))
			{
				return (this.simulate_key & 1U) > 0U;
			}
			return IN.isLO(press);
		}

		public virtual bool isRunningRO(out bool stick_over)
		{
			stick_over = false;
			if (IN.isPadMode() && EV.lockPrInputManipulate(KEY.SIMKEY.R, Map2d.can_handle, false))
			{
				stick_over = IN.getCurrentKeyAssignObject().isRunningInput(M2MoverPr.running_thresh, true);
				return stick_over || this.lock_stick_run > 0f;
			}
			return false;
		}

		public virtual bool isRunningLO(out bool stick_over)
		{
			stick_over = false;
			if (IN.isPadMode() && EV.lockPrInputManipulate(KEY.SIMKEY.L, Map2d.can_handle, false))
			{
				stick_over = IN.getCurrentKeyAssignObject().isRunningInput(M2MoverPr.running_thresh, false);
				return stick_over || this.lock_stick_run < 0f;
			}
			return false;
		}

		public virtual bool isTO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.T, Map2d.can_handle, false))
			{
				return (this.simulate_key & 4U) > 0U;
			}
			return IN.isTO(press);
		}

		public virtual bool isBO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.B, Map2d.can_handle, false))
			{
				return (this.simulate_key & 8U) > 0U;
			}
			return IN.isBO(press);
		}

		public virtual bool isJumpO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.T, Map2d.can_handle, false))
			{
				return (this.simulate_key & 4U) > 0U;
			}
			return IN.isJumpO(press);
		}

		public virtual bool isRunO()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.RUN, Map2d.can_handle, false) && IN.isRunO(0);
		}

		public virtual bool isRU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.R, Map2d.can_handle, false) && IN.isRU();
		}

		public virtual bool isLU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.L, Map2d.can_handle, false) && IN.isLU();
		}

		public virtual bool isRU(int holded_alloc, bool kill_flag = true)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.R, Map2d.can_handle, false) && IN.isRU(holded_alloc, kill_flag);
		}

		public virtual bool isLU(int holded_alloc, bool kill_flag = true)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.L, Map2d.can_handle, false) && IN.isLU(holded_alloc, kill_flag);
		}

		public virtual bool isTU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.T, Map2d.can_handle, false) && IN.isTU();
		}

		public virtual bool isBU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.B, Map2d.can_handle, false) && IN.isBU();
		}

		public virtual bool isJumpU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.T, Map2d.can_handle, false) && IN.isJumpU();
		}

		public virtual bool isRP(int press_max = 1)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.R, Map2d.can_handle, false) && IN.isRP(press_max);
		}

		public virtual bool isLP(int press_max = 1)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.L, Map2d.can_handle, false) && IN.isLP(press_max);
		}

		public virtual bool isTP(int press_max = 1)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.T, Map2d.can_handle, false) && IN.isTP(press_max);
		}

		public virtual bool isBP()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.B, Map2d.can_handle, false) && IN.isBP(1);
		}

		public bool isMovingPD()
		{
			return this.isLP(1) || this.isRP(1) || this.isBP() || this.isTP(1) || this.isJumpPD(1);
		}

		public bool isCanselableMovingPD()
		{
			return this.isBP() || this.isTP(1) || this.isJumpPD(1);
		}

		public bool isCancelU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.CANCEL, Map2d.can_handle, false) && IN.isCancelU();
		}

		public bool isActionPD()
		{
			return !IN.isMenuO(0) && (this.isMovingPD() || this.isAtkPD(1) || this.isSubmitPD(1) || this.isMagicPD(1) || this.isEvadePD(1) || this.isCheckPD(1));
		}

		public bool isMovingO(int press = 0)
		{
			return this.isLO(press) || this.isRO(press) || this.isBO(press) || this.isTO(press) || this.isJumpO(press);
		}

		public bool isMovingU()
		{
			return this.isLU() || this.isRU() || this.isBU() || this.isTU() || this.isJumpU();
		}

		public bool isActionO(int press = 0)
		{
			return !IN.isMenuO(0) && (this.isMovingO(press) || this.isSubmitO(0) || this.isCheckO(0) || this.isAtkO(press) || this.isMagicO(press) || this.isEvadeO(press));
		}

		public bool isMenuPD(int alloc_frame = 1)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.MENU, Map2d.can_handle, false))
			{
				return (this.simulate_key & 8192U) > 0U;
			}
			return IN.isMenuPD(alloc_frame);
		}

		public bool isMenuO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.MENU, Map2d.can_handle, false))
			{
				return (this.simulate_key & 8192U) > 0U;
			}
			return IN.isMenuO(press);
		}

		public bool isMapPD(int alloc_frame = 1)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.MAP, Map2d.can_handle, false))
			{
				return (this.simulate_key & 16384U) > 0U;
			}
			return IN.isMapPD(alloc_frame);
		}

		public bool isMapO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.MAP, Map2d.can_handle, false))
			{
				return (this.simulate_key & 16384U) > 0U;
			}
			return IN.isMapO(press);
		}

		public bool isItmPD()
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.ITEM, Map2d.can_handle, false))
			{
				return (this.simulate_key & 32768U) > 0U;
			}
			return IN.isItmPD(1);
		}

		public bool isItmO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.ITEM, Map2d.can_handle, false))
			{
				return (this.simulate_key & 32768U) > 0U;
			}
			return IN.isItmO(press);
		}

		public bool isItmU(int overhold_time = 1)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.ITEM, Map2d.can_handle, false))
			{
				return (this.simulate_key & 32768U) > 0U;
			}
			return IN.isItmU(overhold_time);
		}

		public bool isSubmitPD(int alloc_pd_frame = 1)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.SUBMIT, Map2d.can_handle, false))
			{
				return (this.simulate_key & 512U) > 0U;
			}
			return IN.isSubmitPD(alloc_pd_frame);
		}

		public bool isCancelO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.CANCEL, Map2d.can_handle, false))
			{
				return (this.simulate_key & 1024U) > 0U;
			}
			return IN.isCancelOn(press);
		}

		public bool isMagicPD(int alloc_frame = 1)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && IN.isMagicPD(alloc_frame);
		}

		public bool isTargettingPD(int alloc_frame = 1)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.C, Map2d.can_handle, false) && IN.isTargettingPD(alloc_frame);
		}

		public bool isTargettingO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.C, Map2d.can_handle, false))
			{
				return (this.simulate_key & 64U) > 0U;
			}
			return IN.isTargettingO(press);
		}

		public virtual bool isJumpPD(int alloc_frame = 1)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.T, Map2d.can_handle, false))
			{
				return (this.simulate_key & 4U) > 0U;
			}
			return IN.isJumpPD(alloc_frame);
		}

		public bool isMagicO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false))
			{
				return (this.simulate_key & 32U) > 0U;
			}
			return IN.isMagicO(press);
		}

		public bool isMagicU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && IN.isMagicU();
		}

		public bool isSubmitO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.SUBMIT, Map2d.can_handle, false))
			{
				return (this.simulate_key & 512U) > 0U;
			}
			return IN.isSubmitOn(press);
		}

		public bool isAtkPD(int alloc_frame = 1)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.Z, Map2d.can_handle, false))
			{
				return (this.simulate_key & 16U) > 0U;
			}
			return IN.isAtkPD(alloc_frame);
		}

		public bool isAtkU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.Z, Map2d.can_handle, false) && IN.isAtkU();
		}

		public bool isAtkO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.Z, Map2d.can_handle, false))
			{
				return (this.simulate_key & 16U) > 0U;
			}
			return IN.isAtkO(press);
		}

		public bool isRunO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.RUN, Map2d.can_handle, false))
			{
				return (this.simulate_key & 65536U) > 0U;
			}
			return IN.isRunO(press);
		}

		public bool isEvadePD(int alloc_frame = 1)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.LSH, Map2d.can_handle, false) && IN.isEvadePD(alloc_frame);
		}

		public bool isEvadeU()
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.LSH, Map2d.can_handle, false) && IN.isEvadeU();
		}

		public bool isEvadeO(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.LSH, Map2d.can_handle, false) && IN.isEvadeO(press);
		}

		public bool isCheckPD(int alloc_frame = 1)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.CHECK, Map2d.can_handle, false))
			{
				return (this.simulate_key & 4194304U) > 0U;
			}
			return IN.isCheckPD(alloc_frame);
		}

		public bool isCheckO(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.CHECK, Map2d.can_handle, false))
			{
				return (this.simulate_key & 4194304U) > 0U;
			}
			return IN.isCheckO(press);
		}

		public bool isCheckU(int press = 0)
		{
			if (!EV.lockPrInputManipulate(KEY.SIMKEY.CHECK, Map2d.can_handle, false))
			{
				return (this.simulate_key & 4194304U) > 0U;
			}
			return IN.isCheckU();
		}

		public bool isMagicStickO(int press = 0)
		{
			return this.isMagicNeutralO(0) || (EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && EV.lockPrInputManipulate(KEY.SIMKEY.L | KEY.SIMKEY.R | KEY.SIMKEY.T | KEY.SIMKEY.B, Map2d.can_handle, true) && (IN.isMagicLO(0) || IN.isMagicRO(0) || IN.isMagicBO(0) || IN.isMagicTO(0)));
		}

		public bool isMagicStickPD(int press = 0)
		{
			return this.isMagicNeutralPD(0) || (EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && EV.lockPrInputManipulate(KEY.SIMKEY.L | KEY.SIMKEY.R | KEY.SIMKEY.T | KEY.SIMKEY.B, Map2d.can_handle, true) && (IN.isMagicLPD(1) || IN.isMagicRPD(1) || IN.isMagicBPD(1) || IN.isMagicTPD(1)));
		}

		public bool isMagicNeutralPD(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && IN.isMagicNeutralPD(press);
		}

		public bool isMagicNeutralO(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && IN.isMagicNeutralO(press);
		}

		public bool isMagicLPD(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && EV.lockPrInputManipulate(KEY.SIMKEY.L | KEY.SIMKEY.R, Map2d.can_handle, true) && IN.isMagicLPD(press);
		}

		public bool isMagicLO(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && EV.lockPrInputManipulate(KEY.SIMKEY.L | KEY.SIMKEY.R, Map2d.can_handle, true) && IN.isMagicLO(press);
		}

		public bool isMagicRPD(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && EV.lockPrInputManipulate(KEY.SIMKEY.L | KEY.SIMKEY.R, Map2d.can_handle, true) && IN.isMagicRPD(press);
		}

		public bool isMagicRO(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false) && EV.lockPrInputManipulate(KEY.SIMKEY.L | KEY.SIMKEY.R, Map2d.can_handle, true) && IN.isMagicRO(press);
		}

		public bool isMagicBPD(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.B | KEY.SIMKEY.X, Map2d.can_handle, false) && IN.isMagicBPD(press);
		}

		public bool isMagicBO(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.B | KEY.SIMKEY.X, Map2d.can_handle, false) && IN.isMagicBO(press);
		}

		public bool isMagicTPD(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.T | KEY.SIMKEY.X, Map2d.can_handle, false) && IN.isMagicTPD(press);
		}

		public bool isMagicTO(int press = 0)
		{
			return EV.lockPrInputManipulate(KEY.SIMKEY.T | KEY.SIMKEY.X, Map2d.can_handle, false) && IN.isMagicTO(press);
		}

		public virtual bool isManipulateState()
		{
			return this.is_alive;
		}

		public bool hasRightInput()
		{
			return (this.move_key & M2MoverPr.MOVEK._RIGHT) > (M2MoverPr.MOVEK)0;
		}

		public bool hasLeftInput()
		{
			return (this.move_key & M2MoverPr.MOVEK._LEFT) > (M2MoverPr.MOVEK)0;
		}

		public KEY.SIMKEY getKeyPDState()
		{
			return (this.isLP(1) ? KEY.SIMKEY.L : ((KEY.SIMKEY)0)) | (this.isRP(1) ? KEY.SIMKEY.R : ((KEY.SIMKEY)0)) | (this.isTP(1) ? KEY.SIMKEY.T : ((KEY.SIMKEY)0)) | (this.isBP() ? KEY.SIMKEY.B : ((KEY.SIMKEY)0)) | (this.isAtkPD(1) ? KEY.SIMKEY.Z : ((KEY.SIMKEY)0)) | (this.isMagicPD(1) ? KEY.SIMKEY.X : ((KEY.SIMKEY)0)) | (this.isTargettingPD(1) ? KEY.SIMKEY.C : ((KEY.SIMKEY)0));
		}

		public KEY.SIMKEY getKeyPOState()
		{
			return (this.isLO(0) ? KEY.SIMKEY.L : ((KEY.SIMKEY)0)) | (this.isRO(0) ? KEY.SIMKEY.R : ((KEY.SIMKEY)0)) | (this.isTO(0) ? KEY.SIMKEY.T : ((KEY.SIMKEY)0)) | (this.isBO(0) ? KEY.SIMKEY.B : ((KEY.SIMKEY)0)) | (this.isAtkO(0) ? KEY.SIMKEY.Z : ((KEY.SIMKEY)0)) | (this.isMagicO(0) ? KEY.SIMKEY.X : ((KEY.SIMKEY)0)) | (this.isTargettingO(0) ? KEY.SIMKEY.C : ((KEY.SIMKEY)0));
		}

		public static KEY.SIMKEY key2SIMKEY(string k)
		{
			string text = k.ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 3322673650U)
				{
					if (num <= 1742883422U)
					{
						if (num != 401953830U)
						{
							if (num != 467490188U)
							{
								if (num != 1742883422U)
								{
									return (KEY.SIMKEY)0;
								}
								if (!(text == "LA"))
								{
									return (KEY.SIMKEY)0;
								}
							}
							else
							{
								if (!(text == "BA"))
								{
									return (KEY.SIMKEY)0;
								}
								return KEY.SIMKEY.B;
							}
						}
						else
						{
							if (!(text == "TA"))
							{
								return (KEY.SIMKEY)0;
							}
							return KEY.SIMKEY.T;
						}
					}
					else if (num != 2080701468U)
					{
						if (num != 3009237600U)
						{
							if (num != 3322673650U)
							{
								return (KEY.SIMKEY)0;
							}
							if (!(text == "C"))
							{
								return (KEY.SIMKEY)0;
							}
							return KEY.SIMKEY.C;
						}
						else
						{
							if (!(text == "LSH"))
							{
								return (KEY.SIMKEY)0;
							}
							return KEY.SIMKEY.LSH;
						}
					}
					else
					{
						if (!(text == "RA"))
						{
							return (KEY.SIMKEY)0;
						}
						return KEY.SIMKEY.R;
					}
				}
				else if (num <= 3507227459U)
				{
					if (num != 3339451269U)
					{
						if (num != 3373006507U)
						{
							if (num != 3507227459U)
							{
								return (KEY.SIMKEY)0;
							}
							if (!(text == "T"))
							{
								return (KEY.SIMKEY)0;
							}
							return KEY.SIMKEY.T;
						}
						else if (!(text == "L"))
						{
							return (KEY.SIMKEY)0;
						}
					}
					else
					{
						if (!(text == "B"))
						{
							return (KEY.SIMKEY)0;
						}
						return KEY.SIMKEY.B;
					}
				}
				else if (num <= 3708558887U)
				{
					if (num != 3607893173U)
					{
						if (num != 3708558887U)
						{
							return (KEY.SIMKEY)0;
						}
						if (!(text == "X"))
						{
							return (KEY.SIMKEY)0;
						}
						return KEY.SIMKEY.X;
					}
					else
					{
						if (!(text == "R"))
						{
							return (KEY.SIMKEY)0;
						}
						return KEY.SIMKEY.R;
					}
				}
				else if (num != 3742114125U)
				{
					if (num != 3931159262U)
					{
						return (KEY.SIMKEY)0;
					}
					if (!(text == "ESC"))
					{
						return (KEY.SIMKEY)0;
					}
					return KEY.SIMKEY.ESC;
				}
				else
				{
					if (!(text == "Z"))
					{
						return (KEY.SIMKEY)0;
					}
					return KEY.SIMKEY.Z;
				}
				return KEY.SIMKEY.L;
			}
			return (KEY.SIMKEY)0;
		}

		public virtual bool executeCheckEvent(IM2TalkableObject Tk)
		{
			return Tk != null && this.canOpenCheckEvent(Tk);
		}

		public override float getSpShiftX()
		{
			return base.getSpShiftX() + (float)((int)(this.offset_pixel_x / this.Mp.base_scale));
		}

		public override float getSpShiftY()
		{
			return base.getSpShiftY() + (float)((int)(this.offset_pixel_y / this.Mp.base_scale));
		}

		public virtual bool cannotAccessToCheckEvent()
		{
			return this.isDamagingOrKo();
		}

		public virtual bool isBusySituation()
		{
			return this.isDamagingOrKo();
		}

		public virtual bool isFacingEnemy()
		{
			return false;
		}

		public virtual void runInFloorPausing()
		{
		}

		public virtual bool isGaraakiState()
		{
			return !this.is_alive;
		}

		public virtual bool is_crouch
		{
			get
			{
				return this.crouching > 0f || this.t_force_crouch > 0f;
			}
			set
			{
				if (!value && !this.forceCrouch(false, false))
				{
					this.crouching = 0f;
				}
			}
		}

		public bool view_crouching
		{
			get
			{
				return this.crouching > 0f || this.t_force_crouch != 0f;
			}
		}

		public virtual bool isNormalState()
		{
			return true;
		}

		public bool isMoveStartPushedDown()
		{
			return (this.move_key & (M2MoverPr.MOVEK)5) > (M2MoverPr.MOVEK)0;
		}

		public bool isMoveRightOn()
		{
			return (this.move_key & M2MoverPr.MOVEK._RIGHT) > (M2MoverPr.MOVEK)0;
		}

		public bool isMoveLeftOn()
		{
			return (this.move_key & M2MoverPr.MOVEK._LEFT) > (M2MoverPr.MOVEK)0;
		}

		public bool isRunning()
		{
			return this.running != 0f;
		}

		public bool isRunningCurrent()
		{
			return X.Abs(this.running) == 1f;
		}

		public bool isRunStopping()
		{
			return X.Abs(this.running) >= 2f;
		}

		public float run_continue_time
		{
			get
			{
				return this.run_continue_time_;
			}
		}

		public virtual bool cannotRun(bool starting)
		{
			return starting && (this.FootD == null || !this.FootD.canStartRunning());
		}

		public override bool considerFricOnVelocityCalc()
		{
			return base.considerFricOnVelocityCalc() || (this.isNormalState() && ((this.manip & M2MoverPr.PR_MNP.NO_MOVING_B) != (M2MoverPr.PR_MNP)0 || X.Abs(this.Phy.release_velocity_x) < 0.18f));
		}

		public override IFootable checkSkipLift(M2BlockColliderContainer.BCCLine _P)
		{
			IFootable footable = base.checkSkipLift(_P);
			if (footable == null || footable != _P)
			{
				return footable;
			}
			_P = (((this.move_key & M2MoverPr.MOVEK.THROUGH_LIFT) != (M2MoverPr.MOVEK)0) ? null : _P);
			if (_P == null)
			{
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (this.FootD.LadderBCC != null && footBCC != null && footBCC.is_lift && !footBCC.is_ladder && X.Abs(this.FootD.LadderBCC.shifted_cx - base.x) < 0.65f && X.BTW(this.FootD.LadderBCC.shifted_y + 1.5f, base.mbottom, X.Mn(this.FootD.LadderBCC.sbottom, this.FootD.LadderBCC.sbottom - 1f)))
				{
					this.climbToTopLiftFromLadder(this.FootD.LadderBCC, true);
					return this.FootD.LadderBCC;
				}
			}
			return _P;
		}

		public override HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.PR;
		}

		public override float last_foot_bottom_y
		{
			get
			{
				return this.last_foot_bottom_y_;
			}
		}

		public virtual bool isTrappedState()
		{
			return false;
		}

		public override bool isDamagingOrKo()
		{
			return false;
		}

		public virtual bool isCalcableThroughLiftState()
		{
			return this.isMoveScriptActive(false) || this.isNormalState();
		}

		public void setOffsetPixel(float x = -1000f, float y = -1000f)
		{
			this.offset_pixel_x = ((x != -1000f) ? x : this.offset_pixel_x);
			this.offset_pixel_y = ((y != -1000f) ? y : this.offset_pixel_y);
		}

		public M2MoverPr.MOVEK getMoveKey()
		{
			return this.move_key;
		}

		public bool move_inputting_anything
		{
			get
			{
				return (this.move_key & M2MoverPr.MOVEK._TO) > (M2MoverPr.MOVEK)0;
			}
		}

		public const float RUNNING_STICK_THRESH_DEFAULT = 0.8f;

		public static float running_thresh = 0.8f;

		public static bool double_tap_running = true;

		public static bool jump_press_reverse = false;

		private const float jump_ratio_on_web = 0.5f;

		protected float walkSpeed = 0.085f;

		protected float runSpeed = 0.17f;

		public float ySpeedMax0 = 0.19f;

		protected float ySpeedStart = -0.298f;

		protected float ySpeedStart_water = -0.158f;

		protected float ySpeedKeyReleased_water = -0.35f;

		private const int JUMP_HOLD_MAXT = 2;

		protected int JUMP_HOLD_MAXT_WATER = 60;

		protected const float ySpeedKeyReleased = -0.06695f;

		protected float accel_run_break = 0.0063f;

		protected float accel_run_break_air = 0.0044f;

		public const float XSPD_MP_REDUCE = 0.75f;

		public const int ladder_access_t = 10;

		private const float ladder_margin_top = 0.1f;

		private bool ladder_check;

		public int falling_camera_shift;

		private const float FALLING_CAM_SHIFT_MAP_Y = 1.4f;

		private const int FALLING_CAM_SHIFT_MAXT = 38;

		private float pre_jump_velocity_y;

		private bool jump_hold_lock_ = true;

		private float jump_pushing;

		protected float jump_cam_shift_y;

		private const float talkable_len = 0.8f;

		private const int RUNNING_HOLD_BIT = 32;

		private const int RUNNING_HOLD_ERROR = 192;

		private const int RUN_BREAK_DELAY = 48;

		private const int RUN_SLIP_TIME = 13;

		protected List<List<IM2TalkableObject>> AATalkable;

		protected List<List<M2EventItem>> AAEvStand;

		public int enemy_targetted;

		public uint simulate_key;

		private bool pre_ladder_shifting;

		public int ep;

		public M2MoverPr.PR_MNP manip;

		private M2MoverPr.MOVEK move_key;

		private const int RUN_DELAY = 18;

		public M2Light MyLight;

		protected float last_foot_bottom_y_;

		protected bool need_check_foot_camera_scroll = true;

		protected float pre_camera_shift_x;

		protected float pre_camera_y = -1000f;

		public float offset_pixel_x;

		public float offset_pixel_y;

		protected int size_y_default_pixel;

		private bool recheck_force_crouch = true;

		protected float t_force_crouch;

		protected const float MAXT_FORCE_CROUCH = 15f;

		protected bool change_crouch;

		protected byte has_talktarget_nearby = 2;

		protected bool outfit_cannot_run;

		private const float footing_clip_x = 0.18f;

		protected M2MoverPr.CHECKEV checkevent;

		protected M2MoverPr.BOUNDS_TYPE bounds_;

		private int run_delay;

		public float lock_stick_run;

		protected float base_gravity0;

		protected float crouching;

		private float running_;

		protected float run_continue_time_;

		public byte target_calced;

		private Func<M2BlockColliderContainer.BCCLine, bool> FindBccLadderClimbOut;

		protected enum CHECKEV : byte
		{
			TALK_CHECK = 1,
			STAND_CHECK,
			OTHER_BASEPR_CHECK = 4,
			OTHER_CHECK = 8,
			TALK_EXECUTE = 16,
			STAND_EXECUTE = 32,
			OTHER_BASEPR_EXECUTE = 64,
			OTHER_EXECUTE = 128,
			_CHECK = 15,
			_EXECUTE = 240,
			_ALL = 255
		}

		public enum PR_MNP
		{
			NO_RUN = 1,
			NO_SINK,
			NO_JUMP = 4,
			NO_MOVING_B = 8,
			NO_MOVING,
			NO_JUMP_AND_MOVING = 13,
			NO_MOVE_INIT = 16,
			NO_CROUCH = 32,
			NO_MOVE = 61,
			CHANGED_STATE = 134217728
		}

		public enum MOVEK
		{
			KEYP_R = 1,
			KEYD_R,
			KEYP_L = 4,
			KEYD_L = 8,
			_KEYS = 15,
			TO_R,
			TO_L = 32,
			TO_R_RUN = 64,
			TO_L_RUN = 128,
			_TO = 240,
			_TO_WALK = 48,
			_TO_RUN = 192,
			TO_R_DT_RUN = 256,
			TO_L_DT_RUN = 512,
			TO_R2 = 336,
			TO_L2 = 672,
			CRAWL_R = 1024,
			CRAWL_L = 2048,
			_CRAWL = 3072,
			_RIGHT = 1107,
			_LEFT = 2220,
			THROUGH_LIFT = 4096
		}

		protected enum BOUNDS_TYPE
		{
			OFFLINE,
			NORMAL,
			CROUCH,
			CROUCH_WIDE,
			DOWN,
			DAMAGE_L,
			PRESSCROUCH,
			AUTO
		}

		public enum DECL
		{
			STOP_ACT = 1,
			STOP_EVADE,
			STOP_MG = 4,
			STOP_SHIELD = 8,
			STOP_COMMAND = 15,
			STOP_SPECIAL_OUTFIT = 271,
			EVENT = 16,
			WATER_CHOKE_FLOAT = 32,
			NO_PROGRESS_SHIELD_POWER = 64,
			DO_NOT_CARRY_BY_OTHER = 128,
			STOP_BURST = 256,
			THROW_RAY = 512,
			ABORT_BY_MOVE = 1024,
			STOP_FOOTSND = 2048,
			EVENT_THROW_RAY = 4096,
			WEAK_THROW_RAY = 8192,
			ALLOC_SHIELD_HOLD = 16384,
			FORCE_SHIELD_KEY_HOLD = 32768,
			CANNOT_EXECUTE_CHECKTARGET = 65536,
			ORGASM_INJECTABLE = 131072,
			NO_USE_ITEM = 262144,
			FLAG_PD = 1048576,
			FLAG_PRESS = 2097152,
			FLAG_HIT = 4194304,
			FLAG0 = 16777216,
			FLAG1 = 33554432,
			FLAG2 = 67108864,
			INIT_A = 134217728,
			DO_NOT_RESET_POS_ON_APPEAR = 8388608
		}
	}
}
