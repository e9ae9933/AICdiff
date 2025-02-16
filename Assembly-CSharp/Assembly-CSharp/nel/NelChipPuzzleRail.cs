using System;
using m2d;

namespace nel
{
	public class NelChipPuzzleRail : NelChip
	{
		public NelChipPuzzleRail(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		public override M2Puts inputXy()
		{
			base.inputXy();
			this.Adir = null;
			return this;
		}

		public int[] getDirs()
		{
			if (this.Adir == null)
			{
				this.Adir = base.Meta.getDirs("puzzle_rail", this.rotation, this.flip, 0);
			}
			return this.Adir;
		}

		private int[] Adir;

		public int walk_count;
	}
}
