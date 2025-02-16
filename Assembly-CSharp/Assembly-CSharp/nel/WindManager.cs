using System;
using m2d;
using XX;

namespace nel
{
	public class WindManager : RBase<WindItem>, IRunAndDestroy
	{
		public override WindItem Create()
		{
			return new WindItem();
		}

		public WindManager(NelM2DBase _M2D)
			: base(4, false, false, true)
		{
			this.M2D = _M2D;
		}

		public override void clear()
		{
			base.clear();
			this.id_count = 0U;
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			if (this.Mp != null)
			{
				this.Mp.remRunnerObject(this);
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.M2D.curMap;
			}
		}

		public WindHandlerItem Add(float sx, float sy, float radius, float agR, float max_lgt, float _apply_velocity, float maxt = -1f, float _near_multiple = 1f)
		{
			WindItem windItem = base.Pop(6);
			uint num = this.id_count + 1U;
			this.id_count = num;
			WindItem windItem2 = windItem.Set(num, sx, sy, radius, agR, max_lgt, _apply_velocity, maxt, _near_multiple);
			if (this.LEN == 1)
			{
				this.Mp.addRunnerObject(this);
				if (this.FD_drawWind == null)
				{
					this.FD_drawWind = new M2DrawBinder.FnEffectBind(this.drawWind);
				}
				if (this.Ed == null)
				{
					this.Ed = this.Mp.setED("WindManager", this.FD_drawWind, 0f);
				}
			}
			return new WindHandlerItem(windItem2);
		}

		public override bool run(float fcnt)
		{
			return this.LEN != 0 && base.run(fcnt);
		}

		public void recheckLength()
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				this.AItems[i].need_fine_length = true;
			}
		}

		private bool drawWind(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.LEN == 0)
			{
				if (Ed == this.Ed)
				{
					this.Ed = null;
				}
				return false;
			}
			MeshDrawer meshDrawer = null;
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItems[i].draw(Ef, ref meshDrawer);
			}
			return true;
		}

		public override string ToString()
		{
			return "WindManager";
		}

		public readonly NelM2DBase M2D;

		public const int FINE_INTV = 8;

		private uint id_count;

		private M2DrawBinder Ed;

		private M2DrawBinder.FnEffectBind FD_drawWind;
	}
}
