using System;
using System.Collections.Generic;

namespace XX
{
	public class SndPlayer
	{
		public SndPlayer(string _key, SndPlayer.SNDTYPE _type = SndPlayer.SNDTYPE.SND)
		{
			this.key = _key;
			this.stype = _type;
			if (!X.DEBUGNOSND)
			{
				this.Exp = new CriAtomExPlayer();
			}
		}

		public virtual bool isPlaying()
		{
			if (this.Exp == null)
			{
				return false;
			}
			CriAtomExPlayer.Status status = this.Exp.GetStatus();
			return status == CriAtomExPlayer.Status.Prep || status == CriAtomExPlayer.Status.Playing || this.is_loop == 2;
		}

		public virtual void Dispose()
		{
			if (this.Exp == null)
			{
				return;
			}
			if (this.isPlaying())
			{
				this.Exp.StopWithoutReleaseTime();
			}
			this.Exp.Dispose();
		}

		public bool stopPlay(string cue_name)
		{
			this.Stop();
			return this.play(cue_name, false);
		}

		public virtual void Stop()
		{
			if (this.Exp == null)
			{
				return;
			}
			this.Exp.Stop();
			if (this.is_loop == 2)
			{
				this.is_loop = 1;
			}
			if (this.APlayBack != null)
			{
				this.APlayBack.Clear();
			}
		}

		public virtual void Pause()
		{
			if (this.Exp == null)
			{
				return;
			}
			this.Exp.Pause();
			if (this.is_loop == 2)
			{
				this.is_loop = 1;
			}
		}

		public virtual void Start()
		{
			if (this.Exp == null)
			{
				return;
			}
			this.FineVol();
			this.Exp.Start();
			if (this.is_loop == 1)
			{
				this.is_loop = 2;
			}
		}

		public virtual SndPlayer FineVol()
		{
			if (this.Exp == null)
			{
				return this;
			}
			this.Exp.SetVolume(this.base_volume);
			this.UpdateAll();
			return this;
		}

		public virtual SndPlayer UpdateAll()
		{
			if (this.Exp == null)
			{
				return this;
			}
			this.Exp.UpdateAll();
			this.need_update_flag = false;
			return this;
		}

		public virtual void SetAisacControl(string controlName, float value)
		{
			if (this.Exp == null)
			{
				return;
			}
			this.Exp.SetAisacControl(controlName, value);
		}

		public bool play(string cue_name, bool force = false)
		{
			return this.play("", cue_name, force);
		}

		public virtual bool play(string header, string cue_name, bool force = false)
		{
			if (cue_name == "" || cue_name == null || this.Exp == null)
			{
				return false;
			}
			if (!SND.GetCue(header, cue_name, this))
			{
				return false;
			}
			if (!SND.checkSameFrameSound(header, cue_name) && !force)
			{
				return false;
			}
			this.Exp.ResetParameters();
			this.Exp.SetVoicePriority(this.voice_priority);
			this.playDefault();
			return true;
		}

		public virtual void playDefault()
		{
			this.Start();
			this.need_update_flag = true;
		}

		public bool prepare(string cue_name, bool force = false)
		{
			return this.prepare("", cue_name, force);
		}

		public virtual bool prepare(string header, string cue_name, bool force = false)
		{
			return SND.GetCue(header, cue_name, this) && this.prepareCurrentCue(force);
		}

		public bool prepareCurrentCue(bool force = false)
		{
			if (this.Exp == null || (!SND.checkSameFrameSound("", this.current_cue) && !force))
			{
				return false;
			}
			this.Exp.ResetParameters();
			this.Exp.SetVoicePriority(this.voice_priority);
			CriAtomExPlayback criAtomExPlayback = this.Exp.Prepare();
			if (this.APlayBack == null)
			{
				this.APlayBack = new List<CriAtomExPlayback>(4);
			}
			this.FineVol();
			this.APlayBack.Add(criAtomExPlayback);
			this.need_update_flag = true;
			return true;
		}

		public virtual void SetCue(CriAtomExAcb acb, string name, CriAtomEx.CueInfo Info)
		{
			if (this.Exp == null)
			{
				return;
			}
			this.Exp.SetCue(acb, Info.id);
			this.Exp.SetVoicePriority(this.voice_priority);
			this.current_cue = name;
			this.duration = Info.length;
			this.is_loop = ((this.duration == -1L) ? 1 : 0);
		}

		public bool checkPreparing(bool resumeing = true)
		{
			if (this.APlayBack != null && this.APlayBack.Count > 0)
			{
				int count = this.APlayBack.Count;
				for (int i = this.APlayBack.Count - 1; i >= 0; i--)
				{
					CriAtomExPlayback.Status status = this.APlayBack[i].GetStatus();
					if (status == CriAtomExPlayback.Status.Playing)
					{
						if (resumeing)
						{
							this.APlayBack[i].Resume(CriAtomEx.ResumeMode.PreparedPlayback);
						}
						else
						{
							this.resume_flag = true;
						}
						this.APlayBack.RemoveAt(i);
						if (this.is_loop == 1)
						{
							this.is_loop = 2;
						}
					}
					else if (status == CriAtomExPlayback.Status.Removed)
					{
						this.APlayBack.RemoveAt(i);
					}
				}
				return this.APlayBack.Count > 0;
			}
			return false;
		}

		public long rest_duration_milisecond
		{
			get
			{
				if (this.isPlaying())
				{
					return this.duration - this.Exp.GetTime();
				}
				return 0L;
			}
		}

		public bool is_preparing()
		{
			return this.APlayBack != null && this.APlayBack.Count > 0;
		}

		public virtual float base_volume
		{
			get
			{
				if (this.stype == SndPlayer.SNDTYPE.VOICE)
				{
					return SND.voice_volume01;
				}
				if (this.stype != SndPlayer.SNDTYPE.BGM)
				{
					return SND.volume01;
				}
				return SND.bgm_volume01;
			}
		}

		public int voice_priority
		{
			get
			{
				return (int)(this.stype * (SndPlayer.SNDTYPE)100 + (int)this.voice_priority_manual);
			}
		}

		public CriAtomExPlayer getPlayerInstance()
		{
			return this.Exp;
		}

		public virtual void SetBusSendLevelAndOffset(string k, float v1, float levelOffset)
		{
			if (this.Exp == null)
			{
				return;
			}
			this.Exp.SetBusSendLevel(k, v1);
			this.Exp.SetBusSendLevelOffset(k, levelOffset);
		}

		public string key;

		public bool need_update_flag = true;

		public string current_cue;

		public long duration;

		public byte is_loop;

		public byte voice_priority_manual = 1;

		public bool resume_flag;

		protected CriAtomExPlayer Exp;

		protected List<CriAtomExPlayback> APlayBack;

		public SndPlayer.SNDTYPE stype;

		public enum SNDTYPE
		{
			SND,
			VOICE,
			BGM
		}
	}
}
