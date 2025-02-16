using System;
using m2d;
using nel.gm;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class PRMain : PR
	{
		protected override void Awake()
		{
			base.Awake();
			this.RunnerBench = new M2PrABench(this);
			this.RunnerGameOverRecovery = new M2PrAGameOverRecovery(this);
		}

		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			this.RunnerBench.initS();
			this.RunnerGameOverRecovery.initS();
		}

		public Vector3 setToLoadGame(float x, float y)
		{
			BetobetoManager.immediate_load_material = 8;
			this.setTo(x, y);
			base.M2D.Cam.fineImmediately();
			NelChipBench nearBench = base.getNearBench(true, false);
			Vector3 zero = Vector3.zero;
			if (nearBench != null)
			{
				this.initBenchSitDown(nearBench, true, true);
				this.Anm.setPose("stand2bench", -1, false);
				zero = new Vector3(nearBench.mapcx, nearBench.mbottom - 0.125f, 1f);
			}
			else
			{
				base.changeState(PR.STATE.NORMAL);
			}
			UiBenchMenu.auto_start_temp_disable = true;
			this.Phy.recheckFoot(0f);
			return zero;
		}

		public override void newGame()
		{
			bool flag = this.SttInjector == null;
			base.newGame();
			if (flag)
			{
				this.initStateInjectorMain();
			}
		}

		protected override void changeState(PR.STATE state, PR.STATE prestate)
		{
			base.changeState(state, prestate);
			if (state <= PR.STATE.ONNIE)
			{
				if (state == PR.STATE.GAMEOVER_RECOVERY)
				{
					base.SpRunner = this.RunnerGameOverRecovery.activate();
					return;
				}
				if (state != PR.STATE.ONNIE)
				{
					return;
				}
				M2PrMasturbate m2PrMasturbate = new M2PrMasturbate(this, false);
				base.SpRunner = m2PrMasturbate;
				m2PrMasturbate.init(0, true);
				return;
			}
			else
			{
				if (state - PR.STATE.BENCH <= 1)
				{
					base.SpRunner = this.RunnerBench.activate();
					return;
				}
				if (state != PR.STATE.BENCH_ONNIE)
				{
					return;
				}
				M2PrMasturbate m2PrMasturbate2 = new M2PrMasturbate(this, true);
				base.SpRunner = m2PrMasturbate2;
				m2PrMasturbate2.init(0, true);
				return;
			}
		}

		protected void initStateInjectorMain()
		{
			this.SttInjector.Add(PR.STATE.ABSORB, 200, 0, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				return this.AbsorbCon.isActive() && this.state == PR.STATE.ABSORB;
			});
			this.SttInjector.Add(PR.STATE.WORM_TRAPPED, 195, 0, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				return base.isWormTrapped(this.state) && this.Ser.has(SER.WORM_TRAPPED);
			});
			this.SttInjector.Add(PR.STATE.EV_GACHA, 180, 0, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				return this.AbsorbCon.isActive() && this.state == PR.STATE.EV_GACHA;
			});
			this.SttInjector.Add(PR.STATE.WATER_CHOKED, 175, 0, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				if (base.forceCrouch(false, false) || base.isSleepingDownState(state) || state == PR.STATE.WATER_CHOKED_DOWN)
				{
					target_state = PR.STATE.WATER_CHOKED_DOWN;
				}
				return !this.DMG.is_stt_injection_locked && base.isWaterChokingDamageAlreadyApplied(false);
			});
			this.SttInjector.Add(PR.STATE.BURST, 170, 0, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				if (base.isBurstState(this.state) && state != PR.STATE.NORMAL)
				{
					execute = false;
					return true;
				}
				return false;
			});
			this.SttInjector.Add(PR.STATE.DAMAGE_BURNED, 160, 0, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				return this.Ser.has(SER.BURNED);
			});
			this.SttInjector.Add(PR.STATE.LAYING_EGG, 150, 0, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				if (!this.Ser.has(SER.LAYING_EGG) || this.isMoveScriptActive(false))
				{
					return false;
				}
				if (this.Ser.has(SER.EGGED) || base.isAnimationFrozen())
				{
					return false;
				}
				if (this.Ser.has(SER.DO_NOT_LAY_EGG) || base.secureTimeState())
				{
					return false;
				}
				if (this.EggCon.isLaying())
				{
					this.Ser.Cure(SER.SLEEP);
				}
				if (!base.changeLayingEggState(state, true) || this.DMG.is_stt_injection_locked)
				{
					execute = false;
				}
				return true;
			});
			this.SttInjector.Add(PR.STATE.FROZEN, 105, 1, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				if (base.isFrozen() || base.isStoneSer())
				{
					execute = (!base.isTrappedState(state) && !this.DMG.is_stt_injection_locked) & execute;
					return true;
				}
				return false;
			});
			this.SttInjector.Add(PR.STATE.ORGASM, 100, 1, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				if (this.EpCon.isOrgasm())
				{
					this.Ser.Cure(SER.SLEEP);
					if (base.changeLayingEggState(state, true) && !this.DMG.is_stt_injection_locked)
					{
						this.Skill.killHoldMagic(false, false);
					}
					else
					{
						execute = false;
					}
					return true;
				}
				return false;
			});
			this.SttInjector.Add(PR.STATE.SLEEP, 99, 1, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				if (this.Ser.isSleepDown())
				{
					if (state != PR.STATE.SLEEP && !base.isTrappedState(state) && !this.DMG.is_stt_injection_locked)
					{
						this.Skill.killHoldMagic(false, false);
					}
					else
					{
						execute = false;
					}
					return true;
				}
				return false;
			});
			this.SttInjector.Add(PR.STATE.DAMAGE_WEB_TRAPPED, 98, 2, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				if (!this.Rebagacha.ev_locked && this.Ser.has(SER.WEB_TRAPPED))
				{
					if (!base.isWebTrappedState(state) && !base.isTrappedState(state) && !this.DMG.is_stt_injection_locked)
					{
						target_state = (base.isPoseCrouch(false) ? PR.STATE.DAMAGE_WEB_TRAPPED : (base.isPoseDown(false) ? PR.STATE.DAMAGE_WEB_TRAPPED_LAND : PR.STATE.ENEMY_SINK));
					}
					else
					{
						execute = false;
					}
					return true;
				}
				return false;
			});
			this.SttInjector.Add(PR.STATE.DOWN_STUN, 30, 1, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				if (!this.is_alive)
				{
					if (!base.isDamagingOrKo(state) && !base.isSleepingDownState(state))
					{
						if (!base.isPoseDown(false))
						{
							this.SpSetPose(base.isPoseCrouch(false) ? "stun2down_2" : "stunned", -1, null, false);
							target_state = PR.STATE.DOWN_STUN;
						}
						else
						{
							if (!this.Anm.poseIs("downdamage_t", "downdamage"))
							{
								this.SpSetPose("stun2down_2", -1, null, false);
							}
							target_state = PR.STATE.DAMAGE_L_LAND;
						}
					}
					else
					{
						execute = false;
					}
					return true;
				}
				return false;
			});
			this.SttInjector.Add(PR.STATE.SHIELD_BREAK_STUN, 30, 1, delegate(PR.STATE state, ref float t_state, ref bool execute, ref PR.STATE target_state)
			{
				return this.Ser.has(SER.SHIELD_BREAK);
			});
		}

		public bool initBenchSitDown(NelChipBench Cp, bool is_load_after = false, bool force_to_state_reset = false)
		{
			return this.RunnerBench.initBenchSitDown(Cp, is_load_after, force_to_state_reset);
		}

		public void benchWaitInit(bool set_flag)
		{
			if (set_flag && this.state != PR.STATE.BENCH_SITDOWN_WAIT)
			{
				this.Skill.killHoldMagic(false, false);
				base.changeState(PR.STATE.BENCH_SITDOWN_WAIT);
				this.Phy.killSpeedForce(true, false, true, false, false);
				return;
			}
			if (!set_flag && this.state == PR.STATE.BENCH_SITDOWN_WAIT)
			{
				base.changeState(PR.STATE.NORMAL);
			}
		}

		public M2PrMasturbate initMasturbation(bool change_state = false, bool immediate = false)
		{
			if (immediate && !(base.SpRunner is M2PrMasturbate))
			{
				if (this.ep > 0)
				{
					base.cureFull(false, true, false, false);
					this.NM2D.FlagOpenGm.Rem("MASTURBATE");
					this.UP.setFade("masturbate_orgasm", UIPictureBase.EMSTATE.ORGASM, false, true, false);
					if (base.isBenchState())
					{
						this.Anm.setPose("bench_must_orgasm_2", -1, false);
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
			else if (change_state && !base.isMasturbateState(this.state) && !X.SENSITIVE)
			{
				base.recheck_emot = true;
				if (base.isBenchState())
				{
					base.changeState(PR.STATE.BENCH_ONNIE);
				}
				else if (base.isNormalState(this.state))
				{
					base.changeState(PR.STATE.ONNIE);
				}
			}
			return base.SpRunner as M2PrMasturbate;
		}

		internal M2PrABench RunnerBench;

		internal M2PrAGameOverRecovery RunnerGameOverRecovery;
	}
}
