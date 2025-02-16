using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipPuzzleRailStart : NelChip
	{
		public NelChipPuzzleRailStart(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.shift_px = (float)base.Meta.GetI("puzzle_rail_start", 4, 2);
		}

		public void activateRailStart(bool immediate = false)
		{
			if (this.aim == 255)
			{
				return;
			}
			PuzzLiner.initLiner(this.Mp, (float)this.mapx + 0.5f + 0.5f * (float)CAim._XD((int)this.aim, 1), (float)this.mapy + 0.5f - 0.5f * (float)CAim._YD((int)this.aim, 1), (int)this.puzzle_id, (int)this.aim, 0, immediate);
		}

		public override M2Puts inputXy()
		{
			base.inputXy();
			this.aim = (byte)base.Meta.getDirsI("puzzle_rail_start", this.rotation, this.flip, 1, 1);
			this.puzzle_id = (byte)base.Meta.GetI("puzzle_rail_start", 255, 0);
			return this;
		}

		public float get_real_draw_map_cx()
		{
			return M2CImgDrawerPuzzleRailStart.get_real_draw_map_cx(this.aim, this, this.shift_px);
		}

		public float get_real_draw_map_cy()
		{
			return M2CImgDrawerPuzzleRailStart.get_real_draw_map_cy(this.aim, this, this.shift_px);
		}

		public byte aim = byte.MaxValue;

		public byte puzzle_id = byte.MaxValue;

		public float shift_px = 4f;
	}
}
