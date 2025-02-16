using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class MBoxDrawer
	{
		public MBoxDrawer Create(int _kaku, PxlImage ImgTop, PxlImage ImgFront, PxlImage ImgSide, PxlImage ImgCSide)
		{
			this.kaku = _kaku;
			int num = this.cover_ver_count + 20;
			int num2 = 10 + this.kaku * 4;
			this.AMeshUv = new Vector2[num];
			this.AVertice = new Vector3[num];
			this.AVertice_buf = new Vector3[this.cover_ver_count - 4];
			this.Acolor_lvl = new float[num];
			this.Atriangle = new int[num2 * 3];
			int num3 = 0;
			int num4 = 0;
			float num5 = 1f / (float)this.kaku;
			float num6 = num5 * 3.1415927f;
			float num7 = 0f;
			float num8 = 0f;
			int num9 = 0;
			this.Pos(ref num3, -1f, 0f, 0f, ImgCSide, 0.5f, 0f, 1f);
			this.Pos(ref num3, 1f, 0f, 0f, ImgCSide, 0.5f, 0f, 1f);
			for (;;)
			{
				float num10 = X.Sin(num7);
				float num11 = -X.Cos(num7);
				float num12 = X.Abs(0.5f - num8) * 2f;
				this.Pos(ref num3, -1f, num10, num11, ImgTop, 0f, num8, num12);
				this.Pos(ref num3, 1f, num10, num11, ImgTop, 1f, num8, num12);
				float num13 = 0.5f - 0.5f * num11;
				this.Pos(ref num3, -1f, num10, num11, ImgCSide, num13, num10, 1f - num10);
				this.Pos(ref num3, 1f, num10, num11, ImgCSide, num13, num10, 1f - num10);
				if (num9 > 0)
				{
					this.Tri(ref num4, num3, -8, -4, -7).Tri(ref num4, num3, -4, -3, -7);
					this.Tri2(ref num4, num3, 0, -2, -6).Tri2(ref num4, num3, 1, -5, -1);
				}
				if (num9 >= this.kaku)
				{
					break;
				}
				if (++num9 == this.kaku)
				{
					num7 = 3.1415927f;
					num8 = 1f;
				}
				else
				{
					num7 += num6;
					num8 += num5;
				}
			}
			Array.Copy(this.AVertice, this.AVertice_buf, this.AVertice_buf.Length);
			this.Tri(ref num4, num3, 0, 2, 1).Tri(ref num4, num3, 0, 3, 2);
			this.Pos(ref num3, -1f, -1f, -1f, ImgSide, 0f, 0f, 0f);
			this.Pos(ref num3, -1f, -1f, 1f, ImgSide, 0f, 1f, 1f);
			this.Pos(ref num3, 1f, -1f, 1f, ImgSide, 1f, 1f, 0f);
			this.Pos(ref num3, 1f, -1f, -1f, ImgSide, 1f, 0f, 0f);
			this.Pos4XY(ref num3, ref num4, 1f, -1f, 1f, -2f, 1f, ImgSide);
			this.Pos4ZY(ref num3, ref num4, -1f, -1f, 1f, -2f, 1f, ImgSide);
			this.Pos4ZY(ref num3, ref num4, 1f, -1f, -1f, 2f, 1f, ImgSide);
			this.Pos4XY(ref num3, ref num4, -1f, -1f, -1f, 2f, 1f, ImgFront);
			return this;
		}

		private int cover_ver_count
		{
			get
			{
				return (this.kaku + 1) * 2 * 2 + 2;
			}
		}

		private int cover_tri_count
		{
			get
			{
				return this.kaku * 4;
			}
		}

		private MBoxDrawer Pos(ref int vi, float x, float y, float z, PxlImage Img, float uvl, float uvt, float clvl)
		{
			Rect rectIUv = Img.RectIUv;
			this.AMeshUv[vi] = new Vector2(rectIUv.xMin + rectIUv.width * uvl, rectIUv.yMin + rectIUv.height * uvt);
			this.Acolor_lvl[vi] = clvl;
			Vector3[] avertice = this.AVertice;
			int num = vi;
			vi = num + 1;
			avertice[num] = new Vector3(x, y, z);
			return this;
		}

		private MBoxDrawer Pos4XY(ref int vi, ref int ti, float x, float y, float z, float w, float h, PxlImage Img)
		{
			this.Tri(ref ti, vi, 0, 1, 2).Tri(ref ti, vi, 0, 2, 3);
			this.Pos(ref vi, x, y, z, Img, 0f, 0f, 0f);
			this.Pos(ref vi, x, y + h, z, Img, 0f, 1f, 1f);
			this.Pos(ref vi, x + w, y + h, z, Img, 1f, 1f, 1f);
			this.Pos(ref vi, x + w, y, z, Img, 1f, 0f, 0f);
			return this;
		}

		private MBoxDrawer Pos4ZY(ref int vi, ref int ti, float x, float y, float z, float w, float h, PxlImage Img)
		{
			this.Tri(ref ti, vi, 0, 1, 2).Tri(ref ti, vi, 0, 2, 3);
			this.Pos(ref vi, x, y, z, Img, 0f, 0f, 0f);
			this.Pos(ref vi, x, y + h, z, Img, 0f, 1f, 1f);
			this.Pos(ref vi, x, y + h, z + w, Img, 1f, 1f, 1f);
			this.Pos(ref vi, x, y, z + w, Img, 1f, 0f, 0f);
			return this;
		}

		private MBoxDrawer Tri(ref int ti, int vi, int a, int b, int c)
		{
			int[] atriangle = this.Atriangle;
			int num = ti;
			ti = num + 1;
			atriangle[num] = vi + a;
			int[] atriangle2 = this.Atriangle;
			num = ti;
			ti = num + 1;
			atriangle2[num] = vi + b;
			int[] atriangle3 = this.Atriangle;
			num = ti;
			ti = num + 1;
			atriangle3[num] = vi + c;
			return this;
		}

		private MBoxDrawer Tri2(ref int ti, int vi, int abs_a, int b, int c)
		{
			int[] atriangle = this.Atriangle;
			int num = ti;
			ti = num + 1;
			atriangle[num] = abs_a;
			int[] atriangle2 = this.Atriangle;
			num = ti;
			ti = num + 1;
			atriangle2[num] = vi + b;
			int[] atriangle3 = this.Atriangle;
			num = ti;
			ti = num + 1;
			atriangle3[num] = vi + c;
			return this;
		}

		public unsafe void drawTo(MeshDrawer Md, float x, float y, float wh_px, Matrix4x4 Mx, float open_lvl = 0f, bool use_color = true, bool inner = false, int draw_tb = 3)
		{
			if (this.pre_open_level != open_lvl)
			{
				this.pre_open_level = open_lvl;
				Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(0f, 0f, 1f)) * Matrix4x4.Rotate(Quaternion.Euler(110f * open_lvl, 0f, 0f)) * Matrix4x4.Translate(new Vector3(0f, 0f, -1f));
				for (int i = this.AVertice_buf.Length - 1; i >= 0; i--)
				{
					this.AVertice[i] = matrix4x.MultiplyPoint3x4(this.AVertice_buf[i]);
				}
			}
			int num2;
			int num;
			int num3;
			int num4;
			if (draw_tb == 1)
			{
				num = (num2 = 0);
				num3 = this.cover_ver_count;
				num4 = this.cover_tri_count * 3;
			}
			else if (draw_tb == 2)
			{
				num2 = this.cover_ver_count;
				num = this.cover_tri_count * 3;
				num3 = this.AVertice.Length;
				num4 = this.Atriangle.Length;
			}
			else
			{
				num = (num2 = 0);
				num3 = this.AVertice.Length;
				num4 = this.Atriangle.Length;
			}
			Md.allocTri(Md.getTriMax() + num4 - num, 0);
			Md.allocVer(Md.getVertexMax() + num3 - num2, 0);
			if (!inner || open_lvl > 0f)
			{
				fixed (int* ptr = &this.Atriangle[num])
				{
					int* ptr2 = ptr;
					for (int j = num; j < num4; j += 3)
					{
						int num5 = *(ptr2++);
						int num6 = *(ptr2++);
						int num7 = *(ptr2++);
						if (inner)
						{
							Md.Tri(num5 - num2, num7 - num2, num6 - num2, false);
						}
						else
						{
							Md.Tri(num5 - num2, num6 - num2, num7 - num2, false);
						}
					}
				}
			}
			wh_px *= 0.015625f;
			Mx = Matrix4x4.Translate(new Vector3(x, y, 0f) * 0.015625f) * Mx * Matrix4x4.Scale(new Vector3(wh_px, wh_px, wh_px));
			fixed (Vector3* ptr3 = &this.AVertice[num2])
			{
				Vector3* ptr4 = ptr3;
				fixed (Vector2* ptr5 = &this.AMeshUv[num2])
				{
					Vector2* ptr6 = ptr5;
					fixed (float* ptr7 = &this.Acolor_lvl[num2])
					{
						float* ptr8 = ptr7;
						Vector3* ptr9 = ptr4;
						Vector2* ptr10 = ptr6;
						float* ptr11 = ptr8;
						C32 colb = MTRX.colb;
						Color32 col = Md.Col;
						for (int k = num2; k < num3; k++)
						{
							Vector3 vector = Mx.MultiplyPoint3x4(*(ptr9++));
							Vector2 vector2 = *(ptr10++);
							Md.uvRectN(vector2.x, vector2.y);
							if (use_color)
							{
								float num8 = *(ptr11++);
								Md.Col = colb.Set(col).blend(Md.ColGrd, inner ? (1f - num8) : num8).C;
							}
							Md.Pos(vector.x, vector.y, null);
						}
					}
				}
			}
		}

		private float[] Acolor_lvl;

		private Vector2[] AMeshUv;

		private Vector3[] AVertice;

		private Vector3[] AVertice_buf;

		private float pre_open_level;

		private int[] Atriangle;

		private int kaku;
	}
}
