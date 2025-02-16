using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class BonfireEffector
	{
		public BonfireEffector()
		{
		}

		public BonfireEffector(M2Puts Cp)
		{
			this.BaseCp = Cp;
			this.posx = (float)Cp.iwidth * 0.5f;
			this.posx = (float)Cp.iheight * 0.5f;
		}

		public BonfireEffector readMeta(META Meta, string meta_key, int pos_index_first = 0)
		{
			this.posx = Meta.GetNm(meta_key, this.posx, pos_index_first);
			this.posy = Meta.GetNm(meta_key, this.posy, pos_index_first + 1);
			return this;
		}

		public M2DrawBinder setEDC(M2Puts Cp = null, M2DrawBinder.FnEffectBind fnEf = null)
		{
			this.BaseCp = Cp ?? this.BaseCp;
			if (fnEf != null)
			{
				return this.BaseCp.Mp.setEDC("bonfire_c", fnEf, 0f);
			}
			return this.BaseCp.Mp.setEDC("bonfire_c", new M2DrawBinder.FnEffectBind(this.fnDrawToChip), 0f);
		}

		public M2DrawBinder setEDOnEffect(M2Puts Cp = null, M2DrawBinder.FnEffectBind fnEf = null)
		{
			this.BaseCp = Cp ?? this.BaseCp;
			if (fnEf != null)
			{
				return this.BaseCp.Mp.setED("bonfire_ef", fnEf, 0f);
			}
			return this.BaseCp.Mp.setED("bonfire_ef", new M2DrawBinder.FnEffectBind(this.fnDrawOnEffect), 0f);
		}

		public bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Map2d mp = this.BaseCp.Mp;
			float clen = Ed.Mp.CLEN;
			Vector2 vector = this.BaseCp.PixelToMapPoint(this.posx, this.posy);
			Ef.x = vector.x;
			Ef.y = vector.y;
			if (!Ed.isinCamera(Ef, 1f, 2.5f))
			{
				return true;
			}
			Ef.y -= 8f / clen * mp.base_scale;
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
			MeshDrawer meshImg2 = Ef.GetMeshImg("b", MTRX.MIicon, BLEND.ADD, true);
			EffectItemNel.drawFlyingMoth(meshImg, meshImg2, 0f, 0f, mp.floort, 3, 150f, 30f, 22f, 44f, 8f, 4294940715U, 8457475U, this.BaseCp.index * 21, 2f);
			return true;
		}

		public bool fnDrawToChip(EffectItem Ef, M2DrawBinder Ed)
		{
			Vector2 vector = this.BaseCp.PixelToMapPoint(this.posx, this.posy);
			Ef.x = vector.x;
			Ef.y = vector.y;
			if (!Ed.isinCamera(Ef, 1f, 2.5f))
			{
				return true;
			}
			if (this.PtcLight == null)
			{
				this.PtcLight = new EfParticleLooper("cp_bonfire_light");
				this.PtcSmoke = new EfParticleLooper("cp_bonfire_smoke");
			}
			this.PtcLight.Draw(Ef, -1f);
			this.PtcSmoke.Draw(Ef, -1f);
			return true;
		}

		public float posx;

		public float posy;

		private EfParticleLooper PtcLight;

		private EfParticleLooper PtcSmoke;

		private M2Puts BaseCp;
	}
}
