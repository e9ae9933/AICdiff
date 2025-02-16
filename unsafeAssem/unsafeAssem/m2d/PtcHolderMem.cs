using System;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class PtcHolderMem : IRunAndDestroy
	{
		public PtcHolderMem(PtcHolder _Hld)
		{
			this.Hld = _Hld;
		}

		public PtcHolderMem Set(PTCThread _St, PTCThread.StFollow _follow)
		{
			this.St = _St;
			this.Snd = null;
			this.St.follow = _follow;
			this.ptc_id = this.St.id;
			return this;
		}

		public PtcHolderMem Set(M2SoundPlayerItem _Snd, PTCThread.StFollow _follow)
		{
			this.St = null;
			this.Snd = _Snd;
			this.ptc_id = this.Snd.id;
			this.follow_for_sound = _follow;
			return this;
		}

		public bool run(float fcnt)
		{
			if (!this.isActive())
			{
				return false;
			}
			if (PtcHolder.run_mode != PtcHolder.RUN_MODE.UPDATE && (this.hold & PtcHolder.PTC_HOLD.NO_HOLD) == PtcHolder.PTC_HOLD.NO_HOLD && (this.hold & PtcHolder.killing_hold) != PtcHolder.PTC_HOLD.NO_HOLD)
			{
				this.kill(PtcHolder.run_mode == PtcHolder.RUN_MODE.KILL_ONLY_READER);
				return false;
			}
			if (this.Snd != null && this.follow_for_sound != PTCThread.StFollow.NO_FOLLOW)
			{
				this.Hld.need_update_t = 1;
				Vector3 vector;
				if (this.Hld.Con.getEffectReposition(null, this.follow_for_sound, fcnt, out vector))
				{
					this.Hld.SndCon.finePosition(this.Snd, this.Snd.key, vector.x, vector.y);
				}
			}
			return true;
		}

		public void destruct()
		{
		}

		public void kill(bool kill_only_reader)
		{
			if (this.slow_factor > 0f)
			{
				this.Hld.has_slow_factor = false;
				this.slow_factor = 0f;
			}
			if (this.St != null && this.St.isActive(this.ptc_id))
			{
				this.St.kill(kill_only_reader || this.do_not_kill);
			}
			this.St = null;
			if (this.Snd != null && this.Snd.id == this.ptc_id && !kill_only_reader && !this.do_not_kill)
			{
				this.Snd.Stop();
			}
			this.Snd = null;
		}

		public bool isActive()
		{
			if (this.St != null)
			{
				return this.St.isActive(this.ptc_id);
			}
			return this.Snd != null && this.Snd.id == this.ptc_id;
		}

		public bool isActiveSt(PTCThread Thread = null)
		{
			return (Thread == null || Thread == this.St) && this.St != null && this.isActive();
		}

		public bool do_not_kill
		{
			get
			{
				return (this.hold & PtcHolder.PTC_HOLD._NO_KILL) > PtcHolder.PTC_HOLD.NO_HOLD;
			}
		}

		public readonly PtcHolder Hld;

		public PTCThread St;

		public M2SoundPlayerItem Snd;

		public uint ptc_id;

		public float slow_factor;

		public PtcHolder.PTC_HOLD hold = PtcHolder.PTC_HOLD.NORMAL;

		public PTCThread.StFollow follow_for_sound;
	}
}
