using System;
using m2d;
using UnityEngine;

namespace nel
{
	public sealed class M2PhysPr : M2Phys
	{
		private static M2Phys createPhysPr(M2Mover _Mv, Rigidbody2D _Rgd = null)
		{
			return new M2PhysPr(_Mv, _Rgd);
		}

		public static Func<M2Mover, Rigidbody2D, M2Phys> FD_createPhysPr
		{
			get
			{
				if (M2PhysPr.FD_createPhysPr_ == null)
				{
					M2PhysPr.FD_createPhysPr_ = new Func<M2Mover, Rigidbody2D, M2Phys>(M2PhysPr.createPhysPr);
				}
				return M2PhysPr.FD_createPhysPr_;
			}
		}

		public M2PhysPr(M2Mover _Mv, Rigidbody2D _Rgd = null)
			: base(_Mv, _Rgd)
		{
			this.Pr = _Mv as PR;
		}

		public override M2Phys addFocXy(FOCTYPE type, float velocity_x, float velocity_y, float max_abs = -1f, int t_attack = -1, int t_hold = 1, int t_release = 0, int fric_time = -1, int fric_ignore = 0)
		{
			if ((type & (FOCTYPE.HIT | FOCTYPE.DAMAGE)) != (FOCTYPE)0U && this.Pr.getAbsorbContainer().cannot_move)
			{
				return this;
			}
			return base.addFocXy(type, velocity_x, velocity_y, max_abs, t_attack, t_hold, t_release, fric_time, fric_ignore);
		}

		protected override float speedRatio(M2Phys.P2Foc Foc)
		{
			if ((Foc.foctype & (FOCTYPE.HIT | FOCTYPE.KNOCKBACK | FOCTYPE.CARRY | FOCTYPE.ABSORB | FOCTYPE.DAMAGE)) != (FOCTYPE)0U)
			{
				return this.Pr.Ser.baseTimeScaleRev();
			}
			return 1f;
		}

		public readonly PR Pr;

		private static Func<M2Mover, Rigidbody2D, M2Phys> FD_createPhysPr_;
	}
}
