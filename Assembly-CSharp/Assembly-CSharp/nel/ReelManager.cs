using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class ReelManager : IEventWaitListener
	{
		public ReelManager(ReelManager Src = null)
		{
			this.AStackIR = new List<ReelManager.ItemReelContainer>(1);
			this.AEf = new List<ReelExecuter>(4);
			this.Aef_stack = new List<string>(8);
			this.newGame();
			ReelManager.initReelScript();
			if (Src != null)
			{
				this.obtainReels(Src);
			}
		}

		public void destructGob()
		{
			if (this.Ui != null)
			{
				this.digestObtainedMoney();
				this.digestObtainedItem(true);
				if (M2DBase.Instance != null)
				{
					M2DBase.Instance.remValotAddition(this.Ui);
				}
				this.Ui.destruct();
				IN.DestroyOne(this.Ui.gameObject);
				this.Ui = null;
				this.DropLp = null;
				this.AStackIR.Clear();
			}
		}

		public void destructExecuterReels(bool clear_array_content = true)
		{
			for (int i = this.AEf.Count - 1; i >= 0; i--)
			{
				this.AEf[i].destruct();
			}
			if (clear_array_content)
			{
				this.AEf.Clear();
			}
		}

		public void newGame()
		{
			this.clearReels(false, true, true);
			this.AStackIR.Clear();
		}

		public void assignDropLp(M2LabelPoint Lp)
		{
			this.DropLp = Lp ?? this.DropLp;
		}

		public static void initReelScript()
		{
			string resource = TX.getResource("Data/reel", ref ReelManager.LoadDate, ".csv", false, "Resources/");
			if (resource == null)
			{
				return;
			}
			SupplyManager.clearDate();
			CsvReaderA csvReaderA = new CsvReaderA(resource, false);
			ReelManager.OAreel_content = new NIDic<int, ReelManager.EFReel>("reel_content");
			XorsMaker xorsMaker = new XorsMaker(4085363881U, false);
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				int num = 0;
				int num2 = -1;
				while (csvReaderA.read())
				{
					ReelExecuter.EFFECT effect;
					if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
					{
						if (num2 != -1)
						{
							ReelManager.OAreel_content[num2] = new ReelManager.EFReel(blist, num);
						}
						blist.Clear();
						num = 0;
						string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
						ReelExecuter.ETYPE etype;
						if (!FEnum<ReelExecuter.ETYPE>.TryParse(index, out etype, true))
						{
							csvReaderA.tError("不明なETYPE: " + index);
						}
						num2 = (int)etype;
					}
					else if (!(csvReaderA.cmd == "%RARE") && FEnum<ReelExecuter.EFFECT>.TryParse(csvReaderA.last_input, out effect, true))
					{
						blist.Add(effect.ToString().ToUpper());
					}
				}
				if (num2 != -1)
				{
					ReelManager.OAreel_content[num2] = new ReelManager.EFReel(blist, num);
				}
			}
			ReelManager.OItemR = new NDic<ReelManager.ItemReelContainer>("Reel_ItemR", 12, 0);
			ReelManager.OColor = new NDic<ReelManager.ItemReelColor>("Reel_Color", 16, 0);
			ReelManager.AIRBuf = new List<ReelManager.ItemReelContainer>(3);
			ReelManager.ItemReelColor itemReelColor = (ReelManager.OColor["_"] = new ReelManager.ItemReelColor(4294440951U, 4287926159U, 4283391212U));
			List<ReelManager.ItemReelContainer> list = new List<ReelManager.ItemReelContainer>(8);
			List<NelItemEntry> list2 = new List<NelItemEntry>(8);
			csvReaderA.parseText(TX.getResource("Data/itemreel", ".csv", false));
			int num3 = 60000;
			while (csvReaderA.read())
			{
				if (TX.isStart(csvReaderA.cmd, '#'))
				{
					if (csvReaderA.cmd == "#COLOR")
					{
						ReelManager.ItemReelColor itemReelColor2 = (ReelManager.OColor[csvReaderA._1] = new ReelManager.ItemReelColor(X.NmUI(csvReaderA._2, 4294440951U, true, true), X.NmUI(csvReaderA._3, 4287926159U, true, true), X.NmUI(csvReaderA._4, 4283391212U, true, true)));
						if (csvReaderA._1 == "demon")
						{
						}
					}
				}
				else if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					string[] array = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1).Split(new char[] { '|' });
					list.Clear();
					for (int i = array.Length - 1; i >= 0; i--)
					{
						string text = array[i];
						if (!TX.noe(text))
						{
							ReelManager.ItemReelContainer itemReelContainer = X.Get<string, ReelManager.ItemReelContainer>(ReelManager.OItemR, text);
							if (itemReelContainer != null)
							{
								list.Add(itemReelContainer);
							}
							else
							{
								list.Add(ReelManager.OItemR[text] = (itemReelContainer = new ReelManager.ItemReelContainer(text, itemReelColor, null)));
								TX tx = TX.getTX("_ItemReel_name_" + text, true, true, TX.getDefaultFamily());
								itemReelContainer.useableItem = tx != null;
								for (int j = 0; j < 2; j++)
								{
									ReelManager.CreateReelItemEntry(((j == 0) ? "itemreelC_" : "itemreelG_") + text, num3++);
								}
							}
						}
					}
				}
				else if (TX.isStart(csvReaderA.cmd, '%'))
				{
					if (csvReaderA.cmd == "%COLOR")
					{
						ReelManager.ItemReelColor itemReelColor3 = X.Get<string, ReelManager.ItemReelColor>(ReelManager.OColor, csvReaderA._1);
						if (itemReelColor3 == null)
						{
							csvReaderA.tError("不明なカラーセット: " + csvReaderA._1);
						}
						else
						{
							for (int k = list.Count - 1; k >= 0; k--)
							{
								list[k].ColSet = itemReelColor3;
							}
						}
					}
					else if (csvReaderA.cmd == "%RARE")
					{
						for (int l = list.Count - 1; l >= 0; l--)
						{
							list[l].rarelity = (byte)csvReaderA.Int(1, 0);
						}
					}
					else if (csvReaderA.cmd == "%SHUFFLE")
					{
						for (int m = list.Count - 1; m >= 0; m--)
						{
							list[m].shuffle(xorsMaker);
						}
					}
					else if (csvReaderA.cmd == "%CLONE")
					{
						ReelManager.ItemReelContainer itemReelContainer2 = X.Get<string, ReelManager.ItemReelContainer>(ReelManager.OItemR, csvReaderA._1);
						if (itemReelContainer2 == null)
						{
							csvReaderA.tError("不明なSrc IR: " + csvReaderA._1);
						}
						else
						{
							for (int n = list.Count - 1; n >= 0; n--)
							{
								list[n].copyFrom(itemReelContainer2);
							}
						}
					}
					else if (csvReaderA.cmd == "%GRADE")
					{
						for (int num4 = list.Count - 1; num4 >= 0; num4--)
						{
							list[num4].addGrade(csvReaderA.Int(1, 0));
						}
					}
					else if (csvReaderA.cmd == "%COUNT")
					{
						for (int num5 = list.Count - 1; num5 >= 0; num5--)
						{
							list[num5].addCount(csvReaderA.Int(1, 0));
						}
					}
				}
				else
				{
					NelItem byId = NelItem.GetById(csvReaderA.cmd, false);
					if (byId != null)
					{
						list2.Clear();
						int num6 = csvReaderA.clength - 1;
						for (int num7 = 0; num7 < num6; num7++)
						{
							string index2 = csvReaderA.getIndex(num7 + 1);
							int num8 = -1000;
							byte b;
							if (REG.match(index2, ReelManager.RegComma))
							{
								NelItemEntry nelItemEntry = list2[num7 - index2.Length];
								num8 = nelItemEntry.count;
								b = nelItemEntry.grade;
							}
							else
							{
								int num9 = index2.IndexOf("g");
								if (num9 >= 0)
								{
									string text2 = TX.slice(index2, 0, num9);
									if (REG.match(text2, ReelManager.RegComma))
									{
										num8 = list2[num7 - text2.Length].count;
									}
									else
									{
										num8 = X.NmI(text2, num8, false, false);
									}
									text2 = TX.slice(index2, num9 + 1);
									if (REG.match(text2, ReelManager.RegComma))
									{
										b = list2[num7 - text2.Length].grade;
									}
									else
									{
										b = (byte)X.NmI(text2, 0, false, false);
									}
								}
								else
								{
									num8 = X.NmI(index2, num8, false, false);
									if (num7 == 0)
									{
										ReelManager.ItemReelContainer itemReelContainer3 = list[list.Count - 1];
										if (itemReelContainer3.Count > 0)
										{
											b = itemReelContainer3[itemReelContainer3.Count - 1].grade;
										}
										else
										{
											b = 0;
										}
									}
									else
									{
										b = list2[num7 - 1].grade;
									}
								}
							}
							if (num8 == -1000)
							{
								X.dl("不明な書式: " + index2, null, false, false);
							}
							else
							{
								if (num8 < 0)
								{
									num8 = -num8 * byId.stock;
								}
								list2.Add(new NelItemEntry(byId, num8, b));
							}
						}
						for (int num10 = list.Count - 1; num10 >= 0; num10--)
						{
							list[num10].AddRange(list2);
						}
					}
				}
			}
			ReelManager.OItemR.scriptFinalize();
			ReelManager.OColor.scriptFinalize();
			ReelManager.OAreel_content.scriptFinalize();
		}

		public static NelItem CreateReelItemEntry(string itemkey, int id)
		{
			return NelItem.CreateItemEntry(itemkey, new NelItem(itemkey, 0, 5, 1)
			{
				category = (NelItem.CATEG)2097281U,
				FnGetName = NelItem.fnGetNameItemReel,
				FnGetDesc = NelItem.fnGetDescItemReel,
				FnGetDetail = NelItem.fnGetDetailItemReel,
				FnGetColor = new FnGetItemColor(NelItem.fnGetColorItemReel),
				stock = 1
			}, id, true);
		}

		public void obtain(ReelExecuter.ETYPE type)
		{
			ReelExecuter reelExecuter = new ReelExecuter(this, type);
			this.AEf.Add(reelExecuter);
			if (this.Ui != null)
			{
				this.Ui.obtain(reelExecuter);
			}
		}

		public void obtainReels(ReelManager Src)
		{
			this.AEf.AddRange(Src.AEf);
		}

		public void clearReels(bool only_executer = false, bool remove_gob = true, bool flush_obtainable = true)
		{
			if (only_executer)
			{
				this.destructExecuterReels(true);
				return;
			}
			if (remove_gob)
			{
				this.destructGob();
			}
			this.destructExecuterReels(true);
			this.AStackIR.Clear();
			this.Aef_stack.Clear();
			if (flush_obtainable)
			{
				this.flushObtainableReel();
			}
		}

		public void flushObtainableReel()
		{
			this.Aef_stack.Clear();
			using (BList<string> blist = ListBuffer<string>.Pop(6))
			{
				blist.Add("GRADE1");
				blist.Add("COUNT_ADD1");
				if (X.XORSP() < 0.5f)
				{
					blist.Add((X.XORSP() < 0.5f) ? "GRADE2" : "COUNT_ADD2");
				}
				else
				{
					blist.Add((X.XORSP() < 0.5f) ? "ADD_MONEY" : "COUNT_ADD1");
				}
				X.shuffle<string>(blist, -1, null);
				this.Aef_stack.AddRange(blist);
				this.Aef_stack.Add("COUNT_MUL1");
				this.addObtainableReelAfter(0);
			}
		}

		public void addObtainableReelAfter(int superiour = 0)
		{
			using (BList<string> blist = ListBuffer<string>.Pop(6))
			{
				blist.Clear();
				if (superiour >= 1)
				{
					blist.Add((X.XORSP() < 0.5f) ? "RANDOM" : "ADD_MONEY");
				}
				else
				{
					blist.Add("RANDOM");
					blist.Add("ADD_MONEY");
					blist.Add((X.XORSP() < 0.3f) ? "GRADE1" : "COUNT_ADD1");
				}
				if (X.XORSP() < 0.8f)
				{
					blist.Add((X.XORSP() < 0.3f) ? "GRADE2" : "COUNT_ADD2");
				}
				else
				{
					blist.Add((X.XORSP() < 0.3f) ? ((superiour >= 2) ? "RANDOM" : "GRADE1") : "COUNT_ADD1");
				}
				if (superiour >= 1)
				{
					blist.Add((X.XORSP() < 0.4f) ? "GRADE3" : "COUNT_ADD3");
					if (superiour >= 2)
					{
						blist.Add((X.XORSP() < 0.3f) ? "GRADE3" : "COUNT_ADD3");
						blist.Add((X.XORSP() < 0.3f) ? "COUNT_MUL1" : "COUNT_ADD3");
					}
				}
				X.shuffle<string>(blist, -1, null);
				for (int i = 0; i < 3; i++)
				{
					this.Aef_stack.Add(blist[i]);
				}
			}
		}

		public void obtainProgress(int reel_obtained)
		{
			if (this.Aef_stack.Count == 0)
			{
				this.addObtainableReelAfter((reel_obtained >= 15) ? 2 : ((reel_obtained >= 7) ? 1 : 0));
			}
			string text = this.Aef_stack[0];
			this.Aef_stack.RemoveAt(0);
			ReelExecuter.ETYPE etype;
			if (FEnum<ReelExecuter.ETYPE>.TryParse(text, out etype, true))
			{
				this.obtain(etype);
				return;
			}
			X.de("不明なETYPE:" + text, null);
		}

		public void overwriteEfReel(List<ReelExecuter.ETYPE> Atype)
		{
			this.destructExecuterReels(true);
			int count = Atype.Count;
			for (int i = 0; i < count; i++)
			{
				this.obtain(Atype[i]);
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			int num = this.AEf.Count;
			Ba.writeUShort((ushort)num);
			for (int i = 0; i < num; i++)
			{
				Ba.writeByte((int)this.AEf[i].getEType());
			}
			num = this.Aef_stack.Count;
			Ba.writeUShort((ushort)num);
			for (int j = 0; j < num; j++)
			{
				ReelExecuter.ETYPE etype;
				if (FEnum<ReelExecuter.ETYPE>.TryParse(this.Aef_stack[j], out etype, true))
				{
					Ba.writeByte((int)etype);
				}
				else
				{
					Ba.writeByte(0);
				}
			}
		}

		public void readBinaryFrom(ByteReader Ba, int vers = 9)
		{
			this.destructExecuterReels(true);
			this.Aef_stack.Clear();
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				ReelExecuter.ETYPE etype = (ReelExecuter.ETYPE)Ba.readByte();
				if (X.BTW(1f, (float)etype, 11f))
				{
					this.obtain(etype);
				}
			}
			if (vers >= 4)
			{
				num = (int)Ba.readUShort();
				for (int j = 0; j < num; j++)
				{
					ReelExecuter.ETYPE etype2 = (ReelExecuter.ETYPE)Ba.readByte();
					if (X.BTW(1f, (float)etype2, 11f))
					{
						this.Aef_stack.Add(etype2.ToString());
					}
				}
			}
		}

		public UiReelManager initUiState(ReelManager.MSTATE stt, Transform Parent = null, bool recreate_instance = true)
		{
			if (this.Ui != null && recreate_instance)
			{
				this.destructGob();
			}
			if (this.Ui == null)
			{
				this.Ui = new GameObject("ReelUI").AddComponent<UiReelManager>();
				if (Parent != null)
				{
					this.Ui.transform.SetParent(Parent);
				}
				this.Ui.InitUiReelManager(this);
				this.Ui.use_valotile = M2DBase.Instance.use_valotile;
				if (stt == ReelManager.MSTATE.OPENING)
				{
					this.Ui.first_reelopen = true;
				}
				M2DBase.Instance.addValotAddition(this.Ui);
			}
			this.Ui.changeState(stt, false);
			return this.Ui;
		}

		public ReelManager assignCurrentItemReel(ReelManager.ItemReelContainer IR, bool initui = true)
		{
			this.AStackIR.Add(IR);
			if (this.Ui != null && initui && !this.Ui.isRotatingState())
			{
				this.initUiState(ReelManager.MSTATE.OPENING, this.Ui.transform.parent, false);
			}
			return this;
		}

		public ReelManager assignCurrentItemReel(List<NelItemEntry> AItm, bool initui = true, bool touch_obtain = false)
		{
			int count = AItm.Count;
			for (int i = 0; i < count; i++)
			{
				ReelManager.ItemReelContainer ir = ReelManager.GetIR(AItm[i].Data);
				if (ir != null)
				{
					if (touch_obtain)
					{
						ir.touchObtainCountAll();
					}
					int count2 = AItm[i].count;
					for (int j = 0; j < count2; j++)
					{
						this.AStackIR.Add(ir);
					}
				}
			}
			if (this.Ui != null && initui && !this.Ui.isRotatingState())
			{
				this.initUiState(ReelManager.MSTATE.OPENING, this.Ui.transform.parent, false);
			}
			return this;
		}

		public ReelManager assignCurrentItemReel(List<ReelManager.ItemReelContainer> AIR, bool initui = true, bool touch_obtain = false)
		{
			int count = AIR.Count;
			for (int i = 0; i < count; i++)
			{
				ReelManager.ItemReelContainer itemReelContainer = AIR[i];
				if (touch_obtain)
				{
					itemReelContainer.touchObtainCountAll();
				}
				this.AStackIR.Add(itemReelContainer);
			}
			if (this.Ui != null && initui && !this.Ui.isRotatingState())
			{
				this.initUiState(ReelManager.MSTATE.OPENING, this.Ui.transform.parent, false);
			}
			return this;
		}

		public ReelManager assignCurrentItemReel(string key, bool initui = true)
		{
			ReelManager.ItemReelContainer ir = ReelManager.GetIR(key, false, false);
			if (ir == null)
			{
				X.de("assignCurrentItemReel:: アイテムリール取得失敗:" + key, null);
				return this;
			}
			return this.assignCurrentItemReel(ir, initui);
		}

		public void clearItemReelCache()
		{
			this.AStackIR.Clear();
		}

		public bool hasItemReelCache()
		{
			return this.AStackIR.Count > 0;
		}

		public bool EvtWait(bool is_first = false)
		{
			if (this.Ui != null && this.Ui.isActive())
			{
				return true;
			}
			this.digestObtainedMoney();
			this.digestObtainedItem(true);
			return false;
		}

		public static ReelManager.ItemReelContainer GetIR(string key, bool no_error = false, bool calc_other_reel_class = false)
		{
			ReelManager.initReelScript();
			ReelManager.ItemReelContainer itemReelContainer = X.Get<string, ReelManager.ItemReelContainer>(ReelManager.OItemR, key);
			if (itemReelContainer == null)
			{
				if (calc_other_reel_class)
				{
					itemReelContainer = GuildManager.GetIR(key);
				}
				if (itemReelContainer == null && !no_error)
				{
					X.de("不明なItemReelContainer: " + key, null);
				}
			}
			return itemReelContainer;
		}

		public static ReelManager.ItemReelContainer GetIR(NelItem Itm)
		{
			return X.Get<string, ReelManager.ItemReelContainer>(ReelManager.OItemR, TX.slice(Itm.key, "itemreelC_".Length));
		}

		public static ReelManager.ItemReelContainer GetIR(NelItem Itm, bool no_error, bool calc_other_reel_class = false)
		{
			return ReelManager.GetIR(TX.slice(Itm.key, "itemreelC_".Length), no_error, calc_other_reel_class);
		}

		public static ReelManager.ItemReelColor GetNearColor(NelItem Itm)
		{
			foreach (KeyValuePair<string, ReelManager.ItemReelContainer> keyValuePair in ReelManager.OItemR)
			{
				if (keyValuePair.Value.isin(Itm) >= 0)
				{
					return keyValuePair.Value.ColSet;
				}
			}
			return null;
		}

		public static ReelManager.ItemReelContainer ReplaceToLowerGrade(ReelManager.ItemReelContainer Src)
		{
			if (ReelManager.AIRBuf.Count == 0 || ReelManager.AIRBuf[0] != Src)
			{
				ReelManager.AIRBuf.Clear();
				ReelManager.AIRBuf.Add(Src);
				foreach (KeyValuePair<string, ReelManager.ItemReelContainer> keyValuePair in ReelManager.OItemR)
				{
					if (keyValuePair.Value != Src && keyValuePair.Value.ColSet == Src.ColSet && keyValuePair.Value.rarelity < Src.rarelity && keyValuePair.Value.useableItem && (!Src.is_rare || !keyValuePair.Value.is_rare))
					{
						ReelManager.AIRBuf.Add(keyValuePair.Value);
					}
				}
			}
			if (ReelManager.AIRBuf.Count == 1)
			{
				return ReelManager.AIRBuf[0];
			}
			return ReelManager.AIRBuf[X.xors(ReelManager.AIRBuf.Count - 1) + 1];
		}

		public ReelManager.ItemReelContainer getCurrentItemReel()
		{
			if (this.AStackIR.Count <= 0)
			{
				return null;
			}
			return this.AStackIR[0];
		}

		public ReelManager.ItemReelContainer progressReelStack()
		{
			if (this.AStackIR.Count <= 0)
			{
				return null;
			}
			this.AStackIR.RemoveAt(0);
			if (this.AStackIR.Count <= 0)
			{
				return null;
			}
			return this.AStackIR[0];
		}

		public void fineUiShiftX(float ui_shift_x)
		{
			if (this.Ui != null && !this.Ui.isUiGMActive())
			{
				this.Ui.basex_u = ui_shift_x * 0.015625f;
				this.Ui.SetXy();
			}
		}

		public ReelManager digestObtainedMoney()
		{
			if (this.Ui != null && this.Ui.added_money > 0)
			{
				CoinStorage.addCount(this.Ui.added_money, CoinStorage.CTYPE.GOLD, true);
				this.Ui.added_money = 0;
			}
			return this;
		}

		public ReelManager digestObtainedItem(bool show_ui_log = true)
		{
			if (this.Ui != null && this.Ui.StDecided != null)
			{
				ItemStorage stDecided = this.Ui.StDecided;
				this.Ui.StDecided = null;
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				bool flag = nelM2DBase.canAccesableToHouseInventory();
				ItemStorage itemStorage = (flag ? nelM2DBase.IMNG.getHouseInventory() : nelM2DBase.IMNG.getInventory());
				if (nelM2DBase.IMNG.combineToInventory(stDecided, itemStorage, !flag && show_ui_log, null, true))
				{
					if (flag)
					{
						nelM2DBase.IMNG.getHouseInventory().fineRows(false);
					}
					if (flag)
					{
						nelM2DBase.reel_content_send_to_house_inventory = true;
					}
				}
			}
			return this;
		}

		public ReelManager digestItemEntry(NelItemEntry[] APublishedEntry)
		{
			if (APublishedEntry != null)
			{
				this.digestObtainedMoney();
				int num = (int)COOK.CurAchive.Get(ACHIVE.MENT.treasure_max_obtain);
				int num2 = (int)COOK.CurAchive.Get(ACHIVE.MENT.treasure_total_obtain);
				for (int i = APublishedEntry.Length - 1; i >= 0; i--)
				{
					NelItemEntry nelItemEntry = APublishedEntry[i];
					num = X.Mx(num, nelItemEntry.count);
					num2 += nelItemEntry.count;
				}
				COOK.CurAchive.Set(ACHIVE.MENT.treasure_max_obtain, num);
				COOK.CurAchive.Set(ACHIVE.MENT.treasure_total_obtain, num2);
				if (this.DropLp != null)
				{
					if (this.DropLp is M2LpItemSupplier)
					{
						(this.DropLp as M2LpItemSupplier).activate(APublishedEntry, true, true);
					}
					else
					{
						NelM2DBase nelM2DBase = this.DropLp.Mp.M2D as NelM2DBase;
						for (int j = APublishedEntry.Length - 1; j >= 0; j--)
						{
							NelItemEntry nelItemEntry2 = APublishedEntry[j];
							nelM2DBase.IMNG.dropManual(nelItemEntry2.Data, nelItemEntry2.count, (int)nelItemEntry2.grade, this.DropLp.mapfocx, this.DropLp.mapfocy, 0f, 0f, null, false, NelItemManager.TYPE.ABSORB);
						}
					}
				}
				else
				{
					bool flag = false;
					NelM2DBase nelM2DBase2 = M2DBase.Instance as NelM2DBase;
					if (nelM2DBase2 != null)
					{
						bool flag2 = nelM2DBase2.canAccesableToHouseInventory();
						for (int k = APublishedEntry.Length - 1; k >= 0; k--)
						{
							NelItemEntry nelItemEntry3 = APublishedEntry[k];
							int num3 = (flag2 ? nelM2DBase2.IMNG.getHouseInventory() : nelM2DBase2.IMNG.getInventory()).Add(nelItemEntry3.Data, nelItemEntry3.count, (int)nelItemEntry3.grade, true, true);
							if (num3 > 0)
							{
								nelM2DBase2.IMNG.addObtainCount(nelItemEntry3.Data, num3, (int)nelItemEntry3.grade);
								if (!flag2)
								{
									UILog.Instance.AddGetItem(nelM2DBase2.IMNG, nelItemEntry3.Data, num3, (int)nelItemEntry3.grade);
								}
								flag = true;
							}
						}
						if (flag && flag2)
						{
							nelM2DBase2.reel_content_send_to_house_inventory = true;
						}
					}
				}
			}
			return this;
		}

		public List<ReelExecuter> getReelVector()
		{
			return this.AEf;
		}

		public List<ReelManager.ItemReelContainer> getItemReelCacheVector()
		{
			return this.AStackIR;
		}

		public bool isUiActive()
		{
			return this.Ui != null && this.Ui.isActive();
		}

		public static ReelManager.ReelDecription listupItemSupplier(NelItem Itm, bool only_useableItem = false)
		{
			ReelManager.ReelDecription reelDecription = default(ReelManager.ReelDecription);
			foreach (KeyValuePair<string, ReelManager.ItemReelContainer> keyValuePair in ReelManager.OItemR)
			{
				if ((!only_useableItem || keyValuePair.Value.useableItem) && keyValuePair.Value.isin(Itm) >= 0)
				{
					reelDecription.AddReel(keyValuePair.Value);
				}
			}
			return reelDecription;
		}

		public static void createReelListDsn(Designer Ds, ReelManager.ItemReelContainer[] AReel, float _size, Color32 _TxCol, string splitter = ", ")
		{
			int num = AReel.Length;
			for (int i = 0; i < num; i++)
			{
				ReelManager.ItemReelContainer itemReelContainer = AReel[i];
				FillImageBlock fillImageBlock = Ds.addImg(new DsnDataImg
				{
					swidth = 22f,
					sheight = Ds.use_h,
					name = "mbox_" + i.ToString(),
					FnDraw = new MeshDrawer.FnGeneralDraw(itemReelContainer.fnGeneralDraw)
				});
				Ds.addP(new DsnDataP("", false)
				{
					size = _size,
					TxCol = _TxCol,
					alignx = ALIGN.LEFT,
					aligny = ALIGNY.BOTTOM,
					sheight = Ds.use_h,
					text = TX.ReplaceTX(itemReelContainer.tx_key, false) + ((i < num - 1) ? splitter : "")
				}, false);
				MTR.DrReelBox.setFillImageBlock(fillImageBlock, new MeshDrawer.FnGeneralDraw(itemReelContainer.fnGeneralDraw));
				fillImageBlock.getMeshDrawer().activation_key = itemReelContainer.key;
			}
		}

		private UiReelManager Ui;

		private static DateTime LoadDate;

		public static NIDic<int, ReelManager.EFReel> OAreel_content;

		private static NDic<ReelManager.ItemReelContainer> OItemR;

		private static NDic<ReelManager.ItemReelColor> OColor;

		private readonly List<ReelExecuter> AEf;

		private readonly List<string> Aef_stack;

		public const int stencil_ref = 225;

		public M2LabelPoint DropLp;

		private readonly List<ReelManager.ItemReelContainer> AStackIR;

		private static readonly Regex RegComma = new Regex("^\\.+$");

		public const int MAX_OBTAIN = 8;

		private static List<ReelManager.ItemReelContainer> AIRBuf;

		public delegate bool FnItemReelProgressing(ReelManager.ItemReelContainer PreIR);

		public sealed class ItemReelContainer
		{
			public bool is_rare
			{
				get
				{
					return this.rarelity >= 100;
				}
			}

			public ItemReelContainer(string _key, ReelManager.ItemReelColor _ColSet, string _tx_key = null)
			{
				this.key = _key;
				this.item_gkey = "itemreelG_" + this.key;
				this.item_ckey = "itemreelC_" + this.key;
				this.tx_key = _tx_key ?? ("&&_ItemReel_name_" + this.key);
				if (REG.match(this.key, REG.RegSuffixNumberOnly))
				{
					this.rarelity = (byte)X.NmI(REG.R1, 0, false, false);
				}
				this.AContent = new List<NelItemEntry>();
				this.ColSet = _ColSet;
			}

			public ReelManager.ItemReelContainer copyFrom(ReelManager.ItemReelContainer Src)
			{
				this.AContent.AddRange(Src.AContent);
				this.ColSet = Src.ColSet;
				return this;
			}

			public string getLocalizedName(M2LabelPoint LpSupplier)
			{
				string localizedName = this.GReelItem.getLocalizedName(0);
				if (TX.noe(localizedName))
				{
					NelM2DBase nelM2DBase = LpSupplier.Mp.M2D as NelM2DBase;
					string text = nelM2DBase.getMapTitle(LpSupplier.Mp);
					if (TX.noe(text))
					{
						WholeMapItem wholeFor = nelM2DBase.WM.GetWholeFor(LpSupplier.Mp, false);
						if (wholeFor != null)
						{
							text = wholeFor.localized_name;
						}
					}
					if (TX.noe(text))
					{
						text = "---";
					}
					return TX.GetA("Catalog_pickup_current_reel_spot", text);
				}
				return localizedName;
			}

			public void addGrade(int _add)
			{
				for (int i = this.AContent.Count - 1; i >= 0; i--)
				{
					this.AContent[i].grade = (byte)X.MMX(0, (int)this.AContent[i].grade + _add, 4);
				}
			}

			public void addCount(int _add)
			{
				for (int i = this.AContent.Count - 1; i >= 0; i--)
				{
					this.AContent[i].count = X.Mx(0, this.AContent[i].count + _add);
				}
			}

			public int Count
			{
				get
				{
					return this.AContent.Count;
				}
			}

			public NelItemEntry this[int i]
			{
				get
				{
					return this.AContent[i];
				}
			}

			public NelItem GReelItem
			{
				get
				{
					return NelItem.GetById(this.item_gkey, false);
				}
			}

			public NelItem CReelItem
			{
				get
				{
					return NelItem.GetById(this.item_ckey, false);
				}
			}

			public void AddRange(IEnumerable<NelItemEntry> collection)
			{
				this.AContent.AddRange(collection);
			}

			public void Add(NelItemEntry IE)
			{
				this.AContent.Add(IE);
			}

			public void shuffle(XorsMaker Xors)
			{
				X.shuffle<NelItemEntry>(this.AContent, -1, Xors);
			}

			public void touchObtainCountAll()
			{
				for (int i = this.Count - 1; i >= 0; i--)
				{
					this.AContent[i].Data.touchObtainCount();
				}
			}

			public NelItemEntry[] ToArray()
			{
				return this.AContent.ToArray();
			}

			public void listupItemsTo(STB Stb, string splitter, bool show_storage_count = false)
			{
				int count = this.AContent.Count;
				NelItemManager imng = (M2DBase.Instance as NelM2DBase).IMNG;
				for (int i = 0; i < count; i++)
				{
					if (i > 0)
					{
						Stb.Add(splitter);
					}
					this.getOneRowDetail(Stb, i, splitter, imng, show_storage_count);
				}
			}

			public STB getOneRowDetail(STB Stb, int i, string splitter, NelItemManager IMNG, bool show_storage_count = false)
			{
				if (i < 0 || i >= this.AContent.Count)
				{
					return Stb;
				}
				NelItemEntry nelItemEntry = this.AContent[i];
				nelItemEntry.getLocalizedName(Stb, 0, 2, false);
				if (show_storage_count)
				{
					Stb.Add(splitter).Add("\u3000 - <font size=\"14\">");
					IMNG.holdItemString(Stb, nelItemEntry.Data, -1, true);
					Stb.Add("</font>");
				}
				return Stb;
			}

			public bool fnGeneralDraw(MeshDrawer Md, float alpha)
			{
				return this.fnGeneralDraw(Md, alpha, Matrix4x4.Scale(new Vector3(0.32f, 0.32f, 0.32f)) * NelMBoxDrawer.BasicMx, true);
			}

			public bool fnGeneralDraw(MeshDrawer Md, float alpha, Matrix4x4 Mx, bool update_mesh_renderer)
			{
				NelMBoxDrawer drReelBox = MTR.DrReelBox;
				drReelBox.col_base_light = this.ColSet.top;
				drReelBox.col_base_dark = this.ColSet.bottom;
				drReelBox.col_pic_light = this.ColSet.icon;
				drReelBox.col_pic_dark = Md.ColGrd.Set(this.ColSet.icon).multiply(0.6f, 0.6f, 0.6f, 1f).rgba;
				drReelBox.col_inner_light = Md.ColGrd.Set(this.ColSet.top).multiply(0.2f, 0.3f, 0.4f, 1f).rgba;
				drReelBox.col_inner_dark = Md.ColGrd.Set(this.ColSet.bottom).multiply(0.2f, 0.3f, 0.4f, 1f).rgba;
				drReelBox.fnGeneralDraw(Md, alpha, Matrix4x4.Scale(new Vector3(0.32f, 0.32f, 0.32f)) * NelMBoxDrawer.BasicMx, update_mesh_renderer);
				return true;
			}

			public bool drawSmallIcon(MeshDrawer Md, float x, float y, float alpha, float scale, bool update_mesh_renderer = false)
			{
				PxlFrame pxlFrame = MTRX.getPF("mbox_base_s");
				PxlImage img = pxlFrame.getLayer(0).Img;
				Md.initForImg(img, 0);
				Md.Col = C32.MulA(this.ColSet.bottom, alpha);
				Md.ColGrd.Set(this.ColSet.top).mulA(alpha);
				Md.RectBLGradation(x - (float)img.width * 0.5f * scale, y - (float)img.height * 0.5f * scale, (float)img.width * scale, (float)img.height * scale, GRD.BOTTOM2TOP, false);
				pxlFrame = MTRX.getPF(this.is_rare ? "mbox_base_kira" : "mbox_base_i");
				PxlLayer layer = pxlFrame.getLayer(0);
				Md.Col = C32.MulA(this.ColSet.icon, alpha);
				Md.RotaPF(x + layer.x * scale, y + layer.y * scale, scale, scale, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
				if (update_mesh_renderer)
				{
					Md.updateForMeshRenderer(false);
				}
				return true;
			}

			public NelItem IndividualItem
			{
				get
				{
					int count = this.AContent.Count;
					if (count == 0)
					{
						return null;
					}
					NelItem data = this.AContent[0].Data;
					for (int i = 1; i < count; i++)
					{
						if (this.AContent[i].Data != data)
						{
							return null;
						}
					}
					return data;
				}
			}

			public int isin(NelItem Itm)
			{
				int count = this.AContent.Count;
				for (int i = 0; i < count; i++)
				{
					if (this.AContent[i].Data == Itm)
					{
						return i;
					}
				}
				return -1;
			}

			public readonly string key;

			public readonly string item_gkey;

			public readonly string item_ckey;

			public readonly string tx_key;

			private readonly List<NelItemEntry> AContent;

			public ReelManager.ItemReelColor ColSet;

			public byte rarelity;

			public bool not_appear_fieldguide;

			public bool useableItem;
		}

		public sealed class ItemReelDrop
		{
			public ItemReelDrop(ReelManager.ItemReelContainer _IR, int _grade_add = -1)
			{
				this.IR = _IR;
				if (_grade_add < 0)
				{
					this.grade_add = X.xors() % 2U > 0U;
					return;
				}
				this.grade_add = _grade_add != 0;
			}

			public void fineQuestTargetItem(QuestTracker QUEST)
			{
				this.quest_target_item = false;
				int count = this.IR.Count;
				for (int i = 0; i < count; i++)
				{
					if (QUEST.isQuestTargetItem(this.IR[i].Data, 0))
					{
						this.quest_target_item = true;
						return;
					}
				}
			}

			public string key
			{
				get
				{
					return this.IR.key;
				}
			}

			public ReelManager.ItemReelColor ColSet
			{
				get
				{
					return this.IR.ColSet;
				}
			}

			public readonly ReelManager.ItemReelContainer IR;

			public bool grade_add;

			public int grade;

			public bool quest_target_item;
		}

		public sealed class ItemReelColor
		{
			public ItemReelColor(uint _Top, uint _Bottom, uint _Icon)
			{
				this.top = _Top;
				this.bottom = _Bottom;
				this.icon = _Icon;
			}

			public uint top;

			public uint bottom;

			public uint icon;
		}

		public struct ReelDecription
		{
			public void AddReel(ReelManager.ItemReelContainer IR)
			{
				if (IR.useableItem)
				{
					if (this.AIR_useable == null)
					{
						this.AIR_useable = new List<ReelManager.ItemReelContainer>(4);
					}
					this.AIR_useable.Add(IR);
					return;
				}
				if (this.AIR_supplier == null)
				{
					this.AIR_supplier = new List<ReelManager.ItemReelContainer>(4);
				}
				this.AIR_supplier.Add(IR);
			}

			public List<ReelManager.ItemReelContainer> AIR_useable;

			public List<ReelManager.ItemReelContainer> AIR_supplier;
		}

		public class EFReel
		{
			public EFReel(List<string> As, int _rarelity = 0)
			{
				this.Aeffect = As.ToArray();
				this.rarelity = _rarelity;
			}

			public int rarelity;

			public readonly string[] Aeffect;
		}

		public enum MSTATE
		{
			NONE,
			OBTAIN,
			DETAIL_REEL,
			DETAIL_RECIPEBOOK,
			NIGHTCON_ADDING,
			REMOVE_REELS,
			PREPARE,
			OPENING,
			OPENING_AUTO
		}
	}
}
