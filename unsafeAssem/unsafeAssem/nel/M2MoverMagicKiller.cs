using System;
using m2d;
using UnityEngine;

namespace nel
{
	public class M2MoverMagicKiller : M2Mover, IM2RayHitAble
	{
		public override void appear(Map2d Mp)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			base.appear(Mp);
			if (this.CC == null)
			{
				this.CC = new M2MvColliderCreatorAtk(this);
			}
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			if (!this.magic_killable)
			{
				return HITTYPE.NONE;
			}
			return (HITTYPE)65544;
		}

		protected override bool noHitableAttack()
		{
			return false;
		}

		public virtual RAYHIT can_hit(M2Ray Ray)
		{
			if (!this.magic_killable)
			{
				return RAYHIT.DO_NOT_AUTO_TARGET;
			}
			return (RAYHIT)36;
		}

		public virtual int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			return 0;
		}

		public bool magic_killable = true;
	}
}
