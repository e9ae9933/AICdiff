using System;
using XX;

namespace m2d
{
	public class M2LpFootTypeReplacer : M2LabelPoint, IBCCEventListener
	{
		public M2LpFootTypeReplacer(string __key, int _i, M2MapLayer L)
			: base(__key, _i, L)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			M2ChipImage m2ChipImage = null;
			if (TX.valid(this.comment))
			{
				string si = new META(this.comment).GetSi(0, "img");
				if (TX.valid(si))
				{
					m2ChipImage = this.Mp.M2D.IMGS.Get(si);
				}
			}
			this.TargetCp = null;
			int num = X.Mx(0, this.mapx);
			int num2 = X.Mn(this.Mp.clms, this.mapx + this.mapw);
			int num3 = X.Mx(0, this.mapy);
			int num4 = X.Mn(this.Mp.rows, this.mapy + this.maph);
			for (int i = num; i < num2; i++)
			{
				for (int j = num3; j < num4; j++)
				{
					M2Pt pointPuts = this.Mp.getPointPuts(i, j, false, false);
					if (pointPuts != null)
					{
						int count = pointPuts.count;
						for (int k = 0; k < count; k++)
						{
							M2Puts m2Puts = pointPuts[k];
							if (m2ChipImage != null)
							{
								if (m2Puts.Img == m2ChipImage)
								{
									this.TargetCp = m2Puts;
									break;
								}
							}
							else if (m2Puts.Lay == this.Lay && !TX.noe(m2Puts.getMeta().GetSi(0, "foot_type")))
							{
								this.TargetCp = m2Puts;
								break;
							}
						}
						if (this.TargetCp != null)
						{
							break;
						}
					}
				}
				if (this.TargetCp != null)
				{
					break;
				}
			}
			if (this.TargetCp != null)
			{
				this.Mp.addBCCEventListener(this);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			this.Mp.remBCCEventListener(this);
		}

		public void BCCInitializing(M2BlockColliderContainer BCC)
		{
		}

		public bool isBCCListenerActive(M2BlockColliderContainer.BCCLine BCC)
		{
			if (this.TargetCp != null && BCC.is_map_bcc && base.isContainingMapXy(BCC.x, BCC.y, BCC.right, BCC.bottom, 0f))
			{
				BCC.fixFootStampChip(this.TargetCp, -1);
			}
			return false;
		}

		public void BCCtouched(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
		}

		public bool runBCCEvent(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
			return false;
		}

		private M2Puts TargetCp;
	}
}
