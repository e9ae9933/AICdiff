using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class QuestTracker
	{
		public static bool initQuestScript()
		{
			string resource = TX.getResource("Data/quest", ref QuestTracker.LoadDate, ".csv", false, "Resources/");
			if (resource == null)
			{
				return false;
			}
			QuestTracker.OQuest = new BDic<string, QuestTracker.Quest>((QuestTracker.OQuest != null) ? QuestTracker.OQuest.Count : 10);
			List<QuestTracker.QuestDeperture> list = new List<QuestTracker.QuestDeperture>();
			List<QuestTracker.QuestItemBufList> list2 = new List<QuestTracker.QuestItemBufList>();
			CsvReader csvReader = new CsvReader(resource, CsvReader.RegSpace, false);
			csvReader.tilde_replace = false;
			QuestTracker.CATEG categ = QuestTracker.CATEG.MAIN;
			QuestTracker.Quest quest = null;
			ushort num = 0;
			bool flag = false;
			bool flag2 = false;
			string text = "Noel";
			while (csvReader.read())
			{
				if (TX.isStart(csvReader.cmd, "##", 0))
				{
					string text2 = csvReader.cmd;
					if (text2 != null)
					{
						if (text2 == "##UID")
						{
							num = (ushort)csvReader.Nm(1, (float)num);
							continue;
						}
						if (text2 == "##CLIENT_DEFAULT")
						{
							text = csvReader._1;
							continue;
						}
					}
					string[] array = TX.slice(csvReader.cmd, 2).Split(new char[] { '|' });
					int num2 = array.Length;
					categ = (QuestTracker.CATEG)0;
					for (int i = 0; i < num2; i++)
					{
						QuestTracker.CATEG categ2;
						if (FEnum<QuestTracker.CATEG>.TryParse(array[i], out categ2, false))
						{
							categ |= categ2;
						}
					}
					if (categ == (QuestTracker.CATEG)0)
					{
						categ = QuestTracker.CATEG.MAIN;
					}
				}
				else if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					if (quest != null)
					{
						quest.finalizeLoading(list, list2, ref flag, ref flag2);
					}
					if (!QuestTracker.OQuest.TryGetValue(index, out quest))
					{
						Dictionary<string, QuestTracker.Quest> oquest = QuestTracker.OQuest;
						string text3 = index;
						string text4 = index;
						QuestTracker.CATEG categ3 = categ;
						ushort num3 = num;
						num = num3 + 1;
						quest = (oquest[text3] = new QuestTracker.Quest(text4, categ3, num3, text));
					}
				}
				else if (quest != null)
				{
					string text2 = csvReader.cmd;
					if (text2 != null)
					{
						uint num4 = <PrivateImplementationDetails>.ComputeStringHash(text2);
						if (num4 <= 2707589251U)
						{
							if (num4 <= 1158081290U)
							{
								if (num4 != 820522307U)
								{
									if (num4 != 1085591854U)
									{
										if (num4 == 1158081290U)
										{
											if (text2 == "end_phase")
											{
												quest.end_phase = QuestTracker.fixPhase(quest, csvReader._1, list);
											}
										}
									}
									else if (text2 == "%IMPORTANT")
									{
										quest.important = (byte)csvReader.Int(1, 0);
									}
								}
								else if (text2 == "deperture")
								{
									int num5 = QuestTracker.fixPhase(quest, csvReader._1, list);
									QuestTracker.QuestDeperture questDeperture = list[num5];
									questDeperture.wm_key = csvReader._2;
									questDeperture.map_key = csvReader._3;
									questDeperture.real_map_target = csvReader._4;
									if (TX.noe(questDeperture.real_map_target))
									{
										if (TX.isStart(questDeperture.map_key, '@') && REG.match(questDeperture.map_key, WholeMapItem.RegWholeSpiSelect))
										{
											questDeperture.real_map_target = REG.R1;
										}
										else
										{
											questDeperture.real_map_target = questDeperture.map_key;
										}
									}
									list[num5] = questDeperture;
								}
							}
							else
							{
								if (num4 <= 2399039025U)
								{
									if (num4 != 1501762398U)
									{
										if (num4 != 2399039025U)
										{
											continue;
										}
										if (!(text2 == "collect"))
										{
											continue;
										}
									}
									else
									{
										if (!(text2 == "%SHOW_PROGRESS_NUM"))
										{
											continue;
										}
										if (csvReader.clength == 1)
										{
											flag = true;
											continue;
										}
										for (int j = 1; j < csvReader.clength; j++)
										{
											int num6 = csvReader.Int(j, -1);
											if (num6 >= 0)
											{
												QuestTracker.QuestDeperture questDeperture2 = list[num6];
												questDeperture2.show_progress_num = true;
												list[num6] = questDeperture2;
											}
										}
										continue;
									}
								}
								else if (num4 != 2588867956U)
								{
									if (num4 != 2707589251U)
									{
										continue;
									}
									if (!(text2 == "%INVISIBLE"))
									{
										continue;
									}
									quest.invisible = (QuestTracker.INVISIBLE)0;
									for (int k = 1; k < csvReader.clength; k++)
									{
										string[] array2 = csvReader.getIndex(k).Split(new char[] { '|' });
										int num7 = array2.Length;
										for (int l = 0; l < num7; l++)
										{
											QuestTracker.INVISIBLE invisible;
											if (FEnum<QuestTracker.INVISIBLE>.TryParse(array2[l], out invisible, false))
											{
												quest.invisible |= invisible;
											}
										}
									}
									continue;
								}
								else if (!(text2 == "collect_new"))
								{
									continue;
								}
								int num5 = QuestTracker.fixPhase(quest, csvReader._1, list);
								while (list2.Count <= num5)
								{
									list2.Add(null);
								}
								if (list2[num5] == null)
								{
									list2[num5] = new QuestTracker.QuestItemBufList(1);
								}
								if (csvReader.cmd == "collect_new")
								{
									list2[num5].need_new_item = true;
								}
								NelItem byId = NelItem.GetById(csvReader._2, false);
								if (byId != null)
								{
									list2[num5].Add(new NelItemEntry(byId, X.Mx(1, csvReader.Int(3, 1)), (byte)X.MMX(0, csvReader.Int(4, 0), 4)));
								}
							}
						}
						else if (num4 <= 3447462692U)
						{
							if (num4 != 3240287073U)
							{
								if (num4 != 3266374662U)
								{
									if (num4 == 3447462692U)
									{
										if (text2 == "treasure")
										{
											int num5 = QuestTracker.fixPhase(quest, csvReader._1, list);
											QuestTracker.QuestDeperture questDeperture = list[num5];
											questDeperture.treasure_img_key = csvReader._2;
											list[num5] = questDeperture;
										}
									}
								}
								else if (text2 == "auto_check_item_collection")
								{
									flag2 = true;
								}
							}
							else if (text2 == "%DESC_KEY")
							{
								quest.desc_key = csvReader._1;
							}
						}
						else if (num4 <= 3823199323U)
						{
							if (num4 != 3649057272U)
							{
								if (num4 == 3823199323U)
								{
									if (text2 == "auto_hide")
									{
										int num5 = X.NmI(csvReader._1, -1, false, false);
										if (num5 >= 0)
										{
											quest.auto_hide_bits |= 1U << num5;
										}
									}
								}
							}
							else if (text2 == "phase")
							{
								int num5 = QuestTracker.fixPhase(quest, csvReader._1, list);
							}
						}
						else if (num4 != 4133275983U)
						{
							if (num4 == 4271544270U)
							{
								if (text2 == "%TALKER_REPLACE")
								{
									quest.addTelkerReplaceTerm(csvReader._1, csvReader.slice_join(1, " ", ""));
								}
							}
						}
						else if (text2 == "%CLIENT")
						{
							quest.client = csvReader._1;
						}
					}
				}
			}
			if (quest != null)
			{
				quest.finalizeLoading(list, list2, ref flag, ref flag2);
			}
			return true;
		}

		private static int fixPhase(QuestTracker.Quest Current, string v, List<QuestTracker.QuestDeperture> ADepBuf)
		{
			int num = Current.calcPhase(v);
			while (ADepBuf.Count <= num)
			{
				ADepBuf.Add(default(QuestTracker.QuestDeperture));
			}
			return num;
		}

		public static void fineNameLocalizedWhole()
		{
			foreach (KeyValuePair<string, QuestTracker.Quest> keyValuePair in QuestTracker.OQuest)
			{
				keyValuePair.Value.flushLocalizeDesc();
			}
		}

		public QuestTracker.Quest Get(string key, bool no_error = false)
		{
			this.fineScript();
			return QuestTracker.GetS(key, no_error);
		}

		public static QuestTracker.Quest GetS(string key, bool no_error = false)
		{
			QuestTracker.Quest quest;
			if (!QuestTracker.OQuest.TryGetValue(key, out quest) && !no_error)
			{
				X.de("不明な Quest: " + key, null);
			}
			return quest;
		}

		private static bool GetFromBa(ByteReader Ba, out QuestTracker.Quest Q, bool no_error = false)
		{
			ushort num = Ba.readUShort();
			if (num == 64999)
			{
				Q = null;
				return false;
			}
			if (num != 65000)
			{
				Q = QuestTracker.GetById(num, no_error);
			}
			else
			{
				Q = QuestTracker.GetS(Ba.readPascalString("utf-8", false), false);
			}
			return true;
		}

		private static void writeKeyToBa(ByteArray Ba, QuestTracker.Quest Q)
		{
			if (Q == null)
			{
				Ba.writeUShort(64999);
				return;
			}
			Ba.writeUShort(Q.uid);
			if (Q.uid == 65000)
			{
				Ba.writePascalString(Q.key, "utf-8");
			}
		}

		private static QuestTracker.Quest GetById(ushort id, bool no_error = false)
		{
			foreach (KeyValuePair<string, QuestTracker.Quest> keyValuePair in QuestTracker.OQuest)
			{
				if (keyValuePair.Value.uid == id)
				{
					return keyValuePair.Value;
				}
			}
			if (!no_error)
			{
				X.de("不明な Quest id: " + id.ToString(), null);
			}
			return null;
		}

		public static void newGameS()
		{
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				foreach (KeyValuePair<string, QuestTracker.Quest> keyValuePair in QuestTracker.OQuest)
				{
					if (keyValuePair.Value.isTemporary())
					{
						blist.Add(keyValuePair.Key);
					}
				}
				for (int i = blist.Count - 1; i >= 0; i--)
				{
					QuestTracker.OQuest.Remove(blist[i]);
				}
			}
		}

		public void assignTemporaryQuest(QuestTracker.Quest Qt)
		{
			if (Qt.uid != 65000)
			{
				X.de("TEMP_UID ではないクエストを追加しようとしています", null);
			}
			QuestTracker.OQuest[Qt.key] = Qt;
			this.need_fine_item_quest_target = true;
			this.need_fine_auto_check_item_collection = true;
		}

		public void removeTemporary(QuestTracker.Quest Qt)
		{
			if (Qt.uid != 65000)
			{
				X.de("TEMP_UID ではないクエストを破棄しようとしています", null);
			}
			this.need_fine_item_quest_target = true;
			QuestTracker.OQuest.Remove(Qt.key);
			this.need_fine_auto_check_item_collection = true;
			using (BList<int> blist = ListBuffer<int>.Pop(0))
			{
				foreach (KeyValuePair<QuestTracker.CATEG, QuestTracker.Quest> keyValuePair in this.OSelectedQuest)
				{
					if (keyValuePair.Value == Qt)
					{
						blist.Add((int)keyValuePair.Key);
					}
				}
				for (int i = blist.Count - 1; i >= 0; i--)
				{
					this.OSelectedQuest.Remove((QuestTracker.CATEG)blist[i]);
				}
			}
			for (int j = 0; j < 3; j++)
			{
				List<QuestTracker.QuestProgress> list = ((j == 0) ? this.AProg : ((j == 1) ? this.AProgFinished : this.AProgBuf));
				for (int k = list.Count - 1; k >= 0; k--)
				{
					if (list[k].Q == Qt)
					{
						list.RemoveAt(k);
						break;
					}
				}
			}
		}

		public QuestTracker(NelM2DBase _M2D)
		{
			this.AProg = new List<QuestTracker.QuestProgress>(4);
			this.AProgFinished = new List<QuestTracker.QuestProgress>(4);
			this.AProgBuf = new List<QuestTracker.QuestProgress>(4);
			this.OAQuestTarget = new BDic<NelItem, BList<QuestTracker.QuestProgress>>();
			this.OSelectedQuest = new BDic<QuestTracker.CATEG, QuestTracker.Quest>(2);
			this.M2D = _M2D;
			this.FD_fnSortProgressRow = new Comparison<QuestTracker.QuestProgress>(this.fnSortProgressRow);
		}

		public void newGame()
		{
			this.AProg.Clear();
			this.AProgFinished.Clear();
			this.AProgBuf.Clear();
			this.OSelectedQuest.Clear();
			this.current_update = (this.current_created = 0U);
			this.need_fine_item_quest_target = true;
			this.sort_type = QuestTracker.SORT_TYPE.NEWER;
			this.need_fine_pos = true;
			this.need_fine_head_quest_ = false;
			this.need_fine_auto_check_item_collection = true;
			this.HeadQuest_ = null;
			if (this.Ui != null)
			{
				this.Ui.clearTask(true);
			}
		}

		public QuestTracker fineScript()
		{
			if (QuestTracker.initQuestScript())
			{
				for (int i = this.AProg.Count - 1; i >= 0; i--)
				{
					QuestTracker.QuestProgress questProgress = this.AProg[i];
					questProgress.Q = X.Get<string, QuestTracker.Quest>(QuestTracker.OQuest, questProgress.Q.key);
					if (questProgress.Q == null)
					{
						this.AProg.RemoveAt(i);
					}
				}
			}
			return this;
		}

		public void destruct()
		{
			this.releaseItemQuestTargetList();
		}

		public void readBinaryFrom(ByteReader Ba)
		{
			this.newGame();
			this.need_fine_head_quest_ = (this.need_fine_auto_check_item_collection = true);
			this.fineScript();
			int num = Ba.readByte();
			int num2;
			for (int i = 0; i < 2; i++)
			{
				List<QuestTracker.QuestProgress> list = ((i == 0) ? this.AProg : this.AProgFinished);
				num2 = Ba.readInt();
				uint num3 = 0U;
				for (int j = 0; j < num2; j++)
				{
					QuestTracker.QuestProgress questProgress = QuestTracker.QuestProgress.readBinaryFrom(Ba, num, ref num3);
					if (questProgress != null)
					{
						list.Add(questProgress);
						this.current_update = X.Mx(questProgress.update_count, this.current_update);
					}
				}
				if (i == 0)
				{
					this.current_created = num3;
				}
			}
			this.sort_type = (QuestTracker.SORT_TYPE)Ba.readByte();
			num2 = Ba.readByte();
			for (int k = 0; k < num2; k++)
			{
				QuestTracker.CATEG categ = (QuestTracker.CATEG)Ba.readByte();
				QuestTracker.Quest quest;
				if (QuestTracker.GetFromBa(Ba, out quest, true) && quest != null)
				{
					this.OSelectedQuest[categ] = quest;
				}
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(1);
			for (int i = 0; i < 2; i++)
			{
				List<QuestTracker.QuestProgress> list = ((i == 0) ? this.AProg : this.AProgFinished);
				int count = list.Count;
				Ba.writeInt(count);
				for (int j = 0; j < count; j++)
				{
					list[j].writeBinaryTo(Ba);
				}
			}
			Ba.writeByte((int)this.sort_type);
			Ba.writeByte(this.OSelectedQuest.Count);
			foreach (KeyValuePair<QuestTracker.CATEG, QuestTracker.Quest> keyValuePair in this.OSelectedQuest)
			{
				Ba.writeByte((int)keyValuePair.Key);
				QuestTracker.writeKeyToBa(Ba, keyValuePair.Value);
			}
		}

		public void updateQuest(string k, int phase, bool hidden = false, bool fill_target_item = false, bool set_focus = false, bool fix_phase = false)
		{
			QuestTracker.Quest quest = this.Get(k, false);
			if (quest == null || phase < 0)
			{
				return;
			}
			phase = X.Mn(phase, quest.end_phase);
			QuestTracker.QuestProgress questProgress = null;
			int i = this.AProg.Count - 1;
			while (i >= 0)
			{
				QuestTracker.QuestProgress questProgress2 = this.AProg[i];
				if (questProgress2.Q == quest)
				{
					if (fix_phase ? (questProgress2.phase == phase) : (questProgress2.phase >= phase))
					{
						return;
					}
					questProgress2.phase = phase;
					questProgress2.countUpdate(ref this.current_update);
					if (this.updateQuestCollectItemInStorage(questProgress2, i, hidden))
					{
						hidden = true;
					}
					this.ProgressAnounce(questProgress2, i, hidden, 0U);
					questProgress = questProgress2;
					break;
				}
				else
				{
					i--;
				}
			}
			if (phase >= quest.end_phase)
			{
				return;
			}
			for (int j = this.AProgFinished.Count - 1; j >= 0; j--)
			{
				if (this.AProgFinished[j].Q == quest)
				{
					return;
				}
			}
			if (questProgress == null)
			{
				List<QuestTracker.QuestProgress> aprog = this.AProg;
				QuestTracker.Quest quest2 = quest;
				uint num = this.current_created;
				this.current_created = num + 1U;
				uint num2 = num;
				num = this.current_update + 1U;
				this.current_update = num;
				aprog.Add(questProgress = new QuestTracker.QuestProgress(quest2, num2, num, phase));
				if (this.updateQuestCollectItemInStorage(questProgress, this.AProg.Count - 1, hidden))
				{
					hidden = true;
				}
				if (!hidden && this.Ui != null && (quest.invisible & QuestTracker.INVISIBLE.START) == (QuestTracker.INVISIBLE)0)
				{
					this.Ui.AddStack(questProgress, QuestTracker.INVISIBLE.START, phase, 0U);
				}
				this.need_fine_head_quest_ = (this.need_fine_auto_check_item_collection = true);
			}
			if (questProgress != null && !questProgress.finished)
			{
				if (fill_target_item && phase > 0)
				{
					bool flag;
					NelItemEntry[] collectTarget = quest.getCollectTarget(phase - 1, out flag);
					if (collectTarget != null)
					{
						for (int l = collectTarget.Length - 1; l >= 0; l--)
						{
							NelItemEntry nelItemEntry = collectTarget[l];
							questProgress.addCollectionCount(phase - 1, nelItemEntry.Data, nelItemEntry.count, (int)nelItemEntry.grade, true);
						}
					}
				}
				if (set_focus && !questProgress.finished)
				{
					this.setFocusedQuest(questProgress);
				}
			}
		}

		public void fineAutoItemCollection(bool force = false)
		{
			if (!this.need_fine_auto_check_item_collection && !force)
			{
				return;
			}
			this.need_fine_auto_check_item_collection = false;
			ItemStorage[] array = null;
			for (int i = this.AProg.Count - 1; i >= 0; i--)
			{
				QuestTracker.QuestProgress questProgress = this.AProg[i];
				if (questProgress.Q.auto_check_item_collection)
				{
					for (int j = questProgress.phase; j >= 0; j--)
					{
						bool flag;
						if (questProgress.Q.getCollectTarget(j, out flag) == null)
						{
							if (j < questProgress.phase)
							{
								break;
							}
						}
						else
						{
							if (array == null)
							{
								array = this.M2D.IMNG.getInventoryArray();
							}
							questProgress.clearCollectionCount(j);
							this.updateQuestCollectItemInStorage(questProgress, i, j, true, array);
						}
					}
				}
			}
		}

		public void remove(string k)
		{
			QuestTracker.Quest quest = this.Get(k, false);
			for (int i = this.AProg.Count - 1; i >= 0; i--)
			{
				if (this.AProg[i].Q == quest)
				{
					this.AProg.RemoveAt(i);
					this.need_fine_head_quest_ = (this.need_fine_auto_check_item_collection = true);
				}
			}
		}

		private bool ProgressAnounce(QuestTracker.QuestProgress Prog, int index, bool hidden = false, uint written_row_bits_for_task = 0U)
		{
			bool flag = false;
			QuestTracker.Quest q = Prog.Q;
			Prog.new_icon = true;
			this.need_fine_item_quest_target = true;
			if (Prog.phase >= q.end_phase)
			{
				Prog.phase = q.end_phase;
				Prog.tracking = false;
				if ((q.invisible & QuestTracker.INVISIBLE.END) == (QuestTracker.INVISIBLE)0 && !hidden && this.Ui != null)
				{
					this.Ui.AddStack(Prog, QuestTracker.INVISIBLE.END, Prog.phase, written_row_bits_for_task);
					flag = true;
				}
				if (Prog == this.HeadQuest_)
				{
					this.need_fine_head_quest_ = true;
				}
				this.AProg.RemoveAt(index);
				this.AProgFinished.Add(Prog);
				this.AProgFinished.Sort((QuestTracker.QuestProgress Pa, QuestTracker.QuestProgress Pb) => (int)(Pa.created - Pb.created));
			}
			else if ((q.invisible & QuestTracker.INVISIBLE.UPDATE) == (QuestTracker.INVISIBLE)0 && !hidden && this.Ui != null)
			{
				this.Ui.AddStack(Prog, QuestTracker.INVISIBLE.UPDATE, Prog.phase, written_row_bits_for_task);
				flag = true;
			}
			return flag;
		}

		public bool updateQuestCollectItemInStorage(QuestTracker.QuestProgress Prog, int index, bool hidden)
		{
			return this.updateQuestCollectItemInStorage(Prog, index, Prog.phase, hidden, null);
		}

		public bool updateQuestCollectItemInStorage(QuestTracker.QuestProgress Prog, int index, int phase, bool hidden, ItemStorage[] AInv = null)
		{
			bool flag;
			NelItemEntry[] collectTarget = Prog.Q.getCollectTarget(phase, out flag);
			bool flag2 = false;
			if (collectTarget != null && !flag && this.M2D != null)
			{
				if (AInv == null)
				{
					AInv = this.M2D.IMNG.getInventoryArray();
				}
				int num = collectTarget.Length;
				foreach (ItemStorage itemStorage in AInv)
				{
					for (int j = 0; j < num; j++)
					{
						NelItem data = collectTarget[j].Data;
						ItemStorage.ObtainInfo info = itemStorage.getInfo(data);
						if (info != null)
						{
							for (int k = (int)collectTarget[j].grade; k < 5; k++)
							{
								int count = info.getCount(k);
								if (count > 0 && this.updateQuestCollectItem(Prog, index, phase, data, ref count, k, true, hidden) > 0)
								{
									flag2 = true;
								}
							}
						}
					}
				}
				if (Prog.phase < Prog.Q.end_phase && Prog.Q.auto_check_item_collection && !Prog.collectionFinished(phase))
				{
					Prog.phase = X.Mn(phase, Prog.phase);
				}
			}
			return flag2;
		}

		public int updateQuestCollectItem(NelItem Itm, int count, int grade, bool obtaining = true)
		{
			int num = 0;
			if (this.fineItemQuestTargetList() && count > 0)
			{
				for (int i = this.AProg.Count - 1; i >= 0; i--)
				{
					QuestTracker.QuestProgress questProgress = this.AProg[i];
					num += this.updateQuestCollectItem(questProgress, i, questProgress.phase, Itm, ref count, grade, obtaining, false);
				}
			}
			return num;
		}

		private int updateQuestCollectItem(QuestTracker.QuestProgress Prog, int index, int phase, NelItem Itm, ref int count, int grade, bool obtaining = true, bool hidden = false)
		{
			int num = phase;
			int num2 = 0;
			int num3 = num;
			while (num3 <= num && count > 0)
			{
				bool flag = false;
				uint num5;
				int num4 = Prog.addCollectionCount(num3, Itm, count, grade, out num5, obtaining);
				if (num4 > 0)
				{
					count -= num4;
					num2 += num4;
					Prog.countUpdate(ref this.current_update);
					bool flag2 = !hidden;
					if (Prog.collectionFinished(num3))
					{
						flag = true;
						int num6 = X.Mx(Prog.phase, num3 + 1);
						if (num6 != Prog.phase)
						{
							Prog.phase = num6;
							if (this.ProgressAnounce(Prog, index, hidden, num5))
							{
								flag2 = false;
							}
						}
					}
					if (flag2 && this.Ui != null && (Prog.Q.invisible & QuestTracker.INVISIBLE.COLLECT) == (QuestTracker.INVISIBLE)0)
					{
						this.Ui.AddStack(Prog, QuestTracker.INVISIBLE.COLLECT, num3, num5);
					}
					if (count <= 0)
					{
						break;
					}
				}
				if (flag || Prog.Q.cascade_show_phase(num3))
				{
					num++;
				}
				num3++;
			}
			return num2++;
		}

		public QuestTracker.SummonerEntry getSummonerEntryFor(EnemySummoner Smn, out QuestTracker.QuestProgress OutProg)
		{
			int count = this.AProg.Count;
			OutProg = null;
			for (int i = 0; i < count; i++)
			{
				QuestTracker.QuestProgress questProgress = this.AProg[i];
				if (!questProgress.finished)
				{
					QuestTracker.SummonerEntry[] summonerEntryTarget = questProgress.Q.getSummonerEntryTarget(questProgress.phase);
					if (summonerEntryTarget != null && !questProgress.summonerAlreadyDefeated(questProgress.phase, Smn.key))
					{
						for (int j = summonerEntryTarget.Length - 1; j >= 0; j--)
						{
							if (summonerEntryTarget[j].summoner_key == Smn.key)
							{
								OutProg = questProgress;
								return summonerEntryTarget[j];
							}
						}
					}
				}
			}
			return default(QuestTracker.SummonerEntry);
		}

		public int summonerFinished(EnemySummoner Smn, bool noel_defeated, bool hidden = false)
		{
			bool flag = !hidden;
			bool flag2 = false;
			if (!noel_defeated)
			{
				int count = this.AProg.Count;
				bool flag3 = false;
				for (int i = 0; i < count; i++)
				{
					QuestTracker.QuestProgress questProgress = this.AProg[i];
					if (!questProgress.finished)
					{
						if (questProgress.Q.GQ != null && this.M2D.WM.CurWM != null)
						{
							if (!questProgress.aborted)
							{
								flag3 = true;
							}
							if (questProgress.Q.GQ.WM.enable_gq_type == this.M2D.WM.CurWM.enable_gq_type)
							{
								string enable_gq_type = questProgress.Q.GQ.WM.enable_gq_type;
							}
							if (questProgress.Q.GQ.WM != this.M2D.WM.CurWM && questProgress.Q.GQ.auto_abort_qt_in_other_battle && !questProgress.Q.GQ.isQTFinishedAuto(true))
							{
								flag2 = (questProgress.aborted = true);
							}
						}
						uint num2;
						int num = (int)questProgress.defeatedSummoner(questProgress.phase, Smn, out num2);
						if (num > 0)
						{
							if (!questProgress.aborted && questProgress.collectionFinished(questProgress.phase))
							{
								questProgress.phase++;
								if (this.ProgressAnounce(questProgress, i, hidden, num2))
								{
									flag = false;
								}
							}
							if (num == 2 && flag && this.Ui != null && !questProgress.aborted && (questProgress.Q.invisible & QuestTracker.INVISIBLE.COLLECT) == (QuestTracker.INVISIBLE)0)
							{
								this.Ui.AddStack(questProgress, QuestTracker.INVISIBLE.COLLECT, questProgress.phase, num2);
							}
						}
					}
				}
				if (flag3)
				{
					this.M2D.GUILD.summonerFinished(null);
				}
			}
			else
			{
				int count2 = this.AProg.Count;
				bool flag4 = false;
				for (int j = 0; j < count2; j++)
				{
					QuestTracker.QuestProgress questProgress2 = this.AProg[j];
					if (!questProgress2.finished && !questProgress2.aborted)
					{
						if (questProgress2.Q.GQ != null)
						{
							flag4 = true;
						}
						if (questProgress2.Q.GQ != null && this.M2D.WM.CurWM != null && !questProgress2.Q.GQ.isQTFinishedAuto(true))
						{
							flag2 = (questProgress2.aborted = true);
						}
					}
				}
				if (flag4)
				{
					this.M2D.GUILD.summonerFinished(null);
				}
			}
			if (flag2)
			{
				GF.setC("GLD_RENZOKU", 0U);
			}
			return 0;
		}

		public void positionNoticeCheck(Map2d Mp)
		{
			if (this.Ui == null)
			{
				return;
			}
			this.need_fine_pos = false;
			int count = this.AProg.Count;
			for (int i = 0; i < count; i++)
			{
				QuestTracker.QuestProgress questProgress = this.AProg[i];
				if (!questProgress.finished && !questProgress.aborted && (questProgress.Q.invisible & QuestTracker.INVISIBLE.POS) == (QuestTracker.INVISIBLE)0 && questProgress.isDepertArrived(this.M2D, Mp.key))
				{
					this.Ui.AddStack(questProgress, QuestTracker.INVISIBLE.POS, questProgress.phase, 0U);
				}
			}
		}

		public bool fineItemQuestTargetList()
		{
			if (this.need_fine_item_quest_target)
			{
				int count = this.AProg.Count;
				this.releaseItemQuestTargetList();
				this.need_fine_item_quest_target = false;
				for (int i = 0; i < count; i++)
				{
					QuestTracker.QuestProgress questProgress = this.AProg[i];
					if (!questProgress.finished && !questProgress.aborted)
					{
						int phase = questProgress.phase;
						for (int j = 0; j <= phase; j++)
						{
							bool flag;
							NelItemEntry[] collectTarget = questProgress.Q.getCollectTarget(j, out flag);
							if (collectTarget != null)
							{
								int num = collectTarget.Length;
								for (int k = 0; k < num; k++)
								{
									NelItem data = collectTarget[k].Data;
									BList<QuestTracker.QuestProgress> blist;
									if (!this.OAQuestTarget.TryGetValue(data, out blist))
									{
										blist = (this.OAQuestTarget[data] = ListBuffer<QuestTracker.QuestProgress>.Pop(0));
									}
									X.pushIdentical<QuestTracker.QuestProgress>(blist, questProgress);
								}
							}
						}
					}
				}
			}
			return this.OAQuestTarget.Count > 0;
		}

		private void releaseItemQuestTargetList()
		{
			foreach (KeyValuePair<NelItem, BList<QuestTracker.QuestProgress>> keyValuePair in this.OAQuestTarget)
			{
				ListBuffer<QuestTracker.QuestProgress>.Release(keyValuePair.Value);
			}
			this.need_fine_item_quest_target = true;
			this.OAQuestTarget.Clear();
		}

		public bool isQuestTargetItem(NelItem Itm, int grade = -1)
		{
			this.fineAutoItemCollection(false);
			BList<QuestTracker.QuestProgress> blist;
			if (this.fineItemQuestTargetList() && this.OAQuestTarget.TryGetValue(Itm, out blist))
			{
				if (grade < 0)
				{
					return true;
				}
				for (int i = blist.Count - 1; i >= 0; i--)
				{
					QuestTracker.QuestProgress questProgress = blist[i];
					int num = questProgress.phase;
					for (int j = num; j <= num; j++)
					{
						if (questProgress.isQuestTargetItem(j, Itm, grade))
						{
							return true;
						}
						if (questProgress.Q.cascade_show_phase(j))
						{
							num++;
						}
					}
				}
			}
			return false;
		}

		public void reassign(Designer DsSort, Designer GmTargetDs, QuestTracker.CATEG _target_categ)
		{
			this.GmTargetDs = GmTargetDs;
			this.target_categ = _target_categ;
			this.PSortDesc = DsSort.Get("__sort_desc", false) as FillBlock;
			this.SortBCon = (DsSort.Get("__sort_container", false) as BtnContainerRunner).BCon;
			this.fineSortButton(this.SortBCon, false);
		}

		public void releaseDesigner()
		{
			this.GmTargetDs = null;
			this.SortBCon = null;
			this.PSortDesc = null;
		}

		public void createSortButton(Designer Ds)
		{
			DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
			dsnDataButtonMulti.name = "__sort_container";
			dsnDataButtonMulti.titles = new string[] { "sort_newer", "sort_latest_update" };
			dsnDataButtonMulti.skin = "mini_sort";
			dsnDataButtonMulti.w = 28f;
			dsnDataButtonMulti.h = 28f;
			dsnDataButtonMulti.clms = 0;
			dsnDataButtonMulti.margin_w = 1f;
			dsnDataButtonMulti.margin_h = 0f;
			dsnDataButtonMulti.unselectable = 2;
			dsnDataButtonMulti.fnClick = new FnBtnBindings(this.fnSortButtonClicked);
			dsnDataButtonMulti.fnMaking = delegate(BtnContainer<aBtn> BCon, aBtn B)
			{
				IN.setZ(B.transform, -0.1f);
				return true;
			};
			this.SortBCon = Ds.addButtonMultiT<aBtnNel>(dsnDataButtonMulti);
			this.PSortDesc = Ds.XSh(10f).addP(new DsnDataP("", false)
			{
				name = "__sort_desc",
				TxCol = C32.d2c(4283780170U),
				text = "<key sort/>",
				swidth = Ds.use_w - 40f,
				sheight = 26f,
				html = true,
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.BOTTOM,
				text_auto_wrap = false,
				text_auto_condense = true
			}, false);
			Ds.Br();
			this.fineSortButton(this.SortBCon, false);
		}

		private void fineSortButton(BtnContainerBasic BCon, bool play_snd = true)
		{
			if (BCon == null)
			{
				return;
			}
			int length = BCon.Length;
			QuestTracker.SORT_TYPE sort_TYPE = this.sort_type & (QuestTracker.SORT_TYPE)(-9);
			for (int i = 0; i < length; i++)
			{
				aBtn button = BCon.GetButton(i);
				QuestTracker.SORT_TYPE sort_TYPE2;
				FEnum<QuestTracker.SORT_TYPE>.TryParse(TX.slice(button.title, 5).ToUpper(), out sort_TYPE2, true);
				if (sort_TYPE2 == sort_TYPE)
				{
					(button.get_Skin() as ButtonSkinMiniSortNel).is_descend = (this.sort_type & QuestTracker.SORT_TYPE._DESCEND) > QuestTracker.SORT_TYPE.NEWER;
					button.SetChecked(true, false);
					button.Fine(false);
				}
				else
				{
					button.SetChecked(false, true);
				}
			}
			if (this.PSortDesc != null)
			{
				FillBlock psortDesc = this.PSortDesc;
				string text = "<key sort/>";
				string text2 = "quest_sort_";
				QuestTracker.SORT_TYPE sort_TYPE3 = sort_TYPE;
				psortDesc.setValue(text + TX.Get(text2 + sort_TYPE3.ToString().ToLower(), ""));
			}
			if (play_snd)
			{
				SND.Ui.play("toggle_button_open", false);
			}
		}

		public void progressSortByLshKey()
		{
			int num = (int)(this.sort_type & (QuestTracker.SORT_TYPE)(-9));
			QuestTracker.SORT_TYPE sort_TYPE = this.sort_type & QuestTracker.SORT_TYPE._DESCEND;
			sort_TYPE &= (QuestTracker.SORT_TYPE)(-9);
			num = (num + 1) % 2;
			this.sort_type = (QuestTracker.SORT_TYPE)(num | (int)sort_TYPE);
			if (this.SortBCon != null)
			{
				this.fineSortButton(this.SortBCon, true);
			}
		}

		private List<QuestTracker.QuestProgress> prepareBuffer()
		{
			this.AProgBuf.Clear();
			for (int i = 0; i < 2; i++)
			{
				List<QuestTracker.QuestProgress> list = ((i == 0) ? this.AProg : this.AProgFinished);
				int count = list.Count;
				for (int j = 0; j < count; j++)
				{
					QuestTracker.QuestProgress questProgress = list[j];
					if ((questProgress.Q.categ & this.target_categ) != (QuestTracker.CATEG)0 && (i != 1 || (questProgress.Q.invisible & QuestTracker.INVISIBLE.AUTOREM) == (QuestTracker.INVISIBLE)0))
					{
						this.AProgBuf.Add(questProgress);
					}
				}
			}
			this.AProgBuf.Sort(this.FD_fnSortProgressRow);
			return this.AProgBuf;
		}

		public bool createGmUi(Designer Ds, QuestTracker.CATEG categ, FnBtnBindings fnClickBtn, UiQuestCard.FnQuestBtnTouched fnQBtnTouched)
		{
			this.GmTargetDs = Ds;
			this.target_categ = categ;
			List<QuestTracker.QuestProgress> list = this.prepareBuffer();
			int count = list.Count;
			if (count == 0)
			{
				return false;
			}
			Ds.item_margin_y_px = 14f;
			for (int i = 0; i < count; i++)
			{
				QuestTracker.QuestProgress questProgress = list[i];
				if (!questProgress.finished && this.Ui != null)
				{
					this.Ui.deactivateRow(questProgress, true);
				}
				UiQuestCard uiQuestCard = Ds.addTabT<UiQuestCard>("tab_" + i.ToString(), Ds.use_w, 0f, Ds.use_w, 80f, false);
				uiQuestCard.QM = this;
				uiQuestCard.fnQBtnTouched = fnQBtnTouched;
				uiQuestCard.initQ(Ds.stencil_ref, questProgress, fnClickBtn);
				uiQuestCard.BelongDesigner = Ds;
				Ds.endTab(true);
				Ds.Br();
			}
			UiQuestCard.relinkNaviAll(Ds, null);
			return true;
		}

		public bool selectGmFirstButton()
		{
			if (this.GmTargetDs == null)
			{
				return false;
			}
			using (BList<DesignerRowMem.DsnMem> blist = ListBuffer<DesignerRowMem.DsnMem>.Pop(0))
			{
				this.GmTargetDs.getRowManager().copyMems(blist, null, null);
				int count = blist.Count;
				QuestTracker.Quest quest = X.Get<QuestTracker.CATEG, QuestTracker.Quest>(this.OSelectedQuest, this.target_categ);
				UiQuestCard uiQuestCard = null;
				for (int i = 0; i < count; i++)
				{
					UiQuestCard uiQuestCard2 = blist[i].Blk as UiQuestCard;
					if (!(uiQuestCard2 == null))
					{
						if (uiQuestCard == null)
						{
							uiQuestCard = uiQuestCard2;
						}
						if (quest != null && uiQuestCard2.Is(quest))
						{
							uiQuestCard = uiQuestCard2;
							break;
						}
					}
				}
				if (uiQuestCard != null)
				{
					uiQuestCard.SelectFirstButton(null);
					if (this.GmTargetDs.CurrentAttachScroll != null)
					{
						this.GmTargetDs.CurrentAttachScroll.reveal(uiQuestCard, true, REVEALTYPE.ALWAYS);
					}
					return true;
				}
			}
			return false;
		}

		private bool fnSortButtonClicked(aBtn B)
		{
			QuestTracker.SORT_TYPE sort_TYPE;
			FEnum<QuestTracker.SORT_TYPE>.TryParse(TX.slice(B.title, 5).ToUpper(), out sort_TYPE, true);
			QuestTracker.SORT_TYPE sort_TYPE2 = this.sort_type & (QuestTracker.SORT_TYPE)(-9);
			if (sort_TYPE == sort_TYPE2)
			{
				this.sort_type = sort_TYPE2 | (((this.sort_type & QuestTracker.SORT_TYPE._DESCEND) != QuestTracker.SORT_TYPE.NEWER) ? QuestTracker.SORT_TYPE.NEWER : QuestTracker.SORT_TYPE._DESCEND);
			}
			else
			{
				this.sort_type = (this.sort_type & QuestTracker.SORT_TYPE._DESCEND) | sort_TYPE;
			}
			this.fineSortButton(B.Container, true);
			if (this.GmTargetDs != null)
			{
				UiQuestCard uiQuestCard = null;
				string text = null;
				if (aBtn.PreSelected != null)
				{
					uiQuestCard = this.getTabForButton(aBtn.PreSelected);
					text = aBtn.PreSelected.title;
				}
				List<QuestTracker.QuestProgress> list = this.prepareBuffer();
				using (BList<Designer.EvacuateMem> blist = ListBuffer<Designer.EvacuateMem>.Pop(list.Count))
				{
					using (BList<Designer.EvacuateMem> blist2 = ListBuffer<Designer.EvacuateMem>.Pop(list.Count))
					{
						this.GmTargetDs.EvacuateMemory(blist, (Designer Ds, DesignerRowMem.DsnMem DMem) => DMem.Blk is UiQuestCard, false);
						int count = list.Count;
						int num = blist.Count;
						for (int i = 0; i < count; i++)
						{
							QuestTracker.QuestProgress questProgress = list[i];
							for (int j = 0; j < num; j++)
							{
								if ((blist[j].Blk as UiQuestCard).Prog == questProgress)
								{
									blist2.Add(blist[j]);
									blist.RemoveAt(j);
									num--;
									break;
								}
							}
						}
						this.GmTargetDs.Clear();
						this.GmTargetDs.ReassignEvacuatedMemory(blist2, null, false);
					}
				}
				if (uiQuestCard != null)
				{
					aBtn aBtn = uiQuestCard.SelectFirstButton(text);
					if (aBtn != null && this.GmTargetDs.CurrentAttachScroll != null)
					{
						this.GmTargetDs.CurrentAttachScroll.reveal(aBtn, true, REVEALTYPE.ALWAYS);
					}
				}
				UiQuestCard.relinkNaviAll(this.GmTargetDs, null);
			}
			return true;
		}

		private int fnSortProgressRow(QuestTracker.QuestProgress ProgA, QuestTracker.QuestProgress ProgB)
		{
			return QuestTracker.sortProg(ProgA, ProgB, this.sort_type);
		}

		public static int sortProg(QuestTracker.QuestProgress ProgA, QuestTracker.QuestProgress ProgB, QuestTracker.SORT_TYPE sort_type)
		{
			int num = X.MPF((sort_type & QuestTracker.SORT_TYPE._DESCEND) == QuestTracker.SORT_TYPE.NEWER);
			sort_type &= (QuestTracker.SORT_TYPE)(-9);
			if (ProgA.finished != ProgB.finished)
			{
				if (!ProgA.finished)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				if (sort_type == QuestTracker.SORT_TYPE.NEWER)
				{
					return (int)(-(ProgA.created - ProgB.created) * (uint)num);
				}
				return (int)(-(ProgA.update_count - ProgB.update_count) * (uint)num);
			}
		}

		public static int sortProgInWM(QuestTracker.QuestProgress ProgA, QuestTracker.QuestProgress ProgB)
		{
			if (ProgA.tracking == ProgB.tracking)
			{
				return QuestTracker.sortProg(ProgA, ProgB, QuestTracker.SORT_TYPE.NEWER);
			}
			if (!ProgA.tracking)
			{
				return 1;
			}
			return -1;
		}

		public UiQuestCard getTabForButton(aBtn B)
		{
			if (B is UiQuestCard.aBtnNelQuestCard)
			{
				return (B as UiQuestCard.aBtnNelQuestCard).DsCard;
			}
			return null;
		}

		public UiQuestCard getTabForQuest(QuestTracker.Quest Q)
		{
			if (!(this.GmTargetDs != null))
			{
				return null;
			}
			return UiQuestCard.getTabForQuest(this.GmTargetDs, Q);
		}

		public void UiDefaultFocusChange(QuestTracker.QuestProgress Prog)
		{
			if (this.GmTargetDs != null)
			{
				this.UiDefaultFocusChange(this.target_categ, Prog.Q);
			}
		}

		public void UiDefaultFocusChange(QuestTracker.CATEG target_categ, QuestTracker.Quest Q)
		{
			this.OSelectedQuest[target_categ] = Q;
		}

		public bool hasNewProg(QuestTracker.CATEG target_categ)
		{
			for (int i = this.AProg.Count - 1; i >= 0; i--)
			{
				QuestTracker.QuestProgress questProgress = this.AProg[i];
				if ((questProgress.Q.categ & target_categ) != (QuestTracker.CATEG)0 && questProgress.new_icon)
				{
					return true;
				}
			}
			return false;
		}

		public void listupDepertureInCurrentWm(List<QuestTracker.QuestDepertureOnMap> ADep, string showing_wm_key)
		{
			WholeMapItem byTextKey = this.M2D.WM.GetByTextKey(showing_wm_key);
			if (byTextKey == null)
			{
				return;
			}
			int count = this.AProg.Count;
			string text = null;
			WholeMapItem wholeMapItem = null;
			using (BList<MapPosition> blist = ListBuffer<MapPosition>.Pop(0))
			{
				for (int i = 0; i < count; i++)
				{
					QuestTracker.QuestProgress questProgress = this.AProg[i];
					QuestTracker.QuestDeperture currentDepert = questProgress.CurrentDepert;
					if (currentDepert.isActiveMap())
					{
						if (text != currentDepert.wm_key)
						{
							text = currentDepert.wm_key;
							wholeMapItem = this.M2D.WM.GetByTextKey(currentDepert.wm_key);
						}
						if (wholeMapItem != null)
						{
							blist.Clear();
							if (currentDepert.listupWmDepert(this.M2D, wholeMapItem, blist, byTextKey, questProgress, questProgress.phase))
							{
								int count2 = blist.Count;
								for (int j = 0; j < count2; j++)
								{
									MapPosition mapPosition = blist[j];
									bool flag = true;
									for (int k = ADep.Count - 1; k >= 0; k--)
									{
										QuestTracker.QuestDepertureOnMap questDepertureOnMap = ADep[k];
										if (X.LENGTHXYS(questDepertureOnMap.x, questDepertureOnMap.y, mapPosition.x, mapPosition.y) < 0.1f)
										{
											questDepertureOnMap.addProg(this.M2D, questProgress);
											flag = false;
										}
									}
									if (flag)
									{
										ADep.Add(new QuestTracker.QuestDepertureOnMap(mapPosition.x, mapPosition.y, this.M2D, questProgress, mapPosition.Mp));
									}
								}
							}
						}
					}
				}
			}
		}

		public void listupDepertureWa(BDic<string, QuestTracker.QuestDepertureOnWa> ODep)
		{
			int count = this.AProg.Count;
			string text = null;
			QuestTracker.QuestDepertureOnWa questDepertureOnWa = null;
			for (int i = 0; i < count; i++)
			{
				QuestTracker.QuestProgress questProgress = this.AProg[i];
				QuestTracker.QuestDeperture currentDepert = questProgress.CurrentDepert;
				if (currentDepert.isActiveMap())
				{
					if (currentDepert.wm_key != text)
					{
						text = currentDepert.wm_key;
						questDepertureOnWa = X.Get<string, QuestTracker.QuestDepertureOnWa>(ODep, text);
						if (questDepertureOnWa == null)
						{
							WAManager.WARecord wa = WAManager.GetWa(text, false);
							if (wa != null)
							{
								questDepertureOnWa = (ODep[text] = new QuestTracker.QuestDepertureOnWa(wa));
							}
						}
					}
					if (questDepertureOnWa != null)
					{
						questDepertureOnWa.addProg(this.M2D, questProgress);
					}
				}
			}
		}

		public QuestTracker.QuestProgress getHeadQuest()
		{
			if (this.need_fine_head_quest_)
			{
				this.need_fine_head_quest_ = false;
				this.HeadQuest_ = null;
				int num = -1;
				for (int i = this.AProg.Count - 1; i >= 0; i--)
				{
					QuestTracker.QuestProgress questProgress = this.AProg[i];
					int priority = questProgress.priority;
					if (priority > num)
					{
						num = priority;
						this.HeadQuest_ = questProgress;
					}
				}
			}
			return this.HeadQuest_;
		}

		public bool need_fine_head_quest
		{
			get
			{
				return this.need_fine_head_quest_;
			}
			set
			{
				if (value)
				{
					this.need_fine_auto_check_item_collection = value;
					this.need_fine_head_quest_ = value;
				}
			}
		}

		public void setFocusedQuest(QuestTracker.QuestProgress Prog)
		{
			if (!Prog.tracking)
			{
				Prog.tracking = true;
				for (int i = this.AProg.Count - 1; i >= 0; i--)
				{
					QuestTracker.QuestProgress questProgress = this.AProg[i];
					if (questProgress != Prog)
					{
						questProgress.tracking = false;
					}
				}
				this.need_fine_head_quest = true;
				if (this.GmTargetDs != null)
				{
					int num = 0;
					for (;;)
					{
						UiQuestCard uiQuestCard = this.GmTargetDs.getTab("tab_" + num++.ToString()) as UiQuestCard;
						if (!(uiQuestCard != null))
						{
							break;
						}
						aBtn btn = uiQuestCard.getBtn("quest_tracking");
						if (btn != null)
						{
							bool flag = uiQuestCard.Prog == Prog;
							if (flag != btn.isChecked())
							{
								btn.SetChecked(flag, true);
								uiQuestCard.need_redraw_background = true;
							}
						}
					}
				}
			}
		}

		public int getProgress(string quest_key)
		{
			for (int i = this.AProg.Count - 1; i >= 0; i--)
			{
				QuestTracker.QuestProgress questProgress = this.AProg[i];
				if (questProgress.Q.key == quest_key)
				{
					return questProgress.phase;
				}
			}
			return -1;
		}

		public QuestTracker.QuestProgress getProgressObject(QuestTracker.Quest Q)
		{
			for (int i = this.AProg.Count - 1; i >= 0; i--)
			{
				QuestTracker.QuestProgress questProgress = this.AProg[i];
				if (questProgress.Q == Q)
				{
					return questProgress;
				}
			}
			return null;
		}

		public const string need_new_item_wm = "need_new";

		private static BDic<string, QuestTracker.Quest> OQuest;

		private static DateTime LoadDate;

		public const string CLIENT_GUILDQUEST = "Guildq";

		public const ushort TEMP_UID = 65000;

		public const ushort REMOVED_UID = 64999;

		public UiQuestTracker Ui;

		public readonly NelM2DBase M2D;

		private List<QuestTracker.QuestProgress> AProg;

		private List<QuestTracker.QuestProgress> AProgFinished;

		private List<QuestTracker.QuestProgress> AProgBuf;

		private BDic<QuestTracker.CATEG, QuestTracker.Quest> OSelectedQuest;

		private uint current_update;

		private uint current_created;

		private QuestTracker.SORT_TYPE sort_type;

		private Comparison<QuestTracker.QuestProgress> FD_fnSortProgressRow;

		private bool need_fine_item_quest_target;

		private readonly BDic<NelItem, BList<QuestTracker.QuestProgress>> OAQuestTarget;

		private bool need_fine_head_quest_;

		private QuestTracker.QuestProgress HeadQuest_;

		public bool need_fine_auto_check_item_collection;

		public bool need_fine_pos;

		private Designer GmTargetDs;

		private FillBlock PSortDesc;

		private QuestTracker.CATEG target_categ;

		private BtnContainer<aBtn> SortBCon;

		public enum CATEG
		{
			MAIN = 1,
			SUB,
			_ALL
		}

		public enum SORT_TYPE
		{
			NEWER,
			LATEST_UPDATE,
			_ALL_TYPE,
			_DESCEND = 8
		}

		public enum INVISIBLE
		{
			START = 1,
			UPDATE,
			POS = 4,
			END = 8,
			COLLECT = 16,
			_ALL = 31,
			AUTOREM = 4096
		}

		public struct SummonerEntry
		{
			public SummonerEntry(string _summoner_key, string _quest_key, int add_dlevel_, WeatherItem.WEATHER _weather, ENATTR nattr_, int _fix_enemykind, int nattr_addable_max_)
			{
				this.summoner_key = _summoner_key;
				this.quest_key = _quest_key;
				this.add_dlevel = add_dlevel_;
				this.weather = _weather;
				this.nattr = nattr_;
				this.nattr_addable_max = nattr_addable_max_;
				this.fix_enemykind = _fix_enemykind;
			}

			public bool valid
			{
				get
				{
					return this.summoner_key != null;
				}
			}

			public bool Equals(QuestTracker.SummonerEntry Src)
			{
				return Src.summoner_key == this.summoner_key && Src.quest_key == this.quest_key;
			}

			public bool kindMatch(string s)
			{
				ENEMYID enemyid;
				return this.valid && this.fix_enemykind >= 0 && FEnum<ENEMYID>.TryParse(s, out enemyid, true) && enemyid == (ENEMYID)this.fix_enemykind;
			}

			public string summoner_key;

			public string quest_key;

			public int add_dlevel;

			public WeatherItem.WEATHER weather;

			public ENATTR nattr;

			public int nattr_addable_max;

			public int fix_enemykind;
		}

		public class Quest
		{
			public Quest(string _key, QuestTracker.CATEG _categ, ushort _uid, string _client)
			{
				this.key = _key;
				this.desc_key = "quest__" + this.key;
				this.categ = _categ;
				this.uid = _uid;
				this.client = _client;
			}

			public int calcPhase(string t)
			{
				STB stb = TX.PopBld(t, 0);
				int num = 0;
				bool flag = false;
				int num2 = 0;
				for (;;)
				{
					if (stb.isStart("^", num2))
					{
						num++;
					}
					else
					{
						if (!stb.isStart("~", num2))
						{
							break;
						}
						flag = true;
					}
					num2++;
				}
				int num3 = stb.NmI(num2, -1, 0);
				if (flag)
				{
					this.smooth_show_bits |= 1U << num3;
				}
				int num4 = num3;
				while (--num >= 0 && --num4 >= 0)
				{
					this.visible_cascade_next_bits |= 1U << num4;
				}
				TX.ReleaseBld(stb);
				if (num3 >= 64)
				{
					num3 = 63;
				}
				return num3;
			}

			public void addTelkerReplaceTerm(string replace_key, string term)
			{
				if (this.ATalerReplaceTerm == null)
				{
					this.ATalerReplaceTerm = new List<QuestTracker.Quest.TalkerTerm>(1);
				}
				this.ATalerReplaceTerm.Add(new QuestTracker.Quest.TalkerTerm(replace_key, term));
			}

			public void finalizeLoading(List<QuestTracker.QuestDeperture> ADepBuf, List<QuestTracker.QuestItemBufList> AADepItem, ref bool show_progress_num_all, ref bool auto_check_item_collection)
			{
				this.end_phase = X.Mx(this.end_phase, X.Mx(this.end_phase, X.Mx((AADepItem == null) ? 0 : AADepItem.Count, ADepBuf.Count)));
				while (this.end_phase > ADepBuf.Count)
				{
					ADepBuf.Add(default(QuestTracker.QuestDeperture));
				}
				bool flag = false;
				if (AADepItem != null)
				{
					for (int i = AADepItem.Count - 1; i >= 0; i--)
					{
						QuestTracker.QuestItemBufList questItemBufList = AADepItem[i];
						if (questItemBufList != null)
						{
							QuestTracker.QuestDeperture questDeperture = ADepBuf[i];
							questDeperture.AItm = questItemBufList.ToArray();
							questDeperture.wm_key = (questItemBufList.need_new_item ? "need_new" : "");
							ADepBuf[i] = questDeperture;
							flag = true;
						}
					}
				}
				if (show_progress_num_all)
				{
					for (int j = ADepBuf.Count - 1; j >= 0; j--)
					{
						QuestTracker.QuestDeperture questDeperture2 = ADepBuf[j];
						questDeperture2.show_progress_num = true;
						ADepBuf[j] = questDeperture2;
					}
				}
				this.auto_check_item_collection = flag & auto_check_item_collection;
				auto_check_item_collection = (show_progress_num_all = false);
				this.Adepert = ADepBuf.ToArray();
				ADepBuf.Clear();
				if (AADepItem != null)
				{
					AADepItem.Clear();
				}
				this.Adesc_buffer = new string[this.end_phase];
			}

			public string getDescription(int phase)
			{
				if (phase >= this.end_phase)
				{
					return null;
				}
				string text = this.Adesc_buffer[phase];
				if (text != null)
				{
					return text;
				}
				string text2 = this.desc_key ?? this.key;
				using (STB stb = TX.PopBld(null, 0))
				{
					int i;
					if (this.GQ != null)
					{
						this.GQ.getDescriptionForQT(phase, stb);
						text = stb.ToString();
						i = phase;
					}
					else
					{
						for (i = phase; i >= 0; i--)
						{
							stb.Clear();
							stb.Add(text2).Add("__").Add(i);
							TX tx = TX.getTX(stb.ToString(), true, true, null);
							if (tx != null)
							{
								text = tx.text;
								break;
							}
						}
						if (text == null)
						{
							i = 0;
							TX tx2 = TX.getTX(text2, true, true, null);
							text = ((tx2 == null) ? "" : tx2.text);
						}
						if (i < 0)
						{
							i = 0;
						}
					}
					while (i <= phase)
					{
						if (this.Adesc_buffer[i] == null)
						{
							if (this.Adepert[i].show_progress_num)
							{
								stb.Set(text).Add(" ", i + 1, " / ").Add(this.end_phase);
								this.Adesc_buffer[i] = stb.ToString();
							}
							else
							{
								this.Adesc_buffer[i] = text;
							}
						}
						i++;
					}
				}
				return text;
			}

			public void getClientData(out string localize_name, out PxlFrame PFIco, out uint personal_color)
			{
				PFIco = null;
				localize_name = "Mob";
				personal_color = 4293980400U;
				if (TX.valid(this.client))
				{
					if (this.client == "Guildq")
					{
						PFIco = MTRX.getPF("IconGQ");
						personal_color = 4291622970U;
						localize_name = TX.Get("MAP_city_in_guild", "");
						if (this.GQ != null)
						{
							localize_name = localize_name + " (" + this.GQ.WM.localized_name + ")";
						}
						return;
					}
					if (this.ClientPerson == null)
					{
						this.ClientPerson = EvPerson.getPersonByNameDefault(this.client);
						if (this.ClientPerson == null)
						{
							this.client = null;
						}
					}
					if (this.ClientPerson != null)
					{
						PFIco = this.ClientPerson.SmallIconPF;
						personal_color = this.ClientPerson.personal_color;
						localize_name = this.ClientPerson.talker_name_first;
					}
				}
				if (this.ATalerReplaceTerm != null)
				{
					for (int i = this.ATalerReplaceTerm.Count - 1; i >= 0; i--)
					{
						QuestTracker.Quest.TalkerTerm talkerTerm = this.ATalerReplaceTerm[i];
						if (talkerTerm.Term == null || talkerTerm.Term.getValue(null) != 0.0)
						{
							localize_name = this.ATalerReplaceTerm[i].replace_key;
							break;
						}
					}
				}
				localize_name = TX.Get("Talker_" + localize_name, "");
			}

			public bool cascade_show_phase(int phase)
			{
				return phase < this.end_phase - 1 && (this.visible_cascade_next_bits & (1U << phase)) > 0U;
			}

			public bool smooth_to_next_phase(int phase)
			{
				return phase < this.end_phase - 1 && (this.smooth_show_bits & (1U << phase)) > 0U;
			}

			public void flushLocalizeDesc()
			{
				X.ALLN<string>(this.Adesc_buffer);
			}

			public int CollectTargetMax()
			{
				for (int i = this.end_phase - 1; i >= 0; i--)
				{
					if (this.hasCollect(i))
					{
						return i + 1;
					}
				}
				return 0;
			}

			public bool hasCollect(int phase)
			{
				return this.Adepert != null && phase < this.end_phase && phase < this.Adepert.Length && (this.Adepert[phase].AItm is NelItemEntry[] || this.Adepert[phase].AItm is QuestTracker.SummonerEntry[]);
			}

			public QuestTracker.QuestDeperture getDepert(int phase)
			{
				if (this.Adepert == null || phase >= this.end_phase || phase >= this.Adepert.Length)
				{
					return default(QuestTracker.QuestDeperture);
				}
				return this.Adepert[phase];
			}

			public NelItemEntry[] getCollectTarget(int phase, out bool need_new_item)
			{
				need_new_item = false;
				if (phase >= this.end_phase || phase >= this.Adepert.Length)
				{
					return null;
				}
				NelItemEntry[] array = this.Adepert[phase].AItm as NelItemEntry[];
				if (array != null)
				{
					need_new_item = this.Adepert[phase].wm_key == "need_new";
				}
				return array;
			}

			public QuestTracker.SummonerEntry[] getSummonerEntryTarget(int phase)
			{
				if (phase >= this.end_phase || phase >= this.Adepert.Length)
				{
					return null;
				}
				return this.Adepert[phase].AItm as QuestTracker.SummonerEntry[];
			}

			public EvImg getTreasureImg(int phase)
			{
				if (phase >= this.end_phase || phase >= this.Adepert.Length)
				{
					return null;
				}
				string treasure_img_key = this.Adepert[phase].treasure_img_key;
				if (!TX.valid(treasure_img_key))
				{
					return null;
				}
				return EV.Pics.getPic(treasure_img_key, true, false);
			}

			public Color32 PinColor
			{
				get
				{
					if ((this.categ & QuestTracker.CATEG.MAIN) != (QuestTracker.CATEG)0)
					{
						return C32.d2c(4294737932U);
					}
					return C32.d2c(4280734660U);
				}
			}

			public bool abortable
			{
				get
				{
					return false;
				}
			}

			public bool getFieldGuideTarget(int phase, out object Target)
			{
				if (this.GQ != null && this.GQ.Con.getCateg(this.GQ.categ).getFieldGuideTarget(phase, this.GQ, out Target))
				{
					return true;
				}
				phase = X.Mn(X.Mn(phase, this.Adepert.Length - 1), this.end_phase - 1);
				for (int i = phase; i >= 0; i--)
				{
					bool flag;
					NelItemEntry[] collectTarget = this.getCollectTarget(i, out flag);
					if (collectTarget != null)
					{
						Target = collectTarget[0].Data;
						return true;
					}
					QuestTracker.SummonerEntry[] summonerEntryTarget = this.getSummonerEntryTarget(i);
					if (summonerEntryTarget != null)
					{
						Target = EnemySummoner.Get(summonerEntryTarget[0].summoner_key, false);
						return true;
					}
				}
				Target = null;
				return false;
			}

			public override string ToString()
			{
				return "<Quest> " + this.key;
			}

			public bool isTemporary()
			{
				return this.uid == 65000;
			}

			public readonly ushort uid;

			public readonly string key;

			public readonly QuestTracker.CATEG categ;

			public string desc_key;

			public GuildManager.GQEntry GQ;

			private QuestTracker.QuestDeperture[] Adepert;

			public int end_phase = 1;

			public uint auto_hide_bits;

			public bool auto_check_item_collection;

			private uint visible_cascade_next_bits;

			private uint smooth_show_bits;

			public QuestTracker.INVISIBLE invisible;

			public byte important;

			public string client;

			private EvPerson ClientPerson;

			private List<QuestTracker.Quest.TalkerTerm> ATalerReplaceTerm;

			public string[] Adesc_buffer;

			public struct TalkerTerm
			{
				public TalkerTerm(string _replace_key, string _term)
				{
					this.replace_key = _replace_key;
					if (TX.valid(_term))
					{
						this.Term = new EvalP(null).Parse(_term);
						return;
					}
					this.Term = null;
				}

				public string replace_key;

				public EvalP Term;
			}
		}

		public class QuestItemBufList : List<NelItemEntry>
		{
			public QuestItemBufList(int cap = 0)
				: base(cap)
			{
			}

			public bool need_new_item;
		}

		public struct QuestDeperture
		{
			public QuestDeperture(QuestTracker.QuestDeperture Src)
			{
				this.wm_key = Src.wm_key;
				this.map_key = Src.map_key;
				this.real_map_target = Src.real_map_target;
				this.show_progress_num = Src.show_progress_num;
				this.AItm = Src.AItm;
				this.treasure_img_key = Src.treasure_img_key;
			}

			public bool isActiveMap()
			{
				return this.wm_key != null && (this.AItm == null || this.AItm is QuestTracker.SummonerEntry[]);
			}

			public WmDeperture WmDepert(QuestTracker.QuestProgress Prog, int phase)
			{
				if (TX.valid(this.map_key))
				{
					return new WmDeperture(this.wm_key, this.map_key);
				}
				if (Prog != null && this.AItm is QuestTracker.SummonerEntry[] && M2DBase.Instance != null)
				{
					NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
					WholeMapItem byTextKey = nelM2DBase.WM.GetByTextKey(this.wm_key);
					if (byTextKey != null)
					{
						EnemySummonerManager manager = EnemySummonerManager.GetManager(this.wm_key);
						QuestTracker.SummonerEntry[] array = this.AItm as QuestTracker.SummonerEntry[];
						int num = array.Length;
						for (int i = 0; i < num; i++)
						{
							string summoner_key = array[i].summoner_key;
							WMIconPosition wmiconPosition;
							if (!Prog.summonerAlreadyDefeated(phase, summoner_key) && manager.getWMPosition(nelM2DBase, summoner_key, byTextKey, out wmiconPosition, true, false))
							{
								return new WmDeperture(this.wm_key, wmiconPosition.getDepertureMap().key);
							}
						}
					}
				}
				return new WmDeperture(this.wm_key, this.map_key);
			}

			public bool listupWmDepert(NelM2DBase M2D, WholeMapItem Wm, List<MapPosition> APosBuf, WholeMapItem WmShowingFrom, QuestTracker.QuestProgress Prog, int phase)
			{
				bool flag = new WmDeperture(this.wm_key, this.map_key).getPosCache(Wm).getPos(M2D, WmShowingFrom, APosBuf);
				if (this.AItm is QuestTracker.SummonerEntry[])
				{
					EnemySummonerManager manager = EnemySummonerManager.GetManager(this.wm_key);
					QuestTracker.SummonerEntry[] array = this.AItm as QuestTracker.SummonerEntry[];
					int num = array.Length;
					for (int i = 0; i < num; i++)
					{
						string summoner_key = array[i].summoner_key;
						WMIconPosition wmiconPosition;
						if (!Prog.summonerAlreadyDefeated(phase, summoner_key) && manager.getWMPosition(M2D, summoner_key, Wm, out wmiconPosition, true, false))
						{
							flag = new WmDeperture(this.wm_key, wmiconPosition.getDepertureMap().key).getPosCache(Wm).getPos(M2D, WmShowingFrom, APosBuf) || flag;
						}
					}
				}
				return flag;
			}

			public bool isEqual(QuestTracker.QuestDeperture S)
			{
				if (S.AItm != null != (this.AItm != null))
				{
					return false;
				}
				if (this.AItm is NelItemEntry[])
				{
					NelItemEntry[] array = this.AItm as NelItemEntry[];
					NelItemEntry[] array2 = S.AItm as NelItemEntry[];
					if (array2 == null || array.Length != array2.Length)
					{
						return false;
					}
					for (int i = array.Length - 1; i >= 0; i--)
					{
						if (array[i] != array2[i])
						{
							return false;
						}
					}
					return true;
				}
				else
				{
					if (!(this.AItm is QuestTracker.SummonerEntry[]))
					{
						return this.wm_key == S.wm_key && this.map_key == S.map_key && this.real_map_target == S.real_map_target;
					}
					QuestTracker.SummonerEntry[] array3 = this.AItm as QuestTracker.SummonerEntry[];
					QuestTracker.SummonerEntry[] array4 = S.AItm as QuestTracker.SummonerEntry[];
					if (array4 == null || array3.Length != array4.Length)
					{
						return false;
					}
					for (int j = array3.Length - 1; j >= 0; j--)
					{
						if (array3[j].Equals(array4[j]))
						{
							return false;
						}
					}
					return true;
				}
			}

			public string wm_key;

			public string map_key;

			public string real_map_target;

			public object AItm;

			public bool show_progress_num;

			public string treasure_img_key;
		}

		public struct QuestMapInfo
		{
			public QuestMapInfo(Color32 _C, bool _tracking, bool _semitransp = false)
			{
				this.C = _C;
				this.tracking = _tracking;
				this.semitransp = _semitransp;
			}

			public QuestMapInfo(QuestTracker.QuestProgress Prog, bool _semitransp = false)
			{
				this.C = Prog.Q.PinColor;
				this.tracking = Prog.tracking;
				this.semitransp = _semitransp;
			}

			public static List<QuestTracker.QuestMapInfo> pushIdentical(List<QuestTracker.QuestMapInfo> A, NelM2DBase M2D, QuestTracker.QuestProgress Prog)
			{
				Color32 pinColor = Prog.Q.PinColor;
				if (A != null)
				{
					bool flag = false;
					QuestTracker.QuestProgress headQuest = M2D.QUEST.getHeadQuest();
					if (headQuest != null && headQuest.tracking)
					{
						flag = Prog != headQuest;
					}
					for (int i = A.Count - 1; i >= 0; i--)
					{
						QuestTracker.QuestMapInfo questMapInfo = A[i];
						if (C32.isEqual(questMapInfo.C, pinColor))
						{
							questMapInfo.tracking = questMapInfo.tracking || Prog.tracking;
							questMapInfo.semitransp = questMapInfo.semitransp && flag;
							A[i] = questMapInfo;
							return A;
						}
					}
					A.Add(new QuestTracker.QuestMapInfo(Prog, flag));
				}
				return A;
			}

			public static List<QuestTracker.QuestMapInfo> pushIdentical(List<QuestTracker.QuestMapInfo> A, List<QuestTracker.QuestMapInfo> ASrc)
			{
				if (A != null && ASrc != null)
				{
					int count = ASrc.Count;
					for (int i = 0; i < count; i++)
					{
						QuestTracker.QuestMapInfo questMapInfo = ASrc[i];
						Color32 c = questMapInfo.C;
						bool flag = true;
						for (int j = A.Count - 1; j >= 0; j--)
						{
							QuestTracker.QuestMapInfo questMapInfo2 = A[j];
							if (C32.isEqual(A[j].C, c))
							{
								questMapInfo2.tracking = questMapInfo2.tracking || questMapInfo.tracking;
								questMapInfo2.semitransp = questMapInfo2.semitransp && questMapInfo.semitransp;
								A[j] = questMapInfo2;
								flag = false;
								break;
							}
						}
						if (flag)
						{
							A.Add(new QuestTracker.QuestMapInfo(questMapInfo.C, questMapInfo.tracking, questMapInfo.semitransp));
						}
					}
				}
				return A;
			}

			public Color32 C;

			public bool tracking;

			public bool semitransp;
		}

		public class QuestDepertureOnMap
		{
			public QuestDepertureOnMap(float _x, float _y, NelM2DBase M2D, QuestTracker.QuestProgress _Prog, Map2d _Mp = null)
			{
				this.x = _x;
				this.y = _y;
				this.Mp = _Mp;
				this.ACol = new List<QuestTracker.QuestMapInfo>(1);
				this.AProg = new List<QuestTracker.QuestProgress>(1);
				this.addProg(M2D, _Prog);
			}

			public void addProg(NelM2DBase M2D, QuestTracker.QuestProgress Prog)
			{
				this.AProg.Add(Prog);
				QuestTracker.QuestMapInfo.pushIdentical(this.ACol, M2D, Prog);
			}

			public float x;

			public float y;

			public Map2d Mp;

			public List<QuestTracker.QuestMapInfo> ACol;

			public List<QuestTracker.QuestProgress> AProg;
		}

		public class QuestDepertureOnWa
		{
			public QuestDepertureOnWa(WAManager.WARecord _Record)
			{
				Vector2 drawCenter = _Record.getDrawCenter();
				this.x = drawCenter.x;
				this.y = drawCenter.y;
				this.Record = _Record;
				this.ACol = new List<QuestTracker.QuestMapInfo>();
				this.AProg = new List<QuestTracker.QuestProgress>();
			}

			public void addProg(NelM2DBase M2D, QuestTracker.QuestProgress Prog)
			{
				this.AProg.Add(Prog);
				QuestTracker.QuestMapInfo.pushIdentical(this.ACol, M2D, Prog);
			}

			public float x;

			public float y;

			public WAManager.WARecord Record;

			public List<QuestTracker.QuestMapInfo> ACol;

			public List<QuestTracker.QuestProgress> AProg;
		}

		public class QuestProgress
		{
			public QuestProgress(QuestTracker.Quest _Q, uint _created, uint _update_count, int _phase = 0)
			{
				this.Q = _Q;
				this.update_count = _update_count;
				this.created = _created;
				this.phase = _phase;
			}

			public int priority
			{
				get
				{
					return (int)((((this.Q.categ & QuestTracker.CATEG.MAIN) != (QuestTracker.CATEG)0) ? 5 : 0) + 10 * this.Q.important + (this.tracking ? 28 : 0));
				}
			}

			public void countUpdate(ref uint cnt)
			{
				if (this.update_count != cnt)
				{
					uint num = cnt;
					cnt = num + 1U;
					this.update_count = num;
				}
			}

			private void allocCollectionArray(int _max = 0)
			{
				if (this.AAcollected_count == null && this.Q.CollectTargetMax() > 0)
				{
					this.AAcollected_count = new ushort[this.Q.CollectTargetMax()][];
				}
				if (this.AAcollected_count != null && this.AAcollected_count.Length < _max)
				{
					Array.Resize<ushort[]>(ref this.AAcollected_count, _max);
				}
			}

			public void clearCollectionCount(int _phase = -1)
			{
				if (this.AAcollected_count != null)
				{
					if (_phase < 0)
					{
						for (int i = this.AAcollected_count.Length - 1; i >= 0; i--)
						{
							ushort[] array = this.AAcollected_count[i];
							if (array != null)
							{
								X.ALL0(array);
							}
						}
						return;
					}
					if (_phase < this.AAcollected_count.Length)
					{
						ushort[] array2 = this.AAcollected_count[_phase];
						if (array2 != null)
						{
							X.ALL0(array2);
						}
					}
				}
			}

			public int addCollectionCount(int phase, NelItem Itm, int count, int grade, bool obtaining = true)
			{
				uint num;
				return this.addCollectionCount(phase, Itm, count, grade, out num, obtaining);
			}

			public int addCollectionCount(int phase, NelItem Itm, int count, int grade, out uint written_row, bool obtaining = true)
			{
				written_row = 0U;
				if (count <= 0 || this.aborted)
				{
					return 0;
				}
				int num = 0;
				bool flag;
				NelItemEntry[] collectTarget = this.Q.getCollectTarget(phase, out flag);
				if (collectTarget == null || (!obtaining && flag))
				{
					return num;
				}
				int num2 = collectTarget.Length;
				for (int i = 0; i < num2; i++)
				{
					NelItemEntry nelItemEntry = collectTarget[i];
					if (nelItemEntry.Data == Itm && (int)nelItemEntry.grade <= grade)
					{
						this.allocCollectionArray(phase + 1);
						ushort[] array = this.AAcollected_count[phase];
						if (array == null)
						{
							array = (this.AAcollected_count[phase] = new ushort[num2]);
						}
						else if (array.Length != num2)
						{
							Array.Resize<ushort>(ref array, num2);
							this.AAcollected_count[phase] = array;
						}
						if ((int)array[i] < collectTarget[i].count)
						{
							int num3 = X.Mn(collectTarget[i].count - (int)array[i], count);
							ushort[] array2 = array;
							int num4 = i;
							array2[num4] += (ushort)num3;
							num += num3;
							count -= num3;
							written_row |= 1U << i;
							if (count <= 0)
							{
								break;
							}
						}
					}
				}
				return num;
			}

			public byte defeatedSummoner(int phase, EnemySummoner Smn, out uint written_row)
			{
				written_row = 0U;
				QuestTracker.SummonerEntry[] summonerEntryTarget = this.Q.getSummonerEntryTarget(phase);
				if (summonerEntryTarget == null)
				{
					return 0;
				}
				int num = summonerEntryTarget.Length;
				int i = 0;
				while (i < num)
				{
					if (!(summonerEntryTarget[i].summoner_key != Smn.key))
					{
						this.allocCollectionArray(phase + 1);
						ushort[] array = this.AAcollected_count[phase];
						if (array == null)
						{
							array = (this.AAcollected_count[phase] = new ushort[num]);
						}
						else if (array.Length != num)
						{
							Array.Resize<ushort>(ref array, num);
							this.AAcollected_count[phase] = array;
						}
						if (array[i] == 0)
						{
							written_row |= 1U << i;
							array[i] = 1;
							return 2;
						}
						return 1;
					}
					else
					{
						i++;
					}
				}
				return 0;
			}

			public bool isQuestTargetItem(int phase, NelItem Itm, int grade)
			{
				bool flag;
				NelItemEntry[] collectTarget = this.Q.getCollectTarget(phase, out flag);
				if (collectTarget == null)
				{
					return false;
				}
				int num = collectTarget.Length;
				for (int i = 0; i < num; i++)
				{
					NelItemEntry nelItemEntry = collectTarget[i];
					if (nelItemEntry.Data == Itm && (grade < 0 || (int)nelItemEntry.grade <= grade))
					{
						return true;
					}
				}
				return false;
			}

			public bool collectionFinished(int phase)
			{
				bool flag;
				NelItemEntry[] collectTarget = this.Q.getCollectTarget(phase, out flag);
				if (collectTarget != null && this.AAcollected_count != null && this.AAcollected_count.Length > phase)
				{
					ushort[] array = this.AAcollected_count[phase];
					int num = collectTarget.Length;
					if (array != null && array.Length >= num)
					{
						for (int i = 0; i < num; i++)
						{
							if (collectTarget[i].count > (int)array[i])
							{
								return false;
							}
						}
						return true;
					}
				}
				QuestTracker.SummonerEntry[] summonerEntryTarget = this.Q.getSummonerEntryTarget(phase);
				if (summonerEntryTarget != null && this.AAcollected_count != null && this.AAcollected_count.Length > phase)
				{
					ushort[] array2 = this.AAcollected_count[phase];
					int num2 = summonerEntryTarget.Length;
					if (array2 != null && array2.Length >= num2)
					{
						for (int j = 0; j < num2; j++)
						{
							if (array2[j] == 0)
							{
								return false;
							}
						}
						return true;
					}
				}
				return false;
			}

			public bool summonerAlreadyDefeated(int phase, string smn)
			{
				QuestTracker.SummonerEntry[] summonerEntryTarget = this.Q.getSummonerEntryTarget(phase);
				if (summonerEntryTarget != null && this.AAcollected_count != null && this.AAcollected_count.Length > phase)
				{
					ushort[] array = this.AAcollected_count[phase];
					int num = summonerEntryTarget.Length;
					if (array != null && array.Length >= num)
					{
						for (int i = 0; i < num; i++)
						{
							if (summonerEntryTarget[i].summoner_key == smn && array[i] > 0)
							{
								return true;
							}
						}
					}
				}
				return false;
			}

			public QuestTracker.QuestDeperture CurrentDepert
			{
				get
				{
					return this.getDepert(this.phase);
				}
			}

			public QuestTracker.QuestDeperture getDepert(int phase)
			{
				QuestTracker.QuestDeperture questDeperture = default(QuestTracker.QuestDeperture);
				if (phase >= this.Q.end_phase)
				{
					return questDeperture;
				}
				for (int i = phase; i >= 0; i--)
				{
					QuestTracker.QuestDeperture depert = this.Q.getDepert(i);
					if (!(depert.wm_key == "^"))
					{
						if (depert.isActiveMap())
						{
							return depert;
						}
						if (i <= 0 || !this.Q.cascade_show_phase(i - 1))
						{
							break;
						}
					}
				}
				return questDeperture;
			}

			public bool isDepertArrived(NelM2DBase M2D, string key)
			{
				if (this.phase >= this.Q.end_phase)
				{
					return false;
				}
				EnemySummonerManager enemySummonerManager = null;
				bool flag = false;
				for (int i = this.phase; i >= 0; i--)
				{
					QuestTracker.QuestDeperture depert = this.Q.getDepert(i);
					if (!(depert.wm_key == "^"))
					{
						if (depert.map_key == key)
						{
							return true;
						}
						if (TX.valid(depert.wm_key) && TX.valid(depert.map_key))
						{
							WmDeperture wmDeperture = new WmDeperture(depert.wm_key, depert.map_key);
							WmPosition posCache = wmDeperture.getPosCache(M2D.WM.GetByTextKey(depert.wm_key));
							if (posCache.valid && posCache.Wmi != null && posCache.Wmi.SrcMap.key == key)
							{
								return true;
							}
						}
						if (depert.AItm is QuestTracker.SummonerEntry[])
						{
							QuestTracker.SummonerEntry[] array = depert.AItm as QuestTracker.SummonerEntry[];
							if (!flag)
							{
								flag = true;
								enemySummonerManager = EnemySummonerManager.GetManager(depert.wm_key);
							}
							if (enemySummonerManager != null)
							{
								foreach (QuestTracker.SummonerEntry summonerEntry in array)
								{
									string text = summonerEntry.summoner_key;
									EnemySummonerManager.SDescription summonerDescription = enemySummonerManager.getSummonerDescription(summonerEntry.summoner_key, true);
									if (summonerDescription.valid)
									{
										text = summonerDescription.map_target(summonerEntry.summoner_key);
									}
									if (key == text)
									{
										return true;
									}
								}
							}
						}
						if (i <= 0 || !this.Q.cascade_show_phase(i - 1))
						{
							break;
						}
					}
				}
				return false;
			}

			public string current_description
			{
				get
				{
					if (this.phase >= this.Q.end_phase)
					{
						return "";
					}
					return this.Q.getDescription(this.phase);
				}
			}

			public static QuestTracker.QuestProgress readBinaryFrom(ByteReader Ba, int vers, ref uint current_created)
			{
				QuestTracker.Quest quest;
				if (!QuestTracker.GetFromBa(Ba, out quest, false))
				{
					return null;
				}
				int num = Ba.readByte();
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				if ((num & 128) != 0)
				{
					num &= -129;
					flag = true;
				}
				if ((num & 64) != 0)
				{
					num &= -65;
					flag2 = true;
				}
				if (vers >= 1)
				{
					flag3 = Ba.readBoolean();
				}
				uint num2 = Ba.readUInt();
				int num3 = Ba.readByte();
				ushort[][] array = null;
				if (num3 > 0)
				{
					array = new ushort[num3][];
					for (int i = 0; i < num3; i++)
					{
						int num4 = Ba.readByte();
						if (num4 > 0)
						{
							ushort[] array2 = (array[i] = new ushort[num4]);
							for (int j = 0; j < num4; j++)
							{
								array2[j] = Ba.readUShort();
							}
						}
					}
				}
				if (quest != null)
				{
					QuestTracker.Quest quest2 = quest;
					uint num5 = current_created;
					current_created = num5 + 1U;
					return new QuestTracker.QuestProgress(quest2, num5, num2, num)
					{
						AAcollected_count = array,
						new_icon = flag,
						tracking = flag2,
						aborted = flag3
					};
				}
				return null;
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				if (this.Q.GQ != null && this.Q.GQ.destructed)
				{
					QuestTracker.writeKeyToBa(Ba, null);
					return;
				}
				QuestTracker.writeKeyToBa(Ba, this.Q);
				Ba.writeByte(this.phase | (this.tracking ? 64 : 0) | (this.new_icon ? 128 : 0));
				Ba.writeBool(this.aborted);
				Ba.writeUInt(this.update_count);
				if (this.AAcollected_count == null)
				{
					Ba.writeByte(0);
					return;
				}
				int num = this.AAcollected_count.Length;
				Ba.writeByte(num);
				for (int i = 0; i < num; i++)
				{
					ushort[] array = this.AAcollected_count[i];
					if (array == null)
					{
						Ba.writeByte(0);
					}
					else
					{
						int num2 = array.Length;
						Ba.writeByte(num2);
						for (int j = 0; j < num2; j++)
						{
							Ba.writeUShort(array[j]);
						}
					}
				}
			}

			public ushort[] getCollectedCount(int phase)
			{
				if (this.AAcollected_count == null || phase >= this.Q.end_phase || phase >= this.AAcollected_count.Length)
				{
					return null;
				}
				return this.AAcollected_count[phase];
			}

			public bool finished
			{
				get
				{
					return this.phase >= this.Q.end_phase;
				}
			}

			public override string ToString()
			{
				return "<QuestProgress> " + this.Q.key + " : " + this.phase.ToString();
			}

			public QuestTracker.Quest Q;

			public int phase;

			public uint created;

			public bool new_icon = true;

			public bool tracking;

			public bool aborted;

			public uint update_count;

			public ushort[][] AAcollected_count;
		}
	}
}
