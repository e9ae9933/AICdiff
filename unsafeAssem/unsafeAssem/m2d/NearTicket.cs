using System;
using XX;

namespace m2d
{
	public class NearTicket
	{
		public virtual NearTicket Set(NearManager.NearTicketSrc _Src)
		{
			this.Src = _Src;
			return this;
		}

		public DRect getBounds(M2Mover Mv, DRect Dest)
		{
			return this.Src.Target.getBounds(Mv, Dest);
		}

		public bool moving_always
		{
			get
			{
				return (this.Src.CK & NearManager.NCK._MOVING_ALWAYS) > (NearManager.NCK)0;
			}
			set
			{
				if (value)
				{
					this.Src.CK |= NearManager.NCK._MOVING_ALWAYS;
					return;
				}
				this.Src.CK &= (NearManager.NCK)(-513);
			}
		}

		public NearManager.INearLsnObject Target
		{
			get
			{
				return this.Src.Target;
			}
		}

		public NearManager.NearTicketSrc Src;
	}
}
