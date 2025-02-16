using System;
using UnityEngine;

namespace XX
{
	public class WindDrawer
	{
		public WindDrawer()
		{
			this.ran = (uint)X.xors(1048576);
			if (WindDrawer.Astart == null)
			{
				WindDrawer.Astart = new float[2];
				WindDrawer.Asyuuki = new float[2];
				WindDrawer.Alevel = new float[2];
			}
			this.line_len = this.line_len_;
		}

		public WindDrawer Set(uint _ran, float _thick, float _line_len, float _move_len, float _maxt, float _wave_v_max)
		{
			this.ran = _ran;
			this.line_thick = _thick;
			this.line_len_ = _line_len;
			this.move_len_ = _move_len;
			this.maxt = _maxt;
			this.wave_v_max = _wave_v_max;
			this.line_len_ratio = this.line_len_ / this.move_len_;
			return this;
		}

		public bool drawTo(MeshDrawer Md, float sx, float sy, float agR, float af)
		{
			if (af < 0f)
			{
				return true;
			}
			if (af >= this.maxt)
			{
				return false;
			}
			for (int i = 0; i < 2; i++)
			{
				WindDrawer.Asyuuki[i] = 1f / (X.NI(0.3f, 0.52f, X.RAN(this.ran, 2652 + i * 420)) + (float)i * 0.6f + 0.6f);
				WindDrawer.Astart[i] = X.NI(0f, 0.8f + (float)i, X.RAN(this.ran, 3313 + i * 51));
				WindDrawer.Alevel[i] = X.NI(0.6f, 1f, X.RAN(this.ran, 2457 + i * 35));
			}
			float num = af / this.maxt;
			float line_len = this.line_len;
			X.ZLINE(num, 0.125f);
			float num2 = num + 0.0625f;
			float num3 = X.Mx(num2 - this.line_len_ratio, 0f);
			int num4 = (int)((num2 - num3) * this.move_len_ / this.resolution) + 2;
			float num5 = this.move_len_ * 0.015625f;
			float num6 = this.line_thick * 0.5f * 0.015625f;
			float num7 = this.wave_v_max * 0.015625f;
			float num8 = -num5 * 0.5f;
			int num9 = X.MMX(1, (int)((float)num4 * 0.125f), num4 / 2);
			float num10 = (num2 - num3) / (float)num4;
			float num11 = num3;
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md.Rotate(agR, false).TranslateP(sx, sy, false);
			float num12 = X.ZLINE(this.maxt - af, this.maxt * 0.25f);
			bool flag = true;
			for (int j = 0; j < num4; j++)
			{
				float num13 = num11 * num5 + num8;
				float num14 = 0f;
				for (int k = 0; k < 2; k++)
				{
					num14 += WindDrawer.Alevel[k] * X.Cos0(num11 * WindDrawer.Asyuuki[k] - WindDrawer.Astart[k]);
				}
				num14 *= num7;
				float num15 = (0.125f + 0.875f * X.ZLINE((float)j, (float)num9)) * (0.125f + 0.875f * X.ZLINE((float)(num4 - 1 - j), (float)num9));
				float num16 = num6 * num15;
				if (num15 < 1f || flag)
				{
					Md.Col = C32.MulA(this.Col, num15 * num12);
					if (num15 < 1f)
					{
						flag = true;
					}
				}
				if (j > 0)
				{
					Md.Tri(-2, -1, 1, false).Tri(-2, 1, 0, false);
				}
				Md.Pos(num13, num14 - num16, null).Pos(num13, num14 + num16, null);
				num11 += num10;
			}
			Md.setCurrentMatrix(currentMatrix, false);
			return true;
		}

		public float line_len
		{
			get
			{
				return this.line_len_;
			}
			set
			{
				this.line_len_ = value;
				this.line_len_ratio = this.line_len_ / this.move_len_;
			}
		}

		public float move_len
		{
			get
			{
				return this.move_len_;
			}
			set
			{
				this.move_len_ = value;
				this.line_len_ratio = this.line_len_ / this.move_len_;
			}
		}

		public float line_thick = 8f;

		private float line_len_ = 800f;

		private float move_len_ = 1500f;

		private float line_len_ratio = 1f;

		public float maxt = 70f;

		public float wave_v_max = 140f;

		public uint ran;

		public float resolution = 18f;

		public Color32 Col = C32.d2c(1728053247U);

		private static float[] Astart;

		private static float[] Asyuuki;

		private static float[] Alevel;

		private const int WAVE_MAX = 2;
	}
}
