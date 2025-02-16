using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class ObjCarrierConBlockBtnContainer<T> : ObjCarrierConBlock, IAlphaSetable where T : aBtn
	{
		public ObjCarrierConBlockBtnContainer(BtnContainer<T> _BCon, Transform _Trs)
			: base(_Trs)
		{
			this.BCon = _BCon;
		}

		public override void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
			this.BCon.AddSelectableItems(ASlc, only_front);
		}

		public override void setAlpha(float value)
		{
			this.BCon.setAlpha(value);
		}

		public BtnContainer<T> BCon;
	}
}
