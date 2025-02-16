using System;
using UnityEngine.InputSystem;

namespace XX
{
	public sealed class DsPrmpRunner : HideScreen
	{
		protected override bool runIRD(float fcnt)
		{
			bool flag = base.runIRD(fcnt);
			if (this.disable_next_frame)
			{
				this.deactivate(false);
				this.disable_next_frame = false;
				return flag;
			}
			if (IN.getKD(Key.Enter, -1))
			{
				DsPrmp.doneFront();
			}
			if (IN.getKD(Key.Escape, -1))
			{
				DsPrmp.cancelFront();
			}
			return flag;
		}

		public bool disable_next_frame;
	}
}
