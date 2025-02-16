using System;
using UnityEngine;

namespace m2d
{
	public interface M2CameraInterrupt
	{
		void RenderImage(RenderTexture Image0, RenderTexture Image1);

		void destruct();
	}
}
