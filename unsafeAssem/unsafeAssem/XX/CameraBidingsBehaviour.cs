using System;
using System.Collections.Generic;
using GGEZ;
using UnityEngine;

namespace XX
{
	[RequireComponent(typeof(Camera))]
	public class CameraBidingsBehaviour : MonoBehaviour
	{
		public bool need_sort_binds
		{
			set
			{
				if (value)
				{
					this.need_sort_binds_pre = (this.need_sort_binds_post = true);
				}
			}
		}

		public void Awake()
		{
			this.JCon = new ProjectionContainer();
		}

		public void overrideJCon(ProjectionContainer _JCon)
		{
			this.JCon = _JCon;
			this.jcon_override = true;
		}

		public void Start()
		{
			PerfectPixelCamera component = base.gameObject.GetComponent<PerfectPixelCamera>();
			this.Cam = base.GetComponent<Camera>();
			if (component != null)
			{
				component.TexturePixelsPerWorldUnit = 64;
				component.float_scaling = 1f;
			}
			if (this.ABindPost == null && this.ABindPre == null)
			{
				base.enabled = false;
			}
		}

		public void ClearBindings()
		{
			this.ABindPre = null;
			this.ABindPost = null;
		}

		public static bool assignRenderFunc(ICameraRenderBinder Fn, ref List<ICameraRenderBinder> ABind)
		{
			bool flag = false;
			if (ABind == null)
			{
				ABind = new List<ICameraRenderBinder>(1) { Fn };
				return false;
			}
			if (ABind.IndexOf(Fn) == -1)
			{
				flag = true;
				ABind.Add(Fn);
			}
			return flag;
		}

		public static bool deassignRenderFunc(ICameraRenderBinder Fn, ref List<ICameraRenderBinder> ABind)
		{
			if (ABind != null)
			{
				ABind.Remove(Fn);
				if (ABind.Count == 0)
				{
					ABind = null;
					return true;
				}
			}
			return false;
		}

		public static int fnSortICameraRenderBinder(ICameraRenderBinder A, ICameraRenderBinder B)
		{
			float farLength = A.getFarLength();
			float farLength2 = B.getFarLength();
			if (farLength > farLength2)
			{
				return -1;
			}
			if (farLength != farLength2)
			{
				return 1;
			}
			return 0;
		}

		public void assignPreRenderFunc(ICameraRenderBinder Fn)
		{
			if (CameraBidingsBehaviour.assignRenderFunc(Fn, ref this.ABindPre))
			{
				this.need_sort_binds_pre = true;
			}
			base.enabled = true;
		}

		public void deassignPreRenderFunc(ICameraRenderBinder Fn)
		{
			if (CameraBidingsBehaviour.deassignRenderFunc(Fn, ref this.ABindPre))
			{
				this.need_sort_binds_pre = false;
				if (this.ABindPost == null && this.ABindPre == null)
				{
					base.enabled = false;
				}
			}
		}

		public void assignPostRenderFunc(ICameraRenderBinder Fn)
		{
			if (CameraBidingsBehaviour.assignRenderFunc(Fn, ref this.ABindPost))
			{
				this.need_sort_binds_post = true;
			}
			base.enabled = true;
		}

		public void deassignPostRenderFunc(ICameraRenderBinder Fn)
		{
			if (CameraBidingsBehaviour.deassignRenderFunc(Fn, ref this.ABindPost))
			{
				this.need_sort_binds_post = false;
				if (this.ABindPost == null && this.ABindPre == null)
				{
					base.enabled = false;
				}
			}
		}

		private void executeRenderBindings(ref List<ICameraRenderBinder> ABind, RenderTexture TargetTex, ref bool need_sort_binds)
		{
			if (this.need_fine_ortho && !this.jcon_override)
			{
				this.need_fine_ortho = false;
				this.JCon.CameraProjection = this.Cam.projectionMatrix;
				this.JCon.CameraProjectionTransformed = this.JCon.CameraProjection * this.Cam.worldToCameraMatrix;
			}
			if (need_sort_binds)
			{
				need_sort_binds = false;
				ABind.Sort((ICameraRenderBinder a, ICameraRenderBinder b) => CameraBidingsBehaviour.fnSortICameraRenderBinder(a, b));
			}
			int num = ABind.Count;
			int i = 0;
			Graphics.SetRenderTarget(TargetTex);
			string text = Bench.P(this.ToString());
			while (i < num)
			{
				ICameraRenderBinder cameraRenderBinder = ABind[i];
				string text2 = Bench.P(cameraRenderBinder.ToString());
				try
				{
					if (!cameraRenderBinder.RenderToCam(this.XCon, this.JCon, this.Cam))
					{
						ABind.RemoveAt(i);
						num--;
					}
					else
					{
						i++;
					}
				}
				catch (Exception ex)
				{
					X.de(ex.ToString(), null);
					i++;
				}
				Bench.Pend(text2);
			}
			Bench.Pend(text);
		}

		public void OnPreRender()
		{
			if (Camera.current == this.Cam && this.ABindPre != null)
			{
				this.executeRenderBindings(ref this.ABindPre, this.Cam.targetTexture, ref this.need_sort_binds_pre);
			}
		}

		public RenderTexture replaceTemporaryBuffer()
		{
			if (!this.temporary_replaced)
			{
				GL.Flush();
				BLIT.JustPaste(this.RenderingBuffer, this.RenderingTarget, false);
				Graphics.SetRenderTarget(this.RenderingTarget);
				this.temporary_replaced = true;
			}
			return this.RenderingBuffer;
		}

		public void replaceMatualTexture(out RenderTexture TxSrc, out RenderTexture DestSrc)
		{
			GL.Flush();
			TxSrc = this.RenderingBuffer;
			DestSrc = this.RenderingTarget;
			this.RenderingTarget = TxSrc;
			this.RenderingBuffer = DestSrc;
		}

		public void OnRenderImage(RenderTexture Src, RenderTexture Dest)
		{
			if (Camera.current != this.Cam)
			{
				return;
			}
			bool flag = Dest == null;
			if (this.ABindPost != null)
			{
				this.temporary_replaced = false;
				this.RenderingBuffer = Src;
				this.RenderingTarget = Dest;
				this.executeRenderBindings(ref this.ABindPost, Src, ref this.need_sort_binds_post);
				if (this.RenderingBuffer == Dest)
				{
					GL.Flush();
				}
				else
				{
					this.replaceTemporaryBuffer();
				}
				this.RenderingBuffer = null;
			}
			else
			{
				BLIT.JustPaste(Src, Dest, false);
			}
			GL.Flush();
			if (!flag)
			{
				Graphics.SetRenderTarget(Src);
				GL.LoadOrtho();
			}
		}

		public string getInfo()
		{
			STB stb = TX.PopBld(null, 0);
			if (this.ABindPre != null)
			{
				int num = this.ABindPre.Count;
				stb.Add("Pre ", num.ToString(), " Counts");
				for (int i = 0; i < num; i++)
				{
					stb.Add("\n - ", i.ToString(), ": ", this.ABindPre[i].ToString());
				}
			}
			if (this.ABindPost != null)
			{
				int num = this.ABindPost.Count;
				stb.Append3("Post ", num.ToString(), " Counts", "\n\n");
				for (int j = 0; j < num; j++)
				{
					stb.Add("\n - ", j.ToString(), ": ", this.ABindPost[j].ToString());
				}
			}
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			return text;
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld("BindingBehaviour:", 0);
				stb += base.name;
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public bool awakened
		{
			get
			{
				return this.JCon != null;
			}
		}

		public static CameraBidingsBehaviour UiBind;

		protected List<ICameraRenderBinder> ABindPre;

		protected List<ICameraRenderBinder> ABindPost;

		public bool need_sort_binds_pre;

		public bool need_sort_binds_post;

		public bool need_fine_ortho = true;

		private bool jcon_override;

		private Camera Cam;

		public XCameraBase XCon;

		private ProjectionContainer JCon;

		private bool temporary_replaced;

		private RenderTexture RenderingBuffer;

		private RenderTexture RenderingTarget;

		public string _tostring;
	}
}
