using System;
using Spine;
using XX;

namespace nel.fatal
{
	internal sealed class FtParticle : EffectHandler<EffectItem>
	{
		public FtParticle()
			: base(6, null)
		{
		}

		public FtParticle Set(FtLayer _Ftl, string particle, string bone, float _intv, float _z, int _time, float _saf, bool _is_effect = true, bool _use_rot_to_z = false, bool _is_once = false)
		{
			base.release(false);
			this.Ftl = _Ftl;
			this.Ptc = EfParticleManager.Get(particle, false, false);
			if (this.Ptc == null || this.Ftl.Spv == null)
			{
				return null;
			}
			if (bone != null)
			{
				this.FixTo = this.Ftl.Spv.FindBone(bone);
				if (this.FixTo == null)
				{
					X.de("不明なボーン: " + bone, null);
					return null;
				}
			}
			else
			{
				this.FixTo = null;
			}
			this.intv = _intv;
			this.z = _z;
			this.time = _time;
			this.is_effect = _is_effect;
			this.use_rot_to_z = _use_rot_to_z;
			this.is_once = _is_once;
			if (this.intv <= 0f)
			{
				this.t = 0f;
				this.saf = _saf;
			}
			else
			{
				this.t = _saf;
				this.saf = 0f;
			}
			return this;
		}

		public bool run(FtEffect EF, float fcnt)
		{
			if (this.is_effect && !EF.effect_ptc_enabled)
			{
				if (base.Count > 0)
				{
					for (int i = base.Count - 1; i >= 0; i--)
					{
						this.AEf[i].Ef.destruct();
					}
					this.AEf.Clear();
					this.t /= 2f;
				}
			}
			else
			{
				if ((this.intv > 0f) ? (this.t <= 0f) : (this.t == 0f))
				{
					EF.enabled = true;
					EffectItem effectItem = EF.PtcN(this.Ptc, 0f, 0f, this.z, this.time, (int)this.saf);
					if (this.is_once)
					{
						this.FineEf(effectItem);
					}
					else
					{
						base.Set(effectItem);
					}
					if (this.intv > 0f)
					{
						this.t = this.intv;
					}
					else
					{
						this.t = -1f;
					}
				}
				this.t -= fcnt;
			}
			if (this.FixTo != null)
			{
				for (int j = base.Count - 1; j >= 0; j--)
				{
					EffectHandler<EffectItem>.HandledItem handledItem = this.AEf[j];
					if (!this.FineEf(handledItem))
					{
						this.AEf.RemoveAt(j);
					}
				}
			}
			return this.intv > 0f || base.Count > 0;
		}

		private bool FineEf(EffectHandler<EffectItem>.HandledItem E)
		{
			if (!E.isActive())
			{
				return false;
			}
			this.FineEf(E.Ef);
			return true;
		}

		private void FineEf(EffectItem Ef)
		{
			if (Ef != null && this.FixTo != null)
			{
				Ef.x = this.FixTo.WorldX;
				Ef.y = this.FixTo.WorldY;
				if (this.use_rot_to_z)
				{
					Ef.z = this.calcZ();
				}
			}
		}

		public float calcZ()
		{
			if (this.use_rot_to_z && this.FixTo != null)
			{
				this.z = this.FixTo.WorldRotationX * 0.017453292f;
			}
			return this.z;
		}

		internal FtLayer Ftl;

		public EfParticle Ptc;

		internal Bone FixTo;

		public float z;

		public int time;

		public float saf;

		public float t;

		public float intv;

		public bool is_effect;

		public bool use_rot_to_z;

		public bool is_once;
	}
}
