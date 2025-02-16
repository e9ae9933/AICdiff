using System;
using PixelLiner;

namespace XX
{
	public class ButtonSkinKadomaruIcon : ButtonSkin
	{
		public ButtonSkinKadomaruIcon(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.MdStripe = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshStriped.shader, base.container_stencil_ref));
			this.MdIco = base.makeMesh(null);
			this.w = ((_w > 0f) ? _w : 50f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 50f) * 0.015625f;
			if (ButtonSkinKadomaruIcon.Col == null)
			{
				ButtonSkinKadomaruIcon.Col = new C32();
			}
			this.fine_continue_flags = 9U;
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			this.PFMesh = MTRX.getPF(this.title);
			this.Img = null;
			this.fine_flag = true;
			return this;
		}

		public PxlFrame PFMesh
		{
			get
			{
				return this.PFMesh_;
			}
			set
			{
				if (this.PFMesh_ == value)
				{
					return;
				}
				this.PFMesh_ = value;
				this.fineMI(MTRX.getMI(this.PFMesh));
			}
		}

		protected void fineMI(MImage _MI)
		{
			this.MI = _MI;
			if (this.MI != null)
			{
				this.MMRD.setMaterial(this.MdIco, this.MI.getMtr(base.container_stencil_ref), false);
			}
		}

		protected virtual C32 setIconColor()
		{
			return ButtonSkinKadomaruIcon.Col.Set(this.col_icon).mulA(base.isLocked() ? 0.5f : 1f);
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
			this.MdIco.clear(false, false);
			this.MdStripe.clear(false, false);
			this.drawIco();
			float num5 = ((this.radius >= 0f) ? this.radius : num4);
			if (base.isLocked())
			{
				this.Md.Col = ButtonSkinKadomaruIcon.Col.Set(this.col_lock).mulA(this.alpha_).C;
				this.Md.KadomaruRect(0f, 0f, num, num2, num5, 0f, false, 0f, 0f, false);
			}
			else if (this.isPushDown())
			{
				this.Md.Col = ButtonSkinKadomaruIcon.Col.Set(this.col_pushdown).mulA(this.alpha_).C;
				this.Md.KadomaruRect(0f, 0f, num, num2, num5, 2f, false, 0f, 0f, false);
			}
			else if (base.isHoveredOrPushOut())
			{
				this.Md.Col = ButtonSkinKadomaruIcon.Col.Set(base.isChecked() ? this.col_checked : this.col_hovered_or_pushout).blend(base.isChecked() ? this.col_checked : this.col_hovered_or_pushout2, 0.9f + 0.1f * X.COSIT(25f)).mulA(this.alpha_)
					.C;
				this.Md.KadomaruRect(0f, 0f, num, num2, num5, 0f, false, 0f, 0f, false);
				this.MdStripe.Col = ButtonSkinKadomaruIcon.Col.Set(this.col_stripe).mulA(0.2f + 0.1f * X.COSIT(25f)).mulA(this.alpha_)
					.C;
				this.MdStripe.StripedM(0.7853982f, 20f, 0.5f, 4).KadomaruRect(0f, 0f, num - 6f, num2 - 6f, num5 - 2f, 0f, false, 0f, 0f, false).allocUv2(0, true);
			}
			else
			{
				this.Md.Col = ButtonSkinKadomaruIcon.Col.Set(base.isChecked() ? this.col_checked : this.col_normal).mulA(this.alpha_).C;
				this.Md.KadomaruRect(0f, 0f, num, num2, num5, 0f, false, 0f, 0f, false);
			}
			if (this.Img == null && this.PFMesh_ == null)
			{
				this.Md.Col = ButtonSkinKadomaruIcon.Col.Set(this.col_stripe).mulA(this.alpha_).C;
				float num6 = num / 2f - num5 * 0.2f;
				this.Md.Line(-num6, -num6, num6, num6, 2f, false, 0f, 0f);
			}
			if (base.isChecked() && (!this.locked_checkmark_invisible || !base.isLocked()))
			{
				this.drawChecked(num3, num4);
			}
			this.MdIco.updateForMeshRenderer(false);
			this.Md.updateForMeshRenderer(false);
			this.MdStripe.updateForMeshRenderer(false);
			return base.Fine();
		}

		protected virtual void drawChecked(float wh, float hh)
		{
			if (base.isLocked())
			{
				ButtonSkinKadomaruIcon.Col.Set(3431499912U).mulA(this.alpha);
			}
			this.MdIco.Col = ButtonSkinKadomaruIcon.Col.C;
			this.MdIco.RotaPF(wh - 12f, -hh + 12f, 2f, 2f, 0f, MTRX.getPF("checked"), false, false, false, uint.MaxValue, false, 0);
		}

		public float icon_scale
		{
			get
			{
				return this.icon_scale_;
			}
			set
			{
				if (this.icon_scale == value)
				{
					return;
				}
				this.icon_scale_ = value;
				this.B.Fine(false);
			}
		}

		protected virtual void drawIco()
		{
			if (this.Img != null || this.PFMesh_ != null)
			{
				this.setIconColor();
				this.MdIco.Col = ButtonSkinKadomaruIcon.Col.mulA(this.alpha_).C;
				if (this.Img != null)
				{
					this.MdIco.initForImg(this.Img, 0).DrawScaleGraph(0f, 0f, this.icon_scale_, this.icon_scale_, null);
				}
				if (this.PFMesh_ != null)
				{
					this.MdIco.RotaPF(0f, 0f, this.icon_scale_, this.icon_scale_, 0f, this.PFMesh_, false, false, false, uint.MaxValue, false, 0);
				}
			}
		}

		protected MeshDrawer Md;

		protected MeshDrawer MdStripe;

		protected MeshDrawer MdIco;

		protected PxlImage Img;

		protected PxlFrame PFMesh_;

		protected MImage MI;

		private float icon_scale_ = 1f;

		public static C32 Col;

		public const float DEFAULT_W = 50f;

		public const float DEFAULT_H = 50f;

		public bool locked_checkmark_invisible;

		public float radius = -1f;

		public uint col_lock = 4286019447U;

		public uint col_pushdown = 4278201224U;

		public uint col_checked = 4278211071U;

		public uint col_hovered_or_pushout = 4288787156U;

		public uint col_hovered_or_pushout2 = 3433149140U;

		public uint col_normal = 4293717228U;

		public uint col_stripe = 1431326800U;

		public uint col_icon = uint.MaxValue;
	}
}
