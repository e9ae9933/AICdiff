using System;
using System.Collections.Generic;

namespace nel
{
	public class TimeDeclineAreaContainer
	{
		public TimeDeclineAreaContainer(int capacity = 4)
		{
			this.ARc = new List<TimeDeclineArea>(capacity);
		}

		public TimeDeclineAreaContainer Add(float a, float b, float _w, float _h, float time)
		{
			this.ARc.Add(new TimeDeclineArea(a, b, _w, _h, time));
			return this;
		}

		public bool run(float fcnt)
		{
			int num = this.ARc.Count;
			for (int i = this.ARc.Count - 1; i >= 0; i--)
			{
				TimeDeclineArea timeDeclineArea = this.ARc[i];
				timeDeclineArea.time -= fcnt;
				if (timeDeclineArea.time <= 0f)
				{
					this.ARc.RemoveAt(i);
					num--;
				}
			}
			return num > 0;
		}

		public bool isinXy(float _x, float _y, float marg_x = 0f, float marg_y = 0f)
		{
			return this.isCovering(_x - marg_x, _y - marg_y, _x + marg_x, _y + marg_y);
		}

		public bool isCovering(float _x, float _y, float r, float b)
		{
			int count = this.ARc.Count;
			for (int i = this.ARc.Count - 1; i >= 0; i--)
			{
				if (this.ARc[i].isCoveringXy(_x, _y, r, b, 0f, -1000f))
				{
					return true;
				}
			}
			return false;
		}

		private List<TimeDeclineArea> ARc;
	}
}
