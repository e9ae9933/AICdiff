﻿using System;

namespace nel
{
	[Flags]
	public enum POSE_TYPE
	{
		STAND = 0,
		CROUCH = 1,
		DOWN = 2,
		BACK = 4,
		ORGASM = 8,
		SENSITIVE = 16,
		ONGROUND = 32,
		JUMP = 64,
		FALL = 128,
		MANGURI = 256,
		NO_FALL_CANE = 512,
		ABSORB_DEFAULT = 1024,
		NO_SLOW_IN_WATER = 2048,
		DAMAGE_RESET = 4096,
		USE_TOP_LAYER = 8192,
		MARUNOMI = 16384,
		PRESS_DAMAGE = 32768
	}
}
