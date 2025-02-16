using System;

namespace XX
{
	public class DsnDataNumCounter : DsnDataBtnBase
	{
		public int def;

		public bool locked;

		public string skin = "normal";

		public float w;

		public float h;

		public int navi_loop;

		public int minval;

		public int maxval = 999;

		public int digit;

		public bool slide_cur_digit_only;

		public FnBtnBindings fnClick;

		public BtnContainer<aBtnNumCounter>.FnBtnMakingBindings fnMaking;
	}
}
