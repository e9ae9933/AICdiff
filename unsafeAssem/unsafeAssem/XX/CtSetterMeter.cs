using System;

namespace XX
{
	public class CtSetterMeter : CtSetter
	{
		public CtSetterMeter(aBtnMeter _Meter)
			: base(_Meter.transform.parent.gameObject, "-CtSetter")
		{
			this.Meter = _Meter;
			using (STB stb = TX.PopBld(null, 0))
			{
				this.fineValue(stb.Add(this.Meter.getValue()), false);
			}
			if (this.Meter.Container != null)
			{
				base.stencil_ref = this.Meter.Container.stencil_ref;
			}
			this.updown_move_scale = 0f;
			this.Meter.addChangedFn(new aBtnMeter.FnMeterBindings(this.fnMeterChange));
		}

		private bool fnMeterChange(aBtnMeter _B, float pre_value, float cur_value)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				this.fineValue(stb.Add(cur_value), false);
			}
			return true;
		}

		protected override bool moveToLeft(float scale = 1f)
		{
			return CtSetterMeter.moveToLeftS(this.Meter, scale);
		}

		protected override bool moveToRight(float scale = 1f)
		{
			return CtSetterMeter.moveToRightS(this.Meter, scale);
		}

		public static bool moveToLeftS(aBtnMeter Meter, float scale)
		{
			return !Meter.isLocked() && Meter.getValue() > Meter.minval && Meter.setValueAndCallFunc(X.Mx(Meter.getValue() - Meter.valintv * scale, Meter.minval), false);
		}

		public static bool moveToRightS(aBtnMeter Meter, float scale)
		{
			return !Meter.isLocked() && Meter.getValue() < Meter.maxval && Meter.setValueAndCallFunc(X.Mn(Meter.getValue() + Meter.valintv * scale, Meter.maxval), false);
		}

		protected override void deactivateAndSelect()
		{
			base.deactivate();
			((this.SelectedOverride == null) ? this.Meter : this.SelectedOverride).Select(true);
		}

		public override void fineValue(STB Stb, bool set_to_element = false)
		{
			STB.PARSERES parseres;
			if (Stb.Nm(out parseres))
			{
				float num = (float)STB.NmRes(parseres, -1.0);
				this.Meter.getDescForValue(Stb, num);
				base.fineValue(Stb, set_to_element);
				if (set_to_element)
				{
					this.Meter.setValue(num, false);
				}
			}
		}

		public override bool isEnableMoving()
		{
			return ((this.SelectedOverride == null) ? this.Meter : this.SelectedOverride).isSelected();
		}

		public override string focus_curs_category
		{
			get
			{
				return this.Meter.focus_curs_category;
			}
		}

		public readonly aBtnMeter Meter;

		public aBtn SelectedOverride;
	}
}
