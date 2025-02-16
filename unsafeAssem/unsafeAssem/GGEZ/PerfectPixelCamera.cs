using System;
using UnityEngine;
using XX;

namespace GGEZ
{
	[ExecuteInEditMode]
	[HelpURL("http://ggez.org/posts/perfect-pixel-camera/")]
	[DisallowMultipleComponent]
	[AddComponentMenu("GGEZ/Camera/Perfect Pixel Camera")]
	public class PerfectPixelCamera : MonoBehaviour
	{
		public float SnapSizeWorldUnits { get; private set; }

		public void OnEnable()
		{
			if (!base.TryGetComponent<Camera>(out this.cameraComponent))
			{
				return;
			}
			this.LateUpdate();
		}

		public Rect PixelRect
		{
			get
			{
				Camera camera = this.cameraComponent;
				if (!(camera == null) && !this.use_fix_pixel_rect)
				{
					return camera.pixelRect;
				}
				return this.FixPixelRect;
			}
		}

		public float pixel_scale
		{
			get
			{
				Camera camera = this.cameraComponent;
				if (camera == null)
				{
					return 1f;
				}
				RenderTexture targetTexture = camera.targetTexture;
				Rect pixelRect = this.PixelRect;
				float num = 1f * ((targetTexture == null) ? pixelRect.height : IN.h) / ((targetTexture == null) ? IN.h : (camera.orthographicSize * 2f * (float)this.TexturePixelsPerWorldUnit));
				if (!(targetTexture != null))
				{
					if (IN.w * num > pixelRect.width)
					{
						num *= pixelRect.width / (IN.w * num);
					}
					return num;
				}
				if (num < 1f)
				{
					return 1f;
				}
				return Mathf.Floor(0.0625f + num);
			}
		}

		public void cameraFixSize(Rect Rc)
		{
		}

		public void LateUpdate()
		{
			this.updateMatrix();
		}

		public Matrix4x4 updateMatrix()
		{
			Camera camera = this.cameraComponent;
			camera.transparencySortMode = TransparencySortMode.Orthographic;
			camera.orthographic = true;
			camera.transform.rotation = Quaternion.identity;
			camera.orthographicSize = Mathf.Max(camera.orthographicSize, 1E-05f);
			Rect pixelRect = this.PixelRect;
			float num = (float)this.TexturePixelsPerWorldUnit;
			RenderTexture targetTexture = camera.targetTexture;
			float pixel_scale = this.pixel_scale;
			float num2 = pixel_scale * this.float_scaling;
			float num3 = 1f;
			targetTexture != null;
			float num4 = num3 * pixelRect.width / (num2 * 2f * num);
			float num5 = 1f;
			targetTexture != null;
			float num6 = num5 * pixelRect.height / (num2 * 2f * num);
			float num7 = 1f / (num2 * num);
			float num8 = 0f * num7;
			float num9 = num8 - Mathf.Repeat(num7 + Mathf.Repeat(camera.transform.position.x, num7), num7);
			float num10 = num8 - Mathf.Repeat(num7 + Mathf.Repeat(camera.transform.position.y, num7), num7);
			this.SnapSizeWorldUnits = num7;
			return camera.projectionMatrix = Matrix4x4.Ortho(-num4 + num9, num4 + num9, -num6 + num10, num6 + num10, camera.nearClipPlane, camera.farClipPlane);
		}

		[Tooltip("The number of texture pixels that fit in 1.0 world units. Common values are 8, 16, 32 and 64. If you're making a tile-based game, this is your tile size.")]
		[Range(1f, 64f)]
		public int TexturePixelsPerWorldUnit = 16;

		public float float_scaling = 1f;

		[SerializeField]
		public bool use_fix_pixel_rect;

		[SerializeField]
		public Rect FixPixelRect;

		private Camera cameraComponent;

		private const float halfPixelOffsetIfNeededForD3D = 0f;
	}
}
