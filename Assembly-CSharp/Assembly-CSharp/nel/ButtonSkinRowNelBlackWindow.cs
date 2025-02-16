using System;
using XX;

namespace nel
{
	public class ButtonSkinRowNelBlackWindow : ButtonSkinNelUi
	{
		public ButtonSkinRowNelBlackWindow(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			this.base_col_normal = 0U;
			this.base_col_locked = 855638016U;
			this.base_col_pushdown = 4294966715U;
			this.base_col_hovered0 = C32.c2d(C32.MulA(4290689711U, 0.4f));
			this.base_col_hovered1 = C32.c2d(C32.MulA(4288057994U, 0.4f));
			this.base_col_checked0 = 4291611332U;
			this.base_col_checked1 = 4291611332U;
			this.stripe_col_pushdown = 1727383984U;
			this.stripe_col_hovered = 2293543079U;
			this.tx_col_normal = uint.MaxValue;
			this.tx_col_locked = 4286940549U;
			this.tx_col_pushdown = 4278190080U;
			this.tx_col_hovered = 4294966715U;
			this.tx_col_checked = 4283780170U;
		}
	}
}
