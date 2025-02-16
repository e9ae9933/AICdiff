using System;
using System.Collections.Generic;

namespace XX
{
	public class DsnDataChecks : DsnDataBtnBase
	{
		public int def;

		public string skin = "checkbox_string";

		public string[] keys;

		public List<string> keysL;

		public string[] descs;

		public float w = 140f;

		public float h = 24f;

		public float scale = 1f;

		public int clms = 1;

		public int margin_w = 30;

		public int margin_h = 18;

		public int navi_loop;

		public FnBtnBindings fnClick;

		public FnBtnBindings fnHover;

		public FnGenerateRemakeKeys fnGenerateKeys;

		public BtnContainer<aBtn>.FnBtnMakingBindings fnMaking;
	}
}
