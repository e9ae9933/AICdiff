using System;
using System.Collections.Generic;

namespace XX
{
	public class DsnDataRadio : DsnDataBtnBase
	{
		public bool callBasic(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			return this.fnChanged(_B, pre_value, cur_value);
		}

		public DsnDataRadio RowMode(string _skin)
		{
			this.skin = _skin;
			this.margin_h = (this.margin_w = 0);
			this.clms = 1;
			this.def = -1;
			return this;
		}

		public int def;

		public string def_key;

		public string skin = "radio_string";

		public string[] keys;

		public List<string> keysL;

		public string[] descs;

		public float w = 140f;

		public float h = 24f;

		public int clms;

		public float scale = 1f;

		public int margin_w = 30;

		public int margin_h = 18;

		public bool value_return_name;

		public bool all_function_same;

		public int navi_loop;

		public List<aBtn> APoolEvacuated;

		public BtnContainerRadio<aBtn>.FnRadioBindings fnChanged;

		public FnBtnBindings fnClick;

		public FnBtnBindings fnHover;

		public FnBtnBindings fnOut;

		public BtnContainer<aBtn>.FnBtnMakingBindings fnMaking;

		public BtnContainer<aBtn>.FnBtnMakingBindings fnMakingAfter;

		public FnGenerateRemakeKeys fnGenerateKeys;

		public BtnContainerRadio<aBtn>.FnCreateContainer fnCreateContainer;

		public ScrollAppend SCA;
	}
}
