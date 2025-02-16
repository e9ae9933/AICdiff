using System;

namespace XX
{
	public class FlaggerT<T> where T : IComparable
	{
		public FlaggerT(FlaggerT<T>.FnFlaggerCall _FnActivate = null, FlaggerT<T>.FnFlaggerCall _FnDeactivate = null)
		{
			this.Astr = new T[16];
			this.FnActivate = _FnActivate;
			this.FnDeactivate = _FnDeactivate;
		}

		public FlaggerT<T> Add(FlaggerT<T> Fl)
		{
			int num = Fl.str_i;
			for (int i = 0; i < num; i++)
			{
				this.Add(Fl.Astr[i]);
			}
			return this;
		}

		public FlaggerT<T> Add(T str)
		{
			if (str != null && X.isinCP<T>(this.Astr, str, this.str_i) == -1)
			{
				bool flag = this.isActive();
				if (this.str_i >= this.Astr.Length)
				{
					Array.Resize<T>(ref this.Astr, this.str_i + 16);
				}
				T[] astr = this.Astr;
				int num = this.str_i;
				this.str_i = num + 1;
				astr[num] = str;
				if (!flag && this.FnActivate != null)
				{
					this.FnActivate(this);
				}
			}
			return this;
		}

		public FlaggerT<T> Clear()
		{
			bool flag = this.isActive();
			this.Astr = new T[16];
			this.str_i = 0;
			if (flag && this.FnDeactivate != null)
			{
				this.FnDeactivate(this);
			}
			return this;
		}

		public FlaggerT<T> Rem(T str)
		{
			if (str == null)
			{
				return this;
			}
			int num = X.isinCP<T>(this.Astr, str, this.str_i);
			if (num != -1)
			{
				X.shiftNotInput1<T>(this.Astr, num, ref this.str_i);
				if (!this.isActive() && this.FnDeactivate != null)
				{
					this.FnDeactivate(this);
				}
			}
			return this;
		}

		public bool isActive()
		{
			return this.str_i > 0;
		}

		public bool isActive(T[] n)
		{
			if (this.str_i == 0)
			{
				return false;
			}
			int num = n.Length;
			for (int i = this.str_i - 1; i >= 0; i--)
			{
				if (X.isinCP<T>(n, this.Astr[i], num) == -1)
				{
					return true;
				}
			}
			return false;
		}

		public bool hasKey(T s)
		{
			return this.str_i != 0 && X.isinCP<T>(this.Astr, s, this.str_i) != -1;
		}

		public int Count
		{
			get
			{
				return this.str_i;
			}
		}

		public static FlaggerT<T>operator +(FlaggerT<T> Fl, T w)
		{
			Fl.Add(w);
			return Fl;
		}

		public static FlaggerT<T>operator +(FlaggerT<T> Fl, FlaggerT<T> w)
		{
			Fl.Add(w);
			return Fl;
		}

		public static FlaggerT<T>operator -(FlaggerT<T> Fl, T w)
		{
			Fl.Rem(w);
			return Fl;
		}

		private T[] Astr;

		private int str_i;

		public FlaggerT<T>.FnFlaggerCall FnActivate;

		public FlaggerT<T>.FnFlaggerCall FnDeactivate;

		public delegate void FnFlaggerCall(FlaggerT<T> Flg);
	}
}
