using System;

namespace XX
{
	public class ClsPoolD<T> : ClsPool<T> where T : class, IDisposable
	{
		public ClsPoolD(Func<T> _FnCreate, int initial_stock = 2)
			: base(_FnCreate, initial_stock)
		{
		}

		public override T Release(T Target)
		{
			if (Target != null)
			{
				Target.Dispose();
			}
			return base.Release(Target);
		}

		public override void releaseAll(bool alloc_whole_using_to_release = false, bool not_shuffle_to_after = false)
		{
			if (alloc_whole_using_to_release)
			{
				for (int i = this.using_cnt - 1; i >= 0; i--)
				{
					this.AItems[i].Dispose();
				}
			}
			base.releaseAll(alloc_whole_using_to_release, not_shuffle_to_after);
		}
	}
}
