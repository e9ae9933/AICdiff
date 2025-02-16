using System;
using System.Collections.Generic;

namespace XX
{
	public class BgmTransitionPoint
	{
		public BgmTransitionPoint()
		{
			this.AItm = new List<BgmTransitionPoint.BgmTransData>();
		}

		public int Set(string[] _AFrom, string[] _ATo, int _fade_millisec_a, int _fade_millisec_b)
		{
			byte[] array = new byte[_AFrom.Length];
			byte[] array2 = new byte[_ATo.Length];
			int num = array.Length;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				int num3 = (int)(_AFrom[i].ToUpper().ToCharArray()[0] - 'A');
				array[i] = (byte)num3;
				num2 = X.Mx(num3, num2);
			}
			num = array2.Length;
			for (int j = 0; j < num; j++)
			{
				array2[j] = (byte)(_ATo[j].ToUpper().ToCharArray()[0] - 'A');
			}
			this.AItm.Add(new BgmTransitionPoint.BgmTransData
			{
				block_from = array,
				block_to = array2,
				fade_millisec_a = _fade_millisec_a,
				fade_millisec_b = _fade_millisec_b
			});
			return num2;
		}

		public BgmTransitionPoint.BgmTransData GetDepBlock(byte frm_blk)
		{
			int count = this.AItm.Count;
			for (int i = 0; i < count; i++)
			{
				if (X.isinS<byte>(this.AItm[i].block_from, frm_blk) >= 0)
				{
					return this.AItm[i];
				}
			}
			return null;
		}

		public List<BgmTransitionPoint.BgmTransData> AItm;

		public class BgmTransData
		{
			public byte[] block_from;

			public byte[] block_to;

			public int fade_millisec_a;

			public int fade_millisec_b;
		}
	}
}
