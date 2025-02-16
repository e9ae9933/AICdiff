using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class GuildManager : XorsMaker
	{
		public GuildManager(NelM2DBase _M2D)
			: base(0U, true)
		{
			this.M2D = _M2D;
			this.InvForCollectNew = new ItemStorage("_InvForCollectNew", 99)
			{
				water_stockable = true,
				infinit_stockable = true
			};
		}

		public void initGQCategory()
		{
			if (this.ACateg != null)
			{
				return;
			}
			this.ACateg = new GQCategInfo[7];
			this.ACateg[0] = new GQCategInfoCollectItem(this, GuildManager.CATEG.COLLECTNEW, 66, true)
			{
				comment_max = 6
			};
			this.ACateg[1] = new GQCategInfoCollectItem(this, GuildManager.CATEG.COLLECT, 67, false)
			{
				comment_max = 25
			};
			this.ACateg[2] = new GQCategInfoDefeat(this, GuildManager.CATEG.DEFEAT, 68)
			{
				comment_max = 5,
				reward_gp_min = 2,
				reward_gp_max = 12,
				reward_money_min = 100,
				reward_money_max = 300
			};
			this.ACateg[3] = new GQCategInfo(this, GuildManager.CATEG.COOK, 7)
			{
				comment_max = 7,
				reward_gp_min = 2,
				reward_gp_max = 10
			};
			this.ACateg[4] = new GQCategInfo(this, GuildManager.CATEG.RESCUE, 69)
			{
				comment_max = 4,
				reward_gp_min = 3,
				reward_gp_max = 10,
				reward_money_min = 100,
				reward_money_max = 250,
				lost_gp_ratio = 2f
			};
			this.ACateg[5] = new GQCategInfo(this, GuildManager.CATEG.WEAPON, 70)
			{
				comment_max = 1
			};
			this.ACateg[6] = new GQCategInfo(this, GuildManager.CATEG.DELIVER, 71)
			{
				comment_max = 4
			};
		}

		private void reloadGuildScript(string enbl_type)
		{
			string text = "gq_" + enbl_type;
			this.initGQCategory();
			int num = this.ACateg.Length;
			for (int i = 0; i < num; i++)
			{
				this.ACateg[i].FnGRank2EntryCount = null;
			}
			bool flag = false;
			List<ReelManager.ItemReelContainer> list = null;
			GuildManager.EnblContainer enblContainer;
			if (!this.OEnbl.TryGetValue(enbl_type, out enblContainer))
			{
				enblContainer = (this.OEnbl[enbl_type] = new GuildManager.EnblContainer());
			}
			if (enblContainer.AIRreward == null)
			{
				flag = true;
				list = enblContainer.createRewardReelItemList();
			}
			CsvReader csvReader = new CsvReader(TX.getResource("Data/gq/" + text, ".csv", false), CsvReader.RegSpace, false);
			string text2 = "##";
			while (csvReader.read())
			{
				if (csvReader.cmd.IndexOf("##") == 0)
				{
					text2 = "##TREASURE";
				}
				else
				{
					if (text2 == "##")
					{
						GuildManager.CATEG categ;
						if (!Enum.TryParse<GuildManager.CATEG>(csvReader.cmd, out categ))
						{
							csvReader.tError("不明なGQ CATEG: " + csvReader.cmd);
							continue;
						}
						this.ACateg[(int)categ].FnGRank2EntryCount = new EfParticleFuncCalc(csvReader.slice_join(1, " ", ""), "ZLINE", 1f);
					}
					if (text2 == "##TREASURE" && flag)
					{
						NelItem byId = NelItem.GetById(csvReader.cmd, false);
						if (byId != null)
						{
							ReelManager.ItemReelColor itemReelColor = ReelManager.GetNearColor(byId) ?? GuildManager.IRColSetDefault;
							int num2 = csvReader.Int(1, 1);
							byte b = (byte)csvReader.Int(2, 0);
							ReelManager.ItemReelContainer itemReelContainer = new ReelManager.ItemReelContainer(string.Concat(new string[]
							{
								"guild_",
								enbl_type,
								":",
								byId.id.ToString(),
								"x",
								num2.ToString(),
								"@",
								b.ToString()
							}), itemReelColor, "&&_NelItem_name_" + byId.key + " x" + num2.ToString());
							for (int j = 0; j < 2; j++)
							{
								ReelManager.CreateReelItemEntry((j == 0) ? itemReelContainer.item_ckey : itemReelContainer.item_gkey, 65535);
							}
							itemReelContainer.Add(new NelItemEntry(byId, num2, b));
							itemReelContainer.Add(new NelItemEntry(byId, X.Mx(1, X.IntC((float)num2 * 0.6f)), (byte)X.Mn(4, (int)(b + 1))));
							itemReelContainer.Add(new NelItemEntry(byId, X.Mx(1, X.IntC((float)num2 * 0.3f)), (byte)X.Mn(4, (int)(b + 2))));
							itemReelContainer.Add(new NelItemEntry(byId, X.Mx(1, X.IntR((float)num2 * 0.77f)), (byte)X.Mn(4, (int)(b + 1))));
							list.Add(itemReelContainer);
						}
					}
				}
			}
		}

		public int grank_start_point(int grank)
		{
			int num;
			switch (grank)
			{
			case 0:
				num = 0;
				break;
			case 1:
				num = 5;
				break;
			case 2:
				num = 20;
				break;
			case 3:
				num = 50;
				break;
			case 4:
				num = 115;
				break;
			default:
				num = 115;
				break;
			}
			return num;
		}

		public int current_grank
		{
			get
			{
				return this.getRankFor((float)this.gq_point);
			}
		}

		public int getRankFor(float gq_point)
		{
			if (gq_point >= 115f)
			{
				return 4;
			}
			if (gq_point >= 50f)
			{
				return 3;
			}
			if (gq_point >= 20f)
			{
				return 2;
			}
			if (gq_point >= 5f)
			{
				return 1;
			}
			return 0;
		}

		private int get_entry_grank(int grank)
		{
			if (grank >= 4)
			{
				return base.map(10, 15, 20, 25, 20);
			}
			if (grank == 3)
			{
				return base.map(14, 20, 22, 28, 8);
			}
			if (grank == 2)
			{
				return base.map(20, 30, 40, 10, 0);
			}
			if (grank == 1)
			{
				return base.map(30, 50, 15, 0, 0);
			}
			return base.map(65, 35, 0, 0, 0);
		}

		public int getRecievableGQ(int grank)
		{
			return 2 + grank;
		}

		private int getMaxGQEntry(int grank)
		{
			return X.IntR(X.NIL(4f, 15f, (float)grank, 4f));
		}

		public static ReelManager.ItemReelContainer GetIR(string key)
		{
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase == null)
			{
				return null;
			}
			return nelM2DBase.GUILD.getIRByKey(key);
		}

		public static ReelManager.ItemReelContainer GetIR(NelItem Itm)
		{
			return GuildManager.GetIR(TX.slice(Itm.key, "itemreelC_".Length));
		}

		public void newGame()
		{
			this.gq_point = -1;
			this.OACurrentGQ.Clear();
			this.AGqCollectNew.Clear();
			this.InvForCollectNew.clearAllItems(99);
			base.init(false, 0U, 0U, 0U, 0U);
			this.current_item_id_seek = 62400;
			this.need_fine_flag_ = 1;
			this.danger_progress = 0;
			this.current_gq_enable_type = "";
			if (this.ACateg != null)
			{
				for (int i = this.ACateg.Length - 1; i >= 0; i--)
				{
					this.ACateg[i].newGame();
				}
			}
			foreach (KeyValuePair<string, GuildManager.EnblContainer> keyValuePair in this.OEnbl)
			{
				keyValuePair.Value.newGame();
			}
		}

		public void destruct()
		{
			foreach (KeyValuePair<string, GuildManager.EnblContainer> keyValuePair in this.OEnbl)
			{
				keyValuePair.Value.releaseRewardReelItem();
			}
		}

		public void progressDangerousness(int _obt)
		{
			if (this.danger_progress < 9)
			{
				this.danger_progress += (byte)_obt;
				if (this.danger_progress >= 9)
				{
					this.need_fine_flag_ |= 1;
				}
			}
		}

		public void checkFineGQ(string gq_enable_type)
		{
			if (this.gq_point < 0)
			{
				this.gq_point = 0;
			}
			if (this.current_gq_enable_type != gq_enable_type)
			{
				this.need_fine_flag_ |= 1;
				this.current_gq_enable_type = gq_enable_type;
			}
			if (this.need_fine_flag_ == 0)
			{
				return;
			}
			if ((this.need_fine_flag_ & 1) != 0 && TX.valid(this.current_gq_enable_type))
			{
				this.need_fine_flag_ -= 1;
				this.danger_progress = 0;
				GF.setC("GLD_RENZOKU", 0U);
				this.initGQCategory();
				if (this.current_gq_enable_type != this.gq_valid_type)
				{
					this.gq_valid_type = this.current_gq_enable_type;
					this.reloadGuildScript(this.gq_valid_type);
				}
				GuildManager.EnblContainer enblContainer = this.GetEnblContainer(this.gq_valid_type, true, true);
				BDic<string, WholeMapItem> wholeMapDescriptionObject = this.M2D.WM.getWholeMapDescriptionObject();
				using (BList<string> blist = ListBuffer<string>.Pop(0))
				{
					foreach (KeyValuePair<string, WholeMapItem> keyValuePair in wholeMapDescriptionObject)
					{
						if (keyValuePair.Value.enable_gq_type == gq_enable_type)
						{
							blist.Add(keyValuePair.Value.text_key);
							if (!this.OACurrentGQ.ContainsKey(keyValuePair.Value.text_key))
							{
								this.OACurrentGQ[keyValuePair.Value.text_key] = new GuildManager.GQEntryList(10, gq_enable_type, this.M2D.curMap.key);
							}
						}
					}
					int current_grank = this.current_grank;
					for (int i = blist.Count - 1; i >= 0; i--)
					{
						GuildManager.GQEntryList gqentryList = this.OACurrentGQ[blist[i]];
						for (int j = gqentryList.Count - 1; j >= 0; j--)
						{
							GuildManager.GQEntry gqentry = gqentryList[j];
							if (this.M2D.IMNG.getInventoryPrecious().getCount(gqentry.ItemEntry, -1) <= 0)
							{
								gqentry.destruct();
								this.need_fine_flag_ |= 8;
								gqentryList.RemoveAt(j);
								this.current_item_id_seek = -1;
							}
						}
					}
					if (!this.existAnyQuests())
					{
						this.need_fine_flag_ |= 8;
						this.current_item_id_seek = 62400;
					}
					for (int k = blist.Count - 1; k >= 0; k--)
					{
						string text = blist[k];
						GuildManager.GQEntryList gqentryList2 = this.OACurrentGQ[text];
						int num = this.getMaxGQEntry(current_grank);
						if (num > gqentryList2.Count)
						{
							using (BList<int> blist2 = ListBuffer<int>.Pop(0))
							{
								WholeMapItem byTextKey = this.M2D.WM.GetByTextKey(text);
								for (int l = this.ACateg.Length - 1; l >= 0; l--)
								{
									for (int m = this.ACateg[l].getEntryCount(current_grank) - 1; m >= 0; m--)
									{
										blist2.Add(l);
									}
								}
								base.shuffle<int>(blist2, -1);
								num = X.Mn(num - gqentryList2.Count, blist2.Count);
								for (int n = 0; n < num; n++)
								{
									int nextEntryId = this.getNextEntryId(gqentryList2);
									if (nextEntryId < 0)
									{
										break;
									}
									GuildManager.GQEntry gqentry2 = this.ACateg[blist2[n]].createEntry(gqentryList2, byTextKey, this.get_entry_grank(current_grank), (ushort)nextEntryId);
									if (gqentry2 != null)
									{
										this.randomizeReward(gqentry2, enblContainer);
										gqentryList2.Add(gqentry2);
									}
								}
							}
						}
					}
				}
				base.updateFirst(0);
			}
			if ((this.need_fine_flag_ & 8) != 0)
			{
				this.need_fine_flag_ -= 8;
				this.AGqCollectNew.Clear();
				foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair2 in this.OACurrentGQ)
				{
					for (int num2 = keyValuePair2.Value.Count - 1; num2 >= 0; num2--)
					{
						GuildManager.GQEntry gqentry3 = keyValuePair2.Value[num2];
						if (gqentry3.Qt != null && gqentry3.categ == GuildManager.CATEG.COLLECTNEW)
						{
							this.AGqCollectNew.Add(gqentry3);
						}
					}
				}
				if (this.AGqCollectNew.Count == 0)
				{
					this.InvForCollectNew.clearAllItems(99);
				}
			}
		}

		private void randomizeReward(GuildManager.GQEntry Gq, GuildManager.EnblContainer Enbl)
		{
			Gq.RewardIR = null;
			Gq.reward_etype = ReelExecuter.ETYPE.ITEMKIND;
			int grade = Gq.grade;
			float num = (float)this.getCateg(Gq.categ).reward_money_default(Gq.grade);
			if (base.xors(400) < 250)
			{
				if (grade <= 1)
				{
					ReelExecuter.ETYPE etype;
					switch (base.xors(5))
					{
					case 0:
						etype = ReelExecuter.ETYPE.GRADE1;
						break;
					case 1:
						etype = ReelExecuter.ETYPE.GRADE1;
						break;
					case 2:
						etype = ReelExecuter.ETYPE.COUNT_ADD1;
						break;
					case 3:
						etype = ReelExecuter.ETYPE.COUNT_ADD1;
						break;
					default:
						etype = ReelExecuter.ETYPE.GRADE2;
						break;
					}
					Gq.reward_etype = etype;
				}
				else if (grade <= 3)
				{
					ReelExecuter.ETYPE etype;
					switch (base.xors(5))
					{
					case 0:
						etype = ReelExecuter.ETYPE.GRADE1;
						break;
					case 1:
						etype = ReelExecuter.ETYPE.GRADE2;
						break;
					case 2:
						etype = ReelExecuter.ETYPE.COUNT_ADD1;
						break;
					case 3:
						etype = ReelExecuter.ETYPE.COUNT_ADD2;
						break;
					default:
						etype = ReelExecuter.ETYPE.COUNT_MUL1;
						break;
					}
					Gq.reward_etype = etype;
				}
				else
				{
					Gq.reward_etype = ReelExecuter.ETYPE.GRADE1 + base.xors(10);
					if (Gq.reward_etype == ReelExecuter.ETYPE.RARE_ADD1)
					{
						Gq.reward_etype = ReelExecuter.ETYPE.COUNT_MUL1;
					}
				}
				ReelManager.EFReel efreel = ReelManager.OAreel_content[(int)Gq.reward_etype];
				if (efreel.rarelity > 0)
				{
					num = X.NIL(1f, 0.33f, (float)efreel.rarelity, 3f) * num;
				}
			}
			else
			{
				Gq.RewardIR = Enbl.AIRreward[base.xors(Enbl.AIRreward.Count)];
			}
			Gq.reward_money = (int)num;
		}

		public void receiveQuest(GuildManager.GQEntry Gq)
		{
			GuildManager.GQEntryList gqentryList;
			if (Gq.Qt == null && this.OACurrentGQ.TryGetValue(Gq.WM.text_key, out gqentryList))
			{
				gqentryList.map_key_receive_at = ((this.M2D.curMap != null) ? this.M2D.curMap.key : "");
				Gq.Qt = this.ACateg[(int)Gq.categ].createQT(Gq, gqentryList.map_key_receive_at);
				this.M2D.QUEST.assignTemporaryQuest(Gq.Qt);
				this.M2D.QUEST.updateQuest(Gq.Qt.key, 0, false, false, false, false);
			}
			if (Gq.categ == GuildManager.CATEG.COLLECTNEW)
			{
				this.AGqCollectNew.Add(Gq);
			}
		}

		public void removeQuest(GuildManager.GQEntry Gq)
		{
			GuildManager.GQEntryList gqentryList;
			if (this.OACurrentGQ.TryGetValue(Gq.WM.text_key, out gqentryList))
			{
				gqentryList.Remove(Gq);
			}
			Gq.destruct();
			if (Gq.categ == GuildManager.CATEG.COLLECTNEW)
			{
				this.AGqCollectNew.Remove(Gq);
				this.need_fine_flag_ |= 8;
			}
		}

		public void addObtainItemForGQ(NelItem Item, int count, int grade)
		{
			if (this.AGqCollectNew.Count > 0 && this.M2D.QUEST.isQuestTargetItem(Item, -1))
			{
				this.InvForCollectNew.Add(Item, count, grade, true, true);
			}
		}

		public void summonerFinished(string gq_enable_type)
		{
			foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair in this.OACurrentGQ)
			{
				GuildManager.GQEntryList value = keyValuePair.Value;
				if (value.gq_enable_type == gq_enable_type || gq_enable_type == null)
				{
					value.progressing = true;
				}
			}
		}

		public void flushWholeMapSwitching(WholeMapItem WmPre, WholeMapItem WmAft)
		{
			GuildManager.GQEntryList gqentryList;
			if (WmPre != null && WmAft != null && !WmPre.safe_area && WmAft.safe_area && this.OACurrentGQ.TryGetValue(WmPre.text_key, out gqentryList) && gqentryList.progressing)
			{
				int count = gqentryList.Count;
				for (int i = 0; i < count; i++)
				{
					GuildManager.GQEntry gqentry = gqentryList[i];
					if (gqentry.auto_abort_in_flush_area && gqentry.Qt != null)
					{
						QuestTracker.QuestProgress progressObject = this.M2D.QUEST.getProgressObject(gqentry.Qt);
						if (progressObject != null && !progressObject.aborted && progressObject.phase == 0)
						{
							progressObject.aborted = true;
						}
					}
				}
			}
		}

		public void flushWholeMapSwitchingItemCollect(string pre_text_key)
		{
			bool flag = false;
			foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair in this.OACurrentGQ)
			{
				GuildManager.GQEntryList value = keyValuePair.Value;
				if (value.progressing && value.gq_enable_type == pre_text_key)
				{
					int count = value.Count;
					for (int i = 0; i < count; i++)
					{
						GuildManager.GQEntry gqentry = value[i];
						if (gqentry.auto_abort_in_flush_area_itemcollect && gqentry.Qt != null)
						{
							if (!flag)
							{
								flag = true;
								this.M2D.QUEST.fineAutoItemCollection(true);
							}
							QuestTracker.QuestProgress progressObject = this.M2D.QUEST.getProgressObject(gqentry.Qt);
							if (progressObject != null && !progressObject.aborted && progressObject.phase == 0)
							{
								progressObject.aborted = true;
							}
						}
					}
				}
			}
		}

		public void progressFromEventAfterSummoner()
		{
			GF.setC("GLD0", GF.getC("GLD0") & 4294967294U);
		}

		public override void readBinaryFrom(ByteReader Ba, bool noClearRandA = false)
		{
			this.newGame();
			int num = Ba.readByte();
			base.readBinaryFrom(Ba, false);
			this.gq_point = (int)Ba.readShort();
			this.need_fine_flag_ = (byte)Ba.readByte();
			this.current_item_id_seek = (int)Ba.readUShort();
			this.danger_progress = (byte)Ba.readByte();
			this.current_gq_enable_type = Ba.readPascalString("utf-8", false);
			if (this.gq_point >= 0)
			{
				this.initGQCategory();
				int num2 = Ba.readByte();
				if (num2 > 0)
				{
					for (int i = 0; i < num2; i++)
					{
						string text = Ba.readPascalString("utf-8", false);
						string text2 = Ba.readPascalString("utf-8", false);
						int num3 = 0;
						if (num >= 3)
						{
							num3 = Ba.readByte();
						}
						int num4 = Ba.readByte();
						GuildManager.GQEntryList gqentryList = null;
						WholeMapItem byTextKey = this.M2D.WM.GetByTextKey(text);
						GuildManager.EnblContainer enblContainer = this.GetEnblContainer(byTextKey.enable_gq_type, true, true);
						for (int j = 0; j < num4; j++)
						{
							int num5 = Ba.readByte();
							bool flag = false;
							if ((num5 & 128) != 0)
							{
								num5 &= -129;
								flag = true;
							}
							GuildManager.GQEntry gqentry = this.ACateg[num5].readBinaryFrom(Ba, byTextKey, num);
							if (gqentry != null && gqentry.ItemEntry != null)
							{
								if (gqentryList == null)
								{
									gqentryList = (this.OACurrentGQ[text] = new GuildManager.GQEntryList(num4, byTextKey.enable_gq_type, text2));
									gqentryList.progressing = (num3 & 1) != 0;
									gqentryList.gq_enable_type = byTextKey.enable_gq_type;
								}
								if (flag)
								{
									gqentry.Qt = this.ACateg[num5].createQT(gqentry, text2);
									this.M2D.QUEST.assignTemporaryQuest(gqentry.Qt);
								}
								gqentryList.Add(gqentry);
								if (gqentry.categ == GuildManager.CATEG.COLLECTNEW)
								{
									this.AGqCollectNew.Add(gqentry);
								}
								if (num < 4)
								{
									this.randomizeReward(gqentry, enblContainer);
								}
							}
						}
					}
				}
				if (num >= 1)
				{
					this.InvForCollectNew.readBinaryFrom(Ba, true, true, false, 9, false);
				}
				if (num >= 2)
				{
					int num6 = Ba.readByte();
					for (int k = 0; k < num6; k++)
					{
						GQCategInfo.readCategDataBinaryFrom(Ba, (k < this.ACateg.Length) ? this.ACateg[k] : null);
					}
				}
				if (num >= 4)
				{
					int num7 = Ba.readByte();
					for (int l = 0; l < num7; l++)
					{
						string text3 = Ba.readPascalString("utf-8", false);
						this.GetEnblContainer(text3, true, true).readBinaryFrom(Ba, this);
					}
				}
				if (num < 5)
				{
					base.init(false, 0U, 0U, 0U, 0U);
				}
			}
		}

		public override void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(5);
			base.writeBinaryTo(Ba);
			Ba.writeShort((short)this.gq_point);
			Ba.writeByte((int)this.need_fine_flag_);
			Ba.writeUShort((ushort)this.current_item_id_seek);
			Ba.writeByte((int)this.danger_progress);
			Ba.writePascalString(this.current_gq_enable_type, "utf-8");
			if (this.gq_point >= 0)
			{
				this.initGQCategory();
				Ba.writeByte(this.OACurrentGQ.Count);
				foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair in this.OACurrentGQ)
				{
					Ba.writePascalString(keyValuePair.Key, "utf-8");
					Ba.writePascalString(keyValuePair.Value.map_key_receive_at, "utf-8");
					Ba.writeByte(keyValuePair.Value.progressing ? 1 : 0);
					Ba.writeByte(keyValuePair.Value.Count);
					for (int i = 0; i < keyValuePair.Value.Count; i++)
					{
						GuildManager.GQEntry gqentry = keyValuePair.Value[i];
						Ba.writeByte((int)(gqentry.categ | ((gqentry.Qt != null && !gqentry.destructed) ? ((GuildManager.CATEG)128) : GuildManager.CATEG.COLLECTNEW)));
						this.getCateg(gqentry.categ).writeBinaryTo(Ba, gqentry);
					}
				}
				this.InvForCollectNew.writeBinaryTo(Ba);
				int num = this.ACateg.Length;
				Ba.writeByte(num);
				for (int j = 0; j < num; j++)
				{
					this.ACateg[j].writeCategDataBinaryTo(Ba);
				}
				Ba.writeByte(this.OEnbl.Count);
				foreach (KeyValuePair<string, GuildManager.EnblContainer> keyValuePair2 in this.OEnbl)
				{
					Ba.writePascalString(keyValuePair2.Key, "utf-8");
					keyValuePair2.Value.writeBinaryTo(Ba, this);
				}
			}
		}

		public ReelManager.ItemReelContainer getIRByKey(string key)
		{
			foreach (KeyValuePair<string, GuildManager.EnblContainer> keyValuePair in this.OEnbl)
			{
				GuildManager.EnblContainer value = keyValuePair.Value;
				if (value.AIRreward != null)
				{
					int count = value.AIRreward.Count;
					for (int i = 0; i < count; i++)
					{
						ReelManager.ItemReelContainer itemReelContainer = value.AIRreward[i];
						if (itemReelContainer.key == key)
						{
							return itemReelContainer;
						}
					}
				}
			}
			return null;
		}

		public GuildManager.EnblContainer GetEnblContainer(string gq_enable_type, bool load_data = false, bool load_ir_data = false)
		{
			GuildManager.EnblContainer enblContainer;
			if (!this.OEnbl.TryGetValue(gq_enable_type, out enblContainer))
			{
				if (!load_data)
				{
					return null;
				}
				enblContainer = (this.OEnbl[gq_enable_type] = new GuildManager.EnblContainer());
			}
			if (load_ir_data && enblContainer.AIRreward == null)
			{
				this.reloadGuildScript(gq_enable_type);
			}
			return enblContainer;
		}

		public void addReward(ReelExecuter.ETYPE reward_etype, string enable_gq_key)
		{
			if (reward_etype > ReelExecuter.ETYPE.ITEMKIND)
			{
				this.GetEnblContainer(enable_gq_key, true, true).AReelEf.Add(reward_etype);
			}
		}

		public void addReward(ReelManager.ItemReelContainer IR, string enable_gq_key)
		{
			if (IR != null)
			{
				this.GetEnblContainer(enable_gq_key, true, true).AReelItem.Add(IR);
			}
		}

		public bool isDepertTarget(WholeMapItem WM)
		{
			GuildManager.GQEntryList gqentryList;
			if (this.OACurrentGQ.TryGetValue(WM.text_key, out gqentryList))
			{
				for (int i = gqentryList.Count - 1; i >= 0; i--)
				{
					GuildManager.GQEntry gqentry = gqentryList[i];
					if (gqentry.Qt != null)
					{
						QuestTracker.QuestProgress progressObject = this.M2D.QUEST.getProgressObject(gqentry.Qt);
						if (progressObject != null && !progressObject.aborted)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool checkDepertTargetNotFinished(WholeMapItem WM, out Vector2Int V, bool check_flushing_map = false)
		{
			V = Vector2Int.zero;
			GuildManager.GQEntryList gqentryList;
			if (!this.OACurrentGQ.TryGetValue(WM.text_key, out gqentryList))
			{
				return false;
			}
			if (!gqentryList.progressing)
			{
				return false;
			}
			for (int i = gqentryList.Count - 1; i >= 0; i--)
			{
				GuildManager.GQEntry gqentry = gqentryList[i];
				if ((!check_flushing_map || gqentry.auto_abort_in_flush_area) && gqentry.Qt != null)
				{
					int num = V.y;
					V.y = num + 1;
					QuestTracker.QuestProgress progressObject = this.M2D.QUEST.getProgressObject(gqentry.Qt);
					if (progressObject != null && !progressObject.aborted && !this.getCateg(gqentry.categ).isQTFinishedAuto(gqentry, progressObject, false, true))
					{
						num = V.x;
						V.x = num + 1;
					}
				}
			}
			return V.x > 0;
		}

		public bool hasAnotherDepertTarget(WholeMapItem WM, out WholeMapItem WM_another)
		{
			WM_another = null;
			if (this.isDepertTarget(WM))
			{
				return false;
			}
			foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair in this.OACurrentGQ)
			{
				if (!(keyValuePair.Key == WM.text_key))
				{
					GuildManager.GQEntryList value = keyValuePair.Value;
					for (int i = value.Count - 1; i >= 0; i--)
					{
						GuildManager.GQEntry gqentry = value[i];
						if (gqentry.Qt != null)
						{
							QuestTracker.QuestProgress progressObject = this.M2D.QUEST.getProgressObject(gqentry.Qt);
							if (progressObject != null && !progressObject.aborted)
							{
								WM_another = gqentry.WM;
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		public bool isProgressing(string gq_enable_type)
		{
			foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair in this.OACurrentGQ)
			{
				GuildManager.GQEntryList value = keyValuePair.Value;
				if (value.gq_enable_type == gq_enable_type && value.progressing)
				{
					int count = value.Count;
					for (int i = 0; i < count; i++)
					{
						if (value[i].Qt != null)
						{
							return true;
						}
					}
					value.progressing = false;
				}
			}
			return false;
		}

		public bool isUseableSummoner(string wm_text_key, EnemySummoner Smn)
		{
			GuildManager.GQEntryList gqentryList;
			if (this.OACurrentGQ.TryGetValue(wm_text_key, out gqentryList))
			{
				for (int i = gqentryList.Count - 1; i >= 0; i--)
				{
					GuildManager.GQEntry gqentry = gqentryList[i];
					GQCategInfoDefeat gqcategInfoDefeat = this.getCateg(gqentry.categ) as GQCategInfoDefeat;
					if (gqcategInfoDefeat != null && gqcategInfoDefeat.hasSummoner(gqentry, Smn.key))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool need_fine_flag
		{
			get
			{
				return (this.need_fine_flag_ & 1) > 0;
			}
			set
			{
				if (value)
				{
					this.need_fine_flag_ |= 1;
				}
			}
		}

		public GQCategInfo getCateg(GuildManager.CATEG categ)
		{
			return this.ACateg[(int)categ];
		}

		public GuildManager.GQEntry getEntry(NelItem Itm)
		{
			foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair in this.OACurrentGQ)
			{
				for (int i = keyValuePair.Value.Count - 1; i >= 0; i--)
				{
					GuildManager.GQEntry gqentry = keyValuePair.Value[i];
					if (gqentry.ItemEntry == Itm)
					{
						return gqentry;
					}
				}
			}
			return null;
		}

		public BDic<string, GuildManager.GQEntryList> getCurrentEntryWholeObject()
		{
			return this.OACurrentGQ;
		}

		private int getNextEntryId(GuildManager.GQEntryList ACurrentGQ)
		{
			if (this.current_item_id_seek >= 0)
			{
				int num = this.current_item_id_seek;
				this.current_item_id_seek = num + 1;
				return num;
			}
			for (int i = X.Mx(62400, -this.current_item_id_seek); i < 62600; i++)
			{
				ushort num2 = (ushort)i;
				bool flag = true;
				int count = ACurrentGQ.Count;
				for (int j = 0; j < count; j++)
				{
					if (ACurrentGQ[j].ItemEntry.id == num2)
					{
						num2 += 1;
						flag = false;
						break;
					}
				}
				if (flag)
				{
					this.current_item_id_seek = X.Mn((int)(-num2 - 1), this.current_item_id_seek);
					return (int)num2;
				}
			}
			return -1;
		}

		private bool existAnyQuests()
		{
			foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair in this.OACurrentGQ)
			{
				if (keyValuePair.Value.Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public readonly NelM2DBase M2D;

		private GQCategInfo[] ACateg;

		public readonly ItemStorage InvForCollectNew;

		private BDic<string, GuildManager.GQEntryList> OACurrentGQ = new BDic<string, GuildManager.GQEntryList>();

		private readonly BDic<string, GuildManager.EnblContainer> OEnbl = new BDic<string, GuildManager.EnblContainer>();

		private List<GuildManager.GQEntry> AGqCollectNew = new List<GuildManager.GQEntry>(4);

		public const int STOCK_ITEMREEL_MAX = 5;

		public static ReelManager.ItemReelColor IRColSetDefault = new ReelManager.ItemReelColor(4288453788U, 4293322470U, 4282664004U);

		public int gq_point = -1;

		public const int MAX_GRANK = 5;

		private const ushort item_id_min = 62400;

		private const ushort item_id_max = 62600;

		public const string itemheader_quest_item = "__guildquest_";

		private const int dangerlevel_for_day = 9;

		private const string gq_script_dir = "Data/gq/";

		private const byte NEED_REMAKE = 1;

		private const byte RECHECK_ITEM_OBTAIN_NEW = 8;

		public int current_item_id_seek = 62400;

		private byte need_fine_flag_ = 1;

		public byte danger_progress;

		private string current_gq_enable_type = "";

		public string gq_valid_type = "";

		public const int GQ_POINT_MAX = 150;

		private const int gq_point_level4 = 115;

		private const int gq_point_level3 = 50;

		private const int gq_point_level2 = 20;

		private const int gq_point_level1 = 5;

		public enum CATEG
		{
			COLLECTNEW,
			COLLECT,
			DEFEAT,
			COOK,
			RESCUE,
			WEAPON,
			DELIVER,
			_MAX
		}

		public sealed class GQEntry
		{
			public void destruct()
			{
				this.destructed = true;
				NelItem.flush(this.ItemEntry, false);
				if (this.Qt != null)
				{
					this.Con.M2D.QUEST.removeTemporary(this.Qt);
				}
			}

			private GQEntry(GuildManager _Con, WholeMapItem _WM, GuildManager.CATEG _categ, int _grade)
			{
				this.Con = _Con;
				this.WM = _WM;
				this.categ = _categ;
				this.grade = _grade;
			}

			public GQEntry(GuildManager _Con, WholeMapItem _WM, GuildManager.CATEG _categ, NelItem _ItemEntry, int _grade, int _comment_id)
				: this(_Con, _WM, _categ, _grade)
			{
				this.ItemEntry = _ItemEntry;
				this.ran0 = this.Con.get0();
				this.comment_id = _comment_id;
				this.initItemEntry();
			}

			public GQEntry(GuildManager _Con, WholeMapItem _WM, GuildManager.CATEG _categ, NelItem _ItemEntry, int _grade, ByteReader Ba)
				: this(_Con, _WM, _categ, _grade)
			{
				this.ItemEntry = _ItemEntry;
				this.ran0 = Ba.readUInt();
				this.comment_id = Ba.readByte();
				this.initItemEntry();
			}

			private void initItemEntry()
			{
				if (this.ItemEntry != null)
				{
					NelItem.CreateItemEntry(this.ItemEntry.key, this.ItemEntry, (int)this.ItemEntry.id, false);
					this.ItemEntry.FnGetName = new FnGetItemDetail(this.fnGetItemName);
					this.ItemEntry.FnGetDesc = new FnGetItemDetail(this.fnGetItemDesc);
					this.ItemEntry.FnGetDetail = new FnGetItemDetail(this.fnGetItemDetail);
				}
			}

			public void writeBasicDataToBinary(ByteArray Ba)
			{
				Ba.writeUInt(this.ran0);
				Ba.writeByte(this.comment_id);
			}

			public float value1
			{
				get
				{
					return this.ItemEntry.value;
				}
				set
				{
					if (this.ItemEntry != null)
					{
						this.ItemEntry.value = value;
					}
				}
			}

			public float value2
			{
				get
				{
					return this.ItemEntry.value2;
				}
				set
				{
					if (this.ItemEntry != null)
					{
						this.ItemEntry.value2 = value;
					}
				}
			}

			public float value3
			{
				get
				{
					return this.ItemEntry.value3;
				}
				set
				{
					if (this.ItemEntry != null)
					{
						this.ItemEntry.value3 = value;
					}
				}
			}

			public bool auto_abort_qt_in_other_battle
			{
				get
				{
					return this.Con.getCateg(this.categ).auto_abort_qt_in_other_battle;
				}
			}

			public bool auto_abort_in_progressing
			{
				get
				{
					return this.Con.getCateg(this.categ).auto_abort_in_progressing;
				}
			}

			public bool auto_abort_in_flush_area
			{
				get
				{
					return this.Con.getCateg(this.categ).auto_abort_in_flush_area;
				}
			}

			public bool auto_abort_in_flush_area_itemcollect
			{
				get
				{
					return this.Con.getCateg(this.categ).auto_abort_in_flush_area_itemcollect;
				}
			}

			public string qt_key
			{
				get
				{
					return "__guildquest_" + this.ItemEntry.id.ToString();
				}
			}

			public int reward_gp
			{
				get
				{
					return this.ItemEntry.rare;
				}
			}

			public int lost_gp
			{
				get
				{
					return this.Con.getCateg(this.categ).lost_gp(this.grade);
				}
			}

			public int reward_money
			{
				get
				{
					return this.ItemEntry.price;
				}
				set
				{
					this.ItemEntry.price = value;
				}
			}

			public void getDescriptionForQT(int phase, STB Stb)
			{
				this.Con.getCateg(this.categ).getDescriptionForQT(phase, Stb, this);
			}

			private void fnGetItemName(STB Stb, NelItem I, int grade)
			{
				this.Con.getCateg(this.categ).getItemName(Stb, this, I, grade);
			}

			private void fnGetItemDesc(STB Stb, NelItem I, int grade)
			{
				this.Con.getCateg(this.categ).getItemDesc(Stb, this, I, grade);
			}

			public bool isQTFinishedAuto(bool auto_check)
			{
				if (this.Qt == null)
				{
					return false;
				}
				QuestTracker.QuestProgress progressObject = this.Con.M2D.QUEST.getProgressObject(this.Qt);
				return progressObject != null && !progressObject.aborted && this.Con.getCateg(this.categ).isQTFinishedAuto(this, progressObject, auto_check, false);
			}

			public bool isQTAbortedAuto()
			{
				if (this.Qt == null)
				{
					return false;
				}
				QuestTracker.QuestProgress progressObject = this.Con.M2D.QUEST.getProgressObject(this.Qt);
				return progressObject != null && progressObject.aborted;
			}

			private void fnGetItemDetail(STB Stb, NelItem I, int grade)
			{
				GQCategInfo gqcategInfo = this.Con.getCateg(this.categ);
				gqcategInfo.getItemDetail(Stb, this, I, grade);
				gqcategInfo.getItemDetailAfter(Stb, this);
			}

			public readonly GuildManager Con;

			public readonly WholeMapItem WM;

			public readonly GuildManager.CATEG categ;

			public readonly NelItem ItemEntry;

			public readonly int grade;

			public readonly uint ran0;

			public QuestTracker.Quest Qt;

			public int position_x;

			public int position_y;

			public bool destructed;

			public object TargetItem;

			public int comment_id;

			public ReelExecuter.ETYPE reward_etype;

			public ReelManager.ItemReelContainer RewardIR;
		}

		public class GQEntryList : List<GuildManager.GQEntry>
		{
			public GQEntryList(int capacity, string _gq_enable_type, string _map_key_receive_at)
				: base(capacity)
			{
				this.gq_enable_type = _gq_enable_type;
				this.map_key_receive_at = _map_key_receive_at;
			}

			public SupplyManager.ItemDescriptorForWholeMap getDescriptor(GuildManager Con, string wm_text_key)
			{
				if (this.IDesc.valid)
				{
					return this.IDesc;
				}
				this.IDesc = SupplyManager.createWholeMapDescriptor(Con.M2D, wm_text_key);
				return this.IDesc;
			}

			public void copyRowsToStorage(string wm_key, ItemStorage Str)
			{
				for (int i = 0; i < base.Count; i++)
				{
					GuildManager.GQEntry gqentry = base[i];
					Str.Add(gqentry.ItemEntry, 1, gqentry.grade, true, true);
				}
				Str.fineRows(false);
			}

			public string gq_enable_type;

			public bool progressing;

			public string map_key_receive_at = "";

			public List<ReelManager.ItemReelContainer> AIRItem;

			private SupplyManager.ItemDescriptorForWholeMap IDesc;
		}

		public class EnblContainer
		{
			public List<ReelManager.ItemReelContainer> AIRreward { get; private set; }

			public EnblContainer()
			{
				this.AReelItem = new List<ReelManager.ItemReelContainer>();
				this.AReelEf = new List<ReelExecuter.ETYPE>();
			}

			public void newGame()
			{
				this.AReelItem.Clear();
				this.AReelEf.Clear();
			}

			public List<ReelManager.ItemReelContainer> createRewardReelItemList()
			{
				if (this.AIRreward == null)
				{
					this.AIRreward = new List<ReelManager.ItemReelContainer>(10);
				}
				return this.AIRreward;
			}

			public void releaseRewardReelItem()
			{
				if (this.AIRreward != null)
				{
					for (int i = this.AIRreward.Count - 1; i >= 0; i--)
					{
						ReelManager.ItemReelContainer itemReelContainer = this.AIRreward[i];
						for (int j = 0; j < 2; j++)
						{
							NelItem nelItem = ((j == 0) ? NelItem.GetById(itemReelContainer.item_ckey, true) : NelItem.GetById(itemReelContainer.item_gkey, true));
							if (nelItem != null)
							{
								try
								{
									NelItem.flush(nelItem, true);
								}
								catch
								{
								}
							}
						}
					}
				}
				this.AIRreward = null;
			}

			public void readBinaryFrom(ByteReader Ba, GuildManager Con)
			{
				this.newGame();
				int num = (int)Ba.readUShort();
				for (int i = 0; i < num; i++)
				{
					this.AReelEf.Add((ReelExecuter.ETYPE)Ba.readUByte());
				}
				num = (int)Ba.readUShort();
				for (int j = 0; j < num; j++)
				{
					ReelManager.ItemReelContainer irbyKey = Con.getIRByKey(Ba.readPascalString("utf-8", false));
					if (irbyKey != null)
					{
						this.AReelItem.Add(irbyKey);
					}
				}
			}

			public void writeBinaryTo(ByteArray Ba, GuildManager Con)
			{
				int num = this.AReelEf.Count;
				Ba.writeUShort((ushort)num);
				for (int i = 0; i < num; i++)
				{
					Ba.writeByte((int)this.AReelEf[i]);
				}
				num = this.AReelItem.Count;
				Ba.writeUShort((ushort)num);
				for (int j = 0; j < num; j++)
				{
					Ba.writePascalString(this.AReelItem[j].key, "utf-8");
				}
			}

			public readonly List<ReelManager.ItemReelContainer> AReelItem;

			public readonly List<ReelExecuter.ETYPE> AReelEf;
		}
	}
}
