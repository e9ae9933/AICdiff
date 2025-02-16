using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2BreakableWallMover : ChipMover, IM2RayHitAble, IBCCFootListener
	{
		public void initLp(M2LpBreakable _Lp, int _maxhp)
		{
			this.Lp = _Lp;
			this.hp = _maxhp;
			this.maxhp = _maxhp;
			this.FallInit = null;
			if (_Lp.type == M2LpBreakable.BREAKT.FIRE)
			{
				this.fire_af = 99f;
			}
			if (this.Lp.do_not_bind_BCC)
			{
				base.do_not_bind_BCC = true;
				if (this.Lp.fallbreak)
				{
					this.Lp.Mp.addBCCFootListener(this);
				}
			}
			else
			{
				base.do_not_bind_BCC = false;
			}
			this.Lp.Mp.assignMover(this, false);
			this.disappearing_collider_trigger = this.Lp.auto_revertable;
			base.initTransEffecter();
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
		}

		public override void destruct()
		{
			base.destruct();
			if (this.Lp != null)
			{
				this.Lp.destructedMover(this);
			}
			this.Lp = null;
		}

		public override void runPre()
		{
			if (this.damage_applyable)
			{
				if (this.fire_af >= 0f)
				{
					if (this.fire_af < 100f)
					{
						this.fire_af -= this.TS;
						if (this.fire_af <= 0f)
						{
							this.fire_af = 30f;
							if (this.checkFireNeighbor())
							{
								this.fire_af = 100f;
								this.Lp.breakPrepareEffect();
							}
						}
					}
					else
					{
						if (this.fire_af >= 125f && !this.TeCon.existSpecific(TEKIND.QUAKE_VIB))
						{
							this.TeCon.setQuake(2f, 50, 2f, 0);
						}
						this.fire_af += this.TS;
						if (this.fire_af >= 190f)
						{
							if (this.checkFireNeighbor())
							{
								this.FallInit = new AttackInfo().CenterXy(base.x, base.y, 0f);
								this.FallInit.hpdmg0 = 9999;
								this.applyHpDamage(this.maxhp, true, this.FallInit);
							}
							this.fire_af = 99f;
						}
					}
				}
				else if (this.FallInit != null)
				{
					if (this.hp > 1)
					{
						bool flag = (float)this.hp >= (float)this.maxhp * 0.5f;
						this.hp--;
						if (this.maxhp >= 20 && flag && (float)this.hp < (float)this.maxhp * 0.5f)
						{
							this.TeCon.setQuake(2f, this.hp, 2f, 0);
						}
					}
					else
					{
						this.applyHpDamage(this.maxhp, true, this.FallInit);
						this.hp = 0;
					}
				}
			}
			if (!this.is_alive)
			{
				if (this.Lp.revertable && this.Lp.revert_time > 0)
				{
					if (this.UnderRevertCheckMv == null || !this.Lp.under_revert || this.UnderRevertCheckMv.mtop > base.mbottom + 0.04f)
					{
						this.no_damage_time -= this.TS;
					}
					if (this.no_damage_time < (float)(-(float)this.Lp.revert_time))
					{
						this.revertPuzzRevertHp((float)this.get_maxhp(), true);
					}
				}
				return;
			}
			if (this.no_damage_time > 0f)
			{
				this.no_damage_time = X.Mx(this.no_damage_time - this.TS, 0f);
			}
			base.runPre();
		}

		public int get_maxhp()
		{
			return this.maxhp;
		}

		public int get_hp()
		{
			return this.hp;
		}

		public bool is_alive
		{
			get
			{
				return this.hp > 0;
			}
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			return (HITTYPE)6;
		}

		public RAYHIT can_hit(M2Ray Ray)
		{
			if (!this.is_alive)
			{
				return RAYHIT.NONE;
			}
			if ((Ray.hittype & HITTYPE.GUARD_IGNORE) != HITTYPE.NONE && this.Lp.fallbreak)
			{
				return RAYHIT.NONE;
			}
			if (this.no_damage_time != 0f)
			{
				return RAYHIT.NONE;
			}
			return (RAYHIT)99;
		}

		public void revertPuzzRevertHp(float dephp = 0f, bool set_effect = false)
		{
			if (dephp > 0f)
			{
				if (this.is_alive)
				{
					this.hp = (int)dephp;
					return;
				}
				this.no_damage_time = 0f;
				this.FallInit = null;
				this.GobEnemyStaying = null;
				this.UnderRevertCheckMv = null;
				this.hp = (int)dephp;
				base.disappearing = false;
				if (set_effect)
				{
					this.Mp.PtcSTsetVar("x", (double)base.x).PtcSTsetVar("y", (double)base.y).PtcSTsetVar("w", (double)((float)this.Lp.mapw * this.Mp.CLENB * 0.5f / 0.33f))
						.PtcSTsetVar("h", (double)((float)this.Lp.maph * this.Mp.CLENB * 0.5f / 0.33f))
						.PtcST("revert_breakable_wall", null, PTCThread.StFollow.NO_FOLLOW);
					this.TeCon.setColorBlinkAdd(1f, 30f, 0.4f, 16777215, 0);
				}
				M2BlockColliderContainer bcccon = base.getBCCCon();
				if (this.Lp.do_not_bind_BCC && this.Lp.fallbreak)
				{
					this.Lp.Mp.addBCCFootListener(this);
				}
				if (bcccon != null)
				{
					this.Mp.assignCarryableBCC(bcccon);
					bcccon.active = true;
					return;
				}
			}
			else
			{
				if (!this.is_alive)
				{
					return;
				}
				this.hp = 0;
				this.initDeath();
				if (set_effect)
				{
					this.Lp.breakEffect(base.x, base.y, 0f, 0.03f, 0);
				}
			}
		}

		public bool initDeath()
		{
			this.FallInit = null;
			this.no_damage_time = 0f;
			this.Lp.initBreak(false);
			this.TeCon.clear();
			base.disappearing = true;
			M2BlockColliderContainer bcccon = base.getBCCCon();
			if (this.Lp.do_not_bind_BCC)
			{
				this.Lp.Mp.remBCCFootListener(this);
			}
			if (bcccon != null)
			{
				this.Mp.removeCarryableBCC(bcccon);
				bcccon.active = false;
			}
			return true;
		}

		public int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			if (!this.damage_applyable)
			{
				return 0;
			}
			if (!force)
			{
				if (this.no_damage_time != 0f)
				{
					return 0;
				}
				if (this.Lp.check_damage(Atk as NelAttackInfo) == 0)
				{
					this.no_damage_time = 3f;
					return 0;
				}
			}
			float num = Atk.burst_vx;
			float num2 = Atk.burst_vy;
			float num3 = this.Mp.GAR(0f, Atk.center_y, Atk.hit_x - Atk.center_x * 3f, Atk.hit_y);
			if (num == 0f)
			{
				num = X.Cos(num3) * 0.15f;
				num2 += X.Sin(num3) * 0.15f;
			}
			int num4 = 1;
			M2MoverPr m2MoverPr = null;
			if (Atk is NelAttackInfo)
			{
				NelAttackInfo nelAttackInfo = Atk as NelAttackInfo;
				MagicItem publishMagic = nelAttackInfo.PublishMagic;
				if (publishMagic != null && !publishMagic.is_normal_attack)
				{
					num4 = this.maxhp;
				}
				if (nelAttackInfo.Caster is M2MoverPr)
				{
					m2MoverPr = nelAttackInfo.Caster as M2MoverPr;
				}
			}
			this.hp -= (force ? val : num4);
			this.no_damage_time = 12f;
			string text;
			if (this.hp <= 0)
			{
				text = this.Lp.breakEffect(Atk.hit_x, Atk.hit_y, num, num2, this.Lp.revertable ? 9 : (-1));
				this.initDeath();
				if (m2MoverPr != null)
				{
					m2MoverPr.PadVib("lp_breakable_death", 1f);
				}
			}
			else
			{
				this.TeCon.setColorBlinkAdd(1f, 6f, 0.6f, 16777215, 0);
				text = "hit_breakable_wall";
				if (m2MoverPr != null)
				{
					m2MoverPr.PadVib("lp_breakable_hit", 1f);
				}
			}
			if (text != "")
			{
				this.Mp.PtcSTsetVar("x", (double)Atk.hit_x).PtcSTsetVar("y", (double)Atk.hit_y).PtcSTsetVar("agR", (double)(-(double)num3))
					.PtcST(text, null, PTCThread.StFollow.NO_FOLLOW);
			}
			if (Atk != null)
			{
				Atk.playEffect(this);
			}
			if (this.Lp.type == M2LpBreakable.BREAKT.FALLPUNCH && this.FallInit != Atk)
			{
				this.FallInit = Atk;
				if (this.hp > 1)
				{
					this.hp = 1;
				}
			}
			return 1;
		}

		public uint getFootableAimBits()
		{
			return 8U;
		}

		public DRect getMapBounds(DRect BufRc)
		{
			if (this.Lp == null)
			{
				return null;
			}
			return BufRc.Set((float)this.Lp.mapx, (float)this.Lp.mapy, (float)this.Lp.mapw, (float)this.Lp.maph);
		}

		public bool footedInit(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Lsn)
		{
			if (base.destructed || this.FallInit != null || !this.is_alive || !(Lsn is M2FootManager))
			{
				return false;
			}
			M2FootManager m2FootManager = Lsn as M2FootManager;
			M2BlockColliderContainer bcccon = base.getBCCCon();
			if (bcccon == null)
			{
				return false;
			}
			M2BlockColliderContainer.BCCLine bccline;
			bcccon.isFallable(m2FootManager.Mv.x, m2FootManager.Mv.mbottom, 0.05f, 0.15f, out bccline, true, true, -1f);
			if (bccline != null)
			{
				this.initRideBy(m2FootManager.Mv);
			}
			return true;
		}

		public bool footedQuit(IMapDamageListener Fd, bool from_jump_init = false)
		{
			return true;
		}

		public override IFootable initCarry(ICarryable FootD)
		{
			IFootable footable = base.initCarry(FootD);
			if (footable != null && this.Lp.fallbreak && FootD is M2FootManager)
			{
				this.initRideBy((FootD as M2FootManager).Mv);
			}
			return footable;
		}

		private void initRideBy(M2Mover Mv)
		{
			if (Mv == null || !this.Lp.fallbreak)
			{
				return;
			}
			this.UnderRevertCheckMv = Mv;
			if (Mv == null)
			{
				this.FallInit = new AttackInfo().CenterXy(base.x, base.y, 0f);
			}
			else
			{
				this.FallInit = new AttackInfo().CenterXy(Mv.x, Mv.mbottom, 0f);
			}
			this.FallInit.hpdmg0 = 9999;
		}

		private void OnTriggerStay2D(Collider2D other)
		{
			if (base.disappearing && this.no_damage_time < 0f && this.Lp.revert_time > 0)
			{
				this.no_damage_time = X.MMX((float)X.Mn(-this.Lp.revert_time + 40, 0), this.no_damage_time, 0f);
			}
		}

		protected override void OnCollisionStay2D(Collision2D col)
		{
			base.OnCollisionStay2D(col);
			if (this.Mp.getTag(col.gameObject) == "MoverEn" && this.Lp.fallbreak)
			{
				this.initRideBy(col.gameObject.GetComponent<M2Mover>());
			}
		}

		private bool checkFireNeighbor()
		{
			int num = this.Lp.mapx + this.Lp.mapw + 1;
			int num2 = this.Lp.mapy + this.Lp.maph + 1;
			for (int i = this.Lp.mapx - 1; i < num; i++)
			{
				for (int j = this.Lp.mapy - 1; j < num2; j++)
				{
					if (CCON.isWater(this.Mp.getConfig(i, j)) && (base.M2D as NelM2DBase).MIST.isFire(i, j))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool damage_applyable
		{
			get
			{
				return this.is_alive && (this.Lp.Managed == null || PUZ.IT.barrier_active);
			}
		}

		public bool breakableByBomb(MagicItem Mg)
		{
			return this.Lp.check_damage(Mg.Atk0) > 0;
		}

		private M2LpBreakable Lp;

		private AttackInfo FallInit;

		private float fire_af = -1f;

		private M2Mover UnderRevertCheckMv;

		private GameObject GobEnemyStaying;

		private float no_damage_time;

		private int maxhp;

		private int hp;

		private const float MAXT_REVERT_ANIM = 20f;
	}
}
