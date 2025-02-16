using System;

namespace XX
{
	public class ClosingEyeDrawer
	{
		public ClosingEyeDrawer drawTo(MeshDrawer Md, float cx, float cy, float w, float h, float level)
		{
			cx *= 0.015625f;
			cy *= 0.015625f;
			if (level >= 1f)
			{
				Md.Rect(cx, cy, this.bounds_wh * 64f, this.bounds_hh * 64f, true);
				return this;
			}
			float num = w / 2f * 0.015625f;
			float num2 = h / 2f * 0.015625f;
			int num3 = X.Mx(X.IntC((w + h) / 48f), 2);
			Md.Tri(0, 4, 3, false).Tri(0, 1, 4 + num3, false).Tri(1, 2, 4 + 2 * num3, false)
				.Tri(2, 3, 4 + 3 * num3, false);
			Md.Pos(cx - this.bounds_wh, cy - this.bounds_hh, null);
			Md.Pos(cx - this.bounds_wh, cy + this.bounds_hh, null);
			Md.Pos(cx + this.bounds_wh, cy + this.bounds_hh, null);
			Md.Pos(cx + this.bounds_wh, cy - this.bounds_hh, null);
			for (int i = 0; i < 4; i++)
			{
				int num4 = num3 * i;
				int num5 = num3 * (i + 4);
				for (int j = 0; j < num3; j++)
				{
					int num6 = (num4 + 1) % (num3 * 4);
					int num7 = num6 + num3 * 4;
					Md.Tri(i - 4, num6, num4, false).Tri(num5, num4, num6, false);
					Md.Tri(num7, num5, num6, false);
					num4++;
					num5++;
				}
			}
			Md.Col = Md.ColGrd.Set(Md.Col).mulA(0.8f + 0.1f * X.ZPOW(level - 0.25f, 0.75f) + 0.1f * X.ZPOW(level - 0.75f, 0.25f)).C;
			this.protPos(Md, cx, cy, num, num2, level, (float)num3, 1f);
			Md.Col = Md.ColGrd.mulA(0f).C;
			this.protPos(Md, cx, cy, num, num2, 1f - X.Pow((1f - level) * 0.86f, 2), (float)num3, X.NI(1f, 0.6f, X.ZSIN(level, 0.4f)));
			return this;
		}

		private void protPos(MeshDrawer Md, float cx, float cy, float wh, float hh, float level, float divide, float yscl = 1f)
		{
			float num = X.ZSIN(X.ZCOS(level - 0.25f, 0.75f) * 0.75f + X.ZPOW(level) * 0.25f);
			float num2 = X.NI3(hh * 0.85f, wh * 0.3f, 0f, num) * X.ZPOW(level, 0.15f);
			float num3 = -X.NI(wh * 0.6f, 0f, X.ZPOW(level, 0.75f) * 0.25f + X.ZPOW(level, 0.25f) * 0.75f);
			float num4 = 1f / divide;
			float num5 = X.NI(-wh, -wh * 0.8f, X.ZPOW(level));
			float num6 = X.NI(-hh, 0f, X.ZCOS(level, 0.5f) * 0.75f + X.ZCOS(level - 0.25f, 0.5f) * 0.1925f + X.ZLINE(level - 0.78f, 0.22f) * 0.0625f);
			for (int i = 0; i < 4; i++)
			{
				float num7 = num5;
				float num8 = num6;
				float num9 = num3;
				float num10 = -0.5f * (1f - num) * 3.1415927f;
				if (i == 1 || i == 2)
				{
					num10 = -num10;
					num8 = -num6;
				}
				if (i == 3 || i == 2)
				{
					num10 = 3.1415927f - num10;
					num9 *= -1f;
					num7 = -num5;
				}
				float num11 = ((i == 1 || i == 3) ? (-num4) : num4);
				float num12 = ((i == 1 || i == 3) ? (1f + num11) : 0f);
				int num13 = 0;
				while ((float)num13 < divide)
				{
					float num14 = X.BEZIER_I(0f, num9, num7 + X.Cos(num10) * num2, num7, num12);
					float num15 = X.BEZIER_I(num8, num8, X.Sin(num10) * num2, 0f, num12);
					Md.Pos(cx + num14, cy + num15 * yscl, null);
					num12 += num11;
					num13++;
				}
			}
		}

		public float bounds_wh = IN.wh * 1.5f * 0.015625f;

		public float bounds_hh = IN.hh * 1.5f * 0.015625f;
	}
}
