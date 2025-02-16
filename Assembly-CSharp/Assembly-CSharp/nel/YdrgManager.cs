using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel
{
	public class YdrgManager : IRunAndDestroy
	{
		public YdrgManager(PR _Pr)
		{
			this.OLsn = new BDic<NelEnemy, YdrgManager.YdrgListener>();
			this.Pr = _Pr;
		}

		public void clear()
		{
			this.max_lv_ = 0;
			this.OLsn.Clear();
		}

		public bool alreadyAttached(NelEnemy En)
		{
			return this.OLsn.ContainsKey(En);
		}

		public void Attach(NelEnemy En, YdrgManager.FnYdrgApplyDamage Fn, int thresh_lv1 = -1, int thresh_lv2 = -1)
		{
			if (!this.OLsn.ContainsKey(En))
			{
				if (this.OLsn.Count == 0)
				{
					this.Pr.Ser.Add(SER.PARASITISED, -1, 0, false);
				}
				this.OLsn[En] = new YdrgManager.YdrgListener(Fn);
			}
			YdrgManager.YdrgListener ydrgListener = this.OLsn[En];
			if (ydrgListener.attach_t < 260f)
			{
				ydrgListener.attach_count++;
				bool flag = false;
				if (ydrgListener.level == 0 && thresh_lv1 >= 0 && ydrgListener.attach_count >= thresh_lv1)
				{
					ydrgListener.level = 1;
					flag = true;
				}
				if (ydrgListener.level < 2 && thresh_lv2 >= 0 && ydrgListener.attach_count >= thresh_lv2)
				{
					ydrgListener.level = 2;
					flag = true;
				}
				if (flag)
				{
					this.fineLevel(true);
				}
			}
			ydrgListener.attach_t = 0f;
		}

		public void fineLevel(bool resetting = false)
		{
			if (resetting)
			{
				this.max_lv_ = -1;
			}
			int max_lv = this.max_lv;
			if (max_lv >= 0)
			{
				this.Pr.Ser.Add(SER.PARASITISED, -1, max_lv, false);
			}
		}

		public bool run(float fcnt)
		{
			List<NelEnemy> list = null;
			foreach (KeyValuePair<NelEnemy, YdrgManager.YdrgListener> keyValuePair in this.OLsn)
			{
				NelEnemy key = keyValuePair.Key;
				if (key == null || !key.is_alive)
				{
					if (list == null)
					{
						list = new List<NelEnemy>(1);
					}
					list.Add(key);
					this.max_lv_ = -1;
				}
				else
				{
					YdrgManager.YdrgListener value = keyValuePair.Value;
					if (value.attach_t < 260f)
					{
						value.attach_t += fcnt;
					}
					value.t += fcnt;
					if (value.t >= (float)YdrgManager.Aintv[value.level] && value.FnApplyDamage(key, this.Pr, value.level))
					{
						this.Pr.PtcVar("agR", (double)this.Mp.GAR(this.Pr.x, this.Pr.y, key.x, key.y)).PtcST("dmg_yadorigi", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Pr.TeCon.setDmgBlink(MGATTR.POISON, 30f, 0.6f, 0.6f, 0);
						value.t = 0f;
					}
				}
			}
			if (list != null)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					this.OLsn.Remove(list[i]);
				}
			}
			return this.OLsn.Count > 0;
		}

		public void destruct()
		{
			this.clear();
		}

		public Map2d Mp
		{
			get
			{
				return this.Pr.Mp;
			}
		}

		public int max_lv
		{
			get
			{
				if (this.max_lv_ == -1)
				{
					this.max_lv_ = 0;
					foreach (KeyValuePair<NelEnemy, YdrgManager.YdrgListener> keyValuePair in this.OLsn)
					{
						this.max_lv_ = X.Mx(this.max_lv_, keyValuePair.Value.level);
					}
				}
				return this.max_lv_;
			}
		}

		public const int LEVEL_MAX = 3;

		private static readonly int[] Aintv = new int[] { 180, 135, 90 };

		private const int ATTACH_DELAY = 260;

		private int max_lv_ = -1;

		private BDic<NelEnemy, YdrgManager.YdrgListener> OLsn;

		public readonly PR Pr;

		public delegate bool FnYdrgApplyDamage(NelEnemy ApplyFrom, PR ApplyTo, int level);

		public class YdrgListener
		{
			public YdrgListener(YdrgManager.FnYdrgApplyDamage Fn)
			{
				this.FnApplyDamage = Fn;
			}

			public float t;

			public float attach_t;

			public int level;

			public int attach_count = -1;

			public readonly YdrgManager.FnYdrgApplyDamage FnApplyDamage;
		}
	}
}
