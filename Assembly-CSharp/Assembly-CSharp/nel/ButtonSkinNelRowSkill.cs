using System;
using PixelLiner;
using XX;

namespace nel
{
	public class ButtonSkinNelRowSkill : ButtonSkinNelUi
	{
		public ButtonSkinNelRowSkill(aBtnNelRowSkill _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.BSk = _B;
			this.fix_text_size_ = 18f;
			this.TxHowTo = base.MakeTx("-text");
			this.TxHowTo.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE);
			this.TxHowTo.stencil_ref = base.container_stencil_ref;
			this.TxHowTo.html_mode = true;
			this.TxHowTo.use_valotile = true;
			if (TX.isEnglishLang())
			{
				this.TxHowTo.LetterSpacing(0.96f);
				this.TxHowTo.LineSpacing(1.16f);
				this.TxHowTo.auto_wrap = true;
			}
			else
			{
				this.TxHowTo.auto_condense = true;
			}
			this.TxHowTo.Size(14f);
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(MTR.MIiconL.getMtr(BLEND.NORMAL, base.container_stencil_ref), false);
			this.Md.chooseSubMesh(0, false, false);
			this.Md.connectRendererToTriMulti(this.MyMeshRenderer);
		}

		public void initSkill(PrSkill _Sk)
		{
			if (this.Sk != _Sk)
			{
				this.Sk = _Sk;
				if (this.Sk != null)
				{
					this.setTitle(null);
					IN.PosP(this.TxHowTo.transform, this.w * 64f * (this.Sk.always_enable ? 0.25f : 0.19f), 0f, -0.25f);
					this.TxHowTo.Txt(this.Sk.manipulate);
					if (this.Sk.new_icon)
					{
						base.show_new_icon = 10;
						this.Md.chooseSubMesh(2, false, false);
						this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, base.container_stencil_ref), false);
					}
					else
					{
						base.show_new_icon = 0;
					}
					this.fine_flag = true;
				}
			}
		}

		public override ButtonSkin WHPx(float _wpx, float _hpx)
		{
			base.WHPx(_wpx, _hpx);
			return this;
		}

		protected override void fineDrawAdditional(ref float shift_px_x, ref float shift_px_y, ref bool shift_y_abs)
		{
			PxlFrame pxlFrame = ((this.Sk != null) ? this.Sk.getThumbPF() : null);
			if (pxlFrame == null)
			{
				return;
			}
			this.Md.chooseSubMesh(1, false, false);
			this.Md.Col = C32.MulA(uint.MaxValue, this.alpha_ * 0.3f);
			int num = pxlFrame.countLayers();
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = pxlFrame.getLayer(i);
				PxlImage img = layer.Img;
				float num2 = this.w * -0.38f + layer.x * 0.015625f;
				float num3 = (0.5f - layer.y) * 0.015625f;
				float num4 = (float)img.width * 0.015625f;
				float num5 = (float)img.height * 0.015625f;
				this.Md.uvRect(num2 - num4 * 0.5f, num3 - num5 * 0.5f, num4, num5, img, true, false);
				float num6 = X.Mx(-0.5f * this.w, num2 - num4 * 0.5f);
				float num7 = X.Mx(-0.5f * this.h + 0.015625f, num3 - num5 * 0.5f);
				float num8 = X.Mn(0.5f * this.w, num2 + num4 * 0.5f);
				float num9 = X.Mn(0.5f * this.h, num3 + num5 * 0.5f);
				this.Md.RectBL(num6, num7, num8 - num6, num9 - num7, true);
			}
			shift_px_x += this.w * 64f * 0.12f + 35f;
			if (base.show_new_icon >= 10)
			{
				this.Md.chooseSubMesh(2, false, false);
				this.Md.Col = C32.MulA(4294926244U, this.alpha_ * ((base.show_new_icon == 10) ? 0.5f : 1f));
				this.Md.RotaPF(-this.w * 64f * 0.5f + 20f, 0f, 1f, 1f, 0f, MTR.ImgNew, false, false, false, uint.MaxValue, false, 0);
			}
			this.Md.chooseSubMesh(0, false, false);
		}

		protected override void setTitleText(string str)
		{
			base.setTitleText(str);
			if (this.Sk != null)
			{
				float num = this.w * 64f;
				this.Tx.auto_condense = true;
				this.Tx.max_swidth_px = (num - (num * 0.12f + 35f)) * 0.5f - (float)(this.Sk.always_enable ? 0 : 25);
				this.TxHowTo.max_swidth_px = X.Mx(30f, (num - (num * 0.12f + 35f)) * 0.4f - (float)(this.Sk.always_enable ? 0 : 45));
			}
		}

		public void fineNewIconBlink()
		{
			if (base.show_new_icon >= 10)
			{
				byte b = ((X.ANMT(2, 30f) == 1) ? 10 : 11);
				base.show_new_icon = b;
			}
		}

		public override ButtonSkin Fine()
		{
			base.Fine();
			if (this.Tx != null)
			{
				this.TxHowTo.Alpha(this.alpha_).Col(this.Tx.TextColor);
			}
			return this;
		}

		protected override void RowFineAfter(float w, float h)
		{
			this.Md.Col = C32.MulA(4283780170U, this.alpha_ * 0.4f);
			float num = w * 0.5f;
			this.Md.Line(-num, -h * 0.5f, num, -h * 0.5f, 1f, false, 0f, 0f);
			base.RowFineAfter(w, h);
		}

		protected override void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
		}

		public override ButtonSkin setTitle(string str)
		{
			return base.setTitle((this.Sk != null) ? this.Sk.title : str);
		}

		public override float alpha
		{
			set
			{
				if (base.alpha == value)
				{
					return;
				}
				base.alpha = value;
				if (this.BSk != null && this.BSk.UseCheck != null)
				{
					this.BSk.UseCheck.setAlpha(value);
				}
			}
		}

		public const float DEFAULT_RS_H = 38f;

		public readonly aBtnNelRowSkill BSk;

		private PrSkill Sk;

		protected TextRenderer TxHowTo;
	}
}
