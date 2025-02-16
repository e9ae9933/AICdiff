using System;
using Spine;
using UnityEngine;
using XX;

namespace nel
{
	public class MosaicShower
	{
		public MosaicShower(Transform _Trs)
		{
			this.Trs = _Trs;
			this.MtrMosaic = MTRX.newMtr("Hachan/ShaderGDTMosaic");
			this.MtrMosaic.mainTexture = MTRX.MIicon.Tx;
			this.Md = new MeshDrawer(null, 4, 6);
			this.Md.draw_gl_only = true;
			this.BindCam = new CameraRenderBinderFunc("Mosaic", new FnRenderToCam(this.FnDrawMosaic), -9.39f);
		}

		public void destruct()
		{
			this.use_mosaic = false;
			this.Md = null;
			if (this.MtrMosaic != null)
			{
				IN.DestroyOne(this.MtrMosaic);
			}
			this.MtrMosaic = null;
			this.BindTarget.deassignPostRenderFunc(this.BindCam);
		}

		public bool render_to_target_texture
		{
			set
			{
				this.MtrMosaic.SetFloat("_Y_Reverse", (float)(value ? (-1) : 1));
			}
		}

		public bool draw_gl_only
		{
			get
			{
				return this.Md.draw_gl_only;
			}
		}

		public int draw_mesh_layer
		{
			get
			{
				return this.mesh_layer_;
			}
			set
			{
				if (value == this.mesh_layer_)
				{
					return;
				}
				bool draw_gl_only = this.Md.draw_gl_only;
				this.mesh_layer_ = value;
				this.Md.draw_gl_only = this.mesh_layer_ < 0;
				if (draw_gl_only && !this.Md.draw_gl_only)
				{
					this.Md.updateForMeshRenderer(true);
				}
			}
		}

		public CameraBidingsBehaviour BindTo
		{
			get
			{
				return this.BindTo_;
			}
			set
			{
				if (this.BindTo_ == value)
				{
					return;
				}
				if (this.use_mosaic)
				{
					this.BindTarget.deassignPostRenderFunc(this.BindCam);
				}
				this.BindTo_ = value;
				if (this.use_mosaic)
				{
					this.BindTarget.assignPostRenderFunc(this.BindCam);
				}
			}
		}

		public float base_z
		{
			get
			{
				return this.Md.base_z;
			}
			set
			{
				this.Md.base_z = value;
			}
		}

		public float abs_z
		{
			get
			{
				return this.BindCam.z_far;
			}
			set
			{
				this.BindCam.z_far = value;
			}
		}

		public int resolution
		{
			get
			{
				return this.resolution_;
			}
			set
			{
				this.resolution_ = value;
				this.MtrMosaic.SetFloat("_Dissolve", (float)value);
			}
		}

		private CameraBidingsBehaviour BindTarget
		{
			get
			{
				return this.BindTo_ ?? CameraBidingsBehaviour.UiBind;
			}
		}

		public void setTarget(IMosaicDescriptor _Targ, bool force = false)
		{
			bool flag = this.Targ == _Targ;
			this.Targ = _Targ;
			int num = ((this.Targ == null) ? 0 : this.Targ.countMosaic(X.SENSITIVE));
			if (num > 0 != this.use_mosaic)
			{
				this.use_mosaic = num > 0;
				if (!this.use_mosaic)
				{
					this.BindTarget.deassignPostRenderFunc(this.BindCam);
				}
				else
				{
					this.BindTarget.assignPostRenderFunc(this.BindCam);
				}
			}
			bool flag2 = num == this.use_pre_count;
			this.use_pre_count = num;
			if (num <= 0 || (flag && flag2 && !force))
			{
				return;
			}
			this.Md.clear(false, false);
			this.CacheAtc = null;
			this.Skdata = null;
			if (!X.SENSITIVE)
			{
				this.Md.activate("", this.MtrMosaic, false, MTRX.ColWhite, null);
				this.Md.base_x = 0f;
				this.Md.base_y = 0f;
				this.initBasicMosaicMesh();
				this.Md.updateForMeshRenderer(false);
				this.is_sensitive = false;
				return;
			}
			this.is_sensitive = true;
			this.Md.activate("", MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false, MTRX.ColBlack, null);
		}

		public void initBasicMosaicMesh()
		{
			this.Md.clear(false, false);
			this.Skdata = null;
			this.Md.initForImg(MTRX.EffCircle128, 0);
			this.Md.Rect(0f, 0f, 1f, 1f, true);
		}

		public void drawToMesh(Camera Cam)
		{
			if (this.draw_gl_only)
			{
				return;
			}
			this.FnDrawMosaic(null, null, Cam);
		}

		private bool FnDrawMosaic(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			if (!this.use_mosaic || (!this.draw_gl_only && JCon != null))
			{
				return false;
			}
			if (JCon != null)
			{
				GL.Flush();
				GL.PushMatrix();
			}
			int num = this.Targ.countMosaic(this.is_sensitive);
			bool flag = true;
			if (!this.is_sensitive)
			{
				for (int i = 0; i < num; i++)
				{
					MeshAttachment meshAttachment = null;
					Slot slot = null;
					Matrix4x4 matrix4x = Matrix4x4.identity;
					if (this.Targ.getSensitiveOrMosaicRect(ref matrix4x, i, ref meshAttachment, ref slot))
					{
						if (meshAttachment != this.CacheAtc || meshAttachment != null)
						{
							this.CacheAtc = meshAttachment;
							if (meshAttachment != null)
							{
								this.Md.clear(false, false);
								this.Md.setCurrentMatrix(matrix4x, false);
								this.drawMeshAttachment(meshAttachment, slot);
								matrix4x = Matrix4x4.identity;
							}
							else
							{
								this.initBasicMosaicMesh();
							}
						}
						if (JCon != null)
						{
							this.MtrMosaic.SetTexture("_MosTex", this.BindTarget.replaceTemporaryBuffer());
							GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed * matrix4x);
							if (flag)
							{
								flag = false;
								this.Md.getMaterial().SetPass(0);
							}
							BLIT.RenderToGLImmediate(this.Md, -1);
						}
						else
						{
							this.Md.RedrawSameMesh(this.mesh_layer_, this.mesh_layer_, matrix4x, Cam, false);
						}
					}
				}
			}
			else
			{
				if (JCon != null)
				{
					GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed * ValotileRenderer.Mx_z_zero);
					this.Md.getMaterial().SetPass(0);
					GL.Begin(4);
				}
				this.Md.clearSimple();
				for (int j = 0; j < num; j++)
				{
					Matrix4x4 identity = Matrix4x4.identity;
					MeshAttachment meshAttachment2 = null;
					Slot slot2 = null;
					if (this.Targ.getSensitiveOrMosaicRect(ref identity, j, ref meshAttachment2, ref slot2))
					{
						if (meshAttachment2 != null)
						{
							this.Md.setCurrentMatrix(identity, false);
							this.drawMeshAttachment(meshAttachment2, slot2);
						}
						else
						{
							float num2 = X.Abs(identity.MultiplyPoint3x4(Vector3.one).x - identity.MultiplyPoint3x4(Vector3.zero).x);
							num2 = X.Mx(num2, 0.625f) / num2 * 2.5f;
							this.Md.setCurrentMatrix(identity * Matrix4x4.Scale(new Vector3(num2, num2, 1f)), false);
							this.Md.initForImg(MTRX.EffBlurCircle245, 0);
							this.Md.Rect(0f, 0f, 1f, 1f, true);
							this.Md.Rect(0f, 0f, 2f, 2f, true);
							this.Md.initForImg(MTRX.EffCircle128, 0);
							this.Md.Rect(0f, 0f, 0.75f, 0.75f, true);
						}
						if (JCon != null)
						{
							BLIT.RenderToGLOneTask(this.Md, -1, false);
						}
					}
				}
				if (JCon != null)
				{
					GL.End();
				}
				else
				{
					this.Md.updateForMeshRenderer(false);
					this.Md.RedrawSameMesh(this.mesh_layer_, this.mesh_layer_, Matrix4x4.identity, Cam, false);
				}
			}
			if (JCon != null)
			{
				GL.PopMatrix();
			}
			return true;
		}

		private unsafe void drawMeshAttachment(MeshAttachment Atc, Slot ConSlot)
		{
			int[] triangles = Atc.Triangles;
			if (triangles != null)
			{
				int worldVerticesLength = Atc.WorldVerticesLength;
				if (this.Avertice_buf == null)
				{
					this.Avertice_buf = new float[worldVerticesLength];
				}
				else if (this.Avertice_buf.Length < worldVerticesLength)
				{
					Array.Resize<float>(ref this.Avertice_buf, worldVerticesLength);
				}
				Atc.ComputeWorldVertices(ConSlot, 0, Atc.WorldVerticesLength, this.Avertice_buf, 0, 2);
				this.Md.allocVer(this.Md.getVertexMax() + Atc.WorldVerticesLength, 0);
				fixed (int* ptr = &triangles[0])
				{
					int* ptr2 = ptr;
					int i = triangles.Length;
					int* ptr3 = ptr2;
					this.Md.allocTri(this.Md.getTriMax() + i * 2, 0);
					while (i > 0)
					{
						this.Md.Tri(*(ptr3++), *(ptr3++), *(ptr3++), true);
						i -= 3;
					}
				}
				int vertexMax = this.Md.getVertexMax();
				if (this.AtcRc == null)
				{
					this.AtcRc = new DRect("");
				}
				else
				{
					this.AtcRc.Set(0f, 0f, 0f, 0f);
				}
				Vector3[] array;
				fixed (float* ptr4 = &this.Avertice_buf[0])
				{
					float* ptr5 = ptr4;
					for (int j = 0; j < worldVerticesLength; j += 2)
					{
						float num = *(ptr5++);
						float num2 = *(ptr5++);
						this.Md.Pos(num, num2, null);
						array = this.Md.getVertexArray();
						Vector3 vector = array[this.Md.getVertexMax() - 1];
						this.AtcRc.Expand(vector.x - 0.0001f, vector.y - 0.0001f, 0.0002f, 0.0002f, false);
					}
				}
				Vector2[] uvArray = this.Md.getUvArray();
				array = this.Md.getVertexArray();
				Vector2 vector2 = new Vector2(MTRX.IconWhite.RectIUv.width, MTRX.IconWhite.RectIUv.height);
				Vector2 vector3 = new Vector2(MTRX.IconWhite.RectIUv.x, MTRX.IconWhite.RectIUv.y);
				int k = vertexMax;
				int vertexMax2 = this.Md.getVertexMax();
				while (k < vertexMax2)
				{
					Vector2 vector4 = array[k];
					uvArray[k] = new Vector2((vector4.x - this.AtcRc.x) / this.AtcRc.width, (vector4.y - this.AtcRc.y) / this.AtcRc.height) * vector2 + vector3;
					k++;
				}
			}
		}

		public Transform Trs;

		private MeshDrawer Md;

		private IMosaicDescriptor Targ;

		private Material MtrMosaic;

		private CameraRenderBinderFunc BindCam;

		private CameraBidingsBehaviour BindTo_;

		private MeshAttachment CacheAtc;

		private SkeletonData Skdata;

		private DRect AtcRc;

		private float[] Avertice_buf;

		protected bool use_mosaic;

		public bool is_sensitive;

		private int use_pre_count;

		private int resolution_ = 5;

		private int mesh_layer_ = -1;

		public class MosaicInfo
		{
			public string bone_key;

			public float radius;

			public bool only_appear_sensitive;

			public bool for_attachment;
		}
	}
}
