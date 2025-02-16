using System;

namespace XX
{
	public class DesignerWhite : Designer
	{
		public override Designer addTab(string name, float _w = 0f, float _h = 0f, float min_w = 0f, float min_h = 0f, bool use_scroll = false)
		{
			DesignerWhite designerWhite = base.addTabT<DesignerWhite>(name, _w, _h, min_w, min_h, use_scroll);
			designerWhite.scroll_normal_color = this.scroll_normal_color;
			designerWhite.scroll_push_color = this.scroll_push_color;
			this.initScrollBoxColor(designerWhite.getScrollBox());
			return designerWhite;
		}

		protected override void initScrollBoxColor(ScrollBox Scr)
		{
			if (Scr != null)
			{
				Scr.setSliderColor(C32.d2c(this.scroll_normal_color), C32.d2c(this.scroll_push_color));
			}
			base.initScrollBoxColor(Scr);
		}

		public uint scroll_normal_color = 2583691263U;

		public uint scroll_push_color = 285212671U;
	}
}
