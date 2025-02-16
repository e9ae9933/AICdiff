using System;
using XX;

namespace nel
{
	public class ButtonSkinNormalNel : ButtonSkin
	{
		public uint main_color
		{
			get
			{
				return this.nncolor.main_color;
			}
			set
			{
				this.nncolor.main_color = value;
			}
		}

		public uint main_color_locked
		{
			get
			{
				return this.nncolor.main_color_locked;
			}
			set
			{
				this.nncolor.main_color_locked = value;
			}
		}

		public uint stripe_color
		{
			get
			{
				return this.nncolor.stripe_color;
			}
			set
			{
				this.nncolor.stripe_color = value;
			}
		}

		public uint push_color
		{
			get
			{
				return this.nncolor.push_color;
			}
			set
			{
				this.nncolor.push_color = value;
			}
		}

		public uint push_color_sub
		{
			get
			{
				return this.nncolor.push_color_sub;
			}
			set
			{
				this.nncolor.push_color_sub = value;
			}
		}

		public ButtonSkinNormalNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.MdStripe = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshStriped.shader, base.container_stencil_ref));
			IN.setZ(this.MMRD.GetGob(this.MdStripe).transform, -0.0061f);
			this.FD_DrawHover = new Action(this.drawHover);
			this.w = ((_w > 0f) ? _w : 190f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 32f) * 0.015625f;
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
				if (this.Tx != null)
				{
					this.Tx.use_valotile = value;
				}
			}
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			if (this.Tx == null)
			{
				this.Tx = IN.CreateGob(this.Gob, "-text").AddComponent<TextRenderer>();
				this.Tx.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE);
				if (this.use_valotile_)
				{
					this.Tx.use_valotile = this.use_valotile_;
				}
			}
			this.Tx.html_mode = this.html_mode;
			this.Tx.Txt(X.T2K(str)).LetterSpacing(0.95f).Alpha(this.alpha_)
				.SizeFromHeight(this.h * 64f * 0.7f, 0.125f);
			this.Tx.auto_condense = true;
			float num = ((this.left_daia_size > 0f && this.w != this.h) ? (this.h * X.NI(0.12f, 0.24f, this.left_daia_size)) : 0f);
			this.Tx.max_swidth_px = (this.w - num - this.h * 1.15f) * 64f - 14f;
			if (this.B.Container != null && this.B.Container.stencil_ref >= 0)
			{
				this.Tx.StencilRef(this.B.Container.stencil_ref);
			}
			this.default_title_width = this.Tx.get_swidth_px();
			IN.Pos(this.Tx.transform, num, 0f, -0.0065f);
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
			this.Md.clear(false, false);
			this.MdStripe.clear(false, false);
			if (base.isChecked())
			{
				this.drawChecked();
			}
			ButtonSkinNormalNel.drawS(this, this.Md, this.Tx, this.nncolor, num, num2, this.left_daia_size, this.FD_DrawHover, this.hold_level_);
			this.Md.updateForMeshRenderer(false);
			return base.Fine();
		}

		public static void drawS(ButtonSkin Sk, MeshDrawer Md, TextRenderer Tx, ButtonSkinNormalNel.NNColor nncolor, float w, float h, float left_daia_size, Action FD_DrawHover = null, float hold_level = 0f)
		{
			w = (float)(X.IntR(w * 0.5f) * 2);
			h = (float)(X.IntR(h * 0.5f) * 2);
			float num = w * 0.5f;
			float num2 = h * 0.5f;
			float alpha = Sk.alpha;
			uint main_color = nncolor.main_color;
			uint main_color_locked = nncolor.main_color_locked;
			uint push_color = nncolor.push_color;
			uint push_color_sub = nncolor.push_color_sub;
			if (Tx != null)
			{
				ButtonSkinNormalNel.Col.Set(main_color);
				if (Sk.isLocked())
				{
					ButtonSkinNormalNel.Col.Set(main_color);
				}
				else if (Sk.isPushDown())
				{
					ButtonSkinNormalNel.Col.Set(push_color);
				}
				else if (Sk.isHoveredOrPushOut())
				{
					ButtonSkinNormalNel.Col.blend(push_color_sub, 0.2f + 0.2f * X.COSIT(34f));
				}
				Tx.Col(ButtonSkinNormalNel.Col.C);
				if (!Sk.isLocked() && (Sk.isHoveredOrPushOut() || Sk.isChecked()))
				{
					Tx.BorderCol(C32.d2c(Sk.isPushed() ? main_color : push_color));
				}
				else
				{
					Tx.BorderCol(MTRX.ColTrnsp);
				}
			}
			if (Sk.isLocked())
			{
				Md.Col = Md.ColGrd.Set(main_color_locked).mulA(alpha).C;
				Md.NelBanner(0f, 0f, w, h, 0f, true, false, 0f, 0f, 0f);
				return;
			}
			if (Sk.isPushDown())
			{
				Md.Col = Md.ColGrd.Set(main_color).mulA(alpha).C;
				Md.NelBanner(0f, 0f, w - 4f, h - 4f, 0f, true, false, 0f, 0f, 0f);
				Md.Col = Md.ColGrd.Set(push_color).mulA(alpha).C;
				if (left_daia_size > 0f && w != h)
				{
					float num3 = (float)X.IntR((num2 - 2f) * ((hold_level < 0f) ? 0.7f : X.NIL(0.2f, 0.97f, hold_level, 1f)) * left_daia_size);
					float num4 = (float)X.IntR(num2 - 2f - num3);
					Md.Daia(-num + 2f + num4 + num3, 0f, num3 * 2f, num3 * 2f, false);
					Md.GT(-num + 2f + num4 * 2f + num3 + num3 * 0.11f, 0f, num2 - 2f, h - 4f, 4f, false, 0f, 0f);
					return;
				}
			}
			else
			{
				Md.Col = Md.ColGrd.Set(main_color).mulA(alpha).C;
				float num5 = (float)(X.IntR(num2 * 1.2f) + 3) * left_daia_size;
				float num6 = 2f;
				if (Sk.isHoveredOrPushOut())
				{
					if (FD_DrawHover != null)
					{
						FD_DrawHover();
					}
					Md.NelBanner(0f, 0f, w + 4f, h + 4f, 2f, false, false, 0f, 0f, 0f);
					num5 += 2f;
					num6 = 1f;
				}
				else
				{
					if (Sk.isChecked())
					{
						if (FD_DrawHover != null)
						{
							FD_DrawHover();
						}
						num5 += 2f;
						num6 = 1f;
					}
					Md.NelBanner(0f, 0f, w, h, 1f, false, false, 0f, 0f, 0f);
				}
				if (left_daia_size > 0f)
				{
					float num7 = num5 - num2;
					float num8 = -num - num7 + num5;
					Md.Daia3(num8, 0f, num5 * 2f, num5 * 2f, num6, num6, false);
					if (hold_level > 0f)
					{
						float num9 = (num5 - 3f) * (0.05f + hold_level * 0.95f);
						Md.Col = C32.MulA(Md.Col, 0.7f + 0.3f * X.COSIT(34f));
						Md.Daia3(num8, 0f, num9 * 2f, num9 * 2f, 0f, 0f, false);
					}
				}
			}
		}

		protected virtual void drawChecked()
		{
		}

		protected virtual void drawHover()
		{
			if (this.stripe_color >= 16777216U)
			{
				float num = (float)(X.IntR(this.w * 64f * 0.5f) * 2);
				float num2 = (float)(X.IntR(this.h * 64f * 0.5f) * 2);
				this.MdStripe.Col = this.MdStripe.ColGrd.Set(this.stripe_color).mulA(this.alpha_).C;
				this.MdStripe.uvRectN(X.Cos(0.7853982f), X.Sin(-0.7853982f));
				this.MdStripe.allocUv2(6, false).Uv2(3f, 0.5f, false);
				this.MdStripe.NelBanner(0f, 0f, num - this.mask_margin * 2f, num2 - this.mask_margin * 2f, 0f, true, false, 0f, 0f, 0f);
				this.MdStripe.allocUv2(0, true);
				this.MdStripe.updateForMeshRenderer(false);
			}
		}

		public override void setEnable(bool f)
		{
			base.setEnable(f);
			if (this.Tx != null)
			{
				this.Tx.enabled = f;
			}
		}

		public float hold_level
		{
			get
			{
				return this.hold_level_;
			}
			set
			{
				if (this.hold_level_ != value)
				{
					this.hold_level_ = value;
					this.fine_flag = true;
				}
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

		private float hold_level_ = -1f;

		protected MeshDrawer Md;

		protected MeshDrawer MdStripe;

		public const float DEFAULT_W = 190f;

		public const float DEFAULT_W_WIDE = 246f;

		public const float DEFAULT_H = 32f;

		public static C32 Col = new C32();

		public const float ICON_W = 16f;

		public const float SPRITE_Y = 1f;

		public float mask_margin = 3f;

		public float left_daia_size = 1f;

		public ButtonSkinNormalNel.NNColor nncolor = ButtonSkinNormalNel.NNColor.NNcolor_default;

		private Action FD_DrawHover;

		public struct NNColor
		{
			public NNColor(uint _main_color, uint _main_color_locked, uint _stripe_color, uint _push_color, uint _push_color_sub)
			{
				this.main_color = _main_color;
				this.main_color_locked = _main_color_locked;
				this.stripe_color = _stripe_color;
				this.push_color = _push_color;
				this.push_color_sub = _push_color_sub;
			}

			public static ButtonSkinNormalNel.NNColor NNcolor_default
			{
				get
				{
					return new ButtonSkinNormalNel.NNColor(4283780170U, 4288057994U, 542461002U, 4293321691U, 4291611332U);
				}
			}

			public uint main_color;

			public uint main_color_locked;

			public uint stripe_color;

			public uint push_color;

			public uint push_color_sub;
		}
	}
}
