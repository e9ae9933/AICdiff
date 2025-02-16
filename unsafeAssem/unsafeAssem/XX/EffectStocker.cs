using System;

namespace XX
{
	public class EffectStocker : IRunAndDestroy
	{
		public bool isKilled(PTCThread Thread)
		{
			if (this.Ef != null)
			{
				return this.Ef.index != this.index;
			}
			return this.Sound != null && Thread.Listener != null && !Thread.Listener.isSoundActive(this.Sound);
		}

		public void kill(PTCThread Thread)
		{
			if (!this.isKilled(Thread))
			{
				if (this.Ef != null)
				{
					this.Ef.destruct();
				}
				if (this.Sound != null && Thread != null)
				{
					this.Sound.Stop();
				}
			}
			this.Ef = null;
			this.Sound = null;
		}

		public void updateFrameStockEffect(float mint)
		{
			if (this.Ef != null)
			{
				this.Ef.af = X.Mx(mint - this.Ef.saf, this.Ef.af);
				this.Ef.saf = 0f;
			}
		}

		public EffectStocker Set(EffectItem _E)
		{
			this.Ef = _E;
			this.Sound = null;
			this.index = _E.index;
			return this;
		}

		public EffectStocker Set(SndPlayer _E)
		{
			this.Sound = _E;
			this.Ef = null;
			return this;
		}

		public bool Is(PTCThread Thread, string s)
		{
			if (!this.isKilled(Thread))
			{
				if (this.Ef != null)
				{
					return this.Ef.title == s;
				}
				if (this.Sound != null)
				{
					return this.Sound.current_cue == s;
				}
			}
			return false;
		}

		public bool run(float fcnt)
		{
			PTCThread currentReading = PTCThread.CurrentReading;
			if (currentReading == null || this.isKilled(currentReading))
			{
				return false;
			}
			if (this.Ef != null && PTCThread.PosEffectReposit.z != 0f)
			{
				this.Ef.x = PTCThread.PosEffectReposit.x;
				this.Ef.y = PTCThread.PosEffectReposit.y;
			}
			return true;
		}

		public void destruct()
		{
			this.kill(null);
		}

		public EffectItem Ef;

		public SndPlayer Sound;

		private uint index;
	}
}
