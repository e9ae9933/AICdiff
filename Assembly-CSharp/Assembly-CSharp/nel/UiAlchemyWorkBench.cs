using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel
{
	public class UiAlchemyWorkBench : UiCraftBase
	{
		public static void initReadScript()
		{
			if (UiAlchemyWorkBench.ORecipe != null)
			{
				return;
			}
			UiAlchemyWorkBench.ORecipe = new BDic<string, RCP.Recipe>(2);
			UiAlchemyWorkBench.ORecipeRow = new BDic<UiAlchemyWorkBench.WKB_KIND, List<UiAlchemyWorkBench.WorkBenchRow>>(2);
			UiAlchemyWorkBench.Omax = new BDic<UiAlchemyWorkBench.WKB_KIND, int>(2);
			UiAlchemyWorkBench.CInfoDef = new UiCraftBase.AutoCreationInfo(0, UiCraftBase.AF_COST.LOW_COST, UiCraftBase.AF_KIND.ENOUGH, UiCraftBase.AF_QUANTITY.MINIMUM);
			UiAlchemyWorkBench.WKB_KIND wkb_KIND = UiAlchemyWorkBench.WKB_KIND.CAPACITY;
			for (int i = 0; i < 2; i++)
			{
				wkb_KIND = (UiAlchemyWorkBench.WKB_KIND)i;
				RCP.Recipe recipe = new RCP.Recipe(wkb_KIND.ToString(), RCP.RP_CATEG.ALCHEMY_WORKBENCH);
				UiAlchemyWorkBench.Omax[wkb_KIND] = 0;
				recipe.Completion = NelItem.GetById("workbench_" + recipe.key.ToLower(), false);
				recipe.CInfo = new UiCraftBase.AutoCreationInfo(UiAlchemyWorkBench.CInfoDef);
				recipe.CInfo.obtain_flag = true;
				UiAlchemyWorkBench.ORecipe[recipe.key] = recipe;
			}
			CsvReaderA csvReaderA = new CsvReaderA(TX.getResource("Data/workbench", ".csv", false), true);
			EfParticleFuncCalc efParticleFuncCalc = null;
			List<UiAlchemyWorkBench.WorkBenchRow> list = null;
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					RCP.Recipe recipe2 = X.Get<string, RCP.Recipe>(UiAlchemyWorkBench.ORecipe, index.ToUpper());
					efParticleFuncCalc = null;
					if (recipe2 == null)
					{
						csvReaderA.tError("不明なワークベンチカテゴリ: " + index);
						return;
					}
					wkb_KIND = UiAlchemyWorkBench.getKind(recipe2);
					list = (UiAlchemyWorkBench.ORecipeRow[wkb_KIND] = new List<UiAlchemyWorkBench.WorkBenchRow>(3));
				}
				else if (list != null)
				{
					if (csvReaderA.cmd == "%GRADE")
					{
						efParticleFuncCalc = new EfParticleFuncCalc(csvReaderA.slice_join(1, " ", ""), "Z0", 1f);
					}
					else if (csvReaderA.cmd == "%MAX")
					{
						UiAlchemyWorkBench.Omax[wkb_KIND] = csvReaderA.Int(1, 0);
					}
					else
					{
						RCP.Recipe recipe3;
						NelItem nelItem;
						RCP.RPI_CATEG rpi_CATEG;
						NelItem.CATEG categ;
						if (!RCP.getIngredientTarget(csvReaderA, out recipe3, out nelItem, out rpi_CATEG, out categ))
						{
							break;
						}
						EfParticleFuncCalc efParticleFuncCalc2 = new EfParticleFuncCalc(csvReaderA.slice_join(1, " ", ""), "Z0", 1f);
						list.Add(new UiAlchemyWorkBench.WorkBenchRow(nelItem, recipe3, rpi_CATEG, categ, efParticleFuncCalc2, efParticleFuncCalc));
					}
				}
			}
		}

		public static List<RCP.RecipeDescription> listupDefinitionRecipe(ref List<RCP.RecipeDescription> A, NelItem Itm, bool only_useableItem = false)
		{
			foreach (KeyValuePair<string, RCP.Recipe> keyValuePair in UiAlchemyWorkBench.ORecipe)
			{
				RCP.Recipe value = keyValuePair.Value;
				int count = value.AIng.Count;
				for (int i = 0; i < count; i++)
				{
					if (value.AIng[i].forNeeds(Itm, true))
					{
						if (A == null)
						{
							A = new List<RCP.RecipeDescription>(1);
						}
						A.Add(new RCP.RecipeDescription(value, value.Completion, null));
						break;
					}
				}
			}
			return A;
		}

		protected override void Awake()
		{
			this.cartbtn_icon = "pict_workbench";
			base.Awake();
			this.IMNG = ((this.M2D != null) ? this.M2D.IMNG : null);
			this.Inventory = ((this.IMNG != null) ? this.IMNG.getInventory() : null);
			this.alloc_multiple_creation = false;
			UiAlchemyWorkBench.initReadScript();
			this.StRecipeTopic = new ItemStorage("Inventory_workbench", 2);
			this.StRecipeTopic.sort_button_bits = 0;
			this.StRecipeTopic.infinit_stockable = true;
			this.topic_use_topright_counter = false;
			UiAlchemyWorkBench.AddTopicRow(this.StRecipeTopic);
			this.StRecipeTopic.fineRows(false);
			this.StRecipeTopic.select_row_key = "workbench_capacity";
		}

		protected override string topic_title_text_content
		{
			get
			{
				return TX.Get("storage_title_workbench", "");
			}
		}

		public static void AddTopicRow(ItemStorage St)
		{
			for (int i = 0; i < 2; i++)
			{
				UiAlchemyWorkBench.WKB_KIND wkb_KIND = (UiAlchemyWorkBench.WKB_KIND)i;
				RCP.Recipe recipe = UiAlchemyWorkBench.ORecipe[wkb_KIND.ToString()];
				if (St != null)
				{
					St.Add(recipe.Completion, 1, 0, true, true);
				}
				UiAlchemyWorkBench.initializeRcp(recipe);
			}
		}

		public static UiAlchemyWorkBench.WKB_KIND initializeRcp(RCP.Recipe Rcp)
		{
			UiAlchemyWorkBench.WKB_KIND kind = UiAlchemyWorkBench.getKind(Rcp);
			List<UiAlchemyWorkBench.WorkBenchRow> list = X.Get<UiAlchemyWorkBench.WKB_KIND, List<UiAlchemyWorkBench.WorkBenchRow>>(UiAlchemyWorkBench.ORecipeRow, kind);
			if (list != null)
			{
				int num2;
				float num = (float)UiAlchemyWorkBench.getCurrentCount(kind, out num2);
				Rcp.AIng.Clear();
				if (num < (float)UiAlchemyWorkBench.Omax[kind])
				{
					int count = list.Count;
					int num3 = 0;
					for (int i = 0; i < count; i++)
					{
						RCP.RecipeIngredient recipeIngredient = list[i].createRow(num3, num);
						if (recipeIngredient != null)
						{
							Rcp.AIng.Add(recipeIngredient);
							num3++;
						}
					}
				}
			}
			else
			{
				X.de("ワークベンチカテゴリが初期化されていません: " + kind.ToString(), null);
			}
			return kind;
		}

		protected override ItemStorage getRecipeTopicDefault()
		{
			return this.StRecipeTopic;
		}

		protected override void prepareRecipeCreatable()
		{
			uint num;
			base.prepareRecipeCreatableS(out num);
		}

		protected override string[] getRcpTopicTabKeys()
		{
			return null;
		}

		protected override void fnRecipeTopicRowsPrepare(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest)
		{
			ADest.AddRange(ASource);
		}

		protected override string fnRecipeTopicDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			return UiAlchemyWorkBench.fnRecipeTopicDescAdditionSForWBench(Itm, row, def_string, grade, Obt, count);
		}

		public static string fnRecipeTopicDescAdditionSForWBench(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.DETAIL)
			{
				RCP.Recipe recipeS = UiAlchemyWorkBench.getRecipeS(Itm);
				if (recipeS != null)
				{
					if (recipeS.AIng.Count == 0)
					{
						return TX.Get("workbench_limit_reached", "");
					}
					return recipeS.listupIngredients("\n", true, true);
				}
			}
			else
			{
				if (row == UiItemManageBox.DESC_ROW.DESC)
				{
					return TX.add(def_string, UiAlchemyWorkBench.getCurrentCountTx(UiAlchemyWorkBench.getKind(Itm), -1, 0, false), "\n\n");
				}
				if (row == UiItemManageBox.DESC_ROW.ROW_COUNT)
				{
					RCP.Recipe recipeS2 = UiAlchemyWorkBench.getRecipeS(Itm);
					if (recipeS2 != null)
					{
						UiAlchemyWorkBench.WKB_KIND kind = UiAlchemyWorkBench.getKind(recipeS2);
						int num;
						int currentCount = UiAlchemyWorkBench.getCurrentCount(kind, out num);
						return TX.GetA("workbench_current_short", (currentCount + num).ToString(), (UiAlchemyWorkBench.Omax[kind] + num).ToString());
					}
					return "";
				}
			}
			return def_string;
		}

		protected override string getCompletedWorkCountString()
		{
			return null;
		}

		protected override string getCompleteDialogPrompt(string item_name, int created, int add_rest, ref float lineSpacing)
		{
			UiAlchemyWorkBench.WKB_KIND kind = UiAlchemyWorkBench.getKind(this.TargetRcp);
			int num;
			created = UiAlchemyWorkBench.getCurrentCount(kind, out num);
			created += num;
			return TX.GetA("alchemy_workbench_complete_" + kind.ToString().ToLower(), created.ToString());
		}

		protected override void prepareRecipeIngredient(List<List<UiCraftBase.IngEntryRow>> AAPre = null)
		{
			UiAlchemyWorkBench.prepareRecipeIngredientS(this.TargetRcp, this, AAPre);
			base.prepareRecipeIngredient(AAPre);
		}

		public static void prepareRecipeIngredientS(RCP.Recipe TargetRcp, UiCraftBase _this, List<List<UiCraftBase.IngEntryRow>> AAPre = null)
		{
			if (UiAlchemyWorkBench.initializeRcp(TargetRcp) == UiAlchemyWorkBench.WKB_KIND.CAPACITY)
			{
				_this.tx_ingredient_title = "workbench_ingredient_title";
			}
			else
			{
				_this.tx_ingredient_title = "alchemy_ingredient_title";
			}
			_this.need_recipe_recheck = true;
		}

		protected override UiCraftBase.AutoCreationInfo getFirstAutoCreateInfo()
		{
			return UiAlchemyWorkBench.CInfoDef;
		}

		public static int getCurrentCount(UiAlchemyWorkBench.WKB_KIND kind, out int tx_addition)
		{
			tx_addition = 0;
			if (M2DBase.Instance == null)
			{
				return 0;
			}
			NelItemManager imng = (M2DBase.Instance as NelM2DBase).IMNG;
			if (imng == null)
			{
				return 0;
			}
			ItemStorage inventory = imng.getInventory();
			if (inventory == null)
			{
				return 0;
			}
			if (kind == UiAlchemyWorkBench.WKB_KIND.CAPACITY)
			{
				tx_addition = 12;
				return inventory.row_max - tx_addition;
			}
			if (kind != UiAlchemyWorkBench.WKB_KIND.BOTTLE)
			{
				return 0;
			}
			return inventory.hide_bottle_max;
		}

		public static string getCurrentCountTx(UiAlchemyWorkBench.WKB_KIND kind, int cnt = -1, int tx_addition = 0, bool next = false)
		{
			if (cnt < 0)
			{
				cnt = UiAlchemyWorkBench.getCurrentCount(kind, out tx_addition);
			}
			string text;
			switch (kind)
			{
			case UiAlchemyWorkBench.WKB_KIND.CAPACITY:
				text = "workbench_current_capacity";
				break;
			case UiAlchemyWorkBench.WKB_KIND.BOTTLE:
				text = "workbench_current_obtain";
				break;
			case UiAlchemyWorkBench.WKB_KIND._MAX:
				text = "workbench_current_short";
				break;
			default:
				return "";
			}
			string a = TX.GetA("workbench_current_short", (cnt + tx_addition).ToString(), (UiAlchemyWorkBench.Omax[kind] + tx_addition).ToString());
			if (next)
			{
				return TX.GetA(text + "_after", a);
			}
			return TX.GetA(text, a);
		}

		protected override void getCompletionDetail(STB Stb)
		{
			UiAlchemyWorkBench.WKB_KIND kind = UiAlchemyWorkBench.getKind(this.TargetRcp);
			int num;
			int currentCount = UiAlchemyWorkBench.getCurrentCount(kind, out num);
			Stb.Add(UiAlchemyWorkBench.getCurrentCountTx(kind, currentCount, num, false));
			if (this.TargetRcp.AIng.Count > 0)
			{
				if (this.recipe_creatable)
				{
					Stb.Append(UiAlchemyWorkBench.getCurrentCountTx(kind, currentCount + 1, num, true), "\n\n");
					return;
				}
			}
			else
			{
				Stb.Append(TX.Get("workbench_limit_reached", ""), "\n\n");
			}
		}

		protected override int addCompletionToStorage(int add_rest)
		{
			this.CompletionImage.ItemData = this.TargetRcp.Completion;
			ItemStorage itemStorage = ((this.IMNG != null) ? this.IMNG.getInventoryPrecious() : null);
			bool flag = true;
			UiAlchemyWorkBench.WKB_KIND kind = UiAlchemyWorkBench.getKind(this.TargetRcp);
			if (kind != UiAlchemyWorkBench.WKB_KIND.CAPACITY)
			{
				if (kind == UiAlchemyWorkBench.WKB_KIND.BOTTLE)
				{
					this.Inventory.hide_bottle_max += add_rest;
					this.Inventory.fineRows(true);
				}
			}
			else
			{
				this.IMNG.increaseInenvoryCapacity(add_rest, UiAlchemyWorkBench.Omax[UiAlchemyWorkBench.WKB_KIND.CAPACITY] + 12);
				flag = false;
			}
			if (itemStorage != null && flag)
			{
				itemStorage.Add(this.TargetRcp.Completion, add_rest, 0, true, true);
			}
			return 0;
		}

		public override string topic_row_skin
		{
			get
			{
				return "normal";
			}
		}

		public override RCP.Recipe getRecipe(string key)
		{
			return UiAlchemyWorkBench.getRecipeS(key);
		}

		public static RCP.Recipe getRecipeS(string key)
		{
			return X.Get<string, RCP.Recipe>(UiAlchemyWorkBench.ORecipe, key);
		}

		public override RCP.Recipe getRecipe(NelItem Itm)
		{
			return UiAlchemyWorkBench.getRecipeS(Itm);
		}

		public static RCP.Recipe getRecipeS(NelItem Itm)
		{
			UiAlchemyWorkBench.initReadScript();
			if (!TX.isStart(Itm.key, "workbench_", 0))
			{
				return null;
			}
			return X.Get<string, RCP.Recipe>(UiAlchemyWorkBench.ORecipe, TX.slice(Itm.key, "workbench_".Length).ToUpper());
		}

		private static UiAlchemyWorkBench.WKB_KIND getKind(NelItem Itm)
		{
			UiAlchemyWorkBench.WKB_KIND wkb_KIND;
			if (TX.isStart(Itm.key, "workbench_", 0) && FEnum<UiAlchemyWorkBench.WKB_KIND>.TryParse(TX.slice(Itm.key, "workbench_".Length).ToUpper(), out wkb_KIND, true))
			{
				return wkb_KIND;
			}
			return UiAlchemyWorkBench.WKB_KIND.CAPACITY;
		}

		private static UiAlchemyWorkBench.WKB_KIND getKind(RCP.Recipe Rcp)
		{
			UiAlchemyWorkBench.WKB_KIND wkb_KIND;
			if (FEnum<UiAlchemyWorkBench.WKB_KIND>.TryParse(Rcp.key, out wkb_KIND, true))
			{
				return wkb_KIND;
			}
			return UiAlchemyWorkBench.WKB_KIND.CAPACITY;
		}

		private NelItemManager IMNG;

		private ItemStorage Inventory;

		private static UiCraftBase.AutoCreationInfo CInfoDef;

		private static BDic<string, RCP.Recipe> ORecipe;

		private static BDic<UiAlchemyWorkBench.WKB_KIND, List<UiAlchemyWorkBench.WorkBenchRow>> ORecipeRow;

		private static BDic<UiAlchemyWorkBench.WKB_KIND, int> Omax;

		private const string wb_script = "Data/workbench";

		public const string wkb_header = "workbench_";

		public enum WKB_KIND
		{
			CAPACITY,
			BOTTLE,
			_MAX
		}

		private class WorkBenchRow
		{
			public WorkBenchRow(NelItem _Target, RCP.Recipe _TargetRecipe, RCP.RPI_CATEG _target_category, NelItem.CATEG _target_ni_category, EfParticleFuncCalc _CFn, EfParticleFuncCalc _CGradeFn)
			{
				this.Target = _Target;
				this.TargetRecipe = _TargetRecipe;
				this.target_category = _target_category;
				this.target_ni_category = _target_ni_category;
				this.CFn = _CFn;
				this.GradeFn = _CGradeFn;
			}

			public RCP.RecipeIngredient createRow(int index, float count)
			{
				int num = (int)(this.CFn.Get(-1f, count, false) + 0.0001f);
				if (num <= 0)
				{
					return null;
				}
				return new RCP.RecipeIngredient(this.Target, this.TargetRecipe, this.target_category, this.target_ni_category, 1f, num, num, (this.GradeFn == null) ? 0 : ((int)X.MMX(0f, this.GradeFn.Get(-1f, count, false), 4f)), index, -2, false);
			}

			public readonly NelItem Target;

			public readonly RCP.Recipe TargetRecipe;

			public readonly RCP.RPI_CATEG target_category;

			public readonly NelItem.CATEG target_ni_category;

			public EfParticleFuncCalc CFn;

			public EfParticleFuncCalc GradeFn;
		}
	}
}
