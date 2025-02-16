using System;

namespace nel
{
	public class MagicItemHandler
	{
		public MagicItemHandler(MagicItem _Mg)
		{
			this.id = _Mg.id;
			this.Mg = _Mg;
		}

		public bool isActive(M2MagicCaster Caster)
		{
			return this.Mg.isActive(Caster, this.id);
		}

		public readonly int id;

		public readonly MagicItem Mg;
	}
}
