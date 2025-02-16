using System;
using System.Text.RegularExpressions;

namespace XX
{
	public sealed class REG
	{
		public bool _match(string __t, string __r)
		{
			this.t = __t;
			this.Nobj = Regex.Match(this.t, __r);
			return this.Nobj.Success;
		}

		public bool _match(string __t, Regex __r)
		{
			this.t = __t;
			this.Nobj = __r.Match(this.t);
			return this.Nobj.Success;
		}

		public string _R0
		{
			get
			{
				if (this.Nobj == null || !this.Nobj.Success)
				{
					return "";
				}
				return this.Nobj.Groups[0].Value;
			}
		}

		public string _R1
		{
			get
			{
				return this.Nobj.Groups[1].Value;
			}
		}

		public string _R2
		{
			get
			{
				return this.Nobj.Groups[2].Value;
			}
		}

		public string _R3
		{
			get
			{
				return this.Nobj.Groups[3].Value;
			}
		}

		public string _R4
		{
			get
			{
				return this.Nobj.Groups[4].Value;
			}
		}

		public string _R5
		{
			get
			{
				return this.Nobj.Groups[5].Value;
			}
		}

		public string _R6
		{
			get
			{
				return this.Nobj.Groups[6].Value;
			}
		}

		public string _R7
		{
			get
			{
				return this.Nobj.Groups[7].Value;
			}
		}

		public string _R8
		{
			get
			{
				return this.Nobj.Groups[8].Value;
			}
		}

		public string _R9
		{
			get
			{
				return this.Nobj.Groups[9].Value;
			}
		}

		public Match _Get()
		{
			return this.Nobj;
		}

		public string[] _makeArray()
		{
			if (this.Nobj == null || !this.Nobj.Success)
			{
				return new string[0];
			}
			string[] array = new string[this.Nobj.Length - 1];
			int length = this.Nobj.Length;
			for (int i = 1; i < length; i++)
			{
				array[i - 1] = this.Nobj.Groups[i].Value;
			}
			return array;
		}

		public string _leftContext
		{
			get
			{
				if (this.Nobj == null || !this.Nobj.Success)
				{
					return null;
				}
				return this.t.Substring(0, this.Nobj.Index);
			}
		}

		public string _rightContext
		{
			get
			{
				if (this.Nobj == null || !this.Nobj.Success)
				{
					return null;
				}
				int num = this.Nobj.Index + this.Nobj.Groups[0].Value.Length;
				return this.t.Substring(num, this.t.Length - num);
			}
		}

		public string _otherContext
		{
			get
			{
				string leftContext = this._leftContext;
				string rightContext = this._rightContext;
				return ((leftContext != null) ? leftContext : "") + ((rightContext != null) ? rightContext : "");
			}
		}

		public string _ReplaceExpression(string t)
		{
			if (this.Nobj == null || !this.Nobj.Success)
			{
				return null;
			}
			REG reg = new REG();
			string text = this._leftContext;
			while (reg._match(t, REG.RegDollerNumber))
			{
				t = reg._rightContext;
				text += reg._leftContext;
				int num = X.NmI(reg._R1, 0, false, false);
				if (this.Nobj.Groups.Count > num)
				{
					string text2 = text;
					Group group = this.Nobj.Groups[num];
					text = text2 + ((group != null) ? group.ToString() : null);
				}
			}
			return text + t + this._rightContext;
		}

		public static void initReg()
		{
			REG._MyReg = new REG();
		}

		public static bool match(string __t, string __r)
		{
			return REG._MyReg._match(__t, __r);
		}

		public static bool match(string __t, Regex __r)
		{
			return REG._MyReg._match(__t, __r);
		}

		public static string R0
		{
			get
			{
				return REG._MyReg._R0;
			}
		}

		public static string R1
		{
			get
			{
				return REG._MyReg._R1;
			}
		}

		public static string R2
		{
			get
			{
				return REG._MyReg._R2;
			}
		}

		public static string R3
		{
			get
			{
				return REG._MyReg._R3;
			}
		}

		public static string R4
		{
			get
			{
				return REG._MyReg._R4;
			}
		}

		public static string R5
		{
			get
			{
				return REG._MyReg._R5;
			}
		}

		public static string R6
		{
			get
			{
				return REG._MyReg._R6;
			}
		}

		public static string R7
		{
			get
			{
				return REG._MyReg._R7;
			}
		}

		public static string R8
		{
			get
			{
				return REG._MyReg._R8;
			}
		}

		public static string R9
		{
			get
			{
				return REG._MyReg._R9;
			}
		}

		public static Match Get()
		{
			return REG._MyReg._Get();
		}

		public static string[] makeArray()
		{
			return REG._MyReg._makeArray();
		}

		public static string ReplaceExpression(string t)
		{
			return REG._MyReg._ReplaceExpression(t);
		}

		public static string leftContext
		{
			get
			{
				return REG._MyReg._leftContext;
			}
		}

		public static string rightContext
		{
			get
			{
				return REG._MyReg._rightContext;
			}
		}

		public static string otherContext
		{
			get
			{
				return REG._MyReg._otherContext;
			}
		}

		public static readonly Regex RegCharW = new Regex("[A-Za-z0-9_]");

		public static readonly Regex RegCharVar = new Regex("[A-Za-z0-9\\.\\-_\\=\\;\\n\\r \\t\\s]");

		public static readonly Regex RegCharVarTilde = new Regex("[A-Za-z0-9\\.\\-_\\=\\;\\~\\+\\*\\/\\|\\&\\(\\)\\$\\n\\r \\t\\s]");

		public static readonly Regex RegCharPos = new Regex("[\\d\\.\\, ]");

		public static readonly Regex RegW = new Regex("^[A-Za-z0-9_]*$");

		public static readonly Regex RegVariable = new Regex("^[A-Za-z_][A-Za-z0-9_]*$");

		public static readonly Regex RegCharI = new Regex("[\\-\\d]");

		public static readonly Regex RegCharSI = new Regex("[\\d]");

		public static readonly Regex RegCharF = new Regex("[\\-\\.\\d]");

		public static readonly Regex RegCharSF = new Regex("[\\.\\d]");

		public static readonly Regex RegCharX = new Regex("[A-Fa-f\\d]");

		public static readonly Regex RegSuffixNumber = new Regex("_(\\d+)$");

		public static readonly Regex RegSuffixNumberOnly = new Regex("(\\d+)$");

		public static readonly Regex RegSuffixPeriodInt = new Regex("\\.([\\d\\-]+)$");

		public static readonly Regex RegPosition = new Regex("(\\-?[\\.\\d]+) *\\, *(\\-?[\\.\\d]+)");

		public static readonly Regex RegOneNumber = new Regex("(\\-?[\\.\\d]+)");

		public static readonly Regex RegDollerNumber = new Regex("\\$([\\d]+)");

		public static readonly Regex RegAllNumber = new Regex("^(\\-?[\\.\\d]+)$");

		public static readonly Regex RegAllNumberComma = new Regex("^([\\- \\.\\d\\,]+)$");

		public static readonly Regex RegFirstAlphabet = new Regex("^([a-zA-Z0-9]+)");

		public static readonly Regex RegFirstAlphabetUnderBar = new Regex("^_*([a-zA-Z0-9]+)");

		public static readonly Regex RegSmallNumber = new Regex("^\\-?\\d?\\d?\\d?(?:\\.\\d\\d?\\d?)?");

		public static readonly Regex RegSmallNumberWhole = new Regex("^\\-?\\d+(?:\\.\\d+)?$");

		public static readonly Regex RegTxTitle = new Regex("\\&\\&([A-Za-z0-9_]+)");

		public static readonly Regex RegKeisen = new Regex("─+");

		public static readonly Regex RegCRLF = new Regex("[\\n|\\r]");

		public static readonly Regex RegSpaceComma = new Regex("[ \\t\\,]");

		public static readonly Regex RegValueInput = new Regex(" *(\\w+) *\\=(~)?([^;\\n\\r]*);?");

		public static readonly Regex RegBigAlphabet = new Regex("^[A-Z]*$");

		public static readonly Regex RegAlphabet = new Regex("^[A-Za-z]*$");

		private static REG _MyReg;

		internal Match Nobj;

		internal string t;

		public delegate string FnGetReplacedString();
	}
}
