using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Better;
using UnityEngine;

namespace XX
{
	public sealed class TX
	{
		public static event FnTxLocalization EvReadLocalize;

		public static bool isInitted
		{
			get
			{
				return TX.TxCon != null && TX.TxCon.Count > 0;
			}
		}

		public static void initTx()
		{
			STB.InitSTB();
			if (TX.TxCon == null)
			{
				TX.TX_EMPTY = new TX("_");
				TX.Aeval_buf = new List<string>(4);
				TX.Atx_buf = new List<string>(4);
				TX.OLsnEval = new BDic<object, TxEvalListenerContainer>(8);
				if (TX.ABld == null)
				{
					TX.ABld = new STB[20];
					TX.ABld[0] = new STB(128);
				}
				TX.OLsnEval.Add(TX.TX_EMPTY, TxEvalListenerContainer.getDefault());
				TX.OLsnEvalStaticFn = new BDic<string, FnTxEval>(10);
				if (IN.init_key_and_text_when_awake)
				{
					TX.reloadTx(false);
					return;
				}
				TX.TxCon = new TX.TXFamily("_", "JP", "JP", 16, null);
				TX.TxConDef = TX.TxCon;
				BDic<string, TX.TXFamily> bdic = new BDic<string, TX.TXFamily>();
				bdic["_"] = TX.TxCon;
				TX.OTxFam = bdic;
			}
		}

		public static void releaseBannerIcon()
		{
			if (TX.OTxFam == null)
			{
				return;
			}
			foreach (KeyValuePair<string, TX.TXFamily> keyValuePair in TX.OTxFam)
			{
				keyValuePair.Value.releaseBannerIcon();
			}
		}

		public static string considerCurrentFamilyWithTimezone(bool changing = true)
		{
			TX.TXFamily txfamily = null;
			SystemLanguage systemLanguage = Application.systemLanguage;
			foreach (KeyValuePair<string, TX.TXFamily> keyValuePair in TX.OTxFam)
			{
				TX.TXFamily value = keyValuePair.Value;
				if (value.is_default)
				{
					if (txfamily == null)
					{
						txfamily = value;
					}
				}
				else if (value.AsystemLanguage != null && X.isinS<SystemLanguage>(value.AsystemLanguage, systemLanguage) >= 0)
				{
					return TX.default_family = value.key;
				}
			}
			if (txfamily == null)
			{
				return TX.default_family;
			}
			return TX.default_family = txfamily.key;
		}

		public static void reloadTx(bool reloading = false)
		{
			if (reloading)
			{
				string text = X.isinOC<string, TX.TXFamily>(TX.OTxFam, TX.TxCon);
				if (TX.valid(text))
				{
					TX.default_family = text;
				}
				MTRX.remakeFontStorageDictionary();
			}
			TX.TxCon = null;
			TX.TxConDef = (TX.TxConEng = null);
			X.ENG_MODE = false;
			BDic<string, TX.TXFamily> otxFam = TX.OTxFam;
			TX.OTxFam = new BDic<string, TX.TXFamily>();
			string[] files = Directory.GetFiles(TX.tx_script_full_dir, "___family*.txt", SearchOption.TopDirectoryOnly);
			int num = files.Length;
			for (int i = 0; i < num; i++)
			{
				string text2 = NKT.readSpecificStreamingText(files[i], false);
				if (TX.valid(text2))
				{
					CsvReader csvReader = new CsvReader(text2, new Regex("[ \\s\\t]+"), false);
					TX.TXFamily txfamily = null;
					while (csvReader.read())
					{
						if (!TX.isStart(csvReader.cmd, "%", 0))
						{
							if (TX.OTxFam.ContainsKey(csvReader.cmd))
							{
								txfamily = TX.OTxFam[csvReader.cmd];
							}
							else
							{
								txfamily = new TX.TXFamily(csvReader.cmd, csvReader._1, csvReader._2, 128, (otxFam != null && otxFam.ContainsKey(csvReader.cmd)) ? otxFam[csvReader.cmd] : null);
								if (csvReader.cmd == "_")
								{
									TX.TxConDef = (TX.TxCon = txfamily);
								}
								if (csvReader.cmd == "en")
								{
									TX.TxConEng = txfamily;
								}
								TX.OTxFam[csvReader.cmd] = txfamily;
							}
						}
						else if (txfamily != null)
						{
							string cmd = csvReader.cmd;
							if (cmd != null)
							{
								uint num2 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
								if (num2 <= 2496045785U)
								{
									if (num2 <= 2045886179U)
									{
										if (num2 != 1083912111U)
										{
											if (num2 != 1418210799U)
											{
												if (num2 == 2045886179U)
												{
													if (cmd == "%FONT_XRATIO_1BYTE")
													{
														txfamily.bundle_font_xratio_1byte = csvReader.Nm(1, 0f);
														continue;
													}
												}
											}
											else if (cmd == "%FONT_YSHIFT")
											{
												txfamily.bundle_font_yshift = csvReader.Nm(1, 0f);
												continue;
											}
										}
										else if (cmd == "%FONT_BASE_HEIGHT")
										{
											txfamily.bundle_font_base_height = csvReader.Nm(1, 0f);
											continue;
										}
									}
									else if (num2 != 2150206756U)
									{
										if (num2 != 2300197976U)
										{
											if (num2 == 2496045785U)
											{
												if (cmd == "%FONT_XRATIO")
												{
													txfamily.bundle_font_xratio = csvReader.Nm(1, 0f);
													continue;
												}
											}
										}
										else if (cmd == "%SPACE_DELIMITER")
										{
											txfamily.is_space_delimiter = csvReader.Nm(1, 1f) != 0f;
											continue;
										}
									}
									else if (cmd == "%DEFAULT_LANGUAGE")
									{
										txfamily.is_default = csvReader.Nm(1, 1f) != 0f;
										continue;
									}
								}
								else if (num2 <= 3025410629U)
								{
									if (num2 != 2565318275U)
									{
										if (num2 != 2925660424U)
										{
											if (num2 == 3025410629U)
											{
												if (cmd == "%FONT_DEFAULT_SIZE")
												{
													txfamily.bundle_font_def_default_renderer_size = csvReader.Nm(1, txfamily.bundle_font_def_default_renderer_size);
													txfamily.bundle_font_tit_default_renderer_size = csvReader.Nm(2, txfamily.bundle_font_tit_default_renderer_size);
													continue;
												}
											}
										}
										else if (cmd == "%SYSTEM_LANGUAGE")
										{
											txfamily.inputSystemLanguage(csvReader.slice(1, -1000));
											continue;
										}
									}
									else if (cmd == "%USE_ARIEL")
									{
										txfamily.use_ariel = csvReader.Nm(1, 1f) != 0f;
										continue;
									}
								}
								else if (num2 <= 3091441466U)
								{
									if (num2 != 3050332845U)
									{
										if (num2 == 3091441466U)
										{
											if (cmd == "%ENGLISH_MODE")
											{
												txfamily.is_english = csvReader.Nm(1, 1f) != 0f;
												continue;
											}
										}
									}
									else if (cmd == "%LEFTSHIFT_CONSONANT")
									{
										txfamily.consider_leftshift = csvReader.Nm(1, 1f) != 0f;
										continue;
									}
								}
								else if (num2 != 3734604630U)
								{
									if (num2 == 4274783757U)
									{
										if (cmd == "%FONT")
										{
											txfamily.bundle_font_def = csvReader.getIndex(1);
											txfamily.bundle_font_tit = csvReader.getIndex(2);
											if (TX.noe(txfamily.bundle_font_tit))
											{
												txfamily.bundle_font_tit = txfamily.bundle_font_def;
												continue;
											}
											continue;
										}
									}
								}
								else if (cmd == "%BUNDLE_PATH")
								{
									txfamily.prepareAssetBundlePath(csvReader.slice(1, -1000));
									continue;
								}
							}
							X.de("不明な SystemLanguage 指定子: " + csvReader.cmd, null);
						}
					}
				}
			}
			if (!reloading)
			{
				TX.default_family = TX.considerCurrentFamilyWithTimezone(false);
			}
			if (!TX.OTxFam.ContainsKey(TX.TxCon.key))
			{
				TX.OTxFam[TX.TxCon.key] = TX.TxCon;
			}
			foreach (KeyValuePair<string, TX.TXFamily> keyValuePair in TX.OTxFam)
			{
				keyValuePair.Value.scriptFinalize();
			}
			string[] array = TX.split(MTRX.Read("__tx_list", ""), "\n");
			int num3 = array.Length;
			for (int j = 0; j < num3; j++)
			{
				string text3 = TX.trim(array[j]);
				if (text3 != "")
				{
					TX.readTextsAt(text3);
				}
			}
			if (TX.OTxFam.ContainsKey(TX.default_family))
			{
				TX.changeFamily(TX.default_family);
			}
		}

		public static string trim(string str)
		{
			int length = str.Length;
			int num = 0;
			while (num < length && " \t\n\r".IndexOf(str[num]) >= 0)
			{
				num++;
			}
			if (num >= length)
			{
				return str;
			}
			int num2 = length - 1;
			while (num2 > num && " \t\n\r".IndexOf(str[num2]) >= 0)
			{
				num2--;
			}
			return str.Substring(num, num2 + 1 - num);
		}

		public static string add(string src, string add_str, string delimiter = "\n")
		{
			bool flag = TX.valid(add_str);
			return src + ((src == "" || !flag) ? "" : delimiter) + (flag ? add_str : "");
		}

		private static void readTextsAt(string key)
		{
			bool flag = false;
			if (TX.isStart(key, "!", 0))
			{
				flag = true;
				key = TX.slice(key, 1);
			}
			foreach (KeyValuePair<string, TX.TXFamily> keyValuePair in TX.OTxFam)
			{
				string key2 = keyValuePair.Key;
				if (!flag || !(key2 != "_"))
				{
					string text = NKT.readStreamingText(Path.Combine(Path.Combine("localization", keyValuePair.Key), key2 + key + ".txt"), !X.DEBUG || !X.DEBUGANNOUNCE);
					if (!TX.noe(text))
					{
						TX.readTexts(text, keyValuePair.Value);
					}
				}
			}
		}

		public static void reloadFontLetterSpace()
		{
			foreach (KeyValuePair<MFont, FontStorage> keyValuePair in MTRX.OFontStorage)
			{
				keyValuePair.Value.reloadLetterSpacingScript();
			}
		}

		private static void readTexts(string LT, TX.TXFamily Fam)
		{
			if (LT == null)
			{
				return;
			}
			CsvReader csvReader = new CsvReader(null, CsvReader.RegOnlySpace, false)
			{
				no_write_varcon = 2,
				no_replace_quote = true
			};
			TX tx = null;
			int i = 0;
			StbReader stbReader = new StbReader(64, LT);
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					int num;
					int num2;
					while (stbReader.readCorrectly(out num, out num2, false))
					{
						if (num < num2)
						{
							if (stbReader.charIs(0, '%'))
							{
								stbReader.TaleTrimComment(num, out num2, num2);
								if (tx != null)
								{
									tx.setContent(stb2.ToString());
									tx = null;
								}
								csvReader.readInner(stbReader, num, num2);
								if (csvReader.cmd == "%FAMILY")
								{
									continue;
								}
								if (TX.EvReadLocalize != null && TX.EvReadLocalize(csvReader, Fam, ref tx))
								{
									if (tx != null)
									{
										i = stbReader.get_cur_line() + ((stb2.Set(tx.text).Length == 0) ? 1 : 0);
										continue;
									}
									continue;
								}
							}
							if (stbReader.IsSectionHeader(num, stb, num2))
							{
								if (tx != null)
								{
									tx.setContent(stb2.ToString());
									tx = null;
								}
								if (stb.Length > 0 && stb.IsWholeWMatch(0, -1))
								{
									tx = TX.getTX(stb.ToString(), false, false, Fam);
									i = stbReader.get_cur_line() + ((stb2.Set(tx.text).Length == 0) ? 1 : 0);
								}
							}
							else if (stbReader.isStart("&&", num))
							{
								if (tx != null)
								{
									tx.setContent(stb2.ToString());
									tx = null;
								}
								int num3;
								stbReader.ScrollFirstWord(num + 2, out num3, stb, num2);
								if (stb.Length > 0)
								{
									TX tx2 = TX.getTX(stb.ToString(), false, false, Fam);
									bool flag;
									stbReader.Replace("\\n", stb.Set("\n"), out flag, num3, ref num2);
									tx2.setContent(stb2.Set(tx2.text).Append(stbReader, " ", num3, num2 - num3).ToString());
								}
							}
							else if (tx != null && !stbReader.isStart("//", num))
							{
								while (i < stbReader.get_cur_line())
								{
									stb2.Add("\n");
									i++;
								}
								stb2.Add(stbReader);
							}
						}
					}
					if (tx != null)
					{
						tx.setContent(stb2.ToString());
						tx = null;
					}
				}
			}
		}

		public static void changeFamily(string fam = "_")
		{
			if (!TX.OTxFam.ContainsKey(fam))
			{
				TX.TxCon = TX.TxConDef;
			}
			else
			{
				TX.TxCon = TX.OTxFam[fam];
			}
			TX.TxCon.prepareLanguage();
			X.ENG_MODE = TX.TxCon.is_english;
		}

		public static void changeFamilyToIndex(int i)
		{
			foreach (KeyValuePair<string, TX.TXFamily> keyValuePair in TX.OTxFam)
			{
				if (i-- == 0)
				{
					TX.changeFamily(keyValuePair.Key);
					break;
				}
			}
		}

		public static TX getTX(string title, bool no_make = true, bool no_error = false, TX.TXFamily NowFam = null)
		{
			NowFam = NowFam ?? TX.TxCon;
			TX tx = NowFam.Get(title);
			if (tx != null)
			{
				return tx;
			}
			if (no_make)
			{
				if (NowFam != TX.TxConDef)
				{
					if (NowFam != TX.TxConEng && TX.TxConEng != null)
					{
						return TX.getTX(title, true, no_error, TX.TxConEng);
					}
					if (NowFam == TX.TxConEng || TX.TxConEng == null)
					{
						return TX.getTX(title, true, no_error, TX.TxConDef);
					}
				}
				if (!no_error)
				{
					X.de("テキストシーケンス " + title + " が見つかりません。", null);
					return TX.TX_EMPTY;
				}
				return null;
			}
			else
			{
				if (title == "" || title == null)
				{
					return TX.TX_EMPTY;
				}
				tx = new TX(title);
				NowFam.Add(tx);
				return tx;
			}
		}

		public static string ReplaceTX(string src, bool no_error = false)
		{
			if (src.IndexOf("&&") == -1)
			{
				return src;
			}
			int num = 0;
			string text2;
			using (STB stb = TX.PopBld(src, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					for (;;)
					{
						int num2 = stb.IndexOf("&&", num, -1);
						if (num2 < 0)
						{
							break;
						}
						stb2.Add(stb, num, num2 - num);
						num2 += 2;
						int num3;
						stb.Scroll(num2, out num3, TX.FnIsWMatch, -1);
						stb2.AddTxA(stb.ToString(num2, num3 - num2), true);
						while (stb.Is('[', num3))
						{
							num2 = num3 + 1;
							if (stb.isStart("&&", num2))
							{
								num2 += 2;
								stb.Scroll(num2, out num3, TX.FnIsWMatch, -1);
								string text = stb.ToString(num2, num3 - num2);
								TX tx = TX.getTX(text, true, true, null);
								if (tx != null)
								{
									stb2.TxRpl(tx.text);
								}
								else
								{
									stb2.TxRpl(text);
								}
							}
							else
							{
								stb.Scroll(num2, out num3, TX.FnIsWMatch, -1);
								stb2.TxRpl(stb.ToString(num2, num3 - num2));
							}
							num3 += (stb.Is(']', num3) ? 1 : 0);
						}
						num = num3;
					}
					stb2.Add(stb, num, stb.Length - num);
					text2 = stb2.ToString();
				}
			}
			return text2;
		}

		public static string Get(string title, string default_str = "")
		{
			TX tx = TX.getTX(title, true, true, null);
			if (tx == null)
			{
				return default_str;
			}
			return tx.text;
		}

		public static string GetA(string title, string[] Astr)
		{
			TX.Atx_buf.Clear();
			TX.Atx_buf.AddRange(Astr);
			return TX.GetA(TX.getTX(title, true, true, null), TX.Atx_buf);
		}

		public static string GetA(string title, string str0)
		{
			return TX.GetA(TX.getTX(title, true, true, null), str0);
		}

		public static string GetA(string title, string str0, string str1)
		{
			return TX.GetA(TX.getTX(title, true, true, null), str0, str1);
		}

		public static string GetA(string title, string str0, string str1, string str2)
		{
			return TX.GetA(TX.getTX(title, true, true, null), str0, str1, str2);
		}

		public static string GetA(string title, string str0, string str1, string str2, string str3)
		{
			return TX.GetA(TX.getTX(title, true, true, null), str0, str1, str2, str3);
		}

		public static string GetA(string title, string str0, string str1, string str2, string str3, string str4)
		{
			return TX.GetA(TX.getTX(title, true, true, null), str0, str1, str2, str3, str4, null, null);
		}

		public static string GetA(string title, string str0, string str1, string str2, string str3, string str4, string str5)
		{
			return TX.GetA(TX.getTX(title, true, true, null), str0, str1, str2, str3, str4, str5, null);
		}

		public static string GetA(string title, string str0, string str1, string str2, string str3, string str4, string str5, string str6)
		{
			return TX.GetA(TX.getTX(title, true, true, null), str0, str1, str2, str3, str4, str5, str6);
		}

		public static string GetA(TX t, string str0)
		{
			if (t == null)
			{
				return "";
			}
			TX.Atx_buf.Clear();
			TX.Atx_buf.Add(str0);
			return TX.GetA(t, TX.Atx_buf);
		}

		public static string GetA(TX t, string str0, string str1)
		{
			if (t == null)
			{
				return "";
			}
			TX.Atx_buf.Clear();
			TX.Atx_buf.Add(str0);
			TX.Atx_buf.Add(str1);
			return TX.GetA(t, TX.Atx_buf);
		}

		public static string GetA(TX t, string str0, string str1, string str2)
		{
			if (t == null)
			{
				return "";
			}
			TX.Atx_buf.Clear();
			TX.Atx_buf.Add(str0);
			TX.Atx_buf.Add(str1);
			TX.Atx_buf.Add(str2);
			return TX.GetA(t, TX.Atx_buf);
		}

		public static string GetA(TX t, string str0, string str1, string str2, string str3)
		{
			if (t == null)
			{
				return "";
			}
			TX.Atx_buf.Clear();
			TX.Atx_buf.Add(str0);
			TX.Atx_buf.Add(str1);
			TX.Atx_buf.Add(str2);
			TX.Atx_buf.Add(str3);
			return TX.GetA(t, TX.Atx_buf);
		}

		public static string GetA(TX t, string str0, string str1, string str2, string str3, string str4, string str5 = null, string str6 = null)
		{
			if (t == null)
			{
				return "";
			}
			TX.Atx_buf.Clear();
			if (str0 != null)
			{
				TX.Atx_buf.Add(str0);
			}
			if (str1 != null)
			{
				TX.Atx_buf.Add(str1);
			}
			if (str2 != null)
			{
				TX.Atx_buf.Add(str2);
			}
			if (str3 != null)
			{
				TX.Atx_buf.Add(str3);
			}
			if (str4 != null)
			{
				TX.Atx_buf.Add(str4);
			}
			if (str5 != null)
			{
				TX.Atx_buf.Add(str5);
			}
			if (str6 != null)
			{
				TX.Atx_buf.Add(str6);
			}
			return TX.GetA(t, TX.Atx_buf);
		}

		public static string GetA(TX t, List<string> Astr)
		{
			if (t != null)
			{
				STB stb = TX.PopBld(t.text, 0);
				int i = 0;
				while (i < stb.Length)
				{
					char c = stb[i++];
					if (c == '\\')
					{
						i++;
					}
					else if (c == '&')
					{
						int num2;
						int num = stb.NmI(i, out num2, -1, -1);
						if (num >= 1)
						{
							int num3 = i - 1;
							int num4 = num2 - num3;
							string text = ((num <= Astr.Count) ? Astr[num - 1] : "");
							if (num4 < text.Length)
							{
								stb.unshift(num3, '\0', text.Length - num4);
							}
							else if (num4 > text.Length)
							{
								stb.Splice(num3 + text.Length, num4 - text.Length);
							}
							stb.Overwrite(num3, text);
							i = num3 + text.Length;
						}
					}
				}
				string text2 = stb.ToString();
				TX.ReleaseBld(stb);
				return text2;
			}
			return "";
		}

		public static string[] GetArray(string str0, string str1)
		{
			TX.Atx_buf.Clear();
			TX.Atx_buf.Add(str0);
			TX.Atx_buf.Add(str1);
			return TX.GetArray(TX.Atx_buf);
		}

		public static string[] GetArray(string str0, string str1, string str2)
		{
			TX.Atx_buf.Clear();
			TX.Atx_buf.Add(str0);
			TX.Atx_buf.Add(str1);
			TX.Atx_buf.Add(str2);
			return TX.GetArray(TX.Atx_buf);
		}

		public static string[] GetArray(string str0, string str1, string str2, string str3)
		{
			TX.Atx_buf.Clear();
			TX.Atx_buf.Add(str0);
			TX.Atx_buf.Add(str1);
			TX.Atx_buf.Add(str2);
			TX.Atx_buf.Add(str3);
			return TX.GetArray(TX.Atx_buf);
		}

		public static string[] GetArray(List<string> Atx_keys)
		{
			int count = Atx_keys.Count;
			string[] array = new string[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = TX.Get(Atx_keys[i], "");
			}
			return array;
		}

		public static string[] GetArrayCountUp(string suffix, int len, int start_i = 0)
		{
			string[] array = new string[len];
			for (int i = 0; i < len; i++)
			{
				array[i] = TX.Get(suffix + (i + start_i).ToString(), "");
			}
			return array;
		}

		public static int countFamilies()
		{
			return TX.OTxFam.Count;
		}

		public static bool familyIs(string s)
		{
			return TX.TxCon.key == s;
		}

		public static bool isFamilyDefault()
		{
			return TX.TxConDef == TX.TxCon;
		}

		public static TX.TXFamily getDefaultFamily()
		{
			return TX.TxConDef;
		}

		public static bool isFamilyAriel()
		{
			return TX.TxCon.use_ariel;
		}

		public static string familyName(string s, bool full_name = true)
		{
			s = s.ToLower();
			if (!TX.OTxFam.ContainsKey(s))
			{
				return s.ToUpper();
			}
			if (full_name)
			{
				return TX.OTxFam[s].full_name;
			}
			return TX.OTxFam[s].simple_name;
		}

		public static int getCurrentFamilyIndex()
		{
			int num = 0;
			foreach (KeyValuePair<string, TX.TXFamily> keyValuePair in TX.OTxFam)
			{
				if (keyValuePair.Value == TX.TxCon)
				{
					return num;
				}
				num++;
			}
			return num;
		}

		public static string getCurrentFamilyName()
		{
			return TX.TxCon.key;
		}

		public static TX.TXFamily getCurrentFamily()
		{
			return TX.TxCon;
		}

		public static bool isEnglishLang()
		{
			return TX.TxCon.is_english;
		}

		public static bool isSpaceDelimiterLang()
		{
			return TX.TxCon.is_space_delimiter;
		}

		public static MFont getDefaultFont()
		{
			return TX.TxCon.getDefaultFont();
		}

		public static MFont getTitleFont()
		{
			return TX.TxCon.getTitleFont();
		}

		public static BDic<string, TX.TXFamily> getWholeTextFamilyObject()
		{
			return TX.OTxFam;
		}

		public static BList<string> listUpFamilyName(BList<string> Adest, bool full_name = true)
		{
			if (Adest.Capacity < TX.OTxFam.Count + Adest.Count)
			{
				Adest.Capacity = TX.OTxFam.Count + Adest.Count;
			}
			foreach (KeyValuePair<string, TX.TXFamily> keyValuePair in TX.OTxFam)
			{
				Adest.Add(full_name ? keyValuePair.Value.full_name : keyValuePair.Value.simple_name);
			}
			return Adest;
		}

		public static TX.TXFamily getFamilyByName(string s)
		{
			if (!TX.OTxFam.ContainsKey(s))
			{
				return null;
			}
			return TX.OTxFam[s];
		}

		public static string evalT(string str)
		{
			return TX.convertDefinedData(str);
		}

		public static bool evalLsnConvert(string s, List<string> Aeval = null)
		{
			if (s != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(s);
				if (num > 1832977262U)
				{
					if (num <= 2195501096U)
					{
						if (num != 2012698496U)
						{
							if (num != 2149762166U)
							{
								if (num != 2195501096U)
								{
									goto IL_025F;
								}
								if (!(s == "pad_mode"))
								{
									goto IL_025F;
								}
								TX.value_inputted = (double)(IN.getCurrentKeyAssignObject().pad_mode ? 1 : 0);
								return true;
							}
							else
							{
								if (!(s == "sensitive_level"))
								{
									goto IL_025F;
								}
								TX.value_inputted = (double)X.sensitive_level;
								return true;
							}
						}
						else if (!(s == "PI2"))
						{
							goto IL_025F;
						}
					}
					else if (num <= 3387359064U)
					{
						if (num != 2470526784U)
						{
							if (num != 3387359064U)
							{
								goto IL_025F;
							}
							if (!(s == "DEBUGALLSKILL"))
							{
								goto IL_025F;
							}
							TX.value_inputted = (double)(X.DEBUGALLSKILL ? 1 : 0);
							return true;
						}
						else if (!(s == "pi2"))
						{
							goto IL_025F;
						}
					}
					else if (num != 3998840952U)
					{
						if (num != 4059568014U)
						{
							goto IL_025F;
						}
						if (!(s == "PIH"))
						{
							goto IL_025F;
						}
						goto IL_0217;
					}
					else
					{
						if (!(s == "FALSE"))
						{
							goto IL_025F;
						}
						goto IL_01E7;
					}
					TX.value_inputted = 6.2831854820251465;
					return true;
				}
				if (num > 1213090802U)
				{
					if (num != 1303515621U)
					{
						if (num != 1343949093U)
						{
							if (num != 1832977262U)
							{
								goto IL_025F;
							}
							if (!(s == "pih"))
							{
								goto IL_025F;
							}
							goto IL_0217;
						}
						else if (!(s == "TRUE"))
						{
							goto IL_025F;
						}
					}
					else if (!(s == "true"))
					{
						goto IL_025F;
					}
					TX.value_inputted = 1.0;
					return true;
				}
				if (num != 184981848U)
				{
					if (num != 671087282U)
					{
						if (num != 1213090802U)
						{
							goto IL_025F;
						}
						if (!(s == "pi"))
						{
							goto IL_025F;
						}
					}
					else if (!(s == "PI"))
					{
						goto IL_025F;
					}
					TX.value_inputted = 3.1415927410125732;
					return true;
				}
				if (!(s == "false"))
				{
					goto IL_025F;
				}
				IL_01E7:
				TX.value_inputted = 0.0;
				return true;
				IL_0217:
				TX.value_inputted = 1.5707963705062866;
				return true;
			}
			IL_025F:
			TX.value_inputted = 0.0;
			Aeval = Aeval ?? TX.Aeval_buf;
			if (TX.need_txeval_static_reassign)
			{
				TX.need_txeval_static_reassign = false;
				TX.OLsnEvalStaticFn.Clear();
				foreach (KeyValuePair<object, TxEvalListenerContainer> keyValuePair in TX.OLsnEval)
				{
					if (keyValuePair.Value.static_input)
					{
						keyValuePair.Value.AssignToStatic(TX.OLsnEvalStaticFn);
					}
				}
			}
			FnTxEval fnTxEval = X.Get<string, FnTxEval>(TX.OLsnEvalStaticFn, s);
			if (fnTxEval != null)
			{
				fnTxEval(null, Aeval);
				return true;
			}
			foreach (KeyValuePair<object, TxEvalListenerContainer> keyValuePair2 in TX.OLsnEval)
			{
				if (!keyValuePair2.Value.static_input && keyValuePair2.Value.Get(null, s, Aeval))
				{
					return true;
				}
			}
			return false;
		}

		public static string convertDefinedData(string str)
		{
			string text = "";
			REG reg = new REG();
			TX.Aeval_buf.Clear();
			while (reg._match(str, "&\\{([^\\}\\n\\r]+)\\}"))
			{
				string r = reg._R1;
				text += reg._leftContext;
				string rightContext = reg._rightContext;
				string text2 = "";
				TX tx = TX.getTX(r, true, true, null);
				if (tx != null)
				{
					text2 = tx.Get();
				}
				else if (TX.evalLsnConvert(r, null))
				{
					text2 = TX.value_inputted.ToString();
				}
				else
				{
					X.de("convertDefinedData::不明な&変換キー " + r, null);
				}
				text += text2;
				str = rightContext;
			}
			return text + str;
		}

		public static string replaceAlphabet(string t, bool to_big = true)
		{
			string text = (to_big ? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890" : "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ１２３４５６７８９０");
			string text2 = (to_big ? "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ１２３４５６７８９０" : "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890");
			int length = text2.Length;
			for (int i = 0; i < length; i++)
			{
				t = t.Replace(text[i], text2[i]);
			}
			return t;
		}

		public static bool is_small(string t)
		{
			return t.Length != 0 && (X.BTWW(0f, (float)t[0], 127f) || X.BTWW(65377f, (float)t[0], 65439f));
		}

		public static TxEvalListenerContainer createListenerEval(object _Parent, int capacity_fn = 32, bool static_flag = true)
		{
			TxEvalListenerContainer txEvalListenerContainer = X.Get<object, TxEvalListenerContainer>(TX.OLsnEval, _Parent);
			if (txEvalListenerContainer == null)
			{
				txEvalListenerContainer = (TX.OLsnEval[_Parent] = new TxEvalListenerContainer(capacity_fn, static_flag));
			}
			if (static_flag)
			{
				TX.need_txeval_static_reassign = true;
			}
			return txEvalListenerContainer;
		}

		public static void addListenerEval(object _Parent, TxEvalListenerContainer Lsn)
		{
			TX.OLsnEval[_Parent] = Lsn;
			if (Lsn.static_input)
			{
				TX.need_txeval_static_reassign = true;
			}
		}

		public static void removeListenerEval(object _Parent)
		{
			TxEvalListenerContainer txEvalListenerContainer = X.Get<object, TxEvalListenerContainer>(TX.OLsnEval, _Parent);
			if (txEvalListenerContainer != null)
			{
				TX.OLsnEval.Remove(_Parent);
				if (txEvalListenerContainer.static_input)
				{
					TX.need_txeval_static_reassign = true;
				}
			}
		}

		public TX(string _title)
		{
			this.title = _title;
		}

		public void setContent(string str)
		{
			this._data = str;
		}

		public string Get()
		{
			return this._data;
		}

		public string text
		{
			get
			{
				return this._data;
			}
		}

		public void replaceTextContents(string s)
		{
			this._data = s;
		}

		public static bool isFirstS(string c)
		{
			if (c == null || c.Length == 0)
			{
				return false;
			}
			switch (c[0])
			{
			default:
				return false;
			}
		}

		public static bool isTfMatch(STB c, int index, int e, out STB.MULT_OPE comp_cache)
		{
			if (e < 0)
			{
				e = c.Length;
			}
			comp_cache = STB.MULT_OPE.ERROR;
			if (c == null || e <= index + 1)
			{
				return false;
			}
			char c2 = c[index + 1];
			char c3 = c[index];
			if (c3 <= '&')
			{
				if (c3 != '!')
				{
					if (c3 == '&')
					{
						if (c2 == '&')
						{
							comp_cache = STB.MULT_OPE.COMP_AND;
							return true;
						}
					}
				}
				else if (c2 == '=')
				{
					comp_cache = STB.MULT_OPE.COMP_NOTEQUAL;
					return true;
				}
			}
			else
			{
				switch (c3)
				{
				case '<':
					if (c2 == '=')
					{
						comp_cache = STB.MULT_OPE.COMP_LE;
						return true;
					}
					comp_cache = STB.MULT_OPE.COMP_L;
					return true;
				case '=':
					if (c2 == '=')
					{
						comp_cache = STB.MULT_OPE.COMP_EQUAL;
						return true;
					}
					break;
				case '>':
					if (c2 == '=')
					{
						comp_cache = STB.MULT_OPE.COMP_GE;
						return true;
					}
					comp_cache = STB.MULT_OPE.COMP_G;
					return true;
				default:
					if (c3 == '|' && c2 == '|')
					{
						comp_cache = STB.MULT_OPE.COMP_OR;
						return true;
					}
					break;
				}
			}
			return false;
		}

		private static bool isIntDefineMatch(STB c, int index, int e)
		{
			if (e < 0)
			{
				e = c.Length;
			}
			return c != null && e > index + 4 && (c[index] == 'i' && c[index + 1] == 'n' && c[index + 2] == 't' && (c[index + 3] == ' ' || c[index + 3] == '.'));
		}

		public static bool isAlphaColorHeaderMatch(STB c, int index, int e)
		{
			if (e < 0)
			{
				e = c.Length;
			}
			return c != null && e > index + 4 && (TX.isNumXMatch(c[index]) && TX.isNumXMatch(c[index + 1]) && c[index + 2] == ':' && c[index + 3] == '#');
		}

		public static bool isNumDotMatch(STB c, int index, int e)
		{
			if (e < 0)
			{
				e = c.Length;
			}
			return c != null && e > index && TX.isNumDotMatch(c[index]);
		}

		public static bool isNumDotMatch(char cn)
		{
			switch (cn)
			{
			case '.':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				return true;
			}
			return false;
		}

		public static bool isNumDotEMatch(string c, int index = 0)
		{
			return c != null && c.Length > index && TX.isNumDotEMatch(c[index]);
		}

		public static bool isNumDotEMatch(char cn)
		{
			switch (cn)
			{
			case '.':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				break;
			case '/':
				return false;
			default:
				if (cn != '[' && cn != ']')
				{
					return false;
				}
				break;
			}
			return true;
		}

		public static bool isNumDotPMMatch(char cn)
		{
			if ('0' <= cn && cn <= '9')
			{
				return true;
			}
			switch (cn)
			{
			case '+':
			case '-':
			case '.':
				return true;
			}
			return false;
		}

		public static bool isNumDotPXMMatch(char cn)
		{
			if (('a' <= cn && cn <= 'f') || ('A' <= cn && cn <= 'F') || ('0' <= cn && cn <= '9'))
			{
				return true;
			}
			switch (cn)
			{
			case '+':
			case '-':
			case '.':
				return true;
			}
			return false;
		}

		public static bool isValEMatch(char cn)
		{
			if (('a' <= cn && cn <= 'z') || ('A' <= cn && cn <= 'Z') || ('0' <= cn && cn <= '9'))
			{
				return true;
			}
			if (cn != '$' && cn != '.')
			{
				switch (cn)
				{
				case '[':
				case ']':
				case '_':
					return true;
				}
				return false;
			}
			return true;
		}

		public static bool isVariableNameMatch(char cn)
		{
			return ('a' <= cn && cn <= 'z') || ('A' <= cn && cn <= 'Z') || ('0' <= cn && cn <= '9') || (cn == '_' || cn == '{' || cn == '}');
		}

		public static bool isNPlusMatch(char cn)
		{
			if (('a' <= cn && cn <= 'z') || ('A' <= cn && cn <= 'Z') || ('0' <= cn && cn <= '9'))
			{
				return true;
			}
			if (cn <= '/')
			{
				if (cn != '\t')
				{
					switch (cn)
					{
					case ' ':
					case '#':
					case '%':
					case '&':
					case '*':
					case '+':
					case '-':
					case '.':
					case '/':
						break;
					case '!':
					case '"':
					case '$':
					case '\'':
					case '(':
					case ')':
					case ',':
						return false;
					default:
						return false;
					}
				}
			}
			else if (cn != '@')
			{
				switch (cn)
				{
				case '[':
				case ']':
				case '_':
					break;
				case '\\':
				case '^':
					return false;
				default:
					return false;
				}
			}
			return true;
		}

		public static bool isNumXMatch(char cn)
		{
			return ('a' <= cn && cn <= 'f') || ('A' <= cn && cn <= 'F') || ('0' <= cn && cn <= '9');
		}

		public static bool isAlphabetMatch(char cn)
		{
			return ('a' <= cn && cn <= 'z') || ('A' <= cn && cn <= 'Z');
		}

		public static bool isWMatch(char cn)
		{
			return TX.isAlphabetMatch(cn) || TX.isNumMatch(cn) || cn == '_';
		}

		public static bool isNumMatch(char cn)
		{
			return '0' <= cn && cn <= '9';
		}

		public static bool isNumMinusMatch(char cn)
		{
			return ('0' <= cn && cn <= '9') || cn == '-';
		}

		public static bool isSpaceOrCommaOrTilde(char _c)
		{
			return _c == ' ' || _c == '\t' || _c == '\n' || _c == '\r' || _c == '\u3000' || _c == ',' || _c == '~';
		}

		public static bool isSpaceOrComma(char _c)
		{
			return _c == ' ' || _c == '\t' || _c == '\n' || _c == '\r' || _c == '\u3000' || _c == ',';
		}

		public static bool isSpace(char _c)
		{
			return _c == ' ' || _c == '\t' || _c == '\n' || _c == '\r' || _c == '\u3000';
		}

		public static bool isSpaceStrict(char _c)
		{
			return _c == ' ' || _c == '\t' || _c == '\n' || _c == '\r' || _c == '\u3000';
		}

		public static int charToHex(char _c, int defval = 0)
		{
			if ('0' <= _c && _c <= '9')
			{
				return (int)(_c - '0');
			}
			if ('a' <= _c && _c <= 'f')
			{
				return (int)(_c - 'a' + '\n');
			}
			if ('A' <= _c && _c <= 'F')
			{
				return (int)(_c - 'A' + '\n');
			}
			return defval;
		}

		public static bool chkREG(string t, Regex Reg, params string[] Aheader)
		{
			for (int i = Aheader.Length - 1; i >= 0; i--)
			{
				if (TX.isStart(t, Aheader[i], 0))
				{
					return REG.match(t, Reg);
				}
			}
			return false;
		}

		public static bool Nm(string st, out float val, int starti = 0, int end_i = -1)
		{
			if (end_i < 0)
			{
				end_i = st.Length;
			}
			if (!TX.isNumMinusMatch(st[starti]))
			{
				val = -1000f;
				return false;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(st, starti, end_i - starti);
				if (stb.Nm())
				{
					val = (float)STB.parse_result_double;
					return true;
				}
			}
			val = -1000f;
			return false;
		}

		public static double eval(string the_script, string tx0 = "")
		{
			STB stb = TX.PopBld(the_script, 0);
			double num = TX.eval(stb, 0, stb.Length);
			TX.ReleaseBld(stb);
			return num;
		}

		public static int commandEvalSet(string val, int preval)
		{
			if (TX.isStart(val, "max:", 0))
			{
				return X.Mx(preval, TX.evalI(TX.slice(val, 4)));
			}
			if (TX.isStart(val, "min:", 0))
			{
				return X.Mn(preval, TX.evalI(TX.slice(val, 4)));
			}
			if (TX.isStart(val, "+=", 0))
			{
				return preval + TX.evalI(TX.slice(val, 2));
			}
			if (TX.isStart(val, "-=", 0))
			{
				return preval - TX.evalI(TX.slice(val, 2));
			}
			if (TX.isStart(val, "|=~", 0))
			{
				return preval | ~TX.evalI(TX.slice(val, 3));
			}
			if (TX.isStart(val, "|=", 0))
			{
				return preval | TX.evalI(TX.slice(val, 2));
			}
			if (TX.isStart(val, "&=~", 0))
			{
				return preval & ~TX.evalI(TX.slice(val, 3));
			}
			if (TX.isStart(val, "&=", 0))
			{
				return preval & TX.evalI(TX.slice(val, 2));
			}
			return TX.evalI(val);
		}

		public static double eval(STB Stb, int pre, int last)
		{
			double num = 0.0;
			double num2 = 0.0;
			int num3 = pre - 1;
			int num4 = 0;
			bool flag = false;
			double num5 = 0.0;
			STB.MULT_OPE mult_OPE = STB.MULT_OPE.ERROR;
			STB.MULT_OPE mult_OPE2 = STB.MULT_OPE.ERROR;
			while (++num3 < last)
			{
				char c = Stb[num3];
				if (c == '\\')
				{
					num3++;
				}
				else
				{
					if (c == ')')
					{
						X.de("不明な閉じカッコ:" + Stb.get_slice(pre, last), null);
					}
					else
					{
						if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
						{
							continue;
						}
						if (TX.isTfMatch(Stb, num3, last, out mult_OPE2))
						{
							num += num2;
							num2 = 0.0;
							num4 = 0;
							if (mult_OPE2 != STB.MULT_OPE.COMP_AND)
							{
								if (mult_OPE2 == STB.MULT_OPE.COMP_OR)
								{
									num = TX.calcOperand(num5, mult_OPE, num);
									return (double)((num != 0.0 || TX.eval(Stb, num3 + 2, last) != 0.0) ? 1 : 0);
								}
								num5 = TX.calcOperand(num5, mult_OPE, num);
								mult_OPE = mult_OPE2;
								num = 0.0;
								if (mult_OPE != STB.MULT_OPE.COMP_L && mult_OPE != STB.MULT_OPE.COMP_G)
								{
									num3++;
								}
							}
							else
							{
								num = TX.calcOperand(num5, mult_OPE, num);
								if (num == 0.0)
								{
									return 0.0;
								}
								return (double)((TX.eval(Stb, num3 + 2, last) != 0.0) ? 1 : 0);
							}
						}
						else if (c == '*' || c == '/' || c == '%' || c == '&' || c == '|')
						{
							num4 = ((c == '*') ? 1 : ((c == '%') ? 2 : ((c == '&') ? 3 : ((c == '|') ? 4 : (-1)))));
						}
						else
						{
							if (c == 'i' && TX.isIntDefineMatch(Stb, num3, last))
							{
								flag = true;
								num3 += 3;
								continue;
							}
							if (num4 == -1000)
							{
								num += num2;
								num2 = 0.0;
								num4 = 0;
							}
							int num6;
							TX.extractValue(Stb, num3, last, out num6, (float)((num4 == -1 || num4 == 1 || num4 == 2) ? 1 : 0));
							if (flag)
							{
								TX.value_inputted = (double)((int)TX.value_inputted);
							}
							num2 = TX.calcMul(num2, TX.value_inputted, num4);
							num4 = -1000;
							num3 = num6 - 1;
						}
					}
					flag = false;
				}
			}
			num += num2;
			return TX.calcOperand(num5, mult_OPE, num);
		}

		private static double calcOperand(double vl, STB.MULT_OPE ope, double vr)
		{
			switch (ope)
			{
			case STB.MULT_OPE.COMP_EQUAL:
				return (double)((vl == vr) ? 1 : 0);
			case STB.MULT_OPE.COMP_NOTEQUAL:
				return (double)((vl != vr) ? 1 : 0);
			case STB.MULT_OPE.COMP_G:
				return (double)((vl > vr) ? 1 : 0);
			case STB.MULT_OPE.COMP_GE:
				return (double)((vl >= vr) ? 1 : 0);
			case STB.MULT_OPE.COMP_L:
				return (double)((vl < vr) ? 1 : 0);
			case STB.MULT_OPE.COMP_LE:
				return (double)((vl <= vr) ? 1 : 0);
			default:
				return vr;
			}
		}

		private static double calcMul(double val_md, double v, int mul)
		{
			switch (mul)
			{
			case -1:
				return val_md / v;
			case 1:
				return val_md * v;
			case 2:
				return val_md % v;
			case 3:
				return (double)((int)val_md & (int)v);
			case 4:
				return (double)((int)val_md | (int)v);
			}
			return v;
		}

		private static void extractValue(STB Stb, int sti, int ei, out int end_char_index, float defval = 0f)
		{
			Stb.TaleTrimSpace(ei, out end_char_index, sti);
			Stb.SkipSpace(sti, out sti, end_char_index);
			ei = end_char_index;
			double num = 1.0;
			bool flag = false;
			bool flag2 = true;
			while (sti < ei)
			{
				if (Stb.Is('!', sti))
				{
					sti++;
					flag2 = !flag2;
				}
				else if (Stb.Is('+', sti) || Stb.isSpace(Stb[sti]))
				{
					sti++;
				}
				else
				{
					if (!Stb.Is('-', sti))
					{
						break;
					}
					sti++;
					num *= -1.0;
				}
			}
			if ((float)(ei - sti) <= 0f)
			{
				end_char_index = sti;
				TX.value_inputted = (double)defval * num;
				return;
			}
			if (Stb[sti] == '(')
			{
				Stb.ScrollBracket(sti, out end_char_index, ei);
				TX.value_inputted = TX.eval(Stb, sti + 1, end_char_index - 1);
			}
			else if (Stb[sti] == '\'')
			{
				Stb.ScrollNextQuote(sti, out end_char_index, ei);
				TX.value_inputted = TX.eval(Stb, sti + 1, end_char_index - 1);
			}
			else if (TX.isNumDotMatch(Stb, sti, ei) || TX.isAlphaColorHeaderMatch(Stb, sti, ei) || Stb.Is('#', sti))
			{
				STB.PARSERES parseres = Stb.Nm(sti, out end_char_index, ei, false);
				if (parseres == STB.PARSERES.DOUBLE)
				{
					TX.value_inputted = STB.parse_result_double;
				}
				else if (parseres == STB.PARSERES.INT)
				{
					TX.value_inputted = (double)STB.parse_result_int;
				}
				else
				{
					TX.value_inputted = (double)defval;
				}
			}
			else
			{
				Func<char, bool> func = (char c) => TX.isValEMatch(c);
				Func<char, bool> func2 = func;
				int num2 = -1;
				int num3 = 0;
				int num4 = 0;
				int num5 = sti - 1;
				bool flag3 = true;
				TX.Aeval_buf.Clear();
				while (++num5 < ei)
				{
					char c2 = Stb[num5];
					if (c2 == '\\')
					{
						num5++;
					}
					else if (c2 == '!' && flag3)
					{
						flag2 = !flag2;
					}
					else
					{
						if (c2 == ' ' || c2 == '\t' || c2 == '\n' || c2 == '\r')
						{
							break;
						}
						bool flag4 = flag3;
						flag3 = false;
						if (c2 == '[')
						{
							func = (char c) => TX.isNPlusMatch(c);
							if (num4 == 0)
							{
								if (num2 < 0)
								{
									num2 = num5;
								}
								num3 = num5;
							}
							num4++;
						}
						else if (c2 == ']')
						{
							if (num4 > 0 && --num4 == 0)
							{
								func = func2;
								TX.Aeval_buf.Add(Stb.get_slice(num3 + 1, num5));
								continue;
							}
							continue;
						}
						else if (num2 >= 0 && num4 == 0)
						{
							break;
						}
						if (flag4 && c2 == '(')
						{
							Stb.ScrollBracket(num5, out end_char_index, ei);
							double num6 = TX.eval(Stb, num5 + 1, end_char_index - 1);
							TX.value_inputted = (flag2 ? num6 : ((double)((num6 != 0.0) ? 0 : 1)));
							return;
						}
						if (!func(c2))
						{
							if (flag4)
							{
								flag = true;
							}
							else if (!flag)
							{
								break;
							}
						}
					}
				}
				end_char_index = num5;
				if (flag)
				{
					TX.value_inputted = num * (double)(flag2 ? defval : ((float)((defval != 0f) ? 0 : 1)));
					return;
				}
				int num7;
				Stb.TaleTrimSpace((num2 >= 0) ? num2 : num5, out num7, sti);
				Stb.SkipSpace(sti, out sti, num7);
				string text = Stb.get_slice(sti, num7);
				if (!TX.evalLsnConvert(text, null))
				{
					TX.value_inputted = (double)defval;
					X.dl("EVAL に失敗:" + text, null, false, true);
				}
			}
			if (!flag2)
			{
				TX.value_inputted = (double)((TX.value_inputted != 0.0) ? 0 : 1);
			}
			TX.value_inputted *= num;
		}

		public static bool InputE(double f)
		{
			TX.value_inputted = f;
			return true;
		}

		public static bool InputE(float f)
		{
			TX.value_inputted = (double)f;
			return true;
		}

		public static bool InputE(bool f)
		{
			TX.value_inputted = (double)(f ? 1 : 0);
			return true;
		}

		public static bool charIs(string str, int _at, char compare)
		{
			if (str == null)
			{
				return false;
			}
			char[] array = str.ToCharArray();
			return array.Length > _at && array[_at] == compare;
		}

		public static char ToUpper(char c)
		{
			if (X.BTWW(97f, (float)c, 122f))
			{
				return (char)(-32 + (int)c);
			}
			return c;
		}

		public static char ToLower(char c)
		{
			if (X.BTWW(65f, (float)c, 90f))
			{
				return ' ' + c;
			}
			return c;
		}

		public static string HeadLower(string s)
		{
			if (s.Length <= 1)
			{
				return s.ToLower();
			}
			return TX.ToLower(s[0]).ToString() + TX.slice(s, 1, s.Length);
		}

		public static string HeadUpper(string s)
		{
			if (s.Length <= 1)
			{
				return s.ToUpper();
			}
			return TX.ToUpper(s[0]).ToString() + TX.slice(s, 1, s.Length);
		}

		public static bool hasSpecificChar(string str, char c, bool ignore_case = true)
		{
			char c2 = c;
			if (ignore_case)
			{
				c2 = (X.BTWW(97f, (float)c, 122f) ? TX.ToUpper(c) : (X.BTWW(65f, (float)c, 90f) ? TX.ToLower(c) : c));
			}
			int length = str.Length;
			for (int i = 0; i < length; i++)
			{
				char c3 = str[i];
				if (c2 == c3 || c == c3)
				{
					return true;
				}
			}
			return false;
		}

		public static bool isStart(string str, string compare, int start_index = 0)
		{
			if (str == null || compare == null)
			{
				return false;
			}
			int length = compare.Length;
			if (str.Length - start_index < length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (str[i + start_index] != compare[i])
				{
					return false;
				}
			}
			return true;
		}

		public static bool isStart(string str, string compare, out string sliced_after, int start_index = 0)
		{
			if (TX.isStart(str, compare, start_index))
			{
				int num = start_index + compare.Length;
				sliced_after = TX.slice(str, num);
				return true;
			}
			sliced_after = null;
			return false;
		}

		public static bool isStart(string str, char[] Acompare)
		{
			int num = Acompare.Length;
			if (str.Length < num)
			{
				return false;
			}
			for (int i = 0; i < num; i++)
			{
				if (str[i] != Acompare[i])
				{
					return false;
				}
			}
			return true;
		}

		public static bool headerIs(string str, string header, int start_index = 0, char allocate_after_char = '_', bool allow_header_only = true)
		{
			if (TX.isStart(str, header, start_index))
			{
				int length = header.Length;
				if (allow_header_only && str.Length == start_index + length)
				{
					return true;
				}
				if (str[start_index + length] == allocate_after_char)
				{
					return true;
				}
			}
			return false;
		}

		public static bool isMatch(string str, string compare, int start_index = 0)
		{
			if (start_index == 0)
			{
				return str == compare;
			}
			if (str == null || compare == null)
			{
				return false;
			}
			int length = compare.Length;
			if (str.Length - start_index != length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (str[i + start_index] != compare[i])
				{
					return false;
				}
			}
			return true;
		}

		public static bool isStart(string str, STB Acompare)
		{
			int length = Acompare.Length;
			if (str.Length < length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (str[i] != Acompare[i])
				{
					return false;
				}
			}
			return true;
		}

		public static bool isStart(string str, char c)
		{
			return TX.valid(str) && str[0] == c;
		}

		public static int evalI(string tx)
		{
			return (int)TX.eval(tx, "");
		}

		public static STB PopBld(string initchar = null, int ensure_capacity = 0)
		{
			if (TX.ABld == null)
			{
				TX.ABld = new STB[20];
				TX.ABld[0] = new STB(128);
			}
			if (TX.builders_i >= TX.ABld.Length)
			{
				Array.Resize<STB>(ref TX.ABld, TX.builders_i + 12);
			}
			STB stb;
			if ((stb = TX.ABld[TX.builders_i]) == null)
			{
				stb = (TX.ABld[TX.builders_i] = new STB(128));
			}
			stb.Clear();
			if (ensure_capacity > 0)
			{
				stb.EnsureCapacity(ensure_capacity);
			}
			TX.builders_i++;
			if (initchar != null)
			{
				stb += initchar;
			}
			return stb;
		}

		public static void ReleaseBld(STB B)
		{
			if (TX.builders_i == 0 || B == null)
			{
				return;
			}
			if (B == TX.ABld[TX.builders_i - 1])
			{
				TX.builders_i--;
				return;
			}
			int num = X.isinC<STB>(TX.ABld, B, TX.builders_i);
			if (num >= 0)
			{
				X.shiftNotInput1<STB>(TX.ABld, num, ref TX.builders_i);
			}
		}

		public static string getResource(string path, string ext = ".csv", bool no_error = false)
		{
			DateTime dateTime = default(DateTime);
			return TX.getResource(path, ref dateTime, ext, no_error, "Resources/");
		}

		public static string getResource(string path, ref DateTime Date, string ext = ".csv", bool no_error = false, string path_prefix = "Resources/")
		{
			if (Date.Year >= 100)
			{
				return null;
			}
			Date = X.TimeEpoch;
			TextAsset textAsset = Resources.Load<TextAsset>(path);
			if (textAsset == null)
			{
				if (!no_error)
				{
					X.dl("テキストリソース " + path + "が見つかりません", null, true, false);
				}
				return null;
			}
			return textAsset.text;
		}

		public static byte[] getResourceBytes(string path, bool no_error = false)
		{
			TextAsset textAsset = Resources.Load<TextAsset>(path);
			if (textAsset == null)
			{
				if (!no_error)
				{
					X.dl("Bytes リソース " + path + ".bytes が見つかりません", null, true, false);
				}
				return null;
			}
			return textAsset.bytes;
		}

		public static string appendLast(string tx, string aft)
		{
			string[] array = new Regex("[\\n\\r] ").Split(tx);
			for (int i = array.Length - 1; i >= 0; i--)
			{
				if (i <= 0 || !(array[i] == ""))
				{
					string[] array2 = array;
					int num = i;
					array2[num] += aft;
					return string.Join("\n", array);
				}
			}
			return tx + aft;
		}

		public static string linePrefix(string tx, string p, int stl = 0, int el = 0)
		{
			string[] array = new Regex("[\\n\\r] ").Split(tx);
			el = array.Length + X.Mn(0, el);
			for (int i = stl; i < el; i++)
			{
				if (!(array[i] == ""))
				{
					array[i] = p + array[i];
				}
			}
			return string.Join("\n", array);
		}

		public static uint str2color(string t, uint _default = 4294967295U)
		{
			if (t != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(t);
				if (num <= 2316152773U)
				{
					if (num <= 1750494276U)
					{
						if (num != 268805718U)
						{
							if (num == 1750494276U)
							{
								if (t == "BLACK")
								{
									return TX.Acolors[1];
								}
							}
						}
						else if (t == "SKY")
						{
							return TX.Acolors[6];
						}
					}
					else if (num != 2211354620U)
					{
						if (num == 2316152773U)
						{
							if (t == "DARK")
							{
								return IN.MainDarkColor;
							}
						}
					}
					else if (t == "RED")
					{
						return TX.Acolors[2];
					}
				}
				else if (num <= 2856149510U)
				{
					if (num != 2539335743U)
					{
						if (num == 2856149510U)
						{
							if (t == "WHITE")
							{
								return TX.Acolors[0];
							}
						}
					}
					else if (t == "PURPLE")
					{
						return TX.Acolors[7];
					}
				}
				else if (num != 2875364188U)
				{
					if (num == 2964049737U)
					{
						if (t == "YELLOW")
						{
							return TX.Acolors[5];
						}
					}
				}
				else if (t == "GREEN")
				{
					return TX.Acolors[4];
				}
			}
			if (t.IndexOf("0x") == 0)
			{
				return X.NmUI(t.Substring(2), _default, false, true);
			}
			if (t.IndexOf(":#") == 2)
			{
				return X.NmUI(t, _default, false, true);
			}
			return _default;
		}

		public static uint str2num(string str)
		{
			int length = str.Length;
			uint num = 0U;
			for (int i = 0; i < length; i++)
			{
				num = (num << 8) + (uint)((byte)str[i]);
			}
			return num;
		}

		public static string slice(string str, int i)
		{
			return str.Substring(X.Mn(str.Length, i));
		}

		public static string slice(string str, int i, int e)
		{
			if (e < 0)
			{
				e = str.Length + e;
			}
			if (i >= str.Length || e <= i)
			{
				return "";
			}
			return str.Substring(i, e - i);
		}

		public static string findNextWordBorder(char[] Astr, int si = 0, int e = -1)
		{
			if (e < 0)
			{
				e = Astr.Length + e;
			}
			int num = -1;
			for (int i = si; i < e; i++)
			{
				char c = Astr[i];
				if (c == '\n' || c == '\r' || c == '\b' || c == ' ' || c == '\u3000' || c == '\t')
				{
					if (num >= 0)
					{
						e = i;
						break;
					}
				}
				else if (num < 0)
				{
					num = i;
				}
			}
			if (num < 0)
			{
				return "";
			}
			return new string(Astr, num, e - num);
		}

		public static bool noe(string s)
		{
			return string.IsNullOrEmpty(s);
		}

		public static bool valid(string s)
		{
			return !string.IsNullOrEmpty(s);
		}

		public static int ToNum(char c)
		{
			switch (c)
			{
			case '0':
				return 0;
			case '1':
				return 1;
			case '2':
				return 2;
			case '3':
				return 3;
			case '4':
				return 4;
			case '5':
				return 5;
			case '6':
				return 6;
			case '7':
				return 7;
			case '8':
				return 8;
			case '9':
				return 9;
			default:
				return -1;
			}
		}

		public static bool isReturn(string c)
		{
			return c == "\n" || c == "\r";
		}

		public static bool isReturn(char c)
		{
			return c == '\n' || c == '\r';
		}

		public static bool isReturnAt(string c, int _at)
		{
			return TX.charIs(c, _at, '\n') || TX.charIs(c, _at, '\r');
		}

		public static bool isEnd(string c, string s)
		{
			if (c.Length < s.Length)
			{
				return false;
			}
			int num = c.IndexOf(s);
			return num >= 0 && num == c.Length - s.Length;
		}

		public static bool isEnd(string c, char s)
		{
			return c != null && c.Length != 0 && c[c.Length - 1] == s;
		}

		public static string getLine(string str, int startChar = 0)
		{
			int num = startChar;
			while (startChar < str.Length && !TX.isReturn(str[startChar]))
			{
				startChar++;
			}
			return TX.slice(str, num, startChar);
		}

		public static string getLine(STB Stb, int startChar = 0)
		{
			string text = "";
			while (startChar < Stb.Length)
			{
				char c = Stb[startChar];
				if (TX.isReturn(c))
				{
					return text;
				}
				text += c.ToString();
				startChar++;
			}
			return text;
		}

		public static int countLine(string c)
		{
			int num = c.Length;
			int num2 = 1;
			while (--num >= 0)
			{
				char c2 = c[num];
				num2 += (TX.isReturn(c2) ? 1 : 0);
			}
			return num2;
		}

		public static int countLine(char[] Ac)
		{
			int num = Ac.Length;
			int num2 = 1;
			while (--num >= 0)
			{
				char c = Ac[num];
				num2 += (TX.isReturn(c) ? 1 : 0);
			}
			return num2;
		}

		public static int countLine(STB Ac)
		{
			int num = Ac.Length;
			int num2 = 1;
			while (--num >= 0)
			{
				char c = Ac[num];
				num2 += (TX.isReturn(c) ? 1 : 0);
			}
			return num2;
		}

		public static string fixLine(string c, int line)
		{
			int length = c.Length;
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				if (TX.isReturn(c[i]) && ++num >= line)
				{
					return TX.slice(c, 0, i);
				}
			}
			num++;
			while (num++ < line)
			{
				c += "\n";
			}
			return c;
		}

		public static int getWeight(string c)
		{
			return Encoding.GetEncoding(932).GetByteCount(c);
		}

		public static float qsCheckRelative(STB Stb, string str, bool strict = false)
		{
			if (TX.noe(str) || Stb.Length == 0)
			{
				return 1f;
			}
			int length = Stb.Length;
			int length2 = Stb.Length;
			int length3 = str.Length;
			int i = 0;
			int num = 0;
			int num2 = length2;
			char c = '0';
			int num3 = Stb.IndexOf(str, 0, -1);
			float num4;
			if (num3 >= 0)
			{
				num4 = 1.5f + (float)length3 / (float)length;
			}
			else
			{
				while (i < length3)
				{
					char c2 = str[i];
					num3 = Stb.IndexOf(c2, 0, -1);
					if (num3 < 0)
					{
						num2 -= Stb.Length;
						break;
					}
					if (i == 0)
					{
						num2 -= num3;
					}
					else if (num3 >= 1 && Stb.charIs(num3 - 1, c))
					{
						num2 -= num3;
					}
					else
					{
						num += num3;
					}
					i++;
					Stb.Splice(num3 + 1, -1);
					c = c2;
				}
				num4 = ((!strict || i >= length3) ? ((float)i / (float)length * 0.5f) : 0f);
				if (num4 > 0f)
				{
					num4 += ((num2 == 0) ? 0.5f : ((float)X.MMX(0, num2 - num, num2) / (float)num2 * 0.5f));
				}
				if (i >= length3)
				{
					num4 += 1f;
				}
			}
			return num4;
		}

		public static string[] split(string src, string separator)
		{
			List<string> list = new List<string>(1) { "" };
			int num = 0;
			int length = separator.Length;
			if (separator != "")
			{
				int num2;
				while ((num2 = src.IndexOf(separator)) >= 0)
				{
					string text = TX.slice(src, 0, num2);
					src = TX.slice(src, num2 + length);
					if (num == 0)
					{
						list[num] = text;
					}
					else
					{
						list.Add(text);
					}
					num++;
				}
			}
			if (num == 0)
			{
				list[num] = src;
			}
			else
			{
				list.Add(src);
			}
			return list.ToArray();
		}

		public static string[] split(string src, Regex RegSeparator)
		{
			List<string> list = new List<string>(1) { "" };
			int num = 0;
			REG reg = new REG();
			while (reg._match(src, RegSeparator))
			{
				string leftContext = reg._leftContext;
				src = reg._rightContext;
				if (num == 0)
				{
					list[num] = leftContext;
				}
				else
				{
					list.Add(leftContext);
				}
				num++;
			}
			if (num == 0)
			{
				list[num] = src;
			}
			else
			{
				list.Add(src);
			}
			return list.ToArray();
		}

		public static string join<T>(string splitter, T[] A, int sline = 0, int eline = -1)
		{
			int num = A.Length;
			if (num == 0)
			{
				return "";
			}
			if (eline < 0)
			{
				eline = num + 1 + eline;
			}
			string text = A[sline].ToString();
			for (int i = sline + 1; i < eline; i++)
			{
				text = text + splitter + A[i].ToString();
			}
			return text;
		}

		public static string join<T>(string splitter, List<T> A, int sline = 0, int eline = -1)
		{
			int count = A.Count;
			if (count == 0)
			{
				return "";
			}
			if (eline < 0)
			{
				eline = count + 1 + eline;
			}
			T t = A[sline];
			string text = t.ToString();
			for (int i = sline + 1; i < eline; i++)
			{
				string text2 = text;
				t = A[i];
				text = text2 + splitter + t.ToString();
			}
			return text;
		}

		public static string join2<T>(string splitter, T[] A, int sline = 0, int eline = -1) where T : class
		{
			int num = A.Length;
			if (num == 0)
			{
				return "";
			}
			if (eline < 0)
			{
				eline = num + 1 + eline;
			}
			string text = "";
			for (int i = sline; i < eline; i++)
			{
				T t = A[i];
				if (t != null)
				{
					text = text + ((text == "") ? "" : splitter) + t.ToString();
				}
			}
			return text;
		}

		public static void join(STB builder, string[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				builder += A[i];
			}
		}

		public static string escape_join<T>(string splitter, T[] A)
		{
			int num = A.Length;
			if (num == 0)
			{
				return "";
			}
			string text = TX.escape(A[0].ToString());
			for (int i = 1; i < num; i++)
			{
				text = text + splitter + TX.escape(A[i].ToString());
			}
			return text;
		}

		public static bool isLess(string a, string b)
		{
			int num = X.Mn(a.Length, b.Length);
			for (int i = 0; i < num; i++)
			{
				char c = a[i];
				char c2 = b[i];
				if (c != c2)
				{
					return c < c2;
				}
			}
			return a.Length < b.Length;
		}

		public static string remLF(string s)
		{
			return s.Replace('\n', ' ');
		}

		public static int text2id(string s, int max = 16777215)
		{
			int length = s.Length;
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				num = (num + (int)s[i] * (i & 63)) & max;
			}
			return num;
		}

		public static int text2id(STB s, int max = 16777215)
		{
			int length = s.Length;
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				num = (num + (int)s[i] * (i & 63)) & max;
			}
			return num;
		}

		public static string ToUnityCase(string s)
		{
			if (TX.RegAllUpper.Match(s).Success)
			{
				return s;
			}
			string text = "";
			int i = 0;
			string text2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			string text3 = "\n\r\t_";
			int num = 0;
			while (i < s.Length)
			{
				char c = s[i];
				if (text3.IndexOf(c) >= 0)
				{
					if (num > 0)
					{
						text += " ";
					}
				}
				else
				{
					if (text2.IndexOf(c) >= 0 && num > 0)
					{
						text += " ";
					}
					text += c.ToString();
					if (num == 0)
					{
						text = text.ToUpper();
					}
					num++;
				}
				i++;
			}
			return text;
		}

		public static string escape(string tx)
		{
			if (tx != null)
			{
				return Uri.EscapeDataString(tx);
			}
			return "";
		}

		public static string unescape(string tx)
		{
			if (tx != null)
			{
				return Uri.UnescapeDataString(tx);
			}
			return "";
		}

		public static string escapeSlash(string tx)
		{
			return tx.Replace("<", "\\<").Replace(">", "\\>");
		}

		public static string coltag(uint c)
		{
			return "<font color=\"0x" + C32.codeToCodeText(c) + "\">";
		}

		public static string colend
		{
			get
			{
				return "</font>";
			}
		}

		public static string decodeURIComponent(string tx)
		{
			if (tx != null)
			{
				return Uri.UnescapeDataString(tx);
			}
			return "";
		}

		public static long ToUnixTime(DateTime targetTime)
		{
			targetTime = targetTime.ToUniversalTime();
			return (long)(targetTime - TX.UNIX_EPOCH).TotalSeconds;
		}

		public static DateTime FromUnixTime(long unixTime)
		{
			return TX.UNIX_EPOCH.AddSeconds((double)unixTime).ToLocalTime();
		}

		public static string default_family = "_";

		public static string debug_checking_family = null;

		public static readonly string tx_script_full_dir = Path.Combine(Application.streamingAssetsPath, "localization");

		public const string letterspace_dir = "__AdditionalFonts";

		private const string tx_script_dir = "localization";

		public const string asset_bundle_ext = ".dat";

		private const string RegDef = "&\\{([^\\}\\n\\r]+)\\}";

		private static readonly Regex RegUri = new Regex("%([0-9A-Fa-f][0-9A-Fa-f])");

		public static readonly Regex RegAllUpper = new Regex("^[0-9A-Z_\\-\\:/. ]*$");

		private static TX.TXFamily TxCon;

		private static TX.TXFamily TxConDef;

		private static TX.TXFamily TxConEng;

		private static BDic<string, TX.TXFamily> OTxFam;

		private static BDic<object, TxEvalListenerContainer> OLsnEval;

		private static BDic<string, FnTxEval> OLsnEvalStaticFn;

		private static bool need_txeval_static_reassign;

		private static List<string> Aeval_buf;

		private static List<string> Atx_buf;

		private static TX TX_EMPTY;

		public const string lf = "\n";

		public const string tx_data_ext = ".txt";

		public string title;

		private string _data = "";

		public static readonly uint[] Acolors = new uint[] { 16777215U, 0U, 16711680U, 255U, 65280U, 16776960U, 65535U, 16711935U };

		public const string alphabet_big = "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ１２３４５６７８９０";

		public const string alphabet_small = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

		private static STB[] ABld;

		private static int builders_i;

		private static Func<char, bool> FnIsWMatch = (char c) => TX.isWMatch(c);

		public static double value_inputted;

		private static readonly Regex RegS = new Regex("[\\s\\t\\n\\r]");

		private static readonly Regex RegN = new Regex("[\\d\\.]");

		private static readonly Regex RegNE = new Regex("[\\.\\d\\[\\]]");

		private static readonly Regex RegNX = new Regex("[a-fA-F\\.\\d\\[\\]]");

		private static readonly Regex RegNPlus = new Regex("[a-zA-Z0-9_\\.\\d\\[\\]\\%\\#\\@\\& \\s\\t\\-\\+\\*\\/]");

		private static readonly Regex RegH = new Regex("[\\+\\-\\*\\/\\|\\&]");

		private static readonly Regex RegV = new Regex("[\\.\\$a-zA-Z0-9_]");

		private static readonly Regex RegVE = new Regex("[\\.\\$a-zA-Z0-9_\\[\\]]");

		private static readonly Regex RegTF = new Regex("^(\\|\\||\\&\\&|[\\=\\!><]\\=|>|<)");

		private static readonly Regex RegIntDefine = new Regex("^int *\\.? *");

		private static readonly Regex RegNumE = new Regex("([eE][\\-\\+]\\d+)");

		private static readonly Regex RegNumHex = new Regex("^(?:0x|(?:([a-fA-F0-9][a-fA-F0-9])\\:)?\\#)");

		private static readonly Regex RegFnRand = new Regex("^rand\\[([0-9]+)\\]$");

		private static readonly Regex RegFnAbs = new Regex("^[Aa]bs\\[([\\-0-9\\.\\+\\-E]+)\\]$");

		private static readonly Regex RegFnXd = new Regex("^[Xx][Dd]\\[([\\-0-9lrtbLRTB]+)\\]$");

		private static readonly Regex RegFnYd = new Regex("^[Yy][Dd]\\[([\\-0-9lrtbLRTB]+)\\]$");

		private static readonly Regex RegFnCos = new Regex("^[Cc][Oo][Ss]\\[([\\-0-9\\.\\+\\-E]+)\\]$");

		private static readonly Regex RegFnSin = new Regex("^[Ss][Ii][Nn]\\[([\\-0-9\\.\\+\\-E]+)\\]$");

		private static readonly Regex RegFnGar = new Regex("^GAR\\[(\\-?[0-9\\.\\+\\+E]+)\\]\\[(\\-?[0-9\\.\\+\\+E]+)\\]\\[(\\-?[0-9\\.\\+\\+E]+)\\]\\[(\\-?[0-9\\.\\+\\+E]+)\\]$");

		public static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public sealed class TXFamily
		{
			public TXFamily(string _key, string smp, string fn, int capacity = 128, TX.TXFamily Pre_Fam = null)
			{
				this.key = _key;
				this.simple_name = (TX.valid(smp) ? smp : this.key);
				this.full_name = (TX.valid(fn) ? fn : this.simple_name);
				if (Pre_Fam != null)
				{
					capacity = X.Mx(capacity, Pre_Fam.OTx.Count);
					this.Mti = Pre_Fam.Mti;
					this.BannerIcon_ = Pre_Fam.BannerIcon_;
				}
				this.OTx = new NDic<TX>("TX_" + _key, capacity);
			}

			public void scriptFinalize()
			{
				this.OTx.scriptFinalize();
			}

			public TX Get(string title)
			{
				return X.Get<string, TX>(this.OTx, title);
			}

			public TX.TXFamily Add(TX Tx)
			{
				this.OTx[Tx.title] = Tx;
				return this;
			}

			public void inputSystemLanguage(string[] As)
			{
				if (As == null || As.Length == 0)
				{
					return;
				}
				List<SystemLanguage> list = new List<SystemLanguage>();
				int num = As.Length;
				for (int i = 0; i < num; i++)
				{
					try
					{
						SystemLanguage systemLanguage = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), As[i]);
						list.Add(systemLanguage);
					}
					catch
					{
					}
				}
				if (list.Count > 0)
				{
					this.AsystemLanguage = list.ToArray();
				}
			}

			public void releaseBannerIcon()
			{
				this.BannerIcon_ = BLIT.nDispose(this.BannerIcon_);
			}

			public Texture getBannerIcon()
			{
				if (this.BannerIcon_ == null)
				{
					byte[] array = NKT.readSpecificFileBinary(Path.Combine(Path.Combine(TX.tx_script_full_dir, this.key), "icon.png"), 0, 0, true);
					Texture2D texture2D = new Texture2D(26, 24, TextureFormat.RGB24, false);
					texture2D.LoadImage(array);
					texture2D.Apply(false, true);
					this.BannerIcon_ = texture2D;
				}
				return this.BannerIcon_;
			}

			public string asset_bundle_font_path
			{
				get
				{
					if (this.asset_bundle_font_path_ == null)
					{
						this.prepareAssetBundlePath(null);
					}
					return this.asset_bundle_font_path_;
				}
			}

			public void prepareAssetBundlePath(string[] Abundle_to)
			{
				string[] array;
				if (Abundle_to != null)
				{
					array = new string[Abundle_to.Length + 1];
					array[0] = TX.tx_script_full_dir;
					Array.Copy(Abundle_to, 0, array, 1, Abundle_to.Length);
				}
				else
				{
					array = new string[]
					{
						TX.tx_script_full_dir,
						this.key,
						this.key + "_font"
					};
				}
				this.asset_bundle_font_path_ = Path.Combine(array);
				if (!File.Exists(this.asset_bundle_font_path_ + ".dat"))
				{
					array[0] = NKT.appDirectory;
					this.asset_bundle_font_path_ = Path.Combine(array);
					X.dl("find bundle to Application Root", null, false, false);
				}
				int num = array.Length - 1;
				if (num == 1)
				{
					this.letter_space_scipt_dir_ = array[0];
					return;
				}
				for (int i = 1; i < num; i++)
				{
					this.letter_space_scipt_dir_ = Path.Combine((i == 1) ? array[0] : this.letter_space_scipt_dir_, array[i]);
				}
			}

			public string letter_space_scipt_dir
			{
				get
				{
					if (this.letter_space_scipt_dir_ == null)
					{
						this.letter_space_scipt_dir_ = Path.Combine(new string[] { Path.Combine(TX.tx_script_full_dir, "__AdditionalFonts") });
					}
					return this.letter_space_scipt_dir_;
				}
			}

			public void prepareLanguage()
			{
				if (TX.noe(this.bundle_font_def))
				{
					return;
				}
				if (this.Mti == null)
				{
					this.Mti = MTI.LoadContainer("/" + this.asset_bundle_font_path, this.key);
				}
				if (this.BundleFontDef == null && this.Mti != null)
				{
					Font font = this.Mti.Load<Font>(this.bundle_font_def);
					if (font != null)
					{
						this.BundleFontDef = MTRX.addFontStorageBundled(font, this.Mti, this.bundle_font_def, this.bundle_font_base_height, this.bundle_font_yshift, this.bundle_font_xratio, this.bundle_font_xratio_1byte, this.bundle_font_def_default_renderer_size, Path.Combine(this.letter_space_scipt_dir, "letterspace_" + this.bundle_font_def + ".txt")).TargetFont;
					}
					if (this.bundle_font_tit == this.bundle_font_def)
					{
						this.BundleFontTit = this.BundleFontDef;
					}
					else
					{
						font = this.Mti.Load<Font>(this.bundle_font_tit);
						if (font != null)
						{
							this.BundleFontTit = MTRX.addFontStorageBundled(font, this.Mti, this.bundle_font_tit, this.bundle_font_base_height, this.bundle_font_yshift, this.bundle_font_xratio, this.bundle_font_xratio_1byte, this.bundle_font_tit_default_renderer_size, Path.Combine(this.letter_space_scipt_dir, "letterspace_" + this.bundle_font_def + ".txt")).TargetFont;
						}
					}
				}
				this.bundle_font_def = null;
			}

			public MFont getDefaultFont()
			{
				if (TX.valid(this.bundle_font_def))
				{
					this.prepareLanguage();
				}
				if (this.BundleFontDef != null)
				{
					return this.BundleFontDef;
				}
				if (!this.use_ariel)
				{
					return MTRX.getDefaultFont();
				}
				return MTRX.getArialFont();
			}

			public MFont getTitleFont()
			{
				if (TX.valid(this.bundle_font_tit))
				{
					this.prepareLanguage();
				}
				if (this.BundleFontTit != null)
				{
					return this.BundleFontTit;
				}
				if (!this.use_ariel)
				{
					return MTRX.getTitleFontDefault();
				}
				return MTRX.getArialFont();
			}

			public int Count
			{
				get
				{
					return this.OTx.Count;
				}
			}

			public void checkDebugExting(TX.TXFamily CheckTarget)
			{
				foreach (KeyValuePair<string, TX> keyValuePair in this.OTx)
				{
					TX tx;
					if (!CheckTarget.OTx.TryGetValue(keyValuePair.Key, out tx) && !TX.isStart(keyValuePair.Key, "Input_", 0) && !TX.isStart(keyValuePair.Key, "PadInput_", 0))
					{
						Debug.Log(CheckTarget.key + "に存在しないTX: " + keyValuePair.Key);
					}
				}
			}

			public string key;

			public string simple_name;

			public string full_name;

			public bool is_english;

			public bool is_space_delimiter;

			public bool is_default;

			public bool use_ariel;

			public string asset_bundle_font_path_;

			public string bundle_font_def;

			public string bundle_font_tit;

			public float bundle_font_def_default_renderer_size;

			public float bundle_font_tit_default_renderer_size;

			public float bundle_font_base_height = 1f;

			public float bundle_font_yshift = 0.88f;

			public float bundle_font_xratio = 1f;

			public float bundle_font_xratio_1byte = 0.7f;

			public bool consider_leftshift;

			public string letter_space_scipt_dir_;

			private MTI Mti;

			private MFont BundleFontDef;

			private MFont BundleFontTit;

			private Texture BannerIcon_;

			private readonly NDic<TX> OTx;

			public SystemLanguage[] AsystemLanguage;

			private const string bundled_letterspace_header = "letterspace_";

			private const string bundled_letterspace_suffix = ".txt";
		}
	}
}
