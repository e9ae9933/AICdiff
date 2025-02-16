using System;

namespace XX
{
	public class EfParticleLooperZT : EfParticleLooper
	{
		public EfParticleLooperZT(string _key)
			: base(_key)
		{
		}

		public override bool Draw(EffectItem Ef, float loop_maxt = -1f)
		{
			Ef.z = this.z;
			Ef.time = this.time;
			return base.Draw(Ef, loop_maxt);
		}

		public float x;

		public float y;

		public float z;

		public int time;
	}
}
