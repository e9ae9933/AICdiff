using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerAlchemyPot : M2CImgDrawerWithEffect
	{
		public M2CImgDrawerAlchemyPot(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
			METACImg meta = this.Cp.getMeta();
			this.posx = meta.GetNm("alchemy_pot", (float)this.Cp.iwidth * 0.5f, 0);
			this.posy = meta.GetNm("alchemy_pot", (float)this.Cp.iheight * 0.75f, 1);
			this.pos_smk_y = meta.GetNm("alchemy_pot", this.posy, 2);
			this.pos_bubble_y = meta.GetNm("alchemy_pot", this.posy, 3);
			this.effect_2_ed = true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.EdPt != null)
			{
				this.EdPt.destruct();
			}
			this.EdPt = base.Mp.setEDC("alchemypot", new M2DrawBinder.FnEffectBind(this.fnDrawToChip), 0f);
			this.V = this.Cp.PixelToMapPoint(this.posx, this.posy);
			this.Vsmk = this.Cp.PixelToMapPoint(this.posx, this.pos_smk_y);
			this.Vbubble = this.Cp.PixelToMapPoint(this.posx, this.pos_bubble_y);
			if (this.PtcLight == null)
			{
				string[] array = base.Meta.Get("ptc_light");
				if (array == null)
				{
					this.PtcLight = new EfParticleLooper("cp_alchemy_pot_light");
				}
				else if (TX.valid(array[0]))
				{
					this.PtcLight = new EfParticleLooper(array[0]);
				}
				array = base.Meta.Get("ptc_smoke");
				if (array == null)
				{
					this.PtcSmoke = new EfParticleLooper("cp_alchemy_pot_smoke");
				}
				else if (TX.valid(array[0]))
				{
					this.PtcSmoke = new EfParticleLooper(array[0]);
				}
				array = base.Meta.Get("ptc_bubble");
				if (array == null)
				{
					this.PtcBubble = new EfParticleLooper("cp_alchemy_pot_bubble");
					return;
				}
				if (TX.valid(array[0]))
				{
					this.PtcBubble = new EfParticleLooper(array[0]);
				}
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
			Ef.x = this.Vsmk.x;
			Ef.y = this.Vsmk.y;
			if (!Ed.isinCamera(Ef, 1f, 2.5f))
			{
				return true;
			}
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
			MeshDrawer meshImg2 = Ef.GetMeshImg("b", MTRX.MIicon, BLEND.ADD, true);
			this.PtcSmoke.Draw(Ef, -1f);
			EffectItemNel.drawFlyingMoth(meshImg, meshImg2, 0f, 12f, base.Mp.floort, 5, 150f, 40f, 22f, 64f, 12f, 4280811339U, 2621259U, this.Cp.index * 13, 2f);
			return true;
		}

		protected bool fnDrawToChip(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.V.x;
			Ef.y = this.V.y;
			if (!Ed.isinCamera(Ef, 1f, 3.5f))
			{
				return true;
			}
			this.PtcLight.Draw(Ef, -1f);
			Ef.x = this.Vbubble.x;
			Ef.y = this.Vbubble.y;
			this.PtcBubble.Draw(Ef, -1f);
			return true;
		}

		private float posx;

		private float posy;

		private float pos_smk_y;

		private float pos_bubble_y;

		private int anm_t = -1;

		private EfParticleLooper PtcLight;

		private EfParticleLooper PtcSmoke;

		private EfParticleLooper PtcBubble;

		protected M2DrawBinder EdPt;

		private Vector2 V;

		private Vector2 Vsmk;

		private Vector2 Vbubble;
	}
}
