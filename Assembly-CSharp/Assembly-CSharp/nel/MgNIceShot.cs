using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class MgNIceShot : MgFDHolder
	{
		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.changeRay(Mg.MGC.makeRay(Mg, 0.23f, true, true));
				Mg.Ray.HitLock(40f, null);
				Mg.raypos_s = (Mg.efpos_s = (Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true)));
				Mg.wind_apply_s_level = 1.75f;
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.Ray.LenM(0.21f);
				Mg.projectile_power = 100;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.REFLECTED;
				Mg.calcAimPos(false);
				Mg.Ray.AngleR(Mg.aim_agR);
				Mg.dx = 0.21f * X.Cos(Mg.aim_agR);
				Mg.dy = -0.21f * X.Sin(Mg.aim_agR);
				Mg.PtcST("frozenbullet", PTCThread.StFollow.FOLLOW_S, false);
				Mg.input_null_to_other_when_quit = true;
			}
			if (Mg.Atk0 == null)
			{
				return Mg.t <= 20f;
			}
			if (Mg.phase == 1)
			{
				if (Mg.Ray.reflected)
				{
					Mg.reflectV(Mg.Ray, ref Mg.dx, ref Mg.dy, 0f, 0.25f, true);
					Mg.calcAimPos(false);
				}
				else
				{
					Mg.calced_aim_pos = true;
				}
				Mg.setRayStartPos(Mg.Ray);
				HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(0.4f * Mg.mpf_is_right, -0.21f), HITTYPE.NONE);
				if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
				{
					return Mg.kill(Mg.Ray.lenmp);
				}
				if ((hittype & (HITTYPE)36868) != HITTYPE.NONE)
				{
					Mg.PtcST("frozenbullet_hit", PTCThread.StFollow.FOLLOW_S, false);
					if ((hittype & (HITTYPE)4100) == HITTYPE.NONE)
					{
						return false;
					}
					Mg.projectile_power = 10;
					Mg.phase = 2;
					Mg.Ray.RadiusM(0.7f).LenM(0f);
					Mg.Ray.HitLock(62f, null);
					Mg.t = 0f;
					if (Mg.sa == 0f)
					{
						Mg.sa = X.NIXP(100f, 140f);
					}
				}
				else
				{
					Mg.sx += Mg.dx;
					Mg.sy += Mg.dy;
				}
			}
			else if (Mg.phase == 2)
			{
				if (Mg.Atk1 != null && (Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk1, HITTYPE.NONE) & HITTYPE.KILLED) != HITTYPE.NONE)
				{
					return Mg.kill(Mg.Ray.lenmp);
				}
				if (Mg.t >= Mg.sa)
				{
					return false;
				}
			}
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			float num = 1f;
			float num2 = 1f;
			if (Mg.phase == 0)
			{
				return true;
			}
			MeshDrawer meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
			MeshDrawer meshDrawer2 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, true);
			float num3 = 1f;
			if (Mg.phase == 1)
			{
				meshDrawer.Rotate(Mg.aim_agR, false);
				meshDrawer2.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
				num2 = 1.5f;
			}
			else if (Mg.phase == 2)
			{
				num *= 2f + 1.5f * (1f - X.ZSIN(Mg.t, 22f));
				num3 = X.ZLINE(Mg.sa - Mg.t, 30f);
			}
			num *= 3f;
			meshDrawer.initForImg(MTRX.EffBlurCircle245, 0);
			meshDrawer2.initForImg(MTRX.EffBlurCircle245, 0);
			float num4 = 15f * (1f + 0.23f * X.COSI(Mg.t, 2.38f) + 0.3f * X.COSI(Mg.t, 5.78f)) * num;
			Color32 c = meshDrawer.ColGrd.Set(4286381055U).blend(4292738815U, X.Mx(0f, X.COSI(Mg.t, 4.85f) * 0.5f + X.COSI(Mg.t, 7.67f) * 0.5f)).mulA(num3)
				.C;
			meshDrawer.Col = c;
			meshDrawer.Rect(0f, 0f, num4 * num2, num4, false);
			num4 = 24f * (1f + 0.34f * X.COSI(Mg.t, 3.41f) + 0.35f * X.COSI(Mg.t, 6.89f)) * num;
			meshDrawer2.Col = meshDrawer2.ColGrd.Set(4288058938U).mulA(num3).C;
			meshDrawer2.Rect(0f, 0f, num4 * num2, num4, false);
			meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
			meshDrawer2 = Mg.Ef.GetMeshImg("icechip", MTRX.MIicon, BLEND.MUL, false);
			meshDrawer.Col = c;
			meshDrawer2.Col = C32.MulA(4285563354U, num3);
			if (Mg.phase == 1)
			{
				meshDrawer.Rotate(Mg.aim_agR, false);
				meshDrawer2.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
				MgNIceShot.drawIceChip(meshDrawer, meshDrawer2, 0f, 0f, 2f, 0.88f, Mg.id, 0);
			}
			else
			{
				float num5 = X.ZSIN(Mg.t, 13f);
				float num6 = X.ZSIN2(Mg.t, 17f) - X.ZSIN(Mg.t - 11f, 21f);
				for (int i = 0; i < 6; i++)
				{
					uint ran = X.GETRAN2(Mg.id * 9, 1 + i * 2);
					meshDrawer.Identity();
					meshDrawer.Rotate(Mg.aim_agR + 1.0471976f * ((float)i + X.RANS(ran, 2426) * 0.12f), false);
					meshDrawer2.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
					float num7 = X.NI(16, 22, X.RAN(ran, 918)) * num5;
					float num8 = 0.45f + num6 * 1.7f;
					num = X.NI(2.8f, 3.3f, X.RAN(ran, 2055));
					MgNIceShot.drawIceChip(meshDrawer, meshDrawer2, num7, 0f, num, num * num8, Mg.id, i);
				}
			}
			return true;
		}

		public static void drawIceChip(MeshDrawer MdA, MeshDrawer MdMul, float x, float y, float scaleX, float scaleY, int index, int id)
		{
			if (MdMul.getTriMax() == 0)
			{
				MdMul.base_z = MdA.base_z - 0.001f;
			}
			uint ran = X.GETRAN2(index * 13, 2 + id * 3);
			PxlFrame frame = MTR.SqEffectFrozenBullet.getFrame((int)((ulong)ran % (ulong)((long)MTR.SqEffectFrozenBullet.countFrames())));
			bool flag = X.RAN(ran, 1411) < 0.5f;
			MdA.RotaPF(x, y, scaleX, scaleY, 0f, frame, flag, false, false, 1U, false, 0);
			MdMul.RotaPF(x, y, scaleX, scaleY, 0f, frame, flag, false, false, 2U, false, 0);
		}
	}
}
