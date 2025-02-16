using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2LabelPointCheck : NelLp, NearManager.INearLsnObject, ICheckPointListener
	{
		public M2LabelPointCheck(string __key, int _i, M2MapLayer L)
			: base(__key, 0, L)
		{
		}

		public override void initActionPre()
		{
			List<M2Puts> allPointMetaPutsTo = this.Mp.getAllPointMetaPutsTo(this.mapx, this.mapy, this.mapw, this.maph, null, Map2d.LayArray2Bits(this.Lay), "check_point", "hiddencheck");
			META meta = new META(this.comment);
			this.air_assign = meta.GetB("air_assign", false);
			bool flag = false;
			int num = 0;
			if (allPointMetaPutsTo != null)
			{
				for (int i = allPointMetaPutsTo.Count - 1; i >= 0; i--)
				{
					NelChipCheckPoint nelChipCheckPoint = allPointMetaPutsTo[i] as NelChipCheckPoint;
					if (nelChipCheckPoint != null)
					{
						nelChipCheckPoint.AssignCLp = this;
						nelChipCheckPoint.air_assign = this.air_assign;
					}
					else if (!flag)
					{
						M2Puts m2Puts = allPointMetaPutsTo[i];
						int i2 = m2Puts.Img.Meta.GetI("hiddencheck", 0, 0);
						if (i2 > num)
						{
							flag = true;
							num = i2;
							this.CpJumpTo = m2Puts;
							if (this.Mp.NM != null)
							{
								this.Mp.NM.AssignForCenterPr(this, false, false);
							}
						}
					}
				}
			}
		}

		public override void closeAction(bool when_map_close)
		{
			if (when_map_close)
			{
				this.CpJumpTo = null;
			}
			if (this.Mp.NM != null)
			{
				this.Mp.NM.Deassign(this);
			}
			base.closeAction(when_map_close);
		}

		public DRect getBounds(M2Mover Mv, DRect Dest)
		{
			return Dest.Set((float)this.mapx, (float)this.mapy, (float)this.mapw, (float)this.maph);
		}

		public bool nearCheck(M2Mover Mv, NearTicket NTk)
		{
			if (base.nM2D.CheckPoint.get_CurCheck() != this && Mv == this.Mp.getKeyPr() && Mv.isCovering((float)this.mapx, (float)(this.mapx + this.mapw), (float)this.mapy, (float)(this.mapy + this.maph), 1f))
			{
				if (!Mv.hasFoot() && !this.air_assign)
				{
					return true;
				}
				base.nM2D.CheckPoint.setCheckPointManual(this);
			}
			return true;
		}

		public void returnChcekPoint(PR Pr)
		{
			if (this.CpJumpTo != null && !this.CpJumpTo.Img.Meta.invisible)
			{
				Pr.cureMpNotHunger(true);
			}
		}

		public int getCheckPointPriority()
		{
			return 5;
		}

		public Color32 getDrawEffectPositionAndColor(ref int pixel_x, ref int pixel_y)
		{
			return MTRX.ColTrnsp;
		}

		public void activateCheckPoint()
		{
		}

		public bool drawCheckPoint(EffectItem Ef, float pixe_x, float pixel_y, Color32 Col)
		{
			return false;
		}

		public Vector2 getReturnPos()
		{
			if (this.CpJumpTo != null)
			{
				return new Vector2(this.CpJumpTo.mapcx, this.CpJumpTo.mapcy);
			}
			return new Vector2(base.mapfocx, base.mapfocy);
		}

		private bool air_assign;

		public M2Puts CpJumpTo;
	}
}
