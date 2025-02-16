using System;

namespace XX
{
	public class ButtonSkinNormalHilighted : ButtonSkinNormal
	{
		public ButtonSkinNormalHilighted(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref), false);
			this.Md.connectRendererToTriMulti(base.getMeshRenderer(this.Md));
			this.Md.chooseSubMesh(0, false, false);
			this.fine_continue_flags |= 28U;
			this.auto_update = false;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			this.Md.clear(false, false);
			base.Fine();
			float base_x = this.Md.base_x;
			float base_y = this.Md.base_y;
			this.base_color = ((X.ANM(IN.totalframe, 2, 15f) == 1) ? 4293519849U : 4291097087U);
			float num = 0.5f;
			float num2 = this.h * 64f + num;
			float num3 = this.w * 64f + num;
			if (!base.isPushed() || !base.isFocused())
			{
				this.Md.base_x -= this.shadow_shift;
				this.Md.base_y += this.shadow_shift;
			}
			else
			{
				this.Md.base_x += this.shadow_shift;
				this.Md.base_y -= this.shadow_shift;
				num3 += 8f;
				num2 += 8f;
			}
			this.Md.chooseSubMesh(1, false, false);
			this.Md.Col = C32.codeToColor32((base.isFocused() && !base.isPushed()) ? 4287151359U : (base.isPushed() ? 4286216826U : 4288317183U));
			this.Md.ButtonKadomaruDashedM(0f, 0f, num3, num2, num2, 16, 2f, false, 0.5f, -1);
			this.Md.chooseSubMesh(0, false, false);
			this.Md.base_x = base_x;
			this.Md.base_y = base_y;
			this.Md.updateForMeshRenderer(false);
			return this;
		}
	}
}
