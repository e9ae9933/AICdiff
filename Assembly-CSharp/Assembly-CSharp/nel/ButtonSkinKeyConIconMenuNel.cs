using System;
using XX;

namespace nel
{
	public class ButtonSkinKeyConIconMenuNel : ButtonSKinKadomaruIconNel
	{
		public ButtonSkinKeyConIconMenuNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			this.MdStripeB = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref));
			base.icon_scale = 0.6f;
			this.radius = 8f;
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			int num = X.NmI(str, 0, true, false) - 1;
			if (!X.BTW(0f, (float)num, (float)MTRX.SqFImgKCIcon.countLayers()))
			{
				num = -1;
			}
			this.Img = ((num == -1) ? null : MTRX.SqFImgKCIcon.getLayer(num).Img);
			base.fineMI(MTRX.MIicon);
			this.fine_flag = true;
			return this;
		}

		protected override C32 setIconColor()
		{
			ButtonSkinKadomaruIcon.Col.Set(4283780170U);
			if (base.isLocked())
			{
				ButtonSkinKadomaruIcon.Col.Set(4288057994U);
			}
			else if (this.isPushDown())
			{
				ButtonSkinKadomaruIcon.Col.Set(4293321691U);
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinKadomaruIcon.Col.mulA(0.7f + 0.3f * X.COSIT(50f));
			}
			return ButtonSkinKadomaruIcon.Col;
		}

		protected override void drawChecked(float wh, float hh)
		{
			base.drawChecked(wh, hh);
			this.MdStripeB.Col = ButtonSkinKadomaruIcon.Col.Set(this.col_stripe).mulA(0.6f + 0.4f * X.COSIT(25f)).mulA(this.alpha_)
				.C;
			this.MdStripeB.RectDashedM(0f, 0f, this.w - 6f, this.h - 6f, 20, 2f, 0.5f, false, false);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			this.MdStripeB.clear(true, false);
			base.Fine();
			this.MdStripeB.updateForMeshRenderer(false);
			return this;
		}

		private MeshDrawer MdStripeB;
	}
}
