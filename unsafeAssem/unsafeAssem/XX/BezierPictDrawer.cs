using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class BezierPictDrawer
	{
		public BezierPictDrawer PtCenterPx(float sx, float sy, float cx, float cy, float bezier_len, float bezier_agR, float dx, float dy)
		{
			this.u0x = sx * 0.015625f;
			this.u0y = sy * 0.015625f;
			this.u1x = cx * 0.015625f;
			this.u1y = cy * 0.015625f;
			this.u2x = bezier_len * 0.015625f;
			this.u2y = bezier_agR;
			this.u3x = dx * 0.015625f;
			this.u3y = dy * 0.015625f;
			this.center_mode = true;
			return this.clearAngleCache();
		}

		public BezierPictDrawer PtPx(float sx, float sy, float b1x, float b1y, float b2x, float b2y, float dx, float dy)
		{
			this.u0x = sx * 0.015625f;
			this.u0y = sy * 0.015625f;
			this.u1x = b1x * 0.015625f;
			this.u1y = b1y * 0.015625f;
			this.u2x = b2x * 0.015625f;
			this.u2y = b2y * 0.015625f;
			this.u3x = dx * 0.015625f;
			this.u3y = dy * 0.015625f;
			this.center_mode = false;
			return this.clearAngleCache();
		}

		public BezierPictDrawer drawTo(MeshDrawer Md, float x, float y, float h, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x *= 0.015625f;
				y *= 0.015625f;
				h *= 0.015625f;
			}
			int num = 0;
			for (int i = 0; i < this.resolution; i++)
			{
				Md.Tri(num, num + 1, num + 3, false).Tri(num, num + 3, num + 2, false);
				num += 2;
			}
			h *= 0.5f;
			float num2 = this.u0x;
			float num3 = this.u0y;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			if (this.center_mode)
			{
				num = this.resolution / 2;
				float num8 = this.u2x * X.Cos(this.u2y);
				float num9 = this.u2x * X.Sin(this.u2y);
				num4 = this.u1x - num8;
				num5 = this.u1y - num9;
				num6 = this.u1x + num8;
				num7 = this.u1y + num9;
			}
			UV_SETTYPE uv_settype = Md.uv_settype;
			float base_x = Md.base_x;
			float base_y = Md.base_y;
			Md.base_x += x;
			Md.base_y += y;
			float uv_left = Md.uv_left;
			float uv_top = Md.uv_top;
			float num10 = uv_left;
			float num11 = Md.uv_width * this.divide;
			float num12 = uv_top + Md.uv_height;
			for (int j = 1; j <= this.resolution; j++)
			{
				float num14;
				float num15;
				if (this.center_mode)
				{
					if (j < num)
					{
						float num13 = this.divide * 2f * (float)j;
						num14 = X.BEZIER_I(this.u0x, this.u0x, num4, this.u1x, num13);
						num15 = X.BEZIER_I(this.u0y, this.u0y, num5, this.u1y, num13);
					}
					else
					{
						float num16 = this.divide * 2f * (float)j - 1f;
						num14 = X.BEZIER_I(this.u1x, num6, this.u3x, this.u3x, num16);
						num15 = X.BEZIER_I(this.u1y, num7, this.u3y, this.u3y, num16);
					}
				}
				else
				{
					float num17 = this.divide * (float)j;
					num14 = X.BEZIER_I(this.u0x, this.u1x, this.u2x, this.u3x, num17);
					num15 = X.BEZIER_I(this.u0y, this.u1y, this.u2y, this.u3y, num17);
				}
				Vector2 vector;
				if (this.Acs.Count < j)
				{
					float num18 = X.GAR2(num2, num3, num14, num15);
					List<Vector2> acs = this.Acs;
					vector = new Vector2(X.Cos(num18), X.Sin(num18));
					acs.Add(vector);
				}
				else
				{
					vector = this.Acs[j - 1];
				}
				vector *= h;
				if (j == 1)
				{
					Md.uvRectN(num10, uv_top).Pos(num2 + vector.y, num3 - vector.x, null).uvRectN(num10, num12)
						.Pos(num2 - vector.y, num3 + vector.x, null);
				}
				num10 += num11;
				Md.uvRectN(num10, uv_top).Pos(num14 + vector.y, num15 - vector.x, null).uvRectN(num10, num12)
					.Pos(num14 - vector.y, num15 + vector.x, null);
				num2 = num14;
				num3 = num15;
			}
			Md.uvRectN(uv_left, uv_top);
			Md.base_x = base_x;
			Md.base_y = base_y;
			Md.uv_settype = uv_settype;
			return this;
		}

		public BezierPictDrawer drawDebugTo(MeshDrawer Md, float x, float y, float h, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x *= 0.015625f;
				y *= 0.015625f;
				h *= 0.015625f;
			}
			h *= 0.5f;
			float num = this.u0x;
			float num2 = this.u0y;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			int num7 = 0;
			float base_x = Md.base_x;
			float base_y = Md.base_y;
			Md.base_x += x;
			Md.base_y += y;
			if (this.center_mode)
			{
				num7 = this.resolution / 2;
				float num8 = this.u2x * X.Cos(this.u2y);
				float num9 = this.u2x * X.Sin(this.u2y);
				num3 = this.u1x - num8;
				num4 = this.u1y - num9;
				num5 = this.u1x + num8;
				num6 = this.u1y + num9;
				Md.Col = C32.d2c(2008538357U);
				Md.Line(num3, num4, num5, num6, 0.015625f, true, 0f, 0f);
				Md.Box(num3, num4, 0.09375f, 0.09375f, 0.015625f, true);
				Md.Box(num5, num6, 0.09375f, 0.09375f, 0.015625f, true);
			}
			Md.Col = C32.d2c(4282791676U);
			Md.Box(this.u0x, this.u0y, 0.09375f, 0.09375f, 0.015625f, true);
			Md.Box(this.u3x, this.u3y, 0.09375f, 0.09375f, 0.015625f, true);
			for (int i = 1; i <= this.resolution; i++)
			{
				float num11;
				float num12;
				if (this.center_mode)
				{
					if (i < num7)
					{
						float num10 = this.divide * 2f * (float)i;
						num11 = X.BEZIER_I(this.u0x, num3, num3, this.u1x, num10);
						num12 = X.BEZIER_I(this.u0y, num4, num4, this.u1y, num10);
					}
					else
					{
						float num13 = this.divide * 2f * (float)i - 1f;
						num11 = X.BEZIER_I(this.u1x, num5, num5, this.u3x, num13);
						num12 = X.BEZIER_I(this.u1y, num6, num6, this.u3y, num13);
					}
				}
				else
				{
					float num14 = this.divide * (float)i;
					num11 = X.BEZIER_I(this.u0x, this.u1x, this.u2x, this.u3x, num14);
					num12 = X.BEZIER_I(this.u0y, this.u1y, this.u2y, this.u3y, num14);
				}
				Vector2 vector;
				if (this.Acs.Count < i)
				{
					float num15 = X.GAR2(num, num2, num11, num12);
					List<Vector2> acs = this.Acs;
					vector = new Vector2(X.Cos(num15), X.Sin(num15));
					acs.Add(vector);
				}
				else
				{
					vector = this.Acs[i - 1];
				}
				vector *= h;
				if (i == 1)
				{
					Md.Poly(num + vector.y, num2 - vector.x, 0.0625f, 0f, 4, 0.03125f, true, 0f, 0f);
					Md.Poly(num - vector.y, num2 + vector.x, 0.0625f, 0f, 4, 0.03125f, true, 0f, 0f);
				}
				Md.Poly(num11 + vector.y, num12 - vector.x, 0.0625f, 0f, 4, 0.03125f, true, 0f, 0f);
				Md.Poly(num11 - vector.y, num12 + vector.x, 0.0625f, 0f, 4, 0.03125f, true, 0f, 0f);
				num = num11;
				num2 = num12;
			}
			Md.base_x = base_x;
			Md.base_y = base_y;
			return this;
		}

		public BezierPictDrawer clearAngleCache()
		{
			this.Acs.Clear();
			return this;
		}

		public int resolution
		{
			get
			{
				return this.resolution_;
			}
			set
			{
				this.resolution_ = value;
				this.divide = 1f / (float)this.resolution_;
				if (this.Acs.Capacity < this.resolution_)
				{
					this.Acs.Capacity = this.resolution_;
				}
			}
		}

		public float u0x;

		public float u0y;

		public float u1x;

		public float u1y;

		public float u2x;

		public float u2y;

		public float u3x;

		public float u3y;

		private List<Vector2> Acs = new List<Vector2>(8);

		private int resolution_ = 8;

		private float divide = 0.125f;

		public bool center_mode;
	}
}
