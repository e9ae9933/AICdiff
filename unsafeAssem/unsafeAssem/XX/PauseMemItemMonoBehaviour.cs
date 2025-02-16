using System;
using UnityEngine;

namespace XX
{
	public class PauseMemItemMonoBehaviour : PauseMemItem
	{
		public override string ToString()
		{
			return this.Mb.ToString();
		}

		public PauseMemItemMonoBehaviour(MonoBehaviour _Mb)
		{
			this.Mb = _Mb;
		}

		public override void Pause()
		{
			this.Mb.enabled = false;
		}

		public override void Resume()
		{
			this.Mb.enabled = true;
		}

		private MonoBehaviour Mb;
	}
}
