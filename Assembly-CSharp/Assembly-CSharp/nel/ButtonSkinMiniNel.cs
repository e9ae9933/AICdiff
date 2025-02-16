using System;
using PixelLiner;
using XX;

namespace nel
{
	public class ButtonSkinMiniNel : ButtonSkin
	{
		public ButtonSkinMiniNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.MdStripe = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshStriped.shader, base.container_stencil_ref));
			this.MdIco = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.w = ((_w > 0f) ? _w : 28f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 28f) * 0.015625f;
			if (ButtonSkinMiniNel.Col == null)
			{
				ButtonSkinMiniNel.Col = new C32();
			}
			this.fine_continue_flags = 5U;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = (float)(X.IntR(this.w * 64f * 0.5f) * 2);
			float num2 = (float)(X.IntR(this.h * 64f * 0.5f) * 2);
			this.Md.clear(false, false);
			this.MdStripe.clear(false, false);
			if (base.isChecked())
			{
				this.drawChecked();
			}
			if (this.PF != null || base.isChecked())
			{
				this.MdIco.ColGrd.Set(this.main_color);
				if (base.isLocked())
				{
					this.MdIco.ColGrd.Set(this.main_color_locked);
				}
				else if (this.isPushDown())
				{
					this.MdIco.ColGrd.Set(this.push_color);
				}
				else if (base.isHoveredOrPushOut())
				{
					this.MdIco.ColGrd.blend(this.main_color_locked, 0.3f + 0.3f * X.COSIT(40f));
				}
				this.MdIco.Col = this.MdIco.ColGrd.C;
				if (this.PF != null)
				{
					this.MdIco.RotaPF(0f, 0f, 1f, 1f, 0f, this.PF, false, false, false, uint.MaxValue, false, 0);
				}
			}
			if (base.isLocked())
			{
				this.Md.Col = this.Md.ColGrd.Set(this.main_color_locked).mulA(this.alpha_).C;
				this.Md.Box(0f, 0f, num, num2, 0f, false);
			}
			else if (this.isPushDown())
			{
				this.Md.Col = this.Md.ColGrd.Set(this.main_color).mulA(this.alpha_).C;
				this.Md.Box(0f, 0f, num - 4f, num2 - 4f, 0f, false);
			}
			else
			{
				if (base.isChecked() && (this.check_fill_color & 4278190080U) != 0U)
				{
					this.Md.Col = this.Md.ColGrd.Set(this.check_fill_color).mulA(this.alpha_).C;
					this.Md.Box(0f, 0f, num, num2, 0f, false);
				}
				this.Md.Col = this.Md.ColGrd.Set(this.main_color).mulA(this.alpha_).C;
				if (base.isHoveredOrPushOut())
				{
					this.drawHover();
					this.Md.Box(0f, 0f, num + 4f, num2 + 4f, 2f, false);
				}
				else
				{
					this.Md.Box(0f, 0f, num, num2, 1f, false);
				}
			}
			if (base.isChecked())
			{
				this.drawChecked();
			}
			this.Md.updateForMeshRenderer(false);
			this.MdIco.updateForMeshRenderer(false);
			return base.Fine();
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			this.PF = MTRX.getPF(str);
			return this;
		}

		protected virtual void drawChecked()
		{
			float num = (float)X.IntR(this.w * 64f * 0.5f);
			float num2 = (float)X.IntR(this.h * 64f * 0.5f);
			this.MdIco.RotaPF(num - 5f, -num2 + 5f, 1f, 1f, 0f, MTRX.getPF("nel_check"), false, false, false, uint.MaxValue, false, 0);
		}

		protected virtual void drawHover()
		{
			if (this.stripe_color >= 16777216U)
			{
				float num = (float)(X.IntR(this.w * 64f * 0.5f) * 2);
				float num2 = (float)(X.IntR(this.h * 64f * 0.5f) * 2);
				this.MdStripe.Col = this.MdStripe.ColGrd.Set(this.stripe_color).mulA(this.alpha_).C;
				this.MdStripe.StripedM(0.7853982f, 20f, 0.5f, 4).Rect(0f, 0f, num - this.mask_margin * 2f, num2 - this.mask_margin * 2f, false).allocUv2(0, true);
				this.MdStripe.updateForMeshRenderer(false);
			}
		}

		protected MeshDrawer Md;

		protected MeshDrawer MdIco;

		protected MeshDrawer MdStripe;

		public const float DEFAULT_W = 28f;

		public const float DEFAULT_W_WIDE = 246f;

		public const float DEFAULT_H = 28f;

		public static C32 Col;

		public PxlFrame PF;

		public float mask_margin = 3f;

		public float left_daia_size = 1f;

		public uint check_fill_color = 4294966715U;

		public uint main_color = 4283780170U;

		public uint main_color_locked = 4288057994U;

		public uint stripe_color = 542461002U;

		public uint push_color = 4293321691U;

		public uint push_color_sub = 4291611332U;
	}
}
