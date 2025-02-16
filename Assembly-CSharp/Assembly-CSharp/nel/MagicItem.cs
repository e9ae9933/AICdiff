using System;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MagicItem : IRunAndDestroy, IEfPInteractale
	{
		public MagicItem(MGContainer _MGC)
		{
			this.MGC = _MGC;
			this.PtcHld = new PtcHolder(this, 4, 4);
			this.FD_DrawDelegate = new M2DrawBinder.FnEffectBind(this.edDrawInner);
			if (MagicItem.Halo == null)
			{
				MagicItem.Halo = new HaloDrawer(-1f, -1f, -1f, -1f, -1f, -1f);
			}
		}

		public MagicItem init(int _id, M2MagicCaster _Caster, MGKIND _kind, MGHIT _hittype)
		{
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			if (this.Dro != null)
			{
				this.Dro.destruct(true);
				this.Dro = null;
			}
			this.id = _id;
			this.PtcHld.clear();
			this.flags = 0;
			this.Caster = _Caster;
			this.kind = _kind;
			this.hittype = _hittype & (MGHIT)(-385);
			this.fnRunMain = null;
			this.changeRay(null);
			this.reduce_mp = 0f;
			this.mp_crystalize = 0.5f;
			this.projectile_power_ = -1;
			this.crystalize_neutral_ratio = 0.35f;
			this.TS = 1f;
			this.type = 0;
			this.wind_velocity = Vector3.zero;
			this.wind_apply_s_level = 0f;
			this.sz = (this.dz = (this.sa = (this.da = 0f)));
			this.dx = (this.dy = 0f);
			this.phase = 0;
			Vector2 center = this.Caster.getCenter();
			this.sx = center.x;
			this.sy = center.y;
			this.reflect_lock_floort = 0f;
			this.Mn = null;
			if (this.Other != null && this.Other is IDisposable)
			{
				(this.Other as IDisposable).Dispose();
			}
			this.Other = null;
			bool flag;
			MDAT.initMagicItem(this, out flag);
			if (this.fnRunMain == null)
			{
				this.type = 1;
				this.initFunc(MagicItem.FD_runMagicCircle, (MagicItem Mg, float fcnt) => MagicItem.drawMagicCircle(Mg, fcnt));
			}
			if (!this.isPreparingCircle)
			{
				this.hittype |= MGHIT.IMMEDIATE;
				if (this.Mp != null)
				{
					this.calcAimPos(false);
				}
				this.calced_aim_pos = false;
				this.chant_finished = true;
			}
			this.t = 0f;
			this.Awake();
			return this;
		}

		public M2Ray Ray
		{
			get
			{
				return this.Ray_;
			}
		}

		public void changeRay(M2Ray Target)
		{
			if (this.Ray_ == Target)
			{
				return;
			}
			if (this.Ray_ != null)
			{
				this.MGC.destructRay(this.Ray_);
			}
			this.Ray_ = Target;
		}

		public MagicItem initFuncNoDraw(MagicItem.FnMagicRun fnRun)
		{
			return this.initFunc(fnRun, (MagicItem Mg, float fcnt) => MagicItem.drawNone(Mg, fcnt));
		}

		public MagicItem initFunc(MagicItem.FnMagicRun fnRun, MagicItem.FnMagicRun fnDraw = null)
		{
			this.fnRunMain = fnRun;
			if (fnDraw != null)
			{
				this.fnDrawMain = fnDraw;
				if (fnDraw != null && this.Ed == null)
				{
					this.Ed = this.Mp.setED("magic_draw", this.FD_DrawDelegate, 0f);
				}
			}
			return this;
		}

		public MagicItem MpConsume(NOD.MpConsume Mcs, float ratio = 1f)
		{
			if (Mcs == null)
			{
				return this;
			}
			this.reduce_mp = (float)Mcs.consume;
			this.mp_crystalize = ((this.reduce_mp == 0f) ? 0f : ((float)Mcs.release / this.reduce_mp));
			this.crystalize_neutral_ratio = Mcs.neutral_ratio;
			this.reduce_mp = (float)X.IntR(this.reduce_mp * ratio);
			return this;
		}

		public MagicItem close()
		{
			if (!this.killed && !this.closed)
			{
				if (this.casttime == 0f)
				{
					this.kill(-1f);
					return this;
				}
				if (!this.exploded && this.isPreparingCircle)
				{
					if (this.Mp != null)
					{
						this.PtcST("magic_abort", PTCThread.StFollow.NO_FOLLOW, false);
					}
					this.kill(-1f);
					return this;
				}
				this.killEffect();
				this.hittype = MGHIT.CLOSED | (this.hittype & MGHIT.BERSERK);
				this.fnRunMain(this, 1f);
			}
			return this;
		}

		public bool kill(float killeffect_speed = -1f)
		{
			if (killeffect_speed >= 0f)
			{
				this.calcAimPos(false);
			}
			if ((this.hittype & MGHIT.IMMEDIATE) != (MGHIT)0 && !this.exploded)
			{
				this.explode(false);
			}
			if (!this.MGC.no_set_kill_effect && killeffect_speed >= 0f && !this.killed)
			{
				this.PtcVar("speed", (double)killeffect_speed).PtcVar("agR", (double)this.aim_agR);
				this.PtcST("magic_killed", PTCThread.StFollow.NO_FOLLOW, false);
			}
			if (this.Dro != null)
			{
				this.Dro.destruct(true);
				this.Dro = null;
			}
			this.hittype = MGHIT.KILLED;
			this.casttime = 0f;
			this.wind_apply_s_level = 0f;
			this.t = -1f;
			if (!this.no_kill_effect_when_close && !this.MGC.no_set_kill_effect)
			{
				this.killEffect();
			}
			this.releasePooledObject(true, false);
			this.Mn = null;
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			if (this.input_null_to_other_when_quit)
			{
				if (this.Other is IDisposable)
				{
					(this.Other as IDisposable).Dispose();
				}
				this.Other = null;
			}
			return false;
		}

		public void releasePooledObject(bool destroy_ray_atk = true, bool destroy_other = false)
		{
			if (destroy_ray_atk)
			{
				this.changeRay(null);
				this.Atk0 = this.MGC.destructAtk(this.Atk0);
				this.Atk1 = this.MGC.destructAtk(this.Atk1);
				this.Atk2 = this.MGC.destructAtk(this.Atk2);
			}
			if (destroy_other)
			{
				if (this.Other is IDisposable)
				{
					(this.Other as IDisposable).Dispose();
				}
				this.Other = null;
			}
		}

		public void killEffect()
		{
			this.PtcHld.killPtc(false);
		}

		public void killPtc(string s)
		{
			this.PtcHld.killPtc(s, false);
		}

		public Vector2 calcTargetPos()
		{
			if (!this.calced_target_pos)
			{
				if (this.target_s)
				{
					this.PosT.Set(this.sx, this.sy);
				}
				else if (this.target_d)
				{
					this.PosT.Set(this.dx, this.dy);
				}
				else
				{
					this.PosT = this.Caster.getTargetPos();
					this.calced_target_pos = true;
				}
			}
			return this.PosT;
		}

		public Vector2 getAimInitPos()
		{
			if (this.aimagr_calc_s)
			{
				return new Vector2(this.sx, this.sy);
			}
			if (this.aimagr_calc_d)
			{
				return new Vector2(this.dx, this.dy);
			}
			float num = 0f;
			float num2 = 0f;
			if (this.Mn != null)
			{
				num += this.Mn._0.x;
				num2 += this.Mn._0.y;
			}
			return new Vector2(this.Cen.x + num, this.Cen.y + num2);
		}

		public Vector2 getAimInitRotateCenterPos()
		{
			float num = 0f;
			float num2 = 0f;
			if (this.Mn != null)
			{
				num += this.Mn._0.x;
				num2 += this.Mn._0.y;
			}
			return new Vector2(this.Cen.x + num, this.Cen.y + num2);
		}

		public Vector2 calcAimPos(bool input_dpos)
		{
			if (!this.calced_aim_pos)
			{
				this.calced_aim_pos = true;
				Vector2 aimInitPos = this.getAimInitPos();
				if (this.aimagr_calc_vector_d)
				{
					if (this.Dro != null)
					{
						this.aim_agR = this.Mp.GAR(0f, 0f, this.Dro.vx, this.Dro.vy);
					}
					else
					{
						this.aim_agR = this.Mp.GAR(0f, 0f, this.dx, this.dy);
					}
					input_dpos = false;
					this.PosA.Set(aimInitPos.x + this.dx * 16f, aimInitPos.y + this.dy * 16f);
				}
				else
				{
					int aimDirection = this.Caster.getAimDirection();
					if (aimDirection >= 0)
					{
						this.PosA.Set(aimInitPos.x + (float)(CAim._XD(aimDirection, 1) * 4), aimInitPos.y + (float)(CAim._YD(aimDirection, 1) * 4));
						this.aim_agR = CAim.get_agR((AIM)aimDirection, 0f);
					}
					else
					{
						this.PosA = this.Caster.getAimPos(this);
						this.aim_agR = this.Mp.GAR(aimInitPos.x, aimInitPos.y, this.PosA.x, this.PosA.y);
					}
				}
			}
			if (input_dpos)
			{
				this.dx = this.PosA.x;
				this.dy = this.PosA.y;
			}
			return this.PosA;
		}

		public EffectItem setE(string ef_name, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			return this.MGC.M2D.curMap.setE(ef_name, this.Cen.x + mapx, this.Cen.y + mapy, _z, _time, _saf);
		}

		public EffectItem setET(string ef_name, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			return this.MGC.M2D.curMap.setET(ef_name, this.Cen.x + mapx, this.Cen.y + mapy, _z, _time, _saf);
		}

		public EffectItem PtcNCen(string ptc_name, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			return this.MGC.M2D.curMap.PtcN(ptc_name, this.Cen.x + mapx, this.Cen.y + mapy, _z, _time, _saf);
		}

		public EffectItem PtcNCen(EfParticle Ptc, float mapx, float mapy, float _z, int _time, int _saf = 0)
		{
			return this.MGC.M2D.curMap.PtcN(Ptc, this.Cen.x + mapx, this.Cen.y + mapy, _z, _time, _saf);
		}

		public void defineParticlePreVariable()
		{
			this.PtcHld.Var("cx", (double)this.Cen.x);
			this.PtcHld.Var("cy", (double)this.Cen.y);
			this.PtcHld.Var("_pr", ((this.hittype & MGHIT.PR) != (MGHIT)0) ? "_pr" : "_en");
			this.PtcHld.first_ver = true;
		}

		public string getSoundKey()
		{
			return "MGC";
		}

		public MagicItem PtcVar(string key, double v)
		{
			if (this.Mp == null)
			{
				return null;
			}
			this.Mp.getEffect();
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			this.PtcHld.Var(key, v);
			return this;
		}

		public MagicItem PtcVarS(string key, string v)
		{
			if (this.Mp == null)
			{
				return null;
			}
			this.Mp.getEffect();
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			this.PtcHld.Var(key, v);
			return this;
		}

		public MagicItem PtcVarS(string key, MGATTR attr)
		{
			if (this.Mp == null)
			{
				return null;
			}
			this.Mp.getEffect();
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			this.PtcHld.Var(key, FEnum<MGATTR>.ToStr(attr));
			return this;
		}

		public PTCThread PtcST(string ptc_key, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW, bool do_not_kill_on_release = false)
		{
			if (this.Mp == null)
			{
				return null;
			}
			this.Mp.getEffect();
			if (!this.PtcHld.first_ver)
			{
				this.defineParticlePreVariable();
			}
			return this.PtcHld.PtcST(ptc_key, PtcHolder.PTC_HOLD.NORMAL | (do_not_kill_on_release ? PtcHolder.PTC_HOLD._NO_KILL : PtcHolder.PTC_HOLD.NO_HOLD), _follow);
		}

		public bool isHoldingPtcST(string st_key)
		{
			return this.PtcHld.indexOfPtcST(st_key) >= 0;
		}

		public Vector2 getRayStartPos()
		{
			if (this.isPreparingCircle)
			{
				return this.getExplodePos(false);
			}
			if (this.raypos_s)
			{
				return new Vector2(this.sx, this.sy);
			}
			if (this.raypos_d)
			{
				return new Vector2(this.dx, this.dy);
			}
			if (this.raypos_c)
			{
				return this.Cen;
			}
			if (this.Ray == null)
			{
				return this.Cen;
			}
			return this.Ray.getMapPos(0f);
		}

		public M2Ray setRayStartPos(M2Ray Ray)
		{
			return Ray.PosMap(this.getRayStartPos());
		}

		public M2Ray MnSetRay(M2Ray Ray, int id, float agR, float t)
		{
			this.setRayStartPos(Ray);
			if (this.Mn != null)
			{
				this.Mn.SetRay(Ray, id, agR, t, 0f);
			}
			else
			{
				Ray.Dir = new Vector2(X.Cos(agR), X.Sin(agR));
			}
			return Ray;
		}

		public bool runPre(float fcnt)
		{
			this.calced_target_pos = (this.calced_aim_pos = false);
			if (this.killed)
			{
				return false;
			}
			if (this.closed && this.t >= this.casttime)
			{
				this.kill(-1f);
				return false;
			}
			this.Cen = this.Caster.getCenter();
			if (!this.is_sleep)
			{
				if (this.Ray != null)
				{
					this.setRayStartPos(this.Ray);
				}
				if (this.Dro != null)
				{
					if (this.efpos_s)
					{
						this.sx = this.Dro.x;
						this.sy = this.Dro.y;
					}
					else if (this.efpos_d)
					{
						this.dx = this.Dro.x;
						this.dy = this.Dro.y;
					}
				}
				if (this.wind_apply_s_level > 0f && this.wind_velocity.z > 0f)
				{
					if (this.Dro != null)
					{
						float num = this.wind_velocity.y;
						if (this.Dro.af_ground >= 0f)
						{
							num *= 0f;
						}
						else if (num < 0f)
						{
							float num2 = num;
							num *= X.NIL(1f, 0f, -this.Dro.af_ground - 200f, 200f);
							num = X.Mn(num, X.Mx(num2, -M2DropObject.getGravityVelocity(this.Mp, 0.5f * this.Dro.gravity_scale)));
						}
						this.Dro.addFoc(this.wind_velocity.x, num, this.wind_velocity.z);
						this.wind_velocity.z = 0f;
					}
					else
					{
						this.sx += this.wind_velocity.x * fcnt;
						this.sy += this.wind_velocity.y * fcnt;
						this.wind_velocity.z = X.VALWALK(this.wind_velocity.z, 0f, 1f * fcnt);
					}
				}
			}
			if (fcnt == 0f)
			{
				if (this.Atk0 != null)
				{
					this.Atk0.resetAttackCount();
				}
				if (this.Atk1 != null)
				{
					this.Atk1.resetAttackCount();
				}
				if (this.Atk2 != null)
				{
					this.Atk2.resetAttackCount();
				}
			}
			return true;
		}

		public bool run(float fcnt)
		{
			Bench.P("MAGIC RUN");
			if (!this.runPre(fcnt))
			{
				Bench.Pend("MAGIC RUN");
				return false;
			}
			if (!this.is_sleep)
			{
				if (!this.fnRunMain(this, fcnt))
				{
					if (!this.killed)
					{
						this.kill(-1f);
					}
					Bench.Pend("MAGIC RUN");
					return false;
				}
				this.t += fcnt * this.TS;
			}
			if (X.D)
			{
				this.PtcHld.checkUpdate(X.AF);
			}
			Bench.Pend("MAGIC RUN");
			return true;
		}

		public bool edDrawInner(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.killed)
			{
				return false;
			}
			this.Ef = Ef;
			if (this.efpos_s)
			{
				Ef.x = this.sx;
				Ef.y = this.sy;
			}
			else if (this.efpos_d)
			{
				Ef.x = this.dx;
				Ef.y = this.dy;
			}
			else
			{
				Ef.x = this.Cen.x;
				Ef.y = this.Cen.y;
			}
			Bench.P("MAGIC DRAW");
			bool flag = false;
			try
			{
				flag = this.fnDrawMain == null || this.fnDrawMain(this, (float)X.AF);
			}
			catch
			{
				flag = true;
			}
			Bench.Pend("MAGIC DRAW");
			return flag;
		}

		public Vector2 getExplodePos(bool shifted_first_manipulator = false)
		{
			if (this.explode_pos_c)
			{
				if (!shifted_first_manipulator)
				{
					return this.Cen;
				}
				return this.getMnTranslatedCenterPos();
			}
			else
			{
				if ((this.hittype & MGHIT.IMMEDIATE) != (MGHIT)0)
				{
					return this.calcTargetPos();
				}
				Vector2 vector = default(Vector2);
				if (this.type == 1)
				{
					this.calcAimPos(false);
					Vector2 vector2 = (shifted_first_manipulator ? this.getMnTranslatedCenterPos() : this.Cen);
					vector.x = vector2.x + 1.8f * X.Cos(this.aim_agR);
					vector.y = vector2.y - 1.8f * X.Sin(this.aim_agR);
					return vector;
				}
				return this.calcTargetPos();
			}
		}

		public Vector2 getMnTranslatedCenterPos()
		{
			Vector2 cen = this.Cen;
			if (this.Mn != null)
			{
				cen.x += this.Mn._0.x;
				cen.y += this.Mn._0.y;
			}
			return cen;
		}

		public MagicItem explodeS()
		{
			bool raypos_s = this.raypos_s;
			this.raypos_s = true;
			this.explode(false);
			this.raypos_s = raypos_s;
			return this;
		}

		public MagicItem explode(bool do_not_run1 = false)
		{
			if (this.exploded)
			{
				return this;
			}
			if (this.Ray != null && (this.Ray.hittype & HITTYPE.NO_RETURN_MANA) != HITTYPE.NONE)
			{
				this.mp_crystalize = 0f;
			}
			if ((this.hittype & MGHIT.IMMEDIATE) == (MGHIT)0)
			{
				this.exploded = true;
				Vector2 explodePos = this.getExplodePos(false);
				MagicItem magicItem = this.createNewMagic(this.kind, explodePos.x, explodePos.y, false);
				magicItem.reduce_mp = this.reduce_mp;
				magicItem.mp_crystalize = MDAT.crystalizeRatio(this.Caster, magicItem.mp_crystalize);
				if (!do_not_run1)
				{
					magicItem.run(1f);
				}
				this.reduce_mp = 0f;
				this.close();
				return magicItem;
			}
			if (this.reduce_mp > 0f && this.mp_crystalize > 0f)
			{
				float num = this.reduce_mp * this.mp_crystalize;
				if (num > 0f)
				{
					this.explodeMana(this.raypos_s ? this.sx : (this.raypos_d ? this.dx : this.Cen.x), this.raypos_s ? this.sy : (this.raypos_d ? this.dy : this.Cen.y), num);
					this.exploded = true;
				}
			}
			this.exploded = true;
			return null;
		}

		public MagicItem explodeManaToRay(float ratio = 1f)
		{
			if (this.exploded || this.reduce_mp <= 0f)
			{
				return this;
			}
			Vector2 mapPos = this.Ray.getMapPos(0.78f);
			this.explodeMana(mapPos.x, mapPos.y, this.reduce_mp * this.mp_crystalize * ratio);
			if (ratio >= 1f)
			{
				this.exploded = true;
			}
			else
			{
				this.reduce_mp *= 1f - ratio;
			}
			return this;
		}

		public MagicItem explodeMana(float _x, float _y, float ret_mp)
		{
			if (this.exploded || ret_mp <= 0f || (this.Ray.hittype & HITTYPE.NO_RETURN_MANA) != HITTYPE.NONE)
			{
				return this;
			}
			_y -= 0.4f;
			float num;
			if (this.Caster != null)
			{
				this.M2D.Mana.AddMulti(_x, _y, ret_mp * (1f - this.crystalize_neutral_ratio), this.caster_another_manahit | MANA_HIT.FALL, 1f);
				num = this.crystalize_neutral_ratio;
			}
			else
			{
				num = 1f;
			}
			this.M2D.Mana.AddMulti(_x, _y, ret_mp * num, MANA_HIT.PR | MANA_HIT.EN | MANA_HIT.FALL, this.splash_fall_reduce ? 0.25f : 1f);
			return this;
		}

		public void ManaAbsorbReplace(M2MagicCaster Target, MANA_HIT target_type, ref float mpdmg)
		{
			if (this.exploded || mpdmg <= 0f || (this.Ray.hittype & HITTYPE.NO_RETURN_MANA) != HITTYPE.NONE || this.Caster == null || this.mp_crystalize <= 0f)
			{
				return;
			}
			if ((this.caster_another_manahit & MANA_HIT.ALL) == (target_type & MANA_HIT.ALL))
			{
				return;
			}
			float num = this.reduce_mp * this.mp_crystalize;
			float num2 = num * (1f - this.crystalize_neutral_ratio);
			float num3 = num - num2;
			float num4 = X.Mn(num2, mpdmg);
			if (num4 > 0f)
			{
				num2 -= num4;
				mpdmg -= num4;
				num = num3 + num2;
				if (num > 0f)
				{
					this.crystalize_neutral_ratio = num2 / num;
				}
				this.reduce_mp = num / this.mp_crystalize;
			}
		}

		private MANA_HIT caster_another_manahit
		{
			get
			{
				return ((this.Caster is PR) ? MANA_HIT.EN : MANA_HIT.NOUSE) | ((this.Caster is NelEnemy) ? MANA_HIT.PR : MANA_HIT.NOUSE);
			}
		}

		public MagicItem createNewMagic(MGKIND kind, float explode_x, float explode_y, bool auto_run = true)
		{
			MagicItem magicItem = this.MGC.setMagic(this.Caster, kind, this.hittype | MGHIT.IMMEDIATE);
			magicItem.sx = explode_x;
			magicItem.sy = explode_y;
			if (magicItem.Mn != null && this.Mn != null)
			{
				magicItem.Mn.CopyFrom(this.Mn);
			}
			if (auto_run)
			{
				magicItem.run(1f);
			}
			magicItem.calced_aim_pos = false;
			return magicItem;
		}

		public bool reflectAgR(M2Ray Ray, ref float agR, float weaken_neutral = 0.25f)
		{
			if (Ray.ReflectAnotherRay != null)
			{
				Vector2 mapSpeed = Ray.getMapSpeed();
				float x = mapSpeed.x;
				float y = mapSpeed.y;
				if (this.reflectV(Ray, ref x, ref y, 0f, weaken_neutral, true))
				{
					agR = this.Mp.GAR(0f, 0f, x, y);
					return true;
				}
			}
			return false;
		}

		public bool reflectV(M2Ray Ray, M2DropObject Dro, float min_spd = 0f, float weaken_neutral = 0.25f, bool ray_overwrite_dir = true)
		{
			return this.reflectV(Ray, ref Dro.vx, ref Dro.vy, min_spd, weaken_neutral, ray_overwrite_dir);
		}

		public bool reflectV(M2Ray Ray, ref float vx, ref float vy, float min_spd = 0f, float weaken_neutral = 0.25f, bool ray_overwrite_dir = true)
		{
			if (Ray.ReflectAnotherRay == null || (this.reflect_lock_floort > 0f && this.Mp.floort < this.reflect_lock_floort))
			{
				return false;
			}
			M2Ray reflectAnotherRay = Ray.ReflectAnotherRay;
			Ray.ReflectAnotherRay = null;
			float num = -1f;
			Vector2 mapPos = reflectAnotherRay.getMapPos(0f);
			Vector2 vector = Vector2.zero;
			int hittedMax = Ray.getHittedMax();
			int i = 0;
			while (i < hittedMax)
			{
				M2Ray.M2RayHittedItem hitted = Ray.GetHitted(i);
				if ((hitted.type & HITTYPE.REFLECTED) != HITTYPE.NONE && hitted.Hit == reflectAnotherRay)
				{
					num = 0.7f;
					vector.Set(this.Mp.globaluxToMapx(hitted.hit_ux), this.Mp.globaluyToMapy(hitted.hit_uy));
					if (reflectAnotherRay.lenmp > 0f)
					{
						vector.x -= reflectAnotherRay.Dir.x * 1.1f;
						vector.y += reflectAnotherRay.Dir.y * 1.1f;
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			if (num < 0f)
			{
				num = 0.25f;
				if (reflectAnotherRay.projectile_power < 0 && reflectAnotherRay.Caster != null)
				{
					vector.x = reflectAnotherRay.Caster.x;
					vector.y = X.NI(reflectAnotherRay.Caster.y, mapPos.y, 0.5f);
					num = 0.85f;
				}
				else
				{
					vector = mapPos;
				}
				if (reflectAnotherRay.lenmp > 0f)
				{
					vector.x += -reflectAnotherRay.Dir.x * 2.5f;
					vector.y -= -reflectAnotherRay.Dir.y * 2.5f;
				}
			}
			Vector2 mapPos2 = Ray.getMapPos(0f);
			float num2 = this.Mp.GAR(vector.x, 0f, mapPos2.x, (mapPos2.y - vector.y) * X.NIXP(0.8f, 1.4f));
			float num3 = X.Mx(min_spd, X.LENGTHXY(vx, vy, 0f, 0f));
			vx = X.Cos(num2) * num3;
			vy = -X.Sin(num2) * num3;
			if (ray_overwrite_dir)
			{
				Ray.DirXyM(vx, vy);
			}
			if (!this.isHoldingPtcST("reflect_circle"))
			{
				this.PtcVar("cx", (double)X.NI(vector.x, mapPos2.x, num)).PtcVar("cy", (double)X.NI(vector.y, mapPos2.y, num)).PtcVar("agR", (double)(num2 + 1.5707964f));
				this.PtcST("reflect_circle", PTCThread.StFollow.NO_FOLLOW, true);
			}
			this.crystalize_neutral_ratio *= weaken_neutral;
			this.reflect_lock_floort = this.Mp.floort + 15f;
			return true;
		}

		public M2DropObject createDropper(float vx, float vy, float size, float z = -1f, float time = -1f)
		{
			if (this.Dro == null)
			{
				float x;
				float y;
				if (this.efpos_s)
				{
					x = this.sx;
					y = this.sy;
				}
				else if (this.efpos_d)
				{
					x = this.dx;
					y = this.dy;
				}
				else
				{
					x = this.Cen.x;
					y = this.Cen.y;
				}
				this.Dro = this.Mp.DropCon.AddManual(x, y, vx, vy, z, time);
			}
			this.Dro.size = size;
			this.Dro.type &= (DROP_TYPE)(-2);
			this.Dro.TypeAdd(DROP_TYPE.CHECK_OTHER_BCC);
			return this.Dro;
		}

		public bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			switch (follow)
			{
			case PTCThread.StFollow.NO_FOLLOW:
				V = new Vector2(this.Cen.x, this.Cen.y);
				return false;
			case PTCThread.StFollow.FOLLOW_C:
				V = new Vector2(this.Cen.x, this.Cen.y);
				return true;
			case PTCThread.StFollow.FOLLOW_T:
				this.calcTargetPos();
				V = new Vector2(this.PosT.x, this.PosT.y);
				return true;
			case PTCThread.StFollow.FOLLOW_S:
				V = new Vector2(this.sx, this.sy);
				return true;
			case PTCThread.StFollow.FOLLOW_D:
				V = new Vector2(this.dx, this.dy);
				return true;
			case PTCThread.StFollow.FOLLOW_MAGICCIRCLE:
				V = this.getExplodePos(false);
				return true;
			}
			if (this.Caster != null && this.Caster is IEfPInteractale)
			{
				return (this.Caster as IEfPInteractale).getEffectReposition(St, follow, fcnt, out V);
			}
			V = new Vector2(this.Cen.x, this.Cen.y);
			return true;
		}

		public bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			float effectSlowFactor = this.PtcHld.getEffectSlowFactor(Thread, Ef);
			if (effectSlowFactor > 0f)
			{
				PostEffect.IT.addTimeFixedEffect(Ef, effectSlowFactor);
			}
			return true;
		}

		public void setPtcToCircle(PTCThread rER, STB Stb, int count, float randomize_agR = 0f, float z = 0f, int time = 0, int saf = 0, int saf_randomize = 0)
		{
			float num = 6.2831855f / (float)count;
			float num2 = 0f;
			float num3 = this.circle_radius / this.CLENB;
			bool flag = false;
			if (Stb.isStart('*'))
			{
				Stb.Splice(0, 1);
				flag = true;
			}
			EfParticle efParticle = EfParticleManager.Get(rER.Hash.Get(Stb, 0, -1), false);
			bool flag2 = this.initRotationMatrixForCircle(null, false);
			Vector3 vector = Vector3.zero;
			this.calcAimPos(false);
			for (int i = 0; i < count; i++)
			{
				float num4 = ((randomize_agR != 0f) ? (num2 + num * (-0.5f + X.XORSP()) * randomize_agR) : num2);
				vector.Set(num3 * X.Cos(num4), -num3 * X.Sin(num4), 0f);
				vector = MagicItem.Mrx.MultiplyPoint3x4(vector);
				EffectItem effectItem = this.PtcNCen(efParticle, vector.x, vector.y, num4 + z + (flag2 ? this.aim_agR : 0f), time, saf + ((saf_randomize != 0) ? X.IntR((float)saf_randomize * X.XORSP()) : 0));
				if (flag)
				{
					rER.StockEffect(effectItem);
				}
				num2 += num;
			}
		}

		public bool readPtcScript(PTCThread rER)
		{
			if (this.killed || this.PtcHld == null)
			{
				return rER.quitReading();
			}
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2073177697U)
				{
					if (num <= 1286814321U)
					{
						if (num <= 363198355U)
						{
							if (num != 6730327U)
							{
								if (num != 27896808U)
								{
									if (num != 363198355U)
									{
										goto IL_0983;
									}
									if (!(cmd == "%SND"))
									{
										goto IL_0983;
									}
									goto IL_03B5;
								}
								else if (!(cmd == "%QU_HANDSHAKE"))
								{
									goto IL_0983;
								}
							}
							else
							{
								if (!(cmd == "%SND_INTERVAL2_SPOS"))
								{
									goto IL_0983;
								}
								if (X.DEBUGNOSND)
								{
									return true;
								}
								this.PtcHld.playSndInterval(rER, 1, 0, rER.Nm(2, 0f), this.sx, this.sy, rER.Int(6, -1), true, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_S).TimeAbsolute(rER.Nm(5, 1f) != 0f).Change(rER._3, rER.Nm(4, -1f));
								return true;
							}
						}
						else if (num != 836404553U)
						{
							if (num != 1209842151U)
							{
								if (num != 1286814321U)
								{
									goto IL_0983;
								}
								if (!(cmd == "%KILLEFFECT"))
								{
									goto IL_0983;
								}
								this.PtcHld.killPtc(false);
								return true;
							}
							else
							{
								if (!(cmd == "%SND_INTERVAL_SPOS"))
								{
									goto IL_0983;
								}
								if (X.DEBUGNOSND)
								{
									return true;
								}
								this.PtcHld.playSndInterval(rER, 1, 0, rER.Nm(2, 0f), this.sx, this.sy, rER.Int(4, -1), true, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_S).TimeAbsolute(rER.Nm(3, 1f) != 0f);
								return true;
							}
						}
						else
						{
							if (!(cmd == "%QU_SINH2"))
							{
								goto IL_0983;
							}
							goto IL_08B0;
						}
					}
					else if (num <= 1750609613U)
					{
						if (num != 1574621873U)
						{
							if (num != 1612293463U)
							{
								if (num != 1750609613U)
								{
									goto IL_0983;
								}
								if (!(cmd == "%HOLD"))
								{
									goto IL_0983;
								}
								PTCThread.StFollow stFollow;
								if (!FEnum<PTCThread.StFollow>.TryParse(rER._1, out stFollow, true))
								{
									rER.tError("不明な StFollow: " + rER._1);
								}
								else
								{
									this.PtcHld.changeCurrentBufferFollow(stFollow, rER);
									rER.follow = stFollow;
								}
								return true;
							}
							else
							{
								if (!(cmd == "%PHASE"))
								{
									goto IL_0983;
								}
								rER.Def("phase", (float)this.phase);
								return true;
							}
						}
						else
						{
							if (!(cmd == "%PVIB2"))
							{
								goto IL_0983;
							}
							goto IL_0949;
						}
					}
					else if (num != 2014939182U)
					{
						if (num != 2050094175U)
						{
							if (num != 2073177697U)
							{
								goto IL_0983;
							}
							if (!(cmd == "%CALCPOS"))
							{
								goto IL_0983;
							}
							rER.Def("cx", this.Cen.x);
							rER.Def("cy", this.Cen.y);
							this.calcTargetPos();
							rER.Def("tx", this.PosT.x);
							rER.Def("ty", this.PosT.y);
							bool flag = this.type == 1;
							this.calcAimPos(false);
							if (flag)
							{
								rER.Def("circlex", this.Cen.x + this.circle_rotate_shift / this.CLENB * X.Cos(this.aim_agR));
								rER.Def("circley", this.Cen.y - this.circle_rotate_shift / this.CLENB * X.Sin(this.aim_agR));
							}
							else
							{
								rER.Def("circlex", this.Cen.x);
								rER.Def("circley", this.Cen.y);
							}
							return true;
						}
						else
						{
							if (!(cmd == "%QU_SINV"))
							{
								goto IL_0983;
							}
							goto IL_08E2;
						}
					}
					else if (!(cmd == "%QU_HANDSHAKE2"))
					{
						goto IL_0983;
					}
					this.QuakeHandShake(rER.Nm(1, 0f), rER.Nm(2, 0f), rER.Nm(3, 1f), rER.Int(4, 0));
					return true;
				}
				if (num <= 2952609249U)
				{
					if (num <= 2491796050U)
					{
						if (num != 2215071754U)
						{
							if (num != 2419201793U)
							{
								if (num != 2491796050U)
								{
									goto IL_0983;
								}
								if (!(cmd == "%QU_VIB2"))
								{
									goto IL_0983;
								}
							}
							else
							{
								if (!(cmd == "%QU_SINH"))
								{
									goto IL_0983;
								}
								goto IL_08B0;
							}
						}
						else
						{
							if (!(cmd == "%SND_TPOS"))
							{
								goto IL_0983;
							}
							if (X.DEBUGNOSND)
							{
								return true;
							}
							this.calcTargetPos();
							this.PtcHld.playSndPos(rER, 1, rER.clength - 1, this.PosT.x, this.PosT.y, PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.FOLLOW_T, 1);
							return true;
						}
					}
					else if (num != 2684167345U)
					{
						if (num != 2700944964U)
						{
							if (num != 2952609249U)
							{
								goto IL_0983;
							}
							if (!(cmd == "%DATAS"))
							{
								goto IL_0983;
							}
							rER.Def("sx", this.sx);
							rER.Def("sy", this.sy);
							rER.Def("sa", this.sa);
							rER.Def("sz", this.sz);
							return true;
						}
						else
						{
							if (!(cmd == "%DATAD"))
							{
								goto IL_0983;
							}
							rER.Def("dx", this.dx);
							rER.Def("dy", this.dy);
							rER.Def("da", this.da);
							rER.Def("dz", this.dz);
							return true;
						}
					}
					else
					{
						if (!(cmd == "%DATAC"))
						{
							goto IL_0983;
						}
						rER.Def("cx", this.Cen.x);
						rER.Def("cy", this.Cen.y);
						return true;
					}
				}
				else if (num <= 3249949017U)
				{
					if (num != 3041186419U)
					{
						if (num != 3132573588U)
						{
							if (num != 3249949017U)
							{
								goto IL_0983;
							}
							if (!(cmd == "%SND_SPOS"))
							{
								goto IL_0983;
							}
							if (X.DEBUGNOSND)
							{
								return true;
							}
							this.PtcHld.playSndPos(rER, 1, rER.clength - 1, this.sx, this.sy, PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.FOLLOW_S, 1);
							return true;
						}
						else if (!(cmd == "%QU_VIB"))
						{
							goto IL_0983;
						}
					}
					else
					{
						if (!(cmd == "%SND2"))
						{
							goto IL_0983;
						}
						goto IL_03B5;
					}
				}
				else if (num <= 3704915169U)
				{
					if (num != 3382953879U)
					{
						if (num != 3704915169U)
						{
							goto IL_0983;
						}
						if (!(cmd == "%CALCAGR"))
						{
							goto IL_0983;
						}
						this.calcAimPos(false);
						rER.Def("agR", this.aim_agR);
						return true;
					}
					else
					{
						if (!(cmd == "%QU_SINV2"))
						{
							goto IL_0983;
						}
						goto IL_08E2;
					}
				}
				else if (num != 3836261763U)
				{
					if (num != 4233139481U)
					{
						goto IL_0983;
					}
					if (!(cmd == "%PVIB"))
					{
						goto IL_0983;
					}
					goto IL_0949;
				}
				else
				{
					if (!(cmd == "%MAGIC_CIRCLE_SET"))
					{
						goto IL_0983;
					}
					STB stb = TX.PopBld(null, 0);
					rER.CopyBaked(1, stb);
					this.setPtcToCircle(rER, stb, rER.Int(2, 4), rER.Nm(3, 0f), rER.Nm(4, 0f), rER.Int(5, 0), rER.Int(6, 0), rER.Int(7, 0));
					TX.ReleaseBld(stb);
					return true;
				}
				this.QuakeVib(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
				return true;
				IL_03B5:
				if (!X.DEBUGNOSND)
				{
					float num2 = (this.efpos_s ? this.sx : (this.efpos_d ? this.dx : this.Cen.x));
					float num3 = (this.efpos_s ? this.sy : (this.efpos_d ? this.dy : this.Cen.y));
					if (rER.clength >= 3 && rER.isNm(2))
					{
						this.PtcHld.playSndPos(rER, 1, 0, rER.Nm(2, 0f), rER.Nm(3, -1000f), PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.FOLLOW_C, (rER.cmd == "%SND2") ? 2 : 1);
					}
					else
					{
						this.PtcHld.playSndPos(rER, 1, rER.clength - 1, num2, num3, PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.NO_FOLLOW, (rER.cmd == "%SND2") ? 2 : 1);
					}
				}
				return true;
				IL_08B0:
				this.QuakeSinH(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
				return true;
				IL_08E2:
				this.QuakeSinV(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
				return true;
				IL_0949:
				for (int i = 1; i < rER.clength; i++)
				{
					this.PadVib(rER.getIndex(i), rER.cmd == "%PVIB2", 1f);
				}
				return true;
			}
			IL_0983:
			return this.Mp != null && this.Mp.M2D.readPtcScript(rER);
		}

		public void PadVib(string key, bool not_consider_length = false, float level = 1f)
		{
			if (CFG.vib_level == 0)
			{
				return;
			}
			if (!not_consider_length)
			{
				if (this.Ray == null)
				{
					return;
				}
				Vector2 mapPos = this.Ray.getMapPos(0f);
				M2Mover baseMover = this.M2D.Cam.getBaseMover();
				Vector2 vector = ((baseMover != null) ? new Vector2(baseMover.x, baseMover.y) : new Vector2(this.M2D.Cam.x * this.Mp.rCLEN, this.M2D.Cam.y * this.Mp.rCLEN));
				level *= 0.125f + 0.875f * X.Pow(1f - X.ZLINE(X.LENGTHXYN(mapPos.x, mapPos.y, vector.x, vector.y) - 8f, 8f), 2);
			}
			NEL.PadVib(key, level);
		}

		public bool isSoundActive(SndPlayer S)
		{
			M2SoundPlayerItem m2SoundPlayerItem = S as M2SoundPlayerItem;
			return m2SoundPlayerItem != null && this.Mp != null && this.PtcHld != null && this.PtcHld.isSoundActive(m2SoundPlayerItem);
		}

		protected float quake_level
		{
			get
			{
				M2Camera cam = this.M2D.Cam;
				float num = (this.efpos_s ? this.sx : (this.efpos_d ? this.dx : this.Cen.x));
				float num2 = (this.efpos_s ? this.sy : (this.efpos_d ? this.dy : this.Cen.y));
				return X.Pow(1f - X.ZLINE(X.LENGTHXYN(num, num2, cam.map_center_x, cam.map_center_y) - 8f, 8f), 2);
			}
		}

		public MagicItem QuakeVib(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			float quake_level = this.quake_level;
			if (quake_level > 0f)
			{
				this.M2D.Cam.Qu.Vib(_slevel * quake_level, _time, (_elevel < 0f) ? _elevel : (_elevel * quake_level), _saf);
			}
			return this;
		}

		public MagicItem QuakeSinH(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			float quake_level = this.quake_level;
			if (quake_level > 0f)
			{
				this.M2D.Cam.Qu.SinH(_slevel * quake_level, _time, (_elevel < 0f) ? _elevel : (_elevel * quake_level), _saf);
			}
			return this;
		}

		public MagicItem QuakeSinV(float _slevel, float _time, float _elevel = -1f, int _saf = 0)
		{
			float quake_level = this.quake_level;
			if (quake_level > 0f)
			{
				this.M2D.Cam.Qu.SinV(_slevel * quake_level, _time, (_elevel < 0f) ? _elevel : (_elevel * quake_level), _saf);
			}
			return this;
		}

		public MagicItem QuakeHandShake(float _holdtime, float _fadetime, float _level, int _saf = 0)
		{
			float quake_level = this.quake_level;
			if (quake_level > 0f)
			{
				this.M2D.Cam.Qu.HandShake((float)((int)_holdtime), _fadetime, _level * quake_level, _saf);
			}
			return this;
		}

		public MagicItem Identity()
		{
			MagicItem.Mrx = Matrix4x4.identity;
			return this;
		}

		public MagicItem Scale(float scalex, float scaley)
		{
			MagicItem.Mrx = Matrix4x4.Scale(new Vector3(scalex, scaley, 1f)) * MagicItem.Mrx;
			return this;
		}

		public MagicItem Rotate(float rotR)
		{
			MagicItem.Mrx = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotR / 3.1415927f * 180f)) * MagicItem.Mrx;
			return this;
		}

		public MagicItem Rotate01(float rot01)
		{
			return this.Rotate(rot01 * 6.2831855f);
		}

		public MagicItem TranslateP(float translatex, float translatey)
		{
			MagicItem.Mrx = Matrix4x4.Translate(new Vector3(translatex, translatey, 0f)) * MagicItem.Mrx;
			return this;
		}

		public bool closed
		{
			get
			{
				return (this.hittype & MGHIT.CLOSED) > (MGHIT)0;
			}
		}

		public bool killed
		{
			get
			{
				return this.hittype == MGHIT.KILLED;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.MGC.Mp;
			}
		}

		public float CLEN
		{
			get
			{
				return this.MGC.Mp.CLEN;
			}
		}

		public float CLENB
		{
			get
			{
				return this.MGC.Mp.CLENB;
			}
		}

		public bool hit_pr
		{
			get
			{
				return (this.hittype & MGHIT.EN) > (MGHIT)0;
			}
			set
			{
				this.hittype = (value ? (this.hittype | MGHIT.EN) : (this.hittype & (MGHIT)(-3)));
			}
		}

		public bool hit_en
		{
			get
			{
				return (this.hittype & MGHIT.PR) > (MGHIT)0;
			}
			set
			{
				this.hittype = (value ? (this.hittype | MGHIT.PR) : (this.hittype & (MGHIT)(-2)));
			}
		}

		public bool calced_target_pos
		{
			get
			{
				return (this.flags & 1) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 1) : (this.flags & -2));
			}
		}

		public bool calced_aim_pos
		{
			get
			{
				return (this.flags & 2) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 2) : (this.flags & -3));
			}
		}

		public bool exploded
		{
			get
			{
				return (this.flags & 4) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 4) : (this.flags & -5));
			}
		}

		public bool target_s
		{
			get
			{
				return (this.flags & 8) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 8) : (this.flags & -9));
			}
		}

		public bool target_d
		{
			get
			{
				return (this.flags & 16) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 16) : (this.flags & -17));
			}
		}

		public bool raypos_s
		{
			get
			{
				return (this.flags & 32) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 32) : (this.flags & -33));
			}
		}

		public bool raypos_d
		{
			get
			{
				return (this.flags & 64) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 64) : (this.flags & -65));
			}
		}

		public bool efpos_s
		{
			get
			{
				return (this.flags & 128) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 128) : (this.flags & -129));
			}
		}

		public bool efpos_d
		{
			get
			{
				return (this.flags & 256) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 256) : (this.flags & -257));
			}
		}

		public bool raypos_c
		{
			get
			{
				return (this.flags & 1024) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 1024) : (this.flags & -1025));
			}
		}

		public bool aimagr_calc_s
		{
			get
			{
				return (this.flags & 2048) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 2048) : (this.flags & -2049));
				this.calced_aim_pos = false;
			}
		}

		public bool aimagr_calc_d
		{
			get
			{
				return (this.flags & 4096) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 4096) : (this.flags & -4097));
				this.calced_aim_pos = false;
			}
		}

		public bool aimagr_calc_vector_d
		{
			get
			{
				return (this.flags & 65536) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 65536) : (this.flags & -65537));
				this.calced_aim_pos = false;
			}
		}

		public bool explode_pos_c
		{
			get
			{
				return (this.flags & 8192) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 8192) : (this.flags & -8193));
			}
		}

		public bool no_kill_effect_when_close
		{
			get
			{
				return (this.flags & 16384) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 16384) : (this.flags & -16385));
			}
		}

		public bool chant_finished
		{
			get
			{
				return (this.flags & 32768) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 32768) : (this.flags & -32769));
			}
		}

		public bool input_null_to_other_when_quit
		{
			get
			{
				return (this.flags & 131072) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 131072) : (this.flags & -131073));
			}
		}

		public bool padvib_enable
		{
			get
			{
				return (this.flags & 262144) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 262144) : (this.flags & -262145));
			}
		}

		public bool splash_fall_reduce
		{
			get
			{
				return (this.flags & 524288) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 524288) : (this.flags & -524289));
			}
		}

		public bool mana_absorb_replace
		{
			get
			{
				return (this.flags & 1048576) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 1048576) : (this.flags & -1048577));
			}
		}

		public bool already_reflected
		{
			get
			{
				return this.Ray != null && (this.Ray.hittype & HITTYPE.REFLECTED) > HITTYPE.NONE;
			}
		}

		public static bool drawNone(MagicItem Mg, float fcnt)
		{
			return true;
		}

		public static bool runMagicCircle(MagicItem Mg, float fcnt)
		{
			if (Mg.closed && Mg.sa < 100f)
			{
				Mg.sa = 100f + Mg.t;
				Mg.dz = (float)MDAT.getExplodeCircleReleaseTime(Mg);
				if (Mg.casttime >= 0f)
				{
					Mg.casttime = Mg.t + Mg.dz;
				}
				Mg.PtcST("magic_circle_close", PTCThread.StFollow.NO_FOLLOW, false);
			}
			Mg.TS = ((Mg.t >= Mg.casttime) ? 1f : Mg.Caster.getCastingTimeScale(Mg));
			bool flag = false;
			if (Mg.sa < 100f)
			{
				if (Mg.t == 0f)
				{
					Mg.sz = 1f;
					Mg.dx = 0f;
					if (Mg.casttime >= 0f)
					{
						Mg.PtcST("magic_init", PTCThread.StFollow.NO_FOLLOW, false);
					}
				}
				flag = true;
			}
			else
			{
				if (Mg.Caster.isManipulatingMagic(Mg))
				{
					Mg.dy = Mg.dz;
					if (Mg.casttime >= 0f)
					{
						Mg.casttime = Mg.t + (float)((int)Mg.dz);
					}
					flag = true;
				}
				if (Mg.dy > 0f)
				{
					Mg.dy = X.Mx(0f, Mg.dy - fcnt);
				}
				if (Mg.t >= Mg.casttime)
				{
					return false;
				}
			}
			if (flag)
			{
				if (Mg.sz > 0f)
				{
					Mg.calcAimPos(false);
					int aimDirection = Mg.Caster.getAimDirection();
					bool flag2;
					if (aimDirection >= 0)
					{
						int num = CAim._XD(aimDirection, 1);
						flag2 = num != 0 && num != CAim._XD(Mg.Caster.getAimForCaster(), 1);
					}
					else
					{
						flag2 = X.Abs(Mg.Cen.x - Mg.PosA.x) > 0.0625f && Mg.Cen.x < Mg.PosA.x != CAim._XD(Mg.Caster.getAimForCaster(), 1) > 0;
					}
					if (flag2)
					{
						float num2 = Mg.sz + 1f;
						Mg.sz = num2;
						if (num2 >= 8f)
						{
							Mg.sz = 1f;
							Mg.Caster.setAimForCaster((Mg.Cen.x < Mg.PosA.x) ? AIM.R : AIM.L);
						}
					}
					else
					{
						Mg.sz = 1f;
					}
					float num3 = X.angledifR(Mg.da, Mg.aim_agR);
					float num4 = X.Abs(num3);
					if (num4 < 0.024543693f)
					{
						Mg.da = Mg.aim_agR;
					}
					else if (num4 >= 1.8849558f)
					{
						Mg.da = X.correctangleR(Mg.da + 3.1415927f);
						Mg.dx = 20f;
					}
					else
					{
						Mg.da = X.correctangleR(Mg.da + num3 * 0.21f);
					}
					if (Mg.dx > 0f)
					{
						Mg.dx = X.Mx(0f, Mg.dx - fcnt);
					}
				}
				if (Mg.sa == 0f && Mg.casttime >= 0f && Mg.t >= Mg.casttime)
				{
					Mg.sa = 1f;
					Mg.chant_finished = true;
					if ((Mg.hittype & MGHIT.PR) != (MGHIT)0)
					{
						Mg.PtcVar("centerpr", (double)((Mg.Caster as M2MoverPr == Mg.Mp.Pr) ? 1 : 0));
					}
					Mg.PtcST("magic_chanted", PTCThread.StFollow.NO_FOLLOW, false);
				}
			}
			return true;
		}

		public bool initRotationMatrixForCircle(MeshDrawer Md = null, bool pixel_mode = false)
		{
			if (this.type == 1)
			{
				this.calcAimPos(false);
				return this.initRotationMatrixForCircle(Md, this.da, X.ZPOW3(this.dx, 20f), pixel_mode);
			}
			return false;
		}

		public bool initRotationMatrixForCircle(MeshDrawer Md, float agR, float flip_level = 0f, bool pixel_mode = false)
		{
			bool flag = this.type == 1;
			MagicItem.Mrx = Matrix4x4.identity;
			if (flag)
			{
				float num = X.NI(1, -1, flip_level);
				float num2 = X.NI(1f, 0.78f, X.ZLINE(X.Abs(flip_level - 0.5f), 0.5f));
				this.Scale(num2, 1f).TranslateP(num * this.circle_rotate_shift * (pixel_mode ? 1f : (1f / this.CLENB)), 0f).Rotate((float)(pixel_mode ? 1 : (-1)) * agR);
				if (Md != null)
				{
					Md.Identity().Scale(num2, 1f, false).TranslateP(num * this.circle_rotate_shift, 0f, false)
						.Rotate(agR, false);
				}
				return true;
			}
			return false;
		}

		public static bool drawMagicCircle(MagicItem Mg, float fcnt)
		{
			if (!Mg.isPreparingCircle)
			{
				return true;
			}
			MeshDrawer mesh = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
			mesh.Col = C32.d2c(Mg.hit_pr ? 4294948667U : 4288467960U);
			float num = Mg.t / 48f * 3.1415927f;
			if (!Mg.closed)
			{
				float num2 = X.ZSIN(Mg.t, Mg.casttime);
				mesh.ColGrd.Set(mesh.Col).setA1(0f);
				float num3 = Mg.circle_radius;
				Mg.initRotationMatrixForCircle(mesh, true);
				if (num2 < 1f)
				{
					float num4 = num2 * 1.5707964f;
					float num5 = 1f + 0.14f * X.COSIT(3.77f);
					MagicItem.Halo.Set(4f * num5, 40f * num5, -1f, -1f, -1f, -1f).Col(mesh.Col).mulA(0.67f + 0.19f * X.COSIT(3.27f));
					for (int i = 0; i < 2; i++)
					{
						float num6 = ((i == 0) ? 0f : 3.1415927f);
						mesh.BlurArc(0f, 0f, num3, num6 - num4, num6 + num4, 4f, 3);
					}
					mesh.Identity();
					Vector3 vector = Vector3.zero;
					for (int j = 0; j < 2; j++)
					{
						float num7 = ((j == 0) ? 0f : 3.1415927f);
						for (int k = 0; k < 2; k++)
						{
							float num8 = num7 + (float)X.MPF(k == 0) * num4;
							vector.Set(num3 * X.Cos(num8), num3 * X.Sin(num8), 0f);
							vector = MagicItem.Mrx.MultiplyPoint3x4(vector);
							MagicItem.Halo.drawTo(mesh, vector.x, vector.y, 0.7853982f, false, 3);
						}
					}
				}
				else
				{
					float num9 = 1f - X.ZSIN(Mg.t, 14f);
					mesh.ColGrd.Set(mesh.Col).setA1(0.4f * (1f - X.ZSIN(Mg.t, 77f)));
					float num10 = X.ZPOW3(Mg.dx, 20f);
					float num11 = X.NI(4f, 1f, X.ZSIN(X.Abs(num10 - 0.5f), 0.5f));
					mesh.BlurCircle2(0f, 0f, num3 * (1f + 0.7f * num9) - 4f, (6f + 60f * num9) * num11, (6f + 60f * (1f - X.ZSIN(Mg.t, 33f))) * num11, null, null);
					mesh.BlurPoly(0f, 0f, num3 * 0.97f, num, 4, 8f * num11).BlurPoly(0f, 0f, num3 * 0.97f, num + 0.7853982f, 4, 8f * num11);
					mesh.Identity();
				}
			}
			else
			{
				mesh.ColGrd.Set(mesh.Col);
				float num12 = X.ZLINE(Mg.t - (Mg.sa - 100f), Mg.dz);
				float num13 = (1f - 0.44f * X.ZSIN2(num12 - 0.15f, 0.1f) - 0.56f * X.ZSIN2(num12 - 0.68f, 0.32f)) * (0.8f + 0.2f * X.COSIT(11.57f));
				float num14 = Mg.circle_radius * (1f + 0.3f * X.ZSIN(num12 - 0.05f, 0.04f) - 0.87f * X.ZPOW(num12 - 0.2f, 0.04f));
				float num15 = Mg.circle_radius * (0.3f + X.ZPOW(num12 - 0.08f, 0.12f) * 0.8f - 1f * X.ZPOW(num12 - 0.24f, 0.44f));
				Mg.initRotationMatrixForCircle(mesh, true);
				if (num12 < 1f)
				{
					C32 c = mesh.ColGrd.Set(mesh.Col).setA1(0.2f + 0.8f * X.ZSIN(num12, 0.07f)).mulA(num13);
					C32 c2 = EffectItem.Col1.Set(mesh.Col).setA1(0.4f - X.ZSINV(num12, 0.7f)).mulA(num13);
					mesh.Col.a = (byte)(255f * num13);
					mesh.BlurPoly2(0f, 0f, num14 * 0.82f, num, 4, num14 + 1f, num15, c, c2).BlurPoly2(0f, 0f, num14 * 0.82f, num + 0.7853982f, 4, num14 + 1f, num15, c, c2);
				}
				if (Mg.dy > 0f)
				{
					num14 = Mg.circle_radius;
					float num16 = X.ZLINE(Mg.dy, Mg.dz);
					Vector3 vector2 = Vector3.zero;
					float num17 = 1f + 0.14f * X.COSIT(3.77f);
					C32 col = EffectItem.Col1;
					uint num18 = (Mg.hit_pr ? 8479488U : 4947U);
					for (int l = 0; l < 2; l++)
					{
						float num19 = num + (float)l * 3.1415927f;
						int num20 = mesh.getVertexMax() + 1;
						mesh.Col = mesh.ColGrd.setA1(num16).C;
						MagicItem.Halo.Col(mesh.Col).mulA(0.67f + 0.19f * X.COSIT(3.27f));
						mesh.ColGrd.mulA(0.125f);
						mesh.BlurArc(0f, 0f, num14, num19 - 2.0734513f, num19, 6f, 1);
						int num21 = (mesh.getVertexMax() - num20) / 3 - 1;
						float num22 = 1f / (float)num21;
						Color32[] colorArray = mesh.getColorArray();
						for (int m = 0; m <= num21; m++)
						{
							float num23 = 1f - X.ZPOW3((float)m * num22);
							int num24 = num20 + m * 3;
							colorArray[num24] = col.Set(colorArray[num24]).blend(num18, num23).C;
							colorArray[num24 + 1] = col.Set(colorArray[num24 + 1]).blend(num18, num23).C;
							colorArray[num24 + 2] = col.Set(colorArray[num24 + 2]).blend(num18, num23).C;
						}
						colorArray[num20 - 1] = col.Set(colorArray[num20 - 1]).blend(num18, 1f).C;
					}
					mesh.Identity();
					MagicItem.Halo.Set(10f * num17, 22f * num17, -1f, -1f, -1f, -1f);
					for (int n = 0; n < 2; n++)
					{
						float num25 = num + (float)n * 3.1415927f;
						vector2.Set(num14 * X.Cos(num25), num14 * X.Sin(num25), 0f);
						vector2 = MagicItem.Mrx.MultiplyPoint3x4(vector2);
						MagicItem.Halo.drawTo(mesh, vector2.x, vector2.y, 0.7853982f, false, 3);
					}
				}
				mesh.Identity();
			}
			Mg.Identity();
			return true;
		}

		public static bool runOnlyMakeRay(MagicItem Mg, float fcnt)
		{
			if (Mg.t == 0f && Mg.Ray == null)
			{
				Mg.changeRay(Mg.MGC.makeRay(Mg, 0.3f, (Mg.hittype & MGHIT.EN) == (MGHIT)0, true));
			}
			return true;
		}

		public static bool runTackle(MagicItem Mg, float fcnt)
		{
			if (Mg.t == 0f)
			{
				if (Mg.Ray == null)
				{
					Mg.changeRay(Mg.MGC.makeRay(Mg, 0.3f, (Mg.hittype & MGHIT.EN) == (MGHIT)0, true));
				}
				if (Mg.Ray.get_hit_lock() == 0f)
				{
					Mg.Ray.HitLock(-1f, null);
				}
			}
			if (Mg.phase == -1)
			{
				Mg.explodeManaToRay(1f);
				return false;
			}
			if (Mg.sz >= 0f)
			{
				Mg.Ray.DirXyM(Mg.sx, Mg.sy);
				Mg.Ray.PosMap(Mg.Cen.x, Mg.Cen.y);
				Mg.Ray.RadiusM(Mg.sz);
				Mg.Atk0.BurstDir(Mg.mpf_is_right);
			}
			else
			{
				if (Mg.dx != 0f || Mg.dy != 0f)
				{
					Mg.Ray.DirXyM(Mg.dx, Mg.dy);
				}
				else
				{
					Mg.Ray.LenM(0f);
				}
				Mg.Ray.PosMap(Mg.Cen.x + Mg.sx, Mg.Cen.y + Mg.sy);
				Mg.Ray.RadiusM(-Mg.sz);
			}
			if (fcnt == 0f)
			{
				return true;
			}
			if (!Mg.Caster.canHoldMagic(Mg) || Mg.Atk0 == null)
			{
				Mg.explodeManaToRay(1f);
				return false;
			}
			HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
			if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE || ((Mg.phase & 1) == 0 && (hittype & HITTYPE.BREAK) != HITTYPE.NONE) || ((Mg.phase & 4) == 0 && (hittype & HITTYPE.REFLECTED) != HITTYPE.NONE))
			{
				Mg.explodeManaToRay(1f);
				return false;
			}
			return true;
		}

		public static bool runEfWormPublish(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.efpos_s = true;
				Mg.dz += X.NIXP(8f, 18f);
				Mg.dx = 3f + X.XORSP() * 11f;
			}
			Mg.sx = Mg.Cen.x;
			Mg.sy = Mg.Cen.y;
			Mg.dx -= fcnt;
			if (Mg.dx <= 0f)
			{
				M2WormTrap.setWormRelease(Mg.Caster as M2Mover);
				Mg.dx = 1f + X.XORSP() * 2f;
				float num = Mg.dz - 1f;
				Mg.dz = num;
				if (num <= 0f)
				{
					return false;
				}
			}
			return true;
		}

		public NelM2DBase M2D
		{
			get
			{
				return this.MGC.M2D;
			}
		}

		public bool is_normal_attack
		{
			get
			{
				return (this.hittype & MGHIT.NORMAL_ATTACK) > (MGHIT)0;
			}
		}

		public bool isPlayerShotgun()
		{
			return this.Caster is PR && (this.Caster as PR).Skill.isShotgun(this);
		}

		public MagicItem Sleep()
		{
			if (!this.is_sleep)
			{
				if (!this.no_kill_effect_when_close)
				{
					this.killEffect();
				}
				this.hittype |= MGHIT.SLEEP;
				if (this.Ed != null)
				{
					this.Ed = this.Mp.remED(this.Ed);
				}
				this.Ef = null;
				this.PtcVar("finished", (double)(this.chant_finished ? 1 : 0));
				this.PtcST("magic_hold", PTCThread.StFollow.NO_FOLLOW, false);
			}
			return this;
		}

		public MagicItem castedTimeResetTo(float _t)
		{
			if (this.exploded || !this.isPreparingCircle)
			{
				return this;
			}
			this.t = _t;
			if (this.chant_finished && this.t < this.casttime)
			{
				this.killEffect();
				this.chant_finished = false;
				this.PtcVar("finished", 0.0);
				this.PtcST("magic_hold", PTCThread.StFollow.NO_FOLLOW, false);
			}
			return this;
		}

		public MagicItem Awake()
		{
			this.hittype &= (MGHIT)(-2049);
			if (this.fnDrawMain != null && this.Ed == null)
			{
				this.Ed = this.Mp.setED("magic_draw", this.FD_DrawDelegate, 0f);
			}
			if (this.fnRunMain == MagicItem.FD_runMagicCircle)
			{
				this.run(1f);
			}
			else if (this.fnRunMain == MagicItem.FD_runOnlyMakeRay)
			{
				this.run(0f);
			}
			return this;
		}

		public void playSndPos(string cue_key, bool no_killable = true)
		{
			this.calcTargetPos();
			this.playSndPos(cue_key, this.PosT.x, this.PosT.y, no_killable, PTCThread.StFollow.NO_FOLLOW);
		}

		public void playSndPos(StringKey cue_key, bool no_killable = true)
		{
			this.calcTargetPos();
			this.playSndPos(cue_key, this.PosT.x, this.PosT.y, no_killable, PTCThread.StFollow.NO_FOLLOW);
		}

		public M2SoundPlayerItem playSndPos(string cue_key, float x, float y, bool no_killable = true, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW)
		{
			return this.PtcHld.playSndPos(cue_key, x, y, no_killable ? PtcHolder.PTC_HOLD.NO_HOLD : PtcHolder.PTC_HOLD.NORMAL, _follow, null, 1);
		}

		public M2SoundPlayerItem playSndPos(StringKey cue_key, float x, float y, bool no_killable = true, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW)
		{
			return this.PtcHld.playSndPos(cue_key, x, y, PtcHolder.PTC_HOLD.NO_HOLD, _follow, null, 1);
		}

		public M2SndInterval playSndInterval(string cue_key, float intv, float x, float y, int playcount = 128, bool no_killable = true, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW)
		{
			return this.PtcHld.playSndInterval(cue_key, intv, x, y, playcount, no_killable, PtcHolder.PTC_HOLD.NORMAL, _follow, null);
		}

		public void destruct()
		{
			if (!this.killed)
			{
				this.kill(-1f);
			}
			this.Caster = null;
			if (this.Ed != null)
			{
				this.Ed = this.Mp.remED(this.Ed);
			}
			this.Mn = null;
			this.Ef = null;
		}

		public float mpf_is_right
		{
			get
			{
				return (float)(this.is_right ? 1 : (-1));
			}
		}

		public bool is_right
		{
			get
			{
				if (this.Ray != null)
				{
					return this.Ray.difmapx > 0f;
				}
				this.calcAimPos(false);
				return this.PosA.x > this.sx;
			}
		}

		public M2Ray PrepareRay()
		{
			if (this.Ray == null)
			{
				if (this.Mn != null)
				{
					this.changeRay(this.Mn.makeRayForMagic(this, 0));
				}
				else
				{
					this.changeRay(this.MGC.makeRay(this, (this.Mn != null) ? this.Mn._0.thick : 0f, true, true));
				}
			}
			return this.Ray;
		}

		public bool isPreparingCircle
		{
			get
			{
				return (this.hittype & MGHIT.IMMEDIATE) == (MGHIT)0 && this.casttime != 0f;
			}
		}

		public bool is_sleep
		{
			get
			{
				return (this.hittype & MGHIT.SLEEP) > (MGHIT)0;
			}
		}

		public bool is_chanted_magic
		{
			get
			{
				return (this.hittype & MGHIT.CHANTED) > (MGHIT)0;
			}
		}

		public bool is_baby
		{
			get
			{
				return this.t < 1f;
			}
		}

		public float rCLEN
		{
			get
			{
				return this.Mp.rCLEN;
			}
		}

		public float rCLENB
		{
			get
			{
				return this.Mp.rCLENB;
			}
		}

		public int projectile_power
		{
			get
			{
				return this.projectile_power_;
			}
			set
			{
				this.projectile_power_ = value;
				if (this.Ray != null)
				{
					this.Ray.projectile_power = value;
				}
			}
		}

		public float circle_rotate_shift
		{
			get
			{
				return this.circle_radius * 0.53f;
			}
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"<MagicItem>",
				this.kind.ToString(),
				"(p:",
				this.phase.ToString(),
				") ",
				this.sx.ToString(),
				",",
				this.sy.ToString(),
				",",
				this.sz.ToString(),
				",",
				this.sa.ToString(),
				"/",
				this.dx.ToString(),
				",",
				this.dy.ToString(),
				",",
				this.dz.ToString(),
				",",
				this.da.ToString()
			});
		}

		public bool isActive(M2MagicCaster Par, MGKIND k)
		{
			return this.Caster == Par && this.kind == k && !this.killed && !this.isPreparingCircle;
		}

		public bool isActive(M2MagicCaster Par, int _id)
		{
			return this.Caster == Par && this.id == _id && !this.killed && !this.isPreparingCircle;
		}

		public bool isActive(M2MagicCaster Par, MagicItem.FnMagicRun FD)
		{
			return this.Caster == Par && !this.killed && !this.isPreparingCircle && this.fnRunMain == FD;
		}

		public bool killedCheck(M2MagicCaster Par, MGKIND k)
		{
			return Par != this.Caster || this.kind != k;
		}

		public bool runHinagata(float fcnt)
		{
			return true;
		}

		public bool drawHinagata(float fcnt)
		{
			return true;
		}

		public readonly MGContainer MGC;

		public int id;

		public M2MagicCaster Caster;

		public MGKIND kind;

		public MGHIT hittype = MGHIT.KILLED;

		public int type;

		public float casttime;

		public float reduce_mp;

		public float mp_crystalize;

		public float crystalize_neutral_ratio;

		public M2DropObject Dro;

		private int projectile_power_ = -1;

		public float sx;

		public float sy;

		public float sz;

		public float sa;

		public float dx;

		public float dy;

		public float dz;

		public float da;

		public int phase;

		public float t = -1f;

		public float TS = 1f;

		public float reflect_lock_floort;

		public MagicNotifiear Mn;

		public NelAttackInfo Atk0;

		public NelAttackInfo Atk1;

		public NelAttackInfo Atk2;

		public Vector2 Cen;

		public int flags;

		public Vector2 PosT;

		public Vector2 PosA;

		public float aim_agR;

		private M2Ray Ray_;

		public static Matrix4x4 Mrx;

		private PtcHolder PtcHld;

		public static HaloDrawer Halo;

		private float circle_radius = 150f;

		private MagicItem.FnMagicRun fnRunMain;

		private MagicItem.FnMagicRun fnDrawMain;

		public M2DrawBinder Ed;

		private M2DrawBinder.FnEffectBind FD_DrawDelegate;

		public EffectItem Ef;

		public object Other;

		public float wind_apply_s_level;

		public Vector3 wind_velocity;

		public const int MAGICCIRCLE_FLIP_T = 20;

		private const int REFLECT_LOCK_T = 15;

		private static MagicItem.FnMagicRun FD_runMagicCircle = new MagicItem.FnMagicRun(MagicItem.runMagicCircle);

		public static MagicItem.FnMagicRun FD_runOnlyMakeRay = new MagicItem.FnMagicRun(MagicItem.runOnlyMakeRay);

		public delegate bool FnCheckMagicFrom(MagicItem Mg, M2MagicCaster CheckedBy);

		public delegate bool FnMagicRun(MagicItem Mg, float fcnt);
	}
}
