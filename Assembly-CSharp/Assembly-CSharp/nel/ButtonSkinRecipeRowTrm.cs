using System;
using XX;

namespace nel
{
	public class ButtonSkinRecipeRowTrm : ButtonSkinItemRow
	{
		public ButtonSkinRecipeRowTrm(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
		}

		public override void setItem(UiItemManageBox _ItemMng, ItemStorage _Storage, ItemStorage.IRow _ItmRow)
		{
			this.Trm = UiAlchemyTRM.GetTrmFromItem(_ItmRow.Data.key);
			this.Rcp = ((this.Trm != null) ? this.Trm.TRecipe : null);
			base.setItem(_ItemMng, _Storage, _ItmRow);
			if (this.Rcp != null)
			{
				this.Rcp.isUseable();
			}
		}

		protected override STB getTitleString(STB Stb)
		{
			if (this.Trm == null)
			{
				return Stb.Clear();
			}
			if (!this.Trm.is_active)
			{
				return base.getTitleString(Stb);
			}
			Stb.Set(this.Trm.getNameLocalized());
			return Stb;
		}

		protected override STB getCountString(STB Stb)
		{
			if (this.Trm != null && this.Trm.is_active)
			{
				Stb.Add(this.Trm.icon_watched_a, this.Trm.icon_watched_b);
			}
			return Stb;
		}

		public TRMManager.TRMItem TargetTrm
		{
			get
			{
				return this.Trm;
			}
		}

		protected RCP.Recipe Rcp;

		protected TRMManager.TRMItem Trm;
	}
}
