using System;
using System.Collections.Generic;
using Better;

namespace XX
{
	public class EfSetterP
	{
		public static EfSetterP scriptConvert(EfSetterP SP)
		{
			STB sourceStb = SP.SourceStb;
			int length = sourceStb.Length;
			SP.max_line = 0;
			int num = 0;
			bool flag = false;
			for (int i = 0; i < length; i++)
			{
				char c = sourceStb[i];
				if (c == '\n')
				{
					SP.max_line += 1;
					if (flag)
					{
						num++;
					}
					flag = false;
				}
				else if (flag)
				{
					num++;
				}
				else if (c == '/' && i < length - 1 && sourceStb[i + 1] == '/')
				{
					flag = true;
				}
				else if (c == '\\')
				{
					i++;
					if (flag)
					{
						num++;
					}
				}
				else if (c == '&' && i < length - 1 && TX.isVariableNameMatch(sourceStb[i + 1]))
				{
					c = (sourceStb[i] = '$');
				}
				if (num > 0)
				{
					sourceStb[i - num] = c;
				}
			}
			if (num > 0)
			{
				sourceStb.Length -= num;
			}
			sourceStb.TrimCapacity();
			return SP;
		}

		public void ParseScript()
		{
			if (this.AELine != null)
			{
				this.allUnbake();
				return;
			}
			EfSetterP.AGenBuf.Clear();
			EfSetterP.AGenBufGuide.Clear();
			EfSetterP.AELBuf.Clear();
			int i = 0;
			int length = this.SourceStb.Length;
			int num = 0;
			while (i < length)
			{
				int num2;
				int num3;
				this.SourceStb.ScrollLine(i, out num2, length).csvAdjust(ref i, out num3, num2);
				EfSetterP.ESPLine espline = new EfSetterP.ESPLine(this.SourceStb, i, ref num3, num++);
				EfSetterP.AELBuf.Add(espline);
				int count = EfSetterP.AGenBuf.Count;
				i = num3 + 1;
			}
			this.AELine = EfSetterP.AELBuf.ToArray();
			this.SourceStb = null;
		}

		public void allUnbake()
		{
			if (this.AELine == null)
			{
				return;
			}
			for (int i = this.AELine.Length - 1; i >= 0; i--)
			{
				this.AELine[i].allUnbake();
			}
		}

		public EfSetterP(string _key)
		{
			this.key = _key;
			this.SourceStb = new STB(128);
			this.Hash = new HashP(4);
			if (EfSetterP.AGenBuf == null)
			{
				EfSetterP.AELBuf = new List<EfSetterP.ESPLine>(8);
				EfSetterP.AGenBuf = new List<EfSetterP.ESPLine>(8);
				EfSetterP.AGenBufGuide = new List<EfSetterP.ESPLineGuide>(8);
			}
		}

		public EfSetterP ScriptAdd(EfSetterP PS)
		{
			this.SourceStb += PS.SourceStb;
			return this;
		}

		public EfSetterP ScriptSet(EfSetterP PS)
		{
			this.SourceStb.Set(PS.SourceStb);
			return this;
		}

		public EfSetterP ScriptAdd(string s)
		{
			if (TX.valid(s))
			{
				this.SourceStb.Append(s, "\n");
			}
			return this;
		}

		public EfSetterP.ESPLine getNextLine(ref int next)
		{
			bool flag = false;
			EfSetterP.ESPLine espline = null;
			while (!flag)
			{
				if (next >= this.AELine.Length)
				{
					return null;
				}
				EfSetterP.ESPLine[] aeline = this.AELine;
				int num = next;
				next = num + 1;
				espline = aeline[num];
				switch (espline.espk)
				{
				case EfSetterP.ESPK.NOUSE:
				case EfSetterP.ESPK.LABEL:
					break;
				case EfSetterP.ESPK.DEFINE:
					EfSetterP.TargetVP.Add(espline.cmd, espline.getReplacable(1).unbake(), false);
					break;
				case EfSetterP.ESPK.DEFINE_EVAL:
					EfSetterP.TargetVP.Add(espline.cmd, (double)espline.Nm(1, 0f));
					break;
				case EfSetterP.ESPK.GOTO:
					if (espline.jumpto >= 0)
					{
						next = espline.jumpto + 1;
						if (this.goto_count >= 8)
						{
							flag = true;
						}
						else
						{
							this.goto_count += 1;
						}
					}
					else
					{
						X.de("不明なラベル: " + espline._1, null);
					}
					break;
				case EfSetterP.ESPK.IF:
				case EfSetterP.ESPK.IFDEF:
				case EfSetterP.ESPK.IFNDEF:
				case EfSetterP.ESPK.IFSTR:
					this.chooseSuitableIfPoint(espline, ref next);
					break;
				case EfSetterP.ESPK.ELSE:
				case EfSetterP.ESPK.ELSIF:
				case EfSetterP.ESPK.ELSIFSTR:
				case EfSetterP.ESPK.ELSIFDEF:
				case EfSetterP.ESPK.ELSIFNDEF:
					this.goToBracketBlockQuit(espline, ref next);
					break;
				case EfSetterP.ESPK.DEBUG:
				{
					STB stb = TX.PopBld(null, 0);
					stb.Add("DEBUG: ", (next - 1).ToString(), "...");
					stb.Add(espline.ToString());
					X.dl(stb.ToString(), null, false, false);
					TX.ReleaseBld(stb);
					break;
				}
				case EfSetterP.ESPK.SEEK_SET:
					next = 0;
					break;
				case EfSetterP.ESPK.SEEK_END:
					next = this.AELine.Length;
					break;
				default:
					flag = true;
					break;
				}
			}
			return espline;
		}

		private void chooseSuitableIfPoint(EfSetterP.ESPLine L, ref int next)
		{
			for (;;)
			{
				int num;
				switch (L.espk)
				{
				case EfSetterP.ESPK.NOUSE:
				{
					if (next >= this.AELine.Length)
					{
						return;
					}
					EfSetterP.ESPLine[] aeline = this.AELine;
					num = next;
					next = num + 1;
					L = aeline[num];
					continue;
				}
				case EfSetterP.ESPK.IF:
				case EfSetterP.ESPK.ELSIF:
					if (L.Nm(1, 0f) != 0f)
					{
						return;
					}
					goto IL_00E3;
				case EfSetterP.ESPK.ELSE:
					return;
				case EfSetterP.ESPK.IFDEF:
				case EfSetterP.ESPK.ELSIFDEF:
					if (EfSetterP.TargetVP != null && EfSetterP.TargetVP.isDefined(L.getReplacable(1)))
					{
						return;
					}
					goto IL_00E3;
				case EfSetterP.ESPK.IFNDEF:
				case EfSetterP.ESPK.ELSIFNDEF:
					if (EfSetterP.TargetVP != null && !EfSetterP.TargetVP.isDefined(L.getReplacable(1)))
					{
						return;
					}
					goto IL_00E3;
				case EfSetterP.ESPK.IFSTR:
				case EfSetterP.ESPK.ELSIFSTR:
				{
					ReplacableString replacable = L.getReplacable(1);
					ReplacableString replacable2 = L.getReplacable(3);
					if (replacable.calcOperandIsStr(L.getIndex(2), replacable2, EfSetterP.TargetVP))
					{
						return;
					}
					goto IL_00E3;
				}
				}
				break;
				IL_00E3:
				next = ((L.jumpto < 0) ? this.AELine.Length : L.jumpto);
				if (next >= this.AELine.Length)
				{
					return;
				}
				EfSetterP.ESPLine[] aeline2 = this.AELine;
				num = next;
				next = num + 1;
				L = aeline2[num];
			}
			next--;
		}

		private void goToBracketBlockQuit(EfSetterP.ESPLine L, ref int next)
		{
			for (;;)
			{
				if (L.espk == EfSetterP.ESPK.NOUSE)
				{
					next++;
				}
				else
				{
					next = ((L.jumpto < 0) ? this.AELine.Length : L.jumpto);
				}
				if (L.espk == EfSetterP.ESPK.ELSE || next >= this.AELine.Length)
				{
					break;
				}
				L = this.AELine[next];
				EfSetterP.ESPK espk = L.espk;
				if (espk != EfSetterP.ESPK.NOUSE && espk - EfSetterP.ESPK.ELSE > 1 && espk - EfSetterP.ESPK.ELSIFSTR > 1)
				{
					return;
				}
			}
		}

		public static int cmd_count_for_if(EfSetterP.ESPK espk)
		{
			if (espk == EfSetterP.ESPK.ELSE)
			{
				return 0;
			}
			if (espk - EfSetterP.ESPK.IFSTR <= 1)
			{
				return 3;
			}
			return 1;
		}

		public int countLines()
		{
			if (this.AELine == null)
			{
				return 0;
			}
			return this.AELine.Length;
		}

		public EfSetterP.ESPLine GetLine(int i)
		{
			if (this.AELine == null)
			{
				return null;
			}
			return this.AELine[i];
		}

		public string key;

		public uint id;

		private STB SourceStb;

		private byte max_line;

		private EfSetterP.ESPLine[] AELine;

		public HashP Hash;

		public byte goto_count;

		private const byte GOTO_MAX = 8;

		private static List<EfSetterP.ESPLine> AGenBuf;

		private static List<EfSetterP.ESPLine> AELBuf;

		private static List<EfSetterP.ESPLineGuide> AGenBufGuide;

		public static VariableP TargetVP;

		public enum ESPK : byte
		{
			NORMAL,
			NOUSE,
			DEFINE,
			DEFINE_EVAL,
			GOTO,
			LABEL,
			IF,
			ELSE,
			ELSIF,
			IFDEF,
			IFNDEF,
			IFSTR,
			ELSIFSTR,
			ELSIFDEF,
			ELSIFNDEF,
			DEBUG,
			SEEK_SET,
			SEEK_END
		}

		public struct ESPLineGuide
		{
			public ESPLineGuide(string _dep_label, int _dep_char = 0)
			{
				this.dep_label = _dep_label;
				this.dep_char = _dep_char;
			}

			public string dep_label;

			public int dep_char;
		}

		public sealed class ESPLine : IStringHolder
		{
			public ESPLine(STB SourceStb, int i, ref int line_read_end, int this_line)
			{
				this.this_line = this_line;
				int num;
				int num2;
				bool flag;
				if (SourceStb.vpVarDefineHeader(i, out num, out num2, out flag, line_read_end))
				{
					this.RawCmd = new ReplacableString(SourceStb, i, num);
					this.Acmds = new string[] { SourceStb.get_slice(num2, line_read_end) };
					this.espk = (flag ? EfSetterP.ESPK.DEFINE_EVAL : EfSetterP.ESPK.DEFINE);
				}
				else
				{
					int num3;
					SourceStb.Scroll(i, out num3, (char c) => c == '}' || TX.isSpaceOrComma(c), line_read_end);
					int num4;
					SourceStb.Scroll(num3, out num4, (char c) => !TX.isSpaceOrCommaOrTilde(c), line_read_end);
					this.RawCmd = new ReplacableString(SourceStb, num3, num4 - num3);
					this.Acmds = SourceStb.csvSplit(num4, line_read_end, false).ToArray();
					if (!FEnum<EfSetterP.ESPK>.TryParse(this.cmd, out this.espk, true))
					{
						string cmd = this.cmd;
						if (cmd != null && (cmd == "{" || cmd == "}" || (cmd != null && cmd.Length == 0)))
						{
							this.espk = EfSetterP.ESPK.NOUSE;
						}
						else
						{
							this.espk = EfSetterP.ESPK.NORMAL;
						}
					}
					else
					{
						EfSetterP.ESPK espk = this.espk;
						if (espk - EfSetterP.ESPK.GOTO > 1)
						{
							if (espk - EfSetterP.ESPK.IF <= 8)
							{
								if (SourceStb[line_read_end - 1] == '{')
								{
									int num5;
									SourceStb.ScrollBracket(line_read_end - 1, out num5, -1);
									EfSetterP.AGenBuf.Add(this);
									EfSetterP.AGenBufGuide.Add(new EfSetterP.ESPLineGuide(null, num5 - 1));
								}
								else
								{
									int num6 = EfSetterP.cmd_count_for_if(this.espk);
									this.jumpto = this_line + 2;
									if (this.clength > 1 + EfSetterP.cmd_count_for_if(this.espk))
									{
										SourceStb.SkipSeparationItem(num4, num6, out line_read_end, line_read_end, null, null);
										Array.Resize<string>(ref this.Acmds, num6);
									}
								}
							}
						}
						else if (this.clength >= 2)
						{
							bool flag2 = true;
							if (this.espk == EfSetterP.ESPK.GOTO)
							{
								for (int j = EfSetterP.AGenBuf.Count - 1; j >= 0; j--)
								{
									EfSetterP.ESPLine espline = EfSetterP.AGenBuf[j];
									if (espline.espk == EfSetterP.ESPK.LABEL && espline._1 == this._1)
									{
										flag2 = false;
										this.jumpto = EfSetterP.AGenBufGuide[j].dep_char;
										break;
									}
								}
							}
							if (flag2)
							{
								EfSetterP.AGenBuf.Add(this);
								EfSetterP.AGenBufGuide.Add(new EfSetterP.ESPLineGuide(this._1, this_line));
							}
						}
					}
					for (int k = EfSetterP.AGenBuf.Count - 1; k >= 0; k--)
					{
						EfSetterP.ESPLine espline2 = EfSetterP.AGenBuf[k];
						if (espline2 != this && this.checkGuide(espline2, EfSetterP.AGenBufGuide[k], i, line_read_end, this_line))
						{
							break;
						}
					}
				}
				this.pre_script = this.ToString();
			}

			public bool checkGuide(EfSetterP.ESPLine Target, EfSetterP.ESPLineGuide Guide, int this_start_char, int this_end_char, int this_line)
			{
				EfSetterP.ESPK espk = Target.espk;
				if (espk != EfSetterP.ESPK.GOTO)
				{
					if (espk - EfSetterP.ESPK.IF <= 8 && X.BTW((float)this_start_char, (float)Guide.dep_char, (float)this_end_char))
					{
						Target.jumpto = this_line;
						return true;
					}
				}
				else if (this.espk == EfSetterP.ESPK.LABEL && this._1 == Guide.dep_label)
				{
					Target.jumpto = this_line;
					return true;
				}
				return false;
			}

			public override string ToString()
			{
				return this.RawCmd.ToString() + ((this.espk == EfSetterP.ESPK.DEFINE) ? "=" : ((this.espk == EfSetterP.ESPK.DEFINE_EVAL) ? "~" : " ")) + TX.join<string>(" ", this.Acmds, 0, this.Acmds.Length);
			}

			public int clength
			{
				get
				{
					return this.Acmds.Length + 1;
				}
			}

			public string getIndex(int _i)
			{
				if (_i == 0)
				{
					return this.RawCmd.ToString();
				}
				if (this.clength <= _i)
				{
					return "";
				}
				return this.Acmds[_i - 1];
			}

			public string getRandom(int s = 1, int len = -1)
			{
				if (len < 0)
				{
					len = this.clength - s;
				}
				return this.getIndex((len <= 1) ? s : (s + X.xors(len)));
			}

			public StringKey getRandomHash(HashP Hash, int s = 1, int len = -1)
			{
				if (len < 0)
				{
					len = this.clength - s;
				}
				ReplacableString replacable = this.getReplacable((len <= 1) ? s : (s + X.xors(len)));
				return Hash.Get(replacable, EfSetterP.TargetVP);
			}

			public ReplacableString getReplacable(int _i)
			{
				if (_i == 0)
				{
					return this.RawCmd;
				}
				if (this.clength <= _i || _i < 0)
				{
					return null;
				}
				if (this.AReplacableCmd == null)
				{
					this.AReplacableCmd = new ReplacableString[this.clength - 1];
				}
				_i--;
				ReplacableString replacableString = this.AReplacableCmd[_i];
				if (replacableString == null)
				{
					replacableString = (this.AReplacableCmd[_i] = new ReplacableString(this.Acmds[_i]));
				}
				return replacableString;
			}

			public void allUnbake()
			{
				this.RawCmd.unbake();
				if (this.AReplacableCmd == null)
				{
					return;
				}
				int num = this.AReplacableCmd.Length;
				for (int i = 0; i < num; i++)
				{
					ReplacableString replacableString = this.AReplacableCmd[i];
					if (replacableString != null)
					{
						replacableString.unbake();
					}
				}
			}

			public float Nm(int _i, float defv = 0f)
			{
				if (this.clength <= _i || _i <= 0)
				{
					return defv;
				}
				if (this.AEvalMem == null)
				{
					this.AEvalMem = new EvalP[this.clength - 1];
				}
				_i--;
				EvalP evalP = this.AEvalMem[_i];
				if (evalP == null)
				{
					evalP = (this.AEvalMem[_i] = new EvalP(null).Parse(this.Acmds[_i]));
				}
				return (float)evalP.getValue(EfSetterP.TargetVP);
			}

			public bool isNm(int i)
			{
				ReplacableString replacable = this.getReplacable(i);
				if (replacable == null)
				{
					return false;
				}
				string text = replacable.ToString();
				return text.Length != 0 && (TX.isNumMatch(text[0]) || text[0] == '$');
			}

			public float NmE(int _i, float defv = 0f)
			{
				return this.Nm(_i, defv);
			}

			public int Int(int _i, int defv = 0)
			{
				return (int)this.Nm(_i, (float)defv);
			}

			public int IntE(int _i, int defv = 0)
			{
				return (int)this.Nm(_i, (float)defv);
			}

			public string cmd
			{
				get
				{
					return this.RawCmd.ToString();
				}
			}

			public string _1
			{
				get
				{
					return this.getIndex(1);
				}
			}

			public string _2
			{
				get
				{
					return this.getIndex(2);
				}
			}

			public string _3
			{
				get
				{
					return this.getIndex(3);
				}
			}

			public string _4
			{
				get
				{
					return this.getIndex(4);
				}
			}

			public string _5
			{
				get
				{
					return this.getIndex(5);
				}
			}

			public string _6
			{
				get
				{
					return this.getIndex(6);
				}
			}

			public string _7
			{
				get
				{
					return this.getIndex(7);
				}
			}

			public float _N1
			{
				get
				{
					return this.Nm(1, 0f);
				}
			}

			public float _N2
			{
				get
				{
					return this.Nm(2, 0f);
				}
			}

			public float _N3
			{
				get
				{
					return this.Nm(3, 0f);
				}
			}

			public float _N4
			{
				get
				{
					return this.Nm(4, 0f);
				}
			}

			public float _N5
			{
				get
				{
					return this.Nm(5, 0f);
				}
			}

			public float _N6
			{
				get
				{
					return this.Nm(6, 0f);
				}
			}

			public float _N7
			{
				get
				{
					return this.Nm(7, 0f);
				}
			}

			public float NE1()
			{
				return this.Nm(1, 0f);
			}

			public float NE2()
			{
				return this.Nm(2, 0f);
			}

			public float NE3()
			{
				return this.Nm(3, 0f);
			}

			public float NE4()
			{
				return this.Nm(4, 0f);
			}

			public float NE5()
			{
				return this.Nm(5, 0f);
			}

			public float NE6()
			{
				return this.Nm(6, 0f);
			}

			public float NE7()
			{
				return this.Nm(7, 0f);
			}

			public bool tError(string e)
			{
				X.de(e, null);
				X.de("Line: " + this.this_line.ToString(), null);
				return true;
			}

			private string[] Acmds;

			private ReplacableString RawCmd;

			public readonly int this_line;

			private EvalP[] AEvalMem;

			private ReplacableString[] AReplacableCmd;

			public EfSetterP.ESPK espk;

			public int jumpto = -1;

			public string pre_script;
		}
	}
}
