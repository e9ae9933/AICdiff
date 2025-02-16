using System;
using m2d;

namespace nel
{
	public class NelChipDrawBridgePiece : NelChip
	{
		public NelChipDrawBridgePiece(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
		}

		public override int getConfig(int x, int y)
		{
			if (this.CpRoot != null && !this.CpRoot.isConfigActive())
			{
				return 4;
			}
			return base.getConfig(x, y);
		}

		public NelChipDrawBridge CpRoot;
	}
}
