using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinMiniLink : ButtonSkin
	{
		public ButtonSkinMiniLink(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.shadow_shift = 1.4f;
			this.w = ((_w > 0f) ? _w : 20f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 20f) * 0.015625f;
		}

		public void setTexture(Texture _Tx, Material Mtr, float scale = 1f, uint col = 4294967295U)
		{
			if (this.MdIco == null)
			{
				this.MdIco = base.makeMesh(Mtr);
				this.MdIco.initForImg(_Tx);
			}
			this.fill_scale = scale;
			this.fill_col = col;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f || this.MdIco == null)
			{
				return this;
			}
			float num = this.w * 64f + 0.5f;
			float h = this.h;
			bool flag = false;
			float num2 = 0f;
			int num3 = 2;
			this.Md.clear(false, false);
			float num4 = 0f;
			if (this.isPushDown())
			{
				this.Md.Col = this.Md.ColGrd.Set(uint.MaxValue).mulA(this.alpha_ * 0.7f).C;
				C32 c = MTRX.cola.Set(this.Md.ColGrd);
				this.Md.BlurPoly2(this.shadow_shift, -this.shadow_shift, num * 0.3f, 0f, 30, num, num * 0.2f, c, this.Md.ColGrd.setA(0f));
				this.hover_t = 20f;
				num2 = this.shadow_shift;
				num3 = 5;
			}
			else if (base.isHoveredOrPushOut())
			{
				if (this.hover_t < 20f)
				{
					flag = true;
					this.hover_t = X.Mn(this.hover_t + (float)X.AF, 20f);
					num4 = X.COSI((float)IN.totalframe, 5.5f) * 0.12f * 3.1415927f * (1f - X.ZSIN(this.hover_t, 20f));
				}
			}
			else if (this.hover_t > 0f)
			{
				flag = true;
				this.hover_t = X.Mn(this.hover_t - (float)X.AF, 20f);
			}
			float num5 = this.alpha_ * (0.6f + 0.4f * X.ZLINE(this.hover_t, 20f));
			this.MdIco.ColGrd.Set(this.fill_col).mulA(num5);
			for (int i = num3 - 1; i >= 0; i--)
			{
				float num6 = 0f;
				float num7 = 0f;
				if (i >= 1)
				{
					if (num3 == 2)
					{
						this.MdIco.Col = C32.MulA(2852126720U, num5);
						num6 = 1f;
						num7 = -1f;
					}
					else
					{
						this.MdIco.Col = C32.MulA(4278190080U, this.alpha_);
						num6 = (float)(2 * CAim._XD(i - 1, 1));
						num7 = (float)(2 * CAim._YD(i - 1, 1));
					}
				}
				else
				{
					this.MdIco.Col = this.MdIco.ColGrd.C;
				}
				if (this.PFIco != null)
				{
					this.MdIco.RotaPF(num6 + num2, num7 - num2, this.fill_scale, this.fill_scale, num4, this.PFIco, false, false, false, uint.MaxValue, false, 0);
				}
				else
				{
					this.MdIco.RotaGraph(num6 + num2, num7 - num2, this.fill_scale, num4, null, false);
				}
			}
			this.MdIco.updateForMeshRenderer(false);
			this.Md.updateForMeshRenderer(false);
			base.Fine();
			if (flag)
			{
				this.fine_flag = true;
			}
			return this;
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

		private MeshDrawer Md;

		private MeshDrawer MdIco;

		private PxlFrame PFIco;

		private uint fill_col;

		public float fill_scale;

		public const float DEFAULT_W = 20f;

		public const float DEFAULT_H = 20f;

		protected readonly float shadow_shift;

		public float hover_t;

		private const float T_FADE = 20f;
	}
}
