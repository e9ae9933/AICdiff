using System;

namespace XX
{
	public class SORT<T>
	{
		public SORT(Comparison<T> FnBase = null)
		{
			this.FuncBase = FnBase;
		}

		public virtual T[] qSort(T[] Array, Comparison<T> Fn = null, int length = -1)
		{
			this.FuncCurrent = ((Fn == null) ? this.FuncBase : Fn);
			this.ArrayCurrent = Array;
			return this.qSortInner(0, ((length < 0) ? Array.Length : length) - 1, 0);
		}

		private T[] qSortInner(int left, int right, int pivot_pos)
		{
			if (right <= left)
			{
				return this.ArrayCurrent;
			}
			int num = left;
			int num2 = right;
			T t = this.ArrayCurrent[pivot_pos];
			for (;;)
			{
				if (num <= right)
				{
					if (this.FuncCurrent(this.ArrayCurrent[num], t) < 0)
					{
						num++;
						continue;
					}
				}
				while (num2 >= left && this.FuncCurrent(this.ArrayCurrent[num2], t) > 0)
				{
					num2--;
				}
				if (num >= num2)
				{
					break;
				}
				this.swapItem(num, num2);
				num++;
				num2--;
			}
			num2 = X.Mx(left, num2);
			this.qSortInner(left, num2, (left + num2) / 2);
			this.qSortInner(num2 + 1, right, (right + num2 + 1) / 2);
			return this.ArrayCurrent;
		}

		private void swapItem(int l, int r)
		{
			T[] arrayCurrent = this.ArrayCurrent;
			T[] arrayCurrent2 = this.ArrayCurrent;
			T t = this.ArrayCurrent[r];
			T t2 = this.ArrayCurrent[l];
			arrayCurrent[l] = t;
			arrayCurrent2[r] = t2;
		}

		protected Comparison<T> FuncCurrent;

		protected Comparison<T> FuncBase;

		private T[] ArrayCurrent;
	}
}
