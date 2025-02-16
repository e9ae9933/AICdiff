using System;
using m2d;

namespace nel
{
	public struct MapPosition
	{
		public MapPosition(float _x, float _y, Map2d _Mp)
		{
			this.x = _x;
			this.y = _y;
			this.Mp = _Mp;
		}

		public float x;

		public float y;

		public Map2d Mp;
	}
}
