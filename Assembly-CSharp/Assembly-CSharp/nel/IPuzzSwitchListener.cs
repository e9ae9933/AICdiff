using System;

namespace nel
{
	public interface IPuzzSwitchListener
	{
		void changePuzzleSwitchActivation(int id, bool activated);
	}
}
