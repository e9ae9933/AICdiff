﻿using System;

namespace XX
{
	public sealed class EfParticleFuncCalcItem
	{
		public EfParticleFuncCalcItem(string fntype, float _st, float _et, float _mult, bool _time_rate)
		{
			try
			{
				if (TX.charIs(fntype, 0, 'R'))
				{
					this.reverse_flag = true;
				}
				this.Fn = ((fntype == "Z1") ? new FnZoom(X.Zone) : X.getFnZoom(fntype));
			}
			catch
			{
				X.de("Function " + fntype + " が見つかりません", null);
				this.Fn = new FnZoom(X.ZLINE);
			}
			this.st = _st;
			this.et = _et;
			this.mult = _mult;
			this.time_rate = _time_rate;
		}

		public float Get(EfParticle EP, float ratio = -1f)
		{
			if (this.st == this.et)
			{
				return 0f;
			}
			float num = ((ratio < 0f && EP != null) ? EfParticle.tz : ratio);
			float num2 = this.et;
			if (!this.time_rate && EP != null)
			{
				num *= (float)EP.maxt;
				if (this.et == -1000f)
				{
					num2 = (float)EP.maxt;
				}
			}
			num = (num - this.st) / (num2 - this.st);
			return this.Fn(num) * this.mult;
		}

		private FnZoom Fn;

		private float st;

		private float et = 1f;

		private float mult = 1f;

		private bool time_rate = true;

		private bool reverse_flag;
	}
}
