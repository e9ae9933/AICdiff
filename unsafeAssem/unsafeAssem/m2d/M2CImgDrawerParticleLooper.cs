using System;
using PixelLiner;
using XX;

namespace m2d
{
	public class M2CImgDrawerParticleLooper : M2CImgDrawer
	{
		public M2CImgDrawerParticleLooper(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, false)
		{
			this.Plp = new EfParticleLooperZT(Meta.GetS("particle_loop"));
			this.Plp.z = Meta.GetNm("particle_loop", 0f, 1);
			this.Plp.time = (int)Meta.GetNm("particle_loop", 0f, 2);
			this.loop_maxt = Meta.GetNm("particle_loop", this.Plp.sum_delay, 3);
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (base.Mp.apply_chip_effect && this.Cp.Img.family == "no_draw")
			{
				return false;
			}
			base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			return false;
		}

		public override void initAction(bool normal_map)
		{
			if (this.Ed != null)
			{
				this.Ed = base.Mp.remED(this.Ed);
			}
			this.Ed = (base.Meta.GetB("particle_ef", false) ? base.Mp.setED(base.unique_key, new M2DrawBinder.FnEffectBind(this.edDraw), 0f) : base.Mp.setEDC(base.unique_key, new M2DrawBinder.FnEffectBind(this.edDraw), 0f));
		}

		public override void closeAction(bool when_map_close)
		{
			this.Ed = base.Mp.remED(this.Ed);
		}

		public bool edDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!Ed.isinCamera(this.Cp.mapcx, this.Cp.mapcy, 1.5f, 1.5f))
			{
				return true;
			}
			Ef.x = this.Cp.mapcx;
			Ef.y = this.Cp.mapcy;
			this.Plp.Draw(Ef, this.loop_maxt);
			return true;
		}

		private readonly EfParticleLooperZT Plp;

		private float loop_maxt;

		private M2DrawBinder Ed;
	}
}
