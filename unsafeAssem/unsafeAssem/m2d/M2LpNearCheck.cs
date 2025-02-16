using System;
using XX;

namespace m2d
{
	public class M2LpNearCheck : M2LabelPoint, IRunAndDestroy, NearManager.INearLsnObject
	{
		protected NearManager.NCK check_type
		{
			get
			{
				return NearManager.NCK.CENTER_PR;
			}
		}

		public M2LpNearCheck(string __key, int _i, M2MapLayer L)
			: base(__key, 0, L)
		{
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			if (this.Mp.NM != null && this.auto_assign_to_NM)
			{
				NearManager.NearTicketSrc nearTicketSrc = this.Mp.NM.Assign(this.check_type, this);
				if (this.alloc_in_ev)
				{
					nearTicketSrc.CK |= NearManager.NCK._ALLOC_IN_EV;
				}
			}
		}

		protected void recheckNM()
		{
			if (this.check_type == NearManager.NCK.CENTER_PR)
			{
				M2Mover baseMover = this.Mp.M2D.Cam.getBaseMover();
				if (baseMover != null && baseMover.NCM != null)
				{
					baseMover.NCM.fine_all = true;
				}
			}
		}

		public override void closeAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			if (this.Mp.NM != null)
			{
				this.Mp.NM.Deassign(this);
			}
			if (this.activated)
			{
				this.quitEnter(null);
			}
		}

		public DRect getBounds(M2Mover Mv, DRect Dest)
		{
			float num = 1f + (this.activated ? this.release_map_margin : 0f);
			return Dest.Set((float)this.mapx - num, (float)this.mapy - num, (float)this.mapw + num * 2f, (float)this.maph + num * 2f);
		}

		public virtual bool nearCheck(M2Mover Mv, NearTicket NTk)
		{
			bool flag = this.nearCheck(Mv);
			if (this.activated != flag)
			{
				if (flag)
				{
					this.initEnter(Mv);
				}
				else
				{
					this.quitEnter(Mv);
				}
			}
			return Mv == this.Mp.M2D.Cam.getBaseMover();
		}

		protected virtual bool nearCheck(M2Mover Mv)
		{
			return Mv != null && Mv == this.Mp.M2D.Cam.getBaseMover() && base.isContainingMover(Mv, this.player_in_margin_pixel + (this.activated ? (1f * base.CLEN) : 0f));
		}

		public virtual void initEnter(M2Mover Mv)
		{
			if (!this.activated)
			{
				if (this.T_FADE >= 0f)
				{
					this.Mp.addRunnerObject(this);
					this.t = X.Mx(0f, this.T_FADE + this.t);
					if (this.fnEnterOrRelease != null)
					{
						this.fnEnterOrRelease(true, this.Mp.M2D.Cam.getBaseMover());
						return;
					}
				}
				else
				{
					this.t = 0f;
				}
			}
		}

		public virtual void quitEnter(M2Mover Mv)
		{
			if (this.activated)
			{
				if (this.T_FADE >= 0f)
				{
					this.t = X.Mn(-this.T_FADE + this.t, -1f);
					return;
				}
				this.t = -1f;
			}
		}

		public virtual bool recheck(M2Mover Mv)
		{
			return this.nearCheck(Mv);
		}

		public virtual bool run(float fcnt)
		{
			if (this.t >= 0f)
			{
				this.t += fcnt;
				if (this.t >= X.Mx(this.T_FADE, 0f) + this.T_RECHECK)
				{
					M2Mover baseMover = this.Mp.M2D.Cam.getBaseMover();
					if (!this.recheck(baseMover))
					{
						this.quitEnter(baseMover);
					}
					else
					{
						this.t -= this.T_RECHECK;
					}
				}
			}
			else
			{
				if (this.t <= -this.T_FADE)
				{
					return false;
				}
				this.t -= fcnt;
			}
			return true;
		}

		public bool activated
		{
			get
			{
				return this.t >= 0f;
			}
		}

		public void destruct()
		{
		}

		protected float t = -1000f;

		public float T_FADE = 80f;

		public float T_RECHECK = 80f;

		public bool alloc_in_ev;

		private const int check_map_margin = 1;

		protected float player_in_margin_pixel = 14f;

		protected bool auto_assign_to_NM = true;

		protected float release_map_margin = 0.5f;

		public M2LpNearCheck.FnEnterOrRelease fnEnterOrRelease;

		public delegate void FnEnterOrRelease(bool entering, M2Mover Mv);
	}
}
