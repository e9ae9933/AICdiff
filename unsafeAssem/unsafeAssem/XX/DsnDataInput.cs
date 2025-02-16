using System;
using System.Text.RegularExpressions;

namespace XX
{
	public class DsnDataInput : DsnDataBtnBase
	{
		public DsnDataInput()
		{
			this.z_push_click = true;
		}

		public DsnDataInput LNW(string _label = null, string _name = null, int _bounds_w = 0)
		{
			if (_label != null)
			{
				this.label = _label;
			}
			if (_name != null)
			{
				this.name = _name;
			}
			if (_bounds_w != 0)
			{
				this.bounds_w = (float)_bounds_w;
			}
			return this;
		}

		public string def = "";

		public string label = "";

		public string skin = "";

		public float w;

		public float bounds_w;

		public int size;

		public float h;

		public Regex alloc_char;

		public int changed_delay_maxt = 60;

		public int multi_line = 1;

		public bool label_top;

		public int desc_aim_bit;

		public bool integer;

		public bool hex_integer;

		public bool return_blur = true;

		public bool number;

		public bool editable = true;

		public bool alloc_empty = true;

		public int max_len = -1;

		public double min = -2147483648.0;

		public double max = 2147483647.0;

		public FnFldBindings fnChanged;

		public FnFldBindings fnChangedDelay;

		public FnFldBindings fnFocus;

		public FnFldBindings fnBlur;

		public FnFldBindings fnReturn;

		public FnFldKeyInputBindings fnInputtingKeyDown;
	}
}
