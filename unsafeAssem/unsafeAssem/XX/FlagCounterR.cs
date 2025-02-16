using System;
using System.Collections.Generic;
using Better;

namespace XX
{
	public sealed class FlagCounterR<T> : FlagCounter<T>
	{
		public FlagCounterR<T> AddS(T val, float maxt = 180f)
		{
			base.Add(val, maxt);
			return this;
		}

		public void run(float fcnt)
		{
			this.OItemS.Clear();
			foreach (KeyValuePair<T, float> keyValuePair in this.OItem)
			{
				float num = keyValuePair.Value;
				if (num < 0f)
				{
					this.OItemS[keyValuePair.Key] = num;
				}
				else
				{
					num -= fcnt;
					if (num > 0f)
					{
						this.OItemS[keyValuePair.Key] = num;
					}
				}
			}
			BDic<T, float> oitem = this.OItem;
			this.OItem = this.OItemS;
			this.OItemS = oitem;
		}

		public FlagCounterR()
			: base(4)
		{
		}

		private BDic<T, float> OItemS = new BDic<T, float>();
	}
}
