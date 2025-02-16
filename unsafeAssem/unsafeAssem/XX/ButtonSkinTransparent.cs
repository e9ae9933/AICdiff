using System;

namespace XX
{
	public class ButtonSkinTransparent : ButtonSkin
	{
		public ButtonSkinTransparent(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w * 0.015625f, _h * 0.015625f)
		{
		}

		public override bool isEnable()
		{
			return this.enabled;
		}

		public override void setEnable(bool f)
		{
			this.enabled = f;
		}

		private bool enabled;
	}
}
