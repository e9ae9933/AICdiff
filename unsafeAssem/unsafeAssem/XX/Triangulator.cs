using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class Triangulator
	{
		public Mesh CreateMesh(Vector3[] vertices, Vector2[] Auv, bool calc_bounds = true, bool reverse_flag = false)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = this.CreateTriArray(vertices, reverse_flag).ToArray();
			mesh.uv = ((Auv == null) ? new Vector2[vertices.Length] : Auv);
			if (calc_bounds)
			{
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();
			}
			return mesh;
		}

		public List<int> CreateTriArray(Vector3[] vertices, bool reverse_flag = false)
		{
			List<int> list = this.Triangulate(vertices, 0, -1, null, 0);
			if (reverse_flag)
			{
				int count = list.Count;
				for (int i = 1; i < count; i += 3)
				{
					List<int> list2 = list;
					int num = i;
					List<int> list3 = list;
					int num2 = i + 1;
					int num3 = list[i + 1];
					int num4 = list[i];
					list2[num] = num3;
					list3[num2] = num4;
				}
			}
			return list;
		}

		public List<int> Triangulate(Vector2[] points, int si, int length = -1, List<int> Adest = null, int start_ver_i = 0)
		{
			this.m_points.Clear();
			if (length < 0)
			{
				length = points.Length - si;
			}
			int num = si + length;
			for (int i = si; i < num; i++)
			{
				this.m_points.Add(points[i]);
			}
			return this.TriangulateInner(Adest, start_ver_i);
		}

		public List<int> Triangulate(List<Vector3> points, int si, int length = -1, List<int> Adest = null, int start_ver_i = 0)
		{
			this.m_points.Clear();
			if (length < 0)
			{
				length = points.Count - si;
			}
			int num = si + length;
			for (int i = si; i < num; i++)
			{
				this.m_points.Add(points[i]);
			}
			return this.TriangulateInner(Adest, start_ver_i);
		}

		public List<int> Triangulate(Vector3[] points, int si, int length = -1, List<int> Adest = null, int start_ver_i = 0)
		{
			this.m_points.Clear();
			if (length < 0)
			{
				length = points.Length - si;
			}
			int num = si + length;
			for (int i = si; i < num; i++)
			{
				this.m_points.Add(new Vector2(points[i].x, points[i].y));
			}
			return this.TriangulateInner(Adest, start_ver_i);
		}

		public List<int> Triangulate(Vector4[] points, int si, int length = -1, List<int> Adest = null, int start_ver_i = 0)
		{
			this.m_points.Clear();
			if (length < 0)
			{
				length = points.Length - si;
			}
			int num = si + length;
			for (int i = si; i < num; i++)
			{
				this.m_points.Add(new Vector2(points[i].x, points[i].y));
			}
			return this.TriangulateInner(Adest, start_ver_i);
		}

		private List<int> TriangulateInner(List<int> indices, int start_ver_i = 0)
		{
			if (indices == null)
			{
				indices = this.indices;
				indices.Clear();
			}
			int count = this.m_points.Count;
			if (count < 3)
			{
				return indices;
			}
			int[] array = new int[count];
			if (this.Area() > 0f)
			{
				for (int i = 0; i < count; i++)
				{
					array[i] = i;
				}
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					array[j] = count - 1 - j;
				}
			}
			int k = count;
			int num = 2 * k;
			int num2 = k - 1;
			while (k > 2)
			{
				if (num-- <= 0)
				{
					return indices;
				}
				int num3 = num2;
				if (k <= num3)
				{
					num3 = 0;
				}
				num2 = num3 + 1;
				if (k <= num2)
				{
					num2 = 0;
				}
				int num4 = num2 + 1;
				if (k <= num4)
				{
					num4 = 0;
				}
				if (this.Snip(num3, num2, num4, k, array))
				{
					int num5 = array[num3];
					int num6 = array[num2];
					int num7 = array[num4];
					indices.Add(start_ver_i + num5);
					indices.Add(start_ver_i + num7);
					indices.Add(start_ver_i + num6);
					int num8 = num2;
					for (int l = num2 + 1; l < k; l++)
					{
						array[num8] = array[l];
						num8++;
					}
					k--;
					num = 2 * k;
				}
			}
			return indices;
		}

		private float Area()
		{
			int count = this.m_points.Count;
			float num = 0f;
			int num2 = count - 1;
			int i = 0;
			while (i < count)
			{
				Vector2 vector = this.m_points[num2];
				Vector2 vector2 = this.m_points[i];
				num += vector.x * vector2.y - vector2.x * vector.y;
				num2 = i++;
			}
			return num * 0.5f;
		}

		private bool Snip(int u, int v, int w, int n, int[] V)
		{
			Vector2 vector = this.m_points[V[u]];
			Vector2 vector2 = this.m_points[V[v]];
			Vector2 vector3 = this.m_points[V[w]];
			if (Mathf.Epsilon > (vector2.x - vector.x) * (vector3.y - vector.y) - (vector2.y - vector.y) * (vector3.x - vector.x))
			{
				return false;
			}
			for (int i = 0; i < n; i++)
			{
				if (i != u && i != v && i != w)
				{
					Vector2 vector4 = this.m_points[V[i]];
					if (this.InsideTriangle(vector, vector2, vector3, vector4))
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
		{
			float num = C.x - B.x;
			float num2 = C.y - B.y;
			float num3 = A.x - C.x;
			float num4 = A.y - C.y;
			float num5 = B.x - A.x;
			float num6 = B.y - A.y;
			float num7 = P.x - A.x;
			float num8 = P.y - A.y;
			float num9 = P.x - B.x;
			float num10 = P.y - B.y;
			float num11 = P.x - C.x;
			float num12 = P.y - C.y;
			float num13 = num * num10 - num2 * num9;
			float num14 = num5 * num8 - num6 * num7;
			float num15 = num3 * num12 - num4 * num11;
			return num13 >= 0f && num15 >= 0f && num14 >= 0f;
		}

		private List<Vector2> m_points = new List<Vector2>();

		private List<int> indices = new List<int>(24);
	}
}
