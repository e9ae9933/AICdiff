using System;
using System.Collections.Generic;

namespace m2d
{
	public sealed class M2MoveTicket
	{
		public static void initS()
		{
			if (M2MoveTicket.IT == null)
			{
				M2MoveTicket.IT = new M2MoveTicket();
				return;
			}
			M2MoveTicket.IT.AMv.Clear();
		}

		public M2MoveTicket()
		{
			this.AMv = new List<M2Mover>();
		}

		public bool init(M2Mover Mv)
		{
			if (this.AMv.IndexOf(Mv) == -1)
			{
				this.AMv.Add(Mv);
				return true;
			}
			return false;
		}

		public void quit(M2Mover Mv)
		{
			if (this.AMv.Count == 0 || this.AMv[0] == Mv)
			{
				this.AMv.Clear();
			}
		}

		public bool isActive()
		{
			return this.AMv.Count > 0;
		}

		public static M2MoveTicket IT;

		private List<M2Mover> AMv;
	}
}
