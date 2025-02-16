using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class GQCategInfoDefeat : GQCategInfo
	{
		public GQCategInfoDefeat(GuildManager _Con, GuildManager.CATEG _categ, int _icon)
			: base(_Con, _categ, _icon)
		{
			this.show_target_without_know_icon = true;
		}

		protected int dlevel_add(float grank)
		{
			return X.Mx(4, X.IntR(this.NILP(13f, 60f, grank, 4f)));
		}

		protected int norma_danger(float grank)
		{
			return X.Mx(3, X.IntR(this.NILP(5f, 11f, grank, 4f)));
		}

		protected int norma_max_battle_count(float grank)
		{
			return X.MMX(1, 2 + X.IntR(grank), 5);
		}

		protected float nattr_add_ratio(float grank)
		{
			if (grank == 0f)
			{
				return 0.125f;
			}
			return this.NILP(0.3f, 0.7f, grank - 1f, 3f);
		}

		protected float NILP(float a, float b, float r, float m)
		{
			if (r < 0f || r >= 1f)
			{
				return X.NI(a, b, r / m);
			}
			return X.NILP(a, b, r, m);
		}

		protected float weather_add_ratio(float grank)
		{
			if (grank == 0f)
			{
				return 0f;
			}
			return this.NILP(0.125f, 0.65f, grank - 1f, 3f);
		}

		protected float fix_enemy_ratio(float grank)
		{
			return this.NILP(0.6f, 0.8f, grank - 1f, 5f);
		}

		protected ENATTR attachable_nattr(int grank)
		{
			if (grank >= 2)
			{
				return ENATTR.ATK | ENATTR.DEF | ENATTR.MP_STABLE | ENATTR.FIRE | ENATTR.ICE | ENATTR.THUNDER | ENATTR.SLIMY | ENATTR.ACME | ENATTR.INVISIBLE;
			}
			return ENATTR.ATK | ENATTR.DEF | ENATTR.FIRE | ENATTR.ICE | ENATTR.THUNDER | ENATTR.SLIMY | ENATTR.ACME;
		}

		protected int attachable_nattr_kind_max(ref float grank, ref float reduce_level)
		{
			float num = this.Con.XORSP();
			if (grank >= 4f && num < 0.21f)
			{
				grank -= X.NI(2.4f, 2.8f, this.Con.XORSP());
				reduce_level = 3f;
				return 3;
			}
			if (grank >= 2f && num < X.NIL(0.25f, 0.5f, grank - 2f, 2f))
			{
				grank -= X.NI(1.6f, 2f, this.Con.XORSP());
				reduce_level = 2f;
				return 2;
			}
			grank -= X.NI(0.8f, 1f, this.Con.XORSP());
			return 1;
		}

		public override GuildManager.GQEntry createEntry(GuildManager.GQEntryList AList, WholeMapItem WM, int grank, ushort id)
		{
			GuildManager.GQEntry gqentry = base.createEntry(AList, WM, grank, id);
			float num = (float)grank;
			float num2 = (float)grank;
			float num3 = (float)grank;
			ENATTR enattr = ENATTR.NORMAL;
			WeatherItem.WEATHER weather = WeatherItem.WEATHER.NORMAL;
			if (this.Con.XORSP() < this.nattr_add_ratio(num3))
			{
				float num4 = 1f;
				enattr = EnemyAttr.attachKind(this.attachable_nattr((int)num3), this.Con.XORSP(), this.attachable_nattr_kind_max(ref num3, ref num4));
				float num5 = this.Con.XORSP();
				num -= num5 * num4;
				num2 -= (1f - num5) * num4;
			}
			if (this.Con.XORSP() < this.weather_add_ratio(num3))
			{
				num3 -= X.NI(0.5f, 0.7f, this.Con.XORSP());
				weather = WeatherItem.WEATHER.WIND + (uint)this.Con.xors(6);
				float num6;
				if (weather != WeatherItem.WEATHER.WIND)
				{
					if (weather == WeatherItem.WEATHER.THUNDER)
					{
						num6 = 1.3f;
					}
					else
					{
						num6 = 0.86f;
					}
				}
				else
				{
					num6 = 2f;
				}
				float num7 = num6;
				float num8 = this.Con.XORSP();
				num -= num8 * num7;
				num2 -= (1f - num8) * num7;
			}
			int num9 = -1;
			EnemySummonerManager manager = EnemySummonerManager.GetManager(WM.text_key);
			manager.initializeMti();
			if (this.Con.XORSP() < this.fix_enemy_ratio((float)grank))
			{
				float num10 = 0f;
				foreach (KeyValuePair<string, Vector2> keyValuePair in manager.Oenemy_weight)
				{
					if (keyValuePair.Value.x > 0f && keyValuePair.Value.y > 0f)
					{
						num10 += keyValuePair.Value.y;
					}
				}
				if (num10 > 0f)
				{
					float num11 = this.Con.XORSP() * num10;
					foreach (KeyValuePair<string, Vector2> keyValuePair2 in manager.Oenemy_weight)
					{
						if (keyValuePair2.Value.x > 0f && keyValuePair2.Value.y > 0f)
						{
							num11 -= keyValuePair2.Value.y;
							if (num11 < 0f)
							{
								NDAT.EnemyDescryption typeAndId = NDAT.getTypeAndId(keyValuePair2.Key);
								if (!typeAndId.valid)
								{
									X.de("EnemyKind が不明: " + keyValuePair2.Key, null);
									break;
								}
								num9 = typeAndId.id;
								float num12 = X.NIL(0.25f, 1.5f, (keyValuePair2.Value.x - 1f) / 2f, 1f);
								num2 -= num12 + 0.3f;
								num += 0.3f;
								break;
							}
						}
					}
				}
			}
			if (num2 > 0f && this.Con.XORSP() < 0.5f)
			{
				float num13 = X.Mn(num2, 0.4f + this.Con.XORSP() * 0.8f);
				num += num13;
				num2 -= num13;
			}
			int num14 = this.norma_danger(num2);
			BDic<string, EnemySummonerManager.SDescription> bdic = manager.listupAll();
			int num15 = this.norma_max_battle_count(num2);
			SummonerList summonerList = new SummonerList(num14 / 2);
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				manager.listupAllSummonerKey(blist, true);
				int num16 = num14;
				int num17 = blist.Count;
				int num18 = 0;
				int num19 = 0;
				while (num16 > 0 && num17 > 0 && summonerList.Count < num15)
				{
					int num20 = this.Con.xors(num17);
					EnemySummonerManager.SDescription sdescription = bdic[blist[num20]];
					EnemySummoner smn = this.getSmn(blist[num20]);
					if (smn.grade >= 5 || smn.grade <= 0 || sdescription.no_choose_guild)
					{
						blist.RemoveAt(num20);
						num17--;
					}
					else if (!this.Con.isUseableSummoner(WM.text_key, smn))
					{
						blist.RemoveAt(num20);
						num17--;
						if (++num19 >= 80)
						{
							break;
						}
					}
					else if (num18++ >= 10 || smn.grade <= num16)
					{
						num18 = 0;
						summonerList.Add(blist[num20]);
						blist.RemoveAt(num20);
						num17--;
						num16 -= smn.grade;
					}
				}
				num += 0.15f * (float)num16;
			}
			if (summonerList.Count == 0)
			{
				gqentry.destruct();
				return null;
			}
			int num21 = this.dlevel_add(num);
			gqentry.TargetItem = summonerList;
			summonerList.enemyid_fix = (ENEMYID)X.Mx(num9, 0);
			gqentry.value1 = enattr;
			gqentry.value2 = (float)(((int)weather << 16) | (WeatherItem.WEATHER)(num21 << 8) | (WeatherItem.WEATHER)num14);
			gqentry.value3 = (float)num9;
			return gqentry;
		}

		public int getEntryDlevelAdd(GuildManager.GQEntry Gq)
		{
			return ((int)Gq.value2 >> 8) & 255;
		}

		public int getEntryNolmaDanger(GuildManager.GQEntry Gq)
		{
			return (int)Gq.value2 & 255;
		}

		public int getEntryNAttrMax(GuildManager.GQEntry Gq)
		{
			return 3 + Gq.grade;
		}

		public WeatherItem.WEATHER getEntryWeather(GuildManager.GQEntry Gq)
		{
			return (WeatherItem.WEATHER)(((int)Gq.value2 >> 16) & 255);
		}

		public EnemySummoner getSmn(string key)
		{
			EnemySummoner enemySummoner = EnemySummoner.Get(key, false);
			if (enemySummoner != null)
			{
				enemySummoner.initializeCR(null, false);
			}
			return enemySummoner;
		}

		public override GuildManager.GQEntry readBinaryFrom(ByteReader Ba, WholeMapItem _WM, int vers)
		{
			GuildManager.GQEntry gqentry = base.readBinaryFrom(Ba, _WM, vers);
			int num = Ba.readByte();
			SummonerList summonerList = new SummonerList(num);
			gqentry.TargetItem = summonerList;
			for (int i = 0; i < num; i++)
			{
				summonerList.Add(Ba.readPascalString("utf-8", false));
			}
			gqentry.value1 = (float)Ba.readInt();
			gqentry.value2 = (float)Ba.readInt();
			gqentry.value3 = (float)Ba.readInt();
			summonerList.enemyid_fix = (ENEMYID)X.Mx((int)gqentry.value3, 0);
			return gqentry;
		}

		public override void writeBinaryTo(ByteArray Ba, GuildManager.GQEntry Gq)
		{
			base.writeBinaryTo(Ba, Gq);
			SummonerList summonerList = Gq.TargetItem as SummonerList;
			int count = summonerList.Count;
			Ba.writeByte(count);
			for (int i = 0; i < count; i++)
			{
				string text = summonerList[i];
				Ba.writePascalString(text, "utf-8");
			}
			Ba.writeInt((int)Gq.value1);
			Ba.writeInt((int)Gq.value2);
			Ba.writeInt((int)Gq.value3);
		}

		public override void getItemName(STB Stb, GuildManager.GQEntry Entry, NelItem I, int grade)
		{
			base.getItemName(Stb, Entry, I, grade);
			SummonerList summonerList = Entry.TargetItem as SummonerList;
			Stb.AddTxA("subquest_gq_defeat_count", false).TxRpl(summonerList.Count);
			using (STB stb = TX.PopBld(null, 0))
			{
				this.copyAdditional(stb, Entry, false);
				if (stb.Length > 0)
				{
					Stb.Add(" ").Add(stb);
				}
			}
		}

		private void copyAdditional(STB Stb, GuildManager.GQEntry Entry, bool detailed_nattr = false)
		{
			if (detailed_nattr)
			{
				Stb.AddTxA("subquest_gq_defeat_dangerousness", false).TxRpl(this.getEntryDlevelAdd(Entry)).Ret("\n");
			}
			if (Entry.value1 > 0f)
			{
				if (detailed_nattr)
				{
					using (STB stb = TX.PopBld(null, 0))
					{
						uint num = (uint)Entry.value1;
						uint num2 = 1U;
						while (num2 < 131072U && num != 0U)
						{
							if ((num & num2) != 0U)
							{
								num &= ~num2;
								stb.Append(EnemyAttr.getLocalizedName((ENATTR)num2), ",");
							}
							num2 <<= 1;
						}
						Stb.AddTxA("subquest_gq_defeat_attr_detail", false).TxRpl(stb);
						goto IL_00A1;
					}
				}
				Stb.AddTxA("subquest_gq_defeat_attr", false);
			}
			IL_00A1:
			WeatherItem.WEATHER entryWeather = this.getEntryWeather(Entry);
			if (entryWeather > WeatherItem.WEATHER.NORMAL)
			{
				Stb.AddTxA("subquest_gq_defeat_weather", false);
				Stb.TxRpl(TX.Get("Weather_" + entryWeather.ToString().ToLower(), ""));
			}
			if (Entry.value3 >= 0f)
			{
				Stb.AddTxA("subquest_gq_defeat_fix_enemy", false);
				Stb.TxRpl(NDAT.getEnemyName((ENEMYID)Entry.value3, true));
			}
		}

		public override void getItemDetail(STB Stb, GuildManager.GQEntry Entry, NelItem I, int grade)
		{
			base.getItemDetail(Stb, Entry, I, grade);
			Stb.AddTxA("subquest_gq_defeat", false).Add(":").Ret("\n")
				.Add("  ");
			SummonerList summonerList = Entry.TargetItem as SummonerList;
			int count = summonerList.Count;
			for (int i = 0; i < count; i++)
			{
				if (i > 0)
				{
					Stb.Add(", ");
				}
				Stb.AddTxA("Summoner_" + summonerList[i], false);
			}
			Stb.Ret("\n");
			this.copyAdditional(Stb, Entry, true);
			Stb.Ret("\n");
		}

		public bool hasSummoner(GuildManager.GQEntry Entry, string smn_key)
		{
			SummonerList summonerList = Entry.TargetItem as SummonerList;
			return summonerList != null && X.isinStr(summonerList, smn_key, -1) >= 0;
		}

		public override bool cannotReceive(GuildManager.GQEntry Entry, out string tx_key)
		{
			SummonerList summonerList = Entry.TargetItem as SummonerList;
			SupplyManager.SupplyDescription supplyDescription = SupplyManager.AddSummoner(this.Con.M2D, Entry.WM, summonerList, default(SupplyManager.SupplyDescription), false);
			if (supplyDescription.APosSmn == null || supplyDescription.APosSmn.Count < summonerList.Count)
			{
				tx_key = "Guild_alert_cannotreceive_not_discovered";
				return true;
			}
			tx_key = null;
			return false;
		}

		public bool isDefeated(GuildManager.GQEntry Entry, string smn_key)
		{
			if (Entry.Qt == null)
			{
				return false;
			}
			QuestTracker.QuestProgress progressObject = this.Con.M2D.QUEST.getProgressObject(Entry.Qt);
			return progressObject != null && progressObject.summonerAlreadyDefeated(0, smn_key);
		}

		protected override QuestTracker.Quest createQT(GuildManager.GQEntry Entry, string map_key, out List<QuestTracker.QuestDeperture> AQd)
		{
			QuestTracker.Quest quest = base.createQT(Entry, map_key, out AQd);
			SummonerList summonerList = Entry.TargetItem as SummonerList;
			QuestTracker.QuestDeperture questDeperture = AQd[0];
			QuestTracker.SummonerEntry[] array = new QuestTracker.SummonerEntry[summonerList.Count];
			questDeperture.AItm = array;
			QuestTracker.SummonerEntry summonerEntry = new QuestTracker.SummonerEntry("", quest.key, this.getEntryDlevelAdd(Entry), this.getEntryWeather(Entry), (ENATTR)Entry.value1, (int)Entry.value3, (Entry.value1 > 0f) ? this.getEntryNAttrMax(Entry) : 0);
			for (int i = 0; i < summonerList.Count; i++)
			{
				summonerEntry.summoner_key = summonerList[i];
				array[i] = summonerEntry;
			}
			AQd[0] = questDeperture;
			bool flag = false;
			bool flag2 = false;
			quest.finalizeLoading(AQd, null, ref flag, ref flag2);
			return quest;
		}

		public override void getDescriptionForQT(int phase, STB Stb, GuildManager.GQEntry Entry)
		{
			if (phase == 0)
			{
				Stb.Add(base.localized_categ_name);
				this.copyAdditional(Stb, Entry, false);
				return;
			}
			Stb.AddTxA("subquest_gq_report", false);
		}
	}
}
