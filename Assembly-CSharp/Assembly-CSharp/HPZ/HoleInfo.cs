using System;
using System.Collections.Generic;
using XX;

namespace HPZ
{
	public sealed class HoleInfo
	{
		public int pt_count
		{
			get
			{
				int num = this.Apt_lit_count.Length;
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					num2 += this.Apt_lit_count[i];
				}
				return num2;
			}
		}

		public HoleInfo(int _index, int clms, int rows)
		{
			this.index = _index;
			this.AAPt = new HoleInfo.PtInfo[clms, rows];
			this.Apt_lit_count = new int[HoleInfo.Opt.lit_count];
		}

		public void clear()
		{
			for (int i = 0; i < HoleInfo.Opt.hole_clms; i++)
			{
				for (int j = 0; j < HoleInfo.Opt.hole_rows; j++)
				{
					this.AAPt[i, j] = null;
				}
			}
			this.same_exists = false;
			X.ALL0(this.Apt_lit_count);
			this.hilighted_count = (this.slided = (this.looped_lit = (this.appeared_value = 0)));
			this.Aanswerable = (this.Aanswerable_void = null);
		}

		public int addValueAt(ZLit Lit, int x, int y)
		{
			bool flag = !X.BTW(0f, (float)x, (float)HoleInfo.Opt.hole_clms) || !X.BTW(0f, (float)y, (float)HoleInfo.Opt.hole_rows);
			if (flag)
			{
				if (!Lit.checkAllocLoop(ref x, ref y))
				{
					return 0;
				}
				x = (x + HoleInfo.Opt.hole_clms * 8) % HoleInfo.Opt.hole_clms;
				y = (y + HoleInfo.Opt.hole_rows * 8) % HoleInfo.Opt.hole_rows;
			}
			if (Lit.getFlipX(this.index))
			{
				x = HoleInfo.Opt.hole_clms - 1 - x;
			}
			if (Lit.getFlipY(this.index))
			{
				y = HoleInfo.Opt.hole_rows - 1 - y;
			}
			HoleInfo.PtInfo ptInfo = this.AAPt[x, y];
			if (ptInfo != null)
			{
				if (ptInfo.isContains(Lit))
				{
					return 0;
				}
				if (!HoleInfo.Opt.alloc_adding)
				{
					return -1;
				}
			}
			if (ptInfo == null)
			{
				HoleInfo.PtInfo ptInfo2 = (this.AAPt[x, y] = new HoleInfo.PtInfo());
			}
			this.looped_lit |= (flag ? (1 << Lit.id) : 0);
			this.Apt_lit_count[Lit.id]++;
			this.AAPt[x, y].plot(Lit, flag);
			return 1;
		}

		public bool plotFinalize(ZLit[] ALt)
		{
			this.slided += HoleInfo.Opt.slide_variable;
			this.hilighted_count = 0;
			this.appeared_value = 0;
			for (int i = 0; i < HoleInfo.Opt.lit_count; i++)
			{
				if (!HoleInfo.Opt.force_progress && this.Apt_lit_count[i] == 0)
				{
					return false;
				}
				ALt[i].identic_board |= 1 << this.index;
			}
			int num = (1 << HoleInfo.Opt.lit_count) - 1;
			for (int j = 0; j < HoleInfo.Opt.hole_clms; j++)
			{
				for (int k = 0; k < HoleInfo.Opt.hole_rows; k++)
				{
					this.appeared_value |= 1 << this.getValueAt(j, k, false, false);
					this.hilighted_count += ((this.getValueAt(j, k, true, true) > 0) ? 1 : 0);
					HoleInfo.PtInfo ptInfo = this.AAPt[j, k];
					if (ptInfo != null && !ptInfo.identical && num > 0)
					{
						for (int l = 0; l < HoleInfo.Opt.lit_count; l++)
						{
							ZLit zlit = ptInfo.ALit[l];
							if (zlit == null)
							{
								break;
							}
							zlit.identic_board &= ~(1 << this.index);
							num &= ~(1 << l);
						}
					}
				}
			}
			return true;
		}

		public int getValueAt(int x, int y, bool use_mod, bool consider_opt_slide)
		{
			HoleInfo.PtInfo ptInfo = this.AAPt[x, y];
			int num = ((ptInfo == null) ? 0 : ptInfo.val);
			int num2 = HoleInfo.Opt.ptskin_bits.Length;
			if (use_mod || consider_opt_slide)
			{
				num = X.MMX(0, num, num2 - 1);
			}
			if (consider_opt_slide && HoleInfo.Opt.ptskin_bits[num] != 18)
			{
				num = (num + this.slided) % num2;
				while (HoleInfo.Opt.ptskin_bits[num] == 18)
				{
					num = (num + 1) % num2;
				}
			}
			return num;
		}

		public bool suitableForAnswer(HoleInfo[] AInfo, ZLit[] ALt)
		{
			if (HoleInfo.Opt.fix_answer_surface >= 0 && this.index != HoleInfo.Opt.fix_answer_surface)
			{
				return false;
			}
			if (HoleInfo.Opt.need_lit_click > this.hilighted_count || HoleInfo.Opt.need_void_click > HoleInfo.Opt.hole_count - this.hilighted_count)
			{
				return false;
			}
			if (this.pt_count == 0)
			{
				return false;
			}
			if (this.same_exists)
			{
				return false;
			}
			if (!HoleInfo.Opt.alloc_same_board)
			{
				for (int i = this.index + 1; i < AInfo.Length; i++)
				{
					HoleInfo holeInfo = AInfo[i];
					if (i != this.index && !holeInfo.same_exists && holeInfo.isSame(this))
					{
						this.same_exists = (holeInfo.same_exists = true);
					}
				}
				if (this.same_exists)
				{
					return false;
				}
			}
			if (!HoleInfo.Opt.exist_left && !HoleInfo.Opt.exist_right && (this.index <= 0 || this.index >= HoleInfo.Opt.board_count - 1))
			{
				return false;
			}
			bool flag = false;
			for (int j = 0; j < AInfo.Length; j++)
			{
				if (j != this.index && AInfo[j].hilighted_count == this.hilighted_count)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			if (HoleInfo.Opt.reversible_click_count > 0)
			{
				int lit_count = HoleInfo.Opt.lit_count;
				for (int k = 0; k < lit_count; k++)
				{
					if (this.Apt_lit_count[k] != 0 && ALt[k].onlyTurns(this.index))
					{
						return false;
					}
				}
			}
			bool flag2 = true;
			if (this.looped_lit != 0)
			{
				bool flag3 = false;
				for (int l = 0; l < AInfo.Length; l++)
				{
					if (l != this.index && AInfo[l].looped_lit == this.looped_lit)
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					flag2 = false;
				}
			}
			this.Aanswerable = new List<int>(this.hilighted_count);
			if (HoleInfo.Opt.reversible_click_count > 0)
			{
				this.Aanswerable_void = new List<int>(HoleInfo.Opt.hole_count - this.hilighted_count);
			}
			for (int m = 0; m < HoleInfo.Opt.hole_clms; m++)
			{
				for (int n = 0; n < HoleInfo.Opt.hole_rows; n++)
				{
					int valueAt = this.getValueAt(m, n, false, false);
					if (HoleInfo.Opt.ptskin_bits[this.getValueAt(m, n, true, true)] != 18)
					{
						if (valueAt != 0)
						{
							if (HoleInfo.Opt.need_lit_click != 0)
							{
								bool flag4 = false;
								for (int num = 0; num < AInfo.Length; num++)
								{
									if (num != this.index && (AInfo[num].appeared_value & (1 << valueAt)) != 0)
									{
										flag4 = true;
										break;
									}
								}
								if (flag4)
								{
									HoleInfo.PtInfo ptInfo = this.AAPt[m, n];
									if (ptInfo != null && (flag2 || ptInfo.looped_lit <= 0) && (ptInfo.identical || ptInfo.turned_lit == 0))
									{
										bool flag5 = true;
										for (int num2 = 0; num2 < HoleInfo.Opt.lit_count; num2++)
										{
											ZLit zlit = ptInfo.ALit[num2];
											if (zlit == null)
											{
												break;
											}
											if (zlit.cannot_answer)
											{
												flag5 = false;
												break;
											}
											if (HoleInfo.Opt.lit_count <= 1)
											{
												break;
											}
											if (zlit != null && zlit.count_identic_board(this.index, HoleInfo.Opt.board_count) < 2)
											{
												flag5 = false;
												break;
											}
										}
										if (flag5)
										{
											if (ptInfo.turned_lit != 0)
											{
												bool flag6 = false;
												for (int num3 = 0; num3 < HoleInfo.Opt.lit_count; num3++)
												{
													if ((ptInfo.turned_lit & (1 << num3)) != 0 && ALt[num3].onlyTurns(this.index))
													{
														flag6 = true;
														break;
													}
												}
												if (flag6)
												{
													goto IL_03B8;
												}
											}
											this.Aanswerable.Add((m << 8) | n);
										}
									}
								}
							}
						}
						else if (HoleInfo.Opt.need_void_click != 0)
						{
							this.Aanswerable_void.Add((m << 8) | n);
						}
					}
					IL_03B8:;
				}
			}
			return this.Aanswerable.Count >= HoleInfo.Opt.need_lit_click && (HoleInfo.Opt.need_void_click == 0 || (this.Aanswerable_void != null && this.Aanswerable_void.Count >= HoleInfo.Opt.need_void_click));
		}

		public bool isSame(HoleInfo B)
		{
			if (B.hilighted_count != this.hilighted_count)
			{
				return false;
			}
			for (int i = 0; i < HoleInfo.Opt.hole_clms; i++)
			{
				for (int j = 0; j < HoleInfo.Opt.hole_rows; j++)
				{
					if (B.getValueAt(i, j, true, false) != this.getValueAt(i, j, true, false))
					{
						return false;
					}
				}
			}
			return true;
		}

		public static void setOpt(PuzzleOption _Opt)
		{
			HoleInfo.Opt = _Opt;
		}

		public static HoleInfo[] GetContainer(PuzzleOption _Opt, HoleInfo[] ARet)
		{
			HoleInfo.Opt = _Opt;
			if (ARet == null)
			{
				ARet = new HoleInfo[HoleInfo.Opt.board_count];
			}
			for (int i = 0; i < HoleInfo.Opt.board_count; i++)
			{
				if (ARet[i] != null)
				{
					ARet[i].clear();
				}
				else
				{
					ARet[i] = new HoleInfo(i, HoleInfo.Opt.hole_clms, HoleInfo.Opt.hole_rows);
				}
			}
			return ARet;
		}

		public readonly int index;

		private HoleInfo.PtInfo[,] AAPt;

		private int[] Apt_lit_count;

		private List<int> Aanswerable;

		private List<int> Aanswerable_void;

		private int slided;

		private int hilighted_count;

		private int appeared_value;

		private int looped_lit;

		private bool same_exists;

		private static PuzzleOption Opt;

		public sealed class PtInfo
		{
			public void plot(ZLit Lit, bool looped)
			{
				if (this.ALit == null)
				{
					this.ALit = new ZLit[HoleInfo.Opt.lit_count];
				}
				X.pushToEmpty<ZLit>(this.ALit, Lit, 1);
				this.turned_lit |= (Lit.current_turned ? (1 << Lit.id) : 0);
				this.looped_lit |= (looped ? (1 << Lit.id) : 0);
				this.val += Lit.variable;
			}

			public bool isContains(ZLit Lit)
			{
				return X.isinC<ZLit>(this.ALit, Lit) >= 0;
			}

			public int lit_bits
			{
				get
				{
					if (this.ALit == null)
					{
						return 0;
					}
					int num = this.ALit.Length;
					int num2 = 0;
					int num3 = 0;
					while (num3 < num && this.ALit[num3] != null)
					{
						num2 |= 1 << this.ALit[num3].id;
						num3++;
					}
					return num2;
				}
			}

			public bool identical
			{
				get
				{
					return this.ALit.Length == 1 || this.ALit[1] == null;
				}
			}

			public ZLit[] ALit;

			public int turned_lit;

			public int val;

			public int looped_lit;
		}
	}
}
