using System;
using System.Collections.Generic;
using XX;

namespace nel
{
	public class ButtonSkinAlchemyIngredientTrmRow : ButtonSkinAlchemyIngredientRow
	{
		public ButtonSkinAlchemyIngredientTrmRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
		}

		protected override STB getCountString(STB Stb)
		{
			return Stb;
		}

		protected override STB getEffectString(STB Stb)
		{
			return base.getCountString(Stb);
		}

		public override void setItem(UiCraftBase Con, List<List<UiCraftBase.IngEntryRow>> _AARie, RecipeManager.Recipe _Rcp, RecipeManager.RecipeIngredient _Ing, ItemStorage _Storage)
		{
			base.setItem(Con, _AARie, _Rcp, _Ing, _Storage);
			this.TargetTrm = (Con as ITrmListener).CurrentTargetTrmItem;
			if (this.TargetTrm.RcmHerb.RecipeInfo != null && (this.TargetTrm.RcmHerb.RecipeInfo.categ & _Ing.target_category) != (RecipeManager.RPI_CATEG)0)
			{
				this.main_ing = true;
				this.fine_continue_flags |= 16U;
			}
		}

		protected override void RowFineAfter(float w, float h)
		{
			if (!base.isChecked() && this.main_ing)
			{
				this.Md.Col = C32.MulA(4294410942U, this.alpha_ * 0.22f);
				this.MdStripe.Col = this.Md.Col;
				this.MdStripe.StripedM(0.7853982f, 24f, 0.5f, 4);
				this.MdStripe.Rect(0f, 0f, w - 4f, h - 4f, false).allocUv2(0, true);
				this.Md.Rect(0f, -(h - 4f) * 0.5f, w - 4f, 1f, false);
			}
			base.RowFineAfter(w, h);
		}

		private bool main_ing;

		private TRMManager.TRMItem TargetTrm;
	}
}
