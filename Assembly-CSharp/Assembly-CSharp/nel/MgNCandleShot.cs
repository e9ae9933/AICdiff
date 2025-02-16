using System;
using m2d;
using XX;

namespace nel
{
	public class MgNCandleShot : MgFDHolder
	{
		public static MagicItem addCandleShot(NelM2DBase nM2D, M2MagicCaster Caster, MGHIT mg_hit, float x, float y, float vx, float vy, float maxt, float gravity_lock = 0f)
		{
			return MgNCandleShot.addCandleShot(MGKIND.CANDLE_SHOT, nM2D, Caster, mg_hit, x, y, vx, vy, maxt, gravity_lock);
		}

		public static MagicItem addCandleShot(MGKIND kind, NelM2DBase nM2D, M2MagicCaster Caster, MGHIT mg_hit, float x, float y, float vx, float vy, float maxt, float gravity_lock = 0f)
		{
			MagicItem magicItem = nM2D.MGC.setMagic(Caster, kind, mg_hit | MGHIT.IMMEDIATE);
			magicItem.sx = x;
			magicItem.sy = y;
			magicItem.dx = vx;
			magicItem.dy = vy;
			magicItem.da = maxt;
			magicItem.sa = gravity_lock;
			magicItem.t = X.XORSP() * X.Mn(maxt * 0.5f, 40f);
			return magicItem;
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			int num = MgNCandleShot.runCandleShot(Mg, fcnt, true, "fox_candleshot_appear", "fox_candleshot_erased", "fox_candleshot_ground", false);
			if (num == -1)
			{
				Mg.M2D.STAIN.Set(Mg.Dro.x, Mg.Dro.y + Mg.Dro.size + 0.25f, StainItem.TYPE.FIRE, AIM.B, Mg.da, Mg.Dro.get_FootBcc());
			}
			return num != 0 && num > 0;
		}

		public static int runCandleShot(MagicItem Mg, float fcnt, bool check_lift, string appear_effect = "fox_candleshot_appear", string erased_effect = "fox_candleshot_erased", string footed_effect = "fox_candleshot_ground", bool reflect_break = false)
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
				Mg.PtcST(appear_effect, PTCThread.StFollow.FOLLOW_S, false);
				Mg.input_null_to_other_when_quit = true;
				Mg.createDropper(Mg.dx, Mg.dy, 0.060000002f, -1f, -1f);
				Mg.Dro.bounce_x_reduce = 0.4f;
				Mg.Dro.bounce_y_reduce = 0.04f;
				if (check_lift)
				{
					Mg.Dro.type = DROP_TYPE.CHECK_LIFT;
				}
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
				}
				else
				{
					M2Pt pointPuts2 = Mg.Mp.getPointPuts((int)Mg.sx, (int)Mg.sy, false, false);
					if (CCON.isWater((pointPuts2 != null) ? pointPuts2.cfg : 0))
					{
						Mg.PtcST(erased_effect, PTCThread.StFollow.NO_FOLLOW, false);
						return 0;
					}
				}
				if (Mg.phase == 1)
				{
					Mg.Ray.clearTempReflect();
					Mg.MnSetRay(Mg.Ray, 0, Mg.Mp.GAR(0f, 0f, -Mg.Dro.vx, -Mg.Dro.vy), 0f);
					Mg.Ray.LenM(X.LENGTHXYS(0f, 0f, Mg.Dro.vx, Mg.Dro.vy));
					M2MoverPr keyPr = Mg.Mp.getKeyPr();
					if (keyPr != null && X.Abs(keyPr.x - Mg.sx) < 2f && X.Abs(keyPr.y - Mg.sy) < 2f)
					{
						HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir((float)X.MPF(Mg.Dro.vx > 0f)), HITTYPE.NONE);
						if ((hittype & (HITTYPE.HITTED_WATER | HITTYPE.KILLED | HITTYPE.REFLECT_KILLED)) != HITTYPE.NONE || (reflect_break && (hittype & HITTYPE._TEMP_REFLECT) != HITTYPE.NONE))
						{
							Mg.kill(0.125f);
							return 0;
						}
						if ((hittype & HITTYPE.BREAK) != HITTYPE.NONE)
						{
							Mg.PtcVar("maxt", 30.0).PtcST(footed_effect, PTCThread.StFollow.NO_FOLLOW, false);
							return 0;
						}
					}
				}
				else
				{
					Mg.killEffect();
					Mg.PtcVar("maxt", 50.0).PtcST(footed_effect, PTCThread.StFollow.NO_FOLLOW, false);
				}
			}
			if (Mg.phase == 10 || Mg.phase == 11)
			{
				return -1;
			}
			return 1;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			if (Mg.phase < 10)
			{
				MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, true);
				meshImg.Col = meshImg.ColGrd.Set(4282014922U).blend(4282512114U, 0.5f + X.COSI(Mg.t, 43f) * 0.5f).C;
				meshImg.initForImg(MTRX.EffBlurCircle245, 0);
				float num = X.NI(25, 60, 0.5f + X.COSI(Mg.t, 11.3f) * 0.4f + X.COSI(Mg.t, 3.72f) * 0.1f);
				meshImg.Rect(0f, 0f, num, num, false);
				MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
				meshImg2.TranslateP(0f, 9f, false).Scale(1f, 1f + 1.5f * X.ZIGZAGI(Mg.t, 18f), false).TranslateP(0f, -9f, false);
				meshImg2.RotaPF(0f, 0f, 1f, 1f, 0f, MTR.AEfKagaribiActivate[X.ANM((int)Mg.t, 6, 3f)], false, false, false, uint.MaxValue, false, 0);
			}
			return true;
		}

		public const int RUN_RSLT_PROGRESS = 1;

		public const int RUN_RSLT_FOOTED = -1;

		public const int RUN_RSLT_KILL = 0;
	}
}
