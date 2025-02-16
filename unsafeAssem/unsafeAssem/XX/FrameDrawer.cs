using System;
using PixelLiner;

namespace XX
{
	public class FrameDrawer
	{
		public FrameDrawer()
		{
			this.AImg = new PxlImage[8];
		}

		public FrameDrawer DrawTo(MeshDrawer Md, float x, float y, float w, float h)
		{
			float num = x - w / 2f;
			float num2 = y - h / 2f;
			float num3 = num + w;
			float num4 = num2 + h;
			float num5 = w;
			float num6 = h;
			float num7 = num;
			float num8 = num2;
			if (this.corner_setted)
			{
				PxlImage pxlImage = null;
				bool flag2;
				bool flag = (flag2 = false);
				if (this.AImg[4] != null)
				{
					pxlImage = this.AImg[4];
				}
				else if (this.AImg[5] != null)
				{
					pxlImage = this.AImg[5];
					flag2 = true;
				}
				else if (this.AImg[6] != null)
				{
					pxlImage = this.AImg[6];
					flag = true;
				}
				else if (this.AImg[7] != null)
				{
					pxlImage = this.AImg[7];
					flag = (flag2 = true);
				}
				if (pxlImage != null)
				{
					num5 -= ((float)pxlImage.width + this.h_shift_x) * this.corner_scale_x;
					num6 -= (float)pxlImage.height * this.corner_scale_y;
					num7 += ((float)pxlImage.width + this.h_shift_x) * this.corner_scale_x;
					Md.initForImg(pxlImage, 0);
					Md.DrawFlipGraph(num, num4, 0f, 1f, this.corner_scale_x, this.corner_scale_y, null, flag2, flag);
				}
				pxlImage = null;
				flag = (flag2 = false);
				if (this.AImg[5] != null)
				{
					pxlImage = this.AImg[5];
				}
				else if (this.AImg[4] != null)
				{
					pxlImage = this.AImg[4];
					flag2 = true;
				}
				else if (this.AImg[7] != null)
				{
					pxlImage = this.AImg[7];
					flag = true;
				}
				else if (this.AImg[6] != null)
				{
					pxlImage = this.AImg[6];
					flag = (flag2 = true);
				}
				if (pxlImage != null)
				{
					Md.initForImg(pxlImage, 0);
					Md.DrawFlipGraph(num3, num4, 1f, 1f, this.corner_scale_x, this.corner_scale_y, null, flag2, flag);
				}
				pxlImage = null;
				flag = (flag2 = false);
				if (this.AImg[6] != null)
				{
					pxlImage = this.AImg[6];
				}
				else if (this.AImg[7] != null)
				{
					pxlImage = this.AImg[7];
					flag2 = true;
				}
				else if (this.AImg[4] != null)
				{
					pxlImage = this.AImg[4];
					flag = true;
				}
				else if (this.AImg[5] != null)
				{
					pxlImage = this.AImg[5];
					flag = (flag2 = true);
				}
				if (pxlImage != null)
				{
					num5 -= (float)pxlImage.width * this.corner_scale_x;
					num6 -= ((float)pxlImage.height + this.v_shift_y) * this.corner_scale_y;
					num8 += ((float)pxlImage.height + this.v_shift_y) * this.corner_scale_y;
					Md.initForImg(pxlImage, 0);
					Md.DrawFlipGraph(num, num2, 0f, 0f, this.corner_scale_x, this.corner_scale_y, null, flag2, flag);
				}
				pxlImage = null;
				flag = (flag2 = false);
				if (this.AImg[7] != null)
				{
					pxlImage = this.AImg[7];
				}
				else if (this.AImg[6] != null)
				{
					pxlImage = this.AImg[6];
					flag2 = true;
				}
				else if (this.AImg[5] != null)
				{
					pxlImage = this.AImg[5];
					flag = true;
				}
				else if (this.AImg[4] != null)
				{
					pxlImage = this.AImg[4];
					flag = (flag2 = true);
				}
				if (pxlImage != null)
				{
					Md.initForImg(pxlImage, 0);
					Md.DrawFlipGraph(num3, num2, 1f, 0f, this.corner_scale_x, this.corner_scale_y, null, flag2, flag);
				}
			}
			if (num5 > 0f && this.ImgH != null)
			{
				Md.initForImg(this.ImgH, 0);
				Md.DrawExtendGraphBL(num7, num2 + this.h_shift_y, num5 + 1f, (float)this.ImgH.height * this.corner_scale_y, null, false, false);
				Md.DrawExtendGraphBL(num7, num4 - this.h_shift_y - (float)this.ImgH.height * this.corner_scale_y, num5 + 1f, (float)this.ImgH.height * this.corner_scale_y, null, false, true);
			}
			if (num6 > 0f && this.ImgV != null)
			{
				Md.initForImg(this.ImgV, 0);
				Md.DrawExtendGraphBL(num + this.v_shift_x, num8, (float)this.ImgV.width * this.corner_scale_x, num6 + 1f, null, false, false);
				Md.DrawExtendGraphBL(num3 - this.v_shift_x - (float)this.ImgV.width * this.corner_scale_x, num8, (float)this.ImgV.width * this.corner_scale_x, num6 + 1f, null, true, false);
			}
			return this;
		}

		public FrameDrawer Scale(float _x, float _y = -1000f)
		{
			this.corner_scale_x = _x;
			this.corner_scale_y = ((_y == -1000f) ? this.corner_scale_x : _y);
			return this;
		}

		public FrameDrawer LT(PxlImage Img)
		{
			this.AImg[4] = Img;
			this.corner_setted = true;
			return this;
		}

		public FrameDrawer TR(PxlImage Img)
		{
			this.AImg[5] = Img;
			this.corner_setted = true;
			return this;
		}

		public FrameDrawer RB(PxlImage Img)
		{
			this.AImg[7] = Img;
			this.corner_setted = true;
			return this;
		}

		public FrameDrawer BL(PxlImage Img)
		{
			this.AImg[6] = Img;
			this.corner_setted = true;
			return this;
		}

		public FrameDrawer L(PxlImage Img)
		{
			this.AImg[0] = Img;
			this.center_setted = true;
			return this;
		}

		public FrameDrawer T(PxlImage Img)
		{
			this.AImg[1] = Img;
			this.center_setted = true;
			return this;
		}

		public FrameDrawer R(PxlImage Img)
		{
			this.AImg[2] = Img;
			this.center_setted = true;
			return this;
		}

		public FrameDrawer B(PxlImage Img)
		{
			this.AImg[3] = Img;
			this.center_setted = true;
			return this;
		}

		public FrameDrawer H(PxlImage Img, float shiftx = 0f, float shifty = 0f)
		{
			this.ImgH = Img;
			this.h_shift_x = shiftx;
			this.h_shift_y = shifty;
			return this;
		}

		public FrameDrawer V(PxlImage Img, float shiftx = 0f, float shifty = 0f)
		{
			this.ImgV = Img;
			this.v_shift_x = shiftx;
			this.v_shift_y = shifty;
			return this;
		}

		private PxlImage[] AImg;

		private PxlImage ImgH;

		private PxlImage ImgV;

		public float corner_scale_x = 1f;

		public float corner_scale_y = 1f;

		private bool center_setted;

		private bool corner_setted;

		public float v_shift_x;

		public float v_shift_y;

		public float h_shift_y;

		public float h_shift_x;
	}
}
