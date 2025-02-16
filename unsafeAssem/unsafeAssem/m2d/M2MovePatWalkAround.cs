using System;
using XX;

namespace m2d
{
	internal class M2MovePatWalkAround : M2MovePatWalker
	{
		public M2MovePatWalkAround(M2EventItem _Mv)
			: base(_Mv, M2EventItem.MOV_PAT.WALKAROUND_LR)
		{
			this.movtf = -120f;
		}

		protected override bool walkInner(float fcnt, ref bool moved, ref string dep_walk_pose)
		{
			this.movtf += fcnt;
			if (this.movtf >= 600f)
			{
				return false;
			}
			moved = true;
			return true;
		}

		protected override void walkInit()
		{
			float num = 0.0255f;
			if (this.first_x == -1000f)
			{
				this.first_x = this.Mv.x;
			}
			this.movtf = 600f - X.NIXP(100f, 160f) * this.walking_time_scale;
			if (X.Abs(this.first_x - this.Mv.x) < 0.2f)
			{
				num *= (float)X.MPF(X.XORSP() >= 0.5f);
			}
			else
			{
				num *= (float)X.MPF(this.Mv.x < this.first_x);
			}
			base.setVelocityX(num);
		}

		public override void readMovePat(StringHolder rER)
		{
			this.walking_time_scale = rER.Nm(3, this.walking_time_scale);
		}

		public float first_x = -1000f;

		public float walking_time_scale = 1f;
	}
}
