using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonForestInTree : DungeonForest
	{
		public DungeonForestInTree(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "ground";
			this.draw_TT_over_mv = true;
		}

		public override Material getChipMaterial(int layer, Material Mtr = null)
		{
			if (this.SubMpData != null && this.SubMpData.order == SMORDER.SKY && layer == 4)
			{
				if (Mtr == null)
				{
					Mtr = this.SubMpData.getChipMtr(M2DBase.getShd("M2d/BasicMapAdd"));
				}
				return Mtr;
			}
			return base.getChipMaterial(layer, Mtr);
		}

		public override int getLayerForChip(int layer, Dungeon.MESHTYPE meshtype = Dungeon.MESHTYPE.CHIP)
		{
			if (meshtype == Dungeon.MESHTYPE.CHIP && this.SubMpData != null && this.SubMpData.order == SMORDER.SKY && layer == 4)
			{
				return this.layer_final;
			}
			return base.getLayerForChip(layer, meshtype);
		}

		public override RenderTexture ArrangeSimplifyMaterial(MeshDrawer Md, Material Mtr, RenderTexture Rd, M2SubMap Sm, int update_flag)
		{
			if (Sm == null)
			{
				return null;
			}
			if (Sm.order == SMORDER.SKY && update_flag == 8192)
			{
				RenderTexture renderTexture = new RenderTexture(Rd.width, Rd.height, Rd.depth, Rd.format);
				int num = 15;
				BLIT.Blur(renderTexture, Rd, num, num, (float)renderTexture.width * 0.5f, (float)renderTexture.height * 0.5f, 1f, 1f, 0f, 0f, 1f, 1f, 16777215U);
				RenderTexture.active = null;
				return renderTexture;
			}
			return null;
		}

		public override bool canBakeSimplify(bool is_grd, int layer)
		{
			return (this.SubMpData != null && this.SubMpData.order == SMORDER.SKY && layer == 4 && !is_grd) || base.canBakeSimplify(is_grd, layer);
		}
	}
}
