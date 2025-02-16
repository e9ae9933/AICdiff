using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class OverDriveManager
	{
		public OverDriveManager(NelEnemy _En, int _od_size_pixel_x, int _od_size_pixel_y)
		{
			this.En = _En;
			this.od_size_pixel_x = _od_size_pixel_x;
			this.od_size_pixel_y = _od_size_pixel_y;
		}

		public void activate(bool change_state = false)
		{
			float num = (change_state ? 0.666f : 0.75f);
			float num2 = (change_state ? 180f : 59.4f);
			PostEffect.IT.setSlow(num2, num, 0);
			this.En.defineParticlePreVariable();
			this.En.PtcHld.PtcSTTimeFixed("enemyod_init", 0.66f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C, false);
			if (change_state)
			{
				PostEffect.IT.setPEfadeinout(POSTM.ENEMY_OVERDRIVE_APPEAR, num2, 20f, 0.3f, 0);
				PostEffect.IT.setPEfadeinout(POSTM.ENEMY_OVERDRIVE_APPEAR, num2 * 0.25f, 5f, 1f, (int)(num2 * 0.75f));
			}
			else
			{
				PostEffect.IT.setPEfadeinout(POSTM.ENEMY_OVERDRIVE_APPEAR, num2 * 0.85f, 0f, 0.6f, 0);
			}
			this.initted = true;
			this.thunder_overdrive_t = 0f;
			this.En.M2D.Cam.Qu.Vib(8f, 13f, 1f, 0);
		}

		public bool runOverDriveActivate(ref float t, ref float walk_time, ref int walk_st)
		{
			float transform_duration = this.transform_duration;
			if (t <= 0f)
			{
				t = 0f;
				walk_time = (float)(walk_st = 0);
				this.En.defineParticlePreVariable();
				this.En.PtcST("enemyod_init_charge", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
				if (this.enlarge_on_transforming)
				{
					float enlarge_anim_scale_max = this.En.enlarge_anim_scale_max;
					this.En.TeCon.setEnlargeTransform((float)this.od_size_pixel_x / this.CLENM / (this.En.sizex0 * enlarge_anim_scale_max), (float)this.od_size_pixel_y / this.CLENM / (this.En.sizey0 * enlarge_anim_scale_max), 180f, 10f, 0);
				}
				this.En.TeCon.setColorBlink(180f, 5f, 1f, 0, 0);
				this.En.M2D.Cam.Qu.HandShake(135f, 45f, 10f, 0);
			}
			if (t >= transform_duration * 0.75f)
			{
				float num = (t - transform_duration * 0.75f) / 0.666f;
				float num2 = 4f;
				if ((int)(num / num2) > walk_st)
				{
					int num3 = walk_st;
					walk_st = num3 + 1;
					if (num3 == 0)
					{
						this.En.defineParticlePreVariable();
						this.En.PtcHld.PtcST("enemyod_init_charge2", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
						this.En.M2D.Cam.Qu.Vib(1f, transform_duration * 0.3f, 8f, 2);
					}
					this.En.defineParticlePreVariable();
					this.En.PtcHld.PtcST("enemyod_init_charge_lines", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			return t < transform_duration;
		}

		public float transform_duration
		{
			get
			{
				return 119f;
			}
		}

		public M2DBase M2D
		{
			get
			{
				return this.En.M2D;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.En.Mp;
			}
		}

		public float CLENM
		{
			get
			{
				return this.En.CLENM;
			}
		}

		public void appear()
		{
			this.En.killPtc();
			this.En.TeCon.clear();
			this.En.M2D.Cam.Qu.SinV(12f, 50f, 0f, 0);
			this.En.M2D.Cam.Qu.Vib(10f, 18f, 0f, 0);
			this.Mp.setET("flash", 4f, 50f, 0.35f, 0, 0);
			this.En.PtcST("enemyod_appear", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			(this.M2D as NelM2DBase).FlagRain.Add("OD_" + this.En.key);
			this.eff_t = 0f;
			this.SndNicha = this.Mp.M2D.Snd.Environment.AddLoop("areasnd_od_walking", this.En.key, this.En.x, this.En.y, 9f, 9f, 6f, 3f, this.En);
			this.snd_t = 0f;
			this.thunder_overdrive_t = 0f;
			if (this.SndNicha != null)
			{
				this.SndNicha.Vol(this.En.key, 0f);
			}
		}

		public static void initGrowl(NelEnemy En)
		{
			En.PtcST("overdrive_growl", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
			En.M2D.Cam.Qu.Vib(11f, 13f, 8f, 0);
			En.M2D.Cam.Qu.Vib(3f, 80f, 1f, 13);
		}

		public void quitOverDrive(bool immediate = false)
		{
			(this.M2D as NelM2DBase).FlagRain.Rem("OD_" + this.En.key);
			if (immediate && this.SndNicha != null)
			{
				this.SndNicha.destruct();
				this.SndNicha = null;
			}
			else
			{
				this.volumeActivate(false);
			}
			if (this.initted)
			{
				this.initted = false;
			}
		}

		public void addMpFromMana(float val)
		{
		}

		public void runPre(float fcnt)
		{
			if (this.initted)
			{
				uint ran = X.GETRAN2(IN.totalframe + this.En.index * 3, 4 + this.En.index % 6);
				this.eff_t -= fcnt;
				if (this.eff_t <= 0f)
				{
					if (this.En.M2D.Cam.isCoveringMp(this.En.mleft, this.En.mtop, this.En.mright, this.En.mbottom, 60f))
					{
						this.En.PtcVar("sizex", (double)this.En.sizex).PtcVar("sizey", (double)this.En.sizey).PtcST("overdrive_walk", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					int eyeCount = this.En.Anm.getEyeCount();
					if (eyeCount > 0)
					{
						Vector2 eyeMapPos = this.En.Anm.getEyeMapPos(X.xors(eyeCount));
						this.En.Mp.PtcN("overdrive_eye_daia", eyeMapPos.x, eyeMapPos.y, X.NIXP(4f, 8f), X.IntR(X.NIXP(2f, 4f)), (int)X.NIXP(4f, 14f));
					}
					this.eff_t = 18f + X.RAN(ran, 644) * 11f;
				}
			}
			else if (this.thunder_overdrive_t > 0f && !this.En.disappearing)
			{
				this.thunder_overdrive_t -= fcnt;
				if (this.thunder_overdrive_t <= 0f)
				{
					this.thunder_overdrive_t = 5f;
					if (true && this.En.initOverDrive(false, true))
					{
						this.En.PtcVar("delay", 7.0).PtcST("weather_thunder_hit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						PostEffect.IT.setPEfadeinout(POSTM.ENEMY_OVERDRIVE_APPEAR, 30f, 7f, 1f, -30);
						this.En.getAI().delay += 20f;
					}
				}
			}
			if (this.SndNicha != null)
			{
				if (this.snd_t >= 0f)
				{
					if (this.snd_t < 60f)
					{
						this.snd_t += fcnt;
						this.SndNicha.Vol(this.En.key, X.ZLINE(this.snd_t, 60f));
						return;
					}
				}
				else if (this.snd_t > -60f)
				{
					this.snd_t -= fcnt;
					this.SndNicha.Vol(this.En.key, X.ZLINE(60f + this.snd_t, 60f));
					if (this.snd_t <= -60f && !this.En.isOverDrive())
					{
						this.SndNicha.destruct();
						this.SndNicha = null;
					}
				}
			}
		}

		public void volumeActivate(bool f)
		{
			if (this.SndNicha == null)
			{
				return;
			}
			if (f)
			{
				if (this.snd_t < 0f)
				{
					this.snd_t = X.Mx(0f, 60f + this.snd_t);
					return;
				}
			}
			else if (this.snd_t >= 0f)
			{
				this.snd_t = X.Mn(-1f, -60f + this.snd_t);
			}
		}

		public bool thunder_overdrive
		{
			get
			{
				return this.thunder_overdrive_t > 0f;
			}
			set
			{
				if (value)
				{
					this.thunder_overdrive_t = 40f;
				}
			}
		}

		public bool near_overdrive
		{
			get
			{
				return this.pre_overdrive || this.thunder_overdrive_t > 0f;
			}
		}

		public readonly NelEnemy En;

		public readonly int od_size_pixel_x;

		public readonly int od_size_pixel_y;

		private const float slow_maxt = 180f;

		private const float slow_ratio = 0.666f;

		public float enlarge_od_anim_scale_min = 0.875f;

		public float enlarge_od_anim_scale_max = 1f;

		public int od_killed_mana_splash = 90;

		private bool initted;

		private float eff_t;

		private M2SndLoopItem SndNicha;

		private float snd_t;

		private float thunder_overdrive_t;

		private const float SND_FADE_T = 60f;

		public NelItem DropItem;

		public bool pre_overdrive;

		public bool enlarge_on_transforming = true;
	}
}
