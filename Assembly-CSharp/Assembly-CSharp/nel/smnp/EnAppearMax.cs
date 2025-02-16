using System;
using XX;

namespace nel.smnp
{
	public sealed class EnAppearMax
	{
		public EnAppearMax(int _max, ENEMYID _id_strict = (ENEMYID)0U, bool _is_od = false)
		{
			this.max = _max;
			this.id_strict = _id_strict;
			this.is_od = _is_od || (this.id_strict & (ENEMYID)2147483648U) > (ENEMYID)0U;
		}

		public bool isSame(SmnEnemyKind K, string my_key)
		{
			if (this.id_strict > (ENEMYID)0U)
			{
				ENEMYID enemyid;
				return FEnum<ENEMYID>.TryParse(K.enemyid, out enemyid, false) && (enemyid | ((K.pre_overdrive || K.thunder_overdrive) ? ((ENEMYID)2147483648U) : ((ENEMYID)0U))) == this.id_strict;
			}
			return K.isSame(my_key);
		}

		public readonly int max;

		public int appeared;

		public ENEMYID id_strict;

		public bool is_od;
	}
}
