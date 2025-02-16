using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerFirePlace : M2CImgDrawerWithEffect
	{
		public M2CImgDrawerFirePlace(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
			METACImg meta = this.Cp.getMeta();
			this.posx = meta.GetNm("fire_place", (float)this.Cp.iwidth * 0.5f, 0);
			this.posy = meta.GetNm("fire_place", (float)this.Cp.iheight * 0.5f, 1);
			this.effect_2_ed = true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.EdPt != null)
			{
				this.EdPt.destruct();
			}
			this.EdPt = base.Mp.setEDC("fireplace_c", new M2DrawBinder.FnEffectBind(this.fnDrawToChip), 0f);
			this.V = this.Cp.PixelToMapPoint(this.posx, this.posy);
			if (this.PtcLight == null)
			{
				this.PtcLight = new EfParticleLooper("cp_bonfire_light");
				this.PtcSmoke = new EfParticleLooper("cp_bonfire_smoke");
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.EdPt = base.Mp.remED(this.EdPt);
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.V.x;
			Ef.y = this.V.y;
			if (!Ed.isinCamera(Ef, 1f, 2.5f))
			{
				return true;
			}
			Ef.y -= 8f / base.CLEN * base.Mp.base_scale;
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
			MeshDrawer meshImg2 = Ef.GetMeshImg("b", MTRX.MIicon, BLEND.ADD, true);
			EffectItemNel.drawFlyingMoth(meshImg, meshImg2, 0f, 0f, base.Mp.floort, 3, 150f, 30f, 22f, 44f, 8f, 4294940715U, 8457475U, this.Cp.index * 21, 2f);
			return true;
		}

		protected bool fnDrawToChip(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.V.x;
			Ef.y = this.V.y;
			if (!Ed.isinCamera(Ef, 1f, 2.5f))
			{
				return true;
			}
			this.PtcLight.Draw(Ef, -1f);
			this.PtcSmoke.Draw(Ef, -1f);
			return true;
		}

		private float posx;

		private float posy;

		private int anm_t = -1;

		private EfParticleLooper PtcLight;

		private EfParticleLooper PtcSmoke;

		protected M2DrawBinder EdPt;

		private Vector2 V;
	}
}
