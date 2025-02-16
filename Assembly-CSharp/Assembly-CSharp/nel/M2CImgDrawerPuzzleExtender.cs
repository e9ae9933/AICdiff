using System;
using m2d;
using XX;

namespace nel
{
	public class M2CImgDrawerPuzzleExtender : M2CImgDrawerSlide
	{
		public M2CImgDrawerPuzzleExtender(MeshDrawer Md, int _layer, M2Puts _Cp, bool _redraw_flag = false)
			: base(Md, _layer, _Cp, _redraw_flag)
		{
		}

		public void initExtending(AIM _a, int _ex_basex, int _ex_basey, int _id, M2Chip _BaseCp)
		{
			this.aim = _a;
			this.ex_basex = _ex_basex;
			this.ex_basey = _ex_basey;
			this.id = _id;
			this.BaseCp = _BaseCp;
			M2Chip m2Chip = this.Cp as M2Chip;
			if (this.BaseCp != null && m2Chip != null)
			{
				switch (this.aim)
				{
				case AIM.L:
					base.slideTo(m2Chip.drawx2meshx((float)(this.BaseCp.drawx + this.BaseCp.iwidth) + base.CLEN * (float)(this.id - 1)), m2Chip.draw_meshy);
					break;
				case AIM.T:
					base.slideTo(m2Chip.draw_meshx, m2Chip.drawy2meshy((float)(this.BaseCp.drawy + this.BaseCp.iheight) + base.CLEN * (float)(this.id - 1)));
					return;
				case AIM.R:
					base.slideTo(m2Chip.drawx2meshx((float)this.BaseCp.drawx - base.CLEN * (float)this.id), m2Chip.draw_meshy);
					return;
				case AIM.B:
					base.slideTo(m2Chip.draw_meshx, m2Chip.drawy2meshy((float)this.BaseCp.drawy - base.CLEN * (float)this.id));
					return;
				default:
					return;
				}
			}
		}

		public override int redraw(float fcnt)
		{
			if (this.APos == null || this.index_last <= this.index_first)
			{
				return 0;
			}
			if (this.need_reposit_flag && this.BaseCp != null && this.id > 0)
			{
				M2Puts cp = this.Cp;
				int num = this.ASh.Length;
				switch (this.aim)
				{
				case AIM.L:
				{
					float num2 = this.BaseCp.drawx2meshx((float)this.ex_basex * base.CLEN + (float)this.BaseCp.getShift()[0]) + (float)(this.BaseCp.iwidth / 2);
					for (int i = 0; i < num; i++)
					{
						if (this.APos[i].x + this.ASh[i].x > num2)
						{
							this.ASh[i].x = num2 - this.APos[i].x;
						}
					}
					break;
				}
				case AIM.T:
				{
					float num2 = this.BaseCp.drawy2meshy((float)this.ex_basey * base.CLEN + (float)this.BaseCp.getShift()[1]) - (float)(this.BaseCp.iheight / 2);
					for (int j = 0; j < num; j++)
					{
						if (this.APos[j].y + this.ASh[j].y < num2)
						{
							this.ASh[j].y = num2 - this.APos[j].y;
						}
					}
					break;
				}
				case AIM.R:
				{
					float num2 = this.BaseCp.drawx2meshx((float)this.ex_basex * base.CLEN + (float)this.BaseCp.getShift()[0]) - (float)(this.BaseCp.iwidth / 2);
					for (int k = 0; k < num; k++)
					{
						if (this.APos[k].x + this.ASh[k].x < num2)
						{
							this.ASh[k].x = num2 - this.APos[k].x;
						}
					}
					break;
				}
				case AIM.B:
				{
					float num2 = this.BaseCp.drawy2meshy((float)this.ex_basey * base.CLEN + (float)this.BaseCp.getShift()[1]) + (float)(this.BaseCp.iheight / 2);
					for (int l = 0; l < num; l++)
					{
						if (this.APos[l].y + this.ASh[l].y > num2)
						{
							this.ASh[l].y = num2 - this.APos[l].y;
						}
					}
					break;
				}
				}
			}
			return base.redraw(fcnt);
		}

		private int id;

		private AIM aim;

		private int ex_basex;

		private int ex_basey;

		private M2Chip BaseCp;
	}
}
