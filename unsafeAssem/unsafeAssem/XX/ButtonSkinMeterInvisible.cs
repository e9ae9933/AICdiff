using System;
using UnityEngine;

namespace XX
{
	public class ButtonSkinMeterInvisible : ButtonSkinMeterNormal
	{
		public ButtonSkinMeterInvisible(aBtnMeter _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
		}

		protected override void redrawMemori()
		{
		}

		protected override void initCircleRenderer()
		{
		}

		protected override Material getTitleStringChrMaterial(BLEND blnd, BMListChars Chr, MeshDrawer Md)
		{
			return null;
		}

		protected override void redrawMarker()
		{
		}

		public override bool canClickable(Vector2 PosU)
		{
			if (this.Meter != null)
			{
				CtSetter ctSetter = this.Meter.getCtSetter();
				if (ctSetter != null)
				{
					PosU += new Vector2(ctSetter.get_swidth_px() * 0.5f * 0.015625f, 0f);
					return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), ctSetter.get_swidth_px() * 0.015625f, X.Mx(0.53125f, this.h));
				}
			}
			return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), this.swidth * 0.015625f, X.Mx(0.53125f, this.h));
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			return this;
		}
	}
}
