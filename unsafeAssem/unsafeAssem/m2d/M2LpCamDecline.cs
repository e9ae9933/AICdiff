using System;
using XX;

namespace m2d
{
	public class M2LpCamDecline : M2LabelPoint
	{
		public M2LpCamDecline(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public M2LpCamDecline(string _key, float _x, float _y, float _w, float _h)
			: base(_key, -1, null)
		{
			this.x = _x;
			base.y = _y;
			base.w = _w;
			base.h = _h;
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			META meta = new META(this.comment);
			int[] array = meta.getDirs("aim", 0, false, 0);
			if (array != null)
			{
				this.aim_bits = 0;
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					this.aim_bits |= 1 << array[0];
				}
			}
			else
			{
				this.aim_bits = 15;
			}
			array = meta.getDirs("pushout", 0, false, 0);
			if (array != null)
			{
				this.pushoutaim_bits = 0;
				int num2 = array.Length;
				for (int j = 0; j < num2; j++)
				{
					this.pushoutaim_bits |= 1 << array[0];
				}
			}
			else
			{
				this.pushoutaim_bits = 15;
			}
			if (this.mapx <= this.Mp.crop + 2)
			{
				this.pushoutaim_bits &= -2;
			}
			if (this.mapx + this.mapw >= this.Mp.clms - this.Mp.crop - 2)
			{
				this.pushoutaim_bits &= -5;
			}
			if (this.mapy <= this.Mp.crop + 2)
			{
				this.pushoutaim_bits &= -3;
			}
			if (this.mapy + this.maph >= this.Mp.rows - this.Mp.crop - 2)
			{
				this.pushoutaim_bits &= -9;
			}
			this.Mp.M2D.Cam.addCropping(this);
		}

		public override void closeAction(bool when_map_close = false)
		{
			if (!when_map_close)
			{
				return;
			}
			this.Mp.M2D.Cam.remCropping(this);
		}

		public bool isPushoutEnable(AIM a)
		{
			return (this.pushoutaim_bits & (1 << (int)a)) != 0;
		}

		public bool isEnable(float depx, float depy, float cl, float ct, float cr, float cb)
		{
			if (!this.isCoveringXy(cl, ct, cr, cb, 0f, -1000f))
			{
				return false;
			}
			if (this.aim_bits != 15)
			{
				if (base.bottom < depy && (this.aim_bits & 2) == 0)
				{
					return false;
				}
				if (base.y > depy && (this.aim_bits & 8) == 0)
				{
					return false;
				}
				if (base.right < depx && (this.aim_bits & 1) == 0)
				{
					return false;
				}
				if (this.x > depx && (this.aim_bits & 4) == 0)
				{
					return false;
				}
			}
			return true;
		}

		public override void deactivate()
		{
			this.active = false;
		}

		public override void activate()
		{
			this.active = true;
		}

		public static M2LpCamDecline createBounds(string header, int i, float drawx, float drawy, float drawr, float drawb, float extend_drawx, float extend_drawy)
		{
			M2LpCamDecline m2LpCamDecline;
			switch (i)
			{
			case 0:
				m2LpCamDecline = new M2LpCamDecline("PUZZAREA_0", drawx - extend_drawx - 896f, drawy - 896f, 896f, drawb - drawy + 1792f);
				break;
			case 1:
				m2LpCamDecline = new M2LpCamDecline("PUZZAREA_1", drawr + extend_drawx, drawy - 896f, 896f, drawb - drawy + 1792f);
				break;
			case 2:
				m2LpCamDecline = new M2LpCamDecline("PUZZAREA_2", drawx - 896f, drawy - extend_drawy - 896f, drawr - drawx + 1792f, 896f);
				break;
			default:
				m2LpCamDecline = new M2LpCamDecline("PUZZAREA_3", drawx - 896f, drawb + extend_drawy, drawr - drawx + 1792f, 896f);
				break;
			}
			m2LpCamDecline.finePos();
			return m2LpCamDecline;
		}

		private int aim_bits = 15;

		private int pushoutaim_bits = 15;
	}
}
