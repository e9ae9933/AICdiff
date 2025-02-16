using System;
using m2d;

namespace nel
{
	public class EnAttackInfo : NelAttackInfo
	{
		public EnAttackInfo()
		{
		}

		public EnAttackInfo(float _torn_min, float _torn_max = -1000f)
		{
			base.Torn(_torn_min, _torn_max);
		}

		public EnAttackInfo(AttackInfo Src)
		{
			this.CopyFrom(Src);
		}

		public override AttackInfo CopyFrom(AttackInfo Src)
		{
			base.CopyFrom(Src);
			if (Src is EnAttackInfo)
			{
				EnAttackInfo enAttackInfo = Src as EnAttackInfo;
				this.nattr_ser_apply = enAttackInfo.nattr_ser_apply;
			}
			return this;
		}

		public EnAttackInfo Prepare(NelEnemy En, bool overwrite_attr = true)
		{
			EnemyAttr.PrepareAtk(En, this, overwrite_attr);
			return this;
		}

		public byte nattr_ser_apply;
	}
}
