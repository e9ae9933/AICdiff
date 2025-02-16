using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class Dungeon : IBgmManageArea
	{
		protected virtual Shader ShaderBasicChip
		{
			get
			{
				return M2DBase.getShd("M2d/BasicMap");
			}
		}

		protected virtual Shader ShaderBasicChipBg
		{
			get
			{
				return M2DBase.getShd("M2d/BasicMap");
			}
		}

		protected virtual Shader ShaderBasicChipTop
		{
			get
			{
				return M2DBase.getShd("M2d/BasicMap");
			}
		}

		protected virtual Shader ShaderBasicChipSimplify
		{
			get
			{
				return M2DBase.getShd("M2d/BasicMap");
			}
		}

		protected virtual Shader ShaderSubMapTop
		{
			get
			{
				return MTRX.getShd("Hachan/ShaderGDTNormalBlur");
			}
		}

		public Dungeon(string _key, DungeonContainer _DGN)
		{
			if (Dungeon.C == null)
			{
				Dungeon.C = new C32();
				Dungeon.Ma = new MdArranger(null);
			}
			this.DGN = _DGN;
			this.ACreatedMtr = new List<Material>(4);
			this.color_family_key = _key;
			this.key = _key;
			if (this.key == "_editor")
			{
				this.color_family_key = "_";
			}
			this.effect_layer_bottom = LayerMask.NameToLayer("ForFinalRender");
			this.effect_layer_top = LayerMask.NameToLayer("Default");
			this.effect_layer_t_bottom = LayerMask.NameToLayer("FinalRendered");
			if (_key == "_editor")
			{
				this.submap_fillup_level = 0.2f;
			}
		}

		public void destruct()
		{
			this.DisposeMaterial();
		}

		public virtual void DisposeMaterial()
		{
			for (int i = this.ACreatedMtr.Count - 1; i >= 0; i--)
			{
				IN.DestroyOne(this.ACreatedMtr[i]);
			}
			this.MtrImageWithLight_ = (this.MtrBgColor_ = (this.MtrSubMapTop_ = (this.MtrChipTopLight_ = (this.MtrChip_ = (this.MtrChipBg_ = (this.MtrChipTop_ = (this.MtrChipForSimplify_ = (this.MtrWater_ = null))))))));
			this.ACreatedMtr.Clear();
		}

		public void reloadTexture(Texture nTx)
		{
			if (this.MtrSubMapTop_ != null)
			{
				this.MtrSubMapTop_.mainTexture = nTx;
			}
			if (this.MtrChipTopLight_ != null)
			{
				this.MtrChipTopLight_.mainTexture = nTx;
			}
			if (this.MtrChip_ != null)
			{
				this.MtrChip_.mainTexture = nTx;
			}
			if (this.MtrChipBg_ != null)
			{
				this.MtrChipBg_.mainTexture = nTx;
			}
			if (this.MtrChipTop_ != null)
			{
				this.MtrChipTop_.mainTexture = nTx;
			}
			if (this.MtrChipForSimplify_ != null)
			{
				this.MtrChipForSimplify_.mainTexture = nTx;
			}
		}

		protected Material BasicChipMtr(Material _Mtr, bool clone = true)
		{
			if (clone)
			{
				_Mtr = this.newMtr(_Mtr);
			}
			_Mtr.SetColor("_Color", C32.d2c(uint.MaxValue));
			_Mtr.SetFloat("_Map_Scale", 2f);
			_Mtr.SetFloat("_LightScale", 0.125f);
			return _Mtr;
		}

		public Material MtrChip
		{
			get
			{
				if (this.MtrChip_ == null)
				{
					this.MtrChip_ = this.BasicChipMtr(this.DGN.getMtr(this.ShaderBasicChip, -1), true);
				}
				return this.MtrChip_;
			}
		}

		public Material MtrChipBg
		{
			get
			{
				if (this.MtrChipBg_ == null)
				{
					this.MtrChipBg_ = this.BasicChipMtr(this.DGN.getMtr(this.ShaderBasicChipBg, -1), true);
				}
				return this.MtrChipBg_;
			}
		}

		public Material MtrChipTop
		{
			get
			{
				if (this.MtrChipTop_ == null)
				{
					this.MtrChipTop_ = this.BasicChipMtr(this.DGN.getMtr(this.ShaderBasicChipTop, -1), true);
				}
				return this.MtrChipTop_;
			}
		}

		public Material MtrChipForSimplify
		{
			get
			{
				if (this.MtrChipForSimplify_ == null)
				{
					this.MtrChipForSimplify_ = this.BasicChipMtr(this.DGN.getMtr(this.ShaderBasicChipSimplify, -1), true);
				}
				this.MtrChipForSimplify_.EnableKeyword("NO_PIXELSNAP");
				return this.MtrChipForSimplify_;
			}
		}

		public Material MtrChipTopLight
		{
			get
			{
				if (this.MtrChipTopLight_ == null)
				{
					this.MtrChipTopLight_ = this.newMtr(this.DGN.getMtr(M2DBase.getShd("M2d/ChipsTopLight"), -1));
				}
				return this.MtrChipTopLight_;
			}
		}

		public Material MtrBgColor
		{
			get
			{
				if (this.MtrBgColor_ == null)
				{
					this.MtrBgColor_ = this.newMtr(MTRX.MtrMeshNormal);
					this.MtrBgColor_.SetColor("_Color", C32.d2c(uint.MaxValue));
				}
				return this.MtrBgColor_;
			}
		}

		public Material MtrImageWithLight
		{
			get
			{
				if (this.MtrImageWithLight_ == null)
				{
					this.MtrImageWithLight_ = this.newMtr("M2d/ImageWithLight");
					this.MtrImageWithLight_.renderQueue = 3000;
				}
				return this.MtrImageWithLight_;
			}
		}

		public Material MtrSubMapTop
		{
			get
			{
				if (this.MtrSubMapTop_ == null)
				{
					this.MtrSubMapTop_ = this.newMtr(this.DGN.getMtr(this.ShaderSubMapTop, -1));
					this.MtrSubMapTop_.SetFloat("_Level", 0.2f);
				}
				return this.MtrSubMapTop_;
			}
		}

		protected Material newMtr(string shader_name)
		{
			return this.newMtr(M2DBase.getShd(shader_name));
		}

		protected Material newMtr(Shader Shd)
		{
			Material material = MTRX.newMtr(Shd);
			this.ACreatedMtr.Add(material);
			return material;
		}

		protected Material newMtr(Material Src)
		{
			Material material = MTRX.newMtr(Src);
			this.ACreatedMtr.Add(material);
			return material;
		}

		public virtual void resetColor()
		{
			DgnColorContainer colCon = this.DGN.ColCon;
			this.TopDarkColor = colCon.blend3("TopDark", this.night_level, uint.MaxValue);
			this.SMCpFrontDarkColor = colCon.blend3("SMCpFrontDark", this.night_level, uint.MaxValue);
			this.SMCpBackDarkColor = colCon.blend3("SMCpBackDark", this.night_level, uint.MaxValue);
			this.SMCpTopBaseColor = colCon.blend3("SMCpTopBase", this.night_level, uint.MaxValue);
			this.SMCpBackBaseColor = colCon.blend3("SMCpBackBase", this.night_level, uint.MaxValue);
			this.BrightColor = colCon.blend3("Bright", this.night_level, uint.MaxValue);
			this.BrightColorBottom = colCon.blend3("BrightBottom", this.night_level, uint.MaxValue);
			this.WaterSurfaceBottomColor = colCon.blend3("WaterSurfaceBottom", this.night_level, uint.MaxValue);
			this.WaterSurfaceTopColor = colCon.blend3("WaterSurfaceTop", this.night_level, uint.MaxValue);
			this.ColWater = colCon.GC("Water", 0, uint.MaxValue);
			this.ColWaterBottom = colCon.GC("WaterBottom", 0, uint.MaxValue);
			this.DGN.MtrHouseLight.SetColor("_HouseLightBrightB", colCon.blend3("HouseLightBrightB", this.night_level, uint.MaxValue));
			this.DGN.MtrHouseLight.SetColor("_HouseLightBrightW", colCon.blend3("HouseLightBrightW", this.night_level, uint.MaxValue));
		}

		public virtual void fineMaterialColor()
		{
		}

		public virtual Material getChipMaterial(int layer, Material Mtr = null)
		{
			if (layer == 3)
			{
				if (!(Mtr != null))
				{
					return this.MtrChipTopLight;
				}
				return Mtr;
			}
			else
			{
				if (this.SubMpData != null)
				{
					float num = this.SubMpData.camera_length;
					if (this.SubMpData.order == SMORDER.TOP)
					{
						if (Mtr == null)
						{
							Mtr = this.SubMpData.getChipMtr((this.key != "_editor") ? this.MtrChip : this.DGN.MtrSubMapBlending);
						}
						Mtr.SetColor("_Color", this.SMCpFrontDarkColor);
						Mtr.SetFloat("_Level", X.MMX(0f, (num - 1f) * 1.3f, 1f));
						Mtr.SetColor("_BrightColor", C32.d2c(16777215U));
						Mtr.SetColor("_MinColor", this.SMCpBackBaseColor);
					}
					else
					{
						num = num * (1f - this.camera_len_min_threshold) + this.camera_len_min_threshold;
						if (Mtr == null)
						{
							Mtr = this.SubMpData.getChipMtr(this.DGN.MtrSubMapBlending);
						}
						Mtr.SetColor("_Color", this.SMCpBackDarkColor);
						Mtr.SetColor("_MinColor", this.SMCpTopBaseColor);
						Mtr.SetFloat("_Level", this.submap_fillup_level + (1f - this.submap_fillup_level) * X.MMX(0f, (1f - num) * 1.78f, 1f));
						Mtr.SetColor("_BrightColor", this.DGN.ColCon.blend3("SMCpBright", this.night_level, uint.MaxValue));
						float num2 = X.MMX(0f, 1f - (num * 2f - 0.34f), 1f);
						Mtr.SetFloat("_BrightLevel", num2);
					}
					return Mtr;
				}
				if (Mtr != null)
				{
					return Mtr;
				}
				if (this.Mp.is_whole)
				{
					return this.DGN.MtrChipWhole;
				}
				if (layer == 0)
				{
					return this.MtrChipBg;
				}
				if (layer != 2 && layer != 4)
				{
					return this.MtrChip;
				}
				return this.MtrChipTop;
			}
		}

		public virtual Material getGradationMaterial(int layer, Material Mtr = null)
		{
			if (this.is_house_light_gradation_layer(layer))
			{
				return this.DGN.MtrHouseLight;
			}
			if (Mtr == null)
			{
				switch (layer)
				{
				case -1:
					return this.DGN.getMtr(M2DBase.getShd("M2d/BasicGradation"), -1);
				case 0:
				case 1:
				case 2:
				case 4:
					return this.DGN.getMtr(M2DBase.getShd("M2d/BasicGradation"), -1);
				}
				Mtr = null;
			}
			return Mtr ?? this.DGN.getMtr(BLEND.NORMAL, -1);
		}

		public virtual void initS(Map2d Mp)
		{
		}

		public virtual Dungeon initMap(Map2d _Mp, M2SubMap _SubMpData)
		{
			this.Mp = _Mp;
			this.SubMpData = _SubMpData;
			return this;
		}

		public virtual Dungeon initCamera(M2Camera Cam)
		{
			this.cam_finalized = false;
			Cam.clearCameraCompoent();
			if (!this.DGN.ColCon.SetFamily(this.color_family_key, true))
			{
				this.DGN.ColCon.SetFamily("_", false);
			}
			this.resetColor();
			this.DCBase = Cam.GetCameraComponent(0);
			this.DCBase.backgroundColor = MTRX.cola.Set(this.Mp.bgcol).setA1(1f).C;
			return this;
		}

		public void initMapFinalize(Map2d _Mp, M2SubMap _SubMpData)
		{
			this.initMap(_Mp, _SubMpData);
			if (!this.cam_finalized)
			{
				this.cam_finalized = true;
				this.M2D.Cam.initCameraFinalize(this.key == "_editor");
				this.fineMaterialColor();
			}
			this.DGN.MtrWaterFall.SetTexture("_MainTex", _Mp.M2D.Cam.getFinalizedTexture());
			this.DGN.MtrWaterFallT.SetTexture("_MainTex", this.getBackGroundRendered());
		}

		public virtual void initChipEffect<T>(Map2d _Mp, Effect<T> Ef, Effect<T>.FnCreateEffectItem _fnCreateEffectItem) where T : EffectItem
		{
			this.Mp = _Mp;
			this.SubMpData = this.Mp.SubMapData;
			Ef.layer_effect_top = this.getLayerForChip(1, Dungeon.MESHTYPE.EFFECT);
			Ef.layer_effect_bottom = this.getLayerForChip(0, Dungeon.MESHTYPE.EFFECT);
			Ef.initEffect("EFC[" + _Mp.ToString() + "]", this.Mp.M2D.Cam.getCameraComponentForLayer(1U << Ef.layer_effect_bottom), _fnCreateEffectItem, EFCON_TYPE.NORMAL);
			Ef.topBaseZ = this.getDrawZ(this.Mp.mode, 13) - 0.0004f;
			Ef.bottomBaseZ = this.getDrawZ(this.Mp.mode, 12) - 0.0004f;
		}

		public virtual float getDrawZ(MAPMODE mode, int layer)
		{
			if (layer - -11 > 1)
			{
				switch (layer)
				{
				case -1:
					if (mode != MAPMODE.NORMAL && !Map2d.isTempMode(mode))
					{
						return 0.07f;
					}
					return 520f;
				case 0:
					break;
				case 1:
					if (mode != MAPMODE.NORMAL && !Map2d.isTempMode(mode))
					{
						return 0.05f;
					}
					return 400f;
				case 2:
					if (mode != MAPMODE.NORMAL && !Map2d.isTempMode(mode))
					{
						return 0.04f;
					}
					return 340f;
				case 3:
					if (mode != MAPMODE.NORMAL && !Map2d.isTempMode(mode))
					{
						return 0.039f;
					}
					return 320f;
				case 4:
					if (mode != MAPMODE.NORMAL && !Map2d.isTempMode(mode))
					{
						return -50f;
					}
					if (!this.draw_TT_over_mv)
					{
						return 310f;
					}
					return 130f;
				default:
					switch (layer)
					{
					case 12:
						if (mode == MAPMODE.SUBMAP)
						{
							return -2f;
						}
						return 402.5f;
					case 13:
						if (mode == MAPMODE.SUBMAP)
						{
							return -1f;
						}
						return 398.5f;
					case 19:
						return ((mode == MAPMODE.NORMAL || Map2d.isTempMode(mode)) ? 520f : 0.07f) - 1.5f;
					case 20:
						return ((mode == MAPMODE.NORMAL || Map2d.isTempMode(mode)) ? 500f : 0.06f) - 1.5f;
					case 21:
						return ((mode == MAPMODE.NORMAL || Map2d.isTempMode(mode)) ? 400f : 0.05f) - 1.5f;
					case 22:
						return ((mode == MAPMODE.NORMAL || Map2d.isTempMode(mode)) ? (this.draw_TT_over_mv ? 118f : 340f) : 0.04f) - 1.5f;
					}
					break;
				}
				if (mode != MAPMODE.NORMAL && !Map2d.isTempMode(mode))
				{
					return 0.06f;
				}
				return 500f;
			}
			else
			{
				if (mode != MAPMODE.NORMAL)
				{
					return this.getDrawZ(mode, 1);
				}
				if (layer != -11)
				{
					return 370f;
				}
				return 370f;
			}
		}

		public virtual float getIconLightAlpha(Map2d Mp, M2SubMap Sm)
		{
			if (Sm == null)
			{
				return 1f;
			}
			float camera_length = Sm.camera_length;
			if (camera_length < 1f)
			{
				return X.NIL(0.35f, 0.84f, camera_length, 1f);
			}
			return 1f;
		}

		public virtual BLEND getIconLightBlend(Map2d Mp, M2SubMap Sm)
		{
			if (Sm != null)
			{
				return BLEND.ADD;
			}
			return BLEND.NORMAL;
		}

		public virtual bool drawUCol(MeshDrawer Md, Map2d Mp, bool fine_cam_transparent_col = true)
		{
			Md.Col = Mp.bgcol;
			Md.Rect(0f, 0f, (float)Mp.clms * Mp.CLEN, (float)Mp.rows * Mp.CLEN, false);
			return true;
		}

		public virtual void fineCameraBgColor()
		{
			this.M2D.setCameraTransparentColor(this.M2D.curMap.bgcol);
		}

		public virtual int reentryGradation(Map2d Mp, MeshDrawer MdU, MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT)
		{
			return 0;
		}

		public virtual int drawCheck(float fcnt)
		{
			return 0;
		}

		public virtual void closeCamera()
		{
		}

		public void defineFinalCamera(Camera DC)
		{
			DC.gameObject.layer = this.layer_final;
		}

		public virtual int getLayerForBgColor()
		{
			return LayerMask.NameToLayer("ChipsUCol");
		}

		public virtual int getLayerForChip(int layer, Dungeon.MESHTYPE meshtype = Dungeon.MESHTYPE.CHIP)
		{
			if (meshtype == Dungeon.MESHTYPE.GRADATION)
			{
				this.is_house_light_gradation_layer(layer);
				if (layer > 1)
				{
					return this.layer_final;
				}
				return this.LAY("ChipsSubBottom");
			}
			else if (meshtype == Dungeon.MESHTYPE.EFFECT)
			{
				if (layer != 1)
				{
					return this.effect_layer_bottom;
				}
				return this.effect_layer_top;
			}
			else
			{
				if (layer != 3)
				{
					return LayerMask.NameToLayer((this.SubMpData == null && layer == 1 && meshtype == Dungeon.MESHTYPE.CHIP) ? "ChipsForBorder" : "Chips");
				}
				if (!this.is_editor)
				{
					return this.LAY("ChipsRendered");
				}
				return this.LAY("Chips");
			}
		}

		public virtual MeshDrawer checkBlurDrawing(M2Puts Cp, M2SubMap SubMap, ref PxlMeshDrawer Src)
		{
			return null;
		}

		public bool useBgColLayer(Map2d Mp, out Material MtrU)
		{
			if (Mp.mode == MAPMODE.NORMAL || Map2d.isTempMode(Mp.mode))
			{
				MtrU = this.getBgColorMaterial();
				return MtrU != null;
			}
			MtrU = null;
			return false;
		}

		public virtual Material getBgColorMaterial()
		{
			return this.MtrBgColor;
		}

		public virtual RenderTexture ArrangeSimplifyMaterial(MeshDrawer Md, Material Mtr, RenderTexture Rd, M2SubMap Sm, int update_flag)
		{
			return null;
		}

		public virtual RenderTexture getBorderTexture()
		{
			return null;
		}

		public Material getChipMaterialActive(Map2d _Mp, Material Mtr = null, int layer = 1)
		{
			this.Mp = _Mp;
			this.SubMpData = this.Mp.SubMapData;
			return this.getChipMaterial(layer, Mtr);
		}

		public Color32 getSpecificColor(Color32 depCol)
		{
			depCol.r /= 2;
			depCol.g /= 2;
			depCol.b /= 2;
			return depCol;
		}

		public bool isCreateFloatMesh(MeshDrawer Md)
		{
			if (Md == this.Mp.MyDrawerTGrd)
			{
				return !this.is_house_light_gradation_layer(2);
			}
			return this.SubMpData != null && this.SubMpData.order == SMORDER.TOP;
		}

		public virtual bool setEffectVar(float v0)
		{
			return false;
		}

		public virtual Material getWaterMaterial()
		{
			if (!(this.MtrWater_ != null))
			{
				return this.MtrChip;
			}
			return this.MtrWater_;
		}

		public virtual Material getWaterSurfaceReflectionMaterial()
		{
			Material mtrWaterSurfaceReflection = this.DGN.MtrWaterSurfaceReflection;
			MTRX.setMaterialST(mtrWaterSurfaceReflection, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			return mtrWaterSurfaceReflection;
		}

		public virtual int getLayerForWater()
		{
			return LayerMask.NameToLayer("Water");
		}

		public Material getMaterialForSimplifySubMap(MeshDrawer Md)
		{
			Md.Col = MTRX.ColGray;
			Material mtrChipForSimplify = this.MtrChipForSimplify;
			mtrChipForSimplify.mainTexture = this.M2D.MIchip.Tx;
			return mtrChipForSimplify;
		}

		public virtual bool canBakeSimplify(bool is_grd, int layer)
		{
			return false;
		}

		public virtual void getTextureForUiBg(ref RenderTexture TxB, ref RenderTexture TxG)
		{
		}

		public M2DBase M2D
		{
			get
			{
				return this.DGN.M2D;
			}
		}

		public bool is_editor
		{
			get
			{
				return this.key == "_editor";
			}
		}

		public Camera EffectCamera
		{
			get
			{
				if (!(this.EffectCamera_ == null))
				{
					return this.EffectCamera_;
				}
				return this.DCBase;
			}
		}

		public virtual Camera BackGroundCamera
		{
			get
			{
				return null;
			}
		}

		public virtual Color32 getSimplifyTransparentColor(M2SubMap SubMpData, Map2d Mp)
		{
			return this.M2D.Cam.transparent_color;
		}

		public RenderTexture getBackGroundRendered()
		{
			Camera backGroundCamera = this.BackGroundCamera;
			if (!(backGroundCamera != null))
			{
				return null;
			}
			return backGroundCamera.targetTexture;
		}

		public virtual Material getWithLightTextureMaterial(MImage MI, Shader Shd = null, int stencil = -1)
		{
			return MI.getMtr((Shd == null) ? MTRX.blend2ShaderImg(BLEND.NORMALP3) : Shd, stencil);
		}

		public void setBorderMask(ref Material Mtr, MImage MI, bool flag = true, string tex_name = null)
		{
			RenderTexture borderTexture = this.getBorderTexture();
			if (borderTexture != null && flag)
			{
				Mtr = this.getWithLightTextureMaterial(MI, Mtr.shader, -100);
				Mtr.EnableKeyword("USEMASK");
				Mtr.SetTexture(tex_name ?? "_MaskTex", borderTexture);
				Mtr.SetFloat("_ScreenMargin", 8f);
				return;
			}
			Mtr = this.getWithLightTextureMaterial(MI, Mtr.shader, -1);
		}

		public int LAYB(string lay_name)
		{
			return 1 << this.LAY(lay_name);
		}

		public int LAY(string lay_name)
		{
			return LayerMask.NameToLayer(lay_name);
		}

		public bool getCueAndSheet(ref string cue, ref string sheet)
		{
			if (this.bgm_cue_key == null)
			{
				return false;
			}
			cue = this.bgm_cue_key;
			sheet = this.bgm_sheet_timing;
			return true;
		}

		public bool is_house_light_gradation_layer(int layer)
		{
			return this.SubMpData != null && this.SubMpData.order == SMORDER.SKY && TX.isStart(this.Mp.key, "_light_", 0) && layer == 2;
		}

		public void drawWaterSurfaceReflection(MeshDrawer Md, float height, Color32 C, float alpha)
		{
			float num = IN.w + 16f;
			float num2 = IN.h + 16f;
			float num3 = height / num2;
			int vertexMax = Md.getVertexMax();
			float num4 = -num2 * 0.5f * 0.015625f;
			float num5 = height * 0.015625f;
			Md.allocUv2(64, false);
			Md.allocVer(vertexMax + 8, 0);
			Md.allocTri(Md.getTriMax() + 18, 0);
			Md.uvRect(-num * 0.5f * 0.015625f, num4, num * 0.015625f, num5, 0f, X.Mn(1f, num3 * 2f), 1f, -num3, true, false);
			num += 80f;
			Md.Col = Md.ColGrd.Set(C).setA1(alpha).C;
			float num6 = height * 0.675f;
			Md.ColGrd.Set(this.WaterSurfaceBottomColor).addG(C, false).mulA(alpha);
			Md.RectBLGradation(-num * 0.5f, -num2 * 0.5f - 40f, num, num6 + 40f, GRD.TOP2BOTTOM, false);
			Md.ColGrd.Set(this.WaterSurfaceTopColor).addG(C, false).mulA(alpha);
			float num7 = X.Mn(height * 0.25f, 6f);
			Md.Tri(-3, 0, 3, false).Tri(-3, 3, -2, false);
			Md.RectBLGradation(-num * 0.5f, -num2 * 0.5f + height - num7, num, num7, GRD.BOTTOM2TOP, false);
			Md.getUvArray();
			Vector3[] vertexArray = Md.getVertexArray();
			int vertexMax2 = Md.getVertexMax();
			float num8 = num3 * 1.5f;
			for (int i = vertexMax; i < vertexMax2; i++)
			{
				Vector3 vector = vertexArray[i];
				float num9 = X.Mx(0f, 1f - (vector.y - num4) / num5);
				Md.Uv2(num9, num8, false);
			}
		}

		public bool isLowerBgm(Map2d Mp)
		{
			bool flag = this.is_lower_bgm_;
			if (Mp != null)
			{
				int i = Mp.prepareMeta().GetI("lower_bgm", -1, 0);
				if (i >= 0)
				{
					flag = i > 0;
				}
			}
			return flag;
		}

		public bool isHalfBgm(Map2d Mp)
		{
			bool flag = this.is_half_bgm_;
			if (Mp != null)
			{
				int i = Mp.prepareMeta().GetI("half_bgm", -1, 0);
				if (i >= 0)
				{
					flag = i > 0;
				}
			}
			return flag;
		}

		public readonly string key;

		protected string color_family_key;

		public readonly DungeonContainer DGN;

		protected Material MtrChip_;

		protected Material MtrChipBg_;

		protected Material MtrChipTop_;

		protected Material MtrChipForSimplify_;

		protected Material MtrChipTopLight_;

		protected Material MtrSubMapTop_;

		protected Material MtrBgColor_;

		protected Material MtrImageWithLight_;

		protected Material MtrWater_;

		protected Map2d Mp;

		protected M2SubMap SubMpData;

		protected Camera DCBase;

		public const int RQUEUE_AFTER = 3000;

		public BLEND blend_grd_U = BLEND.MUL;

		public BLEND blend_grd_G = BLEND.MUL;

		public BLEND blend_grd_T = BLEND.MUL;

		public BLEND blend_grd_B = BLEND.MUL;

		public bool draw_efc_b_to_01;

		public bool draw_TT_over_mv;

		public float camera_len_min_threshold;

		public bool no_rain_fall;

		public bool use_window_remover;

		public float multiply_mv_light_alpha = 1f;

		protected Color32 TopDarkColor;

		protected Color32 SMCpFrontDarkColor;

		protected Color32 SMCpBackDarkColor;

		public Color32 BrightColor;

		protected Color32 BrightColorBottom;

		protected Color32 SMCpBackBaseColor = C32.d2c(584440529U);

		protected Color32 SMCpTopBaseColor = C32.d2c(584440529U);

		protected Color32 BehindChipSubColor = C32.d2c(0U);

		protected Color32 WaterSurfaceTopColor = C32.d2c(2868903935U);

		protected Color32 WaterSurfaceBottomColor = C32.d2c(uint.MaxValue);

		public Color32 ColWater;

		public Color32 ColWaterBottom;

		public Color32 ColBlurTransparent = C32.d2c(16777215U);

		public float night_level;

		public float rain_level;

		public float toplight_level;

		public float submap_fillup_level = 0.7f;

		public string bgm_sheet_timing;

		public string bgm_cue_key;

		public int effect_layer_bottom;

		public int effect_layer_top;

		public int effect_layer_t_bottom;

		public int layer_final = LayerMask.NameToLayer("ForFinalRender");

		public bool use_finalsource_depth;

		public string foot_type = "";

		protected Camera EffectCamera_;

		private bool cam_finalized;

		protected static C32 C;

		protected static MdArranger Ma;

		public bool use_rain;

		protected bool is_half_bgm_;

		protected bool is_lower_bgm_;

		private List<Material> ACreatedMtr;

		public enum MESHTYPE
		{
			CHIP,
			GRADATION,
			EFFECT
		}
	}
}
