using System;
using XX;

namespace nel
{
	public class ButtonSkinAlchemyEntryRow : ButtonSkinItemRow
	{
		public ButtonSkinAlchemyEntryRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.right_shift_pixel = 130f;
			this.row_left_px = 41;
		}

		public void blur(UiCraftBase.IngEntryRow _Rie)
		{
			if (this.Rie == _Rie)
			{
				this.Rie = null;
				this.fine_flag = true;
				this.ItmRow = null;
				this.TempObt = null;
			}
		}

		public void setItem(UiCraftBase.IngEntryRow _Rie, RCP.Recipe _Rcp, ItemStorage _Storage)
		{
			this.Rie = _Rie;
			this.TempObt = null;
			if (this.Rie.Itm != null)
			{
				this.TempObt = new ItemStorage.ObtainInfo();
				this.TempObt.changeGradeForPrecious(this.Rie.grade, 1);
				this.ItmRow = new ItemStorage.IRow(this.Rie.Itm, this.TempObt, false);
				this.ItmRow.splitted_grade = (byte)this.Rie.grade;
			}
			else
			{
				this.ItmRow = null;
			}
			RCP.Recipe recipe = ((this.ItmRow == null) ? this.Rie.Source.TargetRecipe : null);
			if (this.Rcp != recipe)
			{
				this.Rcp = recipe;
				if (this.Rcp != null)
				{
					ButtonSkinRowNelDark.setColorDark(this);
				}
				else
				{
					ButtonSkinNelUi.setColorBasic(this);
				}
			}
			base.setItem(null, _Storage, this.ItmRow);
			int num = this.Rie.generation * 13;
			this.row_left_px += num - this.generation_shift;
			this.generation_shift = num;
		}

		public override ButtonSkin setTitle(string str)
		{
			return base.setTitle((this.Rie == null) ? "" : str);
		}

		protected override STB getTitleString(STB Stb)
		{
			if (this.Rie == null)
			{
				return Stb;
			}
			if (this.Rcp != null)
			{
				return Stb.Add(this.Rcp.title);
			}
			return base.getTitleString(Stb);
		}

		protected override STB getCountString(STB Stb)
		{
			return Stb;
		}

		protected override void repositRightText()
		{
		}

		public override void fineCount(bool reposit_right_text = true)
		{
		}

		protected override void drawIcon(float x, float y)
		{
			if (this.Rie == null)
			{
				return;
			}
			if (this.Rcp != null)
			{
				this.Md.Col = C32.MulA(4294823042U, this.alpha_);
				this.Md.chooseSubMesh(0, false, false);
				this.Md.Box(x, y, 30f, 30f, 1f, false);
				this.Md.chooseSubMesh(1, false, false);
				this.Md.RotaPF(x, y, 1f, 1f, 0f, MTR.AItemIcon[this.Rcp.get_icon_id()], false, false, false, uint.MaxValue, false, 0);
			}
			else
			{
				base.drawIcon(x, y);
			}
			this.Md.chooseSubMesh(0, false, false);
			if (this.Rie.generation > 0)
			{
				this.Md.Col = C32.MulA(this.Tx.TextColor, this.alpha_);
				this.Md.Line(x - 15f, 0f, x - 22.5f, 0f, 1f, false, 0f, 0f);
				this.Md.Line(x - 22.5f, 0f, x - 22.5f, 15f, 1f, false, 0f, 0f);
			}
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f || this.Rie == null)
			{
				return this;
			}
			return base.Fine();
		}

		private RCP.Recipe Rcp;

		private UiCraftBase.IngEntryRow Rie;

		private ItemStorage.ObtainInfo TempObt;

		private int generation_shift;
	}
}
