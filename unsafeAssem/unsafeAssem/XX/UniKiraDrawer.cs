using System;

namespace XX
{
	public class UniKiraDrawer
	{
		public UniKiraDrawer()
		{
			this.rand_hash = X.xors();
		}

		public UniKiraDrawer Radius(float _min_w_px, float _max_w_px = -1f)
		{
			this.min_w = _min_w_px * 0.015625f;
			this.max_w = ((_max_w_px < 0f) ? this.min_w : (_max_w_px * 0.015625f));
			return this;
		}

		public UniKiraDrawer Dent(float _min_dent, float _max_dent = -1f)
		{
			this.min_dent = _min_dent;
			this.max_dent = ((_max_dent < 0f) ? this.min_dent : _max_dent);
			return this;
		}

		public UniKiraDrawer Focus(float _min_focus, float _max_focus = -1f)
		{
			this.min_focus = _min_focus;
			this.max_focus = ((_max_focus < 0f) ? (1f - this.min_focus) : _max_focus);
			return this;
		}

		public UniKiraDrawer Kira(int _kaku = 4)
		{
			this.kaku = _kaku;
			this.min_w = this.max_w;
			return this.Dent(0.125f, -1f).Focus(0.5f, -1f);
		}

		public UniKiraDrawer drawTo(MeshDrawer Md, float x, float y, float sagR, bool no_divide_ppu = false, float grd_level_in = 0f, float grd_level_out = 0f)
		{
			float num = 6.2831855f / (float)this.kaku;
			if (!no_divide_ppu)
			{
				x /= 64f;
				y /= 64f;
			}
			int vertexMax = Md.getVertexMax();
			if (grd_level_out > 0f)
			{
				if (grd_level_out != 1f)
				{
					MeshDrawer.ColBuf0.Set(Md.Col).blend(Md.ColGrd, grd_level_out);
				}
				else
				{
					C32 colGrd = Md.ColGrd;
				}
			}
			if (grd_level_in > 0f)
			{
				if (grd_level_in != 1f)
				{
					MeshDrawer.ColBuf1.Set(Md.Col).blend(Md.ColGrd, grd_level_in);
				}
				else
				{
					C32 colGrd2 = Md.ColGrd;
				}
			}
			Md.Pos(x, y, null);
			uint num2 = X.GETRAN2((int)((this.rand_hash & 255U) + (uint)this.kaku), (int)((this.rand_hash & 15U) + (uint)(this.kaku % 11)));
			float num3 = X.NI(this.min_w, this.max_w, X.RAN(num2, 1386));
			float num4 = num3;
			float num5 = X.Cos(sagR) * num4;
			float num6 = X.Sin(sagR) * num4;
			int num7 = X.Mx(3, X.IntR(this.max_w * 6.2831855f * 64f / 32f));
			int vertexMax2 = Md.getVertexMax();
			Md.Pos(x + num5, y + num6, null);
			for (int i = 0; i < this.kaku; i++)
			{
				num2 = X.GETRAN2((int)((this.rand_hash & 255U) + (uint)i), (int)((this.rand_hash & 15U) + (uint)(i % 11)));
				bool flag = i == this.kaku - 1;
				float num8 = sagR + num;
				float num9 = X.NI(this.min_focus, this.max_focus, X.RAN(num2, 1708));
				float num10 = num9 * 0.5f;
				float num11 = 1f - (1f - num9) * 0.5f;
				float num12 = sagR + num * num10;
				float num13 = sagR + num * num11;
				float num14 = X.NI(this.min_dent, this.max_dent, X.RAN(num2, 692));
				float num15 = (flag ? num3 : X.NI(this.min_w, this.max_w, X.RAN(num2, 1190)));
				float num16 = (num4 + num15) / 2f * num14;
				float num17 = X.Cos(num8) * num15;
				float num18 = X.Sin(num8) * num15;
				float num19 = X.Cos(num12) * num16;
				float num20 = X.Sin(num12) * num16;
				float num21 = X.Cos(num13) * num16;
				float num22 = X.Sin(num13) * num16;
				int num23 = (flag ? (num7 - 1) : num7);
				for (int j = 0; j < num23; j++)
				{
					Md.TriN(vertexMax).Tri(j).Tri(j - 1);
				}
				if (flag)
				{
					Md.TriN(vertexMax).TriN(vertexMax2).Tri(num7 - 2);
				}
				for (int k = 1; k <= num23; k++)
				{
					float num24 = (float)k / (float)num7;
					Md.Pos(x + X.BEZIER_I(num5, num19, num21, num17, num24), y + X.BEZIER_I(num6, num20, num22, num18, num24), null);
				}
				if (flag)
				{
					break;
				}
				num4 = num15;
				sagR = num8;
				num5 = num17;
				num6 = num18;
			}
			return this;
		}

		public float min_w = 0.625f;

		public float max_w = 0.625f;

		public float min_dent = 0.2f;

		public float max_dent = 0.2f;

		public float min_focus = 0.5f;

		public float max_focus = 0.5f;

		public int kaku = 4;

		public uint rand_hash;
	}
}
