using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipWithEffect : NelChip
	{
		public NelChipWithEffect(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			this.FD_fnDrawOnEffect = new M2DrawBinder.FnEffectBind(this.fnDrawOnEffect);
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			this.Ed = this.Mp.remED(this.Ed);
			base.closeAction(when_map_close, do_not_remove_drawer);
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.set_effect_on_initaction)
			{
				this.setEffect();
			}
		}

		protected virtual M2DrawBinder setEffect()
		{
			int num;
			if (this.Ed != null)
			{
				num = this.Ed.f0;
				this.Ed = this.Mp.remED(this.Ed);
			}
			else
			{
				num = IN.totalframe - (int)X.NIXP((float)this.start_af_min, (float)this.start_af_max);
			}
			this.Ed = (this.effect_2_ed ? this.Mp.setED(base.unique_key, this.FD_fnDrawOnEffect, 0f) : this.Mp.setEDC(base.unique_key, this.FD_fnDrawOnEffect, 0f));
			this.Ed.t = (float)(IN.totalframe - num);
			this.Ed.f0 = num;
			return this.Ed;
		}

		protected virtual bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.mapcx;
			Ef.y = this.mapcy;
			return false;
		}

		protected M2DrawBinder Ed;

		protected bool set_effect_on_initaction = true;

		protected bool effect_2_ed;

		protected bool no_basic_draw;

		protected int start_af_min = 100;

		protected int start_af_max = 300;

		private readonly M2DrawBinder.FnEffectBind FD_fnDrawOnEffect;
	}
}
