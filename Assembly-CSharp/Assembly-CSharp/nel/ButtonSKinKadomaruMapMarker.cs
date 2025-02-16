using System;
using XX;

namespace nel
{
	public class ButtonSKinKadomaruMapMarker : ButtonSKinKadomaruIconNel
	{
		public ButtonSKinKadomaruMapMarker(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
		}

		protected override void drawChecked(float wh, float hh)
		{
			base.drawCheckedAt(wh - 12f, hh - 6f);
		}

		protected override void drawIco()
		{
			base.drawIco();
			if (this.x_counter_ > -1000)
			{
				this.Md.Col = this.Md.ColGrd.Set(base.isLocked() ? 3431499912U : 4282004532U).mulA(this.alpha_).C;
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add("x", this.x_counter_, 22);
					MTRX.ChrM.DrawStringTo(this.Md, stb, 8f, -13f, ALIGN.CENTER, ALIGNY.BOTTOM, false, 0f, 0f, null);
				}
			}
		}

		private int x_counter_ = -1000;
	}
}
