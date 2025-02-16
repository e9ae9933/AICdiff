using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using evt;
using PixelLiner.PixelLinerLib;

namespace XX
{
	public static class GF
	{
		public static void initErrorMsg()
		{
			GF.errorMsg = new BDic<string, string>();
			GF.errorMsg["T0"] = "ヘッダエラー";
			GF.errorMsg["T1"] = "ヘッダエラー(2)";
			GF.errorMsg["TV"] = "バージョンエラー";
			GF.errorMsg["T2"] = "スコア内容が空です";
		}

		private static void swapData()
		{
			if (GF.Aswap_map == null)
			{
				return;
			}
			int num = GF.Aswap_map.Length;
			for (int i = 0; i < num; i++)
			{
				string text = GF.Aswap_map[i];
				if (REG.match(text, GF.RegSwap))
				{
					bool flag = REG.R2 == "<>";
					bool flag2 = REG.R2 == "=>";
					string text2 = null;
					string text3 = null;
					int num2 = -1;
					int num3 = -1;
					string text4 = TX.slice(REG.R1, 1);
					if (REG.R1.IndexOf("C") == 0)
					{
						text2 = "c";
						if (GF.Onamed_c.ContainsKey(text4))
						{
							num2 = GF.Onamed_c[text4];
						}
						else
						{
							num2 = X.NmI(text4, -1, false, false);
						}
					}
					else if (REG.R1.IndexOf("B") == 0)
					{
						text2 = "b";
						if (GF.Onamed_b.ContainsKey(text4))
						{
							num2 = GF.Onamed_b[text4];
						}
						else
						{
							num2 = X.NmI(text4, -1, false, false);
						}
					}
					else
					{
						text4 = REG.R1;
						if (GF.Onamed_c.ContainsKey(text4))
						{
							num2 = GF.Onamed_c[text4];
							text2 = "c";
						}
						else if (GF.Onamed_b.ContainsKey(text4))
						{
							num2 = GF.Onamed_b[text4];
							text2 = "b";
						}
					}
					text4 = TX.slice(REG.R3, 1);
					if (REG.R3.IndexOf("C") == 0)
					{
						text3 = "c";
						if (GF.Onamed_c.ContainsKey(text4))
						{
							num3 = GF.Onamed_c[text4];
						}
						else
						{
							num3 = X.NmI(text4, -1, false, false);
						}
					}
					else if (REG.R3.IndexOf("B") == 0)
					{
						text3 = "b";
						if (GF.Onamed_b.ContainsKey(text4))
						{
							num3 = GF.Onamed_b[text4];
						}
						else
						{
							num3 = X.NmI(text4, -1, false, false);
						}
					}
					else
					{
						text4 = REG.R3;
						if (GF.Onamed_c.ContainsKey(text4))
						{
							num3 = GF.Onamed_c[text4];
							text3 = "c";
						}
						else if (GF.Onamed_b.ContainsKey(text4))
						{
							num3 = GF.Onamed_b[text4];
							text3 = "b";
						}
					}
					if (text2 == null || !X.BTW(0f, (float)num2, (text2 == "c") ? GF.maxc : GF.maxb))
					{
						X.de("GF::swapData 不正なスワップ元文字列: " + text, null);
					}
					else if (text3 == null || !X.BTW(0f, (float)num3, (text3 == "c") ? GF.maxc : GF.maxb))
					{
						X.de("GF::swapData 不正なスワップ先文字列: " + text, null);
					}
					else
					{
						uint num4 = ((text2 == "c") ? GF.getC(num2) : (GF.getB(num2) ? 1U : 0U));
						uint num5 = ((text3 == "c") ? GF.getC(num3) : (GF.getB(num3) ? 1U : 0U));
						if (text3 == "c")
						{
							GF.setC(num3, num4);
						}
						else
						{
							GF.setB(num3, num4 != 0U);
						}
						if (!flag2)
						{
							if (text2 == "c")
							{
								GF.setC(num2, flag ? num5 : 0U);
							}
							else
							{
								GF.setB(num2, flag && num5 != 0U);
							}
						}
					}
				}
				else
				{
					X.de("GF::swapData 不正なスワップ値: " + text, null);
				}
			}
		}

		public static void init(uint _maxc = 0U, uint _maxb = 0U)
		{
			GF.maxc = ((_maxc > 0U) ? _maxc : 128U);
			GF.maxb = ((_maxb > 0U) ? _maxb : 128U);
			GF.Aflags_c = new uint[X.IntC(GF.maxc / 8U)];
			GF.Aflags_b = new uint[X.IntC(GF.maxb / 32U)];
			GF.maxc = (uint)((long)GF.Aflags_c.Length * 8L);
			GF.maxb = (uint)(GF.Aflags_b.Length * 32);
		}

		public static void clear()
		{
			if (!GF.initted)
			{
				return;
			}
			int num = GF.Aflags_c.Length;
			for (int i = 0; i < num; i++)
			{
				GF.Aflags_c[i] = 0U;
			}
			num = GF.Aflags_b.Length;
			for (int i = 0; i < num; i++)
			{
				GF.Aflags_b[i] = 0U;
			}
		}

		public static bool initted
		{
			get
			{
				return GF.Aflags_c != null;
			}
		}

		public static void defineNameC(string k, int i)
		{
			GF.Onamed_c[k] = i;
		}

		public static void defineNameB(string k, int i)
		{
			GF.Onamed_b[k] = i;
		}

		public static void setC2(int i, int v)
		{
			GF.setC(i, (uint)X.MMX(0f, (float)v, 15f));
		}

		public static void setC2(string key, int v)
		{
			GF.setC(GF.getReplaceKeyForC(key), (uint)X.MMX(0f, (float)v, 15f));
		}

		public static void setC(int i, uint v)
		{
			if (!X.BTW(0f, (float)i, GF.maxc))
			{
				return;
			}
			if (v >= 15U)
			{
				GF.Aflags_c[i >> 3] |= 15U << (int)((byte)(4L * ((long)i & 7L)));
				return;
			}
			GF.Aflags_c[i >> 3] &= ~(15U << (int)((byte)(4L * ((long)i & 7L))));
			GF.Aflags_c[i >> 3] |= (v & 15U) << (int)((byte)(4L * ((long)i & 7L)));
		}

		public static void setC(string key, uint v)
		{
			GF.setC(GF.getReplaceKeyForC(key), v);
		}

		public static uint getC(int i)
		{
			if (!X.BTW(0f, (float)i, GF.maxc))
			{
				return 0U;
			}
			return (GF.Aflags_c[i >> 3] >> (int)((byte)(4L * ((long)i & 7L)))) & 15U;
		}

		public static uint getC(string key)
		{
			return GF.getC(GF.getReplaceKeyForC(key));
		}

		public static void setB(string key, bool f)
		{
			GF.setB(GF.getReplaceKeyForB(key), f);
		}

		public static void setB(int i, bool f)
		{
			if (!X.BTW(0f, (float)i, GF.maxb))
			{
				return;
			}
			if (!f)
			{
				GF.Aflags_b[i >> 5] &= ~(1U << (int)((byte)(i & 31)));
				return;
			}
			GF.Aflags_b[i >> 5] |= 1U << (int)((byte)(i & 31));
		}

		public static bool getB(string key)
		{
			return GF.getB(GF.getReplaceKeyForB(key));
		}

		public static bool getB(int i)
		{
			return X.BTW(0f, (float)i, GF.maxb) && ((GF.Aflags_b[i >> 5] >> (int)((byte)(i & 31))) & 1U) != 0U;
		}

		public static int getReplaceKeyForC(string key)
		{
			return X.Get<string, int>(GF.Onamed_c, key, -1);
		}

		public static int getReplaceKeyForB(string key)
		{
			return X.Get<string, int>(GF.Onamed_b, key, -1);
		}

		public static ByteArray readFromSvString(ByteArray Ba)
		{
			try
			{
				GF.init(0U, 0U);
				int num = Ba.readByte();
				int num2 = Ba.readByte();
				if (num < 0)
				{
					num += 256;
				}
				if (num2 < 0)
				{
					num2 += 256;
				}
				if (num > GF.Aflags_c.Length)
				{
					IN.Throw("GFC個数エラー");
				}
				if (num2 > GF.Aflags_b.Length)
				{
					IN.Throw("GFB個数エラー");
				}
				for (int i = 0; i < num; i++)
				{
					GF.Aflags_c[i] = Ba.readUnsignedInt();
				}
				for (int i = 0; i < num2; i++)
				{
					GF.Aflags_b[i] = Ba.readUnsignedInt();
				}
				if (GF.Aswap_map != null)
				{
					GF.swapData();
				}
			}
			catch (Exception ex)
			{
				X.de("フラグの読込でエラー発生:" + ex.ToString(), null);
				return null;
			}
			return Ba;
		}

		public static ByteArray writeSvString(ByteArray Ba)
		{
			try
			{
				int num = GF.Aflags_c.Length - 1;
				while (num >= 0 && GF.Aflags_c[num] == 0U)
				{
					num--;
				}
				num++;
				Ba.writeByte((int)((byte)num));
				int num2 = GF.Aflags_b.Length - 1;
				while (num2 >= 0 && GF.Aflags_b[num2] == 0U)
				{
					num2--;
				}
				num2++;
				Ba.writeByte(num2);
				for (int i = 0; i < num; i++)
				{
					Ba.writeUInt(GF.Aflags_c[i]);
				}
				for (int i = 0; i < num2; i++)
				{
					Ba.writeUInt(GF.Aflags_b[i]);
				}
			}
			catch
			{
				X.de("フラグの書き込みでエラー発生", null);
				return null;
			}
			return Ba;
		}

		public static string getDebugStringForTextRenderer()
		{
			string text = "";
			for (int i = 0; i < 2; i++)
			{
				text = string.Concat(new string[]
				{
					text,
					TX.coltag(4294924376U),
					"<b>=== ",
					(i == 0) ? "GFB" : "GFC",
					" === </b>",
					TX.colend,
					"\n"
				});
				int num = ((i == 0) ? 128 : 128);
				uint num2 = 0U;
				int[] array = null;
				int num3 = 0;
				int num4;
				if (i == 0)
				{
					num4 = 128;
				}
				else
				{
					array = new int[GF.Osplitter_c.Count];
					GF.Osplitter_c.Keys.CopyTo(array, 0);
					new SORT<int>(null).qSort(array, X.FnSortIntager, -1);
					num3 = 0;
					num4 = ((num3 < array.Length) ? array[num3] : num);
				}
				for (int j = 0; j < num; j++)
				{
					if (j > 0)
					{
						if (j == num4)
						{
							if (num2 != 0U)
							{
								text += TX.colend;
							}
							text = string.Concat(new string[]
							{
								text,
								"\n- <b><i>",
								TX.coltag(4294573901U),
								GF.Osplitter_c[j],
								TX.colend,
								"</i></b>\n"
							});
							num2 = 0U;
							num3++;
							num4 = ((num3 < array.Length) ? array[num3] : num);
						}
						else if (j % 10 == 0)
						{
							if (j % 50 == 0)
							{
								if (num2 != 0U)
								{
									text += TX.colend;
								}
								text = string.Concat(new string[]
								{
									text,
									"\n- <i>",
									TX.coltag(4294573901U),
									j.ToString(),
									TX.colend,
									"</i>\n"
								});
								num2 = 0U;
							}
							else
							{
								text += "\n";
							}
						}
						else if (j % 5 == 0)
						{
							text += " ";
						}
					}
					string text2;
					uint num5;
					if (i == 0)
					{
						if (GF.getB(j))
						{
							text2 = "/";
							num5 = uint.MaxValue;
						}
						else
						{
							text2 = "_";
							num5 = 4286940549U;
						}
					}
					else
					{
						uint c = GF.getC(j);
						if (c > 0U)
						{
							text2 = c.ToString("x");
							num5 = uint.MaxValue;
						}
						else
						{
							text2 = "_";
							num5 = 4286940549U;
						}
						GF.TEMPLV templv = X.Get<int, GF.TEMPLV>(GF.Otemporary_level_c, j, GF.TEMPLV.NORMAL);
						if (templv > GF.TEMPLV.NORMAL)
						{
							num5 = (num5 & 16777215U) | ((templv == GF.TEMPLV.CLEAR_ON_MAPCHANGE) ? 2566914048U : 3154116608U);
						}
					}
					if (num5 != num2)
					{
						if (num2 != 0U)
						{
							text += TX.colend;
						}
						text += TX.coltag(num2 = num5);
					}
					text += text2;
				}
				if (num2 != 0U)
				{
					text += TX.colend;
				}
				text += "\n\n";
			}
			return text;
		}

		public static void flushGfc(bool in_area_change)
		{
			GF.TEMPLV templv = (in_area_change ? GF.TEMPLV.CLEAR_ON_FLUSH : GF.TEMPLV.CLEAR_ON_MAPCHANGE);
			foreach (KeyValuePair<int, GF.TEMPLV> keyValuePair in GF.Otemporary_level_c)
			{
				if (keyValuePair.Value >= templv)
				{
					GF.setC(keyValuePair.Key, 0U);
				}
			}
		}

		public static bool readEvLineGf(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2223327817U)
				{
					if (num <= 243851437U)
					{
						if (num != 117500799U)
						{
							if (num != 243851437U)
							{
								return false;
							}
							if (!(cmd == "PVV_ABSOLUTE"))
							{
								return false;
							}
							return rER.tError(GF.setPVV(rER.IntE(1, 0), true));
						}
						else
						{
							if (!(cmd == "DEFINE_GFC_NAME"))
							{
								return false;
							}
							int num2;
							GF.defineNameC(rER._2, num2 = (int)rER._N1);
							GF.TEMPLV templv = (GF.TEMPLV)rER.Int(3, 0);
							if (templv > GF.TEMPLV.NORMAL)
							{
								GF.Otemporary_level_c[num2] = templv;
								return true;
							}
							return true;
						}
					}
					else if (num != 1020392786U)
					{
						if (num != 1499723975U)
						{
							if (num != 2223327817U)
							{
								return false;
							}
							if (!(cmd == "GFB_SET"))
							{
								return false;
							}
							goto IL_01DB;
						}
						else if (!(cmd == "GFC_PUT"))
						{
							return false;
						}
					}
					else if (!(cmd == "GFC_SET_MX"))
					{
						return false;
					}
				}
				else if (num <= 2746773653U)
				{
					if (num != 2479854664U)
					{
						if (num != 2636597474U)
						{
							if (num != 2746773653U)
							{
								return false;
							}
							if (!(cmd == "GFC_SEPARATOR"))
							{
								return false;
							}
							GF.Osplitter_c[rER.Int(1, 0)] = rER._2;
							return true;
						}
						else if (!(cmd == "GFC_SET"))
						{
							return false;
						}
					}
					else
					{
						if (!(cmd == "GFB_PUT"))
						{
							return false;
						}
						goto IL_01DB;
					}
				}
				else if (num != 2921794231U)
				{
					if (num != 3594590254U)
					{
						if (num != 4110780549U)
						{
							return false;
						}
						if (!(cmd == "GFC_PUT_MX"))
						{
							return false;
						}
					}
					else
					{
						if (!(cmd == "DEFINE_GFB_NAME"))
						{
							return false;
						}
						GF.defineNameB(rER._2, (int)rER._N1);
						return true;
					}
				}
				else
				{
					if (!(cmd == "PVV"))
					{
						return false;
					}
					return rER.tError(GF.setPVV(rER.IntE(1, 0), false));
				}
				rER.tError(GF.commandGfcSet(rER._1, rER._2, rER.cmd == "GFC_SET_MX" || rER.cmd == "GFC_PUT_MX"));
				return true;
				IL_01DB:
				rER.tError(GF.commandGfbSet(rER._1, rER._2));
				return true;
			}
			return false;
		}

		public static string setPVV(int v, bool ignore_min_flag)
		{
			if (v < 0)
			{
				return "不正な数値";
			}
			if (GF.Onamed_c.ContainsKey("PHASE") && GF.Onamed_c.ContainsKey("PHASEV"))
			{
				int num = (int)(GF.getC(GF.Onamed_c["PHASE"]) * 100U + GF.getC(GF.Onamed_c["PHASEV"]));
				if (v > num || ignore_min_flag)
				{
					GF.commandGfcSet("PHASE", (v / 100).ToString(), false);
					GF.commandGfcSet("PHASEV", (v % 100).ToString(), false);
				}
				return null;
			}
			return "PHASE,PHASEV のラベルが設定されていません";
		}

		public static string commandGfcSet(string key, string val, bool max_mode)
		{
			if (val == "")
			{
				return "GFC_PUT: 値が空";
			}
			int num = (GF.Onamed_c.ContainsKey(key) ? GF.Onamed_c[key] : X.NmI(key, 0, false, false));
			if (max_mode)
			{
				GF.setC2(num, X.Mx((int)GF.getC(num), TX.evalI(val)));
			}
			else
			{
				int num2 = TX.commandEvalSet(val, (int)GF.getC(num));
				GF.setC2(num, num2);
			}
			return null;
		}

		public static string commandGfbSet(string key, string val)
		{
			if (val == "")
			{
				return "GFB_PUT: 値が空";
			}
			if (GF.Onamed_b.ContainsKey(key))
			{
				GF.setB(GF.Onamed_b[key], TX.evalI(val) != 0);
			}
			else
			{
				GF.setB(X.NmI(key, 0, false, false), X.Nm(val, 0f, false) != 0f);
			}
			return null;
		}

		public static void createListenerEval(TxEvalListenerContainer EvalT)
		{
			GF.GfEvalT = EvalT;
			EvalT.Add("PHASE", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(GF.getC("PHASE"));
			}, Array.Empty<string>());
			EvalT.Add("PHASEV", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(GF.getC("PHASEV"));
			}, Array.Empty<string>());
			EvalT.Add("PVV", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(GF.getC("PHASE") * 100U + GF.getC("PHASEV"));
			}, Array.Empty<string>());
			EvalT.Add("GFC", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				string text = X.Get<string>(Aargs, 0);
				int num;
				if (GF.Onamed_c.TryGetValue(text, out num))
				{
					TX.InputE(GF.getC(num));
					return;
				}
				int num2 = X.NmI(text, -1, true, false);
				if (X.BTW(0f, (float)num2, GF.maxc))
				{
					TX.InputE(GF.getC(num2));
					return;
				}
				X.de("GFC インデックスエラー: " + text, null);
				TX.InputE(0f);
			}, Array.Empty<string>());
			EvalT.Add("GFB", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				string text2 = X.Get<string>(Aargs, 0);
				int num3;
				if (GF.Onamed_b.TryGetValue(text2, out num3))
				{
					TX.InputE(GF.getB(num3));
					return;
				}
				int num4 = X.NmI(text2, -1, true, false);
				if (X.BTW(0f, (float)num4, GF.maxb))
				{
					TX.InputE(GF.getB(num4));
					return;
				}
				X.de("GFB インデックスエラー: " + text2, null);
				TX.InputE(0f);
			}, Array.Empty<string>());
		}

		public static string getPVV()
		{
			return (GF.getC("PHASE") * 100U + GF.getC("PHASEV")).ToString();
		}

		public static string TxEvalContentForDebug(string key)
		{
			if (key == "PHASE" || key == "PHASEV")
			{
				key = "GFC[" + key + "]";
			}
			if (REG.match(key, GF.RegGF))
			{
				string r = REG.R2;
				bool flag = TX.charIs(REG.R1, 0, 'C');
				if (flag && GF.Onamed_c.ContainsKey(r))
				{
					return GF.getC(GF.Onamed_c[r]).ToString();
				}
				if (flag || !GF.Onamed_b.ContainsKey(r))
				{
					return (flag ? GF.getC(X.NmI(r, 0, false, false)) : (GF.getB(X.NmI(r, 0, false, false)) ? 1U : 0U)).ToString();
				}
				if (!GF.getB(GF.Onamed_b[r]))
				{
					return "0";
				}
				return "1";
			}
			else
			{
				if (key == "PVV" && GF.Onamed_c.ContainsKey("PHASE") && GF.Onamed_c.ContainsKey("PHASEV"))
				{
					return (GF.getC(GF.Onamed_c["PHASE"]) * 100U + GF.getC(GF.Onamed_c["PHASEV"])).ToString();
				}
				return null;
			}
		}

		public const int GFC_MAX = 128;

		public const int GFB_MAX = 128;

		public static uint maxc;

		public static uint maxb;

		public const byte C_B_SHIFT = 5;

		public const byte C_B_BIT = 31;

		public const byte C_USE_BIT = 4;

		public const byte C_USE_BIT_SHIFT = 2;

		public const uint C_ITEM_HOLD = 8U;

		public const byte C_ITEM_SHIFT = 3;

		public const uint C_ITEM_BIT = 7U;

		public const uint C_VAL_BIT = 15U;

		public static string[] Aswap_map = null;

		public static Regex RegSwap = new Regex("/^([\\w\\.]+)([<\\=\\-]>)([\\w\\.]+)$/");

		public static uint[] Aflags_c = null;

		public static uint[] Aflags_b = null;

		public static BDic<string, int> Onamed_c = new BDic<string, int>();

		public static BDic<string, int> Onamed_b = new BDic<string, int>();

		private static BDic<int, GF.TEMPLV> Otemporary_level_c = new BDic<int, GF.TEMPLV>();

		private static BDic<int, string> Osplitter_c = new BDic<int, string>();

		public static BDic<string, string> errorMsg;

		private static TxEvalListenerContainer GfEvalT;

		public static readonly Regex RegGF = new Regex("^GF?([BC])\\[ *(\\w+) *\\]");

		private enum TEMPLV : byte
		{
			NORMAL,
			CLEAR_ON_FLUSH,
			CLEAR_ON_MAPCHANGE
		}
	}
}
