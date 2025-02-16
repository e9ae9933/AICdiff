using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2Camera : ITeScaler, ValotileRenderer.IValotConnetcable
	{
		public static float texture_w_with_margin
		{
			get
			{
				return IN.w + 16f;
			}
		}

		public static float texture_h_with_margin
		{
			get
			{
				return IN.h + 16f;
			}
		}

		public M2Camera(M2DBase _M2D)
		{
			this.M2D = _M2D;
			this.PosEffectiveScaleMul = new Vector2(1f, 1f);
			this.Qu = new Quaker(null);
			this.AFoc = new List<M2LpCamFocus>();
			this.ADecl = new List<M2LpCamDecline>();
			this.base_scale = this.M2D.GobBase.transform.lossyScale.x;
			this.CamGob = new GameObject("CameraContainer");
			this.CamTrs = this.CamGob.transform;
			this.MovRender = new M2MovRenderContainer(this);
			this.MtrFinalize = MTRX.newMtr(MTRX.ShaderGDT);
			this.MtrFinalRendered = MTRX.newMtr(MTRX.ShaderGDT);
			this.ARdPool = new List<RenderTexturePool>();
			this.Ameshdrawer_scale = new List<float>(9);
			this.vareawh_focus_max = IN.wh / this.base_scale;
			this.vareahh_focus_max = IN.hh / this.base_scale;
			this.clearCameraCompoent();
			int num = LayerMask.NameToLayer("Default");
			this.TeCon = new TransEffecter("Cam", this.M2D.GobBase, 4, num, num, EFCON_TYPE.NORMAL);
			this.TeCon.RegisterScl(this);
		}

		public void destruct()
		{
			int count = this.ARdPool.Count;
			for (int i = 0; i < count; i++)
			{
				this.ARdPool[i].dispose();
			}
			IN.DestroyOne(this.MtrFinalRendered);
			IN.DestroyOne(this.MtrFinalize);
		}

		public Camera createCamera(string name, bool move_with_main = true, bool transparent = true)
		{
			GameObject gameObject = new GameObject("M2D Camera -" + name);
			gameObject.transform.SetParent(this.CamTrs, false);
			Camera camera = gameObject.AddComponent<Camera>();
			camera.CopyFrom(IN.getGUICamera());
			camera.gameObject.layer = 0;
			camera.depth = (float)this.ACam.Length;
			camera.rect = this.ACam[0].Cam.rect;
			IN.Pos2(gameObject.transform, 0f, 0f);
			camera.clearFlags = (transparent ? CameraClearFlags.Depth : CameraClearFlags.Nothing);
			return this.addCamera(camera, name, move_with_main);
		}

		private Camera addCamera(Camera Cam, string name, bool move_with_main = true)
		{
			if (Cam == null)
			{
				return null;
			}
			CameraComponentCollecter cameraComponentCollecter = new CameraComponentCollecter(this, name, Cam);
			cameraComponentCollecter.move_with_main = move_with_main;
			X.push<CameraComponentCollecter>(ref this.ACam, cameraComponentCollecter, -1);
			X.push<XCameraBase>(ref this.AXcBase, cameraComponentCollecter, -1);
			if (this.CCFinalCamera != null && this.ACam.Length >= 2)
			{
				this.ACam[this.ACam.Length - 1] = this.ACam[this.ACam.Length - 2];
				this.ACam[this.ACam.Length - 2] = cameraComponentCollecter;
				this.AXcBase[this.AXcBase.Length - 1] = this.AXcBase[this.AXcBase.Length - 2];
				this.AXcBase[this.AXcBase.Length - 2] = cameraComponentCollecter;
				cameraComponentCollecter.Trs.SetParent(this.CamTrs, true);
				IN.Pos2(cameraComponentCollecter.Trs, 0f, 0f);
			}
			return cameraComponentCollecter.Cam;
		}

		public Camera GetCameraComponent(int i)
		{
			return this.ACam[i].Cam;
		}

		public CameraComponentCollecter GetCameraCollecter(int i)
		{
			return this.ACam[i];
		}

		public CameraComponentCollecter GetCameraCollecter(Camera Cam)
		{
			int index = this.getIndex(Cam);
			if (index < 0)
			{
				return null;
			}
			return this.ACam[index];
		}

		public CameraComponentCollecter setCameraScale(Camera Cam, float _scale)
		{
			CameraComponentCollecter cameraCollecter = this.GetCameraCollecter(Cam);
			if (cameraCollecter == null)
			{
				return null;
			}
			cameraCollecter.scale = _scale;
			this.animateScaleTo(this.scale, 1);
			return cameraCollecter;
		}

		public int getIndex(Camera C)
		{
			if (C == null)
			{
				return -1;
			}
			int num = this.ACam.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.ACam[i].Cam == C)
				{
					return i;
				}
			}
			return -1;
		}

		public M2Camera clearCameraCompoent()
		{
			Camera camera = Camera.main;
			this.CamForEditor_ = null;
			this.cnt_scale_material = 0;
			this.AScaleListenerMaterial = null;
			this.ATempCreatingMd = null;
			M2Camera.no_limit_camera = false;
			this.viewport_scale_vector = Vector3.one;
			M2MeshContainer.prepareMeshContainer(this.CamGob, null, null, ref this.MMRD, true, this.M2D);
			this.TxFinalCameraCache = null;
			this.TrsFinalizeScreen = (this.TrsFinalizeExport = null);
			if (this.CCFinalCamera != null)
			{
				this.CCFinalCamera.Cam.targetTexture = null;
			}
			this.stabilize_drawing = X.DEBUGSTABILIZE_DRAW || Map2d.editor_decline_lighting;
			this.CCMoverCamera = (this.CCFinalCamera = (this.CCFinalCameraSource = null));
			this.Ameshdrawer_scale.Clear();
			this.fine_finalized_mesh = (this.fine_finalized_trs = (this.fine_finalized_trs_rot = true));
			this.MdFinalize = null;
			this.GobFinalize = null;
			this.ValotFinalize = (this.ValotFinalizeScreen = null);
			RenderTexturePool.ReleaseA(this.ARdPool);
			if (this.ACam == null)
			{
				this.layer_mask_def = camera.cullingMask;
				this.ACam = new CameraComponentCollecter[]
				{
					new CameraComponentCollecter(this, "(Main)", camera)
				};
				camera.transform.SetParent(this.CamTrs, true);
				this.ForMainProjection = this.ACam[0].getProjectionContainer();
				IN.Pos2(camera.transform, 0f, 0f);
				this.LightCamX = new XCameraTx("Light", MTRX.ColBlack, RenderTexturePool.PopA(this.ARdPool, (int)((IN.w + 16f) * 0.125f), (int)((IN.h + 16f) * 0.125f), 0, RenderTextureFormat.ARGB32), false);
				this.MainLightMeshDrawer = this.createLightMesh("MainLightMeshDrawer", BLEND.NORMAL);
			}
			else
			{
				this.MovRender.clearCameraComponent();
				camera = this.ACam[0].Cam;
				this.ForMainProjection = this.ACam[0].getProjectionContainer();
				this.MMRD.OnDestroy();
				camera.cullingMask = this.layer_mask_def;
				int num = this.ACam.Length;
				for (int i = 0; i < num; i++)
				{
					this.ACam[i].destroy(i > 0);
				}
				this.LightCamX.releaseAllBinder();
				Array.Resize<CameraComponentCollecter>(ref this.ACam, 1);
				this.animateScaleTo(this.scale, 4);
			}
			this.MMRD.use_valotile = false;
			this.MMRD.ValotConnetcable = null;
			IN.getGUICamera().clearFlags = CameraClearFlags.Color;
			if (this.CurDgn == null)
			{
				return this;
			}
			if (this.CurDgn == null || this.CurDgn.is_editor)
			{
				this.AXcBase = new XCameraBase[] { this.ACam[0] };
			}
			else
			{
				this.AXcBase = new XCameraBase[]
				{
					this.LightCamX,
					this.ACam[0]
				};
			}
			this.ACam[0].final_render = false;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = C32.d2c(4284124927U);
			this.transparent_color_ = MTRX.ColTrnsp;
			return this;
		}

		public MeshDrawer createMeshDrawer(Camera DCam, string name_suffix, RenderTexture Rd, Material Mtr, int layer, uint mesh_col = 4294967295U, float fillscale = 1f, bool transparent = true)
		{
			if (Mtr == null)
			{
				throw new Exception("Mtr is Empty");
			}
			MeshDrawer meshDrawer = this.createMeshDrawer(DCam, name_suffix, Mtr, layer, fillscale, transparent);
			meshDrawer.Col = C32.d2c(mesh_col);
			this.drawScaledMesh(meshDrawer, fillscale, fillscale, Rd);
			CameraComponentCollecter cameraCollecter = this.GetCameraCollecter(DCam);
			if (cameraCollecter != null && cameraCollecter.FirstMd == null)
			{
				cameraCollecter.FirstMd = meshDrawer;
			}
			return meshDrawer;
		}

		public MeshDrawer createMeshDrawer(Camera DCam, string name_suffix, Material Mtr, int layer, float scale, bool transparent = true)
		{
			if (Mtr == null)
			{
				throw new Exception("Mtr is Empty");
			}
			MdMap mdMap = null;
			this.MMRD.CreateMesh(ref mdMap, name_suffix, 440f - (float)this.MMRD.getLength() * 0.001f, Mtr, layer, false, false, true, false);
			this.Ameshdrawer_scale.Add(scale);
			int index = this.getIndex(DCam);
			bool flag = true;
			if (DCam != null && index >= 0)
			{
				flag = this.ACam[index].move_with_main;
			}
			if (flag)
			{
				this.MMRD.transformCamTrace(mdMap);
			}
			else
			{
				GameObject gob = this.MMRD.GetGob(mdMap);
				if (gob != null)
				{
					gob.transform.SetParent(null, true);
					IN.Pos2(gob.transform, 0f, 0f);
				}
			}
			return mdMap;
		}

		public bool connectToBinder(ValotileRenderer Target)
		{
			return this.assignRenderFuncXC(Target, Target.gameObject.layer, false, Target) != null;
		}

		public CameraComponentCollecter assignRenderFunc(ICameraRenderBinder Fn, int layer, bool assign_only_last = false, ValotileRenderer ValotAssigning = null)
		{
			return this.assignRenderFuncXC(Fn, layer, assign_only_last, ValotAssigning) as CameraComponentCollecter;
		}

		public XCameraBase assignRenderFuncXC(ICameraRenderBinder Fn, int layer, bool assign_only_last = false, ValotileRenderer ValotAssigning = null)
		{
			if (layer == M2Camera.layerlight)
			{
				this.LightCamX.assignRenderFunc(Fn);
				if (ValotAssigning != null)
				{
					ValotAssigning.Add(this.LightCamX);
				}
				return this.LightCamX;
			}
			int num = this.ACam.Length;
			layer = 1 << layer;
			CameraComponentCollecter cameraComponentCollecter = null;
			for (int i = num - 1; i >= 0; i--)
			{
				CameraComponentCollecter cameraComponentCollecter2 = this.ACam[i];
				if ((cameraComponentCollecter2.Cam.cullingMask & layer) != 0)
				{
					cameraComponentCollecter2.assignRenderFunc(Fn);
					if (ValotAssigning != null)
					{
						ValotAssigning.Add(cameraComponentCollecter2);
					}
					if (assign_only_last)
					{
						return cameraComponentCollecter2;
					}
					if (cameraComponentCollecter == null)
					{
						cameraComponentCollecter = cameraComponentCollecter2;
					}
				}
			}
			return cameraComponentCollecter;
		}

		public void deassignRenderFunc(ICameraRenderBinder Fn, int layer)
		{
			if (layer == IN.LAY("M2DLight"))
			{
				this.LightCamX.deassignRenderFunc(Fn);
				return;
			}
			int num = this.ACam.Length;
			layer = 1 << layer;
			for (int i = 0; i < num; i++)
			{
				this.ACam[i].deassignRenderFunc(Fn);
			}
		}

		private bool renderCameraInnerMesh(M2Camera Cam, CameraComponentCollecter CC)
		{
			return true;
		}

		public void drawScaledMesh(MeshDrawer Md, float scalex = 1f, float scaley = 1f, Texture Rd = null)
		{
			float num = 1f;
			float num2 = 1f;
			if (Rd != null)
			{
				Md.initForImgAndTexture(Rd);
			}
			Md.uvRect(0f, 0f, 1f, 1f, false, false);
			float num3 = (IN.w + 16f) * num;
			float num4 = (IN.h + 16f) * num2;
			Md.Rect(0f, 0f, num3, num4, false);
			Md.updateForMeshRenderer(false);
		}

		public RenderTexture initBufferScreen(Camera SrcCamera = null, float scale = 1f, RenderTexture Tx = null, RenderTextureFormat Format = RenderTextureFormat.ARGB32, bool use_depth = false)
		{
			if (SrcCamera == null)
			{
				SrcCamera = this.GetCameraComponent(0);
			}
			if (SrcCamera.targetTexture == null)
			{
				SrcCamera.clearFlags = CameraClearFlags.Color;
				SrcCamera.backgroundColor = this.transparent_color_;
				if (Tx == null)
				{
					Tx = RenderTexturePool.PopA(this.ARdPool, (int)((IN.w + 16f) * scale), (int)((IN.h + 16f) * scale), use_depth ? 16 : 0, RenderTextureFormat.ARGB32);
					Tx.name = "<RT>" + SrcCamera.gameObject.name;
				}
				SrcCamera.targetTexture = Tx;
				Tx.filterMode = FilterMode.Point;
				Tx.wrapMode = TextureWrapMode.Mirror;
				this.setCameraScale(SrcCamera, scale);
			}
			return SrcCamera.targetTexture;
		}

		public Camera initBufferScreenCamera(string name, Material Mtr, int lay_buf, Camera SrcCamera = null, float scale = 1f, uint mesh_col = 4294967295U, bool transparent = true, bool use_depth = false)
		{
			RenderTexture renderTexture = this.initBufferScreen(SrcCamera, scale, null, RenderTextureFormat.ARGB32, use_depth);
			if (lay_buf == -1)
			{
				return null;
			}
			SrcCamera.gameObject.layer = lay_buf;
			Camera camera = this.createCamera(name, true, transparent);
			camera.cullingMask = 1 << lay_buf;
			this.createMeshDrawer(camera, name, renderTexture, Mtr, lay_buf, mesh_col, 1f, transparent);
			return camera;
		}

		public RenderTexture initBufforForLightScreenCamera()
		{
			return this.LightCamX.getTexture();
		}

		public void addXCamera(XCameraTx Xc)
		{
			X.push<XCameraBase>(ref this.AXcBase, Xc, -1);
		}

		public void addTransformWithCam(Transform Trs)
		{
			Trs.SetParent(this.CamTrs, true);
			Vector3 localPosition = Trs.localPosition;
			localPosition.x = (localPosition.y = 0f);
			Trs.localPosition = localPosition;
			Trs.localScale = new Vector3(1f, 1f, 1f);
		}

		public void addScaleListenerMaterial(Material Mtr)
		{
			if (this.AScaleListenerMaterial == null)
			{
				this.cnt_scale_material = 1;
				this.AScaleListenerMaterial = new Material[] { Mtr };
			}
			else
			{
				X.pushToEmptyS<Material>(ref this.AScaleListenerMaterial, Mtr, ref this.cnt_scale_material, 16);
			}
			Mtr.SetFloat("_Scale", this.scale);
		}

		public void initCameraFinalize(bool editor_simple = false)
		{
			int num = this.ACam.Length;
			int layer_final = this.CurDgn.layer_final;
			int num2 = LayerMask.NameToLayer("FinalRendered");
			this.CCFinalCameraSource = null;
			Camera guicamera = IN.getGUICamera();
			Camera camera = null;
			if (editor_simple)
			{
				CameraComponentCollecter cameraComponentCollecter = this.ACam[0];
				this.CCFinalCamera = (this.CCFinalCameraSource = cameraComponentCollecter);
				cameraComponentCollecter.Cam.cullingMask |= IN.LAYB("ForFinalRender") | IN.LAYB("FinalRendered");
				guicamera.clearFlags = CameraClearFlags.Depth;
				guicamera.backgroundColor = this.transparent_color_;
			}
			else
			{
				guicamera.clearFlags = ((!this.stabilize_drawing) ? CameraClearFlags.Depth : CameraClearFlags.Color);
				guicamera.backgroundColor = ((!this.stabilize_drawing) ? MTRX.ColTrnsp : MTRX.ColBlack);
				for (int i = 0; i < 2; i++)
				{
					for (int j = this.ACam.Length - 1; j >= 0; j--)
					{
						CameraComponentCollecter cameraComponentCollecter2 = this.ACam[j];
						if ((i == 1 || cameraComponentCollecter2.Cam.gameObject.layer == layer_final) && cameraComponentCollecter2.Cam.targetTexture == null)
						{
							if (this.CCFinalCamera == null)
							{
								this.CCFinalCameraSource = cameraComponentCollecter2;
								cameraComponentCollecter2.Cam.cullingMask |= 1 << layer_final;
								Color backgroundColor = cameraComponentCollecter2.Cam.backgroundColor;
								bool stabilize_draw = this.MMRD.stabilize_draw;
								this.MMRD.stabilize_draw = true;
								this.ForMainProjection = cameraComponentCollecter2.getProjectionContainer();
								this.initBufferScreenCamera("final_rendered", this.MtrFinalRendered, num2, cameraComponentCollecter2.Cam, 1f, uint.MaxValue, false, true);
								this.CCFinalCamera = this.ACam[this.ACam.Length - 1];
								this.CCFinalCamera.final_render = true;
								this.MMRD.stabilize_draw = stabilize_draw;
								cameraComponentCollecter2.Cam.gameObject.layer = layer_final;
								cameraComponentCollecter2.Cam.backgroundColor = backgroundColor;
								this.CCFinalCamera.Cam.clearFlags = CameraClearFlags.Nothing;
								this.CCFinalCamera.Cam.gameObject.layer = num2;
							}
							else if (cameraComponentCollecter2 != this.CCFinalCamera)
							{
								this.CCFinalCameraSource.Cam.cullingMask |= cameraComponentCollecter2.Cam.cullingMask;
								this.CCFinalCameraSource.assignRenderFunc(cameraComponentCollecter2);
								X.splice<CameraComponentCollecter>(ref this.ACam, j, 1);
								int num3 = X.isinC<XCameraBase>(this.AXcBase, cameraComponentCollecter2);
								if (num3 >= 0)
								{
									X.splice<XCameraBase>(ref this.AXcBase, num3, 1);
								}
								cameraComponentCollecter2.destroy(true);
							}
						}
					}
				}
				int num4 = this.ACam.Length;
				for (int k = 0; k < num; k++)
				{
					CameraComponentCollecter cameraComponentCollecter3 = this.ACam[k];
					if (cameraComponentCollecter3.Cam.cullingMask == 1 << M2MovRenderContainer.drawer_mask_layer && cameraComponentCollecter3 != this.CCFinalCamera && cameraComponentCollecter3 != this.CCFinalCameraSource && cameraComponentCollecter3.Cam.targetTexture != null)
					{
						this.CCMoverCamera = cameraComponentCollecter3;
						break;
					}
				}
				for (int l = 0; l < num; l++)
				{
					if (!this.ACam[l].do_not_use_by_buffer && this.ACam[l].scale == 1f)
					{
						CameraComponentCollecter cameraComponentCollecter4 = this.ACam[l];
						if (cameraComponentCollecter4 != this.CCMoverCamera && cameraComponentCollecter4 != this.CCFinalCamera && cameraComponentCollecter4 != this.CCFinalCameraSource && cameraComponentCollecter4.Cam.targetTexture != null)
						{
							camera = cameraComponentCollecter4.Cam;
							break;
						}
					}
				}
			}
			if (this.CCFinalCamera == null)
			{
				X.de("最終カメラを設定できませんでした", null);
				return;
			}
			this.fineScale(true);
			bool stabilize_draw2 = this.MMRD.stabilize_draw;
			for (int m = this.ACam.Length - 1; m >= 0; m--)
			{
				CameraComponentCollecter cameraComponentCollecter5 = this.ACam[m];
				if (cameraComponentCollecter5 == this.CCFinalCamera)
				{
					if (!editor_simple && this.CCMoverCamera != null)
					{
						cameraComponentCollecter5.Cam.gameObject.SetActive(false);
						this.initBufferScreen(cameraComponentCollecter5.Cam, 1f, (camera != null) ? camera.targetTexture : null, RenderTextureFormat.ARGB32, true);
						cameraComponentCollecter5.Cam.clearFlags = CameraClearFlags.Color;
						cameraComponentCollecter5.Cam.backgroundColor = MTRX.ColBlack;
						cameraComponentCollecter5.initMatualTexture(this.TxFinalCameraCache = this.CCFinalCameraSource.Cam.targetTexture);
						this.MdFinalize = this.MMRD.Make(this.MtrFinalize);
						GameObject gob = this.MMRD.GetGob(this.MdFinalize);
						this.GobFinalize = gob;
						gob.layer = IN.gui_layer;
						gob.name = "CameraFinalRender to GUI";
						this.TrsFinalizeExport = gob.transform;
						this.TrsFinalizeExport.SetParent(null, false);
						IN.setZAbs(this.TrsFinalizeExport, 440f);
						this.MdFinalize.initForImgAndTexture(cameraComponentCollecter5.Cam.targetTexture);
					}
					else
					{
						IN.addCameraObject(cameraComponentCollecter5.Cam.gameObject, true);
						cameraComponentCollecter5.Cam.gameObject.SetActive(true);
					}
				}
				else
				{
					cameraComponentCollecter5.Cam.gameObject.SetActive(false);
				}
			}
			if (!editor_simple)
			{
				this.MMRD.ValotConnetcable = this;
				this.ValotFinalize = this.MMRD.GetValotileRenderer(this.MdFinalize);
				this.ValotFinalizeScreen = this.MMRD.GetValotileRenderer(this.CCFinalCamera.FirstMd);
			}
			if (this.MdFinalize != null)
			{
				this.GobFinalize.SetActive(this.finalize_screen_enabled_);
			}
			this.fineRenderedTextureAntiAlias();
			this.MovRender.initCameraFinalize(this.ACam, this.AXcBase);
			guicamera.depth = 31f;
			if (this.CCFinalCameraSource != null)
			{
				this.runMapFinalized();
			}
			if (this.fnPostRender != null)
			{
				this.CCFinalCamera.Cam.gameObject.AddComponent<M2PostRenderComponent>().Cam = this;
			}
		}

		public void fineRenderedTextureAntiAlias()
		{
			try
			{
				RenderTexture finalizeExportTexture = this.getFinalizeExportTexture();
				if (finalizeExportTexture != null)
				{
					bool flag = IN.pixel_scale == (float)((int)IN.pixel_scale);
					finalizeExportTexture.filterMode = (flag ? FilterMode.Point : FilterMode.Bilinear);
				}
			}
			catch
			{
			}
		}

		public Camera getCameraComponentForLayer(uint layer_bit)
		{
			for (int i = this.ACam.Length - 1; i >= 0; i--)
			{
				Camera cam = this.ACam[i].Cam;
				if (((long)cam.cullingMask & (long)((ulong)layer_bit)) != 0L)
				{
					return cam;
				}
			}
			return null;
		}

		public bool getCameraComponentForLayerIsSame(uint layer_bit_s, uint layer_bit_d)
		{
			for (int i = this.ACam.Length - 1; i >= 0; i--)
			{
				Camera cam = this.ACam[i].Cam;
				if (((long)cam.cullingMask & (long)((ulong)layer_bit_s)) != 0L)
				{
					return ((long)cam.cullingMask & (long)((ulong)layer_bit_d)) != 0L;
				}
			}
			return false;
		}

		public void init(Map2d _Md = null)
		{
			this.Md = _Md;
			this.TeCon.clear();
			if (this.walkFn == null)
			{
				this.walkFn = new FnCameraWalk(this.defaultWalkFn);
			}
			M2Camera.no_limit_camera = false;
			this.fix_to_player = 1;
			this.camera_moved = 1;
			this.crop_mapwh = 0;
			this.MvCenter = null;
			this.FocusTo_ = null;
			this.fine_focus_area_ = 2;
			this.do_not_consider_decline_area_ = false;
			this.scale = (this.scale_rev = 1f);
			this.Qu.clear();
			this.AFoc.Clear();
			if (this.Md != null)
			{
				this.setToLabelPt("_start", true, (float)this.Md.clms * 0.5f, (float)this.Md.rows * 0.5f);
				this.depx = this.x;
				this.depy = this.y;
				this.init2(this.Md);
			}
		}

		public void init2(Map2d _Md)
		{
			this.need_initialize_draw = true;
			this.crop_mapwh = 0;
			if (_Md != null)
			{
				this.Md = _Md;
				this.map_dw = (float)this.Md.width;
				this.map_dh = (float)this.Md.height;
				this.map_dwh = this.map_dw / 2f;
				this.map_dhh = this.map_dh / 2f;
				this.crop_mapwh = 0;
				if (this.Md.Meta != null)
				{
					this.crop_mapwh = this.Md.Meta.GetI("crop", 0, 0);
					int num = this.crop_mapwh;
					float clen = this.CLEN;
				}
				this.fineFixBaseArea();
			}
			if (_Md == null || _Md.mode == MAPMODE.SUBMAP)
			{
				return;
			}
			this.cam_walk_speed_x = (this.cam_walk_speed_y = this.cam_walk_speed_default);
			this.PreRenderTicketChecked.Set(-1000f, -1000f);
			if (this.Md != null)
			{
				this.MainLightMeshDrawer.setMaterial(MTRX.MIicon.getMtr(this.CurDgn.getIconLightBlend(this.Md, null), -1), false);
			}
			this.ADecl.Clear();
			this.fineScale(true);
			this.callImmediateMove();
		}

		private void fineFixBaseArea()
		{
			this.cam_x_fix = (this.cam_y_fix = false);
			int num = (int)((float)this.crop_mapwh * this.CLEN);
			if (this.map_dw - (float)(num * 2) < this.vareawh * 2f * this.scale)
			{
				this.cam_x_fix = true;
			}
			if (this.map_dh - (float)(num * 2) < this.vareahh * 2f * this.scale)
			{
				this.cam_y_fix = true;
			}
		}

		public void assignBaseMover(M2Mover _MvCenter, int fix_to_player_pos = -1)
		{
			if (_MvCenter == null)
			{
				_MvCenter = ((this.Md != null) ? this.Md.Pr : null);
			}
			NearCheckerM nearCheckerM = null;
			if (this.MvCenter != _MvCenter)
			{
				this.pos_center &= (M2Camera.IMMD)252;
				this.fine_focus_area = true;
				this.M2D.mainMvIntPosChanged(false);
				if (this.MvCenter != null)
				{
					nearCheckerM = this.MvCenter.NCM;
				}
			}
			if (_MvCenter is M2Attackable)
			{
				this.use_focus_area = (_MvCenter as M2Attackable).is_alive;
			}
			this.MvCenter = _MvCenter;
			if (nearCheckerM != null)
			{
				nearCheckerM.is_center_pr = false;
			}
			this.camera_moved++;
			if (this.walkFn == null)
			{
				this.walkFn = new FnCameraWalk(this.defaultWalkFn);
			}
			if (fix_to_player_pos == -1)
			{
				fix_to_player_pos = ((this.fix_to_player != 0) ? 1 : 0);
			}
			this.fix_to_player = ((this.fix_to_player > 1) ? this.fix_to_player : 0);
			if (this.MvCenter == null)
			{
				this.use_focus_area = false;
				return;
			}
			if (this.MvCenter.NCM != null)
			{
				this.MvCenter.NCM.is_center_pr = true;
			}
			if (fix_to_player_pos != 0)
			{
				int num = this.walk_margin_x;
				int num2 = this.walk_margin_y;
				this.walk_margin_x = (this.walk_margin_y = 0);
				float num3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				this.MvCenter.getCameraCenterPos(ref num3, ref num4, 0f, 0f, false, ref num5);
				this.setTo(num3 / this.CLEN, num4 / this.CLEN);
				this.walk_margin_x = num;
				this.walk_margin_y = num2;
			}
		}

		public bool use_focus_area
		{
			get
			{
				return this.fine_focus_area_ > 0;
			}
			set
			{
				if (value)
				{
					this.fine_focus_area_ = 2;
					return;
				}
				this.fine_focus_area_ = 0;
			}
		}

		public bool fine_focus_area
		{
			get
			{
				return this.fine_focus_area_ == 2;
			}
			set
			{
				if (!value)
				{
					return;
				}
				if (this.fine_focus_area_ == 1)
				{
					this.fine_focus_area_ = 2;
				}
				this.MovRender.need_clip_check = true;
			}
		}

		public void resetBaseMoverIfFocusing(M2Mover _MvCenter, bool only_not_key_player = false)
		{
			if (this.MvCenter == _MvCenter)
			{
				if (only_not_key_player && this.Md.getKeyPr() == _MvCenter)
				{
					return;
				}
				this.MvCenter = null;
				this.camera_moved++;
				this.use_focus_area = false;
				this.pos_center &= (M2Camera.IMMD)252;
				this.fine_focus_area = true;
			}
		}

		public bool setTo(float mpx, float mpy)
		{
			mpx *= this.CLEN;
			mpy *= this.CLEN;
			if (X.Abs(mpx - this.x) >= 4f)
			{
				this.pos_center &= (M2Camera.IMMD)254;
			}
			if (X.Abs(mpy - this.y) >= 4f)
			{
				this.pos_center &= (M2Camera.IMMD)253;
			}
			this.fixPosition(ref this.x, ref this.y, mpx, mpy);
			this.camera_moved++;
			this.fine_focus_area = true;
			return this.fine(M2Camera.IMMD.IMMEDIATE_FORCE, 65536f);
		}

		public bool setToPt(Vector2 P)
		{
			return P.x != -1000f && this.setTo(P.x, P.y);
		}

		public bool setToLabelPt(string label, bool no_error = false, float defx = -1000f, float defy = -1000f)
		{
			Vector2 vector = new Vector2(-1000f, -1000f);
			if (this.Md != null && this.Md.getCenter(label, ref vector, no_error))
			{
				return this.setToPt(vector);
			}
			return defx != -1000f && this.setTo(defx, defy);
		}

		public bool setToLabelPt(string label, bool no_error, Vector2 Pos)
		{
			Vector2 vector = new Vector2(-1000f, -1000f);
			if (this.Md != null && this.Md.getCenter(label, ref vector, no_error))
			{
				return this.setToPt(vector);
			}
			return this.setToPt(Pos);
		}

		public M2Camera animateScaleTo(float lvl, int time = 40)
		{
			this.anm_scale_s = this.scale;
			this.anm_scale_d = this.fixScaleFloat(lvl);
			this.anm_scale_t = 0;
			this.anm_scale_maxt = ((this.anm_scale_maxt == 0) ? time : X.Mn(time, this.anm_scale_maxt));
			this.moveBy(0f, 0f, 4f);
			if (this.anm_scale_maxt == 0)
			{
				this.scale = this.anm_scale_d;
				this.scale_rev = 1f / this.scale;
				this.need_scale_fine = true;
			}
			return this;
		}

		public void run(bool immediate_scale_finalize = false)
		{
			this.Qu.run(1f);
			this.TeCon.runSetter(1f);
			if (X.D)
			{
				this.TeCon.runDraw((float)X.AF * Map2d.TSbase, false);
			}
			if (this.anm_scale_maxt != 0)
			{
				if (!immediate_scale_finalize)
				{
					int num = this.anm_scale_t + 1;
					this.anm_scale_t = num;
					if (num < this.anm_scale_maxt)
					{
						this.scale = this.fixScaleFloat(this.anm_scale_s + X.ZSIN((float)this.anm_scale_t, (float)this.anm_scale_maxt) * (this.anm_scale_d - this.anm_scale_s));
						this.scale_rev = 1f / this.scale;
						goto IL_00DD;
					}
				}
				this.scale = (this.anm_scale_s = this.anm_scale_d);
				this.scale_rev = 1f / this.scale;
				this.anm_scale_maxt = 0;
				IL_00DD:
				this.fineScale(false);
			}
			else if (this.need_scale_fine)
			{
				this.fineScale(false);
			}
			if (X.D)
			{
				if (this.MvCenter != null)
				{
					M2Phys physic = this.MvCenter.getPhysic();
					if (physic != null && physic.walk_xspeed != 0f)
					{
						float num2 = 0f;
						M2FootManager footManager = physic.getFootManager();
						if (footManager != null && footManager.get_FootBCC() != null)
						{
							num2 = footManager.get_FootBCC().line_a;
						}
						this.translateIfCenter(this.MvCenter, physic.walk_xspeed * this.CLEN * (float)X.AF, num2 * physic.walk_xspeed * this.CLEN * (float)X.AF);
					}
				}
				this.fine(M2Camera.IMMD.AUTO, (float)X.AF);
			}
		}

		private float fixScaleFloat(float f)
		{
			if (f <= 1f)
			{
				return f;
			}
			float num = IN.h + 16f;
			float num2 = (float)X.IntR(num / f);
			return num / num2;
		}

		public bool redraw_behind(bool draw_flag, bool draw_finalaizecam_flag)
		{
			bool flag = X.D || this.need_initialize_draw;
			return draw_flag && flag;
		}

		public bool RenderWholeCamera(bool draw_flag, bool draw_finalaizecam_flag)
		{
			int num = this.AXcBase.Length - 1;
			bool flag = false;
			bool flag2 = X.D || this.need_initialize_draw;
			if (this.need_initialize_draw)
			{
				draw_finalaizecam_flag = (draw_flag = true);
				this.need_initialize_draw = false;
			}
			bool flag3 = this.ValotFinalize != null && this.ValotFinalize.enabled;
			if (flag2 && (draw_flag || draw_finalaizecam_flag) && draw_flag)
			{
				GL.Flush();
				Bench.P("Render- ");
				for (int i = 0; i < num; i++)
				{
					XCameraBase xcameraBase = this.AXcBase[i];
					Bench.P(xcameraBase.key);
					xcameraBase.Render(this.ForMainProjection, flag3);
					Bench.Pend(xcameraBase.key);
				}
				Bench.Pend("Render- ");
			}
			Bench.P("Render-Finalize ");
			CameraComponentCollecter cameraComponentCollecter = this.ACam[this.ACam.Length - 1];
			if (draw_finalaizecam_flag && (flag2 || this.TxFinalCameraCache == null))
			{
				cameraComponentCollecter.Render(null, flag3 || this.TxFinalCameraCache == null);
			}
			if (draw_finalaizecam_flag && this.TxFinalCameraCache != null)
			{
				if (this.fine_finalized_mesh)
				{
					this.MdFinalize.clearSimple();
					this.MdFinalize.resetVertexCount();
					this.MdFinalize.base_z = 0f;
					this.MdFinalize.Col.a = this.update_final_mesh_alpha255_;
					this.MdFinalize.DrawCen(0f, 0f, null);
					this.MdFinalize.updateForMeshRenderer(true);
					this.fine_finalized_mesh = false;
				}
				if (this.fine_finalized_trs)
				{
					this.fine_finalized_trs = false;
					this.TrsFinalizeExport.transform.localPosition = new Vector3(-this.cam_shift_x, -this.cam_shift_y, 440f);
				}
				if (this.fine_finalized_trs_rot)
				{
					if (this.effect_confuse_ != 0f)
					{
						float num2 = X.ZLINE(this.effect_confuse_) * 3f * (1.5f * X.COSI(this.Md.floort, 160f) + 0.5f * X.COSI(this.Md.floort + 93f, 280f));
						this.TrsFinalizeExport.localEulerAngles = new Vector3(0f, 0f, num2);
					}
					else
					{
						this.fine_finalized_trs_rot = false;
						this.TrsFinalizeExport.localEulerAngles = Vector3.zero;
					}
				}
				Texture currentMatualTexture = cameraComponentCollecter.currentMatualTexture;
				if (currentMatualTexture != this.MtrFinalize.mainTexture)
				{
					this.MtrFinalize.mainTexture = currentMatualTexture;
				}
				flag = true;
			}
			Bench.Pend("Render-Finalize ");
			return flag;
		}

		public M2Camera fineScale(bool first = false)
		{
			float num = this.scale * this.PosEffectiveScaleMul.x;
			this.need_scale_fine = false;
			this.MovRender.need_clip_check = true;
			for (int i = this.cnt_scale_material - 1; i >= 0; i--)
			{
				this.AScaleListenerMaterial[i].SetFloat("_Scale", num);
			}
			this.viewport_scale_vector = new Vector3(X.Mx(1f, this.scale) * this.PosEffectiveScaleMul.x, X.Mx(1f, this.scale) * this.PosEffectiveScaleMul.y, 1f);
			for (int j = this.ACam.Length - 1; j >= 0; j--)
			{
				CameraComponentCollecter cameraComponentCollecter = this.ACam[j];
				if (cameraComponentCollecter.move_with_main)
				{
					cameraComponentCollecter.scaling_need_fine_all = (cameraComponentCollecter.scaling_need_fine_projection = true);
					bool flag = true;
					if (cameraComponentCollecter.Cam.targetTexture != null)
					{
						flag = this.scale < 1f;
					}
					cameraComponentCollecter.PxC.float_scaling = cameraComponentCollecter.scale * (flag ? this.scale : 1f);
				}
			}
			if (first && this.Ameshdrawer_scale != null)
			{
				for (int k = this.Ameshdrawer_scale.Count - 1; k >= 0; k--)
				{
					float num2 = this.Ameshdrawer_scale[k];
					if (num2 > 0f)
					{
						MeshDrawer meshDrawer = this.MMRD.Get(k);
						this.drawScaledMesh(meshDrawer, num2, num2, null);
					}
				}
			}
			return this;
		}

		public void fineImmediately()
		{
			try
			{
				this.fine_focus_area = true;
				this.fine(M2Camera.IMMD.IMMEDIATE_FORCE, 65536f);
				this.need_initialize_draw = true;
				this.M2D.Snd.posFine();
				if (this.M2D.curMap != null)
				{
					this.M2D.curMap.MovRenderer.need_clip_check = true;
				}
			}
			catch
			{
			}
		}

		protected bool fine(M2Camera.IMMD immediate = M2Camera.IMMD.AUTO, float speed = 65536f)
		{
			bool flag = (immediate & M2Camera.IMMD.XY) > (M2Camera.IMMD)0;
			if (flag)
			{
				this.M2D.cameraFinedImmediately();
			}
			if (this.camera_moved > 0)
			{
				this.camera_moved--;
			}
			bool flag2 = false;
			if (this.MvCenter != null)
			{
				if ((immediate & M2Camera.IMMD.AUTO) != (M2Camera.IMMD)0)
				{
					immediate = this.default_immediate | (((this.pos_center & M2Camera.IMMD._FORCE) != (M2Camera.IMMD)0) ? M2Camera.IMMD.XY : ((M2Camera.IMMD)0));
				}
				if ((this.pos_center & M2Camera.IMMD._FORCE) != (M2Camera.IMMD)0)
				{
					this.fine_focus_area = true;
					this.pos_center &= (M2Camera.IMMD)247;
				}
				this.MvCenter.getCameraCenterPos(ref this.depx, ref this.depy, 0f, 0f, (immediate & M2Camera.IMMD.XY) > (M2Camera.IMMD)0, ref speed);
				bool flag3 = false;
				if (speed < 0f)
				{
					speed *= -1f;
					flag3 = true;
				}
				bool flag4 = false;
				if (!M2Camera.no_limit_camera)
				{
					M2LpCamFocus focusTo_ = this.FocusTo_;
					M2Camera.IMMD immd;
					if (this.checkDeclineArea(flag, out immd) || this.FocusTo_ != focusTo_)
					{
						this.pos_center = ((this.FocusTo_ != focusTo_) ? ((M2Camera.IMMD)0) : (this.pos_center & ~immd));
						this.fine_focus_area = true;
						flag4 = !flag;
					}
				}
				if ((immediate & M2Camera.IMMD.AUTO) != (M2Camera.IMMD)0)
				{
					immediate &= (M2Camera.IMMD)223;
					if (this.pos_center != (M2Camera.IMMD)0)
					{
						immediate |= this.pos_center & M2Camera.IMMD.XY;
					}
				}
				if ((immediate & M2Camera.IMMD._FORCE) != (M2Camera.IMMD)0)
				{
					this.pos_center |= immediate & M2Camera.IMMD._FORCE;
				}
				this.fixPosition(ref this.depx, ref this.depy, this.depx, this.depy);
				if (!flag3)
				{
					if (X.Abs(this.x - this.depx) < 1f + speed * this.cam_walk_speed_x)
					{
						immediate |= M2Camera.IMMD.IM_X;
					}
					if (X.Abs(this.y - this.depy) < 1f + speed * this.cam_walk_speed_y)
					{
						immediate |= M2Camera.IMMD.IM_Y;
					}
				}
				this.pre_frame_moved = new Vector2Int(X.IntNC(this.depx - this.x), X.IntNC(this.depy - this.y));
				if ((immediate & M2Camera.IMMD.XY) != M2Camera.IMMD.XY)
				{
					this.walkFn(this, this.depx, this.depy, speed);
				}
				if ((immediate & M2Camera.IMMD.IM_X) != (M2Camera.IMMD)0)
				{
					this.x = this.depx;
					this.cam_walk_speed_x = this.cam_walk_speed_default;
					if ((this.pos_center & M2Camera.IMMD.IM_X) == (M2Camera.IMMD)0 && (!flag4 || (immediate & M2Camera.IMMD._FORCE) != (M2Camera.IMMD)0))
					{
						this.pos_center |= M2Camera.IMMD.IM_X;
					}
				}
				if ((immediate & M2Camera.IMMD.IM_Y) != (M2Camera.IMMD)0)
				{
					this.y = this.depy;
					this.cam_walk_speed_y = this.cam_walk_speed_default;
					if ((this.pos_center & M2Camera.IMMD.IM_Y) == (M2Camera.IMMD)0 && (!flag4 || (immediate & M2Camera.IMMD._FORCE) != (M2Camera.IMMD)0))
					{
						this.pos_center |= M2Camera.IMMD.IM_Y;
					}
				}
				if ((this.pos_center & M2Camera.IMMD.XY) == M2Camera.IMMD.XY)
				{
					flag2 = true;
				}
			}
			this.callImmediateMove();
			return flag2;
		}

		private void fixPosition(ref float depx, ref float depy, float xval, float yval)
		{
			if (M2Camera.no_limit_camera)
			{
				depx = xval;
				depy = yval;
				return;
			}
			if (this.scale == 1f && this.cam_x_fix)
			{
				this.x = (depx = this.map_dwh);
			}
			else
			{
				float vareawh = this.vareawh;
				depx = X.MMX2((float)this.crop_mapwh * this.CLEN + vareawh, xval, this.map_dw - (float)this.crop_mapwh * this.CLEN - vareawh);
			}
			if (this.scale == 1f && this.cam_y_fix)
			{
				this.y = (depy = this.map_dhh);
				return;
			}
			float vareahh = this.vareahh;
			depy = X.MMX2((float)this.crop_mapwh * this.CLEN + vareahh, yval, this.map_dh - (float)this.crop_mapwh * this.CLEN - vareahh);
		}

		public void defaultWalkFn(M2Camera Cam, float depx, float depy, float speed)
		{
			float num = X.ScrPow(0.25f, X.IntC(X.Mn(6f, speed)));
			Cam.x = X.VALWALKMXR(Cam.x, depx, Cam.cam_walk_speed_x * speed, num);
			Cam.y = X.VALWALKMXR(Cam.y, depy, Cam.cam_walk_speed_y * speed, num);
		}

		public M2Camera callImmediateMove()
		{
			if (this.Md == null)
			{
				return this;
			}
			float num = this.base_scale;
			float num2 = this.scale_rev / this.PosEffectiveScaleMul.x;
			float num3 = this.scale_rev / this.PosEffectiveScaleMul.y;
			float num4 = (float)X.IntR((this.Qu.x * 64f * num2 + (this.x - this.map_dwh)) * this.Md.mover_scale) / this.Md.mover_scale;
			float num5 = (float)X.IntR((this.Qu.y * 64f * num3 - (this.y - this.map_dhh)) * this.Md.mover_scale) / this.Md.mover_scale;
			if (this.scale == (float)((int)this.scale))
			{
				num4 = (float)((int)(num4 * this.scale)) * this.scale_rev;
				num5 = (float)((int)(num5 * this.scale)) * this.scale_rev;
			}
			float num6 = num4 * 0.015625f * num;
			float num7 = num5 * 0.015625f * num;
			if (this.PosMainTransform.x != num6 || this.PosMainTransform.y != num7)
			{
				this.PosMainTransform.x = num6;
				this.PosMainTransform.y = num7;
				this.CamTrs.localPosition = this.PosMainTransform;
				for (int i = this.ACam.Length - 1; i >= 0; i--)
				{
					this.ACam[i].scaling_need_fine_projection = true;
				}
				if (this.M2D.curMap != null && this.M2D.curMap.MovRenderer != null && X.LENGTHXYN(this.PreRenderTicketChecked.x, this.PreRenderTicketChecked.y, num6, num7) >= 2f * this.CLEN * 0.015625f)
				{
					this.M2D.curMap.MovRenderer.need_clip_check = true;
					this.PreRenderTicketChecked.Set(num6, num7);
				}
			}
			return this;
		}

		public void setEditorSetPosAndScale(float _x, float _y, float _scale = 0f)
		{
			this.x = _x;
			this.y = _y;
			this.pos_center = M2Camera.IMMD.IMMEDIATE_FORCE;
			this.fine_focus_area = true;
			if (_scale > 0f)
			{
				this.scale = this.fixScaleFloat(_scale);
				this.scale_rev = 1f / this.scale;
			}
			this.anm_scale_maxt = 0;
			this.callImmediateMove();
			this.fineScale(false);
		}

		public void scalingWheel(bool enlarge)
		{
			float num = (this.scale - 0.5f) / 3.5f;
			float num2 = X.NI(0.01f, 0.05f, X.ZPOW(this.scale - 0.5f, 0.5f));
			if (enlarge)
			{
				this.setEditorSetPosAndScale(this.x, this.y, X.VALWALK(this.scale, 4f, num2));
				return;
			}
			this.setEditorSetPosAndScale(this.x, this.y, X.VALWALK(this.scale, 0.5f, num2));
		}

		public void scrollingWheel(bool to_l, bool to_t, bool to_r, bool to_b)
		{
			this.setEditorSetPosAndScale(this.x + (to_l ? (-1.5f) : (to_r ? 1.5f : 0f)), this.y + (to_t ? (-1.5f) : (to_b ? 1.5f : 0f)), 0f);
		}

		public void setWH(float _w = -1000f, float _h = -1000f, bool set_xy = false, bool divide_base_scale = false)
		{
			if (_w > 0f)
			{
				this.areaw = _w / (divide_base_scale ? this.base_scale : 1f);
			}
			if (_h > 0f)
			{
				this.areah = _h / (divide_base_scale ? this.base_scale : 1f);
			}
			float num = this.areawh;
			float num2 = this.areahh;
			this.areawh = this.areaw * 0.5f;
			this.areahh = this.areah * 0.5f;
			if (set_xy)
			{
				float num3 = num - this.areawh;
				float num4 = num2 - this.areahh;
				this.depx += num3;
				this.x += num3;
				this.depy += num4;
				this.y += num4;
			}
			this.pos_center &= (M2Camera.IMMD)252;
			this.fine_focus_area = true;
		}

		public bool moveBy(float mpx = 0f, float mpy = 0f, float _speed = 4f)
		{
			return this.moveTo(this.depx + mpx, this.depy + mpy, _speed);
		}

		public bool moveTo(float mpx, float mpy, float _speed = 4f)
		{
			this.fixPosition(ref this.depx, ref this.depy, mpx * this.CLEN, mpy * this.CLEN);
			if (_speed > 0f)
			{
				this.cam_walk_speed_y = _speed;
				this.cam_walk_speed_x = _speed;
			}
			this.pos_center &= (M2Camera.IMMD)252;
			this.fine_focus_area = true;
			this.camera_moved++;
			return true;
		}

		public bool moveToPt(Vector2 P)
		{
			return P.x != -1000f && this.moveTo(P.x, P.y, 4f);
		}

		public bool moveToLabelPt(string label)
		{
			Vector2 vector = new Vector2(-1000f, -1000f);
			return this.Md.getCenter(label, ref vector, false) && this.moveToPt(vector);
		}

		protected bool checkDeclineArea(bool consider_dep_limit_camera, out M2Camera.IMMD sliced)
		{
			float num = this.depx;
			float num2 = this.depy;
			int num3 = this.AFoc.Count;
			int num4 = 0;
			sliced = (M2Camera.IMMD)0;
			if (this.fine_focus_area_ == 0)
			{
				this.FocusTo_ = null;
			}
			else if (this.fine_focus_area_ == 2 && this.scale < 1.1f)
			{
				this.fine_focus_area_ = 1;
				this.FocusTo_ = null;
				if (num3 != 0 && this.MvCenter != null)
				{
					float num5 = -1f;
					for (int i = num3 - 1; i >= 0; i--)
					{
						M2LpCamFocus m2LpCamFocus = this.AFoc[i];
						if (m2LpCamFocus.active)
						{
							if (m2LpCamFocus.only_foot != 0)
							{
								M2Phys physic = this.MvCenter.getPhysic();
								if (physic == null || physic.hasFoot() != m2LpCamFocus.only_foot > 0)
								{
									goto IL_0159;
								}
							}
							if (!m2LpCamFocus.run(1f))
							{
								this.AFoc.RemoveAt(i);
								num3--;
							}
							else
							{
								if (num3 == 1)
								{
									this.FocusTo_ = m2LpCamFocus;
									break;
								}
								float num6 = X.LENGTHXYS(this.MvCenter.drawx, this.MvCenter.drawy, m2LpCamFocus.mapfocx * this.CLEN, m2LpCamFocus.mapfocy * this.CLEN);
								if (this.FocusTo_ == null || num5 > num6)
								{
									num5 = num6;
									this.FocusTo_ = m2LpCamFocus;
								}
							}
						}
						IL_0159:;
					}
				}
			}
			if (this.FocusTo_ != null && this.scale < 1.1f && this.FocusTo_.active)
			{
				if (this.FocusTo_.focus_level_x > 0f || this.FocusTo_.focus_shift_x != 0f)
				{
					num = (this.depx = X.NI(num, this.FocusTo_.mapfocx * this.CLEN, this.FocusTo_.focus_level_x) + this.FocusTo_.focus_shift_x);
					num4++;
				}
				if (this.FocusTo_.focus_level_y > 0f || this.FocusTo_.focus_shift_y != 0f)
				{
					num2 = (this.depy = X.NI(num2, this.FocusTo_.mapfocy * this.CLEN, this.FocusTo_.focus_level_y) + this.FocusTo_.focus_shift_y);
					num4++;
				}
			}
			num3 = this.ADecl.Count;
			if (num3 != 0)
			{
				float num7 = 1f / (this.getScale(false) * this.PosEffectiveScaleMul.x);
				float num8 = X.Mn(this.vareawh_focus_max * this.scale_rev, this.vareawh);
				float num9 = X.Mn(this.vareahh_focus_max * this.scale_rev, this.vareahh);
				float num10 = this.depx - num8 * num7;
				float num11 = this.depx + num8 * num7;
				float num12 = this.depy - num9 * num7;
				float num13 = this.depy + num9 * num7;
				float num14 = num10;
				float num15 = num11;
				float num16 = num12;
				float num17 = num13;
				float num18 = num - num8 * num7;
				float num19 = num2 - num9 * num7;
				float num20 = num + num8 * num7;
				float num21 = num2 + num9 * num7;
				if (!this.do_not_consider_decline_area_)
				{
					for (int j = 0; j < num3; j++)
					{
						M2LpCamDecline m2LpCamDecline = this.ADecl[j];
						if (m2LpCamDecline != null && m2LpCamDecline.active && m2LpCamDecline.isEnable(this.depx, this.depy, num18, num19, num20, num21))
						{
							float num22 = m2LpCamDecline.x + m2LpCamDecline.width * 0.5f;
							float num23 = m2LpCamDecline.y + m2LpCamDecline.height * 0.5f;
							bool flag = X.isContaining(m2LpCamDecline.x, m2LpCamDecline.right, num18, num20, -2f);
							bool flag2 = X.isContaining(m2LpCamDecline.y, m2LpCamDecline.bottom, num19, num21, -2f);
							bool flag3 = m2LpCamDecline.isPushoutEnable(AIM.L);
							bool flag4 = m2LpCamDecline.isPushoutEnable(AIM.R);
							bool flag5 = m2LpCamDecline.isPushoutEnable(AIM.B);
							bool flag6 = m2LpCamDecline.isPushoutEnable(AIM.T);
							float num24 = 0f;
							float num25 = 0f;
							if (!flag && (flag3 || flag4))
							{
								if (flag3 && flag4)
								{
									num24 = ((num < num22) ? X.Mn(m2LpCamDecline.x - num20, 0f) : X.Mx(m2LpCamDecline.right - num18, 0f));
								}
								else
								{
									num24 = (flag4 ? X.Mx(m2LpCamDecline.right - num18, 0f) : X.Mn(m2LpCamDecline.x - num20, 0f));
								}
							}
							if (!flag2 && (flag6 || flag5))
							{
								if (flag6 && flag5)
								{
									num25 = ((num2 < num23) ? X.Mn(m2LpCamDecline.y - num21, 0f) : X.Mx(m2LpCamDecline.bottom - num19, 0f));
								}
								else
								{
									num25 = (flag5 ? X.Mx(m2LpCamDecline.bottom - num19, 0f) : X.Mn(m2LpCamDecline.y - num21, 0f));
								}
							}
							if (num24 != 0f && num25 != 0f)
							{
								if (X.Abs(num24) < X.Abs(num25))
								{
									num25 = 0f;
								}
								else
								{
									num24 = 0f;
								}
							}
							if (num24 < 0f && flag3)
							{
								num11 = X.Mn(num15 + num24, num11);
							}
							if (num24 > 0f && flag4)
							{
								num10 = X.Mx(num14 + num24, num10);
							}
							if (num25 < 0f && flag6)
							{
								num13 = X.Mn(num17 + num25, num13);
							}
							if (num25 > 0f && flag5)
							{
								num12 = X.Mx(num16 + num25, num12);
							}
						}
					}
				}
				if (num10 > num14 && num11 < num15)
				{
					this.depx = (num10 + num11) * 0.5f;
				}
				else if (num10 > num14)
				{
					this.depx += num10 - num14;
				}
				else if (num11 < num15)
				{
					this.depx += num11 - num15;
				}
				if (num12 > num16 && num13 < num17)
				{
					this.depy = (num12 + num13) * 0.5f;
				}
				else if (num12 > num16)
				{
					this.depy += num12 - num16;
				}
				else if (num13 < num17)
				{
					this.depy += num13 - num17;
				}
			}
			if (!consider_dep_limit_camera)
			{
				if (this.walk_margin_x > 0)
				{
					float num26 = this.depx - num;
					if (X.BTWW((float)(-(float)this.walk_margin_x), num26, (float)this.walk_margin_x))
					{
						this.depx = num;
					}
					else
					{
						this.depx = num + ((num26 > 0f) ? (num26 - (float)this.walk_margin_x) : (num26 + (float)this.walk_margin_x));
					}
				}
				if (this.walk_margin_y > 0)
				{
					float num27 = this.depy - num2;
					if (X.BTWW((float)(-(float)this.walk_margin_y), num27, (float)this.walk_margin_y))
					{
						this.depy = num2;
					}
					else
					{
						this.depy = num2 + ((num27 > 0f) ? (num27 - (float)this.walk_margin_y) : (num27 + (float)this.walk_margin_y));
					}
				}
			}
			if (X.Abs(this.depx - this.x) > 25f - (float)(num4 * 9))
			{
				sliced |= M2Camera.IMMD.IM_X;
			}
			if (X.Abs(this.depy - this.y) > 25f - (float)(num4 * 9))
			{
				sliced |= M2Camera.IMMD.IM_Y;
			}
			return sliced > (M2Camera.IMMD)0;
		}

		public void addCropping(M2LpCamDecline Lp)
		{
			if (this.ADecl.IndexOf(Lp) == -1)
			{
				this.ADecl.Add(Lp);
			}
		}

		public void remCropping(M2LpCamDecline Lp)
		{
			this.ADecl.Remove(Lp);
		}

		public void remCropping(string k)
		{
			for (int i = this.ADecl.Count - 1; i >= 0; i--)
			{
				if (this.ADecl[i].key == k)
				{
					this.ADecl.RemoveAt(i);
					return;
				}
			}
		}

		public void addFocusArea(M2LpCamFocus Lp)
		{
			this.AFoc.Add(Lp);
			if (this.fine_focus_area_ == 1)
			{
				this.fine_focus_area_ = 2;
			}
			if (this.Md.floort <= 2f)
			{
				this.pos_center |= M2Camera.IMMD._FORCE;
			}
		}

		public void remFocusArea(M2LpCamFocus Lp)
		{
			this.AFoc.Remove(Lp);
			if (this.fine_focus_area_ == 1)
			{
				this.fine_focus_area_ = 2;
			}
		}

		public void eventCloseRefocus(M2Mover _MvCenter)
		{
			this.assignBaseMover(_MvCenter, -1);
		}

		public void stabilizeFinalizeValot(bool stabilize)
		{
			if (this.ValotFinalize != null)
			{
				this.ValotFinalize.enabled = !stabilize;
				this.ValotFinalizeScreen.enabled = !stabilize;
			}
		}

		public float getFarLength()
		{
			return 999f;
		}

		public Vector2 getSoundCenter()
		{
			if (!(this.MvCenter != null))
			{
				return new Vector2(this.x / this.CLEN, this.y / this.CLEN);
			}
			return new Vector2(this.MvCenter.x, this.MvCenter.y);
		}

		public bool isBaseMap(Map2d _Md)
		{
			return _Md == this.Md;
		}

		public bool isBaseMover(M2Mover _MvCenter)
		{
			return _MvCenter == this.MvCenter;
		}

		public M2Mover getBaseMover()
		{
			return this.MvCenter;
		}

		public M2Camera setQuake(float slevel, int time, float elevel = -1f, int saf = 0)
		{
			this.Qu.Vib(slevel, (float)time, elevel, saf);
			return this;
		}

		public void blurCenterIfFocusing(M2Mover _MvCenter)
		{
			if (this.MvCenter == _MvCenter)
			{
				this.pos_center &= (M2Camera.IMMD)252;
				this.fine_focus_area = true;
			}
		}

		public void blurCenterForce()
		{
			this.pos_center &= (M2Camera.IMMD)252;
			this.fine_focus_area = true;
		}

		public bool isPosCenterMode()
		{
			return (this.pos_center & M2Camera.IMMD.XY) == M2Camera.IMMD.XY;
		}

		public void fineBaseShiftPixel(float pixel_x, float pixel_y, bool shift_to_pos = false)
		{
			this.cam_shift_x = pixel_x * 0.015625f;
			this.cam_shift_y = pixel_y * 0.015625f;
			this.fine_finalized_trs = true;
			this.fine_focus_area = true;
			this.fineFixBaseArea();
		}

		public bool finalize_screen_enabled
		{
			get
			{
				return this.finalize_screen_enabled_;
			}
			set
			{
				if (this.finalize_screen_enabled_ == value)
				{
					return;
				}
				this.finalize_screen_enabled_ = value;
				if (this.MdFinalize != null)
				{
					this.GobFinalize.SetActive(value);
				}
			}
		}

		public float effect_confuse
		{
			get
			{
				return this.effect_confuse_;
			}
			set
			{
				if (this.effect_confuse_ == value)
				{
					return;
				}
				this.effect_confuse_ = value;
				this.fine_finalized_trs_rot = true;
			}
		}

		public void translateIfCenter(M2Mover _MvCenter, float pixel_x, float pixel_y)
		{
			if (_MvCenter == this.MvCenter)
			{
				if (pixel_x != 0f)
				{
					if (this.scale == 1f && this.cam_x_fix)
					{
						this.x = (this.depx = this.map_dwh);
					}
					else
					{
						float num = this.depx;
						float vareawh = this.vareawh;
						float num2 = (float)this.crop_mapwh * this.CLEN + vareawh;
						float num3 = this.map_dw - (float)this.crop_mapwh * this.CLEN - vareawh;
						this.depx = X.MMX2(num2, this.depx + pixel_x, num3);
						this.x = X.MMX2(num2, this.x + this.depx - num, num3);
						if (this.depx != this.x && this.depx < this.x == pixel_x < 0f)
						{
							this.depx += (this.depx - this.x) * 0.5f;
						}
					}
				}
				if (pixel_y != 0f)
				{
					if (this.scale == 1f && this.cam_y_fix)
					{
						this.y = (this.depy = this.map_dhh);
						return;
					}
					float vareahh = this.vareahh;
					float num4 = this.depy;
					float num5 = (float)this.crop_mapwh * this.CLEN + vareahh;
					float num6 = this.map_dh - (float)this.crop_mapwh * this.CLEN - vareahh;
					this.depy = X.MMX2(num5, this.depy + pixel_y, num6);
					this.y = X.MMX2(num5, this.y + this.depy - num4, num6);
				}
			}
		}

		public float shiftx
		{
			get
			{
				return -(this.x - this.areawh + this.Qu.x);
			}
		}

		public float shifty
		{
			get
			{
				return -(this.y - this.areahh + this.Qu.y);
			}
		}

		public float lt_mapx
		{
			get
			{
				return (this.x - this.areawh) * this.rCLEN;
			}
		}

		public float lt_mapy
		{
			get
			{
				return (this.y - this.areahh) * this.rCLEN;
			}
		}

		public float map_center_x
		{
			get
			{
				return this.x * this.rCLEN;
			}
		}

		public float map_center_y
		{
			get
			{
				return this.y * this.rCLEN;
			}
		}

		public float CLEN
		{
			get
			{
				return this.M2D.CLEN;
			}
		}

		public float rCLEN
		{
			get
			{
				if (this.Md != null)
				{
					return this.Md.rCLEN;
				}
				return 1f / this.CLEN;
			}
		}

		public Camera CamForEditor
		{
			get
			{
				if (this.CamForEditor_ == null)
				{
					int num = this.ACam.Length;
					for (int i = 0; i < num; i++)
					{
						CameraComponentCollecter cameraComponentCollecter = this.ACam[i];
						if (cameraComponentCollecter.scale == 1f && cameraComponentCollecter.move_with_main)
						{
							this.CamForEditor_ = cameraComponentCollecter.Cam;
							break;
						}
					}
					if (this.CamForEditor_ == null)
					{
						return Camera.main;
					}
				}
				return this.CamForEditor_;
			}
		}

		public Camera CamForMover
		{
			get
			{
				if (this.CCMoverCamera == null)
				{
					int num = this.ACam.Length;
					int num2 = 1 << M2MovRenderContainer.drawer_mask_layer;
					for (int i = 0; i < num; i++)
					{
						CameraComponentCollecter cameraComponentCollecter = this.ACam[i];
						if ((cameraComponentCollecter.Cam.cullingMask & num2) != 0)
						{
							this.CCMoverCamera = cameraComponentCollecter;
							break;
						}
					}
					if (this.CCMoverCamera == null)
					{
						return Camera.main;
					}
				}
				return this.CCMoverCamera.Cam;
			}
		}

		public Vector2 fnGetLoc(float _x, float _y, float _z)
		{
			M2Camera.BufPt.x = this.shiftx + _x * this.CLEN;
			M2Camera.BufPt.y = this.shifty + _y * this.CLEN - _z;
			return M2Camera.BufPt;
		}

		public Vector2 getScaleTe()
		{
			return this.PosEffectiveScaleMul;
		}

		public void setScaleTe(Vector2 V)
		{
			if (V.x != this.PosEffectiveScaleMul.x || V.y != this.PosEffectiveScaleMul.y)
			{
				this.PosEffectiveScaleMul.Set(V.x, V.y);
				this.need_scale_fine = true;
			}
		}

		public float get_w()
		{
			return this.areaw;
		}

		public float get_h()
		{
			return this.areah;
		}

		public bool isCoveringMp(float mapl, float mapt, float mapr, float mapb, float extend_pixel)
		{
			return X.isCovering(this.x - this.areawh, this.x + this.areawh, mapl * this.CLEN, mapr * this.CLEN, extend_pixel) && X.isCovering(this.y - this.areahh, this.y + this.areahh, mapt * this.CLEN, mapb * this.CLEN, extend_pixel);
		}

		public bool isCoveringCenMeshPixel(float cen_x, float cen_y, float _w, float _h, float extend_pixel = 0f)
		{
			return this.Md != null && this.isCoveringCen(this.Md.meshx2pixel(cen_x), this.Md.meshy2pixel(cen_y), _w, _h, extend_pixel);
		}

		public bool isCoveringCen(float cen_x, float cen_y, float _w, float _h, float extend_pixel = 0f)
		{
			_w *= 0.5f;
			_h *= 0.5f;
			return X.isCovering(this.x - this.areawh, this.x + this.areawh, cen_x - _w, cen_x + _w, extend_pixel) && X.isCovering(this.y - this.areahh, this.y + this.areahh, cen_y - _h, cen_y + _h, extend_pixel);
		}

		public bool isCoveringEffectPixel(float elpx, float erpx, float ebpx, float etpx, float extend_pixel = 0f, float base_x_u = 0f, float base_y_u = 0f)
		{
			return this.isCoveringEffectUPos(elpx * 0.015625f + base_x_u, erpx * 0.015625f + base_x_u, ebpx * 0.015625f + base_y_u, etpx * 0.015625f + base_y_u, extend_pixel);
		}

		public bool isCoveringEffectUPos(float el, float er, float eb, float et, float extend_pixel = 0f)
		{
			float num = -X.Abs(this.cam_shift_x) * 0.5f + this.areawh * 0.015625f * this.base_scale * this.scale_rev;
			float num2 = this.areahh * 0.015625f * this.base_scale * this.scale_rev;
			float num3 = this.PosMainTransform.x;
			float num4 = this.PosMainTransform.y;
			return X.isCovering(el, er, num3 - num, num3 + num, extend_pixel * 0.015625f) && X.isCovering(eb, et, num4 - num2, num4 + num2, extend_pixel * 0.015625f);
		}

		public float getScale(bool real_scale = false)
		{
			if (this.anm_scale_maxt > 0 && !real_scale)
			{
				return this.anm_scale_d;
			}
			if (!real_scale)
			{
				return this.scale;
			}
			return this.scale * this.PosEffectiveScaleMul.x;
		}

		public float getScaleRev()
		{
			return this.scale_rev;
		}

		public Vector2 getQuakeXyPixel()
		{
			return new Vector2(this.Qu.x, this.Qu.y) * 64f;
		}

		public TransformMem getTransformMem()
		{
			TransformMem transformMem = new TransformMem(null);
			float num = this.getScale(false);
			transformMem.localScale.Set(num, num, 1f);
			transformMem.localPosition.Set(this.x, this.y, 0f);
			return transformMem;
		}

		public TransformMem setTransformMem(TransformMem Mem)
		{
			this.setEditorSetPosAndScale(Mem.localPosition.x, Mem.localPosition.y, Mem.localScale.x);
			return Mem;
		}

		public M2LpCamFocus FocusTo
		{
			get
			{
				return this.FocusTo_;
			}
		}

		public float viewable_pixel_width
		{
			get
			{
				float num = this.scale_rev / this.PosEffectiveScaleMul.x;
				return this.areaw * this.base_scale * num;
			}
		}

		public float viewable_pixel_height
		{
			get
			{
				float num = this.scale_rev / this.PosEffectiveScaleMul.y;
				return this.areah * this.base_scale * num;
			}
		}

		public float vareawh
		{
			get
			{
				return (this.areawh - X.Abs(this.cam_shift_x * 64f) / this.base_scale) * this.scale_rev;
			}
		}

		public float vareahh
		{
			get
			{
				return (this.areahh - X.Abs(this.cam_shift_y * 64f) / this.base_scale) * this.scale_rev;
			}
		}

		public float getShiftedCenterX(float rate)
		{
			return -this.shiftx * rate + this.areaw * 0.5f;
		}

		public float getShiftedCenterY(float rate)
		{
			return -this.shifty * rate + this.areah * 0.5f;
		}

		public float getAfterShiftX()
		{
			return this.cam_shift_x;
		}

		public float getAfterShiftY()
		{
			return this.cam_shift_y;
		}

		public float getCamAreaZ()
		{
			return this.CamTrs.position.z;
		}

		public Vector2 PosMainMoverShift
		{
			get
			{
				if (this.MvCenter is M2MoverPr)
				{
					float num = this.Md.map2globalux(this.MvCenter.x);
					float num2 = this.Md.map2globaluy(this.MvCenter.mbottom - 1.5f);
					return new Vector2((num - this.PosMainTransform.x) * this.scale_rev, (num2 - this.PosMainTransform.y) * this.scale_rev);
				}
				return Vector2.zero;
			}
		}

		public bool do_not_consider_decline_area
		{
			get
			{
				return this.do_not_consider_decline_area_;
			}
			set
			{
				if (value == this.do_not_consider_decline_area_)
				{
					return;
				}
				this.do_not_consider_decline_area_ = value;
				this.pos_center &= (M2Camera.IMMD)252;
				this.fine_focus_area = true;
			}
		}

		public byte update_final_mesh_alpha255
		{
			get
			{
				return this.update_final_mesh_alpha255_;
			}
			set
			{
				if (this.update_final_mesh_alpha255_ != value)
				{
					this.update_final_mesh_alpha255_ = value;
					this.fine_finalized_mesh = true;
				}
			}
		}

		public void setCamWalkSpeedX(float _x)
		{
			if ((this.pos_center & M2Camera.IMMD.IM_X) == (M2Camera.IMMD)0)
			{
				this.cam_walk_speed_x = X.Mx(this.cam_walk_speed_x, X.Abs(_x));
			}
		}

		public void setCamWalkSpeedY(float _y)
		{
			if ((this.pos_center & M2Camera.IMMD.IM_Y) == (M2Camera.IMMD)0)
			{
				this.cam_walk_speed_y = X.Mx(this.cam_walk_speed_y, X.Abs(_y));
			}
		}

		public Camera get_FinalCamera()
		{
			if (this.CCFinalCamera == null)
			{
				return null;
			}
			return this.CCFinalCamera.Cam;
		}

		public MeshRenderer GetMeshRendererFromCamera(MeshDrawer Md)
		{
			return this.MMRD.GetMeshRenderer(Md);
		}

		public int getFinalRenderedLayer()
		{
			if (this.CCFinalCamera == null)
			{
				return 0;
			}
			return this.CCFinalCamera.Cam.gameObject.layer;
		}

		public int getFinalSourceRenderedLayer()
		{
			if (this.CCFinalCameraSource == null)
			{
				return 0;
			}
			return this.CCFinalCameraSource.Cam.gameObject.layer;
		}

		public Camera getFinalSourceRenderedCamera()
		{
			if (this.CCFinalCameraSource == null)
			{
				return null;
			}
			return this.CCFinalCameraSource.Cam;
		}

		public bool isFinalizeLayer(int layer)
		{
			return this.CCFinalCameraSource == null || (this.CCFinalCameraSource.Cam.cullingMask & (1 << layer)) != 0;
		}

		public MeshDrawer createLightMesh(string name, BLEND blnd = BLEND.NORMAL)
		{
			MeshDrawer meshDrawer = new MeshDrawer(null, 4, 6);
			meshDrawer.draw_gl_only = true;
			meshDrawer.activate(name, MTRX.MIicon.getMtr(blnd, -1), false, MTRX.ColWhite, null);
			return meshDrawer;
		}

		public MeshDrawer getLightMesh(bool clearing)
		{
			if (clearing)
			{
				this.MainLightMeshDrawer.clear(true, false);
			}
			return this.MainLightMeshDrawer;
		}

		public RenderTexture getLightTexture()
		{
			return this.LightCamX.getTexture();
		}

		public void getTextureForUiBg(ref RenderTexture TxB, ref RenderTexture TxG)
		{
			if (this.CurDgn != null)
			{
				this.CurDgn.getTextureForUiBg(ref TxB, ref TxG);
			}
			if (TxB == null)
			{
				TxB = this.getFinalizedTexture();
			}
		}

		public Color32 transparent_color
		{
			get
			{
				return this.transparent_color_;
			}
			set
			{
				value.a = 0;
				if (value.Equals(this.transparent_color_))
				{
					return;
				}
				this.transparent_color_ = value;
				if (this.ACam == null)
				{
					return;
				}
				int num = this.ACam.Length;
				for (int i = 1; i < num; i++)
				{
					Camera cam = this.ACam[i].Cam;
					if (cam.backgroundColor.a == 0f)
					{
						cam.backgroundColor = value;
					}
				}
			}
		}

		public RenderTexture getBackGroundRendered()
		{
			if (this.CurDgn == null)
			{
				return null;
			}
			return this.CurDgn.getBackGroundRendered();
		}

		public RenderTexture getFinalizedTexture()
		{
			if (this.CCFinalCameraSource == null)
			{
				return null;
			}
			return this.CCFinalCameraSource.Cam.targetTexture;
		}

		public RenderTexture getFinalizeExportTexture()
		{
			if (this.CCFinalCamera == null || !(this.CCFinalCamera.Cam.targetTexture != null))
			{
				return this.getFinalizedTexture();
			}
			return this.CCFinalCamera.Cam.targetTexture;
		}

		public CameraComponentCollecter getMoverCameraCC()
		{
			return this.CCMoverCamera;
		}

		public RenderTexture getMoverTexture()
		{
			if (this.CCMoverCamera == null)
			{
				return null;
			}
			return this.CCMoverCamera.Cam.targetTexture;
		}

		public GameObject getGameObject()
		{
			return this.CamGob;
		}

		public void addMapFinalizedListener(FnMapFinalized Fn)
		{
			if (this.AFnMapFinalized == null)
			{
				this.AFnMapFinalized = new FnMapFinalized[4];
			}
			if (Fn != null)
			{
				X.pushToEmptyRI<FnMapFinalized>(ref this.AFnMapFinalized, Fn, -1);
			}
		}

		private bool runMapFinalized()
		{
			if (this.AFnMapFinalized == null)
			{
				return true;
			}
			int num = this.AFnMapFinalized.Length;
			for (int i = 0; i < num; i++)
			{
				FnMapFinalized fnMapFinalized = this.AFnMapFinalized[i];
				if (fnMapFinalized == null)
				{
					break;
				}
				if (!fnMapFinalized(this.CCFinalCameraSource.Cam, this.CCFinalCamera.Cam, this.M2D))
				{
					return false;
				}
			}
			return true;
		}

		public string is_pos_center
		{
			get
			{
				return FEnum<M2Camera.IMMD>.ToStr(this.pos_center);
			}
		}

		protected Map2d Md;

		protected M2Mover MvCenter;

		public static readonly int layer_finalrendered = LayerMask.NameToLayer("FinalRendered");

		public static readonly int layerlight = IN.LAY("M2DLight");

		protected float areaw = IN.w;

		protected float areah = IN.h;

		protected float areawh = IN.w * 0.5f;

		protected float areahh = IN.h * 0.5f;

		private bool cam_x_fix = true;

		private bool cam_y_fix = true;

		public float map_dw;

		public float map_dh;

		public float map_dwh;

		public float map_dhh;

		public static Vector3 BufPt;

		public readonly M2DBase M2D;

		public float x;

		public float y;

		public float depx;

		public float depy;

		public float vareawh_focus_max;

		public float vareahh_focus_max;

		public int crop_mapwh;

		protected float anm_scale_s;

		protected float anm_scale_d;

		protected int anm_scale_t;

		protected int anm_scale_maxt;

		public Quaker Qu;

		protected float scale = 1f;

		protected float scale_rev = 1f;

		public readonly float base_scale = 2f;

		private byte fine_focus_area_ = 2;

		private bool do_not_consider_decline_area_;

		public float cam_shift_x;

		public float cam_shift_y;

		public float cam_walk_speed_default = 2f;

		public float cam_walk_speed_x = 2f;

		public float cam_walk_speed_y = 2f;

		public int walk_margin_x;

		public int walk_margin_y;

		private Vector2 PosEffectiveScaleMul;

		public TransEffecter TeCon;

		public bool need_scale_fine;

		public bool stabilize_drawing;

		private bool finalize_screen_enabled_ = true;

		private M2Camera.IMMD pos_center = M2Camera.IMMD.XY;

		public static bool no_limit_camera = false;

		public Dungeon CurDgn;

		public int camera_moved = 1;

		public M2Camera.IMMD default_immediate = M2Camera.IMMD.AUTO;

		public int fix_to_player;

		public int layer_mask_def;

		public const int TEXTURE_MARGIN = 8;

		public FnCameraWalk walkFn;

		public FnCameraPostRender fnPostRender;

		private Camera CamForEditor_;

		private CameraComponentCollecter[] ACam;

		private XCameraBase[] AXcBase;

		private Material[] AScaleListenerMaterial;

		private int cnt_scale_material;

		private M2MeshContainer MMRD;

		private Transform CamTrs;

		private CameraComponentCollecter CCFinalCamera;

		private CameraComponentCollecter CCFinalCameraSource;

		private CameraComponentCollecter CCMoverCamera;

		private ProjectionContainer ForMainProjection;

		public Vector3 PosMainTransform;

		private M2LpCamFocus FocusTo_;

		private GameObject CamGob;

		private XCameraTx LightCamX;

		private MeshDrawer MainLightMeshDrawer;

		private MeshDrawer MdFinalize;

		private GameObject GobFinalize;

		private Material MtrFinalize;

		private Material MtrFinalRendered;

		private ValotileRenderer ValotFinalize;

		private ValotileRenderer ValotFinalizeScreen;

		private Transform TrsFinalizeScreen;

		private Transform TrsFinalizeExport;

		public M2MovRenderContainer MovRender;

		private List<MeshDrawer> ATempCreatingMd;

		private List<float> Ameshdrawer_scale;

		public const float LIGHT_TEX_SCALE = 0.125f;

		protected List<M2LpCamDecline> ADecl;

		protected List<M2LpCamFocus> AFoc;

		public const int DEFAULT_SCALING_ANIMATE_TIME = 40;

		private FnMapFinalized[] AFnMapFinalized;

		private bool fine_finalized_mesh = true;

		private bool fine_finalized_trs = true;

		private bool fine_finalized_trs_rot = true;

		private RenderTexture TxFinalCameraCache;

		public bool frame_rendered = true;

		public bool need_initialize_draw = true;

		public Vector3 viewport_scale_vector = Vector3.one;

		public Vector2Int pre_frame_moved;

		private byte update_final_mesh_alpha255_ = byte.MaxValue;

		private float light_shift_x;

		private float light_shift_y;

		private Color32 transparent_color_ = new Color32(0, 0, 0, 0);

		private float effect_confuse_;

		private Vector2 PreRenderTicketChecked;

		public Matrix4x4 FinalProjBuffer;

		private List<RenderTexturePool> ARdPool;

		private const string string_render = "Render- ";

		private const string string_Render_Finalize = "Render-Finalize ";

		public enum IMMD : byte
		{
			IM_X = 1,
			IM_Y,
			XY,
			IMMEDIATE_FORCE = 11,
			_FORCE = 8,
			AUTO = 32
		}
	}
}
