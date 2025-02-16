using System;
using System.Collections.Generic;
using XX;

namespace nel
{
	public class UiAlchemy : UiCraftBase
	{
		protected override void Awake()
		{
			base.Awake();
			this.alloc_multiple_creation = true;
		}

		protected override ItemStorage getRecipeTopicDefault()
		{
			return this.M2D.IMNG.getInventoryPrecious();
		}

		protected override void prepareRecipeCreatable()
		{
			this.rcp_use_bits = 0U;
			base.prepareRecipeCreatableS(out this.rcp_use_bits);
		}

		protected override string[] getRcpTopicTabKeys()
		{
			return UiCraftBase.getRcpTopicTabKeysS(this.rcp_use_bits);
		}

		protected override void fineTabIndexFirst(int maxtab)
		{
			this.tptab_index = X.bit_index(this.rcp_use_bits, 1U << this.init_tptab_index);
			if (this.tptab_index < 0)
			{
				this.tptab_index = 0;
			}
		}

		protected override void saveTabIndexFirst()
		{
			this.init_tptab_index = X.bit_on_index(this.rcp_use_bits, this.tptab_index);
		}

		protected override void fnRecipeTopicRowsPrepare(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest)
		{
			RecipeManager.RP_CATEG rp_CATEG = (RecipeManager.RP_CATEG)X.bit_on_index(this.rcp_use_bits, this.tptab_index);
			base.fnRecipeTopicRowsPrepareS(ASource, ADest, rp_CATEG);
		}

		protected override string fnRecipeTopicDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			return UiCraftBase.fnRecipeTopicDescAdditionSForBasic(Itm, row, def_string, grade, Obt, count, this.AStorage, this.StRecipeTopic, ref this.recipe_have_count);
		}

		protected override string getCompletedWorkCountString()
		{
			return "<font size=\"14\">" + TX.GetA("item_current_obtain", this.recipe_have_count) + "</font>";
		}

		protected override void changeState(UiCraftBase.STATE st)
		{
			base.changeState(st);
			if (st == UiCraftBase.STATE.RECIPE_CHOOSE_ROW && base.isCompletionState(st))
			{
				this.recipe_have_count = UiCraftBase.fineRTHaveCount(this.TargetRcp, this.AStorage, this.StRecipeTopic);
			}
		}

		private string recipe_have_count;

		private uint rcp_use_bits;
	}
}
