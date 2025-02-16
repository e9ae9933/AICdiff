using System;
using m2d;
using UnityEngine;

namespace nel
{
	public struct WMIconPosition
	{
		public WMIconPosition(WMIcon _Ico, WholeMapItem.WMItem Wmi, WMIconHiddenDeperture _Whd)
		{
			this.Ico = _Ico;
			this.Whd = _Whd;
			Vector2 mapWmPos = _Ico.getMapWmPos(Wmi);
			this.DestMap = Wmi.SrcMap;
			this.wmx = mapWmPos.x + Wmi.Rc.x;
			this.wmy = mapWmPos.y + Wmi.Rc.y;
		}

		public Map2d getDepertureMap()
		{
			return ((this.Whd != null) ? this.Whd.DestMap : null) ?? this.DestMap;
		}

		public Vector2 getDepertureMapPos()
		{
			if (this.Whd != null)
			{
				return new Vector2((float)this.Whd.x, (float)this.Whd.y);
			}
			return new Vector2((float)this.Ico.x, (float)this.Ico.y);
		}

		public bool valid
		{
			get
			{
				return this.Ico != null;
			}
		}

		public bool isSame(WMIconPosition P)
		{
			return this.Ico == P.Ico && this.DestMap == P.DestMap && this.Whd == P.Whd;
		}

		public WMIcon get_Icon()
		{
			return this.Ico;
		}

		private readonly WMIcon Ico;

		public readonly Map2d DestMap;

		private readonly WMIconHiddenDeperture Whd;

		public readonly float wmx;

		public readonly float wmy;
	}
}
