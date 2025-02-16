using System;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class M2SoundPlayer : SndPlayerMultiT<M2SoundPlayerItem>
	{
		public M2SoundPlayer(M2DBase _M2D)
			: base(8)
		{
			this.M2D = _M2D;
			this.APlayer.Add(new M2SoundPlayerItem(this, "_abs", true, SndPlayer.SNDTYPE.SND));
			this.APlayer.Add(new M2SoundPlayerItem(this, "_voice", true, SndPlayer.SNDTYPE.VOICE));
			this.Environment = new M2AreaSndLoop(this.M2D);
		}

		public void clear()
		{
			if (this.Environment == null)
			{
				return;
			}
			this.Environment.clear();
			base.flush(null, 2);
			this.need_position_fine_ = -1;
		}

		public override void destruct()
		{
			if (this.Environment == null)
			{
				return;
			}
			this.clear();
			base.destruct();
			this.Environment.destruct();
			this.Environment = null;
		}

		public M2SoundPlayerItem play(string cue_key)
		{
			return this.play("", cue_key);
		}

		public M2SoundPlayerItem play(string header, string cue_key)
		{
			this.Abs.setVolManual(1f, false).prepare(header, cue_key, false);
			return this.Abs;
		}

		public M2SoundPlayerItem playAbsVo(string cue_key)
		{
			return this.playAbsVo("", cue_key);
		}

		public M2SoundPlayerItem playAbsVo(string header, string cue_key)
		{
			this.AbsVo.setVolManual(1f, false).prepare(header, cue_key, false);
			return this.AbsVo;
		}

		public M2SoundPlayerItem createAbs(string pos_snd_key)
		{
			int count = this.APlayer.Count;
			for (int i = 0; i < count; i++)
			{
				M2SoundPlayerItem m2SoundPlayerItem = this.APlayer[i];
				if (m2SoundPlayerItem != null && m2SoundPlayerItem.abs_flag && m2SoundPlayerItem.key == pos_snd_key)
				{
					return m2SoundPlayerItem;
				}
			}
			M2SoundPlayerItem m2SoundPlayerItem2 = new M2SoundPlayerItem(this, pos_snd_key, true, SndPlayer.SNDTYPE.SND);
			this.APlayer.Add(m2SoundPlayerItem2);
			return m2SoundPlayerItem2;
		}

		public M2SndInterval createInterval(string pos_snd_key, string cue_name, float _interval, IPosLitener _PosLsn, float saf = 0f, int _play_count = 128)
		{
			if (X.DEBUGNOSND)
			{
				return null;
			}
			int count = this.APlayer.Count;
			for (int i = 0; i < count; i++)
			{
				M2SndInterval m2SndInterval = this.APlayer[i] as M2SndInterval;
				if (m2SndInterval != null && m2SndInterval.abs_flag && m2SndInterval.key == pos_snd_key && m2SndInterval.current_cue == cue_name)
				{
					return m2SndInterval;
				}
			}
			M2SndInterval m2SndInterval2 = new M2SndInterval(this, pos_snd_key, cue_name, _interval, _PosLsn, saf, _play_count);
			this.APlayer.Add(m2SndInterval2);
			return m2SndInterval2;
		}

		public M2SoundPlayerItem removeAndDispose(M2SoundPlayerItem Pl)
		{
			Pl.Dispose();
			this.APlayer.Remove(Pl);
			return null;
		}

		public M2SoundPlayerItem playAt(string cue_key, string pos_snd_key, float mapx, float mapy = -1000f, SndPlayer.SNDTYPE sndtype = SndPlayer.SNDTYPE.SND, byte _voice_priority_manual = 1)
		{
			return this.playAt("", cue_key, pos_snd_key, mapx, mapy, sndtype, _voice_priority_manual);
		}

		public M2SoundPlayerItem playAt(string header, string cue_key, string pos_snd_key, float mapx, float mapy = -1000f, SndPlayer.SNDTYPE sndtype = SndPlayer.SNDTYPE.SND, byte _voice_priority_manual = 1)
		{
			M2Mover baseMover = this.M2D.Cam.getBaseMover();
			if (baseMover != null && pos_snd_key == baseMover.snd_key)
			{
				return this.play(cue_key);
			}
			float num = ((baseMover == null) ? this.M2D.Cam.x : baseMover.drawx);
			float num2 = ((baseMover == null) ? this.M2D.Cam.y : baseMover.drawy);
			if (mapy == -1000f)
			{
				mapy = num2 / this.M2D.CLEN;
			}
			float num3 = mapx * this.M2D.CLEN - num;
			float num4 = mapy * this.M2D.CLEN - num2;
			float num5 = X.LENGTHXYN(0f, 0f, num3, num4);
			float num6 = 20f * this.M2D.CLEN;
			if (num5 <= num6 || _voice_priority_manual >= 2)
			{
				int count = this.APlayer.Count;
				M2SoundPlayerItem m2SoundPlayerItem = null;
				bool flag = true;
				for (int i = 0; i < count; i++)
				{
					M2SoundPlayerItem m2SoundPlayerItem2 = this.APlayer[i];
					if (m2SoundPlayerItem2 == null)
					{
						m2SoundPlayerItem = (this.APlayer[i] = new M2SoundPlayerItem(this, pos_snd_key, false, SndPlayer.SNDTYPE.SND));
						break;
					}
					if (!m2SoundPlayerItem2.abs_flag)
					{
						if (!m2SoundPlayerItem2.isPlaying())
						{
							m2SoundPlayerItem = m2SoundPlayerItem2;
							break;
						}
						if (m2SoundPlayerItem2.key == pos_snd_key)
						{
							m2SoundPlayerItem = m2SoundPlayerItem2;
							flag = false;
							break;
						}
					}
				}
				if (m2SoundPlayerItem == null)
				{
					m2SoundPlayerItem = new M2SoundPlayerItem(this, pos_snd_key, false, SndPlayer.SNDTYPE.SND);
					this.APlayer.Add(m2SoundPlayerItem);
				}
				else
				{
					m2SoundPlayerItem.key = pos_snd_key;
				}
				if (flag)
				{
					m2SoundPlayerItem.id = (M2SoundPlayer.id_count += 1U);
				}
				m2SoundPlayerItem.stype = sndtype;
				m2SoundPlayerItem.voice_priority_manual = _voice_priority_manual;
				m2SoundPlayerItem.setVolManual(0.125f + 0.875f * (1f - X.ZLINE(num5 - num6 * 0.5f, num6 * 0.5f)), false);
				m2SoundPlayerItem.prepareAt(header, cue_key, mapx, mapy);
				return m2SoundPlayerItem;
			}
			return null;
		}

		public M2SoundPlayerItem finePosition(string cue_key, string pos_snd_key, float mapx, float mapy = -1000f)
		{
			int count = this.APlayer.Count;
			M2SoundPlayerItem m2SoundPlayerItem = null;
			for (int i = 0; i < count; i++)
			{
				M2SoundPlayerItem m2SoundPlayerItem2 = this.APlayer[i];
				if (!(m2SoundPlayerItem2.current_cue != cue_key))
				{
					m2SoundPlayerItem2 = this.finePosition(m2SoundPlayerItem2, pos_snd_key, mapx, mapy);
					if (m2SoundPlayerItem2 != null)
					{
						m2SoundPlayerItem = m2SoundPlayerItem2;
					}
				}
			}
			return m2SoundPlayerItem;
		}

		public M2SoundPlayerItem finePosition(M2SoundPlayerItem _Sr, string pos_snd_key, float mapx, float mapy = -1000f)
		{
			if (_Sr == null || !_Sr.isPlaying() || (pos_snd_key != null && _Sr.key != pos_snd_key))
			{
				return null;
			}
			if (mapy == -1000f)
			{
				mapy = _Sr.mapy;
			}
			_Sr.finePosition(_Sr.current_cue, mapx, mapy);
			return _Sr;
		}

		public void stop(string cue_key, string pos_snd_key)
		{
			for (int i = this.APlayer.Count - 1; i >= 0; i--)
			{
				M2SoundPlayerItem m2SoundPlayerItem = this.APlayer[i];
				if (m2SoundPlayerItem != null && !(m2SoundPlayerItem.current_cue != cue_key) && (pos_snd_key == null || !(m2SoundPlayerItem.key != pos_snd_key)))
				{
					m2SoundPlayerItem.Stop();
				}
			}
		}

		public void stopP(string pos_snd_key)
		{
			for (int i = this.APlayer.Count - 1; i >= 0; i--)
			{
				M2SoundPlayerItem m2SoundPlayerItem = this.APlayer[i];
				if (m2SoundPlayerItem != null && (pos_snd_key == null || !(m2SoundPlayerItem.key != pos_snd_key)))
				{
					m2SoundPlayerItem.Stop();
				}
			}
		}

		public bool isActive(string cue_key, string pos_snd_key)
		{
			for (int i = this.APlayer.Count - 1; i >= 0; i--)
			{
				M2SoundPlayerItem m2SoundPlayerItem = this.APlayer[i];
				if (m2SoundPlayerItem != null && !(m2SoundPlayerItem.current_cue != cue_key) && (pos_snd_key == null || !(m2SoundPlayerItem.key != pos_snd_key)))
				{
					return true;
				}
			}
			return false;
		}

		public bool isActive(M2SoundPlayerItem _Sr, string pos_snd_key)
		{
			return _Sr != null && (pos_snd_key == null || !(_Sr.key != pos_snd_key));
		}

		public void posFine()
		{
			if (this.need_position_fine_ >= 0)
			{
				this.need_position_fine_ = 8;
			}
		}

		public void run()
		{
			Vector2 soundCenter = this.M2D.Cam.getSoundCenter();
			bool flag = true;
			if (this.need_position_fine_ < 8)
			{
				if (this.need_position_fine_ == -1)
				{
					if (this.M2D.curMap != null && this.M2D.curMap.load_cover_disable)
					{
						this.need_position_fine_ = 8;
					}
					else
					{
						flag = false;
					}
				}
				else if (this.pre_x != soundCenter.x || this.pre_y != soundCenter.y)
				{
					this.need_position_fine_++;
				}
			}
			this.pre_x = soundCenter.x;
			this.pre_y = soundCenter.y;
			if (flag)
			{
				this.Environment.need_pos_fine = this.need_position_fine_ >= 8;
				if (this.Environment.need_pos_fine)
				{
					this.need_position_fine_ = 0;
				}
				if (this.Environment.need_pos_fine || this.Environment.need_pos_fine_item)
				{
					this.Environment.run(1f);
				}
			}
			if (this.Environment.need_pos_fine || this.need_update_flag)
			{
				int count = this.APlayer.Count;
				int num = 0;
				for (int i = 0; i < count; i++)
				{
					M2SoundPlayerItem m2SoundPlayerItem = this.APlayer[i];
					if (m2SoundPlayerItem != null && (m2SoundPlayerItem.isPlaying() || m2SoundPlayerItem.is_preparing()))
					{
						if (!m2SoundPlayerItem.abs_flag && (this.Environment.need_pos_fine || m2SoundPlayerItem.need_update_flag))
						{
							float num2 = (m2SoundPlayerItem.mapx - this.pre_x) * this.pan_length_divide;
							m2SoundPlayerItem.Pan(num2, 0.0625f + 0.9375f * (1f - X.ZLINE((X.LENGTHXYN(0f, 0f, m2SoundPlayerItem.mapx - this.pre_x, m2SoundPlayerItem.mapy - this.pre_y) - 1f) * this.pan_length_divide)), false, false);
						}
						if (m2SoundPlayerItem.need_update_flag)
						{
							m2SoundPlayerItem.UpdateAll();
						}
						if (m2SoundPlayerItem.checkPreparing(true))
						{
							num++;
						}
					}
				}
				if (this.Abs.need_update_flag)
				{
					this.Abs.UpdateAll();
				}
				if (this.Abs.checkPreparing(true))
				{
					num++;
				}
				if (this.AbsVo.need_update_flag)
				{
					this.AbsVo.UpdateAll();
				}
				if (this.AbsVo.checkPreparing(true))
				{
					num++;
				}
				this.need_update_flag = num > 0;
				this.Environment.need_pos_fine = false;
			}
		}

		public void fineVolume()
		{
			int count = this.APlayer.Count;
			for (int i = 0; i < count; i++)
			{
				M2SoundPlayerItem m2SoundPlayerItem = this.APlayer[i];
				if (m2SoundPlayerItem != null && m2SoundPlayerItem.isPlaying())
				{
					m2SoundPlayerItem.FineVol();
				}
			}
		}

		public float sound_volume_for_effect
		{
			get
			{
				return this.sound_volume_for_effect_;
			}
			set
			{
				if (value == this.sound_volume_for_effect_)
				{
					return;
				}
				this.sound_volume_for_effect_ = value;
				this.fineVolume();
			}
		}

		public float voice_volume_for_effect
		{
			get
			{
				return this.voice_volume_for_effect_;
			}
			set
			{
				if (value == this.voice_volume_for_effect_)
				{
					return;
				}
				this.voice_volume_for_effect_ = value;
				this.fineVolume();
			}
		}

		public M2SoundPlayerItem Abs
		{
			get
			{
				return base[0];
			}
		}

		public M2SoundPlayerItem AbsVo
		{
			get
			{
				return base[1];
			}
		}

		public readonly M2DBase M2D;

		public M2AreaSndLoop Environment;

		public const int reserved_count = 2;

		public float pre_x;

		public float pre_y;

		public float pan_length_divide = 0.03125f;

		private int need_position_fine_;

		public const int FINE_CNT = 8;

		private float sound_volume_for_effect_ = 1f;

		public static bool monoral;

		private float voice_volume_for_effect_ = 1f;

		private static uint id_count;
	}
}
