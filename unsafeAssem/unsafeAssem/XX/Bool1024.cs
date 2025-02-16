using System;

namespace XX
{
	public class Bool1024
	{
		public Bool1024(int capacity = 1024, Bool1024 CopyFromSrc = null)
		{
			this.Alloc(capacity);
			if (CopyFromSrc != null)
			{
				this.CopyFrom(CopyFromSrc, -1, 0);
			}
		}

		public int capacity
		{
			get
			{
				return this.Arg.Length * 64;
			}
		}

		public Bool1024 All0()
		{
			int num = this.Arg.Length;
			for (int i = 0; i < num; i++)
			{
				this.Arg[i] = 0UL;
			}
			return this;
		}

		public bool CalcAnd1(Bool1024 B)
		{
			int num = X.Mn(this.Arg.Length, B.Arg.Length);
			for (int i = 0; i < num; i++)
			{
				if ((B.Arg[i] & this.Arg[i]) != 0UL)
				{
					return true;
				}
			}
			return false;
		}

		public Bool1024 Alloc(int capacity)
		{
			capacity = (capacity >> 6) + (((capacity & 63) > 0) ? 1 : 0);
			if (this.Arg == null)
			{
				this.Arg = new ulong[capacity];
			}
			else if (this.Arg.Length != capacity)
			{
				Array.Resize<ulong>(ref this.Arg, capacity);
			}
			return this;
		}

		public Bool1024 CopyFrom(Bool1024 Src, int src_capacity = -1, int shift_i = 0)
		{
			if (shift_i == 0 && src_capacity == -1)
			{
				for (int i = X.Mn(this.Arg.Length, Src.Arg.Length) - 1; i >= 0; i--)
				{
					this.Arg[i] = Src.Arg[i];
				}
			}
			else
			{
				int num = ((src_capacity == -1) ? Src.capacity : src_capacity);
				int capacity = this.capacity;
				for (int j = X.Mx(0, -shift_i); j < num; j++)
				{
					int num2 = j + shift_i;
					if (num2 >= capacity)
					{
						break;
					}
					this[num2] = Src[j];
				}
			}
			return this;
		}

		public void Not(int i, bool val)
		{
			int num = i >> 6;
			i &= 63;
			this.Arg[num] ^= 1UL << i;
		}

		public bool this[int i]
		{
			get
			{
				int num = i >> 6;
				i &= 63;
				return (this.Arg[num] & (1UL << i)) > 0UL;
			}
			set
			{
				int num = i >> 6;
				i &= 63;
				if (!value)
				{
					this.Arg[num] &= ~(1UL << i);
					return;
				}
				this.Arg[num] |= 1UL << i;
			}
		}

		private ulong[] Arg;
	}
}
