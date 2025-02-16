using System;

namespace XX
{
	public class DsnDataSlider : DsnDataBtnBase
	{
		public float def;

		public string skin = "normal";

		public string title = "";

		public string skin_title;

		public float mn;

		public float mx = 1f;

		public float valintv = 1f;

		public float w = 160f;

		public float h = 28f;

		public bool submit_holding;

		public bool lr_reverse;

		public aBtnMeter.FnMeterBindings fnChanged;

		public FnBtnMeterLine fnBtnMeterLine;

		public FnBtnBindings fnHover;

		public FnBtnBindings fnOut;

		public FnDescConvert fnDescConvert;

		public FnBtnBindings fnClick;

		public byte checkbox_mode;

		public string[] Adesc_keys;
	}
}
