using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public sealed class M2SndLoopItem : IRunAndDestroy
	{
		public M2SndLoopItem(M2AreaSndLoop _Con)
		{
			this.Con = _Con;
		}

		public M2SndLoopItem SetM2(string _sound_name)
		{
			this.sound_name = _sound_name;
			this.load_check = true;
			if (this.MyPlayer != null)
			{
				this.MyPlayer.Stop();
			}
			this.MyPlayer = null;
			if (this.ARc == null)
			{
				this.ARc = new List<M2SndLoopItem.M2SL_Area>(2);
			}
			else
			{
				this.ARc.Clear();
			}
			return this;
		}

		public M2SndLoopItem clearRect()
		{
			this.ARc.Clear();
			return this;
		}

		public M2SndLoopItem.M2SL_Area AddArea(string _unique_key, float _cx, float _cy, float _listenx = 8f, float _listeny = 6f, float _areax = 0f, float _areay = 0f, IPosLitener _PosLsn = null)
		{
			M2SndLoopItem.M2SL_Area m2SL_Area = new M2SndLoopItem.M2SL_Area
			{
				unique_key = _unique_key,
				cx = _cx,
				cy = _cy,
				listenxr = 1f / _listenx,
				listenyr = 1f / _listeny,
				areax = _areax,
				areay = _areay,
				PosLsn = _PosLsn
			};
			this.ARc.Add(m2SL_Area);
			this.pos_fine = (this.Con.need_pos_fine_item = true);
			return m2SL_Area;
		}

		public bool run(float fcnt)
		{
			if (this.sound_name == null || this.ARc == null)
			{
				this.destruct();
				return false;
			}
			if (!this.pos_fine && !this.Con.need_pos_fine)
			{
				return true;
			}
			this.pos_fine = false;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = this.ARc.Count - 1; i >= 0; i--)
			{
				M2SndLoopItem.M2SL_Area m2SL_Area = this.ARc[i];
				if (m2SL_Area.volume < 0f)
				{
					this.ARc.RemoveAt(i);
				}
				else if (m2SL_Area.PosLsn == null || m2SL_Area.PosLsn.getPosition(out m2SL_Area.cx, out m2SL_Area.cy))
				{
					float num4 = X.Mx(0f, X.Abs(m2SL_Area.cx - this.Con.pre_x) - m2SL_Area.areax) * m2SL_Area.listenxr;
					float num5 = X.Mx(0f, X.Abs(m2SL_Area.cy - this.Con.pre_y) - m2SL_Area.areay) * m2SL_Area.listenyr;
					float num6 = num4 + num5;
					num3 += m2SL_Area.cx;
					if (num6 < 1f)
					{
						float num7 = X.Mx(0f, 1f - num6) * m2SL_Area.volume;
						float num8 = (num4 * (float)X.MPF(m2SL_Area.cx - this.Con.pre_x > 0f) + 1f) * 0.5f;
						num2 += X.ZLINE(num8 - 0.25f, 0.5f) * num7;
						num += X.ZLINE(0.5f - (num8 - 0.25f), 0.5f) * num7;
					}
				}
			}
			if (this.min_volume > 0f)
			{
				num3 /= (float)this.ARc.Count;
				float num9 = X.ZLINE(this.Con.pre_x - num3, 12f);
				float num10 = X.ZLINE(num3 - this.Con.pre_x, 12f);
				float num11 = 0.5f + (-num9 + num10) * 0.5f;
				num = X.Scr(this.min_volume * (1f - num11), num);
				num2 = X.Scr(this.min_volume * num11, num2);
			}
			if (num2 == 0f && num == 0f)
			{
				if (this.isActive())
				{
					this.Stop();
				}
				return true;
			}
			if (!this.isActive())
			{
				if (this.load_check && !SND.loaded)
				{
					return true;
				}
				this.load_check = false;
				if (this.MyPlayer == null)
				{
					this.MyPlayer = this.Con.M2D.Snd.createAbs(this.pos_snd_key);
					this.MyPlayer.voice_priority_manual = 6;
				}
				if (!this.MyPlayer.isPlaying())
				{
					this.MyPlayer.play(this.sound_name, false);
				}
			}
			if (this.MyPlayer != null)
			{
				float num12 = num + num2;
				this.MyPlayer.Pan((-num + num2) / num12, X.Mn(num12, 1f), false, this.force_monoral);
			}
			return true;
		}

		public M2SndLoopItem Vol(string unique_key, float vol)
		{
			if (this.ARc != null)
			{
				int i = this.ARc.Count - 1;
				while (i >= 0)
				{
					M2SndLoopItem.M2SL_Area m2SL_Area = this.ARc[i];
					if (m2SL_Area.unique_key == unique_key)
					{
						if (m2SL_Area.volume == vol)
						{
							return this;
						}
						m2SL_Area.volume = vol;
						return this;
					}
					else
					{
						i--;
					}
				}
			}
			return this;
		}

		public void removeUniqueKey(string unique_key)
		{
			if (this.ARc != null)
			{
				for (int i = this.ARc.Count - 1; i >= 0; i--)
				{
					if (this.ARc[i].unique_key == unique_key)
					{
						this.ARc.RemoveAt(i);
					}
				}
				if (this.ARc.Count == 0)
				{
					this.destruct();
				}
			}
		}

		public bool clipIs(string _sound_name)
		{
			return this.sound_name == _sound_name;
		}

		public string pos_snd_key
		{
			get
			{
				return "m2d.loop." + this.sound_name;
			}
		}

		public void Stop()
		{
			if (this.MyPlayer != null)
			{
				this.MyPlayer.Stop();
			}
		}

		public bool isActive()
		{
			return this.MyPlayer != null && this.MyPlayer.isPlaying();
		}

		public void setPos(string _unique_key, float _cx, float _cy)
		{
			if (this.ARc == null)
			{
				return;
			}
			for (int i = this.ARc.Count - 1; i >= 0; i--)
			{
				M2SndLoopItem.M2SL_Area m2SL_Area = this.ARc[i];
				if (m2SL_Area.unique_key == _unique_key)
				{
					m2SL_Area.cx = _cx;
					m2SL_Area.cy = _cy;
					this.pos_fine = (this.Con.need_pos_fine_item = true);
					return;
				}
			}
		}

		public void setVolume(string _unique_key, float lv)
		{
			if (this.ARc == null)
			{
				return;
			}
			for (int i = this.ARc.Count - 1; i >= 0; i--)
			{
				M2SndLoopItem.M2SL_Area m2SL_Area = this.ARc[i];
				if (m2SL_Area.unique_key == _unique_key)
				{
					m2SL_Area.volume = lv;
					this.pos_fine = (this.Con.need_pos_fine_item = true);
					return;
				}
			}
		}

		public void destruct()
		{
			this.Stop();
			this.ARc = null;
			this.sound_name = "";
		}

		public bool destructed
		{
			get
			{
				return this.ARc == null;
			}
		}

		public readonly M2AreaSndLoop Con;

		private List<M2SndLoopItem.M2SL_Area> ARc;

		private string sound_name;

		private bool load_check = true;

		private bool pos_fine;

		public bool force_monoral;

		public float min_volume;

		private M2SoundPlayerItem MyPlayer;

		public class M2SL_Area
		{
			public string unique_key;

			public float cx;

			public float cy;

			public float listenxr;

			public float listenyr;

			public float areax;

			public float areay;

			public float volume = 1f;

			public IPosLitener PosLsn;
		}
	}
}
