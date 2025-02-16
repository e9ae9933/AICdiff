using System;
using XX;

namespace m2d
{
	public class M2Light
	{
		public M2Light(Map2d _Mp, M2Mover _FollowMv)
		{
			this.Mp = _Mp;
			this.FollowMv = _FollowMv;
			this.Col = new C32(2550136831U);
			if (this.FollowMv != null)
			{
				this.mapx = this.FollowMv.x;
				this.mapy = this.FollowMv.y;
				this.alpha_rgb = this.Mp.Dgn.multiply_mv_light_alpha;
			}
		}

		public M2Light Pos(float mapx, float mapy)
		{
			this.mapx = mapx;
			this.mapy = mapy;
			return this;
		}

		public virtual void drawLight(MeshDrawer Md, M2SubMap Sm, float alpha, float fcnt)
		{
			if (this.FollowMv != null)
			{
				if (this.Mp.isJustOpened)
				{
					this.mapx = this.FollowMv.x;
					this.mapy = this.FollowMv.y;
				}
				else
				{
					this.mapx = X.MULWALKF(this.mapx, this.FollowMv.x, this.follow_speed, fcnt);
					this.mapy = X.MULWALKF(this.mapy, this.FollowMv.y, this.follow_speed, fcnt);
				}
			}
			float num;
			float num2;
			this.calcDrawMeshPos(out num, out num2);
			if (this.showing_delay >= 0)
			{
				if (Sm == null && !this.isinCamera(num, num2))
				{
					if (this.showing_delay == 0)
					{
						return;
					}
					if (this.showing_t >= 0)
					{
						this.showing_t = X.Mn(-1, -this.showing_delay + this.showing_t);
					}
					int num3 = this.showing_t - 1;
					this.showing_t = num3;
					if (num3 <= -this.showing_delay)
					{
						return;
					}
					alpha *= X.ZLINE((float)(this.showing_delay + this.showing_t + 1), (float)this.showing_delay);
				}
				else
				{
					if (this.showing_t < 0)
					{
						this.showing_t = X.Mx(0, this.showing_delay + this.showing_t);
					}
					if (this.showing_t < this.showing_delay)
					{
						float num4 = alpha;
						int num3 = this.showing_t + 1;
						this.showing_t = num3;
						alpha = num4 * X.ZLINE((float)num3, (float)this.showing_delay);
					}
				}
			}
			this.drawLightAt(Md, num, num2, 1f, alpha);
		}

		public virtual bool isinCamera(float meshx, float meshy)
		{
			float num = (this.radius + this.fill_radius) * this.Mp.base_scale * this.Mp.rCLEN;
			return this.Mp.M2D.Cam.isCoveringCenMeshPixel(meshx, meshy, num * 2f, num * 2f, 40f);
		}

		protected virtual void calcDrawMeshPos(out float x, out float y)
		{
			x = this.Mp.map2meshx(this.mapx);
			y = this.Mp.map2meshy(this.mapy);
		}

		public virtual void drawLightAt(MeshDrawer Md, float x, float y, float scale, float alpha = 1f)
		{
			Md.ColGrd.Set(this.Col).mulA(alpha);
			Md.ColGrd.multiply(this.alpha_rgb, false);
			Md.Col = Md.ColGrd.C;
			Md.initForImg(MTRX.EffBlurCircle245, 0);
			float num = this.radius + this.fill_radius;
			Md.Rect(x, y, scale * num * 2f, scale * num * 2f, false);
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				this._tostring = "Light -" + ((this.FollowMv != null) ? ("==>" + this.FollowMv.key) : (" @" + this.mapx.ToString() + "," + this.mapy.ToString()));
			}
			return this._tostring;
		}

		public readonly Map2d Mp;

		protected string _tostring;

		public float mapx;

		public float mapy;

		public float radius = 60f;

		public float fill_radius;

		public float follow_speed = 1f;

		public int showing_delay = 10;

		public int showing_t;

		public M2Mover FollowMv;

		public float alpha_rgb = 1f;

		public C32 Col;
	}
}
