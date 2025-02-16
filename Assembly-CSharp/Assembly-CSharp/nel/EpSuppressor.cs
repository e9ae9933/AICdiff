using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class EpSuppressor
	{
		public bool Orgasmed(EPCATEG categ, int delay)
		{
			float num = (float)categ;
			for (int i = this.ASp.Count - 1; i >= 0; i--)
			{
				Vector2 vector = this.ASp[i];
				if (vector[0] == num)
				{
					vector[1] = (float)delay;
				}
			}
			if (this.Acount[(int)categ] < 10)
			{
				byte[] acount = this.Acount;
				acount[(int)categ] = acount[(int)categ] + 1;
				this.sum++;
				this.ASp.Add(new Vector2(num, (float)delay));
				return true;
			}
			return false;
		}

		public int Count
		{
			get
			{
				return this.ASp.Count;
			}
		}

		public void run(float fcnt)
		{
			for (int i = this.ASp.Count - 1; i >= 0; i--)
			{
				Vector2 vector = this.ASp[i];
				vector.y -= fcnt;
				if (vector.y <= 0f)
				{
					byte[] acount = this.Acount;
					int num = (int)vector.x;
					acount[num] -= 1;
					this.sum--;
					this.ASp.RemoveAt(i);
				}
			}
		}

		public int Sum()
		{
			return this.sum;
		}

		public float getOrgasmableRatio(EPCATEG categ)
		{
			return X.Pow(0.33f, (int)this.Acount[(int)categ]);
		}

		public bool Has(EPCATEG categ)
		{
			return this.Acount[(int)categ] > 0;
		}

		public int getCount(EPCATEG categ)
		{
			return (int)this.Acount[(int)categ];
		}

		public void Clear()
		{
			this.ASp.Clear();
			X.ALL0(this.Acount);
			this.sum = 0;
		}

		private List<Vector2> ASp = new List<Vector2>(4);

		private byte[] Acount = new byte[11];

		private int sum;
	}
}
