using System;
using System.Collections.Generic;
using HPZ;
using XX;

namespace nel
{
	public class HpzSimulator
	{
		public HpzSimulator(int clms, int rows)
			: this(new PuzzleOption
			{
				board_count = 1,
				hole_clms = clms,
				hole_rows = rows,
				ptskin_bits = HpzSimulator.getSkinCombination(2 + X.xors(3)),
				lit_count = 7 + X.xors(10),
				alloc_adding = true,
				alloc_loop = true,
				alloc_same_board = true,
				force_progress = true
			})
		{
		}

		private static int[] getSkinCombination(int cnt)
		{
			if (cnt <= 1)
			{
				return new int[] { X.xors(18) };
			}
			int num = cnt / 2;
			int num2 = cnt - num;
			List<int> combinationI = X.getCombinationI(9, num, X.xors(), null);
			List<int> combinationI2 = X.getCombinationI(9, num2, X.xors(), null);
			int count = combinationI2.Count;
			for (int i = 0; i < count; i++)
			{
				combinationI.Add(combinationI2[i] + 9);
			}
			return combinationI.ToArray();
		}

		public HpzSimulator(PuzzleOption _Opt)
		{
			this.Opt = _Opt;
			this.AInfo = HoleInfo.GetContainer(this.Opt, null);
			this.single_mode = this.AInfo.Length == 1;
			PuzzleLitOption[] array = this.Opt.createLits().ToArray();
			int num = array.Length;
			this.ALit = new ZLit[num];
			for (int i = 0; i < num; i++)
			{
				array[i].hole_variable = 1 + i % (this.Opt.ptskin_bits.Length - 1);
				this.ALit[i] = array[i].generateFirst(this.Opt, i);
			}
			this.AApt_val = new int[this.Opt.hole_clms, this.Opt.hole_rows];
			if (HpzSimulator.Uni == null)
			{
				HpzSimulator.Uni = new UniKiraDrawer().Dent(0.3f, 0.3f);
				HpzSimulator.Uni.kaku = 4;
			}
			if (this.single_mode)
			{
				this.plotPoints(0);
			}
		}

		public void plotPoints(int info_index = 0)
		{
			HoleInfo.setOpt(this.Opt);
			HoleInfo holeInfo = this.AInfo[info_index];
			if (this.single_mode)
			{
				holeInfo.clear();
			}
			int num = this.ALit.Length;
			for (int i = 0; i < num; i++)
			{
				this.ALit[i].applyToHole(holeInfo, true);
			}
			int num2 = this.Opt.ptskin_bits.Length;
			for (int j = 0; j < this.Opt.hole_clms; j++)
			{
				for (int k = 0; k < this.Opt.hole_rows; k++)
				{
					this.AApt_val[j, k] = this.Opt.ptskin_bits[holeInfo.getValueAt(j, k, false, false) % num2];
				}
			}
		}

		public void drawBL(MeshDrawer Md, float x, float y, float areaw, float areah, float cell_w = 16f)
		{
			float num = cell_w / 2f;
			float num2 = 0f;
			float num3 = 0f;
			if (this.Opt.hole_clms == 1)
			{
				x += areaw / 2f;
			}
			else
			{
				areaw -= cell_w;
				num2 = areaw / (float)(this.Opt.hole_clms - 1);
				x += num;
			}
			if (this.Opt.hole_rows == 1)
			{
				y += areah / 2f;
			}
			else
			{
				areah -= cell_w;
				num3 = areah / (float)(this.Opt.hole_rows - 1);
				y += num;
			}
			float num4 = y;
			for (int i = this.Opt.hole_clms - 1; i >= 0; i--)
			{
				y = num4;
				for (int j = this.Opt.hole_rows - 1; j >= 0; j--)
				{
					LPTSKIN lptskin = (LPTSKIN)this.AApt_val[i, j];
					if (lptskin != LPTSKIN.EMPTY)
					{
						bool flag = lptskin >= LPTSKIN.DAIA_FILL;
						switch (lptskin)
						{
						case LPTSKIN.DAIA:
							Md.Daia2(x, y, num, 1f, false);
							break;
						case LPTSKIN.CIRCLE:
						case LPTSKIN.CIRCLE_FILL:
							Md.Poly(x, y, num, 0f, 12, flag ? 0f : 1f, false, 0f, 0f);
							break;
						case LPTSKIN.SQUARE:
						case LPTSKIN.SQUARE_FILL:
							Md.Poly(x, y, num * 1.125f, 0.7853982f, 4, flag ? 0f : 1f, false, 0f, 0f);
							break;
						case LPTSKIN.TRI_T:
						case LPTSKIN.TRI_T_FILL:
						case LPTSKIN.RIBBON_FILL:
							Md.Poly(x, y, num, 1.5707964f, 3, flag ? 0f : 1f, false, 0f, 0f);
							break;
						case LPTSKIN.TRI_B:
						case LPTSKIN.TRI_B_FILL:
							Md.Poly(x, y, num, -1.5707964f, 3, flag ? 0f : 1f, false, 0f, 0f);
							break;
						case LPTSKIN.SLASH:
							Md.Line(x + num, y + num, x - num, y - num, 1f, false, 0f, 0f);
							break;
						case LPTSKIN.PENTA:
						case LPTSKIN.PENTA_FILL:
							Md.Poly(x, y, num, 1.5707964f, 5, flag ? 0f : 1f, false, 0f, 0f);
							break;
						case LPTSKIN.WAVE:
						case LPTSKIN.DAIA_FILL:
							Md.Daia(x, y, cell_w, cell_w, false);
							break;
						case LPTSKIN.DAIA2:
							Md.Poly(x, y + cell_w * 0.1f, cell_w * 0.4f, 0f, 4, 1f, false, 0f, 0f);
							Md.Poly(x, y + -cell_w * 0.1f, cell_w * 0.4f, 0f, 4, 1f, false, 0f, 0f);
							break;
						case LPTSKIN.UNI_FILL:
						case LPTSKIN.S_FILL:
							HpzSimulator.Uni.Radius(num, num).drawTo(Md, x, y, 1.5707964f, false, 0f, 0f);
							break;
						}
					}
					y += num3;
				}
				x += num2;
			}
		}

		public int hole_clms
		{
			get
			{
				return this.Opt.hole_clms;
			}
		}

		public int hole_rows
		{
			get
			{
				return this.Opt.hole_rows;
			}
		}

		private HoleInfo[] AInfo;

		private PuzzleOption Opt;

		private ZLit[] ALit;

		private int[,] AApt_val;

		private bool single_mode;

		private static UniKiraDrawer Uni;
	}
}
