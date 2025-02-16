using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class DungeonContainer
	{
		private void clearMaterialLink()
		{
			this.MtrSubMapBlending_ = null;
			this.MtrSubMapTopMul_ = null;
			this.MtrSubMapTopNormal_ = null;
			this.MtrBorderLight_ = null;
			this.MtrChipWhole_ = null;
			this.MtrChipWithBright_ = null;
			this.MtrWater_ = null;
			this.MtrWaterInBright_ = null;
			this.MtrWaterSurfaceReflection_ = null;
			this.MtrHouseLight_ = null;
			this.MtrWaterFall_ = null;
			this.MtrWaterFallT_ = null;
			this.MtrFakeLayerDissolve_ = null;
		}

		public DungeonContainer(M2DBase _M2D)
		{
			this.M2D = _M2D;
			this.ColCon = new DgnColorContainer();
			this.ACreatedMtr = new List<Material>(4);
			this.MIchip = new MImage(null);
		}

		public void init()
		{
			this.ODgn = new BDic<string, Dungeon>();
		}

		private Material newMtr(Shader Shd)
		{
			Material material = MTRX.newMtr(Shd);
			this.ACreatedMtr.Add(material);
			return material;
		}

		private Material newMtr(Material Src)
		{
			Material material = MTRX.newMtr(Src);
			this.ACreatedMtr.Add(material);
			return material;
		}

		private void DisposeMaterial()
		{
			for (int i = this.ACreatedMtr.Count - 1; i >= 0; i--)
			{
				IN.DestroyOne(this.ACreatedMtr[i]);
			}
			this.ACreatedMtr.Clear();
		}

		public void destruct()
		{
			this.recreateMaterial();
			foreach (KeyValuePair<string, Dungeon> keyValuePair in this.ODgn)
			{
				keyValuePair.Value.destruct();
			}
		}

		private Dungeon createDungeonInstace(string key)
		{
			Dungeon dungeon = null;
			if (key != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
				if (num <= 2336649189U)
				{
					if (num <= 424117420U)
					{
						if (num != 217793883U)
						{
							if (num != 230981954U)
							{
								if (num == 424117420U)
								{
									if (key == "city_in")
									{
										dungeon = new DungeonGraziaIn("city_in", this);
									}
								}
							}
							else if (key == "city")
							{
								dungeon = new DungeonGrazia("city", this);
							}
						}
						else if (key == "house")
						{
							dungeon = new DungeonHouse("house", this);
						}
					}
					else if (num <= 1328097888U)
					{
						if (num != 497756744U)
						{
							if (num == 1328097888U)
							{
								if (key == "forest")
								{
									dungeon = new DungeonForest("forest", this);
								}
							}
						}
						else if (key == "house_in_atelier")
						{
							dungeon = new DungeonHouseAtelier("house_in_atelier", this);
						}
					}
					else if (num != 1341997391U)
					{
						if (num == 2336649189U)
						{
							if (key == "sacred")
							{
								dungeon = new DungeonSacred("sacred", this);
							}
						}
					}
					else if (key == "_editor")
					{
						dungeon = new Dungeon("_editor", this)
						{
							camera_len_min_threshold = 0.4f
						};
					}
				}
				else if (num <= 3106337420U)
				{
					if (num != 2459078144U)
					{
						if (num != 2807445042U)
						{
							if (num == 3106337420U)
							{
								if (key == "sea")
								{
									dungeon = new DungeonSea("sea", this);
								}
							}
						}
						else if (key == "glacier")
						{
							dungeon = new DungeonGlacier("glacier", this);
						}
					}
					else if (key == "house_in_bright")
					{
						dungeon = new DungeonHouseInBright("house_in_bright", this);
					}
				}
				else if (num <= 3324309751U)
				{
					if (num != 3222698627U)
					{
						if (num == 3324309751U)
						{
							if (key == "forest_in_tree")
							{
								dungeon = new DungeonForestInTree("forest_in_tree", this);
							}
						}
					}
					else if (key == "house_in")
					{
						dungeon = new DungeonHouseIn("house_in", this);
					}
				}
				else if (num != 3658226030U)
				{
					if (num == 4083262388U)
					{
						if (key == "forest_hiroba")
						{
							dungeon = new DungeonForestHiroba("forest_hiroba", this);
						}
					}
				}
				else if (key == "_")
				{
					dungeon = new Dungeon("_", this);
				}
			}
			if (dungeon != null)
			{
				this.ODgn[key] = dungeon;
			}
			return dungeon;
		}

		public void reloadTexture(Texture nTx)
		{
			this.MIchip.Tx = nTx;
			foreach (KeyValuePair<string, Dungeon> keyValuePair in this.ODgn)
			{
				keyValuePair.Value.reloadTexture(nTx);
			}
		}

		public void recreateMaterial()
		{
			foreach (KeyValuePair<string, Dungeon> keyValuePair in this.ODgn)
			{
				keyValuePair.Value.DisposeMaterial();
			}
			this.MIchip.DisposeMaterial();
			this.DisposeMaterial();
			this.clearMaterialLink();
		}

		private string getDgnKey(Map2d Mp)
		{
			if (this.ODgn == null)
			{
				this.init();
			}
			string text = "";
			if (REG.match(Mp.key, REG.RegFirstAlphabetUnderBar))
			{
				text = REG.R1.ToLower();
			}
			string s = Mp.prepareMeta().GetS("dgn");
			if (!TX.noe(s))
			{
				text = s;
			}
			if (TX.noe(text))
			{
				text = "";
			}
			return text;
		}

		public Dungeon getDgn(Map2d Mp)
		{
			string dgnKey = this.getDgnKey(Mp);
			return this.getDgn(dgnKey);
		}

		public Dungeon getDgn(string key)
		{
			if (this.ODgn == null)
			{
				this.init();
			}
			Dungeon dungeon;
			if (this.ODgn.TryGetValue(key, out dungeon))
			{
				return dungeon;
			}
			dungeon = this.createDungeonInstace(key);
			if (dungeon != null)
			{
				return dungeon;
			}
			if (this.ODgn.TryGetValue("_", out dungeon))
			{
				return dungeon;
			}
			return this.createDungeonInstace("_");
		}

		public M2Camera Cam
		{
			get
			{
				return this.M2D.Cam;
			}
		}

		public Material MtrBorderLight
		{
			get
			{
				if (this.MtrBorderLight_ == null)
				{
					this.MtrBorderLight_ = this.newMtr(M2DBase.getShd("M2d/BorderLight"));
				}
				return this.MtrBorderLight_;
			}
		}

		public Material MtrWaterInBright
		{
			get
			{
				if (this.MtrWaterInBright_ == null)
				{
					this.MtrWaterInBright_ = this.newMtr(M2DBase.getShd("M2d/WaterInBright"));
				}
				return this.MtrWaterInBright_;
			}
		}

		public Material MtrHouseLight
		{
			get
			{
				if (this.MtrHouseLight_ == null)
				{
					this.MtrHouseLight_ = this.newMtr(M2DBase.getShd("M2d/Gradation_HouseLight"));
				}
				return this.MtrHouseLight_;
			}
		}

		public Material MtrWaterSurfaceReflection
		{
			get
			{
				if (this.MtrWaterSurfaceReflection_ == null)
				{
					this.MtrWaterSurfaceReflection_ = this.newMtr(M2DBase.getShd("M2d/WaterReflectionSurface"));
				}
				return this.MtrWaterSurfaceReflection_;
			}
		}

		public Material MtrSubMapBlending
		{
			get
			{
				if (this.MtrSubMapBlending_ == null)
				{
					this.MtrSubMapBlending_ = this.MIchip.getMtr(M2DBase.getShd("M2d/SubMapBlending"), -1);
				}
				return this.MtrSubMapBlending_;
			}
		}

		public Material MtrSubMapTopMul
		{
			get
			{
				if (this.MtrSubMapTopMul_ == null)
				{
					this.MtrSubMapTopMul_ = this.MIchip.getMtr(M2DBase.getShd("M2d/SubMapTopMul"), -1);
				}
				return this.MtrSubMapTopMul_;
			}
		}

		public Material MtrSubMapTopNormal
		{
			get
			{
				if (this.MtrSubMapTopNormal_ == null)
				{
					this.MtrSubMapTopNormal_ = this.MIchip.getMtr(M2DBase.getShd("M2d/SubMapTopNormal"), -1);
				}
				return this.MtrSubMapTopNormal_;
			}
		}

		public Material MtrChipWhole
		{
			get
			{
				if (this.MtrChipWhole_ == null)
				{
					this.MtrChipWhole_ = this.MIchip.getMtr(M2DBase.getShd("M2d/WholeMap"), -1);
				}
				return this.MtrChipWhole_;
			}
		}

		public Material MtrChipWithBright
		{
			get
			{
				if (this.MtrChipWithBright_ == null)
				{
					this.MtrChipWithBright_ = this.MIchip.getMtr(M2DBase.getShd("M2d/BasicMapWithLight"), -1);
				}
				return this.MtrChipWithBright_;
			}
		}

		public Material MtrWater
		{
			get
			{
				if (this.MtrWater_ == null)
				{
					this.MtrWater_ = this.MIchip.getMtr(M2DBase.getShd("M2d/Water"), -1);
				}
				return this.MtrWater_;
			}
		}

		public Material MtrFakeLayerDissolve
		{
			get
			{
				if (this.MtrFakeLayerDissolve_ == null)
				{
					this.MtrFakeLayerDissolve_ = this.MIchip.getMtr(M2DBase.getShd("Hachan/DissolveFade"), -1);
					MTRX.setMaterialST(this.MtrFakeLayerDissolve_, "_NoiseTex", MTRX.SqEfPattern.getImage(8, 0), 0f);
					this.MtrFakeLayerDissolve_.SetFloat("_Count", 13f);
					this.MtrFakeLayerDissolve_.SetFloat("_CountDiv", 0.07692308f);
					this.MtrFakeLayerDissolve_.SetFloat("_Map_Scale", 2f);
				}
				return this.MtrFakeLayerDissolve_;
			}
		}

		public Material MtrWaterFall
		{
			get
			{
				if (this.MtrWaterFall_ == null)
				{
					this.MtrWaterFall_ = this.newMtr(M2DBase.getShd("Hachan/ShaderGDTWaterFall"));
					MTRX.setMaterialST(this.MtrWaterFall_, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
					this.MtrWaterFall_.SetFloat("_ScreenMargin", 8f);
				}
				return this.MtrWaterFall_;
			}
		}

		public Material MtrWaterFallT
		{
			get
			{
				if (this.MtrWaterFallT_ == null)
				{
					this.MtrWaterFallT_ = this.newMtr(M2DBase.getShd("Hachan/ShaderGDTWaterFall"));
					MTRX.setMaterialST(this.MtrWaterFallT_, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
					this.MtrWaterFallT_.SetFloat("_ScreenMargin", 8f);
				}
				return this.MtrWaterFallT_;
			}
		}

		public Shader getShd(string shader_name)
		{
			return M2DBase.getShd(shader_name);
		}

		public Material getMtr(int stencil_ref)
		{
			return this.MIchip.getMtr(stencil_ref);
		}

		public Material getMtr(Shader Shd, int stencil_ref = -1)
		{
			return this.MIchip.getMtr(Shd, stencil_ref);
		}

		public Material getMtr(BLEND blnd = BLEND.NORMAL, int stencil_ref = -1)
		{
			return this.MIchip.getMtr(blnd, stencil_ref);
		}

		public readonly M2DBase M2D;

		private readonly MImage MIchip;

		private BDic<string, Dungeon> ODgn;

		private Material MtrBorderLight_;

		private Material MtrSubMapBlending_;

		private Material MtrSubMapTopMul_;

		private Material MtrSubMapTopNormal_;

		private Material MtrChipWhole_;

		private Material MtrChipWithBright_;

		private Material MtrWater_;

		private Material MtrWaterInBright_;

		private Material MtrWaterSurfaceReflection_;

		private Material MtrHouseLight_;

		private Material MtrWaterFall_;

		private Material MtrWaterFallT_;

		private Material MtrFakeLayerDissolve_;

		public readonly DgnColorContainer ColCon;

		private List<Material> ACreatedMtr;

		public const int border_scale = 2;
	}
}
