using System;
using System.Collections.Generic;

namespace XX
{
	public class EvalP : IValItem
	{
		public EvalP(List<IValItem> Src = null)
		{
			this.AItems = Src ?? new List<IValItem>(4);
			if (EvalP.Aargsbuf == null)
			{
				EvalP.Aargsbuf = new List<string>(8);
			}
		}

		public EvalP Parse(string s)
		{
			STB stb = TX.PopBld(s, 0);
			this.Parse(stb, 0, -1);
			TX.ReleaseBld(stb);
			return this;
		}

		public EvalP Clear()
		{
			this.AItems.Clear();
			return this;
		}

		public EvalP Multiple(int _sign)
		{
			this.sign *= _sign;
			return this;
		}

		public EvalP Parse(STB Stb, int charindex, int e = -1)
		{
			int num;
			return this.Parse(Stb, charindex, out num, e);
		}

		public EvalP Parse(STB Stb, int charindex, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = Stb.Length;
			}
			Stb.TaleTrimSpace(e, out e, charindex);
			int num = charindex;
			double num2 = 1.0;
			bool flag = false;
			EvalP.parse_error = false;
			bool flag2 = false;
			while (num < e && !EvalP.parse_error)
			{
				char c2 = Stb[num];
				if (c2 == '(')
				{
					Stb.ScrollBracket(num, out end_char_index, -1);
					if (end_char_index == e && !flag2 && this.AItems.Count == 0)
					{
						e--;
						Stb.TaleTrimSpace(e, out e, charindex);
						num++;
					}
					else
					{
						this.Attach(new EvalP(null).Parse(Stb, num + 1, end_char_index - 1).Multiple((int)num2), ref num2, ref flag);
						num2 = 1.0;
						num = end_char_index;
					}
				}
				else if (c2 == ')' || c2 == ' ' || c2 == '\t' || c2 == '+')
				{
					num++;
				}
				else
				{
					if (c2 == '\n' || c2 == '\r')
					{
						EvalP.parse_error = true;
						end_char_index = e;
						return this;
					}
					flag2 = true;
					if (c2 == '-')
					{
						num2 *= -1.0;
						num++;
					}
					else if (TX.isNumMatch(c2))
					{
						STB.PARSERES parseres = Stb.Nm(num, out end_char_index, e, false);
						if (parseres == STB.PARSERES.ERROR)
						{
							X.de("Nm パースに失敗 ", Stb.get_slice(num, end_char_index), null);
						}
						else
						{
							if (parseres == STB.PARSERES.INT)
							{
								this.Attach(new EvalP.ConstantiateNum((double)STB.parse_result_int * num2), ref num2, ref flag);
							}
							if (parseres == STB.PARSERES.DOUBLE)
							{
								this.Attach(new EvalP.ConstantiateNum(STB.parse_result_double * num2), ref num2, ref flag);
							}
							num2 = 1.0;
						}
						num = end_char_index;
					}
					else if (TX.isVariableNameMatch(c2))
					{
						Stb.Scroll(num, out end_char_index, (char c) => TX.isVariableNameMatch(c) || c == ' ', e);
						string text = Stb.get_slice(num, end_char_index);
						int num3 = num;
						int num4 = end_char_index - num;
						EvalP.Aargsbuf.Clear();
						num = end_char_index;
						while (num < e && Stb[num] == '[')
						{
							Stb.ScrollBracket(num, out end_char_index, e);
							EvalP.Aargsbuf.Add(Stb.get_slice(num + 1, end_char_index - 1));
							num = end_char_index;
						}
						this.Attach(new EvalP.ListenEvalBlock(Stb, num3, num4, EvalP.Aargsbuf, (int)num2), ref num2, ref flag);
					}
					else if (c2 == '$')
					{
						Stb.Scroll(num + 1, out end_char_index, (char c) => TX.isVariableNameMatch(c), e);
						this.Attach(new EvalP.VariableBlock(Stb.get_slice(num + 1, end_char_index), (int)num2), ref num2, ref flag);
						num = end_char_index;
					}
					else
					{
						STB.MULT_OPE mult_OPE = Stb.ScrollOperand(num, out end_char_index, e);
						if (mult_OPE != STB.MULT_OPE.ERROR)
						{
							if (mult_OPE == STB.MULT_OPE.NOT)
							{
								flag = !flag;
							}
							else
							{
								int num5 = STB.ope_level(mult_OPE);
								if (num5 == 5)
								{
									EvalP.LogicalBlock logicalBlock = new EvalP.LogicalBlock(mult_OPE, new EvalP(this.AItems));
									this.AItems = new List<IValItem>(4);
									this.AItems.Add(logicalBlock);
									flag = false;
									num2 = 1.0;
								}
								else if (num5 == 4)
								{
									EvalP.CompareBlock compareBlock = new EvalP.CompareBlock(mult_OPE);
									this.AItems.Add(compareBlock);
									flag = false;
									num2 = 1.0;
								}
								else
								{
									flag = false;
									num2 = 1.0;
									IValItem valItem = null;
									if (this.AItems.Count > 0)
									{
										valItem = this.AItems[this.AItems.Count - 1];
										if (valItem is EvalP.LogicalBlock)
										{
											valItem = null;
										}
									}
									if (valItem is EvalP.MultipleBlock)
									{
										(valItem as EvalP.MultipleBlock).addOpe(mult_OPE);
									}
									else
									{
										EvalP.MultipleBlock multipleBlock = new EvalP.MultipleBlock(valItem, mult_OPE);
										if (valItem != null)
										{
											this.AItems[this.AItems.Count - 1] = multipleBlock;
										}
										else
										{
											this.AItems.Add(multipleBlock);
										}
									}
								}
							}
						}
						num = end_char_index;
					}
				}
			}
			end_char_index = num;
			return this;
		}

		private void Attach(IValItem P, ref double next_sign, ref bool next_not)
		{
			if (next_not)
			{
				next_not = false;
				P = new EvalP.NotBlock(P);
			}
			next_sign = 1.0;
			if (this.AItems.Count > 0)
			{
				EvalP.MultipleBlock multipleBlock = this.AItems[this.AItems.Count - 1] as EvalP.MultipleBlock;
				if (multipleBlock != null && multipleBlock.addValItem(P))
				{
					return;
				}
			}
			this.AItems.Add(P);
		}

		public double getValue(VariableP VP)
		{
			int count = this.AItems.Count;
			double num = 0.0;
			int num2 = this.sign;
			STB.MULT_OPE mult_OPE = STB.MULT_OPE.ERROR;
			STB.MULT_OPE mult_OPE2 = STB.MULT_OPE.ERROR;
			double num3 = 0.0;
			double num4 = 0.0;
			for (int i = 0; i < count; i++)
			{
				IValItem valItem = this.AItems[i];
				if (valItem is EvalP.LogicalBlock)
				{
					EvalP.LogicalBlock logicalBlock = valItem as EvalP.LogicalBlock;
					num4 = logicalBlock.getValue(VP);
					if (logicalBlock.ope == STB.MULT_OPE.COMP_AND && num4 == 0.0)
					{
						return 0.0;
					}
					mult_OPE2 = logicalBlock.ope;
					num = 0.0;
				}
				else if (valItem is EvalP.CompareBlock)
				{
					mult_OPE = (valItem as EvalP.CompareBlock).ope;
					num3 = num;
					num = 0.0;
				}
				else
				{
					num += valItem.getValue(VP);
				}
			}
			if (mult_OPE != STB.MULT_OPE.ERROR)
			{
				num = EvalP.CompareOpe(num3, mult_OPE, num);
			}
			if (mult_OPE2 != STB.MULT_OPE.ERROR)
			{
				num = (double)(((mult_OPE2 == STB.MULT_OPE.COMP_AND) ? (num4 != 0.0 && num != 0.0) : (num4 != 0.0 || num != 0.0)) ? 1 : 0);
			}
			return num * (double)this.sign;
		}

		private static double CompareOpe(double l, STB.MULT_OPE ope, double r)
		{
			switch (ope)
			{
			case STB.MULT_OPE.COMP_NOTEQUAL:
				return (double)((l == r) ? 0 : 1);
			case STB.MULT_OPE.COMP_G:
				return (double)((l > r) ? 1 : 0);
			case STB.MULT_OPE.COMP_GE:
				return (double)((l >= r) ? 1 : 0);
			case STB.MULT_OPE.COMP_L:
				return (double)((l < r) ? 1 : 0);
			case STB.MULT_OPE.COMP_LE:
				return (double)((l <= r) ? 1 : 0);
			default:
				return (double)((l == r) ? 1 : 0);
			}
		}

		private int sign = 1;

		private List<IValItem> AItems;

		private static List<string> Aargsbuf;

		private static bool parse_error;

		public sealed class ConstantiateNum : IValItem
		{
			public ConstantiateNum(double _val)
			{
				this.val = _val;
			}

			public double getValue(VariableP VP)
			{
				return this.val;
			}

			public double val;
		}

		public sealed class VariableBlock : IValItem
		{
			public VariableBlock(string _key, int _sign)
			{
				this.key = _key;
				this.sign = _sign;
			}

			public double getValue(VariableP VP)
			{
				if (VP == null)
				{
					return 0.0;
				}
				return VP.getDouble(this.key) * (double)this.sign;
			}

			public string key;

			public int sign;
		}

		public sealed class ListenEvalBlock : ReplacableString, IValItem
		{
			public ListenEvalBlock(STB Stb, int key_si, int key_len, List<string> _Aargs, int _sign)
				: base(Stb, key_si, key_len)
			{
				this.sign = _sign;
				string text = this.source.ToUpper();
				if (text != null)
				{
					uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
					if (num <= 1561581017U)
					{
						if (num <= 923884400U)
						{
							if (num != 523493304U)
							{
								if (num == 923884400U)
								{
									if (text == "YD")
									{
										this.bfunc = EvalP.ListenEvalBlock.BFUNC.YD;
										this.RsSpecial = new ReplacableString(_Aargs[0]);
										return;
									}
								}
							}
							else if (text == "TAN")
							{
								this.bfunc = EvalP.ListenEvalBlock.BFUNC.TAN;
								this.createEvalArgs(_Aargs);
								return;
							}
						}
						else if (num != 1047001532U)
						{
							if (num == 1561581017U)
							{
								if (text == "XD")
								{
									this.bfunc = EvalP.ListenEvalBlock.BFUNC.XD;
									this.RsSpecial = new ReplacableString(_Aargs[0]);
									return;
								}
							}
						}
						else if (text == "COS")
						{
							this.bfunc = EvalP.ListenEvalBlock.BFUNC.COS;
							this.createEvalArgs(_Aargs);
							return;
						}
					}
					else if (num <= 1820608667U)
					{
						if (num != 1661642285U)
						{
							if (num == 1820608667U)
							{
								if (text == "ABS")
								{
									this.bfunc = EvalP.ListenEvalBlock.BFUNC.ABS;
									this.createEvalArgs(_Aargs);
									return;
								}
							}
						}
						else if (text == "SIN")
						{
							this.bfunc = EvalP.ListenEvalBlock.BFUNC.SIN;
							this.createEvalArgs(_Aargs);
							return;
						}
					}
					else if (num != 2666459158U)
					{
						if (num == 3438387413U)
						{
							if (text == "GAR")
							{
								this.bfunc = EvalP.ListenEvalBlock.BFUNC.GAR;
								this.createEvalArgs(_Aargs);
								return;
							}
						}
					}
					else if (text == "RAND")
					{
						this.bfunc = EvalP.ListenEvalBlock.BFUNC.RAND;
						this.createEvalArgs(_Aargs);
						return;
					}
				}
				if (_Aargs != null && _Aargs.Count > 0)
				{
					this.Aargs = _Aargs.ToArray();
					int num2 = this.Aargs.Length;
					for (int i = 0; i < num2; i++)
					{
						if (this.Aargs[i].IndexOf('$') >= 0)
						{
							this.use_vp_bits |= 1U << i;
						}
					}
				}
			}

			private void createEvalArgs(List<string> _Aargs)
			{
				int count = _Aargs.Count;
				this.Abuilt_func = new EvalP[count];
				for (int i = 0; i < count; i++)
				{
					this.Abuilt_func[i] = new EvalP(null).Parse(_Aargs[i]);
				}
			}

			private float vBF(int i, VariableP VP)
			{
				return (float)this.Abuilt_func[i].getValue(VP);
			}

			public double getValue(VariableP VP)
			{
				if (this.bfunc != EvalP.ListenEvalBlock.BFUNC.NOUSE)
				{
					switch (this.bfunc)
					{
					case EvalP.ListenEvalBlock.BFUNC.SIN:
						return (double)X.Sin(this.vBF(0, VP));
					case EvalP.ListenEvalBlock.BFUNC.COS:
						return (double)X.Cos(this.vBF(0, VP));
					case EvalP.ListenEvalBlock.BFUNC.TAN:
						return (double)X.Tan(this.vBF(0, VP));
					case EvalP.ListenEvalBlock.BFUNC.GAR:
						return (double)X.GAR(this.vBF(0, VP), this.vBF(1, VP), this.vBF(2, VP), this.vBF(3, VP));
					case EvalP.ListenEvalBlock.BFUNC.RAND:
						return (double)X.xors((int)this.vBF(0, VP));
					case EvalP.ListenEvalBlock.BFUNC.ABS:
						return (double)X.Abs(this.vBF(0, VP));
					case EvalP.ListenEvalBlock.BFUNC.XD:
					case EvalP.ListenEvalBlock.BFUNC.YD:
					{
						AIM aim;
						if (!this.RsSpecial.use_vp)
						{
							aim = (AIM)CAim.parseString(this.RsSpecial.ToString(), 0);
						}
						else
						{
							STB stb = TX.PopBld(null, 0);
							this.RsSpecial.CopyBaked(VP, stb);
							aim = (AIM)CAim.parseString(stb, 0, 0);
							TX.ReleaseBld(stb);
						}
						return (double)((this.bfunc == EvalP.ListenEvalBlock.BFUNC.XD) ? CAim._XD(aim, 1) : CAim._YD(aim, 1));
					}
					}
				}
				string text = base.ToString(VP);
				double num;
				if (this.Aargs != null)
				{
					EvalP.Aargsbuf.Clear();
					EvalP.Aargsbuf.AddRange(this.Aargs);
					if (this.use_vp_bits != 0U)
					{
						STB stb2 = TX.PopBld(null, 0);
						for (int i = EvalP.Aargsbuf.Count - 1; i >= 0; i--)
						{
							if (((ulong)this.use_vp_bits & (ulong)(1L << (i & 31))) != 0UL)
							{
								stb2.Set(this.Aargs[i]);
								VP.Convert(stb2, 0, null);
								EvalP.Aargsbuf[i] = stb2.ToString();
							}
						}
						TX.ReleaseBld(stb2);
					}
					num = (TX.evalLsnConvert(text, EvalP.Aargsbuf) ? ((double)this.sign * TX.value_inputted) : 0.0);
				}
				else
				{
					num = (TX.evalLsnConvert(text, null) ? ((double)this.sign * TX.value_inputted) : 0.0);
				}
				return num;
			}

			public int sign;

			private string[] Aargs;

			private EvalP[] Abuilt_func;

			private uint use_vp_bits;

			private EvalP.ListenEvalBlock.BFUNC bfunc;

			private ReplacableString RsSpecial;

			private enum BFUNC : byte
			{
				NOUSE,
				SIN,
				COS,
				TAN,
				GAR,
				RAND,
				ABS,
				XD,
				YD
			}
		}

		public sealed class NotBlock : IValItem
		{
			public NotBlock(IValItem _Src)
			{
				this.Src = _Src;
			}

			public double getValue(VariableP VP)
			{
				return (double)((this.Src.getValue(VP) != 0.0) ? 1 : 0);
			}

			private IValItem Src;
		}

		public sealed class CompareBlock : IValItem
		{
			public CompareBlock(STB.MULT_OPE _ope)
			{
				this.ope = _ope;
			}

			public double getValue(VariableP VP)
			{
				return 0.0;
			}

			public STB.MULT_OPE ope;
		}

		public sealed class LogicalBlock : IValItem
		{
			public LogicalBlock(STB.MULT_OPE _ope, EvalP _Left)
			{
				this.ope = _ope;
				this.Left = _Left;
			}

			public double getValue(VariableP VP)
			{
				return this.Left.getValue(VP);
			}

			public STB.MULT_OPE ope;

			public EvalP Left;
		}

		public sealed class MultipleBlock : IValItem
		{
			public MultipleBlock(IValItem FirstItem, STB.MULT_OPE first_ope)
			{
				this.AItems = new List<IValItem>(4);
				this.Atypes = new List<int>(4);
				this.AItems.Add(FirstItem);
				this.Atypes.Add((int)first_ope);
			}

			public void addOpe(STB.MULT_OPE ope)
			{
				if (this.AItems.Count > this.Atypes.Count)
				{
					this.Atypes.Add((int)ope);
					return;
				}
				this.Atypes[this.Atypes.Count - 1] = (int)ope;
			}

			public bool addValItem(IValItem FirstItem)
			{
				if (this.AItems.Count <= this.Atypes.Count)
				{
					this.AItems.Add(FirstItem);
					return true;
				}
				return false;
			}

			public double getValue(VariableP VP)
			{
				int count = this.Atypes.Count;
				double num = ((this.AItems[0] != null) ? this.AItems[0].getValue(VP) : 1.0);
				for (int i = 1; i <= count; i++)
				{
					double num2 = ((this.AItems.Count > i) ? this.AItems[i].getValue(VP) : 1.0);
					switch (this.Atypes[i - 1])
					{
					case 2:
						num *= num2;
						break;
					case 3:
						num /= num2;
						break;
					case 4:
						num %= num2;
						break;
					case 5:
					{
						int num3 = (int)num2;
						if ((double)num3 == num2)
						{
							num = (double)X.Pow((float)num, num3);
						}
						else
						{
							num = (double)X.Pow((float)num, (float)num2);
						}
						break;
					}
					case 6:
						num = (uint)num | (uint)num2;
						break;
					case 7:
						num = (uint)num & (uint)num2;
						break;
					case 8:
						num = (uint)num << (int)num2;
						break;
					case 9:
						num = (uint)num >> (int)num2;
						break;
					}
				}
				return num;
			}

			private List<IValItem> AItems;

			private List<int> Atypes;
		}
	}
}
