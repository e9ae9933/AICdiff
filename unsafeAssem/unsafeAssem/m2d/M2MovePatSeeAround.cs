using System;
using XX;

namespace m2d
{
	internal class M2MovePatSeeAround : M2MovePat
	{
		public M2MovePatSeeAround(M2EventItem _Mv)
			: base(_Mv, M2EventItem.MOV_PAT.SEE_AROUND)
		{
		}

		public override bool run(float fcnt)
		{
			if (this.mvsc_assigned == 1)
			{
				return false;
			}
			M2MoverPr pr = this.Mp.Pr;
			if (this.mvsc_assigned >= 2)
			{
				this.mvsc_assigned += 1;
				if (this.mvsc_assigned >= 100)
				{
					this.mvsc_assigned = 0;
				}
				return false;
			}
			AIM aim;
			if (pr == null || X.LENGTH_SIZE(this.Mv.y, this.Mv.sizey, pr.y, pr.sizey) > 6f)
			{
				aim = AIM.T;
			}
			else
			{
				float num = X.LENGTH_SIZE(this.Mv.x, this.Mv.sizex, pr.x, pr.sizex);
				if (num > 4f)
				{
					aim = AIM.T;
				}
				else
				{
					aim = CAim.get_aim2(0f, 0f, (float)X.MPF(this.Mv.x < pr.x), (float)((num < 1.8f) ? 1 : 0), false);
				}
			}
			M2PxlAnimatorRT anm = this.Mv.Anm;
			if (anm != null)
			{
				if (aim != (AIM)anm.pose_aim)
				{
					anm.setAim((int)aim, -1);
				}
			}
			else if (aim != this.Mv.aim)
			{
				base.setAim(aim, false);
			}
			return false;
		}

		public override void assignMoveScript(bool soft_touch)
		{
			base.assignMoveScript(soft_touch);
			if (!soft_touch)
			{
				this.mvsc_assigned = 1;
			}
		}

		public override void evQuit()
		{
			if (this.mvsc_assigned == 1)
			{
				this.mvsc_assigned = 2;
			}
			base.evQuit();
		}

		private byte mvsc_assigned;
	}
}
