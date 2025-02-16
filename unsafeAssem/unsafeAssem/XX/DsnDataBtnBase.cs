using System;

namespace XX
{
	public class DsnDataBtnBase : DsnData
	{
		public void setBtn(aBtn B)
		{
			B.setHoverManager(this.HovCurs);
			if (this.unselectable > 0)
			{
				B.unselectable(this.unselectable >= 2);
				return;
			}
			B.z_push_click = this.z_push_click;
			B.hover_to_select = this.hover_to_select;
			B.navi_auto_fill = this.navi_auto_fill;
			B.click_to_select = this.click_to_select;
			B.navigated_to_click = this.navigated_to_click;
			B.locked_click = this.locked_click;
		}

		public bool z_push_click = true;

		public bool navi_auto_fill;

		public bool navigated_to_click;

		public bool hover_to_select = true;

		public bool click_to_select = true;

		public bool locked_click = true;

		public int unselectable;

		public string click_snd;

		public HoverCursManager HovCurs;
	}
}
