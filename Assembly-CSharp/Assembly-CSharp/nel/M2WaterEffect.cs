using System;
using m2d;
using XX;

namespace nel
{
	public sealed class M2WaterEffect : RBase<WaterEffectItem>
	{
		public M2WaterEffect(M2WaterWatcher _WMng)
			: base(4, false, false, false)
		{
			this.WMng = _WMng;
			this.Mp = this.WMng.Mp;
			this.Ed = this.Mp.setED("WaterEffect", new M2DrawBinder.FnEffectBind(this.fnDraw), 0f);
		}

		public override void destruct()
		{
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			base.destruct();
		}

		public override WaterEffectItem Create()
		{
			return new WaterEffectItem();
		}

		public WaterEffectItem Get(WaterEffectItem.TYPE _type, float x, float y)
		{
			uint num = Map2d.xy2b((int)x, (int)y);
			for (int i = 0; i < this.LEN; i++)
			{
				WaterEffectItem waterEffectItem = this.AItems[i];
				if (waterEffectItem.type == _type && waterEffectItem.xybit == num)
				{
					return waterEffectItem;
				}
			}
			return null;
		}

		public WaterEffectItem Make(WaterEffectItem.TYPE _type, float x, float y, float z, int time)
		{
			WaterEffectItem waterEffectItem = this.Get(_type, x, y);
			if (waterEffectItem == null)
			{
				waterEffectItem = base.Pop(16).Init(this.Mp, _type, x, y, z, time);
			}
			else
			{
				waterEffectItem.makeEffect(x, y, z, time);
			}
			return waterEffectItem;
		}

		private bool fnDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.Ed == null)
			{
				return false;
			}
			if (this.LEN == 0)
			{
				return true;
			}
			WaterEffectItem.Ef = Ef;
			base.run((float)X.AF_EF);
			WaterEffectItem.Ef = null;
			return true;
		}

		public readonly M2WaterWatcher WMng;

		public readonly Map2d Mp;

		public M2DrawBinder Ed;
	}
}
