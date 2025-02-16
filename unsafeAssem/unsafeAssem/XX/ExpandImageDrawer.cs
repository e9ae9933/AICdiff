using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ExpandImageDrawer
	{
		public ExpandImageDrawer()
		{
			this.FD_zoom = new FnZoom(X.ZSIN);
		}

		public ExpandImageDrawer Divide(int i = 1)
		{
			this.divide_count = X.Mx(i, 1);
			return this;
		}

		public void drawTo(MeshDrawer Md, float x, float y, float scale, float agR, PxlLayer L, float expand_level, float layer_draw_scale = 1f)
		{
			Md.initForImg(L.Img, 0);
			float base_x = Md.base_x;
			float base_y = Md.base_y;
			Md.base_x += x * 0.015625f;
			Md.base_y += y * 0.015625f;
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md.Rotate(agR, false);
			this.drawTo(Md, L.x * scale, -L.y * scale, scale * layer_draw_scale, expand_level);
			Md.setCurrentMatrix(currentMatrix, false);
		}

		public void drawTo(MeshDrawer Md, float x, float y, float scale, float expand_level)
		{
			int num = this.divide_count * 2 + 1;
			int num2 = num + 1;
			Md.allocTri(6 * num * num, 60);
			for (int i = 0; i < num; i++)
			{
				int num3 = i * num2;
				for (int j = 0; j < num; j++)
				{
					Md.TriRectBL(num3, num3 + num2, num3 + num2 + 1, num3 + 1);
					num3++;
				}
			}
			float num4 = Md.texture_width * Md.uv_width * scale * 0.015625f * 0.5f;
			float num5 = Md.texture_height * Md.uv_height * scale * 0.015625f * 0.5f;
			x *= 0.015625f;
			y *= 0.015625f;
			float uv_left = Md.uv_left;
			float uv_top = Md.uv_top;
			float uv_width = Md.uv_width;
			float uv_height = Md.uv_height;
			UV_SETTYPE uv_settype = Md.uv_settype;
			Md.allocVer(num2 * num2, 64);
			for (int k = 0; k < num2; k++)
			{
				float num6 = ((float)k - (float)num * 0.5f) / ((float)num * 0.5f);
				float num7 = X.Abs(num6);
				float num8 = ((k == 0 || k == num) ? num6 : (X.NI(num7, this.FD_zoom(num7), expand_level) * (float)X.MPF(num6 > 0f)));
				float num9 = uv_top + uv_height * (float)k / (float)num;
				for (int l = 0; l < num2; l++)
				{
					float num10 = ((float)l - (float)num * 0.5f) / ((float)num * 0.5f);
					float num11 = X.Abs(num10);
					float num12 = ((l == 0 || l == num) ? num10 : (X.NI(num11, this.FD_zoom(num11), expand_level) * (float)X.MPF(num10 > 0f)));
					float num13 = uv_left + uv_width * (float)l / (float)num;
					Md.uvRectN(num13, num9);
					Md.Pos(x + num4 * num12, y + num5 * num8, null);
				}
			}
			Md.uv_settype = uv_settype;
			Md.uv_left = uv_left;
			Md.uv_top = uv_top;
		}

		public int divide_count = 1;

		private FnZoom FD_zoom;

		private string fd_zoom_key = "ZSIN";
	}
}
