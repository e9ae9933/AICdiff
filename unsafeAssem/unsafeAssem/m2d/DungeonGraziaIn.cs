using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class DungeonGraziaIn : DungeonHouse
	{
		public DungeonGraziaIn(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "woodbridge";
			this.color_family_key = "city_in";
			this.use_window_remover = true;
			this.Achip_top_light_level[0] = 1f;
			this.Achip_top_light_level[1] = 0.7f;
			this.Achip_top_light_level[2] = 0.4f;
			this.Aborder_light_level[0] = 0.6f;
			this.Aborder_light_level[1] = 0.6f;
			this.Aborder_light_level[2] = 0.4f;
		}

		public override void resetColor()
		{
			base.resetColor();
			DgnColorContainer colCon = this.DGN.ColCon;
			this.SMinMinColor = colCon.blend3("SMinMinColor", this.night_level, uint.MaxValue);
			this.SMinCpColor = colCon.blend3("SMinCpColor", this.night_level, uint.MaxValue);
			this.SMinMulColor = colCon.blend3("SMinMul", this.night_level, uint.MaxValue);
			this.SMinMulBottomColor = colCon.blend3("SMinMulBottom", this.night_level, uint.MaxValue);
			this.SMinCpBright = this.DGN.ColCon.blend3("SMinCpBright", this.night_level, uint.MaxValue);
		}

		private void setBCW(Material Mtr, int index = -1)
		{
			Mtr.EnableKeyword("_USECOLORFIX_ON");
			if (index == -1)
			{
				index = ((this.SubMpData == null || this.SubMpData.order == SMORDER.GROUND) ? 0 : ((this.SubMpData.order == SMORDER.TOP) ? 2 : 1));
			}
			Mtr.SetColor("_BColor", this.DGN.ColCon.GC("ChipFillB", index, uint.MaxValue));
			Mtr.SetColor("_CColor", this.DGN.ColCon.GC("ChipFillC", index, uint.MaxValue));
			Mtr.SetColor("_WColor", this.DGN.ColCon.GC("ChipFillW", index, uint.MaxValue));
		}

		public override Material createMaterilForBackRendered(MImage MI, int stencil_ref = -1)
		{
			Material material = base.createMaterilForBackRendered(MI, stencil_ref);
			material.EnableKeyword("ADD_BLURTEX");
			return material;
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
			bool flag = false;
			if (this.SubMpData != null && this.SubMpData.order == SMORDER.TOP)
			{
				if (Mtr == null)
				{
					Mtr = this.SubMpData.getChipMtr(this.DGN.MtrSubMapTopNormal);
				}
				Mtr.SetColor("_Color", this.SMCpBackDarkColor);
				Mtr.SetFloat("_Level", X.MMX(0f, (this.SubMpData.camera_length - 1f) * 1.4f, 1f));
			}
			else
			{
				Mtr = base.getChipMaterial(layer, Mtr);
				if (this.isHouseInSM(this.SubMpData))
				{
					Mtr.SetFloat("_Level", (float)this.SMinCpBright.a / 255f);
					Mtr.SetColor("_Color", this.SMinCpColor);
					Mtr.SetColor("_MinColor", this.SMinMinColor);
					Mtr.SetColor("_MulColor", this.SMinMulColor);
					Mtr.SetColor("_MulBottomColor", this.SMinMulBottomColor);
					Mtr.SetColor("_BrightColor", this.SMinCpBright);
					flag = true;
				}
			}
			if (this.SubMpData != null && !flag)
			{
				if (Mtr != base.MtrChip && Mtr != base.MtrChipBg && (Mtr.shader.name == base.MtrChip.shader.name || Mtr.shader.name == base.MtrChipBg.shader.name || Mtr.shader.name == this.DGN.MtrSubMapBlending.shader.name))
				{
					this.setBCW(Mtr, -1);
				}
				else
				{
					Mtr.DisableKeyword("_USECOLORFIX_ON");
				}
			}
			else
			{
				Mtr.DisableKeyword("_USECOLORFIX_ON");
			}
			return Mtr;
		}

		public bool isHouseInSM(M2SubMap SubMapData)
		{
			return this.SubMpData != null && this.SubMpData.getTargetMap().key.IndexOf("_city_in_") >= 0;
		}

		public override Color32 getSimplifyTransparentColor(M2SubMap SubMpData, Map2d Mp)
		{
			if (SubMpData == null || this.isHouseInSM(SubMpData))
			{
				return C32.d2c(5056807U);
			}
			return base.getSimplifyTransparentColor(SubMpData, Mp);
		}

		private Color32 SMinMulColor;

		private Color32 SMinMulBottomColor;

		private Color32 SMinCpColor;

		private Color32 SMinCpBright;

		private Color32 SMinMinColor;
	}
}
