using System;

namespace XX
{
	public class QuakeItem
	{
		public QuakeItem Set(float _slevel, float _time, float _elevel = -1f, float _saf = 0f)
		{
			this.slevel = _slevel;
			this.elevel = ((_elevel < 0f) ? this.slevel : _elevel);
			this.time = _time;
			if (_saf < 0f)
			{
				this.saf = 0f;
				this.t = -_saf;
			}
			else
			{
				this.saf = _saf;
				this.t = 0f;
			}
			return this;
		}

		public QuakeItem Vib(float _slevel, float _time, float _elevel = -1f, float _saf = 0f)
		{
			this.qtype = QuakeItem.QUAKETYPE.VIB;
			return this.Set(_slevel, _time, _elevel, _saf);
		}

		public QuakeItem SinH(float _slevel, float _time, float _elevel = -1f, float _saf = 0f)
		{
			this.qtype = QuakeItem.QUAKETYPE.SIN;
			this.val_cos = X.Cos(0f);
			this.val_sin = X.Sin(0f);
			return this.Set(_slevel, _time, _elevel, _saf);
		}

		public QuakeItem SinV(float _slevel, float _time, float _elevel = -1f, float _saf = 0f)
		{
			this.qtype = QuakeItem.QUAKETYPE.SIN;
			this.val_cos = X.Cos(1.5707964f);
			this.val_sin = X.Sin(1.5707964f);
			return this.Set(_slevel, _time, _elevel, _saf);
		}

		public QuakeItem SinR(float _slevel, float _time, float agR, float _elevel = -1f, float _saf = 0f)
		{
			this.qtype = QuakeItem.QUAKETYPE.SIN;
			this.val_cos = X.Cos(agR);
			this.val_sin = X.Sin(agR);
			return this.Set(_slevel, _time, _elevel, _saf);
		}

		public QuakeItem HandShake(int _holdtime, float _fadetime, float _level, int _saf = 0)
		{
			this.qtype = QuakeItem.QUAKETYPE.HANDSHAKE;
			this.val_cos = X.NIXP(95f, 170f);
			this.val_sin = X.NIXP(95f, 170f);
			return this.Set(_level, (float)_holdtime + _fadetime * 2f, _fadetime, (float)_saf);
		}

		public bool run(float fcnt = 1f, bool only_reduce_saf = false)
		{
			if (this.saf > 0f)
			{
				this.saf = X.Mx(0f, this.saf - fcnt);
				return true;
			}
			this.t += fcnt;
			if (this.t >= this.time)
			{
				this.x = (this.y = 0f);
				return false;
			}
			if (only_reduce_saf)
			{
				return true;
			}
			switch (this.qtype)
			{
			case QuakeItem.QUAKETYPE.VIB:
			{
				float num = X.NI(this.slevel, this.elevel, this.t / this.time);
				this.x = (X.XORSP() * 2f - 1f) * num;
				this.y = (X.XORSP() * 2f - 1f) * num;
				break;
			}
			case QuakeItem.QUAKETYPE.SIN:
			{
				float num = X.NI(this.slevel, this.elevel, this.t / this.time) * X.Sin0(this.t / 31f);
				this.x = this.val_cos * num;
				this.y = this.val_sin * num;
				break;
			}
			case QuakeItem.QUAKETYPE.HANDSHAKE:
			{
				float num;
				if (this.t < this.elevel)
				{
					num = this.slevel * X.ZPOW(this.t, this.elevel);
				}
				else if (this.t >= this.time - this.elevel)
				{
					num = this.slevel * (1f - X.ZSIN(this.t - (this.time - this.elevel), this.elevel));
				}
				else
				{
					num = this.slevel;
				}
				this.x = num * X.COSI(this.t, this.val_cos);
				this.y = num * X.SINI(this.t, this.val_sin);
				break;
			}
			default:
				return false;
			}
			return true;
		}

		public bool isVib()
		{
			return this.qtype == QuakeItem.QUAKETYPE.VIB;
		}

		public float x;

		public float y;

		private QuakeItem.QUAKETYPE qtype;

		private float slevel;

		private float elevel;

		private float val_cos;

		private float val_sin;

		private float saf;

		private float time;

		private float t;

		public enum QUAKETYPE
		{
			VIB,
			SIN,
			HANDSHAKE
		}
	}
}
