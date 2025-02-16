using System;
using XX;

namespace nel
{
	public class ButtonSkinItemLunchTimeRow : ButtonSkinItemRow
	{
		public ButtonSkinItemLunchTimeRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
		}

		protected override string getTitleString()
		{
			if (base.Inventory == null)
			{
				return base.getTitleString();
			}
			NelItem data = this.ItmRow.Data;
			return this.ItemMng.getDescStr(this.ItmRow, UiItemManageBox.DESC_ROW.NAME, (int)this.ItmRow.splitted_grade);
		}
	}
}
