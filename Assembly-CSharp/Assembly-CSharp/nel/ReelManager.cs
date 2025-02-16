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
			this.AReel = new List<ReelExecuter>(4);
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
			for (int i = this.AReel.Count - 1; i >= 0; i--)
			{
				this.AReel[i].destruct();
			}
			if (clear_array_content)
			{
				this.AReel.Clear();
			}
		}

		public void newGame()
		{
			this.clearReels(false, true);
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
			ReelManager.OAreel_content = new NIDic<int, string[]>("reel_content");
			List<string> list = new List<string>(16);
			XorsMaker xorsMaker = new XorsMaker(4085363881U, false);
			int num = -1;
			while (csvReaderA.read())
			{
				ReelExecuter.EFFECT effect;
				if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					if (num != -1)
					{
						ReelManager.OAreel_content[num] = list.ToArray();
					}
					list.Clear();
					string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					ReelExecuter.ETYPE etype;
					if (!FEnum<ReelExecuter.ETYPE>.TryParse(index, out etype, true))
					{
						csvReaderA.tError("不明なETYPE: " + index);
					}
					num = (int)etype;
				}
				else if (FEnum<ReelExecuter.EFFECT>.TryParse(csvReaderA.last_input, out effect, true))
				{
					list.Add(effect.ToString().ToUpper());
				}
			}
			if (num != -1)
			{
				ReelManager.OAreel_content[num] = list.ToArray();
			}
			ReelManager.OItemR = new NDic<ReelManager.ItemReelContainer>("Reel_ItemR", 12);
			ReelManager.OColor = new NDic<ReelManager.ItemReelColor>("Reel_Color", 16);
			ReelManager.AIRBuf = new List<ReelManager.ItemReelContainer>(3);
			ReelManager.ItemReelColor itemReelColor = (ReelManager.OColor["_"] = new ReelManager.ItemReelColor(4294440951U, 4287926159U, 4283391212U));
			List<ReelManager.ItemReelContainer> list2 = new List<ReelManager.ItemReelContainer>(8);
			List<NelItemEntry> list3 = new List<NelItemEntry>(8);
			csvReaderA.parseText(TX.getResource("Data/itemreel", ".csv", false));
			int num2 = 60000;
			while (csvReaderA.read())
			{
				if (TX.isStart(csvReaderA.cmd, '#'))
				{
					if (csvReaderA.cmd == "#COLOR")
					{
						ReelManager.ItemReelColor itemReelColor2 = (ReelManager.OColor[csvReaderA._1] = new ReelManager.ItemReelColor(global::XX.X.NmUI(csvReaderA._2, 4294440951U, true, true), global::XX.X.NmUI(csvReaderA._3, 4287926159U, true, true), global::XX.X.NmUI(csvReaderA._4, 4283391212U, true, true)));
						if (csvReaderA._1 == "demon")
						{
						}
					}
				}
				else if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					string[] array = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1).Split(new char[] { '|' });
					list2.Clear();
					for (int i = array.Length - 1; i >= 0; i--)
					{
						string text = array[i];
						if (!TX.noe(text))
						{
							ReelManager.ItemReelContainer itemReelContainer = global::XX.X.Get<string, ReelManager.ItemReelContainer>(ReelManager.OItemR, text);
							if (itemReelContainer != null)
							{
								list2.Add(itemReelContainer);
							}
							else
							{
								list2.Add(ReelManager.OItemR[text] = new ReelManager.ItemReelContainer(text, itemReelColor));
								for (int j = 0; j < 2; j++)
								{
									string text2 = ((j == 0) ? "itemreelC_" : "itemreelG_") + text;
									NelItem.CreateItemEntry(text2, new NelItem(text2, 0, 5, 1)
									{
										category = (NelItem.CATEG)2097281U,
										FnGetName = new FnGetItemDetail(NelItem.fnGetNameItemReel),
										FnGetDesc = new FnGetItemDetail(NelItem.fnGetDescItemReel),
										FnGetDetail = new FnGetItemDetail(NelItem.fnGetDetailItemReel),
										FnGetColor = new FnGetItemColor(NelItem.fnGetColorItemReel),
										stock = 1
									}, num2++, true);
								}
							}
						}
					}
				}
				else if (TX.isStart(csvReaderA.cmd, '%'))
				{
					if (csvReaderA.cmd == "%COLOR")
					{
						ReelManager.ItemReelColor itemReelColor3 = global::XX.X.Get<string, ReelManager.ItemReelColor>(ReelManager.OColor, csvReaderA._1);
						if (itemReelColor3 == null)
						{
							csvReaderA.tError("不明なカラーセット: " + csvReaderA._1);
						}
						else
						{
							for (int k = list2.Count - 1; k >= 0; k--)
							{
								list2[k].ColSet = itemReelColor3;
							}
						}
					}
					else if (csvReaderA.cmd == "%RARE")
					{
						for (int l = list2.Count - 1; l >= 0; l--)
						{
							list2[l].rarelity = (byte)csvReaderA.Int(1, 0);
						}
					}
					else if (csvReaderA.cmd == "%SHUFFLE")
					{
						for (int m = list2.Count - 1; m >= 0; m--)
						{
							list2[m].shuffle(xorsMaker);
						}
					}
					else if (csvReaderA.cmd == "%CLONE")
					{
						ReelManager.ItemReelContainer itemReelContainer2 = global::XX.X.Get<string, ReelManager.ItemReelContainer>(ReelManager.OItemR, csvReaderA._1);
						if (itemReelContainer2 == null)
						{
							csvReaderA.tError("不明なSrc IR: " + csvReaderA._1);
						}
						else
						{
							for (int n = list2.Count - 1; n >= 0; n--)
							{
								list2[n].copyFrom(itemReelContainer2);
							}
						}
					}
					else if (csvReaderA.cmd == "%GRADE")
					{
						for (int num3 = list2.Count - 1; num3 >= 0; num3--)
						{
							list2[num3].addGrade(csvReaderA.Int(1, 0));
						}
					}
					else if (csvReaderA.cmd == "%COUNT")
					{
						for (int num4 = list2.Count - 1; num4 >= 0; num4--)
						{
							list2[num4].addCount(csvReaderA.Int(1, 0));
						}
					}
				}
				else
				{
					NelItem byId = NelItem.GetById(csvReaderA.cmd, false);
					if (byId != null)
					{
						list3.Clear();
						int num5 = csvReaderA.clength - 1;
						for (int num6 = 0; num6 < num5; num6++)
						{
							string index2 = csvReaderA.getIndex(num6 + 1);
							int num7 = -1000;
							byte b;
							if (REG.match(index2, ReelManager.RegComma))
							{
								NelItemEntry nelItemEntry = list3[num6 - index2.Length];
								num7 = nelItemEntry.count;
								b = nelItemEntry.grade;
							}
							else
							{
								int num8 = index2.IndexOf("g");
								if (num8 >= 0)
								{
									string text3 = TX.slice(index2, 0, num8);
									if (REG.match(text3, ReelManager.RegComma))
									{
										num7 = list3[num6 - text3.Length].count;
									}
									else
									{
										num7 = global::XX.X.NmI(text3, num7, false, false);
									}
									text3 = TX.slice(index2, num8 + 1);
									if (REG.match(text3, ReelManager.RegComma))
									{
										b = list3[num6 - text3.Length].grade;
									}
									else
									{
										b = (byte)global::XX.X.NmI(text3, 0, false, false);
									}
								}
								else
								{
									num7 = global::XX.X.NmI(index2, num7, false, false);
									if (num6 == 0)
									{
										ReelManager.ItemReelContainer itemReelContainer3 = list2[list2.Count - 1];
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
										b = list3[num6 - 1].grade;
									}
								}
							}
							if (num7 == -1000)
							{
								global::XX.X.dl("不明な書式: " + index2, null, false, false);
							}
							else
							{
								if (num7 < 0)
								{
									num7 = -num7 * byId.stock;
								}
								list3.Add(new NelItemEntry(byId, num7, b));
							}
						}
						for (int num9 = list2.Count - 1; num9 >= 0; num9--)
						{
							list2[num9].AddRange(list3);
						}
					}
				}
			}
			ReelManager.OItemR.scriptFinalize();
			ReelManager.OColor.scriptFinalize();
			ReelManager.OAreel_content.scriptFinalize();
		}

		public void obtain(ReelExecuter.ETYPE type)
		{
			ReelExecuter reelExecuter = new ReelExecuter(this, type);
			this.AReel.Add(reelExecuter);
			if (this.Ui != null)
			{
				this.Ui.obtain(reelExecuter);
			}
		}

		public void obtainReels(ReelManager Src)
		{
			this.AReel.AddRange(Src.AReel);
		}

		public void clearReels(bool only_executer = false, bool remove_gob = true)
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
			this.Aobtainable_reel = null;
			this.flushObtainableReel();
		}

		public void flushObtainableReel()
		{
			this.Aobtainable_reel = new List<string>(7);
			List<string> list = new List<string>(6);
			list.Add("GRADE1");
			list.Add("COUNT_ADD1");
			if (global::XX.X.XORSP() < 0.5f)
			{
				list.Add((global::XX.X.XORSP() < 0.5f) ? "GRADE2" : "COUNT_ADD2");
			}
			else
			{
				list.Add((global::XX.X.XORSP() < 0.5f) ? "ADD_MONEY" : "COUNT_ADD1");
			}
			global::XX.X.shuffle<string>(list, -1, null);
			this.Aobtainable_reel.AddRange(list);
			this.Aobtainable_reel.Add("COUNT_MUL1");
			this.addObtainableReelAfter(list, 0);
		}

		public void addObtainableReelAfter(List<string> Buf = null, int superiour = 0)
		{
			Buf = Buf ?? new List<string>(6);
			Buf.Clear();
			if (superiour >= 1)
			{
				Buf.Add((global::XX.X.XORSP() < 0.5f) ? "RANDOM" : "ADD_MONEY");
			}
			else
			{
				Buf.Add("RANDOM");
				Buf.Add("ADD_MONEY");
				Buf.Add((global::XX.X.XORSP() < 0.3f) ? "GRADE1" : "COUNT_ADD1");
			}
			if (global::XX.X.XORSP() < 0.8f)
			{
				Buf.Add((global::XX.X.XORSP() < 0.3f) ? "GRADE2" : "COUNT_ADD2");
			}
			else
			{
				Buf.Add((global::XX.X.XORSP() < 0.3f) ? ((superiour >= 2) ? "RANDOM" : "GRADE1") : "COUNT_ADD1");
			}
			if (superiour >= 1)
			{
				Buf.Add((global::XX.X.XORSP() < 0.4f) ? "GRADE3" : "COUNT_ADD3");
				if (superiour >= 2)
				{
					Buf.Add((global::XX.X.XORSP() < 0.3f) ? "GRADE3" : "COUNT_ADD3");
					Buf.Add((global::XX.X.XORSP() < 0.3f) ? "COUNT_MUL1" : "COUNT_ADD3");
				}
			}
			global::XX.X.shuffle<string>(Buf, -1, null);
			for (int i = 0; i < 3; i++)
			{
				this.Aobtainable_reel.Add(Buf[i]);
			}
		}

		public void obtainProgress(int reel_obtained)
		{
			if (this.Aobtainable_reel == null)
			{
				this.flushObtainableReel();
			}
			if (this.Aobtainable_reel.Count == 0)
			{
				this.addObtainableReelAfter(null, (reel_obtained >= 15) ? 2 : ((reel_obtained >= 7) ? 1 : 0));
			}
			string text = this.Aobtainable_reel[0];
			this.Aobtainable_reel.RemoveAt(0);
			ReelExecuter.ETYPE etype;
			if (FEnum<ReelExecuter.ETYPE>.TryParse(text, out etype, true))
			{
				this.obtain(etype);
				return;
			}
			global::XX.X.de("不明なETYPE:" + text, null);
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			int num = this.AReel.Count;
			Ba.writeUShort((ushort)num);
			for (int i = 0; i < num; i++)
			{
				Ba.writeByte((int)this.AReel[i].getEType());
			}
			num = ((this.Aobtainable_reel != null) ? this.Aobtainable_reel.Count : 0);
			Ba.writeUShort((ushort)num);
			for (int j = 0; j < num; j++)
			{
				ReelExecuter.ETYPE etype;
				if (FEnum<ReelExecuter.ETYPE>.TryParse(this.Aobtainable_reel[j], out etype, true))
				{
					Ba.writeByte((int)etype);
				}
				else
				{
					Ba.writeByte(0);
				}
			}
		}

		public void readBinaryFrom(ByteArray Ba, int vers)
		{
			this.destructGob();
			int num = (int)Ba.readUShort();
			this.AReel.Clear();
			if (this.Aobtainable_reel == null)
			{
				this.Aobtainable_reel = new List<string>(7);
			}
			this.Aobtainable_reel.Clear();
			for (int i = 0; i < num; i++)
			{
				ReelExecuter.ETYPE etype = (ReelExecuter.ETYPE)Ba.readByte();
				if (global::XX.X.BTW(1f, (float)etype, 11f))
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
					if (global::XX.X.BTW(1f, (float)etype2, 11f))
					{
						this.Aobtainable_reel.Add(etype2.ToString());
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

		public ReelManager assignCurrentItemReel(string key, bool initui = true)
		{
			ReelManager.ItemReelContainer ir = ReelManager.GetIR(key, false);
			if (ir == null)
			{
				global::XX.X.de("assignCurrentItemReel:: アイテムリール取得失敗:" + key, null);
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

		public static ReelManager.ItemReelContainer GetIR(string key, bool no_error = false)
		{
			ReelManager.initReelScript();
			ReelManager.ItemReelContainer itemReelContainer = global::XX.X.Get<string, ReelManager.ItemReelContainer>(ReelManager.OItemR, key);
			if (itemReelContainer == null && !no_error)
			{
				global::XX.X.de("不明なItemReelContainer: " + key, null);
			}
			return itemReelContainer;
		}

		public static ReelManager.ItemReelContainer GetIR(NelItem Itm)
		{
			return global::XX.X.Get<string, ReelManager.ItemReelContainer>(ReelManager.OItemR, TX.slice(Itm.key, "itemreelC_".Length));
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
			return ReelManager.AIRBuf[global::XX.X.xors(ReelManager.AIRBuf.Count - 1) + 1];
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
				if (this.DropLp != null)
				{
					if (this.DropLp is M2LpItemSupplier)
					{
						(this.DropLp as M2LpItemSupplier).activate(APublishedEntry, true, true);
					}
					else
					{
						NelM2DBase nelM2DBase = this.DropLp.Mp.M2D as NelM2DBase;
						for (int i = APublishedEntry.Length - 1; i >= 0; i--)
						{
							NelItemEntry nelItemEntry = APublishedEntry[i];
							nelM2DBase.IMNG.dropManual(nelItemEntry.Data, nelItemEntry.count, (int)nelItemEntry.grade, this.DropLp.mapfocx, this.DropLp.mapfocy, 0f, 0f, null, false, NelItemManager.TYPE.ABSORB);
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
						for (int j = APublishedEntry.Length - 1; j >= 0; j--)
						{
							NelItemEntry nelItemEntry2 = APublishedEntry[j];
							int num = (flag2 ? nelM2DBase2.IMNG.getHouseInventory() : nelM2DBase2.IMNG.getInventory()).Add(nelItemEntry2.Data, nelItemEntry2.count, (int)nelItemEntry2.grade, true, true);
							if (num > 0)
							{
								nelItemEntry2.Data.addObtainCount(num);
								if (!flag2)
								{
									UILog.Instance.AddGetItem(nelM2DBase2.IMNG, nelItemEntry2.Data, num, (int)nelItemEntry2.grade);
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
			return this.AReel;
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
					text = TX.Get("_ItemReel_name_" + itemReelContainer.key, "") + ((i < num - 1) ? splitter : "")
				}, false);
				MTR.DrReelBox.setFillImageBlock(fillImageBlock, new MeshDrawer.FnGeneralDraw(itemReelContainer.fnGeneralDraw));
				fillImageBlock.getMeshDrawer().activation_key = itemReelContainer.key;
			}
		}

		private readonly List<ReelExecuter> AReel;

		private UiReelManager Ui;

		private static DateTime LoadDate;

		public static NIDic<int, string[]> OAreel_content;

		private static NDic<ReelManager.ItemReelContainer> OItemR;

		private static NDic<ReelManager.ItemReelColor> OColor;

		private List<string> Aobtainable_reel;

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

			public ItemReelContainer(string _key, ReelManager.ItemReelColor _ColSet)
			{
				this.key = _key;
				this.tx_key = "_ItemReel_name_" + this.key;
				if (REG.match(this.key, REG.RegSuffixNumberOnly))
				{
					this.rarelity = (byte)global::XX.X.NmI(REG.R1, 0, false, false);
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

			public void addGrade(int _add)
			{
				for (int i = this.AContent.Count - 1; i >= 0; i--)
				{
					this.AContent[i].grade = (byte)global::XX.X.MMX(0, (int)this.AContent[i].grade + _add, 4);
				}
			}

			public void addCount(int _add)
			{
				for (int i = this.AContent.Count - 1; i >= 0; i--)
				{
					this.AContent[i].count = global::XX.X.Mx(0, this.AContent[i].count + _add);
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
					return NelItem.GetById("itemreelG_" + this.key, false);
				}
			}

			public NelItem CReelItem
			{
				get
				{
					return NelItem.GetById("itemreelC_" + this.key, false);
				}
			}

			public void AddRange(IEnumerable<NelItemEntry> collection)
			{
				this.AContent.AddRange(collection);
			}

			public void shuffle(XorsMaker Xors)
			{
				global::XX.X.shuffle<NelItemEntry>(this.AContent, -1, Xors);
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

			public string listupItems(string splitter, bool show_storage_count = false)
			{
				int count = this.AContent.Count;
				string text;
				using (STB stb = TX.PopBld(null, 0))
				{
					NelItemManager imng = (M2DBase.Instance as NelM2DBase).IMNG;
					for (int i = 0; i < count; i++)
					{
						if (i > 0)
						{
							stb.Add(splitter);
						}
						this.getOneRowDetail(stb, i, splitter, imng, show_storage_count);
					}
					text = stb.ToString();
				}
				return text;
			}

			public STB getOneRowDetail(STB Stb, int i, string splitter, NelItemManager IMNG, bool show_storage_count = false)
			{
				if (i < 0 || i >= this.AContent.Count)
				{
					return Stb;
				}
				NelItemEntry nelItemEntry = this.AContent[i];
				Stb.Add(nelItemEntry.getLocalizedName(0, 2, false));
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

			public bool useableItem
			{
				get
				{
					return TX.getTX(this.tx_key, true, true, TX.getDefaultFamily()) != null;
				}
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

			public readonly string tx_key;

			private readonly List<NelItemEntry> AContent;

			public ReelManager.ItemReelColor ColSet;

			public byte rarelity;
		}

		public sealed class ItemReelDrop
		{
			public ItemReelDrop(ReelManager.ItemReelContainer _IR, int _grade_add = -1)
			{
				this.IR = _IR;
				if (_grade_add < 0)
				{
					this.grade_add = global::XX.X.xors() % 2U > 0U;
					return;
				}
				this.grade_add = _grade_add != 0;
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
