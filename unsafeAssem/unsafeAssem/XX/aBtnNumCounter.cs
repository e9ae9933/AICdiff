using System;

namespace XX
{
	public class aBtnNumCounter : aBtn
	{
		public override ButtonSkin makeButtonSkin(string key)
		{
			this.click_snd = "enter_small";
			return this.Skin = (this.NcSkin = new ButtonSkinNumCounter(this, this.w, this.h));
		}

		public int getValue()
		{
			return this.dval;
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (this.slide_t != 0f && X.D)
			{
				this.slide_t = X.VALWALK(this.slide_t, 0f, (float)X.AF);
				base.Fine(false);
			}
			return true;
		}

		public void setValue(int v, bool upper)
		{
			v %= 10;
			if (v != this.dval)
			{
				this.slide_t = (float)X.MPF(!upper) * 20f;
				this.dval = v;
				base.Fine(false);
			}
		}

		public int digit_unit
		{
			get
			{
				if (this.Container == null)
				{
					return 0;
				}
				return (int)X.Pow(10f, this.Container.Length - this.carr_index - 1);
			}
		}

		public bool unuse_digit
		{
			get
			{
				INumCounterFuncs numCounterFuncs = this.Container as INumCounterFuncs;
				if (numCounterFuncs == null || base.isLocked())
				{
					return true;
				}
				int digit_unit = this.digit_unit;
				return this.dval == 0 && digit_unit > 1 && numCounterFuncs.getDigitValue() < digit_unit;
			}
		}

		public void slideTo(int v)
		{
			INumCounterFuncs numCounterFuncs = this.Container as INumCounterFuncs;
			if (numCounterFuncs == null)
			{
				return;
			}
			int num = numCounterFuncs.getDigitValue();
			int digit_unit = this.digit_unit;
			int num2 = num / digit_unit % 10;
			if (v > 0)
			{
				if (numCounterFuncs.getDigitValue() >= numCounterFuncs.getMaximumValue())
				{
					if (numCounterFuncs.slide_cur_digit_only)
					{
						num -= num2 * digit_unit;
						numCounterFuncs.setValue(num);
					}
					else
					{
						numCounterFuncs.setValue(numCounterFuncs.getMinimumValue());
					}
				}
				else if (numCounterFuncs.slide_cur_digit_only)
				{
					if (num2 == 9)
					{
						num -= num2 * digit_unit;
					}
					else
					{
						num += digit_unit;
					}
					numCounterFuncs.setValue(num);
				}
				else
				{
					numCounterFuncs.setValue(num + digit_unit);
				}
				numCounterFuncs.playSlideSound(true);
				return;
			}
			if (v < 0)
			{
				if (numCounterFuncs.slide_cur_digit_only)
				{
					if (num <= numCounterFuncs.getMinimumValue() || num2 == 0)
					{
						num -= num2 * digit_unit;
						num += 9 * digit_unit;
					}
					else
					{
						num -= digit_unit;
					}
					numCounterFuncs.setValue(num);
				}
				else if (num <= numCounterFuncs.getMinimumValue())
				{
					numCounterFuncs.setValue(numCounterFuncs.getMaximumValue());
				}
				else
				{
					numCounterFuncs.setValue(num - digit_unit);
				}
				numCounterFuncs.playSlideSound(false);
			}
		}

		protected override void simulateNaviTranslation(int aim = -1)
		{
			INumCounterFuncs numCounterFuncs = this.Container as INumCounterFuncs;
			if (aim == 1 || IN.isT())
			{
				if (numCounterFuncs == null || base.isLocked())
				{
					return;
				}
				IN.clearCursDown();
				this.slideTo(1);
				return;
			}
			else
			{
				if (aim != 3 && !IN.isB())
				{
					base.simulateNaviTranslation(aim);
					return;
				}
				if (numCounterFuncs == null || base.isLocked())
				{
					return;
				}
				IN.clearCursDown();
				this.slideTo(-1);
				return;
			}
		}

		public const float DEFAULT_W = 24f;

		public const float DEFAULT_H = 48f;

		public float slide_t;

		public float chr_scale = 2f;

		public BMListChars Chr = MTRX.ChrL;

		private const float SLIDE_ANIM_T = 20f;

		private int dval;

		public ButtonSkinNumCounter NcSkin;
	}
}
