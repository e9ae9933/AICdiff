using System;

namespace XX
{
	public class WaterShakeDrawer : WaterShakeDrawerT<WaveItem>
	{
		public WaterShakeDrawer(int _count)
			: base(_count)
		{
		}

		protected override WaveItem createWaveItem()
		{
			return new WaveItem();
		}
	}
}
