using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MgWhiteArrow : MgFDHolder
	{
		public override MagicNotifiear GetNotifiear()
		{
			return new MagicNotifiear(4).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				x = 0f,
				y = -2.5f,
				maxt = 39f,
				thick = 0f,
				wall_hit = false,
				penetrate = true,
				other_hit = true,
				auto_target = true,
				fnManipulateMagic = new MagicNotifiear.FnManipulateMagic(this.fnManipulateWhiteArrow),
				fnManipulateTargetting = new MagicNotifiear.FnManipulateTargetting(this.fnManipulateTargettingWhiteArrow)
			}).AddHit(new MagicNotifiear.MnHit
			{
				cast_on_autotarget = true,
				type = MagicNotifiear.HIT.CIRCLE,
				maxt = 22f,
				thick = 0.2f,
				v0 = 0.47f,
				auto_target_fixable = true,
				penetrate = false,
				other_hit = true
			}).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				no_draw = true,
				maxt = 60f,
				other_hit = true,
				penetrate = true,
				auto_target = true,
				thick = 0f,
				wall_hit = false
			})
				.AddHit(new MagicNotifiear.MnHit
				{
					maxt = 30f,
					no_draw = true,
					other_hit = true,
					v0 = 0.34f,
					thick = 0f,
					penetrate = false
				});
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			float num = -28f / Mg.CLENB;
			float num2 = 94f / Mg.CLENB + num;
			if (Mg.phase == 0)
			{
				Mg.target_s = (Mg.raypos_c = (Mg.efpos_s = true));
				Mg.calcAimPos(true);
				Mg.sa = Mg.aim_agR;
				Mg.dx = Mg.sx;
				Mg.dy = Mg.sy;
				Mg.sz = 0f;
				Mg.da = 20f;
				Mg.dz = Mg.Mn._0.maxt / X.Mx(1f, Mg.Caster.getCastingTimeScale(Mg));
				Mg.phase = 1;
				Mg.changeRay(Mg.Mn.makeRayForMagic(Mg, 0));
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.PtcST("whitearrow_appear", PTCThread.StFollow.NO_FOLLOW, false);
			}
			if (Mg.phase == 1)
			{
				float num3 = Mg.dz;
				float num4 = X.ZSIN2(Mg.t, num3 * 0.8f);
				Mg.sx = (Mg.dx = Mg.Cen.x);
				Mg.sy = (Mg.dy = Mg.Cen.y + Mg.Mn._0.y * num4);
				if (Mg.Caster != null && Mg.Caster.canHoldMagic(Mg))
				{
					Mg.calced_aim_pos = false;
					Mg.calcAimPos(false);
					Mg.sa = X.VALWALKANGLER(Mg.sa, Mg.aim_agR, 0.08726647f);
				}
				else
				{
					Mg.calced_aim_pos = true;
				}
				Mg.sz = X.ZSIN2(Mg.t - num3 * 0.5f, num3 * 0.3f) * 0.8f + X.ZSIN(Mg.t - num3 * 0.7f, num3 * 0.3f) * 0.2f;
				float num5 = -40f / Mg.CLENB * Mg.sz;
				Mg.dx += (num2 + num5) * X.Cos(Mg.sa);
				Mg.dy -= (num2 + num5) * X.Sin(Mg.sa);
				Mg.MnSetRay(Mg.Ray, 0, Mg.sa, Mg.t);
				Mg.da += fcnt;
				if (Mg.da >= 10f)
				{
					Mg.PtcST("whitearrow_preparing", PTCThread.StFollow.NO_FOLLOW, false);
					Mg.da -= 10f;
				}
				if (Mg.t >= num3)
				{
					Mg.killEffect();
					Mg.PtcST("whitearrow_shot_first", PTCThread.StFollow.NO_FOLLOW, false);
					Mg.phase = 2;
					Mg.t = 0f;
					Mg.Mn._0.maxt = (Mg.Mn._0.x = (Mg.Mn._0.y = 0f));
					Mg.target_s = (Mg.raypos_c = (Mg.efpos_s = false));
					Mg.aimagr_calc_s = (Mg.efpos_s = (Mg.raypos_d = (Mg.target_s = true)));
					Mg.explode_pos_c = false;
					Mg.raypos_c = false;
					Mg.da = 0f;
					Mg.wind_apply_s_level = 1.35f;
					if (!Mg.exploded)
					{
						Mg.explode(false);
					}
				}
			}
			else if (Mg.phase == 2 || Mg.phase >= 0)
			{
				float num3 = Mg.Mn._1.maxt;
				if (Mg.Ray.reflected)
				{
					Mg.reflectAgR(Mg.Ray, ref Mg.sa, 0.25f);
				}
				Mg.MnSetRay(Mg.Ray, 1, Mg.sa, Mg.t);
				Mg.Ray.check_other_hit = true;
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(0.36f * Mg.mpf_is_right, -0.21f), HITTYPE.NONE);
				if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
				{
					return Mg.kill(Mg.Ray.lenmp);
				}
				if ((hittype & HITTYPE.WALL_AND_BREAK) != HITTYPE.NONE)
				{
					Mg.wind_apply_s_level = 0f;
					if ((hittype & HITTYPE.HITTED_WATER) != HITTYPE.NONE)
					{
						Mg.PtcVar("vx", (double)X.Cos(Mg.sa)).PtcVar("vy", (double)(-(double)X.Sin(Mg.sa))).PtcST("magic_hit_to_water", PTCThread.StFollow.NO_FOLLOW, false);
					}
					if ((hittype & HITTYPE.PR_AND_EN) != HITTYPE.NONE)
					{
						int hittedMax = Mg.Ray.getHittedMax();
						for (int i = 0; i < hittedMax; i++)
						{
							M2Mover mv = Mg.Ray.GetHitted(i).Mv;
							if (mv != null && mv is M2MagicCaster)
							{
								Mg.phase = -2;
								Mg.Caster = mv as M2MagicCaster;
								break;
							}
						}
					}
					Mg.Mn = null;
					if (Mg.phase == -2)
					{
						Mg.flags = 0;
						Mg.exploded = true;
						Mg.efpos_s = true;
						Mg.PtcST("whitearrow_hit_mv", PTCThread.StFollow.NO_FOLLOW, false);
						Mg.da = Mg.sa;
						Mg.sz = Mg.Caster.getPoseAngleRForCaster();
						Vector2 vector = (Mg.Cen = Mg.Caster.getCenter());
						Mg.dx -= vector.x;
						Mg.dy = -(Mg.dy - vector.y);
					}
					else
					{
						Mg.flags = 0;
						Mg.exploded = true;
						Mg.efpos_d = true;
						Mg.PtcST("whitearrow_hit_wall", PTCThread.StFollow.NO_FOLLOW, false);
						Mg.phase = -1;
					}
					Mg.t = 0f;
					return true;
				}
				Mg.da += fcnt;
				if (Mg.da >= 2f)
				{
					Mg.PtcST("whitearrow_arrow_float", PTCThread.StFollow.NO_FOLLOW, false);
					Mg.da = 0f;
				}
				float sx = Mg.sx;
				float sy = Mg.sy;
				Mg.Mn.RayShift(Mg.Ray, 1, ref Mg.sx, ref Mg.sy, fcnt);
				Mg.dx = Mg.sx + num2 * X.Cos(Mg.sa);
				Mg.dy = Mg.sy - num2 * X.Sin(Mg.sa);
				if (Mg.t >= num3)
				{
					Mg.phase = 3;
					Mg.aimagr_calc_d = true;
					Mg.t = 0f;
					Mg.Mn._1.maxt = 0f;
				}
			}
			else if (Mg.phase < 0)
			{
				return MgWhiteArrow.runWhiteArrowHitted(Mg, fcnt);
			}
			return true;
		}

		public static bool runWhiteArrowHitted(MagicItem Mg, float fcnt)
		{
			float num = -28f / Mg.CLENB;
			float num2 = 94f / Mg.CLENB + num;
			if (Mg.phase == -2)
			{
				M2Attackable m2Attackable = Mg.Caster as M2Attackable;
				if (m2Attackable == null || !m2Attackable.is_alive)
				{
					Mg.phase = -100;
					Mg.kill(-1f);
					return false;
				}
			}
			if (Mg.phase == -2)
			{
				float num3 = Mg.Caster.getPoseAngleRForCaster() - Mg.sz;
				Mg.sa = Mg.da + num3;
				Vector2 vector = X.ROTV2e(new Vector2(Mg.dx, Mg.dy), num3);
				float num4 = X.ZSIN(Mg.t, 10f) * 0.78f;
				Mg.sx = Mg.Cen.x + X.Cos(Mg.sa) * num4 + vector.x + num2 * X.Cos(Mg.sa);
				Mg.sy = Mg.Cen.y - X.Sin(Mg.sa) * num4 - vector.y - num2 * X.Sin(Mg.sa);
			}
			else
			{
				float num5 = X.NI(0.24f, 0f, X.ZSINV(Mg.t, 10f));
				if (Mg.t <= 10f)
				{
					Mg.dx += num5 * X.Cos(Mg.sa);
					Mg.dy -= num5 * X.Sin(Mg.sa);
				}
			}
			if (Mg.t >= 60f || Mg.phase == -100)
			{
				Mg.kill(-1f);
				return false;
			}
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			float num = 94f / Mg.CLENB;
			if (Mg.phase == 0)
			{
				return true;
			}
			if (Mg.phase <= -1)
			{
				return MgWhiteArrow.drawWhiteArrowHitted(Mg, fcnt);
			}
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTR.MIiconL, BLEND.ADD, false);
			MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("", MTR.MIiconL, BLEND.SUB, false);
			MeshDrawer mesh = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
			if (Mg.phase == 1)
			{
				float num2 = X.ZLINE(Mg.t, Mg.dz);
				meshImg.Scale(0f + X.ZPOW(num2, 0.28f) * 0.7f + X.ZSIN(num2 - 0.27f, 0.2f) * 0.7f - X.ZCOS(num2 - 0.45f, 0.55f) * 0.4f, 0f + X.ZSIN2(num2, 0.15f) * 1.3f - X.ZSIN(num2 - 0.12f, 0.37f) * 0.8f + X.ZPOW(num2 - 0.4f, 0.6f) * 0.4f, false);
				float num3 = X.NI(1.4f, 1f, X.ZSIN2(num2));
				Matrix4x4 currentMatrix = meshImg.getCurrentMatrix();
				meshImg2.setCurrentMatrix(currentMatrix, false).Scale(num3, num3, false);
				EffectItemNel.drawWhiteArrowBow(meshImg, meshImg2, Mg.Ef, 0f, 0f, Mg.sa, -1f + num2, false);
				mesh.setCurrentMatrix(currentMatrix, false);
				mesh.Rotate(Mg.sa, false);
				if (Mg.sz == 0f)
				{
					mesh.Line(-28f, -100f, -28f, 100f, 1f, false, 0f, 0f);
				}
				else
				{
					float num4 = -40f * Mg.sz;
					mesh.Line(-28f, -100f, -28f + num4, 0f, 1f, false, 0f, 0f).Line(-28f, 100f, -28f + num4, 0f, 1f, false, 0f, 0f);
				}
				meshImg.setCurrentMatrix(currentMatrix, false);
				meshImg2.setCurrentMatrix(currentMatrix, false).Scale(num3, num3, false);
				EffectItemNel.drawWhiteArrowArrow(meshImg, meshImg2, Mg.Ef, (Mg.dx - Mg.sx) * Mg.CLENB, -(Mg.dy - Mg.sy) * Mg.CLENB, Mg.sa, -1f + num2 * 0.99f, false, 0f);
				meshImg.Identity();
				meshImg2.Identity();
			}
			else
			{
				float num5 = ((Mg.phase == 2 || Mg.phase == 4) ? X.ZSIN2(Mg.t, 12f) : 0f);
				EffectItemNel.drawWhiteArrowArrow(meshImg, meshImg2, Mg.Ef, 0f, 0f, Mg.sa, 0f, false, num5);
			}
			return true;
		}

		public static bool drawWhiteArrowHitted(MagicItem Mg, float fcnt)
		{
			EffectItemNel.drawWhiteArrowArrow(null, null, Mg.Ef, 0f, 0f, Mg.sa + 0.37699112f * (1f - X.ZSIN3(Mg.t, 22f)) * X.SINI(Mg.t, 4.78f), 1f, false, 0f);
			return true;
		}

		public bool fnManipulateWhiteArrow(MagicItem Mg, M2MagicCaster _Mv, float fcnt)
		{
			return Mg.phase >= 0 && _Mv != null && _Mv == Mg.Caster && !base.isWrongMagic(Mg, _Mv) && (Mg.isPreparingCircle || Mg.phase <= 1);
		}

		public MagicNotifiear.TARGETTING fnManipulateTargettingWhiteArrow(MagicItem Mg, PR Pr, ref int dx, ref int dy, bool is_first)
		{
			if (Mg.isPreparingCircle || Mg.phase <= 1)
			{
				return (MagicNotifiear.TARGETTING)511;
			}
			return (MagicNotifiear.TARGETTING)0;
		}

		public const float whitearrow_bow_shift = -28f;

		public const float whitearrow_bow_pull_shift = -40f;
	}
}
