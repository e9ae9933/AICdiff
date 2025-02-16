using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class WormDrawer
	{
		public WormDrawer(PxlImage Img)
		{
			this.Tx = Img.get_I();
			this.TxRec = Img.RectIUv;
		}

		public WormDrawer DefineSplit(float _scale = 1f, params float[] Ax)
		{
			this.iwidth = this.TxRec.width * (float)this.Tx.width;
			this.iheight = this.TxRec.height * (float)this.Tx.height;
			this.scale = _scale;
			this.split_cnt = Ax.Length + 1;
			this.APt = new WormDrawer.WormPoint[this.split_cnt];
			for (int i = 0; i < this.split_cnt - 1; i++)
			{
				this.APt[i] = new WormDrawer.WormPoint(((i == 0) ? 0f : (Ax[i] - Ax[i - 1])) * this.scale, Ax[i] / this.iwidth);
			}
			this.APt[this.split_cnt - 1] = new WormDrawer.WormPoint(this.iwidth - Ax[Ax.Length - 1], 1f);
			return this;
		}

		public WormDrawer LineUp(float x, float y, float cubic_height, float agR)
		{
			float num = x + this.iwidth * X.Cos(agR) * this.scale;
			float num2 = y + this.iwidth * X.Sin(agR) * this.scale;
			float num3 = (x + num) / 2f + cubic_height * X.Cos(agR + 1.5707964f);
			float num4 = (y + num2) / 2f + cubic_height * X.Sin(agR + 1.5707964f);
			for (int i = 0; i < this.split_cnt; i++)
			{
				WormDrawer.WormPoint wormPoint = this.APt[i];
				wormPoint.Set(X.BEZIER_I(x, num3, num3, num, wormPoint.ratio), X.BEZIER_I(y, num4, num4, num2, wormPoint.ratio));
			}
			for (int j = 1; j < this.split_cnt; j++)
			{
				WormDrawer.WormPoint wormPoint = this.APt[j];
				WormDrawer.WormPoint wormPoint2 = this.APt[j - 1];
				wormPoint.agR = X.GAR(wormPoint.x, wormPoint.y, wormPoint2.x, wormPoint2.y);
				wormPoint.SetDepert(wormPoint2.x - X.Cos(wormPoint.agR) * wormPoint2.base_len, wormPoint2.y - X.Sin(wormPoint.agR) * wormPoint2.base_len);
			}
			this.APt[0].agR = this.APt[1].agR;
			return this;
		}

		public WormDrawer TranslateR(float len, float agR)
		{
			WormDrawer.WormPoint wormPoint;
			WormDrawer.WormPoint wormPoint2;
			for (int i = this.split_cnt - 1; i >= 1; i--)
			{
				wormPoint = this.APt[i];
				wormPoint2 = this.APt[i - 1];
				float num = X.GAR(wormPoint.x, wormPoint.y, wormPoint2.x, wormPoint2.y);
				wormPoint.agR = X.VALWALKANGLER(wormPoint.agR, num, 0.052359883f);
				wormPoint.SetDepert(-wormPoint.base_len * X.Cos(wormPoint.agR), -wormPoint.base_len * X.Sin(wormPoint.agR));
			}
			wormPoint = this.APt[0];
			try
			{
				if (len > 2f)
				{
					float num2 = X.angledifR(wormPoint.agR, agR);
					agR = wormPoint.agR + X.MMX(-this.max_curve_agR_head, num2, this.max_curve_agR_head);
					wormPoint.agR = X.VALWALKANGLER(wormPoint.agR, agR, 0.052359883f);
					wormPoint.SetDepert(wormPoint.x + len * X.Cos(wormPoint.agR), wormPoint.y + len * X.Sin(wormPoint.agR));
				}
				else
				{
					wormPoint.SetDepert(wormPoint.x + len * X.Cos(agR), wormPoint.y + len * X.Sin(agR));
				}
			}
			catch
			{
				X.dl("エラーなんでおきるの？？たぶんNaNか？？？ agR= " + wormPoint.agR.ToString(), null, false, false);
			}
			wormPoint2 = this.APt[0];
			for (int j = 1; j < this.split_cnt; j++)
			{
				wormPoint = this.APt[j];
				float num3 = X.angledifR(wormPoint2.agR, wormPoint.agR);
				wormPoint.agR = wormPoint2.agR + X.MMX(-this.max_curve_agR, num3, this.max_curve_agR);
				wormPoint2 = wormPoint;
			}
			return this.Update();
		}

		public WormDrawer Update()
		{
			WormDrawer.WormPoint wormPoint = null;
			for (int i = 0; i < this.split_cnt; i++)
			{
				WormDrawer.WormPoint wormPoint2 = this.APt[i];
				float num = ((i == 0) ? wormPoint2.dx : (wormPoint.x + wormPoint2.dx));
				float num2 = ((i == 0) ? wormPoint2.dy : (wormPoint.y + wormPoint2.dy));
				wormPoint2.x = X.MULWALK(wormPoint2.x, num, wormPoint2.move_ratio);
				wormPoint2.y = X.MULWALK(wormPoint2.y, num2, wormPoint2.move_ratio);
				wormPoint = wormPoint2;
			}
			return this;
		}

		public WormDrawer DrawTo(MeshDrawer Md)
		{
			float num = X.Mx(this.head_ratio_, 0f);
			float num2 = 1f - X.Mx(-this.head_ratio_, 0f);
			WormDrawer.WormPoint wormPoint = null;
			float num3 = this.iheight * this.scale / 2f;
			float num4 = 1f / (float)this.Tx.width;
			float num5 = 1f / (float)this.Tx.height;
			float num6 = this.wave_x * num4;
			int num7 = 0;
			int num8 = this.split_cnt - 1;
			for (int i = 0; i < this.split_cnt; i++)
			{
				WormDrawer.WormPoint wormPoint2 = this.APt[i];
				if (wormPoint2.ratio > num)
				{
					num7 = i - 1;
					break;
				}
			}
			for (int j = this.split_cnt - 2; j >= 0; j--)
			{
				WormDrawer.WormPoint wormPoint2 = this.APt[j];
				if (wormPoint2.ratio <= num2)
				{
					num8 = j + 1;
					break;
				}
			}
			if (num7 >= num8)
			{
				return this;
			}
			for (int k = num7; k < num8; k++)
			{
				int num9 = (k - num7) * 2;
				Md.Tri(num9).Tri(num9 + 3).Tri(num9 + 1)
					.Tri(num9)
					.Tri(num9 + 2)
					.Tri(num9 + 3);
			}
			if (num7 > 0)
			{
				wormPoint = this.APt[num7 - 1];
			}
			for (int l = num7; l <= num8; l++)
			{
				WormDrawer.WormPoint wormPoint2 = this.APt[l];
				float num10 = ((l == 0) ? wormPoint2.agR : (wormPoint2.agR + X.angledifR(wormPoint2.agR, wormPoint.agR) * 0.5f));
				float num11 = X.Cos(num10 - 1.5707964f);
				float num12 = X.Sin(num10 - 1.5707964f);
				float num13 = ((l == 0) ? 0f : (num6 * X.COSI((float)(IN.totalframe + 23 * l), 200f)));
				float num14 = ((l == num7) ? num : ((l == num8) ? num2 : wormPoint2.ratio));
				float num15 = this.TxRec.xMax - num14 * this.TxRec.width + num13;
				for (int m = 0; m < 2; m++)
				{
					Md.uvRectN(num15, (m == 0) ? this.TxRec.y : this.TxRec.yMax);
					float num16 = (float)((m == 0) ? 1 : (-1));
					float num17 = wormPoint2.x + num3 * num11 * num16;
					float num18 = wormPoint2.y + num3 * num12 * num16;
					if (l == num7)
					{
						WormDrawer.WormPoint wormPoint3 = this.APt[l + 1];
						float num19 = (num - wormPoint2.ratio) / (wormPoint3.ratio - wormPoint2.ratio);
						Md.PosD(X.NI(num17, wormPoint3.x + num3 * num11 * num16, num19), X.NI(num18, wormPoint3.y + num3 * num12 * num16, num19), null);
					}
					else if (l == num8)
					{
						float num20 = (num2 - wormPoint.ratio) / (wormPoint2.ratio - wormPoint.ratio);
						Md.PosD(X.NI(wormPoint.x + num3 * num11 * num16, num17, num20), X.NI(wormPoint.y + num3 * num12 * num16, num18, num20), null);
					}
					else
					{
						Md.PosD(num17, num18, null);
					}
				}
				wormPoint = wormPoint2;
			}
			return this;
		}

		public float x
		{
			get
			{
				return this.APt[0].x;
			}
		}

		public float y
		{
			get
			{
				return this.APt[0].y;
			}
		}

		public float head_ratio
		{
			get
			{
				return this.head_ratio_;
			}
			set
			{
				this.head_ratio_ = X.MMX(-1f, value, 1f);
			}
		}

		public Texture Tx;

		public Rect TxRec;

		public float iwidth = 60f;

		public float iheight = 60f;

		public float scale = 1f;

		public float wave_x = 5f;

		public float max_curve_agR = 0.34906587f;

		public float max_curve_agR_head = 0.17453294f;

		private float head_ratio_;

		private int split_cnt = 4;

		private WormDrawer.WormPoint[] APt;

		private sealed class WormPoint
		{
			public WormPoint(float _base_len, float _ratio)
			{
				this.base_len = _base_len;
				this.ratio = _ratio;
				this.move_ratio = ((this.ratio == 0f) ? 1f : (0.3f - this.ratio * 0.2f));
			}

			public WormDrawer.WormPoint Set(float _x, float _y)
			{
				this.x = _x;
				this.y = _y;
				return this;
			}

			public WormDrawer.WormPoint SetDepert(float _x, float _y)
			{
				this.dx = _x;
				this.dy = _y;
				return this;
			}

			public float x;

			public float y;

			public float dx;

			public float dy;

			public float agR;

			public float base_len;

			public float ratio;

			public float move_ratio;
		}
	}
}
