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
			CsvReader csvReader = new CsvReader(data_text, CsvReader.RegSpace, true);
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
			CsvVariableContainer csvVariableContainer = null;
			for (;;)
			{
				if (efSetterP != null)
				{
					if (!csvReader.readCorrectly())
					{
						break;
					}
					if (TX.noe(csvReader.getLastStr()))
					{
						continue;
					}
					if (!csvReader.stringsInput(csvReader.getLastStr()))
					{
						continue;
					}
				}
				else if (!csvReader.read())
				{
					break;
				}
				if (csvReader.cmd.IndexOf("@") == 0)
				{
					string text = X.noext(TX.slice(csvReader.cmd, 1));
					if (!EfParticleManager.do_not_access_other_file || (EfParticleManager.Aaccessable_other_file != null && EfParticleManager.Aaccessable_other_file.IndexOf(text) != -1))
					{
						if (Aother_load == null)
						{
							Aother_load = new List<string>(1);
						}
						Aother_load.Add(text);
					}
				}
				else if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					string text2 = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					flag3 = true;
					if (efParticleLoader != null)
					{
						efParticleLoader.endCR();
						efParticleLoader = null;
					}
					if (efSetterP != null)
					{
						EfSetterP.scriptConvert(efSetterP);
						efSetterP = null;
					}
					attackGhostDrawer = ((attackGhostDrawer != null) ? attackGhostDrawer.endCR() : null);
					if (EfParticleManager.RegSetter.Match(text2).Success)
					{
						text2 = text2.Substring(7);
						efSetterP = new EfSetterP(text2);
						csvReader.no_replace_quote = true;
						csvReader.no_write_varcon = 2;
						EfParticleManager.OPtcSetter[text2] = efSetterP;
						if (csvVariableContainer == null)
						{
							csvVariableContainer = csvReader.VarCon;
							csvReader.VarCon = null;
						}
					}
					else if (EfParticleManager.RegAgd.Match(text2).Success)
					{
						text2 = text2.Substring(4);
						attackGhostDrawer = (EfParticleManager.OAgd[text2] = new AttackGhostDrawer(csvReader));
					}
					else
					{
						csvReader.no_replace_quote = false;
						csvReader.no_write_varcon = 0;
						if (csvVariableContainer != null)
						{
							csvReader.VarCon = csvVariableContainer;
							csvVariableContainer = null;
						}
						if (EfParticleManager.RegW.Match(text2).Success)
						{
							efParticleLoader = new EfParticleLoader(text2);
							EfParticleManager.APtcO.Add(efParticleLoader);
						}
						else
						{
							X.de("不正なパーティクルキー:" + text2, null);
						}
					}
				}
				else if (efParticleLoader != null)
				{
					if (!(csvReader.cmd == "{") && !(csvReader.cmd == "}"))
					{
						if (csvReader.cmd == "%CLONE")
						{
							efParticleLoader.clone_from = csvReader._1;
						}
						else if (csvReader.cmd == "%MERGE")
						{
							efParticleLoader.merge_from = csvReader._1;
						}
						else
						{
							efParticleLoader.addScript(csvReader);
						}
					}
				}
				else if (efSetterP != null)
				{
					if (csvReader.cmd == "%CLONE")
					{
						EfSetterP efSetterP2 = X.Get<string, EfSetterP>(EfParticleManager.OPtcSetter, csvReader._1);
						if (efSetterP2 != null)
						{
							efSetterP.ScriptSet(efSetterP2);
						}
						else
						{
							X.de("不明な PST キー: " + csvReader._1, null);
						}
					}
					else if (csvReader.cmd == "%MERGE")
					{
						EfSetterP efSetterP3 = X.Get<string, EfSetterP>(EfParticleManager.OPtcSetter, csvReader._1);
						if (efSetterP3 != null)
						{
							efSetterP.ScriptAdd(efSetterP3);
						}
						else
						{
							X.de("不明な PST キー: " + csvReader._1, null);
						}
					}
					else
					{
						efSetterP.ScriptAdd(csvReader.getLastStr());
					}
				}
				else if (attackGhostDrawer != null)
				{
					if (csvReader.cmd == "%CLONE")
					{
						AttackGhostDrawer attackGhostDrawer2 = X.Get<string, AttackGhostDrawer>(EfParticleManager.OAgd, csvReader._1);
						if (attackGhostDrawer2 == null)
						{
							csvReader.tError("不明なAGDキー: " + csvReader._1);
						}
						else
						{
							attackGhostDrawer.copyFrom(attackGhostDrawer2);
						}
					}
					else
					{
						attackGhostDrawer.readCR(csvReader);
					}
				}
				else if (flag3)
				{
					X.de("curPtc/curPSt/CurAgd がない状態でのコマンドです: " + csvReader.cmd, null);
					flag3 = false;
				}
			}
			if (efParticleLoader != null)
			{
				efParticleLoader.endCR();
			}
			if (efSetterP != null)
			{
				EfSetterP.scriptConvert(efSetterP);
			}
			if (attackGhostDrawer != null)
			{
				attackGhostDrawer = attackGhostDrawer.endCR();
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

		private static readonly Regex RegSetter = new Regex("^SETTER\\.\\w+$", RegexOptions.IgnoreCase);

		private static readonly Regex RegAgd = new Regex("^AGD\\.\\w+$", RegexOptions.IgnoreCase);
	}
}
