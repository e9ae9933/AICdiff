using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MgThunderbolt : MgFDHolder
	{
		public override MagicNotifiear GetNotifiear()
		{
			return new MagicNotifiear(2).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				maxt = 80f,
				thick = 0.3f,
				accel = -0.003f,
				v0 = 0.15f,
				accel_maxt = 50f,
				penetrate = false,
				other_hit = true,
				cast_on_autotarget = true,
				auto_target = true,
				auto_target_fixable = false,
				fnManipulateMagic = new MagicNotifiear.FnManipulateMagic(this.fnManipulateThunderBolt),
				fnFixTargetMoverPos = new MagicNotifiear.FnManipulateTargetMoverPos(this.fnFixTargetMoverPosThunderBolt)
			}).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				thick = 1.6f,
				maxt = 1f,
				penetrate = true,
				other_hit = false,
				v0 = 12f,
				accel_maxt = 28f,
				do_not_consider_magic_t = true,
				draw_thick_kadomaru = true
			});
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.aimagr_calc_s = true;
				Mg.raypos_s = true;
				Mg.calcAimPos(false);
				float num = Mg.Mn._0.ReachableLen(Mg.Mn._0.accel_maxt);
				Mg.sa = Mg.aim_agR;
				Mg.dx = Mg.sx + X.Cos(Mg.aim_agR) * num;
				Mg.dy = Mg.sy - X.Sin(Mg.aim_agR) * num;
				Mg.aimagr_calc_s = false;
				Mg.efpos_s = (Mg.aimagr_calc_d = true);
				Mg.calcAimPos(false);
				Mg.da = Mg.aim_agR;
				Mg.changeRay(Mg.Mn.makeRayForMagic(Mg, 0));
				Mg.Ray.HitLock(60f, null);
				Mg.Ray.hittype |= HITTYPE.WALL;
				Mg.Mn._0.aim_fixed = true;
				Mg.MnSetRay(Mg.Ray, 0, Mg.sa, 0f);
				if (fcnt > 0f)
				{
					Mg.sz = Mg.Mn._0.maxt;
					Mg.PtcVar("sx", (double)Mg.sx).PtcVar("sy", (double)Mg.sy).PtcVar("time", (double)Mg.sz)
						.PtcST("magic_thunderbolt_init", PTCThread.StFollow.FOLLOW_S, false);
					Mg.phase = 2;
					Mg.t = 0f;
					Mg.Mn._0.maxt = 0f;
				}
			}
			if (Mg.phase == 2)
			{
				float sz = Mg.sz;
				Mg.calcAimPos(false);
				if (Mg.Ray.reflected && Mg.already_reflected)
				{
					Mg.reflectAgR(Mg.Ray, ref Mg.sa, 0.25f);
				}
				if (!Mg.already_reflected)
				{
					if (Mg.t <= 6f)
					{
						Mg.da = Mg.aim_agR;
					}
					else
					{
						Mg.Ray.hittype |= HITTYPE.PR | HITTYPE.EN | HITTYPE.BERSERK_MYSELF;
						Mg.da = X.VALWALKANGLER(Mg.da, Mg.aim_agR, X.NI(0.023f, 0.0008f, X.ZLINE(Mg.t - 6f, 50f)) * 6.2831855f);
					}
				}
				else
				{
					Mg.da = Mg.sa;
				}
				if (Mg.t <= Mg.Mn._0.accel_maxt)
				{
					Mg.sx += Mg.Mn._0.Spd(Mg.t) * fcnt * X.Cos(Mg.sa);
					Mg.sy += -Mg.Mn._0.Spd(Mg.t) * fcnt * X.Sin(Mg.sa);
				}
				else if (!Mg.already_reflected)
				{
					Mg.sx = X.MULWALKF(Mg.sx, Mg.dx, 0.04f, fcnt);
					Mg.sy = X.MULWALKF(Mg.sy, Mg.dy, 0.04f, fcnt);
				}
				if (Mg.t >= sz)
				{
					Mg.phase = 100;
				}
				else if (fcnt > 0f && Mg.t >= 3f)
				{
					Mg.raypos_s = true;
					Mg.raypos_d = false;
					Mg.MnSetRay(Mg.Ray, 0, Mg.sa, Mg.t);
					Mg.Mn._1.agR = Mg.da;
					Mg.Mn._1.aim_fixed = true;
					HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir((float)((X.Cos(Mg.da) > 0f) ? 1 : (-1))), HITTYPE.NONE);
					if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
					{
						return Mg.kill(0f);
					}
					if ((hittype & HITTYPE.WALL_AND_BREAK) != HITTYPE.NONE)
					{
						Mg.PtcST("magic_thunderbolt_explode_s", PTCThread.StFollow.NO_FOLLOW, false);
						return false;
					}
					Mg.raypos_d = true;
					Mg.raypos_s = false;
					Mg.Ray.PosMap(Mg.dx, Mg.dy);
				}
			}
			if (Mg.phase == 100)
			{
				Mg.MnSetRay(Mg.Ray, 1, Mg.da, 0f);
				Mg.Mn._1.aim_fixed = true;
				Mg.sz = Mg.Mn._1.accel_maxt;
				Mg.killEffect();
				Mg.phase = 101;
				Mg.t = 0f;
				Mg.Ray.clearHittedTarget();
				if (!Mg.exploded)
				{
					Mg.explode(false);
				}
				Mg.PtcVar("height", (double)Mg.Mn._1.v0).PtcVar("heightb", (double)(Mg.Mn._1.v0 * Mg.CLENB)).PtcVar("agR", (double)Mg.da)
					.PtcVar("maxt", (double)Mg.sz)
					.PtcST("magic_thunderbolt_explode", PTCThread.StFollow.NO_FOLLOW, false);
				PostEffect.IT.setPEbounce(POSTM.SHOTGUN, 10f, 0.35f, 21);
				PostEffect.IT.setPEbounce(POSTM.FINAL_ALPHA, 30f, 0.5f, 11);
			}
			else if (Mg.phase == 101)
			{
				Mg.Ray.check_other_hit = false;
				if ((Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk1.BurstDir((float)((X.Cos(Mg.da) > 0f) ? 1 : (-1))), HITTYPE.NONE) & HITTYPE.KILLED) != HITTYPE.NONE)
				{
					return Mg.kill(0f);
				}
				return Mg.t <= Mg.sz;
			}
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 2)
			{
				float num = X.ZPOW(Mg.t / Mg.sz - 0.5f, 0.5f);
				float num2 = 23f * (1f - num * 0.95f);
				MeshDrawer meshDrawer = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.SUB, false);
				float num3 = 10f * (1f - num * 0.95f);
				float num4 = 40f * (0.375f + num);
				for (int i = 0; i < 3; i++)
				{
					float num5 = num2 * (1f - (float)(i - 1) * 0.08f) * 0.015625f;
					int num6 = 9 - i;
					float num7 = 6.2831855f / (float)num6;
					if (i == 0)
					{
						meshDrawer.Col = meshDrawer.ColGrd.Set(1735804147).blend(1742544250U, 0.5f + 0.5f * X.COSI(Mg.t + 33f, 68f)).C;
					}
					else if (i == 1)
					{
						meshDrawer = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
						meshDrawer.Col = meshDrawer.ColGrd.Set(2705522067U).blend(2704576256U, 0.5f + 0.5f * X.COSI(Mg.t + 63f, 52f)).C;
					}
					else
					{
						meshDrawer.Col = meshDrawer.ColGrd.Set(2714630257U).blend(2701238395U, 0.5f + 0.5f * X.COSI(Mg.t + 13f, 38f)).C;
					}
					uint num8 = X.GETRAN2(i * 13 + Mg.id, 0);
					float num9 = (X.RAN(num8, 2552) + Mg.t * (1f / (float)((i == 0) ? (-77) : ((i == 1) ? 64 : 38)))) * 6.2831855f;
					for (int j = 1; j < num6; j++)
					{
						meshDrawer.Tri(0, j + 1, j, false);
					}
					meshDrawer.Tri(0, 1, num6, false);
					meshDrawer.Pos(0f, 0f, null);
					for (int k = 0; k < num6; k++)
					{
						num8 = X.GETRAN2((int)(num8 & 127U), 1 + k);
						float num10 = num5 * (1f + 0.125f * X.COSI(Mg.t + X.RAN(num8, 1492) * 100f, X.NI(60, 81, X.RAN(num8, 2771))) + 0.125f * X.COSI(Mg.t + X.RAN(num8, 924) * 100f, X.NI(13, 21, X.RAN(num8, 3431))));
						float num11 = num10 * X.Cos(num9);
						float num12 = num10 * X.Sin(num9);
						meshDrawer.Pos(num11, num12, null);
						num9 += num7;
					}
					for (int l = 0; l < 4; l++)
					{
						num8 = X.GETRAN2((int)(num8 & 127U), 15 + l);
						float num13 = X.COSI(Mg.t + X.RAN(num8, 1492) * 100f, X.NI(60, 81, X.RAN(num8, 2771))) * 0.5f + X.COSI(Mg.t + X.RAN(num8, 2115) * 100f, X.NI(90, 131, X.RAN(num8, 1988))) * 0.5f;
						if (num13 > 0f)
						{
							float num14 = (X.RAN(num8, 1767) + Mg.t / X.NI(58, 84, X.RAN(num8, 1558)) * (float)X.MPF(num8 % 2U == 0U)) * 6.2831855f;
							MTRX.Halo.Set(num3, num5 * 64f * 0.8f + num13 * num4, 0.7f, 0.6f, 1f, 1f);
							MTRX.Halo.Dent(0.7f);
							MgFDHolder.Halo.drawTo(meshDrawer, 0f, 0f, num14, false, 0);
						}
					}
					if (i == 2 && num > 0f)
					{
						float num15 = 170f * (1f - num);
						meshDrawer.ColGrd.setA(0f);
						meshDrawer.BlurCircle2(0f, 0f, num15, num15 * 0.25f, num15 * 0.4f, null, null);
					}
				}
			}
			return true;
		}

		public bool fnManipulateThunderBolt(MagicItem Mg, M2MagicCaster _Mv, float fcnt)
		{
			return Mg.phase >= 0 && _Mv != null && _Mv == Mg.Caster && !base.isWrongMagic(Mg, _Mv) && (Mg.isPreparingCircle || Mg.phase <= 2);
		}

		public bool fnFixTargetMoverPosThunderBolt(MagicItem Mg, M2MagicCaster _Mv, float defx, float defy, ref Vector2 Aim, int fix_aim = -1)
		{
			if (_Mv == null || _Mv != Mg.Caster || base.isWrongMagic(Mg, _Mv))
			{
				return false;
			}
			if (!Mg.isPreparingCircle || fix_aim >= 0)
			{
				return false;
			}
			float num = Mg.Mn._0.ReachableLen(Mg.Mn._0.accel_maxt) + Mg.Mn._0.thick * 2.2f + 1.5f;
			Vector2 center = _Mv.getCenter();
			Vector2 explodePos = Mg.getExplodePos(false);
			float x = center.x;
			float y = center.y;
			float num2 = Mg.Mp.GAR(center.x, center.y, defx, defy);
			int num3 = 4;
			float num4 = -1f;
			float num5 = 1.5707964f;
			for (int i = 0; i < num3; i++)
			{
				float num6 = X.NI(0.31415927f, 1.2566371f, (float)i / (float)(num3 - 1));
				for (int j = 0; j < 2; j++)
				{
					float num7 = (float)((j == 0) ? (-1) : 1);
					float num8 = num2 + num7 * num6;
					float num9 = x + num * X.Cos(num8);
					float num10 = y - num * X.Sin(num8);
					int config = Mg.Mp.getConfig((int)num9, (int)num10);
					if (CCON.canStandAndNoBlockSlope(config) && !CCON.isWater(config) && Mg.Mp.canThroughR(explodePos.x, explodePos.y, num, num8, 1f))
					{
						float num11 = X.LENGTHXY2(center.x * 0.5f, center.y, num9 * 0.5f, num10);
						float num12 = X.Abs(X.angledif(Mg.Mp.GAR(num9, num10, center.x, center.y) / 6.2831855f, Mg.Mp.GAR(num9, num10, defx, defy) / 6.2831855f));
						num11 *= X.ZLINE(num12, 0.25f);
						if (num4 < 0f || num4 < num11)
						{
							num4 = num11;
							Aim.Set(num9, num10);
							num5 = num8;
						}
					}
				}
			}
			if (num4 < 0f)
			{
				Aim.Set(center.x, center.y - 2f);
			}
			Mg.Mn._1.agR = X.GAR2(explodePos.x + num * X.Cos(num5), explodePos.y - num * X.Sin(num5), defx, defy);
			Mg.Mn._1.aim_fixed = true;
			return true;
		}

		private const float thunderbolt_a = 0.003f;

		private const float thunderbolt_accel_t = 50f;

		private const float thunderbolt_damage_lgt = 12f;
	}
}
