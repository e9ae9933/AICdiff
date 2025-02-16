using System;
using PixelLiner;
using XX;

namespace m2d
{
	public class M2CImgDrawerHalf : M2CImgDrawer
	{
		public M2CImgDrawerHalf(MeshDrawer Md, int _lay, M2Puts _Cp, bool redraw = false)
			: base(Md, _lay, _Cp, redraw)
		{
			this._scale = base.Meta.GetNm("scale", 0.5f, 0);
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			Md.Identity();
			Md.TranslateP(-(this.pivot_x - 0.5f) * (float)this.Cp.iwidth, -(this.pivot_y - 0.5f) * (float)this.Cp.iheight, false).Rotate(_rotR, false).Scale(_zmx * this._scale, _zmy * this._scale, false);
			Md.TranslateP(meshx + (this.pivot_x - 0.5f) * (float)this.Cp.iwidth, meshy + (this.pivot_y - 0.5f) * (float)this.Cp.iheight, false);
			bool flag = base.entryMainPicToMesh(Md, 0f, 0f, 1f, 1f, 0f, Ms);
			Md.Identity();
			return flag;
		}

		public float pivot_x = 0.5f;

		public float pivot_y = 0.5f;

		public float _scale = 0.5f;
	}
}
