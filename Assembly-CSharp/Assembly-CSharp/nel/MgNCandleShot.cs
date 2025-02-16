using System;
using m2d;
using XX;

namespace nel
{
	public class MgNCandleShot : MgFDHolder
	{
		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.changeRay(Mg.MGC.makeRay(Mg, 0.45000002f, (Mg.hittype & MGHIT.EN) == (MGHIT)0, true));
				Mg.Ray.HitLock(40f, Mg.MGC.getHitLink(MGKIND.CANDLE_SHOT));
				Mg.raypos_s = (Mg.efpos_s = (Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true)));
				Mg.wind_apply_s_level = 1.75f;
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.Ray.Atk = Mg.Atk0;
				Mg.projectile_power = 10;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.PtcST("fox_candleshot_appear", PTCThread.StFollow.FOLLOW_S, false);
				Mg.input_null_to_other_when_quit = true;
				Mg.createDropper(Mg.dx, Mg.dy, 0.060000002f, -1f, -1f);
				Mg.Dro.bounce_x_reduce = 0.4f;
				Mg.Dro.bounce_y_reduce = 0.04f;
				Mg.Dro.type = DROP_TYPE.CHECK_LIFT;
			}
			if (Mg.phase == 1 && fcnt > 0f)
			{
				Mg.Dro.gravity_scale = ((Mg.t < Mg.sa) ? 0.0003f : 0.045f);
				if (Mg.Dro.on_ground || Mg.Dro.y_bounced)
				{
					Mg.Dro.vy = 0f;
					int num = (int)(Mg.sy + 0.3f + 0.4f);
					M2Pt pointPuts = Mg.Mp.getPointPuts((int)Mg.sx, num, false, false);
					int num2 = ((pointPuts != null) ? pointPuts.cfg : 0);
					if (Mg.Dro.on_ground)
					{
						Mg.phase = 10;
						if (pointPuts != null && pointPuts.isLift())
						{
							Mg.phase = 11;
							Mg.sy = (float)num;
						}
					}
					else if (!CCON.canStand(num2) || CCON.isBlockSlope(num2))
					{
						Mg.phase = 10;
						Mg.sy = (float)num;
					}
					else if (CCON.isWater(num2))
					{
						Mg.PtcST("fox_candleshot_erased", PTCThread.StFollow.NO_FOLLOW, false);
						return false;
					}
				}
				if (Mg.phase == 1)
				{
					Mg.MnSetRay(Mg.Ray, 0, Mg.Mp.GAR(0f, 0f, -Mg.Dro.vx, -Mg.Dro.vy), 0f);
					Mg.Ray.LenM(X.LENGTHXYS(0f, 0f, Mg.Dro.vx, Mg.Dro.vy));
					M2MoverPr keyPr = Mg.Mp.getKeyPr();
					if (keyPr != null && X.Abs(keyPr.x - Mg.sx) < 2f && X.Abs(keyPr.y - Mg.sy) < 2f)
					{
						HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir((float)X.MPF(Mg.Dro.vx > 0f)), HITTYPE.NONE);
						if ((hittype & (HITTYPE)8458240) != HITTYPE.NONE)
						{
							Mg.kill(0.125f);
							return false;
						}
						if ((hittype & HITTYPE.BREAK) != HITTYPE.NONE)
						{
							Mg.PtcVar("maxt", 30.0).PtcST("fox_candleshot_ground", PTCThread.StFollow.NO_FOLLOW, false);
							return false;
						}
					}
				}
				else
				{
					Mg.M2D.STAIN.Set(Mg.Dro.x, Mg.Dro.y + Mg.Dro.size + 0.25f, StainItem.TYPE.FIRE, AIM.B, Mg.da, Mg.Dro.get_FootBcc());
					Mg.killEffect();
					Mg.PtcVar("maxt", 50.0).PtcST("fox_candleshot_ground", PTCThread.StFollow.NO_FOLLOW, false);
				}
			}
			return Mg.phase != 10 && Mg.phase != 11;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			if (Mg.phase < 10)
			{
				MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
				meshImg.TranslateP(0f, 9f, false).Scale(1f, 1f + 1.5f * X.ZIGZAGI(Mg.t, 18f), false).TranslateP(0f, -9f, false);
				meshImg.RotaPF(0f, 0f, 1f, 1f, 0f, MTR.AEfKagaribiActivate[X.ANM((int)Mg.t, 6, 3f)], false, false, false, uint.MaxValue, false, 0);
			}
			return true;
		}
	}
}
