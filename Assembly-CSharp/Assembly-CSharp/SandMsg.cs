using System;
using evt;
using UnityEngine;
using XX;

public class SandMsg : MonoBehaviour
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
				EV.loadEV();
				this.t++;
				return;
			}
		}
		else if (this.t == 1 && EV.material_prepared)
		{
			EV.initEvent(null, null, null);
			EV.stack("_DEBUG01", 0, -1, null, null);
			this.t++;
		}
	}

	private int t;
}
