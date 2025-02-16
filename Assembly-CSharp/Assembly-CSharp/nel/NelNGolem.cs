using System;
using System.Collections.Generic;
using m2d;
using nel.smnp;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNGolem : NelEnemy
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			base.weight = (this.weight0 = 2f);
			NOD.BasicData basicData;
			if (this.id == ENEMYID.GOLEM_0_NM)
			{
				basicData = NOD.getBasicData("GOLEM_0_NM");
			}
			else
			{
				this.id = ENEMYID.GOLEM_0;
				basicData = NOD.getBasicData("GOLEM_0");
			}
			if (this.id >= ENEMYID.GOLEM_0_NM)
			{
				base.weight = (this.weight0 = 9f);
				this.nomove_golem = true;
			}
			base.appear(_Mp, basicData);
			if (NelNGolem.PFStone == null)
			{
				NelNGolem.PFStone = MTRX.getPF("stone_shot");
			}
			PxlSequence sequence = this.Anm.getPoseByName("_parts").getSequence(0);
			this.FD_checkNearToy = new NelEnemy.FnCheckEnemy(this.checkNearToy);
			this.FD_getBossLen = new Func<M2Mover, float>(this.getBossLen);
			this.Anm.addAdditionalDrawer(this.EadShotHand = new EnemyAnimator.EnemyAdditionalDrawFrame(sequence.getFrame(1), new EnemyAnimator.EnemyAdditionalDrawFrame.FnDrawEAD(this.fnDrawShotHand), false));
			this.Nai.awake_length = num;
			this.Nai.pl_magic_awake_time_min = 14f;
			this.Nai.pl_magic_awake_time_max = 19f;
			this.Nai.attackable_length_x = 13f;
			this.Nai.attackable_length_top = -6f;
			this.Nai.attackable_length_bottom = 1f;
			this.Nai.fnSleepLogic = new NAI.FnNaiLogic(this.Nai.fnAwakeDoNothing);
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.Nai.suit_distance_garaaki_ratio = 1.5f;
			this.Phy.fric_reduce_x = 0.012f;
			this.absorb_weight = 1;
			this.FD_isTortureInjection = new Func<AbsorbManager, bool>(this.isTortureInjection);
			this.FD_isTortureGolem = new Func<AbsorbManager, bool>(this.isTortureGolem);
			this.FD_MgRunNormalShot = new MagicItem.FnMagicRun(this.MgRunNormalShot);
			this.FD_MgDrawNormalShot = new MagicItem.FnMagicRun(this.MgDrawNormalShot);
			this.FD_fnFineAbsorbAttack = new EnemyAnimator.FnFineFrame(this.fnFineAbsorbAttack);
			this.AtkShotHit.Prepare(this, true);
			this.AtkGroundWave.Prepare(this, true);
			this.AtkAbsorbFatal.Prepare(this, true);
			this.AtkOdAttack.Prepare(this, true);
			this.AtkAbsorbFatal2.Prepare(this, true);
		}

		public static bool countGolem(NelEnemy N)
		{
			return N is NelNGolem;
		}

		public static bool countGolemToy(NelEnemy N)
		{
			return N is NelNGolemToy;
		}

		public bool checkNearToy(NelEnemy N)
		{
			if (!(N is NelNGolemToy))
			{
				return false;
			}
			NelNGolemToy nelNGolemToy = N as NelNGolemToy;
			if (nelNGolemToy.create_finished || !nelNGolemToy.creator_addable || !nelNGolemToy.is_alive)
			{
				return false;
			}
			M2BlockColliderContainer.BCCLine lastBCC = this.FootD.get_LastBCC();
			M2BlockColliderContainer.BCCLine lastBCC2 = N.getFootManager().get_LastBCC();
			return lastBCC != null && lastBCC2 != null && X.LENGTHXYS(base.x, base.footbottom, N.x, N.footbottom) < 6f && lastBCC.isLinearWalkableTo(lastBCC2, true) != 0;
		}

		public void recheckBossEnemy()
		{
			for (int i = this.Mp.count_movers - 1; i >= 0; i--)
			{
				NelNGolem nelNGolem = this.Mp.getMv(i) as NelNGolem;
				if (!(nelNGolem == null) && !(nelNGolem == this) && !nelNGolem.isOverDrive() && nelNGolem.need_check_boss == 0)
				{
					nelNGolem.need_check_boss = 1;
				}
			}
		}

		public override void initOverDriveAppear()
		{
			base.initOverDriveAppear();
			if (this.FollowBoss != null)
			{
				this.FollowBoss.removePawn(this);
			}
			this.FollowBoss = null;
			this.absorb_weight = 5;
			if (this.APawn == null)
			{
				this.APawn = new List<NelNGolem>(4);
			}
			this.APawn.Clear();
			this.recheckBossEnemy();
		}

		public override void quitOverDrive()
		{
			base.quitOverDrive();
			this.absorb_weight = 1;
			this.need_check_boss = 1;
			this.APawn = null;
			this.recheckBossEnemy();
		}

		protected override bool initDeathEffect()
		{
			if (base.isOverDrive())
			{
				this.recheckBossEnemy();
			}
			if (this.FollowBoss != null)
			{
				this.FollowBoss.removePawn(this);
			}
			this.APawn = null;
			if (this.TargetToy != null)
			{
				this.TargetToy.remCreator(this);
			}
			this.TargetToy = null;
			return base.initDeathEffect();
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			this.MgShot = null;
			NelEnemy.STATE state = this.state;
			base.changeState(st);
			if (st == NelEnemy.STATE.DAMAGE && !base.isOverDrive())
			{
				this.setAim((base.mpf_is_right > 0f) ? AIM.L : AIM.R, false);
				this.setLandPose();
			}
			if (base.isAbsorbState(state) && !base.isAbsorbState(st))
			{
				this.Anm.fnFineFrame = null;
				if (this.Nai.isPrAlive())
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_1, 200f);
				}
			}
			return this;
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai, NAI.TYPE.GAZE))
			{
				return true;
			}
			if (base.hasFoot() && !base.hasPT(1, false, false))
			{
				if (base.AimPr is PR)
				{
					AbsorbManagerContainer absorbContainer = (base.AimPr as PR).getAbsorbContainer();
					if (this.addTicketToyCreate(0.7f))
					{
						return true;
					}
					if (absorbContainer.isActive() && absorbContainer.countTortureItem(this.FD_isTortureInjection, false) > 0)
					{
						if (this.nomove_golem)
						{
							return Nai.AddTicketB(NAI.TYPE.GAZE, 1, true);
						}
						if (Nai.RANtk(7713) < 0.2f && Nai.AddMoveTicketToTarget(4f, 0f, 1, false, NAI.TYPE.WALK) != null)
						{
							return true;
						}
						Nai.AddTicket(NAI.TYPE.WAIT, 1, true);
						return true;
					}
				}
				if (this.need_check_boss > 1)
				{
					if (this.Useable(this.McsShot, 1.1f, 0f))
					{
						if (Nai.RANtk(7713) < 0.8f)
						{
							this.need_check_boss -= 1;
							return Nai.AddTicketB(NAI.TYPE.PUNCH, 60, true);
						}
						if (Nai.target_xdif > 6f && !this.nomove_golem && Nai.AddMoveTicketToTarget(0f, 0f, 1, false, NAI.TYPE.WALK) != null)
						{
							return true;
						}
						Nai.AddTicket(NAI.TYPE.WAIT, 1, true);
						return true;
					}
					else
					{
						this.need_check_boss = 1;
					}
				}
				if (this.need_check_boss == 1)
				{
					NelNGolem nelNGolem = X.getLowest<M2Mover>(this.Mp.getVectorMover(), this.FD_getBossLen, this.Mp.count_movers, -1000f) as NelNGolem;
					if (nelNGolem != this.FollowBoss)
					{
						if (this.FollowBoss != null)
						{
							this.FollowBoss.removePawn(this);
						}
						this.FollowBoss = nelNGolem;
						if (this.FollowBoss != null)
						{
							this.FollowBoss.addPawn(this);
						}
					}
					this.need_check_boss = 0;
				}
				if (this.need_check_boss == 0)
				{
					if (this.Useable(this.McsShot, 1.1f, 0f))
					{
						if (this.addTicketToyCreate(0.3f))
						{
							return true;
						}
						if (Nai.RANtk(7713) < ((Nai.isPrMagicChanting(1f) || Nai.isPrMagicExploded(1f)) ? 0.7f : 0.21f) && (this.FollowBoss != null || X.BTW(2.4f * (Nai.isPrGaraakiState() ? 0.2f : 1f), Nai.target_lastfoot_len, 8f)) && !Nai.hasTypeLock(NAI.TYPE.PUNCH))
						{
							return Nai.AddTicketB(NAI.TYPE.PUNCH, 60, true);
						}
					}
					else if (this.addTicketToyCreate(0.5f))
					{
						return true;
					}
					if (!this.nomove_golem)
					{
						if (this.FollowBoss == null || this.FollowBoss.FootD == null)
						{
							Nai.suit_distance = Nai.NIRANtk(2.8f, 4.4f, 498);
							if (Nai.AddMoveTicketToTarget(0f, 0f, 1, true, NAI.TYPE.WALK) != null)
							{
								return true;
							}
						}
						else
						{
							Nai.suit_distance = 2.2f;
							float num = (float)X.MPF(this.FollowBoss.x > Nai.target_x);
							if (Nai.AddMoveTicketFor(this.FollowBoss.x + Nai.NIRANtk(3f, 6f, 1844) * num, base.mbottom, this.FollowBoss.FootD.get_LastBCC(), 1, true, NAI.TYPE.WALK) != null)
							{
								return true;
							}
						}
					}
					if (this.Useable(this.McsShot, 1f, 0f) && Nai.RANtk(4944) < 0.67f)
					{
						return Nai.AddTicketB(NAI.TYPE.PUNCH, 60, true);
					}
				}
				Nai.AddTicket(NAI.TYPE.WAIT, 1, true);
				return true;
			}
			return false;
		}

		private float getBossLen(M2Mover B)
		{
			if (!(B is NelNGolem) || B == this)
			{
				return -1000f;
			}
			NelNGolem nelNGolem = B as NelNGolem;
			if (nelNGolem.destructed || !nelNGolem.isOverDrive() || !nelNGolem.is_alive)
			{
				return -1000f;
			}
			return X.LENGTHXYS(B.x, B.y, base.x, base.y);
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (this.state == NelEnemy.STATE.SPECIAL_0 && this.t >= 25f)
			{
				this.changeState(NelEnemy.STATE.STAND);
			}
		}

		public override bool readTicket(NaTicket Tk)
		{
			if (base.isOverDrive())
			{
				return this.readTicketOd(Tk);
			}
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.PUNCH)
			{
				if (type != NAI.TYPE.WALK)
				{
					if (type == NAI.TYPE.PUNCH)
					{
						return this.runShotNormal(Tk.initProgress(this), Tk);
					}
				}
				else
				{
					bool flag = Tk.initProgress(this);
					int num = base.walkThroughLift(flag, Tk, 20);
					if (num >= 0)
					{
						return num == 0;
					}
					return this.runWalkNormal(flag, Tk);
				}
			}
			else
			{
				if (type == NAI.TYPE.MAG)
				{
					return this.runToyCreating(Tk.initProgress(this), Tk);
				}
				if (type - NAI.TYPE.GAZE <= 1)
				{
					base.AimToLr((X.xors(2) == 0) ? 0 : 2);
					Tk.after_delay = 20f + this.Nai.RANtk(840) * 30f;
					return false;
				}
			}
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.PUNCH)
				{
					this.MgShot = null;
				}
				if (Tk.type == NAI.TYPE.WALK && base.SpPoseIs("walk", "od_walk"))
				{
					this.SpSetPose("stand", -1, null, false);
				}
				this.Anm.fnFineFrame = null;
			}
			this.checknear = false;
			if (this.TargetToy != null)
			{
				this.TargetToy.remCreator(this);
			}
			this.TargetToy = null;
			base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			this.ThrowPawn = null;
			base.quitTicket(Tk);
		}

		protected override bool setLandPose()
		{
			bool flag = base.setLandPose();
			this.Nai.delay = (float)((!base.isOverDrive()) ? 78 : 55);
			if (this.need_check_boss > 1)
			{
				this.Anm.rotationR_speed = 0f;
				this.Anm.rotationR = 0f;
			}
			return flag;
		}

		public bool runWalkNormal(bool init_flag, NaTicket Tk)
		{
			float num = (float)((Tk.priority == 128) ? 340 : 160);
			if (init_flag)
			{
				if (base.isOverDrive() && X.LENGTHXYS(Tk.depx, Tk.depy, base.x, base.mbottom) < 0.02f)
				{
					this.Nai.addDeclineArea(base.x - 0.5f, base.mbottom - 0.5f, 1f, 1f, 200f);
					return false;
				}
				float num2 = (base.isOverDrive() ? 0.09f : ((Tk.priority == 128) ? 0.08f : this.Nai.NIRANtk(0.06f, 0.08f, 4811)));
				this.walk_st = X.IntC(X.Abs(Tk.depx - base.x) / num2);
				if (Tk.priority != 128)
				{
					this.walk_st = X.Mx(base.isOverDrive() ? 15 : 40, this.walk_st);
				}
				this.t = X.Mx(0f, num - (float)this.walk_st);
				this.walk_time = 0f;
				this.SpSetPose("walk", -1, null, false);
			}
			float num3 = X.Abs(this.Nai.ticket_start_x - Tk.depx) / (float)this.walk_st;
			if (!base.hasFoot() || this.walk_time > 0f)
			{
				this.walk_time += this.TS;
				this.Phy.addFoc(FOCTYPE.WALK | (base.hasFoot() ? FOCTYPE._RELEASE : ((FOCTYPE)0U)), num3 * base.mpf_is_right * X.NI(1f, 0.5f, X.ZLINE(this.walk_time, 30f)), 0f, -1f, -1, 1, 0, -1, 0);
				if (base.hasFoot())
				{
					Tk.check_nearplace_error = 0;
					return false;
				}
			}
			else
			{
				bool flag = base.wallHitted(this.aim);
				if (this.t > num || flag)
				{
					if (!base.isOverDrive())
					{
						this.SpSetPose("walk2stand", -1, null, false);
					}
					if (base.isOverDrive() ? (this.Nai.target_foot_slen > 3f) : (this.Nai.target_len > 5f))
					{
						this.Nai.delay = this.Nai.NIRANtk(20f, 30f, 3189);
					}
					Tk.check_nearplace_error = (flag ? 2 : 0);
					if (flag && this.t < 5f)
					{
						this.Nai.addDeclineArea(Tk.depx - 1f, Tk.depy - 5f, 2f, 10f, 200f);
					}
					if (this.TargetToy != null && this.TargetToy.isCoveringMv(this, 0f, 0f))
					{
						Tk.Recreate(NAI.TYPE.MAG, -1, false, null);
						return true;
					}
					return false;
				}
				else
				{
					if (!base.isOverDrive() && this.t >= 30f && this.Nai.isDangerousWalk((num3 > 0f) ? AIM.R : AIM.L, true))
					{
						if ((num3 < 0f) ? (this.Nai.target_x < base.mleft - 0.5f) : (this.Nai.target_x > base.mright + 0.5f))
						{
							this.Nai.AddF(NAI.FLAG.BOTHERED, 160f);
						}
						this.Nai.delay = this.Nai.NIRANtk(40f, 60f, 3189);
						return false;
					}
					this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, num3 * base.mpf_is_right, 0f, -1f, -1, 1, 0, -1, 0);
				}
			}
			return true;
		}

		public bool runShotNormal(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_time = -this.Nai.NIRANtk(70f, 90f, 4921);
				this.SpSetPose("atk_0", -1, null, false);
				this.MgShot = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunNormalShot, this.FD_MgDrawNormalShot);
				this.walk_st = this.MgShot.id;
			}
			if (!base.hasFoot())
			{
				return false;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.MgShot != null && (this.MgShot.phase != 0 || this.MgShot.killed || this.MgShot.id != this.walk_st))
				{
					this.MgShot = null;
				}
				this.walk_time += this.TS;
				if (this.walk_time >= 0f)
				{
					this.MpConsume(this.McsShot, this.MgShot, 1f, 1f);
					if (this.MgShot == null)
					{
						return false;
					}
					this.MgShot.phase = 1;
					this.MgShot.Ray.check_hit_wall = true;
					this.MgShot = null;
					this.t = 0f;
					Tk.prog = PROG.PROG0;
					this.SpSetPose("atk_1", -1, null, false);
					Tk.after_delay += 68f;
					return false;
				}
			}
			return true;
		}

		private bool fnDrawShotHand(MeshDrawer Md, EnemyAnimator.EnemyAdditionalDrawFrame Ead)
		{
			float num;
			if (this.MgShot != null)
			{
				num = this.MgShot.da;
			}
			else
			{
				num = CAim.get_agR(this.aim, 0f);
			}
			PxlLayer layer = Ead.F.getLayer(1);
			PxlLayer layer2 = Ead.F.getLayer(0);
			PxlImage img = layer2.Img;
			float num2 = (layer.x - layer2.x) / (float)img.width + 0.5f;
			float num3 = -(layer.y - layer2.y) / (float)img.height + 0.5f;
			Md.initForImg(img, 0);
			Md.RotaGraph3(layer.x, -layer.y, num2, num3, 1f, 1f, num, null, false);
			return true;
		}

		public override Vector2 getAimPos(MagicItem Mg)
		{
			Vector2 aimPos = base.getAimPos(Mg);
			if (!base.isOverDrive() && Mg == this.MgShot)
			{
				aimPos.y -= X.Mn(X.MMX(0f, this.Nai.target_len * this.Nai.NIRANtk(0.3f, 1.6f, 815), 6f), X.Abs(aimPos.x - base.x) * 1.5f);
			}
			return aimPos;
		}

		private bool MgRunNormalShot(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (Mg.t <= 0f)
			{
				Mg.Ray.RadiusM(0.12f).HitLock(40f, null);
				Mg.Ray.projectile_power = 100;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.calcAimPos(false);
				Mg.da = -1.5707964f;
				Mg.sa = X.XORSPS() * 3.1415927f;
				Mg.efpos_s = (Mg.raypos_s = true);
				Mg.wind_apply_s_level = 1f;
				Mg.Atk0 = this.AtkShotHit;
				Mg.playSndPos("gorem_shot_0", true);
			}
			else if (!Mg.Caster.canHoldMagic(Mg))
			{
				return false;
			}
			if (Mg.phase == 0)
			{
				Mg.calcAimPos(false);
				if (Mg.t > 200f)
				{
					return false;
				}
				Mg.da = X.VALWALKANGLER(Mg.da, Mg.aim_agR, 0.025132744f * X.NI(2.2f, 1f, 1f - X.ZLINE(Mg.t, 40f)));
				if (base.mpf_is_right > 0f)
				{
					Mg.da = X.MMX(-1.0115929f, Mg.da, 1.0115929f);
				}
				else if (Mg.da > 0f)
				{
					Mg.da = X.Mx(Mg.da, 2.1299999f);
				}
				else
				{
					Mg.da = X.Mn(Mg.da, -2.1299999f);
				}
				float num = 26f * this.Mp.rCLENB * base.scaleX;
				Mg.sx = Mg.Cen.x + num * X.Cos(Mg.da);
				Mg.sy = Mg.Cen.y - num * X.Sin(Mg.da);
			}
			if (Mg.phase == 1)
			{
				Mg.phase = 2;
				float num2 = X.NIXP(0.11f, 0.23f);
				Mg.dx = num2 * X.Cos(Mg.da);
				Mg.dy = -num2 * X.Sin(Mg.da);
				Mg.t = 1f;
				Mg.PtcVar("agR", (double)Mg.da).PtcST("gorem_shot_start", PTCThread.StFollow.NO_FOLLOW, false);
				Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true);
			}
			if (Mg.phase >= 2)
			{
				Mg.dy = X.Mn(Mg.dy, 0.21f);
				bool flag = Mg.Mp.simulateDropItem(ref Mg.sx, ref Mg.sy, ref Mg.dx, ref Mg.dy, 0.12f, 0.8f, 0.4f, fcnt, 0.12f, false, true) != 0;
				float num3 = X.NI(0f, 0.014f, X.ZLINE(X.LENGTHXYS(0f, 0f, Mg.dx, Mg.dy), 0.1f)) * 3.1415927f * (float)X.MPF(Mg.dx > 0f);
				Mg.sa += num3;
				Mg.calcAimPos(false);
				Mg.MnSetRay(Mg.Ray, 0, Mg.aim_agR, Mg.t);
				HITTYPE hittype = HITTYPE.NONE;
				if (flag)
				{
					Mg.dx = X.VALWALK(Mg.dx, 0f, 0.007f);
					if (Mg.phase == 2)
					{
						Mg.Ray.hittype |= HITTYPE.TEMP_OFFLINE;
						Mg.phase = 3;
						Mg.t = 1f;
					}
					hittype |= HITTYPE.WALL;
				}
				float num4 = (((Mg.Ray.hittype & HITTYPE.TEMP_OFFLINE) == HITTYPE.NONE) ? 0.015f : 0f);
				Mg.Ray.LenM(num4);
				hittype |= Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(X.absmin(Mg.dx, 0.25f), 0f), HITTYPE.NONE);
				if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
				{
					Mg.kill(0.125f);
					return false;
				}
				if (Mg.t >= (float)((Mg.phase >= 3) ? 90 : 280))
				{
					return false;
				}
				if ((hittype & HITTYPE.WALL_AND_BREAK) != HITTYPE.NONE)
				{
					Mg.PtcVar("sx", (double)Mg.sx).PtcVar("sy", (double)Mg.sy).PtcST("golem_shot_broken", PTCThread.StFollow.NO_FOLLOW, false);
					return false;
				}
				if ((hittype & HITTYPE.REFLECTED) != HITTYPE.NONE)
				{
					Mg.reflectV(Mg.Ray, ref Mg.dx, ref Mg.dy, 0f, 0.25f, true);
				}
			}
			return true;
		}

		private bool MgDrawNormalShot(MagicItem Mg, float fcnt)
		{
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			if (Mg.phase == 3)
			{
				meshImg.Col = meshImg.ColGrd.White().mulA(X.ZLINE(Mg.t - 30f, 55f)).C;
			}
			float num = 2f;
			if (Mg.phase == 0)
			{
				num *= X.ZSIN(this.t, 40f);
			}
			meshImg.RotaPF(0f, 0f, num, num, Mg.sa + Mg.da, NelNGolem.PFStone, this.index % 2 == 0, false, false, uint.MaxValue, false, 0);
			return true;
		}

		private bool runToyCreating(bool init_flag, NaTicket Tk)
		{
			if (this.TargetToy == null)
			{
				return false;
			}
			if (this.TargetToy.create_finished && Tk.prog < PROG.PROG4)
			{
				Tk.prog = PROG.PROG4;
				this.t = 0f;
			}
			if (!base.hasFoot())
			{
				return false;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				M2BlockColliderContainer.BCCLine footBCC = this.TargetToy.getFootManager().get_FootBCC();
				if (footBCC == null)
				{
					return this.t < 60f;
				}
				this.walk_st = 0;
				this.walk_time = 0f;
				Tk.Progress(ref this.t, 0, true);
				float num = X.Mx(footBCC.shifted_x, this.TargetToy.mleft - 0.8f);
				float num2 = X.Mn(footBCC.shifted_right, this.TargetToy.mright + 0.8f);
				if (X.BTW(num, base.x, num2))
				{
					Tk.prog = PROG.PROG2;
					if (this.TargetToy.disappearing)
					{
						this.TargetToy.progressCreate(this);
					}
					this.setAim((X.xors(2) == 0) ? AIM.R : AIM.L, false);
				}
				else
				{
					Tk.prog = PROG.PROG0;
					Tk.Dep(X.NI(num, num2, X.XORSP()), Tk.depy, footBCC);
					this.setAim((base.x < Tk.depx) ? AIM.R : AIM.L, false);
					this.Anm.setPose("walk", -1);
				}
			}
			if (Tk.prog >= PROG.ACTIVE && Tk.prog < PROG.PROG5 && !base.hasFoot())
			{
				Tk.prog = PROG.PROG5;
				this.Anm.setPose("fall", -1);
				this.t = 0f;
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.setWalkXSpeed(base.mpf_is_right * X.NIL(0.01f, 0.04f, this.t, 25f), true, false);
				if (!base.hasFoot())
				{
					Tk.prog = PROG.PROG5;
					this.Anm.setPose("fall", -1);
					this.t = 0f;
				}
				else if (base.wallHittedA() || this.t >= 150f)
				{
					this.walk_time += this.TS;
					if (this.walk_time >= 40f)
					{
						this.Nai.addTypeLock(NAI.TYPE.MAG, 150f);
						Tk.AfterDelay(40f);
						return false;
					}
				}
				else
				{
					this.walk_time = X.Mx(0f, this.walk_time - this.TS * 0.25f);
					if (X.Abs(base.x - Tk.depx) <= this.sizex)
					{
						this.t = 0f;
						Tk.prog = PROG.ACTIVE;
					}
				}
			}
			if (Tk.prog == PROG.PROG2)
			{
				this.setWalkXSpeed(X.VALWALK(this.Phy.walk_xspeed, 0f, this.TS * ((this.walk_st > 0) ? 0.0013f : 0.0043f)), true, false);
				if (this.Phy.walk_xspeed == 0f)
				{
					this.Anm.setPose((X.xors(2) == 0) ? "society_0" : "society_1", -1);
					this.t = 0f;
					Tk.prog = PROG.PROG3;
					this.walk_st = 0;
				}
			}
			if (Tk.prog == PROG.PROG3 && this.t >= 24f)
			{
				this.TargetToy.progressCreate(this);
				int num3 = this.walk_st + 1;
				this.walk_st = num3;
				if (num3 >= 3)
				{
					this.setAim((base.x < this.TargetToy.x) ? AIM.R : AIM.L, false);
					this.Anm.setPose("walk", -1);
					this.setWalkXSpeed(base.mpf_is_right * 0.041f, true, false);
					Tk.prog = PROG.PROG2;
					this.t = 0f;
				}
				else
				{
					this.t -= (float)this.Anm.getCurrentSequence().getDuration();
				}
			}
			if (Tk.prog == PROG.PROG4)
			{
				if (this.t >= 20f)
				{
					this.Anm.setPose("stand", -1);
				}
				if (this.t >= 30f)
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG, 500f);
					return false;
				}
			}
			if (Tk.prog == PROG.PROG5 && base.hasFoot())
			{
				Tk.AfterDelay(70f);
				return false;
			}
			return true;
		}

		public void addPawn(NelNGolem G)
		{
			if (this.APawn != null)
			{
				this.APawn.Add(G);
			}
		}

		public void removePawn(NelNGolem G)
		{
			if (this.APawn != null)
			{
				this.APawn.Remove(G);
			}
		}

		private bool considerOverDrive(NAI Nai)
		{
			if (Nai.HasF(NAI.FLAG.OVERDRIVED, true))
			{
				Nai.AddTicket(NAI.TYPE.APPEAL_0, 128, true).AfterDelay(20f);
				return true;
			}
			if (this.APawn == null)
			{
				this.initOverDriveAppear();
			}
			if (!Nai.HasF(NAI.FLAG.ABSORB_FINISHED, true))
			{
				if (!base.hasPT(60, false, false))
				{
					if (base.AimPr is PR)
					{
						AbsorbManagerContainer absorbContainer = (base.AimPr as PR).getAbsorbContainer();
						if (absorbContainer.isActive())
						{
							AbsorbManager specificPublisher = absorbContainer.getSpecificPublisher(this.FD_isTortureGolem);
							float num = ((specificPublisher != null) ? 0.12f : (absorbContainer.isFilled() ? 0.22f : 0f));
							if (specificPublisher != null && !Nai.HasF(NAI.FLAG.INJECTED, false) && (specificPublisher.getPublishMover() as NelNGolem).Nai.HasF(NAI.FLAG.INJECTED, false))
							{
								num += 0.7f;
							}
							if (num > 0f)
							{
								if (!Nai.HasF(NAI.FLAG.POWERED, false) && !X.BTW(1.8f, Nai.target_xdif, 5f))
								{
									Nai.suit_distance_overdrive = 2.6f + Nai.RANtk(4113) * 1.5f;
									if (Nai.AddMoveTicketToTarget(0f, 0f, 60, true, NAI.TYPE.WALK) != null)
									{
										return true;
									}
								}
								Nai.suit_distance_overdrive = 0f;
								if (Nai.RANtk(1049) < (Nai.HasF(NAI.FLAG.INJECTED, false) ? 0.2f : 1f) * num && Nai.target_ydif < 2.8f)
								{
									Nai.AddF(NAI.FLAG.POWERED, 160f);
								}
								if (!Nai.HasF(NAI.FLAG.POWERED, false))
								{
									return Nai.AddTicketB(NAI.TYPE.GAZE, 60, true);
								}
								this.Phy.addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, 120f);
								if (Nai.target_foot_slen < 1.6f)
								{
									Nai.AddTicket(NAI.TYPE.PUNCH_1, 60, true);
									return true;
								}
								Nai.suit_distance_overdrive = 1f;
								return Nai.AddMoveTicketToTarget(0f, 0f, 60, true, NAI.TYPE.WALK) != null || Nai.AddTicketB(NAI.TYPE.GAZE, 60, true);
							}
						}
					}
					bool flag = true;
					bool flag2 = Nai.HasF(NAI.FLAG.BOTHERED, false);
					if (Nai.target_lastfoot_bottom_mx < base.mbottom - 1.88f)
					{
						Nai.suit_distance_overdrive = 1.1f;
						flag = false;
						if (Nai.target_xdif < 2.7f)
						{
							if (Nai.target_foot_len < 3.5f)
							{
								if (Nai.fnBasicPunch(Nai, 60, 20f, 4f, 70f, 0f, 2877, false))
								{
									return true;
								}
							}
							else
							{
								if (Nai.fnBasicPunch(Nai, 60, 0f, (float)(this.Useable(this.SwiOd.Mcs, 1.5f, 0f) ? (60 - (flag2 ? 40 : 0)) : 0), 0f, (float)(20 + (flag2 ? 40 : 0)), 3194, false))
								{
									return true;
								}
								if (Nai.fnBasicPunch(Nai, 60, 5f, 0f, 0f, 0f, 5158, false))
								{
									return true;
								}
							}
						}
					}
					else if (!Nai.isPrAlive() && !Nai.HasF(NAI.FLAG.ATTACKED, false))
					{
						Nai.suit_distance_overdrive = 1.3f;
						if (Nai.target_foot_slen > 1.1f)
						{
							if (Nai.AddMoveTicketToTarget(0f, 0f, 1, true, NAI.TYPE.WALK) != null)
							{
								return true;
							}
						}
						else
						{
							if (Nai.fnBasicPunch(Nai, 60, 0f, 0f, 90f, 0f, 2315, false))
							{
								Nai.AddF(NAI.FLAG.ATTACKED, 900f);
								return true;
							}
							flag = false;
						}
					}
					if (flag)
					{
						Nai.RemF(NAI.FLAG.ATTACKED);
					}
					float target_foot_slen = Nai.target_foot_slen;
					if (Nai.isPrGaraakiState())
					{
						Nai.suit_distance_overdrive = 1.3f;
						if (Nai.fnBasicPunch(Nai, 60, (float)((target_foot_slen > 2.8f) ? 0 : 23), 10f, (float)(((target_foot_slen > 1f) ? 0 : 50) + (flag2 ? 40 : 0)), 0f, 1944, false))
						{
							return true;
						}
					}
					else
					{
						Nai.suit_distance_overdrive = 6.8f;
						if (Nai.isPrMagicExploded(1f) && Nai.fnBasicPunch(Nai, 60, 43f, (float)(this.Useable(this.SwiOd.Mcs, 1.5f, 0f) ? 50 : 0), 0f, 0f, 1944, true))
						{
							return true;
						}
						Nai.suit_distance_overdrive = 2.8f;
						if (target_foot_slen > Nai.NIRANtk(1f, 2.8f, 4215))
						{
							if (this.Useable(this.SwiOd.Mcs, 1.5f, 0f) && Nai.fnBasicPunch(Nai, 60, 0f, 20f, 0f, (float)((this.APawn.Count > 0) ? 15 : 0), 5581, true))
							{
								return true;
							}
							if (Nai.fnBasicPunch(Nai, 60, 5f, 0f, 0f, 0f, 3162, true))
							{
								return true;
							}
						}
						else if (flag2)
						{
							Nai.suit_distance_overdrive = 1.1f;
							if (Nai.fnBasicPunch(Nai, 60, 15f, 0f, 80f, 0f, 8841, true))
							{
								return true;
							}
						}
						else if (Nai.fnBasicPunch(Nai, 60, 35f, 10f, 50f, 0f, 8841, true))
						{
							return true;
						}
					}
				}
				if (!base.hasPT(1, false, false))
				{
					if (Nai.target_lastfoot_len > 1.5f)
					{
						Nai.suit_distance_overdrive = 1.4f;
						if (Nai.AddMoveTicketToTarget(0f, 0f, 1, true, NAI.TYPE.WALK) != null)
						{
							return true;
						}
					}
					Nai.AddTicket(NAI.TYPE.WAIT, 1, true);
				}
				return true;
			}
			if (Nai.HasF(NAI.FLAG.BOTHERED, false))
			{
				Nai.delay = 60f;
				this.SpSetPose("od_throw", -1, null, false);
				return false;
			}
			this.SpSetPose("od_stand", -1, null, false);
			Nai.delay = 50f;
			return false;
		}

		private bool addTicketToyCreate(float ratio = 1f)
		{
			if (base.destructed)
			{
				return false;
			}
			try
			{
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (this.Nai.hasPT(60, false) || footBCC == null || this.Nai.hasTypeLock(NAI.TYPE.MAG))
				{
					return false;
				}
				if (this.Summoner == null || (ratio < 1f && this.Nai.RANtk(8853) >= ratio))
				{
					return false;
				}
				if (SummonerPlayer.golemtoy_lock_delay > 0f)
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG, 300f);
					return false;
				}
				if (!this.checknear)
				{
					this.checknear = true;
					NelNGolemToy nelNGolemToy = this.Summoner.searchActiveEnemy(this.FD_checkNearToy) as NelNGolemToy;
					if (nelNGolemToy != null && nelNGolemToy.getFootManager() != null)
					{
						this.TargetToy = nelNGolemToy;
						M2BlockColliderContainer.BCCLine footBCC2 = this.TargetToy.getFootManager().get_FootBCC();
						if (footBCC2 == footBCC)
						{
							this.TargetToy.addCreator(this);
							this.Nai.AddTicket(NAI.TYPE.MAG, 128, true);
						}
						else if (this.Nai.AddMoveTicketFor(this.TargetToy.x, this.TargetToy.mbottom, footBCC2, 128, false, NAI.TYPE.WALK) != null)
						{
							this.TargetToy.addCreator(this);
							return true;
						}
						return true;
					}
				}
				int num;
				int golemToyCreatable = this.Summoner.getGolemToyCreatable(out num);
				if (golemToyCreatable == 0 || num >= golemToyCreatable)
				{
					this.Nai.addTypeLock(NAI.TYPE.MAG, 300f);
					if (golemToyCreatable > 0)
					{
						this.Summoner.LockGolemToyMax();
					}
					return false;
				}
				float num2;
				if (base.x < this.Nai.target_x)
				{
					num2 = X.Mx(footBCC.shifted_x + 0.5f, base.x - 4f);
				}
				else
				{
					num2 = X.Mn(footBCC.shifted_right - 0.5f, base.x + 4f);
				}
				float num3 = footBCC.slopeBottomY(num2, footBCC.BCC.base_shift_x, footBCC.BCC.base_shift_y, true);
				SmnEnemyKind smnEnemyKind = new SmnEnemyKind(this.Summoner.RanSelect<string>(NelNGolemToy.Acreate_key, 1389, 51), 1, -1, this.toy_mp_min, this.toy_mp_max, "", 0f, 0f, ENATTR.NORMAL)
				{
					temporary_adding_count = true,
					nattr = this.nattr
				};
				smnEnemyKind.nattr &= ~smnEnemyKind.EnemyDesc.nattr_decline;
				SmnPoint smnPoint = new SmnPoint(this.Summoner, num2, num3, 1f, null)
				{
					sudden_appear = true
				};
				this.TargetToy = this.Summoner.summonEnemyFromOther(smnEnemyKind, smnPoint) as NelNGolemToy;
				if (this.TargetToy != null)
				{
					this.TargetToy.fixFirstPos(footBCC);
					this.TargetToy.addCreator(this);
					this.Nai.AddTicket(NAI.TYPE.MAG, 128, true);
					return true;
				}
			}
			catch
			{
				this.Nai.addTypeLock(NAI.TYPE.MAG, 900f);
			}
			return false;
		}

		public bool readTicketOd(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
			{
				bool flag = Tk.initProgress(this);
				int num = base.walkThroughLift(flag, Tk, 20);
				if (num >= 0)
				{
					return num == 0;
				}
				return this.runWalkNormal(flag, Tk);
			}
			case NAI.TYPE.WALK_TO_WEED:
				break;
			case NAI.TYPE.PUNCH:
				return this.runOdPunchNormal(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_0:
				return this.runOdGroundShockwave(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_1:
				return this.runOdGrab(Tk.initProgress(this), Tk);
			case NAI.TYPE.PUNCH_2:
				return this.runOdThrowPawn(Tk.initProgress(this), Tk);
			default:
				switch (type)
				{
				case NAI.TYPE.APPEAL_0:
					return base.runOverDriveAppealBasic(Tk.initProgress(this), Tk, "od_appeal", "od_appeal2stand", 100, 130);
				case NAI.TYPE.GAZE:
					base.AimToPlayer();
					if (Tk.initProgress(this))
					{
						this.SpSetPose("od_gaze", -1, null, false);
						this.t = 2000f - X.NIXP(240f, 1200f);
					}
					if (this.t >= 2000f || (base.AimPr is PR && !(base.AimPr as PR).isAbsorbState()))
					{
						this.SpSetPose("od_gaze2stand", -1, null, false);
						Tk.after_delay = 60f;
						return false;
					}
					return true;
				case NAI.TYPE.WAIT:
					base.AimToLr((X.xors(2) == 0) ? 0 : 2);
					Tk.after_delay = 12f + this.Nai.RANtk(840) * 14f;
					return false;
				}
				break;
			}
			return false;
		}

		private bool runOdPunchNormal(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = (float)(100 - X.xors(14));
				this.walk_st = 0;
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 100, true))
			{
				this.SpSetPose("od_attack_1", -1, null, false);
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, 48, true))
			{
				this.SpSetPose("od_attack_2", -1, null, false);
				base.PtcVar("scl", (double)base.scaleX).PtcST("golemL_swing_attack_2", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				base.tackleInit(this.AtkOdAttack, this.ATackleOdAttack);
				this.Phy.addFoc(FOCTYPE.WALK, base.mpf_is_right * 0.04f, 0f, -1f, 0, 2, 10, -1, 0);
				EnemyAttr.SplashSOnAir(this, base.x + (float)CAim._XD(this.aim, 1) * 1f, base.y, 1.5f, CAim.get_agR(this.aim, 0f), 0f, 0.4f, 1f, 1, 1f);
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.t >= 3f)
				{
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(ref this.t, 38, true))
				{
					this.SpSetPose("od_attack_3", -1, null, false);
					base.PtcVar("scl", (double)base.scaleX).PtcST("golemL_swing_attack_3", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
					this.walk_st = 0;
					this.Phy.addFoc(FOCTYPE.WALK, base.mpf_is_right * 0.08f, 0f, -1f, 0, 2, 18, -1, 0);
				}
			}
			if (Tk.prog == PROG.PROG2)
			{
				if (this.t >= 6f && this.walk_st == 0)
				{
					base.tackleInit(this.AtkOdAttack, this.ATackleOdAttack);
					this.walk_st++;
					EnemyAttr.SplashSOnAir(this, base.x + (float)CAim._XD(this.aim, 1) * 1f, base.y, 1.5f, CAim.get_agR(this.aim, 0f), 0f, 0.4f, 1f, 2, 1f);
				}
				if (this.t >= 9f)
				{
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(this.t >= 44f))
				{
					this.SpSetPose("od_attack_4", -1, null, false);
					this.playSndPos("golem_atk_close", 1);
					Tk.after_delay = this.Nai.NIRANtk(56f, 80f, 4102);
					return false;
				}
			}
			return true;
		}

		private bool runOdGroundShockwave(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				this.SpSetPose("od_gpond_1", -1, null, false);
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(this.t >= 46f))
			{
				this.t = 0f;
				this.SpSetPose("od_gpond_2", -1, null, false);
				base.PtcVar("scl", (double)base.scaleX).PtcST("golemL_shockwave_swing", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 5f && this.walk_st == 0)
				{
					base.tackleInit(this.AtkGroundWave, this.ATackleOdShockwavePrepare);
					this.walk_st = 1;
				}
				if (this.t >= 8f)
				{
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(this.t >= 13f))
				{
					EnemyAttr.Splash(this, base.x + (float)CAim._XD(this.aim, 1) * 1.4f, base.mbottom - 0.6f, 2f, 1f, 1.5f);
					this.t = 0f;
					this.can_hold_tackle = false;
					base.groundShockWaveInit(this.AtkGroundWave, this.SwiOd);
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(this.t >= 40f))
			{
				this.SpSetPose("od_gpond_3", -1, null, false);
				Tk.after_delay = this.Nai.NIRANtk(44f, 70f, 4102);
				this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 240f);
				return false;
			}
			return true;
		}

		private bool runOdGrab(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				this.SpSetPose("od_grab_1", -1, null, false);
			}
			else if (!base.hasFoot())
			{
				return false;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(this.t >= 24f))
			{
				this.t = 0f;
				this.playSndPos("golem_grab_attack", 1);
				this.SpSetPose("od_grab_2", -1, null, false);
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 6f && this.walk_st == 0)
				{
					base.tackleInit(this.AtkAbsorbGrab, this.ATackleOdGrab);
					if (!this.Nai.isPrAlive())
					{
						base.tackleInit(this.AtkAbsorbGrab, this.TackleOdGrab_Dead, MGHIT.AUTO);
					}
					this.walk_st = 1;
				}
				if (this.t >= 9f)
				{
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(this.t >= 15f))
				{
					this.can_hold_tackle = false;
					Tk.after_delay = this.Nai.NIRANtk(64f, 80f, 4102);
					return false;
				}
			}
			return true;
		}

		private bool runOdThrowPawn(bool init_flag, NaTicket Tk)
		{
			if (this.APawn == null || this.APawn.Count == 0)
			{
				this.Nai.addTypeLock(NAI.TYPE.PUNCH_2, 900f);
				return false;
			}
			if (init_flag)
			{
				this.t = 0f;
				this.ThrowPawn = null;
				NelNGolem nelNGolem = this.APawn.Find(new Predicate<NelNGolem>(this.getNearPawn));
				if (nelNGolem == null)
				{
					this.Nai.addTypeLock(NAI.TYPE.PUNCH_2, 400f);
					return false;
				}
				base.AimToLr((nelNGolem.x > base.x) ? 2 : 0);
				this.SpSetPose("od_grab_1", -1, null, false);
			}
			else if (!base.hasFoot())
			{
				return false;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 24, true))
			{
				this.SpSetPose("od_grab_2", -1, null, false);
			}
			if (Tk.prog >= PROG.PROG0)
			{
				if (this.t >= 3f && this.t <= 8f && Tk.prog == PROG.PROG0)
				{
					this.ThrowPawn = this.APawn.Find(new Predicate<NelNGolem>(this.getNearPawnThrowInitialize));
					if (this.ThrowPawn != null)
					{
						Tk.prog = PROG.PROG1;
						this.ThrowPawn.PtcVarS("attr", "NORMAL").PtcVar("hit_x", (double)X.NI(base.x, this.ThrowPawn.x, 0.7f)).PtcVar("hit_y", (double)X.NI(base.y, this.ThrowPawn.y, 0.7f))
							.PtcST("hitabsorb", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						base.AimToPlayer();
						this.SpSetPose("od_throw", -1, null, false);
						this.t = 0f;
					}
				}
				if (this.ThrowPawn != null)
				{
					if (this.ThrowPawn.isDestructed() || !this.ThrowPawn.throwInitialize(this, false))
					{
						this.ThrowPawn = null;
					}
					else if (this.t < 15f)
					{
						float num = X.ZPOW(this.t, 15f);
						float num2 = base.mpf_is_right * base.scaleX * X.NI(-1.2f, 0.7f, X.ZLINE(this.t, 15f));
						float num3 = base.scaleY * X.NI(1.7f, 1.3f, num);
						this.ThrowPawn.Phy.addFoc(FOCTYPE.WALK, X.absMn(num2 - this.ThrowPawn.x, 0.4f), X.absMn(num3 - this.ThrowPawn.y, 0.4f), -1f, -1, 1, 0, -1, 0);
					}
					else
					{
						Tk.prog = PROG.PROG2;
						float num4 = this.Nai.target_sight_x(1.2f, 19f);
						float num5 = this.Nai.target_y - this.ThrowPawn.y - X.NIXP(1f, 5f);
						this.ThrowPawn.changeState(NelEnemy.STATE.STAND);
						this.ThrowPawn.jumpInit(num4 - this.ThrowPawn.x, num5, X.Mx(3f, -num5 + X.NIXP(2.2f, 5.5f)), true);
						Vector2 vector = this.ThrowPawn.Phy.calcFocVelocity(FOCTYPE.WALK, true, false);
						this.ThrowPawn.PtcVar("agR", (double)this.Mp.GAR(0f, 0f, vector.x, vector.y)).PtcST("golem_throw_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.ThrowPawn.need_check_boss = (byte)X.IntR(X.NIXP(4f, 12f));
						this.ThrowPawn.Anm.rotationR_speed = base.mpf_is_right * X.NIXP(0.008f, 0.03f) * 3.1415927f;
						this.ThrowPawn = null;
					}
				}
				if (this.t >= 140f)
				{
					if (Tk.prog != PROG.PROG2)
					{
						this.Nai.addTypeLock(NAI.TYPE.PUNCH_2, 400f);
					}
					return false;
				}
			}
			return true;
		}

		private bool getNearPawn(NelNGolem _G)
		{
			return !_G.isOverDrive() && _G.isCoveringMv(this, 1.1f, 0.2f);
		}

		private bool getNearPawnThrowInitialize(NelNGolem _G)
		{
			return this.getNearPawn(_G) && _G.throwInitialize(this, true);
		}

		public bool throwInitialize(NelNGolem Thrower, bool execute = true)
		{
			if (!base.isOverDrive() && this.is_alive && (this.state == NelEnemy.STATE.STAND || this.state == NelEnemy.STATE.SPECIAL_0))
			{
				if (execute)
				{
					this.changeState(NelEnemy.STATE.SPECIAL_0);
					this.FootD.initJump(false, false, false);
					this.SpSetPose("fall", -1, null, false);
				}
				this.t = 0f;
				this.Phy.addLockGravityFrame(5);
				return true;
			}
			return false;
		}

		public override AttackInfo applyDamageFromMap(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo _Atk, float efx, float efy, bool apply_execute = true)
		{
			NelAttackInfo nelAttackInfo = base.applyDamageFromMap(MDI, _Atk, efx, efy, false) as NelAttackInfo;
			if (nelAttackInfo == null)
			{
				return null;
			}
			if (!apply_execute)
			{
				return null;
			}
			nelAttackInfo.shuffleHpMpDmg(this, 1f, 1f, -1000, -1000);
			this.Nai.AddF(NAI.FLAG.BOTHERED, 160f);
			if (this.applyDamage(nelAttackInfo, false) <= 0)
			{
				return null;
			}
			return nelAttackInfo;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			if (!X.SENSITIVE && base.isOverDrive())
			{
				A.Add("torture_golem_inject");
			}
			if (base.isOverDrive())
			{
				Aattr.Add(MGATTR.NORMAL);
				Aattr.Add(MGATTR.ABSORB_V);
				return;
			}
			Aattr.Add(MGATTR.MUD);
			Aattr.Add(MGATTR.NORMAL);
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (!base.isOverDrive())
			{
				return false;
			}
			if (!base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			Abm.kirimomi_release = true;
			Abm.release_from_publish_count = true;
			Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 10, 63U);
			if (this.Nai.HasF(NAI.FLAG.BOTHERED, false))
			{
				this.Nai.AddF(NAI.FLAG.BOTHERED, 90f);
			}
			return true;
		}

		public override bool runAbsorb()
		{
			if (!base.isOverDrive())
			{
				return false;
			}
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.t <= 0f)
			{
				this.Absorb.target_pose = "torture_leg_catch";
				this.walk_st = 0;
				this.walk_time = 0f;
				this.Absorb.setKirimomiReleaseDir((int)this.aim);
				if (this.Nai.HasF(NAI.FLAG.BOTHERED, false))
				{
					this.walk_st = -1;
				}
				else if (!X.SENSITIVE && !pr.is_alive && X.XORSP() < ((!this.Nai.HasF(NAI.FLAG.INJECTED, false) && (float)pr.EggCon.total_real < pr.get_maxmp()) ? 0.82f : 0.04f))
				{
					this.Absorb.target_pose = "torture_gorem_inject";
				}
				this.Anm.fnFineFrame = this.FD_fnFineAbsorbAttack;
			}
			if (this.Absorb.target_pose == "torture_leg_catch" || X.SENSITIVE)
			{
				if (!this.SpPoseIs("od_torture_leg_catch"))
				{
					this.Absorb.changeTorturePose(this.Absorb.target_pose, true, true, -1, -1);
					this.Absorb.uipicture_fade_key = "";
					this.Absorb.get_Gacha().SoloPositionPixel = new Vector3(-base.mpf_is_right * 95f, -80f, 0f);
					this.Absorb.Con.need_fine_gacha_effect = true;
				}
				if (this.walk_time < 0f)
				{
					this.walk_time += this.TS;
					if (this.walk_time >= 0f)
					{
						return false;
					}
				}
				else if (this.walk_st == 1 || this.walk_st == -2)
				{
					if (this.walk_st == 1)
					{
						this.walk_st = 2;
					}
					else
					{
						this.walk_st = -3;
					}
					this.walk_time += 1f;
					if ((this.nattr & ENATTR.ACME) != ENATTR.NORMAL)
					{
						this.walk_time += (float)X.xors(7);
					}
					base.applyAbsorbDamageTo(pr, this.AtkAbsorbMain, true, false, true, 0f, true, null, false, false);
					pr.PtcVar("cx", (double)(base.x + base.mpf_is_right * 1.9f)).PtcVar("cy", (double)(base.mbottom - 0.4f)).PtcST("ground_pound_attacked", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.Mp.DropCon.setBlood(pr, 15, MTR.col_blood, 0f, true);
					if (this.walk_time >= 15f)
					{
						bool flag = this.walk_st < 0 || (this.Nai.isPrAlive() && ((float)pr.ep < 500f || this.hp < this.maxhp));
						if ((this.nattr & ENATTR.ACME) != ENATTR.NORMAL)
						{
							flag = false;
						}
						if (flag || X.SENSITIVE)
						{
							if (this.walk_st >= 0 && X.XORSP() < 0.02f)
							{
								this.walk_time = -26f;
							}
						}
						else if (X.XORSP() < ((!this.Nai.HasF(NAI.FLAG.INJECTED, false) && (float)pr.EggCon.total < pr.get_maxmp() && !pr.EggCon.isLaying()) ? 0.25f : 0.01f))
						{
							this.Absorb.changeTorturePose("torture_gorem_inject", false, false, -1, -1);
							this.Absorb.uipicture_fade_key = "torture_golem_inject";
							this.walk_time = 0f;
							this.Absorb.get_Gacha().SoloPositionPixel = new Vector3(0f, 30f, 0f);
							this.Absorb.Con.need_fine_gacha_effect = true;
						}
					}
				}
				if (this.walk_st < 0 && this.t >= 76f)
				{
					this.setAim(this.Nai.getEnoughRoomAim(pr.x, pr.y, true), false);
					this.Absorb.kirimomi_release = true;
					this.Absorb.setKirimomiReleaseDir((int)this.aim);
					this.Nai.delay = 0f;
					this.Nai.AddF(NAI.FLAG.BOTHERED, 10f);
					return false;
				}
			}
			else
			{
				if (!base.SpPoseIs("od_torture_gorem_inject", "od_torture_gorem_inject_2", "od_torture_gorem_inject_3"))
				{
					this.Absorb.changeTorturePose(this.Absorb.target_pose, false, true, -1, -1);
					this.Absorb.uipicture_fade_key = "torture_golem_inject";
					this.Absorb.get_Gacha().SoloPositionPixel = new Vector3(0f, 30f, 0f);
					this.Absorb.Con.need_fine_gacha_effect = true;
				}
				if (this.walk_time < 0f)
				{
					this.walk_time += this.TS;
					if (this.walk_time >= 0f)
					{
						return false;
					}
				}
				else if (this.walk_st == 1)
				{
					this.walk_st = 2;
					pr.Ser.Add(SER.DO_NOT_LAY_EGG, 240, 99, false);
					this.walk_time += 1f;
					base.applyAbsorbDamageTo(pr, ((float)pr.ep >= 500f) ? this.AtkAbsorbFatal2 : this.AtkAbsorbFatal, true, false, false, 0f, false, null, false, true);
					Vector3 vector = pr.DMGE.publishVaginaSplashPiston(1.5707964f, false);
					base.PtcVar("x", (double)pr.x).PtcVar("y", (double)pr.y).PtcVar("hit_x", (double)vector.x)
						.PtcVar("hit_y", (double)vector.y)
						.PtcST("player_absorbed_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					if (this.walk_time >= 10f)
					{
						if (this.SpPoseIs("od_torture_gorem_inject_3") && X.XORSP() < (pr.EpCon.isOrgasmInitTime() ? 0.7f : 0.022f))
						{
							this.Absorb.changeTorturePose("torture_gorem_inject_2", false, false, -1, -1);
						}
						else if (X.XORSP() < 0.002f)
						{
							this.walk_time = -20f;
						}
						else if (X.XORSP() < (((float)pr.ep >= 900f && this.SpPoseIs("od_torture_gorem_inject_2")) ? 0.5f : 0.05f))
						{
							this.Absorb.changeTorturePose(this.SpPoseIs("torture_gorem_inject_2") ? "torture_gorem_inject_3" : "torture_gorem_inject_2", false, false, -1, -1);
						}
						if (this.walk_time >= 30f && this.SpPoseIs("od_torture_gorem_inject_3") && !this.Nai.HasF(NAI.FLAG.INJECTED, false) && X.XORSP() < 0.3f)
						{
							PrEggManager.CATEG categ = PrEggManager.CATEG.GOLEM_OD;
							if (pr.EggCon.applyEggPlantDamage(0.7f, categ, true, 1000f) > 0)
							{
								pr.BetoMng.Check(this.BetoSperm, true, true);
								this.Nai.AddF(NAI.FLAG.INJECTED, (float)(800 + X.xors(2100)));
							}
						}
					}
				}
			}
			return true;
		}

		private void fnFineAbsorbAttack(EnemyFrameDataBasic nF, PxlFrame F)
		{
			if (TX.isStart(F.name, "attack", 0))
			{
				if (this.walk_st == 0)
				{
					this.walk_st = 1;
				}
				if (this.walk_st == -1)
				{
					this.walk_st = -2;
					return;
				}
			}
			else if (this.walk_st >= 2)
			{
				this.walk_st = 0;
			}
		}

		public override float enlarge_level_to_anim_scale(float r = -1f)
		{
			return base.enlarge_level_to_anim_scale(r) * (base.isOverDrive() ? 1.5f : 1f);
		}

		public override void SpSetPose(string nPose, int reset_anmf = -1, string fix_change = null, bool sprite_force_aim_set = false)
		{
			this.EadShotHand.active = nPose == "atk_0";
			base.SpSetPose(nPose, reset_anmf, fix_change, sprite_force_aim_set);
		}

		public bool isTortureInjection(AbsorbManager Ab)
		{
			NelNGolem nelNGolem = Ab.getPublishMover() as NelNGolem;
			return !(nelNGolem == null) && nelNGolem.Absorb != null && nelNGolem.Absorb.target_pose.IndexOf("torture_gorem_inject") == 0;
		}

		public bool isTortureGolem(AbsorbManager Ab)
		{
			NelNGolem nelNGolem = Ab.getPublishMover() as NelNGolem;
			return !(nelNGolem == null) && nelNGolem.Absorb != null;
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			if (this.Nai == null || !this.is_alive)
			{
				return false;
			}
			if (Mg.kind == MGKIND.TACKLE)
			{
				return this.canAbsorbContinue() && this.can_hold_tackle && !this.Ser.has(SER.TIRED);
			}
			if (Mg.kind != MGKIND.BASIC_SHOT)
			{
				return false;
			}
			if (Mg == this.MgShot)
			{
				NaTicket curTicket = this.Nai.getCurTicket();
				return curTicket != null && curTicket.prog == PROG.ACTIVE && curTicket.type == NAI.TYPE.PUNCH;
			}
			return Mg.phase >= 1;
		}

		protected EnAttackInfo AtkShotHit = new EnAttackInfo(0.01f, 0.04f)
		{
			hpdmg0 = 4,
			burst_vx = 0.03f,
			huttobi_ratio = -100f,
			parryable = true,
			Beto = BetoInfo.Mud,
			attr = MGATTR.MUD
		};

		protected EnAttackInfo AtkGroundWave = new EnAttackInfo(0.21f, 0.25f)
		{
			hpdmg0 = 18,
			split_mpdmg = 25,
			huttobi_ratio = 2.5f,
			shield_break_ratio = 0.3f,
			burst_vx = 0.19f,
			burst_vy = -0.1f,
			Beto = BetoInfo.BigBite
		};

		protected NelAttackInfo AtkAbsorbGrab = new NelAttackInfo
		{
			hpdmg0 = 1,
			ndmg = NDMG.GRAB,
			split_mpdmg = 1,
			Beto = BetoInfo.Grab,
			is_penetrate_grab_attack = true
		};

		protected NelAttackInfo AtkAbsorbMain = new NelAttackInfo
		{
			split_mpdmg = 1,
			hpdmg0 = 14,
			attr = MGATTR.NORMAL,
			Beto = BetoInfo.Blood,
			huttobi_ratio = 2f
		}.Torn(0.03f, 0.05f);

		protected EnAttackInfo AtkAbsorbFatal = new EnAttackInfo
		{
			split_mpdmg = 9,
			hpdmg0 = 6,
			attr = MGATTR.ABSORB_V,
			Beto = BetoInfo.Normal.Pow(3, false),
			EpDmg = new EpAtk(9, "golem_od")
			{
				vagina = 1,
				canal = 6,
				gspot = 2
			}
		};

		protected EnAttackInfo AtkAbsorbFatal2 = new EnAttackInfo
		{
			split_mpdmg = 18,
			hpdmg0 = 2,
			attr = MGATTR.ABSORB_V,
			Beto = BetoInfo.Normal.Pow(3, false),
			EpDmg = new EpAtk(11, "golem_od2")
			{
				vagina = 2,
				canal = 6,
				gspot = 2
			}
		};

		protected EnAttackInfo AtkOdAttack = new EnAttackInfo(0.05f, 0.19f)
		{
			hpdmg0 = 30,
			split_mpdmg = 12,
			huttobi_ratio = 2.5f,
			shield_break_ratio = 2f,
			burst_vx = 0.16f,
			burst_vy = -0.06f,
			Beto = BetoInfo.BigBite,
			parryable = true
		};

		protected BetoInfo BetoSperm = BetoInfo.Sperm2.JumpRate(0.7f, false).Level(400f, true);

		protected NOD.TackleInfo[] ATackleOdAttack = NOD.getTackleA(new string[] { "golem_od_attack_0", "golem_od_attack_1", "golem_od_attack_2" });

		protected NOD.TackleInfo[] ATackleOdGrab = NOD.getTackleA(new string[] { "golem_od_grab_0", "golem_od_grab_1" });

		protected NOD.TackleInfo TackleOdGrab_Dead = NOD.getTackle("golem_od_grab_on_dead");

		private const float od_plant_val = 0.7f;

		protected NOD.MpConsume McsShot = NOD.getMpConsume("golem_shot");

		protected NOD.ShockwaveInfo SwiOd = NOD.getShockwave("golem_od_shockwave");

		protected NOD.TackleInfo[] ATackleOdShockwavePrepare = NOD.getTackleA(new string[] { "golem_od_shockwave_prepare_0", "golem_od_shockwave_prepare_1" });

		private const float normal_walk_spd_min = 0.06f;

		private const float normal_walk_spd_max = 0.08f;

		private const float od_walk_spd = 0.09f;

		private const float normal_shot_delay_min = 70f;

		private const float normal_shot_delay_max = 90f;

		private const float normal_shot_spd_min = 0.11f;

		private const float normal_shot_spd_max = 0.23f;

		private const int shot_after_delay = 68;

		public const float shot_gravity_scale = 0.12f;

		public byte need_check_boss;

		private NelNGolem FollowBoss;

		private List<NelNGolem> APawn;

		private const int normal_land_delay = 78;

		private const int od_land_delay = 55;

		private EnemyAnimator.EnemyAdditionalDrawFrame EadShotHand;

		private MagicItem MgShot;

		private static PxlFrame PFStone;

		private NelEnemy.FnCheckEnemy FD_checkNearToy;

		private bool nomove_golem;

		private NelNGolemToy TargetToy;

		private MagicItem.FnMagicRun FD_MgRunNormalShot;

		private MagicItem.FnMagicRun FD_MgDrawNormalShot;

		private EnemyAnimator.FnFineFrame FD_fnFineAbsorbAttack;

		private Func<M2Mover, float> FD_getBossLen;

		private float toy_mp_min;

		private float toy_mp_max = 1f;

		private bool checknear;

		public bool dropable_created_toy = true;

		private const int PRI_MOVE = 1;

		private const int PRI_TOY = 128;

		private const int PRI_PUNCH = 60;

		private NelNGolem ThrowPawn;

		private Func<AbsorbManager, bool> FD_isTortureInjection;

		private Func<AbsorbManager, bool> FD_isTortureGolem;
	}
}
