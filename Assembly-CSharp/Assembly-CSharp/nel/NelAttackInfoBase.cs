using System;
using m2d;

namespace nel
{
	public class NelAttackInfoBase : AttackInfo
	{
		public NelAttackInfoBase()
		{
		}

		public NelAttackInfoBase(NelAttackInfoBase Src)
		{
		}

		public override AttackInfo CopyFrom(AttackInfo Src)
		{
			base.CopyFrom(Src);
			if (Src is NelAttackInfoBase)
			{
				NelAttackInfoBase nelAttackInfoBase = Src as NelAttackInfoBase;
				this.EpDmg = nelAttackInfoBase.EpDmg;
				this.split_mpdmg = nelAttackInfoBase.split_mpdmg;
				this.CurrentAbsorbedBy = nelAttackInfoBase.CurrentAbsorbedBy;
			}
			return this;
		}

		public EpAtk EpDmg;

		public int split_mpdmg;

		public M2Attackable CurrentAbsorbedBy;
	}
}
