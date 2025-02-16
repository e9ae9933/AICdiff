using System;

namespace nel
{
	public interface IGachaListener
	{
		bool canAbsorbContinue();

		void absorbFinished(bool abort);

		bool individual { get; }
	}
}
