using System;
using System.Collections.Generic;

namespace nel.mgm.smncr
{
	public class SmncFileContainer : List<SmncFile>
	{
		public SmncFileContainer(int capacity)
			: base(capacity)
		{
		}

		public byte first_file;
	}
}
