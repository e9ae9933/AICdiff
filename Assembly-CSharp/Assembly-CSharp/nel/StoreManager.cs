using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class StoreManager
	{
		public static void initG()
		{
			if (StoreManager.OStorage == null)
			{
				StoreManager.OStorage = new BDic<string, StoreManager>();
			}
			Dictionary<string, StoreManager> ostorage = StoreManager.OStorage;
			string text = "Night";
			StoreManager storeManager = new StoreManager("Night", true, null);
			storeManager.store_title_tx_key = "Talker_Nightingale";
			storeManager.icon_pf_key = "IconNightingale";
			storeManager.FnAlreadyMeet = () => GF.getC("NIG0") > 0U;
			ostorage[text] = storeManager;
			StoreManager.OStorage["Laevi"] = new StoreManager("Laevi", false, null)
			{
				store_title_tx_key = "Area_house",
				icon_pf_key = "IconLaevi"
			};
			Dictionary<string, StoreManager> ostorage2 = StoreManager.OStorage;
			string text2 = "CoffeeMaker";
			StoreManager storeManager2 = new StoreManager("CoffeeMaker", true, null);
			storeManager2.PFIcon = MTR.AImgMapMarker[7];
			storeManager2.FnAlreadyMeet = () => GF.getC("COF0") > 0U;
			ostorage2[text2] = storeManager2;
			Dictionary<string, StoreManager> ostorage3 = StoreManager.OStorage;
			string text3 = "Puppet";
			StoreManager storeManager3 = new StoreManager("Puppet", true, typeof(UiItemStorePuppetCrafts));
			storeManager3.FD_GetServiceRatio = new StoreManager.FnGetServiceRatio(PuppetRevenge.FD_StoreGetServiceRatio);
			storeManager3.store_title_tx_key = "Store_Puppet_Help_Title";
			storeManager3.icon_pf_key = "IconGolem";
			storeManager3.FnAlreadyMeet = () => !PuppetRevenge.first_meet;
			ostorage3[text3] = storeManager3;
			Func<bool> func = () => false;
			Func<bool> func2 = () => SCN.fine_pvv(false) >= 105;
			Dictionary<string, StoreManager> ostorage4 = StoreManager.OStorage;
			string text4 = "BarUnder";
			StoreManager storeManager4 = new StoreManager("BarUnder", true, typeof(UiItemStoreBarUnder));
			storeManager4.FnAlreadyMeet = () => SCN.sp_config_store_enabled;
			storeManager4.FnAutoLoadOnSave = func;
			ostorage4[text4] = storeManager4;
			StoreManager.OStorage["city_bread"] = new StoreManager("city_bread", true, null)
			{
				store_title_tx_key = null,
				FnAutoLoadOnSave = func
			};
			StoreManager.OStorage["city_bar"] = new StoreManager("city_bar", true, null)
			{
				store_title_tx_key = "Store_BarUnder_Help_Title",
				icon_pf_key = "IconBarten",
				FnAutoLoadOnSave = func
			};
			StoreManager.OStorage["city_bar_t02"] = new StoreManager("city_bar_t02", true, null)
			{
				store_title_tx_key = null,
				FnAutoLoadOnSave = func
			};
			StoreManager.OStorage["city_cafe"] = new StoreManager("city_cafe", true, null)
			{
				store_title_tx_key = null,
				FnAutoLoadOnSave = func
			};
			StoreManager.OStorage["city_slam_egg"] = new StoreManager("city_slam_egg", true, null)
			{
				store_title_tx_key = null,
				FnAutoLoadOnSave = func2
			};
			StoreManager.OStorage["city_vegi"] = new StoreManager("city_vegi", true, null)
			{
				store_title_tx_key = null,
				FnAutoLoadOnSave = func2
			};
			StoreManager.OStorage["city_stone"] = new StoreManager("city_stone", true, null)
			{
				store_title_tx_key = null,
				FnAutoLoadOnSave = func2
			};
			StoreManager.OStorage["scl_koubai"] = new StoreManager("scl_koubai", true, null)
			{
				store_title_tx_key = null,
				FnAutoLoadOnSave = func2
			};
			StoreManager.OStorage["city_guild"] = new StoreManager("city_guild", true, null)
			{
				store_title_tx_key = null,
				FnAutoLoadOnSave = func2
			};
		}

		public static void newGame()
		{
			if (StoreManager.OStorage == null)
			{
				StoreManager.initG();
			}
			foreach (KeyValuePair<string, StoreManager> keyValuePair in StoreManager.OStorage)
			{
				keyValuePair.Value.newGameItem();
			}
		}

		public static void reloadStorage(StoreManager.MODE _mode)
		{
			foreach (KeyValuePair<string, StoreManager> keyValuePair in StoreManager.OStorage)
			{
				keyValuePair.Value.need_summon_flush = _mode;
			}
		}

		public static void flushSoldItemsAll()
		{
			foreach (KeyValuePair<string, StoreManager> keyValuePair in StoreManager.OStorage)
			{
				keyValuePair.Value.flushSoldItems();
			}
		}

		private static void reloadStorage(string k, StoreManager St, StoreManager.MODE _mode = StoreManager.MODE.FLUSH)
		{
			if (_mode == StoreManager.MODE.NONE)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			if ((_mode & StoreManager.MODE.READ_BASIC) != StoreManager.MODE.NONE)
			{
				flag2 = true;
				flag = _mode == StoreManager.MODE.READ_BASIC;
				_mode &= (StoreManager.MODE)(-5);
				if (St.APremire != null)
				{
					St.APremire.Clear();
				}
			}
			if ((_mode & StoreManager.MODE.REMAKE) != StoreManager.MODE.NONE)
			{
				St.clearAllItems(false);
				_mode = StoreManager.MODE.REMAKE;
			}
			else
			{
				_mode &= StoreManager.MODE.FLUSH;
			}
			StoreManager.cur_line_key = "";
			St.createListenerEval();
			StoreManager.readStorageInner(St, St.name, _mode, flag2, flag, null);
			TX.removeListenerEval(St);
			St.load_vers = 4;
			St.need_summon_flush_ = StoreManager.MODE.NONE;
		}

		private static void readStorageInner(StoreManager St, string name, StoreManager.MODE _mode, bool read_basic, bool only_basic, CsvVariableContainer PreVarCon = null)
		{
			string textResourceByName = StoreManager.getTextResourceByName(name);
			if (TX.noe(textResourceByName))
			{
				return;
			}
			CsvReaderA csvReaderA = new CsvReaderA(textResourceByName, PreVarCon == null);
			csvReaderA.tilde_replace = true;
			if (PreVarCon == null)
			{
				csvReaderA.VarCon.define("_MODE", _mode.ToString(), true);
				csvReaderA.VarCon.define("_vers", St.load_vers.ToString(), true);
				if (M2DBase.Instance != null)
				{
					NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
					if (nelM2DBase.WM != null)
					{
						WholeMapItem curWM = nelM2DBase.WM.CurWM;
						csvReaderA.VarCon.define("_WM", (curWM != null) ? curWM.text_key : "", true);
					}
				}
			}
			else
			{
				csvReaderA.VarCon = PreVarCon;
			}
			RCP.Recipe recipe = null;
			RCP.MakeDishDescription makeDishDescription = null;
			List<List<UiCraftBase.IngEntryRow>> list = null;
			BDic<string, List<List<UiCraftBase.IngEntryRow>>> bdic = null;
			int num = -1;
			bool flag = false;
			bool flag2;
			while ((flag2 = csvReaderA.read()) || recipe != null)
			{
				if (recipe != null && csvReaderA.get_cur_line() >= num)
				{
					if (makeDishDescription != null)
					{
						RCP.RecipeDish recipeDish = recipe.createDish(list, false, makeDishDescription);
						recipeDish.finalizeDishEffect();
						if (makeDishDescription.price < 0)
						{
							recipeDish.calcPrice(list);
						}
						else
						{
							recipeDish.price = makeDishDescription.price;
						}
						RCP.RecipeDish recipeDish2 = RCP.findSameDish(recipeDish);
						if (recipeDish2 != null)
						{
							recipeDish = recipeDish2;
							recipeDish.referred += makeDishDescription.count;
						}
						else
						{
							recipeDish.fineTitle(list, true, makeDishDescription);
							recipeDish.referred = 1 + makeDishDescription.count;
							RCP.assignDish(recipeDish);
						}
						St.Add(recipeDish.ItemData, makeDishDescription.count, recipeDish.calced_grade, StoreManager.cur_line_key, !flag);
						flag = true;
					}
					makeDishDescription = null;
					list = null;
					recipe = null;
					if (!flag2)
					{
						break;
					}
				}
				if (csvReaderA.cmd == "##SCRIPT")
				{
					StoreManager.readStorageInner(St, csvReaderA._1, _mode, read_basic, only_basic, csvReaderA.VarCon);
				}
				else if (TX.isStart(csvReaderA.cmd, "#", 0))
				{
					if (read_basic)
					{
						StoreManager.ITMMATCH itmmatch4;
						if (csvReaderA.cmd == "#SELL_RATIO")
						{
							St.sell_ratio = X.Mx(0f, csvReaderA.Nm(1, 0.4f));
						}
						else if (csvReaderA.cmd == "#BUY_RATIO")
						{
							St.buy_ratio = X.Mx(0f, csvReaderA.Nm(1, 0f));
						}
						else if (csvReaderA.cmd == "#CANNOT_SELL_NOT_PREMIRE")
						{
							St.cannot_sell_not_premire = csvReaderA.Nm(1, 1f) != 0f;
						}
						else if (csvReaderA.cmd == "#PREMIRE_GRADE_REVERSE")
						{
							StoreManager.ITMMATCH itmmatch;
							if (FEnum<StoreManager.ITMMATCH>.TryParse(csvReaderA._1, out itmmatch, true))
							{
								string text = St.addPremireCategory(itmmatch | StoreManager.ITMMATCH._REVERSE_GRADE, csvReaderA._2, 1f, 1f);
								if (text != null)
								{
									csvReaderA.tError(text);
								}
							}
						}
						else if (csvReaderA.cmd == "#PREMIRE")
						{
							StoreManager.ITMMATCH itmmatch2;
							if (FEnum<StoreManager.ITMMATCH>.TryParse(csvReaderA._1, out itmmatch2, true))
							{
								string text2 = St.addPremireCategory(itmmatch2, csvReaderA._2, St.NmBuy(csvReaderA._3), St.NmSell(csvReaderA._4));
								if (text2 != null)
								{
									csvReaderA.tError(text2);
								}
							}
						}
						else if (csvReaderA.cmd == "#PREMIRESELL")
						{
							StoreManager.ITMMATCH itmmatch3;
							if (FEnum<StoreManager.ITMMATCH>.TryParse(csvReaderA._1, out itmmatch3, true))
							{
								float num2 = St.NmSell(csvReaderA._3);
								float num3 = csvReaderA.Nm(4, 2f);
								string text3 = St.addPremireCategory(itmmatch3, csvReaderA._2, num2 * num3 / St.buy_ratio * St.sell_ratio, num2);
								if (text3 != null)
								{
									csvReaderA.tError(text3);
								}
							}
						}
						else if (csvReaderA.cmd == "#PREMIRESELL_ID_ABS")
						{
							NelItem byId = NelItem.GetById(csvReaderA._1, true);
							if (byId == null)
							{
								csvReaderA.tError("不明なアイテム: " + csvReaderA._1);
							}
							else
							{
								float num4 = St.NmSell(csvReaderA._2) / byId.getSellPrice(0);
								float num5 = csvReaderA.Nm(3, 2f);
								St.addPremireCategory(StoreManager.ITMMATCH.ID, (uint)byId.id, num4 * num5 / St.buy_ratio * St.sell_ratio, num4);
							}
						}
						else if (csvReaderA.cmd == "#PREMIREBUY" && FEnum<StoreManager.ITMMATCH>.TryParse(csvReaderA._1, out itmmatch4, true))
						{
							float num6 = St.NmBuy(csvReaderA._3);
							float num7 = csvReaderA.Nm(4, 0.5f);
							string text4 = St.addPremireCategory(itmmatch4, csvReaderA._2, num6, num6 * num7 / St.sell_ratio * St.buy_ratio);
							if (text4 != null)
							{
								csvReaderA.tError(text4);
							}
						}
					}
				}
				else if (!only_basic)
				{
					if (recipe != null)
					{
						if (csvReaderA.cmd == "DESC")
						{
							if (makeDishDescription == null)
							{
								csvReaderA.tError("内部レシピ登録中ははDESCを使用できません");
							}
							else
							{
								string text5 = csvReaderA._1;
								if (text5 != null)
								{
									if (text5 == "cost_add")
									{
										makeDishDescription.cost_add = csvReaderA.Int(2, 0);
										continue;
									}
									if (text5 == "cost_fix")
									{
										makeDishDescription.cost_fix = csvReaderA.Int(2, 0);
										continue;
									}
									if (text5 == "power_multiply")
									{
										makeDishDescription.power_multiply = csvReaderA.Nm(2, 1f);
										continue;
									}
									if (text5 == "tx_no_grade")
									{
										makeDishDescription.tx_no_grade = csvReaderA.Nm(2, 1f) != 0f;
										continue;
									}
								}
								int i = csvReaderA.Int(1, -1);
								if (i >= 0)
								{
									while (i >= makeDishDescription.Count)
									{
										makeDishDescription.Add(new RCP.MakeDishIngDescription(1f));
									}
									RCP.MakeDishIngDescription makeDishIngDescription = makeDishDescription[i];
									text5 = csvReaderA._2;
									if (text5 == null)
									{
										goto IL_06BB;
									}
									if (!(text5 == "power_multiply"))
									{
										if (!(text5 == "power_weeken_ignore"))
										{
											if (!(text5 == "tx_enough_ignore"))
											{
												if (!(text5 == "tx_listup"))
												{
													if (!(text5 == "tx_ignore"))
													{
														goto IL_06BB;
													}
													makeDishIngDescription.tx_ignore = csvReaderA.Int(3, 1) != 0;
												}
												else
												{
													makeDishIngDescription.tx_listup = csvReaderA.Int(3, 1) != 0;
												}
											}
											else
											{
												makeDishIngDescription.tx_enough_ignore = csvReaderA.Int(3, 1) != 0;
											}
										}
										else
										{
											makeDishIngDescription.power_weeken_ignore = X.Nm(csvReaderA._3, 1f, true);
										}
									}
									else
									{
										makeDishIngDescription.power_multiply = X.Nm(csvReaderA._3, 1f, true);
									}
									IL_06D2:
									makeDishDescription[i] = makeDishIngDescription;
									continue;
									IL_06BB:
									csvReaderA.tError("不明な DESC 2:" + csvReaderA._2);
									goto IL_06D2;
								}
								csvReaderA.tError("不明な DESC:" + csvReaderA._1);
							}
						}
						else if (csvReaderA.cmd == "ING")
						{
							int j = csvReaderA.Int(1, -1);
							if (j < 0 || j >= recipe.AIng.Count)
							{
								csvReaderA.tError("不適切な ING index:" + csvReaderA._1);
							}
							else
							{
								List<List<UiCraftBase.IngEntryRow>> list2 = null;
								NelItem nelItem = null;
								if (TX.isStart(csvReaderA._2, "&", 0))
								{
									if (recipe.AIng[j].TargetRecipe == null)
									{
										csvReaderA.tError("不明なAAIng指定子" + csvReaderA._2);
										continue;
									}
									if (bdic != null)
									{
										bdic.TryGetValue(TX.slice(csvReaderA._2, 1), out list2);
									}
									if (list2 == null)
									{
										csvReaderA.tError("不明なAAIng指定子" + csvReaderA._2);
										continue;
									}
								}
								else
								{
									nelItem = NelItem.GetById(csvReaderA._2, true);
									if (nelItem == null)
									{
										csvReaderA.tError("不明なItem ID:" + csvReaderA._2);
										continue;
									}
								}
								int num8 = csvReaderA.Int(3, 1);
								int num9 = csvReaderA.Int(4, 0);
								while (j >= list.Count)
								{
									list.Add(null);
								}
								List<UiCraftBase.IngEntryRow> list3 = list[j];
								if (list3 == null)
								{
									list3 = (list[j] = new List<UiCraftBase.IngEntryRow>(recipe.AIng[j].max));
								}
								while (--num8 >= 0)
								{
									UiCraftBase.IngEntryRow ingEntryRow = new UiCraftBase.IngEntryRow(null, recipe.AIng[j], nelItem, num9)
									{
										index = list3.Count
									};
									list3.Add(ingEntryRow);
									if (list2 != null)
									{
										ingEntryRow.setInnerIngredientFromBinary(list2);
									}
								}
							}
						}
					}
					else if (csvReaderA.cmd == "%FLUSH")
					{
						if (TX.noe(csvReaderA._1))
						{
							St.flushCategoryTemporary();
						}
						else
						{
							St.flushCategory(csvReaderA._1);
						}
					}
					else if (TX.isStart(csvReaderA.cmd, '@'))
					{
						StoreManager.cur_line_key = TX.slice(csvReaderA.cmd, 1);
						flag = false;
					}
					else if (csvReaderA.cmd == "%CLIP_CATEGORY_KIND")
					{
						St.clipCategoryCount(StoreManager.cur_line_key, csvReaderA.IntE(1, 0));
					}
					else if (csvReaderA.cmd == "%RECIPE")
					{
						if (csvReaderA.getIndex(csvReaderA.clength - 1) != "{")
						{
							csvReaderA.tError("RECIPE エントリー最後の行は { で終わること");
						}
						else
						{
							recipe = RCP.Get(csvReaderA._1);
							if (recipe == null)
							{
								csvReaderA.tError("不明な Recipe: " + csvReaderA._1);
							}
							else
							{
								if (csvReaderA.clength == 5)
								{
									makeDishDescription = new RCP.MakeDishDescription(recipe.AIng.Count)
									{
										count = csvReaderA.Int(2, 0),
										price = csvReaderA.Int(3, -1)
									};
									list = new List<List<UiCraftBase.IngEntryRow>>(recipe.AIng.Count);
								}
								else
								{
									makeDishDescription = null;
									if (!REG.match(csvReaderA._2, REG.RegVariable))
									{
										csvReaderA.tError("不適切なAAIng指定子 " + csvReaderA._1);
										recipe = null;
										continue;
									}
									if (bdic == null)
									{
										bdic = new BDic<string, List<List<UiCraftBase.IngEntryRow>>>(1);
									}
									if (bdic.ContainsKey(csvReaderA._2))
									{
										csvReaderA.tError("重複したAAIng指定子 " + csvReaderA._1);
										recipe = null;
										continue;
									}
									list = (bdic[csvReaderA._2] = new List<List<UiCraftBase.IngEntryRow>>(recipe.AIng.Count));
								}
								int cur_line = csvReaderA.get_cur_line();
								csvReaderA.jumpToNextBracket(4);
								num = csvReaderA.get_cur_line();
								csvReaderA.seek_set(cur_line);
							}
						}
					}
					else
					{
						int num10 = 0;
						string text6 = csvReaderA.cmd;
						if (TX.isStart(text6, "+", 0))
						{
							text6 = TX.slice(text6, 1);
							num10 = 1;
						}
						else if (TX.isStart(text6, "-", 0))
						{
							text6 = TX.slice(text6, 1);
							num10 = 2;
						}
						NelItem byId2 = NelItem.GetById(text6, false);
						if (byId2 == null)
						{
							csvReaderA.tError("不明なアイテムID: " + csvReaderA.cmd);
						}
						else if (num10 == 2)
						{
							int num11 = csvReaderA.IntE(1, 9999);
							int num12 = csvReaderA.IntE(2, -1);
							St.Reduce(byId2, num11, num12, StoreManager.cur_line_key);
						}
						else
						{
							int num13 = csvReaderA.IntE(1, 1);
							int num14 = X.MMX(0, csvReaderA.IntE(2, 0), 4);
							if (num10 == 0)
							{
								num13 = X.Mx(0, num13 - St.getCount(byId2, num14, StoreManager.cur_line_key));
							}
							if (num13 > 0)
							{
								St.Add(byId2, num13, num14, StoreManager.cur_line_key, !flag);
								flag = true;
							}
						}
					}
				}
			}
		}

		public void createListenerEval()
		{
			if (this.EvalLsn == null)
			{
				this.EvalLsn = TX.createListenerEval(this, 1, false);
				this.EvalLsn.Add("category_count", delegate(TxEvalListenerContainer O, List<string> Aargs)
				{
					TX.InputE((float)this.getCategoryCount(X.Get<string>(Aargs, 0)));
				}, Array.Empty<string>());
				this.EvalLsn.Add("already_created", delegate(TxEvalListenerContainer O, List<string> Aargs)
				{
					if (Aargs.Count > 0)
					{
						TX.InputE((float)((this.Acreated_line_key.IndexOf(Aargs[0]) >= 0) ? 1 : 0));
					}
				}, Array.Empty<string>());
				this.EvalLsn.Add("total_buy", delegate(TxEvalListenerContainer O, List<string> Aargs)
				{
					TX.InputE((float)this.total_buy);
				}, Array.Empty<string>());
				this.EvalLsn.Add("total_sell", delegate(TxEvalListenerContainer O, List<string> Aargs)
				{
					TX.InputE((float)this.total_sell);
				}, Array.Empty<string>());
				this.EvalLsn.Add("StoreHas", delegate(TxEvalListenerContainer O, List<string> Aargs)
				{
					if (Aargs.Count > 0)
					{
						NelItem byId = NelItem.GetById(Aargs[0], false);
						if (byId == null)
						{
							TX.InputE(0f);
							return;
						}
						int num = ((Aargs.Count >= 2) ? X.NmI(Aargs[1], 0, false, false) : 0);
						int num2 = 0;
						for (int i = num; i < 5; i++)
						{
							num2 += this.getCount(byId, i, StoreManager.cur_line_key);
						}
						TX.InputE((float)num2);
					}
				}, Array.Empty<string>());
				return;
			}
			TX.addListenerEval(this, this.EvalLsn);
		}

		public static StoreManager Get(string k, bool no_error = false)
		{
			if (TX.noe(k))
			{
				return null;
			}
			if (StoreManager.OStorage == null)
			{
				StoreManager.initG();
			}
			StoreManager storeManager = X.Get<string, StoreManager>(StoreManager.OStorage, k);
			if (storeManager == null && !no_error)
			{
				X.de("不明なストアマネージャ: " + k, null);
			}
			return storeManager;
		}

		public static BDic<string, StoreManager> GetWholeStoreObject()
		{
			if (StoreManager.OStorage == null)
			{
				StoreManager.initG();
			}
			return StoreManager.OStorage;
		}

		public static void FlushAll(bool remaking = false)
		{
			foreach (KeyValuePair<string, StoreManager> keyValuePair in StoreManager.OStorage)
			{
				keyValuePair.Value.need_summon_flush = (remaking ? StoreManager.MODE.REMAKE : StoreManager.MODE.FLUSH);
			}
		}

		public static void FlushWanderingStore(StoreManager.MODE mode)
		{
			foreach (KeyValuePair<string, StoreManager> keyValuePair in StoreManager.OStorage)
			{
				if (keyValuePair.Value.wandering)
				{
					keyValuePair.Value.need_summon_flush = mode;
				}
			}
		}

		public static void fineStorageBeforeSaveS()
		{
			foreach (KeyValuePair<string, StoreManager> keyValuePair in StoreManager.OStorage)
			{
				keyValuePair.Value.fineStorageBeforeSave();
			}
		}

		public static ByteArray StoreWholeWriteBinaryTo(ByteArray Ba)
		{
			using (BList<string> blist = X.objKeysB<string, StoreManager>(StoreManager.OStorage))
			{
				Ba.writeByte(1);
				int count = blist.Count;
				Ba.writeByte(count);
				for (int i = 0; i < count; i++)
				{
					Ba.writeString(blist[i], "utf-8");
					StoreManager.OStorage[blist[i]].writeBinaryTo(Ba);
				}
			}
			return Ba;
		}

		public static void StoreWholeReadBinaryFrom(ByteReader Ba)
		{
			StoreManager.newGame();
			int num = (int)Ba.readUByte();
			int num2 = (int)Ba.readUByte();
			for (int i = 0; i < num2; i++)
			{
				StoreManager storeManager = StoreManager.Get(Ba.readString("utf-8", false), true);
				if (storeManager == null)
				{
					storeManager = new StoreManager("_temp", false, null);
				}
				storeManager.readBinaryFrom(Ba, num);
			}
		}

		public StoreManager(string _name, bool _wandering = false, Type _TUi = null)
		{
			this.TUi = _TUi;
			this.name = _name;
			this.store_title_tx_key = "Talker_" + this.name;
			this.wandering = _wandering;
			this.AItm = new List<StoreManager.StoreEntry>(64);
			this.Acreated_line_key = new List<string>(6);
			this.clearAllItems(true);
		}

		private string getTextResource()
		{
			return StoreManager.getTextResourceByName(this.name);
		}

		private static string getTextResourceByName(string name)
		{
			return TX.getResource("Data/store/" + name + ".store", ".csv", false);
		}

		public void newGameItem()
		{
			this.clearAllItems(true);
			this.Acreated_line_key.Clear();
			this.releasePremireCache();
			this.total_buy = 0U;
			this.total_sell = 0U;
		}

		private void clearAllItems(bool remaking = false)
		{
			this.need_summon_flush_ &= StoreManager.MODE.READ_BASIC;
			if (remaking)
			{
				this.need_summon_flush_ |= (StoreManager.MODE)5;
			}
			this.AItm.Clear();
			this.load_vers = 0;
		}

		public string icon_pf_key
		{
			set
			{
				this.PFIcon = MTRX.getPF(value);
			}
		}

		public ItemStorage CreateItemStorage(out string service_buy_tx_key, out string service_sell_tx_key)
		{
			if (this.need_summon_flush_ != StoreManager.MODE.NONE)
			{
				StoreManager.reloadStorage(this.name, this, this.need_summon_flush_);
			}
			ItemStorage itemStorage = new ItemStorage(this.name, 99);
			itemStorage.clearAllItems(99);
			itemStorage.water_stockable = true;
			itemStorage.infinit_stockable = true;
			itemStorage.grade_split = true;
			itemStorage.auto_splice_zero_row = false;
			itemStorage.check_quest_target = true;
			int count = this.AItm.Count;
			for (int i = 0; i < count; i++)
			{
				StoreManager.StoreEntry storeEntry = this.AItm[i];
				if (storeEntry.count > 0)
				{
					itemStorage.Add(storeEntry.Data, storeEntry.count, (int)storeEntry.grade, true, true);
				}
			}
			itemStorage.setSort(ItemStorage.SORT_TYPE.KIND, false);
			itemStorage.select_row_key = "";
			this.temp_buy_ratio = (this.temp_sell_ratio = 1f);
			string text;
			service_sell_tx_key = (text = null);
			service_buy_tx_key = text;
			if (this.FD_GetServiceRatio != null)
			{
				this.FD_GetServiceRatio(this, itemStorage, ref this.temp_buy_ratio, out service_buy_tx_key, ref this.temp_sell_ratio, out service_sell_tx_key);
			}
			return itemStorage;
		}

		public StoreManager.StoreEntry Add(NelItem _Data, int _count, int _grade, string _line_key, bool add_to_created_line = false)
		{
			StoreManager.StoreEntry storeEntry = this.GetEntry(_Data, _grade, _line_key);
			if (storeEntry != null)
			{
				storeEntry.count = X.Mn(storeEntry.count + _count, 999);
				return storeEntry;
			}
			storeEntry = new StoreManager.StoreEntry(_Data, _count, (byte)_grade, _line_key);
			this.AItm.Add(storeEntry);
			if (add_to_created_line && _line_key != "_" && this.Acreated_line_key.IndexOf(_line_key) == -1)
			{
				this.Acreated_line_key.Add(_line_key);
			}
			return storeEntry;
		}

		private int Reduce(NelItem _Data, int _count, int _grade, string _line_key)
		{
			StoreManager.StoreEntry entry = this.GetEntry(_Data, _grade, _line_key);
			if (entry == null)
			{
				return 0;
			}
			_count = X.Mn(entry.count, _count);
			entry.count -= _count;
			if (entry.count <= 0)
			{
				this.AItm.Remove(entry);
			}
			return _count;
		}

		private int Reduce(NelItem _Data, int _count, int _grade)
		{
			int num = 0;
			int num2 = this.AItm.Count - 1;
			while (num2 >= 0 && _count > 0)
			{
				StoreManager.StoreEntry storeEntry = this.AItm[num2];
				if (storeEntry.Data == _Data && (int)storeEntry.grade == _grade)
				{
					int num3 = X.Mn(storeEntry.count, _count);
					storeEntry.count -= num3;
					_count -= num3;
					num += num3;
					if (storeEntry.count <= 0)
					{
						this.AItm.Remove(storeEntry);
					}
				}
				num2--;
			}
			return num;
		}

		private StoreManager.StoreEntry GetEntry(NelItem _Data, int _grade, string _line_key)
		{
			for (int i = this.AItm.Count - 1; i >= 0; i--)
			{
				StoreManager.StoreEntry storeEntry = this.AItm[i];
				if (storeEntry.Data == _Data && (int)storeEntry.grade == _grade && storeEntry.line_key == _line_key)
				{
					return storeEntry;
				}
			}
			return null;
		}

		public int getCount(NelItem _Data, int _grade, string _line_key)
		{
			StoreManager.StoreEntry entry = this.GetEntry(_Data, _grade, _line_key);
			if (entry == null)
			{
				return 0;
			}
			return entry.count;
		}

		public int getCategoryCount(string t)
		{
			int num = 0;
			for (int i = this.AItm.Count - 1; i >= 0; i--)
			{
				StoreManager.StoreEntry storeEntry = this.AItm[i];
				if (storeEntry.line_key == t)
				{
					num += storeEntry.count;
				}
			}
			return num;
		}

		public int countItems()
		{
			if (this.need_summon_flush_ != StoreManager.MODE.NONE)
			{
				StoreManager.reloadStorage(this.name, this, this.need_summon_flush_);
			}
			return this.AItm.Count;
		}

		private float NmSell(string n)
		{
			return this.getRatio(n, true);
		}

		private float NmBuy(string n)
		{
			return this.getRatio(n, false);
		}

		private float getRatio(string n, bool is_sell)
		{
			float num = 1f;
			int num2 = 0;
			float num4;
			using (STB stb = TX.PopBld(n, 0))
			{
				if (stb.isStart("!", 0))
				{
					num2 = 1;
					num = 1f / (is_sell ? this.sell_ratio : this.buy_ratio);
				}
				int num3;
				if (stb.Nm(num2, out num3, -1, false) != STB.PARSERES.ERROR)
				{
					num4 = num * (float)STB.parse_result_double;
				}
				else
				{
					num4 = -1000f;
				}
			}
			return num4;
		}

		public bool is_city
		{
			get
			{
				return TX.isStart(this.name, "city", 0);
			}
		}

		public bool fnSortInStore(ItemStorage.IRow Ra, ItemStorage.IRow Rb, ItemStorage.SORT_TYPE sort_type, out int ret, bool buying_mode)
		{
			ret = 0;
			if (sort_type == ItemStorage.SORT_TYPE.PRICE)
			{
				float num;
				float num2;
				if (buying_mode)
				{
					num = (float)this.buyPrice(Ra.Data, Ra.top_grade);
					num2 = (float)this.buyPrice(Rb.Data, Rb.top_grade);
				}
				else
				{
					num = (float)this.sellPrice(Ra.Data, Ra.top_grade);
					num2 = (float)this.sellPrice(Rb.Data, Rb.top_grade);
				}
				ret = ((num < num2) ? (-1) : 1);
				return true;
			}
			return false;
		}

		public UiItemStore AddStoreComponent(GameObject Gob)
		{
			UiItemStore uiItemStore;
			if (this.TUi == null)
			{
				uiItemStore = Gob.AddComponent<UiItemStore>();
			}
			else
			{
				uiItemStore = Gob.AddComponent(this.TUi) as UiItemStore;
			}
			return uiItemStore;
		}

		public void confirmCheckout(NelM2DBase M2D, ItemStorage StBuy, ItemStorage StSell, UiItemStore.StoreResult SRes)
		{
			for (int i = 0; i < 2; i++)
			{
				ItemStorage itemStorage = ((i == 0) ? StSell : StBuy);
				if (itemStorage != null)
				{
					foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in itemStorage.getWholeInfoDictionary())
					{
						for (int j = 0; j < 5; j++)
						{
							int num = keyValuePair.Value.getCount(j);
							if (num != 0)
							{
								if (i == 1)
								{
									num -= this.Reduce(keyValuePair.Key, num, j, "_SELL");
									if (num > 0)
									{
										keyValuePair.Key.addObtainCount(num);
										if (M2D != null && M2D.GUILD != null)
										{
											M2D.GUILD.addObtainItemForGQ(keyValuePair.Key, num, j);
										}
										this.Reduce(keyValuePair.Key, num, j);
									}
								}
								else
								{
									this.Add(keyValuePair.Key, num, j, "_SELL", false);
								}
							}
						}
					}
				}
			}
			if (SRes.buy_money > 0)
			{
				this.total_buy += (uint)SRes.buy_money;
			}
			if (SRes.sell_money > 0)
			{
				this.total_sell += (uint)SRes.sell_money;
			}
		}

		public void flushCategory(string key)
		{
			for (int i = this.AItm.Count - 1; i >= 0; i--)
			{
				if (this.AItm[i].line_key == key)
				{
					this.AItm.RemoveAt(i);
				}
			}
		}

		public void flushCategoryTemporary()
		{
			for (int i = this.AItm.Count - 1; i >= 0; i--)
			{
				if (this.AItm[i].isTemp())
				{
					this.AItm.RemoveAt(i);
				}
			}
		}

		public void flushSoldItems()
		{
			for (int i = this.AItm.Count - 1; i >= 0; i--)
			{
				if (this.AItm[i].isSoldItems())
				{
					this.AItm.RemoveAt(i);
				}
			}
		}

		public void clipCategoryCount(string key, int max)
		{
			List<StoreManager.StoreEntry> list = new List<StoreManager.StoreEntry>();
			for (int i = this.AItm.Count - 1; i >= 0; i--)
			{
				StoreManager.StoreEntry storeEntry = this.AItm[i];
				if (storeEntry.line_key == key)
				{
					list.Add(storeEntry);
					this.AItm.RemoveAt(i);
				}
			}
			if (list.Count == 0 || max <= 0)
			{
				return;
			}
			X.shuffle<StoreManager.StoreEntry>(list, -1, null);
			max = X.Mn(max, list.Count);
			for (int j = 0; j < max; j++)
			{
				this.AItm.Add(list[j]);
			}
		}

		public StoreManager.MODE need_summon_flush
		{
			get
			{
				return this.need_summon_flush_;
			}
			set
			{
				if (value != StoreManager.MODE.NONE && (this.need_summon_flush_ & value) != value)
				{
					this.need_summon_flush_ |= value;
				}
			}
		}

		public bool need_reload_basic
		{
			get
			{
				return (this.need_summon_flush_ & StoreManager.MODE.READ_BASIC) > StoreManager.MODE.NONE;
			}
			set
			{
				if (value)
				{
					this.need_summon_flush_ |= StoreManager.MODE.READ_BASIC;
				}
			}
		}

		public bool isAlreadyMeet()
		{
			return M2DBase.Instance == null || this.FnAlreadyMeet == null || this.FnAlreadyMeet();
		}

		public STB AddStoreTitleTo(STB Stb)
		{
			Stb.AddTxA(this.store_title_tx_key, false);
			return Stb;
		}

		public bool appear_on_field_guide
		{
			get
			{
				return TX.valid(this.store_title_tx_key);
			}
		}

		private string addPremireCategory(StoreManager.ITMMATCH pcateg0, string idstr, float rt_b, float rt_s)
		{
			if (this.APremire == null)
			{
				this.APremire = new List<StoreManager.Premire>(1);
			}
			uint num = 0U;
			StoreManager.ITMMATCH itmmatch = pcateg0 & StoreManager.ITMMATCH._BIT_MAIN;
			if (itmmatch == StoreManager.ITMMATCH.ID)
			{
				NelItem byId = NelItem.GetById(idstr, false);
				if (byId == null)
				{
					return null;
				}
				num = (uint)byId.id;
			}
			else if (itmmatch == StoreManager.ITMMATCH.CATEG_AND || itmmatch == StoreManager.ITMMATCH.CATEG_OR || itmmatch == StoreManager.ITMMATCH.CATEG_EQ)
			{
				num = (uint)NelItem.calcCateg(idstr);
				if (num == 0U)
				{
					return "不明な NelItem::CATEG: " + idstr;
				}
			}
			else if (itmmatch == StoreManager.ITMMATCH.RPI_CATEG_AND || itmmatch == StoreManager.ITMMATCH.RPI_CATEG_OR || itmmatch == StoreManager.ITMMATCH.RPI_CATEG_EQ)
			{
				RCP.RPI_CATEG rpi_CATEG;
				num = (uint)RCP.calcCateg(idstr, out rpi_CATEG);
				if (num == 0U)
				{
					return "不明な RPI_CATEG: " + idstr;
				}
			}
			this.addPremireCategory(pcateg0, num, rt_b, rt_s);
			return null;
		}

		private void addPremireCategory(StoreManager.ITMMATCH pcateg0, uint id, float rt_b, float rt_s)
		{
			float num = ((rt_b == -1000f) ? 1f : rt_b);
			float num2 = ((rt_s == -1000f) ? num : rt_s);
			StoreManager.Premire premire = new StoreManager.Premire(pcateg0, num, num2, id);
			for (int i = this.APremire.Count - 1; i >= 0; i--)
			{
				if (this.APremire[i].isEqual(premire))
				{
					this.APremire[i].Set(premire);
					return;
				}
			}
			this.APremire.Add(premire);
		}

		public int buyPrice(NelItem Itm, int grade)
		{
			bool flag;
			bool flag2;
			float num = this.getPremireRatio(Itm, false, out flag, out flag2) * this.buy_ratio;
			return (int)(Itm.getPrice(flag ? (4 - grade) : grade) * num * this.temp_buy_ratio);
		}

		public int sellPrice(NelItem Itm, int grade)
		{
			bool flag;
			bool flag2;
			float num = this.getPremireRatio(Itm, true, out flag, out flag2) * this.sell_ratio;
			if (!flag2 && this.cannot_sell_not_premire)
			{
				return 0;
			}
			return (int)(Itm.getSellPrice(flag ? (4 - grade) : grade) * num * this.temp_sell_ratio);
		}

		private float getPremireRatio(NelItem Itm, bool selling, out bool grade_reverse, out bool has_premire)
		{
			grade_reverse = (has_premire = false);
			if (this.APremire != null)
			{
				if (this.OItmPremireCache == null)
				{
					this.OItmPremireCache = new BDic<NelItem, StoreManager.PremireCache>(16);
				}
				StoreManager.PremireCache premireCache;
				if (this.OItmPremireCache.TryGetValue(Itm, out premireCache))
				{
					grade_reverse = premireCache.reverse_grade;
					has_premire = true;
					if (!selling)
					{
						return premireCache.Prm.buy_ratio;
					}
					return premireCache.Prm.sell_ratio;
				}
				else
				{
					int count = this.APremire.Count;
					for (int i = 0; i < count; i++)
					{
						StoreManager.Premire premire = this.APremire[i];
						if (premire.isMatch(Itm))
						{
							if (premire.is_grade_reverse)
							{
								grade_reverse = true;
							}
							else
							{
								this.OItmPremireCache[Itm] = new StoreManager.PremireCache(premire, grade_reverse);
								has_premire = true;
								if (!selling)
								{
									return premire.buy_ratio;
								}
								return premire.sell_ratio;
							}
						}
					}
				}
			}
			return 1f;
		}

		public void releasePremireCache()
		{
			this.OItmPremireCache = null;
		}

		public List<NelItemEntry> stealItems(List<NelItemEntry> Dest, int count_sold, int count_buy, float buy_item_replace_ratio = 0.1f)
		{
			using (BList<StoreManager.StoreEntry> blist = ListBuffer<StoreManager.StoreEntry>.Pop(this.AItm.Count))
			{
				int num = this.AItm.Count;
				for (int i = 0; i < num; i++)
				{
					StoreManager.StoreEntry storeEntry = this.AItm[i];
					if (storeEntry.line_key == "_")
					{
						blist.Add(storeEntry);
					}
				}
				while ((count_sold > 0 || count_buy > 0) && this.AItm.Count > 0)
				{
					int num2;
					if (count_sold > 0)
					{
						count_sold--;
						num2 = ((NightController.XORSP() < buy_item_replace_ratio) ? 0 : 1);
					}
					else
					{
						count_buy--;
						num2 = 0;
					}
					for (int j = num2; j < 2; j++)
					{
						List<StoreManager.StoreEntry> list = ((j == 0) ? this.AItm : blist);
						num = list.Count;
						if (num != 0)
						{
							int num3 = NightController.xors(num);
							StoreManager.StoreEntry storeEntry2 = list[num3];
							int num4 = storeEntry2.count;
							if ((float)num4 > (float)storeEntry2.Data.stock * 0.8f)
							{
								num4 = X.MMX(1, (int)((float)num4 * 0.8f), storeEntry2.Data.stock);
							}
							Dest.Add(new NelItemEntry(storeEntry2.Data, num4, storeEntry2.grade));
							storeEntry2.count -= num4;
							if (storeEntry2.count <= 0)
							{
								this.AItm.Remove(storeEntry2);
								blist.Remove(storeEntry2);
							}
							break;
						}
					}
				}
			}
			return Dest;
		}

		public List<StoreManager.StoreEntry> listUpByLineKey(List<StoreManager.StoreEntry> Dest, string category)
		{
			int count = this.AItm.Count;
			for (int i = 0; i < count; i++)
			{
				StoreManager.StoreEntry storeEntry = this.AItm[i];
				if (storeEntry.line_key == category)
				{
					Dest.Add(storeEntry);
				}
			}
			return Dest;
		}

		public void removeSpecificEntry(StoreManager.StoreEntry Entry)
		{
			this.AItm.Remove(Entry);
		}

		public List<NelItem> listUpWholeItem()
		{
			if (this.need_summon_flush_ != StoreManager.MODE.NONE)
			{
				StoreManager.reloadStorage(this.name, this, this.need_summon_flush_);
			}
			if (this.AWholeItem != null)
			{
				return this.AWholeItem;
			}
			this.AWholeItem = new List<NelItem>();
			using (SResourceReader sresourceReader = new SResourceReader(this.getTextResource(), true))
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					while (sresourceReader.readCorrectly(stb))
					{
						int num;
						stb.SkipSeparationItem(0, 1, out num, -1, null, null);
						if (num > 0 && stb[0] != '#' && stb[0] != '%' && stb[0] != '@' && stb[0] != '}' && !stb.isStart("IF", 0))
						{
							string text = stb.ToString(0, num);
							if (TX.valid(text))
							{
								NelItem byId = NelItem.GetById(text, true);
								if (byId != null && this.AWholeItem.IndexOf(byId) == -1)
								{
									this.AWholeItem.Add(byId);
								}
							}
						}
					}
				}
			}
			return this.AWholeItem;
		}

		public void fineStorageBeforeSave()
		{
			if ((this.need_summon_flush_ & (StoreManager.MODE)(-5)) != StoreManager.MODE.NONE && (this.FnAutoLoadOnSave == null || this.FnAutoLoadOnSave()))
			{
				StoreManager.reloadStorage(this.name, this, this.need_summon_flush_);
			}
		}

		private void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(4);
			Ba.writeByte((int)this.need_summon_flush_);
			int num = this.AItm.Count;
			Ba.writeUShort((ushort)num);
			for (int i = 0; i < num; i++)
			{
				StoreManager.StoreEntry storeEntry = this.AItm[i];
				Ba.writePascalString(storeEntry.Data.key, "utf-8");
				Ba.writeUShort((ushort)storeEntry.count);
				Ba.writeByte((int)storeEntry.grade);
				Ba.writePascalString(storeEntry.line_key, "utf-8");
			}
			num = this.Acreated_line_key.Count;
			Ba.writeUShort((ushort)num);
			for (int j = 0; j < num; j++)
			{
				Ba.writePascalString(this.Acreated_line_key[j], "utf-8");
			}
			Ba.writeUInt(this.total_buy);
			Ba.writeUInt(this.total_sell);
		}

		private void readBinaryFrom(ByteReader Ba, int whole_vers)
		{
			this.newGameItem();
			if (whole_vers >= 1)
			{
				this.load_vers = Ba.readByte();
			}
			else
			{
				this.load_vers = 1;
			}
			bool flag = this.load_vers >= 3;
			bool flag2 = this.load_vers >= 4;
			this.need_summon_flush_ = (this.need_summon_flush_ & StoreManager.MODE.READ_BASIC) | (StoreManager.MODE)Ba.readByte();
			int num = (int)Ba.readUShort();
			string text = null;
			for (int i = 0; i < num; i++)
			{
				NelItem byId = NelItem.GetById(Ba.readPascalString("utf-8", false), false);
				int num2 = (int)Ba.readUShort();
				byte b = (byte)Ba.readByte();
				string text2 = Ba.readPascalString("utf-8", false);
				if (text != text2)
				{
					text = text2;
					if (!flag && this.Acreated_line_key.IndexOf(text2) == -1)
					{
						this.Acreated_line_key.Add(text2);
					}
				}
				if (byId != null)
				{
					StoreManager.StoreEntry storeEntry = new StoreManager.StoreEntry(byId, num2, b, text2);
					this.AItm.Add(storeEntry);
					if (byId.RecipeInfo != null && byId.RecipeInfo.DishInfo != null)
					{
						byId.RecipeInfo.DishInfo.referred++;
					}
				}
			}
			if (flag)
			{
				num = (int)Ba.readUShort();
				for (int j = 0; j < num; j++)
				{
					this.Acreated_line_key.Add(Ba.readPascalString("utf-8", false));
				}
			}
			if (flag2)
			{
				this.total_buy = Ba.readUInt();
				this.total_sell = Ba.readUInt();
			}
		}

		public const string StoreDir = "Data/store/";

		public const string StoreExtSuffix = ".store";

		private static BDic<string, StoreManager> OStorage;

		public const float sell_ratio_default = 0.4f;

		private static string cur_line_key;

		private TxEvalListenerContainer EvalLsn;

		private int load_vers;

		private float sell_ratio = 0.4f;

		private float buy_ratio = 1f;

		private bool cannot_sell_not_premire;

		public readonly bool wandering;

		public readonly string name;

		public uint total_buy;

		public uint total_sell;

		private string store_title_tx_key;

		public PxlFrame PFIcon;

		private List<NelItem> AWholeItem;

		private StoreManager.MODE need_summon_flush_ = StoreManager.MODE.READ_BASIC;

		private readonly List<StoreManager.StoreEntry> AItm;

		private List<StoreManager.Premire> APremire;

		public readonly List<string> Acreated_line_key;

		private BDic<NelItem, StoreManager.PremireCache> OItmPremireCache;

		private readonly Type TUi;

		public float temp_buy_ratio = 1f;

		public float temp_sell_ratio = 1f;

		private Func<bool> FnAlreadyMeet;

		private Func<bool> FnAutoLoadOnSave;

		private StoreManager.FnGetServiceRatio FD_GetServiceRatio;

		public const string line_key_sell = "_SELL";

		private const int LOAD_VERS = 4;

		public delegate void FnGetServiceRatio(StoreManager Store, ItemStorage St, ref float buy_ratio, out string tx_key_buy, ref float sell_ratio, out string tx_key_sell);

		public enum ITMMATCH : byte
		{
			ID,
			CATEG_OR,
			CATEG_AND,
			CATEG_EQ,
			RPI_CATEG_OR,
			RPI_CATEG_AND,
			RPI_CATEG_EQ,
			_REVERSE_GRADE = 128,
			_BIT_MAIN = 127
		}

		private struct Premire
		{
			public Premire(StoreManager.ITMMATCH _categ, float _buy_ratio, float _sell_ratio, uint _id)
			{
				this.categ = _categ;
				this.buy_ratio = _buy_ratio;
				this.sell_ratio = _sell_ratio;
				this.id = _id;
			}

			public bool isMatch(NelItem Itm)
			{
				return StoreManager.Premire.isMatchS(Itm, this.categ, this.id);
			}

			public static bool isMatchS(NelItem Itm, StoreManager.ITMMATCH categ, uint id)
			{
				switch (categ & StoreManager.ITMMATCH._BIT_MAIN)
				{
				case StoreManager.ITMMATCH.CATEG_OR:
					return (Itm.category & (NelItem.CATEG)id) > NelItem.CATEG.OTHER;
				case StoreManager.ITMMATCH.CATEG_AND:
					return (Itm.category & (NelItem.CATEG)id) == (NelItem.CATEG)id;
				case StoreManager.ITMMATCH.CATEG_EQ:
					return Itm.category == (NelItem.CATEG)id;
				case StoreManager.ITMMATCH.RPI_CATEG_OR:
					return Itm.RecipeInfo != null && (Itm.RecipeInfo.categ & (RCP.RPI_CATEG)id) > (RCP.RPI_CATEG)0;
				case StoreManager.ITMMATCH.RPI_CATEG_AND:
					return Itm.RecipeInfo != null && (Itm.RecipeInfo.categ & (RCP.RPI_CATEG)id) == (RCP.RPI_CATEG)id;
				case StoreManager.ITMMATCH.RPI_CATEG_EQ:
					return Itm.RecipeInfo != null && Itm.RecipeInfo.categ == (RCP.RPI_CATEG)id;
				default:
					return id == (uint)Itm.id;
				}
			}

			public bool is_grade_reverse
			{
				get
				{
					return (this.categ & StoreManager.ITMMATCH._REVERSE_GRADE) > StoreManager.ITMMATCH.ID;
				}
			}

			public bool isEqual(StoreManager.Premire P)
			{
				return (P.categ & StoreManager.ITMMATCH._BIT_MAIN) == (this.categ & StoreManager.ITMMATCH._BIT_MAIN) && (P.categ & StoreManager.ITMMATCH._REVERSE_GRADE) == (this.categ & StoreManager.ITMMATCH._REVERSE_GRADE) && P.id == this.id;
			}

			public StoreManager.Premire Set(StoreManager.Premire P)
			{
				this.buy_ratio = P.buy_ratio;
				this.sell_ratio = P.sell_ratio;
				return this;
			}

			public readonly StoreManager.ITMMATCH categ;

			public float buy_ratio;

			public float sell_ratio;

			private uint id;
		}

		private struct PremireCache
		{
			public PremireCache(StoreManager.Premire _Prm, bool _reverse_grade)
			{
				this.Prm = _Prm;
				this.reverse_grade = _reverse_grade;
			}

			public StoreManager.Premire Prm;

			public bool reverse_grade;
		}

		public class StoreEntry : NelItemEntry
		{
			public StoreEntry(NelItem _Data, int _count, byte _grade, string _line_key)
				: base(_Data, _count, _grade)
			{
				this.line_key = _line_key;
			}

			public bool isTemp()
			{
				return TX.isStart(this.line_key, '_');
			}

			public bool isSoldItems()
			{
				return this.line_key == "_";
			}

			public string line_key;
		}

		public enum MODE
		{
			NONE,
			REMAKE,
			FLUSH,
			READ_BASIC = 4
		}
	}
}
