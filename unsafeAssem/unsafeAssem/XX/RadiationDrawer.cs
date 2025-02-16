using System;
using UnityEngine;

namespace XX
{
	public class RadiationDrawer
	{
		public RadiationDrawer()
		{
			this.ran0 = X.xors();
		}

		public bool needDraw(float time)
		{
			if (this.fine_intv <= 0)
			{
				return this.pre_drawn != -this.fine_intv;
			}
			return (int)(time / (float)this.fine_intv) != this.pre_drawn;
		}

		public RadiationDrawer clear()
		{
			this.pre_drawn = -1;
			return this;
		}

		public RadiationDrawer drawTo(MeshDrawer Md, float x, float y, float w, float h, float t, float time, bool no_divide_ppu = false, float t_randomise_ratio = 1f)
		{
			if (!no_divide_ppu)
			{
				x *= 0.015625f;
				y *= 0.015625f;
				t *= 0.015625f;
				w *= 0.015625f;
				h *= 0.015625f;
			}
			float base_x = Md.base_x;
			float base_y = Md.base_y;
			Md.base_x += x;
			Md.base_y += y;
			int num = (int)((w + h) * this.count_ratio / 1.40625f);
			this.pre_drawn = ((this.fine_intv <= 0) ? (-this.fine_intv) : ((int)(time / (float)this.fine_intv)));
			float num2 = 1f / (float)num;
			for (int i = 0; i < num; i++)
			{
				uint ran = X.GETRAN2(this.pre_drawn + 13 + i * 7 + (int)(this.ran0 & 65535U), 2 + i % 9 + (int)(this.ran0 % 49U));
				Vector3 vector = X.RANBORDER(w, h, (this.fix_intv_randomize < 0f) ? X.RAN(ran, 2575) : (num2 * (0.5f + (float)i + -0.5f * X.RAN(ran, 2952) * this.fix_intv_randomize)));
				AIM aim = CAim.get_clockwise2((AIM)vector.z, false);
				AIM aim2 = CAim.get_clockwise2((AIM)vector.z, true);
				float num3 = t;
				if (t_randomise_ratio != 1f)
				{
					num3 *= X.NI(1f, t_randomise_ratio, X.RAN(ran, 2611));
				}
				float num4 = X.GAR2(0f, 0f, vector.x, vector.y);
				float num5 = ((CAim._XD((int)vector.z, 1) != 0) ? w : h) * X.NI(this.min_len_ratio, this.max_len_ratio, X.RAN(ran, 1334));
				Md.Tri(1, 0, 2, false);
				Md.Pos(vector.x + num3 * (float)CAim._XD(aim, 1), vector.y + num3 * (float)CAim._YD(aim, 1), null);
				Md.Pos(vector.x + num3 * (float)CAim._XD(aim2, 1), vector.y + num3 * (float)CAim._YD(aim2, 1), null);
				Md.Pos(vector.x - num5 * X.Cos(num4), vector.y - num5 * X.Sin(num4), null);
			}
			Md.base_x = base_x;
			Md.base_y = base_y;
			return this;
		}

		public int fine_intv = 5;

		private int pre_drawn = -1;

		public float min_len_ratio = 0.16f;

		public float max_len_ratio = 0.3f;

		public float fix_intv_randomize = -1f;

		public float count_ratio = 1f;

		public uint ran0;
	}
}
