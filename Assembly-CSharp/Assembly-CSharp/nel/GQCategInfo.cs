using System;
using System.Collections.Generic;
using m2d;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public class GQCategInfo
	{
		public GQCategInfo(GuildManager _Con, GuildManager.CATEG _categ, int _icon)
		{
			this.categ = _categ;
			this.Con = _Con;
			this.icon = _icon;
			this.reward_money_min = 50;
			this.reward_money_max = 150;
		}

		public void newGame()
		{
			this.count_success = (this.count_failed = 0);
		}

		public int getEntryCount(int grank)
		{
			if (this.FnGRank2EntryCount == null)
			{
				return 0;
			}
			return X.IntR(this.FnGRank2EntryCount.Get(-1f, (float)grank, false));
		}

		public virtual GuildManager.GQEntry createEntry(GuildManager.GQEntryList AList, WholeMapItem WM, int grank, ushort id)
		{
			NelItem nelItem = this.createItemEntry(grank, id);
			return new GuildManager.GQEntry(this.Con, WM, this.categ, nelItem, grank, this.Con.xors(this.comment_max));
		}

		public NelItem createItemEntry(int grank, ushort id)
		{
			return new NelItem("__guildquest_" + id.ToString(), this.reward_gp(grank), this.reward_money_default(grank), 1)
			{
				id = id,
				specific_icon_id = this.icon,
				category = (NelItem.CATEG)2097153U,
				SpecificColor = C32.d2c(4283780170U),
				max_grade_enpower = 1f,
				max_price_enpower = 1f
			};
		}

		public string categ_key_lower
		{
			get
			{
				return FEnum<GuildManager.CATEG>.ToStr(this.categ).ToLower();
			}
		}

		public string localized_categ_name
		{
			get
			{
				return TX.Get("GuildQ_categ_" + this.categ_key_lower, "");
			}
		}

		public virtual void getItemName(STB Stb, GuildManager.GQEntry Entry, NelItem I, int grade)
		{
			Stb.Clear();
			Stb.AddTxA("Guild_entry_title", false).TxRpl(this.localized_categ_name);
		}

		public virtual void getItemDesc(STB Stb, GuildManager.GQEntry Entry, NelItem I, int grade)
		{
			Stb.Clear();
			Stb.AddTxA("GuildQ_categ_comment_" + this.categ_key_lower + "_" + Entry.comment_id.ToString(), false);
		}

		public virtual void getItemDetail(STB Stb, GuildManager.GQEntry Entry, NelItem I, int grade)
		{
			Stb.Clear();
			Stb.AddTxA("GuildQ_categ_desc_" + this.categ_key_lower, false).Ret("\n").Ret("\n");
		}

		public void getItemDetailAfter(STB Stb, GuildManager.GQEntry Entry)
		{
			Stb.AddTxA("Guild_detail_obtain_rank", false);
			Stb.TxRpl(this.reward_gp(Entry.grade));
			Stb.TxRpl(this.lost_gp(Entry.grade));
		}

		public int reward_gp(int grade)
		{
			return X.IntR(X.NIL((float)this.reward_gp_min, (float)this.reward_gp_max, (float)grade, 4f));
		}

		public int lost_gp(int grade)
		{
			return X.IntR(this.lost_gp_ratio * X.NIL((float)this.reward_gp_min, (float)this.reward_gp_max, (float)grade, 4f));
		}

		public int reward_money_default(int grade)
		{
			return X.IntR(X.NIL((float)this.reward_money_min, (float)this.reward_money_max, (float)grade, 4f));
		}

		public virtual bool cannotReceive(GuildManager.GQEntry Entry, out string tx_key)
		{
			tx_key = null;
			return false;
		}

		public static void readCategDataBinaryFrom(ByteReader Ba, GQCategInfo Categ)
		{
			ushort num = Ba.readUShort();
			ushort num2 = Ba.readUShort();
			if (Categ != null)
			{
				Categ.count_success = (int)num;
				Categ.count_failed = (int)num2;
			}
		}

		public void writeCategDataBinaryTo(ByteArray Ba)
		{
			Ba.writeUShort((ushort)this.count_success);
			Ba.writeUShort((ushort)this.count_failed);
		}

		public virtual GuildManager.GQEntry readBinaryFrom(ByteReader Ba, WholeMapItem _WM, int vers)
		{
			int num = Ba.readByte();
			NelItem nelItem = this.createItemEntry(num, Ba.readUShort());
			GuildManager.GQEntry gqentry = new GuildManager.GQEntry(this.Con, _WM, this.categ, nelItem, num, Ba);
			if (vers >= 4)
			{
				gqentry.reward_money = (int)Ba.readUShort();
				gqentry.reward_etype = (ReelExecuter.ETYPE)Ba.readUByte();
				if (gqentry.reward_etype == ReelExecuter.ETYPE.ITEMKIND)
				{
					string text = Ba.readPascalString("utf-8", false);
					gqentry.RewardIR = this.Con.getIRByKey(text);
				}
			}
			return gqentry;
		}

		public virtual void writeBinaryTo(ByteArray Ba, GuildManager.GQEntry Gq)
		{
			Ba.writeByte(Gq.grade);
			Ba.writeUShort(Gq.ItemEntry.id);
			Gq.writeBasicDataToBinary(Ba);
			Ba.writeUShort((ushort)Gq.reward_money);
			Ba.writeByte((int)Gq.reward_etype);
			if (Gq.reward_etype == ReelExecuter.ETYPE.ITEMKIND)
			{
				if (Gq.RewardIR == null)
				{
					Ba.writeByte(0);
					return;
				}
				Ba.writePascalString(Gq.RewardIR.key, "utf-8");
			}
		}

		public virtual object getFieldGuideTarget(GuildManager.GQEntry Entry)
		{
			return Entry.TargetItem;
		}

		public virtual bool getFieldGuideTarget(int phase, GuildManager.GQEntry Entry, out object Target)
		{
			if (phase == 0 && Entry.TargetItem != null)
			{
				Target = Entry.TargetItem;
				return true;
			}
			Target = null;
			return false;
		}

		public QuestTracker.Quest createQT(GuildManager.GQEntry Entry, string map_key)
		{
			List<QuestTracker.QuestDeperture> list;
			return this.createQT(Entry, map_key, out list);
		}

		protected virtual QuestTracker.Quest createQT(GuildManager.GQEntry Entry, string map_key, out List<QuestTracker.QuestDeperture> AQd)
		{
			QuestTracker.Quest quest = new QuestTracker.Quest(Entry.qt_key, QuestTracker.CATEG.SUB, 65000, "Guildq");
			quest.desc_key = "";
			quest.end_phase = 2;
			quest.GQ = Entry;
			quest.calcPhase("^1");
			AQd = new List<QuestTracker.QuestDeperture>(2);
			AQd.Add(new QuestTracker.QuestDeperture
			{
				wm_key = Entry.WM.text_key
			});
			if (TX.valid(map_key))
			{
				Map2d map2d = this.Con.M2D.Get(map_key, false);
				if (map2d != null)
				{
					WholeMapItem wholeFor = this.Con.M2D.WM.GetWholeFor(map2d, false);
					if (wholeFor != null)
					{
						map2d.prepared = true;
						string s = map2d.Meta.GetS("wholemap_pos");
						string text = "";
						if (TX.valid(s))
						{
							text = map_key;
							map_key = "@" + s;
						}
						AQd.Add(new QuestTracker.QuestDeperture
						{
							wm_key = wholeFor.text_key,
							map_key = map_key,
							real_map_target = text
						});
					}
				}
			}
			return quest;
		}

		public virtual void getDescriptionForQT(int phase, STB Stb, GuildManager.GQEntry Entry)
		{
		}

		public virtual bool isQTFinishedAuto(GuildManager.GQEntry Entry, QuestTracker.QuestProgress Prog, bool auto_check, bool is_manual = false)
		{
			return Prog.phase >= 1;
		}

		public GuildManager Con;

		public readonly GuildManager.CATEG categ;

		public int reward_money_min;

		public int reward_money_max;

		public int reward_gp_min = 1;

		public int reward_gp_max = 9;

		public int comment_max = 1;

		public float lost_gp_ratio = 1f;

		public bool auto_abort_qt_in_other_battle = true;

		public bool auto_abort_in_progressing = true;

		public bool auto_abort_in_flush_area = true;

		public bool auto_abort_in_flush_area_itemcollect;

		public readonly int icon;

		public const int GRADE_MAX = 5;

		public EfParticleFuncCalc FnGRank2EntryCount;

		public const string itemheader_quest_item = "__guildquest_";

		public bool show_target_without_know_icon;

		public int count_success;

		public int count_failed;
	}
}
