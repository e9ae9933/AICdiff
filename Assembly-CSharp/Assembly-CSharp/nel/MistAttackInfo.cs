using System;

namespace nel
{
	public sealed class MistAttackInfo : NelAttackInfoBase
	{
		public MistAttackInfo(int _apply_count_thresh = 0)
		{
		}

		public bool no_cough_move;

		public bool corrupt_gacha;
	}
}
