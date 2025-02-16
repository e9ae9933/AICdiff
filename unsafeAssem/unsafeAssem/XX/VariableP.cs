using System;

namespace XX
{
	public class VariableP
	{
		public VariableP(int Capacity)
		{
			this.AValN = new VariableP.VariableItemNum[Capacity];
			this.AValS = new VariableP.VariableItemString[Capacity];
		}

		public VariableP Clear()
		{
			this.valn_max = (this.vals_max = 0);
			return this;
		}

		public VariableP Add(VariableP Src, bool src_release = false)
		{
			int num = Src.valn_max;
			if (this.valn_max + num >= this.AValN.Length)
			{
				Array.Resize<VariableP.VariableItemNum>(ref this.AValN, this.valn_max + num + 4);
			}
			for (int i = 0; i < num; i++)
			{
				VariableP.VariableItemNum variableItemNum = Src.AValN[i];
				this.Add(variableItemNum.Key, variableItemNum.num);
			}
			num = Src.vals_max;
			if (this.vals_max + num >= this.AValS.Length)
			{
				Array.Resize<VariableP.VariableItemString>(ref this.AValS, this.vals_max + num + 4);
			}
			for (int j = 0; j < num; j++)
			{
				VariableP.VariableItemString variableItemString = Src.AValS[j];
				this.AddStringItem(variableItemString);
			}
			if (src_release)
			{
				Src.Clear();
			}
			return this;
		}

		public VariableP Add(string key, double v)
		{
			VariableP.VariableItemNum variableItemNum = this.GetN(key);
			if (variableItemNum != null)
			{
				variableItemNum.num = v;
				return this;
			}
			if (this.valn_max >= this.AValN.Length)
			{
				Array.Resize<VariableP.VariableItemNum>(ref this.AValN, this.valn_max + 16);
			}
			variableItemNum = this.AValN[this.valn_max];
			if (variableItemNum == null)
			{
				variableItemNum = (this.AValN[this.valn_max] = new VariableP.VariableItemNum(key));
			}
			variableItemNum.SetKey(key).Set(v);
			this.valn_max++;
			return this;
		}

		public VariableP Add(STB Key, double v)
		{
			return this.Add(Key, v, 0, Key.Length);
		}

		public VariableP Add(STB Key, double v, int key_s, int key_e)
		{
			VariableP.VariableItemNum variableItemNum = this.GetN(Key, key_s, key_e);
			if (variableItemNum != null)
			{
				variableItemNum.num = v;
				return this;
			}
			if (this.valn_max >= this.AValN.Length)
			{
				Array.Resize<VariableP.VariableItemNum>(ref this.AValN, this.valn_max + 16);
			}
			variableItemNum = this.AValN[this.valn_max];
			if (variableItemNum == null)
			{
				variableItemNum = (this.AValN[this.valn_max] = new VariableP.VariableItemNum(Key));
			}
			variableItemNum.SetKey(Key).Set(v);
			this.valn_max++;
			return this;
		}

		public VariableP AddStringItem(string key, string v)
		{
			VariableP.VariableItemString variableItemString = this.GetS(key);
			if (variableItemString != null)
			{
				variableItemString.Set(v).Bake(this);
				return this;
			}
			if (this.vals_max >= this.AValS.Length)
			{
				Array.Resize<VariableP.VariableItemString>(ref this.AValS, this.vals_max + 16);
			}
			variableItemString = this.AValS[this.vals_max];
			if (variableItemString == null)
			{
				variableItemString = (this.AValS[this.vals_max] = new VariableP.VariableItemString(key));
			}
			else
			{
				variableItemString.SetKey(key);
			}
			variableItemString.Set(v).Bake(this);
			this.vals_max++;
			return this;
		}

		public VariableP AddStringItem(VariableP.VariableItemString V)
		{
			VariableP.VariableItemString variableItemString = this.GetS(V.Key, 0, V.Key.Length);
			if (variableItemString != null)
			{
				variableItemString.Set(V).Bake(this);
				return this;
			}
			if (this.vals_max >= this.AValS.Length)
			{
				Array.Resize<VariableP.VariableItemString>(ref this.AValS, this.vals_max + 16);
			}
			variableItemString = this.AValS[this.vals_max];
			if (variableItemString == null)
			{
				VariableP.VariableItemString variableItemString2 = (this.AValS[this.vals_max] = new VariableP.VariableItemString(V));
			}
			else
			{
				variableItemString.SetKey(V.Key).Set(V).Bake(this);
			}
			this.vals_max++;
			return this;
		}

		public VariableP AddStringItem(string key, STB v_Stb, int v_char_s, int v_char_e)
		{
			VariableP.VariableItemString variableItemString = this.GetS(key);
			if (variableItemString != null)
			{
				variableItemString.Set(v_Stb, v_char_s, v_char_e - v_char_s).Bake(this);
				return this;
			}
			if (this.vals_max >= this.AValS.Length)
			{
				Array.Resize<VariableP.VariableItemString>(ref this.AValS, this.vals_max + 16);
			}
			variableItemString = this.AValS[this.vals_max];
			if (variableItemString == null)
			{
				variableItemString = (this.AValS[this.vals_max] = new VariableP.VariableItemString(key));
			}
			else
			{
				variableItemString.SetKey(key);
			}
			variableItemString.Set(v_Stb, v_char_s, v_char_e - v_char_s).Bake(this);
			this.vals_max++;
			return this;
		}

		public VariableP AddStringItem(STB k_Stb, int k_char_s, int k_char_e, STB v_Stb, int v_char_s, int v_char_e)
		{
			VariableP.VariableItemString variableItemString = this.GetS(k_Stb, k_char_s, k_char_e);
			if (variableItemString != null)
			{
				variableItemString.Set(v_Stb, v_char_s, v_char_e - v_char_s).Bake(this);
				return this;
			}
			if (this.vals_max >= this.AValS.Length)
			{
				Array.Resize<VariableP.VariableItemString>(ref this.AValS, this.vals_max + 16);
			}
			variableItemString = this.AValS[this.vals_max];
			if (variableItemString == null)
			{
				variableItemString = (this.AValS[this.vals_max] = new VariableP.VariableItemString(k_Stb, k_char_s, k_char_e - k_char_s));
			}
			variableItemString.Set(v_Stb, v_char_s, v_char_e - v_char_s).Bake(this);
			this.vals_max++;
			return this;
		}

		public VariableP Add(string key, string s)
		{
			STB stb = TX.PopBld(s, 0);
			int num;
			if (!this.Add(key, stb, 0, out num, true))
			{
				this.AddStringItem(key, s);
			}
			TX.ReleaseBld(stb);
			return this;
		}

		public bool Add(string key, ReplacableString RVal, bool no_set_string_item = false)
		{
			int num;
			if (RVal.use_vp)
			{
				STB vpBaked = RVal.Bake(this).VpBaked;
				return this.Add(key, vpBaked, 0, out num, no_set_string_item);
			}
			STB stb = TX.PopBld(RVal.ToString(), 0);
			bool flag = this.Add(key, stb, 0, out num, no_set_string_item);
			TX.ReleaseBld(stb);
			return flag;
		}

		public bool Add(string key, STB Stb, int charindex, out int end_char_index, bool no_set_string_item = false)
		{
			if (TX.isNumDotPXMMatch(Stb[charindex]))
			{
				STB.PARSERES parseres = Stb.Nm(charindex, out end_char_index, -1, false);
				if (parseres != STB.PARSERES.ERROR)
				{
					if (parseres == STB.PARSERES.DOUBLE)
					{
						this.Add(key, STB.parse_result_double);
					}
					if (parseres == STB.PARSERES.INT)
					{
						this.Add(key, (double)STB.parse_result_int);
					}
					return true;
				}
			}
			if (!no_set_string_item)
			{
				Stb.Scroll(charindex, out end_char_index, (char c) => c != '\n', -1);
				if (end_char_index >= charindex)
				{
					this.AddStringItem(key, Stb, charindex, end_char_index);
				}
				return true;
			}
			end_char_index = charindex;
			return false;
		}

		public VariableP.VariableItemNum GetN(string key)
		{
			for (int i = 0; i < this.valn_max; i++)
			{
				VariableP.VariableItemNum variableItemNum = this.AValN[i];
				if (variableItemNum.Key.Equals(key))
				{
					return variableItemNum;
				}
			}
			return null;
		}

		public VariableP.VariableItemNum GetN(STB Stb, int char_s, int char_e)
		{
			for (int i = 0; i < this.valn_max; i++)
			{
				VariableP.VariableItemNum variableItemNum = this.AValN[i];
				if (Stb.Equals(char_s, char_e, variableItemNum.Key, 0, -1))
				{
					return variableItemNum;
				}
			}
			return null;
		}

		public VariableP.VariableItemString GetS(string key)
		{
			for (int i = 0; i < this.vals_max; i++)
			{
				VariableP.VariableItemString variableItemString = this.AValS[i];
				if (variableItemString.Key.Equals(key))
				{
					return variableItemString;
				}
			}
			return null;
		}

		public VariableP.VariableItemString GetS(STB Stb, int char_s = 0, int char_e = -1)
		{
			if (char_e < 0)
			{
				char_e = Stb.Length;
			}
			for (int i = 0; i < this.vals_max; i++)
			{
				VariableP.VariableItemString variableItemString = this.AValS[i];
				if (Stb.Equals(char_s, char_e, variableItemString.Key, 0, -1))
				{
					return variableItemString;
				}
			}
			return null;
		}

		public double getDouble(string key)
		{
			VariableP.VariableItemNum n = this.GetN(key);
			if (n != null)
			{
				return n.num;
			}
			VariableP.VariableItemString s = this.GetS(key);
			if (s != null)
			{
				STB stb = TX.PopBld(null, 0);
				s.CopyBaked(this, stb);
				int num;
				STB.PARSERES parseres = stb.Nm(0, out num, -1, false);
				double num2 = ((parseres == STB.PARSERES.DOUBLE) ? STB.parse_result_double : ((double)((parseres == STB.PARSERES.INT) ? STB.parse_result_int : 0)));
				TX.ReleaseBld(stb);
				return num2;
			}
			X.de("VariableP: 不明な double 値 " + key, null);
			VariableP.error_occured = true;
			return 0.0;
		}

		public bool isDefined(ReplacableString Rpl)
		{
			if (Rpl.use_vp)
			{
				return this.isDefined(Rpl.Bake(this).VpBaked, 0, -1);
			}
			return this.isDefined(Rpl.ToString());
		}

		public bool isDefined(STB Stb, int char_s = 0, int char_e = -1)
		{
			return this.GetS(Stb, char_s, char_e) != null || this.GetN(Stb, char_s, char_e) != null;
		}

		public bool isDefined(string _t)
		{
			return this.GetS(_t) != null || this.GetN(_t) != null;
		}

		public bool CopyString(STB StbKey, STB Dep)
		{
			VariableP.VariableItemString s = this.GetS(StbKey, 0, StbKey.Length);
			if (s != null)
			{
				Dep.Clear();
				s.CopyBaked(this, Dep);
				return true;
			}
			VariableP.VariableItemNum n = this.GetN(StbKey, 0, StbKey.Length);
			if (n != null)
			{
				Dep.Clear();
				Dep.Add((float)n.num);
				return true;
			}
			X.de("VariableP: 不明な string 値 " + StbKey.ToString(), null);
			VariableP.error_occured = true;
			Dep.Clear();
			return false;
		}

		public void Convert(STB Stb, int starti = 0, STB StbOut = null)
		{
			int num = Stb.Length - 1;
			int i = starti;
			if (StbOut != null)
			{
				StbOut.EnsureCapacity(num - starti + 1);
			}
			STB stb = TX.PopBld(null, 0);
			while (i < num)
			{
				char c2 = Stb[i];
				if (c2 == '$' && (i == 0 || Stb[i - 1] != '\\'))
				{
					int num2;
					if (Stb[i + 1] == '{')
					{
						Stb.ScrollBracket(i + 1, out num2, num + 1);
						stb.Clear().Add(Stb, i + 2, num2 - 1 - (i + 2));
					}
					else
					{
						Stb.Scroll(i + 1, out num2, (char c) => TX.isVariableNameMatch(c), num + 1);
						stb.Clear().Add(Stb, i + 1, num2 - (i + 1));
					}
					this.CopyString(stb, stb);
					if (StbOut != null)
					{
						StbOut += stb;
						i = num2;
					}
					else
					{
						int length = Stb.Length;
						Stb.Splice(i, num2 - i);
						Stb.Insert(i, stb);
						i += stb.Length;
						num += Stb.Length - length;
					}
				}
				else
				{
					i++;
					if (StbOut != null)
					{
						StbOut += c2;
					}
				}
			}
			TX.ReleaseBld(stb);
		}

		public bool hasDefinedVar()
		{
			return this.valn_max > 0 || this.vals_max > 9;
		}

		public static bool error_occured;

		private VariableP.VariableItemNum[] AValN;

		private int valn_max;

		private VariableP.VariableItemString[] AValS;

		private int vals_max;

		public class VariableItemNum
		{
			public VariableItemNum(string _key)
			{
				this.Key = new STB(_key);
			}

			public VariableItemNum(STB _Key)
			{
				this.Key = new STB(_Key, 0, _Key.Length, _Key.Length);
			}

			public VariableItemNum(VariableP.VariableItemNum Src)
			{
				this.Key = new STB(Src.Key, 0, Src.Key.Length, Src.Key.Length);
				this.num = Src.num;
			}

			public VariableP.VariableItemNum SetKey(string _key)
			{
				this.Key.Set(_key);
				return this;
			}

			public VariableP.VariableItemNum SetKey(STB _Key)
			{
				this.Key.Clear().Add(_Key, 0, _Key.Length);
				return this;
			}

			public VariableP.VariableItemNum Set(double _num)
			{
				this.num = _num;
				this.recache = true;
				return this;
			}

			public override string ToString()
			{
				return this.Rendered.ToString();
			}

			public ReplacableString Rendered
			{
				get
				{
					if (this.recache)
					{
						this.recache = false;
						if (this.Cache_ == null)
						{
							this.Cache_ = new ReplacableString();
						}
						this.Cache_.Set(this.num.ToString());
					}
					return this.Cache_;
				}
			}

			public void CopyBaked(STB Dep)
			{
				Dep += this.num;
			}

			public STB Key;

			public double num;

			private ReplacableString Cache_;

			private bool recache = true;
		}

		public class VariableItemString
		{
			public VariableItemString(string _key)
			{
				this.Key = new STB(_key);
			}

			public VariableItemString(VariableP.VariableItemString Src)
			{
				this.Key = new STB(Src.Key, 0, Src.Key.Length, Src.Key.Length);
				this.Val = new ReplacableString(Src.Val);
			}

			public VariableItemString(STB keyStb, int key_si, int key_len)
			{
				this.Key = new STB(key_len);
				this.Key.Add(keyStb, key_si, key_len);
			}

			public VariableP.VariableItemString SetKey(string _key)
			{
				this.Key.Set(_key);
				return this;
			}

			public VariableP.VariableItemString SetKey(STB _Key)
			{
				this.Key.Clear().Add(_Key, 0, _Key.Length);
				return this;
			}

			public VariableP.VariableItemString Set(string _s)
			{
				this.Val = ((this.Val == null) ? new ReplacableString(_s) : this.Val.Set(_s));
				return this;
			}

			public VariableP.VariableItemString Set(STB Stb, int key_si, int key_len)
			{
				this.Val = ((this.Val == null) ? new ReplacableString(Stb, key_si, key_len) : this.Val.Set(Stb, key_si, key_len));
				return this;
			}

			public VariableP.VariableItemString Set(VariableP.VariableItemString Src)
			{
				this.Val.Set(Src.Val);
				return this;
			}

			public VariableP.VariableItemString Bake(VariableP VP)
			{
				if (this.Val != null)
				{
					this.Val.Bake(VP);
				}
				return this;
			}

			public void CopyBaked(VariableP VP, STB Dep)
			{
				this.Val.CopyBaked(VP, Dep);
			}

			public STB Key;

			public ReplacableString Val;
		}
	}
}
