using System;
using XX;

namespace nel
{
	public class ButtonSkinRowNelDark : ButtonSkinNelUi
	{
		public static void setColorDark(ButtonSkinNelUi B)
		{
			B.base_col_normal = 4283780170U;
			B.base_col_locked = 4288057994U;
			B.base_col_pushdown = 4294966715U;
			B.base_col_hovered0 = 4290689711U;
			B.base_col_hovered1 = 4288057994U;
			B.base_col_checked0 = 4291611332U;
			B.base_col_checked1 = 4291611332U;
			B.stripe_col_pushdown = 1727383984U;
			B.stripe_col_hovered = 2293543079U;
			B.tx_col_normal = 4294175976U;
			B.tx_col_locked = 4292072403U;
			B.tx_col_pushdown = 4278190080U;
			B.tx_col_hovered = uint.MaxValue;
			B.tx_col_checked = 4283780170U;
		}

		public ButtonSkinRowNelDark(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			ButtonSkinRowNelDark.setColorDark(this);
		}
	}
}
