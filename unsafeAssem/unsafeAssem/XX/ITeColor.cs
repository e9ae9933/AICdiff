using System;
using UnityEngine;

namespace XX
{
	public interface ITeColor
	{
		Color32 getColorTe();

		void setColorTe(C32 Buf, C32 CMul, C32 CAdd);
	}
}
