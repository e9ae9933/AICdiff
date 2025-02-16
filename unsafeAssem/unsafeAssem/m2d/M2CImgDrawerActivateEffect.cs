using System;
using XX;

namespace m2d
{
	public class M2CImgDrawerActivateEffect : M2CImgDrawer, IActivatable
	{
		public M2CImgDrawerActivateEffect(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, true)
		{
			this.act_lay = Meta.GetI("activate_ptcst", -1, 3);
		}

		public void activate()
		{
			if (this.act_lay < 0 || this.act_lay == this.layer)
			{
				base.Mp.M2D.curMap.PtcSTsetVar("x", (double)(this.Cp.mapcx + base.Meta.GetNm("activate_ptcst", 0f, 1))).PtcSTsetVar("y", (double)(this.Cp.mapcy + base.Meta.GetNm("activate_ptcst", 0f, 2))).PtcST2Base(base.Meta.GetS("activate_ptcst"), 0f, PTCThread.StFollow.NO_FOLLOW);
			}
		}

		public void deactivate()
		{
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		private int act_lay;
	}
}
