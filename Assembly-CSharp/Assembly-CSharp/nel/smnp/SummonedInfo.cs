using System;
using UnityEngine;

namespace nel.smnp
{
	public sealed class SummonedInfo
	{
		public SummonedInfo(SmnEnemyKind _K, Vector2 _FirstPos, SmnPoint _PosInfo)
		{
			this.K = _K;
			this.FirstPos = _FirstPos;
			this.PosInfo = _PosInfo;
		}

		public SmnEnemyKind K;

		public Vector2 FirstPos;

		public SmnPoint PosInfo;
	}
}
