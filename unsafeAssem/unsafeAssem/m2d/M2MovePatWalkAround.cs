using System;
using XX;

namespace m2d
{
	internal class M2MovePatWalkAround : M2MovePatWalker
	{
		public M2MovePatWalkAround(M2EventItem _Mv, M2EventItem.MOV_PAT _pat = M2EventItem.MOV_PAT.WALKAROUND_LR)
			: base(_Mv, _pat)
		{
			this.movtf = X.Mn(-1f, -150f + 190f * X.XORSP());
		}

		protected override bool walkInner(float fcnt, ref bool moved, ref string dep_walk_pose)
		{
			this.movtf += fcnt;
			if (this.movtf >= 600f || this.walk_speed == 0f)
			{
				base.setVelocityX(0f);
				return false;
			}
			base.setVelocityX(this.walk_speed);
			moved = true;
			return true;
		}

		protected override void walkInit()
		{
			if (this.first_x == -1000f)
			{
				this.first_x = this.Mv.x;
			}
			this.walk_speed = this.walk_speed0;
			this.movtf = X.Mx(0f, 600f - X.NIXP(this.walk_time_min, this.walk_time_max) * this.walking_time_scale);
			if (X.Abs(this.first_x - this.Mv.x) < 0.2f)
			{
				this.walk_speed *= (float)X.MPF(X.XORSP() >= 0.5f);
			}
			else
			{
				this.walk_speed *= (float)X.MPF(this.Mv.x < this.first_x);
			}
			base.setVelocityX(this.walk_speed);
		}

		public override void readMovePat(StringHolder rER)
		{
			this.walking_time_scale = rER.Nm(3, this.walking_time_scale);
			this.walk_speed0 = rER.Nm(4, this.walk_speed0);
		}

		public float first_x = -1000f;

		public float walking_time_scale = 1f;

		protected float walk_time_min = 100f;

		protected float walk_time_max = 160f;

		protected float walk_speed0 = 0.0255f;

		protected float walk_speed;
	}
}
