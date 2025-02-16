using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class M2AirRegionContainer : M2RegionContainerBase<M2AirRegion>
	{
		public M2AirRegionContainer(Map2d _Mp, M2AirRegionContainer.FnThroughCheck _fnDefCheck = null)
			: base(_Mp)
		{
			this.Arega = new List<M2AirRegionAdditional>();
			this.fnDefCheck = _fnDefCheck ?? new M2AirRegionContainer.FnThroughCheck(this.cfg_canThrough);
		}

		private bool cfg_canThrough(int mpc, int _x, int _y)
		{
			return CCON.canStand(mpc) && !CCON.isSlope(mpc) && !CCON.isWater(mpc);
		}

		public M2AirRegionContainer recognizeInit(Map2d _Mp, int _sx = 0, int _sy = 0, int _clms = -1, int _rows = -1, M2AirRegionContainer.FnThroughCheck _fnCheck = null)
		{
			this.fnCheck = _fnCheck;
			if (this.fnCheck == null)
			{
				this.fnCheck = this.fnDefCheck;
			}
			this.rcg_state = M2AirRegionContainer.RSTATE_POINT;
			this.rcg_i = 0;
			this.rcg_j = 0;
			this.Mp = _Mp;
			this.sx = _sx;
			this.sy = _sy;
			this.clms = ((_clms < 0) ? this.Mp.clms : _clms);
			this.rows = ((_rows < 0) ? this.Mp.rows : _rows);
			this.Aregs.Clear();
			return this;
		}

		public bool recognize(int repspd = 200)
		{
			while (this.rcg_state != M2AirRegionContainer.RSTATE_END)
			{
				if (this.rcg_state == M2AirRegionContainer.RSTATE_POINT)
				{
					int num = this.rcg_i % this.clms;
					int num2 = this.rcg_i / this.clms;
					if (num2 >= this.rows)
					{
						this.rcg_state = M2AirRegionContainer.RSTATE_COVER;
						this.rcg_i = 0;
						this.rcg_j = this.Aregs.Count;
					}
					else
					{
						num += this.sx;
						num2 += this.sy;
						if (this.fnCheck(this.Mp.getConfig(num, num2), num, num2) && !this.existsReg(num, num2, null))
						{
							M2AirRegion m2AirRegion = new M2AirRegion(this.Mp, this.fnCheck, num, num2, this.Aregs.Count, -1);
							this.Aregs.Add(m2AirRegion);
							this.rcg_i += m2AirRegion.w - 1;
						}
						this.rcg_i++;
					}
				}
				if (this.rcg_state == M2AirRegionContainer.RSTATE_COVER)
				{
					int num2 = this.rcg_i % this.rcg_j;
					int num = this.rcg_i / this.rcg_j;
					if (M2AirRegionContainer.ACornerPt == null)
					{
						M2AirRegionContainer.ACornerPt = new List<Vector2>(2);
					}
					if (num >= this.rcg_j)
					{
						if (this.rcg_j == this.Aregs.Count)
						{
							this.rcg_state = M2AirRegionContainer.RSTATE_END;
							this.rcg_i = (this.rcg_j = 0);
						}
						else
						{
							this.rcg_i = this.Aregs.Count * this.rcg_j;
							this.rcg_j = this.Aregs.Count;
						}
					}
					else
					{
						this.rcg_i++;
						M2AirRegion m2AirRegion2 = this.Aregs[num];
						M2AirRegion m2AirRegion3 = this.Aregs[num2];
						if (m2AirRegion2 == null)
						{
							this.rcg_i = (num + 1) * this.rcg_j;
						}
						else if (num != num2 && m2AirRegion3 != null && !m2AirRegion2.alreadyChecked(this.Aregs.Count, m2AirRegion3))
						{
							m2AirRegion3.addCheckedRegion(m2AirRegion2);
							if (m2AirRegion2.isOnOver(m2AirRegion3))
							{
								m2AirRegion2.addOverRegion(m2AirRegion3);
								m2AirRegion3.addOverRegion(m2AirRegion2);
							}
							else if (this.alloc_corner_touch && this.checkTouchingCorner(m2AirRegion2, m2AirRegion3) != (M2AirRegionContainer.DIRPAT)0)
							{
								m2AirRegion2.addOverRegion(m2AirRegion3, M2AirRegionContainer.ACornerPt[0]);
								m2AirRegion3.addOverRegion(m2AirRegion2, M2AirRegionContainer.ACornerPt[1]);
							}
							else if (m2AirRegion2.isTouching(m2AirRegion3))
							{
								Rect coveringArea = m2AirRegion2.getCoveringArea(m2AirRegion3);
								M2AirRegion m2AirRegion4 = new M2AirRegion(this.Mp, this.fnCheck, (int)(coveringArea.x + coveringArea.width * 0.5f), (int)(coveringArea.y + coveringArea.height * 0.5f), this.Aregs.Count, (int)m2AirRegion2.touchingPattern);
								if (!this.has(m2AirRegion4))
								{
									this.Aregs.Add(m2AirRegion4);
								}
							}
						}
					}
				}
			}
			return this.rcg_state >= M2AirRegionContainer.RSTATE_END;
		}

		public bool recognize_finished
		{
			get
			{
				return this.rcg_state >= M2AirRegionContainer.RSTATE_END;
			}
		}

		public M2AirRegionContainer.DIRPAT checkTouchingCorner(M2AirRegion WR1, M2AirRegion WR2)
		{
			if (WR1.r == WR2.x && WR1.b == WR2.y)
			{
				if (!this.checkTouchingCornerMove(WR1.r - 1, WR1.b - 1, WR2.x, WR2.y))
				{
					return (M2AirRegionContainer.DIRPAT)0;
				}
				return M2AirRegionContainer.DIRPAT.CRB;
			}
			else if (WR1.r == WR2.x && WR1.y == WR2.b)
			{
				if (!this.checkTouchingCornerMove(WR1.r - 1, WR1.y, WR2.x, WR2.b - 1))
				{
					return (M2AirRegionContainer.DIRPAT)0;
				}
				return M2AirRegionContainer.DIRPAT.CTR;
			}
			else if (WR1.x == WR2.r && WR1.b == WR2.y)
			{
				if (!this.checkTouchingCornerMove(WR1.x, WR1.b - 1, WR2.r - 1, WR2.y))
				{
					return (M2AirRegionContainer.DIRPAT)0;
				}
				return M2AirRegionContainer.DIRPAT.CBL;
			}
			else
			{
				if (WR1.x != WR2.r || WR1.y != WR2.b)
				{
					return (M2AirRegionContainer.DIRPAT)0;
				}
				if (!this.checkTouchingCornerMove(WR1.x, WR1.y, WR2.r - 1, WR2.b - 1))
				{
					return (M2AirRegionContainer.DIRPAT)0;
				}
				return M2AirRegionContainer.DIRPAT.CLT;
			}
		}

		private bool checkTouchingCornerMove(int x1, int y1, int x2, int y2)
		{
			if (this.Mp.canThroughXy((float)x1, (float)y1, (float)x2, (float)y2, 0f))
			{
				M2AirRegionContainer.ACornerPt.Clear();
				M2AirRegionContainer.ACornerPt.Add(new Vector2((float)x1, (float)y1));
				M2AirRegionContainer.ACornerPt.Add(new Vector2((float)x2, (float)y2));
				return true;
			}
			return false;
		}

		public bool existsReg(int _x, int _y, List<M2AirRegion> Aret = null)
		{
			for (int i = 0; i < this.Aregs.Count; i++)
			{
				if (this.Aregs[i].isContains(_x, _y))
				{
					return true;
				}
			}
			return false;
		}

		public List<M2AirRegion> getRegByXy(int _x, int _y, List<M2AirRegion> Aret = null)
		{
			bool flag = Aret != null && Aret.Count > 0;
			Aret = Aret ?? new List<M2AirRegion>();
			for (int i = 0; i < this.Aregs.Count; i++)
			{
				if (this.Aregs[i].isContains(_x, _y) && (!flag || Aret.IndexOf(this.Aregs[i]) < 0))
				{
					Aret.Add(this.Aregs[i]);
				}
			}
			return Aret;
		}

		public List<M2AirRegion> getRegByPt(Vector2 _Pt)
		{
			return this.getRegByXy((int)_Pt.x, (int)_Pt.y, null);
		}

		public bool canThrough(int ax, int ay, int bx, int by, M2Mover _Cs = null, M2Mover tgCs = null)
		{
			int count = this.Arega.Count;
			if (count != 0)
			{
				float num = (X.GA01((float)ax, (float)(-(float)ay), (float)bx, (float)(-(float)by)) + 0.5f) / 0.125f;
				num = (float)((int)((num + 1f) % 8f / 2f));
				for (int i = 0; i < count; i++)
				{
					M2AirRegionAdditional m2AirRegionAdditional = this.Arega[i];
					if (!m2AirRegionAdditional.canThrough((int)num) && X.vec_rec_X((float)ax, (float)ay, (float)bx, (float)by, (float)m2AirRegionAdditional.x, (float)m2AirRegionAdditional.y, (float)m2AirRegionAdditional.r, (float)m2AirRegionAdditional.b))
					{
						return false;
					}
				}
			}
			return !(_Cs != null) || !(tgCs != null) || this.objectMoveCheck(ax, ay, bx, by, _Cs, tgCs);
		}

		public void inputMapRegionAdditional(M2AirRegionAdditional[] _Arega)
		{
			this.Arega.Clear();
			for (int i = 0; i < _Arega.Length; i++)
			{
				this.Arega.Add(new M2AirRegionAdditional(_Arega[i].x, _Arega[i].y, _Arega[i].r, _Arega[i].b, i, _Arega[i].direction));
			}
		}

		public bool has(M2RegionBase WR)
		{
			for (int i = 0; i < this.Aregs.Count; i++)
			{
				if (this.Aregs[i].isSame(WR))
				{
					return true;
				}
			}
			return false;
		}

		public BDic<int, M2AirRegion> getNeighborRegions(int iposx, int iposy, int minw = 1, int minh = 1)
		{
			BDic<int, M2AirRegion> bdic = new BDic<int, M2AirRegion>();
			BDic<int, M2AirRegion> bdic2 = new BDic<int, M2AirRegion>();
			bool flag = false;
			List<M2AirRegion> regByXy = this.getRegByXy(iposx, iposy, null);
			int count = regByXy.Count;
			for (int i = 0; i < count; i++)
			{
				M2AirRegion m2AirRegion = regByXy[i];
				if (m2AirRegion.r - m2AirRegion.x >= minw && m2AirRegion.b - m2AirRegion.y >= minh)
				{
					bdic[m2AirRegion.index] = (bdic2[m2AirRegion.index] = m2AirRegion);
					flag = true;
				}
			}
			while (flag)
			{
				flag = false;
				BDic<int, M2AirRegion> bdic3 = new BDic<int, M2AirRegion>();
				foreach (KeyValuePair<int, M2AirRegion> keyValuePair in bdic2)
				{
					M2AirRegion m2AirRegion = keyValuePair.Value;
					int overWR_count = m2AirRegion.overWR_count;
					for (int j = 0; j < overWR_count; j++)
					{
						M2AirRegion overRegByIndex = m2AirRegion.getOverRegByIndex(j);
						if (!bdic.ContainsKey(overRegByIndex.index) && overRegByIndex.r - overRegByIndex.x >= minw && overRegByIndex.b - overRegByIndex.y >= minh)
						{
							bdic3[overRegByIndex.index] = (bdic[overRegByIndex.index] = overRegByIndex);
							flag = true;
						}
					}
				}
				bdic2 = bdic3;
			}
			return bdic;
		}

		public bool objectMoveCheck(int x0, int y0, int x1, int y1, M2Mover _Cs = null, M2Mover tgCs = null)
		{
			return true;
		}

		private readonly List<M2AirRegionAdditional> Arega;

		private readonly M2AirRegionContainer.FnThroughCheck fnDefCheck;

		private M2AirRegionContainer.FnThroughCheck fnCheck;

		public bool alloc_corner_touch;

		private static readonly int RSTATE_POINT = 0;

		private static readonly int RSTATE_COVER = 1;

		private static readonly int RSTATE_END = 3;

		public static readonly int NA_LT = 1;

		public static readonly int NA_TR = 2;

		public static readonly int NA_BL = 4;

		public static readonly int NA_RB = 8;

		public static readonly int NA_ALL = 15;

		public static List<Vector2> ACornerPt = new List<Vector2>();

		private int rcg_state = M2AirRegionContainer.RSTATE_POINT;

		private int rcg_i;

		private int rcg_j;

		private int sx;

		private int sy;

		private int clms;

		private int rows;

		public delegate bool FnThroughCheck(int cfg, int x, int y);

		public enum DIRPAT
		{
			T = 2,
			B = 8,
			L = 1,
			R = 4,
			TB = 10,
			LR = 5,
			ALL = 15,
			CL,
			CT = 32,
			CR = 64,
			CB = 128,
			CLT = 48,
			CTR = 96,
			CRB = 192,
			CBL = 144
		}
	}
}
