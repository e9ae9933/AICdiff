using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using XX;

namespace m2d
{
	public sealed class M2MapMaterialLoader
	{
		public M2DBase M2D
		{
			get
			{
				return this.Mp.M2D;
			}
		}

		public M2MapMaterialLoader(Map2d _Mp, bool _load_additinal_material = true)
		{
			this.Mp = _Mp;
			this.load_additinal_material = _load_additinal_material;
			this.ASubMapTemp = new List<Map2d>();
		}

		public bool read(int count = 400)
		{
			if (this.ASubMapTemp == null)
			{
				return false;
			}
			while (count != 0)
			{
				Map2d map2d = ((this.sm_index == -1) ? this.Mp : this.ASubMapTemp[this.sm_index]);
				if (this.line < 0)
				{
					if (!map2d.prepared)
					{
						map2d.prepared = true;
					}
					map2d.getMapBodyContentReader(ref this.CR, ref this.BaLoad, true);
					if (this.CR != null)
					{
						this.line = 0;
					}
					else
					{
						this.line = (int)this.BaLoad.position;
					}
					bool flag = this.load_additinal_material;
				}
				if (this.BaLoad != null)
				{
					this.BaLoad.position = (ulong)((long)this.line);
					if (map2d.prepareMaterialFromCurrentReader(this, ref count, this.ASubMapTemp, this.load_additinal_material))
					{
						this.line = (int)this.BaLoad.position;
						return true;
					}
				}
				else if (this.CR != null)
				{
					this.CR.seek_set(this.line);
					if (map2d.prepareMaterialFromCurrentReader(this, ref count, this.ASubMapTemp, this.load_additinal_material))
					{
						this.line = this.CR.get_cur_line() + 1;
						return true;
					}
				}
				if (!this.pxl_loaded && !map2d.is_whole)
				{
					this.M2D.IMGS.Atlas.initCspAtlas(M2DBase.Achip_pxl_key);
				}
				this.line = -1;
				this.CR = null;
				this.BaLoad = null;
				this.pxl_loaded = false;
				this.sm_index++;
				if (this.sm_index >= this.ASubMapTemp.Count)
				{
					this.ASubMapTemp = null;
					return false;
				}
			}
			return true;
		}

		public readonly Map2d Mp;

		public readonly bool load_additinal_material;

		private int sm_index = -1;

		private int line = -1;

		private ByteArray BaLoad;

		private CsvReader CR;

		private List<Map2d> ASubMapTemp;

		public bool next_map;

		public bool pxl_loaded;

		public bool flushing;
	}
}
