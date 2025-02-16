using System;
using m2d;

namespace nel
{
	public abstract class M2PuzzleGimmick : M2AttackableOnChip, IActivatable
	{
		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return (float)((this.BelongLp == null || PUZ.IT.barrier_active) ? 1 : 0);
		}

		public override void fineHittingLayer()
		{
			base.gameObject.layer = 2;
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.BelongLp = null;
			this.need_finalize = true;
		}

		public override void runPre()
		{
			base.runPre();
			if (this.need_finalize)
			{
				this.need_finalize = false;
				M2LpPuzzManageArea m2LpPuzzManageArea = this.Mp.getLabelPoint((M2LabelPoint V) => V.isContainingMover(this, 0f) && V is M2LpPuzzManageArea) as M2LpPuzzManageArea;
				if (m2LpPuzzManageArea != null)
				{
					this.BelongLp = m2LpPuzzManageArea;
				}
			}
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if (this.BelongLp != null && !PUZ.IT.barrier_active)
			{
				return RAYHIT.NONE;
			}
			return base.can_hit(Ray);
		}

		public override bool isDamagingOrKo()
		{
			return false;
		}

		public override bool initDeath()
		{
			return false;
		}

		public string getActivateKey()
		{
			return base.key;
		}

		public abstract void activate();

		public abstract void deactivate();

		protected M2LpPuzzManageArea BelongLp;

		protected bool need_finalize = true;
	}
}
