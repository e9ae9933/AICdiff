using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2PrADmgEffect : M2PrAssistant
	{
		public M2PrADmgEffect(PR _Pr)
			: base(_Pr)
		{
			this.PrM = this.Pr as PRMain;
		}

		public void applyDamage(NelAttackInfo Atk, int val, M2PrADmg.DMGRESULT res, ref UIPictureBase.EMSTATE add_emot, out float uipic_sx, out float uipic_sy)
		{
			bool flag = false;
			uipic_sx = (uipic_sy = 0f);
			bool flag2 = (res & M2PrADmg.DMGRESULT._TO_DEAD) > M2PrADmg.DMGRESULT.MISS;
			if ((res & M2PrADmg.DMGRESULT._MAIN) == M2PrADmg.DMGRESULT.WORM)
			{
				base.TeCon.clear();
				this.TeSetDmgBlink(Atk.attr, 60f, 0.9f, 0f, 0);
				this.PtcHit(Atk).PtcSTDead("hitl", flag2);
				add_emot |= UIPictureBase.EMSTATE.SMASH;
				uipic_sx = (float)(-8 + X.xors(16));
				uipic_sy = (float)(2 + X.xors(6));
			}
			else if ((res & M2PrADmg.DMGRESULT._MAIN) == M2PrADmg.DMGRESULT.L)
			{
				if ((res & M2PrADmg.DMGRESULT._ABSORBING) == M2PrADmg.DMGRESULT.MISS)
				{
					flag = true;
					if ((res & M2PrADmg.DMGRESULT._PENETRATE_ABSORB) != M2PrADmg.DMGRESULT.MISS)
					{
						base.TeCon.clear();
						this.TeSetDmgBlink(Atk.attr, 60f, 0.9f, 0f, 0);
					}
					else
					{
						this.QuVibP(14f, 8f, 6f, 0).QuVibP(7f, 22f, 1f, 0);
						this.TeSetDmgBlink(Atk.attr, 60f, 0.9f, 0f, 0);
					}
				}
				else
				{
					this.QuVibP(6f, 3f, 3f, 0).QuVibP(3f, 5f, 1f, 0);
					this.TeSetDmgBlink(Atk.attr, 10f, 0.9f, 0f, 0);
				}
				this.PtcHit(Atk).PtcSTDead("hitl", flag2);
				add_emot |= UIPictureBase.EMSTATE.SMASH;
				uipic_sx = (float)(-5 + X.xors(14));
				uipic_sy = (float)(3 + X.xors(15));
				if (Atk.attr == MGATTR.THUNDER)
				{
					if (!base.LockCntOccur.isLocked(PR.OCCUR.HITSTOP_THUNDER))
					{
						if (Atk.ndmg == NDMG.MAPDAMAGE_THUNDER)
						{
							PostEffect.IT.setSlow(33f, 0f, 0);
						}
						else
						{
							PostEffect.IT.setSlow(15f, 0.25f, 0);
						}
						if (!base.LockCntOccur.isLocked(PR.OCCUR.THUNDER_MOSAIC))
						{
							PostEffect.IT.addTimeFixedEffect(PostEffect.IT.setPEfadeinout(POSTM.THUNDER_TRAP, 40f, 10f, 1f, -30), 1f);
							base.LockCntOccur.Add(PR.OCCUR.THUNDER_MOSAIC, 240f);
						}
						PostEffect.IT.addTimeFixedEffect(base.TeCon.setQuakeSinH(7f, 28, 18.6f, 2f, 0), 0.33f);
						PostEffect.IT.addTimeFixedEffect(base.Anm, 1f);
					}
					base.LockCntOccur.Add(PR.OCCUR.HITSTOP_THUNDER, 140f);
					PostEffect.IT.addTimeFixedEffect(base.TeCon.setQuake(7f, 3, 4f, 0), 0.33f);
				}
			}
			else if ((res & M2PrADmg.DMGRESULT._MAIN) == M2PrADmg.DMGRESULT.S)
			{
				if ((res & M2PrADmg.DMGRESULT._ABSORBING) == M2PrADmg.DMGRESULT.MISS)
				{
					if ((res & M2PrADmg.DMGRESULT._PENETRATE_ABSORB) != M2PrADmg.DMGRESULT.MISS)
					{
						base.TeCon.clear();
						this.TeSetDmgBlink(Atk.attr, 60f, 0.9f, 0f, 0);
					}
					else
					{
						this.QuVibP(6f, 19f, 3f, 0);
						this.dmgBlinkSimple(Atk.attr, 40f);
					}
				}
				else
				{
					this.QuVibP(3f, 5f, 1f, 0);
					float num = X.XORSP();
					if (num < 0.4f)
					{
						base.TeCon.setQuake(4f, 9, 1f, 0);
					}
					else if (num < 0.6f)
					{
						base.TeCon.setQuakeSinH(7f, 20, X.NIXP(13.3f, 24f), 0f, 0);
					}
					else
					{
						this.TeSetDmgBlink(Atk.attr, 6f, 0.9f, 0f, 0);
					}
				}
				this.PtcHit(Atk).PtcSTDead("hits", flag2);
				uipic_sx = (float)(-4 + X.xors(9));
				uipic_sy = (float)(-2 + X.xors(11));
			}
			if (flag)
			{
				this.Pr.BetoMng.Check(this.Pr, BetoInfo.Ground, false, false);
			}
			base.VO.playDmgVo(Atk, val, res);
		}

		public void applyWormTrapDamage(NelAttackInfo Atk, int phase_count, bool dmg_applied, bool play_vo, bool decline_additional_effect)
		{
			this.Pr.UP.applyDamage(MGATTR.WORM, (float)(-1 + X.xors(6)), (float)(4 + X.xors(8)), UIPictureBase.EMSTATE.NORMAL, decline_additional_effect, null, false);
			if (dmg_applied)
			{
				base.DMGE.dmgBlinkSimple(MGATTR.WORM, X.NIXP(6f, 28f));
			}
			else
			{
				X.XORSP();
				if (X.XORSP() < 0.8f)
				{
					base.DMGE.dmgBlinkSimple(MGATTR.WORM, X.NIXP(6f, 28f));
				}
				if (X.XORSP() < 0.4f)
				{
					base.DMGE.PtcHit(MGATTR.WORM, base.x, base.y).PtcSTDead("hits", false);
				}
				base.NM2D.Mana.AddMulti(base.x, base.y, X.NIXP(4f, 8f) * 4f, MANA_HIT.FROM_DAMAGE_SPLIT | MANA_HIT.FALL | MANA_HIT.FALL_EN | MANA_HIT.FROM_ABSORB_SPLIT, 1f);
			}
			if (X.XORSP() < 0.33f)
			{
				base.TeCon.setEnlargeBouncy(1.08f, 1.08f, X.NIXP(13f, 26f), 0);
			}
			if (play_vo)
			{
				base.VO.playWormTrapVo(Atk, phase_count);
			}
		}

		public void applyPressDamage(NelAttackInfo Atk, int _xd, int _yd, bool play_ui_press_animation)
		{
			if (_xd != 0)
			{
				base.TeCon.setQuakeSinV(9f, 120, 82f, 1f, 0);
				base.TeCon.setQuakeSinH(3f, 130, 59f, 1f, 0);
			}
			else
			{
				base.TeCon.setQuakeSinH(9f, 120, 82f, 1f, 0);
				base.TeCon.setQuakeSinV(4f, 130, 59f, 1f, 0);
			}
			base.TeCon.setQuake(4f, 20, 1.7f, 0);
			base.PtcVar("hit_x", Atk.hit_x).PtcVar("hit_y", Atk.hit_y).PtcVar("agR", CAim.get_agR(base.aim, 0f) + 1.5707964f)
				.PtcVar("aim", (float)base.aim);
			base.PtcVar("ui_press", (float)(play_ui_press_animation ? 1 : 0)).PtcST("press_damage_pr", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			if (play_ui_press_animation)
			{
				int num = X.xors(8) | ((X.sensitive_level >= 2) ? 1 : 0);
				UIPictureBase.EMSTATE emstate = (((num & 1) != 0) ? UIPictureBase.EMSTATE.PROG0 : UIPictureBase.EMSTATE.NORMAL) | (((num & 2) != 0) ? UIPictureBase.EMSTATE.PROG1 : UIPictureBase.EMSTATE.NORMAL) | (((num & 4) != 0) ? UIPictureBase.EMSTATE.PROG2 : UIPictureBase.EMSTATE.NORMAL);
				string text = ((_xd == 0) ? "damage_press_t" : "damage_press");
				if (CFGSP.use_uipic_press_balance < 100 && text == "damage_press_t")
				{
					if (X.xors(100) >= (int)CFGSP.use_uipic_press_balance)
					{
						text = "damage_press";
					}
				}
				else if (CFGSP.use_uipic_press_balance > 100 && text == "damage_press" && X.xors(100) < (int)(CFGSP.use_uipic_press_balance - 100))
				{
					text = "damage_press_t";
				}
				this.Pr.UP.setFade(text, emstate, true, true, false);
				this.Pr.UP.applyUiPressDamage();
				Atk._apply_knockback_current = false;
				base.Anm.setPose((base.state == PR.STATE.DAMAGE_PRESS_TB) ? "ui_press_damage_down" : "ui_press_damage", -1, false);
				bool flag = true;
				if (_xd == 0)
				{
					base.Anm.setAim(CAim.get_aim2(0f, 0f, (float)CAim._XD(base.aim, 1), (float)_yd, false), -1, false);
				}
				int num2;
				if (text == "damage_press_t")
				{
					if ((emstate & UIPictureBase.EMSTATE.PROG2) != UIPictureBase.EMSTATE.NORMAL)
					{
						num2 = 10;
					}
					else if ((emstate & UIPictureBase.EMSTATE.PROG1) != UIPictureBase.EMSTATE.NORMAL)
					{
						num2 = (((emstate & UIPictureBase.EMSTATE.PROG0) != UIPictureBase.EMSTATE.NORMAL) ? 9 : 8);
						flag = false;
					}
					else
					{
						num2 = (((emstate & UIPictureBase.EMSTATE.PROG0) != UIPictureBase.EMSTATE.NORMAL) ? 4 : 0);
					}
				}
				else if ((emstate & (UIPictureBase.EMSTATE.PROG1 | UIPictureBase.EMSTATE.PROG2)) == UIPictureBase.EMSTATE.PROG2 || (emstate & (UIPictureBase.EMSTATE.PROG1 | UIPictureBase.EMSTATE.PROG2)) == UIPictureBase.EMSTATE.PROG1)
				{
					num2 = (((emstate & UIPictureBase.EMSTATE.PROG0) != UIPictureBase.EMSTATE.NORMAL) ? 9 : 8);
					flag = false;
				}
				else if ((emstate & UIPictureBase.EMSTATE.PROG0) != UIPictureBase.EMSTATE.NORMAL)
				{
					num2 = 4;
				}
				else
				{
					num2 = 0;
				}
				if (flag && this.Pr.UP.isActive())
				{
					UIPictureBodySpine uipictureBodySpine = this.Pr.UP.getBodyData() as UIPictureBodySpine;
					if (uipictureBodySpine != null)
					{
						SpineViewerNel viewer = uipictureBodySpine.getViewer();
						if (viewer.hasAnim("f_m4"))
						{
							num2 += 3;
						}
						else if (viewer.hasAnim("f_m3"))
						{
							num2 += 2;
						}
						else if (viewer.hasAnim("f_m1"))
						{
							num2++;
						}
					}
				}
				base.Anm.animReset(num2);
				return;
			}
			base.Mp.DropCon.setBlood(this.Pr, 57, MTR.col_blood, (float)(-(float)_xd) * 0.08f, true);
		}

		public void applyDamageAfter(NelAttackInfo Atk, bool up_apply, string fade_key, string fade_key0, UIPictureBase.EMSTATE add_emot, float uipic_sx, float uipic_sy, bool decline_ui_additional_effect)
		{
			if (!base.AbsorbCon.use_torture)
			{
				if (!base.is_alive && base.Anm.strictPoseIs("buried"))
				{
					base.Anm.setPose("buried_death", -1, false);
				}
				if (base.Anm.poseIs(POSE_TYPE.DAMAGE_RESET, false))
				{
					base.Anm.animReset(X.xors(3));
				}
			}
			if (up_apply && Atk.setable_UP)
			{
				if (base.AbsorbCon.isActive() && TX.noe(fade_key0))
				{
					string uipicture_fade_key = base.AbsorbCon.uipicture_fade_key;
					if (TX.valid(uipicture_fade_key) && base.AbsorbCon.normal_UP_fade_injectable <= X.XORSP())
					{
						fade_key = uipicture_fade_key;
					}
				}
				this.Pr.UP.applyDamage(Atk.attr, uipic_sx, uipic_sy, add_emot, decline_ui_additional_effect, fade_key, false);
			}
		}

		public void applyAbsorbDamage(NelAttackInfo Atk, bool execute_attack, bool mouth_damage = false, string fade_key = null, bool decline_additional_effect = false, float mpreduced = 0f, bool ep_added = false)
		{
			if (X.XORSP() < 0.23f)
			{
				base.NM2D.Cam.setQuake(2f, 7, 1f, 0);
			}
			if (X.XORSP() < 0.6f)
			{
				this.Pr.PadVib(execute_attack ? "dmg_absorb" : "dmg_absorb_s", X.NIXP(0.4f, 1f));
			}
			if (!base.AbsorbCon.no_shuffleframe_on_applydamage && X.XORSP() < 0.63f)
			{
				base.Anm.randomizeFrame();
				base.TeCon.setQuake(2f, 7, 1f, 4);
			}
			if (execute_attack)
			{
				PostEffect.IT.setPEabsorbed(POSTM.MP_ABSORBED, 4f, X.NIXP(0.6f, 0.9f), -2);
				if (Atk.attr == MGATTR.FIRE && X.XORSP() < 0.33f)
				{
					Vector3 hipPos = this.Pr.getHipPos();
					base.PtcVarS("attr", FEnum<MGATTR>.ToStr(Atk.attr)).PtcVar("x", hipPos.x).PtcVar("y", hipPos.y)
						.PtcVar("hit_x", hipPos.x)
						.PtcVar("hit_y", hipPos.y)
						.PtcST("hits", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (mpreduced > 0f)
			{
				this.TeSetDmgBlink(Atk.attr, 10f, 0.9f, 0f, 0);
				Atk.playEffect(this.Pr);
			}
			else
			{
				this.TeSetDmgBlink(Atk.attr, (float)((!base.is_alive && this.Pr.overkill && X.XORSP() < 0.3f) ? 12 : 4), 0.9f, 0f, 0);
			}
			if (mouth_damage)
			{
				if (CFGSP.epdmg_vo_mouth < 100 && (int)CFGSP.epdmg_vo_mouth <= X.xors(100))
				{
					mouth_damage = false;
				}
			}
			else if (CFGSP.epdmg_vo_mouth > 100 && (int)(CFGSP.epdmg_vo_mouth - 100) >= X.xors(100))
			{
				mouth_damage = true;
			}
			if (TX.noe(fade_key))
			{
				fade_key = base.AbsorbCon.uipicture_fade_key;
			}
			if (TX.noe(fade_key) && base.isWormTrapped())
			{
				fade_key = "insected";
			}
			if (base.AbsorbCon.current_pose_priority == 0 && X.XORSP() < 0.75f)
			{
				string text = base.SfPose.absorb_default_pose(X.XORSP() < (base.is_alive ? 0.012f : 0.03f) * 2.8f);
				base.SpSetPose(text, -1, null, false);
				if (text == "absorb2absorb_crouch" && TX.noe(fade_key))
				{
					fade_key = "crouch";
				}
			}
			if (Atk.setable_UP)
			{
				this.Pr.UP.applyDamage(Atk.attr, (float)(4 + X.xors(4)) * (execute_attack ? 1f : 0.5f), 0f, UIPictureBase.EMSTATE.NORMAL, decline_additional_effect, fade_key, false);
			}
			base.VO.playAbsorbDamageVo(Atk, execute_attack, mouth_damage, ep_added);
		}

		public void executeReleaseFromTrapByDamage(out float _vx, out float _vy)
		{
			float num = (float)CAim._YD(base.Anm.pose_aim, 1);
			_vx = 0f;
			_vy = 0f;
			if (base.Anm.poseIs("spike_trapped", "spike_trapped_crouch"))
			{
				if (num == 0f)
				{
					_vy = -0.13f;
					_vx = -base.mpf_is_right * 0.23f;
				}
				else
				{
					_vx = X.NIXP(-0.07f, 0.07f);
					_vy = 0.18f * num;
				}
			}
			else if (num == 0f)
			{
				_vx = X.NIXP(-0.07f, 0.07f);
				_vy = -0.25f * num;
			}
			else
			{
				_vx = -base.mpf_is_right * 0.25f;
				_vy = -0.09f;
			}
			base.PtcVar("agR", base.Mp.GAR(0f, 0f, _vx, _vy)).PtcST("worm_released_splash", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			this.FootD.initJump(false, false, false);
			this.Phy.addLockMoverHitting(HITLOCK.DAMAGE, 8f);
			this.Pr.recheck_emot = true;
		}

		public void effectShieldBreak(MGATTR attr, float hit_x, float hit_y)
		{
			this.QuVibP(14f, 8f, 6f, 0).QuVibP(7f, 22f, 1f, 0);
			base.Ser.Add(SER.SHIELD_BREAK, -1, 99, false);
			this.TeSetDmgBlink(attr, 90f, 0.9f, 0f, 0);
			this.Pr.UP.applyDamage(attr, 1f, 9f, UIPictureBase.EMSTATE.NORMAL, false, null, false);
			this.Pr.playVo("shield_break", false, false);
			base.PtcVarS("attr", FEnum<MGATTR>.ToStr(MGATTR.NORMAL)).PtcVar("hit_x", hit_x).PtcVar("hit_y", hit_y)
				.PtcST("hitl", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
		}

		public void effectAbsorbInit(AttackInfo Atk)
		{
			base.PtcVarS("attr", FEnum<MGATTR>.ToStr(Atk.attr));
			base.PtcVar("hit_x", Atk.hit_x);
			base.PtcVar("hit_y", Atk.hit_y);
			base.PtcST("hitabsorb", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
		}

		public void effectParry(AttackInfo Atk)
		{
			PostEffect it = PostEffect.IT;
			it.setSlowFading(20f, 5f, 0f, -20);
			it.addTimeFixedEffect(it.setPEbounce(POSTM.ZOOM2, 24f, 0.03f, -5), 1f);
			float num = X.NI(Atk.hit_x, base.x, 0.63f) + base.mpf_is_right * 0.32f;
			float num2 = X.NI(Atk.hit_y, base.y, 0.4f);
			float num3 = base.Mp.GAR(0f, base.y, (num - base.x) * 5f, num2) + base.mpf_is_right * 1.5707964f;
			base.PtcVar("hx", num);
			base.PtcVar("hy", num2);
			base.PtcVar("hagR", num3);
			base.PtcHld.PtcSTTimeFixed("hit_parry", 0.8f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
		}

		public void effectParalysisDamage()
		{
			base.Anm.setPose((this.Pr.isPoseBackDown(false) || X.XORSP() < 0.7f) ? "downdamage" : "downdamage_t", -1, false);
			base.PtcVarS("attr", FEnum<MGATTR>.ToStr(MGATTR.THUNDER)).PtcVar("hit_x", base.x).PtcVar("hit_y", base.y)
				.PtcST("hits", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			this.Pr.UP.applyDamage(MGATTR.THUNDER, 8f, 1f, UIPictureBase.EMSTATE.NORMAL, false, "damage_thunder", false);
			base.VO.playParalysisVo();
		}

		public TransEffecterItem setBurnedEffect(bool playvo, bool applyemot, bool decline_apply_emot = false)
		{
			if (playvo)
			{
				this.Pr.playVo("heat", false, false);
			}
			TransEffecterItem transEffecterItem = base.DMGE.TeSetDmgBlink(MGATTR.FIRE, (float)(25 + X.xors(15)), 1f, 1f, 0);
			this.Pr.UP.applyBurned(applyemot && !this.Pr.isBenchOrGoRecoveryState(), true);
			bool flag = this.Pr.UP.getCurEmot() == UIEMOT.BURNED;
			if (!decline_apply_emot && ((!flag && !base.LockCntOccur.isLocked(PR.OCCUR.BURNED_FADER)) || applyemot))
			{
				this.Pr.UP.setFade("burned", UIPictureBase.EMSTATE.NORMAL, false, false, false);
				if (!flag && this.Pr.UP.getCurEmot() == UIEMOT.BURNED)
				{
					base.LockCntOccur.Add(PR.OCCUR.BURNED_FADER, X.NIXP(240f, 340f));
				}
			}
			return transEffecterItem;
		}

		public void publishVaginaSplash(uint col, int count, float splash_speed_level = 1f)
		{
			base.Mp.DropCon.setLoveJuice(this.Pr, count, col, 1f, false);
		}

		public Vector3 publishVaginaSplashPiston(float shot_ax, bool no_snd = false)
		{
			Vector3 hipPos = this.Pr.getHipPos();
			base.PtcVar("_dx", hipPos.x).PtcVar("_dy", hipPos.y).PtcVar("ax", shot_ax)
				.PtcST(no_snd ? "pr_piston_nosnd" : "pr_piston", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			return hipPos;
		}

		public void dmgBlinkSimple(MGATTR attr, float blink_t = 40f)
		{
			base.TeCon.setQuake(4f, 9, 1f, 0);
			this.TeSetDmgBlink(attr, blink_t, 0.9f, 0f, 0);
			if (X.XORSP() < 0.4f)
			{
				base.TeCon.setQuakeSinH(4f, 20, 1.7f, 0f, 0);
			}
		}

		public TransEffecterItem TeSetDmgBlink(MGATTR attr, float maxt = 0f, float mul_ratio = 1f, float add_ratio = 0f, int _saf = 0)
		{
			maxt *= (float)CFGSP.dmgte_pixel_duration * 0.01f;
			mul_ratio *= (float)CFGSP.dmgte_pixel_density * 0.01f;
			add_ratio *= (float)CFGSP.dmgte_pixel_density * 0.01f;
			return base.TeCon.setDmgBlink(attr, maxt, mul_ratio, add_ratio, _saf);
		}

		public M2PrADmgEffect PtcHit(AttackInfo Atk)
		{
			return this.PtcHit(Atk.attr, Atk.hit_x, Atk.hit_y);
		}

		public M2PrADmgEffect PtcHit(MGATTR attr, float hit_x, float hit_y)
		{
			string text = ((attr == MGATTR.STAB || attr == MGATTR.BITE || attr == MGATTR.CUT_H) ? "hitl_stab" : "hitl");
			base.PtcVar("cx", base.drawx * base.Mp.rCLEN);
			base.PtcVar("cy", base.drawy * base.Mp.rCLEN);
			base.PtcVarS("attr", FEnum<MGATTR>.ToStr(attr));
			base.PtcVarS("snd_hitl", text);
			base.PtcVar("hit_x", hit_x);
			base.PtcVar("hit_y", hit_y);
			return this;
		}

		public void PtcSTDead(string ptc_key, bool to_dead)
		{
			if (ptc_key != null)
			{
				if (!base.PtcHld.first_ver)
				{
					base.defineParticlePreVariable();
				}
				base.PtcHld.PtcSTTimeFixed(ptc_key, 0.6f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
			}
			UIStatus.Instance.fineHpRatio(true, true);
			if (to_dead)
			{
				PostEffect.IT.addTimeFixedEffect(base.NM2D.Cam.Qu, 0.3f);
				this.QuVibP(6f, 10f, 4f, 0);
				this.QuHandShake(40f, 60f, 8f, 0);
				PostEffect.IT.setSlowFading(80f, 90f, 0.02f, -80);
				PostEffect.IT.setPEfadeinout(POSTM.FINAL_ALPHA, 70f, 50f, 0.8f, -70);
				PostEffect.IT.addTimeFixedEffect(PostEffect.IT.setPEfadeinout(POSTM.SUMMONER_ACTIVATE, 70f, 50f, 0.5f, -70), 1f);
				if (this.Pr.isStoneSer() && base.Ser.apply_pe)
				{
					string currentFadeKey = this.Pr.UP.getCurrentFadeKey(true);
					this.Pr.UP.setFade(TX.noe(currentFadeKey) ? "damage" : currentFadeKey, UIPictureBase.EMSTATE.NORMAL, true, true, false);
				}
			}
		}

		public void effectEggNearlyLaying()
		{
			this.Pr.playSndPos("heartbeat_ll", 1);
			PostEffect.IT.setSlowFading(70f, 60f, 0.3f, -40);
			PostEffect.IT.setPEfadeinout(POSTM.ZOOM2, 60f, 20f, 0.1f, -30);
			PostEffect.IT.setPEfadeinout(POSTM.GAS_APPLIED, 100f, 30f, 0.5f, -70);
			PostEffect.IT.setPEfadeinout(POSTM.FINAL_ALPHA, 50f, 30f, 0.25f, -30);
			this.QuHandShake(40f, 60f, 8f, 0);
		}

		public void effectEggInitLaying(float eftime)
		{
			this.Pr.playSndPos("heartbeat_ll", 1);
			this.Pr.PtcST("laying_egg_activate", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			PostEffect.IT.setPEfadeinout(POSTM.FINAL_ALPHA, 50f, 30f, 0.25f, -30);
			PostEffect.IT.setPEfadeinout(POSTM.GAS_APPLIED, 60f, 30f, 0.5f, -40);
			PostEffect.IT.setPEfadeinout(POSTM.ZOOM2, 60f, eftime, 1f, 0);
			PostEffect.IT.setPEfadeinout(POSTM.LAYING_EGG, 140f, eftime, 1f, 0);
			PostEffect.IT.setPEfadeinout(POSTM.HEARTBEAT, 150f, X.Mx(eftime - 120f, 0f), 1f, 0);
		}

		public M2PrADmgEffect QuVib(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			base.NM2D.Cam.Qu.Vib(_slevel, _time, _elevel, _saf);
			return this;
		}

		public M2PrADmgEffect QuVibP(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			base.NM2D.Cam.Qu.VibP(_slevel, _time, _elevel, _saf);
			return this;
		}

		public M2PrADmgEffect QuSinH(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			base.NM2D.Cam.Qu.SinH(_slevel, _time, _elevel, _saf);
			return this;
		}

		public M2PrADmgEffect QuSinV(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			base.NM2D.Cam.Qu.SinV(_slevel, _time, _elevel, _saf);
			return this;
		}

		public M2PrADmgEffect QuSinR(float _slevel, float _time, float agR, float _elevel = -1f, int _saf = 0)
		{
			base.NM2D.Cam.Qu.SinR(_slevel, _time, agR, _elevel, _saf);
			return this;
		}

		public M2PrADmgEffect QuHandShake(float _holdtime, float _fadetime, float _level, int _saf = 0)
		{
			base.NM2D.Cam.Qu.HandShake(_holdtime, _fadetime, _level, _saf);
			return this;
		}

		private PRMain PrM;

		public const float dmg_blink_mul_ratio = 0.9f;

		public const float absorb_pose_change_ratio = 2.8f;
	}
}
