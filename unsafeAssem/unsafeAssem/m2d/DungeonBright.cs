using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonBright : Dungeon
	{
		public DungeonBright(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.effect_layer_top = base.LAY("ChipsRendered");
			this.effect_layer_bottom = LayerMask.NameToLayer("ForFinalRender");
			this.effect_layer_t_bottom = base.LAY("FinalRendered");
			this.dcback_mask = base.LAYB("ChipsSubBottom");
			this.dcbackrend_lay = this.effect_layer_top;
			this.use_finalsource_depth = true;
			this.use_rain = true;
		}

		protected override Shader ShaderBasicChipBg
		{
			get
			{
				return M2DBase.getShd("M2d/BasicMapWithLight");
			}
		}

		protected override Shader ShaderBasicChipTop
		{
			get
			{
				return M2DBase.getShd("M2d/BasicMapWithLight");
			}
		}

		protected virtual Shader ShaderForBackRendered
		{
			get
			{
				return M2DBase.getShd("M2d/ImageWithLightAndBright");
			}
		}

		public virtual Material createMaterilForBackRendered(MImage MI, int stencil_ref = -1)
		{
			Material material;
			if (MI == null)
			{
				material = base.newMtr(this.ShaderForBackRendered);
			}
			else
			{
				material = base.getWithLightTextureMaterial(MI, this.ShaderForBackRendered, stencil_ref);
			}
			return material;
		}

		public override void DisposeMaterial()
		{
			base.DisposeMaterial();
			this.MtrBrightPrepare_ = null;
			this.MtrBorder = null;
		}

		public Material MtrBrightPrepare
		{
			get
			{
				if (this.MtrBrightPrepare_ == null)
				{
					this.MtrBrightPrepare_ = base.newMtr("M2d/BrightPrepare");
				}
				return this.MtrBrightPrepare_;
			}
		}

		protected virtual float blur_texture_scale
		{
			get
			{
				return 0.25f;
			}
		}

		public override void resetColor()
		{
			base.resetColor();
			DgnColorContainer colCon = this.DGN.ColCon;
			this.LightMulColor = colCon.blend3("LightMul", this.night_level, uint.MaxValue);
			this.LightMulDarkColor = colCon.blend3("LightMulDark", this.night_level, uint.MaxValue);
			this.BackMulColor = colCon.blend3("BackMul", this.night_level, uint.MaxValue);
			this.BackMulBottomColor = colCon.blend3("BackMulBottom", this.night_level, uint.MaxValue);
			this.TopMulColor = colCon.blend3("TopMul", this.night_level, uint.MaxValue);
			this.TopMulBottomColor = colCon.blend3("TopMulBottom", this.night_level, uint.MaxValue);
			this.SMMulColor = colCon.blend3("SMMul", this.night_level, uint.MaxValue);
			this.SMMulBottomColor = colCon.blend3("SMMulBottom", this.night_level, uint.MaxValue);
			this.BackBaseColor = colCon.blend3("BackBase", this.night_level, uint.MaxValue);
			this.BehindChipSubColor = colCon.blend3("BehindChipSub", this.night_level, uint.MaxValue);
			this.RenderMulColorTop = colCon.blend3("RenderMulTop", this.night_level, uint.MaxValue);
			this.RenderMulColorBottom = colCon.blend3("RenderMulBottom", this.night_level, uint.MaxValue);
			this.ColWater = colCon.blend3("Water", this.night_level, uint.MaxValue);
			this.ColWaterBottom = colCon.blend3("WaterBottom", this.night_level, uint.MaxValue);
			this.WaterLightBaseColor = colCon.blend3("WaterLightBase", this.night_level, uint.MaxValue);
			this.OutGradationColor = colCon.blend3("OutGradation", this.night_level, uint.MaxValue);
			this.BgColorTopRain = colCon.blend3("BgTopRain", this.night_level, uint.MaxValue);
			this.BgColorBottomRain = colCon.blend3("BgBottomRain", this.night_level, uint.MaxValue);
		}

		public override void fineMaterialColor()
		{
			base.fineMaterialColor();
			float num = this.rain_level * 0.45f;
			DgnColorContainer colCon = this.DGN.ColCon;
			base.MtrChipTopLight.SetFloat("_TopLightLevel", X.NI(X.NI3(this.Achip_top_light_level, this.night_level), 0.14f, num));
			this.fineBackRenderMaterial(this.MtrBackRendered);
			base.MtrImageWithLight.SetColor("_White", this.LightMulColor);
			base.MtrImageWithLight.SetColor("_DarkColor", this.LightMulDarkColor);
			this.MtrBorder.SetFloat("_LightLevel", X.NI(X.NI3(this.Aborder_light_level, this.night_level), 0.95f, num));
			this.fine_water_drawer = true;
			if (this.MtrWater_ != null)
			{
				this.getWaterMaterial();
			}
			if (this.AMtrCacheBorderMask != null)
			{
				for (int i = this.AMtrCacheBorderMask.Count - 1; i >= 0; i--)
				{
					Material material = this.AMtrCacheBorderMask[i];
					if (material == null)
					{
						this.AMtrCacheBorderMask.RemoveAt(i);
					}
					else
					{
						this.fineBackRenderMaterial(material);
					}
				}
			}
		}

		public virtual void fineBackRenderMaterial(Material Mtr)
		{
			if (Mtr != null)
			{
				float num = this.rain_level * 0.45f;
				DgnColorContainer colCon = this.DGN.ColCon;
				Mtr.SetFloat("_RainLevel", num);
				Mtr.SetColor("_BlurColor", C32.MulA(this.BrightColor, 1f - this.rain_level * 0.75f));
				Mtr.SetColor("_BlurColorBottom", C32.MulA(this.BrightColorBottom, 1f - this.rain_level));
				Color32 color = colCon.blend3("BlurScreenColor", this.night_level, uint.MaxValue);
				Mtr.SetColor("_BlurScreenColor", color);
				Mtr.SetColor("_RenderMulColorTop", this.RenderMulColorTop);
				Mtr.SetColor("_RenderMulColorBottom", this.RenderMulColorBottom);
				Color color2 = colCon.blend3("BgLightLevel", this.night_level, uint.MaxValue);
				Mtr.SetFloat("_LightLevel", X.NI(color2.a, 1f, num));
				Mtr.SetTexture("_BlurTex", this.MainBlured);
				Mtr.SetTexture("_LightTex", base.M2D.Cam.getLightTexture());
			}
		}

		public override Material getChipMaterial(int layer, Material Mtr = null)
		{
			Material chipMaterial = base.getChipMaterial(layer, Mtr);
			if (this.SubMpData == null)
			{
				float num = this.rain_level * 0.45f;
				DgnColorContainer colCon = this.DGN.ColCon;
				if (layer == 0)
				{
					Color color = colCon.blend3("BgLightLevel", this.night_level, uint.MaxValue);
					chipMaterial.SetFloat("_LightLevel", X.NI(color.a, 1f, num));
					chipMaterial.SetColor("_BaseColor", this.BackBaseColor);
					chipMaterial.SetColor("_MulColor", this.BackMulColor);
					chipMaterial.SetColor("_MulBottomColor", this.BackMulBottomColor);
				}
				else if (layer == 2 || layer == 4)
				{
					Color color2 = colCon.blend3("BgLightLevel", this.night_level, uint.MaxValue);
					chipMaterial.SetFloat("_LightLevel", X.NI(color2.a, 0.75f, num));
					chipMaterial.SetColor("_BaseColor", this.TopDarkColor);
					chipMaterial.SetColor("_MulColor", this.TopMulColor);
					chipMaterial.SetColor("_MulBottomColor", this.TopMulBottomColor);
				}
			}
			else
			{
				chipMaterial.SetColor("_MulColor", this.SMMulColor);
				chipMaterial.SetColor("_MulBottomColor", this.SMMulBottomColor);
			}
			return chipMaterial;
		}

		public override Material getWaterMaterial()
		{
			if (this.MtrWater_ == null)
			{
				this.MtrWater_ = base.newMtr(this.DGN.MtrWaterInBright);
				this.fine_water_drawer = true;
			}
			if (this.fine_water_drawer && !Map2d.editor_decline_lighting)
			{
				this.MtrWater_.SetFloat("_ScreenMargin", 8f);
				this.MtrWater_.SetColor("_FillColor", this.BrightColor);
				this.MtrWater_.SetTexture("_MainTex", this.DCBack.targetTexture);
				this.MtrWater_.SetTexture("_MainTexB", this.DCMain.targetTexture);
				this.MtrWater_.SetTexture("_MainTexMask", this.DCBorder.targetTexture);
				this.MtrWater_.SetTexture("_MainTexMoverMask", this.DCMover.targetTexture);
				this.MtrWater_.SetTexture("_LightTex", base.M2D.Cam.getLightTexture());
				MTRX.setMaterialST(this.MtrWater_, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
				this.MtrWater_.SetFloat("_LightLevel", 0.4f);
				this.MtrWater_.SetFloat("_LightScale", 0.125f);
				this.MtrWater_.SetColor("_LightBaseColor", this.WaterLightBaseColor);
				this.MtrWater_.SetFloat("_StencilRef", 30f);
			}
			return this.MtrWater_;
		}

		public override bool drawUCol(MeshDrawer Md, Map2d Mp, bool fine_cam_transparent_col = true)
		{
			Dungeon.Ma.clear(Md);
			Md.clear(false, false);
			Dungeon.Ma.Set(true);
			Md.Col = Md.ColGrd.Set(this.DGN.ColCon.blend3("BgOut", this.night_level, uint.MaxValue)).blend(this.BgColorTopRain, this.rain_level).setA(255f)
				.C;
			Md.ColGrd.Set(this.DGN.ColCon.blend3("BgIn", this.night_level, uint.MaxValue)).blend(this.BgColorBottomRain, this.rain_level).setA(255f);
			Md.Identity();
			Md.Scale(1.7f, 1.4f, false);
			Md.InnerCircle(0f, 0f, IN.w, IN.h, 0f, 0f, IN.w / 2f * 0.9f, IN.h / 2f * 0.9f, IN.w / 2f * 0.3f, IN.h / 2f * 0.3f, true, false, 0f, 1f);
			Md.Identity();
			if (!this.ucol_setted)
			{
				this.ucol_setted = true;
				Mp.get_MMRD().transformCamTrace(Md);
			}
			Color32 color = this.DGN.ColCon.blend3("BgBottom", this.night_level, uint.MaxValue);
			color.a = byte.MaxValue;
			Dungeon.Ma.Set(false).setColAllGrdation(-IN.h * 0.5f * 1.2f, IN.h * 0.5f * 1.2f, color, Dungeon.C.Set(this.DGN.ColCon.blend3("BgTop", this.night_level, uint.MaxValue)).setA(255f).C, GRD.BOTTOM2TOP, false, true);
			if (fine_cam_transparent_col)
			{
				base.M2D.setCameraTransparentColor(Md.ColGrd.blend(Md.Col, 0.5f).C);
			}
			return true;
		}

		public override int reentryGradation(Map2d Mp, MeshDrawer MdU, MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT)
		{
			if (Mp.is_submap)
			{
				return 0;
			}
			if (this.MdOutSide == null)
			{
				Mp.get_MMRD().CreateMesh(ref this.MdOutSide, "-Bright-OutSide", 340.01f, MTRX.MtrMeshMul, base.LAY("Chips"), false, false, true, false);
			}
			MdMap mdOutSide = this.MdOutSide;
			mdOutSide.Col = this.OutGradationColor;
			mdOutSide.ColGrd.Set(mdOutSide.Col).setA1(0f);
			float num = (float)Mp.width - (float)Mp.M2D.Cam.crop_mapwh * Mp.CLEN * 2f;
			float num2 = (float)Mp.height - (float)Mp.M2D.Cam.crop_mapwh * Mp.CLEN * 2f;
			mdOutSide.RectDoughnut(0f, 0f, num, num2, 0f, 0f, num - 120f, num2 - 120f, false, 0f, 1f, false);
			mdOutSide.RectDoughnut(0f, 0f, (float)Mp.width, (float)Mp.height, 0f, 0f, num, num2, false, 0f, 0f, false);
			mdOutSide.updateForMeshRenderer(false);
			return 0;
		}

		public override Camera BackGroundCamera
		{
			get
			{
				return this.DCBack;
			}
		}

		protected virtual void createDCBack(M2Camera Cam)
		{
			this.DCBack = Cam.createCamera("back", true, true);
			this.DCBack.cullingMask = this.dcback_mask;
			this.DCBack.clearFlags = CameraClearFlags.Color;
			Cam.GetCameraCollecter(this.DCBack).do_not_use_by_buffer = true;
		}

		public override void initS(Map2d Mp)
		{
			base.initS(Mp);
			this.AMtrCacheBorderMask = null;
		}

		public override Dungeon initMap(Map2d _Mp, M2SubMap _SubMpData)
		{
			base.initMap(_Mp, _SubMpData);
			this.ucol_setted = false;
			return this;
		}

		public override Dungeon initCamera(M2Camera Cam)
		{
			base.initCamera(Cam);
			this.ucol_setted = false;
			this.fine_water_drawer = true;
			this.DCBase.cullingMask &= ~(base.LAYB("ChipsUCol") | base.LAYB("FinalRendered") | base.LAYB("Water") | base.LAYB("Default"));
			RenderTexture renderTexture = Cam.initBufforForLightScreenCamera();
			this.createDCBack(Cam);
			if (this.MtrBackRendered == null)
			{
				this.MtrBackRendered = this.createMaterilForBackRendered(null, -1);
				this.MtrBackRendered.name = "MtrBackRendered";
			}
			RenderTexture renderTexture2 = Cam.initBufferScreen(this.DCBack, 1f, null, RenderTextureFormat.ARGB32, false);
			Cam.createMeshDrawer(null, "-BackRender", renderTexture2, this.MtrBackRendered, this.dcbackrend_lay, uint.MaxValue, 1f, true);
			float blur_texture_scale = this.blur_texture_scale;
			RenderTexture renderTexture3 = Cam.initBufferScreen(null, blur_texture_scale, null, RenderTextureFormat.ARGB4444, false);
			if (this.MainBlured == null)
			{
				this.MainBlured = this.createBlurBufferTexture(renderTexture3);
			}
			if (this.MainBluredRenderer == null)
			{
				this.MainBluredRenderer = new DungeonBright.BrightCacheRenderer(renderTexture3, this.MainBlured, this.MtrBrightPrepare);
			}
			this.MainBluredRenderer.Mtr = this.MtrBrightPrepare;
			this.MainBluredRenderer.Src = renderTexture3;
			this.MainBluredRenderer.Dest = this.MainBlured;
			Cam.GetCameraCollecter(this.DCBase).assignRenderFunc(this.MainBluredRenderer);
			this.DCMain = Cam.createCamera("main", true, true);
			this.DCMain.cullingMask = Cam.layer_mask_def & ~(base.LAYB("FinalRendered") | base.LAYB("Water") | base.LAYB("ChipsSubBottom") | base.LAYB("ChipsUCol") | base.LAYB("Default") | base.LAYB("Enemy"));
			this.DCMover = Cam.createCamera("mover", true, true);
			this.DCMover.cullingMask = 1 << M2MovRenderContainer.drawer_mask_layer;
			RenderTexture renderTexture4 = Cam.initBufferScreen(this.DCMover, 1f, null, RenderTextureFormat.ARGB32, false);
			this.DCBorder = Cam.createCamera("border", true, true);
			this.DCBorder.cullingMask = base.LAYB("ChipsForBorder");
			this.EffectCamera_ = (this.DCMainRendered = Cam.initBufferScreenCamera("main_rendered", base.MtrImageWithLight, this.effect_layer_top, this.DCMain, 1f, uint.MaxValue, true, false));
			this.DCMainRendered.cullingMask |= base.LAYB("Water") | base.LAYB("Default") | base.LAYB("Enemy") | base.LAYB("ChipsUCol");
			base.defineFinalCamera(this.DCMainRendered);
			base.MtrImageWithLight.SetTexture("_LightTex", renderTexture);
			base.MtrImageWithLight.SetTexture("_MoverTex", renderTexture4);
			base.MtrChipBg.SetTexture("_LightTex", renderTexture);
			base.MtrChipBg.SetFloat("_LightScale", 0.125f);
			base.MtrChipTop.SetTexture("_LightTex", renderTexture);
			base.MtrChipTop.SetFloat("_LightScale", 0.125f);
			this.MtrBorder = this.MtrBorder ?? base.newMtr(this.DGN.MtrBorderLight);
			RenderTexture renderTexture5 = Cam.initBufferScreen(this.DCBorder, 0.5f, null, RenderTextureFormat.ARGB32, false);
			this.MtrBorder.SetTexture("_MainTex", renderTexture5);
			this.MtrBorder.SetTexture("_LightTex", renderTexture);
			Cam.createMeshDrawer(null, "-Border", renderTexture5, this.MtrBorder, this.layer_final, 4278190080U, 1f, true);
			this.fineMaterialColor();
			return this;
		}

		protected virtual RenderTexture createBlurBufferTexture(RenderTexture TexEffectBlur)
		{
			return new RenderTexture(TexEffectBlur.width, TexEffectBlur.height, 0, RenderTextureFormat.RG16)
			{
				name = "<RD>MainBlured"
			};
		}

		public override void closeCamera()
		{
			this.DCBack = null;
			if (this.MdOutSide != null)
			{
				try
				{
					M2MeshContainer mmrd = base.M2D.curMap.get_MMRD();
					if (mmrd != null)
					{
						mmrd.DestroyOne(this.MdOutSide);
					}
				}
				catch
				{
				}
			}
			this.MdOutSide = null;
			this.ucol_setted = false;
			BLIT.nDispose(this.MainBlured);
			this.MainBlured = null;
		}

		public override MeshDrawer checkBlurDrawing(M2Puts Cp, M2SubMap SubMap, ref PxlMeshDrawer Src)
		{
			Map2d mp = Cp.Mp;
			if (Cp.Img.Meta.no_blur_draw || !this.isBlurSubmap(SubMap))
			{
				return null;
			}
			float num = X.Mx(mp.SubMapData.scrollx - 1f, 0f);
			if (num <= 0f)
			{
				return null;
			}
			M2BlurImage bluredImage = mp.IMGS.Atlas.getBluredImage(Cp.Img, X.IntC(X.Mn(num * 7f + 15f, 90f)), this);
			Src = bluredImage.Mesh;
			return mp.getMeshDrawerForBluredChip(bluredImage, Cp);
		}

		public virtual bool isBlurSubmap(M2SubMap SubMap)
		{
			return SubMap != null && SubMap.order == SMORDER.TOP;
		}

		public override int getLayerForChip(int layer, Dungeon.MESHTYPE meshtype = Dungeon.MESHTYPE.CHIP)
		{
			if (this.SubMpData != null)
			{
				if (base.is_house_light_gradation_layer(layer))
				{
					return this.effect_layer_bottom;
				}
				if (this.SubMpData.order == SMORDER.TOP)
				{
					return base.LAY("FinalRendered");
				}
				if (this.SubMpData.order == SMORDER.SKY || this.SubMpData.order == SMORDER.BACK)
				{
					return base.LAY("ChipsSubBottom");
				}
			}
			if (layer <= 0)
			{
				return base.LAY("ChipsSubBottom");
			}
			if (layer == 2 || layer == 4)
			{
				return this.effect_layer_top;
			}
			return base.getLayerForChip(layer, meshtype);
		}

		public override bool canBakeSimplify(bool is_grd, int layer)
		{
			return !is_grd && layer != 3;
		}

		public override void getTextureForUiBg(ref RenderTexture TxB, ref RenderTexture TxG)
		{
			TxB = this.DCBack.targetTexture;
			TxG = this.DCBorder.targetTexture;
		}

		public override RenderTexture getBorderTexture()
		{
			if (!(this.DCBorder != null))
			{
				return null;
			}
			return this.DCBorder.targetTexture;
		}

		public override Material getWithLightTextureMaterial(MImage MI, Shader Shd = null, int stencil = -1)
		{
			Material material;
			if (Shd != null)
			{
				material = base.getWithLightTextureMaterial(MI, Shd, stencil);
			}
			else
			{
				material = this.createMaterilForBackRendered(MI, stencil);
			}
			if (this.AMtrCacheBorderMask == null)
			{
				this.AMtrCacheBorderMask = new List<Material>();
			}
			if (this.AMtrCacheBorderMask.IndexOf(material) == -1)
			{
				this.AMtrCacheBorderMask.Add(material);
			}
			this.fineBackRenderMaterial(material);
			return material;
		}

		protected Camera DCMover;

		protected Camera DCBack;

		protected Camera DCMain;

		protected Camera DCBorder;

		protected Camera DCMainRendered;

		protected Color32 TopChipMulColor = C32.d2c(4290036404U);

		protected Color32 BackBaseColor = C32.d2c(uint.MaxValue);

		protected Color32 BackMulColor = C32.d2c(uint.MaxValue);

		protected Color32 BackMulBottomColor = C32.d2c(uint.MaxValue);

		protected Color32 TopMulColor = C32.d2c(uint.MaxValue);

		protected Color32 TopMulBottomColor = C32.d2c(uint.MaxValue);

		protected Color32 SMMulColor = C32.d2c(uint.MaxValue);

		protected Color32 SMMulBottomColor = C32.d2c(uint.MaxValue);

		protected Color32 LightMulColor = C32.d2c(uint.MaxValue);

		protected Color32 LightMulDarkColor = C32.d2c(4285564268U);

		protected Color32 RenderMulColorTop = C32.d2c(4282664004U);

		protected Color32 RenderMulColorBottom = C32.d2c(uint.MaxValue);

		protected Color32 WaterLightBaseColor;

		protected Color32 OutGradationColor;

		protected bool ucol_setted;

		protected Material MtrBackRendered;

		protected Material MtrBorder;

		protected MdMap MdOutSide;

		protected Color32 BgColorTopRain = C32.d2c(4284047705U);

		protected Color32 BgColorBottomRain = C32.d2c(4286878340U);

		protected bool fine_water_drawer;

		protected RenderTexture MainBlured;

		private DungeonBright.BrightCacheRenderer MainBluredRenderer;

		private List<Material> AMtrCacheBorderMask;

		protected int dcback_mask;

		protected int dcbackrend_lay;

		protected float[] Aborder_light_level = new float[] { 0.12f, 0.42f, 0.63f };

		protected float[] Achip_top_light_level = new float[] { 0.2f, 0.44f, 1f };

		private Material MtrBrightPrepare_;

		protected class BrightCacheRenderer : ICameraRenderBinder
		{
			public BrightCacheRenderer(RenderTexture _Src, RenderTexture _Dest, Material _Mtr)
			{
				this.Src = _Src;
				this.Dest = _Dest;
				this.Mtr = _Mtr;
			}

			public float getFarLength()
			{
				return 3.5f;
			}

			public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
			{
				GL.LoadOrtho();
				Graphics.Blit(this.Src, this.Dest, this.Mtr);
				return true;
			}

			public override string ToString()
			{
				if (this._tostring == null)
				{
					STB stb = TX.PopBld(null, 0);
					this._tostring = stb.Add("BrightCacheRenderer: ", this.Mtr.name).ToString();
					TX.ReleaseBld(stb);
				}
				return this._tostring;
			}

			public RenderTexture Src;

			public RenderTexture Dest;

			public Material Mtr;

			private string _tostring;
		}
	}
}
