using System;

namespace nel
{
	public interface IIrisOutListener
	{
		IrisOutManager.IRISOUT_TYPE getIrisOutKey();

		bool initSubmitIrisOut(PR Pr, bool execute, ref bool no_iris_out);

		bool warpInitIrisOut(PR Pr, ref PR.STATE changestate, ref string change_pose);
	}
}
