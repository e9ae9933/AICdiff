using System;
using System.Collections.Generic;

namespace XX
{
	public class ClsPool<T> where T : class
	{
		public ClsPool(Func<T> _FnCreate, int initial_stock = 2)
		{
			this.FnCreate = _FnCreate;
			this.AItems = new List<T>(initial_stock);
			this.Stock(initial_stock);
		}

		public ClsPool<T> Stock(int cnt = 1)
		{
			while (--cnt >= 0)
			{
				this.AItems.Add(this.FnCreate());
			}
			return this;
		}

		public virtual void releaseAll(bool alloc_whole_using_to_release = false, bool not_shuffle_to_after = false)
		{
			if (!alloc_whole_using_to_release)
			{
				this.AItems.RemoveRange(0, this.using_cnt);
				this.Stock(this.using_cnt);
			}
			else if (!not_shuffle_to_after)
			{
				using (BList<T> blist = ListBuffer<T>.Pop(0))
				{
					for (int i = 0; i < this.using_cnt; i++)
					{
						blist.Add(this.AItems[i]);
					}
					this.AItems.RemoveRange(0, this.using_cnt);
					this.AItems.AddRange(blist);
				}
			}
			this.using_cnt = 0;
		}

		public T Pool()
		{
			if (this.using_cnt >= this.AItems.Count)
			{
				this.Stock(this.using_cnt - this.AItems.Count + 1);
			}
			List<T> aitems = this.AItems;
			int num = this.using_cnt;
			this.using_cnt = num + 1;
			return aitems[num];
		}

		public virtual T Release(T Target)
		{
			if (Target != null)
			{
				int num = this.AItems.IndexOf(Target);
				if (num >= 0 && num < this.using_cnt)
				{
					this.AItems.RemoveAt(num);
					this.AItems.Add(Target);
					this.using_cnt--;
				}
			}
			return default(T);
		}

		public void Release(List<T> Target)
		{
			for (int i = Target.Count - 1; i >= 0; i--)
			{
				this.Release(Target[i]);
			}
			Target.Clear();
		}

		protected List<T> AItems;

		protected int using_cnt;

		public Func<T> FnCreate;
	}
}
