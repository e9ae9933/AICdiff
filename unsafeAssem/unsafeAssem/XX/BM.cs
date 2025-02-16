using System;
using UnityEngine;

namespace XX
{
	public sealed class BM
	{
		public int width
		{
			get
			{
				return this.bmw;
			}
		}

		public int height
		{
			get
			{
				return this.bmh;
			}
		}

		public BM()
		{
			this.cola = new C32();
			this.colb = new C32();
		}

		public unsafe BM setPointer(Color32* ptr, int _bmw, int _bmh)
		{
			this.col_ptr0 = ptr;
			this.bmw = _bmw;
			this.bmh = _bmh;
			this.blend = BLEND.NORMAL;
			return this;
		}

		public unsafe BM clear()
		{
			int num = this.bmw * this.bmh;
			Color32* ptr = this.col_ptr0;
			this.cola.Set(0f, 0f, 0f, 0f);
			this.blend = BLEND.NORMAL;
			for (int i = 0; i < num; i++)
			{
				*ptr = this.cola.C;
				ptr++;
			}
			return this;
		}

		public void Dispose()
		{
			this.col_ptr0 = null;
		}

		public BM ADDBLEND()
		{
			this.blend = BLEND.ADD;
			return this;
		}

		public BM NOB()
		{
			this.blend = BLEND.NORMAL;
			return this;
		}

		public unsafe BM SetPixel32(int x, int y, Color32 col)
		{
			if (X.BTW(0f, (float)x, (float)this.bmw) && X.BTW(0f, (float)y, (float)this.bmh))
			{
				this.col_ptr0[y * this.bmw + x] = col;
			}
			return this;
		}

		public unsafe Color32 GetPixel32(int x, int y)
		{
			if (X.BTW(0f, (float)x, (float)this.bmw) && X.BTW(0f, (float)y, (float)this.bmh))
			{
				return this.col_ptr0[y * this.bmw + x];
			}
			return new Color32
			{
				r = 0,
				g = 0,
				b = 0,
				a = 0
			};
		}

		public BM DrawPixelRGBA(int _x, int _y, Color32 col)
		{
			return this.DrawPixelRGBA(_x, _y, col, (int)col.r, (int)col.g, (int)col.b, (int)col.a, false);
		}

		public unsafe BM DrawPixelRGBA(int _x, int _y, Color32 col, int _r, int _g, int _b, int _a, bool no_alp_mix = false)
		{
			if (_a >= 245 || (no_alp_mix && this.blend == BLEND.NORMAL))
			{
				return this.SetPixel32(_x, _y, col);
			}
			if (_a >= 10 && X.BTW(0f, (float)_x, (float)this.bmw) && X.BTW(0f, (float)_y, (float)this.bmh))
			{
				Color32* ptr = this.col_ptr0 + (_y * this.bmw + _x);
				Color32 color = *ptr;
				float num = (float)color.a;
				if (num == 0f)
				{
					*ptr = col;
				}
				else
				{
					if (this.blend == BLEND.ADD)
					{
						float num2 = num / 255f;
						col.r = (byte)X.MMX(0f, (float)color.r * num2 + (float)_r, 255f);
						col.g = (byte)X.MMX(0f, (float)color.g * num2 + (float)_g, 255f);
						col.b = (byte)X.MMX(0f, (float)color.b * num2 + (float)_b, 255f);
					}
					else
					{
						float num3 = (float)(255 - _a) * num / X.Mx(num, (float)_a);
						col.r = (byte)X.MMX(0f, ((float)color.r * num3 + (float)_r * (255f - num3)) / 255f, 255f);
						col.g = (byte)X.MMX(0f, ((float)color.g * num3 + (float)_g * (255f - num3)) / 255f, 255f);
						col.b = (byte)X.MMX(0f, ((float)color.b * num3 + (float)_b * (255f - num3)) / 255f, 255f);
					}
					col.a = (byte)X.MMX(0f, (65025f - (float)(255 - _a) * (255f - num)) / 255f, 255f);
					*ptr = col;
				}
			}
			return this;
		}

		public unsafe static void blendPixel(Color32* cptr, Color32 col, bool no_alp_mix = false)
		{
			int a = (int)col.a;
			if (a >= 245 || no_alp_mix)
			{
				*cptr = col;
				return;
			}
			if (a >= 10)
			{
				Color32 color = *cptr;
				float num = (float)color.a;
				if (num == 0f)
				{
					*cptr = col;
					return;
				}
				float num2 = (float)(255 - a) * num / 255f;
				col.r = (byte)X.MMX(0f, ((float)color.r * num2 + (float)col.r * (255f - num2)) / 255f, 255f);
				col.g = (byte)X.MMX(0f, ((float)color.g * num2 + (float)col.g * (255f - num2)) / 255f, 255f);
				col.b = (byte)X.MMX(0f, ((float)color.b * num2 + (float)col.b * (255f - num2)) / 255f, 255f);
				col.a = (byte)X.MMX(0f, (65025f - (float)(255 - a) * (255f - num)) / 255f, 255f);
				*cptr = col;
			}
		}

		public BM drawStarPixelsBottom(int bx, int by, Color32 col, int[] Aargs)
		{
			int num = Aargs.Length;
			int r = (int)col.r;
			int g = (int)col.g;
			int b = (int)col.b;
			int a = (int)col.a;
			for (int i = 0; i < num; i += 2)
			{
				int num2 = Aargs[i];
				int num3 = Aargs[i + 1];
				if (num2 == 0 && num3 == 0)
				{
					this.DrawPixelRGBA(bx, by, col, r, g, b, a, false);
				}
				else if (num2 == 0)
				{
					this.DrawPixelRGBA(bx, by - num3, col, r, g, b, a, false);
				}
				else if (num3 == 0)
				{
					this.DrawPixelRGBA(bx - num2, by, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by, col, r, g, b, a, false);
				}
				else
				{
					this.DrawPixelRGBA(bx - num2, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by - num3, col, r, g, b, a, false);
				}
			}
			return this;
		}

		public BM drawStarPixels(int bx, int by, Color32 col, int[] Aargs)
		{
			int num = Aargs.Length;
			int r = (int)col.r;
			int g = (int)col.g;
			int b = (int)col.b;
			int a = (int)col.a;
			for (int i = 0; i < num; i += 2)
			{
				int num2 = Aargs[i];
				int num3 = Aargs[i + 1];
				if (num2 == 0 && num3 == 0)
				{
					this.DrawPixelRGBA(bx, by, col, r, g, b, a, false);
				}
				else if (num2 == 0)
				{
					this.DrawPixelRGBA(bx, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx, by + num3, col, r, g, b, a, false);
				}
				else if (num3 == 0)
				{
					this.DrawPixelRGBA(bx - num2, by, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by, col, r, g, b, a, false);
				}
				else
				{
					this.DrawPixelRGBA(bx - num2, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx - num2, by + num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by + num3, col, r, g, b, a, false);
				}
			}
			return this;
		}

		public BM drawStarPixelsEven(int bx, int by, Color32 col, bool no_alp_mix, int[] Aargs)
		{
			int num = Aargs.Length;
			int r = (int)col.r;
			int g = (int)col.g;
			int b = (int)col.b;
			int a = (int)col.a;
			for (int i = 0; i < num; i += 2)
			{
				int num2 = Aargs[i];
				int num3 = Aargs[i + 1];
				this.DrawPixelRGBA(bx - num2, by - num3, col, r, g, b, a, no_alp_mix);
				this.DrawPixelRGBA(bx - num2, by + 1 + num3, col, r, g, b, a, no_alp_mix);
				this.DrawPixelRGBA(bx + 1 + num2, by - num3, col, r, g, b, a, no_alp_mix);
				this.DrawPixelRGBA(bx + 1 + num2, by + 1 + num3, col, r, g, b, a, no_alp_mix);
			}
			return this;
		}

		public BM drawVerticalStarPixelsEven(int bx, int by, Color32 col, bool no_alp_mix, int[] Aargs)
		{
			int num = Aargs.Length;
			int r = (int)col.r;
			int g = (int)col.g;
			int b = (int)col.b;
			int a = (int)col.a;
			for (int i = 0; i < num; i += 2)
			{
				int num2 = Aargs[i];
				int num3 = Aargs[i + 1];
				this.DrawPixelRGBA(bx + num2, by - num3, col, r, g, b, a, no_alp_mix);
				this.DrawPixelRGBA(bx + num2, by + 1 + num3, col, r, g, b, a, no_alp_mix);
			}
			return this;
		}

		public BM drawHanabiPixels(int bx, int by, Color32 col, int[] Aargs)
		{
			int num = Aargs.Length;
			int r = (int)col.r;
			int g = (int)col.g;
			int b = (int)col.b;
			int a = (int)col.a;
			for (int i = 0; i < num; i += 2)
			{
				int num2 = Aargs[i];
				int num3 = Aargs[i + 1];
				if (num2 == 0 && num3 == 0)
				{
					this.DrawPixelRGBA(bx, by, col, r, g, b, a, false);
				}
				else if (num2 == 0)
				{
					this.DrawPixelRGBA(bx, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx, by + num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx - num3, by, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num3, by, col, r, g, b, a, false);
				}
				else if (num3 == 0)
				{
					this.DrawPixelRGBA(bx - num2, by, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx, by - num2, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx, by + num2, col, r, g, b, a, false);
				}
				else if (num2 == num3)
				{
					this.DrawPixelRGBA(bx - num2, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx - num2, by + num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by + num3, col, r, g, b, a, false);
				}
				else
				{
					this.DrawPixelRGBA(bx - num2, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx - num2, by + num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by - num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by + num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx - num3, by - num2, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx - num3, by + num2, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num3, by - num2, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num3, by + num2, col, r, g, b, a, false);
				}
			}
			return this;
		}

		public BM drawVerticalPixels(int bx, int by, Color32 col, int[] Aargs)
		{
			int num = Aargs.Length;
			int r = (int)col.r;
			int g = (int)col.g;
			int b = (int)col.b;
			int a = (int)col.a;
			for (int i = 0; i < num; i += 2)
			{
				int num2 = Aargs[i];
				int num3 = Aargs[i + 1];
				if (num3 == 0)
				{
					this.DrawPixelRGBA(bx + num2, by, col, r, g, b, a, false);
				}
				else
				{
					this.DrawPixelRGBA(bx + num2, by + num3, col, r, g, b, a, false);
					this.DrawPixelRGBA(bx + num2, by - num3, col, r, g, b, a, false);
				}
			}
			return this;
		}

		public BM fillRectF(float x, float y, float w, float h, Color32 color)
		{
			return this.fillRect((int)x, (int)y, (int)w, (int)h, color);
		}

		public unsafe BM fillRect(int x, int y, int w, int h, Color32 color)
		{
			if (x >= this.bmw || y >= this.bmh || w <= 0 || h <= 0)
			{
				return this;
			}
			int num = x + w;
			int num2 = y + h;
			if (num <= 0 || h <= 0)
			{
				return this;
			}
			if (x < 0)
			{
				x = 0;
			}
			if (y < 0)
			{
				y = 0;
			}
			if (num > this.bmw)
			{
				num = this.bmw;
			}
			if (num2 > this.bmh)
			{
				num2 = this.bmh;
			}
			int num3 = this.bmw - num + x;
			Color32* ptr = this.col_ptr0 + y * this.bmw + x;
			for (int i = y; i < num2; i++)
			{
				for (int j = x; j < num; j++)
				{
					*ptr = color;
					ptr++;
				}
				ptr += num3;
			}
			return this;
		}

		public unsafe BM colorShiftRect(float mr, float mg, float mb, float ma, float pr = 0f, float pg = 0f, float pb = 0f, float pa = 0f, int x = 0, int y = 0, int w = 0, int h = 0)
		{
			if (x >= this.bmw || y >= this.bmh || w <= 0 || h <= 0)
			{
				return this;
			}
			int num = x + w;
			int num2 = y + h;
			if (num <= 0 || h <= 0)
			{
				return this;
			}
			if (x < 0)
			{
				x = 0;
			}
			if (y < 0)
			{
				y = 0;
			}
			if (num > this.bmw)
			{
				num = this.bmw;
			}
			if (num2 > this.bmh)
			{
				num2 = this.bmh;
			}
			int num3 = this.bmw - num + x;
			Color32* ptr = this.col_ptr0 + y * this.bmw + x;
			for (int i = y; i < num2; i++)
			{
				for (int j = x; j < num; j++)
				{
					*ptr = this.cola.Set(*ptr).colorShift(mr, mg, mb, ma, pr, pg, pb, pa).C;
					ptr++;
				}
				ptr += num3;
			}
			return this;
		}

		public unsafe BM colorShift(float mr, float mg, float mb, float ma, float pr = 0f, float pg = 0f, float pb = 0f, float pa = 0f)
		{
			Color32* ptr = this.col_ptr0;
			int num = this.bmw * this.bmh;
			for (int i = 0; i < num; i++)
			{
				*ptr = this.cola.Set(*ptr).colorShift(mr, mg, mb, ma, pr, pg, pb, pa).C;
				ptr++;
			}
			return this;
		}

		public BM DrawLine1(int x1, int y1, int x2, int y2, Color32 color, bool no_alp_mix = false)
		{
			return this.DrawLineN(x1, y1, x2, y2, color, 1, no_alp_mix);
		}

		public BM DrawLineN(int x1, int y1, int x2, int y2, Color32 color, int thick = 1, bool no_alp_mix = false)
		{
			if (x1 == x2)
			{
				if (y2 < y1)
				{
					y1 ^= y2;
					y2 ^= y1;
					y1 ^= y2;
				}
				this.fillRect(x1, y1, 1, y2 - y1 + 1, color);
				return this;
			}
			if (y1 == y2)
			{
				if (x2 < x1)
				{
					x1 ^= x2;
					x2 ^= x1;
					x1 ^= x2;
				}
				this.fillRect(x1, y1, x2 - x1 + 1, 1, color);
				return this;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int a = (int)color.a;
			if (a < 10)
			{
				return this;
			}
			bool flag = X.Abs(y2 - y1) > X.Abs(x2 - x1);
			int num4 = 0;
			uint num5 = (uint)((thick <= 1) ? 1 : thick);
			if (thick > 1)
			{
				num4 = (flag ? ((x2 > x1) ? (-1) : 1) : ((y2 > y1) ? (-1) : 1));
			}
			if ((x2 < x1 && !flag) || (y2 < y1 && flag))
			{
				y1 ^= y2;
				y2 ^= y1;
				y1 ^= y2;
				x1 ^= x2;
				x2 ^= x1;
				x1 ^= x2;
			}
			float num6 = (float)(y2 - y1) / (float)(x2 - x1);
			float num7 = -num6 * (float)x1 + (float)y1;
			bool flag2 = a < 245 && !no_alp_mix;
			if (flag2)
			{
				num = (int)color.r;
				num2 = (int)color.g;
				num3 = (int)color.b;
			}
			int num8 = ((!flag) ? x1 : y1);
			int num9 = ((!flag) ? x2 : y2);
			for (int i = num8; i <= num9; i++)
			{
				int num10 = (int)((!flag) ? ((float)i) : (((float)i - num7) / num6));
				int num11 = (int)((!flag) ? (num6 * (float)num10 + num7) : ((float)i));
				int num12 = 0;
				int num13 = 0;
				int num14 = 0;
				while ((long)num14 < (long)((ulong)num5))
				{
					if (num14 > 0)
					{
						if (flag)
						{
							num12 = (((num14 & 2) > 0) ? num4 : (-num4)) * (num14 + 1 >> 1);
						}
						else
						{
							num13 = (((num14 & 2) > 0) ? num4 : (-num4)) * (num14 + 1 >> 1);
						}
					}
					if (!flag2)
					{
						return this.SetPixel32(num10, num11, color);
					}
					this.DrawPixelRGBA(num10 + num12, num11 + num13, color, num, num2, num3, a, no_alp_mix);
					num14++;
				}
			}
			return this;
		}

		public BM DrawCircle1(int x, int y, float r1, Color32 color, int even = -1)
		{
			int num = (int)r1;
			if (even < 0)
			{
				even = (((int)(r1 * 2f) % 2 == 1) ? 0 : 1);
			}
			int num2 = (int)((float)num / 1.4142135f);
			float num3 = (float)num;
			int num4 = 0;
			int num5 = num * num;
			int num6 = ((even > 0) ? (-1) : 0);
			int num7 = 1;
			int num8 = 0;
			if (even == 0 && num == 1)
			{
				this.DrawPixelRGBA(x - 1, y, color);
				this.DrawPixelRGBA(x + 1, y, color);
				this.DrawPixelRGBA(x, y - 1, color);
				this.DrawPixelRGBA(x, y + 1, color);
			}
			else
			{
				for (;;)
				{
					num3 -= 1f;
					bool flag = num4 >= num2;
					int num9 = (int)Mathf.Sqrt((float)num5 - num3 * num3);
					if (num8 == 0 && ((num >= 5 && num != 8) || (num == 3 && even == 0)))
					{
						num9--;
					}
					int num10 = ((num4 == 0 && even == 0) ? 1 : 0);
					if (num8 == 1 && (num == 10 || num == 17))
					{
						num9--;
					}
					if (num8 == 2 && num == 18)
					{
						num9++;
					}
					if (num8 == 1 && num == 5)
					{
						num9--;
					}
					if (num8 == 3 && num == 17)
					{
						num9++;
					}
					if ((num8 == 3 || num8 == 4) && num == 22)
					{
						num9++;
					}
					int num11 = num9 - num4;
					if (num11 > 0)
					{
						num3 += 1f;
						this.fillRectF((float)(x - num4 - num11), (float)y - num3, (float)(num11 + num10), 1f, color);
						this.fillRectF((float)x + num3 + (float)num6, (float)(y - num4 - num11), 1f, (float)(num11 + num10), color);
						this.fillRectF((float)x - num3, (float)(y + num4 + num6 + num7 - num10), 1f, (float)(num11 + num10), color);
						this.fillRectF((float)(x + num4 + num6 + num7 - num10), (float)y + num3 + (float)num6, (float)(num11 + num10), 1f, color);
						if (!flag)
						{
							this.fillRectF((float)x + num3 + (float)num6, (float)(y + num4 + num6 + num7), 1f, (float)num11, color);
							this.fillRectF((float)x - num3, (float)(y - num4 - num11), 1f, (float)num11, color);
							this.fillRectF((float)(x - num4 - num11), (float)y + num3 + (float)num6, (float)num11, 1f, color);
							this.fillRectF((float)(x + num4 + num6 + num7), (float)y - num3, (float)num11, 1f, color);
						}
						num3 -= 1f;
					}
					if (flag)
					{
						break;
					}
					num4 = num9;
					num8++;
				}
			}
			return this;
		}

		public BM drawGradation(GRD mode, int kaisu, int sx, int sy, int width, int height, float col_s_r, float col_s_g, float col_s_b, float col_s_al, float col_d_r, float col_d_g, float col_d_b, float col_d_al, GRDSHT useColorShift = GRDSHT.NO_COLORSHIFT)
		{
			if (width == 0 || height == 0)
			{
				return this;
			}
			if (mode == GRD.LR)
			{
				if (sx > -width)
				{
					this.drawGradation(GRD.LEFT2RIGHT, kaisu, sx, sy, width, height, col_s_r, col_s_g, col_s_b, col_s_al, col_d_r, col_d_g, col_d_b, col_d_al, useColorShift);
				}
				if (sx < this.bmw + width)
				{
					this.drawGradation(GRD.RIGHT2LEFT, kaisu, sx, sy, width, height, col_s_r, col_s_g, col_s_b, col_s_al, col_d_r, col_d_g, col_d_b, col_d_al, useColorShift);
				}
				return this;
			}
			if (mode == GRD.TB)
			{
				if (sy > -height)
				{
					this.drawGradation(GRD.TOP2BOTTOM, kaisu, sx, sy, width, height, col_s_r, col_s_g, col_s_b, col_s_al, col_d_r, col_d_g, col_d_b, col_d_al, useColorShift);
				}
				if (sy < this.bmh + height)
				{
					this.drawGradation(GRD.TOP2BOTTOM, kaisu, sx, sy - height, width, height, col_d_r, col_d_g, col_d_b, col_d_al, col_s_r, col_s_g, col_s_b, col_s_al, useColorShift);
				}
				return this;
			}
			if (mode == GRD.RIGHT2LEFT)
			{
				return this.drawGradation(GRD.LEFT2RIGHT, kaisu, sx - width, sy, width, height, col_d_r, col_d_g, col_d_b, col_d_al, col_s_r, col_s_g, col_s_b, col_s_al, useColorShift);
			}
			if (mode == GRD.BOTTOM2TOP)
			{
				return this.drawGradation(GRD.TOP2BOTTOM, kaisu, sx, sy - height, width, height, col_d_r, col_d_g, col_d_b, col_d_al, col_s_r, col_s_g, col_s_b, col_s_al, useColorShift);
			}
			if (kaisu <= 1)
			{
				this.fillRect(sx, sy, width, height, this.cola.Set2(col_s_r, col_s_g, col_s_b, col_s_al).C);
				return this;
			}
			if (useColorShift == GRDSHT.COLORSHIFT_MUL255)
			{
				useColorShift = GRDSHT.COLORSHIFT_MUL;
				col_s_r /= 255f;
				col_d_r /= 255f;
				col_s_g /= 255f;
				col_d_g /= 255f;
				col_s_b /= 255f;
				col_d_b /= 255f;
				col_s_al /= 255f;
				col_d_al /= 255f;
			}
			if (mode == GRD.LEFT2RIGHT)
			{
				kaisu = X.Mn(width, kaisu);
			}
			if (mode == GRD.TOP2BOTTOM)
			{
				kaisu = X.Mn(height, kaisu);
			}
			float num = (float)(kaisu - 1);
			float num2 = (col_d_r - col_s_r) / num;
			float num3 = (col_d_g - col_s_g) / num;
			float num4 = (col_d_b - col_s_b) / num;
			float num5 = (col_d_al - col_s_al) / num;
			if (mode <= GRD.TOP2BOTTOM)
			{
				float num6 = (float)sx;
				float num7 = (float)sy;
				float num8;
				int num9;
				int num10;
				float num11;
				if (mode == GRD.LEFT2RIGHT)
				{
					num8 = (float)width / (float)kaisu;
					num9 = (int)num8;
					num10 = height;
					num11 = 0f;
				}
				else
				{
					num11 = (float)height / (float)kaisu;
					num9 = width;
					num10 = (int)num11;
					num8 = 0f;
				}
				for (int i = 0; i < kaisu; i++)
				{
					if (useColorShift == GRDSHT.NO_COLORSHIFT)
					{
						this.fillRect((int)num6, (int)num7, num9, num10, this.cola.Set(col_s_r, col_s_g, col_s_b, col_s_al).C);
					}
					else if (useColorShift == GRDSHT.COLORSHIFT_MUL)
					{
						this.colorShiftRect(col_s_r, col_s_g, col_s_b, col_s_al, 0f, 0f, 0f, 0f, (int)num6, (int)num7, num9, num10);
					}
					else if (useColorShift == GRDSHT.COLORSHIFT_ADD)
					{
						this.colorShiftRect(1f, 1f, 1f, 1f, col_s_r, col_s_g, col_s_b, col_s_al, (int)num6, (int)num7, num9, num10);
					}
					num6 += num8;
					num7 += num11;
					if (num2 != 0f)
					{
						col_s_r += num2;
					}
					if (num3 != 0f)
					{
						col_s_g += num3;
					}
					if (num4 != 0f)
					{
						col_s_b += num4;
					}
					if (num5 != 0f)
					{
						col_s_al += num5;
					}
				}
			}
			return this;
		}

		public BM drawGradationC(GRD mode, int kaisu, int sx, int sy, int width, int height, Color32 cols, Color32 cold, GRDSHT useColorShift = GRDSHT.NO_COLORSHIFT)
		{
			if (useColorShift == GRDSHT.COLORSHIFT_MUL)
			{
				useColorShift = GRDSHT.COLORSHIFT_MUL255;
			}
			return this.drawGradation(mode, kaisu, sx, sy, width, height, (float)cols.r, (float)cols.g, (float)cols.b, (float)cols.a, (float)cold.r, (float)cold.g, (float)cold.b, (float)cold.a, useColorShift);
		}

		public BM colorShiftRect255(float mr, float mg, float mb, float ma, float pr = 0f, float pg = 0f, float pb = 0f, float pa = 0f, int rx = 0, int ry = 0, int rw = 0, int rh = 0)
		{
			return this.colorShiftRect(mr / 255f, mg / 255f, mb / 255f, ma / 255f, pr, pg, pb, pa, rx, ry, rw, rh);
		}

		public BM colorShiftRectC(Color32 cm, Color32 cp, int rx = 0, int ry = 0, int rw = 0, int rh = 0)
		{
			return this.colorShiftRect255((float)cm.r, (float)cm.g, (float)cm.b, (float)cm.a, (float)cp.r, (float)cp.g, (float)cp.b, (float)cp.a, rx, ry, rw, rh);
		}

		public BM colorShift255(float mr, float mg, float mb, float ma, float pr = 0f, float pg = 0f, float pb = 0f, float pa = 0f)
		{
			return this.colorShift(mr / 255f, mg / 255f, mb / 255f, ma / 255f, pr, pg, pb, pa);
		}

		public unsafe Color32* col_ptr0;

		public C32 cola;

		public C32 colb;

		private BLEND blend;

		private int bmw;

		private int bmh;
	}
}
