using System;

namespace XX
{
	public class ButtonSkinCommand : ButtonSkin
	{
		public ButtonSkinCommand(aBtn _B, float _w, float _h)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.MdStripeB = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref));
			this.w = ((_w > 0f) ? _w : 300f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 40f) * 0.015625f;
			this.fine_continue_flags = 13U;
			if (ButtonSkinCommand.Col == null)
			{
				ButtonSkinCommand.Col = new C32();
			}
		}

		public override bool use_valotile
		{
			set
			{
				if (this.use_valotile == value)
				{
					return;
				}
				base.use_valotile = value;
				this.Tx.use_valotile = value;
			}
		}

		public override ButtonSkin WHPx(float _wpx, float _hpx)
		{
			base.WHPx(_wpx, _hpx);
			if (this.Tx != null && this.Tx.max_swidth_px != _wpx - 24f)
			{
				this.Tx.max_swidth_px = _wpx - 24f;
				this.Tx.Redraw(false);
			}
			return this;
		}

		public override ButtonSkin Fine()
		{
			float num = this.w * 64f;
			float num2 = this.h * 64f - 1f;
			float num3 = (base.isChecked() ? 1f : this.alpha_);
			bool flag = this.isPushDown();
			bool flag2 = true;
			bool flag3 = true;
			if (base.isLocked())
			{
				ButtonSkinCommand.Col.Set(1167694233);
				if (base.isHoveredOrPushOut())
				{
					ButtonSkinCommand.Col.blend(4284374691U, 0.6f + 0.4f * X.COSIT(40f));
				}
				else
				{
					flag3 = false;
				}
			}
			else if (flag)
			{
				ButtonSkinCommand.Col.Set(4286352383U).blend(4281085146U, 0.5f + 0.5f * X.COSIT(40f));
			}
			else if (base.isChecked())
			{
				ButtonSkinCommand.Col.Set(4283681279U).blend(4286709755U, 0.5f + 0.5f * X.COSIT(40f));
			}
			else
			{
				flag2 = base.isHoveredOrPushOut() && false;
			}
			if (flag2)
			{
				this.Md.Col = C32.MulA(ButtonSkinCommand.Col.C, num3);
				this.Md.KadomaruRect(0f, 0f, num - 2f, num2 - 2f, flag3 ? (num2 / 2f) : 0f, 0f, false, 0f, 0f, false);
			}
			if (flag || base.isHoveredOrPushOut())
			{
				if (!flag3)
				{
					this.Md.Col = C32.MulA(MTRX.ColWhite, num3 * 0.4f);
					this.Md.Rect(0f, 0f, num, num2, false);
				}
				else
				{
					this.MdStripeB.Col = C32.MulA(MTRX.ColWhite, num3);
					this.MdStripeB.ButtonKadomaruDashedM(0f, 0f, num - (float)(flag ? 2 : 0), num2 - (float)(flag ? 2 : 0), num2, 20, 1f, false, 0.5f, -1);
				}
			}
			float num4 = (float)(this.isPushDown() ? 2 : 0);
			IN.PosP(this.Tx.transform, num4, 1f - num4 + this.Tx.size * 0.25f, -0.08f);
			ButtonSkinCommand.Col.Set(uint.MaxValue);
			if (base.isLocked())
			{
				ButtonSkinCommand.Col.Set(2868903935U);
			}
			else if (this.isPushDown())
			{
				ButtonSkinCommand.Col.Set(4291750911U);
			}
			else if (base.isChecked())
			{
				ButtonSkinCommand.Col.Set(4278396506U);
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinCommand.Col.Set(uint.MaxValue);
			}
			this.Tx.Col(ButtonSkinCommand.Col.C);
			this.Tx.alpha = num3;
			if (base.isLocked())
			{
				this.Md.Col = C32.MulA(ButtonSkinCommand.Col.C, num3);
				this.Md.Line(-num * 0.45f, 0f, num * 0.45f, 0f, 2f, false, 0f, 0f);
			}
			if (TX.valid(this.flags) && ButtonSkinCommand.FnDrawAdditionalIcon != null)
			{
				ButtonSkinCommand.FnDrawAdditionalIcon(this, this.Md);
			}
			if (this.MdIco != null)
			{
				this.MdIco.updateForMeshRenderer(false);
			}
			this.Md.updateForMeshRenderer(false);
			this.MdStripeB.updateForMeshRenderer(false);
			return base.Fine();
		}

		protected override bool makeDefaultTitleString(string str, ref MeshDrawer MdSpr, BLEND blnd = BLEND._MAX)
		{
			this.setTitleText(str);
			return false;
		}

		public override ButtonSkin setTitle(string str)
		{
			base.setTitle(str);
			this.setTitleText(str);
			return this;
		}

		protected void setTitleText(string str)
		{
			if (this.Tx == null)
			{
				this.Tx = base.MakeTx("-text");
			}
			if (this.B.Container != null)
			{
				this.Tx.StencilRef(this.B.Container.stencil_ref);
			}
			str = TX.ReplaceTX(str, false);
			this.Tx.html_mode = true;
			this.Tx.Txt(str).Align(ALIGN.CENTER).SizeFromHeight(this.sheight * 0.7f, 0.125f)
				.Alpha(this.alpha_);
			this.Tx.auto_condense = true;
			this.Tx.max_swidth_px = 0f;
			this.default_title_width = this.Tx.get_swidth_px();
			this.fine_flag = true;
		}

		public float getFirstDrawnTitleWidth()
		{
			return this.default_title_width;
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

		public override void setEnable(bool f)
		{
			if (this.Tx != null)
			{
				this.Tx.enabled = f;
			}
			base.setEnable(f);
		}

		public MeshDrawer prepareIconMesh()
		{
			if (this.MdIco == null)
			{
				this.MdIco = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
			}
			return this.MdIco;
		}

		protected MeshDrawer Md;

		protected MeshDrawer MdIco;

		protected MeshDrawer MdStripeB;

		public static C32 Col;

		public const float DEFAULT_W = 300f;

		public const float DEFAULT_H = 40f;

		public const int row_margin = 24;

		private TextRenderer Tx;

		public string flags;

		public static Action<ButtonSkinCommand, MeshDrawer> FnDrawAdditionalIcon;
	}
}
