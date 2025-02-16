using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public class NearManager
	{
		public NearManager(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.ATkSrc = new List<NearManager.NearTicketSrc>(16);
		}

		public NearManager.NearTicketSrc AssignForMv(NearManager.INearLsnObject _Target, bool _moving = false, bool _flexible = false)
		{
			return this.Assign(NearManager.NCK.MV | (_moving ? NearManager.NCK._MOVING : ((NearManager.NCK)0)) | (_flexible ? NearManager.NCK._FLEXIBLE : ((NearManager.NCK)0)), _Target);
		}

		public NearManager.NearTicketSrc AssignForPr(NearManager.INearLsnObject _Target, bool _moving = false, bool _flexible = false)
		{
			return this.Assign(NearManager.NCK.PR | (_moving ? NearManager.NCK._MOVING : ((NearManager.NCK)0)) | (_flexible ? NearManager.NCK._FLEXIBLE : ((NearManager.NCK)0)), _Target);
		}

		public NearManager.NearTicketSrc AssignForCenterPr(NearManager.INearLsnObject _Target, bool _moving = false, bool _flexible = false)
		{
			return this.Assign(NearManager.NCK.PR | (_moving ? NearManager.NCK._MOVING : ((NearManager.NCK)0)) | (_flexible ? NearManager.NCK._FLEXIBLE : ((NearManager.NCK)0)), _Target);
		}

		public NearManager.NearTicketSrc Assign(NearManager.NCK _CK, NearManager.INearLsnObject _Target)
		{
			NearManager.NearTicketSrc nearTicketSrc = new NearManager.NearTicketSrc(_CK, _Target);
			this.ATkSrc.Add(nearTicketSrc);
			if ((_CK & NearManager.NCK._MOVING) != (NearManager.NCK)0)
			{
				this.recheckMovingAllMover();
			}
			return nearTicketSrc;
		}

		public void recheckMovingAllMover()
		{
			for (int i = this.Mp.mover_count - 1; i >= 0; i--)
			{
				NearCheckerM ncm = this.Mp.getMv(i).NCM;
				if (ncm != null)
				{
					ncm.fine_moving = true;
				}
			}
		}

		public void recheckAll()
		{
			for (int i = this.Mp.mover_count - 1; i >= 0; i--)
			{
				NearCheckerM ncm = this.Mp.getMv(i).NCM;
				if (ncm != null)
				{
					ncm.fine_all = true;
				}
			}
		}

		public NearManager.NearTicketSrc Deassign(NearManager.INearLsnObject _Target)
		{
			for (int i = this.ATkSrc.Count - 1; i >= 0; i--)
			{
				if (this.ATkSrc[i].Target == _Target)
				{
					this.ATkSrc.RemoveAt(i);
				}
			}
			return null;
		}

		public void clear()
		{
			this.ATkSrc.Clear();
			for (int i = this.Mp.count_movers - 1; i >= 0; i--)
			{
				NearCheckerM ncm = this.Mp.getMv(i).NCM;
				if (ncm != null)
				{
					ncm.clear();
				}
			}
		}

		public List<NearManager.NearTicketSrc> getWholeTicketSource()
		{
			return this.ATkSrc;
		}

		public readonly Map2d Mp;

		private List<NearManager.NearTicketSrc> ATkSrc;

		public enum NCK
		{
			MV = 1,
			PR,
			EN = 4,
			CENTER_PR = 8,
			_PARENT_TYPE = 15,
			_MOVING = 256,
			_MOVING_ALWAYS = 512,
			_ALLOC_IN_EV = 1024,
			_FLEXIBLE = 2048
		}

		public interface INearLsnObject
		{
			DRect getBounds(M2Mover Mv, DRect Dest);

			bool nearCheck(M2Mover Mv, NearTicket NTk);
		}

		public class NearTicketSrc
		{
			public NearTicketSrc(NearManager.NCK _CK, NearManager.INearLsnObject _Target)
			{
				this.CK = _CK;
				this.Target = _Target;
			}

			public bool moving_always
			{
				get
				{
					return (this.CK & NearManager.NCK._MOVING_ALWAYS) > (NearManager.NCK)0;
				}
				set
				{
					if (value)
					{
						this.CK |= NearManager.NCK._MOVING_ALWAYS;
						return;
					}
					this.CK &= (NearManager.NCK)(-513);
				}
			}

			public bool can_exec
			{
				get
				{
					return (this.CK & NearManager.NCK._ALLOC_IN_EV) != (NearManager.NCK)0 || Map2d.can_handle;
				}
			}

			public NearManager.NCK CK;

			public NearManager.INearLsnObject Target;
		}
	}
}
