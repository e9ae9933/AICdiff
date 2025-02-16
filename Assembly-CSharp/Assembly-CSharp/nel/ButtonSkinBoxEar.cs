using System;
using PixelLiner;
using XX;

namespace nel
{
	public class ButtonSkinBoxEar : ButtonSkin
	{
		public ButtonSkinBoxEar(aBtnBoxEar _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.BEr = _B;
			this.Md = base.makeMesh(null);
			this.w = ((_w > 0f) ? _w : 30f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 30f) * 0.015625f;
		}

		public void setMesh(PxlFrame _PF)
		{
			if (this.PF == _PF)
			{
				return;
			}
			this.PF = _PF;
			if (this.PF != null && !this.Md.hasMultipleTriangle())
			{
				this.Md.chooseSubMesh(1, false, true);
				this.Md.base_z = -0.01f;
				this.Md.setMaterial(MTRX.getMI(this.PF).getMtr(BLEND.NORMAL, -1), false);
				this.Md.connectRendererToTriMulti(this.MyMeshRenderer);
			}
			this.fine_flag = true;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 64f * 0.5f;
			if (this.Md.hasMultipleTriangle())
			{
				this.Md.chooseSubMesh(0, false, true);
			}
			this.Md.Col = C32.MulA(this.BEr.C, this.alpha);
			ButtonSkinNelTab.drawHalfKadomaru(this.Md, num, num, num * 0.3f);
			if (this.Md.hasMultipleTriangle())
			{
				this.Md.chooseSubMesh(1, false, true);
				if (this.PF != null)
				{
					this.Md.Col = C32.MulA(uint.MaxValue, this.alpha * this.BEr.pict_alpha);
					this.Md.RotaPF(0f, 0f, this.draw_scale, this.draw_scale, 0f, this.PF, false, false, false, uint.MaxValue, false, 0);
				}
			}
			this.Md.updateForMeshRenderer(false);
			return this;
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			this.setMesh(MTRX.getPF(str));
			return this;
		}

		public const float DEFAULT_W = 30f;

		private aBtnBoxEar BEr;

		private MeshDrawer Md;

		private PxlFrame PF;

		public float draw_scale = 1f;
	}
}
