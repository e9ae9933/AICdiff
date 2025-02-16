using System;
using XX;

namespace m2d
{
	public class M2VoiceController : VoiceController
	{
		public M2VoiceController(M2Mover _Mv, VoiceController Src, string _unique_key)
			: base(Src, _unique_key, true)
		{
			this.Mv = _Mv;
		}

		public override void playInner(string str, BusChannelManager BChn)
		{
			if (this.Mv.M2D.Cam.getBaseMover() == this.Mv)
			{
				this.Chn = this.Mv.M2D.Snd.playAbsVo(this.s_header, str);
			}
			else
			{
				this.Chn = this.Mv.M2D.Snd.playAt(this.s_header, str, this.unique_key, this.Mv.x, this.Mv.y, SndPlayer.SNDTYPE.VOICE, 1);
			}
			if (BChn != null)
			{
				BChn.updateSound(this.Chn);
			}
		}

		public readonly M2Mover Mv;
	}
}
