using System;

namespace XX
{
	public class WarpDoorDrawer : RippleDoorDrawer
	{
		public WarpDoorDrawer(float _w, float _h, int _count)
			: base(_w, _h, _count)
		{
			this.Radius(28f, 28f);
		}

		public override RippleDoorDrawer WH(float _w, float _h)
		{
			base.WH(_w, _h);
			this.div_x = X.IntC(_w / (float)this.resolution_xy);
			this.div_y = X.IntC(_h / (float)this.resolution_xy);
			return this;
		}

		public WarpDoorDrawer Radius(float _rx, float _ry)
		{
			if (this.radius_x != _rx || this.radius_y != _ry)
			{
				this.radius_x = _rx;
				this.radius_y = _ry;
				this.need_check_dir = -1;
				this.div_c = X.Mx(3, X.IntC((_rx + _ry) * 3.1415927f / 4f / (float)this.resolution * 0.75f));
			}
			return this;
		}

		public override int putVertice(MeshDrawer Md, float w, float h)
		{
			float num = this.radius_x * X.ZLINE(w, this.w0 * 0.75f);
			float num2 = this.radius_y * X.ZLINE(h, this.h0 * 0.75f);
			w *= 0.015625f;
			h *= 0.015625f;
			float num3 = X.Mn(w / 2f, num * 0.015625f);
			float num4 = X.Mn(h / 2f, num2 * 0.015625f);
			float num5 = w - num3 * 2f;
			float num6 = h - num4 * 2f;
			float num7 = num5 / (float)this.div_x;
			float num8 = num6 / (float)this.div_y;
			float num9 = 1.5707964f / (float)this.div_c;
			int vertexMax = Md.getVertexMax();
			AIM aim = AIM.BL;
			for (int i = 0; i < 4; i++)
			{
				float num10 = w * 0.5f * (float)CAim._XD(aim, 1);
				float num11 = h * 0.5f * (float)CAim._YD(aim, 1);
				AIM aim2 = CAim.get_clockwise3(aim, false);
				float num12 = CAim.get_ag01(aim2, -0.25f) * 6.2831855f;
				for (int j = 0; j < this.div_c; j++)
				{
					Md.Pos(num10 + num3 * X.Cos(num12), num11 + num4 * X.Sin(num12), null);
					num12 += num9;
				}
				int num13 = CAim._XD(aim2, 1);
				int num14 = CAim._YD(aim2, 1);
				num10 += num3 * (float)num13;
				num11 += num4 * (float)num14;
				int num15 = ((i % 2 == 0) ? this.div_y : this.div_x);
				for (int k = 0; k < num15; k++)
				{
					Md.Pos(num10, num11, null);
					num10 += num7 * (float)num13;
					num11 += num8 * (float)num14;
				}
				aim = CAim.get_clockwise2(aim, false);
			}
			return Md.getVertexMax() - vertexMax;
		}

		public int resolution_xy = 35;

		public int resolution = 15;

		private float radius_x;

		private float radius_y;

		private int div_x;

		private int div_y;

		private int div_c;
	}
}
