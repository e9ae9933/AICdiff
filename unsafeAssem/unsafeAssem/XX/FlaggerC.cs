using System;

namespace XX
{
	public class FlaggerC<T> where T : class
	{
		public FlaggerC(FlaggerC<T>.FnFlaggerCall _FnActivate = null, FlaggerC<T>.FnFlaggerCall _FnDeactivate = null)
		{
			this.Astr = new T[16];
			this.FnActivate = _FnActivate;
			this.FnDeactivate = _FnDeactivate;
		}

		public FlaggerC<T> Add(T str)
		{
			if (str != null && X.isinC<T>(this.Astr, str, this.str_i) == -1)
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

		public FlaggerC<T> Clear()
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

		public FlaggerC<T> Rem(T str)
		{
			if (str == null)
			{
				return this;
			}
			int num = X.isinC<T>(this.Astr, str, this.str_i);
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
				if (X.isinC<T>(n, this.Astr[i]) == -1)
				{
					return true;
				}
			}
			return false;
		}

		public T Get1()
		{
			return this.Astr[0];
		}

		private T[] Astr;

		private int str_i;

		public FlaggerC<T>.FnFlaggerCall FnActivate;

		public FlaggerC<T>.FnFlaggerCall FnDeactivate;

		public delegate void FnFlaggerCall(FlaggerC<T> Flg);
	}
}
