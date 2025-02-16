using System;
using UnityEngine;

namespace XX
{
	public class DRect : IIdvName
	{
		public float y
		{
			get
			{
				return this.y_;
			}
			set
			{
				if (this.y_ == value)
				{
					return;
				}
				this.y_ = value;
				if (this.y_ == 56f)
				{
					this.y_ = 56f;
				}
			}
		}

		public float right
		{
			get
			{
				return this.x + this.width;
			}
			set
			{
				this.width = value - this.x;
			}
		}

		public float bottom
		{
			get
			{
				return this.y + this.height;
			}
			set
			{
				this.height = value - this.y;
			}
		}

		public float left
		{
			get
			{
				return this.x;
			}
			set
			{
				this.x = value;
			}
		}

		public float top
		{
			get
			{
				return this.y;
			}
			set
			{
				this.y = value;
			}
		}

		public string get_individual_key()
		{
			return this.key;
		}

		public DRect(string _key)
		{
			this.key = _key;
		}

		public DRect(string _key, float a, float b = 0f, float _w = 0f, float _h = 0f, float _e = 0f)
		{
			this.key = _key;
			this.extend_pixel = _e;
			this.Set(a, b, _w, _h);
		}

		public DRect(string _key, DRect Rc)
		{
			this.key = _key;
			this.extend_pixel = Rc.extend_pixel;
			this.Set(Rc);
		}

		public DRect Set(float a, float b = 0f, float _w = 0f, float _h = 0f)
		{
			if (_w < 0f)
			{
				a += _w;
				_w = -_w;
			}
			if (_h < 0f)
			{
				b += _h;
				_h = -_h;
			}
			this.x = a;
			this.y = b;
			this.width = _w;
			this.height = _h;
			return this.finePos();
		}

		public DRect SetXy(Vector2 Rc)
		{
			this.x = Rc.x;
			this.y = Rc.y;
			return this.finePos();
		}

		public DRect Set(Rect Rc)
		{
			return this.Set(Rc.x, Rc.y, Rc.width, Rc.height);
		}

		public DRect Set(DRect Rc)
		{
			return this.Set(Rc.x, Rc.y, Rc.width, Rc.height);
		}

		public virtual DRect finePos()
		{
			return this;
		}

		public DRect Clone(DRect E = null)
		{
			E = ((E == null) ? new DRect(this.key) : E);
			E.Set(this);
			E.extend_pixel = this.extend_pixel;
			E.active = this.active;
			return E;
		}

		public float w
		{
			get
			{
				return this.width;
			}
			set
			{
				this.width = value;
			}
		}

		public float h
		{
			get
			{
				return this.height;
			}
			set
			{
				this.height = value;
			}
		}

		public float cy
		{
			get
			{
				return this.y + this.h * 0.5f;
			}
		}

		public float cx
		{
			get
			{
				return this.x + this.w * 0.5f;
			}
		}

		public Vector2 getCenter()
		{
			return new Vector2(this.x + this.w * 0.5f, this.y + this.h * 0.5f);
		}

		public Vector2 getRandom()
		{
			return new Vector2(this.x + X.XORSP() * this.w, this.y + X.XORSP() * this.h);
		}

		public Vector2 getFix(Vector2 Pt)
		{
			return new Vector2((float)X.IntR(Pt.x / 0.5f) * 0.5f, (float)X.IntR(Pt.y / 0.5f) * 0.5f);
		}

		public Vector2 getShift(Vector2 Pt, float shx, float shy)
		{
			return new Vector2(Pt.x + shx, Pt.y + shy);
		}

		public Vector2 getSide(int _sidex = 0, int _sidey = 0)
		{
			return new Vector2(this.left + ((_sidex >= 2) ? (this.width - 1f) : ((_sidex == 1) ? (this.width * 0.5f) : 0f)), this.top + ((_sidey >= 2) ? (this.height - 1f) : ((_sidey == 1) ? (this.height * 0.5f) : 0f)));
		}

		public bool isinP(Vector2 Pt, float _extend = 0f)
		{
			return X.BTW(this.x - _extend, Pt.x, this.x + this.w + _extend) && X.BTW(this.y - _extend, Pt.y, this.y + this.h + _extend);
		}

		public bool isin(float _px, float _py, float _extend = 0f)
		{
			return this.active && X.BTW(this.x - _extend - this.extend_pixel, _px, this.x + this.width + _extend + this.extend_pixel) && X.BTW(this.y - _extend - this.extend_pixel, _py, this.y + this.height + _extend + this.extend_pixel);
		}

		public bool isin(float _px, float _py, float _extendx, float _extendy)
		{
			return this.active && X.BTW(this.x - _extendx - this.extend_pixel, _px, this.x + this.width + _extendx + this.extend_pixel) && X.BTW(this.y - _extendy - this.extend_pixel, _py, this.y + this.height + _extendy + this.extend_pixel);
		}

		public int checkSide(float _px, float _py, float _extend, float extend_outer = 0f)
		{
			int num = 0;
			int num2 = 0;
			if (!this.isin(_px, _py, _extend + extend_outer))
			{
				return -1;
			}
			if (_px < this.x + _extend)
			{
				num = -1;
			}
			else if (_px > this.right - _extend)
			{
				num = 1;
			}
			if (_py < this.y + _extend)
			{
				num2 = 1;
			}
			else if (_py > this.bottom - _extend)
			{
				num2 = -1;
			}
			if (num != 0 || num2 != 0)
			{
				return (int)CAim.get_aim2(0f, 0f, (float)num, (float)num2, false);
			}
			return -1;
		}

		public float getLength(float _px, float _py)
		{
			return X.LENGTHXYS(this.x + this.width * 0.5f, this.y + this.height * 0.5f, _px, _py);
		}

		public float getLength2(float _px, float _py)
		{
			return X.Pow2(this.x + this.width * 0.5f - _px) + X.Pow2(this.y + this.height * 0.5f - _py);
		}

		public float getLengthInContainer(float _px, float _py, float consider_inner_level = 0f)
		{
			float num = X.Mn(this.width, this.height) * 0.5f;
			float num2 = this.x + num;
			float num3 = this.right - num;
			float num4 = this.y + num;
			float num5 = this.bottom - num;
			float num6 = ((_px < num2) ? (num2 - _px) : ((_px > num3) ? (_px - num3) : 0f));
			float num7 = ((_py < num4) ? (num4 - _py) : ((_py > num5) ? (_py - num5) : 0f));
			if (!this.isin(_px, _py, 0f))
			{
				num6 += 128f;
				num7 += 128f;
			}
			else
			{
				if (num6 == 0f)
				{
					if (consider_inner_level >= 1f)
					{
						num6 = 128f;
					}
					else
					{
						float num8 = this.width * 0.5f;
						float num9 = num8 - num;
						num6 = ((num9 != 0f) ? (X.ZLINE(X.Abs(_px - (this.x + num8)), num9) * 128f) : 128f);
					}
				}
				else
				{
					num6 += 128f;
				}
				if (num7 == 0f)
				{
					if (consider_inner_level >= 1f)
					{
						num7 = 128f;
					}
					else
					{
						float num8 = this.height * 0.5f;
						float num9 = num8 - num;
						num7 = ((num9 != 0f) ? (X.ZLINE(X.Abs(_py - (this.y + num8)), num9) * 128f) : 128f);
					}
				}
				else
				{
					num7 += 128f;
				}
			}
			return num6 + num7 - 256f * X.ZLINE(consider_inner_level);
		}

		public void draw(MeshDrawer Md, int thick = 1)
		{
			Md.Box(this.x, this.y, this.x + this.width, this.y + this.height, (float)thick, false);
		}

		public Rect getRect()
		{
			return new Rect(this.x, this.y, this.w, this.h);
		}

		public DRect Expand(float _x, float _y, float _w, float _h, bool do_not_reset = false)
		{
			if (this.isEmpty() && !do_not_reset)
			{
				this.Set(_x, _y, 0f, 0f);
			}
			X.rectExpand(this, _x, _y, _w, _h);
			return this;
		}

		public DRect ExpandRc(DRect Rc, bool do_not_reset = false)
		{
			return this.Expand(Rc.x, Rc.y, Rc.w, Rc.h, do_not_reset);
		}

		public DRect ExpandAim(float len, AIM a, bool x_opposite = false, bool y_opposite = false)
		{
			int num = CAim._XD(a, 1);
			if (num != 0)
			{
				this.width += len;
				if (num < 0 != x_opposite)
				{
					this.x -= len;
				}
			}
			int num2 = CAim._YD(a, 1);
			if (num2 != 0)
			{
				this.height += len;
				if (num2 < 0 != y_opposite)
				{
					this.y -= len;
				}
			}
			return this;
		}

		public bool isCovering(DRect D, float _extend = 0f)
		{
			return X.isCovering(this.x, this.right, D.x, D.right, _extend) && X.isCovering(this.y, this.bottom, D.y, D.bottom, _extend);
		}

		public bool isCovering(Rect D, float _extend = 0f)
		{
			return X.isCovering(this.x, this.right, D.x, D.xMax, _extend) && X.isCovering(this.y, this.bottom, D.y, D.yMax, _extend);
		}

		public bool isContaining(DRect D, float _extend = 0f)
		{
			return X.isContaining(this.x, this.right, D.x, D.right, _extend) && X.isContaining(this.y, this.bottom, D.y, D.bottom, _extend);
		}

		public bool isContaining(Vector2 P, float _extend = 0f)
		{
			return X.isContaining(this.x, this.right, P.x, P.x, _extend) && X.isContaining(this.y, this.bottom, P.y, P.y, _extend);
		}

		public virtual bool isCoveringXy(float _x, float _y, float _r, float _b, float _extendx = 0f, float _extendy = -1000f)
		{
			if (_extendy == -1000f)
			{
				_extendy = _extendx;
			}
			return X.isCovering(this.x, this.right, _x, _r, _extendx) && X.isCovering(this.y, this.bottom, _y, _b, _extendy);
		}

		public bool isCoveringXyR(float _x, float _y, float _r, float _b, float _extend = 0f)
		{
			return X.isCoveringR(this.x, this.right, _x, _r, _extend) && X.isCoveringR(this.y, this.bottom, _y, _b, _extend);
		}

		public virtual bool isContainingXy(float _x, float _y, float _r, float _b, float _extend = 0f)
		{
			return X.isContaining(this.x, this.right, _x, _r, _extend) && X.isContaining(this.y, this.bottom, _y, _b, _extend);
		}

		public bool isEmpty()
		{
			return this.width == 0f || this.height == 0f;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"[",
				this.key,
				"] ",
				this.x.ToString(),
				", ",
				this.y.ToString(),
				", ",
				this.width.ToString(),
				", ",
				this.height.ToString()
			});
		}

		public float x;

		private float y_;

		public float width;

		public float height;

		public string key;

		public float extend_pixel;

		public bool active = true;
	}
}
