using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinItemRow : ButtonSkinNelUi
	{
		public ButtonSkinItemRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.fine_continue_flags |= 8U;
			this.fineTextScale();
			this.fine_on_binding_changing = false;
			this.TxR = base.MakeTx("-text_r");
			this.TxR.html_mode = true;
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, base.container_stencil_ref), false);
			this.Md.chooseSubMesh(0, false, false);
			base.bottom_line = true;
			this.Md.connectRendererToTriMulti(base.getMeshRenderer(this.Md));
		}

		private void fineTextScale()
		{
			this.text_scale = X.ZLINE(this.h * 64f, 30f);
		}

		protected int basic_left_x
		{
			get
			{
				return X.IntR(this.h * 64f) + 12;
			}
		}

		protected int hidden_bottle_shift_x
		{
			get
			{
				return ButtonSkinItemRow.h_px_2_hidden_bottle_shift_x(this.h * 64f);
			}
		}

		protected static int h_px_2_hidden_bottle_shift_x(float h_px)
		{
			return X.IntR(X.Mn(h_px - 6f, 28f) + 20f);
		}

		public virtual void setItem(UiItemManageBox _ItemMng, ItemStorage _Storage, ItemStorage.IRow _ItmRow)
		{
			this.Storage = _Storage;
			this.ItmRow = _ItmRow;
			this.ItemMng = _ItemMng;
			this.hid = ((this.ItmRow != null && this.Storage != null) ? this.ItmRow.hidden : ItemStorage.ROWHID.VISIBLE);
			this.row_left_px = this.basic_left_x + (this.is_hidden_bottle ? this.hidden_bottle_shift_x : 0);
			this.fine_title_string = true;
			this.fineTextScale();
			if (this.TxR == null)
			{
				this.TxR = base.MakeTx("-text_r");
				this.TxR.html_mode = true;
			}
			if (this.hid == ItemStorage.ROWHID.H_BOTTLE)
			{
				this.text_alpha = 0.66f;
				if (this.ItmRow.Data == NelItem.HolderBottle)
				{
					this.text_alpha = 0f;
				}
			}
			else
			{
				this.text_alpha = 1f;
			}
			this.quest_target = false;
			this.fine_continue_flags &= 4294967279U;
			if (this.Storage != null && this.ItmRow != null && this.Storage.check_quest_target && this.ItemMng != null)
			{
				QuestTracker quest = this.ItemMng.IMNG.M2D.QUEST;
				this.quest_target = quest.isQuestTargetItem(this.ItmRow.Data);
				if (this.quest_target)
				{
					this.fine_continue_flags |= 16U;
				}
			}
			this.TxR.StencilRef(base.container_stencil_ref);
			this.fineCount(true);
			IN.setZ(this.TxR.transform, -0.001f);
			IN.setZ(this.Tx.transform, -0.001f);
			this.fine_flag = true;
		}

		public ItemStorage Inventory
		{
			get
			{
				if (this.ItemMng == null)
				{
					return null;
				}
				return this.ItemMng.Inventory;
			}
		}

		protected virtual string getTitleString()
		{
			NelItem data = this.ItmRow.Data;
			if (this.Storage == null)
			{
				return data.getLocalizedName((int)this.ItmRow.splitted_grade, null);
			}
			base.effect_confusion = this.Storage.isMngEffectConfusion();
			string text = (this.Storage.grade_split ? data.getLocalizedName((int)this.ItmRow.splitted_grade, this.Inventory) : data.getLocalizedName(this.ItmRow.top_grade, this.Inventory));
			if (this.Storage.FD_RowNameAddition != null)
			{
				text = this.Storage.FD_RowNameAddition(this.ItmRow, this.Storage, text);
			}
			if (this.Storage.grade_split && !data.individual_grade)
			{
				text = text + " <img mesh=\"nel_item_grade." + ((int)(5 + this.ItmRow.splitted_grade)).ToString() + "\" width=\"34\" tx_color/>";
			}
			return text;
		}

		protected override void setTitleText(string str)
		{
			bool flag = false;
			if (this.fix_text_size_ <= 0f)
			{
				flag = true;
				this.fix_text_size_ = 18f * this.text_scale;
			}
			base.setTitleText(str);
			if (flag)
			{
				this.fix_text_size_ = 0f;
			}
		}

		protected virtual STB getCountString(STB Stb)
		{
			if (this.Storage == null)
			{
				return Stb;
			}
			Stb.Add((this.ItemMng != null) ? this.ItemMng.getDescStr(this.ItmRow, UiItemManageBox.DESC_ROW.ROW_COUNT, this.Storage.grade_split ? ((int)this.ItmRow.splitted_grade) : (-1)) : this.ItmRow.Data.getCountString(this.ItmRow.total, this.Storage));
			return Stb;
		}

		protected virtual void repositRightText()
		{
			if (this.TxR == null)
			{
				return;
			}
			Vector3 localPosition = this.TxR.transform.localPosition;
			localPosition.x = this.w / 2f - this.right_shift_pixel * 0.015625f;
			this.TxR.transform.localPosition = localPosition;
			if (this.Tx != null)
			{
				this.Tx.auto_condense_line = true;
				this.row_right_px = (int)X.Mx(this.w * 64f * 0.2f, (this.w * 0.5f - this.TxR.transform.localPosition.x) * 64f + 6f + this.TxR.get_swidth_px());
				this.row_right_px += this.quest_target_right_shift_px;
			}
		}

		protected int quest_target_right_shift_px
		{
			get
			{
				if (!this.quest_target)
				{
					return 0;
				}
				return X.IntC(this.h * 64f + 4f);
			}
		}

		public virtual void fineCount(bool reposit_right_text = true)
		{
			if (this.is_hidden_row)
			{
				this.TxR.text_content = "";
				return;
			}
			using (STB countString = this.getCountString(TX.PopBld(null, 0)))
			{
				this.TxR.Txt(countString).LetterSpacing(0.8f).Size(14f * this.text_scale)
					.Align(ALIGN.RIGHT)
					.AlignY(this.aligny_)
					.Alpha(this.alpha_);
				if (reposit_right_text)
				{
					this.repositRightText();
				}
			}
		}

		public void setRightExistIcon(int id, string s)
		{
			if (this.Aexist_icon_mesh == null)
			{
				if (!TX.valid(s))
				{
					return;
				}
				this.Aexist_icon_mesh = new string[X.Mx(id + 1, this.use_exist_icon)];
			}
			else if (this.Aexist_icon_mesh.Length <= id)
			{
				Array.Resize<string>(ref this.Aexist_icon_mesh, id + 1);
			}
			if (this.Aexist_icon_mesh[id] != s)
			{
				this.fine_flag = true;
			}
			this.Aexist_icon_mesh[id] = s;
			this.use_exist_icon = X.Mx(id + 1, this.use_exist_icon_);
		}

		protected virtual void drawIcon(float x, float y)
		{
			float num = 1f;
			Color32 color = MTRX.ColTrnsp;
			PxlFrame pxlFrame = null;
			if (this.hid == ItemStorage.ROWHID.H_BOTTLE)
			{
				if (this.ItmRow.Data == NelItem.HolderBottle)
				{
					num = 0f;
					color = C32.d2c(4288057994U);
				}
				else
				{
					color = C32.d2c(4283780170U);
				}
				pxlFrame = MTR.AItemIcon[NelItem.HolderBottle.getIcon(this.Storage, null)];
			}
			if (color.a > 0)
			{
				this.Md.chooseSubMesh(0, false, false);
				this.Md.Col = C32.MulA(color, this.alpha_);
				float num2 = X.Mn(this.h * 64f - 6f, 28f);
				float num3 = x - (float)this.hidden_bottle_shift_x + 5f;
				this.Md.KadoPolyRect(num3, 0f, num2, num2, 8f, 12, 0f, false, 0f, 0f, false);
				if (pxlFrame != null)
				{
					this.Md.chooseSubMesh(1, false, false);
					this.Md.Col = C32.MulA(4293321691U, this.alpha_);
					this.Md.RotaPF(num3, 0f, 1f, 1f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
					this.Md.chooseSubMesh(0, false, false);
				}
			}
			if (num > 0f)
			{
				this.ItmRow.Data.drawIconTo(this.Md, this.Storage, 0, 1, x, y, this.text_scale, this.alpha_ * this.text_alpha * (base.isLocked() ? 0.45f : 1f), this.ItmRow);
			}
		}

		protected override void RowFineAfter(float w, float h)
		{
			if (this.Tx != null)
			{
				this.TxR.Col(this.Tx.TextColor).Alpha(this.alpha_);
				this.TxR.effect_confusion = this.ItemMng != null && this.ItemMng.effect_confusion;
				this.Md.chooseSubMesh(1, false, true);
				this.drawIcon(-w * 0.5f + 15f + 5f + (float)this.row_left_px - (float)this.basic_left_x, 0f);
				if (this.Aexist_icon_mesh != null)
				{
					this.Md.chooseSubMesh(1, false, false);
					this.Md.Col = MTRX.ColWhite;
					float num = w * 0.5f - ((float)this.use_exist_icon - 0.5f) * 14f;
					int num2 = X.Mn(this.Aexist_icon_mesh.Length, this.use_exist_icon);
					for (int i = 0; i < num2; i++)
					{
						if (TX.valid(this.Aexist_icon_mesh[i]))
						{
							this.Md.RotaPF(num, 0f, 1f, 1f, 0f, MTRX.getPF(this.Aexist_icon_mesh[i]), false, false, false, uint.MaxValue, false, 0);
						}
						num += 14f;
					}
				}
				if (this.quest_target)
				{
					float num3 = this.Tx.transform.localPosition.x * 64f + this.Tx.get_swidth_px() + h * 0.5f;
					this.Md.Col = C32.WMulA(this.alpha_);
					NEL.QuestNoticeExc(this.Md, num3, 0f);
				}
				this.Md.chooseSubMesh(0, false, false);
			}
			if (base.isChecked())
			{
				this.Md.ColGrd.Set(this.tx_col_normal);
				if (base.isLocked())
				{
					this.Md.ColGrd.Set(this.tx_col_locked);
				}
				else if (this.isPushDown())
				{
					this.Md.ColGrd.Set(this.tx_col_pushdown);
				}
				else if (base.isChecked())
				{
					this.Md.ColGrd.Set(this.tx_col_checked);
				}
				else if (base.isHoveredOrPushOut())
				{
					this.Md.ColGrd.blend(this.tx_col_locked, 0.3f + 0.3f * X.COSIT(40f));
				}
				this.MdStripe.Col = C32.MulA(this.Md.ColGrd.C, this.alpha_ * 0.14f);
				this.MdStripe.uvRectN(X.Cos(0.7853982f), X.Sin(-0.7853982f));
				this.MdStripe.allocUv2(6, false).Uv2(3f, 0.5f, false);
				this.MdStripe.Rect(0f, 0f, w - 4f, h - 4f, false);
				this.MdStripe.allocUv2(0, true);
			}
			if (base.isLocked())
			{
				this.Md.Col = C32.MulA(4283780170U, this.alpha_ * 0.15f);
				this.Md.Rect(0f, 0f, w, h, false);
			}
			base.RowFineAfter(w, h);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha_ == 0f)
			{
				return this;
			}
			this.executeFineTitleString();
			base.Fine();
			if (this.Tx != null)
			{
				this.Tx.MustRedraw();
			}
			return this;
		}

		protected void executeFineTitleString()
		{
			if (this.ItmRow == null || !this.fine_title_string)
			{
				return;
			}
			this.fine_title_string = false;
			this.setTitle(this.getTitleString());
		}

		public NelItem getItemData()
		{
			if (this.ItmRow == null)
			{
				return null;
			}
			return this.ItmRow.Data;
		}

		public ItemStorage.IRow getItemRow()
		{
			return this.ItmRow;
		}

		protected override void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
		}

		public bool is_hidden_bottle
		{
			get
			{
				return this.hid == ItemStorage.ROWHID.H_BOTTLE;
			}
		}

		public bool is_hidden_row
		{
			get
			{
				return this.hid > ItemStorage.ROWHID.VISIBLE;
			}
		}

		public override float alpha
		{
			set
			{
				base.alpha = value;
				if (this.TxR != null)
				{
					this.TxR.Alpha(this.text_alpha * this.alpha_);
				}
			}
		}

		public override float text_alpha
		{
			set
			{
				base.text_alpha = value;
				if (this.TxR != null)
				{
					this.TxR.Alpha(this.text_alpha * this.alpha_);
				}
			}
		}

		public TextRenderer getRightTextRenderer()
		{
			return this.TxR;
		}

		public int use_exist_icon
		{
			get
			{
				return this.use_exist_icon_;
			}
			set
			{
				if (this.use_exist_icon_ != value)
				{
					this.right_shift_pixel += (float)(14 * (value - this.use_exist_icon_));
					this.use_exist_icon_ = value;
					this.repositRightText();
				}
			}
		}

		public ItemStorage.IRow ItmRow;

		private ItemStorage.ROWHID hid;

		public UiItemManageBox ItemMng;

		protected ItemStorage Storage;

		protected TextRenderer TxR;

		public float right_shift_pixel = 22f;

		private int use_exist_icon_;

		private string[] Aexist_icon_mesh;

		private const int EXICON_W = 14;

		protected const int GRADE_MAX = 5;

		private int grade = -1;

		protected bool fine_title_string;

		public bool quest_target;

		public const float DEFAULT_H_ROW = 30f;
	}
}
