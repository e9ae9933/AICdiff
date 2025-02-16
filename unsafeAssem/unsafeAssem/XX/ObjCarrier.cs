using System;
using UnityEngine;

namespace XX
{
	public class ObjCarrier
	{
		public float Calc(float t, Transform Trans, bool reverse = false)
		{
			float num = ((this.start_shift_maxt <= 0f) ? ((float)((t >= 0f) ? 0 : 1)) : this.Fn_shift.Get(null, t / this.start_shift_maxt));
			if (reverse)
			{
				num = 1f - num;
			}
			Vector3 localPosition = Trans.localPosition;
			localPosition.x = this.base_x + this.start_shiftx * num;
			localPosition.y = this.base_y + this.start_shifty * num;
			Trans.localPosition = localPosition;
			return num;
		}

		public virtual ObjCarrier Base(float x, float y)
		{
			this.base_x = x;
			this.base_y = y;
			return this;
		}

		public ObjCarrier BasePx(float x, float y)
		{
			return this.Base(x * 0.015625f, y * 0.015625f);
		}

		public ObjCarrier StartShift(float x, float y, int maxt, string shiftt = "")
		{
			this.start_shiftx = x;
			this.start_shifty = y;
			this.start_shift_maxt = (float)maxt;
			this.Fn_shift = new EfParticleFuncCalc(shiftt, "RZSIN", 1f);
			return this;
		}

		public ObjCarrier StartShiftPx(float x, float y, int maxt, string shiftt = "")
		{
			this.start_shiftx = x * 0.015625f;
			this.start_shifty = y * 0.015625f;
			this.start_shift_maxt = (float)maxt;
			this.Fn_shift = new EfParticleFuncCalc(shiftt, "RZSIN", 1f);
			return this;
		}

		public float base_x;

		public float base_y;

		public float start_shiftx;

		public float start_shifty;

		public float start_shift_maxt;

		public EfParticleFuncCalc Fn_shift;
	}
}
