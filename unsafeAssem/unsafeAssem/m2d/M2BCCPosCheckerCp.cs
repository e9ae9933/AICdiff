using System;
using XX;

namespace m2d
{
	public sealed class M2BCCPosCheckerCp : M2BCCPosChecker, IPosLitener
	{
		public M2BCCPosCheckerCp(M2Puts _Pt, IPosLitener PosLsn_ = null)
			: base(_Pt.Mp, null)
		{
			this.Pt = _Pt;
			this.PosLsn = PosLsn_ ?? this;
		}

		public override M2BlockColliderContainer TargetBCC()
		{
			if (this.Pt.AttachCM != null)
			{
				return this.Pt.AttachCM.getBCCCon() ?? base.TargetBCC();
			}
			return base.TargetBCC();
		}

		public bool getPosition(out float x, out float y)
		{
			x = this.Pt.mapcx;
			y = this.Pt.mbottom;
			return !this.Pt.active_closed;
		}

		private readonly M2Puts Pt;
	}
}
