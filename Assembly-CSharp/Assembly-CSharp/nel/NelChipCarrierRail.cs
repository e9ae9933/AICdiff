using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipCarrierRail : NelChip, ILinerReceiver
	{
		public NelChipCarrierRail(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		public override M2Puts inputXy()
		{
			base.inputXy();
			this.MyBlock = null;
			return this;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.MyBlock = null;
		}

		public void activateLiner(bool immediate)
		{
			if (this.MyBlock == null)
			{
				this.MyBlock = CarrierRailBlock.getRailRoad(this.Lay, this.mapx, this.mapy, -1, true);
			}
			if (this.MyBlock != null)
			{
				this.MyBlock.Actv.Add(this);
				this.activateToDrawer();
			}
		}

		public void deactivateLiner(bool immediate)
		{
			if (this.MyBlock == null)
			{
				this.MyBlock = CarrierRailBlock.getRailRoad(this.Lay, this.mapx, this.mapy, -1, true);
			}
			if (this.MyBlock != null)
			{
				this.MyBlock.Actv.Rem(this);
				this.deactivateToDrawer();
			}
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			return true;
		}

		private CarrierRailBlock MyBlock;
	}
}
