using System;
using XX;

namespace nel
{
	public class ButtonSkinCheckBoxNel : ButtonSkinCheckBox
	{
		public ButtonSkinCheckBoxNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			if (ButtonSkinCheckBoxNel.Col == null)
			{
				ButtonSkinCheckBoxNel.Col = new C32();
			}
			this.PFCheck = MTRX.getPF("nel_check");
			this.border_col = 4293321691U;
			this.border_col_pushdown = 4291611332U;
			this.inner_col = 4283780170U;
			this.inner_col_hovered = 4288057994U;
			this.inner_col_pushdown = 4294966715U;
		}

		protected virtual void fineTextColor()
		{
			ButtonSkinCheckBoxNel.Col.Set(4283780170U);
			if (base.isLocked())
			{
				ButtonSkinCheckBoxNel.Col.Set(4288057994U);
			}
			else if (this.isPushDown())
			{
				ButtonSkinCheckBoxNel.Col.Set(3707764736U);
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinCheckBoxNel.Col.Set(4283780170U);
			}
			this.Tx.Col(ButtonSkinCheckBoxNel.Col.C);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 64f;
			float num2 = this.h * 64f - 1f;
			float num3 = (float)(this.isPushDown() ? 2 : 0);
			this.fineTextColor();
			IN.PosP(this.Tx.transform, -num / 2f + num2 / 2f * this.scale + num3 + this.text_margin, -num3, -0.08f);
			return base.Fine();
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			if (this.Tx == null)
			{
				this.Tx = base.MakeTx("-text");
				this.Tx.Size(18f).Align(ALIGN.LEFT).AlignY(ALIGNY.MIDDLE);
			}
			if (this.B.Container != null)
			{
				this.Tx.StencilRef(this.B.Container.stencil_ref);
			}
			this.Tx.Txt(X.T2K(str)).LetterSpacing(0.95f).Alpha(this.alpha_)
				.LineSpacing(1.02f);
			this.Tx.html_mode = true;
			this.default_title_width = this.Tx.get_swidth_px();
			if (this.default_title_width > this.w * 64f - 30f)
			{
				this.Tx.Size(14f);
				this.Tx.max_swidth_px = this.w * 64f - 30f;
				this.Tx.auto_wrap = true;
				this.Tx.redraw_flag = true;
				this.default_title_width = this.Tx.get_swidth_px();
			}
			this.fine_flag = true;
			return this;
		}

		public override float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ != value)
				{
					base.alpha = value;
					if (this.Tx != null)
					{
						this.Tx.Alpha(this.alpha_);
					}
				}
			}
		}

		public override float swidth
		{
			get
			{
				return this.w * 64f;
			}
		}

		public override float sheight
		{
			get
			{
				return X.Mx(base.sheight, (this.Tx != null) ? this.Tx.get_sheight_px() : 0f);
			}
		}

		public float text_margin
		{
			get
			{
				if (!(this.Tx != null))
				{
					return 0f;
				}
				return this.Tx.size * 0.25f;
			}
		}

		protected TextRenderer Tx;

		public static C32 Col;
	}
}
