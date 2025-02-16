using System;

namespace XX
{
	public class DripDrawer
	{
		public DripDrawer(float _w = -1f)
		{
			if (this.w > 0f)
			{
				this.Set(_w, -1f);
			}
		}

		public DripDrawer Set(float _w, float _sclaley = -1f)
		{
			this.w = _w * 0.015625f;
			this.scaley = ((_sclaley >= 0f) ? _sclaley : this.scaley);
			return this;
		}

		public DripDrawer drawTo(MeshDrawer _Md, float x, float y, float h, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x /= 64f;
				y /= 64f;
				h /= 64f;
			}
			if (this.w == 0f)
			{
				return this;
			}
			this.Md = _Md;
			float num = h / this.w;
			this.Md.base_x += x;
			this.Md.base_y += y;
			float num2 = 5f + this.w * 64f / 45f + h * 64f / 45f;
			int num3 = X.Mx(2, X.IntC(num2));
			int num4 = num3;
			int num5 = X.IntR((float)num4 * (1f + X.ZLINE(num, 2f) * 1f));
			int num6 = num4 + num5;
			int num7 = 0;
			float num8 = this.w * (0.5f - 0.34f * X.ZCOS(num - 0.7f, 2f));
			float num9 = this.w * (X.ZSIN(num, 1.3f) * 0.3f + X.ZCOS(num - 1.4f, 2f) * 0.2f);
			float num10 = this.w * (0.2f + 0.21f * X.ZCOS(num, 1.9f) - 0.06f * X.ZCOS(num - 1.7f, 2.2f));
			float num11 = h * (0.2f + 0.15f * X.ZCOS(num, 1.8f) + 0.12f * X.ZCOS(num - 1f, 1.2f));
			float num12 = 0f - X.ZLINE(num, 0.6f) * 1.5707964f - X.ZLINE(num - 1.2f, 2.2f) * 0.9424779f + X.ZCOS(num - 2.4f, 1.2f) * 0.62831855f;
			float num13 = this.w * (0.05f + 0.2f * X.ZCOS(num - 0.2f, 1.3f) + 0.08f * X.ZCOS(num - 1.1f, 1.1f));
			float num14 = this.w * (0.11f + 0.24f * X.ZCOS(num, 1.4f) + 0.3f * X.ZCOS(num - 1.2f, 1.3f));
			float num15 = this.w * (0.1f + 0.4f * X.ZLINE(num - 0.9f, 2.4f) - 0.2f * X.ZCOS(num - 1.1f, 2.4f));
			float num16 = num11 * 0.7f;
			float num17 = X.Mn(num11 * 1.4f, h * 0.75f);
			float num18 = X.Cos(num12);
			float num19 = X.Sin(num12);
			int num20 = num6 + 1;
			int num21 = num20 + 1;
			int num22 = num21 + 1;
			int num23 = X.Mx(X.IntC((float)(num4 * 2 / 3)), 1);
			this.rep = 0;
			while (this.rep < 2)
			{
				int num24 = num20;
				for (int i = num7 + 1; i <= num4; i++)
				{
					this.Tri(i - 1, num24, i);
					if (i == num23)
					{
						num24 = num21;
						this.Tri(num20, num21, num23);
					}
				}
				this.Tri(num21, num22, num4);
				for (int j = num4 + 1; j <= num6; j++)
				{
					this.Tri(j - 1, num22, (j == num6 && this.rep == 1) ? (num3 + num5) : j);
				}
				if (this.rep == 0)
				{
					num7 = num22 + 1;
					num23 += num7;
					num4 = num7 + num3;
					num6 = num4 + num5;
				}
				this.rep++;
			}
			float num25 = this.w / 2f;
			this.rep = 0;
			while (this.rep < 2)
			{
				float num26 = (float)X.MPF(this.rep == 1);
				float num27 = num26 * num25;
				float num28 = num26 * num8;
				this.Md.Pos(num27, 0f, null);
				for (int k = 1; k <= num3; k++)
				{
					this.Md.Pos(X.BEZIER_I(num28, num27 - num26 * num9, num27 - num26 * num10 + num26 * num18 * num13, num27 - num26 * num10, (float)k / (float)num3), X.BEZIER_I(0f, 0f, -num11 - num19 * num13, -num11, (float)k / (float)num3) * this.scaley, null);
				}
				int num29 = num5 - ((this.rep == 1) ? 1 : 0);
				for (int l = 1; l <= num29; l++)
				{
					this.Md.Pos(X.BEZIER_I(num27 - num26 * num10, num27 - num26 * num10 - num26 * num18 * num14, num26 * num15, 0f, (float)l / (float)num5), X.BEZIER_I(-num11, -num11 + num19 * num14, -h, -h, (float)l / (float)num5) * this.scaley, null);
				}
				if (this.rep == 0)
				{
					this.Md.Pos(0f, 0f, null);
					this.Md.Pos(0f, -num16 * this.scaley, null);
					this.Md.Pos(0f, -num17 * this.scaley, null);
				}
				this.rep++;
			}
			this.Md.base_x -= x;
			this.Md.base_y -= y;
			this.Md = null;
			return this;
		}

		private DripDrawer Tri(int a, int b, int c)
		{
			if (this.rep == 0)
			{
				this.Md.Tri(a).Tri(b).Tri(c);
			}
			else
			{
				this.Md.Tri(a).Tri(c).Tri(b);
			}
			return this;
		}

		public float w = 0.625f;

		public float scaley = 1f;

		private MeshDrawer Md;

		private int ia;

		private int ib;

		private int ic;

		private int rep;
	}
}
