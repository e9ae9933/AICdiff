using System;

namespace XX
{
	public class SndPlayerNoUseable : SndPlayer
	{
		public SndPlayerNoUseable(string key = "")
			: base(key, SndPlayer.SNDTYPE.SND)
		{
		}

		public override bool play(string header, string cue_name, bool force = false)
		{
			return false;
		}

		public override bool prepare(string header, string cue_name, bool force = false)
		{
			return false;
		}

		public override SndPlayer FineVol()
		{
			this.need_update_flag = false;
			return this;
		}
	}
}
