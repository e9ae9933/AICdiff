using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinCheckBox : ButtonSkin
	{
		public ButtonSkinCheckBox(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.WHPx((_w == 0f) ? 140f : _w, (_h == 0f) ? 14f : _h);
			this.MdCheck = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.PFCheck = MTRX.PFCheckBoxChecked;
		}

		public void setScale(float _scale)
		{
			this.scale = _scale;
			this.WHPx(this.w * 64f, this.h * 64f);
			this.fine_flag = true;
		}

		public override ButtonSkin WHPx(float _wpx, float _hpx)
		{
			base.WHPx(_wpx, _hpx);
			this.box_w = X.Mn(14f, _hpx / this.scale);
			this.box_wh = this.box_w / 2f;
			return this;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			this.Md.clear(false, false);
			this.Md.Identity().Scale(this.scale, this.scale, false);
			this.MdCheck.Identity().Scale(this.scale, this.scale, false);
			C32 c = MTRX.cola.White();
			if (this.MdSpr != null)
			{
				this.MdSpr.Identity().Scale(this.scale, this.scale, false);
			}
			if (this.isPushDown())
			{
				this.Md.TranslateP(this.scale * 2f, -this.scale * 2f, false);
				this.MdCheck.TranslateP(this.scale * 2f, -this.scale * 2f, false);
				if (this.MdSpr != null)
				{
					this.MdSpr.TranslateP(this.scale * 2f, -this.scale * 2f, false);
				}
			}
			float num = -this.swidth / this.scale * 0.5f;
			c.Set(this.border_col);
			if (this.isPushDown())
			{
				c.Set(this.border_col_pushdown);
			}
			this.Md.Col = c.mulA(this.alpha_).C;
			this.DrawDoughnut(this.Md, num, true);
			c.Set(this.inner_col);
			if (this.isPushDown())
			{
				c.Set(this.inner_col_pushdown);
			}
			else if (base.isHoveredOrPushOut())
			{
				c.Set(this.inner_col_hovered);
			}
			this.Md.Col = c.mulA(this.alpha_).C;
			this.DrawDoughnut(this.Md, num, false);
			this.Md.updateForMeshRenderer(false);
			this.MdCheck.Col = this.Md.Col;
			if (base.isChecked())
			{
				this.MdCheck.RotaPF(num + this.box_wh, 0f, 1f, 1f, 0f, this.PFCheck, false, false, false, uint.MaxValue, false, 0);
			}
			this.MdCheck.updateForMeshRenderer(false);
			if (this.MdSpr != null)
			{
				c.Set(2952790016U);
				if (this.isPushDown())
				{
					c.Set(4289586406U);
				}
				this.MdSpr.allocUv23(64, false).Uv23(c.mulA(this.alpha_).C, false);
				this.MdSpr.Col = this.Md.Col;
				if (this.default_title_width > 0f)
				{
					this.DrawTitleSprite(this.MdSpr, -num + this.box_w + 4f + this.default_title_width * 0.5f + this.Vdefault_title_shift.x, this.Vdefault_title_shift.y, 1f);
				}
				this.MdSpr.allocUv23(0, true);
				this.MdSpr.updateForMeshRenderer(false);
			}
			return base.Fine();
		}

		protected virtual void DrawDoughnut(MeshDrawer Md, float pleft, bool black_border)
		{
			if (!black_border)
			{
				Md.RectDoughnut(pleft + this.box_wh, 0f, this.box_w - 2f, this.box_w - 2f, pleft + this.box_wh, 0f, this.box_w - 4f, this.box_w - 4f, false, 0f, 0f, false);
				return;
			}
			if (this.fill_box_inner)
			{
				Md.Rect(pleft + this.box_wh, 0f, this.box_w, this.box_w, false);
				return;
			}
			Md.RectDoughnut(pleft + this.box_wh, 0f, this.box_w, this.box_w, pleft + this.box_wh, 0f, this.box_w - 6f, this.box_w - 6f, false, 0f, 0f, false);
		}

		public override bool canClickable(Vector2 PosU)
		{
			float num = X.Mx(this.w * 64f, this.swidth);
			PosU += new Vector2((num - this.swidth) / 2f * 0.015625f, 0f);
			return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), (num + 4f) * 0.015625f, (this.sheight + 4f) * 0.015625f);
		}

		public override ButtonSkin setTitle(string str)
		{
			base.setTitle(str);
			this.fine_flag = true;
			this.makeDefaultTitleString(str, ref this.MdSpr, BLEND.NORMALBORDER8);
			return this;
		}

		public override float swidth
		{
			get
			{
				return this.box_w * this.scale + 4f + this.default_title_width;
			}
		}

		public float title_scale
		{
			get
			{
				if (this.scale < 2f)
				{
					return 1f;
				}
				return 0.5f;
			}
		}

		public override float sheight
		{
			get
			{
				return X.Mx(this.fix_h, this.box_w * this.scale);
			}
		}

		protected MeshDrawer Md;

		private MeshDrawer MdTitle;

		private MeshDrawer MdCheck;

		private MeshDrawer MdSpr;

		public uint border_col = 2952790016U;

		public uint border_col_pushdown = 4289586406U;

		public uint inner_col = uint.MaxValue;

		public uint inner_col_pushdown = 4283586759U;

		public uint inner_col_hovered = 4290099452U;

		protected PxlFrame PFCheck;

		protected float scale = 2f;

		public float fix_h;

		protected float box_w;

		protected float box_wh;

		protected bool fill_box_inner;

		protected const float title_margin = 4f;
	}
}
