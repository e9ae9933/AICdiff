using System;

namespace XX
{
	public class PauseMemItem
	{
		public virtual void Pause()
		{
		}

		public virtual void Resume()
		{
		}

		public void setEnable(bool f)
		{
			if (f)
			{
				this.Resume();
				return;
			}
			this.Pause();
		}
	}
}
