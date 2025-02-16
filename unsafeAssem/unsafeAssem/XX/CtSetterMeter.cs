using System;

namespace XX
{
	public class CtSetterMeter : CtSetter
	{
		public CtSetterMeter(aBtnMeter _Meter)
			: base(_Meter.transform.parent.gameObject, "-CtSetter")
		{
			this.Meter = _Meter;
			this.fineValue(this.Meter.getValue().ToString(), false);
			if (this.Meter.Container != null)
			{
				base.stencil_ref = this.Meter.Container.stencil_ref;
			}
			this.updown_move_scale = 0f;
			this.Meter.addChangedFn(new aBtnMeter.FnMeterBindings(this.fnMeterChange));
		}

		private bool fnMeterChange(aBtnMeter _B, float pre_value, float cur_value)
		{
			this.fineValue(cur_value.ToString(), false);
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
			((this.SelectedOverride == null) ? this.Meter : this.SelectedOverride).Select(false);
		}

		public override void fineValue(string val, bool set_to_element = false)
		{
			base.fineValue(this.Meter.getDescForValue(X.Nm(val, 0f, false)), set_to_element);
			if (set_to_element)
			{
				this.Meter.setValue(val);
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
