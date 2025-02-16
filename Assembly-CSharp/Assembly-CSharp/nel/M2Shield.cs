using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2Shield
	{
		public M2Shield(M2Attackable _Mv, MGHIT _mvhit = MGHIT.EN)
		{
			this.FD_draw = new M2DrawBinder.FnEffectBind(this.draw);
			if (M2Shield.FD_drawLine == null)
			{
				M2Shield.FD_drawLine = new OctaHedronDrawer.FnDrawOctaHedronLine(M2Shield.drawLine);
				M2Shield.FD_drawSurfaceSilhouette = new OctaHedronDrawer.FnDrawOctaHedronSurface(M2Shield.drawSurfaceSilhouette);
				M2Shield.FD_drawSurfaceBlur = new OctaHedronDrawer.FnDrawOctaHedronSurface(M2Shield.drawSurfaceBlur);
				M2Shield.FD_drawSurfaceDeactivate = new OctaHedronDrawer.FnDrawOctaHedronSurface(M2Shield.drawSurfaceDeactivate);
				M2Shield.FD_drawPoint = new OctaHedronDrawer.FnDrawOctaHedronPoint(M2Shield.drawPoint);
				M2Shield.MtrSilhouette = MTR.newMtr("Nel/ShieldSilhouette");
				M2Shield.MtrBlurBehind = MTR.newMtr("Nel/ShieldBlurBehind");
			}
			this.Mv = _Mv;
			this.mvhit = _mvhit;
			this.Ocd = new OctaHedronDrawer(1f, 1f);
			this.Qu = new Quaker(null);
			this.t_actv = new RevCounterLock();
			this.FlgShieldLock = new FlagCounterR<AttackInfo>();
			this.sizeRatio(1f);
			this.appearable_time = this.appearable_time_;
			this.initS();
		}

		public void sizeRatio(float z = 1f)
		{
			M2Shield.sizeRatio(this.Ocd, z);
		}

		public static void sizeRatio(OctaHedronDrawer Ocd, float z)
		{
			Ocd.w = M2Shield.octa_w * z;
			Ocd.h = M2Shield.octa_h * z;
		}

		public static float octa_w
		{
			get
			{
				return 74.8f;
			}
		}

		public static float octa_h
		{
			get
			{
				return M2Shield.octa_w * 1.18f;
			}
		}

		public M2Shield initS()
		{
			try
			{
				if (this.Hitable != null)
				{
					IN.DestroyE(this.Hitable.gameObject);
				}
			}
			catch
			{
			}
			this.Hitable = null;
			this.MhUnmnp.destruct(this.Hitable);
			this.shifty = (this.shiftx = 0f);
			this.resetValue(true);
			return this;
		}

		public M2Shield resetValue(bool flag_clear = true)
		{
			this.pow = 0f;
			this.hit_t = 0f;
			this.alpha = 0f;
			if (flag_clear)
			{
				this.FlgShieldLock.Clear();
			}
			this.t_actv.Clear();
			this.scale = 1f;
			this.recover_lock_t = 0f;
			this.t_lariat_enlarge_power = 0f;
			this.changeStateNormal();
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			return this;
		}

		public bool activate(bool force = false)
		{
			this.t_actv.Add(0.001f, false, false);
			if (!force && !this.isManipulatableState())
			{
				if (this.stt == M2Shield.STATE.BROKEN)
				{
					if (this.Ed == null)
					{
						this.initEffect();
					}
					this.alpha = -1f;
					return false;
				}
				if (this.isLariatState() && !this.isGuardableLgt(true))
				{
					this.follow_speed = 0.06f;
					return false;
				}
			}
			if (this.alpha <= 0f)
			{
				this.alpha = X.Mx(-this.alpha, 0.00390625f);
				if (this.activate_snd != "")
				{
					this.Mp.playSnd(this.activate_snd, this.snd_key, this.Mv.x, this.Mv.y, 1);
				}
			}
			this.changeStateNormal();
			return true;
		}

		public M2Shield deactivate(bool force = false, bool immediate = false)
		{
			if (this.alpha > 0f)
			{
				if (this.fnSwitchActivation != null && !this.fnSwitchActivation(false) && !force)
				{
					return this;
				}
				if (!force && !this.isManipulatableState() && this.isLariatState() && !this.isGuardableLgt(true))
				{
					return this;
				}
				this.Mv.M2D.Snd.kill(this.snd_key);
				this.alpha = X.MMX(-1f, -this.alpha, 0f);
				this.changeStateNormal();
				this.sizeRatio(1f);
				Vector2 visivilityPos = this.getVisivilityPos();
				this.Mv.PtcVar("w", (double)this.Ocd.w).PtcVar("h", (double)this.Ocd.h).PtcVar("cx", (double)visivilityPos.x)
					.PtcVar("cy", (double)visivilityPos.y)
					.PtcST("shield_deactivate", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (immediate)
			{
				this.alpha = 0f;
			}
			return this;
		}

		public bool changeStateLariat(float fcnt, bool progress = false, bool follow_to_mv = false)
		{
			if (!this.isLariatState())
			{
				if (!this.activate(false) || !this.canGuard())
				{
					return false;
				}
				this.applyDamage(40f);
				this.Mv.PtcST("shield_lariat_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.t_lariat_enlarge_power = 0f;
				this.t_actv.Clear();
			}
			if (this.t_actv.Equals(0))
			{
				this.posrot_agR = ((this.Mv.mpf_is_right > 0f) ? 0f : 3.1415927f);
			}
			this.t_actv.ClearLock();
			this.stt = M2Shield.STATE.LARIAT;
			if (progress)
			{
				this.t_actv.Add((this.t_actv.Get() < this.LARIAT_ACTV_MAXT) ? fcnt : 0f, false, false);
				if (follow_to_mv)
				{
					this.follow_speed = 0.075f;
				}
			}
			return true;
		}

		public void changeStateNormal()
		{
			this.follow_speed = ((this.alpha <= 0f) ? (-1000f) : 0.25f);
			this.stt = M2Shield.STATE.SHOWING;
			this.t_lariat_enlarge_power = 0f;
			this.t_actv.Clear();
			this.Ocd.AgR(0f);
			this.bottom_draw_flag = false;
			this.MhUnmnp.destruct(this.Hitable);
		}

		public static void resetMaterial()
		{
			M2Shield.MaterialInitialize(M2Shield.MtrSilhouette, M2Shield.MtrBlurBehind, M2Shield.ColSub.C);
		}

		private void initEffect()
		{
			if (this.Ed != null)
			{
				this.Ed = this.Mp.remED(this.Ed);
			}
			this.Ed = this.Mp.setED("Shield", this.FD_draw, 0f);
		}

		public static void MaterialInitialize(Material MtrSilhouette, Material MtrBlurBehind, Color32 _ColSub)
		{
			M2DBase instance = M2DBase.Instance;
			if (MtrSilhouette != null)
			{
				MtrSilhouette.SetFloat("_ScreenMargin", 8f);
				MtrSilhouette.SetColor("_Color", _ColSub);
				if (instance.Cam.CamForMover != null)
				{
					MtrSilhouette.SetTexture("_MainTex", instance.Cam.CamForMover.targetTexture);
				}
			}
			if (MtrBlurBehind != null)
			{
				MtrBlurBehind.SetFloat("_ScreenMargin", 8f);
				if (instance.Cam.CamForMover != null)
				{
					MtrBlurBehind.SetTexture("_MainTex", instance.Cam.CamForMover.targetTexture);
				}
				MtrBlurBehind.SetTexture("_BehindTex", instance.Cam.getFinalizedTexture());
			}
		}

		public void cure()
		{
			this.pow = 0f;
			this.recover_lock_t_ = 0f;
			this.hit_t = 0f;
			this.follow_speed = -1000f;
		}

		public void run(float fcnt, float power_progress_level = 1f, float fcnt_for_lock_time_progress = -1f)
		{
			this.activating = this.t_actv.isAdding();
			if (fcnt_for_lock_time_progress < 0f)
			{
				fcnt_for_lock_time_progress = fcnt;
			}
			this.FlgShieldLock.run(fcnt_for_lock_time_progress);
			int num = -1;
			if (this.follow_speed == -1000f)
			{
				this.x = this.Mv.x;
				this.y = this.Mv.y;
				this.shiftx = X.VALWALK(this.shiftx, 0f, fcnt * 0.06f);
				this.shifty = X.VALWALK(this.shifty, 0f, fcnt * 0.06f);
				this.scale = X.MULWALK(this.scale, 1f, 0.14f * fcnt);
			}
			else if (this.follow_speed > 0f)
			{
				this.x = X.VALWALK(this.x, this.Mv.x, fcnt * this.follow_speed);
				this.y = X.VALWALK(this.y, this.Mv.y, fcnt * this.follow_speed);
			}
			float num2 = 1f;
			bool flag = false;
			if (this.alpha > 0f)
			{
				bool flag2 = true;
				if (this.alpha < 1f && this.alpha > 0f)
				{
					this.alpha = X.Mn(1f, this.alpha + fcnt / this.activate_time);
					if (this.alpha >= 1f && this.appear_snd != "")
					{
						this.Mp.playSnd(this.appear_snd, "", this.Mv.x, this.Mv.y, 1);
					}
				}
				else
				{
					flag2 = true;
					this.alpha = X.VALWALK(this.alpha, 2f, 0.045454547f * fcnt);
					float num3 = this.pow;
					float num4 = this.appearable_time_;
				}
				if (this.Ed == null)
				{
					this.initEffect();
				}
				if (this.stt == M2Shield.STATE.SHOWING)
				{
					this.follow_speed = 0.25f;
					float num5 = this.appearable_time_ * 0.7f;
					this.Ocd.zagR = ((this.pow >= num5) ? (X.SINI(this.pow - num5, 47f) * 0.04f * 3.1415927f) : 0f) + 0.04712389f;
				}
				else if (this.isLariatState())
				{
					this.recover_lock_t = 90f;
					if (this.stt == M2Shield.STATE.LARIAT)
					{
						if (!this.t_actv.isAdding())
						{
							this.stt = M2Shield.STATE.LARIAT_HOLD;
							this.t_actv.Lock(20f, false);
						}
						else
						{
							flag = true;
							bool flag3 = this.t_lariat_enlarge_power >= 30f;
							this.t_lariat_enlarge_power += fcnt;
							if (!flag3 && this.t_lariat_enlarge_power >= 30f)
							{
								this.t_actv.Set(this.LARIAT_ACTV_MAXT, false);
								this.posrot_agR = 1.5707964f;
							}
						}
					}
					flag2 = !this.activating;
					float num6 = 0f;
					float mpf_is_right = this.Mv.mpf_is_right;
					if (this.t_lariat_enlarge_power < 30f)
					{
						this.follow_speed = 0.84f * (1f - X.ZLINE(this.t_lariat_enlarge_power, 30f));
						this.scale = X.ZSIN(this.t_lariat_enlarge_power, 30f) * 0.25f + 1f;
						this.Ocd.zagR = 0.06f * mpf_is_right * 3.1415927f * X.SINI(this.t_lariat_enlarge_power - 30f, 17.33f);
					}
					else
					{
						this.Ocd.zagR = 0.09f * mpf_is_right * 3.1415927f * X.SINI(this.t_lariat_enlarge_power - 30f, 14.33f);
						if (this.follow_speed == 0.06f)
						{
							this.scale = X.VALWALK(this.scale, 1f, 0.028f);
						}
						else
						{
							this.scale = X.VALWALK(this.scale, X.NI(1.25f, 2.5f, X.ZCOS(this.t_lariat_enlarge_power - 30f, 220f)), 0.016f);
						}
						this.follow_speed = (this.t_actv.isAdding() ? 0.018f : 0f);
						num6 = X.ZSIN(this.t_lariat_enlarge_power - 30f, 15f) * X.NI(1, 0, X.ZSINV(this.t_lariat_enlarge_power - 30f, 220f)) * 2.6f * X.ZSIN(this.t_actv.Get(), this.LARIAT_ACTV_MAXT);
						this.posrot_agR += 3.1415927f * X.NIL(0.005f, 0.04f, this.t_actv.Get(), this.LARIAT_ACTV_MAXT) * this.Mv.mpf_is_right;
					}
					this.shiftx = X.Cos(this.posrot_agR) * num6;
					this.shifty = 0f;
					num2 = ((this.t_actv.isAdding() || this.t_actv.has_lock) ? 1f : 0.5f);
					num = ((!this.isManipulatable()) ? ((num6 < 0.6f) ? 0 : ((this.shiftx > 0f) ? 2 : 1)) : (-1));
				}
				if (flag2)
				{
					this.pow += power_progress_level * fcnt;
				}
			}
			else
			{
				this.follow_speed = -1000f;
				if (power_progress_level > 0f)
				{
					if (this.recover_lock_t_ > 0f)
					{
						this.recover_lock_t_ = X.Mx(this.recover_lock_t_ - fcnt, 0f);
					}
					else
					{
						this.pow = X.VALWALK(this.pow, 0f, fcnt * X.Scr(power_progress_level) * this.recover_addt * ((this.stt == M2Shield.STATE.BROKEN) ? this.broken_recover_TS : 1f));
						if (this.pow == 0f && this.stt == M2Shield.STATE.BROKEN)
						{
							this.stt = M2Shield.STATE.SHOWING;
						}
					}
				}
				if (this.alpha < 0f)
				{
					this.alpha = X.Mn(0f, this.alpha + 1f / this.deactivate_anim_time);
					if (this.alpha == 0f && this.Ed != null)
					{
						this.Ed = this.Mp.remED(this.Ed);
						this.hit_t = 0f;
					}
				}
			}
			if (num >= 0 && this.AtkUnmnp != null)
			{
				if (!this.MhUnmnp.isActive(this.Hitable))
				{
					if (this.Hitable == null)
					{
						this.Hitable = IN.CreateGob(this.Mp.gameObject, "-" + this.Mv.key + "-ShieldHitable").AddComponent<M2ShieldHitable>();
						this.Hitable.Init(this);
						this.FD_fnMagicLariatUnmnp = new MagicItem.FnMagicRun(this.fnMagicLariatUnmnp);
					}
					this.Hitable.enabled = true;
					MagicItem magicItem = (this.Mv.M2D as NelM2DBase).MGC.setMagic(this.Hitable, MGKIND.BASIC_SHOT, ((this.mvhit == MGHIT.PR) ? MGHIT.EN : MGHIT.PR) | MGHIT.IMMEDIATE).initFuncNoDraw(this.FD_fnMagicLariatUnmnp);
					this.MhUnmnp = new MagicItemHandlerS(magicItem);
					magicItem.sx = (magicItem.sy = 0f);
					magicItem.Atk0 = this.AtkUnmnp;
					magicItem.Atk0.PublishMagic = magicItem;
					magicItem.run(0f);
					flag = true;
				}
				else
				{
					flag = flag || this.activating;
				}
				this.MhUnmnp.sa = (float)(flag ? 1 : 0);
				M2DropObject dro = this.MhUnmnp.Mg.Dro;
				if (flag && this.MhUnmnp.sz == 0f)
				{
					Vector2 visivilityPos = this.getVisivilityPos();
					this.MhUnmnp.sx = (dro.x = visivilityPos.x);
					this.MhUnmnp.sy = (dro.y = visivilityPos.y);
					dro.initJump(false, false, false);
					dro.vx = (dro.vy = 0f);
				}
				else
				{
					if (flag)
					{
						this.x = X.VALWALK(this.x, dro.x - this.shiftx, 0.4f * fcnt);
						this.y = X.VALWALK(this.y, dro.y - this.shifty, 0.4f * fcnt);
					}
					else
					{
						this.x = dro.x - this.shiftx;
						this.y = dro.y - this.shifty;
					}
					if (num >= 0 && this.y >= (float)this.Mp.rows)
					{
						num = -1;
						this.deactivate(true, true);
					}
				}
				if (num >= 0)
				{
					this.Hitable.reposit(M2Shield.octa_w, this.scale);
					if (num == 0)
					{
						this.AtkUnmnp.burst_center = 0.8f;
						this.AtkUnmnp.burst_vx = X.Abs(this.AtkUnmnp.burst_vx);
					}
					else
					{
						this.AtkUnmnp.burst_center = 0f;
						this.AtkUnmnp.BurstDir(num == 2);
					}
				}
			}
			else
			{
				this.deactivateShieldHitable();
			}
			this.t_actv.Update(num2 * fcnt);
			if (this.hit_t > 0f)
			{
				this.hit_t = X.Mx(this.hit_t - fcnt, 0f);
				this.Qu.run(1f);
			}
		}

		public void deactivateShieldHitable()
		{
			if (this.MhUnmnp.isActive(this.Hitable))
			{
				if (this.Hitable != null)
				{
					this.Hitable.enabled = false;
				}
				this.MhUnmnp.Mg.kill(-1f);
				this.MhUnmnp.release();
			}
		}

		private void PtcVarPre(float hit_x, float hit_y)
		{
			this.PtcVarPre(ref hit_x, ref hit_y);
		}

		private void PtcVarPre(ref float hit_x, ref float hit_y)
		{
			Vector2 visivilityPos = this.getVisivilityPos();
			float num = X.LENGTHXYS(visivilityPos.x, visivilityPos.y, hit_x, hit_y);
			if (num > M2Shield.octa_w * this.Mp.rCLENB)
			{
				float num2 = M2Shield.octa_w * this.Mp.rCLENB / num;
				hit_x = (hit_x - visivilityPos.x) * num2 + visivilityPos.x;
				hit_y = (hit_y - visivilityPos.y) * num2 + visivilityPos.y;
			}
			this.Mv.PtcVar("x", (double)hit_x).PtcVar("y", (double)hit_y).PtcVar("agR", (double)X.GAR(hit_x, hit_y, visivilityPos.x, visivilityPos.y))
				.PtcVar("w", (double)(this.Ocd.w * this.scale))
				.PtcVar("h", (double)(this.Ocd.h * this.scale))
				.PtcVar("cx", (double)visivilityPos.x)
				.PtcVar("cy", (double)visivilityPos.y);
		}

		public M2Shield.RESULT checkShield(AttackInfo Atk, float val, bool from_away = false)
		{
			val *= this.shield_enpower;
			if ((!this.canGuard() && !from_away) || val < 0f || Atk == null)
			{
				return M2Shield.RESULT.NO_SHIELD;
			}
			if (this.FlgShieldLock.Has(Atk))
			{
				return M2Shield.RESULT.GUARD_CONTINUE;
			}
			this.alpha = 1f;
			this.Qu.clear();
			float num = 0f;
			if (Atk is NelAttackInfo)
			{
				num = (Atk as NelAttackInfo).shield_success_nodamage;
			}
			float num2 = ((num > 0f) ? num : ((float)Atk.nodamage_time));
			if (val > 0f)
			{
				if (val > 1f)
				{
					this.pow += X.Mx(10f, val * 2.5f);
				}
				if (this.pow >= this.appearable_time)
				{
					M2Shield.RESULT result = this.breakShield(Atk.hit_x, Atk.hit_y, Atk.attr, from_away);
					this.FlgShieldLock.Add(Atk, X.Mx(40f, num2));
					return result;
				}
				this.Qu.Vib(10f, 22f, 4f, 0);
			}
			else
			{
				this.Qu.Vib(4f, 10f, 0f, 0);
			}
			this.hit_t = 55f;
			this.PtcVarPre(Atk.hit_x, Atk.hit_y);
			this.Mv.PtcST("shield_guard_success", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			this.FlgShieldLock.Add(Atk, X.Mx(10f, num2));
			return M2Shield.RESULT.GUARD;
		}

		public float break_unavailable_maxt
		{
			get
			{
				return this.appearable_time * 0.5f;
			}
		}

		public M2Shield.RESULT breakShield(float hit_x, float hit_y, MGATTR attr, bool from_away)
		{
			float num = -this.break_unavailable_maxt;
			this.resetValue(false);
			this.stt = M2Shield.STATE.BROKEN;
			if (this.fnSwitchActivation != null)
			{
				this.fnSwitchActivation(false);
			}
			this.pow = num;
			this.PtcVarPre(ref hit_x, ref hit_y);
			this.Mv.PtcST("shield_break", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			this.recover_lock_t_ = 0f;
			M2Shield.RESULT result = M2Shield.RESULT.BROKEN;
			if (!from_away)
			{
				result |= M2Shield.RESULT._NEAR_FLAG;
			}
			else if (this.Mv is PR)
			{
				PR pr = this.Mv as PR;
				if (this.isGuardableLgt(false))
				{
					result |= M2Shield.RESULT._NEAR_FLAG;
				}
				pr.applyShieldResult(result, attr, hit_x, hit_y, false);
			}
			return result;
		}

		public void applyDamage(float val)
		{
			this.pow = X.Mn(this.appearable_time + 50f, X.Abs(this.pow) + val) * (float)X.MPF(this.pow >= 0f);
		}

		public bool overdamage
		{
			get
			{
				return this.pow >= this.appearable_time;
			}
		}

		public bool fnMagicLariatUnmnp(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				if (Mg.Ray == null)
				{
					Mg.changeRay(Mg.MGC.makeRay(Mg, 0.3f, (Mg.hittype & MGHIT.EN) == (MGHIT)0, true));
				}
				Mg.Ray.HitLock(30f, null);
				Mg.Ray.hit_target_max = 1;
				Mg.Ray.shape = RAYSHAPE.DAIA;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.REFLECTED;
				Mg.aimagr_calc_s = true;
				Mg.projectile_power = -50;
				Mg.Ray.check_other_hit = EnemySummoner.isActiveBorder();
				Mg.Ray.check_hit_wall = false;
				Mg.MGC.initRayCohitable(Mg.Ray);
				Mg.efpos_s = (Mg.raypos_s = true);
				Mg.Ray.cohitable_allow_berserk = M2Ray.COHIT.BERSERK_N;
				Mg.createDropper(0f, 0f, 0.125f, -1f, -1f);
				Mg.Dro.type |= (DROP_TYPE)384;
			}
			if (Mg.sz > 0f)
			{
				Mg.sz = X.Mx(Mg.sz - fcnt, 0f);
				Mg.wind_apply_s_level = 0.08f;
				Mg.Dro.gravity_scale = 0.125f;
				if (Mg.sa == 1f)
				{
					Mg.Dro.vx = X.VALWALK(Mg.Dro.vx, 0f, 0.018f * fcnt);
					Mg.Dro.vy = X.VALWALK(Mg.Dro.vy, 0f, 0.018f * fcnt);
				}
			}
			else
			{
				Mg.Dro.initJump(false, false, false);
				Mg.Dro.bounce_x_reduce = 0.25f;
				Mg.Dro.bounce_y_reduce = 0f;
				if (Mg.sa == 0f)
				{
					Mg.wind_apply_s_level = 0.15f;
					Mg.Dro.gravity_scale = 0.05f;
				}
				else
				{
					Mg.Dro.vx = (Mg.Dro.vy = 0f);
					Mg.Dro.gravity_scale = 0f;
					Mg.wind_apply_s_level = 0f;
				}
			}
			if (fcnt == 0f)
			{
				return true;
			}
			Mg.Ray.Atk = Mg.Atk0;
			Mg.Dro.size = M2Shield.octa_w * this.scale * this.Mp.rCLEN * 0.5f - 0.25f;
			Mg.Ray.PosMap(Mg.Cen.x, Mg.Cen.y);
			Mg.Ray.RadiusM(Mg.Dro.size);
			if (!Mg.Caster.canHoldMagic(Mg) || Mg.Atk0 == null)
			{
				Mg.explodeManaToRay();
				return false;
			}
			Mg.Ray.clearReflectBuffer();
			if ((Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE) & (HITTYPE)4194336) != HITTYPE.NONE && Mg.reflectV(Mg.Ray, ref Mg.Dro.vx, ref Mg.Dro.vy, 0.15f, 1f, false))
			{
				Mg.reflect_lock_floort = 0f;
				Mg.sz = 45f;
				Mg.Dro.bounce_x_reduce = 0.35f;
				Mg.Dro.bounce_y_reduce = 0.45f;
				Mg.Dro.vx = X.absMn(Mg.Dro.vx, 0.06f);
				Mg.Dro.vy = X.absMn(Mg.Dro.vy + (float)X.MPF(Mg.Dro.vy > 0f) * 0.04f, 0.1f);
				if (Mg.Ray.ReflectAnotherRay != null && (Mg.Ray.ReflectAnotherRay.hittype & ((this.mvhit == MGHIT.EN || 2 != 0) ? HITTYPE.PR : HITTYPE.NONE)) != HITTYPE.NONE)
				{
					this.checkShield(Mg.Atk0, 8f, true);
				}
			}
			return true;
		}

		public bool draw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.alpha == 0f || this.Mv == null || this.Mp == null)
			{
				if (Ed == this.Ed)
				{
					this.Ed = null;
				}
				return false;
			}
			Vector2 visivilityPos = this.getVisivilityPos();
			this.Ocd.agR = X.ANMP((int)Ef.af, 110, 1f) * 6.2831855f;
			if (this.stt == M2Shield.STATE.BROKEN)
			{
				Ef.x = visivilityPos.x;
				Ef.y = visivilityPos.y;
				MeshDrawer mesh = Ef.GetMesh("", MTRX.MtrMeshDashLine, true);
				mesh.Col = mesh.ColGrd.Set(M2Shield.ColMain).Scr(0.5f).mulA(-this.alpha * (0.7f + 0.3f * X.COSI(this.Mp.floort, 33.8f)))
					.C;
				float w = this.Ocd.w;
				float h = this.Ocd.h;
				X.GAR2(0f, 0f, w, h);
				float num = 0.0625f;
				float num2 = 0.07375f;
				float num3 = X.Mx(2f, w * 1.4142135f * 1.18f / 14f);
				float num4 = w * 0.015625f;
				float num5 = h * 0.015625f;
				mesh.TriRectBL(0, 1, 7, 6).TriRectBL(6, 7, 3, 2).TriRectBL(2, 3, 5, 4)
					.TriRectBL(4, 5, 1, 0);
				mesh.uvRectN(0f, 0.35f);
				mesh.Pos(-num4, 0f, null).Pos(-num4 - num, 0f, null);
				mesh.Pos(num4, 0f, null).Pos(num4 + num, 0f, null);
				mesh.uvRectN(num3, 0.35f);
				mesh.Pos(0f, -num5, null).Pos(0f, -num5 - num2, null);
				mesh.Pos(0f, num5, null).Pos(0f, num5 + num2, null);
				float num6 = X.NIL(0.25f, 1f, -this.pow, this.break_unavailable_maxt);
				num3 = X.Mx(h * num6 / 11f, 0.5f);
				mesh.TriRectBL(0, 2, 3, 1).TriRectBL(4, 0, 1, 5);
				mesh.uvRectN(0f, 0.35f);
				mesh.Pos(-num * 0.5f, 0f, null).Pos(num * 0.5f, 0f, null);
				mesh.uvRectN(num3, 0.35f);
				mesh.Pos(-num * 0.5f, num5, null).Pos(num * 0.5f, num5, null);
				mesh.Pos(-num * 0.5f, -num5, null).Pos(num * 0.5f, -num5, null);
				return true;
			}
			return M2Shield.draw(Ef, Ed, this.Ocd, this.bottom_draw_flag, visivilityPos.x, visivilityPos.y, this.scale, this.alpha, this.pow, this.activate_time, this.appearable_time_, this.Mp.M2D.Cam.getScale(true), this.hit_t, this.hit_zagR, this.Qu);
		}

		public static bool draw(EffectItem Ef, M2DrawBinder Ed, OctaHedronDrawer Ocd, bool bottom_draw_flag, float x, float y, float scale, float alpha, float pow, float activate_time, float appearable_time_, float cam_scale, float hit_t, float hit_zagR, Quaker Qu = null)
		{
			Ef.x = x;
			Ef.y = y;
			M2Shield.draw_alpha_s = alpha;
			float num = ((alpha >= 0f) ? X.Mx(pow, 0f) : (-alpha * 15f));
			float num2 = appearable_time_ * 0.95f;
			float num3 = appearable_time_ * 0.7f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = ((num >= num2) ? (0.65f + 0.35f * X.COSIT(20f)) : 0f);
			C32 c = ((num >= num2) ? MTRX.cola.Set(M2Shield.ColMain).blend(M2Shield.ColBreaking, num6) : MTRX.cola.Set(M2Shield.ColMain));
			float num7 = ((num >= num3) ? (X.SINI(num - num3, 25f) * 0.025f * 3.1415927f) : 0f);
			float zagR = Ocd.zagR;
			if (hit_t > 0f)
			{
				Ocd.zagR += hit_zagR * X.SINI(55f - hit_t, 18.8f) * X.ZPOW(hit_t, 20f);
				if (Qu != null)
				{
					num4 = Qu.x * 64f;
					num5 = Qu.y * 64f;
				}
			}
			MeshDrawer mesh = Ef.GetMesh("", c.rgba, BLEND.ADD, bottom_draw_flag);
			mesh.Rotate(num7, false);
			mesh.ColGrd.Set(c).mulA(0f);
			if (alpha < 0f)
			{
				float num8 = (1f - X.ZPOW3(1f + alpha)) / 2f;
				Ocd.agR += 0.08975979f;
				M2Shield.sizeRatio(Ocd, scale * X.NI(1.2f, 1f, -alpha));
				mesh.Col = C32.MulA(c.C, -alpha * (0.5f + 0.25f * X.COSIT(9.82f) + 0.25f * X.COSIT(4.31f)));
				mesh.ColGrd.Set(mesh.Col);
				Ocd.drawSurface(mesh, num4, num5, M2Shield.FD_drawSurfaceDeactivate);
			}
			else if (alpha < 1f)
			{
				float num9 = X.ZPOW3(alpha) / 2f;
				M2Shield.sizeRatio(Ocd, scale * X.NI(1.2f, 1f, alpha));
				Ocd.agR = X.NI(-0.7853982f, 0.7853982f, alpha);
				Ocd.drawLine(mesh, num4, num5, M2Shield.FD_drawLine, 0.5f - num9, 0.5f + num9);
			}
			else
			{
				M2Shield.sizeRatio(Ocd, scale);
				float num10 = alpha - 1f;
				Ocd.agR += 0.036959916f;
				MeshDrawer mesh2 = Ef.GetMesh("_s", uint.MaxValue, BLEND.SUB, bottom_draw_flag);
				if (alpha < 1.2f)
				{
					MeshDrawer meshDrawer = Ef.GetMesh("", M2Shield.MtrSilhouette, bottom_draw_flag).Rotate(num7, false);
					meshDrawer.base_z = (mesh2.base_z + mesh.base_z) * 0.5f;
					Ocd.drawSurface(meshDrawer, num4, num5, M2Shield.FD_drawSurfaceSilhouette);
				}
				else
				{
					M2Shield.MtrBlurBehind.SetFloat("_Scale", cam_scale);
					M2Shield.MtrBlurBehind.SetColor("_Color", C32.MulA(M2Shield.ColSub.C, 0.2f + 0.1f * X.COSIT(9.82f) + 0.1f * X.COSIT(4.31f)));
					MeshDrawer meshDrawer = Ef.GetMesh("", M2Shield.MtrBlurBehind, bottom_draw_flag).Rotate(num7, false);
					meshDrawer.base_z = mesh2.base_z + 0.0001f;
					meshDrawer.ColGrd.Set(c).setA((float)M2Shield.ColMain.a).mulA(0.8f + 0.08f * X.COSIT(8.89f) + 0.08f * X.COSIT(4.67f));
					Ocd.drawSurface(meshDrawer, num4, num5, M2Shield.FD_drawSurfaceBlur);
				}
				C32 colb = MTRX.colb;
				C32 c2 = mesh2.ColGrd.Set(colb).mulA(0f);
				mesh2.Col = colb.Set(M2Shield.ColSub).mulA(1f - X.ZPOW3(num10) * 0.75f + 0.06f * X.COSIT(13f) + 0.06f * X.COSIT(7.88f)).C;
				mesh2.Scale(1f, 1.18f, false).Rotate(num7, false);
				mesh2.BlurPoly2(num4, num5, Ocd.w, 0f, 4, Ocd.w * 4f, Ocd.w * 0.3f, colb, c2);
				mesh2.Identity();
				MTRX.colb.Set(c).mulA(0.8f + 0.1f * X.COSIT(11.52f) + 0.1f * X.COSIT(6.43f));
				bool flag = false;
				if (num >= num2)
				{
					flag = 0.5f + 0.4f * X.COSIT(9.7f) + 0.4f * X.COSIT(5.14f) < 0.45f;
				}
				else if (num >= num3)
				{
					flag = 0.5f + 0.4f * X.COSIT(15.7f) + 0.4f * X.COSIT(21.14f) < 0.2f;
				}
				if (flag)
				{
					C32 cola = MTRX.cola;
					mesh.Col = cola.mulA(0.2f).C;
				}
				Ocd.drawLine(mesh, num4, num5, M2Shield.FD_drawLine, 0f, 1f);
				Ocd.drawPoint(mesh, num4, num5, M2Shield.FD_drawPoint);
			}
			mesh.Identity();
			Ocd.zagR = zagR;
			return true;
		}

		private static void drawLine(OctaHedronDrawer Ocd, MeshDrawer Md, float x0, float y0, float x1, float y1, bool is_front, int to_top)
		{
			Md.BlurLine(x0, y0, x1, y1, (M2Shield.draw_alpha_s < 1f) ? 4f : (14f * (1f - X.ZPOW3(M2Shield.draw_alpha_s - 1f) * 0.6f + 0.1f * X.COSIT(11.7f) + 0.1f * X.COSIT(6.83f))), 0, 20f, false);
		}

		private static void drawPoint(OctaHedronDrawer Ocd, MeshDrawer Md, float x0, float y0, bool is_front)
		{
			Md.BlurPoly2(x0, y0, 2f, 0f, 10, 100f, 10f, MTRX.colb, Md.ColGrd);
		}

		private static void drawSurfaceSilhouette(OctaHedronDrawer Ocd, MeshDrawer Md, float x0, float y0, float x1, float y1, float x2, float y2, float front_level, bool is_under, bool is_front)
		{
			if (!is_front)
			{
				return;
			}
			Md.Tri012();
			Md.PosD(x0, y0, null).PosD(x1, y1, null).PosD(x2, y2, null);
		}

		private static void drawSurfaceBlur(OctaHedronDrawer Ocd, MeshDrawer Md, float x0, float y0, float x1, float y1, float x2, float y2, float front_level, bool is_under, bool is_front)
		{
			if (!is_front)
			{
				return;
			}
			Md.Tri012();
			Md.Col = C32.MulA(Md.ColGrd.C, (is_under ? 0.8f : 1f) * X.NI(0.8f, 0.2f, X.Abs(front_level)));
			Md.PosD(x0, y0, null).PosD(x1, y1, null).PosD(x2, y2, null);
		}

		private static void drawSurfaceDeactivate(OctaHedronDrawer Ocd, MeshDrawer Md, float x0, float y0, float x1, float y1, float x2, float y2, float front_level, bool is_under, bool is_front)
		{
			if (!is_front)
			{
				return;
			}
			Md.Tri012();
			Md.Col = C32.MulA(Md.ColGrd.C, (is_under ? 0.6f : 1f) * X.NI(0.8f, 0.1f, X.Abs(front_level)) * -M2Shield.draw_alpha_s);
			Md.PosD(x0, y0, null).PosD(x1, y1, null).PosD(x2, y2, null);
		}

		public float appearable_time
		{
			get
			{
				return this.appearable_time_;
			}
			set
			{
				this.appearable_time_ = value;
				this.recover_addt = this.appearable_time_ / this.recover_time_;
			}
		}

		public float recover_time
		{
			get
			{
				return this.recover_time_;
			}
			set
			{
				this.recover_time_ = value;
				this.recover_addt = this.appearable_time_ / this.recover_time_;
			}
		}

		public float recover_lock_t
		{
			get
			{
				return this.recover_lock_t_;
			}
			set
			{
				this.recover_lock_t_ = X.Mx(value, this.recover_lock_t_);
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Mv.Mp;
			}
		}

		public void addHedronYRot(float r)
		{
			this.Ocd.agR += 0.036959916f;
		}

		public string snd_key
		{
			get
			{
				if (this.snd_key_ == null)
				{
					this.snd_key_ = this.Mv.snd_key + ".shield";
				}
				return this.snd_key_;
			}
		}

		public bool isGuardableLgt(bool strict = false)
		{
			return X.Abs(this.x + this.shiftx - this.Mv.x) <= 1.25f * this.scale && X.Abs(this.y + this.shifty - this.Mv.y) <= 1.3f * this.scale && (this.stt == M2Shield.STATE.SHOWING || !strict || ((!this.isLariatState() || this.t_lariat_enlarge_power < 30f || X.Abs(this.scale - 1f) <= ((this.shiftx == 0f && this.shifty == 0f) ? 0.15f : 0.04f)) && X.LENGTHXYS(0f, 0f, this.shiftx, this.shifty) < 0.125f));
		}

		public bool isManipulatable()
		{
			return this.isGuardableLgt(false) && this.isManipulatableState();
		}

		public bool isManipulatableState()
		{
			return this.stt != M2Shield.STATE.BROKEN && (this.stt == M2Shield.STATE.SHOWING || (this.isLariatState() && this.t_lariat_enlarge_power < 30f));
		}

		public bool isBroken()
		{
			return this.stt == M2Shield.STATE.BROKEN;
		}

		public M2Shield immediateGuard()
		{
			if (this.activate(false) && this.alpha < 1f)
			{
				this.alpha = 1f;
				if (this.appear_snd != "")
				{
					this.Mp.playSnd(this.appear_snd, "", this.Mv.x, this.Mv.y, 1);
				}
			}
			return this;
		}

		public Vector2 getVisivilityPos()
		{
			float num = (this.isManipulatableState() ? (-this.Mv.getSpShiftY()) : 0f);
			return new Vector2(this.x + this.shiftx, this.y + this.shifty + num * this.Mp.rCLEN);
		}

		public bool isLariatState()
		{
			return this.stt == M2Shield.STATE.LARIAT || this.stt == M2Shield.STATE.LARIAT_HOLD;
		}

		public bool isActive()
		{
			return this.alpha > 0f;
		}

		public bool isVisible()
		{
			return this.alpha != 0f;
		}

		public bool isHolding()
		{
			return this.alpha > 0f || this.activating;
		}

		public bool isVisibleCompletely()
		{
			return this.alpha >= 1f;
		}

		public bool canGuard()
		{
			return this.alpha >= 1f && this.isGuardableLgt(false);
		}

		public readonly M2Attackable Mv;

		public readonly MGHIT mvhit;

		public float shield_enpower = 1f;

		public float alpha;

		private float recover_lock_t_;

		private float pow;

		private float hit_zagR;

		private float hit_t;

		private float appearable_time_ = 260f;

		private float recover_time_ = 200f;

		private float recover_addt;

		public bool activating;

		private float x;

		private float y;

		private float shiftx;

		private float shifty;

		public const float LARIAT_FOLLOWBACK_SPD = 0.06f;

		public const float LARIAT_FOLLOW_SPD = 0.075f;

		public const float BASE_FOLLOW_SPD = 0.25f;

		public const int LARIAT_HITLOCK_T = 30;

		public float broken_recover_TS = 0.3f;

		public const float LARIAT_FOLLOWBACK_SPD_LOW = 0.018f;

		public const float LARIAT_HIT_DAMAGE_TO_SHIELD = 16f;

		public const float LARIAT_REFLECT_DAMAGE_TO_SHIELD = 8f;

		public NelAttackInfo AtkUnmnp;

		private RevCounterLock t_actv;

		private float posrot_agR;

		public float scale = 1f;

		private float follow_speed = -1000f;

		public bool bottom_draw_flag;

		public float activate_time = 20f;

		public float deactivate_anim_time = 20f;

		private const float HIT_MAXT = 55f;

		private OctaHedronDrawer Ocd;

		private M2DrawBinder Ed;

		private Quaker Qu;

		public static C32 ColMain = new C32(2573986807U);

		public static C32 ColBreaking = new C32(2581864565U);

		public static C32 ColSub = new C32(1719452767);

		private FlagCounterR<AttackInfo> FlgShieldLock;

		private M2Shield.STATE stt;

		private float t_lariat_enlarge_power;

		public static Material MtrSilhouette;

		public static Material MtrBlurBehind;

		public const float wh_ratio = 1.18f;

		public float LARIAT_ACTV_MAXT = 30f;

		private MagicItemHandlerS MhUnmnp;

		private M2ShieldHitable Hitable;

		public string activate_snd = "shield_activate";

		public string appear_snd = "shield_appear";

		public string break_snd = "shield_break";

		private static OctaHedronDrawer.FnDrawOctaHedronLine FD_drawLine;

		private static OctaHedronDrawer.FnDrawOctaHedronSurface FD_drawSurfaceSilhouette;

		private static OctaHedronDrawer.FnDrawOctaHedronSurface FD_drawSurfaceBlur;

		private static OctaHedronDrawer.FnDrawOctaHedronSurface FD_drawSurfaceDeactivate;

		private static OctaHedronDrawer.FnDrawOctaHedronPoint FD_drawPoint;

		public M2Shield.FnSwitchActivation fnSwitchActivation;

		public const float LARIAT_INIT_T = 30f;

		public const float LARIAT_MAXT_ENLARGE = 220f;

		public MagicItem.FnMagicRun FD_fnMagicLariatUnmnp;

		private M2DrawBinder.FnEffectBind FD_draw;

		private static float draw_alpha_s;

		private string snd_key_;

		public delegate bool FnSwitchActivation(bool to_active);

		public enum RESULT
		{
			NO_SHIELD,
			GUARD,
			GUARD_CONTINUE,
			BROKEN,
			_BIT_TYPE = 255,
			_NEAR_FLAG
		}

		private enum STATE
		{
			BROKEN = -1,
			SHOWING,
			LARIAT,
			LARIAT_HOLD
		}
	}
}
