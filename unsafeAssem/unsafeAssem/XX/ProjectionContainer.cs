using System;
using UnityEngine;

namespace XX
{
	public class ProjectionContainer
	{
		public Matrix4x4 CameraProjection = Matrix4x4.identity;

		public Matrix4x4 CameraProjectionTransformed = Matrix4x4.identity;
	}
}
