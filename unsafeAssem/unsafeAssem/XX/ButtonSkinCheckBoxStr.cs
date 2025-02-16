using System;

namespace XX
{
	public class ButtonSkinCheckBoxStr : ButtonSkinCheckBox
	{
		public ButtonSkinCheckBoxStr(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			if (ButtonSkinCheckBoxStr.Col == null)
			{
				ButtonSkinCheckBoxStr.Col = new C32();
			}
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
			ButtonSkinCheckBoxStr.Col.Set(4294309365U);
			if (base.isLocked())
			{
				ButtonSkinCheckBoxStr.Col.Set(3128391543U);
			}
			else if (this.isPushDown())
			{
				ButtonSkinCheckBoxStr.Col.Set(4291024888U);
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinCheckBoxStr.Col.Set(4287009521U);
			}
			if (this.Tx != null)
			{
				this.Tx.Col(ButtonSkinCheckBoxStr.Col.C);
				IN.PosP(this.Tx.transform, -num / 2f + num2 / 2f * this.scale + num3 + this.text_margin, -num3, -0.08f);
			}
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
			this.Tx.Txt(X.T2K(str)).LetterSpacing(0.95f).Alpha(this.alpha_);
			this.default_title_width = this.Tx.get_swidth_px();
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

		private TextRenderer Tx;

		public static C32 Col;
	}
}
