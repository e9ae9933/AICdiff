using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX
{
	public class BgmKind
	{
		public static void readBgmScript()
		{
			CsvReader csvReader = new CsvReader(Resources.Load<TextAsset>("Basic/Data/_bgm").text, CsvReader.RegSpace, false);
			BgmKind.OKind = new BDic<string, BgmKind>();
			BgmKind bgmKind = null;
			while (csvReader.read())
			{
				if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					bgmKind = X.Get<string, BgmKind>(BgmKind.OKind, index);
					if (bgmKind != null)
					{
						X.de("BgmKind 重複: " + index.ToString(), null);
						bgmKind = null;
					}
					else
					{
						bgmKind = (BgmKind.OKind[index] = new BgmKind(index));
					}
				}
				if (bgmKind != null)
				{
					string cmd = csvReader.cmd;
					if (cmd != null)
					{
						uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
						if (num <= 1575106246U)
						{
							if (num != 938626731U)
							{
								if (num != 1333443158U)
								{
									if (num == 1575106246U)
									{
										if (cmd == "bpm")
										{
											bgmKind.bpm = csvReader.Nm(1, 0f);
										}
									}
								}
								else if (cmd == "author")
								{
									bgmKind.author = csvReader._1;
								}
							}
							else if (cmd == "battle_transition")
							{
								if (bgmKind.battle_que == null)
								{
									X.de("CK.battle_queを設定したあとに実行すること", null);
								}
								bgmKind.addTransitionPoint(bgmKind.battle_que, TX.split(csvReader._1, "|"), TX.split(csvReader._2, "|"), csvReader.Int(3, 2000), csvReader.Int(4, 2000));
							}
						}
						else if (num <= 2342698647U)
						{
							if (num != 1918847885U)
							{
								if (num == 2342698647U)
								{
									if (cmd == "block_override")
									{
										if (bgmKind.ABlkOvr == null)
										{
											bgmKind.ABlkOvr = new List<BgmBlockOverride>(2);
											if (csvReader._1 != "_")
											{
												X.dl("block_override の初手は _ キー推奨。", null, false, false);
											}
										}
										BgmBlockOverride bgmBlockOverride = new BgmBlockOverride(csvReader._1, X.IntC((float)(csvReader.clength - 2) / 2f));
										bgmKind.ABlkOvr.Add(bgmBlockOverride);
										for (int i = 2; i < csvReader.clength; i += 2)
										{
											char c = csvReader.getIndex(i)[0];
											char c2 = ((i + 1 >= csvReader.clength) ? (c + '\u0001') : csvReader.getIndex(i + 1)[0]);
											bgmBlockOverride.Add(c, c2);
										}
									}
								}
							}
							else if (cmd == "battle_que")
							{
								bgmKind.battle_que = csvReader._1;
							}
						}
						else if (num != 3815642588U)
						{
							if (num == 3869500228U)
							{
								if (cmd == "default_que")
								{
									bgmKind.default_que = csvReader._1;
								}
							}
						}
						else if (cmd == "default_transition")
						{
							if (bgmKind.default_que == null)
							{
								X.de("CK.default_queを設定したあとに実行すること", null);
							}
							bgmKind.addTransitionPoint(bgmKind.default_que, TX.split(csvReader._1, "|"), TX.split(csvReader._2, "|"), csvReader.Int(3, 2000), csvReader.Int(4, 2000));
						}
					}
				}
			}
		}

		public static BgmKind GetFromSheet(string key)
		{
			return X.Get<string, BgmKind>(BgmKind.OKind, key);
		}

		public BgmKind(string _key)
		{
			this.key = _key;
			this.default_que = this.key;
		}

		public BgmTransitionPoint addTransitionPoint(string k, string[] _AFrom, string[] _ATo, int _fade_millisec_a, int _fade_millisec_b)
		{
			if (this.OTrans == null)
			{
				this.OTrans = new BDic<string, BgmTransitionPoint>();
			}
			BgmTransitionPoint bgmTransitionPoint = X.Get<string, BgmTransitionPoint>(this.OTrans, k);
			if (bgmTransitionPoint == null)
			{
				bgmTransitionPoint = (this.OTrans[k] = new BgmTransitionPoint());
			}
			int num = bgmTransitionPoint.Set(_AFrom, _ATo, _fade_millisec_a, _fade_millisec_b);
			this.block_max = X.Mx(this.block_max, num);
			return bgmTransitionPoint;
		}

		public const string data_csv = "Basic/Data/_bgm";

		private static BDic<string, BgmKind> OKind;

		public List<BgmBlockOverride> ABlkOvr;

		public readonly string key;

		public string author;

		public float bpm = 120f;

		public int shift;

		public string default_que;

		public string battle_que;

		public int block_max;

		public BDic<string, BgmTransitionPoint> OTrans;

		public enum QUE_KIND
		{
			NORMAL,
			BATTLE
		}
	}
}
