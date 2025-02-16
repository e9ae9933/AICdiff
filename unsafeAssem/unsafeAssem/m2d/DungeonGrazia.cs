using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonGrazia : DungeonBright
	{
		public DungeonGrazia(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "woodbridge";
			this.ColBlurTransparent = C32.d2c(4229201U);
			this.draw_TT_over_mv = true;
			this.multiply_mv_light_alpha = 0.65f;
		}

		public override Material createMaterilForBackRendered(MImage MI, int stencil_ref = -1)
		{
			Material material = base.createMaterilForBackRendered(MI, stencil_ref);
			material.EnableKeyword("ADD_BLURTEX");
			return material;
		}

		protected override float blur_texture_scale
		{
			get
			{
				return 0.1f;
			}
		}

		public override MeshDrawer checkBlurDrawing(M2Puts Cp, M2SubMap SubMap, ref PxlMeshDrawer Src)
		{
			return null;
		}

		protected override RenderTexture createBlurBufferTexture(RenderTexture TexEffectBlur)
		{
			RenderTexture renderTexture = base.createBlurBufferTexture(TexEffectBlur);
			renderTexture.filterMode = (TexEffectBlur.filterMode = FilterMode.Trilinear);
			return renderTexture;
		}

		private void setBCW(Material _Mtr, int index = -1)
		{
			_Mtr.EnableKeyword("_USECOLORFIX_ON");
			if (index == -1)
			{
				index = ((this.SubMpData == null || this.SubMpData.order == SMORDER.GROUND) ? 0 : ((this.SubMpData.order == SMORDER.TOP) ? 2 : 1));
			}
			_Mtr.SetColor("_BColor", this.DGN.ColCon.GC("ChipFillB", index, uint.MaxValue));
			_Mtr.SetColor("_CColor", this.DGN.ColCon.GC("ChipFillC", index, uint.MaxValue));
			_Mtr.SetColor("_WColor", this.DGN.ColCon.GC("ChipFillW", index, uint.MaxValue));
		}

		public override void resetColor()
		{
			base.resetColor();
			this.setBCW(base.MtrChip, 0);
			this.setBCW(base.MtrChipBg, 0);
		}

		public override Material getChipMaterial(int layer, Material Mtr = null)
		{
			if (layer == 3)
			{
				if (Mtr == null)
				{
					Mtr = this.DGN.getMtr(M2DBase.getShd("M2d/ChipsTopLightAdd"), -1);
				}
				return Mtr;
			}
			if (this.SubMpData != null && this.SubMpData.order == SMORDER.TOP)
			{
				if (Mtr == null)
				{
					Mtr = this.SubMpData.getChipMtr(this.DGN.MtrSubMapTopNormal);
				}
				Mtr.SetColor("_Color", this.SMCpBackDarkColor);
				Mtr.SetFloat("_Level", X.MMX(0f, (this.SubMpData.camera_length - 1f) * 1.4f, 1f));
				return Mtr;
			}
			Material chipMaterial = base.getChipMaterial(layer, Mtr);
			if (chipMaterial != base.MtrChip && chipMaterial != base.MtrChipBg && (chipMaterial.shader.name == base.MtrChip.shader.name || chipMaterial.shader.name == base.MtrChipBg.shader.name || chipMaterial.shader.name == this.DGN.MtrSubMapBlending.shader.name))
			{
				this.setBCW(chipMaterial, -1);
			}
			return chipMaterial;
		}

		public override int getLayerForChip(int layer, Dungeon.MESHTYPE meshtype = Dungeon.MESHTYPE.CHIP)
		{
			if (meshtype == Dungeon.MESHTYPE.CHIP && layer == 0 && this.SubMpData != null && this.SubMpData.order == SMORDER.SKY && X.Mn(this.SubMpData.scrolly, this.SubMpData.scrollx) < 0.35f)
			{
				return base.LAY("ChipsUCol");
			}
			return base.getLayerForChip(layer, meshtype);
		}
	}
}
