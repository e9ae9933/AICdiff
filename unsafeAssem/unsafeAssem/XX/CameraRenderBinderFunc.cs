using System;
using UnityEngine;

namespace XX
{
	public class CameraRenderBinderFunc : ICameraRenderBinder
	{
		public CameraRenderBinderFunc(string _key, FnRenderToCam _Fn, float _z_far)
		{
			this.key = _key;
			this.Fn = _Fn;
			this.z_far = _z_far;
		}

		public float getFarLength()
		{
			return this.z_far;
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			return this.Fn(XCon, JCon, Cam);
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				this._tostring = stb.Add("CRBFunc - ", this.key).ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public readonly string key;

		protected FnRenderToCam Fn;

		public float z_far;

		private string _tostring;
	}
}
