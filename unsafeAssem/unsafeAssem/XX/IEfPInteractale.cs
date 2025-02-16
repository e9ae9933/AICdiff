using System;
using UnityEngine;

namespace XX
{
	public interface IEfPInteractale
	{
		bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V);

		bool readPtcScript(PTCThread St);

		string getSoundKey();

		bool isSoundActive(SndPlayer S);

		bool initSetEffect(PTCThread Thread, EffectItem Ef);
	}
}
