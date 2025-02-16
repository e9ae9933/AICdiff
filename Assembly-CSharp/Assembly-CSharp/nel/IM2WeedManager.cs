using System;
using m2d;

namespace nel
{
	public interface IM2WeedManager
	{
		void assignWeed(M2ManaWeed _Weed);

		void deassignWeed(M2ManaWeed _Weed);

		MANA_HIT deassignActiveWeed(M2ManaWeed _Weed, AttackInfo AtkHitExecute = null);

		void assignActiveWeed(M2ManaWeed _Weed);

		bool destructed { get; }
	}
}
