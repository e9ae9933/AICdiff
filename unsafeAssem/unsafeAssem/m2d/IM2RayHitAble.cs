using System;

namespace m2d
{
	public interface IM2RayHitAble
	{
		RAYHIT can_hit(M2Ray Ray);

		HITTYPE getHitType(M2Ray Ray);

		int applyHpDamage(int val, bool force = false, AttackInfo Atk = null);
	}
}
