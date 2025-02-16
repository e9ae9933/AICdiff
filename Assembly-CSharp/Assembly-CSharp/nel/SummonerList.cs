using System;
using System.Collections.Generic;

namespace nel
{
	public class SummonerList : List<string>
	{
		public SummonerList(int capacity)
			: base(capacity)
		{
		}

		public ENEMYID enemyid_fix;
	}
}
