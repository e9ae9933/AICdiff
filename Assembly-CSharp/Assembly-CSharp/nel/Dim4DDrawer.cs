using System;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class Dim4DDrawer
	{
		public float xyagR
		{
			get
			{
				return this.xyagR_;
			}
			set
			{
				this.xyagR_ = value;
				this.need_fine_pos = true;
			}
		}

		public float yzagR
		{
			get
			{
				return this.yzagR_;
			}
			set
			{
				this.yzagR_ = value;
				this.need_fine_pos = true;
			}
		}

		public float xzagR
		{
			get
			{
				return this.xzagR_;
			}
			set
			{
				this.xzagR_ = value;
				this.need_fine_pos = true;
			}
		}

		public float xwagR
		{
			get
			{
				return this.xwagR_;
			}
			set
			{
				this.xwagR_ = value;
				this.need_fine_pos = true;
			}
		}

		public float ywagR
		{
			get
			{
				return this.ywagR_;
			}
			set
			{
				this.ywagR_ = value;
				this.need_fine_pos = true;
			}
		}

		public float zwagR
		{
			get
			{
				return this.zwagR_;
			}
			set
			{
				this.zwagR_ = value;
				this.need_fine_pos = true;
			}
		}

		public Dim4DDrawer(int _max_pt)
		{
			this.MAX_PT = _max_pt;
			this.APos = new Vector4[this.MAX_PT];
		}

		public Dim4DDrawer clear()
		{
			this.xyagR_ = 0f;
			this.yzagR_ = 0f;
			this.xzagR_ = 0f;
			this.xwagR_ = 0f;
			this.ywagR_ = 0f;
			this.zwagR_ = 0f;
			this.need_fine_pos = true;
			return this;
		}

		public void drawTo(MeshDrawer Md, float scale_xy = 1f)
		{
			this.finePos();
			if (this.FD_DrawSurface != null)
			{
				this.drawSurfaceTo(Md, scale_xy);
			}
			if (this.FD_DrawPoint != null)
			{
				for (int i = 0; i < this.MAX_PT; i++)
				{
					Vector4 vector = this.ScaleP(i, scale_xy);
					this.FD_DrawPoint(Md, vector);
				}
			}
			if (this.FD_DrawLine != null)
			{
				this.drawLineTo(Md, scale_xy);
			}
		}

		public abstract void drawLineTo(MeshDrawer Md, float scale_xy = 1f);

		public abstract void drawSurfaceTo(MeshDrawer Md, float scale_xy = 1f);

		protected Vector4 ScaleP(int i, float scale_xy)
		{
			Vector4 vector = this.APos[i];
			vector.x *= scale_xy;
			vector.y *= scale_xy;
			return vector;
		}

		protected void finePos()
		{
			if (!this.need_fine_pos)
			{
				return;
			}
			this.need_fine_pos = false;
			Matrix4x4 matrix4x = Matrix4x4.identity;
			matrix4x = this.Rot(matrix4x, this.xyagR_, 2, 2, 3, 3);
			matrix4x = this.Rot(matrix4x, this.yzagR_, 0, 0, 3, 3);
			matrix4x = this.Rot(matrix4x, this.xzagR_, 1, 1, 3, 3);
			matrix4x = this.Rot(matrix4x, this.xwagR_, 1, 1, 2, 2);
			matrix4x = this.Rot(matrix4x, this.yzagR_, 0, 0, 2, 2);
			matrix4x = this.Rot(matrix4x, this.zwagR_, 0, 0, 1, 1);
			for (int i = 0; i < this.MAX_PT; i++)
			{
				Vector4 pt = this.GetPt(i);
				Vector4 vector = new Vector4(matrix4x[0, 0] * pt.x + matrix4x[0, 1] * pt.y + matrix4x[0, 2] * pt.z + matrix4x[0, 3] * pt.w, matrix4x[1, 0] * pt.x + matrix4x[1, 1] * pt.y + matrix4x[1, 2] * pt.z + matrix4x[1, 3] * pt.w, matrix4x[2, 0] * pt.x + matrix4x[2, 1] * pt.y + matrix4x[2, 2] * pt.z + matrix4x[2, 3] * pt.w, matrix4x[3, 0] * pt.x + matrix4x[3, 1] * pt.y + matrix4x[3, 2] * pt.z + matrix4x[3, 3] * pt.w);
				float num = this.Rw_length / (this.Rw_length - vector.w);
				Vector4 vector2 = new Vector4(vector.x * num, vector.y * num, vector.z * num);
				float num2 = this.Rz_length / (this.Rz_length - vector2.z);
				Vector4 vector3 = new Vector4(vector2.x * num2, vector2.y * num2, vector2.z, num2);
				this.APos[i] = vector3;
			}
		}

		private Matrix4x4 Rot(Matrix4x4 Src, float agR, int clms0, int row0, int clms1, int row1)
		{
			if (agR == 0f)
			{
				return Src;
			}
			Matrix4x4 identity = Matrix4x4.identity;
			float num = X.Cos(agR);
			float num2 = X.Sin(agR);
			identity[row0, clms0] = num;
			identity[row0, clms1] = -num2;
			identity[row1, clms0] = num2;
			identity[row1, clms1] = num;
			return identity * Src;
		}

		protected Vector4 GetPt(int i)
		{
			Vector4 pt_Inner = this.GetPt_Inner(i);
			Vector3 vector = this.MxTransform.MultiplyPoint3x4(pt_Inner);
			return new Vector4(vector.x, vector.y, pt_Inner.z, pt_Inner.w);
		}

		protected abstract Vector4 GetPt_Inner(int i);

		public float Rw_length = 5f;

		public float Rz_length = 15f;

		private float xyagR_;

		private float yzagR_;

		private float xzagR_;

		private float xwagR_;

		private float ywagR_;

		private float zwagR_;

		public Matrix4x4 MxTransform = Matrix4x4.identity;

		private Vector4[] APos;

		private bool need_fine_pos = true;

		public Dim4DDrawer.FnDrawPoint FD_DrawPoint;

		public Dim4DDrawer.FnDrawLine FD_DrawLine;

		public Dim4DDrawer.FnDrawSurface FD_DrawSurface;

		public readonly int MAX_PT;

		public delegate void FnDrawPoint(MeshDrawer Md, Vector4 P);

		public delegate void FnDrawLine(MeshDrawer Md, Vector4 P0, Vector4 P1);

		public delegate void FnDrawSurface(MeshDrawer Md, int index, Vector4 P0, Vector4 P1, Vector4 P2);
	}
}
