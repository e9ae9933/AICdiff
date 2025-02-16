using System;
using XX;

namespace nel
{
	public class ButtonSkinGuildQuestItemRow : ButtonSkinItemRow
	{
		public ButtonSkinGuildQuestItemRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
		}

		protected override STB getTitleString(STB Stb)
		{
			this.ItmRow.Data.getLocalizedName(Stb, this.Storage.grade_split ? ((int)this.ItmRow.splitted_grade) : this.ItmRow.top_grade);
			return Stb;
		}

		public override void fineCount(bool reposit_right_text = true)
		{
			base.fineCount(false);
			if (base.is_hidden_row)
			{
				this.TxR.text_content = "";
				return;
			}
			this.row_right_px = 90;
			using (STB stb = TX.PopBld(null, 0))
			{
				NelItem.getGradeMeshTxTo(stb, (int)this.ItmRow.splitted_grade, 4, 74);
				this.TxR.Txt(stb);
			}
			if (reposit_right_text)
			{
				this.repositRightText();
			}
		}

		protected override void RowFineAfter(float w, float h)
		{
			base.RowFineAfter(w, h);
			float num = w * 0.5f - (float)this.row_right_px + 12f;
			if (this.received_ != ButtonSkinGuildQuestItemRow.RECIEVE.NONE)
			{
				float num2 = 1f;
				if (this.received_ == ButtonSkinGuildQuestItemRow.RECIEVE.RECIEVED_TEMP)
				{
					num2 *= 0.4f + 0.3f * X.COSIT(120f);
				}
				this.Md.Col = this.Md.ColGrd.Set(this.Tx.TextColor).mulA(num2).C;
				float num3 = h * 0.5f;
				float num4 = num - h;
				float num5 = num4 - 10.4f;
				float num6 = h * 0.4f;
				this.Md.TriRectBL(0);
				this.Md.PosD(num, num3, null).PosD(num4, -num3, null).PosD(num4 - 8f, -num3, null)
					.PosD(num - 8f, num3, null);
				this.Md.TriRectBL(0);
				this.Md.PosD(num5, -num3, null).PosD(num5 - 8f, -num3, null).PosD(num5 - 8f - num6, -num3 + num6, null)
					.PosD(num5 - num6, -num3 + num6, null);
			}
		}

		protected override STB getCountString(STB Stb)
		{
			if (this.Storage == null)
			{
				return Stb;
			}
			NelItem.getGradeMeshTxTo(Stb, (int)this.ItmRow.splitted_grade, 4, 58);
			return Stb;
		}

		public ButtonSkinGuildQuestItemRow.RECIEVE received
		{
			get
			{
				return this.received_;
			}
			set
			{
				if (this.received == value)
				{
					return;
				}
				this.received_ = value;
				this.fine_flag = true;
				if (this.received_ == ButtonSkinGuildQuestItemRow.RECIEVE.RECIEVED_TEMP)
				{
					base.hilighted = true;
					this.fine_continue_flags = 31U;
					return;
				}
				base.hilighted = false;
				this.fine_continue_flags = 1U;
			}
		}

		private ButtonSkinGuildQuestItemRow.RECIEVE received_;

		public enum RECIEVE
		{
			NONE,
			RECIEVED,
			RECIEVED_TEMP
		}
	}
}
