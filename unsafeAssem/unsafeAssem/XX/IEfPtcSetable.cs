using System;

namespace XX
{
	public interface IEfPtcSetable : IEfPInteractale
	{
		IEfPtcSetable PtcVar(string key, float v);

		IEfPtcSetable PtcVarS(string key, string v);

		IEfPtcSetable PtcVarS(string key, MGATTR v);

		PTCThread PtcST(string ptcst_name, IEfPInteractale Listener = null, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW);
	}
}
