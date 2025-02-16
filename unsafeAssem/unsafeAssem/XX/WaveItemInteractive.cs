using System;

namespace XX
{
	public class WaveItemInteractive : WaveItem
	{
		public WaveItemInteractive(WaterShakeDrawerInteractive _Con)
		{
			this.Con = _Con;
		}

		public override float get_y(float _x, ALIGNY wave_align)
		{
			return base.get_y(_x, wave_align) * this.clevel;
		}

		public override bool progress(int td, float w)
		{
			bool flag = base.progress(td, w);
			if (this.inter_f0 == 0 && this.clevel != this.Con.dep_level)
			{
				this.clevel = X.VALWALK(this.clevel, this.Con.dep_level, this.Con.level_change_speed);
			}
			else if (this.inter_f0 > 0 && flag)
			{
				this.clevel = 0f;
			}
			return flag;
		}

		public readonly WaterShakeDrawerInteractive Con;

		public float clevel;

		public int inter_f0;
	}
}
