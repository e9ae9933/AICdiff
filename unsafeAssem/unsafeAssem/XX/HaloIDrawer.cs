using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class HaloIDrawer
	{
		public HaloIDrawer(PxlImage _SrcImg)
		{
			this.SrcImg_ = _SrcImg;
		}

		public PxlImage SrcImg
		{
			get
			{
				return this.SrcImg_;
			}
			set
			{
				this.SrcImg_ = value;
				this.MI_ = null;
			}
		}

		public MImage MI
		{
			get
			{
				if (this.MI_ == null && this.SrcImg_ != null)
				{
					this.MI_ = MTRX.getMI(this.SrcImg_.pChar);
				}
				return this.MI_;
			}
		}

		public void allocTriVer(MeshDrawer Md, int attr, int count = 1)
		{
			int num = (((attr & 2) != 0) ? 2 : 1) * count;
			Md.allocTri(Md.getTriMax() + 24 * num, 60).allocVer(Md.getVertexMax() + 9 * num, 64);
		}

		public void drawTo(MeshDrawer Md, int attr, float x, float y, float w, float h, float agR, float dent_lv = 1f, bool no_divide_ppu = false)
		{
			if (this.SrcImg_ == null)
			{
				return;
			}
			if (!no_divide_ppu)
			{
				x *= 0.015625f;
				y *= 0.015625f;
				w *= 0.015625f;
				h *= 0.015625f;
			}
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			if ((attr & 4) == 0)
			{
				this.allocTriVer(Md, attr, 1);
				Md.Translate(x, y, true).Rotate(agR, true);
				Md.initForImg(this.SrcImg_, 0);
				if ((attr & 8) != 0)
				{
					float uv_width = Md.uv_width;
					Md.uv_width *= 0.5f;
					Md.uv_left += uv_width - Md.uv_width;
				}
			}
			Vector2[] uvArray = Md.getUvArray();
			Md.TriRectBL(0, 1, 2, 3).Tri(4, 0, 5, false).Tri(0, 3, 5, false)
				.Tri(0, 6, 1, false)
				.Tri(7, 6, 0, false)
				.Tri(8, 7, 0, false)
				.Tri(8, 0, 4, false);
			Md.Pos(0f, 0f, null).Pos(0f, h, null).Pos(w * dent_lv, h * dent_lv, null)
				.Pos(w, 0f, null);
			Md.InputImageUv();
			int vertexMax = Md.getVertexMax();
			float num = Md.uv_left + Md.uv_width;
			float num2 = Md.uv_top + Md.uv_height;
			Md.Pos(0f, -h, null).Pos(w * dent_lv, -h * dent_lv, null);
			uvArray[vertexMax++].Set(Md.uv_left, num2);
			uvArray[vertexMax++].Set(num, num2);
			float num3 = -w;
			float num4 = dent_lv;
			if ((attr & 1) == 0)
			{
				num3 = -h * 0.8f;
				num4 = 1.4f;
			}
			float num5 = h * num4;
			Md.Pos(num3 * num4, num5, null).Pos(num3, 0f, null).Pos(num3 * num4, -num5, null);
			uvArray[vertexMax++].Set(num, num2);
			uvArray[vertexMax++].Set(num, Md.uv_top);
			uvArray[vertexMax++].Set(num, num2);
			if ((attr & 2) != 0)
			{
				Md.Rotate(1.5707964f, true);
				this.drawTo(Md, (attr & -3) | 4, 0f, 0f, w, h, 0f, dent_lv, true);
			}
			if ((attr & 4) == 0)
			{
				Md.setCurrentMatrix(currentMatrix, false);
			}
		}

		private PxlImage SrcImg_;

		private MImage MI_;

		public const int ATTR_REV = 1;

		public const int ATTR_ROT90 = 2;

		public const int ATTR_AGAIN = 4;

		public const int ATTR_TALE = 8;
	}
}
