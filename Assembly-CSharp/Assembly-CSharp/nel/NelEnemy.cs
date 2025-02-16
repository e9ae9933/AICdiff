using System;
using System.Collections.Generic;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class NelEnemy : M2Attackable, M2MagicCaster, NelM2Attacker, IWindApplyable
	{
		public bool disappearing
		{
			get
			{
				return this.disappearing_;
			}
			set
			{
				if (this.disappearing == value)
				{
					return;
				}
				this.disappearing_ = value;
			}
		}

		public virtual void Awake()
		{
			if (this.Anm == null)
			{
				this.Anm = new EnemyAnimator(this, new EnemyAnimator.FnCreate(EnemyFrameDataBasic.Create), null);
			}
		}

		public virtual void appear(Map2d _Mp, NOD.BasicData BasicData)
		{
			this.Mp = _Mp;
			if (BasicData != null)
			{
				this.Set0(BasicData);
			}
			this.base_TS = this.nM2D.NightCon.WindSpeed();
			base.gameObject.layer = LayerMask.NameToLayer("Enemy");
			_Mp.setTag(base.gameObject, "MoverEn");
			if (this.smn_xorsp == -1000f)
			{
				this.smn_xorsp = X.XORSP();
			}
			if (this.maxhp == 0)
			{
				X.de("HPが正しく設定されていません: " + this.id.ToString(), null);
			}
			if (this.maxmp == 0)
			{
				X.de("MPが正しく設定されていません: " + this.id.ToString(), null);
			}
			IN.GetOrAdd<Rigidbody2D>(base.gameObject);
			float base_gravity = base.base_gravity;
			if (this.CC == null)
			{
				this.CC = new M2MvColliderCreatorEn(this, 0.56f, 0.4f);
			}
			base.appear(_Mp);
			if (this.kind == ENEMYKIND.MACHINE)
			{
				this.snd_die = "machine_die";
			}
			this.NCM = new NearCheckerM(this, base.key, (NearManager.NCK)5);
			if (this.CC is M2MvColliderCreatorAtk)
			{
				(this.CC as M2MvColliderCreatorAtk).collider_head_slice = 4;
			}
			this.sizex0 = this.sizex;
			this.sizey0 = this.sizey;
			if (this.Phy != null)
			{
				this.Phy.sharedMaterial = this.getNormalPhysicMaterial();
			}
			if (!this.summoned)
			{
				this.Phy.hit_mover_threshold = 1;
			}
			if (NelEnemy.enemy_layer == 0)
			{
				NelEnemy.enemy_layer = LayerMask.NameToLayer("Enemy");
			}
			this.Phy.default_layer = NelEnemy.enemy_layer;
			this.finePositionFromTransform();
			this.x0 = base.x;
			this.y0 = base.y;
			this.mp = X.MMX(0, X.IntR(((this.first_mp_ratio < 0f) ? 0.3f : this.first_mp_ratio) * (float)this.maxmp), this.maxmp);
			this.Ser = new M2Ser(this, this, false);
			this.Ser.Regist = EnemyData.getSerRegist(this.id.ToString(), null, true);
			M2PxlAnimatorRT m2PxlAnimatorRT = null;
			if (this.anim_chara_name != "")
			{
				m2PxlAnimatorRT = _Mp.M2D.createBasicPxlAnimatorForRenderTicket(this, this.anim_chara_name, "stand", false, M2Mover.DRAW_ORDER.PR0);
			}
			this.Anm.initS(m2PxlAnimatorRT);
			if (m2PxlAnimatorRT != null)
			{
				this.exist_fall_pose = this.Anm.getPoseByName("fall") != null;
				this.exist_land_pose = this.Anm.getPoseByName("land") != null;
				this.exist_stun_pose = this.Anm.getPoseByName("stun") != null;
			}
			if (BasicData != null)
			{
				this.Set1(BasicData);
			}
			this.fineEnlargeScale(1f, false, false);
			this.fineEnlargeScale(-1f, false, false);
			this.setAim(((double)this.NCXORSP(443) < 0.5) ? AIM.L : AIM.R, false);
			this.Lig = new M2Light(_Mp, this);
			this.Lig.follow_speed = 0.1f;
			this.Lig.Col.Set(2864408976U);
			this.Lig.radius = this.sizex * base.CLEN * 3.5f;
			this.Mp.addLight(this.Lig);
			if (this.Nai == null)
			{
				this.Nai = new NAI(this);
			}
			this.flags &= (NelEnemy.FLAG)(-2);
		}

		public override void appear(Map2d _Mp)
		{
			this.appear(_Mp, null);
		}

		public NOD.BasicData Set0(NOD.BasicData B)
		{
			if (B.maxhp > 0)
			{
				this.maxhp = B.maxhp;
			}
			if (B.maxmp > 0)
			{
				this.maxmp = B.maxmp;
			}
			if (B.drop_mp_min >= 0)
			{
				this.drop_mp_min = (float)B.drop_mp_min;
			}
			if (B.drop_mp_max >= 0)
			{
				this.drop_mp_max = (float)B.drop_mp_max;
			}
			if (B.weight != -1000f)
			{
				this.weight0 = ((B.weight < 0f) ? (-1f) : B.weight);
			}
			if (B.sizew_x > 0f)
			{
				this.SizeW(B.sizew_x, (B.sizew_y > 0f) ? B.sizew_y : B.sizew_x, ALIGN.CENTER, ALIGNY.MIDDLE);
			}
			if (B.anim_chara_name != null)
			{
				this.anim_chara_name = B.anim_chara_name;
			}
			if (B.od_damage_ratio > 0f)
			{
				this.overdrive_damage_ratio = B.od_damage_ratio;
			}
			if (B.od_mp_multiple > 0f)
			{
				this.overdrive_mp_multiple = B.od_mp_multiple;
			}
			if (B.od_hp_multiple > 0f)
			{
				this.overdrive_hp_multiple = B.od_hp_multiple;
			}
			if (B.killed_add_exp > 0f)
			{
				this.killed_add_exp = B.killed_add_exp;
			}
			if (B.killed_add_exp_od > 0f)
			{
				this.killed_add_exp_od = B.killed_add_exp_od;
			}
			if (B.mana_desire_multiple >= 0f)
			{
				this.mana_desire_multiple = B.mana_desire_multiple;
			}
			return B;
		}

		public NOD.BasicData Set1(NOD.BasicData B)
		{
			if (B.apply_damage_ratio_max_divide > 0f)
			{
				this.apply_damage_ratio_max_divide = B.apply_damage_ratio_max_divide;
			}
			if (B.enlarge_anim_scale_max > 0f)
			{
				this.enlarge_anim_scale_max = B.enlarge_anim_scale_max;
			}
			if (B.enlarge_publish_damage_ratio > 0f)
			{
				this.enlarge_publish_damage_ratio = B.enlarge_publish_damage_ratio;
			}
			if (B.stun_time > 0)
			{
				this.stun_time = (float)B.stun_time;
			}
			if (B.basic_stun_ratio >= 0f)
			{
				this.basic_stun_ratio = B.basic_stun_ratio;
			}
			if (B.flashbang_time_ratio >= 0f)
			{
				this.flashbang_time_ratio = B.flashbang_time_ratio;
			}
			if (B.ser_resist_key != null)
			{
				this.Ser.Regist = EnemyData.getSerRegist(B.ser_resist_key, this.Ser.Regist, true);
			}
			if (B.drop_ratio_normal100 > 0 && this.NCXORSP(324) * 100f < (float)B.drop_ratio_normal100)
			{
				this.DropItem = B.DropItemNormal;
			}
			if (this.Od != null)
			{
				if (B.enlarge_od_anim_scale_min > 0f)
				{
					this.Od.enlarge_od_anim_scale_min = B.enlarge_od_anim_scale_min;
				}
				if (B.enlarge_od_anim_scale_max > 0f)
				{
					this.Od.enlarge_od_anim_scale_max = B.enlarge_od_anim_scale_max;
				}
				if (B.od_killed_mana_splash >= 0)
				{
					this.Od.od_killed_mana_splash = B.od_killed_mana_splash;
				}
				if (B.drop_ratio_od100 > 0 && this.NCXORSP(578) * 100f < (float)B.drop_ratio_od100)
				{
					this.Od.DropItem = B.DropItemOd;
				}
			}
			return B;
		}

		public virtual void awakeInit()
		{
			this.Nai.AddF(NAI.FLAG.AWAKEN, 180f);
			this.Nai.RemF(NAI.FLAG.RECHECK_PLAYER);
			if (this.Nai.HasF(NAI.FLAG.SUMMON_APPEARED, false))
			{
				this.AimToPlayer();
			}
		}

		private void killAllNestedChildren()
		{
			if (this.ANested != null)
			{
				List<NelEnemyNested> anested = this.ANested;
				this.ANested = null;
				for (int i = anested.Count - 1; i >= 0; i--)
				{
					anested[i].hp0_remove = true;
					anested[i].changeStateToDie();
				}
			}
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			this.killAllNestedChildren();
			this.nM2D.MGC.Notf.RemoveCaster(this);
			if (this.MdHpMpBar != null)
			{
				this.MdHpMpBar.destruct();
				this.MdHpMpBar = null;
			}
			if (this.TeCon != null)
			{
				this.TeCon.clearRegistered();
			}
			if (this.Absorb != null)
			{
				this.Absorb.releaseFromPublish(this);
				this.Absorb = null;
			}
			if (this.Lig != null)
			{
				this.Mp.remLight(this.Lig);
				this.Lig = null;
			}
			if (this.Od != null)
			{
				this.Od.quitOverDrive(true);
				this.Od = null;
			}
			if (this.Nai != null)
			{
				this.Nai.destruct();
				this.Nai = null;
			}
			if (this.Anm != null)
			{
				this.Anm.destruct();
				this.Anm = null;
			}
			base.destruct();
		}

		public override void runPre()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.t < 0f)
			{
				this.t = 0f;
			}
			float ts = this.TS;
			this.Nai.flagRun(ts);
			this.Anm.runPre(ts);
			base.runPre();
			if (!this.isNoticePlayer() && Map2d.can_handle && base.mp_ratio > this.first_mp_ratio + 0.001f)
			{
				int num = (int)((float)this.maxmp * this.first_mp_ratio);
				if (this.mp > num)
				{
					this.mpred_counter += (float)(this.maxmp - num) / (float)this.max_mp_return_to_default_time;
					if (this.mpred_counter > 1f)
					{
						int num2 = (int)this.mpred_counter;
						this.mp = X.Mx(this.mp - num2, num);
						this.mpred_counter -= (float)num2;
						this.flags |= NelEnemy.FLAG.CHECK_ENLARGE;
					}
				}
			}
			if ((this.flags & NelEnemy.FLAG.CHECK_ENLARGE) != NelEnemy.FLAG.NONE && (this.flags & NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING) == NelEnemy.FLAG.NONE && this.canCheckEnlargeState(this.state))
			{
				this.flags &= (NelEnemy.FLAG)(-2);
				if (this.is_evil)
				{
					this.fineEnlargeScale(-1f, true, false);
				}
			}
			if ((this.flags & NelEnemy.FLAG.FINE_HPMP_BAR) != NelEnemy.FLAG.NONE)
			{
				this.prepareHpMpBarMesh();
			}
			bool flag = this.stunCheck();
			float num3 = ts;
			NelEnemy.STATE state = this.state;
			if (state <= NelEnemy.STATE.OD_ACTIVATE)
			{
				if (state != NelEnemy.STATE.STUN)
				{
					if (state != NelEnemy.STATE.ABSORB)
					{
						if (state == NelEnemy.STATE.OD_ACTIVATE)
						{
							if (!this.runOverDriveActivate())
							{
								this.changeStateToNormal();
								goto IL_0351;
							}
							goto IL_0351;
						}
					}
					else
					{
						if (!this.Nai.isPrAlive())
						{
							this.NoDamage.Add(3f);
						}
						if (this.auto_absorb_lock_mover_hitting)
						{
							this.Phy.addLockMoverHitting(HITLOCK.ABSORB, 4f);
						}
						if (this.Ser.has(SER.TIRED))
						{
							num3 = 0f;
							goto IL_0351;
						}
						if (this.Absorb == null || !this.runAbsorb())
						{
							this.changeStateToNormal();
							this.stunCheck();
							goto IL_0351;
						}
						if (this.absorb_pos_fix_maxt >= 0 && this.Absorb != null)
						{
							this.Absorb.walkPublishPos(ts, (float)this.absorb_pos_fix_maxt);
							goto IL_0351;
						}
						goto IL_0351;
					}
				}
				else
				{
					if (!this.runStun())
					{
						this.Ser.Cure(SER.EATEN);
						this.changeStateToNormal();
						goto IL_0351;
					}
					goto IL_0351;
				}
			}
			else if (state <= NelEnemy.STATE.DAMAGE_HUTTOBI)
			{
				if (state != NelEnemy.STATE.DAMAGE)
				{
					if (state == NelEnemy.STATE.DAMAGE_HUTTOBI)
					{
						if (!this.runDamageHuttobi())
						{
							this.changeStateToNormal();
							goto IL_0351;
						}
						goto IL_0351;
					}
				}
				else
				{
					if (!this.is_alive)
					{
						this.changeState(NelEnemy.STATE.DIE);
						goto IL_0351;
					}
					if (!this.runDamageSmall())
					{
						this.changeStateToNormal();
						goto IL_0351;
					}
					goto IL_0351;
				}
			}
			else if (state != NelEnemy.STATE.DIE)
			{
				if (state == NelEnemy.STATE.SUMMONED)
				{
					if (!this.runSummoned())
					{
						this.changeStateToNormal();
						goto IL_0351;
					}
					goto IL_0351;
				}
			}
			else
			{
				if (!this.runDie())
				{
					return;
				}
				goto IL_0351;
			}
			if (!this.isMoveScriptActive(false))
			{
				if (this.Ser.has(SER.TIRED) || !flag)
				{
					num3 = 0f;
				}
				else
				{
					if (this.Nai.HasF(NAI.FLAG.TICKET_REFINED, true))
					{
						this.ticketRefined();
					}
					this.Nai.consider(ts);
				}
			}
			IL_0351:
			if (this.t < 0f)
			{
				this.t = X.Mn(this.t + num3, 0f);
			}
			else
			{
				this.t += ts;
			}
			if (this.Ser != null && this.Anm != null && !base.destructed)
			{
				ulong pre_bits = this.Ser.get_pre_bits();
				this.Ser.run(ts);
				if (!this.Ser.has(SER.TIRED))
				{
					this.Anm.TempStop.Rem("DAMAGE_TIRED");
				}
			}
			if (base.destructed)
			{
				return;
			}
			if (this.Od != null)
			{
				this.Od.runPre(ts);
				if (this.isOverDrive())
				{
					this.Od.volumeActivate(this.Absorb == null);
				}
			}
			if (this.Summoner != null && !base.hasFoot() && !this.Summoner.checkRingOut(this))
			{
				this.changeStateToDie();
				return;
			}
			this.TeCon.checkRepositImmediate();
		}

		protected virtual bool stunCheck()
		{
			if (this.Ser.has(SER.EATEN))
			{
				if (this.showFlashEatenEffect(false))
				{
					this.can_hold_tackle = false;
					return false;
				}
				if (this.state != NelEnemy.STATE.STUN && !this.isDamagingOrKo())
				{
					this.Ser.Fine(SER.EATEN, (int)this.stun_time);
				}
				if (this.state == NelEnemy.STATE.STAND)
				{
					this.changeState(NelEnemy.STATE.STUN);
				}
			}
			return true;
		}

		public override void runPhysics(float fcnt)
		{
			if (this.is_alive && !base.destructed && this.Phy.main_updated_count >= 1 && this.Phy.canConsiderCheckHit())
			{
				for (int i = this.Mp.count_players - 1; i >= 0; i--)
				{
					M2MoverPr pr = this.Mp.getPr(i);
					base.checkHitTo(pr);
				}
			}
			base.runPhysics(fcnt);
		}

		protected virtual void ticketRefined()
		{
		}

		public virtual bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type != NAI.TYPE.WALK)
			{
				if (type == NAI.TYPE.GAZE)
				{
					if (Tk.initProgress(this))
					{
						this.AimToPlayer();
						this.fineHittingLayer();
						if (Tk.after_delay == 0f)
						{
							Tk.after_delay = 20f + this.Nai.RANtk(458) * 20f;
						}
					}
					return (this.is_flying ? (Tk.t >= 90f) : (!base.hasFoot())) || Tk.t >= 120f;
				}
				if (type != NAI.TYPE.WAIT)
				{
					return false;
				}
				this.SpSetPose("stand", -1, null, false);
				this.AimToPlayer();
				Tk.after_delay = 30f + this.Nai.RANtk(840) * 40f;
				return false;
			}
			else
			{
				if (Tk.initProgress(this))
				{
					this.AimToPlayer();
					this.fineHittingLayer();
				}
				if (!this.is_flying)
				{
					return !base.hasFoot();
				}
				return Tk.t >= 40f;
			}
		}

		public virtual void quitTicket(NaTicket Tk)
		{
		}

		public virtual void runAppeal()
		{
			if (this.Anm.isPoseExist("awake"))
			{
				this.SpSetPose("awake", 1, null, false);
			}
		}

		public virtual bool runAbsorb()
		{
			this.absorbEffect();
			AbsorbManager absorb = this.Absorb;
			return false;
		}

		public void absorbEffect()
		{
			this.setDmgBlinkFading(MGATTR.CURE_MP, 27f, 0.46f, 0);
		}

		public virtual NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			this.t = -1f;
			if (this.state == NelEnemy.STATE.STUN)
			{
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
				this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
				this.Nai.AddF(NAI.FLAG.STUN_FINISHED, 180f);
			}
			if (this.state == NelEnemy.STATE.SUMMONED && this.walk_st <= 0)
			{
				this.quitSummonAndAppear(true);
			}
			if (this.state == NelEnemy.STATE.OD_ACTIVATE)
			{
				this.Phy.remLockMoverHitting(HITLOCK.SPECIAL_ATTACK).remLockGravity(HITLOCK.SPECIAL_ATTACK);
			}
			if (this.isAbsorbState(this.state) && !this.isAbsorbState(st))
			{
				this.Phy.remLockMoverHitting(HITLOCK.ABSORB);
				this.Phy.remLockGravity(NelEnemy.STATE.ABSORB);
				this.absorb_pos_fix_maxt = -1;
				this.releaseAbsorb(this.Absorb);
			}
			if (st != NelEnemy.STATE.STAND)
			{
				this.Nai.clearTicket(-1, false);
				this.can_hold_tackle = false;
				this.Phy.remLockMoverHitting(HITLOCK.SPECIAL_ATTACK);
				this.Phy.remLockGravity(HITLOCK.SPECIAL_ATTACK);
				if (!this.disappearing)
				{
					this.throw_ray = st == NelEnemy.STATE.DIE;
				}
			}
			if (this.state == NelEnemy.STATE.DAMAGE_HUTTOBI)
			{
				this.Phy.sharedMaterial = this.getNormalPhysicMaterial();
				this.flags |= NelEnemy.FLAG.CHECK_ENLARGE;
				this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
				this.Phy.addLockMoverHitting(HITLOCK.APPEARING, 1f);
				this.TeCon.removeSpecific(TEKIND.DMG_BLINK);
			}
			if (!this.can_appear_to_front_state(st))
			{
				this.Anm.showToFront(false, false);
			}
			this.state = st;
			if (st <= NelEnemy.STATE.OD_ACTIVATE)
			{
				if (st <= NelEnemy.STATE.STUN)
				{
					if (st != NelEnemy.STATE.STAND)
					{
						if (st == NelEnemy.STATE.STUN)
						{
							this.Phy.addLockMoverHitting(HITLOCK.DAMAGE, -1f);
						}
					}
					else
					{
						this.Nai.ran_a = X.xors();
					}
				}
				else if (st != NelEnemy.STATE.ABSORB)
				{
					if (st == NelEnemy.STATE.OD_ACTIVATE)
					{
						this.walk_time = (float)(this.walk_st = 0);
					}
				}
				else
				{
					this.Phy.addLockMoverHitting(HITLOCK.ABSORB, -1f);
				}
			}
			else if (st <= NelEnemy.STATE.DAMAGE_HUTTOBI)
			{
				if (st != NelEnemy.STATE.DAMAGE)
				{
					if (st == NelEnemy.STATE.DAMAGE_HUTTOBI)
					{
						this.walk_st = 0;
						this.Phy.sharedMaterial = MTRX.PmdM2Huttobi;
						this.Phy.addLockMoverHitting(HITLOCK.DAMAGE, -1f);
					}
				}
			}
			else if (st != NelEnemy.STATE.DIE)
			{
				if (st == NelEnemy.STATE.SUMMONED)
				{
					this.Anm.alpha = 0f;
					this.Phy.addLockMoverHitting(HITLOCK.APPEARING, -1f);
				}
			}
			else
			{
				this.throw_ray = true;
				this.base_TS = 1f;
			}
			return this;
		}

		protected void changeStateToNormal()
		{
			this.AimToPlayer();
			this.changeState(NelEnemy.STATE.STAND);
			this.walk_time = (float)(this.walk_st = 1);
		}

		public override bool check_dangerous_bcc
		{
			get
			{
				return !this.no_apply_map_damage && !this.disappearing && !this.throw_ray;
			}
		}

		public override AttackInfo applyDamageFromMap(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo _Atk, float efx, float efy, bool apply_execute = true)
		{
			if (this.no_apply_map_damage || this.disappearing || this.throw_ray)
			{
				return null;
			}
			NelAttackInfo nelAttackInfo = base.applyDamageFromMap(MDI, _Atk, efx, efy, false) as NelAttackInfo;
			if (nelAttackInfo == null)
			{
				return null;
			}
			if (!apply_execute || this.NoDamage.isActive(nelAttackInfo.ndmg))
			{
				return nelAttackInfo;
			}
			nelAttackInfo.shuffleHpMpDmg(this, 1f, 1f, -1000, -1000);
			if (this.applyDamage(nelAttackInfo, false) <= 0)
			{
				return null;
			}
			return nelAttackInfo;
		}

		public override bool setWaterDunk(int water_id, int misttype)
		{
			if (misttype == 5)
			{
				NelAttackInfo atkMapLavaForEn = MDAT.AtkMapLavaForEn;
				this.applyDamage(atkMapLavaForEn, false);
				base.PtcVar("x", (double)base.x).PtcVar("y", (double)base.y).PtcVarS("attr", FEnum<MGATTR>.ToStr(atkMapLavaForEn.attr))
					.PtcST("hits", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._RELEASE, 0f, this.isOverDrive() ? (-0.04f) : (-0.3f - 0.02f * (1f - base.mp_ratio)), -1f, 0, 10, 0, -1, 0);
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._RELEASE, (float)X.MPFXP() * X.NIXP(0.15f, 0.35f), 0f, -1f, 0, 30, 0, -1, 0);
			}
			return true;
		}

		protected virtual bool runSummoned()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.disappearing = true;
				this.walk_st = 0;
				this.Phy.addLockMoverHitting(HITLOCK.APPEARING, 60f);
				this.Phy.addLockGravity(HITLOCK.APPEARING, 0f, 60f);
				this.Nai.consider(this.TS);
				if (this.Nai.AimPr == null)
				{
					this.Nai.AimPr = this.nM2D.getPrNoel();
				}
				if (this.Od != null && this.Od.pre_overdrive)
				{
					this.initOverDrive(false, false);
				}
			}
			if (this.t >= 60f)
			{
				if (this.walk_st == 0)
				{
					if (this.AimPr != null)
					{
						this.setAim((base.x < this.AimPr.x) ? AIM.R : AIM.L, false);
					}
					this.disappearing = false;
					this.walk_st = 1;
					if (base.hasFoot())
					{
						this.setLandPose();
					}
					else
					{
						this.setFallPose();
					}
					this.quitSummonAndAppear(true);
					if (this.isOverDrive())
					{
						this.initOverDriveAppear();
					}
					this.Nai.AddF(NAI.FLAG.SUMMON_APPEARED, 180f);
				}
				if (base.hasFoot() || this.is_flying)
				{
					return false;
				}
			}
			else
			{
				this.FootD.lockPlayFootStamp(10);
			}
			return true;
		}

		public virtual void initSummoned(EnemySummoner.SmnEnemyKind K, bool is_sudden, int _dupe_count)
		{
			this.is_follower = this.Summoner.is_follower(K, null);
			if (this.is_follower)
			{
				X.dl("follower:ON", null, false, false);
			}
			if (!is_sudden)
			{
				this.changeState(NelEnemy.STATE.SUMMONED);
				base.PtcVar("mp_added", (double)K.mp_added).PtcVar("delay", 60.0).PtcVar("duped", (double)((_dupe_count > 0 || K.DupeConnect != null) ? 1 : 0))
					.PtcST("en_summoned_prepare", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
			}
		}

		public virtual void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			if (this.Absorb != null)
			{
				this.Absorb.releaseFromPublish(this);
			}
			if (clearlock_on_summon)
			{
				this.Phy.remLockMoverHitting(HITLOCK.APPEARING);
				this.Phy.remLockGravity(HITLOCK.APPEARING);
				this.Phy.killSpeedForce(true, true, true, false, false);
			}
			this.Anm.alpha = 1f;
			this.Phy.addLockMoverHitting(HITLOCK.APPEARING, 1f);
			this.enlarge_level = this.getEnlargeLevel();
			this.fineEnlargeScale(-1f, false, false);
			this.fineFootType();
			if (this.Summoner != null)
			{
				EnemySummoner.EnemySummonedInfo summonedInfo = this.Summoner.getSummonedInfo(this);
				if (summonedInfo != null && !summonedInfo.PosInfo.sudden_appear && !this.Summoner.isEnemyEventUsing())
				{
					base.PtcST("en_summoned", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (this.Anm.isPoseExist("awake"))
			{
				this.SpSetPose("awake", -1, null, false);
			}
		}

		public bool event_enemy_flag
		{
			get
			{
				return this.hasF(NelEnemy.FLAG.EVENT_SHOW);
			}
			set
			{
				if (value)
				{
					this.addF(NelEnemy.FLAG.EVENT_SHOW);
					return;
				}
				this.remF(NelEnemy.FLAG.EVENT_SHOW);
			}
		}

		public virtual bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (Abm == null || !(MvTarget is PR) || !Abm.checkPublisher(this))
			{
				return false;
			}
			if (MvTarget as PR != this.AimPr)
			{
				return false;
			}
			this.changeState(NelEnemy.STATE.ABSORB);
			this.absorb_pos_fix_maxt = -1;
			this.Absorb = Abm;
			return true;
		}

		public override void initTortureAbsorbPoseSet(string p, int set_frame = -1, int reset_anmf = -1)
		{
			base.initTortureAbsorbPoseSet(p, set_frame, reset_anmf);
			if (set_frame >= 0)
			{
				this.Anm.animReset(set_frame, false);
			}
			this.absorb_pos_fix_maxt = -1;
			this.flags |= NelEnemy.FLAG.CHECK_ENLARGE;
		}

		public virtual bool releaseAbsorb(AbsorbManager Absorb)
		{
			if (Absorb != null)
			{
				this.Nai.AddF(NAI.FLAG.ABSORB_FINISHED, 40f);
				Absorb.releaseFromPublish(this);
				if (Absorb == this.Absorb)
				{
					this.Absorb = null;
				}
				return true;
			}
			return false;
		}

		public abstract void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr);

		public override bool isDamagingOrKo()
		{
			return this.isDamagingOrKo(this.state);
		}

		public bool isDamagingOrKo(NelEnemy.STATE state)
		{
			return state >= NelEnemy.STATE.DAMAGE && state <= NelEnemy.STATE.DIE;
		}

		public bool isDamagingOrKoOrStun()
		{
			return this.isDamagingOrKoOrStun(this.state);
		}

		public bool isDamagingOrKoOrStun(NelEnemy.STATE state)
		{
			return this.isDamagingOrKo(state) || state == NelEnemy.STATE.STUN;
		}

		public override void moveByHitCheck(M2Phys AnotherPhy, FOCTYPE foctype, float map_dx, float map_dy)
		{
			if (!this.cannot_move)
			{
				base.moveByHitCheck(AnotherPhy, foctype, map_dx / X.Mx(1f, this.enlarge_level), map_dy);
			}
		}

		public override bool cannotHitTo(M2Mover Mv)
		{
			return this.throw_ray_;
		}

		public NelEnemy AimToPlayer()
		{
			if (this.AimPr == null || this.Nai.target_x == 0f)
			{
				return this;
			}
			this.setAim((base.x < this.Nai.target_x) ? AIM.R : AIM.L, false);
			return this;
		}

		public NelEnemy AimToPlayerRev()
		{
			if (this.AimPr == null || this.Nai.target_x == 0f)
			{
				return this;
			}
			this.setAim((base.x > this.Nai.target_x) ? AIM.R : AIM.L, false);
			return this;
		}

		public NelEnemy AimToLr(int _aim)
		{
			if (_aim == -1)
			{
				return this;
			}
			int num = CAim._XD(_aim, 1);
			if (num == 0)
			{
				return this;
			}
			if (num > 0 != CAim._XD(this.aim, 1) > 0)
			{
				this.setAim((num < 0) ? AIM.L : AIM.R, false);
			}
			return this;
		}

		public float getAimLenS()
		{
			if (this.AimPr == null)
			{
				return 1000f;
			}
			return X.LENGTHXYS(base.x, base.y, this.AimPr.x, this.AimPr.y);
		}

		public float getAimLen()
		{
			if (this.AimPr == null)
			{
				return 1000f;
			}
			return X.LENGTHXY(base.x, base.y, this.AimPr.x, this.AimPr.y);
		}

		public override bool isRingOut()
		{
			return this.Mp != null && (base.isRingOut() || base.x < (float)X.Mx(0, this.Mp.crop - 3) || base.x >= (float)X.Mn(this.Mp.clms, this.Mp.clms - this.Mp.crop + 3) || base.mtop < (float)X.Mx(0, this.Mp.crop - 3));
		}

		protected int walkThroughLift(bool init_flag, NaTicket Tk, int afterdelay = 20)
		{
			if (CAim._YD(Tk.aim, 1) >= 0)
			{
				return -1;
			}
			int num = this.skipLift(Tk, 20);
			if (num == -1)
			{
				if (Tk.aim == 3)
				{
					Tk.aim = (int)this.aim;
					return num;
				}
				Tk.aim = (int)CAim.get_aim2(0f, 0f, (float)CAim._XD(Tk.aim, 1), 0f, false);
			}
			return num;
		}

		private int skipLift(NaTicket Tk, int afterdelay = 20)
		{
			if (Tk == null || Tk.prog < PROG.PROG1)
			{
				if (this.FootD == null)
				{
					return -1;
				}
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC == null || !footBCC.is_lift)
				{
					return -1;
				}
				if (!base.canGoToSide(AIM.B, 0.3f, -0.2f, false, true, false))
				{
					return -1;
				}
				if (Tk != null)
				{
					Tk.prog = PROG.PROG0;
					Tk.t += this.TS;
					if (Tk.t < (float)afterdelay)
					{
						return 0;
					}
				}
				base.initJump();
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._RELEASE, 0f, X.NI(0.125f, 0f, this.enlarge_level - 1f), -1f, -1, 1, 0, -1, 0);
				this.Phy.addFocToSmooth(FOCTYPE.WALK, base.x + X.absmin(Tk.depx - base.x, 2f), base.y, 30, 3, 0, -1f);
				if (Tk != null)
				{
					Tk.prog = PROG.PROG1;
				}
				this.skip_lift_mapy = (int)(base.mbottom + 0.125f);
			}
			if (Tk == null)
			{
				return 1;
			}
			if (Tk.prog < PROG.PROG2)
			{
				if (!base.hasFoot())
				{
					bool is_flying = this.is_flying;
				}
				return 1;
			}
			return 1;
		}

		protected void applyAbsorbDamageTo(PR Pr, NelAttackInfo Atk, bool execute_attack = true, bool mouth_damage = false, bool use_applydamage = false, float fade_key_replace_ratio01 = 0f, bool do_not_cure = false, string fade_key_replace = null, bool decline_ui_additional_effect = false)
		{
			if (base.destructed)
			{
				return;
			}
			Atk.AttackFrom = this;
			Atk.CurrentAbsorbedBy = this;
			Atk.Caster = this;
			if (TX.noe(fade_key_replace))
			{
				fade_key_replace = ((this.Absorb != null && X.XORSP() < fade_key_replace_ratio01) ? this.Absorb.Con.uipicture_fade_key : "");
			}
			if (!execute_attack)
			{
				Pr.applyAbsorbDamage(Atk, execute_attack, mouth_damage, fade_key_replace, decline_ui_additional_effect);
				return;
			}
			int split_mpdmg = Atk.split_mpdmg;
			Atk.hpdmg_current = Atk.hpdmg0;
			Atk.mpdmg_current = Atk.mpdmg0;
			if (Atk.hpdmg_current > 0)
			{
				Atk.hpdmg_current = X.IntR((float)Atk._hpdmg * this.getHpDamagePublishRatio(null) * X.NIXP(1f, Atk.damage_randomize_min));
			}
			if (Atk.mpdmg_current > 0)
			{
				Atk.mpdmg_current = X.IntR((float)Atk.mpdmg_current * X.NIXP(1f, Atk.damage_randomize_min));
			}
			Atk.split_mpdmg = X.Mx(1, X.IntR((float)split_mpdmg * X.NIXP(0.9f, 1.15f)));
			Atk.CenterXy(base.x, base.y, 0f);
			this.setAbsorbHitPos(Atk, Pr);
			float hp = Pr.get_hp();
			if (use_applydamage)
			{
				Pr.applyDamage(Atk, true, fade_key_replace, decline_ui_additional_effect, false);
			}
			else
			{
				Pr.applyAbsorbDamage(Atk, execute_attack, mouth_damage, fade_key_replace, decline_ui_additional_effect);
			}
			if (hp > Pr.get_hp())
			{
			}
			Atk.split_mpdmg = split_mpdmg;
		}

		public virtual void setAbsorbHitPos(NelAttackInfo Atk, PR Pr)
		{
			Atk.HitXy(X.NI(this.drawx * this.Mp.rCLEN, Pr.drawx * this.Mp.rCLEN, X.NIXP(0.6f, 0.88f)) + X.XORSPS() * 0.13f, X.NI(this.drawy * this.Mp.rCLEN, Pr.drawy * this.Mp.rCLEN, X.NIXP(0.6f, 0.88f)) + X.XORSPS() * 0.13f, true);
		}

		public virtual bool runStun()
		{
			if (this.showFlashEatenEffect(false))
			{
				this.Ser.Cure(SER.EATEN);
				return false;
			}
			if (this.t <= 0f)
			{
				this.Phy.walk_xspeed = 0f;
				base.PtcVar("sizex", (double)(this.sizex * base.CLENM)).PtcVar("sizey", (double)(this.sizey * base.CLENM)).PtcVar("maxt", (double)X.Mx(900f, this.Ser.getRestTime(SER.EATEN)))
					.PtcST("en_stunned", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				if (this.exist_stun_pose)
				{
					this.Anm.setPose("stun", -1);
				}
				this.t = 0f;
			}
			if (!this.exist_stun_pose || !this.Anm.poseIs("stun", false))
			{
				bool flag = true;
				if (this.Anm.poseIs("damage", false))
				{
					float num = (float)this.Anm.getCurrentSequence().countFrames();
					flag = this.Anm.cframe >= X.IntC(num * 0.7f) || this.Anm.looped_already;
				}
				if (flag)
				{
					this.Anm.setPose("damage", -1);
				}
			}
			if (this.auto_rot_on_damage)
			{
				this.Anm.rotationR = 0.12566371f * X.COSI((float)(this.index * 33) + this.t, 147f);
			}
			if (!this.Ser.has(SER.EATEN))
			{
				if (this.auto_rot_on_damage)
				{
					this.Anm.rotationR = 0f;
				}
				return false;
			}
			return true;
		}

		public bool applySerDamage(AttackInfo Atk, float apply_ratio = 1f, int maxt = -1)
		{
			bool flag = false;
			bool flag2 = this.Ser.has(SER.EATEN);
			if (Atk.SerDmg != null)
			{
				flag = this.Ser.applySerDamage(Atk.SerDmg, this.nM2D.NightCon.applySerRatio(true) * apply_ratio, maxt) || flag;
			}
			if (this.state == NelEnemy.STATE.STAND && !this.showFlashEatenEffect(false) && !flag2 && this.Ser.has(SER.EATEN))
			{
				this.remF(NelEnemy.FLAG.HAS_SUPERARMOR);
				this.Ser.Cure(SER.BURST_TIRED);
			}
			return flag;
		}

		public void applyFlashBangEatenSer(int maxt)
		{
			if (this.flashbang_time_ratio > 0f)
			{
				this.Ser.Add(SER.EATEN, X.IntC((float)maxt * this.flashbang_time_ratio), 99, false);
			}
		}

		protected override void setDamageCounter(int delta_hp, int delta_mp, M2DmgCounterItem.DC ef = M2DmgCounterItem.DC.NORMAL, M2Attackable AbsorbedBy = null)
		{
			if (this.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD))
			{
				ef |= M2DmgCounterItem.DC.DULL;
			}
			if (delta_hp <= 0 && delta_mp <= 0)
			{
				if (this.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD))
				{
					ef |= (M2DmgCounterItem.DC)6;
				}
				if (delta_hp == 0 && delta_mp == 0)
				{
					ef |= M2DmgCounterItem.DC.DULL;
				}
				if (this.hasF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL))
				{
					ef |= M2DmgCounterItem.DC.CRITICAL;
				}
			}
			base.setDamageCounter(delta_hp, delta_mp, ef, AbsorbedBy);
		}

		public virtual int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			if (base.destructed || !this.is_alive)
			{
				if (this.state != NelEnemy.STATE.DIE)
				{
					this.changeStateToDie();
				}
				if (this.state == NelEnemy.STATE.DIE)
				{
					return 0;
				}
			}
			if (!this.hasF(NelEnemy.FLAG.DMG_EFFECT_NOSET))
			{
				Atk.playEffect(this);
			}
			int num = 0;
			bool flag = Atk.ndmg != NDMG.DEFAULT && Atk.ndmg > NDMG.NORMAL;
			bool flag2 = true;
			bool flag3 = this.NoDamage.isActive(Atk.ndmg);
			float num2;
			if (!this.isNoDamageState())
			{
				num2 = this.applyHpDamageRatio(Atk);
				if (flag3)
				{
					num2 = 0f;
				}
				else if (Atk.Caster is NelEnemy)
				{
					NelEnemy nelEnemy = Atk.Caster as NelEnemy;
					num2 /= (1f + nelEnemy.experience) * ((nelEnemy == this) ? 1f : (1f + this.experience));
				}
			}
			else
			{
				num2 = 0f;
			}
			num = X.IntR((float)Atk._hpdmg * ((Atk.fix_damage && !flag3) ? 1f : num2));
			this.Ser.checkDamageSpecial(ref num, Atk);
			bool flag4 = false;
			if (Atk.Caster is M2MoverPr)
			{
				flag4 = true;
				if (!this.Nai.isNoticePlayer())
				{
					this.Nai.awakeInit(Atk.Caster as M2MoverPr);
				}
				if (X.DEBUGMIGHTY)
				{
					num = this.maxhp * 64;
				}
			}
			int mpdmg = Atk._mpdmg;
			int num3 = mpdmg;
			if (num == 0 && mpdmg == 0 && Atk.split_mpdmg == 0)
			{
				if (!flag3 && M2NoDamageManager.isMapDamageKey(Atk.ndmg))
				{
					this.NoDamage.Add(Atk.ndmg, 15f);
				}
				if (!flag3 && (Atk.ndmg == NDMG.DEFAULT || Atk.hpdmg_current == 0))
				{
					this.setDamageCounter(0, 0, M2DmgCounterItem.DC.NORMAL, null);
				}
				return 0;
			}
			float num4 = Atk.burst_vx;
			float num5 = Atk.burst_vy;
			int num6 = num;
			bool flag5 = this.isOverDrive() || this.enlarge_level >= 2f;
			bool flag6 = this.isAttacking();
			num = this.applyHpDamage(num, ref num3, true, Atk);
			this.applySerDamage(Atk, 1f, -1);
			float num7 = 1f;
			float num8 = 30f;
			if (force || (num > 0 && !this.hasSuperArmor(Atk)) || !this.is_alive)
			{
				if (Atk.PublishMagic != null && Atk.PublishMagic.padvib_enable && Atk.Caster is M2MoverPr)
				{
					Atk.PublishMagic.padvib_enable = false;
					(Atk.Caster as M2MoverPr).PadVib("pr_magic_hit", base.getQuakeLevel(1f, 0.25f));
				}
				if (num5 == 0f && !flag5)
				{
					num5 -= 0.1f + 0.16f * X.XORSP();
				}
				if (num4 != 0f)
				{
					num4 += (float)X.MPF(num4 > 0f) * (0.04f + 0.18f * X.XORSP());
				}
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
				this.TeCon.clear();
				if (flag)
				{
					this.NoDamage.Add(Atk.ndmg, (float)Atk.nodamage_time);
				}
				float num9 = (base.hasFoot() ? 1f : 0.2f);
				if (!this.is_alive)
				{
					this.DeathAtk = Atk;
					this.changeStateToDie();
				}
				else if (!this.cannot_move && base.weight >= 0f && X.XORSP() < Atk.huttobi_ratio - base.weight)
				{
					this.changeState(NelEnemy.STATE.DAMAGE_HUTTOBI);
					if (num4 == 0f)
					{
						if (this.AimPr != null)
						{
							num4 = this.speedratio_enlarging * ((float)X.MPF(this.AimPr.x < base.x) * 0.14f);
						}
					}
					else
					{
						num4 = this.speedratio_enlarging * X.absMx(num4, 0.14f);
					}
					if (flag5)
					{
						float num10 = (this.isOverDrive() ? 0.25f : 0.06f);
						if (X.Abs(num5) < num10)
						{
							num5 = 0f;
						}
						else
						{
							num5 = this.speedratio_enlarging * (num5 - num10);
						}
					}
					else
					{
						num5 = this.speedratio_enlarging * X.absMx((num5 == 0f) ? (-0.001f) : num5, 0.02f);
					}
					PostEffect.IT.addTimeFixedEffect(this.TeCon.setQuake(6f, 18, 4f, 0), 1f);
					if (flag)
					{
						this.NoDamage.Add(4f);
					}
					this.SpSetPose("damage", 1, null, false);
				}
				else
				{
					this.changeState(NelEnemy.STATE.DAMAGE);
					this.t = -1f;
					this.SpSetPose("damage", 1, null, false);
					if (flag)
					{
						this.NoDamage.Add(1f);
					}
				}
				if (num5 < 0f)
				{
					this.FootD.initJump(false, false, false);
				}
				num3 += X.Mn(this.mp, this.getMpDamageValue(Atk, Atk.split_mpdmg));
				this.Phy.remFoc(FOCTYPE.WALK | FOCTYPE.JUMP, true);
				num5 *= ((num5 < 0f) ? num9 : 1f);
				if (!this.cannot_move)
				{
					this.Phy.remFoc(FOCTYPE.JUMP, true);
					this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, num4, num5, -1f, -1, 1, 0, -1, 0);
				}
				if (this.checkDamageStun(Atk, (float)(flag6 ? 2 : 1)))
				{
					this.Ser.Add(SER.EATEN, (int)this.stun_time, 99, false);
				}
				this.Nai.delay = 0f;
			}
			else
			{
				num8 = 5f;
				num7 = 0.25f;
				if (num > 0)
				{
					if (flag)
					{
						this.NoDamage.Add(1f);
					}
					num3 += X.Mn(this.mp, this.getMpDamageValue(Atk, Atk.split_mpdmg));
					num7 = 0.6f;
					num8 = 20f;
				}
				if (num2 > 0f)
				{
					if (Atk.ndmg != NDMG.NORMAL)
					{
						this.NoDamage.Add(Atk.ndmg, (float)Atk.nodamage_time);
						if (num <= 0)
						{
							flag2 = false;
						}
					}
					if (!this.hasF(NelEnemy.FLAG.DMG_EFFECT_NOSET))
					{
						num7 = 1f;
						num8 = X.Mx((float)((num > 0) ? 40 : 12), num8);
					}
					else
					{
						num7 = 0.25f;
					}
					int num11 = X.IntC(X.Mx((float)this.knockback_time_superarmor, Atk.tired_time_to_super_armor));
					this.checkTiredTime(ref num11, Atk);
					if (num11 > 0 && this.canApplyTiredInSuperArmor(Atk))
					{
						if (flag4)
						{
							this.Ser.Add(SER.TIRED, num11, 99, false);
							this.Anm.TempStop.Add("DAMAGE_TIRED");
						}
						num8 = X.Mx((float)num11, num8);
						this.TeCon.setQuake(6f, num11, 1.5f, 0);
					}
					else
					{
						this.TeCon.setQuake(6f, 20, 1.5f, 0);
					}
				}
				else
				{
					flag2 = false;
				}
			}
			if (num8 != 0f)
			{
				this.setDmgBlink(Atk.attr, num8, num7, 0);
			}
			if (flag2)
			{
				this.setDamageCounter(this.is_alive ? (-num) : (-num6), 0, M2DmgCounterItem.DC.NORMAL, null);
			}
			if (num3 > 0 && this.is_evil)
			{
				num3 = X.Mn(this.mp, X.IntR((float)num3 * this.nM2D.NightCon.SpilitMpRatioEn()));
				if (num3 > 0)
				{
					num3 = this.applyMpDamage(num3, true, null);
					if (num3 > 0)
					{
						this.setDamageCounter(0, -num3, M2DmgCounterItem.DC.NORMAL, null);
						this.nM2D.Mana.AddMulti(base.x, base.y - 0.5f, X.Mx(0f, (float)num3 - (float)mpdmg * this.mp_split_reduce_ratio), (MANA_HIT)13);
					}
				}
			}
			if (this.is_alive)
			{
				if (Atk.Caster != null && Atk.Caster is M2MoverPr)
				{
					this.Nai.AimPr = Atk.Caster as M2MoverPr;
				}
				this.flags |= (NelEnemy.FLAG)768;
				if (this.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD) && flag2)
				{
					this.Anm.openShield(Atk, 12f);
				}
			}
			return num;
		}

		public virtual void checkTiredTime(ref int t0, NelAttackInfo Atk)
		{
			if (this.isAbsorbState())
			{
				t0 = X.Mn(t0, 60);
			}
		}

		public void applySlipDamage(int hpdmg, int mpdmg, bool no_kill = true, MGATTR attr = MGATTR.NORMAL)
		{
			if (!this.is_alive)
			{
				return;
			}
			if (no_kill)
			{
				hpdmg = X.Mn(this.hp - 1, hpdmg);
			}
			bool flag = false;
			if (hpdmg > 0)
			{
				NelAttackInfo atkSlipDmgForEn = MDAT.AtkSlipDmgForEn;
				atkSlipDmgForEn.hpdmg0 = hpdmg;
				atkSlipDmgForEn.attr = attr;
				int num = this.applyHpDamage(hpdmg, ref mpdmg, true, atkSlipDmgForEn);
				flag = true;
				this.setDamageCounter(-num, 0, M2DmgCounterItem.DC.NORMAL, null);
			}
			if (mpdmg > 0 && this.is_evil)
			{
				int num2 = mpdmg;
				mpdmg = X.Mn(this.mp, X.IntR((float)mpdmg * this.nM2D.NightCon.SpilitMpRatioEn()));
				if (mpdmg > 0)
				{
					mpdmg = this.applyMpDamage(mpdmg, true, null);
					flag = true;
					if (mpdmg > 0)
					{
						this.setDamageCounter(0, -mpdmg, M2DmgCounterItem.DC.NORMAL, null);
						this.nM2D.Mana.AddMulti(base.x, base.y - 0.5f, X.Mx(0f, (float)mpdmg - (float)num2 * 0.75f), (MANA_HIT)13);
					}
				}
			}
			if (flag)
			{
				this.setDmgBlink(attr, 5f, 0.25f, 0);
			}
		}

		public virtual TransEffecterItem setDmgBlink(MGATTR attr, float maxt = 0f, float factor = 1f, int _saf = 0)
		{
			if (this.Anm == null)
			{
				return null;
			}
			return this.Anm.setDmgBlink(attr, maxt, factor, _saf);
		}

		public virtual TransEffecterItem setDmgBlinkFading(MGATTR attr, float maxt = 0f, float factor = 1f, int _saf = 0)
		{
			if (this.Anm == null)
			{
				return null;
			}
			return this.Anm.setDmgBlinkFading(attr, maxt, factor, _saf);
		}

		public virtual void setAbsorbBlink(AbsorbManager Absorb)
		{
			if (this.Anm == null)
			{
				return;
			}
			this.Anm.setAbsorbBlink(Absorb);
		}

		public virtual void setAbsorbBlink(float map_pixel_x, float map_pixel_y)
		{
			if (this.Anm == null)
			{
				return;
			}
			this.Anm.setAbsorbBlink(map_pixel_x, map_pixel_y);
		}

		public virtual void RegisterToTeCon(ITeColor TCol, ITeScaler TScl, ITeShift TShift)
		{
			if (this.TeCon == null)
			{
				return;
			}
			if (TCol != null)
			{
				this.TeCon.RegisterCol(TCol, false);
			}
			if (TScl != null)
			{
				this.TeCon.RegisterScl(TScl);
			}
			if (TShift != null)
			{
				this.TeCon.RegisterPos(TShift);
			}
		}

		public virtual int applyHpDamage(int val, ref int mpdmg, bool force, NelAttackInfo Atk)
		{
			this.addF(NelEnemy.FLAG.FINE_HPMP_BAR);
			return this.applyHpDamage(val, force, Atk);
		}

		public bool applyParry(AIM a)
		{
			if (this.isDamagingOrKo())
			{
				return false;
			}
			float num = 0f;
			if (!this.hasSuperArmor(null))
			{
				num = -0.01f;
				this.Nai.delay = 30f;
				this.changeState(NelEnemy.STATE.DAMAGE);
				this.Phy.remFoc(FOCTYPE.WALK | FOCTYPE.JUMP, true);
				this.SpSetPose("damage", -1, null, false);
			}
			if (!this.cannot_move)
			{
				if (this.Phy.fineGravityScale() != 0f)
				{
					base.killSpeedForce(true, true, false);
				}
				else
				{
					num = 0f;
				}
				this.Phy.remFoc(FOCTYPE.JUMP, true);
				this.Phy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, (float)CAim._XD(a, 1) * X.Mn(0.09f, 0.38f / base.weight), num, -1f, 0, 5, 80, 55, 12);
			}
			return true;
		}

		public override int applyMpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			if (val > 0)
			{
				this.flags |= NelEnemy.FLAG.CHECK_ENLARGE;
			}
			return base.applyMpDamage(val, force, Atk);
		}

		public virtual void applyGasDamage(MistAttackInfo Atk)
		{
			if (!this.no_apply_gas_damage)
			{
				this.applySerDamage(Atk, 1f, -1);
			}
		}

		public virtual float mp_split_reduce_ratio
		{
			get
			{
				return 0.4f;
			}
		}

		public virtual float getWindApplyLevel(WindItem Wind)
		{
			if (this.Phy.isin_water || this.cannot_move || base.weight <= 0f || this.Absorb != null)
			{
				return 0f;
			}
			float num = X.Mn(1.5f, 1f / base.weight);
			if (this.isOverDrive())
			{
				return (base.hasFoot() ? 0f : 0.3f) * num;
			}
			return (base.hasFoot() ? 0.66f : 1f) * num;
		}

		public void applyWindFoc(WindItem Wind, float vx, float vy)
		{
			if (this.FootD.FootIsLadder())
			{
				return;
			}
			this.Phy.addFoc(FOCTYPE.KNOCKBACK, vx, 0f, -4f, 0, 8, 0, -1, 0);
			if (vy != 0f && !base.hasFoot())
			{
				this.Phy.addFoc(FOCTYPE.KNOCKBACK, 0f, vy, -4f, 0, 8, 0, -1, 0);
			}
		}

		public virtual bool checkDamageStun(NelAttackInfo Atk, float level = 1f)
		{
			if (this.isOverDrive())
			{
				return false;
			}
			level *= this.basic_stun_ratio;
			return this.enlarge_level <= 1f && level > 0f && X.XORSP() < level * ((Atk.attr == MGATTR.NORMAL) ? 0.7f : 0.2f) * (1f - X.ZSIN(base.mp_ratio - 0.08f, 0.2f));
		}

		public void initRingOut(bool with_checing)
		{
			if (with_checing)
			{
				if (this.Summoner == null || !this.Summoner.checkRingOut(this))
				{
					this.changeStateToDie();
				}
				return;
			}
			this.quitSummonAndAppear(false);
			this.Nai.AddF(NAI.FLAG.SUMMON_APPEARED, 180f);
		}

		public override bool applyPressDamage(IPresserBehaviour Press, int aim, out bool stop_carrier)
		{
			stop_carrier = false;
			if (aim < 0)
			{
				return false;
			}
			NelAttackInfo atkPressDamage = MDAT.getAtkPressDamage(Press, this, (AIM)aim);
			return this.applyDamage(atkPressDamage, true) > 0;
		}

		public bool canGetMana(M2Mana Mana, bool is_focus)
		{
			if (!this.is_evil || !this.is_alive || this.Ser.has(SER.STRONG_HOLD))
			{
				return false;
			}
			if ((Mana.mana_hit & MANA_HIT.EN) == MANA_HIT.NOUSE || (Mana.mana_hit & MANA_HIT.TARGET_PR) != MANA_HIT.NOUSE)
			{
				return false;
			}
			float num = (is_focus ? 0.12f : X.NI(0.2f, 1.25f, X.ZLINE(this.enlarge_level - 0.3f, 0.3f)));
			return X.BTW(base.mleft - num, Mana.x, base.mright + num) && X.BTW(base.mtop - num, Mana.y, base.mbottom + num);
		}

		public virtual void addMpFromMana(M2Mana Mana, float val)
		{
			if (this.Mp == null || this.Anm == null || !this.is_evil || Mana == null)
			{
				return;
			}
			int num = 0;
			bool flag = true;
			bool flag2 = Mana.from_player_damage | Mana.from_enemy_supplier;
			if (this.isOverDrive())
			{
				if (this.Od == null)
				{
					return;
				}
				num = 1;
				this.Od.addMpFromMana(val);
				flag = flag2;
			}
			if (flag)
			{
				float num2 = (flag2 ? DIFF.enemy_twicemana_mp_cure_ratio : 1f);
				num = (int)(val * num2);
				this.addMpFromMana(num);
				if (flag2)
				{
					this.Mp.DmgCntCon.Make(this, 0, num, M2DmgCounterItem.DC.NORMAL, false);
					this.addHpFromMana(Mana, val * DIFF.enemy_twicemana_hp_cure_ratio);
				}
			}
			if (num > 0)
			{
				if (this.Absorb != null)
				{
					M2Attackable targetMover = this.Absorb.getTargetMover();
					if (targetMover != null)
					{
						this.setAbsorbBlink(targetMover.x * this.Mp.CLEN, targetMover.y * this.Mp.CLEN);
					}
					else
					{
						this.setAbsorbBlink(Mana.x * this.Mp.CLEN, Mana.y * this.Mp.CLEN);
					}
				}
				else
				{
					this.setAbsorbBlink(Mana.x * this.Mp.CLEN, Mana.y * this.Mp.CLEN);
				}
				this.setDmgBlinkFading(MGATTR.CURE_MP, 28f, 0.4f * (flag2 ? 1.25f : 1f), 0);
			}
			base.PtcVar("mx", (double)(Mana.x + X.NIXP(-0.45f, 0.45f))).PtcVar("my", (double)(Mana.y + X.NIXP(-0.45f, 0.45f))).PtcST("mana_get_en", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
		}

		public virtual void addMpFromMana(int val)
		{
			this.mp = X.Mn(this.maxmp, this.mp + val);
			this.flags |= NelEnemy.FLAG.CHECK_ENLARGE;
			this.flags |= NelEnemy.FLAG.FINE_HPMP_BAR;
		}

		public virtual void addHpFromMana(M2Mana Mana, float val)
		{
			this.addHpWithAbsorbing((int)val);
		}

		public int splitMyMana(int mpdmg, float neutral_ratio = 0.125f, int releaseable = -1)
		{
			mpdmg = this.applyMpDamage(X.IntR((float)mpdmg * this.nM2D.NightCon.SpilitMpRatioEn()), true, null);
			if (releaseable < 0)
			{
				releaseable = mpdmg;
			}
			else
			{
				releaseable = X.Mn(releaseable, mpdmg);
			}
			float num = (float)releaseable * neutral_ratio;
			if (num > 0f)
			{
				this.nM2D.Mana.AddMulti(base.x, base.y - 0.5f, num, (MANA_HIT)11);
			}
			this.nM2D.Mana.AddMulti(base.x, base.y - 0.5f, (float)releaseable - num, (MANA_HIT)9);
			return mpdmg;
		}

		public bool Useable(NOD.MpConsume _Mcs, float ratio = 1f, float add = 0f)
		{
			return _Mcs == null || (float)this.mp >= X.Mn((float)_Mcs.consume * this.consume_ratio * ratio, (float)this.maxmp * 0.85f) + add;
		}

		public bool Useable(NOD.TackleInfo _Tki, float ratio = 1f)
		{
			return this.Useable(_Tki.Mcs, ratio, 0f);
		}

		public void MpConsume(NOD.MpConsume _Mcs, MagicItem Mg = null, float ratio = 1f, float release_ratio = 1f)
		{
			if (_Mcs == null)
			{
				return;
			}
			ratio *= this.consume_ratio;
			if (Mg != null)
			{
				int num = this.applyMpDamage(X.IntR((float)_Mcs.consume * this.nM2D.NightCon.SpilitMpRatioEn() * ratio), true, null);
				Mg.reduce_mp = (int)X.Mn((float)_Mcs.release * ratio, (float)num);
				Mg.mp_crystalize = 1f;
				Mg.crystalize_neutral_ratio = _Mcs.neutral_ratio;
				return;
			}
			this.splitMyMana(X.IntR((float)_Mcs.consume * ratio), _Mcs.neutral_ratio, X.IntR((float)_Mcs.release * ratio * release_ratio));
		}

		public virtual void addHpWithAbsorbing(int val)
		{
			if (val <= 0)
			{
				return;
			}
			this.Mp.DmgCntCon.Make(this, val, 0, M2DmgCounterItem.DC.NORMAL, false);
			val = X.Mn(this.maxhp - this.hp, val);
			if (val > 0)
			{
				this.hp += val;
			}
			this.flags |= NelEnemy.FLAG.FINE_HPMP_BAR;
			this.setAbsorbBlink(this.Absorb);
			this.setDmgBlinkFading(MGATTR.CURE_HP, 50f, 0.43f, 0);
		}

		public bool canApplySer(SER ser)
		{
			return false;
		}

		public M2Ser getSer()
		{
			return this.Ser;
		}

		public override void cureHp(int val)
		{
			base.cureHp(val);
			this.flags |= NelEnemy.FLAG.FINE_HPMP_BAR;
		}

		public override void cureMp(int val)
		{
			base.cureMp(val);
			this.flags |= NelEnemy.FLAG.FINE_HPMP_BAR;
			this.flags |= NelEnemy.FLAG.CHECK_ENLARGE;
		}

		public virtual bool runDamageHuttobi()
		{
			if (this.walk_st < 2)
			{
				Vector2 vector;
				if (this.t <= 0f)
				{
					vector = this.Phy.calcFocVelocity(FOCTYPE.DAMAGE, true, false);
					this.t = 0f;
					this.walk_time = 0f;
					this.walk_st = 0;
					vector += this.Phy.releasedVelocity;
					this.Phy.killSpeedForce(true, true, false, false, false);
					this.Phy.remFoc(FOCTYPE.WALK | FOCTYPE.DAMAGE, true);
					if (X.LENGTHXY2(0f, 0f, vector.x, vector.y) <= 0.2304f)
					{
						float num = this.Nai.GARfromPr();
						vector.x += 0.48f * X.Cos(num);
						vector.y -= 0.48f * X.Sin(num);
					}
					vector.y += (float)X.MPF(vector.y >= -0.04f) * 0.15f;
					float num2 = this.Mp.GAR(0f, 0f, vector.x, vector.y);
					base.PtcVar("agR", (double)num2).PtcVar("sx", (double)(base.x + 1.2f * X.Cos(num2))).PtcVar("sy", (double)(base.y - 1.2f * X.Sin(num2)))
						.PtcST("en_huttobi_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				else
				{
					vector = this.Phy.calcFocVelocity(FOCTYPE.WALK, true, false);
				}
				float num3 = -1000f;
				bool flag = false;
				bool flag2 = false;
				if (base.wallHitted(AIM.T) || base.wallHitted(AIM.B))
				{
					vector.y *= ((this.walk_st == 0) ? (-0.86f) : (-0.5f));
					if (this.walk_st == 1 && X.Abs(vector.y) < 0.03f)
					{
						flag2 = true;
					}
					flag = true;
				}
				if (base.wallHitted(AIM.L) || base.wallHitted(AIM.R))
				{
					vector.x *= ((this.walk_st == 0) ? (-0.86f) : (-0.5f));
					flag = true;
				}
				if (this.auto_rot_on_damage)
				{
					num3 = (this.Anm.rotationR = this.Mp.GAR(0f, 0f, vector.x, vector.y));
				}
				if (flag)
				{
					if (num3 == -1000f)
					{
						num3 = this.Mp.GAR(0f, 0f, vector.x, vector.y);
					}
					base.PtcVar("agR", (double)num3).PtcVar("hagR", (double)num3).PtcST("en_huttobi_bound", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				FOCTYPE foctype = FOCTYPE.WALK | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL;
				if (this.walk_time <= 0f)
				{
					if (num3 == -1000f)
					{
						num3 = this.Mp.GAR(0f, 0f, vector.x, vector.y);
					}
					base.PtcVar("agR", (double)num3).PtcST("en_huttobi", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.walk_time = 4f;
				}
				else
				{
					this.walk_time -= this.TS;
				}
				if (this.walk_st == 0)
				{
					if (this.t < 20f)
					{
						foctype |= FOCTYPE._GRAVITY_LOCK;
					}
					if ((X.LENGTHXYS(0f, 0f, vector.x, vector.y) < 0.14f && this.t >= 13f) || this.t >= 30f)
					{
						this.walk_st++;
						this.t = 1f;
						this.Phy.sharedMaterial = this.getNormalPhysicMaterial();
					}
					else
					{
						this.NoDamage.Add(5f);
					}
				}
				else
				{
					foctype |= FOCTYPE._GRAVITY_LOCK;
					if (((X.LENGTHXYS(0f, 0f, vector.x, vector.y) < 0.063f || (base.hasFoot() && X.Abs(vector.y) < 0.03f)) && this.t >= 13f) || flag2 || this.t >= 90f)
					{
						this.flags |= NelEnemy.FLAG.CHECK_ENLARGE;
						this.walk_st++;
						this.walk_time = 0f;
						foctype |= FOCTYPE._RELEASE;
						this.Phy.remLockMoverHitting(HITLOCK.DAMAGE);
						this.NoDamage.Clear();
					}
					else
					{
						vector.x = X.VALWALK(vector.x, 0f, 0.003f * this.TS);
						vector.y += M2Phys.getGravityApplyVelocity(this.Mp, this.Phy.base_gravity, 0.5f * this.TS);
					}
				}
				this.Phy.addFoc(foctype, vector.x, vector.y, -3f, 0, 5, 0, -1, 0);
			}
			else if ((base.hasFoot() || this.is_flying) && (X.LENGTHXYS(0f, 0f, base.vx, base.vy) < 0.004f || this.t >= 120f))
			{
				this.walk_time += this.TS;
				if (this.walk_time >= 30f)
				{
					return false;
				}
			}
			else if (this.auto_rot_on_damage && base.hasFoot())
			{
				this.Anm.rotationR_speed = X.VALWALK(this.Anm.rotationR_speed, 0f, 0.005f * this.TS * 6.2831855f);
				if (this.Anm.rotationR_speed == 0f)
				{
					this.Anm.rotationR = X.MULWALKANGLER(this.Anm.rotationR, 0f, 0.06f);
				}
			}
			return true;
		}

		public virtual bool runDamageSmall()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				if (this.auto_rot_on_damage)
				{
					this.Anm.rotationR_speed = (float)X.MPF(base.vx < 0f) * X.Mx(0f, X.NIXP(-0.02f, 0.04f - 0.05f * X.Mx(0f, this.enlarge_level - 1f))) * 6.2831855f;
				}
			}
			if (this.t >= (float)this.knockback_time)
			{
				if (this.auto_rot_on_damage)
				{
					this.Anm.rotationR_speed = 0f;
					this.Anm.rotationR = 0f;
				}
				return false;
			}
			if (this.auto_rot_on_damage && base.hasFoot())
			{
				this.Anm.rotationR_speed = X.VALWALK(this.Anm.rotationR_speed, 0f, 0.03141593f);
				if (this.Anm.rotationR_speed == 0f)
				{
					this.Anm.rotationR = X.MULWALKANGLER(this.Anm.rotationR, 0f, 0.06f);
				}
			}
			return true;
		}

		private void initDeathPreparation()
		{
			if (!this.hasF(NelEnemy.FLAG.INITDEATH_PREPARED) && !base.destructed)
			{
				this.addF(NelEnemy.FLAG.INITDEATH_PREPARED);
				base.killPtc();
				this.Phy.addLockMoverHitting(HITLOCK.DEATH, -1f);
				this.Nai.clearTicket(-1, true);
				this.blurTargettingFromPr();
				UiEnemyDex.addDefeatCount(this.id);
				base.initDeath();
			}
		}

		public override bool initDeath()
		{
			if (this.state != NelEnemy.STATE.DIE)
			{
				this.changeStateToDie();
				return this.state == NelEnemy.STATE.DIE;
			}
			return true;
		}

		protected virtual bool initDeathEffect()
		{
			if (base.destructed)
			{
				return false;
			}
			this.hp = 0;
			if (!this.hasF(NelEnemy.FLAG.INITDEATH_PREPARED))
			{
				this.initDeathPreparation();
			}
			base.initDeath();
			if (!this.hasF(NelEnemy.FLAG.INITDEATH_PARTICLE_SETTED))
			{
				NelAttackInfo deathAtk = this.DeathAtk;
				this.addF(NelEnemy.FLAG.INITDEATH_PARTICLE_SETTED);
				EnemyData.killEnemy(this, deathAtk);
				if (this.kind == ENEMYKIND.MACHINE)
				{
					if (!this.disappearing)
					{
						base.PtcVar("size", (double)((this.sizex + this.sizey) * 0.5f)).PtcST("machine_die", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					if (deathAtk != null && deathAtk.Caster != null)
					{
						deathAtk.Caster.initPublishKill(this);
					}
				}
				else if (deathAtk != null)
				{
					if (!this.disappearing)
					{
						int num = X.IntR(X.Mn(this.sizex + this.sizey, 2f) * 18f);
						float num2 = (this.sizex + this.sizey) / 2f;
						if (this.is_evil)
						{
							M2DropObject.FnDropObjectDraw fn = M2DropObjectReader.GetFn("EnemyBlood");
							for (int i = 0; i < num; i++)
							{
								M2DropObject m2DropObject = this.Mp.setDrop(fn, base.x + X.NIXP(-this.sizex, this.sizex) * 0.3f, base.y + X.NIXP(-this.sizey * 1.5f, 0f) * 0.7f, deathAtk.burst_vx * X.NIXP(0.003f, 0.22f) + (float)X.MPF(deathAtk.burst_vx > 0f) * X.NIXP(0.01f, 0.11f), X.NIXP(-0.12f, -0.41f), -num2 * X.NIXP(0.15f, 0.55f), (float)((int)X.NIXP(40f, 55f)));
								m2DropObject.type |= DROP_TYPE.GROUND_STOP_X;
								m2DropObject.gravity_scale = 0.33f;
								m2DropObject.BounceReduce(0f, 0f);
							}
						}
						base.PtcVar("x", (double)base.x).PtcVar("y", (double)base.y).PtcVar("hit_x", (double)deathAtk.hit_x)
							.PtcVar("hit_y", (double)deathAtk.hit_y)
							.PtcVar("agR", (double)this.Mp.GAR(0f, deathAtk.hit_y, (base.x - deathAtk.hit_x) * 3f, base.y))
							.PtcST("enemy_die", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					if (deathAtk.Caster != null)
					{
						deathAtk.Caster.initPublishKill(this);
					}
				}
				else if (!this.disappearing)
				{
					base.PtcVar("x", (double)base.x).PtcVar("y", (double)base.y).PtcVar("hit_x", (double)base.x)
						.PtcVar("hit_y", (double)base.y)
						.PtcVar("agR", (double)(X.XORSPS() * 3.1415927f))
						.PtcST("enemy_die", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				if (TX.valid(this.snd_die))
				{
					this.Mp.playSnd(this.snd_die, "_", base.x, base.y, 1);
				}
			}
			return false;
		}

		public virtual void changeStateToDie()
		{
			this.hp = 0;
			this.changeState(NelEnemy.STATE.DIE);
			if (this.Nai != null)
			{
				this.Nai.AimPr = null;
			}
		}

		public virtual bool runDie()
		{
			bool flag = true;
			if (this.DeathAtk != null && this.DeathAtk.Caster is NelEnemy && this.DeathAtk.Caster as NelEnemy != this)
			{
				flag = false;
			}
			if (flag)
			{
				if (this.isOverDrive())
				{
					if (this.Od.DropItem != null)
					{
						NelItemManager.NelItemDrop nelItemDrop = this.nM2D.IMNG.dropManual(this.Od.DropItem, 1, this.getItemDropGrade(), base.x, base.y, 0f, 0f, null, true, NelItemManager.TYPE.NORMAL);
						this.Od.DropItem = null;
					}
				}
				else if (this.DropItem != null)
				{
					NelItemManager.NelItemDrop nelItemDrop = this.nM2D.IMNG.dropManual(this.DropItem, 1, this.getItemDropGrade(), base.x, base.y, 0f, 0f, null, true, NelItemManager.TYPE.NORMAL);
					this.DropItem = null;
				}
			}
			if (!this.hasF(NelEnemy.FLAG.INITDEATH_PARTICLE_SETTED))
			{
				this.initDeathEffect();
			}
			if (this.Summoner != null && this.Anm != null)
			{
				this.Summoner.killedEnemy(this);
			}
			this.killAllNestedChildren();
			this.Mp.removeMover(this);
			return false;
		}

		public virtual int getItemDropGrade()
		{
			float dangerLevel = this.nM2D.NightCon.getDangerLevel();
			int num = (int)(dangerLevel * 0.5f);
			return (int)((float)num + this.NCXORSP(146) * X.Mx(0f, (this.isOverDrive() ? 1f : (this.enlarge_level - 1f)) + dangerLevel - (float)num));
		}

		public void FixSizeW(float px_w, float px_h)
		{
			this.enlarge_level_to_anim_scale(this.enlarge_level);
			this.sizex0 = px_w * 0.5f / base.CLENM;
			this.sizey0 = px_h * 0.5f / base.CLENM;
			this.fineEnlargeScale(-1f, false, false);
		}

		public virtual void fineEnlargeScale(float r = -1f, bool set_effect = false, bool resize_moveby = false)
		{
			if (r < 0f)
			{
				r = this.getEnlargeLevel();
			}
			float num = this.enlarge_level;
			ALIGNY aligny = (base.hasFoot() ? ALIGNY.BOTTOM : ALIGNY.MIDDLE);
			this.enlarge_level = r;
			float num2 = this.enlarge_level_to_anim_scale(r);
			X.IntC(base.mbottom - this.sizey * 2f);
			float num3;
			float num4;
			if (!this.isOverDrive())
			{
				num3 = this.sizex0;
				num4 = this.sizey0;
				this.Anm.extend_eye_wh = 2f;
			}
			else
			{
				num3 = (float)this.Od.od_size_pixel_x * this.Mp.rCLENB;
				num4 = (float)this.Od.od_size_pixel_y * this.Mp.rCLENB;
				this.Anm.extend_eye_wh = X.NI(2, 7, base.mp_ratio);
			}
			base.Size(num3 * base.CLENM * num2, num4 * base.CLENM * num2, ALIGN.CENTER, aligny, resize_moveby);
			base.weight = ((this.weight0 < 0f) ? this.weight0 : (base.weight * this.enlarge_level));
			if (num != this.enlarge_level)
			{
				if (set_effect && this.set_enlarge_bouncy_effect)
				{
					if (this.do_not_scale_by_mp)
					{
						this.TeCon.setEnlargeBouncy(1.08f, 1.08f, 22f, 2);
					}
					else
					{
						this.TeCon.setEnlargeBouncy(1.11f, 1.11f, 22f, 2);
					}
					if (this.enlarge_level > num)
					{
						base.PtcVar("enlarge_time", 30.0).PtcVar("scale", (double)((this.sizex + this.sizey) * this.Mp.CLENB * this.enlarge_level_to_anim_scale(this.enlarge_level))).PtcST("en_enlarge", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					else
					{
						base.PtcVar("enlarge_time", 30.0).PtcVar("scale", (double)((this.sizex + this.sizey) * this.Mp.CLENB * this.enlarge_level_to_anim_scale(num))).PtcST("en_ensmall", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
					}
				}
				this.fineFootType();
			}
			if (this.Absorb != null && this.Absorb.get_Gacha().isUseable())
			{
				this.Absorb.Con.need_fine_gacha_effect = true;
			}
			this.Anm.fineAnimatorOffset(r);
			this.knockback_time = (int)(this.knockback_time0 * X.NI(1f, 0.5f, this.enlarge_level - 1f));
			if (!this.do_not_scale_by_mp)
			{
				this.Phy.base_gravity = this.gravity_scale_enlarged;
			}
			this.Phy.mass = ((this.weight0 < 0f) ? 900f : (this.weight0 * this.enlarge_level));
		}

		public virtual float getEnlargeLevel()
		{
			if (this.isOverDrive())
			{
				return 8f * X.NI(0.875f, 1f, (float)X.IntC(X.ZSINV(base.mp_ratio - 0.125f, 0.875f) * 5f) / 5f);
			}
			float mp_ratio = base.mp_ratio;
			return 1f + (float)X.IntC(X.ZSINV(mp_ratio - 0.125f, this.enlarge_maximize_mp_ratio - 0.125f) * (float)this.enlarge_splice_count) / (float)this.enlarge_splice_count;
		}

		public virtual float enlarge_level_to_anim_scale(float r = -1f)
		{
			if (r < 0f)
			{
				r = this.enlarge_level;
			}
			if (this.do_not_scale_by_mp || (this.torture_absorbing_secure_scale && this.Absorb != null && this.Absorb.isTortureUsing()))
			{
				return 1f;
			}
			if (this.isOverDrive())
			{
				return X.NI(this.Od.enlarge_od_anim_scale_min, this.Od.enlarge_od_anim_scale_max, (r / 8f - 0.875f) / 0.125f);
			}
			if (r <= 1f)
			{
				return r;
			}
			return 1f + (r - 1f) * (this.enlarge_anim_scale_max - 1f);
		}

		public bool initOverDrive(bool change_state = false, bool check_current_state = false)
		{
			if ((check_current_state || change_state) && !this.canCheckEnlargeState(this.state))
			{
				return false;
			}
			if (this.Od == null)
			{
				return false;
			}
			try
			{
				this.Nai.clearTicket(-1, true);
			}
			catch
			{
			}
			this.overdriveStatusFine();
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			this.releaseAbsorb(this.Absorb);
			this.red_eye_blink = true;
			this.FootD.initJump(false, false, false);
			if (change_state)
			{
				this.changeState(NelEnemy.STATE.OD_ACTIVATE);
			}
			else
			{
				this.flags |= (NelEnemy.FLAG)5;
				base.killSpeedForce(true, true, false);
				this.SpSetPose("stand", -1, null, false);
			}
			this.Nai.initOverdrive();
			this.Od.activate(change_state);
			if (this.Summoner != null)
			{
				this.Summoner.enemyOverDriveInit(this);
			}
			if (check_current_state)
			{
				this.initOverDriveAppear();
			}
			return true;
		}

		private void overdriveStatusFine()
		{
			if ((this.flags & NelEnemy.FLAG.OD_HP_MULTIPLE) == NelEnemy.FLAG.NONE)
			{
				this.flags |= NelEnemy.FLAG.OD_HP_MULTIPLE;
				this.maxhp = (int)((float)this.maxhp * this.overdrive_hp_multiple);
				this.hp = (int)((float)this.hp * this.overdrive_hp_multiple);
				this.maxmp = (int)((float)this.maxmp * this.overdrive_mp_multiple);
				this.mp = this.maxmp;
			}
		}

		public virtual void initOverDriveAppear()
		{
			this.changeStateToNormal();
			this.flags |= (NelEnemy.FLAG)5;
			this.overdriveStatusFine();
			this.TeCon.clear();
			this.Od.appear();
			this.Phy.killSpeedForce(true, true, true, false, false).clearLock();
			this.fineEnlargeScale(-1f, false, true);
			this.FootD.initJump(false, false, false);
			this.Nai.clearTicket(-1, true);
			this.exist_fall_pose = this.Anm.getPoseByName("od_fall") != null;
			this.exist_land_pose = this.Anm.getPoseByName("od_land") != null;
			this.Nai.AddF(NAI.FLAG.OVERDRIVED, -1f).AddF(NAI.FLAG.JUMPED, 180f);
		}

		public virtual void quitOverDrive()
		{
			if (this.Od == null)
			{
				return;
			}
			this.flags &= (NelEnemy.FLAG)(-5);
			this.red_eye_blink = false;
			if ((this.flags & NelEnemy.FLAG.OD_HP_MULTIPLE) != NelEnemy.FLAG.NONE)
			{
				this.flags &= (NelEnemy.FLAG)(-9);
				this.maxhp = X.IntC((float)this.maxhp / this.overdrive_hp_multiple);
				this.maxmp = X.IntC((float)this.maxmp / this.overdrive_mp_multiple);
				this.hp = X.MMX(1, X.IntC((float)this.hp / this.overdrive_hp_multiple), this.maxhp);
				this.mp = X.MMX(0, X.IntR((float)this.mp / this.overdrive_mp_multiple), this.maxmp);
			}
			this.exist_fall_pose = this.Anm.getPoseByName("fall") != null;
			this.exist_land_pose = this.Anm.getPoseByName("land") != null;
			this.Od.quitOverDrive(false);
			if (this.Summoner != null)
			{
				this.Summoner.enemyOverDriveQuit(this);
			}
		}

		protected bool runOverDriveAppealBasic(bool init_flag, NaTicket Tk, string pose_0, string pose_1, int wait_1 = 100, int wait_end = 130)
		{
			if (!this.isOverDrive())
			{
				return false;
			}
			if (init_flag)
			{
				this.AimToPlayer();
				this.SpSetPose(pose_0, -1, null, false);
				OverDriveManager.initGrowl(this);
				this.t = 0f;
				this.PadVib("summoner_activate_suddon_0", false, 1f).PadVib("summoner_activate_suddon_1", false, 1f);
				this.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			}
			if (this.t >= (float)wait_1 && Tk.prog == PROG.ACTIVE)
			{
				this.SpSetPose(pose_1, -1, null, false);
				this.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				Tk.prog = PROG.PROG0;
			}
			return this.t < (float)wait_end;
		}

		public bool initDebugOverDrive()
		{
			if (!X.DEBUG || this.state == NelEnemy.STATE.OD_ACTIVATE)
			{
				return false;
			}
			if (this.canCheckEnlargeState(this.state))
			{
				this.initOverDrive(false, false);
			}
			return true;
		}

		public virtual bool runOverDriveActivate()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, -1f).addLockGravity(HITLOCK.SPECIAL_ATTACK, 0f, -1f);
				this.Phy.killSpeedForce(true, true, true, false, false);
				if (!this.cannot_move)
				{
					this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._RELEASE, 0f, -0.093f, -1f, 0, 30, 150, 0, 0).addFoc(FOCTYPE.WALK, base.mpf_is_right * -0.08f, 0f, 0f, 0, 10, 80, 0, 0);
				}
				this.SpSetPose("od_transform", -1, null, false);
			}
			if (!this.Od.runOverDriveActivate(ref this.t, ref this.walk_time, ref this.walk_st))
			{
				this.initOverDriveAppear();
				this.SpSetPose("fall", 1, null, false);
				return false;
			}
			return true;
		}

		public void setEyeColor(C32 C, C32 CBuf, ref float od_blink)
		{
			if (od_blink == -1000f)
			{
				float num = (this.red_eye_blink ? 1f : (X.ZPOW(base.mp_ratio) * 0.5f));
				if (num <= 0f)
				{
					od_blink = 0f;
				}
				else
				{
					od_blink = (0.5f + 0.25f * X.COSI((float)(IN.totalframe + this.index * 27), 7.13f) + 0.25f * X.COSI((float)(IN.totalframe + this.index * 27), 2.18f)) * num;
				}
			}
			if (od_blink > 0f)
			{
				CBuf.Set(this.col_od_blink_color);
				float add_color_white_blend_level = EnemyMeshDrawer.add_color_white_blend_level;
				CBuf.blend(uint.MaxValue, X.ZPOW(add_color_white_blend_level));
				C.blend(CBuf, od_blink);
			}
		}

		public override void jumpInit(float xlen, float ypos, float high_y, bool release_x_velocity = false)
		{
			this.skip_lift_mapy = (int)(this.Nai.target_mbottom + X.Mn(ypos, 0f) + 0.125f) - 1;
			base.jumpInit(xlen, ypos, high_y, release_x_velocity);
		}

		public MagicItem tackleInit(NelAttackInfo Atk, float difx_map = 0f, float dify_map = 0f, float radius = 0.8f, bool no_consider_size = false, bool abs_pos_flag = false)
		{
			MagicItem magicItem = (base.M2D as NelM2DBase).MGC.setMagic(this, MGKIND.TACKLE, this.mg_hit | MGHIT.IMMEDIATE);
			magicItem.sx = difx_map * (no_consider_size ? 1f : this.scaleX) * base.mpf_is_right;
			magicItem.sy = dify_map * (no_consider_size ? 1f : this.scaleY);
			magicItem.sz = (radius + ((this.AimPr != null && !this.AimPr.is_alive) ? 0.9f : 0f)) * (float)X.MPF(!abs_pos_flag);
			magicItem.Atk0 = Atk;
			Atk.knockback_ratio_p = 1f;
			Atk.burst_vx = X.Abs(Atk.burst_vx) * base.mpf_is_right;
			this.can_hold_tackle = true;
			magicItem.run(0f);
			return magicItem;
		}

		public MagicItem tackleInit(NelAttackInfo Atk, NOD.TackleInfo TkData)
		{
			float num = TkData.calc_difx_map(this);
			float num2 = TkData.calc_dify_map(this);
			if (TkData.consider_rotate && this.Anm.rotationR != 0f)
			{
				Vector2 vector = X.ROTV2e(new Vector2(num, num2), this.Anm.rotationR);
				num = vector.x;
				num2 = vector.y;
			}
			MagicItem magicItem = this.tackleInit(Atk, num, num2, X.Abs(TkData.radius), TkData.no_consider_size, TkData.abs_pos_flag);
			magicItem.projectile_power = TkData.projectile_power;
			magicItem.Ray.hittype_to_week_projectile = TkData.reflect;
			magicItem.Ray.check_other_hit = TkData.hit_other;
			magicItem.Ray.shape = TkData.shape;
			if (TkData.no_break_at_hit)
			{
				magicItem.phase = 1;
			}
			if (TkData.hitlock >= -1)
			{
				magicItem.Ray.HitLock((float)TkData.hitlock, null);
			}
			if (!TkData.kill_on_target_hit)
			{
				magicItem.phase = 1;
			}
			this.MpConsume(TkData.Mcs, magicItem, 1f, 1f);
			return magicItem;
		}

		public MagicItem tackleInit(NelAttackInfo Atk, NOD.TackleInfo[] ATkData)
		{
			MagicItem magicItem = null;
			for (int i = ATkData.Length - 1; i >= 0; i--)
			{
				MagicItem magicItem2 = this.tackleInit(Atk, ATkData[i]);
				if (magicItem != null)
				{
					magicItem2.Ray.SyncHitLock(magicItem.Ray);
				}
				magicItem = magicItem2;
			}
			return magicItem;
		}

		public MagicItem groundShockWaveInit(NelAttackInfo Atk, float x_far_abs, float map_high, float spd = 0.34f, bool forward = true)
		{
			MagicItem magicItem = (base.M2D as NelM2DBase).MGC.setMagic(this, MGKIND.GROUND_SHOCKWAVE, this.mg_hit | MGHIT.IMMEDIATE);
			float x = base.x;
			float mbottom = base.mbottom;
			AIM aim = ((base.mpf_is_right > 0f == forward) ? AIM.R : AIM.L);
			M2FootManager footD = this.FootD;
			return MgNGroundShockwave.Init(magicItem, x, x_far_abs, mbottom, aim, map_high, spd, Atk, (footD != null) ? footD.get_FootBCC() : null);
		}

		public MagicItem groundShockWaveInit(NelAttackInfo Atk, NOD.ShockwaveInfo Swi)
		{
			MagicItem magicItem = this.groundShockWaveInit(Atk, Swi.calc_x_far_abs(this), Swi.map_high, Swi.spd, Swi.forward);
			this.MpConsume(Swi.Mcs, magicItem, 1f, 1f);
			return magicItem;
		}

		public MagicItem groundShockWaveInit(NelAttackInfo Atk, NOD.ShockwaveInfo[] ASwiData)
		{
			MagicItem magicItem = null;
			for (int i = ASwiData.Length - 1; i >= 0; i--)
			{
				magicItem = this.groundShockWaveInit(Atk, ASwiData[i]);
			}
			return magicItem;
		}

		public virtual bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			return true;
		}

		public virtual void initPublishKill(M2MagicCaster Target)
		{
			if (Target is NelEnemy && this.is_alive && Target != this)
			{
				NelEnemy nelEnemy = Target as NelEnemy;
				float num = nelEnemy.experience + (nelEnemy.isOverDrive() ? nelEnemy.killed_add_exp_od : nelEnemy.killed_add_exp);
				if (num > 0f)
				{
					if (this.experience == 0f)
					{
						base.PtcST("enemy_get_experience_loop", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
					this.experience += num;
					this.flags |= NelEnemy.FLAG.CHECK_ENLARGE;
					base.PtcST("enemy_get_experience", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
				}
			}
		}

		public float consume_ratio
		{
			get
			{
				return 1f / (1f + this.experience);
			}
		}

		public virtual bool fineFootType()
		{
			if (this.disappearing || this.hasF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT))
			{
				this.FootD.footstamp_type = FOOTSTAMP.NONE;
				return false;
			}
			this.FootD.footstamp_type = (this.isOverDrive() ? FOOTSTAMP.OVERDRIVE : ((this.enlarge_level >= this.foot_bump_effect_enlarge_level) ? FOOTSTAMP.BIG : FOOTSTAMP.BARE));
			return true;
		}

		public void addNestedChild(NelEnemyNested N, int array_create_capacity = 4)
		{
			if (this.ANested == null)
			{
				this.ANested = new List<NelEnemyNested>(4);
			}
			this.ANested.Add(N);
		}

		public void remNestedChild(NelEnemyNested N)
		{
			if (this.ANested != null)
			{
				this.ANested.Remove(N);
			}
		}

		public Vector2 getCenter()
		{
			if (this.Phy == null)
			{
				return new Vector2(base.x, base.y);
			}
			return new Vector2(this.Phy.tstacked_x, this.Phy.tstacked_y);
		}

		public override Vector2 getTargetPos()
		{
			return this.getCenter();
		}

		public virtual Vector2 getAimPos(MagicItem Mg)
		{
			if (this.Nai == null)
			{
				return new Vector2(base.x + (float)(CAim._XD(this.aim, 1) * 4), base.y);
			}
			if (this.AimPr == null)
			{
				Vector2 targetPos = this.getTargetPos();
				targetPos.x += (float)(CAim._XD(this.aim, 1) * 4);
				return targetPos;
			}
			return new Vector2(this.Nai.target_x, this.Nai.target_y);
		}

		public virtual int getAimDirection()
		{
			if (!(this.AimPr == null))
			{
				return -1;
			}
			if (base.x >= this.Nai.target_x)
			{
				return 0;
			}
			return 2;
		}

		public AIM getAimForCaster()
		{
			return this.aim;
		}

		public Vector2 getMoveAimPos()
		{
			return this.getAimPos(null);
		}

		public virtual bool canHoldMagic(MagicItem Mg)
		{
			return false;
		}

		public bool isManipulatingMagic(MagicItem Mg)
		{
			return false;
		}

		public virtual float getHpDamagePublishRatio(MagicItem Mg)
		{
			return (1f + this.experience) * ((Mg == null || Mg.is_normal_attack) ? (this.Ser.AtkRate() * (this.isOverDrive() ? 3f : X.NIL(1f, this.enlarge_publish_damage_ratio, this.enlarge_level - 1f, 1f))) : this.Ser.ChantAtkRate());
		}

		public virtual float getCastingTimeScale(MagicItem Mg)
		{
			return this.base_TS;
		}

		public virtual float getMpDesireRatio(int add_mp)
		{
			if (!this.is_evil || !this.is_alive || this.Ser.has(SER.STRONG_HOLD))
			{
				return -1f;
			}
			if (this.isOverDrive())
			{
				return 0.45f;
			}
			float num = base.mp_ratio + (float)(add_mp / this.maxmp);
			if (num < 1f)
			{
				return num;
			}
			return -1f;
		}

		public virtual float getMpDesireMaxValue()
		{
			if (this.isOverDrive())
			{
				return 12f;
			}
			if (this.Od != null && this.Od.near_overdrive)
			{
				return 0f;
			}
			if (this.Summoner != null && this.Summoner.hasOverDrivedEnemy(1))
			{
				return 2f;
			}
			return this.mana_desire_multiple * ((this.Summoner != null) ? this.Summoner.mana_desire_multiple : 1f);
		}

		public void setAimForCaster(AIM a)
		{
			this.setAim(a, true);
		}

		public virtual M2Mover SizeW(float px_w = -1000f, float px_h = -1000f, ALIGN align = ALIGN.CENTER, ALIGNY aligny = ALIGNY.MIDDLE)
		{
			if (px_w == -1000f)
			{
				px_w = this.sizex0 * 2f * base.CLENM;
			}
			if (px_h == -1000f)
			{
				px_h = this.sizey0 * 2f * base.CLENM;
			}
			return base.Size(px_w / 2f, px_h / 2f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
		}

		public bool hasInnerCollider()
		{
			return this.CC is M2MvColliderCreatorEn && (this.CC as M2MvColliderCreatorEn).hasInnerCollider();
		}

		public override IFootable checkFootObject(float pre_fall_y)
		{
			if (this.Absorb == null || !this.Absorb.publish_float)
			{
				return base.checkFootObject(pre_fall_y);
			}
			return null;
		}

		public override void changeRiding(IFootable _PD, FOOTRES footres)
		{
			base.changeRiding(_PD, footres);
			if (footres == FOOTRES.JUMPED || footres == FOOTRES.JUMPED_IJ || footres == FOOTRES.FOOTED)
			{
				if (this.hasInnerCollider())
				{
					this.fineHittingLayer();
				}
				if (footres == FOOTRES.FOOTED)
				{
					this.skip_lift_mapy = 0;
				}
			}
			if (footres != FOOTRES.FOOTED)
			{
				if (footres - FOOTRES.JUMPED <= 1)
				{
					if (!this.disappearing && (this.isLandFallPoseSetableState(this.state) || this.Anm.isPoseStand()))
					{
						this.setFallPose();
					}
					this.Nai.AddF(NAI.FLAG.JUMPED, 80f);
					return;
				}
			}
			else
			{
				if (!this.disappearing && (this.isLandFallPoseSetableState(this.state) || this.Anm.isPoseStand()))
				{
					this.setLandPose();
				}
				this.Nai.AddF(NAI.FLAG.FOOTED, 80f);
			}
		}

		protected virtual bool setFallPose()
		{
			if (this.exist_fall_pose && !this.hasF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET))
			{
				this.SpSetPose("fall", -1, null, false);
				return true;
			}
			return false;
		}

		protected virtual bool setLandPose()
		{
			if (this.exist_land_pose && !this.hasF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET))
			{
				this.SpSetPose("land", -1, null, false);
				return true;
			}
			return false;
		}

		public virtual bool isTortureUsing()
		{
			return this.Absorb != null && this.Absorb.isTortureUsing();
		}

		public override void fineHittingLayer()
		{
			base.fineHittingLayer();
			if (this.hasInnerCollider())
			{
				(this.CC as M2MvColliderCreatorEn).fineHittingLayerInner();
			}
		}

		public override bool SpPoseIs(string pose)
		{
			return this.Anm != null && this.Anm.poseIs(pose, false);
		}

		public override PxlLayer[] SpGetPointsData(ref M2PxlAnimator MyAnimator, ref ITeScaler Scl, ref float rotation_plusR)
		{
			return this.Anm.SpGetPointsData(ref MyAnimator, ref Scl, ref rotation_plusR);
		}

		public override void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			if (this.Absorb != null && this.Absorb.isTortureUsing() && this.Absorb.target_pose != nPose)
			{
				return;
			}
			if (nPose != null && this.Anm != null)
			{
				PxlPose currentPose = this.Anm.getCurrentPose();
				if (this.Anm.getPoseByName(nPose) == null)
				{
					if (!this.auto_fix_notfound_pose_to_stand)
					{
						return;
					}
					nPose = "stand";
				}
				this.Anm.setPose(nPose, reset_anmf);
				if (currentPose != this.Anm.getCurrentPose())
				{
					this.Anm.fineAnimatorOffset(-1f);
				}
			}
		}

		public override M2Mover setAim(AIM n, bool sprite_force_aim_set = false)
		{
			if (this.aim != n || sprite_force_aim_set)
			{
				this.aim = n;
				if (!base.fix_aim || sprite_force_aim_set)
				{
					this.Anm.setAim(n, 0);
				}
			}
			return this;
		}

		public void blurTargettingFromPr()
		{
			if (this.Mp == null)
			{
				return;
			}
			for (int i = this.Mp.count_players - 1; i >= 0; i--)
			{
				PR pr = this.Mp.getPr(i) as PR;
				if (pr != null)
				{
					pr.blurTargetting(this);
				}
			}
		}

		public override M2Mover setTo(float _x, float _y)
		{
			base.setTo(_x, _y);
			this.blurTargettingFromPr();
			return this;
		}

		public override M2Mover setToDefaultPosition(bool no_set_camera = false, Map2d TargetMap = null)
		{
			Map2d map2d = TargetMap ?? this.Mp;
			if (map2d == null || this.Summoner == null)
			{
				return base.setToDefaultPosition(no_set_camera, TargetMap);
			}
			EnemySummoner.EnemySummonedInfo summonedInfo = this.Summoner.getSummonedInfo(this);
			if (summonedInfo == null)
			{
				return base.setToDefaultPosition(no_set_camera, TargetMap);
			}
			this.setTo(summonedInfo.FirstPos.x, summonedInfo.FirstPos.y);
			if (this.Phy != null && !this.Phy.isLockWallHittingActive())
			{
				M2LpSummon summonedArea = this.Summoner.getSummonedArea();
				int num = 0;
				while (summonedArea != null && base.isStuckInWall(false) && num++ < 4)
				{
					try
					{
						summonedArea.getWalkable(map2d, (float)(summonedArea.mapx + 1) + X.XORSP() * (float)(summonedArea.mapw - 2), (float)(summonedArea.mapy + 1) + X.XORSP() * (float)(summonedArea.maph - 2));
						this.setTo(summonedArea.x, summonedArea.y);
					}
					catch
					{
					}
				}
				summonedInfo.FirstPos = new Vector2(base.x, base.y);
			}
			return this;
		}

		public override bool stuckExtractFailure(Rect RcMap)
		{
			if (EnemySummoner.isActiveBorder())
			{
				M2LpSummon summonedArea = EnemySummoner.ActiveScript.getSummonedArea();
				if (summonedArea != null)
				{
					RcMap = new Rect((float)summonedArea.mapx, (float)summonedArea.mapy, (float)summonedArea.mapw, (float)summonedArea.maph);
				}
			}
			if (!base.stuckExtractFailure(RcMap))
			{
				if (this.Summoner != null)
				{
					EnemySummoner.EnemySummonedInfo summonedInfo = this.Summoner.getSummonedInfo(this);
					if (summonedInfo != null)
					{
						float num = X.VALWALK(base.x, summonedInfo.FirstPos.x, 0.35f);
						float num2 = X.VALWALK(base.y, summonedInfo.FirstPos.y, 0.35f);
						this.moveBy(num - base.x, num2 - base.y, true);
						return true;
					}
				}
				return false;
			}
			return true;
		}

		public static void warpParticleSetting(Map2d Mp, float sx, float sy, float dx, float dy, string ptc_key, float length_divide = 2.25f)
		{
			float num = X.LENGTHXY(sx, sy, dx, dy);
			float num2 = Mp.GAR(sx, sy, dx, dy);
			int num3 = X.IntC(num / length_divide);
			float num4 = 1f / (float)num3;
			float num5 = X.Cos(num2) * num * num4;
			float num6 = X.Sin(num2) * num * num4;
			float num7 = X.NI(10, 30, X.ZLINE((float)(num3 - 2), 12f));
			for (int i = 0; i <= num3; i++)
			{
				Mp.PtcN(ptc_key, sx, sy, 0f, 18 + num3 * 3, (int)(num7 * (float)i * num4));
				sx += num5;
				sy -= num6;
			}
		}

		public virtual void prepareHpMpBarMesh()
		{
			this.prepareHpMpBarMeshInner((this.flags & NelEnemy.FLAG.FINE_HPMP_BAR_CREATE) > NelEnemy.FLAG.NONE);
			this.flags &= (NelEnemy.FLAG)(-769);
		}

		protected void prepareHpMpBarMeshInner(bool create = false)
		{
			if ((EnhancerManager.enhancer_bits & EnhancerManager.EH.hp_eye) == (EnhancerManager.EH)0U || this.Mp == null)
			{
				return;
			}
			if (this.MdHpMpBar == null)
			{
				if (!create)
				{
					return;
				}
				this.MdHpMpBar = new MeshDrawer(null, 4, 6);
				this.MdHpMpBar.draw_gl_only = true;
				this.MdHpMpBar.activate("hpmpbar", MTRX.MtrMeshNormal, false, MTRX.ColWhite, null);
			}
			this.MdHpMpBar.clear(false, false);
			this.MdHpMpBar.Col = C32.d2c(2281701376U);
			this.MdHpMpBar.BoxBL(-43f, -2f, 87f, 4f, 0f, false);
			if (this.hp > 0)
			{
				this.MdHpMpBar.Col = C32.d2c(3739510374U);
				int num = X.IntC(42f * base.hp_ratio);
				this.MdHpMpBar.Line((float)(-(float)num), 0f, 0f, 0f, 2f, false, 0f, 0f);
			}
			if (this.mp > 0)
			{
				this.MdHpMpBar.Col = C32.d2c(3728523232U);
				int num2 = X.IntC(42f * base.mp_ratio);
				this.MdHpMpBar.Line(1f, 0f, (float)(1 + num2), 0f, 2f, false, 0f, 0f);
			}
		}

		public virtual bool createTicketFromEvent(StringHolder rER)
		{
			NAI.TYPE type;
			if (!FEnum<NAI.TYPE>.TryParse(rER._1U, out type, true))
			{
				return false;
			}
			this.Nai.AddTicket(type, rER.Int(2, 0), false).Dep(base.x + rER.Nm(3, 0f), base.y + rER.Nm(3, 0f), null);
			return true;
		}

		public void getMouthPosition(out float _x, out float _y)
		{
			_x = base.x;
			_y = base.y - this.sizey * 0.66f;
		}

		public NelM2DBase nM2D
		{
			get
			{
				return ((this.Mp != null) ? this.Mp.M2D : M2DBase.Instance) as NelM2DBase;
			}
		}

		public bool summoned
		{
			get
			{
				return this.Summoner != null;
			}
		}

		public float speedratio_enlarging
		{
			get
			{
				return X.NI(1f, 0.45f, X.ZPOW(this.enlarge_level - 1f));
			}
		}

		protected PhysicsMaterial2D getNormalPhysicMaterial()
		{
			return MTRX.PmdM2Enemy;
		}

		public override bool readPtcScript(PTCThread rER)
		{
			if (this.Anm == null || this.PtcHld == null)
			{
				return rER.quitReading();
			}
			string cmd = rER.cmd;
			if (cmd != null)
			{
				if (cmd == "%ENLARGE")
				{
					rER.Def("enlarge_level", this.enlarge_level);
					rER.Def("enlarge_scale", this.scaleY);
					return true;
				}
				if (cmd == "%ANGLE")
				{
					rER.Def("agR", this.Anm.rotationR);
					return true;
				}
				if (cmd == "%PVIB" || cmd == "%PVIB2")
				{
					for (int i = 1; i < rER.clength; i++)
					{
						this.PadVib(rER.getIndex(i), rER.cmd == "%PVIB2", 1f);
					}
					return true;
				}
			}
			return base.readPtcScript(rER);
		}

		public override bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			float effectSlowFactor = this.PtcHld.getEffectSlowFactor(Thread, Ef);
			if (effectSlowFactor > 0f)
			{
				PostEffect.IT.addTimeFixedEffect(Ef, effectSlowFactor);
			}
			return true;
		}

		protected override void getPtcCalc(PTCThread rER)
		{
			base.getPtcCalc(rER);
			rER.Def("enlarge_level", this.enlarge_level);
		}

		public float getCastableMp()
		{
			return (float)this.mp;
		}

		public override bool canApplyKnockBack()
		{
			return base.canApplyKnockBack() && !this.cannot_move && this.state != NelEnemy.STATE.DAMAGE_HUTTOBI && !this.isNoDamageState() && this.state != NelEnemy.STATE.ABSORB;
		}

		public float apply_hp_dmg_ratio_enlg
		{
			get
			{
				if (!this.isOverDrive())
				{
					return 1f / X.NI(1f, this.apply_damage_ratio_max_divide, X.ZLINE(this.enlarge_level - 1f));
				}
				return X.NI(1f, this.overdrive_damage_ratio, X.ZPOW(0.7f - base.mp_ratio, 0.7f));
			}
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			if (this.isNoDamageState())
			{
				return 0f;
			}
			float num = 1f;
			float apply_hp_dmg_ratio_enlg = this.apply_hp_dmg_ratio_enlg;
			if (Atk != null && Atk is NelAttackInfo)
			{
				M2Attackable m2Attackable = (Atk as NelAttackInfo).Caster as M2Attackable;
				if (m2Attackable != null)
				{
					NelEnemy nelEnemy = m2Attackable as NelEnemy;
					if (this.Summoner != null && !EnemySummoner.isActiveBorder() && !this.Summoner.getSummonedArea().isContainingMover(m2Attackable, 12f))
					{
						return 0f;
					}
					if (nelEnemy != null)
					{
						if (this.isOverDrive() && nelEnemy.isOverDrive())
						{
							num = 0.25f;
						}
						if (!this.is_follower && nelEnemy.is_follower)
						{
							num *= 0.25f;
						}
						num *= 1f - X.ZLINE(this.experience * 0.66f);
					}
					if (this.isOverDrive() && m2Attackable is PR)
					{
						num *= DIFF.apply_damage_ratio_to_od_from_pr;
					}
				}
			}
			return num * apply_hp_dmg_ratio_enlg;
		}

		public override int ratingHpDamageVal(float ratio)
		{
			return base.ratingHpDamageVal(ratio * this.apply_hp_dmg_ratio_enlg * 0.75f);
		}

		public virtual int getMpDamageValue(NelAttackInfo Atk, int val)
		{
			float num = 1f;
			val = X.Mn(X.IntC(base.get_mp() * 0.25f), val);
			if (this.isOverDrive())
			{
				num = 2.5f;
			}
			else if (this.enlarge_level >= 2f)
			{
				num = 1.5f;
			}
			return X.Mn(X.IntC((float)X.IntC((float)val * num)), val);
		}

		public bool isNoticePlayer()
		{
			return this.Nai.isNoticePlayer();
		}

		public M2MoverPr AimPr
		{
			get
			{
				if (this.Nai == null)
				{
					return null;
				}
				return this.Nai.AimPr;
			}
		}

		public void OnDisabled()
		{
			this.isBusyState();
		}

		public float getPoseAngleRForCaster()
		{
			if (this.Anm == null)
			{
				return 0f;
			}
			return this.Anm.rotationR;
		}

		public bool isBusyState()
		{
			return this.state >= (NelEnemy.STATE)100 || (!base.hasFoot() && !this.is_flying);
		}

		public OverDriveManager getOdManager()
		{
			return this.Od;
		}

		public bool isOverDrive()
		{
			return (this.flags & NelEnemy.FLAG.OD) > NelEnemy.FLAG.NONE;
		}

		public bool isNearOverDriveOnInitialize()
		{
			return this.Od != null && this.Od.near_overdrive;
		}

		public int grade
		{
			get
			{
				return (int)(this.id % (ENEMYID)100U);
			}
		}

		public override IFootable checkSkipLift(M2BlockColliderContainer.BCCLine _P)
		{
			IFootable footable = base.checkSkipLift(_P);
			if (footable != _P)
			{
				return footable;
			}
			if (_P.bottom > (float)this.skip_lift_mapy)
			{
				return _P;
			}
			return null;
		}

		public bool isAbsorbState(NelEnemy.STATE st)
		{
			return st == NelEnemy.STATE.ABSORB;
		}

		protected override bool noHitableAttack()
		{
			return this.disappearing || this.throw_ray || (this.Od != null && !this.isOverDrive() && this.Od.thunder_overdrive) || this.state == NelEnemy.STATE.OD_ACTIVATE || this.state == NelEnemy.STATE.DAMAGE_HUTTOBI;
		}

		protected virtual bool canCheckEnlargeState(NelEnemy.STATE state)
		{
			return !this.isDamagingOrKo() && state != NelEnemy.STATE.SUMMONED && state != NelEnemy.STATE.OD_ACTIVATE;
		}

		public override void runMoveScript()
		{
			if (this.state == NelEnemy.STATE.STAND && !this.MScr.run(false) && (!this.MScr.lock_remove_ms || !EV.isActive(false)))
			{
				this.MScr = null;
			}
		}

		public virtual bool hasSuperArmor(NelAttackInfo Atk)
		{
			return this.hp > 0 && (this.isOverDrive() || this.state == NelEnemy.STATE.STUN || this.hasF(NelEnemy.FLAG.HAS_SUPERARMOR) || this.Ser.has(SER.BURST_TIRED));
		}

		public bool isAbsorbState()
		{
			return this.isAbsorbState(this.state);
		}

		public MGHIT mg_hit
		{
			get
			{
				return MGHIT.EN | ((this.isOverDrive() && this.AimPr != null && this.AimPr.is_alive) ? MGHIT.PR : ((MGHIT)0));
			}
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if (this.hasF(NelEnemy.FLAG.EVENT_SHOW) || this.state == NelEnemy.STATE.SUMMONED)
			{
				return RAYHIT.NONE;
			}
			if (this.ANested != null && Ray.Caster is NelEnemyNested && this.ANested.IndexOf(Ray.Caster as NelEnemyNested) >= 0)
			{
				return RAYHIT.NONE;
			}
			if ((!this.is_alive && !this.overkill) || this.noHitableAttack())
			{
				return (RAYHIT)48;
			}
			if ((Ray.hittype & HITTYPE.GUARD_IGNORE) != HITTYPE.NONE && this.hasF(NelEnemy.FLAG.CUT_RAY_GUARD_IGNORE))
			{
				return RAYHIT.NONE;
			}
			if ((Ray.hittype & HITTYPE.EN) != HITTYPE.NONE && (Ray.hittype & HITTYPE.TEMP_OFFLINE) == HITTYPE.NONE)
			{
				if ((Ray.hittype & HITTYPE.TARGET_CHECKER) != HITTYPE.NONE)
				{
					this.autoTargetRayHitted(Ray);
				}
				return (RAYHIT)3;
			}
			return RAYHIT.NONE;
		}

		public virtual bool canApplyTiredInSuperArmor(NelAttackInfo Atk)
		{
			return !this.Ser.has(SER.BURST_TIRED) && !M2NoDamageManager.isMapDamageKey(Atk.ndmg);
		}

		protected virtual void autoTargetRayHitted(M2Ray Ray)
		{
			this.Nai.autotargetted_me = true;
		}

		public override HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.EN;
		}

		public virtual bool can_appear_to_front_state(NelEnemy.STATE st)
		{
			return this.isAbsorbState(st);
		}

		public float gravity_scale_enlarged
		{
			get
			{
				return base.base_gravity * X.NI(1f, 1.4f, X.ZLINE(this.enlarge_level - 1f));
			}
		}

		public bool isAttacking()
		{
			return this.Nai != null && this.Nai.isAttacking();
		}

		public EnemyAnimator getAnimator()
		{
			return this.Anm;
		}

		public AbsorbManager getAbsorbManager()
		{
			return this.Absorb;
		}

		public bool isNoDamageState()
		{
			return this.state >= NelEnemy.STATE.SUMMONED || this.state == NelEnemy.STATE.DIE || this.state == NelEnemy.STATE.OD_ACTIVATE;
		}

		public bool isStunned()
		{
			return this.state == NelEnemy.STATE.STUN;
		}

		public bool isLandFallPoseSetableState(NelEnemy.STATE state)
		{
			return state < NelEnemy.STATE.STUN || state == NelEnemy.STATE.SUMMONED;
		}

		public float NIENL(float va, float vb)
		{
			return X.NIL(va, vb, this.enlarge_level - 1f, 1f);
		}

		public int getAbsorbWeight()
		{
			return this.absorb_weight;
		}

		public override string snd_key
		{
			get
			{
				return "m2d.enemy." + this.index.ToString();
			}
		}

		public override float getSpShiftX()
		{
			return base.getSpShiftX();
		}

		public override float getSpShiftY()
		{
			return base.getSpShiftY();
		}

		public override float TS
		{
			get
			{
				return base.TS * this.base_TS;
			}
		}

		public bool is_awaken
		{
			get
			{
				return this.Nai.is_awaken;
			}
		}

		public bool is_flying
		{
			get
			{
				return base.base_gravity == 0f || this.floating;
			}
		}

		public NAI getAI()
		{
			return this.Nai;
		}

		public bool hasPT(int _priority, bool remove_if_after_delaying = false, bool only_active = false)
		{
			return this.Nai.hasPriorityTicket(_priority, remove_if_after_delaying, only_active);
		}

		public virtual void fineTargetPos(M2MoverPr Pr, ref float target_x, ref float target_y)
		{
			target_x = Pr.x;
			target_y = Pr.y;
		}

		public bool throw_ray
		{
			get
			{
				return this.throw_ray_;
			}
			set
			{
				if (this.throw_ray_ == value)
				{
					return;
				}
				this.throw_ray_ = value;
				if (value)
				{
					this.blurTargettingFromPr();
					if (this.Phy != null)
					{
						this.Phy.addLockMoverHitting(HITLOCK.THROWRAY, -1f);
						return;
					}
				}
				else if (this.Phy != null)
				{
					this.Phy.remLockMoverHitting(HITLOCK.THROWRAY);
				}
			}
		}

		public override bool pressdamage_applyable
		{
			get
			{
				return base.pressdamage_applyable && !this.throw_ray && !this.disappearing;
			}
		}

		public bool hasF(NelEnemy.FLAG f)
		{
			return (this.flags & f) > NelEnemy.FLAG.NONE;
		}

		public NelEnemy addF(NelEnemy.FLAG f)
		{
			this.flags |= f;
			if ((f & NelEnemy.FLAG.NO_AUTO_LAND_EFFECT) != NelEnemy.FLAG.NONE)
			{
				this.fineFootType();
			}
			return this;
		}

		public NelEnemy remF(NelEnemy.FLAG f)
		{
			this.flags &= ~f;
			if ((f & NelEnemy.FLAG.NO_AUTO_LAND_EFFECT) != NelEnemy.FLAG.NONE)
			{
				this.fineFootType();
			}
			return this;
		}

		public override bool is_alive
		{
			get
			{
				return base.is_alive && !base.destructed;
			}
		}

		public bool is_evil
		{
			get
			{
				return this.kind == ENEMYKIND.DEVIL;
			}
		}

		public void setSkipLift(int y, bool overwriting = false)
		{
			this.skip_lift_mapy = (overwriting ? y : X.Mx(this.skip_lift_mapy, y));
		}

		public float state_time
		{
			get
			{
				return this.t;
			}
		}

		public float scaleX
		{
			get
			{
				if (this.Anm != null)
				{
					return this.Anm.scaleX;
				}
				if (this.sizex0 != 0f)
				{
					return this.sizex / this.sizex0;
				}
				return 1f;
			}
		}

		public float scaleY
		{
			get
			{
				if (this.Anm != null)
				{
					return this.Anm.scaleY;
				}
				if (this.sizex0 != 0f)
				{
					return this.sizey / this.sizey0;
				}
				return 1f;
			}
		}

		public bool battleable_enemy
		{
			get
			{
				return this.battleable_enemy_;
			}
			set
			{
				if (this.battleable_enemy == value)
				{
					return;
				}
				this.battleable_enemy_ = value;
				if (this.Summoner != null)
				{
					this.Summoner.count_battleable_enemy = -1;
				}
			}
		}

		public float enlarge_level_for_animator
		{
			get
			{
				if (!this.isOverDrive())
				{
					return this.enlarge_level;
				}
				return X.NI(2f, 6.3f, base.mp_ratio);
			}
		}

		public float NIel(float a, float b)
		{
			return X.NIL(a, b, this.enlarge_level - 1f, 1f);
		}

		public bool canApplyMistDamage()
		{
			return this.is_alive;
		}

		public virtual bool showFlashEatenEffect(bool for_effect = false)
		{
			return this.isOverDrive();
		}

		public float NCXORSP(int val)
		{
			return X.frac((float)val * this.smn_xorsp);
		}

		public NelEnemy PadVib(string key, bool not_consider_length = false, float level = 1f)
		{
			if (CFG.vib_level == 0)
			{
				return this;
			}
			if (!not_consider_length)
			{
				M2Mover baseMover = base.M2D.Cam.getBaseMover();
				Vector2 vector = ((baseMover != null) ? new Vector2(baseMover.x, baseMover.y) : new Vector2(base.M2D.Cam.x * this.Mp.rCLEN, base.M2D.Cam.y * this.Mp.rCLEN));
				level *= 0.125f + 0.875f * X.Pow(1f - X.ZLINE(X.LENGTHXYN(base.x, base.y, vector.x, vector.y) - 8f, 8f), 2);
			}
			NEL.PadVib(key, level);
			return this;
		}

		public virtual bool canAbsorbContinue()
		{
			return this.is_alive && !this.Ser.has(SER.EATEN);
		}

		public float base_TS = 1f;

		public EnemyAnimator Anm;

		public ENEMYID id;

		protected ENEMYKIND kind;

		public EnemySummoner Summoner;

		public bool is_follower;

		public float smn_xorsp = -1000f;

		public bool exist_fall_pose;

		public bool exist_land_pose;

		public bool exist_stun_pose;

		private bool battleable_enemy_ = true;

		protected float walkspd_awake = 0.11f;

		protected float walkspd_sleep = 0.04f;

		protected float walkspd_od = 0.13f;

		protected float weight0 = 4f;

		public float drop_mp_min = 4f;

		public float drop_mp_max = 20f;

		protected float foot_bump_effect_enlarge_level = 1.75f;

		public float sizex0;

		public float sizey0;

		protected NelEnemy.STATE state;

		protected float x0;

		protected float y0;

		protected float t = -1f;

		protected float walk_time;

		protected int walk_st;

		public float first_mp_ratio = -1f;

		public float knockback_time0 = 55f;

		public float killed_add_exp = 0.5f;

		public float killed_add_exp_od = 1f;

		public float experience;

		public float stun_time = 200f;

		protected M2Light Lig;

		public bool nohit_to_enemy_jumping = true;

		protected bool no_apply_gas_damage;

		protected bool no_apply_map_damage;

		public bool do_not_scale_by_mp;

		public bool auto_rot_on_damage;

		public bool torture_absorbing_secure_scale;

		public float mana_desire_multiple = 10f;

		protected bool auto_absorb_lock_mover_hitting = true;

		public int knockback_time_superarmor = 18;

		public int enlarge_splice_count = 5;

		public const float FIRST_MP_RATIO_DEFAULT = 0.3f;

		public const int HIT_MOVER_THRESHOLD_NORMAL = 1;

		protected string anim_chara_name = "";

		public bool auto_fix_notfound_pose_to_stand = true;

		public int max_mp_return_to_default_time = 3000;

		private float mpred_counter;

		public float sink_ratio = 1f;

		public float enlarge_level = 1f;

		public const int ENLARGE_MAXT = 24;

		public const float overdrive_enlarge_level = 8f;

		public float enlarge_maximize_mp_ratio = 0.94f;

		public const float apply_damage_ratio_max_divide_default = 2.5f;

		public float apply_damage_ratio_max_divide = 2.5f;

		public float overdrive_damage_ratio = 2f;

		public float enlarge_publish_damage_ratio = 2f;

		public float basic_stun_ratio = 1f;

		public const float overdrive_publish_damage_ratio = 3f;

		public float overdrive_hp_multiple = 12f;

		public float overdrive_mp_multiple = 1f;

		public bool cannot_jump;

		public bool set_enlarge_bouncy_effect = true;

		public float flashbang_time_ratio = 1f;

		public float enlarge_anim_scale_max = 2.3f;

		public bool can_hold_tackle;

		public uint col_od_blink_color = 4294901760U;

		protected NAI Nai;

		public bool cannot_move;

		public int absorb_weight = 1;

		protected int absorb_pos_fix_maxt = -1;

		public bool red_eye_blink;

		private static int enemy_layer;

		protected AbsorbManager Absorb;

		protected M2Ser Ser;

		private NelEnemy.FLAG flags;

		public string snd_die = "en_die";

		protected OverDriveManager Od;

		protected int skip_lift_mapy;

		public bool disappearing_;

		private bool throw_ray_;

		public const int SUMMON_DELAY = 60;

		private const int HPBAR_W = 42;

		public MeshDrawer MdHpMpBar;

		public NelAttackInfo DeathAtk;

		public List<NelEnemyNested> ANested;

		private NelItem DropItem;

		public bool do_not_shuffle_on_cheat;

		public delegate bool FnCheckEnemy(NelEnemy N);

		public enum STATE
		{
			STAND,
			SPECIAL_0 = 500,
			STUN = 900,
			ABSORB = 1000,
			OD_ACTIVATE = 2000,
			DAMAGE = 4000,
			DAMAGE_HUTTOBI = 4010,
			DIE = 5000,
			SUMMONED = 10000,
			RINGOUT_RESUME
		}

		public enum FLAG
		{
			NONE,
			CHECK_ENLARGE,
			OD = 4,
			OD_HP_MULTIPLE = 8,
			EVENT_SHOW = 16,
			CAN_TRANSFORM = 32,
			NO_AUTO_LANDFALL_POSE_SET = 64,
			NO_AUTO_LAND_EFFECT = 128,
			FINE_HPMP_BAR = 256,
			FINE_HPMP_BAR_CREATE = 512,
			DMG_EFFECT_NOSET = 65536,
			DMG_EFFECT_CRITICAL = 131072,
			DMG_EFFECT_SHIELD = 262144,
			_DMG_EFFECT_BITS = 983040,
			CUT_RAY_GUARD_IGNORE = 1048576,
			HAS_SUPERARMOR = 2097152,
			DECLINE_ENLARGE_CHECKING = 4194304,
			INITDEATH_PREPARED = 8388608,
			INITDEATH_PARTICLE_SETTED = 16777216
		}
	}
}
