using System;
using System.Collections.Generic;

namespace m2d
{
	public abstract class M2RegionContainerBase<T> where T : M2RegionBase
	{
		public M2RegionContainerBase(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.Aregs = new List<T>(16);
		}

		public int getLength()
		{
			return this.Aregs.Count;
		}

		public T getWR(int i)
		{
			return this.Aregs[i];
		}

		protected Map2d Mp;

		protected List<T> Aregs;
	}
}
