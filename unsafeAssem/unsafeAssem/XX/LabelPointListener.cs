using System;
using System.Collections.Generic;
using Better;

namespace XX
{
	public class LabelPointListener<T> where T : DRect
	{
		public LabelPointListener()
		{
			this.Aitrk = null;
			this.OStd = new BDic<string, T>();
			this.OStdP = new BDic<string, T>();
			this.OEnt = new BDic<string, T>();
			this.OOut = new BDic<string, T>();
		}

		internal void findStart()
		{
			this.OStdP = this.OStd;
			this.OStd = new BDic<string, T>();
			this.OEnt = new BDic<string, T>();
			this.OOut = new BDic<string, T>();
		}

		internal void addStanding(T P)
		{
			this.OStd[P.key] = P;
		}

		internal void findEnd(T _nearestP)
		{
			this.nearestP = _nearestP;
			foreach (KeyValuePair<string, T> keyValuePair in this.OStd)
			{
				string key = keyValuePair.Key;
				if (!this.OStdP.ContainsKey(key))
				{
					this.OEnt[key] = keyValuePair.Value;
				}
			}
			foreach (KeyValuePair<string, T> keyValuePair2 in this.OStdP)
			{
				string key2 = keyValuePair2.Key;
				if (!this.OStd.ContainsKey(key2))
				{
					this.OOut[key2] = keyValuePair2.Value;
				}
			}
		}

		public T getNearest()
		{
			return this.nearestP;
		}

		public void beginIterator(BDic<string, T> O, string[] Akeys = null)
		{
			this.Oitr = O;
			if (this.Oitr == null)
			{
				this.itri = (this.itrl = -1);
				return;
			}
			if (Akeys != null)
			{
				this.Aitrk = X.concat<string>(Akeys, null, -1, -1);
			}
			else
			{
				this.Aitrk = X.objKeys<string, T>(this.Oitr);
			}
			this.itri = -1;
			this.itrl = X.countNotEmpty<string>(this.Aitrk);
		}

		public void beginEnter()
		{
			this.beginIterator(this.OEnt, null);
		}

		public void beginStand()
		{
			this.beginIterator(this.OStd, null);
		}

		public void beginOut()
		{
			this.beginIterator(this.OOut, null);
		}

		public bool next()
		{
			int num = this.itri + 1;
			this.itri = num;
			if (num >= this.itrl)
			{
				this.Oitr = null;
				this.Aitrk = null;
				this.itri = -1;
				return false;
			}
			return true;
		}

		public T cur
		{
			get
			{
				if (this.Oitr == null)
				{
					return default(T);
				}
				return this.Oitr[this.Aitrk[this.itri]];
			}
		}

		private BDic<string, T> OStd;

		private BDic<string, T> OStdP;

		private BDic<string, T> OEnt;

		private BDic<string, T> OOut;

		private T nearestP;

		private BDic<string, T> Oitr;

		private string[] Aitrk;

		private int itri = -1;

		private int itrl;
	}
}
