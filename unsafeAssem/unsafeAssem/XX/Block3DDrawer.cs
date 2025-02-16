using System;
using UnityEngine;

namespace XX
{
	public class Block3DDrawer
	{
		public Block3DDrawer clear()
		{
			this.Bounds = Vector3.zero;
			this.Apt = null;
			this.Ahen = null;
			this.pointmax = (this.henmax = 0);
			return this;
		}

		private Block3DDrawer.BlockPt AddPt(Block3DDrawer.BlockPt P, int dmax)
		{
			int num = -1;
			for (int i = 0; i < dmax; i++)
			{
				if (this.Apt[i].isSame(P))
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				this.Apt[num] = P;
			}
			else
			{
				if (this.pointmax >= this.Apt.Length)
				{
					Array.Resize<Block3DDrawer.BlockPt>(ref this.Apt, this.pointmax + 1);
				}
				Block3DDrawer.BlockPt[] apt = this.Apt;
				int num2 = this.pointmax;
				this.pointmax = num2 + 1;
				apt[num2] = P;
			}
			return P;
		}

		protected Block3DDrawer AddPt(float _x, float _y, float _z)
		{
			this.AddPt(new Block3DDrawer.BlockPt(_x, _y, _z), this.pointmax);
			return this;
		}

		private Block3DDrawer AddHen(Block3DDrawer.BlockHen H, int dmax)
		{
			int num = -1;
			for (int i = 0; i < dmax; i++)
			{
				if (this.Ahen[i].isSame(H))
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				this.Ahen[num] = H;
			}
			else
			{
				if (this.henmax >= this.Ahen.Length)
				{
					Array.Resize<Block3DDrawer.BlockHen>(ref this.Ahen, this.henmax + 1);
				}
				Block3DDrawer.BlockHen[] ahen = this.Ahen;
				int num2 = this.henmax;
				this.henmax = num2 + 1;
				ahen[num2] = H;
			}
			return this;
		}

		public Block3DDrawer AddHen(Vector3 S, Vector3 D, bool add_pt = true)
		{
			return this.AddHen(S.x, S.y, S.z, D.x, D.y, D.z, add_pt);
		}

		public Block3DDrawer AddHen(float _x, float _y, float _z, float dx, float dy, float dz, bool add_pt = true)
		{
			Block3DDrawer.BlockPt blockPt = new Block3DDrawer.BlockPt(_x, _y, _z);
			Block3DDrawer.BlockPt blockPt2 = new Block3DDrawer.BlockPt(dx, dy, dz);
			if (add_pt)
			{
				if (this.pointmax + 1 >= this.Apt.Length)
				{
					Array.Resize<Block3DDrawer.BlockPt>(ref this.Apt, this.pointmax + 1 + 1);
				}
				this.AddPt(blockPt, this.pointmax);
				this.AddPt(blockPt2, this.pointmax);
			}
			this.AddHen(new Block3DDrawer.BlockHen(blockPt, blockPt2), this.henmax);
			return this;
		}

		public Vector3[] AddPoly(float r, int kaku, Matrix4x4 Trs, bool add_pt = true)
		{
			Vector3[] array = new Vector3[kaku];
			float num = 6.2831855f / (float)kaku;
			float num2 = 0f;
			kaku--;
			for (int i = 0; i <= kaku; i++)
			{
				array[i] = Trs.MultiplyPoint3x4(new Vector3(r * X.Cos(num2), r * X.Sin(num2), 0f));
				num2 += num;
			}
			if (add_pt)
			{
				if (this.Apt == null)
				{
					this.Apt = new Block3DDrawer.BlockPt[this.pointmax + kaku + 1];
				}
				if (this.pointmax + kaku >= this.Apt.Length)
				{
					Array.Resize<Block3DDrawer.BlockPt>(ref this.Apt, this.pointmax + kaku + 1);
				}
				int num3 = this.pointmax;
				for (int j = 0; j <= kaku; j++)
				{
					this.AddPt(new Block3DDrawer.BlockPt(array[j]), num3);
				}
			}
			if (this.Ahen == null)
			{
				this.Ahen = new Block3DDrawer.BlockHen[this.henmax + kaku + 1];
			}
			if (this.henmax + kaku >= this.Ahen.Length)
			{
				Array.Resize<Block3DDrawer.BlockHen>(ref this.Ahen, this.henmax + kaku + 1);
			}
			int num4 = this.henmax;
			for (int k = 0; k <= kaku; k++)
			{
				this.AddHen(new Block3DDrawer.BlockHen(array[k], array[(k == kaku) ? 0 : (k + 1)]), num4);
			}
			return array;
		}

		public Block3DDrawer defineBlock2D(params int[] Axy)
		{
			int num = Axy.Length / 2;
			if (this.Apt == null)
			{
				this.Apt = new Block3DDrawer.BlockPt[num * 8];
			}
			else if (this.Apt.Length < this.pointmax + num * 8)
			{
				Array.Resize<Block3DDrawer.BlockPt>(ref this.Apt, this.pointmax + num * 8);
			}
			if (this.Ahen == null)
			{
				this.Ahen = new Block3DDrawer.BlockHen[num * 12];
			}
			else if (this.Ahen.Length < this.henmax + num * 12)
			{
				Array.Resize<Block3DDrawer.BlockPt>(ref this.Apt, this.henmax + num * 12);
			}
			for (int i = 0; i < num; i++)
			{
				int num2 = Axy[i * 2];
				int num3 = Axy[i * 2 + 1];
				int num4 = this.pointmax;
				int num5 = this.henmax;
				Block3DDrawer.BlockPt blockPt = this.AddPt(new Block3DDrawer.BlockPt((float)num2, (float)num3, 0f), num4);
				Block3DDrawer.BlockPt blockPt2 = this.AddPt(new Block3DDrawer.BlockPt((float)num2, (float)(num3 + 1), 0f), num4);
				Block3DDrawer.BlockPt blockPt3 = this.AddPt(new Block3DDrawer.BlockPt((float)(num2 + 1), (float)(num3 + 1), 0f), num4);
				Block3DDrawer.BlockPt blockPt4 = this.AddPt(new Block3DDrawer.BlockPt((float)(num2 + 1), (float)num3, 0f), num4);
				Block3DDrawer.BlockPt blockPt5 = this.AddPt(new Block3DDrawer.BlockPt((float)num2, (float)num3, 1f), num4);
				Block3DDrawer.BlockPt blockPt6 = this.AddPt(new Block3DDrawer.BlockPt((float)num2, (float)(num3 + 1), 1f), num4);
				Block3DDrawer.BlockPt blockPt7 = this.AddPt(new Block3DDrawer.BlockPt((float)(num2 + 1), (float)(num3 + 1), 1f), num4);
				Block3DDrawer.BlockPt blockPt8 = this.AddPt(new Block3DDrawer.BlockPt((float)(num2 + 1), (float)num3, 1f), num4);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt, blockPt2), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt, blockPt4), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt, blockPt5), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt3, blockPt2), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt3, blockPt4), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt3, blockPt7), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt6, blockPt7), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt6, blockPt5), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt6, blockPt2), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt8, blockPt7), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt8, blockPt5), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt8, blockPt4), num5);
				this.Bounds.x = X.Mx(this.Bounds.x, (float)(num2 + 1));
				this.Bounds.y = X.Mx(this.Bounds.y, (float)(num3 + 1));
			}
			this.Bounds.z = X.Mx(this.Bounds.z, 1f);
			return this;
		}

		public Block3DDrawer defineBlock3D(params int[] Axyz)
		{
			int num = Axyz.Length / 3;
			if (this.Apt == null)
			{
				this.Apt = new Block3DDrawer.BlockPt[num * 8];
			}
			else if (this.Apt.Length < this.pointmax + num * 8)
			{
				Array.Resize<Block3DDrawer.BlockPt>(ref this.Apt, this.pointmax + num * 8);
			}
			if (this.Ahen == null)
			{
				this.Ahen = new Block3DDrawer.BlockHen[num * 12];
			}
			else if (this.Ahen.Length < this.henmax + num * 12)
			{
				Array.Resize<Block3DDrawer.BlockPt>(ref this.Apt, this.henmax + num * 12);
			}
			for (int i = 0; i < num; i++)
			{
				int num2 = Axyz[i * 3];
				int num3 = Axyz[i * 3 + 1];
				int num4 = Axyz[i * 3 + 2];
				int num5 = this.pointmax;
				int num6 = this.henmax;
				Block3DDrawer.BlockPt blockPt = this.AddPt(new Block3DDrawer.BlockPt((float)num2, (float)num3, (float)num4), num5);
				Block3DDrawer.BlockPt blockPt2 = this.AddPt(new Block3DDrawer.BlockPt((float)num2, (float)(num3 + 1), (float)num4), num5);
				Block3DDrawer.BlockPt blockPt3 = this.AddPt(new Block3DDrawer.BlockPt((float)(num2 + 1), (float)(num3 + 1), (float)num4), num5);
				Block3DDrawer.BlockPt blockPt4 = this.AddPt(new Block3DDrawer.BlockPt((float)(num2 + 1), (float)num3, (float)num4), num5);
				Block3DDrawer.BlockPt blockPt5 = this.AddPt(new Block3DDrawer.BlockPt((float)num2, (float)num3, (float)(num4 + 1)), num5);
				Block3DDrawer.BlockPt blockPt6 = this.AddPt(new Block3DDrawer.BlockPt((float)num2, (float)(num3 + 1), (float)(num4 + 1)), num5);
				Block3DDrawer.BlockPt blockPt7 = this.AddPt(new Block3DDrawer.BlockPt((float)(num2 + 1), (float)(num3 + 1), (float)(num4 + 1)), num5);
				Block3DDrawer.BlockPt blockPt8 = this.AddPt(new Block3DDrawer.BlockPt((float)(num2 + 1), (float)num3, (float)(num4 + 1)), num5);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt, blockPt2), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt, blockPt4), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt, blockPt5), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt3, blockPt2), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt3, blockPt4), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt3, blockPt7), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt6, blockPt7), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt6, blockPt5), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt6, blockPt2), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt8, blockPt7), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt8, blockPt5), num6);
				this.AddHen(new Block3DDrawer.BlockHen(blockPt8, blockPt4), num6);
				this.Bounds.x = X.Mx(this.Bounds.x, (float)(num2 + 1));
				this.Bounds.y = X.Mx(this.Bounds.y, (float)(num3 + 1));
				this.Bounds.z = X.Mx(this.Bounds.z, (float)(num4 + 1));
			}
			return this;
		}

		public void drawTo(MeshDrawer Md, Matrix4x4 Mx)
		{
			Vector3 vector = -this.Bounds * 0.5f;
			if (this.Apt != null && (this.point_lgt > 0f || this.point_blur_in_alp > 0f))
			{
				Color32 color = (Md.Col = Md.ColGrd.Set(this.col_point_).C);
				for (int i = 0; i < this.pointmax; i++)
				{
					Block3DDrawer.BlockPt blockPt = this.Apt[i];
					Vector3 vector2 = Mx.MultiplyPoint3x4((blockPt.getV() + vector) * this.block_size_u);
					if (this.point_lgt > 0f)
					{
						Md.Col = color;
						Md.Circle(vector2.x, vector2.y, this.point_lgt, 0f, true, 0f, 0f);
					}
					if (this.point_blur_lgt > 0f)
					{
						Md.Col = Md.ColGrd.setA1(this.point_blur_in_alp).C;
						Md.ColGrd.setA(0f);
						Md.Circle(vector2.x, vector2.y, this.point_blur_lgt, 0f, true, 1f, 0f);
					}
				}
			}
			if (this.Ahen != null && (this.hen_thick > 0f || this.hen_blur_in_alp > 0f))
			{
				Color32 color2 = (Md.Col = Md.ColGrd.Set(this.col_hen_).C);
				for (int j = 0; j < this.henmax; j++)
				{
					Block3DDrawer.BlockHen blockHen = this.Ahen[j];
					Vector3 vector3 = Mx.MultiplyPoint3x4((blockHen.getVS() + vector) * this.block_size_u);
					Vector3 vector4 = Mx.MultiplyPoint3x4((blockHen.getVD() + vector) * this.block_size_u);
					if (this.hen_thick > 0f)
					{
						Md.Col = color2;
						Md.Line(vector3.x, vector3.y, vector4.x, vector4.y, this.hen_thick, true, 0f, 0f);
					}
					if (this.hen_blur_thick > 0f)
					{
						Md.Col = Md.ColGrd.setA1(this.hen_blur_in_alp).C;
						Md.ColGrd.setA(0f);
						Md.BlurLine(vector3.x, vector3.y, vector4.x, vector4.y, this.hen_blur_thick, 3, 0.3125f, true);
					}
				}
			}
		}

		public virtual uint col_point
		{
			get
			{
				return this.col_point_;
			}
			set
			{
				this.col_point_ = value;
			}
		}

		public virtual uint col_hen
		{
			get
			{
				return this.col_hen_;
			}
			set
			{
				this.col_hen_ = value;
			}
		}

		protected Mesh Ms;

		protected Block3DDrawer.BlockPt[] Apt;

		protected Block3DDrawer.BlockHen[] Ahen;

		protected int pointmax;

		protected int henmax;

		public Vector3 Bounds = Vector3.zero;

		public float block_size_u = 1f;

		protected uint col_point_ = uint.MaxValue;

		protected uint col_hen_ = uint.MaxValue;

		public float hen_thick = 0.03125f;

		public float hen_blur_thick = 0.53125f;

		public float point_lgt = 0.078125f;

		public float point_blur_lgt = 0.703125f;

		public float hen_blur_in_alp = 0.75f;

		public float point_blur_in_alp = 0.75f;

		protected class BlockPt
		{
			public BlockPt(float _x, float _y, float _z)
			{
				this.x = _x;
				this.y = _y;
				this.z = _z;
			}

			public BlockPt(Vector3 V)
			{
				this.x = V.x;
				this.y = V.y;
				this.z = V.z;
			}

			public bool isSame(Block3DDrawer.BlockPt P)
			{
				return this.x == P.x && this.y == P.y && this.z == P.z;
			}

			public Vector3 getV()
			{
				return new Vector3(this.x, this.y, this.z);
			}

			public float x;

			public float y;

			public float z;

			public Block3DDrawer.BlockPt[] Ahen;
		}

		protected class BlockHen
		{
			public BlockHen(Block3DDrawer.BlockPt S, Block3DDrawer.BlockPt D)
			{
				this.x = S.x;
				this.y = S.y;
				this.z = S.z;
				this.dx = D.x;
				this.dy = D.y;
				this.dz = D.z;
			}

			public BlockHen(Vector3 S, Vector3 D)
			{
				this.x = S.x;
				this.y = S.y;
				this.z = S.z;
				this.dx = D.x;
				this.dy = D.y;
				this.dz = D.z;
			}

			public bool isSame(Block3DDrawer.BlockHen P)
			{
				return (this.x == P.x && this.y == P.y && this.z == P.z && this.dx == P.dx && this.dy == P.dy && this.dz == P.dz) || (this.dx == P.x && this.dy == P.y && this.dz == P.z && this.x == P.dx && this.y == P.dy && this.z == P.dz);
			}

			public Vector3 getVS()
			{
				return new Vector3(this.x, this.y, this.z);
			}

			public Vector3 getVD()
			{
				return new Vector3(this.dx, this.dy, this.dz);
			}

			public float x;

			public float y;

			public float z;

			public float dx;

			public float dy;

			public float dz;
		}
	}
}
