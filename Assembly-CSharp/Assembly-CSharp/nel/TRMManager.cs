using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public static class TRMManager
	{
		public static bool initTRMScript()
		{
			string resource = TX.getResource("Data/trm_data", ref TRMManager.LoadDate, ".csv", false, "Resources/");
			if (resource == null)
			{
				return false;
			}
			TRMManager.need_fine = true;
			TRMManager.OTri = new BDic<string, TRMManager.TRMItem>(1);
			TRMManager.OAHerbPower = new BDic<string, byte[]>(10);
			CsvReader csvReader = new CsvReader(resource, CsvReader.RegSpace, false);
			TRMManager.TRMItem trmitem = null;
			ushort num = 0;
			RecipeManager.Recipe recipe = RecipeManager.Get("aloma_water");
			while (csvReader.read())
			{
				if (csvReader.cmd == "#ID")
				{
					num = (ushort)csvReader.Int(1, 0);
				}
				else if (csvReader.cmd == "#RECIPE")
				{
					recipe = RecipeManager.Get(csvReader._1) ?? recipe;
				}
				else if (csvReader.cmd == "%POWER")
				{
					NelItem byId = NelItem.GetById(csvReader._1, false);
					if (byId != null)
					{
						byte[] array = new byte[5];
						TRMManager.OAHerbPower[byId.key] = array;
						for (int i = 0; i < 5; i++)
						{
							array[i] = (byte)csvReader.Int(2 + i, 0);
						}
					}
				}
				else if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					if (!TRMManager.OTri.TryGetValue(index, out trmitem))
					{
						Dictionary<string, TRMManager.TRMItem> otri = TRMManager.OTri;
						string text = index;
						string text2 = index;
						ushort num2 = num;
						num = num2 + 1;
						trmitem = (otri[text] = new TRMManager.TRMItem(text2, num2, recipe));
					}
					string text3 = "TrmItem_" + index;
					trmitem.RowItm = NelItem.GetById(text3, true);
					if (trmitem.RowItm == null)
					{
						trmitem.RowItm = NelItem.CreateItemEntry(text3, new NelItem(text3, 0, 200, 1)
						{
							category = (NelItem.CATEG)2097153U,
							FnGetName = new FnGetItemDetail(TRMManager.fnGetNameRecipe),
							FnGetDesc = new FnGetItemDetail(TRMManager.fnGetDescRecipe),
							FnGetDetail = new FnGetItemDetail(TRMManager.fnGetDetailRecipe),
							specific_icon_id = 42,
							SpecificColor = C32.d2c(4294942310U)
						}, (int)(62000 + trmitem.uid), false);
					}
				}
				else if (trmitem != null)
				{
					string cmd = csvReader.cmd;
					if (cmd != null)
					{
						if (!(cmd == "term"))
						{
							if (!(cmd == "watched_term"))
							{
								if (!(cmd == "reward_a"))
								{
									if (!(cmd == "reward_b"))
									{
										if (cmd == "herb")
										{
											trmitem.RcmHerb = NelItem.GetById(csvReader._1, false) ?? trmitem.RcmHerb;
											trmitem.rcm_count = csvReader.Int(2, trmitem.rcm_count);
											trmitem.rcm_grade = csvReader.Int(3, trmitem.rcm_grade);
										}
									}
									else
									{
										trmitem.reward_b = (ushort)csvReader.Int(1, 0);
										trmitem.reward_b_max = (ushort)csvReader.Int(2, global::XX.X.IntC((float)trmitem.reward_b * 1.5f));
									}
								}
								else
								{
									trmitem.reward_a = (ushort)csvReader.Int(1, 0);
									trmitem.reward_a_max = (ushort)csvReader.Int(2, global::XX.X.IntC((float)trmitem.reward_a * 1.5f));
								}
							}
							else
							{
								trmitem.Awatched_term = csvReader.slice(1, -1000);
							}
						}
						else
						{
							trmitem.term = csvReader.slice_join(1, " ", "");
						}
					}
				}
			}
			return true;
		}

		public static void newGame()
		{
			TRMManager.need_fine = (TRMManager.need_recheck_has_newer = true);
			TRMManager.has_newer = 0;
			if (TRMManager.OTri != null)
			{
				foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair in TRMManager.OTri)
				{
					keyValuePair.Value.newGame();
				}
			}
		}

		public static void fineExecute(bool force = false)
		{
			SCN.fine_pvv(false);
			TRMManager.initTRMScript();
			if (!TRMManager.need_fine && !force)
			{
				return;
			}
			TRMManager.need_fine = false;
			foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair in TRMManager.OTri)
			{
				TRMManager.TRMItem value = keyValuePair.Value;
				bool flag = TX.eval(value.term, "") != 0.0;
				if (value.Awatched_term != null && flag)
				{
					for (int i = value.Awatched_term.Length - 1; i >= 0; i--)
					{
						TRMManager.TRMItem trmitem = TRMManager.Get(value.Awatched_term[i], false);
						if (trmitem != null && !trmitem.watched_a)
						{
							flag = false;
							break;
						}
					}
				}
				if (value.is_active != flag)
				{
					value.is_active = flag;
					if (value.is_active)
					{
						value.TRecipe.RecipeItem.touchObtainCount();
						value.RowItm.touchObtainCount();
					}
					TRMManager.need_recheck_has_newer = true;
				}
			}
		}

		public static bool isAlomaRecipe(NelItem Itm)
		{
			return Itm.is_recipe && TX.isStart(Itm.key, "Recipe_", 0) && Itm.key == "Recipe_aloma_water";
		}

		public static byte hasNewerItem(bool force_recheck = false)
		{
			TRMManager.fineExecute(false);
			if (TRMManager.need_recheck_has_newer || force_recheck)
			{
				TRMManager.need_recheck_has_newer = false;
				TRMManager.has_newer = 0;
				foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair in TRMManager.OTri)
				{
					TRMManager.TRMItem value = keyValuePair.Value;
					if (value.is_active)
					{
						TRMManager.has_newer |= (value.watched_a ? 0 : 1);
						TRMManager.has_newer |= (value.watched_b ? 0 : 2);
						if (TRMManager.has_newer == 3)
						{
							break;
						}
					}
				}
			}
			return TRMManager.has_newer;
		}

		public static TRMManager.TRMItem Get(string key, bool no_error = false)
		{
			TRMManager.initTRMScript();
			TRMManager.TRMItem trmitem;
			if (!TRMManager.OTri.TryGetValue(key, out trmitem) && !no_error)
			{
				global::XX.X.de("不明な TRMItem: " + key, null);
			}
			return trmitem;
		}

		public static TRMManager.TRMItem GetByRowItm(NelItem Itm, bool no_error = false)
		{
			if (TRMManager.OTri == null)
			{
				TRMManager.initTRMScript();
			}
			foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair in TRMManager.OTri)
			{
				if (keyValuePair.Value.RowItm == Itm)
				{
					return keyValuePair.Value;
				}
			}
			if (!no_error)
			{
				global::XX.X.de("不明な Quest Itm: " + Itm.key, null);
			}
			return null;
		}

		public static TRMManager.TRMItem GetById(ushort id, bool no_error = false)
		{
			if (TRMManager.OTri == null)
			{
				TRMManager.initTRMScript();
			}
			foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair in TRMManager.OTri)
			{
				if (keyValuePair.Value.uid == id)
				{
					return keyValuePair.Value;
				}
			}
			if (!no_error)
			{
				global::XX.X.de("不明な Quest id: " + id.ToString(), null);
			}
			return null;
		}

		public static byte[] GetHerbPower(string key, bool no_error = false)
		{
			byte[] array;
			if (!TRMManager.OAHerbPower.TryGetValue(key, out array) && !no_error)
			{
				global::XX.X.de("不明な herb item key: " + key, null);
			}
			return array;
		}

		public static BDic<string, TRMManager.TRMItem> getWholeTriObject()
		{
			return TRMManager.OTri;
		}

		public static List<RecipeManager.RecipeDescription> listupDefinitionRecipe(ref List<RecipeManager.RecipeDescription> A, RecipeManager.Recipe TargetRecipe, bool only_useableItem = false)
		{
			foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair in TRMManager.OTri)
			{
				TRMManager.TRMItem value = keyValuePair.Value;
				if (value.TRecipe == TargetRecipe)
				{
					if (A == null)
					{
						A = new List<RecipeManager.RecipeDescription>(1);
					}
					A.Add(new RecipeManager.RecipeDescription(value.TRecipe, value.RowItm, (only_useableItem && !TRMManager.isTrmActive(value.RowItm, value)) ? "???" : value.getNameLocalized()));
				}
			}
			return A;
		}

		public static bool isTrmActive(NelItem Trm_RowItm, TRMManager.TRMItem Trm = null)
		{
			bool flag;
			if (!SCN.trm_enabled)
			{
				flag = false;
			}
			else
			{
				if (Trm == null)
				{
					Trm = TRMManager.GetByRowItm(Trm_RowItm, false);
				}
				flag = Trm != null && Trm.is_active;
			}
			return flag;
		}

		public static string fnGetNameRecipe(NelItem Itm, int grade, string def)
		{
			TRMManager.TRMItem trmitem = TRMManager.Get(TX.slice(Itm.key, "TrmItem_".Length), true);
			if (trmitem == null || !trmitem.is_active)
			{
				return "???";
			}
			return trmitem.getNameLocalized();
		}

		public static string fnGetDetailRecipe(NelItem Itm, int grade, string def)
		{
			TRMManager.TRMItem trmitem = TRMManager.Get(TX.slice(Itm.key, "TrmItem_".Length), true);
			if (trmitem == null || !trmitem.is_active)
			{
				return "???";
			}
			string localizedRecommendedItem = trmitem.getLocalizedRecommendedItem();
			return TX.GetA("TrmUi_recommend_aloma", localizedRecommendedItem);
		}

		public static string fnGetDescRecipe(NelItem Itm, int grade, string def)
		{
			TRMManager.TRMItem trmitem = TRMManager.Get(TX.slice(Itm.key, "TrmItem_".Length), true);
			if (trmitem == null || !trmitem.is_active)
			{
				return TX.Get("TrmUi_selector_detail_notactive", "");
			}
			string text = trmitem.icon_watched_a;
			string text2 = trmitem.icon_watched_b;
			string text3 = trmitem.reward_string_a;
			if (trmitem.watched_a)
			{
				text = "<font alpha=\"0.36\">" + text;
				text3 = text3 + "</font> " + TX.Get("TrmUi_selector_already_watched", "");
			}
			string text4 = trmitem.reward_string_b;
			if (trmitem.watched_b)
			{
				text2 = "<font alpha=\"0.36\">" + text2;
				text4 = text4 + "</font> " + TX.Get("TrmUi_selector_already_watched", "");
			}
			string text5 = TX.GetA("TrmUi_selector_detail", text, text3, text2, text4, TX.Get("TrmUi_root_a", ""), TX.Get("TrmUi_root_b", ""));
			if (trmitem.watched_all)
			{
				text5 = text5 + "\n\n" + TX.Get("TrmUi_unlocked_all", "");
			}
			return text5;
		}

		public static void readBinaryFrom(ByteArray Ba)
		{
			int num = Ba.readByte();
			int num2 = Ba.readByte();
			if (num2 > 0)
			{
				TRMManager.initTRMScript();
				for (int i = 0; i < num2; i++)
				{
					ushort num3 = Ba.readUShort();
					TRMManager.TRMItem.readBinaryFrom(Ba, num, TRMManager.GetById(num3, true));
				}
			}
		}

		public static void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(0);
			if (TRMManager.OTri == null)
			{
				Ba.writeByte(0);
				return;
			}
			int num = 0;
			foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair in TRMManager.OTri)
			{
				TRMManager.TRMItem value = keyValuePair.Value;
				if (value.watched_a || value.watched_b)
				{
					num++;
				}
			}
			Ba.writeByte(num);
			foreach (KeyValuePair<string, TRMManager.TRMItem> keyValuePair2 in TRMManager.OTri)
			{
				TRMManager.TRMItem value2 = keyValuePair2.Value;
				if (value2.watched_a || value2.watched_b)
				{
					Ba.writeUShort(value2.uid);
					value2.writeBinaryTo(Ba);
				}
			}
		}

		private static BDic<string, TRMManager.TRMItem> OTri;

		private static BDic<string, byte[]> OAHerbPower;

		private static DateTime LoadDate;

		public static bool need_fine;

		public static bool need_recheck_has_newer;

		private static byte has_newer;

		private const string default_recipe = "aloma_water";

		public const string row_item_header = "TrmItem_";

		public static TRMManager.TRMReward CurrentReward;

		public class TRMItem
		{
			public TRMItem(string _key, ushort _id, RecipeManager.Recipe _TRecipe)
			{
				this.uid = _id;
				this.key = _key;
				this.TRecipe = _TRecipe;
				this.RcmHerb = NelItem.GetById("mtr_sage", false);
			}

			public bool watched_all
			{
				get
				{
					return (this.watched & 3) == 3;
				}
			}

			public bool watched_a
			{
				get
				{
					return (this.watched & 1) == 1;
				}
				set
				{
					if (value)
					{
						this.watched |= 1;
					}
				}
			}

			public bool watched_b
			{
				get
				{
					return (this.watched & 2) == 2;
				}
				set
				{
					if (value)
					{
						this.watched |= 2;
					}
				}
			}

			public void newGame()
			{
				this.is_active = false;
				this.watched = 0;
				TRMManager.CurrentReward = null;
			}

			public static void readBinaryFrom(ByteArray Ba, int vers, TRMManager.TRMItem Target)
			{
				byte b = (byte)(Ba.readByte() & 3);
				if (Target != null)
				{
					Target.watched = b;
				}
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeByte((int)this.watched);
			}

			public string getNameLocalized()
			{
				return TX.Get("Trm_" + this.key, "");
			}

			public string getLocalizedRecommendedItem()
			{
				string text = this.RcmHerb.getLocalizedName(this.rcm_grade, null);
				if (this.rcm_grade > 0)
				{
					text = string.Concat(new string[]
					{
						text,
						"<img mesh=\"nel_item_grade.",
						this.rcm_grade.ToString(),
						"\" color=\"0x",
						C32.codeToCodeText(4283780170U),
						"\"/>"
					});
				}
				return text + " x " + this.rcm_count.ToString();
			}

			public string icon_watched_a
			{
				get
				{
					if (this.watched_a)
					{
						return "<img mesh=\"trm_heart_a_checked\" width=\"26\" height=\"22\" />";
					}
					return "<img mesh=\"trm_heart_a\" width=\"26\" height=\"22\" />";
				}
			}

			public string icon_watched_b
			{
				get
				{
					if (this.watched_b)
					{
						return "<img mesh=\"trm_heart_b_checked\" width=\"26\" height=\"22\" />";
					}
					return "<img mesh=\"trm_heart_b\" width=\"26\" height=\"22\" />";
				}
			}

			public string reward_string_a
			{
				get
				{
					return "<img mesh=\"money_icon\" width=\"24\" height=\"22\" />" + this.reward_a.ToString() + "..." + this.reward_a_max.ToString();
				}
			}

			public string reward_string_b
			{
				get
				{
					return "<img mesh=\"money_icon\" width=\"24\" height=\"22\" />" + this.reward_b.ToString() + "..." + this.reward_b_max.ToString();
				}
			}

			public readonly ushort uid;

			public readonly string key;

			public string term = "1";

			public string[] Awatched_term;

			public ushort reward_a = 300;

			public ushort reward_a_max = 450;

			public ushort reward_b = 100;

			public ushort reward_b_max = 150;

			public RecipeManager.Recipe TRecipe;

			public NelItem RowItm;

			public NelItem RcmHerb;

			public int rcm_count = 1;

			public int rcm_grade;

			public bool is_active;

			private byte watched;
		}

		public class TRMReward
		{
			public TRMReward(TRMManager.TRMItem _Target, int _grade, bool _success)
			{
				this.Target = _Target;
				this.grade = _grade;
				this.success = _success;
				this.base_reward = (int)(this.success ? this.Target.reward_a : this.Target.reward_b);
				this.chip_reward = (int)(this.success ? this.Target.reward_a_max : this.Target.reward_b_max) - this.base_reward;
				this.chip_reward = this.chip_reward * this.grade / 4;
			}

			public int total_reward
			{
				get
				{
					return this.base_reward + this.chip_reward;
				}
			}

			public string base_reward_spr5
			{
				get
				{
					return global::XX.X.spr0(this.base_reward, 5, ' ');
				}
			}

			public string chip_reward_spr5
			{
				get
				{
					return global::XX.X.spr0(this.chip_reward, 5, ' ');
				}
			}

			public string total_reward_spr5
			{
				get
				{
					return global::XX.X.spr0(this.total_reward, 5, ' ');
				}
			}

			public TRMManager.TRMItem Target;

			public int grade;

			public bool success = true;

			public int base_reward;

			public int chip_reward;
		}

		public enum HerbPow
		{
			cool,
			elegance,
			sweatness,
			spicy,
			bitter,
			_MAX
		}
	}
}
