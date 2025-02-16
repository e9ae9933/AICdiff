using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public interface IPresserBehaviour
	{
		int getPressAim(M2Mover Mv);

		List<M2BlockColliderContainer.BCCLine> getPressableBccLineNear(float mapx, float mapy, float sizex, float sizey, AIM a, List<M2BlockColliderContainer.BCCLine> ARet);

		bool publishPress(M2Attackable MvTarget, List<M2BlockColliderContainer.BCCLine> ATargetBcc, out bool stop_carrier);

		Vector2 getTranslatedDelta();
	}
}
