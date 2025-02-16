using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public abstract class WaterShakeDrawerT<T> where T : WaveItem
	{
		public WaterShakeDrawerT(int _count)
		{
			this.count = _count;
			this.random_seed = X.xors(65535);
			this.AWave = new List<T>();
		}

		public void drawBL(MeshDrawer Md, float lx, float ly, float w, float top_h)
		{
			this.drawBL(Md, lx, ly, w, top_h, IN.totalframe);
		}

		protected abstract T createWaveItem();

		public virtual void waveProgress(float w, int t)
		{
			int num = this.count;
			while (this.AWave.Count < this.count)
			{
				this.AWave.Add(default(T));
			}
			for (int i = 0; i < num; i++)
			{
				T t2 = this.AWave[i];
				uint ran = X.GETRAN2(i + this.random_seed, this.random_seed % 7 + 3 + i % 6);
				if (t2 == null)
				{
					t2 = (this.AWave[i] = this.createWaveItem());
					t2.spd = X.NI(this.min_spd, this.max_spd, X.RAN(ran, 2197));
					t2.sinw = X.NI(this.min_w, this.max_w, X.RAN(ran, 2404));
					t2.h = X.NI(this.min_h, this.max_h, X.RAN(ran, 632));
					t2.x = (-0.5f + X.RAN(ran, 1753)) * w;
					t2.t0 = t;
				}
				int num2 = t - t2.t0;
				if (num2 > 0)
				{
					t2.progress(num2, w);
				}
			}
		}

		public void drawBL(MeshDrawer Md, float lx, float ly, float w, float top_h, int t)
		{
			int num = this.count;
			if (this.auto_wave_progress)
			{
				this.waveProgress(w, t);
			}
			int num2 = X.IntC(w / this.resolution);
			float num3 = 0f;
			float num4 = ((this.under_resolution <= 0f) ? w : this.under_resolution);
			int num5 = X.Mx(1, X.IntC(w / num4));
			Color32 color = ((this.grd_level_t == 1f) ? Md.ColGrd.C : MTRX.cola.Set(Md.Col).blend(Md.ColGrd, this.grd_level_t).C);
			Color32 color2 = ((this.grd_level_b == 1f) ? Md.ColGrd.C : MTRX.cola.Set(Md.Col).blend(Md.ColGrd, this.grd_level_b).C);
			float num6 = 0f;
			float num7 = ((num5 == 1) ? w : (num4 / 2f));
			int num8 = 0;
			for (int i = 0; i <= num2; i++)
			{
				if (i < num2)
				{
					Md.Tri(num8, num5 + 1 + i, num5 + 1 + i + 1, false);
				}
				num6 += this.resolution;
				if (num6 >= num7 && num8 < num5)
				{
					Md.Tri(num8, num5 + 1 + X.Mn(i, num2 - 1) + 1, num8 + 1, false);
					num8++;
					num7 = ((num8 == num5) ? w : (num7 + num4));
				}
			}
			if (num8 < num5)
			{
				Md.Tri(num5 - 1, num5 + 1 + num2, num5, false);
			}
			num6 = 0f;
			Md.Col = color2;
			for (int j = 0; j <= num5; j++)
			{
				if (j == num5)
				{
					num6 = w;
				}
				Md.PosD(lx + num6, ly, null);
				num6 += num4;
			}
			for (int k = 0; k <= num2; k++)
			{
				if (k == num2)
				{
					num3 = w;
				}
				float num9 = top_h + this.getShiftY(((this.calc_include_draw_x != -1024f) ? (lx + this.calc_include_draw_x) : 0f) + num3);
				if (this.max_draw_height >= 0f)
				{
					num9 = X.Mn(this.max_draw_height, num9);
				}
				if (this.min_draw_height >= 0f)
				{
					num9 = X.Mx(this.min_draw_height, num9);
				}
				if (this.grd_level_t != this.grd_level_b)
				{
					Md.Col = MTRX.cola.Set(color2).blend(color, X.ZLINE(num9, top_h + this.max_h)).C;
				}
				Md.PosD(lx + num3, ly + num9, null);
				num3 += this.resolution;
			}
		}

		public float getShiftY(float _x)
		{
			float num = 0f;
			for (int i = 0; i < this.count; i++)
			{
				num += this.AWave[i].get_y(_x, this.wave_align);
			}
			return num;
		}

		protected List<T> AWave;

		public int count;

		public float min_w = 110f;

		public float max_w = 277f;

		public float min_h = 6f;

		public float max_h = 20f;

		public float min_spd = 1.1f;

		public float max_spd = 4.3f;

		public float resolution = 6f;

		public float under_resolution;

		public float max_draw_height = -1f;

		public float min_draw_height;

		public int random_seed;

		public float grd_level_b;

		public float grd_level_t;

		public bool auto_wave_progress = true;

		public float calc_include_draw_x = -1024f;

		public ALIGNY wave_align;
	}
}
