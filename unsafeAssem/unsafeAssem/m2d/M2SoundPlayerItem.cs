using System;
using XX;

namespace m2d
{
	public class M2SoundPlayerItem : SndPlayer
	{
		public M2SoundPlayerItem(M2SoundPlayer _Con, string _key, bool abs, SndPlayer.SNDTYPE stype = SndPlayer.SNDTYPE.SND)
			: base(_key, stype)
		{
			this.Con = _Con;
			this.abs_flag = abs;
		}

		public override bool play(string header, string cue_key, bool force = false)
		{
			if (base.play(header, cue_key, force))
			{
				this.need_update_flag = (this.Con.need_update_flag = true);
				return true;
			}
			return false;
		}

		public override void Stop()
		{
			base.Stop();
			this.id = 0U;
		}

		public override bool prepare(string header, string cue_key, bool force = false)
		{
			if (base.prepare(header, cue_key, force))
			{
				this.need_update_flag = (this.Con.need_update_flag = true);
				return true;
			}
			return false;
		}

		public M2SoundPlayerItem setVolManual(float f, bool fine_player = true)
		{
			this.volume_maunal = f;
			if (fine_player)
			{
				this.FineVol();
			}
			this.need_update_flag = (this.Con.need_update_flag = true);
			return this;
		}

		public virtual M2SoundPlayerItem Pan(float f, float _volume_pan, bool force_fine_volume = false, bool force_monoral = false)
		{
			if (this.Exp == null)
			{
				return this;
			}
			this.Exp.SetPan3dAngle((force_monoral || M2SoundPlayer.monoral) ? 0f : (X.MMX(-0.8f, f, 0.8f) * 180f));
			if (force_fine_volume || _volume_pan != this.volume_pan)
			{
				this.volume_pan = _volume_pan;
				this.FineVol();
			}
			else
			{
				this.need_update_flag = (this.Con.need_update_flag = true);
			}
			return this;
		}

		public override SndPlayer FineVol()
		{
			if (this.Exp == null)
			{
				return this;
			}
			this.Exp.SetVolume(this.volume_pan * this.volume_maunal * this.base_volume);
			this.need_update_flag = (this.Con.need_update_flag = true);
			return this;
		}

		public void prepareAt(string header, string cue_key, float mapx, float mapy)
		{
			this.prepare(header, cue_key, false);
			this.mapx = mapx;
			this.mapy = mapy;
		}

		public void finePosition(string cue_key, float mapx, float mapy)
		{
			this.Con.posFine();
			this.mapx = mapx;
			this.mapy = mapy;
			this.need_update_flag = (this.Con.need_update_flag = true);
		}

		public override float base_volume
		{
			get
			{
				return base.base_volume * ((this.stype == SndPlayer.SNDTYPE.VOICE) ? this.Con.voice_volume_for_effect : this.Con.sound_volume_for_effect);
			}
		}

		public SndPlayer BgmPlayer;

		public readonly M2SoundPlayer Con;

		public readonly bool abs_flag;

		public float mapx;

		public float mapy;

		public float volume_pan = 1f;

		public float volume_maunal = 1f;

		public uint id;
	}
}
