using System;

namespace nel
{
	public enum MGHIT
	{
		AUTO = -1,
		PR = 1,
		EN,
		BERSERK,
		HIT_TO_MYSELF,
		CLOSED = 128,
		KILLED = 256,
		IMMEDIATE = 1024,
		SLEEP = 2048,
		NORMAL_ATTACK = 4096,
		CHANTED = 8192
	}
}
