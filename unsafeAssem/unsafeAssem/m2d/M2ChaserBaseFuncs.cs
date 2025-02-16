using System;

namespace m2d
{
	public abstract class M2ChaserBaseFuncs
	{
		public M2ChaserBaseFuncs(M2Mover _Mv)
		{
			this.Mp = _Mv.Mp;
			this.Mv = _Mv;
		}

		public abstract bool reset();

		public abstract M2ChaserBaseFuncs.CHSRES chaseProgress();

		public abstract bool hasDestPoint();

		public readonly Map2d Mp;

		public readonly M2Mover Mv;

		public enum CHSRES
		{
			ERROR = -1,
			FINDING,
			FOUND,
			PROGRESS,
			REACHED
		}
	}
}
