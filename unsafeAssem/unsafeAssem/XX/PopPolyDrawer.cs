using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class PopPolyDrawer
	{
		public PopPolyDrawer(int pos_capacity = 4)
		{
			this.randomize_seed = X.xors(65536);
			this.APos = new Vector4[pos_capacity];
			this.Aflags = new byte[pos_capacity];
			this.FnZX = (this.FnZY = new FnZoom(X.ZSIN));
		}

		public PopPolyDrawer(string load_str, int pos_capacity = 4)
			: this(pos_capacity)
		{
			StringHolder stringHolder = new StringHolder(load_str, CsvReader.RegComma);
			this.loadFromSH(stringHolder, 0);
		}

		public PopPolyDrawer AddPos(float x, float y)
		{
			return this.SetPos(this.pos_max, x, y, true);
		}

		public PopPolyDrawer AddPos(Vector2 P)
		{
			return this.SetPos(this.pos_max, P, true);
		}

		private bool EnsureCapacity(int index)
		{
			if (index >= this.APos.Length)
			{
				Array.Resize<Vector4>(ref this.APos, index + 1);
			}
			if (this.Aflags.Length < this.APos.Length)
			{
				Array.Resize<byte>(ref this.Aflags, this.APos.Length);
			}
			bool flag = false;
			if (this.pos_max < index + 1)
			{
				this.pos_max = index + 1;
				flag = true;
			}
			this.APos[(index + 1) % this.pos_max].z = -1000f;
			this.APos[(index + this.pos_max - 1) % this.pos_max].z = -1000f;
			this.fine_angle = (this.fine_exangle = true);
			return flag;
		}

		public PopPolyDrawer AllFineAngle()
		{
			for (int i = 0; i < this.pos_max; i++)
			{
				this.APos[i].z = -1000f;
			}
			this.fine_angle = (this.fine_exangle = true);
			return this;
		}

		public PopPolyDrawer SetPos(int index, float x, float y, bool fine_triangulate = true)
		{
			if (this.EnsureCapacity(index))
			{
				fine_triangulate = true;
			}
			this.APos[index] = new Vector4(x, y, -1000f, 0f);
			if (fine_triangulate)
			{
				this.fine_triangulate = true;
			}
			return this;
		}

		public PopPolyDrawer SetPos(int index, Vector2 P, bool fine_triangulate = true)
		{
			if (this.EnsureCapacity(index))
			{
				fine_triangulate = true;
			}
			this.APos[index] = new Vector4(P.x, P.y, -1000f, 0f);
			if (fine_triangulate)
			{
				this.fine_triangulate = true;
			}
			return this;
		}

		public PopPolyDrawer PushPos(int index)
		{
			Vector2 vector = this.APos[index];
			Vector2 vector2 = this.APos[(index + this.pos_max - 1) % this.pos_max];
			return this.PushPos(index, X.NI(vector.x, vector2.x, 0.5f), X.NI(vector.y, vector2.y, 0.5f));
		}

		public PopPolyDrawer PushPos(int index, float x, float y)
		{
			this.EnsureCapacity(this.pos_max);
			X.unshiftEmpty<Vector4>(this.APos, new Vector4(x, y, -1000f, 0f), index, 1, this.pos_max - 1);
			X.unshiftEmpty<byte>(this.Aflags, 0, index, 1, this.pos_max - 1);
			this.fine_triangulate = true;
			return this.AllFineAngle();
		}

		public PopPolyDrawer SetZoomPos(float x, float y)
		{
			this.ZoomPos.x = x;
			this.ZoomPos.y = y;
			return this;
		}

		public Vector2 getPos(int index)
		{
			return this.APos[index];
		}

		public Vector2 getZoomPos()
		{
			return this.ZoomPos;
		}

		public int getFlags(int index)
		{
			return (int)this.Aflags[index];
		}

		public void setFlags(int index, int b)
		{
			if ((int)this.Aflags[index] != b)
			{
				this.Aflags[index] = (byte)b;
				this.fine_exangle = (this.redraw_flag = true);
			}
		}

		public int Length
		{
			get
			{
				return this.pos_max;
			}
		}

		private void fineTriangle()
		{
			if (this.fine_triangulate)
			{
				if (MTRX.Tri == null)
				{
					MTRX.Tri = new Triangulator();
				}
				if (this.Atri != null)
				{
					this.Atri.Clear();
				}
				this.Atri = MTRX.Tri.Triangulate(this.APos, 0, this.pos_max, this.Atri ?? new List<int>(24), 0);
			}
			if (this.fine_angle)
			{
				this.fine_angle = false;
				float num = -1000f;
				for (int i = 0; i < this.pos_max; i++)
				{
					Vector4 vector = this.APos[i];
					if (vector.z == -1000f)
					{
						Vector4 vector2 = this.APos[(i + 1) % this.pos_max];
						float num2 = X.GAR2(vector.x, vector.y, vector2.x, vector2.y);
						if (num == -1000f)
						{
							Vector4 vector3 = this.APos[(i + this.pos_max - 1) % this.pos_max];
							num = X.GAR2(vector3.x, vector3.y, vector.x, vector.y);
						}
						float num3 = X.Cos(num) + X.Cos(num2 + 3.1415927f);
						float num4 = X.Sin(num) + X.Sin(num2 + 3.1415927f);
						this.APos[i].z = ((num3 == 0f && num4 == 0f) ? (num + 1.5707964f) : X.GAR2(0f, 0f, num3, num4));
						num = num2;
					}
					else
					{
						num = -1000f;
					}
				}
			}
			if (this.fine_exangle)
			{
				this.fine_exangle = false;
				int num5 = -1;
				float num6 = 0f;
				for (int j = 0; j < this.pos_max; j++)
				{
					Vector4 vector4 = this.APos[j];
					int num7 = (int)this.Aflags[j];
					vector4.w = -1000f;
					if (num5 >= 0)
					{
						if ((num7 & (int)this.FLAG_UPPER_EXT_END) != 0)
						{
							Vector4 vector5 = this.APos[(j + 1) % this.pos_max];
							float num8 = (vector4.w = X.GAR2(vector5.x, vector5.y, vector4.x, vector4.y));
							float num9 = (num6 - num8) / (float)(j - num5);
							num8 += num9;
							for (int k = j - 1; k >= num5; k--)
							{
								this.APos[k].w = num8;
								num8 += num9;
							}
							num5 = -1;
						}
						else
						{
							vector4.w = num6;
						}
					}
					if (num5 < 0 && (num7 & (int)this.FLAG_UPPER_EXT_START) != 0)
					{
						num5 = j;
						Vector4 vector6 = this.APos[(j + this.pos_max - 1) % this.pos_max];
						num6 = (vector4.w = X.GAR2(vector6.x, vector6.y, vector4.x, vector4.y));
					}
					this.APos[j] = vector4;
				}
			}
		}

		private BList<Vector4> getPosBuffer(BList<Vector4> A, float tzx, float tzy)
		{
			int num = -1;
			if (this.randomize_seed >= 0 && this.randomize_pixel > 0f && this.frame_randomize > 0)
			{
				int num2 = IN.totalframe / this.frame_randomize;
				num = (this.randomize_seed + num2) % 65535;
				this.pre_drawn_anm_index = num2;
			}
			for (int i = 0; i < this.pos_max; i++)
			{
				Vector4 vector = this.APos[i];
				uint ran = X.GETRAN2(num * 3 + 4313, num * 7 + 13);
				float num3;
				if (tzx >= 1f)
				{
					num3 = vector.x;
				}
				else
				{
					num3 = (vector.x - this.ZoomPos.x) * tzx + this.ZoomPos.x;
				}
				float num4;
				if (tzy >= 1f)
				{
					num4 = vector.y;
				}
				else
				{
					num4 = (vector.y - this.ZoomPos.y) * tzy + this.ZoomPos.y;
				}
				if (num >= 0)
				{
					float num5 = X.RAN(ran, 2177 + i * 33) * 6.2831855f;
					num3 += this.randomize_pixel * tzx * X.Cos(num5);
					num4 += this.randomize_pixel * tzy * X.Sin(num5);
				}
				num3 *= 0.015625f;
				num4 *= 0.015625f;
				A.Add(new Vector4(num3, num4, vector.z, vector.w));
			}
			return A;
		}

		private void addTri(MeshDrawer Md)
		{
			Md.Tri(this.Atri);
		}

		public bool drawRecheck(float fcnt)
		{
			if (!this.redraw_flag && this.randomize_seed >= 0 && this.randomize_pixel > 0f && this.frame_randomize > 0)
			{
				int num = IN.totalframe / this.frame_randomize;
				this.redraw_flag = this.pre_drawn_anm_index != num;
			}
			if (this.t <= this.anim_maxt)
			{
				this.t += fcnt;
				this.redraw_flag = true;
			}
			return this.redraw_flag;
		}

		public void sinkTime(PopPolyDrawer Src)
		{
			this.t = X.Mn(Src.t, this.anim_maxt);
		}

		public void drawTo(MeshDrawer Md, float base_x, float base_y, float line_thick, float alpha = 1f)
		{
			this.fineTriangle();
			float tz = this.getTZ();
			float num = this.FnZX(tz);
			float num2 = this.FnZY(tz);
			float base_x2 = Md.base_x;
			float base_y2 = Md.base_y;
			Md.base_x += base_x * 0.015625f;
			Md.base_y += base_y * 0.015625f;
			Color32 col = Md.Col;
			this.addTri(Md);
			using (BList<Vector4> posBuffer = this.getPosBuffer(ListBuffer<Vector4>.Pop(this.pos_max), num, num2))
			{
				Md.Col = C32.MulA(Md.Col, alpha);
				for (int i = 0; i < this.pos_max; i++)
				{
					Vector4 vector = posBuffer[i];
					Md.Pos(vector.x, vector.y, null);
				}
				if (line_thick > 0f && this.ColLine.a > 0)
				{
					int num3 = this.pos_max * 2;
					Color32 col2 = Md.Col;
					Md.Col = C32.MulA(this.ColLine, alpha);
					for (int j = 0; j < num3; j += 2)
					{
						Md.TriRectBL(j, j + 1, (j + 3) % num3, (j + 2) % num3);
					}
					float num4 = line_thick * (1f - this.line_inner_shift) * 0.015625f;
					float num5 = -line_thick * this.line_inner_shift * 0.015625f;
					for (int k = 0; k < this.pos_max; k++)
					{
						bool flag = (this.Aflags[k] & this.FLAG_INNER) > 0;
						Vector4 vector2 = posBuffer[k];
						float num6 = vector2.z + (flag ? 3.1415927f : 0f);
						float num7 = X.Cos(num6);
						float num8 = X.Sin(num6);
						Md.Pos(vector2.x + num7 * num5, vector2.y + num8 * num5, null);
						Md.Pos(vector2.x + num7 * num4, vector2.y + num8 * num4, null);
					}
					Md.Col = col2;
				}
			}
			Md.Col = col;
			Md.base_x = base_x2;
			Md.base_y = base_y2;
			this.redraw_flag = false;
		}

		public float getTZ()
		{
			return X.ZLINE(this.t, this.anim_maxt);
		}

		public void drawExtendAreaTo(MeshDrawer Md, float base_x, float base_y)
		{
			this.fineTriangle();
			this.redraw_flag = false;
			float tz = this.getTZ();
			float num = this.FnZX(tz);
			float num2 = this.FnZY(tz);
			base_x *= 0.015625f;
			base_y *= 0.015625f;
			this.addTri(Md);
			using (BList<Vector4> posBuffer = this.getPosBuffer(ListBuffer<Vector4>.Pop(this.pos_max), num, num2))
			{
				for (int i = 0; i < this.pos_max; i++)
				{
					Vector4 vector = posBuffer[i];
					if (vector.w != -1000f)
					{
						vector.x += X.Cos(vector.w) * this.extend_pixel * 0.015625f * num;
						vector.y += X.Sin(vector.w) * this.extend_pixel * 0.015625f * num2;
					}
					Md.Pos(vector.x + base_x, vector.y + base_y, null);
				}
			}
		}

		public void loadFromSH(StringHolder CR, int si = 1)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				this.fine_angle = (this.fine_exangle = (this.redraw_flag = true));
				this.fine_triangulate = true;
				stb.Set(CR.getIndex(si));
				int num;
				this.pos_max = (int)STB.NmRes(stb.Nm(0, out num, -1, false), -1.0);
				this.EnsureCapacity(this.pos_max - 1);
				X.ALL0(this.Aflags);
				for (int i = 0; i < this.pos_max; i++)
				{
					if (num >= stb.Length)
					{
						this.APos[i] = new Vector4(0f, 0f, -1000f, -1000f);
					}
					else
					{
						float num2 = (float)STB.NmRes(stb.Nm(++num, out num, -1, false), -1.0);
						float num3 = (float)STB.NmRes(stb.Nm(++num, out num, -1, false), -1.0);
						this.APos[i] = new Vector4(num2, num3, -1000f, -1000f);
					}
				}
				if (si + 1 < CR.clength)
				{
					stb.Set(CR.getIndex(si + 1));
					num = -1;
					for (int j = 0; j < this.pos_max; j++)
					{
						if (num >= stb.Length)
						{
							this.Aflags[j] = 0;
						}
						else
						{
							int num4 = (int)STB.NmRes(stb.Nm(++num, out num, -1, false), -1.0);
							this.Aflags[j] = (byte)num4;
						}
					}
					if (si + 2 < CR.clength)
					{
						stb.Set(CR.getIndex(si + 2));
						num = -1;
						float num5 = (float)STB.NmRes(stb.Nm(++num, out num, -1, false), -1.0);
						float num6 = (float)STB.NmRes(stb.Nm(++num, out num, -1, false), -1.0);
						this.ZoomPos.Set(num5, num6);
						if (si + 3 < CR.clength)
						{
							stb.Set(CR.getIndex(si + 3));
							this.extend_pixel = (float)STB.NmRes(stb.Nm(0, out num, -1, false), -1.0);
							if (si + 4 < CR.clength)
							{
								stb.Set(CR.getIndex(si + 4));
								this.frame_randomize = (int)STB.NmRes(stb.Nm(0, out num, -1, false), -1.0);
								if (si + 5 < CR.clength)
								{
									stb.Set(CR.getIndex(si + 5));
									this.randomize_pixel = (float)STB.NmRes(stb.Nm(0, out num, -1, false), -1.0);
									if (si + 6 < CR.clength)
									{
										stb.Set(CR.getIndex(si + 6));
										this.anim_maxt = (float)STB.NmRes(stb.Nm(0, out num, -1, false), -1.0);
									}
								}
							}
						}
					}
				}
			}
		}

		public bool isSame(PopPolyDrawer S)
		{
			if (this.pos_max != S.pos_max)
			{
				return false;
			}
			if (this.extend_pixel != S.extend_pixel)
			{
				return false;
			}
			if (this.frame_randomize != S.frame_randomize)
			{
				return false;
			}
			if (this.randomize_pixel != S.randomize_pixel)
			{
				return false;
			}
			if (this.anim_maxt != S.anim_maxt)
			{
				return false;
			}
			if (C32.c2d(this.ColLine) != C32.c2d(S.ColLine))
			{
				return false;
			}
			for (int i = 0; i < this.pos_max; i++)
			{
				Vector2 vector = this.APos[i];
				Vector2 vector2 = S.APos[i];
				if (vector.x != vector2.x || vector.y != vector2.y)
				{
					return false;
				}
			}
			return true;
		}

		public void restartAnim(bool only_minimize = false)
		{
			if (only_minimize)
			{
				this.t = X.Mn(this.t, this.anim_maxt - 1f);
				return;
			}
			this.t = 0f;
		}

		public bool isContaining(float vx, float vy)
		{
			float num = this.ZoomPos.x;
			float num2 = this.ZoomPos.y;
			if (this.pos_max < 3)
			{
				return false;
			}
			if (vx == this.ZoomPos.x && vy == this.ZoomPos.y)
			{
				bool flag = false;
				for (int i = 0; i < this.pos_max; i++)
				{
					Vector2 vector = X.NI(this.getPos(i), this.getPos((i + 1) % this.pos_max), 0.5f);
					if (vector.x != this.ZoomPos.x || vector.y != this.ZoomPos.y)
					{
						flag = true;
						num = vector.x;
						num2 = vector.y;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			int num3 = 0;
			Vector2 vector2 = this.getPos(this.pos_max - 1);
			for (int j = 0; j < this.pos_max; j++)
			{
				Vector2 pos = this.getPos(j);
				Vector3 vector3 = X.crosspoint(num, num2, vx, vy, vector2.x, vector2.y, pos.x, pos.y);
				if (vector3.z != 0f)
				{
					bool flag2;
					if (num == vx)
					{
						flag2 = vy < num2 == vy < vector3.y;
					}
					else
					{
						flag2 = vx < num == vx < vector3.x;
					}
					if (flag2)
					{
						if (vector2.x == pos.x)
						{
							if (X.BTWRV(vector2.y, vector3.y, pos.y))
							{
								num3++;
							}
						}
						else if (X.BTWRV(vector2.x, vector3.x, pos.x))
						{
							num3++;
						}
					}
				}
				vector2 = pos;
			}
			return num3 % 2 == 1;
		}

		public FnZoom FnZX;

		public FnZoom FnZY;

		private Vector4[] APos;

		private byte[] Aflags;

		private byte FLAG_INNER = 1;

		private byte FLAG_UPPER_EXT_START = 2;

		private byte FLAG_UPPER_EXT_END = 4;

		public Vector2 ZoomPos;

		private List<int> Atri;

		private int pos_max;

		private bool fine_angle = true;

		private bool fine_exangle = true;

		public float line_inner_shift = 0.25f;

		public int frame_randomize;

		private int randomize_seed;

		public float randomize_pixel = 5f;

		public float extend_pixel = 34f;

		public bool redraw_flag;

		public int pre_drawn_anm_index;

		public float anim_maxt = 45f;

		public float t;

		public bool fine_triangulate;

		public Color32 ColLine = C32.d2c(4287269514U);
	}
}
