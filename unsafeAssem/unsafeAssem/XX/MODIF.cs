using System;

namespace XX
{
	public enum MODIF
	{
		NONE,
		SHIFT,
		SH = 1,
		OPT,
		OP = 2,
		SH_OP,
		CTRL,
		CT = 4,
		SH_CT,
		OP_CT,
		SH_OP_CT,
		CMD,
		CM = 8,
		SH_CM,
		OP_CM,
		SH_OP_CM,
		CT_CM,
		SH_CT_CM,
		OP_CT_CM,
		SH_OP_CT_CM,
		ALL = 255
	}
}
