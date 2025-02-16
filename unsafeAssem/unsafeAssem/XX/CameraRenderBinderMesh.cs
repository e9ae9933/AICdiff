using System;
using UnityEngine;

namespace XX
{
	public class CameraRenderBinderMesh : ICameraRenderBinder
	{
		public CameraRenderBinderMesh(string _key, MeshDrawer _Md, int _layer, bool _follow_posmain, float _z_far = -1000f)
		{
			this.Md = _Md;
			this.key = _key;
			this.Md.draw_gl_only = true;
			this.layer = _layer;
			this.follow_posmain = _follow_posmain;
			this.z_far = _z_far;
		}

		public CameraRenderBinderMesh SetLocalMatrix(Matrix4x4 Scl)
		{
			this.Trs = Scl;
			return this;
		}

		public CameraRenderBinderMesh BaseScale(float s)
		{
			this.Trs *= Matrix4x4.Scale(new Vector3(s, s, 1f));
			return this;
		}

		public CameraRenderBinderMesh AfterScale(float s)
		{
			this.TrsA *= Matrix4x4.Scale(new Vector3(s, s, 1f));
			return this;
		}

		public virtual bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			Vector3 zero = Vector3.zero;
			Matrix4x4 matrix4x = this.TrsA * (this.follow_posmain ? JCon.CameraProjectionTransformed : JCon.CameraProjection) * this.Trs;
			if (this.AMtr == null)
			{
				BLIT.RenderToGLMtr(this.Md, this.base_x, this.base_y, 1f, this.Md.getMaterial(), matrix4x, this.Md.draw_triangle_count, false, false);
			}
			else
			{
				GL.LoadProjectionMatrix(matrix4x * Matrix4x4.Translate(new Vector3(this.base_x, this.base_y, 0f)));
				int num = this.AMtr.Length;
				int currentSubMeshIndex = this.Md.getCurrentSubMeshIndex();
				for (int i = 0; i < num; i++)
				{
					this.Md.chooseSubMesh(i, false, false);
					Material material = this.AMtr[i];
					if (material != null)
					{
						BLIT.RenderToGLImmediateSP(this.Md, material, this.Md.draw_triangle_count);
					}
				}
				this.Md.chooseSubMesh(currentSubMeshIndex, false, false);
			}
			return true;
		}

		public float getFarLength()
		{
			if (this.z_far != -1000f)
			{
				return this.z_far;
			}
			return this.Md.base_z;
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				this._tostring = stb.Add("CRBMesh - ", this.key).ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public MeshDrawer Md;

		public int layer;

		private bool follow_posmain;

		public float base_x;

		public float base_y;

		public bool consider_uishift = true;

		public Matrix4x4 Trs = Matrix4x4.identity;

		public Matrix4x4 TrsA = Matrix4x4.identity;

		public readonly string key;

		public float z_far;

		public Material[] AMtr;

		private string _tostring;
	}
}
