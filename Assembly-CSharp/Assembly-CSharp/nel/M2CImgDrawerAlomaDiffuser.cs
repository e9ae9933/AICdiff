using System;
using m2d;
using XX;

namespace nel
{
	public class M2CImgDrawerAlomaDiffuser : M2CImgDrawerWithEffect, IActivatable
	{
		public M2CImgDrawerAlomaDiffuser(MeshDrawer Md, int lay, M2Puts _Cp)
			: base(Md, lay, _Cp, false)
		{
			this.set_effect_on_initaction = false;
			this.effect_2_ed = true;
			this.EfPSmoke = new EfParticleLooper("trm_aloma_diffuser_smoke");
		}

		public void activate()
		{
			base.setEffect();
		}

		public void deactivate()
		{
			base.remEffect();
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.Cp.mapcx;
			Ef.y = this.Cp.mapcy;
			MeshDrawer meshImg = Ef.GetMeshImg("", this.Cp.IMGS.MIchip, BLEND.ADD, false);
			meshImg.Col = meshImg.ColGrd.White().mulA(0.4f + 0.2f * X.COSI(base.Mp.floort, 55f)).C;
			meshImg.RotaMesh(0f, 0f, base.Mp.base_scale, base.Mp.base_scale, 0f, this.Cp.Img.MeshB, this.Cp.flip, false);
			Ef.y = (float)this.Cp.drawy * this.Cp.rCLEN;
			this.EfPSmoke.Draw(Ef, -1f);
			return true;
		}

		private EfParticleLooper EfPSmoke;
	}
}
