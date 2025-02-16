using System;
using System.Collections.Generic;

namespace XX
{
	public class DsnDataButtonMulti : DsnDataBtnBase
	{
		public int def;

		public int locked;

		public string skin = "normal";

		public string[] titles;

		public List<string> titlesL;

		public string[] skin_title;

		public string[] descs;

		public float w;

		public float h;

		public int clms;

		public float margin_w = 30f;

		public float margin_h = 18f;

		public int navi_loop;

		public FnBtnBindings fnClick;

		public FnBtnBindings fnDown;

		public FnBtnBindings fnHover;

		public FnBtnBindings fnOut;

		public FnGenerateRemakeKeys fnGenerateKeys;

		public BtnContainer<aBtn>.FnBtnMakingBindings fnMaking;
	}
}
