using System;
using XX;

namespace m2d
{
	public class M2LpWalkDecline : M2LabelPoint
	{
		public M2LpWalkDecline(string __key, int _i, M2MapLayer L)
			: base(__key, _i, L)
		{
		}

		public M2LpWalkDecline(string _key, float _x, float _y, float _w, float _h, M2MapLayer L)
			: base(_key, -1, L)
		{
			this.x = _x;
			base.y = _y;
			base.w = _w;
			base.h = _h;
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.pre_on_ = 2;
			this.Meta = new META(this.comment);
			string s = this.Meta.GetS("config");
			this.config = (TX.valid(s) ? CCON.getStringToValue(s) : 128);
		}

		public override void initAction(bool normal_map)
		{
		}

		private bool pre_on
		{
			get
			{
				if (this.pre_on_ == 2)
				{
					if (this.Meta == null)
					{
						this.Meta = new META(this.comment);
					}
					this.pre_on_ = ((this.Meta.GetNm("pre_on", 1f, 0) != 0f) ? 1 : 0);
				}
				return this.pre_on_ > 0;
			}
		}

		public override void considerConfig4(int _l, int _t, int _r, int _b, M2Pt[,] AAconfig)
		{
			if (this.activated != this.pre_on)
			{
				_l = X.Mx(0, X.Mx(_l, this.mapx));
				_r = X.Mn(this.Mp.clms, X.Mn(_r, this.mapx + this.mapw));
				_t = X.Mx(0, X.Mx(_t, this.mapy));
				_b = X.Mn(this.Mp.rows, X.Mn(_b, this.mapy + this.maph));
				for (int i = _l; i < _r; i++)
				{
					for (int j = _t; j < _b; j++)
					{
						CCON.calcConfigManual(ref AAconfig[i, j], this.config);
					}
				}
			}
		}

		public override void activate()
		{
			if (!this.activated)
			{
				base.activate();
				this.activated = true;
				this.Mp.considerConfig4(this);
				this.Mp.need_update_collider = true;
			}
		}

		public override void deactivate()
		{
			if (this.activated)
			{
				base.deactivate();
				this.activated = false;
				this.Mp.considerConfig4(this);
				this.Mp.need_update_collider = true;
			}
		}

		public override void closeAction(bool when_map_close)
		{
		}

		public static M2LpWalkDecline createBounds(string header, int i, float drawx, float drawy, float drawr, float drawb, float extend_drawx, float extend_drawy, M2MapLayer Lay)
		{
			M2LpWalkDecline m2LpWalkDecline;
			switch (i)
			{
			case 0:
				m2LpWalkDecline = new M2LpWalkDecline(header + "0", drawx - extend_drawx - 896f, drawy - 896f, 896f, drawb - drawy + 1792f, Lay);
				break;
			case 1:
				m2LpWalkDecline = new M2LpWalkDecline(header + "1", drawr + extend_drawx, drawy - 896f, 896f, drawb - drawy + 1792f, Lay);
				break;
			case 2:
				m2LpWalkDecline = new M2LpWalkDecline(header + "2", drawx - 896f, drawy - extend_drawy - 896f, drawr - drawx + 1792f, 896f, Lay);
				break;
			default:
				m2LpWalkDecline = new M2LpWalkDecline(header + "3", drawx - 896f, drawb + extend_drawy, drawr - drawx + 1792f, 896f, Lay);
				break;
			}
			m2LpWalkDecline.finePos();
			return m2LpWalkDecline;
		}

		private int config = 128;

		private bool activated;

		private byte pre_on_ = 2;

		private META Meta;
	}
}
