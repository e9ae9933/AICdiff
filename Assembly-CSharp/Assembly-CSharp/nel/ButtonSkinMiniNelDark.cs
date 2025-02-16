using System;
using XX;

namespace nel
{
	public class ButtonSkinMiniNelDark : ButtonSkinMiniNel
	{
		public ButtonSkinMiniNelDark(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.main_color = uint.MaxValue;
			this.main_color_locked = 2583691263U;
			this.check_fill_color = 4294966715U;
			this.stripe_color = 1157627903U;
			this.push_color = 4283780170U;
			this.push_color_sub = 4294966715U;
		}
	}
}
