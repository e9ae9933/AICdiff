using System;

namespace evt
{
	public interface IEventWaitListener
	{
		bool EvtWait(bool is_first = false);
	}
}
