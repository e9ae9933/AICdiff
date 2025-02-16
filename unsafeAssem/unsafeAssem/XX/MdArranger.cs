using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class MdArranger
	{
		public MdArranger(MeshDrawer _Md = null)
		{
			this.Md = _Md;
		}

		public void setMdTarget(MeshDrawer _Md)
		{
			this.Md = _Md;
		}

		public MdArranger destroy()
		{
			this.clear(null);
			return null;
		}

		public MdArranger updateForMeshRenderer(bool temp = false)
		{
			this.Md.updateForMeshRenderer(temp);
			return this;
		}

		public MdArranger Set(bool first = false)
		{
			if (first || this.index_last == -1)
			{
				this.index_first = (this.index_last = this.Md.getVertexMax());
				this.tri_first = (this.tri_last = this.Md.getTriMax());
			}
			else
			{
				this.index_last = this.Md.getVertexMax();
				this.tri_last = this.Md.getTriMax();
			}
			return this;
		}

		public MdArranger Set(int vert, int tri)
		{
			this.index_first = vert;
			this.tri_first = tri;
			return this.Set(false);
		}

		public MdArranger Set(int vert, int tri, int _vert_last, int _tri_last)
		{
			this.index_first = vert;
			this.tri_first = tri;
			this.index_last = _vert_last;
			this.tri_last = _tri_last;
			return this;
		}

		public MdArranger SwapLastIndex()
		{
			this.index_first = this.index_last;
			this.tri_first = this.tri_last;
			return this;
		}

		public MdArranger SetLastVerAndTriIndex(int vert, int tri)
		{
			this.index_last = vert;
			this.tri_last = tri;
			return this;
		}

		public MdArranger SetWhole(bool check_mesh_inner = true)
		{
			this.index_first = (this.tri_first = 0);
			this.index_last = this.Md.getVertexMax();
			this.tri_last = this.Md.getTriMax();
			if (this.index_last == 0 && check_mesh_inner)
			{
				Mesh mesh = this.Md.getMesh();
				if (mesh != null)
				{
					this.index_last = mesh.vertexCount;
					int[] triangles = mesh.GetTriangles(this.Md.getCurrentSubMeshIndex());
					this.tri_last = ((triangles != null) ? triangles.Length : 0);
				}
			}
			return this;
		}

		public MdArranger CopyIndex(MdArranger From)
		{
			this.Md = From.Md;
			this.index_first = From.index_first;
			this.tri_first = From.tri_first;
			this.index_last = From.index_last;
			this.tri_last = From.tri_last;
			return this;
		}

		public MdArranger revertVerAndTriIndex(ref int pre_v, ref int pre_t)
		{
			if (this.tri_first >= 0)
			{
				pre_v = this.Md.getVertexMax();
				pre_t = this.Md.getTriMax();
				this.Md.revertVerAndTriIndex(this.index_first, this.tri_first, false);
			}
			return this;
		}

		public MdArranger revertVerAndTriIndexAfter(int pre_v, int pre_t, bool apply_to_uv23 = false)
		{
			this.Md.revertVerAndTriIndex(pre_v, pre_t, apply_to_uv23);
			return this;
		}

		public MdArranger revertVerAndTriIndexSaved(bool apply_to_uv23 = false)
		{
			this.Md.revertVerAndTriIndex(X.Mx(this.index_last, 0), this.tri_last, apply_to_uv23);
			return this;
		}

		public MdArranger revertVerAndTriIndexFirstSaved(bool apply_to_uv23 = false)
		{
			this.Md.revertVerAndTriIndex(this.index_first, this.tri_first, apply_to_uv23);
			return this;
		}

		public MdArranger allocateAfterIndex()
		{
			this.Md.allocateAfterIndex(X.Mx(this.index_last, 0), this.tri_last);
			return this;
		}

		public int Length
		{
			get
			{
				return X.Mx(this.index_last, 0) - this.index_first;
			}
		}

		public MdArranger clear(MeshDrawer _Md)
		{
			this.index_last = -1;
			this.index_first = 0;
			this.Md = _Md;
			return this;
		}

		public Vector3 getPos(int i)
		{
			return this.Md.getVertexArray()[this.index_first + i];
		}

		public MdArranger setPos(int i, Vector3 P)
		{
			this.Md.getVertexArray()[this.index_first + i] = P;
			return this;
		}

		public virtual MdArranger translateAll(float x, float y, bool no_divide_ppu = false)
		{
			int length = this.Length;
			if (!no_divide_ppu)
			{
				x *= this.Md.ppu_rev;
				y *= this.Md.ppu_rev;
			}
			for (int i = 0; i < length; i++)
			{
				this.translate(i, x, y, true);
			}
			return this;
		}

		public MdArranger translate(int i, float x, float y, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x *= this.Md.ppu_rev;
				y *= this.Md.ppu_rev;
			}
			Vector3[] vertexArray = this.Md.getVertexArray();
			Vector3 vector = vertexArray[this.index_first + i];
			vector.x += x;
			vector.y += y;
			vertexArray[this.index_first + i] = vector;
			return this;
		}

		public virtual MdArranger scaleAll(float xscl, float yscl, float center_x = 0f, float center_y = 0f, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				center_x *= this.Md.ppu_rev;
				center_y *= this.Md.ppu_rev;
			}
			int length = this.Length;
			for (int i = 0; i < length; i++)
			{
				this.scale(i, xscl, yscl, center_x, center_y, true);
			}
			return this;
		}

		public MdArranger scale(int i, float xscl, float yscl, float center_x = 0f, float center_y = 0f, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				center_x *= this.Md.ppu_rev;
				center_y *= this.Md.ppu_rev;
			}
			Vector3[] vertexArray = this.Md.getVertexArray();
			Vector3 vector = vertexArray[this.index_first + i];
			vector.x = (vector.x - center_x) * xscl + center_x;
			vector.y = (vector.y - center_y) * yscl + center_y;
			vertexArray[this.index_first + i] = vector;
			return this;
		}

		public Vector2 getUv(int i)
		{
			return this.Md.getUvArray()[this.index_first + i];
		}

		public MdArranger setUv(int i, Vector2 P)
		{
			this.Md.getUvArray()[this.index_first + i] = P;
			return this;
		}

		public MdArranger BaseAlpha(float a)
		{
			this.base_alpha_multiply = a;
			return this;
		}

		public MdArranger setColAll(Color32 P, bool only_visible_color = false)
		{
			int length = this.Length;
			Color32[] colorArray = this.Md.getColorArray();
			P.a = (byte)((float)P.a * this.base_alpha_multiply);
			if (only_visible_color)
			{
				for (int i = 0; i < length; i++)
				{
					Color32 color = colorArray[this.index_first + i];
					if (color.a == 0)
					{
						color = P;
						color.a = 0;
						colorArray[this.index_first + i] = color;
					}
					else
					{
						colorArray[this.index_first + i] = P;
					}
				}
			}
			else
			{
				for (int j = 0; j < length; j++)
				{
					colorArray[this.index_first + j] = P;
				}
			}
			return this;
		}

		public MdArranger setColAllGrdation(float poss, float posd, Color32 Ps, Color32 Pd, GRD grd, bool no_divide_ppu = false, bool consider_blend_level_from_alpha = false)
		{
			if (!no_divide_ppu)
			{
				poss *= this.Md.ppu_rev;
				posd *= this.Md.ppu_rev;
			}
			int length = this.Length;
			Color32[] colorArray = this.Md.getColorArray();
			Vector3[] vertexArray = this.Md.getVertexArray();
			float num = ((posd == poss) ? 0f : (1f / (posd - poss)));
			for (int i = 0; i < length; i++)
			{
				Vector3 vector = vertexArray[this.index_first + i];
				float num2 = ((grd == GRD.LEFT2RIGHT) ? vector.x : vector.y);
				MdArranger.CBuf.Set(Ps).blend(Pd, X.MMX(0f, (num2 - poss) * num, 1f));
				if (consider_blend_level_from_alpha)
				{
					Color32 color = colorArray[this.index_first + i];
					MdArranger.CBuf.blend(color, 1f - (float)MdArranger.CBuf.a / 255f);
					MdArranger.CBuf.a = color.a;
				}
				colorArray[this.index_first + i] = MdArranger.CBuf.mulA(this.base_alpha_multiply).C;
			}
			return this;
		}

		public MdArranger setColAllGrdation(float poss, float posd, Color32[] Ps, float[] Alvl, GRD grd, bool no_divide_ppu = false, float alpha_lvl = 1f, bool alpha_mul = false)
		{
			if (!no_divide_ppu)
			{
				poss *= this.Md.ppu_rev;
				posd *= this.Md.ppu_rev;
			}
			int length = this.Length;
			Color32[] colorArray = this.Md.getColorArray();
			Vector3[] vertexArray = this.Md.getVertexArray();
			float num = ((posd == poss) ? 0f : (1f / (posd - poss)));
			float num2 = (float)(Ps.Length - 1);
			for (int i = 0; i < length; i++)
			{
				Vector3 vector = vertexArray[this.index_first + i];
				float num3 = (((grd == GRD.LEFT2RIGHT) ? vector.x : vector.y) - poss) * num;
				float arrayRealPos = X.getArrayRealPos(Alvl, num3 * 255f, 0f);
				if (arrayRealPos >= num2)
				{
					MdArranger.CBuf.Set(Ps[(int)num2]);
				}
				else if (arrayRealPos <= 0f)
				{
					MdArranger.CBuf.Set(Ps[0]);
				}
				else
				{
					MdArranger.CBuf.Set(Ps[(int)arrayRealPos]).blend(Ps[(int)arrayRealPos + 1], arrayRealPos - (float)((int)arrayRealPos));
				}
				MdArranger.CBuf.mulA(alpha_lvl * this.base_alpha_multiply * (alpha_mul ? ((float)colorArray[this.index_first + i].a / 255f) : 1f));
				colorArray[this.index_first + i] = MdArranger.CBuf.C;
			}
			return this;
		}

		public MdArranger setAlpha1(float l, bool only_alp_over_zero = false)
		{
			int length = this.Length;
			byte b = (byte)(255f * l * this.base_alpha_multiply);
			Color32[] colorArray = this.Md.getColorArray();
			if (only_alp_over_zero)
			{
				for (int i = 0; i < length; i++)
				{
					if (colorArray[this.index_first + i].a != 0)
					{
						colorArray[this.index_first + i].a = b;
					}
				}
			}
			else
			{
				for (int j = 0; j < length; j++)
				{
					colorArray[this.index_first + j].a = b;
				}
			}
			return this;
		}

		public MdArranger mulAlpha(float l)
		{
			int length = this.Length;
			l *= this.base_alpha_multiply;
			Color32[] colorArray = this.Md.getColorArray();
			for (int i = 0; i < length; i++)
			{
				colorArray[this.index_first + i].a = (byte)((float)colorArray[this.index_first + i].a * l);
			}
			return this;
		}

		public Color32 getCol(int i)
		{
			return this.Md.getColorArray()[this.index_first + i];
		}

		public MdArranger setCol(int i, Color32 P)
		{
			this.Md.getColorArray()[this.index_first + i] = P;
			return this;
		}

		public MdArranger copySliding(float shiftx, float shifty, int index)
		{
			if (this.index_last < 0)
			{
				return this;
			}
			int num = this.index_last - this.index_first;
			int num2 = this.tri_last - this.tri_first;
			int num3 = this.index_last + (index + 1) * num;
			int num4 = this.tri_last + (index + 1) * num2;
			this.Md.allocVer(num3, 1).allocTri(num4, 1);
			Color32[] colorArray = this.Md.getColorArray();
			Vector3[] vertexArray = this.Md.getVertexArray();
			Vector2[] uvArray = this.Md.getUvArray();
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = vertexArray[this.index_first + i];
				vector.x += shiftx;
				vector.y += shifty;
				vertexArray[num3 - num + i] = vector;
				colorArray[num3 - num + i] = colorArray[this.index_first + i];
				uvArray[num3 - num + i] = uvArray[this.index_first + i];
			}
			int[] triangleArray = this.Md.getTriangleArray();
			for (int j = 0; j < num2; j++)
			{
				triangleArray[num4 - num2 + j] = triangleArray[this.tri_first + j];
			}
			this.Md.revertVerAndTriIndex(X.Mx(num3, this.Md.getVertexMax()), X.Mx(num4, this.Md.getTriMax()), false);
			return this;
		}

		public bool vertexContains(List<int> A, bool splice = true)
		{
			if (A == null)
			{
				return false;
			}
			int num = A.Count;
			int num2 = num / 2 * 2 - 2;
			bool flag = false;
			for (int i = num2; i >= 0; i -= 2)
			{
				int num3 = A[i];
				int num4 = ((i + 1 == num) ? this.index_last : A[i + 1]);
				if (X.isContaining((float)this.index_first, (float)this.index_last, (float)num3, (float)num4, 0f))
				{
					flag = true;
					if (!splice)
					{
						return flag;
					}
					A.RemoveAt(i);
					if (--num > i)
					{
						A.RemoveAt(i);
						num--;
					}
				}
			}
			return flag;
		}

		public MeshDrawer get_Md()
		{
			return this.Md;
		}

		public int getStartVerIndex()
		{
			return this.index_first;
		}

		public int getStartTriIndex()
		{
			return this.tri_first;
		}

		public int getEndVerIndex()
		{
			return X.Mx(this.index_last, 0);
		}

		public int getEndTriIndex()
		{
			return X.Mx(this.tri_last, 0);
		}

		protected MeshDrawer Md;

		protected int index_first;

		protected int tri_first;

		protected int tri_last;

		protected int index_last = -1;

		private static C32 CBuf = new C32();

		public float base_alpha_multiply = 1f;
	}
}
