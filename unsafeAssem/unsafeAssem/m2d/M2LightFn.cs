using System;
using XX;

namespace m2d
{
	public class M2LightFn : M2Light
	{
		public M2LightFn(Map2d _Mp, M2LightFn.FnDrawLight _Fn, M2Mover _FollowMv, Func<float, float, bool> _FnIsinCamera = null)
			: base(_Mp, _FollowMv)
		{
			this.FnIsinCamera = _FnIsinCamera;
			this.Fn = _Fn;
		}

		public override bool isinCamera(float meshx, float meshy)
		{
			if (this.FnIsinCamera != null)
			{
				return this.FnIsinCamera(meshx, meshy);
			}
			return base.isinCamera(meshx, meshy);
		}

		public override void drawLightAt(MeshDrawer Md, float x, float y, float scale, float alpha = 1f)
		{
			if (alpha <= 0f)
			{
				return;
			}
			this.Fn(Md, this, x, y, scale, alpha);
		}

		private M2LightFn.FnDrawLight Fn;

		private Func<float, float, bool> FnIsinCamera;

		public delegate void FnDrawLight(MeshDrawer Md, M2LightFn Lt, float x, float y, float scale, float alpha);
	}
}
