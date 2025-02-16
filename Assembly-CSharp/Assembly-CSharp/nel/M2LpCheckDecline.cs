using System;
using m2d;

namespace nel
{
	public sealed class M2LpCheckDecline : NelLp
	{
		public M2LpCheckDecline(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			base.nM2D.CheckPoint.addDeclineArea(this);
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			base.nM2D.CheckPoint.remDeclineArea(this);
		}
	}
}
