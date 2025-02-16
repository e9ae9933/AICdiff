using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public class NearCheckerM
	{
		public NearCheckerM(M2Mover _Parent, string name, NearManager.NCK _nck_type)
		{
			this.Parent = _Parent;
			this.Parent.NCM = this;
			this.nck_type = _nck_type;
			this.Mp = this.Parent.Mp;
			this.NM = this.Mp.NM;
			this.is_center_pr = this.Parent == this.Mp.M2D.Cam.getBaseMover();
			this.ATk4 = new List<NearManager.NearTicketSrc>(16);
			this.ATkNear = new NearTicket[16];
			this.ATkMoving = new NearTicketMoving[16];
			if (this.Checker == null)
			{
				this.Checker = new DRect(name);
			}
		}

		public void clear()
		{
			this.ATk4.Clear();
			this.update_t = 0f;
			this.tknear_max = 0;
			this.tkmoving_max = 0;
			this.upd_flg = NearCheckerM.UPDATE._ALL;
		}

		private static NearTicket PopTicket(NearManager.NearTicketSrc Src, ref NearTicket[] A, ref int t)
		{
			if (A.Length <= t)
			{
				Array.Resize<NearTicket>(ref A, t + 16);
			}
			if (A[t] == null)
			{
				A[t] = new NearTicket();
			}
			NearTicket[] array = A;
			int num = t;
			t = num + 1;
			return array[num].Set(Src);
		}

		private static NearTicketMoving PopTicketMoving(NearManager.NearTicketSrc Src, ref NearTicketMoving[] A, ref int t)
		{
			if (A.Length <= t)
			{
				Array.Resize<NearTicketMoving>(ref A, t + 16);
			}
			if (A[t] == null)
			{
				A[t] = new NearTicketMoving();
			}
			NearTicketMoving[] array = A;
			int num = t;
			t = num + 1;
			object obj = array[num];
			obj.Set(Src);
			return obj;
		}

		public bool grid4_updated
		{
			get
			{
				return (this.upd_flg & NearCheckerM.UPDATE.GRID4) > (NearCheckerM.UPDATE)0;
			}
			set
			{
				if (value)
				{
					this.upd_flg |= NearCheckerM.UPDATE.GRID4;
				}
			}
		}

		public bool grid1_updated
		{
			get
			{
				return (this.upd_flg & NearCheckerM.UPDATE.GRID1) > (NearCheckerM.UPDATE)0;
			}
			set
			{
				if (value)
				{
					this.upd_flg |= NearCheckerM.UPDATE.GRID1;
				}
			}
		}

		public bool fine_moving
		{
			get
			{
				return (this.upd_flg & NearCheckerM.UPDATE.FINE_MOVING) > (NearCheckerM.UPDATE)0;
			}
			set
			{
				if (value)
				{
					this.upd_flg |= NearCheckerM.UPDATE.FINE_MOVING;
				}
			}
		}

		public bool fine_all
		{
			get
			{
				return (this.upd_flg & NearCheckerM.UPDATE._FINE_ALL) == NearCheckerM.UPDATE._FINE_ALL;
			}
			set
			{
				if (value)
				{
					this.upd_flg |= NearCheckerM.UPDATE._FINE_ALL;
				}
			}
		}

		public bool fine_float_pos
		{
			get
			{
				return (this.upd_flg & NearCheckerM.UPDATE.FLOATPOS) > (NearCheckerM.UPDATE)0;
			}
			set
			{
				if (value)
				{
					this.upd_flg |= NearCheckerM.UPDATE.FLOATPOS;
				}
			}
		}

		public bool has_flexible
		{
			get
			{
				return (this.upd_flg & NearCheckerM.UPDATE.HAS_FLEXIBLE) > (NearCheckerM.UPDATE)0;
			}
			set
			{
				if (value)
				{
					this.upd_flg |= NearCheckerM.UPDATE.HAS_FLEXIBLE;
					return;
				}
				this.upd_flg &= (NearCheckerM.UPDATE)(-17);
			}
		}

		public bool is_center_pr
		{
			get
			{
				return (this.nck_type & NearManager.NCK.CENTER_PR) > (NearManager.NCK)0;
			}
			set
			{
				if (value == this.is_center_pr)
				{
					return;
				}
				if (value)
				{
					this.nck_type |= NearManager.NCK.CENTER_PR;
				}
				else
				{
					this.nck_type &= (NearManager.NCK)(-9);
				}
				if (!value)
				{
					this.checkCachedObject(true, NearManager.NCK.CENTER_PR);
					return;
				}
				this.grid4_updated = true;
			}
		}

		private void FineFarExecute()
		{
			List<NearManager.NearTicketSrc> wholeTicketSource = this.NM.getWholeTicketSource();
			this.tknear_max = 0;
			this.has_flexible = false;
			float mleft = this.Parent.mleft;
			float mright = this.Parent.mright;
			float mtop = this.Parent.mtop;
			float mbottom = this.Parent.mbottom;
			if (this.grid4_updated)
			{
				this.ATk4.Clear();
				this.upd_flg &= (NearCheckerM.UPDATE)(-4);
				for (int i = wholeTicketSource.Count - 1; i >= 0; i--)
				{
					NearManager.NearTicketSrc nearTicketSrc = wholeTicketSource[i];
					if ((nearTicketSrc.CK & this.nck_type) != (NearManager.NCK)0 && (nearTicketSrc.CK & NearManager.NCK._MOVING) == (NearManager.NCK)0)
					{
						this.Checker.active = true;
						DRect bounds = nearTicketSrc.Target.getBounds(this.Parent, this.Checker);
						if (bounds != null)
						{
							bool moving_always = nearTicketSrc.moving_always;
							if (moving_always || bounds.isCoveringXy(mleft, mtop, mright, mbottom, 4f + this.near_lgt, -1000f))
							{
								this.ATk4.Add(nearTicketSrc);
								if (moving_always || bounds.isCoveringXy(mleft, mtop, mright, mbottom, this.near_lgt, -1000f))
								{
									NearCheckerM.PopTicket(nearTicketSrc, ref this.ATkNear, ref this.tknear_max);
									if ((nearTicketSrc.CK & NearManager.NCK._FLEXIBLE) != (NearManager.NCK)0)
									{
										this.has_flexible = true;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				this.upd_flg &= (NearCheckerM.UPDATE)(-3);
				for (int j = this.ATk4.Count - 1; j >= 0; j--)
				{
					NearManager.NearTicketSrc nearTicketSrc2 = this.ATk4[j];
					bool moving_always2 = nearTicketSrc2.moving_always;
					this.Checker.active = true;
					DRect bounds2 = nearTicketSrc2.Target.getBounds(this.Parent, this.Checker);
					if (bounds2 != null && (moving_always2 || bounds2.isCoveringXy(mleft, mtop, mright, mbottom, this.near_lgt, -1000f)))
					{
						NearCheckerM.PopTicket(nearTicketSrc2, ref this.ATkNear, ref this.tknear_max);
						if ((nearTicketSrc2.CK & NearManager.NCK._FLEXIBLE) != (NearManager.NCK)0)
						{
							this.has_flexible = true;
						}
					}
				}
			}
			this.update_t = 0f;
		}

		public void run()
		{
			if (this.fine_moving)
			{
				this.upd_flg &= (NearCheckerM.UPDATE)(-9);
				this.tkmoving_max = 0;
				List<NearManager.NearTicketSrc> wholeTicketSource = this.NM.getWholeTicketSource();
				for (int i = wholeTicketSource.Count - 1; i >= 0; i--)
				{
					NearManager.NearTicketSrc nearTicketSrc = wholeTicketSource[i];
					if ((nearTicketSrc.CK & this.nck_type) != (NearManager.NCK)0 && (nearTicketSrc.CK & NearManager.NCK._MOVING) != (NearManager.NCK)0)
					{
						NearCheckerM.PopTicketMoving(nearTicketSrc, ref this.ATkMoving, ref this.tkmoving_max);
					}
				}
			}
			bool flag = this.checkCachedObject(true, this.nck_type);
			if ((this.upd_flg & NearCheckerM.UPDATE._GRID_CHECKING) != (NearCheckerM.UPDATE)0)
			{
				this.FineFarExecute();
			}
			this.checkMovingObject(flag);
		}

		public bool checkCachedObject(bool force = false, NearManager.NCK check_nck = NearManager.NCK._PARENT_TYPE)
		{
			bool flag = false;
			this.update_t -= Map2d.TS * (((this.upd_flg & NearCheckerM.UPDATE.FLOATPOS) != (NearCheckerM.UPDATE)0) ? 4f : ((this.has_flexible || this.update_t < 16f) ? 1.5f : 0f));
			this.upd_flg &= (NearCheckerM.UPDATE)(-5);
			if (force || this.update_t <= 0f)
			{
				this.update_t = 16f;
				flag = true;
				for (int i = this.tknear_max - 1; i >= 0; i--)
				{
					NearTicket nearTicket = this.ATkNear[i];
					if ((nearTicket.Src.CK & check_nck) != (NearManager.NCK)0)
					{
						if (nearTicket.Src.can_exec && !nearTicket.Target.nearCheck(this.Parent, nearTicket))
						{
							X.shiftEmpty<NearTicket>(this.ATkNear, 1, i, this.tknear_max);
							NearTicket[] atkNear = this.ATkNear;
							int num = this.tknear_max - 1;
							this.tknear_max = num;
							atkNear[num] = nearTicket;
						}
						else if ((nearTicket.Src.CK & NearManager.NCK._MOVING_ALWAYS) != (NearManager.NCK)0)
						{
							this.upd_flg |= NearCheckerM.UPDATE.FLOATPOS;
						}
					}
				}
			}
			return flag;
		}

		public void checkMovingObject(bool force = false)
		{
			for (int i = this.tkmoving_max - 1; i >= 0; i--)
			{
				NearTicketMoving nearTicketMoving = this.ATkMoving[i];
				if (nearTicketMoving.isTargetMoved(this.Parent, this.Checker, force) && nearTicketMoving.Src.can_exec && !nearTicketMoving.Target.nearCheck(this.Parent, nearTicketMoving))
				{
					X.shiftEmpty<NearTicketMoving>(this.ATkMoving, 1, i, this.tkmoving_max);
					NearTicketMoving[] atkMoving = this.ATkMoving;
					int num = this.tkmoving_max - 1;
					this.tkmoving_max = num;
					atkMoving[num] = nearTicketMoving;
				}
			}
		}

		public readonly Map2d Mp;

		public readonly NearManager NM;

		public readonly M2Mover Parent;

		public List<NearManager.NearTicketSrc> ATk4;

		public NearTicketMoving[] ATkMoving;

		private int tkmoving_max;

		public NearTicket[] ATkNear;

		private int tknear_max;

		private DRect Checker;

		private static uint checking_id;

		public float near_lgt = 2f;

		private NearCheckerM.UPDATE upd_flg = NearCheckerM.UPDATE._ALL;

		public NearManager.NCK nck_type;

		private const float UPDATE_FINE_T = 16f;

		public float update_t;

		private enum UPDATE
		{
			GRID4 = 1,
			GRID1,
			_GRID_CHECKING,
			FLOATPOS,
			_FINE_ALL = 7,
			FINE_MOVING,
			_ALL = 15,
			HAS_FLEXIBLE
		}
	}
}
