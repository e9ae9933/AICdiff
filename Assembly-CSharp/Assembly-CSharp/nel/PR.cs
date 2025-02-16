using System;
using Better;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class PR : M2MoverPr, M2MagicCaster, NelM2Attacker, IPuzzRevertable, ITortureListener, IWindApplyable, NelItem.IItemUser, EnemySummoner.ISummonActivateListener
	{
		public bool fine_frozen_replace
		{
			get
			{
				return this.fine_frozen_replace_;
			}
			set
			{
				if (value)
				{
					this.fine_frozen_replace_ = true;
				}
			}
		}

		public PrVoiceController VO { get; private set; }

		protected override void Awake()
		{
			base.Awake();
			this.FD_drawWaterRelease = new M2DropObject.FnDropObjectDraw(this.drawWaterRelease);
		}

		public void refineAllLanguageCache()
		{
			if (this.Rebagacha != null)
			{
				this.Rebagacha.fineFont();
			}
		}

		public override void newGame()
		{
			base.newGame();
			this.newgame_assign = true;
			this.NM2D = M2DBase.Instance as NelM2DBase;
			if (this.NoDamage == null)
			{
				this.NoDamage = new M2NoDamageManager(null);
			}
			if (this.PtcHld == null)
			{
				this.PtcHld = new PtcHolder(this, 4, 4);
			}
			if (this.AbsorbCon == null)
			{
				this.Skill = new M2PrSkill(this, this.NM2D);
				this.SttInjector = new PrStateInjector(this, 4);
				this.AbsorbCon = new AbsorbManagerContainer(5, this);
				this.Ser = new M2Ser(this, this, true);
				this.GaugeBrk = new MpGaugeBreaker(this);
				this.GSaver = new PrGaugeSaver(this);
				this.EggCon = new PrEggManager(this);
				this.EpCon = new EpManager(this);
				this.JuiceCon = new M2PrAJuiceCon(this);
				this.BetoMng = BetobetoManager.GetManager("noel");
				this.VO = new PrVoiceController(this, MTR.VcNoelSource, this.snd_key + ".voice");
				this.DMGE = new M2PrADmgEffect(this);
				this.DMG = new M2PrADmg(this, this.RCenemy_sink, this.NoDamage);
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
			this.SpRunner = null;
			if (this.UP != null)
			{
				this.UP.recheck(0, 0);
			}
			this.setVoiceOverrideAllowLevel(0f);
			base.fix_aim = false;
			this._base_TS = 1f;
			this.JuiceCon.newGame();
			this.outfit_default_mnp = (M2MoverPr.PR_MNP)0;
			this.outfit_default_dcl = (M2MoverPr.DECL)0;
			this.BetoMng.cleanAll(true);
			this.GSaver.newGame();
			this.Skill.newGame();
			this.VO.newGame();
			this.DMG.newGame();
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
				this.LockCntDmgEffect = new M2LockCounter<MGATTR>(this.Mp, 4);
				this.LockCntOccur = new M2LockCounter<PR.OCCUR>(this.Mp, 4);
				this.Rebagacha = new M2RebagachaAnnounce(this);
				this.Rebagacha.initG();
				this.Phy.FnHitting = new M2Phys.FnHittingVelocity(this.DMG.FnHittingVelocity);
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
				this.LockCntDmgEffect.initS(this.Mp);
				this.LockCntOccur.initS(this.Mp);
			}
			this.JuiceCon.initS();
			EnemySummoner.addSummonerListener(this);
			this.Anm.rotationR = 0f;
			this.Ser.resetPtcSt();
			this.Ser.Cure(SER.OVERRUN_TIRED);
			this.walk_auto_assisting = 0;
			this.Phy.hit_mover_threshold = 4;
			this.Phy.hold_ice_on_air = true;
			this.Phy.quitSoftFall(0f);
			this.TeCon.RegisterScl(this.Anm);
			this.mp = X.MMX(0, this.mp, this.maxmp - this.EggCon.total);
			this.wind_apply_t = 0f;
			this.enemy_targetted = 0;
			this.EpCon.initS();
			this.MyLight = new M2Light(_Mp, this);
			this.MyLight.follow_speed = 0.1f;
			this.MyLight.Col.Set(2866067885U);
			this.MyLight.radius = 140f;
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
			this.frozen_fineaf_time = 0f;
			_Mp.addLight(this.MyLight, -1);
			if (!flag3)
			{
				this.crouching = 0f;
			}
			this.Mp.assignCenterPlayer(this);
			if (this.aim == AIM.B)
			{
				this.setAim(AIM.R, false);
			}
			this.DMG.initS();
			this.DMGE.initS();
			this.GSaver.initS();
			this.Rebagacha.initS();
			this.JuiceCon.initS();
			this.do_not_destruct_when_remove = true;
			this.Skill.initS();
			this._temp_puzzle_cache_str = null;
			this.NM2D.CheckPoint.appearPlayer(this);
			if (this.UP == null)
			{
				UIPicture.changePlayer(this);
			}
			this.SpRunner = null;
			this.setVoiceOverrideAllowLevel(0f);
			this.newgame_assign = false;
			if (this.Rebagacha != null)
			{
				this.Rebagacha.fineEnable();
			}
			this.fineFrozenAppearance();
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
			this.SpRunner = null;
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
			if (this.Rebagacha != null)
			{
				this.Rebagacha.endS();
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
				this.Anm.destruct();
			}
			this.Ser.destruct();
			this.VO.destruct();
			this.AbsorbCon.clearTextInstance().destruct();
			this.EpCon.destruct();
			if (this.Rebagacha != null)
			{
				this.Rebagacha.destruct();
			}
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
			this.DMG.runPre();
			if (this.SttInjector.need_check_on_runpre)
			{
				this.SttInjector.need_check_on_runpre = false;
				this.SttInjector.check(this.state, ref this.t_state, true);
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
				this.setDefaultPose(null);
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
					if (state <= PR.STATE.SHIELD_BREAK_STUN)
					{
						if (state != PR.STATE.ENEMY_SINK)
						{
							if (state == PR.STATE.SHIELD_BREAK_STUN)
							{
								if (flag)
								{
									this.VO.breath_key = "breath_e";
									this.SpSetPose(this.isPoseDown(false) ? "down2confused" : "stand2confused", -1, null, false);
								}
								this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
								if (!this.Ser.has(SER.SHIELD_BREAK))
								{
									this.changeState(PR.STATE.NORMAL);
									goto IL_05AC;
								}
								goto IL_05AC;
							}
						}
						else
						{
							if (flag)
							{
								this.RCenemy_sink.Clear();
								if (this.bounds_ != M2MoverPr.BOUNDS_TYPE.PRESSCROUCH)
								{
									this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
								}
								if (X.sensitive_level < 2 && this.Ser.getLevel(SER.NEAR_PEE) >= 1 && this.JuiceCon.checkNoelJuice((float)(this.hasD(M2MoverPr.DECL.FLAG1) ? 150 : 3), true, false, -1))
								{
									this.Ser.Add(SER.NEAR_PEE, -1, 2, false);
									this.addD(M2MoverPr.DECL.FLAG0);
									this.UP.setFade("wetten_osgm", UIPictureBase.EMSTATE.NORMAL, false, false, false);
								}
							}
							if (this.t_state >= 18f)
							{
								if (this.hasD(M2MoverPr.DECL.FLAG0) && this.Ser.getLevel(SER.NEAR_PEE) >= 2)
								{
									this.Ser.Cure(SER.NEAR_PEE);
									this.remD(M2MoverPr.DECL.FLAG0);
									this.JuiceCon.executeSplashNoelJuice(false, true, 0, false, false, false, false);
								}
								if ((this.Phy.is_on_web || this.Ser.has(SER.WEB_TRAPPED)) && !this.Ser.has(SER.BURNED))
								{
									this.playVo("awk", false, false);
									if (!this.Ser.has(SER.WEB_TRAPPED))
									{
										this.Ser.applySerDamage(EnemyAttr.SerDmgSlimy100, 1f, -1);
									}
									this.changeState(PR.STATE.DAMAGE_WEB_TRAPPED);
									goto IL_05AC;
								}
							}
							if (this.t_state >= 15f)
							{
								this.decline &= (M2MoverPr.DECL)(-3);
							}
							if (!this.Anm.poseIs("sink", "stand2sink") || this.t_state >= 55f)
							{
								this.changeState(PR.STATE.NORMAL);
								goto IL_05AC;
							}
							goto IL_05AC;
						}
					}
					else if (state != PR.STATE.EV_GACHA)
					{
						if (state - PR.STATE.WATER_CHOKED > 1)
						{
							if (state == PR.STATE.BENCH_SITDOWN_WAIT)
							{
								if (this.t_state >= 180f)
								{
									this.changeState(PR.STATE.NORMAL);
									goto IL_05AC;
								}
								this.setDefaultPose(null);
								goto IL_05AC;
							}
						}
						else
						{
							if (flag)
							{
								this.SpSetPose((base.forceCrouch(false, false) || this.state == PR.STATE.WATER_CHOKED_DOWN) ? "water_choked2_down" : "water_choked2", -1, null, false);
							}
							this.crouching = 0f;
							this.Ser.Add(SER.WORM_TRAPPED, -1, 99, false);
							base.getFootManager().lockPlayFootStamp(10);
							if (this.MistApply == null || !this.MistApply.isFloatingWaterChoke())
							{
								this.MistApply.abortWaterChokeRelease();
								goto IL_05AC;
							}
							goto IL_05AC;
						}
					}
					else
					{
						this.AbsorbCon.runAbsorbPr(this, this.t_state, this.TS);
						if (!this.AbsorbCon.isActive())
						{
							this.changeState(PR.STATE.NORMAL);
							goto IL_05AC;
						}
						goto IL_05AC;
					}
					bool flag2 = true;
					PR.STATE state2 = this.state;
					if (this.SpRunner != null)
					{
						if (!this.SpRunner.runPreSPPR(this, this.TS, ref this.t_state))
						{
							if (this.state != PR.STATE.NORMAL)
							{
								this.changeState(PR.STATE.NORMAL);
								goto IL_05AC;
							}
							goto IL_05AC;
						}
						else
						{
							flag2 = this.hasD(M2MoverPr.DECL.ORGASM_INJECTABLE);
						}
					}
					if ((!flag2 || !this.processLayingOrOrgasm(0f)) && state2 == this.state)
					{
						if (this.isDamagingOrKo())
						{
							Bench.P("run damaging");
							this.DMG.runDamaging(ref this.t_state);
							Bench.Pend("run damaging");
						}
						else if (this.SpRunner == null && this.state != PR.STATE.NORMAL)
						{
							this.changeState(PR.STATE.NORMAL);
						}
					}
				}
			}
			IL_05AC:
			Bench.P("pre _after ");
			this.runEnemySink((this.manip & M2MoverPr.PR_MNP.NO_SINK) == (M2MoverPr.PR_MNP)0);
			if (this.Ydrg != null && !this.Ydrg.run(this.TS))
			{
				this.Ydrg = null;
			}
			float num = (this.secureTimeState() ? 0f : this.TS);
			float num2 = (this.isTrappedState() ? 0f : (num * this.Ser.baseTimeScaleRev()));
			this.GSaver.run(num * ((this.DMG.hp_crack > 0) ? (1f / (float)(this.DMG.hp_crack + 1)) : 1f));
			this.reduceCrackMp(this.GaugeBrk.getReducePlayerMpValue(num2));
			this.AbsorbCon.run(num * this.Ser.baseTimeScaleRev());
			this.EggCon.run(num);
			this.EpCon.run(num);
			this.runSer(num2);
			if (this.wind_apply_t > 0f)
			{
				this.wind_apply_t = X.Mx(this.wind_apply_t - this.TS, 0f);
			}
			if (this.frozen_fineaf_time > 0f)
			{
				this.frozen_fineaf_time = X.Mx(this.frozen_fineaf_time - this.TS, 0f);
				this.Anm.fineFreezeFrame(false, true);
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
					if (this.isAnimationFrozen())
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
						else if (this.isDownState(this.state) || this.isPoseDown(false))
						{
							this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, false);
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
				this.SttInjector.need_check_on_runpre = true;
				if (this.isMagicExistState(this.state))
				{
					this.recheck_emot = true;
				}
				this.Ser.checkSer();
				if (this.Rebagacha != null)
				{
					this.Rebagacha.fineEnable();
				}
				Stomach stomach = this.NM2D.IMNG.getStomach(this);
				float stomachApplyRatio = this.Ser.getStomachApplyRatio();
				if (stomachApplyRatio != stomach.ser_apply_ratio)
				{
					stomach.ser_apply_ratio = stomachApplyRatio;
					stomach.fineEffect(true);
				}
				if (this.SfPose != null)
				{
					this.SfPose.fineSerState();
				}
				if ((pre_bits & 51808043008UL) != (this.Ser.get_pre_bits() & 51808043008UL))
				{
					this.fineFrozenAppearance();
				}
			}
		}

		public string setDefaultPose(string dep_pose = null)
		{
			dep_pose = ((this.SfPose != null) ? this.SfPose.setDefaultPose(dep_pose, ref this.fix_pose, ref this.break_pose_fix_on_walk_level) : dep_pose);
			if (this.crouching != 0f)
			{
				this.crouching = ((this.crouching < 0f) ? X.Mn(this.crouching + this.TS, 0f) : (this.crouching + this.TS));
				if (this.crouching > 0f && this.bounds_ == M2MoverPr.BOUNDS_TYPE.NORMAL)
				{
					this.setBounds(M2MoverPr.BOUNDS_TYPE.CROUCH, false);
				}
			}
			return dep_pose;
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
			if (this.SfPose != null)
			{
				this.SfPose.clearStandPoseTime();
			}
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
					this.Skill.killHoldMagic(false, false);
					this.playVo("laying_l", false, false);
					UIPictureBase.FlgStopAutoFade.Add("EggCon");
					this.UP.applyLayingEgg(true);
					bool flag3 = this.isPoseDown(false);
					if (this.isPoseCrouch(false) || flag3)
					{
						this.SpSetPose("laying_egg", -1, null, false);
					}
					else
					{
						this.SpSetPose("stand2laying_egg", -1, null, false);
					}
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
					if (!this.hasD(M2MoverPr.DECL.FLAG1))
					{
						this.addD(M2MoverPr.DECL.FLAG1);
						this.SttInjector.need_check_on_runpre = true;
					}
					if (fcnt != 0f && this.UP.isPoseLayingEgg())
					{
						this.recheck_emot = true;
					}
					if (this.VO.breath_key == null)
					{
						this.VO.breath_key = "breath_aft";
						this.Ser.Add(SER.TIRED, 240, 99, false);
						if (!this.poseIs(POSE_TYPE.ORGASM))
						{
							this.SpSetPose("down_u", -1, null, false);
						}
					}
					if (!base.isActionO(0))
					{
						return flag;
					}
					if (this.is_alive)
					{
						this.SpSetPose("wakeup_b", -1, null, false);
						this.changeState(PR.STATE.NORMAL);
						return flag;
					}
					this.changeState(PR.STATE.DAMAGE_L_LAND);
					return flag;
				}
				else
				{
					this.remD(M2MoverPr.DECL.FLAG1);
					if (this.Anm.poseIs("orgasm_down", "orgasm_down_laying_egg") && (this.t_state <= 0f || X.XORSP() < 0.05f))
					{
						this.Anm.setPose("orgasm_down_laying_egg", 1, false);
						return flag;
					}
					return flag;
				}
				break;
			case PR.STATE.ORGASM:
				if (this.t_state <= 0f)
				{
					this.t_state = 0f;
					this.UP.recheck_emot = true;
					this.VO.breath_key = "breath_e";
					if (!this.Anm.poseIs("must_orgasm", "must_orgasm_2"))
					{
						bool flag4 = this.isPoseDown(false);
						if (this.isPoseCrouch(false) || flag4 || this.Anm.poseIs("laying_egg", "stand2laying_egg"))
						{
							this.SpSetPose("orgasm_down", -1, null, false);
						}
						else
						{
							this.SpSetPose("stand2orgasm", -1, null, false);
						}
					}
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
					if (!this.hasD(M2MoverPr.DECL.FLAG1))
					{
						this.addD(M2MoverPr.DECL.FLAG1);
						this.SttInjector.need_check_on_runpre = true;
					}
					if (base.isActionO(0) || !this.is_alive)
					{
						this.quitOrgasm();
						return flag;
					}
					return flag;
				}
				else
				{
					if (this.hasD(M2MoverPr.DECL.FLAG1))
					{
						this.remD(M2MoverPr.DECL.FLAG1);
					}
					float orgasm_absorb_time = this.EpCon.get_orgasm_absorb_time();
					this.AbsorbCon.runAbsorbPr(this, orgasm_absorb_time, this.TS);
					if (this.Anm.poseIs("orgasm_down", "orgasm_down_2", "orgasm_down_laying_egg"))
					{
						if (this.t_state <= 0f || X.XORSP() < 0.05f + (this.Ser.has(SER.LAYING_EGG) ? 0.1f : 0f))
						{
							this.Anm.setPose("orgasm_down_laying_egg", 1, false);
							return flag;
						}
						return flag;
					}
					else
					{
						if (!this.Anm.poseIs("downdamage", "downdamage_t", "orgasm_from_stand", "stand2orgasm", "laying_egg", "must_orgasm_2", "bench_must_orgasm_2"))
						{
							this.Anm.setPose((this.isPoseDown(false) && !this.isPoseBack(false)) ? "orgasm_down_2" : "orgasm_from_stand", 1, false);
							return flag;
						}
						return flag;
					}
				}
				break;
			case PR.STATE.WATER_CHOKED_RELEASE:
				if (!this.hasD(M2MoverPr.DECL.FLAG0))
				{
					this.Ser.Cure(SER.WORM_TRAPPED);
					this.addD((M2MoverPr.DECL)50331648);
					bool flag5 = true;
					bool flag6 = false;
					bool flag7 = this.hasD(M2MoverPr.DECL.INIT_A);
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
							if (!CCON.isWater(config) && CCON.canStand(config) && (!flag7 || CCON.isWater(this.Mp.getConfig((int)num3, (int)num4))))
							{
								flag5 = false;
							}
						}
						else if (!CCON.isWater(this.Mp.getConfig((int)num, (int)num2)))
						{
							flag5 = false;
							flag6 = true;
						}
					}
					if (flag5)
					{
						this.SpSetPose(this.isPoseDown(false) ? "water_choked_release" : "stand2water_choked_release", -1, null, false);
						this.VO.breath_key = "water_choked_release_b";
					}
					else
					{
						this.SpSetPose(flag6 ? "crouch_dmg_breathe" : "dmg_breathe", -1, null, false);
						this.VO.breath_key = "cough";
					}
					this.playVo("water_choked_release_a", false, false);
				}
				if (this.t_state < 40f)
				{
					return flag;
				}
				if (this.MistApply == null || this.MistApply.canReleaseWaterChokeStun())
				{
					if (this.hasD(M2MoverPr.DECL.FLAG1))
					{
						this.SttInjector.need_check_on_runpre = true;
						this.remD((M2MoverPr.DECL)33554447);
					}
					if (base.isActionO(0))
					{
						this.changeState(PR.STATE.NORMAL);
						return flag;
					}
					return flag;
				}
				else
				{
					if (!this.hasD(M2MoverPr.DECL.FLAG1) && this.MistApply.waterDelayActive())
					{
						this.addD(M2MoverPr.DECL.STOP_COMMAND);
						this.remD(M2MoverPr.DECL.FLAG0);
						this.SpSetPose("water_choked", -1, null, false);
						this.VO.breath_key = null;
						return flag;
					}
					return flag;
				}
				break;
			case PR.STATE.SLEEP:
				if (this.t_state <= 0f)
				{
					this.t_state = 0f;
					this.recheck_emot = true;
					this.VO.breath_key = "breath_sleep";
					if (!this.VO.isPlaying())
					{
						this.playVo("sleep_init", false, false);
					}
					if (this.isPoseDown(false))
					{
						this.Anm.setPose("sleepdown", -1, false);
					}
					else
					{
						this.Anm.setPose(this.isPoseCrouch(false) ? "sleep" : (this.Ser.isSleepDownBursted() ? "stand2fainted" : "stand2sleep"), -1, false);
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
					return flag;
				}
				return flag;
			case PR.STATE.FROZEN:
				if (this.t_state <= 0f)
				{
					this.t_state = 0f;
				}
				if (!EnemySummoner.isActiveBorder())
				{
					PR.PunchDecline(50, false);
				}
				if (!this.isAnimationFrozen())
				{
					if (this.MistApply != null && !this.MistApply.isWaterChoking())
					{
						this.MistApply.cureAll();
					}
					this.changeState(PR.STATE.NORMAL);
					return flag;
				}
				return flag;
			}
			flag = false;
			return flag;
		}

		public void runSpecialObject(float fcnt)
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
			this.SfPose.runPost(this.TS);
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
							if (this.NM2D.GM.initRecipeBook(null, obj) && (this.is_alive || !CFGSP.go_cheat))
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
			this.VO.runUi();
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
				if (this.isAnimationFrozen())
				{
					return 0f;
				}
				float num = this.Anm.animator_TS * X.Pow2(this.Ser.baseTimeScale());
				if (this.MScr == null)
				{
					if (this.isNormalState() && this.Phy.walk_xspeed != 0f && this.Phy.is_on_ice && base.move_inputting_anything)
					{
						num *= 1.75f;
					}
				}
				else
				{
					num *= this.MScr.ms_timescale;
				}
				return num;
			}
		}

		public float uipicture_TS(int _torture_emot)
		{
			if (this.isAnimationFrozen())
			{
				return 0f;
			}
			float num = this.base_TS;
			if (_torture_emot == 1)
			{
				return num;
			}
			if (this.Ser.has(SER.WEB_TRAPPED))
			{
				num *= 1f - 0.25f * (float)this.Ser.getLevel(SER.WEB_TRAPPED) * ((_torture_emot > 0) ? 0.5f : 1f);
			}
			return num;
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

		public void setAimOppositeManual()
		{
			this.aim = CAim.get_opposite(this.aim);
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
					this.SttInjector.need_check_on_runpre = true;
				}
			}
			else
			{
				this.Skill.fineFoot();
				if (base.isRunStopping())
				{
					base.stopRunning(false, false);
				}
				if (this.SfPose != null)
				{
					this.SfPose.stateChanged();
				}
				if (this.isMagicExistState(this.state))
				{
					this.checkOazuke();
				}
				this.SttInjector.need_check_on_runpre = true;
			}
			if (this.FootD.FootIsLadder() && this.SfPose != null)
			{
				this.SfPose.clearStandPoseTime();
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
			bool flag = (this.crouching >= 4f || this.crouching < 0f) && this.isNormalState(this.state) && (this.manip & M2MoverPr.PR_MNP.NO_JUMP_AND_MOVING) == (M2MoverPr.PR_MNP)0;
			return base.checkSkipLift((flag && !this.isCrouchPunchState(this.state)) ? null : _P);
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
			this.changeState(_state, this.state);
		}

		protected virtual void changeState(PR.STATE _state, PR.STATE prestate)
		{
			if (prestate == PR.STATE.DAMAGE_LT_KIRIMOMI || _state == PR.STATE.NORMAL)
			{
				this.Anm.rotationR = 0f;
			}
			if (this.isLayingEgg(prestate))
			{
				UIPictureBase.FlgStopAutoFade.Rem("EggCon");
			}
			if (_state == PR.STATE.NORMAL)
			{
				this.DMG.killLockSttInjection();
				PR.STATE state;
				if (this.SttInjector.check(_state, ref this.t_state, false, out state))
				{
					_state = state;
					if (_state == this.state)
					{
						return;
					}
				}
			}
			if (this.is_crouch || !this.canJump())
			{
				base.recheckForceCrouch();
			}
			bool flag = this.isTrappedState(_state);
			if (this.isTrappedState(prestate) && !flag && this.Ser.has(SER.WORM_TRAPPED))
			{
				return;
			}
			bool flag2 = this.isDamagingOrKo(prestate);
			bool flag3 = this.isDamagingOrKo(_state);
			bool flag4 = this.isAbsorbState(prestate);
			bool flag5 = this.isAbsorbState(_state);
			if (flag4 != flag5)
			{
				if (!flag5)
				{
					this.AbsorbCon.gacha_renderable = false;
					this.NM2D.MIST.FlgHideSurface.Rem("ABSORB");
					this.quitTortureAbsorb();
					this.Anm.FlgDropCane.Rem("ABSORB");
					this.Anm.torture_by_invisible_enemy = false;
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
			if (this.SfPose != null)
			{
				this.SfPose.stateChanged();
			}
			bool flag6 = this.isMagicExistState(_state);
			if (!flag6)
			{
				if (this.FootD.FootIsLadder())
				{
					this.FootD.initJump(false, true, true);
				}
				if (!this.isAnimationFrozen() || (!flag2 && !this.isFrozenState(prestate) && (flag3 || this.isFrozenState(_state))))
				{
					this.fine_frozen_replace = true;
				}
			}
			this.decline = (this.decline & (M2MoverPr.DECL)4112) | ((this.is_alive && flag6) ? ((M2MoverPr.DECL)0) : ((M2MoverPr.DECL)11)) | (this.isNormalState(_state) ? ((M2MoverPr.DECL)0) : M2MoverPr.DECL.STOP_MG) | this.outfit_default_dcl;
			if (_state != PR.STATE.NORMAL)
			{
				this.decline |= M2MoverPr.DECL.CANNOT_EXECUTE_CHECKTARGET;
			}
			this.Skill.changeState(prestate, _state);
			if (this.isEvadeState(prestate) || this.isBenchState(prestate) || this.isBurstState(prestate) || prestate == PR.STATE.DAMAGE_LT_KIRIMOMI || prestate == PR.STATE.DAMAGE_PRESS_LR)
			{
				this.Phy.remLockMoverHitting(HITLOCK.EVADE);
				this.Phy.remLockGravity(this);
			}
			if (!this.isEvadeState(_state) && this.isEvadeState(prestate))
			{
				this.Phy.remLockMoverHitting(HITLOCK.EVADE);
			}
			bool flag7 = base.forceCrouch(false, false);
			int num = this.canCrouchState(_state);
			if (((_state == PR.STATE.NORMAL || num == 1) && this.isBO(0)) || flag7 || num == 2)
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
			if (prestate == PR.STATE.BURST_SCAPECAT && this.NM2D.GameOver != null)
			{
				this.NM2D.GameOver.executeScapecatRespawnAfter();
			}
			if (this.isBenchState(prestate) && !this.isBenchState(_state) && (this.Ser.has(SER.ORGASM_AFTER) || this.EpCon.isOrgasm()))
			{
				this.EpCon.quitOrasmSageTime(true);
			}
			this.state = _state;
			if (this.SpRunner_ != null)
			{
				this.SpRunner_.quitSPPR(this, this.state);
				this.SpRunner_ = null;
			}
			this.DMG.changeState(_state, prestate, flag3, flag2, flag);
			this.VO.breath_key = null;
			PR.STATE state2 = this.state;
			if (state2 <= PR.STATE.BURST_SCAPECAT)
			{
				if (state2 != PR.STATE.NORMAL)
				{
					if (state2 == PR.STATE.BURST || state2 == PR.STATE.BURST_SCAPECAT)
					{
						this.addD(M2MoverPr.DECL.THROW_RAY);
					}
				}
				else
				{
					if (this.Ser.has(SER.SHIELD_BREAK))
					{
						this.Ser.Cure(SER.SHIELD_BREAK);
					}
					this.checkOazuke();
				}
			}
			else if (state2 != PR.STATE.ENEMY_SINK)
			{
				if (state2 != PR.STATE.DAMAGE)
				{
					if (state2 - PR.STATE.WATER_CHOKED <= 1)
					{
						this.Skill.killHoldMagic(false, false);
					}
				}
				else
				{
					base.weight = this.weight0 * 16f;
				}
			}
			else
			{
				this.SpSetPose(this.isPoseCrouch(false) ? "sink" : "stand2sink", -1, null, false);
			}
			if (this.Rebagacha != null)
			{
				this.Rebagacha.fineEnable();
			}
			if (this.state == PR.STATE.DAMAGE_L || this.state == PR.STATE.DAMAGE_LT)
			{
				this.NoDamage.Add(30f);
			}
			if (this.canCrouchState(this.state) == 2 || (flag7 && this.bounds_ == M2MoverPr.BOUNDS_TYPE.NORMAL))
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
			this.SttInjector.need_check_on_runpre = true;
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

		public void setBoundsToNormal(bool force = false)
		{
			this.setBounds(M2MoverPr.BOUNDS_TYPE.NORMAL, force);
		}

		public void setBoundsToDown(bool force = false)
		{
			this.setBounds(M2MoverPr.BOUNDS_TYPE.DOWN, force);
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
				base.Size((float)((this.bounds_ == M2MoverPr.BOUNDS_TYPE.CROUCH_WIDE) ? 70 : 12), 40f, ALIGN.CENTER, ALIGNY.BOTTOM, use_move_by);
				this.Phy.fric_reduce_x = 0.06f;
				break;
			case M2MoverPr.BOUNDS_TYPE.DOWN:
				this.offset_pixel_y = 67f;
				base.Size(70f, 20f, ALIGN.CENTER, ALIGNY.BOTTOM, use_move_by);
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

		public void resetWeight()
		{
			base.weight = this.weight0;
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

		public void CureBench()
		{
			this.Ser.CureBench(0UL);
			this.GSaver.FineAll(true);
			this.EpCon.attachable_ser_oazuke = true;
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

		public void applyGasDamage(MistManager.MistKind K, MistAttackInfo Atk)
		{
			if (this.isBurstState())
			{
				return;
			}
			if (Atk.corrupt_gacha)
			{
				this.EpCon.corruptGachaMist((float)(K.damage_cooltime + 10));
			}
			if (!Atk.no_cough_move && this.isDamagingOrKo() && !this.is_alive && ((this.Anm.poseIs(POSE_TYPE.DOWN, false) && !this.isSleepingDownState()) || this.isPoseCrouch(false)))
			{
				if (this.AtkBufferForGasDamage == null)
				{
					this.AtkBufferForGasDamage = new NelAttackInfo();
				}
				this.AtkBufferForGasDamage.CopyFrom(Atk);
				this.applyDamage(this.AtkBufferForGasDamage, true);
				return;
			}
			if (Atk.no_cough_move && this.UP.isActive())
			{
				this.UP.applyEmotionEffect(Atk.attr, UIPictureBase.EMSTATE.NORMAL, 0.5f, false, false, 0f, 0f, false);
			}
			this.Ser.applySerDamage(Atk.SerDmg, this.getSerApplyRatio(), -1);
			int num = X.Mn(this.hp - 1, Atk._hpdmg);
			if (num > 0)
			{
				bool flag;
				this.DMG.applyHpDamageSimple(Atk, out flag, num, true);
			}
			if (Atk._mpdmg > 0)
			{
				this.DMG.splitMpByDamage(Atk, Atk._mpdmg, MANA_HIT.EN, 0, 0f, null, true);
			}
			if (Atk.EpDmg != null)
			{
				this.EpCon.applyEpDamage(Atk.EpDmg, this, EPCATEG_BITS._ALL, 3f, true);
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
						bool flag2;
						num3 = this.GSaver.applyHpDamage(num3, null, out dc, out num4, out flag2);
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
					bool flag3;
					num3 = this.GSaver.applyHpDamage(num3, null, out dc2, out num7, out flag3);
					base.applyHpDamage(num3, true, null);
					this.GSaver.GsHp.Fine(true);
					this.setDamageCounter(-num3, 0, dc2, null);
					UIStatus.Instance.fineHpRatio(true, false);
				}
				this.playVo("water_choked", false, false);
			}
			if (this.canChokedByWaterState())
			{
				this.isSleepingDownState();
				this.changeState((base.forceCrouch(false, false) || this.isSleepingDownState()) ? PR.STATE.WATER_CHOKED_DOWN : PR.STATE.WATER_CHOKED);
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
			this.JuiceCon.applyYdrgDamage(Atk);
			this.PadVib("dmg_ydrg", 1f);
			if (!this.is_alive && X.XORSP() < 0.38f)
			{
				return this.applyDamage(Atk, true);
			}
			this.UP.applyYdrgDamage();
			bool is_alive = this.is_alive;
			bool flag;
			int num = this.DMG.applyHpDamageSimple(Atk, out flag, Atk._hpdmg, true);
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
			if (this.Phy.isin_water)
			{
				vx *= 0.02f;
				vy *= 0.1f;
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

		public bool applying_wind
		{
			get
			{
				return this.wind_apply_t > 0f;
			}
		}

		public int applyDamage(NelAttackInfo Atk, bool force)
		{
			return this.DMG.applyDamage(Atk, force, "", false, false);
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
				if (CFGSP.use_uipic_press_enemy > 0 && X.xors(100) < (int)CFGSP.use_uipic_press_enemy)
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
				flag3 = CFGSP.use_uipic_press_gimmick;
			}
			float absorb_replace_prob = Atk.absorb_replace_prob;
			float absorb_replace_prob_ondamage = Atk.absorb_replace_prob_ondamage;
			int nodamage_time = Atk.nodamage_time;
			if (flag2)
			{
				Atk.absorb_replace_prob_both = 0f;
				Atk.nodamage_time = (flag ? 140 : 0);
			}
			int num = this.DMG.applyDamage(Atk, force, flag3 ? null : "", false, true);
			if (num > 0)
			{
				int num2 = CAim._XD(aim, 1);
				int num3 = CAim._YD(aim, 1);
				if (flag2)
				{
					if (num2 != 0)
					{
						this.setAim((num2 < 0) ? AIM.R : AIM.L, false);
					}
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
					this.DMGE.applyPressDamage(Atk, num2, num3, flag3);
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

		public void applyAbsorbDamage(NelAttackInfo Atk, bool execute_attack = true, bool mouth_damage = false, string fade_key = null, bool decline_additional_effect = false)
		{
			this.DMG.applyAbsorbDamage(Atk, execute_attack, mouth_damage, fade_key, decline_additional_effect);
		}

		public int applyMySelfFire(MagicItem Mg, M2Ray Ray, NelAttackInfo Atk)
		{
			return this.DMG.applyMySelfFire(Mg, Ray, Atk, this.manip);
		}

		public override Vector3 getDamageCounterShiftMapPos()
		{
			return this.DMG.getDamageCounterShiftMapPos();
		}

		public bool isAlreadyAbsorbed(NelM2Attacker AbsorbBy)
		{
			return this.AbsorbCon.isAlreadyAbsorbed(AbsorbBy);
		}

		public bool isPoseAbsorbDefault()
		{
			return this.Anm.strictPoseIs("absorbed_downb", "absorbed_down", "absorbed_crouch2absorbed_down", "absorbed_crouch2absorbed_downb", "absorbed_crouch", "absorbed2absorbed_crouch", "absorbed");
		}

		public override bool setAbsorbAnimation(string p, bool set_default = false, bool frozen_replacable = false)
		{
			if (frozen_replacable)
			{
				this.fine_frozen_replace = true;
			}
			if (TX.valid(p))
			{
				this.SpSetPose(p, -1, null, false);
				return true;
			}
			if (this.isFrozen())
			{
				return false;
			}
			if (!set_default)
			{
				return true;
			}
			this.SpSetPose(this.SfPose.absorb_default_pose(X.XORSP() < X.Mx(0f, -0.2f + (float)this.AbsorbCon.Length * 0.2f) + (this.is_alive ? 0.4f : 0f)), -1, null, false);
			return true;
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
				if (this.isWormTrapped())
				{
					this.executeReleaseFromTrapByDamage(false, false);
				}
				this.changeState(PR.STATE.ABSORB);
				if (this.UP.isActive())
				{
					this.NM2D.PE.addTimeFixedEffect(base.M2D.Cam.TeCon.setBounceZoomIn(1.03125f, 12f, -4), 1f);
					this.UP.applyDamage(Atk.attr, 0f, (float)(8 + X.xors(12)), UIPictureBase.EMSTATE.NORMAL, false, this.AbsorbCon.uipicture_fade_key, false);
				}
				this.playAwkVo();
			}
			else
			{
				this.t_state = 0f;
			}
			this.PadVib("init_absorb", flag ? 1f : 0.66f);
			this.DMG.initAbsorb();
			Abm.use_cam_zoom2 = true;
			return true;
		}

		public override void initTortureAbsorbPoseSet(M2Attackable Another, string p, int set_frame = -1, int reset_anmf = -1)
		{
			this.Anm.clearDownTurning(false);
			base.initTortureAbsorbPoseSet(Another, p, set_frame, -1);
			if (!this.isAbsorbState() || this.t_state < 5f)
			{
				this.fine_frozen_replace = true;
			}
			if (set_frame >= 0)
			{
				this.Anm.animReset(set_frame);
			}
			this.Anm.fineFreezeFrame(false, true);
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
			if (!this.AbsorbCon.no_wall_hit_ignore_on_torture)
			{
				this.Phy.addLockWallHitting(this.AbsorbCon, -1f);
			}
			this.Phy.killSpeedForce(true, true, true, false, false);
			this.Phy.addLockGravity(this.AbsorbCon, 0f, -1f);
			if (!(Another is NelEnemy))
			{
				this.Anm.torture_by_invisible_enemy = false;
				return;
			}
			if ((Another as NelEnemy).nattr_invisible)
			{
				this.Anm.torture_by_invisible_enemy = true;
				return;
			}
			this.Anm.torture_by_invisible_enemy = false;
		}

		public override void quitTortureAbsorb()
		{
			this.Phy.remLockWallHitting(this.AbsorbCon);
			this.Phy.remLockGravity(this.AbsorbCon);
			this.fine_frozen_replace = true;
		}

		public bool setTortureAnimation(string pose_name, int cframe, int loop_to)
		{
			if (TX.noe(pose_name))
			{
				this.Anm.clearDownTurning(true);
				return true;
			}
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

		public float GachaReleaseRate()
		{
			return X.ZPOW((this.Ser.GachaReleaseRate() + 0.25f) * (this.EpCon.is_in_corrupt_gacha_mist ? 0.66f : 1f));
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
			if (this.DMG.applyDamage(nelAttackInfo, false, (MDI != null) ? MDI.ui_fade_key : null, false, false) <= 0)
			{
				return null;
			}
			return nelAttackInfo;
		}

		public override bool setWaterDunk(int water_id, int misttype)
		{
			return this.DMG.setWaterDunk(water_id, misttype);
		}

		public override int ratingHpDamageVal(float ratio)
		{
			return (int)(150f * ratio + (float)X.Mx(this.maxhp - 150, 0) * 0.25f * ratio);
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

		public override M2Mover setToDefaultPosition(bool no_set_camera = false, Map2d TargetMap = null)
		{
			if ((TargetMap ?? this.Mp) == null || this.Mp == null)
			{
				return this;
			}
			if (EnemySummoner.isActiveBorder())
			{
				Vector2 revertPosPr = EnemySummoner.ActiveScript.getSummonedArea().RevertPosPr;
				this.setTo(revertPosPr.x, revertPosPr.y);
				return this;
			}
			return base.setToDefaultPosition(no_set_camera, TargetMap);
		}

		public void resetFlagsForGameOver()
		{
			BGM.remHalfFlag("PR");
			this.Anm.repairCane();
			this.Anm.FlgDropCane.Rem("DEAD");
			this.Anm.FlgDropCane.Rem("DMG");
			this.DMG.resetFlagsForGameOver();
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
			if (this.Ser.has(SER.ORGASM_STACK))
			{
				this.Ser.Add(SER.ORGASM_STACK, 150, 99, false);
			}
			this.Skill.initS();
			this.EggCon.forcePushout(false, true);
			this.changeState(PR.STATE.GAMEOVER_RECOVERY);
			this.VO.recoverFromGameOver();
			base.killSpeedForce(true, true, true);
			this.Phy.clearLock();
			this.Phy.immidiateCheckStuck();
			this.resetFlagsForGameOver();
			this.recoverGoSer(true, true);
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
			this.Ser.CureFrozen3(false);
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

		public void recoverGoSer(bool on_gameover = true, bool is_bench = false)
		{
			this.Ser.Cure(SER.BURST_TIRED);
			this.Ser.Cure(SER.BURNED);
			this.Ser.Cure(SER.SLEEP);
			if (is_bench)
			{
				this.Ser.CureFrozen3(true);
			}
			else
			{
				this.Ser.Cure(SER.FROZEN);
				this.Ser.Cure(SER.STONE);
			}
			if (on_gameover)
			{
				this.EpCon.attachable_ser_oazuke = true;
				if (this.Ser.getLevel(SER.SEXERCISE) >= 1)
				{
					this.Ser.Add(SER.FRUSTRATED, -1, 99, false);
					this.EpCon.attachable_ser_oazuke = false;
				}
			}
			if (on_gameover)
			{
				M2SerItem m2SerItem = this.Ser.Get(SER.ORGASM_STACK);
				if (m2SerItem != null)
				{
					m2SerItem.orgasmStackLockProgress(150f);
				}
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

		public void applyParalysisDamage()
		{
			if (this.isOrgasm() && this.AbsorbCon.isActive())
			{
				this.DMGE.effectParalysisDamage();
			}
			else
			{
				NelAttackInfo nelAttackInfo = MDAT.AtkParalysis.BurstDir((float)X.MPF(X.xors(2) == 0));
				nelAttackInfo.CenterXy(base.x, base.y, 0f);
				this.DMG.applyDamage(nelAttackInfo, true, "damage_thunder", false, false);
			}
			if (this.AbsorbCon.isActive())
			{
				this.AbsorbCon.CorruptGacha(2f, true);
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
				this.Skill.killHoldMagic(false, false);
				if (cure_egg)
				{
					if (base.M2D.isSafeArea())
					{
						this.EggCon.clearWormCount();
					}
					this.EggCon.clear(true);
				}
				this.cureMp(this.maxmp, true, true, true);
				UIStatus.Instance.fineMpRatio(true, false);
				UIStatus.Instance.quitCrack();
				this.recheck_emot = true;
			}
			if (cure_water_drunk)
			{
				this.JuiceCon.cureFromEvent(0);
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
			if (!this.RCenemy_sink.isLocked() && this.RCenemy_sink >= 6f + 90f * X.ZPOW(this.getRE(RCP.RPI_EFFECT.SINK_REDUCE)))
			{
				this.changeState(PR.STATE.ENEMY_SINK);
			}
		}

		public virtual bool canStartFrustratedMasturbate(bool starting = true)
		{
			if (this.Ser.has(SER.ORGASM_STACK))
			{
				return false;
			}
			if ((!starting || (this.isNormalState() && this.is_alive && base.hasFoot() && !this.getSkillManager().hasInput(true, true, true) && !this.isWebTrappedState(true))) && Map2d.can_handle && TX.noe(base.M2D.ev_mobtype) && this.get_walk_xspeed() == 0f && !this.Phy.isin_water && (this.MistApply == null || !this.MistApply.isActive()))
			{
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC == null || !footBCC.is_map_bcc)
				{
					return false;
				}
				if (footBCC != null && !footBCC.is_ladder && !base.hasTalkTargetNearby())
				{
					return true;
				}
			}
			return false;
		}

		public void changeStatePressToLHitWall()
		{
			M2MoverPr.BOUNDS_TYPE bounds_ = this.bounds_;
			this.changeState(PR.STATE.DAMAGE_L_HITWALL);
			this.NoDamage.Add(30f);
			this.SpSetPose("downdamage_t", -1, null, false);
			this.need_check_bounds = false;
			this.setBounds(bounds_, false);
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
			this.VO.playAwkVo();
		}

		public void fineFrozenAF(float time = 4f, bool force_on_ser = false)
		{
			if (this.Ser.has(SER.FROZEN) && (!this.isStoneSer() || force_on_ser))
			{
				this.frozen_fineaf_time = X.Mx(this.frozen_fineaf_time, time);
			}
		}

		public void fineFrozenAppearance()
		{
			if (this.Anm == null)
			{
				return;
			}
			NoelAnimator.FRZ_STATE frz_STATE = NoelAnimator.FRZ_STATE.NORMAL;
			int num = 0;
			int num2 = 0;
			bool flag = false;
			if (this.Ser.has(SER.FROZEN))
			{
				frz_STATE |= NoelAnimator.FRZ_STATE.FROZEN;
				int level = this.Ser.getLevel(SER.FROZEN);
				num = X.Mx(num, level + 1);
				if ((int)this.BetoMng.frozen_lv != level + 1)
				{
					if ((int)this.BetoMng.frozen_lv < level + 1)
					{
						base.PtcVar("level", (double)level).PtcST("frozen_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					this.BetoMng.frozen_lv = (byte)(level + 1);
					flag = true;
					if (this.UP != null)
					{
						this.UP.killPtc("ui_ser_frozen");
						this.UP.PtcST("ui_ser_frozen", null);
					}
				}
			}
			else if (this.BetoMng.frozen_lv > 0)
			{
				this.BetoMng.frozen_lv = 0;
				flag = true;
				base.PtcST("frozen_quit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				if (this.UP != null)
				{
					this.UP.killPtc("ui_ser_frozen");
				}
			}
			if (this.Ser.has(SER.STONE))
			{
				frz_STATE |= NoelAnimator.FRZ_STATE.STONE;
				int level2 = this.Ser.getLevel(SER.STONE);
				num2 = X.Mx(num2, level2 + 1);
				if ((int)this.BetoMng.stone_lv != level2 + 1)
				{
					if ((int)this.BetoMng.stone_lv < level2 + 1)
					{
						base.PtcVar("level", (double)level2).PtcST("stone_ser_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					else
					{
						base.PtcST("stone_ser_finish", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					this.BetoMng.stone_lv = (byte)(level2 + 1);
					flag = true;
					this.UP.PtcST("ui_ser_stone", null);
				}
			}
			else if (this.BetoMng.stone_lv > 0)
			{
				this.BetoMng.stone_lv = 0;
				flag = true;
				base.PtcST("stone_ser_finish", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (this.Ser.has(SER.WEB_TRAPPED))
			{
				frz_STATE |= NoelAnimator.FRZ_STATE.WEB_TRAPPED;
				int level3 = this.Ser.getLevel(SER.WEB_TRAPPED);
				num = X.Mx(num, level3 + 1);
				if ((int)this.BetoMng.web_trapped_lv != level3 + 1)
				{
					byte web_trapped_lv = this.BetoMng.web_trapped_lv;
					int num3 = level3 + 1;
					this.BetoMng.web_trapped_lv = (byte)(level3 + 1);
					flag = true;
				}
			}
			else if (this.BetoMng.web_trapped_lv > 0)
			{
				this.BetoMng.web_trapped_lv = 0;
				flag = true;
				base.PtcST("ser_web_trapped_quit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (frz_STATE != NoelAnimator.FRZ_STATE.NORMAL)
			{
				base.base_gravity = this.base_gravity0 * this.Ser.baseTimeScaleRev();
				if (this.UP != null)
				{
					if (this.BetoMng.stone_lv >= 1)
					{
						this.UP.FLUSH_FADE_COL = 5395026U;
					}
					else if (this.BetoMng.frozen_lv >= 2)
					{
						this.UP.FLUSH_FADE_COL = 12506068U;
					}
					else if (this.BetoMng.web_trapped_lv >= 2)
					{
						this.UP.FLUSH_FADE_COL = 5395026U;
					}
					else
					{
						this.UP.FLUSH_FADE_COL = 16777215U;
					}
				}
				if (this.Anm != null)
				{
					if (this.Anm.frz_state != frz_STATE)
					{
						this.Anm.frz_state = frz_STATE;
						flag = true;
					}
					this.Anm.frozen_lv = (byte)num;
					this.Anm.stone_lv = (byte)num2;
				}
				if (flag && this.Anm != null)
				{
					this.Anm.fineFreezeFrame(false, false);
				}
				if (this.Anm != null)
				{
					this.Anm.fineFrozenAppearance(flag);
				}
			}
			else
			{
				if (this.UP != null)
				{
					this.UP.FLUSH_FADE_COL = 16777215U;
				}
				base.base_gravity = this.base_gravity0;
				if (this.Anm != null)
				{
					this.Anm.frz_state = frz_STATE;
					this.Anm.frozen_lv = 0;
					this.Anm.stone_lv = 0;
					this.Anm.fineFreezeFrame(false, false);
					this.Anm.fineFrozenAppearance(flag);
				}
			}
			if (this.UP != null)
			{
				this.UP.getAdditionalState(true);
			}
		}

		public void executeReleaseFromTrapByDamage(bool changestate = true, bool addfoc = true)
		{
			this.Phy.killSpeedForce(true, true, true, false, false).remLockGravity(this).clearColliderLock();
			this.Ser.Cure(SER.WORM_TRAPPED);
			if (changestate)
			{
				this.changeState(PR.STATE.DAMAGE_LT_KIRIMOMI);
			}
			float num;
			float num2;
			this.DMGE.executeReleaseFromTrapByDamage(out num, out num2);
			if (addfoc)
			{
				this.Phy.addFoc(FOCTYPE.KNOCKBACK | FOCTYPE._INDIVIDUAL, num, num2, -1f, 0, 20, 15, -1, 0);
			}
		}

		public void releaseFromIrisOut(bool return_position = false, PR.STATE _state = PR.STATE.DAMAGE_L_LAND, string setpose = "dmg_down2", bool fine_cam_position = true)
		{
			this.Phy.killSpeedForce(true, true, true, false, false).remLockGravity(this).clearColliderLock();
			this.Ser.Cure(SER.WORM_TRAPPED);
			this.NoDamage.AddAll(130f, 48U);
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

		public void checkOazuke()
		{
			if (!this.EpCon.attachable_ser_oazuke)
			{
				return;
			}
			if (this.Mp.floort >= 60f && !this.Ser.has(SER.FRUSTRATED) && this.ep >= 700 && this.EpCon.getOrgasmedIndividualTotal() >= 5 && SCN.occurableOazuke() && X.XORSP() < 0.0625f)
			{
				this.EpCon.attachable_ser_oazuke = false;
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
			return this.Ser.has(SER.FROZEN) && (this.Ser.frozen_state & NoelAnimator.FRZ_STATE.FROZEN) > NoelAnimator.FRZ_STATE.NORMAL;
		}

		public bool isStoneSer()
		{
			return this.Ser.has(SER.STONE) && (this.Ser.frozen_state & NoelAnimator.FRZ_STATE.STONE) > NoelAnimator.FRZ_STATE.NORMAL;
		}

		public bool isOrgasmLocked(bool from_digesting = false)
		{
			return this.Ser.isOrgasmLocked() || (this.isFrozen() && CFGSP.frozen_lock_orgasm) || (this.isStoneSer() && CFGSP.stone_lock_orgasm) || (from_digesting && (EV.isActive(false) || this.isMoveScriptActive(false)));
		}

		public bool isAnimationFrozen()
		{
			return this.isFrozen() || this.isStoneSer();
		}

		public bool isDoubleSexercise()
		{
			return this.Ser.has(SER.SEXERCISE) && this.Ser.EpApplyRatio() >= 3f;
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

		public bool runLayingEggCheckActivate()
		{
			this.Ser.Add(SER.LAYING_EGG, 100, 99, false);
			if (this.Ser.has(SER.DO_NOT_LAY_EGG) && this.isMagicExistState(this.state))
			{
				this.Ser.Cure(SER.DO_NOT_LAY_EGG);
			}
			bool flag = this.isLayingEgg();
			this.SttInjector.check(this.state, ref this.t_state, true);
			if (!flag && this.isLayingEgg())
			{
				this.playVo("laying_l", false, false);
			}
			return this.isLayingEgg();
		}

		public bool hasNearlyLayingEgg(PrEggManager.CATEG categ)
		{
			return this.Ser.has(SER.LAYING_EGG) && this.EggCon.hasNearlyLayingEgg(categ);
		}

		public bool canProgressLayingEgg()
		{
			return this.enemy_targetted > 0 || !this.is_alive || this.Ser.has(SER.EGGED_2);
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
			this.VO.evInit();
			if (this.Rebagacha != null)
			{
				this.Rebagacha.evInit();
			}
			base.evInit();
		}

		public override void evQuit()
		{
			this.remD((M2MoverPr.DECL)4112);
			this.VO.evQuit();
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
			if (this.Rebagacha != null)
			{
				this.Rebagacha.evQuit();
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

		public override bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			if (follow == PTCThread.StFollow.FOLLOW_HIP)
			{
				Vector2 vector = this.getHipPos();
				V = new Vector3(vector.x, vector.y, 1f);
				return true;
			}
			return base.getEffectReposition(St, follow, fcnt, out V);
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
									if (FEnum<SER>.TryParse(rER.getHash(2), out ser, true))
									{
										rER.Def(rER.getHash(1), (float)(this.Ser.has(ser) ? 1 : 0));
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
						if (num != 1838106216U)
						{
							if (num == 2021943417U)
							{
								if (cmd == "%AIM")
								{
									rER.Def("aim", (float)this.Anm.pose_aim_visible);
									rER.Def("ax", (float)CAim._XD(this.Anm.pose_aim_visible, 1));
									rER.Def("ay", (float)CAim._YD(this.Anm.pose_aim, 1));
									return true;
								}
							}
						}
						else if (cmd == "%GET_LOCK_MGATTR")
						{
							StringKey stringKey = rER.getHash(2);
							MGATTR mgattr;
							if (FEnum<MGATTR>.TryParse(stringKey, out mgattr, true))
							{
								rER.Def(rER.getHash(1), (float)(this.LockCntDmgEffect.isLocked(mgattr) ? 1 : 0));
							}
							else
							{
								rER.tError("不明なattr:" + stringKey);
							}
							return true;
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
				else if (num != 3459549804U)
				{
					if (num != 3472483256U)
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
				else if (cmd == "%SET_LOCK_MGATTR")
				{
					StringKey stringKey = rER.getHash(1);
					MGATTR mgattr2;
					if (FEnum<MGATTR>.TryParse(stringKey, out mgattr2, true))
					{
						this.LockCntDmgEffect.Add(mgattr2, rER.Nm(2, 15f));
					}
					else
					{
						rER.tError("不明なattr:" + stringKey);
					}
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
				this.Ser.Cure(SER.BURNED);
				this.Ser.CureFrozen3(true);
				this.EpCon.cureOrgasmAfter();
				if (this.state == PR.STATE.ONNIE || this.state == PR.STATE.EV_GACHA || this.isDamagingOrKo() || this.isPiyoStunState() || this.isSleepState() || this.isSinkState())
				{
					this.changeState(PR.STATE.NORMAL);
				}
			}
			if (this.MScr == null || !this.MScr.isActive())
			{
				this.Skill.killHoldMagic(false, false);
				if (this.isOnBench(true))
				{
					this.changeState(PR.STATE.NORMAL);
					this.Anm.setPose(this.poseIs("bench_must_orgasm_2") ? "must_orgasm_2" : "bench2stand", -1, false);
				}
			}
			base.assignMoveScript(str, soft_touch);
			return this;
		}

		public override bool canOpenCheckEvent(IM2TalkableObject Tk)
		{
			return base.canOpenCheckEvent(Tk) && !this.hasD(M2MoverPr.DECL.CANNOT_EXECUTE_CHECKTARGET) && (base.hasFoot() || !EnemySummoner.isActiveBorder()) && (!(Tk is M2EventItem) || (!this.isWormTrapped() && !this.Ser.has(SER.WORM_TRAPPED)));
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
			return this.Mp != null && (this.isTrappedState() || this.isAbsorbState(this.state) || this.isDamagingOrKo() || this.Skill.cannotAccessToCheckEvent() || this.EggCon.isLaying() || this.isMagicState(this.state) || this.isGaraakiState() || this.isOnBench(false) || (this.MistApply != null && this.MistApply.isWaterChokeDamageAlreadyApplied(true)));
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
			this.EpCon.lockOazuke(200f);
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
			if (this.Ser.getLevel(SER.SEXERCISE) >= 1 && this.EpCon.attachable_ser_oazuke)
			{
				this.Ser.Add(SER.FRUSTRATED, -1, 99, false);
				this.EpCon.attachable_ser_oazuke = false;
			}
		}

		public void initSpRunner(ISpecialPrRunner SpR)
		{
			this.changeState(PR.STATE.SP_RUN);
			this.SpRunner = SpR;
			this.addD(M2MoverPr.DECL.STOP_SPECIAL_OUTFIT);
		}

		public ISpecialPrRunner SpRunner
		{
			get
			{
				return this.SpRunner_;
			}
			set
			{
				if (this.SpRunner_ == value)
				{
					return;
				}
				if (this.SpRunner_ != null)
				{
					this.SpRunner_.quitSPPR(this, PR.STATE.NORMAL);
				}
				this.SpRunner_ = value;
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
			bool use_torture = this.AbsorbCon.use_torture;
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
			this.PtcHld.Var("ax", (double)this.Anm.mpf_is_right_visible);
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
					return this.Anm.mpf_is_right_visible * (float)X.MPF(!this.isPoseBack(false));
				}
				return X.NIXP(-0.023f, 0.023f);
			}
		}

		public float anm_mpf_is_right
		{
			get
			{
				return this.Anm.mpf_is_right_visible;
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
				if (this.isMoveScriptActive(false))
				{
					this.need_check_bounds = true;
				}
			}
		}

		public bool frozenAnimReplaceable(bool break_flag = true)
		{
			if (!this.isAnimationFrozen())
			{
				return true;
			}
			if (this.state != PR.STATE.FROZEN && this.fine_frozen_replace_)
			{
				if (break_flag)
				{
					this.fine_frozen_replace_ = false;
				}
				return true;
			}
			return false;
		}

		public override void SpMotionReset(int set_to_frame = 0)
		{
			this.Anm.animReset(set_to_frame);
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
			return this.VO.playVo(family, no_use_post, force_mouth_override);
		}

		public bool mouth_is_covered()
		{
			if (this.AbsorbCon.isActive() && this.AbsorbCon.mouth_is_covered)
			{
				return true;
			}
			int level = this.Ser.getLevel(SER.WEB_TRAPPED);
			return level >= 2 || (level == 1 && X.XORSP() < 0.4f);
		}

		public void setVoiceOverrideAllowLevel(float f)
		{
			this.VO.ignore_prechn_stop = (float)CFGSP.dmg_multivoice / 100f;
			this.VO.override_allow_level = X.Scr(f, this.VO.ignore_prechn_stop);
		}

		public override void moveByHitCheck(M2Phys AnotherPhy, FOCTYPE foctype, float map_dx, float map_dy)
		{
			if (base.hasFoot())
			{
				this.Phy.clipWalkXSpeed();
				if (AnotherPhy.Mv is NelEnemy)
				{
					this.addEnemySink(EnemyAttr.getSinkRatio(AnotherPhy.Mv as NelEnemy), false, 0f);
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
			float num3 = (float)(is_crouch ? 40 : (is_down ? 20 : 68)) * this.Mp.rCLENB;
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
			if (!this.is_alive || this.Mp == null || EnemySummoner.isActiveBorder())
			{
				return false;
			}
			if (this.MistApply != null && (this.MistApply.isWaterChoking() || (this.MistApply.isActive() && this.Phy.isin_water)))
			{
				return false;
			}
			if (this.Ser != null && this.Ser.frozen_state != NoelAnimator.FRZ_STATE.NORMAL)
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

		public bool isCrouchPunchState(PR.STATE st)
		{
			return this.isSlidingState(st) || st == PR.STATE.SMASH || st == PR.STATE.SMASH_SHOTGUN;
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

		public bool isFreezingState(PR.STATE st)
		{
			return this.isSleepingDownState(st) || st == PR.STATE.FROZEN;
		}

		public int canCrouchState(PR.STATE st)
		{
			if (this.isCrouchPunchState(st) || st == PR.STATE.DOWN_STUN || st == PR.STATE.SHIELD_BREAK_STUN || st == PR.STATE.ONNIE)
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
					nelChipBench = (this.Mp.findChip((int)base.x, (int)(base.mbottom - 0.25f), "bench") ?? this.Mp.findChip((int)base.x, (int)(base.mbottom - 1.25f), "bench")) as NelChipBench;
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

		public bool changeLayingEggState(PR.STATE st, bool do_not_consider_frame = false)
		{
			if (st == PR.STATE.DAMAGE_L_LAND || st == PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER || st == PR.STATE.DAMAGE_OTHER_STUN || st == PR.STATE.DOWN_STUN || st == PR.STATE.DAMAGE_LT_LAND)
			{
				return this.DMG.isInjectableNormalState(st);
			}
			return st == PR.STATE.ORGASM || st == PR.STATE.DAMAGE_WEB_TRAPPED || st == PR.STATE.DAMAGE_WEB_TRAPPED_LAND || (!this.isAbsorbState(st) && !this.isBenchState(st) && !this.isBurstState(st) && (st == PR.STATE.NORMAL || this.isMagicState(st) || this.isEvadeState(st) || (this.isDownState(st) && this.canJump())));
		}

		public bool canApplyParalysisAttack()
		{
			return this.state != PR.STATE.DAMAGE && !this.isBurstState(this.state);
		}

		public bool changeOrgasmState(PR.STATE st)
		{
			return st != PR.STATE.LAYING_EGG && st != PR.STATE.ORGASM && this.changeLayingEggState(st, false);
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
			return this.isFrozenState(this.state);
		}

		public bool isFrozenState(PR.STATE state)
		{
			return state == PR.STATE.FROZEN;
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
			return this.state == PR.STATE.SLIDING || this.state == PR.STATE.SMASH || this.state == PR.STATE.SMASH_SHOTGUN;
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
			else if (st != PR.STATE.SHIELD_BUSH && st - PR.STATE.SHIELD_COUNTER > 3 && st != PR.STATE.USE_BOMB)
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
			return _state == PR.STATE.WORM_TRAPPED || _state == PR.STATE.WATER_CHOKED || _state == PR.STATE.WATER_CHOKED_DOWN;
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
			return this.isBurstState() || this.state == PR.STATE.WATER_CHOKED || this.state == PR.STATE.WATER_CHOKED_DOWN || this.Ser.has(SER.BURST_TIRED);
		}

		public bool isPressDamageState(PR.STATE st)
		{
			return st == PR.STATE.DAMAGE_PRESS_LR || st == PR.STATE.DAMAGE_PRESS_TB;
		}

		public bool isPressDamageState()
		{
			return this.isPressDamageState(this.state);
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

		public override bool isGaraaakiForNAI()
		{
			return base.isGaraaakiForNAI() || this.isGaraakiState() || this.isAbsorbState();
		}

		public override float mpf_is_right_visible
		{
			get
			{
				return this.Anm.mpf_is_right_visible;
			}
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

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return this.DMG.applyHpDamageRatio(Atk);
		}

		public bool canApplyAbrosb()
		{
			return this.state != PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER && !this.isBurstState(this.state);
		}

		public Stomach MyStomach
		{
			get
			{
				return this.NM2D.IMNG.StmNoel;
			}
		}

		public string canUseItem(NelItem Itm, ItemStorage Storage, bool for_using = true)
		{
			string text = "";
			if (for_using)
			{
				if (this.hasD(M2MoverPr.DECL.NO_USE_ITEM))
				{
					return TX.Get("Alert_bench_execute_scenario_locked", "");
				}
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
				string omorashiEventName = this.JuiceCon.getOmorashiEventName();
				int num2 = 0;
				if (this.getEH(ENHA.EH.juice_server) && (Itm.RecipeInfo == null || (Itm.RecipeInfo.categ & RCP.RPI_CATEG.FROMNOEL) == (RCP.RPI_CATEG)0))
				{
					num2 = 1;
				}
				float num3 = ((num2 > 0) ? (this.MyStomach.hasWater() ? 1f : 0.38f) : 0f);
				float num4 = 0.4f + num3 + (this.Ser.has(SER.DRUNK) ? 1f : 0f);
				if (omorashiEventName != null)
				{
					num4 *= 2f;
				}
				this.JuiceCon.addWaterDrunkCache(num4, this.MyStomach.water_drunk_level, num2);
				if (omorashiEventName != null && this.JuiceCon.checkNoelJuice(X.NI(4, 30, num3) + (float)(this.Ser.has(SER.DRUNK) ? 35 : 0), false, false, num2))
				{
					int level = this.Ser.getLevel(SER.NEAR_PEE);
					if (level == -1)
					{
						this.JuiceCon.SerApplyNearPee(0);
					}
					else if (level == 0)
					{
						this.JuiceCon.SerApplyNearPee(1);
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
					b = (this.MyStomach.Has(RCP.RP_CATEG.ACTIHOL) ? 1 : 0);
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

		public bool isWaterChokingDamageAlreadyApplied(bool consider_release_after = true)
		{
			return this.MistApply != null && this.MistApply.isWaterChokeDamageAlreadyApplied(consider_release_after);
		}

		public bool isWaterChokeState()
		{
			return this.state == PR.STATE.WATER_CHOKED || this.state == PR.STATE.WATER_CHOKED_DOWN;
		}

		public bool can_applydamage_state()
		{
			return this.state != PR.STATE.EVADE_SHOTGUN && this.state != PR.STATE.UKEMI_SHOTGUN && !this.isBurstState() && (this.state != PR.STATE.WORM_TRAPPED || this.t_state > 120f) && (!this.isWaterChokeState() || !this.is_alive || this.t_state > 180f);
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
			if (!this.isWormTrapped() && (DIFF.I != 0 || (this.state != PR.STATE.ENEMY_SINK && !this.isWebTrappedState(false))))
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

		public bool isWebTrappedState(bool include_on_web = false)
		{
			return (include_on_web && (this.Phy.is_on_web || this.Ser.has(SER.WEB_TRAPPED))) || this.isWebTrappedState(this.state);
		}

		public bool isWebTrappedState(PR.STATE state)
		{
			return state - PR.STATE.DAMAGE_WEB_TRAPPED <= 1;
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
			this.JuiceCon.initPublishKill(Target);
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
			return this.state != PR.STATE.SMASH && this.state != PR.STATE.SMASH_SHOTGUN && (base.isCalcableThroughLiftState() || (this.isPunchState(this.state) && !this.Skill.isManipulatingMagic(this.Skill.getCurMagicForCursor())) || (this.is_alive && !this.AbsorbCon.isActive() && (this.state == PR.STATE.DAMAGE_LT_LAND || this.state == PR.STATE.DAMAGE_L_LAND)));
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
			return this.isMasturbateState(this.state) || this.SpRunner is M2PrMasturbate;
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
			if (!this.is_alive && this.isStoneSer())
			{
				emstate |= UIPictureBase.EMSTATE.STONEOVER;
			}
			emstate |= (this.Ser.has(SER.HP_REDUCE) ? (UIPictureBase.EMSTATE.LOWHP | UIPictureBase.EMSTATE.SER) : UIPictureBase.EMSTATE.NORMAL);
			if (flag)
			{
				emstate |= UIPictureBase.EMSTATE.LOWHP | UIPictureBase.EMSTATE.SER | UIPictureBase.EMSTATE.SHAMED | UIPictureBase.EMSTATE.ABSORBED | UIPictureBase.EMSTATE.ORGASM;
			}
			if (this.EggCon.total > (int)(base.get_maxmp() * (float)CFGSP.threshold_pregnant * 0.01f))
			{
				emstate |= UIPictureBase.EMSTATE.SHAMED | UIPictureBase.EMSTATE.LOWMP | UIPictureBase.EMSTATE.BOTE;
			}
			if (this.Ser.isShamed())
			{
				emstate |= UIPictureBase.EMSTATE.SHAMED;
			}
			if (this.Ser.hasBit(110804820220UL))
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

		public bool isInjectableNormalState(PR.STATE state)
		{
			return this.isMagicExistState(state) || state == PR.STATE.GAMEOVER_RECOVERY || this.DMG.isInjectableNormalState(state);
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

		public MANA_HIT mana_hit_absorb
		{
			get
			{
				if (!this.isAbsorbState() && !this.isWormTrapped())
				{
					return MANA_HIT.NOUSE;
				}
				return MANA_HIT.FROM_ABSORB_SPLIT;
			}
		}

		public bool getEH(ENHA.EH ehbit)
		{
			return (ENHA.enhancer_bits & ehbit) > (ENHA.EH)0U;
		}

		public float getRE(RCP.RPI_EFFECT rpi_effect)
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
			this.Mp.BCC.isFallable(base.x, base.y, this.sizex * 0.8f, 8f + this.sizey + 0.5f, out bccline, true, true, -1f, null);
			return bccline != null;
		}

		public NelM2DBase NM2D;

		public const float gravity_scale_noel = 0.6f;

		private float _base_TS = 1f;

		private const float water_spd_scale = 0.75f;

		protected bool newgame_assign = true;

		protected NoelAnimator Anm;

		private float weight0;

		public PrStateInjector SttInjector;

		public M2MoverPr.PR_MNP outfit_default_mnp;

		protected M2MoverPr.DECL outfit_default_dcl;

		private RevCounterLock RCenemy_sink = new RevCounterLock();

		public const int UKEMI_DELAY = 20;

		private const int T_CROUCH_THROUGH_LIFT = 4;

		public const int T_EVADE_IN_DMG = 13;

		private const int HIT_MOVER_THRESHOLD = 4;

		private const float CORRUPT_MAGIC_CASTTIME_REDUCE = 0.25f;

		private const float CORRUPT_MAGIC_CASTTIME_CHANTING_REDUCE = 0.6f;

		private const int CORRUPT_MAGIC_CASTTIME_REDUCE_MIN = 20;

		private const int ENEMYSINK_THRESH_TIME = 6;

		private const int ENEMYSINK_EVADE_DELAY = 15;

		public const int ENEMYSINK_GUARD_TIME = 18;

		private const int ENEMYSINK_GUARD_EVADION = 13;

		public const int ENEMYSINK_MAXT = 55;

		public const int knockback_time_web_trapped = 15;

		public const float absorb_pose_change_ratio = 2.8f;

		public const float PUZZLE_PIECE_MP = 64f;

		public bool need_check_bounds;

		public const int size_x_normal = 12;

		public const int size_y_normal = 68;

		private const int size_y_crouch = 40;

		private const int size_y_presscrouch = 10;

		private const int size_x_down = 70;

		private const int size_y_down = 20;

		private const int size_x_damage_l = 12;

		private const int size_y_damage_l = 24;

		private float wind_apply_t;

		private string fix_pose;

		private int break_pose_fix_on_walk_level;

		public M2Ser Ser;

		internal M2PrADmg DMG;

		internal M2PrADmgEffect DMGE;

		public MpGaugeBreaker GaugeBrk;

		public PrEggManager EggCon;

		public EpManager EpCon;

		public BetobetoManager BetoMng;

		public M2PrSkill Skill;

		public PrGaugeSaver GSaver;

		internal M2PrAJuiceCon JuiceCon;

		protected AbsorbManagerContainer AbsorbCon;

		public const int ABSORB_MAX = 5;

		private Vector2 BufTg;

		protected PR.STATE state;

		public M2MoverPr.DECL decline;

		private float t_state;

		private float frozen_fineaf_time;

		private bool fine_frozen_replace_;

		public AnimationShuffler SfPose;

		private NelItemManager IMNG;

		protected M2PrMistApplier MistApply;

		public M2RebagachaAnnounce Rebagacha;

		private YdrgManager Ydrg;

		private M2LockCounter<MGATTR> LockCntDmgEffect;

		public M2LockCounter<PR.OCCUR> LockCntOccur;

		public UIPicture UP;

		private ISpecialPrRunner SpRunner_;

		public FloatCounter<Collider2D>.FnNoReduce FD_ignoreHitToBlockCollider;

		private NelAttackInfo AtkBufferForGasDamage;

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
			SMASH,
			SMASH_SHOTGUN,
			BURST_SCAPECAT = 200,
			SP_RUN = 350,
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
			DAMAGE_WEB_TRAPPED = 4200,
			DAMAGE_WEB_TRAPPED_LAND,
			ABSORB = 4600,
			WORM_TRAPPED = 4980,
			WATER_CHOKED,
			WATER_CHOKED_DOWN,
			BENCH = 10000,
			BENCH_LOADAFTER,
			BENCH_ONNIE,
			BENCH_SITDOWN_WAIT = 100003
		}

		public enum OCCUR
		{
			HITSTOP_THUNDER,
			HITSTOP_ORGASM,
			BURNED_FADER,
			HITSTOP_NOELJUICE,
			NOELJUICE_ZOOMIN,
			BOMB_SELF,
			THUNDER_MOSAIC
		}
	}
}
