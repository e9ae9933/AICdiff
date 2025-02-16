using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class MgItemBombNormal : MgItemBomb
	{
		public MgItemBombNormal()
		{
			this.col_stroke_add0 = 4294119531U;
			this.col_stroke_add1 = 15929451U;
			this.col_stroke_sub0 = 2568080882U;
			this.col_stroke_sub1 = 1146610U;
			this.col_explode_sub_circle = 2004926975U;
			this.ptc_key_explode = "itembomb_explode";
			this.ptc_key_explode_prepare = "itembomb_explode_prepare";
			this.snd_key_dro = "itembomb_drop";
			this.explode_radius_min = 2.9f;
			this.explode_radius_max = 3.3f;
		}

		protected override void InitItemInner(MgItemBomb.MgBombMem Mem, float gratio)
		{
			NelAttackInfo atk = Mem.Mg.Atk0;
			atk.hpdmg0 = (int)X.NI(220, 350, gratio);
			atk.split_mpdmg = 5;
			atk.ignore_nodamage_time = true;
			atk.pr_myself_fire = true;
			atk.attr = MGATTR.BOMB;
			atk.burst_vx = 1.4f;
			atk.burst_vy = -0.8f;
			atk.attack_max0 = -1;
			atk.burst_center = 1f;
			atk.huttobi_ratio = 1000f;
			atk.centerblur_damage_ratio = 0.5f;
			atk.ndmg = NDMG.GRAB_PENETRATE;
			atk.SerDmg = Mem.Mg.MGC.makeSDmg().Add(SER.EATEN, 50f);
			atk.Torn(0.5f, 1.3f);
			atk.Beto = BetoInfo.Lava;
			atk.damage_randomize_min = 0.95f;
			Mem.Mg.phase = 100;
			Mem.Mg.Mn._1.thick = X.NI(this.explode_radius_min, this.explode_radius_max, gratio);
		}

		protected override bool ReflectCheck(MagicItem Mg, HITTYPE hitres, int fphase)
		{
			if (!base.ReflectCheck(Mg, hitres, fphase))
			{
				return false;
			}
			if (Mg.Ray.ReflectAnotherRay != null && Mg.Ray.ReflectAnotherRay.Atk is NelAttackInfo)
			{
				NelAttackInfo nelAttackInfo = Mg.Ray.ReflectAnotherRay.Atk as NelAttackInfo;
				MGATTR attr = nelAttackInfo.attr;
				if ((attr == MGATTR.BOMB && (nelAttackInfo.PublishMagic == null || nelAttackInfo.PublishMagic.kind != MGKIND.DROPBOMB)) || attr == MGATTR.FIRE)
				{
					return false;
				}
			}
			return true;
		}

		protected override bool runFlying(MagicItem Mg, float fcnt, bool on_ground)
		{
			if (!base.runFlying(Mg, fcnt, on_ground))
			{
				return false;
			}
			MgItemBombNormal.runFlyingLavaExp(Mg, fcnt, on_ground);
			return true;
		}

		public static bool runFlyingLavaExp(MagicItem Mg, float fcnt, bool on_ground)
		{
			if (on_ground)
			{
				M2BlockColliderContainer.BCCLine footBcc = Mg.Dro.get_FootBcc();
				if (footBcc != null)
				{
					List<IBCCFootListener> afootLsnRegistered = footBcc.AFootLsnRegistered;
				}
			}
			if (CCON.isWater(Mg.Mp.getConfig((int)Mg.Dro.x, (int)Mg.Dro.y)))
			{
				M2WaterWatcher waterInfo = Mg.M2D.MIST.getWaterInfo(Mg.Dro.x, Mg.Dro.y, true);
				if (waterInfo != null && waterInfo.MistK.type == MistManager.MISTTYPE.LAVA)
				{
					return false;
				}
			}
			return true;
		}

		public override void applyMapDamage(MagicItem Mg, M2MapDamageContainer.M2MapDamageItem MapDmg, M2BlockColliderContainer.BCCLine Bcc)
		{
			if (MapDmg.Atk.attr == MGATTR.FIRE)
			{
				base.initExplode(Mg);
			}
		}

		public override void drawExplodePrepare(MagicItem Mg, MeshDrawer MdAdd, float count_tz, float r, float scl)
		{
			float num = Mg.Mn._1.thick / this.explode_radius_min;
			UniBrightDrawer uniBrightDrawer = MTRX.UniBright.Count(X.IntC(count_tz * (4f + 4f * num))).RotTime(550f, 780f).Col(C32.MulA(4291476561U, count_tz), C32.MulA(15933472U, count_tz));
			uniBrightDrawer.Radius(r * scl + 20f, r * scl + 25f).CenterCicle(30f * scl, 40f * scl, 160f).Thick(20f * num, 33f * num);
			uniBrightDrawer.drawTo(MdAdd, 0f, 0f, Mg.t, false);
		}
	}
}
