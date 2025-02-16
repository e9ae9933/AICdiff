using System;

namespace m2d
{
	public enum RAYHIT
	{
		NONE,
		HITTED,
		BREAK,
		KILL = 4,
		REFLECT = 8,
		NO_RETURN_MANA = 16,
		DO_NOT_AUTO_TARGET = 32,
		FINE_TARGET_CHECKER = 64
	}
}
