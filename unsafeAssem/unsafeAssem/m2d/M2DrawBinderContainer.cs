using System;
using XX;

namespace m2d
{
	public class M2DrawBinderContainer : RBase<M2DrawBinder>
	{
		public M2DrawBinderContainer(Map2d _Mp, M2SubMap _SubMapData, IEffectSetter _EF, string _key = "_")
			: base(8, false, true, false)
		{
			this.key = _key;
			this.SubMapData = _SubMapData;
			this.ef_name = "_binder_" + this.key;
			this.FD_efDraw = new FnEffectRun(this.efDraw);
			this.SetContainer(_Mp, _SubMapData, _EF);
		}

		public M2DrawBinderContainer SetContainer(Map2d _Mp, M2SubMap _SubMapData, IEffectSetter _EF)
		{
			this.Mp = _Mp;
			this.SubMapData = _SubMapData;
			this.EF = _EF;
			if (this.Ef != null)
			{
				this.setEffect();
			}
			return this;
		}

		public M2DrawBinder Add(string name, M2DrawBinder.FnEffectBind Fn, float saf = 0f)
		{
			if (this.Ef == null || this.Ef.index != this.ef_index)
			{
				this.setEffect();
			}
			return base.Pop(16).Set(name, Fn, saf);
		}

		public void Rem(M2DrawBinder.FnEffectBind Fn)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				M2DrawBinder m2DrawBinder = this.AItems[i];
				if (m2DrawBinder.fnDraw == Fn)
				{
					m2DrawBinder.destruct();
					return;
				}
			}
		}

		public override M2DrawBinder Create()
		{
			uint num = this.create_index;
			this.create_index = num + 1U;
			return new M2DrawBinder(this, num);
		}

		private void removeEffect()
		{
			if (this.Ef != null && this.Ef.index == this.ef_index)
			{
				this.Ef.FnDef = null;
			}
			this.Ef = null;
		}

		private EffectItem setEffect()
		{
			this.removeEffect();
			this.Ef = this.EF.setEffectWithSpecificFn(this.ef_name, 0f, 0f, 0f, 0, 0, this.FD_efDraw);
			if (this.Ef != null)
			{
				this.ef_index = this.Ef.index;
			}
			return this.Ef;
		}

		public override bool run(float fcnt)
		{
			if (this.Ef == null || this.ef_index != this.Ef.index)
			{
				if (this.LEN == 0)
				{
					return false;
				}
				this.setEffect();
			}
			return base.run(fcnt);
		}

		private bool efDraw(EffectItem Ef)
		{
			if (Ef == this.Ef)
			{
				for (int i = 0; i < this.LEN; i++)
				{
					this.AItems[i].efDraw(Ef);
				}
				Ef.index = this.ef_index;
			}
			return true;
		}

		public override void destruct()
		{
			base.destruct();
			this.removeEffect();
		}

		public override string ToString()
		{
			return "<M2DrawBinderContainer>" + this.key + " C:" + this.LEN.ToString();
		}

		public Map2d Mp;

		public M2SubMap SubMapData;

		public IEffectSetter EF;

		public string key;

		public float fcnt;

		private uint create_index;

		private EffectItem Ef;

		private uint ef_index;

		public readonly string ef_name;

		public FnEffectRun FD_efDraw;
	}
}
