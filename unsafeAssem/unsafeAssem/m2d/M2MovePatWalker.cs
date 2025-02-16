using System;
using XX;

namespace m2d
{
	public abstract class M2MovePatWalker : M2MovePat
	{
		public M2MovePatWalker(M2EventItem _Mv, M2EventItem.MOV_PAT _type)
			: base(_Mv, _type)
		{
			this.Mv.gameObject.isStatic = false;
		}

		public bool event_stop_waiting
		{
			get
			{
				return this.movtf <= -1000f;
			}
		}

		public override bool run(float fcnt)
		{
			if (this.mvsc_assigned)
			{
				return false;
			}
			if (this.movtf <= -1000f)
			{
				if (this.event_stop_continue || this.reactivate_move_delay < 0f)
				{
					this.movtf = -1000f;
				}
				else
				{
					this.movtf -= fcnt;
					if (this.movtf <= -1000f - this.reactivate_move_delay)
					{
						this.movtf = -30f;
					}
				}
				return false;
			}
			bool flag = false;
			if (this.movtf < 0f)
			{
				this.setVelocityX(0f);
				if (this.movtf < -1f)
				{
					this.SpSetPose(this.dep_stand_pose, -1, null, false);
				}
				this.movtf += fcnt;
				if (this.movtf >= 0f)
				{
					this.movtf = 0f;
					this.walkInit();
				}
			}
			else if (!this.walkInner(fcnt, ref flag, ref this.dep_walk_pose))
			{
				if (this.movtf >= 0f)
				{
					this.movtf = X.Mn(-1f, -X.NIXP(this.wait_time_min, this.wait_time_max));
				}
				this.setVelocityX(0f);
			}
			else
			{
				this.SpSetPose(flag ? this.dep_walk_pose : this.dep_stand_pose, -1, null, false);
			}
			return flag;
		}

		protected virtual bool event_stop_continue
		{
			get
			{
				return this.Mp.TalkTarget == this.Mv;
			}
		}

		protected abstract void walkInit();

		protected abstract bool walkInner(float fcnt, ref bool moved, ref string dep_walk_pose);

		protected void setVelocityX(float vx)
		{
			if (base.Phy != null)
			{
				base.Phy.walk_xspeed = vx;
			}
			else
			{
				base.setVelocityForce(vx, 0f);
			}
			if (vx != 0f)
			{
				base.setAim((vx > 0f) ? AIM.R : AIM.L, false);
			}
		}

		public override void assignMoveScript(bool soft_touch)
		{
			base.assignMoveScript(soft_touch);
			if (!soft_touch)
			{
				this.mvsc_assigned = true;
				if (M2EventCommand.Ev0 == this.Mv)
				{
					this.movtf = -1000f;
				}
				this.setVelocityX(0f);
				this.SpSetPose(this.dep_stand_pose, -1, null, false);
			}
		}

		public override void evQuit()
		{
			if (this.mvsc_assigned)
			{
				this.mvsc_assigned = false;
				if (this.movtf > -1000f)
				{
					this.movtf = -120f;
				}
			}
			base.evQuit();
		}

		protected float movtf;

		protected bool mvsc_assigned;

		protected float wait_time_min = 130f;

		protected float wait_time_max = 190f;

		protected string dep_stand_pose = "stand";

		protected string dep_walk_pose = "walk";

		protected float reactivate_move_delay = 70f;
	}
}
