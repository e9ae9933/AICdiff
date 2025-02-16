using System;
using m2d;
using UnityEngine;

namespace nel
{
	public class SandBgm : MonoBehaviour
	{
		public void Start()
		{
		}

		public void Update()
		{
			if (this.t == 0)
			{
				if (MTR.preparedG)
				{
					this.t++;
					return;
				}
			}
			else
			{
				if (this.t == 1 && M2DBase.Instance != null && M2DBase.Instance.curMap != null)
				{
					this.t++;
					return;
				}
				if (this.t == 2)
				{
					Object.Destroy(this);
				}
			}
		}

		private int t;
	}
}
