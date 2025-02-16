using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNGolemToyMkb : NelNGolemToy
	{
		public override int create_count_normal
		{
			get
			{
				return 6;
			}
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.Above = new NASAboveChecker(this, 240f, 5f);
			this.AnmT.allow_check_main = false;
			this.AtkMkb.Prepare(this, true);
			this.AtkMkbAb.Prepare(this, true);
			this.Nai.can_progress_delay_if_ticket_exists = true;
		}

		protected override void initBorn()
		{
			base.initBorn();
			this.Nai.AddF(NAI.FLAG.GAZE, 180f);
			this.cannot_move = false;
			this.SqMokuba = this.Anm.getCurrentCharacter().getPoseByName("mokuba").getSequence(0);
			this.makeMkb(out this.Mkb);
			this.makeMkbA(out this.MkbA);
		}

		private void makeMkb(out MokubaDrawer Mkb)
		{
			Mkb = new MokubaDrawer();
			Mkb.setImage(this.SqMokuba.getImage(0, 0), this.SqMokuba.getImage(4 + EnemyAttr.mattrIndex(this.nattr), 0), this.SqMokuba.getImage(1, 0), this.SqMokuba.getImage(2, 0));
			Mkb.draw_w_scale = 0.7878788f;
		}

		private void makeMkbA(out MokubaDrawer Mkb)
		{
			Mkb = null;
			PxlImage image = this.SqMokuba.getImage(4 + EnemyAttr.mattrIndex(this.nattr), 1);
			if (image == null)
			{
				return;
			}
			Mkb = new MokubaDrawer();
			Mkb.setImage(null, image, null, null);
			Mkb.draw_w_scale = 0.7878788f;
		}

		public override void setDeathDro()
		{
			if (this.SqMokuba != null)
			{
				this.Anm.setBreakerDropObject("golemtoy_break", 0f, 0f, 0f, this.SqMokuba);
			}
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.TkRide != null)
			{
				this.TkRide = this.Mp.MovRenderer.deassignDrawable(this.TkRide, -1);
				this.TkRide = null;
			}
			base.destruct();
		}

		protected override bool considerNormal(NAI Nai)
		{
			if (!base.create_finished)
			{
				return true;
			}
			if (!Nai.hasPT(128, false))
			{
				if (Nai.cant_access_to_pr())
				{
					return Nai.AddTicketB(NAI.TYPE.WAIT, 128, true);
				}
				if (this.Above.isAboveActive())
				{
					return Nai.AddTicketB(NAI.TYPE.PUNCH_0, 128, true);
				}
				if (Nai.HasF(NAI.FLAG.GAZE, true))
				{
					return Nai.AddTicketB(NAI.TYPE.GAZE, 128, true);
				}
				if (!Nai.HasF(NAI.FLAG.ABSORB_FINISHED, false) && Nai.isTargetAbove(0.1f, 1.2f, 5f))
				{
					return Nai.AddTicketB(NAI.TYPE.PUNCH, 128, true);
				}
			}
			if (!Nai.hasPT(5, false))
			{
				Nai.AddTicket(NAI.TYPE.WALK, 5, true).Dep(Nai.target_x, Nai.target_y, Nai.TargetLastBcc);
			}
			return true;
		}

		public override NelEnemy changeState(NelEnemy.STATE state)
		{
			int state2 = (int)this.state;
			base.changeState(state);
			if (state2 == 1000)
			{
				this.rotXR = 0f;
				this.Anm.showToFront(false, false);
				this.makeRideRenderTicket(false);
				this.Nai.AddF(NAI.FLAG.ABSORB_FINISHED, 160f);
				base.carryable_other_object = false;
			}
			return this;
		}

		public override void runPre()
		{
			base.runPre();
			if (this.Nai == null)
			{
				return;
			}
			if (base.create_finished && !this.Nai.isFrontType(NAI.TYPE.PUNCH_0, PROG.ACTIVE))
			{
				if (!this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE) && this.state != NelEnemy.STATE.ABSORB && this.state != NelEnemy.STATE.STUN)
				{
					float num = X.Mx(0f, X.COSI(this.t, 38f));
					this.AnmT.vib_y = num * num * 6f;
				}
				this.Above.run(this.TS);
			}
		}

		public override bool readTicket(NaTicket Tk)
		{
			if (!base.create_finished)
			{
				return false;
			}
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
				return this.runWalk(Tk.initProgress(this), Tk);
			case NAI.TYPE.WALK_TO_WEED:
				break;
			case NAI.TYPE.PUNCH:
				return this.runPunch_Digging(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_0:
				return this.runPunch0_Jump(Tk.initProgress(this), Tk);
			default:
				if (type - NAI.TYPE.GAZE <= 1)
				{
					return this.runGaze(Tk.initProgress(this), Tk, Tk.type == NAI.TYPE.GAZE);
				}
				break;
			}
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.PUNCH_0)
				{
					base.killPtc("golemtoy_mkb_jump", false);
				}
				if (Tk.type == NAI.TYPE.GAZE || Tk.type == NAI.TYPE.WAIT || Tk.type == NAI.TYPE.PUNCH_0)
				{
					this.Above.Clear();
				}
			}
			this.Above.maxt = X.NIXP(140f, 340f);
			this.rotXR = (this.rotZR = 0f);
			base.throw_ray = !base.create_finished;
			this.stopSndWalk();
			this.can_hold_tackle = false;
			if (Tk != null && Tk.type == NAI.TYPE.PUNCH)
			{
				this.Anm.setBorderMaskEnable(false);
			}
			base.quitTicket(Tk);
		}

		private void createSndWalk()
		{
			if (this.SndWalk == null)
			{
				this.SndWalk = this.playSndPos("golemtoy_mkb_move", 2);
			}
		}

		private void stopSndWalk()
		{
			if (this.SndWalk != null && this.SndWalk.key == this.snd_key)
			{
				this.SndWalk.Stop();
				this.SndWalk = null;
			}
		}

		public bool runGaze(bool init_flag, NaTicket Tk, bool show_front)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_time = this.Nai.NIRANtk(130f, 190f, 4413);
			}
			if (show_front)
			{
				this.rotYR = base.VALWALK(this.rotYR, 0f, 0.025132744f);
			}
			float num = this.t % 16f / 16f;
			float num2 = (0.5f - num) * 2f * 0.06f * 3.1415927f;
			this.rotXR = base.VALWALK(this.rotXR, num2, 0.07225663f);
			float num3 = (float)X.MPF((int)this.t / 16 % 2 == 0) * 0.04f * 3.1415927f;
			this.rotZR = base.VALWALK(this.rotZR, num3, 0.13508849f);
			return this.t < this.walk_time;
		}

		public bool runWalk(bool init_flag, NaTicket Tk)
		{
			float mpf_is_right = base.mpf_is_right;
			float num = mpf_is_right * 1.5707964f;
			if (Tk.prog <= PROG.ACTIVE)
			{
				if (init_flag)
				{
					this.t = 0f;
					this.walk_time = -1f;
					this.walk_st = 0;
				}
				if (Tk.prog == PROG.ACTIVE)
				{
					if (this.walk_time != 0f)
					{
						this.walk_time = 0f;
						this.t = 0f;
						if (X.Abs(this.rotYR_ - num) < 0.008f)
						{
							Tk.prog = PROG.PROG0;
						}
						else
						{
							this.playSndPos("golemtoy_mkb_start", 1);
						}
					}
					this.setWalkXSpeed(base.VALWALK(this.Phy.walk_xspeed, 0f, 0.008f), true, false);
					if (Tk.prog == PROG.ACTIVE)
					{
						this.rotYR = base.VALWALK(this.rotYR, num, 0.11623893f);
						if (Tk.Progress(ref this.t, 0, X.Abs(this.rotYR_ - num) < 0.008f) && this.walk_st == 1 && X.XORSP() < 0.2f && !this.Nai.HasF(NAI.FLAG.ABSORB_FINISHED, false))
						{
							this.walk_st = 22 + (int)(X.XORSP() * 40f);
						}
					}
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_time <= 0f)
				{
					this.walk_time = 30f;
					this.t = 0f;
					this.createSndWalk();
				}
				this.setWalkXSpeed(mpf_is_right * X.NI(0.084f, 0.03f, this.enlarge_level - 1f), true, false);
				num += X.SINI(this.t, 40f) * 0.1f * 3.1415927f * mpf_is_right;
				this.rotYR = base.VALWALK(this.rotYR, num, 0.07853982f);
				AIM aim = this.aim;
				if (base.wallHittedA())
				{
					this.walk_time -= this.TS * 4f;
					aim = ((mpf_is_right > 0f) ? AIM.L : AIM.R);
					if (this.walk_time <= 0f)
					{
						this.walk_st = 1;
					}
				}
				else if (base.x < this.Nai.target_x != mpf_is_right > 0f && this.walk_st == 0)
				{
					aim = ((base.x < this.Nai.target_x) ? AIM.R : AIM.L);
					this.walk_time -= this.TS;
				}
				else
				{
					this.walk_time = base.VALWALK(this.walk_time, 30f, 0.33f);
				}
				this.rotZR = this.AnmT.vib_y / 6f * mpf_is_right * 0.07f * 3.1415927f;
				if (this.walk_time <= 0f)
				{
					this.rotZR = 0f;
					this.walk_time = -1f;
					Tk.prog = PROG.ACTIVE;
					this.setAim(aim, false);
					this.stopSndWalk();
					return true;
				}
				if (this.walk_st == 1)
				{
					M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
					if (footBCC != null && this.Nai.TargetLastBcc == footBCC)
					{
						this.walk_st = 0;
					}
				}
				if (this.walk_st >= 2 && this.t >= (float)this.walk_st)
				{
					this.rotZR = 0f;
					Tk.Recreate(NAI.TYPE.PUNCH, 128, true, null);
					this.stopSndWalk();
					return true;
				}
			}
			return true;
		}

		public bool runPunch_Digging(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.AnmT.vib_y = 0f;
				this.t = 0f;
				this.walk_st = 0;
				base.PtcVar("by", (double)base.mbottom).PtcVar("mpf", (double)(1.5707964f + 1.5707964f * base.mpf_is_right)).PtcST("golemtoy_mkb_bump_dive", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Anm.setBorderMaskEnable(true);
				this.setWalkXSpeed(0f, false, true);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				this.yshift = base.VALWALK(this.yshift, -50f, 1f);
				this.rotYR += base.mpf_is_right * 0.077f * 3.1415927f;
				if (this.t >= 60f)
				{
					base.throw_ray = true;
				}
				if (Tk.Progress(ref this.t, 140, true))
				{
					this.walk_time = X.NIXP(160f, 380f);
					this.rotYR = X.correctangleR(this.rotYR);
					this.Above.ensureDelay(120f);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				bool flag = this.Nai.isTargetAbove(0.5f, -0.7f, 1.4f) || !base.hasFoot();
				this.rotYR = base.VALWALK(this.rotYR, 0f, 0.12566371f);
				if (!flag && this.t >= this.walk_time)
				{
					this.t = 0f;
					M2BlockColliderContainer.BCCLine targetLastBcc = this.Nai.TargetLastBcc;
					M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
					if (this.Above.isAboveActive())
					{
						this.walk_st = 1;
						flag = true;
					}
					else if (targetLastBcc == null || footBCC == null)
					{
						flag = X.XORSP() < 0.2f;
					}
					else
					{
						flag = targetLastBcc.isLinearWalkableTo(footBCC, false) != 0 && X.XORSP() < 0.5f;
					}
				}
				if (Tk.Progress(ref this.t, 0, flag))
				{
					base.PtcVar("cy", (double)base.mbottom).PtcST("golemtoy_mkb_bump_prepare", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.TeCon.setQuakeSinH(5f, 30, 4.7f, 2f, 0);
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				this.rotZR = X.COSI(this.t, 3.3f) * 0.12f * 3.1415927f;
				this.rotYR = X.COSI(this.t, 4.23f) * 0.08f * 3.1415927f;
				this.yshift -= this.TS * 0.8f;
				if (Tk.Progress(ref this.t, 60, true))
				{
					base.PtcVar("cy", (double)(base.y + this.sizey * 0.33f)).PtcVar("by", (double)base.mbottom).PtcST("golemtoy_mkb_bump", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.rotZR = 0f;
					this.rotYR = X.XORSPS() * 0.1f * 3.1415927f;
					EnemyAttr.Splash(this, 0.8f);
					if (this.walk_st == 1)
					{
						this.RecreateJumping(Tk);
						return true;
					}
					base.tackleInit(this.AtkMkb, this.TackleMkb, MGHIT.AUTO);
				}
			}
			if (Tk.prog == PROG.PROG2)
			{
				if (this.t >= 15f)
				{
					this.can_hold_tackle = false;
					base.throw_ray = false;
				}
				if (this.t <= 40f)
				{
					this.rotXR = (-0.5f + X.ZSIN2(this.t, 40f)) * 2f * 0.07f * 3.1415927f;
					this.yshift = X.NI(-30f, 0f, X.ZSIN2(this.t, 40f));
					this.AnmT.vib_y = base.VALWALK(this.AnmT.vib_y, 20f, 1.25f);
				}
				else
				{
					float t = this.t;
					this.yshift = 0f;
					this.rotXR = base.VALWALK(this.rotXR, 0f, 0.009424779f);
					this.AnmT.vib_y = base.VALWALK(this.AnmT.vib_y, 0f, 0.33f);
				}
				if (this.t >= 70f)
				{
					this.rotXR = 0f;
					this.AnmT.vib_y = 0f;
					this.Nai.AddF(NAI.FLAG.ABSORB_FINISHED, 70f);
					return false;
				}
			}
			return true;
		}

		public void RecreateJumping(NaTicket Tk)
		{
			Tk.Recreate(NAI.TYPE.PUNCH_0, -1, true, null);
			Tk.prog = PROG.PROG1;
			this.t = 20f;
			this.walk_time = 0.7f;
			base.throw_ray = false;
		}

		public bool runPunch0_Jump(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.playSndPos("golemtoy_mkb_jump_grawl", 1);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				this.rotZR = base.VALWALK(this.rotZR, -0.18849556f, 0.009424779f);
				this.rotXR = X.COSI(this.t, 3.7f) * 0.05f * 3.1415927f;
				this.rotYR = X.COSI(this.t, 5.23f) * 0.034f * 3.1415927f + X.ZLINE(this.t, 120f) * 0.14f * 3.1415927f;
				this.AnmT.vib_y = base.VALWALK(this.AnmT.vib_y, -20f, 0.18f);
				if (Tk.Progress(ref this.t, 130, true))
				{
					Tk.prog = PROG.PROG1;
					this.walk_time = 0.4f;
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, 20, true))
			{
				this.setWalkXSpeed(0f, false, true);
				this.walk_st = 0;
				base.PtcST("golemtoy_mkb_jump", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				base.tackleInit(this.AtkMkb, this.TackleJump, MGHIT.AUTO);
			}
			if (Tk.prog == PROG.PROG2)
			{
				this.yshift = base.VALWALK(this.yshift, 14f, 0.29f);
				this.rotZR = base.VALWALK(this.rotZR, -this.Phy.walk_xspeed / 0.1f * 0.08f * 3.1415927f, 0.02199115f);
				this.rotXR = X.COSI(this.t, 2.82f) * 0.02f * 3.1415927f;
				this.rotYR = base.VALWALK(this.rotYR, 0f, 0.028274333f);
				this.AnmT.vib_y = X.ZSIN(this.t, 19f) * 20f + X.COSI(this.t, 7.13f) * 2.5f;
				if (this.walk_st == 0)
				{
					this.Phy.addLockGravityFrame(5);
					this.setWalkXSpeed(X.MMX(-0.1f, this.Phy.walk_xspeed + (float)X.MPF(base.x < this.Nai.target_x) * 0.008f * this.TS, 0.1f), true, false);
					if (this.t >= 200f)
					{
						this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK, 0f, -0.14f, -1f, 0, 6, 60, 1, 0);
						this.can_hold_tackle = false;
						this.walk_st = 1;
						base.killPtc("golemtoy_mkb_jump", false);
					}
					else
					{
						this.Phy.addFoc(FOCTYPE.JUMP, 0f, -0.14f * X.NIL(this.walk_time, 1f, this.t, 100f), -4f, -1, 1, 0, -1, 0);
					}
				}
				else
				{
					this.setWalkXSpeed(base.VALWALK(this.Phy.walk_xspeed, 0f, 0.003f), true, false);
					Tk.Progress(ref this.t, 0, base.hasFoot());
				}
			}
			if (Tk.prog == PROG.PROG3)
			{
				this.yshift = base.VALWALK(this.yshift, 0f, 0.04f);
				this.AnmT.vib_y = 20f - 35f * X.ZSIN(this.t, 20f) + 15f * X.ZCOS(this.t - 20f, 40f);
				this.rotZR = base.VALWALK(this.rotZR, 0f, 0.009424779f);
				this.rotXR = (-X.ZSIN(this.t, 34f) * 0.08f + X.ZCOS(this.t - 30f, 70f) * 0.08f) * 3.1415927f;
				this.rotYR = base.VALWALK(this.rotYR, 0f, X.ZSIN(this.t - 30f, 45f) * 0.005f * 3.1415927f);
				if (Tk.Progress(ref this.t, 140, base.hasFoot()))
				{
					return false;
				}
			}
			return true;
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (this.state != NelEnemy.STATE.STAND || this.Absorb != null || Abm.Con.current_pose_priority >= 5)
			{
				return false;
			}
			M2Attackable m2Attackable = MvTarget as M2Attackable;
			if (m2Attackable == null || m2Attackable.getFootManager() == null)
			{
				return false;
			}
			IFootable foot = m2Attackable.getFootManager().get_Foot();
			if (foot != null && !(foot is M2BlockColliderContainer.BCCLine))
			{
				return false;
			}
			if (this.Nai.isFrontType(NAI.TYPE.PUNCH_0, PROG.ACTIVE) && !base.hasFoot())
			{
				this.Phy.addFoc(FOCTYPE.WALK, this.Phy.walk_xspeed * 0.8f, 0f, -1f, 0, 4, 30, 1, 0);
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK, 0f, -0.14f * this.walk_time, -1f, 0, 4, 30, 1, 0);
			}
			if (!base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			Abm.kirimomi_release = true;
			Abm.no_clamp_speed = true;
			Abm.pose_priority = 5;
			Abm.target_pose = "stride_front";
			Abm.no_shuffleframe_on_applydamage = true;
			Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 7, KEY.getRandomKeyBitsLRTB(4));
			Abm.get_Gacha().SoloPositionPixel = new Vector3(0f, -158f, 0f);
			m2Attackable.getPhysic().killSpeedForce(true, true, true, false, false);
			this.walk_st = -1;
			this.rotYR *= 0.2f;
			base.carryable_other_object = true;
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.checkTargetAndLength(pr, 3f) || this.Absorb.Con.use_torture || !this.Absorb.isNotTortureEnable() || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.walk_st == -1)
			{
				this.walk_st = 0;
				base.carryable_other_object = true;
				pr.getFootManager().rideInitTo(this, false);
				this.AnmT.showToFront(true, false);
				this.makeRideRenderTicket(true);
				this.playSndPos("golemtoy_mkb_jump", 1);
				this.Absorb.uipicture_fade_key = (((this.nattr & ENATTR.ACME) != ENATTR.NORMAL) ? "torture_mkb_acme" : "torture_mkb");
				this.walk_time = 0.12f;
			}
			else if (pr.getFootManager().get_Foot() != this)
			{
				return false;
			}
			if (this.walk_st == 0)
			{
				this.walk_st = (int)X.NIXP(32f, 42f);
				this.t = 0f;
			}
			if (this.t >= (float)this.walk_st)
			{
				pr.TeCon.setQuake(6f, 11, 1f, 0);
				pr.getAnimator().animReset(0);
				this.t = 0f;
				this.rotYR = X.XORSPS() * 0.023f * 3.1415927f * (float)((X.XORSP() < 0.3f) ? 2 : 1);
				this.Phy.addLockMoverHitting(HITLOCK.ABSORB, 50f);
				pr.getFootManager().rideInitTo(this, false);
				pr.getPhysic().addLockMoverHitting(HITLOCK.ABSORB, 50f);
				if (X.XORSP() < 0.4f)
				{
					this.walk_st = 0;
				}
				this.walk_time = X.NIXP(0.01f, 0.18f);
				this.yshift = 8f;
				this.playSndPos("golemtoy_mkb_jump", 1);
				base.applyAbsorbDamageTo(pr, this.AtkMkbAb, true, false, X.XORSP() < 0.66f * (1f - 0.55f * (float)pr.ep / 1000f), 1f, false, null, false, true);
				if (X.XORSP() < 0.51f)
				{
					this.Mp.DropCon.setBlood(pr, (int)X.NIXP(4f, 17f), uint.MaxValue, 0f, true);
				}
			}
			this.yshift = base.VALWALK(this.yshift, 0f, 1.4f);
			float num = X.ZSIN(this.t, 30f);
			this.rotXR = X.NI(this.walk_time, -0.013f, num) * 3.1415927f;
			this.AnmT.vib_y = base.VALWALK(this.AnmT.vib_y, 10f + 43f * (1f - X.ZPOW(this.t, 45f)), 2.2f);
			pr.getPhysic();
			return true;
		}

		public override IFootable isCarryable(M2FootManager FootD)
		{
			PR pr = FootD.Mv as PR;
			if (!(pr != null) || !base.carryable_other_object || this.state != NelEnemy.STATE.ABSORB || !base.isCoveringMv(pr, 1f, 1f))
			{
				return null;
			}
			if (this.Absorb == null || !(this.Absorb.getTargetMover() == pr))
			{
				return null;
			}
			return this;
		}

		public override void setAbsorbHitPos(NelAttackInfo Atk, PR Pr)
		{
			Atk.CenterXy(Pr.x + X.XORSPS() * 0.13f, Pr.y + X.XORSPS() * 0.13f, 0f);
		}

		public override float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy)
		{
			if (FootD.Mv as PR != null && base.carryable_other_object && this.state == NelEnemy.STATE.ABSORB)
			{
				dx = this.Phy.move_depert_x - x;
				dy = this.Phy.move_depert_y - 1.92f * this.sizey + (-this.yshift * this.Mp.rCLENB * this.Anm.scaleY - this.AnmT.vib_y * this.Mp.rCLENB) - y;
				return 0.2f;
			}
			return base.fixToFootPos(FootD, x, y, out dx, out dy);
		}

		public override bool runStun()
		{
			float t = this.t;
			this.rotZR = (this.Nai.RANStk(4431) * 0.12f + X.COSI(this.Mp.floort, 136f) * 0.03f) * 3.1415927f;
			this.rotYR = (this.Nai.RANStk(3233) * 0.09f + X.COSI(this.Mp.floort, 155f) * 0.03f) * 3.1415927f;
			this.rotXR = (this.Nai.RANStk(5233) * 0.04f + X.COSI(this.Mp.floort, 141f) * 0.03f) * 3.1415927f;
			return base.runStun();
		}

		public void makeRideRenderTicket(bool flag)
		{
			if (flag)
			{
				if (this.TkRide == null)
				{
					if (this.Mp.M2D.Cam.stabilize_drawing)
					{
						return;
					}
					this.TkRide = this.Mp.MovRenderer.assignDrawable(this.AnmT.order_back, null, new M2RenderTicket.FnPrepareMd(this.FnRenderBack), null, this, null);
					this.MdBack = new MeshDrawer(null, 8, 12);
					this.MdBack.draw_gl_only = true;
					this.MdBack.activate("", this.AnmT.getMainMtr(), false, MTRX.ColWhite, null);
					this.makeMkb(out this.MkbBack);
					this.MkbBack.draw_bits = 6U;
				}
				this.Mkb.draw_bits = 1U;
				return;
			}
			this.Mkb.draw_bits = 7U;
		}

		public override void makeBone(List<Vector3> ABone)
		{
			float num = 2.25f * base.CLEN;
			int num2 = 3;
			float num3 = -0.5f * num;
			for (int i = 0; i < num2; i++)
			{
				ABone.Add(new Vector3(num3, (i == 1) ? 5.2f : 4f, (float)(i * 3)));
				num3 += num / (float)(num2 - 1);
			}
		}

		public override bool drawSpecial(MeshDrawer Md)
		{
			this.drawMokuba(Md, this.Mkb, true);
			return true;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			Aattr.Add(MGATTR.STAB);
		}

		public void drawMokuba(MeshDrawer Md, MokubaDrawer Mkb, bool draw_btm = true)
		{
			if (Mkb == null)
			{
				return;
			}
			Md.Identity();
			if (draw_btm)
			{
				Md.initForImg(Mkb.PIBone, 0);
				float num = 48f * X.Sin(this.rotYR);
				Md.RotaGraph(-num, 9f + this.yshift, 1.5f, this.rotZR, null, false);
				Md.RotaGraph(0f, 9f + this.yshift, 1.5f, this.rotZR, null, false);
				Md.RotaGraph(num, 9f + this.yshift, 1.5f, this.rotZR, null, false);
			}
			Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(1.15f, 1.15f, 1.15f));
			if (this.rotZR != 0f)
			{
				matrix4x *= Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, this.rotZR * 180f * 0.31830987f));
			}
			if (this.rotXR != 0f)
			{
				matrix4x *= Matrix4x4.Rotate(Quaternion.Euler(this.rotXR * 180f * 0.31830987f, 0f, 0f));
			}
			if (this.rotYR != 0f)
			{
				matrix4x *= Matrix4x4.Rotate(Quaternion.Euler(0f, this.rotYR * 180f * 0.31830987f, 0f));
			}
			Mkb.draw(Md, 0f, 12f + this.yshift + this.AnmT.vib_y / this.Anm.scaleY, matrix4x, 0f);
		}

		private bool FnRenderBack(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool color_one_overwrite)
		{
			MdOut = null;
			if (this.MkbBack.draw_bits == 7U)
			{
				return false;
			}
			if (draw_id != 0)
			{
				return false;
			}
			if (this.AnmT.noNeedDraw())
			{
				return false;
			}
			Tk.Matrix = this.Anm.drawMatrix;
			if (need_redraw)
			{
				this.MdBack.clear(false, false);
				this.drawMokuba(this.MdBack, this.MkbBack, false);
			}
			MdOut = this.MdBack;
			return true;
		}

		public float rotXR
		{
			get
			{
				return this.rotXR_;
			}
			set
			{
				if (this.rotXR == value)
				{
					return;
				}
				this.rotXR_ = value;
				this.Anm.need_fine_mesh = true;
			}
		}

		public float rotYR
		{
			get
			{
				return this.rotYR_;
			}
			set
			{
				if (this.rotYR == value)
				{
					return;
				}
				this.rotYR_ = value;
				this.Anm.need_fine_mesh = true;
			}
		}

		public float rotZR
		{
			get
			{
				return this.rotZR_;
			}
			set
			{
				if (this.rotZR == value)
				{
					return;
				}
				this.rotZR_ = value;
				this.Anm.need_fine_mesh = true;
			}
		}

		public float yshift
		{
			get
			{
				return this.yshift_;
			}
			set
			{
				if (this.yshift == value)
				{
					return;
				}
				this.yshift_ = value;
				this.Anm.need_fine_mesh = true;
			}
		}

		private MokubaDrawer Mkb;

		private MokubaDrawer MkbA;

		public float rotYR_;

		public float rotXR_;

		public float rotZR_;

		public float yshift_;

		private M2SoundPlayerItem SndWalk;

		private M2RenderTicket TkRide;

		private MokubaDrawer MkbBack;

		private MeshDrawer MdBack;

		private NASAboveChecker Above;

		private const float max_fly_yspd = 0.14f;

		private const float auto_vib_y_px = 6f;

		protected EnAttackInfo AtkMkb = new EnAttackInfo
		{
			hpdmg0 = 5,
			split_mpdmg = 13,
			burst_vy = -0.14f,
			shield_break_ratio = 0f,
			huttobi_ratio = 1000f,
			is_grab_attack = true
		};

		protected EnAttackInfo AtkMkbAb = new EnAttackInfo(0.037037037f, 0.029411765f)
		{
			hpdmg0 = 5,
			split_mpdmg = 13,
			burst_vy = -0.14f,
			huttobi_ratio = -1000f,
			attr = MGATTR.STAB,
			pee_apply100 = 7f,
			EpDmg = new EpAtk(9, "mkb")
			{
				vagina = 4
			}
		};

		private NOD.TackleInfo TackleMkb = NOD.getTackle("mkb_bump");

		private NOD.TackleInfo TackleJump = NOD.getTackle("mkb_flying");

		private PxlSequence SqMokuba;

		private const float draw_w_scale = 0.7878788f;

		public const int PRI_ATK = 128;

		public const int PRI_WALK = 5;

		private const int ABSORB_PRI = 5;
	}
}
