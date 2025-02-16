using System;
using m2d;
using PixelLiner.PixelLinerLib;

namespace nel
{
	public struct WMIconHiddenDeperture
	{
		public WMIconHiddenDeperture(Map2d _DestMap, int _x, int _y)
		{
			this.DestMap = _DestMap;
			this.x = (ushort)_x;
			this.y = (ushort)_y;
		}

		public WMIconHiddenDeperture(ByteReader Ba, M2DBase M2D)
		{
			this.DestMap = M2D.Get(Ba.readPascalString("utf-8", false), false);
			this.x = Ba.readUShort();
			this.y = Ba.readUShort();
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writePascalString(this.DestMap.key, "utf-8");
			Ba.writeUShort(this.x);
			Ba.writeUShort(this.y);
		}

		public bool valid
		{
			get
			{
				return this.DestMap != null;
			}
		}

		public bool Equals(WMIconHiddenDeperture Src)
		{
			return this.x == Src.x && this.y == Src.y && this.DestMap == Src.DestMap;
		}

		public readonly Map2d DestMap;

		public readonly ushort x;

		public readonly ushort y;
	}
}
