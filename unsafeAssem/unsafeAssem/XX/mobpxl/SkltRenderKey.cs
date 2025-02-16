using System;

namespace XX.mobpxl
{
	internal struct SkltRenderKey
	{
		internal SkltRenderKey(MobSklt _Sklt, string _colvari_key)
		{
			this.Sklt = _Sklt;
			this.colvari_key = _colvari_key;
		}

		internal readonly MobSklt Sklt;

		internal readonly string colvari_key;
	}
}
