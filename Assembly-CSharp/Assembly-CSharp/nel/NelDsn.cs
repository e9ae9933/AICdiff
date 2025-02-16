using System;
using XX;

namespace nel
{
	public class NelDsn
	{
		public static DsnDataP PT(int _size = 14, bool _html = true)
		{
			return new DsnDataP("", false)
			{
				text = "  ",
				TxCol = C32.d2c(4283780170U),
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				size = (float)_size,
				html = _html,
				text_margin_x = 0f,
				text_margin_y = 0f
			};
		}

		public static DsnDataP PT_LT(int _size = 14, bool _html = true)
		{
			DsnDataP dsnDataP = NelDsn.PT(_size, _html);
			dsnDataP.alignx = ALIGN.LEFT;
			dsnDataP.aligny = ALIGNY.TOP;
			return dsnDataP;
		}
	}
}
