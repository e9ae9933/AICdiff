using System;
using System.Collections.Generic;

namespace XX
{
	public class BList<T> : List<T>, IDisposable
	{
		public BList()
		{
		}

		public BList(IEnumerable<T> collection)
			: base(collection)
		{
		}

		public BList(int capacity)
			: base(capacity)
		{
		}

		public void Dispose()
		{
			ListBuffer<T>.Release(this);
		}
	}
}
