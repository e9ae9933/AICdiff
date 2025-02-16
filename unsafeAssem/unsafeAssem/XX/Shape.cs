using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public static class Shape
	{
		public static void DrawMeshIcon(MeshDrawer Md, float cx, float cy, float wd, string shape, float val1 = 0f)
		{
			if (shape != null && shape == "lock")
			{
				Shape.DrawMeshIcon(Md, cx, cy, wd, 0, val1);
			}
		}

		public static void DrawMeshIcon(MeshDrawer Md, float cx, float cy, float wd, int shape_id, float val1 = 0f)
		{
			if (shape_id <= 1)
			{
				float num = 0.55f;
				if (shape_id == 1)
				{
					if (val1 < 0.125f)
					{
						Md.Poly(cx, cy, wd * 2f, 0f, 20, 0f, false, 0f, 0f);
					}
					else if (val1 < 2f)
					{
						float num2 = X.ZSIN(val1 - 0.125f, 1.875f);
						Md.Poly(cx, cy, wd * 2f, 0f, 20, wd * 0.15f * (1f - num2 * 0.98f), false, 0f, 0f);
					}
					wd *= 1f + 0.08f * (X.ZSIN(val1, 0.6f) - X.ZSINV(val1 - 0.6f, 0.6f));
				}
				Md.Rect(cx, cy + wd * (-0.5f + num / 2f), wd, wd * num, false);
				Md.Arc(cx, cy + (-0.5f + num) * wd, (1f - num) * 0.78f * wd, 3.1415927f * (0.95f * X.ZPOW(val1, 0.4f) - 0.12f * X.ZCOS(val1 - 0.4f, 0.6f)), 3.1415927f, X.Mx(2f, wd * 0.1f));
			}
		}

		public static void kadomaruRectExtImg(MeshDrawer Md, float x, float y, float w, float h, float radius, PxlImage Img = null, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x *= 0.015625f;
				y *= 0.015625f;
				w *= 0.015625f;
				h *= 0.015625f;
				radius *= 0.015625f;
			}
			Md.initForImg(Img ?? MTRX.EffCircle128, 0);
			radius = X.Mn(X.Mn(w, h) * 0.5f, radius);
			if (radius * 2.00001f >= w && radius * 2.00001f >= h)
			{
				Md.Rect(x, y, w, h, true);
				return;
			}
			float uv_left = Md.uv_left;
			float uv_top = Md.uv_top;
			float uv_width = Md.uv_width;
			float uv_height = Md.uv_height;
			float num = w * 0.5f;
			float num2 = h * 0.5f;
			float num3 = x - num;
			float num4 = y - num2;
			float num5 = x + num;
			float num6 = y + num2;
			float num7 = uv_left + uv_width;
			float num8 = uv_top + uv_height;
			if (radius * 2.00001f >= w)
			{
				float num9 = uv_top + uv_height * 0.5f;
				Md.TriRectBL(0).TriRectBL(4).Tri(5, 0, 3, false)
					.Tri(5, 3, 7, false);
				Md.PosUv(num3, num6 - radius, uv_left, num9, null).PosUv(num3, num6, uv_left, num8, null).PosUv(num5, num6, num7, num8, null)
					.PosUv(num5, num6 - radius, num7, num9, null);
				Md.PosUv(num3, num4, uv_left, uv_top, null).PosUv(num3, num4 + radius, uv_left, num9, null).PosUv(num5, num4 + radius, num7, num9, null)
					.PosUv(num5, num4, num7, uv_top, null);
				return;
			}
			if (radius * 2.00001f >= h)
			{
				float num10 = uv_left + uv_width * 0.5f;
				Md.TriRectBL(0).TriRectBL(4).Tri(3, 2, 5, false)
					.Tri(3, 5, 4, false);
				Md.PosUv(num3, num4, uv_left, uv_top, null).PosUv(num3, num6, uv_left, num8, null).PosUv(num3 + radius, num6, num10, num8, null)
					.PosUv(num3 + radius, num4, num10, uv_top, null);
				Md.PosUv(num5 - radius, num4, num10, uv_top, null).PosUv(num5 - radius, num6, num10, num8, null).PosUv(num5, num6, num7, num8, null)
					.PosUv(num5, num4, num7, uv_top, null);
				return;
			}
			float num11 = uv_left + uv_width * 0.5f;
			float num12 = uv_top + uv_height * 0.5f;
			Md.TriRectBL(0).TriRectBL(4).TriRectBL(8)
				.TriRectBL(12)
				.TriRectBL(1, 4, 7, 2)
				.TriRectBL(7, 6, 9, 8)
				.TriRectBL(13, 8, 11, 14)
				.TriRectBL(3, 2, 13, 12)
				.TriRectBL(2, 7, 8, 13);
			Md.PosUv(num3, num4, uv_left, uv_top, null).PosUv(num3, num4 + radius, uv_left, num12, null).PosUv(num3 + radius, num4 + radius, num11, num12, null)
				.PosUv(num3 + radius, num4, num11, uv_top, null);
			Md.PosUv(num3, num6 - radius, uv_left, num12, null).PosUv(num3, num6, uv_left, num8, null).PosUv(num3 + radius, num6, num11, num8, null)
				.PosUv(num3 + radius, num6 - radius, num11, num12, null);
			Md.PosUv(num5 - radius, num6 - radius, num11, num12, null).PosUv(num5 - radius, num6, num11, num8, null).PosUv(num5, num6, num7, num8, null)
				.PosUv(num5, num6 - radius, num7, num12, null);
			Md.PosUv(num5 - radius, num4, num11, uv_top, null).PosUv(num5 - radius, num4 + radius, num11, num12, null).PosUv(num5, num4 + radius, num7, num12, null)
				.PosUv(num5, num4, num7, uv_top, null);
		}

		public static void TriangleImg(MeshDrawer Md, float x0, float y0, float x1, float y1, float x2, float y2, PxlImage Img = null, AIM posa = AIM.L, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x0 *= 0.015625f;
				y0 *= 0.015625f;
				x1 *= 0.015625f;
				y1 *= 0.015625f;
				x2 *= 0.015625f;
				y2 *= 0.015625f;
			}
			float uv_left = Md.uv_left;
			float uv_top = Md.uv_top;
			float uv_width = Md.uv_width;
			float uv_height = Md.uv_height;
			Md.initForImg(Img ?? MTRX.IconWhite, 0);
			Md.Tri(0, 1, 2, false).Pos(x0, y0, null).Pos(x1, y1, null)
				.Pos(x2, y2, null);
			Vector2[] uvArray = Md.getUvArray();
			int num = Md.getVertexMax() - 3;
			switch (posa)
			{
			case AIM.L:
				uvArray[num] = new Vector2(uv_left, uv_top + uv_height * 0.5f);
				uvArray[num + 1] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top);
				return;
			case AIM.T:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left + uv_width * 0.5f, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top);
				return;
			case AIM.R:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height * 0.5f);
				return;
			case AIM.B:
				uvArray[num] = new Vector2(uv_left + uv_width * 0.5f, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				return;
			case AIM.LT:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				return;
			case AIM.TR:
				uvArray[num] = new Vector2(uv_left + uv_width, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				return;
			case AIM.BL:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top);
				return;
			case AIM.RB:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left + uv_width, uv_top);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				return;
			default:
				return;
			}
		}

		public const int LOCK = 0;

		public const int LOCK_UNLOCK_ANIM = 1;
	}
}
