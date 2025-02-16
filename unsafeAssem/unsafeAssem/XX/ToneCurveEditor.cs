using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX
{
	public class ToneCurveEditor
	{
		private bool need_refine_map
		{
			set
			{
				if (value)
				{
					ToneCurveEditor.CalcedTarget = null;
				}
			}
		}

		public ToneCurveEditor()
		{
			this.AAPos = new List<Vector2>[4];
			for (int i = 0; i < 4; i++)
			{
				List<Vector2> list = (this.AAPos[i] = new List<Vector2>(3));
				list.Add(new Vector2(0f, 0f));
				list.Add(new Vector2(1f, 1f));
			}
			if (ToneCurveEditor.Acol_map == null)
			{
				ToneCurveEditor.Acol_map_u = new uint[256];
				ToneCurveEditor.Acol_map = new Color[256];
			}
		}

		public static Color32 chn2Col(int channel)
		{
			uint num;
			switch (channel)
			{
			case 0:
				num = 4278190080U;
				break;
			case 1:
				num = 4294901760U;
				break;
			case 2:
				num = 4278255360U;
				break;
			default:
				num = 4278190335U;
				break;
			}
			return C32.d2c(num);
		}

		public Color[] getColorMap()
		{
			if (ToneCurveEditor.CalcedTarget != this)
			{
				ToneCurveEditor.CalcedTarget = this;
				for (int i = 0; i < 4; i++)
				{
					List<Vector2> list = this.AAPos[i];
					int num = -1;
					int num2 = 0;
					int num3 = X.IntR(list[0].x * 255f);
					float num4 = list[0].y;
					float num5 = num4;
					float num6 = 1f / (float)X.Mx(1, num3 - num2);
					for (int j = 0; j <= 255; j++)
					{
						if (j >= num3)
						{
							num++;
							num2 = j;
							num3 = ((num >= list.Count - 1) ? 256 : X.IntR(list[num + 1].x * 255f));
							num4 = num5;
							num5 = ((num < list.Count - 1) ? list[num + 1].y : num4);
							num6 = 1f / (float)X.Mx(1, num3 - num2);
						}
						uint num7 = (uint)X.IntR(255f * X.saturate(X.NI(num4, num5, (float)(j - num2) * num6)));
						uint num8 = ((i == 0) ? 0U : ToneCurveEditor.Acol_map_u[j]);
						num8 |= num7 << ((i == 0) ? 24 : ((i == 1) ? 16 : ((i == 2) ? 8 : 0)));
						ToneCurveEditor.Acol_map_u[j] = num8;
					}
				}
				for (int k = 0; k <= 255; k++)
				{
					ToneCurveEditor.Acol_map[k] = C32.d2c(ToneCurveEditor.Acol_map_u[k]);
				}
			}
			return ToneCurveEditor.Acol_map;
		}

		public void readFromBytes(ByteArray Ba)
		{
			int num = Ba.readByte();
			for (int i = 0; i < num; i++)
			{
				int num2 = (int)Ba.readUByte();
				List<Vector2> list = ((i < 4) ? this.AAPos[i] : new List<Vector2>(num2));
				list.Clear();
				if (list.Capacity < num2)
				{
					list.Capacity = num2;
				}
				for (int j = 0; j < num2; j++)
				{
					Vector2 vector = new Vector2((float)Ba.readUByte() / 255f, (float)Ba.readUByte() / 255f);
					list.Add(vector);
				}
			}
		}

		public readonly List<Vector2>[] AAPos;

		private const int CHANNEL_MAX = 4;

		private static Color[] Acol_map;

		private static uint[] Acol_map_u;

		private static ToneCurveEditor CalcedTarget;
	}
}
