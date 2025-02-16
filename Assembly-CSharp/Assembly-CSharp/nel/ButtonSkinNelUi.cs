using System;
using XX;

namespace nel
{
	public class ButtonSkinNelUi : ButtonSkinRow
	{
		public static void setColorBasic(ButtonSkinNelUi B)
		{
			B.base_col_normal = 16775915U;
			B.base_col_locked = 1153482683U;
			B.base_col_pushdown = 4283780170U;
			B.base_col_hovered0 = 4294634216U;
			B.base_col_hovered1 = 4290819509U;
			B.base_col_checked0 = 3428142154U;
			B.base_col_checked1 = 3428142154U;
			B.tx_col_normal = 4283780170U;
			B.tx_col_locked = 4288582804U;
			B.tx_col_pushdown = 4294965995U;
			B.tx_col_hovered = 4286213742U;
			B.tx_col_checked = 4294833122U;
			B.stripe_col_pushdown = 16777215U;
			B.stripe_col_hovered = 3453539274U;
		}

		public ButtonSkinNelUi(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			ButtonSkinNelUi.setColorBasic(this);
		}

		protected override void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
			this.Md.ColGrd.Set(this.tx_col_normal);
			if (base.isLocked())
			{
				this.Md.ColGrd.Set(this.tx_col_locked);
			}
			else if (this.isPushDown())
			{
				this.Md.ColGrd.Set(this.tx_col_pushdown);
			}
			else if (base.isChecked())
			{
				this.Md.ColGrd.Set(this.tx_col_checked);
			}
			else if (base.isHoveredOrPushOut())
			{
				this.Md.ColGrd.blend(this.tx_col_locked, 0.3f + 0.3f * X.COSIT(40f));
			}
			this.Md.Col = this.Md.ColGrd.mulA(this.alpha_).C;
			this.Md.Col = C32.MulA(ButtonSkinRow.Col.C, this.alpha_);
			this.Md.CheckMark(this.w * 64f / 2f - 9f + sht_clk_pixel - (float)((this.show_new_icon_ == 1) ? 22 : 0), -sht_clk_pixel, this.h * 64f * 0.58f, X.Mx(2.5f, this.h * 64f * 0.0625f), false);
			this.Md.Col = MTRX.ColWhite;
		}

		protected override void RowFineAfter(float w, float h)
		{
			if (this.notice_exc_)
			{
				base.prepareIconMesh();
				NEL.QuestNoticeExc(this.MdIco, w / 2f - 30f, -1f);
			}
			if (this.bottom_line)
			{
				this.Md.Col = C32.MulA(4283780170U, this.alpha_ * 0.4f);
				this.Md.Line(-w * 0.5f, -h * 0.5f, w * 0.5f, -h * 0.5f, 1f, false, 0f, 0f);
			}
			base.RowFineAfter(w, h);
		}

		public override void drawBox(MeshDrawer Md, float wpx, float hpx)
		{
			Md.Rect(0f, 0.5f, wpx, hpx, false);
			if (this.hilighted_ && !base.isLocked())
			{
				Md.Col = Md.ColGrd.Set(4294966715U).mulA((base.isHoveredOrPushOut() || this.isPushDown() || base.isChecked()) ? (this.alpha_ * 0.5f) : this.alpha_).C;
				Md.ColGrd.setA(0f);
				Md.RectBLGradation(0f - wpx * 0.5f, 0.5f - hpx * 0.5f, wpx * 0.875f, hpx, GRD.LEFT2RIGHT, false);
			}
		}

		protected override void fineDrawAdditional(ref float shift_px_x, ref float shift_px_y, ref bool shift_y_abs)
		{
			if (this.show_new_icon_ == 1)
			{
				this.Md.Col = C32.MulA(4294926244U, this.alpha_);
				this.Md.Poly(this.w * 64f * 0.5f - (this.h * 64f * 0.2f + 4f), 0f, this.h * 64f * 0.08f + 1f, 0f, 16, 0f, false, 0f, 0f);
			}
		}

		public bool hilighted
		{
			get
			{
				return this.hilighted_;
			}
			set
			{
				if (this.hilighted_ == value)
				{
					return;
				}
				this.hilighted_ = value;
				this.fine_flag = true;
			}
		}

		public bool notice_exc
		{
			get
			{
				return this.notice_exc_;
			}
			set
			{
				if (this.notice_exc == value)
				{
					return;
				}
				this.notice_exc_ = value;
				if (value)
				{
					this.fine_continue_flags |= 31U;
				}
			}
		}

		public bool bottom_line
		{
			get
			{
				return this.bottom_line_;
			}
			set
			{
				if (this.bottom_line_ == value)
				{
					return;
				}
				this.bottom_line_ = value;
				this.fine_flag = true;
			}
		}

		public byte show_new_icon
		{
			get
			{
				return this.show_new_icon_;
			}
			set
			{
				if (this.show_new_icon == value)
				{
					return;
				}
				this.show_new_icon_ = value;
				this.fine_flag = true;
			}
		}

		private bool hilighted_;

		private bool notice_exc_;

		protected byte show_new_icon_;

		private bool bottom_line_;
	}
}
