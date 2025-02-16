using System;

namespace XX
{
	public class FlaggerS<T> where T : struct
	{
		public FlaggerS(FlaggerS<T>.FnFlaggerCall _FnActivate = null, FlaggerS<T>.FnFlaggerCall _FnDeactivate = null)
		{
			this.Astr = new T[16];
			this.FnActivate = _FnActivate;
			this.FnDeactivate = _FnDeactivate;
		}

		public FlaggerS<T> Add(FlaggerS<T> Fl)
		{
			int num = Fl.str_i;
			for (int i = 0; i < num; i++)
			{
				this.Add(Fl.Astr[i]);
			}
			return this;
		}

		public FlaggerS<T> Add(T str)
		{
			if (X.isinS<T>(this.Astr, str, this.str_i) == -1)
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

		public FlaggerS<T> Clear()
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

		public FlaggerS<T> Rem(T str)
		{
			int num = X.isinS<T>(this.Astr, str, this.str_i);
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

		public bool isActive(params T[] n)
		{
			if (this.str_i == 0)
			{
				return false;
			}
			for (int i = this.str_i - 1; i >= 0; i--)
			{
				if (X.isinS<T>(n, this.Astr[i]) == -1)
				{
					return true;
				}
			}
			return false;
		}

		public int Count
		{
			get
			{
				return this.str_i;
			}
		}

		public static FlaggerS<T>operator +(FlaggerS<T> Fl, T w)
		{
			Fl.Add(w);
			return Fl;
		}

		public static FlaggerS<T>operator +(FlaggerS<T> Fl, FlaggerS<T> w)
		{
			Fl.Add(w);
			return Fl;
		}

		public static FlaggerS<T>operator -(FlaggerS<T> Fl, T w)
		{
			Fl.Rem(w);
			return Fl;
		}

		private T[] Astr;

		private int str_i;

		public FlaggerS<T>.FnFlaggerCall FnActivate;

		public FlaggerS<T>.FnFlaggerCall FnDeactivate;

		public delegate void FnFlaggerCall(FlaggerS<T> Flg);
	}
}
