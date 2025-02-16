using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonHouse : DungeonBright
	{
		public DungeonHouse(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "leaf";
			this.ColBlurTransparent = C32.d2c(5590347U);
			this.draw_TT_over_mv = true;
		}

		public override Material getChipMaterial(int layer, Material Mtr = null)
		{
			if (layer == 3)
			{
				if (this.SubMpData == null)
				{
					if (Mtr == null)
					{
						Mtr = this.DGN.getMtr(M2DBase.getShd("M2d/ChipsTopLightAdd"), -1);
					}
					return Mtr;
				}
				return base.getChipMaterial(layer, Mtr);
			}
			else
			{
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
				return base.getChipMaterial(layer, Mtr);
			}
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
