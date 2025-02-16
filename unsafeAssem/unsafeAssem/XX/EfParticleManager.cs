using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using UnityEngine;

namespace XX
{
	public class EfParticleManager
	{
		private static string getParticleScript(string name)
		{
			string text = null;
			if (TX.noe(text))
			{
				TextAsset textAsset = Resources.Load<TextAsset>(EfParticleManager.particle_dir + name + EfParticleManager.particle_ext);
				if (textAsset != null)
				{
					text = textAsset.text;
				}
			}
			if (TX.noe(text))
			{
				X.dl("EfParticleManager::getParticleScript テキストの読み込みに失敗:" + name, null, false, false);
				return null;
			}
			return text;
		}

		public static void accessableOtherFile(params string[] A)
		{
			if (EfParticleManager.Aaccessable_other_file == null)
			{
				EfParticleManager.Aaccessable_other_file = new List<string>(A.Length);
			}
			EfParticleManager.Aaccessable_other_file.AddRange(A);
			EfParticleManager.do_not_access_other_file = true;
		}

		public static void addAdditionalFile(string _key, TextAsset Tx = null, List<string> Aother_load = null)
		{
			if (EfParticleManager.Aload_other_file == null)
			{
				EfParticleManager.Aload_other_file = new List<string>(1);
			}
			if (X.pushIdentical<string>(EfParticleManager.Aload_other_file, _key) && EfParticleManager.OPtc != null)
			{
				EfParticleManager.loadParticleCsv((Tx == null) ? EfParticleManager.getParticleScript(_key) : Tx.text, null, Aother_load);
			}
		}

		public static void remAdditionalFile(string _key)
		{
			EfParticleManager.Aload_other_file.Remove(_key);
		}

		public static void reloadParticleCsv(bool reload = false)
		{
			if (!reload && EfParticleManager.initted)
			{
				return;
			}
			string particleScript = EfParticleManager.getParticleScript(EfParticleManager.particle_main_script_name);
			if (particleScript == null)
			{
				return;
			}
			EfParticle.initEfParticle();
			EfParticleManager.APtcO = null;
			BDic<string, EfParticle> optc = EfParticleManager.OPtc;
			EfParticleManager.OPtc = null;
			EfParticleManager.OPtcSetter = null;
			EfParticleManager.reload_count++;
			EfParticleManager.loadParticleCsv(particleScript, optc, null);
		}

		private static void loadParticleCsv(string data_text = null, BDic<string, EfParticle> OPtcPre = null, List<string> Aother_load = null)
		{
			if (data_text == null)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			if (EfParticleManager.APtcO == null)
			{
				flag = true;
				EfParticleManager.APtcO = new List<EfParticleLoader>(256);
			}
			if (EfParticleManager.OPtcSetter == null)
			{
				EfParticleManager.OPtcSetter = new BDic<string, EfSetterP>();
				EfParticleManager.OAgd = new BDic<string, AttackGhostDrawer>();
				flag2 = true;
			}
			EfParticleLoader efParticleLoader = null;
			EfSetterP efSetterP = null;
			AttackGhostDrawer attackGhostDrawer = null;
			bool flag3 = true;
			CsvReader csvReader = new CsvReader(null, CsvReader.RegSpace, true);
			csvReader.no_replace_quote = false;
			csvReader.no_write_varcon = 0;
			CsvVariableContainer varCon = csvReader.VarCon;
			StbReader stbReader = new StbReader(64, data_text);
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.EnsureCapacity(512);
				using (STB stb2 = TX.PopBld(null, 0))
				{
					int num;
					int num2;
					while (stbReader.readCorrectlyNoEmpty(out num, out num2, true))
					{
						if (stbReader.IsSectionHeader(num, stb2, num2))
						{
							flag3 = true;
							if (efParticleLoader != null)
							{
								efParticleLoader.addScript(stb, 0, -1);
								efParticleLoader.endCR();
								efParticleLoader = null;
							}
							if (efSetterP != null)
							{
								efSetterP.ScriptAdd(stb, 0, -1);
								EfSetterP.scriptConvert(efSetterP);
								efSetterP = null;
							}
							attackGhostDrawer = ((attackGhostDrawer != null) ? attackGhostDrawer.endCR() : null);
							stb.Clear();
							if (stb2.isStart("SETTER.", 0))
							{
								string text = stb2.ToString("SETTER.".Length, stb2.Length - "SETTER.".Length);
								efSetterP = new EfSetterP(text);
								EfParticleManager.OPtcSetter[text] = efSetterP;
							}
							else if (stb2.isStart("AGD.", 0))
							{
								string text = stb2.ToString("AGD.".Length, stb2.Length - "AGD.".Length);
								attackGhostDrawer = (EfParticleManager.OAgd[text] = new AttackGhostDrawer(csvReader));
							}
							else if (stb2.IsWholeWMatch(0, -1))
							{
								string text = stb2.ToString();
								efParticleLoader = new EfParticleLoader(text);
								EfParticleManager.APtcO.Add(efParticleLoader);
							}
							else
							{
								X.de("不正なパーティクルキー:" + stb2.ToString(), null);
							}
						}
						else
						{
							int num3;
							stbReader.ScrollFirstWord(num, out num3, stb2, num2);
							if (efParticleLoader != null)
							{
								if (!stb2.Equals("{") && !stb2.Equals("}"))
								{
									if (stb2.Equals("%CLONE"))
									{
										int num4;
										stbReader.ScrollFirstWord(num3, out num4, stb2, num2);
										efParticleLoader.clone_from = stb2.ToString();
									}
									else if (stb2.Equals("%MERGE"))
									{
										int num5;
										stbReader.ScrollFirstWord(num3, out num5, stb2, num2);
										efParticleLoader.merge_from = stb2.ToString();
									}
									else
									{
										stb2.Clear();
										if (csvReader.CopyWithReplacingVar(stbReader, num, num2, stb2, " "))
										{
											stb.Add(stb2).Ret("\n");
										}
									}
								}
							}
							else if (efSetterP != null)
							{
								if (stb2.Equals("%CLONE"))
								{
									int num6;
									stbReader.ScrollFirstWord(num3, out num6, stb2, num2);
									string text2 = stb2.ToString();
									EfSetterP efSetterP2 = X.Get<string, EfSetterP>(EfParticleManager.OPtcSetter, text2);
									if (efSetterP2 != null)
									{
										efSetterP2.CopyTo(stb.Clear());
									}
									else
									{
										X.de("不明な PST キー: " + text2, null);
									}
								}
								else if (stb2.Equals("%MERGE"))
								{
									int num7;
									stbReader.ScrollFirstWord(num3, out num7, stb2, num2);
									string text3 = stb2.ToString();
									EfSetterP efSetterP3 = X.Get<string, EfSetterP>(EfParticleManager.OPtcSetter, text3);
									if (efSetterP3 != null)
									{
										efSetterP3.CopyTo(stb);
									}
									else
									{
										X.de("不明な PST キー: " + text3, null);
									}
								}
								else
								{
									stb.Add(stbReader, num, num2 - num).Ret("\n");
								}
							}
							else if (attackGhostDrawer != null)
							{
								if (stb2.Equals("%CLONE"))
								{
									int num8;
									stbReader.ScrollFirstWord(num3, out num8, stb2, num2);
									string text4 = stb2.ToString();
									AttackGhostDrawer attackGhostDrawer2 = X.Get<string, AttackGhostDrawer>(EfParticleManager.OAgd, text4);
									if (attackGhostDrawer2 == null)
									{
										stbReader.tError("不明なAGDキー: " + text4);
									}
									else
									{
										attackGhostDrawer.copyFrom(attackGhostDrawer2);
									}
								}
								else if (csvReader.readInner(stbReader, num, num2))
								{
									attackGhostDrawer.readCR(csvReader);
								}
							}
							else if (stbReader.isStart("@", num))
							{
								stbReader.ScrollFirstWord(num + 1, out num3, stb2, num2);
								string text5 = X.noext(stb2.ToString());
								if (!EfParticleManager.do_not_access_other_file || (EfParticleManager.Aaccessable_other_file != null && EfParticleManager.Aaccessable_other_file.IndexOf(text5) != -1))
								{
									if (Aother_load == null)
									{
										Aother_load = new List<string>(1);
									}
									Aother_load.Add(text5);
								}
							}
							else if (flag3)
							{
								X.de("curPtc/curPSt/CurAgd がない状態でのコマンドです: " + stbReader.ToString(num, num2 - num), null);
								flag3 = false;
							}
						}
					}
				}
				if (efParticleLoader != null)
				{
					efParticleLoader.addScript(stb, 0, -1);
					efParticleLoader.endCR();
					efParticleLoader = null;
				}
				if (efSetterP != null)
				{
					efSetterP.ScriptAdd(stb, 0, -1);
					EfSetterP.scriptConvert(efSetterP);
					efSetterP = null;
				}
				if (attackGhostDrawer != null)
				{
					attackGhostDrawer = attackGhostDrawer.endCR();
				}
			}
			if (Aother_load != null)
			{
				int count = Aother_load.Count;
				for (int i = 0; i < count; i++)
				{
					EfParticleManager.loadParticleCsv(EfParticleManager.getParticleScript(Aother_load[i]), null, null);
				}
			}
			if (flag)
			{
				if (flag2 && EfParticleManager.Aload_other_file != null)
				{
					int count2 = EfParticleManager.Aload_other_file.Count;
					for (int j = 0; j < count2; j++)
					{
						EfParticleManager.loadParticleCsv(EfParticleManager.getParticleScript(EfParticleManager.Aload_other_file[j]), null, null);
					}
				}
				EfParticleManager.loadFinalize(OPtcPre);
			}
		}

		private static void loadFinalize(BDic<string, EfParticle> OPtcPre = null)
		{
			if (EfParticleManager.APtcO == null)
			{
				return;
			}
			int count = EfParticleManager.APtcO.Count;
			bool flag = EfParticleManager.OPtc != null && OPtcPre == null;
			if (EfParticleManager.OPtc == null || OPtcPre != null)
			{
				EfParticleManager.OPtc = new BDic<string, EfParticle>(count);
			}
			for (int i = 0; i < count; i++)
			{
				EfParticleLoader efParticleLoader = EfParticleManager.APtcO[i];
				string key = efParticleLoader.key;
				if (efParticleLoader.clone_from != null)
				{
					efParticleLoader.CloneFrom = EfParticleManager.GetLoader(efParticleLoader.clone_from);
				}
				if (efParticleLoader.merge_from != null)
				{
					efParticleLoader.MergeFrom = EfParticleManager.GetLoader(efParticleLoader.merge_from);
				}
				if (key.IndexOf("___") != 0 && (!flag || !EfParticleManager.OPtc.ContainsKey(key)))
				{
					EfParticle efParticle = (EfParticleManager.OPtc[key] = new EfParticle(efParticleLoader, key));
					if (OPtcPre != null)
					{
						EfParticle efParticle2 = X.Get<string, EfParticle>(OPtcPre, key);
						if (efParticle2 != null && efParticle2.BmL != null && efParticle.BmL == null)
						{
							efParticle.BmL = efParticle2.BmL;
						}
						if (efParticle2 != null && efParticle2.Bm0 != null && efParticle.Bm0 == null)
						{
							efParticle.Bm0 = efParticle2.Bm0;
						}
					}
				}
			}
			EfParticleManager.APtcO = null;
		}

		private static EfParticleLoader GetLoader(string key)
		{
			if (EfParticleManager.APtcO != null)
			{
				for (int i = EfParticleManager.APtcO.Count - 1; i >= 0; i--)
				{
					EfParticleLoader efParticleLoader = EfParticleManager.APtcO[i];
					if (efParticleLoader.key == key)
					{
						return efParticleLoader;
					}
				}
			}
			return null;
		}

		public static EfParticle Get(string key, bool no_load = false, bool no_error = false)
		{
			EfParticle efParticle = X.Get<string, EfParticle>(EfParticleManager.OPtc, key);
			if (efParticle == null)
			{
				if (!no_error)
				{
					X.de("パーティクルオブジェクト " + key + " が見つかりません。", null);
				}
				return null;
			}
			if (!no_load)
			{
				efParticle.initParticle();
			}
			return efParticle;
		}

		public static EfParticle Get(StringKey key, bool no_error = false)
		{
			EfParticle efParticle;
			if (EfParticleManager.OPtc.TryGetValue(key, out efParticle))
			{
				efParticle.initParticle();
				return efParticle;
			}
			if (!no_error)
			{
				X.de("パーティクルオブジェクト " + key + " が見つかりません。", null);
			}
			return null;
		}

		public static AttackGhostDrawer GetAGD(string key)
		{
			AttackGhostDrawer attackGhostDrawer = X.Get<string, AttackGhostDrawer>(EfParticleManager.OAgd, key);
			if (attackGhostDrawer == null)
			{
				X.de("AGDオブジェクト " + key + " が見つかりません。", null);
				return null;
			}
			return attackGhostDrawer;
		}

		public static AttackGhostDrawer GetAGD(StringKey key)
		{
			AttackGhostDrawer attackGhostDrawer;
			if (EfParticleManager.OAgd.TryGetValue(key, out attackGhostDrawer))
			{
				return attackGhostDrawer;
			}
			X.de("AGDオブジェクト " + key + " が見つかりません。", null);
			return null;
		}

		public static EfSetterP GetSetterScript(string key)
		{
			EfSetterP efSetterP = X.Get<string, EfSetterP>(EfParticleManager.OPtcSetter, key);
			if (efSetterP == null)
			{
				X.de("パーティクルSetter " + key + " が見つかりません。", null);
				return null;
			}
			return efSetterP;
		}

		public static bool initted
		{
			get
			{
				return EfParticleManager.OPtc != null;
			}
		}

		public static string particle_dir = "Basic/DataParticle/";

		public static string particle_main_script_name = "__main";

		public static string particle_ext = ".particle";

		public static string particle_ext_full = ".particle.csv";

		public static bool do_not_access_other_file = false;

		private static List<string> Aaccessable_other_file;

		private static List<string> Aload_other_file;

		private static string initialize_particle_data = null;

		private static BDic<string, EfParticle> OPtc;

		private static BDic<string, AttackGhostDrawer> OAgd;

		private static BDic<string, EfSetterP> OPtcSetter;

		private static int load_level = 0;

		private static List<EfParticleLoader> APtcO;

		public static int reload_count = 0;

		public static uint create_count = 0U;

		public static TextAsset InitializeData;

		public static readonly Regex RegW = new Regex("^\\w+$");

		private const string SETTER_header = "SETTER.";

		private const string AGD_header = "AGD.";
	}
}
