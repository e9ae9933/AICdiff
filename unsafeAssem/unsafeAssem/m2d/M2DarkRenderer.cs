using System;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class M2DarkRenderer : ICameraRenderBinder
	{
		public M2DarkRenderer(M2DBase _M2D, Map2d Mp, ref Material Mtr)
		{
			this.M2D = _M2D;
			this.Md = new MeshDrawer(null, 4, 6);
			this.Md.draw_gl_only = true;
			this.Md.activation_key = "";
			if (Mtr == null)
			{
				Mtr = M2DBase.newMtr("m2d/WholeDarkArea");
			}
			this.Md.setMaterial(Mtr, false);
			this.f0 = Mp.floort;
			if (this.f0 < 3f)
			{
				this.f0 = 0f;
			}
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			Map2d curMap = M2DBase.Instance.curMap;
			if (curMap == null)
			{
				return false;
			}
			if (this.need_fine_mesh)
			{
				this.Md.clear(false, false);
				this.need_fine_mesh = false;
				this.Md.ColGrd.Black();
				if (this.f0 > 0f)
				{
					float num = X.ZLINE(curMap.floort - this.f0, 20f);
					if (num < 1f)
					{
						this.need_fine_mesh = true;
					}
					this.Md.ColGrd.mulA(num);
				}
				else if (this.f0 < 0f)
				{
					if (this.f0 >= -1f)
					{
						return false;
					}
					float num2 = 1f - X.ZLINE(curMap.floort + this.f0, 70f);
					if (num2 <= 0f)
					{
						return false;
					}
					this.need_fine_mesh = true;
					this.Md.ColGrd.mulA(num2);
				}
				this.Md.Col = this.Md.ColGrd.mulA(this.base_alpha).C;
				this.Md.base_z = -20f;
				this.Md.initForImgAndTexture(this.M2D.Cam.getLightTexture());
				this.Md.Rect(0f, 0f, IN.w + 16f, IN.h + 16f, false);
			}
			GL.LoadProjectionMatrix(JCon.CameraProjection);
			BLIT.RenderToGLImmediate001(this.Md, -1, -1, true, true, null);
			return true;
		}

		public void deactivate()
		{
			Map2d curMap = M2DBase.Instance.curMap;
			this.f0 = X.Mn(-1f, -curMap.floort);
			this.need_fine_mesh = true;
		}

		public bool need_fine_mesh
		{
			get
			{
				return this.Md.activation_key == "";
			}
			set
			{
				this.Md.activation_key = (value ? "" : "DARK");
			}
		}

		public float getFarLength()
		{
			return -10000f;
		}

		public override string ToString()
		{
			return "DarkRenderer";
		}

		private M2DBase M2D;

		private MeshDrawer Md;

		private float f0;

		public float base_alpha = 1f;
	}
}
