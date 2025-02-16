using System;

namespace XX
{
	public interface INumCounterFuncs
	{
		int getDigitValue();

		int getMaximumValue();

		int getMinimumValue();

		void playSlideSound(bool to_upper);

		void setValue(int v);

		bool slide_cur_digit_only { get; set; }
	}
}
