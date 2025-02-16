using System;
using UnityEngine;

namespace nel
{
	public interface IItemSupplier
	{
		NelItemEntry[] getDataList(ref bool is_reel);

		Vector4 getShowPos();
	}
}
