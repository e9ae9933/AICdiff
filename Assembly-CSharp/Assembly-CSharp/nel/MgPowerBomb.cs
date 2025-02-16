using System;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class MgPowerBomb : MgFDHolder
	{
		public override MagicNotifiear GetNotifiear()
		{
			return new MagicNotifiear(2).AddHit(new MagicNotifiear.MnHit
			{
				maxt = 160f,
				v0 = 0.4f,
				thick = 0.25f,
				accel_maxt = 50f,
				accel = -0.008f,
				wall_hit = true,
				penetrate = true,
				other_hit = false,
				no_draw = false,
				auto_target = false,
				fnManipulateTargetting = new MagicNotifiear.FnManipulateTargetting(MagicNotifiearData.fnManipulateTargettingDefault)
			}).AddHit(new MagicNotifiear.MnHit
			{
				thick = 7.5f,
				accel_maxt = 40f,
				maxt = 190f,
				wall_hit = true,
				penetrate = true,
				other_hit = false,
				no_draw = false,
				cast_on_autotarget = true,
				auto_target = false
			});
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase <= 1)
			{
				if (Mg.Ray == null)
				{
					Mg.changeRay(Mg.MGC.makeRay(Mg, Mg.Mn._0.thick, false, true));
				}
				if ((Mg.hittype & MGHIT.BERSERK) != (MGHIT)0)
				{
					Mg.Ray.hittype |= HITTYPE.BERSERK_MYSELF;
				}
				Mg.MGC.initRayCohitable(Mg.Ray);
				Mg.Ray.cohitable_allow_berserk = M2Ray.COHIT.BERSERK_N;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.NONE;
				Mg.raypos_s = (Mg.efpos_s = (Mg.target_s = true));
				Mg.wind_apply_s_level = 0.66f;
				Mg.Atk0.PublishMagic = Mg;
				Mg.Mn._0.x = (Mg.Mn._0.y = 0f);
				Mg.PtcVar("maxt", (double)Mg.Mn._0.maxt).PtcST("powerbomb_prepare", PTCThread.StFollow.FOLLOW_S, false);
				Mg.phase = 2;
			}
			if (Mg.phase == 2 && Mg.t >= Mg.Mn._0.maxt)
			{
				Mg.t = 0f;
				Mg.phase = 3;
				Mg.killEffect();
				Mg.Ray.cohitable_allow_berserk = M2Ray.COHIT.NONE;
				Mg.Mn.SetRay(Mg.Ray, 1, 0f, 0f);
				Mg.PtcVar("accel_maxt", (double)Mg.Mn._1.accel_maxt).PtcVar("maxt", (double)Mg.Mn._1.maxt).PtcST("powerbomb_explode", PTCThread.StFollow.FOLLOW_S, false);
				Mg.Ray.HitLock(20f, null);
			}
			if (Mg.phase == 3)
			{
				Mg.setRayStartPos(Mg.Ray);
				Mg.Ray.RadiusM(Mg.Mn._1.thick * X.ZPOWN(Mg.t, Mg.Mn._1.accel_maxt, 4f));
				Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
				if (Mg.t > Mg.Mn._1.maxt)
				{
					Mg.phase = 4;
					Mg.t = 0f;
				}
			}
			return Mg.phase != 4 || Mg.t < 120f;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 2)
			{
				PxlImage img = MTRX.AEff[0].getLayer(0).Img;
				float num = 0.3f * X.COSI(Mg.t, 23.4f) + 0.3f;
				MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
				meshImg.initForImg(img, 0);
				float num2 = (30f + 60f * X.ZPOW(num - 0.25f, 0.75f)) * (0.75f + 0.125f * num * X.COSI(Mg.t, 7.65f) + 0.125f * num * X.COSI(Mg.t, 5.675f));
				meshImg.Col = meshImg.ColGrd.Set(4280359664U).C;
				meshImg.RotaGraph(0f, 0f, num2 / 15f, 0f, null, false);
				MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
				meshImg2.initForImg(img, 0);
				meshImg2.Col = meshImg2.ColGrd.Set(4285488383U).C;
				num2 = (30f + 80f * X.ZPOW(num - 0.4f, 0.6f)) * (0.68f + 0.16f * num * X.COSI(Mg.t + 99f, 8.13f) + 0.16f * num * X.COSI(Mg.t + 233f, 6.275f));
				meshImg2.RotaGraph(0f, 0f, num2 / 18f, 0f, null, false);
			}
			return true;
		}

		private const float pbomb_v0 = 0.4f;

		private const float pbomb_movet = 50f;
	}
}
