using System;
using System.Collections.Generic;
using PixelLiner;
using XX;

namespace nel
{
	public sealed class UiItemWholeSelector : UiBoxDesignerFamily
	{
		public static string WCATEG_ToStr(UiItemWholeSelector.WCATEG wcateg)
		{
			if (wcateg == UiItemWholeSelector.WCATEG._ALL)
			{
				return "SUMMONER";
			}
			return FEnum<UiItemWholeSelector.WCATEG>.ToStr(wcateg);
		}

		public static BtnContainerRadio<aBtn> prepareDesigner<TWlist, TItemList>(Designer WList, Designer ItemList, int wlist_w, int wlist_h, int wlist_clms, string wlist_skin = "checkbox_string", bool wlist_unselectable = true, string ilist_skin = "row", int ilist_h = 20) where TWlist : aBtn where TItemList : aBtn
		{
			string[] array = new string[X.beki_cnt(32768U) + 1];
			for (int i = 0; i < array.Length; i++)
			{
				UiItemWholeSelector.WCATEG wcateg = (UiItemWholeSelector.WCATEG)((i == 0) ? 0 : (1 << i - 1));
				array[i] = FEnum<UiItemWholeSelector.WCATEG>.ToStr(wcateg);
			}
			WList.addButtonMultiT<TWlist>(new DsnDataButtonMulti
			{
				name = "wlist",
				def = 1,
				titles = array,
				skin = wlist_skin,
				w = (float)wlist_w,
				h = (float)wlist_h,
				clms = wlist_clms,
				margin_w = 0f,
				margin_h = 0f,
				unselectable = (wlist_unselectable ? 2 : 0),
				fnClick = (aBtn B) => UiItemWholeSelector.fnClickWCheck<TItemList>(WList, ItemList, B, ilist_skin)
			});
			ItemList.Br();
			Designer itemList = ItemList;
			DsnDataRadio dsnDataRadio = new DsnDataRadio();
			dsnDataRadio.name = "ilist";
			dsnDataRadio.skin = ilist_skin;
			dsnDataRadio.def = -1;
			dsnDataRadio.w = ItemList.use_w - 10f - 16f;
			dsnDataRadio.h = (float)ilist_h;
			dsnDataRadio.clms = 1;
			dsnDataRadio.margin_h = 0;
			dsnDataRadio.margin_w = 0;
			dsnDataRadio.fnMakingAfter = delegate(BtnContainer<aBtn> BCon, aBtn B)
			{
				NelItem byId = NelItem.GetById(B.title, false);
				if (byId == null)
				{
					B.setSkinTitle("? " + byId.key);
				}
				else
				{
					B.setSkinTitle(byId.getLocalizedName(0, null));
				}
				return true;
			};
			dsnDataRadio.fnGenerateKeys = delegate(BtnContainerBasic BCon, List<string> Adest)
			{
				UiItemWholeSelector.fnGenerateIListKeys(WList, BCon, Adest);
			};
			dsnDataRadio.SCA = new ScrollAppend(30, ItemList.use_w, ItemList.use_h - 28f, 4f, 6f, 0);
			BtnContainerRadio<aBtn> btnContainerRadio = itemList.addRadioT<TItemList>(dsnDataRadio);
			btnContainerRadio.APool = new List<aBtn>(btnContainerRadio.Length);
			return btnContainerRadio;
		}

		public static bool fnClickWCheck<TItemList>(Designer WList, Designer ItemList, aBtn B, string ilist_skin) where TItemList : aBtn
		{
			if (B.title == "ALL")
			{
				if (B.isChecked())
				{
					return false;
				}
				B.Container.setValue("1");
			}
			else
			{
				B.SetChecked(!B.isChecked(), true);
				B.Container.GetButton(0).SetChecked(false, true);
				if (!B.isChecked() && B.Container.getValueString() == "0")
				{
					B.Container.setValue("1");
				}
			}
			UiItemWholeSelector.fineIList<TItemList>(WList, ItemList, ilist_skin);
			return true;
		}

		public static void fineIList<TItemList>(Designer WList, Designer ItemList, string ilist_skin) where TItemList : aBtn
		{
			BtnContainerRunner btnContainerRunner = ItemList.Get("ilist", false) as BtnContainerRunner;
			if (btnContainerRunner != null)
			{
				btnContainerRunner.BCon.RemakeT<TItemList>(null, ilist_skin);
			}
		}

		public static void fnGenerateIListKeys(Designer WList, BtnContainerBasic BCon, List<string> Adest)
		{
			int num = X.NmI(WList.getValue("wlist"), 0, false, false);
			foreach (KeyValuePair<string, NelItem> keyValuePair in NelItem.getWholeDictionary())
			{
				bool flag = num <= 1;
				NelItem value = keyValuePair.Value;
				if (!value.is_cache_item)
				{
					if (!flag)
					{
						UiItemWholeSelector.WCATEG wcateg = (UiItemWholeSelector.WCATEG)(num >> 1);
						flag = UiItemWholeSelector.isEnableForItm(value, wcateg);
					}
					if (flag)
					{
						Adest.Add(value.key);
					}
				}
			}
		}

		public static bool isEnableForItm(NelItem Itm, UiItemWholeSelector.WCATEG val)
		{
			bool flag = false;
			if (Itm == NelItem.Unknown)
			{
				return false;
			}
			if (Itm.is_reelmbox)
			{
				ReelManager.ItemReelContainer ir = ReelManager.GetIR(Itm);
				return ir != null && ir.useableItem && !TX.isStart(Itm.key, "itemreelC_", 0) && (val & UiItemWholeSelector.WCATEG.REEL) > UiItemWholeSelector.WCATEG.ALL;
			}
			if ((val & UiItemWholeSelector.WCATEG.CURE) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || (Itm.category & NelItem.CATEG.CURE_HP) != NelItem.CATEG.OTHER || (Itm.category & NelItem.CATEG.CURE_MP) != NelItem.CATEG.OTHER || (Itm.category & NelItem.CATEG.CURE_EP) > NelItem.CATEG.OTHER;
			}
			if ((val & UiItemWholeSelector.WCATEG.BOMB) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || Itm.is_bomb;
			}
			if ((val & UiItemWholeSelector.WCATEG.MTR) != UiItemWholeSelector.WCATEG.ALL && !Itm.is_reelmbox && ((Itm.category & NelItem.CATEG.MTR) != NelItem.CATEG.OTHER || Itm.RecipeInfo != null))
			{
				if (Itm.RecipeInfo == null)
				{
					flag = true;
				}
				else if ((Itm.RecipeInfo.categ & RecipeManager.RPI_CATEG._NOT_FOR_COOKING) != (RecipeManager.RPI_CATEG)0)
				{
					flag = true;
				}
			}
			if ((val & UiItemWholeSelector.WCATEG.INGREDIENT) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || (!Itm.is_tool && Itm.RecipeInfo != null && (Itm.RecipeInfo.categ & RecipeManager.RPI_CATEG._FOR_COOKING) > (RecipeManager.RPI_CATEG)0);
			}
			if ((val & UiItemWholeSelector.WCATEG.WATER) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || ((Itm.category & NelItem.CATEG.WATER) != NelItem.CATEG.OTHER && (Itm.category & NelItem.CATEG.BOTTLE) == NelItem.CATEG.OTHER);
			}
			if ((val & UiItemWholeSelector.WCATEG.BOTTLE) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || (Itm.category & NelItem.CATEG.BOTTLE) > NelItem.CATEG.OTHER;
			}
			if ((val & UiItemWholeSelector.WCATEG.FRUIT) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || (Itm.category & NelItem.CATEG.FRUIT) > NelItem.CATEG.OTHER;
			}
			if ((val & UiItemWholeSelector.WCATEG.DUST) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || (Itm.category & NelItem.CATEG.DUST) > NelItem.CATEG.OTHER;
			}
			if ((val & UiItemWholeSelector.WCATEG.PRECIOUS) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || (Itm.is_precious && !Itm.is_enhancer && !Itm.is_skillbook && !Itm.is_tool && !Itm.is_workbench_craft && (!Itm.is_recipe && !Itm.is_workbench_craft && !Itm.is_trm_episode) && !Itm.is_barunder_spconfig);
			}
			if ((val & UiItemWholeSelector.WCATEG.ENHANCER) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || Itm.is_enhancer;
			}
			if ((val & UiItemWholeSelector.WCATEG.SKILL) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || Itm.is_skillbook;
			}
			if ((val & UiItemWholeSelector.WCATEG.TOOL) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || Itm.is_tool;
			}
			if ((val & UiItemWholeSelector.WCATEG.SPCONFIG) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || Itm.is_barunder_spconfig;
			}
			if ((val & UiItemWholeSelector.WCATEG.RECIPE) != UiItemWholeSelector.WCATEG.ALL)
			{
				flag = flag || (Itm.is_recipe && !TRMManager.isAlomaRecipe(Itm)) || Itm.is_workbench_craft || Itm.is_trm_episode;
			}
			return flag;
		}

		public static BList<NelItem> PopForSpecificCategory(UiItemWholeSelector.WCATEG val, BList<NelItem> AList = null)
		{
			if (AList == null)
			{
				AList = ListBuffer<NelItem>.Pop(0);
			}
			Dictionary<string, NelItem> wholeDictionary = NelItem.getWholeDictionary();
			TRMManager.fineExecute(false);
			foreach (KeyValuePair<string, NelItem> keyValuePair in wholeDictionary)
			{
				NelItem value = keyValuePair.Value;
				if (!value.is_cache_item && UiItemWholeSelector.isEnableForItm(value, val))
				{
					AList.Add(value);
				}
			}
			return AList;
		}

		public static PxlFrame getCategoryIconFor(UiItemWholeSelector.WCATEG val)
		{
			int num = -1;
			if (val <= UiItemWholeSelector.WCATEG.PRECIOUS)
			{
				if (val <= UiItemWholeSelector.WCATEG.WATER)
				{
					switch (val)
					{
					case UiItemWholeSelector.WCATEG.CURE:
						num = 12;
						break;
					case UiItemWholeSelector.WCATEG.BOMB:
						num = 45;
						break;
					case (UiItemWholeSelector.WCATEG)3U:
						break;
					case UiItemWholeSelector.WCATEG.MTR:
						num = 0;
						break;
					default:
						if (val != UiItemWholeSelector.WCATEG.INGREDIENT)
						{
							if (val == UiItemWholeSelector.WCATEG.WATER)
							{
								num = 6;
							}
						}
						else
						{
							num = 21;
						}
						break;
					}
				}
				else if (val <= UiItemWholeSelector.WCATEG.FRUIT)
				{
					if (val != UiItemWholeSelector.WCATEG.BOTTLE)
					{
						if (val == UiItemWholeSelector.WCATEG.FRUIT)
						{
							num = 2;
						}
					}
					else
					{
						num = 4;
					}
				}
				else if (val != UiItemWholeSelector.WCATEG.DUST)
				{
					if (val == UiItemWholeSelector.WCATEG.PRECIOUS)
					{
						num = 55;
					}
				}
				else
				{
					num = 36;
				}
			}
			else if (val <= UiItemWholeSelector.WCATEG.SKILL)
			{
				if (val != UiItemWholeSelector.WCATEG.TOOL)
				{
					if (val != UiItemWholeSelector.WCATEG.ENHANCER)
					{
						if (val == UiItemWholeSelector.WCATEG.SKILL)
						{
							num = 18;
						}
					}
					else
					{
						num = 19;
					}
				}
				else
				{
					num = 51;
				}
			}
			else if (val <= UiItemWholeSelector.WCATEG.RECIPE)
			{
				if (val != UiItemWholeSelector.WCATEG.REEL)
				{
					if (val == UiItemWholeSelector.WCATEG.RECIPE)
					{
						num = 28;
					}
				}
				else
				{
					num = 56;
				}
			}
			else if (val != UiItemWholeSelector.WCATEG.SPCONFIG)
			{
				if (val == UiItemWholeSelector.WCATEG._ALL)
				{
					num = 29;
				}
			}
			else
			{
				num = 62;
			}
			if (num >= 0)
			{
				return MTR.AItemIcon[num];
			}
			return null;
		}

		public static void ListUpSubForSpecificCategory(UiItemWholeSelector.WCATEG val, List<string> ASub)
		{
			ASub.Add("ALL");
			if (val <= UiItemWholeSelector.WCATEG.MTR)
			{
				if (val != UiItemWholeSelector.WCATEG.CURE)
				{
					if (val == UiItemWholeSelector.WCATEG.MTR)
					{
						int num = X.beki_cntC(524288U);
						for (int i = 0; i < num; i++)
						{
							RecipeManager.RPI_CATEG rpi_CATEG = (RecipeManager.RPI_CATEG)(1 << i);
							if ((rpi_CATEG & RecipeManager.RPI_CATEG._NOT_FOR_COOKING) != (RecipeManager.RPI_CATEG)0)
							{
								ASub.Add(FEnum<RecipeManager.RPI_CATEG>.ToStr(rpi_CATEG));
							}
						}
					}
				}
				else
				{
					ASub.Add(FEnum<UiItemWholeSelector.WSCATEG>.ToStr(UiItemWholeSelector.WSCATEG.CURE_HP));
					ASub.Add(FEnum<UiItemWholeSelector.WSCATEG>.ToStr(UiItemWholeSelector.WSCATEG.CURE_MP));
					ASub.Add(FEnum<UiItemWholeSelector.WSCATEG>.ToStr(UiItemWholeSelector.WSCATEG.CURE_EP));
				}
			}
			else if (val != UiItemWholeSelector.WCATEG.INGREDIENT)
			{
				if (val == UiItemWholeSelector.WCATEG.RECIPE)
				{
					int num = 5;
					for (int j = 0; j < num; j++)
					{
						RecipeManager.RP_CATEG rp_CATEG = (RecipeManager.RP_CATEG)j;
						ASub.Add(FEnum<RecipeManager.RP_CATEG>.ToStr(rp_CATEG));
					}
				}
			}
			else
			{
				int num = X.beki_cntC(524288U);
				for (int k = 0; k < num; k++)
				{
					RecipeManager.RPI_CATEG rpi_CATEG2 = (RecipeManager.RPI_CATEG)(1 << k);
					if ((rpi_CATEG2 & RecipeManager.RPI_CATEG._FOR_COOKING) != (RecipeManager.RPI_CATEG)0)
					{
						ASub.Add(FEnum<RecipeManager.RPI_CATEG>.ToStr(rpi_CATEG2));
					}
				}
			}
			if (ASub.Count == 1)
			{
				ASub[0] = "_";
			}
		}

		public static string getTxKeyForCategory(UiItemWholeSelector.WCATEG val)
		{
			if (val == UiItemWholeSelector.WCATEG.PRECIOUS)
			{
				return "Item_Tab_precious";
			}
			return "Catalog_categ_item_" + UiItemWholeSelector.WCATEG_ToStr(val).ToLower();
		}

		public static string getTitleForSubKey(UiItemWholeSelector.WCATEG val, string sub_key)
		{
			if (sub_key == "ALL")
			{
				return TX.Get("Catalog_categ_sub_all", "");
			}
			if (sub_key == "_")
			{
				return TX.Get(UiItemWholeSelector.getTxKeyForCategory(val), "");
			}
			if (val <= UiItemWholeSelector.WCATEG.MTR)
			{
				if (val == UiItemWholeSelector.WCATEG.CURE)
				{
					return TX.Get("Catalog_categ_sub_" + sub_key.ToLower(), "");
				}
				if (val != UiItemWholeSelector.WCATEG.MTR)
				{
					goto IL_00B9;
				}
			}
			else if (val != UiItemWholeSelector.WCATEG.INGREDIENT)
			{
				if (val != UiItemWholeSelector.WCATEG.RECIPE)
				{
					goto IL_00B9;
				}
				return TX.Get("recipe_categ_" + sub_key.ToLower(), "");
			}
			RecipeManager.RPI_CATEG rpi_CATEG;
			if (FEnum<RecipeManager.RPI_CATEG>.TryParse(sub_key, out rpi_CATEG, true))
			{
				return TX.Get("recipe_item_categ_" + sub_key.ToLower(), "");
			}
			return "";
			IL_00B9:
			return TX.Get("Catalog_categ_sub_all", "");
		}

		public static bool isValidForSubKey(NelItem Itm, UiItemWholeSelector.WCATEG val, string sub_key)
		{
			if (sub_key == "ALL" || sub_key == "_")
			{
				return true;
			}
			if (Itm.is_cache_item)
			{
				return false;
			}
			if (val <= UiItemWholeSelector.WCATEG.MTR)
			{
				if (val != UiItemWholeSelector.WCATEG.CURE)
				{
					if (val != UiItemWholeSelector.WCATEG.MTR)
					{
						return false;
					}
				}
				else
				{
					UiItemWholeSelector.WSCATEG wscateg;
					if (!FEnum<UiItemWholeSelector.WSCATEG>.TryParse(sub_key, out wscateg, true))
					{
						return false;
					}
					if (wscateg == UiItemWholeSelector.WSCATEG.CURE_HP && (Itm.category & NelItem.CATEG.CURE_HP) != NelItem.CATEG.OTHER)
					{
						return true;
					}
					if (wscateg == UiItemWholeSelector.WSCATEG.CURE_MP && (Itm.category & NelItem.CATEG.CURE_MP) != NelItem.CATEG.OTHER)
					{
						return true;
					}
					if (wscateg == UiItemWholeSelector.WSCATEG.CURE_EP && (Itm.category & NelItem.CATEG.CURE_EP) != NelItem.CATEG.OTHER)
					{
						return true;
					}
					return false;
				}
			}
			else if (val != UiItemWholeSelector.WCATEG.INGREDIENT)
			{
				if (val != UiItemWholeSelector.WCATEG.RECIPE)
				{
					return false;
				}
				RecipeManager.Recipe recipeAllType = UiCraftBase.getRecipeAllType(Itm);
				RecipeManager.RP_CATEG rp_CATEG;
				if (recipeAllType != null && FEnum<RecipeManager.RP_CATEG>.TryParse(sub_key, out rp_CATEG, true))
				{
					return recipeAllType.categ == rp_CATEG;
				}
				return false;
			}
			RecipeManager.RPI_CATEG rpi_CATEG;
			return Itm.RecipeInfo != null && FEnum<RecipeManager.RPI_CATEG>.TryParse(sub_key, out rpi_CATEG, true) && (Itm.RecipeInfo.categ & rpi_CATEG) > (RecipeManager.RPI_CATEG)0;
		}

		public enum WCATEG : uint
		{
			ALL,
			CURE,
			BOMB,
			MTR = 4U,
			INGREDIENT = 8U,
			WATER = 16U,
			BOTTLE = 32U,
			FRUIT = 64U,
			DUST = 128U,
			PRECIOUS = 256U,
			TOOL = 512U,
			ENHANCER = 1024U,
			SKILL = 2048U,
			REEL = 4096U,
			RECIPE = 8192U,
			SPCONFIG = 16384U,
			_ALL = 32768U,
			SUMMONER = 32768U,
			_ALL_FD = 65536U
		}

		public enum WSCATEG : byte
		{
			ALL,
			CURE_HP,
			CURE_MP,
			CURE_EP,
			MTR_ORE,
			MTR_ENEMY
		}
	}
}
