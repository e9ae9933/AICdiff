using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public sealed class HideScreenClickable : HideScreen
	{
		protected override void Awake()
		{
			base.Awake();
			base.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			this.AIgnoreRect = new List<DRect>();
		}

		public override void OnEnable()
		{
			base.OnEnable();
		}

		public HideScreenClickable ClearRects()
		{
			this.AIgnoreRect.Clear();
			return this;
		}

		public HideScreenClickable IgnoreRect(IDesignerBlock Blk, float marg_pixel = 0f)
		{
			Vector3 position = Blk.getTransform().position;
			return this.IgnoreRect(Blk, position.x, position.y, marg_pixel);
		}

		public HideScreenClickable IgnoreRect(IDesignerBlock Blk, float pos_ux, float pos_uy, float marg_pixel = 0f)
		{
			float num = Blk.get_swidth_px() * 0.015625f / 2f;
			float num2 = Blk.get_sheight_px() * 0.015625f / 2f;
			return this.IgnoreRect(Blk.getTransform().gameObject.name, pos_ux - num, pos_uy - num2, num * 2f, num2 * 2f, marg_pixel);
		}

		public HideScreenClickable IgnoreRect(string key, float x, float y, float w, float h, float marg_pixel = 0f)
		{
			marg_pixel /= 64f;
			this.AIgnoreRect.Add(new DRect(key, x - marg_pixel, y - marg_pixel, w + marg_pixel * 2f, h + marg_pixel * 2f, 0f));
			return this;
		}

		public override bool OnPointerDown()
		{
			bool flag = false;
			int count = this.AIgnoreRect.Count;
			Vector2 mousePos = IN.getMousePos(null);
			for (int i = 0; i < count; i++)
			{
				if (this.AIgnoreRect[i].isContaining(mousePos, 0f))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.deactivate(false);
				return false;
			}
			return true;
		}

		private List<DRect> AIgnoreRect;
	}
}
