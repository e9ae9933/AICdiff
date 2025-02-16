using System;
using m2d;
using XX;

namespace nel
{
	public class M2CImgDrawerBench : M2CImgDrawerHalf
	{
		public M2CImgDrawerBench(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
			this.pivot_y = 0f;
		}
	}
}
