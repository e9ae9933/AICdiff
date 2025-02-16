using System;
using XX;

namespace nel
{
	public class aBtnNelArrowOvr : aBtnNel
	{
		protected bool runFnDir(aBtnNelArrowOvr.FnBtnBindingArrowOvr[] AFn, AIM a)
		{
			if (base.destructed || AFn == null)
			{
				return false;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				aBtnNelArrowOvr.FnBtnBindingArrowOvr fnBtnBindingArrowOvr = AFn[i];
				if (fnBtnBindingArrowOvr == null)
				{
					return flag;
				}
				flag = fnBtnBindingArrowOvr(this, a) && flag;
			}
			return flag;
		}

		public aBtnNelArrowOvr addFnDir(aBtnNelArrowOvr.FnBtnBindingArrowOvr Fn)
		{
			aBtn.addFnT<aBtnNelArrowOvr.FnBtnBindingArrowOvr>(ref this.AFnDir, Fn);
			return this;
		}

		protected override void simulateNaviTranslation(int aim = -1)
		{
			if ((aim == 0 || IN.isL()) && this.runFnDir(this.AFnDir, AIM.L))
			{
				return;
			}
			if ((aim == 2 || IN.isR()) && this.runFnDir(this.AFnDir, AIM.R))
			{
				return;
			}
			if ((aim == 1 || IN.isT()) && this.runFnDir(this.AFnDir, AIM.T))
			{
				return;
			}
			if ((aim == 3 || IN.isB()) && this.runFnDir(this.AFnDir, AIM.B))
			{
				return;
			}
			base.simulateNaviTranslation(aim);
		}

		private aBtnNelArrowOvr.FnBtnBindingArrowOvr[] AFnDir;

		public delegate bool FnBtnBindingArrowOvr(aBtn _B, AIM a);
	}
}
