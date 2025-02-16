using System;

namespace XX
{
	public sealed class WaterShakeDrawerInteractive : WaterShakeDrawerT<WaveItemInteractive>
	{
		public WaterShakeDrawerInteractive(int _count)
			: base(_count)
		{
			this.count0 = _count;
		}

		public WaterShakeDrawerInteractive setSeedXy(int xi, int yi, int add = 0)
		{
			uint ran = X.GETRAN2(49 + add + xi * 43 + yi * 51, 2 + yi % 3 + xi % 5 + add % 7);
			this.random_seed = (int)(ran % 16777215U);
			return this;
		}

		protected override WaveItemInteractive createWaveItem()
		{
			return new WaveItemInteractive(this)
			{
				clevel = this.dep_level
			};
		}

		public void setWaveHigh(float lv)
		{
			for (int i = this.count - 1; i >= 0; i--)
			{
				WaveItemInteractive waveItemInteractive = this.AWave[i];
				if (waveItemInteractive.inter_f0 == 0)
				{
					waveItemInteractive.clevel = X.Mx(lv, waveItemInteractive.clevel);
				}
			}
		}

		public override void waveProgress(float w, int t)
		{
			base.waveProgress(w, t);
			if (this.has_interactive)
			{
				this.has_interactive = false;
				for (int i = this.count - 1; i >= 0; i--)
				{
					WaveItemInteractive waveItemInteractive = this.AWave[i];
					if (waveItemInteractive.inter_f0 > 0 && waveItemInteractive.inter_f0 < t)
					{
						waveItemInteractive.clevel -= (float)(t - waveItemInteractive.inter_f0) * this.level_change_speed;
						if (waveItemInteractive.clevel < 0f)
						{
							this.AWave.RemoveAt(i);
							this.count--;
						}
						else
						{
							this.has_interactive = true;
						}
					}
				}
			}
		}

		public void addInteractive(float x, float level, int t0)
		{
			if (this.count > this.count0 * 4)
			{
				return;
			}
			int count = this.count;
			WaveItemInteractive waveItemInteractive = this.createWaveItem();
			WaveItemInteractive waveItemInteractive2 = this.createWaveItem();
			waveItemInteractive.spd = X.NI(this.min_spd, this.max_spd, X.XORSP());
			waveItemInteractive2.spd = -waveItemInteractive.spd;
			float num = (float)X.MPF(X.xors(2) == 1);
			waveItemInteractive.sinw = (waveItemInteractive2.sinw = X.NI(this.min_w, this.max_w, X.XORSP()) * 0.4f);
			waveItemInteractive.h = X.NI(this.min_h, this.max_h, X.XORSP()) * num;
			waveItemInteractive2.h = -waveItemInteractive.h;
			waveItemInteractive.x = x - waveItemInteractive.sinw * 0.25f;
			waveItemInteractive2.x = x - waveItemInteractive.sinw * 0.25f;
			this.has_interactive = true;
			WaveItemInteractive waveItemInteractive3 = waveItemInteractive;
			waveItemInteractive2.clevel = level;
			waveItemInteractive3.clevel = level;
			WaveItem waveItem = waveItemInteractive;
			WaveItem waveItem2 = waveItemInteractive2;
			WaveItemInteractive waveItemInteractive4 = waveItemInteractive;
			waveItemInteractive2.inter_f0 = t0;
			waveItemInteractive4.inter_f0 = t0;
			waveItem2.t0 = t0;
			waveItem.t0 = t0;
			this.AWave.Add(waveItemInteractive);
			this.AWave.Add(waveItemInteractive2);
			this.count += 2;
		}

		public float dep_level = 0.5f;

		public int count0;

		public float level_change_speed = 0.004f;

		private bool has_interactive;
	}
}
