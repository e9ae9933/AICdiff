using System;
using XX;

namespace nel
{
	public class ButtonSkinKeyConLabelNel : ButtonSkin
	{
		public ButtonSkinKeyConLabelNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w * 0.015625f, _h * 0.015625f)
		{
			this.Md = base.makeMesh(null);
			this.MdStripe = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshStriped.shader, base.container_stencil_ref));
			if (ButtonSkinKeyConLabelNel.Col == null)
			{
				ButtonSkinKeyConLabelNel.Col = new C32();
			}
			this.curs_level_x = 0.4f;
			this.curs_level_y = 0f;
			this.fine_continue_flags = 9U;
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			if (this.Tx == null)
			{
				this.Tx = base.MakeTx("-text");
				this.Tx.Size(this.h * 64f * 0.85f).Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE);
			}
			this.Tx.Txt(X.T2K(str)).LetterSpacing(0.95f).Alpha(this.alpha_);
			if (this.B.Container != null)
			{
				this.Tx.StencilRef(this.B.Container.stencil_ref);
			}
			this.Tx.auto_condense = true;
			this.Tx.max_swidth_px = this.w * 64f - 20f;
			this.Tx.BorderCol(C32.d2c(4283780170U));
			this.default_title_width = this.Tx.get_swidth_px();
			IN.Pos(this.Tx.transform, -this.h * 0.07f, 0f, -0.003f);
			this.fine_flag = true;
			return this;
		}

		public override ButtonSkin WHPx(float _w, float _h)
		{
			base.WHPx(_w, _h);
			if (this.Tx != null)
			{
				this.setTitle(this.title);
			}
			return this;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = (float)(X.IntR(this.w * 64f * 0.5f) * 2);
			float num2 = (float)(X.IntR(this.h * 64f * 0.5f) * 2);
			float num3 = num * 0.5f;
			float num4 = num2 * 0.5f;
			this.Md.clear(false, false);
			this.MdStripe.clear(false, false);
			if (this.Tx != null)
			{
				ButtonSkinKeyConLabelNel.SetTxColor(ButtonSkinKeyConLabelNel.Col, this, 1f);
				this.Tx.Col(ButtonSkinKeyConLabelNel.Col.C);
			}
			uint num5 = 4293321691U;
			if (base.isLocked())
			{
				this.Md.Col = ButtonSkinKeyConLabelNel.Col.Set(2001554765).mulA(this.alpha_).C;
				this.Md.Rect(0f, 0f, num, num2, false);
				num5 = 0U;
			}
			else if (this.isPushDown())
			{
				this.Md.Col = ButtonSkinKeyConLabelNel.Col.Set(4293321691U).mulA(this.alpha_).C;
				this.Md.Rect(0f, 0f, num, num2, false);
				num5 = 4283780170U;
			}
			else if (base.isChecked())
			{
				this.MdStripe.Col = (this.Md.Col = ButtonSkinKeyConLabelNel.Col.Set(4293321691U).mulA(this.alpha_ * 0.08f).C);
				this.MdStripe.StripedM(0.7853982f, 20f, 0.5f, 4).Rect(0f, 0f, num - 4f, num2 - 4f, false).allocUv2(0, true);
				num5 = 0U;
			}
			else if (base.isHoveredOrPushOut())
			{
				this.Md.Col = ButtonSkinKeyConLabelNel.Col.Set(4293321691U).mulA((0.15f + 0.15f * X.COSIT(34f)) * this.alpha_).C;
				this.Md.Rect(0f, 0f, num, num2, false);
			}
			if (num5 >= 16777216U)
			{
				this.Md.Col = ButtonSkinKeyConLabelNel.Col.Set(num5).mulA(this.alpha_).C;
				this.Md.Tri(0, 1, 2, false);
				int num6 = 5;
				this.Md.PosD(num3 - 8f, -num4 + 3f, null).PosD(num3 - 8f - (float)num6, -num4 + 3f + (float)num6, null).PosD(num3 - 8f + (float)num6, -num4 + 3f + (float)num6, null);
			}
			if (!base.isLocked() && !this.isPushDown())
			{
				this.Md.Col = ButtonSkinKeyConLabelNel.Col.Set(4293321691U).mulA(this.alpha_).C;
				this.Md.Line(-num3, -num4, num3, -num4, 1f, false, 0f, 0f);
			}
			this.Md.updateForMeshRenderer(false);
			return base.Fine();
		}

		public static C32 SetTxColor(C32 _Col, ButtonSkin B, float normalmode_alpha = 1f)
		{
			_Col.Set(4293321691U);
			if (B.isLocked())
			{
				_Col.Set(4287795858U);
			}
			else if (B.isPushDown())
			{
				_Col.Set(4283780170U);
			}
			else if (B.isHoveredOrPushOut())
			{
				_Col.blend(4283780170U, 0.3f + 0.3f * X.COSIT(34f));
			}
			else
			{
				_Col.mulA(normalmode_alpha);
			}
			return _Col;
		}

		public override void setEnable(bool f)
		{
			base.setEnable(f);
			if (this.Tx != null)
			{
				this.Tx.enabled = f;
			}
		}

		public override float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ != value)
				{
					base.alpha = value;
					if (this.Tx != null)
					{
						this.Tx.Alpha(this.alpha_);
					}
				}
			}
		}

		private TextRenderer Tx;

		protected MeshDrawer Md;

		protected MeshDrawer MdStripe;

		public static C32 Col;

		public const float SPRITE_Y = 1f;
	}
}
