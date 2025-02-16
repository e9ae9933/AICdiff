using System;
using XX;

namespace nel
{
	public class ButtonSkinNormalNelDark : ButtonSkinNormalNel
	{
		public ButtonSkinNormalNelDark(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			base.main_color = 4293321691U;
			base.main_color_locked = 4286675064U;
			base.stripe_color = 818727620U;
			base.push_color = 4283780170U;
			base.push_color_sub = 4288057994U;
		}
	}
}
