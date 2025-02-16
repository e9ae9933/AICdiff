using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public class M2BCCPosChecker
	{
		public M2BCCPosChecker(Map2d _Mp, IPosLitener PosLsn_)
		{
			this.PosLsn = PosLsn_;
			this.Mp = _Mp;
			if (M2BCCPosChecker.ABuf == null)
			{
				M2BCCPosChecker.ABuf = new List<M2BlockColliderContainer.BCCLine>(4);
			}
		}

		public virtual M2BlockColliderContainer TargetBCC()
		{
			return this.Mp.BCC;
		}

		public M2BlockColliderContainer.BCCLine Get()
		{
			M2BlockColliderContainer m2BlockColliderContainer = this.TargetBCC();
			if (m2BlockColliderContainer == null)
			{
				return null;
			}
			if (this.C == null || m2BlockColliderContainer.gen_id != this.gen_id || this.C.BCC != m2BlockColliderContainer)
			{
				float num;
				float num2;
				if (!this.PosLsn.getPosition(out num, out num2))
				{
					return null;
				}
				this.gen_id = m2BlockColliderContainer.gen_id;
				this.C = null;
				int num3 = (int)num;
				int num4 = (int)(num2 + 0.04f);
				if (m2BlockColliderContainer == this.Mp.BCC && !this.Mp.canStandAndNoBlockSlope(num3, num4))
				{
					M2Pt pointPuts = this.Mp.getPointPuts(num3, num4, true, false);
					if (pointPuts != null)
					{
						this.C = pointPuts.getSideBcc(this.Mp, num3, num4, this.check_aim);
					}
				}
				if (this.C == null)
				{
					M2BCCPosChecker.ABuf.Clear();
					m2BlockColliderContainer.getNear(num, num2, 1f, 1f, (int)this.check_aim, M2BCCPosChecker.ABuf, true, false, 0f);
					this.C = M2BlockColliderContainer.getNearest(num, num2, M2BCCPosChecker.ABuf);
				}
			}
			return this.C;
		}

		private int gen_id;

		protected IPosLitener PosLsn;

		protected M2BlockColliderContainer.BCCLine C;

		protected Map2d Mp;

		public AIM check_aim = AIM.B;

		public bool find_lift;

		public static List<M2BlockColliderContainer.BCCLine> ABuf;
	}
}
