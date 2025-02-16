using System;
using XX;

namespace nel
{
	public class aBtnNumCounterNel : aBtnNumCounter
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			this.click_snd = "enter_small";
			return this.Skin = (this.NcSkin = new ButtonSkinNumCounterNel(this, this.w, this.h));
		}
	}
}
