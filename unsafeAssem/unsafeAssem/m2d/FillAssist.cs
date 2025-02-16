using System;
using System.Collections.Generic;
using Better;
using XX;

namespace m2d
{
	public class FillAssist
	{
		public FillAssist(M2MapLayer _Lay, FillAssist.FnFillAssistPosInput _fnInput)
		{
			this.Lay = _Lay;
			this.Mp = this.Lay.Mp;
			this.fnInput = _fnInput;
		}

		public virtual bool executePos(int px, int py)
		{
			this.executed = false;
			FillAssist.FillChecker fillChecker = new FillAssist.FillChecker(px, py);
			int clms = this.Mp.clms;
			int rows = this.Mp.rows;
			if (this.OFillPos == null)
			{
				this.OFillPos = new BDic<uint, bool>();
				this.APre = new List<FillAssist.FillChecker>(10);
				this.ANex = new List<FillAssist.FillChecker>(10);
			}
			this.OFillPos.Clear();
			this.APre.Clear();
			this.ANex.Clear();
			this.BaseChip = this.getFirstChip(px, py, ref fillChecker, this.APre);
			this.APre.Add(fillChecker);
			bool flag = this.BaseChip == null;
			List<FillAssist.FillChecker> list = null;
			bool flag2 = true;
			while (list == null || list.Count > 0)
			{
				list = this.ANex;
				this.ANex.Clear();
				int count = this.APre.Count;
				for (int i = 0; i < count; i++)
				{
					FillAssist.FillChecker fillChecker2 = this.APre[i];
					for (int j = -1; j <= fillChecker2.rows; j++)
					{
						bool flag3 = j == -1 || j == fillChecker2.rows;
						for (int k = -1; k <= fillChecker2.clms; k++)
						{
							bool flag4 = k == -1 || k == fillChecker2.clms;
							if (!(flag2 ? (flag3 || flag4) : (flag3 && flag4)))
							{
								int num = fillChecker2.x + k;
								int num2 = fillChecker2.y + j;
								uint num3 = Map2d.xy2b(num, num2);
								if (!this.OFillPos.ContainsKey(num3))
								{
									if (!X.BTW(0f, (float)num, (float)clms) || !X.BTW(0f, (float)num2, (float)rows) || !this.pointUseable(num, num2, flag))
									{
										this.OFillPos[num3] = true;
									}
									else
									{
										M2Pt pointPuts = this.Mp.getPointPuts(num, num2, flag, false);
										if (pointPuts != null)
										{
											bool flag5 = false;
											M2Chip m2Chip = null;
											for (int l = pointPuts.count - 1; l >= 0; l--)
											{
												M2Chip c = pointPuts.GetC(l);
												if (c != null && this.chipConsiderable(num, num2, c))
												{
													flag5 = true;
													if (flag)
													{
														break;
													}
													if (this.fillUseable(num, num2, fillChecker2, c))
													{
														m2Chip = c;
														break;
													}
												}
											}
											if (m2Chip != null)
											{
												list.Add(this.inputFiller(new FillAssist.FillChecker(m2Chip, this.BaseChip)));
											}
											else if (flag && !flag5)
											{
												list.Add(this.inputFiller(new FillAssist.FillChecker(num, num2)));
											}
										}
									}
								}
							}
						}
					}
				}
				this.APre.Clear();
				this.APre.AddRange(list);
				flag2 = false;
			}
			return true;
		}

		protected virtual M2Chip getFirstChip(int px, int py, ref FillAssist.FillChecker Ck, List<FillAssist.FillChecker> AFirstCheck)
		{
			M2Pt pointPuts = this.Mp.getPointPuts(px, py, true, false);
			if (pointPuts.count > 0)
			{
				M2Chip m2Chip = null;
				for (int i = pointPuts.count - 1; i >= 0; i--)
				{
					M2Chip c = pointPuts.GetC(i);
					if (c != null && this.chipConsiderable(px, py, c))
					{
						m2Chip = c;
						break;
					}
				}
				if (m2Chip != null)
				{
					Ck = new FillAssist.FillChecker(this.BaseChip, null);
					return m2Chip;
				}
			}
			return null;
		}

		protected FillAssist.FillChecker inputFiller(FillAssist.FillChecker _Ck)
		{
			for (int i = 0; i < _Ck.rows; i++)
			{
				for (int j = 0; j < _Ck.clms; j++)
				{
					int num = _Ck.x + j;
					int num2 = _Ck.y + i;
					if (num < this.Mp.clms && num >= 0 && num2 < this.Mp.rows && num2 >= 0)
					{
						uint num3 = Map2d.xy2b(num, num2);
						if (!this.OFillPos.ContainsKey(num3))
						{
							this.OFillPos[num3] = true;
							if (this.fnInput(num, num2, this.BaseChip, _Ck))
							{
								this.executed = true;
							}
						}
					}
				}
			}
			return _Ck;
		}

		protected virtual bool chipConsiderable(int x, int y, M2Chip __C)
		{
			return __C.Lay == this.Lay;
		}

		protected virtual bool pointUseable(int x, int y, bool fill_to_empty)
		{
			return true;
		}

		protected virtual bool fillUseable(int x, int y, FillAssist.FillChecker SrcCk, M2Chip __C)
		{
			return SrcCk.isSame(__C);
		}

		private BDic<uint, bool> OFillPos;

		public bool executed;

		public int pointer_width = 1;

		public int pointer_height = 1;

		public readonly Map2d Mp;

		public readonly M2MapLayer Lay;

		private M2Chip BaseChip;

		private List<FillAssist.FillChecker> APre;

		private List<FillAssist.FillChecker> ANex;

		public FillAssist.FnFillAssistPosInput fnInput;

		public struct FillChecker
		{
			public FillChecker(int _x, int _y)
			{
				this.x = _x;
				this.y = _y;
				this.clms = (this.rows = 1);
				this.Cp = null;
				this.fill_chip_id = 0U;
				this.pat = false;
			}

			public FillChecker(M2Chip _Cp, M2Chip BaseChip = null)
			{
				this.Cp = _Cp;
				this.x = this.Cp.mapx;
				this.y = this.Cp.mapy;
				this.clms = this.Cp.clms;
				this.rows = this.Cp.rows;
				this.pat = false;
				if (BaseChip == null)
				{
					BaseChip = this.Cp;
				}
				if (BaseChip.pattern != 0U)
				{
					this.fill_chip_id = BaseChip.pattern;
					this.pat = true;
					return;
				}
				this.fill_chip_id = BaseChip.Img.chip_id;
			}

			public bool isSame(M2Chip Cp)
			{
				if (this.fill_chip_id == 0U)
				{
					return Cp == null;
				}
				if (!this.pat)
				{
					return Cp.pattern == 0U && Cp.Img.chip_id == this.fill_chip_id;
				}
				return Cp.pattern == this.fill_chip_id;
			}

			public int x;

			public int y;

			public int clms;

			public int rows;

			public M2Chip Cp;

			private uint fill_chip_id;

			private bool pat;
		}

		public delegate bool FnFillAssistPosInput(int x, int y, M2Chip TriggerChip, FillAssist.FillChecker TriggerChecker);
	}
}
