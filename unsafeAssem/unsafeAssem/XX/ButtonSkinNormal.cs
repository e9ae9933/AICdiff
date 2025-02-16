using System;
using UnityEngine;

namespace XX
{
	public class ButtonSkinNormal : ButtonSkin
	{
		public ButtonSkinNormal(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.shadow_shift = 0.0234375f;
			this.w = ((_w > 0f) ? _w : 140f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 24f) * 0.015625f;
			if (ButtonSkinNormal.Col == null)
			{
				ButtonSkinNormal.Col = new C32();
			}
			this.fine_continue_flags = 5U;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			this.Md.clear(false, false);
			if (this.Tx != null)
			{
				ButtonSkinNormal.Col.Set(4282795590U);
				if (base.isLocked())
				{
					ButtonSkinNormal.Col.Set(4291414473U);
				}
				else if (this.isPushDown())
				{
					ButtonSkinNormal.Col.Set(uint.MaxValue);
				}
				else if (base.isHoveredOrPushOut())
				{
					ButtonSkinNormal.Col.blend(4278468729U, 0.5f + 0.5f * X.COSIT(34f));
				}
				this.Tx.Col(ButtonSkinNormal.Col.C);
				IN.Pos(this.Tx.transform, this.isPushDown() ? this.shadow_shift : 0f, 0.03125f - (this.isPushDown() ? this.shadow_shift : 0f), -0.01f);
			}
			this.Md.base_z = 0.001f;
			ButtonSkinNormal.Col.Set(3716780472U);
			if (base.isLocked())
			{
				ButtonSkinNormal.Col.Set(2861731470U);
			}
			else if (this.isPushDown())
			{
				ButtonSkinNormal.Col.blend(4286885097U, 0.5f + 0.5f * X.COSIT(34f));
			}
			this.Md.Col = ButtonSkinNormal.Col.C;
			float num = 0.0078125f;
			this.Md.KadomaruRect(this.shadow_shift, -this.shadow_shift, this.w + num, this.h + num, this.h, 0f, true, 0f, 0f, false);
			this.Md.base_z = 0f;
			float px_sprite_center_x = this.px_sprite_center_x;
			float num2 = this.default_title_width;
			if (this.meshicon_name_ != "")
			{
				num2 += 16f;
			}
			float num3 = (float)((this.swidth - this.h - 22f - ((this.meshicon_name_ != "") ? 20f : 0f) - this.default_title_width * 2f < 0f) ? 1 : 2);
			if (this.MdSpr != null)
			{
				ButtonSkinNormal.Col.White();
				if (base.isLocked())
				{
					ButtonSkinNormal.Col.Set(3439329279U);
				}
				else if (base.isChecked() && !base.isFocused() && !base.isPushed())
				{
					ButtonSkinNormal.Col.Set(4282083805U);
				}
				this.MdSpr.Col = ButtonSkinNormal.Col.mulA(this.alpha_).C;
				this.MdSpr.base_z = 0f;
				this.DrawTitleSprite(this.MdSpr, px_sprite_center_x + (num2 - this.default_title_width + this.Vdefault_title_shift.x) * num3 / 2f + 2f, this.Vdefault_title_shift.y * num3 - 2f, num3);
			}
			if (base.isLocked() || this.isPushDown())
			{
				ButtonSkinNormal.Col.White();
				if (base.isLocked())
				{
					ButtonSkinNormal.Col.Set(3439329279U);
				}
				this.Md.Col = ButtonSkinNormal.Col.mulA(this.alpha_).C;
				if (this.meshicon_name_ != "")
				{
					this.Md.MIcon(px_sprite_center_x + (-num2 + 16f) / 2f * num3, this.Vdefault_title_shift.y * num3, 8f * num3, this.meshicon_name_, 0f);
				}
			}
			else
			{
				ButtonSkinNormal.Col.Set(this.base_color);
				if (base.isChecked() && base.isFocused())
				{
					ButtonSkinNormal.Col.Set(4287928319U).blend(4283604479U, 0.5f + 0.5f * X.COSIT(34f));
				}
				else if (base.isHoveredOrPushOut())
				{
					ButtonSkinNormal.Col.Set(4290047743U).blend(4288282586U, 0.5f + 0.5f * X.COSIT(34f));
				}
				else if (base.isChecked())
				{
					ButtonSkinNormal.Col.Set(4287928319U);
				}
				this.Md.Col = ButtonSkinNormal.Col.mulA(this.alpha_).C;
				this.Md.KadomaruRect(-this.shadow_shift, this.shadow_shift, this.w + num, this.h + num, this.h, 0f, true, 0f, 0f, false);
				if (this.meshicon_name_ != "")
				{
					string meshicon_name_ = this.meshicon_name_;
					if (meshicon_name_ != null && meshicon_name_ == "lock")
					{
						this.Md.Col = ButtonSkinNormal.Col.Set(4293295228U).C;
					}
					this.Md.MIcon(px_sprite_center_x + (-num2 + 16f) / 2f * num3, this.Vdefault_title_shift.y * num3, 8f * num3, this.meshicon_name_, 0f);
				}
				if (this.MdSpr != null)
				{
					ButtonSkinNormal.Col.Set(4282467420U);
					if (base.isFocused())
					{
						ButtonSkinNormal.Col.blend(uint.MaxValue, 0.3f + 0.3f * X.COSIT(34f));
					}
					else if (base.isChecked() && !base.isFocused() && !base.isPushed())
					{
						ButtonSkinNormal.Col.Set(4289193978U);
					}
					this.MdSpr.Col = ButtonSkinNormal.Col.mulA(this.alpha_).C;
					this.MdSpr.base_z = -0.0001f;
					this.DrawTitleSprite(this.MdSpr, px_sprite_center_x + (num2 - this.default_title_width + this.Vdefault_title_shift.x) * num3 / 2f, this.Vdefault_title_shift.y * num3, num3);
				}
			}
			if (this.auto_update)
			{
				this.Md.updateForMeshRenderer(false);
			}
			if (this.auto_update_spritemesh && this.MdSpr != null)
			{
				this.MdSpr.updateForMeshRenderer(false);
			}
			return base.Fine();
		}

		public override bool canClickable(Vector2 PosU)
		{
			return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), this.w + this.shadow_shift * 2f, this.h + this.shadow_shift * 2f);
		}

		public override void setEnable(bool f)
		{
			base.setEnable(f);
			if (this.Tx != null)
			{
				this.Tx.enabled = f;
			}
		}

		protected override bool makeDefaultTitleString(string str, ref MeshDrawer MdSpr, BLEND blnd = BLEND._MAX)
		{
			bool flag = base.makeDefaultTitleString(str, ref MdSpr, blnd);
			if (flag)
			{
				this.Vdefault_title_shift.y = this.Vdefault_title_shift.y - 4f;
			}
			return flag;
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
			this.title = str;
			if (this.Tx == null)
			{
				this.Tx = base.MakeTx("-text");
				this.Tx.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE);
			}
			if (this.B.Container != null)
			{
				this.Tx.StencilRef(this.B.Container.stencil_ref);
			}
			this.Tx.Txt(X.T2K(str)).SizeFromHeight(this.sheight * 0.5f, 0.125f).LetterSpacing(0.95f)
				.Alpha(this.alpha_);
			this.Tx.auto_condense = (this.Tx.html_mode = true);
			this.Tx.max_swidth_px = this.w * 64f - 10f;
			this.default_title_width = this.Tx.get_swidth_px();
			this.fine_flag = true;
			return this;
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

		public float sprite_center_x
		{
			get
			{
				return this.w * 0.5f - (this.w - this.h) * 0.56f;
			}
		}

		public float px_sprite_center_x
		{
			get
			{
				return this.sprite_center_x * 64f;
			}
		}

		public float star_x
		{
			get
			{
				return -this.w * 0.5f + this.h * 0.5f;
			}
		}

		private TextRenderer Tx;

		protected MeshDrawer Md;

		protected bool auto_update = true;

		protected bool auto_update_spritemesh = true;

		private MeshDrawer MdSpr;

		public const float DEFAULT_W = 140f;

		public const float DEFAULT_W_WIDE = 246f;

		public const float DEFAULT_H = 24f;

		protected readonly float shadow_shift;

		public static C32 Col;

		public const float ICON_W = 16f;

		public const float SPRITE_Y = 1f;
	}
}
