using System;
using PixelLiner;
using XX;

namespace m2d
{
	public class M2CImgDrawerNoDraw : M2CImgDrawer
	{
		public M2CImgDrawerNoDraw(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			return !base.Mp.apply_chip_effect && base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
		}
	}
}
