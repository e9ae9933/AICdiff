using System;
using XX;

namespace evt
{
	public class SpMove
	{
		public static SPMOVE Get(string k)
		{
			SPMOVE spmove;
			if (!FEnum<SPMOVE>.TryParse(k, out spmove, true))
			{
				return SPMOVE.NONE;
			}
			return spmove;
		}
	}
}
