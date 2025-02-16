using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MgNThunderBallShot : MgFDHolder
	{
		public MgNThunderBallShot()
		{
			this.PtcElecA = new EfParticleOnce("thunderball_shot_elec_a", EFCON_TYPE.FIXED);
			this.PtcElecS = new EfParticleOnce("thunderball_shot_elec_s", EFCON_TYPE.FIXED);
		}

		public static MagicItem addThunderBallShot(NelM2DBase nM2D, M2MagicCaster Caster, MGHIT mg_hit, float x, float y, float _agR)
		{
			return MgNCandleShot.addCandleShot(MGKIND.THUNDERBALL_SHOT, nM2D, Caster, mg_hit, x, y, 0.1f * X.Cos(_agR), -0.1f * X.Sin(_agR), 0f, 0f);
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.changeRay(Mg.MGC.makeRay(Mg, 0.06f, (Mg.hittype & MGHIT.EN) == (MGHIT)0, true));
				Mg.Ray.HitLock(40f, Mg.MGC.getHitLink(MGKIND.CANDLE_SHOT));
				Mg.raypos_s = (Mg.efpos_s = (Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true)));
				Mg.wind_apply_s_level = 1.75f;
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.Ray.Atk = Mg.Atk0;
				Mg.sa = 0f;
				Mg.sz = 0f;
				Mg.projectile_power = 5;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.PtcST("thunderball_shot_appear", PTCThread.StFollow.FOLLOW_S, false);
				Mg.input_null_to_other_when_quit = true;
				Mg.createDropper(Mg.dx, Mg.dy, 0.008f, -1f, -1f);
				Mg.Dro.bounce_x_reduce = 1f;
				Mg.Dro.bounce_y_reduce = 1f;
				Mg.Dro.gravity_scale = 0.0002f;
			}
			if (fcnt > 0f)
			{
				if (Mg.Dro.x_bounced || Mg.Dro.y_bounced)
				{
					Mg.phase++;
					float num = Mg.aim_agR;
					Mg.calcAimPos(false);
					num += X.angledifR(num, Mg.aim_agR) * 0.5f;
					Mg.PtcVar("agR", (double)num);
					Mg.sz = 0f;
					if (Mg.phase > 3)
					{
						Mg.PtcST("thunderball_shot_break", PTCThread.StFollow.NO_FOLLOW, false);
						return false;
					}
					Mg.PtcST("thunderball_shot_bounce", PTCThread.StFollow.NO_FOLLOW, false);
				}
				M2Pt pointPuts = Mg.Mp.getPointPuts((int)Mg.sx, (int)Mg.sy, false, false);
				if (CCON.isWater((pointPuts != null) ? pointPuts.cfg : 0))
				{
					Mg.kill(0.125f);
					return false;
				}
			}
			if (Mg.sz < 2.5f)
			{
				Mg.sz = X.Mn(Mg.sz + fcnt * 0.1f, 2.5f);
			}
			Mg.Ray.clearTempReflect();
			Mg.calcAimPos(false);
			float num2 = X.Cos(Mg.aim_agR);
			float num3 = X.Sin(Mg.aim_agR);
			Mg.Ray.PosMap(Mg.sx - num2 * Mg.sz, Mg.sy + num3 * Mg.sz);
			Mg.Ray.LenM(Mg.sz);
			Mg.Ray.Dir = new Vector2(num2, num3);
			HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir((float)X.MPF(Mg.Dro.vx > 0f)), HITTYPE.NONE);
			if ((hittype & (HITTYPE.HITTED_WATER | HITTYPE.KILLED | HITTYPE.REFLECT_KILLED)) != HITTYPE.NONE || (hittype & HITTYPE._TEMP_REFLECT) != HITTYPE.NONE)
			{
				Mg.kill(0.125f);
				return false;
			}
			if ((hittype & HITTYPE.BREAK) != HITTYPE.NONE && (hittype & HITTYPE.WALL) == HITTYPE.NONE)
			{
				Mg.PtcVar("agR", (double)Mg.aim_agR);
				Mg.PtcST("thunderball_shot_break", PTCThread.StFollow.NO_FOLLOW, false);
				return false;
			}
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			if (Mg.phase < 1)
			{
				return true;
			}
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
			meshImg.Col = meshImg.ColGrd.Set(4292382524U).blend(4292427157U, 0.5f + X.COSI(Mg.t, 63f) * 0.5f).C;
			meshImg.initForImg(MTRX.EffBlurCircle245, 0);
			float num = X.NI(40, 80, 0.5f + X.COSI(Mg.t, 11.3f) * 0.4f + X.COSI(Mg.t, 3.72f) * 0.1f);
			meshImg.Rect(0f, 0f, num, num, false);
			MeshDrawer mesh = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
			Mg.calcAimPos(false);
			meshImg.initForImg(MTRX.IconWhite, 0);
			this.PtcElecA.drawTo(mesh, mesh.base_px_x, mesh.base_px_y, 3.1415927f + Mg.aim_agR, (int)(Mg.sz * Mg.Mp.CLENB + 8f), Mg.t, this.PtcElecA.loop_time);
			this.PtcElecS.drawTo(meshImg, mesh.base_px_x, mesh.base_px_y, 3.1415927f + Mg.aim_agR, (int)(Mg.sz * Mg.Mp.CLENB + 8f), Mg.t, this.PtcElecS.loop_time);
			mesh.ColGrd.Set(4292803908U).blend(4279594249U, 0.5f + X.COSI(Mg.t + 22f, 53f) * 0.5f).blend(4288737279U, 0.5f + X.COSI(Mg.t + 33f, 5.31f) * 0.3f + X.COSI(Mg.t + 49f, 16.15f) * 0.2f);
			if (X.COSI(Mg.t, 13.3f) >= 0.5f)
			{
				mesh.ColGrd.blend(4280789014U, 0.7f);
			}
			mesh.Col = mesh.ColGrd.C;
			for (int i = 0; i < 1; i++)
			{
				mesh.Pos(0f, 0f, null);
				for (int j = 0; j < 12; j++)
				{
					mesh.Tri(-1, j, (j + 1) % 12, false);
				}
				float num2 = (float)X.MPF(i == 0) * (Mg.t + 15f) / 55f * 3.1415927f;
				for (int k = 0; k < 12; k++)
				{
					uint ran = X.GETRAN2(Mg.id + k * 13 + i * 7, k * 6 + i * 5);
					float num3 = 1f + X.COSI(Mg.t + X.RAN(ran, 2575) * 21f, 14f + X.RAN(ran, 2558) * 25f);
					if (k % 3 > 0)
					{
						num3 = ((num3 - 1f) * 0.125f + 0.4f) * 0.70710677f;
					}
					else if (num3 < 1f)
					{
						num3 = X.Scr(num3, 0.5f);
						num3 = X.Scr(num3, num3);
					}
					else
					{
						num3 -= 1f;
						num3 = X.Scr(num3, num3);
						num3 = X.Scr(num3, num3);
						num3 = num3 * 0.3f + 1f;
					}
					float num4 = num3 * 18f * 0.015625f;
					mesh.Pos(X.Cos(num2) * num4, X.Sin(num2) * num4, null);
					num2 -= 0.7853982f;
				}
			}
			return true;
		}

		private const float ELEC_FINE_INTV = 8f;

		private EfParticleOnce PtcElecA;

		private EfParticleOnce PtcElecS;

		public const int RUN_RSLT_PROGRESS = 1;

		public const int RUN_RSLT_FOOTED = -1;

		public const int RUN_RSLT_KILL = 0;

		private const float HIT_LEN = 2.5f;

		private const float spd_default = 0.1f;
	}
}
