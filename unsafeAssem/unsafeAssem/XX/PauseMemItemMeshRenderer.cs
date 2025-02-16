using System;
using UnityEngine;

namespace XX
{
	public class PauseMemItemMeshRenderer : PauseMemItem
	{
		public PauseMemItemMeshRenderer(MeshRenderer _Mr)
		{
			this.Mr = _Mr;
		}

		public override void Pause()
		{
			try
			{
				this.Mr.enabled = false;
			}
			catch
			{
			}
		}

		public override void Resume()
		{
			try
			{
				this.Mr.enabled = true;
			}
			catch
			{
			}
		}

		private MeshRenderer Mr;
	}
}
