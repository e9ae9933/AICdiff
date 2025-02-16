using System;
using XX;

namespace evt
{
	public interface IEventListener
	{
		bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0);

		bool EvtOpen(bool is_first_or_end);

		bool EvtClose(bool is_first_or_end);

		int EvtCacheRead(EvReader ER, string cmd, CsvReader rER);

		bool EvtMoveCheck();
	}
}
