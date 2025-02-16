using System;
using XX;

namespace nel
{
	public class TimeDeclineArea : DRect
	{
		public TimeDeclineArea(float a, float b, float _w, float _h, float _time)
			: base("", a, b, _w, _h, 0f)
		{
			this.time = _time;
		}

		public float time = -1f;
	}
}
