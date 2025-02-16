using System;
using nel.gm;
using UnityEngine;
using XX;

namespace nel
{
	public class SandGM : MonoBehaviour
	{
		public void Start()
		{
		}

		public void Update()
		{
			if (this.t == 0)
			{
				if (MTRX.prepared)
				{
					this.t++;
					X.dl("prepared", null, false, false);
					this.GM = new GameObject("GM").AddComponent<UiGameMenu>();
					return;
				}
			}
			else if (this.t >= 1 && IN.isMenuPD(1))
			{
				if (this.GM.isActive())
				{
					this.GM.deactivate(false);
					return;
				}
				this.GM.activate();
			}
		}

		private int t;

		private UiGameMenu GM;
	}
}
