using System;
using Better;
using UnityEngine;

namespace XX
{
	public class ButtonSkinForLabel : ButtonSkinDesc
	{
		public ButtonSkinForLabel(LabeledInputField _B, float _w, float _h)
			: base(_B, 0f, 0f)
		{
			this.LI = _B;
			Material material;
			if (this.B.Container != null && this.B.Container.stencil_ref >= 0)
			{
				if (!ButtonSkinForLabel.OMtrMd.TryGetValue(this.B.Container.stencil_ref, out material))
				{
					material = (ButtonSkinForLabel.OMtrMd[this.B.Container.stencil_ref] = MTRX.newMtr(MTRX.ShaderMeshST));
					material.SetFloat("_StencilRef", (float)this.B.Container.stencil_ref);
					material.SetFloat("_StencilComp", 3f);
					material.SetFloat("_StencilOp", 3f);
				}
			}
			else if (!ButtonSkinForLabel.OMtrMd.TryGetValue(-1, out material))
			{
				material = (ButtonSkinForLabel.OMtrMd[-1] = MTRX.newMtr(MTRX.ShaderMeshST));
				material.SetFloat("_StencilRef", 4f);
				material.SetFloat("_StencilComp", 8f);
				material.SetFloat("_StencilOp", 2f);
			}
			this.Md = base.makeMesh(material);
			this.Md.base_z = -0.0625f;
			this.Md.setMaterialCloneFlag();
			this.w = ((_w == 0f) ? 240f : _w) * 0.015625f;
			this.h = _h * 0.015625f;
			this.desc_aim_bit = 0;
			base.desc = "";
			this.curs_level_x = -0.65f;
			this.curs_level_y = 0f;
			if (ButtonSkinForLabel.Col == null)
			{
				ButtonSkinForLabel.Col = new C32();
			}
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f || this.TxR == null)
			{
				return this;
			}
			if (!this.LI.isGUIactive())
			{
				Rect fieldBounds = this.getFieldBounds(false);
				ButtonSkinForLabel.Col.Set(3724015607U);
				if (base.isLocked())
				{
					ButtonSkinForLabel.Col.Set(2863640495U);
				}
				this.Md.Col = ButtonSkinForLabel.Col.mulA(this.alpha_).C;
				float num = (fieldBounds.xMin + fieldBounds.xMax) / 2f * 64f;
				float num2 = (fieldBounds.yMin + fieldBounds.yMax) / 2f * 64f;
				this.Md.KadomaruRect(num, num2, fieldBounds.width * 64f, fieldBounds.height * 64f, 2f, 0f, false, 0f, 0f, false);
				ButtonSkinForLabel.Col.Set(4285565374U);
				if (base.isLocked())
				{
					ButtonSkinForLabel.Col.Set(4285032552U);
				}
				this.Md.Col = ButtonSkinForLabel.Col.mulA(this.alpha_).C;
				this.Md.KadomaruRect(num, num2, fieldBounds.width * 64f, fieldBounds.height * 64f, 2f, 1f, false, 0f, 0f, false);
				this.Md.updateForMeshRenderer(false);
				ButtonSkinForLabel.Col.Set(4281677109U);
				if (base.isLocked())
				{
					ButtonSkinForLabel.Col.Set(4283914071U);
				}
				this.TxR.Col(ButtonSkinForLabel.Col.mulA(this.alpha_).C);
				this.TxR.enabled = true;
			}
			else
			{
				this.Md.clear(false, false);
				this.TxR.enabled = false;
			}
			if (this.LI.multi_line <= 1)
			{
				if (this.LI.label_top)
				{
					IN.PosP2(this.TxR.transform, -this.swidth * 0.5f + this.field_margin, -this.sheight * 0.5f + this.h * 64f * 0.5f + 1f);
				}
				else
				{
					IN.PosP2(this.TxR.transform, -this.swidth * 0.5f + this.getTextSwidth() + this.field_margin, 1f);
				}
			}
			else if (this.LI.label_top)
			{
				IN.PosP2(this.TxR.transform, -this.swidth * 0.5f + this.field_margin, -this.sheight * 0.5f + this.h * 64f - this.TxR.get_sheight_px() * 0.5f - this.field_margin_y);
			}
			else
			{
				IN.PosP2(this.TxR.transform, -this.swidth * 0.5f + this.getTextSwidth() + this.field_margin, this.sheight * 0.5f - this.TxR.get_sheight_px() * 0.5f - this.field_margin_y);
			}
			ButtonSkinForLabel.Col.Set(this.label_col);
			if (base.isLocked())
			{
				ButtonSkinForLabel.Col.Set(this.label_col_locked);
			}
			this.TxLabel.Col(ButtonSkinForLabel.Col.mulA(this.alpha_).C);
			return base.Fine();
		}

		public virtual bool CharInputted(string new_string, string pre_string)
		{
			return true;
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
			if (this.TxLabel == null)
			{
				this.TxLabel = base.MakeTx("-txlabel");
				this.TxR = base.MakeTx("-txright");
				this.TxLabel.Align(ALIGN.LEFT).AlignY(ALIGNY.MIDDLE);
				this.TxR.Align(ALIGN.LEFT).AlignY(ALIGNY.MIDDLE).LineSpacing(1f);
			}
			if (this.B.Container != null && this.B.Container.stencil_ref >= 0)
			{
				this.TxLabel.StencilRef(this.B.Container.stencil_ref);
				this.TxR.StencilRef(this.B.Container.stencil_ref + 1);
			}
			else
			{
				this.TxR.StencilRef(4);
			}
			this.TxLabel.Txt(X.T2K(str)).SizeFromHeight((float)this.LI.size, 0.125f).LetterSpacing(0.95f)
				.Alpha(this.alpha_);
			this.TxR.Size((float)this.LI.size).LetterSpacing(0.95f).Alpha(this.alpha_);
			this.setText(this.LI.text);
			this.default_title_width = this.TxLabel.get_swidth_px() + 4f;
			IN.setZ(this.TxR.transform, -0.065f);
			if (this.LI.label_top)
			{
				IN.PosP(this.TxLabel.transform, -this.swidth * 0.5f, this.sheight * 0.5f - this.TxLabel.get_line_sheight_px(false) / 2f + 4f, 0f);
			}
			else
			{
				IN.PosP(this.TxLabel.transform, -this.swidth * 0.5f, this.TxLabel.get_line_sheight_px(false) / 12f, 0f);
			}
			this.fineAlign();
			this.fine_flag = true;
		}

		public void fineAlign()
		{
			if (this.TxR != null)
			{
				this.TxR.alignx = this.LI.alignx;
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
					if (this.TxLabel != null)
					{
						this.TxLabel.Alpha(this.alpha_);
						this.TxR.Alpha(this.alpha_);
					}
				}
			}
		}

		public Rect getFieldBounds(bool screen_pos)
		{
			if (this.LI.label_top)
			{
				ButtonSkinForLabel.Pos.Set(0f, -this.sheight * 0.5f * 0.015625f + this.h * 0.5f, 0f);
			}
			else
			{
				ButtonSkinForLabel.Pos.Set(this.swidth / 2f * 0.015625f - this.w / 2f, 0f, 0f);
			}
			ButtonSkinForLabel.Pos2.Set(ButtonSkinForLabel.Pos.x - this.w / 2f, ButtonSkinForLabel.Pos.y - this.h / 2f, 0f);
			ButtonSkinForLabel.Pos.Set(ButtonSkinForLabel.Pos.x + this.w / 2f, ButtonSkinForLabel.Pos.y + this.h / 2f, 0f);
			if (screen_pos)
			{
				ButtonSkinForLabel.Pos2 = this.LI.BaseCamera.WorldToScreenPoint(this.B.transform.TransformPoint(ButtonSkinForLabel.Pos2));
				ButtonSkinForLabel.Pos = this.LI.BaseCamera.WorldToScreenPoint(this.B.transform.TransformPoint(ButtonSkinForLabel.Pos));
				ButtonSkinForLabel.Rc.Set(ButtonSkinForLabel.Pos2.x, ButtonSkinForLabel.Pos.y, ButtonSkinForLabel.Pos.x - ButtonSkinForLabel.Pos2.x, ButtonSkinForLabel.Pos.y - ButtonSkinForLabel.Pos2.y);
				ButtonSkinForLabel.Rc.y = IN.h * IN.pixel_scale - ButtonSkinForLabel.Rc.y;
			}
			else
			{
				ButtonSkinForLabel.Rc.Set(ButtonSkinForLabel.Pos2.x, ButtonSkinForLabel.Pos2.y, ButtonSkinForLabel.Pos.x - ButtonSkinForLabel.Pos2.x, ButtonSkinForLabel.Pos.y - ButtonSkinForLabel.Pos2.y);
			}
			return ButtonSkinForLabel.Rc;
		}

		public void setText(string str)
		{
			if (str == null)
			{
				return;
			}
			string[] array = str.Split(new char[] { '\n' });
			if (this.LI.multi_line < array.Length)
			{
				str = TX.join<string>("\n", array, 0, this.LI.multi_line) + "...";
			}
			this.TxR.Txt(str);
			if (this.desc_aim_bit != 0)
			{
				base.desc = str;
			}
			this.fine_flag = true;
		}

		public override bool canClickable(Vector2 PosU)
		{
			return CLICK.getClickableRectSimple(PosU, this.B.getTransform(), this.swidth * 0.015625f, this.sheight * 0.015625f);
		}

		public override void setEnable(bool f)
		{
			if (this.TxR != null)
			{
				this.TxR.enabled = f;
				this.TxLabel.enabled = f;
			}
			base.setEnable(f);
		}

		public float getTextSwidth()
		{
			if (!this.LI.label_top && this.default_title_width != 0f)
			{
				return 6f + this.default_title_width;
			}
			return 0f;
		}

		public override float sheight
		{
			get
			{
				return this.h * 64f + ((this.LI.label_top && this.TxLabel != null) ? (this.TxLabel.get_sheight_px() + 6f) : 0f);
			}
		}

		public override float swidth
		{
			get
			{
				return this.w * 64f + this.getTextSwidth();
			}
		}

		public float field_margin
		{
			get
			{
				return (float)this.LI.size * 0.15f;
			}
		}

		public float field_margin_y
		{
			get
			{
				return (float)this.LI.size * 0.28f;
			}
		}

		private LabeledInputField LI;

		protected MeshDrawer Md;

		public static C32 Col;

		private TextRenderer TxLabel;

		private TextRenderer TxR;

		public const int TX_MARGIN = 6;

		public static Rect Rc = default(Rect);

		public static Vector3 Pos = default(Vector3);

		public static Vector3 Pos2 = default(Vector3);

		private const int default_right_stencil = 4;

		public uint label_col = uint.MaxValue;

		public uint label_col_locked = 2583691263U;

		private static BDic<int, Material> OMtrMd = new BDic<int, Material>();
	}
}
