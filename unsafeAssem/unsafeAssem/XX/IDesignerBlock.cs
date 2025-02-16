using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public interface IDesignerBlock
	{
		float get_swidth_px();

		float get_sheight_px();

		void AddSelectableItems(List<aBtn> ASlc, bool only_front);

		Transform getTransform();
	}
}
