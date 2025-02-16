using System;
using UnityEngine;

namespace m2d
{
	public class M2PostRenderComponent : MonoBehaviour
	{
		private void OnPreRender()
		{
			this.Cam.fnPostRender();
		}

		public M2Camera Cam;
	}
}
