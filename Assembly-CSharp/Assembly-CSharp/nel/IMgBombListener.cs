using System;

namespace nel
{
	public interface IMgBombListener
	{
		bool isEatableBomb(MagicItem Mg, M2MagicCaster CheckedBy, bool execute);

		bool forceExplode(MagicItem Mg);
	}
}
