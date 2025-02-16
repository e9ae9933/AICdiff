using System;
using XX;

namespace nel
{
	public class aBtnNelMapArea : aBtn
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			this.click_snd = "tool_eraser_init";
			return this.Skin = new ButtonSkinWholeMapArea(this, this.w, this.h);
		}

		protected override void simulateNaviTranslation(int aim = -1)
		{
		}
	}
}
