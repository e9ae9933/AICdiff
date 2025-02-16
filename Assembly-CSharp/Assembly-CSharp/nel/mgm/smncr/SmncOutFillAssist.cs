using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel.mgm.smncr
{
	public class SmncOutFillAssist : FillAssist
	{
		public SmncOutFillAssist(M2LpUiSmnCreator _Lp)
			: base(_Lp.Lay, null)
		{
			this.Lp = _Lp;
			this.fnInput = new FillAssist.FnFillAssistPosInput(this.fillPosInput);
		}

		protected override bool chipConsiderable(int x, int y, M2Chip __C)
		{
			return base.chipConsiderable(x, y, __C) && !this.isStartPoint(__C) && this.Mp.getHardChip(x, y, null, false, false, null) != null;
		}

		public bool isStartPoint(M2Chip __C)
		{
			if (__C == null)
			{
				return false;
			}
			if (__C.Lay != this.Lay)
			{
				return false;
			}
			M2ChipImage img = __C.Img;
			return img.Meta.Get("smnc_out_fill_start") != null || img.Meta.Get("mana_weed") != null;
		}

		public bool isStartPoint(M2Pt P, out M2Chip Target)
		{
			Target = null;
			if (P == null)
			{
				return false;
			}
			for (int i = P.count - 1; i >= 0; i--)
			{
				M2Puts m2Puts = P[i];
				if (m2Puts.Lay == this.Lay && this.isStartPoint(m2Puts as M2Chip))
				{
					Target = m2Puts as M2Chip;
					return true;
				}
			}
			return false;
		}

		public void considerConfig4(M2Pt[,] AAPt)
		{
			int num = X.Mx(this.Lp.mapx, 0);
			int num2 = X.Mx(this.Lp.mapy, 0);
			int num3 = X.Mn(this.Lp.mapx + this.Lp.mapw, this.Mp.clms - 1);
			int num4 = X.Mn(this.Lp.mapy + this.Lp.maph, this.Mp.rows - 1);
			this.Ofilled = new BDic<uint, bool>((num4 - num2) * (num3 - num));
			this.Ofilled.Clear();
			for (int i = num2; i < num4; i++)
			{
				for (int j = num; j < num3; j++)
				{
					M2Pt m2Pt = AAPt[j, i];
					uint num5 = Map2d.xy2b(j, i);
					if (this.isStartPoint(m2Pt, out this.FirstChip) && !this.Ofilled.ContainsKey(num5))
					{
						this.Ofilled[num5] = true;
						this.executePos(j, i);
					}
				}
			}
			for (int k = num2; k < num4; k++)
			{
				for (int l = num; l < num3; l++)
				{
					uint num6 = Map2d.xy2b(l, k);
					if (!this.Ofilled.ContainsKey(num6))
					{
						AAPt[l, k];
						if (this.Mp.getHardChip(l, k, null, false, false, null) == null)
						{
							CCON.calcConfigManual(ref AAPt[l, k], 128);
						}
					}
				}
			}
			this.Ofilled = null;
		}

		protected override M2Chip getFirstChip(int px, int py, ref FillAssist.FillChecker Ck, List<FillAssist.FillChecker> AFirstCheck)
		{
			int dirsI = this.FirstChip.Img.Meta.getDirsI("smnc_out_fill_start", this.FirstChip.rotation, this.FirstChip.flip, 1, -1);
			if (dirsI >= 0)
			{
				Ck = new FillAssist.FillChecker(px + CAim._XD(dirsI, 1), py - CAim._YD(dirsI, 1));
			}
			else
			{
				Ck = new FillAssist.FillChecker(this.FirstChip, null);
			}
			return null;
		}

		private bool fillPosInput(int x, int y, M2Chip TriggerChip, FillAssist.FillChecker TriggerChecker)
		{
			uint num = Map2d.xy2b(x, y);
			this.Ofilled[num] = true;
			return true;
		}

		protected override bool pointUseable(int x, int y, bool fill_to_empty)
		{
			return X.BTW((float)this.Lp.mapx, (float)x, (float)(this.Lp.mapx + this.Lp.mapw)) && X.BTW((float)this.Lp.mapy, (float)y, (float)(this.Lp.mapy + this.Lp.maph));
		}

		public readonly M2LpUiSmnCreator Lp;

		private BDic<uint, bool> Ofilled;

		private M2Chip FirstChip;
	}
}
