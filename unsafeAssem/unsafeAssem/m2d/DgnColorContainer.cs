using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class DgnColorContainer
	{
		public DgnColorContainer()
		{
			this.reload();
		}

		public bool SetFamily(string _family, bool no_error = false)
		{
			DgnColorContainer.ColObj colObj = X.Get<string, DgnColorContainer.ColObj>(this.OFml, _family);
			if (colObj != null)
			{
				this.cur_family = _family;
				this.CurFam = colObj;
				return true;
			}
			if (!no_error)
			{
				X.de("ColContainer::SetFamily 不明なファミリー " + _family, null);
			}
			return false;
		}

		public void reload()
		{
			this.OFml = new BDic<string, DgnColorContainer.ColObj>();
			this.OAlias = new BDic<string, string>();
			DgnColorContainer.ColObj colObj = new DgnColorContainer.ColObj();
			string text = this.cur_family;
			string text2 = "_";
			this.OFml[text2] = (this.DefFam = colObj);
			CsvReader csvReader = new CsvReader(NKT.readStreamingText("m2d/__m2d_color.dat", false), CsvReader.RegSpace, true);
			List<Color32> list = new List<Color32>(3);
			while (csvReader.read())
			{
				if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					text2 = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					colObj = X.Get<string, DgnColorContainer.ColObj>(this.OFml, text2);
					if (colObj == null)
					{
						colObj = (this.OFml[text2] = new DgnColorContainer.ColObj());
					}
				}
				else if (TX.isStart(csvReader.cmd, "%", 0))
				{
					string cmd = csvReader.cmd;
					if (cmd != null)
					{
						if (!(cmd == "%CLONE"))
						{
							if (cmd == "%ALIAS")
							{
								string index = csvReader.getIndex(csvReader.clength - 1);
								for (int i = 1; i <= csvReader.clength - 2; i++)
								{
									this.OAlias[csvReader.getIndex(i)] = index;
								}
							}
						}
						else if (!this.OFml.ContainsKey(csvReader._1))
						{
							X.de("ColContainer::reload 不明なファミリー " + csvReader._1, null);
						}
						else
						{
							colObj.CopyFrom(this.OFml[csvReader._1]);
						}
					}
				}
				else
				{
					string cmd2 = csvReader.cmd;
					if (csvReader.clength == 1)
					{
						X.de("ColContainer::reload キー " + cmd2 + " が空 ", null);
					}
					else
					{
						list.Clear();
						for (int j = 1; j < csvReader.clength; j++)
						{
							list.Add(C32.d2c(TX.str2color(csvReader.getIndex(j), uint.MaxValue)));
						}
						colObj.Add(cmd2, list.ToArray());
					}
				}
			}
			if ((!TX.valid(text) || !this.SetFamily(text, false)) && !this.SetFamily(text2, false))
			{
				this.SetFamily("_", false);
			}
		}

		public Color32 GC(string key, int index = 0, uint def = 4294967295U)
		{
			if (this.CurFam.Valid(key, this.OAlias))
			{
				return this.CurFam.GC(index);
			}
			if (this.CurFam != this.DefFam && this.DefFam.Valid(key, this.OAlias))
			{
				return this.DefFam.GC(index);
			}
			X.de("ColContainer::GC ファミリー " + this.cur_family + " で不明なキー " + key, null);
			return C32.d2c(def);
		}

		public Color32 GC(int index, uint def = 4294967295U)
		{
			return this.GC(null, index, def);
		}

		public Color32 blend3(string key, float level, uint def = 4294967295U)
		{
			if (this.CurFam.Valid(key, this.OAlias))
			{
				return this.CurFam.blend3(this.C, level);
			}
			if (this.CurFam != this.DefFam && this.DefFam.Valid(key, this.OAlias))
			{
				return this.DefFam.blend3(this.C, level);
			}
			return C32.d2c(def);
		}

		public Color32 blend3r(string key, string key_rain, float nightlevel, float rainlevel, uint def = 4294967295U)
		{
			Color32 color = this.blend3(key, nightlevel, def);
			Color32 color2 = this.blend3(key_rain, nightlevel, def);
			return this.C.Set(color).blend(color2, rainlevel).C;
		}

		public Color32 blend(string key, float level, uint def = 4294967295U)
		{
			if (this.CurFam.Valid(key, this.OAlias))
			{
				return this.CurFam.blend(this.C, level);
			}
			if (this.CurFam != this.DefFam && this.DefFam.Valid(key, this.OAlias))
			{
				return this.DefFam.blend(this.C, level);
			}
			return C32.d2c(def);
		}

		private BDic<string, DgnColorContainer.ColObj> OFml;

		private string cur_family;

		private DgnColorContainer.ColObj DefFam;

		private DgnColorContainer.ColObj CurFam;

		private BDic<string, string> OAlias;

		private C32 C = new C32();

		private sealed class ColObj
		{
			public bool Has(string s)
			{
				return this.OCol.ContainsKey(s);
			}

			public void Add(string s, Color32[] A)
			{
				this.OCol[s] = A;
			}

			public void CopyFrom(DgnColorContainer.ColObj Src)
			{
				foreach (KeyValuePair<string, Color32[]> keyValuePair in Src.OCol)
				{
					this.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}

			public bool Valid(string _key, BDic<string, string> OAlias)
			{
				if (_key != null && this.cur_key != _key)
				{
					this.cur_key = _key;
					this.ACurCol = X.Get<string, Color32[]>(this.OCol, this.cur_key);
					if (this.ACurCol == null)
					{
						int num = 20;
						do
						{
							this.cur_key = X.Get<string, string>(OAlias, this.cur_key);
							if (this.cur_key == null)
							{
								break;
							}
							this.ACurCol = X.Get<string, Color32[]>(this.OCol, this.cur_key);
						}
						while (this.ACurCol == null && --num > 0);
						this.OCol[_key] = this.ACurCol;
					}
				}
				return this.ACurCol != null;
			}

			public Color32 GC(int index)
			{
				return this.ACurCol[index % this.ACurCol.Length];
			}

			public Color32 blend3(C32 C, float l)
			{
				return C.blend3(this.GC(0), this.GC(1), this.GC(2), l).C;
			}

			public Color32 blend(C32 C, float l)
			{
				return C.Set(this.GC(0)).blend(this.GC(1), l).C;
			}

			private BDic<string, Color32[]> OCol = new BDic<string, Color32[]>();

			private string cur_key;

			private Color32[] ACurCol;
		}
	}
}
