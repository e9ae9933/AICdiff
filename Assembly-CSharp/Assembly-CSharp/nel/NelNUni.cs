using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNUni : NelEnemy
	{
		public static void FlushPxlData()
		{
			if (NelNUni.Olayer2pos != null)
			{
				NelNUni.Olayer2pos.Clear();
			}
			NelNUni.DroDrillHit = null;
		}

		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.BaseAttackTarget = this.AtkTackleBody;
			if (NelNUni.Olayer2pos == null)
			{
				NelNUni.Olayer2pos = new BDic<PxlFrame, int>();
			}
			if (NelNUni.DroDrillHit == null)
			{
				NelNUni.DroDrillHit = M2DropObjectReader.Get("uni_drill_hit", false);
				NelNUni.DroOdGroundPound = M2DropObjectReader.Get("uni_od_ground_pound", false);
			}
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			this.Od = new OverDriveManager(this, 53, 43);
			ENEMYID id = this.id;
			this.id = ENEMYID.UNI_0;
			NOD.BasicData basicData = NOD.getBasicData("UNI_0");
			base.base_gravity = 0f;
			base.appear(_Mp, basicData);
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 10f;
			this.Nai.attackable_length_top = -7f;
			this.Nai.attackable_length_bottom = 4f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.auto_rot_on_damage = true;
			this.FD_MgRunNormalShot = new MagicItem.FnMagicRun(this.MgRunNormalShot);
			this.FD_MgDrawNormalShot = new MagicItem.FnMagicRun(this.MgDrawNormalShot);
			this.FD_isUniPublish = new Func<AbsorbManager, bool>(this.isUniPublish);
			this.NasChaser = new NASAirChaser(this, new NASAirChaser.FnProgressWalk(this.fnChaserProgressWalk))
			{
				fnGetPointAddition = new M2ChaserAir.FnGetPointAddition(this.fnChaserGetPointAddition)
			};
			this.absorb_weight = 1;
			this.Nai.consider_only_onfoot = false;
			this.enlarge_maximize_mp_ratio = 1f;
			this.FD_fnFineUniFrame = new EnemyAnimator.FnFineFrame(this.fnFineUniFrame);
			this.Anm.fnFineFrame = this.FD_fnFineUniFrame;
			base.addF((NelEnemy.FLAG)192);
		}

		public override void initOverDriveAppear()
		{
			base.initOverDriveAppear();
			this.Nai.consider_only_onfoot = true;
			this.Anm.fnFineFrame = null;
			this.Anm.rotationR = 0f;
			this.Anm.rotationR_speed = 0f;
			base.base_gravity = 0.66f;
			base.remF((NelEnemy.FLAG)192);
			this.killBaseTackle();
			this.SpSetPose(base.hasFoot() ? "od_land" : "od_fall", -1, null, false);
		}

		public override void quitOverDrive()
		{
			base.quitOverDrive();
			this.Anm.fnFineFrame = this.FD_fnFineUniFrame;
			this.Nai.consider_only_onfoot = false;
			base.addF((NelEnemy.FLAG)192);
			this.killBaseTackle();
			this.absorb_weight = 1;
		}

		public override IFootable canFootOn(IFootable F)
		{
			if (!(F is M2BlockColliderContainer.BCCLine))
			{
				return F;
			}
			M2BlockColliderContainer.BCCLine bccline = F as M2BlockColliderContainer.BCCLine;
			if (!base.isOverDrive() && bccline.is_lift)
			{
				return null;
			}
			if (!bccline.is_ladder)
			{
				return F;
			}
			return null;
		}

		private void setBaseTackle(bool set_mag = false, NelAttackInfo Atk = null)
		{
			NOD.TackleInfo tackleInfo = (base.isOverDrive() ? this.TkiBodyOd : this.TkiBody);
			if (Atk == null)
			{
				if (this.MgTackleBody.isActive(this))
				{
					Atk = this.MgTackleBody.Mg.Atk0;
				}
				else
				{
					Atk = this.BaseAttackTarget;
					if (base.isOverDrive())
					{
						Atk.knockback_len = 1.2f;
					}
				}
			}
			if (!this.MgTackleBody.isActive(this))
			{
				if (!set_mag)
				{
					return;
				}
				bool can_hold_tackle = this.can_hold_tackle;
				this.MgTackleBody = new MagicItemHandlerS(base.tackleInit(Atk, tackleInfo));
				this.can_hold_tackle = can_hold_tackle;
			}
			else
			{
				this.MgTackleBody.Mg.Atk0 = Atk;
			}
			this.MgTackleBody.Mg.Ray.HitLock(15f, null);
			this.MgTackleBody.Mg.sz = tackleInfo.radius * base.scaleX;
		}

		private void killBaseTackle()
		{
			this.MgTackleBody.destruct(this);
		}

		private void setNeedleTackle(NelAttackInfo _Atk = null, NOD.TackleInfo Tki = null)
		{
			if (!this.MgNeedle.isActive(this))
			{
				if (Tki == null)
				{
					return;
				}
				this.MgNeedle = new MagicItemHandlerS(base.tackleInit(_Atk, Tki));
				this.TkiNeedleSetted = Tki;
			}
			float num = this.TkiNeedleSetted.calc_difx_map(this);
			float rotR = this.rotR;
			this.MgNeedle.Mg.sx = num * X.Cos(rotR);
			this.MgNeedle.Mg.sy = -num * X.Sin(rotR);
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			NelEnemy.STATE state = this.state;
			base.changeState(st);
			if (state == NelEnemy.STATE.ABSORB)
			{
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
				base.carryable_other_object = false;
				this.Anm.showToFront(false, false);
				base.remF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
				this.killEDforSplashNeedle();
				this.cannot_move = false;
			}
			if (this.state == NelEnemy.STATE.STAND)
			{
				this.setBaseTackle(true, null);
				if (!base.isOverDrive())
				{
					if (this.state == NelEnemy.STATE.DAMAGE || this.state == NelEnemy.STATE.DAMAGE_HUTTOBI || this.state == NelEnemy.STATE.STUN)
					{
						this.SpSetPose("stand", -1, null, false);
					}
					base.base_gravity = 0f;
					this.Anm.rotationR = 0f;
					this.Anm.rotationR_speed = 0f;
				}
			}
			else if (state == NelEnemy.STATE.STAND)
			{
				this.killBaseTackle();
				this.Anm.rotationR_speed = 0f;
			}
			return this;
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (this.state == NelEnemy.STATE.STAND)
			{
				if (this.Nai.HasF(NAI.FLAG.INJECTED, false) && !this.Ser.has(SER.TIRED) && !this.Ser.has(SER.EATEN))
				{
					this.Nai.RemF(NAI.FLAG.INJECTED);
					this.setBaseTackle(true, null);
				}
				if (!base.isOverDrive())
				{
					if (!this.Nai.HasF(NAI.FLAG.ATTACKED, false))
					{
						float num = this.dep_agR;
						num = ((this.Anm.getAim() == 0) ? (3.1415927f + num) : num);
						float num2 = (float)X.MPF(X.angledifR(this.Anm.rotationR, num) > 0f);
						bool flag = this.Anm.rotationR_speed > 0f != num2 > 0f;
						this.Anm.rotationR_speed = X.VALWALK(this.Anm.rotationR_speed, num2 * 0.03f * 3.1415927f, (flag ? 0.0055f : 0.004f) * 3.1415927f);
					}
					else
					{
						this.Anm.rotationR_speed = 0f;
					}
					if (!this.MgTackleBody.isActive(this) && !this.Nai.HasF(NAI.FLAG.WANDERING, false))
					{
						this.changeState(NelEnemy.STATE.DAMAGE);
					}
				}
				else
				{
					this.Anm.rotationR_speed = X.VALWALK(this.Anm.rotationR_speed, 0f, 0.003141593f);
				}
				if (this.MgNeedle.isActive(this))
				{
					this.setNeedleTackle(null, null);
				}
			}
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai))
			{
				return true;
			}
			if (!base.isOverDrive() && !Nai.hasPriorityTicket(200, false, false))
			{
				float target_slen_n = Nai.target_slen_n;
				if (target_slen_n < 3f)
				{
					return Nai.AddTicketB(NAI.TYPE.WALK, 200, true);
				}
				if ((float)this.mp < (float)this.maxmp * 0.65f && Nai.AddTicketSearchAndGetManaWeed(200, 20f, -10f, 10f, 1.5f, -1f, 5f, true) != null)
				{
					return true;
				}
				if (base.Useable(this.McDrill, 1f, 0f) && (Nai.target_hasfoot || Nai.isPrMagicChanting(1f) || Nai.isPrShieldOpening(20)) && target_slen_n > 3f && target_slen_n < 10f && this.Mp.canThroughBcc(base.x, base.y, Nai.target_x, Nai.target_y, 0.3f, -1000f, -1, false, false, null, true, null) && Nai.fnBasicMagic(Nai, 200, 0f, (float)(base.Useable(this.McDrill, 1.6f, 0f) ? 80 : 55), 0f, 0f, 7145, false))
				{
					return true;
				}
				if (base.Useable(this.McShot, 1f, 0f) && Nai.fnBasicMagic(Nai, 200, (float)(Nai.isPrAlive() ? 50 : 80), 0f, 0f, 0f, 7145, false))
				{
					return true;
				}
				if (target_slen_n >= 15f || Nai.RANtk(461) < 0.6f)
				{
					return Nai.AddTicketB(NAI.TYPE.WALK, 200, true);
				}
			}
			return Nai.fnBasicMove(Nai);
		}

		private bool considerOverDrive(NAI Nai)
		{
			if (Nai.HasF(NAI.FLAG.OVERDRIVED, true))
			{
				Nai.AddTicket(NAI.TYPE.APPEAL_0, 128, true);
				return true;
			}
			if (Nai.hasPriorityTicket(200, false, false))
			{
				return true;
			}
			float num = Nai.RANtk(463);
			if (((num < 0.4f && Nai.target_lastfoot_len < this.TkiSplashNeedle.calc_difx_map(this) * 0.7f) || Nai.HasF(NAI.FLAG.POWERED, false)) && base.Useable(this.McsSplashNeedle, 1f, 0f) && !Nai.hasTypeLock(NAI.TYPE.PUNCH_0))
			{
				Nai.RemF(NAI.FLAG.POWERED);
				return Nai.AddTicketB(NAI.TYPE.PUNCH_0, 200, true);
			}
			if (num < 0.8f && !Nai.hasTypeLock(NAI.TYPE.PUNCH))
			{
				return Nai.AddTicketB(NAI.TYPE.PUNCH, 200, true);
			}
			return Nai.AddTicketB(NAI.TYPE.WAIT, 200, true);
		}

		public override bool readTicket(NaTicket Tk)
		{
			if (base.isOverDrive())
			{
				return this.readTicketOd(Tk);
			}
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
				return this.runWalk(Tk.initProgress(this), Tk);
			case NAI.TYPE.WALK_TO_WEED:
			{
				bool flag = Tk.initProgress(this);
				if (flag && Tk.DepBCC != null && Tk.DepBCC.isFallable(base.x, base.y, this.sizex, 0.15f) > 0f)
				{
					Tk.Recreate(NAI.TYPE.PUNCH_WEED, -1, true, null);
					return true;
				}
				return this.runWalk(flag, Tk);
			}
			case NAI.TYPE.PUNCH:
			case NAI.TYPE.PUNCH_0:
			case NAI.TYPE.PUNCH_1:
			case NAI.TYPE.PUNCH_2:
				break;
			case NAI.TYPE.PUNCH_WEED:
				return this.runPunchWeed(Tk.initProgress(this), Tk);
			case NAI.TYPE.MAG:
				return this.runNormalShot(Tk.initProgress(this), Tk);
			case NAI.TYPE.MAG_0:
				return this.runDrillAttack(Tk.initProgress(this), Tk);
			default:
				if (type == NAI.TYPE.WAIT)
				{
					base.AimToLr((X.xors(2) == 0) ? 0 : 2);
					Tk.after_delay = 30f + this.Nai.RANtk(840) * 40f;
					return false;
				}
				break;
			}
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.MAG_0)
				{
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
				}
				if (!base.isOverDrive() && Tk.type == NAI.TYPE.MAG_0)
				{
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
					if (Tk.prog >= PROG.PROG0)
					{
						base.MpConsume(this.McDrill, null, 1f, 1f);
					}
					this.cannot_move = false;
				}
			}
			this.BaseAttackTarget = this.AtkTackleBody;
			this.Nai.RemF(NAI.FLAG.WANDERING);
			this.fineAngle();
			base.remF((NelEnemy.FLAG)6291456);
			this.Nai.RemF(NAI.FLAG.ATTACKED);
			this.Nai.AddF(NAI.FLAG.INJECTED, -1f);
			if (!base.isOverDrive())
			{
				base.base_gravity = 0f;
			}
			this.CpForGroundBreaking = null;
			this.MgNeedle.destruct(this);
		}

		protected override bool initDeathEffect()
		{
			if (this.SndLoopWalk != null)
			{
				this.SndLoopWalk.destruct();
				this.SndLoopWalk = null;
			}
			this.killEDforSplashNeedle();
			return base.initDeathEffect();
		}

		protected bool runWalk(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				this.Anm.rotationR_speed = 0f;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				this.NasChaser.Clear(false);
				this.NasChaser.run_speed = this.normal_walk_spd;
				if (Tk.type == NAI.TYPE.WALK_TO_WEED)
				{
					this.NasChaser.setDest(Tk.depx, Tk.depy - 2.2f);
				}
				else
				{
					Tk.check_nearplace_error = 0;
					int num = this.walk_st / 8;
					float num2 = this.Nai.NIRANtk(0.13f, 0.49f, 519) * 3.1415927f;
					float num3 = this.Nai.RANtk(1993) * 6.2831855f + (float)this.walk_st * num2 + 0.39f * (float)num;
					float num4 = this.Nai.NIRANtk(3f, 4.5f, 3314) + (float)this.walk_st * 0.05f;
					int num5 = 0;
					this.walk_st += 8;
					for (int i = 0; i < 8; i++)
					{
						float num6 = this.Nai.target_x + num4 * X.Cos(num3);
						float num7 = this.Nai.target_y - num4 * X.Sin(num3);
						if (!this.Nai.isDecliningXy(num6, num7, 0.5f, 0.5f) && this.Nai.isAreaSafe(num6, num7, 0.2f, 0.2f, true, true, true))
						{
							this.NasChaser.setDest(num6, num7);
							if (++num5 >= 2)
							{
								break;
							}
							if (this.NasChaser.dest_count >= 6)
							{
								this.walk_st = -1;
								break;
							}
						}
						num3 += num2;
						num4 += 0.05f;
					}
					if (this.walk_st >= 0 && this.walk_st < 80)
					{
						return true;
					}
				}
				if (!this.NasChaser.hasDestPoint())
				{
					this.walkRandomJump();
					return Tk.error();
				}
				Tk.prog = PROG.PROG0;
				this.walk_st = 0;
			}
			M2ChaserBaseFuncs.CHSRES chsres = this.NasChaser.Walk(this.TS);
			if (chsres == M2ChaserBaseFuncs.CHSRES.ERROR)
			{
				this.walkRandomJump();
				return Tk.error();
			}
			if (chsres == M2ChaserBaseFuncs.CHSRES.REACHED)
			{
				if (Tk.type != NAI.TYPE.WALK_TO_WEED)
				{
					Tk.after_delay = 65f;
				}
				return false;
			}
			return true;
		}

		private void walkRandomJump()
		{
			float num = X.NIXP(0.14f, 0.2f);
			float num2 = X.XORSPS() * 3.1415927f;
			this.Phy.addFoc(FOCTYPE.WALK, num * X.Cos(num2), -num * X.Sin(num2), -1f, 0, 4, 30, -1, 0);
			this.Nai.delay = 60f;
		}

		private bool fnChaserProgressWalk(M2ChaserAir Chaser, M2ChaserAir.ChaserReachedDepert Depert, Vector2 Next, float agR, bool pos_updated)
		{
			this.dep_agR = agR;
			this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, this.normal_walk_spd * X.Cos(agR), -this.normal_walk_spd * X.Sin(agR), -1f, -1, 1, 0, -1, 0);
			return false;
		}

		private void fnChaserGetPointAddition(ref M2ChaserAir.ChaserReachedDepert Depert)
		{
			Vector2 vector = new Vector2(Depert.x, Depert.y);
			bool flag = this.Nai.isPrAlive() != this.Nai.RANtk(9115) < 0.8f;
			float num;
			if (this.Mp.canThroughBcc(this.Nai.target_x, this.Nai.target_y, vector.x, vector.y, 0.2f, -1000f, -1, false, false, null, true, null))
			{
				num = (float)(flag ? 0 : 100);
			}
			else
			{
				num = (float)(flag ? 50 : 0);
			}
			int num2 = (int)vector.x;
			for (int i = 0; i < 4; i++)
			{
				if (!this.Mp.canStand(num2, (int)(vector.y + (float)i + 1f)))
				{
					num += (float)((4 - i) * 5);
					break;
				}
			}
			Depert.point_addition = num;
		}

		protected bool runPunchWeed(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				if (X.Abs(this.Mp.getFootableY(base.x, (int)base.y, 12, false, -1f, false, true, true, 0f) - (Tk.depy + this.sizey)) >= 0.125f)
				{
					Tk.Recreate(NAI.TYPE.WALK_TO_WEED, -1, false, null);
					return true;
				}
				this.Phy.addFocToSmooth(FOCTYPE.WALK, base.x, base.y - 1.5f, 23, -1, 0, -1f);
				this.dep_agR = -1.5707964f;
				this.SpSetPose("attack_0", -1, null, false);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 23, true))
			{
				base.base_gravity = 0.66f;
				this.setNeedleTackle(this.AtkPunchWeed, this.TkiPunchWeed);
				this.SpSetPose("attack_1", -1, null, false);
			}
			if (Tk.prog == PROG.PROG0 && base.hasFoot())
			{
				this.Nai.addDeclineArea(Tk.depx - 1f, Tk.depy - 1f, 2f, 2f, 180f);
				this.Phy.addFoc(FOCTYPE.JUMP, 0f, -0.06f, -1f, 14, 20, 40, -1, 0);
				Tk.AfterDelay(70f);
				base.base_gravity = 0f;
				this.can_hold_tackle = false;
				this.SpSetPose("attack_2", -1, null, false);
				return false;
			}
			return true;
		}

		protected bool runNormalShot(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				if (!base.Useable(this.McShot, 1f, 0f))
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG, 300f);
					return false;
				}
				this.walk_time = 0f;
				this.walk_st = 0;
				this.dep_agR = 1.5707964f;
				this.ShotDepert = Vector3.zero;
				this.SpSetPose("shot_0", -1, null, false);
				base.PtcVar("agR", (double)this.rotR).PtcST("uni_shot_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.Anm.rotationR_speed = 0f;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.walk_time <= 0f && (this.ShotDepert.z == 0f || this.walk_st < 36))
				{
					this.walk_time = 2f;
					if (this.ShotDepert.z == 0f && this.Mp.canThroughBccR(this.Nai.target_x, this.Nai.target_y, base.x, base.y, 0.3f))
					{
						this.ShotDepert.Set(this.Nai.target_x, this.Nai.target_y, 0.001f);
						this.walk_st = 100;
					}
					else
					{
						float num = this.Nai.RANtk(1993) * 6.2831855f + (float)this.walk_st * 2.1048672f;
						float num2 = 4.5f + (float)this.walk_st * 0.33f;
						for (int i = 0; i < 2; i++)
						{
							float num3 = this.Nai.target_x + num2 * X.Cos(num);
							float num4 = this.Nai.target_y - num2 * X.Sin(num);
							if (this.Mp.canThroughBccR(this.Nai.target_x, this.Nai.target_y, num2, num, 0.2f) && this.Mp.canThroughBcc(num3, num4, base.x, base.y, 0.2f, -1000f, -1, false, false, null, true, null))
							{
								float num5 = X.LENGTHXY2(num3, num4, base.x, base.y);
								if (this.ShotDepert.z == 0f || num5 < this.ShotDepert.z)
								{
									float num6 = this.Mp.GAR(base.x, base.y, num3, num4);
									if (this.Mp.canThroughBccR(base.x, base.y, 5f, num6, 0.2f))
									{
										this.ShotDepert.Set(num3, num4, num5);
									}
								}
							}
							num += 2.1048672f;
							num2 += 0.33f;
							this.walk_st++;
						}
					}
				}
				if (this.ShotDepert.z > 0f)
				{
					float num7 = this.Mp.GAR(base.x, base.y, this.ShotDepert.x, this.ShotDepert.y);
					if (this.t < 12f)
					{
						this.dep_agR = num7;
					}
					else
					{
						this.dep_agR = X.VALWALKANGLER(this.dep_agR, num7, 0.010053096f);
					}
				}
				else
				{
					this.walk_time -= this.TS;
				}
				if (Tk.Progress(ref this.t, 68, true))
				{
					MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunNormalShot, this.FD_MgDrawNormalShot);
					magicItem.Ray.check_hit_wall = true;
					magicItem.efpos_s = (magicItem.raypos_s = (magicItem.aimagr_calc_s = true));
					magicItem.sa = this.rotR;
					magicItem.Atk0 = this.AtkShot;
					magicItem.wind_apply_s_level = 1f;
					base.MpConsume(this.McShot, magicItem, 1f, 1f);
					magicItem.Ray.RadiusM(0.12f);
					if (this.ShotDepert.z > 0f)
					{
						magicItem.dx = this.ShotDepert.x;
						magicItem.dy = this.ShotDepert.y;
					}
					else
					{
						magicItem.phase = 1;
					}
					magicItem.PtcVar("agR", (double)magicItem.sa).PtcVar("hagR", (double)(magicItem.sa + 1.5707964f)).PtcST("uni_shot_released", PTCThread.StFollow.NO_FOLLOW, false);
					this.Phy.addFoc(FOCTYPE.WALK, -0.12f * X.Cos(magicItem.sa), 0.12f * X.Sin(magicItem.sa), -1f, 0, 1, 40, -1, 0);
					this.SpSetPose("shot_1", -1, null, false);
					this.walk_st = 0;
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t > 15f && this.walk_st == 0)
				{
					this.walk_st = 1;
					this.dep_agR += X.NIXP(0.4f, 0.6f) * 3.1415927f;
				}
				if (this.t >= 70f)
				{
					if (this.ShotDepert.z == 0f)
					{
						this.Nai.addTypeLock(NAI.TYPE.MAG, 300f);
					}
					return false;
				}
			}
			return true;
		}

		private bool MgRunNormalShot(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			Mg.sz += fcnt;
			float num = X.ZPOW(Mg.da, 30f);
			if (Mg.sz >= 3f)
			{
				Mg.sz -= 3f;
				if (Mg.phase == 0)
				{
					if (X.chkLEN(Mg.sx, Mg.sy, Mg.dx, Mg.dy, 0.08f) || (X.chkLEN(Mg.sx, Mg.sy, Mg.dx, Mg.dy, 5f) && this.Mp.canThroughBcc(Mg.sx, Mg.sy, this.Nai.target_x, this.Nai.target_y, 0.1f, -1000f, -1, false, false, null, true, null)))
					{
						Mg.phase = 1;
						Mg.t = 1f;
					}
					else
					{
						Mg.calced_aim_pos = true;
						Mg.aim_agR = this.Mp.GAR(Mg.sx, Mg.sy, Mg.dx, Mg.dy);
					}
				}
				if (Mg.phase == 1)
				{
					Mg.calcAimPos(false);
				}
				float num2 = X.NI(3, 1, num);
				Mg.sa = X.VALWALKANGLER(Mg.sa, Mg.aim_agR, X.NI(0.021f, 0.0078f, X.ZLINE(Mg.t, 24f)) * num2 * 3.1415927f);
			}
			Mg.MnSetRay(Mg.Ray, 0, Mg.sa, 0f);
			float num3 = 0.07f * fcnt * X.NI(1, 3, num);
			Mg.Ray.LenM(num3);
			float num4 = num3 * Mg.Ray.Dir.x;
			float num5 = -num3 * Mg.Ray.Dir.y;
			Mg.Atk0.BurstDir((float)X.MPF(num4 > 0f));
			HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
			if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
			{
				Mg.kill(0.125f);
				return false;
			}
			if (Mg.t >= 540f)
			{
				return false;
			}
			Mg.sx += num4;
			Mg.sy += num5;
			if ((hittype & HITTYPE.WALL_AND_BREAK) != HITTYPE.NONE)
			{
				Mg.PtcVar("sx", (double)Mg.sx).PtcVar("sy", (double)Mg.sy).PtcST("uni_shot_broken", PTCThread.StFollow.NO_FOLLOW, false);
				return false;
			}
			if (Mg.da > 0f)
			{
				Mg.da -= fcnt;
			}
			if ((hittype & HITTYPE.REFLECTED) != HITTYPE.NONE)
			{
				if (Mg.phase == 0)
				{
					Mg.phase = 1;
					Mg.t = 1f;
				}
				Mg.da = 40f;
				Mg.reflectAgR(Mg.Ray, ref Mg.sa, 0.25f);
			}
			return true;
		}

		public bool MgDrawNormalShot(MagicItem Mg, float fcnt)
		{
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
			meshImg.Col = meshImg.ColGrd.Set(4283014879U).C;
			float num = Mg.sa;
			if (Mg.da > 0f)
			{
				num += (1f - X.ZLINE(Mg.da, 40f)) * (X.COSI(Mg.t, 15.73f) * 3.1415927f * 0.03f);
			}
			float num2 = 0.5f * X.COSI(Mg.t, 3.73f) + 0.5f * X.COSI(Mg.t, 5.13f);
			float num3 = 61f + 20f * num2;
			meshImg.initForImg(MTRX.EffBlurCircle245, 0).Rect(0f, 0f, num3, num3, false);
			MeshDrawer mesh = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
			mesh.Col = mesh.ColGrd.Set(4290650157U).blend(4294957272U, X.saturate(0.4f + 0.4f * X.COSI(Mg.t, 12.4f) + 0.2f * X.COSI(Mg.t, 5.4f))).mulA(0.8f - num2 * 0.25f)
				.C;
			mesh.Poly(0f, 0f, 13f + 1.2f * num2, num, 3, 0f, false, 0f, 0f);
			return true;
		}

		protected bool runDrillAttack(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.Anm.rotationR_speed = 0f;
				base.addF((NelEnemy.FLAG)6291456);
				base.PtcVar("scl", (double)base.scaleX).PtcST("uni_drill_run", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.walk_st = 0;
				this.walk_time = 0f;
				this.AtkDrill.knockback_len = 2.7f;
				this.setNeedleTackle(this.AtkDrill, this.TkiDrill);
				this.SpSetPose("drill_0", -1, null, false);
				this.killBaseTackle();
				this.Nai.AddF(NAI.FLAG.WANDERING, -1f);
				this.CpForGroundBreaking = null;
			}
			if (this.walk_time <= 0f)
			{
				this.walk_time += 6f;
				if (Tk.prog == PROG.PROG1 && this.CpForGroundBreaking != null)
				{
					float rotR = this.rotR;
					NelNUni.DroDrillHit.dirR[0] = 3.1415927f + rotR;
					this.Mp.DropCon.setGroundBreaker(this.CpForGroundBreaking, base.x + 1.4f * X.Cos(rotR), base.y - 1.4f * X.Sin(rotR), 1f, 1f, NelNUni.DroDrillHit);
				}
				else if (Tk.prog < PROG.PROG1)
				{
					float num = this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y);
					if (init_flag)
					{
						this.Phy.addFoc(FOCTYPE.WALK, -0.013f * X.Cos(num), 0.013f * X.Sin(num), -1f, 0, 1, 22, -1, 0);
					}
					if (Tk.prog == PROG.ACTIVE)
					{
						if (this.t < 10f)
						{
							this.dep_agR = num;
						}
						else
						{
							this.dep_agR = X.VALWALK(this.dep_agR, num, 0.17907077f);
						}
					}
					else if (Tk.prog == PROG.PROG0)
					{
						this.dep_agR = X.VALWALK(this.dep_agR, num, 0.050893802f);
					}
				}
			}
			this.walk_time -= this.TS;
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 100, true))
			{
				Tk.AfterDelay(50f);
				if (!this.MgNeedle.isActive(this))
				{
					Tk.prog = PROG.PROG2;
				}
				else
				{
					if (!X.DEBUGNOSND)
					{
						this.MgNeedle.Mg.playSndInterval("uni_drill_run", 30f, base.x, base.y, 12, true, PTCThread.StFollow.FOLLOW_C).Change("uni_drill_run_loop", 43f);
					}
					base.PtcVar("scl", (double)base.scaleX).PtcST("uni_drill_attack_release", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
					this.SpSetPose("drill_1", -1, null, false);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				float rotR2 = this.rotR;
				float num2 = 0.38f * X.NI(0.4f, 1f, X.ZSIN(this.t, 11f));
				float num3 = X.Cos(rotR2);
				this.AtkDrill.BurstDir((float)X.MPF(num3 > 0f));
				if (this.MgNeedle.isActive(this) && base.hit_wall_collider && this.t >= 5f)
				{
					float num4 = base.x + (this.sizex + 0.5f) * num3;
					float num5 = base.y - (this.sizey + 0.5f) * X.Sin(rotR2);
					this.CpForGroundBreaking = this.Mp.getHardChip((int)num4, (int)num5, null, false, true, null);
					if (this.CpForGroundBreaking != null)
					{
						this.AtkDrill.knockback_len = 0.2f;
						Tk.prog = PROG.PROG1;
						this.t = 0f;
						base.PtcST("uni_drill_hit", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
						this.Nai.AddF(NAI.FLAG.ATTACKED, 200f);
						this.SpSetPose("drill_2", -1, null, false);
						Tk.AfterDelay(70f);
						this.MgNeedle.Mg.killEffect();
						this.cannot_move = true;
					}
				}
				if (Tk.prog == PROG.PROG0)
				{
					bool flag = !this.MgNeedle.isActive(this);
					if (this.t > 90f || flag || (this.t > 15f && !base.Useable(this.McDrill, 1f, 0f)) || this.Phy.isin_water)
					{
						Tk.prog = (flag ? PROG.PROG3 : PROG.PROG2);
						num2 *= ((Tk.prog == PROG.PROG3) ? (-0.6f) : 0.7f);
						this.Phy.addFoc(FOCTYPE.WALK, num2 * num3, -num2 * X.Sin(rotR2), -1f, 0, 0, 40, -1, 0);
						if (!flag)
						{
							this.MgNeedle.Mg.killEffect();
						}
					}
					else
					{
						this.Phy.addFoc(FOCTYPE.WALK, num2 * num3, -num2 * X.Sin(rotR2), -1f, -1, 1, 0, -1, 0);
					}
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, 100, true))
			{
				float rotR3 = this.rotR;
				float num6 = X.Cos(rotR3);
				this.Phy.addFoc(FOCTYPE.WALK, -0.12f * num6, 0.12f * X.Sin(rotR3), -1f, 0, 1, 60, -1, 0);
				float num7 = (float)X.MPF(num6 > 0f);
				this.dep_agR += X.NIXP(0.4f, 0.6f) * 3.1415927f * num7;
				this.Anm.rotationR_speed = 0.21991149f * num7;
				base.PtcVar("hagR", (double)(rotR3 + 1.5707964f)).PtcST("uni_drill_quit", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.cannot_move = false;
			}
			if (Tk.prog == PROG.PROG2 || Tk.prog == PROG.PROG3)
			{
				this.MgNeedle.destruct(this);
				this.SpSetPose("drill_3", -1, null, false);
				this.can_hold_tackle = false;
				base.killPtc(PtcHolder.PTC_HOLD.ACT);
				this.Nai.RemF(NAI.FLAG.ATTACKED);
				base.remF((NelEnemy.FLAG)6291456);
				this.Nai.addTypeLock(NAI.TYPE.MAG_0, 200f);
				if (Tk.prog == PROG.PROG3)
				{
					this.changeState(NelEnemy.STATE.DAMAGE);
					this.SpSetPose("damage", -1, null, false);
				}
				return false;
			}
			return true;
		}

		public override AttackInfo applyDamageFromMap(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo _Atk, float efx, float efy, bool apply_execute = true)
		{
			NelAttackInfo nelAttackInfo = base.applyDamageFromMap(MDI, _Atk, efx, efy, false) as NelAttackInfo;
			if (nelAttackInfo == null)
			{
				return null;
			}
			if (MDI.kind == MAPDMG.SPIKE)
			{
				return null;
			}
			if (!apply_execute)
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

		public override bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			if (HitMv == null && (Atk == this.AtkTackleBodyOd || Atk == this.AtkTackleBody) && (this.Ser.has(SER.TIRED) || this.Ser.has(SER.EATEN) || base.throw_ray || base.disappearing))
			{
				return false;
			}
			if (this.GClimb != null && HitMv != null && HitMv.Mv is NelEnemy && ((HitMv.Mv as NelEnemy).isOverDrive() || X.XORSP() < ((this.Absorb != null) ? 0.7f : 0.12f)))
			{
				this.GClimb.Turn();
				if (this.state == NelEnemy.STATE.STAND)
				{
					this.od_rotate_spd_ratio = X.Mn(this.od_rotate_spd_ratio + 0.66f, 4f);
				}
			}
			return base.initPublishAtk(Mg, Atk, hittype, HitMv);
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			NelNUni nelNUni = null;
			if (Atk.AttackFrom is NelNUni)
			{
				nelNUni = Atk.AttackFrom as NelNUni;
				if (nelNUni.destructed || !nelNUni.isOverDrive())
				{
					return 0;
				}
			}
			int num = base.applyDamage(Atk, force);
			if (num > 0 && this.GClimb != null && Atk.AttackFrom != null && Atk.PublishMagic != null && Atk.PublishMagic.is_normal_attack)
			{
				Vector2 vector;
				bool flag;
				if (nelNUni != null && nelNUni.isOverDrive() && (vector = this.GClimb.getVelocityDir()).x == 0f)
				{
					flag = vector.y > 0f == base.y < Atk.AttackFrom.y;
				}
				else
				{
					flag = base.x < Atk.AttackFrom.x == this.GClimb.go_to_right();
				}
				if (flag)
				{
					this.GClimb.Turn();
					M2BlockColliderContainer.BCCLine preBcc = this.GClimb.getPreBcc();
					if (this.state == NelEnemy.STATE.STAND)
					{
						this.od_rotate_spd_ratio = X.Mn(this.od_rotate_spd_ratio + 0.66f, 4f);
					}
					if (preBcc != null && preBcc._yd < 0)
					{
						this.jumpInit((float)X.MPF(base.x > Atk.AttackFrom.x) * X.NIXP(2f, 3f), 0f, X.NIXP(1.5f, 2.2f), false);
					}
				}
			}
			return num;
		}

		public override void addKnockbackVelocity(float v0, AttackInfo Atk, M2Attackable Another, FOCTYPE _foctype_add = (FOCTYPE)0U)
		{
			if (Atk is NelAttackInfo)
			{
				if ((Atk as NelAttackInfo).Caster == this && (!(Another is NelNUni) || !(Another as NelNUni).isOverDrive()))
				{
					return;
				}
				if ((v0 != 0f || base.vx == 0f) && base.isOverDrive() && this.od_rotate_spd_ratio > 1f && (base.vx == 0f || base.vx > 0f != v0 > 0f))
				{
					this.od_rotate_spd_ratio = 1f + X.Mx(0f, X.Mn(this.od_rotate_spd_ratio - 1.66f, (this.od_rotate_spd_ratio - 1f) * 0.5f));
				}
			}
			base.addKnockbackVelocity(v0, Atk, Another, _foctype_add);
		}

		public override bool runDamageSmall()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.SpSetPose("damage", 1, null, false);
				base.base_gravity = 0.66f;
			}
			base.runDamageSmall();
			if (base.hasFoot())
			{
				if (this.t >= 30f)
				{
					if (!base.isOverDrive())
					{
						base.base_gravity = 0f;
					}
					this.SpSetPose("stand", -1, null, false);
					this.t = 1f;
					this.Phy.addFoc(FOCTYPE.JUMP, 0f, -0.06f, -1f, 14, 20, 40, -1, 0);
					this.Nai.delay = 45f;
					this.dep_agR = 0f;
					return false;
				}
			}
			else
			{
				this.t = 1f;
			}
			return true;
		}

		public bool runOverDriveAppeal(NaTicket Tk)
		{
			return base.runOverDriveAppealBasic(Tk.initProgress(this), Tk, "od_appear", "od_appear2stand", 100, 130);
		}

		public bool readTicketOd(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
				break;
			case NAI.TYPE.WALK_TO_WEED:
				goto IL_005E;
			case NAI.TYPE.PUNCH:
				return this.runOdRotateAttack(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_0:
				return this.runOdSplashNeedle(Tk.initProgress(this), Tk);
			default:
				if (type == NAI.TYPE.APPEAL_0)
				{
					return this.runOverDriveAppeal(Tk);
				}
				if (type - NAI.TYPE.GAZE > 1)
				{
					goto IL_005E;
				}
				break;
			}
			Tk.after_delay = 30f;
			return false;
			IL_005E:
			return base.readTicket(Tk);
		}

		protected bool runOdRotateAttack(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_time = 0f;
				this.walk_st = 0;
				this.GClimb = new NASGroundClimber(this, 4f);
				int num = this.GClimb.FixAim(false, 1f);
				if (num != -1)
				{
					this.setAim((AIM)num, false);
				}
				if (!this.GClimb.initClimbWalk(15U))
				{
					this.GClimb = null;
					return false;
				}
				this.GClimb.alloc_jump_air = this.FootD.FootIsLift();
				this.GClimb.addChangedFn(new NASGroundClimber.FnClimbEvent(this.fnBccChanged));
				this.Nai.RemF((NAI.FLAG)48);
				if (this.SndLoopWalk == null)
				{
					this.SndLoopWalk = this.Mp.M2D.Snd.createInterval(this.snd_key, "uni_in_ground", 110f, this, 0f, 128);
				}
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.Nai.RemF(NAI.FLAG.INJECTED);
				this.fineFootType();
				this.error_cnt = 0;
				this.od_rotate_spd_ratio = 1f;
				this.recheck_fall_start = 60f;
				this.absorb_weight = 2;
				this.AtkTackleBodyOd.knockback_len = 1.2f;
				this.AtkTackleBodyOd.press_state_replace = byte.MaxValue;
				this.BaseAttackTarget = this.AtkTackleBodyOd;
				this.setBaseTackle(true, this.AtkTackleBodyOd);
			}
			bool flag = false;
			if (!this.MgTackleBody.isActive(this))
			{
				flag = true;
			}
			if (!this.progressOdRotation(flag))
			{
				if (this.Nai.HasF(NAI.FLAG.POWERED, false))
				{
					Tk.after_delay = 34f;
				}
				if (!flag)
				{
					this.setBaseTackle(true, null);
				}
				return false;
			}
			return true;
		}

		public bool fnBccChanged(M2BlockColliderContainer.BCCLine Bcc)
		{
			if (this.walk_st == 10 && CAim._YD(Bcc.aim, 1) > 0)
			{
				if (this.Absorb != null && !this.Absorb.getTargetMover().is_alive && X.XORSP() < 0.85f)
				{
					this.GClimb.Turn();
					return false;
				}
				this.walk_st = 101;
				if (this.Absorb != null)
				{
					this.Absorb.target_pose = "damage_m";
				}
				base.carryable_other_object = false;
			}
			return true;
		}

		protected bool progressOdRotation(bool quit_flag = false)
		{
			this.walk_time += this.TS;
			this.SpSetPose(base.hasFoot() ? "od_rotate" : "od_fall", -1, null, false);
			if (this.walk_st >= 200)
			{
				this.AtkTackleBodyOd.press_state_replace = byte.MaxValue;
				if (this.GClimb != null)
				{
					if (this.SndLoopWalk != null)
					{
						this.SndLoopWalk.destruct();
						this.SndLoopWalk = null;
					}
					this.Anm.rotationR_speed = this.GClimb.getVelocityDir().x * 0.02f * 3.1415927f;
					base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
					this.GClimb.quitClimbWalk();
					this.GClimb = null;
					this.fineFootType();
					this.killBaseTackle();
					if (!base.hasFoot())
					{
						this.setNeedleTackle(this.AtkOdTackleBodyFall, this.TkiOdTackleBodyFall);
					}
				}
				return !base.hasFoot();
			}
			if (this.GClimb == null)
			{
				return false;
			}
			float num = 0.04f;
			if (this.walk_st >= 100)
			{
				quit_flag = true;
			}
			else if (this.recheck_fall_start > 0f)
			{
				this.recheck_fall_start = X.VALWALK(this.recheck_fall_start, 0f, this.TS);
			}
			this.FootD.lockPlayFootStamp(5);
			M2BlockColliderContainer.BCCLine preBcc = this.GClimb.getPreBcc();
			num = num * X.ZLINE(this.walk_time, 40f) + 0.050000004f * X.ZLINE(this.walk_time - 120f, 100f) * this.od_rotate_spd_ratio;
			if (this.walk_st == 0 && !quit_flag && preBcc != null && this.state == NelEnemy.STATE.STAND)
			{
				int num2 = CAim._YD(preBcc.aim, 1);
				if (this.recheck_fall_start <= 0f)
				{
					if (num2 >= 0 && X.Abs(base.x - this.Nai.target_x - X.NI(-2.5f, 2.5f, X.XORSP())) < 1f * X.Mx(1f, num * 25f))
					{
						this.recheck_fall_start = 50f;
						if (X.XORSP() < ((num2 < 0) ? 0.1f : 0.36f) && base.mbottom + 1.5f < this.Nai.target_y)
						{
							float num3 = (float)(-(float)CAim._XD(preBcc.aim, 1)) * 0.5f;
							if (this.Mp.canThroughBcc(base.x + num3, base.y, base.x + num3, this.Nai.target_y, this.sizex + 0.0625f, 0.01f, -1, false, false, null, true, null))
							{
								quit_flag = true;
								this.walk_st = 100;
							}
						}
					}
					else
					{
						this.recheck_fall_start = 10f;
					}
				}
			}
			if (quit_flag)
			{
				if (this.recheck_fall_start >= 0f)
				{
					this.recheck_fall_start = 0f;
				}
				this.recheck_fall_start -= this.TS;
				num += this.recheck_fall_start * 0.005f;
				if (num < 0f)
				{
					this.walk_st = 200;
					return true;
				}
			}
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			this.AtkTackleBodyOd.press_state_replace = (byte)((footBCC == null) ? ((AIM)255U) : footBCC.foot_aim);
			if (!this.GClimb.progressClimbWalk(num))
			{
				this.walk_st = 200;
				return true;
			}
			if (this.Mp.floort >= this.eff_t && preBcc != null)
			{
				this.eff_t = this.Mp.floort + 16f;
				this.GClimb.alloc_jump_air = this.Nai.target_lastfoot_bottom > base.y && X.XORSP() < (preBcc.is_lift ? 0.55f : 0.33f);
				int num4 = CAim._YD(preBcc.aim, 1);
				int num5 = ((num4 == 0) ? CAim._XD(preBcc.aim, 1) : 0);
				base.PtcVar("x", (double)(base.x + (float)num5 * this.sizex)).PtcVar("y", (double)(base.y - (float)num4 * this.sizey)).PtcVar("agR", (double)CAim.get_agR(preBcc.aim, 0f))
					.PtcST("uni_od_ground_dig", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				if (this.state != NelEnemy.STATE.ABSORB && X.XORSP() < 0.5f)
				{
					float num6 = CAim.get_agR(CAim.get_opposite(preBcc.aim), 0f);
					NelNUni.DroOdGroundPound.dirR[0] = num6 - 1.2566371f;
					NelNUni.DroOdGroundPound.dirR[1] = num6 + 1.2566371f;
					this.Mp.DropCon.setGroundBreaker(base.x + (float)num5 * (this.sizex + 0.25f), base.y - (float)num4 * (this.sizey + 0.25f), base.x + (float)num5 * this.sizex * 0.8f, base.y - (float)num4 * this.sizey * 0.8f, (float)X.Abs(num4), (float)X.Abs(num5), NelNUni.DroOdGroundPound);
				}
			}
			return true;
		}

		protected override bool setLandPose()
		{
			if (base.setLandPose())
			{
				this.Anm.rotationR_speed = 0f;
				this.Anm.rotationR = 0f;
				this.Nai.delay += (float)(this.Nai.HasF(NAI.FLAG.OVERDRIVED, false) ? 30 : 60);
				return true;
			}
			return false;
		}

		public override bool fineFootType()
		{
			if (this.GClimb != null)
			{
				this.FootD.footstamp_type = FOOTSTAMP.NONE;
				return false;
			}
			return base.fineFootType();
		}

		protected void setEDforSplashNeedle()
		{
			if (this.EDSplashNeedle == null)
			{
				if (this.FD_fnDrawSplashNeedle == null)
				{
					this.FD_fnDrawSplashNeedle = new M2DrawBinder.FnEffectBind(this.fnDrawSplashNeedle);
				}
				this.EDSplashNeedle = this.Mp.setED("uni-needle", this.FD_fnDrawSplashNeedle, 0f);
			}
		}

		protected void killEDforSplashNeedle()
		{
			if (this.EDSplashNeedle != null && this.Mp != null)
			{
				this.EDSplashNeedle = this.Mp.remED(this.EDSplashNeedle);
				int count = this.TkiSplashNeedle._count;
				float num = (this.TkiSplashNeedle.calc_difx_map(this) + this.TkiSplashNeedle.radius) * base.CLENM;
				for (int i = 0; i < count; i++)
				{
					float osnagR = this.getOSNagR(i);
					base.PtcVar("agR", (double)osnagR).PtcVar("len", (double)num).PtcST("uni_od_splash_needle_disappear", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				base.MpConsume(this.McsSplashNeedle, null, 1f, 1f);
				if (!this.Nai.isPrGaraakiState())
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 140f);
				}
			}
			if (this.EDTSplashNeedle != null)
			{
				this.EDTSplashNeedle = this.Mp.remED(this.EDTSplashNeedle);
			}
		}

		public void fineAngle()
		{
			if (base.isOverDrive() && base.hasFoot() && !this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE))
			{
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC != null)
				{
					this.Anm.rotationR = footBCC.housenagR - 1.5707964f;
					this.Anm.rotationR_speed = 0f;
				}
			}
		}

		protected bool runOdSplashNeedle(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_time = 0f;
				if (this.GClimb != null)
				{
					this.GClimb.quitClimbWalk();
					this.GClimb = null;
				}
				this.error_cnt = X.xors(16777215);
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
				base.PtcVar("_t", (double)this.TkiSplashNeedle._prepare_delay).PtcST("uni_od_splash_needle_first", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.setEDforSplashNeedle();
				this.SpSetPose("od_appear", -1, null, false);
				this.absorb_weight = 5;
				this.BaseAttackTarget = this.AtkTackleBody;
				this.fineAngle();
				this.killBaseTackle();
			}
			this.walk_time += this.TS;
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, (int)this.TkiSplashNeedle._prepare_delay, true))
			{
				this.SpSetPose("od_splash_hold", -1, null, false);
				this.killBaseTackle();
				int count = this.TkiSplashNeedle._count;
				for (int i = 0; i < count; i++)
				{
					float osnagR = this.getOSNagR(i);
					base.PtcVar("cx", (double)(base.x + this.sizex * 0.5f * X.Cos(osnagR))).PtcVar("cy", (double)(base.y - this.sizex * 0.5f * X.Sin(osnagR))).PtcVar("i", (double)i)
						.PtcVar("agR", (double)osnagR)
						.PtcST((i == 0) ? "uni_od_splash_needle_start_first" : "uni_od_splash_needle_start", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				}
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, (int)this.TkiSplashNeedle._start_delay, true))
			{
				this.SpSetPose("od_splash_appear", -1, null, false);
				this.walk_time = 0f;
				this.setOdSplashNeedle(false, true);
				this.cannot_move = true;
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, (int)this.TkiSplashNeedle._hold, true))
			{
				this.can_hold_tackle = false;
				this.SpSetPose("od_appear2stand", -1, null, false);
				this.cannot_move = false;
				this.killEDforSplashNeedle();
				Tk.AfterDelay((float)((int)this.TkiSplashNeedle._after_delay));
				return false;
			}
			return true;
		}

		public void setOdSplashNeedle(bool hold_forever = false, bool set_tackle = true)
		{
			int count = this.TkiSplashNeedle._count;
			float num = ((!base.Useable(this.McsSplashNeedle, 1f, 0f)) ? 0.5f : 1f);
			for (int i = 0; i < count; i++)
			{
				float osnagR = this.getOSNagR(i);
				base.PtcVar("_hold", (double)(hold_forever ? (-1f) : this.TkiSplashNeedle._hold)).PtcVar("i", (double)i).PtcVar("agR", (double)osnagR)
					.PtcST((i == 0) ? "uni_od_splash_needle_appear_first" : "uni_od_splash_needle_appear", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				if (set_tackle)
				{
					MagicItem magicItem = base.tackleInit(this.AtkOdSplashNeedle, this.TkiSplashNeedle);
					float num2 = X.Abs(magicItem.sx) * num;
					magicItem.sx = num2 * X.Cos(osnagR);
					magicItem.sy = -num2 * X.Sin(osnagR);
				}
			}
		}

		protected float getOSNagR(int _id)
		{
			return 3.1415927f * (0.5f + 0.58f * (-1f + 2f * ((float)_id + 0.5f + X.NI(-0.3f, 0.3f, X.RAN((uint)this.error_cnt, 2441))) / (float)this.TkiSplashNeedle._count));
		}

		public bool isUniPublish(AbsorbManager Abm)
		{
			return Abm.getPublishMover() is NelNUni && Abm.getPublishMover() != this;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			if (!X.SENSITIVE && base.isOverDrive())
			{
				A.Add("torture_mkb_urchin");
			}
			Aattr.Add(MGATTR.STAB);
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (!base.isOverDrive() || this.state != NelEnemy.STATE.STAND || this.Absorb != null || Abm.Con.current_pose_priority >= 20 || Abm.Con.getSpecificPublisher(this.FD_isUniPublish) != null)
			{
				return false;
			}
			M2Attackable m2Attackable = MvTarget as M2Attackable;
			if (m2Attackable == null)
			{
				return false;
			}
			M2FootManager footManager = m2Attackable.getFootManager();
			if (footManager == null)
			{
				return false;
			}
			IFootable foot = footManager.get_Foot();
			if (foot != null && !(foot is M2BlockColliderContainer.BCCLine))
			{
				return false;
			}
			if (this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE))
			{
				if (this.GClimb == null)
				{
					return false;
				}
				int num = 0;
				if ((double)m2Attackable.mbottom < (double)base.y - 0.2 && m2Attackable.getFootManager() != null)
				{
					num = -1;
					Abm.uipicture_fade_key = "torture_mkb_urchin";
				}
				else if (this.GClimb.getVelocityDir().y >= 0f && m2Attackable.hasFoot() && (double)m2Attackable.mbottom > (double)base.mbottom - 0.5)
				{
					num = 1;
				}
				if (num == 0 || !base.initAbsorb(Atk, MvTarget, Abm, penetrate))
				{
					return false;
				}
				if (num == 1 && X.XORSP() < (m2Attackable.is_alive ? 0.1f : 0.02f))
				{
					num = -1;
				}
				Abm.pose_priority = 20;
				this.AtkTackleBodyOd._apply_knockback_current = false;
				this.walk_st = ((num == -1) ? 10 : 11);
				this.od_rotate_spd_ratio = 1f;
				Abm.target_pose = ((num == -1) ? "stride" : "injectrided");
				Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 10, 63U);
				Abm.get_Gacha().SoloPositionPixel = new Vector3(0f, -38f, 0f);
				Abm.no_clamp_speed = true;
				Abm.no_change_release_in_dead = true;
				Abm.no_shuffle_aim = true;
				m2Attackable.getPhysic().killSpeedForce(true, true, true, false, false);
				if (num == -1)
				{
					base.carryable_other_object = true;
					m2Attackable.getFootManager().rideInitTo(this, false);
					if (X.XORSP() < 0.2f)
					{
						m2Attackable.TeCon.setQuakeSinV(10f, 30, X.NIXP(22f, 41f), 0f, 0);
					}
				}
				else
				{
					this.Anm.showToFront(true, false);
				}
			}
			else
			{
				if (!this.Nai.isFrontType(NAI.TYPE.PUNCH_0, PROG.ACTIVE))
				{
					return false;
				}
				if (!base.Useable(this.McsSplashNeedle, 1f, 0f) || Atk != this.AtkOdSplashNeedle)
				{
					return false;
				}
				float num2 = this.TkiSplashNeedle.calc_difx_map(this);
				if (!X.chkLEN(base.x, base.y, m2Attackable.x, m2Attackable.y, num2 * 0.75f))
				{
					return false;
				}
				if (!base.initAbsorb(Atk, MvTarget, Abm, penetrate))
				{
					return false;
				}
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
				float num3 = -1f;
				float num4 = this.Mp.GAR(base.x, base.y, m2Attackable.x, m2Attackable.y);
				for (int i = this.TkiSplashNeedle._count - 1; i >= 0; i--)
				{
					float osnagR = this.getOSNagR(i);
					float num5 = X.Abs(X.angledifR(num4, osnagR));
					if (num3 == -1f || num5 < num3)
					{
						num3 = num5;
						this.walk_st = 1000 + i;
					}
				}
				if (num4 > 1.727876f)
				{
					this.Absorb.setKirimomiReleaseDir(0);
				}
				else if (num4 < 1.4137168f)
				{
					this.Absorb.setKirimomiReleaseDir(2);
				}
				Abm.pose_priority = 20;
				Abm.no_clamp_speed = true;
				Abm.target_pose = ((CAim._XD(m2Attackable.aim, 1) > 0 == num4 < 1.5707964f) ? "absorb_stabbed_b" : "absorb_stabbed");
				Abm.kirimomi_release = true;
				Abm.no_change_release_in_dead = true;
				Abm.no_shuffle_aim = true;
				Abm.no_shuffleframe_on_applydamage = true;
				m2Attackable.getPhysic().killSpeedForce(true, true, true, false, false);
				base.carryable_other_object = true;
			}
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			bool flag = false;
			if (this.walk_st < 1000)
			{
				if (pr == null || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.Absorb.isNotTortureEnable() || !this.canAbsorbContinue())
				{
					flag = true;
				}
				bool flag2 = this.t <= 20f;
				if (flag2)
				{
					this.t = 100f;
				}
				if (this.t < 120f)
				{
					if (!flag && this.walk_time >= 0f)
					{
						if (this.t >= 100f && this.GClimb != null && (flag2 || pr.isCoveringMv(this, -0.03f, (this.walk_st < 0) ? 0.4f : (-0.1f))))
						{
							float num = 18f * ((X.XORSP() < 0.2f) ? X.NIXP(0.13f, 0.3f) : X.NIXP(0.8f, 1.6f));
							string text = "";
							if (this.walk_st == 10 && !pr.getFootManager().FootIs(this))
							{
								this.walk_st = 101;
							}
							if (this.walk_st == 10)
							{
								text = "stride";
								if (pr == base.M2D.Cam.getBaseMover())
								{
									base.M2D.Cam.Qu.SinH(11f * X.NIXP(0.2f, 1f), 20f, 0.2f, 0);
								}
							}
							else if (this.walk_st == 101)
							{
								pr.getPhysic().addFoc(FOCTYPE.DAMAGE | FOCTYPE._GRAVITY_LOCK | FOCTYPE._INDIVIDUAL, 0f, 0.013f, -1f, 0, 3, 20, -1, 0);
								text = "damage_m";
							}
							else if (this.walk_st == 11)
							{
								text = "injectrided";
								pr.getPhysic().addFocToSmooth(FOCTYPE.DAMAGE | FOCTYPE._INDIVIDUAL, this.Phy.move_depert_x + base.mpf_is_right * (this.sizex * 0.3f + 0.71999997f), pr.y, (int)num + 10, -1, 0, -1f);
								if (pr == base.M2D.Cam.getBaseMover())
								{
									base.M2D.Cam.Qu.SinV(6f * X.NIXP(0.2f, 1f), 20f, 0.2f, 0);
								}
							}
							base.applyAbsorbDamageTo(pr, this.AtkAbsorbOd, true, false, true, 0f, false, (X.XORSP() < 0.9f) ? this.Absorb.uipicture_fade_key : null, false);
							base.PtcVar("cy", (double)(base.mbottom - 0.4f)).PtcST("ground_pound_attacked", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							this.Mp.DropCon.setBlood(pr, 7, MTR.col_blood, 0f, true);
							if (TX.valid(text) && this.Absorb.Con.current_pose_priority == this.Absorb.pose_priority && (!pr.poseIs(text) || this.walk_st == 10 || this.walk_st == 11))
							{
								pr.setPoseByEn(text, 1, null, false);
							}
							base.runAbsorb();
							if (this.walk_st == 1 || this.walk_st == -1)
							{
								pr.getAnimator().randomizeFrame();
							}
							this.t = 100f - num;
						}
					}
					else
					{
						this.t = 120f;
					}
				}
				else
				{
					if (this.walk_st == 11)
					{
						this.walk_st = 100;
						if (pr != null && pr.poseIs("injectrided"))
						{
							pr.setPoseByEn("downdamage_t", -1, null, false);
						}
					}
					else if (this.walk_st == 10)
					{
						this.walk_st = 101;
						if (pr != null && pr.poseIs("stride"))
						{
							this.Absorb.target_pose = "damage_m";
						}
					}
					flag = true;
					base.carryable_other_object = false;
					if (this.t >= 135f && this.Absorb.no_clamp_speed)
					{
						this.Absorb.releaseFromPublish(this);
					}
				}
				return this.progressOdRotation(flag);
			}
			if (pr == null || this.Absorb.Con.use_torture || !base.hasFoot())
			{
				flag = true;
			}
			M2Phys physic = pr.getPhysic();
			physic.addLockGravityFrame(4);
			if (this.t <= 100f)
			{
				this.t = (float)(200 + X.xors(140));
				base.applyAbsorbDamageTo(pr, this.AtkOdSplashNeedle, true, false, true, 0f, false, null, false);
				physic.killSpeedForce(true, true, true, false, false);
				this.walk_time = 0f;
				base.carryable_other_object = true;
				pr.getFootManager().rideInitTo(this, false);
				float num2 = this.TkiSplashNeedle.calc_difx_map(this) * 0.5f;
				float osnagR = this.getOSNagR(this.walk_st - 1000);
				physic.addFoc(FOCTYPE.CARRY, (base.x + num2 * X.Cos(osnagR) - pr.x) * 0.25f, (base.y - num2 * X.Sin(osnagR) - pr.y) * 0.25f, -1f, 0, 4, 0, -1, 0);
				this.Mp.DropCon.setBlood(pr, 57, MTR.col_blood, 0f, true);
				base.M2D.Cam.Qu.HandShake(40f, 50f, 19f, 0);
				pr.TeCon.setColorBlink(1f, 40f, 0.5f, 16711680, 0);
			}
			else if (this.walk_time < 100f)
			{
				float num3 = this.TkiSplashNeedle.calc_difx_map(this) * (0.5f + (X.ZSIN(this.walk_time, 30f) - X.ZCOS(this.walk_time - 30f, 70f) * 0.5f) * 0.28f);
				float osnagR2 = this.getOSNagR(this.walk_st - 1000);
				physic.addFoc(FOCTYPE.CARRY, (base.x + num3 * X.Cos(osnagR2) - pr.x) * 0.004f, (base.y - num3 * X.Sin(osnagR2) - pr.y) * 0.004f, -1f, -1, 1, 0, -1, 0);
			}
			if (pr.is_alive && this.t >= 450f)
			{
				flag = true;
			}
			this.walk_time += this.TS;
			if (this.t >= 500f)
			{
				pr.setPoseByEn("absorb_stabbed_3", 1, null, false);
				if (X.XORSP() < 0.3f)
				{
					this.Mp.DropCon.setBlood(pr, 15, MTR.col_blood, 0f, true);
				}
				if (X.XORSP() < 0.2f)
				{
					base.M2D.Cam.Qu.HandShake(30f, 90f, X.NIXP(5f, 11f), 0);
					pr.TeCon.setColorBlink(1f, 20f, 0.3f, 16711680, 0);
				}
				this.t = 500f - X.NIXP(70f, 200f);
				if (!pr.is_alive && this.walk_time >= 900f && X.XORSP() < 0.1f)
				{
					flag = true;
				}
			}
			else
			{
				if (X.XORSP() < 0.02f)
				{
					this.Mp.DropCon.setBlood(pr, 15, MTR.col_blood, 0f, true);
				}
				if (X.XORSP() < 0.04f)
				{
					base.M2D.Cam.Qu.HandShake(2f, 20f, X.NIXP(3f, 5f), 0);
					pr.TeCon.setColorBlink(1f, 14f, 0.14f, 16711680, 0);
				}
			}
			if (flag)
			{
				base.carryable_other_object = false;
				base.killPtc();
				this.SpSetPose("od_appear2stand", -1, null, false);
				this.Nai.delay = 90f;
				this.killEDforSplashNeedle();
				return false;
			}
			return true;
		}

		public override IFootable isCarryable(M2FootManager FootD)
		{
			PR pr = FootD.Mv as PR;
			if (!(pr != null) || !base.carryable_other_object || this.state != NelEnemy.STATE.ABSORB || !base.isCoveringMv(pr, 1f, 1f))
			{
				return null;
			}
			if (this.Absorb == null || !(this.Absorb.getTargetMover() == pr) || this.walk_st != 10)
			{
				return null;
			}
			return this;
		}

		public override float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy)
		{
			if (FootD.Mv as PR != null && base.carryable_other_object && this.state == NelEnemy.STATE.ABSORB)
			{
				dx = this.Phy.move_depert_x - x;
				dy = this.Phy.move_depert_y - 0.88f * this.sizey - y;
				return 0.1f;
			}
			return base.fixToFootPos(FootD, x, y, out dx, out dy);
		}

		public override bool readPtcScript(PTCThread rER)
		{
			if (base.destructed || this.PtcHld == null)
			{
				return rER.quitReading();
			}
			string cmd = rER.cmd;
			if (cmd != null && cmd == "%ANGLE")
			{
				rER.Def("agR", this.rotR);
				return true;
			}
			return base.readPtcScript(rER);
		}

		public void fnFineUniFrame(EnemyFrameDataBasic nF, PxlFrame F)
		{
			if (base.isOverDrive())
			{
				return;
			}
			int num = -1;
			int num2 = F.countLayers();
			if (!NelNUni.Olayer2pos.TryGetValue(F, out num))
			{
				int num3 = -1;
				for (int i = 0; i < num2; i++)
				{
					PxlLayer layer = F.getLayer(i);
					if (layer.name == "Layer_s")
					{
						num3 = i;
						if (num2 - num3 < 8)
						{
							X.de("レイヤー個数配置エラー: " + F.ToString(), null);
							num3 = -2;
							break;
						}
					}
					else if (num3 >= 0)
					{
						int num4 = i - num3;
						if (num4 >= 8)
						{
							break;
						}
						if (layer.name != "Layer_s_" + (num4 + 1).ToString())
						{
							X.de("Layer レイヤー配置エラー: " + layer.ToString(), null);
							num3 = -2;
							break;
						}
					}
				}
				num = (NelNUni.Olayer2pos[F] = num3);
			}
			if (num < 0)
			{
				this.Anm.layer_mask = uint.MaxValue;
				return;
			}
			uint num5 = 0U;
			int num6 = X.IntR((this.enlarge_level - 1f) * 8f);
			for (int j = 0; j < num2; j++)
			{
				if (j < num)
				{
					num5 |= 1U << j;
				}
				else
				{
					int num7 = j - num;
					if (num7 >= 8)
					{
						num5 |= 1U << j;
					}
					else
					{
						num7 = NelNUni.Alayer_link[num7 % 8];
						if (num7 <= num6)
						{
							num5 |= 1U << j;
						}
					}
				}
			}
			this.Anm.layer_mask = num5;
		}

		public override float getEnlargeLevel()
		{
			if (base.isOverDrive())
			{
				return base.getEnlargeLevel();
			}
			float mp_ratio = base.mp_ratio;
			return 1f + (float)((int)(X.ZLINE(mp_ratio, this.enlarge_maximize_mp_ratio) * 8f)) / 8f;
		}

		public override void fineEnlargeScale(float r = -1f, bool set_effect = false, bool resize_moveby = false)
		{
			base.fineEnlargeScale(r, set_effect, false);
			this.setBaseTackle(false, null);
		}

		private bool fnDrawSplashNeedle(EffectItem Ef, M2DrawBinder ED)
		{
			if (!this.is_alive || base.destructed || !base.isOverDrive())
			{
				this.killEDforSplashNeedle();
				return false;
			}
			Ef.x = base.x;
			Ef.y = base.y;
			float num = 0f;
			int count = this.TkiSplashNeedle._count;
			if (this.state == NelEnemy.STATE.STAND)
			{
				NaTicket curTicket = this.Nai.getCurTicket();
				if (curTicket == null || curTicket.type != NAI.TYPE.PUNCH_0)
				{
					this.killEDforSplashNeedle();
					return false;
				}
				if (curTicket.prog == PROG.ACTIVE)
				{
					num = -1f + X.ZSIN(this.t, this.TkiSplashNeedle._prepare_delay - 60f) * 0.8f + 0.199f * X.ZSIN(this.t - (this.TkiSplashNeedle._prepare_delay - 60f), 60f);
				}
				else if (curTicket.prog != PROG.PROG1)
				{
					return true;
				}
			}
			float num2 = (this.TkiSplashNeedle.calc_difx_map(this) + this.TkiSplashNeedle.radius + 0.6f) * base.CLENM * ((this.state == NelEnemy.STATE.STAND && !base.Useable(this.McsSplashNeedle, 1f, 0f)) ? 0.5f : 1f);
			if (num < 0f)
			{
				num2 *= X.ZSIN(this.t, 90f);
				MeshDrawer mesh = Ef.GetMesh("", MTRX.MtrMeshAdd, false);
				mesh.Col = mesh.ColGrd.Set(4294910594U).mulA(0.5f + X.COSIT(14.2f) * 0.1f + X.COSIT(5.3f) * 0.1f).C;
				for (int i = 0; i < count; i++)
				{
					float num3 = this.getOSNagR(i) + 19.477875f * (float)X.MPF(i % 2 == 0) * -num;
					mesh.Line(0f, 0f, num2 * X.Cos(num3), num2 * X.Sin(num3), 1f, false, 0f, 0f);
				}
			}
			else
			{
				num = 0.5f + 0.5f * X.ZSIN2(this.walk_time, this.TkiSplashNeedle._hold * 0.25f);
				num2 *= num * 0.015625f;
				float num4 = 18f * num * 0.015625f;
				MeshDrawer mesh2 = Ef.GetMesh("", MTRX.MtrMeshAdd, true);
				MeshDrawer mesh3 = Ef.GetMesh("", MTRX.MtrMeshSub, true);
				mesh3.Col = C32.d2c(4284707961U);
				mesh2.Col = C32.d2c(4294583093U);
				for (int j = 0; j < count; j++)
				{
					uint ran = X.GETRAN2(IN.totalframe / 4, 7 + j * 3);
					M2MoverPr m2MoverPr = null;
					float num5 = X.NI(3, 5, X.RAN(ran, 1983)) * 0.015625f * num;
					float num6 = X.NI(5, 13, X.RAN(ran, 2145)) * 0.015625f;
					MeshDrawer meshDrawer = null;
					float num7;
					if (this.state == NelEnemy.STATE.ABSORB && this.walk_st - 1000 == j && base.AimPr != null)
					{
						m2MoverPr = base.AimPr;
						num7 = this.Mp.GAR(base.x, base.y, m2MoverPr.drawx_map, m2MoverPr.drawy_map);
						meshDrawer = Ef.GetMesh("", MTRX.MtrMeshSub, false);
						meshDrawer.Col = mesh3.Col;
					}
					else
					{
						num7 = this.getOSNagR(j);
					}
					mesh3.Tri(0, 1, 2, false).Tri(0, 2, 3, false);
					mesh2.Tri(0, 3, 1, false).Tri(1, 3, 4, false).Tri(2, 1, 5, false)
						.Tri(1, 4, 5, false);
					mesh3.Identity().Rotate(num7, false);
					mesh2.setCurrentMatrix(mesh3.getCurrentMatrix(), false);
					mesh3.Pos(0f, num4, null).Pos(num2, 0f, null).Pos(0f, -num4, null)
						.Pos(-num4, 0f, null);
					mesh2.Pos(0f, num4, null).Pos(num2, 0f, null).Pos(0f, -num4, null)
						.Pos(0f, num4 + num5, null)
						.Pos(num2 + num6, 0f, null)
						.Pos(0f, -num4 - num5, null);
					if (meshDrawer != null)
					{
						meshDrawer.setCurrentMatrix(mesh3.getCurrentMatrix(), false);
						float num8 = num4 * 0.5f;
						float num9 = X.LENGTHXY(base.x, base.y, m2MoverPr.drawx_map, m2MoverPr.drawy_map) * base.CLENM * 0.015625f;
						meshDrawer.TriRectBL(0).Pos(num9 * 0.25f, 0f, null).Pos(num9 - num8 * 0.5f, num8, null)
							.Pos(num9, 0f, null)
							.Pos(num9 - num8 * 0.5f, -num8, null);
					}
				}
			}
			return true;
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
			if (this.MgTackleBody.isActive(this) && Mg == this.MgTackleBody.Mg)
			{
				return this.state == NelEnemy.STATE.STAND;
			}
			return this.can_hold_tackle;
		}

		public float rotR
		{
			get
			{
				if (this.Anm == null)
				{
					return 0f;
				}
				if (this.Anm.getAim() != 0)
				{
					return this.Anm.rotationR;
				}
				return 3.1415927f + this.Anm.rotationR;
			}
		}

		public override bool canApplyTiredInSuperArmor(NelAttackInfo Atk)
		{
			return base.canApplyTiredInSuperArmor(Atk) && (base.destructed || base.isOverDrive() || !this.Nai.isFrontType(NAI.TYPE.MAG_0, PROG.PROG0));
		}

		private float normal_walk_spd = 0.08f;

		protected NelAttackInfo AtkTackleBody = new NelAttackInfo
		{
			hpdmg0 = 4,
			split_mpdmg = 2,
			burst_vx = 0.1f,
			burst_center = 0.01f,
			knockback_len = 1.8f,
			huttobi_ratio = 0.02f,
			attr = MGATTR.STAB,
			Beto = BetoInfo.Normal,
			nodamage_time = 24,
			shield_success_nodamage = 24f,
			parryable = true
		}.Torn(0.02f, 0.09f);

		protected NOD.TackleInfo TkiBody = NOD.getTackle("uni_body");

		protected NelAttackInfo AtkPunchWeed = new NelAttackInfo
		{
			hpdmg0 = 2,
			burst_vx = 0.02f,
			huttobi_ratio = -100f,
			attr = MGATTR.STAB,
			Beto = BetoInfo.Normal,
			parryable = true
		}.Torn(0.02f, 0.08f);

		protected NOD.TackleInfo TkiPunchWeed = NOD.getTackle("uni_punch_weed");

		protected NelAttackInfo AtkShot = new NelAttackInfo
		{
			hpdmg0 = 15,
			split_mpdmg = 6,
			burst_vx = 0.04f,
			huttobi_ratio = 0.01f,
			attr = MGATTR.STAB,
			Beto = BetoInfo.Normal,
			parryable = true
		}.Torn(0.04f, 0.08f);

		protected NOD.MpConsume McShot = NOD.getMpConsume("uni_shot");

		private const float knockback_drill_moving = 2.7f;

		protected NelAttackInfo AtkDrill = new NelAttackInfo
		{
			hpdmg0 = 2,
			split_mpdmg = 5,
			burst_vx = 0.23f,
			burst_vy = 0.02f,
			huttobi_ratio = 0.04f,
			knockback_len = 2.7f,
			attr = MGATTR.STAB,
			Beto = BetoInfo.Normal,
			shield_break_ratio = 7f,
			nodamage_time = 6,
			shield_success_nodamage = 4f,
			parryable = true
		}.Torn(0.03f, 0.05f);

		protected NOD.TackleInfo TkiDrill = NOD.getTackle("uni_drill");

		protected NOD.MpConsume McDrill = NOD.getMpConsume("uni_drill");

		private const float drill_spd = 0.38f;

		private const float knockback_od_tackle = 1.2f;

		protected NelAttackInfo AtkTackleBodyOd = new NelAttackInfo
		{
			is_grab_attack = true,
			hpdmg0 = 5,
			split_mpdmg = 2,
			mpdmg0 = 4,
			burst_vx = 0.18f,
			burst_center = 0.01f,
			shield_break_ratio = 1.5f,
			knockback_len = 1.2f,
			huttobi_ratio = 0.2f,
			attr = MGATTR.STAB,
			nodamage_time = 14,
			shield_success_nodamage = 14f,
			Beto = BetoInfo.Normal,
			parryable = true
		}.Torn(0.02f, 0.2f);

		protected NelAttackInfo AtkOdTackleBodyFall = new NelAttackInfo
		{
			hpdmg0 = 10,
			split_mpdmg = 2,
			mpdmg0 = 4,
			shield_break_ratio = 2.5f,
			burst_vx = 0.18f,
			burst_vy = -0.04f,
			burst_center = 0.01f,
			huttobi_ratio = 8.2f,
			attr = MGATTR.STAB,
			Beto = BetoInfo.Normal,
			press_state_replace = 3,
			parryable = true
		}.Torn(0.02f, 0.2f);

		protected NOD.TackleInfo TkiOdTackleBodyFall = NOD.getTackle("uni_body_od_fall");

		private static M2DropObjectReader DroDrillHit;

		private static M2DropObjectReader DroOdGroundPound;

		protected NelAttackInfo AtkAbsorbOd = new NelAttackInfo
		{
			hpdmg0 = 3,
			mpdmg0 = 3,
			burst_vx = 0f,
			attr = MGATTR.STAB,
			Beto = BetoInfo.Normal,
			huttobi_ratio = -100f,
			EpDmg = new EpAtk(6, "uni")
			{
				vagina = 1,
				canal = 6
			}
		}.Torn(0.02f, 0.05f);

		protected NelAttackInfo AtkOdSplashNeedle = new NelAttackInfo
		{
			absorb_replace_prob_both = 100f,
			hpdmg0 = 19,
			split_mpdmg = 32,
			huttobi_ratio = 100f,
			shield_break_ratio = 40f,
			burst_vx = 0.56f,
			burst_vy = -0.06f,
			attr = MGATTR.STAB,
			Beto = BetoInfo.Normal,
			aim_to_opposite_when_pr_dieing = 0f,
			pee_apply100 = 5f,
			parryable = true
		}.Torn(0.4f, 0.4f);

		protected NOD.TackleInfo TkiSplashNeedle = NOD.getTackle("uni_od_splash_needle");

		protected NOD.MpConsume McsSplashNeedle = NOD.getMpConsume("uni_od_splash_needle");

		protected const float od_rotate_spd = 0.04f;

		protected const float od_rotate_spd_fast = 0.09f;

		protected NOD.TackleInfo TkiBodyOd = NOD.getTackle("uni_body_od");

		private static readonly int[] Alayer_link = new int[] { 1, 6, 7, 3, 5, 8, 4, 2 };

		private static BDic<PxlFrame, int> Olayer2pos;

		protected MagicItemHandlerS MgTackleBody;

		protected MagicItemHandlerS MgNeedle;

		private NelAttackInfo BaseAttackTarget;

		private NOD.TackleInfo TkiNeedleSetted;

		private const int ball_count = 8;

		private float dep_agR;

		private int error_cnt;

		private float eff_t;

		private const int absorb_weight_rotating = 2;

		private const int absorb_weight_stab = 5;

		private Vector3 ShotDepert;

		private M2SndInterval SndLoopWalk;

		private M2Chip CpForGroundBreaking;

		private M2DrawBinder EDSplashNeedle;

		private M2DrawBinder EDTSplashNeedle;

		private MagicItem.FnMagicRun FD_MgRunNormalShot;

		private MagicItem.FnMagicRun FD_MgDrawNormalShot;

		private NASAirChaser NasChaser;

		private const int PRI_WALK = 200;

		private int bench_marked;

		private NASGroundClimber GClimb;

		private float od_rotate_spd_ratio = 1f;

		private const int WST_NORMAL = 0;

		private const int WST_STRIDE = 10;

		private const int WST_RIDEROT = 11;

		private const int WST_STOPPING = 100;

		private const int WST_STOPPING_STRIDE = 101;

		private const int WST_STOP = 200;

		private float recheck_fall_start;

		private const float absorb_dmg_intv = 18f;

		private Func<AbsorbManager, bool> FD_isUniPublish;

		private const int ABSORB_PRI = 20;

		private EnemyAnimator.FnFineFrame FD_fnFineUniFrame;

		private M2DrawBinder.FnEffectBind FD_fnDrawSplashNeedle;
	}
}
