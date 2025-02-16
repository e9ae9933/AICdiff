using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class NelNNusiCage : NelEnemyNested, EnemyAnimator.IEnemyAnimListener
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			ENEMYID id = this.id;
			this.id = ENEMYID.BOSS_NUSI_CAGE;
			NOD.BasicData basicData = NOD.getBasicData("BOSS_NUSI_CAGE");
			base.base_gravity = 0f;
			this.hp0_remove = false;
			base.appear(_Mp, basicData);
			this.do_not_shuffle_on_cheat = true;
			this.ringoutable = false;
			this.no_apply_map_damage = true;
			this.Anm.setPose("cage_close", -1);
			this.Anm.showToFront(true, false);
			this.Anm.addAdditionalListener(this);
			this.Anm.checkframe_on_drawing = false;
			this.SqColumn = this.Anm.getCurrentCharacter().getPoseByName("cage_column").getSequence(0);
			this.auto_fix_notfound_pose_to_stand = false;
			this.FootD.auto_fix_to_foot = false;
			this.AnmB = new NelNNusiCage.CageBehindDrawer(this);
			this.AnmB.showToFront(false, false);
			this.AnmB.draw_margin = 40f;
			this.Anm.draw_margin = 5f;
			this.Nai.awake_length = 60f;
			this.cannot_move = true;
			this.Nai.consider_only_onfoot = false;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnly;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.set_enlarge_bouncy_effect = false;
			base.addF((NelEnemy.FLAG)2097280);
		}

		public override NelEnemyNested initNest(NelEnemy _Parent, int array_create_capacity = 4)
		{
			base.initNest(_Parent, array_create_capacity);
			this.Boss = this.Parent as NelNBoss_Nusi;
			this.AnmB.copyColor(this.Parent.getAnimator());
			return this;
		}

		public override void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			base.quitSummonAndAppear(clearlock_on_summon);
			this.Phy.addLockWallHitting(this, -1f);
			this.Phy.addLockMoverHitting(HITLOCK.EVENT, -1f);
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.Anm != null)
			{
				this.Anm.remAdditionalListener(this);
			}
			if (this.AnmB != null)
			{
				this.AnmB.destruct();
				this.AnmB = null;
			}
			if (this.EvIxiaAnm != null)
			{
				this.EvIxiaAnm.color = C32.d2c(0U);
			}
			if (this.RealIxia != null)
			{
				this.RealIxia.cageKilled();
			}
			base.destruct();
		}

		public void fineCagePosition(bool reposit_setto = false)
		{
			this.need_fine_position = false;
			this.Anm.need_fine_mesh = true;
			float num = 0f;
			float num2 = 0f;
			if (this.Anm.rotationR != 0f)
			{
				num2 += 7.8f;
				Vector2 vector = X.ROTV2e(new Vector2(num, -num2), this.Anm.rotationR);
				num = vector.x * 1f;
				num2 = -vector.y - 7.8f;
			}
			num += this.Parent.x + this.Parent.mpf_is_right * 4.5f;
			num2 += this.Parent.y + -0.5f - 7f * (1f - this.ivy_level);
			if (reposit_setto)
			{
				this.setTo(num, num2);
			}
			else
			{
				this.Phy.addTranslateStack(num - base.x, num2 - base.y);
			}
			this.setAim(this.Parent.aim, false);
			if (this.MvIxia != null && (this.RealIxia == null || !this.RealIxia.isAbsorbing()))
			{
				float num3 = this.ixia_pos_shift_mapy;
				float num4 = num + num3 * X.Cos(this.angleR);
				float num5 = num2 - num3 * X.Sin(this.angleR);
				this.MvIxia.moveBy(X.absMn(num4 - this.MvIxia.x, this.ixia_follow_spd), X.absMn(num5 - this.MvIxia.y, this.ixia_follow_spd), true);
			}
		}

		public override void runPre()
		{
			base.runPre();
			if (!base.destructed)
			{
				NelEnemy.STATE state = this.state;
			}
		}

		public override void runPhysics(float fcnt)
		{
			if ((this.need_fine_position || this.Anm.rotationR_speed != 0f) && this.Phy.main_updated_count >= 1)
			{
				this.fineCagePosition(false);
			}
			base.runPhysics(fcnt);
		}

		public void assignIxia(M2Mover Target, IxiaPVV104 _RealIxia = null)
		{
			if (this.MvIxia != null)
			{
				return;
			}
			this.MvIxia = Target;
			this.RealIxia = _RealIxia;
			this.need_fine_position = true;
		}

		private bool considerNormal(NAI Nai)
		{
			if (base.hasF(NelEnemy.FLAG.EVENT_SHOW))
			{
				return true;
			}
			if (this.anmtype_ == NelNBoss_Nusi.MA_CAGE.HIDDEN)
			{
				return Nai.AddTicketB(NAI.TYPE.BACKSTEP, 128, true);
			}
			return Nai.AddTicketB(NAI.TYPE.WALK, 128, true);
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type == NAI.TYPE.WALK)
			{
				return this.runWalk(Tk.initProgress(this), Tk);
			}
			if (type != NAI.TYPE.BACKSTEP)
			{
				return type == NAI.TYPE.GAZE && this.runGaze(Tk.initProgress(this), Tk);
			}
			return this.runBackstep(Tk.initProgress(this), Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null && Tk.type == NAI.TYPE.WALK && this.anmtype != NelNBoss_Nusi.MA_CAGE.APPEAL && this.RealIxia != null)
			{
				this.RealIxia.check_torture = false;
			}
			this.ixia_follow_spd = 20f;
			this.ixia_pos_shift_mapy = 0.24f;
			this.can_hold_tackle = false;
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			base.quitTicket(Tk);
		}

		private bool runWalk(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 110;
				this.walk_time = this.ivy_level;
				this.Anm.rotationR_speed = 0.07539823f * X.XORSPS();
				this.playSndPos("collect_leaf", 1);
				base.disappearing = false;
				base.throw_ray = !this.Boss.is_alive;
				this.Anm.showToFront(true, false);
				this.need_fine_position = true;
				if (this.MvIxia == null)
				{
					M2EventItem m2EventItem = this.Mp.getEventContainer().Get("ixiadoll", true, true);
					if (m2EventItem != null)
					{
						this.MvIxia = m2EventItem;
						this.EvIxiaAnm = m2EventItem.Anm;
					}
				}
				if (this.RealIxia != null)
				{
					this.RealIxia.disappearing = false;
				}
				if (this.EvIxiaAnm != null)
				{
					this.EvIxiaAnm.color = C32.d2c(uint.MaxValue);
				}
				this.ixia_follow_spd = 20f;
				this.ixia_pos_shift_mapy = 0.24f;
				this.fineCagePosition(true);
			}
			if (this.anmtype_ != NelNBoss_Nusi.MA_CAGE.APPEAL && this.anmtype_ != NelNBoss_Nusi.MA_CAGE.APPEAL_OPEN)
			{
				return false;
			}
			this.Anm.rotationR_speed += (float)X.MPF(this.Anm.rotationR < 0f) * 3.1415927f * 0.0019f * ((this.Anm.rotationR < 0f == this.Anm.rotationR_speed > 0f) ? 0.66f : 1f);
			if (Tk.prog == PROG.ACTIVE)
			{
				float num = this.t - 30f;
				if (num < 0f)
				{
					this.ivy_level = X.NI(this.walk_time, 1f, X.ZPOW(this.t, 30f));
				}
				else
				{
					if (this.walk_st >= 0 && this.t >= (float)this.walk_st)
					{
						this.walk_st = -1;
						if (this.anmtype_ == NelNBoss_Nusi.MA_CAGE.APPEAL)
						{
							this.SpSetPose("cage_close_prepare", -1, null, false);
						}
					}
					if (num < 64f)
					{
						this.ivy_level = 1f - 0.3f * (X.ZPOWV(num, 32f) - X.ZPOW(num - 32f, 32f));
					}
					else
					{
						float num2 = num - 64f;
						this.ivy_level = 1f - 0.17f * (X.ZPOWV(num2, 28f) - X.ZPOW(num2 - 28f, 28f));
						if (this.walk_st == -1 && this.ivy_level >= 0.999f && ((this.anmtype_ == NelNBoss_Nusi.MA_CAGE.APPEAL) ? this.Anm.isAnimEnd() : (num2 >= 40f)))
						{
							this.t = 0f;
							this.ivy_level = 1f;
							Tk.prog = PROG.PROG0;
							this.setAim(this.Parent.aim, false);
							this.Anm.rotationR_speed = 0f;
							this.Anm.rotationR = 0f;
							if (this.anmtype_ != NelNBoss_Nusi.MA_CAGE.APPEAL)
							{
								Tk.Recreate(NAI.TYPE.GAZE, -1, true, null);
								return this.runGaze(Tk.initProgress(this), Tk);
							}
							if (this.RealIxia != null)
							{
								this.RealIxia.changeStateToAbsorb();
								this.can_hold_tackle = true;
							}
							base.PtcST("nusi_inject_ixiacharge", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
							this.initTortureAbsorbPoseSet(null, "torture_nusi_atk", 0, 1);
						}
					}
				}
			}
			if ((Tk.prog == PROG.PROG0 || Tk.prog == PROG.PROG1) && (this.Parent as NelNBoss_Nusi).burst_counter_success > 0 && Tk.Progress(ref this.t, 600, true))
			{
				this.can_hold_tackle = false;
				this.PtcHld.killPtc("nusi_inject_ixiacharge", false);
				this.anmtype = NelNBoss_Nusi.MA_CAGE.HIDDEN;
				this.SpSetPose("cage_act_end", -1, null, false);
				return false;
			}
			return true;
		}

		public override bool isTortureUsingForAnim()
		{
			return this.can_hold_tackle;
		}

		public void initIxiaHold()
		{
			if (!this.Nai.isFrontType(NAI.TYPE.WALK, PROG.ACTIVE))
			{
				this.anmtype = NelNBoss_Nusi.MA_CAGE.APPEAL;
				this.Nai.AddTicket(NAI.TYPE.WALK, 128, true);
			}
		}

		private bool runBackstep(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_time = this.ivy_level;
				if (this.ivy_level == 0f)
				{
					Tk.prog = PROG.PROG4;
				}
				this.playSndPos("nusi_leaf_laugh", 1);
				this.ixia_follow_spd = 20f;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				this.ixia_pos_shift_mapy = base.VALWALK(this.ixia_pos_shift_mapy, 0.24f, 0.025f);
				this.Anm.rotationR_speed += (float)X.MPF(this.Anm.rotationR < 0f) * 3.1415927f * 0.0019f * ((this.Anm.rotationR < 0f == this.Anm.rotationR_speed > 0f) ? 0.66f : 1f);
				this.ivy_level = X.NI(this.walk_time, 0f, X.ZPOW3(this.t, 45f));
				if (this.t >= 45f)
				{
					Tk.prog = PROG.PROG4;
				}
			}
			if (Tk.prog == PROG.PROG4)
			{
				Tk.prog = PROG.PROG5;
				this.ivy_level = 0f;
				this.Anm.rotationR_speed = 0f;
				this.angleR = -1.5707964f;
				base.disappearing = true;
				if (this.RealIxia != null)
				{
					this.RealIxia.disappearing = true;
				}
				if (this.EvIxiaAnm != null)
				{
					this.EvIxiaAnm.color = C32.d2c(16777215U);
				}
			}
			return this.anmtype_ == NelNBoss_Nusi.MA_CAGE.HIDDEN;
		}

		private bool runGaze(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				if (this.ivy_level < 0.8f)
				{
					return false;
				}
				this.ivy_level = 1f;
				this.Anm.rotationR_speed = 0.065973446f * X.XORSPS();
				base.PtcST("nusi_cage_open", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.playSndPos("nusi_leaf_small0", 1);
				this.SpSetPose("cage_open", -1, null, false);
				this.ixia_follow_spd = 0.1f;
				base.throw_ray = true;
			}
			this.Anm.rotationR_speed += (float)X.MPF(this.Anm.rotationR < 0f) * 3.1415927f * 0.0019f * ((this.Anm.rotationR < 0f == this.Anm.rotationR_speed > 0f) ? 0.66f : 1f);
			this.ixia_pos_shift_mapy = base.VALWALK(this.ixia_pos_shift_mapy, 0.4f, 0.01f);
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 5, this.anmtype_ != NelNBoss_Nusi.MA_CAGE.APPEAL_OPEN))
			{
				this.playSndPos("nusi_leaf_closing", 1);
				this.SpSetPose("cage_open2close", -1, null, false);
				this.ixia_follow_spd = 0.06f;
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.ixia_pos_shift_mapy = base.VALWALK(this.ixia_pos_shift_mapy, 0.24f, 0.025f);
				if (this.Anm.isAnimEnd())
				{
					base.throw_ray = false;
					Tk.Recreate(NAI.TYPE.BACKSTEP, -1, true, null);
					return this.runBackstep(Tk.initProgress(this), Tk);
				}
			}
			return true;
		}

		public bool cage_opened
		{
			get
			{
				return this.Nai == null || (this.Nai.isFrontType(NAI.TYPE.GAZE, PROG.ACTIVE) && this.Nai.getCurTicket().prog == PROG.ACTIVE);
			}
		}

		public bool cage_return_back
		{
			get
			{
				return this.Nai.isFrontType(NAI.TYPE.BACKSTEP, PROG.ACTIVE);
			}
		}

		public override void checkTiredTime(ref int t0, NelAttackInfo Atk)
		{
			t0 = X.Mn(t0, 4);
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			return base.applyDamage(Atk, force);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return base.applyHpDamageRatio(Atk) * 0.25f;
		}

		public override float applyMpDamageRatio(AttackInfo Atk)
		{
			return base.applyMpDamageRatio(Atk) * 0.35f;
		}

		public override float mp_split_reduce_ratio
		{
			get
			{
				if (!this.Boss.isStunBigDamage())
				{
					return base.mp_split_reduce_ratio;
				}
				return 0f;
			}
		}

		public void fineFrameData(EnemyAnimator Anm, EnemyFrameDataBasic FrmData, bool created)
		{
			this.AnmB.need_fine_mesh = true;
			this.AnmB.layer_mask = 1U;
			Anm.layer_mask &= 4294967294U;
		}

		public int createEyes(EnemyAnimator Anm, Matrix4x4 MxAfterMultiple, ref int eyepos_search)
		{
			return 0;
		}

		public void checkFrame(EnemyAnimator Anm, float TS)
		{
		}

		public void DrawBehind(MeshDrawer Md)
		{
			if (this.ivy_level <= 0f || base.destructed)
			{
				return;
			}
			Md.setCurrentMatrix(this.AnmB.getAfterMultipleMatrix(true), true);
			float num = 7f * this.ivy_level;
			int num2 = X.IntC(num / 1.2f);
			float num3 = -70f * X.Cos(this.angleR);
			float num4 = -70f * X.Sin(this.angleR);
			float num5 = (this.Parent.x + this.Parent.mpf_is_right * 4.5f - this.drawx * this.Mp.rCLEN) * this.Mp.CLENB;
			float num6 = -(this.Parent.y + -0.5f - 7f - this.drawy * this.Mp.rCLEN) * this.Mp.CLENB;
			float num7 = num * 0.48f * this.Mp.CLENB;
			float num8 = num3 - num7 * X.Cos(this.angleR);
			float num9 = num4 - num7 * X.Sin(this.angleR);
			float num10 = X.NI(num8, num3, 0.5f);
			float num11 = X.NI(num9, num4, 0.5f);
			float num12 = num3;
			float num13 = num4;
			float num14 = 0f;
			for (int i = 0; i < num2; i++)
			{
				float num15 = (float)(num2 - 1 - i) / (float)num2;
				float num16 = X.BEZIER_I(num5, num8, num10, num3, num15);
				float num17 = X.BEZIER_I(num6, num9, num11, num4, num15);
				float num18 = X.GAR2(num12, num13, num16, num17) + ((i % 3 == 1) ? 3.1415927f : 0f);
				num14 += 1.2f;
				float num19 = 1.2f;
				if (num14 > num)
				{
					num19 *= (num14 - num) / 1.2f;
				}
				Md.RotaPF(X.NI(num12, num16, 0.5f), X.NI(num13, num17, 0.5f), num19, 1.2f, num18, this.SqColumn.getFrame(i % this.SqColumn.countFrames()), false, false, false, uint.MaxValue, false, 0);
				num12 = num16;
				num13 = num17;
			}
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
		}

		public float ivy_level
		{
			get
			{
				return this.ivy_level_;
			}
			set
			{
				if (this.ivy_level == value)
				{
					return;
				}
				this.ivy_level_ = value;
				this.need_fine_position = true;
			}
		}

		public float angleR
		{
			get
			{
				return this.Anm.rotationR - 1.5707964f;
			}
			set
			{
				this.Anm.rotationR = value + 1.5707964f;
				this.need_fine_position = true;
			}
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

		public void drawEyeInnerInit(EnemyAnimator Anm)
		{
		}

		public override bool isRingOut()
		{
			return false;
		}

		public override bool showFlashEatenEffect(bool for_effect = false)
		{
			return !for_effect;
		}

		private NelNBoss_Nusi.MA_CAGE anmtype_;

		private NelNNusiCage.CageBehindDrawer AnmB;

		private M2Mover MvIxia;

		private IxiaPVV104 RealIxia;

		private M2PxlAnimatorRT EvIxiaAnm;

		private PxlSequence SqColumn;

		private NelNBoss_Nusi Boss;

		private const float ixia_pos_def = 0.24f;

		private float ixia_pos_shift_mapy;

		public bool need_fine_position = true;

		private const float ivy_shift_x = 4.5f;

		private const float ivy_shift_y = -0.5f;

		private const float ivy_max_height = 7f;

		private float ivy_level_;

		private float ixia_follow_spd = 10f;

		private const int PRI_WALK = 128;

		private sealed class CageBehindDrawer : EnemyAnimatorBase
		{
			public CageBehindDrawer(NelNNusiCage _EnCage)
				: base(_EnCage)
			{
				this.EnCage = _EnCage;
				this.Anm = this.EnCage.getAnimator();
				EnemyAttr.initAnimator(this.EnCage, this);
				base.prepareMesh(this.Anm.getMI(), null);
				base.prepareRendetTicket(this.Mv, null, null);
			}

			public override Matrix4x4 getAfterMultipleMatrix(float scalex, float scaley, bool ignore_rot = false)
			{
				return this.Anm.getAfterMultipleMatrix(scalex, scaley, ignore_rot);
			}

			protected override void redrawBodyMeshInner()
			{
				base.redrawBodyMeshInner(this.Anm.getCurrentDrawnFrame(), this.Anm.getCurFrameData());
				this.EnCage.DrawBehind(this.Md);
			}

			private NelNNusiCage EnCage;

			private EnemyAnimator Anm;
		}
	}
}
