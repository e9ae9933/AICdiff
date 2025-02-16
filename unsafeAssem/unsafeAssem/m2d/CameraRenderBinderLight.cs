using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class CameraRenderBinderLight : ICameraRenderBinder
	{
		public CameraRenderBinderLight(M2Camera _Cam, Map2d _Mp, M2SubMap _Sm)
		{
			this.Cam = _Cam;
			this.Mp = _Mp;
			this.Sm = _Sm;
			BLEND iconLightBlend = this.Mp.Dgn.getIconLightBlend(this.Mp, this.Sm);
			if (this.Sm == null)
			{
				this.Md = this.Cam.getLightMesh(false);
				this.layer = M2Camera.layerlight;
			}
			else
			{
				this.Md = this.Cam.createLightMesh(this.Sm.ToString(), iconLightBlend);
				this.layer = this.Mp.Dgn.getLayerForChip(1, Dungeon.MESHTYPE.EFFECT);
			}
			this.z = this.Mp.Dgn.getDrawZ(this.Mp.mode, 3);
			this.name_ = "LightRender-" + this.Md.activation_key;
			this.alpha = this.Mp.Dgn.getIconLightAlpha(this.Mp, this.Sm);
		}

		public override string ToString()
		{
			return this.name_;
		}

		public float getFarLength()
		{
			return ((this.Sm != null) ? this.Sm.Pos.z : 0f) + this.z;
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			Matrix4x4 matrix4x = JCon.CameraProjectionTransformed;
			if (this.Sm == null)
			{
				matrix4x = matrix4x * Matrix4x4.Scale(new Vector3(1f, 1f, 0f)) * this.Mp.gameObject.transform.localToWorldMatrix;
			}
			else
			{
				matrix4x *= this.Sm.getTransformForCamera(false);
			}
			GL.LoadProjectionMatrix(matrix4x);
			this.Md.clearSimple();
			this.Md.getMaterial().SetPass(0);
			GL.Begin(4);
			this.Mp.drawLights(this.Md, this.Sm, this.alpha, (float)X.AF, true);
			GL.End();
			return true;
		}

		private M2Camera Cam;

		private MeshDrawer Md;

		private Map2d Mp;

		private M2SubMap Sm;

		private float z;

		private float alpha;

		public int layer;

		private string name_;
	}
}
