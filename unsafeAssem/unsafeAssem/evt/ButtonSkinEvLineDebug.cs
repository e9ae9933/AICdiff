using System;
using XX;

namespace evt
{
	public class ButtonSkinEvLineDebug : ButtonSkin
	{
		public ButtonSkinEvLineDebug(aBtn _B, float _w, float _h)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.fine_continue_flags = 1U;
			this.w = _w * 0.015625f;
			this.h = _h * 0.015625f;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = 1f;
			if (base.isPushed())
			{
				num = 0.5f;
			}
			else if (base.isHoveredOrPushOut())
			{
				num = 0.6f + 0.3f * X.COSIT(40f);
			}
			this.Md.Col = C32.MulA(uint.MaxValue, num * this.alpha);
			STB stb = TX.PopBld(this.title, 0);
			MTRX.ChrL.DrawStringTo(this.Md, stb, 0f, -this.h * 0.015625f * 0.5f, ALIGN.CENTER, ALIGNY.BOTTOM, false, 0f, 0f, null);
			TX.ReleaseBld(stb);
			this.Md.updateForMeshRenderer(false);
			return base.Fine();
		}

		protected MeshDrawer Md;
	}
}
