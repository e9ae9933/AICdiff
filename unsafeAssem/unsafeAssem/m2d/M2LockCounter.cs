using System;
using Better;

namespace m2d
{
	public class M2LockCounter<T> : LockCounter<T>
	{
		public M2LockCounter(Map2d _Mp, int capacity = 4)
			: base(capacity)
		{
			this.Mp = _Mp;
		}

		public void initS(Map2d _Mp)
		{
			this.Mp = _Mp;
			base.clear();
		}

		public override float getFrame()
		{
			return this.Mp.floort;
		}

		private readonly BDic<T, float> O;

		private Map2d Mp;
	}
}
