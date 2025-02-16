using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class DebugLogBlockBase : MonoBehaviour, IDesignerBlock
	{
		public bool isActive()
		{
			return false;
		}

		public float get_swidth_px()
		{
			return 1f;
		}

		public float get_sheight_px()
		{
			return 1f;
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
		}

		public Transform getTransform()
		{
			return base.transform;
		}
	}
}
