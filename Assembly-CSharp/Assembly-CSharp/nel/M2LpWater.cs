using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class M2LpWater : NelLp, IRunAndDestroy
	{
		public M2LpWater(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.Meta = new META(this.comment);
		}

		public override void initAction(bool normal_map)
		{
			if (this.Meta == null)
			{
				this.initActionPre();
			}
			if (this.Meta.Get("fill") != null && this.AList == null)
			{
				this.Mp.addRunnerObject(this);
			}
		}

		public void destruct()
		{
		}

		public override void closeAction(bool l)
		{
			base.closeAction(l);
			this.Mp.remRunnerObject(this);
			if (this.AList != null)
			{
				MistManager mist = (this.Mp.M2D as NelM2DBase).MIST;
				int num = this.AList.Length;
				for (int i = 0; i < num; i++)
				{
					NelChipWater nelChipWater = this.AList[i];
					if (nelChipWater != null)
					{
						if (mist != null)
						{
							mist.clearAt(this.AttachedKind, nelChipWater.mapx, nelChipWater.mapy);
						}
						this.Lay.removeChip(nelChipWater, true, true);
					}
				}
				this.AList = null;
			}
			this.Meta = null;
		}

		public bool run(float fcnt)
		{
			if (this.Mp.BCC == null || !this.Mp.BCC.is_prepared)
			{
				return true;
			}
			NelM2DBase nelM2DBase = this.Mp.M2D as NelM2DBase;
			string[] array = this.Meta.Get("fill");
			M2ChipImage m2ChipImage = nelM2DBase.IMGS.Get(array[0]);
			if (m2ChipImage != null)
			{
				List<NelChipWater> list = new List<NelChipWater>();
				int num = this.mapw + this.mapx;
				int num2 = this.maph + this.mapy;
				MistManager.MistKind mistKind = (this.AttachedKind = null);
				string s = this.Meta.GetS("water_whole_fill");
				if (TX.valid(s))
				{
					mistKind = (this.AttachedKind = NelChipWater.getWaterKind(this.Mp.Dgn, this.water_type, this.water_level, false, this.water_bottom_raise, this.water_surface, this.water_raise_alloc_level));
					nelM2DBase.MIST.assignWholeFillMist(mistKind, CCON.getStringToValue(s));
				}
				else
				{
					bool water_static = this.water_static;
					M2Chip m2Chip = null;
					int num3 = this.mapx;
					while (num3 < num && m2ChipImage != null)
					{
						for (int i = this.mapy; i < num2; i++)
						{
							if (nelM2DBase.MIST.isAvailablePos(num3, i) && CCON.mistPassable(this.Mp.getConfig(num3, i), 0))
							{
								NelChipWater nelChipWater = m2ChipImage.CreateOneChip(this.Lay, (int)((float)num3 * base.CLEN), (int)((float)i * base.CLEN), 255, 0, false) as NelChipWater;
								if (nelChipWater == null)
								{
									X.de("Lp:" + ((this != null) ? this.ToString() : null) + ":: NelChipWater となるパスが指定されていない " + array[0], null);
									m2ChipImage = null;
									break;
								}
								if (mistKind == null)
								{
									mistKind = (this.AttachedKind = NelChipWater.getWaterKind(this.Mp.Dgn, this.water_type, water_static ? (this.maph - 1) : this.water_level, true, this.water_bottom_raise, this.water_surface, this.water_raise_alloc_level));
									float water_surface_raise_pixel = this.water_surface_raise_pixel;
									if (water_surface_raise_pixel > 0f)
									{
										mistKind.surface_raise_pixel = water_surface_raise_pixel;
									}
								}
								nelChipWater.K = mistKind;
								nelChipWater.water_amount = this.water_amount;
								nelChipWater.arrangeable = true;
								nelChipWater.mapx = num3;
								nelChipWater.mapy = i;
								nelChipWater.inputRots(true);
								nelChipWater.static_clip_top = (water_static ? this.mapy : (-1));
								this.Lay.assignNewMapChip(nelChipWater, -2, true, false);
								list.Add(nelChipWater);
								nelChipWater.initAction(true);
								m2Chip = m2Chip ?? nelChipWater;
							}
						}
						num3++;
					}
					if (list.Count > 0)
					{
						this.AList = list.ToArray();
					}
					if (m2Chip != null && water_static)
					{
						nelM2DBase.MIST.addStaticWaterArea(base.unique_key, mistKind, (int)m2Chip.Img.Aconfig[0], this.mapx, this.mapy, this.mapw, this.maph);
					}
				}
			}
			else
			{
				X.de("Lp:" + ((this != null) ? this.ToString() : null) + ":: 不明な fill パス " + array[0], null);
			}
			return false;
		}

		public int water_level
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				return this.Meta.GetI("water_level", -2, 0);
			}
		}

		public int water_amount
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				int i = this.Meta.GetI("water_amount", -1, 0);
				if (i < 0)
				{
					return -1;
				}
				return i * 255;
			}
		}

		public int water_surface
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				if (!this.Meta.GetB("water_surface_draw", false))
				{
					return -1;
				}
				return this.mapy;
			}
		}

		public bool water_bottom_raise
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				return this.Meta.GetI("water_bottom_raise", 0, 0) != 0;
			}
		}

		public float water_raise_alloc_level
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				return this.Meta.GetNm("water_raise_alloc_level", 1f, 0);
			}
		}

		public float water_surface_raise_pixel
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				return this.Meta.GetNm("water_surface_raise_pixel", 0f, 0);
			}
		}

		public bool water_static
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				return this.Meta.GetI("water_static", 0, 0) != 0;
			}
		}

		public bool water_whole_fill
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				return this.Meta.GetI("water_whole_fill", 0, 0) != 0;
			}
		}

		public MistManager.MISTTYPE water_type
		{
			get
			{
				if (this.Meta == null)
				{
					this.initActionPre();
				}
				string s = this.Meta.GetS("water_type");
				MistManager.MISTTYPE misttype;
				if (s == null)
				{
					misttype = MistManager.MISTTYPE.WATER;
				}
				else if (!FEnum<MistManager.MISTTYPE>.TryParse(s.ToUpper(), out misttype, true))
				{
					X.de("不明なMISTTYPE: " + s, null);
					misttype = MistManager.MISTTYPE.WATER;
				}
				return misttype;
			}
		}

		private META Meta;

		private NelChipWater[] AList;

		private MistManager.MistKind AttachedKind;
	}
}
