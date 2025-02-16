using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonSacred : DungeonGrazia
	{
		public DungeonSacred(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "sand";
			this.ColBlurTransparent = C32.d2c(3559480U);
			this.draw_TT_over_mv = true;
			this.multiply_mv_light_alpha = 1f;
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
			if (this.SubMpData == null && layer == 4 && TX.isStart(this.Mp.key, "sacred_", 0) && Mtr == null)
			{
				Mtr = this.DGN.getMtr(M2DBase.getShd("M2d/BasicMapAdd"), -1);
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

		public override bool isBlurSubmap(M2SubMap SubMap)
		{
			return SubMap != null && SubMap.order == SMORDER.SKY && TX.isStart(SubMap.getTargetMap().key, "_add_", 0);
		}

		public override Material getGradationMaterial(int layer, Material Mtr = null)
		{
			if (layer != 2)
			{
				return base.getGradationMaterial(layer, Mtr);
			}
			if (!(Mtr == null))
			{
				return Mtr;
			}
			return this.DGN.getMtr(M2DBase.getShd("M2d/BasicGradationAdd"), -1);
		}
	}
}
