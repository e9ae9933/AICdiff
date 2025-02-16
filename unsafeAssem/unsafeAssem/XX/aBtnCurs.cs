using System;
using UnityEngine;

namespace XX
{
	public class aBtnCurs : aBtn
	{
		public override void OnPointerEnter()
		{
			base.OnPointerEnter();
			if (base.isMouseOver())
			{
				aBtnCurs.PreMs = IN.getMouseScreenPos();
				if (this.aFnMove != null)
				{
					base.runFn(this.aFnMove);
				}
			}
		}

		public aBtn addMoveFn(FnBtnBindings Fn)
		{
			return base.addFn(ref this.aFnMove, Fn);
		}

		public override bool run(float fcnt)
		{
			if (base.isMouseOver())
			{
				Vector3 vector = IN.getMouseScreenPos();
				Vector3 preMs = aBtnCurs.PreMs;
				if ((vector.x != preMs.x || vector.y != preMs.y) && this.aFnMove != null)
				{
					base.runFn(this.aFnMove);
				}
				aBtnCurs.PreMs = vector;
			}
			return base.run(fcnt);
		}

		private FnBtnBindings[] aFnMove;

		private static Vector3 PreMs;
	}
}
