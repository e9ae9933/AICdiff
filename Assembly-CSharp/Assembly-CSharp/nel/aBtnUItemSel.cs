using System;
using m2d;
using XX;

namespace nel
{
	public class aBtnUItemSel : aBtnNel
	{
		protected override void Awake()
		{
			base.Awake();
			this.hover_snd = "";
			this.USel = (M2DBase.Instance as NelM2DBase).IMNG.USel;
			base.addClickFn(delegate(aBtn B)
			{
				this.USel.uiClicked();
				return true;
			});
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			if (key != null && (key == "normal" || (key != null && key.Length == 0)))
			{
				this.click_snd = "tool_hand_init";
				this.Skin = (this.MsSkin = new ButtonSkinUItemSel(this, this.w, this.h));
				this.MsSkin.BSel = this;
				return this.Skin;
			}
			return base.makeButtonSkin(key);
		}

		public void fineCursor()
		{
			this.MsSkin.fineCursor();
			if (base.isSelected())
			{
				CURS.focusOnBtn(this, true);
			}
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (base.isActive() && IN.use_mouse)
			{
				this.MsSkin.checkMouseCurs();
			}
			return true;
		}

		protected override void simulateNaviTranslation(int aim = -1)
		{
		}

		public TextRenderer TxCenter
		{
			get
			{
				if (this.MsSkin == null)
				{
					this.initializeSkin("", "");
				}
				return this.MsSkin.TxC;
			}
		}

		public ButtonSkinUItemSel MsSkin;

		public UseItemSelector USel;

		private bool desc_activated;
	}
}
