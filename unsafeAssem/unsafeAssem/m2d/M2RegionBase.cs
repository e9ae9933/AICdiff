using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2RegionBase
	{
		public M2RegionBase(int _x, int _y, int _index = -1)
		{
			this.x = _x;
			this.y = _y;
			this.index = _index;
			this.r = this.x + 1;
			this.b = this.y + 1;
		}

		public int w
		{
			get
			{
				return this.r - this.x;
			}
		}

		public float cx
		{
			get
			{
				return (float)this.x + (float)this.r * 0.5f;
			}
		}

		public float cy
		{
			get
			{
				return (float)this.y + (float)this.h * 0.5f;
			}
		}

		public int h
		{
			get
			{
				return this.b - this.y;
			}
		}

		public bool isContains(int _x, int _y)
		{
			return X.BTW((float)this.x, (float)_x, (float)this.r) && X.BTW((float)this.y, (float)_y, (float)this.b);
		}

		public bool isContains(float _x, float _y, float marginx, float marginy)
		{
			return X.BTW((float)this.x - marginx, _x, (float)this.r + marginx) && X.BTW((float)this.y - marginy, _y, (float)this.b + marginy);
		}

		public bool isOnOver(M2RegionBase WR)
		{
			return this.x < WR.r && this.r > WR.x && this.y < WR.b && this.b > WR.y;
		}

		public bool isSame(M2RegionBase WR)
		{
			return this.x == WR.x && this.y == WR.y && this.r == WR.r && this.b == WR.b;
		}

		public Rect getCoveringArea(M2RegionBase WR)
		{
			int num = X.Mx(this.x, WR.x);
			int num2 = X.Mx(this.y, WR.y);
			Rect rect = new Rect((float)num, (float)num2, 0f, 0f);
			int num3 = X.Mn(this.r, WR.r);
			int num4 = X.Mn(this.b, WR.b);
			rect.width = (float)(num3 - num);
			rect.height = (float)(num4 - num2);
			return rect;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"M2RB: ",
				this.x.ToString(),
				", ",
				this.y.ToString(),
				", R",
				this.r.ToString(),
				", B",
				this.b.ToString()
			});
		}

		public static M2RegionBase WR_cov_buf;

		public int x;

		public int y;

		public int r;

		public int b;

		public int index;
	}
}
