using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipWaterShooter : NelChipThunderShooter
	{
		public NelChipWaterShooter(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			this.MK = new MistManager.MistKind(MistManager.MISTTYPE.LAVA);
			this.no_draw_on_reach_len_is_zero = false;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Mp.NM == null)
			{
				return;
			}
			this.pre_reach_len = -2f;
			this.water_touched = false;
			this.Ext.mist_passable = false;
			this.Ext.ChipCheckingFn = (int mapx, int mapy, M2Pt Pt) => CCON.mistPassable(Pt.cfg, 0) && !CCON.isWater(Pt.cfg);
		}

		public override void fnReachFined(bool initted, bool changed)
		{
			if (this.Ext == null)
			{
				return;
			}
			base.fnReachFined(initted, changed);
			if (changed || initted)
			{
				if (this.pre_reach_len == -2f)
				{
					this.pre_reach_len = -1f;
				}
				else
				{
					this.pre_reach_len = this.cur_reach_len;
				}
			}
			this.cur_reach_len = this.Ext.reachable_len;
			this.draw_margin_mapy = 2f + X.Mx(this.cur_reach_len, this.pre_reach_len);
			int num = X.IntR(this.Ext.shot_dy_cur(this.cur_reach_len)) - ((this.Ext.aim == AIM.T) ? 1 : 0);
			this.water_touched = CCON.isWater(this.Mp.getConfig((int)this.Ext.shot_sx, num));
			if (this.water_touched)
			{
				if (NelChipWaterShooter.PtcZT_inject == null)
				{
					NelChipWaterShooter.PtcZT_inject = new EfParticleOnce("wmng_fall_inject", EFCON_TYPE.UI);
				}
				this.PtcZT = NelChipWaterShooter.PtcZT_inject;
				this.PtcZT.z = 42f;
				return;
			}
			if (NelChipWaterShooter.PtcZT_hit == null)
			{
				NelChipWaterShooter.PtcZT_hit = new EfParticleOnce("wmng_fall_hit", EFCON_TYPE.UI);
			}
			this.PtcZT = NelChipWaterShooter.PtcZT_hit;
			this.PtcZT.z = 20f;
		}

		protected override void createMapDamageInstance()
		{
			this.MDI = base.M2D.MDMGCon.Create(MAPDMG.LAVA, this.Ext.cx, this.Ext.cy, 0f, 0f, null);
		}

		protected override bool fnDrawThunderEffectInner(EffectItem Ef, M2DrawBinder Ed, float dw, float dh)
		{
			MeshDrawer meshDrawer = Ef.GetMesh("", this.Mp.Dgn.getWaterMaterial(), false);
			meshDrawer.base_x = (meshDrawer.base_y = 0f);
			meshDrawer.Scale(this.Mp.base_scale, this.Mp.base_scale, false);
			float num = 1f;
			if (this.pre_reach_len > 0f)
			{
				float num2 = this.Mp.floort - this.fined_floort;
				if (num2 < 20f)
				{
					num = X.ZSIN(num2, 20f);
					M2WaterWatcher.drawWaterFall(meshDrawer, this.Mp, this.mapx, this.clms, (int)this.Ext.shot_sy, 1f - num, this.pre_reach_len - 1f, this.MK.color0, this.MK.color1, true, 1U, false);
					num = X.Scr(num, 0.5f);
				}
				else
				{
					this.pre_reach_len = -1f;
				}
			}
			float num3 = (this.water_touched ? 0.3f : 0f);
			if (this.cur_reach_len > 0f)
			{
				M2WaterWatcher.drawWaterFall(meshDrawer, this.Mp, this.mapx, this.clms, (int)this.Ext.shot_sy, num, num3 + this.cur_reach_len - 1f, this.MK.color0, this.MK.color1, true, 3U, false);
			}
			if (this.PtcZT != null)
			{
				Ef.x = this.mapcx;
				Ef.y = this.Ext.shot_dy_cur(this.cur_reach_len) - (float)CAim._YD(this.aim, 1) * num3 * 0.5f;
				meshDrawer = Ef.GetMesh("", MTRX.MtrMeshNormal, false);
				this.PtcZT.drawTo(meshDrawer, Ef.af, 20f);
			}
			return true;
		}

		protected override AttackInfo GetAtk(M2Attackable MvA, out float efx, out float efy, bool set_burst_vx = true, bool set_burst_vy = true)
		{
			AttackInfo atk = base.GetAtk(MvA, out efx, out efy, set_burst_vx, false);
			if (atk != null)
			{
				atk.burst_vy = 0f;
			}
			return atk;
		}

		protected override void afterDamageApplied(M2Attackable MvA)
		{
			base.afterDamageApplied(MvA);
			if (MvA is PR)
			{
				(MvA as PR).Ser.Add(SER.BURNED, -1, 1, false);
			}
			M2Phys physic = MvA.getPhysic();
			if (physic != null)
			{
				physic.addFoc(FOCTYPE.HIT | FOCTYPE._GRAVITY_LOCK | FOCTYPE._CHECK_WALL, 0f, -0.2f, -1f, 0, 5, 18, -1, 0);
			}
		}

		public override string areasnd_cue
		{
			get
			{
				return "areasnd_river";
			}
		}

		public override string main_tag_name
		{
			get
			{
				return "lavawater_shooter";
			}
		}

		public float pre_reach_len = -1000f;

		public float cur_reach_len;

		private MistManager.MistKind MK;

		public bool water_touched;

		private EfParticleOnce PtcZT;

		private static EfParticleOnce PtcZT_inject;

		private static EfParticleOnce PtcZT_hit;
	}
}
