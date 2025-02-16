using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NASStandardInjection : NelEnemyAssist
	{
		public NASStandardInjection(NelEnemy _En, string _posename_prepare, string _posename_inject, string _posename_fast, string _posename_finish, NelAttackInfo _AtkAbsorbPiston, NelAttackInfo _AtkAbsorbPistonFast, NelAttackInfo _AtkAbsorbPistonFinish)
			: base(_En)
		{
			this.posename_prepare = _posename_prepare;
			this.posename_inject = _posename_inject;
			this.posename_fast = _posename_fast;
			this.posename_finish = _posename_finish;
			this.AtkAbsorbPiston = _AtkAbsorbPiston;
			this.AtkAbsorbPistonFast = _AtkAbsorbPistonFast;
			this.AtkAbsorbPistonFinish = _AtkAbsorbPistonFinish;
		}

		public void quitAbsorb()
		{
			this.En.getAnimator().timescale = 1f;
		}

		public bool runAbsorb(ref float t, PR Pr, AbsorbManager Absorb, out bool egg_planted)
		{
			EnemyAnimator animator = this.En.getAnimator();
			egg_planted = false;
			if (t <= 0f)
			{
				t = 0f;
				this.walk_st = 0;
				this.walk_time = 0f;
				if (this.ep_count >= 1000f)
				{
					this.ep_count = 0f;
				}
				else if (this.ep_count >= 40f)
				{
					this.ep_count = 40f;
				}
				else if (this.ep_count >= 20f)
				{
					this.ep_count = 20f;
				}
				else
				{
					this.ep_count /= 2f;
				}
				animator.timescale = 1f;
				this.mode = NASStandardInjection.MODE.PREPARE;
				Absorb.changeTorturePose(this.posename_prepare, false, false, -1, -1);
				Absorb.uipicture_fade_key = this.fadekey_prepare;
				Absorb.no_shuffle_aim = true;
				Absorb.setKirimomiReleaseDir((int)base.aim);
				this.fineWaitTime(true);
			}
			if (this.mode != NASStandardInjection.MODE.AFTER)
			{
				Pr.Ser.Add(SER.DO_NOT_LAY_EGG, 20, 99, false);
			}
			if (this.walk_time <= 0f)
			{
				NelAttackInfo nelAttackInfo = this.AtkAbsorbPiston;
				if (this.ep_count >= 1000f)
				{
					if (t < this.orgasm_maxt)
					{
						this.fineWaitTime(false);
						if (this.mode != NASStandardInjection.MODE.AFTER && t >= this.orgasm_afterfinish_swap_t)
						{
							Absorb.changeTorturePose(this.posename_after, false, false, -1, -1);
							Absorb.uipicture_fade_key = this.fineFade(NASStandardInjection.MODE.AFTER);
							Absorb.breath_key = (Pr.is_alive ? "breath_e" : "breath_aft");
							Absorb.emstate_attach |= UIPictureBase.EMSTATE.PROG1;
							Pr.UP.setFade(Absorb.uipicture_fade_key, UIPictureBase.EMSTATE.NORMAL, false, false, false);
						}
						else
						{
							this.En.applyAbsorbDamageTo(Pr, this.AtkAbsorbPistonFinish ?? nelAttackInfo, false, false, false, 0f, false, null, false, true);
							if (X.XORSP() < 0.5f)
							{
								Pr.DMGE.publishVaginaSplashPiston((base.aim == AIM.R) ? 3.1415927f : 0f, true);
							}
						}
					}
					else
					{
						Absorb.breath_key = null;
						if (Pr.EggCon.isLaying())
						{
							if (X.XORSP() < this.finishratio_laying_base + (float)this.walk_st * this.finishratio_laying_mul)
							{
								return false;
							}
							animator.randomizeFrame(0.5f, 0.5f);
							this.walk_time = 120f;
							return true;
						}
						else
						{
							if (X.XORSP() < this.finishratio_base + (float)this.walk_st * this.finishratio_mul)
							{
								return false;
							}
							this.ep_count = 0f;
						}
					}
				}
				if (this.ep_count < 1000f)
				{
					if (this.mode == NASStandardInjection.MODE.PREPARE || this.mode == NASStandardInjection.MODE.AFTER)
					{
						Absorb.emstate_attach = UIPictureBase.EMSTATE.PROG0;
						Absorb.uipicture_fade_key = this.fineFade(NASStandardInjection.MODE.INJECT);
						Absorb.changeTorturePose(this.posename_inject, false, false, -1, -1);
						Pr.UP.setFade(Absorb.uipicture_fade_key, Absorb.emstate_attach, true, true, false);
					}
					this.ep_count += ((Pr.ep < 400) ? X.NIXP(0.4f, 0.8f) : (X.NIXP(0.4f, 0.6f) * this.ep_add_ratio_ep_high)) * this.ep_add_ratio;
					int num = 1;
					if (this.ep_count >= 65f)
					{
						animator.timescale = 1f;
						Absorb.uipicture_fade_key = this.fineFade(NASStandardInjection.MODE.FINISH);
						Absorb.changeTorturePose(this.posename_finish, false, false, -1, -1);
						NelAttackInfo nelAttackInfo2;
						if ((nelAttackInfo2 = this.AtkAbsorbPistonFinish) == null)
						{
							nelAttackInfo2 = this.AtkAbsorbPistonFast ?? nelAttackInfo;
						}
						nelAttackInfo = nelAttackInfo2;
					}
					else if (this.ep_count >= 40f)
					{
						animator.timescale = 1.6f;
						nelAttackInfo = this.AtkAbsorbPistonFast ?? nelAttackInfo;
						Absorb.uipicture_fade_key = this.fineFade(NASStandardInjection.MODE.FAST);
						if (!base.SpPoseIs(this.posename_fast))
						{
							Absorb.changeTorturePose(this.posename_fast, false, false, -1, -1);
						}
						num = 2;
					}
					else if (this.ep_count >= 20f)
					{
						Absorb.uipicture_fade_key = this.fineFade(NASStandardInjection.MODE.FAST);
						if (!base.SpPoseIs(this.posename_fast))
						{
							Absorb.changeTorturePose(this.posename_fast, false, false, -1, -1);
						}
						nelAttackInfo = this.AtkAbsorbPistonFast ?? nelAttackInfo;
						num = 2;
					}
					else
					{
						num = 3;
						if (!base.SpPoseIs(this.posename_inject))
						{
							Absorb.changeTorturePose(this.posename_inject, false, false, -1, -1);
						}
					}
					this.fineWaitTime(false);
					animator.animReset(X.xors(num), false);
					Vector3 vector = Pr.DMGE.publishVaginaSplashPiston((base.aim == AIM.R) ? 3.1415927f : 0f, false);
					this.En.applyAbsorbDamageTo(Pr, nelAttackInfo, true, false, false, 0f, false, null, false, true);
					this.En.PtcVar("x", (double)Pr.x).PtcVar("y", (double)Pr.y).PtcVar("hit_x", (double)vector.x)
						.PtcVar("hit_y", (double)vector.y)
						.PtcST("player_absorbed_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					if (this.ep_count >= 65f)
					{
						this.orgasm_maxt = X.NIXP(480f, 530f);
						t = 1f;
						this.walk_st++;
						this.ep_count = 1000f;
						if (this.egg_plant_val > 0f && this.eggcateg != PrEggManager.CATEG._ALL && Pr.EggCon.applyEggPlantDamage(this.egg_plant_val, this.eggcateg, true, 1000f) > 0)
						{
							egg_planted = true;
						}
					}
					else if (this.ep_count < 40f && TX.valid(this.snd_breathe_piston))
					{
						this.En.playSndPos(this.snd_breathe_piston, 1);
					}
				}
			}
			this.walk_time -= base.TS;
			return true;
		}

		public void fineWaitTime(bool is_first = false)
		{
			if (this.ep_count >= 1000f)
			{
				this.walk_time = X.NIXP(20f, 60f);
			}
			else if (this.ep_count >= 65f)
			{
				this.walk_time = X.NIXP(10f, 60f);
			}
			else if (this.ep_count >= 40f)
			{
				this.walk_time = X.NIXP(13f, 16f);
			}
			else if (this.ep_count >= 20f)
			{
				this.walk_time = X.NIXP(22f, 25f);
			}
			else
			{
				this.walk_time = X.NIXP(30f, 40f);
			}
			if (is_first)
			{
				this.walk_time *= this.first_time_ratio;
			}
		}

		private string fineFade(NASStandardInjection.MODE _mode)
		{
			this.mode = _mode;
			return this.fineFade();
		}

		private string fineFade()
		{
			switch (this.mode)
			{
			case NASStandardInjection.MODE.INJECT:
				return this.fadekey_inject ?? this.fadekey_prepare;
			case NASStandardInjection.MODE.FAST:
			{
				string text;
				if ((text = this.fadekey_fast) == null)
				{
					text = this.fadekey_inject ?? this.fadekey_prepare;
				}
				return text;
			}
			case NASStandardInjection.MODE.FINISH:
			{
				string text2;
				if ((text2 = this.fadekey_finish) == null && (text2 = this.fadekey_fast) == null)
				{
					text2 = this.fadekey_inject ?? this.fadekey_prepare;
				}
				return text2;
			}
			case NASStandardInjection.MODE.AFTER:
			{
				string text3;
				if ((text3 = this.fadekey_after) == null && (text3 = this.fadekey_finish) == null && (text3 = this.fadekey_fast) == null)
				{
					text3 = this.fadekey_inject ?? this.fadekey_prepare;
				}
				return text3;
			}
			default:
				return this.fadekey_prepare;
			}
		}

		public void SpSetPose(string pose)
		{
			if (TX.valid(pose))
			{
				this.En.SpSetPose(pose, -1, null, false);
			}
		}

		public float ep_add_ratio = 1f;

		public float ep_add_ratio_ep_high = 2.5f;

		private const float EP_COUNT_1 = 20f;

		private const float EP_COUNT_2 = 40f;

		private const float EP_COUNT_ORGASM = 65f;

		private const float EP_COUNT_ORGASM_AFTER = 1000f;

		public float orgasm_afterfinish_swap_t = 288f;

		public string snd_breathe_piston;

		public float orgazm_time_ratio = 1f;

		public float first_time_ratio = 3.5f;

		public PrEggManager.CATEG eggcateg = PrEggManager.CATEG._ALL;

		public float egg_plant_val;

		public float maxt_after;

		public NelAttackInfo AtkAbsorbPiston;

		public NelAttackInfo AtkAbsorbPistonFast;

		public NelAttackInfo AtkAbsorbPistonFinish;

		private float orgasm_maxt = 400f;

		public float ep_count;

		private float walk_time;

		private int walk_st;

		private NASStandardInjection.MODE mode;

		public string posename_prepare;

		public string posename_inject;

		public string posename_fast;

		public string posename_finish;

		public string posename_after;

		public string fadekey_prepare;

		public string fadekey_inject;

		public string fadekey_fast;

		public string fadekey_finish;

		public string fadekey_after;

		public float finishratio_laying_base = 0.11f;

		public float finishratio_laying_mul = 0.04f;

		public float finishratio_base = 0.24f;

		public float finishratio_mul = 0.11f;

		private enum MODE
		{
			PREPARE,
			INJECT,
			FAST,
			FINISH,
			AFTER
		}
	}
}
