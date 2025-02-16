using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2Rect : DRect
	{
		public M2Rect(string _key, int _index, Map2d _Mp)
			: base(_key, 0f, 0f, 0f, 0f, 0f)
		{
			this.Mp = _Mp;
			this.index = _index;
			this.finePos();
		}

		public override DRect finePos()
		{
			if (base.w <= 0f || base.h <= 0f)
			{
				base.w = (base.h = 0f);
			}
			if (this.Mp == null)
			{
				return this;
			}
			this.mapx = X.Int(this.x / this.Mp.CLEN);
			this.mapy = X.Int(base.y / this.Mp.CLEN);
			this.mapw = X.IntC(base.right / this.Mp.CLEN) - this.mapx;
			this.maph = X.IntC(base.bottom / this.Mp.CLEN) - this.mapy;
			return this;
		}

		public M2Rect multiple(float lv)
		{
			this.x *= lv;
			base.y *= lv;
			base.w *= lv;
			base.h *= lv;
			this.finePos();
			return this;
		}

		public float mapfocx
		{
			get
			{
				if (this.focx != -1f)
				{
					return this.x / this.Mp.CLEN + (this.focx + 0.5f);
				}
				return (this.x + base.w / 2f) / this.Mp.CLEN;
			}
		}

		public float mapfocy
		{
			get
			{
				if (this.focy != -1f)
				{
					return base.y / this.Mp.CLEN + (this.focy + 0.5f);
				}
				return (base.y + base.h / 2f) / this.Mp.CLEN;
			}
		}

		public M2Rect setFoc(Vector2 Pos)
		{
			this.focx = Pos.x;
			this.focy = Pos.y;
			return this;
		}

		public bool isSame(M2Rect Rc)
		{
			return this.x == Rc.x && base.y == Rc.y && this.width == Rc.width && this.height == Rc.height && this.focx == Rc.focx && this.focy == Rc.focy;
		}

		public Vector2 getWalkable(M2Mover Mv)
		{
			return this.getWalkable(Mv, base.getRandom() * this.Mp.rCLEN);
		}

		public Vector2 getWalkable(M2Mover Mv, Vector2 StartPt)
		{
			Vector2 vector = StartPt;
			float num = 0f;
			float num2 = X.XORSP() * 6.2831855f;
			int num3 = 150;
			float x = vector.x;
			float y = vector.y;
			while (--num3 >= 0)
			{
				if (this.isinMapP(vector, 0f) && Mv.canStand((int)vector.x, (int)vector.y))
				{
					return vector;
				}
				num2 += (50f + 50f * X.XORSP()) / 180f * 3.1415927f;
				num += 0.37f + 0.45f * X.XORSP();
				vector.x = x + num * X.Cos(num2);
				vector.y = y + num * X.Sin(num2);
			}
			return base.getCenter() * this.Mp.rCLEN;
		}

		public Vector2 getWalkable(Map2d Mp, float cx, float cy)
		{
			return M2Rect.getWalkableS(Mp, cx, cy, this.x * Mp.rCLEN, base.y * Mp.rCLEN, base.right * Mp.rCLEN, base.bottom * Mp.rCLEN, false, 0f, 0f);
		}

		public static Vector2 getWalkableS(Map2d Mp, float cx, float cy, float mapx, float mapy, float mapr, float mapb, bool first_random = false, float clipx = 0f, float clipy = 0f)
		{
			Vector2 vector = new Vector2(cx, cy);
			float num = 0f;
			float num2 = X.XORSP() * 6.2831855f;
			int num3 = 150;
			if (first_random)
			{
				vector.x = X.NIXP(mapx + clipx, mapr - clipx);
				vector.y = X.NIXP(mapy + clipy, mapb - clipy);
				num3 = 300;
			}
			while (--num3 >= 0)
			{
				if (X.BTW(mapx + clipx, vector.x, mapr - clipx) && X.BTW(mapy + clipy, vector.y, mapb - clipy))
				{
					bool flag = false;
					int num4 = (int)vector.x;
					int num5 = (int)vector.y;
					int config = Mp.getConfig(num4, num5);
					if (CCON.canStand(config))
					{
						flag = true;
						if (!Mp.canStand(num4, num5 - 1))
						{
							flag = false;
						}
						if (CCON.isSlope(config) && ((float)num5 < mapy + 2f || !Mp.canStand(num4, num5 - 2)))
						{
							flag = false;
						}
					}
					if (flag)
					{
						return vector;
					}
				}
				num2 += (50f + 50f * X.XORSP()) / 180f * 3.1415927f;
				num += 0.37f + 0.45f * X.XORSP();
				vector.x = cx + num * X.Cos(num2);
				vector.y = cy + num * X.Sin(num2);
			}
			vector.x = (mapx + mapr) * 0.5f;
			vector.y = (mapy + mapb) * 0.5f;
			return vector;
		}

		public bool SetFromField(string str, string n)
		{
			int num;
			int num2;
			if (REG.match(str, REG.RegPosition))
			{
				num = X.NmI(REG.R1, 0, false, false);
				num2 = X.NmI(REG.R2, 0, false, false);
			}
			else
			{
				if (!REG.match(str, REG.RegOneNumber))
				{
					return false;
				}
				num = X.NmI(REG.R1, 0, false, false);
				num2 = num;
			}
			if (n == "drawxy")
			{
				this.x = (float)num;
				base.y = (float)num2;
			}
			if (n == "drawwh")
			{
				this.width = (float)num;
				this.height = (float)num2;
			}
			if (n == "mapxy")
			{
				if (this.mapx != num)
				{
					this.x = (float)num * this.Mp.CLEN;
				}
				if (this.mapy != num2)
				{
					base.y = (float)num2 * this.Mp.CLEN;
				}
			}
			if (n == "mapwh")
			{
				if (this.mapw != num)
				{
					base.w = (float)num * this.Mp.CLEN;
				}
				if (this.maph != num2)
				{
					base.h = (float)num2 * this.Mp.CLEN;
				}
			}
			if (n == "foc")
			{
				this.focx = (float)num;
				this.focy = (float)num2;
			}
			this.finePos();
			return true;
		}

		public bool isContainingMapXy(float _x, float _y, float _r, float _b, float _extend_px = 0f)
		{
			return this.isContainingXy(_x * this.CLEN, _y * this.CLEN, _r * this.CLEN, _b * this.CLEN, _extend_px);
		}

		public bool isConveringMapXy(float _x, float _y, float _r, float _b)
		{
			return this.isCoveringXy(_x * this.CLEN, _y * this.CLEN, _r * this.CLEN, _b * this.CLEN, 0f, -1000f);
		}

		public bool isinMapP(Vector2 P, float _extend_pixel = 0f)
		{
			return base.isinP(P * this.CLEN, _extend_pixel);
		}

		public bool isinMapP(float x, float y, float _extend_mapx, float _extend_mapy)
		{
			return base.isin(x * this.CLEN, y * this.CLEN, _extend_mapx * this.CLEN, _extend_mapy * this.CLEN);
		}

		public float mapcx
		{
			get
			{
				return (float)this.mapx + (float)this.mapw * 0.5f;
			}
		}

		public float mapcy
		{
			get
			{
				return (float)this.mapy + (float)this.maph * 0.5f;
			}
		}

		public bool destructed
		{
			get
			{
				return this.index < 0;
			}
		}

		public float CLEN
		{
			get
			{
				return this.Mp.CLEN;
			}
		}

		public float CLENB
		{
			get
			{
				return this.Mp.CLENB;
			}
		}

		public static void readFromDta(string[] Dta, M2Rect LPI, int shift_drawx = 0, int shift_drawy = 0)
		{
			LPI.Set(X.Nm(Dta[1], 0f, false) + (float)shift_drawx, X.Nm(Dta[2], 0f, false) + (float)shift_drawy, X.Nm(Dta[3], 0f, false), X.Nm(Dta[4], 0f, false));
		}

		public readonly Map2d Mp;

		public int index;

		public float focx = -1f;

		public float focy = -1f;

		public int mapx;

		public int mapy;

		public int mapw;

		public int maph;
	}
}
