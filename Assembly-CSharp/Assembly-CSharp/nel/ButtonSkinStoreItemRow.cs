using System;
using XX;

namespace nel
{
	public class ButtonSkinStoreItemRow : ButtonSkinItemRow
	{
		public ButtonSkinStoreItemRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
		}

		public override void setItem(UiItemManageBox _ItemMng, ItemStorage _Storage, ItemStorage.IRow _ItmRow)
		{
			if (this.TxC == null)
			{
				this.TxC = IN.CreateGob(this.Gob, "-text_c").AddComponent<TextRenderer>();
				this.TxC.html_mode = true;
				this.TxC.Col(4283780170U);
			}
			if (this.B.Container != null)
			{
				this.TxC.StencilRef(this.B.Container.stencil_ref);
			}
			base.setItem(_ItemMng, _Storage, _ItmRow);
		}

		protected override string getTitleString()
		{
			NelItem data = this.ItmRow.Data;
			if (!this.Storage.grade_split)
			{
				return data.getLocalizedName(this.ItmRow.top_grade, base.Inventory);
			}
			return data.getLocalizedName((int)this.ItmRow.splitted_grade, base.Inventory);
		}

		protected override void repositRightText()
		{
			base.repositRightText();
			float num = this.TxR.transform.localPosition.x - (this.TxR.get_swidth_px() + 15f) * 0.015625f;
			IN.Pos(this.TxC.transform, num, 0f, -0.001f);
			this.row_right_px = (int)X.Mx(this.w * 64f * 0.2f, (this.w * 0.5f - num) * 64f + 16f + this.TxC.get_swidth_px());
			this.row_right_px += base.quest_target_right_shift_px;
		}

		public override void fineCount(bool reposit_right_text = true)
		{
			base.fineCount(false);
			if (this.ItemMng == null || this.ItmRow == null || this.ItmRow.is_fake_row)
			{
				this.TxC.text_content = "";
			}
			else
			{
				this.TxC.Txt(((this.Storage.grade_split && !this.ItmRow.Data.individual_grade) ? string.Concat(new string[]
				{
					"<img mesh=\"nel_item_grade.",
					((int)(5 + this.ItmRow.splitted_grade)).ToString(),
					"\" width=\"34\" color=\"0x",
					C32.codeToCodeText(4283780170U),
					"\"/> "
				}) : "") + this.ItemMng.getDescStr(this.ItmRow, UiItemManageBox.DESC_ROW.ROW_PRICE, this.Storage.grade_split ? ((int)this.ItmRow.splitted_grade) : (-1))).LetterSpacing(0.9f).Size(14f)
					.Align(ALIGN.RIGHT)
					.AlignY(this.aligny_)
					.Alpha(this.alpha_);
			}
			if (reposit_right_text)
			{
				this.repositRightText();
			}
		}

		private TextRenderer TxC;
	}
}
