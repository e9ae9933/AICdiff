using System;
using UnityEngine;
using XX;

namespace nel
{
	public class ColumnRowNel : ColumnRow
	{
		public ColumnRowNel(GameObject _Base, int default_val = 0, int alloc_size = 0, ObjCarrierCon _Carr = null, ScrollAppend SCA = null)
			: base(_Base, default_val, alloc_size, _Carr, SCA)
		{
		}

		public static ColumnRowNel NCreateT<T>(Designer Ds, string _name, string _skin, int _def, string[] Akeys, BtnContainerRadio<aBtn>.FnRadioBindings FnChanged, float use_w = 0f, float tab_h = 0f, bool selectable = false, bool use_scroll = false) where T : aBtn
		{
			return ColumnRow.CreateT<T>((GameObject _Base, int default_val, int alloc_size, ObjCarrierCon _Carr, ScrollAppend SCA) => new ColumnRowNel(_Base, default_val, alloc_size, _Carr, SCA), Ds, _name, _skin, _def, Akeys, FnChanged, use_w, tab_h, selectable, use_scroll) as ColumnRowNel;
		}

		public ColumnRowNel setNewIcon(int index, bool flag = true)
		{
			aBtn aBtn = base.Get(index);
			if (aBtn != null)
			{
				ButtonSkinNelTab buttonSkinNelTab = aBtn.get_Skin() as ButtonSkinNelTab;
				if (buttonSkinNelTab != null)
				{
					buttonSkinNelTab.new_circle = flag;
				}
			}
			return this;
		}
	}
}
