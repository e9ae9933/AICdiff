using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinDraggerColorChooser : ButtonSkin
	{
		public uint base_col
		{
			get
			{
				return this.base_col_;
			}
			set
			{
				if (this.base_col == value)
				{
					return;
				}
				this.base_col_ = value;
				this.fine_flag = true;
			}
		}

		public ButtonSkinDraggerColorChooser(aBtnDragger _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Bdr = _B;
			this.MdB = base.makeMesh(null);
			this.MdB.chooseSubMesh(1, false, false);
			this.MdB.connectRendererToTriMulti(base.getMeshRenderer(this.MdB));
			this.Md = base.makeMesh(null);
			this.w = _w * 0.015625f;
			this.h = _h * 0.015625f;
		}

		public void initDraggerTexture(Texture2D Tx)
		{
			int container_stencil_ref = base.container_stencil_ref;
			this.MdB.chooseSubMesh(1, false, false);
			Material material = new Material((container_stencil_ref >= 0) ? MTRX.ShaderGDTST : MTRX.ShaderGDT);
			this.MdB.setMaterial(material, true);
			this.MdB.initForImgAndTexture(Tx);
			if (container_stencil_ref >= 0)
			{
				material.SetFloat("_StencilRef", (float)container_stencil_ref);
				material.SetFloat("_StencilComp", 3f);
			}
			this.MdB.connectRendererToTriMulti(base.getMeshRenderer(this.MdB));
			this.fine_flag = true;
		}

		public void initDraggerTexture(PxlFrame _PF)
		{
			this.MdB.chooseSubMesh(1, false, false);
			int container_stencil_ref = base.container_stencil_ref;
			new Material(MTRX.ShaderGDT);
			this.PF = _PF;
			this.MdB.setMaterial(MTRX.getMI(this.PF).getMtr(BLEND.NORMAL, container_stencil_ref), false);
			this.MdB.connectRendererToTriMulti(base.getMeshRenderer(this.MdB));
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha <= 0f)
			{
				return this;
			}
			float num = this.w * 64f;
			float num2 = this.h * 64f;
			this.Md.clearSimple();
			this.MdB.clearSimple();
			this.MdB.chooseSubMesh(0, false, true);
			if ((this.base_col_ & 4278190080U) > 0U)
			{
				this.MdB.Col = this.MdB.ColGrd.Set(this.base_col_).mulA(this.alpha_).C;
				this.MdB.Rect(0f, 0f, num - 2f, num2 - 2f, false);
			}
			this.MdB.chooseSubMesh(1, false, true);
			this.MdB.Col = C32.WMulA(this.alpha_);
			if (this.PF != null)
			{
				this.MdB.RotaPF(0f, 0f, 1f, 1f, 0f, this.PF, false, false, false, uint.MaxValue, false, 0);
			}
			else
			{
				this.MdB.Rect(0f, 0f, num - 2f, num2 - 2f, false);
			}
			Vector2 drawPositionPixel = this.getDrawPositionPixel();
			float num3 = 7f;
			this.Md.Col = C32.MulA(4278190080U, this.alpha);
			this.Md.Box(0f, 0f, num, num2, 2f, false);
			this.Md.Poly(drawPositionPixel.x, drawPositionPixel.y, num3, 0f, 14, 2f, false, 0f, 0f);
			this.Md.Col = C32.MulA(uint.MaxValue, this.alpha);
			this.Md.Box(0f, 0f, num - 2f, num2 - 2f, 1f, false);
			this.Md.Poly(drawPositionPixel.x, drawPositionPixel.y, num3 - 1f, 0f, 14, 1f, false, 0f, 0f);
			this.Md.updateForMeshRenderer(false);
			this.MdB.updateForMeshRenderer(false);
			return base.Fine();
		}

		public Vector2 getDrawPositionPixel()
		{
			Vector2 valueV = this.Bdr.getValueV(false);
			Vector2 zero = Vector2.zero;
			if (this.Bdr.moveable_x > 0f)
			{
				zero.x = -this.Bdr.moveable_x * 0.5f + valueV.x;
			}
			if (this.Bdr.moveable_y > 0f)
			{
				zero.y = -this.Bdr.moveable_y * 0.5f + valueV.y;
			}
			return zero;
		}

		public readonly aBtnDragger Bdr;

		protected MeshDrawer Md;

		protected MeshDrawer MdB;

		private uint base_col_;

		private PxlFrame PF;
	}
}
