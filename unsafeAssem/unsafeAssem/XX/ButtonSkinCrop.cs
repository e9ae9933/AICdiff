using System;

namespace XX
{
	public class ButtonSkinCrop : ButtonSkin
	{
		public ButtonSkinCrop(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w * 0.015625f, _h * 0.015625f)
		{
			this.Md = base.makeMesh(null);
		}

		public void setCropArea(float x, float y, float _w, float _h)
		{
			this.crop_x = x;
			this.crop_y = y;
			this.crop_w = _w;
			this.crop_h = _h;
			this.fine_flag = true;
		}

		public override ButtonSkin Fine()
		{
			this.Md.clear(false, false);
			if (this.alpha == 0f || !this.use_crop_)
			{
				return this;
			}
			if (this.B.isHovered())
			{
				this.Md.Col = C32.d2c(this.color_hover);
			}
			else
			{
				this.Md.Col = C32.d2c(this.color);
			}
			this.Md.RectDoughnut(0f, 0f, this.w, this.h, this.crop_x, this.crop_y, this.crop_w, this.crop_h, true, 0f, 0f, false);
			this.Md.updateForMeshRenderer(false);
			return base.Fine();
		}

		public bool use_crop
		{
			get
			{
				return this.use_crop_;
			}
			set
			{
				this.use_crop_ = value;
				this.fine_flag = true;
			}
		}

		private MeshDrawer Md;

		private uint color = 2566914048U;

		private uint color_hover = 855638016U;

		private float crop_w;

		private float crop_h;

		private float crop_x;

		private float crop_y;

		private bool use_crop_ = true;
	}
}
