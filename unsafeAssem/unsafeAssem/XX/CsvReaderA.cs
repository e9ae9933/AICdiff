using System;
using Better;

namespace XX
{
	public class CsvReaderA : CsvReader
	{
		public CsvReaderA(string basetext, bool use_variable = false)
			: base(basetext, CsvReader.RegSpace, use_variable)
		{
		}

		public CsvReaderA(string basetext, CsvVariableContainer _VarCon)
			: base(basetext, CsvReader.RegSpace, _VarCon)
		{
		}

		public void seekReset()
		{
			this.finding_label = "";
			this.skip_count = 0;
			this.jump_bracket = false;
			this.jump_els = true;
			this.cur_line = 0;
			this.no_write_varcon = 0;
			this.labelscroll_count = 0;
			base.last_str = null;
		}

		public override bool read()
		{
			while (!base.isEnd())
			{
				if (this.finding_label == "")
				{
					if (!base.read())
					{
						return false;
					}
					bool flag = this.jump_els;
					this.jump_els = true;
					if (!this.jump_bracket && this.Acmds[0] == "}")
					{
						flag = true;
						base.shiftEmpty(1, 0);
						if (base.clength == 0)
						{
							continue;
						}
					}
					if (base.cmd == "LABEL" && base._1 != "")
					{
						if (this.Olabels == null)
						{
							this.Olabels = new BDic<string, int>();
						}
						this.Olabels[base._1] = this.cur_line;
						if (this.finding_label == base._1)
						{
							this.finding_label = "";
						}
					}
					if (this.jump_bracket)
					{
						if (base.cmd != "")
						{
							return true;
						}
					}
					else if (this.finding_label == "")
					{
						if (base.cmd == "IF" || base.cmd == "IF2")
						{
							if (this.falseJumpEvaluate())
							{
								if (!this.jumpIfFalse(2))
								{
									continue;
								}
							}
							else if (!this.slideIfTrue(2))
							{
								continue;
							}
						}
						else if (base.cmd == "IFSTR")
						{
							if (this.falseJumpEvaluate())
							{
								if (!this.jumpIfFalse(4))
								{
									continue;
								}
							}
							else if (!this.slideIfTrue(4))
							{
								continue;
							}
						}
						else if (base.cmd == "IFDEF")
						{
							if (this.falseJumpEvaluate())
							{
								if (!this.jumpIfFalse(2))
								{
									continue;
								}
							}
							else if (!this.slideIfTrue(2))
							{
								continue;
							}
						}
						else if (base.cmd == "IFNDEF")
						{
							if (this.falseJumpEvaluate())
							{
								if (!this.jumpIfFalse(2))
								{
									continue;
								}
							}
							else if (!this.slideIfTrue(2))
							{
								continue;
							}
						}
						else if (base.cmd == "IFLANG")
						{
							if (this.falseJumpEvaluate())
							{
								if (!this.jumpIfFalse(2))
								{
									continue;
								}
							}
							else if (!this.slideIfTrue(2))
							{
								continue;
							}
						}
						else if (base.cmd == "ELSIF" || base.cmd == "ELSIF2" || base.cmd == "ELSIFSTR" || base.cmd == "ELSIFLANG" || base.cmd == "ELSIFDEF" || base.cmd == "ELSIFNDEF")
						{
							if (flag)
							{
								while (base.cmd == "ELSIF" || base.cmd == "ELSIFSTR" || base.cmd == "ELSIF2" || base.cmd == "ELSE" || base.cmd == "ELSIFLANG" || base.cmd == "ELSIFDEF" || base.cmd == "ELSIFNDEF")
								{
									if (!this.jumpToNextBracket((base.cmd == "ELSIFSTR") ? 4 : 2))
									{
										break;
									}
								}
								continue;
							}
							if (this.falseJumpEvaluate())
							{
								if (!this.jumpIfFalse((base.cmd == "ELSIFSTR") ? 4 : 2))
								{
									continue;
								}
							}
							else if (!this.slideIfTrue((base.cmd == "ELSIFSTR") ? 4 : 2))
							{
								continue;
							}
						}
						else if (base.cmd == "ELSE")
						{
							if (flag)
							{
								this.jumpToNextBracket(1);
								continue;
							}
							if (!this.slideIfTrue(1))
							{
								continue;
							}
						}
						if (base.cmd != null && !(base.cmd == ""))
						{
							if (base.cmd == "SKIP")
							{
								this.skip_count++;
							}
							else
							{
								if (base.cmd == "IF_DEBUG")
								{
									this.skip_count += ((this.skip_count > 0 || !X.DEBUG) ? 1 : 0);
									continue;
								}
								if (base.cmd == "}")
								{
									this.skip_count = X.Mx(this.skip_count - 1, 0);
									continue;
								}
							}
							if (this.skip_count == 0)
							{
								if (base.cmd == "GOTO")
								{
									this.jumpTo(base._1);
								}
								else
								{
									if (base.cmd == "SEEK_END")
									{
										this.cur_line = this.Adata.Length;
										return false;
									}
									if (base.cmd == "SEEK_SET")
									{
										this.cur_line = X.NmI(base._1, 0, false, false);
									}
									else if (!(base.cmd == "{") && !(base.cmd == "LABEL") && !false)
									{
										return true;
									}
								}
							}
						}
					}
					else
					{
						int num = this.labelscroll_count - 1;
						this.labelscroll_count = num;
						if (num < 0)
						{
							X.de("ラベル " + this.finding_label + " が見つかりませんでした", null);
							return false;
						}
						if (base.isEnd())
						{
							this.cur_line = 0;
						}
					}
				}
				else
				{
					if (!base.readCorrectly())
					{
						return false;
					}
					if (base.stringsInput(base.last_str) && base.cmd == "LABEL" && base._1 != "")
					{
						if (this.Olabels == null)
						{
							this.Olabels = new BDic<string, int>();
						}
						this.Olabels[base._1] = this.cur_line;
						if (this.finding_label == base._1)
						{
							this.finding_label = "";
						}
					}
				}
			}
			return false;
		}

		private bool evalIfStr(string vl, string operand, string vr)
		{
			vl = TX.trim(vl);
			vr = TX.trim(vr);
			string text = operand.ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 2496394275U)
				{
					if (num <= 1079992859U)
					{
						if (num != 183859059U)
						{
							if (num != 866248570U)
							{
								if (num != 1079992859U)
								{
									goto IL_02A7;
								}
								if (!(text == "ENDS"))
								{
									goto IL_02A7;
								}
							}
							else
							{
								if (!(text == "ISNOT"))
								{
									goto IL_02A7;
								}
								return vl != vr;
							}
						}
						else
						{
							if (!(text == "NOTSTARTS"))
							{
								goto IL_02A7;
							}
							goto IL_026B;
						}
					}
					else if (num <= 1843990421U)
					{
						if (num != 1285841380U)
						{
							if (num != 1843990421U)
							{
								goto IL_02A7;
							}
							if (!(text == "IS"))
							{
								goto IL_02A7;
							}
							return vl == vr;
						}
						else
						{
							if (!(text == "NOTENDSWITH"))
							{
								goto IL_02A7;
							}
							goto IL_028D;
						}
					}
					else if (num != 1945876997U)
					{
						if (num != 2496394275U)
						{
							goto IL_02A7;
						}
						if (!(text == "ENDSWITH"))
						{
							goto IL_02A7;
						}
					}
					else
					{
						if (!(text == "ISNOTIN"))
						{
							goto IL_02A7;
						}
						return vr.IndexOf(vl) < 0;
					}
					return vl.IndexOf(vr) == vl.Length - vr.Length;
				}
				if (num <= 2908551163U)
				{
					if (num != 2631874108U)
					{
						if (num != 2783786092U)
						{
							if (num != 2908551163U)
							{
								goto IL_02A7;
							}
							if (!(text == "NOTSTARTSWITH"))
							{
								goto IL_02A7;
							}
							goto IL_026B;
						}
						else
						{
							if (!(text == "NOTENDS"))
							{
								goto IL_02A7;
							}
							goto IL_028D;
						}
					}
					else if (!(text == "STARTSWITH"))
					{
						goto IL_02A7;
					}
				}
				else if (num <= 3439459332U)
				{
					if (num != 3017670126U)
					{
						if (num != 3439459332U)
						{
							goto IL_02A7;
						}
						if (!(text == "STARTS"))
						{
							goto IL_02A7;
						}
					}
					else
					{
						if (!(text == "ISIN"))
						{
							goto IL_02A7;
						}
						return vr.IndexOf(vl) >= 0;
					}
				}
				else if (num != 3640136440U)
				{
					if (num != 3915038215U)
					{
						goto IL_02A7;
					}
					if (!(text == "NOTCONTAINS"))
					{
						goto IL_02A7;
					}
					return vl.IndexOf(vr) < 0;
				}
				else
				{
					if (!(text == "CONTAINS"))
					{
						goto IL_02A7;
					}
					return vl.IndexOf(vr) >= 0;
				}
				return vl.IndexOf(vr) == 0;
				IL_026B:
				return vl.IndexOf(vr) != 0;
				IL_028D:
				return vl.IndexOf(vr) != vl.Length - vr.Length;
			}
			IL_02A7:
			X.de("不明なオペランド:" + operand, null);
			return false;
		}

		protected virtual bool falseJumpEvaluate()
		{
			string cmd = base.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num > 2920975145U)
				{
					if (num <= 3168140685U)
					{
						if (num != 3014846266U)
						{
							if (num != 3168140685U)
							{
								goto IL_0180;
							}
							if (!(cmd == "IFDEF"))
							{
								goto IL_0180;
							}
							goto IL_0145;
						}
						else if (!(cmd == "IFLANG"))
						{
							goto IL_0180;
						}
					}
					else if (num != 3222953209U)
					{
						if (num != 4229469130U)
						{
							goto IL_0180;
						}
						if (!(cmd == "ELSIFLANG"))
						{
							goto IL_0180;
						}
					}
					else
					{
						if (!(cmd == "ELSIFSTR"))
						{
							goto IL_0180;
						}
						goto IL_0113;
					}
					return base._1U != TX.getCurrentFamilyName().ToUpper();
				}
				if (num <= 1461756063U)
				{
					if (num != 528843695U)
					{
						if (num != 1461756063U)
						{
							goto IL_0180;
						}
						if (!(cmd == "IFNDEF"))
						{
							goto IL_0180;
						}
					}
					else if (!(cmd == "ELSIFNDEF"))
					{
						goto IL_0180;
					}
					return this.VarCon != null && this.VarCon.isDefined(base._1);
				}
				if (num != 2348529725U)
				{
					if (num != 2920975145U)
					{
						goto IL_0180;
					}
					if (!(cmd == "IFSTR"))
					{
						goto IL_0180;
					}
				}
				else
				{
					if (!(cmd == "ELSIFDEF"))
					{
						goto IL_0180;
					}
					goto IL_0145;
				}
				IL_0113:
				return !this.evalIfStr(base._1, base._2, base._3);
				IL_0145:
				return this.VarCon == null || !this.VarCon.isDefined(base._1);
			}
			IL_0180:
			if (X.DEBUG && X.DEBUGTIMESTAMP && X.DEBUGANNOUNCE && TX.isEnd(base._1, '{'))
			{
				base.tError("書式エラー: IF で _1 の最後に { が入ってます");
			}
			return TX.eval(base._1, "") == 0.0;
		}

		private bool jumpIfFalse(int first_bracket_pos)
		{
			bool flag = false;
			bool flag2 = false;
			if (this.jumpToNextBracket(first_bracket_pos))
			{
				while (base.cmd == "ELSIF" || base.cmd == "ELSIF2" || base.cmd == "ELSIFSTR" || base.cmd == "ELSIFLANG" || base.cmd == "ELSIFDEF" || base.cmd == "ELSIFNDEF")
				{
					if (!this.falseJumpEvaluate())
					{
						flag = this.slideIfTrue((base.cmd == "ELSIFSTR") ? 4 : 2);
						break;
					}
					if (!this.jumpToNextBracket((base.cmd == "ELSIFSTR") ? 4 : 2))
					{
						this.jump_els = false;
						flag2 = true;
						break;
					}
				}
			}
			else
			{
				this.jump_els = false;
				flag2 = true;
			}
			if (!flag2)
			{
				this.jump_els = base.cmd == "ELSIF" || base.cmd == "ELSIF2" || base.cmd == "ELSIFSTR" || base.cmd == "ELSIFNDEF" || base.cmd == "ELSIFDEF" || base.cmd == "ELSIFLANG" || base.cmd == "ELSE";
			}
			if (base.cmd == "ELSE" && !flag)
			{
				flag = this.slideIfTrue(1);
			}
			return flag;
		}

		public bool slideIfTrue(int i = 2)
		{
			if (base.clength > i && this.Acmds[i] == "{")
			{
				return false;
			}
			this.jump_els = true;
			if (base.clength <= i || this.Acmds[i] == "")
			{
				return false;
			}
			base.shiftEmpty(i, 0);
			bool flag = base.cmd != "";
			if (flag && this.VarCon != null)
			{
				flag = flag && !this.VarCon.read(this, base.slice_join(0, " ", ""), true, true);
			}
			return flag;
		}

		public bool jumpToNextBracket(int first_bracket_pos)
		{
			this.jump_bracket = true;
			CsvVariableContainer varCon = this.VarCon;
			this.VarCon = null;
			int num = 0;
			int num2 = 0;
			int cur_line = this.cur_line;
			int i = 0;
			bool flag = false;
			while (i <= 1)
			{
				int clength = base.clength;
				if (num2 != 0 && this.Acmds[0] == "}")
				{
					num--;
				}
				if (num != 0 || num2 == 0)
				{
					for (int j = 0; j < clength; j++)
					{
						if (this.Acmds[j] == "{")
						{
							num++;
						}
					}
				}
				if (num == 0)
				{
					if (num2 == 0)
					{
						if (base.clength > first_bracket_pos && this.Acmds[first_bracket_pos] != "{")
						{
							base.shiftEmpty(base.clength - 2, 2);
						}
						else
						{
							this.read();
						}
						flag = false;
						break;
					}
					base.shiftEmpty(1, 0);
					flag = base.clength > 0;
					break;
				}
				else
				{
					if (i == 0)
					{
						this.read();
					}
					num2++;
					if (base.isEnd())
					{
						i++;
						if (i >= 2)
						{
							if (num != 0)
							{
								X.de("ブラケット展開中に EOF に到達 (初期行 " + cur_line.ToString() + ")", null);
								break;
							}
							break;
						}
					}
				}
			}
			if (i >= 2)
			{
				base.clength = 0;
			}
			this.VarCon = varCon;
			this.jump_bracket = false;
			if (flag && this.VarCon != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					base.slice_join(stb, 0, " ", "");
					int num3 = 0;
					int num4;
					stb.csvAdjust(ref num3, out num4, -1);
					flag = flag && base.readInner(stb, num3, num4);
				}
			}
			return flag;
		}

		public int countBracketsOnThisLine(ref int _cnt)
		{
			for (int i = base.clength - 1; i >= 0; i--)
			{
				string text = this.Acmds[i];
				if (text != null)
				{
					if (!(text == "{"))
					{
						if (text == "}")
						{
							_cnt--;
						}
					}
					else
					{
						_cnt++;
					}
				}
			}
			return _cnt;
		}

		public void jumpTo(string label)
		{
			this.labelscroll_count = 0;
			if (this.Olabels == null || !this.Olabels.TryGetValue(label, out this.cur_line))
			{
				this.finding_label = label;
				this.labelscroll_count = this.Adata.Length;
			}
		}

		public int restLines()
		{
			return this.Adata.Length - this.cur_line + 1;
		}

		public CsvReaderA seekBack()
		{
			this.cur_line = X.Mx(this.cur_line - 1, 0);
			return this;
		}

		public CsvReaderA save()
		{
			if (this.Astores == null)
			{
				this.Astores = new int[] { this.cur_line };
			}
			else
			{
				X.push<int>(ref this.Astores, this.cur_line, -1);
			}
			return this;
		}

		public CsvReaderA restore()
		{
			if (this.Astores != null)
			{
				this.cur_line = X.pop<int>(ref this.Astores);
				if (this.Astores.Length == 0)
				{
					this.Astores = null;
				}
			}
			return this;
		}

		public override void destruct()
		{
			this.Olabels = null;
			base.destruct();
		}

		public static bool defVariable = true;

		private string finding_label = "";

		private int skip_count;

		protected BDic<string, int> Olabels;

		private int[] Astores;

		protected bool jump_bracket;

		protected int labelscroll_count;

		private bool jump_els = true;
	}
}
