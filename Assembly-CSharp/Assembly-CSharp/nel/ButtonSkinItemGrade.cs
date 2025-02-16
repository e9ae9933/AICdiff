using System;
using XX;

namespace nel
{
	public class ButtonSkinItemGrade : ButtonSkin
	{
		public float draw_w
		{
			get
			{
				return 78f + this.count_w + 16f;
			}
		}

		public ButtonSkinItemGrade(aBtn _B, float _w, float _h)
			: base(_B, _w * 0.015625f, _h * 0.015625f)
		{
			this.Md = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref));
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, base.container_stencil_ref), false);
			this.Md.chooseSubMesh(0, false, false);
			this.Md.connectRendererToTriMulti(base.getMeshRenderer(this.Md));
			this.fine_continue_flags = 9U;
		}

		public void fineItem(NelItem _Itm, ItemStorage.ObtainInfo _ItmObt, int _grade, UiItemManageBox _ItemMng)
		{
			this.Itm = _Itm;
			this.ItemMng = _ItemMng;
			this.ItmObt = _ItmObt;
			this.grade = _grade;
			if (this.Itm == null)
			{
				return;
			}
			if (this.Tx == null)
			{
				this.Tx = base.MakeTx("-text");
				this.Tx.Align(ALIGN.RIGHT).AlignY(ALIGNY.MIDDLE).Size(16f);
				this.Tx.html_mode = true;
				this.Tx.auto_condense = true;
				this.Tx.letter_spacing = 0.88f;
				if (this.ItemMng != null)
				{
					this.count_w = this.ItemMng.grade_text_area_width;
				}
				this.Tx.max_swidth_px = this.count_w;
			}
			this.Tx.effect_confusion = this.ItemMng != null && this.ItemMng.effect_confusion;
			if (this.Itm.individual_grade)
			{
				this.Tx.Txt("<font size=\"13\">x</font>");
			}
			else
			{
				string text = ((this.ItmObt == null) ? "0" : this.ItmObt.getCount(this.grade).ToString());
				this.Tx.Txt("<font size=\"13\">x</font>" + ((this.ItemMng != null) ? this.ItemMng.getDescStr(this.Itm, UiItemManageBox.DESC_ROW.GRADE, _grade, _ItmObt, 0) : text));
			}
			IN.PosP2(this.Tx.transform, this.draw_w * 0.5f - 4f, 0f);
			this.fine_flag = true;
		}

		public bool isVivid()
		{
			if (this.Itm == null || this.Itm.individual_grade)
			{
				return false;
			}
			if (this.ItemMng != null)
			{
				return this.ItemMng.isGradeTouched(this.Itm, this.ItmObt, this.grade, false);
			}
			return this.ItmObt != null && this.ItmObt.getCount(this.grade) != 0;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f || this.ItemMng == null)
			{
				return this;
			}
			float num = (this.isVivid() ? 1f : 0.4f);
			bool flag = this.Itm != null && this.isPushDown() && !this.Itm.individual_grade;
			this.Md.Col = C32.MulA(flag ? 4293321691U : 4283780170U, num);
			this.Md.chooseSubMesh(1, true, false);
			this.Md.RotaPF(-this.draw_w * 0.5f + 39f, 0f, 1f, 1f, 0f, MTR.AItemGradeStars[this.grade], false, false, false, uint.MaxValue, false, 0);
			this.Md.chooseSubMesh(0, true, false);
			float num2 = X.NI(this.draw_w, this.w * 64f, 0.25f);
			if (this.Itm != null && !this.Itm.individual_grade && (base.isChecked() || base.isHoveredOrPushOut()))
			{
				this.Md.RectDashedM(0f, 0f, num2, 20f, X.IntC((this.draw_w + 20f) / 7f), 2f, 0.5f, false, false);
			}
			this.Md.updateForMeshRenderer(false);
			if (flag)
			{
				this.Md.Rect(0f, 0f, num2, 20f, false);
				this.Tx.Col(C32.MulA(4293321691U, num));
			}
			else
			{
				this.Tx.Col(C32.MulA(4283780170U, num));
			}
			return base.Fine();
		}

		public override void setEnable(bool f)
		{
			if (this.Tx != null)
			{
				this.Tx.enabled = f;
			}
			base.setEnable(f);
		}

		public UiItemManageBox getCurrentManager()
		{
			return this.ItemMng;
		}

		private int grade;

		private NelItem Itm;

		private ItemStorage.ObtainInfo ItmObt;

		private MeshDrawer Md;

		public const int img_w = 78;

		private float count_w = 24f;

		public const int rect_h = 20;

		private TextRenderer Tx;

		private UiItemManageBox ItemMng;
	}
}
