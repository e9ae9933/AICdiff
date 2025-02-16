using System;
using XX;

namespace nel.mgm.dojo
{
	public class aBtnDJ : aBtnNel
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			if (key != null && (key == "normal" || (key != null && key.Length == 0)))
			{
				this.click_snd = "";
				return this.Skin = new ButtonSkinDJRow(this, this.w, this.h);
			}
			return base.makeButtonSkin(key);
		}
	}
}
