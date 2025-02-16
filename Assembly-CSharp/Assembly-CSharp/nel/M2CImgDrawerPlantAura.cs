using System;
using m2d;
using XX;

namespace nel
{
	public class M2CImgDrawerPlantAura : M2CImgDrawer
	{
		public M2CImgDrawerPlantAura(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, false)
		{
			if (M2CImgDrawerPlantAura.PtcDB == null)
			{
				M2CImgDrawerPlantAura.PtcDB = new EfParticleOnce("grazia_plant_aura_big", EFCON_TYPE.FIXED);
				M2CImgDrawerPlantAura.PtcDS = new EfParticleOnce("grazia_plant_aura_sml", EFCON_TYPE.FIXED);
			}
			this.ptc_count = X.IntR((float)Meta.GetI("plant_aura", 10, 0) * X.NIXP(0.6f, 1f));
			this.ptc_size = (float)X.IntR(Meta.GetNm("plant_aura", (float)this.Cp.Img.iwidth * 0.5f, 1) * X.NIXP(0.68f, 1.2f));
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Ed != null)
			{
				this.Ed = base.Mp.remED(this.Ed);
			}
			this.Ed = base.Mp.setEDC(base.unique_key, new M2DrawBinder.FnEffectBind(this.fnDrawAuraLight), 0f);
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (this.Ed != null)
			{
				this.Ed = base.Mp.remED(this.Ed);
			}
		}

		private bool fnDrawAuraLight(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.Cp.mapcx;
			Ef.y = this.Cp.mapcy;
			if (!Ed.isinCamera(Ef, 6f, 6f))
			{
				return true;
			}
			MeshDrawer mesh = Ef.GetMesh("", MTRX.MtrMeshAdd, false);
			float base_floort = base.Mp.base_floort;
			M2CImgDrawerPlantAura.PtcDB.drawTo(mesh, mesh.base_px_x, mesh.base_px_y, this.ptc_size, 4, base_floort + 90f + (float)(this.Cp.index * 13), M2CImgDrawerPlantAura.PtcDB.delay * 4f);
			M2CImgDrawerPlantAura.PtcDS.drawTo(mesh, mesh.base_px_x, mesh.base_px_y, this.ptc_size * 0.5f, this.ptc_count, base_floort + 66f + (float)(this.Cp.index * 19), M2CImgDrawerPlantAura.PtcDS.delay * (float)this.ptc_count);
			return true;
		}

		private int ptc_count;

		private float ptc_size;

		private M2DrawBinder Ed;

		private static EfParticleOnce PtcDB;

		private static EfParticleOnce PtcDS;
	}
}
