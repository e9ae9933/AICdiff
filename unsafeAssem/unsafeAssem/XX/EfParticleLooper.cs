using System;

namespace XX
{
	public class EfParticleLooper
	{
		public EfParticleLooper(string _key)
		{
			this.key = _key;
		}

		public string particle_key
		{
			get
			{
				return this.key;
			}
			set
			{
				if (this.key == value)
				{
					return;
				}
				this.key = value;
				this.cur_reload_count = -1;
			}
		}

		public void checkReload()
		{
			if (this.cur_reload_count < EfParticleManager.reload_count)
			{
				this.cur_reload_count = EfParticleManager.reload_count;
				this.Ptc = EfParticle.Get(this.key, false);
				if (this.Ptc != null && (this.xdf_size >= 0f || this.ydf_size >= 0f))
				{
					this.Ptc = this.Ptc.clone();
				}
			}
		}

		public virtual bool Draw(EffectItem Ef, float loop_maxt = -1f)
		{
			this.checkReload();
			if (this.Ptc == null)
			{
				return false;
			}
			if (this.xdf_size >= 0f)
			{
				this.Ptc.xdf_dif = this.xdf_size;
			}
			if (this.ydf_size >= 0f)
			{
				this.Ptc.ydf_dif = this.ydf_size;
			}
			if (loop_maxt == -1f)
			{
				loop_maxt = X.Mx((float)this.Ptc.maxt, this.Ptc.delay * (float)this.Ptc.count);
			}
			return this.Ptc.drawMain(Ef, loop_maxt, 0f, false);
		}

		public int maxt
		{
			get
			{
				this.checkReload();
				if (this.Ptc == null)
				{
					return 1;
				}
				return this.Ptc.maxt;
			}
		}

		public float delay
		{
			get
			{
				this.checkReload();
				if (this.Ptc == null)
				{
					return 0f;
				}
				return this.Ptc.delay;
			}
		}

		public float total_delay
		{
			get
			{
				this.checkReload();
				if (this.Ptc == null)
				{
					return 0f;
				}
				return X.Mx(this.Ptc.delay * (float)this.Ptc.count, (float)this.Ptc.maxt);
			}
		}

		public float sum_delay
		{
			get
			{
				this.checkReload();
				if (this.Ptc == null)
				{
					return 0f;
				}
				return this.Ptc.delay * (float)this.Ptc.count;
			}
		}

		public float count
		{
			get
			{
				this.checkReload();
				return (float)((this.Ptc != null) ? this.Ptc.count : 0);
			}
		}

		public float bounds_wh
		{
			get
			{
				this.checkReload();
				if (this.Ptc == null)
				{
					return 0f;
				}
				return this.Ptc.bounds_wh;
			}
		}

		private string key;

		public float xdf_size = -1f;

		public float ydf_size = -1f;

		private EfParticle Ptc;

		private int cur_reload_count = -1;
	}
}
