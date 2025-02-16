using System;
using m2d;
using XX;

namespace nel
{
	public class M2SerResist : FlagCounter<SER>
	{
		public M2SerResist(int alloc = 4)
			: base(alloc)
		{
		}

		public M2SerResist(M2SerResist _Src)
			: base(_Src.Count)
		{
			base.Add(_Src, 1f);
		}

		public M2SerResist Dupe()
		{
			if (!this.duplicated)
			{
				return new M2SerResist(this)
				{
					duplicated = true
				};
			}
			return this;
		}

		private bool duplicated;
	}
}
