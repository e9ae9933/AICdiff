using System;
using System.Collections.Generic;
using m2d;
using Spine;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class NelNNusiTentacle : NelEnemyNested
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			ENEMYID id = this.id;
			this.id = ENEMYID.BOSS_NUSI_TENTACLE;
			NOD.BasicData basicData = NOD.getBasicData("BOSS_NUSI_TENTACLE");
			base.base_gravity = 0f;
			this.hp0_remove = false;
			base.appear(_Mp, basicData);
			this.do_not_shuffle_on_cheat = true;
			this.FootD.auto_fix_to_foot = false;
			this.Anm.setPose("cage_close", -1);
			this.Lig.radius = 70f;
			this.Lig.fill_radius = 0f;
			this.Anm.alpha = 0f;
			this.Anm.checkframe_on_drawing = true;
			this.Anm.draw_margin = 5f;
			this.absorb_weight = 1;
			this.Nai.busy_consider_intv = 20f;
			this.ringoutable = false;
			this.no_apply_map_damage = true;
			this.Nai.awake_length = 60f;
			this.cannot_move = true;
			this.Nai.consider_only_onfoot = false;
			this.Nai.fnSleepLogic = (this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal));
			this.FD_getOtherTentacleAbsorbing = new Func<AbsorbManager, bool>(this.getOtherTentacleAbsorbing);
			this.set_enlarge_bouncy_effect = false;
			base.addF((NelEnemy.FLAG)6291584);
			this.AtkUPunch.Prepare(this, true);
			this.AtkStruggle.Prepare(this, true);
			this.AtkGrab.Prepare(this, true);
			this.AtkNormalAbsorb.Prepare(this, true);
		}

		public override NelEnemyNested initNest(NelEnemy _Parent, int array_create_capacity = 4)
		{
			base.initNest(_Parent, array_create_capacity);
			this.Boss = this.Parent as NelNBoss_Nusi;
			return this;
		}

		public void initTentacle(NelNBoss_Nusi _Boss, int _v_id)
		{
			this.Boss = _Boss;
			this.Freeze = new NelNBoss_Nusi.AIFreeze(this.Boss);
			this.v_id = _v_id;
			M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc((int)this.Boss.x, (int)this.Boss.y, AIM.B);
			if (sideBcc != null)
			{
				this.underpos = sideBcc.y;
			}
			else
			{
				this.underpos = this.Boss.y + 7f;
			}
			if (this.v_id >= 2)
			{
				this.sizex0 *= 0.6f;
				this.sizey0 *= 0.5f;
			}
			this.EA = new EnemyAnimatorSpine(this, "boss_nusi__enemy_boss_nusi", "MvT_" + _v_id.ToString(), "tentacle", "enemy_boss_nusi", "main0");
			this.EA.fine_intv = 3f;
			this.EA.showToFront(true, false);
			this.EA.addListenerComplete(new AnimationState.TrackEntryDelegate(this.triggerAnimComplete));
			this.NPos = (this.EA.NestTarget = new CCNestItem(null, "MvT_" + _v_id.ToString()));
			this.Boss.setTentacleEA(this.EA, false);
			this.visible = false;
		}

		public override void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			base.quitSummonAndAppear(clearlock_on_summon);
			this.Phy.addLockWallHitting(this, -1f);
			this.Anm.alpha = 0f;
			base.disappearing = false;
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			this.Unlink();
			if (this.EA != null)
			{
				this.EA.destruct();
				this.EA = null;
			}
			base.destruct();
		}

		public void Unlink()
		{
			this.unlink_lock_on_quit_ticket = 0;
			if (this.LinkTentacle != null)
			{
				NelNBoss_Nusi.TentacleLink linkTentacle = this.LinkTentacle;
				linkTentacle.EA.remListenerEvent(new AnimationState.TrackEntryEventDelegate(this.fnEAEventStruggle));
				this.LinkTentacle = null;
				linkTentacle.Unlink();
			}
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed || this.state == NelEnemy.STATE.DIE || this.EA == null)
			{
				return;
			}
			if (this.set_to_home_position)
			{
				this.set_to_home_position = false;
				this.faintshift_lgt = 0f;
				this.EA.fine_intv = 3f;
				this.angleR = 0f;
				this.export_aim_fix = true;
				base.Size(this.sizex0 * base.CLENM, this.sizey0 * base.CLENM, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				this.visible = false;
				if (this.v_connectable)
				{
					base.throw_ray = false;
					int num = this.v_id;
					if (num <= 1)
					{
						this.moveBy(this.Boss.x + this.Boss.mpf_is_right * this.sizex - this.Phy.move_depert_tstack_x, this.underpos - this.sizey - this.Phy.move_depert_tstack_y, true);
					}
					else
					{
						this.moveBy(this.Boss.x + this.Boss.mpf_is_right * this.sizex - this.Phy.move_depert_tstack_x, this.Boss.y - 5.5f - this.Phy.move_depert_tstack_y, true);
					}
				}
				else
				{
					base.throw_ray = true;
				}
			}
			if (this.visible)
			{
				this.EA.checkFrameManual(this.TS, false);
				if (this.Lig.Col.a < 255)
				{
					this.Lig.Col.a = (byte)X.Mn((int)(this.Lig.Col.a + 5), 255);
				}
				if (this.DmgCounterSPos.z > 0f)
				{
					this.DmgCounterSPos.z = this.DmgCounterSPos.z - this.TS;
					return;
				}
			}
			else if (this.Lig.Col.a > 0)
			{
				this.Lig.Col.a = (byte)X.Mx((int)(this.Lig.Col.a - 5), 0);
			}
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			NelEnemy.STATE state = this.state;
			base.changeState(st);
			if (this.EA == null)
			{
				return this;
			}
			if (state == NelEnemy.STATE.ABSORB)
			{
				base.carryable_other_object = false;
			}
			if (state == NelEnemy.STATE.ABSORB && st == NelEnemy.STATE.STAND)
			{
				if (this.LinkTentacle != null && this.LinkTentacle.curpose == NelNBoss_Nusi.TENPOSE.ground)
				{
					this.LinkTentacle.EAPose(NelNBoss_Nusi.TENPOSE.ground2stand, true, -1);
				}
				else
				{
					this.faintshift_lgt = 0f;
					this.Unlink();
				}
				base.Size(this.sizex0 * base.CLENM, this.sizey0 * base.CLENM, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				this.Phy.addLockMoverHitting(HITLOCK.ABSORB, 90f);
				this.Nai.AddTicket(NAI.TYPE.BACKSTEP, 150, true);
				this.set_to_home_position = false;
			}
			else
			{
				this.EA.showToFront(true, false);
			}
			return this;
		}

		public void AddFreeze(int level, float t, float m1_extend = 180f)
		{
			this.Freeze.Set(this.Nai, level, t, m1_extend);
		}

		public void AddFreezeToBoss(int level, float t)
		{
			this.Boss.AddFreeze(level, t);
		}

		public void clearFreeze()
		{
			this.Freeze.Clear();
		}

		private bool considerNormal(NAI Nai)
		{
			if (this.v_id >= 2 || this.set_to_home_position || !this.Boss.considerable)
			{
				return true;
			}
			bool flag;
			if (!this.Freeze.canMove(Nai, out flag))
			{
				return Nai.AddTicketB(NAI.TYPE.WAIT, 128, true);
			}
			float num = this.BRANtk(4123, 0.6f);
			float num2 = 0.3f;
			if (Nai.isPrGaraakiState() && this.BRANtk(3871, 0.6f) < 0.85f)
			{
				this.Boss.progressPhase2Lock(flag);
				return Nai.AddTicketB(NAI.TYPE.PUNCH_2, 129, true);
			}
			if (base.AimPr.mbottom > this.underpos - 2.6f)
			{
				bool flag2 = X.BTW(-4f, this.atk_xdif, this.TkiUPunch.difx_map + this.TkiUPunch.radius * 0.4f) && num < 0.4f;
				if (num < 0.09f || num < 0f || flag2)
				{
					this.Boss.progressPhase2Lock(flag);
					return Nai.AddTicketB(NAI.TYPE.PUNCH, 129, true);
				}
				if (num < 0.73f && !Nai.hasTypeLock(NAI.TYPE.PUNCH_1))
				{
					this.Boss.progressPhase2Lock(flag);
					return Nai.AddTicketB(NAI.TYPE.PUNCH_1, 129, true);
				}
				if (!flag2)
				{
					num2 = X.Scr(num2, 0.7f);
				}
			}
			else
			{
				bool flag3 = false;
				if (X.Abs(base.AimPr.y - (this.underpos + this.TkiMPunch.dify_map)) < 4f)
				{
					flag3 = X.BTW(-4f, this.atk_xdif, this.TkiMPunch.difx_map + this.TkiMPunch.radius * 0.4f) && num < 0.4f;
					if (num < 0.09f || num < 0f || flag3)
					{
						this.Boss.progressPhase2Lock(flag);
						return Nai.AddTicketB(NAI.TYPE.PUNCH_0, 129, true);
					}
				}
				if (!flag3)
				{
					num2 = X.Scr(num2, 0.4f);
				}
			}
			if (this.BRANtk(3471, 0.6f) < num2)
			{
				this.Boss.progressPhase2Lock(flag);
				return Nai.AddTicketB(NAI.TYPE.PUNCH_2, 129, true);
			}
			return Nai.AddTicketB(NAI.TYPE.WAIT, 5, true);
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.PUNCH:
			case NAI.TYPE.PUNCH_0:
				return this.runPunch_UnderPunch(this.initTicket(Tk), Tk, Tk.type == NAI.TYPE.PUNCH_0);
			case NAI.TYPE.PUNCH_1:
				return this.runPunch1_Struggle(this.initTicket(Tk), Tk);
			case NAI.TYPE.PUNCH_2:
				return this.runPunch2_Grab(this.initTicket(Tk), Tk);
			case NAI.TYPE.PUNCH_WEED:
			case NAI.TYPE.MAG_0:
			case NAI.TYPE.MAG_1:
			case NAI.TYPE.MAG_2:
				break;
			case NAI.TYPE.MAG:
				return this.runMag_FaintTentacle(this.initTicket(Tk), Tk);
			case NAI.TYPE.GUARD:
				return this.runGuard_StunEscape(this.initTicket(Tk), Tk);
			default:
				switch (type)
				{
				case NAI.TYPE.BACKSTEP:
					return this.runBackstep_FaintTentacleEnd(this.initTicket(Tk), Tk);
				case NAI.TYPE.WARP:
					return this.runWarp_BigRun(this.initTicket(Tk), Tk);
				case NAI.TYPE.WAIT:
					if (Tk.initProgress(this))
					{
						this.walk_time = X.NIXP(10f, 30f);
						this.t = 0f;
						this.Unlink();
					}
					return this.t < this.walk_time && this.Boss.isFaintPreparing(false);
				}
				break;
			}
			return false;
		}

		public bool initTicket(NaTicket Tk)
		{
			if (Tk.initProgress(this))
			{
				this.setAim(this.Boss.aim, false);
				return true;
			}
			return false;
		}

		public bool initLink(NaTicket Tk, float ea_fine_intv = 1f)
		{
			this.Unlink();
			this.LinkTentacle = this.Boss.initLink(this);
			if (this.LinkTentacle != null)
			{
				base.throw_ray = false;
				this.set_to_home_position = false;
				this.LinkTentacle.EA.fine_intv = ea_fine_intv;
				return true;
			}
			Tk.Recreate(NAI.TYPE.WAIT, 128, false, null);
			return false;
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (this.EA == null)
			{
				return;
			}
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.PUNCH_2)
				{
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
				}
				if (Tk.type != NAI.TYPE.WAIT)
				{
					this.set_to_home_position = true;
				}
			}
			else
			{
				this.set_to_home_position = true;
			}
			this.MgFaintAtk = null;
			this.can_hold_tackle = false;
			this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
			this.Phy.remLockMoverHitting(HITLOCK.ABSORB);
			if (this.unlink_lock_on_quit_ticket > 0)
			{
				this.unlink_lock_on_quit_ticket -= 1;
			}
			else
			{
				this.Unlink();
			}
			this.EA.showToFront(true, false);
			this.EA.timescale = 1f;
			this.visible = false;
			base.quitTicket(Tk);
		}

		private bool runPunch_UnderPunch(bool init_flag, NaTicket Tk, bool middlepunch = false)
		{
			NOD.TackleInfo tackleInfo = (middlepunch ? this.TkiMPunch : this.TkiUPunch);
			if (init_flag)
			{
				this.t = 0f;
				if (!this.initLink(Tk, 1f))
				{
					return true;
				}
				this.LinkTentacle.EAPose(middlepunch ? NelNBoss_Nusi.TENPOSE.middlepunch_0 : NelNBoss_Nusi.TENPOSE.underpunch_0, true, -1);
				base.throw_ray = true;
				if (middlepunch)
				{
					this.moveBy(0f, this.underpos - this.sizey + tackleInfo.dify_map - base.y, true);
				}
				base.Size(tackleInfo.x_reachable * 0.5f * base.CLENM, X.Abs(tackleInfo.radius) * base.CLENM * 0.7f, (this.Boss.mpf_is_right > 0f) ? ALIGN.LEFT : ALIGN.RIGHT, ALIGNY.MIDDLE, false);
				this.dangerousStar(tackleInfo.difx_map, 0f, tackleInfo.radius, 130f);
				this.Boss.AddFreezeToTentacleToOther(1, 80f, this, 180f);
				this.Boss.AddFreezeToTentacleToOther(0, 230f, this, 180f);
				this.Boss.AddFreeze(1, 80f);
				this.Boss.AddFreeze(0, 210f);
				this.playSndPos("nusit_punch_prep", 1);
			}
			if (this.LinkTentacle == null)
			{
				return false;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 120, true))
			{
				this.LinkTentacle.EAPose(middlepunch ? NelNBoss_Nusi.TENPOSE.middlepunch_1 : NelNBoss_Nusi.TENPOSE.underpunch_1, true, -1);
				this.LinkTentacle.EA.timescale = 2f;
				base.PtcVar("cx", (double)this.atk_cx).PtcVar("cy", (double)base.y).PtcVar("lgt", (double)tackleInfo.x_reachable)
					.PtcVar("middle", (double)(middlepunch ? 1 : 0))
					.PtcST("nusit_underpunch", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, 4, true))
			{
				this.AtkUPunch.press_state_replace = byte.MaxValue;
				MagicItem magicItem = base.tackleInit(this.AtkUPunch, tackleInfo, MGHIT.AUTO);
				magicItem.sx = this.atk_cx - this.Phy.tstacked_x;
				magicItem.sy = 0f;
				magicItem.dx = tackleInfo.difx_map * this.Boss.mpf_is_right;
				magicItem.dy = 0f;
				base.throw_ray = false;
				this.walk_st = 0;
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.t >= 6f && this.can_hold_tackle)
				{
					base.throw_ray = true;
					this.can_hold_tackle = false;
					if (base.nattr_has_mattr)
					{
						EnemyAttr.SplashSOnAir(this.Parent, this.Phy.tstacked_x - this.Boss.mpf_is_right * 3f, this.Phy.tstacked_y, tackleInfo.difx_map, (this.Boss.mpf_is_right > 0f) ? 0f : 3.1415927f, 0f, 0.4f, 1f, 6, 1.6f);
					}
				}
				if (this.t >= 40f)
				{
					this.LinkTentacle.EA.fine_intv = 5f;
				}
				if (this.t >= 90f && this.walk_st == 0)
				{
					this.walk_st = 1;
					this.set_to_home_position = true;
				}
				if (Tk.Progress(ref this.t, 140, true))
				{
					this.Boss.TentacleAfterDelay(Tk, 240f + this.Boss.tentacle_after_delay);
					this.Unlink();
					return false;
				}
			}
			return true;
		}

		private bool runPunch1_Struggle(bool init_flag, NaTicket Tk)
		{
			NOD.TackleInfo tkiStruggle = this.TkiStruggle;
			if (init_flag)
			{
				this.t = 0f;
				if (!this.initLink(Tk, 1f))
				{
					return true;
				}
				this.LinkTentacle.EAPose(NelNBoss_Nusi.TENPOSE.struggle_0, true, -1);
				this.LinkTentacle.EA.fine_intv = 2f;
				base.throw_ray = true;
				this.dangerousStar(tkiStruggle.difx_map, 0f, tkiStruggle.radius, 196f);
				this.Boss.AddFreezeToTentacleToOther(1, 46f, this, 180f);
				this.Boss.AddFreezeToTentacleToOther(0, 226f, this, 180f);
				this.Boss.AddFreeze(1, 36f);
				this.Boss.AddFreeze(0, 176f);
				this.playSndPos("nusit_strg_prep", 1);
				this.Boss.TentacleAddTypeLock(NAI.TYPE.PUNCH_1, 466f);
			}
			if (this.LinkTentacle == null)
			{
				return false;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				CCNestItem nestTarget = this.LinkTentacle.EA.NestTarget;
				nestTarget.shifty = base.VALWALK(nestTarget.shifty, this.underpos - this.Boss.y - 0.3f, 0.2f);
				nestTarget.shiftx = base.VALWALK(nestTarget.shiftx, -1.5f, 0.2f);
				if (Tk.Progress(ref this.t, 76, true))
				{
					this.LinkTentacle.EAPose(NelNBoss_Nusi.TENPOSE.struggle_1, true, -1);
					this.LinkTentacle.EA.addListenerEvent(new AnimationState.TrackEntryEventDelegate(this.fnEAEventStruggle));
					this.LinkTentacle.EA.timescale = 1.6f;
					this.playSndPos("nusit_struggle_trigger", 1);
					this.LinkTentacle.EA.fine_intv = 1f;
					base.throw_ray = false;
					this.walk_time = 0f;
					base.Size(tkiStruggle.x_reachable * 0.1f * base.CLENM, X.Abs(tkiStruggle.radius) * base.CLENM * 0.7f, (this.Boss.mpf_is_right > 0f) ? ALIGN.LEFT : ALIGN.RIGHT, ALIGNY.MIDDLE, false);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_time > 0f)
				{
					this.walk_time -= this.TS;
					if (this.walk_time <= 0f)
					{
						this.can_hold_tackle = false;
						base.throw_ray = false;
					}
				}
				if (Tk.Progress(ref this.t, 170, true))
				{
					this.can_hold_tackle = false;
					base.throw_ray = false;
					this.LinkTentacle.EAPose(NelNBoss_Nusi.TENPOSE.struggle_2, true, -1);
					this.LinkTentacle.EA.fine_intv = 5f;
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				this.LinkTentacle.walkShiftPosToDefault(0.04f * this.TS);
				if (Tk.Progress(ref this.t, 40, true))
				{
					this.Boss.TentacleAfterDelay(Tk, 100f + this.Boss.tentacle_after_delay);
					this.set_to_home_position = true;
					this.Unlink();
					return false;
				}
			}
			return true;
		}

		public void fnEAEventStruggle(TrackEntry trackEntry, Event e)
		{
			if (base.destructed)
			{
				return;
			}
			if (e.Data.Name == "attack" && this.Nai.isFrontType(NAI.TYPE.PUNCH_1, PROG.ACTIVE))
			{
				float num = ((e.Int == 1) ? 0.25f : ((e.Int == 2) ? 0.56f : 1f));
				base.Size(this.TkiStruggle.x_reachable * num * base.CLENM * 0.5f, X.Abs(this.TkiStruggle.radius) * base.CLENM * 0.7f, (this.Boss.mpf_is_right > 0f) ? ALIGN.LEFT : ALIGN.RIGHT, ALIGNY.MIDDLE, false);
				float num2 = this.TkiStruggle.difx_map * num;
				float tstacked_x = this.Phy.tstacked_x;
				base.PtcVar("cx", (double)tstacked_x).PtcVar("lgt", (double)(num2 * 0.5f)).PtcST("nusit_strg_hit", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.AtkUPunch.press_state_replace = ((e.Int >= 3) ? 3 : byte.MaxValue);
				MagicItem magicItem = base.tackleInit(this.AtkUPunch, this.TkiStruggle, MGHIT.AUTO);
				magicItem.sx = this.atk_cx - tstacked_x;
				magicItem.sy = 0f;
				magicItem.dx = num2 * this.Boss.mpf_is_right;
				magicItem.dy = 0f;
				this.walk_time = 8f;
				if (base.nattr_has_mattr)
				{
					EnemyAttr.Splash(this.Parent, this.Boss.x + this.Boss.mpf_is_right * (num2 * 0.875f), base.mbottom - 0.125f, 3f, 1f, 3f);
				}
			}
		}

		private bool runPunch2_Grab(bool init_flag, NaTicket Tk)
		{
			NOD.TackleInfo tkiGrab = this.TkiGrab;
			if (init_flag)
			{
				this.t = 0f;
				if (!this.initLink(Tk, 1f))
				{
					return true;
				}
				this.LinkTentacle.EAPose(NelNBoss_Nusi.TENPOSE.ground, true, -1);
				this.LinkTentacle.EA.fine_intv = 2f;
				this.setAim(this.Boss.getTentacleGrabAim(this), false);
				this.angleR = ((this.aim == AIM.T) ? 1.5707964f : (-1.5707964f));
				base.throw_ray = true;
				this.export_aim_fix = false;
				base.Size(X.Abs(tkiGrab.radius) * base.CLENM, tkiGrab.y_reachable * base.CLENM, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				this.Boss.AddFreezeToTentacleToOther(1, 70f, this, 180f);
				this.Boss.AddFreezeToTentacleToOther(0, 228f, this, 180f);
				this.Boss.AddFreeze(1, 146f);
				this.Boss.AddFreeze(0, 178f);
				this.playSndPos("nusit_grab_prep_0", 1);
				this.walk_st = 0;
			}
			if (this.LinkTentacle == null)
			{
				return false;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				CCNestItem nestTarget = this.LinkTentacle.EA.NestTarget;
				nestTarget.shifty = base.VALWALK(nestTarget.shifty, this.underpos - this.Boss.y - 0.3f, 0.04f);
				if (Tk.Progress(ref this.t, 55, true))
				{
					base.PtcVar("cx", (double)(this.Boss.x + nestTarget.shiftx)).PtcVar("cy", (double)this.underpos).PtcVar("ay", (double)CAim._YD(this.aim, 1))
						.PtcVar("ax", (double)this.Boss.mpf_is_right)
						.PtcST("nusit_grab_prep_1", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (Tk.prog == PROG.PROG0 && this.t >= 55f)
			{
				float num = 0f;
				float num2 = X.XORSPS() * 1.2f + this.Nai.target_x;
				if (this.Summoner != null)
				{
					M2LpSummon lp = this.Summoner.Lp;
					num2 = X.MMX((float)lp.mapx + 0.5f, num2, (float)(lp.mapx + lp.mapw) - 0.5f);
				}
				if (this.aim == AIM.T)
				{
					num = this.underpos - base.y;
				}
				else
				{
					M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc((int)num2, (int)this.Boss.y, AIM.T);
					if (sideBcc == null)
					{
						Tk.prog = PROG.PROG4;
						this.t = 0f;
					}
					else
					{
						num = sideBcc.slopeBottomY(num2) - base.y;
					}
				}
				if (Tk.Progress(ref this.t, 0, Tk.prog == PROG.PROG0))
				{
					this.moveBy(num2 - base.x, num, true);
					this.visible = true;
					this.EAPose(NelNBoss_Nusi.TENPOSE.grab_prepare);
					this.dangerousStarY(0f, tkiGrab.dify_map, tkiGrab.radius, 99f);
					base.PtcVar("ay", (double)CAim._YD(this.aim, 1)).PtcST("nusit_grab_prep_2", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, 90, true))
			{
				base.PtcVar("ay", (double)CAim._YD(this.aim, 1)).PtcST("nusit_grab_trigger", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.EAPose(NelNBoss_Nusi.TENPOSE.atk);
				this.walk_st = 0;
				this.EA.timescale = 1.5f;
				this.EA.fine_intv = 1f;
			}
			if (Tk.prog == PROG.PROG2)
			{
				this.DmgCounterSPos.Set(base.x, this.Boss.y - 2.1f, 90f);
				if (this.walk_st == 0 && this.t >= 18f)
				{
					this.walk_st = 1;
					this.absorb_weight = 2;
					MagicItem magicItem = base.tackleInit(this.AtkGrab, tkiGrab, MGHIT.AUTO);
					magicItem.sx = 0f;
					magicItem.sy = 0f;
					magicItem.dx = 0f;
					magicItem.dy = (float)(-(float)CAim._YD(this.aim, 1)) * tkiGrab.dify_map;
					base.throw_ray = false;
				}
				if (this.walk_st == 1 && this.t >= 24f && base.nattr_has_mattr)
				{
					this.walk_st = 2;
					float num3 = base.y + (float)((this.aim == AIM.T) ? (-4) : 1);
					EnemyAttr.SplashSOnAir(this.Parent, base.x, num3, 8f, CAim.get_agR(this.aim, 0f), 0.7f, 0.2f, 1f, 5, 1.6f);
				}
				if (this.t >= 38f)
				{
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(ref this.t, 98, true))
				{
					this.walk_st = 0;
					this.EA.fine_intv = 3f;
					this.EAPose(NelNBoss_Nusi.TENPOSE.atk2stand);
					this.EA.timescale = 1.5f;
					base.PtcVar("ay", (double)CAim._YD(this.aim, 1)).PtcST("nusit_grab_end", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (Tk.prog == PROG.PROG3)
			{
				float num4 = X.ZSIN(this.t, 90f);
				this.faintshift_lgt = -tkiGrab.dify_map * num4 * 1.3f;
				if (Tk.Progress(ref this.t, 90, true))
				{
					this.walk_st = 0;
				}
			}
			if (Tk.prog == PROG.PROG4)
			{
				if (this.walk_st == 0)
				{
					this.walk_st = 1;
					this.LinkTentacle.EA.fine_intv = 5f;
					this.LinkTentacle.EAPose(NelNBoss_Nusi.TENPOSE.ground2stand, true, -1);
				}
				this.LinkTentacle.walkShiftPosToDefault(0.04f * this.TS);
				if (this.t >= 100f)
				{
					this.Boss.TentacleAfterDelay(Tk, this.Boss.tentacle_after_delay);
					return false;
				}
			}
			return this;
		}

		public void initBigRun()
		{
			this.Nai.AddTicket(NAI.TYPE.WARP, 147, true);
			this.Nai.RemF(NAI.FLAG.AWAKEN);
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
		}

		public void bigRunColliderWalk()
		{
			if (this.Nai.isFrontType(NAI.TYPE.WARP, PROG.ACTIVE))
			{
				this.walk_st = 1;
			}
		}

		private bool runWarp_BigRun(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				base.throw_ray = true;
				this.walk_st = 0;
				this.set_to_home_position = false;
				base.Size(2f * base.CLENM, 12f * base.CLENM, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				this.Phy.addTranslateStack(0f, this.Boss.y - this.Phy.tstacked_y);
			}
			if (!this.Boss.getAI().isFrontType(NAI.TYPE.WARP, PROG.ACTIVE))
			{
				return false;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				this.Phy.addTranslateStack(this.Boss.x - this.Phy.tstacked_x, 0f);
				if (this.walk_st == 1)
				{
					M2BlockColliderContainer.BCCLine fallableBcc = this.Mp.getFallableBcc(base.x, this.Boss.y, this.sizex, 18f, -1f, true, false, null);
					if (fallableBcc != null)
					{
						this.t = 0f;
						this.Phy.addTranslateStack(0f, fallableBcc.slopeBottomY(base.x) - this.sizey - this.Phy.tstacked_y);
						this.walk_st = 2;
						base.throw_ray = false;
					}
					else
					{
						this.walk_st = 0;
						base.throw_ray = true;
					}
				}
				if (this.walk_st == 2 && this.t >= 10f)
				{
					base.throw_ray = true;
				}
			}
			return true;
		}

		public void initFaintTentacle(float _angleR)
		{
			this.Unlink();
			if (base.isAbsorbState())
			{
				this.releaseAbsorb(this.Absorb);
				base.changeStateToNormal();
			}
			this.Nai.AddTicket(NAI.TYPE.MAG, 148, true);
			this.Nai.RemF(NAI.FLAG.AWAKEN);
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			this.angleR = _angleR;
		}

		private bool runMag_FaintTentacle(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = (float)(80 - this.v_id * 9);
				this.visible = false;
				this.set_to_home_position = false;
				this.walk_st = 0;
				base.throw_ray = true;
				this.Phy.addLockMoverHitting(HITLOCK.DAMAGE, -1f);
				base.Size(48f, 48f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				this.Nai.clearTypeLock();
				this.MgFaintAtk = null;
				this.can_hold_tackle = false;
				this.export_aim_fix = false;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.walk_st == 0)
				{
					if (base.AimPr == null || this.Nai.isPrTortured())
					{
						return true;
					}
					if (this.Summoner == null || this.Summoner.AreaContainsMv(base.AimPr, 0f))
					{
						this.walk_st = 1;
					}
				}
				if (this.walk_st == 1 && Tk.Progress(ref this.t, 100, true))
				{
					Vector3 vector = this.fineFaintTentaclePos(-1f);
					if (!EnemySummoner.isActiveBorder() && vector.z == 0f)
					{
						this.visible = false;
					}
					else
					{
						this.visible = true;
					}
					this.walk_st = 0;
					this.playSndPos("nusi_tentacle_prepare", 1);
					this.EAPose(NelNBoss_Nusi.TENPOSE.atk_prepare);
					this.EA.timescale = 1f * (1f - (float)this.v_id * 0.08f);
				}
			}
			float num = 0f;
			if (Tk.prog == PROG.PROG0)
			{
				num = 0.006283186f;
				if (this.t <= 24f)
				{
					float num2 = 0.21f * (1f - X.ZPOWV(this.t, 24f));
					this.Phy.addFoc(FOCTYPE.WALK, num2 * X.Cos(this.angleR), -num2 * X.Sin(this.angleR), -1f, -1, 1, 0, -1, 0);
				}
				if (Tk.Progress(ref this.t, 10, this.Boss.isFaintCounterExecuting(false)))
				{
					this.EAPose(NelNBoss_Nusi.TENPOSE.faint_attack);
					this.t = (float)(85 - this.v_id * 4);
					this.walk_time = 0f;
					this.visible = true;
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				num = 0.3455752f;
				if (this.walk_time < 30f)
				{
					this.Phy.addFoc(FOCTYPE.WALK, -0.1f * X.Cos(this.angleR), 0.1f * X.Sin(this.angleR), -1f, -1, 1, 0, -1, 0);
					this.walk_time += this.TS;
				}
				if (Tk.Progress(ref this.t, 100, this.Nai.target_hasfoot || !this.Nai.isPrSpecialAttacking()))
				{
					Vector3 vector2 = this.fineFaintTentaclePos(0f);
					base.PtcVar("aim", (double)vector2.z).PtcVar("agR", (double)this.angleR).PtcST("nusi_fainttentacle_appear", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.EAPose(NelNBoss_Nusi.TENPOSE.faint_attack);
					this.t = (float)(88 - this.v_id * 2);
					this.walk_st = 0;
					this.walk_time = 0f;
				}
			}
			if (Tk.prog < PROG.PROG3 && (!this.Boss.isFaintCounter() || this.Boss.isFaintCured(false)))
			{
				Tk.prog = PROG.PROG4;
				this.t = X.NIXP(-100f, -40f);
				this.walk_st = 0;
			}
			if (Tk.prog == PROG.PROG2)
			{
				if (this.t >= 100f)
				{
					if (this.walk_st == 0)
					{
						this.walk_st = 1;
						this.NoDamage.Clear();
						base.throw_ray = false;
					}
					if (this.MgFaintAtk == null || this.MgFaintAtk.id != this.faintatk_id || this.MgFaintAtk.killed)
					{
						this.absorb_weight = 1;
						MagicItem magicItem = (this.MgFaintAtk = base.tackleInit(this.AtkCaptureFaint, 0f, 0f, 2.5f, true, false, MGHIT.AUTO));
						this.faintatk_id = magicItem.id;
						magicItem.Ray.check_hit_wall = false;
						magicItem.Ray.check_other_hit = true;
						magicItem.Ray.penetrate = true;
						magicItem.Ray.HitLock(0f, null);
					}
					this.walk_time += this.TS;
					if (this.walk_st == 1000)
					{
						if (this.MgFaintAtk != null)
						{
							this.MgFaintAtk.kill(-1f);
							this.MgFaintAtk = null;
							this.t = (float)(this.Nai.isPrAlive() ? 80 : 40);
						}
						this.walk_st = 1;
					}
				}
				if (this.Nai.isPrAbsorbed())
				{
					num = 0.15707964f;
					if (this.MgFaintAtk != null)
					{
						this.MgFaintAtk.sz = -5f;
						this.MgFaintAtk.sx = this.Nai.AimPr.x - base.x;
						this.MgFaintAtk.sy = this.Nai.AimPr.y - base.y;
					}
				}
				else if (this.MgFaintAtk != null)
				{
					this.MgFaintAtk.sx = 50f * X.Cos(this.angleR);
					this.MgFaintAtk.sy = -50f * X.Sin(this.angleR);
				}
				if (!this.Boss.isFaintCounter())
				{
					Tk.prog = PROG.PROG3;
					this.t = 0f;
				}
			}
			if (Tk.prog == PROG.PROG3)
			{
				this.can_hold_tackle = false;
				this.EA.timescale = -1f;
				if (this.t > 60f)
				{
					Tk.AfterDelay(260f);
					return false;
				}
				float num3 = 0.1f + 0.24f * X.ZPOWV(this.t, 60f);
				this.Phy.addFoc(FOCTYPE.WALK, -num3 * X.Cos(this.angleR), num3 * X.Sin(this.angleR), -1f, -1, 1, 0, -1, 0);
			}
			else if (Tk.prog == PROG.PROG4)
			{
				this.can_hold_tackle = false;
				this.EA.timescale = -1f;
				if (this.t >= 0f)
				{
					if (this.walk_st == 0)
					{
						this.walk_st = 1;
						this.playSndPos("nusi_tentacle_prepare", 1);
					}
					if (this.t > 60f)
					{
						Tk.AfterDelay(260f);
						return false;
					}
					float num4 = 0.1f + 0.24f * X.ZPOWV(this.t, 60f);
					this.Phy.addFoc(FOCTYPE.WALK, -num4 * X.Cos(this.angleR), num4 * X.Sin(this.angleR), -1f, -1, 1, 0, -1, 0);
				}
			}
			else if (num != 0f)
			{
				this.angleR = base.VALWALKANGLER(this.angleR, this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y), num);
			}
			return true;
		}

		public override bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			if (base.destructed)
			{
				return false;
			}
			if (this.state == NelEnemy.STATE.STAND && this.Nai.isFrontType(NAI.TYPE.MAG, PROG.ACTIVE) && HitMv != null && HitMv.Mv is PR)
			{
				if (this.Nai.getCurTicket().prog == PROG.PROG2 && this.walk_st < 1000)
				{
					this.walk_st = 1000;
					this.t = 100f;
				}
				(HitMv.Mv as PR).penetrateNoDamageTime(NDMG._ALL, 1000);
			}
			return base.initPublishAtk(Mg, Atk, hittype, HitMv);
		}

		private Vector3 fineFaintTentaclePos(float margin_in = 0f)
		{
			M2LpSummon m2LpSummon = ((this.Summoner != null) ? this.Summoner.Lp : null);
			Vector2 vector;
			Vector2 vector2;
			if (m2LpSummon != null)
			{
				vector = new Vector2(m2LpSummon.mapcx, m2LpSummon.mapcy);
				vector2 = new Vector2((float)m2LpSummon.mapw, (float)m2LpSummon.maph);
			}
			else
			{
				vector = new Vector2(this.Boss.x + this.Boss.mpf_is_right * 10f, this.Boss.y - 2f);
				vector2 = new Vector2(22f, 12f);
			}
			Vector3 vector3 = X.BorderRectAtAngleR(vector2.x, vector2.y, 3.1415927f + this.angleR);
			this.setAim(CAim.get_opposite((AIM)vector3.z), false);
			vector3.x += vector.x;
			vector3.y = -vector3.y + vector.y;
			if (margin_in != 0f)
			{
				vector3.x += X.Cos(this.angleR) * margin_in;
				vector3.y += -X.Sin(this.angleR) * margin_in;
			}
			this.moveBy(vector3.x - base.x, vector3.y - base.y, true);
			this.FootD.initJump(false, false, false);
			return vector3;
		}

		public void initStun()
		{
			if (base.isAbsorbState())
			{
				base.changeStateToNormal();
			}
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			this.Nai.clearTicket(-1, true);
			this.Nai.AddTicket(NAI.TYPE.GUARD, 153, true);
			this.Nai.RemF((NAI.FLAG)2050);
			this.Ser.Cure(SER.TIRED);
			this.set_to_home_position = false;
		}

		private bool runGuard_StunEscape(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.Phy.addLockMoverHitting(HITLOCK.ABSORB, 1000f);
				this.visible = true;
				base.throw_ray = false;
				this.set_to_home_position = false;
				this.t = 180f - X.XORSP() * 34f;
				this.Nai.RemF(NAI.FLAG.ABSORB_FINISHED);
				this.EAPose(NelNBoss_Nusi.TENPOSE.faint_countered);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 200, true))
			{
				this.playSndPos("nusi_tentacle_act", 1);
				this.playSndPos("nusi_leaf_laugh", 1);
				this.EAPose(NelNBoss_Nusi.TENPOSE.faint_back);
				this.EA.timescale = 1f * X.NIXP(0.5f, 1.25f);
			}
			if (Tk.prog == PROG.PROG0)
			{
				float num = X.NI(0.02f, 0.38f, X.ZPOW(this.t, 60f));
				this.Phy.addFoc(FOCTYPE.WALK, -num * X.Cos(this.angleR), num * X.Sin(this.angleR), -1f, -1, 1, 0, -1, 0);
				if (Tk.Progress(ref this.t, 60, true))
				{
					this.visible = false;
					this.set_to_home_position = true;
				}
			}
			return Tk.prog != PROG.PROG1 || this.Boss.isStunned() || !this.Boss.is_alive;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Absorb = null, bool penetrate = false)
		{
			this.unlink_lock_on_quit_ticket = 1;
			if (Absorb.Con.use_torture)
			{
				Absorb.Con.countTortureItem((AbsorbManager V) => V.isTortureUsing(), true);
			}
			if (!base.initAbsorb(Atk, MvTarget, Absorb, penetrate))
			{
				return false;
			}
			this.visible = true;
			base.disappearing = false;
			this.set_to_home_position = (this.export_aim_fix = false);
			this.playSndPos("nusi_tentacle_prepare", 1);
			this.EA.alpha = 1f;
			Absorb.target_pose = null;
			Absorb.kirimomi_release = false;
			Absorb.release_from_publish_count = true;
			Absorb.pose_priority = 94;
			PrGachaItem gacha = Absorb.get_Gacha();
			this.walk_st = (this.Boss.isFaintCounterExecuting(false) ? 1000 : 0);
			if (this.walk_st >= 1000)
			{
				this.Unlink();
				base.Size(266f, 266f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				if (DIFF.I == 0)
				{
					gacha.activateNotDiffFix(PrGachaItem.TYPE.REP, 24 * X.Mx(1, this.Boss.absorb_tap_count), 63U);
				}
				else
				{
					gacha.activateNotDiffFix(PrGachaItem.TYPE.SEQUENCE, 9 * X.Mx(1, this.Boss.absorb_tap_count), 63U);
				}
				this.angleR = this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y);
			}
			else
			{
				gacha.activate(PrGachaItem.TYPE.REP, 6, 63U);
			}
			Absorb.publish_float = true;
			return true;
		}

		public override bool runAbsorb()
		{
			if (this.Absorb == null)
			{
				return false;
			}
			if (this.walk_st < 0)
			{
				return this.t < 40f;
			}
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, false))
			{
				this.walk_st = ((this.walk_st >= 1000) ? (-1001) : (-1));
				this.t = 0f;
				return true;
			}
			M2Phys physic = pr.getPhysic();
			if (this.walk_st < 1000)
			{
				if (this.walk_st == 0)
				{
					this.t = 0f;
					this.walk_st = 1;
					this.walk_time = 80f;
					this.faintshift_lgt_ = 0f;
					base.carryable_other_object = true;
					this.EAPose(NelNBoss_Nusi.TENPOSE.atk_absorb);
				}
				IFootable foot = pr.getFootManager().get_Foot();
				if (!(foot is NelNNusiTentacle))
				{
					if (X.Abs(pr.x - base.x) > 2.5f || this.walk_st == 2 || this.Absorb.Con.use_torture)
					{
						return false;
					}
					pr.getFootManager().rideInitTo(this, false);
					this.walk_st = 2;
					return true;
				}
				else
				{
					bool flag = false;
					foot as NelNNusiTentacle == this;
					if (this.walk_st == 1 || this.walk_st == 2)
					{
						this.walk_st = 3;
						flag = true;
					}
					this.NPos.shifty = (this.faintshift_lgt_ = X.ZSIN(this.t, 25f) * 3.6f * (float)X.MPF(this.aim == AIM.T));
					this.NPos.shiftx = 0f;
					float move_depert_tstack_x = pr.getPhysic().move_depert_tstack_x;
					float num = pr.getPhysic().move_depert_tstack_y + ((this.aim == AIM.T) ? 5.3f : (-5.77f));
					this.Phy.addTranslateStack(X.absMx((move_depert_tstack_x - this.Phy.tstacked_x) * 0.18f, 0.02f), X.absMx((num - this.Phy.tstacked_y) * 0.18f, 0.02f));
					if (this.t >= this.walk_time || flag)
					{
						bool flag2 = this.aim == AIM.B;
						string text = (flag2 ? "torture_topchew" : "torture_underchew");
						AbsorbManager specificPublisher = this.Absorb.Con.getSpecificPublisher(this.FD_getOtherTentacleAbsorbing);
						if (specificPublisher != null)
						{
							M2Attackable publishMover = specificPublisher.getPublishMover();
							if (publishMover.aim != this.aim)
							{
								flag2 = flag2 || publishMover.aim == AIM.B;
								text = "torture_doublechew";
							}
						}
						pr.SpSetPose(text, -1, null, false);
						if (this.t >= this.walk_time)
						{
							if (this.Absorb.Con.use_torture)
							{
								this.walk_st = -1;
								this.t = 0f;
								return true;
							}
							this.EA.timescale = 1f * X.NIXP(1f, 1.8f);
							bool flag3 = this.t <= 85f || X.XORSP() < (this.Nai.isPrAlive() ? 0.3f : 0.4f);
							float num2 = pr.y + (float)CAim._YD(this.aim, 1) * 0.3f;
							pr.PtcVar("cy", (double)num2).PtcVar("rot_agR", (double)(X.NIXP(-0.09f, 0.09f) * 3.1415927f)).PtcVar("count_ratio", 0.4000000059604645)
								.PtcST("absorb_dmg_bite", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							this.Mp.DropCon.setBlood(pr, flag3 ? 9 : 6, MTR.col_blood, 0f, true);
							if (X.XORSP() < 0.5f)
							{
								this.Mp.DropCon.setLoveJuice(pr, flag3 ? 5 : 15, uint.MaxValue, 0f, false);
							}
							this.walk_time += (float)(16 + X.xors(25));
							base.applyAbsorbDamageTo(pr, this.AtkNormalAbsorb, flag3, flag2, false, 0f, false, null, false, true);
							if (X.XORSP() < 0.3f)
							{
								pr.getAnimator().randomizeFrame();
							}
							if (X.XORSP() < 0.04f)
							{
								pr.setAim((pr.aim == AIM.R) ? AIM.L : AIM.R, false);
							}
							if (X.XORSP() < 0.8f)
							{
								pr.PtcVar("cy", (double)num2).PtcST("player_absorbed_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							}
							if (X.XORSP() < 0.38f)
							{
								pr.PtcVar("cy", (double)num2).PtcST("player_absorbed_fatal", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							}
							if (this.t >= 1200f && X.XORSP() < 0.01f)
							{
								return false;
							}
						}
					}
				}
			}
			if (this.walk_st >= 1000)
			{
				if (this.walk_st == 1000)
				{
					this.t = 0f;
					this.walk_st = 1001;
					this.walk_time = 0f;
					AbsorbManager specificPublisher2 = this.Absorb.Con.getSpecificPublisher(NelNNusiTentacle.isTentacleAbsorb);
					if (specificPublisher2 != null && specificPublisher2.getPublishMover() == this)
					{
						this.walk_st = 1002;
						this.Absorb.Con.countTortureItem(NelNNusiTentacle.isNotTentacleAbsorb, true);
						this.Absorb.target_pose = "ceiltrap3";
						pr.SpSetPose(this.Absorb.target_pose, -1, null, false);
						this.Boss.tentacleFaintAbsorbProgress();
					}
					else if (specificPublisher2 != null)
					{
						(specificPublisher2.getPublishMover() as NelNNusiTentacle).walk_time = 0f;
					}
					this.EA.showToFront(this.v_id % 2 == 1, false);
					this.EA.timescale = 1f * X.NIXP(2f, 3.3f);
					pr.PtcST("nusi_fainttentacle_absorb_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				float num3 = X.MULWALKMX(this.faintshift_lgt, 11.599999f, 0.1f, 0.2f);
				if (num3 != this.faintshift_lgt)
				{
					this.faintshift_lgt = num3;
				}
				float num4 = 0.04f;
				if (this.walk_st == 1002)
				{
					if (this.walk_time == 0f)
					{
						this.walk_time = 1f;
						int num5 = this.Absorb.Con.countTortureItem(NelNNusiTentacle.isTentacleAbsorb, false);
						pr.UP.applyDamage(MGATTR.STAB, 20f, 33f, UIPictureBase.EMSTATE.NORMAL, false, (num5 >= 2) ? "torture_tentacle_2" : null, false);
					}
					float num6 = this.nsft_x + X.Cos(this.angleR) * 13.4f + (0.48f * X.COSI(this.t, 74f) + 0.47f * X.COSI(this.t, 156f)) * 1.6f;
					float num7 = this.underpos - 4.5f - (1f + 0.58f * X.COSI(this.t, 68f) + 0.42f * X.COSI(this.t, 111f)) * 0.5f * 3.4f;
					physic.addFoc(FOCTYPE.ABSORB, (num6 - pr.x) * 0.03f, (num7 - pr.y) * 0.03f, -1f, -1, 1, 0, -1, 0);
					physic.addLockGravityFrame(10);
					physic.addLockWallHitting(this, 2f);
					float num8 = physic.move_depert_tstack_y + X.Sin(this.angleR) * 13.4f;
					this.Phy.addFoc(FOCTYPE.WALK, 0f, (num8 - this.nsft_y) * 0.22f, -1f, -1, 1, 0, -1, 0);
					pr.GSaver.penetrateHpDamageReduce();
				}
				else
				{
					float num9 = physic.move_depert_tstack_x - X.Cos(this.angleR) * 13.4f;
					float num10 = physic.move_depert_tstack_y + X.Sin(this.angleR) * 13.4f;
					this.Phy.addFoc(FOCTYPE.WALK, (num9 - this.nsft_x) * 0.14f, (num10 - this.nsft_y) * 0.14f, -1f, -1, 1, 0, -1, 0);
					num4 = 0.002f;
				}
				this.DmgCounterSPos.Set(X.NI(base.x, pr.x, 0.75f), X.NI(base.y, pr.y, 0.75f), 50f);
				this.angleR = base.VALWALKANGLER(this.angleR, this.Mp.GAR(this.nsft_x, this.nsft_y, this.Nai.target_x, this.Nai.target_y), 3.1415927f * num4);
				if (this.Boss.isFaintTargetEscaped())
				{
					this.walk_st = -1001;
					this.t = 0f;
					return true;
				}
			}
			return true;
		}

		public bool isNormalAbsorbing()
		{
			return this.state == NelEnemy.STATE.ABSORB && X.BTW(0f, (float)this.walk_st, 1000f);
		}

		private bool getOtherTentacleAbsorbing(AbsorbManager Abm)
		{
			M2Attackable publishMover = Abm.getPublishMover();
			return publishMover is NelNNusiTentacle && publishMover != this && (publishMover as NelNNusiTentacle).isNormalAbsorbing();
		}

		public override IFootable isCarryable(M2FootManager FootD)
		{
			PR pr = FootD.Mv as PR;
			if (!(pr != null) || !base.carryable_other_object || this.state != NelEnemy.STATE.ABSORB || !base.isCoveringMv(pr, 2.5f, 20f))
			{
				return null;
			}
			if (this.Absorb == null || !(this.Absorb.getTargetMover() == pr) || !X.BTW(0f, (float)this.walk_st, 1000f))
			{
				return null;
			}
			return this;
		}

		public override float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy)
		{
			if (FootD.Mv as PR != null && base.carryable_other_object && this.state == NelEnemy.STATE.ABSORB && X.BTW(0f, (float)this.walk_st, 1000f))
			{
				dx = 0f;
				dy = ((this.aim == AIM.T) ? (this.underpos - 7.7f) : (this.underpos - 9f)) - FootD.Phy.move_depert_tstack_y;
				return 0.26f;
			}
			return base.fixToFootPos(FootD, x, y, out dx, out dy);
		}

		private bool runBackstep_FaintTentacleEnd(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 70f - X.XORSP() * 24f;
				this.walk_st = 1100;
				this.Nai.RemF(NAI.FLAG.ABSORB_FINISHED);
				base.throw_ray = false;
				this.Phy.addLockMoverHitting(HITLOCK.ABSORB, 1000f);
				this.EA.timescale = 1f * X.NIXP(0.23f, 0.7f);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, (this.LinkTentacle != null) ? 30 : 100, true))
			{
				this.EAPose((this.LinkTentacle != null) ? NelNBoss_Nusi.TENPOSE.atk : NelNBoss_Nusi.TENPOSE.faint_attack);
				this.EA.timescale = -1f;
				TrackEntry track = this.EA.getBaseAnimator().getTrack(0);
				track.TrackTime = track.AnimationEnd;
				this.playSndPos("nusi_tentacle_act", 1);
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.faintshift_lgt = base.VALWALK(this.faintshift_lgt, 0f, 0.25f);
				float num = X.NI(0.017f, 0.25f, X.ZPOW(this.t, 40f));
				this.Phy.addFoc(FOCTYPE.WALK, -num * X.Cos(this.angleR), num * X.Sin(this.angleR), -1f, -1, 1, 0, -1, 0);
				if (this.t >= 120f)
				{
					this.visible = false;
					return false;
				}
			}
			return true;
		}

		public float nsft_x
		{
			get
			{
				return base.x + this.NPos.shiftx;
			}
		}

		public float nsft_y
		{
			get
			{
				return base.y + this.NPos.shifty;
			}
		}

		public void initBossDeathPhase()
		{
			this.Nai.clearTicket(-1, true);
			this.set_to_home_position = false;
			base.throw_ray = true;
			this.visible = false;
		}

		public void EAPose(NelNBoss_Nusi.TENPOSE p)
		{
			this.tenpose = p;
			NelNBoss_Nusi.TentacleLink.EAPoseS(this.EA, this.v_id % 5, p, -1);
			this.EA.timescale = 1f;
		}

		public void triggerAnimComplete(TrackEntry trackEntry)
		{
			if (base.destructed)
			{
				return;
			}
			if (NelNBoss_Nusi.TentacleLink.animComplete(this.EA, this.v_id % 5, ref this.tenpose))
			{
				this.EA.timescale = 1f;
				return;
			}
			if (this.tenpose == NelNBoss_Nusi.TENPOSE.faint_attack && this.isAbsorbInFaint())
			{
				this.EAPose(NelNBoss_Nusi.TENPOSE.faint_grab);
				this.EA.timescale = 1f * X.NIXP(1.2f, 2f);
			}
		}

		public bool visible
		{
			get
			{
				if (this.v_connectable)
				{
					return this.EA.alpha != 0f;
				}
				return !base.disappearing;
			}
			set
			{
				if (this.v_connectable)
				{
					this.EA.alpha = (float)(value ? 1 : 0);
				}
				else
				{
					base.disappearing = !value;
				}
				if (value)
				{
					this.Lig.mapx = base.x;
					this.Lig.mapy = base.y;
				}
			}
		}

		private float faintshift_lgt
		{
			get
			{
				return this.faintshift_lgt_;
			}
			set
			{
				if (this.faintshift_lgt_ == value)
				{
					return;
				}
				this.faintshift_lgt_ = value;
				this.fineFaintShiftLgt(false);
			}
		}

		private void fineFaintShiftLgt(bool do_not_shift_my_pos = false)
		{
			float num = -this.faintshift_lgt_ * X.Cos(this.angleR);
			float num2 = this.faintshift_lgt_ * X.Sin(this.angleR);
			if (!do_not_shift_my_pos)
			{
				this.Phy.addTranslateStack(this.NPos.shiftx - num, this.NPos.shifty - num2);
			}
			this.NPos.shiftx = num;
			this.NPos.shifty = num2;
		}

		public bool v_connectable
		{
			get
			{
				return this.v_id < 3;
			}
		}

		private bool getFaintBurstRatio(NelAttackInfo Atk, ref float out_t)
		{
			if (this.Boss.isBurstForFaint(Atk))
			{
				out_t = 10f;
				return true;
			}
			return false;
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			float num = 0.25f;
			this.getFaintBurstRatio(Atk as NelAttackInfo, ref num);
			return base.applyHpDamageRatio(Atk) * num;
		}

		public override float applyMpDamageRatio(AttackInfo Atk)
		{
			return base.applyMpDamageRatio(Atk) * 0.17f;
		}

		public override void checkTiredTime(ref int t0, NelAttackInfo Atk)
		{
			if (this.Boss.isBurstForFaint(Atk))
			{
				t0 = X.Mn(t0, 15);
			}
			base.checkTiredTime(ref t0, Atk);
		}

		public override Vector3 getDamageCounterShiftMapPos()
		{
			if (this.DmgCounterSPos.z > 0f)
			{
				return new Vector3(this.DmgCounterSPos.x - base.x, this.DmgCounterSPos.y - base.y, 1f);
			}
			return base.getDamageCounterShiftMapPos();
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			int num = base.applyDamage(Atk, force);
			float num2 = 0f;
			if (this.getFaintBurstRatio(Atk, ref num2))
			{
				this.Boss.initBurstStunPhase(Atk.Caster as PR);
			}
			return num;
		}

		private void dangerousStar(float len, float cy, float radius, float maxt)
		{
			int num = X.IntR(len / 2.6f);
			float num2 = len / (float)num;
			num2 *= this.Boss.mpf_is_right;
			float num3 = this.atk_cx;
			float tstacked_y = this.Phy.tstacked_y;
			for (int i = 0; i <= num; i++)
			{
				base.PtcVar("cx", (double)num3).PtcVar("cy", (double)(tstacked_y + cy)).PtcVar("maxt", (double)maxt)
					.PtcVar("radius", (double)(radius + 0.5f))
					.PtcST("nusit_dangerous_star", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				num3 += num2;
			}
		}

		private void dangerousStarY(float cx, float len, float radius, float maxt)
		{
			int num = X.IntR(len / 2.6f);
			float num2 = len / (float)num;
			num2 *= (float)(-(float)CAim._YD(this.aim, 1));
			float tstacked_x = this.Phy.tstacked_x;
			float num3 = this.Phy.tstacked_y;
			for (int i = 0; i <= num; i++)
			{
				base.PtcVar("cx", (double)(tstacked_x + cx)).PtcVar("cy", (double)num3).PtcVar("maxt", (double)maxt)
					.PtcVar("radius", (double)(radius + 0.5f))
					.PtcST("nusit_dangerous_star", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				num3 += num2;
			}
		}

		public float angleR
		{
			get
			{
				return this.EA.rotationR - 3.1415927f;
			}
			set
			{
				this.EA.rotationR = value + 3.1415927f;
				if (this.faintshift_lgt_ > 0f)
				{
					this.fineFaintShiftLgt(false);
				}
			}
		}

		public override bool isRingOut()
		{
			return false;
		}

		public NelNBoss_Nusi.MA_CAGE anmtype
		{
			get
			{
				return this.anmtype_;
			}
			set
			{
				if (this.anmtype == value)
				{
					return;
				}
				this.anmtype_ = value;
			}
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle && !this.Ser.has(SER.TIRED);
		}

		public bool isAbsorbInFaint()
		{
			return base.isAbsorbState() && X.BTW(1000f, (float)this.walk_st, 1100f);
		}

		public override bool export_other_mover_right(M2Mover Other)
		{
			if (!this.export_aim_fix)
			{
				return base.export_other_mover_right(Other);
			}
			return this.Boss.mpf_is_right > 0f;
		}

		public float atk_cx
		{
			get
			{
				return this.Phy.tstacked_x - this.Boss.mpf_is_right * this.sizex;
			}
		}

		public float atk_xdif
		{
			get
			{
				if (this.Boss.mpf_is_right < 0f)
				{
					return base.mright - this.Nai.target_x;
				}
				return this.Nai.target_x - base.mleft;
			}
		}

		public float BRANtk(int rseed, float boss_ratio = 0.6f)
		{
			return this.Boss.getAI().RANtk(rseed + 33 + 59 * this.v_id) * boss_ratio + this.Nai.RANtk(rseed + 41 * this.v_id) * (1f - boss_ratio);
		}

		public override bool showFlashEatenEffect(bool for_effect = false)
		{
			return !for_effect;
		}

		private NelNBoss_Nusi.MA_CAGE anmtype_;

		public EnemyAnimatorSpine EA;

		public NelNBoss_Nusi.TentacleLink LinkTentacle;

		public CCNestItem NPos;

		public NelNBoss_Nusi Boss;

		private NelNBoss_Nusi.AIFreeze Freeze;

		private int v_id;

		private const NAI.TYPE NT_UNDERPUNCH = NAI.TYPE.PUNCH;

		private const NAI.TYPE NT_MIDDLEPUNCH = NAI.TYPE.PUNCH_0;

		private MagicItem MgFaintAtk;

		private int faintatk_id;

		private byte unlink_lock_on_quit_ticket;

		private NOD.TackleInfo TkiUPunch = NOD.getTackle("nusit_upunch");

		private NOD.TackleInfo TkiMPunch = NOD.getTackle("nusit_mpunch");

		private const int T_UPUNCH_PREP0 = 120;

		private const int T_UPUNCH_PREP1 = 4;

		private const int T_UPUNCH_TRIGGER = 140;

		private const int T_UPUNCH_AFTER_DELAY = 240;

		private EnAttackInfo AtkUPunch = new EnAttackInfo
		{
			hpdmg0 = 33,
			burst_vx = 0.3f,
			burst_vy = -0.08f,
			shield_break_ratio = 4f,
			split_mpdmg = 12,
			huttobi_ratio = 100f,
			parryable = true,
			Beto = BetoInfo.BigBite
		};

		private const NAI.TYPE NT_STRUGGLE = NAI.TYPE.PUNCH_1;

		private NOD.TackleInfo TkiStruggle = NOD.getTackle("nusit_struggle");

		private EnAttackInfo AtkStruggle = new EnAttackInfo
		{
			hpdmg0 = 20,
			burst_vx = 0.5f,
			shield_break_ratio = 1.2f,
			split_mpdmg = 12,
			huttobi_ratio = 0.3f,
			parryable = true,
			SerDmg = new FlagCounter<SER>(4).Add(SER.CONFUSE, 20f),
			Beto = BetoInfo.BigBite
		};

		private const int T_STRG_PREP0 = 76;

		private const int T_STRG_TRIGGER = 170;

		private const int T_STRG_CNT_MAX = 3;

		private const int T_STRG_AFTER_DELAY = 140;

		private const float T_STRG_TIMESCALE = 1.6f;

		private const float TS_EA = 1f;

		private const int T_STRG_TYPELOCK = 220;

		public const NAI.TYPE NT_GRAB = NAI.TYPE.PUNCH_2;

		private NOD.TackleInfo TkiGrab = NOD.getTackle("nusit_grab");

		private EnAttackInfo AtkGrab = new EnAttackInfo
		{
			hpdmg0 = 10,
			mpdmg0 = 2,
			split_mpdmg = 9,
			burst_vx = 0.13f,
			absorb_replace_prob_both = 100f,
			shield_break_ratio = 2f,
			parryable = true,
			Beto = BetoInfo.NormalS
		};

		private EnAttackInfo AtkNormalAbsorb = new EnAttackInfo(0.04f, 0.1f)
		{
			hpdmg0 = 14,
			split_mpdmg = 10,
			attr = MGATTR.BITE,
			Beto = BetoInfo.Blood
		};

		private const int T_GRAB_PREP0 = 55;

		private const int T_GRAB_PREP1 = 55;

		private const int T_GRAB_PREP2 = 90;

		private const int T_GRAB_TRIGGER = 18;

		private const int T_GRAB_S2TRIGGER = 218;

		private const int T_GRAB_END = 80;

		private const int T_GRAB_AFTER = 90;

		private const int T_GRAB_AFTERDELAY = 100;

		private const int T_ABSORB_FIRST_DMG = 80;

		public const NAI.TYPE NT_BIGRUN = NAI.TYPE.WARP;

		private float faintshift_lgt_;

		private const float tentacle_atk_lgt = 50f;

		private const float tentacle_grab_lgt = 13.4f;

		private const float faint_side_agR = 0.43982297f;

		private float underpos;

		private bool set_to_home_position = true;

		private bool export_aim_fix = true;

		private const float sizex_faintattack_pre = 48f;

		private const float sizey_faintattack_pre = 48f;

		private const float sizex_faintattack = 266f;

		private const float sizey_faintattack = 266f;

		private Vector3 DmgCounterSPos;

		private NelAttackInfo AtkCaptureFaint = new NelAttackInfo
		{
			hpdmg0 = 0,
			mpdmg0 = 1,
			absorb_replace_prob_both = 1f,
			huttobi_ratio = -100f,
			shield_break_ratio = 0f,
			attr = MGATTR.GRAB,
			parryable = false,
			ndmg = NDMG.GRAB_PENETRATE
		};

		private NelNBoss_Nusi.TENPOSE tenpose;

		private const NAI.TYPE NT_STUNESCAPE = NAI.TYPE.GUARD;

		private const int PRI_FORCE = 148;

		public const int PRI_MAIN = 129;

		public const int PRI_WAIT = 128;

		private const int ABSORB_PRI = 94;

		private Func<AbsorbManager, bool> FD_getOtherTentacleAbsorbing;

		private static Func<AbsorbManager, bool> isTentacleAbsorb = (AbsorbManager V) => V.getPublishMover() is NelNNusiTentacle && (V.getPublishMover() as NelNNusiTentacle).isAbsorbInFaint();

		private static Func<AbsorbManager, bool> isNotTentacleAbsorb = (AbsorbManager V) => !(V.getPublishMover() is NelNNusiTentacle) || !(V.getPublishMover() as NelNNusiTentacle).isAbsorbInFaint();
	}
}
