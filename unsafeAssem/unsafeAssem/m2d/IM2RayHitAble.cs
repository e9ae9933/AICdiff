using System;

namespace m2d
{
	public interface IM2RayHitAble
	{
		RAYHIT can_hit(M2Ray Ray);

		HITTYPE getHitType(M2Ray Ray);

		float auto_target_priority(M2Mover CalcFrom);

		int applyHpDamage(int val, bool force = false, AttackInfo Atk = null);
	}
}
