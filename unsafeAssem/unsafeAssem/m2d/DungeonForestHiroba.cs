using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonForestHiroba : DungeonBright
	{
		public DungeonForestHiroba(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "ground";
			this.color_family_key = _key;
			this.ShdHirobaScape = M2DBase.getShd("M2d/SubMapHirobaScape");
		}

		public override void DisposeMaterial()
		{
			base.DisposeMaterial();
			this.MtrScapeRend_ = null;
		}

		public Material MtrScapeRend
		{
			get
			{
				if (this.MtrScapeRend_ == null)
				{
					this.MtrScapeRend_ = base.newMtr("M2d/ImageWithLightAndBrightForScape");
				}
				return this.MtrScapeRend_;
			}
		}

		public override void resetColor()
		{
			base.resetColor();
			DgnColorContainer colCon = this.DGN.ColCon;
			this.TopDarkColor = colCon.blend3r("ScapeTopDark", "ScapeTopDarkRain", this.night_level, this.rain_level, uint.MaxValue);
			this.SMCpBackDarkColor = colCon.blend3r("ScapeBackDark", "ScapeBackDarkRain", this.night_level, this.rain_level, uint.MaxValue);
			this.BrightColor = colCon.blend3r("ScapeBright", "ScapeBrightRain", this.night_level, this.rain_level, uint.MaxValue);
			this.LightMulDarkColor = colCon.blend3r("ScapeLightMulDark", "ScapeLightMulDarkRain", this.night_level, this.rain_level, uint.MaxValue);
			this.ColScapeBackBottom = colCon.blend3r("ScapeBackBottom", "ScapeBackBottomRain", this.night_level, this.rain_level, uint.MaxValue);
			float effect_variable = base.M2D.effect_variable0;
			this.MtrScapeRend.SetFloat("_EffectLevel", effect_variable);
			this.MtrScapeRend.SetColor("_FillColor", this.BrightColor);
			this.MtrScapeRend.SetColor("_BlurColor", colCon.blend3r("ScapeFill", "ScapeFillRain", this.night_level, this.rain_level, uint.MaxValue));
		}

		public override void fineBackRenderMaterial(Material Mtr)
		{
			base.fineBackRenderMaterial(Mtr);
			if (Mtr != null)
			{
				float effect_variable = base.M2D.effect_variable0;
				Mtr.SetFloat("_EffectLevel", effect_variable);
				Mtr.SetColor("_EffectColor", this.LightMulDarkColor);
			}
		}

		public override void fineMaterialColor()
		{
			base.fineMaterialColor();
			DgnColorContainer colCon = this.DGN.ColCon;
		}

		public override Dungeon initCamera(M2Camera Cam)
		{
			base.initCamera(Cam);
			this.DCScape = null;
			float effect_variable = base.M2D.effect_variable0;
			this.MtrBackRendered.SetFloat("_EffectLevel", effect_variable);
			this.MtrScapeRend.SetColor("_BlurColor", this.BrightColor);
			this.MtrScapeRend.SetTexture("_BlurTex", this.MainBlured);
			return this;
		}

		protected override void createDCBack(M2Camera Cam)
		{
			base.createDCBack(Cam);
		}

		public override bool drawUCol(MeshDrawer Md, Map2d Mp, bool fine_cam_transparent_col = true)
		{
			if (this.DCScape == null)
			{
				base.drawUCol(Md, Mp, false);
			}
			else
			{
				Md.clear(false, false);
			}
			Md.Col = this.BrightColor;
			Md.ColGrd.Set(this.ColScapeBackBottom);
			float num = IN.wh * 1.15f;
			float num2 = IN.hh * 1.15f;
			Md.RectBLGradation(-num, -num2, num * 2f, -IN.hh, GRD.TOP2BOTTOM, false);
			if (!this.ucol_setted)
			{
				this.ucol_setted = true;
				Mp.get_MMRD().transformCamTrace(Md);
			}
			if (fine_cam_transparent_col)
			{
				base.M2D.setCameraTransparentColor(Md.ColGrd.blend(Md.Col, 0.5f).C);
			}
			return true;
		}

		public override int getLayerForChip(int layer, Dungeon.MESHTYPE meshtype = Dungeon.MESHTYPE.CHIP)
		{
			if (this.SubMpData != null && this.SubMpData.order == SMORDER.SKY && this.SubMpData.camera_length < 0.2f)
			{
				return base.LAY("ChipsUCol");
			}
			if (meshtype != Dungeon.MESHTYPE.EFFECT)
			{
				return base.getLayerForChip(layer, meshtype);
			}
			if (layer != 1)
			{
				return this.effect_layer_bottom;
			}
			return this.effect_layer_top;
		}

		public override Material getChipMaterial(int layer, Material Mtr = null)
		{
			if (this.SubMpData != null && this.SubMpData.order == SMORDER.SKY && this.SubMpData.camera_length < 0.2f)
			{
				if (Mtr == null)
				{
					Mtr = this.SubMpData.getChipMtr(this.ShdHirobaScape);
				}
				DgnColorContainer colCon = this.DGN.ColCon;
				float effect_variable = base.M2D.effect_variable0;
				Mtr.SetFloat("_EffectLevel", effect_variable);
				Mtr.SetColor("_FillColor", colCon.blend("ScapeFill", effect_variable, uint.MaxValue));
				Mtr.SetColor("_MulColor", colCon.blend3("ScapeBackFill", this.night_level, uint.MaxValue));
				Mtr.SetColor("_EffectColor", colCon.blend3("ScapeFill", this.night_level, uint.MaxValue));
				return Mtr;
			}
			return base.getChipMaterial(layer, Mtr);
		}

		private Color32 ColScapeBackBottom;

		private Material MtrScapeRend_;

		private Camera DCScape;

		private Shader ShdHirobaScape;
	}
}
