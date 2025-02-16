using System;
using m2d;
using XX;

namespace nel
{
	public abstract class M2LpDrawer : M2LabelPoint
	{
		public M2LpDrawer(string __key, int _i, M2MapLayer L)
			: base(__key, 0, L)
		{
		}

		public override void initAction(bool normal_map)
		{
			this.closeAction(false);
			Map2d mp = this.Lay.Mp;
			this.Meta = new META(this.comment);
			if (mp.Dgn.is_editor)
			{
				return;
			}
			if (this.initMain())
			{
				this.ef_z = this.Meta.GetNm("z", this.ef_z, 0);
				this.ef_time = this.Meta.GetI("time", this.ef_time, 0);
				if (this.set_to_edc)
				{
					this.Ed = mp.setEDC(base.unique_key, new M2DrawBinder.FnEffectBind(this.fnDrawLpDrawer), (float)this.saf);
					return;
				}
				this.Ed = mp.setED(base.unique_key, new M2DrawBinder.FnEffectBind(this.fnDrawLpDrawer), (float)this.saf);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			if (this.Ed != null)
			{
				this.Lay.Mp.remED(this.Ed);
				this.Ed = null;
			}
		}

		private bool fnDrawLpDrawer(EffectItem Ef, M2DrawBinder Ed)
		{
			float bounds_wh = this.bounds_wh;
			Ef.x = base.mapcx;
			Ef.y = base.mapcy;
			if (!Ed.isinCamera(Ef, bounds_wh, bounds_wh))
			{
				return true;
			}
			Ef.z = this.ef_z;
			Ef.time = this.ef_time;
			return this.drawMain(Ef, Ed);
		}

		protected abstract bool initMain();

		protected abstract bool drawMain(EffectItem Ef, M2DrawBinder Ed);

		public virtual float bounds_wh
		{
			get
			{
				return X.Mx(this.width, this.height) + 70f;
			}
		}

		protected M2DrawBinder Ed;

		protected META Meta;

		public float ef_z;

		public int ef_time;

		public int saf;

		public bool set_to_edc = true;
	}
}
