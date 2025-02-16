using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class SandSlider : MonoBehaviour
	{
		public void Start()
		{
		}

		public void Update()
		{
			if (this.t == 0)
			{
				if (MTR.preparedT && M2DBase.Instance != null)
				{
					this.t++;
					this.Btn = new GameObject("otumami").AddComponent<aBtnMeter>();
					this.Btn.initMeter(0f, 1f, 1f, 0f, 80f);
					this.Btn.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
					IN.PosP(this.Btn.transform, -IN.wh + 450f, 100f, -7f);
					this.Btn.addChangedFn(new aBtnMeter.FnMeterBindings(this.fnChangeMeter));
					this.M2D = M2DBase.Instance as NelM2DBase;
					return;
				}
			}
			else
			{
				int num = this.t;
			}
		}

		private bool fnChangeMeter(aBtnMeter _B, float pre_value, float cur_value)
		{
			if (cur_value == 1f)
			{
				BGM.addBattleTransition("DEBUG");
			}
			else if (cur_value == 0f)
			{
				BGM.remBattleTransition("DEBUG");
			}
			return true;
		}

		private int t;

		private aBtnMeter Btn;

		private NelM2DBase M2D;
	}
}
