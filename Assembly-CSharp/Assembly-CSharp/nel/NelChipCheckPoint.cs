using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelChipCheckPoint : NelChip, ICheckPointListener, NearManager.INearLsnObject
	{
		public NelChipCheckPoint(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
		}

		public virtual bool can_touch_checkpoint
		{
			get
			{
				return true;
			}
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Mp.mode == MAPMODE.NORMAL)
			{
				this.Mp.NM.AssignForCenterPr(this, false, false);
			}
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.Mp.NM != null)
			{
				this.Mp.NM.Deassign(this);
			}
			this.AssignCLp = null;
		}

		public DRect getBounds(M2Mover Mv, DRect Dest)
		{
			if (this.AssignCLp != null)
			{
				return Dest.Set((float)this.AssignCLp.mapx - 1f, (float)this.AssignCLp.mapy - 1f, (float)(this.AssignCLp.mapw + 2), (float)(this.AssignCLp.maph + 2));
			}
			return Dest.Set(this.mleft, this.mtop - 0.5f, (float)this.iwidth * base.rCLEN, (float)this.iheight * base.rCLEN + 1f);
		}

		public bool nearCheck(M2Mover Mv, NearTicket NTk)
		{
			if (this.can_touch_checkpoint && Mv == this.Mp.getKeyPr())
			{
				if (!Mv.hasFoot() && !this.air_assign)
				{
					if (this.Mp.BCC == null)
					{
						return false;
					}
					M2BlockColliderContainer.BCCLine bccline;
					this.Mp.BCC.isFallable(this.mapcx, this.mbottom, 0.5f, 1f, out bccline, true, true, -1f);
					if (bccline == null)
					{
						return true;
					}
					float num = bccline.slopeBottomY(Mv.x, 0f, 0f, true);
					if (X.Abs(this.mbottom - num) > 1.5f)
					{
						return true;
					}
				}
				if ((this.AssignCLp != null) ? Mv.isCovering((float)this.AssignCLp.mapx, (float)(this.AssignCLp.mapx + this.AssignCLp.mapw), (float)this.AssignCLp.mapy, (float)(this.AssignCLp.mapy + this.AssignCLp.maph), 0.5f) : Mv.isCovering(this.mleft, this.mright, this.mtop, this.mbottom, 0.5f))
				{
					PR pr = Mv as PR;
					if (pr == null || (!this.air_assign && !pr.isManipulateState()))
					{
						return true;
					}
					bool flag = true;
					bool flag2 = false;
					if (this.Mp.floort >= this.t_mp_cure_recharge)
					{
						this.t_mp_cure_recharge = this.Mp.floort + 240f;
						flag2 = pr.cureMpNotHunger(true);
					}
					if ((this.Mp.M2D as NelM2DBase).CheckPoint.get_CurCheck() == this)
					{
						flag = flag2;
						if (flag)
						{
							(this.Mp.M2D as NelM2DBase).CheckPoint.removeCheckPointManual(this);
						}
					}
					if (flag)
					{
						this.initCheckPoint(this.Mp.getKeyPr());
					}
					return true;
				}
			}
			return false;
		}

		protected virtual void initCheckPoint(M2MoverPr Mv)
		{
			(this.Mp.M2D as NelM2DBase).CheckPoint.setCheckPointManual(this);
		}

		public virtual void returnChcekPoint(PR Pr)
		{
			Pr.cureMpNotHunger(true);
		}

		public virtual void activateCheckPoint()
		{
			this.activateToDrawer();
		}

		public Vector2 getReturnPos()
		{
			return new Vector2(this.mapcx, this.mbottom);
		}

		public virtual int getCheckPointPriority()
		{
			return 5;
		}

		public virtual Color32 getDrawEffectPositionAndColor(ref int pixel_x, ref int pixel_y)
		{
			return PlayerCheckPoint.defaultAssignPositionAndColor(this, ref pixel_x, ref pixel_y);
		}

		public virtual bool drawCheckPoint(EffectItem Ef, float pixel_x, float pixel_y, Color32 Col)
		{
			return PlayerCheckPoint.defaultDrawChipPC(Ef, this, pixel_x, pixel_y, Col);
		}

		public M2LabelPoint AssignCLp;

		public float t_mp_cure_recharge;

		public bool air_assign;
	}
}
