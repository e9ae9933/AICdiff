using System;
using System.Text.RegularExpressions;

namespace XX
{
	public class CsvReader : StringHolder
	{
		protected string last_str
		{
			get
			{
				if (this.last_str_ == null)
				{
					this.last_str_ = base.slice_join(0, " ", "");
				}
				return this.last_str_;
			}
			set
			{
				this.last_str_ = value;
			}
		}

		public CsvReader(string basetext, Regex _RegSplitter, bool use_variable = false)
			: base(_RegSplitter)
		{
			this.VarCon = (use_variable ? CsvVariableContainer.getDefault(false) : null);
			if (basetext != null)
			{
				this.parseText(basetext);
			}
		}

		public CsvReader(string basetext, Regex _RegSplitter, CsvVariableContainer _VarCon)
			: base(_RegSplitter)
		{
			this.VarCon = _VarCon;
			if (basetext != null)
			{
				this.parseText(basetext);
			}
		}

		public virtual CsvReader parseText(string basetext)
		{
			this.Adata = CsvReader.RegReturn.Split(basetext);
			this.cur_line = 0;
			this.last_str = "";
			return this;
		}

		public virtual CsvReader parseText(STB Stb)
		{
			this.Adata = Stb.splitReturn();
			this.cur_line = 0;
			this.last_str = "";
			return this;
		}

		public CsvReader CopyDataFrom(CsvReader Src)
		{
			this.VarCon = Src.VarCon;
			this.RegItemSplitter = Src.RegItemSplitter;
			this.Adata = Src.Adata;
			this.cur_line = Src.cur_line;
			this.last_str = Src.last_str;
			this.tilde_replace = Src.tilde_replace;
			this.no_replace_quote = Src.no_replace_quote;
			this.no_write_varcon = Src.no_write_varcon;
			return this;
		}

		public STB copyDataTo(STB Stb, int line_s = 0, int line_e = -1)
		{
			if (this.Adata == null)
			{
				return Stb;
			}
			if (line_e < 0)
			{
				line_e = this.Adata.Length;
			}
			if (line_s >= line_e)
			{
				return Stb;
			}
			Stb.Add(this.Adata[line_s]);
			for (int i = line_s + 1; i < line_e; i++)
			{
				Stb.Add("\n", this.Adata[i]);
			}
			return Stb;
		}

		public int countCharacters(int line_s = 0, int line_e = -1)
		{
			if (this.Adata == null)
			{
				return 0;
			}
			if (line_e < 0)
			{
				line_e = this.Adata.Length;
			}
			if (line_s >= line_e)
			{
				return 0;
			}
			int num = this.Adata[line_s].Length;
			for (int i = line_s + 1; i < line_e; i++)
			{
				num += this.Adata[i].Length + 1;
			}
			return num;
		}

		public virtual CsvReader parseText(string[] basetext)
		{
			if (this.Adata == null)
			{
				this.Adata = X.concat<string>(basetext, null, -1, -1);
			}
			else
			{
				int num = this.Adata.Length;
				int num2 = basetext.Length;
				if (this.Adata.Length < num2)
				{
					Array.Resize<string>(ref this.Adata, basetext.Length);
				}
				for (int i = 0; i < num2; i++)
				{
					this.Adata[i] = basetext[i];
				}
				for (int j = num2; j < num; j++)
				{
					this.Adata[j] = "";
				}
			}
			this.cur_line = 0;
			this.last_str = "";
			return this;
		}

		public string getBaseScript()
		{
			return string.Join("\n", this.Adata);
		}

		public virtual bool read()
		{
			bool flag;
			using (STB stb = TX.PopBld(null, 0))
			{
				while (this.Adata.Length > this.cur_line)
				{
					this.last_str = this.Adata[this.cur_line];
					this.cur_line++;
					if (this.last_str == null)
					{
						return false;
					}
					stb.Set(this.last_str);
					int num = 0;
					int length = this.last_str.Length;
					stb.csvAdjust(ref num, out length, -1);
					if (this.readInner(stb, num, length))
					{
						return true;
					}
				}
				base.clength = 0;
				flag = false;
			}
			return flag;
		}

		public bool readInner(STB Stb, int lcs, int lce)
		{
			if (this.VarCon != null && this.VarCon.read(Stb, lcs, ref lce, this.no_write_varcon == 0, this.no_replace_quote) && this.no_write_varcon != 2)
			{
				return false;
			}
			if (!base.stringsInput(Stb, lcs, lce))
			{
				return false;
			}
			this.last_str = this.last_input;
			if (this.readAfterForVarCon(this.tilde_replace))
			{
				this.last_str_ = null;
			}
			if (this.skip_debug_cmd && base.cmd == "DEBUG")
			{
				if (base.clength > 0)
				{
					this.tNote("DEBUG:: " + base.slice_join(1, " ", ""), false);
				}
				return false;
			}
			return true;
		}

		public bool CopyWithReplacingVar(STB Stb, int lcs, int lce, STB StbCopyTo, string after_delimiter = " ")
		{
			bool flag = false;
			if (this.VarCon != null)
			{
				if (this.VarCon.read(Stb, lcs, ref lce, out flag, this.no_write_varcon == 0, this.no_replace_quote))
				{
					if (this.no_write_varcon != 2)
					{
						return false;
					}
				}
				else if (this.no_replace_quote)
				{
					flag = Stb.IndexOf('$', 0, -1) >= 0;
				}
			}
			if (lcs >= lce)
			{
				this.last_input = (this.last_str = "");
				return false;
			}
			if (!flag)
			{
				StbCopyTo.Add(Stb, lcs, lce - lcs);
				return true;
			}
			if (base.stringsInput(Stb, lcs, lce))
			{
				this.last_str = this.last_input;
				if (this.readAfterForVarCon(this.tilde_replace))
				{
					this.last_str_ = null;
					for (int i = 0; i < base.clength; i++)
					{
						if (i > 0)
						{
							StbCopyTo.Add(after_delimiter);
						}
						StbCopyTo.Add(this.Acmds[i]);
					}
				}
				else
				{
					StbCopyTo.Add(this.last_input);
				}
				return true;
			}
			return false;
		}

		public bool readAfterForVarCon(bool tilde_replace)
		{
			return this.VarCon != null && this.VarCon.read_after(this.Acmds, base.clength, tilde_replace);
		}

		public bool readCorrectly()
		{
			if (this.Adata.Length <= this.cur_line)
			{
				return false;
			}
			this.last_str = TX.trim(this.Adata[this.cur_line]);
			this.cur_line++;
			base.clength = 0;
			return true;
		}

		public bool readCorrectly(STB Stb, out int lcs, out int lce, bool comment_clip = true)
		{
			lcs = (lce = 0);
			if (this.Adata.Length <= this.cur_line)
			{
				return false;
			}
			this.last_str = this.Adata[this.cur_line];
			Stb.Set(this.last_str);
			if (comment_clip)
			{
				Stb.csvAdjust(ref lcs, out lce, -1);
			}
			else
			{
				Stb.TrimSpace(lcs, out lcs, out lce, -1);
			}
			this.cur_line++;
			base.clength = 0;
			return true;
		}

		public void copyLastStrToOneCmd()
		{
			this.Acmds[0] = this.last_str;
			base.clength = 1;
		}

		public void seek_set(int i = 0)
		{
			this.cur_line = i;
		}

		public bool isEnd()
		{
			return this.Adata == null || this.Adata.Length <= this.cur_line;
		}

		public int get_cur_line()
		{
			return this.cur_line - 1;
		}

		public string getLastStr()
		{
			return this.last_str;
		}

		public int getLength()
		{
			return this.Adata.Length;
		}

		public string[] getBaseData()
		{
			return this.Adata;
		}

		public bool isVariable()
		{
			return this.Adata != null;
		}

		public CsvReader rewrite_last_str(string _str)
		{
			this.last_str = _str;
			return this;
		}

		public bool de(string t)
		{
			this.tNote(t, true);
			return false;
		}

		public override bool tNote(string t, bool is_error = false)
		{
			if (t == null)
			{
				return true;
			}
			X.dl(t, null, is_error, false);
			X.dl(" - string: " + this.last_str + ", line: " + this.cur_line.ToString(), null, is_error, false);
			return true;
		}

		public virtual void destruct()
		{
			this.VarCon = null;
		}

		public static Regex RegSpace = new Regex("[ \\s\\t\\,]+");

		public static Regex RegOnlySpace = new Regex("[ \\s\\t]+");

		public static Regex RegComma = new Regex("[ \\s\\t]*\\,[ \\s\\t]*");

		public static readonly Regex RegReturn = new Regex("[\\n\\r]");

		protected string[] Adata;

		private string last_str_;

		protected int cur_line;

		public bool no_replace_quote;

		public int no_write_varcon;

		public bool tilde_replace;

		public bool skip_debug_cmd = true;

		public CsvVariableContainer VarCon;
	}
}
