using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinMini : ButtonSkinDesc
	{
		public ButtonSkinMini(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref), false);
			this.Md.connectRendererToTriMulti(base.getMeshRenderer(this.Md));
			this.Md.chooseSubMesh(0, false, false);
			this.shadow_shift = 1.4f;
			this.w = ((_w > 0f) ? _w : 20f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 20f) * 0.015625f;
			if (ButtonSkinMini.Col == null)
			{
				ButtonSkinMini.Col = new C32();
			}
			this.fine_continue_flags = 5U;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 64f + 0.5f;
			float num2 = this.h * 64f + 0.5f;
			if (base.isChecked())
			{
				this.prepareIconMesh();
			}
			this.Md.Identity();
			this.Md.chooseSubMesh(0, false, false);
			this.Md.clear(false, false);
			this.Md.base_z = 0f;
			if (this.MdIco != null)
			{
				this.MdIco.Identity();
			}
			if (this.isPushDown())
			{
				this.Md.TranslateP(this.shadow_shift, -this.shadow_shift, false);
				if (this.MdIco != null)
				{
					this.MdIco.TranslateP(this.shadow_shift, -this.shadow_shift, false);
				}
			}
			ButtonSkinMini.Col.Set(15658734);
			if (base.isLocked())
			{
				ButtonSkinMini.Col.Set(9604750);
			}
			else if (this.isPushDown())
			{
				ButtonSkinMini.Col.Set(8695017);
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinMini.Col.Set(base.isChecked() ? 4287488764U : 4291488511U);
			}
			else if (base.isChecked())
			{
				ButtonSkinMini.Col.Set(8299263);
			}
			ButtonSkinMini.Col.setA1(0.65f * this.alpha_);
			this.Md.Col = ButtonSkinMini.Col.C;
			this.Md.KadomaruRect(0f, 0f, num, num2, num2 / 2f, 0f, false, 0f, 0f, false);
			if (!base.isLocked())
			{
				ButtonSkinMini.Col.blend(uint.MaxValue, 0.4f);
			}
			ButtonSkinMini.Col.setA1(this.alpha_);
			this.Md.Col = ButtonSkinMini.Col.C;
			if (!base.isLocked() && !this.isPushDown() && base.isHoveredOrPushOut())
			{
				this.Md.chooseSubMesh(1, false, false);
				this.Md.Col = ButtonSkinMini.Col.Set(4283480575U).mulA(this.alpha_).C;
				this.Md.ButtonKadomaruDashedM(0f, 0f, num, num2, num2, 10, 1f, false, 0.5f, -1);
				this.Md.chooseSubMesh(0, false, false);
			}
			else
			{
				this.Md.KadomaruRect(0f, 0f, num, num2, num2 / 2f, 1f, false, 0f, 0f, false);
			}
			this.Md.updateForMeshRenderer(false);
			if (this.MdIco != null)
			{
				if (this.PFIco != null)
				{
					ButtonSkinMini.Col.Set(uint.MaxValue);
					if (base.isLocked())
					{
						ButtonSkinMini.Col.Set(3431499912U);
					}
					else if (this.isPushDown())
					{
						ButtonSkinMini.Col.Set(2868903935U);
					}
					else if (base.isHoveredOrPushOut())
					{
						ButtonSkinMini.Col.Set(uint.MaxValue).blend(2868903935U, X.Mx(0f, 0.4f + 0.4f * X.COSIT(34f)));
					}
					this.MdIco.Col = ButtonSkinMini.Col.mulA(this.alpha_).C;
					if (this.PFIco != null)
					{
						this.MdIco.RotaPF(0f, 0f, 1f, 1f, 0f, this.PFIco, false, false, false, uint.MaxValue, false, 0);
					}
				}
				if (base.isChecked())
				{
					ButtonSkinMini.Col.Set(uint.MaxValue);
					if (base.isLocked())
					{
						ButtonSkinMini.Col.Set(3431499912U);
					}
					this.MdIco.Col = ButtonSkinMini.Col.mulA(this.alpha_).C;
					this.MdIco.RotaPF(num / 2f - 4f, -num2 / 2f + 4f, 1f, 1f, 0f, MTRX.getPF("checked_s"), false, false, false, uint.MaxValue, false, 0);
				}
				this.MdIco.updateForMeshRenderer(false);
			}
			return base.Fine();
		}

		protected override Material getTitleStringChrMaterial(BLEND blnd, BMListChars Chr, MeshDrawer Md)
		{
			if (blnd == BLEND._MAX)
			{
				blnd = BLEND.NORMAL;
			}
			Material mtr = Chr.MI.getMtr(blnd, -1);
			this.Vdefault_title_shift.y = this.Vdefault_title_shift.y + 0.5f;
			return mtr;
		}

		public override ButtonSkin setTitle(string str)
		{
			this.fine_flag = true;
			this.PFIco = MTRX.getPF(str);
			if (this.PFIco != null)
			{
				this.prepareIconMesh();
			}
			this.title = str;
			return this;
		}

		private void prepareIconMesh()
		{
			if (this.MdIco != null)
			{
				return;
			}
			this.MdIco = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
		}

		protected MeshDrawer Md;

		private MeshDrawer MdIco;

		private PxlFrame PFIco;

		public const float DEFAULT_W = 20f;

		public const float DEFAULT_H = 20f;

		public const float DEFAULT_WL = 34f;

		public const float DEFAULT_HL = 34f;

		protected readonly float shadow_shift;

		public static C32 Col;
	}
}
