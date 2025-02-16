using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonGlacier : DungeonBright
	{
		public DungeonGlacier(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "glass";
			this.bgm_cue_key = "degree45";
			this.bgm_sheet_timing = "BGM_degree45";
			this.no_rain_fall = true;
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
			Material chipMaterial = base.getChipMaterial(layer, Mtr);
			if (chipMaterial != base.MtrChip && chipMaterial != base.MtrChipBg && (chipMaterial.shader.name == base.MtrChip.shader.name || chipMaterial.shader.name == base.MtrChipBg.shader.name || chipMaterial.shader.name == this.DGN.MtrSubMapBlending.shader.name))
			{
				this.setBCW(chipMaterial, -1);
			}
			else
			{
				chipMaterial.DisableKeyword("_USECOLORFIX_ON");
			}
			return chipMaterial;
		}

		public override int getLayerForChip(int layer, Dungeon.MESHTYPE meshtype = Dungeon.MESHTYPE.CHIP)
		{
			if (this.SubMpData != null || meshtype != Dungeon.MESHTYPE.GRADATION)
			{
				return base.getLayerForChip(layer, meshtype);
			}
			if (layer > 2)
			{
				return this.effect_layer_bottom;
			}
			return base.LAY("ChipsSubBottom");
		}

		public override RenderTexture ArrangeSimplifyMaterial(MeshDrawer Md, Material Mtr, RenderTexture Rd, M2SubMap Sm, int update_flag)
		{
			if (Sm == null)
			{
				return null;
			}
			RenderTexture renderTexture = new RenderTexture(Rd.width, Rd.height, Rd.depth, Rd.format);
			int num = X.IntC(X.NI(0.5f, 5f, X.Abs(1f - Sm.camera_length))) * ((Sm.order == SMORDER.TOP) ? 2 : 1);
			BLIT.Blur(renderTexture, Rd, num, num, (float)renderTexture.width * 0.5f, (float)renderTexture.height * 0.5f, 1f, 1f, 0f, 0f, 1f, 1f, 16777215U);
			RenderTexture.active = null;
			return renderTexture;
		}

		public override MeshDrawer checkBlurDrawing(M2Puts Cp, M2SubMap SubMap, ref PxlMeshDrawer Src)
		{
			return null;
		}
	}
}
