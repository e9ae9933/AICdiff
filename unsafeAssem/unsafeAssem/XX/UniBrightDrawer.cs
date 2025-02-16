using System;
using UnityEngine;

namespace XX
{
	public class UniBrightDrawer
	{
		public UniBrightDrawer()
		{
			this.rand_hash = X.xors();
		}

		public UniBrightDrawer Radius(float pixel_min_r, float pixel_max_r)
		{
			this.min_r = pixel_min_r * 0.015625f;
			this.max_r = ((pixel_max_r < 0f) ? this.min_r : (pixel_max_r * 0.015625f));
			return this;
		}

		public UniBrightDrawer Thick(float _min_thick_px, float _max_thick_px = -1f)
		{
			this.min_thick = _min_thick_px * 0.015625f;
			this.max_thick = ((_max_thick_px > 0f) ? (_max_thick_px * 0.015625f) : this.min_thick);
			return this;
		}

		public UniBrightDrawer RotTime(float _rot_mint, float _rot_maxt = -1f)
		{
			this.rot_mint = _rot_mint;
			this.rot_maxt = ((_rot_maxt > 0f) ? _rot_maxt : this.rot_mint);
			return this;
		}

		public UniBrightDrawer Count(int c)
		{
			this.count = c;
			return this;
		}

		public UniBrightDrawer Col(Color32 _ColIn, float alpha = 1f)
		{
			this.ColIn.Set(_ColIn).mulA(alpha);
			this.ColOut.Set(_ColIn).mulA(0f);
			return this;
		}

		public UniBrightDrawer Col(Color32 _ColIn, Color32 _ColOut)
		{
			this.ColIn.Set(_ColIn);
			this.ColOut.Set(_ColOut);
			return this;
		}

		public UniBrightDrawer CenterCicle(float _radius_min_px, float _radius_max_px = -1f, float vib_t = 160f)
		{
			this.center_circle_min_r = _radius_min_px * 0.015625f;
			this.center_circle_max_r = ((_radius_max_px > 0f) ? (_radius_max_px * 0.015625f) : this.center_circle_min_r);
			this.center_circle_vib_t = vib_t;
			return this;
		}

		public UniBrightDrawer drawTo(MeshDrawer Md, float x, float y, float t, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x /= 64f;
				y /= 64f;
			}
			int num = this.count;
			int vertexMax = Md.getVertexMax();
			if (this.min_alpha == 1f)
			{
				Md.Pos(x, y, null);
			}
			int i = 0;
			while (i < num)
			{
				uint ran = X.GETRAN2((int)((this.rand_hash & 255U) + (uint)i), (int)((this.rand_hash & 15U) + (uint)(i % 11)));
				float num2 = X.RAN(ran, 2782) * 6.2831855f + (float)X.MPF((float)i / (float)num >= this.rot_anti_ratio) * 6.2831855f * (t / X.NI(this.rot_mint, this.rot_maxt, X.RAN(ran, 1368)));
				float num3 = 1f;
				if (this.agR_visible_range <= 0f)
				{
					goto IL_010E;
				}
				float num4 = X.Abs(X.angledifR(this.agR_visible_center, num2));
				if (num4 <= this.agR_visible_range)
				{
					goto IL_010E;
				}
				num3 = 1f - X.ZLINE(num4 - this.agR_visible_range, this.agR_visible_margin);
				if (num3 > 0f)
				{
					goto IL_010E;
				}
				IL_0237:
				i++;
				continue;
				IL_010E:
				float num5 = X.NI(this.min_alpha, 1f, X.RAN(ran, 1187)) * num3;
				float num6 = X.NI(this.min_r, this.max_r, X.RAN(ran, 1536));
				float num7 = X.NI(this.min_thick, this.max_thick, X.RAN(ran, 1367));
				Md.Col = Md.ColGrd.Set(this.ColIn).mulA(num5).C;
				Md.ColGrd.Set(this.ColOut).mulA(num5);
				float num8 = num7 / (num6 * 6.2831855f) * 6.2831855f;
				if (this.min_alpha == 1f)
				{
					Md.TriN(vertexMax).Tri(0).Tri(1);
				}
				else
				{
					Md.Tri012();
					Md.Pos(x, y, null);
				}
				Md.Pos(x + num6 * X.Cos(num2), y + num6 * X.Sin(num2), Md.ColGrd).Pos(x + num6 * X.Cos(num2 - num8), y + num6 * X.Sin(num2 - num8), Md.ColGrd);
				goto IL_0237;
			}
			if (this.center_circle_min_r > 0f)
			{
				Md.Col = this.ColIn.C;
				if (this.center_circle_min_r == this.center_circle_max_r || this.center_circle_vib_t <= 0f)
				{
					Md.Circle(x, y, this.center_circle_min_r, 0f, true, 1f, 0f);
				}
				else
				{
					Md.Circle(x, y, X.NI(this.center_circle_min_r, this.center_circle_max_r, 0.5f + 0.5f * X.COSI(t, this.center_circle_vib_t)), 0f, true, 1f, 0f);
				}
			}
			return this;
		}

		public float min_r = 1.71875f;

		public float max_r = 2.5f;

		public int count = 13;

		public float min_thick = 0.28125f;

		public float max_thick = 0.671875f;

		public float rot_mint = 160f;

		public float rot_maxt = 350f;

		public float min_alpha = 0.8f;

		public float rot_anti_ratio = 0.5f;

		public uint rand_hash;

		public float center_circle_min_r = 0.3125f;

		public float center_circle_max_r = 0.46875f;

		public float center_circle_vib_t = 160f;

		public float agR_visible_center;

		public float agR_visible_range;

		public float agR_visible_margin;

		private C32 ColIn = new C32(uint.MaxValue);

		private C32 ColOut = new C32(16777215);
	}
}
