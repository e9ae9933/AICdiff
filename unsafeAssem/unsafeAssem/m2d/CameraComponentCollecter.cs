using System;
using GGEZ;
using UnityEngine;
using XX;

namespace m2d
{
	public class CameraComponentCollecter : XCameraBase
	{
		public CameraComponentCollecter(M2Camera _Container, string _key, Camera _Cam)
			: base(_key)
		{
			this.Cam = _Cam;
			this.JCon = new ProjectionContainer();
			this.Container = _Container;
			this.Trs = this.Cam.transform;
			this.PxC = IN.GetOrAdd<PerfectPixelCamera>(this.Cam.gameObject);
			this.PxC.TexturePixelsPerWorldUnit = 64;
			this.PxC.OnEnable();
			this.PxC.enabled = false;
		}

		public override string ToString()
		{
			return "CC - " + this.key;
		}

		public override void destroy(bool destory_gameobject = true)
		{
			base.destroy(destory_gameobject);
			this.scale = 1f;
			this.final_render = (this.do_not_use_by_buffer = false);
			this.FirstMd = null;
			this.AMatualTexture = null;
			this.matual_i = 0;
			this.scaling_buffer = 3;
			this.InjectionRenderer = null;
			if (this.Cam.targetTexture != null)
			{
				this.Cam.targetTexture = null;
			}
			IN.remCameraObject(this.Cam.gameObject);
			if (destory_gameobject)
			{
				IN.DestroyOne(this.Cam.gameObject);
				return;
			}
			this.Cam.gameObject.SetActive(true);
			this.Cam.rect = new Rect(0f, 0f, 1f, 1f);
		}

		public void initMatualTexture(RenderTexture T)
		{
			this.AMatualTexture = new RenderTexture[]
			{
				this.Cam.targetTexture,
				T
			};
		}

		protected override void renderBindingsInit()
		{
			Graphics.SetRenderTarget(this.Cam.targetTexture);
			this.matual_i = 0;
		}

		public void replaceMutualTexture(ref RenderTexture TxSrc, ref RenderTexture DestSrc)
		{
			TxSrc = this.AMatualTexture[(int)this.matual_i];
			this.matual_i = 1 - this.matual_i;
			DestSrc = this.AMatualTexture[(int)this.matual_i];
			if ((this.scaling_buffer & 4) == 0)
			{
				this.scaling_buffer |= 5;
				this.JCon.CameraProjectionTransformed = this.Container.FinalProjBuffer * this.Cam.worldToCameraMatrix;
			}
		}

		public bool already_mutual_replaced
		{
			get
			{
				return (this.scaling_buffer & 4) > 0;
			}
		}

		public bool scaling_need_fine_projection
		{
			get
			{
				return (this.scaling_buffer & 2) > 0;
			}
			set
			{
				this.scaling_buffer = (byte)(value ? ((int)(this.scaling_buffer | 2)) : ((int)this.scaling_buffer & -3));
			}
		}

		public bool scaling_need_fine_all
		{
			get
			{
				return (this.scaling_buffer & 1) > 0;
			}
			set
			{
				this.scaling_buffer = (byte)(value ? ((int)(this.scaling_buffer | 1)) : ((int)this.scaling_buffer & -2));
			}
		}

		protected override void renderBindingsQuit()
		{
			Graphics.SetRenderTarget(null);
		}

		public bool dest_texture_matual
		{
			get
			{
				return this.matual_i == 1;
			}
		}

		public Texture currentMatualTexture
		{
			get
			{
				if (this.AMatualTexture != null)
				{
					return this.AMatualTexture[(int)this.matual_i];
				}
				return this.Cam.targetTexture;
			}
		}

		public override void assignRenderFunc(ICameraRenderBinder Fn)
		{
			if (this.InjectionRenderer != null)
			{
				this.InjectionRenderer.assignRenderFunc(Fn);
				return;
			}
			base.assignRenderFunc(Fn);
		}

		public override void deassignRenderFunc(ICameraRenderBinder Fn)
		{
			if (this.InjectionRenderer != null)
			{
				this.InjectionRenderer.deassignRenderFunc(Fn);
				return;
			}
			base.deassignRenderFunc(Fn);
		}

		public override Camera getTargetCam()
		{
			return this.Cam;
		}

		public override void Render(ProjectionContainer _JCon, bool no_static_render)
		{
			if (this.scaling_buffer != 0)
			{
				if ((this.scaling_buffer & 1) != 0)
				{
					this.JCon.CameraProjection = (this.Cam.projectionMatrix = this.PxC.updateMatrix());
					if (this.final_render)
					{
						this.Container.FinalProjBuffer = this.JCon.CameraProjection;
						this.JCon.CameraProjection = (this.Cam.projectionMatrix = Matrix4x4.Scale(this.Container.viewport_scale_vector) * this.JCon.CameraProjection);
					}
				}
				this.JCon.CameraProjectionTransformed = this.Cam.projectionMatrix * this.Cam.worldToCameraMatrix;
				this.scaling_buffer = 0;
			}
			if (this.InjectionRenderer != null)
			{
				this.InjectionRenderer.Render(this.JCon, no_static_render);
				return;
			}
			if (!no_static_render)
			{
				this.Cam.Render();
			}
			else if (!this.Cam.gameObject.activeSelf && this.Cam.targetTexture != null)
			{
				Graphics.SetRenderTarget(this.Cam.targetTexture);
				GL.Clear(true, true, this.Cam.backgroundColor);
			}
			base.Render(this.JCon, no_static_render);
		}

		public ProjectionContainer getProjectionContainer()
		{
			return this.JCon;
		}

		public Camera Cam;

		public PerfectPixelCamera PxC;

		public readonly M2Camera Container;

		public Transform Trs;

		public MeshDrawer FirstMd;

		public bool do_not_use_by_buffer;

		public bool move_with_main = true;

		public bool final_render;

		public float scale = 1f;

		private RenderTexture[] AMatualTexture;

		private byte matual_i;

		public XCameraBase InjectionRenderer;

		private readonly ProjectionContainer JCon;

		private byte scaling_buffer;
	}
}
