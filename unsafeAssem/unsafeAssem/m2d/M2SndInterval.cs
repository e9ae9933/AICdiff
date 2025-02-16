using System;
using XX;

namespace m2d
{
	public sealed class M2SndInterval : M2SoundPlayerItem, IRunAndDestroy
	{
		public M2SndInterval(M2SoundPlayer _Con, string _key, string cue_name, float _interval, IPosLitener _PosLsn, float saf = 0f, int _play_count = 128)
			: base(_Con, _key, _PosLsn == null, SndPlayer.SNDTYPE.SND)
		{
			this.interval = _interval;
			this.play_count = _play_count;
			SND.GetCue(cue_name, this);
			this.PosLsn = _PosLsn;
			if (saf <= 0f)
			{
				if (this.play_count != 0)
				{
					this.playSoundInterval();
				}
			}
			else
			{
				this.t = -saf;
			}
			this.Con.M2D.curMap.addRunnerObject(this);
		}

		public M2SndInterval Change(string _next_cue_key, float _next_interval = -1f)
		{
			this.next_cue = _next_cue_key;
			if (_next_interval >= 0f)
			{
				this.interval = _next_interval;
			}
			return this;
		}

		public M2SndInterval TimeAbsolute(bool flag = false)
		{
			this.time_absolute = flag;
			return this;
		}

		public bool run(float fcnt)
		{
			if (this.t >= 0f)
			{
				if (this.play_count == 0)
				{
					this.destruct();
					return false;
				}
				if (this.active)
				{
					this.playSoundInterval();
					this.t += (this.time_absolute ? 1f : fcnt);
				}
			}
			else
			{
				this.t += (this.time_absolute ? 1f : fcnt);
			}
			if (this.already_played && this.PosLsn != null)
			{
				float num;
				float num2;
				if (!this.PosLsn.getPosition(out num, out num2))
				{
					this.destruct();
					return false;
				}
				if (this.mapx != num || this.mapy != num2)
				{
					base.finePosition(this.current_cue, num, num2);
				}
			}
			return true;
		}

		public override bool isPlaying()
		{
			return this.play_count != 0 || base.isPlaying();
		}

		public void playSoundInterval()
		{
			this.already_played = true;
			if (this.next_cue != null)
			{
				this.current_cue = this.next_cue;
				SND.GetCue(this.current_cue, this);
				this.next_cue = null;
			}
			base.prepareCurrentCue(false);
			this.playDefault();
			this.need_update_flag = (this.Con.need_update_flag = true);
			this.t -= this.interval;
			if (this.play_count > 0)
			{
				this.play_count--;
			}
		}

		public override void Stop()
		{
			base.Stop();
			this.play_count = 0;
			this.t = 0f;
		}

		public override string ToString()
		{
			return this.current_cue ?? "<M2SndInverval>";
		}

		public void destruct()
		{
			this.t = 0f;
			this.play_count = 0;
			this.Con.removeAndDispose(this);
		}

		private float t;

		public float interval;

		public int play_count = -1;

		private bool already_played;

		public bool active = true;

		public bool time_absolute = true;

		public string next_cue;

		public IPosLitener PosLsn;
	}
}
