using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using nel.smnp;
using PixelLiner;
using Spine;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNBoss_Nusi : NelEnemyBoss, IAnimListener, IOtherKillListener
	{
		public float hp_level_attacking
		{
			get
			{
				if (this.burst_counter_success < 2)
				{
					return 0.165f;
				}
				return 0.155f;
			}
		}

		public bool faint_stun_lock
		{
			get
			{
				return this.faint_stun_lock_;
			}
			set
			{
				if (this.faint_stun_lock == value)
				{
					return;
				}
				this.faint_stun_lock_ = value;
				if (!value)
				{
					this.faint_stun_lock_ = false;
				}
			}
		}

		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.Freeze = new NelNBoss_Nusi.AIFreeze(this);
			this.kind = ENEMYKIND.DEVIL;
			this.AtkFaintAbsorb1 = new EnAttackInfo(this.AtkFaintAbsorb0);
			this.AtkFaintAbsorb1.attr = MGATTR.STAB;
			ENEMYID id = this.id;
			this.id = ENEMYID.BOSS_NUSI_0;
			NOD.BasicData basicData = NOD.getBasicData("BOSS_NUSI_0");
			this.CCN = new M2MvColliderCreatorEnNest(this, 2);
			this.CCN.setToRect();
			this.CC = this.CCN;
			base.base_gravity = 0f;
			this.foc_center_level_y = 0.45f;
			this.foc_shift_y_ratio = 0f;
			this.foc_base_shift_y = -40f;
			base.appear(_Mp, basicData);
			this.do_not_shuffle_on_cheat = true;
			this.no_apply_map_damage = true;
			base.fix_aim = true;
			this.setAim(AIM.L, true);
			this.FootD.auto_fix_to_foot = false;
			this.snd_die = null;
			this.ringoutable = false;
			this.Anm.checkframe_on_drawing = false;
			this.Phy.auto_air_mvhitting = false;
			this.Anm.showToFront(true, false);
			this.Anm.draw_margin = 5f;
			this.set_enlarge_bouncy_effect = false;
			this.cannot_move = true;
			this.Nai.consider_only_onfoot = false;
			this.Nai.awake_length = 60f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.absorb_weight = 1;
			this.red_eye_blink = true;
			this.Nai.busy_consider_intv = 20f;
			this.FD_MgRunShot = new MagicItem.FnMagicRun(this.MgRunShot);
			this.FD_MgRunDrawShot = new MagicItem.FnMagicRun(this.MgRunDrawShot);
			this.FD_MgRunSpore = new MagicItem.FnMagicRun(this.MgRunSpore);
			this.FD_MgRunDrawSpore = new MagicItem.FnMagicRun(this.MgRunDrawSpore);
			this.PoseShotBody = this.Anm.getCurrentCharacter().getPoseByName("shot_bomb");
			this.SqSpore = this.Anm.getCurrentCharacter().getPoseByName("housi").getSequence(0);
			this.SqSporeSpike = this.Anm.getCurrentCharacter().getPoseByName("housi_spike").getSequence(0);
			base.addF((NelEnemy.FLAG)2097280);
			this.OHitSpore = base.nM2D.MGC.getHitLink(MGKIND.GROUND_SHOCKWAVE);
			this.FD_sporeCount = (MagicItem Mg, M2MagicCaster Caster) => Mg.isActive(Caster, this.FD_MgRunSpore);
			this.AtkShotMist.Prepare(this, true);
			this.AtkSporeSpike.Prepare(this, true);
			this.AtkBigRun.Prepare(this, true);
			if (base.nattr_has_mattr)
			{
				if ((this.nattr & ENATTR.ACME) != ENATTR.NORMAL)
				{
					this.MkBigRun = new MistManager.MistKind(NelNMush.MkAcme);
				}
				else
				{
					this.MkBigRun.AAtk[0].SerDmg = EnemyAttr.createSer(this, 85);
					this.MkBigRun.color0 = C32.d2c(EnemyAttr.get_mcolor(this.nattr, C32.c2d(this.MkBigRun.color0)));
				}
			}
			this.AtkFaintAbsorb0.Prepare(this, true);
			this.AtkFaintAbsorb1.Prepare(this, false);
		}

		public override M2Mover setAim(AIM n, bool sprite_force_aim_set = false)
		{
			if (this.aim != n && sprite_force_aim_set)
			{
				this.aim = n;
				this.Anm.setAim(n, 0);
				if (this.ATentacleL != null)
				{
					this.EA_Base.BaseScale = new Vector3(-base.mpf_is_right, 1f);
					for (int i = 0; i < 3; i++)
					{
						EnemyAnimatorSpine ea = this.ATentacleL[i].EA;
						ea.BaseScale = this.EA_Base.BaseScale;
						Vector2 vector = new Vector2(ea.NestTarget.shiftx, ea.NestTarget.shifty);
						this.initTentacleEAPos(i);
						ea.NestTarget.MapShift(vector.x, vector.y);
					}
				}
			}
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
			this.killFollowers();
			if (this.ATentacleL != null)
			{
				for (int i = 0; i < 3; i++)
				{
					this.ATentacleL[i].EA.destruct();
				}
				this.EA_Base.destruct();
			}
			base.destruct();
		}

		public override void initSummoned(SmnEnemyKind K, bool is_sudden, int _dupe_count)
		{
			base.initSummoned(K, is_sudden, _dupe_count);
			this.FirstPos = new Vector2(base.x, base.y);
			this.danger_lv = base.nM2D.NightCon.getDangerLevel();
			this.time_p1_lock_prob = X.NIL(1f, 0.4f, this.danger_lv - 2f, 2.5f);
			this.time_p2_reduce_base_time = X.NIL(70f, 10f, this.danger_lv - 0.8f, 3.8f);
			this.time_p2_lock_ratio = X.NIL(0.42f, 0.1f, this.danger_lv - 0.6f, 2.4f);
			this.act3_canmove_alloc_ratio = X.NI(0.85f, 0.02f, X.ZPOW(this.danger_lv - 0.6f, 4.2f));
			this.time_p1_delay_reduce_ratio = X.NI(0.89f, 0.3f, X.ZLINE(this.danger_lv - 2.5f, 1.5f));
			this.time_p2_tentacle_afterdelay_ratio = X.NI(1f, 0.4f, X.ZLINE(this.danger_lv - 2f, 3f));
			this.follower_spl_id = K.splitter_id + 1;
			this.aiphase2_lock_max = X.Mx(0, 4 - base.nM2D.NightCon.getDangerMeterVal(false, false) / 10);
		}

		public override bool can_appear_to_front_state(NelEnemy.STATE st)
		{
			return true;
		}

		public override void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			base.quitSummonAndAppear(false);
			this.Phy.addLockWallHitting(this, -1f);
			this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, -1f);
			if (this.Summoner != null)
			{
				float x = this.Summoner.Lp.RevertPosPr.x;
				this.setAim((base.x < x) ? AIM.R : AIM.L, true);
			}
			else if (this.Mp.Pr == null)
			{
				this.setAim(AIM.L, true);
			}
			else
			{
				this.setAim((base.x < this.Mp.Pr.x) ? AIM.R : AIM.L, true);
			}
			if (this.Summoner != null)
			{
				SummonedInfo summonedInfo = this.Summoner.getSummonedInfo(this);
				if (summonedInfo != null)
				{
					this.moveBy(summonedInfo.FirstPos.x - base.x, summonedInfo.FirstPos.y - base.y, true);
					this.FirstPos = summonedInfo.FirstPos;
				}
			}
			if (this.ATentacleL == null)
			{
				this.EnCage = NelEnemyNested.CreateNest(this, "BOSS_NUSI_CAGE", 1f, 6) as NelNNusiCage;
				this.EnCage.NestTeColor(true).NestManaAbsorb(true);
				this.EnCage.no_target_from_bomb = (this.EnCage.no_auto_target = true);
				this.EnCage.fineCagePosition(true);
				this.EA_Base = new EnemyAnimatorSpine(this, "boss_nusi__enemy_boss_nusi", "Base", "body", "enemy_boss_nusi", "main0");
				this.EA_Base.check_clip = false;
				this.EA_Base.showToFront(true, false);
				this.EA_Base.draw_margin = 40f;
				this.EA_Base.addListenerComplete(new AnimationState.TrackEntryDelegate(this.FnBaseAnimComplete));
				this.EA_Base.NestTarget = (this.NestBaseBody = this.CCN.Create("body").Size(2.2f, 1.2f));
				this.NestBaseBody.MapShift(-0.8f, 0f);
				this.EA_Base.base_rotate_shuffle360 = 0.05f;
				this.EA_Base.scale_shuffle01 = 0.002f;
				this.EA_Base.after_offset_x = 30f;
				this.Anm.addAdditionalListener(this.EA_Base);
				this.EA_Base.auto_check_frame_from_listener_attached = false;
				this.ATentacleL = new NelNBoss_Nusi.TentacleLink[3];
				for (int i = 0; i < 3; i++)
				{
					EnemyAnimatorSpine enemyAnimatorSpine = new EnemyAnimatorSpine(this, "boss_nusi__enemy_boss_nusi", "MainT" + i.ToString(), "tentacle", "enemy_boss_nusi", "main" + i.ToString());
					this.ATentacleL[i] = new NelNBoss_Nusi.TentacleLink(enemyAnimatorSpine, i);
					enemyAnimatorSpine.NestTarget = new CCNestItem(null, "tentacle" + i.ToString());
					enemyAnimatorSpine.showToFront(true, false);
					this.setTentacleEA(enemyAnimatorSpine, false);
					this.Anm.addAdditionalListener(enemyAnimatorSpine);
				}
				this.EA_FaintTentacle = new EnemyAnimatorSpine(this, "boss_nusi__enemy_boss_nusi", "FaintT", "tentacle", "enemy_boss_nusi", "main0");
				this.EA_FaintTentacle.alpha = 0f;
				this.EA_FaintTentacle.NestTarget = new CCNestItem(null, "FaintT");
				this.EA_FaintTentacle.addListenerComplete(new AnimationState.TrackEntryDelegate(this.triggerFaintTentacleAnimComplete));
				this.EA_FaintTentacle.fine_intv = 1f;
				this.setTentacleEA(this.EA_FaintTentacle, true);
				this.Anm.addAdditionalListener(this.EA_FaintTentacle);
				this.EA_FaintTentacle.showToFront(true, false);
				this.initTentacleEAPos();
				this.AEnTentacle = new NelNNusiTentacle[5];
				for (int j = 0; j < 5; j++)
				{
					NelNNusiTentacle nelNNusiTentacle = (this.AEnTentacle[j] = NelEnemyNested.CreateNest(this, "BOSS_NUSI_TENTACLE", 1f, 6) as NelNNusiTentacle);
					nelNNusiTentacle.NestTeColor(true).NestManaAbsorb(true);
					nelNNusiTentacle.no_target_from_bomb = (nelNNusiTentacle.no_auto_target = true);
					nelNNusiTentacle.initTentacle(this, j);
				}
				this.fineMaPosition(0f);
			}
		}

		public void setTentacleEA(EnemyAnimatorSpine EA, bool no_shuffle = false)
		{
			EA.draw_margin = 40f;
			EA.base_rotate_shuffle360 = (no_shuffle ? 0f : 0.05f);
			EA.scale_shuffle01 = (no_shuffle ? 0f : 0.002f);
			EA.checkframe_on_drawing = false;
			EA.auto_check_frame_from_listener_attached = false;
		}

		public override void moveBy(float map_dx, float map_dy, bool recheck_foot = true)
		{
			base.moveBy(map_dx, map_dy, recheck_foot);
			if (this.ATentacleL != null)
			{
				this.EnCage.need_fine_position = true;
			}
		}

		public override void positionChanged(float prex, float prey)
		{
			base.positionChanged(prex, prey);
			if (this.ATentacleL != null)
			{
				this.EnCage.need_fine_position = true;
			}
		}

		private NelNBoss_Nusi.MA_FLW ma_flower
		{
			get
			{
				return this.ma_flower_;
			}
			set
			{
				if (this.ma_flower == value)
				{
					return;
				}
				this.ma_flower_ = value;
				this.anmt = 0f;
			}
		}

		public bool updateAnimator(float f)
		{
			this.fineMaPosition(f);
			return true;
		}

		private void fineMaPosition(float fcnt)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = -1000f;
			switch (this.ma_flower)
			{
			case NelNBoss_Nusi.MA_FLW.STATIC:
				num = 30f + 25f * X.COSI(this.anmt, 84f);
				num2 = 20f - 20f * X.COSI(this.anmt, 153f);
				num3 = -X.COSI(this.anmt, 96f) * 0.05f * 3.1415927f;
				break;
			case NelNBoss_Nusi.MA_FLW.LAUGH:
			{
				float num4 = 1.5707964f - X.ZIGZAGI(this.anmt, 48f) * 3.1415927f * 0.18f;
				num = 40f * X.Cos(num4) + 4f * X.COSI(this.anmt, 31f);
				num2 = 20f * X.Sin(num4) + 8f * X.COSI(this.anmt, 27f);
				num3 = 0.25132743f * X.COSI(this.anmt, 61f);
				break;
			}
			case NelNBoss_Nusi.MA_FLW.DAMAGE:
				num = 35f + X.COSI(this.anmt, 14.5f) * 8f + X.COSI(this.anmt, 5.25f) * 4f;
				num2 = 50f + X.COSI(this.anmt, 11.3f) * 8f + X.COSI(this.anmt, 6.31f) * 4f;
				num3 = 3.1415927f * (X.COSI(this.anmt, 11.5f) * 0.04f + X.COSI(this.anmt, 7.22f) * 0.013f);
				break;
			case NelNBoss_Nusi.MA_FLW.STUN:
				num = 35f + 20f * X.ZSINV(this.anmt, 30f);
				num2 = 50f - 78f * X.ZPOW(this.anmt, 28f) + (X.ZPOWV(this.anmt - 28f, 13f) - X.ZPOWV(this.anmt - 28f - 13f, 13f)) * 19f;
				num3 = 3.1415927f * (X.COSI(this.anmt, 49.5f) * 0.03f + X.COSI(this.anmt, 77.52f) * 0.018f);
				break;
			case NelNBoss_Nusi.MA_FLW.DAMAGE_IN_STUN:
				num = 35f + X.COSI(this.anmt, 14.5f) * 8f + X.COSI(this.anmt, 5.25f) * 4f;
				num2 = -28f + X.COSI(this.anmt, 11.3f) * 8f + X.COSI(this.anmt, 6.31f) * 4f;
				num3 = 3.1415927f * (-0.03f * X.SINI(this.anmt, 18f) * 0.06f) * (1f - X.ZPOW(this.anmt, 20f));
				if (this.anmt >= 20f)
				{
					this.ma_flower_ = NelNBoss_Nusi.MA_FLW.STUN;
					this.anmt = X.NIXP(140f, 200f);
				}
				break;
			case NelNBoss_Nusi.MA_FLW.GRAWL:
				num = 35f * X.ZSIN(this.anmt, 38f) + 11f * X.COSI(this.anmt, 24f);
				num2 = -30f + 40f * X.ZSIN(this.anmt, 38f) - 7f * X.COSI(this.anmt, 19f);
				num3 = 3.1415927f * (-0.16f + 0.26f * X.ZSIN(this.anmt, 21f) + X.COSI(this.anmt - 24f, 11.5f) * 0.06f);
				break;
			case NelNBoss_Nusi.MA_FLW.SHOT:
				num = 10f + 25f * X.ZPOW(this.anmt, 34f);
				num2 = -20f + 40f * X.ZPOW(this.anmt, 50f);
				break;
			case NelNBoss_Nusi.MA_FLW.SHOT_EXPLODED:
			{
				float num5 = (X.ZSIN2(this.anmt, 7f) - X.ZPOW(this.anmt - 7f, 23f)) * 1.9f * this.Mp.CLENB;
				num = 35f - X.Cos(this.angleanm_first_agR) * num5 * base.mpf_is_right;
				num2 = 20f - X.Sin(this.angleanm_first_agR) * num5;
				break;
			}
			case NelNBoss_Nusi.MA_FLW.DANCE:
			{
				float num4 = 1.5707964f - X.ZIGZAGI(this.anmt, 68f) * 3.1415927f * 0.18f;
				num = 30f + 30f * X.Cos(num4);
				num2 = 20f + 15f * X.Sin(num4);
				num3 = 0.3455752f * X.COSI(this.anmt, 91f);
				break;
			}
			case NelNBoss_Nusi.MA_FLW.RUN:
				num = 35f + 58f * X.ZSIN(this.anmt, 22f) + 11f * X.COSI(this.anmt, 24f);
				num2 = 10f - 12f * X.COSI(this.anmt, 19f);
				num3 = 3.1415927f * (-0.16f + X.COSI(this.anmt - 24f, 11.5f) * 0.06f);
				break;
			case NelNBoss_Nusi.MA_FLW.RUN2STATIC:
				num = 83f - 53f * X.ZPOW(this.anmt, 36f);
				num2 = 10f + X.ZPOW(this.anmt, 36f);
				if (this.anmt >= 36f)
				{
					this.ma_flower = NelNBoss_Nusi.MA_FLW.LAUGH;
				}
				break;
			case NelNBoss_Nusi.MA_FLW.DIE:
				num = 35f + X.COSI(this.anmt, 14.5f) * 8f + X.COSI(this.anmt, 5.25f) * 4f;
				num2 = -28f + X.COSI(this.anmt, 11.3f) * 8f + X.COSI(this.anmt, 6.31f) * 4f;
				num3 = 3.1415927f * (X.COSI(this.anmt, 11.5f) * 0.013f + X.COSI(this.anmt, 7.22f) * 0.004f);
				break;
			}
			this.Anm.after_offset_x = num * base.mpf_is_right;
			this.Anm.after_offset_y = -num2;
			if (num3 != -1000f && this.angleanm_maxt == 0f)
			{
				this.Anm.rotationR = base.mpf_is_right * num3;
			}
			this.anmt += fcnt;
		}

		private bool considerOverDrive(NAI Nai)
		{
			return true;
		}

		public void angleAnimTo(float agR, float time, bool clear_rotspd = false)
		{
			if (clear_rotspd)
			{
				this.Anm.rotationR_speed = 0f;
			}
			if (time <= 0f || agR == this.Anm.rotationR)
			{
				this.Anm.rotationR = agR;
				this.angleanm_maxt = 0f;
				return;
			}
			this.angleanm_first_agR = X.correctangleR(this.Anm.rotationR);
			this.angleanm_dep_agR = agR;
			this.angleanm_t = 0f;
			this.angleanm_maxt = time;
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (this.ATentacleL != null)
			{
				if (!this.Anm.TempStop.isActive())
				{
					this.fineMaPosition(this.TS);
				}
				if (this.depert_stun_hp < 0 && this.state == NelEnemy.STATE.STAND && Map2d.can_handle && !base.event_enemy_flag)
				{
					this.depert_stun_hp--;
					if (this.depert_stun_hp < -10)
					{
						if (this.burst_counter_success == 0 && this.Summoner != null)
						{
							if (this.Summoner.countActiveEnemy((NelEnemy N) => N is NelNBoss_Nusi, true) > 1)
							{
								this.depert_stun_hp = (int)X.Mx((float)this.hp - (float)this.maxhp * this.hp_level_attacking, 1f);
								this.EnCage.anmtype = NelNBoss_Nusi.MA_CAGE.HIDDEN;
								this.burst_counter_success++;
								goto IL_0112;
							}
						}
						this.EnCage.anmtype = NelNBoss_Nusi.MA_CAGE.APPEAL;
						this.initFaintStun();
					}
				}
				IL_0112:
				if (!this.Anm.TempStop.isActive())
				{
					this.EA_Base.checkFrameManual(this.TS, false);
				}
				for (int i = this.ATentacleL.Length - 1; i >= 0; i--)
				{
					this.ATentacleL[i].checkFrameManual(this.TS);
				}
				if (this.EA_FaintTentacle.alpha > 0f)
				{
					this.EA_FaintTentacle.checkFrameManual(this.TS, false);
				}
				if (this.angleanm_maxt > 0f)
				{
					this.angleanm_first_agR += this.Anm.rotationR_speed;
					this.angleanm_dep_agR += this.Anm.rotationR_speed;
					this.angleanm_t += this.TS;
					this.Anm.rotationR = this.angleanm_first_agR + X.angledifR(this.angleanm_first_agR, this.angleanm_dep_agR) * X.ZPOW(this.angleanm_t, this.angleanm_maxt);
					if (this.angleanm_t >= this.angleanm_maxt)
					{
						this.angleanm_maxt = 0f;
					}
				}
			}
		}

		public override void runPost()
		{
			base.runPost();
			float mpf_is_right = base.mpf_is_right;
			if (!this.faint_stun_lock && this.hp > 0 && !base.throw_ray && (!base.disappearing || this.isMainBgm()))
			{
				for (int i = this.Mp.count_players - 1; i >= 0; i--)
				{
					M2MoverPr pr = this.Mp.getPr(i);
					if (pr.getPhysic() != null)
					{
						float num = X.NI(0.25f, 0.5f, this.sink_ratio);
						if (base.isCoveringMv(pr, 0.12f, 0.5f) || (X.isCovering(base.mtop - 3.5f, base.mbottom + 5.5f, pr.mtop, pr.mbottom, 0f) && X.isCovering(base.mleft + num - (float)((mpf_is_right > 0f) ? 4 : 0), base.mright - num + (float)((mpf_is_right < 0f) ? 4 : 0), pr.mleft, pr.mright, 0f)))
						{
							PR pr2 = this.Mp.getPr(i) as PR;
							if (pr2 != null)
							{
								pr2.moveByHitCheck(this.Phy, FOCTYPE.HIT, 0.18f * mpf_is_right, 0.05f);
								pr2.addEnemySink(3f * this.sink_ratio, true, 0f);
							}
						}
					}
				}
			}
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			if (this.state == NelEnemy.STATE.DAMAGE)
			{
				this.PtcHld.killPtc("nusi_fainttentacle_drill_prepare", false);
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
			}
			if ((this.state == NelEnemy.STATE.DAMAGE || this.state == NelEnemy.STATE.STUN) && st == NelEnemy.STATE.STAND)
			{
				this.burst_counter_success = X.Mx(1, this.burst_counter_success);
				this.sink_ratio = 1f;
				if (this.state == NelEnemy.STATE.DAMAGE && this.hp < this.depert_stun_hp && this.depert_stun_hp > 0)
				{
					this.hp = X.Mn(X.Mx(this.depert_stun_hp + 200, this.hp), this.maxhp);
				}
			}
			if (st == NelEnemy.STATE.DAMAGE || st == NelEnemy.STATE.STUN || st == NelEnemy.STATE.DIE)
			{
				this.walk_st = 0;
			}
			if (this.EA_FaintTentacle != null)
			{
				if (st == NelEnemy.STATE.DIE)
				{
					this.SpSetPose("death_0", -1, null, false);
				}
				else
				{
					this.EA_Base.clearAnim("main0", -1000, null);
				}
				this.EA_Base.timescale = 1f;
				if (st == NelEnemy.STATE.STUN || st == NelEnemy.STATE.DIE)
				{
					this.EnCage.anmtype = NelNBoss_Nusi.MA_CAGE.APPEAL_OPEN;
				}
				this.EA_FaintTentacle.alpha = 0f;
			}
			base.changeState(st);
			if (st == NelEnemy.STATE.DAMAGE || st == NelEnemy.STATE.STUN)
			{
				this.Nai.clearTypeLock();
			}
			return this;
		}

		private void cureMpMax()
		{
			this.mp = this.maxmp;
			base.addF(NelEnemy.FLAG.FINE_HPMP_BAR);
		}

		public override void changeStateToDie()
		{
			if (!this.isStunBigDamage())
			{
				this.hp = 1;
				this.faintCounterExecuteChecking();
				if ((this.hp <= this.depert_stun_hp || this.depert_stun_hp <= 1) && !this.faint_stun_lock && this.state == NelEnemy.STATE.STAND)
				{
					this.initFaintStun();
					this.depert_stun_hp = X.Mx(1, this.depert_stun_hp);
				}
				return;
			}
			base.changeStateToDie();
		}

		public bool considerable
		{
			get
			{
				return this.is_alive && !base.event_enemy_flag && this.depert_stun_hp >= 0 && this.state == NelEnemy.STATE.STAND;
			}
		}

		public int getAiPhase()
		{
			if (this.aiphase2_lock > 0)
			{
				return X.Mn(this.burst_counter_success, 1);
			}
			return this.burst_counter_success;
		}

		public void progressPhase2Lock(bool lock_a2)
		{
			if (lock_a2)
			{
				this.aiphase2_lock = X.Mx(this.aiphase2_lock, this.aiphase2_lock_max - ((DIFF.I == 2) ? 1 : 0));
				return;
			}
			if (this.aiphase2_lock > 0)
			{
				this.aiphase2_lock--;
			}
		}

		private bool considerNormal(NAI Nai)
		{
			if (!this.considerable)
			{
				return true;
			}
			float target_sxdif = Nai.target_sxdif;
			bool flag;
			if (!this.Freeze.canMove(Nai, out flag))
			{
				return Nai.AddTicketB(NAI.TYPE.GAZE, 128, true);
			}
			if ((!Nai.isPrAlive() && Nai.RANtk(439) < 0.88f) || (Nai.isPrAbsorbed() && Nai.RANtk(512) < 0.95f))
			{
				return Nai.AddTicketB(NAI.TYPE.GAZE, 128, true);
			}
			if (this.ma_flower == NelNBoss_Nusi.MA_FLW.LAUGH)
			{
				this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
				if (this.Anm.poseIs("laugh", true))
				{
					this.Anm.setPose("stand", -1);
				}
			}
			bool flag2 = this.burst_counter_success >= 2 && !Nai.hasTypeLock(NAI.TYPE.WARP) && Nai.RANtk(3191) < 0.06f;
			if (Nai.HasF(NAI.FLAG.BOTHERED, true) || flag2)
			{
				return Nai.AddTicketB(NAI.TYPE.WARP, 134, true);
			}
			if (Nai.HasF(NAI.FLAG.POWERED, true))
			{
				this.progressPhase2Lock(flag);
				return Nai.AddTicketB(NAI.TYPE.MAG_0, 129, true);
			}
			float num = 0.3f;
			bool flag3 = !Nai.hasTypeLock(NAI.TYPE.MAG_0);
			if (target_sxdif > 9f)
			{
				if (Nai.fnBasicPunch(Nai, 129, (float)(flag3 ? 25 : 40), 30f, 0f, 0f, 8142, true))
				{
					this.progressPhase2Lock(flag);
					return false;
				}
			}
			else
			{
				num = 0.1f;
				if (target_sxdif < ((Nai.RANtk(3111) < 0.7f || this.burst_counter_success <= 1) ? 4.6f : 8f) && Nai.RANtk(4134) < 0.77f && !Nai.hasTypeLock(NAI.TYPE.MAG))
				{
					this.progressPhase2Lock(flag);
					return Nai.AddTicketB(NAI.TYPE.MAG, 129, true);
				}
				if (Nai.fnBasicPunch(Nai, 129, 30f, 50f, 0f, 0f, 9114, true))
				{
					this.progressPhase2Lock(flag);
					return false;
				}
			}
			if (Nai.RANtk(3441) < num && flag3)
			{
				this.progressPhase2Lock(flag);
				return Nai.AddTicketB(NAI.TYPE.MAG_0, 129, true);
			}
			return Nai.AddTicketB(NAI.TYPE.WAIT, 1, true);
		}

		public void AddFreeze(int level, float t)
		{
			this.Freeze.Set(this.Nai, level, t, 180f);
		}

		public void AddFreezeToTentacle(int level, float t, float m1_extend = 180f)
		{
			for (int i = 1; i >= 0; i--)
			{
				this.AEnTentacle[i].AddFreeze(level, t, m1_extend);
			}
		}

		public void AddFreezeToTentacleToOther(int level, float t, NelNNusiTentacle T, float m1_extend = 180f)
		{
			int aiPhase = this.getAiPhase();
			t = X.Mx(t - ((aiPhase >= 2) ? this.time_p2_reduce_base_time : 0f), 0f);
			for (int i = 2; i >= 0; i--)
			{
				NelNNusiTentacle nelNNusiTentacle = this.AEnTentacle[i];
				if (!(T == nelNNusiTentacle))
				{
					nelNNusiTentacle.AddFreeze(level, t, m1_extend);
				}
			}
		}

		public void TentacleAfterDelay(NaTicket Tk, float t)
		{
			int aiPhase = this.getAiPhase();
			Tk.AfterDelay(((aiPhase >= 2) ? this.time_p2_tentacle_afterdelay_ratio : 1f) * t);
		}

		public void TentacleAddTypeLock(NAI.TYPE type, float t)
		{
			for (int i = 2; i >= 0; i--)
			{
				this.AEnTentacle[i].getAI().addTypeLock(type, t);
			}
		}

		public int countActiveAttacker(NelEnemy Attacker)
		{
			int num = 0;
			for (int i = -1; i < 2; i++)
			{
				NelEnemy nelEnemy = ((i == -1) ? this : this.AEnTentacle[i]);
				if (nelEnemy != Attacker && nelEnemy.getAI().hasPriorityTicket(129, false, true))
				{
					num++;
				}
			}
			return num;
		}

		public NelNBoss_Nusi.TentacleLink initLink(NelNNusiTentacle Tentacle)
		{
			int num = 2;
			for (int i = 0; i < num; i++)
			{
				NelNBoss_Nusi.TentacleLink tentacleLink = this.ATentacleL[(this.link_start_i + i) % 2];
				if (tentacleLink.LinkTentacle == null)
				{
					this.link_start_i = i + 1;
					tentacleLink.LinkTentacle = Tentacle;
					return tentacleLink;
				}
			}
			return null;
		}

		public AIM getTentacleGrabAim(NelNNusiTentacle Tentacle)
		{
			for (int i = 0; i < 2; i++)
			{
				NelNNusiTentacle nelNNusiTentacle = this.AEnTentacle[i];
				if (!(nelNNusiTentacle == Tentacle) && (nelNNusiTentacle.getAI().isFrontType(NAI.TYPE.PUNCH_2, PROG.ACTIVE) || nelNNusiTentacle.isNormalAbsorbing()))
				{
					return CAim.get_opposite(nelNNusiTentacle.aim);
				}
			}
			if (X.xors(2) != 0)
			{
				return AIM.B;
			}
			return AIM.T;
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.PUNCH:
				return this.runPunch_SmallShot(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_0:
				return this.runPunch0_SporeRain(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_1:
			case NAI.TYPE.PUNCH_2:
			case NAI.TYPE.PUNCH_WEED:
				break;
			case NAI.TYPE.MAG:
				return this.runMag_ChargeSplash(Tk.initProgress(this), Tk);
			case NAI.TYPE.MAG_0:
				return this.runMag0_CallFollower(Tk.initProgress(this), Tk);
			default:
				switch (type)
				{
				case NAI.TYPE.WARP:
					return this.runWarp_BigRun(Tk.initProgress(this), Tk);
				case NAI.TYPE.GAZE:
					return this.runGaze(Tk.initProgress(this), Tk);
				case NAI.TYPE.WAIT:
					Tk.after_delay = 50f + this.Nai.RANtk(840) * 70f;
					return false;
				}
				break;
			}
			return false;
		}

		public override void quitTicket(NaTicket Tk)
		{
			bool flag = true;
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.GAZE)
				{
					flag = false;
				}
				if (Tk.type == NAI.TYPE.PUNCH)
				{
					base.killPtc("nusi_shot_charge", false);
					base.killPtc("nusi_shot_targetting", false);
					this.Nai.addTypeLock(NAI.TYPE.PUNCH, 240f);
				}
				if (Tk.type == NAI.TYPE.MAG)
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG, 400f);
				}
				if (Tk.type == NAI.TYPE.PUNCH_0)
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 380f);
					this.EA_Base.clearAnim("main0", -1000, null);
				}
				if (Tk.type == NAI.TYPE.WARP)
				{
					this.EA_Base.remListenerEvent(new AnimationState.TrackEntryEventDelegate(this.fnEAEventBigRun));
				}
			}
			this.faint_stun_lock = false;
			this.Anm.timescale = 1f;
			this.EA_Base.timescale = 1f;
			if (flag)
			{
				this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
			}
			base.quitTicket(Tk);
		}

		private bool runGaze(bool init_flag, NaTicket Tk)
		{
			this.t = 0f;
			bool flag = false;
			if (this.Anm.poseIs("laugh", true) || this.Nai.isPrAbsorbed() || X.XORSP() < 0.3f)
			{
				if (this.ma_flower != NelNBoss_Nusi.MA_FLW.LAUGH)
				{
					if (this.Nai.isPrAbsorbed())
					{
						this.playSndPos("nusivo_laugh", 1);
					}
					this.ma_flower = NelNBoss_Nusi.MA_FLW.LAUGH;
				}
				if (this.Nai.isPrAbsorbed())
				{
					flag = true;
				}
				this.SpSetPose("laugh", -1, null, false);
			}
			Tk.after_delay = X.NIXP(30f, 70f);
			if (flag)
			{
				this.AddFreeze(2, Tk.after_delay + 135f);
			}
			return false;
		}

		private bool runPunch_SmallShot(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("shot_0", -1, null, false);
				base.PtcST("nusi_shot_targetting", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				base.PtcVar("maxt", 90.0).PtcST("nusi_shot_prep_0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.walk_st = 3 + X.xors((this.burst_counter_success >= 2) ? 3 : 2);
				this.walk_st = X.MMX(1, this.walk_st, this.mp / this.McsShot.consume);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.SHOT;
				this.AddFreezeToTentacle(1, (float)(90 + 77 * (this.walk_st - X.Mn(2, this.burst_counter_success - 1)) + 40), 180f);
				this.AddFreezeToTentacle(0, (float)(90 + 77 * this.walk_st + 40), 20f);
				this.angleanm_maxt = 0f;
				this.angleanm_dep_agR = 0f;
				Tk.prog = ((X.XORSP() < 0.38f) ? PROG.PROG1 : PROG.PROG0);
				this.walk_time = 0f;
			}
			if (Tk.prog == PROG.PROG0 || Tk.prog == PROG.PROG1)
			{
				if (this.walk_time == 0f && this.t >= 90f)
				{
					this.t = 0f;
					this.walk_time = 1f;
				}
				bool flag = false;
				if (this.walk_time == 1f)
				{
					this.t = 0f;
					this.walk_time = 2f;
					flag = this.Anm.poseIs("shot_2", true);
					this.SpSetPose("shot_1", -1, null, false);
					base.killPtc("nusi_shot_charge", false);
					base.PtcST("nusi_shot_prep_1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_S);
				}
				Vector4 vector = Vector4.zero;
				if (this.walk_time <= 2f)
				{
					float num = this.Nai.target_x;
					float num2 = base.mbottom + 0.4f;
					if (base.mpf_is_right < 0f)
					{
						num = X.Mn(num, base.mleft - 1.2f);
					}
					else
					{
						num = X.Mx(num, base.mright + 1.2f);
					}
					float num3;
					if (Tk.prog == PROG.PROG0)
					{
						num3 = this.Mp.GAR(base.x, base.y, num, num2);
					}
					else
					{
						Vector2 shotExplodePos = this.ShotExplodePos;
						num2 -= 0.2f;
						float num4 = num - shotExplodePos.x;
						vector = M2Mover.getJumpVelocityT(this.Mp, num4, num2 - shotExplodePos.y, X.MMX(30f, X.Abs(num4) / 0.25f, 170f), 0.18f, 0f);
						num3 = this.Mp.GAR(0f, 0f, vector.x, vector.y);
					}
					float num5 = X.NI(0.009f, 0.114f, X.ZPOW(this.t, 14f)) * 3.1415927f;
					if (this.t == 0f)
					{
						this.angleanm_dep_agR = num3;
						if (flag)
						{
							num5 = 0.9424779f;
						}
					}
					else
					{
						float num6 = 0.006283186f;
						this.angleanm_dep_agR = base.VALWALKANGLER(this.angleanm_dep_agR, num3, num6);
					}
					this.shotagR = base.VALWALKANGLER(this.shotagR, this.angleanm_dep_agR, num5);
					this.ShotDep.Set(num, num2);
				}
				if (this.walk_time == 2f && this.t >= 22f)
				{
					this.t = 0f;
					this.walk_time = 3f;
					bool flag2 = Tk.prog == PROG.PROG1;
					this.walk_st--;
					this.angleanm_first_agR = this.shotagR;
					this.ma_flower = NelNBoss_Nusi.MA_FLW.SHOT_EXPLODED;
					this.anmt = 0f;
					bool flag3 = this.Nai.isPrGaraakiState() || X.xors(100) < 30;
					Vector2 shotExplodePos2 = this.ShotExplodePos;
					MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE);
					magicItem.Atk0 = (flag3 ? this.AtkShotGranade : this.AtkShotMist);
					this.MpConsume(this.McsShot, magicItem, 1f, 1f);
					magicItem.efpos_s = (magicItem.raypos_s = (magicItem.aimagr_calc_s = (magicItem.aimagr_calc_vector_d = true)));
					magicItem.sx = shotExplodePos2.x;
					magicItem.sy = shotExplodePos2.y;
					magicItem.sa = this.shotagR;
					magicItem.sz = (float)(flag3 ? 1 : 0);
					if (flag2)
					{
						magicItem.createDropper(vector.x, vector.y, 0.22f, -1f, -1f);
						magicItem.Dro.gravity_scale = 0.18f;
						magicItem.Dro.bounce_y_reduce = 0.01f;
						magicItem.Dro.bounce_x_reduce_when_ground = 1f;
					}
					else
					{
						magicItem.createDropper(X.Cos(magicItem.sa) * 0.34f, -X.Sin(magicItem.sa) * 0.34f, 0.22f, -1f, -1f);
						magicItem.Dro.gravity_scale = 0f;
						magicItem.Dro.bounce_y_reduce = 0f;
					}
					magicItem.sa += 3.1415927f;
					magicItem.Dro.type |= (DROP_TYPE)408;
					magicItem.initFunc(this.FD_MgRunShot, this.FD_MgRunDrawShot);
					base.killPtc("nusi_shot_targetting", false);
					this.SpSetPose("shot_2", -1, null, false);
				}
				if (this.walk_time == 3f)
				{
					this.shotagR = X.NI(this.angleanm_first_agR, this.angleanm_first_agR + base.mpf_is_right * 3.1415927f * 0.33f, X.ZSIN2(this.t, 34f));
					if (this.t >= 55f)
					{
						this.t = 0f;
						if (this.walk_st <= 0)
						{
							Tk.prog = PROG.PROG5;
							this.SpSetPose("shot2stand", -1, null, false);
							base.killPtc("nusi_shot_charge", false);
							this.playSndPos("nusi_leaf_closing", 1);
							this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
							this.angleAnimTo(0f, 50f, false);
							Tk.AfterDelay(80f);
							return false;
						}
						this.walk_time = 1f;
						base.PtcST("nusi_shot_targetting", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
						Tk.prog = ((X.XORSP() < 0.38f) ? PROG.PROG1 : PROG.PROG0);
					}
				}
			}
			return true;
		}

		private bool MgRunShot(MagicItem Mg, float fcnt)
		{
			if (base.destructed || this.state != NelEnemy.STATE.STAND)
			{
				return false;
			}
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.REFLECTED;
				Mg.Ray.HitLock(-1f, null);
				Mg.wind_apply_s_level = 1f;
				Mg.dz = 1f;
				Mg.aim_agR = Mg.sa - 3.1415927f;
				Mg.PtcVar("granade", (double)Mg.sz).PtcST("nusi_shot_trigger", PTCThread.StFollow.NO_FOLLOW, false);
				Mg.projectile_power = 200;
				Mg.Ray.cohitable_min_projectile = 50;
				Mg.Ray.Atk = Mg.Atk0;
			}
			else
			{
				if (Mg.dz > 0f)
				{
					if (Mg.dz >= 1f)
					{
						Mg.dz -= fcnt;
					}
					if (Mg.dz <= 1f)
					{
						Mg.dz = (float)((Mg.Dro.gravity_scale == 0f) ? 0 : 1);
					}
					Mg.aim_agR = this.Mp.GAR(0f, 0f, Mg.Dro.vx, Mg.Dro.vy);
					Mg.Ray.AngleR(Mg.aim_agR).DirXyM(Mg.Dro.vx, Mg.Dro.vy);
				}
				Mg.calced_aim_pos = true;
				Mg.sa = Mg.aim_agR + 3.1415927f;
				Mg.Atk0.burst_center = 0f;
				Mg.setRayStartPos(Mg.Ray);
				bool flag = false;
				if (Mg.Dro.x_bounced || Mg.Dro.on_ground)
				{
					flag = true;
					Mg.Atk0.burst_center = 1f;
					Mg.Ray.RadiusM(0.9f);
				}
				else
				{
					Mg.Ray.RadiusM(0.22f);
				}
				HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(X.absmin(Mg.Dro.vx, 0.25f), 0f), HITTYPE.NONE);
				if ((hittype & HITTYPE.REFLECTED) != HITTYPE.NONE)
				{
					Mg.Ray.clearTempReflect();
					if (Mg.dz <= 1f)
					{
						flag = false;
						Mg.Dro.vx = -Mg.Dro.vx;
						Mg.Dro.vy = -Mg.Dro.vy;
						Mg.dz = 15f;
						Mg.PtcVar("agR", (double)(Mg.aim_agR + 3.1415927f)).PtcST("reflect_circle", PTCThread.StFollow.NO_FOLLOW, true);
						Mg.Ray.hit_en = true;
						Mg.Ray.hittype |= HITTYPE.BERSERK_MYSELF;
						Mg.explode(false);
					}
				}
				else if ((hittype & HITTYPE.BREAK) != HITTYPE.NONE)
				{
					flag = true;
				}
				else if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
				{
					Mg.kill(0.125f);
					return false;
				}
				if (flag)
				{
					Mg.PtcVar("granade", (double)Mg.sz).PtcST("nusi_shot_explode", PTCThread.StFollow.NO_FOLLOW, false);
					if (Mg.sz == 0f)
					{
						Mg.M2D.MIST.addMistGenerator(this.MKShotMist, this.MKShotMist.calcAmount(220, 1.4f), (int)Mg.sx, (int)(Mg.sy - 1.5f), false);
					}
					else
					{
						int num = (int)(Mg.sy - 0.3f);
						int num2 = (int)(Mg.sx - 2f);
						for (int i = 0; i < 5; i++)
						{
							for (int j = 0; j < 2; j++)
							{
								Mg.M2D.STAIN.Set((float)(num2 + i), (float)(num + j), StainItem.TYPE.FIRE, AIM.B, X.NIXP(200f, 250f), null);
							}
						}
					}
					Mg.explode(false);
					return false;
				}
			}
			return true;
		}

		private bool MgRunDrawShot(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0 || base.destructed)
			{
				return true;
			}
			PxlSequence sequence = this.PoseShotBody.getSequence((int)Mg.sz);
			uint num;
			uint num2;
			uint num3;
			uint num4;
			if (Mg.sz == 1f)
			{
				num = 4293172595U;
				num2 = 4294901760U;
				num3 = 4282437314U;
				num4 = 4283461529U;
			}
			else
			{
				num = 4292589779U;
				num2 = 4283781716U;
				num3 = 4288917927U;
				num4 = 4283851610U;
			}
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", this.Anm.getMI(), BLEND.NORMALP3, false);
			MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
			MeshDrawer meshImg3 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, true);
			meshImg.allocUv23(4, false);
			meshImg.Col = MTRX.ColWhite;
			meshImg.Uv23(meshImg.ColGrd.Black().blend(num, X.ZLINE(X.COSI(this.Mp.floort, 2.55f) * 0.85f + X.COSI(this.Mp.floort, 4.73f) * 0.15f)).C, false);
			meshImg.RotaPF(0f, 0f, 1f, 1f, Mg.aim_agR, sequence.getFrame(X.ANM((int)Mg.t, 3, 3f)), false, false, false, uint.MaxValue, false, 0);
			meshImg.allocUv23(0, true);
			meshImg2.initForImg(MTRX.EffBlurCircle245, 0);
			meshImg3.initForImg(MTRX.EffBlurCircle245, 0);
			meshImg2.Col = meshImg2.ColGrd.Set(num).blend(num2, 0.5f + X.COSI(Mg.t, 11.44f) * 0.4f + X.COSI(Mg.t, 5.11f) * 0.1f).C;
			float num5 = 136f * (0.7f + 0.22f * X.COSI(Mg.t, 13.52f) + 0.08f * X.COSI(Mg.t, 9.37f));
			meshImg2.Rect(0f, 0f, num5, num5, false);
			meshImg3.Col = meshImg3.ColGrd.Set(num3).blend(num4, 0.5f + X.COSI(Mg.t, 14.78f) * 0.4f + X.COSI(Mg.t, 7.66f) * 0.1f).C;
			num5 = 161f * (0.6f + 0.32f * X.COSI(Mg.t, 14.14f) + 0.08f * X.COSI(Mg.t, 7.23f));
			meshImg3.Rect(0f, 0f, num5, num5, false);
			return true;
		}

		private bool runMag_ChargeSplash(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("close", -1, null, false);
				this.playSndPos("nusivo_charge_0", 1);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
				this.AddFreezeToTentacle(1, 10f, 180f);
				this.AddFreezeToTentacle(0, 288f, 20f);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 40, true))
			{
				this.SpSetPose("closecharge", -1, null, false);
				this.t = 100f;
				this.walk_time = 0f;
				this.walk_st = 0;
			}
			if (Tk.prog == PROG.PROG0 && this.t >= 100f)
			{
				int walk_st = this.walk_st;
				this.walk_st = walk_st + 1;
				if (walk_st >= 8)
				{
					Tk.prog = PROG.PROG1;
					this.t = 0f;
					this.walk_st = 0;
					base.PtcST("nusi_charge_hit_0", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.SpSetPose("chargeshot_0", -1, null, false);
				}
				else
				{
					this.t = 74f;
					base.PtcST("nusi_charge_prep_beep", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.TeCon.setColorBlinkAddFadeout(14.4f, 14.3f, 1f, 16606644, 0);
					this.Anm.timescale = X.NI(0.4f, 2f, X.ZLINE((float)(this.walk_st - 1), 8f));
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, 26, true))
			{
				base.nM2D.MIST.addMistGenerator(NelNBoss_Nusi.MkCharge, NelNBoss_Nusi.MkCharge.calcAmount(320, 1.4f), (int)(base.x + base.mpf_is_right * 2f), (int)base.y, false);
				base.PtcVar("radius", (double)(this.TkiCharge.radius * this.Mp.CLENB)).PtcST("nusi_charge_hit_1", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				base.tackleInit(this.AtkCharge, this.TkiCharge, MGHIT.AUTO);
				this.walk_time = this.Mp.floort + 8f;
				this.SpSetPose("chargeshot_1", -1, null, false);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.GRAWL;
			}
			if (Tk.prog == PROG.PROG2 && Tk.Progress(ref this.t, 95, true))
			{
				if (this.walk_st != 1)
				{
					this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
					this.playSndPos("nusivo_charge_1", 1);
					Tk.AfterDelay(120f);
					return false;
				}
				this.SpSetPose("laugh", -1, null, false);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.LAUGH;
			}
			if (Tk.prog == PROG.PROG3 && Tk.Progress(ref this.t, 110, true))
			{
				Tk.AfterDelay(120f);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
				this.SpSetPose("close2stand", -1, null, false);
				return false;
			}
			return this;
		}

		public float spore_cx
		{
			get
			{
				return base.x + base.mpf_is_right * (this.sizex + 8.3f);
			}
		}

		public float spore_cy
		{
			get
			{
				return base.y - 6.4f;
			}
		}

		private bool runPunch0_SporeRain(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				if (base.nM2D.MGC.countMg(this.FD_sporeCount, this) >= 9)
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 250f);
					return false;
				}
				this.t = 0f;
				this.SpSetPose("close", -1, null, false);
				base.PtcST("nusi_spore_prep_0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.EA_Base.clearAnim("spore", -1000, null);
				this.EA_Base.timescale = 0.3f;
				this.walk_st = 3 + X.xors((this.burst_counter_success >= 2) ? 3 : 2);
				this.AddFreezeToTentacle(1, 160f, 180f);
				this.AddFreezeToTentacle(0, (float)(70 + 20 * X.Mx(0, this.walk_st) + 80 + 22), 0f);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 20, true))
			{
				this.SpSetPose("rotatedance", -1, null, false);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.DANCE;
				base.PtcVar("scx", (double)this.spore_cx).PtcVar("scy", (double)this.spore_cy).PtcVar("maxt", (double)(50 + 20 * this.walk_st))
					.PtcST("nusi_spore_prep_1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.EA_Base.timescale = 1f;
				this.t = 50f;
			}
			if (Tk.prog == PROG.PROG0 && this.t >= 100f)
			{
				int num = this.walk_st - 1;
				this.walk_st = num;
				if (num >= 0)
				{
					this.t = 80f;
					this.EA_Base.timescale = X.NIXP(0.7f, 1.5f);
					MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE);
					magicItem.Atk0 = this.AtkSporeSpike;
					magicItem.Ray.HitLock(40f, this.OHitSpore);
					this.MpConsume(this.McsSpore, magicItem, 1f, 1f);
					magicItem.sx = this.spore_cx + X.XORSPS() * 7f;
					magicItem.sy = this.spore_cy;
					magicItem.sz = X.NIXP(0.4f, 2.1f);
					magicItem.sa = ((this.Nai.target_x < magicItem.sx) ? 0f : 3.1415927f);
					magicItem.da = X.NIXP(0.02f, 0.034f);
					magicItem.efpos_s = (magicItem.raypos_s = (magicItem.aimagr_calc_s = (magicItem.aimagr_calc_vector_d = true)));
					magicItem.createDropper(0f, magicItem.da, 0.1f, -1f, -1f);
					magicItem.Dro.gravity_scale = 0f;
					magicItem.Dro.bounce_x_reduce = 0.8f;
					magicItem.Dro.bounce_y_reduce = 0f;
					magicItem.Dro.type |= (DROP_TYPE)408;
					magicItem.initFunc(this.FD_MgRunSpore, this.FD_MgRunDrawSpore);
				}
				else
				{
					Tk.prog = PROG.PROG1;
					this.t = 0f;
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, 80, true))
			{
				this.SpSetPose("dance2stand", -1, null, false);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
				Tk.AfterDelay(45f);
				this.EA_Base.clearAnim("main0", -1000, null);
				return false;
			}
			return true;
		}

		private bool MgRunSpore(MagicItem Mg, float fcnt)
		{
			if (base.destructed || !this.is_alive)
			{
				return false;
			}
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.Ray.HitLock(40f, null);
				Mg.Ray.quit_on_reflection_break = false;
				Mg.PtcST("nusi_spore_fall", PTCThread.StFollow.NO_FOLLOW, false);
				Mg.Ray.check_hit_wall = (Mg.Ray.check_other_hit = false);
				Mg.Ray.hit_target_max = 4;
				Mg.dz = X.NIXP(90f, 150f);
				Mg.wind_apply_s_level = 1f;
				Mg.projectile_power = 200;
				Mg.Ray.cohitable_min_projectile = 50;
				return true;
			}
			if (Mg.phase == 1)
			{
				Mg.Dro.vx = -Mg.sz * X.Sin(Mg.sa + Mg.t / Mg.dz * 6.2831855f) / Mg.dz * 6.2831855f;
				Mg.Dro.vy = X.VALWALK(Mg.Dro.vy, Mg.da, 0.008f);
				Mg.setRayStartPos(Mg.Ray);
				if ((Mg.Ray.Cast(false, null, false) & HITTYPE._TEMP_REFLECT) != HITTYPE.NONE)
				{
					Mg.reflectV(Mg.Ray, ref Mg.Dro.vx, ref Mg.Dro.vy, 0.3f, 0.25f, true);
					Mg.Ray.clearTempReflect();
					Mg.Dro.vy = X.Mn(Mg.Dro.vy, 0f) - 0.125f;
					Mg.Ray.projectile_power = 1;
					Mg.phase = 9;
					return this;
				}
				if (Mg.Dro.on_ground && Mg.t >= 30f)
				{
					Mg.phase = 10;
					Mg.t = 0f;
					Mg.sa = X.NIXP(70f, 90f) * (float)((X.XORSP() < 0.25f) ? 3 : 1);
					Mg.Dro.gravity_scale = 0.2f;
					Mg.killEffect();
				}
			}
			if (Mg.phase == 10 && Mg.t >= Mg.sa && Mg.Dro.on_ground)
			{
				if ((this.nattr & (ENATTR.ICE | ENATTR.ACME)) != ENATTR.NORMAL)
				{
					EnemyAttr.Splash(this, Mg.sx, Mg.sy, 1.5f, 0f, 1f);
					return false;
				}
				if ((this.nattr & (ENATTR.FIRE | ENATTR.SLIMY)) != ENATTR.NORMAL)
				{
					EnemyAttr.setStain(Mg.M2D, Mg.sx, Mg.sy, 1, this.nattr, 380f, true);
					return false;
				}
				Mg.Dro.gravity_scale = 0.4f;
				Mg.phase = 20;
				Mg.t = 0f;
				Mg.Ray.RadiusM(0.6f);
				Mg.PtcST("nusi_spore_spike", PTCThread.StFollow.FOLLOW_S, false);
			}
			if (Mg.phase == 20)
			{
				Mg.setRayStartPos(Mg.Ray);
				HITTYPE hittype;
				if (this.Nai.isPrAbsorbed())
				{
					hittype = Mg.Ray.Cast(false, null, false);
				}
				else
				{
					hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
				}
				if ((hittype & HITTYPE._TEMP_REFLECT) != HITTYPE.NONE)
				{
					Mg.reflectV(Mg.Ray, ref Mg.Dro.vx, ref Mg.Dro.vy, 0.2f, 0.25f, true);
					Mg.Ray.clearTempReflect();
					if (X.Abs(Mg.Dro.vx) > 0.1f || (hittype & HITTYPE.KILLED_OR_BREAK) != HITTYPE.NONE)
					{
						Mg.Dro.vy = X.Mn(Mg.Dro.vy, 0f) - 0.14f;
						Mg.phase = 29;
						Mg.Ray.projectile_power = 1;
						return this;
					}
				}
			}
			if (Mg.phase == 9 || Mg.phase == 29)
			{
				bool flag = false;
				if (Mg.Ray.hit_pr)
				{
					Mg.t = 0f;
					Mg.explode(false);
					Mg.killEffect();
					Mg.PtcST("nusi_reflected_spore", PTCThread.StFollow.NO_FOLLOW, false);
					Mg.Ray.hit_en = true;
					Mg.Ray.hit_pr = false;
					Mg.Ray.hittype |= HITTYPE.BERSERK_MYSELF;
					Mg.Ray.check_hit_wall = false;
				}
				if (Mg.phase >= 29)
				{
					Mg.calcAimPos(false);
					Mg.Ray.AngleR(Mg.aim_agR).DirXyM(Mg.Dro.vx, Mg.Dro.vy);
					flag = (Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE) & HITTYPE.KILLED_OR_BREAK) > HITTYPE.NONE;
				}
				Mg.Dro.gravity_scale = 0.15f;
				if (flag || (Mg.t >= 5f && Mg.Dro.on_ground) || Mg.Dro.x_bounced)
				{
					Mg.PtcST("nusi_spore_killed", PTCThread.StFollow.NO_FOLLOW, false);
					return false;
				}
			}
			return true;
		}

		private bool MgRunDrawSpore(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0 || Mg.Dro == null || base.destructed)
			{
				return true;
			}
			Map2d mp = Mg.Mp;
			bool flag = Mg.phase == 9 || Mg.phase == 29;
			C32 c = EffectItem.Col1.White().setA(0f);
			float num = 0f;
			float num2 = 1f;
			float num3 = 1f;
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", this.Anm.getMI(), BLEND.NORMALP3, false);
			MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, true);
			meshImg.allocUv2(4, false);
			float num4 = 0f;
			uint ran = X.GETRAN2(Mg.id, 3);
			if (flag)
			{
				num = (float)X.MPF(Mg.Dro.vx < 0f) * Mg.t / 55f * 6.2831855f;
				c.rgb = 15472999U;
				c.setA1(X.Mx(X.COSI(Mg.Mp.floort, 11.4f) * 0.7f + X.COSI(Mg.Mp.floort, 7.13f) * 0.3f, 0f));
			}
			if (Mg.phase == 1)
			{
				num2 *= X.ZSIN(Mg.t, 40f);
				num3 = num2;
			}
			if (Mg.phase == 10)
			{
				num2 *= 1f - X.ZPOW(Mg.t, 45f);
				num3 = X.NI(0.5f, 1f, num2);
				num4 = -X.ZPOW(Mg.t, 30f) * 0.2f * mp.CLENB;
			}
			if (Mg.phase == 20)
			{
				float num5 = X.ZSIN2(Mg.t, 14f);
				num2 = num5 * 1.25f - X.ZCOS(Mg.t - 10f, 24f) * 0.25f;
				num3 = X.NI(0.3f, 2.4f, num5);
			}
			else if (Mg.phase >= 20)
			{
				num3 = 2.4f;
			}
			meshImg.Col = meshImg.ColGrd.White().blendRgb(c.C, 1f, true).C;
			meshImg2.ColGrd.Set(4283017793U).blend(4289741260U, X.COSI(mp.floort, 23.7f) * 0.7f + X.COSI(mp.floort, 7.13f) * 0.3f);
			uint num7;
			if (Mg.phase < 20)
			{
				MeshDrawer meshImg3 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
				meshImg3.Col = meshImg3.ColGrd.Set(meshImg2.ColGrd).blendRgb(c.C, 1f, true).C;
				float num6 = 20f + 3f * X.COSI(Mg.t, 8.29f) + 0.6f * X.COSI(Mg.t, 5.43f);
				meshImg3.initForImg(MTRX.EffBlurCircle245, 0);
				meshImg3.Rect(0f, num4, num6 * num3, num6 * num3, false);
				num7 = 4286147954U;
				num += X.COSI((float)(Mg.id * 23) + Mg.Mp.floort, 140f) * 3.1415927f * 0.13f;
				meshImg.RotaPF(0f, num4, num2, num2, num, this.SqSpore.getFrame(Mg.id % this.SqSpore.countFrames()), ran % 2U == 0U, false, false, uint.MaxValue, false, 0);
				meshImg2.ColGrd.Set(4289967027U).blend(4282276952U, X.COSI(mp.floort, 23.7f) * 0.7f + X.COSI(mp.floort, 7.13f) * 0.3f);
			}
			else
			{
				num7 = 4290013829U;
				meshImg.RotaPF(0f, num4, num2, num2, num, this.SqSporeSpike.getFrame(Mg.id % this.SqSporeSpike.countFrames()), ran % 2U == 0U, false, false, uint.MaxValue, false, 0);
			}
			meshImg.Uv23(meshImg.ColGrd.Set(num7).multiply(X.COSI(Mg.Mp.floort, 42.13f) * 0.6f, false).C, false);
			meshImg.allocUv23(0, true);
			meshImg2.initForImg(MTRX.EffBlurCircle245, 0);
			meshImg2.Col = meshImg2.ColGrd.blendRgb(c.C, 1f, true).C;
			float num8 = 70f + 6f * X.COSI(Mg.t, 9.31f) + 2f * X.COSI(Mg.t, 6.51f);
			meshImg2.Rect(0f, num4, num8 * num3, num8 * num3, false);
			return true;
		}

		private bool runMag0_CallFollower(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				if (this.Summoner == null)
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG_0, -1f);
					return false;
				}
				if (this.follower_count_cache < 0)
				{
					this.follower_count_cache = this.Summoner.countSplitterEnemy(this.follower_spl_id, true, false, false);
				}
				int num = 1;
				if (this.follower_count_cache <= ((this.burst_counter_success >= 2) ? 1 : 0))
				{
					int num2 = X.Mn(this.burst_counter_success, 2) + X.IntC((float)base.nM2D.NightCon.summoner_enemy_count_addition(this.Summoner.Summoner) * 0.3f);
					num = this.Summoner.callFollowerEnemy(ref this.follower_spl_id, 160, num2);
				}
				if (num < 2)
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG_0, (float)((num == 0) ? (-1) : 320));
					return false;
				}
				this.follower_count_cache = -1;
				this.playSndPos("nusivo_call_follower", 1);
				this.SpSetPose("close2laugh", -1, null, false);
				this.EA_Base.clearAnim("call_follower", -1000, null);
				this.AddFreezeToTentacle(2, 400f, 180f);
				this.AddFreezeToTentacle(0, 430f, 140f);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 360, true))
			{
				this.SpSetPose("open2stand", -1, null, false);
				this.playSndPos("nusi_leaf_laugh", 1);
				this.EA_Base.clearAnim("call2main", -1000, null);
				Tk.AfterDelay(200f);
				this.Nai.addTypeLock(NAI.TYPE.MAG_0, 2400f);
				return false;
			}
			return true;
		}

		public void otherEnemyKilled(NelEnemy Other)
		{
			this.follower_count_cache = -1;
		}

		public override void initPublishKill(M2MagicCaster Target)
		{
			if (Target is NelEnemy && this.is_alive && Target != this && this.Summoner != null && this.Summoner.is_follower(Target as NelEnemy, null))
			{
				return;
			}
			base.initPublishKill(Target);
		}

		private bool runWarp_BigRun(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 30;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				this.AddFreezeToTentacle(2, 150f, 0f);
				for (int i = 0; i < 2; i++)
				{
					NelNNusiTentacle nelNNusiTentacle = this.AEnTentacle[i];
					if (nelNNusiTentacle.isAbsorbState() || nelNNusiTentacle.getAI().hasPriorityTicket(129, false, true))
					{
						this.t = 0f;
						this.walk_st = 60;
						return this;
					}
				}
				if (Tk.Progress(ref this.t, this.walk_st, true))
				{
					base.PtcST("nusi_bigran_growl", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
					this.Anm.setPose("grawl", -1);
					this.walk_st = 0;
					this.EA_Base.clearAnim("stun", -1000, null);
					this.EA_Base.fine_intv = 0f;
					this.ma_flower = NelNBoss_Nusi.MA_FLW.GRAWL;
					this.clearTentacleLink();
					this.VTentaclePose(NelNBoss_Nusi.TENPOSE.laugh, 2f, -1);
					this.VTentacleFineIntv(2f);
					for (int j = 0; j < 2; j++)
					{
						this.AEnTentacle[j].initBigRun();
					}
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 40f)
				{
					this.faint_stun_lock = true;
					if (this.walk_st == 0)
					{
						this.walk_st = 1;
						this.EA_Base.clearAnim("run0", -1000, null);
						this.VTentaclePose(NelNBoss_Nusi.TENPOSE.running_0, 1f, -1);
						this.EA_Base.fine_intv = 0f;
						this.VTentacleFineIntv(0f);
						this.EA_Base.timescale = 1f;
						this.VTentacleTS(1.4f);
						this.playSndPos("nusi_bigrun_prep_0", 2);
						this.Anm.setPose("running", -1);
						this.Phy.killSpeedForce(true, true, true, false, false);
					}
					this.ATentacleL[0].EA.NestTarget.MapShiftMul(base.mpf_is_right * 1.2f, 1.4f, this.TS * 0.06f);
					this.ATentacleL[1].EA.NestTarget.MapShiftMul(-base.mpf_is_right * 1.2f, 1.2f, this.TS * 0.06f);
					this.ATentacleL[2].EA.NestTarget.MapShiftMul(base.mpf_is_right * 0.03f, -1.5f, this.TS * 0.06f);
					this.Phy.addTranslateStack(0f, (this.FirstPos.y + -3f - this.Phy.move_depert_tstack_y) * 0.04f * this.TS);
				}
				if (Tk.Progress(ref this.t, 150, true))
				{
					this.walk_st = 0;
					this.walk_time = 0f;
					this.EA_Base.clearAnim("run1", -1000, null);
					this.EA_Base.addListenerEvent(new AnimationState.TrackEntryEventDelegate(this.fnEAEventBigRun));
					this.VTentaclePose(NelNBoss_Nusi.TENPOSE.running_1, 1f, -1);
					this.ma_flower = NelNBoss_Nusi.MA_FLW.RUN;
					this.faint_stun_lock = true;
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				float num;
				float num2;
				if (this.walk_st == 0)
				{
					num = 0.33f;
					num2 = 2f;
				}
				else
				{
					if (this.walk_time > 0f)
					{
						this.walk_time -= this.TS;
					}
					num = X.NIL(1f, (this.walk_st == 1) ? 0.14f : 0.5f, this.walk_time, (float)((this.walk_st == 1) ? 20 : 12));
					num2 = num * num * 1.8f;
				}
				this.EA_Base.timescale = num2;
				this.VTentacleTS(num2);
				float num3 = this.FirstPos.x;
				if (base.mpf_is_right < 0f)
				{
					if (this.Summoner != null)
					{
						M2LpSummon lp = this.Summoner.Lp;
						num3 = (float)lp.mapx + ((float)(lp.mapx + lp.mapw) - num3) - 2.1f;
					}
					else
					{
						num3 = this.FirstPos.x - 13f;
					}
				}
				if (X.Abs(base.x - num3) < 0.51f * num || this.t >= 300f)
				{
					this.walk_st = 0;
					this.Phy.addTranslateStack(num3 - this.Phy.move_depert_tstack_x, this.FirstPos.y - this.Phy.move_depert_tstack_y);
					Tk.prog = PROG.PROG3;
					this.t = 0f;
					this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
					this.Anm.setPose("running_end_0", -1);
					this.EA_Base.clearAnim("run2", -1000, null);
					this.EA_Base.timescale = 1f;
					this.VTentaclePose(NelNBoss_Nusi.TENPOSE.running2stand, 1f, -1);
					this.playSndPos("nusi_bigrun_prep_0", 2);
					this.can_hold_tackle = false;
					this.ma_flower = NelNBoss_Nusi.MA_FLW.RUN2STATIC;
				}
				else
				{
					this.setWalkXSpeed(num * (float)X.MPF(base.x < num3) * 0.17f, true, false);
				}
			}
			if (Tk.prog == PROG.PROG3)
			{
				this.Phy.addTranslateStack(0f, (this.FirstPos.y - this.Phy.move_depert_tstack_y) * 0.04f * this.TS);
				if (this.t >= 32f)
				{
					if (this.walk_st == 0)
					{
						this.faint_stun_lock = false;
						this.walk_st = 1;
						this.Anm.setPose("running_end_1", -1);
						this.setAim((base.mpf_is_right < 0f) ? AIM.R : AIM.L, true);
						this.EA_Base.clearAnim("run3", -1000, null);
						this.EA_Base.fine_intv = 2f;
						this.VTentaclePose(NelNBoss_Nusi.TENPOSE.running2stand, 1f, -1);
					}
					for (int k = 0; k < 3; k++)
					{
						this.ATentacleL[k].walkShiftPosToDefault(0.11f * this.TS);
					}
				}
				if (Tk.Progress(ref this.t, 150, true))
				{
					Tk.after_delay = 10f;
					this.Nai.addTypeLock(NAI.TYPE.WARP, 2400f);
					this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
					this.Anm.setPose("close2stand", -1);
					this.EA_Base.clearAnim("main0", -1000, null);
					this.EA_Base.fine_intv = 3f;
					this.VTentacleFineIntv(5f);
					this.AddFreezeToTentacle(2, 90f, 180f);
					if (this.danger_lv >= 1f)
					{
						this.Nai.AddF(NAI.FLAG.POWERED, 180f);
					}
					return false;
				}
			}
			return true;
		}

		public void fnEAEventBigRun(TrackEntry trackEntry, Event e)
		{
			if (base.destructed)
			{
				return;
			}
			if (e.Data.Name == "attack" && this.Nai.isFrontType(NAI.TYPE.WARP, PROG.ACTIVE) && e.Int > 0)
			{
				this.walk_st++;
				this.walk_time = (float)((this.walk_st == 1) ? 20 : X.Mx(2, 12 - this.walk_st));
				float num = base.y;
				float num2;
				AIM aim;
				if (e.Int == 1)
				{
					num2 = base.x + base.mpf_is_right * 2.2f;
					aim = AIM.B;
				}
				else
				{
					num2 = base.x + base.mpf_is_right * 3.4f;
					aim = AIM.T;
					num = base.y - 3f;
				}
				M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc((int)num2, (int)base.y, aim);
				if (sideBcc != null)
				{
					float num3 = sideBcc.slopeBottomY(num2);
					base.PtcVar("cx", (double)num2).PtcVar("cy", (double)num3).PtcVar("ay", (double)CAim._YD(aim, 1))
						.PtcST("nusi_bigrun_bump", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					if (aim == AIM.T)
					{
						num = num3 + 4f;
					}
				}
				this.playSndPos("nusi_tentacle_prepare", 1);
				this.playSndPos("nusi_leaf_laugh", 1);
				if (this.walk_st == 1)
				{
					MagicItem magicItem = base.tackleInit(this.AtkBigRun, this.TkiBigRun, MGHIT.AUTO);
					magicItem.sx = 1.2f * base.mpf_is_right;
					magicItem.sy = 4f;
					magicItem.dx = 0f;
					magicItem.dy = this.TkiBigRun.dify_map;
					magicItem.sz = -this.sizey;
					magicItem.Ray.shape = RAYSHAPE.RECT;
					base.throw_ray = false;
				}
				base.nM2D.MIST.addMistGenerator(this.MkBigRun, this.MkBigRun.calcAmount(300, 1.4f), (int)base.x, (int)num, false);
				for (int i = 0; i < 2; i++)
				{
					this.AEnTentacle[i].bigRunColliderWalk();
				}
			}
		}

		public override bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			if (base.destructed || !this.is_alive)
			{
				return false;
			}
			if (HitMv != null && Atk == this.AtkCharge && this.Nai.isFrontType(NAI.TYPE.MAG, PROG.ACTIVE) && this.Nai.getCurTicket().prog == PROG.PROG2 && HitMv.Mv is PR)
			{
				this.walk_st = 1;
			}
			return base.initPublishAtk(Mg, Atk, hittype, HitMv);
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			if (this.Nai == null || !this.is_alive)
			{
				return false;
			}
			if (Mg.kind != MGKIND.TACKLE)
			{
				return false;
			}
			if (!this.canAbsorbContinue())
			{
				return false;
			}
			if (Mg.Atk0 == this.AtkCharge)
			{
				return this.Mp.floort < this.walk_time;
			}
			return this.can_hold_tackle;
		}

		public override float getMpDesireRatio(int add_mp)
		{
			if (base.isStunned())
			{
				return -1f;
			}
			if (!this.isFaintCounterExecuting(false))
			{
				return 0.88f;
			}
			return 0.2f;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			if (this.isStunBigDamage())
			{
				base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
			}
			if (this.faint_stun_lock)
			{
				base.addF(NelEnemy.FLAG.DMG_EFFECT_SHIELD);
			}
			int num = base.applyDamage(Atk, force);
			if (this.state == NelEnemy.STATE.STAND && !this.faint_stun_lock && this.depert_stun_hp >= 0 && this.hp <= this.depert_stun_hp)
			{
				this.initFaintStun();
				base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
			}
			else if (num > 0)
			{
				if (Atk != null && Atk.Caster is PR)
				{
					this.faintCounterExecuteChecking();
				}
				if (base.hasF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL))
				{
					if (this.SKIP_STUN && this.is_alive && this.isStunBigDamage())
					{
						this.hp = X.Mx(1, (int)this.walk_time);
					}
					this.ma_flower = NelNBoss_Nusi.MA_FLW.DAMAGE_IN_STUN;
					if (this.is_alive)
					{
						base.PtcST("nusi_damage_in_stun", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
				}
			}
			if (this.isBurstForFaint(Atk))
			{
				this.initBurstStunPhase(Atk.Caster as PR);
			}
			if (this.Nai.isFrontType(NAI.TYPE.MAG, PROG.ACTIVE))
			{
				this.Ser.Cure(SER.TIRED);
			}
			return num;
		}

		public override int applyHpDamage(int val, ref int mpdmg, bool force, NelAttackInfo Atk)
		{
			if (val > 0 && Atk != null && Atk.Caster is PR)
			{
				this.faintCounterExecuteChecking();
			}
			return this.applyHpDamage(val, force, Atk);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			float num = 1f;
			if (this.isStunBigDamage())
			{
				num = 2f;
			}
			if (this.faint_stun_lock)
			{
				num = 0.33f;
			}
			if (Atk == this.AtkShotGranade || Atk == this.AtkShotMist)
			{
				num *= 3.3f;
			}
			return num * base.applyHpDamageRatio(Atk);
		}

		public bool isBurstForFaint(NelAttackInfo Atk)
		{
			return this.isFaintCounterExecuting(true) && Atk.PublishMagic != null && Atk.PublishMagic.kind == MGKIND.PR_BURST;
		}

		public override void checkTiredTime(ref int t0, NelAttackInfo Atk)
		{
			if (this.isBurstForFaint(Atk))
			{
				t0 = X.Mn(t0, 15);
			}
			base.checkTiredTime(ref t0, Atk);
			t0 = X.Mn(t0, 40);
		}

		public override int applyMpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			return base.applyMpDamage((int)(this.isStunBigDamage() ? ((float)val * 3.5f + 32f) : ((float)val * 1.5f)), force, Atk);
		}

		public override float mp_split_reduce_ratio
		{
			get
			{
				if (!this.isStunBigDamage())
				{
					return base.mp_split_reduce_ratio;
				}
				return 0f;
			}
		}

		private void initTentacleEAPos()
		{
			if (this.ATentacleL == null)
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				this.initTentacleEAPos(i);
			}
		}

		private void initTentacleEAPos(int v_id)
		{
			if (this.ATentacleL == null)
			{
				return;
			}
			NelNBoss_Nusi.TentacleLink tentacleLink = this.ATentacleL[v_id];
			CCNestItem nestTarget = tentacleLink.EA.NestTarget;
			if (v_id != 0)
			{
				if (v_id != 1)
				{
					nestTarget.MapShift(-0.3f * base.mpf_is_right, -6.9f);
				}
				else
				{
					nestTarget.MapShift(1f * base.mpf_is_right, 5.8f);
				}
			}
			else
			{
				nestTarget.MapShift(1.7f * base.mpf_is_right, 4.5f);
			}
			tentacleLink.FirstShiftPos = new Vector2(nestTarget.shiftx, nestTarget.shifty);
		}

		public void clearTentacleLink()
		{
			for (int i = 0; i < 3; i++)
			{
				this.ATentacleL[i].Unlink();
			}
		}

		public void VTentaclePose(NelNBoss_Nusi.TENPOSE pose, float timescale = 1f, int order_front = -1)
		{
			for (int i = 0; i < 3; i++)
			{
				NelNBoss_Nusi.TentacleLink tentacleLink = this.ATentacleL[i];
				tentacleLink.EAPose(pose, false, order_front);
				tentacleLink.EA.timescale = timescale;
			}
		}

		public void VTentacleTS(float timescale = 1f)
		{
			for (int i = 0; i < 3; i++)
			{
				this.ATentacleL[i].EA.timescale = timescale;
			}
		}

		public void VTentacleFineIntv(float fine_intv = 5f)
		{
			for (int i = 0; i < 3; i++)
			{
				this.ATentacleL[i].EA.fine_intv = fine_intv;
			}
		}

		public void initFaintStun()
		{
			this.changeState(NelEnemy.STATE.DAMAGE);
			if (this.depert_stun_hp < 0)
			{
				this.depert_stun_hp = (int)X.Mx((float)this.hp - (float)this.maxhp * this.hp_level_attacking, 1f);
			}
			this.sink_ratio = 0.25f;
			this.cureEatenAll();
			this.AddFreezeToTentacle(2, 60f, 180f);
		}

		public override bool runDamageSmall()
		{
			if (base.destructed)
			{
				return false;
			}
			this.Nai.fineTargetPosition(this.TS);
			this.cureEatenAll();
			if (this.walk_st > -100 && this.walk_st < 100)
			{
				if (this.EnCage.anmtype != NelNBoss_Nusi.MA_CAGE.APPEAL && this.walk_st >= 2 && this.walk_st < 100)
				{
					this.walk_st = -100;
					this.EnCage.anmtype = NelNBoss_Nusi.MA_CAGE.HIDDEN;
					base.PtcST("nusi_cure_success", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.SpSetPose("faint2laugh", -1, null, false);
					this.playSndPos("nusivo_laugh", 1);
					this.burst_counter_failed++;
					this.t = 1f;
					int hp = this.hp;
					this.hp = (int)X.Mn((float)this.hp + (float)this.maxhp * this.hp_level_attacking * X.NI(0.5f, 0.125f, X.ZLINE((float)this.burst_counter_failed, 6f)), (float)this.maxhp);
					this.Mp.DmgCntCon.Make(this, this.hp - hp, 0, M2DmgCounterItem.DC.ABSORBED, false);
					base.addF(NelEnemy.FLAG.FINE_HPMP_BAR);
					this.VTentaclePose(NelNBoss_Nusi.TENPOSE.laugh, 1f, -1);
					return true;
				}
				if (this.t == 0f)
				{
					float num = X.NIXP(4f, 6f) * (float)X.MPFXP();
					float num2 = X.NIXP(20f, 25f);
					if (X.XORSP() < 0.5f)
					{
						this.TeCon.setQuakeSinH(num, 44, num2, 0f, 0);
					}
					this.angleAnimTo(0f, 25f, true);
					this.walk_st = 0;
					this.clearTentacleLink();
					this.VTentaclePose((this.burst_counter_success == 0) ? NelNBoss_Nusi.TENPOSE.laugh : NelNBoss_Nusi.TENPOSE.stun, 1f, -1);
					this.SpSetPose((this.burst_counter_success == 0) ? "close" : "faint", -1, null, false);
					if (this.burst_counter_success > 0)
					{
						this.playSndPos("nusivo_faintstun", 1);
					}
					float num3 = -1.5707964f;
					for (int i = 0; i < 5; i++)
					{
						this.AEnTentacle[i].initFaintTentacle(num3);
						num3 += (2f + X.XORSPS() * 0.03f) / 5f * 6.2831855f;
					}
				}
				bool flag = false;
				if (this.burst_counter_success == 0)
				{
					if (this.walk_st == 0 && this.t >= 20f)
					{
						this.walk_st = 2;
						this.t = 1f;
					}
					if (this.walk_st >= 2)
					{
						flag = true;
					}
					if (this.Anm.poseIs("close", true) && this.Anm.isAnimEnd())
					{
						this.SpSetPose("laugh", -1, null, false);
						this.playSndPos("nusi_leaf_laugh", 1);
						this.playSndPos("nusivo_laugh", 1);
						this.ma_flower = NelNBoss_Nusi.MA_FLW.LAUGH;
					}
				}
				else
				{
					if (this.walk_st == 0)
					{
						this.ma_flower = NelNBoss_Nusi.MA_FLW.STUN;
						if (this.t >= 20f)
						{
							this.walk_st = 1;
							this.t = 1f;
						}
					}
					if (this.walk_st == 1 && this.t >= 45f)
					{
						this.EnCage.anmtype = NelNBoss_Nusi.MA_CAGE.APPEAL;
						this.walk_st = 2;
						this.t = 1f;
					}
					if (this.walk_st >= 2)
					{
						flag = this.t >= 50f;
					}
				}
				if (this.mp < this.maxmp && flag)
				{
					this.mp = X.Mn(this.mp + 5, this.maxmp);
					base.addF(NelEnemy.FLAG.FINE_HPMP_BAR);
				}
				if (this.walk_st == 2)
				{
					this.walk_st = 3;
				}
			}
			else if (this.walk_st >= 100)
			{
				if (this.walk_st == 100)
				{
					this.walk_st = 101;
					this.absorb_tap_count++;
					EV.TutoBox.RemText(true, true);
					this.SpSetPose((this.burst_counter_success == 0) ? "grawl_2" : "laugh", -1, null, false);
					this.ma_flower = ((this.burst_counter_success == 0) ? NelNBoss_Nusi.MA_FLW.GRAWL : NelNBoss_Nusi.MA_FLW.LAUGH);
					base.PtcST("nusivo_fainttentacle_grawl", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					PostEffect.IT.setSlowFading(30f, 10f, 0.25f, -20);
					PostEffect.IT.addTimeFixedEffect(PostEffect.IT.setPEfadeinoutZSINV(POSTM.JAMMING, 50f, 14f, 1f, -40), 1f);
					PostEffect.IT.addTimeFixedEffect(PostEffect.IT.setPEfadeinoutZSINV(POSTM.ENEMY_OVERDRIVE_APPEAR, 90f, 14f, 1f, -70), 1f);
					this.t = 1f;
					this.EnCage.anmtype = NelNBoss_Nusi.MA_CAGE.HIDDEN;
					this.VTentaclePose(NelNBoss_Nusi.TENPOSE.laugh, 1f, 0);
					this.cureMpMax();
				}
				if (this.walk_st == 101 && this.t >= 15f)
				{
					this.t = 1f;
					this.walk_st = 102;
				}
				if (this.walk_st == 102 && this.t >= 350f)
				{
					this.burst_counter_failed++;
					return false;
				}
				if (this.walk_st == 103 && this.t >= 40f)
				{
					this.walk_st = 104;
					this.EA_FaintTentacle.alpha = 1f;
					this.EA_FaintTentacle.timescale = 1f;
					this.EA_FaintTentacle.clearAnim("drill_prepare", -1000, null);
					this.EAFT_DrillAnim = this.EA_FaintTentacle.addAnim(1, "drill_anim", 0, 0f, 1f);
					base.PtcVar("x", (double)base.AimPr.x).PtcVar("y", (double)base.AimPr.y).PtcST("nusi_fainttentacle_drill_prepare", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_D);
				}
				if (this.walk_st == 104 && this.EAFT_DrillAnim != null)
				{
					this.EAFT_DrillAnim.TimeScale = X.NI(0.3f, 2f, X.ZPOW(this.t - 10f, 30f));
				}
				if (this.walk_st >= 110 && this.walk_st < 220)
				{
					if (this.t >= 100f)
					{
						PR pr = base.AimPr as PR;
						if (pr != null)
						{
							if (pr.isAbsorbState() && this.walk_st < 200)
							{
								EnAttackInfo enAttackInfo = ((X.XORSP() < 0.83f) ? this.AtkFaintAbsorb0 : this.AtkFaintAbsorb1);
								base.applyAbsorbDamageTo(pr, enAttackInfo, X.XORSP() < 0.68f, false, X.XORSP() < 0.2f, 0f, false, (this.walk_st == 110) ? "torture_drilln" : "torture_drilln_2", false, true);
								pr.PtcST("nusi_fainttentacle_hit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
								pr.SpSetPose("ceiltrap_fatal", -1, null, false);
								this.EAFT_DrillAnim.TimeScale = X.NIXP(1.2f, 1.8f);
								if (enAttackInfo == this.AtkFaintAbsorb1)
								{
									base.M2D.Cam.setQuake(X.NIXP(3f, 8f), 10, 1f, 0);
								}
								if (X.XORSP() < 0.24f)
								{
									pr.getAnimator().randomizeFrame();
									this.EA_FaintTentacle.randomizeFrame();
								}
								if (X.XORSP() < 0.45f)
								{
									pr.PtcST("nusi_fainttentacle_absorb", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
								}
								if (X.XORSP() < 0.09f)
								{
									pr.playSndPos("insect_rape", 1);
									pr.TeCon.setQuakeSinV(8f, 20, X.NIXP(14f, 23f), 0f, 0);
									base.absorbEffect();
								}
								if (X.XORSP() < 0.35f)
								{
									pr.playSndPos("absorb_kiss_s", 1);
								}
								if (X.XORSP() < 0.55f)
								{
									this.Mp.DropCon.setLoveJuice(pr, 8, uint.MaxValue, 0f, false);
								}
								if (X.XORSP() < 0.3f)
								{
									this.Mp.DropCon.setBlood(pr, 6, MTR.col_blood, 0f, true);
								}
							}
							else
							{
								this.Nai.fine_target_pos_lock = 60f;
								if (this.walk_st < 210)
								{
									this.walk_st = 210;
								}
								else
								{
									this.walk_st++;
								}
							}
						}
						this.EA_FaintTentacle.rotationR = X.XORSPS() * 0.09f * 3.1415927f;
						this.walk_time = X.NIXP(15f, 23f);
						this.t = 100f - X.NIXP(12f, 16f);
						if (this.walk_st < 140 && X.XORSP() < 0.1f)
						{
							this.walk_st = 110 + X.xors(2);
						}
					}
					if (this.walk_st < 200 && X.XORSP() < 0.012f)
					{
						this.playSndPos("kuchu", 1);
					}
				}
				else if (this.walk_st >= 220)
				{
					this.Nai.fine_target_pos_lock = 60f;
					if (this.walk_st == 220)
					{
						this.t = 0f;
						this.walk_st++;
						this.SpSetPose("open2stand", -1, null, false);
						this.playSndPos("nusi_leaf_laugh", 1);
						this.ma_flower = NelNBoss_Nusi.MA_FLW.LAUGH;
						this.VTentaclePose(NelNBoss_Nusi.TENPOSE.laugh2stand, 1f, -1);
					}
					if (this.walk_st == 221 && this.t >= 120f)
					{
						this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
						this.EA_FaintTentacle.alpha = 0f;
						return false;
					}
				}
			}
			else
			{
				if (this.t >= 65f && this.walk_st == -100)
				{
					this.walk_st--;
					this.VTentaclePose(NelNBoss_Nusi.TENPOSE.laugh2stand, 1f, -1);
				}
				if (this.t >= 140f)
				{
					this.AddFreezeToTentacle(2, 100f, 180f);
					return false;
				}
			}
			if (this.EA_FaintTentacle.alpha > 0f)
			{
				PR pr2 = base.AimPr as PR;
				if (pr2 != null)
				{
					float num4 = 0f;
					float num5;
					if (this.walk_st == 221)
					{
						num5 = 2f + 8f * X.ZPOW(this.t, 90f);
					}
					else if (this.walk_st < 110)
					{
						num5 = 3f * ((this.walk_st >= 200) ? (0.23f * (1f + X.COSI(this.Mp.floort, 65f))) : (1f - X.ZPOW(this.t, 44f)));
					}
					else
					{
						Vector2 vector = pr2.getAnimator().getAnimator().getShiftTe() * this.Mp.rCLEN * 0.7f;
						num4 += vector.x;
						num5 = vector.y - 0.267f + 0.01f * X.COSI(this.t, this.walk_time);
					}
					Vector2 vector2 = pr2.getHipPos();
					this.EA_FaintTentacle.NestTarget.MapShiftAbs(this, vector2.x - pr2.drawx * this.Mp.rCLEN + this.Nai.target_x + num4, vector2.y - pr2.drawy * this.Mp.rCLEN + this.Nai.target_y + num5);
				}
			}
			return true;
		}

		public void faintCounterExecuteChecking()
		{
			if (this.state == NelEnemy.STATE.DAMAGE && this.walk_st >= 3 && this.walk_st < 100)
			{
				this.walk_st = 100;
			}
		}

		public override void setAbsorbHitPos(NelAttackInfo Atk, PR Pr)
		{
			Atk.HitXy(Pr.x, Pr.y + 0.04f, true);
		}

		public void tentacleFaintAbsorbProgress()
		{
			if (this.state == NelEnemy.STATE.DAMAGE && this.walk_st == 102)
			{
				this.walk_st = 103;
				this.t = 1f;
			}
		}

		public void triggerFaintTentacleAnimComplete(TrackEntry trackEntry)
		{
			if (base.destructed)
			{
				return;
			}
			if (trackEntry.Animation.Name == "drill_prepare")
			{
				if (this.state == NelEnemy.STATE.DAMAGE && this.walk_st == 104)
				{
					this.EA_FaintTentacle.clearAnim("drill_attack", -1000, null);
					this.EAFT_DrillAnim = this.EA_FaintTentacle.addAnim(1, "drill_anim", -1000, 0f, 1f);
					this.EAFT_DrillAnim.TimeScale = 2f;
					this.walk_st = 110;
					this.walk_time = 4f;
					this.t = 100f;
					return;
				}
				this.EA_FaintTentacle.timescale = 0f;
				this.EA_FaintTentacle.alpha = 0f;
			}
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			for (int i = 0; i < 3; i++)
			{
				A.Add("torture_drilln_2");
				A.Add("torture_drilln");
			}
			Aattr.Add(MGATTR.NORMAL);
			Aattr.Add(MGATTR.POISON);
			Aattr.Add(MGATTR.STAB);
			Aattr.Add(MGATTR.FIRE);
		}

		public bool isMainBgm()
		{
			return BGM.frontBGMIs("BGM_battle_nusi", "BGM_battle_nusi");
		}

		public void initBurstStunPhase(PR Attacker)
		{
			if (this.isFaintCounter())
			{
				this.EA_FaintTentacle.alpha = 0f;
				this.Ser.Add(SER.EATEN, 1050, 99, false);
				this.changeState(NelEnemy.STATE.STUN);
				this.playSndPos("nusivo_hit_faintburst", 1);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.DAMAGE;
				this.VTentaclePose(NelNBoss_Nusi.TENPOSE.stun, 1f, -1);
				if (this.isMainBgm())
				{
					BGM.GotoBlock((this.burst_counter_success >= 2) ? "F" : "D", true);
					BGM.setOverrideKey((this.burst_counter_success == 0) ? "mainbattle" : "challenge_1", false);
				}
				this.cureMpMax();
				if (Attacker != null)
				{
					Attacker.Skill.clearManaDrainLock();
				}
			}
		}

		public override bool runStun()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.walk_time = X.Mx((float)((this.burst_counter_success >= 2) ? 0 : (this.hp / 2)), (float)this.hp - (float)this.maxhp * 0.24f);
				this.sink_ratio = 0.25f;
				this.walk_st = 0;
				this.hp = X.Mx(this.hp, (this.burst_counter_success >= 3) ? 400 : 650);
				base.addF(NelEnemy.FLAG.FINE_HPMP_BAR);
				for (int i = 0; i < 5; i++)
				{
					this.AEnTentacle[i].initStun();
				}
			}
			if (this.walk_st == 0 && this.t >= 110f)
			{
				this.Ser.Add(SER.EATEN, 1050, 99, false);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.STUN;
				this.Anm.setPose("stun", -1);
				base.PtcVar("sizex", (double)(this.sizex * base.CLENM * 0.8f)).PtcVar("sizey", (double)(this.sizey * base.CLENM * 0.8f)).PtcVar("maxt", 1050.0)
					.PtcST("en_stunned", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				this.Anm.setPose("faint2stun", -1);
				this.walk_st = 1;
				this.t = 0f;
			}
			if (this.walk_st == 1 && (this.t >= 1575f || !this.Ser.has(SER.EATEN) || (this.walk_time >= 1f && (float)this.hp <= this.walk_time)))
			{
				this.walk_st = 2;
				this.Ser.Cure(SER.EATEN);
				this.t = 0f;
				this.burst_counter_success++;
				this.depert_stun_hp = (int)X.Mx((float)this.hp - (float)this.maxhp * this.hp_level_attacking, 1f);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.LAUGH;
				this.EnCage.anmtype = NelNBoss_Nusi.MA_CAGE.HIDDEN;
				this.VTentaclePose(NelNBoss_Nusi.TENPOSE.stun2laugh, 1f, -1);
				this.cureMpMax();
				this.PtcHld.killPtc("en_stunned", false);
				this.Anm.setPose("shot2stand", -1);
				if (this.isMainBgm())
				{
					BGM.setOverrideKey("mainbattle", false);
				}
			}
			if (this.walk_st == 2 && this.t >= 150f)
			{
				this.cureEatenAll();
				this.ma_flower = NelNBoss_Nusi.MA_FLW.STATIC;
				this.burst_counter_failed = 0;
				this.VTentaclePose(NelNBoss_Nusi.TENPOSE.laugh2stand, 1f, -1);
				this.AddFreezeToTentacle(2, (float)((this.burst_counter_success == 1) ? 140 : 100), 180f);
				if (this.burst_counter_success == 2)
				{
					this.Nai.AddF(NAI.FLAG.BOTHERED, 180f);
				}
				this.aiphase2_lock = 0;
				return false;
			}
			return true;
		}

		private void cureEatenAll()
		{
			this.Ser.Cure(SER.EATEN);
			for (int i = 0; i < 5; i++)
			{
				this.AEnTentacle[i].getSer().Cure(SER.EATEN);
			}
		}

		public override bool runDie()
		{
			if (this.walk_st == 0)
			{
				this.t = 0f;
				this.faint_stun_lock = false;
				this.walk_st = 1;
				PostEffect.IT.setSlow(60f, 0f, 0);
				base.M2D.Cam.assignBaseMover(this, 0);
				PostEffect.IT.addTimeFixedEffect(this.Anm, 1f);
				this.Anm.setPose("death_0", -1);
				this.defineParticlePreVariable();
				this.PtcHld.PtcSTTimeFixed("boss_killed_explode", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
				if (this.isMainBgm())
				{
					BGM.setOverrideKey("battle_end", false);
				}
				this.EnCage.anmtype = NelNBoss_Nusi.MA_CAGE.APPEAL_OPEN;
				this.EnCage.throw_ray = true;
				for (int i = this.AEnTentacle.Length - 1; i >= 0; i--)
				{
					this.AEnTentacle[i].initBossDeathPhase();
				}
				this.killFollowers();
				return true;
			}
			if (this.walk_st == 1)
			{
				this.t = 1000f;
				this.walk_st = 10;
				base.PtcST("nusi_die", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.ma_flower = NelNBoss_Nusi.MA_FLW.DIE;
				this.clearTentacleLink();
				this.Anm.setPose("death_1", -1);
				this.EA_Base.clearAnim("stun", -1000, null);
				this.VTentaclePose(NelNBoss_Nusi.TENPOSE.stun, 1f, -1);
			}
			if (X.BTW(10f, (float)this.walk_st, 20f) && this.t >= 100f)
			{
				bool flag = this.walk_st == 10;
				this.t = (float)(100 - (flag ? 40 : 20));
				if (this.walk_st == 11)
				{
					base.PtcVar("intv", 20.0).PtcST("nusi_die_explode", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				this.Anm.setDmgBlink(MGATTR.NORMAL, 8f, X.NIXP(0.4f, 0.8f), 0);
				for (int j = -1; j < 3; j++)
				{
					EnemyAnimatorSpine enemyAnimatorSpine = ((j == -1) ? this.EA_Base : this.ATentacleL[j].EA);
					if (flag)
					{
						TrackEntry trackEntry = enemyAnimatorSpine.addAnim(1, "dead", -1000, 0f, 1f);
						trackEntry.TimeScale = 0f;
					}
					else
					{
						TrackEntry trackEntry = enemyAnimatorSpine.getBaseAnimator().getTrack(1);
						if (trackEntry == null)
						{
							trackEntry = enemyAnimatorSpine.addAnim(1, "dead", -1000, 0f, 1f);
							trackEntry.TimeScale = 0f;
						}
						trackEntry.TrackTime = X.Mn(trackEntry.AnimationEnd, (float)(this.walk_st - 10) / 2f);
						if (j >= 0)
						{
							int num = 7 - (this.walk_st - 11);
							Vector3 vector;
							if (X.BTW(0f, (float)num, 8f) && enemyAnimatorSpine.getBoneMapPos("une" + num.ToString(), out vector))
							{
								base.PtcVar("cx", (double)vector.x).PtcVar("cy", (double)vector.y).PtcST("nusi_die_tentacle_explode", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							}
						}
					}
				}
				this.walk_st++;
			}
			if (this.walk_st == 20 && this.t >= 130f)
			{
				base.M2D.Cam.assignBaseMover(this.Mp.Pr, 0);
				this.Mp.PtcSTsetVar("cx", (double)base.x).PtcSTsetVar("cy", (double)base.y).PtcSTsetVar("ax", (double)base.mpf_is_right)
					.PtcST("boss_explode", null, PTCThread.StFollow.NO_FOLLOW);
				if (this.isMainBgm())
				{
					BGM.GotoBlock("I", false);
				}
				return base.runDie();
			}
			return true;
		}

		private void killFollowers()
		{
			if (this.Summoner != null)
			{
				this.Summoner.countActiveEnemy(delegate(NelEnemy _En)
				{
					SummonedInfo summonedInfo = this.Summoner.getSummonedInfo(_En);
					if (summonedInfo != null && this.Summoner.is_follower(summonedInfo.K, "_FOLLOW_NUSI"))
					{
						_En.changeStateToDie();
					}
					return false;
				}, false);
			}
		}

		public float tentacle_after_delay
		{
			get
			{
				if (this.burst_counter_success < 2)
				{
					return 0f;
				}
				return X.NIXP(220f, 340f);
			}
		}

		public override bool readPtcScript(PTCThread rER)
		{
			if (base.destructed || this.PtcHld == null)
			{
				return rER.quitReading();
			}
			string cmd = rER.cmd;
			if (cmd != null)
			{
				if (cmd == "%DRILL_ANGLE")
				{
					rER.Def("agR", this.EA_FaintTentacle.rotationR - 1.5707964f);
					return true;
				}
				if (cmd == "%SHOT_ANGLE")
				{
					rER.Def("agR", this.shotagR);
					return true;
				}
				if (cmd == "%SHOT_DEP")
				{
					rER.Def("sx", this.ShotDep.x);
					rER.Def("sy", X.NI(this.ShotDep.y, this.Nai.target_y, 0.5f));
					return true;
				}
			}
			return base.readPtcScript(rER);
		}

		public float shotagR
		{
			get
			{
				return this.Anm.rotationR + ((base.mpf_is_right > 0f) ? 0f : 3.1415927f);
			}
			set
			{
				this.Anm.rotationR = value - ((base.mpf_is_right > 0f) ? 0f : 3.1415927f);
			}
		}

		public Vector2 ShotExplodePos
		{
			get
			{
				if (base.destructed)
				{
					return new Vector2(base.x, base.y);
				}
				Vector2 vector = X.ROTV2e(new Vector2(115f * this.Mp.rCLENB, -14f * this.Mp.rCLENB * base.mpf_is_right), this.shotagR);
				return new Vector2(base.x + vector.x + this.Anm.after_offset_x * this.Mp.rCLENB, base.y - vector.y + this.Anm.after_offset_y * this.Mp.rCLENB);
			}
		}

		public override bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			if (!base.destructed)
			{
				if (follow == PTCThread.StFollow.FOLLOW_S)
				{
					V = this.ShotExplodePos;
					return true;
				}
				if (follow == PTCThread.StFollow.FOLLOW_D)
				{
					if (this.EA_FaintTentacle.getBoneMapPos("drill_top", out V))
					{
						return true;
					}
				}
			}
			return base.getEffectReposition(St, follow, fcnt, out V);
		}

		public void FnBaseAnimComplete(TrackEntry Entry)
		{
			if (Entry.Animation.Name == "call2main")
			{
				this.EA_Base.clearAnim("main0", -1000, null);
			}
		}

		public bool isFaintPreparing(bool changeable = false)
		{
			return this.state == NelEnemy.STATE.DAMAGE && (changeable ? 1 : 0) <= this.walk_st && this.walk_st < 100;
		}

		public bool isFaintCured(bool changeable = false)
		{
			return this.state == NelEnemy.STATE.DAMAGE && -200 < this.walk_st && this.walk_st <= -100;
		}

		public bool isFaintTargetEscaped()
		{
			return this.state != NelEnemy.STATE.DAMAGE || this.walk_st >= 200;
		}

		public bool isFaintCounter()
		{
			return this.state == NelEnemy.STATE.DAMAGE && this.walk_st > -100;
		}

		public bool isFaintCounterExecuting(bool burst_hitable = false)
		{
			return this.state == NelEnemy.STATE.DAMAGE && this.walk_st >= (burst_hitable ? 102 : 100) && this.walk_st < (burst_hitable ? 222 : 200);
		}

		public bool isPlayerCounterUnsuccess()
		{
			return this.burst_counter_success == 0;
		}

		public bool isStunBigDamage()
		{
			return base.isStunned() && this.walk_st == 1;
		}

		public override bool export_other_mover_right(M2Mover Other)
		{
			return base.mpf_is_right > 0f;
		}

		public override bool showFlashEatenEffect(bool for_effect = false)
		{
			return (!this.isFaintCounter() && !this.isFaintCounterExecuting(false) && !base.isStunned()) || !for_effect;
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if ((Ray.hittype & HITTYPE.GUARD_IGNORE) != HITTYPE.NONE && this.isFaintPreparing(false))
			{
				return RAYHIT.NONE;
			}
			return base.can_hit(Ray);
		}

		private bool SKIP_STUN;

		public const float tentacle_base_applydmg_hp_ratio = 0.25f;

		public const float tentacle_base_applydmg_mp_ratio = 0.17f;

		public const float cage_base_applydmg_hp_ratio = 0.25f;

		public const float cage_base_applydmg_mp_ratio = 0.35f;

		public const int cage_ixia_absorb_time = 600;

		public const int burst_stun_time = 1050;

		public const float stun_damage_ratio = 2f;

		public const float stain_ratio = 1.6f;

		public const float stain_ratio_struggle = 3f;

		public const float tentacle_anim_fine_intv_def = 5f;

		public const float hp_level_attacking_1 = 0.165f;

		public const float hp_level_attacking_2 = 0.155f;

		private int aiphase2_lock;

		private int aiphase2_lock_max;

		public const float hp_level_stun = 0.24f;

		public const int tentacle_max = 5;

		public const int visible_tentacle_max = 3;

		public const int considerable_tentacle_max = 2;

		public const float mpdmg_ratio_stun = 3.5f;

		public const float mpdmg_stun_add = 32f;

		public const float mpdmg_ratio_normal = 1.5f;

		private const NAI.TYPE NT_SHOT = NAI.TYPE.PUNCH;

		public const int T_SHOT_PREP_0 = 90;

		public const int T_SHOT_PREP_1 = 22;

		public const int T_SHOT_PREP_2 = 55;

		public const float SHOT_SPEED = 0.34f;

		public const float SHOT_SPEED_PARABOLA = 0.25f;

		public const float SHOT_GRAVITY_SCALE = 0.18f;

		private const float SHOT_RADIUS = 0.22f;

		private const float SHOT_EXPLODE_RADIUS = 0.9f;

		private const int SHOTGRANADE_BURN_W = 5;

		private const float SHOTGRANADE_BURN_T_MIN = 200f;

		private const float SHOTGRANADE_BURN_T_MAX = 250f;

		private const int SHOT_MIST_LAST_TIME = 220;

		private const float SHOT_COUNTER_RATIO = 3.3f;

		private const float SHOT_PARABOLA_RATIO = 0.38f;

		private const NAI.TYPE NT_SPORERAIN = NAI.TYPE.PUNCH_0;

		public const int T_SPORE_PREP_0 = 20;

		public const int T_SPORE_PREP_1 = 50;

		public const int T_SPORE_INTV = 20;

		public const int T_SPORE_AFT_0 = 80;

		public const int T_SPORE_AFTDELAY = 45;

		public const float SPORE_CX_SHIFT = 8.3f;

		public const int SPORE_RANDOM_X = 7;

		public const float SPORE_VY_MIN = 0.02f;

		public const float SPORE_VY_MAX = 0.034f;

		public const float SPORE_SPIKE_RADIUS = 0.6f;

		private const float SPORE_HITLOCK = 40f;

		public const int SPORE_MAX = 9;

		private MagicItem.FnCheckMagicFrom FD_sporeCount;

		private const NAI.TYPE NT_CHARGE = NAI.TYPE.MAG;

		public const int T_CHARGE_PREP_0 = 40;

		public const int T_CHARGE_INTV = 26;

		public const int CHARGE_PREP_COUNT = 8;

		public const int T_CHARGE_PREP_1 = 26;

		public const int T_CHARGE_AFTER = 95;

		public const int T_CHARGE_AFTERDELAY = 120;

		public const int T_CHARGE_LAUGH = 110;

		public const int T_CHARGE_LAUGH_AFTERDELAY = 25;

		private const int CHARGE_MIST_LAST_TIME = 320;

		private const NAI.TYPE NT_CALL_FOLLOWER = NAI.TYPE.MAG_0;

		private int follower_spl_id;

		private int follower_count_cache = -1;

		public const int T_CALLF_PREP_0 = 160;

		public const int T_CALLF_PREP_1 = 200;

		public const int T_CALLF_AFTERDELAY = 200;

		public bool faint_stun_lock_;

		private const float TS_DrillAnim = 2f;

		private Vector2 FirstPos;

		public const float BIGRUN_EXE_RATIO = 0.06f;

		public const NAI.TYPE NT_BIGRUN = NAI.TYPE.WARP;

		public const int T_BIGRUN_PREP_0 = 30;

		public const int T_BIGRUN_PREP_1 = 150;

		public const float BIGRUN_SPEED = 0.17f;

		public const int T_BIGRUN_AFTER = 150;

		public const int T_BIGRUN_AFTERDELAY = 10;

		public const int T_BIGRUN_LOCK = 2400;

		private const int BIGRUN_MIST_LAST_TIME = 300;

		private const int BIGRUN_YSHIFT = -3;

		private const float PHASE2_TENTACLE_AFTER_DELAY_MIN = 220f;

		private const float PHASE2_TENTACLE_AFTER_DELAY_MAX = 340f;

		private const float T_ABSORB_LOCK = 135f;

		public float danger_lv;

		private float time_p1_delay_reduce_ratio;

		private const float P1_DELAY_REDUCE_MAX = 0.89f;

		private const float P1_DELAY_REDUCE_MIN = 0.3f;

		private float time_p2_lock_ratio;

		private const float P2_LOCK_PROB_MAX = 0.42f;

		private const float P2_LOCK_PROB_MIN = 0.1f;

		private float time_p1_lock_prob;

		private const float P1_LOCK1_PROB_MAX = 1f;

		private const float P1_LOCK1_PROB_MIN = 0.4f;

		private float time_p2_reduce_base_time;

		private const float P2_FREEZE_TIME_REDUCE_MAX = 70f;

		private const float P2_FREEZE_TIME_REDUCE_MIN = 10f;

		private const float P2_CALL_AFTER_BIGRUN_DANGER = 1f;

		private float act3_canmove_alloc_ratio;

		private const float ACT3_ALLOC_PROB_MAX = 0.85f;

		private const float ACT3_ALLOC_PROB_MIN = 0.02f;

		private float time_p2_tentacle_afterdelay_ratio;

		private const float P2_TENTACLE_AFTERDELAY_MAX = 1f;

		private const float P2_TENTACLE_AFTERDELAY_MIN = 0.4f;

		private int depert_stun_hp = -1;

		private PxlPose PoseShotBody;

		private PxlSequence SqSpore;

		private PxlSequence SqSporeSpike;

		private M2MvColliderCreatorEnNest CCN;

		private NelNNusiCage EnCage;

		private EnemyAnimatorSpine EA_Base;

		private NelNNusiTentacle[] AEnTentacle;

		private EnemyAnimatorSpine EA_FaintTentacle;

		private CCNestItem NestBaseBody;

		private NelNBoss_Nusi.AIFreeze Freeze;

		public int burst_counter_success;

		public int burst_counter_failed;

		private const int burst_counter_failed_max = 6;

		public int link_start_i;

		private float angleanm_dep_agR;

		private float angleanm_first_agR;

		private float angleanm_t;

		private float angleanm_maxt;

		public int absorb_tap_count;

		private TrackEntry EAFT_DrillAnim;

		private const string follower_splitter_title = "_FOLLOW_NUSI";

		protected EnAttackInfo AtkShotMist = new EnAttackInfo(0.002f, 0.003f)
		{
			hpdmg0 = 11,
			split_mpdmg = 9,
			burst_vx = 0.04f,
			huttobi_ratio = 0.02f,
			attr = MGATTR.NORMAL,
			parryable = true,
			nodamage_time = 0,
			SerDmg = new FlagCounter<SER>(1).Add(SER.SLEEP, 24f)
		};

		protected NelAttackInfo AtkShotGranade = new NelAttackInfo
		{
			hpdmg0 = 25,
			split_mpdmg = 1,
			burst_vx = 0.04f,
			huttobi_ratio = 0.08f,
			attr = MGATTR.FIRE,
			parryable = true,
			nodamage_time = 0,
			SerDmg = new FlagCounter<SER>(1).Add(SER.BURNED, 15f)
		}.Torn(0.01f, 0.04f);

		private NOD.MpConsume McsShot = NOD.getMpConsume("nusi_shot");

		private MistManager.MistKind MKShotMist = NelNMush.MkSleep;

		protected EnAttackInfo AtkSporeSpike = new EnAttackInfo
		{
			hpdmg0 = 11,
			split_mpdmg = 15,
			burst_vx = 0.24f,
			burst_center = 0.14f,
			knockback_len = 0.7f,
			huttobi_ratio = 0.14f,
			parryable = false,
			attr = MGATTR.STAB,
			ndmg = NDMG.MAPDAMAGE,
			Beto = BetoInfo.Blood
		};

		private NOD.MpConsume McsSpore = NOD.getMpConsume("nusi_spore");

		private NOD.MpConsume McsCharge = NOD.getMpConsume("nusi_charge");

		private NOD.TackleInfo TkiCharge = NOD.getTackle("nusi_charge");

		protected NelAttackInfo AtkCharge = new NelAttackInfo
		{
			hpdmg0 = 40,
			mpdmg0 = 60,
			split_mpdmg = 30,
			shield_break_ratio = 400f,
			parryable = false,
			ndmg = NDMG.GRAB_PENETRATE,
			attr = MGATTR.POISON,
			SerDmg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 40f).Add(SER.SLEEP, 100f).Add(SER.CONFUSE, 80f)
				.Add(SER.PARALYSIS, 80f),
			Beto = BetoInfo.Blood.Pow(95, false).Thread(1, true)
		};

		protected static MistManager.MistKind MkCharge = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.CONFUSE, 50f).Add(SER.SLEEP, 50f).Add(SER.PARALYSIS, 50f)
				}
			},
			color0 = C32.d2c(4289528105U),
			color1 = C32.d2c(4282190101U),
			max_influence = 6,
			damage_cooltime = 160
		};

		private NOD.TackleInfo TkiBigRun = NOD.getTackle("nusi_bigrun");

		public EnAttackInfo AtkBigRun = new EnAttackInfo
		{
			hpdmg0 = 11,
			burst_vx = 0.5f,
			burst_vy = -0.3f,
			shield_break_ratio = 5f,
			split_mpdmg = 20,
			huttobi_ratio = 0.6f,
			Beto = BetoInfo.BigBite
		};

		public MistManager.MistKind MkBigRun = new MistManager.MistKind(MistManager.MISTTYPE.POISON)
		{
			AAtk = new MistAttackInfo[]
			{
				new MistAttackInfo(0)
				{
					SerDmg = new FlagCounter<SER>(4).Add(SER.CONFUSE, 85f)
				}
			},
			color0 = C32.d2c(4290278565U),
			color1 = C32.d2c(4288366235U),
			max_influence = 4,
			damage_cooltime = 90
		};

		protected EnAttackInfo AtkFaintAbsorb0 = new EnAttackInfo(0.03f, 0.04f)
		{
			hpdmg0 = 14,
			mpdmg0 = 2,
			split_mpdmg = 4,
			attr = MGATTR.ABSORB_V,
			hit_ptcst_name = "",
			EpDmg = new EpAtk(14, "boss_nusi")
			{
				cli = 10,
				vagina = 30,
				canal = 15,
				anal = 40,
				multiple_orgasm = 0.24f
			},
			SerDmg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 20f),
			Beto = BetoInfo.NormalS.Pow(5, false)
		};

		protected EnAttackInfo AtkFaintAbsorb1;

		private NelNBoss_Nusi.TentacleLink[] ATentacleL;

		private NelNBoss_Nusi.MA_FLW ma_flower_;

		private float anmt;

		private BDic<IM2RayHitAble, float> OHitSpore;

		private Vector2 ShotDep;

		private const int PRI_ATK = 129;

		private const int PRI_WAIT = 128;

		private MagicItem.FnMagicRun FD_MgRunShot;

		private MagicItem.FnMagicRun FD_MgRunDrawShot;

		private MagicItem.FnMagicRun FD_MgRunSpore;

		private MagicItem.FnMagicRun FD_MgRunDrawSpore;

		public class AIFreeze
		{
			public AIFreeze(NelNBoss_Nusi _Boss)
			{
				this.Boss = _Boss;
			}

			public bool canMove(NAI Nai, out bool set_lock_aiphase2)
			{
				float floort = this.Boss.Mp.floort;
				bool flag = true;
				int aiPhase = this.Boss.getAiPhase();
				set_lock_aiphase2 = false;
				if (this.lock_2 > floort)
				{
					return false;
				}
				if (this.lock_1 > floort)
				{
					if (aiPhase < 2)
					{
						return false;
					}
					flag = Nai.RANtk(4714) < 0.05f;
				}
				else if (this.lock_0 > floort)
				{
					if (aiPhase < 2 && Nai.RANtk(3133) < this.Boss.time_p1_lock_prob)
					{
						return false;
					}
					flag = Nai.RANtk(4913) < ((Nai.En == this.Boss) ? 0.25f : 0.42f);
				}
				else if (this.lock_m1 > floort)
				{
					flag = false;
					if (this.Boss.getAI().RANtk(3715) < ((aiPhase < 2) ? 0.5f : 0.85f))
					{
						flag = true;
					}
				}
				if (flag)
				{
					int num;
					if (aiPhase < 2)
					{
						num = this.Boss.countActiveAttacker(Nai.En);
						if (num > 0 && this.Boss.getAI().RANtk(1493) < this.Boss.time_p1_delay_reduce_ratio)
						{
							return false;
						}
					}
					else
					{
						num = this.Boss.countActiveAttacker(Nai.En);
						if (num >= 1)
						{
							if (this.Boss.getAI().RANtk(1951) < this.Boss.time_p2_lock_ratio)
							{
								flag = false;
							}
							else
							{
								set_lock_aiphase2 = true;
							}
						}
					}
					if (num >= 2 && this.Boss.getAI().RANtk(2311) >= this.Boss.act3_canmove_alloc_ratio)
					{
						flag = false;
					}
				}
				return flag;
			}

			public NelNBoss_Nusi.AIFreeze Set(NAI Nai, int level, float time, float m1_extend = 180f)
			{
				time += X.Mx(0f, -20f + Nai.RANtk(3138) * 45f);
				int aiPhase = this.Boss.getAiPhase();
				if (level >= 2)
				{
					this.lock_2 = X.Mx(this.lock_2, this.Boss.Mp.floort + time);
				}
				else if (level == 1)
				{
					this.lock_1 = X.Mx(this.lock_1, this.Boss.Mp.floort + time);
				}
				else
				{
					this.lock_0 = X.Mx(this.lock_0, this.Boss.Mp.floort + time + (float)((aiPhase < 2) ? 40 : 0));
					this.lock_m1 = X.Mx(this.lock_m1, this.Boss.Mp.floort + time + m1_extend);
				}
				return this;
			}

			public NelNBoss_Nusi.AIFreeze Clear()
			{
				this.lock_2 = (this.lock_1 = (this.lock_0 = (this.lock_m1 = 0f)));
				return this;
			}

			public readonly NelNBoss_Nusi Boss;

			public float lock_m1;

			public float lock_0;

			public float lock_1;

			public float lock_2;
		}

		public enum MA_CAGE
		{
			HIDDEN,
			APPEAL,
			APPEAL_OPEN
		}

		private enum MA_FLW
		{
			STATIC,
			LAUGH,
			FAINTSTUN,
			DAMAGE,
			STUN,
			DAMAGE_IN_STUN,
			GRAWL,
			SHOT,
			SHOT_EXPLODED,
			DANCE,
			RUN,
			RUN2STATIC,
			DIE
		}

		public enum TENPOSE
		{
			stand,
			atk,
			atk2stand,
			atk_absorb,
			atk_prepare,
			faint_attack,
			faint_countered,
			faint_grab,
			faint_back,
			ground,
			ground2stand,
			grab_prepare,
			laugh,
			laugh2stand,
			stun,
			stun2laugh,
			underpunch_0,
			underpunch_1,
			middlepunch_0,
			middlepunch_1,
			struggle_0,
			struggle_1,
			struggle_2,
			running_0,
			running_1,
			running2stand
		}

		public class TentacleLink
		{
			public TentacleLink(EnemyAnimatorSpine _EA, int _v_id)
			{
				this.EA = _EA;
				this.EA.addListenerComplete(new AnimationState.TrackEntryDelegate(this.triggerAnimComplete));
				this.v_id = _v_id;
			}

			public void Unlink()
			{
				if (this.LinkTentacle != null)
				{
					this.EA.NestTarget.MapShift(this.FirstShiftPos.x, this.FirstShiftPos.y);
					NelNNusiTentacle linkTentacle = this.LinkTentacle;
					this.EA.fine_intv = 5f;
					this.LinkTentacle = null;
					linkTentacle.Unlink();
				}
			}

			public void EAPose(NelNBoss_Nusi.TENPOSE p, bool from_link = false, int order_front = -1)
			{
				if (this.LinkTentacle != null && !from_link)
				{
					return;
				}
				if (this.v_id == 2)
				{
					if (p == NelNBoss_Nusi.TENPOSE.stun2laugh)
					{
						p = NelNBoss_Nusi.TENPOSE.laugh2stand;
					}
					else if (p == NelNBoss_Nusi.TENPOSE.laugh2stand)
					{
						p = NelNBoss_Nusi.TENPOSE.stand;
					}
				}
				NelNBoss_Nusi.TentacleLink.EAPoseS(this.EA, this.v_id, p, order_front);
				this.EA.timescale = 1f;
				this.curpose = p;
			}

			public void checkFrameManual(float fcnt)
			{
				if (this.LinkTentacle != null && this.LinkTentacle.getAnimator().TempStop.isActive())
				{
					return;
				}
				this.EA.checkFrameManual(fcnt, false);
			}

			public CCNestItem walkShiftPosToDefault(float spd)
			{
				CCNestItem nestTarget = this.EA.NestTarget;
				nestTarget.shiftx = X.VALWALK(nestTarget.shiftx, this.FirstShiftPos.x, spd);
				nestTarget.shifty = X.VALWALK(nestTarget.shifty, this.FirstShiftPos.y, spd);
				return nestTarget;
			}

			public void triggerAnimComplete(TrackEntry trackEntry)
			{
				if (NelNBoss_Nusi.TentacleLink.animComplete(this.EA, this.v_id, ref this.curpose))
				{
					this.EA.timescale = 1f;
				}
			}

			public static void EAPoseS(EnemyAnimatorSpine EA, int v_id, NelNBoss_Nusi.TENPOSE p, int order_front = -1)
			{
				bool flag = true;
				switch (p)
				{
				case NelNBoss_Nusi.TENPOSE.atk:
					EA.clearAnim((v_id == 0) ? "atk0" : ((v_id == 1) ? "atk1" : "atk2"), -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.atk2stand:
					EA.clearAnim((v_id == 0) ? "atk2stand0" : ((v_id == 1) ? "atk2stand1" : "atk2stand2"), -1000, null);
					flag = EA.is_front;
					break;
				case NelNBoss_Nusi.TENPOSE.atk_absorb:
					EA.clearAnim("atk_absorb", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.atk_prepare:
					EA.clearAnim("atk_prepare", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.faint_attack:
					EA.clearAnim("faint_attack", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.faint_countered:
					EA.clearAnim("faint_countered", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.faint_grab:
					EA.clearAnim((v_id == 0) ? "faint_grab0" : ((v_id == 1) ? "faint_grab1" : "faint_grab2"), -1000, null);
					flag = EA.is_front;
					break;
				case NelNBoss_Nusi.TENPOSE.faint_back:
					EA.clearAnim("faint_back", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.ground:
					EA.clearAnim((v_id == 0) ? "ground0" : ((v_id == 1) ? "ground1" : "ground2"), -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.ground2stand:
					EA.clearAnim((v_id == 0) ? "ground2stand0" : ((v_id == 1) ? "ground2stand1" : "ground2stand2"), -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.grab_prepare:
					EA.clearAnim("grab_prepare", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.laugh:
					if (v_id >= 2)
					{
						return;
					}
					EA.clearAnim((v_id == 0) ? "laugh0" : "laugh1", -1000, null);
					flag = v_id == 0;
					break;
				case NelNBoss_Nusi.TENPOSE.laugh2stand:
					if (v_id >= 2)
					{
						return;
					}
					EA.clearAnim((v_id == 0) ? "laugh2stand0" : "laugh2stand1", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.stun:
					if (v_id >= 2)
					{
						return;
					}
					EA.clearAnim((v_id == 0) ? "stun0" : ((v_id == 1) ? "stun1" : "stun2"), -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.stun2laugh:
					EA.clearAnim((v_id == 0) ? "stun2laugh0" : "stun2laugh1", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.underpunch_0:
					EA.clearAnim("underpunch_0", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.underpunch_1:
					EA.clearAnim("underpunch_1", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.middlepunch_0:
					EA.clearAnim("middlepunch_0", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.middlepunch_1:
					EA.clearAnim("middlepunch_1", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.struggle_0:
					EA.clearAnim("struggle_0", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.struggle_1:
					EA.clearAnim("struggle_1", -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.struggle_2:
					EA.clearAnim((v_id == 0) ? "struggle_2_0" : ((v_id == 1) ? "struggle_2_1" : "struggle_2_2"), -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.running_0:
					EA.clearAnim((v_id == 0) ? "running0_0" : ((v_id == 1) ? "running0_1" : "running0_2"), -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.running_1:
					EA.clearAnim((v_id == 0) ? "running1_0" : ((v_id == 1) ? "running1_1" : "running1_2"), -1000, null);
					break;
				case NelNBoss_Nusi.TENPOSE.running2stand:
					EA.clearAnim((v_id == 0) ? "running2stand_0" : ((v_id == 1) ? "running2stand_1" : "running2stand_2"), -1000, null);
					break;
				default:
					EA.clearAnim((v_id == 0) ? "main0" : ((v_id == 1) ? "main1" : "main2"), -1000, null);
					break;
				}
				if (order_front >= 0)
				{
					EA.showToFront(order_front == 1, false);
					return;
				}
				EA.showToFront(flag, false);
			}

			public static bool animComplete(EnemyAnimatorSpine EA, int v_id, ref NelNBoss_Nusi.TENPOSE p)
			{
				NelNBoss_Nusi.TENPOSE tenpose = p;
				if (tenpose <= NelNBoss_Nusi.TENPOSE.ground2stand)
				{
					if (tenpose == NelNBoss_Nusi.TENPOSE.atk2stand)
					{
						EA.alpha = 0f;
						return false;
					}
					if (tenpose != NelNBoss_Nusi.TENPOSE.ground2stand)
					{
						return false;
					}
				}
				else
				{
					switch (tenpose)
					{
					case NelNBoss_Nusi.TENPOSE.laugh2stand:
					case NelNBoss_Nusi.TENPOSE.underpunch_1:
					case NelNBoss_Nusi.TENPOSE.middlepunch_1:
						break;
					case NelNBoss_Nusi.TENPOSE.stun:
					case NelNBoss_Nusi.TENPOSE.underpunch_0:
					case NelNBoss_Nusi.TENPOSE.middlepunch_0:
						return false;
					case NelNBoss_Nusi.TENPOSE.stun2laugh:
						p = NelNBoss_Nusi.TENPOSE.laugh;
						goto IL_005E;
					default:
						if (tenpose != NelNBoss_Nusi.TENPOSE.struggle_2 && tenpose != NelNBoss_Nusi.TENPOSE.running2stand)
						{
							return false;
						}
						break;
					}
				}
				p = NelNBoss_Nusi.TENPOSE.stand;
				IL_005E:
				NelNBoss_Nusi.TentacleLink.EAPoseS(EA, v_id, p, -1);
				return true;
			}

			public readonly EnemyAnimatorSpine EA;

			public readonly int v_id;

			public NelNNusiTentacle LinkTentacle;

			public Vector2 FirstShiftPos;

			public NelNBoss_Nusi.TENPOSE curpose;
		}
	}
}
