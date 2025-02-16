using System;
using System.Collections.Generic;
using XX;

namespace HPZ
{
	public sealed class ZLit
	{
		public ZLit(PuzzleLitOption LOpt, PuzzleOption _Opt)
		{
			this.Opt = _Opt;
			this.movetype = (LMOVE)ZLit.getRandomBits(LOpt.movetype_bits);
			this.shape = (LSHAPE)ZLit.getRandomBits(LOpt.shape_bits);
			this.movestep = X.MMX(0, ZLit.getRange(LOpt.min_movestep, LOpt.max_movestep), 3);
			if (X.xors(100) < LOpt.reverse_ratio)
			{
				this.movestep *= -1;
			}
			this.cannot_answer = LOpt.cannot_answer;
			this.size = X.Mx(0, ZLit.getRange(LOpt.min_size, LOpt.max_size));
			this.slice = ZLit.getRange(LOpt.min_slice, LOpt.max_slice);
			this.slip = ZLit.getRange(LOpt.min_slip, LOpt.max_slip);
			this.sizestep = ZLit.getRange(LOpt.min_sizestep, LOpt.max_sizestep);
			int range = ZLit.getRange(LOpt.min_sizestep0, LOpt.max_sizestep0);
			if (range >= 0)
			{
				this.sizestep_first_index = (byte)range;
			}
			else
			{
				this.sizestep_first_index = (byte)((this.sizestep == 0 || this.size == 0) ? 0 : X.xors(this.getSizeStepMax()));
			}
			this.flipx_inner = X.xors(100) < LOpt.flipx_ratio;
			this.flipy_inner = X.xors(100) < LOpt.flipy_ratio;
			this.center_void = X.xors(100) < LOpt.center_void_ratio;
			int num = ((LOpt.Aaim == null || LOpt.Aaim.Count == 0) ? (-1) : LOpt.Aaim[X.xors(LOpt.Aaim.Count)]);
			bool flag = this.movetype == LMOVE.BOUNCE || this.movetype == LMOVE.LOOP;
			if (num < 0 && flag && X.xors(100) < LOpt.naname_ratio)
			{
				num = 4 + X.xors(4);
			}
			this.aim = (AIM)((num <= -1) ? X.xors(4) : num);
			if (!flag)
			{
				this.aim %= AIM.LT;
			}
			if (LOpt.Afix_x != null)
			{
				this.posx = LOpt.Afix_x[X.xors(LOpt.Afix_x.Count)];
				if (this.posx >= 0)
				{
					this.posx %= this.Opt.hole_clms;
				}
			}
			if (LOpt.Afix_y != null)
			{
				this.posy = LOpt.Afix_y[X.xors(LOpt.Afix_y.Count)];
				if (this.posy >= 0)
				{
					this.posy %= this.Opt.hole_rows;
				}
			}
			if (this.posx < 0)
			{
				this.posx = ((this.Opt.hole_clms - this.slice * 2 <= 0) ? ((int)((float)(this.Opt.hole_clms - 1) / 2f)) : (this.slice + X.xors(this.Opt.hole_clms - this.slice * 2)));
			}
			if (this.posy < 0)
			{
				this.posy = ((this.Opt.hole_rows - this.slice * 2 <= 0) ? ((int)((float)(this.Opt.hole_rows - 1) / 2f)) : (this.slice + X.xors(this.Opt.hole_rows - this.slice * 2)));
			}
			this.slideBoard(0);
		}

		public ZLit slideBoard(int cnt = -1)
		{
			bool flag = cnt != 0;
			bool flag2 = false;
			if (cnt < 0)
			{
				cnt = X.Abs(this.movestep);
			}
			int num = this.Opt.hole_clms - this.slice * 2;
			int num2 = this.Opt.hole_rows - this.slice * 2;
			if (num <= 0 && this.Opt.hole_clms % 2 == 0)
			{
				this.shape = LSHAPE.ERROR;
			}
			if (num2 <= 0 && this.Opt.hole_rows % 2 == 0)
			{
				this.shape = LSHAPE.ERROR;
			}
			if (num <= 0 || num2 <= 0)
			{
				this.shape = LSHAPE.CENTER;
			}
			if (this.shape != LSHAPE.CENTER)
			{
				int num3 = ((num <= 0) ? (this.Opt.hole_clms / 2) : this.slice);
				int num4 = ((num2 <= 0) ? (this.Opt.hole_rows / 2) : this.slice);
				int num5 = ((num <= 0) ? (this.Opt.hole_clms / 2) : (this.Opt.hole_clms - 1 - this.slice));
				int num6 = ((num2 <= 0) ? (this.Opt.hole_rows / 2) : (this.Opt.hole_rows - 1 - this.slice));
				switch (this.movetype)
				{
				case LMOVE.SLIDE:
					if (this.movestep > 0)
					{
						while (--cnt >= 0)
						{
							int num7 = this.posx + 1;
							this.posx = num7;
							if (num7 > num5)
							{
								this.posx = num3;
								num7 = this.posy + 1;
								this.posy = num7;
								if (num7 > num6)
								{
									this.posy = num4;
								}
								flag2 = true;
							}
						}
					}
					else
					{
						while (--cnt >= 0)
						{
							int num7 = this.posx - 1;
							this.posx = num7;
							if (num7 < num3)
							{
								this.posx = num5;
								num7 = this.posy - 1;
								this.posy = num7;
								if (num7 < num4)
								{
									this.posy = num6;
								}
								flag2 = true;
							}
						}
					}
					break;
				case LMOVE.SLIDEY:
					if (this.movestep > 0)
					{
						while (--cnt >= 0)
						{
							int num7 = this.posy + 1;
							this.posy = num7;
							if (num7 > num6)
							{
								this.posy = num4;
								num7 = this.posx + 1;
								this.posx = num7;
								if (num7 > num5)
								{
									this.posx = num3;
								}
								flag2 = true;
							}
						}
					}
					else
					{
						while (--cnt >= 0)
						{
							int num7 = this.posy - 1;
							this.posy = num7;
							if (num7 < num4)
							{
								this.posy = num6;
								num7 = this.posx - 1;
								this.posx = num7;
								if (num7 < num3)
								{
									this.posx = num5;
								}
								flag2 = true;
							}
						}
					}
					break;
				case LMOVE.BORDER:
				case LMOVE.CORNER:
					for (;;)
					{
						cnt--;
						int num8 = CAim._XD(this.aim, 1);
						int num9 = -CAim._YD(this.aim, 1);
						if (this.aim == AIM.R)
						{
							this.posy = ((this.movestep >= 0) ? num4 : num6);
						}
						else if (this.aim == AIM.L)
						{
							this.posy = ((this.movestep >= 0) ? num6 : num4);
						}
						else if (this.aim == AIM.B)
						{
							this.posx = ((this.movestep >= 0) ? num5 : num3);
						}
						else
						{
							this.posx = ((this.movestep >= 0) ? num3 : num5);
						}
						if (this.movetype == LMOVE.CORNER)
						{
							if (this.aim == AIM.R)
							{
								this.posx = ((this.movestep >= 0) ? num5 : num3);
								num8 = num5 - num3;
							}
							else if (this.aim == AIM.L)
							{
								this.posx = ((this.movestep >= 0) ? num3 : num5);
								num8 = num3 - num5;
							}
							else if (this.aim == AIM.B)
							{
								this.posy = ((this.movestep >= 0) ? num6 : num4);
								num9 = num6 - num4;
							}
							else
							{
								this.posy = ((this.movestep >= 0) ? num4 : num6);
								num9 = num4 - num6;
							}
							if (cnt < 0)
							{
								break;
							}
							if (num > 0)
							{
								this.posx = X.MMX(num3, this.posx + num8, num5);
							}
							if (num2 > 0)
							{
								this.posy = X.MMX(num4, this.posy + num9, num6);
							}
							this.aim = CAim.get_clockwise2(this.aim, this.movestep < 0);
						}
						else
						{
							if (cnt < 0)
							{
								break;
							}
							if (!X.BTWW((float)num3, (float)(this.posx + num8), (float)num5) || !X.BTWW((float)num4, (float)(this.posy + num9), (float)num6))
							{
								cnt++;
								this.aim = CAim.get_clockwise2(this.aim, this.movestep < 0);
								flag2 = true;
							}
							else
							{
								if (num > 0)
								{
									this.posx += num8;
								}
								if (num2 > 0)
								{
									this.posy += num9;
								}
							}
						}
					}
					break;
				case LMOVE.LOOP:
				{
					int num10 = ((num == 1) ? 0 : CAim._XD(this.aim, 1));
					int num11 = ((num2 == 1) ? 0 : (-CAim._YD(this.aim, 1)));
					if (num > 0)
					{
						this.posx = X.MMX(0, this.posx - num3, num - 1);
					}
					if (num2 > 0)
					{
						this.posy = X.MMX(0, this.posy - num4, num2 - 1);
					}
					while (--cnt >= 0)
					{
						if (num > 0)
						{
							this.posx += num10;
							if (this.posx < 0 || this.posx >= num)
							{
								this.posx += num * 8;
								flag2 = true;
							}
							this.posx %= num;
						}
						if (num2 > 0)
						{
							this.posy += num11;
							if (this.posy < 0 || this.posy >= num2)
							{
								this.posy += num2 * 8;
								flag2 = true;
							}
							this.posy %= num2;
						}
					}
					if (num > 0)
					{
						this.posx += num3;
					}
					if (num2 > 0)
					{
						this.posy += num4;
					}
					break;
				}
				case LMOVE.BOUNCE:
				{
					int num12 = ((num == 1) ? 0 : CAim._XD(this.aim, 1));
					int num13 = ((num2 == 1) ? 0 : (-CAim._YD(this.aim, 1)));
					if (num > 0)
					{
						this.posx = X.MMX(0, this.posx - num3, num - 1);
					}
					if (num2 > 0)
					{
						this.posy = X.MMX(0, this.posy - num4, num2 - 1);
					}
					while (--cnt >= 0)
					{
						if (num > 0 && !X.BTW(0f, (float)(this.posx + num12), (float)num))
						{
							num12 *= -1;
							flag2 = true;
							this.aim = CAim.get_aim2(0f, 0f, (float)num12, (float)(-(float)num13), false);
						}
						if (num2 > 0 && !X.BTW(0f, (float)(this.posy + num13), (float)num2))
						{
							num13 *= -1;
							flag2 = true;
							this.aim = CAim.get_aim2(0f, 0f, (float)num12, (float)(-(float)num13), false);
						}
						if (num > 0)
						{
							this.posx += num12;
						}
						if (num2 > 0)
						{
							this.posy += num13;
						}
					}
					if (num > 0)
					{
						this.posx += num3;
					}
					if (num2 > 0)
					{
						this.posy += num4;
					}
					break;
				}
				}
			}
			if (flag && (this.shape != LSHAPE.CENTER && flag2))
			{
				this.stepTurn();
			}
			return this;
		}

		private void stepTurn()
		{
			this.turned_bits |= 1 << this.step_count;
		}

		public bool checkAllocLoop(ref int px, ref int py)
		{
			int num = this.Opt.hole_clms - this.slice * 2;
			int num2 = this.Opt.hole_rows - this.slice * 2;
			int num3 = ((num <= 0) ? (this.Opt.hole_clms / 2) : this.slice);
			int num4 = ((num2 <= 0) ? (this.Opt.hole_rows / 2) : this.slice);
			if (num > 0)
			{
				int hole_clms = this.Opt.hole_clms;
				int num5 = this.slice;
			}
			else
			{
				int num6 = this.Opt.hole_clms / 2;
			}
			if (num2 > 0)
			{
				int hole_rows = this.Opt.hole_rows;
				int num7 = this.slice;
			}
			else
			{
				int num8 = this.Opt.hole_rows / 2;
			}
			int i = px - num3;
			int j = py - num4;
			switch (this.movetype)
			{
			case LMOVE.SLIDE:
				if (num > 0)
				{
					while (i >= num)
					{
						i -= num;
						j++;
						this.stepTurn();
					}
					while (i < 0)
					{
						i += num;
						j--;
						this.stepTurn();
					}
				}
				if (num2 > 0 && (j < 0 || j >= num2))
				{
					j += num2 * 8;
					this.stepTurn();
				}
				if (num > 0)
				{
					px = (i + num * 8) % num + num3;
				}
				if (num2 > 0)
				{
					py = (j + num2 * 8) % num2 + num4;
				}
				return true;
			case LMOVE.SLIDEY:
				if (num2 > 0)
				{
					while (j >= num2)
					{
						j -= num2;
						i++;
						this.stepTurn();
					}
					while (j < 0)
					{
						j += num2;
						i--;
						this.stepTurn();
					}
				}
				if (num > 0 && (i < 0 || i >= num))
				{
					i += num * 8;
					this.stepTurn();
				}
				if (num > 0)
				{
					px = (i + num * 8) % num + num3;
				}
				if (num2 > 0)
				{
					py = (j + num2 * 8) % num2 + num4;
				}
				return true;
			case LMOVE.LOOP:
				if (CAim._XD(this.aim, 1) == 0 && !X.BTW(0f, (float)i, (float)num))
				{
					return this.Opt.alloc_loop;
				}
				if (CAim._YD(this.aim, 1) == 0 && !X.BTW(0f, (float)j, (float)num2))
				{
					return this.Opt.alloc_loop;
				}
				if (i < 0 && num > 0)
				{
					i += num * 8;
					this.stepTurn();
				}
				if (j < 0 && num2 > 0)
				{
					j += num2 * 8;
					this.stepTurn();
				}
				if ((i >= num && num > 0) || (j >= num2 && num2 > 0))
				{
					this.stepTurn();
				}
				if (num > 0)
				{
					px = (i + num * 8) % num + num3;
				}
				if (num2 > 0)
				{
					py = (j + num2 * 8) % num2 + num4;
				}
				return true;
			}
			return this.Opt.alloc_loop;
		}

		public int[] getMem()
		{
			return new int[]
			{
				this.posx,
				this.posy,
				(int)this.aim,
				this.movestep
			};
		}

		public void setMem(int[] M)
		{
			this.posx = M[0];
			this.posy = M[1];
			this.aim = (AIM)M[2];
			this.movestep = M[3];
		}

		public static int getRandomBits(uint b)
		{
			List<int> list = new List<int>(16);
			int num = 64;
			for (int i = 0; i < num; i++)
			{
				int num2 = 1 << i;
				if (((ulong)b & (ulong)((long)num2)) != 0UL)
				{
					list.Add(i);
				}
				if ((long)num2 > (long)((ulong)b))
				{
					break;
				}
			}
			if (list.Count != 0)
			{
				return list[X.xors(list.Count)];
			}
			return 0;
		}

		public static int getRange(int min_val, int max_val)
		{
			if (min_val < max_val)
			{
				return min_val + X.xors(max_val - min_val + 1);
			}
			return min_val;
		}

		public static bool canSkinRot90(LPTSKIN s)
		{
			if (s == LPTSKIN.EMPTY)
			{
				return false;
			}
			if (s >= LPTSKIN.DAIA_FILL)
			{
				s -= 9;
			}
			return s > LPTSKIN.SQUARE;
		}

		public bool applyToHole(HoleInfo Info, bool check_slipping)
		{
			if (this.shape == LSHAPE.ERROR)
			{
				return false;
			}
			int num = 0;
			if (this.shape == LSHAPE.CENTER)
			{
				int num2 = (int)((float)this.Opt.hole_clms / 2f - 0.5f);
				int num3 = (int)((float)this.Opt.hole_rows / 2f - 0.5f);
				int num4 = this.Opt.hole_clms / 2;
				int num5 = this.Opt.hole_rows / 2;
				for (int i = num2; i <= num4; i++)
				{
					for (int j = num3; j <= num5; j++)
					{
						this.addValueAt(Info, i, j, ref num);
					}
				}
				return num > 0;
			}
			if (!this.center_void || this.size <= 0 || this.shape == LSHAPE.O)
			{
				this.addValueAt(Info, this.posx, this.posy, ref num);
			}
			int currentSize = this.getCurrentSize();
			if (currentSize >= 1)
			{
				switch (this.shape)
				{
				case LSHAPE.PLUS:
				{
					for (int k = 1; k <= currentSize; k++)
					{
						for (int l = 0; l < 4; l++)
						{
							this.addValueAt(Info, this.posx + CAim._XD(l, k), this.posy - CAim._YD(l, k), ref num);
						}
					}
					break;
				}
				case LSHAPE.CROSS:
				{
					for (int m = 1; m <= currentSize; m++)
					{
						for (int n = 0; n < 4; n++)
						{
							this.addValueAt(Info, this.posx + CAim._XD(n + 4, m), this.posy - CAim._YD(n + 4, m), ref num);
						}
					}
					break;
				}
				case LSHAPE.DAIA:
				{
					for (int num6 = 1; num6 <= currentSize; num6++)
					{
						int num7 = num6;
						for (int num8 = 0; num8 < 4; num8++)
						{
							int num9 = CAim._XD(num8, num6);
							int num10 = CAim._YD(num8, num6);
							for (int num11 = 0; num11 < num7; num11++)
							{
								this.addValueAt(Info, this.posx + num9, this.posy - num10, ref num);
								num9 += ((num8 == 0 || num8 == 1) ? 1 : (-1));
								num10 += ((num8 == 1 || num8 == 2) ? (-1) : 1);
							}
						}
					}
					break;
				}
				case LSHAPE.SQUARE:
				{
					for (int num12 = 1; num12 <= currentSize; num12++)
					{
						int num13 = num12 + 1;
						for (int num14 = 0; num14 < 4; num14++)
						{
							int num15 = CAim._XD(num14 + 4, num12);
							int num16 = CAim._YD(num14 + 4, num12);
							for (int num17 = 0; num17 < num13; num17++)
							{
								this.addValueAt(Info, this.posx + num15, this.posy - num16, ref num);
								num15 += ((num14 == 0) ? 1 : ((num14 == 3) ? (-1) : 0));
								num16 += ((num14 == 1) ? (-1) : ((num14 == 2) ? 1 : 0));
							}
						}
					}
					break;
				}
				case LSHAPE.I:
				{
					for (int num18 = 1; num18 <= currentSize; num18++)
					{
						for (int num19 = 1; num19 < 4; num19 += 2)
						{
							this.addValueAt(Info, this.posx + CAim._XD(num19, num18), this.posy - CAim._YD(num19, num18), ref num);
						}
					}
					break;
				}
				case LSHAPE.O:
				{
					for (int num20 = 1; num20 <= currentSize; num20++)
					{
						int num21 = this.posx + num20;
						int num22 = this.posy + num20;
						this.addValueAt(Info, num21, num22, ref num);
						for (int num23 = 1; num23 <= num20; num23++)
						{
							this.addValueAt(Info, num21, num22 - num23, ref num);
							this.addValueAt(Info, num21 - num23, num22, ref num);
						}
					}
					break;
				}
				}
			}
			if (check_slipping && this.slip != 0)
			{
				int num24 = X.Abs(this.slip);
				int[] mem = this.getMem();
				this.movestep = X.MPF(num24 > 0) * ((this.movestep > 0) ? 1 : ((this.movestep < 0) ? (-1) : 0));
				for (int num25 = 0; num25 < num24; num25++)
				{
					this.slideBoard(1);
					if (!this.applyToHole(Info, false))
					{
						return false;
					}
				}
				this.setMem(mem);
			}
			if (check_slipping)
			{
				this.step_count++;
				this.slideBoard(-1);
			}
			return num > 0;
		}

		public int getCurrentSize()
		{
			if (this.sizestep == 0)
			{
				return this.size;
			}
			int sizeStepMax = this.getSizeStepMax();
			int num = ((int)this.sizestep_first_index + sizeStepMax * 8 + ((this.sizestep > 0) ? this.step_count : (sizeStepMax - 1 - this.step_count))) % sizeStepMax;
			if (X.Abs(this.sizestep) >= 2)
			{
				return num;
			}
			if (num >= this.size)
			{
				return this.size - (num - this.size);
			}
			return num;
		}

		public int getSizeStepMax()
		{
			if (this.sizestep == 0 || this.size <= 0)
			{
				return 1;
			}
			if (X.Abs(this.sizestep) >= 2)
			{
				return this.size + 1;
			}
			return this.size * 2;
		}

		private int addValueAt(HoleInfo Info, int x, int y, ref int ret)
		{
			int num = Info.addValueAt(this, x, y);
			if (num < 0 || ret < 0)
			{
				ret = -1;
			}
			else
			{
				ret = ((num + ret > 0) ? 1 : 0);
			}
			return ret;
		}

		public int pos_index
		{
			get
			{
				return this.posx + this.posy * this.Opt.hole_clms;
			}
		}

		public bool current_turned
		{
			get
			{
				return this.step_count != 0 && (this.turned_bits & (1 << this.step_count - 1)) != 0;
			}
		}

		public bool getFlipX(int board)
		{
			return (this.flipx_inner ? (-1) : 1) * (this.Opt.getFlipX(board) ? (-1) : 1) < 0;
		}

		public bool getFlipY(int board)
		{
			return (this.flipy_inner ? (-1) : 1) * (this.Opt.getFlipY(board) ? (-1) : 1) < 0;
		}

		public int count_identic_board(int ignore, int board_max)
		{
			int num = 0;
			for (int i = 0; i < board_max; i++)
			{
				if (i != ignore && (this.identic_board & (1 << i)) != 0)
				{
					num++;
				}
			}
			return num;
		}

		public bool onlyTurns(int step)
		{
			return step != 0 && this.turned_bits == 1 << step - 1;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"ZLit:: ",
				this.posx.ToString(),
				",",
				this.posy.ToString(),
				" <",
				this.movetype.ToString(),
				(this.size > 1) ? ("/" + this.shape.ToString() + "." + this.size.ToString()) : "",
				">"
			});
		}

		public int variable = 1;

		public LMOVE movetype;

		public LSHAPE shape;

		public int movestep = 1;

		private int size = 1;

		public int slice;

		public int slip;

		public int sizestep;

		private byte sizestep_first_index;

		private bool flipx_inner;

		private bool flipy_inner;

		public bool center_void;

		public AIM aim;

		public int posx = -1;

		public int posy = -1;

		public PuzzleOption Opt;

		public int turned_bits;

		public int step_count;

		public int id;

		public bool cannot_answer;

		public int identic_board;
	}
}
