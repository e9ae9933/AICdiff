using System;
using System.Collections.Generic;

namespace m2d
{
	public abstract class M2ChaserBase<T> : M2ChaserBaseFuncs where T : M2RegionBase
	{
		public M2ChaserBase(M2Mover _Mv)
			: base(_Mv)
		{
		}

		public abstract bool setDest(float depx, float depy, List<T> AReg = null);

		public float pre_x;

		public float pre_y;
	}
}
