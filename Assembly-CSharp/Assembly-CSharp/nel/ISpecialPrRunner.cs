using System;

namespace nel
{
	public interface ISpecialPrRunner
	{
		bool runPreSPPR(PR Pr, float fcnt, ref float t);

		void quitSPPR(PR Pr, PR.STATE aftstate);
	}
}
