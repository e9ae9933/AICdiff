using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class ObjCarrierConBlock : ObjCarrierCon, IDesignerBlock, IAlphaSetable
	{
		public ObjCarrierConBlock(Transform _Trs)
		{
			this.Trs = _Trs;
		}

		public ObjCarrierConBlock ItemSizePx(float x, float y, int clmn = -1)
		{
			this.item_w = x * 0.015625f;
			this.item_h = y * 0.015625f;
			return this;
		}

		public ObjCarrierConBlock ItemSize(float x, float y, int clmn = -1)
		{
			this.item_w = x;
			this.item_h = y;
			return this;
		}

		public float get_swidth_px()
		{
			return (this.bounds_w + this.item_w) * 64f * this.scale;
		}

		public float get_sheight_px()
		{
			return (X.Abs(this.bounds_h) + this.item_h) * 64f * this.scale;
		}

		public Transform getTransform()
		{
			return this.Trs;
		}

		public virtual void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
			Component component;
			if (this.Trs.gameObject.TryGetComponent(typeof(IDesignerBlock), out component))
			{
				(component as IDesignerBlock).AddSelectableItems(ASlc, only_front);
			}
		}

		public virtual void setAlpha(float f)
		{
			Component component;
			if (this.Trs.gameObject.TryGetComponent(typeof(IAlphaSetable), out component))
			{
				(component as IAlphaSetable).setAlpha(f);
			}
		}

		protected Transform Trs;

		public float item_w;

		public float item_h;

		public float scale = 1f;
	}
}
