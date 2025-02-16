using System;
using UnityEngine;
using XX;

namespace nel
{
	public interface ICheckPointListener
	{
		void returnChcekPoint(PR Pr);

		int getCheckPointPriority();

		Color32 getDrawEffectPositionAndColor(ref int pixel_x, ref int pixel_y);

		void activateCheckPoint();

		bool drawCheckPoint(EffectItem Ef, float pixe_x, float pixel_y, Color32 Col);

		Vector2 getReturnPos();
	}
}
