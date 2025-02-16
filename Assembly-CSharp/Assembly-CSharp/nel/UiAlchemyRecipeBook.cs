using System;
using System.Collections.Generic;
using PixelLiner;
using XX;

namespace nel
{
	public class UiAlchemyRecipeBook : UiCraftBase
	{
		protected override void Awake()
		{
			this.cartbtn_icon = null;
			this.read_only = true;
			this.first_base_z = -0.1f;
			base.Awake();
			this.IMNG = ((this.M2D != null) ? this.M2D.IMNG : null);
			this.Inventory = ((this.IMNG != null) ? this.IMNG.getInventory() : null);
			this.alloc_multiple_creation = false;
			this.StRecipeTopic = new ItemStorage("Inventory_workbench", 2);
			this.StRecipeTopic.sort_button_bits = 0;
			this.StRecipeTopic.infinit_stockable = true;
			this.topic_use_topright_counter = false;
			this.trm_enabled = SCN.trm_enabled;
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.IMNG.getInventoryPrecious().getWholeInfoDictionary())
			{
				if (keyValuePair.Key.is_recipe)
				{
					this.StRecipeTopic.AddInfo(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}

		public override void InitManager(ItemStorage[] _AInventory, ItemStorage _StRecipeTopic, int _init_tptab_index = -1, PxlFrame _PFCompleteCutin = null)
		{
			if (this.workbench_enabled)
			{
				UiAlchemyWorkBench.initReadScript();
				UiAlchemyWorkBench.AddTopicRow(this.StRecipeTopic);
			}
			if (this.trm_enabled)
			{
				UiAlchemyTRM.AddTopicRow(this.StRecipeTopic);
			}
			this.StRecipeTopic.fineRows(false);
			this.StRecipeTopic.select_row_key = "";
			base.InitManager(_AInventory, _StRecipeTopic, _init_tptab_index, _PFCompleteCutin);
			if (UiAlchemyRecipeBook.NextRevealAtAwake != null)
			{
				this.RevealInBook(UiAlchemyRecipeBook.NextRevealAtAwake);
			}
		}

		protected override ItemStorage getRecipeTopicDefault()
		{
			return this.StRecipeTopic;
		}

		public override void changeTopicTab(RecipeManager.RP_CATEG categ)
		{
			UiCraftBase.changeTopicTabS(this, categ, this.rcp_use_bits);
		}

		protected override void prepareRecipeCreatable()
		{
			base.prepareRecipeCreatableS(out this.rcp_use_bits);
			if (this.workbench_enabled)
			{
				this.rcp_use_bits |= 4U;
			}
			if (this.trm_enabled)
			{
				this.rcp_use_bits |= 8U;
			}
		}

		protected override string[] getRcpTopicTabKeys()
		{
			return UiCraftBase.getRcpTopicTabKeysS(this.rcp_use_bits);
		}

		protected override void fnRecipeTopicRowsPrepare(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest)
		{
			RecipeManager.RP_CATEG rp_CATEG = (RecipeManager.RP_CATEG)X.bit_on_index(this.rcp_use_bits, this.tptab_index);
			if (rp_CATEG == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
			{
				int count = ASource.Count;
				for (int i = 0; i < count; i++)
				{
					ItemStorage.IRow row = ASource[i];
					RecipeManager.Recipe recipeS = UiAlchemyWorkBench.getRecipeS(row.Data);
					if (recipeS != null && recipeS.categ == rp_CATEG)
					{
						ADest.Add(row);
					}
				}
				return;
			}
			base.fnRecipeTopicRowsPrepareS(ASource, ADest, rp_CATEG);
		}

		public override RecipeManager.Recipe getRecipe(NelItem Itm)
		{
			RecipeManager.RP_CATEG rp_CATEG = (RecipeManager.RP_CATEG)X.bit_on_index(this.rcp_use_bits, this.tptab_index);
			if (rp_CATEG == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
			{
				return UiAlchemyWorkBench.getRecipeS(Itm);
			}
			if (rp_CATEG == RecipeManager.RP_CATEG.ALOMA)
			{
				return UiAlchemyTRM.getRecipeS(Itm);
			}
			return UiCraftBase.getRecipeBasic(Itm);
		}

		protected override string fnRecipeTopicDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			RecipeManager.Recipe recipe = this.getRecipe(Itm);
			if (recipe == null)
			{
				return def_string;
			}
			if (recipe.categ == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
			{
				return UiAlchemyWorkBench.fnRecipeTopicDescAdditionSForWBench(Itm, row, def_string, grade, Obt, count);
			}
			if (recipe.categ == RecipeManager.RP_CATEG.ALOMA)
			{
				return UiAlchemyTRM.fnRecipeTopicDescAdditionSForTrm(Itm, row, def_string, grade, Obt, count);
			}
			string text = "";
			return UiCraftBase.fnRecipeTopicDescAdditionSForBasic(Itm, row, def_string, grade, Obt, count, this.AStorage, this.StRecipeTopic, ref text);
		}

		protected override string getCompletedWorkCountString()
		{
			return null;
		}

		public override string topic_row_skin
		{
			get
			{
				if (!this.trm_enabled)
				{
					return base.topic_row_skin;
				}
				RecipeManager.RP_CATEG rp_CATEG = (RecipeManager.RP_CATEG)X.bit_on_index(this.rcp_use_bits, this.tptab_index);
				if (rp_CATEG == RecipeManager.RP_CATEG.ALOMA)
				{
					return "recipe_trm";
				}
				if (rp_CATEG != RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
				{
					return base.topic_row_skin;
				}
				return "normal";
			}
		}

		protected override void prepareRecipe(RecipeManager.Recipe Rcp, NelItem Itm, List<List<UiCraftBase.IngEntryRow>> AAPre = null)
		{
			if (Rcp.categ == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
			{
				UiAlchemyWorkBench.prepareRecipeIngredientS(Rcp, this, AAPre);
			}
			if (Rcp.categ == RecipeManager.RP_CATEG.ALOMA)
			{
				this.TargetTrm = UiAlchemyTRM.GetTrmFromItem(Itm.key);
				if (this.TargetTrm == null || !this.TargetTrm.is_active)
				{
					return;
				}
			}
			else
			{
				this.TargetTrm = null;
			}
			base.prepareRecipe(Rcp, Itm, AAPre);
		}

		protected override UiCraftBase.AutoCreationInfo getFirstAutoCreateInfo()
		{
			return null;
		}

		protected override string getIngredientTitle()
		{
			if (this.TargetTrm == null)
			{
				return base.getIngredientTitle();
			}
			return TX.GetA("aloma_ingredient_title", this.TargetTrm.getNameLocalized());
		}

		protected override string getCompletionDetail()
		{
			if (this.TargetRcp.categ == RecipeManager.RP_CATEG.ALOMA && this.TargetTrm != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AddTxA("TrmUi_recommend_aloma_making", false).TxRpl(this.TargetTrm.getLocalizedRecommendedItem());
					stb.Add("\n  -----  \n");
					stb.Add(this.TargetRcpItm.getDescLocalized(null, 0));
					return stb.ToString();
				}
			}
			return "";
		}

		public void RevealInBook(object Target)
		{
			UiAlchemyRecipeBook.NextRevealAtAwake = null;
			if (this.stt == UiCraftBase.STATE.RECIPE_TOPIC)
			{
				if (Target is NelItem)
				{
					NelItem nelItem = Target as NelItem;
					RecipeManager.Recipe recipe = RecipeManager.Get(nelItem);
					if (recipe != null)
					{
						Target = recipe;
					}
					else if (nelItem.RecipeInfo != null && nelItem.RecipeInfo.DishInfo != null)
					{
						Target = nelItem.RecipeInfo.DishInfo.Rcp;
					}
					else
					{
						recipe = RecipeManager.GetForCompletion(nelItem);
						if (recipe != null)
						{
							Target = recipe;
						}
						else
						{
							recipe = UiAlchemyWorkBench.getRecipeS(nelItem);
							if (recipe != null)
							{
								Target = recipe;
							}
						}
					}
				}
				if (Target is RecipeManager.Recipe)
				{
					RecipeManager.Recipe recipe2 = Target as RecipeManager.Recipe;
					this.changeTopicTab(recipe2.categ);
					if (this.ItemMng != null)
					{
						if (recipe2.categ == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH)
						{
							using (BList<aBtnItemRow> blist = this.ItemMng.Inventory.PopGetItemRowBtnsFor(recipe2.Completion))
							{
								if (blist.Count > 0)
								{
									blist[0].Select(false);
								}
								return;
							}
						}
						NelItem recipeItem = recipe2.RecipeItem;
						if (recipeItem != null)
						{
							using (BList<aBtnItemRow> blist2 = this.ItemMng.Inventory.PopGetItemRowBtnsFor(recipeItem))
							{
								if (blist2.Count > 0)
								{
									blist2[0].Select(false);
								}
							}
						}
					}
				}
			}
		}

		private NelItemManager IMNG;

		private ItemStorage Inventory;

		private uint rcp_use_bits;

		public bool workbench_enabled = true;

		public bool trm_enabled;

		public const string item_key_recipe_book = "recipe_collection";

		public static object NextRevealAtAwake;

		private TRMManager.TRMItem TargetTrm;
	}
}
