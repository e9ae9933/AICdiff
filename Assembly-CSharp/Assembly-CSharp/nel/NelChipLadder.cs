using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class NelChipLadder : NelChip, M2LpPuzzExtender.IExtenderListener
	{
		public NelChipLadder(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.Bcc != null)
			{
				this.Mp.remBCCAdditionalLine(this.Bcc);
				this.Bcc = null;
			}
			this.assigned = false;
		}

		public void SlideDuplicated(M2LpPuzzExtender Lp, M2Chip CenterChip)
		{
			this.assigned = true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (!normal_map || this.assigned)
			{
				return;
			}
			this.assigned = true;
			List<M2Puts> list = null;
			List<M2BlockColliderContainer.M2PtPos> list2 = new List<M2BlockColliderContainer.M2PtPos>(8)
			{
				new M2BlockColliderContainer.M2PtPos(this.mapx, this.mapy, new M2Pt(this, 4))
			};
			M2BorderPt m2BorderPt = new M2BorderPt().Init(this.mapcx, (float)this.mapy);
			M2BorderPt m2BorderPt2 = new M2BorderPt().Init(this.mapcx, (float)this.mapy);
			for (int i = 0; i < 2; i++)
			{
				int num = ((i == 0) ? (-1) : 1);
				int num2 = this.mapy;
				M2BorderPt m2BorderPt3 = ((i == 0) ? m2BorderPt : m2BorderPt2);
				for (;;)
				{
					num2 += num * this.rows;
					if (list != null)
					{
						list.Clear();
					}
					list = this.Mp.getAllPointMetaPutsTo(this.mapx, num2, 1, this.rows, list, Map2d.LayArray2Bits(this.Lay), "ladder");
					if (list == null || list.Count == 0)
					{
						break;
					}
					for (int j = list.Count - 1; j >= 0; j--)
					{
						NelChipLadder nelChipLadder = list[j] as NelChipLadder;
						if (nelChipLadder != null)
						{
							nelChipLadder.assigned = true;
						}
					}
					m2BorderPt3.y += (float)(num * this.rows);
					list2.Add(new M2BlockColliderContainer.M2PtPos((int)this.mapcx, (int)m2BorderPt3.y, new M2Pt(list[0], 4)));
				}
				if (i == 0)
				{
					list2.Reverse();
				}
			}
			m2BorderPt.APt = list2;
			this.Bcc = this.myBCC.addAdditionalLine(m2BorderPt, m2BorderPt2, true);
			this.Bcc.fixFootStampChip(this, this.getConfig(0, 0));
		}

		public M2BlockColliderContainer myBCC
		{
			get
			{
				if (this.AttachCM != null)
				{
					M2BlockColliderContainer bcccon = this.AttachCM.getBCCCon();
					if (bcccon != null)
					{
						return bcccon;
					}
				}
				return this.Mp.BCC;
			}
		}

		public void ExtendChanged(M2LpPuzzExtender Lp, M2Chip CenterChip, AIM aim, int pre_length, int cur_length, bool animation_finished)
		{
			if ((pre_length > cur_length || animation_finished) && this.Bcc != null)
			{
				if (aim == AIM.B)
				{
					int num = CenterChip.mapy + CenterChip.rows - this.mapy + cur_length;
					if (this.Bcc.height != (float)num)
					{
						this.Bcc.height = (float)num;
						this.Bcc.dy = this.Bcc.sy + this.Bcc.height;
						this.Bcc.BCC.expandLiftBounds(this.Bcc);
						this.Mp.recheckBCCCache(null);
						return;
					}
				}
				else if (aim == AIM.T)
				{
					int num2 = this.mapy - CenterChip.mapy + cur_length + this.rows;
					if (this.Bcc.height != (float)num2)
					{
						this.Bcc.height = (float)num2;
						this.Bcc.y = (this.Bcc.sy = this.Bcc.dy - (float)num2);
						this.Bcc.BCC.expandLiftBounds(this.Bcc);
						this.Mp.recheckBCCCache(null);
					}
				}
			}
		}

		private M2BlockColliderContainer.BCCLine Bcc;

		private bool assigned;
	}
}
