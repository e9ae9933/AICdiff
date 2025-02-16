using System;
using PixelLiner;
using XX;

namespace nel
{
	public class ButtonSkinKeyConIconNel : ButtonSkin
	{
		public ButtonSkinKeyConIconNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w * 0.015625f, _h * 0.015625f)
		{
			this.Md = base.makeMesh(null);
			this.MdIco = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.MdStripe = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshStriped.shader, base.container_stencil_ref));
			if (ButtonSkinKeyConIconNel.Col == null)
			{
				ButtonSkinKeyConIconNel.Col = new C32();
			}
			this.fine_continue_flags = 9U;
			this.curs_level_x = 0f;
			this.curs_level_y = 0.3f;
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
			this.fine_flag = true;
			return this;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = (float)(X.IntR(this.icon_w * 0.5f) * 2);
			float num2 = (float)(X.IntR(this.icon_h * 0.5f) * 2);
			float num3 = num * 0.5f;
			float num4 = -this.w * 64f / 2f + num3;
			float num5 = -this.h * 64f / 2f;
			float num6 = num5 + this.icon_h / 2f;
			float num7 = num4 - num3;
			float num8 = num7 + this.w * 64f;
			this.Md.clear(false, false);
			this.MdIco.clear(false, false);
			this.MdStripe.clear(false, false);
			if (this.Img != null)
			{
				ButtonSkinKeyConLabelNel.SetTxColor(ButtonSkinKeyConIconNel.Col, this, 0.7f);
				this.MdIco.Col = ButtonSkinKeyConIconNel.Col.mulA(this.alpha_).C;
				this.MdIco.initForImg(this.Img, 0).DrawScaleGraph(num4, num6, 0.5f, 0.5f, null);
				this.MdIco.updateForMeshRenderer(false);
			}
			uint num9 = 4293321691U;
			if (base.isLocked())
			{
				this.Md.Col = ButtonSkinKeyConIconNel.Col.Set(2001554765).mulA(this.alpha_).C;
				this.Md.Rect(num4, num6, num, num2, false);
				num9 = 0U;
			}
			else if (this.isPushDown())
			{
				this.Md.Col = ButtonSkinKeyConIconNel.Col.Set(4293321691U).mulA(this.alpha_).C;
				this.Md.Rect(num4, num6, num, num2, false);
				num9 = 4283780170U;
			}
			else if (base.isChecked())
			{
				this.MdStripe.Col = (this.Md.Col = ButtonSkinKeyConIconNel.Col.Set(4293321691U).mulA(this.alpha * 0.2f).C);
				this.MdStripe.StripedM(0.7853982f, 20f, 0.5f, 4).Rect(num4, num6, num, num2, false).allocUv2(0, true);
				num9 = 0U;
			}
			else if (base.isHoveredOrPushOut())
			{
				this.Md.Col = ButtonSkinKeyConIconNel.Col.Set(4293321691U).mulA((0.15f + 0.15f * X.COSIT(34f)) * this.alpha_).C;
				this.Md.Rect(num4, num6, num, num2, false);
			}
			if (num9 >= 16777216U)
			{
				this.Md.Col = ButtonSkinKeyConIconNel.Col.Set(num9).mulA(this.alpha_).C;
				this.Md.Tri(0, 1, 2, false);
				this.Md.PosD(num8 - 8f, num5 + 3f, null).PosD(num8 - 8f - 5f, num5 + 3f + 5f, null).PosD(num8 - 8f + 5f, num5 + 3f + 5f, null);
			}
			if (!base.isLocked() && !this.isPushDown())
			{
				this.Md.Col = ButtonSkinKeyConIconNel.Col.Set(4293321691U).mulA(this.alpha_).C;
				this.Md.Line(num7, num5, num8, num5, 1f, false, 0f, 0f);
			}
			this.Md.updateForMeshRenderer(false);
			this.MdStripe.updateForMeshRenderer(false);
			return base.Fine();
		}

		public override bool isPushDown()
		{
			return base.isPushDown() && !base.isChecked();
		}

		protected MeshDrawer Md;

		protected MeshDrawer MdStripe;

		protected MeshDrawer MdIco;

		private PxlImage Img;

		public static C32 Col;

		public float icon_w = 60f;

		public float icon_h = 38f;
	}
}
