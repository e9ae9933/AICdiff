using System;
using XX;

namespace nel
{
	public class ButtonSkinNelPopper : ButtonSkinPopper
	{
		public ButtonSkinNelPopper(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.Bp = _B as aBtnNelPopper;
			this.BaseCol = C32.MulA(4293321691U, 0.85f);
			this.IconCol = C32.d2c(4283780170U);
			this.HilightCol = C32.d2c(4294966715U);
			this.HilightIconCol = C32.d2c(4278190080U);
			this.BlurCol = C32.MulA(4293321691U, 0.5f);
			this.BlurIconCol = C32.MulA(4283780170U, 0.5f);
		}

		protected override float drawalpha
		{
			get
			{
				return this.alpha_ * ((this.Bp != null) ? this.Bp.salpha : 1f);
			}
		}

		private aBtnNelPopper Bp;
	}
}
