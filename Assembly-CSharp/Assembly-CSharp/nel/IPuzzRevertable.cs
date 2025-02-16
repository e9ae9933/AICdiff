using System;

namespace nel
{
	public interface IPuzzRevertable
	{
		void makeSnapShot(PuzzSnapShot.RevertItem Rvi);

		void puzzleRevert(PuzzSnapShot.RevertItem Rvi);
	}
}
