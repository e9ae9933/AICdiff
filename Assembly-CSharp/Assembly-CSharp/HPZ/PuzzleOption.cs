using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace HPZ
{
	public sealed class PuzzleOption
	{
		public PuzzleOption()
		{
			this.ALits = new List<PuzzleLitOption>(4);
		}

		public List<PuzzleLitOption> createLits()
		{
			this.ALits.Clear();
			int num = this.lit_count;
			while (--num >= 0)
			{
				PuzzleLitOption puzzleLitOption = new PuzzleLitOption(null);
				puzzleLitOption.Randomize();
				this.ALits.Add(puzzleLitOption);
			}
			return this.ALits;
		}

		public int hole_count
		{
			get
			{
				return this.hole_clms * this.hole_rows;
			}
		}

		public int getRot(int board)
		{
			return (this.rot45 ? 45 : 0) + (this.rot90 ? 90 : 0) + (this.rot180 ? 180 : 0) + ((this.step_rot45 ? 45 : 0) + (this.step_rot90 ? 90 : 0) + (this.step_rot180 ? 180 : 0)) * board;
		}

		public bool getFlipX(int board)
		{
			bool flag = this.flipx;
			if (!this.step_flipx)
			{
				return flag;
			}
			return flag == (board % 2 == 0);
		}

		public bool getFlipY(int board)
		{
			bool flag = this.flipy;
			if (!this.step_flipy)
			{
				return flag;
			}
			return flag == (board % 2 == 0);
		}

		public int need_lit_click
		{
			get
			{
				return X.Mx(this.need_click - this.reversible_click_count, 0);
			}
		}

		public int need_void_click
		{
			get
			{
				return X.MMX(0, this.reversible_click_count, this.need_click);
			}
		}

		public bool no_division
		{
			get
			{
				return !this.exist_left && !this.exist_right;
			}
		}

		public bool step_rotation_exists
		{
			get
			{
				return this.step_rot45 || this.step_rot180 || this.step_rot90;
			}
		}

		public int board_count;

		public int hole_clms;

		public int hole_rows;

		public int lit_count;

		public int need_click;

		public int[] ptskin_bits;

		public int slide_variable;

		public int reversible_click_count;

		public bool alloc_adding;

		public bool alloc_loop;

		public int fix_answer_surface = -1;

		public bool flipx;

		public bool flipy;

		public bool rot45;

		public bool rot90;

		public bool rot180;

		public bool step_flipx;

		public bool step_flipy;

		public bool step_rot45;

		public bool step_rot90;

		public bool step_rot180;

		public bool exist_left;

		public bool exist_right;

		public bool rotate_first;

		public bool alloc_same_board;

		public bool disperse_answer = true;

		public bool fix_mark_rotation;

		public Color32 ColTop;

		public Color32 ColBottom;

		public Color32 ColDark;

		public List<PuzzleLitOption> ALits;

		public string left_surface_string;

		public string correct_appear_telop;

		public string punch_telop_top;

		public string punch_telop_bottom;

		public string telop_under;

		public bool lock_dummy_surface;

		public bool force_progress;

		public bool merge_variable;
	}
}
