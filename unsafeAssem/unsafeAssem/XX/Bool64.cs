using System;

namespace XX
{
	public struct Bool64
	{
		public void Not(int i)
		{
			this.rg ^= 1UL << i;
		}

		public bool this[int i]
		{
			get
			{
				return (this.rg & (1UL << i)) > 0UL;
			}
			set
			{
				if (!value)
				{
					this.rg &= ~(1UL << i);
					return;
				}
				this.rg |= 1UL << i;
			}
		}

		private ulong rg;
	}
}
