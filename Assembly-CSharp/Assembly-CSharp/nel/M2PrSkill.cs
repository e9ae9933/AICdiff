using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2PrSkill : M2PrAssistant
	{
		public bool skill_on_guard
		{
			get
			{
				return this.isEnable(SkillManager.SKILL_TYPE.guard);
			}
		}

		public bool skill_on_evade
		{
			get
			{
				return this.isEnable(SkillManager.SKILL_TYPE.evade);
			}
		}

		public M2PrSkill(PR _Pr)
			: base(_Pr)
		{
			this.MagicSel = new MagicSelector(this.Pr, this);
			this.BurstSel = new BurstSelector(this.Pr, this.MagicSel);
			this.Shield = new M2Shield(this.Pr, MGHIT.EN)
			{
				AtkUnmnp = MDAT.AtkShieldLariatHit()
			};
			this.Shield.fnSwitchActivation = new M2Shield.FnSwitchActivation(this.fnShieldSwitchActivation);
			this.Oeffect01 = new BDic<RecipeManager.RPI_EFFECT, float>(16);
			this.OcSlots = new M2PrOverChargeSlot(this.Pr);
			this.Shield.activate_time = 10f;
			this.Shield.deactivate_anim_time = 15f;
			this.Cursor = new NelPlayerCursor();
			this.FlgSoftFall = new Flagger(delegate(FlaggerT<string> _Flg)
			{
				if (this.Phy == null)
				{
					return;
				}
				float num = (base.getEH(EnhancerManager.EH.falling_cat) ? this.chanting_softfall_scale_with_enhancer : 1f);
				this.Phy.initSoftFall(this.chanting_softfall_scale * num, 14f * num);
			}, delegate(FlaggerT<string> _Flg)
			{
				if (this.Phy != null)
				{
					this.Phy.quitSoftFall(0f);
				}
			});
			this.SerRegist = new FlagCounter<SER>(4);
		}

		public void destruct()
		{
			this.BurstSel.newGame();
			this.Cursor.destruct();
		}

		public override void initS()
		{
			base.initS();
			this.resetParameterS();
			this.CurMg = (this.CurSkill = null);
		}

		public void resetParameterS()
		{
			this.magic_t = (this.stk_magic_t = 0f);
			this.punch_t = (this.evade_t = 0f);
			this.mp_overused = 0f;
			this.Cursor.init(base.Mp, this.Pr);
			this.BurstSel.deactivate();
			this.BurstSel.clearExecuteCount();
			this.mp_hold = (this.mp_overhold = 0f);
			this.mana_drain_lock_t_ = 0f;
			this.swaysld_t = 0f;
			this.freeze_lock_t = 0f;
			this.CarryBox = null;
			this.pre_chanted_magic_id = -1;
			this.punch_decline_time_ = 0;
			this.cyclone_attacked = false;
			this.FlgSoftFall.Clear();
			this.Shield.initS();
			this.OcSlots.initS();
			this.MagicSel.initS();
		}

		public void deactivateFromMap()
		{
			this.quitPuzzleManagingMp();
			this.BurstSel.deactivate();
			this.Shield.resetValue(true);
		}

		public override void newGame()
		{
			this.Shield.resetValue(true);
			this.SerRegist.Clear();
			this.BurstSel.newGame();
			this.MagicSel.newGame();
			this.evade_count = 0;
			this.resetSkillConnection(true, false, false);
			this.OcSlots.newGame();
		}

		public void initDeath()
		{
			this.killHoldMagic(true);
			if (this.Shield.isActive())
			{
				this.Shield.breakShield(this.Pr.x, this.Pr.y, MGATTR.ENERGY, false);
			}
		}

		public void fineIMNG()
		{
			this.OcSlots.fineSlots(true);
			this.resetSkillConnection(false, false, false);
		}

		public void EnableSkillClear()
		{
			this.enable_skill_bits = 0UL;
			this.shield_fcnt = 1f;
			this.Pr.Ser.progress_speed = 1f;
			this.pr_chant_speed = 1f;
			this.ser_apply_ratio = 1f;
			this.pr_mp_hunder_chant_speed = 0.25f;
			this.Oeffect01.Clear();
		}

		public void readBinaryFrom(ByteArray Ba, int vers)
		{
			this.MagicSel.readBinaryFrom(Ba, vers);
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			this.MagicSel.writeBinaryTo(Ba);
		}

		public M2MoverPr.PR_MNP runPre()
		{
			float ts = this.Pr.TS;
			float num = ts * this.Pr.Ser.baseTimeScaleRev();
			M2MoverPr.PR_MNP pr_MNP = (base.isNormalState() ? ((M2MoverPr.PR_MNP)0) : M2MoverPr.PR_MNP.NO_SINK) | this.Pr.outfit_default_mnp;
			Bench.P("ItemUsel");
			pr_MNP |= this.runUseItemSelectCheck(num);
			Bench.Pend("ItemUsel");
			Bench.P("Punch");
			pr_MNP |= this.runPunchCheck(num);
			Bench.Pend("Punch");
			Bench.P("Evade");
			pr_MNP |= this.runEvadeCheck(num, true, false);
			this.Pr.target_calced = (base.isNormalState() ? ((this.magic_t >= (float)this.MAGIC_CHANT_DELAY || this.magic_t < -1f) ? 1 : 0) : (base.isMagicExistState() ? 1 : 0));
			Bench.Pend("Evade");
			Bench.P("Magic");
			if (!base.hasD(M2MoverPr.DECL.STOP_MG))
			{
				pr_MNP |= this.runMagicCheck(num);
			}
			Bench.Pend("Magic");
			this.Cursor.run();
			Bench.P("Shield");
			this.Shield.run(ts, base.hasD(M2MoverPr.DECL.NO_PROGRESS_SHIELD_POWER) ? 0f : this.shield_fcnt, -1f);
			Bench.Pend("Shield");
			pr_MNP |= this.runFreezeGacha(num);
			Bench.P("Other");
			if (this.punch_decline_time_ > 0)
			{
				this.punch_decline_time_ -= 1;
			}
			if (this.parry_t > 0f)
			{
				this.parry_t = global::XX.X.Mx(this.parry_t - num, 0f);
			}
			bool flag = this.Pr.isNormalState();
			float num2 = ts * ((this.CurMg != null) ? 0.33f : 1f);
			if (this.Pr.Ser.has(SER.BURST_TIRED) || this.Pr.isSleepState())
			{
				this.BurstSel.reduceBurstExecute(num2 * 2f);
			}
			else if (flag && this.punch_t == 0f)
			{
				this.BurstSel.reduceBurstExecute(num2);
			}
			if (this.mana_drain_lock_t_ > 0f)
			{
				float num3 = ts * (((flag && this.magic_t == 0f) || !base.isMagicExistState()) ? ((this.Pr.isPunchState() || this.evade_t > 0f) ? 0.5f : 1f) : 0.125f);
				this.mana_drain_lock_t_ = global::XX.X.Mx(this.mana_drain_lock_t_ - num3, 0f);
				if (this.mana_drain_lock_t_ == 0f)
				{
					this.NM2D.Mana.fineRecheckTarget(1f);
				}
			}
			if (this.mp_overused > 0f && !base.Ser.has(SER.MP_REDUCE))
			{
				this.mp_overused = global::XX.X.Mx(this.mp_overused - num * 0.06666667f, 0f);
			}
			if (base.getEH(EnhancerManager.EH.singletask))
			{
				if (base.isNormalState())
				{
					if (this.magic_t != 0f || this.mp_hold > 0f)
					{
						this.mana_drain_lock_t = 25f;
					}
				}
				else if (base.isMagicExistState() || this.CurMg != null)
				{
					this.mana_drain_lock_t = 45f;
				}
				else if (this.Pr.isPunchState() || this.Pr.isShieldAttackState())
				{
					this.mana_drain_lock_t = 10f;
				}
			}
			if (this.swaysld_t > 0f)
			{
				if (this.swaysld_t < 100f)
				{
					pr_MNP |= M2MoverPr.PR_MNP.NO_SINK;
				}
				if (this.swaysld_t >= 180f)
				{
					this.swaysld_t = 0f;
				}
				else
				{
					this.swaysld_t += ts;
				}
			}
			else if (this.swaysld_t < 0f)
			{
				this.swaysld_t = global::XX.X.Mn(this.swaysld_t + ts, 0f);
			}
			Bench.Pend("Other");
			return pr_MNP;
		}

		public void runInFloorPausing()
		{
			this.runPunchCheck(0f);
			this.runEvadeCheck(0f, false, false);
			if (!base.hasD(M2MoverPr.DECL.STOP_MG))
			{
				this.runMagicCheck(0f);
			}
		}

		public void runPost()
		{
			Bench.P("Oc");
			this.OcSlots.runPost(base.TS);
			Bench.Pend("Oc");
		}

		public void fineFoot()
		{
			if (base.hasFoot())
			{
				this.cyclone_attacked = false;
			}
		}

		public bool runState(bool first, ref float t, ref M2MoverPr.PR_MNP manip)
		{
			bool flag = true;
			PR.STATE state = base.state;
			if (state <= PR.STATE.SHIELD_BUSH)
			{
				switch (state)
				{
				case PR.STATE.MAG_EXPLODE_PREPARE:
					if (first)
					{
						base.SpSetPose("magic_init", -1, null, false);
						if (this.CurMg != null)
						{
							this.magic_t = (float)(-(float)MDAT.ExplodePrepare(this.CurMg));
						}
						base.remD(M2MoverPr.DECL.STOP_ACT);
					}
					this.magic_t += base.TS * global::XX.X.Mx(1f, this.Pr.getCastingTimeScale(this.CurMg));
					base.GaugeBrk.secureSplitMpHoldTime(4f);
					if (this.magic_t < 0f)
					{
						goto IL_1F91;
					}
					this.explodeMagic();
					if (base.state == PR.STATE.MAG_EXPLODE_PREPARE)
					{
						this.Pr.changeState(PR.STATE.MAG_EXPLODED);
					}
					goto IL_1F91;
				case PR.STATE.MAG_EXPLODED:
				{
					if (first)
					{
						this.magic_aim_agR = 1.5707964f - (float)global::XX.CAim._XD(base.aim, 1) * 0.3926991f;
						this.changePoseMagicHold();
						this.magic_t = (float)(-(float)MDAT.getMagicExplodeAfterDelay(this.CurMg));
						base.remD(M2MoverPr.DECL.STOP_ACT);
					}
					base.GaugeBrk.secureSplitMpHoldTime(4f);
					bool flag2 = false;
					if (this.CurMg != null && this.CurMg.Mn != null && this.CurMg.Mn._0.fnManipulateMagic != null)
					{
						flag2 = this.CurMg.Mn._0.fnManipulateMagic(this.CurMg, this.Pr, base.TS);
					}
					if (!flag2)
					{
						base.addD(M2MoverPr.DECL.ABORT_BY_MOVE);
						this.CurMg = null;
						this.magic_t += base.TS;
						if (this.magic_t >= 0f)
						{
							this.Pr.changeState(PR.STATE.NORMAL);
						}
						goto IL_1F91;
					}
					else
					{
						base.remD(M2MoverPr.DECL.ABORT_BY_MOVE);
						this.changePoseMagicHold();
						if (this.CurMg != null)
						{
							this.magic_t = (float)(-(float)MDAT.getMagicExplodeAfterDelay(this.CurMg));
						}
						goto IL_1F91;
					}
					break;
				}
				case (PR.STATE)3:
				case (PR.STATE)4:
				case (PR.STATE)5:
				case (PR.STATE)6:
				case (PR.STATE)7:
				case (PR.STATE)8:
				case (PR.STATE)9:
				case (PR.STATE)14:
				case (PR.STATE)15:
				case (PR.STATE)16:
				case (PR.STATE)17:
				case (PR.STATE)18:
				case (PR.STATE)19:
				case (PR.STATE)21:
					return false;
				case PR.STATE.EVADE:
				case PR.STATE.UKEMI:
				case PR.STATE.EVADE_SHOTGUN:
				case PR.STATE.UKEMI_SHOTGUN:
				{
					bool flag3 = base.state == PR.STATE.UKEMI || base.state == PR.STATE.UKEMI_SHOTGUN;
					bool flag4 = base.state == PR.STATE.EVADE_SHOTGUN || base.state == PR.STATE.UKEMI_SHOTGUN;
					float num = (float)(flag4 ? 46 : 25);
					if (first)
					{
						base.SpSetPose(flag4 ? "ukemi_shotgun" : (flag3 ? "ukemi" : "evade"), 1, null, false);
						this.Pr.killSpeedForce(true, true, false);
						this.evade_count++;
						if (!base.hasFoot())
						{
							this.Anm.setAim((base.aim == global::XX.AIM.L) ? 6 : 7, 0, false);
							base.addD(M2MoverPr.DECL.FLAG2);
						}
						base.addD(M2MoverPr.DECL.WEAK_THROW_RAY);
						float num2 = this.getRE(RecipeManager.RPI_EFFECT.EVADE_NODAM_EXTEND);
						if (base.Ser.has(SER.CLT_BROKEN) && CFG.sp_cloth_broken_debuff)
						{
							num2 = global::XX.X.Scr(num2, 0.5f);
						}
						float num3 = global::XX.X.NI(14f, num, 0.875f * num2);
						base.TeCon.setEvadeBlink(num3);
						this.Pr.addNoDamage(NDMG.EVADE_PREVENT, num3);
						base.PtcVar("maxt", num3).PtcST("evade_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						base.addD((M2MoverPr.DECL)10 | (base.getEH(EnhancerManager.EH.cliff_stopper) ? ((M2MoverPr.DECL)0) : M2MoverPr.DECL.DO_NOT_CARRY_BY_OTHER));
						this.Phy.killSpeedForce(true, true, true, false, false).addLockMoverHitting(HITLOCK.EVADE, 14f);
						if (!flag4)
						{
							this.Phy.addLockGravity(this, 0f, 25f);
						}
						if (flag4 || flag3)
						{
							base.addD(M2MoverPr.DECL.FLAG_HIT);
						}
					}
					bool flag5 = base.state == PR.STATE.EVADE;
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						float num4 = (flag4 ? 0.015f : 0.06f);
						if (t < num && (flag4 || base.hasD(M2MoverPr.DECL.INIT_A) || ((base.aim == global::XX.AIM.L) ? base.isRO() : base.isLO())))
						{
							float num5 = (flag4 ? 2f : (base.hasD(M2MoverPr.DECL.FLAG2) ? 2.4f : 2.6f)) * 2f / num - num4;
							float num6 = (num4 - num5) / num;
							float num7 = num5 + num6 * t;
							this.Pr.walkBy(FOCTYPE.EVADE | this.foc_cliff_stopper, num7 * (float)global::XX.X.MPF(base.aim == global::XX.AIM.L), 0f, true);
							flag5 = false;
						}
						else
						{
							if (flag4 && base.Ser.cannotEvade())
							{
								this.Pr.changeState(PR.STATE.ENEMY_SINK);
								goto IL_1F91;
							}
							if ((t < 6f || !base.hasFoot()) && base.poseIs("evade"))
							{
								this.Anm.setAim((base.aim == global::XX.AIM.L) ? global::XX.AIM.BL : global::XX.AIM.RB, 0, false);
							}
							if (base.hasFoot())
							{
								this.Pr.getPhysic().addFoc(FOCTYPE.EVADE | this.foc_cliff_stopper, num4 * (float)global::XX.X.MPF(base.aim == global::XX.AIM.L), 0f, -1f, 0, 1, (int)global::XX.X.Mn(5f, num - t - 1f), -1, 0);
							}
							base.addD(M2MoverPr.DECL.FLAG0);
						}
					}
					if (this.Pr.hasD(M2MoverPr.DECL.WEAK_THROW_RAY) && !this.Pr.isNoDamageActive(NDMG.EVADE_PREVENT))
					{
						this.Pr.remD((M2MoverPr.DECL)8193);
						flag = false;
					}
					if (t >= 14f && this.evade_count < this.evade_count_max)
					{
						base.addD(M2MoverPr.DECL.ABORT_BY_MOVE);
						this.Pr.remD(M2MoverPr.DECL.STOP_EVADE);
					}
					if (base.hasD(M2MoverPr.DECL.FLAG_HIT))
					{
						flag = true;
						this.Pr.lockSink(16f, true);
					}
					if (flag5)
					{
						if (base.isLO())
						{
							if (this.Anm.mpf_is_right > 0f)
							{
								this.Anm.setAim(global::XX.CAim.get_aim2(0f, 0f, -1f, -1f, false), 0, false);
							}
						}
						else if (base.isRO() && this.Anm.mpf_is_right < 0f)
						{
							this.Anm.setAim(global::XX.CAim.get_aim2(0f, 0f, 1f, -1f, false), 0, false);
						}
					}
					bool flag6 = false;
					if (flag3 && !base.Ser.cannotEvade() && t >= 14f && this.Pr.isActionPD())
					{
						flag6 = true;
					}
					if (!flag6 && t < num)
					{
						goto IL_1F91;
					}
					if (flag5)
					{
						this.Pr.aim = ((this.Anm.mpf_is_right > 0f) ? global::XX.AIM.R : global::XX.AIM.L);
					}
					this.Pr.changeState(base.Ser.cannotEvade() ? PR.STATE.ENEMY_SINK : PR.STATE.NORMAL);
					if (!base.hasFoot())
					{
						base.SpSetPose("fall", 1, null, false);
					}
					goto IL_1F91;
				}
				case PR.STATE.PUNCH:
					if (t < 9f)
					{
						float num8 = t;
						PR.STATE punchVariation = this.getPunchVariation(false, true);
						if (punchVariation != PR.STATE.PUNCH && punchVariation != PR.STATE.NORMAL)
						{
							t = num8;
							goto IL_1F91;
						}
					}
					if (first)
					{
						base.SpSetPose("attack1", -1, null, false);
						if (this.CurMg != null)
						{
							base.Mp.playSnd("swing_pr_1");
						}
					}
					if (t >= 9f && !base.hasD(M2MoverPr.DECL.INIT_A))
					{
						base.addD(M2MoverPr.DECL.INIT_A);
						if (this.Pr.isMoveRightOn())
						{
							this.Pr.setAim(global::XX.AIM.R, false);
						}
						else if (this.Pr.isMoveLeftOn())
						{
							this.Pr.setAim(global::XX.AIM.L, false);
						}
						base.SpSetPose("attack2", -1, null, false);
						this.executeBasicPunch(0f);
					}
					if (t >= 12f)
					{
						base.addD(M2MoverPr.DECL.FLAG2);
					}
					if (t >= 20f)
					{
						base.remD((M2MoverPr.DECL)10);
						base.addD(M2MoverPr.DECL.ABORT_BY_MOVE);
					}
					if (t >= 24f)
					{
						this.Pr.changeState(PR.STATE.NORMAL);
					}
					goto IL_1F91;
				case PR.STATE.BURST:
					break;
				case PR.STATE.SLIDING:
				{
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						this.executeSmallAttack(0, null);
						base.PtcST("sliding_sunabokori", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Pr.addD(M2MoverPr.DECL.FLAG0);
						t = 0f;
						if (this.swaysld_t == 0f && base.getEH(EnhancerManager.EH.sway_sliding))
						{
							base.addD(M2MoverPr.DECL.INIT_A);
							base.PtcVar("maxt", 180f).PtcVar("applyt", 100f).PtcST("sway_sliding_activate", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
							base.TeCon.setColorBlinkAddFadeout(22f, 100f, 0.8f, 7766417, 0);
							this.swaysld_t = 1f;
						}
					}
					base.SpSetPose("sliding", -1, null, false);
					if (!this.Pr.hasD(M2MoverPr.DECL.FLAG2))
					{
						if (!base.hasFoot() && this.Phy.gravity_added_velocity > 0.125f)
						{
							this.Pr.addD(M2MoverPr.DECL.FLAG2);
						}
					}
					else if (base.hasFoot() || this.Pr.wallHittedA())
					{
						this.Pr.changeState(PR.STATE.ENEMY_SINK);
						this.Pr.remD((M2MoverPr.DECL)10);
						goto IL_1F91;
					}
					float num9 = 0.125f;
					float num10 = (base.hasD(M2MoverPr.DECL.INIT_A) ? 4.2f : 3.2f) / 33f / (0.5f + 0.5f * num9) * global::XX.X.NI(1f, num9, global::XX.X.ZLINE(t, 33f));
					this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._FRIC_STRICT | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, num10 * (float)global::XX.CAim._XD(base.aim, 1), 0f, -1f, 0, 40, 30, 1, 0);
					if (base.hasD(M2MoverPr.DECL.STOP_EVADE) && base.hasD(M2MoverPr.DECL.INIT_A) && t >= 12f)
					{
						this.Pr.remD(M2MoverPr.DECL.STOP_EVADE);
					}
					if (t >= 25f)
					{
						this.Pr.remD((M2MoverPr.DECL)10);
						base.addD(M2MoverPr.DECL.ABORT_BY_MOVE);
					}
					if (t >= 33f && base.hasFoot())
					{
						this.Pr.changeState(PR.STATE.NORMAL);
					}
					goto IL_1F91;
				}
				case PR.STATE.WHEEL:
				case PR.STATE.WHEEL_SHOTGUN:
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						this.FootD.initJump(false, true, false);
						if (this.cyclone_attacked)
						{
							base.addD(M2MoverPr.DECL.INIT_A);
						}
						base.addD((M2MoverPr.DECL)16777352);
						base.remD(M2MoverPr.DECL.STOP_EVADE);
						this.Phy.killSpeedForce(true, true, true, false, false);
						this.Anm.setPose("attack_air1", -1, false);
						if (!this.cyclone_attacked)
						{
							this.Phy.addLockGravity(this, 0f, 400f);
						}
						this.Phy.addFoc(FOCTYPE.WALK_FS, 0.06f * this.mpf_is_right, 0f, -1f, 0, 2, 20, 1, 0);
						this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, -1f);
						t = (float)(100 - (this.cyclone_attacked ? 35 : 20));
					}
					if (!Map2d.can_handle)
					{
						this.Phy.killSpeedForce(true, true, true, false, false);
						this.Pr.changeState(PR.STATE.NORMAL);
						goto IL_1F91;
					}
					if (base.hasD(M2MoverPr.DECL.FLAG2))
					{
						flag = false;
						this.Pr.setBoundsToCrouch();
						if (t >= 30f)
						{
							base.remD((M2MoverPr.DECL)11);
						}
						if (t >= 48f)
						{
							this.Pr.changeState(PR.STATE.NORMAL);
						}
					}
					else if (t < 100f)
					{
						if (base.hasFoot() && !global::XX.X.DEBUGSUPERCYCLONE)
						{
							this.Pr.changeState(PR.STATE.ENEMY_SINK);
							goto IL_1F91;
						}
						if (t < 8f && ((this.mpf_is_right > 0f) ? (!this.Pr.hasRightInput()) : (!this.Pr.hasLeftInput())))
						{
							float num11 = t;
							this.Phy.remFoc(FOCTYPE.WALK, true);
							this.Pr.changeState((!base.hasFoot() && this.isEnable(SkillManager.SKILL_TYPE.airpunch)) ? PR.STATE.AIRPUNCH : PR.STATE.PUNCH);
							t = num11;
							goto IL_1F91;
						}
					}
					else
					{
						float pre_force_velocity_y = this.Phy.pre_force_velocity_y;
						if (!base.hasD(M2MoverPr.DECL.FLAG1))
						{
							if (!global::XX.X.DEBUGSUPERCYCLONE && !base.hasFoot())
							{
								this.cyclone_attacked = true;
							}
							base.addD((M2MoverPr.DECL)33554442);
							this.Anm.setPose("attack_air2", -1, false);
							float baonTime = IN.getBAOnTime();
							this.Phy.killSpeedForce(true, true, true, false, false);
							this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._FRIC_STRICT | FOCTYPE._CHECK_WALL, this.Pr.mpf_is_right * 0.34f, global::XX.X.NI(0.2f, 0.41f, global::XX.X.ZLINE(baonTime, 30f)), -1f, -1, 1, 0, 1, 0);
							this.executeSmallAttack(0, null);
							t = 100f;
						}
						else
						{
							if (this.Phy.isin_water || CCON.isWater(base.Mp.getConfig((int)base.x, (int)base.y)))
							{
								this.Pr.changeState(PR.STATE.ENEMY_SINK);
								goto IL_1F91;
							}
							if (this.Pr.wallHittedA())
							{
								this.wheelBounce(true);
								goto IL_1F91;
							}
							if (!this.Pr.hit_wall_collider)
							{
								t = 100f;
							}
							else if (t >= 107f)
							{
								this.wheelBounce(true);
								goto IL_1F91;
							}
							float num12 = global::XX.X.Mx(0.2f, this.Phy.pre_force_velocity_y);
							if (this.Pr.isBO(0))
							{
								num12 += 0.0069999998f;
							}
							if (!base.hasFoot())
							{
								this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._FRIC_STRICT | FOCTYPE._CHECK_WALL, this.Pr.mpf_is_right * 0.34f, global::XX.X.Mn(num12, 0.41f), -1f, -1, 1, 0, -1, 0);
							}
							if (base.hasFoot() || !this.Pr.canStand((int)base.x, (int)(this.Phy.move_depert_y + 0.12f)))
							{
								t = 0f;
								this.Anm.setPose("attack_air3", -1, false);
								base.addD(M2MoverPr.DECL.FLAG2);
								base.PtcST("wheel_ground_bump", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
								this.Phy.remLockGravity(this);
								this.Phy.remFoc(FOCTYPE.WALK, true);
								this.Phy.addFoc(FOCTYPE.WALK_FS | this.foc_cliff_stopper, this.Pr.mpf_is_right * 0.34f * 0.3f, 0f, -1f, 0, 0, 24, -1, 0);
								goto IL_1F91;
							}
						}
					}
					if (base.hasD(M2MoverPr.DECL.FLAG_HIT))
					{
						flag = true;
						this.Pr.lockSink(60f, true);
					}
					goto IL_1F91;
				case PR.STATE.COMET:
				case PR.STATE.COMET_SHOTGUN:
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						base.addD((M2MoverPr.DECL)16777224);
						base.remD(M2MoverPr.DECL.STOP_EVADE);
						base.addD(M2MoverPr.DECL.DO_NOT_CARRY_BY_OTHER);
						this.FootD.initJump(false, true, false);
						this.Phy.killSpeedForce(true, true, true, false, false);
						this.Anm.setPose("attack_misogi1", -1, false);
						this.Anm.timescale = (float)this.Anm.getDuration() / 20f;
						this.Phy.addLockGravity(this, 0f, 40f);
						this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, -1f);
					}
					if (!Map2d.can_handle)
					{
						this.Phy.killSpeedForce(true, true, true, false, false);
						this.Pr.changeState(PR.STATE.NORMAL);
						goto IL_1F91;
					}
					if (base.hasD(M2MoverPr.DECL.FLAG2))
					{
						flag = false;
						this.Pr.setBoundsToCrouch();
						if (t >= 14f)
						{
							base.remD((M2MoverPr.DECL)11);
						}
						if (t >= 30f)
						{
							this.Pr.changeState(PR.STATE.NORMAL);
						}
					}
					else if (t < 20f)
					{
						this.Pr.setDrawPositionShift(0f, -20f * global::XX.X.ZSIN(t, 20f), 4);
					}
					else
					{
						if (base.hasFoot() || this.Pr.hit_wall_collider || this.Pr.wallHitted(global::XX.AIM.B) || !this.Pr.canStand((int)base.x, (int)(base.mbottom + 0.03f)))
						{
							t = 0f;
							this.Anm.setPose("attack_misogi3", -1, false);
							base.addD(M2MoverPr.DECL.FLAG2);
							base.remD(M2MoverPr.DECL.DO_NOT_CARRY_BY_OTHER);
							base.PtcVar("by", this.Pr.mbottom).PtcST("comet_ground_bump", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							this.Phy.remFoc(FOCTYPE.WALK, true);
							this.Phy.killSpeedForce(false, true, true, true, false);
							this.Phy.remLockGravity(this);
							goto IL_1F91;
						}
						float pre_force_velocity_y2 = this.Phy.pre_force_velocity_y;
						if (!base.hasD(M2MoverPr.DECL.FLAG1))
						{
							base.addD((M2MoverPr.DECL)33554442);
							this.Pr.fineDrawPosition();
							this.Anm.setPose("attack_misogi2", -1, false);
							this.Phy.killSpeedForce(true, true, true, false, false);
							this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._FRIC_STRICT | FOCTYPE._RELEASE | FOCTYPE._CHECK_WALL, 0f, 0.34f, -1f, 0, 6000, 0, 1, 0);
							this.executeSmallAttack(0, null);
							base.TeCon.setEvadeBlink(14f);
							t = 20f;
							this.Pr.addNoDamage(NDMG.DEFAULT, base.TS + 1f);
						}
						else
						{
							this.Pr.addNoDamage(NDMG.DEFAULT, base.TS);
						}
					}
					if (base.hasD(M2MoverPr.DECL.FLAG_HIT))
					{
						flag = true;
						this.Pr.lockSink(60f, true);
					}
					goto IL_1F91;
				case PR.STATE.DASHPUNCH:
				case PR.STATE.DASHPUNCH_SHOTGUN:
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						base.addD((M2MoverPr.DECL)16777226);
						this.Phy.killSpeedForce(true, true, true, false, false);
						t = 0f;
						this.Anm.setPose("attack_dash", -1, false);
						this.Phy.addFoc(FOCTYPE.WALK_FS | this.foc_cliff_stopper, this.mpf_is_right * 0.24f, 0f, -3f, 0, 4, 18, -1, 0);
					}
					if (!base.hasD(M2MoverPr.DECL.FLAG1) && t >= 5f)
					{
						base.addD(M2MoverPr.DECL.FLAG1);
						this.executeSmallAttack(0, null);
						this.Pr.addNoDamage(NDMG.DEFAULT, 10f);
					}
					if (t >= 12f)
					{
						base.addD(M2MoverPr.DECL.FLAG2);
					}
					if (t >= 30f)
					{
						if (base.hasD(M2MoverPr.DECL.STOP_EVADE))
						{
							base.remD((M2MoverPr.DECL)11);
							base.addD(M2MoverPr.DECL.ABORT_BY_MOVE);
						}
					}
					else
					{
						flag = true;
					}
					if (t >= 45f)
					{
						this.Pr.changeState(PR.STATE.NORMAL);
					}
					goto IL_1F91;
				case PR.STATE.AIRPUNCH:
				case PR.STATE.AIRPUNCH_SHOTGUN:
				{
					bool flag7 = false;
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						base.SpSetPose("attack_jumpslash1", -1, null, false);
						if (this.CurMg != null)
						{
							base.Mp.playSnd("swing_pr_1");
						}
						this.Phy.initSoftFall(0.23f, 0f);
						base.addD(M2MoverPr.DECL.FLAG0);
						flag7 = true;
					}
					if (t < 13f)
					{
						float num13 = t;
						if (t < 9f)
						{
							PR.STATE punchVariation2 = this.getPunchVariation(false, false);
							if (this.Pr.isAirPunchState(punchVariation2) && punchVariation2 != PR.STATE.AIRPUNCH && punchVariation2 != PR.STATE.AIRPUNCH_SHOTGUN)
							{
								this.Pr.changeState(punchVariation2);
								this.Phy.remFoc(FOCTYPE.WALK, true);
								t = num13;
								goto IL_1F91;
							}
						}
						if (base.hasFoot())
						{
							this.Pr.changeState(PR.STATE.PUNCH);
							this.Phy.remFoc(FOCTYPE.WALK, true);
							t = num13;
							goto IL_1F91;
						}
					}
					if (!base.hasD(M2MoverPr.DECL.FLAG1) && !base.hasD(M2MoverPr.DECL.INIT_A))
					{
						if (!base.hasFoot())
						{
							float num14 = (1f - 0.6f * global::XX.X.ZPOW(t, 13f)) * 0.15f;
							float num15 = 1f;
							if (base.hasD(M2MoverPr.DECL.INIT_A))
							{
								num15 = 3f;
							}
							else if ((this.mpf_is_right > 0f && this.Pr.isMoveLeftOn()) || (this.mpf_is_right < 0f && this.Pr.isMoveRightOn()))
							{
								num14 *= 0.25f;
							}
							num14 = global::XX.X.absMn(num14 * this.mpf_is_right + this.Phy.releasedVelocity.x * 1.2f, num14);
							if (flag7)
							{
								this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE.SPECIAL_ATTACK | FOCTYPE._FRIC_STRICT, num14, 0f, -1f, 0, 1, 1, 0, 0);
							}
							else
							{
								float num16 = this.Phy.calcFocVelocityX(FOCTYPE.WALK | FOCTYPE.SPECIAL_ATTACK | FOCTYPE._FRIC_STRICT, false);
								this.Phy.remFoc(FOCTYPE.WALK | FOCTYPE.SPECIAL_ATTACK | FOCTYPE._FRIC_STRICT, false);
								this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE.SPECIAL_ATTACK | FOCTYPE._FRIC_STRICT, global::XX.X.VALWALK(num16, num14, 0.006f * num15), 0f, -1f, 0, 1, 1, 0, 0);
							}
							this.Phy.walk_xspeed = global::XX.X.VALWALK(this.Phy.walk_xspeed, 0f, 0.008f);
						}
						else
						{
							base.addD(M2MoverPr.DECL.FLAG1);
							base.SpSetPose(base.hasD(M2MoverPr.DECL.INIT_A) ? "attack2" : "attack_jumpslash2", -1, null, false);
						}
					}
					if (t >= 20f)
					{
						base.addD(M2MoverPr.DECL.FLAG2);
					}
					if (t >= 13f && !base.hasD(M2MoverPr.DECL.INIT_A))
					{
						base.addD(M2MoverPr.DECL.INIT_A);
						this.Phy.addFoc(FOCTYPE.WALK_FS, this.mpf_is_right * -0.14f, 0f, -1f, 0, 1, 10, 1, 0);
						base.SpSetPose(base.hasD(M2MoverPr.DECL.FLAG1) ? "attack2" : "attack_jumpslash2", -1, null, false);
						this.executeBasicPunch(0.6f);
					}
					if (t >= 32f)
					{
						base.remD((M2MoverPr.DECL)10);
						base.addD(M2MoverPr.DECL.ABORT_BY_MOVE);
					}
					if (t >= 32f)
					{
						this.Pr.changeState(PR.STATE.NORMAL);
					}
					goto IL_1F91;
				}
				default:
					if (state != PR.STATE.SHIELD_BUSH)
					{
						return false;
					}
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						base.remD(M2MoverPr.DECL.STOP_EVADE);
						if (this.evade_t <= 0f)
						{
							this.Pr.changeState(PR.STATE.NORMAL);
							goto IL_1F91;
						}
						if (!this.Shield.activate(false))
						{
							base.SpSetPose(this.guard_pose, -1, null, false);
							goto IL_1F91;
						}
						this.Shield.deactivate(true, true);
						this.Shield.applyDamage(90f);
						if (base.getEH(EnhancerManager.EH.shield_cat))
						{
							this.FlgSoftFall.Add("MAGIC");
						}
						base.addD((M2MoverPr.DECL)16777290);
						base.remD(M2MoverPr.DECL.ALLOC_SHIELD_HOLD);
						base.SpSetPose("guard_bush", -1, null, false);
						base.PtcVar("maxt", 70f).PtcVar("applyt", 18f).PtcVar("radius", 2.8f * base.Mp.CLENB)
							.PtcST("guard_bush_apply", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
						this.Pr.TeCon.setColorBlinkBush(4f, 18f, 1f, 345239, 0);
						this.executeSmallAttack(0, null);
						this.Pr.addNoDamage(NDMG.DEFAULT, 16f);
						this.parry_t = 18f;
						if (!this.Pr.forceCrouch(false, false))
						{
							this.Pr.quitCrouch(false, false, false);
						}
						t = 0f;
					}
					this.Shield.recover_lock_t = 90f;
					if (t >= 47f)
					{
						base.remD(M2MoverPr.DECL.STOP_EVADE);
						base.addD(M2MoverPr.DECL.ABORT_BY_MOVE);
						base.SpSetPose("guard_bush2stand", -1, null, false);
						if (this.Pr.isEvadeO(0))
						{
							this.runEvadeCheck(0f, false, true);
						}
					}
					if (base.hasD(M2MoverPr.DECL.FLAG_HIT))
					{
						flag = true;
						this.Pr.lockSink(90f, true);
					}
					if (t < 70f)
					{
						goto IL_1F91;
					}
					this.Pr.changeState(PR.STATE.NORMAL);
					if (this.evade_t > 0f)
					{
						this.Shield.immediateGuard();
						goto IL_1F91;
					}
					this.shieldBlur(false, true);
					goto IL_1F91;
				}
			}
			else if (state != PR.STATE.SHIELD_LARIAT)
			{
				if (state != PR.STATE.BURST_SCAPECAT)
				{
					if (state != PR.STATE.USE_BOMB)
					{
						return false;
					}
					if (!this.runItemBomb(ref t) && base.state == PR.STATE.USE_BOMB)
					{
						this.Pr.changeState(PR.STATE.NORMAL);
					}
					goto IL_1F91;
				}
			}
			else
			{
				if (!base.hasD(M2MoverPr.DECL.FLAG0))
				{
					base.remD((M2MoverPr.DECL)10);
					if (this.evade_t <= 0f)
					{
						this.Pr.changeState(PR.STATE.NORMAL);
						goto IL_1F91;
					}
					if (!this.Shield.changeStateLariat(base.TS, false, false))
					{
						base.remD((M2MoverPr.DECL)7);
						base.SpSetPose(this.guard_pose, -1, null, false);
						goto IL_1F91;
					}
					t = 1f;
					base.addD((M2MoverPr.DECL)32775);
					base.addD((M2MoverPr.DECL)16777288);
					base.remD(M2MoverPr.DECL.ALLOC_SHIELD_HOLD);
					if (base.getEH(EnhancerManager.EH.shield_cat))
					{
						this.FlgSoftFall.Add("MAGIC");
					}
					if (!this.Pr.forceCrouch(false, false))
					{
						this.Pr.quitCrouch(false, false, false);
					}
					base.SpSetPose("guard_slash", -1, null, false);
				}
				if (!this.Shield.isLariatState())
				{
					this.Pr.changeState(PR.STATE.NORMAL);
					goto IL_1F91;
				}
				this.Shield.changeStateLariat(base.TS, true, base.isAtkO() && t >= 12f);
				if (this.Shield.isManipulatableState())
				{
					goto IL_1F91;
				}
				if (!base.hasD(M2MoverPr.DECL.FLAG_HIT))
				{
					base.addD(M2MoverPr.DECL.FLAG_HIT);
					base.remD(M2MoverPr.DECL.FORCE_SHIELD_KEY_HOLD);
				}
				if (t >= 30f)
				{
					base.remD((M2MoverPr.DECL)7);
				}
				goto IL_1F91;
			}
			if (first)
			{
				UIStatus.showHold(60, false);
				base.addD(M2MoverPr.DECL.DO_NOT_CARRY_BY_OTHER);
				this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, -1f);
				this.killHoldMagic(false);
				base.SpSetPose("burst_0", 1, null, false);
				if (this.Pr.forceCrouch(false, false))
				{
					this.Pr.need_check_bounds = false;
				}
				PostEffect it = PostEffect.IT;
				it.setSlow(38f, 0.25f, 0);
				it.addTimeFixedEffect(it.setPEfadeinout(POSTM.BGM_LOWER, 43f, 110f, 1f, -29), 1f);
				it.addTimeFixedEffect(this.Anm, 1f);
				this.mana_drain_lock_t = 200f;
				base.PtcVar("cx", base.x).PtcVar("cy", base.y).PtcVar("time", 36f);
				base.PtcSTTimeFixed((base.state == PR.STATE.BURST) ? "burst_prepare" : "burst_scapecat_prepare", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				it.addTimeFixedEffect(it.setPEfadeinout(POSTM.BURST, 46f, 20f, 1f, 0), 1f);
				if (base.state == PR.STATE.BURST)
				{
					it.addTimeFixedEffect(this.NM2D.Cam.TeCon.setBounceZoomIn(1.25f, 38f, 108f, -2), 1f);
				}
				else
				{
					it.addTimeFixedEffect(this.NM2D.Cam.TeCon.setBounceZoomIn(2f, 1f, 18f, -1), 1f);
					base.TeCon.setFadeOut_in(216f, 0f, -108);
				}
				this.NM2D.Cam.Qu.HandShake(2f, 40f, 14f, 0);
				this.FootD.initJump(false, true, false);
				this.Phy.killSpeedForce(true, true, true, false, false).clearLock().addLockGravity(this, 0f, 9f);
				this.Pr.getAbsorbContainer().gacha_renderable = false;
			}
			this.punch_t = 0f;
			if (!base.hasD(M2MoverPr.DECL.FLAG0))
			{
				base.TeCon.clear();
				if (t >= 9f)
				{
					base.addD(M2MoverPr.DECL.FLAG0);
					int reduceMp = MagicSelector.getReduceMp(MGKIND.PR_BURST);
					if (base.mp < (float)reduceMp)
					{
						this.mp_overused += global::XX.X.Mx(0f, (float)reduceMp - base.mp);
					}
					MagicItem magicItem = this.executeSmallAttack(0, null);
					base.SpSetPose("burst_1", -1, null, false);
					if (this.Pr.forceCrouch(false, false))
					{
						this.Pr.need_check_bounds = false;
					}
					base.PtcST((base.state == PR.STATE.BURST) ? "burst_after" : "burst_scapecat_after", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.Pr.recoverGoSer();
					this.Phy.killSpeedForce(true, true, true, false, false);
					base.EggCon.need_fine_mp = true;
					base.AbsorbCon.releaseFromTarget(this.Pr);
					PostEffect it2 = PostEffect.IT;
					it2.addTimeFixedEffect(it2.setPEabsorbed(POSTM.FLASH, 4f, 87f, 1f, 0), 1f);
					this.NM2D.Cam.Qu.Vib(2f, 7f, 1f, 0).SinV(9f, 28f, 0.25f, 0);
					base.TeCon.removeSpecific(TEKIND.FADEOUT_IN);
					PostEffect.IT.setSlow(10f, 0f, 0);
					if (base.state == PR.STATE.BURST)
					{
						if (global::XX.X.XORSP() < this.BurstSel.fainted_ratio)
						{
							base.Ser.Add(SER.BURST_TIRED, 320, 99, false);
							this.checkOverrunTired(false);
						}
						else
						{
							this.checkOverrunTired(true);
						}
					}
					else
					{
						M2PrMistApplier mistApplier = this.Pr.getMistApplier();
						if (mistApplier != null)
						{
							mistApplier.cureAll();
						}
						this.Pr.PtcHld.killPtc("burst_scapecat_prepare", false);
						if (this.NM2D.GameOver != null)
						{
							this.NM2D.GameOver.executeScapecatRespawn(-1);
						}
					}
					this.NM2D.MIST.burstMistArea((int)base.x, (int)base.y, 5);
					M2PrMistApplier mistApplier2 = this.Pr.getMistApplier();
					if (mistApplier2 != null && this.Pr.enemy_targetted > 0)
					{
						mistApplier2.cureAll();
					}
					this.addManaDrainLock(magicItem, 1f, 0f);
				}
				else
				{
					this.Phy.addFoc(FOCTYPE.SPECIAL_ATTACK, 0f, -0.05f * global::XX.X.ZSIN(t + 1f, 9f), -1f, -1, 1, 0, -1, 0);
				}
			}
			else
			{
				this.Phy.killSpeedForce(true, true, true, false, false);
			}
			if (t >= 54f)
			{
				this.Pr.addNoDamage(NDMG._BURST_PREVENT, 70f);
				this.Pr.abortAbsorbForce();
				this.Pr.changeState(PR.STATE.NORMAL);
				this.Pr.NCM.fine_all = true;
			}
			IL_1F91:
			if (flag)
			{
				manip |= M2MoverPr.PR_MNP.NO_SINK;
			}
			else
			{
				manip &= (M2MoverPr.PR_MNP)(-3);
			}
			return true;
		}

		public void wormFocApplied(global::XX.AIM a)
		{
			PR.STATE state = base.state;
			if (state - PR.STATE.WHEEL > 1)
			{
				if (state - PR.STATE.COMET <= 1 && base.hasD(M2MoverPr.DECL.FLAG1))
				{
					this.Phy.killSpeedForce(true, true, true, false, false);
					this.Pr.changeState(PR.STATE.ENEMY_SINK);
					return;
				}
			}
			else if (base.hasD(M2MoverPr.DECL.FLAG1))
			{
				this.Pr.changeState(PR.STATE.ENEMY_SINK);
			}
		}

		public void checkOverrunTired(bool set_burst_tired = true)
		{
			if (this.mp_overused >= 100f)
			{
				float num = global::XX.X.NIL(180f, 100f, (float)(base.Ser.getLevel(SER.OVERRUN_TIRED) + 1), 4f);
				if (this.mp_overused >= num)
				{
					this.mp_overused -= num;
					base.Ser.Add(SER.OVERRUN_TIRED, -1, 99, false);
					if (set_burst_tired)
					{
						base.Ser.Add(SER.BURST_TIRED, 320, 99, false);
					}
				}
			}
		}

		public void changeState(PR.STATE prestate, PR.STATE state)
		{
			if (this.Pr.isPunchState())
			{
				this.Phy.remLockGravity(this);
				this.Phy.remLockMoverHitting(HITLOCK.SPECIAL_ATTACK);
			}
			bool flag = this.Pr.isDamagingOrKo(state);
			if (prestate == PR.STATE.SHIELD_BUSH)
			{
				this.Pr.PtcHld.killPtc("guard_bush_apply", false);
			}
			if (this.Pr.isMagicState(state))
			{
				this.FlgSoftFall.Add("MAGIC");
			}
			this.stk_magic_t = 0f;
			if (prestate == PR.STATE.MAG_EXPLODED && state != PR.STATE.MAG_EXPLODED)
			{
				this.mp_hold = (this.mp_overhold = 0f);
				this.checkOverrunTired(true);
				this.CurMg = null;
				this.FlgSoftFall.Rem("MAGIC");
			}
			else if (this.Pr.isMagicExistState(prestate) && !this.Pr.isMagicExistState(state))
			{
				this.initMagicSleep(!flag, false);
			}
			if (prestate == PR.STATE.AIRPUNCH || prestate == PR.STATE.AIRPUNCH_SHOTGUN)
			{
				this.Phy.quitSoftFall(0f);
			}
			if (this.Pr.isFixAimState(prestate))
			{
				this.Pr.fix_aim = false;
				this.Pr.setAim(base.aim, true);
			}
			if (this.Pr.isFixAimState(state))
			{
				this.Pr.fix_aim = true;
			}
			if (this.Pr.isShieldAttackState(prestate))
			{
				this.FlgSoftFall.Rem("MAGIC");
			}
			this.magic_t = 0f;
			if (state != PR.STATE.NORMAL)
			{
				if (this.Pr.isShieldAttackState(state))
				{
					this.Pr.addD(M2MoverPr.DECL.ALLOC_SHIELD_HOLD);
					this.Pr.remD((M2MoverPr.DECL)10);
				}
				else
				{
					if (this.evade_t >= 0f)
					{
						this.evade_t = -1f;
					}
					this.shieldBlur(false, true);
				}
				if (this.CurMg == null)
				{
					this.MagicSel.deactivate();
				}
				else
				{
					this.MagicSel.deactivateEffect(true);
				}
				if (this.Pr.isMagicExistState(state))
				{
					this.Pr.remD((M2MoverPr.DECL)11);
					return;
				}
				this.Pr.killPtc(PtcHolder.PTC_HOLD.ACT);
				return;
			}
			else
			{
				this.CurSkill = null;
				this.evadelock_on_jump_ = false;
				if (this.Pr.isBenchOrGoRecoveryState(prestate))
				{
					this.killHoldMagic(false);
					this.shieldBlur(true, true);
					IN.clearPushDown(true);
					this.punch_t = (this.evade_t = 0f);
					this.punch_decline_time = 10;
					return;
				}
				if (base.Mp.floort > 10f)
				{
					this.runEvadeCheck(0f, false, true);
					if (this.evade_t <= 0f)
					{
						this.FlgSoftFall.Rem("SHIELD");
					}
					if (this.evade_t == 0f)
					{
						this.runPunchCheck(0f);
						if (this.punch_t == 0f)
						{
							this.runMagicCheck(0f);
						}
					}
				}
				return;
			}
		}

		public string getPoseTitleOnNormal()
		{
			if (this.CarryBox != null && this.magic_t > 0f)
			{
				if (this.FootD.FootIsLadder())
				{
					return "chant_ladder";
				}
				if (!base.hasFoot())
				{
					return "";
				}
				return "interact";
			}
			else if (this.isChantingMagicOnNormal(false, true))
			{
				if (this.FootD.FootIsLadder())
				{
					return "chant_ladder";
				}
				if (!base.Ser.isWeakPose())
				{
					return "chant";
				}
				return "chant_weak";
			}
			else
			{
				if (this.isShieldOpeningOnNormal(true, false))
				{
					return this.guard_pose;
				}
				return "";
			}
		}

		public string guard_pose
		{
			get
			{
				if (this.FootD.FootIsLadder())
				{
					return "guard_ladder";
				}
				if (!base.is_crouch)
				{
					return "guard";
				}
				return "guard_crouch";
			}
		}

		public void strongEnemySink()
		{
			if (this.Pr.isPunchState() && !this.Pr.isBurstState())
			{
				PR.STATE state = base.state;
				if (state != PR.STATE.PUNCH && state - PR.STATE.AIRPUNCH > 1 && state != PR.STATE.SHIELD_BUSH)
				{
					this.Pr.changeState(PR.STATE.ENEMY_SINK);
				}
			}
		}

		public bool considerFricOnVelocityCalc()
		{
			if (this.Pr.isNormalState())
			{
				return (this.isShieldOpeningOnNormal(false, false) && this.skill_on_guard) || this.isChantingMagicOnNormal(false, false);
			}
			switch (base.state)
			{
			case PR.STATE.PUNCH:
			case PR.STATE.AIRPUNCH:
			case PR.STATE.AIRPUNCH_SHOTGUN:
				return false;
			case PR.STATE.WHEEL:
			case PR.STATE.WHEEL_SHOTGUN:
			case PR.STATE.COMET:
			case PR.STATE.COMET_SHOTGUN:
				if (!base.hasD(M2MoverPr.DECL.FLAG1))
				{
					return false;
				}
				break;
			}
			return this.Pr.isMagicExistState() || this.Pr.isPunchState() || this.Pr.isShieldAttackState();
		}

		public M2MoverPr.PR_MNP runPunchCheck(float TS)
		{
			if (base.isAtkPD(26) || (this.punch_t > 0f && base.isAtkO()))
			{
				if (CFG.magsel_decide_type != CFG.MAGSEL_TYPE.NORMAL && this.punch_t == 0f && global::XX.X.BTWS(0f, this.magic_t, (float)this.MAGIC_CHANT_DELAY))
				{
					bool flag = false;
					switch (CFG.magsel_decide_type)
					{
					case CFG.MAGSEL_TYPE.SUBMIT:
						flag = this.Pr.isSubmitPD(26);
						break;
					case CFG.MAGSEL_TYPE.Z:
						flag = this.Pr.isAtkPD(26);
						break;
					case CFG.MAGSEL_TYPE.C:
						flag = this.Pr.isTargettingPD(26);
						break;
					}
					if (flag)
					{
						this.punch_decline_time = 26;
						return (M2MoverPr.PR_MNP)0;
					}
				}
				bool flag2 = base.Ser.punchAllow() && !base.hasD_stopact();
				bool flag3 = base.isMagicO() && this.Pr.isBurstAllocState();
				if (this.punch_t <= 0f && this.FootD.FootIsLadder())
				{
					this.FootD.initJump(false, false, true);
				}
				if (!flag3 && flag2 && TS > 0f)
				{
					PR.STATE punchVariation = this.getPunchVariation(true, false);
					if (punchVariation != PR.STATE.PUNCH && punchVariation != PR.STATE.NORMAL)
					{
						if (base.Ser.punchAllow())
						{
							this.Pr.changeState(punchVariation);
						}
						return M2MoverPr.PR_MNP.CHANGED_STATE;
					}
				}
				M2MoverPr.PR_MNP pr_MNP = (M2MoverPr.PR_MNP)0;
				if (flag3 && base.is_alive)
				{
					if (this.punch_t < 128f)
					{
						this.punch_t = 128f;
					}
					else if (this.punch_t < 143f && TS > 0f)
					{
						this.punch_t = global::XX.X.Mn(this.punch_t + 1f, 143f);
					}
					if (TS > 0f && this.punch_t - 128f >= 15f)
					{
						if (this.Pr.isMagicExistState() && base.state != PR.STATE.NORMAL)
						{
							this.Pr.changeState(PR.STATE.NORMAL);
						}
						if (this.CurMg != null)
						{
							this.killHoldMagic(false);
							this.punch_decline_time = 15;
						}
						this.magic_t = 0f;
						if (this.CarryBox != null)
						{
							this.CarryBox.deactivate(false);
						}
						if (!this.BurstSel.isActive() && this.NM2D.WM.CurWM != null && TX.valid(this.NM2D.WM.CurWM.zx_evt) && !PUZ.IT.barrier_active && !this.NM2D.FlgWarpEventNotInjectable.isActive())
						{
							if (this.Pr.isEventInjectable(true))
							{
								EV.stack(this.NM2D.WM.CurWM.zx_evt, 0, -1, null, null);
							}
							this.punch_t = 0f;
							return (M2MoverPr.PR_MNP)0;
						}
						if (this.BurstSel.runActivating(ref this.punch_t, 143f, 1024f, base.Ser.punchAllow() && !base.hasD(M2MoverPr.DECL.STOP_BURST) && (!base.isPuzzleManagingMp() || this.temp_puzzle_mp != 0f)))
						{
							this.Pr.changeState(PR.STATE.BURST);
							return M2MoverPr.PR_MNP.CHANGED_STATE;
						}
						if (this.BurstSel.isActive())
						{
							if (this.BurstSel.stop_sink)
							{
								pr_MNP |= M2MoverPr.PR_MNP.NO_SINK;
							}
							pr_MNP |= M2MoverPr.PR_MNP.NO_JUMP_AND_MOVING;
						}
					}
				}
				else
				{
					this.BurstSel.runDeactivating(false);
					this.punch_t = global::XX.X.MMX(0.125f, this.punch_t + TS, 50f);
				}
				return ((this.punch_t < 10f) ? M2MoverPr.PR_MNP.NO_SINK : ((M2MoverPr.PR_MNP)0)) | pr_MNP;
			}
			this.BurstSel.runDeactivating(false);
			if (this.punch_t > 0f)
			{
				if (base.Mp.Pr == this.Pr && base.Mp.TalkTarget != null && this.Pr.isCheckU(0))
				{
					this.punch_t = 0f;
				}
				else
				{
					this.punch_t = (float)(((this.punch_t >= 20f && this.punch_t < 128f) || this.punch_decline_time_ > 0) ? 0 : (-7));
				}
			}
			if (this.punch_t >= 0f)
			{
				return (M2MoverPr.PR_MNP)0;
			}
			this.punch_t = global::XX.X.Mn(this.punch_t + TS, 0f);
			if (this.punch_decline_time_ == 0 && !this.Pr.is_crouch && !this.FootD.FootIsLadder() && base.Ser.punchAllow())
			{
				if (TS > 0f && !base.hasD_stopact())
				{
					this.Pr.changeState(PR.STATE.PUNCH);
				}
				else
				{
					if (TS <= 0f || base.hasD_stopevade() || !this.Pr.isDamagingOrKo())
					{
						return M2MoverPr.PR_MNP.NO_SINK;
					}
					this.Pr.changeState(PR.STATE.UKEMI);
				}
				return M2MoverPr.PR_MNP.CHANGED_STATE;
			}
			return M2MoverPr.PR_MNP.NO_SINK;
		}

		public void forcePunchQuit()
		{
			this.punch_t = 0f;
		}

		public void initMenu()
		{
			this.BurstSel.deactivate();
			if (this.punch_t >= 143f)
			{
				this.punch_t = 143f;
			}
		}

		public bool canClimbToLadder()
		{
			return this.punch_t <= 0f;
		}

		public bool canGroundPunchFoot()
		{
			return base.hasFoot() && !this.FootD.FootIsLadder();
		}

		private PR.STATE getPunchVariation(bool is_punch_check, bool execution = false)
		{
			PR.STATE state = PR.STATE.PUNCH;
			bool flag = false;
			bool flag2 = this.isEnable(SkillManager.SKILL_TYPE.guard_lariat);
			bool flag3 = this.isEnable(SkillManager.SKILL_TYPE.guard_bush);
			if (is_punch_check && this.skill_on_guard && (base.MistApply == null || base.MistApply.canOpenShield()) && this.evade_t > 0f && this.Shield.isActive() && (flag2 || flag3))
			{
				if (!this.Pr.isMagicO(0))
				{
					if (flag2 && (this.Pr.isLO(4) || this.Pr.isRO(4)))
					{
						state = PR.STATE.SHIELD_LARIAT;
						flag = true;
					}
					else if (flag3)
					{
						state = PR.STATE.SHIELD_BUSH;
					}
				}
			}
			else if (this.canGroundPunchFoot())
			{
				if (is_punch_check && base.canJump() && (this.Pr.forceCrouch(false, false) || this.Pr.isBO(0) || this.Pr.isCrouchingState()) && this.punch_t < 5f)
				{
					state = PR.STATE.SLIDING;
					flag = true;
				}
				if (this.Pr.run_continue_time >= 22f && !this.Phy.isin_water && this.isEnable(SkillManager.SKILL_TYPE.dashpunch))
				{
					state = ((this.CurMg != null) ? PR.STATE.DASHPUNCH_SHOTGUN : PR.STATE.DASHPUNCH);
				}
			}
			else
			{
				if (!is_punch_check || (this.punch_t > 4f && !this.Pr.isRunO()) || this.punch_t < 0f)
				{
					if ((this.Pr.isLO(4) || this.Pr.isRO(4)) && this.Pr.canStand((int)base.x, (int)(base.mbottom + 0.12f)) && this.isEnable(SkillManager.SKILL_TYPE.wheel))
					{
						state = ((this.CurMg != null) ? PR.STATE.WHEEL_SHOTGUN : PR.STATE.WHEEL);
						flag = true;
					}
					if (this.Pr.isBO(4) && this.Pr.canStand((int)base.x, (int)(base.mbottom + 0.12f)) && this.isEnable(SkillManager.SKILL_TYPE.comet))
					{
						state = ((this.CurMg != null) ? PR.STATE.COMET_SHOTGUN : PR.STATE.COMET);
					}
					if (state == PR.STATE.PUNCH && this.isEnable(SkillManager.SKILL_TYPE.airpunch))
					{
						state = ((this.CurMg != null) ? PR.STATE.AIRPUNCH_SHOTGUN : PR.STATE.AIRPUNCH);
						flag = true;
					}
				}
				if (state == PR.STATE.PUNCH && this.FootD.FootIsLadder())
				{
					return PR.STATE.NORMAL;
				}
			}
			if (flag)
			{
				if (this.Pr.isLO(0))
				{
					base.setAim(global::XX.AIM.L, false);
				}
				if (this.Pr.isRO(0))
				{
					base.setAim(global::XX.AIM.R, false);
				}
			}
			if (execution && state != PR.STATE.PUNCH)
			{
				this.Pr.changeState(state);
			}
			return state;
		}

		public bool isParryable(NelAttackInfo Atk)
		{
			if (this.parry_t <= 0f || Atk.shield_break_ratio == 0f)
			{
				return false;
			}
			if (base.state == PR.STATE.PUNCH)
			{
				if (!Atk.parryable)
				{
					return false;
				}
				if ((this.Pr.mpf_is_right < 0f) ? (Atk.hit_x < base.x - 0.12f) : (base.x + 0.12f < Atk.hit_x))
				{
					return true;
				}
			}
			if (base.state != PR.STATE.SHIELD_BUSH)
			{
				return false;
			}
			if (this.Pr.get_state_time() >= 18f)
			{
				return false;
			}
			if (this.Pr.isNoDamageActive(Atk.ndmg))
			{
				return false;
			}
			float num;
			if (M2NoDamageManager.isMapDamageKey(Atk.ndmg))
			{
				num = 18f - this.Pr.get_state_time();
			}
			else
			{
				num = 70f - this.Pr.get_state_time();
			}
			base.addD(M2MoverPr.DECL.FLAG_HIT);
			this.Pr.addNoDamage(Atk.ndmg, num);
			this.Pr.getPhysic().addLockMoverHitting(HITLOCK.EVADE, 70f - this.Pr.get_state_time());
			return true;
		}

		public bool cannotHitToEnemy(NelEnemy En)
		{
			return 0f < this.swaysld_t && this.swaysld_t < 100f;
		}

		public void abortSwaySliding()
		{
			if (0f < this.swaysld_t && this.swaysld_t < 180f)
			{
				this.swaysld_t = -(180f - this.swaysld_t);
			}
		}

		public bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitItem)
		{
			if (HitItem == null || Mg.Ray == null)
			{
				return true;
			}
			bool flag = Mg.kind == MGKIND.PR_SHOTGUN || base.state == PR.STATE.WHEEL_SHOTGUN || base.state == PR.STATE.COMET_SHOTGUN;
			if ((hittype & HITTYPE.WALL) != HITTYPE.NONE)
			{
				if (Mg.kind == MGKIND.PR_SLIDING && (hittype & HITTYPE.EN) == HITTYPE.NONE && this.Pr.canStand((int)(this.Pr.x + (this.Pr.sizex + 0.02f) * this.mpf_is_right), (int)base.y))
				{
					return false;
				}
				Mg.Ray.hittype &= (HITTYPE)(-5);
				if (Mg.kind != MGKIND.PR_WHEEL && Mg.kind == MGKIND.PR_COMET)
				{
					Mg.killEffect();
				}
				base.PtcVar("x", base.Mp.globaluxToMapx(HitItem.hit_ux)).PtcVar("y", base.Mp.globaluyToMapy(HitItem.hit_uy)).PtcVar("ax", (float)((Mg.sx > 0f) ? 1 : (-1)))
					.PtcST(flag ? "hit_shotgun_wall" : "hit_tackle_wall", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				Mg.PadVib("attack_wall", false, 1f);
				if ((hittype & HITTYPE.EN) == HITTYPE.NONE)
				{
					return false;
				}
			}
			NelEnemy nelEnemy = HitItem.Hit as NelEnemy;
			M2Attackable m2Attackable = HitItem.Hit as M2Attackable;
			bool flag2 = (hittype & (HITTYPE)7) == HITTYPE.EN && m2Attackable != null;
			if ((Mg.kind == MGKIND.PR_SHOTGUN || Mg.kind == MGKIND.PR_PUNCH) && base.isPunchState() && this.CurMg != null && flag2)
			{
				this.parry_t = 0f;
				if (Mg.kind == MGKIND.PR_SHOTGUN && (hittype & HITTYPE.PR_AND_EN) != HITTYPE.NONE && this.publishShotgunHit(Mg, HitItem, 0.47123894f, false, -2))
				{
					this.magic_t = -100f;
					if (base.state == PR.STATE.PUNCH)
					{
						this.Pr.changeState(PR.STATE.EVADE_SHOTGUN);
					}
					else
					{
						this.Pr.changeState(PR.STATE.NORMAL);
						this.wheelBounce(false);
						this.punch_decline_time = 3;
						this.Phy.addFoc(FOCTYPE.WALK_FS, -this.mpf_is_right * 0.14f, 0f, -1f, 0, 1, 15, 1, 0);
						Mg.phase = -1;
					}
				}
			}
			if (Mg.kind == MGKIND.PR_COMET)
			{
				if (flag2 && base.isPunchState())
				{
					Atk.burst_vx = global::XX.X.NIXP(-0.03f, 0.03f);
					base.addD(M2MoverPr.DECL.FLAG_HIT);
					if (this.publishShotgunHit(Mg, HitItem, 1.5707964f, true, -1))
					{
						return true;
					}
				}
				if (HitItem.Hit is M2ManaWeed)
				{
					return EnemySummoner.isActiveBorder();
				}
			}
			if (Mg.kind == MGKIND.PR_DASHPUNCH)
			{
				if (flag2 && base.isPunchState())
				{
					base.addD(M2MoverPr.DECL.FLAG_HIT);
					if (this.publishShotgunHit(Mg, HitItem, 0f, true, -2))
					{
						return true;
					}
				}
				if (HitItem.Hit is M2ManaWeed)
				{
					return EnemySummoner.isActiveBorder();
				}
			}
			if (Mg.kind == MGKIND.PR_WHEEL)
			{
				if (flag2 && base.isPunchState())
				{
					if (!flag)
					{
						this.wheelBounce(true);
						Mg.phase = -1;
					}
					else
					{
						base.addD(M2MoverPr.DECL.FLAG_HIT);
						this.publishShotgunHit(Mg, HitItem, -0.37699112f, true, global::XX.X.Mx(5, global::XX.X.IntC((float)Mg.reduce_mp * 0.5f)));
					}
				}
				return !(HitItem.Hit is M2ManaWeed) || EnemySummoner.isActiveBorder();
			}
			if (Mg.kind == MGKIND.PR_SLIDING && base.isSlidingState())
			{
				if (flag2 && (hittype & HITTYPE.EN) != HITTYPE.NONE)
				{
					if (0f < this.swaysld_t && this.swaysld_t < 100f)
					{
						return false;
					}
					this.Pr.changeState(PR.STATE.ENEMY_SINK);
					return true;
				}
				else if (HitItem.Hit is M2ManaWeed)
				{
					return EnemySummoner.isActiveBorder();
				}
			}
			if (Mg.kind == MGKIND.PR_SHIELD_BUSH && nelEnemy != null)
			{
				base.addD(M2MoverPr.DECL.FLAG_HIT);
				Atk.BurstDir((float)global::XX.X.MPF(nelEnemy.x > this.Pr.x));
				return true;
			}
			if (Mg.kind == MGKIND.PR_SHIELD_LARIAT && flag2 && nelEnemy != null)
			{
				if (this.Shield.overdamage)
				{
					Atk.hpdmg_current = 0;
				}
				this.Shield.applyDamage(120f);
				Atk.BurstDir((float)global::XX.X.MPF(nelEnemy.x > this.Pr.x));
			}
			if (Mg.kind == MGKIND.PR_BURST && nelEnemy != null)
			{
				Atk.BurstDir((float)global::XX.X.MPF(nelEnemy.x > this.Pr.x));
			}
			return true;
		}

		private bool publishShotgunHit(MagicItem Mg, M2Ray.M2RayHittedItem HitItem, float right_agR, bool replace_normal, int reduce_mag = -2)
		{
			if (this.CurMg == null || !this.CurMg.isPreparingCircle || this.mp_hold < 1f)
			{
				return false;
			}
			bool flag = reduce_mag >= 0;
			bool flag2 = reduce_mag <= -2;
			reduce_mag = ((reduce_mag < 0) ? ((int)this.mp_hold) : ((int)global::XX.X.Mn(this.mp_hold, (float)reduce_mag)));
			PostEffect it = PostEffect.IT;
			float num = base.Mp.globaluxToMapx(HitItem.hit_ux);
			float num2 = base.Mp.globaluyToMapy(HitItem.hit_uy);
			if (Mg.da < 1f || !flag2)
			{
				PostEffect.IT.setPEfadeinout(POSTM.SHOTGUN, 10f, 12f, 0.2f, -10);
				it.addTimeFixedEffect(flag ? PostEffect.IT.setPEfadeinout(POSTM.SHOTGUN, 10f, 12f, 0.2f, -10) : it.setPEfadeinout(POSTM.SHOTGUN, 10f, 15f, 0.25f, -7), 1f);
			}
			else
			{
				it.setSlow(7f, 0f, 0);
				it.addTimeFixedEffect(it.setPEfadeinout(POSTM.SHOTGUN, 10f, 15f, 1f, -7), 1f);
				base.PtcVar("x", num).PtcVar("y", num2);
				base.PtcSTTimeFixed("shotgun_pre", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				base.Mp.DmgCntCon.draw_delay = 1f;
			}
			this.Pr.PtcVar("x", (double)num).PtcVar("y", (double)num2).PtcVar("agR", (double)((this.mpf_is_right > 0f) ? right_agR : (3.1415927f - right_agR)))
				.PtcST((Mg.da >= 1f) ? "shotgun_post" : "shotgun_post_small", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			bool flag3 = false;
			MagicItem curMg = this.CurMg;
			if (reduce_mag > 0)
			{
				float num3 = MDAT.crystalizeRatio(this.Pr, this.CurMg.mp_crystalize);
				int num4 = 0;
				this.OcSlots.reserve_consume_flag = true;
				if (!base.isPuzzleManagingMp() && this.mp_hold - (float)reduce_mag < 1f)
				{
					num4 = this.OcSlots.clearMagic(this.CurMg, true);
					int num5 = global::XX.X.IntC(global::XX.X.Mn(this.mp_overhold, (float)reduce_mag));
					if (num5 > 0)
					{
						this.mp_overused += (float)num5;
						this.mana_drain_lock_t = 50f;
						this.checkOverrunTired(true);
					}
				}
				if (num3 > 0f)
				{
					int reduceableMana = this.getReduceableMana(global::XX.X.IntC((float)reduce_mag * num3), num4);
					int num6 = (int)((float)reduce_mag * num3 * this.CurMg.crystalize_neutral_ratio);
					int num7 = reduceableMana - num6;
					this.NM2D.Mana.AddMulti(base.Mp.globaluxToMapx(HitItem.hit_ux), base.Mp.globaluyToMapy(HitItem.hit_uy), (float)num7, (MANA_HIT)10);
					this.NM2D.Mana.AddMulti(base.Mp.globaluxToMapx(HitItem.hit_ux), base.Mp.globaluyToMapy(HitItem.hit_uy), (float)num6, (MANA_HIT)11);
				}
				if (num4 == 0)
				{
					int num8 = this.Pr.applyMpDamage(reduce_mag, true, null, false, false);
					this.Pr.GSaver.addSavedMp((float)num8 * DIFF.shotgun_gsaver_mpback_ratio, false);
				}
				if (flag2)
				{
					curMg.kill(-1f);
				}
				this.mp_overhold = global::XX.X.Mx(0f, global::XX.X.Mn((float)this.CurMg.reduce_mp, this.mp_overhold) - (float)reduce_mag);
				this.mp_hold = (float)((int)global::XX.X.Mx(0f, global::XX.X.Mn((float)this.CurMg.reduce_mp, this.mp_hold) - (float)reduce_mag));
				this.fineHoldMagicTime();
				this.OcSlots.reserve_consume_flag = false;
			}
			if (this.mp_hold < 1f)
			{
				this.mp_hold = (this.mp_overhold = 0f);
				Mg.phase = -1;
				Mg.reduce_mp = 0;
				this.addManaDrainLock(curMg, flag ? 0.1f : 0.5f, 30f);
				flag3 = true;
				this.FlgSoftFall.Rem("MAGIC");
				this.killHoldMagic(false);
				if (replace_normal)
				{
					MagicItem magicItem = this.executeSmallAttack(0, Mg);
					if (magicItem != null)
					{
						magicItem.Ray.SyncHitLock(Mg.Ray);
					}
				}
			}
			else
			{
				this.addManaDrainLock(curMg, 0.1f, 25f);
				MDAT.initShotGun(Mg, curMg, this.mp_hold, base.mp);
			}
			if (this.Pr.canProgressLayingEgg())
			{
				base.EggCon.need_fine_mp = true;
				base.EggCon.check_holded_mp += (float)reduce_mag;
			}
			return flag3;
		}

		public void initPublishKill(M2MagicCaster Target)
		{
			if (this.Pr.pee_lock > 0 && Target is NelEnemy)
			{
				this.Pr.PeeLockReduceCheck(0.66f);
			}
		}

		private void wheelBounce(bool check_double_wheel = true)
		{
			this.Pr.killSpeedForce(true, true, false);
			this.Phy.clearLockGravity();
			this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._FRIC_STRICT | FOCTYPE._RELEASE, -this.Pr.mpf_is_right * 0.11f, 0f, -1f, 0, 32, 0, 1, 0);
			this.Phy.walk_xspeed = -this.Pr.mpf_is_right * 0.11f;
			if (!base.hasD(M2MoverPr.DECL.INIT_A))
			{
				this.Phy.addFocFallingY(FOCTYPE.JUMP | FOCTYPE._GRAVITY_LOCK, -0.06f, 0.78f, 0);
				this.Phy.addLockGravityFrame(7);
			}
			if (!base.isNormalState())
			{
				this.Pr.changeState(PR.STATE.NORMAL);
			}
			base.SpSetPose("backstep", -1, null, false);
			base.PtcST("wheel_ground_bounce", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
		}

		private void executeBasicPunch(float _shift = 0f)
		{
			int num = 0;
			MagicItem magicItem = null;
			for (;;)
			{
				MagicItem magicItem2 = this.executeSmallAttack(num++, magicItem);
				if (magicItem2 == null)
				{
					break;
				}
				magicItem = magicItem2;
			}
			magicItem.defineParticlePreVariable();
			magicItem.PtcVar("mg", (double)((this.CurMg != null) ? (this.CurMg.chant_finished ? 2 : 1) : 0)).PtcVar("lax", (double)(this.mpf_is_right * (base.getEH(EnhancerManager.EH.long_reach) ? 2.4f : 1f))).PtcVar("ax", (double)this.mpf_is_right)
				.PtcVar("shift", (double)_shift)
				.PtcST("pr_cane_swing", PTCThread.StFollow.NO_FOLLOW, false);
			this.NM2D.MIST.attachWindDirectional((int)base.x, (int)(base.mtop + 0.2f), 2, 2, base.aim, 40, 1, 0);
			this.parry_t = 9f;
		}

		private MagicItem executeSmallAttack(int id = 0, MagicItem PreMagic = null)
		{
			int num = -50;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			PR.STATE state = base.state;
			MGKIND mgkind;
			switch (state)
			{
			case PR.STATE.BURST:
				mgkind = MGKIND.PR_BURST;
				goto IL_011B;
			case PR.STATE.SLIDING:
				mgkind = MGKIND.PR_SLIDING;
				goto IL_011B;
			case PR.STATE.WHEEL:
			case PR.STATE.WHEEL_SHOTGUN:
				mgkind = MGKIND.PR_WHEEL;
				flag2 = base.state == PR.STATE.WHEEL_SHOTGUN;
				flag = true;
				goto IL_011B;
			case PR.STATE.COMET:
			case PR.STATE.COMET_SHOTGUN:
				mgkind = MGKIND.PR_COMET;
				flag2 = true;
				flag = true;
				flag3 = true;
				goto IL_011B;
			case PR.STATE.DASHPUNCH:
			case PR.STATE.DASHPUNCH_SHOTGUN:
				mgkind = MGKIND.PR_DASHPUNCH;
				flag = true;
				flag3 = true;
				goto IL_011B;
			case PR.STATE.AIRPUNCH:
			case PR.STATE.AIRPUNCH_SHOTGUN:
			case (PR.STATE)32:
			case (PR.STATE)33:
			case (PR.STATE)34:
			case (PR.STATE)35:
			case (PR.STATE)36:
			case (PR.STATE)37:
			case (PR.STATE)38:
			case (PR.STATE)39:
				break;
			case PR.STATE.SHIELD_BUSH:
				mgkind = MGKIND.PR_SHIELD_BUSH;
				num = -100;
				goto IL_011B;
			case PR.STATE.SHIELD_LARIAT:
				mgkind = MGKIND.PR_SHIELD_LARIAT;
				num = 100;
				goto IL_011B;
			case PR.STATE.SHIELD_COUNTER:
			case PR.STATE.SHIELD_COUNTER_SHOTGUN:
				mgkind = MGKIND.PR_SHIELD_COUNTER;
				flag = true;
				goto IL_011B;
			default:
				if (state == PR.STATE.BURST_SCAPECAT)
				{
					mgkind = MGKIND.PR_BURST;
					goto IL_011B;
				}
				break;
			}
			if (id >= 2 || (id == 1 && !base.getEH(EnhancerManager.EH.long_reach)))
			{
				return null;
			}
			mgkind = ((this.CurMg == null) ? MGKIND.PR_PUNCH : MGKIND.PR_SHOTGUN);
			flag = true;
			IL_011B:
			MagicItem magicItem = (this.CurSkill = this.NM2D.MGC.setMagic(this.Pr, mgkind, (MGHIT)1025));
			if (flag && this.CurMg != null)
			{
				MDAT.initShotGun(magicItem, this.CurMg, this.mp_hold, (float)base.magic_returnable_mp);
				magicItem.Atk0.hit_ptcst_name = "";
				this.prepareMagicForCooking(magicItem, null, true);
			}
			else
			{
				this.prepareMagicForCooking(magicItem, null, false);
			}
			magicItem.run(0f);
			if (magicItem.Atk0 != null && mgkind == MGKIND.PR_PUNCH)
			{
				magicItem.Atk0.knockback_ratio_p = 1f;
			}
			if (magicItem.Ray != null)
			{
				magicItem.projectile_power = num;
				magicItem.Ray.hittype_to_week_projectile = HITTYPE.REFLECTED;
				this.NM2D.MGC.initRayCohitable(magicItem.Ray);
				if (flag3)
				{
					magicItem.Ray.check_other_hit = true;
				}
			}
			if (flag2 && magicItem.Atk0 != null)
			{
				magicItem.Atk0.attack_max0 = -1;
			}
			state = base.state;
			switch (state)
			{
			case PR.STATE.PUNCH:
			case PR.STATE.AIRPUNCH:
			case PR.STATE.AIRPUNCH_SHOTGUN:
				magicItem.no_kill_effect_when_close = true;
				if (id == 1)
				{
					magicItem.sx *= 2.4f;
					magicItem.Ray.SyncHitLock(PreMagic.Ray);
				}
				if (base.state == PR.STATE.AIRPUNCH_SHOTGUN || base.state == PR.STATE.AIRPUNCH)
				{
					magicItem.sx += (float)global::XX.X.MPF(magicItem.sx > 0f) * 0.6f;
					magicItem.sz = 0.74f;
				}
				break;
			case (PR.STATE)21:
			case PR.STATE.SLIDING:
				break;
			case PR.STATE.BURST:
			{
				int magic_returnable_mp = base.magic_returnable_mp;
				int reduce_mp = magicItem.reduce_mp;
				magicItem.Atk0.tired_time_to_super_armor *= global::XX.X.ZPOW((float)magic_returnable_mp, (float)reduce_mp);
				this.Pr.applyBurstMpDamage(magicItem.reduce_mp);
				magicItem.reduce_mp = global::XX.X.Mn(magicItem.reduce_mp, magic_returnable_mp);
				float num2 = global::XX.X.NI(1f, 0.125f, global::XX.X.ZLINE((float)(reduce_mp - magicItem.reduce_mp), (float)reduce_mp));
				magicItem.Atk0.hpdmg0 = (int)((float)magicItem.Atk0.hpdmg0 * num2);
				magicItem.Atk0.mpdmg0 = (int)((float)magicItem.Atk0.mpdmg0 * num2);
				magicItem.Atk0.split_mpdmg = (int)((float)magicItem.Atk0.split_mpdmg * num2);
				base.EggCon.check_holded_mp += (float)magicItem.reduce_mp;
				break;
			}
			case PR.STATE.WHEEL:
			case PR.STATE.WHEEL_SHOTGUN:
				magicItem.PtcVar("ax", (double)this.Pr.mpf_is_right).PtcST((base.state == PR.STATE.WHEEL_SHOTGUN) ? "wheel_fly_shotgun" : "wheel_fly", PTCThread.StFollow.NO_FOLLOW, false);
				break;
			case PR.STATE.COMET:
			case PR.STATE.COMET_SHOTGUN:
				magicItem.PtcST((base.state == PR.STATE.COMET_SHOTGUN) ? "comet_fly_shotgun" : "comet_fly", PTCThread.StFollow.NO_FOLLOW, false);
				break;
			case PR.STATE.DASHPUNCH:
			case PR.STATE.DASHPUNCH_SHOTGUN:
				if (PreMagic == null)
				{
					this.Pr.PtcST((base.state == PR.STATE.DASHPUNCH_SHOTGUN) ? "dashpunch_shotgun" : "dashpunch", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				}
				break;
			default:
				if (state != PR.STATE.SHIELD_LARIAT)
				{
					if (state == PR.STATE.BURST_SCAPECAT)
					{
						MDAT.initBurstAtkForScapeCat(magicItem);
					}
				}
				else
				{
					magicItem.no_kill_effect_when_close = true;
					if (id == 1)
					{
						magicItem.sx *= -1f;
					}
					magicItem.Atk0.BurstDir((float)global::XX.X.MPF(magicItem.sx > 0f));
				}
				break;
			}
			return magicItem;
		}

		public bool canHoldMagic(MagicItem Mg)
		{
			return this.canHoldMagic(Mg.kind, Mg);
		}

		private bool canHoldMagic(MGKIND kind, MagicItem Mg = null)
		{
			bool flag = false;
			bool flag2 = true;
			if (kind <= MGKIND.PR_BURST)
			{
				if (kind == MGKIND.WHITEARROW)
				{
					return this.CurMg == Mg;
				}
				switch (kind)
				{
				case MGKIND.PR_PUNCH:
				case MGKIND.PR_SHOTGUN:
					flag = (base.state == PR.STATE.PUNCH || base.state == PR.STATE.AIRPUNCH || base.state == PR.STATE.AIRPUNCH_SHOTGUN) && base.hasD(M2MoverPr.DECL.INIT_A) && !base.hasD(M2MoverPr.DECL.FLAG2);
					break;
				case MGKIND.PR_SLIDING:
					flag = base.isSlidingState() && base.hasFoot();
					break;
				case MGKIND.PR_WHEEL:
					flag = (base.state == PR.STATE.WHEEL || base.state == PR.STATE.WHEEL_SHOTGUN) && !base.hasD(M2MoverPr.DECL.FLAG2);
					break;
				case MGKIND.PR_COMET:
					flag = (base.state == PR.STATE.COMET || base.state == PR.STATE.COMET_SHOTGUN) && !base.hasD(M2MoverPr.DECL.FLAG2);
					break;
				case MGKIND.PR_SHIELD_BUSH:
					if (base.state == PR.STATE.SHIELD_BUSH && base.hasD(M2MoverPr.DECL.FLAG0) && this.Pr.get_state_time() < 18f)
					{
						flag = true;
					}
					break;
				case MGKIND.PR_SHIELD_LARIAT:
					flag = this.Shield.isLariatState();
					break;
				case MGKIND.PR_DASHPUNCH:
					flag = (base.state == PR.STATE.DASHPUNCH || base.state == PR.STATE.DASHPUNCH_SHOTGUN) && !base.hasD(M2MoverPr.DECL.FLAG2);
					break;
				case MGKIND.PR_SHIELD_COUNTER:
					flag = (base.state == PR.STATE.SHIELD_COUNTER || base.state == PR.STATE.SHIELD_COUNTER_SHOTGUN) && !base.hasD(M2MoverPr.DECL.FLAG2);
					break;
				default:
					if (kind == MGKIND.PR_BURST)
					{
						flag = base.state == PR.STATE.BURST || base.state == PR.STATE.BURST_SCAPECAT;
					}
					break;
				}
			}
			else if (kind == MGKIND.ITEMBOMB_NORMAL || kind == MGKIND.ITEMBOMB_LIGHT || kind == MGKIND.ITEMBOMB_MAGIC)
			{
				flag2 = false;
				flag = this.CurSkill == Mg && base.state == PR.STATE.USE_BOMB && !base.hasD(M2MoverPr.DECL.FLAG1);
			}
			if (!flag && Mg == this.CurSkill && this.CurSkill != null)
			{
				if (flag2)
				{
					this.CurSkill.destruct();
				}
				this.CurSkill = null;
			}
			return flag;
		}

		public void reflectCheck(MagicItem Mg)
		{
			if (this.CurSkill == null || this.CurSkill.Ray == null || Mg.Ray == null)
			{
				return;
			}
			M2Ray.checkReflectCohitRay(this.CurSkill.Ray, Mg.Ray, false, false);
		}

		public bool isManipulatingMagic(MagicItem Mg)
		{
			if (base.state == PR.STATE.USE_BOMB)
			{
				return Mg == this.CurSkill && this.showMagicChantingTimeForEffect();
			}
			if (base.state != PR.STATE.MAG_EXPLODED && base.state != PR.STATE.MAG_EXPLODE_PREPARE)
			{
				if (this.CurMg != null && (base.isDamagingOrKo() || base.isSinkState() || this.Shield.canGuard() || base.isEvadeState()))
				{
					this.CurMg.casttime = Mg.t;
				}
				return false;
			}
			return this.CurMg != null;
		}

		public bool cursor_allow_input_aim()
		{
			if (base.state == PR.STATE.USE_BOMB)
			{
				return this.showMagicChantingTimeForEffect();
			}
			if (base.state == PR.STATE.MAG_EXPLODED || base.state == PR.STATE.MAG_EXPLODE_PREPARE)
			{
				return this.evade_t == 0f;
			}
			return base.isMagicExistState();
		}

		private M2MoverPr.PR_MNP initEvade(global::XX.AIM a, bool no_need_hold = false)
		{
			if (base.Ser.cannotEvade())
			{
				base.Ser.declineEvadeEffect();
				return (M2MoverPr.PR_MNP)0;
			}
			base.setAim(a, false);
			this.Pr.changeState((this.Pr.isDamagingOrKo() || this.Pr.isGaraakiState()) ? PR.STATE.UKEMI : PR.STATE.EVADE);
			if (no_need_hold && this.Pr.isEvadeState())
			{
				this.Pr.addD(M2MoverPr.DECL.INIT_A);
			}
			return M2MoverPr.PR_MNP.CHANGED_STATE;
		}

		public int evade_count_max
		{
			get
			{
				if (!base.getEH(EnhancerManager.EH.double_evade))
				{
					return 1;
				}
				return 2;
			}
		}

		private bool get_evade_active
		{
			get
			{
				return !this.Pr.hasD_stopevade() && !this.magic_preparing && (!this.magic_chanting || base.isEvadePD(30));
			}
		}

		public bool continue_crouch
		{
			get
			{
				return this.evade_t > 0f && this.Pr.is_crouch;
			}
		}

		private M2MoverPr.PR_MNP runEvadeCheck(float TS, bool alloc_execute_evadion = false, bool alloc_evade_initialize = false)
		{
			if (base.hasD(M2MoverPr.DECL.STOP_EVADE) || !base.is_alive)
			{
				alloc_execute_evadion = false;
			}
			if (this.evade_count != 0 && alloc_execute_evadion && base.hasFoot() && !base.isEvadeState())
			{
				this.evade_count = 0;
			}
			M2MoverPr.PR_MNP pr_MNP = (M2MoverPr.PR_MNP)0;
			bool flag = base.MistApply == null || base.MistApply.canOpenShield();
			alloc_execute_evadion = alloc_execute_evadion && TS > 0f && flag && !this.Pr.forceCrouch(false, false);
			bool flag2 = base.isEvadeO();
			if (base.hasD(M2MoverPr.DECL.FORCE_SHIELD_KEY_HOLD))
			{
				alloc_evade_initialize = (flag2 = true);
			}
			if (this.evadelock_on_jump_)
			{
				if (flag2)
				{
					alloc_execute_evadion = (flag2 = false);
				}
				else
				{
					this.evadelock_on_jump_ = false;
				}
			}
			if (flag2 && this.get_evade_active)
			{
				if (this.evade_t <= 0f)
				{
					if (!alloc_evade_initialize && !base.isEvadePD(4))
					{
						return (M2MoverPr.PR_MNP)0;
					}
					this.evade_t = 0f;
					this.stk_magic_t = 0f;
					IN.clearLROutRelease();
				}
				if ((this.magic_chanting_or_preparing || base.isMagicExistState()) && TS > 0f && this.evade_t < 10f)
				{
					this.initMagicSleep(true, alloc_execute_evadion);
				}
				bool flag3 = this.skill_on_guard && !base.hasD(M2MoverPr.DECL.STOP_SHIELD) && flag;
				pr_MNP |= (flag3 ? M2MoverPr.PR_MNP.NO_MOVE : ((M2MoverPr.PR_MNP)0)) & (M2MoverPr.PR_MNP)(-33);
				if (alloc_execute_evadion && this.skill_on_evade && this.evade_count < this.evade_count_max)
				{
					if (!this.Shield.canGuard() && this.evade_t <= 10f && base.isEvadePD(10))
					{
						if (base.isLO())
						{
							pr_MNP |= this.initEvade(global::XX.AIM.R, false);
						}
						else if (base.isRO())
						{
							pr_MNP |= this.initEvade(global::XX.AIM.L, false);
						}
					}
					else if (CFG.shield_hold_evadable)
					{
						if (this.Pr.isLU(15, true))
						{
							pr_MNP |= this.initEvade(global::XX.AIM.R, true);
						}
						else if (this.Pr.isRU(15, true))
						{
							pr_MNP |= this.initEvade(global::XX.AIM.L, true);
						}
					}
					if ((pr_MNP & M2MoverPr.PR_MNP.CHANGED_STATE) != (M2MoverPr.PR_MNP)0)
					{
						return pr_MNP | M2MoverPr.PR_MNP.NO_SINK;
					}
				}
				pr_MNP |= M2MoverPr.PR_MNP.NO_SINK;
				if (flag3)
				{
					this.evade_t = global::XX.X.Mx(0.125f, this.evade_t) + TS;
					if (this.Shield.isActive())
					{
						pr_MNP &= (M2MoverPr.PR_MNP)(-5);
					}
					if (TS > 0f)
					{
						this.Shield.activate(false);
						if (this.Shield.isVisibleCompletely())
						{
							if (this.Shield.canGuard() && base.getEH(EnhancerManager.EH.shield_cat))
							{
								this.FlgSoftFall.Add("SHIELD");
								this.Pr.jumpRaisingQuit(true);
							}
							else
							{
								this.FlgSoftFall.Rem("SHIELD");
							}
							if (!base.isNormalState() && !base.hasD(M2MoverPr.DECL.ALLOC_SHIELD_HOLD))
							{
								this.BombCancelCheck(null, false);
								this.Pr.changeState(PR.STATE.NORMAL);
								pr_MNP |= M2MoverPr.PR_MNP.CHANGED_STATE;
							}
						}
					}
				}
				else
				{
					this.shieldBlur(false, false);
					this.evade_t = 1f;
				}
				return pr_MNP;
			}
			if (this.evade_t > 0f)
			{
				this.evadeTimeBlur(false, false, false);
			}
			if (this.evade_t < 0f)
			{
				this.evade_t = global::XX.X.Mn(this.evade_t + TS, 0f);
				if (alloc_execute_evadion)
				{
					if (this.skill_on_evade && this.evade_count < this.evade_count_max)
					{
						if (base.isLU())
						{
							pr_MNP |= (this.get_evade_active ? this.initEvade(global::XX.AIM.R, true) : pr_MNP);
						}
						else if (base.isRU())
						{
							pr_MNP |= (this.get_evade_active ? this.initEvade(global::XX.AIM.L, true) : pr_MNP);
						}
					}
					if ((pr_MNP & M2MoverPr.PR_MNP.CHANGED_STATE) != (M2MoverPr.PR_MNP)0)
					{
						return pr_MNP | M2MoverPr.PR_MNP.NO_SINK;
					}
				}
				return M2MoverPr.PR_MNP.NO_SINK;
			}
			return (M2MoverPr.PR_MNP)0;
		}

		private bool fnShieldSwitchActivation(bool to_active)
		{
			if (!to_active)
			{
				this.FlgSoftFall.Rem("SHIELD");
			}
			return true;
		}

		public void moveStartPushedDown()
		{
			float num = this.evade_t;
		}

		public void evadeTimeBlur(bool no_stop_pr_move = false, bool setlock = false, bool shield_force_blur = false)
		{
			if (this.evade_t > 0f)
			{
				this.FlgSoftFall.Rem("SHIELD");
				if (setlock)
				{
					this.evadelock_on_jump_ = true;
					this.evade_t = 0f;
				}
				else if (this.evade_t <= 4f || no_stop_pr_move)
				{
					this.evade_t = -6f;
				}
				else
				{
					this.evade_t = -15f;
				}
			}
			else if (this.evade_t < 0f && setlock)
			{
				this.evadelock_on_jump_ = true;
			}
			if (!this.Pr.isShieldAttackState())
			{
				this.shieldBlur(shield_force_blur, !this.Shield.canGuard());
			}
		}

		private void shieldBlur(bool _force, bool immediate)
		{
			this.Shield.deactivate(_force, immediate);
		}

		public M2Shield.RESULT checkShield(NelAttackInfo Atk, float val)
		{
			return this.Shield.checkShield(Atk, val, false);
		}

		private bool magicProgressable(bool ignore_punch_t = false)
		{
			return !base.is_crouch && !this.Pr.hasD_stopmag() && this.Pr.Ser.ChantSpeedRate() > 0f && !base.isOnBench(true) && (base.MistApply == null || base.MistApply.canOpenShield()) && (ignore_punch_t || this.punch_t <= 0f) && !global::XX.X.BTWS(0f, this.evade_t, 10f) && (this.evade_t <= 0f || base.isMagicPD(30));
		}

		public bool magic_progressable
		{
			get
			{
				bool flag = false;
				if (global::XX.X.BTWS(0f, this.magic_t, (float)this.MAGIC_CHANT_DELAY) && CFG.magsel_decide_type != CFG.MAGSEL_TYPE.NORMAL && this.Pr.isAtkPD(1))
				{
					switch (CFG.magsel_decide_type)
					{
					case CFG.MAGSEL_TYPE.SUBMIT:
						flag = this.Pr.isSubmitPD(1);
						break;
					case CFG.MAGSEL_TYPE.Z:
						flag = this.Pr.isAtkPD(1);
						break;
					case CFG.MAGSEL_TYPE.C:
						flag = this.Pr.isTargettingPD(1);
						break;
					}
				}
				return this.magicProgressable(flag);
			}
		}

		private M2MoverPr.PR_MNP runMagicCheck(float TS)
		{
			bool magic_progressable = this.magic_progressable;
			if (!magic_progressable && this.CurMg != null)
			{
				this.initMagicSleep(true, false);
			}
			bool flag = false;
			bool flag2 = this.CurMg != null && this.magic_t > 0f;
			MagicSelector.MAGA maga = MagicSelector.MAGA._ALL;
			if (magic_progressable)
			{
				bool flag3 = true;
				if (this.Pr.isMagicStickO(0))
				{
					if (base.isPuzzleManagingMp() && this.temp_puzzle_mp == 0f)
					{
						if (this.Pr.get_temp_puzzle_max_mp() == 0f && this.Pr.isMagicStickPD(0))
						{
							UILog.Instance.AddAlertTX("Alert_no_magic_puzzle", UILogRow.TYPE.ALERT);
						}
					}
					else if (this.magic_t >= (float)this.MAGIC_CHANT_DELAY)
					{
						this.stk_magic_t = global::XX.X.Mx(this.stk_magic_t, (float)this.MAGIC_CHANT_DELAY);
						flag2 = true;
						flag = true;
						flag3 = false;
					}
					else
					{
						if (this.stk_magic_t < 0f)
						{
							this.stk_magic_t = 0f;
						}
						flag2 = true;
						flag3 = false;
						this.stk_magic_t += TS;
						UIStatus.showHold(40, false);
						if (this.stk_magic_t >= (float)this.MAGIC_CHANT_DELAY)
						{
							flag = true;
							if (this.Pr.isMagicNeutralO(0))
							{
								maga = MagicSelector.MAGA.NORMAL;
							}
							else if (this.Pr.isMagicRO(0))
							{
								maga = MagicSelector.MAGA.LR;
							}
							else if (this.Pr.isMagicLO(0))
							{
								maga = MagicSelector.MAGA.LR;
							}
							else if (this.Pr.isMagicTO(0))
							{
								maga = MagicSelector.MAGA.T;
							}
							else if (this.Pr.isMagicBO(0))
							{
								maga = MagicSelector.MAGA.B;
							}
						}
						if (this.Pr.isMagicRO(0))
						{
							this.Pr.setAim(global::XX.AIM.R, false);
						}
						else if (this.Pr.isMagicLO(0))
						{
							this.Pr.setAim(global::XX.AIM.L, false);
						}
					}
				}
				else if (this.CurMg != null)
				{
					flag3 = true;
					if (this.stk_magic_t > 0f)
					{
						this.stk_magic_t = (float)(base.isNormalState() ? (-15) : 0);
					}
					this.stk_magic_t = global::XX.X.VALWALK(this.stk_magic_t, 0f, TS);
					if (this.stk_magic_t < 0f)
					{
						flag = true;
					}
				}
				if (flag3)
				{
					flag = flag || (base.isMagicO() && (this.magic_t > 0f || base.isMagicPD(30)));
				}
			}
			if (magic_progressable && flag)
			{
				base.Mp.M2D.t_lock_check_push_up = 20f;
				if (!flag2 && this.CarryBox != null && this.CarryBox.runBoxPositionSet(TS, true, this.magic_t == 0f))
				{
					this.magic_t = 1f;
				}
				else if (base.isPuzzleManagingMp() && this.temp_puzzle_mp == 0f)
				{
					if (this.Pr.get_temp_puzzle_max_mp() == 0f && base.isMagicPD(1))
					{
						UILog.Instance.AddAlertTX("Alert_no_magic_puzzle", UILogRow.TYPE.ALERT);
					}
					this.magic_t = (float)((this.CurMg != null && this.CurMg.chant_finished) ? (-1) : 0);
				}
				else
				{
					if (this.CurMg == null && this.magic_t == 0f)
					{
						this.MagicSel.deactivate();
					}
					UIStatus.showHold(40, false);
					if (this.magic_t < 0f && this.CurMg != null && !this.CurMg.is_sleep)
					{
						this.magic_t = (float)this.MAGIC_CHANT_DELAY;
					}
					else if (this.magic_t < (float)this.MAGIC_CHANT_DELAY && maga != MagicSelector.MAGA._ALL)
					{
						if (!this.MagicSel.fineCurrent(maga, this.CurMg, this))
						{
							this.magic_t = 0f;
							return (M2MoverPr.PR_MNP)0;
						}
						this.magic_t = (float)this.MAGIC_CHANT_DELAY;
					}
					else if (TS > 0f)
					{
						this.magic_t = global::XX.X.Mx(this.magic_t, 0f) + TS;
						if (this.CurMg == null && this.MagicSel.run(TS, this.CurMg == null && this.magic_t < (float)this.MAGIC_CHANT_DELAY, null))
						{
							this.magic_t = global::XX.X.Mn(this.magic_t, (float)this.MAGIC_CHANT_DELAY - 0.25f);
						}
					}
					else
					{
						this.magic_t = 1f;
					}
					if (this.evade_t > 0f && this.magic_t >= (float)this.MAGIC_CHANT_EVADE_KILL_DELAY)
					{
						this.evadeTimeBlur(true, true, false);
					}
					if (this.magic_t >= (float)this.MAGIC_CHANT_DELAY)
					{
						MGKIND mgkind = ((this.CurMg != null) ? this.CurMg.kind : MGKIND.NONE);
						if (mgkind == MGKIND.NONE)
						{
							mgkind = this.MagicSel.GetCurent();
						}
						if (mgkind == MGKIND.NONE)
						{
							this.magic_t = -1f;
						}
						else if (this.CurMg == null || this.CurMg.is_sleep)
						{
							this.magic_t = (float)this.MAGIC_CHANT_DELAY;
							if (TS > 0f && !this.reawakeMagic(mgkind))
							{
								this.magic_t = 0f;
								return (M2MoverPr.PR_MNP)0;
							}
						}
						base.GaugeBrk.secureSplitMpHoldTime(10f);
						if (this.CurMg != null && this.CurMg.Mn != null && this.CurMg.Mn._0.fnManipulateMagic != null && !this.CurMg.is_sleep)
						{
							this.CurMg.Mn._0.fnManipulateMagic(this.CurMg, this.Pr, TS);
						}
					}
				}
			}
			else if (this.magic_t > 0f)
			{
				if (this.CarryBox != null)
				{
					this.CarryBox.runBoxPositionSet(TS, false, false);
					this.punch_decline_time = 20;
					this.magic_t = 0f;
				}
				else if (this.magic_t >= (float)this.MAGIC_CHANT_DELAY)
				{
					this.magic_t = -15f;
				}
				else if (this.CurMg != null && this.CurMg.chant_finished)
				{
					this.magic_t = -1f;
				}
				else if (this.CurMg != null && this.CurMg.is_sleep && TS > 0f)
				{
					this.magic_t = -15f;
					this.reawakeMagic(this.CurMg.kind);
				}
				else
				{
					this.magic_t = 0f;
				}
				this.MagicSel.deactivate();
				if (magic_progressable && base.isMagicO() && (this.magic_t > 0f || base.isMagicPD(4)))
				{
					this.magic_t += 0f;
				}
			}
			if (this.magic_t != 0f && this.CurMg != null && this.CurMg.casttime > 0f && !this.CurMg.is_sleep)
			{
				float num = global::XX.X.Mn(this.CurMg.casttime, this.CurMg.t + TS) * (float)this.CurMg.reduce_mp / this.CurMg.casttime;
				if (num > this.mp_hold)
				{
					if (!base.isPuzzleManagingMp())
					{
						if (base.mp - num <= 0f)
						{
							this.mp_overhold = global::XX.X.Mx(this.mp_overhold, -(base.mp - num));
						}
						else
						{
							this.mp_overhold = 0f;
						}
						this.mp_hold = num;
						if (this.mp_hold >= (float)this.CurMg.reduce_mp && base.EggCon.isActive())
						{
							base.EggCon.need_fine_mp = true;
							base.EggCon.check_holded_mp += (float)this.CurMg.reduce_mp;
						}
					}
					else
					{
						this.mp_hold = num;
						this.mp_overhold = 0f;
					}
					this.mp_overhold = global::XX.X.Mn(this.mp_overhold, this.mp_hold);
				}
			}
			if (this.magic_t < 0f)
			{
				if (this.punch_t <= 0f && this.evade_t <= 0f)
				{
					if (this.CurMg != null && this.CurMg.chant_finished && TS > 0f)
					{
						if (this.CurMg.is_sleep)
						{
							float t = this.CurMg.t;
							MGKIND kind = this.CurMg.kind;
							this.CurMg.kill(-1f);
							MagicItem magicItem = this.NM2D.MGC.setMagic(this.Pr, kind, MGHIT.AUTO);
							this.OcSlots.replace(this.CurMg, magicItem);
							this.CurMg = magicItem;
							this.pre_chanted_magic_id = this.CurMg.id;
							this.CurMg.t = global::XX.X.Mx(this.CurMg.t, t);
							this.Cursor.initMagic(this.CurMg, true, true);
						}
						this.Pr.changeState(PR.STATE.MAG_EXPLODE_PREPARE);
						return M2MoverPr.PR_MNP.CHANGED_STATE;
					}
					this.magic_t = global::XX.X.Mn(0f, this.magic_t + TS);
				}
				else
				{
					this.magic_t = 0f;
				}
				if (this.magic_t == 0f)
				{
					if (this.CurMg != null)
					{
						this.initMagicSleep(true, false);
					}
					this.stk_magic_t = 0f;
					this.MagicSel.deactivate();
				}
				else
				{
					base.GaugeBrk.secureSplitMpHoldTime(4f);
				}
			}
			if (this.magic_t != 0f)
			{
				return (M2MoverPr.PR_MNP)63;
			}
			return (M2MoverPr.PR_MNP)0;
		}

		private bool reawakeMagic(MGKIND kind)
		{
			bool flag = true;
			if (this.CurMg != null && this.CurMg.is_sleep && base.TS > 0f)
			{
				this.CurMg.kill(-1f);
				flag = this.OcSlots.clearMagic(this.CurMg, false) > 0;
				this.CurMg = null;
			}
			this.MagicSel.fineLR();
			bool flag2 = SCN.canUseableMagic(kind);
			this.MagicSel.selectQuit(this.mp_hold == 0f, flag2);
			if (this.CurMg == null && !flag2)
			{
				this.MagicSel.DisableLog();
				return false;
			}
			this.FlgSoftFall.Add("MAGIC");
			this.CurMg = this.NM2D.MGC.setMagic(this.Pr, kind, MGHIT.AUTO);
			if (this.CurMg.reduce_mp > 0)
			{
				this.CurMg.reduce_mp = (int)global::XX.X.Mx(1f, (float)this.CurMg.reduce_mp * this.NM2D.pr_mp_consume_ratio);
			}
			this.pre_chanted_magic_id = this.CurMg.id;
			this.Cursor.initMagic(this.CurMg, this.mp_hold > 0f, false);
			if (flag && this.OcSlots.UseCheck(this.CurMg, 1))
			{
				this.mp_hold = (float)this.CurMg.reduce_mp;
			}
			if (this.mp_hold > 0f)
			{
				this.CurMg.t = this.CurMg.casttime * this.mp_hold / (float)this.CurMg.reduce_mp;
			}
			return true;
		}

		public M2MoverPr.PR_MNP initMagicSleep(bool secure_split_time = true, bool alloc_change_state = false)
		{
			this.FlgSoftFall.Rem("MAGIC");
			this.MagicSel.deactivate();
			this.Pr.target_calced = 2;
			if (base.state == PR.STATE.MAG_EXPLODED || this.CurMg == null || !this.CurMg.isPreparingCircle || this.CurMg.exploded)
			{
				this.killHoldMagic(false);
			}
			else if (this.CurMg != null && !this.CurMg.is_sleep)
			{
				if (this.mp_hold == 0f)
				{
					this.fineHoldMagicTime();
				}
				else
				{
					this.CurMg.Sleep();
				}
				if (secure_split_time && base.GaugeBrk.isActive())
				{
					base.GaugeBrk.secureSplitTime(60f);
				}
				this.magic_t = 0f;
			}
			if (alloc_change_state && base.state != PR.STATE.NORMAL && base.isMagicExistState())
			{
				this.Pr.changeState(PR.STATE.NORMAL);
				return M2MoverPr.PR_MNP.CHANGED_STATE;
			}
			return (M2MoverPr.PR_MNP)0;
		}

		private bool explodeMagic()
		{
			if (this.CurMg == null)
			{
				return false;
			}
			if (this.CurMg.killed)
			{
				this.killHoldMagic(false);
				return false;
			}
			int num = (int)this.mp_hold;
			int num2 = 0;
			if (base.isPuzzleManagingMp())
			{
				if (this.temp_puzzle_mp == 0f)
				{
					this.killHoldMagic(false);
					return false;
				}
				if (this.NM2D.Puz.puzz_magic_count_max == -1)
				{
					num = 0;
				}
			}
			else
			{
				num2 = this.OcSlots.clearMagic(this.CurMg, true);
			}
			this.Cursor.need_recalc_len = true;
			this.CurMg.reduce_mp = this.getReduceableMana(num, num2);
			if (num2 == 0)
			{
				this.Pr.applyMpDamage(num, true, null, false, false);
			}
			MagicItem magicItem = this.CurMg.explode(true);
			if (this.CurMg == null || magicItem == null)
			{
				return false;
			}
			this.Pr.PadVib("magic_explode", 1f);
			if (!magicItem.killed)
			{
				if (this.mp_overhold > 0f)
				{
					this.mp_overused += this.mp_overhold;
					this.mana_drain_lock_t = global::XX.X.NIL(50f, 120f, this.mp_overhold, this.mp_hold);
				}
				this.CurMg = this.prepareMagicForCooking(magicItem, this.CurMg, false);
				magicItem.TS = this.NM2D.NightCon.WindSpeed();
				magicItem.run(0f);
				this.NM2D.MGC.initRayCohitable(this.CurMg.Ray);
				this.pre_chanted_magic_id = this.CurMg.id;
				this.Cursor.initExplode(magicItem);
				this.addManaDrainLock(magicItem, 1f, 0f);
				MGContainer mgc = this.NM2D.MGC;
				int num3 = 0;
				if (this.CurMg.kind == MGKIND.DROPBOMB)
				{
					for (int i = mgc.Length - 1; i >= 0; i--)
					{
						MagicItem mg = mgc.getMg(i);
						if (mg != this.CurMg && mg.isActive(this.Pr, MGKIND.DROPBOMB) && ++num3 > 4 && mg.phase < 9)
						{
							mg.phase = 9;
						}
					}
				}
			}
			else
			{
				global::XX.X.de(string.Concat(new string[]
				{
					"初期化前にスペルが破壊されています！ type:",
					FEnum<MGKIND>.ToStr(magicItem.kind),
					" sx:",
					magicItem.sx.ToString(),
					" sy:",
					magicItem.sy.ToString()
				}), null);
			}
			this.mp_hold = (this.mp_overhold = 0f);
			return true;
		}

		public void ExplodeMagicMpToZero()
		{
			if (this.CurMg != null && !this.CurMg.isPreparingCircle)
			{
				this.CurMg.reduce_mp = 0;
			}
		}

		public int getReduceableMana(int reduce_mag, int oc_consumed)
		{
			if (base.isPuzzleManagingMp())
			{
				return 0;
			}
			if (oc_consumed != 0)
			{
				return oc_consumed;
			}
			return global::XX.X.Mn(reduce_mag, base.magic_returnable_mp);
		}

		public void addManaDrainLock(MagicItem Mg, float ratio = 1f, float add = 0f)
		{
			if (Mg != null)
			{
				MagicSelector.KindData kindData = MagicSelector.getKindData(Mg.kind);
				if (kindData != null)
				{
					float num = kindData.mana_drain_lock * ratio + add;
					if (this.Pr.enemy_targetted <= 0)
					{
						num = global::XX.X.Mn(60f, num);
					}
					this.mana_drain_lock_t_ = global::XX.X.Mn(global::XX.X.Mx(200f, this.mana_drain_lock_t_), this.mana_drain_lock_t_ + num * ((this.mana_drain_lock_t_ > num * 0.5f) ? 0.5f : 1f));
				}
			}
		}

		public void clearManaDrainLock()
		{
			if (this.mana_drain_lock_t_ > 0f)
			{
				this.mana_drain_lock_t_ = 0f;
				this.NM2D.Mana.fineRecheckTarget(2f);
			}
		}

		public float mana_drain_lock_t
		{
			get
			{
				return this.mana_drain_lock_t_;
			}
			set
			{
				this.mana_drain_lock_t_ = global::XX.X.Mx(this.mana_drain_lock_t_, value);
			}
		}

		public void changePoseMagicHold()
		{
			base.SpSetPose(this.FootD.FootIsLadder() ? "magic_hold_ladder" : "magic_hold", -1, null, false);
			Vector2 aimPos = this.Pr.getAimPos(this.CurMg);
			float num = base.Mp.GAR(base.x, base.y, aimPos.x, aimPos.y);
			this.magic_aim_agR = global::XX.X.MULWALKANGLER(this.magic_aim_agR, num, 1f);
			if (global::XX.CAim._XD(base.aim, 1) > 0)
			{
				if (this.magic_aim_agR > 0.6597344f)
				{
					this.Anm.setAim(global::XX.AIM.TR, -1, false);
					return;
				}
				if (this.magic_aim_agR < -0.3455752f)
				{
					this.Anm.setAim(global::XX.AIM.RB, -1, false);
					return;
				}
				this.Anm.setAim(global::XX.AIM.R, -1, false);
				return;
			}
			else
			{
				if (0f <= this.magic_aim_agR && this.magic_aim_agR < 2.4818583f)
				{
					this.Anm.setAim(global::XX.AIM.LT, -1, false);
					return;
				}
				if (0f > this.magic_aim_agR && this.magic_aim_agR > -2.7960176f)
				{
					this.Anm.setAim(global::XX.AIM.BL, -1, false);
					return;
				}
				this.Anm.setAim(global::XX.AIM.L, -1, false);
				return;
			}
		}

		public void killHoldMagic(bool split_hold_mp = false)
		{
			if (split_hold_mp && this.mp_hold > this.mp_overhold)
			{
				float num = this.mp_hold - this.mp_overhold;
				this.NM2D.Mana.AddMulti(base.x, base.y, num, (MANA_HIT)14);
				this.Pr.applyMpDamage((int)num, true, null, true, true);
			}
			this.mp_hold = (this.mp_overhold = 0f);
			if (this.magic_t > 0f && this.magic_t < (float)this.MAGIC_CHANT_DELAY)
			{
				this.MagicSel.deactivate();
			}
			this.magic_t = 0f;
			this.fineHoldMagicTime();
		}

		private bool fineHoldMagicTime()
		{
			if (this.CurMg == null || base.state == PR.STATE.MAG_EXPLODED)
			{
				return true;
			}
			this.mp_overhold = global::XX.X.Mn(this.mp_overhold, this.mp_hold);
			if (this.mp_hold <= 0f)
			{
				this.FlgSoftFall.Rem("MAGIC");
				this.CurMg.close();
				this.CurMg.kill(-1f);
				this.OcSlots.clearMagic(this.CurMg, false);
				this.CurMg = null;
				this.MagicSel.deactivate();
				return true;
			}
			this.CurMg.castedTimeResetTo(this.CurMg.casttime * this.mp_hold / (float)this.CurMg.reduce_mp);
			return false;
		}

		public bool magic_chanting
		{
			get
			{
				return base.state == PR.STATE.MAG_EXPLODE_PREPARE || ((this.magic_t < -1f || this.magic_t >= (float)this.MAGIC_CHANT_DELAY) && base.state != PR.STATE.MAG_EXPLODED);
			}
		}

		public bool magic_preparing
		{
			get
			{
				return base.isNormalState() && global::XX.X.BTW((float)this.MAGIC_CHANT_EVADE_KILL_DELAY, this.magic_t, (float)this.MAGIC_CHANT_DELAY);
			}
		}

		public bool magic_chanting_or_preparing
		{
			get
			{
				return base.state == PR.STATE.MAG_EXPLODE_PREPARE || ((this.magic_t < -1f || this.magic_t > 0f) && base.state != PR.STATE.MAG_EXPLODED);
			}
		}

		public bool magic_chant_completed
		{
			get
			{
				return base.state == PR.STATE.MAG_EXPLODE_PREPARE || (this.CurMg != null && this.CurMg.isPreparingCircle && this.CurMg.chant_finished);
			}
		}

		public bool magic_exploded
		{
			get
			{
				return this.CurMg != null && ((base.state == PR.STATE.MAG_EXPLODED && this.magic_t < 0f) || base.state == PR.STATE.MAG_EXPLODE_PREPARE);
			}
		}

		public void setMagicObtainFlag(MGKIND k, bool flag = true)
		{
			this.MagicSel.setObtainFlag(k, flag);
		}

		public bool hasMagic()
		{
			return this.CurMg != null;
		}

		public void cureMp(ref int val)
		{
			this.mp_overhold = global::XX.X.Mx(this.mp_overhold - (float)val, 0f);
		}

		public void reduceCrackMp(ref int val)
		{
			val = (int)global::XX.X.Mn((float)val, global::XX.X.Mx(0f, this.Pr.get_mp() - this.mp_hold));
		}

		public bool initPuzzleManagingMp(int set_to = -1)
		{
			this.killHoldMagic(false);
			this.Pr.tempPuzzleMpStringCacheClear();
			this.temp_puzzle_mp = ((set_to < 0) ? this.Pr.get_temp_puzzle_max_mp() : ((float)set_to));
			base.Ser.checkSer();
			if (UIStatus.Instance != null)
			{
				UIStatus.Instance.finePuzzleMagicManaging(this.Pr, true);
			}
			return true;
		}

		public void finePuzzleManagingMp()
		{
			try
			{
				if (this.Pr.isPuzzleManagingMp() && this.NM2D.Puz.puzz_magic_count_max == -1)
				{
					this.temp_puzzle_mp = this.Pr.get_temp_puzzle_max_mp();
					if (UIStatus.Instance != null)
					{
						UIStatus.Instance.fineMpRatio(false, false);
					}
				}
			}
			catch
			{
			}
		}

		public void quitPuzzleManagingMp()
		{
			this.killHoldMagic(false);
			this.Pr.tempPuzzleMpStringCacheClear();
			this.temp_puzzle_mp = 0f;
			base.Ser.checkSer();
			if (UIStatus.Instance != null)
			{
				UIStatus.Instance.finePuzzleMagicManaging(this.Pr, false);
			}
		}

		public float get_temp_puzzle_mp()
		{
			return this.temp_puzzle_mp;
		}

		public float get_temp_puzzle_max_mp()
		{
			if (!base.isPuzzleManagingMp())
			{
				return 0f;
			}
			if (this.NM2D.Puz.puzz_magic_count_max != -1)
			{
				return (float)this.NM2D.Puz.puzz_magic_count_max * 64f;
			}
			return base.maxmp;
		}

		public float getCastableMpRatioForUi()
		{
			if (!base.isPuzzleManagingMp())
			{
				return global::XX.X.ZLINE(base.getCastableMp(), base.maxmp);
			}
			if (this.temp_puzzle_mp != 0f)
			{
				return (this.temp_puzzle_mp - this.mp_hold) / this.get_temp_puzzle_max_mp();
			}
			return this.temp_puzzle_mp;
		}

		public float getMaxMpForUi()
		{
			if (!base.isPuzzleManagingMp())
			{
				return base.maxmp;
			}
			return this.get_temp_puzzle_max_mp();
		}

		public int applyMpDamagePuzzleMng(int val, bool force)
		{
			if (!base.isPuzzleManagingMp())
			{
				return 0;
			}
			if (this.NM2D.Puz.puzz_magic_count_max == -1)
			{
				return 0;
			}
			int num;
			if (!force && this.Pr.applyHpDamageRatio(null) == 0f)
			{
				num = 0;
			}
			else
			{
				num = (int)global::XX.X.Mn(this.temp_puzzle_mp, (float)val);
			}
			this.temp_puzzle_mp -= (float)num;
			this.Pr.tempPuzzleMpStringCacheClear();
			return val;
		}

		public bool showMagicChantingTimeForEffect()
		{
			PR.STATE state = base.state;
			if (state == PR.STATE.NORMAL)
			{
				return this.magic_t != 0f && this.CurMg != null && !this.CurMg.is_sleep;
			}
			if (state - PR.STATE.MAG_EXPLODE_PREPARE > 1)
			{
				return state == PR.STATE.USE_BOMB && base.hasD(M2MoverPr.DECL.FLAG0);
			}
			return this.CurMg != null;
		}

		public MagicItem getCurMagicForCursor()
		{
			if (base.state == PR.STATE.USE_BOMB && this.showMagicChantingTimeForEffect())
			{
				return this.CurSkill;
			}
			return this.CurMg;
		}

		public float splitMpByDamage(out float gauge_break, NelAttackInfoBase Atk, ref int reduce_val0, MANA_HIT split_mana_type, int counter_saf, float crack_ratio, bool use_quake = true)
		{
			float num = 0f;
			gauge_break = 0f;
			float num2 = this.mp_hold;
			if (this.CurMg == null || !this.CurMg.isPreparingCircle)
			{
				this.mp_hold = (this.mp_overhold = 0f);
			}
			else if (Atk == null || Atk.AttackFrom == null || Atk.AttackFrom is PR || !this.OcSlots.isActive(this.CurMg))
			{
				float num3 = this.mp_overhold;
				float num4 = global::XX.X.Scr(this.getRE(RecipeManager.RPI_EFFECT.LOST_MP_WHEN_CHANTING), DIFF.lostMpChanting_screen_value(this.Pr, Atk));
				float lost_mp_in_chanting_ratio = this.Pr.lost_mp_in_chanting_ratio;
				this.mp_overhold = global::XX.X.Mx(0f, global::XX.X.Mn((float)this.CurMg.reduce_mp, this.mp_overhold) - (float)(3 * reduce_val0) * lost_mp_in_chanting_ratio);
				float num5 = (num3 - this.mp_overhold) * global::XX.X.Mx(0f, global::XX.X.NI(3f, 0f, num4));
				num += num5;
				this.mp_hold = (float)((int)global::XX.X.Mx(0f, global::XX.X.Mn((float)this.CurMg.reduce_mp, this.mp_hold) - ((float)reduce_val0 + num5) * lost_mp_in_chanting_ratio));
				this.fineHoldMagicTime();
				reduce_val0 = (int)global::XX.X.Mn(global::XX.X.Mx((float)reduce_val0, num2 - this.mp_hold), num2);
			}
			float num6 = (float)this.Pr.applyMpDamage(out gauge_break, reduce_val0, true, Atk, use_quake, false, true);
			float num7 = num6;
			this.Pr.GSaver.applyMpDamage((float)reduce_val0, Atk, ref gauge_break, num2, use_quake);
			if (crack_ratio > 0f)
			{
				gauge_break += (num7 * global::XX.X.NI(1f, 0f, this.getRE(RecipeManager.RPI_EFFECT.MP_GAZE_RESIST)) + num) * crack_ratio;
			}
			if (split_mana_type != MANA_HIT.NOUSE && (Atk == null || Atk.attr != MGATTR.WORM))
			{
				(base.Mp.M2D as NelM2DBase).Mana.AddMulti(base.x, base.y, num6, split_mana_type);
			}
			return num6;
		}

		public bool isChantingMagicOnNormal(bool contain_selecting, bool contain_canceling)
		{
			return base.isNormalState() && ((contain_selecting ? (this.magic_t > 0f) : (this.magic_t >= (float)this.MAGIC_CHANT_DELAY)) || (contain_canceling && this.magic_t < 0f));
		}

		public bool isShieldOpeningOnNormal(bool contain_selecting, bool contain_canceling)
		{
			return base.isNormalState() && (((contain_selecting ? (this.evade_t > 0f) : (this.evade_t >= 10f)) && this.Shield.isHolding()) || (contain_canceling && this.evade_t < 0f));
		}

		public bool canUseAutoTargettingMagic()
		{
			MagicItem curMagicForCursor = this.getCurMagicForCursor();
			if (curMagicForCursor == null || curMagicForCursor.Mn == null || !curMagicForCursor.Mn._0.auto_target)
			{
				return false;
			}
			if (curMagicForCursor == this.CurSkill && base.state == PR.STATE.USE_BOMB)
			{
				return this.showMagicChantingTimeForEffect();
			}
			return (base.state == PR.STATE.NORMAL && this.magic_t != 0f && !curMagicForCursor.is_sleep) || base.state == PR.STATE.MAG_EXPLODE_PREPARE || base.state == PR.STATE.MAG_EXPLODED;
		}

		public NoelAnimator Anm
		{
			get
			{
				return this.Pr.getAnimator();
			}
		}

		public float mpf_is_right
		{
			get
			{
				return this.Pr.mpf_is_right;
			}
		}

		public MagicItem getCurMagic()
		{
			return this.CurMg;
		}

		public int getMagicManipulatorCursorAim(MagicItem CurMg)
		{
			if (CurMg == null)
			{
				return -1;
			}
			return this.Cursor.getMagicManipulatorCursorAim(CurMg);
		}

		public int getAimDirection()
		{
			MagicItem curMagicForCursor = this.getCurMagicForCursor();
			if (!this.isManipulatingMagic(curMagicForCursor))
			{
				return -2;
			}
			return this.Cursor.getAim();
		}

		public Vector2 getAimPos(MagicItem Mg)
		{
			if (Mg != null && !Mg.is_normal_attack)
			{
				return this.Cursor.getAimPos();
			}
			return new Vector2(base.x + (float)global::XX.CAim._XD(this.Pr.aim, 4), base.y);
		}

		public void getCameraCenterPos(ref float posx, ref float posy, float shiftx, float shifty)
		{
			if (this.showMagicChantingTimeForEffect())
			{
				Vector3 targetPosition = this.Cursor.getTargetPosition();
				float num = this.Cursor.getCameraShiftLevel() * 0.56f;
				if (targetPosition.z == 1f)
				{
					posx = global::XX.X.NI(posx, targetPosition.x * base.Mp.CLEN + shiftx, num);
					posy = global::XX.X.NI(posy, targetPosition.y * base.Mp.CLEN + shifty, num);
					return;
				}
				int num2 = this.Cursor.getAim();
				if (num2 == -1)
				{
					num2 = (int)base.aim;
				}
				posx = global::XX.X.NI(posx, base.drawx + (float)global::XX.CAim._XD(num2, 3 * (int)base.Mp.CLEN) + shiftx, num);
				posy = global::XX.X.NI(posy, base.drawy - (float)global::XX.CAim._YD(num2, 3 * (int)base.Mp.CLEN) + shifty, num);
			}
		}

		private M2MoverPr.PR_MNP runUseItemSelectCheck(float TS)
		{
			UseItemSelector usel = this.NM2D.IMNG.USel;
			bool flag = base.isNormalState() && this.magicProgressable(CFG.item_usel_type > CFG.USEL_TYPE.D_RELEASE) && this.magic_t == 0f;
			if (CFG.item_usel_type == CFG.USEL_TYPE.D_TAP_Z)
			{
				if (!usel.isSelecting())
				{
					if (this.Pr.isItmPD() && usel.run(flag, false))
					{
						return M2MoverPr.PR_MNP.NO_MOVE;
					}
				}
				else
				{
					this.Pr.isItmO(0);
					bool flag2 = this.Pr.isItmU(60);
					bool flag3 = this.Pr.isItmPD();
					bool flag4 = flag2 || flag3;
					if (usel.run(!flag4 && flag, false))
					{
						return M2MoverPr.PR_MNP.NO_MOVE;
					}
				}
			}
			else if (this.Pr.isItmO(0))
			{
				if (usel.run(flag, false))
				{
					return M2MoverPr.PR_MNP.NO_MOVE;
				}
			}
			else if (usel.isSelecting())
			{
				usel.run(false, flag);
			}
			return (M2MoverPr.PR_MNP)0;
		}

		public bool cannotAccessToCheckEvent()
		{
			return this.NM2D.IMNG.USel.isSelecting() || this.magic_t < 0f || this.magic_t > 1f;
		}

		private M2MoverPr.PR_MNP runFreezeGacha(float TS)
		{
			if (this.Pr.Ser.has(SER.FROZEN))
			{
				if (base.is_alive && ((base.isNormalState() && !this.isBusyTime(true, true, true, true)) || this.Pr.isAbsorbState() || this.Pr.isFrozenState() || this.Pr.isInputtableDownState()))
				{
					bool flag = false;
					if (this.Pr.isBP() || this.Pr.isLP(1) || this.Pr.isRP(1) || this.Pr.isTP(1) || this.Pr.isJumpPD(1))
					{
						if (this.freeze_lock_t == 0f)
						{
							flag = true;
						}
						else
						{
							this.freeze_lock_t = -global::XX.X.Abs(this.freeze_lock_t);
						}
					}
					if (this.freeze_lock_t != 0f)
					{
						bool flag2 = this.freeze_lock_t < 0f;
						this.freeze_lock_t = global::XX.X.VALWALK(this.freeze_lock_t, 0f, TS);
						if (this.freeze_lock_t == 0f && flag2)
						{
							flag = true;
						}
					}
					if (flag)
					{
						this.freeze_lock_t = 8f;
						this.Pr.TeCon.setQuake(1f, 7, 0f, 0);
						this.Pr.TeCon.setQuakeSinH(4f, 13, 5.54f, 1f, 0);
						this.Pr.PtcVar("cx", (double)(this.Pr.x + global::XX.X.XORSPS() * this.Pr.sizex * 0.7f)).PtcVar("cy", (double)(this.Pr.y + global::XX.X.XORSPS() * this.Pr.sizey * 0.7f)).PtcST("frozen_gacha", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						int level = this.Pr.Ser.getLevel(SER.FROZEN);
						this.Pr.Ser.CureTime(SER.FROZEN, (int)global::XX.X.NIL(120f, 180f, (float)level, 2f), false);
					}
				}
			}
			else if (this.freeze_lock_t != 0f)
			{
				this.freeze_lock_t = global::XX.X.Mx(global::XX.X.Abs(this.freeze_lock_t) - TS, 0f);
			}
			return (M2MoverPr.PR_MNP)0;
		}

		public void addCarryingBox(M2BoxMover Mv)
		{
			if (this.CarryBox != null)
			{
				this.CarryBox.deactivate(false);
				this.remCarryingBox(this.CarryBox);
			}
			if (Mv != null)
			{
				if (this.FootD.get_FootBCC() != null && Mv.getBCCCon() == this.FootD.get_FootBCC().BCC)
				{
					this.Pr.jump_hold_lock = true;
					this.Pr.initJump();
				}
				this.CarryBox = Mv;
				this.killHoldMagic(false);
				this.magic_t = 0f;
			}
		}

		public void remCarryingBox(M2BoxMover Mv)
		{
			if (this.CarryBox != null && Mv == this.CarryBox)
			{
				this.CarryBox = null;
			}
		}

		public M2BoxMover getCarryingBox()
		{
			return this.CarryBox;
		}

		public void initItemBomb(NelItem Itm, int grade, ItemStorage Str)
		{
			if (this.CurSkill != null)
			{
				MgItemBomb.MgBombMem mgBombMem = this.CurSkill.Other as MgItemBomb.MgBombMem;
				if (mgBombMem != null)
				{
					mgBombMem.item_back_to_storage = true;
				}
				this.CurSkill.kill(-1f);
			}
			if (base.state != PR.STATE.NORMAL)
			{
				this.Pr.changeState(PR.STATE.NORMAL);
			}
			this.Pr.changeState(PR.STATE.USE_BOMB);
			this.Pr.NM2D.IMNG.need_fine_bomb_self = true;
			this.CurSkill = M2PrSkill.CreateMagicForItemBomb(this.Pr, Itm, grade);
		}

		public static MagicItem CreateMagicForItemBomb(PR Pr, NelItem Itm, int grade)
		{
			MGContainer mgc = Pr.NM2D.MGC;
			string key = Itm.key;
			MGKIND mgkind;
			if (key != null)
			{
				if (key == "throw_magicbomb")
				{
					mgkind = MGKIND.ITEMBOMB_MAGIC;
					goto IL_004A;
				}
				if (key == "throw_lightbomb")
				{
					mgkind = MGKIND.ITEMBOMB_LIGHT;
					goto IL_004A;
				}
			}
			mgkind = MGKIND.ITEMBOMB_NORMAL;
			IL_004A:
			MagicItem magicItem = mgc.setMagic(Pr, mgkind, (MGHIT)1031);
			(magicItem.Other as MgItemBomb.MgBombMem).InitItem(Itm, grade);
			return magicItem;
		}

		public bool runItemBomb(ref float t)
		{
			if (base.hasD(M2MoverPr.DECL.FLAG1))
			{
				if (t >= 30f)
				{
					base.remD((M2MoverPr.DECL)263);
					if (this.Pr.isMoveLeftOn() || this.Pr.isMoveRightOn())
					{
						return false;
					}
				}
				return t < 48f;
			}
			if (this.CurSkill == null)
			{
				return false;
			}
			MagicItem curSkill = this.CurSkill;
			MgItemBomb.MgBombMem mgBombMem = this.CurSkill.Other as MgItemBomb.MgBombMem;
			if (mgBombMem == null || mgBombMem.Con.alreadyThrown(curSkill))
			{
				return false;
			}
			if (!base.hasD(M2MoverPr.DECL.INIT_A))
			{
				base.remD((M2MoverPr.DECL)10);
				base.addD(M2MoverPr.DECL.INIT_A);
				t = 0f;
				this.Pr.playSndPos("cloth_off", 1);
				this.Anm.setPose("item_gosogoso", -1, false);
			}
			else if (!base.hasD(M2MoverPr.DECL.FLAG0))
			{
				if (t >= 30f && mgBombMem.Con.initHoldPrepare(curSkill))
				{
					base.addD(M2MoverPr.DECL.FLAG0);
					this.Anm.setPose("throw", -1, false);
					t = 0f;
					curSkill.PtcST("itembomb_initialize", PTCThread.StFollow.NO_FOLLOW, false);
					this.Cursor.initMagic(curSkill, false, false);
				}
			}
			else
			{
				int aimDirection = curSkill.Caster.getAimDirection();
				bool flag;
				if (aimDirection >= 0)
				{
					int num = global::XX.CAim._XD(aimDirection, 1);
					flag = num != 0 && num != global::XX.CAim._XD(curSkill.Caster.getAimForCaster(), 1);
				}
				else
				{
					flag = global::XX.X.Abs(curSkill.Cen.x - curSkill.PosA.x) > 0.0625f && curSkill.Cen.x < curSkill.PosA.x != global::XX.CAim._XD(curSkill.Caster.getAimForCaster(), 1) > 0;
				}
				if (flag)
				{
					curSkill.Caster.setAimForCaster((curSkill.Cen.x < curSkill.PosA.x) ? global::XX.AIM.R : global::XX.AIM.L);
				}
				else
				{
					curSkill.sz = 1f;
				}
				if (!base.hasD(M2MoverPr.DECL.FLAG1))
				{
					bool flag2 = false;
					if (curSkill != null && curSkill.Mn != null && curSkill.Mn._0.fnManipulateMagic != null)
					{
						flag2 = curSkill.Mn._0.fnManipulateMagic(curSkill, this.Pr, base.TS);
					}
					if (!flag2 || (this.Pr.isCancelO(0) && !this.Pr.isMenuO(0)))
					{
						this.BombCancelCheck(mgBombMem, false);
						this.Pr.NM2D.IMNG.need_fine_bomb_self = true;
						return false;
					}
					if (t < 100f)
					{
						if (t >= 20f && this.Pr.isSubmitPD(10))
						{
							this.Anm.setPose("throw_2", -1, false);
							t = 100f;
						}
						else
						{
							t = global::XX.X.Mn(20f, t);
						}
					}
					else if (t >= 106f)
					{
						if (!this.Pr.isSubmitO(0) || !mgBombMem.Con.isHoldPrepare(curSkill))
						{
							t = 20f;
							if (this.Anm.poseIs("throw_2"))
							{
								this.Anm.setPose("throw", -1, false);
							}
							if (mgBombMem.Con.isHoldPrepare(curSkill))
							{
								this.CurSkill.t = 0f;
							}
						}
						else
						{
							base.addD(M2MoverPr.DECL.FLAG1);
							mgBombMem.Con.initThrow(curSkill, false, false);
							this.CurSkill = null;
							t = 0f;
						}
					}
				}
			}
			return true;
		}

		public bool isHoldingItem(NelItem Itm, int grade = -1)
		{
			if (this.CurSkill != null)
			{
				MgItemBomb.MgBombMem mgBombMem = this.CurSkill.Other as MgItemBomb.MgBombMem;
				if (mgBombMem != null)
				{
					return Itm == null || (Itm == mgBombMem.Data && (grade < 0 || mgBombMem.grade == grade));
				}
			}
			return false;
		}

		public bool BombCancelCheck(MgItemBomb.MgBombMem Mem = null, bool include_preparing = false)
		{
			if (Mem == null)
			{
				MagicItem magicItem = (include_preparing ? this.CurSkill : this.getCurMagicForCursor());
				if (magicItem == null || magicItem != this.CurSkill)
				{
					return false;
				}
				Mem = magicItem.Other as MgItemBomb.MgBombMem;
				if (Mem == null)
				{
					return false;
				}
			}
			Mem.item_back_to_storage = true;
			this.CurSkill.kill(-1f);
			this.CurSkill = null;
			return true;
		}

		private void pushSkillParams()
		{
			this.PreAMem = new M2PrSkill.SkillApplyMem(this.Pr);
		}

		private void popSkillParams(bool expand_hp = false, bool expand_mp = false)
		{
			if (!this.PreAMem.valid)
			{
				return;
			}
			this.PreAMem.Apply(this.Pr, expand_hp, expand_mp);
			this.PreAMem = default(M2PrSkill.SkillApplyMem);
		}

		public void resetSkillConnection(bool newgaming = false, bool expand_hp = false, bool expand_mp = false)
		{
			this.pushSkillParams();
			this.EnableSkillClear();
			this.BurstSel.fineBurstMagic();
			this.SerRegist.Clear();
			float num = 1f;
			float num2 = 1f;
			if (!newgaming)
			{
				foreach (KeyValuePair<RecipeManager.RPI_EFFECT, float> keyValuePair in this.NM2D.IMNG.StmNoel.getLevelDictionary01())
				{
					float num3 = (float)global::XX.X.IntR(keyValuePair.Value * 100f) / 100f;
					if (num3 >= 0f)
					{
						num3 *= this.Pr.Ser.getStomachApplyRatio();
					}
					RecipeManager.RPI_EFFECT key = keyValuePair.Key;
					if (key <= RecipeManager.RPI_EFFECT.FIRE_DAMAGE_REDUCE)
					{
						if (key != RecipeManager.RPI_EFFECT.MAXHP)
						{
							if (key != RecipeManager.RPI_EFFECT.MAXMP)
							{
								if (key == RecipeManager.RPI_EFFECT.FIRE_DAMAGE_REDUCE)
								{
									if (num3 > 0f)
									{
										this.SerRegist.Add(SER.BURNED, keyValuePair.Value);
									}
								}
							}
							else
							{
								this.PreAMem.mp_add = this.PreAMem.mp_add + global::XX.X.IntC((float)this.PreAMem.def_maxmp * num3);
								if (num3 < 0f)
								{
									num2 += num3;
									continue;
								}
								continue;
							}
						}
						else
						{
							this.PreAMem.hp_add = this.PreAMem.hp_add + global::XX.X.IntC((float)this.PreAMem.def_maxhp * num3);
							if (num3 < 0f)
							{
								num += num3;
								continue;
							}
							continue;
						}
					}
					else if (key != RecipeManager.RPI_EFFECT.ELEC_DAMAGE_REDUCE)
					{
						if (key != RecipeManager.RPI_EFFECT.FROZEN_DAMAGE_REDUCE)
						{
							if (key == RecipeManager.RPI_EFFECT.SLEEP_RESIST)
							{
								if (num3 > 0f)
								{
									this.SerRegist.Add(SER.SLEEP, keyValuePair.Value);
								}
							}
						}
						else if (num3 > 0f)
						{
							this.SerRegist.Add(SER.FROZEN, keyValuePair.Value);
						}
					}
					else if (num3 > 0f)
					{
						this.SerRegist.Add(SER.FROZEN, keyValuePair.Value);
					}
					this.addRELevel(keyValuePair.Key, num3);
				}
				int num4 = global::XX.X.beki_cntC(8193U);
				for (int i = 0; i < num4; i++)
				{
					EnhancerManager.EH eh = (EnhancerManager.EH)(1 << i);
					if (base.getEH(eh))
					{
						if (eh != EnhancerManager.EH.overspell)
						{
							if (eh != EnhancerManager.EH.anchor)
							{
								if (eh == EnhancerManager.EH.singletask)
								{
									this.addRELevel(RecipeManager.RPI_EFFECT.LOST_MP_WHEN_CHANTING, 0.92f);
								}
							}
							else
							{
								this.addRELevel(RecipeManager.RPI_EFFECT.SINK_REDUCE, 0.44f);
							}
						}
						else
						{
							this.addRELevel(RecipeManager.RPI_EFFECT.ATK_MAGIC_OVERSPELL, 0.66f);
						}
					}
				}
				int hp_add = this.PreAMem.hp_add;
				int mp_add = this.PreAMem.mp_add;
				foreach (KeyValuePair<string, PrSkill> keyValuePair2 in SkillManager.getSkillDictionary())
				{
					PrSkill value = keyValuePair2.Value;
					if (value.isUseable(this.Pr))
					{
						this.addActivePrSkill(value);
					}
				}
				int num5 = global::XX.X.Int((float)(this.PreAMem.hp_add - hp_add) * num);
				int num6 = global::XX.X.Int((float)(this.PreAMem.mp_add - mp_add) * num2);
				this.PreAMem.hp_add = hp_add + num5;
				this.PreAMem.mp_add = mp_add + num6;
				if (!newgaming && base.isOnBench(false))
				{
					expand_hp = true;
					if (this.NM2D.isSafeArea())
					{
						expand_mp = true;
					}
				}
				this.shield_fcnt = global::XX.X.NI(0.8f, 0.1f, this.getRE(RecipeManager.RPI_EFFECT.SHIELD_ENPOWER));
				this.Shield.shield_enpower = global::XX.X.NI(1f, 0.25f, this.getRE(RecipeManager.RPI_EFFECT.SHIELD_ENPOWER));
				this.Pr.Ser.progress_speed = global::XX.X.Mx(0.0625f, global::XX.X.NI(1f, 2f, this.getRE(RecipeManager.RPI_EFFECT.SER_FAST)));
				this.pr_chant_speed = global::XX.X.NI(1f, 2f, this.getRE(RecipeManager.RPI_EFFECT.CHANT_SPEED));
				this.pr_mp_hunder_chant_speed = global::XX.X.NI(0.25f, 2.66f, this.getRE(RecipeManager.RPI_EFFECT.CHANT_SPEED_OVERHOLD));
				this.ser_apply_ratio = global::XX.X.NI(1f, 0.25f, this.getRE(RecipeManager.RPI_EFFECT.SER_RESIST));
				this.Pr.Ser.Regist = this.SerRegist;
			}
			this.popSkillParams(expand_hp, expand_mp);
		}

		public FOCTYPE foc_cliff_stopper
		{
			get
			{
				if (!base.getEH(EnhancerManager.EH.cliff_stopper))
				{
					return (FOCTYPE)0U;
				}
				return FOCTYPE._CLIFF_STOPPER;
			}
		}

		public FOCTYPE foc_damage_cliff_stopper
		{
			get
			{
				if (!(base.getEH(EnhancerManager.EH.cliff_stopper) | DIFF.damage_cliff_stop))
				{
					return (FOCTYPE)0U;
				}
				return FOCTYPE._CLIFF_STOPPER;
			}
		}

		private void addActivePrSkill(PrSkill K)
		{
			this.enable_skill_bits |= K.skill_type_bit;
			if ((K.category & SkillManager.SKILL_CTG.HP) != (SkillManager.SKILL_CTG)0 && REG.match(K.key, PrSkill.RegHpGain))
			{
				this.PreAMem.hp_add = this.PreAMem.hp_add + global::XX.X.NmI(REG.R1, 0, false, false);
			}
			if ((K.category & SkillManager.SKILL_CTG.MP) != (SkillManager.SKILL_CTG)0 && REG.match(K.key, PrSkill.RegMpGain))
			{
				this.PreAMem.mp_add = this.PreAMem.mp_add + global::XX.X.NmI(REG.R1, 0, false, false);
			}
		}

		public static void resetSkillConnectionWhole(bool newgaming = false, bool expand_hp = false, bool expand_mp = false)
		{
			Map2d curMap = M2DBase.Instance.curMap;
			int count_players = curMap.count_players;
			for (int i = 0; i < count_players; i++)
			{
				PR pr = curMap.getPr(i) as PR;
				if (pr != null)
				{
					pr.Skill.resetSkillConnection(newgaming, expand_hp, expand_mp);
				}
			}
		}

		public MagicItem prepareMagicForCooking(MagicItem Mg, MagicItem PreChantMagic, bool shotgun)
		{
			if (Mg == null)
			{
				return null;
			}
			float num = 1f;
			float num2 = 1f;
			if (PreChantMagic == null)
			{
				num += this.getRE(RecipeManager.RPI_EFFECT.ATK) * 0.75f;
				num2 += this.getRE(RecipeManager.RPI_EFFECT.PUNCH_DRAIN) * 1f;
			}
			if (PreChantMagic != null || shotgun)
			{
				float num3 = this.getRE(RecipeManager.RPI_EFFECT.ATK_MAGIC) * 1f;
				if (this.mp_overhold > 0f && this.mp_hold > 0f)
				{
					num3 = global::XX.X.NI(num3, 1.5f, this.getRE(RecipeManager.RPI_EFFECT.ATK_MAGIC_OVERSPELL) * (shotgun ? global::XX.X.ZPOW(this.mp_overhold, this.mp_hold) : global::XX.X.ZLINE(this.mp_overhold, this.mp_hold)));
				}
				num += num3;
				Mg.crystalize_neutral_ratio = global::XX.X.Scr(Mg.crystalize_neutral_ratio, global::XX.X.NI(0f, 0.875f, this.getRE(RecipeManager.RPI_EFFECT.MANA_NEUTRAL)));
			}
			this.AtkMul(Mg.Atk0, num, num2);
			this.AtkMul(Mg.Atk1, num, num2);
			this.AtkMul(Mg.Atk2, num, num2);
			return Mg;
		}

		private void AtkMul(NelAttackInfo Atk, float hpdmg, float mpdmg = 1f)
		{
			if (Atk == null)
			{
				return;
			}
			Atk.hpdmg0 = (int)((float)Atk.hpdmg0 * hpdmg);
			Atk.mpdmg0 = global::XX.X.IntC((float)Atk.mpdmg0 * mpdmg);
			Atk.split_mpdmg = global::XX.X.IntC((float)Atk.split_mpdmg * mpdmg);
		}

		public bool isCastingSpecificMagic(MGKIND k)
		{
			return this.CurMg != null && this.CurMg.kind == k;
		}

		public int getHoldingMp(bool return_real_val = false)
		{
			if (this.CurMg == null)
			{
				return 0;
			}
			int num = (int)this.mp_hold;
			if (!return_real_val)
			{
				num = this.OcSlots.currentHoldMpForUi(this.CurMg, num);
			}
			return num;
		}

		public int getOverHoldingMp(bool return_real_val = false)
		{
			if (this.CurMg == null)
			{
				return 0;
			}
			int num = (int)this.mp_overhold;
			if (!return_real_val)
			{
				int num2 = this.OcSlots.currentHoldMpForUi(this.CurMg, (int)this.mp_hold);
				num = global::XX.X.Mn(num, num2);
			}
			return num;
		}

		public void blurTargetting(IM2RayHitAble _Mv)
		{
			this.Cursor.blurTargetting(_Mv);
		}

		public int punch_decline_time
		{
			set
			{
				this.punch_decline_time_ = (byte)global::XX.X.Mx((int)this.punch_decline_time_, value);
			}
		}

		public bool punch_progressing
		{
			get
			{
				return this.punch_t > 0f;
			}
		}

		public bool evade_progressing
		{
			get
			{
				return this.evade_t > 0f;
			}
		}

		public bool hasInput(bool check_act = true, bool check_magic = true, bool check_evade = true)
		{
			return (check_act && this.punch_t > 0f) || (check_magic && this.magic_t > 0f) || (check_evade && this.evade_t > 0f);
		}

		public M2PrOverChargeSlot getOverChargeSlots()
		{
			return this.OcSlots;
		}

		public bool isOverChargeUseable()
		{
			return this.OcSlots.getUseableStock() > 0;
		}

		public bool isEnable(SkillManager.SKILL_TYPE type)
		{
			return (this.enable_skill_bits & (1UL << (int)type)) > 0UL;
		}

		public bool isObtained(SkillManager.SKILL_TYPE type)
		{
			return (this.enable_skill_bits & (1UL << (int)type)) > 0UL;
		}

		public bool canShildGuard()
		{
			return this.Shield.canGuard();
		}

		public M2PrSkill addRELevel(RecipeManager.RPI_EFFECT effect, float val)
		{
			this.Oeffect01[effect] = global::XX.X.Mn(1f, global::XX.X.Get<RecipeManager.RPI_EFFECT, float>(this.Oeffect01, effect, 0f) + val);
			return this;
		}

		public float getChantCompletedRatio()
		{
			if (this.CurMg != null && this.CurMg.casttime != 0f)
			{
				return this.CurMg.t / this.CurMg.casttime;
			}
			return -1f;
		}

		public bool isBusyTime(bool punch = true, bool evade = true, bool magic = true, bool usel = true)
		{
			return (punch && this.punch_t > 0f) || (evade && this.evade_t > 0f) || (magic && this.magic_t > 0f) || (usel && this.NM2D.IMNG.USel.isSelecting());
		}

		public float getRE(RecipeManager.RPI_EFFECT effect)
		{
			float num = global::XX.X.Get<RecipeManager.RPI_EFFECT, float>(this.Oeffect01, effect, 0f);
			if (!base.is_alive && num != 0f)
			{
				return global::XX.X.Mn(0f, num);
			}
			return num;
		}

		public bool isShotgunState()
		{
			return this.Pr.isShotgunState();
		}

		public M2PrSkill PadVib(string vib_key, float level = 1f)
		{
			this.Pr.PadVib(vib_key, level);
			return this;
		}

		public bool isShotgun(MagicItem Mg)
		{
			return Mg.is_normal_attack && (Mg.kind == MGKIND.PR_SHOTGUN || this.isShotgunState());
		}

		private const int PUNCH_ATK_DELAY = 9;

		private const int PUNCH_ATK_APPLY_TIME = 3;

		private const int PUNCH_PARRYABLE = 9;

		private const int PUNCH_RELEASE_DELAY = 15;

		private const int PUNCH_RELEASE_EVADE_DELAY = 9;

		private const int WHEEL_ATK_DELAY = 20;

		private const int WHEEL_ATK_DELAY_LONG = 35;

		private const int WHEEL_END_EVADE_AND_SHIELD_ENABLE = 30;

		private const int WHEEL_END_DELAY = 48;

		private const float WHEEL_VX = 0.34f;

		private const float WHEEL_VY = 0.2f;

		private const float WHEEL_VY_MAX = 0.41f;

		private const float WHEEL_VY_MAX_KEY_PO = 30f;

		public const float WHEEL_RADIUS = 1.9f;

		private const float WHEEL_HIT_BOUNCE_VX = 0.11f;

		private const float WHEEL_HIT_BOUNCE_VY = -0.06f;

		private const int COMET_ATK_DELAY = 20;

		private const int COMET_END_EVADE_AND_SHIELD_ENABLE = 14;

		private const int COMET_END_DELAY = 30;

		private const float COMET_VY = 0.34f;

		private const float COMET_BURST_VX = 0.03f;

		public const float COMET_RADIUS = 0.5f;

		private const float DASHPUNCH_INIT_T = 5f;

		private const float DASHPUNCH_START_RUN_CONTINUE_TIME = 22f;

		private const float DASHPUNCH_NODAMAGE_TIME = 10f;

		private const float DASHPUNCH_EVADE_ALLOC_TIME = 30f;

		private const float DASHPUNCH_END_TIME = 45f;

		private const float DASHPUNCH_PUNCH_REMAIN_TIME = 7f;

		public const float DASHPUNCH_RADIUS = 0.66f;

		public const float DASHPUNCH_SHOTGUN_RATIO = 1f;

		public const float AIRPUNCH_FIRST_VX = 0.15f;

		public const float AIRPUNCH_ATK_VX = -0.14f;

		public const float AIRPUNCH_AX = 0.006f;

		public const float AIRPUNCH_EXTEND_REACH = 0.6f;

		public const float AIRPUNCH_SOFTFALL_GRAVITY_SCALE = 0.23f;

		public const float AIRPUNCH_RADIUS = 0.74f;

		private const int AIRPUNCH_EXTEND_DELAY = 4;

		private const int AIRPUNCH_ATK_DELAY = 13;

		private const int AIRPUNCH_ATK_APPLY_TIME = 7;

		private const int AIRPUNCH_PARRYABLE = 13;

		private const int AIRPUNCH_RELEASE_DELAY = 19;

		private const int AIRPUNCH_RELEASE_EVADE_DELAY = 19;

		private const int MAGIC_CANCEL_DELAY = 15;

		private const int SLIDING_DELAY = 5;

		private const int SLIDING_MAXT = 33;

		private const int SLIDING_ALLOC_EVADE_T = 25;

		private const int SWAYSLD_ALLOC_EVADE_T = 12;

		private const float SLIDING_MOVE_DISTANCE = 3.2f;

		private const float SWAYSLD_MOVE_DISTANCE = 4.2f;

		private const float SWAYSLD_NOSINK_TIME = 100f;

		private const float SWAYSLD_RECHARGE_TIME = 180f;

		private const int BURST_PUSH_DELAY = 15;

		public const float BURST_HOLD_TIME = 5f;

		public const float BURST_SLOW_FAST = 0.16666667f;

		public const float BURST_SLOW_LATE = 0.071428575f;

		public const int BURST_PREPARE_TIME = 36;

		private const int BURST_AFTER_TIME = 45;

		public const float BURST_SLOW_RATIO = 0.25f;

		public const float BURST_APPLY_TIME = 8f;

		private const int EVADE_PUSHUP_DELAY = 15;

		private const int GUARD_PUSH_DELAY = 10;

		private const int GUARD_RELEASE_DELAY = 9;

		private const int EVADE_MOVE_MAXT = 13;

		private const int EVADE_SHOTGUN_MOVE_MAXT = 28;

		private const int EVADE_MAXT = 25;

		private const int EVADE_SHOTGUN_MAXT = 46;

		private const int EVADE_DMG_APPLY = 14;

		private const float evade_len = 2.6f;

		private const float evade_len_shotgun = 2f;

		private const float evade_len_air = 2.4f;

		private const float evade_move_lastv = 0.06f;

		public const int SHIELD_APPEARABLE_TIME = 260;

		public const int SHIELD_RECOVER_TIME = 360;

		public const float SHIELD_BREAK_BASE_RATIO = 2.5f;

		private const int GUARDBUSH_ATTACK_TIME = 18;

		private const int GUARDBUSH_DELAY_TIME = 70;

		private const int GUARDBUSH_SHIELD_DAMAGE = 90;

		public const int GUARDBUSH_EVADE_ENABLE = 23;

		public const float GUARDBUSH_RADIUS = 2.8f;

		public const float GUARDBUSH_SHIELD_RECOVER_LOCK = 90f;

		public const int GUARDLARIAT_SHIELD_DAMAGE = 40;

		private const int GUARDLARIAT_SHIELD_HIT_DAMAGE = 120;

		private const int GUARDLARIAT_INIT_T = 30;

		private const float GUARDLARIAT_FLIP_T = 17.5f;

		private const float GUARDLARIAT_END_T = 140f;

		private const float GUARDLARIAT_END_DELAY = 70f;

		public const float GUARDLARIAT_RADIUS = 1.6f;

		private const float GUARDLARIAT_EVADE_START = 45f;

		private const float BOMB_GOSOGOSO_T = 30f;

		private const float BOMB_CAN_THROW_T = 20f;

		private const float BOMB_THROW_AFTER_MOVE_MAXT = 30f;

		private const float BOMB_THROW_AFTER_MAXT = 48f;

		private float chanting_softfall_scale = 0.14f;

		private float chanting_softfall_scale_with_enhancer = 0.4f;

		private const byte chanting_softfall_maxt = 14;

		public const float chanting_softfall_max_xspeed = 0.016f;

		private float punch_t;

		private float evade_t;

		private M2Shield Shield;

		private M2PrOverChargeSlot OcSlots;

		private BDic<RecipeManager.RPI_EFFECT, float> Oeffect01;

		private int evade_count;

		private bool evadelock_on_jump_;

		public BurstSelector BurstSel;

		private float parry_t;

		private float magic_t;

		private float stk_magic_t;

		private float magic_aim_agR;

		private MagicItem CurMg;

		private MagicItem CurSkill;

		public readonly NelPlayerCursor Cursor;

		public MagicSelector MagicSel;

		private float mp_hold;

		private float mp_overhold;

		private float temp_puzzle_mp;

		private float mana_drain_lock_t_;

		public float mp_overused;

		public const float PUZZLE_PIECE_MP = 64f;

		private ulong enable_skill_bits;

		public float shield_fcnt = 1f;

		public float pr_mp_hunder_chant_speed;

		public float pr_chant_speed;

		public float ser_apply_ratio;

		public int MAGIC_CHANT_EVADE_KILL_DELAY = 7;

		public int MAGIC_CHANT_DELAY = 14;

		private int pre_chanted_magic_id = -1;

		private const float MP_OVERUSED_REDUCE_TS = 0.06666667f;

		private byte punch_decline_time_;

		private bool cyclone_attacked;

		private float freeze_lock_t;

		private float swaysld_t;

		private M2BoxMover CarryBox;

		public readonly Flagger FlgSoftFall;

		public readonly FlagCounter<SER> SerRegist;

		private const float burst_ptrfl = 128f;

		public const int alloc_punch_pd_frame = 26;

		private M2PrSkill.SkillApplyMem PreAMem;

		public struct SkillApplyMem
		{
			public SkillApplyMem(PR Pr)
			{
				this.hp = (int)Pr.get_hp();
				this.mp = (int)Pr.get_mp();
				this.maxhp = (int)Pr.get_maxhp();
				this.maxmp = (int)Pr.get_maxmp();
				this.hp_add = 0;
				this.mp_add = 0;
				this.def_maxhp = 150;
				this.def_maxmp = 200;
			}

			public bool valid
			{
				get
				{
					return this.def_maxhp > 0;
				}
			}

			public void Apply(PR Pr, bool expand_hp = false, bool expand_mp = false)
			{
				bool flag = false;
				bool is_alive = Pr.is_alive;
				int num = global::XX.X.Mx(this.hp_add + this.def_maxhp, 1);
				if (num != this.maxhp)
				{
					if (expand_hp && is_alive)
					{
						this.hp = global::XX.X.Mx(1, this.hp + num - this.maxhp);
					}
					else
					{
						this.hp = global::XX.X.IntR((float)(num * this.hp / this.maxhp));
					}
					this.maxhp = num;
					flag = true;
				}
				int cacled_dep_maxmp = this.cacled_dep_maxmp;
				if (cacled_dep_maxmp != this.maxmp)
				{
					if (expand_mp && is_alive)
					{
						this.mp = global::XX.X.Mx(1, this.mp + cacled_dep_maxmp - this.maxmp);
					}
					else
					{
						this.mp = global::XX.X.IntR((float)(cacled_dep_maxmp * this.mp / this.maxmp));
					}
					this.maxmp = cacled_dep_maxmp;
					flag = true;
				}
				if (flag)
				{
					this.hp = global::XX.X.MMX(is_alive ? 1 : 0, this.hp, this.maxhp);
					this.mp = global::XX.X.MMX(0, this.mp, this.maxmp);
					Pr.ApplySkillFixParameter(this);
				}
			}

			public int cacled_dep_maxmp
			{
				get
				{
					return global::XX.X.Mx(this.mp_add + this.def_maxmp, 15);
				}
			}

			public int hp;

			public int maxhp;

			public int mp;

			public int maxmp;

			public int hp_add;

			public int mp_add;

			public readonly int def_maxhp;

			public readonly int def_maxmp;
		}
	}
}
