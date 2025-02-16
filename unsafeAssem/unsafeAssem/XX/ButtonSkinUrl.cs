using System;
using System.Diagnostics;
using UnityEngine;

namespace XX
{
	public class ButtonSkinUrl : ButtonSkin
	{
		public ButtonSkinUrl(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			this.WHPx((_w == 0f) ? 140f : _w, (_h == 0f) ? 20f : _h);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f || this.Tx == null)
			{
				return this;
			}
			float num = this.w * 64f;
			float num2 = this.h * 64f;
			float num3 = 0f;
			if (this.isPushDown())
			{
				num3 = 2f;
				this.Tx.Col(C32.MulA(4291255131U, this.alpha));
			}
			else if (base.isHoveredOrPushOut())
			{
				this.Tx.Col(C32.MulA(4286946813U, this.alpha));
			}
			else
			{
				this.Tx.Col(C32.MulA(4282738933U, this.alpha));
			}
			IN.PosP(this.Tx.transform, -num / 2f + num2 / 2f + num3 + this.text_margin, -num3, -0.08f);
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
				.Italic(true);
			this.default_title_width = this.Tx.get_swidth_px();
			this.fine_flag = true;
			return this;
		}

		public override bool canClickable(Vector2 PosU)
		{
			float num = X.Mx(this.w * 64f, this.swidth);
			PosU += new Vector2((num - this.swidth) / 2f * 0.015625f, 0f);
			return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), (num + 4f) * 0.015625f, (this.sheight + 4f) * 0.015625f);
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

		public static FnBtnBindings fnClick = delegate(aBtn B)
		{
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = B.title,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				X.de(ex.ToString(), null);
				SND.Ui.play("locked", false);
				return false;
			}
			return true;
		};
	}
}
