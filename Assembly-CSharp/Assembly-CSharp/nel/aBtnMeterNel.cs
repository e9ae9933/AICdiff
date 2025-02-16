using System;
using XX;

namespace nel
{
	public class aBtnMeterNel : aBtnMeter
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			this.start_drag_snd = "tool_hand_init";
			this.quit_drag_snd = "tool_hand_quit";
			if (key != null)
			{
				if (!(key == "normal") && !(key == "normal_dark") && (key == null || key.Length != 0))
				{
					if (key == "invisible")
					{
						this.SkinMt = new ButtonSkinMeterInvisible(this, this.w, this.h);
						this.auto_setter_focus = true;
						this.active_drag = false;
						return this.SkinMt;
					}
				}
				else
				{
					if (this.checkbox_mode > 0)
					{
						return this.SkinMt = new ButtonSkinMeterAsCheckBoxNel(this, this.w, this.h, this.checkbox_mode == 2);
					}
					ButtonSkinMeterNel buttonSkinMeterNel = new ButtonSkinMeterNel(this, this.w, this.h);
					buttonSkinMeterNel = ((key == "normal_dark") ? buttonSkinMeterNel.Darken() : buttonSkinMeterNel);
					return this.SkinMt = buttonSkinMeterNel;
				}
			}
			return base.makeButtonSkin(key);
		}

		public const int DEFAULT_W = 200;
	}
}
