using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class NelEnemyMagicable : NelEnemy
	{
		protected abstract MGKIND get_magic_kind();

		public override void appear(Map2d _Mp, NOD.BasicData BasicData)
		{
			this.FD_drawEffectMagicElec = new M2DrawBinder.FnEffectBind(this.drawEffectMagicElecInner);
			this.FD_ElecGetPos = new Func<Vector2>(this.getTargetPos);
			base.appear(this.Mp, BasicData);
		}

		protected MagicItem initChant()
		{
			this.killExpectationEffect();
			if (this.CurMg != null)
			{
				if (this.CurMg.is_sleep)
				{
					this.CurMg.kill(-1f);
				}
				this.CurMg = null;
			}
			if (this.CurMg == null)
			{
				MGKIND magic_kind = this.get_magic_kind();
				this.CurMg = base.nM2D.MGC.setMagic(this, magic_kind, MGHIT.AUTO);
				if (this.mp_hold > 0f)
				{
					this.CurMg.t = this.CurMg.casttime * this.mp_hold / this.CurMg.reduce_mp;
				}
			}
			return this.CurMg;
		}

		public bool progressChant()
		{
			if (this.CurMg == null || this.CurMg.casttime <= 0f || this.CurMg.is_sleep)
			{
				return false;
			}
			float num = X.Mn(this.CurMg.casttime, this.CurMg.t + this.TS) * this.CurMg.reduce_mp / this.CurMg.casttime;
			if ((float)this.mp - num <= 0f)
			{
				return false;
			}
			this.mp_hold = num;
			return true;
		}

		protected bool explodeMagic(bool set_expectation = true)
		{
			if (this.CurMg == null)
			{
				return false;
			}
			int num = (int)this.mp_hold;
			MagicItem magicItem = this.CurMg.explode(false);
			if (this.CurMg == null || magicItem == null)
			{
				return false;
			}
			magicItem.reduce_mp = (float)num;
			this.CurMg = null;
			this.mp_hold = 0f;
			this.applyMpDamage(num, true, null);
			this.CurMg = magicItem;
			if (set_expectation)
			{
				this.setExpectationEffect();
			}
			return true;
		}

		protected void sleepMagic()
		{
			if (this.CurMg != null && !this.CurMg.is_sleep)
			{
				if (!this.CurMg.isPreparingCircle)
				{
					this.mp_hold = 0f;
					if (!this.is_alive)
					{
						this.CurMg.kill(-1f);
						this.killExpectationEffect();
					}
					this.CurMg = null;
					return;
				}
				this.fineHoldMagicTime();
				this.Phy.quitSoftFall(0f);
				if (this.CurMg != null)
				{
					this.CurMg.Sleep();
				}
			}
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
			this.sleepMagic();
			base.quitTicket(Tk);
		}

		public override void runPost()
		{
			base.runPost();
			NoelAnimator.prepareHoldElecAndEd(this.Mp, this.CurMg != null && this.CurMg.isPreparingCircle && this.CurMg.chant_finished, ref this.HoldMagicElec, ref this.HoldMagicEd, this.FD_drawEffectMagicElec, false);
		}

		private bool drawEffectMagicElecInner(EffectItem E, M2DrawBinder Ed)
		{
			if (this.HoldMagicElec == null || this.CurMg == null)
			{
				if (Ed == this.HoldMagicEd)
				{
					this.HoldMagicEd = null;
				}
				return false;
			}
			return NoelAnimator.drawEffectMagicElecS(this.Mp, E, Ed, this.HoldMagicElec, this.FD_ElecGetPos, false);
		}

		public void setExpectationEffect()
		{
			if (this.ExpectationEd == null && this.CurMg != null && this.CurMg.Ray != null)
			{
				if (this.RayForDraw == null)
				{
					this.RayForDraw = new M2Ray();
				}
				this.RayForDraw.CopyFrom(this.CurMg.Ray);
				this.RayForDraw.hittype |= HITTYPE.AUTO_TARGET;
				this.ExpcMg = this.CurMg;
				if (this.FD_drawExpectation == null)
				{
					this.FD_drawExpectation = new M2DrawBinder.FnEffectBind(this.drawExpectation);
				}
				this.ExpectationEd = this.Mp.setED("MgExpec", this.FD_drawExpectation, 0f);
				this.expect_collider_t = 0f;
			}
		}

		public void killExpectationEffect()
		{
			if (this.ExpectationEd != null)
			{
				this.ExpectationEd = this.Mp.remED(this.ExpectationEd);
				this.ExpcMg = null;
			}
		}

		private bool drawExpectation(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!this.is_alive || this.ExpcMg == null || this.ExpcMg.Mn == null || this.RayForDraw == null)
			{
				if (Ed == this.ExpectationEd)
				{
					this.ExpectationEd = null;
				}
				return false;
			}
			MagicItem expcMg = this.ExpcMg;
			MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
			float num = (expcMg.isPreparingCircle ? 0f : expcMg.t);
			expcMg.calcAimPos(false);
			float num2 = expcMg.aim_agR;
			expcMg.setRayStartPos(this.RayForDraw);
			this.RayForDraw.AngleR(num2);
			if (expcMg.isPreparingCircle)
			{
				Vector2 aimPos = this.getAimPos(expcMg);
				Vector2 pos = this.RayForDraw.Pos;
				Vector2 aimInitPos = expcMg.getAimInitPos();
				num2 = this.Mp.GAR(aimInitPos.x, aimInitPos.y, aimPos.x, aimPos.y);
				this.RayForDraw.AngleR(num2);
			}
			else
			{
				num2 = expcMg.aim_agR;
			}
			bool flag = this.expect_collider_t <= this.Mp.floort;
			this.RayForDraw.hittype |= HITTYPE.AUTO_TARGET;
			expcMg.Mn.drawTo(mesh, this.Mp, this.RayForDraw.getMapPos(0f), num, num2, !this.RayForDraw.hit_pr, Ed.t, flag ? this.RayForDraw : null, null);
			if (flag)
			{
				this.expect_collider_t = this.Mp.floort + 12f;
			}
			return true;
		}

		public virtual bool has_chantable_mp()
		{
			return MagicSelector.getKindData((this.CurMg != null) ? this.CurMg.kind : this.get_magic_kind()).reduce_mp <= this.mp;
		}

		public bool magic_explodable()
		{
			return this.CurMg != null && this.mp_hold > 0f && this.CurMg.casttime <= this.CurMg.t;
		}

		public override int applyMpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			val = base.applyMpDamage(val, force, Atk);
			if (val > 0)
			{
				this.mp_hold -= (float)val;
				this.fineHoldMagicTime();
			}
			return val;
		}

		private bool fineHoldMagicTime()
		{
			if (this.CurMg == null || !this.CurMg.isPreparingCircle)
			{
				return true;
			}
			if (this.mp_hold <= 0f)
			{
				this.Phy.quitSoftFall(0f);
				this.CurMg.close();
				this.CurMg.kill(-1f);
				this.CurMg = null;
				this.killExpectationEffect();
				if (this.HoldMagicEd != null)
				{
					this.HoldMagicElec.release();
					this.HoldMagicEd = this.Mp.remED(this.HoldMagicEd);
				}
				return true;
			}
			this.CurMg.castedTimeResetTo(this.CurMg.casttime * this.mp_hold / this.CurMg.reduce_mp);
			return false;
		}

		public override int getMpDamageValue(NelAttackInfo Atk, int val)
		{
			return X.IntR((float)base.getMpDamageValue(Atk, val) * ((this.CurMg != null && this.CurMg.isPreparingCircle) ? (this.CurMg.is_sleep ? 1.5f : 2f) : 1f));
		}

		protected override bool initDeathEffect()
		{
			if (this.HoldMagicEd != null)
			{
				this.HoldMagicElec.release();
				this.HoldMagicEd = this.Mp.remED(this.HoldMagicEd);
			}
			this.killExpectationEffect();
			if (this.CurMg != null)
			{
				if (this.mp_hold > 0f)
				{
					this.mp_hold = 0f;
					this.fineHoldMagicTime();
				}
				else
				{
					this.Phy.quitSoftFall(0f);
					this.CurMg.close();
					this.CurMg.kill(-1f);
					this.CurMg = null;
				}
			}
			return base.initDeathEffect();
		}

		protected MagicItem CurMg;

		protected float mp_hold;

		private ElecTraceDrawer HoldMagicElec;

		private M2DrawBinder HoldMagicEd;

		private M2DrawBinder ExpectationEd;

		private M2Ray RayForDraw;

		private MagicItem ExpcMg;

		private float expect_collider_t;

		private M2DrawBinder.FnEffectBind FD_drawEffectMagicElec;

		private Func<Vector2> FD_ElecGetPos;

		private M2DrawBinder.FnEffectBind FD_drawExpectation;
	}
}
