using System;
using XX;

namespace nel
{
	public class ButtonSkinEnhancerRow : ButtonSkinItemRow
	{
		public ButtonSkinEnhancerRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.right_shift_pixel = 2f;
		}

		public override void setItem(UiItemManageBox _ItemMng, ItemStorage _Storage, ItemStorage.IRow _ItmRow)
		{
			base.use_exist_icon = 2;
			this.Eh = ENHA.Get(_ItmRow.Data);
			base.setItem(_ItemMng, _Storage, _ItmRow);
			this.fineEquip();
		}

		public void fineEquip()
		{
			if (this.is_equip)
			{
				base.setRightExistIcon(1, "equip");
				base.hilighted = true;
			}
			else
			{
				base.setRightExistIcon(1, "");
				base.hilighted = false;
			}
			if (this.is_favorite)
			{
				base.setRightExistIcon(0, "favorite_star");
				return;
			}
			base.setRightExistIcon(0, "");
		}

		public bool is_equip
		{
			get
			{
				return this.ItmRow != null && (this.ItmRow.Info.top_grade & 2) != 0;
			}
		}

		public bool is_favorite
		{
			get
			{
				return this.ItmRow != null && (this.ItmRow.Info.top_grade & 1) != 0;
			}
		}

		protected override STB getTitleString(STB Stb)
		{
			Stb.Add((this.Eh != null) ? this.Eh.title : this.ItmRow.Data.key);
			return Stb;
		}

		protected override void drawIcon(float x, float y)
		{
			if (this.Eh == null || this.Eh.PF == null)
			{
				base.drawIcon(x, y);
				return;
			}
			this.Md.Col = C32.MulA(uint.MaxValue, this.alpha_);
			this.Md.RotaPF(x + 4f, y, 1f, 1f, 0f, this.Eh.PF, false, false, false, uint.MaxValue, false, 0);
			float num = this.h * 64f - 1f;
			this.Md.chooseSubMesh(0, false, false);
			this.Md.Col = C32.MulA(4283780170U, this.alpha_);
			this.Md.Box(x + 4f, y, num, num, 1f, false);
			this.Md.chooseSubMesh(1, false, false);
		}

		public const float DEFAULT_H_EH_ROW = 48f;

		private ENHA.Enhancer Eh;
	}
}
