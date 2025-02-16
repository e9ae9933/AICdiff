using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class MdMap : MeshDrawer, ICameraRenderBinder
	{
		public MdMap(M2MeshContainer _MCon, int i, Mesh Ms = null)
			: base(Ms, 4, 6)
		{
			this.MCon = _MCon;
			this.id = i;
			this.RcPreDefine = this.MCon.GetRcPreDefined(i, true);
		}

		public void releaseTexture()
		{
			BLIT.nDispose(this.TxSimplified);
			this.TxSimplified = null;
			this.MdCacheSimplify = this.MCon.ReleaseSimplifyMesh(this.MdCacheSimplify);
			this.MdCacheSimplifyErase = this.MCon.ReleaseSimplifyMesh(this.MdCacheSimplifyErase);
			if (M2DBase.Instance != null && M2DBase.Instance.Cam != null && this.camera_render_assigned)
			{
				M2DBase.Instance.Cam.deassignRenderFunc(this, this.dep_layer);
				this.camera_render_assigned = false;
			}
			if (this.TriMulti != null)
			{
				base.chooseSubMesh(0, false, false);
				this.TriMulti = null;
			}
		}

		public override void destruct()
		{
			base.destruct();
			this.releaseTexture();
		}

		public void releaseTextureWithoutReleasingMemory()
		{
			this.TxSimplified = null;
		}

		public override bool isEmpty()
		{
			return (this.MdCacheSimplify == null || (this.MdCacheSimplify.isEmpty() && this.TxSimplified == null)) && base.isEmpty();
		}

		public void activateM2(string key, Material _Mtr)
		{
			base.activate(key, _Mtr, false, MTRX.ColGray, null);
			this.base_z = 0f;
			this.reverse_mesh_simplify = false;
		}

		public override MeshDrawer clear(bool no_clear_mesh = false, bool release_trimulti = false)
		{
			base.clear(no_clear_mesh, true);
			this.releaseTexture();
			return this;
		}

		public static bool refine_predef_rc
		{
			get
			{
				return Map2d.editor_decline_lighting;
			}
		}

		public MdMap prepareReentry()
		{
			RenderTexture txSimplified = this.TxSimplified;
			this.TxSimplified = null;
			this.clear(false, false);
			MdMap.MtrMiddleRender = null;
			if (!this.allow_simplify || this.RcPreDefine == null || this.RcPreDefine.isEmpty())
			{
				BLIT.nDispose(txSimplified);
			}
			else
			{
				this.TxSimplified = txSimplified;
				BLIT.Alloc(ref this.TxSimplified, X.IntC(this.RcPreDefine.width * 64f), X.IntC(this.RcPreDefine.height * 64f), false, RenderTextureFormat.ARGB32, 0);
				if (this.TxSimplified.name == "")
				{
					this.TxSimplified.name = "(Simplyfied-):" + this.MCon.getMap().key + "-" + this.id.ToString();
				}
				this.clearSimplifyTexture();
			}
			return this;
		}

		public MeshDrawer checkArrangeableMeshEvacuation(M2Puts Cp, M2CImgDrawer Da)
		{
			if (Cp.arrangeable || Da.redraw_flag)
			{
				return this;
			}
			return this.checkArrangeableMeshEvacuation();
		}

		public MeshDrawer checkArrangeableMeshEvacuation()
		{
			if (!this.allow_simplify || !M2DBase.cfg_simplify_bg_drawing || MdMap.refine_predef_rc)
			{
				return this;
			}
			if (this.MdCacheSimplify == null)
			{
				this.MdCacheSimplify = this.MCon.PoolSimplifyMesh(this, false);
			}
			this.MdCacheSimplify.Col = this.Col;
			return this.MdCacheSimplify;
		}

		public void SimplifyRemoveMesh(M2Puts Cp, float sx, float sy, float _zm, float _rotR, PxlMeshDrawer Ms)
		{
			if (MdMap.refine_predef_rc)
			{
				Color32 col = this.Col;
				this.Col = C32.d2c(3430023269U);
				base.RotaMesh(sx, sy, _zm * (float)X.MPF(!Cp.flip), _zm, _rotR, Ms, false, false);
				this.Col = col;
				return;
			}
			if (this.MdCacheSimplify == null)
			{
				return;
			}
			if (this.MdCacheSimplifyErase == null)
			{
				this.MdCacheSimplifyErase = this.MCon.PoolSimplifyMesh(this, true);
			}
			this.MdCacheSimplifyErase.Col = this.Col;
			this.MdCacheSimplifyErase.RotaMesh(sx, sy, _zm * (float)X.MPF(!Cp.flip), _zm, _rotR, Ms, false, false);
		}

		public bool canBakeSimplifyOnMiddle(bool on_middle_layer_reentry = true)
		{
			if (!MdMap.refine_predef_rc && this.RcPreDefine != null && !this.RcPreDefine.isEmpty())
			{
				int num = 0;
				if (this.MdCacheSimplify != null)
				{
					num += this.MdCacheSimplify.getVertexMax();
				}
				if (this.MdCacheSimplifyErase != null)
				{
					num += this.MdCacheSimplifyErase.getVertexMax();
				}
				return num > (on_middle_layer_reentry ? 240 : 0);
			}
			return false;
		}

		public void reentryLayerAfter(bool lay_chip_arrangeable, bool on_middle_layer_reentry = false)
		{
			if (!MdMap.refine_predef_rc && this.canBakeSimplifyOnMiddle(on_middle_layer_reentry))
			{
				this.RenderSimplify(-this.RcPreDefine.cx, -this.RcPreDefine.cy, true, !on_middle_layer_reentry);
				if (!on_middle_layer_reentry)
				{
					GL.Flush();
				}
			}
		}

		private void clearSimplifyTexture()
		{
			Graphics.SetRenderTarget(this.TxSimplified);
			GL.Clear(false, true, this.MCon.getMap().Dgn.getSimplifyTransparentColor(this.MCon.SM, this.MCon.getMap()));
		}

		private void RenderSimplify(float cx_u, float cy_u, bool on_middle = false, bool consider_remover = true)
		{
			RenderTexture txSimplified = this.TxSimplified;
			Graphics.SetRenderTarget(txSimplified);
			GL.LoadProjectionMatrix(Matrix4x4.Ortho(0f, (float)txSimplified.width * 0.015625f, 0f, (float)txSimplified.height * 0.015625f, -1f, 100f) * Matrix4x4.Translate(new Vector3((float)txSimplified.width * 0.5f * 0.015625f + cx_u, (float)txSimplified.height * 0.5f * 0.015625f + cy_u, 0f)) * Matrix4x4.Scale(new Vector3(1f, 1f, 0f)));
			int num = (consider_remover ? 2 : 1);
			for (int i = 0; i < num; i++)
			{
				MeshDrawer meshDrawer = ((i == 0) ? this.MdCacheSimplify : this.MdCacheSimplifyErase);
				if (meshDrawer != null)
				{
					int triMax = meshDrawer.getTriMax();
					Material material = meshDrawer.getMaterial();
					if (triMax > 0)
					{
						if (!on_middle)
						{
							BLIT.RenderToGLImmediate001(meshDrawer, -1, -1, true, true, null);
						}
						else
						{
							if (MdMap.MtrMiddleRender != material)
							{
								MdMap.MtrMiddleRender = material;
								MdMap.MtrMiddleRender.SetPass(0);
							}
							BLIT.RenderToGLImmediate001(meshDrawer, -1, -1, false, true, null);
						}
						meshDrawer.clearSimple();
					}
				}
			}
			GL.Flush();
			Graphics.SetRenderTarget(null);
		}

		public bool simplifyExecution(M2SubMap SM)
		{
			Map2d map = this.MCon.getMap();
			MdMap.MtrMiddleRender = null;
			if (map == null)
			{
				return false;
			}
			if (MdMap.refine_predef_rc && this.RcPreDefine != null && !this.RcPreDefine.isEmpty())
			{
				X.rectMultiply(this.RcPreDefine, (float)(-(float)map.width) * 0.5f * 0.015625f, (float)(-(float)map.height) * 0.5f * 0.015625f, (float)map.width * 0.015625f, (float)map.height * 0.015625f);
				if (X.IntC(this.RcPreDefine.width * 64f) % 2 == 1)
				{
					this.RcPreDefine.width += 0.015625f;
				}
				if (X.IntC(this.RcPreDefine.height * 64f) % 2 == 1)
				{
					this.RcPreDefine.height += 0.015625f;
				}
			}
			if (!this.allow_simplify)
			{
				return false;
			}
			if (this.MdCacheSimplify == null)
			{
				return this.TxSimplified != null;
			}
			if (SM != null && SM.temporary_duped)
			{
				return true;
			}
			RenderTexture renderTexture = this.TxSimplified;
			DRect drect;
			if (this.RcPreDefine != null && !this.RcPreDefine.isEmpty())
			{
				drect = this.RcPreDefine;
			}
			else
			{
				drect = this.MdCacheSimplify.calcBounds(MdMap.RcBuf, -1);
				if (!drect.isEmpty())
				{
					X.rectMultiply(drect, (float)(-(float)map.width) * 0.5f * 0.015625f, (float)(-(float)map.height) * 0.5f * 0.015625f, (float)map.width * 0.015625f, (float)map.height * 0.015625f);
				}
			}
			if (!drect.isEmpty())
			{
				if (renderTexture == null)
				{
					int num = X.IntC(drect.width * 64f);
					int num2 = X.IntC(drect.height * 64f);
					string text = "(Simplyfied-):" + this.MCon.getMap().key + "-" + this.id.ToString();
					renderTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGB32, 1);
					renderTexture.name = text;
					renderTexture.filterMode = FilterMode.Point;
					this.TxSimplified = renderTexture;
					this.MCon.setMaterialForSimplify(this.MdCacheSimplify);
					this.clearSimplifyTexture();
				}
				else
				{
					if (this.RcPreDefine != null && drect != this.RcPreDefine)
					{
						drect.Set(this.RcPreDefine);
					}
					this.MCon.setMaterialForSimplify(this.MdCacheSimplify);
				}
				this.RenderSimplify(-drect.cx, -drect.cy, false, true);
			}
			else if (this.RcPreDefine != null)
			{
				drect.Set(this.RcPreDefine);
			}
			if (renderTexture != null)
			{
				base.InitSubMeshContainer(1);
				base.chooseSubMesh(0, false, true);
				this.base_z += 0.05f * (float)X.MPF(!this.reverse_mesh_simplify);
				Material material = MTRX.newMtr(base.getMaterial());
				material.name = "Simplified";
				this.Mtr.DisableKeyword("_NO_PIXELSNAP");
				base.setMaterial(material, true);
				this.base_x = (this.base_y = 0f);
				base.initForImgAndTexture(renderTexture);
				base.Identity();
				this.Col = MTRX.ColGray;
				base.DrawCen(drect.cx * 64f, drect.cy * 64f, null);
				base.chooseSubMesh(1, false, false);
				if (SM != null && this.tri_i == 0)
				{
					this.Mtr = null;
				}
				else
				{
					base.initForImgAndTexture(this.MCon.M2D.MIchip);
				}
				base.updateForMeshRenderer(true);
			}
			this.MdCacheSimplify = this.MCon.ReleaseSimplifyMesh(this.MdCacheSimplify);
			this.MdCacheSimplifyErase = this.MCon.ReleaseSimplifyMesh(this.MdCacheSimplifyErase);
			return true;
		}

		public bool has_content
		{
			get
			{
				return this.ver_i > 0;
			}
		}

		public void assignRenderFuncToCam()
		{
			this.MCon.getMap().M2D.Cam.assignRenderFunc(this, this.dep_layer, false, null);
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			if (this.Ms.vertexCount == 0)
			{
				return false;
			}
			int subMeshCount = base.getSubMeshCount(false);
			Map2d map = this.MCon.getMap();
			GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed * map.gameObject.transform.localToWorldMatrix);
			for (int i = 0; i < subMeshCount; i++)
			{
				int num = 0;
				if (subMeshCount >= 2)
				{
					if (base.getSubMeshData(i, ref num) != null && num != 0)
					{
						base.chooseSubMesh(i, false, false);
						this.Mtr.SetPass(0);
						BLIT.RenderToGLImmediate001(this, num, i, false, true, null);
					}
				}
				else
				{
					this.Mtr.SetPass(0);
					BLIT.RenderToGLImmediate001(this, -1, -1, false, true, null);
				}
			}
			if (subMeshCount >= 2)
			{
				base.chooseSubMesh(1, false, false);
			}
			return true;
		}

		public float getFarLength()
		{
			return this.base_z;
		}

		public int dep_layer;

		public readonly int id;

		public readonly M2MeshContainer MCon;

		public RenderTexture TxSimplified;

		public bool transform_cam_trace;

		internal DRect RcPreDefine;

		public static DRect RcBuf = new DRect("Buf");

		public bool allow_simplify;

		private MeshDrawer MdCacheSimplify;

		private MeshDrawer MdCacheSimplifyErase;

		public bool save_predefinedrect = true;

		public bool reverse_mesh_simplify;

		public const int TRI_SIMPLIFY = 0;

		public const int TRI_ARRANGEABLE = 1;

		public const string SIMPLIFY_MESH_KEY = "_simplify";

		private static Material MtrMiddleRender;

		public bool camera_render_assigned;
	}
}
