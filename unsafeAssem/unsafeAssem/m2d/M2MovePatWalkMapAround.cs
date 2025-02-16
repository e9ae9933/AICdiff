using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	internal class M2MovePatWalkMapAround : M2MovePatWalker
	{
		public M2MovePatWalkMapAround(M2EventItem _Mv)
			: base(_Mv, M2EventItem.MOV_PAT.WALKAROUND_MAP)
		{
			this.movtf = 0f;
			this.need_recalc_deperture = byte.MaxValue;
			this.Mv.initPhysics();
			this.ALockBcc = new List<M2BlockColliderContainer.BCCLine>(2);
			this.walk_speed_x = X.NIXP(0.05f, 0.068f);
		}

		protected override void walkInit()
		{
			this.Dep = null;
			this.need_recalc_deperture = byte.MaxValue;
		}

		private bool calcDeperture(float allow_return_back_ratio = 0.2f)
		{
			M2BlockColliderContainer.BCCLine footBCC = base.Phy.getFootManager().get_FootBCC();
			if (footBCC == null)
			{
				return false;
			}
			this.need_recalc_deperture = 0;
			this.Dep = null;
			this.Walking = footBCC;
			this.movtf = 0f;
			this.t_lock_other_wall = 12f;
			using (ListBuffer<M2BlockColliderContainer.BCCLine>.Pop(0))
			{
				using (BList<M2BlockColliderContainer.BCCHitInfo> blist2 = ListBuffer<M2BlockColliderContainer.BCCHitInfo>.Pop(0))
				{
					if (footBCC.BCC.getConnectedBcc(footBCC, blist2, false, true, true, true, true) == 0)
					{
						this.dep_x = X.NIXP(footBCC.x + 2f, footBCC.right - 2f);
						return true;
					}
					float num = 0f;
					bool flag = false;
					int count = blist2.Count;
					for (int i = 0; i < count; i++)
					{
						M2BlockColliderContainer.BCCHitInfo bcchitInfo = blist2[i];
						float num2;
						if (bcchitInfo.Hit == footBCC || bcchitInfo.Hit == this.From)
						{
							num2 = allow_return_back_ratio;
						}
						else if (this.ALockBcc.Contains(bcchitInfo.Hit))
						{
							num2 = 0.01f;
						}
						else
						{
							num2 = 1f;
							flag = true;
						}
						num += num2;
					}
					if (!flag || num <= 0f)
					{
						this.dep_x = X.NIXP(footBCC.x + 2f, footBCC.right - 2f);
						return true;
					}
					float num3 = X.XORSP() * num;
					M2BlockColliderContainer.BCCHitInfo bcchitInfo2 = default(M2BlockColliderContainer.BCCHitInfo);
					for (int j = 0; j < count; j++)
					{
						M2BlockColliderContainer.BCCHitInfo bcchitInfo3 = blist2[j];
						float num4;
						if (bcchitInfo3.Hit == footBCC || bcchitInfo3.Hit == this.From)
						{
							num4 = allow_return_back_ratio;
						}
						else if (this.ALockBcc.Contains(bcchitInfo3.Hit))
						{
							num4 = 0.01f;
						}
						else
						{
							num4 = 1f;
						}
						if (num3 < num4)
						{
							bcchitInfo2 = bcchitInfo3;
							break;
						}
						num3 -= num4;
					}
					if (!bcchitInfo2.valid)
					{
						bcchitInfo2 = blist2[X.xors(count)];
					}
					if (bcchitInfo2.Hit != footBCC.SideL && bcchitInfo2.Hit != footBCC.SideR)
					{
						float sizex = this.Mv.sizex;
					}
					this.dep_x = bcchitInfo2.x;
					int xd = bcchitInfo2.Hit._xd;
					X.MPF(this.Mv.x < bcchitInfo2.x);
					float shifted_x = bcchitInfo2.Hit.shifted_x;
					float shifted_right = bcchitInfo2.Hit.shifted_right;
					float num5 = ((shifted_x == bcchitInfo2.x) ? shifted_x : ((shifted_right == bcchitInfo2.x) ? shifted_right : (-1000f)));
					if (num5 != -1000f)
					{
						if (xd > 0)
						{
							this.dep_x = X.Mn(this.dep_x, num5 - this.Mv.sizex);
						}
						else
						{
							this.dep_x = X.Mx(this.dep_x, num5 + this.Mv.sizex);
						}
					}
					this.Dep = ((bcchitInfo2.Hit == footBCC) ? null : bcchitInfo2.Hit);
				}
			}
			return false;
		}

		protected override bool walkInner(float fcnt, ref bool moved, ref string dep_walk_pose)
		{
			M2BlockColliderContainer.BCCLine footBCC = base.Phy.getFootManager().get_FootBCC();
			if (footBCC == null)
			{
				if (this.need_recalc_deperture == 255)
				{
					return true;
				}
				this.t_lock_other_wall = 0f;
				this.need_recalc_deperture = 1;
				this.From = null;
				base.setVelocityX(0f);
			}
			else
			{
				if (this.need_recalc_deperture == 255)
				{
					this.need_recalc_deperture = 2;
				}
				if (this.Mv.wallHitted(this.Mv.aim))
				{
					this.need_recalc_deperture = 2;
					X.pushIdentical<M2BlockColliderContainer.BCCLine>(this.ALockBcc, this.Dep);
				}
			}
			if (this.need_recalc_deperture > 0)
			{
				if (footBCC == null)
				{
					return true;
				}
				this.calcDeperture((this.need_recalc_deperture == 2) ? 0.4f : 0.25f);
			}
			M2BlockColliderContainer.BCCLine bccline = this.Dep ?? footBCC;
			if (bccline == null)
			{
				return false;
			}
			float num = this.dep_x - bccline.BCC.base_shift_x;
			float num2 = X.Abs(this.Mv.x - num);
			if (footBCC != this.Dep && num2 >= ((this.Walking != footBCC) ? (0.03f + this.Mv.sizex) : 0.04f))
			{
				moved = true;
				float num3 = (float)X.MPF(this.Mv.x < num);
				base.setVelocityX(num3 * X.Mn(num2, this.walk_speed_x));
				base.setAim((num3 > 0f) ? AIM.R : AIM.L, false);
				if (this.Walking != footBCC)
				{
					this.From = this.Walking;
					this.need_recalc_deperture = 1;
				}
				if (this.t_lock_other_wall > 0f)
				{
					this.t_lock_other_wall = X.Mx(0f, this.t_lock_other_wall - fcnt);
				}
				if (this.movtf > 0f)
				{
					this.movtf += fcnt;
					if (this.movtf >= 300f)
					{
						if (this.Dep == null)
						{
							this.From = null;
						}
						return false;
					}
				}
				return true;
			}
			if (this.ALockBcc.Count > 0 && X.XORSP() < 0.3f)
			{
				this.ALockBcc.Clear();
			}
			if (this.Dep == null)
			{
				this.From = null;
				return false;
			}
			if (footBCC == this.Dep)
			{
				this.From = this.Walking;
			}
			else if (footBCC != null)
			{
				this.From = footBCC;
			}
			this.t_lock_other_wall = 0f;
			base.Phy.getFootManager().rideInitTo(this.Dep, false);
			float num4 = X.MMX(this.Dep.shifted_x, num, this.Dep.shifted_right);
			float num5 = this.Dep.slopeBottomY(num);
			base.Phy.addTranslateStack(num4 - this.Mv.x, num5 - this.Mv.mbottom);
			if (!this.calcDeperture(0f))
			{
				if (this.Dep != null && this.Dep.aim == AIM.B && X.XORSP() < 0.1f)
				{
					this.movtf = 300f - X.NIXP(120f, 180f);
				}
				else
				{
					this.movtf = 0f;
				}
			}
			return true;
		}

		public override IFootable canFootOn(IFootable F)
		{
			if (this.t_lock_other_wall > 0f)
			{
				M2BlockColliderContainer.BCCLine bccline = F as M2BlockColliderContainer.BCCLine;
				if (bccline == null || bccline != this.Walking)
				{
					return null;
				}
			}
			return F;
		}

		public float first_x = -1000f;

		public float dep_x;

		private M2BlockColliderContainer.BCCLine Dep;

		private M2BlockColliderContainer.BCCLine From;

		private M2BlockColliderContainer.BCCLine Walking;

		private List<M2BlockColliderContainer.BCCLine> ALockBcc;

		private float walk_speed_x;

		private byte need_recalc_deperture;

		public float t_lock_other_wall;
	}
}
