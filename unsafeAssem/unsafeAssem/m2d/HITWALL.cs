using System;

namespace m2d
{
	public enum HITWALL : byte
	{
		SIM_L = 1,
		SIM_T,
		SIM_R = 4,
		SIM_B = 8,
		COLLIDER = 16
	}
}
