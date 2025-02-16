using System;
using XX;

namespace m2d
{
	public class NearTicketMoving : NearTicket
	{
		public override NearTicket Set(NearManager.NearTicketSrc _Src)
		{
			this.prex = (this.prey = (this.prer = (this.preb = 0f)));
			return base.Set(_Src);
		}

		public bool isTargetMoved(M2Mover Mv, DRect Checker, bool force_moved)
		{
			if ((this.Src.CK & NearManager.NCK._MOVING_ALWAYS) != (NearManager.NCK)0)
			{
				return true;
			}
			DRect bounds = base.Target.getBounds(Mv, Checker);
			if (bounds == null)
			{
				return false;
			}
			if (force_moved || X.Abs(this.prex - bounds.x) + X.Abs(this.prey - bounds.y) + X.Abs(this.prer - bounds.right) + X.Abs(this.preb - bounds.bottom) >= 0.25f)
			{
				this.prex = bounds.x;
				this.prey = bounds.y;
				this.prer = bounds.right;
				this.preb = bounds.bottom;
				return true;
			}
			return false;
		}

		private float prex;

		private float prey;

		private float prer;

		private float preb;
	}
}
