using System;

namespace XX
{
	public class SphereDrawer
	{
		public SphereDrawer Kaku(int k)
		{
			this.kaku = k;
			return this;
		}

		public SphereDrawer Slice(int k)
		{
			this.slice = k;
			return this;
		}

		public SphereDrawer AgR(float _agR)
		{
			this.agR = _agR;
			return this;
		}

		public SphereDrawer drawTo(MeshDrawer Md, float x, float y, float w, float h, bool no_divide_ppu = false, float col_grd_in = 0f, float col_grd_out = 0f)
		{
			return this.drawRectBLTo(Md, x - w * 0.5f, y - h * 0.5f, w, h, x, y, no_divide_ppu, col_grd_in, col_grd_out);
		}

		public SphereDrawer drawRectBLTo(MeshDrawer Md, float rx, float ry, float rw, float rh, float in_x, float in_y, bool no_divide_ppu = false, float col_grd_in = 0f, float col_grd_out = 0f)
		{
			if (!no_divide_ppu)
			{
				rx *= 0.015625f;
				ry *= 0.015625f;
				rw *= 0.015625f;
				rh *= 0.015625f;
				in_x *= 0.015625f;
				in_y *= 0.015625f;
			}
			if (this.slice < 1)
			{
				return this;
			}
			C32 c = null;
			if (col_grd_in != col_grd_out)
			{
				c = MTRX.colb.Set(Md.Col).blend(Md.ColGrd, col_grd_in);
			}
			else if (this.fnCalcColorSphere != null)
			{
				c = MTRX.colb;
			}
			Md.Pos(in_x, in_y, (this.fnCalcColorSphere != null) ? this.fnCalcColorSphere(this, c, in_x, in_y, in_x, in_y, 0f, 0f) : c);
			float num = rw / 2f;
			float num2 = rh / 2f;
			float num3 = rx + num;
			float num4 = ry + num2;
			int num5 = 1;
			float num6 = 2f / (float)(this.slice + 1);
			float num7 = num6;
			for (int i = 0; i < this.slice; i++)
			{
				int num8 = (this.fix_kaku ? this.kaku : X.IntR(X.NI(3f, (float)this.kaku, num7)));
				int num9 = 0;
				int num10 = 0;
				for (int j = 0; j < num8; j++)
				{
					Md.Tri(-num5 + num10, (j + 1) % num8, j, false);
					num9 += num5;
					if (num9 >= num8 && num5 != 1)
					{
						num9 -= num8;
						int num11 = (num10 + 1) % num5;
						Md.Tri(-num5 + num10, -num5 + num11, (j + 1) % num8, false);
						num10 = num11;
					}
				}
				float num12 = 6.2831855f / (float)num8;
				float num13 = this.agR;
				for (int k = 0; k < num8; k++)
				{
					float num14 = X.Cos(num13);
					float num15 = X.Sin(num13);
					num14 = X.NI(in_x, num3 + num * num14, num7);
					num15 = X.NI(in_y, num4 + num2 * num15, num7);
					Md.Pos(num14, num15, (this.fnCalcColorSphere != null) ? this.fnCalcColorSphere(this, c, num14, num15, in_x, in_y, num7, num13) : ((c == null) ? null : c.Set(Md.Col).blend(Md.ColGrd, X.NI(col_grd_in, col_grd_out, num7))));
					num13 += num12;
				}
				num7 = ((i == this.slice - 1) ? 1f : (num7 + (1f - num7) * num6));
				num5 = num8;
			}
			return this;
		}

		public int slice = 2;

		public float agR = 0.7853982f;

		public int kaku = 4;

		public bool fix_kaku;

		public SphereDrawer.FnCalcColorSphere fnCalcColorSphere;

		public delegate C32 FnCalcColorSphere(SphereDrawer Sphere, C32 BufC, float draw_xu, float draw_yu, float in_xu, float in_yu, float radius_ratio, float agR);
	}
}
