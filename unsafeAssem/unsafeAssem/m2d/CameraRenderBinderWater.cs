using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class CameraRenderBinderWater : ICameraRenderBinder
	{
		public CameraRenderBinderWater(Map2d _Mp, M2SubMap _SM, bool to_mover = false)
		{
			this.Mp = _Mp;
			this.SM = _SM;
			this.Mp.M2D.Cam.CurDgn.initMap(this.Mp, this.SM);
			this.z_far = this.Mp.M2D.Cam.CurDgn.getDrawZ(this.Mp.mode, -11) + ((this.SM != null) ? this.SM.Pos.z : 0f);
			if (to_mover)
			{
				this.Mtr2Mover = M2DBase.newMtr("m2d/ShaderMeshWater2Chip");
				this.fineMtrColor();
			}
			this.layer = this.Mp.M2D.Cam.CurDgn.getLayerForWater();
			if (this.SM == null && this.Mp.MyDrawerWater != null)
			{
				this.Mp.MyDrawerWater.draw_gl_only = true;
				this.Mp.MyDrawerWater.use_cache = false;
			}
		}

		public void fineMtrColor()
		{
			if (this.Mtr2Mover == null)
			{
				return;
			}
			Dungeon curDgn = this.Mp.M2D.Cam.CurDgn;
			this.Mtr2Mover.SetColor("_Color", curDgn.ColWater);
			this.Mtr2Mover.SetColor("_ColorB", curDgn.ColWaterBottom);
		}

		public override string ToString()
		{
			return "BinderWater";
		}

		public float getFarLength()
		{
			if (!(this.Mtr2Mover != null))
			{
				return this.z_far;
			}
			return 395f;
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			MdMap myDrawerWater = this.Mp.MyDrawerWater;
			if (myDrawerWater == null)
			{
				return false;
			}
			if (this.SM == null)
			{
				Matrix4x4 matrix4x = JCon.CameraProjectionTransformed * Matrix4x4.Scale(new Vector3(this.Mp.base_scale, this.Mp.base_scale, 1f));
				Material material = ((this.Mtr2Mover != null) ? this.Mtr2Mover : myDrawerWater.getMaterial());
				if (myDrawerWater.getSubMeshCount(false) >= 3)
				{
					GL.PushMatrix();
					GL.LoadProjectionMatrix(matrix4x);
					material.SetPass(0);
					if (this.Mtr2Mover == null)
					{
						BLIT.RenderToGLImmediate001(myDrawerWater, -1, 1, false, true, null);
					}
					BLIT.RenderToGLImmediate001(myDrawerWater, -1, 2, false, true, null);
					GL.PopMatrix();
					myDrawerWater.chooseSubMesh(0, false, false);
				}
				else
				{
					BLIT.RenderToGLMtr(myDrawerWater, 0f, 0f, 1f, myDrawerWater.getMaterial(), matrix4x, myDrawerWater.getTriMax(), false, false);
				}
			}
			else
			{
				Graphics.DrawMesh(myDrawerWater.getMesh(), this.SM.getTransformForCamera(false), myDrawerWater.getMaterial(), this.layer, Cam, 0, null, false, false, false);
			}
			return true;
		}

		protected M2SubMap SM;

		protected Map2d Mp;

		private Material Mtr2Mover;

		public float z_far;

		public int layer;
	}
}
