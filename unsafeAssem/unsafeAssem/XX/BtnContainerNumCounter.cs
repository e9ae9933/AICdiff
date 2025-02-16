using System;
using UnityEngine;

namespace XX
{
	public class BtnContainerNumCounter<T> : BtnContainer<T>, INumCounterFuncs where T : aBtnNumCounter
	{
		public bool slide_cur_digit_only { get; set; }

		public BtnContainerNumCounter(GameObject _Base, int default_val = 0, int alloc_size = 0)
			: base(_Base, alloc_size)
		{
			this.cnt_val = default_val;
			this.default_w = 24f;
			this.default_h = 48f;
		}

		public ObjCarrierConBlockBtnContainer<T> initNumCounter(GameObject Gob, int _minval, int _maxval, int _defval, int min_digit = 0)
		{
			this.minval = _minval;
			this.maxval = _maxval;
			int i = 10;
			this.digit = 1;
			while (i <= this.maxval)
			{
				i *= 10;
				this.digit++;
			}
			this.digit = X.Mx(min_digit, this.digit);
			ObjCarrierConBlockBtnContainer<T> objCarrierConBlockBtnContainer = new ObjCarrierConBlockBtnContainer<T>(this, Gob.transform);
			objCarrierConBlockBtnContainer.BoundsPx(this.default_w * (float)(this.digit - 1), 0f, this.digit);
			objCarrierConBlockBtnContainer.ItemSizePx(this.default_w, this.default_h, this.digit);
			base.setCarrierContainer(objCarrierConBlockBtnContainer, false);
			this.cnt_val = _defval;
			return objCarrierConBlockBtnContainer;
		}

		public void RemakeDigit(string skin = "")
		{
			this.RemakeDigitT<T>(skin);
		}

		public void RemakeDigitT<T2>(string skin = "") where T2 : T
		{
			string[] array = X.makeToStringed<int>(X.makeCountUpArray(this.digit, 0, 1));
			base.RemakeT<T2>(array, skin);
			this.setValue(this.cnt_val);
		}

		public void setValue(int _val)
		{
			uint num = 0U;
			for (int i = this.Length - 1; i >= 0; i--)
			{
				num |= (base.Get(i).unuse_digit ? (1U << i) : 0U);
			}
			_val = X.MMX(this.minval, _val, this.maxval);
			bool flag = this.cnt_val < _val;
			this.cnt_val = _val;
			int num2 = -1;
			for (int j = this.Length - 1; j >= 0; j--)
			{
				int num3 = _val % 10;
				aBtnNumCounter aBtnNumCounter = base.Get(j);
				bool flag2 = (num & (1U << j)) > 0U;
				aBtnNumCounter.setValue(num3, flag);
				if (flag2 != aBtnNumCounter.unuse_digit)
				{
					num2 = j;
				}
				_val /= 10;
			}
			if (num2 >= 0)
			{
				for (int k = num2 + 1; k < this.Length; k++)
				{
					base.Get(k).Fine(false);
				}
			}
		}

		public int getDigitValue()
		{
			return this.cnt_val;
		}

		public int getMaximumValue()
		{
			return this.maxval;
		}

		public int getMinimumValue()
		{
			return this.minval;
		}

		public override string getValueString()
		{
			return this.cnt_val.ToString();
		}

		public void playSlideSound(bool to_upper)
		{
			SND.Ui.play(to_upper ? this.slide_sound_top : this.slide_sound_bottom, false);
		}

		public override void setValue(string s)
		{
			STB stb = TX.PopBld(s, 0);
			int num;
			this.setValue(stb.NmI(0, out num, -1, 0));
			TX.ReleaseBld(stb);
		}

		public string slide_sound_top = "toggle_button_open";

		public string slide_sound_bottom = "toggle_button_close";

		private int cnt_val;

		private int digit;

		public int minval;

		public int maxval;
	}
}
