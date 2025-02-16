using System;
using XX;

namespace m2d
{
	public abstract class M2MoverOnChip : M2Mover
	{
		public override void appear(Map2d Mp)
		{
			if (this.Cp != null)
			{
				this.drawx0 = this.Cp.drawx;
				this.drawy0 = this.Cp.drawy;
			}
			else
			{
				X.de("M2Chip は早めに設定しておいてください", null);
			}
			base.appear(Mp);
		}

		public override void runPre()
		{
			base.runPre();
			if (this.need_reposition_flag < 2)
			{
				if (this.drawx0 != this.Cp.drawx || this.drawy0 != this.Cp.drawy)
				{
					this.drawx0 = this.Cp.drawx;
					this.drawy0 = this.Cp.drawy;
					this.need_reposition_flag = 1;
				}
				if (this.need_reposition_flag == 1)
				{
					this.repositionToChip();
				}
			}
		}

		public abstract void repositionToChip();

		protected override bool noHitableAttack()
		{
			return false;
		}

		protected M2Chip Cp;

		private int drawx0;

		private int drawy0;

		public byte need_reposition_flag;
	}
}
