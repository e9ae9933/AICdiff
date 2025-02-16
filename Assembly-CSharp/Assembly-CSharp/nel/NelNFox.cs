using System;
using System.Collections.Generic;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNFox : NelEnemy
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			this.PtcFireBallAula = EfParticleManager.Get("fox_fireball_running", false, false);
			base.base_gravity = 0.72f;
			ENEMYID id = this.id;
			this.id = ENEMYID.FOX_0;
			NOD.BasicData basicData = NOD.getBasicData("FOX_0");
			this.SizeW(120f, 80f, ALIGN.CENTER, ALIGNY.MIDDLE);
			base.appear(_Mp, basicData);
			this.Jumper = new NASJumper(this, 0.2f, 8f);
			this.Jumper.fnJumpProgress = new NASJumper.FnListenJumpProgress(this.JumpProg);
			this.Jumper.jumpable_x_len = 13f;
			this.FlgSmall = new Flagger(delegate(FlaggerT<string> V)
			{
				base.FixSizeW(50f, 50f);
			}, delegate(FlaggerT<string> V)
			{
				base.FixSizeW(120f, 80f);
			});
			this.enlarge_maximize_mp_ratio = 0.95f;
			this.ball_consume = (int)((float)this.maxmp * this.enlarge_maximize_mp_ratio / 5f);
			this.AFireBall = new List<MagicItemHandler>(this.ball_consume);
			this.McsBall.consume = this.ball_consume;
			this.McsBall.release = (int)((float)this.ball_consume * 0.14f);
			this.McsBall.neutral_ratio = 0.25f;
			this.foot_bump_effect_enlarge_level = 100f;
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 9f;
			this.Nai.attackable_length_top = -7f;
			this.Nai.attackable_length_bottom = 8f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.FD_MgDrawSplashBall = new MagicItem.FnMagicRun(this.MgDrawSplashBall);
			this.FD_MgRunBreatheBall = new MagicItem.FnMagicRun(this.MgRunBreatheBall);
			this.FD_MgDrawFireBall = new MagicItem.FnMagicRun(this.MgDrawFireBall);
			this.FD_MgDrawBreatheBall = new MagicItem.FnMagicRun(this.MgDrawBreatheBall);
			this.FD_MgRunChaserBall = new MagicItem.FnMagicRun(this.MgRunChaserBall);
			this.FD_MgRunSplashBall = new MagicItem.FnMagicRun(this.MgRunSplashBall);
			this.FD_MgDrawChaserBall = new MagicItem.FnMagicRun(this.MgDrawChaserBall);
			this.FD_MgRunFireBall = new MagicItem.FnMagicRun(this.MgRunFireBall);
			this.auto_rot_on_damage = true;
			this.absorb_weight = 2;
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			NelEnemy.STATE state = this.state;
			base.changeState(st);
			if (state == NelEnemy.STATE.ABSORB)
			{
				this.Anm.timescale = 1f;
				if (this.state == NelEnemy.STATE.STAND)
				{
					this.jumpInit(-3.3f * base.mpf_is_right, 0f, 3f, false);
					this.SpSetPose("jump", -1, null, false);
					this.Nai.delay = 130f;
					if (base.AimPr is PR && !base.AimPr.is_alive && (base.AimPr as PR).EggCon.isLaying())
					{
						this.Nai.delay = 300f;
					}
				}
			}
			if (state == NelEnemy.STATE.STUN || state == NelEnemy.STATE.DAMAGE || state == NelEnemy.STATE.DAMAGE_HUTTOBI)
			{
				this.Anm.rotationR = 0f;
			}
			return this;
		}

		public override void runPost()
		{
			base.runPost();
			int count = this.AFireBall.Count;
			if (count > 0)
			{
				this.fireball_rot_count = 0;
				for (int i = count - 1; i >= 0; i--)
				{
					MagicItemHandler magicItemHandler = this.AFireBall[i];
					if (!magicItemHandler.isActive(this))
					{
						if (magicItemHandler.Mg.reduce_mp > 0)
						{
							base.splitMyMana(magicItemHandler.Mg.reduce_mp, 0f, 0);
							if (this.Nai.isFrontType(NAI.TYPE.GUARD, PROG.ACTIVE))
							{
								this.walk_time = 200f;
								this.Nai.addTypeLock(NAI.TYPE.GUARD, 230f);
							}
						}
						this.AFireBall.RemoveAt(i);
					}
					else
					{
						MagicItem mg = magicItemHandler.Mg;
						if (mg.phase >= 0)
						{
							this.fireball_rot_count++;
							if (mg.dz != 0f && mg.phase < 100)
							{
								if (mg.phase == 0 && i > 0)
								{
									mg.sz = this.AFireBall[0].Mg.sz;
								}
								mg.phase = 1 + i;
							}
						}
					}
				}
			}
			if (this.ball_check_t > 0f && !base.disappearing && !base.hasF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING) && this.canApplyFireBallDamageState())
			{
				this.ball_check_t -= this.TS;
				if (this.ball_check_t <= 0f)
				{
					int num = (int)((float)this.mp / ((float)this.McsBall.consume * base.nM2D.NightCon.SpilitMpRatioEn()));
					if (this.AFireBall.Count < num)
					{
						this.ball_check_t = 20f;
						MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunFireBall, this.FD_MgDrawFireBall);
						this.AFireBall.Add(new MagicItemHandler(magicItem));
						magicItem.MpConsume(this.McsBall, 1f);
					}
					else if (this.AFireBall.Count > num)
					{
						int num2 = this.AFireBall.Count - num;
						int num3 = this.AFireBall.Count - 1;
						while (num3 >= 0 && (this.AFireBall[num3].Mg.phase != -1 || --num2 > 0))
						{
							num3--;
						}
						if (num2 > 0)
						{
							for (int j = this.AFireBall.Count - 1; j >= 0; j--)
							{
								MagicItemHandler magicItemHandler2 = this.AFireBall[j];
								if (magicItemHandler2.Mg.phase != -1)
								{
									magicItemHandler2.Mg.phase = -1;
									if (--num2 <= 0)
									{
										break;
									}
								}
							}
							if (num2 > 0)
							{
								this.ball_check_t = 20f;
							}
						}
						else
						{
							this.ball_check_t = 20f;
						}
					}
				}
			}
			if (this.MgHPrepare != null && !this.MgHPrepare.isActive(this))
			{
				this.MgHPrepare = null;
			}
			if (this.MgHChaserBall != null && !this.MgHChaserBall.isActive(this))
			{
				this.MgHChaserBall = null;
			}
			if (this.fireball_drawn >= 0)
			{
				this.fireball_drawn_string = this.fireball_drawn;
				this.fireball_drawn = -1;
			}
		}

		public bool MgRunFireBall(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (Mg.t <= 0f)
			{
				Mg.t = 0f;
				Mg.Ray.RadiusM(0.25f).HitLock(90f, Mg.MGC.getHitLink(MGKIND.CANDLE_SHOT));
				Mg.Ray.check_other_hit = false;
				Mg.Ray.check_hit_wall = false;
				float num = (Mg.aim_agR = X.XORSPS() * 3.1415927f);
				Mg.raypos_s = (Mg.efpos_s = (Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true)));
				Mg.dx = X.Cos(num);
				Mg.dy = -X.Sin(num);
				Mg.sx = base.x + 2f * Mg.dx;
				Mg.sy = base.y + 1.5f * Mg.dy;
				Mg.dz = 0.02f;
				Mg.dx *= Mg.dz;
				Mg.dy *= Mg.dz;
				Mg.da = 1f;
				Mg.projectile_power = 100;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.Ray.Atk = (Mg.Atk0 = this.AtkBallTouch);
				Mg.PtcST("fox_fireball_appear", PTCThread.StFollow.NO_FOLLOW, false);
				return true;
			}
			if (Mg.phase < 0)
			{
				Mg.reduce_mp = 0;
				Mg.kill((Mg.phase <= -2) ? (-1f) : 0.125f);
				return false;
			}
			bool flag = this.canApplyFireBallDamageState();
			if (Mg.phase >= 1)
			{
				if (Mg.sz <= 0f)
				{
					Vector3 vector;
					if (Mg.phase < 100)
					{
						if (this.ball_agR0_fined_floort < this.Mp.floort)
						{
							this.ball_agR0_fined_floort = this.Mp.floort;
							this.ball_agR0 = (-this.Mp.floort * (1f + (Mg.da - 1f) * 2f) + (float)(this.index * 43)) / (float)(70 + 18 * this.fireball_rot_count) * 6.2831855f;
						}
						float num2 = this.ball_agR0 + (float)Mg.phase / (float)this.fireball_rot_count * 6.2831855f;
						vector = new Vector3(2.65f * X.Cos(num2), 2.65f * X.Sin(num2), 1f);
						vector = this.MxFireBall.MultiplyPoint3x4(vector);
						vector.x += base.x;
						vector.y = base.y - vector.y;
					}
					else if (this.Nai.isFrontType(NAI.TYPE.MAG, PROG.ACTIVE))
					{
						vector = new Vector3(this.chaserball_x + X.NIXP(-0.15f, 0.15f), this.chaserball_y + X.NIXP(-0.15f, 0.15f), 1.5f);
					}
					else if (this.Nai.isFrontType(NAI.TYPE.MAG_0, PROG.ACTIVE) || this.Nai.isFrontType(NAI.TYPE.MAG_2, PROG.ACTIVE))
					{
						vector = new Vector3(this.firebreath_x + X.NIXP(-0.15f, 0.15f), base.y + X.NIXP(-0.15f, 0.15f), 1.5f);
					}
					else
					{
						vector = new Vector3(base.x + X.NIXP(-0.5f, 0.5f), base.y - 0.5f + X.NIXP(-0.3f, 0.3f), 1f);
					}
					Mg.sz = 7f;
					if (Mg.da != vector.z)
					{
						Mg.da = vector.z;
						Mg.Ray.RadiusM(0.25f * Mg.da);
					}
					Mg.dz = X.Mn(0.11f, X.LENGTHXYS(Mg.sx, Mg.sy, vector.x, vector.y) / Mg.sz);
					Mg.aim_agR = this.Mp.GAR(Mg.sx, Mg.sy, vector.x, vector.y);
					Mg.dx = X.Cos(Mg.aim_agR) * Mg.dz;
					Mg.dy = -X.Sin(Mg.aim_agR) * Mg.dz;
				}
				if (Mg.sa <= 0f)
				{
					if (flag)
					{
						this.Mp.PtcN(this.PtcFireBallAula, Mg.sx, Mg.sy, Mg.da * 100f, 3, 0);
					}
					Mg.sa = 2f;
				}
				Mg.sa -= fcnt;
				Mg.sz -= fcnt;
				Mg.calced_aim_pos = true;
			}
			Mg.sx += fcnt * Mg.dx;
			Mg.sy += fcnt * Mg.dy;
			if (Mg.phase < 100)
			{
				if (flag)
				{
					Mg.MnSetRay(Mg.Ray, 0, Mg.aim_agR, Mg.t);
					Mg.Ray.LenM(Mg.dz);
					HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(X.absmin(Mg.dx, 0.11f), 0f), HITTYPE.NONE);
					if ((hittype & (HITTYPE)8454144) != HITTYPE.NONE)
					{
						Mg.kill(0.125f);
						return false;
					}
					if ((hittype & (HITTYPE)4195104) != HITTYPE.NONE)
					{
						Mg.PtcST("fox_fireball_break", PTCThread.StFollow.NO_FOLLOW, false);
						return false;
					}
				}
			}
			else if (this.t >= 240f)
			{
				Mg.phase = 0;
				Mg.dx = 0f;
				Mg.dy = 0f;
				Mg.dz = 0.02f;
			}
			return true;
		}

		public bool canApplyFireBallDamageState()
		{
			return this.state == NelEnemy.STATE.STAND || this.state == NelEnemy.STATE.SUMMONED;
		}

		public bool MgDrawFireBall(MagicItem Mg, float fcnt)
		{
			if (!Mg.Ef.EF.isinCameraPtc(Mg.Ef, -140f, -140f, 140f, 140f, 1f, 0f))
			{
				return true;
			}
			this.fireball_drawn = X.Mx(this.fireball_drawn, 0) + 1;
			float num = 0.25f * X.COSI(Mg.t, 33f) + 0.125f * X.COSI(Mg.t, 12.6f) + 0.5f;
			MeshDrawer meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
			float num2 = 1f;
			if (!this.canApplyFireBallDamageState() && this.state != NelEnemy.STATE.ABSORB)
			{
				num2 = 0.3f;
			}
			meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
			float num3 = (40f + 50f * num) * Mg.da * num2;
			meshDrawer.ColGrd.Set(4286538722U);
			if (num2 < 1f)
			{
				meshDrawer.ColGrd.blend(2892861332U, 1f - num2);
			}
			meshDrawer.Col = meshDrawer.ColGrd.C;
			meshDrawer.Rect(0f, 0f, num3 * 2f, num3 * 2f, false);
			meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
			meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
			meshDrawer.ColGrd.Set(4294946918U).blend(4294913068U, num);
			if (num2 < 1f)
			{
				meshDrawer.ColGrd.blend(2895407461U, X.Scr(1f - num2, 1f - num2));
			}
			meshDrawer.Col = meshDrawer.ColGrd.C;
			num3 = (50f - 20f * num) * Mg.da * num2;
			meshDrawer.Rect(0f, 0f, num3 * 2f, num3 * 2f, false);
			return true;
		}

		public bool MgRunSplashBall(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (Mg.t <= 0f)
			{
				Mg.t = 0f;
				Mg.Ray.RadiusM(0.2f);
				Mg.raypos_s = (Mg.efpos_s = (Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true)));
				Mg.wind_apply_s_level = 1f;
				Mg.sx = this.firebreath_x;
				Mg.sy = base.y;
				Mg.Ray.check_hit_wall = true;
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.Ray.projectile_power = 100;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.Ray.Atk = (Mg.Atk0 = NelNFox.AtkBreathe);
				Mg.phase = 2;
				Mg.calced_aim_pos = true;
				Mg.aim_agR = (Mg.sa = this.walk_time);
				Mg.dz = 0.123f;
				Mg.dx = X.Cos(Mg.aim_agR) * Mg.dz;
				Mg.dy = -X.Sin(Mg.aim_agR) * Mg.dz;
				Mg.PtcVar("hagR", (double)(Mg.aim_agR + 1.5707964f)).PtcST("fox_splashball_shot", PTCThread.StFollow.FOLLOW_S, false);
				Mg.t = 1f;
				Mg.sz = 10f;
			}
			if (Mg.phase == 2)
			{
				if (Mg.sz >= 10f)
				{
					Mg.sz -= 10f;
					Mg.calced_aim_pos = true;
					Mg.aim_agR = this.Mp.GAR(Mg.sx, Mg.sy, this.Nai.target_x, this.Nai.target_y);
					Mg.sa = X.VALWALKANGLER(Mg.sa, Mg.aim_agR, 0.15707962f);
					this.Mp.PtcN(this.PtcFireBallAula, Mg.sx, Mg.sy, 100f, 6, 0);
				}
				Mg.sz += fcnt;
				float num = fcnt * Mg.dz;
				Mg.MnSetRay(Mg.Ray, 0, Mg.sa, 0f);
				Mg.Ray.LenM(num);
				Mg.dx = num * Mg.Ray.Dir.x;
				Mg.dy = -num * Mg.Ray.Dir.y;
				Mg.setRayStartPos(Mg.Ray);
				Mg.sx += Mg.dx;
				Mg.sy += Mg.dy;
				HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
				if ((hittype & (HITTYPE)8458240) != HITTYPE.NONE)
				{
					Mg.kill(0.125f);
					return false;
				}
				if ((hittype & HITTYPE.REFLECTED) != HITTYPE.NONE)
				{
					Mg.reflectAgR(Mg.Ray, ref Mg.sa, 0.25f);
				}
				if ((hittype & (HITTYPE)4195072) != HITTYPE.NONE)
				{
					Mg.dz = 0f;
					Mg.phase = 10;
				}
				else if ((hittype & HITTYPE.WALL) != HITTYPE.NONE)
				{
					Mg.dz = 0f;
					Mg.phase = 10;
				}
				if (Mg.phase == 10)
				{
					Mg.PtcST("fox_splashball_break", PTCThread.StFollow.NO_FOLLOW, false);
					int num2 = 6;
					for (int i = 0; i < num2; i++)
					{
						float num3 = (0.5f + (float)i) / (float)num2 * 6.2831855f + Mg.sa;
						float num4 = X.NIXP(0.07f, 0.11f);
						this.addCandleShot(Mg.sx, Mg.sy, num4 * X.Cos(num3) + Mg.dz * Mg.dx, -num4 * X.Sin(num3) + Mg.dz * Mg.dy, X.NIXP(300f, 400f), 70f);
					}
					return false;
				}
			}
			return true;
		}

		public bool MgDrawSplashBall(MagicItem Mg, float fcnt)
		{
			if (!Mg.Ef.isinCameraPtcCen(80f))
			{
				return true;
			}
			float num = 0.25f * X.COSI(Mg.t, 13.3f) + 0.125f * X.COSI(Mg.t, 4.6f) + 0.5f;
			float num2 = 0.25f * X.COSI(Mg.t, 11.3f) + 0.125f * X.COSI(Mg.t, 3.72f) + 0.5f;
			MeshDrawer meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
			meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
			float num3 = 30f + 60f * num;
			meshDrawer.Col = meshDrawer.ColGrd.Set(4283491071U).C;
			meshDrawer.Rect(0f, 0f, num3 * 1.5f, num3 * 1.5f, false);
			meshDrawer.Rect(0f, 0f, num3 * 2.5f, num3 * 2.5f, false);
			meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
			meshDrawer.initForImg(MTRX.EffCircle128, 0);
			meshDrawer.Col = meshDrawer.ColGrd.Set(4293992480U).blend(4294963273U, num2).C;
			num3 = 30f + 12f * num;
			meshDrawer.Rect(0f, 0f, num3 * 2f, num3 * 2f, false);
			meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
			meshDrawer.Rect(0f, 0f, num3 * 4f, num3 * 4f, false);
			num3 += 9f;
			int num4 = 6;
			float num5 = Mg.t / 78f * 6.2831855f + (float)Mg.id * 0.08f * 3.1415927f;
			float num6 = 1f / (float)num4 * 6.2831855f;
			float num7 = 10f + 6f * num2;
			for (int i = 0; i < num4; i++)
			{
				float num8 = num3 * X.Cos(num5);
				float num9 = num3 * X.Sin(num5);
				meshDrawer.initForImg(MTRX.EffCircle128, 0);
				meshDrawer.Rect(num8, num9, num7, num7, false);
				meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
				meshDrawer.Rect(num8, num9, num7 * 2f, num7 * 2f, false);
				num5 += num6;
			}
			return true;
		}

		public bool MgRunChaserBall(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (Mg.t <= 0f)
			{
				Mg.t = 0f;
				Mg.Ray.RadiusM(0.6f);
				Mg.raypos_s = (Mg.efpos_s = (Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true)));
				Mg.sx = this.chaserball_x;
				Mg.sy = this.chaserball_y;
				Mg.Ray.check_hit_wall = true;
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.Ray.projectile_power = 100;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.Ray.Atk = (Mg.Atk0 = this.AtkChaserBallExplode);
				Mg.PtcST("fox_chaserball_appear", PTCThread.StFollow.FOLLOW_S, false);
			}
			if (Mg.phase == 0)
			{
				Mg.sx = X.MULWALK(Mg.sx, this.chaserball_x, 0.015f);
				Mg.sy = X.MULWALK(Mg.sy, this.chaserball_y, 0.015f);
				if (Mg.t >= 70f)
				{
					Mg.phase = 1;
					Mg.aim_agR = this.Mp.GAR(Mg.sx, Mg.sy, this.Nai.target_x, this.Nai.target_y);
					Mg.dz = 0.31f;
					Mg.createDropper(X.Cos(Mg.aim_agR) * Mg.dz, -X.Sin(Mg.aim_agR) * Mg.dz, 0.90000004f, -1f, -1f);
					Mg.Dro.gravity_scale = 0f;
					Mg.Dro.bounce_x_reduce = 1f;
					Mg.Dro.bounce_y_reduce = 1f;
					Mg.PtcVar("hagR", (double)(Mg.aim_agR + 1.5707964f)).PtcST("fox_chaserball_shot", PTCThread.StFollow.FOLLOW_S, false);
					Mg.t = 1f;
					Mg.sz = 0f;
				}
			}
			if (Mg.phase == 1)
			{
				if (Mg.t >= 55f)
				{
					Mg.sz += this.TS;
					Mg.Dro.vx = X.MULWALKMX(Mg.Dro.vx, 0f, 0.12f, 0.06f);
					Mg.Dro.vy = X.MULWALKMX(Mg.Dro.vy, 0f, 0.12f, 0.06f);
					if (Mg.sz >= 5f)
					{
						Mg.sz = 0f;
						Mg.aim_agR = this.Mp.GAR(Mg.sx, Mg.sy, this.Nai.target_x, this.Nai.target_y);
						if (Mg.t >= 500f || (X.LENGTHXYS(Mg.sx, Mg.sy, this.Nai.target_x, this.Nai.target_y) < 7f && X.XORSP() < 0.25f) || this.Mp.canThroughR(Mg.sx, Mg.sy, X.LENGTHXYS(Mg.sx, Mg.sy, this.Nai.target_x, this.Nai.target_y) * 0.8f, Mg.aim_agR, 0.3f))
						{
							Mg.phase = 2;
							Mg.t = 1f;
							Mg.sa = Mg.aim_agR;
							Mg.dz = 0.09f;
							Mg.Dro.vx = X.Cos(Mg.aim_agR) * Mg.dz;
							Mg.Dro.vy = -X.Sin(Mg.aim_agR) * Mg.dz;
							Mg.Dro.size = -1f;
							Mg.sz = 10f;
						}
					}
				}
				else
				{
					Mg.Dro.vx *= 0.95f;
					Mg.Dro.vy *= 0.95f;
				}
			}
			if (Mg.phase == 2)
			{
				if (Mg.sz >= 10f)
				{
					Mg.sz -= 10f;
					Mg.aim_agR = this.Mp.GAR(Mg.sx, Mg.sy, this.Nai.target_x, this.Nai.target_y);
					Mg.sa = X.VALWALKANGLER(Mg.sa, Mg.aim_agR, 0.11938053f);
					Mg.MnSetRay(Mg.Ray, 0, Mg.sa, 0f);
				}
				Mg.sz += fcnt;
				float num = fcnt * 0.022f * X.ZLINE(Mg.t - 1f, 50f);
				Mg.Ray.LenM(num);
				Mg.Dro.vx = num * Mg.Ray.Dir.x;
				Mg.Dro.vy = -num * Mg.Ray.Dir.y;
				Mg.setRayStartPos(Mg.Ray);
				HITTYPE hittype = Mg.Ray.Cast(false, null, false);
				if ((hittype & (HITTYPE)8458240) != HITTYPE.NONE)
				{
					Mg.kill(0.125f);
					return false;
				}
				if ((hittype & HITTYPE.REFLECTED) != HITTYPE.NONE)
				{
					Mg.reflectV(Mg.Ray, Mg.Dro, 0.4f, 0.25f, true);
					Mg.Dro.vx *= 1.2f;
					Mg.Dro.vy *= 1.2f;
					Mg.phase = 10;
				}
				else if ((hittype & (HITTYPE)4227072) != HITTYPE.NONE)
				{
					Mg.phase = 10;
					int hittedMax = Mg.Ray.getHittedMax();
					float num2 = this.Mp.ux2effectScreenx(this.Mp.map2ux(Mg.sx));
					float num3 = this.Mp.uy2effectScreeny(this.Mp.map2uy(Mg.sy));
					Mg.dz = 0f;
					Mg.Dro.vx = (Mg.Dro.vy = 0f);
					for (int i = 0; i < hittedMax; i++)
					{
						M2Ray.M2RayHittedItem hitted = Mg.Ray.GetHitted(i);
						if (hitted.Mv is PR)
						{
							(hitted.Mv as PR).applyDamage(this.AtkChaserBallTouch, true);
						}
						if (Mg.dz == 0f && (hitted.type & HITTYPE.REFLECT_BROKEN) != HITTYPE.NONE)
						{
							Mg.dz = 1f;
							Mg.aim_agR = X.GAR2(num2, num3, hitted.hit_ux, hitted.hit_uy);
							Mg.Dro.vx = -X.Cos(Mg.aim_agR) * 0.14f;
							Mg.Dro.vy = X.Sin(Mg.aim_agR) * 0.14f;
							break;
						}
					}
				}
				else if ((hittype & HITTYPE.WALL) != HITTYPE.NONE)
				{
					Mg.dz = 0f;
					Mg.Dro.vx = (Mg.Dro.vy = 0f);
					Mg.phase = 10;
				}
			}
			if (Mg.phase == 10)
			{
				Mg.phase = 11;
				Mg.t = 1f;
				Mg.killEffect();
				Mg.PtcVar("len", (double)(5.5f * base.CLENM)).PtcVar("maxt", 105.0).PtcST("fox_chaserball_explode_prepare", PTCThread.StFollow.FOLLOW_S, false);
			}
			if (Mg.phase == 11)
			{
				Mg.Dro.gravity_scale = 0f;
				Mg.Dro.size = 0.6f;
				if (Mg.t >= 106f)
				{
					Mg.phase = 100;
					Mg.t = 1f;
					Mg.dz = 0f;
					Mg.killEffect();
					Mg.PtcVar("len", (double)(5.5f * base.CLENM)).PtcST("fox_chaserball_explode", PTCThread.StFollow.NO_FOLLOW, false);
					Mg.Ray.DirXyM(0f, 0f);
					Mg.Ray.penetrate = true;
					Mg.Ray.HitLock(20f, null);
				}
				else if (Mg.dz >= 0f)
				{
					Mg.Dro.vx *= 0.9f;
					Mg.Dro.vy *= 0.9f;
				}
			}
			if (Mg.phase == 100)
			{
				Mg.setRayStartPos(Mg.Ray);
				Mg.Ray.DirXyM(0f, 0f);
				Mg.Ray.RadiusM(5.5f);
				Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
				int num4 = (int)((Mg.t - 2f) * 7f);
				while (Mg.dz < (float)num4)
				{
					float num5 = X.NI(0.2f, 0.95f, Mg.dz / 56f) * 5.5f;
					float num6 = X.XORSPS() * 3.1415927f;
					float num7 = Mg.sx + num5 * X.Cos(num6);
					float num8 = Mg.sy - num5 * X.Sin(num6);
					if (this.Mp.canStand((int)num7, (int)num8))
					{
						this.addCandleShot(num7, num8, X.NIXP(-0.013f, 0.013f), X.NIXP(0f, 0.015f), X.NIXP(320f, 400f), 0f);
					}
					Mg.dz += 1f;
				}
				if (Mg.t >= 10f)
				{
					return false;
				}
			}
			return true;
		}

		public bool MgDrawChaserBall(MagicItem Mg, float fcnt)
		{
			if (Mg.phase > 11)
			{
				return true;
			}
			if (!((Mg.phase < 11) ? Mg.Ef.isinCameraPtcCen(100f) : Mg.Ef.isinCameraPtcCen(120f + base.CLENM * 5.5f)))
			{
				return true;
			}
			float num = ((Mg.phase == 0) ? X.ZSIN(Mg.t, 60f) : 1f);
			float num2 = 0.35f * X.COSI(Mg.t, 24f) + 0.08f * X.COSI(Mg.t, 9.4f) + 0.5f;
			float num3 = 1f;
			if (Mg.phase == 11)
			{
				num3 = X.ZLINE(Mg.t - 19f, 38f);
			}
			MeshDrawer meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
			meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
			float num4 = (70f + 60f * num2) * num;
			meshDrawer.Col = meshDrawer.ColGrd.Set(4286538722U).blend(4288576250U, num2).C;
			if (Mg.phase == 11)
			{
				meshDrawer.Col = meshDrawer.ColGrd.blend(4288528366U, num3).C;
			}
			meshDrawer.Rect(0f, 0f, num4 * 2f, num4 * 2f, false);
			meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, (Mg.phase == 11) ? BLEND.NORMAL : BLEND.ADD, false);
			meshDrawer.initForImg(MTR.SqEffectFireBall.getImage(X.ANM((int)Mg.t, 3, 4f), 0), 0);
			meshDrawer.Col = meshDrawer.ColGrd.White().blend(4290120790U, X.Mx((num2 - 0.6f) * 2f, 0f)).C;
			if (Mg.phase == 11)
			{
				meshDrawer.Col = meshDrawer.ColGrd.blend(4278190080U, num3).C;
			}
			meshDrawer.RotaGraph(0f, 0f, 1f + 0.25f * num2, (float)(-(float)X.ANM(Mg.phase, 4, 6f)) * 1.5707964f, null, false);
			if (Mg.phase == 11)
			{
				meshDrawer = Mg.Ef.GetMesh("", MTRX.MtrMeshAdd, false);
				num2 = X.ZLINE(Mg.t, 105f);
				meshDrawer.Col = meshDrawer.ColGrd.Set(4293831249U).blend(4293787648U, num2).C;
				int num5 = 210;
				float num6 = num2 * base.CLENM * 5.5f;
				for (int i = 0; i < num5; i++)
				{
					float num7 = Mg.t - 0.5f * (float)i;
					if (num7 > 0f && num7 < 20f)
					{
						uint ran = X.GETRAN2(Mg.id + i * 7, 1 + (i & 7));
						X.ZLINE(num7, 20f);
						float num8 = (1f - X.ZSIN2(num7, 12f)) * num6;
						float num9 = (1f - X.ZPOW3(num7 - 5f, 15f)) * num6;
						float num10 = X.RAN(ran, 1052) * 6.2831855f;
						meshDrawer.Identity().Rotate(num10, false);
						meshDrawer.RectBL(num8, -1f, num9 - num8, 2f, false);
					}
				}
			}
			return true;
		}

		private MagicItem addCandleShot(float x, float y, float vx, float vy, float maxt, float gravity_lock = 0f)
		{
			MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.CANDLE_SHOT, base.mg_hit | MGHIT.IMMEDIATE);
			magicItem.sx = x;
			magicItem.sy = y;
			magicItem.dx = vx;
			magicItem.dy = vy;
			magicItem.da = maxt;
			magicItem.sa = gravity_lock;
			magicItem.t = X.XORSP() * X.Mn(maxt * 0.5f, 40f);
			return magicItem;
		}

		public bool MgRunBreatheBall(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (Mg.t <= 0f)
			{
				Mg.t = 0f;
				Mg.Ray.RadiusM(0.5f).HitLock(13f, Mg.MGC.getHitLink(MGKIND.CANDLE_SHOT));
				Mg.raypos_s = (Mg.efpos_s = (Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true)));
				Mg.sx = this.firebreath_x;
				Mg.sy = base.y;
				Mg.calced_aim_pos = true;
				Mg.aim_agR = this.walk_time + X.NIXP(-0.034f, 0.034f) * 3.1415927f;
				Mg.wind_apply_s_level = 1f;
				float num = X.NIXP(0.14f, 0.17f);
				Mg.dx = num * X.Cos(Mg.aim_agR);
				Mg.dy = -num * X.Sin(Mg.aim_agR);
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.projectile_power = 10;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.Ray.Atk = (Mg.Atk0 = NelNFox.AtkBreathe);
				Mg.input_null_to_other_when_quit = true;
			}
			if (Mg.phase == 0)
			{
				float num2 = 2f - X.ZLINE(Mg.t - 5f, 35f) * 1f - X.ZLINE(Mg.t - 70f, 90f) * 0.5f;
				Mg.calcAimPos(false);
				Mg.MnSetRay(Mg.Ray, 0, Mg.aim_agR, 0f);
				Mg.Ray.LenM(X.LENGTHXYS(0f, 0f, Mg.dx, Mg.dy));
				HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir((float)X.MPF(Mg.dx > 0f)), HITTYPE.NONE);
				int num3 = this.Mp.simulateDropItem(ref Mg.sx, ref Mg.sy, ref Mg.dx, ref Mg.dy, 0.052f / num2, 1f, 2.4f, fcnt * num2, 0.4f, true, true);
				if ((num3 & 8) != 0 || (hittype & HITTYPE.HITTED_WATER) != HITTYPE.NONE)
				{
					Mg.PtcVar("hagR", (double)(Mg.aim_agR + 1.5707964f)).PtcST("fox_candleshot_erased", PTCThread.StFollow.NO_FOLLOW, false);
					return false;
				}
				if ((num3 & 7) != 0)
				{
					Mg.PtcST("fox_breathe_hit", PTCThread.StFollow.NO_FOLLOW, false);
					float num4 = X.NIXP(0.6f, 0.8f);
					float num5 = Mg.sy;
					if (Mg.dy > 0f)
					{
						if ((num3 & 2) != 0)
						{
							Mg.dx *= X.NIXP(0.2f, 0.7f);
							Mg.dy = 0f;
							num5 += 0.125f;
						}
						else
						{
							Mg.dy = -Mg.dy * 0.75f;
							num5 -= 0.125f;
						}
					}
					else
					{
						num5 -= 0.125f;
						Mg.dy -= 0.1f;
					}
					this.addCandleShot(Mg.sx += X.NIXP(-0.3f, 0.3f), num5, Mg.dx * num4, Mg.dy * num4 * 1.15f, X.NIXP(220f, 300f), 0f);
					return false;
				}
			}
			return true;
		}

		public bool MgDrawBreatheBall(MagicItem Mg, float fcnt)
		{
			if (!Mg.Ef.EF.isinCameraPtc(Mg.Ef, -60f, -60f, 60f, 60f, 1f, 0f))
			{
				return true;
			}
			float num = 0.25f * X.COSI(Mg.t, 13.3f) + 0.125f * X.COSI(Mg.t, 4.6f) + 0.5f;
			float num2 = 0.25f * X.COSI(Mg.t, 11.3f) + 0.125f * X.COSI(Mg.t, 3.72f) + 0.5f;
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
			meshImg.initForImg(MTRX.EffBlurCircle245, 0);
			float num3 = 40f + 50f * num;
			meshImg.Col = meshImg.ColGrd.Set(4283491071U).C;
			meshImg.Rect(0f, 0f, num3 * 2f, num3 * 2f, false);
			meshImg.Rect(0f, 0f, num3 * 2.5f, num3 * 2.5f, false);
			MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
			meshImg2.initForImg(MTRX.EffBlurCircle245, 0);
			meshImg2.Col = meshImg2.ColGrd.Set(4293992480U).blend(4294963273U, num2).C;
			num3 = 30f + 15f * num;
			meshImg2.Rect(0f, 0f, num3 * 2f, num3 * 2f, false);
			return true;
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.HasF(NAI.FLAG.ESCAPE, true))
			{
				return Nai.AddTicketB(NAI.TYPE.BACKSTEP, 160, true);
			}
			if (Nai.fnAwakeBasicHead(Nai))
			{
				return true;
			}
			if (!base.isOverDrive() && !Nai.hasPriorityTicket(150, false, false))
			{
				if (NelNFox.DEBUGWALK)
				{
					return Nai.AddTicketB(NAI.TYPE.MAG_0, 150, true);
				}
				if (Nai.target_sxdif < 3f && Nai.target_sydif < 0.55f && !Nai.hasTypeLock(NAI.TYPE.PUNCH_2) && !Nai.isPrTortured() && (Nai.HasF(NAI.FLAG.ATTACKED, false) || (Nai.isPrGaraakiState() && !Nai.isPrAbsorbed()) || (Nai.RANtk(4113) < 0.7f && Nai.isPrShieldOpening(10))))
				{
					float num = Nai.target_xdif * 0.75f * (float)X.MPF(base.x < Nai.target_x);
					float num2 = X.Abs(num);
					if (num2 < 0.9f || base.canGoToSideL((num > 0f) ? AIM.R : AIM.L, num2, -0.1f, false, false, false) > num2 * 0.99f)
					{
						return Nai.AddTicketB(NAI.TYPE.PUNCH_2, 150, true);
					}
				}
				if ((int)((this.enlarge_level - 1f) * 5f) < 2 && Nai.AddTicketSearchAndGetManaWeed(150, 20f, -20f, 20f, 2f, -this.sizey - 0.25f, this.sizey + 0.25f, true) != null)
				{
					return true;
				}
				if (Nai.target_slen < 5f && !Nai.isPrGaraakiState() && Nai.RANtk(4922) < 0.5f)
				{
					return Nai.AddTicketB(NAI.TYPE.BACKSTEP, 150, true);
				}
				float target_slen_n = Nai.target_slen_n;
				if (target_slen_n >= 14f)
				{
					return Nai.AddTicketB(NAI.TYPE.WALK, 150, true);
				}
				bool flag = Nai.RANtk(2411) < 0.6f;
				bool flag2 = false;
				if (Nai.isPrMagicChanting(1f) && (Nai.here_dangerous || this.PosRayDangerous.z >= 1f))
				{
					if (Nai.RANtk(4813) < 0.6f && base.Useable(this.McsBall, 2f, 0f) && this.PosRayDangerous.z == 1f && !Nai.hasTypeLock(NAI.TYPE.GUARD))
					{
						return Nai.AddTicketB(NAI.TYPE.GUARD, 150, true);
					}
					if (Nai.RANtk(1349) < 0.5f && !base.Useable(this.McsBall, 2f, 0f))
					{
						return Nai.AddTicketB(NAI.TYPE.BACKSTEP, 150, true);
					}
					flag = true;
					flag2 = Nai.RANtk(3253) < 0.5f + 0.5f * X.ZLINE((float)(this.AFireBall.Count - 2), 3f);
				}
				if (Nai.RANtk(3711) < (flag2 ? 0.9f : 0.58f) && !Nai.hasTypeLock(NAI.TYPE.MAG) && this.MgHChaserBall == null && this.AFireBall.Count >= 2)
				{
					float chaserball_x = this.chaserball_x;
					float chaserball_y = this.chaserball_y;
					if (flag || this.Mp.canThroughR(chaserball_x, chaserball_y, X.Mn(Nai.target_slen, 5f), this.Mp.GAR(chaserball_x, chaserball_y, Nai.target_x, Nai.target_y - 0.3f), 0.3f))
					{
						return Nai.AddTicketB(NAI.TYPE.MAG, 150, true);
					}
				}
				flag = Nai.RANtk(2411) < (flag2 ? 0.9f : 0.4f) && target_slen_n < 9f;
				if (Nai.RANtk(4392) < 0.45f && !Nai.hasTypeLock(NAI.TYPE.MAG_0) && this.AFireBall.Count >= 2)
				{
					float num3 = this.Mp.GAR(this.firebreath_x, base.y, Nai.target_x, Nai.target_y - 0.3f);
					if (this.isBreathAngleCorrect(num3) && (flag || this.Mp.canThroughR(this.firebreath_x, base.y, X.Mn(Nai.target_slen, 5f), num3, 0.3f)))
					{
						return Nai.AddTicketB(NAI.TYPE.MAG_0, 150, true);
					}
				}
				if (Nai.RANtk(2914) < (flag2 ? 0.9f : 0.68f) && !Nai.hasTypeLock(NAI.TYPE.MAG_2) && this.AFireBall.Count >= 1)
				{
					float num4 = this.Mp.GAR(this.firebreath_x, base.y, Nai.target_x, Nai.target_y - 0.3f);
					if (flag || this.Mp.canThroughR(this.firebreath_x, base.y, X.Mn(Nai.target_slen, 5f), num4, 0.125f))
					{
						return Nai.AddTicketB(NAI.TYPE.MAG_2, 150, true);
					}
				}
				if (Nai.AddTicketSearchAndGetManaWeed(150, 20f, -20f, 20f, 0f, 0f, 0f, true) != null)
				{
					return true;
				}
				if (NelNFox.DEBUGWALK || Nai.RANtk(3151) < 0.75f)
				{
					return Nai.AddTicketB(NAI.TYPE.WALK, 150, true);
				}
			}
			return Nai.fnBasicMove(Nai);
		}

		private bool considerOverDrive(NAI Nai)
		{
			return true;
		}

		public override bool readTicket(NaTicket Tk)
		{
			switch (Tk.type)
			{
			case NAI.TYPE.WALK:
			case NAI.TYPE.WALK_TO_WEED:
			{
				bool flag = Tk.initProgress(this);
				int num = base.walkThroughLift(flag, Tk, 20);
				if (num >= 0)
				{
					return num == 0;
				}
				return this.runJumpingWalk(flag, Tk);
			}
			case NAI.TYPE.PUNCH_2:
				return this.runGrabAttack(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_WEED:
				return this.runPunchWeed(Tk.initProgress(this), Tk);
			case NAI.TYPE.MAG:
				return this.runChaserBall(Tk.initProgress(this), Tk);
			case NAI.TYPE.MAG_0:
				return this.runFireBreath(Tk.initProgress(this), Tk);
			case NAI.TYPE.MAG_2:
				return this.runSmallSplashBall(Tk.initProgress(this), Tk);
			case NAI.TYPE.GUARD:
				return this.runGuard(Tk.initProgress(this), Tk);
			case NAI.TYPE.BACKSTEP:
				return this.runJumpingWalk(Tk.initProgress(this), Tk);
			case NAI.TYPE.GAZE:
			case NAI.TYPE.WAIT:
				return this.runGaze(Tk.initProgress(this), Tk);
			}
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			this.MxFireBall = Matrix4x4.identity;
			this.FlgSmall.Rem("JUMP");
			this.Jumper.quitTicket(Tk);
			base.quitTicket(Tk);
			this.Anm.timescale = 1f;
			this.PosRayDangerous = Vector3.zero;
			if (this.MgHPrepare != null)
			{
				this.MgHPrepare.Mg.kill(-1f);
			}
			if (this.SpPoseIs("run"))
			{
				this.SpSetPose("stand", -1, null, false);
			}
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			this.Phy.walk_xspeed = 0f;
			base.remF((NelEnemy.FLAG)6291520);
		}

		public override void changeRiding(IFootable Fd, FOOTRES footres)
		{
			base.changeRiding(Fd, footres);
			if (footres == FOOTRES.FOOTED)
			{
				this.FlgSmall.Rem("JUMP");
			}
		}

		private bool runJumpingWalk(bool init_flag, NaTicket Tk)
		{
			return this.Jumper.runJumpingWalk(init_flag, Tk, ref this.t, ref this.walk_st, ref this.walk_time);
		}

		public bool JumpProg(NASJumper Jp, NASJumper.JPROG prog)
		{
			switch (prog)
			{
			case NASJumper.JPROG.INIT:
				this.SpSetPose("crouch", -1, null, false);
				break;
			case NASJumper.JPROG.SEARCH_DECIDE:
				this.Ser.Cure(SER.BURST_TIRED);
				break;
			case NASJumper.JPROG.RUN0:
			case NASJumper.JPROG.RUN1:
				if (base.hasFoot())
				{
					this.SpSetPose("run", -1, null, false);
				}
				break;
			case NASJumper.JPROG.JUMP_INIT:
			case NASJumper.JPROG.JUMP_FINISH:
				Jp.jumpEffectBasic();
				this.SpSetPose("jump", -1, null, false);
				this.FlgSmall.Add("JUMP");
				break;
			case NASJumper.JPROG.FINISH:
				this.SpSetPose("stand", -1, null, false);
				break;
			case NASJumper.JPROG.ERROR:
				if (base.hasFoot())
				{
					this.jumpInit((float)X.MPFXP() * X.NIXP(4f, 6f), 0f, X.NIXP(6f, 8f), false);
					this.JumpProg(Jp, NASJumper.JPROG.JUMP_INIT);
				}
				this.Nai.delay = 40f;
				break;
			}
			return true;
		}

		private bool runGaze(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_time = -50f - this.Nai.RANtk(285) * 30f;
			}
			this.walk_time += this.TS;
			if (this.walk_time >= 0f)
			{
				if (this.SpPoseIs("crouch"))
				{
					this.SpSetPose("crouch2stand", -1, null, false);
				}
				return this.walk_time < 20f;
			}
			if (base.hasFoot() && !base.SpPoseIs("crouch", "land"))
			{
				this.SpSetPose("crouch", -1, null, false);
			}
			return true;
		}

		public bool runPunchWeed(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.SpSetPose("crouch", -1, null, false);
				this.t = 0f;
			}
			if (this.t >= 20f && Tk.prog <= PROG.PROG4)
			{
				Tk.prog = PROG.PROG5;
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				base.tackleInit(this.AtkSmallPunch, this.TkiSmallPunch);
				this.jumpInit(base.mpf_is_right * 1.4f, 0f, 0.16f, false);
				this.SpSetPose("small_punch", -1, null, false);
			}
			if (this.t >= 28f)
			{
				this.can_hold_tackle = false;
			}
			if (this.t >= 64f && base.hasFoot())
			{
				base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				return false;
			}
			return true;
		}

		public bool runGuard(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.SpSetPose("crouch", -1, null, false);
				this.t = 0f;
				this.playSndPos("fox_vo_open_2", 1);
				this.walk_time = 0f;
			}
			float num;
			if (this.PosRayDangerous.z != 0f)
			{
				num = this.Mp.GAR(base.x, base.y, this.PosRayDangerous.x, this.PosRayDangerous.y);
			}
			else
			{
				num = this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y);
			}
			if (base.hasFoot() && this.SpPoseIs("stand"))
			{
				this.SpSetPose("crouch", -1, null, false);
			}
			float num2 = X.ZSIN(this.t, 30f) * X.Mn(5f, this.Nai.target_slen * 0.7f);
			bool flag = this.Nai.isPrMagicChanting(0f);
			this.MxFireBall = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, num / 3.1415927f * 180f)) * Matrix4x4.Translate(new Vector3(num2, 0f, 0f)) * Matrix4x4.Scale(new Vector3(0.4f, 2f, 1.6f));
			if (this.walk_time < 60f && (this.Nai.here_dangerous || flag))
			{
				this.walk_time = ((!this.Nai.here_dangerous) ? X.Mx(this.walk_time - this.TS, 0f) : 0f);
			}
			else
			{
				this.walk_time += this.TS;
				if (this.walk_time >= 60f)
				{
					if (this.SpPoseIs("crouch"))
					{
						this.SpSetPose("crouch2stand", -1, null, false);
					}
					Tk.AfterDelay(70f);
					return false;
				}
			}
			return true;
		}

		private bool runSmallSplashBall(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				this.walk_time = X.GAR(this.firebreath_x, base.y, this.Nai.target_x, this.Nai.target_y - 0.5f);
				if (!this.prepareSpecialMoveFireBall(1))
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG_2, 300f);
					return this.errorReserveFireball();
				}
				base.addF((NelEnemy.FLAG)6291520);
				this.SpSetPose("attack", -1, null, false);
				base.PtcST("fox_splashball_charge", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 50, true))
			{
				this.SpSetPose("attack2", 1, null, false);
				this.playSndPos("fox_toiki_1", 1);
			}
			if (Tk.prog <= PROG.PROG0)
			{
				this.progressBreathAngle(ref this.walk_time, this.TS * 0.01f * 3.1415927f, false);
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, 10, true))
			{
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
				MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunSplashBall, this.FD_MgDrawSplashBall);
				this.Phy.addFoc(FOCTYPE.WALK, -base.mpf_is_right * 0.15f, 0f, -1f, 0, 1, 24, -1, 0);
				base.MpConsume(this.McsBall, magicItem, 1f, 1f);
				this.killSpecialMovedFireBall(1);
			}
			if (Tk.prog == PROG.PROG1 && this.t >= 90f)
			{
				this.Nai.addTypeLock(NAI.TYPE.MAG_2, 220f);
				return false;
			}
			return true;
		}

		private bool runFireBreath(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				this.walk_time = this.Mp.GAR(this.firebreath_x, base.y, this.Nai.target_x, this.Nai.target_y - 0.6f);
				if (!this.prepareSpecialMoveFireBall(2))
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG, 300f);
					return this.errorReserveFireball();
				}
				base.addF((NelEnemy.FLAG)6291520);
				this.SpSetPose("attack_breathe", -1, null, false);
				this.playSndPos("fox_vo_breathe", 1);
				base.PtcVar("agR", (double)CAim.get_agR(this.aim, 0f)).PtcST("fox_breathe_init", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 70, true))
			{
				this.SpSetPose("attack_breathe", 1, null, false);
				this.Anm.timescale = 1.5f;
				base.PtcVar("agR", (double)CAim.get_agR(this.aim, 0f)).PtcST("fox_breathe", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
				this.t = 100f;
				this.progressBreathAngle(ref this.walk_time, 0.28274336f, true);
				this.killSpecialMovedFireBall(2);
			}
			if (Tk.prog == PROG.PROG0 && this.t >= 100f)
			{
				MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunBreatheBall, this.FD_MgDrawBreatheBall);
				this.progressBreathAngle(ref this.walk_time, 0.028274333f, true);
				this.t -= 8f;
				int num = 0;
				int num2 = this.walk_st + 1;
				this.walk_st = num2;
				if (Tk.Progress(ref this.t, num, num2 >= 22))
				{
					this.SpSetPose("attack2", -1, null, false);
					this.Anm.timescale = 1f;
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
					base.PtcST("fox_breathe_end", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.Phy.addFoc(FOCTYPE.WALK, -base.mpf_is_right * 0.2f, 0f, -1f, 0, 1, 30, -1, 0);
					base.MpConsume(this.McsBall, magicItem, 2f, 1f);
				}
			}
			if (Tk.prog == PROG.PROG1 && this.t >= 120f)
			{
				this.Nai.addTypeLock(NAI.TYPE.MAG_0, 220f);
				return false;
			}
			return true;
		}

		private void progressBreathAngle(ref float agR, float speedR, bool clip = true)
		{
			agR = X.VALWALKANGLER(agR, this.Mp.GAR(this.firebreath_x, base.y, this.Nai.target_x, this.Nai.target_y - 0.6f), speedR);
			if (clip)
			{
				float num = CAim.get_agR(this.aim, 0f);
				float num2 = X.angledifR(num, agR);
				if (X.Abs(num2) >= 1.1938052f)
				{
					agR = num + (float)X.MPF(num2 > 0f) * 0.38f * 3.1415927f;
				}
			}
		}

		private bool isBreathAngleCorrect(float agR)
		{
			float num = X.angledifR(agR, 3.1415927f);
			float num2 = X.angledifR(agR, 0f);
			return X.Mn(X.Abs(num), X.Abs(num2)) < 0.84823006f;
		}

		private bool runChaserBall(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				float chaserball_x = this.chaserball_x;
				float chaserball_y = this.chaserball_y;
				if (!this.prepareSpecialMoveFireBall(2))
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG, 300f);
					return this.errorReserveFireball();
				}
				base.addF((NelEnemy.FLAG)6291520);
				this.t = 0f;
				this.SpSetPose("growl", -1, null, false);
				this.playSndPos("fox_vo_open_1", 1);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 36, true))
			{
				if (!this.Mp.canThroughR(this.chaserball_x, this.chaserball_y, 0f, 1.5707964f, 0.8f))
				{
					return this.errorReserveFireball();
				}
				MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunChaserBall, this.FD_MgDrawChaserBall);
				this.MgHPrepare = new MagicItemHandler(magicItem);
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.MgHPrepare == null)
				{
					return this.errorReserveFireball();
				}
				if (this.MgHPrepare.Mg.phase >= 1)
				{
					this.killSpecialMovedFireBall(2);
					base.MpConsume(this.McsBall, this.MgHPrepare.Mg, 2f, 1f);
					this.MgHChaserBall = this.MgHPrepare;
					this.MgHPrepare = null;
					this.SpSetPose("small_punch", -1, null, false);
					Tk.after_delay = 80f;
					return false;
				}
			}
			return true;
		}

		private bool prepareSpecialMoveFireBall(int _count)
		{
			int count = this.AFireBall.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				MagicItemHandler magicItemHandler = this.AFireBall[i];
				if (magicItemHandler.isActive(this) && magicItemHandler.Mg.phase >= 0 && magicItemHandler.Mg.phase < 100)
				{
					magicItemHandler.Mg.phase = 100;
					if (++num >= _count)
					{
						return true;
					}
				}
			}
			return num >= _count;
		}

		private void killSpecialMovedFireBall(int _count)
		{
			int count = this.AFireBall.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				MagicItemHandler magicItemHandler = this.AFireBall[i];
				if (magicItemHandler.isActive(this) && magicItemHandler.Mg.phase >= 100)
				{
					magicItemHandler.Mg.phase = -2;
					if (++num >= _count)
					{
						break;
					}
				}
			}
		}

		private bool errorReserveFireball()
		{
			int count = this.AFireBall.Count;
			for (int i = 0; i < count; i++)
			{
				MagicItemHandler magicItemHandler = this.AFireBall[i];
				if (magicItemHandler.isActive(this) && magicItemHandler.Mg.phase >= 100)
				{
					magicItemHandler.Mg.phase = 0;
				}
			}
			if (this.SpPoseIs("growl"))
			{
				this.SpSetPose("land", -1, null, false);
			}
			return false;
		}

		public float chaserball_x
		{
			get
			{
				return base.x + base.scaleX * 33f * this.Mp.rCLENB * base.mpf_is_right;
			}
		}

		public float firebreath_x
		{
			get
			{
				return base.x + base.scaleX * 43f * this.Mp.rCLENB * base.mpf_is_right;
			}
		}

		public float chaserball_y
		{
			get
			{
				return base.y - base.scaleX * 0.75f;
			}
		}

		private bool runGrabAttack(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.MxFireBall = Matrix4x4.Translate(new Vector3(-base.mpf_is_right * 2.2f, 0f, 0f)) * Matrix4x4.Scale(new Vector3(0.23f, 1f, 1f));
				if (this.Nai.HasF(NAI.FLAG.ATTACKED, true))
				{
					this.SpSetPose("crouch", -1, null, false);
					Tk.prog = PROG.PROG0;
				}
				else if (this.Nai.isPrShieldOpening(0))
				{
					this.SpSetPose("crouch", -1, null, false);
				}
				else
				{
					Tk.prog = PROG.PROG4;
				}
				base.addF(NelEnemy.FLAG.HAS_SUPERARMOR);
				this.Nai.RemF(NAI.FLAG.ATTACKED);
			}
			if (Tk.prog == PROG.ACTIVE && (this.t >= 40f || !this.Nai.isPrShieldOpening(0)))
			{
				Tk.Progress(true);
				this.t = 0f;
				this.Nai.AddF(NAI.FLAG.ATTACKED, 250f);
				Tk.prog = PROG.PROG4;
			}
			if (Tk.prog == PROG.PROG0 && (this.t >= 90f || base.AimPr == null || base.AimPr.applyHpDamageRatio(this.AtkAbsorbGrab) > 0f))
			{
				Tk.Progress(true);
				this.t = 0f;
				Tk.prog = PROG.PROG4;
			}
			if (Tk.prog == PROG.PROG4)
			{
				if (this.t <= 0f)
				{
					this.SpSetPose("grab_1", -1, null, false);
					this.playSndPos("fox_vo_smallatk", 1);
				}
				if (Tk.Progress(ref this.t, (int)this.TkiGrab._start_delay, true))
				{
					this.playSndPos("fox_grab_swing", 1);
					this.SpSetPose("grab_2", -1, null, false);
					this.Phy.addFoc(FOCTYPE.WALK, base.mpf_is_right * 0.14f, 0f, -1f, 0, 5, 30, -1, 0);
					base.tackleInit(this.AtkAbsorbGrab, this.TkiGrab);
					base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				}
			}
			if (Tk.prog == PROG.PROG5)
			{
				if (this.t >= this.TkiGrab._hold)
				{
					base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					this.can_hold_tackle = false;
				}
				if (this.t >= this.TkiGrab._after_delay)
				{
					if (!this.Nai.HasF(NAI.FLAG.ATTACKED, false))
					{
						this.Nai.addTypeLock(NAI.TYPE.PUNCH_2, 220f);
					}
					return false;
				}
			}
			return true;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			A.Add("torture_ketsudasi");
			Aattr.Add(MGATTR.FIRE);
			Aattr.Add(MGATTR.ABSORB);
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (!base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			Abm.kirimomi_release = true;
			Abm.release_from_publish_count = true;
			Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 13, 63U);
			this.MxFireBall = Matrix4x4.Translate(new Vector3(-base.mpf_is_right * 2.2f, 0f, 0f)) * Matrix4x4.Scale(new Vector3(0.23f, 1f, 1f));
			Abm.uipicture_fade_key = "torture_ketsudasi";
			Abm.no_ser_burned_effect = true;
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.walk_st = 0;
				this.walk_time = 0f;
				if (this.ep_count >= 1000f)
				{
					this.ep_count = 0f;
				}
				else if (this.ep_count >= 40f)
				{
					this.ep_count = 40f;
				}
				else if (this.ep_count >= 20f)
				{
					this.ep_count = 20f;
				}
				else
				{
					this.ep_count /= 2f;
				}
				this.Anm.timescale = 1f;
				this.Absorb.changeTorturePose("torture_inject_prepare", false, false, -1, -1);
				this.Absorb.setKirimomiReleaseDir((int)this.aim);
			}
			if (!this.SpPoseIs("torture_inject_behind_4"))
			{
				pr.Ser.Add(SER.DO_NOT_LAY_EGG, 20, 99, false);
			}
			if (this.walk_time <= 0f)
			{
				NelAttackInfo nelAttackInfo = this.AtkAbsorbPiston;
				if (this.ep_count >= 1000f)
				{
					if (this.t < 1000f)
					{
						this.walk_time = X.NIXP(20f, 60f);
						if (this.SpPoseIs("torture_inject_behind_4"))
						{
							if ((this.Absorb.emstate_attach & UIPictureBase.EMSTATE.PROG1) == UIPictureBase.EMSTATE.NORMAL)
							{
								this.Absorb.emstate_attach = this.Absorb.emstate_attach | UIPictureBase.EMSTATE.PROG1;
								this.Absorb.uipicture_fade_key = "torture_ketsudasi_finish";
								this.Absorb.breath_key = (pr.is_alive ? "breath_e" : "breath_aft");
							}
							pr.UP.setFade(this.Absorb.uipicture_fade_key, UIPictureBase.EMSTATE.NORMAL, false, false, false);
						}
						else
						{
							base.applyAbsorbDamageTo(pr, nelAttackInfo, false, false, false, 0f, false, null, false);
							if (X.XORSP() < 0.5f)
							{
								pr.publishVaginaSplashPiston((this.aim == AIM.R) ? 3.1415927f : 0f, true);
							}
						}
					}
					else
					{
						this.Absorb.breath_key = null;
						if (pr.EggCon.isLaying())
						{
							if (X.XORSP() < 0.11f + (float)this.walk_st * 0.04f)
							{
								return false;
							}
							this.Anm.randomizeFrame();
							this.walk_time = 120f;
							return true;
						}
						else
						{
							if (X.XORSP() < 0.24f + (float)this.walk_st * 0.11f)
							{
								return false;
							}
							this.ep_count = 0f;
						}
					}
				}
				if (this.ep_count < 1000f)
				{
					if (this.t > 0f)
					{
						if (this.Absorb.emstate_attach != UIPictureBase.EMSTATE.PROG0)
						{
							this.Absorb.emstate_attach = UIPictureBase.EMSTATE.PROG0;
							this.Absorb.uipicture_fade_key = "torture_ketsudasi";
							pr.UP.setFade(this.Absorb.uipicture_fade_key, this.Absorb.emstate_attach, true, true, false);
						}
						this.ep_count += ((pr.ep < 400) ? X.NIXP(0.4f, 0.8f) : X.NIXP(1f, 1.5f));
					}
					int num = 1;
					if (this.ep_count >= 65f)
					{
						this.Anm.timescale = 1f;
						this.Absorb.changeTorturePose("torture_inject_behind_3", false, false, -1, -1);
						this.walk_time = X.NIXP(10f, 60f);
						nelAttackInfo = this.AtkAbsorbPistonFinish;
					}
					else if (this.ep_count >= 40f)
					{
						this.walk_time = X.NIXP(13f, 16f);
						this.Anm.timescale = 1.6f;
						nelAttackInfo = this.AtkAbsorbPiston2;
						if (this.t > 0f && !this.SpPoseIs("torture_inject_behind_2"))
						{
							this.Absorb.changeTorturePose("torture_inject_behind_2", false, false, -1, -1);
						}
						num = 2;
					}
					else if (this.ep_count >= 20f)
					{
						this.walk_time = X.NIXP(22f, 25f);
						if (this.t > 0f && !this.SpPoseIs("torture_inject_behind_2"))
						{
							this.Absorb.changeTorturePose("torture_inject_behind_2", false, false, -1, -1);
						}
						nelAttackInfo = this.AtkAbsorbPiston2;
						num = 2;
					}
					else
					{
						num = 3;
						this.walk_time = X.NIXP(30f, 40f);
						if (this.t > 0f && !this.SpPoseIs("torture_inject_behind"))
						{
							this.Absorb.changeTorturePose("torture_inject_behind", false, false, -1, -1);
						}
					}
					if (this.t <= 0f)
					{
						this.walk_time *= 3.5f;
					}
					else
					{
						this.Anm.animReset(X.xors(num), false);
						Vector3 vector = pr.publishVaginaSplashPiston((this.aim == AIM.R) ? 3.1415927f : 0f, false);
						base.applyAbsorbDamageTo(pr, nelAttackInfo, true, false, false, 0f, false, null, false);
						base.PtcVar("x", (double)pr.x).PtcVar("y", (double)pr.y).PtcVar("hit_x", (double)vector.x)
							.PtcVar("hit_y", (double)vector.y)
							.PtcST("player_absorbed_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						if (this.ep_count >= 65f)
						{
							this.t = 1000f - X.NIXP(480f, 530f);
							this.walk_st++;
							this.ep_count = 1000f;
							PrEggManager.CATEG categ = PrEggManager.CATEG.FOX;
							if (pr.applyEggPlantDamage(0.125f, categ, true, 1000f) > 0)
							{
								this.Nai.AddF(NAI.FLAG.INJECTED, (float)(800 + X.xors(2100)));
							}
						}
						else if (this.ep_count < 40f)
						{
							this.playSndPos("fox_toiki_1", 1);
						}
					}
				}
			}
			this.walk_time -= this.TS;
			return true;
		}

		public override bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (Atk != MDAT.AtkCandleTouch || HitMv == null || Mg != null)
			{
				return base.initPublishAtk(Mg, Atk, hittype, HitMv);
			}
			IM2RayHitAble im2RayHitAble = HitMv.Mv as IM2RayHitAble;
			BDic<IM2RayHitAble, float> hitLink = Mg.MGC.getHitLink(MGKIND.CANDLE_SHOT);
			if (im2RayHitAble == null)
			{
				return true;
			}
			float num;
			if (hitLink.TryGetValue(im2RayHitAble, out num) && num > this.Mp.floort)
			{
				return false;
			}
			hitLink[im2RayHitAble] = this.Mp.floort + 40f;
			return true;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			if (Atk != null && Atk.attr == MGATTR.FIRE)
			{
				base.addF(NelEnemy.FLAG.DMG_EFFECT_SHIELD);
				Atk.hpdmg_current = 0;
			}
			return base.applyDamage(Atk, force);
		}

		public override bool runDamageSmall()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.SpSetPose("damage", 1, null, false);
				base.initJump();
				this.playSndPos("fox_vo_damage", 1);
				float num = this.Phy.calcFocVelocityX(FOCTYPE.KNOCKBACK | FOCTYPE.DAMAGE, true);
				if (num == 0f)
				{
					num = (float)X.MPF(this.Nai.target_x < base.x);
				}
				this.Ser.Add(SER.BURST_TIRED, 90, 99, false);
				this.Nai.AddF(NAI.FLAG.ESCAPE, 180f);
				this.Nai.delay = 0f;
				this.Phy.addFoc(FOCTYPE.DAMAGE, (float)X.MPF(num > 0f) * 0.13f, 0f, -1f, 0, 5, 35, 20, 10);
				this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._RELEASE, 0f, -0.12f, -1f, -1, 1, 0, -1, 0);
				this.ep_count = X.Mx(this.ep_count - 0.5f, 0f);
			}
			base.runDamageSmall();
			if (base.hasFoot())
			{
				this.Anm.rotationR = (this.Anm.rotationR_speed = 0f);
				this.SpSetPose("land", -1, null, false);
				return false;
			}
			return true;
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle && !this.Ser.has(SER.TIRED);
		}

		public override float getEnlargeLevel()
		{
			float mp_ratio = base.mp_ratio;
			return 1f + (float)((int)(X.ZLINE(mp_ratio, this.enlarge_maximize_mp_ratio) * 5f)) / 5f;
		}

		public override void fineEnlargeScale(float r = -1f, bool set_effect = false, bool resize_moveby = false)
		{
			base.fineEnlargeScale(r, set_effect, false);
			this.ball_check_t = 20f;
		}

		public override Vector2 getTargetPos()
		{
			if (this.Nai.isFrontType(NAI.TYPE.MAG_0, PROG.ACTIVE) || this.Nai.isFrontType(NAI.TYPE.MAG_2, PROG.ACTIVE))
			{
				return new Vector2(this.firebreath_x, base.y);
			}
			return new Vector2(base.x + base.mpf_is_right * 0.6f, base.y);
		}

		public override bool readPtcScript(PTCThread rER)
		{
			if (base.destructed || this.PtcHld == null)
			{
				return rER.quitReading();
			}
			string cmd = rER.cmd;
			if (cmd != null && cmd == "%BREATHEANGLE")
			{
				rER.Def("agR", this.walk_time);
				return true;
			}
			return base.readPtcScript(rER);
		}

		protected override void autoTargetRayHitted(M2Ray Ray)
		{
			base.autoTargetRayHitted(Ray);
			Vector2 mapPos = Ray.getMapPos(0f);
			this.PosRayDangerous.Set(mapPos.x, mapPos.y, (float)(((Ray.hittype & HITTYPE.PENETRATE) != HITTYPE.NONE) ? 2 : 1));
		}

		private static FlagCounter<SER> SerDmg30 = new FlagCounter<SER>(4).Add(SER.BURNED, 30f);

		private static FlagCounter<SER> SerDmg40 = new FlagCounter<SER>(4).Add(SER.BURNED, 40f);

		private static FlagCounter<SER> SerDmg90 = new FlagCounter<SER>(4).Add(SER.BURNED, 90f);

		private NelAttackInfo AtkBallTouch = new NelAttackInfo
		{
			hpdmg0 = 32,
			burst_vx = 0.004f,
			knockback_len = 0.6f,
			nodamage_time = 20,
			huttobi_ratio = -100f,
			parryable = true,
			attr = MGATTR.FIRE,
			SerDmg = NelNFox.SerDmg30,
			Beto = BetoInfo.Lava.Pow(30, false)
		}.Torn(0.04f, 0.05f);

		private NelAttackInfo AtkChaserBallExplode = new NelAttackInfo
		{
			hpdmg0 = 58,
			burst_vx = 0.14f,
			burst_vy = -0.01f,
			burst_center = 0.3f,
			huttobi_ratio = 50f,
			knockback_len = 0.6f,
			nodamage_time = 20,
			attr = MGATTR.FIRE,
			Beto = BetoInfo.Lava
		}.Torn(0.15f, 0.25f);

		private NelAttackInfo AtkChaserBallTouch = new NelAttackInfo
		{
			hpdmg0 = 0,
			huttobi_ratio = -100f,
			attr = MGATTR.FIRE,
			SerDmg = NelNFox.SerDmg90,
			Beto = BetoInfo.Lava.Pow(30, false)
		};

		protected NelAttackInfo AtkSmallPunch = new NelAttackInfo
		{
			hpdmg0 = 4,
			split_mpdmg = 5,
			huttobi_ratio = -100f,
			burst_vx = 0.008f,
			knockback_len = 0.5f,
			Beto = BetoInfo.NormalS,
			parryable = true
		};

		private NOD.TackleInfo TkiSmallPunch = NOD.getTackle("fox_small_punch");

		private NOD.MpConsume McsGuard = NOD.getMpConsume("fox_guard");

		protected NelAttackInfo AtkAbsorbGrab = new NelAttackInfo
		{
			hpdmg0 = 4,
			split_mpdmg = 1,
			parryable = true,
			shield_break_ratio = 1000f,
			is_grab_attack = true
		};

		private NOD.TackleInfo TkiGrab = NOD.getTackle("fox_grab");

		private const float fox_plant_val = 0.125f;

		protected static EpAtk EpAbsorb = new EpAtk(7, "fox")
		{
			vagina = 3,
			canal = 9,
			gspot = 2
		};

		protected static EpAtk EpAbsorb2 = new EpAtk(14, "fox")
		{
			vagina = 1,
			canal = 9,
			gspot = 6
		};

		protected static EpAtk EpAbsorbFinish = new EpAtk(90, "fox")
		{
			canal = 6,
			gspot = 7
		};

		protected NelAttackInfo AtkAbsorbPiston = new NelAttackInfo
		{
			split_mpdmg = 7,
			hpdmg0 = 6,
			attr = MGATTR.FIRE,
			Beto = BetoInfo.Normal.Pow(20, false),
			EpDmg = NelNFox.EpAbsorb,
			SerDmg = NelNFox.SerDmg30
		}.Torn(0.025f, 0.06f);

		protected NelAttackInfo AtkAbsorbPiston2 = new NelAttackInfo
		{
			split_mpdmg = 16,
			hpdmg0 = 2,
			attr = MGATTR.FIRE,
			Beto = BetoInfo.Normal.Pow(10, false),
			EpDmg = NelNFox.EpAbsorb2,
			SerDmg = NelNFox.SerDmg30
		}.Torn(0.025f, 0.06f);

		protected NelAttackInfo AtkAbsorbPistonFinish = new NelAttackInfo
		{
			split_mpdmg = 22,
			hpdmg0 = 0,
			attr = MGATTR.FIRE,
			Beto = BetoInfo.Sperm2,
			EpDmg = NelNFox.EpAbsorbFinish,
			SerDmg = NelNFox.SerDmg90
		}.Torn(0.025f, 0.06f);

		private const int chaserball_candle_exist_min = 320;

		private const int chaserball_candle_exist_max = 400;

		protected static NelAttackInfo AtkBreathe = new NelAttackInfo
		{
			hpdmg0 = 28,
			burst_vx = 0.13f,
			nodamage_time = 10,
			huttobi_ratio = 0.2f,
			attr = MGATTR.FIRE,
			SerDmg = NelNFox.SerDmg40,
			Beto = BetoInfo.Lava.Pow(32, false)
		}.Torn(0.04f, 0.08f);

		private const int splash_ball_split_max = 6;

		private const int ball_count = 5;

		public int ball_consume;

		private const float chaserball_explode_len = 5.5f;

		private const int chaserball_explode_prepare_time = 105;

		private const float fireball_rotate_len = 2.65f;

		private float ball_check_t = 20f;

		private float ball_agR0_fined_floort;

		private float ball_agR0;

		private Flagger FlgSmall;

		private float ep_count;

		private const float EP_COUNT_1 = 20f;

		private const float EP_COUNT_2 = 40f;

		private const float EP_COUNT_ORGASM = 65f;

		private const float EP_COUNT_ORGASM_AFTER = 1000f;

		private List<MagicItemHandler> AFireBall;

		private NOD.MpConsume McsBall = new NOD.MpConsume();

		private EfParticle PtcFireBallAula;

		private const float size_default_w = 120f;

		private const float size_default_h = 80f;

		private const float size_sml_w = 50f;

		private const float size_sml_h = 50f;

		private Vector3 PosRayDangerous;

		private Matrix4x4 MxFireBall = Matrix4x4.identity;

		private const int mcs_chaserball_count = 2;

		private const int mcs_firebreath_count = 2;

		private const int mcs_splash_count = 1;

		private int fireball_rot_count;

		private const int damage_tired_lock = 90;

		private MagicItemHandler MgHChaserBall;

		private MagicItemHandler MgHPrepare;

		private int fireball_drawn = -1;

		private int fireball_drawn_string;

		private const int candle_touch_hitlock_t = 40;

		private NASJumper Jumper;

		private MagicItem.FnMagicRun FD_MgRunFireBall;

		private MagicItem.FnMagicRun FD_MgDrawFireBall;

		private MagicItem.FnMagicRun FD_MgRunSplashBall;

		private MagicItem.FnMagicRun FD_MgDrawSplashBall;

		private MagicItem.FnMagicRun FD_MgRunChaserBall;

		private MagicItem.FnMagicRun FD_MgDrawChaserBall;

		private MagicItem.FnMagicRun FD_MgRunBreatheBall;

		private MagicItem.FnMagicRun FD_MgDrawBreatheBall;

		private const int PRI_WALK = 150;

		public static readonly bool DEBUGWALK = false;
	}
}
