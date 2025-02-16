using System;
using UnityEngine;

namespace XX
{
	public interface ICameraRenderBinder
	{
		bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam);

		float getFarLength();
	}
}
