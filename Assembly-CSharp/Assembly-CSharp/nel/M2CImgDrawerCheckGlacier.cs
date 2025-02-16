using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerCheckGlacier : M2CImgDrawerWithEffect, IActivatable
	{
		public M2CImgDrawerCheckGlacier(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
			METACImg meta = this.Cp.getMeta();
			this.posx = meta.GetNm("check_glacier", (float)this.Cp.iwidth * 0.5f, 0);
			this.posy = meta.GetNm("check_glacier", (float)this.Cp.iheight * 0.5f, 1);
			this.start_af_min = 80;
			this.start_af_max = 130;
			this.set_effect_on_initaction = false;
			this.Cp.arrangeable = true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.PtcLight == null)
			{
				this.PtcLight = new EfParticleLooper("cp_check_glacier_light");
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
		}

		public void activate()
		{
			Vector2 vector = this.Cp.PixelToMapPoint(this.posx, this.posy);
			base.setEffect();
			base.Mp.PtcSTsetVar("x", (double)vector.x).PtcSTsetVar("y", (double)vector.y).PtcST("checkpoint_registered_glacier", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public void deactivate()
		{
			this.Ed = base.Mp.remED(this.Ed);
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Vector2 vector = this.Cp.PixelToMapPoint(this.posx, this.posy);
			Ef.x = vector.x;
			Ef.y = vector.y;
			if (!Ed.isinCamera(Ef, 1f, 3.5f))
			{
				return true;
			}
			this.PtcLight.Draw(Ef, -1f);
			return true;
		}

		private float posx;

		private float posy;

		private EfParticleLooper PtcLight;
	}
}
