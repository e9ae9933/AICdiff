using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonSea : DungeonBright
	{
		protected override Shader ShaderSubMapTop
		{
			get
			{
				return M2DBase.getShd("M2d/SubMapTopNormal");
			}
		}

		public DungeonSea(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "sand";
			this.submap_fillup_level = 0.125f;
			this.bgm_cue_key = "underwater";
			this.bgm_sheet_timing = "BGM_underwater";
		}

		private void setBCW(Material _Mtr, int index = -1)
		{
		}

		public override void resetColor()
		{
			base.resetColor();
			this.setBCW(base.MtrChip, 0);
			this.setBCW(base.MtrChipBg, 0);
		}

		public override Material getChipMaterial(int layer, Material Mtr = null)
		{
			if (this.SubMpData != null && this.SubMpData.order == SMORDER.TOP)
			{
				if (Mtr == null)
				{
					Mtr = this.SubMpData.getChipMtr(this.DGN.MtrSubMapTopNormal);
				}
				Mtr.SetColor("_Color", this.SMCpFrontDarkColor);
				Mtr.SetFloat("_Level", (float)this.SMCpFrontDarkColor.a / 255f);
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
	}
}
