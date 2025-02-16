using System;
using UnityEngine;

namespace XX
{
	public sealed class EfParticleOnce : EffectItem
	{
		public void shuffle()
		{
			this.index = X.xors();
		}

		public EfParticleOnce(string _key, EFCON_TYPE _ef_type = EFCON_TYPE.FIXED)
			: base(null, "", 0f, 0f, 0f, 0, 0)
		{
			this.key_ = _key;
			this.ef_type = _ef_type;
			this.shuffle();
			this.checkReload();
		}

		public string key
		{
			get
			{
				return this.key_;
			}
			set
			{
				if (this.key_ != value)
				{
					this.cur_reload_count = -1;
					this.Ptc = null;
					this.key_ = value;
				}
			}
		}

		public void checkReload()
		{
			if (TX.valid(this.key_) && this.cur_reload_count < EfParticleManager.reload_count)
			{
				this.cur_reload_count = EfParticleManager.reload_count;
				this.Ptc = EfParticle.Get(this.key, false);
			}
		}

		public override int getParticleCount(EffectItem Ef, int default_cnt)
		{
			if (default_cnt <= 2)
			{
				return default_cnt;
			}
			return X.IntR(X.Mx(2f, (float)default_cnt * ((this.ef_type == EFCON_TYPE.FIXED) ? 1f : ((this.ef_type == EFCON_TYPE.NORMAL) ? X.EF_LEVEL_NORMAL : X.EF_LEVEL_UI))));
		}

		public override float getParticleSpeed(EffectItem Ef, int default_cnt, float default_maxt)
		{
			if (this.ef_type == EFCON_TYPE.FIXED)
			{
				return 1f;
			}
			float num = 1f;
			if (default_cnt <= 2)
			{
				num *= ((this.ef_type == EFCON_TYPE.NORMAL) ? X.EF_LEVEL_NORMAL : X.EF_LEVEL_UI);
			}
			return num * ((this.ef_type == EFCON_TYPE.NORMAL) ? 1f : X.EF_TIMESCALE_UI);
		}

		public int myAf
		{
			get
			{
				return IN.totalframe - this.f0;
			}
		}

		public bool drawTo(MeshDrawer Md, float x, float y, float z, int t, float af, float looptime = 0f)
		{
			this.z = z;
			this.time = t;
			return this.drawTo(Md, x, y, af, looptime);
		}

		public bool drawTo(MeshDrawer Md, float x, float y, float af = 0f, float looptime = 0f)
		{
			float base_x = Md.base_x;
			float base_y = Md.base_y;
			Md.base_x = x * 0.015625f;
			Md.base_y = y * 0.015625f;
			bool flag = this.drawTo(Md, af, looptime);
			Md.base_x = base_x;
			Md.base_y = base_y;
			if (!flag)
			{
				this.shuffle();
			}
			return flag;
		}

		public bool drawTo(MeshDrawer Md, float af, float looptime = 0f)
		{
			this.checkReload();
			if (this.Ptc == null)
			{
				return false;
			}
			EfParticleOnce.CurMd = Md;
			this.af = af;
			bool flag = this.Ptc.drawMain(this, looptime, 0f, this.not_mesh_image_replace);
			EfParticleOnce.CurMd = null;
			return flag;
		}

		public void restart()
		{
			this.af = 0f;
			this.f0 = IN.totalframe;
			this.shuffle();
		}

		public float maxt_one
		{
			get
			{
				this.checkReload();
				return (float)this.Ptc.maxt;
			}
		}

		public int count
		{
			get
			{
				this.checkReload();
				return this.Ptc.count;
			}
		}

		public float loop_maxt
		{
			get
			{
				this.checkReload();
				return (float)this.Ptc.maxt + this.Ptc.delay * (float)this.Ptc.count;
			}
		}

		public float loop_time
		{
			get
			{
				this.checkReload();
				return this.Ptc.delay * (float)this.Ptc.count;
			}
		}

		public float delay
		{
			get
			{
				this.checkReload();
				return this.Ptc.delay;
			}
		}

		public override MeshDrawer GetMesh(string title, Material Mtr, bool bottom_flag = false)
		{
			return EfParticleOnce.CurMd;
		}

		public override MeshDrawer GetMesh(string title, uint col, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			EfParticleOnce.CurMd.Col = C32.d2c(col);
			return EfParticleOnce.CurMd;
		}

		public override MeshDrawer GetMeshGradation(string title, uint col, uint col2, GRD grd, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			EfParticleOnce.CurMd.Col = C32.d2c(col);
			return EfParticleOnce.CurMd;
		}

		public override MeshDrawer GetMeshImg(string title, MImage MI, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			return EfParticleOnce.CurMd;
		}

		public static MeshDrawer CurMd;

		public EFCON_TYPE ef_type = EFCON_TYPE.FIXED;

		private int cur_reload_count = -1;

		private EfParticle Ptc;

		public bool not_mesh_image_replace;

		private string key_;
	}
}
