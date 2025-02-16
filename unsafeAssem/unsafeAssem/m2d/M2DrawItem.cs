using System;
using XX;

namespace m2d
{
	public class M2DrawItem
	{
		public M2DrawItem(Map2d _Mp)
		{
			this.Mp = _Mp;
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

		public float rCLEN
		{
			get
			{
				return this.Mp.rCLEN;
			}
		}

		public float rCLENB
		{
			get
			{
				return this.Mp.rCLENB;
			}
		}

		public M2DBase M2D
		{
			get
			{
				return this.Mp.M2D;
			}
		}

		public virtual bool draw(MeshDrawer Md, int fcnt = 1, float sx = 0f, float sy = 0f, bool force_use_atlas = false)
		{
			return false;
		}

		public virtual bool isOnCamera(float camx, float camy, float camw, float camh)
		{
			return X.BTW(-50f, this.x + camx, camw + 50f) && X.BTW(-50f, this.y + camy, camh + 50f);
		}

		public virtual bool isWithin(float _x, float _y, int drawer_index, bool strict)
		{
			return false;
		}

		public virtual float mleft
		{
			get
			{
				return this.x - 0.15f;
			}
		}

		public virtual float mright
		{
			get
			{
				return this.x + 0.15f;
			}
		}

		public virtual float mtop
		{
			get
			{
				return this.y - 0.15f;
			}
		}

		public virtual float mbottom
		{
			get
			{
				return this.y + 0.15f;
			}
		}

		public void drawRect(MeshDrawer Md, int camx, int camy, uint col = 16711680U)
		{
			Md.Col = C32.d2c(col);
			Md.Box((float)camx + this.mleft * this.CLEN, (float)camy + this.mtop * this.CLEN, (float)camx + this.mright * this.CLEN, (float)camy + this.mbottom * this.CLEN, 0f, false);
		}

		public virtual void drawForDebugFocused(MeshDrawer Md, int camx, int camy)
		{
		}

		public virtual string getDebugString(bool get_pre = true, bool get_post = true)
		{
			return "";
		}

		public float get_x()
		{
			return this.x;
		}

		public float get_y()
		{
			return this.y;
		}

		public float get_carry_vx()
		{
			return 0f;
		}

		public float get_carry_vy()
		{
			return 0f;
		}

		public Map2d Mp;

		public float x;

		public float y;
	}
}
