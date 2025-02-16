using System;

namespace XX
{
	public class ButtonSkinBox : ButtonSkin
	{
		public ButtonSkinBox(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w * 0.015625f, _h * 0.015625f)
		{
			this.Md = base.makeMesh(null);
			this.MdIco = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.MdStripe = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshStriped.shader, base.container_stencil_ref));
			if (this.Col == null)
			{
				this.Col = new C32();
			}
			this.fine_continue_flags = 5U;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 64f;
			float num2 = this.h * 64f;
			this.Md.clear(false, false);
			this.MdStripe.clear(false, false);
			if (this.isPushDown())
			{
				this.Col.Set(3431570607U);
			}
			else if (base.isChecked())
			{
				this.Col.Set(base.isHoveredOrPushOut() ? 3712849558U : 3710019174U);
			}
			else if (base.isHoveredOrPushOut())
			{
				this.Col.Set(3427750485U);
			}
			else
			{
				this.Col.Set(MTRX.ColMenu);
			}
			this.Md.Col = this.Col.C;
			this.Md.Box(0f, 0f, num, num2, 0f, false);
			if (base.isFocused() || base.isPushed())
			{
				this.MdStripe.Col = this.Col.Set(this.isPushDown() ? 3434794207U : (base.isChecked() ? 3711477928U : 3429725050U)).C;
				this.MdStripe.StripedM(0.7853982f, 20f, 0.5f, 4).Rect(0f, 0f, num, num2, false).allocUv2(0, true);
			}
			this.Md.updateForMeshRenderer(false);
			if (base.isChecked())
			{
				this.MdIco.RotaPF(num / 2f - 7f, -num2 / 2f + 7f, 1f, 1f, 0f, MTRX.getPF("checked"), false, false, false, uint.MaxValue, false, 0);
			}
			this.MdIco.updateForMeshRenderer(false);
			this.MdStripe.updateForMeshRenderer(false);
			return base.Fine();
		}

		private MeshDrawer Md;

		private MeshDrawer MdStripe;

		private MeshDrawer MdIco;

		private C32 Col;
	}
}
