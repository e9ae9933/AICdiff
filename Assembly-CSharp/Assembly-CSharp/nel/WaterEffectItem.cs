using System;
using m2d;
using XX;

namespace nel
{
	public class WaterEffectItem : IRunAndDestroy
	{
		public WaterEffectItem Init(Map2d Mp, WaterEffectItem.TYPE _type, float x, float y, float z, int time)
		{
			this.xybit = Map2d.xy2b((int)x, (int)y);
			this.type = _type;
			this.f0 = Mp.floort;
			this.El = (this.El_next = null);
			this.makeEffect(x, y, z, time);
			return this;
		}

		public void makeEffect(float x, float y, float z, int time)
		{
			if (this.El_next != null)
			{
				if (x != -1000f)
				{
					this.El_next.x = x;
				}
				if (y != -1000f)
				{
					this.El_next.y = x;
				}
				if (z != -1000f)
				{
					this.El_next.z = z;
					this.El_next.time = time;
				}
				return;
			}
			EfParticleLooperZT efParticleLooperZT = new EfParticleLooperZT("wmng_" + this.type.ToString().ToLower());
			if (z != -1000f)
			{
				efParticleLooperZT.z = z;
				efParticleLooperZT.time = time;
			}
			else if (this.El != null)
			{
				efParticleLooperZT.z = this.El.z;
				efParticleLooperZT.time = this.El.time;
			}
			if (x != -1000f)
			{
				efParticleLooperZT.x = x;
			}
			else if (this.El != null)
			{
				efParticleLooperZT.x = this.El.x;
			}
			if (y != -1000f)
			{
				efParticleLooperZT.y = y;
			}
			else if (this.El != null)
			{
				efParticleLooperZT.y = this.El.y;
			}
			if (this.El == null)
			{
				this.El = efParticleLooperZT;
				return;
			}
			this.El_next = efParticleLooperZT;
		}

		public bool run(float fcnt)
		{
			if (this.type == WaterEffectItem.TYPE.NONE || (this.El == null && this.El_discard == null))
			{
				return false;
			}
			WaterEffectItem.Ef.x = Map2d.b2x(this.xybit) + 0.5f;
			WaterEffectItem.Ef.y = Map2d.b2y(this.xybit) + 0.5f;
			float sum_delay = ((this.El == null) ? this.El_discard : this.El).sum_delay;
			float floort = M2DBase.Instance.curMap.floort;
			bool flag = false;
			if (this.El_next != null)
			{
				WaterEffectItem.Ef.f0 = (int)(this.f0 + sum_delay);
				WaterEffectItem.Ef.af = floort - (float)WaterEffectItem.Ef.f0;
				if (WaterEffectItem.Ef.af >= 0f)
				{
					flag = true;
					this.El_next.Draw(WaterEffectItem.Ef, 0f);
				}
			}
			if (this.El != null)
			{
				WaterEffectItem.Ef.f0 = (int)this.f0;
				WaterEffectItem.Ef.af = floort - (float)WaterEffectItem.Ef.f0;
				if (!this.El.Draw(WaterEffectItem.Ef, 0f))
				{
					flag = true;
				}
			}
			if (this.El_discard != null)
			{
				WaterEffectItem.Ef.f0 = (int)(this.f0 - sum_delay);
				WaterEffectItem.Ef.af = floort - (float)WaterEffectItem.Ef.f0;
				if (!this.El_discard.Draw(WaterEffectItem.Ef, 0f))
				{
					this.El_discard = null;
				}
			}
			if (flag)
			{
				this.f0 += sum_delay;
				this.El_discard = this.El;
				this.El = this.El_next;
				this.El_next = null;
			}
			return true;
		}

		public void destruct()
		{
			this.type = WaterEffectItem.TYPE.NONE;
		}

		public uint xybit;

		public WaterEffectItem.TYPE type;

		public float f0;

		public EfParticleLooperZT El;

		public EfParticleLooperZT El_discard;

		public EfParticleLooperZT El_next;

		public static EffectItem Ef;

		public enum TYPE
		{
			NONE,
			FALL_INJECT,
			FALL_HIT
		}
	}
}
