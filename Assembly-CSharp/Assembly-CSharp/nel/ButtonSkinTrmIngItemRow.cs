using System;
using XX;

namespace nel
{
	public class ButtonSkinTrmIngItemRow : ButtonSkinItemRow
	{
		public ButtonSkinTrmIngItemRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
		}

		protected override STB getTitleString(STB Stb)
		{
			base.getTitleString(Stb);
			if (UiAlchemyTRM.Instance != null && this.ItmRow != null && UiAlchemyTRM.Instance.CurrentTargetTrmItem != null && UiAlchemyTRM.Instance.CurrentTargetTrmItem.RcmHerb == this.ItmRow.Data)
			{
				Stb.Add("<img mesh=\"IconLaeviLaugh\" scale=\"1\" />");
			}
			return Stb;
		}
	}
}
