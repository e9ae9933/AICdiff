using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XX
{
	public class CsvVariableContainer
	{
		public static CsvVariableContainer getDefault(bool reset = false)
		{
			if (reset || CsvVariableContainer.DefaultVarCon == null)
			{
				CsvVariableContainer.DefaultVarCon = new CsvVariableContainer();
			}
			return CsvVariableContainer.DefaultVarCon;
		}

		public CsvVariableContainer()
		{
			this.Avi = new CsvVariableItem[16];
		}

		public CsvVariableContainer(CsvVariableItem[] _Avi, int ARMX = -1)
		{
			this.Avi = new CsvVariableItem[16];
			if (_Avi != null)
			{
				if (ARMX < 0)
				{
					ARMX = _Avi.Length;
				}
				for (int i = 0; i < ARMX; i++)
				{
					this.Push(_Avi[i]);
				}
			}
		}

		public CsvVariableContainer(CsvVariableContainer Src)
		{
			if (Src == null)
			{
				this.Avi = new CsvVariableItem[16];
				return;
			}
			this.Avi = new CsvVariableItem[Src.vi_cnt];
			this.CopyFrom(Src);
		}

		public CsvVariableContainer CopyFrom(CsvVariableContainer Src)
		{
			if (Src.vi_cnt + this.vi_cnt > this.Avi.Length)
			{
				Array.Resize<CsvVariableItem>(ref this.Avi, Src.vi_cnt + this.vi_cnt);
			}
			int num = Src.vi_cnt;
			for (int i = 0; i < num; i++)
			{
				this.Push(Src.Avi[i]);
			}
			return this;
		}

		public void Push(CsvVariableItem vi)
		{
			if (this.vi_cnt >= this.Avi.Length)
			{
				Array.Resize<CsvVariableItem>(ref this.Avi, X.Mx(16, this.vi_cnt + 16));
			}
			CsvVariableItem[] avi = this.Avi;
			int num = this.vi_cnt;
			this.vi_cnt = num + 1;
			avi[num].Set(vi);
		}

		public void Push(string _name, string _val)
		{
			if (this.vi_cnt >= this.Avi.Length)
			{
				Array.Resize<CsvVariableItem>(ref this.Avi, X.Mx(16, this.vi_cnt + 16));
			}
			CsvVariableItem[] avi = this.Avi;
			int num = this.vi_cnt;
			this.vi_cnt = num + 1;
			avi[num].Set(_name, _val);
		}

		public CsvVariableContainer clone()
		{
			return new CsvVariableContainer(this.Avi, this.vi_cnt);
		}

		public bool read(CsvReader CR, string last_str, bool writeable = true, bool no_replace_quote = false)
		{
			bool flag2;
			using (STB stb = TX.PopBld(last_str, 0))
			{
				int num = 0;
				int num2;
				stb.csvAdjust(ref num, out num2, -1);
				bool flag = this.read(stb, num, ref num2, writeable, no_replace_quote);
				if (!flag && CR != null)
				{
					CR.rewrite_last_str(stb.ToString(num, num2 - num));
				}
				flag2 = flag;
			}
			return flag2;
		}

		public bool read(STB Stb, int lcs, ref int lce, bool writeable = true, bool no_replace_quote = false)
		{
			bool flag;
			return this.read(Stb, lcs, ref lce, out flag, writeable, no_replace_quote);
		}

		public bool read(STB Stb, int lcs, ref int lce, out bool quote_or_doller_exists, bool writeable = true, bool no_replace_quote = false)
		{
			quote_or_doller_exists = false;
			if (this.AquoteReplace != null)
			{
				this.AquoteReplace.Clear();
			}
			if (lcs >= lce)
			{
				return false;
			}
			bool flag = false;
			int num = lcs;
			bool flag2 = false;
			if (Stb.isStart("DEFINE", lcs))
			{
				lcs += 6;
				if (Stb.isStart("NDEF", lcs))
				{
					flag = true;
					lcs += 4;
				}
				if (lcs >= lce || !TX.isSpaceOrComma(Stb[lcs]))
				{
					flag2 = true;
				}
				else
				{
					Stb.Scroll(lcs, out lcs, (char __c) => TX.isSpaceOrComma(__c), lce);
				}
			}
			if (!flag2)
			{
				if (Stb.isStart("*!!", lcs))
				{
					if (writeable)
					{
						this.removeVarAll();
					}
					return true;
				}
				int num2;
				Stb.Scroll(lcs, out num2, (char __c) => TX.isWMatch(__c), lce);
				if (num2 < lce && lcs < num2)
				{
					if (Stb[num2] == '!')
					{
						if (writeable)
						{
							this.removeVar(Stb, lcs, num2, -1);
						}
						return true;
					}
					if (Stb[num2] == '=')
					{
						if (writeable)
						{
							if (flag && this.isin(Stb, lcs, num2) >= 0)
							{
								return true;
							}
							string text = Stb.ToString(lcs, num2 - lcs);
							Stb.Scroll(num2 + 1, out num2, (char __c) => TX.isSpace(__c), lce);
							this.defineOnReading(text, Stb, num2, lce, false);
						}
						return true;
					}
				}
			}
			if (!no_replace_quote)
			{
				int i = num;
				char c = '\0';
				int num3 = 0;
				while (i < lce)
				{
					char c2 = Stb[i++];
					if (c2 == '\\')
					{
						i++;
					}
					else
					{
						if (c2 == '$')
						{
							quote_or_doller_exists = true;
						}
						if (c == '\0')
						{
							if (c2 == '\'' || c2 == '"')
							{
								c = c2;
								quote_or_doller_exists = true;
								num3 = i - 1;
							}
						}
						else if (c2 == c)
						{
							if (this.AquoteReplace == null)
							{
								this.AquoteReplace = new List<string>(1);
							}
							int num4 = i - num3;
							this.AquoteReplace.Add(Stb.ToString(num3 + 1, num4 - 2));
							Stb.Splice(num3, num4);
							string text2 = CsvVariableContainer.quote_replacer_key(this.AquoteReplace.Count - 1);
							Stb.Insert(num3, text2);
							i = num3 + text2.Length;
							lce += -num4 + text2.Length;
							c = '\0';
						}
					}
				}
			}
			return false;
		}

		private void defineOnReading(string _dkey, STB StbSrc, int lcs, int lce, bool eval_mode = false)
		{
			if (lcs >= lce)
			{
				this.define(_dkey, "", false);
				return;
			}
			if (StbSrc.Is('~', lcs))
			{
				lcs++;
				eval_mode = true;
			}
			if (eval_mode)
			{
				bool flag;
				this.replace_simple(StbSrc, lcs, ref lce, out flag, false, false);
				StbSrc.Set(TX.eval(StbSrc, lcs, lce));
				int length = StbSrc.Length;
				lcs = 0;
				lce = length;
			}
			this.define(_dkey, StbSrc, lcs, lce, !eval_mode, null);
		}

		public bool read_after(string[] Astr, int datalen, bool tilde_replace = false)
		{
			bool flag = false;
			using (STB stb = TX.PopBld(null, 0))
			{
				for (int i = 0; i < datalen; i++)
				{
					bool flag2 = tilde_replace && Astr[i].Length > 0 && TX.charIs(Astr[i], 0, '~');
					stb.Clear();
					if (flag2)
					{
						stb.Add(Astr[i], 1, Astr[i].Length - 1);
					}
					else
					{
						stb.Add(Astr[i]);
					}
					bool flag3;
					this.replace_simple(stb, out flag3, false, false);
					if (flag3 || flag2)
					{
						flag = true;
						if (flag2)
						{
							Astr[i] = TX.eval(stb, 0, stb.Length).ToString();
						}
						else
						{
							Astr[i] = stb.ToString();
						}
					}
				}
			}
			return this.ExecuteQuoteReplaceAfter(Astr, datalen) || flag;
		}

		private bool ExecuteQuoteReplaceAfter(string[] Astr, int datalen)
		{
			if (this.AquoteReplace == null)
			{
				return false;
			}
			int count = this.AquoteReplace.Count;
			if (count == 0)
			{
				return false;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					for (int i = 0; i < datalen; i++)
					{
						stb.Set(Astr[i]);
						bool flag = false;
						for (int j = 0; j < count; j++)
						{
							stb2.Set(this.AquoteReplace[j]);
							bool flag2;
							stb.Replace(CsvVariableContainer.quote_replacer_key(j), stb2, out flag2, 0, -1);
							flag = flag || flag2;
						}
						if (flag)
						{
							Astr[i] = stb.ToString();
						}
					}
				}
			}
			this.AquoteReplace.Clear();
			return true;
		}

		public STB replace_simple(STB StbSrc, bool val_replace_with_quote = false, bool val_replace_with_kakko = false)
		{
			bool flag;
			return this.replace_simple(StbSrc, 0, StbSrc.Length, out flag, val_replace_with_quote, val_replace_with_kakko);
		}

		public STB replace_simple(STB StbSrc, out bool changed, bool val_replace_with_quote = false, bool val_replace_with_kakko = false)
		{
			return this.replace_simple(StbSrc, 0, StbSrc.Length, out changed, val_replace_with_quote, val_replace_with_kakko);
		}

		public STB replace_simple(STB StbSrc, int lcs, int lce, bool val_replace_with_quote = false, bool val_replace_with_kakko = false)
		{
			bool flag;
			return this.replace_simple(StbSrc, lcs, lce, out flag, val_replace_with_quote, val_replace_with_kakko);
		}

		public STB replace_simple(STB StbSrc, int lcs, int lce, out bool changed, bool val_replace_with_quote = false, bool val_replace_with_kakko = false)
		{
			return this.replace_simple(StbSrc, lcs, ref lce, out changed, val_replace_with_quote, val_replace_with_kakko);
		}

		public STB replace_simple(STB StbSrc, int lcs, ref int lce, out bool changed, bool val_replace_with_quote = false, bool val_replace_with_kakko = false)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				this.replace_simple(StbSrc, stb, lcs, lce, out changed, val_replace_with_quote, val_replace_with_kakko);
				if (lce > lcs)
				{
					int num = lce - lcs;
					StbSrc.Splice(lcs, num);
					StbSrc.Insert(lcs, stb);
					lce += -num + stb.Length;
				}
			}
			return StbSrc;
		}

		public void replace_simple(STB StbSrc, STB StbDest, int lcs, int lce, out bool changed, bool val_replace_with_quote = false, bool val_replace_with_kakko = false)
		{
			bool flag = false;
			bool flag2 = false;
			int i = lcs;
			changed = false;
			int num = 0;
			while (i < lce)
			{
				char c = StbSrc[i];
				if (flag2)
				{
					flag2 = false;
					StbDest.Add(c);
					num++;
				}
				else if (c == '\\')
				{
					flag2 = true;
				}
				else if (c == '\'')
				{
					flag = !flag;
				}
				else
				{
					if (!flag && c == '$')
					{
						using (STB stb = TX.PopBld(null, 0))
						{
							int num2;
							int num3;
							StbSrc.ScrollNextVariable(i, out num2, out num3, stb, lce);
							if (stb.Length > 0)
							{
								changed = true;
								int num4 = this.isin(stb, 0, -1);
								if (num4 >= 0)
								{
									if (val_replace_with_kakko)
									{
										StbDest.Add("(", this.Avi[num4].val, ")");
									}
									else if (val_replace_with_quote)
									{
										StbDest.Add("'", this.Avi[num4].val, "'");
									}
									else
									{
										StbDest.Add(this.Avi[num4].val);
									}
								}
								else if (!this.no_undef_error)
								{
									X.de("Undef Var:" + stb.ToString(), null);
								}
								i = num3;
								continue;
							}
							StbDest.Add(c);
							num++;
							goto IL_016F;
						}
					}
					if (!flag && c == '/' && i < lce - 1 && StbSrc.charIs(i + 1, '/'))
					{
						break;
					}
					StbDest.Add(c);
					num++;
				}
				IL_016F:
				i++;
			}
			if (num < lce - lcs)
			{
				changed = true;
			}
		}

		public string define(string _name, string val, bool replace_val = true)
		{
			if (TX.noe(_name))
			{
				return val;
			}
			string text;
			using (STB stb = TX.PopBld(val, 0))
			{
				text = this.define(_name, stb, 0, stb.Length, replace_val, val);
			}
			return text;
		}

		public string define(string _name, STB Stb, int lcs, int lce, bool replace_val = true, string _val = null)
		{
			if (TX.noe(_name))
			{
				return _val;
			}
			int num = this.isin(_name);
			if (replace_val)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					bool flag;
					this.replace_simple(Stb, stb, lcs, lce, out flag, false, false);
					if (_val == null || !stb.Equals(_val))
					{
						_val = stb.ToString();
					}
					goto IL_006D;
				}
			}
			if (_val == null || !Stb.Equals(lcs, lce, _val, false))
			{
				_val = Stb.ToString(lcs, lce);
			}
			IL_006D:
			if (_name.Length > 0)
			{
				if (num >= 0)
				{
					this.Avi[num].val = _val;
				}
				else
				{
					this.Push(_name, _val);
				}
			}
			return _val;
		}

		public CsvVariableContainer defineArrayNumeric(string[] A)
		{
			this.removeArrayNumeric();
			if (A == null)
			{
				return this;
			}
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				this.define((i + 1).ToString(), A[i], true);
			}
			return this;
		}

		public CsvVariableContainer removeArrayNumeric()
		{
			int i = 0;
			int num = 0;
			while (i < this.vi_cnt)
			{
				CsvVariableItem csvVariableItem = ((num == 0) ? this.Avi[i] : (this.Avi[i - num] = this.Avi[i]));
				int num2;
				if (csvVariableItem.name == null || (int.TryParse(csvVariableItem.name, out num2) && num2 > 0))
				{
					num++;
				}
				i++;
			}
			if (num > 0)
			{
				this.vi_cnt -= num;
			}
			return this;
		}

		public string[] extractArrayNumeric()
		{
			string[] array = new string[0];
			int num = 0;
			for (;;)
			{
				int num2 = this.isin(num.ToString());
				if (num2 < 0)
				{
					break;
				}
				X.push<string>(ref array, this.Avi[num2].val, -1);
				num++;
			}
			return array;
		}

		public CsvVariableContainer removeVarAll()
		{
			this.vi_cnt = 0;
			return this;
		}

		public void removeVar(string _name, int _ind = -1)
		{
			_ind = ((_ind == -1) ? this.isin(_name) : _ind);
			if (_ind >= 0)
			{
				X.spliceStruct<CsvVariableItem>(this.Avi, _ind, 1);
				this.vi_cnt--;
			}
		}

		public void removeVar(STB Stb, int _sc, int _ec, int _ind = -1)
		{
			_ind = ((_ind == -1) ? this.isin(Stb, _sc, _ec) : _ind);
			if (_ind >= 0)
			{
				X.spliceStruct<CsvVariableItem>(this.Avi, _ind, 1);
				this.vi_cnt--;
			}
		}

		public void removeTemp()
		{
			int i = 0;
			int num = 0;
			while (i < this.vi_cnt)
			{
				CsvVariableItem csvVariableItem = ((num == 0) ? this.Avi[i] : (this.Avi[i - num] = this.Avi[i]));
				if (csvVariableItem.name == null || TX.charIs(csvVariableItem.name, 0, '_'))
				{
					num++;
				}
				i++;
			}
			if (num > 0)
			{
				this.vi_cnt -= num;
			}
		}

		public int isin(string _name)
		{
			for (int i = 0; i < this.vi_cnt; i++)
			{
				if (this.Avi[i].name == _name)
				{
					return i;
				}
			}
			return -1;
		}

		public int isin(STB StbName, int sc = 0, int ec = -1)
		{
			if (ec < 0)
			{
				ec = StbName.Length;
			}
			for (int i = 0; i < this.vi_cnt; i++)
			{
				if (StbName.Equals(sc, ec, this.Avi[i].name, false))
				{
					return i;
				}
			}
			return -1;
		}

		public bool isDefined(string _name)
		{
			for (int i = 0; i < this.vi_cnt; i++)
			{
				CsvVariableItem csvVariableItem = this.Avi[i];
				if (csvVariableItem.name == _name && csvVariableItem.val != null && csvVariableItem.val.Length > 0)
				{
					return true;
				}
			}
			return false;
		}

		public CsvVariableContainer show(Designer Ds)
		{
			Ds.Br();
			C32 cola = MTRX.cola;
			for (int i = 0; i < this.vi_cnt; i++)
			{
				CsvVariableItem csvVariableItem = this.Avi[i];
				Ds.add(new DsnDataP("", false)
				{
					text = csvVariableItem.name,
					TxCol = cola.Set((csvVariableItem.name.IndexOf("_") == 0) ? uint.MaxValue : 4294964118U).C,
					size = 12f,
					swidth = 110f,
					alignx = ALIGN.LEFT
				}).add(new DsnDataP("", false)
				{
					text = csvVariableItem.val,
					size = 12f,
					swidth = Ds.use_w - 10f,
					alignx = ALIGN.LEFT
				}).Br();
			}
			return this;
		}

		private static string quote_replacer_key(int i)
		{
			while (CsvVariableContainer.Aquote_replacer_key.Count <= i)
			{
				CsvVariableContainer.Aquote_replacer_key.Add("{{{/QUOTE_" + CsvVariableContainer.Aquote_replacer_key.Count.ToString() + "/}}}");
			}
			return CsvVariableContainer.Aquote_replacer_key[i];
		}

		public string Get(string n)
		{
			return this.Get(this.isin(n));
		}

		public string Get(int n)
		{
			if (n >= 0)
			{
				return this.Avi[n].val;
			}
			return null;
		}

		public static CsvVariableContainer DefaultVarCon;

		public static readonly Regex RegVarPut = new Regex("^[\\s\\t ]*([\\w]+)\\=");

		public static readonly Regex RegVarRem = new Regex("^[\\s\\t ]*([\\w]+)\\!");

		public static readonly Regex RegVarRemAll = new Regex("^[\\s\\t ]*\\*\\!\\!");

		public static readonly Regex RegQuote = new Regex("(?:'([^']*?)'|\"([^\"]*?)\")");

		public static readonly Regex RegDefault = new Regex("(?:\\$\\w+|\\$\\{ *\\w+ *\\})/");

		public static readonly Regex RegDefault_s0 = new Regex("^\\$(\\w+)");

		public static readonly Regex RegDefault_s1 = new Regex("^\\$\\{ *(\\w+) *\\}");

		public static readonly Regex RegFnRand = new Regex("rand *\\( *\\d+ *\\)");

		public static readonly Regex RegDefine = new Regex("^[\\s\\t ]*DEFINE(NDEF)?[\\s\\t \\,]+([\\w]+)\\=");

		private CsvVariableItem[] Avi;

		private int vi_cnt;

		private List<string> AquoteReplace;

		private static List<string> Aquote_replacer_key = new List<string>(8);

		private static readonly int NAME_W = 150;

		private static readonly int FLD_W = 700;

		private static readonly int LINE_H = 20;

		private static readonly int FONT_SIZE = 14;

		public bool no_undef_error;
	}
}
