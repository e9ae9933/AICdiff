using System;

namespace m2d
{
	public class M2AirRegionAdditional : M2RegionBase
	{
		public M2AirRegionAdditional(int px, int py, int _r = -1, int _b = -1, int _index = -1, int _direction = 8)
			: base(px, py, _index)
		{
			this.r = ((_r >= 0) ? _r : this.r);
			this.b = ((_b >= 0) ? _b : this.b);
			this.direction = _direction;
		}

		public bool canThrough(int aim)
		{
			return (this.direction & (1 << aim)) == 0;
		}

		public int direction;
	}
}
