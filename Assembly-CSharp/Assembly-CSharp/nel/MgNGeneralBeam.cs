using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MgNGeneralBeam : MgFDHolder
	{
		public static MagicItem setGeneralBeam(Map2d Mp, M2MagicCaster Caster, MgNGeneralBeam.IGeneralBeamer Beamer, MGHIT mg_hit, int id, MgNGeneralBeam.BATTR battr, ref NelAttackInfo AtkBuf, ref MagicNotifiear MnBuf, float arrow_power, bool penetrate_only_mv, float radius, float shoot_time, bool no_draw_wide = true)
		{
			MagicItem magicItem = (Mp.M2D as NelM2DBase).MGC.setMagic(Caster, MGKIND.GENERAL_BEAM, mg_hit | MGHIT.IMMEDIATE);
			if (MnBuf == null)
			{
				MnBuf = MgNGeneralBeam.createNotifiearBeam(radius, shoot_time);
			}
			magicItem.Mn = MnBuf;
			magicItem.Mn._0.penetrate_only_mv = penetrate_only_mv;
			magicItem.Mn._0.no_draw_wide = no_draw_wide;
			magicItem.dz = (float)id;
			magicItem.projectile_power = 900;
			magicItem.Other = Beamer;
			MgNGeneralBeam.getAttackInfo(ref AtkBuf, magicItem.Ray, battr, arrow_power);
			magicItem.Atk0 = AtkBuf;
			return magicItem;
		}

		public static NelAttackInfo getAttackInfo(ref NelAttackInfo Ret, M2Ray Ray, MgNGeneralBeam.BATTR attr, float power = 1f)
		{
			switch (attr)
			{
			case MgNGeneralBeam.BATTR.FIRE:
				if (Ret == null)
				{
					Ret = new NelAttackInfo
					{
						hpdmg0 = X.IntC(9f * power),
						split_mpdmg = 3,
						burst_vx = 0.05f,
						huttobi_ratio = 0.002f,
						attr = MGATTR.FIRE,
						shield_break_ratio = -7f,
						parryable = false,
						nodamage_time = 0,
						SerDmg = new FlagCounter<SER>(1).Add(SER.BURNED, 25f)
					}.Torn(0.015f, 0.11f);
				}
				Ray.HitLock(22f, null);
				return Ret;
			case MgNGeneralBeam.BATTR.ICE:
				if (Ret == null)
				{
					Ret = new NelAttackInfo
					{
						hpdmg0 = X.IntC(9f * power),
						split_mpdmg = 1,
						burst_vx = 0.01f,
						huttobi_ratio = 0.002f,
						attr = MGATTR.ICE,
						shield_break_ratio = -7f,
						parryable = false,
						nodamage_time = 0,
						SerDmg = new FlagCounter<SER>(1).Add(SER.FROZEN, 50f)
					}.Torn(0.004f, 0.007f);
				}
				Ray.HitLock(28f, null);
				return Ret;
			case MgNGeneralBeam.BATTR.THUNDER:
				if (Ret == null)
				{
					Ret = new NelAttackInfo
					{
						hpdmg0 = X.IntC(10f * power),
						split_mpdmg = 1,
						burst_vx = 0.03f,
						huttobi_ratio = 0.002f,
						attr = MGATTR.THUNDER,
						shield_break_ratio = -7f,
						parryable = false,
						nodamage_time = 0,
						SerDmg = new FlagCounter<SER>(1).Add(SER.PARALYSIS, 30f)
					}.Torn(0.015f, 0.11f);
				}
				Ray.HitLock(20f, null);
				return Ret;
			case MgNGeneralBeam.BATTR.SLIMY:
				if (Ret == null)
				{
					Ret = new NelAttackInfo
					{
						hpdmg0 = X.IntC(2f * power),
						mpdmg0 = X.IntC(2f * power),
						split_mpdmg = X.IntC(7f * power),
						burst_vx = 0.04f,
						huttobi_ratio = -1000f,
						attr = MGATTR.SPERMA,
						shield_break_ratio = 0.0001f,
						parryable = false,
						nodamage_time = 0,
						SerDmg = new FlagCounter<SER>(1).Add(SER.WEB_TRAPPED, 30f)
					};
				}
				Ray.HitLock(16f, null);
				return Ret;
			case MgNGeneralBeam.BATTR.STONE:
				if (Ret == null)
				{
					Ret = new NelAttackInfo
					{
						hpdmg0 = X.IntC(4f * power),
						split_mpdmg = 3,
						burst_vx = 0.04f,
						huttobi_ratio = -1000f,
						attr = MGATTR.STONE,
						shield_break_ratio = -4f,
						parryable = false,
						nodamage_time = 0,
						SerDmg = new FlagCounter<SER>(1).Add(SER.STONE, 30f)
					};
				}
				Ray.HitLock(18f, null);
				return Ret;
			}
			if (Ret == null)
			{
				Ret = new NelAttackInfo
				{
					hpdmg0 = 0,
					mpdmg0 = X.IntC(8f * power),
					split_mpdmg = X.IntC(7f * power),
					burst_vx = 0.04f,
					huttobi_ratio = -1000f,
					attr = MGATTR.ACME,
					shield_break_ratio = 0.0001f,
					parryable = false,
					nodamage_time = 0,
					SerDmg = new FlagCounter<SER>(1).Add(SER.SEXERCISE, (float)X.IntC(70f * power)),
					EpDmg = new EpAtk(30, "beam")
					{
						canal = 8,
						cli = 2
					}.MultipleOrgasm(0.4f)
				};
			}
			Ray.HitLock(13f, null);
			return Ret;
		}

		public static void prepareMagic(M2MagicCaster En, NelAttackInfo[] ALaserAtk, MagicItem Mg, MgNGeneralBeam.BATTR attr, float power = 1f)
		{
			Mg.Atk0 = MgNGeneralBeam.getAttackInfo(ref ALaserAtk[(int)attr], Mg.Ray, attr, power);
			Mg.projectile_power = 900;
			Mg.Atk0.Caster = En;
		}

		private static MagicNotifiear createNotifiearBeam(float _radius, float shoot_time)
		{
			return new MagicNotifiear(1).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				maxt = 1f,
				thick = _radius,
				accel = 0f,
				accel_mint = shoot_time,
				accel_maxt = 0f,
				v0 = 0f,
				penetrate = false,
				aim_fixed = true,
				wall_hit = true,
				other_hit = false,
				cast_on_autotarget = true,
				draw_only_line = true
			});
		}

		public static bool nattr2battr(ENATTR nattr, out MgNGeneralBeam.BATTR battr)
		{
			if ((nattr & ENATTR._MATTR) != ENATTR.NORMAL)
			{
				battr = MgNGeneralBeam.BATTR.FIRE;
				if ((nattr & ENATTR.FIRE) != ENATTR.NORMAL)
				{
					battr = MgNGeneralBeam.BATTR.FIRE;
				}
				if ((nattr & ENATTR.ICE) != ENATTR.NORMAL)
				{
					battr = MgNGeneralBeam.BATTR.ICE;
				}
				if ((nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
				{
					battr = MgNGeneralBeam.BATTR.THUNDER;
				}
				if ((nattr & ENATTR.ACME) != ENATTR.NORMAL)
				{
					battr = MgNGeneralBeam.BATTR.ACME;
				}
				if ((nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
				{
					battr = MgNGeneralBeam.BATTR.SLIMY;
				}
				return true;
			}
			battr = MgNGeneralBeam.BATTR._OTHER;
			return false;
		}

		public static void mgattr2col(MGATTR attr, out Color32 col0, out Color32 col1, out Color32 colsub)
		{
			switch (attr)
			{
			case MGATTR.FIRE:
				col0 = C32.d2c(4294942222U);
				col1 = C32.d2c(2004157701U);
				colsub = C32.d2c(4282020220U);
				return;
			case MGATTR.ICE:
				col0 = C32.d2c(4280340989U);
				col1 = C32.d2c(1996823413U);
				colsub = C32.d2c(4288439086U);
				return;
			case MGATTR.THUNDER:
				col0 = C32.d2c(4293524018U);
				col1 = C32.d2c(2001889584U);
				colsub = C32.d2c(4281532888U);
				return;
			default:
				if (attr == MGATTR.ACME)
				{
					col0 = C32.d2c(4294138793U);
					col1 = C32.d2c(2863860369U);
					colsub = C32.d2c(4282483037U);
					return;
				}
				if (attr != MGATTR.STONE)
				{
					col0 = C32.d2c(4292927712U);
					col1 = C32.d2c(2859429743U);
					colsub = C32.d2c(4286805614U);
					return;
				}
				col0 = C32.d2c(4284374622U);
				col1 = C32.d2c(2859429743U);
				colsub = C32.d2c(4286805614U);
				return;
			}
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase >= 4)
			{
				return MgNGeneralBeam.MgRunArrowBeam(Mg, fcnt, 4f, null) != 0;
			}
			if (!Mg.Caster.canHoldMagic(Mg))
			{
				return false;
			}
			int num = MgNGeneralBeam.MgRunArrowBeam(Mg, fcnt, 4f, null);
			if (num == 0)
			{
				return false;
			}
			MgNGeneralBeam.IGeneralBeamer generalBeamer = Mg.Other as MgNGeneralBeam.IGeneralBeamer;
			if (generalBeamer == null)
			{
				return true;
			}
			bool flag = false;
			if (Mg.phase == 1)
			{
				Mg.phase = 2;
				flag = true;
				Mg.t = 10f;
			}
			float beamPositionWalkSpeed = generalBeamer.getBeamPositionWalkSpeed();
			if (Mg.da <= 0f && Mg.phase < 4)
			{
				Mg.da = 4f;
				Vector2 vector;
				float num2;
				bool flag2;
				if (!generalBeamer.getBeamPosition(Mg, (int)Mg.dz, out vector, out num2, out flag2))
				{
					return false;
				}
				Mg.dx = vector.x;
				Mg.dy = vector.y;
				if (flag)
				{
					Mg.Mn._0.agR = num2;
					Mg.sx = Mg.dx;
					Mg.sy = Mg.dy;
				}
				else
				{
					Mg.Mn._0.agR = X.VALWALKANGLER(Mg.Mn._0.agR, num2, (flag2 ? 0.0045f : 0.012f) * fcnt * 3.1415927f);
				}
			}
			Mg.sx = X.VALWALK(Mg.sx, Mg.dx, beamPositionWalkSpeed * fcnt);
			Mg.sy = X.VALWALK(Mg.sy, Mg.dy, beamPositionWalkSpeed * fcnt);
			Mg.da -= fcnt;
			if (num == 2 && Mg.phase <= 3)
			{
				Mg.Mn._0.v0 = generalBeamer.getBeamReachableLength();
				Mg.Mn._0.CalcReachable(Mg.Ray, new Vector2(Mg.sx, Mg.sy), 0f, null);
				Mg.Mn._0.accel_maxt = ((Mg.Mn._0.v0 > Mg.Mn._0.len) ? 0.6f : 0f);
				Mg.Mn._0.v0 = Mg.Mn._0.len;
			}
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			MgNGeneralBeam.MgDrawArrowS(Mg, fcnt, true);
			MgNGeneralBeam.IGeneralBeamer generalBeamer = Mg.Other as MgNGeneralBeam.IGeneralBeamer;
			return generalBeamer == null || generalBeamer.BeamDrawAfter(Mg, fcnt);
		}

		public static int MgRunArrowBeam(MagicItem Mg, float fcnt, float fine_intv = 10f, MgNGeneralBeam.FnPhaseContinue FD_phase_continue = null)
		{
			int num = 1;
			if (Mg.phase == 0)
			{
				Mg.Ray.check_hit_wall = false;
				Mg.Ray.hittype |= HITTYPE.AUTO_TARGET | HITTYPE.TARGET_CHECKER;
				Mg.phase = 1;
				Mg.efpos_s = (Mg.raypos_s = (Mg.aimagr_calc_s = true));
				Mg.projectile_power = 50;
				Mg.Ray.check_other_hit = true;
				Mg.Ray.Atk = Mg.Atk0;
				Mg.projectile_power = 100;
				Mg.sz = 0f;
			}
			if (Mg.phase >= 1 && Mg.Mn != null)
			{
				Mg.aim_agR = Mg.Mn._0.agR;
				Mg.calced_aim_pos = true;
			}
			if (Mg.phase >= 2)
			{
				if (Mg.sz <= 0f)
				{
					num = 2;
					Mg.sz = fine_intv;
				}
				Mg.sz -= fcnt;
			}
			if (Mg.phase == 3)
			{
				if (Mg.Mn == null)
				{
					return 0;
				}
				float num2 = Mg.sx;
				float num3 = Mg.sy;
				float num4 = 0f;
				for (int i = 0; i < Mg.Mn.Count; i++)
				{
					MagicNotifiear.MnHit hit = Mg.Mn.GetHit(i);
					if (hit.len <= 0f)
					{
						break;
					}
					Mg.Ray.RadiusM(hit.thick);
					Mg.Ray.PosMap(num2, num3);
					Mg.Mn.SetRay(Mg.Ray, i, hit.agR, hit.thick, num4 + 0.35f);
					Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir(Mg.Ray.Dir.x > 0f), HITTYPE.NONE);
					if (FD_phase_continue != null && !FD_phase_continue(i))
					{
						if (num == 2)
						{
							Mg.sz = 1f;
							num = 1;
						}
						Mg.sz = X.Mn(Mg.sz, 5f);
						break;
					}
					if (i >= Mg.Mn.Count - 1)
					{
						break;
					}
					MagicNotifiear.MnHit hit2 = Mg.Mn.GetHit(i + 1);
					num4 = hit2.thick * 1.75f;
					num2 += hit.v0 * X.Cos(hit.agR) + num4 * X.Cos(hit2.agR);
					num3 += -hit.v0 * X.Sin(hit.agR) - num4 * X.Sin(hit2.agR);
					num4 *= -1f;
				}
			}
			if (Mg.phase != 4)
			{
				return num;
			}
			if (Mg.t >= 20f)
			{
				return 0;
			}
			return num;
		}

		public static bool MgDrawArrowS(MagicItem Mg, float fcnt, bool charge_draw_top = false)
		{
			Map2d mp = Mg.Mp;
			M2MagicCaster caster = Mg.Atk0.Caster;
			MeshDrawer meshDrawer;
			if (Mg.phase == 2)
			{
				meshDrawer = Mg.Ef.GetMesh("", MTRX.MtrMeshNormal, !charge_draw_top);
				Mg.Mn.drawTo(meshDrawer, mp, new Vector2(Mg.sx, Mg.sy), Mg.t, 0f, false, Mg.t, null, null);
				return true;
			}
			if ((Mg.phase != 3 && Mg.phase != 4) || Mg.Atk0 == null)
			{
				return true;
			}
			Color32 color;
			Color32 color2;
			Color32 color3;
			MgNGeneralBeam.mgattr2col(Mg.Atk0.attr, out color, out color2, out color3);
			meshDrawer = Mg.Ef.GetMesh("", (Mg.phase == 3) ? MTR.MtrBeam : MTRX.MtrMeshNormal, false);
			MeshDrawer mesh = Mg.Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.ADD, -1), false);
			mesh.initForImg(MTRX.EffBlurCircle245, 0);
			if (meshDrawer.getTriMax() == 0 && Mg.phase == 3)
			{
				meshDrawer.base_z = mesh.base_z + 0.002f;
			}
			meshDrawer.base_x = (meshDrawer.base_y = (mesh.base_x = (mesh.base_y = 0f)));
			float agR = Mg.Mn._0.agR;
			meshDrawer.Rotate(agR, false).Translate(mp.map2globalux(Mg.sx), mp.map2globaluy(Mg.sy), false);
			Color32 color4 = color2;
			Color32 color5 = color;
			C32 c = null;
			float num2;
			float num3;
			if (Mg.phase == 3)
			{
				float num = X.ZSIN2(Mg.t, 24f);
				if (num < 1f)
				{
					color4 = meshDrawer.ColGrd.Set(color2).blend(uint.MaxValue, 1f - num).C;
				}
				color5 = meshDrawer.ColGrd.Set(color5).blend(uint.MaxValue, 1f - num).C;
				num2 = 1f + X.COSI(mp.floort, 7.33f) * 0.3f + X.COSI(mp.floort, 3.21f) * 0.22f;
				num3 = 11f * num2 * X.NIL(1f, 0.6f, Mg.t - 30f, Mg.Mn._0.accel_mint - 30f) + 43f * (1f - num);
				mesh.Col = mesh.ColGrd.Set(color5).blend(color4, 0.5f + 0.3f * X.COSI(mp.floort, 14.33f) + 0.2f * X.COSI(mp.floort, 8.11f)).setA1(1f)
					.C;
			}
			else
			{
				float num = X.ZSIN2(Mg.t, 20f);
				color4 = meshDrawer.ColGrd.Set(color2).blend(uint.MaxValue, 1f - 0.6f * num).C;
				color5 = meshDrawer.ColGrd.Set(color5).blend(uint.MaxValue, 1f - 0.3f * num).C;
				c = meshDrawer.ColGrd.Set(color4);
				num2 = 1.5f * (1f - num);
				num3 = 23f * num2;
				mesh.Col = mesh.ColGrd.Set(color5).mulA(1f - num).C;
			}
			num3 *= 0.015625f;
			if (num3 <= 0f)
			{
				return true;
			}
			if (Mg.Mn._0.thick != 0.18f)
			{
				num3 *= Mg.Mn._0.thick / 0.18f;
			}
			meshDrawer.Uv23(color4, true);
			mesh.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
			MgNGeneralBeam.drawCirc(mesh, 80f, num2);
			MagicNotifiear.MnHit mnHit = Mg.Mn.GetHit(0);
			float num4 = Mg.sx;
			float num5 = Mg.sy;
			float num6 = Mg.Mn._0.len * mp.CLENB * 0.015625f;
			int num7 = 0;
			while (num7 < Mg.Mn.Count && num6 > 0f)
			{
				meshDrawer.Col = color5;
				meshDrawer.TriRectBL(0).Tri(4, 0, 3, false).Tri(4, 3, 5, false);
				meshDrawer.uvRectN(0f, 0.5f).Pos(0f, 0f, null);
				meshDrawer.uvRectN(0f, 1f).Pos(0f, num3, c);
				meshDrawer.uvRectN(1f, 1f).Pos(num6, num3, c);
				meshDrawer.uvRectN(1f, 0.5f).Pos(num6, 0f, null);
				meshDrawer.uvRectN(0f, 0f).Pos(0f, -num3, c);
				meshDrawer.uvRectN(1f, 0f).Pos(num6, -num3, c);
				bool flag = num7 >= Mg.Mn.Count - 1;
				if (flag && mnHit.accel_maxt == 0f)
				{
					break;
				}
				num4 += mnHit.len * X.Cos(mnHit.agR);
				num5 += -mnHit.len * X.Sin(mnHit.agR);
				meshDrawer.Identity();
				meshDrawer.Translate(mp.map2globalux(num4), mp.map2globaluy(num5), false);
				mesh.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
				if (mnHit.accel_maxt > 0f)
				{
					MgNGeneralBeam.drawCirc(mesh, 80f * mnHit.accel_maxt, num2);
				}
				if (flag)
				{
					break;
				}
				MagicNotifiear.MnHit hit = Mg.Mn.GetHit(num7 + 1);
				if (hit.len <= 0f)
				{
					break;
				}
				meshDrawer.Rotate(hit.agR, true);
				mesh.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
				num6 = hit.len * mp.CLENB * 0.015625f;
				mnHit = hit;
				num7++;
			}
			meshDrawer.allocUv2(0, true).allocUv3(0, true);
			return true;
		}

		private static void drawCirc(MeshDrawer MdA, float circr, float tzr)
		{
			for (int i = 0; i <= 2; i++)
			{
				float num = circr + (1f + 0.3f * (float)i);
				MdA.Rect(0f, 0f, num * tzr, num * tzr, false);
			}
		}

		public const int ATTR_MAX = 4;

		public const int ATTR_MAX2 = 5;

		public const float arrow_radius = 0.18f;

		public const int MPHASE_OFFLINE = 1;

		public const int MPHASE_CHARGE = 2;

		public const int MPHASE_SHOOT = 3;

		public const int MPHASE_RELEASE = 4;

		public delegate bool FnPhaseContinue(int phase);

		public enum BATTR : byte
		{
			FIRE,
			ICE,
			THUNDER,
			ACME,
			SLIMY,
			STONE,
			_MAX,
			_OTHER
		}

		public interface IGeneralBeamer
		{
			bool getBeamPosition(MagicItem Mg, int id, out Vector2 SPos, out float agR, out bool slow_angle_walk);

			bool BeamDrawAfter(MagicItem Mg, float fcnt);

			float getBeamPositionWalkSpeed();

			float getBeamReachableLength();
		}
	}
}
