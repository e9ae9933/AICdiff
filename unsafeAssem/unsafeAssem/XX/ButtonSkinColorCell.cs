using System;
using UnityEngine;

namespace XX
{
	public class ButtonSkinColorCell : ButtonSkin
	{
		public ButtonSkinColorCell(aBtnColorCell _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.BtCell = _B;
			this.w = ((_w > 0f) ? _w : 70f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 20f) * 0.015625f;
			this.Chr = (this.DefaultTitleChr = MTRX.ChrM);
			this.Md = base.makeMesh(null);
			this.MdChr = base.makeMesh(BLEND.NORMALBORDER8, this.Chr.MI);
			if (ButtonSkinColorCell.Col == null)
			{
				ButtonSkinColorCell.Col = new C32();
			}
			this.fine_continue_flags = 1U;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 64f;
			float num2 = this.h * 64f;
			float num3 = (float)(this.isPushDown() ? 2 : 0);
			float swidth = this.swidth;
			float num4 = ((this.Tx != null) ? (this.swidth / 2f - num / 2f) : 0f);
			if (this.Tx != null)
			{
				IN.PosP(this.Tx.transform, 0f, -num3 + 6f, -0.08f);
			}
			ButtonSkinColorCell.Col.Set(uint.MaxValue);
			float num5 = num;
			float num6 = num2;
			if (this.isPushDown())
			{
				num5 -= 4f;
				num6 -= 4f;
			}
			this.Md.Col = ButtonSkinColorCell.Col.C;
			this.Md.Box(num4, 0f, num5, num6, 1f, false);
			ButtonSkinColorCell.Col.Set(this.BtCell.getColor());
			this.Md.Col = ButtonSkinColorCell.Col.C;
			this.Md.Box(num4, 0f, num5 - 4f, num6 - 4f, 0f, false);
			bool flag = C32.isDark(ButtonSkinColorCell.Col.C);
			string rgbax = ButtonSkinColorCell.Col.rgbax;
			if (base.isHoveredOrPushOut())
			{
				this.Md.Col = ButtonSkinColorCell.Col.Set(1442840575).C;
				this.Md.StripedRect(num4, 0f, num - 4f, num2 - 4f, X.ANMPT(40, 1f), 0.5f, 8f, false);
			}
			this.Md.updateForMeshRenderer(false);
			if (this.Tx != null)
			{
				IN.PosP(this.Tx.transform, 4f - swidth / 2f + this.default_title_width / 2f, 0f, 0f);
			}
			if (this.fine_color_flag)
			{
				if (this.BtCell.use_text)
				{
					this.MdChr.Col = ButtonSkinColorCell.Col.Set((!flag) ? 4281084972U : uint.MaxValue).C;
					this.MdChr.allocUv23(64, false);
					this.MdChr.Uv23(ButtonSkinColorCell.Col.Set(flag ? 4281084972U : uint.MaxValue).C, true);
					STB stb = TX.PopBld(this.BtCell.getValueString(), 0);
					this.Chr.DrawStringTo(this.MdChr, stb, num4, -4f, ALIGN.CENTER, ALIGNY.BOTTOM, false, 0f, 0f, null);
					TX.ReleaseBld(stb);
					this.MdChr.allocUv23(0, true);
				}
				this.MdChr.updateForMeshRenderer(false);
				this.fine_color_flag = false;
			}
			return base.Fine();
		}

		public bool fine_color_flag
		{
			get
			{
				return this.fine_color_flag_;
			}
			set
			{
				this.fine_color_flag_ = true;
				this.fine_flag = true;
			}
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			if (this.Tx == null)
			{
				this.Tx = base.MakeTx("-tx");
				this.Tx.Size(12f).Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE);
			}
			if (this.B.Container != null)
			{
				this.Tx.StencilRef(this.B.Container.stencil_ref);
			}
			this.Tx.Txt(X.T2K(str));
			this.default_title_width = this.Tx.get_swidth_px();
			this.fine_flag = true;
			return this;
		}

		public override bool canClickable(Vector2 PosU)
		{
			return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), this.swidth * 0.015625f, this.h);
		}

		public float getTextSwidth()
		{
			if (this.default_title_width != 0f)
			{
				return 10f + this.default_title_width;
			}
			return 0f;
		}

		public override void setEnable(bool f)
		{
			base.setEnable(f);
			if (this.Tx != null)
			{
				this.Tx.enabled = f;
			}
		}

		public override float swidth
		{
			get
			{
				return this.w * 64f + this.getTextSwidth();
			}
		}

		private TextRenderer Tx;

		private MeshDrawer Md;

		private MeshDrawer MdChr;

		private aBtnColorCell BtCell;

		public static C32 Col;

		private BMListChars Chr;

		private bool fine_color_flag_ = true;

		public const float DEFAULT_W = 70f;

		public const float DEFAULT_H = 20f;

		public const float TX_MARGIN = 10f;
	}
}
