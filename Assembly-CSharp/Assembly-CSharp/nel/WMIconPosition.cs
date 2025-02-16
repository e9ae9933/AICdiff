using System;
using m2d;
using UnityEngine;

namespace nel
{
	public struct WMIconPosition
	{
		public WMIconPosition(WMIcon _Ico, WholeMapItem.WMItem Wmi, WMIconHiddenDeperture _Whd = default(WMIconHiddenDeperture))
		{
			this.Ico = _Ico;
			this.Whd = _Whd;
			Vector2 mapWmPos = _Ico.getMapWmPos(Wmi);
			this.DestMap = Wmi.SrcMap;
			this.wmx = mapWmPos.x + Wmi.Rc.x;
			this.wmy = mapWmPos.y + Wmi.Rc.y;
		}

		public WMIconPosition(WholeMapItem.WMItem Wmi)
		{
			this.DestMap = Wmi.SrcMap;
			this.Ico = null;
			this.Whd = new WMIconHiddenDeperture(this.DestMap, Wmi.SrcMap.clms / 2, Wmi.SrcMap.rows / 2);
			this.wmx = Wmi.Rc.cx;
			this.wmy = Wmi.Rc.cy;
		}

		public WmPosition getDepertWmPos(WholeMapItem Wmi)
		{
			return new WmPosition(Wmi, Wmi.GetWmi(this.DestMap, null), default(WMSpecialIcon));
		}

		public Map2d getDepertureMap()
		{
			return (this.Whd.valid ? this.Whd.DestMap : null) ?? this.DestMap;
		}

		public Vector2 getDepertureMapPos()
		{
			if (this.Whd.valid)
			{
				return new Vector2((float)this.Whd.x, (float)this.Whd.y);
			}
			return new Vector2((float)this.Ico.x, (float)this.Ico.y);
		}

		public bool valid
		{
			get
			{
				return this.Ico != null || this.Whd.valid;
			}
		}

		public bool isSame(WMIconPosition P)
		{
			return this.Ico == P.Ico && this.DestMap == P.DestMap && this.Whd.Equals(P);
		}

		public WMIcon get_Icon()
		{
			return this.Ico;
		}

		private readonly WMIcon Ico;

		public readonly Map2d DestMap;

		public readonly WMIconHiddenDeperture Whd;

		public readonly float wmx;

		public readonly float wmy;
	}
}
