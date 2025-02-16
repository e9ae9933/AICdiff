using System;
using m2d;
using nel.smnp;
using XX;

namespace nel
{
	public abstract class NelEnemyNested : NelEnemy
	{
		public static NelEnemyNested CreateNest(NelEnemy _Parent, string enemykindkey, float mp_ratio, int array_create_capacity = 4)
		{
			SummonerPlayer summoner = _Parent.Summoner;
			SmnEnemyKind smnEnemyKind = new SmnEnemyKind(enemykindkey, 1, -1, mp_ratio, mp_ratio, "", 0f, 0f, ENATTR.NORMAL)
			{
				temporary_adding_count = true,
				nattr = _Parent.nattr
			};
			SmnPoint smnPoint = new SmnPoint(summoner, _Parent.x, _Parent.y, 1f, null)
			{
				sudden_appear = true
			};
			NelEnemyNested nelEnemyNested = summoner.summonEnemyFromOther(smnEnemyKind, smnPoint) as NelEnemyNested;
			if (nelEnemyNested != null)
			{
				return nelEnemyNested.initNest(_Parent, array_create_capacity);
			}
			return null;
		}

		public virtual NelEnemyNested initNest(NelEnemy _Parent, int array_create_capacity = 4)
		{
			this.Parent = _Parent;
			this.do_not_shuffle_on_cheat = true;
			this.ringoutable = false;
			this.Parent.addNestedChild(this, array_create_capacity);
			this.Parent.getAnimator().checkframe_on_drawing = false;
			base.disappearing = this.Parent.disappearing;
			this.Nai.AimPr = this.Parent.AimPr;
			this.Nai.fnSearchPlayer = delegate(float fix)
			{
				if (this.Parent == null || this.Parent.destructed)
				{
					return this.Nai.SearchPlayerDefault(fix);
				}
				return this.Parent.AimPr;
			};
			return this;
		}

		public override void awakeInit()
		{
			this.Nai.RemF(NAI.FLAG.AWAKEN);
			this.Nai.RemF(NAI.FLAG.RECHECK_PLAYER);
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			this.hp0_remove = true;
			if (this.Parent != null)
			{
				this.tecolor_link = false;
				this.Parent.remNestedChild(this);
			}
			base.destruct();
		}

		public NelEnemyNested NestSerResist()
		{
			this.Ser.Regist = this.Parent.getSer().Regist;
			return this;
		}

		public NelEnemyNested NestTeColor(bool flag = true)
		{
			this.tecolor_link = flag;
			return this;
		}

		public NelEnemyNested NestManaAbsorb(bool flag = true)
		{
			this.mana_absorb_to_parent = flag;
			return this;
		}

		public bool tecolor_link
		{
			get
			{
				return this.tecolor_link_;
			}
			set
			{
				if (this.tecolor_link == value)
				{
					return;
				}
				this.tecolor_link_ = value;
				if (this.Parent == null || this.Parent.TeCon == null)
				{
					return;
				}
				if (value)
				{
					this.Parent.TeCon.RegisterCol(this.Anm, false);
					if (this.TeCon != null)
					{
						this.TeCon.UnregisterCol(this.Anm, false);
						return;
					}
				}
				else
				{
					this.Parent.TeCon.UnregisterCol(this.Anm, false);
					if (this.TeCon != null)
					{
						this.TeCon.RegisterCol(this.Anm, false);
					}
				}
			}
		}

		public override void changeStateToDie()
		{
			if (base.destructed || this.hp0_remove || this.state == NelEnemy.STATE.DIE)
			{
				base.changeStateToDie();
			}
		}

		public override float getMpDesireRatio(int add_mp)
		{
			if (this.mana_absorb_to_parent)
			{
				return this.Parent.getMpDesireRatio(add_mp);
			}
			return base.getMpDesireRatio(add_mp);
		}

		public override void addHpFromMana(M2Mana Mana, float val)
		{
			if (this.mana_absorb_to_parent)
			{
				this.Parent.addHpFromMana(Mana, val);
				return;
			}
			base.addHpFromMana(Mana, val);
		}

		public override void addMpFromMana(int val)
		{
			if (this.mana_absorb_to_parent)
			{
				this.Parent.addMpFromMana(val);
				return;
			}
			base.addMpFromMana(val);
		}

		public override void addHpWithAbsorbing(int val)
		{
			if (this.mana_absorb_to_parent)
			{
				this.Parent.addHpWithAbsorbing(val);
				return;
			}
			base.addHpWithAbsorbing(val);
		}

		public override bool Useable(NOD.MpConsume _Mcs, float ratio = 1f, float add = 0f)
		{
			if (this.mana_absorb_to_parent)
			{
				return this.Parent.Useable(_Mcs, ratio, add);
			}
			return base.Useable(_Mcs, ratio, add);
		}

		public override void MpConsume(float x, float y, NOD.MpConsume _Mcs, MagicItem Mg = null, float ratio = 1f, float release_ratio = 1f)
		{
			if (this.mana_absorb_to_parent)
			{
				this.Parent.MpConsume(x, y, _Mcs, Mg, ratio, release_ratio);
				return;
			}
			base.MpConsume(x, y, _Mcs, Mg, ratio, release_ratio);
		}

		public override void cureMp(int val)
		{
			if (this.mana_absorb_to_parent)
			{
				this.Parent.cureMp(val);
				return;
			}
			base.cureMp(val);
		}

		public override float getEnlargeLevel()
		{
			if (this.mana_absorb_to_parent)
			{
				return this.Parent.getEnlargeLevel();
			}
			return base.getEnlargeLevel();
		}

		public override bool runDamageSmall()
		{
			if (this.t <= 0f && this.sync_damagestate)
			{
				this.Parent.changeStateDamageFromNest();
			}
			return base.runDamageSmall();
		}

		public override bool runDamageHuttobi()
		{
			if (this.t <= 0f && this.sync_damagestate)
			{
				this.Parent.changeStateDamageHuttobiFromNest();
			}
			return base.runDamageHuttobi();
		}

		public override bool cannotHitTo(M2Mover Mv)
		{
			return Mv == this.Parent || base.cannotHitTo(Mv);
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			return base.applyDamage(Atk, force);
		}

		public override TransEffecterItem setDmgBlink(MGATTR attr, float maxt = 0f, float factor = 1f, int _saf = 0)
		{
			if (this.tecolor_link)
			{
				return this.Parent.setDmgBlink(attr, maxt, factor, _saf);
			}
			return base.setDmgBlink(attr, maxt, factor, _saf);
		}

		public override TransEffecterItem setDmgBlinkFading(MGATTR attr, float maxt = 0f, float factor = 1f, int _saf = 0)
		{
			if (this.tecolor_link)
			{
				return this.Parent.setDmgBlinkFading(attr, maxt, factor, _saf);
			}
			return base.setDmgBlinkFading(attr, maxt, factor, _saf);
		}

		public override void setAbsorbBlink(AbsorbManager Absorb)
		{
			if (this.tecolor_link)
			{
				this.Parent.setAbsorbBlink(Absorb);
				return;
			}
			base.setAbsorbBlink(Absorb);
		}

		public override void setAbsorbBlink(float map_pixel_x, float map_pixel_y)
		{
			if (this.tecolor_link)
			{
				this.Parent.setAbsorbBlink(map_pixel_x, map_pixel_y);
				return;
			}
			base.setAbsorbBlink(map_pixel_x, map_pixel_y);
		}

		public override void prepareHpMpBarMesh()
		{
			if (this.sync_damagestate && base.hasF(NelEnemy.FLAG.FINE_HPMP_BAR_CREATE))
			{
				this.Parent.addF((NelEnemy.FLAG)768);
				this.Parent.prepareHpMpBarMesh();
			}
			else if (this.damage_parry > 0f)
			{
				this.Parent.prepareHpMpBarMesh();
			}
			if (this.damage_parry < 1f)
			{
				base.prepareHpMpBarMesh();
			}
		}

		public override void RegisterToTeCon(ITeColor TCol, ITeScaler TScl, ITeShift TShift)
		{
			if (this.tecolor_link)
			{
				this.Parent.RegisterToTeCon(TCol, TScl, TShift);
				return;
			}
			base.RegisterToTeCon(TCol, TScl, TShift);
		}

		public override int applyHpDamage(int val, ref int mpdmg, bool force, NelAttackInfo Atk)
		{
			int num = 0;
			if (this.damage_parry > 0f)
			{
				float num2 = this.damage_parry;
				int num3 = (int)((float)mpdmg * num2);
				int num4 = num3;
				num += this.Parent.applyHpDamage((int)((float)val * num2), ref num3, force, Atk);
				mpdmg = X.Mx(0, mpdmg + num3 - num4);
				if (!this.Parent.is_alive)
				{
					this.Parent.changeStateToDie();
				}
			}
			if (this.damage_parry < 1f)
			{
				float num5 = 1f - this.damage_parry;
				int num6 = (int)((float)mpdmg * num5);
				int num7 = num6;
				num += base.applyHpDamage((int)((float)val * num5), ref num6, force, Atk);
				mpdmg = X.Mx(0, mpdmg + num6 - num7);
			}
			return num;
		}

		public override int applyMpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			if (this.mana_absorb_to_parent)
			{
				return this.Parent.applyMpDamage(val, force, Atk);
			}
			int num = 0;
			if (this.damage_parry > 0f)
			{
				float num2 = this.damage_parry;
				num += this.Parent.applyMpDamage((int)((float)val * num2), force, Atk);
			}
			if (this.damage_parry < 1f)
			{
				float num3 = 1f - this.damage_parry;
				num += base.applyMpDamage((int)((float)val * num3), force, Atk);
			}
			return num;
		}

		public override bool isRingOut()
		{
			return false;
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if ((Ray.hittype & HITTYPE.GUARD_IGNORE) != HITTYPE.NONE && this.no_target_from_bomb)
			{
				return RAYHIT.NONE;
			}
			if ((Ray.hittype & (HITTYPE.AUTO_TARGET | HITTYPE.TARGET_CHECKER)) == HITTYPE.AUTO_TARGET && this.no_auto_target)
			{
				return RAYHIT.NONE;
			}
			if (Ray.Caster == this.Parent)
			{
				return RAYHIT.NONE;
			}
			if (Ray.Caster is NelEnemyNested && (Ray.Caster as NelEnemyNested).Parent == this.Parent)
			{
				return RAYHIT.NONE;
			}
			return base.can_hit(Ray);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			if (this.applydmg_calc_parent >= 1f)
			{
				return this.Parent.applyHpDamageRatio(Atk);
			}
			return X.NI(base.applyHpDamageRatio(Atk), this.Parent.applyHpDamageRatio(Atk), this.applydmg_calc_parent);
		}

		public override float getHpDamagePublishRatio(MagicItem Mg)
		{
			if (this.publishdmg_calc_parent >= 1f)
			{
				return this.Parent.getHpDamagePublishRatio(Mg);
			}
			return X.NI(base.getHpDamagePublishRatio(Mg), this.Parent.getHpDamagePublishRatio(Mg), this.publishdmg_calc_parent);
		}

		public override float invisible_cosi
		{
			get
			{
				if (this.damage_parry > 0f)
				{
					return this.Parent.invisible_cosi;
				}
				return base.invisible_cosi;
			}
		}

		public NelEnemy Parent;

		public float damage_parry = 1f;

		public float applydmg_calc_parent = 1f;

		public float publishdmg_calc_parent = 1f;

		public bool hp0_remove;

		public bool sync_disappear;

		public bool sync_damagestate;

		public bool sync_aim_from_parent;

		public float base_applydmg_hp_ratio = 1f;

		public float base_applydmg_mp_ratio = 1f;

		public bool no_auto_target;

		public bool no_target_from_bomb;

		private bool tecolor_link_;

		private bool mana_absorb_to_parent;
	}
}
