using System;
using UnityEngine;

namespace XX
{
	public class OctaHedronDrawer
	{
		public OctaHedronDrawer(float half_w_px, float half_h_px)
		{
			this.w = half_w_px;
			this.h = half_h_px;
		}

		public OctaHedronDrawer AgR(float _agR)
		{
			this.agR_ = X.correctangleR(_agR);
			while (this.agR_ <= 0.7853982f)
			{
				this.agR_ += 1.5707964f;
			}
			while (this.agR_ >= 2.3561945f)
			{
				this.agR_ -= 1.5707964f;
			}
			return this;
		}

		public float agR
		{
			get
			{
				return this.agR_;
			}
			set
			{
				if (this.agR_ == value)
				{
					return;
				}
				this.AgR(value);
			}
		}

		public float zagR
		{
			get
			{
				return this.zagR_;
			}
			set
			{
				if (this.zagR_ == value)
				{
					return;
				}
				this.zagR_ = value;
				this.zcos = X.Cos(this.zagR_);
				this.zsin = X.Sin(this.zagR_);
			}
		}

		private bool isPointFront(int id)
		{
			switch (id)
			{
			case 0:
			case 1:
			case 4:
				return true;
			case 2:
				return false;
			case 3:
				return this.agR_ != 0.7853982f;
			default:
				return false;
			}
		}

		public Vector3 getPointC(int i)
		{
			return new Vector3(this.w * X.Cos(this.agR_ + (float)i * 1.5707964f), -this.w * X.Sin(this.agR_ + (float)i * 1.5707964f) * this.zsin, (float)(this.isPointFront(i) ? 1 : 0));
		}

		public float dheight
		{
			get
			{
				return this.h * this.zcos;
			}
		}

		public OctaHedronDrawer drawPoint(MeshDrawer Md, float x0, float y0, OctaHedronDrawer.FnDrawOctaHedronPoint fnDraw)
		{
			for (int i = 0; i < 2; i++)
			{
				if ((this.draw_tb & ((i == 0 || 2 != 0) ? 1 : 0)) != 0)
				{
					fnDraw(this, Md, x0, y0 + this.dheight * (float)X.MPF(i == 0), true);
				}
			}
			for (int j = 0; j < 4; j++)
			{
				Vector3 pointC = this.getPointC(j);
				fnDraw(this, Md, x0 + pointC.x, y0 + pointC.y, pointC.z > 0f);
			}
			return this;
		}

		public OctaHedronDrawer drawLine(MeshDrawer Md, float x0, float y0, OctaHedronDrawer.FnDrawOctaHedronLine fnDraw, float slevel = 0f, float elevel = 1f)
		{
			float dheight = this.dheight;
			Vector3 vector = this.getPointC(0);
			bool flag = vector.z > 0f;
			for (int i = 1; i <= 4; i++)
			{
				Vector3 pointC = this.getPointC(i);
				bool flag2 = pointC.z > 0f;
				fnDraw(this, Md, x0 + X.NI(vector.x, pointC.x, slevel), y0 + X.NI(vector.y, pointC.y, slevel), x0 + X.NI(vector.x, pointC.x, elevel), y0 + X.NI(vector.y, pointC.y, elevel), flag2 && flag, 0);
				for (int j = 0; j < 2; j++)
				{
					if ((this.draw_tb & ((j == 0 || 2 != 0) ? 1 : 0)) != 0)
					{
						float num = ((j == 0) ? dheight : (-dheight));
						fnDraw(this, Md, x0 + X.NI(pointC.x, 0f, slevel), y0 + X.NI(pointC.y, num, slevel), x0 + X.NI(pointC.x, 0f, elevel), y0 + X.NI(pointC.y, num, elevel), flag2, (j == 0) ? 1 : (-1));
					}
				}
				vector = pointC;
				flag = flag2;
			}
			return this;
		}

		public OctaHedronDrawer drawSurface(MeshDrawer Md, float x0, float y0, OctaHedronDrawer.FnDrawOctaHedronSurface fnDraw)
		{
			float dheight = this.dheight;
			for (int i = 0; i < 2; i++)
			{
				float num = this.agR_;
				Vector3 vector = this.getPointC(0);
				bool flag = vector.z > 0f;
				for (int j = 1; j <= 4; j++)
				{
					float num2 = num + 1.5707964f;
					Vector3 pointC = this.getPointC(j);
					bool flag2 = pointC.z > 0f;
					if ((flag2 && flag) == (i == 1))
					{
						float num3 = X.correctangleR((num + num2) / 2f);
						float num4 = ((num3 >= 0f) ? X.angledifR(num3, 1.5707964f) : X.angledifR(num3, -1.5707964f)) / 1.5707964f;
						for (int k = 0; k < 2; k++)
						{
							if ((this.draw_tb & ((k == 0 || 2 != 0) ? 1 : 0)) != 0)
							{
								float num5 = ((k == 0) ? dheight : (-dheight));
								if (k == 0 == (i == 1))
								{
									fnDraw(this, Md, x0, y0 + num5, x0 + vector.x, y0 + vector.y, x0 + pointC.x, y0 + pointC.y, num4, false, i == 1);
								}
								else
								{
									fnDraw(this, Md, x0, y0 + num5, x0 + pointC.x, y0 + pointC.y, x0 + vector.x, y0 + vector.y, num4, true, i == 1);
								}
							}
						}
					}
					num = num2;
					vector = pointC;
					flag = flag2;
				}
			}
			return this;
		}

		public float w = 20f;

		public float h = 30f;

		private float agR_;

		private float zagR_;

		private float zcos;

		private float zsin;

		public byte draw_tb = 3;

		public delegate void FnDrawOctaHedronPoint(OctaHedronDrawer OcD, MeshDrawer Md, float x, float y, bool is_front);

		public delegate void FnDrawOctaHedronLine(OctaHedronDrawer OcD, MeshDrawer Md, float x0, float y0, float x1, float y1, bool is_front, int to_top);

		public delegate void FnDrawOctaHedronSurface(OctaHedronDrawer OcD, MeshDrawer Md, float x0, float y0, float x1, float y1, float x2, float y2, float level, bool is_under, bool is_front);
	}
}
