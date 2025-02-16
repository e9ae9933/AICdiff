using System;

namespace nel
{
	public interface ITrmListener
	{
		TRMManager.TRMItem CurrentTargetTrmItem { get; }
	}
}
