using System;
using UnityEngine;

namespace XX
{
	public class ButtonSkinSc : ButtonSkin
	{
		public ButtonSkinSc(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.shadow_shift = 0.0234375f;
			this.w = ((_w > 0f) ? _w : 186f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 30f) * 0.015625f;
			if (ButtonSkinSc.Col == null)
			{
				ButtonSkinSc.Col = new C32();
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
			this.Md.base_z = 0.001f;
			ButtonSkinSc.Col.Set(3716780472U);
			if (base.isLocked())
			{
				ButtonSkinSc.Col.Set(2861731470U);
			}
			else if (this.isPushDown())
			{
				ButtonSkinSc.Col.blend(4286885097U, 0.5f + 0.5f * X.COSIT(34f));
			}
			this.Md.Col = ButtonSkinSc.Col.mulA(this.alpha_).C;
			float num = 0.0078125f;
			this.Md.ButtonBanner(this.shadow_shift, -this.shadow_shift, this.w + num, this.h + num, true);
			float num2 = 1.5707964f;
			float num3 = this.h * 0.45f;
			if (base.isPushed() || base.isFocused())
			{
				num2 = (float)(-(float)(IN.totalframe % 68)) / 68f * 6.2831855f;
			}
			this.Md.Star(this.shadow_shift + this.star_x, -this.shadow_shift, num3, num2, 5, 0.45f, 0f, true, 0f, 0f);
			this.Md.base_z = 0f;
			float px_sprite_center_x = this.px_sprite_center_x;
			float num4 = this.default_title_width;
			if (this.meshicon_name_ != "")
			{
				num4 += 16f;
			}
			float num5 = (float)((this.swidth - this.h - 22f - ((this.meshicon_name_ != "") ? 20f : 0f) - this.default_title_width * 2f < 0f) ? 1 : 2);
			if (this.MdSpr != null)
			{
				ButtonSkinSc.Col.White();
				if (base.isLocked())
				{
					ButtonSkinSc.Col.Set(3439329279U);
				}
				else if (base.isChecked() && !base.isFocused() && !base.isPushed())
				{
					ButtonSkinSc.Col.Set(4282083805U);
				}
				this.MdSpr.Col = ButtonSkinSc.Col.mulA(this.alpha_).C;
				this.MdSpr.base_z = 0f;
				this.DrawTitleSprite(this.MdSpr, px_sprite_center_x + (num4 - this.default_title_width + this.Vdefault_title_shift.x) * num5 / 2f + 2f, this.Vdefault_title_shift.y * num5 - 2f, num5);
			}
			if (base.isLocked() || this.isPushDown())
			{
				ButtonSkinSc.Col.White();
				if (base.isLocked())
				{
					ButtonSkinSc.Col.Set(3439329279U);
				}
				this.Md.Col = ButtonSkinSc.Col.mulA(this.alpha_).C;
				if (this.meshicon_name_ != "")
				{
					Shape.DrawMeshIcon(this.Md, px_sprite_center_x + (-num4 + 16f) / 2f * num5, this.Vdefault_title_shift.y * num5, 8f * num5, this.meshicon_name_, 0f);
				}
			}
			else
			{
				ButtonSkinSc.Col.Set(this.base_color);
				if (base.isChecked() && base.isFocused())
				{
					ButtonSkinSc.Col.Set(4287928319U).blend(4283604479U, 0.5f + 0.5f * X.COSIT(34f));
				}
				else if (base.isHoveredOrPushOut())
				{
					ButtonSkinSc.Col.Set(4290047743U).blend(4288282586U, 0.5f + 0.5f * X.COSIT(34f));
				}
				else if (base.isChecked())
				{
					ButtonSkinSc.Col.Set(4287928319U);
				}
				this.Md.Col = ButtonSkinSc.Col.mulA(this.alpha_).C;
				this.Md.ButtonBanner(-this.shadow_shift, this.shadow_shift, this.w + num, this.h + num, true);
				ButtonSkinSc.Col.Set(4290753769U);
				if (base.isChecked())
				{
					ButtonSkinSc.Col.White();
				}
				if (base.isFocused())
				{
					ButtonSkinSc.Col.blend(4291481599U, 0.5f + 0.5f * X.COSIT(34f));
				}
				this.Md.Col = ButtonSkinSc.Col.mulA(this.alpha_).C;
				this.Md.Star(-this.shadow_shift + this.star_x, this.shadow_shift, num3, num2, 5, 0.45f, 0f, true, 0f, 0f);
				if (this.meshicon_name_ != "")
				{
					string meshicon_name_ = this.meshicon_name_;
					if (meshicon_name_ != null && meshicon_name_ == "lock")
					{
						this.Md.Col = ButtonSkinSc.Col.Set(4293295228U).C;
					}
					Shape.DrawMeshIcon(this.Md, px_sprite_center_x + (-num4 + 16f) / 2f * num5, this.Vdefault_title_shift.y * num5, 8f * num5, this.meshicon_name_, 0f);
				}
				if (this.MdSpr != null)
				{
					ButtonSkinSc.Col.Set(4282467420U);
					if (base.isFocused())
					{
						ButtonSkinSc.Col.blend(uint.MaxValue, 0.3f + 0.3f * X.COSIT(34f));
					}
					else if (base.isChecked() && !base.isFocused() && !base.isPushed())
					{
						ButtonSkinSc.Col.Set(4289193978U);
					}
					this.MdSpr.Col = ButtonSkinSc.Col.mulA(this.alpha_).C;
					this.MdSpr.base_z = -0.0001f;
					this.DrawTitleSprite(this.MdSpr, px_sprite_center_x + (num4 - this.default_title_width + this.Vdefault_title_shift.x) * num5 / 2f, this.Vdefault_title_shift.y * num5, num5);
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
			base.setTitle(str);
			this.fine_flag = true;
			this.makeDefaultTitleString(str, ref this.MdSpr, BLEND._MAX);
			return this;
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

		protected MeshDrawer Md;

		protected bool auto_update = true;

		protected bool auto_update_spritemesh = true;

		private MeshDrawer MdSpr;

		public const float DEFAULT_W = 186f;

		public const float DEFAULT_W_WIDE = 246f;

		public const float DEFAULT_H = 30f;

		protected readonly float shadow_shift;

		public static C32 Col;

		public const float ICON_W = 16f;

		public const float SPRITE_Y = 1f;
	}
}
