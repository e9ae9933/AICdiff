using System;
using XX;

namespace nel
{
	public class aBtnFDRow : aBtnNel, UiFieldGuide.IFieldGuideOpenable
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			this.click_snd = "tool_hand_init";
			return this.Skin = (this.RowSkin = new ButtonSkinFDItemRow(this, this.w, this.h));
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		public aBtnFDRow setItem(aBtnFDRow Src)
		{
			this.RowSkin.setItem(Src.RowSkin);
			return this;
		}

		public aBtnFDRow setItem(UiFieldGuide _FDCon, NelM2DBase _M2D, UiFieldGuide.FDR Fdr)
		{
			this.RowSkin.setItem(_FDCon, _M2D, Fdr, null);
			return this;
		}

		public UiFieldGuide.FDR getFDR()
		{
			return this.RowSkin.Fdr;
		}

		public ButtonSkinFDItemRow RowSkin;
	}
}
