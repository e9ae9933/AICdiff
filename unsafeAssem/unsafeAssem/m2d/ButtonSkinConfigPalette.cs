using System;
using XX;

namespace m2d
{
	public class ButtonSkinConfigPalette : ButtonSkin
	{
		public ButtonSkinConfigPalette(aBtn _B, float _w, float _h)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.MdIco = base.makeMesh(MTRX.MIicon.getMtr(BLEND.NORMAL, -1));
			this.w = ((_w > 0f) ? _w : 28f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 28f) * 0.015625f;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 64f;
			float num2 = this.h * 64f;
			float num3 = num / 2f;
			float num4 = num2 / 2f;
			this.Md.Col = C32.d2c(2281701376U);
			this.Md.Rect(0f, 0f, num, num2, false);
			this.Md.Col = C32.d2c(1157627903U);
			this.Md.Box(0f, 0f, num, num2, base.isHoveredOrPushOut() ? 1.5f : 0.5f, false);
			if (this.isPushDown())
			{
				this.Md.StripedRect(0f, 0f, num, num2, X.ANMPT(36, 1f), 0.5f, 6f, false);
			}
			CCON.DrawInit(this.Md, this.MdIco, num, 1f);
			CCON.draw(this.config, -num3, -num4, null);
			CCON.DrawQuit();
			this.Md.updateForMeshRenderer(false);
			this.MdIco.updateForMeshRenderer(false);
			return base.Fine();
		}

		protected MeshDrawer Md;

		public int config = 4;

		private MeshDrawer MdIco;

		private const float DEFAULT_W = 28f;

		private const float DEFAULT_H = 28f;
	}
}
