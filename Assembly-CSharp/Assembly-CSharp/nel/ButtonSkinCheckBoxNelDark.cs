using System;
using XX;

namespace nel
{
	public class ButtonSkinCheckBoxNelDark : ButtonSkinCheckBoxNel
	{
		public ButtonSkinCheckBoxNelDark(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			this.border_col = 4283780170U;
			this.border_col_pushdown = 4294966715U;
			this.inner_col = 4291611332U;
			this.inner_col_hovered = 4290689711U;
			this.inner_col_pushdown = 4278190080U;
			this.fill_box_inner = true;
		}

		protected override void fineTextColor()
		{
			ButtonSkinCheckBoxNel.Col.Set(4291611332U);
			if (base.isLocked())
			{
				ButtonSkinCheckBoxNel.Col.Set(4290689711U);
			}
			else if (this.isPushDown())
			{
				ButtonSkinCheckBoxNel.Col.Set(uint.MaxValue);
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinCheckBoxNel.Col.Set(4294966715U);
			}
			this.Tx.Col(ButtonSkinCheckBoxNel.Col.C);
		}
	}
}
