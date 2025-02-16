using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class RollSeetDrawer
	{
		private static void Tri(int a, int b, int c)
		{
			int count = RollSeetDrawer.APosS.Count;
			RollSeetDrawer.AtriS.Add(count + a);
			RollSeetDrawer.AtriS.Add(count + b);
			RollSeetDrawer.AtriS.Add(count + c);
		}

		public RollSeetDrawer RemakeSpiral(float rlvl)
		{
			this.remaked_spiral_level = rlvl;
			new Vector3(0f, -1f, 0f);
			float num = 0f;
			float num2 = 0f;
			float num3 = 1f;
			float num4 = 0f;
			float num5 = (float)this.block_count_y_ * rlvl;
			float num6 = 1f / (float)(2 + this.block_count_y_);
			float num7 = -1.5707964f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 6.2831855f;
			RollSeetDrawer.AuzuS.Clear();
			RollSeetDrawer.APosS.Clear();
			RollSeetDrawer.AUvS.Clear();
			RollSeetDrawer.AtriS.Clear();
			int num11 = 0;
			float num12 = -this.max_agR * (float)this.block_count_x * 0.5f;
			for (;;)
			{
				float num13 = num3 * 2f * 3.1415927f;
				float num14 = num10 / num13 * this.max_agR;
				int num15;
				if (num4 + 1.25f <= num5)
				{
					num15 = 1;
				}
				else
				{
					num15 = X.IntR(num14 / this.resolution_agR);
				}
				float num16 = 1f / (float)num15;
				float num17 = -num14 * num16;
				float num18 = -num6 * num16;
				int num19 = 0;
				float num20 = 0f;
				for (;;)
				{
					float num21 = (1f - (1f - X.ZLINE(rlvl - 0.875f, 0.875f)) * X.ZLINE(X.Abs(num5 - num4), 1.5f)) * (1f - X.ZLINE(num8 - 4.1887903f, 3.1415927f));
					Vector4 vector = new Vector4(num + num3 * X.Cos(num7) + num9, num2 + num3 * X.Sin(num7), (float)num15, num7);
					RollSeetDrawer.AuzuS.Add(vector);
					int num22;
					if (num19 > 0 && this.block_count_x > 0)
					{
						num22 = -this.block_count_x * 2;
						int num23 = 0;
						do
						{
							RollSeetDrawer.Tri(num22, num23, num22 + 1);
							RollSeetDrawer.Tri(num23, num23 + 1, num22 + 1);
							num23 += 2;
							num22 += 2;
						}
						while (num22 < 0);
					}
					num22 = 0;
					if (this.block_count_x > 0)
					{
						float num24 = num12;
						do
						{
							RollSeetDrawer.APosS.Add(new Vector4(num24, -vector.x, -vector.y, num21));
							RollSeetDrawer.AUvS.Add(new Vector2(0f, num20));
							num24 += this.max_agR;
							RollSeetDrawer.APosS.Add(new Vector4(num24, -vector.x, -vector.y, num21));
							RollSeetDrawer.AUvS.Add(new Vector2(1f, num20));
						}
						while (++num22 < this.block_count_x);
					}
					if (++num19 > num15)
					{
						break;
					}
					if (num4 < num5)
					{
						num -= this.max_agR * num16;
						num2 += num18;
					}
					else
					{
						num7 += num17;
						if (this.double_roll)
						{
							float num25 = -num6 * 3f;
							if (num8 < 1.5707964f)
							{
								num9 = num25 * X.ZLINE(num8, 1.5707964f);
							}
							else
							{
								num9 = num25;
							}
						}
						num8 -= num17;
						if (!this.draw_inner_surface && num19 > 1 && num8 >= 7.853982f)
						{
							goto Block_11;
						}
					}
					num4 += num16;
					num3 += num18;
					num20 += num16;
				}
				IL_0315:
				if ((num4 = (float)(++num11)) >= (float)this.block_count_y_)
				{
					break;
				}
				continue;
				Block_11:
				num11 = this.block_count_y_;
				goto IL_0315;
			}
			this.ver_max = RollSeetDrawer.APosS.Count;
			if (this.ver_max > 0)
			{
				if (this.APos == null || this.APos.Length < this.ver_max)
				{
					this.APos = new Vector4[this.ver_max];
					this.AUv = new Vector2[this.ver_max];
				}
				RollSeetDrawer.APosS.CopyTo(this.APos);
				RollSeetDrawer.AUvS.CopyTo(this.AUv);
				this.tri_max = RollSeetDrawer.AtriS.Count;
				if (this.Atri == null || this.Atri.Length < this.tri_max)
				{
					this.Atri = new int[this.tri_max];
				}
				RollSeetDrawer.AtriS.CopyTo(this.Atri);
			}
			return this;
		}

		public RollSeetDrawer drawDebugSpiral(MeshDrawer Md, float rlvl, float scale_px)
		{
			if (this.remaked_spiral_level != rlvl)
			{
				this.RemakeSpiral(rlvl);
			}
			int count = RollSeetDrawer.AuzuS.Count;
			for (int i = 0; i < count; i++)
			{
				Vector4 vector = RollSeetDrawer.AuzuS[i];
				int num = (int)vector.z;
				Md.Col = C32.d2c(4294901760U);
				Md.Circle(scale_px * vector.x, scale_px * vector.y, 5.5f, 1f, false, 0f, 0f);
				Md.Col = MTRX.ColWhite;
				while (--num >= 0)
				{
					Vector4 vector2 = RollSeetDrawer.AuzuS[++i];
					Md.Line(scale_px * vector.x, scale_px * vector.y, scale_px * vector2.x, scale_px * vector2.y, 2f, false, 0f, 0f);
					vector = vector2;
					Md.Circle(scale_px * vector.x, scale_px * vector.y, 3.5f, 0f, false, 0f, 0f);
				}
			}
			return this;
		}

		public RollSeetDrawer drawLine(MeshDrawer Md, float x, float y, float sclx, float scly, float thick, float rlvl, float basepos_shift_x = 0f, float basepos_shift_y = 0f)
		{
			if (this.remaked_spiral_level != rlvl)
			{
				this.RemakeSpiral(rlvl);
			}
			int count = RollSeetDrawer.AuzuS.Count;
			int i = 0;
			thick *= 0.0078125f;
			float num = -1.5707964f;
			float num2 = 0f;
			float num3 = 1f;
			float num4 = 0f;
			float num5 = 0f;
			if (Md.uv_settype == UV_SETTYPE.IMG)
			{
				int num6 = 0;
				while (i < count)
				{
					int num7 = (int)RollSeetDrawer.AuzuS[i].z;
					num6 += X.Mn(count - i, num7);
					i += num7 + 1;
				}
				i = 0;
				num2 = 1f / (float)num6 * Md.uv_width;
				num3 = Md.uv_left + Md.uv_width;
				num4 = Md.uv_top;
				num5 = Md.uv_top + Md.uv_height;
			}
			while (i < count)
			{
				Vector4 vector = RollSeetDrawer.AuzuS[i];
				int num8 = (int)vector.z;
				int num9 = 0;
				while (num9++ < num8 && i < count)
				{
					if (i > 0)
					{
						Md.Tri(0, 1, -1, false).Tri(0, -1, -2, false);
					}
					vector = RollSeetDrawer.AuzuS[i++];
					float num10 = ((i >= count) ? num : vector.w);
					float num11 = X.Cos(num10);
					float num12 = X.Sin(num10);
					float num13 = (x + sclx * (vector.x + basepos_shift_x)) * 0.015625f;
					float num14 = (y + scly * (vector.y + basepos_shift_y)) * 0.015625f;
					Md.Pos(num13 + thick * num11, num14 + thick * num12, null);
					Md.Pos(num13 - thick * num11, num14 - thick * num12, null);
					if (Md.uv_settype == UV_SETTYPE.IMG)
					{
						Vector2[] uvArray = Md.getUvArray();
						uvArray[Md.getVertexMax() - 2] = new Vector2(num3, num4);
						uvArray[Md.getVertexMax() - 1] = new Vector2(num3, num5);
						num3 -= num2;
					}
					num = num10;
				}
				i++;
			}
			return this;
		}

		public unsafe RollSeetDrawer drawTo(MeshDrawer Md, float rlvl, float wh_px, Matrix4x4 Mx, bool inner)
		{
			if (this.remaked_spiral_level != rlvl)
			{
				this.RemakeSpiral(rlvl);
			}
			if (this.APos == null)
			{
				return this;
			}
			int num;
			int num2;
			if (this.double_roll)
			{
				num = 0;
				num2 = 2;
			}
			else
			{
				num = -1;
				num2 = 0;
			}
			Md.allocTri(Md.getTriMax() + this.tri_max * num, 0);
			Md.allocVer(Md.getVertexMax() + this.ver_max * num, 0);
			wh_px *= 0.015625f;
			Rect rectIUv = (inner ? this.ImgBack : this.ImgFront).RectIUv;
			Mx *= Matrix4x4.Scale(new Vector3(wh_px, wh_px, wh_px));
			for (int i = num; i < num2; i++)
			{
				fixed (int* ptr = &this.Atri[0])
				{
					int* ptr2 = ptr;
					for (int j = 0; j < this.tri_max; j += 3)
					{
						int num3 = *(ptr2++);
						int num4 = *(ptr2++);
						int num5 = *(ptr2++);
						if (inner != (i == 1))
						{
							Md.Tri(num3, num5, num4, false);
						}
						else
						{
							Md.Tri(num3, num4, num5, false);
						}
					}
				}
				fixed (Vector4* ptr3 = &this.APos[0])
				{
					Vector4* ptr4 = ptr3;
					fixed (Vector2* ptr5 = &this.AUv[0])
					{
						Vector2* ptr6 = ptr5;
						Vector4* ptr7 = ptr4;
						Vector2* ptr8 = ptr6;
						for (int k = 0; k < this.ver_max; k++)
						{
							Vector2 vector = *(ptr8++);
							Vector4 vector2 = *(ptr7++);
							if (i == 1)
							{
								vector2.y = -vector2.y;
								vector.y = 1f - vector.y;
							}
							Vector3 vector3 = Mx.MultiplyPoint3x4(vector2);
							Md.Col = Md.ColGrd.Set(this.bright_col).blend(this.dark_col, vector2.w).C;
							Md.uvRectN(rectIUv.xMin + rectIUv.width * vector.x, rectIUv.yMin + rectIUv.height * vector.y);
							Md.Pos(vector3.x, vector3.y, null);
						}
					}
				}
			}
			return this;
		}

		public int block_count_y
		{
			get
			{
				return this.block_count_y_;
			}
			set
			{
				this.block_count_y_ = value;
				this.remaked_spiral_level = -1f;
			}
		}

		public PxlImage ImgFront;

		public PxlImage ImgBack;

		public int block_count_x = 5;

		private int block_count_y_ = 4;

		public float max_agR = 1.8849558f;

		public float resolution_agR = 0.3926991f;

		public bool draw_inner_surface;

		public static List<Vector4> AuzuS = new List<Vector4>(120);

		public static List<Vector4> APosS = new List<Vector4>(120);

		public static List<Vector2> AUvS = new List<Vector2>(120);

		public static List<int> AtriS = new List<int>(120);

		public Vector4[] APos;

		public Vector2[] AUv;

		public int[] Atri;

		public int ver_max;

		public int tri_max;

		private float remaked_spiral_level = -1f;

		public uint bright_col = uint.MaxValue;

		public uint dark_col = 4289374890U;

		public bool double_roll;
	}
}
