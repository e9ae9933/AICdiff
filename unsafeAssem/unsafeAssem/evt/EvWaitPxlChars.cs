using System;
using PixelLiner;

namespace evt
{
	public class EvWaitPxlChars : IEventWaitListener
	{
		public bool EvtWait(bool is_first = false)
		{
			return is_first || !PxlsLoader.isLoadCompletedAll();
		}
	}
}
