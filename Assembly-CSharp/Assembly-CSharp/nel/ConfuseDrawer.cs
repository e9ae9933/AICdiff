using System;
using XX;

namespace nel
{
	public sealed class ConfuseDrawer
	{
		public bool isActive()
		{
			return this.t >= 0f;
		}

		public ConfuseDrawer activate()
		{
			if (this.t < 0f)
			{
				this.t = 0f;
			}
			return this;
		}

		public ConfuseDrawer deactivate()
		{
			if (this.t >= 0f)
			{
				this.t = -1f;
			}
			return this;
		}

		public bool run(float fcnt, MeshDrawer Md)
		{
			this.t += (float)X.MPF(this.t >= 0f) * fcnt;
			return ConfuseDrawer.drawUIStatusS(Md, this.t, 40f);
		}

		public static bool drawUIStatusS(MeshDrawer Md, float t, float maxt)
		{
			bool flag;
			float num;
			float num2;
			float num3;
			if (t >= 0f)
			{
				flag = t <= maxt + 4f;
				num = X.ZLINE(t, maxt);
				num2 = 0.03f;
				num3 = 1f - num2 * 20f;
			}
			else
			{
				num = X.ZLINE(maxt + t, maxt);
				num2 = 0f;
				num3 = 1f;
				if (num <= 0f)
				{
					return false;
				}
				flag = true;
			}
			Md.Col = MTRX.ColWhite;
			Md.uvRect(-IN.wh * 0.015625f, -IN.hh * 0.015625f, IN.w * 0.015625f, IN.h * 0.015625f, true, false);
			for (int i = 0; i < 20; i++)
			{
				float num4 = num - num2 * (float)i;
				if (num4 <= 0f)
				{
					break;
				}
				num4 = X.ZLINE(num4, num3);
				float num5 = X.frac(0.35f * (float)i);
				float num6 = X.frac(2.205f * (float)i);
				float num7 = X.frac(5.4005f * (float)i);
				uint ran = X.GETRAN2(i, i % 3);
				float num8 = X.NI(-18, 18, num6);
				float num9 = X.NI(-8, 4, num7);
				num8 += (-1f + 2f * num5) * 140f;
				float num10 = X.NI(19, 24, X.RAN(ran, 2934)) * (X.ZSIN(num4, 0.675f) * 1.25f - X.ZCOS(num4 - 0.5f, 0.5f) * 0.25f);
				Md.Circle(num8, num9, num10, 0f, false, 0f, 0f);
			}
			return flag;
		}

		public float t;

		public float FADE_T = 40f;
	}
}
