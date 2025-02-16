using System;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class M2CImgDrawerWormArea : M2CImgDrawer
	{
		public M2CImgDrawerWormArea(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (!base.Mp.apply_chip_effect)
			{
				return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			}
			Md = (this.Md = base.Mp.MyDrawerUGrd);
			if (Md == null)
			{
				return false;
			}
			base.Set(false);
			base.Lay.M2D.IMGS.Atlas.initForRectWhite(Md, meshx - base.CLEN * 2f, meshy - base.CLEN * 2f, base.CLEN * 4f, base.CLEN * 2f, false, true);
			Md.Rect(meshx, meshy, base.CLEN, base.CLEN, false);
			base.Set(false);
			base.setColAll(base.Mp.Dgn.getSpecificColor(C32.d2c(4281408788U)), false);
			base.Mp.addUpdateMesh(16, false);
			return false;
		}
	}
}
