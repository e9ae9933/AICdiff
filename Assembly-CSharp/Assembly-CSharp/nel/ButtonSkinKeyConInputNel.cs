using System;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinKeyConInputNel : ButtonSkinNormalNelDark
	{
		public ButtonSkinKeyConInputNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			this.fine_continue_flags = 1U;
			base.main_color = 4294966715U;
			base.stripe_color = 0U;
			this.left_daia_size = 0f;
			this.mask_margin = 1f;
			this.html_mode = true;
		}

		protected override void drawChecked()
		{
			float num = (float)(X.IntR(this.w * 64f * 0.5f) * 2);
			float num2 = (float)(X.IntR(this.h * 64f * 0.5f) * 2);
			this.MdStripe.Col = this.Md.ColGrd.Set(base.main_color).mulA(this.alpha_ * 0.5f).C;
			this.MdStripe.StripedM(0.7853982f, 20f, 0.5f, 6);
			this.MdStripe.NelBanner(0f, 0f, num - this.mask_margin * 2f, num2 - this.mask_margin * 2f, 0f, true, false, 0f, 0f, 0f).allocUv2(0, true);
			this.MdStripe.updateForMeshRenderer(false);
			this.Md.Col = this.Md.ColGrd.Set(base.main_color).mulA(this.alpha_ * 0.25f).C;
			this.Md.NelBanner(0f, 0f, num, num2, 0f, true, false, 0f, 0f, 0f);
		}

		protected override void drawHover()
		{
			if (base.isChecked())
			{
				return;
			}
			float num = (float)(X.IntR(this.w * 64f * 0.5f) * 2);
			float num2 = (float)(X.IntR(this.h * 64f * 0.5f) * 2);
			Color32 col = this.Md.Col;
			this.Md.Col = this.Md.ColGrd.Set(base.main_color).mulA((0.15f + 0.15f * X.COSIT(34f)) * this.alpha_).C;
			this.Md.NelBanner(0f, 0f, num, num2, 0f, true, false, 0f, 0f, 0f);
			this.Md.Col = col;
		}
	}
}
