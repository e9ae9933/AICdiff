using System;
using m2d;
using XX;

namespace nel
{
	public class M2LpParticleDrawer : M2LpDrawer
	{
		public M2LpParticleDrawer(string __key, int _i, M2MapLayer L)
			: base(__key, _i, L)
		{
		}

		protected override bool initMain()
		{
			EfParticle efParticle = EfParticleManager.Get(this.Meta.GetS("particle_key"), false, false);
			if (efParticle == null)
			{
				return false;
			}
			this.set_to_edc = this.Meta.GetNm("set_to_ed", 0f, 0) == 0f;
			this.PtcL = new EfParticleLooper(efParticle.key);
			this.saf = this.Meta.GetI("saf", -this.loop_time, 0);
			this.loop_time = this.Meta.GetIE("loop_time", efParticle.maxt * 2, 0);
			float nm = this.Meta.GetNm("df_size", -1f, 0);
			float nm2 = this.Meta.GetNm("df_size", nm, 0);
			this.PtcL.xdf_size = ((nm >= 0f) ? (nm * (float)this.mapw * this.Mp.CLEN * 0.5f * (this.set_to_edc ? 1f : 0.5f) / 0.35f) : (-1f));
			this.PtcL.ydf_size = ((nm2 >= 0f) ? (nm2 * (float)this.maph * this.Mp.CLEN * 0.5f * (this.set_to_edc ? 1f : 0.5f) / 0.35f) : (-1f));
			this.pre_on = true;
			string join = this.Meta.GetJoin(" ", "pre_on");
			if (TX.valid(join))
			{
				this.pre_on = TX.eval(join, "") != 0.0;
			}
			return true;
		}

		protected override bool drawMain(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.pre_on)
			{
				this.PtcL.Draw(Ef, (float)this.loop_time);
			}
			return true;
		}

		public override float bounds_wh
		{
			get
			{
				return this.PtcL.bounds_wh + 60f;
			}
		}

		private EfParticleLooper PtcL;

		private int loop_time;

		private bool pre_on = true;

		private bool active_flag;
	}
}
