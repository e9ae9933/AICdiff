using System;
using System.Collections.Generic;
using XX;

namespace HPZ
{
	public sealed class PuzzleLitOption
	{
		public PuzzleLitOption(PuzzleLitOption L = null)
		{
			if (L != null)
			{
				this.movetype_bits = L.movetype_bits;
				this.shape_bits = L.shape_bits;
				this.min_movestep = L.min_movestep;
				this.max_movestep = L.max_movestep;
				this.min_size = L.min_size;
				this.max_size = L.max_size;
				this.min_sizestep = L.min_sizestep;
				this.max_sizestep = L.max_sizestep;
				this.min_sizestep0 = L.min_sizestep0;
				this.max_sizestep0 = L.max_sizestep0;
				this.min_slice = L.min_slice;
				this.max_slice = L.max_slice;
				this.min_slip = L.min_slip;
				this.max_slip = L.max_slip;
				this.hole_variable = L.hole_variable;
				this.reverse_ratio = L.reverse_ratio;
				this.flipx_ratio = L.flipx_ratio;
				this.flipy_ratio = L.flipy_ratio;
				this.center_void_ratio = L.center_void_ratio;
				this.cannot_answer = L.cannot_answer;
				this.Afix_x = new List<int>(L.Afix_x);
				this.Afix_y = new List<int>(L.Afix_y);
				this.Aaim = new List<int>(L.Aaim);
			}
		}

		public ZLit generateFirst(PuzzleOption Opt, int index)
		{
			return new ZLit(this, Opt)
			{
				id = index,
				variable = ((this.hole_variable == -1) ? (1 + index) : this.hole_variable)
			};
		}

		public PuzzleLitOption Randomize()
		{
			this.movetype_bits = 127U;
			this.movetype_bits = this.movetype_bits;
			if (X.XORSP() < 0.88f)
			{
				this.movetype_bits -= 5U;
			}
			this.shape_bits = 127U;
			this.shape_bits -= 1U;
			this.min_movestep = 1;
			this.max_movestep = 2;
			this.min_size = 0;
			this.max_size = 0;
			if (X.XORSP() < 0.25f)
			{
				this.max_size = 1;
			}
			this.min_slice = 0;
			this.max_slice = 0;
			if (X.XORSP() < 0.125f)
			{
				this.max_slice = 1;
			}
			this.min_slip = 0;
			this.max_slip = 3;
			if (X.XORSP() < 0.32f)
			{
				this.max_slip = 0;
			}
			this.reverse_ratio = 50;
			this.flipx_ratio = 50;
			this.flipy_ratio = 50;
			this.center_void_ratio = 20;
			return this;
		}

		public uint movetype_bits;

		public uint shape_bits;

		public int min_movestep = 1;

		public int max_movestep = 1;

		public int min_size;

		public int max_size;

		public int min_slice;

		public int max_slice;

		public int min_slip;

		public int max_slip;

		public int min_sizestep = -1;

		public int max_sizestep = -1;

		public int min_sizestep0;

		public int max_sizestep0;

		public int hole_variable = -1;

		public int reverse_ratio = 50;

		public int flipx_ratio;

		public int flipy_ratio;

		public int center_void_ratio;

		public bool cannot_answer;

		public int naname_ratio;

		public List<int> Afix_x;

		public List<int> Afix_y;

		public List<int> Aaim;
	}
}
