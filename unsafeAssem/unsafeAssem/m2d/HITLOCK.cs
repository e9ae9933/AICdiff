using System;

namespace m2d
{
	[Flags]
	public enum HITLOCK
	{
		MOVER_ENTER = 1,
		DEATH = 2,
		SPECIAL_ATTACK = 4,
		EVADE = 8,
		DAMAGE = 16,
		APPEARING = 32,
		ABSORB = 64,
		AIR = 128,
		EVENT = 256,
		DIGGING = 512,
		THROWRAY = 1024,
		PRESSER = 2048
	}
}
