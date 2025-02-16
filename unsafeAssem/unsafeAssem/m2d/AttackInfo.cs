using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class AttackInfo
	{
		public virtual AttackInfo CopyFrom(AttackInfo Src)
		{
			this.AttackFrom = Src.AttackFrom;
			this.ndmg = Src.ndmg;
			this.hpdmg0 = Src.hpdmg0;
			this.mpdmg0 = Src.mpdmg0;
			this.hpdmg_current = -1000;
			this.mpdmg_current = -1000;
			this.attr = Src.attr;
			this.burst_vx = Src.burst_vx;
			this.burst_vy = Src.burst_vy;
			this.center_x_ = Src.center_x_;
			this.center_y_ = Src.center_y_;
			this.fix_damage = Src.fix_damage;
			this.centerblur_damage_ratio = Src.centerblur_damage_ratio;
			this._burst_huttobi_thresh = Src._burst_huttobi_thresh;
			this.burst_center = Src.burst_center;
			this.attack_max0 = Src.attack_max0;
			this.damage_randomize_min = Src.damage_randomize_min;
			this.nodamage_time = Src.nodamage_time;
			this.SerDmg = Src.SerDmg;
			this.aim_to_opposite_when_pr_dieing = Src.aim_to_opposite_when_pr_dieing;
			this.hit_r_ = Src.hit_r_;
			this.attack_max_ = Src.attack_max_;
			this.hit_ptcst_name = Src.hit_ptcst_name;
			this.fnHitEffectPre = Src.fnHitEffectPre;
			this.fnHitEffectAfter = Src.fnHitEffectAfter;
			this.snd_name = Src.snd_name;
			this._apply_knockback_current = Src._apply_knockback_current;
			this.knockback_len = Src.knockback_len;
			this.knockback_ratio_p = Src.knockback_ratio_p;
			this.knockback_ratio_t = Src.knockback_ratio_t;
			this.tired_time_to_super_armor = Src.tired_time_to_super_armor;
			this._hitlock_ignore = Src._hitlock_ignore;
			return this;
		}

		public int _hpdmg
		{
			get
			{
				if (this.hpdmg_current != -1000)
				{
					return this.hpdmg_current;
				}
				return this.hpdmg0;
			}
		}

		public int _mpdmg
		{
			get
			{
				if (this.mpdmg_current != -1000)
				{
					return this.mpdmg_current;
				}
				return this.mpdmg0;
			}
		}

		public AttackInfo shuffleHpMpDmg(M2Mover Target, float pbl_hp_ratio = 1f, float pbl_mp_ratio = 1f, int cache_hpdmg = -1000, int cache_mpdmg = -1000)
		{
			if (cache_hpdmg == -1000)
			{
				cache_hpdmg = this.hpdmg0;
			}
			if (cache_mpdmg == -1000)
			{
				cache_mpdmg = this.mpdmg0;
			}
			if (this.centerblur_damage_ratio > 0f && Target != null && this.center_x_ > 0f && this.center_y_ > 0f && this.hit_r_ > 0f)
			{
				float num = 1f - this.centerblur_damage_ratio;
				float num2 = -1f;
				for (int i = -1; i < 8; i++)
				{
					float num3 = Target.x + ((i >= 0) ? (Target.sizex * (float)CAim._XD(i, 1)) : 0f);
					float num4 = Target.y + ((i >= 0) ? (Target.sizey * (float)CAim._YD(i, 1)) : 0f);
					float num5 = X.LENGTHXY2(num3, num4, this.center_x_, this.center_y);
					if (num2 < 0f || num5 < num2)
					{
						num2 = num5;
					}
				}
				num2 = Mathf.Sqrt(num2);
				if (num2 > num * this.hit_r_)
				{
					num2 /= this.hit_r_;
					float num6 = X.NI(1f, 0.125f, X.ZLINE(num2 - num, 1f - num) * this.centerblur_damage_ratio);
					pbl_hp_ratio *= num6;
				}
			}
			this.hpdmg_current = ((cache_hpdmg > 0) ? X.Mx(1, X.IntR((float)cache_hpdmg * X.NIXP(1f, this.damage_randomize_min) * pbl_hp_ratio)) : 0);
			this.mpdmg_current = ((cache_mpdmg > 0) ? X.Mx(1, X.IntR((float)cache_mpdmg * X.NIXP(1f, this.damage_randomize_min) * pbl_mp_ratio)) : 0);
			return this;
		}

		public AttackInfo resetAttackCount()
		{
			this.attack_max_ = this.attack_max0;
			return this;
		}

		public PTCThread playEffect(Map2d Mp)
		{
			if (this.snd_name != "")
			{
				Mp.playSnd(this.snd_name);
			}
			if (this.hit_ptcst_name != "")
			{
				IEffectSetter effect = Mp.getEffect();
				effect.PtcSTsetVar("hit_x", (double)this.hit_x);
				effect.PtcSTsetVar("hit_y", (double)this.hit_y);
				if (this.fnHitEffectPre != null)
				{
					this.fnHitEffectPre(this, null);
				}
				return Mp.PtcST(this.hit_ptcst_name, null, PTCThread.StFollow.NO_FOLLOW);
			}
			return null;
		}

		public PTCThread playEffect(M2Mover Mv)
		{
			float num = X.MMX(Mv.mleft, this.hit_x_, Mv.mright);
			float num2 = X.MMX(Mv.mtop, this.hit_y_, Mv.mbottom);
			if (num != this.hit_x_ || num2 != this.hit_y_)
			{
				this.HitXy(num, num2, false);
			}
			if (!(Mv is M2Attackable))
			{
				return this.playEffect(Mv.Mp);
			}
			M2Attackable m2Attackable = Mv as M2Attackable;
			if (this.snd_name != "")
			{
				m2Attackable.playSndPos(this.snd_name, Mv.x, Mv.y, PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.NO_FOLLOW, null);
			}
			if (this.hit_ptcst_name != "")
			{
				m2Attackable.PtcVar("hit_x", (double)this.hit_x);
				m2Attackable.PtcVar("hit_y", (double)this.hit_y);
			}
			if (this.fnHitEffectPre != null)
			{
				this.fnHitEffectPre(this, Mv as IM2RayHitAble);
			}
			if (!(this.hit_ptcst_name != ""))
			{
				return null;
			}
			return m2Attackable.PtcST(this.hit_ptcst_name, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
		}

		public bool reduceAttackCount()
		{
			if (this.attack_max_ > 0)
			{
				this.attack_max_--;
			}
			return this.attack_max_ == 0;
		}

		public bool isPenetrateAttr(M2Attackable Atk)
		{
			return M2NoDamageManager.isPenetrateDefault(this.ndmg) || (this.attr == MGATTR.PRESS_PENETRATE && !Atk.isNoDamageActive(NDMG.PRESSDAMAGE)) || this.attr == MGATTR.WORM || this.ndmg == NDMG.MAPDAMAGE_THUNDER || this.ndmg == NDMG.MAPDAMAGE_LAVA || this.ndmg == NDMG.GRAB;
		}

		public bool isAbsorbAttr()
		{
			return this.attr == MGATTR.ABSORB || this.attr == MGATTR.ABSORB_V || this.attr == MGATTR.ACME;
		}

		public virtual bool isPenetrateAbsorb()
		{
			return this.attr == MGATTR.PRESS_PENETRATE || this.attr == MGATTR.PRESS || this.attr == MGATTR.WORM;
		}

		public AttackInfo CenterXy(float x, float y, float hit_r)
		{
			this.center_x_ = x;
			this.center_y_ = y;
			this.hit_r_ = hit_r;
			if (this.hit_r_ == 0f)
			{
				this.hit_x_ = this.center_x_;
				this.hit_y_ = this.center_y_;
			}
			return this;
		}

		public AttackInfo HitXy(float x, float y, bool abs = false)
		{
			if (abs)
			{
				this.hit_x_ = x;
				this.hit_y_ = y;
				return this;
			}
			if (this.hit_r_ == 0f)
			{
				this.hit_x_ = this.center_x_;
				this.hit_y_ = this.center_y_;
				return this;
			}
			this.hit_x_ = x;
			this.hit_y_ = y;
			if (this.hit_r_ < 0f)
			{
				this.center_x_ = x;
				this.center_y_ = y;
			}
			else
			{
				this.hit_x_ -= this.center_x_;
				this.hit_y_ -= this.center_y_;
				float num = this.hit_x_ * this.hit_x_ + this.hit_y_ * this.hit_y_;
				float num2 = this.hit_r_ * this.hit_r_;
				if (num > num2)
				{
					num = X.Mn(1f, this.hit_r_ / X.Mx(X.Abs(this.hit_x_), X.Abs(this.hit_y_)));
					this.hit_x_ *= num;
					this.hit_y_ *= num;
				}
				this.hit_x_ += this.center_x_;
				this.hit_y_ += this.center_y_;
			}
			return this;
		}

		public int attack_max
		{
			get
			{
				return this.attack_max_;
			}
		}

		public float center_x
		{
			get
			{
				return this.center_x_;
			}
		}

		public float center_y
		{
			get
			{
				return this.center_y_;
			}
		}

		public float hit_x
		{
			get
			{
				return this.hit_x_;
			}
		}

		public float hit_y
		{
			get
			{
				return this.hit_y_;
			}
		}

		public virtual bool isPhysical()
		{
			MGATTR mgattr = this.attr;
			return mgattr == MGATTR.NORMAL || mgattr - MGATTR.GRAB <= 4U || mgattr == MGATTR.EATEN;
		}

		public M2Attackable AttackFrom;

		public NDMG ndmg;

		public int hpdmg_current = -1000;

		public int mpdmg_current = -1000;

		public int hpdmg0;

		public int mpdmg0;

		public MGATTR attr;

		public float burst_vx;

		public float burst_vy;

		public float _burst_huttobi_thresh;

		public bool fix_damage;

		public float burst_center;

		public int attack_max0 = -1;

		public float damage_randomize_min = 0.85f;

		public float centerblur_damage_ratio;

		public int nodamage_time = 4;

		public FlagCounter<SER> SerDmg;

		public float aim_to_opposite_when_pr_dieing = 0.18f;

		private float center_x_;

		private float center_y_;

		private float hit_x_;

		private float hit_y_;

		private float hit_r_ = -1f;

		private int attack_max_ = -2;

		public string hit_ptcst_name = "";

		public AttackInfo.FnHitPre fnHitEffectPre;

		public AttackInfo.FnHitAfter fnHitEffectAfter;

		public string snd_name = "";

		public bool _apply_knockback_current = true;

		public float knockback_len;

		public float knockback_ratio_p;

		public float knockback_ratio_t = 1f;

		public float tired_time_to_super_armor;

		public bool _hitlock_ignore;

		private const float KNOCKBACK_MAXT = 19f;

		public delegate void FnHitPre(AttackInfo Atk, IM2RayHitAble Target);

		public delegate void FnHitAfter(AttackInfo Atk, IM2RayHitAble Target, HITTYPE touched_hittpe);
	}
}
