using System;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class MgFireBall : MgFDHolder
	{
		public override MagicNotifiear GetNotifiear()
		{
			return new MagicNotifiear(3).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				maxt = 140f,
				thick = 0.6f,
				accel = 0.008f,
				v0 = 0f,
				penetrate = false,
				other_hit = true,
				auto_target = true,
				cast_on_autotarget = true,
				accel_maxt = 30f,
				auto_target_fixable = true,
				fnManipulateMagic = new MagicNotifiear.FnManipulateMagic(this.fnManipulateFireBall)
			}).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				maxt = 0f,
				other_hit = false,
				thick = 3.2f
			}).AddHit(new MagicNotifiear.MnHit
			{
				no_draw = true,
				time = 30f,
				thick = 1.6f,
				maxt = 3f,
				v0 = 0.03f,
				accel_maxt = 33f,
				accel_mint = 10f
			});
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.target_s = (Mg.raypos_s = (Mg.efpos_s = true));
				Mg.wind_apply_s_level = 1.5f;
				Mg.calcAimPos(true);
				Mg.t = 0f;
				Mg.phase = 1;
				Mg.sa = (Mg.da = Mg.aim_agR);
				Mg.changeRay(Mg.Mn.makeRayForMagic(Mg, 0));
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.PtcST("magic_fireball", PTCThread.StFollow.NO_FOLLOW, false);
				Mg.Mn.use_flexible_fix = false;
				return true;
			}
			if (Mg.Ray.reflected)
			{
				Mg.reflectAgR(Mg.Ray, ref Mg.sa, 0.25f);
			}
			Mg.MnSetRay(Mg.Ray, 0, Mg.sa, Mg.t);
			Mg.calcTargetPos();
			bool flag = Mg.t >= Mg.Mn._0.maxt;
			int num = Mg.Atk0.hpdmg0;
			float num2 = ((Mg.Caster is PR) ? X.NIL(1f, 0.4f, X.LENGTHXYN(Mg.Cen.x, Mg.Cen.y, Mg.sx, Mg.sy) - 10.5f, 6f) : 1f);
			Mg.Atk0.hpdmg0 = (int)((float)num * num2);
			Mg.Ray.clearTempReflect();
			HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(0.4f * Mg.mpf_is_right, -0.21f), HITTYPE.NONE);
			Mg.Atk0.hpdmg0 = num;
			if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
			{
				return Mg.kill(Mg.Ray.lenmp);
			}
			if ((hittype & HITTYPE.WALL_AND_BREAK) != HITTYPE.NONE)
			{
				flag = true;
				if ((hittype & HITTYPE.HITTED_WATER) != HITTYPE.NONE)
				{
					Mg.PtcVar("vx", (double)X.Cos(Mg.sa)).PtcVar("vy", (double)(-(double)X.Sin(Mg.sa))).PtcST("magic_hit_to_water", PTCThread.StFollow.NO_FOLLOW, false);
				}
			}
			if (flag)
			{
				Mg.PtcVar("radius", (double)(Mg.Mn._1.thick * Mg.CLENB)).PtcST("magic_fireball_explode", PTCThread.StFollow.NO_FOLLOW, false);
				Mg.projectile_power = 10;
				Mg.MnSetRay(Mg.Ray.PosMap(Mg.sx, Mg.sy), 1, Mg.sa, Mg.t);
				Mg.Ray.check_other_hit = false;
				num = Mg.Atk1.hpdmg0;
				Mg.Atk1.hpdmg0 = (int)((float)num * num2);
				Mg.Ray.hittype &= ~(HITTYPE.REFLECTED | HITTYPE.REFLECT_BROKEN | HITTYPE.REFLECT_KILLED);
				Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk1.Burst(0.2f * Mg.mpf_is_right, -0.04f), HITTYPE.NONE);
				Mg.Atk1.hpdmg0 = num;
				Mg.kill(-1f);
				return false;
			}
			Mg.Mn.RayShift(Mg.Ray, 0, ref Mg.sx, ref Mg.sy, fcnt);
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("icontexture_sub", MTRX.MIicon, BLEND.SUB, true);
			meshImg.Col = C32.d2c(2801795071U);
			float num = X.NI(0.7f, 1f, X.ZSIN(Mg.t, Mg.Mn._2.accel_maxt * 0.5f)) + X.ZPOW(Mg.t - Mg.Mn._0.accel_mint, 40f) * 0.25f;
			PxlFrame pxlFrame = (Mg.hit_pr ? MTRX.getPF("fireball_pr") : MTRX.getPF("fireball_en"));
			float num2 = (float)X.ANM((int)Mg.Ef.af, 4, 3f) * 1.5707964f + Mg.sa;
			float num3 = X.Cos(Mg.Ef.af / 3f * 6.2831855f);
			float num4 = 1.5f + 0.3f * num3;
			meshImg.RotaPF(0f, 0f, num4, num4 * num, num2, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("icontexture_sub", MTRX.MIicon, BLEND.ADD, true);
			meshImg2.Col = C32.d2c(uint.MaxValue);
			num4 = 1f + num3 * 0.1f;
			pxlFrame = (Mg.hit_pr ? MTRX.getPF("fireball_en") : MTRX.getPF("fireball_pr"));
			meshImg2.RotaPF(0f, 0f, 1f, num, num2, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		public bool fnManipulateFireBall(MagicItem Mg, M2MagicCaster _Mv, float fcnt)
		{
			if (_Mv == null || _Mv != Mg.Caster || base.isWrongMagic(Mg, _Mv) || Mg.already_reflected)
			{
				return false;
			}
			if (Mg.isPreparingCircle)
			{
				return true;
			}
			MagicNotifiear mn = Mg.Mn;
			float num = mn._0.accel_mint;
			float accel_maxt = mn._2.accel_maxt;
			float num2 = 0f;
			int num3 = (int)mn._2.maxt;
			int num4 = -1;
			if (_Mv is PR)
			{
				num4 = (_Mv as PR).Skill.getMagicManipulatorCursorAim(Mg);
			}
			float num5 = Mg.da;
			if (num4 >= 0)
			{
				num5 = CAim.get_agR((AIM)num4, 0f);
				num2 = X.Abs(X.angledifR(num5, Mg.da));
			}
			if (num2 > 0.001f)
			{
				if (Mg.t >= num)
				{
					if (mn._0.time < (float)num3)
					{
						mn._0.time += 1f;
						mn._0.v0 = mn._2.v0;
						mn._0.maxt += mn._2.time + 1f - Mg.t;
						mn._0.auto_target = false;
						mn._0.accel_mint = accel_maxt;
						Mg.da = (Mg.sa = num5);
						Mg.sz = 0f;
						Mg.t = 1f;
						Mg.PtcST("mg_fireball_curve", PTCThread.StFollow.NO_FOLLOW, false);
					}
				}
				else
				{
					Mg.da = num5;
					Mg.sz = 0f;
				}
			}
			else if (num4 != -1)
			{
				Mg.sz = 0f;
			}
			else if (Mg.t < num)
			{
				Mg.sz += fcnt;
				if (Mg.sz >= mn._2.accel_mint)
				{
					mn._0.maxt -= num - Mg.t;
					num = (mn._0.accel_mint = Mg.t - 0.001f);
				}
			}
			if (Mg.da != Mg.sa)
			{
				Mg.sa = Mg.da;
			}
			return Mg.t < num || mn._0.time < (float)num3;
		}
	}
}
