using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class NASStasisChecker : NelEnemyAssist
	{
		public NASStasisChecker(NelEnemy En, float _maxt = 50f, int _sample_interval = 10)
			: base(En)
		{
			this.maxt = _maxt;
			this.AsamplePos = new List<Vector2>();
			this.sample_interval = _sample_interval;
			this.shuffle();
		}

		public int sample_interval
		{
			get
			{
				return this.sample_interval_;
			}
			set
			{
				if (this.sample_interval == value)
				{
					return;
				}
				this.sample_interval_ = value;
				this.sample_interval_r = 1f / (float)this.sample_interval_;
				if (this.AsamplePos.Capacity < this.sample_interval_)
				{
					this.AsamplePos.Capacity = this.sample_interval_;
				}
			}
		}

		public void shuffle()
		{
			this.maxt_current = X.NIXP(this.randomize_min, this.randomize_max) * this.maxt;
		}

		public void Clear()
		{
			this.ClearValues();
			this.shuffle();
		}

		public void ClearSampleValues()
		{
			this.AsamplePos.Clear();
			this.sample_t = 0f;
			this.Sum = Vector2.zero;
		}

		public void ClearValues()
		{
			this.stasis_t = 0f;
			this.ClearSampleValues();
		}

		public void run(float fcnt)
		{
			float num = 1f;
			bool flag = false;
			NAI nai = base.Nai;
			if (nai == null)
			{
				return;
			}
			if (nai.isPrAttacking(1f))
			{
				flag = true;
				num *= this.reduce_ratio_attack;
			}
			else if (nai.isPrGaraakiState() || nai.isPrMagicChanting(-1f) || nai.isPrGacharingFrozen())
			{
				num = this.ratio_garaaki;
			}
			else
			{
				this.sample_t += fcnt;
				if (this.sample_t < 1f)
				{
					return;
				}
				this.sample_t -= 1f;
				Vector2 vector = new Vector2(base.target_x, base.target_y);
				this.Sum += vector;
				this.AsamplePos.Add(vector);
				if (this.AsamplePos.Count < this.sample_interval_)
				{
					return;
				}
				this.Sum *= ((this.AsamplePos.Count == this.sample_interval_) ? this.sample_interval_r : (1f / (float)this.AsamplePos.Count));
				float num2 = 0f;
				fcnt *= (float)this.AsamplePos.Count;
				for (int i = this.AsamplePos.Count - 1; i >= 0; i--)
				{
					Vector2 vector2 = this.AsamplePos[i];
					float num3 = X.LENGTHXYS(this.Sum.x, this.Sum.y, vector2.x, vector2.y);
					num2 += X.Mx(0f, num3 - 0.25f);
				}
				this.ClearSampleValues();
				if (num2 >= (float)this.sample_interval_ * 0.4f)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				this.stasis_t += fcnt;
				return;
			}
			this.stasis_t = X.Mx(this.stasis_t - fcnt, 0f);
		}

		public bool isStasis(bool clear_if_true)
		{
			bool flag = this.stasis_t >= this.maxt_current;
			if (flag && clear_if_true)
			{
				this.Clear();
			}
			return flag;
		}

		private float stasis_t;

		public float maxt;

		public float maxt_current;

		private int sample_interval_ = 10;

		private float sample_interval_r;

		private float sample_t;

		public float randomize_min = 0.88f;

		public float randomize_max = 1.2f;

		public float ratio_garaaki = 2f;

		public float reduce_ratio_attack = 2f;

		private List<Vector2> AsamplePos;

		private Vector2 Sum;
	}
}
