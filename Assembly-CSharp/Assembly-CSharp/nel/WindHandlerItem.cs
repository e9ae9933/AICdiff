using System;

namespace nel
{
	public struct WindHandlerItem
	{
		public WindHandlerItem(WindItem _Wind)
		{
			this.Wind = _Wind;
			this.id = this.Wind.id;
		}

		public bool isActive()
		{
			return this.Wind != null && this.Wind.id == this.id;
		}

		public bool isActive(WindItem Wn)
		{
			return this.Wind == Wn && this.isActive();
		}

		public void destruct()
		{
			if (this.isActive())
			{
				this.Wind.destruct();
			}
			this.Wind = null;
		}

		public WindItem Wind;

		public uint id;
	}
}
