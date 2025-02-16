using System;
using XX;

namespace evt
{
	public class aBtnEv : aBtn
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			if (key != null)
			{
				if (key == "ev_emot")
				{
					this.click_snd = "talk_progress";
					return new ButtonSkinEvEmot(this, this.w, this.h);
				}
				if (key == "linedebug")
				{
					this.click_snd = "talk_progress";
					return new ButtonSkinEvLineDebug(this, this.w, this.h);
				}
			}
			return base.makeButtonSkin(key);
		}
	}
}
