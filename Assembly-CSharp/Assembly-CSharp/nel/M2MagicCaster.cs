using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public interface M2MagicCaster
	{
		Vector2 getCenter();

		Vector2 getTargetPos();

		Vector2 getAimPos(MagicItem Mg);

		int getAimDirection();

		float getPoseAngleRForCaster();

		float getHpDamagePublishRatio(MagicItem Mg);

		float getCastingTimeScale(MagicItem Mg);

		float getCastableMp();

		bool canHoldMagic(MagicItem Mg);

		bool isManipulatingMagic(MagicItem Mg);

		bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitTarget);

		void initPublishKill(M2MagicCaster Target);

		AIM getAimForCaster();

		void setAimForCaster(AIM a);
	}
}
