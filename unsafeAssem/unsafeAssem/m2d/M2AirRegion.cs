using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2AirRegion : M2RegionBase
	{
		public M2AirRegion(Map2d _Mp, M2AirRegionContainer.FnThroughCheck _fnCheck, int px, int py, int _index, int expandFlag = -1)
			: base(px, py, _index)
		{
			this.fnCheck = _fnCheck;
			this.Mp = _Mp;
			this.AoverWRs = new List<M2AirRegion.RegInfo>(0);
			M2AirRegionContainer.DIRPAT dirpat = (M2AirRegionContainer.DIRPAT)((expandFlag < 0) ? 15 : expandFlag);
			int num = ((dirpat == M2AirRegionContainer.DIRPAT.ALL) ? 1 : 2);
			while (num != 0)
			{
				while (dirpat != (M2AirRegionContainer.DIRPAT)0)
				{
					if ((dirpat & M2AirRegionContainer.DIRPAT.L) != (M2AirRegionContainer.DIRPAT)0)
					{
						if (!this.checkCFG(this.x - 1, this.y, this.x, this.b))
						{
							dirpat--;
						}
						else
						{
							this.x--;
						}
					}
					if ((dirpat & M2AirRegionContainer.DIRPAT.T) != (M2AirRegionContainer.DIRPAT)0)
					{
						if (!this.checkCFG(this.x, this.y - 1, this.r, this.y))
						{
							dirpat &= (M2AirRegionContainer.DIRPAT)(-3);
						}
						else
						{
							this.y--;
						}
					}
					if ((dirpat & M2AirRegionContainer.DIRPAT.R) != (M2AirRegionContainer.DIRPAT)0)
					{
						if (!this.checkCFG(this.r, this.y, this.r + 1, this.b))
						{
							dirpat &= (M2AirRegionContainer.DIRPAT)(-5);
						}
						else
						{
							this.r++;
						}
					}
					if ((dirpat & M2AirRegionContainer.DIRPAT.B) != (M2AirRegionContainer.DIRPAT)0)
					{
						if (!this.checkCFG(this.x, this.b, this.r, this.b + 1))
						{
							dirpat &= (M2AirRegionContainer.DIRPAT)(-9);
						}
						else
						{
							this.b++;
						}
					}
				}
				num--;
				if (num > 0 && expandFlag >= 0)
				{
					dirpat = M2AirRegionContainer.DIRPAT.ALL - expandFlag;
				}
			}
		}

		public bool checkCFG(int _x, int _y, int _r, int _b)
		{
			if (_x < 0 || _y < 0 || _r > this.Mp.clms || _b > this.Mp.rows)
			{
				return false;
			}
			for (int i = _y; i < _b; i++)
			{
				for (int j = _x; j < _r; j++)
				{
					if (!this.fnCheck(this.Mp.getConfig(j, i), j, i))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool isTouching(M2AirRegion WR)
		{
			if ((this.x == WR.r || this.r == WR.x) && this.y < WR.b && this.b > WR.y)
			{
				this.touchingPattern = M2AirRegionContainer.DIRPAT.LR;
				return true;
			}
			if ((this.y == WR.b || this.b == WR.y) && this.x < WR.r && this.r > WR.x)
			{
				this.touchingPattern = M2AirRegionContainer.DIRPAT.TB;
				return true;
			}
			return false;
		}

		public bool alreadyOver(M2AirRegion WR)
		{
			int count = this.AoverWRs.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.AoverWRs[i].Reg == WR)
				{
					return true;
				}
			}
			return false;
		}

		public bool alreadyChecked(int region_max, M2AirRegion WR)
		{
			if (this.Checker == null)
			{
				this.Checker = new Bool1024(region_max, null);
			}
			if (WR.Checker == null)
			{
				WR.Checker = new Bool1024(region_max, null);
			}
			return this.Checker[WR.index] || WR.Checker[this.index];
		}

		public M2AirRegion addCheckedRegion(M2AirRegion WR)
		{
			this.Checker[WR.index] = true;
			WR.Checker[this.index] = true;
			return this;
		}

		public M2AirRegion addOverRegion(M2AirRegion WR)
		{
			this.AoverWRs.Add(new M2AirRegion.RegInfo(WR));
			return this;
		}

		public M2AirRegion addOverRegion(M2AirRegion WR, Vector2 Cnct)
		{
			this.AoverWRs.Add(new M2AirRegion.RegInfo(WR, Cnct));
			return this;
		}

		public bool getConnectPointFromStart(M2AirRegion WR, Vector2[] AcnctPt, out Vector2 Ret)
		{
			Ret = Vector2.zero;
			if (AcnctPt == null)
			{
				return false;
			}
			Vector2 vector = AcnctPt[0];
			int count = this.AoverWRs.Count;
			for (int i = 0; i < count; i++)
			{
				M2AirRegion.RegInfo regInfo = this.AoverWRs[i];
				if (regInfo.Reg == WR && regInfo.enable_cnct)
				{
					Vector2 cnct = regInfo.Cnct;
					if (X.Abs(cnct.x - vector.x) == X.Abs(cnct.y - vector.y))
					{
						Ret = cnct;
						return true;
					}
				}
			}
			return false;
		}

		public M2AirRegion getOverRegByIndex(int i)
		{
			return this.AoverWRs[i].Reg;
		}

		public int overWR_count
		{
			get
			{
				return this.AoverWRs.Count;
			}
		}

		public readonly Map2d Mp;

		private M2AirRegionContainer.FnThroughCheck fnCheck;

		private List<M2AirRegion.RegInfo> AoverWRs;

		private Bool1024 Checker;

		public M2AirRegionContainer.DIRPAT touchingPattern;

		private struct RegInfo
		{
			public RegInfo(M2AirRegion _Reg)
			{
				this.Reg = _Reg;
				this.Cnct = Vector2.zero;
				this.enable_cnct = false;
			}

			public RegInfo(M2AirRegion _Reg, Vector2 _Cnct)
			{
				this.Reg = _Reg;
				this.Cnct = _Cnct;
				this.enable_cnct = true;
			}

			public M2AirRegion Reg;

			public Vector2 Cnct;

			public bool enable_cnct;
		}
	}
}
