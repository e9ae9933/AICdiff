using System;
using UnityEngine;

namespace XX
{
	public class PauseMemItemGameObject : PauseMemItem
	{
		public PauseMemItemGameObject(GameObject _Gob)
		{
			this.Gob = _Gob;
		}

		public override void Pause()
		{
			this.Gob.SetActive(false);
		}

		public override void Resume()
		{
			this.Gob.SetActive(true);
		}

		private GameObject Gob;
	}
}
