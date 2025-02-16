using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class ItemStorage
	{
		public bool water_stockable
		{
			get
			{
				return this.ABlink == null;
			}
			set
			{
				if (value == this.water_stockable)
				{
					return;
				}
				if (value)
				{
					this.ABlink = null;
					return;
				}
				this.ABlink = new List<ItemStorage.WaterLink>(16);
			}
		}

		public ItemStorage(string _key, int _max)
		{
			this.key = _key;
			this.OItm = new BDic<NelItem, ItemStorage.ObtainInfo>();
			this.ARow = new List<ItemStorage.IRow>(_max);
			if (ItemStorage.ABMem == null)
			{
				ItemStorage.ABlinkBuf = new List<ItemStorage.WaterLink>(8);
				ItemStorage.ABMem = new List<ItemStorage.WaterLinkMem>(8);
			}
			this.FD_sortItemKeys = new Comparison<ItemStorage.IRow>(this.sortItemKeys);
		}

		public string localized_name
		{
			get
			{
				TX tx = TX.getTX(this.key, true, true, null);
				if (tx == null)
				{
					return this.key;
				}
				return tx.text;
			}
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"<ItemStorage>",
				this.key,
				" (",
				this.getVisibleRowCount().ToString(),
				this.infinit_stockable ? "" : ("/" + this.row_max.ToString()),
				")"
			});
		}

		public ItemStorage clearAllItems(int _max)
		{
			this.row_max = _max;
			this.OItm.Clear();
			this.ARow.Clear();
			if (this.ABlink != null)
			{
				this.ABlink.Clear();
			}
			this.select_row_key = "";
			this.need_reindex_newer = false;
			this.last_newer = 0U;
			this.visible_row_count = -1;
			return this;
		}

		public bool increaseCapacity(int i)
		{
			this.row_max += i;
			return true;
		}

		public void copyFrom(ItemStorage Src, bool override_sort = true)
		{
			this.clearAllItems(Src.row_max);
			this.hide_bottle_max = Src.hide_bottle_max;
			this.row_max = Src.row_max;
			this.select_row_key = Src.select_row_key;
			if (override_sort)
			{
				this.sort_type = Src.sort_type;
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in Src.OItm)
			{
				ItemStorage.ObtainInfo obtainInfo = (this.OItm[keyValuePair.Key] = new ItemStorage.ObtainInfo(keyValuePair.Value));
				this.last_newer = X.Mx(this.last_newer, obtainInfo.newer);
			}
			this.fineRows(false);
		}

		public void readBinaryFrom(ByteReader Ba, bool read_grade = true, bool override_sort = true, bool recipe_reffer_add = false, int vers = 9, bool fix_ver024 = false)
		{
			this.clearAllItems(this.row_max);
			int num = Ba.readInt();
			this.hide_bottle_max = Ba.readInt();
			this.row_max = Ba.readInt();
			bool flag = vers >= 7;
			List<ItemStorage.WaterLinkMem> list = null;
			if (!read_grade)
			{
				this.hide_bottle_max = 0;
				for (int i = 0; i < num; i++)
				{
					string text = Ba.readString("utf-8", false);
					int num2 = (int)Ba.readUShort();
					if (REG.match(text, NelItem.RegItemBottleOld))
					{
						text = REG.leftContext;
					}
					NelItem byId = NelItem.GetById(text, true);
					if (byId != null)
					{
						ItemStorage.ObtainInfo obtainInfo = X.Get<NelItem, ItemStorage.ObtainInfo>(this.OItm, byId);
						if (obtainInfo == null)
						{
							obtainInfo = (this.OItm[byId] = new ItemStorage.ObtainInfo());
						}
						obtainInfo.AddCount(num2, 0);
						obtainInfo.newer = (uint)(num - 1 - i);
					}
				}
				this.last_newer = (uint)(num - 1);
			}
			else
			{
				this.select_row_key = Ba.readPascalString("utf-8", false);
				ItemStorage.SORT_TYPE sort_TYPE = (ItemStorage.SORT_TYPE)Ba.readByte();
				if (override_sort)
				{
					this.sort_type = sort_TYPE;
				}
				for (int j = 0; j < num; j++)
				{
					NelItem byId2;
					if (!flag)
					{
						byId2 = NelItem.GetById(Ba.readString("utf-8", false), true);
					}
					else
					{
						NelItem.readBinaryGetKey(Ba, out byId2, true, fix_ver024);
					}
					ItemStorage.ObtainInfo obtainInfo2 = new ItemStorage.ObtainInfo().readBinaryFrom(Ba);
					bool flag2 = false;
					if (!flag)
					{
						flag2 = Ba.readBoolean();
					}
					if (byId2 != null && obtainInfo2.total > 0)
					{
						this.OItm[byId2] = obtainInfo2;
						this.last_newer = X.Mx(this.last_newer, obtainInfo2.newer);
						if (byId2.RecipeInfo != null && byId2.RecipeInfo.DishInfo != null)
						{
							if (recipe_reffer_add)
							{
								byId2.RecipeInfo.DishInfo.referred++;
							}
							if (!this.water_stockable && flag2)
							{
								if (list == null)
								{
									list = ItemStorage.ABMem;
								}
								list.Add(new ItemStorage.WaterLinkMem(NelItem.LunchBox, byId2));
							}
						}
						if (!flag && byId2.is_water && !this.water_stockable)
						{
							if (list == null)
							{
								list = ItemStorage.ABMem;
							}
							list.Add(new ItemStorage.WaterLinkMem(NelItem.Bottle, byId2));
						}
					}
				}
				if (flag && Ba.bytesAvailable > 0UL)
				{
					num = Ba.readInt();
					if (!this.water_stockable)
					{
						list = ItemStorage.ABMem;
						list.Clear();
						for (int k = 0; k < num; k++)
						{
							list.Add(new ItemStorage.WaterLinkMem(Ba, fix_ver024));
						}
					}
				}
			}
			this.fineRows(list, false);
		}

		public int bytearray_about_bits
		{
			get
			{
				return 12 + this.select_row_key.Length + 1 + 1 + this.OItm.Count * 22 + ((this.ABlink != null) ? this.ABlink.Count : 0) * 8 + 4;
			}
		}

		public ByteArray writeBinaryTo(ByteArray Ba)
		{
			int num = this.OItm.Count;
			Ba.writeInt(num);
			Ba.writeInt(this.hide_bottle_max);
			Ba.writeInt(this.row_max);
			Ba.writePascalString(this.select_row_key, "utf-8");
			Ba.writeByte((int)this.sort_type);
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.OItm)
			{
				ItemStorage.ObtainInfo value = keyValuePair.Value;
				NelItem.writeBinaryItemKey(Ba, keyValuePair.Key);
				keyValuePair.Value.writeBinaryTo(Ba);
			}
			if (this.water_stockable)
			{
				Ba.writeInt(0);
			}
			else
			{
				num = this.ABlink.Count;
				Ba.writeInt(num);
				for (int i = 0; i < num; i++)
				{
					this.ABlink[i].writeBinaryTo(Ba);
				}
			}
			return Ba;
		}

		public void assignObtainCountPreVersion()
		{
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.OItm)
			{
				keyValuePair.Key.addObtainCount(keyValuePair.Value.total);
			}
		}

		public int getItemStockable(NelItem Itm)
		{
			if (this.one_item_line)
			{
				return 1;
			}
			bool flag = this.infinit_stockable;
			if (Itm.is_water && !this.water_stockable)
			{
				flag = false;
			}
			if (!flag)
			{
				return Itm.stock;
			}
			return 9999;
		}

		public int Add(NelItem Itm, int add_count0, int grade, bool add_row = true, bool execute = true)
		{
			ItemStorage.IRow row;
			return this.Add(Itm, add_count0, grade, out row, add_row, execute);
		}

		public int Add(NelItem Itm, int add_count0, int grade, out ItemStorage.IRow Assigned, bool add_row = true, bool execute = true)
		{
			this.getVisibleRowCount();
			ItemStorage.ObtainInfo obtainInfo = null;
			int num = 0;
			int itemStockable = this.getItemStockable(Itm);
			bool flag = false;
			bool flag2 = false;
			Assigned = null;
			ItemStorage.IRow row = ((this.SelectedRow != null) ? this.SelectedRow.getItemRow() : null);
			bool flag3 = (this.SelectedRow != null && this.SelectedRow.isSelected()) || (this.ItemMng != null && this.ItemMng.isUsingState());
			bool flag4 = this.ItemRowContainer != null && this.ItemRowContainer.getValue() >= 0;
			this.getVisibleRowCount();
			while (add_count0 > 0)
			{
				int num2 = X.Mn(add_count0, itemStockable);
				add_count0 -= num2;
				ItemStorage.IRow row2 = null;
				if (this.OItm.TryGetValue(Itm, out obtainInfo))
				{
					int num3;
					if (!this.grade_split)
					{
						num3 = obtainInfo.total;
						row2 = obtainInfo.LastRow;
					}
					else
					{
						num3 = obtainInfo.getCount(grade);
						row2 = this.getIRowForGrade(Itm, obtainInfo, grade, null);
					}
					if (row2 != null)
					{
						int num4 = X.Mn(X.Mx(1, X.IntC((float)num3 / (float)itemStockable)) * itemStockable - num3, num2);
						if (num4 > 0)
						{
							num2 -= num4;
							num += num4;
							if (execute)
							{
								row2.AddCount(num4, grade);
								Assigned = row2;
								row = row2;
								if (!this.do_not_input_newer && Itm != this.Newer_Memory_Item && obtainInfo.newer < this.last_newer)
								{
									ItemStorage.ObtainInfo obtainInfo2 = obtainInfo;
									uint num5 = this.last_newer + 1U;
									this.last_newer = num5;
									obtainInfo2.newer = num5;
									this.need_reindex_newer = true;
								}
							}
						}
						if (num2 == 0 || !Itm.nest_multiple)
						{
							continue;
						}
					}
				}
				if (add_row)
				{
					int num6 = -1;
					ItemStorage.ROWHID rowhid = ItemStorage.ROWHID.VISIBLE;
					bool flag5 = this.infinit_stockable;
					if (!this.water_stockable)
					{
						bool flag6;
						if (Itm.isWLinkUser(out flag6))
						{
							num6 = this.checkEmptyConnectableWLink(Itm);
						}
						if (num6 < 0 && (flag6 || (!flag5 && this.getVisibleRowCount() >= this.row_max)))
						{
							num6 = this.getConnectableWLinkTarget(Itm, execute);
						}
						if (flag6 && num6 < 0)
						{
							break;
						}
						if (num6 < 0 && Itm == NelItem.Bottle && this.ObtFakeBottleHolder != null && this.ObtFakeBottleHolder.total > 0)
						{
							rowhid = ItemStorage.ROWHID.H_BOTTLE;
						}
					}
					if (!flag5 && this.getVisibleRowCount() >= this.row_max && num6 < 0 && rowhid == ItemStorage.ROWHID.VISIBLE)
					{
						break;
					}
					if (obtainInfo == null)
					{
						obtainInfo = new ItemStorage.ObtainInfo();
						if (execute)
						{
							this.OItm[Itm] = obtainInfo;
							if (!this.do_not_input_newer)
							{
								if (Itm == this.Newer_Memory_Item)
								{
									obtainInfo.newer = this.newer_memory;
								}
								else
								{
									ItemStorage.ObtainInfo obtainInfo3 = obtainInfo;
									uint num5 = this.last_newer + 1U;
									this.last_newer = num5;
									obtainInfo3.newer = num5;
								}
							}
						}
					}
					int num7 = num2;
					num += num7;
					bool flag7 = false;
					ItemStorage.IRow row3 = null;
					if (execute)
					{
						row2 = row2 ?? obtainInfo.LastRow;
						row3 = new ItemStorage.IRow(Itm, obtainInfo, true).AddCount(num7, grade);
						if (this.grade_split)
						{
							row3.splitted_grade = (byte)grade;
						}
						int num8 = ((row2 != null && row2.index >= 0) ? X.Mn(this.ARow.Count, row2.index + 1) : this.ARow.Count);
						if (num8 < this.ARow.Count)
						{
							flag = true;
						}
						if (num6 >= 0)
						{
							num8 = this.ARow.Count;
						}
						else if (rowhid > ItemStorage.ROWHID.VISIBLE)
						{
							row3.hidden = rowhid;
							if (rowhid == ItemStorage.ROWHID.H_BOTTLE)
							{
								this.addHiddenBottleHolder(false, false);
								flag7 = (flag = true);
							}
						}
						row3.index = num8;
						if (num8 >= this.ARow.Count)
						{
							this.ARow.Add(row3);
						}
						else
						{
							this.ARow.Insert(num8, row3);
						}
						flag2 = true;
						Assigned = row3;
						if (num6 >= 0)
						{
							ItemStorage.WaterLink waterLink = this.ABlink[num6];
							if ((waterLink.Outer != null && waterLink.Outer.hidden > ItemStorage.ROWHID.VISIBLE) || (waterLink.Inner != null && waterLink.Inner.hidden > ItemStorage.ROWHID.VISIBLE))
							{
								this.visible_row_count = -1;
							}
							ItemStorage.IRow row4;
							waterLink.Connect(row3, num6, out row4, this);
							flag7 = (flag = true);
							this.ABlink[num6] = waterLink;
							if (row4 != null)
							{
								row3 = row4;
							}
						}
						else
						{
							this.visible_row_count = -1;
						}
						if (this.SelectedRow == null || !flag3 || this.select_row_key == Itm.key)
						{
							row = row3;
							this.select_row_key = row3.Data.key;
						}
					}
					if (flag && flag7)
					{
						this.reindexWholeIRows();
						flag = false;
					}
					if (row3 != null && this.fnRowAddition != null)
					{
						this.fnRowAddition(row3, this.ARow);
					}
				}
			}
			if (flag)
			{
				this.reindexWholeIRows();
			}
			if (this.Ds != null && this.ItemMng != null)
			{
				if (!this.ItemMng.auto_select_on_adding_row && this.SelectedRow != null)
				{
					if (row != null)
					{
						row.fineRowCount(this);
					}
					row = this.SelectedRow.getItemRow();
				}
				if (flag2)
				{
					this.fineRowButtonLink(row);
				}
				else
				{
					if (row != null && (this.SelectedRow == null || row != this.SelectedRow.getItemRow()))
					{
						this.reselectTargetRow(Itm, row, flag4);
					}
					if (this.SelectedRow != null)
					{
						this.SelectedRow.RowSkin.fineCount(true);
					}
				}
			}
			return num;
		}

		public bool Reduce(NelItem Itm, int count, int grade, bool fine_row = true)
		{
			ItemStorage.ObtainInfo obtainInfo;
			if (!this.OItm.TryGetValue(Itm, out obtainInfo) || count <= 0)
			{
				return false;
			}
			int itemStockable = this.getItemStockable(Itm);
			bool flag = this.ItemRowContainer != null && this.ItemRowContainer.getValue() >= 0;
			bool flag2 = false;
			bool flag3;
			if (grade >= 0)
			{
				int num;
				ItemStorage.IRow row;
				if (!this.grade_split)
				{
					num = obtainInfo.total;
					row = obtainInfo.LastRow;
				}
				else
				{
					if (grade == -1)
					{
						grade = obtainInfo.top_grade;
					}
					num = obtainInfo.getCount(grade);
					row = this.getIRowForGrade(Itm, obtainInfo, grade, null);
				}
				int num2 = X.IntC((float)num / (float)itemStockable);
				if (row != null)
				{
					row.ReduceCount(count, grade);
				}
				int num3;
				if (!this.grade_split)
				{
					num3 = X.IntC((float)obtainInfo.total / (float)itemStockable);
				}
				else
				{
					num3 = X.IntC((float)obtainInfo.getCount(grade) / (float)itemStockable);
				}
				if (num3 == 0 && !this.auto_splice_zero_row)
				{
					num3 = 1;
				}
				flag3 = num3 < num2;
				if (this.Ds != null && this.SelectedRow != null && row != null)
				{
					if (row != this.SelectedRow.getItemRow())
					{
						this.reselectTargetRow(Itm, row, flag);
					}
					if (this.SelectedRow != null)
					{
						this.SelectedRow.RowSkin.fineCount(true);
					}
				}
				flag2 = true;
			}
			else
			{
				int num4 = 0;
				if (!this.grade_split)
				{
					num4 = X.IntC((float)obtainInfo.total / (float)itemStockable);
				}
				for (int i = 4; i >= 0; i--)
				{
					int num5 = X.Mn(count, obtainInfo.getCount(i));
					if (num5 != 0)
					{
						int num6 = 0;
						count -= num5;
						ItemStorage.IRow row2;
						if (!this.grade_split)
						{
							row2 = obtainInfo.LastRow;
						}
						else
						{
							row2 = this.getIRowForGrade(Itm, obtainInfo, i, null);
							num6 = X.IntC((float)obtainInfo.getCount(i) / (float)itemStockable);
						}
						row2.ReduceCount(num5, i);
						if (this.grade_split)
						{
							num4 += X.IntC((float)obtainInfo.getCount(i) / (float)itemStockable) - num6;
						}
						if (count <= 0)
						{
							break;
						}
					}
				}
				if (!this.grade_split)
				{
					flag3 = X.IntC((float)obtainInfo.total / (float)itemStockable) < num4;
				}
				else
				{
					flag3 = num4 < 0;
				}
			}
			bool flag4 = false;
			if (this.auto_splice_zero_row && flag3)
			{
				if (obtainInfo.total <= 0)
				{
					this.OItm.Remove(Itm);
				}
				this.visible_row_count = -1;
				flag4 = (this.need_reindex_newer = true);
				if (fine_row)
				{
					this.fineRows(false);
					return true;
				}
			}
			if (this.Ds != null && !flag2)
			{
				using (BList<aBtnItemRow> blist = this.PopGetItemRowBtnsFor(Itm))
				{
					int count2 = blist.Count;
					for (int j = 0; j < count2; j++)
					{
						blist[j].RowSkin.fineCount(true);
					}
				}
			}
			return flag4;
		}

		public void ReduceOneRow(NelItem Itm, int center_grade = 0, bool fine_row = true, List<int> AOut = null)
		{
			ItemStorage.ObtainInfo obtainInfo;
			if (!this.OItm.TryGetValue(Itm, out obtainInfo))
			{
				return;
			}
			int num = this.getItemStockable(Itm);
			int num2 = obtainInfo.total % num;
			if (num2 > 0)
			{
				num = num2;
			}
			if (AOut != null)
			{
				for (int i = 0; i < 5; i++)
				{
					if (AOut.Count <= i)
					{
						AOut.Add(0);
					}
					else
					{
						AOut[i] = 0;
					}
				}
			}
			int num3 = 0;
			while (num3 < 2 && num > 0)
			{
				int num4 = center_grade + num3;
				int num5 = 0;
				while (num5 < 5 && X.BTW(0f, (float)num4, 5f))
				{
					int num6 = X.Mn(num, obtainInfo.getCount(num4));
					this.Reduce(Itm, num6, num4, false);
					if (AOut != null)
					{
						int num7 = num4;
						AOut[num7] += num6;
					}
					num -= num6;
					if (num <= 0)
					{
						break;
					}
					num4 += X.MPF(num3 == 1);
					num5++;
				}
				num3++;
			}
			if (fine_row)
			{
				this.fineRows(false);
			}
		}

		public bool Reduce(List<NelItemEntry> AEnt)
		{
			int count = AEnt.Count;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				NelItemEntry nelItemEntry = AEnt[i];
				if (nelItemEntry.count >= 0)
				{
					flag = this.Reduce(nelItemEntry.Data, nelItemEntry.count, (int)nelItemEntry.grade, false) || flag;
				}
			}
			if (flag)
			{
				this.fineRows(false);
			}
			return flag;
		}

		public void changeGradeForPrecious(NelItem Itm, int grade)
		{
			ItemStorage.ObtainInfo obtainInfo;
			if (Itm.is_precious && this.OItm.TryGetValue(Itm, out obtainInfo))
			{
				obtainInfo.changeGradeForPrecious(grade, 1);
			}
		}

		public ItemStorage fineRows(bool splice_zero_row = false)
		{
			return this.fineRows(null, splice_zero_row);
		}

		private ItemStorage fineRows(List<ItemStorage.WaterLinkMem> ABMem, bool splice_zero_row = false)
		{
			if (this.auto_splice_zero_row)
			{
				splice_zero_row = true;
			}
			if (ABMem == null)
			{
				ABMem = ItemStorage.ABMem;
				ABMem.Clear();
				if (!this.water_stockable)
				{
					int num = this.ABlink.Count;
					for (int i = 0; i < num; i++)
					{
						ABMem.Add(new ItemStorage.WaterLinkMem(this.ABlink[i]));
					}
				}
			}
			if (splice_zero_row)
			{
				using (BList<NelItem> blist = ListBuffer<NelItem>.Pop(0))
				{
					X.objKeys<NelItem, ItemStorage.ObtainInfo>(this.OItm, blist);
					int num = blist.Count;
					for (int j = 0; j < num; j++)
					{
						NelItem nelItem = blist[j];
						ItemStorage.ObtainInfo obtainInfo = this.OItm[nelItem];
						if (obtainInfo.total <= 0)
						{
							if (obtainInfo.LastRow != null)
							{
								obtainInfo.LastRow.ALink = null;
								obtainInfo.LastRow = null;
							}
							this.OItm.Remove(nelItem);
						}
						else
						{
							obtainInfo.grade_touched = obtainInfo.getGradeUsingBit();
						}
					}
				}
			}
			ItemStorage.IRow row = ((this.SelectedRow != null) ? this.SelectedRow.getItemRow() : null);
			NelItem nelItem2 = ((row != null) ? row.Data : null);
			int num2 = ((this.grade_split && row != null) ? ((int)row.splitted_grade) : (-1));
			int num3 = 0;
			if (this.need_reindex_newer)
			{
				this.fineNewerIndex();
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.OItm)
			{
				ItemStorage.ObtainInfo value = keyValuePair.Value;
				NelItem nelItem3 = keyValuePair.Key;
				bool flag = nelItem3 == nelItem2;
				value.LastRow = null;
				List<ItemStorage.IRow> list = new List<ItemStorage.IRow>(1);
				int itemStockable = this.getItemStockable(nelItem3);
				int num4 = ((!this.grade_split) ? (-1) : 0);
				int num5 = ((!this.grade_split) ? 0 : 5);
				for (int k = num4; k < num5; k++)
				{
					int num6 = ((k == -1) ? value.total : value.getCount(k));
					while (num6 > 0 || (num6 == 0 && !splice_zero_row && (k == -1 || (value.grade_touched & (1U << k)) != 0U)))
					{
						value.grade_touched |= 1U << k;
						int num7 = X.Mn(num6, itemStockable);
						num6 -= num7;
						ItemStorage.IRow row2 = this.addRowInner(ref num3, keyValuePair.Key, value, num7, list, k);
						if (flag && (k < 0 || num2 == k))
						{
							row = row2;
						}
						if (num6 <= 0)
						{
							break;
						}
					}
				}
			}
			bool flag2 = false;
			this.visible_row_count = -1;
			if (this.ObtFakeBottleHolder != null)
			{
				this.ObtFakeBottleHolder.clear();
			}
			if (!this.water_stockable)
			{
				this.ABlink.Clear();
				int num8 = this.hide_bottle_max;
				for (int l = ABMem.Count - 1; l >= 0; l--)
				{
					ItemStorage.WaterLinkMem waterLinkMem = ABMem[l];
					ItemStorage.IRow row3;
					ItemStorage.IRow row4;
					if (this.FindRowsForMem(waterLinkMem, out row3, out row4, num3))
					{
						ABMem.RemoveAt(l);
						this.ABlink.Add(new ItemStorage.WaterLink(row3, row4, this.ABlink.Count));
					}
				}
				int num9 = num3 - 1;
				while (num9 >= 0 && num8 > 0)
				{
					ItemStorage.IRow row5 = this.ARow[num9];
					if (row5.blink_index < 0 && row5.hidden == ItemStorage.ROWHID.VISIBLE && row5.Data == NelItem.Bottle)
					{
						num8--;
						row5.hidden = ItemStorage.ROWHID.H_BOTTLE;
					}
					num9--;
				}
				if (num8 > 0)
				{
					if (this.ObtFakeBottleHolder == null)
					{
						this.ObtFakeBottleHolder = new ItemStorage.ObtainInfo();
					}
					this.ObtFakeBottleHolder.clear();
					this.ObtFakeBottleHolder.AddCount(num8, 0);
					while (--num8 >= 0)
					{
						ItemStorage.IRow row6 = this.addRowInner(ref num3, NelItem.HolderBottle, this.ObtFakeBottleHolder, 1, null, -1);
						row6.hidden = ItemStorage.ROWHID.H_BOTTLE;
						if (nelItem2 == NelItem.HolderBottle && row == null)
						{
							row = row6;
						}
					}
				}
				ABMem.Clear();
			}
			if (num3 < this.ARow.Count)
			{
				this.ARow.RemoveRange(num3, this.ARow.Count - num3);
			}
			if (flag2)
			{
				this.reindexWholeIRows();
			}
			if (this.fnRowAddition != null && this.fnRowAddition(null, this.ARow))
			{
				this.reindexWholeIRows();
			}
			this.fineRowButtonLink(row);
			return this;
		}

		private void addHiddenBottleHolder(bool adding = true, bool do_reindex = false)
		{
			if (this.ObtFakeBottleHolder == null)
			{
				this.ObtFakeBottleHolder = new ItemStorage.ObtainInfo();
			}
			if (adding)
			{
				int count = this.ARow.Count;
				this.addRowInner(ref count, NelItem.HolderBottle, this.ObtFakeBottleHolder, 1, null, -1).hidden = ItemStorage.ROWHID.H_BOTTLE;
				return;
			}
			if (this.ObtFakeBottleHolder.total <= 0)
			{
				return;
			}
			this.ObtFakeBottleHolder.ReduceCount(1, 0);
			int i = this.ARow.Count - 1;
			while (i >= 0)
			{
				ItemStorage.IRow row = this.ARow[i];
				if (row.is_fake_row && row.Data == NelItem.HolderBottle)
				{
					this.ARow.RemoveAt(i);
					if (do_reindex)
					{
						this.reindexWholeIRows();
						return;
					}
					break;
				}
				else
				{
					i--;
				}
			}
		}

		private void reindexWholeIRows()
		{
			for (int i = this.ARow.Count - 1; i >= 0; i--)
			{
				this.ARow[i].index = i;
			}
		}

		private void fineRowButtonLink(ItemStorage.IRow SelectTarget = null)
		{
			if (this.ItemRowContainer != null)
			{
				string text = this.select_row_key;
				int num = ((this.SelectedRow != null) ? this.ItemRowContainer.getIndex(this.SelectedRow) : (-1));
				bool flag = this.SelectedRow != null && this.SelectedRow.isChecked();
				bool flag2 = this.SelectedRow != null && this.SelectedRow.isSelected();
				if (this.ItemMng != null)
				{
					this.ItemMng.blurDescTarget(false);
				}
				for (int i = this.ItemRowContainer.Length - 1; i >= 0; i--)
				{
					this.ItemRowContainer.Get(i).transform.SetParent(this.ItemMng.TrsEvacuateTo, false);
				}
				this.ItemRowContainer.RemakeT<aBtnItemRow>(null, "");
				this.fineNaviLoop();
				this.ItemRowContainer.wholeRun();
				ItemStorage.ObtainInfo obtainInfo = (TX.valid(this.select_row_key) ? X.Get<NelItem, ItemStorage.ObtainInfo>(this.OItm, NelItem.GetById(this.select_row_key, false)) : null);
				int length = this.ItemRowContainer.Length;
				this.SelectedRow = null;
				this.select_row_key = "";
				if (length == 0)
				{
					return;
				}
				for (int j = length - 1; j >= 0; j--)
				{
					aBtnItemRow aBtnItemRow = this.ItemRowContainer.Get(j) as aBtnItemRow;
					if (aBtnItemRow.getItemRow().Info == obtainInfo)
					{
						if (this.SelectedRow == null)
						{
							this.SelectedRow = aBtnItemRow;
						}
						if (SelectTarget == null || SelectTarget == aBtnItemRow.getItemRow())
						{
							this.SelectedRow = aBtnItemRow;
							break;
						}
					}
				}
				if (this.SelectedRow == null)
				{
					if (num >= 0)
					{
						this.SelectedRow = this.ItemRowContainer.Get(X.MMX(0, num, length - 1)) as aBtnItemRow;
					}
					else
					{
						this.SelectedRow = this.ItemRowContainer.Get(0) as aBtnItemRow;
						this.reselectTargetRow(this.SelectedRow.getItemData(), this.SelectedRow.getItemInfo().LastRow, false);
					}
				}
				if (this.SelectedRow != null)
				{
					if (flag && text == this.SelectedRow.getItemData().key)
					{
						this.ItemRowContainer.setValue(this.ItemRowContainer.getIndex(this.SelectedRow), true);
					}
					else
					{
						this.ItemRowContainer.setValue(-1, true);
					}
					if (flag2)
					{
						this.SelectedRow.Select(true);
					}
					this.select_row_key = this.SelectedRow.getItemData().key;
				}
				if (this.auto_update_topright_counter && this.ItemMng != null)
				{
					this.ItemMng.fineTopRightCounter();
				}
			}
		}

		private ItemStorage.IRow addRowInner(ref int row_i, NelItem Itm, ItemStorage.ObtainInfo Obt, int count, List<ItemStorage.IRow> ARowForThisObt, int splitted_grade)
		{
			while (row_i >= this.ARow.Count)
			{
				this.ARow.Add(new ItemStorage.IRow(null, null, true));
			}
			ItemStorage.IRow row = this.ARow[row_i];
			row.ALink = ARowForThisObt;
			if (ARowForThisObt != null)
			{
				ARowForThisObt.Add(row);
			}
			if (splitted_grade >= 0)
			{
				row.splitted_grade = (byte)splitted_grade;
			}
			row.Data = Itm;
			row.Info = Obt;
			row.total = count;
			row.blink_index = -1;
			row.hidden = ItemStorage.ROWHID.VISIBLE;
			ItemStorage.IRow row2 = row;
			int num = row_i;
			row_i = num + 1;
			row2.index = num;
			Obt.LastRow = row;
			return row;
		}

		public void fineNewerIndex()
		{
			if (this.Newer_Memory_Item != null)
			{
				return;
			}
			ItemStorage.ObtainInfo[] array = new ItemStorage.ObtainInfo[this.OItm.Count];
			this.OItm.Values.CopyTo(array, 0);
			new SORT<ItemStorage.ObtainInfo>(null).qSort(array, new Comparison<ItemStorage.ObtainInfo>(this.fnSortObInfoNewer), -1);
			for (int i = array.Length - 1; i >= 0; i--)
			{
				array[i].newer = (uint)i;
			}
			this.last_newer = (uint)(array.Length - 1);
			this.need_reindex_newer = false;
		}

		private int fnSortObInfoNewer(ItemStorage.ObtainInfo Oa, ItemStorage.ObtainInfo Ob)
		{
			if (Oa.newer == Ob.newer)
			{
				return 0;
			}
			if (Oa.newer >= Ob.newer)
			{
				return 1;
			}
			return -1;
		}

		public void MemoryNewer(NelItem Itm)
		{
			bool flag = this.Newer_Memory_Item != null;
			this.Newer_Memory_Item = Itm;
			if (Itm != null)
			{
				ItemStorage.ObtainInfo info = this.getInfo(this.Newer_Memory_Item);
				if (info != null)
				{
					this.newer_memory = info.newer;
				}
				else
				{
					this.Newer_Memory_Item = null;
				}
			}
			if (flag && this.Newer_Memory_Item == null && this.need_reindex_newer)
			{
				this.fineNewerIndex();
			}
		}

		public void copyNewer(ItemStorage Src, bool fine_newer = true)
		{
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in Src.OItm)
			{
				ItemStorage.ObtainInfo info = this.getInfo(keyValuePair.Key);
				if (info != null)
				{
					info.newer = (this.do_not_input_newer ? X.Mx(keyValuePair.Value.newer, info.newer) : keyValuePair.Value.newer);
				}
			}
			if (fine_newer)
			{
				this.fineNewerIndex();
			}
		}

		public ItemStorage.IRow getIRowForGrade(NelItem Itm, ItemStorage.ObtainInfo Info, int grade, List<ItemStorage.IRow> _ARow = null)
		{
			if (Info == null)
			{
				return null;
			}
			if (_ARow == null)
			{
				_ARow = Info.LastRow.ALink;
			}
			if (_ARow == null)
			{
				ItemStorage.IRow lastRow = Info.LastRow;
				if ((int)lastRow.splitted_grade == grade)
				{
					return lastRow;
				}
			}
			for (int i = _ARow.Count - 1; i >= 0; i--)
			{
				ItemStorage.IRow row = _ARow[i];
				if ((int)row.splitted_grade == grade)
				{
					return row;
				}
			}
			return null;
		}

		private int getWLinkedIndex(NelItem Itm)
		{
			if (this.water_stockable)
			{
				return -1;
			}
			for (int i = this.ABlink.Count - 1; i >= 0; i--)
			{
				if (this.ABlink[i].Is(Itm))
				{
					return i;
				}
			}
			return -1;
		}

		public bool isLinked(ItemStorage.IRow Row, out ItemStorage.IRow Another)
		{
			Another = null;
			if (this.water_stockable)
			{
				return false;
			}
			if (Row.blink_index >= 0 && Row.blink_index < this.ABlink.Count)
			{
				Another = this.ABlink[Row.blink_index].getAnother(Row);
				return true;
			}
			return false;
		}

		public bool isLinked(ItemStorage.IRow Row, out ItemStorage.IRow Outer, out ItemStorage.IRow Inner)
		{
			ItemStorage.IRow row;
			Inner = (row = null);
			Outer = row;
			if (this.water_stockable)
			{
				return false;
			}
			if (Row.blink_index >= 0 && Row.blink_index < this.ABlink.Count)
			{
				ItemStorage.IRow another = this.ABlink[Row.blink_index].getAnother(Row);
				if (another == null)
				{
					return false;
				}
				if (Row.Data.connectableWLinkOuter(another.Data))
				{
					ItemStorage.IRow row2 = another;
					Outer = Row;
					Inner = row2;
					return true;
				}
				if (another.Data.connectableWLinkOuter(Row.Data))
				{
					ItemStorage.IRow row2 = another;
					Outer = row2;
					Inner = Row;
					return true;
				}
			}
			return false;
		}

		public void addWLink(ItemStorage.IRow Outer, ItemStorage.IRow Inner, bool fine_row = true)
		{
			if (this.water_stockable || (Outer == null && Inner == null))
			{
				return;
			}
			if (Outer != null)
			{
				this.removeWLink(Outer);
			}
			if (Inner != null)
			{
				this.removeWLink(Inner);
			}
			this.ABlink.Add(new ItemStorage.WaterLink(Outer, Inner, this.ABlink.Count));
			if (fine_row)
			{
				this.fineRows(false);
			}
		}

		private void removeWLink(int index)
		{
			if (this.water_stockable)
			{
				return;
			}
			if (index >= 0 && index < this.ABlink.Count)
			{
				ItemStorage.WaterLink waterLink = this.ABlink[index];
				waterLink.Dispose();
				this.ABlink[index] = waterLink;
			}
		}

		public void removeWLink(ItemStorage.IRow Row)
		{
			if (this.water_stockable || Row == null)
			{
				return;
			}
			this.removeWLink(Row.blink_index);
		}

		public void removeWLink(NelItem Itm, int remove_cnt = -1)
		{
			if (this.water_stockable || Itm == null || remove_cnt == 0)
			{
				return;
			}
			ItemStorage.ObtainInfo info = this.getInfo(Itm);
			if (info == null || info.LastRow == null)
			{
				return;
			}
			int count = info.LastRow.ALink.Count;
			for (int i = 0; i < count; i++)
			{
				ItemStorage.IRow row = info.LastRow.ALink[i];
				if (row.has_wlink)
				{
					this.removeWLink(row);
					if (--remove_cnt == 0)
					{
						break;
					}
				}
			}
		}

		private int checkEmptyConnectableWLink(NelItem Itm)
		{
			if (this.water_stockable)
			{
				return -1;
			}
			for (int i = this.ABlink.Count - 1; i >= 0; i--)
			{
				ItemStorage.WaterLink waterLink = this.ABlink[i];
				if (!waterLink.hasEmpty())
				{
					return -1;
				}
				if (waterLink.emptyConnectable(Itm))
				{
					return i;
				}
			}
			return -1;
		}

		private int getConnectableWLinkTarget(NelItem Itm, bool execute = true)
		{
			this.getVisibleRowCount();
			int count = this.ARow.Count;
			for (int i = (execute ? 0 : 1); i < 2; i++)
			{
				int j = 0;
				while (j < count)
				{
					ItemStorage.IRow row = this.ARow[j];
					bool flag;
					if (row.blink_index < 0 && ((i != 0 && this.visible_row_count < this.row_max) || row.hidden <= ItemStorage.ROWHID.VISIBLE) && row.Data.connectableWLink(Itm, out flag))
					{
						if (!execute)
						{
							return this.ABlink.Count;
						}
						int count2 = this.ABlink.Count;
						this.ABlink.Add(flag ? new ItemStorage.WaterLink(row, null, count2) : new ItemStorage.WaterLink(null, row, count2));
						return count2;
					}
					else
					{
						j++;
					}
				}
			}
			return -1;
		}

		private bool FindRowsForMem(ItemStorage.WaterLinkMem Mem, out ItemStorage.IRow Outer, out ItemStorage.IRow Inner, int row_maxi)
		{
			ItemStorage.IRow row;
			Inner = (row = null);
			Outer = row;
			for (int i = 0; i < row_maxi; i++)
			{
				ItemStorage.IRow row2 = this.ARow[i];
				if (Outer == null && row2 != Inner && Mem.Outer == row2.Data && row2.blink_index < 0)
				{
					Outer = row2;
					if (Outer != null && Inner != null)
					{
						return true;
					}
				}
				if (Inner == null && row2 != Outer && Mem.Inner == row2.Data && row2.blink_index < 0)
				{
					Inner = row2;
					if (Outer != null && Inner != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void fineSpecificRow(NelItem Itm)
		{
			if (this.ItemMng != null)
			{
				int length = this.ItemRowContainer.Length;
				for (int i = 0; i < length; i++)
				{
					aBtnItemRow aBtnItemRow = this.ItemRowContainer.Get(i) as aBtnItemRow;
					if (aBtnItemRow.getItemData() == Itm)
					{
						aBtnItemRow.Fine(false);
					}
				}
			}
		}

		public MeshDrawer drawTempBottle(MeshDrawer Md, int submesh_icon, int submesh_chr, float dx, float dy, bool update = false, uint color = 4283780170U)
		{
			Md.Col = C32.d2c(color);
			if (submesh_icon != -1)
			{
				Md.chooseSubMesh(submesh_icon, false, false);
			}
			Md.RotaPF(dx - 18f, dy, 1f, 1f, 0f, MTR.AItemIcon[4], false, false, false, uint.MaxValue, false, 0);
			if (submesh_chr != -1 && submesh_chr != submesh_icon)
			{
				Md.chooseSubMesh(submesh_chr, false, false);
			}
			STB stb = TX.PopBld(null, 0);
			stb.Add(this.getEmptyBottleCount()).Add("/").Add(this.getCount(NelItem.Bottle, -1));
			MTRX.ChrL.DrawStringTo(Md, stb, dx + 11f, dy, ALIGN.CENTER, ALIGNY.TOP, false, 0f, 0f, null);
			TX.ReleaseBld(stb);
			if (update)
			{
				Md.updateForMeshRenderer(false);
			}
			return Md;
		}

		public bool isMngEffectConfusion()
		{
			return this.ItemMng != null && this.ItemMng.effect_confusion;
		}

		public aBtnItemRow createRowsTo(UiItemManageBox ItemMng, Designer Ds, FnBtnBindings _fnClick, int stencil_ref = -1, string item_row_skin = "normal")
		{
			this.ItemMng = ItemMng;
			this.Ds = Ds;
			this.MemoryNewer(null);
			NelItem.fineNameLocalizedWhole();
			Ds.margin_in_lr = (Ds.margin_in_tb = 0f);
			Ds.item_margin_y_px = 1f;
			Ds.item_margin_x_px = 2f;
			int num = 6;
			Ds.use_button_connection = false;
			BtnContainer<aBtn> btnContainer = null;
			this.PSortDesc = null;
			bool flag = false;
			if (this.sort_button_bits != 0)
			{
				using (BList<string> blist = ListBuffer<string>.Pop(num))
				{
					for (int i = 0; i < num; i++)
					{
						if ((this.sort_button_bits & (1 << i)) != 0)
						{
							List<string> list = blist;
							string text = "sort_";
							ItemStorage.SORT_TYPE sort_TYPE = (ItemStorage.SORT_TYPE)i;
							list.Add(text + sort_TYPE.ToString().ToLower());
						}
					}
					DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
					dsnDataButtonMulti.name = "__sort_container";
					dsnDataButtonMulti.titles = blist.ToArray();
					dsnDataButtonMulti.skin = "mini_sort";
					dsnDataButtonMulti.w = 28f;
					dsnDataButtonMulti.h = 28f;
					dsnDataButtonMulti.clms = blist.Count;
					dsnDataButtonMulti.margin_w = 1f;
					dsnDataButtonMulti.margin_h = 0f;
					dsnDataButtonMulti.unselectable = 2;
					dsnDataButtonMulti.fnClick = new FnBtnBindings(this.fnSortButtonClicked);
					dsnDataButtonMulti.fnMaking = delegate(BtnContainer<aBtn> BCon, aBtn B)
					{
						IN.setZ(B.transform, -0.1f);
						return true;
					};
					btnContainer = Ds.addButtonMultiT<aBtnNel>(dsnDataButtonMulti);
				}
				Ds.XSh(10f);
				float num2 = Ds.use_w - 10f - 6f;
				this.PSortDesc = Ds.XSh(10f).addP(new DsnDataP("", false)
				{
					TxCol = C32.d2c(4283780170U),
					text = "<key sort/>",
					size = 12f,
					swidth = num2 - ((ItemMng != null && ItemMng.use_topright_counter) ? ItemMng.topright_desc_width : 0f),
					sheight = 26f,
					html = true,
					alignx = ALIGN.LEFT,
					aligny = ALIGNY.BOTTOM,
					text_auto_wrap = false,
					text_auto_condense = true
				}, false);
				flag = true;
			}
			if (ItemMng != null && ItemMng.use_topright_counter)
			{
				this.PCounter = Ds.addP(new DsnDataP("", false)
				{
					TxCol = C32.d2c(4283780170U),
					text = ItemMng.getDescStr(UiItemManageBox.DESC_ROW.TOPRIGHT_COUNTER),
					text_auto_condense = true,
					swidth = ((this.PSortDesc == null) ? Ds.use_w : X.MMX(0f, ItemMng.topright_desc_width - Ds.item_margin_x_px - 4f, Ds.use_w)),
					sheight = 26f,
					lineSpacing = 0.9f,
					html = true,
					alignx = ALIGN.RIGHT,
					aligny = ALIGNY.BOTTOM
				}, false);
				flag = true;
			}
			else
			{
				this.PCounter = null;
			}
			if (btnContainer != null)
			{
				this.fineSortButton(btnContainer, false);
			}
			if (flag)
			{
				Ds.Br();
			}
			Ds.addHr(new DsnDataHr
			{
				margin_t = 0f,
				margin_b = 0f,
				swidth = Ds.use_w - 2f,
				Col = C32.d2c(4283780170U),
				draw_width_rate = 1f
			});
			Ds.Br();
			this.ItemRowContainer = Ds.addRadioT<aBtnItemRow>(new DsnDataRadio
			{
				name = "__item_container",
				def = -1,
				skin = item_row_skin,
				fnGenerateKeys = new FnGenerateRemakeKeys(this.getItemKeys),
				value_return_name = true,
				w = Ds.use_w - 10f - 16f,
				h = ItemMng.row_height,
				scale = 1f,
				clms = 1,
				margin_w = 0,
				margin_h = 0,
				navi_loop = 0,
				all_function_same = true,
				fnMaking = new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnMakingItemRow),
				fnHover = new FnBtnBindings(this.fnHoverItemRow),
				fnClick = _fnClick,
				APoolEvacuated = ItemMng.APoolEvacuated,
				fnChanged = delegate(BtnContainerRadio<aBtn> BCon, int pre, int cur)
				{
					if (cur < 0)
					{
						return true;
					}
					if (ItemMng != null && ItemMng.can_handle && !ItemMng.isUsingState())
					{
						aBtnItemRow aBtnItemRow3 = BCon.Get(cur) as aBtnItemRow;
						return !(aBtnItemRow3 == null) && aBtnItemRow3.getItemRow() != null && !aBtnItemRow3.getItemRow().is_fake_row;
					}
					return false;
				},
				SCA = new ScrollAppend(stencil_ref, Ds.use_w, Ds.h - 28f - 20f, 4f, 6f, 0)
			});
			if (ItemMng.APoolEvacuated == null)
			{
				this.ItemRowContainer.APool = (ItemMng.APoolEvacuated = new List<aBtn>(this.ItemRowContainer.Length));
			}
			this.fineNaviLoop();
			aBtnItemRow aBtnItemRow = null;
			if (TX.valid(this.select_row_key))
			{
				for (int j = this.ItemRowContainer.Length - 1; j >= 0; j--)
				{
					aBtnItemRow aBtnItemRow2 = this.ItemRowContainer.GetButton(j) as aBtnItemRow;
					if (aBtnItemRow2.getItemData().key == this.select_row_key)
					{
						aBtnItemRow = aBtnItemRow2;
						break;
					}
				}
			}
			if (aBtnItemRow == null && this.ItemRowContainer.Length > 0)
			{
				aBtnItemRow = this.ItemRowContainer.Get(0) as aBtnItemRow;
			}
			this.SelectedRow = aBtnItemRow;
			this.select_row_key = ((this.SelectedRow != null) ? this.SelectedRow.getItemData().key : "");
			if (this.ItemRowContainer.Length >= 2 && ItemMng.selectable_loop)
			{
				this.ItemRowContainer.Get(0).setNaviT(this.ItemRowContainer.Get(this.ItemRowContainer.Length - 1), true, true);
			}
			return aBtnItemRow;
		}

		private void fineNaviLoop()
		{
			if (this.ItemRowContainer != null && this.ItemMng != null)
			{
				int num = this.ItemRowContainer.Length;
				if (num > 0)
				{
					num--;
					aBtn aBtn = this.ItemRowContainer.Get(0);
					aBtn aBtn2 = this.ItemRowContainer.Get(num);
					if (this.ItemMng.selectable_loop)
					{
						aBtn2.setNaviB(aBtn, true, true);
					}
					if (this.ItemMng.lr_slide_row > 0)
					{
						int num2 = num - 10;
						for (int i = 0; i <= num; i++)
						{
							aBtn aBtn3 = this.ItemRowContainer.Get(i);
							aBtn3.setNaviL((i <= 10) ? aBtn : this.ItemRowContainer.Get(X.Mx(0, i - 10)), false, true);
							aBtn3.setNaviR((i >= num2) ? aBtn2 : this.ItemRowContainer.Get(X.Mn(num, i + 10)), false, true);
						}
					}
				}
				if (this.ItemMng.fnItemRowRemakedAfter != null)
				{
					this.ItemMng.fnItemRowRemakedAfter(this);
				}
			}
		}

		public void setTBNavi(BtnContainer<aBtn> BCon, bool set_top = true, bool set_bottom = true, bool use_scroll_box_focus = false)
		{
			if (this.ItemRowContainer != null)
			{
				aBtn aBtn;
				aBtn aBtn2;
				if (this.ItemRowContainer.Length > 0)
				{
					aBtn = this.ItemRowContainer.Get(this.ItemRowContainer.Length - 1);
					aBtn2 = this.ItemRowContainer.Get(0);
				}
				else
				{
					if (!use_scroll_box_focus)
					{
						for (int i = 0; i < BCon.Length; i++)
						{
							aBtn aBtn3 = BCon.Get(i);
							if (set_bottom)
							{
								aBtn3.clearNavi(8U, false);
							}
							if (set_top)
							{
								aBtn3.clearNavi(2U, false);
							}
						}
						return;
					}
					aBtn2 = (aBtn = this.ItemRowContainer.OuterScrollBox.getScrollBox().BView);
				}
				for (int j = 0; j < BCon.Length; j++)
				{
					aBtn aBtn4 = BCon.Get(j);
					if (set_bottom)
					{
						aBtn4.setNaviT(aBtn, false, false);
						aBtn.clearNavi(8U, false);
						aBtn.setNaviB(aBtn4, false, false);
					}
					if (set_top)
					{
						aBtn4.setNaviB(aBtn2, false, false);
						aBtn2.clearNavi(2U, false);
						aBtn2.setNaviT(aBtn4, false, false);
					}
				}
			}
		}

		private void getItemKeys(BtnContainerBasic _Container, List<string> Adest)
		{
			using (BList<ItemStorage.IRow> blist = ListBuffer<ItemStorage.IRow>.Pop(this.ARow.Count))
			{
				using (BList<ItemStorage.IRow> blist2 = ListBuffer<ItemStorage.IRow>.Pop(this.ARow.Count))
				{
					this.recalcVisibleRowCount(blist2);
					if (this.ItemMng.fnWholeRowsPrepare != null)
					{
						this.ItemMng.fnWholeRowsPrepare(this.ItemMng, blist2, blist);
					}
					else
					{
						blist.AddRange(blist2);
					}
					blist.Sort(this.FD_sortItemKeys);
					int count = blist.Count;
					for (int i = 0; i < count; i++)
					{
						Adest.Add(blist[i].index.ToString());
					}
				}
			}
		}

		private int sortItemKeys(ItemStorage.IRow Ra, ItemStorage.IRow Rb)
		{
			if (Ra.is_fake_row != Rb.is_fake_row)
			{
				if (!Ra.is_fake_row)
				{
					return -1;
				}
				return 1;
			}
			else if (Ra.hidden != Rb.hidden)
			{
				if (Ra.hidden >= Rb.hidden)
				{
					return 1;
				}
				return -1;
			}
			else if (Ra.Data == Rb.Data)
			{
				if (Ra.splitted_grade == Rb.splitted_grade)
				{
					return 0;
				}
				if (Ra.splitted_grade >= Rb.splitted_grade)
				{
					return 1;
				}
				return -1;
			}
			else
			{
				ItemStorage.SORT_TYPE sort_TYPE = ((this.sort_button_bits == 0) ? ItemStorage.SORT_TYPE.KIND : this.sort_type);
				int num = X.MPF((sort_TYPE & ItemStorage.SORT_TYPE._DESCEND) == ItemStorage.SORT_TYPE.NEWER);
				ItemStorage.SORT_TYPE sort_TYPE2 = sort_TYPE & (ItemStorage.SORT_TYPE)(-129);
				int num2;
				if (this.ItemMng != null && this.ItemMng.fnSortInjectMng != null && this.ItemMng.fnSortInjectMng(Ra, Rb, sort_TYPE2, out num2))
				{
					return num2 * num;
				}
				switch (sort_TYPE2)
				{
				case ItemStorage.SORT_TYPE.NEWER:
					if (Ra.Info.newer != Rb.Info.newer)
					{
						return ((Ra.Info.newer > Rb.Info.newer) ? (-1) : 1) * num;
					}
					break;
				case ItemStorage.SORT_TYPE.ABC:
				{
					int num3 = Ra.Info.top_grade;
					int num4 = Rb.Info.top_grade;
					string localizedName = Ra.Data.getLocalizedName(Ra.top_grade);
					string localizedName2 = Rb.Data.getLocalizedName(Rb.top_grade);
					if (localizedName != localizedName2)
					{
						return localizedName.CompareTo(localizedName2) * num;
					}
					break;
				}
				case ItemStorage.SORT_TYPE.PRICE:
				{
					float num5;
					float num6;
					if (this.grade_split)
					{
						num5 = Ra.Data.getPrice((int)Ra.splitted_grade);
						num6 = Rb.Data.getPrice((int)Rb.splitted_grade);
					}
					else
					{
						num5 = Ra.Data.getPrice(Ra.top_grade);
						num6 = Rb.Data.getPrice(Rb.top_grade);
					}
					if (num5 != num6)
					{
						return ((num5 < num6) ? (-1) : 1) * num;
					}
					break;
				}
				case ItemStorage.SORT_TYPE.COST:
					if (Ra.Data.value != Rb.Data.value)
					{
						return ((Ra.Data.value < Rb.Data.value) ? (-1) : 1) * num;
					}
					break;
				case ItemStorage.SORT_TYPE.EQUIP:
				{
					int num3 = Ra.Info.top_grade;
					int num4 = Rb.Info.top_grade;
					if (num3 != num4)
					{
						return ((num3 > num4) ? (-1) : 1) * num;
					}
					break;
				}
				}
				if (this.fnSortKindInject != null)
				{
					num2 = this.fnSortKindInject(Ra, Rb);
					if (num2 != 0)
					{
						return num2;
					}
				}
				int num7 = (Ra.Data.useable ? 2 : (Ra.Data.is_food ? 1 : 0));
				int num8 = (Rb.Data.useable ? 2 : (Rb.Data.is_food ? 1 : 0));
				if (num7 == num8)
				{
					return X.FnSortIntager((int)Ra.Data.id, (int)Rb.Data.id) * ((sort_TYPE2 == ItemStorage.SORT_TYPE.KIND) ? num : 1);
				}
				return (num8 - num7) * num;
			}
		}

		private bool fnMakingItemRow(BtnContainer<aBtn> BCon, aBtn B)
		{
			int num = X.NmI(B.title, 0, false, false);
			B.transform.SetParent(BCon.getGob().transform, false);
			this.ItemMng.itemRowInit(B as aBtnItemRow, this.ARow[num]);
			return true;
		}

		private void fineSortButton(BtnContainerBasic BCon, bool play_snd = true)
		{
			if (BCon == null)
			{
				return;
			}
			int length = BCon.Length;
			ItemStorage.SORT_TYPE sort_TYPE = this.sort_type & (ItemStorage.SORT_TYPE)(-129);
			for (int i = 0; i < length; i++)
			{
				aBtn button = BCon.GetButton(i);
				ItemStorage.SORT_TYPE sort_TYPE2;
				FEnum<ItemStorage.SORT_TYPE>.TryParse(TX.slice(button.title, 5).ToUpper(), out sort_TYPE2, true);
				if (sort_TYPE2 == sort_TYPE)
				{
					(button.get_Skin() as ButtonSkinMiniSortNel).is_descend = (this.sort_type & ItemStorage.SORT_TYPE._DESCEND) > ItemStorage.SORT_TYPE.NEWER;
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
				string text2 = "Item_list_sort_";
				ItemStorage.SORT_TYPE sort_TYPE3 = sort_TYPE;
				psortDesc.setValue(text + TX.Get(text2 + sort_TYPE3.ToString().ToLower(), ""));
			}
			if (play_snd)
			{
				SND.Ui.play("toggle_button_open", false);
			}
		}

		private bool fnSortButtonClicked(aBtn B)
		{
			ItemStorage.SORT_TYPE sort_TYPE;
			FEnum<ItemStorage.SORT_TYPE>.TryParse(TX.slice(B.title, 5).ToUpper(), out sort_TYPE, true);
			ItemStorage.SORT_TYPE sort_TYPE2 = this.sort_type & (ItemStorage.SORT_TYPE)(-129);
			if (sort_TYPE == sort_TYPE2)
			{
				this.sort_type = sort_TYPE2 | (((this.sort_type & ItemStorage.SORT_TYPE._DESCEND) != ItemStorage.SORT_TYPE.NEWER) ? ItemStorage.SORT_TYPE.NEWER : ItemStorage.SORT_TYPE._DESCEND);
			}
			else
			{
				this.sort_type = (this.sort_type & ItemStorage.SORT_TYPE._DESCEND) | sort_TYPE;
			}
			this.fineSortButton(B.Container, true);
			if (this.ItemRowContainer != null)
			{
				this.fineRowButtonLink(null);
			}
			return true;
		}

		public void progressSortByLshKey()
		{
			if (this.sort_button_bits == 0)
			{
				return;
			}
			int num = (int)(this.sort_type & (ItemStorage.SORT_TYPE)(-129));
			ItemStorage.SORT_TYPE sort_TYPE = this.sort_type & ItemStorage.SORT_TYPE._DESCEND;
			sort_TYPE &= (ItemStorage.SORT_TYPE)(-129);
			for (int i = 0; i < 6; i++)
			{
				num = (num + 1) % 6;
				if ((this.sort_button_bits & (1 << num)) != 0)
				{
					break;
				}
			}
			this.sort_type = (ItemStorage.SORT_TYPE)(num | (int)sort_TYPE);
			BtnContainerRunner btnContainerRunner = this.Ds.Get("__sort_container", false) as BtnContainerRunner;
			if (btnContainerRunner != null)
			{
				this.fineSortButton(btnContainerRunner.BCon, true);
			}
			if (this.ItemRowContainer != null)
			{
				this.fineRowButtonLink(null);
			}
		}

		public void setSort(ItemStorage.SORT_TYPE s, bool descend = false)
		{
			this.sort_type = (s & (ItemStorage.SORT_TYPE)(-129)) | (descend ? ItemStorage.SORT_TYPE._DESCEND : ItemStorage.SORT_TYPE.NEWER);
			if (this.Ds != null)
			{
				BtnContainerRunner btnContainerRunner = this.Ds.Get("__sort_container", false) as BtnContainerRunner;
				if (btnContainerRunner != null)
				{
					this.fineSortButton(btnContainerRunner.BCon, true);
				}
				if (this.ItemRowContainer != null)
				{
					this.fineRowButtonLink(null);
				}
			}
		}

		public void BlurCheckedRow()
		{
			if (this.ItemRowContainer != null)
			{
				this.ItemRowContainer.setValue(-1, true);
			}
		}

		public bool hasTopRightCuonter()
		{
			return this.PCounter != null;
		}

		public void setTopRightCounter(string str)
		{
			if (this.PCounter != null)
			{
				this.PCounter.text_content = str;
			}
		}

		public void RowReveal(Transform T, bool immediate = false)
		{
			if (this.ItemRowContainer != null)
			{
				this.ItemRowContainer.OuterScrollBox.getScrollBox().reveal(T, 0f, 0f, !immediate);
			}
		}

		public void ScrollAnimationFinalize()
		{
			if (this.ItemRowContainer != null)
			{
				this.ItemRowContainer.OuterScrollBox.getScrollBox().FineScrollAnim();
			}
		}

		public bool rowcontainer_alloc_wheel
		{
			get
			{
				return this.ItemRowContainer != null && this.ItemRowContainer.BelongScroll.alloc_wheel;
			}
			set
			{
				if (this.ItemRowContainer != null)
				{
					this.ItemRowContainer.BelongScroll.alloc_wheel = value;
				}
			}
		}

		public void hideBCon()
		{
			if (this.ItemRowContainer != null)
			{
				this.ItemRowContainer.hide(false, false);
			}
		}

		public void bindBCon()
		{
			if (this.ItemRowContainer != null)
			{
				this.ItemRowContainer.bind(false, false);
			}
		}

		public aBtnItemRow getAbsNearBtn(Vector2 V)
		{
			if (this.ItemRowContainer == null)
			{
				return null;
			}
			return this.ItemRowContainer.getAbsNearBtn(V) as aBtnItemRow;
		}

		public int getItemRowBtnCount()
		{
			if (this.ItemRowContainer == null)
			{
				return 0;
			}
			return this.ItemRowContainer.Length;
		}

		public aBtnItemRow getItemRowBtnByIndex(int i)
		{
			if (this.ItemRowContainer == null)
			{
				return null;
			}
			return this.ItemRowContainer.Get(i) as aBtnItemRow;
		}

		public List<aBtn> EvacuateButtonsFromPreviousManager(bool release_link = false)
		{
			if (this.ItemRowContainer == null || this.ItemMng == null)
			{
				return null;
			}
			List<aBtn> apool = this.ItemRowContainer.APool;
			for (int i = this.ItemRowContainer.Length - 1; i >= 0; i--)
			{
				aBtnItemRow aBtnItemRow = this.ItemRowContainer.Get(i) as aBtnItemRow;
				if (aBtnItemRow != null)
				{
					this.ItemRowContainer.Rem(i, true);
					aBtnItemRow.transform.SetParent(this.ItemMng.TrsEvacuateTo, false);
				}
			}
			if (release_link)
			{
				this.ItemRowContainer.APool = null;
				this.ItemMng.APoolEvacuated = null;
			}
			return apool;
		}

		public BList<aBtnItemRow> PopGetItemRowBtnsFor(NelItem Data)
		{
			BList<aBtnItemRow> blist = ListBuffer<aBtnItemRow>.Pop(0);
			this.getItemRowBtnsFor(blist, Data);
			return blist;
		}

		private int getItemRowBtnsFor(List<aBtnItemRow> ARow, NelItem Data)
		{
			if (this.ItemRowContainer == null)
			{
				return 0;
			}
			int num = 0;
			int i = 0;
			bool flag = false;
			if (this.SelectedRow != null && Data == this.SelectedRow.getItemData())
			{
				int j = this.SelectedRow.carr_index;
				while (j >= 0)
				{
					if ((this.ItemRowContainer.Get(j) as aBtnItemRow).getItemData() == Data)
					{
						flag = true;
						j--;
					}
					else
					{
						if (flag)
						{
							i = j + 1;
							break;
						}
						break;
					}
				}
			}
			int length = this.ItemRowContainer.Length;
			while (i < length)
			{
				aBtnItemRow aBtnItemRow = this.ItemRowContainer.Get(i) as aBtnItemRow;
				if (aBtnItemRow.getItemData() == Data)
				{
					flag = true;
					ARow.Add(aBtnItemRow);
					num++;
				}
				else if (flag)
				{
					break;
				}
				i++;
			}
			return num;
		}

		private bool fnHoverItemRow(aBtn B)
		{
			if (this.ItemMng != null && !this.ItemMng.can_handle)
			{
				return false;
			}
			if (this.ItemRowContainer != null && this.ItemRowContainer.getValue() >= 0)
			{
				return true;
			}
			this.SelectedRow = B as aBtnItemRow;
			this.select_row_key = this.SelectedRow.getItemData().key;
			if (this.ItemMng != null && this.ItemMng.manager_auto_run_on_select_row && !this.ItemMng.isUsingState())
			{
				this.ItemMng.runEditItem();
			}
			return true;
		}

		public aBtnItemRow reselectTargetRow(NelItem Itm, bool check_target = true, int splitted_grade = -1)
		{
			ItemStorage.ObtainInfo info = this.getInfo(Itm);
			if (info == null || info.LastRow == null)
			{
				return null;
			}
			ItemStorage.IRow row = null;
			if (splitted_grade >= 0 && this.grade_split)
			{
				row = this.getIRowForGrade(Itm, this.getInfo(Itm), splitted_grade, null);
			}
			return this.reselectTargetRow(Itm, (row != null) ? row : ((this.SelectedRow != null && this.SelectedRow.getItemData() == Itm) ? this.SelectedRow.getItemRow() : info.LastRow), check_target);
		}

		public aBtnItemRow reselectTargetRow(NelItem Itm, ItemStorage.IRow Row, bool check_target = true)
		{
			using (BList<aBtnItemRow> blist = this.PopGetItemRowBtnsFor(Itm))
			{
				if (blist.Count == 0)
				{
					return null;
				}
				for (int i = blist.Count - 1; i >= 0; i--)
				{
					if (blist[i].getItemRow() == Row || i == 0)
					{
						this.FocusSelectedRowTo(blist[i], check_target);
						return this.SelectedRow;
					}
				}
			}
			return null;
		}

		public void FocusSelectedRowTo(aBtnItemRow Row, bool recheck = true)
		{
			if (this.ItemRowContainer != null)
			{
				this.SelectedRow = Row;
				this.select_row_key = Row.getItemData().key;
				if (recheck)
				{
					this.ItemRowContainer.setValue(this.ItemRowContainer.getIndex(this.SelectedRow), true);
				}
			}
		}

		public void releaseDesigner()
		{
			this.ItemRowContainer = null;
			this.SelectedRow = null;
			this.ItemMng = null;
			this.PSortDesc = (this.PCounter = null);
			this.Ds = null;
			this.MemoryNewer(null);
		}

		public int getTopGrade(NelItem Itm)
		{
			ItemStorage.ObtainInfo obtainInfo;
			if (!this.OItm.TryGetValue(Itm, out obtainInfo))
			{
				return 0;
			}
			return obtainInfo.top_grade;
		}

		public void AddInfo(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			this.need_reindex_newer = true;
			ItemStorage.ObtainInfo obtainInfo;
			if (this.OItm.TryGetValue(Itm, out obtainInfo))
			{
				obtainInfo.addFrom(Obt);
				return;
			}
			this.OItm[Itm] = new ItemStorage.ObtainInfo(Obt);
		}

		public ItemStorage.ObtainInfo getInfo(NelItem Itm)
		{
			if (Itm == null)
			{
				return null;
			}
			return X.Get<NelItem, ItemStorage.ObtainInfo>(this.OItm, Itm);
		}

		public int getVisibleRowCount()
		{
			if (this.visible_row_count < 0)
			{
				this.recalcVisibleRowCount(null);
			}
			return this.visible_row_count;
		}

		public int getWholeRowCount()
		{
			return this.ARow.Count;
		}

		private int recalcVisibleRowCount(List<ItemStorage.IRow> ARowBuffer = null)
		{
			int count = this.ARow.Count;
			if (this.water_stockable)
			{
				this.visible_row_count = count;
				if (ARowBuffer != null)
				{
					ARowBuffer.Clear();
					ARowBuffer.AddRange(this.ARow);
				}
			}
			else
			{
				this.visible_row_count = 0;
				ItemStorage.ABlinkBuf.Clear();
				ItemStorage.ABlinkBuf.AddRange(this.ABlink);
				if (ARowBuffer != null)
				{
					ARowBuffer.Clear();
				}
				for (int i = 0; i < count; i++)
				{
					ItemStorage.IRow row = this.ARow[i];
					if (ARowBuffer != null && (row.hidden <= ItemStorage.ROWHID.VISIBLE || row.blink_index < 0))
					{
						ARowBuffer.Add(row);
					}
					if (row.hidden <= ItemStorage.ROWHID.VISIBLE)
					{
						this.visible_row_count++;
					}
				}
			}
			return this.visible_row_count;
		}

		public ItemStorage.IRow getRowByIndex(int i)
		{
			return this.ARow[i];
		}

		public int getCount(NelItem Data, int grade = -1)
		{
			if (Data == null)
			{
				return 0;
			}
			ItemStorage.ObtainInfo obtainInfo = X.Get<NelItem, ItemStorage.ObtainInfo>(this.OItm, Data);
			if (obtainInfo == null)
			{
				return 0;
			}
			if (grade >= 0)
			{
				return obtainInfo.getCount(grade);
			}
			return obtainInfo.total;
		}

		public int getCount(RCP.Recipe TargetRecipe)
		{
			int num = 0;
			if (TargetRecipe.Completion != null)
			{
				return this.getCount(TargetRecipe.Completion, -1);
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.OItm)
			{
				RCP.RecipeDish dish = RCP.getDish(keyValuePair.Key);
				if (dish != null && dish.Rcp == TargetRecipe)
				{
					num += keyValuePair.Value.total;
				}
			}
			return num;
		}

		public int getReduceable(NelItem Data, int grade = -1)
		{
			int num = this.getCount(Data, grade);
			bool flag;
			if (!this.water_stockable && Data.isWLinkUser(out flag))
			{
				int count = this.ABlink.Count;
				for (int i = 0; i < count; i++)
				{
					ItemStorage.WaterLink waterLink = this.ABlink[i];
					if (waterLink.AnotherMustHasLink(Data))
					{
						num -= waterLink.getCount(Data);
					}
				}
				return X.Mx(num, 0);
			}
			return num;
		}

		public int getCountMoreGrade(NelItem Data, int grade)
		{
			if (Data == null)
			{
				return 0;
			}
			ItemStorage.ObtainInfo obtainInfo = X.Get<NelItem, ItemStorage.ObtainInfo>(this.OItm, Data);
			if (obtainInfo == null)
			{
				return 0;
			}
			return obtainInfo.getCountMoreGrade(grade);
		}

		public int getCountMoreGrade(string _category, int grade)
		{
			int num = 0;
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.OItm)
			{
				if (keyValuePair.Key.Is(_category))
				{
					num += keyValuePair.Value.getCountMoreGrade(grade);
				}
			}
			return num;
		}

		public int getItemCountFn(ItemStorage.FnCheckItemDataAndInfo Fn, List<ItemStorage.IRow> ARow = null)
		{
			int num = 0;
			int count = this.ARow.Count;
			for (int i = 0; i < count; i++)
			{
				ItemStorage.IRow row = this.ARow[i];
				if (Fn(row))
				{
					num += row.total;
					if (ARow != null)
					{
						ARow.Add(row);
					}
				}
			}
			return num;
		}

		public int tranferItems(ItemStorage Dest, List<NelItemEntry> ARow, int center_grade = 2)
		{
			if (ARow == null)
			{
				return 0;
			}
			int count = ARow.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				NelItemEntry nelItemEntry = ARow[i];
				ItemStorage.ObtainInfo info = this.getInfo(nelItemEntry.Data);
				if (info != null)
				{
					int j = nelItemEntry.count;
					int num2 = 5;
					int num3 = center_grade;
					while (j > 0)
					{
						int num4 = X.Mn(j, info.getCount(num3));
						if (num4 == 0)
						{
							if (--num2 <= 0)
							{
								break;
							}
							num3 = (num3 - 1 + 5) % 5;
						}
						else
						{
							j -= num4;
							this.Reduce(nelItemEntry.Data, num4, num3, false);
							Dest.Add(nelItemEntry.Data, num4, num3, true, true);
							num += num4;
						}
					}
				}
			}
			this.fineRows(false);
			Dest.fineRows(false);
			return num;
		}

		public bool ifExists(NelItem Data, int grade = -1)
		{
			return this.getCount(Data, grade) > 0;
		}

		public int getUnlinkCount(NelItem Data, int grade = -1)
		{
			bool flag;
			if (this.water_stockable || !Data.isWLinkUser(out flag))
			{
				return this.getCount(Data, grade);
			}
			int num = 0;
			for (int i = this.ARow.Count - 1; i >= 0; i--)
			{
				ItemStorage.IRow row = this.ARow[i];
				if (row.Data == Data && row.blink_index < 0)
				{
					num += row.total;
				}
			}
			if (grade >= 0)
			{
				return X.Mn(num, this.getCount(Data, grade));
			}
			return num;
		}

		public int getEmptyBottleCount()
		{
			return this.getUnlinkCount(NelItem.Bottle, -1);
		}

		public bool existStockableRow(NelItem Data)
		{
			return this.Add(Data, 1, 0, false, false) > 0;
		}

		public BDic<NelItem, ItemStorage.ObtainInfo> getWholeInfoDictionary()
		{
			return this.OItm;
		}

		public void getSortType(out ItemStorage.SORT_TYPE _sort_type, out bool _descend)
		{
			_sort_type = this.sort_type & (ItemStorage.SORT_TYPE)(-129);
			_descend = (this.sort_type & ItemStorage.SORT_TYPE._DESCEND) > ItemStorage.SORT_TYPE.NEWER;
		}

		public bool isAddable(NelItem Data, bool ignore_area = false)
		{
			return this.fnAddable == null || this.fnAddable(Data, ignore_area);
		}

		public int getItemCapacity(NelItem Data, bool do_not_calc_to_container = false, bool do_not_add_row = false)
		{
			int itemStockable = this.getItemStockable(Data);
			int count = this.getCount(Data, -1);
			int num = X.IntC((float)count / (float)itemStockable);
			int num2 = X.Mx(0, num * itemStockable - count);
			int num3 = (do_not_add_row ? 0 : (this.infinit_stockable ? itemStockable : ((this.row_max - this.getVisibleRowCount()) * itemStockable)));
			if (!this.water_stockable)
			{
				bool flag;
				if (Data.isWLinkUser(out flag))
				{
					if (flag)
					{
						num3 = 0;
					}
					int num4 = 0;
					if (!do_not_calc_to_container)
					{
						this.getVisibleRowCount();
						int count2 = this.ARow.Count;
						for (int i = 0; i < count2; i++)
						{
							ItemStorage.IRow row = this.ARow[i];
							bool flag2;
							if (row.blink_index < 0 && row.Data.connectableWLink(Data, out flag2))
							{
								if (row.hidden > ItemStorage.ROWHID.VISIBLE)
								{
									if (this.visible_row_count + num4 >= this.row_max)
									{
										goto IL_00D7;
									}
									num4++;
								}
								num2 += itemStockable;
							}
							IL_00D7:;
						}
					}
				}
				if (Data == NelItem.Bottle && this.ObtFakeBottleHolder != null)
				{
					num2 += this.ObtFakeBottleHolder.total;
				}
			}
			return num2 + num3;
		}

		private BDic<NelItem, ItemStorage.ObtainInfo> OItm;

		private List<ItemStorage.WaterLink> ABlink;

		private List<ItemStorage.IRow> ARow;

		public readonly string key;

		public int sort_button_bits = 15;

		public int row_max;

		public bool infinit_stockable;

		public bool grade_split;

		public bool one_item_line;

		public bool auto_splice_zero_row = true;

		public bool auto_update_topright_counter;

		public int hide_bottle_max;

		public bool raw_mode;

		public bool check_quest_target;

		public bool do_not_input_newer;

		public const int GRADE_MAX = 5;

		private uint newer_memory;

		private NelItem Newer_Memory_Item;

		private List<aBtnItemRow> ARowBtn;

		public string select_row_key = "";

		private ItemStorage.SORT_TYPE sort_type;

		public bool need_reindex_newer;

		private uint last_newer;

		public FnItemCheck fnAddable;

		public ItemStorage.FnSortKindInject fnSortKindInject;

		public ItemStorage.FnRowAddition fnRowAddition;

		private int visible_row_count = -1;

		private static List<ItemStorage.WaterLinkMem> ABMem;

		private static List<ItemStorage.WaterLink> ABlinkBuf;

		private Comparison<ItemStorage.IRow> FD_sortItemKeys;

		public UiItemManageBox.FnRowNameAddition FD_RowNameAddition;

		public UiItemManageBox.FnRowIconAddition FD_RowIconAddition;

		private ItemStorage.ObtainInfo ObtFakeBottleHolder;

		private UiItemManageBox ItemMng;

		private Designer Ds;

		private BtnContainerRadio<aBtn> ItemRowContainer;

		private FillBlock PSortDesc;

		private FillBlock PCounter;

		public aBtnItemRow SelectedRow;

		public delegate bool FnCheckItemDataAndInfo(ItemStorage.IRow Row);

		public enum ROWHID
		{
			VISIBLE,
			WL_OUTER,
			H_BOTTLE
		}

		public sealed class ObtainInfo
		{
			public ObtainInfo()
			{
			}

			public ObtainInfo(int srcc, int srcg = 0)
			{
				this.AddCount(srcc, srcg);
			}

			public ObtainInfo(ItemStorage.ObtainInfo Src)
			{
				Src.copyTo(this);
			}

			public ObtainInfo(NelItemEntry IE)
				: this(IE.count, (int)IE.grade)
			{
			}

			public void clear()
			{
				this.total_ = 0;
				X.ALL0(this.Agrade);
			}

			public bool isSame(ItemStorage.ObtainInfo T)
			{
				if (this.total_ != T.total_)
				{
					return false;
				}
				for (int i = 4; i >= 0; i--)
				{
					if (this.Agrade[i] != T.Agrade[i])
					{
						return false;
					}
				}
				return true;
			}

			public void AddCount(int count, int grade)
			{
				this.Agrade[grade] += count;
				this.grade_touched |= 1U << grade;
				this.total_ += count;
			}

			public void countMultiply(int mul)
			{
				for (int i = 4; i >= 0; i--)
				{
					this.Agrade[i] *= mul;
				}
				this.total_ *= mul;
			}

			public int getCount(int grade)
			{
				if (grade >= 0)
				{
					return this.Agrade[grade];
				}
				return this.total_;
			}

			public int getCountMoreGrade(int grade)
			{
				if (grade <= 0)
				{
					return this.total_;
				}
				int num = this.total_;
				for (int i = 0; i < grade; i++)
				{
					num -= this.Agrade[i];
				}
				return num;
			}

			public uint getGradeUsingBit()
			{
				uint num = 0U;
				for (int i = 0; i < 5; i++)
				{
					num |= ((this.Agrade[i] != 0) ? (1U << i) : 0U);
				}
				return num;
			}

			public void ReduceCount(int count, int grade)
			{
				count = X.Mn(this.Agrade[grade], count);
				this.Agrade[grade] -= count;
				this.total_ -= count;
			}

			public int getRandomGrade()
			{
				if (this.total_ == 0)
				{
					return 0;
				}
				int num = X.xors(this.total);
				for (int i = 4; i >= 1; i--)
				{
					int num2 = this.Agrade[i];
					if (num2 != 0)
					{
						if (num < num2)
						{
							return i;
						}
						num -= num2;
					}
				}
				return this.min_grade;
			}

			public int top_grade
			{
				get
				{
					for (int i = 4; i >= 1; i--)
					{
						if (this.Agrade[i] != 0)
						{
							return i;
						}
					}
					return 0;
				}
			}

			public int min_grade
			{
				get
				{
					for (int i = 0; i < 5; i++)
					{
						if (this.Agrade[i] != 0)
						{
							return i;
						}
					}
					return 0;
				}
			}

			public int few_grade
			{
				get
				{
					int num = -1;
					int num2 = 0;
					for (int i = 0; i < 5; i++)
					{
						int num3 = this.Agrade[i];
						if (num3 > 0 && (num == -1 || num2 > num3))
						{
							num = i;
							num2 = num3;
						}
					}
					return num;
				}
			}

			public int enough_grade
			{
				get
				{
					int num = -1;
					int num2 = 0;
					for (int i = 4; i >= 0; i--)
					{
						int num3 = this.Agrade[i];
						if (num3 > 0 && (num == -1 || num2 < num3))
						{
							num = i;
							num2 = num3;
						}
					}
					return num;
				}
			}

			public int total
			{
				get
				{
					return this.total_;
				}
			}

			public int nearGradeCountScore(int grade)
			{
				int num = this.getCount(grade) * 3;
				if (grade > 0)
				{
					num += this.getCount(grade - 1);
				}
				if (grade < 4)
				{
					num += this.getCount(grade + 1);
				}
				return num;
			}

			public void changeGradeForPrecious(int grade, int _count = 1)
			{
				grade = X.MMX(0, grade, 4);
				this.total_ = _count;
				for (int i = 4; i >= 0; i--)
				{
					this.Agrade[i] = ((i == grade) ? _count : 0);
				}
			}

			public void copyGradeArrayTo(int[] A)
			{
				Array.Copy(this.Agrade, A, 5);
			}

			public void copyTo(ItemStorage.ObtainInfo Obt)
			{
				Array.Copy(this.Agrade, Obt.Agrade, 5);
				Obt.newer = this.newer;
				Obt.total_ = (this.total_ = X.sum(this.Agrade));
			}

			public void addFrom(ItemStorage.ObtainInfo Obt)
			{
				for (int i = 4; i >= 0; i--)
				{
					this.Agrade[i] += Obt.Agrade[i];
				}
				this.total_ = X.sum(this.Agrade);
			}

			public ItemStorage.ObtainInfo readBinaryFrom(ByteReader Ba)
			{
				int num = 5;
				for (int i = 0; i < num; i++)
				{
					this.Agrade[i] = (int)Ba.readUShort();
				}
				this.newer = Ba.readUInt();
				this.total_ = X.sum(this.Agrade);
				return this;
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				int num = 5;
				for (int i = 0; i < num; i++)
				{
					Ba.writeUShort((ushort)this.Agrade[i]);
				}
				Ba.writeUInt(this.newer);
			}

			public override string ToString()
			{
				return "<ObtainInfo> " + this.ToSimpleString();
			}

			public string ToSimpleString()
			{
				return string.Concat(new string[]
				{
					"New:",
					this.newer.ToString(),
					" Total:",
					this.total.ToString(),
					" (",
					TX.join<int>(",", this.Agrade, 0, -1),
					")"
				});
			}

			public ItemStorage.IRow UnlinkRow
			{
				get
				{
					if (this.LastRow == null)
					{
						return null;
					}
					return this.LastRow.UnlinkRow;
				}
			}

			private int[] Agrade = new int[5];

			private int total_;

			public uint newer;

			public uint grade_touched;

			public ItemStorage.IRow LastRow;

			public const int obt_info_bits = 14;
		}

		public sealed class IRow
		{
			public IRow(NelItem _Itm, ItemStorage.ObtainInfo _Info, bool auto_link = true)
			{
				this.Data = _Itm;
				this.Info = _Info;
				if (auto_link)
				{
					if (this.Info != null)
					{
						if (this.Info.LastRow != null)
						{
							this.ALink = this.Info.LastRow.ALink;
						}
						this.Info.LastRow = this;
					}
					if (this.Info != null)
					{
						if (this.ALink == null)
						{
							this.ALink = new List<ItemStorage.IRow>(1);
						}
						this.ALink.Add(this);
					}
				}
			}

			public ItemStorage.IRow CopyFromInfo()
			{
				this.total = ((this.Info != null) ? this.Info.total : 0);
				return this;
			}

			public int top_grade
			{
				get
				{
					return this.Info.top_grade;
				}
			}

			public ItemStorage.IRow AddCount(int count, int grade)
			{
				this.total += count;
				this.Info.AddCount(count, grade);
				return this;
			}

			public ItemStorage.IRow ReduceCount(int count, int grade)
			{
				this.total -= count;
				this.Info.ReduceCount(count, grade);
				return this;
			}

			public void fineRowCount(ItemStorage Str)
			{
				using (BList<aBtnItemRow> blist = Str.PopGetItemRowBtnsFor(this.Data))
				{
					if (blist != null && blist.Count != 0)
					{
						for (int i = blist.Count - 1; i >= 0; i--)
						{
							blist[i].RowSkin.fineCount(true);
						}
					}
				}
			}

			public int split_or_top_grade(ItemStorage Str)
			{
				if (!Str.grade_split)
				{
					return this.top_grade;
				}
				return (int)this.splitted_grade;
			}

			public bool has_wlink
			{
				get
				{
					return this.blink_index >= 0;
				}
			}

			public bool is_fake_row
			{
				get
				{
					return this.hidden == ItemStorage.ROWHID.H_BOTTLE && this.Data == NelItem.HolderBottle;
				}
			}

			public ItemStorage.IRow UnlinkRow
			{
				get
				{
					if (this.ALink != null)
					{
						int count = this.ALink.Count;
						for (int i = 0; i < count; i++)
						{
							ItemStorage.IRow row = this.ALink[i];
							if (row.blink_index < 0)
							{
								return row;
							}
						}
						return null;
					}
					if (this.blink_index < 0)
					{
						return this;
					}
					return null;
				}
			}

			public override string ToString()
			{
				return "<IRow>" + this.Data.ToSimpleString() + "-" + this.Info.ToSimpleString();
			}

			public int total;

			public int index;

			public NelItem Data;

			public ItemStorage.ObtainInfo Info;

			public List<ItemStorage.IRow> ALink;

			public ItemStorage.ROWHID hidden;

			public byte splitted_grade;

			public int blink_index = -1;
		}

		private struct WaterLink
		{
			public ItemStorage.IRow Outer { readonly get; private set; }

			public ItemStorage.IRow Inner { readonly get; private set; }

			public WaterLink(ItemStorage.IRow _Outer, ItemStorage.IRow _Inner, int index)
			{
				this.Outer = _Outer;
				this.Inner = _Inner;
				if (this.Outer != null)
				{
					this.Outer.blink_index = index;
					if (this.Inner != null)
					{
						this.Outer.hidden = ItemStorage.ROWHID.WL_OUTER;
					}
				}
				if (this.Inner != null)
				{
					this.Inner.blink_index = index;
				}
			}

			public ItemStorage.WaterLinkMem createMem()
			{
				return new ItemStorage.WaterLinkMem(this);
			}

			public bool Is(NelItem Itm)
			{
				return (this.Outer != null && this.Outer.Data == Itm) || (this.Inner != null && this.Inner.Data == Itm);
			}

			public bool Is(ItemStorage.IRow IR)
			{
				return (this.Outer != null && this.Outer == IR) || (this.Inner != null && this.Inner == IR);
			}

			public void Connect(ItemStorage.IRow NewRow, int index, out ItemStorage.IRow OverrideRow, ItemStorage St)
			{
				OverrideRow = null;
				ItemStorage.ROWHID rowhid = ((this.Outer != null) ? this.Outer.hidden : ItemStorage.ROWHID.VISIBLE);
				bool flag;
				if (this.Inner == null)
				{
					this.Inner = NewRow;
					this.Inner.blink_index = index;
				}
				else if (this.Outer == null)
				{
					rowhid = NewRow.hidden;
					this.Outer = NewRow;
					this.Outer.blink_index = index;
					this.Outer.hidden = ItemStorage.ROWHID.WL_OUTER;
					OverrideRow = this.Inner;
				}
				else if (NewRow.Data.connectableWLink(this.Outer.Data, out flag))
				{
					this.Inner.blink_index = -1;
					this.Inner = NewRow;
					this.Inner.blink_index = index;
				}
				else if (NewRow.Data.connectableWLink(this.Inner.Data, out flag))
				{
					rowhid = NewRow.hidden;
					this.Outer.blink_index = -1;
					this.Outer.hidden &= (ItemStorage.ROWHID)(-2);
					this.Outer = NewRow;
					this.Outer.blink_index = index;
					this.Outer.hidden = ItemStorage.ROWHID.WL_OUTER;
					OverrideRow = this.Inner;
				}
				if (this.Outer != null)
				{
					if (rowhid == ItemStorage.ROWHID.H_BOTTLE)
					{
						St.addHiddenBottleHolder(true, false);
					}
					this.Outer.hidden = ItemStorage.ROWHID.WL_OUTER;
				}
			}

			public bool AnotherMustHasLink(NelItem D)
			{
				bool flag = false;
				bool flag2 = false;
				if (this.Inner != null && this.Inner.Data != D)
				{
					this.Inner.Data.isWLinkUser(out flag);
				}
				if (this.Outer != null && this.Outer.Data != D)
				{
					this.Outer.Data.isWLinkUser(out flag2);
				}
				return flag || flag2;
			}

			public ItemStorage.IRow getAnother(ItemStorage.IRow C)
			{
				if (C != this.Outer)
				{
					return this.Outer;
				}
				return this.Inner;
			}

			public int getCount(NelItem Data)
			{
				int num = 0;
				if (this.Outer != null && this.Outer.Data == Data)
				{
					num += this.Outer.total;
				}
				if (this.Inner != null && this.Inner.Data == Data)
				{
					num += this.Inner.total;
				}
				return num;
			}

			public bool hasEmpty()
			{
				return (this.Outer != null && this.Inner == null) || (this.Outer == null && this.Inner != null);
			}

			public bool emptyConnectable(NelItem Data)
			{
				if (this.Outer != null && this.Inner == null)
				{
					return this.Outer.Data.connectableWLinkOuter(Data);
				}
				return this.Outer == null && this.Inner != null && Data.connectableWLinkOuter(this.Inner.Data);
			}

			public void Dispose()
			{
				if (this.Outer != null)
				{
					this.Outer.blink_index = -1;
					this.Outer.hidden &= (ItemStorage.ROWHID)(-2);
				}
				if (this.Inner != null)
				{
					this.Inner.blink_index = -1;
				}
				this.Outer = (this.Inner = null);
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				ItemStorage.WaterLinkMem.writeBinaryTo(Ba, this);
			}
		}

		private struct WaterLinkMem
		{
			public WaterLinkMem(ItemStorage.WaterLink BL)
			{
				this.Outer = ((BL.Outer != null) ? BL.Outer.Data : null);
				this.Inner = ((BL.Inner != null) ? BL.Inner.Data : null);
			}

			public WaterLinkMem(NelItem Outer, NelItem Inner)
			{
				this.Outer = Outer;
				this.Inner = Inner;
			}

			public WaterLinkMem(ByteReader Ba, bool fix_ver024)
			{
				NelItem.readBinaryGetKey(Ba, out this.Outer, false, fix_ver024);
				NelItem.readBinaryGetKey(Ba, out this.Inner, false, fix_ver024);
			}

			public static void writeBinaryTo(ByteArray Ba, ItemStorage.WaterLink Wl)
			{
				NelItem.writeBinaryItemKey(Ba, (Wl.Outer != null) ? Wl.Outer.Data : null);
				NelItem.writeBinaryItemKey(Ba, (Wl.Inner != null) ? Wl.Inner.Data : null);
			}

			public NelItem Outer;

			public NelItem Inner;
		}

		public enum SORT_TYPE
		{
			NEWER,
			KIND,
			ABC,
			PRICE,
			COST,
			EQUIP,
			_ALL_TYPE,
			_DESCEND = 128
		}

		public delegate int FnSortKindInject(ItemStorage.IRow Ra, ItemStorage.IRow Rb);

		public delegate bool FnRowAddition(ItemStorage.IRow R, List<ItemStorage.IRow> ARows);
	}
}
