using System;
using XX;

namespace nel
{
	public class ButtonSkinRecipeRow : ButtonSkinItemRow
	{
		public ButtonSkinRecipeRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
		}

		public override void setItem(UiItemManageBox _ItemMng, ItemStorage _Storage, ItemStorage.IRow _ItmRow)
		{
			this.Rcp = RecipeManager.Get(_ItmRow.Data);
			base.setItem(_ItemMng, _Storage, _ItmRow);
			if (this.Rcp != null && this.Rcp.isUseable())
			{
				this.Rcp.touchObtainCountAllIngredients();
				if (this.Rcp.categ == RecipeManager.RP_CATEG.ALCHEMY || this.Rcp.categ == RecipeManager.RP_CATEG.COOK)
				{
					NelItem recipeItem = this.Rcp.RecipeItem;
					if (recipeItem != null)
					{
						recipeItem.touchObtainCount();
					}
				}
			}
		}

		protected override string getTitleString()
		{
			if (this.Rcp == null)
			{
				return this.ItmRow.Data.getLocalizedName((this.ItmRow.splitted_grade >= 0) ? ((int)this.ItmRow.splitted_grade) : this.ItmRow.Info.min_grade, this.Storage);
			}
			return this.Rcp.title;
		}

		protected override STB getCountString(STB Stb)
		{
			if (this.Rcp != null && this.Rcp.isUseable())
			{
				Stb.AddTxA("Alchemy_recipe_is_useable", false);
			}
			return Stb;
		}

		public RecipeManager.Recipe get_Recipe()
		{
			return this.Rcp;
		}

		protected override void drawIcon(float x, float y)
		{
			base.drawIcon(x, y);
			if (this.Rcp != null && this.Rcp.created > 0U)
			{
				this.Md.chooseSubMesh(0, false, false);
				this.Md.Col = this.Tx.TextColor;
				this.Md.CheckMark(this.Tx.transform.localPosition.x * 64f + this.Tx.get_swidth_px() + 10f, 0f, this.h * 64f * 0.55f, 3f, false);
			}
		}

		protected RecipeManager.Recipe Rcp;
	}
}
