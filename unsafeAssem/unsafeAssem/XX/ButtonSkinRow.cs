using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinRow : ButtonSkin
	{
		public ButtonSkinRow(aBtn _B, float _w, float _h)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.MdStripe = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshStriped.shader, base.container_stencil_ref));
			this.MdStripeB = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref));
			this.w = ((_w > 0f) ? _w : 300f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 20f) * 0.015625f;
			this.fine_continue_flags = 1U;
			this.curs_level_x = 0.65f;
			this.curs_level_y = 0f;
			if (ButtonSkinRow.Col == null)
			{
				ButtonSkinRow.Col = new C32();
			}
		}

		public virtual void drawBox(MeshDrawer Md, float wpx, float hpx)
		{
			Md.Rect(0f, 0.5f, wpx, hpx, false);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 64f;
			float num2 = this.h * 64f - 1f;
			ButtonSkinRow.Col.Set(this.base_col_normal);
			if (base.isLocked())
			{
				if (this.isPushDown())
				{
					ButtonSkinRow.Col.Set(this.base_col_pushdown).blend(this.base_col_locked, 0.6f);
				}
				else if (base.isHoveredOrPushOut())
				{
					ButtonSkinRow.Col.Set(this.base_col_hovered0).blend(this.base_col_locked, 0.65f + 0.33f * X.COSIT(40f));
				}
				else
				{
					ButtonSkinRow.Col.Set(this.base_col_locked);
				}
			}
			else if (this.isPushDown())
			{
				ButtonSkinRow.Col.Set(this.base_col_pushdown);
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinRow.Col.Set(this.base_col_hovered0).blend(this.base_col_hovered1, 0.5f + 0.5f * X.COSIT(40f));
			}
			else if (base.isChecked())
			{
				ButtonSkinRow.Col.Set(this.base_col_checked0).blend(this.base_col_checked1, 0.5f + 0.5f * X.COSIT(40f));
			}
			this.Md.Col = ButtonSkinRow.Col.mulA(this.alpha_).C;
			this.drawBox(this.Md, num, num2);
			if (this.isPushDown())
			{
				this.MdStripe.Col = ButtonSkinRow.Col.Set(this.stripe_col_pushdown).mulA(this.alpha_).C;
				if (this.MdStripe.Col.a > 0)
				{
					this.MdStripe.StripedM(0.7853982f, 24f, 0.5f, 4);
					this.MdStripe.Rect(0f, 0.5f, num - 2f, num2 - 2f, false);
					this.MdStripe.allocUv2(0, true);
				}
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinRow.Col.Set(this.stripe_col_hovered);
				if (base.isLocked())
				{
					ButtonSkinRow.Col.blend(this.tx_col_normal, 0.75f);
				}
				this.MdStripeB.Col = ButtonSkinRow.Col.mulA(this.alpha_).C;
				if (this.MdStripeB.Col.a > 0)
				{
					this.MdStripeB.RectDashedM(0f, 0.5f, num, num2, X.IntC((num + num2) / 7f), 2f, 0.5f, false, false);
				}
			}
			float num3 = (float)(this.isPushDown() ? 2 : 0);
			float num4 = (float)((this.alignx_ == ALIGN.LEFT) ? this.row_left_px : ((this.alignx_ == ALIGN.RIGHT) ? (-(float)this.row_left_px) : 0));
			float num5 = this.h;
			float num6 = 0f;
			if (this.AIcons != null)
			{
				this.prepareIconMesh();
				this.MdIco.Col = this.MdIco.ColGrd.White().mulA(this.alpha_).multiply(C32.d2c(this.isPushDown() ? this.icon_col_pushdown : this.icon_col_normal), true)
					.C;
				if (!this.icon_top)
				{
					float num7 = (float)this.row_left_px;
					int count = this.AIcons.Count;
					for (int i = 0; i < count; i++)
					{
						PxlFrame pxlFrame = this.AIcons[i];
						this.MdIco.RotaPF(-num / 2f + num3 + num7 + (float)pxlFrame.pPose.width * this.icon_scale / 2f, -num3, this.icon_scale, this.icon_scale, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
						num7 += (float)pxlFrame.pPose.width * this.icon_scale + (float)this.ICON_MARGIN;
					}
					num4 = num7;
				}
				else
				{
					float num8 = 0f;
					int count2 = this.AIcons.Count;
					for (int j = 0; j < count2; j++)
					{
						PxlFrame pxlFrame2 = this.AIcons[j];
						num8 += (float)pxlFrame2.pPose.width * this.icon_scale + (float)((j > 0) ? this.ICON_MARGIN : 0);
						num6 = X.Mx((float)pxlFrame2.pPose.height * this.icon_scale, num6);
					}
					float num9 = ((this.alignx_ == ALIGN.LEFT) ? (-num / 2f + (float)this.row_left_px) : ((this.alignx_ == ALIGN.RIGHT) ? (num / 2f - (float)this.row_left_px - num8) : (0f - num8 / 2f)));
					float num10 = (num6 + (float)this.ICON_MARGIN) / 2f;
					for (int k = 0; k < count2; k++)
					{
						PxlFrame pxlFrame3 = this.AIcons[k];
						this.MdIco.RotaPF(num9 + (float)pxlFrame3.pPose.width * this.icon_scale / 2f, num10, this.icon_scale, this.icon_scale, 0f, pxlFrame3, false, false, false, uint.MaxValue, false, 0);
						num9 += (float)pxlFrame3.pPose.width * this.icon_scale + (float)this.ICON_MARGIN;
					}
				}
			}
			bool flag = false;
			this.fineDrawAdditional(ref num4, ref num6, ref flag);
			if (!flag)
			{
				num6 += (float)((num6 != 0f) ? this.ICON_MARGIN : 0);
				num5 -= num6 * 0.015625f;
				num6 *= -0.5f;
			}
			if (this.Tx != null)
			{
				Vector3 localPosition = this.Tx.transform.localPosition;
				localPosition.x = ((this.alignx_ == ALIGN.LEFT) ? (-this.w / 2f + num4 * 0.015625f) : ((this.alignx_ == ALIGN.CENTER) ? (num4 * 0.015625f * 0.5f) : (this.w / 2f - 0.09375f)));
				localPosition.y = num6 * 0.015625f;
				if (!flag)
				{
					localPosition.y += ((this.aligny_ == ALIGNY.BOTTOM) ? (-num5 / 2f + this.Tx.size * 0.2f * 0.015625f) : ((this.aligny_ == ALIGNY.MIDDLE) ? 0f : (num5 / 2f - this.Tx.size * 0.2f * 0.015625f)));
				}
				this.Tx.transform.localPosition = localPosition;
				float num11 = num - X.Mx(num4, (float)this.row_left_px) - (float)this.row_right_px;
				if (this.auto_fix_max_swidth && this.Tx.max_swidth_px != num11)
				{
					this.Tx.max_swidth_px = num11;
					this.Tx.redraw_flag = true;
				}
			}
			ButtonSkinRow.Col.Set(this.tx_col_normal);
			if (base.isLocked())
			{
				if (this.isPushDown())
				{
					ButtonSkinRow.Col.Set(this.tx_col_pushdown).blend(this.tx_col_locked, 0.5f);
				}
				else if (base.isHoveredOrPushOut())
				{
					ButtonSkinRow.Col.Set(this.tx_col_hovered).blend(this.tx_col_locked, 0.5f);
				}
				else
				{
					ButtonSkinRow.Col.Set(this.tx_col_locked);
				}
			}
			else if (this.isPushDown())
			{
				ButtonSkinRow.Col.Set(this.tx_col_pushdown);
			}
			else if (base.isHoveredOrPushOut())
			{
				ButtonSkinRow.Col.Set(this.tx_col_hovered);
			}
			else if (base.isChecked())
			{
				ButtonSkinRow.Col.Set(this.tx_col_checked);
			}
			if (this.Tx != null)
			{
				this.Tx.Alpha(this.text_alpha_ * this.alpha_).Col(ButtonSkinRow.Col.C);
			}
			if (base.isChecked())
			{
				this.drawCheckedIcon(num3);
			}
			this.RowFineAfter(num, num2);
			this.Md.updateForMeshRenderer(false);
			return base.Fine();
		}

		protected virtual void fineDrawAdditional(ref float shift_px_x, ref float shift_px_y, ref bool shift_y_abs)
		{
		}

		protected virtual void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
			ButtonSkinRow.Col.Set(uint.MaxValue);
			if (base.isLocked())
			{
				ButtonSkinRow.Col.Set(3431499912U);
			}
			this.Md.Col = C32.MulA(ButtonSkinRow.Col.C, this.alpha_);
			this.Md.CheckMark(this.w * 64f / 2f - 4f + sht_clk_pixel, -sht_clk_pixel, this.h * 64f * 0.8f, X.Mx(4f, this.h * 64f * 0.125f), false);
		}

		public void addIcon(PxlFrame PIcon, int index = -1)
		{
			if (PIcon == null)
			{
				return;
			}
			if (this.AIcons == null)
			{
				this.AIcons = new List<PxlFrame>();
			}
			if (index < 0)
			{
				this.AIcons.Add(PIcon);
			}
			else
			{
				this.AIcons.Insert(index, PIcon);
			}
			this.fine_flag = true;
		}

		public void clearIcon(int index = -1)
		{
			if (this.AIcons == null)
			{
				return;
			}
			if (index < 0)
			{
				this.AIcons.Clear();
			}
			else
			{
				PxlFrame pxlFrame = this.AIcons[index];
				this.AIcons.RemoveAt(index);
			}
			this.fine_flag = true;
		}

		protected virtual void RowFineAfter(float w, float h)
		{
			if (this.MdIco != null)
			{
				this.MdIco.updateForMeshRenderer(false);
			}
			this.MdStripe.updateForMeshRenderer(false);
			this.MdStripeB.updateForMeshRenderer(false);
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

		public string text_content
		{
			get
			{
				if (!(this.Tx != null))
				{
					return this.title;
				}
				return this.Tx.text_content;
			}
		}

		public ButtonSkinRow setTitleTextS(STB str)
		{
			if (this.Tx == null)
			{
				this.setTitleText(" ");
			}
			this.Tx.Txt(str);
			return this;
		}

		protected virtual void setTitleText(string str)
		{
			if (this.Tx == null)
			{
				this.Tx = base.MakeTx("-text");
				this.Tx.Align(ALIGN.LEFT);
				this.Tx.html_mode = true;
			}
			this.Tx.StencilRef(this.B.container_stencil_ref);
			if (str != null)
			{
				this.Tx.Txt(X.T2K(str));
			}
			this.Tx.Align(this.alignx_).AlignY(this.aligny_).Alpha(this.alpha_);
			if (this.alloc_auto_wrap && TX.isEnglishLang())
			{
				this.Tx.auto_wrap = true;
			}
			else
			{
				this.Tx.auto_wrap = false;
			}
			this.Tx.auto_condense = !this.Tx.auto_wrap;
			if (this.fix_text_size_ > 0f)
			{
				this.Tx.Size(this.fix_text_size_);
			}
			else
			{
				float num = 0f;
				if (this.AIcons != null)
				{
					int count = this.AIcons.Count;
					for (int i = 0; i < count; i++)
					{
						num = X.Mx((float)this.AIcons[i].pPose.height * this.icon_scale, num);
					}
					num += (float)this.ICON_MARGIN;
				}
				if (this.sheight > num)
				{
					this.Tx.SizeFromHeight((this.sheight - num) * 0.7f * this.text_scale, 0.125f);
				}
				else
				{
					this.Tx.Size(this.Tx.size * this.text_scale);
				}
			}
			this.Tx.effect_confusion = this.effect_confusion_;
			this.default_title_width_ = -1f;
			this.fine_flag = true;
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

		public override float default_title_width
		{
			get
			{
				if (this.default_title_width_ < 0f)
				{
					this.default_title_width_ = this.Tx.get_swidth_px();
				}
				return this.default_title_width_;
			}
		}

		public float fix_text_size
		{
			get
			{
				return this.fix_text_size_;
			}
			set
			{
				this.fix_text_size_ = value;
				if (this.Tx != null)
				{
					this.setTitleText(null);
				}
			}
		}

		public ALIGN alignx
		{
			get
			{
				return this.alignx_;
			}
			set
			{
				if (this.alignx_ != value)
				{
					this.alignx_ = value;
					if (this.Tx != null)
					{
						this.setTitleText(null);
					}
				}
			}
		}

		public ALIGNY aligny
		{
			get
			{
				return this.aligny_;
			}
			set
			{
				if (this.aligny_ != value)
				{
					this.aligny_ = value;
					if (this.Tx != null)
					{
						this.setTitleText(null);
					}
				}
			}
		}

		public bool effect_confusion
		{
			get
			{
				return this.effect_confusion_;
			}
			set
			{
				if (this.effect_confusion_ != value)
				{
					this.effect_confusion_ = value;
					if (this.Tx != null)
					{
						this.Tx.effect_confusion = value;
					}
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
				base.alpha = value;
				if (this.Tx != null)
				{
					this.Tx.Alpha(this.text_alpha_ * this.alpha_);
				}
			}
		}

		public virtual float text_alpha
		{
			get
			{
				return this.text_alpha_;
			}
			set
			{
				this.text_alpha_ = value;
				if (this.Tx != null)
				{
					this.Tx.Alpha(this.text_alpha_ * this.alpha_);
				}
			}
		}

		protected void prepareIconMesh()
		{
			if (this.MdIco != null)
			{
				return;
			}
			this.MdIco = base.makeMesh(BLEND.NORMAL, (this.AIcons != null && this.AIcons.Count > 0) ? MTRX.getMI(this.AIcons[0].pChar) : MTRX.MIicon);
		}

		public override void setEnable(bool f)
		{
			if (this.Tx != null)
			{
				this.Tx.enabled = f;
			}
			base.setEnable(f);
		}

		protected MeshDrawer Md;

		protected MeshDrawer MdStripe;

		protected MeshDrawer MdStripeB;

		protected MeshDrawer MdIco;

		public static C32 Col;

		public const float DEFAULT_W = 300f;

		public const float DEFAULT_H = 20f;

		public int row_left_px = 4;

		public int row_right_px = 4;

		public int ICON_MARGIN = 4;

		public bool icon_top;

		public float icon_scale = 1f;

		protected float fix_text_size_;

		private float text_alpha_ = 1f;

		protected ALIGN alignx_ = ALIGN.LEFT;

		protected ALIGNY aligny_;

		protected TextRenderer Tx;

		protected List<PxlFrame> AIcons;

		protected bool alloc_auto_wrap;

		protected bool auto_fix_max_swidth = true;

		public const uint base_col_normal_default = 3136352496U;

		public uint base_col_normal = 3136352496U;

		public uint base_col_locked = 3132207537U;

		public uint base_col_pushdown = 3428545515U;

		public uint base_col_hovered0 = 3132086015U;

		public uint base_col_hovered1 = 3718297343U;

		public uint base_col_checked0 = 3134847213U;

		public uint base_col_checked1 = 3132356351U;

		public uint icon_col_normal = uint.MaxValue;

		public uint icon_col_pushdown = uint.MaxValue;

		public uint tx_col_normal = 4281348144U;

		public uint tx_col_locked = 3128391543U;

		public uint tx_col_pushdown = 4290506751U;

		public uint tx_col_hovered = 4280162971U;

		public uint tx_col_checked = 4281227127U;

		public uint stripe_col_pushdown = 3125960951U;

		public uint stripe_col_hovered = 3137339391U;

		protected bool effect_confusion_;

		public float text_scale = 1f;
	}
}
