using System;
using XX;

namespace nel
{
	public class LabeledInputFieldNel : LabeledInputField
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			if (this.w <= 0f)
			{
				this.w = 240f;
			}
			if (this.h <= 0f)
			{
				this.h = (float)this.size * 0.95f * (float)this.multi_line + (float)this.size * 0.6f;
			}
			if (this.LbSkin == null)
			{
				this.LbSkin = new ButtonSkinForLabelNel(this, this.w, this.h);
			}
			this.LbSkin.setTitle(base.label);
			this.Skin = this.LbSkin;
			return this.LbSkin;
		}
	}
}
