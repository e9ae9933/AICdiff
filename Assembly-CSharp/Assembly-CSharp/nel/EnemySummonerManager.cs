using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemySummonerManager
	{
		public static EnemySummonerManager GetManager(string header_key)
		{
			EnemySummonerManager enemySummonerManager;
			if (!EnemySummonerManager.OManage.TryGetValue(header_key, out enemySummonerManager))
			{
				if (!MTI.existsFile("summon/" + header_key))
				{
					return null;
				}
				enemySummonerManager = (EnemySummonerManager.OManage[header_key] = new EnemySummonerManager(header_key));
			}
			return enemySummonerManager;
		}

		public static void releaseAll()
		{
			foreach (KeyValuePair<string, EnemySummonerManager> keyValuePair in EnemySummonerManager.OManage)
			{
				keyValuePair.Value.releaseMtiContent();
			}
		}

		public static BDic<WholeMapItem, SummonerList> getAppearListForWholeMaps(NelM2DBase M2D, ENEMYID id, BDic<WholeMapItem, SummonerList> ORet = null)
		{
			Dictionary<string, WholeMapItem> wholeMapDescriptionObject = M2D.WM.getWholeMapDescriptionObject();
			ORet = ORet ?? new BDic<WholeMapItem, SummonerList>();
			foreach (KeyValuePair<string, WholeMapItem> keyValuePair in wholeMapDescriptionObject)
			{
				string text_key = keyValuePair.Value.text_key;
				if (M2D.WA.isActivated(text_key))
				{
					EnemySummonerManager manager = EnemySummonerManager.GetManager(text_key);
					if (manager != null)
					{
						ORet[keyValuePair.Value] = manager.getAppearList(id);
					}
				}
			}
			return ORet;
		}

		public EnemySummonerManager(string _header_key)
		{
			this.header_key = _header_key;
			this.Oenemy_weight = new BDic<string, Vector2>();
			this.OAenemy2list = new BDic<ENEMYID, SummonerList>();
			this.Mti = MTI.LoadContainer("summon/" + this.header_key, null);
		}

		public void destruct()
		{
			this.releaseMtiContent();
		}

		private void releaseMtiContent()
		{
			this.Mti.remLoadKey(" _SUMMONER");
		}

		public void initializeMti()
		{
			if (this.Mti.hasLoadKey("_SUMMONER"))
			{
				return;
			}
			this.Mti.addLoadKey("_SUMMONER", false);
			if (this.OScriptDesc == null)
			{
				this.OScriptDesc = new BDic<string, EnemySummonerManager.SDescription>();
				this.Oenemy_weight.Clear();
				string scriptRaw = this.getScriptRaw("_description.csv", true);
				if (TX.valid(scriptRaw))
				{
					CsvReader csvReader = new CsvReader(scriptRaw, CsvReader.RegSpace, false);
					EnemySummonerManager.SDescription sdescription = default(EnemySummonerManager.SDescription);
					string text = null;
					string text2 = "##";
					while (csvReader.read())
					{
						if (csvReader.cmd == "##IGNORE_HEADER")
						{
							this.ignore_header = true;
						}
						else if (TX.isStart(csvReader.cmd, "##", 0))
						{
							text2 = csvReader.cmd;
						}
						else if (text2 == "##ENEMY")
						{
							float num = csvReader.Nm(1, 0f);
							float num2 = csvReader.Nm(2, -1f);
							if (num2 < 0f)
							{
								num2 = ((num <= 0f) ? 0f : (1f / X.Abs(num)));
							}
							this.Oenemy_weight[csvReader.cmd] = new Vector2(num, num2);
						}
						else if (text2 == "##")
						{
							if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
							{
								if (sdescription.valid)
								{
									this.OScriptDesc[text] = sdescription;
									sdescription = default(EnemySummonerManager.SDescription);
								}
								text = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
								if (!this.OScriptDesc.TryGetValue(text, out sdescription))
								{
									Dictionary<string, EnemySummonerManager.SDescription> oscriptDesc = this.OScriptDesc;
									string text3 = text;
									EnemySummonerManager.SDescription sdescription2 = new EnemySummonerManager.SDescription(this.header_key, text, null);
									oscriptDesc[text3] = sdescription2;
									sdescription = sdescription2;
								}
							}
							else if (sdescription.valid)
							{
								string cmd = csvReader.cmd;
								if (cmd != null)
								{
									if (!(cmd == "map_overwrite"))
									{
										if (!(cmd == "not_appear"))
										{
											if (cmd == "no_choose_guild")
											{
												sdescription.no_choose_guild = true;
											}
										}
										else
										{
											sdescription.not_appear = true;
										}
									}
									else
									{
										sdescription.map_overwrite = csvReader._1;
									}
								}
							}
						}
					}
					if (sdescription.valid)
					{
						this.OScriptDesc[text] = sdescription;
						sdescription = default(EnemySummonerManager.SDescription);
					}
				}
			}
		}

		public EnemySummonerManager.SDescription getSummonerDescription(string summoner_key, bool no_make = true)
		{
			this.initializeMti();
			EnemySummonerManager.SDescription sdescription;
			if (!this.OScriptDesc.TryGetValue(summoner_key, out sdescription))
			{
				EnemySummonerManager.SDescription sdescription2;
				if (no_make || (!this.ignore_header && !TX.isStart(summoner_key, this.header_key, 0)))
				{
					sdescription2 = default(EnemySummonerManager.SDescription);
					return sdescription2;
				}
				Dictionary<string, EnemySummonerManager.SDescription> oscriptDesc = this.OScriptDesc;
				sdescription2 = new EnemySummonerManager.SDescription(this.header_key, summoner_key, null);
				oscriptDesc[summoner_key] = sdescription2;
				sdescription = sdescription2;
			}
			return sdescription;
		}

		public string getSummonerScript(string summoner_key, out EnemySummonerManager.SDescription Desc, bool no_make = false)
		{
			Desc = this.getSummonerDescription(summoner_key, no_make);
			return this.getScriptRaw(Desc.filename, false);
		}

		private string getScriptRaw(string basename, bool no_error = false)
		{
			TextAsset textAsset = this.Mti.Load<TextAsset>(basename);
			if (!(textAsset != null))
			{
				return null;
			}
			return textAsset.text;
		}

		public BDic<string, EnemySummonerManager.SDescription> listupAll()
		{
			if (this.listuped)
			{
				return this.OScriptDesc;
			}
			this.listuped = true;
			this.initializeMti();
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				this.Mti.listUpAllFiles(null, ".summon.csv", blist);
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					string text = X.basename(blist[i]);
					string text2 = TX.slice(text, 0, text.Length - ".summon.csv".Length);
					EnemySummonerManager.SDescription sdescription;
					if (!this.OScriptDesc.TryGetValue(text2, out sdescription))
					{
						sdescription = (this.OScriptDesc[text2] = new EnemySummonerManager.SDescription(this.header_key, text2, text));
					}
				}
			}
			return this.OScriptDesc;
		}

		public List<string> listupAllSummonerKey(List<string> Adest, bool only_correct_header = false)
		{
			this.listupAll();
			foreach (KeyValuePair<string, EnemySummonerManager.SDescription> keyValuePair in this.OScriptDesc)
			{
				if (!keyValuePair.Value.not_appear && (!only_correct_header || this.ignore_header || TX.isStart(keyValuePair.Key, this.header_key, 0)))
				{
					Adest.Add(keyValuePair.Key);
				}
			}
			return Adest;
		}

		public Vector2 getEnemyPower(string key)
		{
			this.initializeMti();
			Vector2 vector;
			if (this.Oenemy_weight.TryGetValue(key, out vector))
			{
				return vector;
			}
			return new Vector2(1f, 1f);
		}

		public SummonerList getAppearList(ENEMYID id)
		{
			SummonerList summonerList;
			if (this.OAenemy2list.TryGetValue(id, out summonerList))
			{
				return summonerList;
			}
			summonerList = (this.OAenemy2list[id] = new SummonerList(4));
			Dictionary<string, EnemySummonerManager.SDescription> dictionary = this.listupAll();
			Regex regex = null;
			Regex regex2 = null;
			PuppetRevenge.getRegexForSummonerAppearList(id, ref regex2);
			if ((id & (ENEMYID)2147483648U) != (ENEMYID)0U)
			{
				id &= (ENEMYID)2147483647U;
				regex = regex ?? new Regex("\\n[ \\,\\t\\s]*%EN_OD[ \\,\\t\\s]+" + id.ToString());
			}
			else
			{
				regex = regex ?? new Regex("\\n[ \\,\\t\\s]*%EN[ \\,\\t\\s]+" + id.ToString());
			}
			foreach (KeyValuePair<string, EnemySummonerManager.SDescription> keyValuePair in dictionary)
			{
				if (!keyValuePair.Value.not_appear)
				{
					string scriptRaw = this.getScriptRaw(keyValuePair.Value.filename, false);
					if (!TX.noe(scriptRaw))
					{
						if (regex.Match(scriptRaw).Success)
						{
							summonerList.Add(keyValuePair.Key);
						}
						else if (regex2 != null && regex2.Match(scriptRaw).Success)
						{
							summonerList.Add(keyValuePair.Key);
						}
					}
				}
			}
			return summonerList;
		}

		public bool getWMPosition(NelM2DBase M2D, string smn_key, WholeMapItem WMTarget, out WMIconPosition WmPos, bool only_if_appeared = true, bool only_icon = false)
		{
			WmPos = default(WMIconPosition);
			EnemySummonerManager.SDescription summonerDescription = this.getSummonerDescription(smn_key, false);
			if (!summonerDescription.valid)
			{
				return false;
			}
			string text = summonerDescription.map_target(smn_key);
			Map2d map2d = M2D.Get(text, true);
			if (map2d == null)
			{
				return false;
			}
			WholeMapItem.WMItem wmi = WMTarget.GetWmi(map2d, null);
			if (wmi == null)
			{
				return false;
			}
			if (only_if_appeared && !WMTarget.isAppeared(map2d, false))
			{
				return false;
			}
			List<WMIcon> list;
			if (WMTarget.getNoticedIconObject().TryGetValue(map2d, out list))
			{
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					WMIcon wmicon = list[i];
					if (wmicon.isForSummoner(smn_key))
					{
						WmPos = new WMIconPosition(wmicon, wmi, default(WMIconHiddenDeperture));
						return WmPos.getDepertureMap() != null;
					}
				}
			}
			if (only_icon)
			{
				return false;
			}
			WmPos = new WMIconPosition(wmi);
			return WmPos.getDepertureMap() != null;
		}

		private static BDic<string, EnemySummonerManager> OManage = new BDic<string, EnemySummonerManager>();

		public readonly string header_key;

		private MTI Mti;

		private BDic<string, EnemySummonerManager.SDescription> OScriptDesc;

		public readonly BDic<string, Vector2> Oenemy_weight;

		public readonly BDic<ENEMYID, SummonerList> OAenemy2list;

		private bool listuped;

		private bool ignore_header;

		private const string top_directory = "summon/";

		private const string description_file = "_description.csv";

		private const string extension_full = ".summon.csv";

		public const string key_header_for_puppetrevenge = "_other";

		public struct SDescription
		{
			public bool valid
			{
				get
				{
					return this.filename != null;
				}
			}

			public SDescription(string header_key, string summoner_key, string _filename = null)
			{
				this.filename = _filename ?? (summoner_key + ".summon.csv");
				this.map_overwrite = null;
				this.not_appear = (this.no_choose_guild = false);
			}

			public string map_target(string summoner_key)
			{
				if (!TX.valid(this.map_overwrite))
				{
					return summoner_key;
				}
				return this.map_overwrite;
			}

			public string filename;

			public string map_overwrite;

			public bool not_appear;

			public bool no_choose_guild;
		}
	}
}
