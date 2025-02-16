using System;
using PixelLiner;

namespace XX
{
	public class ButtonSkinRowToggle : ButtonSkinRow
	{
		public ButtonSkinRowToggle(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.PFToggle = MTRX.getPF("toggle_left");
			this.PFOpened = MTRX.getPF("toggle_down");
			base.addIcon(this.PFOpened, -1);
			this.toggle_icon_index = this.AIcons.Count - 1;
			this.fine_on_binding_changing = false;
			this.tx_col_normal = 4283256141U;
			this.tx_col_locked = 3127273062U;
			this.tx_col_pushdown = uint.MaxValue;
			this.tx_col_hovered = 4286414994U;
			this.tx_col_checked = 4278256652U;
			this.hilighted = true;
		}

		public bool hilighted
		{
			get
			{
				return this.hilighted_;
			}
			set
			{
				if (this.hilighted == value)
				{
					return;
				}
				this.hilighted_ = value;
				this.fine_flag = true;
				if (value)
				{
					this.base_col_normal = 4283676159U;
					this.base_col_locked = 3719411926U;
					this.base_col_pushdown = 4283126271U;
					this.base_col_hovered0 = 4287753727U;
					this.base_col_hovered1 = 3724541951U;
					this.base_col_checked0 = 4288845809U;
					this.base_col_checked1 = 3722767615U;
					return;
				}
				this.base_col_normal = 4288916672U;
				this.base_col_locked = 3719217346U;
				this.base_col_pushdown = 4288454078U;
				this.base_col_hovered0 = 4289512898U;
				this.base_col_hovered1 = 3724541951U;
				this.base_col_checked0 = 4291216612U;
				this.base_col_checked1 = 3723358195U;
			}
		}

		public override ButtonSkin Fine()
		{
			if (base.isChecked() != (this.AIcons[this.toggle_icon_index] == this.PFOpened))
			{
				this.AIcons[this.toggle_icon_index] = (base.isChecked() ? this.PFOpened : this.PFToggle);
			}
			return base.Fine();
		}

		protected override void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
		}

		protected override void setTitleText(string str)
		{
			base.setTitleText(str);
			this.Tx.SizeFromHeight(this.sheight * 0.8f, 0.125f);
		}

		private int toggle_icon_index;

		private PxlFrame PFOpened;

		private PxlFrame PFToggle;

		private bool hilighted_;
	}
}
