using System;

namespace nel
{
	public abstract class M2PrARunner : M2PrAssistant, ISpecialPrRunner
	{
		public M2PrARunner(PR _Pr)
			: base(_Pr)
		{
		}

		public abstract bool runPreSPPR(PR Pr, float fcnt, ref float t);

		public abstract void quitSPPR(PR Pr, PR.STATE aftstate);

		public virtual M2PrARunner activate()
		{
			return this;
		}
	}
}
