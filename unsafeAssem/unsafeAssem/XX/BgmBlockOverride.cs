using System;
using Better;

namespace XX
{
	public class BgmBlockOverride
	{
		public BgmBlockOverride(string _key, int capacity)
		{
			this.key = _key;
			this.Osrc = new BDic<int, int>(capacity);
		}

		public void Add(char _src, char _dest)
		{
			this.Osrc[(int)(_src - 'A')] = (int)(_dest - 'A');
		}

		public readonly BDic<int, int> Osrc;

		public readonly string key;
	}
}
