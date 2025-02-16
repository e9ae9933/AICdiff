using System;
using m2d;
using XX;

namespace nel
{
	public class NelAttackInfo : NelAttackInfoBase
	{
		public float absorb_replace_prob_both
		{
			set
			{
				this.absorb_replace_prob_ondamage = value;
				this.absorb_replace_prob = value;
			}
		}

		public bool is_grab_attack
		{
			get
			{
				return X.Mn(this.absorb_replace_prob, this.absorb_replace_prob_ondamage) >= 100f;
			}
			set
			{
				if (value)
				{
					this.absorb_replace_prob_both = 100f;
					this.huttobi_ratio = -1000f;
				}
			}
		}

		public bool is_penetrate_grab_attack
		{
			get
			{
				return X.Mn(this.absorb_replace_prob, this.absorb_replace_prob_ondamage) >= 200f;
			}
			set
			{
				if (value)
				{
					this.absorb_replace_prob_both = 200f;
					this.huttobi_ratio = -1000f;
				}
			}
		}

		public NelAttackInfo()
		{
		}

		public NelAttackInfo(AttackInfo Src)
		{
			this.CopyFrom(Src);
		}

		public override AttackInfo CopyFrom(AttackInfo Src)
		{
			base.CopyFrom(Src);
			if (Src is NelAttackInfo)
			{
				NelAttackInfo nelAttackInfo = Src as NelAttackInfo;
				this.Caster = nelAttackInfo.Caster;
				this.PublishMagic = nelAttackInfo.PublishMagic;
				this.absorb_replace_prob = nelAttackInfo.absorb_replace_prob;
				this.absorb_replace_prob_ondamage = nelAttackInfo.absorb_replace_prob_ondamage;
				this.ignore_nodamage_time = nelAttackInfo.ignore_nodamage_time;
				this.pr_myself_fire = nelAttackInfo.pr_myself_fire;
				this.huttobi_ratio = nelAttackInfo.huttobi_ratio;
				this.shield_break_ratio = nelAttackInfo.shield_break_ratio;
				this.torn_apply_min = nelAttackInfo.torn_apply_min;
				this.torn_apply_max = nelAttackInfo.torn_apply_max;
				this.pee_apply100 = nelAttackInfo.pee_apply100;
				this.hit_r = nelAttackInfo.hit_r;
				this.setable_UP = nelAttackInfo.setable_UP;
				this.press_state_replace = nelAttackInfo.press_state_replace;
				this.parryable = nelAttackInfo.parryable;
				this.shield_success_nodamage = nelAttackInfo.shield_success_nodamage;
				this.Beto = nelAttackInfo.Beto;
			}
			return this;
		}

		public NelAttackInfo Burst(float x, float y)
		{
			this.burst_vx = x;
			this.burst_vy = y;
			return this;
		}

		public override bool isPenetrateAbsorb()
		{
			return base.isPenetrateAbsorb() || this.huttobi_ratio >= 100f || this.absorb_replace_prob >= 200f;
		}

		public NelAttackInfo Parry(bool f)
		{
			this.parryable = f;
			return this;
		}

		public NelAttackInfo Torn(float _min01, float _max = -1000f)
		{
			this.torn_apply_min = _min01;
			this.torn_apply_max = ((_max <= -1000f) ? this.torn_apply_min : _max);
			return this;
		}

		public NelAttackInfo PeeApply(float level100)
		{
			this.pee_apply100 = level100;
			return this;
		}

		public NelAttackInfo BurstDir(float mpf)
		{
			this.burst_vx = X.Abs(this.burst_vx) * mpf;
			return this;
		}

		public NelAttackInfo BurstDir(bool mpf)
		{
			this.burst_vx = X.Abs(this.burst_vx) * (float)(mpf ? 1 : (-1));
			return this;
		}

		public NelAttackInfo PtcSt(string name)
		{
			this.hit_ptcst_name = name;
			return this;
		}

		public bool isPlayerShotgun()
		{
			return this.PublishMagic != null && this.PublishMagic.isPlayerShotgun();
		}

		public bool isPrBurst()
		{
			return this.PublishMagic != null && this.PublishMagic.kind == MGKIND.PR_BURST;
		}

		public override bool isPhysical()
		{
			return base.isPhysical() || (this.PublishMagic != null && this.PublishMagic.projectile_power < 0);
		}

		public M2MagicCaster Caster;

		public MagicItem PublishMagic;

		public float absorb_replace_prob;

		public float absorb_replace_prob_ondamage;

		public bool ignore_nodamage_time;

		public bool pr_myself_fire;

		public float huttobi_ratio;

		public float shield_break_ratio = 1f;

		public float torn_apply_min;

		public float torn_apply_max;

		public float pee_apply100;

		public float hit_r;

		public bool parryable;

		public bool setable_UP = true;

		public byte press_state_replace = byte.MaxValue;

		public float shield_success_nodamage;

		public BetoInfo Beto;
	}
}
