using System;
using System.Collections.Generic;
using Better;

namespace XX
{
	public class VoiceInfo
	{
		public VoiceInfo(string _family, float _intv_min, float _intv_max = -1f)
		{
			this.family = _family;
			this.intv_min = _intv_min;
			this.intv_max = ((_intv_max < 0f) ? this.intv_min : _intv_max);
		}

		public void clearTime(bool shuffle_time = false)
		{
			this.t = this.startt;
			if (shuffle_time && this.intv_min > 0f && this.VC != null)
			{
				this.t = X.NIXP(this.intv_min * 0.4f, this.intv_max);
			}
		}

		public bool is_loop
		{
			get
			{
				return this.intv_min == -2f;
			}
		}

		public void dispose()
		{
			if (this.Player != null && this.Player.key == "_buffer")
			{
				this.Player.Dispose();
			}
			this.Player = null;
		}

		public void stop(bool only_loop_snd = false)
		{
			if (this.Player != null && (!only_loop_snd || this.is_loop))
			{
				this.Player.Stop();
				this.dispose();
			}
		}

		public void playOnce()
		{
			if (this.VC != null)
			{
				this.VC.play(this.family, false);
				return;
			}
			if (this.intv_min != -2f)
			{
				this.Player = SND.Ui;
				this.Player.play(this.family, false);
				return;
			}
			if (X.DEBUGNOSND)
			{
				return;
			}
			this.Player = new SndPlayer("_buffer", SndPlayer.SNDTYPE.SND);
			this.Player.FineVol();
			this.Player.play(this.family, false);
		}

		public bool run(float fcnt)
		{
			if (this.intv_min < 0f)
			{
				if (this.t >= 1f)
				{
					return true;
				}
				this.t = 1f;
				this.playOnce();
			}
			else
			{
				this.t -= fcnt;
				if (this.t <= 0f)
				{
					this.t = X.NIXP(this.intv_min, this.intv_max);
					this.playOnce();
				}
			}
			return true;
		}

		public static void stopAll(BDic<string, List<VoiceInfo>> OSndInfo, bool only_loop_snd = false)
		{
			if (OSndInfo == null)
			{
				return;
			}
			foreach (KeyValuePair<string, List<VoiceInfo>> keyValuePair in OSndInfo)
			{
				for (int i = keyValuePair.Value.Count - 1; i >= 0; i--)
				{
					keyValuePair.Value[i].stop(only_loop_snd);
				}
			}
		}

		public static BDic<string, List<VoiceInfo>> copyDictionary(BDic<string, List<VoiceInfo>> OPre)
		{
			BDic<string, List<VoiceInfo>> bdic = new BDic<string, List<VoiceInfo>>((OPre != null) ? OPre.Count : 1);
			if (OPre == null)
			{
				return bdic;
			}
			foreach (KeyValuePair<string, List<VoiceInfo>> keyValuePair in OPre)
			{
				bdic[keyValuePair.Key] = new List<VoiceInfo>(keyValuePair.Value);
			}
			return bdic;
		}

		public VoiceController VC;

		public string family;

		private float intv_min;

		private float intv_max;

		public float t;

		public float startt;

		private SndPlayer Player;
	}
}
