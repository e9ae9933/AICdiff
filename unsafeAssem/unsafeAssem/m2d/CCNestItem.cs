using System;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class CCNestItem
	{
		public CCNestItem(M2MvColliderCreator _Con, string _name)
		{
			this.Con = _Con;
			this.name = _name;
		}

		public CCNestItem MapShift(float _x, float _y)
		{
			this.shiftx = _x;
			this.shifty = _y;
			if (this.Con != null)
			{
				this.Con.need_recreate = true;
			}
			return this;
		}

		public CCNestItem MapShiftAbs(M2Mover MvBase, float _x, float _y)
		{
			return this.MapShift(_x - MvBase.x, _y - MvBase.y);
		}

		public CCNestItem MapShiftMul(float _x, float _y, float mul_speed)
		{
			this.shiftx = X.MULWALK(this.shiftx, _x, mul_speed);
			this.shifty = X.MULWALK(this.shifty, _y, mul_speed);
			if (this.Con != null)
			{
				this.Con.need_recreate = true;
			}
			return this;
		}

		public CCNestItem Size(float _x, float _y)
		{
			this.sizex = _x;
			this.sizey = _y;
			if (this.Con != null)
			{
				this.Con.need_recreate = true;
			}
			return this;
		}

		public Vector2 getUShift(Map2d Mp)
		{
			return new Vector2(this.shiftx * Mp.CLENB * 0.015625f, -this.shifty * Mp.CLENB * 0.015625f);
		}

		public Vector2 getUSize(Map2d Mp)
		{
			return new Vector2(this.sizex * 2f * Mp.CLENB * 0.015625f, this.sizey * 2f * Mp.CLENB * 0.015625f);
		}

		public M2MvColliderCreator Con;

		public readonly string name;

		public float shiftx;

		public float shifty;

		public float sizex;

		public float sizey;

		public float rotR;
	}
}
