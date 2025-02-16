using System;
using System.Collections.Generic;
using Better;
using XX;

namespace m2d
{
	public sealed class PtcHolder : RBase<PtcHolderMem>
	{
		public bool first_ver
		{
			get
			{
				return this.first_ver_;
			}
			set
			{
				if (this.first_ver == value)
				{
					return;
				}
				this.first_ver_ = value;
			}
		}

		public PtcHolder(IEfPInteractale _Con, int capacity = 4, int hash_capacity = 4)
			: base(capacity, true, false, false)
		{
			this.Con = _Con;
			this.OSoundId = new BDic<string, uint>(4);
			this.Hash = new HashP(hash_capacity);
			this.VarP = new VariableP(8);
		}

		public void endS()
		{
			this.killPtc(true);
			this.first_ver = false;
		}

		public override PtcHolderMem Create()
		{
			return new PtcHolderMem(this);
		}

		public PtcHolder Var(string var_name, double val)
		{
			this.VarP.Add(var_name, val);
			return this;
		}

		public PtcHolder Var(string var_name, string val)
		{
			this.VarP.Add(var_name, val);
			return this;
		}

		public PTCThread PtcST(string ptc_key, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			return this.PtcSTTimeFixed(ptc_key, 0f, hold, follow, false);
		}

		public PTCThread PtcSTT(string ptc_key, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			return this.PtcSTTimeFixed(ptc_key, 0f, hold, follow, true);
		}

		public PTCThread PtcST(STB Stb, int char_i, int char_len, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			return this.PtcSTTimeFixed(this.Hash.Get(Stb, char_i, char_len), 0f, hold, follow, false);
		}

		public PTCThread PtcSTTimeFixed(string ptc_key, float factor, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, bool to_top = false)
		{
			if (this.Mp == null)
			{
				return null;
			}
			this.first_ver = false;
			PtcHolderMem ptcBuf = PtcHolder.PtcBuf;
			PtcHolder.PtcBuf = base.Pop(8);
			PtcHolder.PtcBuf.hold = hold;
			PtcHolder.PtcBuf.follow_for_sound = follow;
			PtcHolder.PtcBuf.slow_factor = factor;
			PTCThread ptcthread = (to_top ? this.Mp.getEffectTop() : this.Mp.getEffect()).PtcST(ptc_key, this.Con, follow, this.VarP);
			if (ptcthread != null)
			{
				if (PtcHolder.PtcBuf == null)
				{
					PtcHolder.PtcBuf = base.Pop(8);
					PtcHolder.PtcBuf.hold = hold;
					PtcHolder.PtcBuf.follow_for_sound = follow;
				}
				if (factor > 0f)
				{
					this.has_slow_factor_ = 1;
				}
				PtcHolder.PtcBuf.Set(ptcthread, PtcHolder.PtcBuf.follow_for_sound);
				this.need_update_t_ = 5;
			}
			else
			{
				this.LEN = X.Mx(this.LEN - 1, 0);
			}
			PtcHolder.PtcBuf = ptcBuf;
			return ptcthread;
		}

		public void changeCurrentBufferFollow(PTCThread.StFollow follow)
		{
			if (PtcHolder.PtcBuf != null)
			{
				PtcHolder.PtcBuf.follow_for_sound = follow;
			}
		}

		public M2SoundPlayerItem playSndPos(PTCThread Thread, int index, int index_len, float x, float y, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, byte voice_priority = 1)
		{
			bool flag;
			bool flag2;
			StringKey soundKey = Thread.getSoundKey(index, index_len, out flag, out flag2);
			return this.playSndPos(soundKey, x, y, (flag ? PtcHolder.PTC_HOLD.ACT : (flag2 ? PtcHolder.PTC_HOLD.NORMAL : PtcHolder.PTC_HOLD.NO_HOLD)) | hold, follow, Thread, voice_priority);
		}

		public M2SoundPlayerItem playSndPos(string cue_key, float x, float y, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, PTCThread Thread = null, byte voice_priority = 1)
		{
			STB stb = TX.PopBld(cue_key, 0);
			bool flag;
			bool flag2;
			StringKey stringKey = PTCThread.fixSoundKey(stb, out flag, out flag2, this.Hash);
			TX.ReleaseBld(stb);
			return this.playSndPos(stringKey, x, y, (flag ? PtcHolder.PTC_HOLD.ACT : (flag2 ? PtcHolder.PTC_HOLD.NORMAL : PtcHolder.PTC_HOLD.NO_HOLD)) | hold, follow, Thread, voice_priority);
		}

		public M2SoundPlayerItem playSndPos(STB Stb, int char_i, int char_len, float x, float y, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, byte voice_priority = 1)
		{
			return this.playSndPos(this.Hash.Get(Stb, char_i, char_len), x, y, hold, follow, null, voice_priority);
		}

		public M2SoundPlayerItem playSndPos(StringKey cue_key, float x, float y, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, PTCThread Thread = null, byte voice_priority = 1)
		{
			string soundKey = this.getSoundKey(follow, hold);
			M2SoundPlayerItem m2SoundPlayerItem = this.Mp.playSnd(cue_key, soundKey, x, y, voice_priority);
			if (m2SoundPlayerItem != null && hold != PtcHolder.PTC_HOLD.NO_HOLD)
			{
				PtcHolderMem ptcHolderMem = base.Pop(16).Set(m2SoundPlayerItem, follow);
				ptcHolderMem.slow_factor = 0f;
				ptcHolderMem.hold = hold;
				this.OSoundId[soundKey] = m2SoundPlayerItem.id;
				this.need_update_t_ = 5;
				if (follow != PTCThread.StFollow.NO_FOLLOW)
				{
					ptcHolderMem.run(0f);
				}
				if (Thread != null && (hold & PtcHolder.PTC_HOLD._NO_KILL) == PtcHolder.PTC_HOLD.NO_HOLD)
				{
					Thread.StockSound(m2SoundPlayerItem);
				}
			}
			return m2SoundPlayerItem;
		}

		public M2SndInterval playSndInterval(string cue_key, float intv, float x, float y, int playcount = 128, bool killable = true, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW, PTCThread Thread = null)
		{
			STB stb = TX.PopBld(cue_key, 0);
			bool flag;
			bool flag2;
			StringKey stringKey = PTCThread.fixSoundKey(stb, out flag, out flag2, this.Hash);
			TX.ReleaseBld(stb);
			return this.playSndInterval(stringKey, intv, x, y, playcount, killable || flag2, (flag ? PtcHolder.PTC_HOLD.ACT : (flag2 ? PtcHolder.PTC_HOLD.NORMAL : PtcHolder.PTC_HOLD.NO_HOLD)) | hold, _follow, Thread);
		}

		public M2SndInterval playSndInterval(PTCThread Thread, int index, int index_len, float intv, float x, float y, int playcount = 128, bool killable = true, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW)
		{
			bool flag;
			bool flag2;
			StringKey soundKey = Thread.getSoundKey(index, index_len, out flag, out flag2);
			return this.playSndInterval(soundKey, intv, x, y, playcount, killable, (flag ? PtcHolder.PTC_HOLD.ACT : (flag2 ? PtcHolder.PTC_HOLD.NORMAL : PtcHolder.PTC_HOLD.NO_HOLD)) | hold, follow, Thread);
		}

		public M2SndInterval playSndInterval(StringKey cue_key, float intv, float x, float y, int playcount = 128, bool killable = true, PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW, PTCThread Thread = null)
		{
			string soundKey = this.getSoundKey(_follow, hold);
			M2SndInterval m2SndInterval = this.Mp.M2D.Snd.createInterval(this.Con.getSoundKey(), cue_key, intv, null, 0f, playcount);
			if (m2SndInterval != null)
			{
				this.OSoundId[soundKey] = m2SndInterval.id;
				if (hold != PtcHolder.PTC_HOLD.NO_HOLD)
				{
					PtcHolderMem ptcHolderMem = base.Pop(64).Set(m2SndInterval, _follow);
					this.need_update_t_ = 5;
					ptcHolderMem.hold = hold | ((!killable) ? PtcHolder.PTC_HOLD._NO_KILL : PtcHolder.PTC_HOLD.NO_HOLD);
					if (Thread != null && (hold & PtcHolder.PTC_HOLD._NO_KILL) == PtcHolder.PTC_HOLD.NO_HOLD)
					{
						Thread.StockSound(m2SndInterval);
					}
				}
			}
			return m2SndInterval;
		}

		private string getSoundKey(PTCThread.StFollow _follow, PtcHolder.PTC_HOLD hold)
		{
			STB stb = TX.PopBld(null, 0);
			int num = (int)(hold & PtcHolder.PTC_HOLD._ALL);
			string soundKey = this.Con.getSoundKey();
			foreach (KeyValuePair<string, uint> keyValuePair in this.OSoundId)
			{
				stb.Set(keyValuePair.Key);
				int num2;
				if (stb.NmIs((double)_follow, soundKey.Length + 1, out num2, -1) && stb.NmIs((double)num, num2 + 1, out num2, -1))
				{
					TX.ReleaseBld(stb);
					return keyValuePair.Key;
				}
			}
			stb.Set(soundKey);
			stb += '.';
			STB stb2 = stb;
			int num3 = (int)_follow;
			stb2.Add(num3.ToString());
			stb += '.';
			stb.Add(hold.ToString());
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			return text;
		}

		public bool isSoundActive(M2SoundPlayerItem Snd)
		{
			uint num;
			return this.OSoundId.TryGetValue(Snd.key, out num) && Snd.id == num;
		}

		public float getEffectSlowFactor(PTCThread Thread, EffectItem Ef)
		{
			if (PtcHolder.PtcBuf != null)
			{
				return PtcHolder.PtcBuf.slow_factor;
			}
			if (this.has_slow_factor)
			{
				for (int i = this.LEN - 1; i >= 0; i--)
				{
					PtcHolderMem ptcHolderMem = this.AItems[i];
					if (ptcHolderMem.isActiveSt(Thread))
					{
						return ptcHolderMem.slow_factor;
					}
				}
			}
			return 0f;
		}

		public void killPtc(bool kill_only_reader = false)
		{
			int num = -1;
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PtcHolderMem ptcHolderMem = this.AItems[i];
				if (ptcHolderMem == PtcHolder.PtcBuf)
				{
					num = i;
				}
				else
				{
					ptcHolderMem.kill(kill_only_reader);
				}
			}
			this.has_slow_factor_ = 0;
			if (num >= 0)
			{
				if (num > 0)
				{
					ref PtcHolderMem ptr = ref this.AItems[0];
					PtcHolderMem[] aitems = this.AItems;
					int num2 = num;
					PtcHolderMem ptcHolderMem2 = this.AItems[num];
					PtcHolderMem ptcHolderMem3 = this.AItems[0];
					ptr = ptcHolderMem2;
					aitems[num2] = ptcHolderMem3;
				}
				if (PtcHolder.PtcBuf.slow_factor > 0f)
				{
					this.has_slow_factor_ = 1;
				}
				this.LEN = 1;
			}
			else
			{
				this.LEN = 0;
			}
			this.need_update_t_ = 0;
		}

		public void killPtc(string s, bool kill_only_reader = false)
		{
			int num = this.indexOfPtcST(s);
			if (num >= 0)
			{
				this.AItems[num].kill(kill_only_reader);
				base.clearAt(num);
				return;
			}
		}

		public void checkUpdate(int fcnt)
		{
			if (this.need_update_t_ >= 5)
			{
				this.need_update_t_ = 0;
				PtcHolder.run_mode = PtcHolder.RUN_MODE.UPDATE;
				base.run((float)fcnt);
				return;
			}
			if (this.need_update_t_ > 0)
			{
				this.need_update_t_ += fcnt;
			}
		}

		public int indexOfPtcST(string st_key)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PtcHolderMem ptcHolderMem = this.AItems[i];
				if (ptcHolderMem.isActiveSt(null) && ptcHolderMem.St.key == st_key)
				{
					return i;
				}
			}
			return -1;
		}

		public void killPtc(PtcHolder.PTC_HOLD _hold)
		{
			PtcHolder.run_mode = PtcHolder.RUN_MODE.KILL;
			PtcHolder.killing_hold = _hold;
			base.run(0f);
			PtcHolder.run_mode = PtcHolder.RUN_MODE.UPDATE;
			this.need_update_t_ = 0;
		}

		public int need_update_t
		{
			get
			{
				return this.need_update_t_;
			}
			set
			{
				this.need_update_t_ = X.Mx(value, this.need_update_t_);
			}
		}

		public bool has_slow_factor
		{
			get
			{
				if (this.has_slow_factor_ == 2)
				{
					this.has_slow_factor_ = 0;
					for (int i = this.LEN - 1; i >= 0; i--)
					{
						PtcHolderMem ptcHolderMem = this.AItems[i];
						if (ptcHolderMem.isActiveSt(null) && ptcHolderMem.slow_factor > 0f)
						{
							this.has_slow_factor_ = 1;
							break;
						}
					}
				}
				return this.has_slow_factor_ > 0;
			}
			set
			{
				this.has_slow_factor_ = (value ? 1 : 2);
			}
		}

		public Map2d Mp
		{
			get
			{
				return M2DBase.Instance.curMap;
			}
		}

		public M2DBase M2D
		{
			get
			{
				return M2DBase.Instance;
			}
		}

		public M2SoundPlayer SndCon
		{
			get
			{
				return M2DBase.Instance.Snd;
			}
		}

		public static PtcHolder.PTC_HOLD killing_hold;

		public static PtcHolder.RUN_MODE run_mode;

		private int need_update_t_;

		private const int T_NEED_UPDATE = 5;

		public readonly IEfPInteractale Con;

		private static PtcHolderMem PtcBuf;

		public BDic<string, uint> OSoundId;

		private bool first_ver_;

		private HashP Hash;

		private byte has_slow_factor_;

		private VariableP VarP;

		public enum PTC_HOLD
		{
			NO_HOLD,
			NORMAL,
			ACT,
			CONFUSE = 4,
			_ALL = 7,
			_NO_KILL
		}

		public enum RUN_MODE
		{
			UPDATE,
			KILL,
			KILL_ONLY_READER
		}
	}
}
