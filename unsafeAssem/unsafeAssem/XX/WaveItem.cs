using System;

namespace XX
{
	public class WaveItem
	{
		public virtual float get_y(float _x, ALIGNY wave_align)
		{
			if (_x < this.x - this.sinw || this.x + this.sinw <= _x)
			{
				return 0f;
			}
			float num = _x - (this.x - this.sinw);
			float num2 = ((_x < this.x) ? X.ZSIN(num, this.sinw) : X.ZSINV(this.sinw - (_x - this.x), this.sinw));
			return (X.Sin(num / this.sinw * 6.2831855f) * (float)X.MPF(this.h > 0f) + (float)wave_align) * X.Abs(this.h) * num2;
		}

		public virtual bool progress(int td, float w)
		{
			bool flag = false;
			this.t0 += td;
			this.x += (float)td * this.spd;
			float num = w + this.sinw * 2f;
			float num2 = this.x + this.sinw;
			if (num2 >= num)
			{
				flag = true;
				num2 %= num;
			}
			else if (num2 < 0f)
			{
				num2 = (num2 + (float)((int)(-num2 / num + 3f)) * num) % num;
				flag = true;
			}
			this.x = num2 - this.sinw;
			return flag;
		}

		public float x;

		public float h;

		public float spd;

		public float sinw;

		public float w;

		public int t0;
	}
}
