using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XX
{
	public class StringHolder : IStringHolder
	{
		public int clength
		{
			get
			{
				return this.clength_;
			}
			set
			{
				this.clength_ = value;
			}
		}

		public StringHolder(Regex _RegSplitter)
		{
			this.Acmds = new string[8];
			this.RegItemSplitter = ((_RegSplitter != null) ? _RegSplitter : StringHolder.RegItemSplitterDefault);
		}

		public StringHolder(string str, Regex _RegSplitter = null)
			: this(_RegSplitter)
		{
			this.stringsInput(str);
		}

		public StringHolder(string[] _Acmds, Regex _RegSplitter = null)
			: this(_RegSplitter)
		{
			this.Acmds = _Acmds;
			this.clength = X.countNotEmpty<string>(this.Acmds);
		}

		public StringHolder(StringHolder StHol, Regex _RegSplitter = null)
			: this(_RegSplitter)
		{
			this.Acmds = StHol.slice(0, -1000);
			this.clength = StHol.clength;
		}

		public bool stringsInput(string last_str)
		{
			this.last_input = last_str;
			STB stb = TX.PopBld(last_str, 0);
			int num = 0;
			int num2;
			stb.csvAdjust(ref num, out num2, -1);
			last_str = stb.ToString(num, num2 - num);
			TX.ReleaseBld(stb);
			if (last_str.Length > 0)
			{
				this.ArrayInput(this.RegItemSplitter.Split(last_str), false);
				return true;
			}
			return false;
		}

		public bool stringsInput(STB Stb, int lcs, int lce)
		{
			try
			{
				if (lcs >= lce)
				{
					this.last_input = "";
					return false;
				}
				this.last_input = Stb.ToString(lcs, lce - lcs);
			}
			catch (Exception)
			{
				X.de("エラー", null);
			}
			if (this.last_input.Length > 0)
			{
				this.ArrayInput(this.RegItemSplitter.Split(this.last_input), false);
				return true;
			}
			return false;
		}

		public bool ArrayInput(string[] A, bool update_last_input = true)
		{
			if (A == null || A.Length == 0)
			{
				this.clength = 0;
				return false;
			}
			this.clength = A.Length;
			if (this.Acmds.Length < this.clength)
			{
				Array.Resize<string>(ref this.Acmds, this.clength);
			}
			for (int i = this.clength - 1; i >= 0; i--)
			{
				this.Acmds[i] = A[i];
			}
			for (int j = A.Length - 1; j >= this.clength; j--)
			{
				A[j] = null;
			}
			if (update_last_input)
			{
				this.last_input = this.slice_join(0, " ", "");
			}
			return this.clength > 0;
		}

		public string getRandom(int s = 1, int len = -1)
		{
			if (len < 0)
			{
				len = this.clength - s;
			}
			return this.getIndex((len <= 1) ? s : (s + X.xors(len)));
		}

		public string getIndex(int _i)
		{
			if (this.clength <= _i)
			{
				return "";
			}
			return this.Acmds[_i];
		}

		public string getIndex(int _i, string _default, bool default_if_empty = true)
		{
			if (this.clength <= _i)
			{
				return _default;
			}
			string text = this.Acmds[_i];
			if (text != null && (!(text == "") || !default_if_empty))
			{
				return text;
			}
			return _default;
		}

		public int IndexOf(string s)
		{
			for (int i = 0; i < this.clength; i++)
			{
				if (this.Acmds[i] == s)
				{
					return i;
				}
			}
			return -1;
		}

		public string cmd
		{
			get
			{
				if (this.clength <= 0)
				{
					return "";
				}
				return this.Acmds[0];
			}
		}

		public string _1
		{
			get
			{
				if (this.clength <= 1)
				{
					return "";
				}
				return this.Acmds[1];
			}
		}

		public string _2
		{
			get
			{
				if (this.clength <= 2)
				{
					return "";
				}
				return this.Acmds[2];
			}
		}

		public string _3
		{
			get
			{
				if (this.clength <= 3)
				{
					return "";
				}
				return this.Acmds[3];
			}
		}

		public string _4
		{
			get
			{
				if (this.clength <= 4)
				{
					return "";
				}
				return this.Acmds[4];
			}
		}

		public string _5
		{
			get
			{
				if (this.clength <= 5)
				{
					return "";
				}
				return this.Acmds[5];
			}
		}

		public string _6
		{
			get
			{
				if (this.clength <= 6)
				{
					return "";
				}
				return this.Acmds[6];
			}
		}

		public string _7
		{
			get
			{
				if (this.clength <= 7)
				{
					return "";
				}
				return this.Acmds[7];
			}
		}

		public float _N1
		{
			get
			{
				return X.Nm(this._1, 0f, false);
			}
		}

		public float _N2
		{
			get
			{
				return X.Nm(this._2, 0f, false);
			}
		}

		public float _N3
		{
			get
			{
				return X.Nm(this._3, 0f, false);
			}
		}

		public float _N4
		{
			get
			{
				return X.Nm(this._4, 0f, false);
			}
		}

		public float _N5
		{
			get
			{
				return X.Nm(this._5, 0f, false);
			}
		}

		public float _N6
		{
			get
			{
				return X.Nm(this._6, 0f, false);
			}
		}

		public float NE1()
		{
			return (float)TX.eval(this._1, "");
		}

		public float NE2()
		{
			return (float)TX.eval(this._2, "");
		}

		public float NE3()
		{
			return (float)TX.eval(this._3, "");
		}

		public float NE4()
		{
			return (float)TX.eval(this._4, "");
		}

		public float NE5()
		{
			return (float)TX.eval(this._5, "");
		}

		public float NE6()
		{
			return (float)TX.eval(this._6, "");
		}

		public bool _B1
		{
			get
			{
				if (this._1 == "" || this._1 == "0")
				{
					return false;
				}
				string _1U = this._1U;
				return !(_1U == "FALSE") && !(_1U == "NO");
			}
		}

		public bool _B2
		{
			get
			{
				if (this._2 == "" || this._2 == "0")
				{
					return false;
				}
				string _2U = this._2U;
				return !(_2U == "FALSE") && !(_2U == "NO");
			}
		}

		public bool _B3
		{
			get
			{
				if (this._3 == "" || this._3 == "0")
				{
					return false;
				}
				string _3U = this._3U;
				return !(_3U == "FALSE") && !(_3U == "NO");
			}
		}

		public bool _B4
		{
			get
			{
				if (this._4 == "" || this._4 == "0")
				{
					return false;
				}
				string _4U = this._4U;
				return !(_4U == "FALSE") && !(_4U == "NO");
			}
		}

		public bool _B5
		{
			get
			{
				if (this._5 == "" || this._5 == "0")
				{
					return false;
				}
				string _5U = this._5U;
				return !(_5U == "FALSE") && !(_5U == "NO");
			}
		}

		public bool _B6
		{
			get
			{
				if (this._6 == "" || this._6 == "0")
				{
					return false;
				}
				string _6U = this._6U;
				return !(_6U == "FALSE") && !(_6U == "NO");
			}
		}

		public string _1U
		{
			get
			{
				return this._1.ToUpper();
			}
		}

		public string _2U
		{
			get
			{
				return this._2.ToUpper();
			}
		}

		public string _3U
		{
			get
			{
				return this._3.ToUpper();
			}
		}

		public string _4U
		{
			get
			{
				return this._4.ToUpper();
			}
		}

		public string _5U
		{
			get
			{
				return this._5.ToUpper();
			}
		}

		public string _6U
		{
			get
			{
				return this._6.ToUpper();
			}
		}

		public string _1L
		{
			get
			{
				return this._1.ToLower();
			}
		}

		public string _2L
		{
			get
			{
				return this._2.ToLower();
			}
		}

		public string _3L
		{
			get
			{
				return this._3.ToLower();
			}
		}

		public string _4L
		{
			get
			{
				return this._4.ToLower();
			}
		}

		public string _5L
		{
			get
			{
				return this._5.ToLower();
			}
		}

		public string _6L
		{
			get
			{
				return this._6.ToLower();
			}
		}

		public float Nm(int i, float defv = 0f)
		{
			if (this.clength <= i)
			{
				return defv;
			}
			return X.Nm(this.Acmds[i], defv, true);
		}

		public float NmE(int i, float defv = 0f)
		{
			if (this.clength <= i || !(this.Acmds[i] != ""))
			{
				return defv;
			}
			return (float)TX.eval(this.Acmds[i], "");
		}

		public int Int(int i, int defv = 0)
		{
			if (this.clength <= i)
			{
				return defv;
			}
			return X.NmI(this.Acmds[i], defv, true, false);
		}

		public int IntE(int i, int defv = 0)
		{
			if (this.clength <= i || !(this.Acmds[i] != ""))
			{
				return defv;
			}
			return (int)TX.eval(this.Acmds[i], "");
		}

		public int IntAim(int i, int defv = -1)
		{
			if (this.clength <= i)
			{
				return defv;
			}
			return CAim.parseString(this.Acmds[i], -1);
		}

		public bool getB(int i, bool defv = false)
		{
			if (this.clength <= i)
			{
				return defv;
			}
			return X.Nm(this.Acmds[i], (float)(defv ? 1 : 0), true) != 0f;
		}

		public uint get_color(int i)
		{
			string index = this.getIndex(i);
			if (index.Length >= 6)
			{
				return TX.str2color(index, uint.MaxValue);
			}
			return C32.colToCode(this.Nm(i, 0f), this.Nm(i + 1, 0f), this.Nm(i + 2, 0f));
		}

		public void shiftEmpty(int count, int index = 0)
		{
			count = X.Mn(this.clength - index, count);
			if (count > 0)
			{
				X.shiftEmpty<string>(this.Acmds, count, index, this.clength);
				this.clength -= count;
			}
		}

		public bool isStart(string key, int i = 0)
		{
			if (this.Acmds == null)
			{
				return false;
			}
			char[] array = this.Acmds[i].ToCharArray();
			int length = key.Length;
			if (array.Length < length)
			{
				return false;
			}
			char[] array2 = key.ToCharArray();
			for (i = 0; i < length; i++)
			{
				if (array2[i] != array[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool has(string k)
		{
			k = k.ToUpper();
			int clength = this.clength;
			for (int i = 0; i < clength; i++)
			{
				if (this.Acmds[i].ToUpper() == k)
				{
					return true;
				}
			}
			return false;
		}

		public List<string> copyDataTo(List<string> Adep, int _from = 1)
		{
			int clength = this.clength;
			for (int i = _from; i < clength; i++)
			{
				Adep.Add(this.Acmds[i]);
			}
			return Adep;
		}

		public string[] slice(int i = 0, int j = -1000)
		{
			if (i >= this.clength)
			{
				return null;
			}
			if (j == -1000)
			{
				j = this.clength;
			}
			return X.slice<string>(this.Acmds, i, (j < 0) ? (this.clength + j) : j);
		}

		public string slice_join(int i = 1, string k = " ", string cover_quote = "")
		{
			int clength = this.clength;
			string text = "";
			while (i < clength)
			{
				if (cover_quote.Length > 0)
				{
					text = string.Concat(new string[]
					{
						text,
						(text.Length > 0) ? k : "",
						cover_quote,
						this.Acmds[i],
						cover_quote
					});
				}
				else if (this.Acmds[i] != "")
				{
					text = text + ((text.Length > 0) ? k : "") + this.Acmds[i];
				}
				i++;
			}
			return text;
		}

		public void slice_join(STB Stb, int i = 1, string k = " ", string cover_quote = "")
		{
			int clength = this.clength;
			bool flag = true;
			while (i < clength)
			{
				Stb.Add(flag ? "" : k);
				if (cover_quote.Length > 0)
				{
					Stb.Add(cover_quote, this.Acmds[i], cover_quote);
					flag = false;
				}
				else if (this.Acmds[i] != "")
				{
					Stb.Add(this.Acmds[i]);
					flag = false;
				}
				i++;
			}
		}

		public float slice_eval(int i = 1, string k = " ")
		{
			return (float)TX.eval(this.slice_join(i, k, ""), "");
		}

		public string[] slice_unescape(int i = 0, int j = -1000)
		{
			string[] array = X.slice<string>(this.Acmds, i, (j == -1000) ? this.clength : j);
			int num = array.Length;
			for (i = 0; i < num; i++)
			{
				array[i] = TX.unescape(array[i]);
			}
			return array;
		}

		public string[] sliceIfExists(int slice_from = 1, string default_str = "", string prefix = "", string suffix = "")
		{
			string[] array;
			if (this.clength > slice_from)
			{
				array = this.slice(slice_from, -1000);
			}
			else if (default_str.Length > 0)
			{
				array = new string[] { default_str };
			}
			else
			{
				array = new string[0];
			}
			if (prefix.Length > 0 || suffix.Length > 0)
			{
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					array[i] = prefix + array[i] + suffix;
				}
			}
			return array;
		}

		public virtual bool tNote(string t, bool is_error = false)
		{
			if (t == null)
			{
				return true;
			}
			X.dl(t, null, is_error, false);
			X.dl(" - cmd: " + this.cmd, null, is_error, false);
			return true;
		}

		public bool tNote_false(string t, bool is_error = false)
		{
			this.tNote(t, is_error);
			return false;
		}

		public bool tError(string t)
		{
			return this.tNote(t, true);
		}

		public bool tError_false(string t)
		{
			this.tNote(t, true);
			return false;
		}

		protected string[] Acmds;

		public string last_input;

		public static Regex RegItemSplitterDefault = new Regex("[ \\s\\t]*\\,[ \\s\\t]*");

		protected Regex RegItemSplitter;

		public int clength_;
	}
}
