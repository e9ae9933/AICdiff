using System;
using evt;
using m2d;
using nel.gm;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class PR : M2MoverPr, M2MagicCaster, NelM2Attacker, IPuzzRevertable, ITortureListener, IWindApplyable, NelItem.IItemUser, EnemySummoner.ISummonActivateListener
	{
		protected override void Awake()
		{
			base.Awake();
			this.FD_drawWaterRelease = new M2DropObject.FnDropObjectDraw(this.drawWaterRelease);
		}

		public override void newGame()
		{
			base.newGame();
			this.newgame_assign = true;
			this.hp_crack = 0;
			this.NM2D = M2DBase.Instance as NelM2DBase;
			if (this.AbsorbCon == null)
			{
				this.Skill = new M2PrSkill(this);
				this.VO = new M2VoiceController(this, MTR.VcNoelSource, this.snd_key + ".voice");
				this.AbsorbCon = new AbsorbManagerContainer(5, this);
				this.Ser = new M2Ser(this, this, true);
				this.GaugeBrk = new MpGaugeBreaker(this);
				this.GSaver = new PrGaugeSaver(this);
				this.EggCon = new PrEggManager(this);
				this.EpCon = new EpManager(this);
				this.BetoMng = BetobetoManager.GetManager("noel");
			}
			if (this.Anm != null)
			{
				this.Anm.repairCane();
				this.Anm.FlgDropCane.Clear();
			}
			if (this.AbsorbCon != null)
			{
				this.AbsorbCon.gacha_renderable = false;
			}
			if (this.MistApply != null)
			{
				this.MistApply = this.MistApply.destruct();
			}
			if (this.Phy != null)
			{
				this.Phy.clearLock();
			}
			this.Onnie = null;
			if (this.UP != null)
			{
				this.UP.recheck(0, 0);
			}
			this.setVoiceOverrideAllowLevel(0f);
			base.fix_aim = false;
			this._base_TS = 1f;
			this.pee_lock = 0;
			this.outfit_default_mnp = (M2MoverPr.PR_MNP)0;
			this.outfit_default_dcl = (M2MoverPr.DECL)0;
			this.BetoMng.cleanAll(true);
			this.GSaver.newGame();
			this.Skill.newGame();
		}

		public override void appear(Map2d _Mp)
		{
			this.knockback_time = 40;
			this.overkill = true;
			this._base_TS = 1f;
			this.Mp = _Mp;
			if (this.Anm == null)
			{
				base.Size(12f, 68f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				this.PtcHld = new PtcHolder(this, 4, 28);
			}
			this.size_y_default_pixel = 68;
			base.jump_hold_lock = true;
			bool flag = this.Ser != null && this.Ser.has(SER.DEATH);
			_Mp.setTag(base.gameObject, "MoverPr");
			bool flag2 = this.hasD(M2MoverPr.DECL.DO_NOT_RESET_POS_ON_APPEAR);
			if (flag2)
			{
				this.fineTransformToPos(false, true);
			}
			else
			{
				this.setToDefaultPosition(true, null);
			}
			if (this.Phy == null)
			{
				base.createPhys(M2PhysPr.FD_createPhysPr);
			}
			base.appear(_Mp);
			bool flag3 = false;
			bool flag4 = false;
			M2PxlAnimatorRT m2PxlAnimatorRT = _Mp.M2D.createBasicPxlAnimatorForRenderTicket(this, "noel", "stand", false, M2Mover.DRAW_ORDER.PR1);
			if (this.Anm == null)
			{
				this.createAnimator(m2PxlAnimatorRT);
				this.weight0 = base.weight;
				flag4 = true;
				this.IMNG = (base.M2D as NelM2DBase).IMNG;
				this.Phy.FnHitting = new M2Phys.FnHittingVelocity(this.FnHittingVelocity);
				this.Phy.ySpeedMax = this.ySpeedMax0;
				this.Phy.walk_xspeed_manageable_air = this.walkSpeed;
				this.FD_ignoreHitToBlockCollider = new FloatCounter<Collider2D>.FnNoReduce(this.ignoreHitToBlockCollider);
			}
			else
			{
				flag3 = this.crouching > 0f;
				this.Ser.Cure(SER.WORM_TRAPPED);
				this.Anm.initS(m2PxlAnimatorRT);
				if (this.MistApply != null && !this.MistApply.isActive())
				{
					this.MistApply = this.MistApply.destruct();
				}
				if (this.Ydrg != null)
				{
					this.Ydrg.destruct();
					this.Ydrg = null;
				}
				this.AbsorbCon.releaseAll();
				if (flag)
				{
					this.hp = 0;
				}
				this.BetoMng.cleanThread();
				base.killSpeedForce(true, true, true);
			}
			EnemySummoner.addSummonerListener(this);
			this.Anm.rotationR = 0f;
			this.Ser.resetPtcSt();
			this.Ser.Cure(SER.OVERRUN_TIRED);
			this.walk_auto_assisting = 0;
			this.Phy.hit_mover_threshold = 4;
			this.Phy.quitSoftFall(0f);
			this.TeCon.RegisterScl(this.Anm);
			this.mp = X.MMX(0, this.mp, this.maxmp - this.EggCon.total);
			this.wind_apply_t = 0f;
			this.enemy_targetted = 0;
			this.TeFallShift = null;
			this.EpCon.initS();
			M2Light m2Light = new M2Light(_Mp, this);
			m2Light.follow_speed = 0.1f;
			m2Light.Col.Set(2866067885U);
			m2Light.radius = 140f;
			this.fix_pose = null;
			this.break_pose_fix_on_walk_level = 0;
			this.need_check_bounds = true;
			base.recheckForceCrouch();
			if (this.is_alive)
			{
				if (!flag2 && (this.state > PR.STATE.NORMAL || flag4 || this.newgame_assign))
				{
					this.changeState(PR.STATE.NORMAL);
				}
				if (this.newgame_assign)
				{
					this.Anm.setPose(this.Anm.pose_title, 1, false);
				}
			}
			else
			{
				this.Anm.setPose((X.XORSP() < 0.4f) ? "down" : "down_u", -1, false);
			}
			this.CC.isTrigger = false;
			this.NM2D.FlagOpenGm.Rem("MASTURBATE");
			this.check_lava = 0f;
			this.frozen_fineaf_time = 0f;
			this.lava_burn_dount = 0;
			_Mp.addLight(m2Light);
			if (!flag3)
			{
				this.crouching = 0f;
			}
			this.Mp.assignCenterPlayer(this);
			if (this.aim == AIM.B)
			{
				this.setAim(AIM.R, false);
			}
			this.absorb_additional_dying = 0f;
			this.GSaver.removeHeadDelay();
			this.do_not_destruct_when_remove = true;
			this.Skill.initS();
			this._temp_puzzle_cache_str = null;
			this.NM2D.CheckPoint.appearPlayer(this);
			if (this.UP == null)
			{
				UIPicture.changePlayer(this);
			}
			this.setVoiceOverrideAllowLevel(0f);
			this.newgame_assign = false;
		}

		public override void appearToCenterPr()
		{
			if (this.AbsorbCon != null && this.AbsorbCon.isActive())
			{
				this.AbsorbCon.need_fine_gacha_effect = true;
			}
		}

		protected abstract void createAnimator(M2PxlAnimator PAnm);

		public override void deactivateFromMap()
		{
			this.Skill.deactivateFromMap();
			this.EpCon.SituCon.flushLastExSituationTemp();
			COOK.FlgTimerStop.Rem("BENCH");
			this.Ser.releasePtcST(true);
			EnemySummoner.remSummonerListener(this);
			base.deactivateFromMap();
			if (this.isWormTrapped())
			{
				try
				{
					this.releaseFromIrisOut(false, PR.STATE.DAMAGE_L_LAND, "dmg_down2", true);
				}
				catch
				{
				}
			}
		}

		public void endS()
		{
			if (this.Anm != null)
			{
				this.Anm.endS();
			}
		}

		public override void destruct()
		{
			if (this.Ser == null)
			{
				return;
			}
			if (this.isWormTrapped())
			{
				try
				{
					this.releaseFromIrisOut(false, PR.STATE.DAMAGE_L_LAND, "dmg_down2", true);
				}
				catch
				{
				}
			}
			EnemySummoner.remSummonerListener(this);
			this.Skill.destruct();
			this.do_not_destruct_when_remove = false;
			if (this.Anm != null)
			{
				this.Anm.prepareDestruct();
			}
			this.Ser.destruct();
			this.VO.destruct();
			this.AbsorbCon.clearTextInstance().destruct();
			this.EpCon.destruct();
			base.destruct();
			this.Ser = null;
		}

		protected override void refineMoveKey()
		{
			this.manip = this.Skill.runPre();
			if (this.MistApply != null && this.MistApply.isFloatingWaterChoke())
			{
				this.manip |= M2MoverPr.PR_MNP.NO_CROUCH;
			}
			base.refineMoveKey();
			if (base.isMoveStartPushedDown())
			{
				this.Skill.moveStartPushedDown();
			}
			if (this.hasD(M2MoverPr.DECL.STOP_FOOTSND))
			{
				this.FootD.lockPlayFootStamp(4);
			}
		}

		public override void runPre()
		{
			this._base_TS = this.Ser.baseTimeScale() * ((this.Phy.isin_water && !this.isBurstState(this.state) && this.isPunchState(this.state)) ? 0.75f : 1f);
			base.runPre();
			if (this.MistApply != null)
			{
				this.MistApply = this.MistApply.run(ref this.manip, this.TS);
			}
			if (this.check_lava > 0f && this.Mp.floort > 10f && this.Mp.BCC != null && this.Mp.BCC.is_prepared)
			{
				this.check_lava -= this.TS;
				if (this.check_lava <= 0f)
				{
					this.checkLavaExecute();
				}
			}
			if (this.state == PR.STATE.NORMAL)
			{
				if (this.Phy.walk_xspeed != 0f && this.Phy.isin_water && !base.checkCornerConfig(0.001f, -0.2f, new Func<int, bool>(CCON.isWater)))
				{
					this.Phy.releaseWaterDunk();
					if (this.NM2D.MIST.isActive())
					{
						this.NM2D.MIST.releaseWaterDunk(this);
					}
				}
				Bench.P("pose manip");
				this.setDefaultPose();
				Bench.Pend("pose manip");
			}
			else
			{
				bool flag = this.t_state <= 0f;
				if (flag)
				{
					this.t_state = 0f;
				}
				if (this.Skill.runState(flag, ref this.t_state, ref this.manip))
				{
					if (this.isMoveScriptActive(false) || (base.isCanselableMovingPD() && this.canJump() && this.hasD(M2MoverPr.DECL.ABORT_BY_MOVE) && (!this.hasD(M2MoverPr.DECL.STOP_ACT) || !this.hasD(M2MoverPr.DECL.STOP_EVADE))))
					{
						this.changeState(PR.STATE.NORMAL);
					}
				}
				else
				{
					PR.STATE state = this.state;
					if (state <= PR.STATE.EV_GACHA)
					{
						switch (state)
						{
						case PR.STATE.ENEMY_SINK:
							if (flag)
							{
								this.RCenemy_sink.Clear();
								if (this.bounds_ != M2MoverPr.BOUNDS_TYPE.PRESSCROUCH)
								{
									this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
								}
								if (X.sensitive_level < 2 && this.Ser.getLevel(SER.NEAR_PEE) >= 1 && this.checkNoelJuice((float)(this.hasD(M2MoverPr.DECL.FLAG1) ? 150 : 3), true, false, -1))
								{
									this.Ser.Add(SER.NEAR_PEE, -1, 2, false);
									this.addD(M2MoverPr.DECL.FLAG0);
									this.UP.setFade("wetten_osgm", UIPictureBase.EMSTATE.NORMAL, false, false, false);
								}
							}
							if (this.hasD(M2MoverPr.DECL.FLAG0) && this.t_state >= 18f && this.Ser.getLevel(SER.NEAR_PEE) >= 2)
							{
								this.Ser.Cure(SER.NEAR_PEE);
								this.remD(M2MoverPr.DECL.FLAG0);
								this.executeSplashNoelJuice(false, true, 0, false, false, false, false);
							}
							if (this.t_state >= 15f)
							{
								this.decline &= (M2MoverPr.DECL)(-3);
							}
							if (!this.Anm.poseIs("sink", "stand2sink") || this.t_state >= 55f)
							{
								this.changeState(PR.STATE.NORMAL);
								goto IL_056F;
							}
							goto IL_056F;
						case PR.STATE.SHIELD_BREAK_STUN:
							if (flag)
							{
								this.EpCon.breath_key = "breath_e";
								this.SpSetPose(this.isPoseDown(false) ? "down2confused" : "stand2confused", -1, null, false);
							}
							this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
							if (!this.Ser.has(SER.SHIELD_BREAK))
							{
								this.changeState(PR.STATE.NORMAL);
								goto IL_056F;
							}
							goto IL_056F;
						case PR.STATE.LAYING_EGG:
						case PR.STATE.ORGASM:
							goto IL_053F;
						case PR.STATE.GAMEOVER_RECOVERY:
							break;
						default:
							if (state != PR.STATE.ONNIE)
							{
								if (state != PR.STATE.EV_GACHA)
								{
									goto IL_053F;
								}
								this.AbsorbCon.runAbsorbPr(this, this.t_state, this.TS);
								if (!this.AbsorbCon.isActive())
								{
									this.changeState(PR.STATE.NORMAL);
									goto IL_056F;
								}
								goto IL_056F;
							}
							else
							{
								if (this.Onnie == null)
								{
									this.changeState(PR.STATE.NORMAL);
									goto IL_056F;
								}
								if (flag)
								{
									this.EpCon.breath_key = "breath_e";
									this.Onnie.init(0);
								}
								if (this.crouching > 1f)
								{
									this.crouching = 1f;
								}
								float time = this.Onnie.get_time();
								this.AbsorbCon.runAbsorbPr(this, time, this.TS);
								if (!this.canStartFrustratedMasturbate(false) || !this.Onnie.runMusturbate(this.TS))
								{
									this.changeState(PR.STATE.NORMAL);
									goto IL_056F;
								}
								goto IL_056F;
							}
							break;
						}
					}
					else if (state != PR.STATE.WATER_CHOKED)
					{
						if (state - PR.STATE.BENCH > 2)
						{
							if (state != PR.STATE.BENCH_SITDOWN_WAIT)
							{
								goto IL_053F;
							}
							if (this.t_state >= 180f)
							{
								this.changeState(PR.STATE.NORMAL);
								goto IL_056F;
							}
							this.setDefaultPose();
							goto IL_056F;
						}
					}
					else
					{
						this.crouching = 0f;
						this.Ser.Add(SER.WORM_TRAPPED, -1, 99, false);
						base.getFootManager().lockPlayFootStamp(10);
						if (this.MistApply == null || !this.MistApply.isFloatingWaterChoke())
						{
							this.MistApply.abortWaterChokeRelease();
							goto IL_056F;
						}
						goto IL_056F;
					}
					if (!this.runBenchOrGoRecovery(flag))
					{
						this.changeState(PR.STATE.NORMAL);
						goto IL_056F;
					}
					goto IL_056F;
					IL_053F:
					if (!this.processLayingOrOrgasm(0f) && this.isDamagingOrKo())
					{
						Bench.P("run damaging");
						this.runDamaging();
						Bench.Pend("run damaging");
					}
				}
			}
			IL_056F:
			Bench.P("pre _after ");
			this.runEnemySink((this.manip & M2MoverPr.PR_MNP.NO_SINK) == (M2MoverPr.PR_MNP)0);
			if (this.Ydrg != null && !this.Ydrg.run(this.TS))
			{
				this.Ydrg = null;
			}
			float num = (this.secureTimeState() ? 0f : this.TS);
			float num2 = (this.isTrappedState() ? 0f : (num * this.Ser.baseTimeScaleRev()));
			this.GSaver.run(num * ((this.hp_crack > 0) ? (1f / (float)(this.hp_crack + 1)) : 1f));
			this.reduceCrackMp(this.GaugeBrk.getReducePlayerMpValue(num2));
			this.AbsorbCon.run(num * this.Ser.baseTimeScaleRev());
			this.EggCon.run(num);
			this.runSer(num2);
			if (this.wind_apply_t > 0f)
			{
				this.wind_apply_t = X.Mx(this.wind_apply_t - this.TS, 0f);
			}
			if (this.frozen_fineaf_time > 0f)
			{
				this.frozen_fineaf_time = X.Mx(this.frozen_fineaf_time - this.TS, 0f);
				this.Anm.fineFreezeFrame();
			}
			if (!this.is_alive || this.Ser.has(SER.EGGED_2))
			{
				this.NoDamage.Penetrate(NDMG.DEFAULT, 5);
				if (this.EggCon.total > 0 && IN.totalframe % ((this.enemy_targetted == 0) ? 8 : 32) == 0)
				{
					this.EggCon.check_holded_mp += (float)X.IntC((float)this.EggCon.total / 25f);
					if (!this.EggCon.need_fine_mp)
					{
						this.EggCon.need_fine_mp = true;
					}
				}
			}
			this.runLayingEggCheck(false);
			this.runEpOrgasmCheck(this.TS);
			if (this.need_check_bounds && this.bounds_ != M2MoverPr.BOUNDS_TYPE.PRESSCROUCH)
			{
				if (this.isPressDamageState(this.state))
				{
					base.forceCrouch(false, false);
				}
				else
				{
					this.need_check_bounds = false;
					base.forceCrouch(false, false);
					if (this.isFrozen())
					{
						this.setBounds(this.Anm.poseIs(POSE_TYPE.DOWN, true) ? M2MoverPr.BOUNDS_TYPE.DOWN : (this.Anm.poseIs(POSE_TYPE.CROUCH, true) ? M2MoverPr.BOUNDS_TYPE.CROUCH : M2MoverPr.BOUNDS_TYPE.NORMAL), false);
					}
					else if (this.state != PR.STATE.GAMEOVER_RECOVERY)
					{
						if (this.isDamagingOrKo())
						{
							if (this.state == PR.STATE.DAMAGE_LT || this.state == PR.STATE.DAMAGE_LT_KIRIMOMI)
							{
								this.setBounds(M2MoverPr.BOUNDS_TYPE.NORMAL, false);
							}
							else if (this.isFlyingDamageState(this.state) && !this.Anm.poseIs("damage_thunder"))
							{
								this.setBounds(M2MoverPr.BOUNDS_TYPE.DAMAGE_L, false);
							}
							else if (this.isDownState(this.state) || this.isPoseDown(false))
							{
								this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, false);
							}
							else
							{
								this.setBounds(this.Anm.poseIs(POSE_TYPE.CROUCH, false) ? M2MoverPr.BOUNDS_TYPE.CROUCH : M2MoverPr.BOUNDS_TYPE.NORMAL, false);
							}
						}
						else
						{
							this.setBounds(this.is_crouch ? M2MoverPr.BOUNDS_TYPE.CROUCH : M2MoverPr.BOUNDS_TYPE.NORMAL, false);
						}
					}
					if (base.is_crouch && this.bounds_ == M2MoverPr.BOUNDS_TYPE.NORMAL)
					{
						this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
					}
				}
			}
			else
			{
				base.forceCrouch(false, this.isNormalState());
			}
			if (this.t_state < 0f)
			{
				this.t_state = X.Mn(this.t_state + this.TS, 0f);
			}
			else
			{
				this.t_state += this.TS;
			}
			this.TeCon.checkRepositImmediate();
			Bench.Pend("pre _after ");
		}

		protected void runSer(float _TSp)
		{
			ulong pre_bits = this.Ser.get_pre_bits();
			this.Ser.run(_TSp);
			if (pre_bits != this.Ser.get_pre_bits())
			{
				if (this.isMagicExistState(this.state))
				{
					this.recheck_emot = true;
				}
				this.Ser.checkSer();
				Stomach stomach = this.NM2D.IMNG.getStomach(this);
				float stomachApplyRatio = this.Ser.getStomachApplyRatio();
				if (stomachApplyRatio != stomach.ser_apply_ratio)
				{
					stomach.ser_apply_ratio = stomachApplyRatio;
					stomach.fineEffect(true);
				}
				this.Anm.fineSerState();
				if ((pre_bits & 268435456UL) != (this.Ser.get_pre_bits() & 268435456UL))
				{
					this.fineFrozenAppearance();
				}
			}
		}

		private string setDefaultPose()
		{
			string text;
			if (this.MistApply != null && this.MistApply.isFloatingWaterChoke())
			{
				bool flag = base.forceCrouch(false, false);
				text = (base.view_crouching ? "water_choked_down" : "water_choked");
				this.Phy.walk_xspeed *= 0.5f;
				base.jumpRaisingQuit(false);
				if (!flag)
				{
					this.crouching = 0f;
				}
			}
			else if (!((text = this.Skill.getPoseTitleOnNormal()) != ""))
			{
				if (this.FootD.FootIsLadder())
				{
					if (!this.Anm.nextPoseIs("ladder"))
					{
						text = "ladder";
						if (this.Anm.strictPoseIs("ladder"))
						{
							this.Anm.timescale = (float)((this.Anm.cframe % 2 == 1) ? 0 : 1);
						}
					}
					if (this.Anm.strictPoseIs("ladder_wait"))
					{
						this.Anm.timescale = 1f;
					}
				}
				else if (!this.canJump() && (this.Anm.poseIs(POSE_TYPE.JUMP | POSE_TYPE.FALL, false) || base.jump_occuring || this.Phy.gravity_added_velocity > 0.004f))
				{
					text = ((base.vy < -0.01f && (this.Anm.poseIs(POSE_TYPE.JUMP, false) || base.jump_raising)) ? "jump" : "fall");
				}
				else
				{
					float walk_xspeed = this.Phy.walk_xspeed;
					if (base.isRunStopping())
					{
						if (this.Anm.poseIs("run_stop"))
						{
							text = "run_stop";
						}
					}
					else if (base.isRunning())
					{
						text = "run";
					}
					bool flag2 = base.view_crouching || base.forceCrouch(false, false);
					if (text == "")
					{
						PxlPose currentPose = this.Anm.getCurrentPose();
						text = ((Mathf.Abs(walk_xspeed) > 0f) ? (base.isRunning() ? "run" : (flag2 ? "crawl" : "walk")) : (flag2 ? "crouch" : ((currentPose.end_jump_title != "stand") ? "stand" : "")));
					}
				}
			}
			if (text != "")
			{
				if (this.fix_pose != null && (!(text == "stand") || this.break_pose_fix_on_walk_level >= 3 || this.Anm.poseIs("stand")) && this.break_pose_fix_on_walk_level > 0)
				{
					this.fix_pose = null;
					this.break_pose_fix_on_walk_level = 0;
				}
				if (text == "fall" && this.isPoseCrouch(false))
				{
					this.TeFallShift = this.TeCon.setAppearFrom(AIM.B, 29f, 28f);
				}
				this.SpSetPose(text, (this.Mp.floort == 0f) ? 1 : (-1), null, false);
			}
			if (this.crouching != 0f)
			{
				this.crouching = ((this.crouching < 0f) ? X.Mn(this.crouching + this.TS, 0f) : (this.crouching + this.TS));
				if (this.crouching > 0f && this.bounds_ == M2MoverPr.BOUNDS_TYPE.NORMAL)
				{
					this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
				}
			}
			return text;
		}

		public override bool continue_crouch
		{
			get
			{
				return base.continue_crouch || this.Skill.continue_crouch;
			}
		}

		protected override void jumpInitialize()
		{
			this.Skill.evadeTimeBlur(true, true, false);
			base.jumpInitialize();
		}

		public override float jump_speed_ratio
		{
			get
			{
				return base.jump_speed_ratio * this.Ser.jumpSpeedRate();
			}
		}

		public override bool canClimbToLadder()
		{
			return base.canClimbToLadder() && this.Skill.canClimbToLadder();
		}

		public override void climbToTopLiftFromLadder(M2BlockColliderContainer.BCCLine RideTo, bool to_under)
		{
			base.climbToTopLiftFromLadder(RideTo, to_under);
			this.SpSetPose(to_under ? "ladder_t2b" : "ladder_b2t", -1, null, false);
		}

		public override bool climbLadder(bool to_under, int ascend_time)
		{
			if (this.Anm.strictPoseIs("ladder_t2b") || !this.isNormalState() || this.Skill.magic_chanting || this.Skill.isShieldOpeningOnNormal(true, false))
			{
				return false;
			}
			if (!base.climbLadder(to_under, ascend_time))
			{
				return false;
			}
			this.Anm.clearStandPoseTime();
			if (this.Anm.strictPoseIs("ladder"))
			{
				this.Anm.animReset((this.Anm.cframe / 2 + 1) % 2 * 2, 1f);
				this.Anm.timescale = 1f;
			}
			return true;
		}

		public override void runPhysics(float fcnt)
		{
			if (base.jumping_in_water && !base.checkCornerConfig(0.001f, -0.2f, new Func<int, bool>(CCON.isWater)))
			{
				this.Phy.releaseWaterDunk();
				if (this.NM2D.MIST.isActive())
				{
					this.NM2D.MIST.releaseWaterDunk(this);
				}
			}
			base.runPhysics(fcnt);
		}

		public bool processLayingOrOrgasm(float fcnt)
		{
			bool flag = true;
			bool flag2 = this.Ser.has(SER.LAYING_EGG);
			switch (this.state)
			{
			case PR.STATE.LAYING_EGG:
				if (this.t_state <= 0f)
				{
					this.t_state = 0f;
					this.playVo("laying_l", false, false);
					UIPictureBase.FlgStopAutoFade.Add("EggCon");
					this.UP.applyLayingEgg(true);
				}
				if (!EnemySummoner.isActiveBorder())
				{
					PR.PunchDecline(50, false);
				}
				this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, false);
				if (flag2 && this.UP.isTortureEmot() != 1 && this.UP.getCurEmot() != UIEMOT.LAYING_EGG)
				{
					this.UP.recheck_emot = true;
				}
				if (!flag2 && this.t_state >= 120f)
				{
					if (fcnt != 0f && this.UP.isPoseLayingEgg())
					{
						this.recheck_emot = true;
					}
					if (this.EpCon.breath_key == null)
					{
						this.EpCon.breath_key = "breath_aft";
						this.Ser.Add(SER.TIRED, 240, 99, false);
						if (!this.poseIs(POSE_TYPE.ORGASM))
						{
							this.SpSetPose("down_u", -1, null, false);
						}
					}
					if (!base.isActionO(0))
					{
						goto IL_063E;
					}
					if (this.is_alive)
					{
						this.SpSetPose("wakeup_b", -1, null, false);
						this.changeState(PR.STATE.NORMAL);
						goto IL_063E;
					}
					this.changeState(PR.STATE.DAMAGE_L_LAND);
					goto IL_063E;
				}
				else
				{
					if (this.Anm.poseIs("orgasm_down", "orgasm_down_laying_egg") && (this.t_state <= 0f || X.XORSP() < 0.05f))
					{
						this.Anm.setPose("orgasm_down_laying_egg", 1, false);
						goto IL_063E;
					}
					goto IL_063E;
				}
				break;
			case PR.STATE.ORGASM:
				if (this.t_state <= 0f)
				{
					this.t_state = 0f;
					this.EpCon.breath_key = "breath_e";
				}
				if (!EnemySummoner.isActiveBorder())
				{
					PR.PunchDecline(50, false);
				}
				this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, false);
				if (flag2 && this.UP.isTortureEmot() != 1 && this.UP.getCurEmot() != UIEMOT.LAYING_EGG)
				{
					this.UP.recheck_emot = true;
				}
				if (!this.EpCon.isOrgasm())
				{
					if (base.isActionO(0) || !this.is_alive)
					{
						this.quitOrgasm();
						goto IL_063E;
					}
					goto IL_063E;
				}
				else
				{
					float orgasm_absorb_time = this.EpCon.get_orgasm_absorb_time();
					this.AbsorbCon.runAbsorbPr(this, orgasm_absorb_time, this.TS);
					if (this.Anm.poseIs("orgasm_down", "orgasm_down_2", "orgasm_down_laying_egg"))
					{
						if (this.t_state <= 0f || X.XORSP() < 0.05f + (this.Ser.has(SER.LAYING_EGG) ? 0.1f : 0f))
						{
							this.Anm.setPose("orgasm_down_laying_egg", 1, false);
							goto IL_063E;
						}
						goto IL_063E;
					}
					else
					{
						if (!this.Anm.poseIs("downdamage", "downdamage_t", "orgasm_from_stand", "stand2orgasm", "laying_egg", "must_orgasm_2", "bench_must_orgasm_2"))
						{
							this.Anm.setPose((this.isPoseDown(false) && !this.isPoseBack(false)) ? "orgasm_down_2" : "orgasm_from_stand", 1, false);
							goto IL_063E;
						}
						goto IL_063E;
					}
				}
				break;
			case PR.STATE.WATER_CHOKED_RELEASE:
				if (!this.hasD(M2MoverPr.DECL.FLAG0))
				{
					this.Ser.Cure(SER.WORM_TRAPPED);
					this.addD((M2MoverPr.DECL)50331648);
					bool flag3 = true;
					bool flag4 = false;
					bool flag5 = this.hasD(M2MoverPr.DECL.INIT_A);
					if (this.is_alive)
					{
						float num = 0f;
						float num2 = 0f;
						this.getMouthPosition(base.forceCrouch(true, false), false, false, false, out num, out num2, false);
						if (!base.forceCrouch(false, false))
						{
							float num3;
							float num4;
							this.getMouthPosition(false, true, true, false, out num3, out num4, false);
							int config = this.Mp.getConfig((int)num, (int)num2);
							if (!CCON.isWater(config) && CCON.canStand(config) && (!flag5 || CCON.isWater(this.Mp.getConfig((int)num3, (int)num4))))
							{
								flag3 = false;
							}
						}
						else if (!CCON.isWater(this.Mp.getConfig((int)num, (int)num2)))
						{
							flag3 = false;
							flag4 = true;
						}
					}
					if (flag3)
					{
						this.SpSetPose(this.isPoseDown(false) ? "water_choked_release" : "stand2water_choked_release", -1, null, false);
						this.EpCon.breath_key = "water_choked_release_b";
					}
					else
					{
						this.SpSetPose(flag4 ? "crouch_dmg_breathe" : "dmg_breathe", -1, null, false);
						this.EpCon.breath_key = "cough";
					}
					this.playVo("water_choked_release_a", false, false);
				}
				if (this.t_state < 40f)
				{
					goto IL_063E;
				}
				if (this.MistApply == null || this.MistApply.canReleaseWaterChokeStun())
				{
					this.remD((M2MoverPr.DECL)33554447);
					if (base.isActionO(0))
					{
						this.changeState(PR.STATE.NORMAL);
						goto IL_063E;
					}
					goto IL_063E;
				}
				else
				{
					if (!this.hasD(M2MoverPr.DECL.FLAG1) && this.MistApply.waterDelayActive())
					{
						this.addD((M2MoverPr.DECL)15);
						this.remD(M2MoverPr.DECL.FLAG0);
						this.SpSetPose("water_choked", -1, null, false);
						this.EpCon.breath_key = null;
						goto IL_063E;
					}
					goto IL_063E;
				}
				break;
			case PR.STATE.SLEEP:
				if (this.t_state <= 0f)
				{
					this.t_state = 0f;
					this.recheck_emot = true;
					this.EpCon.breath_key = "breath_sleep";
					if (!this.VO.isPlaying())
					{
						this.playVo("sleep_init", false, false);
					}
				}
				this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, false);
				if (!this.Ser.isSleepDown() && !this.Ser.isStun() && this.is_alive)
				{
					if (this.MistApply != null && !this.MistApply.isWaterChoking())
					{
						this.MistApply.cureAll();
					}
					this.changeState(PR.STATE.NORMAL);
					goto IL_063E;
				}
				goto IL_063E;
			case PR.STATE.FROZEN:
				if (this.t_state <= 0f)
				{
					this.t_state = 0f;
				}
				if (!EnemySummoner.isActiveBorder())
				{
					PR.PunchDecline(50, false);
				}
				if (!this.isFrozen())
				{
					if (this.MistApply != null && !this.MistApply.isWaterChoking())
					{
						this.MistApply.cureAll();
					}
					this.changeState(PR.STATE.NORMAL);
					goto IL_063E;
				}
				goto IL_063E;
			}
			flag = false;
			IL_063E:
			if (fcnt > 0f)
			{
				if (this.t_state < 0f)
				{
					this.t_state = X.Mn(this.t_state + this.TS, 0f);
				}
				else
				{
					this.t_state += this.TS;
				}
				this.EggCon.run(fcnt);
				this.EpCon.run(1f);
				this.runLayingEggCheck(false);
				this.runEpOrgasmCheck(fcnt);
			}
			return flag;
		}

		public override void runPost()
		{
			Bench.P("Noel1");
			base.runPost();
			Bench.Pend("Noel1");
			Bench.P("Noel-Skill");
			this.Skill.runPost();
			Bench.Pend("Noel-Skill");
			Bench.P("Noel-Other");
			this.AbsorbCon.runPost();
			this.Anm.runPost(this.TS);
			Bench.Pend("Noel-Other");
		}

		public override bool runUi()
		{
			Bench.P("Noel-runUi");
			bool flag = base.runUi();
			if (!flag && this.NM2D.can_open_gamemenu && base.M2D.pre_map_active)
			{
				if (base.isMenuPD(2))
				{
					this.NM2D.menu_open = NelM2DBase.MENU_OPEN.OPEN;
					base.jump_hold_lock = true;
				}
				else if (base.isMapPD(2))
				{
					if (this.is_alive && !this.isDamagingOrKo())
					{
						if (base.isItmO(0) && this.NM2D.IMNG.has_recipe_collection && !EnemySummoner.isActiveBorder())
						{
							object obj = null;
							if (this.NM2D.IMNG.PreSupplyTarget is NelItemManager.NelItemDrop)
							{
								NelItemManager.NelItemDrop nelItemDrop = this.NM2D.IMNG.PreSupplyTarget as NelItemManager.NelItemDrop;
								if (base.hasTalkableEventItem(nelItemDrop))
								{
									obj = nelItemDrop.Itm;
								}
							}
							else if (this.NM2D.IMNG.PreSupplyTarget is M2EventItem_ItemSupply)
							{
								M2EventItem_ItemSupply m2EventItem_ItemSupply = this.NM2D.IMNG.PreSupplyTarget as M2EventItem_ItemSupply;
								if (base.hasTalkableEventItem(m2EventItem_ItemSupply))
								{
									obj = this.NM2D.IMNG.PreSupplyTarget;
								}
							}
							if (obj == null && EnemySummoner.isActiveBorder())
							{
								obj = EnemySummoner.ActiveScript;
							}
							if (obj == null && M2LpSummon.NearLpSmn != null)
							{
								obj = M2LpSummon.NearLpSmn.Reader;
							}
							if (this.NM2D.GM.initRecipeBook(null, obj) && (this.is_alive || !CFG.sp_go_cheat))
							{
								UseItemSelector usel = this.NM2D.IMNG.USel;
								if (usel.isSelecting())
								{
									usel.run(false, false);
								}
							}
							else
							{
								this.NM2D.menu_open = NelM2DBase.MENU_OPEN.OPEN_MAP;
							}
						}
						else
						{
							this.NM2D.menu_open = NelM2DBase.MENU_OPEN.OPEN_MAP;
						}
					}
					base.jump_hold_lock = true;
				}
			}
			Bench.Pend("Noel-runUi");
			this.EpCon.runUi();
			return flag;
		}

		protected override void talkToFocusExecuted()
		{
			if (!EnemySummoner.isActiveBorder())
			{
				base.talkToFocusExecuted();
				PR.PunchDecline(20, false);
			}
		}

		public float base_TS
		{
			get
			{
				return this._base_TS;
			}
		}

		public override float TS
		{
			get
			{
				return base.TS * this.base_TS;
			}
		}

		public override float animator_TS
		{
			get
			{
				if (this.isFrozen())
				{
					return 0f;
				}
				float num = this.Anm.animator_TS * X.Pow2(this.Ser.baseTimeScale());
				return ((this.MScr != null) ? this.MScr.ms_timescale : 1f) * num;
			}
		}

		public float uipicture_TS
		{
			get
			{
				if (!this.isFrozen())
				{
					return this.base_TS;
				}
				return 0f;
			}
		}

		public void runPostTorture()
		{
			if (!this.AbsorbCon.use_torture)
			{
				this.Anm.timescale = 1f;
				return;
			}
			if (!this.AbsorbCon.syncTorturePosition(this.Anm.pose_title))
			{
				this.AbsorbCon.releaseFromTarget(this);
			}
		}

		public override void quitCrouch(bool delaying = false, bool no_reset_time = false, bool no_set_anim = false)
		{
			base.quitCrouch(delaying, no_reset_time, no_set_anim || this.Anm.poseIs(POSE_TYPE.STAND, false));
		}

		public override void checkCurrentPoint(bool check_stand, bool check_talk, bool check_basepr, bool check_other)
		{
			base.checkCurrentPoint(check_stand, check_talk, check_basepr, check_other);
			if (check_other && !base.jump_occuring)
			{
				base.recheckForceCrouch();
			}
		}

		public override M2Mover setAim(AIM n, bool sprite_force_aim_set = false)
		{
			if (this.aim != n || sprite_force_aim_set)
			{
				this.aim = n;
				if (this.Anm == null)
				{
					return this;
				}
				if (!base.fix_aim || sprite_force_aim_set)
				{
					this.Anm.setAim(n, 0, false);
				}
			}
			return this;
		}

		public override void changeRiding(IFootable Fd, FOOTRES footres)
		{
			base.changeRiding(Fd, footres);
			if (Fd != null && Map2d.can_handle && !this.isBusySituation())
			{
				this.NM2D.CheckPoint.fineFoot(this, Fd as M2BlockColliderContainer.BCCLine, false);
			}
			if (footres != FOOTRES.FOOTED)
			{
				if (footres - FOOTRES.JUMPED <= 1)
				{
					this.Skill.fineFoot();
					if (this.isNormalState())
					{
						this.SpSetPose("jump", -1, null, false);
					}
				}
			}
			else
			{
				this.Skill.fineFoot();
				if (base.isRunStopping())
				{
					base.stopRunning(false, false);
				}
				if (this.TeFallShift != null)
				{
					this.TeFallShift.destruct();
					this.TeFallShift = null;
				}
				if (this.isMagicExistState(this.state))
				{
					this.checkOazuke();
				}
			}
			if (this.FootD.FootIsLadder())
			{
				this.Anm.clearStandPoseTime();
			}
		}

		public override M2SoundPlayerItem playFootSound(string t, byte voice_priority_manual = 1)
		{
			if (this.Mp == null)
			{
				return null;
			}
			return base.playFootSound(t, voice_priority_manual);
		}

		public override IFootable checkSkipLift(M2BlockColliderContainer.BCCLine _P)
		{
			return base.checkSkipLift(((this.crouching >= 4f || this.crouching < 0f) && this.isNormalState(this.state) && (this.manip & M2MoverPr.PR_MNP.NO_JUMP_AND_MOVING) == (M2MoverPr.PR_MNP)0 && !this.isSlidingState(this.state)) ? null : _P);
		}

		protected override bool checkForceCrouch(int cx)
		{
			return base.checkForceCrouch(cx) || (this.PressCheck != null && this.PressCheck.is_force_crouch);
		}

		public override void breakPoseFixOnWalk(int level)
		{
			if (this.fix_pose != null)
			{
				this.break_pose_fix_on_walk_level = level;
			}
		}

		public override void walkByAim(int aim, float speed = 0f)
		{
			base.walkByAim(aim, speed);
			if (aim == -1)
			{
				this.Phy.walk_xspeed = 0f;
				return;
			}
			this.Phy.walk_xspeed = (float)CAim._XD(aim, 1) * speed;
			if (this.Phy.walk_xspeed != 0f)
			{
				this.setAim((AIM)aim, false);
			}
		}

		public override void walkBySpeed(float __vx, float __vy)
		{
			base.walkBySpeed(0f, __vy);
			if (__vx == 0f && __vy == 0f)
			{
				this.walkByAim(-1, 0f);
				return;
			}
			if (__vx != 0f)
			{
				base.walkByAim((__vx > 0f) ? 2 : 0, __vx);
			}
		}

		protected override float calcWalkSpeed(int move_aim_ex)
		{
			float num = base.calcWalkSpeed(move_aim_ex) * this.Ser.xSpeedRate();
			if (this.Skill.isChantingMagicOnNormal(true, true))
			{
				num = X.absMn(num, 0.016f);
			}
			return num;
		}

		public void changeState(PR.STATE _state)
		{
			PR.STATE state = this.state;
			if (state == PR.STATE.DAMAGE_LT_KIRIMOMI || _state == PR.STATE.NORMAL)
			{
				this.Anm.rotationR = 0f;
			}
			if (this.isLayingEgg(state))
			{
				UIPictureBase.FlgStopAutoFade.Rem("EggCon");
			}
			if (this.is_crouch || !this.canJump())
			{
				base.recheckForceCrouch();
			}
			bool flag = this.isTrappedState(_state);
			if (this.isTrappedState(state) && !flag && this.Ser.has(SER.WORM_TRAPPED))
			{
				return;
			}
			bool flag2 = this.isDamagingOrKo(state);
			bool flag3 = this.isDamagingOrKo(_state);
			if (_state == PR.STATE.NORMAL && this.isFrozen())
			{
				_state = PR.STATE.FROZEN;
			}
			else if (_state == PR.STATE.NORMAL && this.MistApply != null && this.MistApply.isWaterChokeDamageAlreadyApplied())
			{
				this.MistApply.waterReleasedInChoking();
				_state = PR.STATE.WATER_CHOKED_RELEASE;
			}
			else if (_state == PR.STATE.NORMAL && this.Ser.has(SER.SHIELD_BREAK))
			{
				_state = PR.STATE.SHIELD_BREAK_STUN;
			}
			else if (_state == PR.STATE.NORMAL && this.Ser.has(SER.BURNED))
			{
				_state = PR.STATE.DAMAGE_BURNED;
			}
			else if (!flag3 && !this.is_alive)
			{
				if (_state == PR.STATE.LAYING_EGG)
				{
					flag3 = true;
				}
				else if (!this.isSleepingDownState(_state) && !flag)
				{
					state = PR.STATE.DOWN_STUN;
					flag3 = true;
					if (_state == PR.STATE.NORMAL)
					{
						if (!this.isPoseDown(false))
						{
							this.SpSetPose(this.isPoseCrouch(false) ? "stun2down_2" : "stunned", -1, null, false);
							_state = PR.STATE.DOWN_STUN;
						}
						else
						{
							if (!this.Anm.poseIs("downdamage_t", "downdamage"))
							{
								this.SpSetPose("stun2down_2", -1, null, false);
							}
							_state = PR.STATE.DAMAGE_L_LAND;
						}
					}
				}
			}
			bool flag4 = this.isAbsorbState(state);
			bool flag5 = this.isAbsorbState(_state);
			if (flag4 != flag5)
			{
				if (!flag5)
				{
					this.AbsorbCon.gacha_renderable = false;
					this.NM2D.MIST.FlgHideSurface.Rem("ABSORB");
					this.quitTortureAbsorb();
					this.Anm.FlgDropCane.Rem("ABSORB");
				}
				else
				{
					this.AbsorbCon.gacha_renderable = true;
					this.NM2D.MIST.FlgHideSurface.Add("ABSORB");
				}
			}
			if (_state == PR.STATE.NORMAL)
			{
				this.NM2D.GM.BenchChip = ((this.NM2D.GM.isActive() && this.NM2D.GM.BenchChip != null) ? this.getNearBench(false, false) : null);
				if (this.AbsorbCon.isActive())
				{
					this.AbsorbCon.clear();
				}
				this.recheck_emot = true;
				this.FootD.recheck_side = true;
			}
			else
			{
				base.jumpRaisingQuit(true);
			}
			if (this.Onnie != null && !this.isMasturbateState(_state))
			{
				if (this.Onnie.isFinished() && !this.isBenchState(this.state))
				{
					this.cureMpNotHunger(false);
				}
				this.Onnie = this.Onnie.quit();
			}
			if ((this.fix_pose != null && this.break_pose_fix_on_walk_level == 1) || this.break_pose_fix_on_walk_level >= 3)
			{
				this.break_pose_fix_on_walk_level = 0;
				this.fix_pose = null;
			}
			this.Phy.walk_xspeed = 0f;
			if (!this.isSinkState(_state) && !this.isDamagingOrKo(_state) && this.bounds_ == M2MoverPr.BOUNDS_TYPE.PRESSCROUCH)
			{
				this.setBounds(M2MoverPr.BOUNDS_TYPE.NORMAL, false);
			}
			this.t_state = -1f;
			this.Anm.setAim((CAim._XD(this.aim, 1) >= 0) ? AIM.R : AIM.L, -1, false);
			if (this.TeFallShift != null)
			{
				this.TeFallShift.destruct();
				this.TeFallShift = null;
			}
			bool flag6 = this.isBenchState(state);
			bool flag7 = this.isBenchState(_state);
			if (flag6)
			{
				(base.M2D as NelM2DBase).IMNG.hidePopUp(NelItemManager.POPUP.BENCH);
				if (!flag7)
				{
					this.FootD.no_change_shift_pixel = false;
					if (!this.NM2D.GM.isActive())
					{
						this.NM2D.GM.BenchChip = null;
					}
				}
			}
			else
			{
				this.isGameoverRecover();
			}
			bool flag8 = this.isMagicExistState(_state);
			if (!flag8 && this.FootD.FootIsLadder())
			{
				this.FootD.initJump(false, true, true);
			}
			this.decline = (this.decline & (M2MoverPr.DECL)4112) | ((this.is_alive && flag8) ? ((M2MoverPr.DECL)0) : ((M2MoverPr.DECL)11)) | (this.isNormalState(_state) ? ((M2MoverPr.DECL)0) : M2MoverPr.DECL.STOP_MG) | this.outfit_default_dcl;
			if (flag7)
			{
				this.decline |= M2MoverPr.DECL.THROW_RAY;
			}
			if (flag6 != flag7)
			{
				if (flag7)
				{
					COOK.FlgTimerStop.Add("BENCH");
				}
				else
				{
					COOK.FlgTimerStop.Rem("BENCH");
				}
			}
			this.Skill.changeState(state, _state);
			if (flag)
			{
				this.AbsorbCon.clear();
				this.Ser.Add(SER.WORM_TRAPPED, -1, 99, false);
				this.Skill.killHoldMagic(false);
			}
			if (flag2 && !flag3)
			{
				base.weight = this.weight0;
				this.RCenemy_sink.Lock(18f, true);
				if (this.UP != null)
				{
					this.UP.recheck(30, 0);
				}
				this.lava_burn_dount = 0;
				this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
			}
			if (flag2 && this.isPressDamageState(state))
			{
				this.Phy.remLockMoverHitting(HITLOCK.PRESSER);
				this.TeCon.removeSpecific(TEKIND.ABSORB_BOUNCY);
			}
			if (flag2 != flag3)
			{
				base.M2D.Cam.blurCenterIfFocusing(this);
				if (flag2 && !flag3)
				{
					this.Anm.FlgDropCane.Rem("DMG");
				}
			}
			if (flag3 && this.Ser.has(SER.SHIELD_BREAK))
			{
				M2SerItem m2SerItem = this.Ser.Get(SER.SHIELD_BREAK);
				if (m2SerItem != null)
				{
					m2SerItem.progressTime(((float)m2SerItem.maxt - m2SerItem.getAf()) * 0.33f);
				}
			}
			if (flag3 || flag || this.isAbsorbState(_state))
			{
				this.addD(M2MoverPr.DECL.STOP_FOOTSND);
			}
			if (state == PR.STATE.ENEMY_SINK)
			{
				this.RCenemy_sink.Clear();
				this.RCenemy_sink.Lock(18f, true);
			}
			if (this.isEvadeState(state) || this.isBenchState(state) || this.isBurstState(state) || state == PR.STATE.DAMAGE_LT_KIRIMOMI || state == PR.STATE.DAMAGE_PRESS_LR)
			{
				this.Phy.remLockMoverHitting(HITLOCK.EVADE);
				this.Phy.remLockGravity(this);
			}
			if (!this.isEvadeState(_state) && this.isEvadeState(state))
			{
				this.Phy.remLockMoverHitting(HITLOCK.EVADE);
			}
			bool flag9 = base.forceCrouch(false, false);
			int num = this.canCrouchState(_state);
			if (((_state == PR.STATE.NORMAL || num == 1) && this.isBO(0)) || flag9 || num == 2)
			{
				this.crouching = X.Mx(this.crouching, 10f);
			}
			else
			{
				this.quitCrouch(false, false, false);
				if (_state == PR.STATE.NORMAL && !this.Anm.next_pose_is_stand)
				{
					if (this.isPoseBackDown(false))
					{
						this.Anm.setPose("wakeup_b", -1, false);
					}
					else if (this.isPoseDown(false))
					{
						this.Anm.setPose("wakeup", -1, false);
					}
					else if (this.isPoseCrouch(false))
					{
						this.Anm.setPose("crouch2stand", -1, false);
					}
				}
			}
			if (state == PR.STATE.BURST_SCAPECAT && this.NM2D.GameOver != null)
			{
				this.NM2D.GameOver.executeScapecatRespawnAfter();
			}
			this.need_check_bounds = true;
			this.state = _state;
			this.EpCon.breath_key = null;
			PR.STATE state2 = this.state;
			if (state2 <= PR.STATE.ENEMY_SINK)
			{
				if (state2 <= PR.STATE.BURST)
				{
					if (state2 == PR.STATE.NORMAL)
					{
						if (this.Ser.has(SER.SHIELD_BREAK))
						{
							this.Ser.Cure(SER.SHIELD_BREAK);
						}
						this.checkOazuke();
						goto IL_0885;
					}
					if (state2 != PR.STATE.BURST)
					{
						goto IL_0885;
					}
				}
				else if (state2 != PR.STATE.BURST_SCAPECAT)
				{
					if (state2 != PR.STATE.ENEMY_SINK)
					{
						goto IL_0885;
					}
					this.SpSetPose(this.isPoseCrouch(false) ? "sink" : "stand2sink", -1, null, false);
					goto IL_0885;
				}
				this.addD(M2MoverPr.DECL.THROW_RAY);
			}
			else if (state2 <= PR.STATE.DAMAGE_L_HITWALL)
			{
				if (state2 != PR.STATE.DAMAGE)
				{
					if (state2 == PR.STATE.DAMAGE_L_HITWALL)
					{
						this.setBounds(M2MoverPr.BOUNDS_TYPE.NORMAL, false);
					}
				}
				else
				{
					base.weight = this.weight0 * 16f;
				}
			}
			else if (state2 != PR.STATE.DAMAGE_LT_KIRIMOMI)
			{
				if (state2 != PR.STATE.WATER_CHOKED)
				{
					if (state2 - PR.STATE.BENCH <= 1)
					{
						this.checkOazuke();
						this.Phy.addLockMoverHitting(HITLOCK.EVADE, -1f);
					}
				}
				else
				{
					this.Skill.killHoldMagic(false);
				}
			}
			else
			{
				this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, false);
				this.moveBy(0f, -0.65f, true);
			}
			IL_0885:
			if (this.state == PR.STATE.DAMAGE_L || this.state == PR.STATE.DAMAGE_LT || this.state == PR.STATE.DAMAGE_L_HITWALL)
			{
				this.NoDamage.Add(30f);
			}
			if (this.canCrouchState(this.state) == 2 || (flag9 && this.bounds_ == M2MoverPr.BOUNDS_TYPE.NORMAL))
			{
				this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
			}
			if (this.isMagicExistState(this.state))
			{
				if (this.isLO(0))
				{
					this.setAim(AIM.L, false);
				}
				else if (this.isRO(0))
				{
					this.setAim(AIM.R, false);
				}
			}
			base.fineDrawPosition();
		}

		public void setBoundsToCrouch()
		{
			this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
			if (this.isBO(0))
			{
				this.crouching = 2f;
			}
		}

		protected override bool setBounds(M2MoverPr.BOUNDS_TYPE _bounds_, bool force = false)
		{
			return this.setBounds(_bounds_, force, false);
		}

		protected bool setBounds(M2MoverPr.BOUNDS_TYPE _bounds_, bool force, bool use_move_by)
		{
			M2MoverPr.BOUNDS_TYPE bounds_ = this.bounds_;
			if (_bounds_ == M2MoverPr.BOUNDS_TYPE.NORMAL && base.forceCrouch(false, false))
			{
				_bounds_ = M2MoverPr.BOUNDS_TYPE.CROUCH;
			}
			if (_bounds_ == M2MoverPr.BOUNDS_TYPE.CROUCH && this.isCrouchWideState())
			{
				_bounds_ = M2MoverPr.BOUNDS_TYPE.CROUCH_WIDE;
			}
			if (!base.setBounds(_bounds_, force))
			{
				return false;
			}
			float sizex = this.sizex;
			float sizey = this.sizey;
			if (bounds_ - M2MoverPr.BOUNDS_TYPE.CROUCH <= 2)
			{
				base.Size(12f, 68f, ALIGN.CENTER, ALIGNY.BOTTOM, use_move_by);
			}
			else
			{
				if (bounds_ == M2MoverPr.BOUNDS_TYPE.PRESSCROUCH)
				{
					this.offset_pixel_x = 0f;
				}
				base.Size(12f, 68f, ALIGN.CENTER, this.canJump() ? ALIGNY.BOTTOM : ALIGNY.MIDDLE, use_move_by);
			}
			float num = 0f;
			float num2 = 0f;
			switch (this.bounds_)
			{
			case M2MoverPr.BOUNDS_TYPE.CROUCH:
			case M2MoverPr.BOUNDS_TYPE.CROUCH_WIDE:
				this.offset_pixel_y = 47f;
				base.Size((float)((this.bounds_ == M2MoverPr.BOUNDS_TYPE.CROUCH_WIDE) ? 30 : 12), 40f, ALIGN.CENTER, ALIGNY.BOTTOM, use_move_by);
				this.Phy.fric_reduce_x = 0.06f;
				break;
			case M2MoverPr.BOUNDS_TYPE.DOWN:
				this.offset_pixel_y = 59f;
				base.Size(30f, 28f, ALIGN.CENTER, ALIGNY.BOTTOM, use_move_by);
				this.Phy.fric_reduce_x = 0.06f;
				break;
			case M2MoverPr.BOUNDS_TYPE.DAMAGE_L:
				num = 8f;
				num2 = 46f;
				this.offset_pixel_y = 19f;
				base.Size(12f, 24f, ALIGN.CENTER, ALIGNY.MIDDLE, use_move_by);
				this.Phy.fric_reduce_x = 0.04f;
				break;
			case M2MoverPr.BOUNDS_TYPE.PRESSCROUCH:
				this.offset_pixel_y = this.presscrouch_offset_pixel_y;
				base.Size(12f, 10f, ALIGN.CENTER, this.isPressDamageState(this.state) ? ALIGNY.MIDDLE : ALIGNY.BOTTOM, use_move_by);
				this.Phy.fric_reduce_x = 0.09f;
				break;
			default:
				this.offset_pixel_y = 19f;
				this.Phy.fric_reduce_x = 0.04f;
				break;
			}
			if (this.CC != null)
			{
				M2MvColliderCreatorAtk m2MvColliderCreatorAtk = this.CC as M2MvColliderCreatorAtk;
				m2MvColliderCreatorAtk.collider_foot_slice_px_x = num;
				m2MvColliderCreatorAtk.collider_foot_slice_px_y = num2;
			}
			if (this.AbsorbCon.isActive())
			{
				float num3 = this.sizex / sizex;
				if (this.bounds_ == M2MoverPr.BOUNDS_TYPE.DOWN)
				{
					num3 = (num3 - 1f) * 0.33f + 1f;
				}
				this.AbsorbCon.resize(num3, this.sizey / sizey);
			}
			this.Anm.need_fine = true;
			return true;
		}

		public float presscrouch_offset_pixel_y
		{
			get
			{
				return 77f;
			}
		}

		public bool WakeUpInput(bool clear_pushdown = true, bool alloc_move_holding = false)
		{
			if (!this.is_alive && IN.isCancelOn(0))
			{
				return false;
			}
			if (base.isMagicO(0) || base.isAtkO(0) || base.isSubmitO(0) || (this.is_alive && ((alloc_move_holding ? (this.isLO(0) || this.isRO(0) || this.isTO(0) || this.isJumpO(0)) : (this.isLP(1) || this.isRP(1) || this.isTP(1) || this.isJumpPD(1))) || this.isBP())))
			{
				if (clear_pushdown)
				{
					IN.clearPushDown(true);
				}
				return true;
			}
			return false;
		}

		public bool isOnBenchAndCanShowModal()
		{
			return this.isOnBench(false) && !this.isMasturbating() && this.hasD(M2MoverPr.DECL.INIT_A) && !this.NM2D.GM.isActive();
		}

		private bool runBenchOrGoRecovery(bool init_flag)
		{
			bool flag = false;
			PR.STATE state = this.state;
			PR.PunchDecline(50, false);
			if (state != PR.STATE.GAMEOVER_RECOVERY)
			{
				if (state - PR.STATE.BENCH > 1)
				{
					if (state == PR.STATE.BENCH_ONNIE)
					{
						if (this.Onnie == null)
						{
							this.changeState(PR.STATE.BENCH_LOADAFTER);
							return true;
						}
						if (init_flag)
						{
							this.EpCon.breath_key = "breath_e";
							this.Onnie.init(0);
						}
						float time = this.Onnie.get_time();
						this.AbsorbCon.runAbsorbPr(this, time, this.TS);
						if (!this.Onnie.runMusturbate(this.TS))
						{
							if (!this.poseIsBenchMusturbOrgasm())
							{
								this.Anm.setPose("bench_shamed", -1, false);
							}
							if (!EV.isActive(false) && !this.NM2D.GM.isBenchMenuActive())
							{
								UIBase.Instance.gameMenuSlide(false, false);
								UIBase.Instance.gameMenuBenchSlide(false, false);
							}
							this.NM2D.FlagOpenGm.Rem("MASTURBATE");
							this.changeState(PR.STATE.BENCH_LOADAFTER);
							this.addD(M2MoverPr.DECL.INIT_A);
							if (this.NM2D.GM.isBenchMenuActive() && !EV.isActive(false))
							{
								this.NM2D.GM.evClose(true);
							}
							return true;
						}
					}
				}
				else
				{
					Bench.P("Bench-1");
					if (init_flag)
					{
						NelChipBench nelChipBench = this.getNearBench(false, false);
						if (nelChipBench == null)
						{
							this.recheck_emot = true;
							if (this.t_state < 40f)
							{
								return false;
							}
						}
						if (this.state == PR.STATE.BENCH_LOADAFTER)
						{
							if (nelChipBench != null)
							{
								this.initBenchSitDown(nelChipBench, true, false);
							}
							if (this.Mp.floort >= 50f)
							{
								this.addD(M2MoverPr.DECL.FLAG1);
							}
							this.Anm.animReset(this.Anm.getCurrentSequence().loop_to);
						}
						else
						{
							UiBenchMenu.auto_start_temp_disable = false;
						}
						if (nelChipBench != null && this.Mp.Pr == this)
						{
							this.NM2D.CheckPoint.fineFoot(this, this.FootD.get_FootBCC(), true);
						}
						if (this.poseIsBenchMusturbOrgasm())
						{
							this.EpCon.breath_key = "breath_e";
						}
						this.Ser.checkSer();
					}
					if (!this.hasD(M2MoverPr.DECL.FLAG1) && !this.hasD(M2MoverPr.DECL.FLAG_PRESS))
					{
						this.addD(M2MoverPr.DECL.FLAG1);
						UiBenchMenu.defineEvents(this, false);
					}
					if (this.t_state >= 18f && !this.hasD(M2MoverPr.DECL.INIT_A))
					{
						this.addD(M2MoverPr.DECL.INIT_A);
						if (this.state != PR.STATE.BENCH_LOADAFTER)
						{
							this.CureBench();
						}
						this.Skill.initS();
						NelChipBench nelChipBench = this.getNearBench(false, false);
						if (nelChipBench == null)
						{
							this.addD(M2MoverPr.DECL.FLAG_PRESS);
						}
						else if (this.state != PR.STATE.BENCH_LOADAFTER)
						{
							nelChipBench.initSitDown(this, true, true);
						}
						else
						{
							nelChipBench.initSitDown(this, false, false);
							this.recheck_emot = true;
						}
					}
					Bench.Pend("Bench-1");
					Bench.P("Bench-2");
					if (this.Onnie != null)
					{
						this.changeState(PR.STATE.BENCH_ONNIE);
					}
					else
					{
						flag = this.t_state >= 10f;
						if (UiBenchMenu.checkStackForceEvent(this, this.t_state >= 27f, !this.NM2D.GM.isActive()))
						{
							flag = false;
						}
					}
					Bench.Pend("Bench-2");
				}
			}
			else
			{
				if (init_flag)
				{
					this.EpCon.breath_key = "breath_e";
				}
				if (!this.hasD(M2MoverPr.DECL.INIT_A))
				{
					if (this.Mp.BCC != null && this.Mp.BCC.is_prepared)
					{
						this.need_check_bounds = false;
						this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, false);
						float num = this.Phy.tstacked_y + this.sizey;
						M2BlockColliderContainer.BCCLine bccline;
						this.Mp.BCC.isFallable(base.x, num - 0.5f, this.sizex, 3f, out bccline, true, true, -1f);
						if (bccline != null)
						{
							float num2 = X.Mn(bccline.slopeBottomY(base.x - this.sizex), bccline.slopeBottomY(base.x + this.sizex));
							this.Phy.addTranslateStack(0f, X.absMn(num2 - num, 0.8f));
						}
						NelChipBench nelChipBench = this.getNearBench(false, false);
						if (nelChipBench != null)
						{
							this.NM2D.CheckPoint.fineFoot(this, this.FootD.get_FootBCC(), true);
						}
					}
					else
					{
						this.Phy.addLockGravityFrame(1);
					}
				}
				flag = this.t_state >= 40f;
				if (flag && this.EggCon.isActive())
				{
					this.EggCon.forcePushout(false, true);
					flag = false;
				}
			}
			this.Skill.punch_decline_time = 20;
			if (flag)
			{
				if (IN.isMenuO(0) || IN.isMenuU() || this.NM2D.GM.isActive())
				{
					this.addD(M2MoverPr.DECL.FLAG0);
					this.remD((M2MoverPr.DECL)3145728);
				}
				else if (this.hasD(M2MoverPr.DECL.FLAG0))
				{
					this.remD(M2MoverPr.DECL.FLAG0);
				}
				else
				{
					if (base.isActionPD())
					{
						this.addD(M2MoverPr.DECL.FLAG_PD);
					}
					if (this.hasD(M2MoverPr.DECL.FLAG_PD) && (!Map2d.can_handle || !base.isActionO(0) || base.isActionO(12)))
					{
						this.addD(M2MoverPr.DECL.FLAG_PRESS);
					}
				}
			}
			else
			{
				this.remD((M2MoverPr.DECL)3145728);
			}
			if (this.hasD(M2MoverPr.DECL.FLAG_PRESS))
			{
				this.changeState(PR.STATE.NORMAL);
				this.recheck_emot = true;
				if (this.isBenchState(state))
				{
					this.Anm.setPose(this.poseIs("bench_must_orgasm_2") ? "must_orgasm_2" : "bench2stand", -1, false);
					UIBase.Instance.gameMenuSlide(false, false);
					UIBase.Instance.gameMenuBenchSlide(false, false);
					this.NM2D.FlagOpenGm.Rem("MASTURBATE");
				}
			}
			return true;
		}

		public void CureBench()
		{
			this.Ser.CureBench(0UL);
			this.GSaver.FineAll(true);
		}

		public int applyGasDamage(MistManager.MistKind Mist, float level01)
		{
			if (this.isBurstState())
			{
				return 0;
			}
			if (this.MistApply == null)
			{
				this.MistApply = new M2PrMistApplier(this);
			}
			return this.MistApply.applyGasDamage(Mist, level01);
		}

		public void applyGasDamage(MistAttackInfo Atk)
		{
			if (this.isBurstState())
			{
				return;
			}
			if (this.isDamagingOrKo() && !this.is_alive && ((this.Anm.poseIs(POSE_TYPE.DOWN, false) && !this.isSleepingDownState()) || this.isPoseCrouch(false)))
			{
				this.applyDamage(new NelAttackInfo(Atk), true);
				return;
			}
			this.Ser.applySerDamage(Atk.SerDmg, this.getSerApplyRatio(), -1);
			int num = X.Mn(this.hp - 1, Atk._hpdmg);
			if (num > 0)
			{
				bool flag;
				this.applyHpDamageSimple(Atk, out flag, num, "", true);
			}
			if (Atk._mpdmg > 0)
			{
				this.splitMpByDamage(Atk, Atk._mpdmg, MANA_HIT.EN, 0, 0f, null, true);
			}
		}

		public int applyWaterChokeDamage(bool init_flag, bool applying_damage = true)
		{
			if (!Map2d.can_handle)
			{
				return -1;
			}
			bool flag = base.forceCrouch(init_flag, true);
			float num;
			float num2;
			this.getMouthPosition(flag, false, false, false, out num, out num2, false);
			int config = this.Mp.getConfig((int)num, (int)num2);
			if (!CCON.isWater(config) && CCON.canStand(config) && this.canChokedByWaterState())
			{
				this.changeState(PR.STATE.WATER_CHOKED_RELEASE);
				return -2;
			}
			if (init_flag)
			{
				if (this.isAbsorbState())
				{
					if (applying_damage)
					{
						bool is_alive = this.is_alive;
						int num3 = X.IntC((float)this.maxhp * 0.1f);
						M2DmgCounterItem.DC dc;
						int num4;
						num3 = this.GSaver.applyHpDamage(num3, null, out dc, out num4);
						base.applyHpDamage(num3, true, null);
						this.GSaver.GsHp.Fine(true);
						float num5;
						float num6;
						this.getMouthPosition(out num5, out num6);
						base.PtcVar("mx", (double)num5).PtcVar("my", (double)num6);
						UIStatus.Instance.fineHpRatio(true, false);
						if (is_alive && !this.is_alive)
						{
							this.MistApply.showChokingLog();
							this.playVo("water_choked", false, false);
							base.PtcST("water_choked", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							this.setDamageCounter(-num3, 0, dc, null);
						}
						else if (this.is_alive || X.XORSP() < 0.04f)
						{
							if (this.is_alive)
							{
								this.MistApply.showChokingLog();
							}
							base.PtcST("water_choked_s", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							this.setDamageCounter(-num3, 0, dc, null);
						}
					}
					return -1;
				}
				if (applying_damage)
				{
					int num3 = (this.isPuzzleManagingMp() ? 5 : X.IntC(0.334f * (float)this.maxhp));
					M2DmgCounterItem.DC dc2;
					int num7;
					num3 = this.GSaver.applyHpDamage(num3, null, out dc2, out num7);
					base.applyHpDamage(num3, true, null);
					this.GSaver.GsHp.Fine(true);
					this.setDamageCounter(-num3, 0, dc2, null);
					UIStatus.Instance.fineHpRatio(true, false);
				}
				this.playVo("water_choked", false, false);
			}
			if (this.canChokedByWaterState())
			{
				bool flag2 = this.isSleepingDownState();
				this.changeState(PR.STATE.WATER_CHOKED);
				this.SpSetPose((base.forceCrouch(false, false) || flag2) ? "water_choked2_down" : "water_choked2", -1, null, false);
			}
			return 1;
		}

		public void changeWaterChokeRelease()
		{
			this.Ser.Cure(SER.WORM_TRAPPED);
			this.changeState(PR.STATE.WATER_CHOKED_RELEASE);
		}

		public bool attachYdrgListener(NelEnemy Parasiter, YdrgManager.FnYdrgApplyDamage Fn, float ratio, int thresh_lvl1 = -1, int thresh_lvl2 = -1)
		{
			if (ratio < 1000f)
			{
				if ((this.Ydrg == null || !this.Ydrg.alreadyAttached(Parasiter)) && X.XORSP() > ratio)
				{
					return false;
				}
				if (X.XORSP() > this.Skill.ser_apply_ratio)
				{
					return false;
				}
			}
			if (this.Ydrg == null)
			{
				this.Ydrg = new YdrgManager(this);
			}
			this.Ydrg.Attach(Parasiter, Fn, thresh_lvl1, thresh_lvl2);
			return true;
		}

		public int applyYdrgDamage(NelAttackInfo Atk)
		{
			if (base.water_drunk < 70)
			{
				base.water_drunk += 3;
				this.Ser.checkSer();
			}
			this.PadVib("dmg_ydrg", 1f);
			if (!this.is_alive && X.XORSP() < 0.38f)
			{
				return this.applyDamage(Atk, true);
			}
			this.UP.applyYdrgDamage();
			bool is_alive = this.is_alive;
			bool flag;
			int num = this.applyHpDamageSimple(Atk, out flag, Atk._hpdmg, "", true);
			this.BetoMng.Check(this, BetoInfo.Ydrg, false, true);
			if (is_alive && !this.is_alive && !this.isDamagingOrKo())
			{
				this.initDeathStasis(false);
			}
			this.recheck_emot = true;
			return num;
		}

		public void initDeathStasis(bool do_initdeath = false)
		{
			if (do_initdeath)
			{
				this.initDeath();
			}
			this.SpSetPose((this.isPoseCrouch(false) || this.isPoseDown(false)) ? "sleep" : "stand2fainted", -1, null, false);
			this.changeState(PR.STATE.DOWN_STUN);
			this.recheck_emot = true;
		}

		public float getWindApplyLevel(WindItem Wind)
		{
			if (this.Phy.isin_water || this.Skill.isShieldOpeningOnNormal(false, false) || this.isTrappedState() || this.isBurstState() || this.AbsorbCon.use_torture)
			{
				return 0f;
			}
			this.wind_apply_t = 40f;
			if (this.isMagicExistState(this.state))
			{
				if (base.hasFoot() && this.Phy.walk_xspeed == 0f)
				{
					if (this.is_crouch)
					{
						return 0f;
					}
					if (!this.Skill.isBusyTime(true, false, true, false))
					{
						return 0.15f;
					}
				}
				if (Wind.last_forever)
				{
					return 1f;
				}
				if (this.Skill.magic_chanting)
				{
					if (!base.hasFoot())
					{
						return 0.88f;
					}
					return 0.68f;
				}
				else
				{
					if (!base.hasFoot())
					{
						return 0.66f;
					}
					return 0.5f;
				}
			}
			else
			{
				if (Wind.last_forever)
				{
					return 1f;
				}
				return (base.hasFoot() ? (this.isPoseDown(false) ? 0.4f : (this.isPoseCrouch(false) ? 0.5f : 0.7f)) : 1f) * (this.AbsorbCon.isActive() ? 0.2f : 1f);
			}
		}

		public void applyWindFoc(WindItem Wind, float vx, float vy)
		{
			if (this.FootD.FootIsLadder())
			{
				this.addEnemySink(1f, false, 0f);
				return;
			}
			if (!Wind.last_forever)
			{
				vx *= 0.5f;
				vy *= 0.3f;
			}
			this.Phy.addFoc(FOCTYPE.KNOCKBACK, vx, 0f, -4f, 0, 8, 0, -1, 0);
			if (vy != 0f && !base.hasFoot())
			{
				if (vy < 0f && this.Phy.current_gravity_scale != 0f)
				{
					vy *= this.Phy.current_gravity_scale;
				}
				this.Phy.addFoc(FOCTYPE.KNOCKBACK, 0f, vy, -4f, 0, 8, 0, -1, 0);
			}
		}

		public override void addKnockbackVelocity(float v0, AttackInfo Atk, M2Attackable Another, FOCTYPE _foctype_add = (FOCTYPE)0U)
		{
			if (this.isUiPressDamage())
			{
				return;
			}
			base.addKnockbackVelocity(v0, Atk, Another, _foctype_add | this.Skill.foc_cliff_stopper);
		}

		public bool applying_wind
		{
			get
			{
				return this.wind_apply_t > 0f;
			}
		}

		public void applyWormTrapDamage(NelAttackInfo Atk, int phase_count, bool decline_additional_effect)
		{
			if (phase_count == 0)
			{
				this.Skill.killHoldMagic(false);
			}
			bool flag = true;
			this.UP.applyDamage(MGATTR.WORM, (float)(-1 + X.xors(6)), (float)(4 + X.xors(8)), UIPictureBase.EMSTATE.NORMAL, decline_additional_effect, null, false);
			if (Atk != null)
			{
				this.dmgBlinkSimple(MGATTR.WORM, X.NIXP(6f, 28f));
				if (Atk._hpdmg > 0)
				{
					this.applyDamage(Atk, true, "", false, false);
					flag = false;
				}
				else
				{
					this.applyDamageAddition(Atk);
					this.splitMpByDamage(Atk, Atk._mpdmg, (MANA_HIT)44, 3, 0.25f, null, true);
				}
			}
			else
			{
				X.XORSP();
				if (X.XORSP() < 0.8f)
				{
					this.dmgBlinkSimple(MGATTR.WORM, X.NIXP(6f, 28f));
				}
				if (X.XORSP() < 0.4f)
				{
					this.PtcHit(MGATTR.WORM, base.x, base.y).PtcSTDead("hits", false);
				}
				this.NM2D.Mana.AddMulti(base.x, base.y, X.NIXP(4f, 8f) * 4f, (MANA_HIT)44);
			}
			if (X.XORSP() < 0.33f)
			{
				this.TeCon.setEnlargeBouncy(1.08f, 1.08f, X.NIXP(13f, 26f), 0);
			}
			if (this.NM2D.GameOver != null)
			{
				this.NM2D.GameOver.worm_damaged = true;
			}
			if (flag)
			{
				string text = ((phase_count < 0) ? "es" : ((phase_count == 0) ? "mouth_fin" : ((X.XORSP() < X.MulOrScr(0.125f, 2f - (float)CFG.sp_epdmg_vo_mouth * 0.01f)) ? "es" : "mouth")));
				if (this.EpCon.isOrgasmInitTime())
				{
					text = "";
				}
				else if (!this.EpCon.isOrgasmInitTime())
				{
					if (Atk != null && Atk.EpDmg != null && this.vo_near_orgasm_replace())
					{
						text = this.vo_near_orgasm_2_must_replace();
					}
					else if (this.EpCon.isOrgasm() && (text == "es" || (text == "mouth" && X.XORSP() < 0.6f)))
					{
						text = "after_orgasm";
					}
				}
				if (!TX.noe(text))
				{
					this.playVo(text, false, false);
				}
			}
		}

		public override bool applyPressDamage(IPresserBehaviour Press, int aim, out bool stop_carrier)
		{
			bool flag = CAim._XD(aim, 1) != 0;
			stop_carrier = false;
			if (!flag && this.bounds_ != M2MoverPr.BOUNDS_TYPE.PRESSCROUCH)
			{
				if (!this.isDamagingOrKo() && !this.isSinkState(this.state) && !this.isGaraakiState(this.state))
				{
					this.changeState(PR.STATE.ENEMY_SINK);
				}
				this.setBounds(M2MoverPr.BOUNDS_TYPE.PRESSCROUCH, false, true);
				if (this.PressCheck != null && this.PressCheck.addForceCrouchDelay(50f))
				{
					base.forceCrouch(true, false);
				}
				return false;
			}
			stop_carrier = true;
			NelAttackInfo atkPressDamage = MDAT.getAtkPressDamage(Press, this, (AIM)aim);
			this.applyPressDamage(atkPressDamage, true, aim);
			stop_carrier = atkPressDamage._apply_knockback_current;
			return true;
		}

		public int applyPressDamage(NelAttackInfo Atk, bool force, int aim)
		{
			bool flag = Atk.Caster is NelEnemy;
			bool flag2 = false;
			bool flag3;
			if (flag)
			{
				if (CFG.sp_use_uipic_press_enemy > 0 && X.xors(100) < (int)CFG.sp_use_uipic_press_enemy)
				{
					if (aim == 3)
					{
						flag2 = this.Phy.hasFoot();
					}
					else
					{
						flag2 = !base.canGoToSide((AIM)aim, 0.3f, -0.15f, false, true, false);
					}
				}
				flag3 = flag2;
				if (!flag2)
				{
					return -2000;
				}
			}
			else
			{
				flag2 = true;
				flag3 = CFG.sp_use_uipic_press_gimmick;
			}
			float absorb_replace_prob = Atk.absorb_replace_prob;
			float absorb_replace_prob_ondamage = Atk.absorb_replace_prob_ondamage;
			int nodamage_time = Atk.nodamage_time;
			if (flag2)
			{
				Atk.absorb_replace_prob_both = 0f;
				Atk.nodamage_time = (flag ? 140 : 0);
			}
			int num = this.applyDamage(Atk, force, flag3 ? null : "", false, true);
			if (num > 0)
			{
				int num2 = CAim._XD(aim, 1);
				int num3 = CAim._YD(aim, 1);
				if (flag2)
				{
					if (num2 != 0)
					{
						this.setAim((num2 < 0) ? AIM.R : AIM.L, false);
						this.TeCon.setQuakeSinV(9f, 120, 82f, 1f, 0);
						this.TeCon.setQuakeSinH(3f, 130, 59f, 1f, 0);
					}
					else
					{
						this.TeCon.setQuakeSinH(9f, 120, 82f, 1f, 0);
						this.TeCon.setQuakeSinV(4f, 130, 59f, 1f, 0);
					}
					this.TeCon.setQuake(4f, 20, 1.7f, 0);
					this.Phy.killSpeedForce(true, true, true, false, false);
					this.changeState((num2 == 0) ? PR.STATE.DAMAGE_PRESS_TB : PR.STATE.DAMAGE_PRESS_LR);
					this.Phy.addLockMoverHitting(HITLOCK.PRESSER, 180f);
					if (this.bounds_ != M2MoverPr.BOUNDS_TYPE.PRESSCROUCH)
					{
						float sizex = this.sizex;
						float sizey = this.sizey;
						this.setBounds(M2MoverPr.BOUNDS_TYPE.PRESSCROUCH, false, true);
						this.Phy.addTranslateStack((float)num2 * (sizex - this.sizex), (float)(-(float)num3) * (sizey - this.sizey));
					}
					this.offset_pixel_x = 0f;
					this.offset_pixel_y = ((this.state == PR.STATE.DAMAGE_PRESS_LR) ? 0f : this.presscrouch_offset_pixel_y);
					if (flag3)
					{
						this.addD(M2MoverPr.DECL.INIT_A);
						this.offset_pixel_x += (float)(num2 * 20);
						this.offset_pixel_y += (float)(num3 * 10);
					}
					else
					{
						this.Mp.DropCon.setBlood(this, 57, MTR.col_blood, (float)(-(float)num2) * 0.08f, true);
					}
					base.PtcVar("hit_x", (double)Atk.hit_x).PtcVar("hit_y", (double)Atk.hit_y).PtcVar("agR", (double)(CAim.get_agR((AIM)aim, 0f) + 1.5707964f))
						.PtcVar("aim", (double)aim);
					base.PtcVar("ui_press", (double)(flag3 ? 1 : 0)).PtcST("press_damage_pr", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					if (flag3)
					{
						int num4 = X.xors(8) | ((X.sensitive_level >= 2) ? 1 : 0);
						UIPictureBase.EMSTATE emstate = (((num4 & 1) != 0) ? UIPictureBase.EMSTATE.PROG0 : UIPictureBase.EMSTATE.NORMAL) | (((num4 & 2) != 0) ? UIPictureBase.EMSTATE.PROG1 : UIPictureBase.EMSTATE.NORMAL) | (((num4 & 4) != 0) ? UIPictureBase.EMSTATE.PROG2 : UIPictureBase.EMSTATE.NORMAL);
						string text = ((num2 == 0) ? "damage_press_t" : "damage_press");
						if (CFG.sp_use_uipic_press_balance < 100 && text == "damage_press_t")
						{
							if (X.xors(100) >= (int)CFG.sp_use_uipic_press_balance)
							{
								text = "damage_press";
							}
						}
						else if (CFG.sp_use_uipic_press_balance > 100 && text == "damage_press" && X.xors(100) < (int)(CFG.sp_use_uipic_press_balance - 100))
						{
							text = "damage_press_t";
						}
						this.UP.setFade(text, emstate, true, true, false);
						this.UP.applyUiPressDamage();
						Atk._apply_knockback_current = false;
						this.Anm.setPose((this.state == PR.STATE.DAMAGE_PRESS_TB) ? "ui_press_damage_down" : "ui_press_damage", -1, false);
						bool flag4 = true;
						if (num2 == 0)
						{
							this.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(this.aim, 1), (float)num3, false), -1, false);
						}
						int num5;
						if (text == "damage_press_t")
						{
							if ((emstate & UIPictureBase.EMSTATE.PROG2) != UIPictureBase.EMSTATE.NORMAL)
							{
								num5 = 10;
							}
							else if ((emstate & UIPictureBase.EMSTATE.PROG1) != UIPictureBase.EMSTATE.NORMAL)
							{
								num5 = (((emstate & UIPictureBase.EMSTATE.PROG0) != UIPictureBase.EMSTATE.NORMAL) ? 9 : 8);
								flag4 = false;
							}
							else
							{
								num5 = (((emstate & UIPictureBase.EMSTATE.PROG0) != UIPictureBase.EMSTATE.NORMAL) ? 4 : 0);
							}
						}
						else if ((emstate & (UIPictureBase.EMSTATE.PROG1 | UIPictureBase.EMSTATE.PROG2)) == UIPictureBase.EMSTATE.PROG2 || (emstate & (UIPictureBase.EMSTATE.PROG1 | UIPictureBase.EMSTATE.PROG2)) == UIPictureBase.EMSTATE.PROG1)
						{
							num5 = (((emstate & UIPictureBase.EMSTATE.PROG0) != UIPictureBase.EMSTATE.NORMAL) ? 9 : 8);
							flag4 = false;
						}
						else if ((emstate & UIPictureBase.EMSTATE.PROG0) != UIPictureBase.EMSTATE.NORMAL)
						{
							num5 = 4;
						}
						else
						{
							num5 = 0;
						}
						if (flag4 && this.UP.isActive())
						{
							UIPictureBodySpine uipictureBodySpine = this.UP.getBodyData() as UIPictureBodySpine;
							if (uipictureBodySpine != null)
							{
								SpineViewerNel viewer = uipictureBodySpine.getViewer();
								if (viewer.hasAnim("f_m4"))
								{
									num5 += 3;
								}
								else if (viewer.hasAnim("f_m3"))
								{
									num5 += 2;
								}
								else if (viewer.hasAnim("f_m1"))
								{
									num5++;
								}
							}
						}
						this.Anm.animReset(num5);
					}
				}
			}
			if (flag2)
			{
				Atk.absorb_replace_prob = absorb_replace_prob;
				Atk.absorb_replace_prob_ondamage = absorb_replace_prob_ondamage;
				Atk.nodamage_time = nodamage_time;
			}
			return num;
		}

		public override int checkStuckInWall(ref M2BlockColliderContainer.BCCLine PreStuckBcc, bool extending = false)
		{
			if (this.AbsorbCon.isActive())
			{
				return -1;
			}
			return base.checkStuckInWall(ref PreStuckBcc, extending);
		}

		public override bool stuckExtractFailure(Rect RcMap)
		{
			if (this.AbsorbCon.isActive())
			{
				return false;
			}
			if (EnemySummoner.isActiveBorder())
			{
				M2LpSummon summonedArea = EnemySummoner.ActiveScript.getSummonedArea();
				if (summonedArea != null)
				{
					RcMap = new Rect((float)summonedArea.mapx, (float)summonedArea.mapy, (float)summonedArea.mapw, (float)summonedArea.maph);
				}
			}
			return base.stuckExtractFailure(RcMap);
		}

		public void applyAbsorbDamage(NelAttackInfo Atk, bool execute_attack = true, bool mouth_damage = false, string fade_key = null, bool decline_additional_effect = false)
		{
			if (!this.canApplyAbrosb() || X.DEBUGNODAMAGE)
			{
				return;
			}
			bool flag = execute_attack && X.XORSP() < 0.15f;
			if (mouth_damage)
			{
				if (CFG.sp_epdmg_vo_mouth < 100 && (int)CFG.sp_epdmg_vo_mouth <= X.xors(100))
				{
					mouth_damage = false;
				}
			}
			else if (CFG.sp_epdmg_vo_mouth > 100 && (int)(CFG.sp_epdmg_vo_mouth - 100) >= X.xors(100))
			{
				mouth_damage = true;
			}
			float num = 0.88f;
			float num2 = 0f;
			if (mouth_damage && CFG.sp_epdmg_vo_mouth > 100)
			{
				float num3 = (float)(CFG.sp_epdmg_vo_mouth - 100) * 0.01f;
				num = X.Scr(num, num3);
				num2 = -num3 * 0.4f;
			}
			string text;
			if (mouth_damage && X.XORSP() < num)
			{
				text = (flag ? "mouthl" : "mouth");
			}
			else
			{
				text = (flag ? "el" : "es");
			}
			if (Atk.Caster as NelEnemy != null)
			{
				this.PeeLockReduceCheck(0.025f);
			}
			this.BetoMng.Check(this, Atk, false, true);
			if (!this.AbsorbCon.no_shuffleframe_on_applydamage && X.XORSP() < 0.63f)
			{
				this.Anm.randomizeFrame();
				this.TeCon.setQuake(2f, 7, 1f, 4);
			}
			if (X.XORSP() < 0.23f)
			{
				base.M2D.Cam.setQuake(2f, 7, 1f, 0);
			}
			if (X.XORSP() < 0.6f)
			{
				this.PadVib(execute_attack ? "dmg_absorb" : "dmg_absorb_s", X.NIXP(0.4f, 1f));
			}
			float num4 = 0f;
			if (execute_attack)
			{
				int ep = this.ep;
				if (Atk._hpdmg > 0)
				{
					bool flag2;
					this.applyHpDamageSimple(Atk, out flag2, -1, "", true);
				}
				else
				{
					this.applyDamageAddition(Atk);
				}
				if (ep < this.ep && this.vo_near_orgasm_replace())
				{
					text = this.vo_near_orgasm_2_must_replace();
				}
				num4 = this.splitMpByDamage(Atk, Atk._mpdmg, (MANA_HIT)1038, 0, 1f, null, true);
				PostEffect.IT.setPEabsorbed(POSTM.MP_ABSORBED, 4f, X.NIXP(0.6f, 0.9f), -2);
				if (Atk.attr == MGATTR.FIRE && X.XORSP() < 0.33f)
				{
					Vector3 hipPos = this.getHipPos();
					base.PtcVarS("attr", FEnum<MGATTR>.ToStr(Atk.attr)).PtcVar("x", (double)hipPos.x).PtcVar("y", (double)hipPos.y)
						.PtcVar("hit_x", (double)hipPos.x)
						.PtcVar("hit_y", (double)hipPos.y)
						.PtcST("hits", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			this.GSaver.LockTime(Atk);
			if (num4 > 0f)
			{
				this.Ser.checkSer();
				this.TeSetDmgBlink(Atk.attr, 10f, 0.9f, 0f, 0);
				Atk.playEffect(this);
			}
			else
			{
				this.TeSetDmgBlink(Atk.attr, (float)((!this.is_alive && this.overkill && X.XORSP() < 0.3f) ? 12 : 4), 0.9f, 0f, 0);
			}
			if (!this.EpCon.isOrgasmInitTime())
			{
				if (this.EpCon.isOrgasm() && (text == "es" || (text == "mouth" && X.XORSP() < 0.6f + num2)))
				{
					text = "after_orgasm";
				}
				if (text != null)
				{
					this.playVo(text, false, false);
				}
			}
			if (TX.noe(fade_key))
			{
				fade_key = this.AbsorbCon.uipicture_fade_key;
			}
			if (TX.noe(fade_key) && this.isWormTrapped())
			{
				fade_key = "insected";
			}
			if (this.AbsorbCon.current_pose_priority == 1 && X.XORSP() < 0.75f)
			{
				string text2 = this.absorb_default_pose(X.XORSP() < (this.is_alive ? 0.012f : 0.03f) * this.absorb_pose_change_ratio);
				this.SpSetPose(text2, -1, null, false);
				if (text2 == "absorb2absorb_crouch" && TX.noe(fade_key))
				{
					fade_key = "crouch";
				}
			}
			if (Atk.setable_UP)
			{
				this.UP.applyDamage(Atk.attr, (float)(4 + X.xors(4)) * (execute_attack ? 1f : 0.5f), 0f, UIPictureBase.EMSTATE.NORMAL, decline_additional_effect, fade_key, false);
			}
		}

		public TransEffecterItem TeSetDmgBlink(MGATTR attr, float maxt = 0f, float mul_ratio = 1f, float add_ratio = 0f, int _saf = 0)
		{
			maxt *= (float)CFG.sp_dmgte_pixel_duration * 0.01f;
			mul_ratio *= (float)CFG.sp_dmgte_pixel_density * 0.01f;
			add_ratio *= (float)CFG.sp_dmgte_pixel_density * 0.01f;
			return this.TeCon.setDmgBlink(attr, maxt, mul_ratio, add_ratio, _saf);
		}

		public bool vo_near_orgasm_replace()
		{
			float sp_voice_for_pleasure = CFG.sp_voice_for_pleasure;
			bool flag;
			if (sp_voice_for_pleasure == 0f)
			{
				flag = false;
			}
			else if (sp_voice_for_pleasure <= 1f && (float)this.EpCon.getOrgasmedIndividualTotal() < 5f * (1f - X.ZLINE((float)CFG.sp_epdmg_vo_iku * 0.01f - 0.25f, 0.75f)))
			{
				flag = false;
			}
			else
			{
				float num = 0.85f;
				float num2 = 0.6f;
				if (sp_voice_for_pleasure > 1f)
				{
					num = X.NI(num, 0.3f, sp_voice_for_pleasure - 1f);
					num2 = X.NI(num2, 1f, sp_voice_for_pleasure - 1f);
				}
				else
				{
					num2 *= sp_voice_for_pleasure;
				}
				flag = (float)this.ep >= 1000f * num && X.XORSP() < num2;
			}
			return flag;
		}

		private bool getMouthVoiceReplace(float base_ratio = 0.7f, float overwrite_sp_epdmg_vo_mouth_ratio = 1f)
		{
			bool flag = this.AbsorbCon.isActive() && this.AbsorbCon.mouth_is_covered && X.XORSP() < base_ratio * X.ZLINE((float)CFG.sp_epdmg_vo_mouth, 100f);
			if (!flag && CFG.sp_epdmg_vo_mouth > 100 && (this.isAbsorbState() || this.isWormTrapped()))
			{
				flag = (float)X.xors(100) < (float)(CFG.sp_epdmg_vo_mouth - 100) * overwrite_sp_epdmg_vo_mouth_ratio;
			}
			return flag;
		}

		public string vo_near_orgasm_2_must_replace()
		{
			string text;
			if (CFG.sp_voice_for_pleasure2m > 0f)
			{
				text = "near_orgasm";
			}
			else
			{
				float num = (float)this.ep / 1000f;
				if (this.getMouthVoiceReplace(CFG.sp_voice_for_pleasure2m, 0.6f + X.ZLINE(num - 0.5f, 0.5f) * 0.3f))
				{
					if ((float)X.xors(100) >= (float)CFG.sp_epdmg_vo_iku * 0.25f)
					{
						return "must_mouth";
					}
					return "near_orgasm_iku";
				}
				else
				{
					float num2 = X.Scr(CFG.sp_voice_for_pleasure2m, num * 0.125f);
					text = "near_orgasm";
					if (X.XORSP() < num2)
					{
						if (num >= 0.9f)
						{
							text = "must_come";
						}
						else if (num >= 0.75f)
						{
							text = "mustll";
						}
						else if (num >= 0.5f)
						{
							text = "mustl";
						}
						else
						{
							text = "must";
						}
					}
				}
			}
			if (text == "near_orgasm" && CFG.sp_epdmg_vo_iku > 0 && X.xors(100) < (int)CFG.sp_epdmg_vo_iku)
			{
				text = "near_orgasm_iku";
			}
			return text;
		}

		public float splitMpByDamage(NelAttackInfoBase Atk, int red_val = 0, MANA_HIT split_mana_type = MANA_HIT.EN, int counter_saf = 0, float crack_ratio = 1f, string fade_key = null, bool calc_gsaver = false)
		{
			float num2;
			float num = this.splitMpByDamage(out num2, Atk, red_val, split_mana_type, counter_saf, crack_ratio, fade_key, calc_gsaver);
			this.GaugeBrk.check(num2);
			return num;
		}

		public float splitMpByDamage(out float gauge_break, NelAttackInfoBase Atk, int red_val = 0, MANA_HIT split_mana_type = MANA_HIT.EN, int counter_saf = 0, float crack_ratio = 1f, string fade_key = null, bool calc_gsaver = false)
		{
			gauge_break = 0f;
			if (this.isPuzzleManagingMp() || X.DEBUGNODAMAGE)
			{
				return 0f;
			}
			int num = ((MDAT.canApplySplitMpDamage(Atk, this) && !X.DEBUGNODAMAGE) ? Atk.split_mpdmg : 0);
			bool flag = false;
			float num2 = 0f;
			if (num + red_val > 0)
			{
				float num3 = (float)(red_val + MDAT.getMpDamageValue(Atk, this, (float)num)) * this.NM2D.NightCon.SpilitMpRatioPr();
				float num4 = 1f;
				if (this.AbsorbCon.isActive())
				{
					float re = this.getRE(RecipeManager.RPI_EFFECT.ARREST_MPDAMAGE_REDUCE);
					if (re >= 0f)
					{
						num4 = X.NI(1f, 0.25f, re);
					}
					else
					{
						num4 = X.NI(1f, 2f, -re);
					}
				}
				int num5 = X.IntR(num3 * num4);
				num2 = this.Skill.splitMpByDamage(out gauge_break, Atk, ref num5, split_mana_type, counter_saf, crack_ratio, true);
				this.setDamageCounter(0, -num5, M2DmgCounterItem.DC.NORMAL, Atk.CurrentAbsorbedBy);
				flag = flag || ((num2 > 0f || this.mp == 0) && num > 0);
			}
			if (base.water_drunk >= 72)
			{
				flag = true;
			}
			if (flag)
			{
				this.checkNoelJuice((float)(this.isAbsorbState() ? (this.is_alive ? 4 : 1) : 3), false, true, -1);
			}
			return num2;
		}

		public float lost_mp_in_chanting_ratio
		{
			get
			{
				MagicItem curMagic = this.Skill.getCurMagic();
				if (curMagic == null || !curMagic.isPreparingCircle)
				{
					return 1f;
				}
				return X.Mx(0f, X.NI(1f, 0f, this.getRE(RecipeManager.RPI_EFFECT.LOST_MP_WHEN_CHANTING) * X.ZLINE((float)this.Skill.getHoldingMp(true), (float)curMagic.reduce_mp * 0.3f)));
			}
		}

		public int applyDamage(NelAttackInfo Atk, bool force)
		{
			return this.applyDamage(Atk, force, "", false, false);
		}

		private PR PtcHit(NelAttackInfo Atk)
		{
			return this.PtcHit(Atk.attr, Atk.hit_x, Atk.hit_y);
		}

		private PR PtcHit(MGATTR attr, float hit_x, float hit_y)
		{
			string text = ((attr == MGATTR.STAB || attr == MGATTR.BITE || attr == MGATTR.CUT_H) ? "hitl_stab" : "hitl");
			base.PtcVar("cx", (double)(this.drawx * this.Mp.rCLEN));
			base.PtcVar("cy", (double)(this.drawy * this.Mp.rCLEN));
			base.PtcVarS("attr", FEnum<MGATTR>.ToStr(attr));
			base.PtcVarS("snd_hitl", text);
			base.PtcVar("hit_x", (double)hit_x);
			base.PtcVar("hit_y", (double)hit_y);
			return this;
		}

		private void dmgBlinkSimple(MGATTR attr, float blink_t = 40f)
		{
			this.TeCon.setQuake(4f, 9, 1f, 0);
			this.TeSetDmgBlink(attr, blink_t, 0.9f, 0f, 0);
			if (X.XORSP() < 0.4f)
			{
				this.TeCon.setQuakeSinH(4f, 20, 1.7f, 0f, 0);
			}
		}

		public int applyDamage(NelAttackInfo Atk, bool force, string fade_key, bool decline_ui_additional_effect = false, bool from_press_damage = false)
		{
			if (!from_press_damage)
			{
				if (this.Skill.isParryable(Atk))
				{
					if (Atk.PublishMagic != null && Atk.PublishMagic.is_normal_attack && Atk.Caster is NelEnemy)
					{
						(Atk.Caster as NelEnemy).applyParry(this.aim);
					}
					PostEffect it = PostEffect.IT;
					it.setSlowFading(20f, 5f, 0f, -20);
					it.addTimeFixedEffect(it.setPEbounce(POSTM.ZOOM2, 24f, 0.03f, -5), 1f);
					float num = X.NI(Atk.hit_x, base.x, 0.63f) + base.mpf_is_right * 0.32f;
					float num2 = X.NI(Atk.hit_y, base.y, 0.4f);
					float num3 = this.Mp.GAR(0f, base.y, (num - base.x) * 5f, num2) + base.mpf_is_right * 1.5707964f;
					base.PtcVar("hx", (double)num);
					base.PtcVar("hy", (double)num2);
					base.PtcVar("hagR", (double)num3);
					this.PtcHld.PtcSTTimeFixed("hit_parry", 0.8f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
					this.RCenemy_sink.Lock(36f, true);
					if (Atk.PublishMagic != null)
					{
						if (Atk.PublishMagic.is_normal_attack)
						{
							if (Atk.parryable)
							{
								Atk.PublishMagic.kill(0f);
							}
						}
						else
						{
							this.Skill.reflectCheck(Atk.PublishMagic);
						}
					}
					return 0;
				}
				if (Atk.press_state_replace < 4)
				{
					int num4 = this.applyPressDamage(Atk, force, (int)Atk.press_state_replace);
					if (num4 != -2000)
					{
						return num4;
					}
				}
			}
			string text = fade_key;
			Atk.playEffect(this);
			int hpdmg = Atk._hpdmg;
			bool flag = true;
			bool flag2 = false;
			int num5;
			float num6;
			if (X.DEBUGWEAK)
			{
				num5 = 9999;
				num6 = 9999f;
			}
			else
			{
				if (Atk.isPenetrateAttr(this))
				{
					flag2 = true;
				}
				if (Atk.fix_damage && !this.NoDamage.isActive(Atk.ndmg))
				{
					num6 = (float)(this.can_applydamage_state() ? 1 : 0);
					num5 = (int)((float)hpdmg * num6);
				}
				else
				{
					num6 = this.applyHpDamageRatio(Atk);
					num5 = (int)((float)hpdmg * num6);
				}
			}
			if (hpdmg <= 0)
			{
				num5 = 0;
				if (Atk.ndmg == NDMG.GRAB_PENETRATE || (num6 > 0f && (Atk.EpDmg != null || Atk.SerDmg != null) && (Atk._mpdmg > 0 || (Atk.split_mpdmg > 0 && MDAT.canApplySplitMpDamage(Atk, this)))))
				{
					force = true;
					flag = false;
				}
				else
				{
					num6 = 0f;
				}
			}
			if (num6 == 0f && (!force || !this.can_applydamage_state()))
			{
				if (!flag2)
				{
					Atk._hitlock_ignore = true;
				}
				return 0;
			}
			if (Atk.shield_break_ratio != 0f)
			{
				M2Shield.RESULT result = this.Skill.checkShield(Atk, (Atk.shield_break_ratio < 0f) ? (-Atk.shield_break_ratio) : ((float)num5 * Atk.shield_break_ratio));
				if (this.applyShieldResult(result, Atk, true))
				{
					return 0;
				}
			}
			bool flag3 = (from_press_damage || Atk.isPenetrateAbsorb()) && this.AbsorbCon.isActive() && (!(Atk.Caster is NelM2Attacker) || !this.AbsorbCon.hasPublisher(Atk.Caster as NelM2Attacker));
			if (!from_press_damage)
			{
				float num7 = (this.canApplyAbrosb() ? ((this.isGaraakiState() || (this.isDamagingOrKo() && this.is_alive && this.getCastableMp() / (float)this.maxmp >= 0.15f)) ? Atk.absorb_replace_prob_ondamage : Atk.absorb_replace_prob) : 0f);
				if (num7 > 0f && (this.isAbsorbState(this.state) || X.XORSP() < num7 + this.absorb_additional_dying) && this.initAbsorb(Atk, Atk.Caster as NelM2Attacker, null, flag3))
				{
					base.PtcVarS("attr", FEnum<MGATTR>.ToStr(Atk.attr));
					base.PtcVar("hit_x", (double)Atk.hit_x);
					base.PtcVar("hit_y", (double)Atk.hit_y);
					base.PtcST("hitabsorb", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					return 0;
				}
			}
			this.Ser.checkDamageSpecial(ref num5, Atk);
			num5 = ((X.DEBUGNODAMAGE && X.DEBUGMIGHTY) ? 0 : MDAT.getPrDamageVal(num5, Atk, this));
			float num8 = 0f;
			float num9 = Atk.burst_vx;
			float num10 = Atk.burst_vy;
			bool is_alive = this.is_alive;
			bool flag4 = this.Ser.has(SER.SHIELD_BREAK);
			bool flag5;
			num5 = this.applyHpDamageSimple(Atk, out flag5, X.DEBUGNODAMAGE ? 0 : num5, "", flag);
			force = force || flag5;
			float num11 = 0f;
			float num12 = 0f;
			UIPictureBase.EMSTATE emstate = UIPictureBase.EMSTATE.NORMAL;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			if (num5 > 0 || force)
			{
				flag7 = true;
				this.Skill.abortSwaySliding();
				bool flag9 = is_alive && !this.is_alive;
				if (X.XORSP() < Atk.huttobi_ratio + 0.3f * (1f - X.ZLINE(base.hp_ratio - 0.17f, 0.44f)))
				{
					if (num10 >= 0f && !this.isFrozen())
					{
						num10 -= 0.03f + 0.03f * X.XORSP();
					}
					num9 += (float)X.MPF(num9 > 0f) * (0.01f + 0.05f * X.XORSP());
				}
				if (this.is_crouch && !this.is_alive)
				{
					num10 -= 0.1f;
				}
				if (!this.is_alive && !is_alive && !this.AbsorbCon.no_shuffle_aim && X.XORSP() < Atk.aim_to_opposite_when_pr_dieing)
				{
					this.aim = CAim.get_opposite(this.aim);
				}
				string text2 = "";
				bool flag10 = !flag4 && this.Ser.has(SER.SHIELD_BREAK);
				bool flag11 = false;
				string text3;
				if (this.isWormTrapped() && Atk.attr != MGATTR.WORM && X.XORSP() < 0.7f)
				{
					this.TeCon.clear();
					this.TeSetDmgBlink(Atk.attr, 60f, 0.9f, 0f, 0);
					this.executeReleaseFromTrapByDamage(true, true);
					this.NoDamage.Add(45f);
					text3 = ((X.XORSP() < 0.9f) ? "dmgl" : "dmgx");
					this.PtcHit(Atk).PtcSTDead("hitl", flag9);
					emstate |= UIPictureBase.EMSTATE.SMASH;
					num11 = (float)(-8 + X.xors(16));
					num12 = (float)(2 + X.xors(6));
					flag8 = true;
				}
				else if (flag10 || Atk.huttobi_ratio >= 100f || (num10 < Atk._burst_huttobi_thresh && Atk.huttobi_ratio > -100f))
				{
					if (!this.is_alive)
					{
						if (!is_alive)
						{
							num9 += X.NIXP(0.02f, 0.06f) * (float)X.MPF(num9 > 0f);
							text3 = ((X.XORSP() < 0.9f) ? "dmgl" : "death");
						}
						else
						{
							num9 += X.NI(0.05f, 0.08f, X.ZLINE(X.XORSP() - 0.5f, 0.5f)) * (float)X.MPF(num9 > 0f);
							text3 = "death";
						}
					}
					else
					{
						text3 = "dmgl";
					}
					flag11 = true;
					this.Ser.CureTime(SER.SLEEP, 80, true);
					if (flag3)
					{
						this.AbsorbCon.clear();
						this.TeCon.clear();
						this.TeSetDmgBlink(Atk.attr, 60f, 0.9f, 0f, 0);
						text3 = "dmgx";
						flag6 = true;
					}
					else if (!this.isAbsorbState(this.state))
					{
						this.quitCrouch(false, false, false);
						this.FootD.initJump(false, false, false);
						base.M2D.Cam.Qu.VibP(14f, 8f, 6f, 0).VibP(7f, 22f, 1f, 0);
						if (flag10 || (X.Abs(num9) < 0.15f && X.Abs(num10) > 0.08f))
						{
							text2 = "dmg_t";
							num10 = X.Mx(-0.27f, num10 - 0.17f);
							this.changeState(PR.STATE.DAMAGE_LT);
						}
						else if (base.hp_ratio < 0.15f && X.Abs(num9) >= 0.15f && X.XORSP() < 0.25f)
						{
							this.changeState(PR.STATE.DAMAGE_LT_KIRIMOMI);
							this.FootD.initJump(false, false, false);
							num10 -= 0.012f;
							if (this.is_alive)
							{
								num9 += (float)X.MPF(num9 > 0f) * 0.02f;
							}
						}
						else
						{
							if (num9 == 0f || num9 < 0f == base.is_right)
							{
								text2 = "dmg_hktb";
							}
							else
							{
								text2 = "dmg_hktb_b";
							}
							this.changeState(PR.STATE.DAMAGE_L);
						}
						this.TeSetDmgBlink(Atk.attr, 60f, 0.9f, 0f, 0);
						if (!this.is_alive)
						{
							this.absorb_additional_dying += 0.02f;
						}
						flag6 = true;
					}
					else
					{
						flag8 = true;
						base.M2D.Cam.Qu.VibP(6f, 3f, 3f, 0).VibP(3f, 5f, 1f, 0);
						this.TeSetDmgBlink(Atk.attr, 10f, 0.9f, 0f, 0);
						num10 = 0f;
						if (text3 == "dmgl" && (Atk.isAbsorbAttr() || (double)X.XORSP() < 0.4))
						{
							text3 = "el";
						}
						if (this.Anm.poseIs("downdamage_t", "down"))
						{
							text2 = "downdamage_t";
						}
						else if (this.AbsorbCon.current_pose_priority == 1)
						{
							text2 = this.absorb_default_pose(X.XORSP() < (0.25f + ((!this.is_alive) ? 0.25f : 0f)) * this.absorb_pose_change_ratio);
							if (text2 == "absorb2absorb_crouch" && TX.noe(fade_key))
							{
								fade_key = "crouch";
							}
						}
					}
					if (!this.AbsorbCon.no_clamp_speed)
					{
						this.Phy.clampSpeed(FOCTYPE._RELEASE, 0.06f, 0.06f, 1f);
					}
					if (Atk.attr == MGATTR.THUNDER)
					{
						if (text2 == "dmg_hktb" || text2 == "dmg_hktb_b" || text2 == "downdamage_t" || text2 == "dmg_t")
						{
							text2 = "damage_thunder";
						}
						if (Atk.ndmg == NDMG.MAPDAMAGE_THUNDER)
						{
							PostEffect.IT.setSlow(33f, 0f, 0);
						}
						else
						{
							PostEffect.IT.setSlow(15f, 0.25f, 0);
						}
						PostEffect.IT.addTimeFixedEffect(PostEffect.IT.setPEfadeinout(POSTM.THUNDER_TRAP, 40f, 10f, 1f, -30), 1f);
						PostEffect.IT.addTimeFixedEffect(this.Anm, 1f);
						PostEffect.IT.addTimeFixedEffect(this.TeCon.setQuakeSinH(7f, 28, 18.6f, 2f, 0), 0.33f);
						PostEffect.IT.addTimeFixedEffect(this.TeCon.setQuake(7f, 3, 4f, 0), 0.33f);
						text = (fade_key = "damage_thunder_big");
					}
					if (Atk.attr == MGATTR.THUNDER && (double)X.XORSP() < 0.7)
					{
						text3 = "dmg_elec";
					}
					this.PtcHit(Atk).PtcSTDead("hitl", flag9);
					emstate |= UIPictureBase.EMSTATE.SMASH;
					num11 = (float)(-5 + X.xors(14));
					num12 = (float)(3 + X.xors(15));
					this.FootD.initJump(false, false, false);
					if (num9 != 0f || num10 != 0f)
					{
						if (X.Abs(num9) > 0.1f && X.XORSP() < 0.1f && !base.canGoToSide((num9 > 0f) ? AIM.R : AIM.L, 0.55f, -0.1f, false, false, false))
						{
							num9 *= -X.NIXP(0.4f, 0.7f);
						}
						this.Phy.remFoc(FOCTYPE.JUMP, true);
						this.Phy.addLockGravityFrame(1);
						this.Phy.addFoc(FOCTYPE.HIT | FOCTYPE._CHECK_WALL, X.absMn(num9 * 2.5f, 0.3f), num10 - 0.1f, -1f, -1, 1, 0, -1, 0);
						this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._CHECK_WALL, num9, num10, -1f, 0, 50, 90, 7, 8);
					}
				}
				else
				{
					if (!this.is_alive)
					{
						if (is_alive)
						{
							text3 = "el";
						}
						else
						{
							float num13 = X.XORSP();
							text3 = ((num13 < 0.14f) ? "dmgs" : ((num13 < 0.92f) ? "es" : "el"));
						}
					}
					else
					{
						text3 = "dmgs";
					}
					bool flag12 = base.hasFoot();
					bool flag13 = flag12 || ((Atk._burst_huttobi_thresh < 0f || Atk.huttobi_ratio <= -100f) && num10 < 0f);
					if (flag3)
					{
						this.AbsorbCon.clear();
						if (!this.AbsorbCon.no_clamp_speed)
						{
							this.Phy.clampSpeed(FOCTYPE._RELEASE, 0.12f, 0.13f, 1f);
						}
						this.TeCon.clear();
						this.TeSetDmgBlink(Atk.attr, 60f, 0.9f, 0f, 0);
						text3 = "el";
					}
					else if (!this.isAbsorbState(this.state))
					{
						this.Phy.clampSpeed(FOCTYPE._RELEASE, 0.12f, 0.13f, 1f);
						text2 = this.setPoseNokezori(num9 == 0f || num9 < 0f == base.is_right == X.XORSP() < 0.78f, X.XORSP() < (float)X.Mn(1, num5 / X.Mx(1, this.hp)) * 0.6f, false);
						this.changeState(PR.STATE.DAMAGE);
						base.M2D.Cam.Qu.VibP(6f, 19f, 3f, 0);
						this.dmgBlinkSimple(Atk.attr, 40f);
						if (!this.isWormTrapped() && this.Ser.has(SER.BURNED))
						{
							num10 = X.Mn(num10, -0.16f);
							flag13 = false;
						}
						else if (!this.canJump())
						{
							num10 -= ((num5 == 0) ? 0.02f : 0.2f);
						}
						if (!this.is_alive)
						{
							this.absorb_additional_dying += 0.02f;
						}
					}
					else
					{
						flag8 = true;
						if (!this.AbsorbCon.no_clamp_speed)
						{
							this.Phy.clampSpeed(FOCTYPE._RELEASE, 0.06f, 0.06f, 1f);
						}
						if (text3 == "dmgs" && (Atk.isAbsorbAttr() || (double)X.XORSP() < 0.4))
						{
							text3 = "es";
						}
						num10 = 0f;
						num9 *= 0.5f;
						base.M2D.Cam.Qu.VibP(3f, 5f, 1f, 0);
						float num14 = X.XORSP();
						if (num14 < 0.4f)
						{
							this.TeCon.setQuake(4f, 9, 1f, 0);
						}
						else if (num14 < 0.6f)
						{
							this.TeCon.setQuakeSinH(7f, 20, X.NIXP(13.3f, 24f), 0f, 0);
						}
						else
						{
							this.TeSetDmgBlink(Atk.attr, 6f, 0.9f, 0f, 0);
						}
						if (this.Anm.poseIs("downdamage_t", "down"))
						{
							text2 = "downdamage_t";
						}
						else if (this.AbsorbCon.current_pose_priority == 1)
						{
							text2 = this.absorb_default_pose(X.XORSP() < (0.11f + ((!this.is_alive) ? 0.11f : 0f)) * this.absorb_pose_change_ratio);
							if (text2 == "absorb2absorb_crouch" && TX.noe(fade_key))
							{
								fade_key = "crouch";
							}
						}
					}
					this.PtcHit(Atk).PtcSTDead("hits", flag9);
					num11 = (float)(-4 + X.xors(9));
					num12 = (float)(-2 + X.xors(11));
					if (num9 != 0f || num10 != 0f)
					{
						this.Phy.remFoc(FOCTYPE.JUMP, true);
						if (flag12)
						{
							this.Phy.addFoc(FOCTYPE.HIT | FOCTYPE._CHECK_WALL | this.Skill.foc_damage_cliff_stopper, DIFF.player_hit_first_velocity_x(num9), 0f, -1f, -1, 1, 0, -1, 0);
						}
						if (flag13)
						{
							this.Phy.addFoc(FOCTYPE.HIT | FOCTYPE._CHECK_WALL | ((num10 < 0f) ? FOCTYPE._GRAVITY_LOCK : ((FOCTYPE)0U)), 0f, num10, -1f, -1, 1, 0, -1, 0);
						}
						this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL | this.Skill.foc_damage_cliff_stopper, num9, 0f, -1f, 0, 2, 20, 5, 3);
					}
				}
				if (text2 != "" && !this.isWormTrapped())
				{
					bool flag14 = this.isPoseDown(false);
					this.SpSetPose(text2, 1, null, false);
					bool flag15 = this.isPoseDown(false);
					if (this.AbsorbCon.isActive() && flag14 != flag15)
					{
						this.changeState(PR.STATE.ABSORB);
					}
				}
				else
				{
					if (CAim._XD(this.Anm.getAim(), 1) != CAim._XD(this.aim, 1))
					{
						this.Anm.setAim(this.aim, 0, false);
					}
					if (Atk.attr != MGATTR.WORM && (!this.AbsorbCon.isActive() || !this.AbsorbCon.no_shuffleframe_on_applydamage))
					{
						int num15 = this.Anm.getCurrentSequence().countFrames();
						if (num15 >= 6)
						{
							this.Anm.animReset(X.Mn(num15 - 1, (int)((float)num15 * X.NIXP(0.5f, 1.2f))));
						}
					}
				}
				if ((float)num5 <= X.Mx(0f, 40f * (CFG.sp_voice_for_pleasure - 0.8f) * X.ZLINE((float)this.ep - 300f, 300f)))
				{
					if (Atk.EpDmg != null && X.XORSP() < X.Scr(0.85f, X.ZLINE(CFG.sp_voice_for_pleasure - 1f, 0.5f)))
					{
						if (flag11 && !this.AbsorbCon.isActive())
						{
							text3 = ((X.XORSP() < 0.6f) ? "el" : "es");
						}
						else
						{
							text3 = ((X.XORSP() < 0.15f) ? "el" : "es");
							if (this.vo_near_orgasm_replace())
							{
								text3 = this.vo_near_orgasm_2_must_replace();
							}
							if (this.EpCon.isOrgasm() && text3 == "es")
							{
								text3 = "after_orgasm";
							}
						}
					}
				}
				else if (!this.is_alive && text3 != "death" && this.VO.isPlaying() && X.XORSP() < 0.6f)
				{
					text3 = "dmgx";
				}
				bool flag16 = false;
				if (CFG.sp_epdmg_vo_mouth > 100 && flag8)
				{
					int num16 = (int)(CFG.sp_epdmg_vo_mouth - 100);
					if (X.xors(120) < num16)
					{
						flag16 = true;
					}
				}
				this.playVo(text3, false, flag16);
			}
			PTCThreadRunner.clearVars();
			if (!this.AbsorbCon.use_torture)
			{
				if (!this.is_alive && this.Anm.strictPoseIs("buried"))
				{
					this.Anm.setPose("buried_death", -1, false);
				}
				if (this.Anm.poseIs(POSE_TYPE.DAMAGE_RESET, false))
				{
					this.Anm.animReset(X.xors(3));
				}
			}
			if (flag7)
			{
				this.BetoMng.Check(this, Atk, false, true);
			}
			if (flag6)
			{
				this.BetoMng.Check(this, BetoInfo.Ground, false, false);
			}
			if (this.pee_lock > 0)
			{
				if (Atk.Caster is NelEnemy)
				{
					this.PeeLockReduceCheck(0.05f);
				}
			}
			else
			{
				X.Mx(Atk.pee_apply100, X.ZPOW(this.MyStomach.water_drunk_level - 3.5f, 10f) * 70f);
				if (Atk.pee_apply100 > 0f)
				{
					this.checkNoelJuice(Atk.pee_apply100, true, true, -1);
				}
			}
			if ((this.splitMpByDamage(out num8, Atk, Atk._mpdmg, (MANA_HIT)14, 22, 1f, null, true) > 0f || num5 > 0 || fade_key != null) && Atk.setable_UP)
			{
				if (this.AbsorbCon.isActive() && TX.noe(text))
				{
					string uipicture_fade_key = this.AbsorbCon.uipicture_fade_key;
					if (TX.valid(uipicture_fade_key) && this.AbsorbCon.normal_UP_fade_injectable <= X.XORSP())
					{
						fade_key = uipicture_fade_key;
					}
				}
				this.UP.applyDamage(Atk.attr, num11, num12, emstate, decline_ui_additional_effect, fade_key, false);
			}
			this.GaugeBrk.check(num8);
			return num5;
		}

		public void PtcSTDead(string ptc_key, bool to_dead)
		{
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			this.PtcHld.PtcSTTimeFixed(ptc_key, 0.6f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
			if (to_dead)
			{
				PostEffect.IT.addTimeFixedEffect(base.M2D.Cam.Qu, 0.3f);
				base.M2D.Cam.Qu.VibP(6f, 10f, 4f, 0);
				base.M2D.Cam.Qu.HandShake(40f, 60f, 8f, 0);
				PostEffect.IT.setSlowFading(80f, 90f, 0.02f, -80);
				PostEffect.IT.setPEfadeinout(POSTM.FINAL_ALPHA, 70f, 50f, 0.8f, -70);
				PostEffect.IT.addTimeFixedEffect(PostEffect.IT.setPEfadeinout(POSTM.SUMMONER_ACTIVATE, 70f, 50f, 0.5f, -70), 1f);
			}
		}

		public override Vector2 getDamageCounterShiftMapPos()
		{
			Vector2 vector = new Vector2(this.Anm.counter_shift_map_x, this.Anm.counter_shift_map_y);
			if (base.hasFoot())
			{
				vector.x += this.FootD.shift_pixel_x * this.Mp.rCLEN;
				vector.y -= this.FootD.shift_pixel_y * this.Mp.rCLEN;
			}
			return vector;
		}

		public int applyHpDamageSimple(NelAttackInfoBase Atk, out bool force, int val = -1, string voice_type = null, bool show_damage_counter = true)
		{
			if (voice_type == null)
			{
				voice_type = "dmgs";
			}
			force = false;
			bool flag = this.applyDamageAddition(Atk);
			int num = val;
			if (val < 0)
			{
				val = (X.DEBUGNODAMAGE ? 0 : (X.DEBUGWEAK ? 9999 : X.IntR((float)Atk._hpdmg * (Atk.fix_damage ? 1f : this.applyHpDamageRatio(Atk)))));
				if (val == 0)
				{
					return 0;
				}
				val = (num = MDAT.getPrDamageVal(val, Atk, this));
			}
			force = val > 0;
			M2DmgCounterItem.DC dc;
			int num2;
			val = this.GSaver.applyHpDamage(val, Atk, out dc, out num2);
			val = base.applyHpDamage(val, true, Atk);
			this.GSaver.GsHp.Fine(true);
			this.NM2D.IMNG.CheckBombSelfExplode(this, Atk.attr, 1f);
			if (force)
			{
				this.setDamageCounter(-num, 0, dc, Atk.CurrentAbsorbedBy);
				this.NoDamage.Add(Atk.ndmg, (float)(this.isAbsorbState() ? X.Mx(Atk.nodamage_time - 6, 0) : Atk.nodamage_time));
				UIStatus.Instance.fineHpRatio(true, true);
				if (!this.is_alive)
				{
				}
			}
			else
			{
				if (flag)
				{
					val = -1;
				}
				this.setDamageCounter(-num, 0, dc, Atk.CurrentAbsorbedBy);
			}
			this.Ser.checkSer();
			return val + num2;
		}

		public bool applyDamageAddition(NelAttackInfoBase Atk)
		{
			bool flag = false;
			if (Atk.SerDmg != null)
			{
				flag = this.Ser.applySerDamage(Atk.SerDmg, this.getSerApplyRatio(), -1) || flag;
			}
			if (Atk.EpDmg != null)
			{
				flag = this.EpCon.applyEpDamage(Atk.EpDmg, Atk.AttackFrom, EPCATEG_BITS._ALL, 1f, true) || flag;
			}
			return flag;
		}

		public bool applyShieldResult(M2Shield.RESULT shield_rslt, AttackInfo Atk, bool show_dmg_counter = true)
		{
			return this.applyShieldResult(shield_rslt, Atk.attr, Atk.hit_x, Atk.hit_y, show_dmg_counter);
		}

		public bool applyShieldResult(M2Shield.RESULT shield_rslt_b, MGATTR attr, float hit_x, float hit_y, bool show_dmg_counter = true)
		{
			M2Shield.RESULT result = shield_rslt_b & M2Shield.RESULT._BIT_TYPE;
			bool flag = (shield_rslt_b & M2Shield.RESULT._NEAR_FLAG) > M2Shield.RESULT.NO_SHIELD;
			if (result == M2Shield.RESULT.GUARD || result == M2Shield.RESULT.GUARD_CONTINUE)
			{
				if (result != M2Shield.RESULT.GUARD_CONTINUE)
				{
					PostEffect.IT.setSlow(6f, 0f, 0);
					this.setDamageCounter(0, 0, M2DmgCounterItem.DC.GUARD, null);
				}
				return true;
			}
			if (result != M2Shield.RESULT.BROKEN || !flag)
			{
				return false;
			}
			if (this.Ser.has(SER.SHIELD_BREAK))
			{
				return true;
			}
			if (this.Skill.hasMagic())
			{
				this.Skill.killHoldMagic(true);
			}
			PostEffect.IT.setSlow(10f, 0f, 0);
			if (!this.AbsorbCon.isActive())
			{
				this.setDamageCounter(0, 0, M2DmgCounterItem.DC.NORMAL, null);
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._RELEASE, 0f, -0.2f, -1f, 0, 5, 0, -1, 0);
				this.quitCrouch(false, false, false);
				this.FootD.initJump(false, false, false);
				this.SpSetPose("dmg_t", 1, null, false);
				this.changeState(PR.STATE.DAMAGE_LT);
			}
			base.M2D.Cam.Qu.VibP(14f, 8f, 6f, 0).VibP(7f, 22f, 1f, 0);
			this.Ser.Add(SER.SHIELD_BREAK, -1, 99, false);
			this.TeSetDmgBlink(attr, 90f, 0.9f, 0f, 0);
			this.UP.applyDamage(attr, 1f, 9f, UIPictureBase.EMSTATE.NORMAL, false, null, false);
			this.playVo("shield_break", false, false);
			base.PtcVarS("attr", FEnum<MGATTR>.ToStr(MGATTR.NORMAL)).PtcVar("hit_x", (double)hit_x).PtcVar("hit_y", (double)hit_y)
				.PtcST("hitl", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			return true;
		}

		public bool FnHittingVelocity(M2Phys P, FOCTYPE type, ref float velocity_x, ref float velocity_y)
		{
			if (this.isAbsorbState() && !this.AbsorbCon.no_clamp_speed)
			{
				velocity_x *= 1f / (1f + this.AbsorbCon.total_weight);
				velocity_y *= 1f / (1f + this.AbsorbCon.total_weight * 1.5f);
			}
			return true;
		}

		public int applyMySelfFire(MagicItem Mg, M2Ray Ray, NelAttackInfo Atk)
		{
			int num = X.Mx((Atk._hpdmg == 0) ? 0 : 1, X.IntR((float)Atk._hpdmg * this.applyHpDamageRatio(Atk) / 4f));
			if (Mg.is_chanted_magic || Mg.kind == MGKIND.ITEMBOMB_MAGIC)
			{
				M2Shield.RESULT result = this.Skill.checkShield(Atk, (float)num);
				if (!this.applyShieldResult(result, Atk, false))
				{
					bool flag = false;
					if (Atk.SerDmg != null && this.Ser.applySerDamage(Atk.SerDmg, this.getSerApplyRatio(), -1))
					{
						flag = true;
					}
					if (num != 0 && ((this.manip & M2MoverPr.PR_MNP.NO_SINK) == (M2MoverPr.PR_MNP)0 || this.isMagicState(this.state)))
					{
						flag = true;
					}
					if (flag)
					{
						this.changeState(PR.STATE.ENEMY_SINK);
						this.Phy.addFoc(FOCTYPE.DAMAGE | this.Skill.foc_cliff_stopper, (float)((Atk.hit_x > base.x) ? (-1) : 1) * 0.22f, 0f, -1f, 0, 3, 30, 32, 0);
					}
					this.NM2D.IMNG.CheckBombSelfExplode(this, MGATTR.ENERGY, 1f);
				}
				return -1;
			}
			return num;
		}

		public bool isAlreadyAbsorbed(NelM2Attacker AbsorbBy)
		{
			return this.AbsorbCon.isAlreadyAbsorbed(AbsorbBy);
		}

		public bool isPoseAbsorbDefault()
		{
			return this.Anm.strictPoseIs("absorbed_downb", "absorbed_down", "absorbed_crouch2absorbed_down", "absorbed_crouch2absorbed_downb", "absorbed_crouch", "absorbed2absorbed_crouch", "absorbed");
		}

		public string absorb_default_pose(bool start_next_down)
		{
			if (this.isDownState(this.state) || this.isPoseDown(false))
			{
				if (!this.isPoseBackDown(false))
				{
					return "absorbed_down";
				}
				return "absorbed_downb";
			}
			else if (this.is_crouch || this.isPoseCrouch(false))
			{
				if (!start_next_down)
				{
					return "absorbed_crouch";
				}
				if (X.XORSP() >= 0.5f)
				{
					return "absorbed_crouch2absorbed_downb";
				}
				return "absorbed_crouch2absorbed_down";
			}
			else
			{
				if (!start_next_down)
				{
					return "absorbed";
				}
				return "absorbed2absorbed_crouch";
			}
		}

		public override void setAbsorbAnimation(string p)
		{
			if (TX.valid(p))
			{
				this.SpSetPose(p, -1, null, false);
				return;
			}
			this.SpSetPose(this.absorb_default_pose(X.XORSP() < X.Mx(0f, -0.2f + (float)this.AbsorbCon.Length * 0.2f) + (this.is_alive ? 0.4f : 0f)), -1, null, false);
		}

		public bool initAbsorb(NelAttackInfo Atk, NelM2Attacker AbsorbBy = null, AbsorbManager Abm = null, bool penetrate_absorb = false)
		{
			if (AbsorbBy == null || !(AbsorbBy is NelEnemy))
			{
				return false;
			}
			bool flag = false;
			this.AbsorbCon.releaseIndividualSpEvent();
			if (Abm == null || !Abm.isActive(this, AbsorbBy, false))
			{
				Abm = this.AbsorbCon.GetOrPop(this, AbsorbBy, ref flag, penetrate_absorb);
			}
			if (Abm == null)
			{
				return false;
			}
			if (!this.isDownState(this.state) && !this.isPoseDown(false))
			{
				if (!this.is_alive)
				{
					bool flag2 = X.XORSP() < 0.4f;
				}
			}
			if (Abm == null || !AbsorbBy.initAbsorb(Atk, this, Abm, penetrate_absorb))
			{
				if (Abm != null)
				{
					Abm.destruct();
				}
				return false;
			}
			if (this.is_alive)
			{
				this.AbsorbCon.countAdd(AbsorbBy);
			}
			if (flag || !this.isAbsorbState(this.state))
			{
				flag = true;
				base.M2D.Cam.TeCon.setBounceZoomIn(1.03125f, 12f, -4);
				if (this.isWormTrapped())
				{
					this.executeReleaseFromTrapByDamage(false, false);
				}
				this.changeState(PR.STATE.ABSORB);
				this.UP.applyDamage(Atk.attr, 0f, (float)(8 + X.xors(12)), UIPictureBase.EMSTATE.NORMAL, false, this.AbsorbCon.uipicture_fade_key, false);
				this.playAwkVo();
			}
			else
			{
				this.t_state = 0f;
			}
			this.PadVib("init_absorb", flag ? 1f : 0.66f);
			this.absorb_additional_dying = 0f;
			Abm.use_cam_zoom2 = true;
			return true;
		}

		public override void initTortureAbsorbPoseSet(string p, int set_frame = -1, int reset_anmf = -1)
		{
			this.Anm.clearDownTurning(false);
			base.initTortureAbsorbPoseSet(p, set_frame, -1);
			if (set_frame >= 0)
			{
				this.Anm.animReset(set_frame);
			}
			this.Anm.fineFreezeFrame();
			if (this.isPoseDown(false))
			{
				this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, false);
			}
			else if (this.isPoseCrouch(false))
			{
				this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
			}
			else
			{
				this.setBounds(M2MoverPr.BOUNDS_TYPE.NORMAL, false);
			}
			this.need_check_bounds = false;
			this.Phy.addLockWallHitting(this.AbsorbCon, -1f);
			this.Phy.killSpeedForce(true, true, true, false, false);
			this.Phy.addLockGravity(this.AbsorbCon, 0f, -1f);
		}

		public override void quitTortureAbsorb()
		{
			this.Phy.remLockWallHitting(this.AbsorbCon);
			this.Phy.remLockGravity(this.AbsorbCon);
		}

		public bool setTortureAnimation(string pose_name, int cframe, int loop_to)
		{
			if (this.Anm.poseIs(pose_name))
			{
				PxlSequence currentSequence = this.Anm.getCurrentSequence();
				loop_to = X.Mn(currentSequence.countFrames() - 1, loop_to);
				this.Anm.clearDownTurning(true);
				this.Anm.animReset((cframe < loop_to) ? cframe : ((cframe - loop_to) % (currentSequence.countFrames() - loop_to) + loop_to));
				return true;
			}
			return false;
		}

		public void setToTortureFix(float x, float y)
		{
			base.initDrawAssist(3, false);
			this.FootD.initJump(false, true, false);
			this.Phy.addLockGravityFrame(3);
			this.NM2D.Cam.blurCenterIfFocusing(this);
			this.drawx_ = x * base.CLEN;
			this.drawy_ = y * base.CLEN;
			if (X.LENGTHXYS(base.x, base.y, x, y) > 0.125f)
			{
				float num = X.VALWALK(base.x, x, 0.03125f);
				float num2 = X.VALWALK(base.y, y, 0.03125f);
				this.Phy.addFoc(FOCTYPE.ABSORB, num - base.x, num2 - base.y, -1f, -1, 1, 0, -1, 0);
			}
		}

		public bool releaseAbsorb(AbsorbManager Absorb)
		{
			return this.AbsorbCon.releaseFromTarget(this);
		}

		public override bool check_dangerous_bcc
		{
			get
			{
				return !this.isTrappedState() && !this.AbsorbCon.use_torture;
			}
		}

		public override AttackInfo applyDamageFromMap(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo _Atk, float efx, float efy, bool apply_execute = true)
		{
			if (X.DEBUGNODAMAGE || this.strong_throw_ray)
			{
				return null;
			}
			NelAttackInfo nelAttackInfo = base.applyDamageFromMap(MDI, _Atk, efx, efy, false) as NelAttackInfo;
			if (nelAttackInfo == null)
			{
				return null;
			}
			if (!apply_execute)
			{
				return nelAttackInfo;
			}
			nelAttackInfo.shuffleHpMpDmg(this, 1f, 1f, -1000, -1000);
			if (this.applyDamage(nelAttackInfo, false, (MDI != null) ? MDI.ui_fade_key : null, false, false) <= 0)
			{
				return null;
			}
			return nelAttackInfo;
		}

		public override bool setWaterDunk(int water_id, int misttype)
		{
			if (misttype == 5)
			{
				if (this.check_lava == 0f)
				{
					this.check_lava = 2f;
				}
				return false;
			}
			return true;
		}

		public override int ratingHpDamageVal(float ratio)
		{
			return (int)(150f * ratio + (float)X.Mx(this.maxhp - 150, 0) * 0.25f * ratio);
		}

		private void checkLavaExecute()
		{
			int num = (int)(base.mbottom - X.Mn(this.sizey, 0.8f));
			int num2 = (int)base.x;
			if (CCON.isWater(this.Mp.getConfig(num2, num)) && (base.M2D as NelM2DBase).MIST.isFire(num2, num))
			{
				if (!this.NoDamage.isActive(NDMG.MAPDAMAGE_LAVA) || !this.isDamagingOrKo())
				{
					this.check_lava = 15f;
					M2MapDamageContainer.M2MapDamageItem m2MapDamageItem = base.M2D.MDMGCon.Create(MAPDMG.LAVA, base.x, base.y, 0.05f, 0.05f, null);
					float num3;
					float num4;
					AttackInfo atk = m2MapDamageItem.GetAtk(null, this, out num3, out num4);
					if (this.applyDamageFromMap(m2MapDamageItem, atk, num3, num4, true) != null || !this.is_alive)
					{
						this.Ser.Add(SER.BURNED, -1, 1, false);
						this.check_lava = 30f;
						float vx = base.vx;
						this.Phy.killSpeedForce(false, true, true, false, false);
						if (this.is_alive)
						{
							this.lava_burn_dount++;
						}
						if (this.lava_burn_dount >= 3 && this.NM2D.CheckPoint.getCurPriority() < 5)
						{
							this.lava_burn_dount = 0;
						}
						if (this.lava_burn_dount >= 3)
						{
							this.Phy.killSpeedForce(true, true, true, false, false);
							this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._GRAVITY_LOCK, 0f, -0.09f, -1f, 0, 0, 40, 80, 2);
							this.NM2D.Iris.assignListener(this.NM2D.MIST).ForceWakeupInput(false);
							this.check_lava = 80f;
						}
						else
						{
							this.Phy.addFoc(FOCTYPE.DAMAGE, (float)((vx != 0f) ? X.MPF(vx > 0f) : X.MPFXP()) * (0.05f + 0.08f * X.XORSP()), -0.29f, -1f, 0, 30, 90, 10, 2);
						}
					}
					base.M2D.MDMGCon.Release(m2MapDamageItem);
					return;
				}
			}
			else
			{
				if (CCON.isWater(this.Mp.getConfig((int)base.x, (int)base.mbottom)))
				{
					this.check_lava = 5f;
					return;
				}
				this.check_lava = 0f;
			}
		}

		public override bool initDeath()
		{
			this.Anm.FlgDropCane.Add("DEAD");
			this.Skill.initDeath();
			(base.M2D as NelM2DBase).initGameOver();
			base.M2D.Cam.blurCenterIfFocusing(this);
			this.AbsorbCon.initDeath();
			return base.initDeath();
		}

		public Vector3 setToLoadGame(float x, float y)
		{
			BetobetoManager.immediate_load_material = 8;
			this.setTo(x, y);
			base.M2D.Cam.fineImmediately();
			NelChipBench nearBench = this.getNearBench(true, false);
			Vector3 zero = Vector3.zero;
			if (nearBench != null)
			{
				this.initBenchSitDown(nearBench, true, true);
				this.Anm.setPose("stand2bench", -1, false);
				zero = new Vector3(nearBench.mapcx, nearBench.mbottom - 0.125f, 1f);
			}
			else
			{
				this.changeState(PR.STATE.NORMAL);
			}
			UiBenchMenu.auto_start_temp_disable = true;
			this.Phy.recheckFoot(0f);
			return zero;
		}

		public override M2Mover setToDefaultPosition(bool no_set_camera = false, Map2d TargetMap = null)
		{
			if ((TargetMap ?? this.Mp) == null || this.Mp == null)
			{
				return this;
			}
			if (EnemySummoner.isActiveBorder())
			{
				M2LpSummon summonedArea = EnemySummoner.ActiveScript.getSummonedArea();
				this.setTo(summonedArea.mapcx, summonedArea.mapcy);
				return this;
			}
			return base.setToDefaultPosition(no_set_camera, TargetMap);
		}

		public void resetFlagsForGameOver()
		{
			this.Ser.checkSer();
			this.Ser.Cure(SER.DEATH);
			BGM.remHalfFlag("PR");
			this.Anm.repairCane();
			this.Anm.FlgDropCane.Rem("DEAD");
			this.Anm.FlgDropCane.Rem("DMG");
			this.setHpCrack(X.Mn(this.hp_crack, 4));
		}

		public void recoverFromGameOver()
		{
			this.Phy.clearLock();
			BetobetoManager.immediate_load_material = 8;
			this.SpSetPose("down_u", -1, null, false);
			this.hp = 1;
			this.mp = X.MMX(0, this.mp, this.maxmp - this.EggCon.total);
			this.setVoiceOverrideAllowLevel(0f);
			if (this.GaugeBrk.cureNotHunger() >= 0f)
			{
				this.mp = (int)X.MMX(0f, X.Mx(0.15f * (float)this.maxmp + 5f, (float)this.mp), (float)(this.maxmp - this.EggCon.total));
			}
			this.Skill.initS();
			this.EggCon.forcePushout(false, true);
			this.changeState(PR.STATE.GAMEOVER_RECOVERY);
			this.EpCon.lock_breath_progress = false;
			base.killSpeedForce(true, true, true);
			this.Phy.clearLock();
			this.Phy.immidiateCheckStuck();
			this.resetFlagsForGameOver();
			this.recoverGoSer();
			UIStatus.Instance.fineLoad();
			this.UP.changeEmotDefault(true, true);
			base.M2D.Cam.fineImmediately();
		}

		public int executeScapecatRespawn(int grade, bool change_state = true)
		{
			this.resetFlagsForGameOver();
			int num;
			int num2;
			float num3;
			int num4;
			MDAT.getScapecatReversalHpMp(grade, this, out num, out num2, out num3, out num4);
			this.hp = num;
			this.mp = X.IntR((float)num2 * (base.get_maxmp() - (float)this.EggCon.total) / base.get_maxmp());
			UIStatus.Instance.fineHpRatio(true, false);
			UIStatus.Instance.fineMpRatio(true, false);
			this.GaugeBrk.cureNotHunger();
			this.GaugeBrk.cureToRatioTemp(num3);
			this.GSaver.newGame();
			this.Skill.resetParameterS();
			if (change_state)
			{
				this.changeState(PR.STATE.BURST_SCAPECAT);
			}
			this.Ser.checkSerExecute(true, true);
			return num4;
		}

		public void recoverGoSer()
		{
			this.Ser.Cure(SER.BURST_TIRED);
			this.Ser.Cure(SER.BURNED);
			this.Ser.Cure(SER.FROZEN);
			this.Ser.Cure(SER.SLEEP);
			if (this.Ser.getLevel(SER.SEXERCISE) >= 1)
			{
				this.Ser.Add(SER.FRUSTRATED, -1, 99, false);
			}
		}

		public override int applyMpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			return this.applyMpDamage(val, force, Atk, true, false);
		}

		public int applyMpDamage(int val, bool force, AttackInfo Atk, bool use_quake, bool calc_gsaver = false)
		{
			float num2;
			int num = this.applyMpDamage(out num2, val, force, Atk, use_quake, calc_gsaver, false);
			this.GaugeBrk.check(num2);
			return num;
		}

		public int applyMpDamage(out float gauge_break, int val, bool force, AttackInfo Atk, bool use_quake, bool calc_gsaver = false, bool do_not_reduce_gsaver = false)
		{
			val -= this.Skill.applyMpDamagePuzzleMng(val, force);
			gauge_break = 0f;
			int num = base.applyMpDamage(val, force, Atk);
			if (calc_gsaver)
			{
				this.GSaver.applyMpDamage((float)val, Atk, ref gauge_break, -1f, use_quake);
			}
			else if (!do_not_reduce_gsaver)
			{
				this.GSaver.reduceMp(val, true, use_quake);
			}
			else
			{
				this.GSaver.GsMp.fineMinusDelay();
			}
			UIStatus.Instance.fineMpRatio(true, use_quake);
			this.Ser.checkSer();
			return num;
		}

		public void applyBurstMpDamage(int val)
		{
			if (this.isPuzzleManagingMp())
			{
				if (this.NM2D.Puz.puzz_magic_count_max == -1)
				{
					return;
				}
				this.applyMpDamage(val, true, null, false, false);
				return;
			}
			else
			{
				float num2;
				int num = this.applyMpDamage(out num2, val, true, null, false, false, false);
				if (num < val)
				{
					this.GaugeBrk.applyDamage(num2 + 7f * (float)(val - num) / (float)val + 0f);
					return;
				}
				this.GaugeBrk.applyDamage(num2 + 0f);
				return;
			}
		}

		public int applyEggPlantDamage(float val_mp_ratio, PrEggManager.CATEG categ, bool set_effect_and_alert, float check_ratio = 1f)
		{
			if (X.SENSITIVE)
			{
				return 0;
			}
			int num = X.IntC((float)this.maxmp * val_mp_ratio);
			int num2 = this.mp + DIFF.mp_egg_lock_adding(this, (float)this.mp);
			if (check_ratio < 1000f)
			{
				float num3 = 1f - (float)num2 / (float)this.maxmp - (float)(this.EggCon.total / this.maxmp);
				num3 = 1f - check_ratio * num3 * this.NM2D.NightCon.applyEggRatio();
				if (X.XORSP() < num3)
				{
					return 0;
				}
			}
			if (this.maxmp - num2 < num || (!this.Ser.has(SER.DO_NOT_LAY_EGG) && this.hasNearlyLayingEgg(categ)))
			{
				return 0;
			}
			num = X.Mx(0, X.Mn(num, this.maxmp - num2 - this.EggCon.total));
			if (num > 0)
			{
				this.EggCon.Add(categ, num);
				this.Ser.checkSer();
				this.GSaver.GsMp.Fine(false);
				this.BetoMng.Check(this, BetoInfo.Sperm, false, true);
				if (set_effect_and_alert)
				{
					PostEffect.IT.setSlow(45f, 0.5f, 0);
					this.PtcHld.PtcSTTimeFixed("worm_egg_activate", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
					base.PtcST("worm_egg_activate", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					UILog.Instance.AddAlertTX("Alert_" + categ.ToString().ToLower() + "_eggplant", UILogRow.TYPE.ALERT_EGG);
					this.UP.applyPlantedEgg();
				}
			}
			return num;
		}

		public void applyParalysisDamage()
		{
			if (this.isOrgasm() && this.AbsorbCon.isActive())
			{
				this.Anm.setPose((this.isPoseBackDown(false) || X.XORSP() < 0.7f) ? "downdamage" : "downdamage_t", -1, false);
				base.PtcVarS("attr", FEnum<MGATTR>.ToStr(MGATTR.THUNDER)).PtcVar("hit_x", (double)base.x).PtcVar("hit_y", (double)base.y)
					.PtcST("hits", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.UP.applyDamage(MGATTR.THUNDER, 8f, 1f, UIPictureBase.EMSTATE.NORMAL, false, "damage_thunder", false);
				float num = X.XORSP();
				this.playVo((num < 0.14f) ? "dmgs" : ((num < 0.92f) ? "es" : "el"), false, false);
			}
			else
			{
				NelAttackInfo nelAttackInfo = MDAT.AtkParalysis.BurstDir((float)X.MPF(X.xors(2) == 0));
				nelAttackInfo.CenterXy(base.x, base.y, 0f);
				this.applyDamage(nelAttackInfo, true, "damage_thunder", false, false);
			}
			if (this.AbsorbCon.isActive())
			{
				this.AbsorbCon.CorruptGacha(2f);
				if (base.M2D.isCenterPlayer(this))
				{
					UILog.Instance.AddAlertTX("corrupt_paralysis", UILogRow.TYPE.ALERT_HUNGER);
				}
			}
			if (base.M2D.isCenterPlayer(this))
			{
				PostEffect.IT.setPEfadeinout(POSTM.FINAL_ALPHA, 30f, 5f, 0.5f, -22);
			}
		}

		public void reduceCrackMp(int val)
		{
			if (val <= 0)
			{
				return;
			}
			this.Skill.reduceCrackMp(ref val);
			this.applyMpDamage(val, true, null, false, false);
		}

		public bool canApplySer(SER ser)
		{
			return ser != SER.MP_REDUCE || !this.isPuzzleManagingMp();
		}

		public override void cureHp(int val)
		{
			this.cureHp(val, true, true);
		}

		public void cureHp(int val, bool use_cushion, bool add_to_gsaver)
		{
			int hp = this.hp;
			base.cureHp(val);
			if (add_to_gsaver)
			{
				this.GSaver.addSavedHp((float)(this.hp - hp), true);
			}
			else
			{
				this.GSaver.GsHp.fineMinusDelay();
			}
			if (this.UP != null && this.UP.isActive())
			{
				UIStatus.Instance.fineHpRatio(use_cushion, false);
			}
			this.Ser.checkSer();
		}

		public override void cureMp(int val)
		{
			this.cureMp2(val, true, 0f, false, true, false, false);
		}

		public void cureMp(int val, bool use_cushion, bool add_to_gsaver, bool fine_gsaver = false)
		{
			this.cureMp2(val, true, 0f, false, use_cushion, add_to_gsaver, fine_gsaver);
		}

		protected void cureMp2(int val, bool secure_gage_split, float addable_to_oc_slot_ratio = 0f, bool addable_to_oc_slot_notenough = false, bool use_cushion = true, bool add_to_gsaver = false, bool fine_gsaver = false)
		{
			int num = val;
			int num2 = 120;
			int num3 = this.maxmp - this.EggCon.total;
			if (addable_to_oc_slot_ratio > 0f && !this.isPuzzleManagingMp() && EnemySummoner.isActiveBorder())
			{
				float num4 = X.NI(X.MMX(0, this.mp + num - X.Mn(num3, (int)((float)this.maxmp * this.GaugeBrk.safe_holdable_ratio)), num), num, addable_to_oc_slot_notenough ? 0.75f : 0.25f) * addable_to_oc_slot_ratio;
				if (num4 > 0f)
				{
					this.Skill.getOverChargeSlots().getMana(num4, ref num);
				}
			}
			if (num < 0)
			{
				this.applyMpDamage(num, true, null, false, false);
				return;
			}
			num = X.Mn(num3 - this.mp, num);
			base.cureMp(num);
			if (add_to_gsaver)
			{
				this.GSaver.addSavedMp((float)num, false);
			}
			this.GSaver.GsMp.Fine(fine_gsaver);
			if (num > 0 && secure_gage_split)
			{
				this.GaugeBrk.secureSplitTime((float)num2);
			}
			this.Skill.cureMp(ref num);
			if (this.UP != null && this.UP.isActive())
			{
				UIStatus.Instance.fineMpRatio(true, false);
			}
			this.Ser.checkSer();
		}

		public void setHpCrack(int _val)
		{
			_val = X.MMX(0, _val, 5);
			if (_val > this.hp_crack)
			{
				UIStatus.Instance.initHpCrack(false);
				PostEffect.IT.setSlow(10f, 0f, 0);
				this.Mp.playSnd((_val >= 5) ? "gage_crack_d" : "gage_crack");
				PostEffect.IT.addTimeFixedEffect(this.Mp.setET("mp_crack", 9f, 30f, 10f, 30, 0), 1f);
			}
			else
			{
				if (_val >= this.hp_crack)
				{
					return;
				}
				UIStatus.Instance.initHpCrack(true);
			}
			this.hp_crack = _val;
			if (this.hp_crack >= 5)
			{
				base.applyHpDamage(this.maxhp * 99, true, null);
				UIStatus.Instance.fineHpRatio(true, true);
				if (this.hp <= 0)
				{
					this.initDeathStasis(true);
				}
			}
		}

		public void cureFull(bool cure_mp, bool cure_ep, bool cure_egg = false, bool cure_water_drunk = false)
		{
			if (cure_ep)
			{
				this.Ser.CureB(50331648UL);
				if (!this.Ser.has(SER.NEAR_PEE))
				{
					cure_water_drunk = true;
				}
				this.ep = 0;
				this.EpCon.fineCounter();
				this.recheck_emot = true;
			}
			if (cure_mp)
			{
				this.GaugeBrk.reset();
				this.Skill.killHoldMagic(false);
				if (cure_egg)
				{
					this.EggCon.clear(true);
				}
				this.cureMp(this.maxmp, true, true, true);
				UIStatus.Instance.fineMpRatio(true, false);
				UIStatus.Instance.quitCrack();
				this.recheck_emot = true;
			}
			if (cure_water_drunk)
			{
				base.water_drunk = 0;
			}
			this.Ser.checkSer();
			if (this.UP != null && this.UP.isActive())
			{
				UIStatus.Instance.fineMpRatio(true, false);
			}
		}

		public bool cureMpNotHunger(bool explode_magic_mp_to_zero = false)
		{
			this.GaugeBrk.cureNotHunger();
			int mp = this.mp;
			this.mp = (int)X.Mn((float)(this.maxmp - this.EggCon.total), X.Mx(0.15f * (float)this.maxmp + 5f, (float)this.mp));
			UIStatus.Instance.fineMpRatio(true, false);
			UIStatus.Instance.quitCrack();
			if (mp < this.mp)
			{
				this.recheck_emot = true;
				if (explode_magic_mp_to_zero)
				{
					this.Skill.ExplodeMagicMpToZero();
				}
				return true;
			}
			return false;
		}

		public bool canGetMana(M2Mana Mana, bool is_focus)
		{
			if ((Mana.mana_hit & MANA_HIT.PR) == MANA_HIT.NOUSE || (Mana.mana_hit & MANA_HIT.TARGET_EN) != MANA_HIT.NOUSE)
			{
				return false;
			}
			float num = 0.25f;
			return X.BTW(base.mleft - num, Mana.x, base.mright + num) && X.BTW(base.mtop - num, Mana.y, base.mbottom + num);
		}

		public void addMpFromMana(M2Mana Mana, float val)
		{
			float num = 0f;
			if ((Mana.mana_hit & MANA_HIT.OC_ADDABLE) != MANA_HIT.NOUSE)
			{
				num = this.Skill.getOverChargeSlots().ser_overchargeable_ratio;
				if (num < 1f && this.EggCon.isActive())
				{
					num = X.Scr(num, (float)this.EggCon.total / (float)this.maxmp);
				}
			}
			if (Mana.special_attract && DIFF.I == 0)
			{
				val *= 2f;
			}
			this.cureMp2((int)val, true, this.is_alive ? (Mana.special_attract ? 1f : num) : 0f, num > 0f, true, DIFF.mana_mp_add_gsaver, (float)this.mp < this.GSaver.GsMp.saved_gauge_value);
			base.PtcVar("mx", (double)(Mana.x + X.NIXP(-0.45f, 0.45f))).PtcVar("my", (double)(Mana.y + X.NIXP(-0.45f, 0.45f))).PtcST("mana_get_pr", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
		}

		public void fineMpClip(bool use_cushion = false, bool fine_up_update = false)
		{
			bool flag = false;
			int num = X.Mx(0, this.maxmp - this.EggCon.total);
			if (this.mp > num)
			{
				this.mp = num;
				flag = true;
			}
			if ((flag || fine_up_update) && this.UP != null && this.UP.isActive())
			{
				UIStatus.Instance.fineMpRatio(use_cushion, false);
			}
			this.Ser.checkSer();
		}

		public bool runLayingEggCheck(bool activation = false)
		{
			if (activation)
			{
				this.Ser.Add(SER.LAYING_EGG, 100, 99, false);
			}
			if (!this.Ser.has(SER.LAYING_EGG) || this.isMoveScriptActive(false))
			{
				return false;
			}
			if (this.Ser.has(SER.EGGED))
			{
				return false;
			}
			if (this.Ser.has(SER.DO_NOT_LAY_EGG) || this.secureTimeState() || !base.hasFoot())
			{
				if (!activation || !this.isMagicExistState(this.state))
				{
					return false;
				}
				this.Ser.Cure(SER.DO_NOT_LAY_EGG);
			}
			if ((activation || this.state != PR.STATE.ORGASM) && this.changeLayingEggState(this.state))
			{
				if (activation)
				{
					this.playVo("laying_l", false, false);
				}
				this.Skill.killHoldMagic(false);
				bool flag = this.isDownState() || this.isPoseDown(false);
				this.changeState(PR.STATE.LAYING_EGG);
				if (this.isPoseCrouch(false) || flag)
				{
					this.SpSetPose("laying_egg", -1, null, false);
				}
				else
				{
					this.SpSetPose("stand2laying_egg", -1, null, false);
				}
			}
			return true;
		}

		public bool hasNearlyLayingEgg(PrEggManager.CATEG categ)
		{
			return this.Ser.has(SER.LAYING_EGG) && this.EggCon.hasNearlyLayingEgg(categ);
		}

		public bool canProgressLayingEgg()
		{
			return this.enemy_targetted > 0 || !this.is_alive || this.Ser.has(SER.EGGED_2);
		}

		public void lockSink(float f, bool clear_counter = true)
		{
			this.RCenemy_sink.Lock(f, clear_counter);
		}

		private void runEnemySink(bool enabled = false)
		{
			if (!base.hasFoot() || this.crouching > 0f)
			{
				enabled = false;
			}
			if (!enabled)
			{
				this.RCenemy_sink.Set(0f, false);
			}
			this.RCenemy_sink.Update(this.TS);
			if (!this.RCenemy_sink.isLocked() && this.RCenemy_sink >= 6f + 90f * X.ZPOW(this.getRE(RecipeManager.RPI_EFFECT.SINK_REDUCE)))
			{
				this.changeState(PR.STATE.ENEMY_SINK);
			}
		}

		public virtual bool canStartFrustratedMasturbate(bool starting = true)
		{
			if ((!starting || (this.isNormalState() && this.is_alive && base.hasFoot() && !this.getSkillManager().hasInput(true, true, true))) && Map2d.can_handle && TX.noe(base.M2D.ev_mobtype) && this.get_walk_xspeed() == 0f && !this.Phy.isin_water && (this.MistApply == null || !this.MistApply.isActive()))
			{
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC != null && !footBCC.is_ladder && !base.hasTalkTargetNearby())
				{
					return true;
				}
			}
			return false;
		}

		private void runDamaging()
		{
			Bench.P("Dmg1");
			if (this.state != PR.STATE.DAMAGE_BURNED && this.t_state >= 1f && this.Ser.has(SER.BURNED) && !this.isWormTrapped() && (!this.AbsorbCon.isActive() || !this.AbsorbCon.use_torture))
			{
				this.changeState(PR.STATE.DAMAGE_BURNED);
			}
			if (this.Anm.poseIs("stun2down") && this.canJump() && this.Anm.looped_already)
			{
				this.SpSetPose("stun2down_2", -1, null, false);
			}
			Bench.Pend("Dmg1");
			PR.STATE state = this.state;
			if (state <= PR.STATE.DAMAGE_PRESS_LR)
			{
				if (state <= PR.STATE.DAMAGE_LT)
				{
					if (state != PR.STATE.DAMAGE)
					{
						switch (state)
						{
						case PR.STATE.DAMAGE_L:
							if (this.t_state <= 0f)
							{
								this.NoDamage.Add(15f);
							}
							if (this.FootD.hasFootHard(0f))
							{
								if (this.t_state < 1000f)
								{
									this.t_state = X.Mn(100f, this.t_state);
								}
								if (this.poseIs("damage_thunder"))
								{
									if (this.t_state >= 15f)
									{
										this.SpSetPose("stunned", -1, null, false);
										this.changeState(PR.STATE.DAMAGE_L_LAND);
										base.PtcST("dmg_down", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
										return;
									}
									return;
								}
								else
								{
									if (this.poseIs("dmg_hktb") && X.XORSP() < 0.6f)
									{
										this.SpSetPose("dmg_down", -1, null, false);
										this.FootD.initJump(false, false, false);
										this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, 0f, -0.19f, -1f, -1, 1, 0, -1, 0);
										base.PtcST("dmg_bound", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
										this.remD(M2MoverPr.DECL.STOP_EVADE);
										return;
									}
									if (this.FootD.hasFootHard(3f) || this.t_state >= 15f)
									{
										this.SpSetPose(this.poseIs("dmg_hktb_b") ? "dmg_down_b_2" : "dmg_down2", -1, null, false);
										this.changeState(PR.STATE.DAMAGE_L_LAND);
										base.PtcST("dmg_down", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
										this.remD(M2MoverPr.DECL.STOP_EVADE);
										return;
									}
									return;
								}
							}
							else
							{
								if (this.t_state >= 1000f)
								{
									return;
								}
								if (this.t_state >= 120f)
								{
									this.addD(M2MoverPr.DECL.STOP_EVADE);
								}
								if (X.Abs(base.vx) > 0.02f)
								{
									this.t_state = X.Mn(100f, this.t_state);
								}
								if (this.t_state > 102f || base.canGoToSide((base.vx == 0f) ? (this.isPoseBack(false) ? this.aim : base.aim_behind) : ((base.vx > 0f) ? AIM.R : AIM.L), X.Mx(X.Abs(base.vx), 0.14f) + 0.02f, -0.1f, false, false, false))
								{
									return;
								}
								if (this.poseIs("dmg_hktb"))
								{
									this.SpSetPose("dmg_down_hitwall", -1, null, false);
									AIM aim = CAim.get_opposite(this.aim);
								}
								else
								{
									if (!this.poseIs("dmg_hktb_b"))
									{
										this.t_state = 1000f;
										return;
									}
									this.SpSetPose("dmg_down_b_hitwall", -1, null, false);
									AIM aim2 = this.aim;
								}
								this.playVo("arrest", false, false);
								this.Phy.killSpeedForce(true, true, true, false, false);
								this.changeState(PR.STATE.DAMAGE_L_HITWALL);
								this.PadVib("dmg_hit_wall", 1f);
								this.Phy.immidiateCheckStuck();
								this.Mp.M2D.Cam.Qu.SinH(18f, 40f, 3f, 0).Vib(9f, 18f, 2f, 0);
								base.playSndAbs("pr_hit_wall");
								if (X.XORSP() < 0.45f)
								{
									this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
									this.Phy.addFoc(FOCTYPE.DAMAGE, X.NIXP(0.35f, 0.4f) * -base.vx, -0.02f, -1f, 0, 30, 90, 28, 0);
									return;
								}
								this.Phy.addFoc(FOCTYPE.DAMAGE, X.NIXP(0.19f, 0.24f) * -base.vx, 0f, -1f, 0, 30, 90, 20, 0);
								return;
							}
							break;
						case PR.STATE.DAMAGE_L_HITWALL:
							if (this.FootD.hasFootHard((float)((this.t_state >= 10f) ? 0 : 3)) || this.t_state >= 100f)
							{
								this.t_state = 100f;
								base.playSndAbs("prko_hit_0");
								if (this.Anm.poseIs("stun2down", "stun2down_2", "down", "downdamage", "downdamage_t", null, null))
								{
									this.changeState(PR.STATE.DAMAGE_L_LAND);
									return;
								}
								if (this.poseIs("dmg_down_hitwall") || this.poseIs("stunned"))
								{
									this.SpSetPose((X.XORSP() < 0.3f) ? "stun2down" : "stunned", -1, null, false);
									this.changeState(PR.STATE.DAMAGE_L_LAND);
									return;
								}
								if (this.poseIs("dmg_down_b_hitwall") || this.poseIs("dmg_down_b_hitwall_2"))
								{
									this.SpSetPose((X.XORSP() < 0.3f) ? "stunned" : "dmg_down_b_hitwall_2", -1, null, false);
									this.changeState(PR.STATE.DAMAGE_L_LAND);
									return;
								}
								return;
							}
							else
							{
								if (this.t_state < 100f)
								{
									this.t_state = X.Mn(this.t_state, 50f);
									return;
								}
								return;
							}
							break;
						case (PR.STATE)4012:
						case (PR.STATE)4013:
						case (PR.STATE)4014:
							return;
						case PR.STATE.DAMAGE_L_LAND:
						case PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER:
							goto IL_0691;
						default:
							if (state != PR.STATE.DAMAGE_LT)
							{
								return;
							}
							break;
						}
					}
					else
					{
						if (this.t_state >= 13f && this.is_alive)
						{
							this.remD((M2MoverPr.DECL)3);
							if (base.isEvadeO(0) || base.isMagicO(0))
							{
								this.changeState(PR.STATE.NORMAL);
								return;
							}
						}
						if (this.t_state >= (float)this.knockback_time)
						{
							this.changeState(PR.STATE.NORMAL);
							return;
						}
						return;
					}
				}
				else if (state != PR.STATE.DAMAGE_LT_KIRIMOMI)
				{
					if (state == PR.STATE.DAMAGE_LT_LAND)
					{
						goto IL_0691;
					}
					if (state != PR.STATE.DAMAGE_PRESS_LR)
					{
						return;
					}
					if (this.t_state <= 0f)
					{
						this.t_state = 0f;
						if (!this.isUiPressDamage())
						{
							this.SpSetPose("dmg_press", -1, null, false);
						}
						this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
						this.Phy.addLockGravity(this, 0f, 120f);
					}
					if (!this.hasD(M2MoverPr.DECL.FLAG0))
					{
						this.Phy.immidiateCheckStuck();
						this.Phy.killSpeedForce(true, true, true, false, false);
						this.offset_pixel_y = X.VALWALK(this.offset_pixel_y, this.presscrouch_offset_pixel_y, 0.55f * this.TS);
						if (!this.press_damage_state_skip(120f))
						{
							return;
						}
						if (this.isUiPressDamage())
						{
							this.addD(M2MoverPr.DECL.FLAG0);
							this.remD((M2MoverPr.DECL)3);
							this.t_state = 800f;
							this.TeCon.setEnlargeAbsorbed(1.45f, 1f, 50f, 10);
							this.SpSetPose("ui_press_damage_return", -1, null, false);
							return;
						}
						this.Mp.DropCon.setBlood(this, 72, MTR.col_blood, 0f, true);
						this.changeState(PR.STATE.DAMAGE_L_HITWALL);
						this.SpSetPose("stunned", -1, null, false);
						return;
					}
					else
					{
						if (this.t_state >= 920f || this.Anm.poseIs("stunned"))
						{
							this.changeState(PR.STATE.DAMAGE_L_HITWALL);
							this.remD((M2MoverPr.DECL)3);
							return;
						}
						return;
					}
				}
				PR.STATE state2 = this.runDamagingHktbLT(this.state);
				if (state2 != this.state)
				{
					this.Anm.rotationR = 0f;
					this.changeState(state2);
					this.remD(M2MoverPr.DECL.STOP_EVADE);
					return;
				}
				return;
			}
			else if (state <= PR.STATE.DOWN_STUN)
			{
				if (state != PR.STATE.DAMAGE_PRESS_TB)
				{
					if (state != PR.STATE.DAMAGE_OTHER_STUN && state != PR.STATE.DOWN_STUN)
					{
						return;
					}
				}
				else
				{
					if (this.t_state <= 0f)
					{
						this.t_state = 0f;
						if (!this.isUiPressDamage())
						{
							this.SpSetPose("dmg_press_t", -1, null, false);
						}
						this.FootD.initJump(false, false, false);
						this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
						this.Phy.killSpeedForce(true, true, true, false, false);
					}
					if (!this.hasD(M2MoverPr.DECL.FLAG0))
					{
						this.Phy.immidiateCheckStuck();
						if (!this.press_damage_state_skip(70f))
						{
							return;
						}
						if (this.isUiPressDamage())
						{
							AIM aim3 = this.aim;
							this.addD(M2MoverPr.DECL.FLAG0);
							this.remD((M2MoverPr.DECL)3);
							this.t_state = 800f;
							this.SpSetPose("ui_press_damage_down_return", -1, null, false);
							this.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(aim3, 1), 1f, false), -1, false);
							this.TeCon.setEnlargeAbsorbed(1f, 1.65f, 50f, 10);
							return;
						}
						this.Mp.DropCon.setBlood(this, 72, MTR.col_blood, 0f, true);
						M2MoverPr.BOUNDS_TYPE bounds_ = this.bounds_;
						this.changeState(PR.STATE.DAMAGE_L_HITWALL);
						this.SpSetPose("downdamage_t", -1, null, false);
						this.need_check_bounds = false;
						this.setBounds(bounds_, false);
						return;
					}
					else
					{
						if (this.t_state >= 920f || this.Anm.poseIs("down"))
						{
							this.changeState(PR.STATE.DAMAGE_L_HITWALL);
							this.remD((M2MoverPr.DECL)3);
							return;
						}
						return;
					}
				}
			}
			else if (state != PR.STATE.DAMAGE_BURNED)
			{
				if (state != PR.STATE.ABSORB)
				{
					if (state != PR.STATE.WORM_TRAPPED)
					{
						return;
					}
					Bench.P("Dmg_WormTrapped2");
					if (this.t_state <= 0f)
					{
						this.UP.applyDamage(MGATTR.NORMAL, (float)(-25 + X.xors(14)), (float)(1 + X.xors(5)), UIPictureBase.EMSTATE.NORMAL, false, null, false);
						this.t_state = (float)(170 - X.xors(10));
						this.playAwkVo();
					}
					Bench.Pend("Dmg_WormTrapped2");
					Bench.P("Dmg_WormTrapped3");
					if (this.t_state >= 200f)
					{
						int num = (int)(this.t_state / 30f) % 2;
						if (num == 0 != this.hasD(M2MoverPr.DECL.FLAG0))
						{
							if (num == 0)
							{
								this.addD(M2MoverPr.DECL.FLAG0);
							}
							else
							{
								this.remD(M2MoverPr.DECL.FLAG0);
							}
							base.M2D.Cam.Qu.Vib(3f, 5f, 2f, 0);
							this.Mp.PtcST("pr_worm_trapped", null, PTCThread.StFollow.NO_FOLLOW);
							this.t_state += (float)(4 + X.xors(14));
						}
					}
					Bench.Pend("Dmg_WormTrapped3");
					Bench.P("Dmg_WormTrapped4");
					if (this.Anm.isAnimEnd())
					{
						this.TeCon.setFadeOut(1f, 0f);
					}
					Bench.Pend("Dmg_WormTrapped4");
					return;
				}
				else
				{
					if (this.AbsorbCon.runAbsorbPr(this, this.t_state, this.TS))
					{
						this.t_state = X.Mn(this.t_state, 120f);
						return;
					}
					bool flag = (this.AbsorbCon.release_type & AbsorbManagerContainer.RELEASE_TYPE.GACHA) > AbsorbManagerContainer.RELEASE_TYPE.NORMAL;
					bool flag2 = this.is_alive && !this.Ser.hasBit(51052608UL);
					this.EpCon.breath_key = null;
					if (flag)
					{
						this.Ser.Cure(SER.SHIELD_BREAK);
					}
					if (flag2 && flag)
					{
						base.endDrawAssist(1);
						this.changeState(PR.STATE.UKEMI_SHOTGUN);
						return;
					}
					if (!this.Ser.has(SER.BURNED) && (this.AbsorbCon.release_type & AbsorbManagerContainer.RELEASE_TYPE.KIRIMOMI) != AbsorbManagerContainer.RELEASE_TYPE.NORMAL)
					{
						base.endDrawAssist(1);
						this.isPoseBack(false);
						this.changeState(PR.STATE.DAMAGE_LT_KIRIMOMI);
						this.Phy.addLockMoverHitting(HITLOCK.ABSORB, 40f);
						this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, (float)CAim._XD(this.AbsorbCon.kirimomi_release_dir_last, 1) * 0.16f, X.NIXP(-0.12f, -0.21f), -1f, 0, 10, 90, 20, 0);
						this.FootD.initJump(false, true, false);
						return;
					}
					if (TX.isStart(this.Anm.pose_title, "torture", 0))
					{
						base.endDrawAssist(1);
						if (this.isPoseBackDown(false))
						{
							this.SpSetPose("dmg_hktb", -1, null, false);
							this.changeState(PR.STATE.DAMAGE_L);
							this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, -base.mpf_is_right * 0.08f, -0.02f, -1f, 0, 30, 90, 20, 0);
							return;
						}
						if (this.isPoseDown(false))
						{
							this.SpSetPose("dmg_hktb_b", -1, null, false);
							this.changeState(PR.STATE.DAMAGE_L);
							this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, -base.mpf_is_right * 0.08f, -0.02f, -1f, 0, 30, 90, 20, 0);
							return;
						}
						this.SpSetPose("dmg_t", 1, null, false);
						this.changeState(PR.STATE.DAMAGE_LT);
						this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, -base.mpf_is_right * 0.02f, -0.07f, -1f, 0, 30, 90, 20, 0);
						return;
					}
					else
					{
						this.quitTortureAbsorb();
						if (this.t_state <= 120f)
						{
							this.t_state = 120f;
						}
						if (this.t_state < 130f)
						{
							return;
						}
						base.endDrawAssist(1);
						bool flag3 = !this.Anm.poseIs("down", "down_b", "down_u") && !this.Anm.nextPoseIs("down", "down_b", "down_u");
						if (this.isPoseDown(false))
						{
							if (flag3 && !this.poseIs(POSE_TYPE.ORGASM))
							{
								this.SpSetPose("down_u", -1, null, false);
							}
							this.changeState(PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER);
						}
						else if (!flag2 || this.getCastableMp() <= 0f)
						{
							if (flag3)
							{
								this.SpSetPose("stunned", -1, null, false);
							}
							this.changeState(PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER);
						}
						else
						{
							bool flag4 = this.isPoseCrouch(false) || this.is_crouch;
							this.changeState(PR.STATE.ENEMY_SINK);
							if (flag3)
							{
								this.Anm.setPose(flag4 ? "absorb_release" : "stand2absorb_release", -1, false);
							}
						}
						this.recheck_emot = true;
						if (this.is_alive)
						{
							this.NoDamage.Add(NDMG.DEFAULT, 55f);
							return;
						}
						return;
					}
				}
			}
			else
			{
				if (this.t_state <= 0f)
				{
					this.playVo("dmgl", false, false);
					this.t_state = 0f;
					float num2 = this.Phy.calcFocVelocityX(FOCTYPE.DAMAGE, false);
					float num3 = this.Phy.calcFocVelocityX(FOCTYPE.WALK, false);
					this.Phy.walk_xspeed = ((X.Abs(num2) > X.Abs(num3)) ? num2 : num3);
					this.SpSetPose("dmg_burned", -1, null, false);
					this.Anm.setAim((this.anm_mpf_is_right > 0f) ? AIM.RB : AIM.BL, -1, false);
				}
				bool flag5 = true;
				float num4 = this.Phy.walk_xspeed;
				bool flag6 = false;
				bool flag7 = this.is_alive || CFG.sp_deadburned;
				if (this.FootD.hasFootHard(2f))
				{
					if (!flag7 || !this.Ser.has(SER.BURNED))
					{
						this.EpCon.breath_key = "breath_e";
						if (!this.Anm.poseIs("stand_norod2laying_egg", "laying_egg"))
						{
							this.SpSetPose((this.isPoseDown(false) || this.is_crouch) ? "laying_egg" : "stand_norod2laying_egg", -1, null, false);
							this.UP.recheck_emot = true;
						}
						flag6 = true;
						if (this.UP.getCurEmot() == UIEMOT.BURNED)
						{
							this.UP.recheck_emot = true;
						}
						this.addD(M2MoverPr.DECL.FLAG0);
						num4 = 0f;
						if (this.is_alive)
						{
							this.remD((M2MoverPr.DECL)3);
							if (base.isMovingPD())
							{
								this.changeState(PR.STATE.NORMAL);
								return;
							}
							flag5 = false;
						}
					}
					else
					{
						this.remD(M2MoverPr.DECL.FLAG0);
						if (this.t_state <= 2000f)
						{
							this.t_state = 2000f;
						}
						AIM aim4 = this.aim;
						if (!this.Anm.poseIs("dmg_burned_run"))
						{
							this.SpSetPose("dmg_burned_run", -1, null, false);
						}
						if (this.t_state >= 2015f)
						{
							if (this.isLP(1))
							{
								aim4 = AIM.L;
							}
							else if (this.isRP(1))
							{
								aim4 = AIM.R;
							}
							if (aim4 == AIM.R && base.wallHitted(AIM.R))
							{
								aim4 = AIM.L;
								num4 = -num4;
							}
							else if (aim4 == AIM.L && base.wallHitted(AIM.L))
							{
								aim4 = AIM.R;
								num4 = -num4;
							}
							if (aim4 != this.aim)
							{
								this.setAim(aim4, false);
								this.t_state = 2000f;
							}
						}
						num4 = X.VALWALK(num4, base.mpf_is_right * 0.115f, 0.00575f);
					}
				}
				else
				{
					this.remD(M2MoverPr.DECL.FLAG0);
					if (this.t_state >= 2000f && X.Abs(num4) < 0.04f)
					{
						num4 = (float)X.MPF(num4 > 0f) * 0.04f;
					}
					if (!this.Anm.poseIs("dmg_burned"))
					{
						this.SpSetPose("dmg_burned", -1, null, false);
						this.Anm.setAim((base.mpf_is_right > 0f) ? AIM.R : AIM.L, -1, false);
					}
					if (this.t_state >= 1900f)
					{
						this.t_state = 1900f;
					}
					if (this.lava_burn_dount < 3)
					{
						if (this.isLO(0) && num4 > -0.085f)
						{
							num4 = X.VALWALK(num4, -0.085f, 0.006071429f);
						}
						else if (this.isRO(0) && num4 < 0.085f)
						{
							num4 = X.VALWALK(num4, 0.085f, 0.006071429f);
						}
						else if (X.Abs(num4) < 0.04f)
						{
							num4 = X.VALWALK(num4, (float)X.MPF(num4 > 0f) * 0.04f, 0.0085f);
						}
					}
				}
				this.Phy.walk_xspeed = num4;
				if (this.t_state <= 0f || (flag5 && !this.TeCon.existSpecific(TEKIND.DMG_BLINK)))
				{
					this.setBurnedEffect(this.canJump() && num4 != 0f, this.t_state <= 0f || (!this.canJump() || flag7), flag6);
					return;
				}
				return;
			}
			IL_0691:
			if (this.t_state <= 0f)
			{
				this.UP.recheck_emot = true;
			}
			bool flag8 = this.MistApply != null && this.MistApply.isWaterChokeDamageAlreadyApplied();
			bool flag9 = false;
			if (this.t_state >= 120f || flag8)
			{
				flag9 = true;
				this.addD(M2MoverPr.DECL.STOP_EVADE);
			}
			if (this.Anm.pose_is_stand || this.Anm.next_pose_is_stand)
			{
				this.SpSetPose((X.XORSP() >= 0.25f == this.isPoseDown(false)) ? "down_b" : "down", -1, null, false);
			}
			if (this.FootD.hasFootHard((float)((this.t_state >= 20f) ? 0 : 3)) || this.t_state >= 100f)
			{
				if (this.t_state < 100f)
				{
					this.t_state = 100f;
					if (!flag9 && !this.hasD(M2MoverPr.DECL.INIT_A))
					{
						this.remD(M2MoverPr.DECL.STOP_EVADE);
					}
				}
				this.addD(M2MoverPr.DECL.INIT_A);
				if (this.is_alive && !flag8)
				{
					if (this.enemy_targetted == 0)
					{
						PR.PunchDecline(20, false);
					}
					else if (this.t_state >= 120f)
					{
						this.remD(M2MoverPr.DECL.STOP_ACT);
					}
					if (this.t_state >= 135f && (this.WakeUpInput(true, true) || base.isEvadeO(0)))
					{
						this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
						this.changeState(PR.STATE.NORMAL);
						return;
					}
				}
			}
			else if (this.t_state < 100f)
			{
				this.t_state = X.Mn(this.t_state, 50f);
				this.addD(M2MoverPr.DECL.STOP_ACT);
				return;
			}
		}

		private bool press_damage_state_skip(float def_wait_t)
		{
			if (this.NM2D.Iris.isWaiting(this, IrisOutManager.IRISOUT_TYPE.PRESS))
			{
				return false;
			}
			if (!this.hasD(M2MoverPr.DECL.INIT_A))
			{
				return this.t_state >= def_wait_t;
			}
			if (!this.is_alive)
			{
				if (this.t_state >= 220f)
				{
					this.t_state = 220f - X.NIXP(50f, 70f);
					this.TeCon.setQuake(2f, (int)X.NIXP(6f, 10f), 1f, 0);
				}
				return false;
			}
			if ((this.t_state >= def_wait_t && base.isActionO(0)) || this.t_state >= 220f)
			{
				this.remD((M2MoverPr.DECL)3);
				return true;
			}
			return false;
		}

		public float pressdamage_scale_level
		{
			get
			{
				return 0.04f + 0.62f * X.ZLINE(this.t_state - 50f, 75f);
			}
		}

		public float pressdamage_float_level
		{
			get
			{
				return X.ZLINE(this.t_state - 50f, 130f);
			}
		}

		protected void playAwkVo()
		{
			this.playVo(((this.EpCon.isOrgasm() || !this.is_alive) && X.XORSP() < 0.8f) ? "awkx" : "awk", false, false);
		}

		public TransEffecterItem setBurnedEffect(bool playvo, bool applyemot, bool decline_apply_emot = false)
		{
			if (playvo)
			{
				this.playVo("heat", false, false);
			}
			TransEffecterItem transEffecterItem = this.TeSetDmgBlink(MGATTR.FIRE, (float)(25 + X.xors(15)), 1f, 1f, 0);
			this.UP.applyBurned(applyemot && !this.isBenchOrGoRecoveryState(), true);
			if (!decline_apply_emot && (this.UP.getCurEmot() != UIEMOT.BURNED || applyemot))
			{
				this.UP.setFade("burned", UIPictureBase.EMSTATE.NORMAL, false, false, false);
			}
			return transEffecterItem;
		}

		public void fineFrozenAF(float time = 4f)
		{
			this.frozen_fineaf_time = X.Mx(this.frozen_fineaf_time, time);
		}

		public void fineFrozenAppearance()
		{
			if (this.Ser.has(SER.FROZEN))
			{
				base.base_gravity = this.base_gravity0 * this.Ser.baseTimeScaleRev();
				int level = this.Ser.getLevel(SER.FROZEN);
				byte b = (byte)(level + 1);
				if (this.Anm != null)
				{
					this.Anm.frozen_lv = b;
				}
				if (this.BetoMng.frozen_lv != b)
				{
					if (this.Anm != null)
					{
						this.Anm.fineFreezeFrame();
					}
					if (this.UP != null)
					{
						this.UP.killPtc("ui_ser_frozen");
						this.UP.PtcST("ui_ser_frozen", null);
					}
				}
				if (this.BetoMng.frozen_lv < b)
				{
					base.PtcVar("level", (double)level).PtcST("frozen_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				this.BetoMng.frozen_lv = b;
			}
			else
			{
				base.base_gravity = this.base_gravity0;
				if (this.BetoMng.frozen_lv > 0)
				{
					this.BetoMng.frozen_lv = 0;
					base.PtcST("frozen_quit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				if (this.Anm != null)
				{
					this.Anm.fineFreezeFrame();
					this.Anm.frozen_lv = 0;
				}
				if (this.UP != null)
				{
					this.UP.killPtc("ui_ser_frozen");
				}
			}
			if (this.UP != null)
			{
				this.UP.getAdditionalState(true);
			}
		}

		private PR.STATE runDamagingHktbLT(PR.STATE state)
		{
			if (this.t_state <= 0f)
			{
				this.NoDamage.Add(15f);
			}
			bool flag = false;
			if (state == PR.STATE.DAMAGE_LT_KIRIMOMI)
			{
				if (this.t_state <= 0f)
				{
					this.t_state = 0f;
					this.SpSetPose("dmg_t", -1, null, false);
					this.Phy.addLockGravity(this, 0.77f, 25f);
					this.FootD.initJump(false, false, false);
				}
				flag = true;
				this.Anm.rotationR = this.Mp.GAR(0f, 0f, base.vx, base.vy) - 1.5707964f;
				if (this.t_state >= 30f)
				{
					this.Phy.clampSpeed(FOCTYPE.HIT, 0.05f, -1f, 0.007f);
				}
			}
			if (this.canJump())
			{
				this.SpSetPose("dmg_down_t", -1, null, false);
				base.PtcST("dmg_down", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.playVo("arrest", false, false);
				return PR.STATE.DAMAGE_LT_LAND;
			}
			if (base.vy >= -0.4f && this.t_state >= 25f && !flag)
			{
				this.SpSetPose("dmg_t_2", -1, null, false);
			}
			return this.state;
		}

		public bool walkWormTrapped(M2WormTrap Worm, bool is_inner)
		{
			this.Ser.Add(SER.WORM_TRAPPED, -1, 99, false);
			if (!is_inner)
			{
				if (CAim._YD(Worm.head_aim, 1) < 0)
				{
					this.Phy.addLockGravity(Worm, 0.022f, 5f);
				}
				else
				{
					this.Phy.addLockGravity(Worm, 0.25f, 5f);
					this.Phy.clampSpeed(FOCTYPE.ABSORB, -1f, this.ySpeedMax0 * 0.33f, 0.125f);
				}
				float num = Worm.x - base.x;
				float num2 = Worm.y - base.y;
				int num3 = CAim._XD(Worm.head_aim, 1);
				if (num3 != 0)
				{
					num *= (float)((num3 > 0 == num > 0f) ? 0 : 4);
				}
				else
				{
					num2 *= 4f;
				}
				float num4 = this.Mp.GAR(0f, 0f, num, num2);
				this.Phy.addFoc(FOCTYPE.ABSORB, 0.02f * X.Cos(num4), ((CAim._YD(Worm.head_aim, 1) < 0) ? (-0.07f) : (-0.02f)) * X.Sin(num4), -1f, -1, 1, 0, -1, 0);
				this.Skill.wormFocApplied(Worm.head_aim);
			}
			else
			{
				this.Phy.killSpeedForce(true, true, true, false, false).addFric(5f);
				if (this.isWormTrapped())
				{
					if (this.NM2D.FCutin.isActive(this))
					{
						this.addD(M2MoverPr.DECL.THROW_RAY);
					}
					return true;
				}
				this.Skill.evadeTimeBlur(false, true, true);
				this.Phy.remLockGravity(Worm);
				this.Phy.addLockGravity(this, 0f, -1f);
				this.AbsorbCon.releaseFromTarget(this);
				this.recheck_emot = true;
				this.changeState(PR.STATE.WORM_TRAPPED);
				this.FootD.initJump(false, true, false);
				this.NM2D.MGC.killAllPlayerMagic(this, (MagicItem Mg) => !(Mg.Other is IMgBombListener));
				if (Worm.head_aim != AIM.B && this.isPoseDown(false))
				{
					this.SpSetPose("spike_trapped_down", -1, null, false);
					if (Worm.head_aim != AIM.T)
					{
						int num5 = (int)base.mpf_is_right * X.MPF(this.isPoseBack(false));
						int num6 = CAim._XD(Worm.head_aim, 1);
						this.Anm.setAim(CAim.get_aim2(0f, 0f, (float)(-(float)num6), (float)((num6 != num5) ? (-1) : 1), false), -1, false);
					}
					else
					{
						this.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(this.aim, 1), 0f, false), -1, false);
					}
				}
				else
				{
					this.SpSetPose(this.is_crouch ? "spike_trapped_crouch" : "spike_trapped", -1, null, false);
					if (Worm.head_aim == AIM.T)
					{
						this.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(this.aim, 1), -1f, false), -1, false);
					}
					else if (Worm.head_aim == AIM.B)
					{
						this.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(this.aim, 1), 1f, false), -1, false);
					}
					else
					{
						this.Anm.setAim(this.aim = CAim.get_aim2(0f, 0f, (float)(-(float)CAim._XD(Worm.head_aim, 1)), 0f, false), -1, false);
					}
				}
			}
			return is_inner;
		}

		public void executeReleaseFromTrapByDamage(bool changestate = true, bool addfoc = true)
		{
			this.Phy.killSpeedForce(true, true, true, false, false).remLockGravity(this).clearColliderLock();
			this.Ser.Cure(SER.WORM_TRAPPED);
			if (changestate)
			{
				this.changeState(PR.STATE.DAMAGE_LT_KIRIMOMI);
			}
			float num = (float)CAim._YD(this.Anm.pose_aim, 1);
			float num2;
			float num3;
			if (this.Anm.poseIs("spike_trapped", "spike_trapped_crouch"))
			{
				if (num == 0f)
				{
					num2 = -0.13f;
					num3 = -base.mpf_is_right * 0.23f;
				}
				else
				{
					num3 = X.NIXP(-0.07f, 0.07f);
					num2 = 0.18f * num;
				}
			}
			else if (num == 0f)
			{
				num3 = X.NIXP(-0.07f, 0.07f);
				num2 = -0.25f * num;
			}
			else
			{
				num3 = -base.mpf_is_right * 0.25f;
				num2 = -0.09f;
			}
			if (addfoc)
			{
				this.Phy.addFoc(FOCTYPE.KNOCKBACK | FOCTYPE._INDIVIDUAL, num3, num2, -1f, 0, 20, 15, -1, 0);
			}
			base.PtcVar("agR", (double)this.Mp.GAR(0f, 0f, base.vx, base.vy)).PtcST("worm_released_splash", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			this.FootD.initJump(false, false, false);
			this.Phy.addLockMoverHitting(HITLOCK.DAMAGE, 8f);
			this.recheck_emot = true;
		}

		public void releaseFromIrisOut(bool return_position = false, PR.STATE _state = PR.STATE.DAMAGE_L_LAND, string setpose = "dmg_down2", bool fine_cam_position = true)
		{
			this.Phy.killSpeedForce(true, true, true, false, false).remLockGravity(this).clearColliderLock();
			this.Ser.Cure(SER.WORM_TRAPPED);
			this.NoDamage.AddAll(130f, 24U);
			base.killSpeedForce(true, true, false);
			if (return_position)
			{
				this.NM2D.CheckPoint.getPos(this);
			}
			if (this.poseIs("spike_trapped_down"))
			{
				this.setAim(CAim.get_aim2(0f, 0f, (float)X.MPF(X.xors(2) == 0), 0f, false), false);
			}
			else
			{
				this.setAim(CAim.get_aim2(0f, 0f, base.mpf_is_right, 0f, false), false);
			}
			this.changeState(_state);
			this.SpSetPose(setpose, -1, null, false);
			this.pre_camera_y = -1000f;
			if (this.Mp.M2D.transferring_game_stopping)
			{
				this.addD(M2MoverPr.DECL.DO_NOT_RESET_POS_ON_APPEAR);
			}
			else if (fine_cam_position && this.NM2D.Cam.getBaseMover() == this)
			{
				this.NM2D.Cam.fineImmediately();
			}
			this.Phy.addLockMoverHitting(HITLOCK.DAMAGE, 8f);
			this.recheck_emot = true;
			if (_state == PR.STATE.WATER_CHOKED_RELEASE && setpose == "water_choked_release2")
			{
				this.addD(M2MoverPr.DECL.INIT_A);
			}
		}

		public void executeWarpToChip(M2Puts Cp)
		{
			if (Cp == null)
			{
				return;
			}
			this.NM2D.Cam.blurCenterIfFocusing(this);
			this.Phy.killSpeedForce(true, true, true, false, false).remLockGravity(this);
			this.FootD.initJump(false, false, false);
			this.pre_camera_y = -1000f;
			base.killSpeedForce(true, true, false);
			this.setTo(Cp.mapcx + base.mpf_is_right * 0.125f, Cp.mbottom - this.sizey - 0.65f);
		}

		public override bool cannotHitTo(M2Mover Mv)
		{
			if (!(Mv is NelEnemy))
			{
				return false;
			}
			NelEnemy nelEnemy = Mv as NelEnemy;
			if (!this.can_applydamage_state())
			{
				return true;
			}
			AbsorbManager absorbManager = nelEnemy.getAbsorbManager();
			return (absorbManager != null && absorbManager.getTargetMover() == this) || this.Skill.cannotHitToEnemy(nelEnemy);
		}

		public void PeeLockReduceCheck(float ratio)
		{
			if (this.getEH(EnhancerManager.EH.juice_server))
			{
				ratio = X.Scr(ratio, 0.15f);
			}
			if (this.pee_lock > 0 && X.XORSP() < ratio)
			{
				this.pee_lock -= 1;
			}
		}

		public Stomach MyStomach
		{
			get
			{
				return this.NM2D.IMNG.StmNoel;
			}
		}

		public void addWaterDrunkCache(float stomach_progress, float water_total, int _juice_server = -1)
		{
			this.water_drunk_cache += (int)(stomach_progress * 100f);
			float num = 0f;
			if (_juice_server < 0)
			{
				_juice_server = (this.getEH(EnhancerManager.EH.juice_server) ? 1 : 0);
			}
			float num2 = X.NI(600, 200, (_juice_server > 0) ? (this.MyStomach.hasWater() ? 1f : 0.3f) : 0f);
			if ((float)this.water_drunk_cache >= num2)
			{
				num = X.NIL(0.4f, 1.4f, (float)this.water_drunk_cache - num2, 900f);
			}
			if (base.water_drunk >= 93)
			{
				num = X.Mx(num, 0.7f);
			}
			else if (base.water_drunk >= 72)
			{
				num = X.Mx(num, 0.25f);
			}
			if (num > 0f)
			{
				this.PeeLockReduceCheck(num);
				this.progressWaterDrunkCache(stomach_progress * num, false, false);
			}
		}

		public void progressWaterDrunkCache(float stomach_progress, bool is_battle_finish = false, bool is_water_drunk = false)
		{
			if (is_battle_finish)
			{
				stomach_progress = X.Mx(stomach_progress, 2f);
			}
			int num = (int)X.Mn((float)this.water_drunk_cache, stomach_progress * 100f);
			this.water_drunk_cache -= num;
			int num2 = X.IntC((float)num / 100f * 9f * (is_water_drunk ? 2.5f : 1f));
			if (is_battle_finish)
			{
				if (num2 < 20 && base.water_drunk < 80 && X.XORSP() < 0.66f)
				{
					num2 = X.Mx(num2, X.IntR(X.NIXP(5f, 10f)));
				}
				if (base.water_drunk >= 72)
				{
					num2 = X.Mx(num2, 5);
				}
			}
			bool flag = base.water_drunk >= 72;
			bool flag2 = base.water_drunk >= 93;
			int num3 = X.Mx(0, X.Mn(100 - base.water_drunk, num2));
			base.water_drunk += num3;
			this.juice_stock = X.Mn(NelItem.NoelJuice.stock - 1, this.juice_stock + (num2 - num3) / 8);
			bool flag3 = base.water_drunk >= 72;
			bool flag4 = base.water_drunk >= 93;
			this.Ser.checkSer();
			if (flag4 && !flag2)
			{
				UILog.Instance.AddAlert(TX.Get("Alert_almost_pee", ""), UILogRow.TYPE.ALERT);
				return;
			}
			if (flag3 && !flag)
			{
				UILog.Instance.AddAlert(TX.Get("Alert_near_pee", ""), UILogRow.TYPE.ALERT);
			}
		}

		protected float drunk_basic_level(bool is_alive)
		{
			float num;
			if (is_alive)
			{
				if (base.water_drunk >= 93)
				{
					num = 0.45f;
				}
				else if (base.water_drunk >= 72)
				{
					num = 0.22f;
				}
				else
				{
					num = 0.1f;
				}
			}
			else if (base.water_drunk >= 93)
			{
				num = 1f;
			}
			else if (base.water_drunk >= 72)
			{
				num = 0.6f;
			}
			else
			{
				num = 0.33f;
			}
			return num * X.NIL(0.5f, 1f, (float)base.water_drunk, 100f);
		}

		public bool checkNoelJuice(float basic_rat100, bool force_pee_splach = false, bool execute = true, int eh_juice_server = -1)
		{
			float num = X.Scr(basic_rat100 / 150f, this.drunk_basic_level(this.is_alive) * 60f / 100f);
			if (eh_juice_server < 0)
			{
				eh_juice_server = (this.getEH(EnhancerManager.EH.juice_server) ? 1 : 0);
			}
			if (eh_juice_server != 0)
			{
				num = X.Scr(num, 0.15f * (this.MyStomach.hasWater() ? 1f : 0.5f));
			}
			if (X.XORSP() < num)
			{
				this.juice_stock++;
				if (this.juice_stock >= NelItem.NoelJuice.stock)
				{
					if (this.Ser.getLevel(SER.NEAR_PEE) >= 2)
					{
						execute = false;
					}
					if (execute)
					{
						this.executeSplashNoelJuice(false, force_pee_splach, 0, false, false, false, false);
					}
					else
					{
						this.juice_stock = NelItem.NoelJuice.stock - 1;
					}
					return true;
				}
			}
			return false;
		}

		public void checkNoelJuiceOrgasm(float basic_rat01)
		{
			float num = basic_rat01 * X.NI(0.3f, 1f, X.XORSP()) * 0.4f;
			float num2 = this.drunk_basic_level(false) * 75f / 100f + X.Mx((!this.is_alive) ? ((float)NelItem.NoelJuice.stock * X.NIXP(0.4f, 1f)) : 0f, (float)this.juice_stock) / (float)NelItem.NoelJuice.stock;
			float num3 = X.Scr(num, num2);
			if (this.getEH(EnhancerManager.EH.juice_server))
			{
				num3 = X.Scr(num3, 0.25f);
			}
			if (X.XORSP() < num3)
			{
				this.executeSplashNoelJuice(num > num2, false, 0, false, false, false, false);
			}
		}

		public void executeSplashNoelJuice(bool orgasm_splash = false, bool force_pee_splash = false, int quality_add = 0, bool no_ptc = false, bool no_snd = false, bool no_item = false, bool no_cutin = false)
		{
			bool flag = false;
			UIPictureBase.EMWET emwet;
			if (this.Ser.has(SER.MILKY))
			{
				emwet = UIPictureBase.EMWET.MILK;
			}
			else if (CFG.sp_publish_juice == 0 && CFG.sp_publish_milk == 0)
			{
				emwet = UIPictureBase.EMWET.NORMAL;
				no_item = true;
			}
			else if (CFG.sp_publish_juice == 0)
			{
				emwet = UIPictureBase.EMWET.MILK;
			}
			else if (CFG.sp_publish_milk == 0)
			{
				emwet = UIPictureBase.EMWET.NORMAL;
			}
			else
			{
				emwet = ((X.xors((int)(CFG.sp_publish_milk + CFG.sp_publish_juice)) < (int)CFG.sp_publish_milk) ? UIPictureBase.EMWET.MILK : UIPictureBase.EMWET.NORMAL);
			}
			if (!no_item && !this.isPuzzleManagingMp() && (this.IMNG.hasEmptyBottle() || this.NM2D.isSafeArea()))
			{
				flag = true;
				this.obtainSplashedNoelJuice(emwet, quality_add);
			}
			this.juice_stock = 0;
			if (!X.SENSITIVE && this.EpCon.isOrgasmStarted(90) && X.XORSP() < 0.8f)
			{
				orgasm_splash = true;
			}
			string omorashiEventName = this.getOmorashiEventName();
			int num = base.water_drunk;
			if (!X.SENSITIVE && orgasm_splash)
			{
				num = -1;
				UILog.Instance.AddAlertTX((emwet == UIPictureBase.EMWET.MILK) ? "Alert_orgasm_splash_milk" : "Alert_orgasm_splash", UILogRow.TYPE.ALERT_EP);
			}
			else if (!X.SENSITIVE && (num > 0 || !flag || force_pee_splash))
			{
				UILog.Instance.AddAlertTX((emwet == UIPictureBase.EMWET.MILK) ? "Alert_pee_milk" : "Alert_pee", UILogRow.TYPE.ALERT_GRAY);
				num = 1;
			}
			else if (flag)
			{
				UILog.Instance.AddLog(TX.Get("Log_supply_juice", ""));
			}
			else if (base.water_drunk < 72 && TX.noe(omorashiEventName))
			{
				if (X.sensitive_level >= 1)
				{
					base.water_drunk += 5;
					this.Ser.checkSer();
				}
				return;
			}
			if (emwet == UIPictureBase.EMWET.NORMAL && num > 0)
			{
				emwet = UIPictureBase.EMWET.PEE;
			}
			this.EpCon.addPeeCount();
			this.pee_lock = 5;
			this.splashNoelJuiceEffect(emwet, num, no_ptc, no_snd, no_cutin);
			if (TX.valid(omorashiEventName))
			{
				EV.stack(omorashiEventName, 0, -1, this.Mp.Meta.Get("ev_omorashi_keys"), null);
			}
		}

		public void obtainSplashedNoelJuice(UIPictureBase.EMWET type, int quality_add = 0)
		{
			if (!this.is_alive)
			{
				return;
			}
			NelItem nelItem = ((type == UIPictureBase.EMWET.MILK) ? NelItem.NoelMilk : NelItem.NoelJuice);
			this.IMNG.dropManual(nelItem, nelItem.stock, quality_add + this.IMNG.getNoelJuiceQuality(1.35f) + this.EpCon.getNoelJuiceQualityAdd(), base.x, base.y, 0f, 0f, new META("force_absorb 1\ndrop_invisible 1\nno_get_effect 1"), false, NelItemManager.TYPE.NORMAL);
		}

		private void splashNoelJuiceEffect(UIPictureBase.EMWET wet_type, int ef_water_drunk = -1000, bool no_ptc = false, bool no_snd = false, bool no_cutin = false)
		{
			if (ef_water_drunk == -1000)
			{
				ef_water_drunk = base.water_drunk;
			}
			PostEffect.IT.setSlow(70f, 0.125f, 0);
			Vector3 hipPos = this.getHipPos();
			this.defineParticlePreVariableVagina();
			this.Mp.getEffect();
			if (!no_ptc)
			{
				this.PtcHld.Var("drunked", (double)ef_water_drunk).Var("_dx", (double)hipPos.x).Var("_dy", (double)hipPos.y);
				this.PtcHld.PtcSTTimeFixed((!X.SENSITIVE) ? "noel_juice_filled" : "noel_juice_filled_sensitive", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
			}
			if (!no_snd)
			{
				this.playSndPos("sxx_paizuri", 16);
				this.playSndPos((wet_type == UIPictureBase.EMWET.MILK) ? "sxx_sperm_shot_delayed" : "split_pee", 16);
			}
			this.EpCon.SituCon.addTempSituation((wet_type == UIPictureBase.EMWET.MILK) ? "_MILK" : "_PEE", 1, false);
			PostEffect.IT.addTimeFixedEffect(base.M2D.Cam.TeCon.setBounceZoomIn(1.5f, 40f, 0), 0.5f);
			PostEffect.IT.addTimeFixedEffect(base.M2D.Cam.TeCon.setBounceZoomIn(1.2f, 10f, 0), 1f);
			int num = X.Mn(base.water_drunk, 50);
			base.water_drunk -= num;
			int num2 = 50 - num;
			this.water_drunk_cache = (int)X.Mx(0f, (float)this.water_drunk_cache - (float)(num2 * 100) / 9f);
			float num3;
			this.MyStomach.progress((float)num * 0.05f + (float)num2 * 0.08f, true, out num3, true, true);
			if (!this.is_alive && X.XORSP() < 0.2f && base.water_drunk == 0)
			{
				base.water_drunk += 20;
			}
			this.Ser.Add(SER.SHAMED_WET, -1, 99, false);
			this.Ser.checkSer();
			this.playVo(this.getMouthVoiceReplace(0.7f, 1f) ? "mouth_split" : "split", false, false);
			if (!X.SENSITIVE)
			{
				this.BetoMng.wetten = true;
				this.NM2D.IMNG.fineSpecialNoelRow(this);
			}
			if (!no_cutin)
			{
				this.UP.applyWetten(wet_type, false);
			}
			if (this.EggCon.no_getout_exist)
			{
				this.EggCon.forcePushout(true, false);
			}
		}

		public void publishVaginaSplash(uint col, int count, float splash_speed_level = 1f)
		{
			this.Mp.DropCon.setLoveJuice(this, count, col, 1f, false);
		}

		public Vector3 publishVaginaSplashPiston(float shot_ax, bool no_snd = false)
		{
			Vector3 hipPos = this.getHipPos();
			base.PtcVar("_dx", (double)hipPos.x).PtcVar("_dy", (double)hipPos.y).PtcVar("ax", (double)shot_ax)
				.PtcST(no_snd ? "pr_piston_nosnd" : "pr_piston", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			return hipPos;
		}

		public void checkOazuke()
		{
			if (this.Mp.floort >= 60f && !this.Ser.has(SER.FRUSTRATED) && this.ep >= 700 && this.EpCon.getOrgasmedIndividualTotal() >= 5 && SCN.occurableOazuke() && X.XORSP() < 0.0625f)
			{
				this.Ser.Add(SER.FRUSTRATED, -1, 99, false);
			}
		}

		public override void cureEp(int val)
		{
			int ep = this.ep;
			base.cureEp(val);
			if (val < 0)
			{
				this.ep = (int)X.Mn((float)this.ep, X.Mx((float)ep, 700f));
			}
			if (this.ep != ep)
			{
				this.EpCon.fineCounter();
			}
		}

		public bool isFrozen()
		{
			return this.Ser.has(SER.FROZEN) && this.Ser.xSpeedRate() == 0f;
		}

		public bool isDoubleSexercise()
		{
			return this.Ser.has(SER.SEXERCISE) && this.Ser.EpApplyRatio() >= 3f;
		}

		public void runEpOrgasmCheck(float ts)
		{
			this.EpCon.run(ts);
			if (this.isFrozen() && !this.isTrappedState() && !this.isDamagingOrKo() && !this.isBurstState())
			{
				if (this.state != PR.STATE.FROZEN)
				{
					this.changeState(PR.STATE.FROZEN);
					return;
				}
			}
			else if (this.EpCon.isOrgasm() || this.EggCon.isLaying())
			{
				this.Ser.Cure(SER.SLEEP);
				if (this.EpCon.isOrgasm() && (this.changeOrgasmState(this.state) || (this.state == PR.STATE.LAYING_EGG && this.EpCon.isOrgasmInitTime())))
				{
					this.Skill.killHoldMagic(false);
					bool flag = this.isPoseDown(false) || this.isDownState();
					this.changeState(PR.STATE.ORGASM);
					if (!this.Anm.poseIs("must_orgasm", "must_orgasm_2"))
					{
						if (this.isPoseCrouch(false) || flag || this.Anm.poseIs("laying_egg", "stand2laying_egg"))
						{
							this.SpSetPose("orgasm_down", -1, null, false);
							return;
						}
						this.SpSetPose("stand2orgasm", -1, null, false);
						return;
					}
				}
			}
			else if (this.Ser.isSleepDown() && !this.isTrappedState() && !this.isDamagingOrKo() && !this.isBurstState() && this.state != PR.STATE.WATER_CHOKED_RELEASE && this.state != PR.STATE.WATER_CHOKED && this.state != PR.STATE.SLEEP)
			{
				bool flag2 = this.isPoseDown(false) || this.isDownState();
				this.Skill.killHoldMagic(false);
				this.changeState(PR.STATE.SLEEP);
				if (flag2)
				{
					this.Anm.setPose("sleepdown", -1, false);
					return;
				}
				this.Anm.setPose(this.isPoseCrouch(false) ? "sleep" : (this.Ser.isSleepDownBursted() ? "stand2fainted" : "stand2sleep"), -1, false);
			}
		}

		public void quitOrgasm()
		{
			this.Ser.Add(SER.TIRED, 240, 99, false);
			if (this.is_alive)
			{
				this.changeState(PR.STATE.NORMAL);
				return;
			}
			if (!this.poseIs(POSE_TYPE.ORGASM))
			{
				this.SpSetPose("down_u", -1, null, false);
			}
			this.changeState(PR.STATE.DAMAGE_L_LAND);
		}

		public bool initBenchSitDown(NelChipBench Cp, bool is_load_after = false, bool force_to_state_reset = false)
		{
			this.Phy.clampSpeed(FOCTYPE.WALK, -1f, -1f, 1f);
			this.Phy.killSpeedForce(true, true, true, false, false);
			this.Skill.BurstSel.clearExecuteCount();
			if (this.isBombHoldingState())
			{
				this.Skill.BombCancelCheck(null, true);
				this.changeState(PR.STATE.NORMAL);
			}
			if (Cp != null)
			{
				float num = (Cp.bottom_fix_ceil ? ((float)X.IntC(Cp.mbottom - 0.05f) - base.mbottom) : (Cp.mbottom - base.mbottom));
				this.moveBy(Cp.mapcx + Cp.shift_pixel * (float)X.MPF(!Cp.flip) / this.Mp.CLENB - base.x, num, true);
				this.Phy.addLockGravityFrame(2);
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC != null)
				{
					this.NM2D.CheckPoint.fineFoot(this, footBCC, true);
					this.FootD.shift_pixel_y = Cp.bottom_raise_px;
					this.FootD.no_change_shift_pixel = true;
				}
				if (Cp.player_aim != -1)
				{
					this.setAim((AIM)Cp.player_aim, false);
				}
				if (!is_load_after)
				{
					this.CureBench();
				}
				SVD.sFile currentFile = COOK.getCurrentFile();
				if (currentFile != null)
				{
					currentFile.assignRevertPosition(this.NM2D);
				}
				if (this.Mp.floort > 5f && !this.hasD(M2MoverPr.DECL.INIT_A))
				{
					this.Anm.setPose("stand2bench", -1, false);
				}
				this.Skill.killHoldMagic(false);
				this.Skill.BurstSel.clearExecuteCount();
				PR.STATE state = (is_load_after ? PR.STATE.BENCH_LOADAFTER : PR.STATE.BENCH);
				if (this.state != state || force_to_state_reset)
				{
					this.changeState(state);
				}
			}
			else
			{
				if (!this.isOnBench(true))
				{
					return false;
				}
				PxlSequence currentSequence = this.Anm.getCurrentSequence();
				if (!this.Anm.poseIs("stand2bench", "bench"))
				{
					this.Anm.setPose("bench", -1, false);
				}
				if (this.Anm.getCurrentSequence() != currentSequence)
				{
					this.Anm.animReset(this.Anm.getCurrentSequence().loop_to);
				}
			}
			this.GSaver.FineAll(true);
			if (!this.hasD(M2MoverPr.DECL.INIT_A))
			{
				this.recheck_emot_in_gm = true;
			}
			return true;
		}

		public void benchWaitInit(bool set_flag)
		{
			if (set_flag && this.state != PR.STATE.BENCH_SITDOWN_WAIT)
			{
				this.Skill.killHoldMagic(false);
				this.changeState(PR.STATE.BENCH_SITDOWN_WAIT);
				this.Phy.killSpeedForce(true, false, true, false, false);
				return;
			}
			if (!set_flag && this.state == PR.STATE.BENCH_SITDOWN_WAIT)
			{
				this.changeState(PR.STATE.NORMAL);
			}
		}

		public M2PrMasturbate initMasturbation(bool change_state = false, bool immediate = false)
		{
			if (immediate && this.Onnie == null)
			{
				if (this.ep > 0)
				{
					this.cureFull(false, true, false, false);
					this.NM2D.FlagOpenGm.Rem("MASTURBATE");
					this.UP.setFade("masturbate_orgasm", UIPictureBase.EMSTATE.ORGASM, false, true, false);
					if (this.isBenchState())
					{
						this.Anm.setPose("bench_must_orgasm_2", -1, false);
						this.runBenchOrGoRecovery(true);
						UiBenchMenu.orgasm_onemore = true;
					}
					else
					{
						this.Anm.setPose("must_orgasm_2", -1, false);
					}
					this.Anm.animReset(2);
					this.EpCon.initOrgasm(MDAT.EpAtkMasturbate2, true);
				}
			}
			else
			{
				new M2PrMasturbate(this, true);
				if (change_state && !this.isMasturbateState(this.state))
				{
					this.recheck_emot = true;
					if (this.isBenchState())
					{
						this.changeState(PR.STATE.BENCH_ONNIE);
					}
					else if (this.isNormalState(this.state))
					{
						this.changeState(PR.STATE.ONNIE);
					}
				}
			}
			return this.Onnie;
		}

		public void ApplySkillFixParameter(M2PrSkill.SkillApplyMem Sa)
		{
			this.maxhp = Sa.maxhp;
			this.maxmp = Sa.maxmp;
			this.Skill.finePuzzleManagingMp();
			UIStatus.Instance.redraw_bar_num = true;
			if (this.hp > 0)
			{
				if (this.hp < Sa.hp)
				{
					this.cureHp(Sa.hp - this.hp);
				}
				else
				{
					this.hp = X.Mx(1, Sa.hp);
					if (this.UP != null && this.UP.isActive())
					{
						UIStatus.Instance.fineHpRatio(false, false);
					}
				}
			}
			if (this.mp < Sa.mp)
			{
				this.cureMp2(Sa.mp - this.mp, false, 0f, false, true, false, false);
			}
			else
			{
				this.mp = Sa.mp;
			}
			this.fineMpClip(true, true);
			this.Ser.checkSer();
			this.GSaver.FineAll(false);
		}

		public override void evInit()
		{
			if (this.isBombHoldingState(this.state) && this.Skill.BombCancelCheck(null, true) && this.is_alive)
			{
				this.changeState(PR.STATE.NORMAL);
			}
			this.addD((M2MoverPr.DECL)4112);
			this.EpCon.lock_breath_progress = true;
			base.evInit();
		}

		public override void evQuit()
		{
			this.remD((M2MoverPr.DECL)4112);
			this.EpCon.lock_breath_progress = false;
			if (((this.simulate_key & 8U) == 0U || this.Anm.poseIs(POSE_TYPE.STAND, false)) && this.crouching > 0f && !base.forceCrouch(true, false))
			{
				this.quitCrouch(true, false, true);
			}
			bool move_script_attached = this.move_script_attached;
			base.evQuit();
			base.jump_hold_lock = true;
			if (move_script_attached)
			{
				this.Anm.releaseCache();
			}
			if (this.break_pose_fix_on_walk_level == 2)
			{
				this.break_pose_fix_on_walk_level = 3;
			}
			else
			{
				this.breakPoseFixOnWalk(1);
			}
			if (base.hasFoot())
			{
				this.NM2D.CheckPoint.fineFoot(this, this.FootD.get_FootBCC(), false);
			}
		}

		public void activateThrowRayForEvent(bool flag)
		{
			if (flag)
			{
				this.addD(M2MoverPr.DECL.EVENT_THROW_RAY);
				return;
			}
			this.remD(M2MoverPr.DECL.EVENT_THROW_RAY);
		}

		public void saveInit()
		{
			if (this.isBombHoldingState(this.state) && this.Skill.BombCancelCheck(null, true) && this.is_alive)
			{
				this.changeState(PR.STATE.NORMAL);
			}
		}

		public override void simulateKeyDown(string k, bool flag)
		{
			base.simulateKeyDown(k, flag);
			if (flag && (this.simulate_key & 3U) != 0U && base.isRunStopping())
			{
				base.stopRunning(false, false);
			}
		}

		public override bool readPtcScript(PTCThread rER)
		{
			if (base.destructed || this.PtcHld == null)
			{
				return rER.quitReading();
			}
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2021943417U)
				{
					if (num <= 868027710U)
					{
						if (num != 712044230U)
						{
							if (num == 868027710U)
							{
								if (cmd == "%SET_HAS_SER")
								{
									SER ser;
									if (FEnum<SER>.TryParse(rER._1, out ser, true))
									{
										rER.Def(rER._1, (float)(this.Ser.has(ser) ? 1 : 0));
									}
									return true;
								}
							}
						}
						else if (cmd == "%CALC_HIP_POS")
						{
							Vector2 vector = this.getHipPos();
							rER.Def("hipx", vector.x);
							rER.Def("hipy", vector.y);
							return true;
						}
					}
					else if (num != 1620874767U)
					{
						if (num == 2021943417U)
						{
							if (cmd == "%AIM")
							{
								rER.Def("aim", (float)this.Anm.getAim());
								rER.Def("ax", (float)CAim._XD(this.Anm.getAim(), 1));
								rER.Def("ay", (float)CAim._YD(this.Anm.getAim(), 1));
								return true;
							}
						}
					}
					else if (cmd == "%CALC_BODY_AGR")
					{
						rER.Def("bodyagR", this.Anm.body_agR + X.XORSPS() * rER.Nm(1, 0f));
						return true;
					}
				}
				else if (num <= 2829726627U)
				{
					if (num != 2796171389U)
					{
						if (num == 2829726627U)
						{
							if (cmd == "%TE_QU_SINV")
							{
								this.TeCon.setQuakeSinV(rER.Nm(1, 0f), rER.Int(2, 1), rER.Nm(3, 20f), rER.Nm(4, -1f), rER.Int(5, 0));
								return true;
							}
						}
					}
					else if (cmd == "%TE_QU_SINH")
					{
						this.TeCon.setQuakeSinH(rER.Nm(1, 0f), rER.Int(2, 1), rER.Nm(3, 20f), rER.Nm(4, -1f), rER.Int(5, 0));
						return true;
					}
				}
				else if (num != 3472483256U)
				{
					if (num == 4233139481U)
					{
						if (cmd == "%PVIB")
						{
							for (int i = 1; i < rER.clength; i++)
							{
								this.PadVib(rER.getIndex(i), 1f);
							}
							return true;
						}
					}
				}
				else if (cmd == "%TE_QU_VIB")
				{
					this.TeCon.setQuake(rER.Nm(1, 0f), rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
					return true;
				}
			}
			return base.readPtcScript(rER);
		}

		public void setStateToEvGacha()
		{
			this.changeState(PR.STATE.EV_GACHA);
		}

		public override bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			float effectSlowFactor = this.PtcHld.getEffectSlowFactor(Thread, Ef);
			if (effectSlowFactor > 0f)
			{
				PostEffect.IT.addTimeFixedEffect(Ef, effectSlowFactor);
			}
			return true;
		}

		public override M2Mover assignMoveScript(string str, bool soft_touch = false)
		{
			if (!this.is_alive)
			{
				if (!this.hasD(M2MoverPr.DECL.EVENT_THROW_RAY))
				{
					return this;
				}
				this.cureHp(1, true, true);
				if (this.NM2D.GameOver != null && this.NM2D.GameOver.isWaitingGiveup())
				{
					this.NM2D.GameOver.deactivate();
				}
				this.resetFlagsForGameOver();
			}
			if (!soft_touch)
			{
				this.Ser.Cure(SER.SHIELD_BREAK);
				this.Ser.Cure(SER.BURST_TIRED);
				this.Ser.Cure(SER.SLEEP);
				this.EpCon.cureOrgasmAfter();
				if (this.state == PR.STATE.ONNIE || this.state == PR.STATE.EV_GACHA || this.isDamagingOrKo() || this.isPiyoStunState() || this.isSleepState() || this.isSinkState())
				{
					this.changeState(PR.STATE.NORMAL);
				}
			}
			if (this.MScr == null || !this.MScr.isActive())
			{
				this.Skill.killHoldMagic(false);
				if (this.isOnBench(true))
				{
					this.changeState(PR.STATE.NORMAL);
					this.Anm.setPose(this.poseIs("bench_must_orgasm_2") ? "must_orgasm_2" : "bench2stand", -1, false);
				}
			}
			return base.assignMoveScript(str, soft_touch);
		}

		public override bool canOpenCheckEvent(IM2TalkableObject Tk)
		{
			return base.canOpenCheckEvent(Tk) && this.state == PR.STATE.NORMAL && (base.hasFoot() || !EnemySummoner.isActiveBorder()) && (!(Tk is M2EventItem) || (!this.isWormTrapped() && !this.Ser.has(SER.WORM_TRAPPED)));
		}

		public override void eventGreeting(M2EventItem Ev, int aim, int shift_pixel_x)
		{
			int num = ((aim == -1) ? 0 : CAim._XD(aim, 1));
			if (this.isNormalState() && base.isRunning())
			{
				base.stopRunning(false, false);
			}
			if (num != 0)
			{
				STB stb = TX.PopBld(null, 0);
				float num2 = (Ev.x + (Ev.sizex + 0.75f) * (float)num - base.x) * base.CLEN + (float)(shift_pixel_x * num);
				string text = FEnum<AIM>.ToStr(CAim.get_opposite((AIM)aim));
				if (!base.isRunning())
				{
					if (X.Abs(num2) < 2f)
					{
						stb += "@";
						stb += text;
						stb += " ! ";
					}
					else if (X.Abs(num2) < base.CLEN * 0.6f)
					{
						stb += " ! ";
					}
				}
				stb += "F >+[ ";
				stb += (int)num2;
				stb += ",0 <1.45 ]` w8 ";
				stb += "? @";
				stb += text;
				stb += " P[stand_ev~]  ";
				string text2 = stb.ToString();
				TX.ReleaseBld(stb);
				this.assignMoveScript(text2, false);
				return;
			}
			this.SpSetPose("stand_ev", -1, null, false);
		}

		public override bool isFacingEnemy()
		{
			return this.enemy_targetted > 0 || EnemySummoner.isActiveBorder();
		}

		public override bool cannotAccessToCheckEvent()
		{
			return this.Mp != null && (this.isTrappedState() || this.isAbsorbState(this.state) || this.isDamagingOrKo() || this.Skill.cannotAccessToCheckEvent() || this.EggCon.isLaying() || this.isMagicState(this.state) || this.isGaraakiState() || this.isOnBench(false) || (this.MistApply != null && this.MistApply.isWaterChokeDamageAlreadyApplied()));
		}

		public bool eventAbsorbBind(StringHolder rER)
		{
			if (!this.isAbsorbState())
			{
				rER.tError("プレイヤーが asborb されていません。");
				return false;
			}
			return this.AbsorbCon.eventAbsorbBind(rER);
		}

		public bool initMemorize(string memory_key)
		{
			return true;
		}

		public bool isPuzzleManagingMp()
		{
			return this.NM2D.Puz.isPuzzleManagingMp();
		}

		public float get_temp_puzzle_mp()
		{
			return this.Skill.get_temp_puzzle_mp();
		}

		public float get_temp_puzzle_max_mp()
		{
			return this.Skill.get_temp_puzzle_max_mp();
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			Rvi.time = ((this.NM2D.Puz.puzz_magic_count_max == -2) ? (-1) : ((int)this.get_temp_puzzle_mp()));
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			if (Rvi.time >= 0 && this.get_temp_puzzle_mp() < (float)Rvi.time)
			{
				this.Skill.initPuzzleManagingMp(Rvi.time);
			}
		}

		public void tempPuzzleMpStringCacheClear()
		{
			this._temp_puzzle_cache_str = null;
		}

		public void getPuzzleMpRatioStringForUi(STB Stb)
		{
			if (this.isPuzzleManagingMp())
			{
				switch (this.NM2D.Puz.puzz_magic_count_max)
				{
				case -2:
					break;
				case -1:
					Stb.Set("i");
					return;
				case 0:
					Stb.Set("---");
					return;
				default:
					Stb += X.IntR(this.get_temp_puzzle_mp() / 64f);
					Stb += "/";
					Stb += this.NM2D.Puz.puzz_magic_count_max;
					break;
				}
			}
		}

		public override void runInFloorPausing()
		{
			this.Skill.runInFloorPausing();
		}

		public void openSummoner(EnemySummoner Smn, bool is_active_border)
		{
		}

		public void closeSummoner(EnemySummoner Smn, bool defeated)
		{
			if (this.Ser == null)
			{
				return;
			}
			this.EpCon.flushCurrentBattle();
			this.EpCon.SituCon.flushLastExSituationTemp();
			this.GSaver.removeHeadDelay();
			this.EggCon.progressAfterBattle();
			if (this.Ser.getLevel(SER.SEXERCISE) >= 1)
			{
				this.Ser.Add(SER.FRUSTRATED, -1, 99, false);
			}
		}

		public override void getCameraCenterPos(ref float posx, ref float posy, float shiftx, float shifty, bool immediate, ref float follow_speed)
		{
			M2Camera cam = this.Mp.M2D.Cam;
			if (!immediate && follow_speed < 8f && this.isAbsorbState(this.state))
			{
				follow_speed *= -1f * X.NIL(3f, 0.2f, this.t_state, 50f);
			}
			if (this.NM2D.GM.isActive())
			{
				shifty += -30f * cam.getScaleRev();
			}
			base.getCameraCenterPos(ref posx, ref posy, shiftx, shifty, immediate, ref follow_speed);
			if (this.AbsorbCon.use_torture)
			{
				Vector2 damageCounterShiftMapPos = this.getDamageCounterShiftMapPos();
				shiftx += damageCounterShiftMapPos.x * this.Mp.CLEN;
				shifty += damageCounterShiftMapPos.y * this.Mp.CLEN;
			}
			this.Skill.getCameraCenterPos(ref posx, ref posy, shiftx, shifty);
			if (cam.FocusTo != null || this.isDamagingOrKo())
			{
				return;
			}
			this.pre_camera_y = posy;
		}

		public override Vector3 getHipPos()
		{
			return this.Anm.getHipPos();
		}

		public override void defineParticlePreVariable()
		{
			base.defineParticlePreVariable();
			this.PtcHld.Var("ax", (double)this.Anm.mpf_is_right);
		}

		public void defineParticlePreVariableVagina()
		{
			base.defineParticlePreVariable();
			this.PtcHld.Var("ax", (double)this.vagina_ax);
		}

		public float vagina_ax
		{
			get
			{
				if (!this.isPoseManguri(false))
				{
					return this.Anm.mpf_is_right * (float)X.MPF(!this.isPoseBack(false));
				}
				return X.NIXP(-0.023f, 0.023f);
			}
		}

		public float anm_mpf_is_right
		{
			get
			{
				return this.Anm.mpf_is_right;
			}
		}

		public override bool SpPoseIs(string pose)
		{
			return this.Anm.poseIs(pose);
		}

		public override PxlLayer[] SpGetPointsData(ref M2PxlAnimator MyAnimator, ref ITeScaler Scl, ref float rotation_plusR)
		{
			return this.Anm.SpGetPointsData(ref MyAnimator, ref Scl);
		}

		public void setPoseByEn(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			if (this.can_applydamage_state())
			{
				this.SpSetPose(nPose, reset_anmf, fix_change, sprite_force_aim_set);
			}
		}

		public override void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			if (fix_change == "")
			{
				this.fix_pose = null;
				fix_change = null;
			}
			if (fix_change != null)
			{
				this.fix_pose = fix_change;
				this.break_pose_fix_on_walk_level = 0;
				if (TX.isStart(fix_change, "!", 0))
				{
					fix_change = TX.slice(fix_change, 1);
				}
				nPose = fix_change;
			}
			else if (this.fix_pose != null)
			{
				return;
			}
			if (nPose != null)
			{
				this.Anm.setPose(nPose, reset_anmf, false);
			}
		}

		public bool frozenAnimReplaceable()
		{
			return !this.isFrozen() || (this.state != PR.STATE.FROZEN && this.t_state < 1.5f);
		}

		public override void SpMotionReset(int set_to_frame = 0)
		{
			this.Anm.animReset(set_to_frame);
		}

		public string setPoseNokezori(bool is_front, bool medium_damage, bool execute = true)
		{
			bool flag = this.is_crouch;
			string text;
			if (this.isPoseDown(false))
			{
				if (X.XORSP() < 0.85f)
				{
					text = ((this.isPoseBack(false) == X.XORSP() < 0.85f) ? "downdamage" : "downdamage_t");
					if (execute)
					{
						this.SpSetPose(text, 1, null, false);
					}
					return text;
				}
				flag = true;
			}
			if (!flag && X.XORSP() < 0.85f && this.isPoseCrouch(false))
			{
				flag = true;
			}
			if (is_front)
			{
				if (flag)
				{
					text = "dmg_crouch";
				}
				else
				{
					text = (medium_damage ? "damage_m" : "dmg_s");
				}
			}
			else
			{
				text = (flag ? "dmg_crouch_b" : "dmg_s_b");
			}
			if (execute)
			{
				this.SpSetPose(text, 1, null, false);
			}
			return text;
		}

		public bool isPoseDown(bool strict = false)
		{
			return this.Anm.poseIs(POSE_TYPE.DOWN, strict);
		}

		public bool isPoseBackDown(bool strict = false)
		{
			return this.isPoseDown(strict) && this.Anm.poseIs(POSE_TYPE.BACK, strict);
		}

		public bool isPoseBack(bool strict = false)
		{
			return this.Anm.poseIs(POSE_TYPE.BACK, strict);
		}

		public bool isPoseManguri(bool strict = false)
		{
			return this.Anm.poseIs(POSE_TYPE.MANGURI, strict);
		}

		public bool isPoseCrouch(bool only_consider_pose = false)
		{
			return (this.poseIs("crouch") && base.view_crouching) || this.Anm.poseIs(POSE_TYPE.CROUCH, false) || (this.Anm.poseIs("stunned", "stun2down") && this.Anm.cframe >= 2);
		}

		public bool isPoseCrouchStrict(bool only_consider_pose = false)
		{
			return (this.Anm.strictPoseIs("crouch") && base.view_crouching) || this.Anm.poseIs(POSE_TYPE.CROUCH, true) || ((this.Anm.strictPoseIs("stunned") || this.Anm.strictPoseIs("stun2down")) && this.Anm.cframe_strict >= 2);
		}

		protected override void OnCollisionStay2D(Collision2D col)
		{
			if (this.Mp != null && this.Mp.getTag(col.gameObject) == "Block" && this.ignoreHitToBlockCollider(null))
			{
				this.Phy.addCollirderLock(col.collider, 4f, this.FD_ignoreHitToBlockCollider, 1f);
				return;
			}
			base.OnCollisionStay2D(col);
		}

		public float getHpDamagePublishRatio(MagicItem Mg)
		{
			if (MDAT.isUsingItem(Mg))
			{
				return 1f;
			}
			if (!Mg.is_normal_attack)
			{
				return this.Ser.ChantAtkRate();
			}
			return this.Ser.AtkRate();
		}

		public float getCastingTimeScale(MagicItem Mg)
		{
			if (this.isPuzzleManagingMp())
			{
				return 1f;
			}
			float num = this.Ser.ChantSpeedRate();
			if (this.getCastableMp() <= 0f)
			{
				num *= this.Skill.pr_mp_hunder_chant_speed;
			}
			else
			{
				num *= this.Skill.pr_chant_speed;
			}
			return num * this.NM2D.NightCon.WindSpeed();
		}

		public float getMpDesireRatio(int add_mp)
		{
			int num;
			int num2;
			this.Skill.getOverChargeSlots().getChargeableMp(out num, out num2);
			float num3 = (float)(this.mp + add_mp + num) / (float)(this.maxmp + num2);
			if (this.Skill.mana_drain_lock_t > 0f)
			{
				num3 = (num3 + 0.25f) * 8f;
			}
			return num3;
		}

		public float getMpDesireMaxValue()
		{
			if (!this.isWormTrapped())
			{
				return 9f;
			}
			return 0f;
		}

		public bool playVo(string family, bool no_use_post = false, bool force_mouth_override = false)
		{
			if (family != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(family);
				if (num <= 1058693732U)
				{
					if (num <= 219797965U)
					{
						if (num != 35244156U)
						{
							if (num != 219797965U)
							{
								goto IL_0239;
							}
							if (!(family == "dmgx"))
							{
								goto IL_0239;
							}
							if (force_mouth_override || (this.AbsorbCon.isActive() && this.AbsorbCon.mouth_is_covered))
							{
								family = ((X.XORSP() < 0.2f) ? "mouth_fin" : "mouthl");
								goto IL_0239;
							}
							goto IL_0239;
						}
						else if (!(family == "dmgs"))
						{
							goto IL_0239;
						}
					}
					else if (num != 597717553U)
					{
						if (num != 1058693732U)
						{
							goto IL_0239;
						}
						if (!(family == "el"))
						{
							goto IL_0239;
						}
						goto IL_015C;
					}
					else
					{
						if (!(family == "cough"))
						{
							goto IL_0239;
						}
						if (this.isFrozen())
						{
							return false;
						}
						goto IL_0239;
					}
				}
				else
				{
					if (num <= 2341128772U)
					{
						if (num != 1176137065U)
						{
							if (num != 2341128772U)
							{
								goto IL_0239;
							}
							if (!(family == "orgasm"))
							{
								goto IL_0239;
							}
						}
						else
						{
							if (!(family == "es"))
							{
								goto IL_0239;
							}
							goto IL_012D;
						}
					}
					else if (num != 2602863636U)
					{
						if (num != 4179212881U)
						{
							goto IL_0239;
						}
						if (!(family == "dmgl"))
						{
							goto IL_0239;
						}
						goto IL_015C;
					}
					else if (!(family == "must_orgasm"))
					{
						goto IL_0239;
					}
					if (force_mouth_override || (this.AbsorbCon.isActive() && this.AbsorbCon.mouth_is_covered && X.XORSP() < X.NI(0.5f, 0.15f, (float)CFG.sp_epdmg_vo_iku * 0.01f)))
					{
						family = "mouth_fin";
						goto IL_0239;
					}
					if (CFG.sp_epdmg_vo_iku > 0 && X.xors(100) < (int)CFG.sp_epdmg_vo_iku)
					{
						family = "orgasm_iku";
						goto IL_0239;
					}
					goto IL_0239;
				}
				IL_012D:
				if (force_mouth_override || (this.AbsorbCon.isActive() && this.AbsorbCon.mouth_is_covered))
				{
					family = "mouth";
					goto IL_0239;
				}
				goto IL_0239;
				IL_015C:
				if (force_mouth_override || (this.AbsorbCon.isActive() && this.AbsorbCon.mouth_is_covered))
				{
					family = "mouthl";
				}
			}
			IL_0239:
			return this.VO.play(family, no_use_post) != null;
		}

		public void setVoiceOverrideAllowLevel(float f)
		{
			this.VO.ignore_prechn_stop = (float)CFG.sp_dmg_multivoice / 100f;
			this.VO.override_allow_level = X.Scr(f, this.VO.ignore_prechn_stop);
		}

		public override void moveByHitCheck(M2Phys AnotherPhy, FOCTYPE foctype, float map_dx, float map_dy)
		{
			if (base.hasFoot())
			{
				this.Phy.clipWalkXSpeed();
				if (AnotherPhy.Mv is NelEnemy)
				{
					this.addEnemySink((AnotherPhy.Mv as NelEnemy).sink_ratio, false, 0f);
				}
				base.moveByHitCheck(AnotherPhy, foctype | this.Skill.foc_cliff_stopper, map_dx, map_dy);
				return;
			}
			X.ZPOW(X.LENGTHXYS(AnotherPhy.x, AnotherPhy.y, base.x, base.y), AnotherPhy.Mv.sizey + AnotherPhy.Mv.sizex);
			this.Phy.addFoc(foctype, map_dx, X.Mx(0f, map_dy) * 0.25f, -1f, -1, 1, 0, -1, 0);
		}

		public void addEnemySink(float ratio = 1f, bool is_strong = false, float min_ratio_ser = 0f)
		{
			if (this.isMoveScriptActive(false))
			{
				return;
			}
			float num = X.Mx(min_ratio_ser, this.Ser.enemySinkRate());
			this.RCenemy_sink.Add(num * ratio, false, false);
			if (is_strong)
			{
				this.Skill.strongEnemySink();
			}
		}

		public float getCastableMp()
		{
			return (float)X.Mx(this.mp - this.Skill.getHoldingMp(false), 0);
		}

		public void getMouthPosition(out float _x, out float _y)
		{
			this.getMouthPosition(this.is_crouch || base.forceCrouch(false, false), this.isPoseDown(false), this.isPoseBackDown(false), this.Phy.walk_xspeed != 0f, out _x, out _y, this.Anm.poseIs(POSE_TYPE.MANGURI, false));
		}

		public void getMouthPosition(bool is_crouch, bool is_down, bool is_backdown, bool moving, out float _x, out float _y, bool manguri = false)
		{
			_x = this.drawx * this.Mp.rCLEN;
			float num = this.drawy * this.Mp.rCLEN;
			float num2 = num + this.sizey;
			float num3 = (float)(is_crouch ? 40 : (is_down ? 28 : 68)) * this.Mp.rCLENB;
			_y = (is_down ? (num2 - num3) : (num2 - num3 * 2f + (is_crouch ? (moving ? 28f : 17f) : 17f) / base.CLENM));
			if (manguri)
			{
				_y = num - (_y - num);
			}
		}

		public Vector2 getCenter()
		{
			return new Vector2(base.x, base.y);
		}

		public override Vector2 getTargetPos()
		{
			return this.Anm.getTargetRodPos();
		}

		public Vector2 getAimPos(MagicItem Mg)
		{
			return this.Skill.getAimPos(Mg);
		}

		public int getAimDirection()
		{
			return this.Skill.getAimDirection();
		}

		public float get_walk_xspeed()
		{
			return this.Phy.walk_xspeed;
		}

		public AIM getAimForCaster()
		{
			return this.aim;
		}

		public float get_state_time()
		{
			return this.t_state;
		}

		public bool canPullByWorm()
		{
			return this.canApplyGasDamage() && !this.AbsorbCon.use_torture && (this.state != PR.STATE.DAMAGE_LT_KIRIMOMI || this.t_state > 50f) && this.state != PR.STATE.WORM_TRAPPED;
		}

		public bool canApplyGasDamage()
		{
			return !this.isBurstState(this.state) && (!this.strong_throw_ray || this.isEvadeState()) && !this.isBenchOrBenchPreparingState();
		}

		public bool canSave(bool has_bench)
		{
			if (!this.is_alive || this.Mp == null)
			{
				return false;
			}
			if (this.MistApply != null && (this.MistApply.isWaterChoking() || (this.MistApply.isActive() && this.Phy.isin_water)))
			{
				return false;
			}
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			if (footBCC != null && !footBCC.is_map_bcc)
			{
				return false;
			}
			if (!this.isBenchState() && !has_bench)
			{
				M2BlockColliderContainer.BCCLine bccline = this.Mp.getSideBcc((int)base.mleft, (int)base.y, AIM.B);
				if (bccline == null && footBCC != null && X.BTW(footBCC.x, base.mleft, footBCC.right))
				{
					bccline = footBCC;
				}
				M2BlockColliderContainer.BCCLine bccline2 = this.Mp.getSideBcc((int)base.mright, (int)base.y, AIM.B);
				if (bccline2 == null && footBCC != null && X.BTW(footBCC.x, base.mright, footBCC.right))
				{
					bccline2 = footBCC;
				}
				if (bccline2 == null || bccline == null)
				{
					return false;
				}
				float num = bccline.slopeBottomY(base.mleft);
				float num2 = bccline2.slopeBottomY(base.mright);
				if (CCON.isDangerous(this.Mp.getConfig((int)base.mleft, (int)(num - 0.02f))) || CCON.isDangerous(this.Mp.getConfig((int)base.mright, (int)(num2 - 0.02f))) || CCON.isDangerous(this.Mp.getConfig((int)base.mleft, (int)(num - 1.02f))) || CCON.isDangerous(this.Mp.getConfig((int)base.mright, (int)(num2 - 1.02f))))
				{
					return false;
				}
			}
			return true;
		}

		public bool poseIs(string p)
		{
			return this.Anm.poseIs(p);
		}

		public bool poseIs(POSE_TYPE pose_type)
		{
			return this.Anm.poseIs(pose_type, false);
		}

		public bool poseIsBenchMusturbOrgasm()
		{
			return this.Anm.poseIs("bench_must_orgasm", "bench_must_orgasm_2");
		}

		public override bool is_crouch
		{
			get
			{
				return this.canCrouchState(this.state) != 0 && base.is_crouch;
			}
		}

		public void setAimForCaster(AIM a)
		{
			this.setAim(a, true);
		}

		public float getPoseAngleRForCaster()
		{
			return (0.5f - (this.isPoseDown(false) ? (this.isPoseBackDown(false) ? 0.9f : (-0.1f)) : 0.45f) * (float)X.MPF(CAim._XD(this.aim, 1) > 0)) * 3.1415927f;
		}

		public override bool isNormalState()
		{
			return this.isNormalState(this.state);
		}

		public bool isNormalState(PR.STATE st)
		{
			return st == PR.STATE.NORMAL;
		}

		public bool isMagicState()
		{
			return this.isMagicState(this.state);
		}

		public bool isMagicState(PR.STATE st)
		{
			return st == PR.STATE.MAG_EXPLODE_PREPARE || st == PR.STATE.MAG_EXPLODED;
		}

		public bool isEvadeState()
		{
			return this.isEvadeState(this.state);
		}

		public bool isEvadeState(PR.STATE st)
		{
			return st == PR.STATE.EVADE || st == PR.STATE.UKEMI || st == PR.STATE.EVADE_SHOTGUN || st == PR.STATE.UKEMI_SHOTGUN;
		}

		public bool isSlidingState(PR.STATE st)
		{
			return st == PR.STATE.SLIDING;
		}

		public bool isMagicExistState(PR.STATE st)
		{
			return this.isMagicState(st) || this.isNormalState(st);
		}

		public bool isMagicExistState()
		{
			return this.isMagicExistState(this.state);
		}

		public bool isSleepingDownState()
		{
			return this.isSleepingDownState(this.state);
		}

		public bool isSleepState()
		{
			return this.state == PR.STATE.SLEEP;
		}

		public bool isSleepingDownState(PR.STATE st)
		{
			return st == PR.STATE.LAYING_EGG || st == PR.STATE.ORGASM || (st == PR.STATE.WATER_CHOKED_RELEASE && this.isPoseDown(false)) || this.isSleepState() || st == PR.STATE.GAMEOVER_RECOVERY;
		}

		public int canCrouchState(PR.STATE st)
		{
			if (st == PR.STATE.SLIDING || st == PR.STATE.DOWN_STUN || st == PR.STATE.SHIELD_BREAK_STUN || st == PR.STATE.ONNIE)
			{
				return 2;
			}
			if (st != PR.STATE.NORMAL && st != PR.STATE.DAMAGE && st != PR.STATE.COMET && st != PR.STATE.COMET_SHOTGUN && st != PR.STATE.WHEEL && st != PR.STATE.WHEEL_SHOTGUN && !this.isBenchState(st) && !this.isSleepingDownState(st))
			{
				return 0;
			}
			return 1;
		}

		public bool isBenchState()
		{
			return this.isBenchState(this.state);
		}

		public bool isBenchState(PR.STATE st)
		{
			return st == PR.STATE.BENCH || st == PR.STATE.BENCH_LOADAFTER || st == PR.STATE.BENCH_ONNIE;
		}

		public bool isBenchOrBenchPreparingState()
		{
			return this.isBenchOrGoRecoveryState(this.state) || this.state == PR.STATE.BENCH_SITDOWN_WAIT;
		}

		public bool isBenchOrGoRecoveryState()
		{
			return this.isBenchOrGoRecoveryState(this.state);
		}

		public bool isBenchOrGoRecoveryState(PR.STATE st)
		{
			return this.isBenchState(st) || st == PR.STATE.GAMEOVER_RECOVERY;
		}

		public bool isCrouchWideState()
		{
			return this.state == PR.STATE.ONNIE || this.state == PR.STATE.SHIELD_BREAK_STUN;
		}

		public bool isMasturbateState()
		{
			return this.isMasturbateState(this.state);
		}

		public bool isMasturbateState(PR.STATE st)
		{
			return st == PR.STATE.BENCH_ONNIE || st == PR.STATE.ONNIE;
		}

		public bool isOnBench(bool only_state_check = false)
		{
			if (this.isBenchState(this.state))
			{
				if (only_state_check)
				{
					return true;
				}
				if (this.getNearBench(false, false) != null)
				{
					return true;
				}
			}
			return false;
		}

		public NelChipBench getNearBench(bool is_wide = false, bool force = false)
		{
			if (this.EggCon == null || this.Mp == null || !this.Mp.point_prepared)
			{
				return null;
			}
			if (((!this.isDamagingOrKo() && !this.isLayingEggOrOrgasm() && !this.isBurstState() && !this.EggCon.hasNearlyLayingEgg()) || force) && !EnemySummoner.isActiveBorder() && this.Mp.point_prepared)
			{
				NelChipBench nelChipBench;
				if (!is_wide)
				{
					nelChipBench = this.Mp.findChip((int)base.x, (int)(base.mbottom - 0.25f), "bench") as NelChipBench;
				}
				else
				{
					nelChipBench = null;
					using (BList<M2Puts> blist = ListBuffer<M2Puts>.Pop(1))
					{
						this.Mp.getAllPointMetaPutsTo((int)base.x - 3, (int)base.mbottom - 6, 7, 9, blist, "bench");
						float num = -1f;
						for (int i = blist.Count - 1; i >= 0; i--)
						{
							NelChipBench nelChipBench2 = blist[i] as NelChipBench;
							if (nelChipBench2 != null && nelChipBench != nelChipBench2)
							{
								float num2 = X.LENGTHXYS(base.x, base.mbottom - 0.25f, nelChipBench2.mapcx, nelChipBench2.mapcy);
								if (num < 0f || num > num2)
								{
									nelChipBench = nelChipBench2;
									num = num2;
								}
							}
						}
					}
				}
				if (nelChipBench != null && !nelChipBench.active_removed && !nelChipBench.Lay.unloaded)
				{
					return nelChipBench;
				}
			}
			return null;
		}

		public PR.STATE get_current_state()
		{
			return this.state;
		}

		public bool secureTimeState()
		{
			return this.isOnBench(false) || !Map2d.can_handle;
		}

		public bool changeLayingEggState(PR.STATE st)
		{
			if (st == PR.STATE.DAMAGE_L_LAND || st == PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER || st == PR.STATE.DAMAGE_OTHER_STUN || st == PR.STATE.DOWN_STUN || st == PR.STATE.DAMAGE_LT_LAND)
			{
				return this.canJump() && this.t_state >= 24f;
			}
			if (st == PR.STATE.ORGASM)
			{
				return !this.Ser.has(SER.ORGASM_INITIALIZE);
			}
			return !this.isAbsorbState(st) && !this.isBenchState(st) && !this.isBurstState(st) && (st == PR.STATE.NORMAL || this.isMagicState(st) || this.isEvadeState(st) || (this.isDownState(st) && this.canJump() && st != PR.STATE.LAYING_EGG));
		}

		public bool canApplyParalysisAttack()
		{
			return this.state != PR.STATE.DAMAGE && !this.isBurstState(this.state);
		}

		public bool changeOrgasmState(PR.STATE st)
		{
			return st != PR.STATE.LAYING_EGG && st != PR.STATE.ORGASM && this.changeLayingEggState(st);
		}

		public bool isBikubikuState()
		{
			return this.state == PR.STATE.LAYING_EGG || this.state == PR.STATE.ORGASM;
		}

		public override bool isDamagingOrKo()
		{
			return this.isDamagingOrKo(this.state);
		}

		public bool isDamagingOrKo(PR.STATE st)
		{
			return st >= PR.STATE.DAMAGE && st < (PR.STATE)6000;
		}

		public bool isDmgHuttobiState(PR.STATE st)
		{
			return st == PR.STATE.DAMAGE_L || st == PR.STATE.DAMAGE_LT_KIRIMOMI || st == PR.STATE.DAMAGE_LT;
		}

		public bool isBurstFastState()
		{
			return this.isDamagingOrKo() || X.BTWW(430f, (float)this.state, 500f) || this.Ser.has(SER.BURNED) || this.Ser.has(SER.FROZEN);
		}

		public bool isFrozenState()
		{
			return this.state == PR.STATE.FROZEN;
		}

		public bool isInputtableDownState()
		{
			return this.isInputtableDownState(this.state);
		}

		public bool isInputtableDownState(PR.STATE st)
		{
			return st == PR.STATE.DAMAGE_L_LAND || st == PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER || st == PR.STATE.DAMAGE_LT_LAND;
		}

		public bool isDownState(PR.STATE st)
		{
			if (st == PR.STATE.DAMAGE_BURNED)
			{
				return this.hasD(M2MoverPr.DECL.FLAG0);
			}
			return this.isLayingEggOrOrgasm(st) || this.isInputtableDownState(st) || this.isSleepingDownState(st);
		}

		public bool isConnectableRunState(PR.STATE st)
		{
			return this.isMagicState(st) || this.isSlidingState(st) || this.isPunchState(st) || this.isEvadeState(st);
		}

		public bool isAbsorbState(PR.STATE st)
		{
			int num = 4600;
			return num <= (int)st && st <= num + (PR.STATE)99;
		}

		public bool isAbsorbState()
		{
			return this.isAbsorbState(this.state);
		}

		public bool isCrouchingState()
		{
			return this.state == PR.STATE.SLIDING;
		}

		public bool isTortureAbsorbed()
		{
			return this.AbsorbCon.use_torture;
		}

		public override bool canApplyKnockBack()
		{
			if (this.AbsorbCon.cannot_move)
			{
				return false;
			}
			PR.STATE state = this.state;
			if (state <= PR.STATE.DASHPUNCH_SHOTGUN)
			{
				if (state - PR.STATE.WHEEL > 1 && state - PR.STATE.DASHPUNCH > 1)
				{
					goto IL_0049;
				}
			}
			else if (state != PR.STATE.SHIELD_BUSH)
			{
				if (state != PR.STATE.DAMAGE_LT_KIRIMOMI)
				{
					goto IL_0049;
				}
			}
			else
			{
				if (this.hasD(M2MoverPr.DECL.FLAG_HIT))
				{
					return false;
				}
				goto IL_006F;
			}
			return false;
			IL_0049:
			if (this.isDmgHuttobiState(this.state) || this.isTrappedState() || this.isBurstState(this.state))
			{
				return false;
			}
			IL_006F:
			return base.canApplyKnockBack();
		}

		public override bool isManipulateState()
		{
			return this.isMagicExistState(this.state) || this.isEvadeState(this.state) || this.isPunchState(this.state);
		}

		public bool isPiyoStunState()
		{
			return this.state == PR.STATE.SHIELD_BREAK_STUN || this.state == PR.STATE.ORGASM;
		}

		public bool isDownState()
		{
			return this.isDownState(this.state);
		}

		public bool isBurstAllocState()
		{
			return !this.isBurstState(this.state) && !this.isBenchOrBenchPreparingState() && this.state != PR.STATE.WORM_TRAPPED && !this.NM2D.Iris.isIrisActive();
		}

		public bool isBurstState()
		{
			return this.isBurstState(this.state);
		}

		public bool isBurstState(PR.STATE state)
		{
			return state == PR.STATE.BURST || state == PR.STATE.BURST_SCAPECAT;
		}

		public bool isPunchState()
		{
			return this.isPunchState(this.state);
		}

		public bool isPunchState(PR.STATE st)
		{
			if (st <= PR.STATE.DASHPUNCH_SHOTGUN)
			{
				if (st != PR.STATE.PUNCH && st != PR.STATE.SLIDING && st - PR.STATE.DASHPUNCH > 1)
				{
					goto IL_002E;
				}
			}
			else if (st != PR.STATE.SHIELD_BUSH && st - PR.STATE.SHIELD_COUNTER > 1 && st != PR.STATE.USE_BOMB)
			{
				goto IL_002E;
			}
			return true;
			IL_002E:
			return this.isAirPunchState(st) || this.isBurstState(st);
		}

		public bool isFixAimState(PR.STATE st)
		{
			return st == PR.STATE.SLIDING || st - PR.STATE.DASHPUNCH <= 1;
		}

		public bool isShotgunState()
		{
			return this.isShotgunState(this.state);
		}

		public bool isShotgunState(PR.STATE st)
		{
			PR.STATE state = this.state;
			switch (state)
			{
			case PR.STATE.WHEEL_SHOTGUN:
			case PR.STATE.COMET_SHOTGUN:
			case PR.STATE.DASHPUNCH_SHOTGUN:
			case PR.STATE.AIRPUNCH_SHOTGUN:
				break;
			case PR.STATE.COMET:
			case PR.STATE.DASHPUNCH:
			case PR.STATE.AIRPUNCH:
				return false;
			default:
				if (state != PR.STATE.SHIELD_COUNTER_SHOTGUN)
				{
					return false;
				}
				break;
			}
			return true;
		}

		public bool isAirPunchState()
		{
			return this.isAirPunchState(this.state);
		}

		public bool isAirPunchState(PR.STATE st)
		{
			PR.STATE state = this.state;
			return state - PR.STATE.WHEEL <= 3 || state - PR.STATE.AIRPUNCH <= 1;
		}

		public bool isSpecialPunchComet()
		{
			return this.state == PR.STATE.COMET || this.state == PR.STATE.COMET_SHOTGUN;
		}

		public bool isSpecialPunchState()
		{
			return this.isSpecialPunchState(this.state);
		}

		public bool isSpecialPunchState(PR.STATE st)
		{
			return st != PR.STATE.PUNCH && this.isPunchState(st);
		}

		public bool isShieldAttackState()
		{
			return this.isShieldAttackState(this.state);
		}

		public bool isShieldAttackState(PR.STATE st)
		{
			return st - PR.STATE.SHIELD_BUSH <= 3;
		}

		public override bool isBusySituation()
		{
			return this.Mp != null && (this.Phy.isin_water || this.isFacingEnemy() || this.cannotAccessToCheckEvent() || (this.MistApply != null && !this.MistApply.isCuring()));
		}

		public bool canChokedByWaterState()
		{
			return this.state == PR.STATE.WATER_CHOKED_RELEASE || (this.isSleepingDownState() && this.state != PR.STATE.GAMEOVER_RECOVERY) || (!this.isTrappedState() && this.state != PR.STATE.DAMAGE_BURNED && !this.AbsorbCon.isActive() && !this.isGaraakiState());
		}

		public override bool isTrappedState()
		{
			return this.isTrappedState(this.state);
		}

		public bool isTrappedState(PR.STATE _state)
		{
			return _state == PR.STATE.WORM_TRAPPED || _state == PR.STATE.WATER_CHOKED;
		}

		public override bool isGaraakiState()
		{
			return this.isGaraakiState(this.state);
		}

		public bool isGaraakiState(PR.STATE st)
		{
			return st == PR.STATE.SHIELD_BREAK_STUN || st == PR.STATE.WATER_CHOKED_RELEASE || st == PR.STATE.FROZEN || this.isSleepingDownState(st) || this.isMasturbateState(st);
		}

		public bool isFlyingDamageState(PR.STATE st)
		{
			return st == PR.STATE.DAMAGE_L;
		}

		public bool isWaterChokedReleaseState()
		{
			return this.state == PR.STATE.WATER_CHOKED_RELEASE;
		}

		public bool canApplyMistDamage()
		{
			return !this.isBurstState() && !this.AbsorbCon.cannot_apply_mist_damage;
		}

		public virtual bool canUseBombState(NelItem Itm)
		{
			return (this.isNormalState() || this.state == PR.STATE.PUNCH || this.isMagicExistState(this.state) || this.isBombHoldingState(this.state)) && !EV.isActive(false);
		}

		public bool isBombHoldingState()
		{
			return this.isBombHoldingState(this.state);
		}

		public bool isBombHoldingState(PR.STATE state)
		{
			return state == PR.STATE.USE_BOMB;
		}

		public bool isWormTrapped()
		{
			return this.isWormTrapped(this.state);
		}

		public bool isWormTrapped(PR.STATE st)
		{
			return st == PR.STATE.WORM_TRAPPED;
		}

		public bool isBurnedState()
		{
			return this.isBurnedState(this.state);
		}

		public bool isBurnedState(PR.STATE st)
		{
			return st == PR.STATE.DAMAGE_BURNED;
		}

		public bool isBurstDisable()
		{
			return this.isBurstState() || this.state == PR.STATE.WATER_CHOKED || this.Ser.has(SER.BURST_TIRED);
		}

		public bool isPressDamageState(PR.STATE st)
		{
			return this.state == PR.STATE.DAMAGE_PRESS_LR || this.state == PR.STATE.DAMAGE_PRESS_TB;
		}

		public bool isUiPressDamage()
		{
			return this.isPressDamageState(this.state) && this.hasD(M2MoverPr.DECL.INIT_A);
		}

		public bool isSinkState()
		{
			return this.isSinkState(this.state);
		}

		public bool isSinkState(PR.STATE st)
		{
			return st == PR.STATE.ENEMY_SINK;
		}

		public bool isLayingEgg()
		{
			return this.isLayingEgg(this.state);
		}

		public bool isLayingEgg(PR.STATE st)
		{
			return st == PR.STATE.LAYING_EGG;
		}

		public bool isLayingEggOrOrgasm()
		{
			return this.isOrgasm() || this.isLayingEgg();
		}

		public bool isLayingEggOrOrgasm(PR.STATE st)
		{
			return this.isOrgasm(st) || this.isLayingEgg(st);
		}

		public bool isOrgasm()
		{
			return this.isOrgasm(this.state);
		}

		public bool isOrgasm(PR.STATE st)
		{
			return st == PR.STATE.ORGASM;
		}

		public bool isOrgasmFinishDown()
		{
			return this.state == PR.STATE.ORGASM && !this.EpCon.isOrgasm();
		}

		public bool isGameoverRecover()
		{
			return this.state == PR.STATE.GAMEOVER_RECOVERY;
		}

		public bool throw_ray
		{
			get
			{
				return this.hasD((M2MoverPr.DECL)12800);
			}
		}

		public bool strong_throw_ray
		{
			get
			{
				return this.hasD((M2MoverPr.DECL)4608);
			}
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			bool flag = false;
			if (Ray != null && Ray.Atk != null && M2NoDamageManager.isMapDamageKey(Ray.Atk.ndmg))
			{
				flag = true;
			}
			if (flag ? this.strong_throw_ray : this.throw_ray)
			{
				return RAYHIT.NONE;
			}
			return (RAYHIT)3;
		}

		private void calcAttributeDamageRatio(ref float base_level, float re_level)
		{
			if (re_level >= 0f)
			{
				base_level = X.Mn(base_level, X.NI(1f, 0.25f, re_level));
				return;
			}
			base_level *= X.NI(1f, 2f, -re_level);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			if (this.isPressDamageState(this.state) && this.NM2D.Iris.isWaiting(this, IrisOutManager.IRISOUT_TYPE.PRESS))
			{
				if (Atk != null)
				{
					Atk._apply_knockback_current = false;
				}
				return 0f;
			}
			if ((Atk == null || !this.NoDamage.isActive(Atk.ndmg)) && (this.is_alive || this.overkill) && this.can_applydamage_state())
			{
				float num = this.Ser.HpDamageRate();
				float num2 = 1f;
				if (this.AbsorbCon.isActive())
				{
					float re = this.getRE(RecipeManager.RPI_EFFECT.ARREST_HPDAMAGE_REDUCE);
					if (re >= 0f)
					{
						num2 = X.NI(1f, 0.25f, re);
					}
					else
					{
						num *= X.NI(1f, 2f, -re);
					}
				}
				if (Atk != null)
				{
					MGATTR attr = Atk.attr;
					if (attr != MGATTR.BOMB)
					{
						switch (attr)
						{
						case MGATTR.FIRE:
							break;
						case MGATTR.ICE:
							this.calcAttributeDamageRatio(ref num2, this.getRE(RecipeManager.RPI_EFFECT.FROZEN_DAMAGE_REDUCE));
							goto IL_011A;
						case MGATTR.THUNDER:
							this.calcAttributeDamageRatio(ref num2, this.getRE(RecipeManager.RPI_EFFECT.ELEC_DAMAGE_REDUCE));
							goto IL_011A;
						default:
							goto IL_011A;
						}
					}
					this.calcAttributeDamageRatio(ref num2, this.getRE(RecipeManager.RPI_EFFECT.FIRE_DAMAGE_REDUCE));
				}
				IL_011A:
				if (Atk is NelAttackInfo)
				{
					MagicItem publishMagic = (Atk as NelAttackInfo).PublishMagic;
					if (publishMagic != null && publishMagic.Caster is PR && publishMagic.kind == MGKIND.ITEMBOMB_NORMAL)
					{
						num2 *= DIFF.itembomb_pr_apply_damage_ratio();
					}
				}
				return num * num2;
			}
			if (Atk != null)
			{
				Atk._apply_knockback_current = false;
			}
			return 0f;
		}

		public bool canApplyAbrosb()
		{
			return this.state != PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER && !this.isBurstState(this.state);
		}

		public string canUseItem(NelItem Itm, ItemStorage Storage, bool for_using = true)
		{
			string text = "";
			if (for_using)
			{
				if (Itm.is_bomb)
				{
					if (Storage.infinit_stockable)
					{
						text = TX.add(text, TX.Get("Item_desc_cannot_use_bomb_from_storage", ""), "\n");
					}
					if (!this.canUseBombState(Itm) || this.NM2D.isDangerItemDisable())
					{
						text = TX.add(text, TX.Get("Alert_bench_execute_scenario_locked", ""), "\n");
					}
					return text;
				}
				if (Itm.key == "scapecat")
				{
					if (!this.canUseScapecat())
					{
						return TX.Get("Alert_bench_execute_scenario_locked", "");
					}
					return "";
				}
				else if (!DIFF.alloc_useitem_in_magic && ((this.isMagicExistState(this.state) && !this.isNormalState()) || this.isBurstState()))
				{
					text = TX.add(text, TX.Get("Item_desc_decline_holding_magic", ""), "\n");
				}
			}
			if (this.is_alive && (this.isDamagingOrKo() || this.isGaraakiState()))
			{
				text = TX.add(text, TX.Get("Item_desc_decline_dmg", ""), "\n");
			}
			else if (!this.is_alive)
			{
				text = TX.add(text, TX.Get("Item_desc_decline_dead", ""), "\n");
			}
			return text;
		}

		public bool canUseScapecat()
		{
			return !this.is_alive && !this.isBurstState(this.state);
		}

		public bool NelItemUseableInt(NelItem Itm, NelItem.CATEG categ, int grade, int v_int)
		{
			if (!this.is_alive)
			{
				return false;
			}
			if (this.Ser.has(SER.CONFUSE))
			{
				return true;
			}
			if (categ <= NelItem.CATEG.CURE_MP)
			{
				if (categ == NelItem.CATEG.CURE_HP)
				{
					return base.get_hp() < base.get_maxhp();
				}
				if (categ == NelItem.CATEG.CURE_MP)
				{
					return base.get_mp() < base.get_maxmp() - (float)this.EggCon.total;
				}
			}
			else
			{
				if (categ == NelItem.CATEG.CURE_EP)
				{
					return true;
				}
				if (categ == NelItem.CATEG.CURE_MP_CRACK)
				{
					return this.GaugeBrk.isActive();
				}
			}
			return false;
		}

		public void NelItemUseInt(NelItem Itm, NelItem.CATEG categ, int grade, int v_int, ref int ef_delay, ref string play_snd, ref bool quit_flag)
		{
			string text = "item_cure";
			if (categ <= NelItem.CATEG.CURE_MP)
			{
				if (categ != NelItem.CATEG.CURE_HP)
				{
					if (categ != NelItem.CATEG.CURE_MP)
					{
						return;
					}
					this.cureMp(v_int, true, true, true);
					if (this.PtcHld != null && !this.NM2D.GM.isActive())
					{
						string text2 = "delay";
						int num = ef_delay;
						ef_delay = num + 1;
						base.PtcVar(text2, (double)(num * 14));
						this.PtcHld.PtcSTTimeFixed("bench_cure_mp", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, false);
					}
				}
				else
				{
					this.cureHp(v_int);
					if (this.PtcHld != null && !this.NM2D.GM.isActive())
					{
						string text3 = "delay";
						int num = ef_delay;
						ef_delay = num + 1;
						base.PtcVar(text3, (double)(num * 14));
						this.PtcHld.PtcSTTimeFixed("bench_cure_hp", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, false);
					}
				}
			}
			else if (categ != NelItem.CATEG.CURE_EP)
			{
				if (categ != NelItem.CATEG.CURE_MP_CRACK)
				{
					if (categ != NelItem.CATEG.BOMB)
					{
						return;
					}
					quit_flag = true;
					this.getSkillManager().initItemBomb(Itm, grade, this.NM2D.IMNG.getInventory());
					text = "itembomb_initialize";
				}
				else
				{
					this.GaugeBrk.cureByItem(v_int);
					if (this.PtcHld != null && !this.NM2D.GM.isActive())
					{
						string text4 = "delay";
						int num = ef_delay;
						ef_delay = num + 1;
						base.PtcVar(text4, (double)(num * 14));
						this.PtcHld.PtcSTTimeFixed("bench_cure_torned", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, false);
					}
				}
			}
			else
			{
				this.cureEp(v_int);
				if (this.PtcHld != null && !this.NM2D.GM.isActive())
				{
					string text5 = "delay";
					int num = ef_delay;
					ef_delay = num + 1;
					base.PtcVar(text5, (double)(num * 14));
					this.PtcHld.PtcSTTimeFixed("bench_cure_torned", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, false);
				}
				string omorashiEventName = this.getOmorashiEventName();
				int num2 = 0;
				if (this.getEH(EnhancerManager.EH.juice_server) && (Itm.RecipeInfo == null || (Itm.RecipeInfo.categ & RecipeManager.RPI_CATEG.FROMNOEL) == (RecipeManager.RPI_CATEG)0))
				{
					num2 = 1;
				}
				float num3 = ((num2 > 0) ? (this.MyStomach.hasWater() ? 1f : 0.38f) : 0f);
				float num4 = 0.4f + num3 + (this.Ser.has(SER.DRUNK) ? 1f : 0f);
				if (omorashiEventName != null)
				{
					num4 *= 2f;
				}
				this.addWaterDrunkCache(num4, this.MyStomach.water_drunk_level, num2);
				if (omorashiEventName != null && this.checkNoelJuice(X.NI(4, 30, num3) + (float)(this.Ser.has(SER.DRUNK) ? 35 : 0), false, false, num2))
				{
					int level = this.Ser.getLevel(SER.NEAR_PEE);
					if (level == -1)
					{
						base.water_drunk = X.Mx(base.water_drunk, 72);
						this.Ser.checkSerExecute(true, true);
					}
					else if (level == 0)
					{
						base.water_drunk = X.Mx(base.water_drunk, 93);
						this.Ser.checkSerExecute(true, true);
					}
					else if (this.isEventInjectable(false))
					{
						EV.getVariableContainer().define("omorashi_water_drunk", "1", true);
						EV.stack(omorashiEventName, 0, -1, this.Mp.Meta.Get("ev_omorashi_keys"), null);
						quit_flag = true;
					}
				}
				else if (v_int > 0 && this.Ser.has(SER.DRUNK))
				{
					this.cureSerDrunk1(15f);
				}
			}
			play_snd = text;
			this.recheck_emot_in_gm = true;
		}

		public void cureSerDrunk1(float ratio100 = 100f)
		{
			byte b = 2;
			while (ratio100 > 0f && this.Ser.has(SER.DRUNK))
			{
				if (ratio100 >= 100f || (float)X.xors(100) < ratio100 || b == 1)
				{
					M2SerItem m2SerItem = this.Ser.Get(SER.DRUNK);
					if (m2SerItem != null)
					{
						m2SerItem.drinkingOnWater();
					}
				}
				else if (b == 2)
				{
					b = (this.MyStomach.Has(RecipeManager.RP_CATEG.ACTIHOL) ? 1 : 0);
					if (b == 1)
					{
						continue;
					}
				}
				ratio100 -= 100f;
			}
		}

		public bool NelItemUseableUint(NelItem Itm, NelItem.CATEG categ, int grade, ulong v_uint)
		{
			return this.is_alive && (this.Ser.has(SER.CONFUSE) || categ == NelItem.CATEG.SER_APPLY || (categ == NelItem.CATEG.SER_CURE && this.Ser.hasBit(v_uint)));
		}

		public void NelItemUseUint(NelItem Itm, NelItem.CATEG categ, int grade, ulong v_uint, ref int ef_delay, ref string play_snd, int value)
		{
			string text = "item_cure";
			if (categ != NelItem.CATEG.SER_APPLY)
			{
				if (categ != NelItem.CATEG.SER_CURE)
				{
					return;
				}
				this.Ser.CureB(v_uint);
				if (this.PtcHld != null && !this.NM2D.GM.isActive())
				{
					string text2 = "delay";
					int num = ef_delay;
					ef_delay = num + 1;
					base.PtcVar(text2, (double)(num * 14));
					this.PtcHld.PtcSTTimeFixed("bench_cure_torned", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, false);
				}
			}
			else
			{
				this.Ser.AddB(v_uint, value);
				text = "";
				if (this.PtcHld != null && !this.NM2D.GM.isActive())
				{
					string text3 = "delay";
					int num = ef_delay;
					ef_delay = num + 1;
					base.PtcVar(text3, (double)(num * 14));
					this.PtcHld.PtcSTTimeFixed("bench_debuff", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, false);
				}
			}
			play_snd = text;
			this.recheck_emot_in_gm = true;
		}

		public override void PadVib(string vib_key, float level = 1f)
		{
			if (base.M2D.pre_map_active && this.UP.isActive())
			{
				NEL.PadVib(vib_key, level);
			}
		}

		public bool ignoreHitToBlockCollider(Collider2D Cld = null)
		{
			return this.isTrappedState() || this.isTortureAbsorbed();
		}

		public bool isShieldOpening()
		{
			return this.Skill.isShieldOpeningOnNormal(false, false);
		}

		public bool isWaterChoking()
		{
			return this.MistApply != null && this.MistApply.isWaterChoking();
		}

		public bool isWaterChokeState()
		{
			return this.state == PR.STATE.WATER_CHOKED;
		}

		public bool can_applydamage_state()
		{
			return this.state != PR.STATE.EVADE_SHOTGUN && this.state != PR.STATE.UKEMI_SHOTGUN && !this.isBurstState() && (this.state != PR.STATE.WORM_TRAPPED || this.t_state > 120f) && (this.state != PR.STATE.WATER_CHOKED || !this.is_alive || this.t_state > 180f);
		}

		public override bool pressdamage_applyable
		{
			get
			{
				return base.pressdamage_applyable && (!this.isBurstState() && !this.isTrappedState()) && !this.isTortureAbsorbed();
			}
		}

		public bool isWormTrappedCannotAccessByEnemy()
		{
			if (!this.isWormTrapped())
			{
				return false;
			}
			if (this.NM2D.FCutin.isActive(this))
			{
				return true;
			}
			if (!this.is_alive)
			{
				return this.t_state < 220f;
			}
			return this.t_state < 420f;
		}

		public MagicItem getCurMagic()
		{
			return this.Skill.getCurMagic();
		}

		public bool canHoldMagic(MagicItem Mg)
		{
			return this.Skill.canHoldMagic(Mg);
		}

		public bool isManipulatingMagic(MagicItem Mg)
		{
			return this.Skill.isManipulatingMagic(Mg);
		}

		public bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitItem)
		{
			return this.Skill.initPublishAtk(Mg, Atk, hittype, HitItem);
		}

		public void initPublishKill(M2MagicCaster Target)
		{
			this.Skill.initPublishKill(Target);
		}

		public int magic_returnable_mp
		{
			get
			{
				if (this.mp > 20)
				{
					return this.mp;
				}
				return X.Mx(((this.enemy_targetted > 0) ? 4 : ((X.XORSP() < 0.3f) ? 1 : 0)) * 4, this.mp);
			}
		}

		public NoelAnimator getAnimator()
		{
			return this.Anm;
		}

		public M2PrSkill getSkillManager()
		{
			return this.Skill;
		}

		protected override void declineRunningEffect()
		{
			this.Ser.declineRunningEffect();
		}

		public override bool cannotRun(bool starting)
		{
			return this.Ser.cannotRun() || (this.MistApply != null && this.MistApply.cannotRun()) || base.cannotRun(starting);
		}

		public M2PrMistApplier getMistApplier()
		{
			return this.MistApply;
		}

		public bool isBreatheStop(bool ache = false, bool water_acke = false)
		{
			return (!ache && this.NM2D.default_mist_pose > 0) || (this.MistApply != null && this.MistApply.isBreatheStop(ache, water_acke));
		}

		public bool canFastCureGSaver()
		{
			if (this.Skill.magic_chanting_or_preparing)
			{
				return false;
			}
			if (this.isNormalState())
			{
				return this.canJump() && this.Phy.walk_xspeed == 0f;
			}
			return this.isDamagingOrKo();
		}

		public bool isGSHpDamageSlowDown()
		{
			return this.isTrappedState() || this.AbsorbCon.isActive() || this.state == PR.STATE.DAMAGE_BURNED || this.isDownState();
		}

		public int getAbsorbWeight()
		{
			return 1;
		}

		public void abortAbsorbForce()
		{
			this.AbsorbCon.clear();
		}

		public float getAbsorbedTotalWeight()
		{
			return this.AbsorbCon.total_weight;
		}

		public bool isGachaRunableState()
		{
			return this.state == PR.STATE.ONNIE || this.state == PR.STATE.BENCH_ONNIE || this.state == PR.STATE.ORGASM || this.state == PR.STATE.ABSORB;
		}

		public bool isEvGachaState()
		{
			return this.state == PR.STATE.EV_GACHA;
		}

		public bool hasD(M2MoverPr.DECL d)
		{
			return ((this.decline | this.EpCon.decline) & d) > (M2MoverPr.DECL)0;
		}

		public bool hasD_stopact()
		{
			M2MoverPr.DECL decl = this.decline | this.EpCon.decline;
			return (decl & (M2MoverPr.DECL)33) != (M2MoverPr.DECL)0 || ((decl & M2MoverPr.DECL.EVENT) != (M2MoverPr.DECL)0 && !EV.lockPrInputManipulate(KEY.SIMKEY.Z, Map2d.can_handle, false));
		}

		public bool hasD_stopevade()
		{
			M2MoverPr.DECL decl = this.decline | this.EpCon.decline;
			return (decl & (M2MoverPr.DECL)34) != (M2MoverPr.DECL)0 || ((decl & M2MoverPr.DECL.EVENT) != (M2MoverPr.DECL)0 && !EV.lockPrInputManipulate(KEY.SIMKEY.LSH, Map2d.can_handle, false));
		}

		public bool hasD_stopmag()
		{
			M2MoverPr.DECL decl = this.decline | this.EpCon.decline;
			return (decl & (M2MoverPr.DECL)36) != (M2MoverPr.DECL)0 || ((decl & M2MoverPr.DECL.EVENT) != (M2MoverPr.DECL)0 && !EV.lockPrInputManipulate(KEY.SIMKEY.X, Map2d.can_handle, false));
		}

		public void addD(M2MoverPr.DECL d)
		{
			this.decline |= d;
		}

		public void remD(M2MoverPr.DECL d)
		{
			this.decline &= ~d;
		}

		public M2Ser getSer()
		{
			return this.Ser;
		}

		public bool magic_chanting
		{
			get
			{
				return this.Skill.magic_chanting;
			}
		}

		public bool magic_chanting_or_preparing
		{
			get
			{
				return this.Skill.magic_chanting_or_preparing;
			}
		}

		public bool magic_exploded
		{
			get
			{
				return this.Skill.magic_exploded;
			}
		}

		public override bool considerFricOnVelocityCalc()
		{
			return base.considerFricOnVelocityCalc() || this.Skill.considerFricOnVelocityCalc();
		}

		public override bool isCalcableThroughLiftState()
		{
			return base.isCalcableThroughLiftState() || (this.isPunchState(this.state) && !this.Skill.isManipulatingMagic(this.Skill.getCurMagicForCursor())) || (this.is_alive && !this.AbsorbCon.isActive() && (this.state == PR.STATE.DAMAGE_LT_LAND || this.state == PR.STATE.DAMAGE_L_LAND));
		}

		public static void PunchDecline(int t, bool do_not_in_battle = false)
		{
			if (do_not_in_battle && EnemySummoner.isActiveBorder())
			{
				return;
			}
			PR pr = M2DBase.Instance.Cam.getBaseMover() as PR;
			if (pr != null)
			{
				pr.Skill.punch_decline_time = t;
			}
		}

		public void blurTargetting(IM2RayHitAble _Mv)
		{
			this.Skill.blurTargetting(_Mv);
		}

		public bool isMasturbating()
		{
			return this.isMasturbateState(this.state) || this.Onnie != null;
		}

		public M2NoelCane getFloatCane()
		{
			return this.Anm.getFloatCane();
		}

		public AbsorbManagerContainer getAbsorbContainer()
		{
			return this.AbsorbCon;
		}

		public void debugSetMp(int f)
		{
			this.mp = X.MMX(0, f, this.maxmp - this.EggCon.total);
			this.GSaver.GsMp.debugSetValue((float)f);
			UIStatus.Instance.fineMpRatio(true, false);
			this.Ser.checkSer();
			this.recheck_emot = true;
		}

		public void debugSetHp(int f)
		{
			bool is_alive = this.is_alive;
			this.hp = X.MMX(0, f, this.maxhp);
			this.GSaver.GsHp.debugSetValue((float)f);
			UIStatus.Instance.fineHpRatio(true, false);
			this.Ser.checkSer();
			this.recheck_emot = true;
			if (is_alive && !this.is_alive)
			{
				base.penetrateNoDamageTime(NDMG._ALL, -1);
				this.hp = 1;
				this.applyDamage(new NelAttackInfo
				{
					hpdmg0 = 9999,
					ndmg = NDMG.PRESSDAMAGE
				}, true);
			}
		}

		public override void setWaterReleaseEffect(int water_id)
		{
			M2DropObject m2DropObject = this.Mp.DropCon.Add(this.FD_drawWaterRelease, base.x, base.y, 0f, 0f, 0f, (float)water_id);
			m2DropObject.gravity_scale = 0f;
			m2DropObject.type = DROP_TYPE.NO_OPTION;
		}

		public override bool canApplyCarryVelocity()
		{
			return base.canApplyCarryVelocity() && !this.hasD(M2MoverPr.DECL.DO_NOT_CARRY_BY_OTHER);
		}

		private bool drawWaterRelease(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			Dro.x = base.x;
			Dro.y = base.y;
			float num = (float)((int)(Dro.af / 3f) + 4);
			while (Dro.z < num)
			{
				Dro.z += 1f;
				this.NM2D.MIST.addWaterPuddleEffect(this, (int)Dro.time);
			}
			return num < 28f;
		}

		public virtual bool isWetPose()
		{
			return this.Ser.isWetPose();
		}

		public virtual bool isWeakPose()
		{
			return this.Ser.isWeakPose();
		}

		public bool isUnconscious()
		{
			return this.EpCon.isOrgasm() || !this.is_alive || this.Ser.has(SER.SLEEP);
		}

		public virtual bool getEmotDefault(out string fade_key, ref UIPictureBase.EMSTATE estate, ref bool force_change)
		{
			if (force_change && this.NM2D.GM.isActive())
			{
				this.runSer(0f);
			}
			fade_key = null;
			return false;
		}

		public virtual UIPictureBase.EMSTATE getEmotState()
		{
			UIPictureBase.EMSTATE emstate = UIPictureBase.EMSTATE.NORMAL;
			bool flag = this.isDamagingOrKo() && base.isNoDamageActive(NDMG.PRESSDAMAGE);
			emstate |= (this.isFacingEnemy() ? UIPictureBase.EMSTATE.BATTLE : UIPictureBase.EMSTATE.NORMAL);
			emstate |= (this.Ser.has(SER.MP_REDUCE) ? UIPictureBase.EMSTATE.LOWMP : UIPictureBase.EMSTATE.NORMAL);
			emstate |= (this.Ser.isWetPose() ? UIPictureBase.EMSTATE.LOWMP : UIPictureBase.EMSTATE.NORMAL);
			if (this.Ser.has(SER.SLEEP) && this.is_alive)
			{
				emstate |= UIPictureBase.EMSTATE.STUNNED | UIPictureBase.EMSTATE.SLEEP;
			}
			else
			{
				emstate |= (this.Ser.isStun() ? UIPictureBase.EMSTATE.STUNNED : UIPictureBase.EMSTATE.NORMAL);
			}
			if (this.Ser.has(SER.NEAR_PEE))
			{
				emstate |= UIPictureBase.EMSTATE.OSGM | ((this.Ser.getLevel(SER.NEAR_PEE) >= 1) ? UIPictureBase.EMSTATE.STUNNED : UIPictureBase.EMSTATE.NORMAL);
			}
			else if (this.EggCon.hasNearlyLayingEgg())
			{
				emstate |= UIPictureBase.EMSTATE.STUNNED | UIPictureBase.EMSTATE.OSGM;
			}
			if (!this.is_alive)
			{
				emstate |= UIPictureBase.EMSTATE.SER | UIPictureBase.EMSTATE.SHAMED | UIPictureBase.EMSTATE.LOWMP | UIPictureBase.EMSTATE.DEAD;
			}
			else
			{
				emstate |= ((this.Ser.has(SER.CONFUSE) || this.Ser.has(SER.DRUNK)) ? UIPictureBase.EMSTATE.CONFUSED : UIPictureBase.EMSTATE.NORMAL);
			}
			emstate |= (this.Ser.has(SER.HP_REDUCE) ? (UIPictureBase.EMSTATE.LOWHP | UIPictureBase.EMSTATE.SER) : UIPictureBase.EMSTATE.NORMAL);
			if (flag)
			{
				emstate |= UIPictureBase.EMSTATE.LOWHP | UIPictureBase.EMSTATE.SER | UIPictureBase.EMSTATE.SHAMED | UIPictureBase.EMSTATE.ABSORBED | UIPictureBase.EMSTATE.ORGASM;
			}
			if (this.EggCon.total > (int)(base.get_maxmp() * (float)CFG.sp_threshold_pregnant * 0.01f))
			{
				emstate |= UIPictureBase.EMSTATE.SHAMED | UIPictureBase.EMSTATE.LOWMP | UIPictureBase.EMSTATE.BOTE;
			}
			if (this.Ser.isShamed())
			{
				emstate |= UIPictureBase.EMSTATE.SHAMED;
			}
			if (this.Ser.hasBit(7725605116UL))
			{
				emstate |= UIPictureBase.EMSTATE.SER;
			}
			if (this.Ser.has(SER.ORGASM_AFTER))
			{
				emstate |= UIPictureBase.EMSTATE.ORGASM;
			}
			if (this.isOrgasm() || (this.Ser.has(SER.EGGED) | this.Ser.has(SER.LAYING_EGG) | this.Ser.has(SER.ORGASM_AFTER) | this.Ser.has(SER.ORGASM_INITIALIZE)))
			{
				emstate |= UIPictureBase.EMSTATE.ABSORBED;
			}
			if (this.isBurnedState() && this.isDownState())
			{
				emstate |= UIPictureBase.EMSTATE.SER | UIPictureBase.EMSTATE.ORGASM;
			}
			if (this.isAbsorbState())
			{
				emstate |= UIPictureBase.EMSTATE.ABSORBED;
				if ((emstate & UIPictureBase.EMSTATE.DEAD) != UIPictureBase.EMSTATE.NORMAL && X.XORSP() < 0.4f)
				{
					emstate &= ~(UIPictureBase.EMSTATE.LOWMP | UIPictureBase.EMSTATE.DEAD);
				}
				AbsorbManagerContainer absorbContainer = this.getAbsorbContainer();
				if (absorbContainer.isActive())
				{
					emstate |= absorbContainer.emstate_attach;
				}
			}
			if (this.BetoMng.isActive() && CFG.ui_effect_dirty > 0)
			{
				emstate |= UIPictureBase.EMSTATE.DIRT;
			}
			if (X.sensitive_level <= 1)
			{
				emstate |= (this.BetoMng.wetten ? UIPictureBase.EMSTATE.WET : UIPictureBase.EMSTATE.NORMAL);
				if (X.sensitive_level == 0 && this.BetoMng.is_torned)
				{
					emstate |= UIPictureBase.EMSTATE.TORNED;
				}
			}
			if (X.sensitive_level >= 2)
			{
				emstate |= UIPictureBase.EMSTATE.SP_SENSITIVE;
			}
			return emstate;
		}

		public bool recheck_emot
		{
			set
			{
				if (value && this.UP != null)
				{
					this.UP.recheck_emot = true;
				}
			}
		}

		public bool recheck_emot_in_gm
		{
			set
			{
				if (value && this.UP != null)
				{
					this.UP.force_recheck_allocate = true;
				}
			}
		}

		public void fineClothTorned()
		{
			this.Anm.finePose(0);
			this.NM2D.IMNG.fineSpecialNoelRow(this);
			if (this.BetoMng.is_torned && X.sensitive_level == 0)
			{
				this.Ser.Add(SER.CLT_BROKEN, -1, 99, false);
				return;
			}
			this.Ser.Cure(SER.CLT_BROKEN);
		}

		public AbsorbManager getAbsorbTorturePublisher()
		{
			if (!this.isAbsorbState())
			{
				return null;
			}
			return this.AbsorbCon.getTorturePublisher();
		}

		public bool getEH(EnhancerManager.EH ehbit)
		{
			return (EnhancerManager.enhancer_bits & ehbit) > (EnhancerManager.EH)0U;
		}

		public float getRE(RecipeManager.RPI_EFFECT rpi_effect)
		{
			return this.Skill.getRE(rpi_effect);
		}

		public float getSerApplyRatio()
		{
			return this.NM2D.NightCon.applySerRatio(false) * this.Skill.ser_apply_ratio;
		}

		public int getYdrgLevel()
		{
			if (this.Ydrg == null)
			{
				return -1;
			}
			return this.Ydrg.max_lv;
		}

		public override float event_cy
		{
			get
			{
				return base.mbottom - 68f * this.Mp.rCLEN;
			}
		}

		public override float event_sizex
		{
			get
			{
				return 12f * this.Mp.rCLEN;
			}
		}

		public override float event_sizey
		{
			get
			{
				return 68f * this.Mp.rCLEN;
			}
		}

		public bool isEventInjectable(bool check_foot = false)
		{
			if (check_foot && (!this.FootD.hasFoot() || this.FootD.FootIsLadder() || !this.isNormalState()))
			{
				return false;
			}
			M2BlockColliderContainer.BCCLine bccline;
			this.Mp.BCC.isFallable(base.x, base.y, this.sizex * 0.8f, 8f + this.sizey + 0.5f, out bccline, true, true, -1f);
			return bccline != null;
		}

		public string getOmorashiEventName()
		{
			string text = ((this.NM2D.WM.CurWM != null && !EnemySummoner.isActiveBorder() && !this.NM2D.FlgWarpEventNotInjectable.isActive() && !this.isMasturbating()) ? this.NM2D.WM.CurWM.ev_omorashi : null);
			if (!TX.valid(text))
			{
				return null;
			}
			if (EV.isActive(false))
			{
				return null;
			}
			return text;
		}

		public NelM2DBase NM2D;

		public const float gravity_scale_noel = 0.6f;

		private float _base_TS = 1f;

		private const float water_spd_scale = 0.75f;

		protected bool newgame_assign = true;

		private const float walkSpeed_burned = 0.115f;

		private const float walkSpeed_air_burned = 0.085f;

		protected NoelAnimator Anm;

		private float weight0;

		public M2MoverPr.PR_MNP outfit_default_mnp;

		protected M2MoverPr.DECL outfit_default_dcl;

		private RevCounterLock RCenemy_sink = new RevCounterLock();

		private const int UKEMI_DELAY = 20;

		private const int T_CROUCH_THROUGH_LIFT = 4;

		private const int T_EVADE_IN_DMG = 13;

		private const int HIT_MOVER_THRESHOLD = 4;

		private const float CORRUPT_MAGIC_CASTTIME_REDUCE = 0.25f;

		private const float CORRUPT_MAGIC_CASTTIME_CHANTING_REDUCE = 0.6f;

		private const int CORRUPT_MAGIC_CASTTIME_REDUCE_MIN = 20;

		private const int ENEMYSINK_THRESH_TIME = 6;

		private const int ENEMYSINK_EVADE_DELAY = 15;

		private const int ENEMYSINK_GUARD_TIME = 18;

		private const int ENEMYSINK_GUARD_EVADION = 13;

		private const int ENEMYSINK_MAXT = 55;

		private const float dmg_blink_mul_ratio = 0.9f;

		public float absorb_pose_change_ratio = 2.8f;

		public const float PUZZLE_PIECE_MP = 64f;

		private const float stomach_progress_to_water_drunk_add = 9f;

		private const float stomach_overdrunk_multiple = 2.5f;

		public bool need_check_bounds;

		public const int size_x_normal = 12;

		public const int size_y_normal = 68;

		private const int size_y_crouch = 40;

		private const int size_y_presscrouch = 10;

		private const int size_x_down = 30;

		private const int size_y_down = 28;

		private const int size_x_damage_l = 12;

		private const int size_y_damage_l = 24;

		private float wind_apply_t;

		private string fix_pose;

		private int break_pose_fix_on_walk_level;

		public int hp_crack;

		public M2Ser Ser;

		public MpGaugeBreaker GaugeBrk;

		public PrEggManager EggCon;

		public EpManager EpCon;

		public BetobetoManager BetoMng;

		public M2PrSkill Skill;

		public PrGaugeSaver GSaver;

		private AbsorbManagerContainer AbsorbCon;

		public const int ABSORB_MAX = 5;

		private float absorb_additional_dying;

		private Vector2 BufTg;

		protected PR.STATE state;

		public M2MoverPr.DECL decline;

		private float t_state;

		private float check_lava;

		private int lava_burn_dount;

		private float frozen_fineaf_time;

		private const int BURN_IRIS_COUNT = 3;

		private VoiceController VO;

		private TransEffecterItem TeFallShift;

		private NelItemManager IMNG;

		private M2PrMistApplier MistApply;

		public M2PrMasturbate Onnie;

		private YdrgManager Ydrg;

		public UIPicture UP;

		public FloatCounter<Collider2D>.FnNoReduce FD_ignoreHitToBlockCollider;

		private string _temp_puzzle_cache_str;

		private M2DropObject.FnDropObjectDraw FD_drawWaterRelease;

		public enum STATE
		{
			NORMAL,
			MAG_EXPLODE_PREPARE,
			MAG_EXPLODED,
			EVADE = 10,
			UKEMI,
			EVADE_SHOTGUN,
			UKEMI_SHOTGUN,
			PUNCH = 20,
			BURST = 22,
			SLIDING,
			WHEEL,
			WHEEL_SHOTGUN,
			COMET,
			COMET_SHOTGUN,
			DASHPUNCH,
			DASHPUNCH_SHOTGUN,
			AIRPUNCH,
			AIRPUNCH_SHOTGUN,
			SHIELD_BUSH = 40,
			SHIELD_LARIAT,
			SHIELD_COUNTER,
			SHIELD_COUNTER_SHOTGUN,
			BURST_SCAPECAT = 200,
			USE_BOMB = 390,
			ENEMY_SINK = 430,
			SHIELD_BREAK_STUN,
			LAYING_EGG,
			ORGASM,
			GAMEOVER_RECOVERY,
			WATER_CHOKED_RELEASE = 436,
			SLEEP,
			FROZEN,
			ONNIE = 500,
			EV_GACHA,
			DAMAGE = 4000,
			DAMAGE_L = 4010,
			DAMAGE_L_HITWALL,
			DAMAGE_L_DOWN_ABSORBAFTER = 4016,
			DAMAGE_L_LAND = 4015,
			DAMAGE_LT = 4020,
			DAMAGE_LT_KIRIMOMI = 4022,
			DAMAGE_LT_LAND = 4025,
			DAMAGE_OTHER_STUN = 4050,
			DAMAGE_PRESS_LR = 4030,
			DAMAGE_PRESS_TB,
			DOWN_STUN = 4100,
			DAMAGE_BURNED = 4150,
			ABSORB = 4600,
			WORM_TRAPPED = 4980,
			WATER_CHOKED,
			BENCH = 10000,
			BENCH_LOADAFTER,
			BENCH_ONNIE,
			BENCH_SITDOWN_WAIT = 100003
		}
	}
}
