using System;
using m2d;
using XX;

namespace nel
{
	public class M2SlimeDecoy : M2Attackable
	{
		public void appear(NelChipSlimeDecoy _Dr, Map2d _Mp)
		{
			base.gameObject.isStatic = !_Dr.arrangeable;
			this.Dr = _Dr;
			base.Size(this.Dr.cld_mapw_scale * (float)this.Dr.iwidth * 0.5f, this.Dr.cld_maph * _Mp.CLEN, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			this.maxhp = 2000;
			this.maxmp = 2000;
			_Mp.assignMover(this, false);
			if (this.Lig == null)
			{
				this.Lig = new M2Light(_Mp, null);
				this.Lig.Pos(_Dr.mapcx, _Dr.mapcy);
				this.Lig.follow_speed = 1f;
				this.Lig.Col.Set(2859760228U);
				this.Lig.radius = 45f;
				this.Mp.addLight(this.Lig);
			}
			this.TeCon.RegisterCol(this.Dr, false);
		}

		public void finePos(float trans_ux = 0f, float trans_uy = 0f)
		{
			this.moveBy(trans_ux * 0.015625f * this.Mp.rCLEN, -trans_uy * 0.015625f * this.Mp.rCLEN, true);
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.Lig != null)
			{
				this.Mp.remLight(this.Lig);
			}
			this.Lig = null;
			base.destruct();
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if (this.isActive())
			{
				return (RAYHIT)3;
			}
			return RAYHIT.NONE;
		}

		public override int applyHpDamage(int val, bool force = false, AttackInfo _Atk = null)
		{
			NelAttackInfo nelAttackInfo = _Atk as NelAttackInfo;
			if (this.NoDamage.isActive(nelAttackInfo.ndmg))
			{
				return 0;
			}
			bool flag = nelAttackInfo.ndmg != NDMG.DEFAULT && nelAttackInfo.ndmg > NDMG.NORMAL;
			float damage_ratio = this.Dr.damage_ratio;
			this.Mp.DmgCntCon.Make(this, -(int)((float)val * damage_ratio), 0, M2DmgCounterItem.DC.NORMAL, false);
			this.Dr.hit_t = this.Mp.floort * (float)X.MPF(nelAttackInfo.burst_vx > 0f);
			if (flag)
			{
				this.NoDamage.Add(nelAttackInfo.ndmg, (float)nelAttackInfo.nodamage_time);
			}
			if (TX.valid(this.Dr.hit_ptc))
			{
				base.PtcVar("hit_x", (double)nelAttackInfo.hit_x).PtcVar("hit_y", (double)nelAttackInfo.hit_y).PtcST(this.Dr.hit_ptc, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (this.TeCon != null)
			{
				this.TeCon.setDmgBlinkEnemy(nelAttackInfo.attr, 15f, 0.8f, 0.9f, 0);
			}
			return 1;
		}

		public override void fineHittingLayer()
		{
			base.gameObject.layer = 2;
		}

		public override bool isDamagingOrKo()
		{
			return false;
		}

		public override HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.EN;
		}

		public bool isActive()
		{
			return this.Dr.isActive();
		}

		public M2BlockColliderContainer.BCCLine GetBcc()
		{
			return this.Dr.GetBcc();
		}

		private float time;

		private const float charge_time_min = 2400f;

		private const float charge_time_max = 3000f;

		private const float charge_time_when_safe = 6000f;

		private NelChipSlimeDecoy Dr;

		private M2Light Lig;
	}
}
