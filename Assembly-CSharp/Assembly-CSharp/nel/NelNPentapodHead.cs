using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNPentapodHead : NelEnemyNested, MgNGeneralBeam.IGeneralBeamer, ITortureListener
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			this.posename_damage = (this.posename_damage_huttobi = "floater_damage");
			ENEMYID id = this.id;
			this.id = ENEMYID.PENTAPOD_HEAD_0;
			NOD.BasicData basicData = NOD.getBasicData("PENTAPOD_HEAD_0");
			base.appear(_Mp, basicData);
			this.exist_fall_pose = false;
			this.exist_land_pose = false;
			this.exist_stun_pose = false;
			this.SpSetPose("floater_stand", -1, null, false);
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 9f;
			this.Nai.attackable_length_top = -5f;
			this.Nai.attackable_length_bottom = 4f;
			this.Nai.fnSleepLogic = (NAI Nai) => false;
			this.Nai.fnAwakeLogic = (NAI Nai) => false;
			this.Nai.fnOverDriveLogic = (NAI Nai) => false;
			this.absorb_weight = 1;
			this.AtkAbsorb.Prepare(this, true);
		}

		public override NelEnemyNested initNest(NelEnemy _Parent, int array_create_capacity = 4)
		{
			base.initNest(_Parent, array_create_capacity);
			this.EnBase = _Parent as NelNPentapod;
			this.applydmg_calc_parent = 0f;
			base.NestTeColor(true).NestManaAbsorb(true);
			this.sync_disappear = (this.sync_damagestate = true);
			this.changeFollow(NelNPentapodHead.FOLLOW.NORMAL);
			if (base.nattr_has_mattr)
			{
				base.M2D.loadMaterialSnd("enemy_golem");
			}
			return this;
		}

		public override void runPhysics(float fcnt)
		{
			if (!base.destructed && !base.disappearing && this.Phy.main_updated_count == 1)
			{
				if (this.cur_flw == NelNPentapodHead.FOLLOW.NORMAL || this.cur_flw == NelNPentapodHead.FOLLOW.SHOT || this.cur_flw == NelNPentapodHead.FOLLOW.ABSORB)
				{
					float num = this.EnBase.neck_len;
					if (this.cur_flw == NelNPentapodHead.FOLLOW.NORMAL)
					{
						float num2 = this.EnBase.Anm.rotationR + 1.5707964f;
						float num3 = this.EnBase.x + this.EnBase.neck_len_cur * 0.5f * X.Cos(num2);
						float num4 = this.EnBase.y - this.EnBase.neck_len_cur * X.Sin(num2);
						num3 += 0.8f * X.Cos(240f + this.Nai.RANn(3114) * 110f);
						num4 -= 0.8f * X.Sin(130f + this.Nai.RANn(2355) * 50f);
						if (((base.mpf_is_right > 0f) ? (base.x + 1f < num3) : (base.x - 1f > num3)) && base.wallHitted((base.mpf_is_right > 0f) ? AIM.R : AIM.L))
						{
							num = X.Mx(0f, this.EnBase.neck_len_cur - 0.2f);
						}
						float num5 = this.Mp.GAR(base.x, base.y, num3, num4);
						float num6 = 0.043f;
						float num7 = X.LENGTHXY2(base.x, base.y, this.EnBase.x, this.EnBase.y);
						float num8 = this.EnBase.neck_len + 0.5f;
						if (num7 >= num8 * num8)
						{
							num6 = X.NIL(num6, 0.27f, num7 - num8 * num8, 10f);
						}
						if (this.EnBase.mtop < base.y)
						{
							num6 = X.NIL(num6, 0.27f, base.y - this.EnBase.mtop, 0.8f);
						}
						float num9 = -num6 * X.Sin(num5);
						this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, num6 * X.Cos(num5), num9, -1f, -1, 1, 0, -1, 0);
					}
					float gravity_added_velocity = this.EnBase.getPhysic().gravity_added_velocity;
					if (this.gravity_simulated < gravity_added_velocity)
					{
						this.gravity_simulated = X.VALWALK(this.gravity_simulated, gravity_added_velocity, 0.009f * this.TS);
					}
					else if (this.gravity_simulated > gravity_added_velocity)
					{
						this.gravity_simulated = X.VALWALK(this.gravity_simulated, gravity_added_velocity, 0.021f * this.TS);
					}
					if (this.gravity_simulated != 0f)
					{
						this.Phy.addFoc(FOCTYPE.JUMP, 0f, this.gravity_simulated, -1f, -1, 1, 0, -1, 0);
					}
					if (this.cur_flw == NelNPentapodHead.FOLLOW.NORMAL)
					{
						this.EnBase.neck_len_cur = X.VALWALK(this.EnBase.neck_len_cur, num, 0.018f * this.TS);
						float num10 = (float)X.MPF(this.EnBase.x > base.x) * X.NIL(0.03f, 0.1f, X.Abs(this.EnBase.x - base.x), 1.5f) * 3.1415927f;
						this.Anm.rotationR = X.VALWALK(this.Anm.rotationR, num10, 0.006283186f * this.TS);
					}
					if (this.cur_flw == NelNPentapodHead.FOLLOW.ABSORB)
					{
						this.Anm.rotationR = X.VALWALK(this.Anm.rotationR, 0f, 0.012566372f * this.TS);
					}
				}
				else
				{
					float num11 = X.LENGTHXY2(base.x, base.y, this.EnBase.x, this.EnBase.y);
					if (num11 > this.EnBase.neck_len * this.EnBase.neck_len)
					{
						num11 = X.q_rsqrt(num11);
						float num12 = X.NI(0.06f, 1f, X.ZLINE(num11 - this.EnBase.neck_len, 1.8f));
						NelNPentapodHead.FOLLOW follow = this.cur_flw;
						float num13;
						if (follow != NelNPentapodHead.FOLLOW.FALL_HUTTOBI)
						{
							if (follow != NelNPentapodHead.FOLLOW.FALL_HUTTOBI_FOLLOW)
							{
								num13 = 0.02f;
							}
							else
							{
								num13 = 0.14f;
							}
						}
						else
						{
							num13 = 0.14f;
						}
						float num14 = num12 * num13;
						float num15 = base.weight * (float)((this.cur_flw == NelNPentapodHead.FOLLOW.FALL_HUTTOBI) ? 9 : 1);
						float num16 = this.EnBase.weight * (float)((this.cur_flw == NelNPentapodHead.FOLLOW.FALL_HUTTOBI_FOLLOW) ? 9 : 1);
						float num17 = num14 / (num15 + num16);
						float num18 = this.Mp.GAR(base.x, base.y, this.EnBase.x, this.EnBase.y);
						this.Phy.addFoc(FOCTYPE.ABSORB | FOCTYPE._INDIVIDUAL, num15 * num17 * X.Cos(num18), -num15 * num17 * X.Sin(num18), -1f, 0, 1, 20, -1, 0);
						this.EnBase.getPhysic().addFoc(FOCTYPE.ABSORB | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, -num16 * num17 * X.Cos(num18), num16 * num17 * X.Sin(num18), -1f, 0, 1, 20, -1, 0);
					}
				}
			}
			base.runPhysics(fcnt);
		}

		public void changeFollow(NelNPentapodHead.FOLLOW flw)
		{
			this.gravity_simulated = 0f;
			this.cur_flw = flw;
			NelNPentapodHead.FOLLOW follow = this.cur_flw;
			if (follow - NelNPentapodHead.FOLLOW.FALL <= 2)
			{
				base.base_gravity = 0.66f;
				return;
			}
			this.FootD.initJump(false, true, false);
			base.base_gravity = 0f;
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			NelEnemy.STATE state = this.state;
			base.changeState(st);
			if (state == NelEnemy.STATE.DAMAGE_HUTTOBI || state == NelEnemy.STATE.DAMAGE)
			{
				this.Anm.rotationR_speed = 0f;
				this.setAim(this.EnBase.aim, false);
				this.FootD.initJump(false, true, false);
				base.getPhysic().addFoc(FOCTYPE.JUMP, base.mpf_is_right * 0.06f, X.NIL(-0.2f, -0.05f, this.EnBase.y - base.y, 1.5f), -1f, 0, 3, 15, -1, 0);
			}
			if (st != NelEnemy.STATE.STAND)
			{
				this.torture_absorb = false;
			}
			NelEnemy.STATE state2 = this.state;
			if (state2 != NelEnemy.STATE.DAMAGE)
			{
				if (state2 != NelEnemy.STATE.DAMAGE_HUTTOBI)
				{
					this.changeFollow(NelNPentapodHead.FOLLOW.NORMAL);
				}
				else
				{
					this.changeFollow(NelNPentapodHead.FOLLOW.FALL_HUTTOBI);
				}
			}
			else
			{
				this.changeFollow(NelNPentapodHead.FOLLOW.FALL);
			}
			if (st == NelEnemy.STATE.STAND && (this.SpPoseIs(this.posename_damage_huttobi) || this.SpPoseIs(this.posename_damage)))
			{
				this.SpSetPose("floater_stand", -1, null, false);
			}
			return this;
		}

		public override void changeStateDamageHuttobiFromNest()
		{
			if (base.getState() != NelEnemy.STATE.DAMAGE_HUTTOBI)
			{
				base.changeStateDamageHuttobiFromNest();
				this.changeFollow(NelNPentapodHead.FOLLOW.FALL_HUTTOBI_FOLLOW);
			}
		}

		public override bool runDamageSmall()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.SpSetPose("floater_damage", 1, null, false);
				base.base_gravity = 0.66f;
				if (base.vx == 0f)
				{
					this.Anm.rotationR_speed = X.NIXP(0.008f, 0.006f) * 3.1415927f * base.mpf_is_right;
				}
			}
			base.runDamageSmall();
			if (base.hasFoot() || base.y >= this.EnBase.mbottom)
			{
				if (this.t >= 30f)
				{
					this.SpSetPose("floater_stand", -1, null, false);
					this.t = 1f;
					return false;
				}
			}
			else
			{
				this.t = 1f;
			}
			return true;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			Aattr.Add(EnemyAttr.atk_attr(this, MGATTR.STONE));
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.MAG_0)
			{
				if (type == NAI.TYPE.WALK)
				{
					return false;
				}
				if (type - NAI.TYPE.MAG <= 1)
				{
					return this.runMagBeam(Tk.initProgress(this), Tk);
				}
			}
			else
			{
				if (type == NAI.TYPE.GUARD)
				{
					return this.runHeadAbsorb(Tk.initProgress(this), Tk);
				}
				if (type == NAI.TYPE.WAIT)
				{
					base.AimToLr((X.xors(2) == 0) ? 0 : 2);
					Tk.after_delay = 30f + this.Nai.RANtk(840) * 40f;
					return false;
				}
			}
			return base.readTicket(Tk);
		}

		private bool runWalk(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
			}
			return true;
		}

		private bool runMagBeam(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				if (Tk.type == NAI.TYPE.MAG_0)
				{
					this.floating = true;
				}
				this.changeFollow(NelNPentapodHead.FOLLOW.SHOT);
				base.PtcVar("charget", 140.0).PtcST("pentapod_beam0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
				base.AimToLr((Tk.depx < this.Nai.target_x) ? 2 : 0);
				this.t = 0f;
				this.walk_st = 0;
				this.MhBeam.destruct(this);
				MgNGeneralBeam.BATTR battr;
				if (!MgNGeneralBeam.nattr2battr(this.nattr, out battr))
				{
					battr = MgNGeneralBeam.BATTR.STONE;
				}
				this.MhBeam = new MagicItemHandlerS(MgNGeneralBeam.setGeneralBeam(this.Mp, this, this, base.mg_hit, 0, battr, ref this.AtkBeam, ref this.MnBeam, 0.5f, true, 0.15300001f, 70f, true));
				if (!this.MhBeam.isActive(this))
				{
					return false;
				}
				if (this.MhBeam.Mg != null)
				{
					this.MpConsume(base.x, base.y, this.EnBase.McsBeam, this.MhBeam.Mg, 1f, 1f);
					if (this.MhBeam.Mg.Ray != null)
					{
						this.MhBeam.Mg.Ray.hittype |= HITTYPE.ONLY_FIRST_BREAKER;
						this.MhBeam.Mg.Ray.hit_target_max = 2;
					}
				}
				this.SpSetPose("floater_attack0", -1, null, false);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (!this.MhBeam.isActive(this))
				{
					return false;
				}
				if (this.t >= 80f && this.walk_st == 0)
				{
					this.SpSetPose("floater_attack_1", -1, null, false);
					this.walk_st = 1;
				}
				if (Tk.Progress(ref this.t, 140, true))
				{
					this.PtcHld.killPtc("pentapod_beam0", false);
					this.SpSetPose("floater_attack_2", -1, null, false);
					if (!base.nattr_has_mattr)
					{
						base.PtcVar("beamt", 70.0).PtcST("pentapod_beam1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
					}
					else
					{
						this.MhBeam.Mg.PtcVar("agR", (double)this.MhBeam.Mg.aim_agR).PtcVarS("attr", FEnum<MGATTR>.ToStr(this.MhBeam.Mg.Atk0.attr)).PtcST("golemtoy_arrow_shot_rotate", PTCThread.StFollow.FOLLOW_S, false);
					}
					this.MhBeam.Mg.phase = 3;
					this.MhBeam.Mg.t = 1f;
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (!this.MhBeam.isActive(this))
				{
					return false;
				}
				if (Tk.Progress(ref this.t, 70, true))
				{
					this.PtcHld.killPtc("pentapod_beam1", false);
					this.MhBeam.Mg.killPtc("golemtoy_arrow_shot_rotate");
					float num = this.MhBeam.Mg.Mn._0.agR + 3.1415927f;
					this.Phy.addFoc(FOCTYPE.JUMP, 0.06f * X.Cos(num), -0.033f * X.Sin(num), -2f, 0, 5, 40, -1, 0);
					this.MhBeam.Mg.phase = 4;
					this.MhBeam.Mg.t = 0f;
					this.MhBeam = default(MagicItemHandlerS);
					this.changeFollow(NelNPentapodHead.FOLLOW.NORMAL);
					this.SpSetPose("floater_attack3", -1, null, false);
				}
			}
			if (Tk.prog == PROG.PROG0 || Tk.prog == PROG.ACTIVE)
			{
				float num2 = Tk.depx;
				float num3 = Tk.depy;
				num2 += 0.8f * X.Cos(240f + this.Nai.RANn(3114) * 110f);
				num3 -= 0.8f * X.Sin(130f + this.Nai.RANn(2355) * 50f);
				float num4 = this.Mp.GAR(base.x, base.y, num2, num3);
				float num5 = X.Mn(X.LENGTHXYS(base.x, base.y, num2, num3) * 0.5f, 0.083f);
				this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, num5 * X.Cos(num4), -num5 * X.Sin(num4), -1f, -1, 1, 0, -1, 0);
			}
			return Tk.prog != PROG.PROG1 || this.t < 80f;
		}

		bool MgNGeneralBeam.IGeneralBeamer.getBeamPosition(MagicItem Mg, int id, out Vector2 SPos, out float agR, out bool slow_angle_walk)
		{
			SPos = this.getTargetPos();
			float num = base.x;
			float num2 = base.y;
			if (Mg.t < 50f)
			{
				NaTicket curTicket = this.Nai.getCurTicket();
				if (curTicket != null && (curTicket.type == NAI.TYPE.MAG_0 || curTicket.type == NAI.TYPE.MAG))
				{
					num = curTicket.depx;
					num2 = curTicket.depy;
				}
			}
			float num3 = this.Nai.target_x;
			if (!base.AimPr.is_alive || (base.AimPr is PR && (base.AimPr as PR).isAnimationFrozen()))
			{
				agR = this.Mp.GAR(num, num2, num3, this.Nai.target_y + 0.1f - 0.2f * this.Nai.RANtk(2993));
			}
			else
			{
				num3 += base.mpf_is_right * (0.5f + 0.8f * X.COSI(this.Mp.floort + this.Nai.RANtk(435) * 200f, this.Nai.NIRANtk(180f, 370f, 2156))) * (this.Nai.isPrGaraakiState() ? 0.2f : 1f);
				if (base.AimPr is PR && ((base.AimPr as PR).isPoseCrouch(false) || (base.AimPr as PR).isPoseDown(false)))
				{
					agR = this.Mp.GAR(num, num2, num3, this.Nai.target_lastfoot_bottom - 0.5f);
				}
				else
				{
					agR = this.Mp.GAR(num, num2, num3 + this.Nai.NIRANtk(0.3f, 1.5f, 2384) * base.mpf_is_right, this.Nai.target_lastfoot_bottom - 0.6f);
				}
			}
			float num4 = 0f;
			if (base.mpf_is_right < 0f)
			{
				num4 = 3.1415927f;
			}
			float num5 = X.angledifR(num4, agR);
			agR = num4 + X.absMn(num5, 1.2566371f);
			slow_angle_walk = true;
			float num6 = agR;
			if (base.mpf_is_right < 0f)
			{
				num6 -= 3.1415927f;
			}
			this.Anm.rotationR = X.VALWALKANGLER(this.Anm.rotationR, num6, 0.07853982f);
			return true;
		}

		bool MgNGeneralBeam.IGeneralBeamer.BeamDrawAfter(MagicItem Mg, float fcnt)
		{
			return true;
		}

		float MgNGeneralBeam.IGeneralBeamer.getBeamPositionWalkSpeed()
		{
			return 0.08f * this.Anm.scaleX;
		}

		float MgNGeneralBeam.IGeneralBeamer.getBeamReachableLength()
		{
			return 13f;
		}

		public override Vector2 getTargetPos()
		{
			Vector2 vector = X.ROTV2e(new Vector2(base.mpf_is_right * 0.4f * this.Anm.scaleX, 0f), this.Anm.rotationR);
			vector.x += base.x;
			vector.y += base.y;
			return vector;
		}

		private bool runHeadAbsorb(bool init_flag, NaTicket Tk)
		{
			if (!this.is_alive || !this.EnBase.is_alive || this.EnBase.getState() != NelEnemy.STATE.ABSORB)
			{
				Tk.AfterDelay(30f);
				return false;
			}
			AbsorbManager absorbManager = this.EnBase.getAbsorbManager();
			if (absorbManager == null)
			{
				return false;
			}
			M2Attackable targetMover = absorbManager.getTargetMover();
			if (init_flag)
			{
				this.changeFollow(NelNPentapodHead.FOLLOW.ABSORB);
				this.t = 0f;
				this.SpSetPose("floater_absorb_0", -1, null, false);
				base.fineDrawPosition();
				base.PtcST("pentapod_absorb_prepare", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Anm.order_front = M2Mover.DRAW_ORDER.N_TOP1;
				this.Anm.showToFront(true, false);
				this.Phy.addLockMoverHitting(HITLOCK.ABSORB, -1f);
			}
			base.initDrawAssist(3, false);
			if (Tk.prog == PROG.ACTIVE)
			{
				base.AimToPlayer();
				float num = targetMover.mpf_is_right_visible * (0.85f * (X.ZSIN(this.t, 20f) - X.ZSINV(this.t - 20f, 25f)) - (0.5f + 0.5f * this.Anm.scaleX) * (X.ZSIN(this.t - 45f, 32f) * 1.5f - X.ZCOS(this.t - 77f, 45f) * 0.5f));
				num += this.EnBase.x;
				float num2 = X.NI(this.EnBase.y - this.EnBase.neck_len_cur - 0.2f, targetMover.y - (0.4f + 0.2f * this.Anm.scaleX), X.ZCOS(this.t, 120f));
				this.drawx_ = X.VALWALK(num * this.Mp.CLEN, this.drawx_, 0.08f * this.Mp.CLEN * this.TS);
				this.drawy_ = X.VALWALK(num2 * this.Mp.CLEN, this.drawy_, 0.08f * this.Mp.CLEN * this.TS);
				if (Tk.Progress(ref this.t, 140, true))
				{
					if (!this.EnBase.initHeadAbsorbInjection())
					{
						return false;
					}
					this.torture_absorb = true;
					this.SpSetPose("floater_absorb_1", -1, null, false);
					this.walk_st = 6 + X.xors(8);
					this.walk_time = 0f;
					if (targetMover is PR)
					{
						PR pr = targetMover as PR;
						pr.fine_frozen_replace = true;
						pr.setAim(this.aim, false);
						pr.getAnimator().setAim(this.aim, 0, true);
					}
				}
			}
			if (Tk.prog >= PROG.PROG0)
			{
				float num3;
				float num4;
				if (!AbsorbManager.syncTorturePositionS(targetMover, this, this.Phy, "Inject2_nohat", out num3, out num4, false))
				{
					return false;
				}
				if (this.walk_time <= 0f)
				{
					PROG prog = Tk.prog;
					int num5 = this.walk_st - 1;
					this.walk_st = num5;
					PROG prog3;
					if (num5 <= 0)
					{
						PROG prog2 = Tk.prog;
						Tk.prog = PROG.PROG0 + X.xors(3);
						prog3 = Tk.prog;
						if (prog3 != PROG.PROG0)
						{
							if (prog3 != PROG.PROG1)
							{
								num5 = 9 + X.xors(33);
							}
							else
							{
								num5 = 9 + X.xors(17);
							}
						}
						else
						{
							num5 = 5 + X.xors(6);
						}
						this.walk_st = num5;
						if (prog2 == Tk.prog)
						{
							this.walk_st = X.IntC((float)this.walk_st * 0.66f);
						}
					}
					base.runAbsorb();
					if (targetMover is PR)
					{
						PR pr2 = targetMover as PR;
						base.applyAbsorbDamageTo(pr2, this.AtkAbsorb, X.XORSP() < 0.44f || !targetMover.is_alive, false, false, 0f, false, null, false, true);
					}
					if (X.XORSP() < 0.5f)
					{
						this.Anm.randomizeFrame(0.5f, 0.5f);
					}
					Vector3 vector;
					if (!targetMover.getEffectReposition(null, PTCThread.StFollow.FOLLOW_HIP, 1f, out vector))
					{
						vector = new Vector3(targetMover.drawx_map - targetMover.mpf_is_right_visible * 1f, targetMover.drawy_map, 1f);
					}
					targetMover.PtcVar("cx", (double)vector.x).PtcVar("cy", (double)vector.y).PtcVar("ax", (double)(-(double)targetMover.mpf_is_right_visible))
						.PtcST("pentapod_absorb_splash", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					prog3 = Tk.prog;
					float num6;
					if (prog3 != PROG.PROG0)
					{
						if (prog3 != PROG.PROG1)
						{
							num6 = X.NIXP(11f, 16f);
						}
						else
						{
							num6 = X.NIXP(14f, 26f);
						}
					}
					else
					{
						num6 = X.NIXP(32f, 45f);
					}
					this.walk_time = num6;
				}
				else
				{
					this.walk_time -= this.TS;
				}
			}
			float num7 = this.drawx_ * this.Mp.rCLEN;
			float num8 = this.drawy_ * this.Mp.rCLEN;
			if (X.LENGTHXYS(base.x, base.y, num7, num8) > 0.125f)
			{
				float num9 = X.VALWALK(base.x, num7, 0.03125f);
				float num10 = X.VALWALK(base.y, num8, 0.03125f);
				this.Phy.addFoc(FOCTYPE.ABSORB, num9 - base.x, num10 - base.y, -1f, -1, 1, 0, -1, 0);
			}
			return true;
		}

		public override bool isTortureUsingForAnim()
		{
			return this.torture_absorb;
		}

		bool ITortureListener.setTortureAnimation(string pose_name, int cframe, int loop_to)
		{
			return this.is_alive && this.EnBase.is_alive && this.torture_absorb;
		}

		void ITortureListener.setToTortureFix(float x, float y)
		{
			this.drawx_ = x * base.CLEN;
			this.drawy_ = y * base.CLEN;
		}

		void ITortureListener.runPostTorture()
		{
		}

		public override void quitTicket(NaTicket Tk)
		{
			this.torture_absorb = false;
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.MAG_0 || Tk.type == NAI.TYPE.MAG || Tk.type == NAI.TYPE.GUARD)
				{
					this.floating = false;
					this.changeFollow(NelNPentapodHead.FOLLOW.NORMAL);
					this.SpSetPose("floater_stand", -1, null, false);
				}
				this.Anm.showToFront(false, false);
			}
			this.Phy.remLockMoverHitting(HITLOCK.ABSORB);
			if (this.MhBeam.isActive(this) && this.MhBeam.Mg.phase == 3)
			{
				this.MhBeam.Mg.phase = 4;
				this.MhBeam.Mg.t = 0f;
				this.MhBeam = default(MagicItemHandlerS);
			}
			this.MhBeam.destruct(this);
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			base.quitTicket(Tk);
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			if (this.Nai == null || !this.is_alive)
			{
				return false;
			}
			if (Mg.kind == MGKIND.TACKLE)
			{
				return this.canAbsorbContinue() && this.can_hold_tackle;
			}
			if (Mg.kind == MGKIND.GENERAL_BEAM)
			{
				NaTicket curTicket = this.Nai.getCurTicket();
				if (curTicket != null && (curTicket.type == NAI.TYPE.MAG_0 || curTicket.type == NAI.TYPE.MAG))
				{
					return true;
				}
			}
			return false;
		}

		private NelNPentapodHead.FOLLOW cur_flw;

		private NelNPentapod EnBase;

		private float gravity_simulated;

		private const NAI.TYPE NTYPE_BEAM = NAI.TYPE.MAG;

		private const NAI.TYPE NTYPE_BEAM_IGNOREWALL = NAI.TYPE.MAG_0;

		public const NAI.TYPE NTYPE_ABSORB = NAI.TYPE.GUARD;

		private MagicItemHandlerS MhBeam;

		private const int charge_maxt = 140;

		private const int beam_maxt = 70;

		private const int beam_after_t = 80;

		private const float beam_power = 0.5f;

		private bool torture_absorb;

		protected EnAttackInfo AtkAbsorb = new EnAttackInfo
		{
			hpdmg0 = 7,
			split_mpdmg = 2,
			attr = MGATTR.ABSORB_V,
			EpDmg = new EpAtk(10, "pentapod")
			{
				anal = 4
			},
			Beto = BetoInfo.Absorbed
		};

		private MagicNotifiear MnBeam;

		private NelAttackInfo AtkBeam;

		public const float beam_radius = 0.15300001f;

		public enum FOLLOW
		{
			NORMAL,
			SHOT,
			ABSORB,
			FALL,
			FALL_HUTTOBI,
			FALL_HUTTOBI_FOLLOW
		}
	}
}
