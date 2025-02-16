using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class NaTicket
	{
		public NaTicket(NAI.TYPE _type, int priority_ = 0)
		{
			this.type = _type;
			this.priority = priority_;
			this.check_nearplace_error = (this.isMove() ? 1 : 0);
		}

		public NaTicket Recreate(NAI.TYPE _type, int priority_ = -1, bool create_dep = true, NAI Nai_For_execute_quitticket = null)
		{
			if (Nai_For_execute_quitticket != null && this.prog != PROG.PREPARE)
			{
				Nai_For_execute_quitticket.quitTicket(this, false);
			}
			this.type = _type;
			this.prog = ((Nai_For_execute_quitticket != null || this.prog == PROG.PREPARE) ? PROG.PREPARE : PROG.PREPARE_RECREATED);
			this.check_nearplace_error = (this.isMove() ? 1 : 0);
			this.t = 0f;
			if (create_dep)
			{
				this.depx_ = -1f;
				this.depy_ = -1f;
				this.DepBCC_ = null;
			}
			if (priority_ >= 0)
			{
				this.priority = priority_;
			}
			return this;
		}

		public NaTicket CheckNearPlaceError(int level)
		{
			this.check_nearplace_error = level;
			return this;
		}

		public NaTicket clear()
		{
			this.aim = -1;
			this.priority = 0;
			this.after_delay = 0f;
			this.prog = PROG.PREPARE;
			this.TargetObject = null;
			return this;
		}

		public bool initProgress(NelEnemy En)
		{
			if (this.prog == PROG.PREPARE || this.prog == PROG.PREPARE_RECREATED)
			{
				this.prog = PROG.ACTIVE;
				NAI ai = En.getAI();
				if (this.DepBCC_ == null)
				{
					if (this.depx_ == -1f)
					{
						this.depx_ = ai.target_x;
					}
					if (this.depy_ == -1f)
					{
						this.depy_ = ai.target_y;
					}
				}
				float depx = this.depx;
				if (depx >= 0f && X.Abs(En.x - depx) > 0.2f)
				{
					int num = ((En.x < depx) ? 2 : 0);
					if (this.aim == -1)
					{
						this.aim = num;
					}
					if (CAim._XD(this.aim, 1) != 0)
					{
						En.AimToLr(this.aim);
					}
				}
				else if (this.aim >= 0)
				{
					if (CAim._XD(this.aim, 1) != 0)
					{
						En.AimToLr(this.aim);
					}
				}
				else if (En.AimPr != null && this.aim != -2)
				{
					En.AimToPlayer();
				}
				ai.fineTicketStartPos();
				return true;
			}
			return false;
		}

		public float depx
		{
			get
			{
				if (this.DepBCC_ == null)
				{
					return this.depx_;
				}
				return this.depx_ + this.DepBCC_.shifted_cx;
			}
			set
			{
				if (this.DepBCC_ != null)
				{
					this.depx_ = value - this.DepBCC_.shifted_cx;
					return;
				}
				this.depx_ = value;
			}
		}

		public float depy
		{
			get
			{
				if (this.DepBCC_ == null)
				{
					return this.depy_;
				}
				return this.depy_ + this.DepBCC_.shifted_cy;
			}
			set
			{
				if (this.DepBCC_ != null)
				{
					this.depy_ = value - this.DepBCC_.shifted_cy;
					return;
				}
				this.depy_ = value;
			}
		}

		public M2BlockColliderContainer.BCCLine DepBCC
		{
			get
			{
				return this.DepBCC_;
			}
			set
			{
				if (this.DepBCC_ == value)
				{
					return;
				}
				if (this.DepBCC_ != null)
				{
					this.depx_ += this.DepBCC_.shifted_cx;
					this.depy_ += this.DepBCC_.shifted_cy;
				}
				this.DepBCC_ = value;
				if (this.DepBCC_ != null)
				{
					this.depx_ -= this.DepBCC_.shifted_cx;
					this.depy_ -= this.DepBCC_.shifted_cy;
				}
			}
		}

		public NaTicket SetAim(int _aim)
		{
			this.aim = _aim;
			return this;
		}

		public NaTicket DepX(float dep_map_x)
		{
			this.depx = dep_map_x;
			return this;
		}

		public NaTicket AfterDelay(float _t)
		{
			this.after_delay = _t;
			return this;
		}

		public NaTicket T(float _t)
		{
			this.t = _t;
			return this;
		}

		public NaTicket Delay(float _t)
		{
			this.t = -X.Mx(0f, _t);
			return this;
		}

		public NaTicket Dep(Vector2 V, M2BlockColliderContainer.BCCLine _B = null)
		{
			return this.Dep(V.x, V.y, _B);
		}

		public NaTicket SetTarget(object V)
		{
			this.TargetObject = V;
			return this;
		}

		public NaTicket Dep(float _x, float _y, M2BlockColliderContainer.BCCLine _B = null)
		{
			if (_B != null)
			{
				this.DepBCC_ = _B;
				this.depx_ = _x - _B.shifted_cx;
				this.depy_ = _y - _B.shifted_cy;
			}
			else
			{
				this.depx_ = _x;
				this.depy_ = _y;
				this.DepBCC_ = null;
			}
			return this;
		}

		public bool Progress(bool flag)
		{
			if (flag && this.prog < PROG.PROG5)
			{
				this.prog++;
			}
			return flag;
		}

		public bool Progress(ref float t, int timeout, bool otherflag = true)
		{
			if (otherflag && t >= (float)timeout)
			{
				t = 0f;
				if (this.prog < PROG.PROG5)
				{
					this.prog++;
				}
				return true;
			}
			return false;
		}

		public bool quit()
		{
			if ((this.prog & PROG.QUIT) == PROG.PREPARE)
			{
				this.prog = PROG.QUIT | (this.prog & (PROG)96);
				return true;
			}
			return false;
		}

		public bool quitFinalize()
		{
			if ((this.prog & PROG.QUIT_FINALIZED) == PROG.PREPARE)
			{
				this.prog = (PROG)80 | (this.prog & (PROG)96);
				return true;
			}
			return false;
		}

		public bool error()
		{
			this.prog = (PROG)48;
			return false;
		}

		public bool isPunch()
		{
			return (this.prog & PROG.QUIT) == PROG.PREPARE && X.BTWW(5f, (float)this.type, 8f);
		}

		public bool isMagic()
		{
			return (this.prog & PROG.QUIT) == PROG.PREPARE && X.BTWW(10f, (float)this.type, 13f);
		}

		public bool isGuard()
		{
			return (this.prog & PROG.QUIT) == PROG.PREPARE && X.BTWW(14f, (float)this.type, 17f);
		}

		public bool isAttack()
		{
			return this.isPunch() || this.isMagic();
		}

		public bool isAction()
		{
			return this.isPunch() || this.isMagic() || this.isGuard();
		}

		public bool isMove()
		{
			return this.type == NAI.TYPE.WALK || this.type == NAI.TYPE.WALK_TO_WEED || this.type == NAI.TYPE.BACKSTEP || this.type == NAI.TYPE.WARP;
		}

		public bool inProgress()
		{
			return !this.isPrepare() && !this.isQuit() && !this.isQuitFinlized();
		}

		public bool isError()
		{
			return (this.prog & PROG.ERROR) > PROG.PREPARE;
		}

		public bool isPrepare()
		{
			return this.prog == PROG.PREPARE || this.prog == PROG.PREPARE_RECREATED;
		}

		public bool isQuit()
		{
			return (this.prog & PROG.QUIT) > PROG.PREPARE;
		}

		public bool isQuitFinlized()
		{
			return (this.prog & PROG.QUIT_FINALIZED) > PROG.PREPARE;
		}

		public override string ToString()
		{
			return "<NaTicket>" + this.getTicketInfoForDebug();
		}

		public string getTicketInfoForDebug()
		{
			return string.Concat(new string[]
			{
				"T:",
				this.type.ToString(),
				" ...[",
				this.prog.ToString(),
				"] P:",
				this.priority.ToString()
			});
		}

		public int aim = -1;

		public int priority;

		public float t;

		public float after_delay;

		public PROG prog;

		public NAI.TYPE type = NAI.TYPE.WALK;

		private float depx_ = -1f;

		private float depy_ = -1f;

		private M2BlockColliderContainer.BCCLine DepBCC_;

		public object TargetObject;

		public int check_nearplace_error;
	}
}
