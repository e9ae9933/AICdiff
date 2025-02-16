using System;
using XX;

namespace nel
{
	public class ButtonSkinNelReelPict : ButtonSkinNelUi
	{
		public ButtonSkinNelReelPict(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.alignx_ = ALIGN.CENTER;
		}

		protected override void setTitleText(string str)
		{
			if (str == null)
			{
				base.setTitleText(null);
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				ReelExecuter.effect2text(stb, str);
				string text = stb.ToString();
				base.setTitleText(text);
			}
		}
	}
}
