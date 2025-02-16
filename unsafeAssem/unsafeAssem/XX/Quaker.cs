using System;
using UnityEngine;

namespace XX
{
	public class Quaker : IAnimListener
	{
		public Quaker(Transform _Trans = null)
		{
			this.AQi = new QuakeItem[8];
			if (_Trans != null)
			{
				this.ATrans = new Transform[1];
				this.ATrans[0] = _Trans;
				return;
			}
			this.ATrans = new Transform[0];
		}

		public Quaker AttachTransform(Transform _Trans)
		{
			X.push<Transform>(ref this.ATrans, _Trans, -1);
			return this;
		}

		public Quaker clearTransform()
		{
			this.ATrans = new Transform[0];
			return this;
		}

		public int Length
		{
			get
			{
				return this.q_i;
			}
		}

		private QuakeItem Pop()
		{
			if (this.q_i >= this.AQi.Length)
			{
				Array.Resize<QuakeItem>(ref this.AQi, this.q_i * 2);
			}
			QuakeItem quakeItem = this.AQi[this.q_i];
			if (quakeItem == null)
			{
				QuakeItem[] aqi = this.AQi;
				int num = this.q_i;
				this.q_i = num + 1;
				return aqi[num] = new QuakeItem();
			}
			this.q_i++;
			return quakeItem;
		}

		public Quaker Vib(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			this.Pop().Vib(_slevel, _time, _elevel, (float)_saf);
			return this;
		}

		public Quaker VibP(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			return this.Vib(_slevel * 0.015625f, _time, (_elevel < 0f) ? _elevel : (_elevel * 0.015625f), _saf);
		}

		public Quaker SinH(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			this.Pop().SinH(_slevel, _time, _elevel, (float)_saf);
			return this;
		}

		public Quaker SinV(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			this.Pop().SinV(_slevel, _time, _elevel, (float)_saf);
			return this;
		}

		public Quaker SinR(float _slevel, float _time, float agR, float _elevel = -1f, int _saf = 0)
		{
			this.Pop().SinR(_slevel, _time, agR, _elevel, (float)_saf);
			return this;
		}

		public Quaker HandShake(float _holdtime, float _fadetime, float _level, int _saf = 0)
		{
			this.Pop().HandShake((int)_holdtime, _fadetime, _level, _saf);
			return this;
		}

		public void clear()
		{
			this.clearLoc();
			X.ALLN<QuakeItem>(this.AQi);
			this.q_i = 0;
		}

		public void clearLoc()
		{
			if (this.x != 0f || this.y != 0f)
			{
				int num = this.ATrans.Length;
				for (int i = 0; i < num; i++)
				{
					Transform transform = this.ATrans[i];
					Vector3 position = transform.position;
					position.x -= this.x;
					position.y -= this.y;
					transform.position = position;
				}
			}
			this.x = 0f;
			this.y = 0f;
		}

		public bool updateAnimator(float fcnt)
		{
			return this.run(fcnt);
		}

		public bool run(float fcnt = 1f)
		{
			this.changed = false;
			if (this.q_i == 0)
			{
				return false;
			}
			bool flag = false;
			int i = 0;
			int num = this.ATrans.Length;
			this.clearLoc();
			while (i < this.q_i)
			{
				QuakeItem quakeItem = this.AQi[i];
				if (quakeItem.isVib() && flag)
				{
					quakeItem.run(fcnt, true);
					i++;
				}
				else
				{
					this.changed = true;
					if (!quakeItem.run(fcnt, false))
					{
						X.shiftEmpty<QuakeItem>(this.AQi, 1, i, -1);
						this.q_i--;
					}
					else
					{
						this.x += quakeItem.x * 0.015625f;
						this.y += quakeItem.y * 0.015625f;
						if (quakeItem.isVib())
						{
							flag = true;
						}
						i++;
					}
				}
			}
			for (int j = 0; j < num; j++)
			{
				Transform transform = this.ATrans[j];
				Vector3 position = transform.position;
				position.x += this.x;
				position.y += this.y;
				transform.position = position;
			}
			return true;
		}

		public bool readPtcScript(PTCThread rER, float level = 1f)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				if (cmd == "%QU_CLEAR")
				{
					this.clear();
					return true;
				}
				if (cmd == "%QU_VIB")
				{
					this.Vib(rER.Nm(1, 0f) * level, (float)rER.Int(2, 1), rER.Nm(3, -1f) * level, rER.Int(4, 0));
					return true;
				}
				if (cmd == "%QU_SINH")
				{
					this.SinH(rER.Nm(1, 0f) * level, (float)rER.Int(2, 1), rER.Nm(3, -1f) * level, rER.Int(4, 0));
					return true;
				}
				if (cmd == "%QU_SINV")
				{
					this.SinV(rER.Nm(1, 0f) * level, (float)rER.Int(2, 1), rER.Nm(3, -1f) * level, rER.Int(4, 0));
					return true;
				}
				if (cmd == "%QU_HANDSHAKE")
				{
					this.HandShake(rER.Nm(1, 0f), rER.Nm(2, 0f), rER.Nm(3, 1f) * level, rER.Int(4, 0));
					return true;
				}
			}
			return false;
		}

		public Transform[] ATrans;

		public float x;

		public float y;

		private QuakeItem[] AQi;

		private int q_i;

		public bool changed;
	}
}
