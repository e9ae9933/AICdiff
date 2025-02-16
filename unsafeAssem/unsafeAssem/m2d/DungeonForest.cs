using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonForest : DungeonBright
	{
		protected override Shader ShaderSubMapTop
		{
			get
			{
				return MTRX.getShd("Hachan/ShaderGDTNormalBlur");
			}
		}

		public DungeonForest(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "ground";
		}

		public override Material getChipMaterial(int layer, Material Mtr = null)
		{
			if (layer == 3)
			{
				return base.getChipMaterial(layer, Mtr);
			}
			if (this.SubMpData != null && this.SubMpData.order == SMORDER.TOP)
			{
				if (Mtr == null)
				{
					Mtr = this.SubMpData.getChipMtr(this.DGN.MtrSubMapTopMul);
				}
				Mtr.SetColor("_Color", this.SMCpBackDarkColor);
				Mtr.SetFloat("_Level", X.MMX(0f, (this.SubMpData.camera_length - 1f) * 1.4f, 1f));
				return Mtr;
			}
			return base.getChipMaterial(layer, Mtr);
		}
	}
}
