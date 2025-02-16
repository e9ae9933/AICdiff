using System;

namespace XX
{
	public class ButtonSkinRadio : ButtonSkinCheckBox
	{
		public ButtonSkinRadio(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
		}

		protected override void DrawDoughnut(MeshDrawer Md, float pleft, bool black_border)
		{
			if (black_border)
			{
				Md.Poly(pleft + this.box_wh, 0f, this.box_wh - 1.5f, 0f, 14, 3f, false, 0f, 0f);
				return;
			}
			Md.Poly(pleft + this.box_wh, 0f, this.box_wh - 1f, 0f, 14, 1f, false, 0f, 0f);
		}
	}
}
