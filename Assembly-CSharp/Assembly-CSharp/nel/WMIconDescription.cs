using System;

namespace nel
{
	public struct WMIconDescription
	{
		public WMIconDescription(WMIcon _Icon, string _wm_key, string _map_key)
		{
			this.Icon = _Icon;
			this.wm_key = _wm_key;
			this.map_key = _map_key;
		}

		public WMIcon Icon;

		public string wm_key;

		public string map_key;
	}
}
