using System;
using PixelLiner;
using XX;

namespace m2d
{
	public class M2CImgDrawerWithEffect : M2CImgDrawer
	{
		public M2CImgDrawerWithEffect(MeshDrawer Md, int _lay, M2Puts _Cp, bool redraw = true)
			: base(Md, _lay, _Cp, redraw)
		{
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (base.Mp.apply_chip_effect && this.no_basic_draw)
			{
				return false;
			}
			base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			return false;
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.Ed = base.Mp.remED(this.Ed);
		}

		public override void initAction(bool normal_map)
		{
			if (this.set_effect_on_initaction)
			{
				this.setEffect();
			}
		}

		protected M2DrawBinder setEffect()
		{
			int num;
			if (this.Ed != null)
			{
				num = this.Ed.f0;
				this.Ed = base.Mp.remED(this.Ed);
			}
			else
			{
				num = IN.totalframe - (int)X.NIXP((float)this.start_af_min, (float)this.start_af_max);
			}
			if (this.FD_fnDrawOnEffect == null)
			{
				this.FD_fnDrawOnEffect = new M2DrawBinder.FnEffectBind(this.fnDrawOnEffect);
			}
			this.Ed = (this.effect_2_ed ? base.Mp.setED(base.unique_key, this.FD_fnDrawOnEffect, 0f) : base.Mp.setEDC(base.unique_key, this.FD_fnDrawOnEffect, 0f));
			this.Ed.t = (float)(IN.totalframe - num);
			this.Ed.f0 = num;
			return this.Ed;
		}

		protected void remEffect()
		{
			if (this.Ed != null)
			{
				this.Ed = base.Mp.remED(this.Ed);
			}
		}

		protected virtual bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.Cp.mapcx;
			Ef.y = this.Cp.mapcy;
			return false;
		}

		protected M2DrawBinder Ed;

		protected bool set_effect_on_initaction = true;

		protected bool effect_2_ed;

		protected bool no_basic_draw;

		protected int start_af_min = 100;

		protected int start_af_max = 300;

		private M2DrawBinder.FnEffectBind FD_fnDrawOnEffect;
	}
}
