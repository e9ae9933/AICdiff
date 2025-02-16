using System;
using XX;

namespace nel
{
	public class ButtonSKinKadomaruIconNel : ButtonSkinKadomaruIcon
	{
		public ButtonSKinKadomaruIconNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			this.col_lock = 4290689711U;
			this.col_pushdown = 4283780170U;
			this.col_checked = 4294966715U;
			this.col_hovered_or_pushout = 4293321691U;
			this.col_hovered_or_pushout2 = 4291611332U;
			this.col_normal = 4293321691U;
			this.col_stripe = 4283780170U;
		}

		protected void drawCheckedAt(float x, float y)
		{
			this.Md.Col = this.Md.ColGrd.White().mulA(this.alpha_ * 0.75f).C;
			this.Md.CheckMark(x, y, (12f + (base.icon_scale - 1f) * 2f) * base.icon_scale, 6f * base.icon_scale, false);
			this.Md.Col = this.Md.ColGrd.Set(base.isLocked() ? 3431499912U : 4283780170U).mulA(this.alpha_).C;
			this.Md.CheckMark(x, y, 10f * base.icon_scale, 4f * base.icon_scale, false);
		}

		protected override void drawChecked(float wh, float hh)
		{
			this.drawCheckedAt(wh - 4f * base.icon_scale, -hh + 8f * base.icon_scale);
		}
	}
}
