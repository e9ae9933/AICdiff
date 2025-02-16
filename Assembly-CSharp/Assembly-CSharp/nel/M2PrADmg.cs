using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2PrADmg : M2PrAssistant
	{
		public float presscrouch_offset_pixel_y
		{
			get
			{
				return this.Pr.presscrouch_offset_pixel_y;
			}
		}

		public M2PrADmg(PR _Pr, RevCounterLock _RCenemy_sink, M2NoDamageManager _NoDamage)
			: base(_Pr)
		{
			this.PrM = this.Pr as PRMain;
			this.RCenemy_sink = _RCenemy_sink;
			this.NoDamage = _NoDamage;
		}

		public override void newGame()
		{
			this.hp_crack = 0;
		}

		public override void initS()
		{
			base.initS();
			this.absorb_additional_dying = 0f;
			this.check_lava = 0f;
			this.lava_burn_dount = 0;
			this.t_stt_inject_lock = 0f;
		}

		public void resetFlagsForGameOver()
		{
			base.Ser.checkSer();
			base.Ser.Cure(SER.DEATH);
			this.setHpCrack(X.Mn(this.hp_crack, 4));
		}

		public void changeState(PR.STATE state, PR.STATE prestate, bool n_dmg, bool pre_dmg, bool n_trapped_state)
		{
			if (n_trapped_state)
			{
				base.AbsorbCon.clear();
				base.Ser.Add(SER.WORM_TRAPPED, -1, 99, false);
				this.Skill.killHoldMagic(false, false);
			}
			if (pre_dmg)
			{
				if (!n_dmg)
				{
					this.Pr.resetWeight();
					this.RCenemy_sink.Lock(18f, true);
					if (this.Pr.UP != null)
					{
						this.Pr.UP.recheck(30, 0);
					}
					this.lava_burn_dount = 0;
					this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
					this.t_stt_inject_lock = 0f;
				}
				if (this.Pr.isPressDamageState(prestate))
				{
					this.Phy.remLockMoverHitting(HITLOCK.PRESSER);
					base.TeCon.removeSpecific(TEKIND.ABSORB_BOUNCY);
				}
			}
			if (pre_dmg != n_dmg)
			{
				base.NM2D.Cam.blurCenterIfFocusing(this.Pr);
				if (pre_dmg && !n_dmg)
				{
					base.Anm.FlgDropCane.Rem("DMG");
				}
			}
			if (n_dmg)
			{
				this.LockSttInjection(32f);
				if (base.Ser.has(SER.SHIELD_BREAK))
				{
					M2SerItem m2SerItem = base.Ser.Get(SER.SHIELD_BREAK);
					if (m2SerItem != null)
					{
						m2SerItem.progressTime(((float)m2SerItem.maxt - m2SerItem.getAf()) * 0.33f);
					}
				}
			}
			bool flag = this.Pr.isAbsorbState(state);
			bool flag2 = this.Pr.isAbsorbState(prestate);
			if (n_dmg || n_trapped_state || flag)
			{
				base.addD(M2MoverPr.DECL.STOP_FOOTSND);
			}
			if (this.Pr.Rebagacha != null && flag != flag2 && this.Pr.Rebagacha.isGobActive())
			{
				this.Pr.Rebagacha.need_reposit = true;
			}
			if (prestate == PR.STATE.ENEMY_SINK)
			{
				this.RCenemy_sink.Clear();
				this.RCenemy_sink.Lock(18f, true);
			}
			if (prestate == PR.STATE.BURST_SCAPECAT && base.NM2D.GameOver != null)
			{
				base.NM2D.GameOver.executeScapecatRespawnAfter();
			}
			if (this.Pr.isWebTrappedState(prestate))
			{
				base.PtcHld.killPtc("pr_web_trapped", false);
				if (base.isPunchState() || base.isMagicExistState())
				{
					this.Pr.UP.recheck_emot = true;
				}
			}
			this.Pr.need_check_bounds = true;
			if (state <= PR.STATE.DAMAGE_LT_KIRIMOMI)
			{
				if (state == PR.STATE.DAMAGE_L_HITWALL)
				{
					this.Pr.setBoundsToNormal(false);
					return;
				}
				if (state != PR.STATE.DAMAGE_LT_KIRIMOMI)
				{
					return;
				}
				this.Pr.setBoundsToDown(false);
				this.Pr.moveBy(0f, -0.65f, true);
				return;
			}
			else if (state != PR.STATE.DAMAGE_BURNED)
			{
				if (state - PR.STATE.DAMAGE_WEB_TRAPPED > 1)
				{
					return;
				}
				base.PtcVar("by", base.mbottom).PtcST("pr_web_trapped", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
				return;
			}
			else
			{
				if (this.Phy.isin_water)
				{
					this.t_stt_inject_lock = 0f;
					return;
				}
				this.LockSttInjection(80f);
				return;
			}
		}

		public void runPre()
		{
			if (this.check_lava > 0f && base.Mp.floort > 10f && base.Mp.BCC != null && base.Mp.BCC.is_prepared)
			{
				this.check_lava -= base.TS;
				if (this.check_lava <= 0f)
				{
					this.checkLavaExecute();
				}
			}
			if (this.t_stt_inject_lock > 0f)
			{
				this.t_stt_inject_lock = X.VALWALK(this.t_stt_inject_lock, 0f, base.TS);
			}
		}

		public void runDamaging(ref float t_state)
		{
			PR.STATE state = base.state;
			if (state <= PR.STATE.DAMAGE_PRESS_LR)
			{
				if (state <= PR.STATE.DAMAGE_LT)
				{
					if (state != PR.STATE.DAMAGE)
					{
						switch (state)
						{
						case PR.STATE.DAMAGE_L:
							if (t_state <= 0f)
							{
								this.NoDamage.Add(15f);
							}
							if (this.FootD.hasFootHard(0f))
							{
								if (t_state < 1000f)
								{
									t_state = X.Mn(100f, t_state);
								}
								if (base.poseIs("damage_thunder"))
								{
									if (t_state >= 15f)
									{
										base.SpSetPose("stunned", -1, null, false);
										this.Pr.changeState(PR.STATE.DAMAGE_L_LAND);
										base.PtcST("dmg_down", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
										return;
									}
									return;
								}
								else
								{
									if (base.poseIs("dmg_hktb") && X.XORSP() < 0.6f)
									{
										base.SpSetPose("dmg_down", -1, null, false);
										this.FootD.initJump(false, false, false);
										this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, 0f, -0.19f, -1f, -1, 1, 0, -1, 0);
										base.PtcST("dmg_bound", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
										base.remD(M2MoverPr.DECL.STOP_EVADE);
										return;
									}
									if (this.FootD.hasFootHard(3f) || t_state >= 15f)
									{
										base.SpSetPose(base.poseIs("dmg_hktb_b") ? "dmg_down_b_2" : "dmg_down2", -1, null, false);
										this.Pr.changeState(PR.STATE.DAMAGE_L_LAND);
										base.PtcST("dmg_down", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
										base.remD(M2MoverPr.DECL.STOP_EVADE);
										return;
									}
									return;
								}
							}
							else
							{
								if (t_state >= 1000f)
								{
									return;
								}
								if (t_state >= 120f)
								{
									base.addD(M2MoverPr.DECL.STOP_EVADE);
								}
								if (X.Abs(base.vx) > 0.02f)
								{
									t_state = X.Mn(100f, t_state);
								}
								if (t_state > 102f || this.Pr.canGoToSide((base.vx == 0f) ? (this.Pr.isPoseBack(false) ? base.aim : base.aim_behind) : ((base.vx > 0f) ? AIM.R : AIM.L), X.Mx(X.Abs(base.vx), 0.14f) + 0.02f, -0.1f, false, false, false))
								{
									return;
								}
								if (base.poseIs("dmg_hktb"))
								{
									base.SpSetPose("dmg_down_hitwall", -1, null, false);
									AIM aim = CAim.get_opposite(base.aim);
								}
								else
								{
									if (!base.poseIs("dmg_hktb_b"))
									{
										t_state = 1000f;
										return;
									}
									base.SpSetPose("dmg_down_b_hitwall", -1, null, false);
									AIM aim2 = base.aim;
								}
								this.Pr.playVo("arrest", false, false);
								this.Phy.killSpeedForce(true, true, true, false, false);
								this.Pr.changeState(PR.STATE.DAMAGE_L_HITWALL);
								this.NoDamage.Add(30f);
								this.Pr.PadVib("dmg_hit_wall", 1f);
								this.Phy.immidiateCheckStuck();
								base.Mp.M2D.Cam.Qu.SinH(18f, 40f, 3f, 0).Vib(9f, 18f, 2f, 0);
								this.Pr.playSndAbs("pr_hit_wall");
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
							if (this.FootD.hasFootHard((float)((t_state >= 10f) ? 0 : 3)) || t_state >= 100f)
							{
								t_state = 100f;
								this.Pr.playSndAbs("prko_hit_0");
								if (base.Anm.poseIs("stun2down", "stun2down_2", "down", "downdamage", "downdamage_t", null, null))
								{
									this.Pr.changeState(PR.STATE.DAMAGE_L_LAND);
									return;
								}
								if (base.poseIs("dmg_down_hitwall") || base.poseIs("stunned"))
								{
									base.SpSetPose((X.XORSP() < 0.3f) ? "stun2down" : "stunned", -1, null, false);
									this.Pr.changeState(PR.STATE.DAMAGE_L_LAND);
									return;
								}
								if (base.poseIs("dmg_down_b_hitwall") || base.poseIs("dmg_down_b_hitwall_2"))
								{
									base.SpSetPose((X.XORSP() < 0.3f) ? "stunned" : "dmg_down_b_hitwall_2", -1, null, false);
									this.Pr.changeState(PR.STATE.DAMAGE_L_LAND);
									return;
								}
								base.SpSetPose("stunned", -1, null, false);
								this.Pr.changeState(PR.STATE.DAMAGE_L_LAND);
								return;
							}
							else
							{
								if (t_state < 100f)
								{
									t_state = X.Mn(t_state, 50f);
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
							goto IL_0666;
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
						if (t_state >= 13f && base.is_alive)
						{
							base.remD((M2MoverPr.DECL)3);
							if (base.isEvadeO() || base.isMagicO())
							{
								this.Pr.changeState(PR.STATE.NORMAL);
								return;
							}
						}
						if (t_state >= (float)this.Pr.get_knockback_time() || (base.is_alive && t_state >= 15f && base.Ser.has(SER.WEB_TRAPPED)))
						{
							this.Pr.changeState(PR.STATE.NORMAL);
							return;
						}
						return;
					}
				}
				else if (state != PR.STATE.DAMAGE_LT_KIRIMOMI)
				{
					if (state == PR.STATE.DAMAGE_LT_LAND)
					{
						goto IL_0666;
					}
					if (state != PR.STATE.DAMAGE_PRESS_LR)
					{
						return;
					}
					if (t_state <= 0f)
					{
						t_state = 0f;
						if (!this.Pr.isUiPressDamage())
						{
							base.SpSetPose("dmg_press", -1, null, false);
						}
						this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
						this.Phy.addLockGravity(this, 0f, 120f);
					}
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						this.Phy.immidiateCheckStuck();
						this.Phy.killSpeedForce(true, true, true, false, false);
						this.Pr.offset_pixel_y = X.VALWALK(this.Pr.offset_pixel_y, this.presscrouch_offset_pixel_y, 0.55f * base.TS);
						if (!this.press_damage_state_skip(120f, ref t_state))
						{
							return;
						}
						if (this.Pr.isUiPressDamage())
						{
							base.addD(M2MoverPr.DECL.FLAG0);
							base.remD((M2MoverPr.DECL)3);
							t_state = 800f;
							base.TeCon.setEnlargeAbsorbed(1.45f, 1f, 50f, 10);
							base.SpSetPose("ui_press_damage_return", -1, null, false);
							return;
						}
						base.Mp.DropCon.setBlood(this.Pr, 72, MTR.col_blood, 0f, true);
						this.Pr.changeState(PR.STATE.DAMAGE_L_HITWALL);
						this.NoDamage.Add(30f);
						base.SpSetPose("stunned", -1, null, false);
						return;
					}
					else
					{
						if (t_state >= 920f || base.Anm.poseIs("stunned"))
						{
							this.Pr.changeState(PR.STATE.DAMAGE_L_HITWALL);
							this.NoDamage.Add(10f);
							base.remD((M2MoverPr.DECL)3);
							return;
						}
						return;
					}
				}
				PR.STATE state2 = this.runDamagingHktbLT(base.state, ref t_state);
				if (state2 != base.state)
				{
					base.Anm.rotationR = 0f;
					this.Pr.changeState(state2);
					base.remD(M2MoverPr.DECL.STOP_EVADE);
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
					if (t_state <= 0f)
					{
						t_state = 0f;
						if (!this.Pr.isUiPressDamage())
						{
							base.SpSetPose("dmg_press_t", -1, null, false);
						}
						this.FootD.initJump(false, false, false);
						this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
						this.Phy.killSpeedForce(true, true, true, false, false);
					}
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						this.Phy.immidiateCheckStuck();
						if (!this.press_damage_state_skip(70f, ref t_state))
						{
							return;
						}
						if (this.Pr.isUiPressDamage())
						{
							AIM aim3 = base.aim;
							base.addD(M2MoverPr.DECL.FLAG0);
							base.remD((M2MoverPr.DECL)3);
							t_state = 800f;
							base.SpSetPose("ui_press_damage_down_return", -1, null, false);
							base.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(aim3, 1), 1f, false), -1, false);
							base.TeCon.setEnlargeAbsorbed(1f, 1.65f, 50f, 10);
							return;
						}
						base.Mp.DropCon.setBlood(this.Pr, 72, MTR.col_blood, 0f, true);
						this.Pr.changeStatePressToLHitWall();
						return;
					}
					else
					{
						if (t_state >= 920f || base.Anm.poseIs("down"))
						{
							this.Pr.changeState(PR.STATE.DAMAGE_L_HITWALL);
							this.NoDamage.Add(10f);
							base.remD((M2MoverPr.DECL)3);
							return;
						}
						return;
					}
				}
			}
			else if (state <= PR.STATE.DAMAGE_WEB_TRAPPED_LAND)
			{
				if (state == PR.STATE.DAMAGE_BURNED)
				{
					this.runDamageBurned(ref t_state);
					return;
				}
				if (state - PR.STATE.DAMAGE_WEB_TRAPPED > 1)
				{
					return;
				}
				if (this.Pr.EpCon.isOrgasm())
				{
					base.addD(M2MoverPr.DECL.FLAG1);
					return;
				}
				Bench.P("Dmg_WebTrapped2");
				if (!base.hasD(M2MoverPr.DECL.INIT_A))
				{
					base.addD(M2MoverPr.DECL.INIT_A);
					this.Pr.Rebagacha.fineEnable();
					base.SpSetPose((base.state == PR.STATE.DAMAGE_WEB_TRAPPED_LAND) ? "down_web_trapped" : "sink_web_trapped", -1, null, false);
				}
				if (!base.Ser.has(SER.WEB_TRAPPED))
				{
					if (!base.hasD(M2MoverPr.DECL.FLAG0))
					{
						base.SpSetPose("wakeup", -1, null, false);
						base.addD(M2MoverPr.DECL.FLAG0);
						t_state = 100f;
						if (base.state == PR.STATE.DAMAGE_WEB_TRAPPED)
						{
							base.Anm.animReset(5);
						}
						base.remD(M2MoverPr.DECL.STOP_COMMAND);
					}
					else if (!base.poseIs("wakeup") || t_state >= 140f || this.Pr.isActionPD())
					{
						this.Pr.changeState(PR.STATE.NORMAL);
					}
				}
				else
				{
					if (base.hasD(M2MoverPr.DECL.FLAG1) && t_state >= 51f)
					{
						base.remD(M2MoverPr.DECL.FLAG1);
						t_state = 10f;
						this.Pr.UP.setFade((base.state == PR.STATE.DAMAGE_WEB_TRAPPED_LAND) ? "web_trapped_down_0" : "web_trapped_0", UIPictureBase.EMSTATE.NORMAL, true, true, false);
					}
					if (!base.hasD(M2MoverPr.DECL.FLAG1))
					{
						if (this.Pr.Rebagacha.tap_hitted)
						{
							base.addD(M2MoverPr.DECL.FLAG1);
							t_state = 10f;
							this.Pr.UP.setFade((base.state == PR.STATE.DAMAGE_WEB_TRAPPED_LAND) ? "web_trapped_down_1" : "web_trapped_1", UIPictureBase.EMSTATE.NORMAL, true, true, false);
						}
						else
						{
							this.Pr.UP.setFade((base.state == PR.STATE.DAMAGE_WEB_TRAPPED_LAND) ? "web_trapped_down_0" : "web_trapped_0", UIPictureBase.EMSTATE.NORMAL, false, false, false);
						}
					}
					if (base.hasD(M2MoverPr.DECL.FLAG0))
					{
						base.PtcVar("by", base.mbottom).PtcST("pr_web_trapped", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
						base.remD(M2MoverPr.DECL.FLAG0);
						base.addD(M2MoverPr.DECL.FLAG1);
						base.remD(M2MoverPr.DECL.INIT_A);
						base.addD(M2MoverPr.DECL.STOP_COMMAND);
					}
				}
				Bench.Pend("Dmg_WebTrapped2");
				return;
			}
			else
			{
				if (state == PR.STATE.ABSORB)
				{
					this.runAbsorbing(ref t_state);
					return;
				}
				if (state != PR.STATE.WORM_TRAPPED)
				{
					return;
				}
				Bench.P("Dmg_WormTrapped2");
				if (t_state <= 0f)
				{
					this.Pr.UP.applyDamage(MGATTR.NORMAL, (float)(-25 + X.xors(14)), (float)(1 + X.xors(5)), UIPictureBase.EMSTATE.NORMAL, false, null, false);
					t_state = (float)(170 - X.xors(10));
					base.VO.playAwkVo();
				}
				Bench.Pend("Dmg_WormTrapped2");
				Bench.P("Dmg_WormTrapped3");
				if (t_state >= 200f)
				{
					int num = (int)(t_state / 30f) % 2;
					if (num == 0 != base.hasD(M2MoverPr.DECL.FLAG0))
					{
						if (num == 0)
						{
							base.addD(M2MoverPr.DECL.FLAG0);
						}
						else
						{
							base.remD(M2MoverPr.DECL.FLAG0);
						}
						base.NM2D.Cam.Qu.Vib(3f, 5f, 2f, 0);
						base.Mp.PtcST("pr_worm_trapped", null, PTCThread.StFollow.NO_FOLLOW);
						t_state += (float)(4 + X.xors(14));
					}
				}
				Bench.Pend("Dmg_WormTrapped3");
				Bench.P("Dmg_WormTrapped4");
				if (base.Anm.isAnimEnd())
				{
					base.TeCon.setFadeOut(1f, 0f);
				}
				Bench.Pend("Dmg_WormTrapped4");
				return;
			}
			IL_0666:
			if (t_state <= 0f)
			{
				this.Pr.UP.recheck_emot = true;
			}
			bool flag = base.MistApply != null && base.MistApply.isWaterChokeDamageAlreadyApplied(true);
			bool flag2 = false;
			if (t_state >= 120f || flag)
			{
				flag2 = true;
				base.addD(M2MoverPr.DECL.STOP_EVADE);
			}
			if (base.Anm.pose_is_stand || base.Anm.next_pose_is_stand)
			{
				base.SpSetPose((X.XORSP() >= 0.25f == this.Pr.isPoseDown(false)) ? "down_b" : "down", -1, null, false);
			}
			bool flag3 = this.FootD.hasFootHard((float)((t_state >= 20f) ? 0 : 3));
			if (flag3 || t_state >= 100f)
			{
				if (t_state < 100f)
				{
					t_state = 100f;
					if (!flag2 && !base.hasD(M2MoverPr.DECL.INIT_A))
					{
						base.remD(M2MoverPr.DECL.STOP_EVADE);
					}
				}
				base.addD(M2MoverPr.DECL.INIT_A);
				if (t_state >= 130f && !base.hasD(M2MoverPr.DECL.FLAG2))
				{
					base.addD(M2MoverPr.DECL.FLAG2);
					this.Pr.SttInjector.need_check_on_runpre = true;
				}
				if (base.is_alive && !flag)
				{
					if (this.Pr.enemy_targetted == 0)
					{
						base.PunchDecline(20, false);
					}
					else if (t_state >= 120f)
					{
						base.remD(M2MoverPr.DECL.STOP_ACT);
					}
					if (flag3 && this.FootD.is_on_web && !base.Ser.has(SER.WEB_TRAPPED))
					{
						base.Ser.applySerDamage(EnemyAttr.SerDmgSlimy100, 1f, -1);
					}
					if (t_state >= 135f && (this.Pr.WakeUpInput(true, true) || base.isEvadeO()))
					{
						this.Pr.changeState(PR.STATE.NORMAL);
						return;
					}
				}
			}
			else if (t_state < 100f)
			{
				t_state = X.Mn(t_state, 50f);
				base.addD(M2MoverPr.DECL.STOP_ACT);
				return;
			}
		}

		private PR.STATE runDamagingHktbLT(PR.STATE state, ref float t_state)
		{
			if (t_state <= 0f)
			{
				this.NoDamage.Add(15f);
			}
			bool flag = false;
			if (state == PR.STATE.DAMAGE_LT_KIRIMOMI)
			{
				if (t_state <= 0f)
				{
					t_state = 0f;
					base.SpSetPose("dmg_t", -1, null, false);
					this.Phy.addLockGravity(this, 0.77f, 25f);
					this.FootD.initJump(false, false, false);
				}
				this.Pr.fine_frozen_replace = true;
				flag = true;
				base.Anm.rotationR = base.Mp.GAR(0f, 0f, base.vx, base.vy) - 1.5707964f;
				if (t_state >= 30f)
				{
					this.Phy.clampSpeed(FOCTYPE.HIT, 0.05f, -1f, 0.007f);
				}
			}
			if (base.canJump())
			{
				base.SpSetPose("dmg_down_t", -1, null, false);
				base.PtcST("dmg_down", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Pr.playVo("arrest", false, false);
				return PR.STATE.DAMAGE_LT_LAND;
			}
			if (base.vy >= -0.4f && t_state >= 25f && !flag)
			{
				base.SpSetPose("dmg_t_2", -1, null, false);
			}
			return base.state;
		}

		private bool press_damage_state_skip(float def_wait_t, ref float t_state)
		{
			if (base.NM2D.Iris.isWaiting(this.Pr, IrisOutManager.IRISOUT_TYPE.PRESS))
			{
				return false;
			}
			if (!base.hasD(M2MoverPr.DECL.INIT_A))
			{
				return t_state >= def_wait_t;
			}
			if (!base.is_alive)
			{
				if (t_state >= 220f)
				{
					t_state = 220f - X.NIXP(50f, 70f);
					base.TeCon.setQuake(2f, (int)X.NIXP(6f, 10f), 1f, 0);
				}
				return false;
			}
			if ((t_state >= def_wait_t && this.Pr.isActionO(0)) || t_state >= 220f)
			{
				base.remD((M2MoverPr.DECL)3);
				return true;
			}
			return false;
		}

		private void runAbsorbing(ref float t_state)
		{
			if (!base.AbsorbCon.runAbsorbPr(this.Pr, t_state, base.TS))
			{
				bool flag = (base.AbsorbCon.release_type & AbsorbManagerContainer.RELEASE_TYPE.GACHA) > AbsorbManagerContainer.RELEASE_TYPE.NORMAL;
				bool flag2 = base.is_alive && !base.Ser.hasBit(51052608UL);
				base.VO.breath_key = null;
				this.Pr.fine_frozen_replace = true;
				if (flag)
				{
					base.Ser.Cure(SER.SHIELD_BREAK);
				}
				if (this.Pr.isWebTrappedState(true))
				{
					this.Pr.endDrawAssist(1);
					t_state = 130f;
				}
				else
				{
					if (flag2 && flag)
					{
						this.Pr.endDrawAssist(1);
						this.Pr.changeState(PR.STATE.UKEMI_SHOTGUN);
						return;
					}
					if (!base.Ser.has(SER.BURNED) && (base.AbsorbCon.release_type & AbsorbManagerContainer.RELEASE_TYPE.KIRIMOMI) != AbsorbManagerContainer.RELEASE_TYPE.NORMAL)
					{
						this.Pr.endDrawAssist(1);
						this.Pr.isPoseBack(false);
						this.Pr.changeState(PR.STATE.DAMAGE_LT_KIRIMOMI);
						this.Phy.addLockMoverHitting(HITLOCK.ABSORB, 40f);
						this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, (float)CAim._XD(base.AbsorbCon.kirimomi_release_dir_last, 1) * 0.16f, X.NIXP(-0.12f, -0.21f), -1f, 0, 10, 90, 20, 0);
						this.FootD.initJump(false, true, false);
						return;
					}
					if (TX.isStart(base.Anm.pose_title, "torture", 0))
					{
						this.Pr.endDrawAssist(1);
						if (this.Pr.isPoseBackDown(false))
						{
							base.SpSetPose("dmg_hktb", -1, null, false);
							this.Pr.changeState(PR.STATE.DAMAGE_L);
							this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, -base.mpf_is_right * 0.08f, -0.02f, -1f, 0, 30, 90, 20, 0);
							return;
						}
						if (this.Pr.isPoseDown(false))
						{
							base.SpSetPose("dmg_hktb_b", -1, null, false);
							this.Pr.changeState(PR.STATE.DAMAGE_L);
							this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, -base.mpf_is_right * 0.08f, -0.02f, -1f, 0, 30, 90, 20, 0);
							return;
						}
						base.SpSetPose("dmg_down_hitwall", -1, null, false);
						this.Pr.changeState(PR.STATE.DAMAGE_L_HITWALL);
						this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, -base.mpf_is_right * 0.02f, 0f, -1f, 0, 30, 90, 20, 0);
						return;
					}
				}
				this.Pr.quitTortureAbsorb();
				if (t_state <= 120f)
				{
					t_state = 120f;
				}
				if (t_state >= 130f)
				{
					this.Pr.endDrawAssist(1);
					bool flag3 = !base.Anm.poseIs("down", "down_b", "down_u") && !base.Anm.nextPoseIs("down", "down_b", "down_u");
					if (this.Pr.isPoseDown(false))
					{
						if (flag3 && !this.Pr.poseIs(POSE_TYPE.ORGASM))
						{
							base.SpSetPose("down_u", -1, null, false);
						}
						this.Pr.changeState(PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER);
					}
					else if (!flag2 || base.getCastableMp() <= 0f)
					{
						if (flag3)
						{
							base.SpSetPose("stunned", -1, null, false);
						}
						this.Pr.changeState(PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER);
					}
					else
					{
						bool flag4 = this.Pr.isPoseCrouch(false) || base.is_crouch;
						this.Pr.changeState(PR.STATE.ENEMY_SINK);
						if (flag3)
						{
							base.Anm.setPose(flag4 ? "absorb_release" : "stand2absorb_release", -1, false);
						}
					}
					this.Pr.recheck_emot = true;
					if (base.is_alive)
					{
						this.NoDamage.Add(NDMG.DEFAULT, 55f);
						return;
					}
				}
			}
			else
			{
				t_state = X.Mn(t_state, 120f);
			}
		}

		public void runDamageBurned(ref float t_state)
		{
			if (t_state <= 0f)
			{
				this.Pr.playVo("dmgl", false, false);
				t_state = 0f;
				float num = this.Phy.calcFocVelocityX(FOCTYPE.DAMAGE, false);
				float num2 = this.Phy.calcFocVelocityX(FOCTYPE.WALK, false);
				this.Phy.walk_xspeed = ((X.Abs(num) > X.Abs(num2)) ? num : num2);
				base.SpSetPose("dmg_burned", -1, null, false);
				base.Anm.setAim((base.anm_mpf_is_right > 0f) ? AIM.RB : AIM.BL, -1, false);
			}
			bool flag = true;
			float num3 = this.Phy.walk_xspeed;
			bool flag2 = false;
			bool flag3 = base.is_alive || CFGSP.deadburned;
			if (this.FootD.hasFootHard(2f))
			{
				if (!flag3 || !base.Ser.has(SER.BURNED))
				{
					base.VO.breath_key = "breath_e";
					if (!base.Anm.poseIs("stand_norod2laying_egg", "laying_egg"))
					{
						base.SpSetPose((this.Pr.isPoseDown(false) || base.is_crouch) ? "laying_egg" : "stand_norod2laying_egg", -1, null, false);
						this.Pr.UP.recheck_emot = true;
					}
					flag2 = true;
					if (this.Pr.UP.getCurEmot() == UIEMOT.BURNED)
					{
						this.Pr.UP.recheck_emot = true;
					}
					base.addD(M2MoverPr.DECL.FLAG0);
					num3 = 0f;
					if (base.is_alive)
					{
						base.remD((M2MoverPr.DECL)3);
						if (this.Pr.isMovingPD())
						{
							this.Pr.changeState(PR.STATE.NORMAL);
							return;
						}
						flag = false;
					}
				}
				else
				{
					base.remD(M2MoverPr.DECL.FLAG0);
					if (t_state <= 2000f)
					{
						t_state = 2000f;
					}
					AIM aim = base.aim;
					if (!base.Anm.poseIs("dmg_burned_run"))
					{
						base.SpSetPose("dmg_burned_run", -1, null, false);
					}
					if (t_state >= 2015f)
					{
						if (base.isLP(1))
						{
							aim = AIM.L;
						}
						else if (base.isRP(1))
						{
							aim = AIM.R;
						}
						if (aim == AIM.R && this.Pr.wallHitted(AIM.R))
						{
							aim = AIM.L;
							num3 = -num3;
						}
						else if (aim == AIM.L && this.Pr.wallHitted(AIM.L))
						{
							aim = AIM.R;
							num3 = -num3;
						}
						if (aim != base.aim)
						{
							base.setAim(aim, false);
							t_state = 2000f;
						}
					}
					num3 = X.VALWALK(num3, base.mpf_is_right * 0.115f, 0.00575f);
				}
			}
			else
			{
				base.remD(M2MoverPr.DECL.FLAG0);
				if (t_state >= 2000f && X.Abs(num3) < 0.04f)
				{
					num3 = (float)X.MPF(num3 > 0f) * 0.04f;
				}
				if (!base.Anm.poseIs("dmg_burned"))
				{
					base.SpSetPose("dmg_burned", -1, null, false);
					base.Anm.setAim((base.mpf_is_right > 0f) ? AIM.R : AIM.L, -1, false);
				}
				if (t_state >= 1900f)
				{
					t_state = 1900f;
				}
				if (this.lava_burn_dount < 3)
				{
					if (base.isLO() && num3 > -0.085f)
					{
						num3 = X.VALWALK(num3, -0.085f, 0.006071429f);
					}
					else if (base.isRO() && num3 < 0.085f)
					{
						num3 = X.VALWALK(num3, 0.085f, 0.006071429f);
					}
					else if (X.Abs(num3) < 0.04f)
					{
						num3 = X.VALWALK(num3, (float)X.MPF(num3 > 0f) * 0.04f, 0.0085f);
					}
				}
			}
			this.Phy.walk_xspeed = num3;
			if (t_state <= 0f || (flag && !base.TeCon.existSpecific(TEKIND.DMG_BLINK)))
			{
				base.DMGE.setBurnedEffect(base.canJump() && num3 != 0f, t_state <= 0f || (!base.canJump() || flag3), flag2);
			}
		}

		public int applyHpDamageSimple(NelAttackInfoBase Atk, out bool force, int val = -1, bool show_damage_counter = true)
		{
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
				val = (num = MDAT.getPrDamageVal(val, Atk, this.Pr));
			}
			force = val > 0;
			M2DmgCounterItem.DC dc;
			int num2;
			bool flag2;
			val = this.Pr.GSaver.applyHpDamage(val, Atk, out dc, out num2, out flag2);
			val = this.Pr.applyHpDamage(val, true, Atk);
			this.Pr.GSaver.GsHp.Fine(true);
			if (val > 0)
			{
				base.NM2D.IMNG.CheckBombSelfExplode(this.Pr, Atk.attr, 1f);
			}
			if (force)
			{
				this.Pr.setDamageCounter(-num, 0, dc, Atk.CurrentAbsorbedBy);
				if (flag2)
				{
					this.NoDamage.Add(Atk.ndmg, (float)(this.Pr.isAbsorbState() ? X.Mx(Atk.nodamage_time - 6, 0) : Atk.nodamage_time));
				}
				UIStatus.Instance.fineHpRatio(true, true);
				if (!base.is_alive)
				{
				}
			}
			else
			{
				if (flag)
				{
					val = -1;
				}
				this.Pr.setDamageCounter(-num, 0, dc, Atk.CurrentAbsorbedBy);
			}
			base.Ser.checkSer();
			return val + num2;
		}

		public bool applyDamageAddition(NelAttackInfoBase Atk)
		{
			bool flag = false;
			if (Atk.SerDmg != null)
			{
				flag = base.Ser.applySerDamage(Atk.SerDmg, this.Pr.getSerApplyRatio(), -1) || flag;
			}
			if (Atk.EpDmg != null)
			{
				flag = this.Pr.EpCon.applyEpDamage(Atk.EpDmg, Atk.AttackFrom, EPCATEG_BITS._ALL, 1f, true) || flag;
			}
			return flag;
		}

		public int applyDamage(NelAttackInfo Atk, bool force, string fade_key = "", bool decline_ui_additional_effect = false, bool from_press_damage = false)
		{
			if (!from_press_damage)
			{
				if (this.Pr.Skill.isParryable(Atk))
				{
					if (Atk.PublishMagic != null && Atk.PublishMagic.is_normal_attack && Atk.Caster is NelEnemy)
					{
						(Atk.Caster as NelEnemy).applyParry(base.aim);
					}
					base.DMGE.effectParry(Atk);
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
					int num = this.Pr.applyPressDamage(Atk, force, (int)Atk.press_state_replace);
					if (num != -2000)
					{
						return num;
					}
				}
			}
			string text = fade_key;
			Atk.playEffect(this.Pr);
			int hpdmg = Atk._hpdmg;
			bool flag = true;
			bool flag2 = false;
			int num2;
			float num3;
			if (X.DEBUGWEAK)
			{
				num2 = 9999;
				num3 = 9999f;
			}
			else
			{
				if (Atk.isPenetrateAttr(this.Pr))
				{
					flag2 = true;
				}
				if (Atk.fix_damage && !this.NoDamage.isActive(Atk.ndmg))
				{
					num3 = (float)(this.can_applydamage_state() ? 1 : 0);
					num2 = (int)((float)hpdmg * num3);
				}
				else
				{
					num3 = this.applyHpDamageRatio(Atk);
					num2 = (int)((float)hpdmg * num3);
				}
			}
			if (hpdmg <= 0)
			{
				num2 = 0;
				if (Atk.ndmg == NDMG.GRAB_PENETRATE || (num3 > 0f && (Atk.EpDmg != null || Atk.SerDmg != null) && (Atk._mpdmg > 0 || (Atk.split_mpdmg > 0 && MDAT.canApplySplitMpDamage(Atk, this.Pr)))))
				{
					force = true;
					flag = false;
				}
				else
				{
					num3 = 0f;
				}
			}
			if (num3 == 0f && (!force || !this.can_applydamage_state()))
			{
				if (!flag2)
				{
					Atk._hitlock_ignore = true;
				}
				return 0;
			}
			if (Atk.shield_break_ratio != 0f)
			{
				M2Shield.RESULT result = this.Skill.checkShield(Atk, (Atk.shield_break_ratio < 0f) ? (-Atk.shield_break_ratio) : ((float)num2 * Atk.shield_break_ratio));
				if (this.applyShieldResult(result, Atk, true))
				{
					return 0;
				}
			}
			bool flag3 = (from_press_damage || Atk.isPenetrateAbsorb()) && base.AbsorbCon.isActive() && (!(Atk.Caster is NelM2Attacker) || !base.AbsorbCon.hasPublisher(Atk.Caster as NelM2Attacker));
			if (!from_press_damage)
			{
				float num4 = (this.Pr.canApplyAbrosb() ? ((this.Pr.isGaraakiState() || (base.isDamagingOrKo() && base.is_alive && base.getCastableMp() / base.maxmp >= 0.15f)) ? Atk.absorb_replace_prob_ondamage : Atk.absorb_replace_prob) : 0f);
				if (num4 > 0f && (this.Pr.isAbsorbState(base.state) || X.XORSP() < num4 + this.absorb_additional_dying) && this.Pr.initAbsorb(Atk, Atk.Caster as NelM2Attacker, null, flag3))
				{
					base.DMGE.effectAbsorbInit(Atk);
					return 0;
				}
			}
			base.Ser.checkDamageSpecial(ref num2, Atk);
			num2 = ((X.DEBUGNODAMAGE && X.DEBUGMIGHTY) ? 0 : MDAT.getPrDamageVal(num2, Atk, this.Pr));
			float num5 = 0f;
			float num6 = Atk.burst_vx;
			float num7 = Atk.burst_vy;
			bool is_alive = base.is_alive;
			bool flag4 = base.Ser.has(SER.SHIELD_BREAK);
			bool flag5;
			num2 = this.applyHpDamageSimple(Atk, out flag5, X.DEBUGNODAMAGE ? 0 : num2, flag);
			force = force || flag5;
			float num8 = 0f;
			float num9 = 0f;
			UIPictureBase.EMSTATE emstate = UIPictureBase.EMSTATE.NORMAL;
			bool flag6 = false;
			if (num2 > 0 || force)
			{
				flag6 = true;
				this.Skill.abortSwaySliding();
				bool flag7 = is_alive && !base.is_alive;
				if (X.XORSP() < Atk.huttobi_ratio + 0.3f * (1f - X.ZLINE(this.Pr.hp_ratio - 0.17f, 0.44f)))
				{
					if (num7 >= 0f && !this.Pr.isAnimationFrozen())
					{
						num7 -= 0.03f + 0.03f * X.XORSP();
					}
					num6 += (float)X.MPF(num6 > 0f) * (0.01f + 0.05f * X.XORSP());
				}
				if (base.is_crouch && !base.is_alive)
				{
					num7 -= 0.1f;
				}
				if (!base.is_alive && !is_alive && !base.AbsorbCon.no_shuffle_aim && X.XORSP() < Atk.aim_to_opposite_when_pr_dieing)
				{
					this.Pr.setAimOppositeManual();
				}
				bool flag8 = !flag4 && base.Ser.has(SER.SHIELD_BREAK);
				M2PrADmg.DMGRESULT dmgresult;
				if (base.isWormTrapped() && Atk.attr != MGATTR.WORM && X.XORSP() < 0.7f)
				{
					this.Pr.executeReleaseFromTrapByDamage(true, true);
					this.NoDamage.Add(45f);
					num6 = 0f;
					dmgresult = M2PrADmg.DMGRESULT.WORM;
				}
				else if (flag8 || Atk.huttobi_ratio >= 100f || (!this.Pr.isWebTrappedState(true) && num7 < Atk._burst_huttobi_thresh && Atk.huttobi_ratio > -100f))
				{
					dmgresult = M2PrADmg.DMGRESULT.L;
					if (!base.is_alive)
					{
						if (!is_alive)
						{
							num6 += X.NIXP(0.02f, 0.06f) * (float)X.MPF(num6 > 0f);
						}
						else
						{
							dmgresult |= M2PrADmg.DMGRESULT._TO_DEAD;
							num6 += X.NI(0.05f, 0.08f, X.ZLINE(X.XORSP() - 0.5f, 0.5f)) * (float)X.MPF(num6 > 0f);
						}
					}
					base.Ser.CureTime(SER.SLEEP, 80, true);
					if (flag3)
					{
						base.AbsorbCon.clear();
						dmgresult |= M2PrADmg.DMGRESULT._PENETRATE_ABSORB;
					}
					else if (!this.Pr.isAbsorbState(base.state))
					{
						this.Pr.quitCrouch(false, false, false);
						this.FootD.initJump(false, false, false);
						if (flag8 || (X.Abs(num6) < 0.15f && X.Abs(num7) > 0.08f))
						{
							num7 = X.Mx(-0.27f, num7 - 0.17f);
							dmgresult |= M2PrADmg.DMGRESULT._DMGT;
							this.Pr.changeState(PR.STATE.DAMAGE_LT);
						}
						else if (this.Pr.hp_ratio < 0.15f && X.Abs(num6) >= 0.15f && X.XORSP() < 0.25f)
						{
							this.Pr.changeState(PR.STATE.DAMAGE_LT_KIRIMOMI);
							this.FootD.initJump(false, false, false);
							num7 -= 0.012f;
							if (base.is_alive)
							{
								num6 += (float)X.MPF(num6 > 0f) * 0.02f;
							}
							dmgresult |= M2PrADmg.DMGRESULT._DMG_KIRIMOMI;
						}
						else
						{
							this.Pr.changeState(PR.STATE.DAMAGE_L);
						}
						if (!base.is_alive)
						{
							this.absorb_additional_dying += 0.02f;
						}
					}
					else
					{
						dmgresult |= M2PrADmg.DMGRESULT._ABSORBING;
						num7 = 0f;
					}
					if (!base.AbsorbCon.no_clamp_speed)
					{
						this.Phy.clampSpeed(FOCTYPE._RELEASE, 0.06f, 0.06f, 1f);
					}
					this.FootD.initJump(false, false, false);
					if (num6 != 0f || num7 != 0f)
					{
						if (X.Abs(num6) > 0.1f && X.XORSP() < 0.1f && !this.Pr.canGoToSide((num6 > 0f) ? AIM.R : AIM.L, 0.55f, -0.1f, false, false, false))
						{
							num6 *= -X.NIXP(0.4f, 0.7f);
						}
						this.Phy.remFoc(FOCTYPE.JUMP, true);
						this.Phy.addLockGravityFrame(1);
						this.Phy.addFoc(FOCTYPE.HIT | FOCTYPE._CHECK_WALL, X.absMn(num6 * 2.5f, 0.3f), num7 - 0.1f, -1f, -1, 1, 0, -1, 0);
						this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._CHECK_WALL, num6, num7, -1f, 0, 50, 90, 7, 8);
					}
				}
				else
				{
					dmgresult = M2PrADmg.DMGRESULT.S;
					bool flag9 = base.hasFoot();
					bool flag10 = flag9 || ((Atk._burst_huttobi_thresh < 0f || Atk.huttobi_ratio <= -100f) && num7 < 0f);
					if (flag3)
					{
						base.AbsorbCon.clear();
						if (!base.AbsorbCon.no_clamp_speed)
						{
							this.Phy.clampSpeed(FOCTYPE._RELEASE, 0.12f, 0.13f, 1f);
						}
						dmgresult |= M2PrADmg.DMGRESULT._PENETRATE_ABSORB;
					}
					else if (!this.Pr.isAbsorbState(base.state))
					{
						this.Phy.clampSpeed(FOCTYPE._RELEASE, 0.12f, 0.13f, 1f);
						this.Pr.changeState(PR.STATE.DAMAGE);
						if (!base.isWormTrapped() && base.Ser.has(SER.BURNED))
						{
							num7 = X.Mn(num7, -0.16f);
							flag10 = false;
						}
						else if (!base.canJump())
						{
							num7 -= ((num2 == 0) ? 0.02f : 0.2f);
						}
						if (!base.is_alive)
						{
							this.absorb_additional_dying += 0.02f;
						}
					}
					else
					{
						dmgresult |= M2PrADmg.DMGRESULT._ABSORBING;
						if (!base.AbsorbCon.no_clamp_speed)
						{
							this.Phy.clampSpeed(FOCTYPE._RELEASE, 0.06f, 0.06f, 1f);
						}
						num7 = 0f;
						num6 *= 0.5f;
					}
					if (num6 != 0f || num7 != 0f)
					{
						this.Phy.remFoc(FOCTYPE.JUMP, true);
						if (flag9)
						{
							this.Phy.addFoc(FOCTYPE.HIT | FOCTYPE._CHECK_WALL | this.Skill.foc_damage_cliff_stopper, DIFF.player_hit_first_velocity_x(num6), 0f, -1f, -1, 1, 0, -1, 0);
						}
						if (flag10)
						{
							this.Phy.addFoc(FOCTYPE.HIT | FOCTYPE._CHECK_WALL | ((num7 < 0f) ? FOCTYPE._GRAVITY_LOCK : ((FOCTYPE)0U)), 0f, num7, -1f, -1, 1, 0, -1, 0);
						}
						this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL | this.Skill.foc_damage_cliff_stopper, num6, 0f, -1f, 0, 2, 20, 5, 3);
					}
				}
				if (flag7)
				{
					dmgresult |= M2PrADmg.DMGRESULT._TO_DEAD;
				}
				base.DMGE.applyDamage(Atk, num2, dmgresult, ref emstate, out num8, out num9);
				string text2 = ((base.SfPose != null) ? base.SfPose.dmg_normal(Atk, num2, dmgresult, num6, ref fade_key, ref text) : "dmg_s");
				if (text2 != "" && !base.isWormTrapped())
				{
					bool flag11 = this.Pr.isPoseDown(false);
					base.SpSetPose(text2, 1, null, false);
					bool flag12 = this.Pr.isPoseDown(false);
					if (base.AbsorbCon.isActive() && flag11 != flag12)
					{
						this.Pr.changeState(PR.STATE.ABSORB);
					}
				}
				else
				{
					if (CAim._XD(base.Anm.pose_aim, 1) != CAim._XD(base.aim, 1))
					{
						base.Anm.setAim(base.aim, 0, false);
					}
					if (Atk.attr != MGATTR.WORM && (!base.AbsorbCon.isActive() || !base.AbsorbCon.no_shuffleframe_on_applydamage))
					{
						int num10 = base.Anm.getCurrentSequence().countFrames();
						if (num10 >= 6)
						{
							base.Anm.animReset(X.Mn(num10 - 1, (int)((float)num10 * X.NIXP(0.5f, 1.2f))));
						}
					}
				}
			}
			PTCThreadRunner.clearVars();
			if (flag6)
			{
				this.Pr.BetoMng.Check(this.Pr, Atk, false, true);
			}
			this.Pr.JuiceCon.applyDamage(Atk);
			bool flag13 = false;
			if (this.splitMpByDamage(out num5, Atk, Atk._mpdmg, MANA_HIT.EN | MANA_HIT.FROM_DAMAGE_SPLIT | MANA_HIT.FALL, 22, 1f, null, true) > 0f || num2 > 0 || fade_key != null)
			{
				flag13 = true;
			}
			base.DMGE.applyDamageAfter(Atk, flag13, fade_key, text, emstate, num8, num9, decline_ui_additional_effect);
			base.GaugeBrk.check(num5);
			return num2;
		}

		public float splitMpByDamage(NelAttackInfoBase Atk, int red_val = 0, MANA_HIT split_mana_type = MANA_HIT.EN, int counter_saf = 0, float crack_ratio = 1f, string fade_key = null, bool calc_gsaver = false)
		{
			float num2;
			float num = this.splitMpByDamage(out num2, Atk, red_val, split_mana_type, counter_saf, crack_ratio, fade_key, calc_gsaver);
			base.GaugeBrk.check(num2);
			return num;
		}

		public float splitMpByDamage(out float gauge_break, NelAttackInfoBase Atk, int red_val = 0, MANA_HIT split_mana_type = MANA_HIT.EN, int counter_saf = 0, float crack_ratio = 1f, string fade_key = null, bool calc_gsaver = false)
		{
			gauge_break = 0f;
			if (base.isPuzzleManagingMp() || X.DEBUGNODAMAGE)
			{
				return 0f;
			}
			int num = ((MDAT.canApplySplitMpDamage(Atk, this.Pr) && !X.DEBUGNODAMAGE) ? Atk.split_mpdmg : 0);
			bool flag = false;
			float num2 = 0f;
			if (num + red_val > 0)
			{
				float num3 = (float)(red_val + MDAT.getMpDamageValue(Atk, this.Pr, (float)num)) * base.NM2D.NightCon.SpilitMpRatioPr();
				float num4 = 1f;
				if (base.AbsorbCon.isActive())
				{
					float re = this.Pr.getRE(RCP.RPI_EFFECT.ARREST_MPDAMAGE_REDUCE);
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
				num2 = this.Pr.Skill.splitMpByDamage(out gauge_break, Atk, ref num5, split_mana_type, counter_saf, crack_ratio, true);
				this.Pr.setDamageCounter(0, -num5, M2DmgCounterItem.DC.NORMAL, Atk.CurrentAbsorbedBy);
				flag = flag || ((num2 > 0f || base.mp == 0f) && num > 0);
			}
			this.Pr.JuiceCon.splitMpByDamage(flag);
			return num2;
		}

		public int applyMySelfFire(MagicItem Mg, M2Ray Ray, NelAttackInfo Atk, M2MoverPr.PR_MNP manip)
		{
			int num = X.Mx((Atk._hpdmg == 0) ? 0 : 1, X.IntR((float)Atk._hpdmg * this.applyHpDamageRatio(Atk) / 4f));
			if (Mg.is_chanted_magic || Mg.kind == MGKIND.ITEMBOMB_MAGIC)
			{
				M2Shield.RESULT result = this.Skill.checkShield(Atk, (float)num);
				if (!this.applyShieldResult(result, Atk, false))
				{
					bool flag = false;
					if (Atk.SerDmg != null && base.Ser.applySerDamage(Atk.SerDmg, this.Pr.getSerApplyRatio(), -1))
					{
						flag = true;
					}
					if (num != 0 && ((manip & M2MoverPr.PR_MNP.NO_SINK) == (M2MoverPr.PR_MNP)0 || this.Pr.isMagicState(base.state)))
					{
						flag = true;
					}
					if (flag)
					{
						this.Pr.changeState(PR.STATE.ENEMY_SINK);
						int num2 = 3;
						int num3 = 30;
						if (this.Phy.is_on_ice)
						{
							num2 = 8;
							num3 = 0;
						}
						this.Phy.addFoc(FOCTYPE.DAMAGE | this.Skill.foc_cliff_stopper, (float)((Atk.hit_x > base.x) ? (-1) : 1) * 0.22f, 0f, -1f, 0, num2, num3, 32, 0);
					}
					base.NM2D.IMNG.CheckBombSelfExplode(this.Pr, MGATTR.ENERGY, 0.33f);
				}
				return -1;
			}
			return num;
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
					this.Pr.setDamageCounter(0, 0, M2DmgCounterItem.DC.GUARD, null);
				}
				return true;
			}
			if (result != M2Shield.RESULT.BROKEN || !flag)
			{
				return false;
			}
			if (base.Ser.has(SER.SHIELD_BREAK))
			{
				return true;
			}
			if (this.Skill.hasMagic())
			{
				this.Skill.killHoldMagic(true, false);
			}
			PostEffect.IT.setSlow(10f, 0f, 0);
			if (!base.AbsorbCon.isActive())
			{
				this.Pr.setDamageCounter(0, 0, M2DmgCounterItem.DC.NORMAL, null);
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._RELEASE, 0f, -0.2f, -1f, 0, 5, 0, -1, 0);
				this.Pr.quitCrouch(false, false, false);
				this.FootD.initJump(false, false, false);
				base.SpSetPose("dmg_t", 1, null, false);
				this.Pr.changeState(PR.STATE.DAMAGE_LT);
			}
			base.DMGE.effectShieldBreak(attr, hit_x, hit_y);
			return true;
		}

		public float applyHpDamageRatio(AttackInfo Atk)
		{
			if (this.Pr.isPressDamageState() && base.NM2D.Iris.isWaiting(this.Pr, IrisOutManager.IRISOUT_TYPE.PRESS))
			{
				if (Atk != null)
				{
					Atk._apply_knockback_current = false;
				}
				return 0f;
			}
			if ((Atk == null || !this.NoDamage.isActive(Atk.ndmg) || this.isPenetrateShutterAttr(Atk.ndmg)) && (base.is_alive || this.Pr.overkill) && this.can_applydamage_state())
			{
				float num = base.Ser.HpDamageRate();
				float num2 = 1f;
				if (Atk != null)
				{
					if (base.AbsorbCon.isActive())
					{
						float re = this.getRE(RCP.RPI_EFFECT.ARREST_HPDAMAGE_REDUCE);
						if (re >= 0f)
						{
							num2 = X.NI(1f, 0.25f, re);
						}
						else
						{
							num *= X.NI(1f, 2f, -re);
						}
					}
					MGATTR attr = Atk.attr;
					if (attr != MGATTR.BOMB)
					{
						switch (attr)
						{
						case MGATTR.FIRE:
							break;
						case MGATTR.ICE:
							this.calcAttributeDamageRatio(ref num2, this.getRE(RCP.RPI_EFFECT.FROZEN_DAMAGE_REDUCE));
							goto IL_0134;
						case MGATTR.THUNDER:
							this.calcAttributeDamageRatio(ref num2, this.getRE(RCP.RPI_EFFECT.ELEC_DAMAGE_REDUCE));
							goto IL_0134;
						default:
							goto IL_0134;
						}
					}
					this.calcAttributeDamageRatio(ref num2, this.getRE(RCP.RPI_EFFECT.FIRE_DAMAGE_REDUCE));
				}
				IL_0134:
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

		private void calcAttributeDamageRatio(ref float base_level, float re_level)
		{
			if (re_level >= 0f)
			{
				base_level = X.Mn(base_level, X.NI(1f, 0.25f, re_level));
				return;
			}
			base_level *= X.NI(1f, 2f, -re_level);
		}

		public bool FnHittingVelocity(M2Phys P, FOCTYPE type, ref float velocity_x, ref float velocity_y)
		{
			if (this.Pr.isAbsorbState() && !base.AbsorbCon.no_clamp_speed)
			{
				velocity_x *= 1f / (1f + base.AbsorbCon.total_weight);
				velocity_y *= 1f / (1f + base.AbsorbCon.total_weight * 1.5f);
			}
			return true;
		}

		public float lost_mp_in_chanting_ratio
		{
			get
			{
				MagicItem curMagic = this.Pr.Skill.getCurMagic();
				if (curMagic == null || !curMagic.isPreparingCircle)
				{
					return 1f;
				}
				return X.Mx(0f, X.NI(1f, 0f, this.Pr.getRE(RCP.RPI_EFFECT.LOST_MP_WHEN_CHANTING) * X.ZLINE((float)this.Pr.Skill.getHoldingMp(true), curMagic.reduce_mp * 0.3f)));
			}
		}

		public void initAbsorb()
		{
			this.absorb_additional_dying = 0f;
		}

		public void applyWormTrapDamage(NelAttackInfo Atk, int phase_count, bool decline_additional_effect)
		{
			if (phase_count == 0)
			{
				this.Pr.Skill.killHoldMagic(false, false);
			}
			bool flag = false;
			bool flag2 = true;
			if (Atk != null)
			{
				flag = true;
				if (Atk._hpdmg > 0)
				{
					this.applyDamage(Atk, true, "", false, false);
					flag2 = false;
				}
				else
				{
					this.applyDamageAddition(Atk);
					this.splitMpByDamage(Atk, Atk._mpdmg, MANA_HIT.FROM_DAMAGE_SPLIT | MANA_HIT.FALL | MANA_HIT.FALL_EN, 3, 0.25f, null, true);
				}
			}
			if (base.NM2D.GameOver != null && base.Ser.apply_pe)
			{
				base.NM2D.GameOver.worm_damaged = true;
			}
			base.DMGE.applyWormTrapDamage(Atk, phase_count, flag, flag2, decline_additional_effect);
		}

		public void applyAbsorbDamage(NelAttackInfo Atk, bool execute_attack = true, bool mouth_damage = false, string fade_key = null, bool decline_additional_effect = false)
		{
			if (!this.Pr.canApplyAbrosb() || X.DEBUGNODAMAGE)
			{
				return;
			}
			this.Pr.JuiceCon.applyAbsorbDamage(Atk);
			this.Pr.BetoMng.Check(this.Pr, Atk, false, true);
			float num = 0f;
			bool flag = false;
			if (execute_attack)
			{
				int ep = this.Pr.ep;
				if (Atk._hpdmg > 0)
				{
					bool flag2;
					this.applyHpDamageSimple(Atk, out flag2, -1, true);
				}
				else
				{
					this.applyDamageAddition(Atk);
				}
				flag = ep < this.Pr.ep;
				num = this.splitMpByDamage(Atk, Atk._mpdmg, MANA_HIT.EN | MANA_HIT.FROM_DAMAGE_SPLIT | MANA_HIT.FALL | MANA_HIT.IMMEDIATE_COLLECTABLE, 0, 1f, null, true);
				if (num > 0f)
				{
					base.Ser.checkSer();
				}
				this.Pr.GSaver.LockTime(Atk);
			}
			base.DMGE.applyAbsorbDamage(Atk, execute_attack, mouth_damage, fade_key, decline_additional_effect, num, flag);
		}

		public bool setWaterDunk(int water_id, int misttype)
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

		private void checkLavaExecute()
		{
			int num = (int)(base.mbottom - X.Mn(base.sizey, 0.8f));
			int num2 = (int)base.x;
			if (CCON.isWater(base.Mp.getConfig(num2, num)) && base.NM2D.MIST.isFire(num2, num))
			{
				if (!this.NoDamage.isActive(NDMG.MAPDAMAGE_LAVA) || !base.isDamagingOrKo())
				{
					this.check_lava = 15f;
					M2MapDamageContainer.M2MapDamageItem m2MapDamageItem = base.NM2D.MDMGCon.Create(MAPDMG.LAVA, base.x, base.y, 0.05f, 0.05f, null);
					float num3;
					float num4;
					AttackInfo atk = m2MapDamageItem.GetAtk(null, this.Pr, out num3, out num4);
					if (this.Pr.applyDamageFromMap(m2MapDamageItem, atk, num3, num4, true) != null || !base.is_alive)
					{
						base.Ser.Add(SER.BURNED, -1, 1, false);
						this.check_lava = 30f;
						float vx = base.vx;
						this.Phy.killSpeedForce(false, true, true, false, false);
						if (base.is_alive)
						{
							this.lava_burn_dount++;
						}
						if (this.lava_burn_dount >= 3 && base.NM2D.CheckPoint.getCurPriority() < 5)
						{
							this.lava_burn_dount = 0;
						}
						if (this.lava_burn_dount >= 3)
						{
							this.Phy.killSpeedForce(true, true, true, false, false);
							this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._GRAVITY_LOCK, 0f, -0.09f, -1f, 0, 0, 40, 80, 2);
							base.NM2D.Iris.assignListener(base.NM2D.MIST).ForceWakeupInput(false);
							this.check_lava = 80f;
						}
						else
						{
							this.Phy.addFoc(FOCTYPE.DAMAGE, (float)((vx != 0f) ? X.MPF(vx > 0f) : X.MPFXP()) * (0.05f + 0.08f * X.XORSP()), -0.29f, -1f, 0, 30, 90, 10, 2);
						}
					}
					base.NM2D.MDMGCon.Release(m2MapDamageItem);
					return;
				}
			}
			else
			{
				if (CCON.isWater(base.Mp.getConfig((int)base.x, (int)base.mbottom)))
				{
					this.check_lava = 5f;
					return;
				}
				this.check_lava = 0f;
			}
		}

		public bool walkWormTrapped(M2WormTrap Worm, bool is_inner)
		{
			base.Ser.Add(SER.WORM_TRAPPED, -1, 99, false);
			if (!is_inner)
			{
				if (CAim._YD(Worm.head_aim, 1) < 0)
				{
					this.Phy.addLockGravity(Worm, 0.022f, 5f);
				}
				else
				{
					this.Phy.addLockGravity(Worm, 0.25f, 5f);
					this.Phy.clampSpeed(FOCTYPE.ABSORB, -1f, this.Pr.ySpeedMax0 * 0.33f, 0.125f);
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
				float num4 = base.Mp.GAR(0f, 0f, num, num2);
				this.Phy.addFoc(FOCTYPE.ABSORB, 0.02f * X.Cos(num4), ((CAim._YD(Worm.head_aim, 1) < 0) ? (-0.07f) : (-0.02f)) * X.Sin(num4), -1f, -1, 1, 0, -1, 0);
				this.Skill.wormFocApplied(Worm.head_aim);
			}
			else
			{
				this.Phy.killSpeedForce(true, true, true, false, false).addFric(5f);
				if (base.isWormTrapped())
				{
					if (base.NM2D.FCutin.isActive(this.Pr))
					{
						base.addD(M2MoverPr.DECL.THROW_RAY);
					}
					return true;
				}
				this.Skill.evadeTimeBlur(false, true, true);
				this.Phy.remLockGravity(Worm);
				this.Phy.addLockGravity(this.Pr, 0f, -1f);
				base.AbsorbCon.releaseFromTarget(this.Pr);
				this.Pr.recheck_emot = true;
				this.Pr.changeState(PR.STATE.WORM_TRAPPED);
				this.FootD.initJump(false, true, false);
				base.NM2D.MGC.killAllPlayerMagic(this.Pr, (MagicItem Mg) => !(Mg.Other is IMgBombListener));
				if (Worm.head_aim != AIM.B && this.Pr.isPoseDown(false))
				{
					base.SpSetPose("spike_trapped_down", -1, null, false);
					if (Worm.head_aim != AIM.T)
					{
						int num5 = (int)base.mpf_is_right * X.MPF(this.Pr.isPoseBack(false));
						int num6 = CAim._XD(Worm.head_aim, 1);
						base.Anm.setAim(CAim.get_aim2(0f, 0f, (float)(-(float)num6), (float)((num6 != num5) ? (-1) : 1), false), -1, false);
					}
					else
					{
						base.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(base.aim, 1), 0f, false), -1, false);
					}
				}
				else
				{
					base.SpSetPose(base.is_crouch ? "spike_trapped_crouch" : "spike_trapped", -1, null, false);
					if (Worm.head_aim == AIM.T)
					{
						base.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(base.aim, 1), -1f, false), -1, false);
					}
					else if (Worm.head_aim == AIM.B)
					{
						base.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(base.aim, 1), 1f, false), -1, false);
					}
					else
					{
						base.Anm.setAim(this.Pr.aim = CAim.get_aim2(0f, 0f, (float)(-(float)CAim._XD(Worm.head_aim, 1)), 0f, false), -1, false);
					}
				}
			}
			return is_inner;
		}

		public void setHpCrack(int _val)
		{
			_val = X.MMX(0, _val, 5);
			if (_val > this.hp_crack)
			{
				UIStatus.Instance.initHpCrack(false);
				PostEffect.IT.setSlow(10f, 0f, 0);
				base.Mp.playSnd((_val >= 5) ? "gage_crack_d" : "gage_crack");
				PostEffect.IT.addTimeFixedEffect(base.Mp.setET("mp_crack", 9f, 30f, 10f, 30, 0), 1f);
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
				this.Pr.applyHpDamage((int)this.Pr.get_maxhp() * 99, true, null);
				UIStatus.Instance.fineHpRatio(true, true);
				if (base.hp <= 0f)
				{
					this.Pr.initDeathStasis(true);
				}
			}
		}

		public bool isInjectableNormalState(PR.STATE state)
		{
			if (state <= PR.STATE.DAMAGE_L_DOWN_ABSORBAFTER)
			{
				if (state - PR.STATE.LAYING_EGG <= 1)
				{
					return base.state == state && base.hasD(M2MoverPr.DECL.FLAG1);
				}
				if (state == PR.STATE.WATER_CHOKED_RELEASE)
				{
					return base.state == state && !base.hasD(M2MoverPr.DECL.FLAG1);
				}
				if (state - PR.STATE.DAMAGE_L_LAND > 1)
				{
					return false;
				}
			}
			else if (state <= PR.STATE.DAMAGE_OTHER_STUN)
			{
				if (state != PR.STATE.DAMAGE_LT_LAND && state != PR.STATE.DAMAGE_OTHER_STUN)
				{
					return false;
				}
			}
			else if (state != PR.STATE.DOWN_STUN)
			{
				if (state - PR.STATE.DAMAGE_WEB_TRAPPED <= 1)
				{
					return true;
				}
				return false;
			}
			return base.hasD(M2MoverPr.DECL.FLAG2);
		}

		public Vector3 getDamageCounterShiftMapPos()
		{
			Vector3 vector = new Vector3(base.Anm.counter_shift_map_x, base.Anm.counter_shift_map_y, base.Anm.counter_expand_x);
			if (base.hasFoot())
			{
				vector.x += this.FootD.shift_pixel_x * base.Mp.rCLEN;
				vector.y -= this.FootD.shift_pixel_y * base.Mp.rCLEN;
			}
			return vector;
		}

		public bool isPenetrateShutterAttr(NDMG ndmg)
		{
			return base.is_alive && !base.isDamagingOrKo() && !base.isSinkState() && !this.Pr.strong_throw_ray && M2NoDamageManager.isPenetrateShutterAttr(ndmg);
		}

		public float getRE(RCP.RPI_EFFECT rpi_effect)
		{
			return this.Skill.getRE(rpi_effect);
		}

		public float t_stt_inject_lock
		{
			get
			{
				return this.t_stt_inject_lock_;
			}
			set
			{
				if (this.t_stt_inject_lock == value)
				{
					return;
				}
				this.t_stt_inject_lock_ = value;
				if (value <= 0f)
				{
					this.Pr.SttInjector.need_check_on_runpre = true;
				}
			}
		}

		public void LockSttInjection(float t)
		{
			this.t_stt_inject_lock = X.Mx(t, this.t_stt_inject_lock);
		}

		public void killLockSttInjection()
		{
			this.t_stt_inject_lock = 0f;
		}

		public M2PrSkill Skill
		{
			get
			{
				return this.Pr.Skill;
			}
		}

		public bool can_applydamage_state()
		{
			return this.Pr.can_applydamage_state();
		}

		public bool is_stt_injection_locked
		{
			get
			{
				return this.t_stt_inject_lock > 0f;
			}
		}

		private readonly PRMain PrM;

		private readonly RevCounterLock RCenemy_sink;

		private readonly M2NoDamageManager NoDamage;

		public const float dmg_blink_mul_ratio = 0.9f;

		private float absorb_additional_dying;

		private const int BURN_IRIS_COUNT = 3;

		public const float UKEMI_DELAY = 20f;

		public const float T_EVADE_IN_DMG = 13f;

		public const int knockback_time_web_trapped = 15;

		private const float walkSpeed_burned = 0.115f;

		private const float walkSpeed_air_burned = 0.085f;

		private float check_lava;

		private int lava_burn_dount;

		public int hp_crack;

		private float t_stt_inject_lock_;

		public const int ENEMYSINK_GUARD_TIME = 18;

		[Flags]
		public enum DMGRESULT
		{
			MISS = 0,
			S = 1,
			L = 2,
			WORM = 3,
			_MAIN = 255,
			_THUNDER = 256,
			_TO_DEAD = 512,
			_PENETRATE_ABSORB = 1024,
			_ABSORBING = 2048,
			_DMGT = 4096,
			_DMG_KIRIMOMI = 8192
		}
	}
}
