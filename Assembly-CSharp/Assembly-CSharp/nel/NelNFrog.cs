using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNFrog : NelEnemy
	{
		public override void appear(Map2d _Mp)
		{
			this.AtkIceBarn = new NelAttackInfo();
			this.AtkIceBarn.CopyFrom(this.AtkIce);
			this.AtkIceBarn.burst_center = 1f;
			this.AtkIceBarn.nodamage_time = 40;
			this.HandlerPE = new EffectHandlerPE(3);
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 14f;
			ENEMYID id = this.id;
			this.id = ENEMYID.FROG_0;
			NOD.BasicData basicData = NOD.getBasicData("FROG_0");
			base.appear(_Mp, basicData);
			this.PxlWhite = this.Anm.getCurrentCharacter().getPoseByName("_eye").getSequence(0)
				.getFrame(0)
				.getLayer(0);
			this.EadTongue = new EnemyAnimator.EnemyAdditionalDrawFrame(null, new EnemyAnimator.EnemyAdditionalDrawFrame.FnDrawEAD(this.fnDrawTongue), false);
			this.Anm.addAdditionalDrawer(this.EadTongue);
			this.Anm.checkframe_on_drawing = false;
			this.Phy.water_speed_scale = 1f;
			this.Nai.pl_magic_awake_time_min = 4f;
			this.Nai.pl_magic_awake_time_max = 7f;
			this.Nai.awake_length = num;
			this.Nai.suit_distance = 4f;
			this.Nai.attackable_length_x = 6f;
			this.Nai.attackable_length_top = -7f;
			this.Nai.attackable_length_bottom = 5f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.auto_rot_on_damage = true;
			this.ChkStasis = new NASStasisChecker(this, 80f, 14);
			this.ChkStasis.randomize_max = 2f;
			this.Nai.busy_consider_intv = 2f;
			this.absorb_weight = 4;
			this.SheetTongue = new RollSeetDrawer();
			this.SheetTongue.block_count_x = 0;
			this.SheetTongue.block_count_y = 7;
			if (NelNFrog.BuryEvent == null)
			{
				NelNFrog.BuryEvent = new NelNFrog.GachaGroundBury();
			}
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			this.HandlerPE.deactivate(false);
			if (this.SndIntvStomach != null)
			{
				this.SndIntvStomach.Stop();
				this.SndIntvStomach = null;
			}
			base.destruct();
		}

		public override void initOverDriveAppear()
		{
			base.initOverDriveAppear();
			this.cannot_move = false;
			this.absorb_weight = 5;
		}

		public override void quitOverDrive()
		{
			base.quitOverDrive();
			this.absorb_weight = 1;
		}

		public override void runPre()
		{
			if (base.destructed)
			{
				return;
			}
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (base.isAbsorbState())
			{
				this.HandlerPE.fine(100);
			}
			if (this.EatBombMg != null)
			{
				Vector2 tongueStartMapPos = this.getTongueStartMapPos();
				float num = 9.6f * base.scaleX * this.tongue_level;
				this.EatBombMg.sx = tongueStartMapPos.x + X.Cos(this.tng_agR) * num;
				this.EatBombMg.sy = tongueStartMapPos.y - X.Sin(this.tng_agR) * num;
				float num2 = this.tng_agR + 3.1415927f;
				this.EatBombMg.sa = num2 + X.angledifR(num2, 3.1415927f - base.mpf_is_right * 3.1415927f * 0.58f) * (1f - this.tongue_level);
			}
			this.ChkStasis.run(this.TS);
		}

		public override NelEnemy changeState(NelEnemy.STATE stt)
		{
			if (this.state == NelEnemy.STATE.ABSORB)
			{
				base.carryable_other_object = false;
				this.EadTongue.active = false;
				this.HandlerPE.deactivate(false);
				this.Nai.RemF(NAI.FLAG.BOTHERED);
				this.Nai.addTypeLock(NAI.TYPE.PUNCH, 150f);
				if (this.SndIntvStomach != null)
				{
					this.SndIntvStomach.Stop();
					this.SndIntvStomach = null;
				}
			}
			if (stt == NelEnemy.STATE.STAND)
			{
				this.Anm.rotationR = 0f;
				this.ChkStasis.Clear();
			}
			return base.changeState(stt);
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai))
			{
				return true;
			}
			if (Nai.HasF(NAI.FLAG.POWERED, true))
			{
				this.SpSetPose("bury_after", -1, null, false);
				Nai.delay = 120f;
				return true;
			}
			if (Nai.HasF(NAI.FLAG.ABSORB_FINISHED, true))
			{
				return Nai.AddTicketB(NAI.TYPE.WALK, 2, true);
			}
			if (!Nai.hasPT(2, false) && (Nai.isPrAbsorbedCannotMove() || (this.ChkStasis.isStasis(false) && Nai.RANtk(1421) < 0.12f)))
			{
				if (Nai.target_sxdif > 0.5f && Nai.isAttackableLength(7f, -1f, 1.5f, false))
				{
					return Nai.AddTicketB((Nai.RANtk(335) < (Nai.isPrAlive() ? 0.01f : 0.2f)) ? NAI.TYPE.PUNCH : NAI.TYPE.PUNCH_0, 2, true);
				}
				float num = Nai.NIRANtk(3f, 4.5f, 4922) + this.sizex;
				float mpfPlayerSide = Nai.getMpfPlayerSide(0f, 0f, 0f, num, 0f, false);
				if (Nai.AddMoveTicketFor(Nai.target_x + num * mpfPlayerSide, Nai.target_y, null, 2, true, NAI.TYPE.WALK_TO_WEED) != null)
				{
					return true;
				}
			}
			bool flag;
			if ((Nai.isPrMagicExploded(1f) || Nai.RANtk(3228) < 0.12f) && Nai.autotargetted_me)
			{
				Nai.AddF(NAI.FLAG.BOTHERED, 140f);
				flag = true;
			}
			else
			{
				flag = Nai.HasF(NAI.FLAG.BOTHERED, false);
			}
			if (!Nai.hasPT(100, false))
			{
				if (!Nai.hasPT(2, false))
				{
					if (Nai.target_sxdif > 7f)
					{
						if (!Nai.hasTypeLock(NAI.TYPE.WALK))
						{
							return Nai.AddTicketB(NAI.TYPE.WALK, 2, true);
						}
					}
					else if (this.ChkStasis.isStasis(true) && !Nai.hasTypeLock(NAI.TYPE.PUNCH))
					{
						return Nai.AddTicketB(NAI.TYPE.PUNCH, 100, true);
					}
				}
				if ((Nai.autotargetted_me && Nai.RANtk(3228) < 0.12f) || flag || (Nai.isPrSpecialAttacking() && Nai.isAttackableLength(3.5f, -3f, 3f, false)) || (Nai.isPrAttacking() && Nai.isAttackableLength(1.5f, -2f, 1f, false)))
				{
					return Nai.AddTicketB(NAI.TYPE.BACKSTEP, 100, true);
				}
			}
			return Nai.AddTicketB(NAI.TYPE.WAIT, 1, true);
		}

		public bool considerGazing(NaTicket Tk)
		{
			bool flag = this.Nai.target_sxdif < 1.5f && X.Abs(this.Nai.target_y - base.y - -5f) < 3.2f && (this.Nai.isPrCometTargetting(true) || this.Nai.RANtk(3181) < 0.33f);
			if (!flag)
			{
				flag = this.Nai.isPrChantCompleted(0.7f) && this.Nai.isAttackableLength(2f, -1f, 1f, false);
			}
			if (flag)
			{
				Tk.Recreate(NAI.TYPE.BACKSTEP, 100, false, null);
				return this.runBackStep(Tk.initProgress(this), Tk);
			}
			return false;
		}

		private bool considerOverDrive(NAI Nai)
		{
			return true;
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
			case NAI.TYPE.WALK_TO_WEED:
				return this.runWalk(Tk.initProgress(this), Tk, Tk.type == NAI.TYPE.WALK_TO_WEED);
			case NAI.TYPE.PUNCH:
				return this.runPunch_Tongue(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_0:
				return this.runPunch0_TongueInGround(Tk.initProgress(this), Tk);
			default:
				if (type == NAI.TYPE.BACKSTEP)
				{
					return this.runBackStep(Tk.initProgress(this), Tk);
				}
				if (type - NAI.TYPE.GAZE <= 1)
				{
					return this.runGaze(Tk.initProgress(this), Tk);
				}
				return base.readTicket(Tk);
			}
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null)
			{
				if (this.immediate_jump > 0)
				{
					this.immediate_jump -= 1;
				}
				this.Anm.showToFront(false, false);
			}
			this.EatBombMg = null;
			this.Anm.draw_margin = 0f;
			base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			this.ChkStasis.Clear();
			this.can_hold_tackle = false;
			this.Phy.quitSoftFall(0f);
			this.EadTongue.active = false;
			this.MgTackleTongue.destruct(this);
			base.quitTicket(Tk);
		}

		public bool runGaze(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("crouch", -1, null, false);
				this.walk_st = (int)X.NIXP(150f, 250f);
				this.immediate_jump = 2;
				base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			}
			if ((Tk.prog == PROG.ACTIVE || Tk.prog == PROG.PROG0) && this.considerGazing(Tk))
			{
				return true;
			}
			if (Tk.prog == PROG.ACTIVE && !Tk.Progress(ref this.t, this.walk_st, true))
			{
				base.hasFoot();
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, 0, true) && base.hasFoot())
			{
				this.SpSetPose("crouch2stand", -1, null, false);
				this.immediate_jump = 0;
			}
			return Tk.prog != PROG.PROG1 || this.t < 24f;
		}

		public bool getSafeDepx(float center_x, int pos_y, out float dep)
		{
			return this.getSafeDepx(center_x, pos_y, this.sizex * 0.78f, out dep, 2.1f, false);
		}

		public bool getSafeDepx(float center_x, int pos_y, float shift, out float dep, float target_deline_sxdif = 2.1f, bool alloc_near_jump = false)
		{
			dep = -1f;
			int num = 5;
			for (int i = 0; i < num; i++)
			{
				float num2 = center_x;
				if (i > 0)
				{
					num2 += shift * (float)X.IntC((float)i * 0.5f) * (float)X.MPF(i % 2 == 1) * base.mpf_is_right;
				}
				if (X.Abs(num2 - this.Nai.target_x) < this.sizex + target_deline_sxdif || (!alloc_near_jump && X.Abs(num2 - base.x) < 2.2f))
				{
					num = X.Mn(9, num + 2);
				}
				else
				{
					M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc((int)num2, pos_y, AIM.B);
					if (sideBcc != null && this.Nai.isAreaSafe(num2, sideBcc.slopeBottomY(num2), -0.2f, -0.2f, false, false, false))
					{
						dep = num2;
						return true;
					}
				}
			}
			return false;
		}

		public override void jumpInit(float xlen, float ypos, float high_y, bool release_x_velocity = false)
		{
			if (X.Abs(xlen) >= 6.5f)
			{
				high_y *= 0.5f;
			}
			base.jumpInit(xlen, ypos, high_y, release_x_velocity);
		}

		public float walk_mpf
		{
			get
			{
				if (X.Abs(this.Nai.target_x - base.x) > 2f)
				{
					return (float)X.MPF(this.Nai.target_x < base.x);
				}
				float num = base.mtop - 2f;
				int num2 = (int)base.x;
				int num3 = (int)num;
				M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc(num2, num3, AIM.R);
				M2BlockColliderContainer.BCCLine sideBcc2 = this.Mp.getSideBcc(num2, num3, AIM.L);
				float num4 = ((sideBcc == null) ? 6f : X.Mn(6f, sideBcc.slopeHitX(num) - base.x));
				float num5 = ((sideBcc2 == null) ? 6f : X.Mn(6f, base.x - sideBcc2.slopeHitX(num)));
				if (num4 == num5)
				{
					return (float)((X.xors(2) == 0) ? (-1) : 1);
				}
				return (float)X.MPF(num4 > num5);
			}
		}

		public bool runWalk(bool init_flag, NaTicket Tk, bool go_to_dep)
		{
			if (init_flag)
			{
				this.t = 0f;
				if (go_to_dep)
				{
					this.walk_time = Tk.depx;
				}
				else if (!this.getSafeDepx(this.Nai.target_x + this.walk_mpf * 7f * 0.8f, (int)((this.Nai.target_lastfoot_bottom + base.y) * 0.5f) - 3, out this.walk_time))
				{
					this.Nai.addTypeLock(NAI.TYPE.WALK, 120f);
					return false;
				}
				this.SpSetPose("crouch", -1, null, false);
				base.addF((NelEnemy.FLAG)192);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, (this.immediate_jump > 0) ? 0 : 13, true))
			{
				this.jumpInit(this.walk_time - base.x, 0f, go_to_dep ? X.Mx(4f, Tk.depy - base.mbottom + this.sizey * 2.6f) : 7f, false);
				this.SpSetPose("jump", -1, null, false);
				base.PtcVar("by", (double)(base.y + this.sizey * 0.6f)).PtcST("frog_jump_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.walk_st = 0;
				this.immediate_jump = 0;
				base.remF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT);
				return true;
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_st == 0)
				{
					if (base.vy >= 0f)
					{
						this.SpSetPose("fall", -1, null, false);
						this.walk_st = 1;
						this.t = 0f;
					}
				}
				else
				{
					if (!go_to_dep && !this.Nai.hasTypeLock(NAI.TYPE.PUNCH) && (this.ChkStasis.isStasis(false) ? this.isAttackableLengthTongue() : this.Nai.isAttackableLength(3.5f, -4f, 6f, false)) && X.BTW(10f, this.t, 15f) && this.TongueExecutable())
					{
						return this.RecreateTongue(Tk);
					}
					if (base.hasFoot())
					{
						this.SpSetPose("land", -1, null, false);
						Tk.AfterDelay(30f);
						return false;
					}
				}
			}
			return true;
		}

		public bool runBackStep(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.Nai.RemF(NAI.FLAG.BOTHERED);
				if (this.Nai.target_sxdif < 1.5f)
				{
					if (!this.getSafeDepx(base.x, (int)((this.Nai.target_lastfoot_bottom + base.y) * 0.5f) - 3, this.sizex * 3f, out this.walk_time, 6.3f, false))
					{
						this.Nai.addTypeLock(NAI.TYPE.WALK, 120f);
						return false;
					}
				}
				else
				{
					float num = X.Mx(7f * X.NIXP(0.5f, 0.85f), X.Abs(this.Nai.target_sxdif + 3f));
					float walk_mpf = this.walk_mpf;
					if (!this.getSafeDepx(this.Nai.target_x + this.walk_mpf * num, (int)((this.Nai.target_lastfoot_bottom + base.y) * 0.5f) - 3, this.sizex * 1.7f, out this.walk_time, 2.1f, false))
					{
						this.walk_time = this.Nai.target_x + this.walk_mpf * 9f;
					}
				}
				base.addF((NelEnemy.FLAG)192);
				base.AimToPlayer();
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, (this.immediate_jump > 0) ? 0 : 13, true))
			{
				this.jumpInit(this.walk_time - base.x, 0f, 5.5f, false);
				this.SpSetPose("backstep", -1, null, false);
				base.PtcVar("by", (double)(base.y + this.sizey * 0.6f)).PtcST("frog_jump_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.walk_st = 0;
				this.t = 80f;
				if (this.Nai.target_hasfoot == this.Nai.RANtk(3151) < 0.78f)
				{
					this.walk_st = 128;
					this.t = 80f;
				}
				this.immediate_jump = 0;
				return true;
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_st >= 0)
				{
					if (this.t >= 100f)
					{
						float num2 = -1000f;
						if (X.BTW(0f, (float)this.walk_st, 128f))
						{
							if (this.walk_st == 0)
							{
								this.walk_time = this.Nai.GARtoPr(0f, 0f);
							}
							this.t = 93f;
							if (this.walk_st < 6)
							{
								num2 = base.mpf_is_right * (float)this.walk_st * 6.2831855f / 6f;
							}
						}
						else
						{
							if (this.walk_st == 128)
							{
								this.walk_time = this.Nai.GARtoPr(0f, 0f) - base.mpf_is_right * 0.31415927f;
							}
							this.t = 96f;
							int num3 = this.walk_st % 128;
							if (num3 < 6)
							{
								num2 = base.mpf_is_right * (float)num3 * 3.1415927f * 0.23f / 6f;
							}
						}
						if (num2 != -1000f)
						{
							this.shotFrozenBullet(this.walk_time + num2);
							this.walk_st++;
						}
					}
					if (base.vy >= 0f)
					{
						this.walk_st = -1;
					}
				}
				else if (base.hasFoot())
				{
					this.SpSetPose("land", -1, null, false);
					Tk.AfterDelay(50f);
					return false;
				}
			}
			return true;
		}

		public void shotFrozenBullet(float agR)
		{
			MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.ICE_SHOT, base.mg_hit | MGHIT.IMMEDIATE);
			magicItem.Atk0 = this.AtkIce;
			magicItem.Atk1 = this.AtkIceBarn;
			float num = 1.5f * this.enlarge_level;
			magicItem.dx = X.Cos(agR);
			magicItem.dy = -X.Sin(agR);
			magicItem.sx = base.x + num * magicItem.dx;
			magicItem.sy = base.y + num * magicItem.dy;
			base.MpConsume(this.McsIceShot, magicItem, 1f, 1f);
		}

		public bool TongueExecutable()
		{
			return base.canGoToSide(AIM.B, 4f, -0.1f, false, false, false);
		}

		public bool runPunch_Tongue(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				if (!this.getSafeDepx(this.Nai.target_x + this.walk_mpf * 7f * 0.4f, (int)((this.Nai.target_lastfoot_bottom + base.y) * 0.5f) - 3, this.sizex * 1.4f, out this.walk_time, -2f, false))
				{
					Tk.Recreate(NAI.TYPE.GAZE, 1, true, null);
					return true;
				}
				this.SpSetPose("crouch", -1, null, false);
				this.setAim((this.walk_time < base.x) ? AIM.L : AIM.R, false);
				base.addF((NelEnemy.FLAG)192);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, (this.immediate_jump > 0) ? 0 : 13, true))
			{
				this.jumpInit(X.absMn(this.walk_time - base.x, 5f), 0f, 5.5f, false);
				this.SpSetPose("jump", -1, null, false);
				base.PtcVar("by", (double)(base.y + this.sizey * 0.6f)).PtcST("frog_jump_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.walk_st = 0;
				this.immediate_jump = 0;
				base.remF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT);
				return true;
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_st == 0)
				{
					if (base.vy >= 0f)
					{
						this.SpSetPose("fall", -1, null, false);
						this.walk_st = 1;
						this.t = 0f;
					}
				}
				else if (this.isAttackableLengthTongue() && X.BTW(0f, this.t, 6f) && this.TongueExecutable())
				{
					this.playSndPos("frog_tongue_voice", 1);
					this.walk_st = 0;
					Tk.prog = PROG.PROG1;
				}
				else if (base.hasFoot())
				{
					this.SpSetPose("land", -1, null, false);
					Tk.AfterDelay(30f);
					return false;
				}
			}
			return Tk.prog != PROG.PROG1 || this.runTongue(Tk, this.AtkGrab);
		}

		public bool isAttackableLengthTongue()
		{
			return this.Nai.isAttackableLength(10f, -5f, 11f, false);
		}

		private bool runTongue(NaTicket Tk, NelAttackInfo Atk)
		{
			float num;
			float num2;
			float num3;
			if (Atk == this.AtkGrab)
			{
				num = 40f;
				num2 = 60f;
				num3 = 38f;
			}
			else
			{
				num = 40f;
				num2 = -1f;
				num3 = this.Nai.NIRANtk(55f, 90f, 1314);
			}
			if (num2 < 0f)
			{
				num2 = num;
			}
			if (this.walk_st == 0)
			{
				this.EatBombMg = null;
				this.walk_st = 1;
				float num4 = this.Nai.target_x;
				float num5 = this.Nai.target_y + 0.2f;
				if (this.Nai.isPrAbsorbedCannotMove())
				{
					num5 += 0.55f;
				}
				else
				{
					num4 += (float)X.MPF(base.x < this.Nai.target_x) * X.Mn(X.Abs(this.Nai.target_x - base.x) * 0.4f, 2f);
					num5 += (this.Nai.target_hasfoot ? 0f : X.NIXP(1.6f, 2.5f));
					M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc((int)num4, (int)num5, AIM.B);
					if (sideBcc != null)
					{
						num5 = X.Mn(num5, sideBcc.slopeBottomY(num4) - 0.5f);
					}
				}
				this.DepTongue = new Vector2(num4, num5);
				Tk.Dep(this.DepTongue.x, this.DepTongue.y, null);
				this.setAim((base.x < this.DepTongue.x) ? AIM.R : AIM.L, false);
				if (!base.hasFoot())
				{
					this.Phy.initSoftFall(0.03f, 0f);
				}
				this.Phy.clampSpeed(FOCTYPE._, 0.03f, 0.002f, 1f);
				this.Anm.setPose(base.hasFoot() ? "attack_1" : "atk_air_1", -1);
				Vector2 tongueStartMapPos = this.getTongueStartMapPos();
				this.tng_agR = base.VALWALKANGLER((base.mpf_is_right > 0f) ? 0f : 3.1415927f, this.Mp.GAR(tongueStartMapPos.x, tongueStartMapPos.y, this.DepTongue.x, this.DepTongue.y), 1.9477875f);
				this.playSndPos("frog_tongue_prepare", 1);
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.t = 0f;
			}
			bool flag = false;
			if (this.walk_st == 1)
			{
				if (!base.hasFoot())
				{
					this.Anm.setPose("atk_air_1", 0);
				}
				else
				{
					this.Anm.setPose("attack_1", 0);
				}
				Vector2 tongueStartMapPos2 = this.getTongueStartMapPos();
				this.tng_agR = base.VALWALKANGLER(this.tng_agR, this.Mp.GAR(tongueStartMapPos2.x, tongueStartMapPos2.y, this.DepTongue.x, this.DepTongue.y), 0.07853982f * (1f - X.ZSINV(this.t, 35f) * 0.45f));
				if (this.t >= (base.hasFoot() ? num : num2))
				{
					this.t = 0f;
					this.walk_st = 2;
					this.Anm.setPose(base.hasFoot() ? "attack_2" : "atk_air_2", -1);
					base.PtcVar("agR", (double)this.tng_agR).PtcST("frog_tongue_roll_out", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.EadTongue.active = true;
					this.tongue_level = 0f;
					this.tongue_koro_lvl = (this.tongue_koro_spd = -1f);
					flag = true;
					this.Anm.showToFront(true, false);
					if (Atk == this.AtkTongueInGround)
					{
						this.MgTackleTongue = new MagicItemHandlerS(base.tackleInit(this.AtkTongueInGround, this.TkiTongueInGround));
						this.walk_time = -1f;
					}
					else
					{
						this.walk_time = 0f;
						this.MgTackleTongue = new MagicItemHandlerS(base.tackleInit(this.AtkGrab, this.TkiTongue));
					}
				}
			}
			if (this.walk_st == 2 || this.walk_st == 3)
			{
				if (!this.Nai.isPrAbsorbedCannotMove())
				{
					this.DepTongue.x = base.VALWALK(this.DepTongue.x, this.Nai.target_x, 0.05f);
					this.DepTongue.y = base.VALWALK(this.DepTongue.y, this.Nai.target_y, 0.012f);
				}
				if (!base.hasFoot())
				{
					this.Anm.setPose("atk_air_2", 0);
				}
				else
				{
					this.Anm.setPose("attack_2", 0);
				}
				Vector2 tongueStartMapPos3 = this.getTongueStartMapPos();
				this.tng_agR = base.VALWALKANGLER(this.tng_agR, this.Mp.GAR(tongueStartMapPos3.x, tongueStartMapPos3.y, this.DepTongue.x, this.DepTongue.y), 0.028274333f);
				if (this.walk_st == 2)
				{
					if (this.tongue_koro_lvl >= 0f)
					{
						uint ran = X.GETRAN2((int)this.Mp.floort, (int)this.Mp.floort % 30 + 1 + this.index % 10);
						this.tongue_koro_spd = X.absMn(this.tongue_koro_spd + (float)X.MPF(this.tongue_level < this.tongue_koro_lvl) * X.NI(0.0003f, 0.013f, X.ZPOW(X.RAN(ran, 705))), 0.08f);
						this.tongue_level = X.saturate(this.tongue_level + this.tongue_koro_spd);
						flag = true;
					}
					else if (this.tongue_level < 1f)
					{
						this.tongue_level = X.ZLINE(this.t, 20f);
						flag = true;
					}
					if (this.t >= num3 || this.walk_time == -2f)
					{
						this.t = 0f;
						this.walk_st = 3;
						this.tongue_koro_spd = -2f;
						this.tongue_koro_lvl = this.tongue_level;
						this.playSndPos("frog_tongue_roll_in", 1);
					}
				}
				if (this.walk_st == 3)
				{
					if (this.tongue_level > 0f)
					{
						this.tongue_level = this.tongue_koro_lvl * (1f - X.ZLINE(this.t, 13f));
						flag = true;
					}
					if (this.t >= 15f)
					{
						this.t = 0f;
						this.walk_st = 10;
						this.can_hold_tackle = false;
						this.EadTongue.active = false;
						this.Anm.setPose(base.hasFoot() ? "attack_3" : "atk_air_3", -1);
						this.Phy.quitSoftFall(0f);
						base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					}
				}
				if (this.EadTongue.active)
				{
					if (this.walk_time >= 0f)
					{
						this.walk_time += this.TS;
						if (this.walk_time >= 3f)
						{
							this.walk_time = 0f;
							this.EatBombMg = base.nM2D.MGC.FindMg(tongueStartMapPos3.x, tongueStartMapPos3.y, 9.6f * base.scaleX * this.tongue_level, this.tng_agR, this.TkiTongue.radius, this.TkiTongue.radius + 1.02f, (MagicItem Mg, M2MagicCaster Caster) => Mg.Other is IMgBombListener && (Mg.Other as IMgBombListener).isEatableBomb(Mg, Caster, true), this);
							if (this.EatBombMg != null)
							{
								Vector2 mapPos = this.EatBombMg.Ray.getMapPos(0f);
								this.Mp.PtcSTsetVar("cx", (double)mapPos.x).PtcSTsetVar("cy", (double)mapPos.y).PtcSTsetVar("agR", (double)this.tng_agR_)
									.PtcST("hitabsorb_R", null, PTCThread.StFollow.NO_FOLLOW);
								this.walk_time = -2f;
								this.can_hold_tackle = false;
							}
						}
					}
					if (flag)
					{
						this.Anm.need_fine_mesh = true;
						if (this.MgTackleTongue.isActive(this))
						{
							float num6 = 9.6f * base.scaleX * this.tongue_level;
							this.MgTackleTongue.Mg.sx = X.Cos(this.tng_agR) * num6;
							this.MgTackleTongue.Mg.sy = -X.Sin(this.tng_agR) * num6;
						}
					}
				}
			}
			if (this.walk_st == 10)
			{
				this.tongueAngleWalkZero(0.02199115f);
				if (base.hasFoot())
				{
					if (!this.Anm.poseIs("land", false))
					{
						this.Anm.setPose("stand", -1);
					}
					this.Anm.rotationR = 0f;
					this.Anm.draw_margin = 0f;
					if (this.t >= 40f)
					{
						return false;
					}
				}
				else
				{
					this.t = X.Mn(this.t, 13f);
				}
			}
			return true;
		}

		public float tongueAngleWalkZero(float spd)
		{
			this.tng_agR = base.VALWALKANGLER(this.tng_agR, (base.mpf_is_right > 0f) ? 0f : 3.1415927f, spd);
			return this.tng_agR;
		}

		private float tng_agR
		{
			get
			{
				return this.tng_agR_;
			}
			set
			{
				this.tng_agR_ = value;
				this.Anm.draw_margin = 9.6f;
				if (base.hasFoot())
				{
					this.tng_agR_ = X.correctangleR(this.tng_agR_);
					if (base.mpf_is_right > 0f)
					{
						this.tng_agR_ = X.absMn(this.tng_agR_, 0.6597344f);
					}
					else if (this.tng_agR_ > 0f)
					{
						this.tng_agR_ = X.MMX(2.4818583f, this.tng_agR_, 3.1415927f);
					}
					else
					{
						this.tng_agR_ = X.MMX(-3.1415927f, this.tng_agR_, -2.4818583f);
					}
					this.Anm.rotationR = 0f;
					return;
				}
				this.Anm.rotationR = ((base.mpf_is_right > 0f) ? this.tng_agR_ : (3.1415927f + this.tng_agR_));
			}
		}

		private bool RecreateTongue(NaTicket Tk)
		{
			Tk.Recreate(NAI.TYPE.PUNCH, -1, true, null);
			Tk.prog = PROG.PROG1;
			this.t = 0f;
			this.walk_st = 0;
			return true;
		}

		public bool runPunch0_TongueInGround(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
			}
			if (!this.runTongue(Tk, this.AtkTongueInGround))
			{
				Tk.AfterDelay(this.Nai.NIRANtk(5f, 30f, 2155) * ((this.Nai.RANtk(4353) < 0.2f) ? 2.5f : 1f));
				return false;
			}
			return true;
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			this.absorb_weight = 4;
			if (this.state != NelEnemy.STATE.STAND || this.Absorb != null || Abm.Con.current_pose_priority >= 45)
			{
				return false;
			}
			PR pr = MvTarget as PR;
			if (pr == null || !base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			NelNFrog.BuryEvent.releaseFromTarget(MvTarget);
			this.Anm.showToFront(true, false);
			base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			this.walk_time = 5f;
			this.tongue_level0 = X.Mx(this.tongue_level, 0.12f);
			this.t = 0f;
			if (pr.hp_ratio < 0.3f == X.XORSP() < 0.75f)
			{
				this.walk_st = 100;
				Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 8, 15U);
			}
			else
			{
				this.walk_st = 0;
				Abm.get_Gacha().activate(PrGachaItem.TYPE.SEQUENCE, 5, KEY.getRandomKeyBitsLRTB(2));
			}
			if (!base.hasFoot())
			{
				this.Phy.initSoftFall(0.24f, 0f);
			}
			Abm.kirimomi_release = true;
			Abm.pose_priority = 45;
			Abm.release_from_publish_count = true;
			Abm.no_clamp_speed = true;
			this.Absorb.target_pose = "dmg_tongue_in";
			this.EadTongue.active = true;
			Abm.get_Gacha().SoloPositionPixel = new Vector3(0f, -200f, 0f);
			this.playSndPos("frog_tongue_roll_in", 1);
			base.carryable_other_object = true;
			pr.getFootManager().rideInitTo(this, false);
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.isNotTortureEnable() || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.walk_st == 100 || this.walk_st == 0)
			{
				this.tongue_level = X.NI(this.tongue_level0, 0f, X.ZPOW(this.t, 35f));
				if (this.t >= this.walk_time)
				{
					this.walk_time += X.NIXP(13f, 22f);
					base.applyAbsorbDamageTo(pr, this.AtkAbsorbInit, true, false, false, 0f, false, null, false);
				}
				this.tongueAngleWalkZero(((this.t >= 10f) ? 0.014f : 0.002f) * 3.1415927f);
				if (this.t >= 60f)
				{
					this.playSndPos("frog_tongue_catch", 1);
					this.t = 1f;
					this.EadTongue.active = false;
					base.carryable_other_object = false;
					this.walk_st++;
					this.Phy.quitSoftFall(0f);
					this.Anm.showToFront(false, false);
				}
			}
			if (this.walk_st == 1)
			{
				this.walk_st = 2;
				base.PtcST("frog_bury_prepare", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Absorb.changeTorturePose("torture_burying", false, false, -1, -1);
				this.Absorb.setKirimomiReleaseDir((int)this.aim);
			}
			if (this.walk_st == 2)
			{
				pr.fineFrozenAF(4f);
				this.tongueAngleWalkZero(0.18849556f);
				if (this.Anm.isAnimEnd())
				{
					float t = this.t;
					float num = pr.drawx * this.Mp.rCLEN + base.mpf_is_right * 1.6f * base.scaleX;
					M2BlockColliderContainer.BCCLine bccline;
					float num2;
					if (!base.canGoToSideLB(out bccline, out num2, this.aim, 4f, -0.2f, false, false, false))
					{
						if (this.aim == AIM.R)
						{
							num = X.Mn(num, base.x + 4f - X.Abs(num2));
						}
						else
						{
							num = X.Mx(num, base.x - 4f + X.Abs(num2));
						}
					}
					this.Mp.BCC.isFallable(num, pr.y, pr.sizex + 0.5f, 3.7f, out bccline, true, false, base.mbottom);
					if (bccline != null && pr.isAbsorbState())
					{
						AbsorbManagerContainer absorbContainer = pr.getAbsorbContainer();
						this.absorb_weight = 1;
						AbsorbManager absorbManager = absorbContainer.initSpecialGacha(pr, NelNFrog.BuryEvent, PrGachaItem.TYPE.SEQUENCE, 5, KEY.SIMKEY.L | KEY.SIMKEY.R | KEY.SIMKEY.T | KEY.SIMKEY.B, true);
						if (absorbManager != null)
						{
							absorbContainer.uipicture_fade_key = "";
							NelNFrog.BuryEvent.Abm = absorbManager;
							absorbManager.cannot_move = true;
							absorbManager.mouth_is_covered = true;
							absorbManager.use_cam_zoom2 = true;
							absorbManager.normal_UP_fade_injectable = 0.03f;
							absorbManager.uipicture_fade_key = (this.Absorb.uipicture_fade_key = "torture_groundbury");
							absorbContainer.timeout = 0;
							absorbManager.release_from_publish_count = true;
							absorbManager.kirimomi_release = false;
							absorbManager.target_pose = (pr.is_alive ? "buried" : "buried_death");
							absorbManager.pose_priority = 44;
							float num3 = bccline.slopeBottomY(num) - pr.sizey;
							pr.fineDrawPosition();
							pr.moveBy(num - pr.x, num3 - pr.y, true);
							pr.getPhysic().killSpeedForce(true, true, true, false, false);
							pr.getFootManager().initJump(true, true, false);
							pr.PtcVar("by", (double)pr.mbottom).PtcST("frog_bury_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							base.applyAbsorbDamageTo(pr, this.AtkAbsorbBuryHit, true, true, true, 1f, false, null, false);
							pr.UP.setFade(this.Absorb.uipicture_fade_key, UIPictureBase.EMSTATE.NORMAL, false, false, false);
							this.Nai.AddF(NAI.FLAG.POWERED, 50f);
							absorbContainer.use_torture = false;
						}
						else
						{
							this.SpSetPose(base.hasFoot() ? "attack_3" : "atk_air_3", -1, null, false);
						}
					}
					else
					{
						this.SpSetPose(base.hasFoot() ? "attack_3" : "atk_air_3", -1, null, false);
					}
					return false;
				}
				this.t = 1f;
			}
			if (this.walk_st == 101)
			{
				this.t = 0f;
				this.walk_st = 102;
				this.walk_time = 40f;
				base.PtcST("frog_vore_prepare", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Absorb.changeTorturePose("torture_frog_vore", false, false, -1, -1);
				pr.PadVib("dmg_absorb_l", 1f);
				this.Anm.rotationR = 0f;
			}
			if (this.walk_st == 102 || this.walk_st == 103)
			{
				if (this.t >= this.walk_time)
				{
					if (this.walk_st == 102)
					{
						this.walk_st++;
						pr.playVo("awkx", false, false);
					}
					this.walk_time += X.NIXP(30f, 40f);
					base.applyAbsorbDamageTo(pr, this.AtkAbsorbInit, true, false, false, 0f, false, null, false);
					this.Absorb.get_Gacha().SoloPositionPixel = new Vector3(0f, -80f, 0f);
					this.Absorb.Con.need_fine_gacha_effect = true;
				}
				if (this.Anm.poseIs("torture_frog_vore2", true))
				{
					base.PtcST("frog_vore_in_stomach", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					pr.fineFrozenAF(5f);
					this.t = 0f;
					this.walk_time = 60f;
					this.walk_st = 110;
					this.Absorb.normal_UP_fade_injectable = 0.12f;
					this.Absorb.uipicture_fade_key = "torture_swallowed";
					if (this.SndIntvStomach != null)
					{
						this.SndIntvStomach.Stop();
					}
					this.SndIntvStomach = base.M2D.Snd.createInterval(this.snd_key, "insect_rape", 120f, pr, 0f, -1);
					pr.addD(M2MoverPr.DECL.THROW_RAY);
					this.Absorb.cannot_apply_mist_damage = true;
					this.HandlerPE.Set(PostEffect.IT.setPE(POSTM.BGM_WATER, 40f, 0.6f, 0));
					this.HandlerPE.Set(PostEffect.IT.setPE(POSTM.ZOOM2_EATEN, 40f, 1f, 0));
					this.HandlerPE.Set(PostEffect.IT.setPE(POSTM.GAS_APPLIED, 90f, 0.6f, 0));
				}
			}
			if (this.walk_st == 110 && this.t >= this.walk_time)
			{
				this.t = 0f;
				this.walk_st = 111;
				this.walk_time = X.NIXP(70f, 90f);
				base.runAbsorb();
				this.AtkAbsorbInStomach.Beto = ((X.XORSP() < 0.8f) ? this.BetoStomachA : this.BetoStomachB);
				base.applyAbsorbDamageTo(pr, this.AtkAbsorbInStomach, true, X.XORSP() < 0.4f, false, 1f, false, null, false);
				pr.PadVib("dmg_absorb_l", 1f);
				pr.PtcST("frog_vore_fatal_dmg", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				pr.PtcST("absorb_atk_in_stomack", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Mp.DropCon.setBlood(pr, 22, (pr.get_mp() > 0f) ? MTR.col_pr_mana : MTR.col_pr_no_mana, 0f, false);
				pr.UP.applyVoreBurned();
				this.Anm.randomizeFrame();
			}
			if (this.walk_st == 111 && this.t >= 40f)
			{
				pr.playVo(((float)pr.ep < 500f) ? "breath_down" : (((float)pr.ep >= 800f && X.XORSP() < 0.55f) ? "mustl" : "must"), false, false);
				this.walk_st = 110;
			}
			return true;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			Aattr.Add(MGATTR.ICE);
			Aattr.Add(MGATTR.ABSORB);
			Aattr.Add(MGATTR.EATEN);
			A.Add("torture_swallowed");
			A.Add("torture_swallowed");
		}

		public override IFootable isCarryable(M2FootManager FootD)
		{
			PR pr = FootD.Mv as PR;
			if (!(pr != null) || !base.carryable_other_object || this.state != NelEnemy.STATE.ABSORB)
			{
				return null;
			}
			if (this.Absorb == null || !(this.Absorb.getTargetMover() == pr))
			{
				return null;
			}
			return this;
		}

		public override bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			if (HitMv == null)
			{
				return true;
			}
			if (Atk == this.AtkTongueInGround && HitMv.Mv is PR)
			{
				if (!this.MgTackleTongue.isActive(this))
				{
					return false;
				}
				this.MgTackleTongue.Mg.Ray.HitLock((float)((int)X.NIXP(8f, 35f)), null);
				if (this.tongue_koro_spd == -1f)
				{
					this.tongue_koro_lvl = X.Scr(this.tongue_level, 0.2f);
					this.tongue_koro_spd = 0.01f;
				}
			}
			return true;
		}

		public override float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy)
		{
			if (FootD.Mv as PR != null && base.carryable_other_object && this.state == NelEnemy.STATE.ABSORB)
			{
				float num = base.x;
				float num2 = base.y;
				if (this.walk_st == 0 || this.walk_st == 100)
				{
					float num3 = base.scaleX * X.NI(1.1f, 8.1f, this.tongue_level);
					num = base.x + num3 * X.Cos(this.tng_agR_);
					num2 = base.y - num3 * X.Sin(this.tng_agR_);
				}
				dx = num - x;
				dy = num2 - y;
				return 0.5f;
			}
			return base.fixToFootPos(FootD, x, y, out dx, out dy);
		}

		private Vector2 getTongueStartMapPos()
		{
			float num = base.scaleX * 34f * base.mpf_is_right * this.Mp.rCLENB;
			float num2 = -base.scaleY * -29f * this.Mp.rCLENB * 0.5f;
			Vector2 vector = X.ROTV2e(new Vector2(num, num2), this.Anm.rotationR);
			vector.x += base.x;
			vector.y += base.y;
			return vector;
		}

		private bool fnDrawTongue(MeshDrawer Md, EnemyAnimator.EnemyAdditionalDrawFrame Ead)
		{
			if (this.Mp == null)
			{
				return false;
			}
			Md.initForImg(this.PxlWhite.Img, 0);
			float mpf_is_right = base.mpf_is_right;
			if (mpf_is_right > 0f)
			{
				Md.Scale(-1f, 1f, true);
			}
			Md.TranslateP(-34f, -29f, true);
			if (base.hasFoot())
			{
				if (mpf_is_right > 0f)
				{
					Md.Rotate(-this.tng_agR, true);
				}
				else
				{
					Md.Rotate(this.tng_agR - 3.1415927f, true);
				}
			}
			float num = this.tongue_level;
			if (base.isAbsorbState())
			{
				num = X.NI(0.08f, 1f, num);
			}
			this.SheetTongue.drawLine(Md, 0f, 0f, this.Mp.CLENB * 0.6f, 25f, 6f, num, 0f, 1f);
			return true;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			if (this.EatBombMg != null && Atk == this.EatBombMg.Atk0)
			{
				base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
				this.Phy.clearLockGravity();
				this.Phy.killSpeedForce(true, true, true, false, false);
				this.Anm.timescale = 1f;
				base.PtcST("bomb_hit_critical", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			int num = base.applyDamage(Atk, force);
			if (num > 0 || force)
			{
				this.playSndPos("frog_dmg", 1);
			}
			return num;
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			if (Atk != null && Atk.attr == MGATTR.ICE)
			{
				return 0f;
			}
			return base.applyHpDamageRatio(Atk) * (float)((this.EatBombMg != null && Atk == this.EatBombMg.Atk0) ? 3 : 1);
		}

		protected override void autoTargetRayHitted(M2Ray Ray)
		{
			this.Nai.autotargetted_me = true;
			if ((Ray.hittype & HITTYPE.AUTO_TARGET) != HITTYPE.NONE)
			{
				this.Nai.AddF(NAI.FLAG.BOTHERED, 100f);
			}
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle;
		}

		private const NAI.TYPE NT_TONGUE_GRAB = NAI.TYPE.PUNCH;

		private const NAI.TYPE NT_TONGUE_IN_GROUND = NAI.TYPE.PUNCH_0;

		private const float backshot_first_delay = 20f;

		private const float backshot_delay = 7f;

		private const float backshot_underslide_delay = 4f;

		private const float walk_sxdif = 7f;

		private const int jump_delay = 13;

		private const int backshot_count = 6;

		private const float tongue_len = 9.6f;

		private byte immediate_jump;

		private NASStasisChecker ChkStasis;

		private float tng_agR_;

		private Vector2 DepTongue;

		private MagicItemHandlerS MgTackleTongue;

		private float tongue_level;

		private float tongue_level0;

		private float tongue_koro_lvl = -1f;

		private float tongue_koro_spd = -1f;

		private MagicItem EatBombMg;

		private EffectHandlerPE HandlerPE;

		protected NelAttackInfo AtkGrab = new NelAttackInfo
		{
			hpdmg0 = 9,
			attr = MGATTR.GRAB,
			is_grab_attack = true,
			knockback_len = 0.2f,
			parryable = true,
			Beto = BetoInfo.Normal
		};

		protected NelAttackInfo AtkIce = new NelAttackInfo
		{
			hpdmg0 = 15,
			split_mpdmg = 1,
			burst_vx = 0.024f,
			knockback_len = 0.6f,
			huttobi_ratio = -1000f,
			attr = MGATTR.ICE,
			shield_break_ratio = -4f,
			parryable = true,
			nodamage_time = 0,
			SerDmg = new FlagCounter<SER>(1).Add(SER.FROZEN, 35f)
		}.Torn(0.002f, 0.003f);

		protected NelAttackInfo AtkIceBarn;

		protected NelAttackInfo AtkAbsorbInit = new NelAttackInfo
		{
			hpdmg0 = 1,
			split_mpdmg = 4,
			huttobi_ratio = -1000f,
			attr = MGATTR.GRAB,
			Beto = BetoInfo.DarkTornado,
			pee_apply100 = 4f,
			SerDmg = new FlagCounter<SER>(1).Add(SER.SEXERCISE, 10f)
		};

		protected NelAttackInfo AtkAbsorbBuryHit = new NelAttackInfo
		{
			hpdmg0 = 30,
			split_mpdmg = 4,
			huttobi_ratio = 1000f,
			attr = MGATTR.STAB,
			Beto = BetoInfo.GroundHard,
			SerDmg = new FlagCounter<SER>(1).Add(SER.CONFUSE, 60f)
		};

		private const float tongue_ground_nodam_min = 8f;

		private const float tongue_ground_nodam_max = 35f;

		protected NelAttackInfo AtkTongueInGround = new NelAttackInfo
		{
			hpdmg0 = 1,
			split_mpdmg = 6,
			huttobi_ratio = -1000f,
			nodamage_time = 8,
			parryable = true,
			attr = MGATTR.ABSORB_V,
			Beto = BetoInfo.Absorbed,
			SerDmg = new FlagCounter<SER>(1).Add(SER.SEXERCISE, 4f),
			EpDmg = new EpAtk(7, "frog")
			{
				vagina = 8,
				cli = 1
			}
		};

		protected NelAttackInfo AtkAbsorbInStomach = new NelAttackInfo
		{
			hpdmg0 = 10,
			mpdmg0 = 22,
			split_mpdmg = 4,
			attr = MGATTR.EATEN,
			hit_ptcst_name = "",
			EpDmg = new EpAtk(40, "frog_vore")
			{
				other = 5
			},
			SerDmg = new FlagCounter<SER>(1).Add(SER.SEXERCISE, 20f)
		}.Torn(0.04f, 0.18f);

		private BetoInfo BetoStomachA = BetoInfo.Vore.Thread(0, true);

		private BetoInfo BetoStomachB = BetoInfo.Lava.Pow(20, false);

		private NOD.TackleInfo TkiTongue = NOD.getTackle("frog_tongue");

		private NOD.TackleInfo TkiTongueInGround = NOD.getTackle("frog_tongue_in_ground");

		private NOD.MpConsume McsIceShot = NOD.getMpConsume("frog_iceshot");

		private RollSeetDrawer SheetTongue;

		private PxlLayer PxlWhite;

		private EnemyAnimator.EnemyAdditionalDrawFrame EadTongue;

		private static NelNFrog.GachaGroundBury BuryEvent;

		private M2SndInterval SndIntvStomach;

		private const int PRI_WAIT = 1;

		private const int PRI_MOVE = 2;

		private const int PRI_ATK = 100;

		private const int ABSORB_PRI = 45;

		private const float tongue_shift_x_pixel = 34f;

		private const float tongue_shift_y_pixel = -29f;

		public class GachaGroundBury : IGachaListener
		{
			public AbsorbManager Abm
			{
				set
				{
					this.up_cnt = 0;
					this.Abm_ = value;
					this.id = ((this.Abm_ != null) ? this.Abm_.id : 0);
				}
			}

			public bool canAbsorbContinue()
			{
				if (this.Abm_ != null && this.Abm_.id == this.id && this.Abm_.Listener == this && this.Abm_.isActive())
				{
					PR pr = this.Abm_.getTargetMover() as PR;
					if (pr == null)
					{
						return false;
					}
					if (pr.getFootManager().get_FootBCC() != null)
					{
						if (pr.UP.getCurrentFadeKey(true) != "torture_groundbury")
						{
							int num = this.up_cnt + 1;
							this.up_cnt = num;
							if (num >= 50)
							{
								pr.UP.setFade("torture_groundbury", UIPictureBase.EMSTATE.NORMAL, false, false, false);
							}
						}
						else
						{
							this.up_cnt = 0;
						}
						return true;
					}
				}
				return false;
			}

			public void releaseFromTarget(NelM2Attacker Atk)
			{
				if (this.Abm_ != null && this.Abm_.checkTarget(Atk))
				{
					this.Abm_ = null;
				}
			}

			public void absorbFinished(bool abort)
			{
			}

			public bool individual
			{
				get
				{
					return false;
				}
			}

			private AbsorbManager Abm_;

			private int id;

			public int up_cnt;
		}
	}
}
