using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public class GQCategInfoCollectItem : GQCategInfo
	{
		public GQCategInfoCollectItem(GuildManager _Con, GuildManager.CATEG _categ, int _icon, bool _need_new_item)
			: base(_Con, _categ, _icon)
		{
			this.need_new_item = _need_new_item;
			this.auto_abort_qt_in_other_battle = this.need_new_item;
			this.auto_abort_in_progressing = (this.auto_abort_in_flush_area = false);
			this.auto_abort_in_flush_area_itemcollect = true;
		}

		public override GuildManager.GQEntry createEntry(GuildManager.GQEntryList AList, WholeMapItem WM, int grank, ushort id)
		{
			GuildManager.GQEntry gqentry = base.createEntry(AList, WM, grank, id);
			SupplyManager.ItemDescriptorForWholeMap descriptor = AList.getDescriptor(this.Con, WM.text_key);
			int num = X.Mx(1, X.IntC((float)descriptor.AAItem.Count * X.NIL(0.3f, 1f, (float)grank, 4f)));
			int num2 = this.Con.xors(num);
			descriptor.AAItem.Sort((List<NelItemEntry> A, List<NelItemEntry> B) => GQCategInfoCollectItem.fnSortEntry(A, B));
			List<NelItemEntry> list = descriptor.AAItem[num2];
			list.Sort((NelItemEntry A, NelItemEntry B) => B.count - A.count);
			int num3 = X.Mx(1, X.IntC((float)list.Count * X.NIL(0.5f, 1f, (float)grank, 4f)));
			int num4 = this.Con.xors(num3);
			NelItemEntry nelItemEntry = list[num4];
			NelItem data = nelItemEntry.Data;
			gqentry.TargetItem = data;
			float num5 = (float)nelItemEntry.count;
			float num6 = X.Scr((float)num2 / (float)num, 0.4f * (float)num4 / (float)num3);
			if (data.is_water)
			{
				if (grank >= 2 && this.Con.xors(100) < (grank - 1) * 17)
				{
					num5 += (float)data.stock;
				}
			}
			else
			{
				num5 += X.Mx(0f, -2f + this.Con.XORSP() * this.randomize_count(grank));
			}
			num5 *= this.randomize_after_multiple(grank);
			int num7 = X.IntR(0.2f + X.NI(this.min_grade_for_grank(grank), 0f, num6) + (this.Con.XORSP() + this.Con.XORSP() + this.Con.XORSP() + this.Con.XORSP()) * 0.25f * X.NI(this.randomize_grade_for_grank(grank), 1.5f, num6));
			num7 = X.MMX(0, num7, 4);
			gqentry.value1 = (float)X.Mx(1, X.IntR(num5));
			gqentry.value2 = (float)num7;
			return gqentry;
		}

		private float min_grade_for_grank(int grank)
		{
			float num;
			if (grank != 0)
			{
				if (grank != 1)
				{
					num = 0.3f + (float)grank * 0.6f;
				}
				else
				{
					num = 1f;
				}
			}
			else
			{
				num = 0f;
			}
			return num;
		}

		private float randomize_grade_for_grank(int grank)
		{
			return 1f + (float)grank * 0.33f;
		}

		private float randomize_count(int grank)
		{
			return (float)grank * 2.15f + 0.8f;
		}

		private float randomize_after_multiple(int grank)
		{
			if (this.need_new_item)
			{
				return X.NIL(0.5f, 1f, (float)grank, 4f);
			}
			return 1f;
		}

		public override void getItemName(STB Stb, GuildManager.GQEntry Entry, NelItem I, int grade)
		{
			base.getItemName(Stb, Entry, I, grade);
			this.copyCollectTargetTo(Stb, Entry);
		}

		private void copyCollectTargetTo(STB Stb, GuildManager.GQEntry Entry)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					(Entry.TargetItem as NelItem).getLocalizedName(stb, (int)Entry.value2);
					Stb.AddTxA("Guild_entry_title_collect", false).TxRpl(stb);
					Stb.TxRpl(NelItem.getGradeMeshTxTo(stb2, (int)Entry.value2, 1, 34));
					Stb.TxRpl(Entry.value1);
				}
			}
		}

		public override void getItemDetail(STB Stb, GuildManager.GQEntry Entry, NelItem I, int grade)
		{
			base.getItemDetail(Stb, Entry, I, grade);
			using (STB stb = TX.PopBld(null, 0))
			{
				NelItem nelItem = Entry.TargetItem as NelItem;
				this.copyCollectTargetTo(stb, Entry);
				Stb.AddTxA("Guild_collect_detail", false);
				Stb.TxRpl(stb);
				if (!this.need_new_item)
				{
					Stb.Add(" ( ").AddTxA("item_current_obtain", false).TxRpl(this.Con.M2D.IMNG.countItem(nelItem, (int)Entry.value2, false, false))
						.Add(" )");
				}
				Stb.Ret("\n");
			}
		}

		public override void getItemDesc(STB Stb, GuildManager.GQEntry Entry, NelItem I, int grade)
		{
			base.getItemDesc(Stb, Entry, I, grade);
			using (STB stb = TX.PopBld(null, 0))
			{
				(Entry.TargetItem as NelItem).getLocalizedName(stb, (int)Entry.value2);
				Stb.TxRpl(stb);
			}
		}

		private static int fnSortEntry(List<NelItemEntry> A, List<NelItemEntry> B)
		{
			int num = A.Count - A[0].Data.price / 20;
			return B.Count - B[0].Data.price / 20 - num;
		}

		public override GuildManager.GQEntry readBinaryFrom(ByteReader Ba, WholeMapItem _WM, int vers)
		{
			GuildManager.GQEntry gqentry = base.readBinaryFrom(Ba, _WM, vers);
			NelItem nelItem;
			NelItem.readBinaryGetKey(Ba, out nelItem, false, false);
			gqentry.TargetItem = nelItem;
			gqentry.value1 = (float)Ba.readByte();
			gqentry.value2 = (float)Ba.readByte();
			if (gqentry.TargetItem == null)
			{
				return null;
			}
			return gqentry;
		}

		public override void writeBinaryTo(ByteArray Ba, GuildManager.GQEntry Gq)
		{
			base.writeBinaryTo(Ba, Gq);
			NelItem nelItem = Gq.TargetItem as NelItem;
			NelItem.writeBinaryItemKey(Ba, nelItem);
			Ba.writeByte((int)Gq.value1);
			Ba.writeByte((int)Gq.value2);
		}

		protected override QuestTracker.Quest createQT(GuildManager.GQEntry Entry, string map_key, out List<QuestTracker.QuestDeperture> AQd)
		{
			QuestTracker.Quest quest = base.createQT(Entry, map_key, out AQd);
			List<QuestTracker.QuestItemBufList> list = new List<QuestTracker.QuestItemBufList>(1);
			QuestTracker.QuestItemBufList questItemBufList = new QuestTracker.QuestItemBufList(1)
			{
				need_new_item = this.need_new_item
			};
			list.Add(questItemBufList);
			questItemBufList.Add(new NelItemEntry(Entry.TargetItem as NelItem, (int)Entry.value1, (byte)Entry.value2));
			bool flag = false;
			bool flag2 = !this.need_new_item;
			quest.finalizeLoading(AQd, list, ref flag, ref flag2);
			return quest;
		}

		public override void getDescriptionForQT(int phase, STB Stb, GuildManager.GQEntry Entry)
		{
			if (phase == 0)
			{
				Stb.AddTxA(this.need_new_item ? "subquest_gq_collect_new" : "subquest_gq_collect", false);
				return;
			}
			Stb.AddTxA("subquest_gq_report", false);
		}

		public override bool isQTFinishedAuto(GuildManager.GQEntry Entry, QuestTracker.QuestProgress Prog, bool auto_check, bool is_manual = false)
		{
			if (auto_check)
			{
				return false;
			}
			if (is_manual && !this.need_new_item)
			{
				NelItem nelItem = Entry.TargetItem as NelItem;
				return this.Con.M2D.IMNG.countItem(nelItem, (int)Entry.value2, true, true) >= (int)Entry.value1;
			}
			return base.isQTFinishedAuto(Entry, Prog, auto_check, is_manual);
		}

		public readonly bool need_new_item;
	}
}
