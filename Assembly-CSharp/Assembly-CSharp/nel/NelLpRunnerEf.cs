using System;
using m2d;
using XX;

namespace nel
{
	public abstract class NelLpRunnerEf : NelLpRunner
	{
		public NelLpRunnerEf(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
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
			if (this.FD_fnDrawOnEffect == null)
			{
				this.FD_fnDrawOnEffect = new M2DrawBinder.FnEffectBind(this.fnDrawOnEffect);
			}
			this.Ed = (this.effect_2_ed ? this.Mp.setED(base.unique_key, this.FD_fnDrawOnEffect, 0f) : this.Mp.setEDC(base.unique_key, this.FD_fnDrawOnEffect, 0f));
			this.Ed.t = (float)(IN.totalframe - num);
			this.Ed.f0 = num;
			return this.Ed;
		}

		public override void closeAction(bool when_map_close)
		{
			this.Ed = this.Mp.remED(this.Ed);
			base.closeAction(when_map_close);
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.set_effect_on_initaction)
			{
				this.setEffect();
			}
		}

		protected virtual bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = base.mapcx;
			Ef.y = base.mapcy;
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
