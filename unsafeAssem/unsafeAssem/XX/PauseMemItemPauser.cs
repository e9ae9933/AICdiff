using System;

namespace XX
{
	public class PauseMemItemPauser : PauseMemItem
	{
		public override string ToString()
		{
			return this.Mb.ToString();
		}

		public PauseMemItemPauser(IPauseable _Mb)
		{
			this.Mb = _Mb;
		}

		public override void Pause()
		{
			this.Mb.Pause();
		}

		public override void Resume()
		{
			this.Mb.Resume();
		}

		private IPauseable Mb;
	}
}
