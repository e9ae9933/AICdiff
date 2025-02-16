using System;
using UnityEngine;

namespace m2d
{
	public interface IMapChipEffectSetter
	{
		void setGraphicMatrix(Matrix4x4 Mx);

		void addSubMapBinder(M2SubMap.EfSubMapEffectBinder Bd);
	}
}
