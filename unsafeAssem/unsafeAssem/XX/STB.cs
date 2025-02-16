using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XX
{
	public class STB : IDisposable
	{
		public static void InitSTB()
		{
			if (STB.Asplit_buf == null)
			{
				STB.Asplit_buf = new List<string>(8);
			}
		}

		public STB()
		{
			this.Stb = new StringBuilder();
		}

		public STB(int capacity)
		{
			this.Stb = new StringBuilder(capacity);
		}

		public STB(string value)
		{
			this.Stb = ((value == null) ? new StringBuilder(16) : new StringBuilder(value));
		}

		public STB(int capacity, int maxCapacity)
		{
			this.Stb = new StringBuilder(capacity, maxCapacity);
		}

		public STB(string value, int capacity)
		{
			this.Stb = new StringBuilder(value, capacity);
		}

		public STB(string value, int startIndex, int length, int capacity)
		{
			this.Stb = new StringBuilder(value, startIndex, length, capacity);
		}

		public STB(STB StbSrc, int startIndex, int length, int capacity)
		{
			this.Stb = new StringBuilder(capacity);
			this.Add(StbSrc, startIndex, length);
		}

		public StringBuilder getRawBuilder()
		{
			return this.Stb;
		}

		public char this[int index]
		{
			get
			{
				return this.Stb[index];
			}
			set
			{
				this.Stb[index] = value;
			}
		}

		public char[] copyToBuf(out int len)
		{
			len = this.Length;
			if (STB.Achar_buf == null || STB.Achar_buf.Length < this.Stb.Length)
			{
				STB.Achar_buf = new char[this.Stb.Length];
			}
			for (int i = 0; i < len; i++)
			{
				STB.Achar_buf[i] = this.Stb[i];
			}
			return STB.Achar_buf;
		}

		public int Capacity
		{
			get
			{
				return this.Stb.Capacity;
			}
			set
			{
				this.Stb.Capacity = value;
			}
		}

		public int Length
		{
			get
			{
				return this.Stb.Length;
			}
			set
			{
				this.Stb.Length = value;
			}
		}

		public int MaxCapacity
		{
			get
			{
				return this.Stb.MaxCapacity;
			}
		}

		public STB Append(string add, string delimiter = "\n")
		{
			if (this.Length > 0)
			{
				this.Stb.Append(delimiter);
			}
			this.Stb.Append(add);
			return this;
		}

		public STB Append(STB StbAdd, string delimiter = "\n", int s = 0, int length = -1)
		{
			if (this.Length > 0)
			{
				this.Stb.Append(delimiter);
			}
			if (length < 0)
			{
				length = StbAdd.Length - s;
			}
			this.Add(StbAdd, s, length);
			return this;
		}

		public STB Append2(string a, string b, string delimiter = "\n")
		{
			if (this.Length > 0)
			{
				this.Stb.Append(delimiter);
			}
			this.Stb.Append(a);
			this.Stb.Append(b);
			return this;
		}

		public STB Append3(string a, string b, string c, string delimiter = "\n")
		{
			if (this.Length > 0)
			{
				this.Stb.Append(delimiter);
			}
			this.Stb.Append(a);
			this.Stb.Append(b);
			this.Stb.Append(c);
			return this;
		}

		public STB Append4(string a, string b, string c, string d, string delimiter = "\n")
		{
			if (this.Length > 0)
			{
				this.Stb.Append(delimiter);
			}
			this.Stb.Append(a);
			this.Stb.Append(b);
			this.Stb.Append(c);
			this.Stb.Append(d);
			return this;
		}

		public STB Ret(string add = "\n")
		{
			this.Stb.Append(add);
			return this;
		}

		public STB Add(char value, int repeatCount)
		{
			this.Stb.Append(value, repeatCount);
			return this;
		}

		public STB AddSlice(string value, int s, int e = -1)
		{
			if (e < 0)
			{
				e = value.Length + 1 + e;
			}
			e = X.Mn(e, value.Length);
			this.EnsureCapacity(e - s);
			for (int i = s; i < e; i++)
			{
				this.Add(value[i]);
			}
			return this;
		}

		public STB Add(AIM a)
		{
			if (a >= AIM.L)
			{
				this.Add(CAim.parseInt(a));
			}
			else
			{
				this.Add((int)a);
			}
			return this;
		}

		public static STB operator +(STB Stb, bool value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, char value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, ulong value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, float value)
		{
			Stb.Add(value);
			return Stb;
		}

		public static STB operator +(STB Stb, uint value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, byte value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public STB Add(string value, int startIndex, int Length)
		{
			this.Stb.Append(value, startIndex, Length);
			return this;
		}

		public STB Add(STB Stb)
		{
			if (Stb == null)
			{
				return this;
			}
			return this.Add(Stb, 0, Stb.Length);
		}

		public STB Set(string value)
		{
			this.Clear();
			if (value != null)
			{
				this.Add(value);
			}
			return this;
		}

		public STB Set(STB Stb)
		{
			this.Clear();
			this.Add(Stb, 0, Stb.Length);
			return this;
		}

		public STB Set(int value)
		{
			this.Clear().Add(value);
			return this;
		}

		public STB Set(uint value)
		{
			this.Clear().Add(value);
			return this;
		}

		public STB Set(float value)
		{
			this.Clear().Add(value);
			return this;
		}

		public STB Set(double value)
		{
			this.Clear().Add(value);
			return this;
		}

		public STB Add(string value)
		{
			this.Stb.Append(value);
			return this;
		}

		public STB AR(string value)
		{
			this.Stb.Append(value);
			this.Stb.Append("\n");
			return this;
		}

		public STB AR(STB value)
		{
			this.Add(value);
			this.Stb.Append("\n");
			return this;
		}

		public STB AR(string v1, string v2)
		{
			this.Stb.Append(v1);
			this.Stb.Append(v2);
			this.Stb.Append("\n");
			return this;
		}

		public STB AR(string v1, string v2, string v3)
		{
			this.Stb.Append(v1);
			this.Stb.Append(v2);
			this.Stb.Append(v3);
			this.Stb.Append("\n");
			return this;
		}

		public STB AR(string v1, string v2, string v3, string v4)
		{
			this.Stb.Append(v1);
			this.Stb.Append(v2);
			this.Stb.Append(v3);
			this.Stb.Append(v4);
			this.Stb.Append("\n");
			return this;
		}

		public STB AR(string v1, string v2, string v3, string v4, string v5)
		{
			this.Stb.Append(v1);
			this.Stb.Append(v2);
			this.Stb.Append(v3);
			this.Stb.Append(v4);
			this.Stb.Append(v5);
			this.Stb.Append("\n");
			return this;
		}

		public STB ARd(string v1, string delimiter = " ", string v2 = null, string v3 = null, string v4 = null, string v5 = null)
		{
			this.Stb.Append(v1);
			if (v2 != null)
			{
				this.Stb.Append(delimiter);
				this.Stb.Append(v2);
			}
			if (v3 != null)
			{
				this.Stb.Append(delimiter);
				this.Stb.Append(v3);
			}
			if (v4 != null)
			{
				this.Stb.Append(delimiter);
				this.Stb.Append(v4);
			}
			if (v5 != null)
			{
				this.Stb.Append(delimiter);
				this.Stb.Append(v5);
			}
			this.Stb.Append("\n");
			return this;
		}

		public STB Add(string value, int v, string suffix = "")
		{
			this.Add(value);
			this.Add(v);
			this.Add(suffix);
			return this;
		}

		public STB Add(string value, float v, string suffix = "")
		{
			this.Add(value);
			this.Add(v);
			this.Add(suffix);
			return this;
		}

		public STB AddLw(string a, string b, string c)
		{
			return this.AddLw(a).AddLw(b).AddLw(c);
		}

		public STB AddLw(string value)
		{
			int length = value.Length;
			for (int i = 0; i < length; i++)
			{
				this.Add(char.ToLower(value[i]));
			}
			return this;
		}

		public STB AddUp(string value)
		{
			int length = value.Length;
			for (int i = 0; i < length; i++)
			{
				this.Add(char.ToUpper(value[i]));
			}
			return this;
		}

		public STB Add(uint value)
		{
			if (value == 0U)
			{
				this.Stb.Append('0');
			}
			else
			{
				int num = 10;
				int i = 1;
				while ((long)num <= (long)((ulong)value))
				{
					num *= 10;
					i++;
				}
				this.EnsureCapacity(i);
				int length = this.Length;
				this.Length += i;
				while (i > 0)
				{
					uint num2 = value / 10U;
					uint num3 = value - num2 * 10U;
					this.Stb[length + --i] = (char)(48U + num3);
					value = num2;
				}
			}
			return this;
		}

		public STB Add0(int value, int keta = 0, char append_char = '0')
		{
			int length = this.Length;
			this.Add(value);
			int num = keta - (this.Length - length);
			while (--num >= 0)
			{
				this.Stb.Insert(length, append_char);
			}
			return this;
		}

		public STB Add(int value)
		{
			if (value == 0)
			{
				this.Stb.Append('0');
			}
			else
			{
				if (value < 0)
				{
					this.Stb.Append('-');
					value = -value;
				}
				int i = 10;
				int j = 1;
				while (i <= value)
				{
					i *= 10;
					j++;
				}
				this.EnsureCapacity(j);
				int length = this.Length;
				this.Length += j;
				while (j > 0)
				{
					int num = value / 10;
					int num2 = value - num * 10;
					this.Stb[length + --j] = (char)(48 + num2);
					value = num;
				}
			}
			return this;
		}

		public STB Add(float value)
		{
			return this.Add((double)value);
		}

		public STB Add(double value)
		{
			if (value == 0.0)
			{
				this.Stb.Append('0');
			}
			else
			{
				if (value < 0.0)
				{
					this.Stb.Append('-');
					value = -value;
				}
				int num = (int)value;
				this.Add(num);
				if ((double)num == value)
				{
					return this;
				}
				this.Stb.Append('.');
				value -= (double)num;
				double num2 = 9.999999960041972E-12;
				while (value > num2)
				{
					value *= 10.0;
					num2 *= 10.0;
					num = (int)value;
					this.Stb.Append((char)(48 + num));
					value -= (double)num;
				}
			}
			return this;
		}

		public STB AddHex(uint value, int spr0 = 0)
		{
			if (value == 0U)
			{
				spr0 = X.Mx(spr0, 1);
				this.EnsureCapacity(spr0);
				while (--spr0 >= 0)
				{
					this.Add('0');
				}
			}
			else
			{
				uint num = 16U;
				int i = 1;
				while (num <= value && i < 8)
				{
					num *= 16U;
					i++;
				}
				if (spr0 > 0 && spr0 > i)
				{
					this.EnsureCapacity(spr0);
					spr0 -= i;
					while (--spr0 >= 0)
					{
						this.Add('0');
					}
				}
				else
				{
					this.EnsureCapacity(i);
				}
				int length = this.Length;
				this.Length += i;
				while (i > 0)
				{
					uint num2 = value >> 4;
					uint num3 = value - num2 * 16U;
					this.Stb[length + --i] = ((num3 >= 10U) ? ((char)(97U + (num3 - 10U))) : ((char)(48U + num3)));
					value = num2;
				}
			}
			return this;
		}

		public STB Add(Color32 value)
		{
			this.EnsureCapacity(8);
			this.AddHex((uint)value.a, 2);
			this.AddHex((uint)value.r, 2);
			this.AddHex((uint)value.g, 2);
			this.AddHex((uint)value.b, 2);
			return this;
		}

		public STB AddCol(uint value, string header = "0x")
		{
			this.EnsureCapacity(header.Length + 8);
			this.Add(header);
			this.AddHex((value >> 24) & 255U, 2);
			this.AddHex((value >> 16) & 255U, 2);
			this.AddHex((value >> 8) & 255U, 2);
			this.AddHex(value & 255U, 2);
			return this;
		}

		public STB Add(char value)
		{
			this.Stb.Append(value);
			return this;
		}

		public STB AddPath(string s, float replace_alpha_tag = 0.5f)
		{
			this.EnsureCapacity(s.Length);
			using (STB stb = TX.PopBld(s, 0))
			{
				try
				{
					string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					if (TX.valid(folderPath))
					{
						using (STB stb2 = TX.PopBld(null, 0))
						{
							if (replace_alpha_tag < 1f)
							{
								stb2.Add("<font alpha=\"").Add(replace_alpha_tag).Add("\">");
							}
							stb2.Add("＜UserProfile＞");
							if (replace_alpha_tag < 1f)
							{
								stb2.Add("</font>");
							}
							stb.Replace(folderPath, stb2, 0, -1);
						}
					}
				}
				catch
				{
				}
				this.Add(stb);
			}
			return this;
		}

		public STB AddIconHtml(string icon, uint color, bool tx_color = false)
		{
			this.Add("<img mesh=\"", icon, "\" width=\"26\" ");
			if (tx_color)
			{
				this.Add("txcolor");
			}
			else
			{
				this.Add("color=\"").AddCol(color, "0x").Add("\"");
			}
			return this.Add("/>");
		}

		public static int Len(string a)
		{
			if (a == null)
			{
				return 0;
			}
			return a.Length;
		}

		public STB Add(string a, string b)
		{
			this.EnsureCapacity(STB.Len(a) + STB.Len(b));
			this.Stb.Append(a ?? "").Append(b ?? "");
			return this;
		}

		public STB Add(string a, string b, string c)
		{
			this.EnsureCapacity(STB.Len(a) + STB.Len(b) + STB.Len(c));
			this.Stb.Append(a ?? "").Append(b ?? "").Append(c ?? "");
			return this;
		}

		public STB Add(string a, string b, string c, string d)
		{
			this.EnsureCapacity(STB.Len(a) + STB.Len(b) + STB.Len(c) + STB.Len(d));
			this.Stb.Append(a ?? "").Append(b ?? "").Append(c ?? "")
				.Append(d ?? "");
			return this;
		}

		public STB Add(string a, string b, string c, string d, string e)
		{
			this.EnsureCapacity(STB.Len(a) + STB.Len(b) + STB.Len(c) + STB.Len(d) + STB.Len(e));
			this.Stb.Append(a ?? "").Append(b ?? "").Append(c ?? "")
				.Append(d ?? "")
				.Append(e ?? "");
			return this;
		}

		public STB addSliceJoin(META Meta, string meta_key, int first_index = 0, string delimiter = " ")
		{
			string[] array = Meta.Get(meta_key);
			return this.addSliceJoin(array, first_index, delimiter);
		}

		public STB addSliceJoin(string[] As, int first_index = 0, string delimiter = " ")
		{
			if (As != null)
			{
				int num = As.Length;
				for (int i = first_index; i < num; i++)
				{
					if (i > first_index)
					{
						this.Add(delimiter);
					}
					this.Add(As[i]);
				}
			}
			return this;
		}

		public STB Add(DateTime Date)
		{
			if (X.ENG_MODE)
			{
				this.Add0(Date.Day, 2, '0');
				this.Add(' ');
				switch (Date.Month)
				{
				case 1:
					this.Add("Jan ");
					break;
				case 2:
					this.Add("Feb ");
					break;
				case 3:
					this.Add("Mar ");
					break;
				case 4:
					this.Add("Apr ");
					break;
				case 5:
					this.Add("May ");
					break;
				case 6:
					this.Add("Jun ");
					break;
				case 7:
					this.Add("Jul ");
					break;
				case 8:
					this.Add("Aug ");
					break;
				case 9:
					this.Add("Sep ");
					break;
				case 10:
					this.Add("Oct ");
					break;
				case 11:
					this.Add("Nov ");
					break;
				case 12:
					this.Add("Dec ");
					break;
				}
				this.Add0(Date.Year, 4, '0');
				this.Add(' ');
				this.Add0(Date.Hour, 2, '0');
				this.Add(':');
				this.Add0(Date.Minute, 2, '0');
				this.Add(':');
				this.Add0(Date.Second, 2, '0');
			}
			else
			{
				this.AddDetailed(Date, false);
			}
			return this;
		}

		public STB AddDetailed(DateTime Date, bool add_millisec = true)
		{
			this.Add0(Date.Year % 100, 2, '0');
			this.Add('/');
			this.Add0(Date.Month % 100, 2, '0');
			this.Add('/');
			this.Add0(Date.Day, 2, '0');
			this.Add(' ');
			this.Add0(Date.Hour, 2, '0');
			this.Add(':');
			this.Add0(Date.Minute, 2, '0');
			this.Add(':');
			this.Add0(Date.Second, 2, '0');
			if (add_millisec)
			{
				this.Add('.');
				this.Add0(Date.Millisecond % 1000, 3, '0');
			}
			return this;
		}

		public STB AddEncode(string d, string k = "utf-8")
		{
			if (d == null)
			{
				return this;
			}
			if (this.Abuffer_encode == null || this.Abuffer_encode.Length < d.Length * 8 + 10)
			{
				this.Abuffer_encode = new byte[d.Length * 8 + 10];
			}
			int bytes = Encoding.UTF8.GetBytes(d, 0, d.Length, this.Abuffer_encode, 0);
			this.EnsureCapacity(bytes);
			for (int i = 0; i < bytes; i++)
			{
				this.Add((char)this.Abuffer_encode[i]);
			}
			return this;
		}

		public STB AddEncode(STB Stb, string k = "utf-8")
		{
			if (Stb == null || Stb.Length == 0)
			{
				return this;
			}
			if (this.Abuffer_encode == null || this.Abuffer_encode.Length < Stb.Length * 8 + 10)
			{
				this.Abuffer_encode = new byte[Stb.Length * 8 + 10];
			}
			int num;
			char[] array = Stb.copyToBuf(out num);
			int bytes = Encoding.UTF8.GetBytes(array, 0, num, this.Abuffer_encode, 0);
			this.EnsureCapacity(bytes);
			for (int i = 0; i < bytes; i++)
			{
				this.Add((char)this.Abuffer_encode[i]);
			}
			return this;
		}

		public STB AddJoin(StringHolder ER, string delimiter = " ", int start_index = 1)
		{
			int clength = ER.clength;
			for (int i = start_index; i < clength; i++)
			{
				if (i > start_index)
				{
					this.Add(delimiter);
				}
				this.Add(ER.getIndex(i));
			}
			return this;
		}

		public STB AddJoin(string[] A, string delimiter = " ", int start_index = 0)
		{
			int num = A.Length;
			for (int i = start_index; i < num; i++)
			{
				if (i > start_index)
				{
					this.Add(delimiter);
				}
				this.Add(A[i]);
			}
			return this;
		}

		public STB TrimCapacity()
		{
			if (this.Capacity > this.Length)
			{
				this.Capacity = this.Length;
			}
			return this;
		}

		public static STB operator +(STB Stb, ushort value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, STB value)
		{
			Stb.Add(value, 0, value.Length);
			return Stb;
		}

		public static STB operator +(STB Stb, char[] value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public STB Add(char[] value, int startIndex, int charCount)
		{
			this.Stb.Append(value);
			return this;
		}

		public STB Add(STB Stb, int startIndex, int charCount)
		{
			this.EnsureCapacity(charCount);
			for (int i = 0; i < charCount; i++)
			{
				try
				{
					this.Add(Stb[startIndex++]);
				}
				catch
				{
					Debug.Log("overrun");
					break;
				}
			}
			return this;
		}

		public STB spr0(int x, int keta, char suppresschar = '0')
		{
			int length = this.Length;
			this.Add(x);
			int num = this.Stb.Length - length;
			this.unshift(length, suppresschar, keta - num);
			return this;
		}

		public STB spr_after(float _val, int kurai)
		{
			if (kurai <= 0)
			{
				return this.Add((int)_val);
			}
			if (_val == 0f)
			{
				this.Add("0.");
				for (int i = 0; i < kurai; i++)
				{
					this.Add("0");
				}
				return this;
			}
			if (_val < 0f)
			{
				this.Add('-');
				_val *= -1f;
			}
			int num = (int)_val;
			this.EnsureCapacity(kurai + 1 + X.keta_count(num));
			this.Add(num).Add('.');
			_val -= (float)num;
			while (--kurai >= 0)
			{
				_val *= 10f;
				num = (int)_val;
				this.Add(num);
				_val -= (float)num;
			}
			return this;
		}

		public STB AppendTxA(string tx_key, string delimiter = "\n")
		{
			if (this.Length != 0)
			{
				this.Add(delimiter);
			}
			return this.AddTxA(tx_key, false);
		}

		public STB SetTxA(string tx_key, bool error_input_raw = false)
		{
			return this.Clear().AddTxA(tx_key, error_input_raw);
		}

		public STB AddTxA(string tx_key, bool error_input_raw = false)
		{
			TX tx = TX.getTX(tx_key, true, error_input_raw, null);
			if (tx == null)
			{
				if (error_input_raw)
				{
					this.Add(tx_key);
				}
				this.txa_first = -1;
				return this;
			}
			this.prepareTxReplace(this.Length, 1);
			this.Add(tx.text);
			return this;
		}

		public void prepareTxReplace(int start_char0, byte start_id = 1)
		{
			this.txa_first = start_char0;
			this.txa_id = start_id;
		}

		public STB TxRpl(STB StO)
		{
			if (this.txa_first < 0)
			{
				return this;
			}
			int i = this.txa_first;
			while (i < this.Length)
			{
				if (this.Stb[i] == '&')
				{
					int num;
					this.Scroll(i + 1, out num, (char _c) => TX.isNumMatch(_c), this.Length);
					if (num > i + 1)
					{
						if (this.NmI(i + 1, out num, num, -1) == (int)this.txa_id)
						{
							this.Stb.Remove(i, num - i);
							this.Insert(i, StO);
							i += StO.Length;
						}
						else
						{
							i = num;
						}
					}
					else
					{
						i = num;
					}
				}
				else
				{
					i++;
				}
			}
			this.txa_id += 1;
			return this;
		}

		public STB TxRpl(string tx_key)
		{
			STB stb = TX.PopBld(tx_key, 0);
			this.TxRpl(stb);
			TX.ReleaseBld(stb);
			return this;
		}

		public STB TxRpl(string tx_key, float num_ket, string tx_key3 = "")
		{
			STB stb = TX.PopBld(tx_key, 0).Add(num_ket).Add(tx_key3);
			this.TxRpl(stb);
			TX.ReleaseBld(stb);
			return this;
		}

		public STB TxRpl(float _t)
		{
			STB stb = TX.PopBld(null, 0);
			stb.Add(_t);
			this.TxRpl(stb);
			TX.ReleaseBld(stb);
			return this;
		}

		public STB TxRpl(int _t)
		{
			STB stb = TX.PopBld(null, 0);
			stb.Add(_t);
			this.TxRpl(stb);
			TX.ReleaseBld(stb);
			return this;
		}

		public STB AddColor(uint c)
		{
			this.Add("0x");
			return this.AddHex(c, 8);
		}

		public STB AddColor(Color32 c)
		{
			return this.AddColor(C32.c2d(c));
		}

		public static STB operator +(STB Stb, sbyte value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, decimal value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public unsafe STB Add(char* value, int valueCount)
		{
			this.Stb.Append(value, valueCount);
			return this;
		}

		public static STB operator +(STB Stb, short value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, int value)
		{
			Stb.Add(value);
			return Stb;
		}

		public static STB operator +(STB Stb, long value)
		{
			Stb.Add((float)value);
			return Stb;
		}

		public static STB operator +(STB Stb, double value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, string value)
		{
			Stb.Stb.Append(value);
			return Stb;
		}

		public static STB operator +(STB Stb, Color32 value)
		{
			Stb.Add(value);
			return Stb;
		}

		public STB AddLine()
		{
			this.Stb.AppendLine();
			return this;
		}

		public STB AddLine(string value)
		{
			this.Stb.AppendLine(value);
			return this;
		}

		public STB Clear()
		{
			this.Stb.Clear();
			return this;
		}

		public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			this.Stb.CopyTo(sourceIndex, destination, destinationIndex, count);
		}

		public int EnsureCapacity(int capacity)
		{
			return this.Stb.EnsureCapacity(X.Mx(0, capacity));
		}

		public bool Equals(STB sb)
		{
			return this.Equals(0, -1, sb, 0, -1);
		}

		public STB Insert(int index, char[] value, int startIndex, int charCount)
		{
			this.Stb.Insert(index, value, startIndex, charCount);
			return this;
		}

		public STB Insert(int index, bool value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, byte value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, ulong value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, char[] value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, ushort value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, string value, int count = 1)
		{
			this.Stb.Insert(index, value, count);
			return this;
		}

		public STB Insert(int index, char value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, uint value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, sbyte value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, STB Stb)
		{
			this.EnsureCapacity(Stb.Length);
			int length = Stb.Length;
			for (int i = 0; i < length; i++)
			{
				this.Stb.Insert(index++, Stb[i]);
			}
			return this;
		}

		public STB unshift(int index, char c, int count = 1)
		{
			if (count <= 0)
			{
				return this;
			}
			this.EnsureCapacity(count);
			this.Length += count;
			for (int i = this.Length - 1; i >= index + count; i--)
			{
				this.Stb[i] = this.Stb[i - count];
			}
			for (int j = 0; j < count; j++)
			{
				this.Stb[index++] = c;
			}
			return this;
		}

		public STB unshift(int index, string s)
		{
			if (TX.noe(s))
			{
				return this;
			}
			int length = s.Length;
			this.EnsureCapacity(length);
			this.Length += length;
			for (int i = this.Length - 1; i >= index + length; i--)
			{
				this.Stb[i] = this.Stb[i - length];
			}
			for (int j = 0; j < length; j++)
			{
				this.Stb[index++] = s[j];
			}
			return this;
		}

		public STB Overwrite(int index, string s)
		{
			if (TX.noe(s))
			{
				return this;
			}
			if (index + s.Length > this.Length)
			{
				int num = index + s.Length - this.Length;
				this.EnsureCapacity(num);
				this.Length += num;
			}
			int length = s.Length;
			for (int i = 0; i < length; i++)
			{
				this.Stb[index++] = s[i];
			}
			return this;
		}

		public STB Insert(int index, long value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, int value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, short value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, double value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, decimal value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, float value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Insert(int index, string value)
		{
			this.Stb.Insert(index, value);
			return this;
		}

		public STB Replace(char s, char e, int start = 0, int end = -1)
		{
			if (end < 0)
			{
				end = this.Length;
			}
			this.Stb.Replace(s, e, start, end - start);
			return this;
		}

		public STB Replace(string src, STB StbDest, int start = 0, int end = -1)
		{
			bool flag;
			return this.Replace(src, StbDest, out flag, 0, -1);
		}

		public STB Replace(string src, STB StbDest, out bool replaced, int start = 0, int end = -1)
		{
			if (end < 0)
			{
				end = this.Length;
			}
			return this.Replace(src, StbDest, out replaced, start, ref end);
		}

		public STB Replace(string src, STB StbDest, out bool replaced, int start, ref int end)
		{
			if (end < 0)
			{
				end = this.Length;
			}
			int num = X.Mn(end, this.Length - (src.Length - 1));
			int i = start;
			replaced = false;
			int num2 = -src.Length + StbDest.Length;
			while (i < num)
			{
				if (this.isStart(src, i))
				{
					replaced = true;
					this.Stb.Remove(i, src.Length);
					this.EnsureCapacity(X.Mx(StbDest.Length - src.Length, 0));
					for (int j = StbDest.Length - 1; j >= 0; j--)
					{
						this.Stb.Append('\0');
					}
					for (int k = this.Stb.Length - StbDest.Length - 1; k >= i; k--)
					{
						this.Stb[k + StbDest.Length] = this.Stb[k];
					}
					for (int l = 0; l < StbDest.Length; l++)
					{
						this.Stb[i++] = StbDest[l];
					}
					end += num2;
					num += num2;
				}
				else
				{
					i++;
				}
			}
			return this;
		}

		public STB RemoveChar(char _c, int start = 0, int end = -1)
		{
			if (end < 0)
			{
				end = this.Length;
			}
			for (int i = end - 1; i >= 0; i--)
			{
				if (this[i] == _c)
				{
					this.Stb.Remove(i, 1);
				}
			}
			return this;
		}

		public bool Equals(string compare_to)
		{
			return this.Equals(0, this.Length, compare_to, false);
		}

		public bool Equals(IVariableObject P)
		{
			return this.Equals(0, this.Length, P.ToString(), false);
		}

		public bool Equals(int my_first, int my_e = -1, string compare_to = null, bool compare_lower = false)
		{
			if (compare_to == null)
			{
				return this.Length == 0;
			}
			if (my_e <= 0)
			{
				my_e = this.Length;
			}
			else
			{
				my_e = X.Mn(my_e, this.Length);
			}
			if (my_e - my_first != compare_to.Length)
			{
				return false;
			}
			int num = 0;
			for (int i = my_first; i < my_e; i++)
			{
				char c = compare_to[num++];
				char c2 = this.Stb[i];
				if (compare_lower)
				{
					char c3 = char.ToLower(c);
					char c4 = char.ToLower(c2);
					c = c3;
					c2 = c4;
				}
				if (c != c2)
				{
					return false;
				}
			}
			return true;
		}

		public bool Equals(int my_first, int my_e, STB CompareToStb, int cstb_first = 0, int cstb_e = -1)
		{
			if (CompareToStb == null)
			{
				return this.Length == 0;
			}
			if (my_e < 0)
			{
				my_e = this.Length;
			}
			else
			{
				my_e = X.Mn(my_e, this.Length);
			}
			if (cstb_e < 0)
			{
				cstb_e = CompareToStb.Length;
			}
			else
			{
				cstb_e = X.Mn(cstb_e, CompareToStb.Length);
			}
			if (my_e - my_first != cstb_e - cstb_first)
			{
				return false;
			}
			int num = cstb_first;
			for (int i = my_first; i < my_e; i++)
			{
				if (CompareToStb[num++] != this.Stb[i])
				{
					return false;
				}
			}
			return true;
		}

		public STB Clip(int first, int end = 0)
		{
			if (end <= 0)
			{
				end = this.Length + end;
			}
			if (first > 0)
			{
				for (int i = first; i < end; i++)
				{
					char c = this.Stb[i];
					this.Stb[i - first] = c;
				}
			}
			this.Length = end - first;
			return this;
		}

		public STB Splice(int s, int length = -1)
		{
			if (length < 0)
			{
				length = this.Length - s;
			}
			else
			{
				length = X.Mn(length, this.Length - s);
			}
			this.Stb.Remove(s, length);
			return this;
		}

		public bool Is(char c, int index)
		{
			return this.Length > index && this.Stb[index] == c;
		}

		public bool IsL(char c, int index)
		{
			return this.Length > index && char.ToLower(this.Stb[index]) == c;
		}

		public bool IsU(char c, int index)
		{
			return this.Length > index && char.ToUpper(this.Stb[index]) == c;
		}

		public bool isStart(char c)
		{
			return this.Length > 0 && this.Stb[0] == c;
		}

		public bool isStart(string s, int index = 0)
		{
			if (this.Length < s.Length + index)
			{
				return false;
			}
			int length = s.Length;
			for (int i = 0; i < length; i++)
			{
				if (this.Stb[index++] != s[i])
				{
					return false;
				}
			}
			return true;
		}

		public int IndexOf(char c, int startfrom = 0, int end = -1)
		{
			if (end < 0)
			{
				end = this.Length;
			}
			for (int i = startfrom; i < end; i++)
			{
				if (this.Stb[i] == c)
				{
					return i;
				}
			}
			return -1;
		}

		public int IndexOf(string s, int startfrom = 0, int end = -1)
		{
			if (end < 0)
			{
				end = this.Length;
			}
			int length = s.Length;
			end -= length - 1;
			for (int i = startfrom; i < end; i++)
			{
				for (int j = 0; j <= length; j++)
				{
					if (j == length)
					{
						return i;
					}
					if (this.Stb[i + j] != s[j])
					{
						break;
					}
				}
			}
			return -1;
		}

		public int IndexOf(STB S, int startfrom = 0, int end = -1)
		{
			if (end < 0)
			{
				end = this.Length;
			}
			int length = S.Length;
			end -= length - 1;
			for (int i = startfrom; i < end; i++)
			{
				for (int j = 0; j <= length; j++)
				{
					if (j == length)
					{
						return i;
					}
					char c = this.Stb[i + j];
					char c2 = S[j];
				}
			}
			return -1;
		}

		public bool charIs(int i, char pchr)
		{
			return i < this.Length && this[i] == pchr;
		}

		public STB ToUpper()
		{
			int length = this.Length;
			for (int i = 0; i < length; i++)
			{
				this.Stb[i] = char.ToUpper(this.Stb[i]);
			}
			return this;
		}

		public STB ToLower()
		{
			int length = this.Length;
			for (int i = 0; i < length; i++)
			{
				this.Stb[i] = char.ToLower(this.Stb[i]);
			}
			return this;
		}

		public STB ToUnityCase()
		{
			string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			bool flag = true;
			for (int i = 0; i < this.Length; i++)
			{
				if (text.IndexOf(this.Stb[i]) == -1)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return this;
			}
			STB stb = TX.PopBld(null, 0);
			stb.EnsureCapacity(this.Length);
			string text2 = "\n\r\t_";
			int num = 0;
			for (int i = 0; i < this.Length; i++)
			{
				char c = this.Stb[i];
				if (text2.IndexOf(c) >= 0)
				{
					if (num > 0)
					{
						stb += ' ';
					}
					num = 0;
				}
				else
				{
					if (text.IndexOf(c) >= 0 && num > 0)
					{
						stb += ' ';
					}
					if (num == 0)
					{
						stb += char.ToUpper(c);
					}
					else
					{
						stb += c;
					}
					num++;
				}
			}
			this.Set(stb);
			TX.ReleaseBld(stb);
			return this;
		}

		public string[] splitReturn()
		{
			STB.Asplit_buf.Clear();
			int num;
			for (int i = 0; i < this.Length; i = num + 1)
			{
				this.Scroll(i, out num, (char _c) => _c != '\n' && _c != '\r', -1);
				STB.Asplit_buf.Add(this.get_slice(i, num));
			}
			if (STB.Asplit_buf.Count == 0)
			{
				STB.Asplit_buf.Add("");
			}
			return STB.Asplit_buf.ToArray();
		}

		public List<int> splitIndex(string separator, List<int> Ast = null)
		{
			Ast = Ast ?? new List<int>(4);
			Ast.Add(0);
			if (!TX.noe(separator))
			{
				int num = 0;
				for (;;)
				{
					num = this.IndexOf(separator, num, -1);
					if (num < 0)
					{
						break;
					}
					num += separator.Length;
					Ast.Add(num);
				}
			}
			return Ast;
		}

		public bool splitByIndex(int i, List<int> Aindex, string separator, out string cutted)
		{
			if (i >= Aindex.Count)
			{
				cutted = null;
				return false;
			}
			int num = Aindex[i];
			if (i == Aindex.Count - 1)
			{
				cutted = this.ToString(num, this.Length - num);
			}
			else
			{
				cutted = this.ToString(num, Aindex[i + 1] - separator.Length - num);
			}
			return true;
		}

		public bool splitByIndex(int i, List<int> Aindex, string separator, out double cutted, double _def = -1.0)
		{
			if (i >= Aindex.Count)
			{
				cutted = _def;
				return false;
			}
			int num = Aindex[i];
			if (i == Aindex.Count - 1)
			{
				int num2;
				cutted = STB.NmRes(this.Nm(num, out num2, this.Length, false), -1.0);
			}
			else
			{
				int num3;
				cutted = STB.NmRes(this.Nm(num, out num3, Aindex[i + 1] - separator.Length, false), -1.0);
			}
			return true;
		}

		public bool splitByIndex(int i, List<int> Aindex, string separator, out int cutted, int _def = -1)
		{
			if (i >= Aindex.Count)
			{
				cutted = _def;
				return false;
			}
			int num = Aindex[i];
			STB.PARSERES parseres;
			if (i == Aindex.Count - 1)
			{
				int num2;
				parseres = this.Nm(num, out num2, this.Length, false);
			}
			else
			{
				int num3;
				parseres = this.Nm(num, out num3, Aindex[i + 1] - separator.Length, false);
			}
			if (parseres == STB.PARSERES.INT)
			{
				cutted = STB.parse_result_int;
			}
			else
			{
				cutted = (int)STB.NmRes(parseres, (double)_def);
			}
			return true;
		}

		public bool splitByIndex(int i, List<int> Aindex, string separator, STB Out)
		{
			Out.Clear();
			if (i >= Aindex.Count)
			{
				return false;
			}
			int num = Aindex[i];
			if (i == Aindex.Count - 1)
			{
				Out.Add(this, num, this.Length - num);
			}
			else
			{
				Out.Add(this, num, Aindex[i + 1] - separator.Length - num);
			}
			return true;
		}

		public bool Nm()
		{
			int num;
			return this.Nm(0, out num, -1, false) > STB.PARSERES.ERROR;
		}

		public bool Nm(out STB.PARSERES res)
		{
			int num;
			res = this.Nm(0, out num, -1, false);
			return res > STB.PARSERES.ERROR;
		}

		public STB.PARSERES Nm(int s, out int end_char_index, int e = -1, bool force_hex = false)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			int num = 0;
			int num2 = 0;
			uint num3 = 0U;
			double num4 = 0.0;
			double num5 = 1.0;
			int num6 = 1;
			bool flag = false;
			bool flag2 = false;
			int num7 = e - s;
			bool flag3 = false;
			if (num7 <= 0)
			{
				end_char_index = s + 1;
				return STB.PARSERES.ERROR;
			}
			Func<char, bool> func;
			if (num7 > 4 && this.Stb[s + 2] == ':' && this.Stb[s + 3] == '#')
			{
				flag = true;
				func = (char c) => TX.isNumXMatch(c);
			}
			else if (num7 > 2 && this.Stb[s] == '0' && this.Stb[s + 1] == 'x')
			{
				flag = true;
				func = (char c) => TX.isNumXMatch(c);
				s += 2;
				num7 -= 2;
			}
			else if (this.Stb[s] == '#')
			{
				flag = true;
				func = (char c) => TX.isNumXMatch(c);
				flag3 = true;
				s++;
				num7--;
			}
			else
			{
				if (force_hex)
				{
					flag = true;
					func = (char c) => TX.isNumXMatch(c);
				}
				else
				{
					func = (char c) => TX.isNumMatch(c);
				}
				while (s < e)
				{
					if (this.Stb[s] == '-')
					{
						num6 *= -1;
						s++;
						num7--;
					}
					else
					{
						if (this.Stb[s] != '+')
						{
							break;
						}
						s++;
						num7--;
					}
				}
			}
			int i = s;
			while (i < e)
			{
				char c3 = char.ToLower(this.Stb[i]);
				if (!func(c3))
				{
					if (c3 == '.')
					{
						if (flag2 || flag || num != 0)
						{
							break;
						}
						flag2 = true;
						i++;
					}
					else if (flag && c3 == ':')
					{
						i += 2;
					}
					else
					{
						if (num != 0 || flag || i >= e - 1 || c3 != 'e')
						{
							break;
						}
						char c2 = this.Stb[i + 1];
						if (c2 != '+' && c2 != '-')
						{
							break;
						}
						num = ((c2 == '+') ? 1 : (-1));
						i += 2;
					}
				}
				else
				{
					int num8 = (int)((c3 >= 'a') ? ('\n' + (c3 - 'a')) : (c3 - '0'));
					if (num != 0)
					{
						num2 = num2 * 10 + num8;
					}
					else if (flag2)
					{
						num5 *= 10.0;
						num4 += (double)num8 / num5;
					}
					else
					{
						num3 = (flag ? (num3 * 16U + (uint)num8) : (num3 * 10U + (uint)num8));
					}
					i++;
				}
			}
			end_char_index = i;
			if (i == s)
			{
				return STB.PARSERES.ERROR;
			}
			if (flag2 || num != 0)
			{
				STB.parse_result_double = num3 + num4;
				if (num != 0)
				{
					double num9 = (double)X.Pow(10f, num2);
					STB.parse_result_double = ((num > 0) ? (STB.parse_result_double * num9) : (STB.parse_result_double / num9));
				}
				return STB.PARSERES.DOUBLE;
			}
			STB.parse_result_int = num6 * (int)num3;
			if (flag3 && end_char_index - s == 6)
			{
				STB.parse_result_int = (int)((uint)((ulong)(-16777216) | (ulong)((long)STB.parse_result_int)));
			}
			if (!flag)
			{
				STB.parse_result_double = (double)STB.parse_result_int;
			}
			return STB.PARSERES.INT;
		}

		public static double NmRes(STB.PARSERES res, double def = -1.0)
		{
			if (res == STB.PARSERES.DOUBLE)
			{
				return STB.parse_result_double;
			}
			if (res == STB.PARSERES.INT)
			{
				return (double)STB.parse_result_int;
			}
			return def;
		}

		public int NmI(int s, int e = -1, int def = 0)
		{
			int num;
			return this.NmI(s, out num, e, def);
		}

		public int NmI(int s, out int end_char_index, int e = -1, int def = 0)
		{
			if (s >= this.Length)
			{
				end_char_index = this.Length;
				return def;
			}
			int num = 0;
			bool flag = false;
			if (e < 0)
			{
				e = this.Length;
			}
			int num2 = 1;
			end_char_index = s;
			while (s < e)
			{
				char c = this.Stb[s];
				if (!TX.isNumMatch(c))
				{
					if (flag || (c != '+' && c != '-'))
					{
						break;
					}
					if (c == '-')
					{
						num2 = -num2;
					}
					s++;
				}
				else
				{
					s++;
					flag = true;
					num = num * 10 + (int)(c - '0');
				}
			}
			if (flag)
			{
				end_char_index = s;
				return num * num2;
			}
			return def;
		}

		public ulong NmUL(int s, out int end_char_index, int e = -1, ulong def = 0UL)
		{
			ulong num = 0UL;
			bool flag = false;
			if (e < 0)
			{
				e = this.Length;
			}
			while (s < e)
			{
				char c = this.Stb[s];
				if (!TX.isNumMatch(c))
				{
					break;
				}
				s++;
				flag = true;
				num = num * 10UL + (ulong)((long)(c - '0'));
			}
			end_char_index = s;
			if (!flag)
			{
				return def;
			}
			return num;
		}

		public bool NmIs(double compare, int s, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			STB.PARSERES parseres = this.Nm(s, out end_char_index, e, false);
			if (parseres == STB.PARSERES.ERROR)
			{
				return false;
			}
			if (parseres == STB.PARSERES.DOUBLE)
			{
				return STB.parse_result_double == compare;
			}
			return (double)STB.parse_result_int == compare;
		}

		public STB Scroll(int s, out int end_char_index, Func<char, bool> Fn, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			while (s < e)
			{
				if (!Fn(this.Stb[s]))
				{
					end_char_index = s;
					return this;
				}
				s++;
			}
			end_char_index = e;
			return this;
		}

		public STB ScrollBeforeDoubleUnderScore(int s, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			bool flag = false;
			while (s < e)
			{
				if (this.Stb[s] == '_')
				{
					if (flag)
					{
						end_char_index = s - 1;
						return this;
					}
					flag = true;
				}
				else
				{
					flag = false;
				}
				s++;
			}
			end_char_index = e;
			return this;
		}

		public STB TrimSpace(int s, out int start_char_index, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			this.SkipSpace(s, out start_char_index, e);
			this.TaleTrimSpace(e, out end_char_index, start_char_index);
			return this;
		}

		public STB SkipSpace(int s, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			while (s < e)
			{
				if (!this.isSpace(s))
				{
					end_char_index = s;
					return this;
				}
				s++;
			}
			end_char_index = e;
			return this;
		}

		public STB TaleTrimSpace(int e, out int end_char_index, int s = 0)
		{
			while (--e >= s)
			{
				if (!this.isSpace(e))
				{
					end_char_index = e + 1;
					return this;
				}
			}
			end_char_index = s;
			return this;
		}

		public STB TaleTrimComment(int s, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			while (s < e - 1)
			{
				if (this.Stb[s] == '/' && this.Stb[s + 1] == '/')
				{
					end_char_index = s;
					return this;
				}
				s++;
			}
			end_char_index = e;
			return this;
		}

		public STB csvAdjust(ref int start, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			this.TaleTrimComment(start, out e, e);
			this.TaleTrimSpace(e, out end_char_index, start);
			this.SkipSpace(start, out start, end_char_index);
			return this;
		}

		public int countLines()
		{
			int num = 1;
			int num2;
			for (int i = 0; i < this.Length; i = num2 + 1)
			{
				num2 = this.IndexOf('\n', i, -1);
				if (num2 < 0)
				{
					break;
				}
				num++;
			}
			return num;
		}

		public bool isSpace(int s)
		{
			if (s >= this.Length)
			{
				return false;
			}
			char c = this.Stb[s];
			return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\u3000';
		}

		public bool isSpaceOrComma(int s)
		{
			if (s >= this.Length)
			{
				return false;
			}
			char c = this.Stb[s];
			return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\u3000' || c == ',';
		}

		public bool isSpaceOrCommaOrTilde(int s)
		{
			if (s >= this.Length)
			{
				return false;
			}
			char c = this.Stb[s];
			return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\u3000' || c == ',' || c == '~';
		}

		public bool isSpace(char s)
		{
			return TX.isSpace(s);
		}

		public bool isSpaceOrComma(char s)
		{
			return TX.isSpaceOrComma(s);
		}

		public bool isSpaceOrCommaOrTilde(char s)
		{
			return TX.isSpaceOrCommaOrTilde(s);
		}

		public bool isFirstWord(string compare_to, int index = 0)
		{
			if (this.Length < compare_to.Length + index)
			{
				return false;
			}
			int num;
			this.ScrollFirstWord(index, out num, null, -1);
			return this.Equals(index, num, compare_to, false);
		}

		public List<string> csvSplit(int start, int e = -1, bool ensure_element_0 = true)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			STB.Asplit_buf.Clear();
			if (start < e)
			{
				STB stb = TX.PopBld(null, 0);
				int num;
				this.SkipSeparationItem(start, -1, out num, e, stb, null);
				TX.ReleaseBld(stb);
			}
			if (STB.Asplit_buf.Count == 0 && ensure_element_0)
			{
				STB.Asplit_buf.Add("");
			}
			return STB.Asplit_buf;
		}

		public void SkipSeparationItem(int start, int skip_count, out int end_char_index, int e = -1, STB BBuf = null, List<string> Asplit_buf = null)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			int i = start;
			Asplit_buf = Asplit_buf ?? STB.Asplit_buf;
			if (i < e)
			{
				bool flag = false;
				bool flag2 = false;
				while (i < e)
				{
					char c = this.Stb[i];
					if (c == '\\')
					{
						if (i < e - 1 && BBuf != null)
						{
							BBuf += this.Stb[i + 1];
						}
						i += 2;
					}
					else
					{
						if (c == '\'')
						{
							flag = !flag;
							if (flag2)
							{
								flag2 = !flag2;
								start = i;
							}
						}
						else
						{
							if (!flag && TX.isSpaceOrCommaOrTilde(c) != flag2)
							{
								if (!flag2)
								{
									if (skip_count == 0)
									{
										end_char_index = i;
										return;
									}
									if (i != start)
									{
										if (BBuf != null)
										{
											if (BBuf.Length > 0)
											{
												Asplit_buf.Add(BBuf.ToString());
											}
											BBuf.Clear();
										}
										if (skip_count > 0 && --skip_count == 0)
										{
											end_char_index = i;
											return;
										}
									}
								}
								else
								{
									start = i;
								}
								flag2 = !flag2;
							}
							if (BBuf != null && (flag || !flag2))
							{
								BBuf += c;
							}
						}
						i++;
					}
				}
				if (!flag2 && BBuf != null && BBuf.Length > 0)
				{
					Asplit_buf.Add(BBuf.ToString());
				}
			}
			end_char_index = e;
		}

		public STB ScrollBracket(int s, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			int num = 1;
			char c = this.Stb[s++];
			char c2;
			if (c != '(')
			{
				if (c != '[')
				{
					if (c != '{')
					{
						end_char_index = s;
						return this;
					}
					c2 = '}';
				}
				else
				{
					c2 = ']';
				}
			}
			else
			{
				c2 = ')';
			}
			while (s < e)
			{
				char c3 = this.Stb[s++];
				if (c3 == c)
				{
					num++;
				}
				else if (c3 == c2 && --num == 0)
				{
					end_char_index = s;
					return this;
				}
			}
			end_char_index = e;
			return this;
		}

		public STB ScrollNextQuote(int s, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			char c = this.Stb[s++];
			if (c != '"' && c != '\'')
			{
				end_char_index = s;
				return this;
			}
			while (s < e)
			{
				char c2 = this.Stb[s++];
				if (c2 == '\\')
				{
					s++;
				}
				else if (c2 == c)
				{
					end_char_index = s;
					return this;
				}
			}
			end_char_index = e;
			return this;
		}

		public STB ScrollLine(int s, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			while (s < e - 1)
			{
				if (this.Stb[s] == '\n')
				{
					end_char_index = s;
					return this;
				}
				s++;
			}
			end_char_index = e;
			return this;
		}

		public STB ScrollPreviousLine(int s, out int start_char_index, out int end_char_index)
		{
			bool flag = true;
			end_char_index = s;
			while (s >= 0)
			{
				char c = this.Stb[s];
				if (c == '\n' || c == '\r')
				{
					if (!flag)
					{
						start_char_index = s + 1;
						return this;
					}
					s--;
				}
				else
				{
					if (flag)
					{
						flag = false;
						end_char_index = s + 1;
					}
					s--;
				}
			}
			start_char_index = 0;
			return this;
		}

		public STB.MULT_OPE ScrollOperand(int s, out int end_char_index, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			if (s >= e)
			{
				end_char_index = s;
				return STB.MULT_OPE.ERROR;
			}
			char c = this.Stb[s];
			if (c <= '*')
			{
				if (c <= '%')
				{
					if (c != '!')
					{
						if (c != '%')
						{
							goto IL_019C;
						}
						end_char_index = s + 1;
						return STB.MULT_OPE.MOD;
					}
					else
					{
						end_char_index = s + 1;
						if (s + 1 < e && this.Stb[s + 1] == '=')
						{
							end_char_index++;
							return STB.MULT_OPE.COMP_NOTEQUAL;
						}
						return STB.MULT_OPE.NOT;
					}
				}
				else if (c != '&')
				{
					if (c != '*')
					{
						goto IL_019C;
					}
					end_char_index = s + 1;
					return STB.MULT_OPE.MUL;
				}
			}
			else if (c <= '>')
			{
				if (c == '/')
				{
					end_char_index = s + 1;
					return STB.MULT_OPE.DIV;
				}
				switch (c)
				{
				case '<':
					end_char_index = s + 1;
					if (s + 1 < e)
					{
						char c2 = this.Stb[s + 1];
						if (c2 == '=')
						{
							end_char_index++;
							return STB.MULT_OPE.COMP_LE;
						}
						if (c2 == c)
						{
							end_char_index++;
							return STB.MULT_OPE.BIT_LEFT;
						}
					}
					return STB.MULT_OPE.COMP_L;
				case '=':
					end_char_index = s + 1;
					if (s + 1 < e && this.Stb[s + 1] == '=')
					{
						end_char_index++;
						return STB.MULT_OPE.COMP_EQUAL;
					}
					return STB.MULT_OPE.COMP_EQUAL;
				case '>':
					end_char_index = s + 1;
					if (s + 1 < e)
					{
						char c3 = this.Stb[s + 1];
						if (c3 == '=')
						{
							end_char_index++;
							return STB.MULT_OPE.COMP_GE;
						}
						if (c3 == c)
						{
							end_char_index++;
							return STB.MULT_OPE.BIT_RIGHT;
						}
					}
					return STB.MULT_OPE.COMP_G;
				default:
					goto IL_019C;
				}
			}
			else
			{
				if (c == '^')
				{
					end_char_index = s + 1;
					return STB.MULT_OPE.EXPO;
				}
				if (c != '|')
				{
					goto IL_019C;
				}
			}
			end_char_index = s + 1;
			if (s + 1 < e && this.Stb[s + 1] == c)
			{
				end_char_index++;
				if (c != '&')
				{
					return STB.MULT_OPE.COMP_OR;
				}
				return STB.MULT_OPE.COMP_AND;
			}
			else
			{
				if (c != '&')
				{
					return STB.MULT_OPE.BIT_OR;
				}
				return STB.MULT_OPE.BIT_AND;
			}
			IL_019C:
			end_char_index = s + 1;
			return STB.MULT_OPE.ERROR;
		}

		public static int ope_level(STB.MULT_OPE ope)
		{
			switch (ope)
			{
			case STB.MULT_OPE.ERROR:
				return 0;
			case STB.MULT_OPE.BIT_OR:
			case STB.MULT_OPE.BIT_AND:
			case STB.MULT_OPE.BIT_LEFT:
			case STB.MULT_OPE.BIT_RIGHT:
				return 3;
			case STB.MULT_OPE.COMP_EQUAL:
			case STB.MULT_OPE.COMP_NOTEQUAL:
			case STB.MULT_OPE.COMP_G:
			case STB.MULT_OPE.COMP_GE:
			case STB.MULT_OPE.COMP_L:
			case STB.MULT_OPE.COMP_LE:
				return 4;
			case STB.MULT_OPE.COMP_AND:
			case STB.MULT_OPE.COMP_OR:
				return 5;
			}
			return 1;
		}

		public bool calcOperandIsStr(string ope, STB Vr)
		{
			if (ope != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(ope);
				if (num > 2496394275U)
				{
					if (num <= 3439459332U)
					{
						if (num <= 2783786092U)
						{
							if (num != 2537784475U)
							{
								if (num != 2631874108U)
								{
									if (num != 2783786092U)
									{
										goto IL_03E6;
									}
									if (!(ope == "NOTENDS"))
									{
										goto IL_03E6;
									}
									goto IL_03CA;
								}
								else if (!(ope == "STARTSWITH"))
								{
									goto IL_03E6;
								}
							}
							else
							{
								if (!(ope == "ends"))
								{
									goto IL_03E6;
								}
								goto IL_03B1;
							}
						}
						else if (num != 2908551163U)
						{
							if (num != 3419831194U)
							{
								if (num != 3439459332U)
								{
									goto IL_03E6;
								}
								if (!(ope == "STARTS"))
								{
									goto IL_03E6;
								}
							}
							else
							{
								if (!(ope == "isnot"))
								{
									goto IL_03E6;
								}
								goto IL_036F;
							}
						}
						else
						{
							if (!(ope == "NOTSTARTSWITH"))
							{
								goto IL_03E6;
							}
							goto IL_03A4;
						}
					}
					else if (num <= 3915038215U)
					{
						if (num != 3640136440U)
						{
							if (num != 3699631684U)
							{
								if (num != 3915038215U)
								{
									goto IL_03E6;
								}
								if (!(ope == "NOTCONTAINS"))
								{
									goto IL_03E6;
								}
								goto IL_038A;
							}
							else
							{
								if (!(ope == "notendswith"))
								{
									goto IL_03E6;
								}
								goto IL_03CA;
							}
						}
						else
						{
							if (!(ope == "CONTAINS"))
							{
								goto IL_03E6;
							}
							goto IL_037A;
						}
					}
					else if (num != 4005059268U)
					{
						if (num != 4037734540U)
						{
							if (num != 4221853948U)
							{
								goto IL_03E6;
							}
							if (!(ope == "startswith"))
							{
								goto IL_03E6;
							}
						}
						else
						{
							if (!(ope == "notends"))
							{
								goto IL_03E6;
							}
							goto IL_03CA;
						}
					}
					else if (!(ope == "starts"))
					{
						goto IL_03E6;
					}
					return this.IndexOf(Vr, 0, -1) == 0;
				}
				if (num > 1285841380U)
				{
					if (num <= 1825239352U)
					{
						if (num != 1312329493U)
						{
							if (num != 1455177703U)
							{
								if (num != 1825239352U)
								{
									goto IL_03E6;
								}
								if (!(ope == "contains"))
								{
									goto IL_03E6;
								}
								goto IL_037A;
							}
							else
							{
								if (!(ope == "notcontains"))
								{
									goto IL_03E6;
								}
								goto IL_038A;
							}
						}
						else if (!(ope == "is"))
						{
							goto IL_03E6;
						}
					}
					else if (num != 1843990421U)
					{
						if (num != 2351321427U)
						{
							if (num != 2496394275U)
							{
								goto IL_03E6;
							}
							if (!(ope == "ENDSWITH"))
							{
								goto IL_03E6;
							}
							goto IL_03B1;
						}
						else
						{
							if (!(ope == "notstarts"))
							{
								goto IL_03E6;
							}
							goto IL_03A4;
						}
					}
					else if (!(ope == "IS"))
					{
						goto IL_03E6;
					}
					return this.Equals(Vr);
				}
				if (num <= 790464931U)
				{
					if (num != 66489947U)
					{
						if (num != 183859059U)
						{
							if (num != 790464931U)
							{
								goto IL_03E6;
							}
							if (!(ope == "endswith"))
							{
								goto IL_03E6;
							}
							goto IL_03B1;
						}
						else
						{
							if (!(ope == "NOTSTARTS"))
							{
								goto IL_03E6;
							}
							goto IL_03A4;
						}
					}
					else
					{
						if (!(ope == "notstartswith"))
						{
							goto IL_03E6;
						}
						goto IL_03A4;
					}
				}
				else if (num != 866248570U)
				{
					if (num != 1079992859U)
					{
						if (num != 1285841380U)
						{
							goto IL_03E6;
						}
						if (!(ope == "NOTENDSWITH"))
						{
							goto IL_03E6;
						}
						goto IL_03CA;
					}
					else
					{
						if (!(ope == "ENDS"))
						{
							goto IL_03E6;
						}
						goto IL_03B1;
					}
				}
				else if (!(ope == "ISNOT"))
				{
					goto IL_03E6;
				}
				IL_036F:
				return !this.Equals(Vr);
				IL_037A:
				return this.IndexOf(Vr, 0, -1) >= 0;
				IL_038A:
				return this.IndexOf(Vr, 0, -1) < 0;
				IL_03A4:
				return this.IndexOf(Vr, 0, -1) != 0;
				IL_03B1:
				return this.IndexOf(Vr, 0, -1) == this.Length - Vr.Length;
				IL_03CA:
				return this.IndexOf(Vr, 0, -1) != this.Length - Vr.Length;
			}
			IL_03E6:
			X.de("不明なオペランド:" + ope, null);
			return false;
		}

		public bool vpVarDefineHeader(int s, out int var_len, out int end_char_index, out bool eval_input, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			var_len = 0;
			eval_input = false;
			while (s < e - 1)
			{
				char c = this.Stb[s];
				if (TX.isSpace(c))
				{
					if (var_len > 0)
					{
						break;
					}
					s++;
				}
				else if (!TX.isVariableNameMatch(c))
				{
					if (var_len > 0 && c == '=')
					{
						end_char_index = s + 1;
						if (end_char_index < e && this.Stb[end_char_index] == '~')
						{
							end_char_index++;
							eval_input = true;
						}
						return true;
					}
					end_char_index = s;
					return false;
				}
				else
				{
					var_len++;
					s++;
				}
			}
			end_char_index = e;
			return false;
		}

		public STB ScrollNextVariable(int s, out int doller_index, out int end_char_index, STB StbVarName, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			StbVarName.Clear();
			doller_index = -1;
			while (s < e)
			{
				char c = this.Stb[s++];
				if (c == '\\')
				{
					s++;
				}
				else if (c == '$' && s < e)
				{
					if (this.Stb[s] == '{')
					{
						doller_index = s - 1;
						this.ScrollBracket(s++, out end_char_index, e);
						int num;
						int num2;
						this.TrimSpace(s, out num, out num2, end_char_index - 1);
						StbVarName.Add(this, num, num2 - num);
						return this;
					}
					if (TX.isWMatch(this.Stb[s]))
					{
						doller_index = s - 1;
						this.Scroll(s, out end_char_index, (char __c) => TX.isWMatch(__c), e);
						StbVarName.Add(this, s, end_char_index - s);
						return this;
					}
				}
			}
			end_char_index = e;
			return this;
		}

		public STB ScrollFirstWord(int s, out int end_char_index, STB StbWord, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			if (StbWord != null)
			{
				StbWord.Clear();
			}
			int num = -1;
			while (s < e)
			{
				char c = this.Stb[s++];
				if ((num >= 0) ? TX.isSpaceStrict(c) : (c == '\n' || c == '\r'))
				{
					if (StbWord != null)
					{
						this.SkipSpace(s, out end_char_index, e);
					}
					else
					{
						end_char_index = s;
					}
					return this;
				}
				if (num < 0)
				{
					if (TX.isSpaceStrict(c))
					{
						continue;
					}
					num = s;
				}
				if (StbWord != null)
				{
					StbWord.Add(c);
				}
			}
			end_char_index = e;
			return this;
		}

		public bool IsSectionHeader(int s, STB StbWord, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			int num;
			this.ScrollFirstWord(s, out num, StbWord, e);
			if (StbWord.Equals("/*___"))
			{
				int num2;
				this.ScrollFirstWord(num, out num2, StbWord, e);
				return true;
			}
			if (StbWord.Equals("/*"))
			{
				int num3;
				this.ScrollFirstWord(num, out num3, StbWord, e);
				if (StbWord.Equals("___"))
				{
					int num4;
					this.ScrollFirstWord(num3, out num4, StbWord, e);
					return true;
				}
			}
			StbWord.Clear();
			return false;
		}

		public bool IsWholeWMatch(int s = 0, int e = -1)
		{
			if (e < 0)
			{
				e = this.Length;
			}
			if (e <= s)
			{
				return false;
			}
			int num;
			this.Scroll(s, out num, (char __c) => TX.isWMatch(__c), e);
			return num >= e;
		}

		public override string ToString()
		{
			return this.Stb.ToString();
		}

		public string ToString(int startIndex, int length)
		{
			if (length > 0)
			{
				return this.Stb.ToString(startIndex, length);
			}
			return "";
		}

		public string get_slice(int startIndex, int e)
		{
			if (e > startIndex)
			{
				return this.Stb.ToString(startIndex, e - startIndex);
			}
			return "";
		}

		public void Dispose()
		{
			TX.ReleaseBld(this);
		}

		private readonly StringBuilder Stb;

		private static char[] Achar_buf;

		private static List<string> Asplit_buf;

		private byte[] Abuffer_encode;

		private int txa_first = -1;

		private byte txa_id = 1;

		public static int parse_result_int;

		public static double parse_result_double;

		public enum PARSERES
		{
			ERROR,
			INT,
			DOUBLE
		}

		public enum MULT_OPE
		{
			ERROR,
			NOT,
			MUL,
			DIV,
			MOD,
			EXPO,
			BIT_OR,
			BIT_AND,
			BIT_LEFT,
			BIT_RIGHT,
			COMP_EQUAL,
			COMP_NOTEQUAL,
			COMP_G,
			COMP_GE,
			COMP_L,
			COMP_LE,
			COMP_AND,
			COMP_OR
		}
	}
}
