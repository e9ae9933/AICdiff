﻿using System;

namespace nel
{
	public struct WmDeperture
	{
		public WmDeperture(string _wm_key, string _map_key)
		{
			this.wm_key = _wm_key;
			this.map_key = _map_key;
		}

		public WmPosition getPosCache(WholeMapItem Wm)
		{
			if (this.map_key == null)
			{
				return new WmPosition(Wm, null, default(WMSpecialIcon));
			}
			WholeMapItem.WMItem wmitem = null;
			WMSpecialIcon wmspecialIcon = default(WMSpecialIcon);
			Wm.GetWMItem(this.map_key, ref wmitem, ref wmspecialIcon);
			return new WmPosition(Wm, wmitem, wmspecialIcon);
		}

		public string wm_key;

		public string map_key;
	}
}
