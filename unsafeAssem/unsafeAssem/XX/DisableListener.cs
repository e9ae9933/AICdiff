using System;
using UnityEngine;

namespace XX
{
	public class DisableListener : MonoBehaviour
	{
		public void OnEnable()
		{
			X.dl("enabled: " + base.gameObject.name, null, false, false);
		}

		public void OnDisable()
		{
			X.dl("disabled: " + base.gameObject.name, null, false, false);
		}
	}
}
