using System;
using m2d;
using XX;

namespace nel
{
	public class ActiveSelector : IAnimListener
	{
		public ActiveSelector(string _bgmhalf_key, string _select_init_snd_cue)
		{
			this.bgmhalf_key = _bgmhalf_key;
			this.FD_drawEd = new M2DrawBinder.FnEffectBind(this.drawEd);
			this.select_init_snd_cue = _select_init_snd_cue;
		}

		public void playSnd(string _cue = null)
		{
			this.playSnd(ref this.PreSnd, ref this.pre_snd_cue, _cue);
		}

		protected void playSnd(ref M2SoundPlayerItem PreSnd, ref string pre_snd_cue, string _cue)
		{
			if (PreSnd != null && PreSnd.current_cue == pre_snd_cue)
			{
				PreSnd.Stop();
			}
			PreSnd = null;
			pre_snd_cue = null;
			if (_cue != null)
			{
				if ((M2DBase.Instance as NelM2DBase).GM.isActive())
				{
					SND.Ui.play(_cue, false);
					return;
				}
				M2MoverPr keyPr = M2DBase.Instance.curMap.getKeyPr();
				M2SoundPlayerItem m2SoundPlayerItem = ((keyPr != null) ? M2DBase.Instance.Snd.playAt(_cue, this.bgmhalf_key, keyPr.x, keyPr.y, SndPlayer.SNDTYPE.SND, 1) : M2DBase.Instance.Snd.play(_cue));
				if (m2SoundPlayerItem != null)
				{
					PreSnd = m2SoundPlayerItem;
					pre_snd_cue = _cue;
				}
			}
		}

		public virtual void deactivateEffect(bool clear_efdraw = true)
		{
			if (clear_efdraw && this.EdDraw != null)
			{
				this.EdDraw.destruct();
			}
			this.EdDraw = null;
			if (this.PeSlow != null)
			{
				this.PeSlow.deactivate(false);
				this.PeSlow = null;
			}
			if (this.PeDarken != null)
			{
				this.PeDarken.deactivate(false);
				this.PeDarken = null;
			}
			BGM.remHalfFlag(this.bgmhalf_key);
			if (this.PeSnd != null)
			{
				this.PeSnd.deactivate(false);
				this.PeSnd = null;
			}
		}

		protected void selectInit(M2DrawBinder Ed, float stop_time_fade, float stop_time_hold, float slow_level)
		{
			if (Ed != null)
			{
				BGM.addHalfFlag(this.bgmhalf_key);
				this.EdDraw = Ed;
				if (stop_time_fade == 0f)
				{
					this.PeSlow = PostEffect.IT.setSlow(stop_time_hold, slow_level, -5);
				}
				else
				{
					this.PeSlow = PostEffect.IT.setSlowFading(stop_time_fade, stop_time_hold, slow_level, -5);
				}
				PostEffect.IT.addTimeFixedEffect(this.PeDarken = PostEffect.IT.setPE(POSTM.MAGICSELECT, 30f, 0.5f, 1), 1f);
				PostEffect.IT.addTimeFixedEffect(this.PeSnd = PostEffect.IT.setPE(POSTM.BGM_LOWER, 15f, 1f, 1), 1f);
				PostEffect.IT.addTimeFixedEffect(this, 1f);
				this.playSnd(this.select_init_snd_cue);
			}
		}

		public bool updateAnimator(float f)
		{
			if (this.EdDraw == null)
			{
				return false;
			}
			this.EdDraw.t += f;
			return true;
		}

		public virtual void runPE()
		{
			if (this.PeSlow != null)
			{
				this.PeSlow.fine(120);
			}
			if (this.PeDarken != null)
			{
				this.PeDarken.fine(120);
			}
			if (this.PeSnd != null)
			{
				this.PeSnd.fine(120);
			}
		}

		protected virtual bool drawEd(EffectItem Ef, M2DrawBinder Ed)
		{
			if (Ed != this.EdDraw && this.EdDraw != null)
			{
				return false;
			}
			if (this.PeDarken != null)
			{
				this.PeDarken.fine(120);
			}
			if (this.PeSnd != null)
			{
				this.PeSnd.fine(120);
			}
			return true;
		}

		public virtual void deactivate()
		{
			this.deactivateEffect(true);
			this.playSnd(null);
		}

		protected M2DrawBinder EdDraw;

		protected PostEffectItem PeSlow;

		protected PostEffectItem PeDarken;

		protected PostEffectItem PeSnd;

		protected M2SoundPlayerItem PreSnd;

		protected string pre_snd_cue;

		public readonly string bgmhalf_key;

		public readonly string select_init_snd_cue;

		protected M2DrawBinder.FnEffectBind FD_drawEd;
	}
}
