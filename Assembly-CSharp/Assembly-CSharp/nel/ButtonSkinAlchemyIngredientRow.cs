using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinAlchemyIngredientRow : ButtonSkinItemRow
	{
		public ButtonSkinAlchemyIngredientRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.Sdr = new SlotDrawer(MTR.SqAlchemyRowSlot.getFrame(0), 0, -6);
		}

		public virtual void setItem(UiCraftBase _Con, List<List<UiCraftBase.IngEntryRow>> _AARie, RCP.Recipe _Rcp, RCP.RecipeIngredient _Ing, ItemStorage _Storage)
		{
			this.Con = _Con;
			this.Ing = _Ing;
			this.Rcp = _Rcp;
			this.AARie = _AARie;
			this.count_r = -1;
			this.right_shift_pixel = (float)(this.Ing.is_tool ? 190 : ((!this.show_effect_string) ? 20 : 130));
			if (this.TxRR == null)
			{
				this.TxRR = IN.CreateGob(this.Gob, "-text_c").AddComponent<TextRenderer>();
				this.TxRR.html_mode = true;
				this.TxRR.Col(4283780170U).StencilRef(base.container_stencil_ref);
			}
			if (this.Ing.Target != null)
			{
				this.ItmRow = new ItemStorage.IRow(this.Ing.Target, this.TempObt = new ItemStorage.ObtainInfo(), false);
			}
			base.setItem(null, _Storage, this.ItmRow);
			this.row_left_px = ((_Ing.target_category != (RCP.RPI_CATEG)0) ? 30 : (_Ing.is_tool ? 100 : 73));
		}

		protected override STB getTitleString(STB Stb)
		{
			if (this.Ing == null)
			{
				return Stb;
			}
			this.Ing.ingredientDescTo(Stb, true, true);
			if (this.Ing.is_tool)
			{
				List<UiCraftBase.IngEntryRow> list = this.AARie[this.Ing.index];
				using (STB stb = TX.PopBld(null, 0))
				{
					int count = list.Count;
					for (int i = 0; i < count; i++)
					{
						stb.Append(list[i].Itm.getLocalizedName(list[i].grade), ", ");
					}
					Stb.Add(" - ");
					Stb.Add(stb);
				}
			}
			return Stb;
		}

		protected override STB getCountString(STB Stb)
		{
			if (this.Ing == null || this.Ing.is_tool)
			{
				return Stb;
			}
			bool flag = this.count_r > this.Ing.need && !this.Ing.allloc_over_quantity;
			if (flag)
			{
				Stb.Add(NEL.error_tag);
			}
			Stb.Add("x", this.count_r, "");
			if (flag)
			{
				Stb.Add(NEL.error_tag_close);
			}
			return Stb.Add("/", this.Ing.need, "");
		}

		protected virtual STB getEffectString(STB Stb)
		{
			if (this.Ing.is_tool)
			{
				this.TxR.alignx = ALIGN.RIGHT;
				List<UiCraftBase.IngEntryRow> list = this.AARie[this.Ing.index];
				int count = list.Count;
				bool flag = false;
				for (int i = 0; i < count; i++)
				{
					UiCraftBase.IngEntryRow ingEntryRow = list[i];
					if (this.Con.reduceOnCreating(ingEntryRow.Itm, ingEntryRow.grade))
					{
						flag = true;
						break;
					}
				}
				if (flag || count == 0)
				{
					Stb.Clear();
				}
				else
				{
					Stb.AddTxA((this.Rcp.categ == RCP.RP_CATEG.COOK) ? "alchemy_tool_is_infinity_cook" : "alchemy_tool_is_infinity", false);
				}
			}
			else
			{
				bool flag2 = this.count_r > this.Ing.need && !this.Ing.allloc_over_quantity;
				if (flag2)
				{
					Stb.Add(NEL.error_tag);
				}
				Stb.AddTxA("alchemy_effect", false).TxRpl(this.Ing.getEffectRatio100((float)this.count_r, 0f));
				if (flag2)
				{
					Stb.Add(NEL.error_tag_close);
				}
			}
			return Stb;
		}

		public bool show_effect_string
		{
			get
			{
				return this.Ing.is_tool || (this.Rcp.Completion == null && this.Rcp.categ != RCP.RP_CATEG.ALOMA);
			}
		}

		protected override void repositRightText()
		{
			this.TxR.LetterSpacing(1f);
			base.repositRightText();
			this.Tx.auto_condense_line = false;
			this.Tx.auto_wrap = true;
			this.row_right_px += (int)this.slot_swidth + 12;
			if (this.show_effect_string)
			{
				using (STB effectString = this.getEffectString(TX.PopBld(null, 0)))
				{
					this.TxRR.Txt(effectString).Size(14f).Align(ALIGN.CENTER)
						.AlignY(this.aligny_)
						.Alpha(this.alpha_);
					float num = this.w * 0.5f - (this.right_shift_pixel * 0.5f - 15f) * 0.015625f;
					IN.Pos(this.TxRR.transform, num, 0f, -0.001f);
				}
			}
		}

		public override void fineCount(bool reposit_right_text = true)
		{
			this.fine_flag = true;
			this.fine_title_string = true;
		}

		public override ButtonSkin Fine()
		{
			if (this.Ing == null)
			{
				return this;
			}
			if (this.fine_title_string)
			{
				List<UiCraftBase.IngEntryRow> list = this.AARie[this.Ing.index];
				int count = list.Count;
				int num = this.count_r;
				this.count_r = X.Mn(X.Mx(this.count_r, 0), count);
				while (this.count_r < count)
				{
					UiCraftBase.IngEntryRow ingEntryRow = list[this.count_r];
					if (!ingEntryRow.visible || !ingEntryRow.isFilled())
					{
						break;
					}
					this.count_r++;
				}
				this.fine_title_string = false;
				using (STB stb = TX.PopBld(null, 0))
				{
					this.getTitleString(stb);
					this.Tx.Txt(stb);
				}
				if (!this.Ing.is_tool && this.count_r != num)
				{
					if (this.Ing.allloc_over_quantity)
					{
						int num2 = X.Mn(this.count_r, this.Ing.need);
						this.Sdr.Fill(MTR.SqAlchemyRowSlot.getFrame(1), 0, num2);
						this.Sdr.Fill(null, num2, this.Ing.need - num2);
						int num3 = X.MMX(0, this.count_r - this.Ing.need, this.Ing.max - this.Ing.need);
						this.Sdr.Fill(MTR.SqAlchemyRowSlot.getFrame(4), this.Ing.need, num3);
						this.Sdr.Fill(MTR.SqAlchemyRowSlot.getFrame(3), this.Ing.need + num3, this.Ing.max - this.Ing.need - num3);
					}
					else
					{
						int num4 = X.Mx(this.Ing.need, this.count_r);
						this.Sdr.Fill(MTR.SqAlchemyRowSlot.getFrame(2), num4, this.Ing.max - num4);
						this.Sdr.Fill(MTR.SqAlchemyRowSlot.getFrame(1), 0, this.count_r);
						this.Sdr.Fill(null, this.count_r, this.Ing.need - this.count_r);
						if (this.count_r > this.Ing.need && !this.Ing.allloc_over_quantity)
						{
							this.Sdr.FillCol(C32.d2c(4290582552U), this.Ing.need, this.count_r - this.Ing.need);
							this.Sdr.DefaultCol(this.count_r, this.Ing.max - this.count_r);
						}
						else
						{
							this.Sdr.DefaultCol(this.Ing.need, this.Ing.max - this.Ing.need);
						}
					}
				}
				base.fineCount(true);
				base.hilighted = this.count_r < this.Ing.need;
				this.slot_scl = (float)((this.Ing.max > 10) ? 1 : 2);
			}
			return base.Fine();
		}

		private float slot_swidth
		{
			get
			{
				return (float)((this.Ing == null) ? 0 : (((this.Ing.max > 10) ? 1 : 2) * (6 * X.Mn(this.Ing.max, 10))));
			}
		}

		private float slot_shift_x_px
		{
			get
			{
				return -this.w * 64f * 0.5f + (float)this.row_left_px + 28f + this.Tx.get_swidth_px();
			}
		}

		protected override void drawIcon(float x, float y)
		{
			x += 12f;
			if (this.TxRR != null)
			{
				this.TxRR.Alpha(this.alpha_).Col(this.Tx.TextColor);
			}
			if (this.Ing.TargetRecipe != null)
			{
				this.Md.Col = C32.MulA(4293358859U, this.alpha_);
				this.Md.chooseSubMesh(0, false, false);
				this.Md.Box(x, y, 30f, 30f, 1f, false);
				this.Md.chooseSubMesh(1, false, false);
				this.Md.RotaPF(x, y, 1f, 1f, 0f, MTR.AItemIcon[this.Ing.TargetRecipe.get_icon_id()], false, false, false, uint.MaxValue, false, 0);
			}
			else if (this.Ing.Target != null)
			{
				if (this.Ing.Target.is_tool)
				{
					this.drawToolIcon(x, y, this.Ing.Target.getIcon(this.Storage, null));
				}
				else
				{
					base.drawIcon(x, y);
				}
			}
			else if (this.Ing.target_ni_category > NelItem.CATEG.OTHER && this.Ing.is_tool)
			{
				this.drawToolIcon(x, y, RCP.RecipeIngredient.getToolIcon(this.Ing.target_ni_category));
			}
			this.Md.chooseSubMesh(1, false, false);
			this.Md.Col = C32.MulA(this.Tx.TextColor, this.alpha_);
			int max = this.Ing.max;
			if (!this.Ing.is_tool)
			{
				this.Sdr.drawTo(this.Md, this.slot_shift_x_px, 0f, this.slot_scl, this.slot_scl, this.Ing.max, 0f, -1f);
			}
			this.Md.chooseSubMesh(0, false, false);
			if (this.count_r >= this.Ing.need)
			{
				this.Md.CheckMark(-this.w * 0.5f * 64f + 14f, 0f, this.h * 64f * 0.6f, 4f, false);
			}
		}

		private void drawToolIcon(float x, float y, int icon_id)
		{
			x -= 12f;
			float num = 28f;
			this.Md.chooseSubMesh(0, false, false);
			this.Md.Col = C32.MulA(base.isChecked() ? 4293321691U : 4283780170U, this.alpha_);
			if (num * 2f > this.h * 64f)
			{
				float num2 = this.h * 64f * 0.5f;
				this.Md.Tri(0, 1, 5, false).TriRectBL(5, 1, 2, 4).Tri(4, 2, 3, false);
				this.Md.PosD(x - num, y, null).PosD(x - num + num2, y + num2, null).PosD(x + num - num2, y + num2, null)
					.PosD(x + num, y, null)
					.PosD(x + num - num2, y - num2, null)
					.PosD(x - num + num2, y - num2, null);
			}
			else
			{
				this.Md.Daia2(x, y, num, 0f, false);
			}
			this.Md.chooseSubMesh(1, false, false);
			this.Md.Col = C32.MulA(base.isChecked() ? 4283780170U : 4293321691U, this.alpha_);
			this.Md.RotaPF(x, y, 1f, 1f, 0f, MTR.AItemIcon[icon_id], false, false, false, uint.MaxValue, false, 0);
		}

		public Vector2 getSlotPosPixel(int p)
		{
			Vector2 pos = this.Sdr.getPos(p, this.slot_scl, this.slot_scl, 0f, -1f);
			pos.x += this.slot_shift_x_px;
			return pos;
		}

		public const float DEFAULT_H_ING_ROW = 48f;

		private UiCraftBase Con;

		private RCP.Recipe Rcp;

		private RCP.RecipeIngredient Ing;

		private ItemStorage.ObtainInfo TempObt;

		private List<List<UiCraftBase.IngEntryRow>> AARie;

		private TextRenderer TxRR;

		private SlotDrawer Sdr;

		private int count_r;

		private float slot_scl = 1f;
	}
}
