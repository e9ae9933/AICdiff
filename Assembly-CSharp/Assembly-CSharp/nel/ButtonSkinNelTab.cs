using System;
using XX;

namespace nel
{
	public class ButtonSkinNelTab : ButtonSkinNelUi
	{
		public ButtonSkinNelTab(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.alloc_auto_wrap = true;
			this.base_col_normal = 3431038075U;
			this.base_col_checked0 = (this.base_col_checked1 = 4294965995U);
			this.tx_col_checked = this.tx_col_normal;
			this.alignx_ = ALIGN.CENTER;
			this.fix_text_size_ = (float)(TX.isEnglishLang() ? 13 : 18);
			this.row_left_px = (this.row_right_px = 10);
		}

		protected override void setTitleText(string str)
		{
			base.setTitleText(str);
			if (this.Tx != null && TX.isEnglishLang())
			{
				this.Tx.LetterSpacing(0.96f);
				this.Tx.LineSpacing(0.8f);
			}
		}

		protected override void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
		}

		public override void drawBox(MeshDrawer Md, float w, float h)
		{
			w -= 2f;
			float num = w * 0.5f;
			float num2 = h * 0.5f;
			float num3 = h * 0.33f;
			if (!base.isChecked())
			{
				ButtonSkinNelTab.drawHalfKadomaru(Md, num, num2, num3);
			}
			if (!base.isLocked())
			{
				Md.Col = C32.MulA(4283780170U, this.alpha_);
				Md.Line(-num, -num2, -num, num2 - num3, 1f, false, 0f, 0f);
				Md.Line(num, -num2, num, num2 - num3, 1f, false, 0f, 0f);
				Md.Line(-num + num3, num2, num - num3, num2, 1f, false, 0f, 0f);
				Md.Arc(-num + num3, num2 - num3, num3, 1.5707964f, 3.1415927f, 1f);
				Md.Arc(num - num3, num2 - num3, num3, 0f, 1.5707964f, 1f);
			}
		}

		public static void drawHalfKadomaru(MeshDrawer Md, float wh, float hh, float rd)
		{
			float num = wh * 0.015625f;
			float num2 = hh * 0.015625f;
			float num3 = rd * 0.015625f;
			int num4 = X.Mx(3, (int)(rd * 6.2831855f / 4f));
			float num5 = 1.5707964f / (float)num4;
			Md.Pos(0f, -num2, null);
			int num6 = 4 + num4 * 2;
			for (int i = 0; i < num6 - 1; i++)
			{
				Md.Tri(-1, i, i + 1, false);
			}
			float num7 = num2 - num3;
			Md.Pos(-num, -num2, null);
			Md.Pos(-num, num7, null);
			float num8 = 3.1415927f;
			for (int j = 1; j < num4; j++)
			{
				num8 -= num5;
				Md.Pos(-num + num3 + num3 * X.Cos(num8), num7 + num3 * X.Sin(num8), null);
			}
			Md.Pos(-num + num3, num2, null);
			Md.Pos(num - num3, num2, null);
			num8 = 1.5707964f;
			for (int k = 1; k < num4; k++)
			{
				num8 -= num5;
				Md.Pos(num - num3 + num3 * X.Cos(num8), num7 + num3 * X.Sin(num8), null);
			}
			Md.Pos(num, num7, null);
			Md.Pos(num, -num2, null);
		}

		protected override void RowFineAfter(float w, float h)
		{
			if (this.new_circle_)
			{
				this.Md.Col = C32.MulA(4294926244U, this.alpha_ * 1f);
				this.Md.Circle(w * 0.5f - h * 0.42f, 0f, 3f, 0f, false, 0f, 0f);
			}
			base.RowFineAfter(w, h);
		}

		public bool new_circle
		{
			get
			{
				return this.new_circle_;
			}
			set
			{
				if (this.new_circle_ != value)
				{
					this.new_circle_ = value;
					this.fine_flag = true;
				}
			}
		}

		public const float TAB_DEFAULT_H = 28f;

		private bool new_circle_;
	}
}
