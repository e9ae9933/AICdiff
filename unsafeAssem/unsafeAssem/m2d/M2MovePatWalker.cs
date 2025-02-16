using System;
using XX;

namespace m2d
{
	internal abstract class M2MovePatWalker : M2MovePat
	{
		public M2MovePatWalker(M2EventItem _Mv, M2EventItem.MOV_PAT _type)
			: base(_Mv, _type)
		{
			this.Mv.gameObject.isStatic = false;
		}

		public override bool run(float fcnt)
		{
			if (this.mvsc_assigned)
			{
				return false;
			}
			if (this.movtf <= -1000f)
			{
				if (base.Mp.TalkTarget == this.Mv)
				{
					this.movtf = -1000f;
				}
				else
				{
					this.movtf -= fcnt;
					if (this.movtf <= -1070f)
					{
						this.movtf = -30f;
					}
				}
				return false;
			}
			bool flag = false;
			if (this.movtf < 0f)
			{
				base.SpSetPose("stand", -1, null, false);
				this.movtf += fcnt;
				if (this.movtf >= 0f)
				{
					this.movtf = 0f;
					this.walkInit();
				}
			}
			else
			{
				string text = "walk";
				if (!this.walkInner(fcnt, ref flag, ref text))
				{
					this.movtf = -X.NIXP(130f, 190f);
					this.setVelocityX(0f);
				}
				else
				{
					base.SpSetPose(text, -1, null, false);
				}
			}
			return flag;
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

		public override void assignMoveScript()
		{
			base.assignMoveScript();
			this.mvsc_assigned = true;
			if (M2EventCommand.Ev0 == this.Mv)
			{
				this.movtf = -1000f;
			}
			this.setVelocityX(0f);
			base.SpSetPose("stand", -1, null, false);
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
	}
}
