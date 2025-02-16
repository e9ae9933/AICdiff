using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNMechGolem : NelEnemy
	{
		public override void Awake()
		{
			if (this.Anm == null)
			{
				this.AnmMM = new EnemyAnimatorMultiMech(this);
				this.Anm = this.AnmMM;
				this.AnmMM.addNormalRendHeader("gun").addNormalRendHeader("mech").addNormalRendHeader("pod")
					.addNormalRendHeader("missile");
			}
		}

		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 25f;
			this.Od = new OverDriveManager(this, 162, 102);
			NOD.BasicData basicData;
			if (this.id == ENEMYID.MECHGOLEM_1)
			{
				this.id = ENEMYID.MECHGOLEM_1;
				basicData = NOD.getBasicData("MECHGOLEM_0");
			}
			else
			{
				this.id = ENEMYID.MECHGOLEM_0;
				basicData = NOD.getBasicData("MECHGOLEM_0");
			}
			base.appear(_Mp, basicData);
			NelNMechGolem.floort_danger_ground = X.Mn(NelNMechGolem.floort_danger_ground, this.Mp.floort);
			this.Nai.awake_length = num;
			this.normaljump_level = X.NIXP(0f, 0.7f) * 4.5f;
			this.Nai.attackable_length_x = 9f;
			this.Nai.suit_distance = 10f;
			this.Nai.suit_distance_garaaki_ratio = -1f;
			this.Nai.attackable_length_top = -5f;
			this.Nai.attackable_length_bottom = 4f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnly;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.absorb_weight = 1;
			this.SqJumpNeedle = this.Anm.getCurrentCharacter().getPoseByName("_jump_needle").getSequence(0);
			this.SqShotParts = this.Anm.getCurrentCharacter().getPoseByName("_shot_parts").getSequence(0);
			this.SqOdMissile = this.Anm.getCurrentCharacter().getPoseByName("missile").getSequence(0);
			this.Anm.addAdditionalDrawer(this.EadShotHand = new EnemyAnimator.EnemyAdditionalDrawFrame(this.SqShotParts.getFrame(1), new EnemyAnimator.EnemyAdditionalDrawFrame.FnDrawEAD(this.fnDrawShotHand), false));
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.Hinge != null)
			{
				base.M2D.DeassignPauseable(this.OrbitRgd);
				this.Hinge.destruct();
				IN.DestroyOne(this.OrbitRgd.gameObject);
			}
			if (this.Danger != null)
			{
				this.Danger.destruct();
				this.Danger = null;
			}
			if (this.SndLoopWalk != null)
			{
				this.SndLoopWalk.destruct();
				this.SndLoopWalk = null;
			}
			base.destruct();
		}

		public override void initOverDriveAppear()
		{
			base.initOverDriveAppear();
			this.MhJumpWire.destruct(this);
			this.cannot_move = false;
			this.Anm.checkframe_on_drawing = true;
			this.absorb_weight = 5;
			if (this.FD_eye0 == null)
			{
				this.FD_eye0 = (EffectItem Ef, M2DrawBinder Ed) => this.drawBinderRotateEye(Ed, Ef, "eye_0");
				this.FD_eye1 = (EffectItem Ef, M2DrawBinder Ed) => this.drawBinderRotateEye(Ed, Ef, "eye_1");
				this.FD_eye2 = (EffectItem Ef, M2DrawBinder Ed) => this.drawBinderRotateEye(Ed, Ef, "eye_2");
				this.EfPtcOnceRotateEye = new EfParticleOnce("mgolem_od_rotate_eye", EFCON_TYPE.UI);
			}
			this.od_arrow_attr_grab = 3;
			this.od_arrow_attr = (byte)NightController.xors(4);
			this.Danger = new NASDangerChecker(this, 11f);
			this.Nai.RemF(NAI.FLAG.ATTACKED);
			this.Nai.AddF(NAI.FLAG.WANDERING, 600f);
			this.weight0 = (base.weight = -1f);
			base.addF(NelEnemy.FLAG.CHECK_ENLARGE);
		}

		public override void quitOverDrive()
		{
			base.quitOverDrive();
			this.Anm.checkframe_on_drawing = false;
			this.absorb_weight = 1;
		}

		protected override bool setLandPose()
		{
			bool flag = base.setLandPose();
			this.Nai.delay = X.Mx(this.Nai.delay, (float)(base.isOverDrive() ? 65 : 45));
			return flag;
		}

		public override NelEnemy changeState(NelEnemy.STATE st)
		{
			if (this.state == st)
			{
				return this;
			}
			if (!base.isOverDrive())
			{
				if (st == NelEnemy.STATE.DAMAGE || st == NelEnemy.STATE.DAMAGE_HUTTOBI)
				{
					if (this.Nai.hasTypeLock(NAI.TYPE.MAG))
					{
						this.Nai.addTypeLock(NAI.TYPE.MAG, 120f);
					}
					this.Nai.RemF(NAI.FLAG.WANDERING);
				}
			}
			else if (this.state == NelEnemy.STATE.ABSORB)
			{
				this.Nai.addTypeLock(NAI.TYPE.GUARD_1, 200f);
				this.Nai.addTypeLock(NAI.TYPE.GUARD_2, 200f);
				this.SpSetPose("od_atk2", -1, null, false);
				this.Nai.delay = 60f;
				this.Nai.RemF(NAI.FLAG.ATTACKED);
				this.Nai.AddF(NAI.FLAG.ATTACKED, 100f);
				this.Nai.AddF(NAI.FLAG.WANDERING, X.NIXP(50f, 800f));
			}
			return base.changeState(st);
		}

		public bool normal_too_near_target()
		{
			return this.normal_too_near_target(base.x, base.y);
		}

		public bool normal_too_near_target(float x, float y)
		{
			if (base.isOverDrive())
			{
				return this.Nai.isAttackableLength(x, y, 8f, -4f, 4f, false);
			}
			if (!this.ammo_empty)
			{
				return this.Nai.isAttackableLength(x, y, 3f, -3f, 3f, false);
			}
			return this.Nai.isAttackableLength(x, y, 6f, -5f, 5f, false);
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.HasF(NAI.FLAG.SUMMON_APPEARED, true))
			{
				Nai.addTypeLock(NAI.TYPE.MAG, X.NIXP(40f, 110f));
			}
			if (Nai.fnAwakeBasicHead(Nai) || !base.hasFoot())
			{
				return true;
			}
			bool flag = base.hasPT(3, false, false);
			bool flag2 = NelNMechGolem.floort_danger_ground > 0f && this.Mp.floort < NelNMechGolem.floort_danger_ground + 10f;
			if (!base.hasPT(100, false, flag2))
			{
				if (this.MhJumpWire.isActive(this))
				{
					Nai.AddTicket(NAI.TYPE.GUARD_0, 100, true);
					return true;
				}
				if (flag2)
				{
					this.normaljump_level = 4.5f;
				}
				else
				{
					if (Nai.autotargetted_me)
					{
						this.normaljump_level += (Nai.isFrontType(NAI.TYPE.WAIT, PROG.ACTIVE) ? Nai.NIRANtk(3f, 9f, 449) : (Nai.isFrontType(NAI.TYPE.WALK, PROG.ACTIVE) ? Nai.NIRANtk(1f, 6f, 449) : Nai.NIRANtk(1f, 2f, 449)));
						Nai.autotargetted_lock = 30f;
					}
					bool flag3 = !flag || !Nai.isFrontType(NAI.TYPE.WAIT, PROG.ACTIVE);
					if ((flag3 || Nai.RANtk(589) < 0.4f) && !this.ammo_empty && !Nai.hasTypeLock(NAI.TYPE.MAG) && Nai.isAttackableLength(7f, -6f, 6f, false))
					{
						if (!Nai.isAttackableLength(2f + this.sizex, -2.5f, 2.5f, false) && this.canShootFrom(base.x, base.mbottom))
						{
							return Nai.AddTicketB(NAI.TYPE.MAG, 100, true);
						}
						if (!Nai.HasF(NAI.FLAG.BOTHERED, false))
						{
							this.normaljump_level += Nai.NIRANtk(1f, 2f, 449);
						}
						Nai.AddF(NAI.FLAG.BOTHERED, 150f);
						if (Nai.HasF(NAI.FLAG.POWERED, true))
						{
							Nai.AddF(NAI.FLAG.INJECTED, 150f);
						}
					}
					if (flag3 && this.ammo_empty && base.Useable(this.McsRecharge, 1f, 0f) && !Nai.hasTypeLock(NAI.TYPE.MAG_1))
					{
						if (!this.normal_too_near_target() && !Nai.HasF(NAI.FLAG.ATTACKED, false))
						{
							return Nai.AddTicketB(NAI.TYPE.MAG_1, 100, true);
						}
						if (!Nai.HasF(NAI.FLAG.BOTHERED, false))
						{
							this.normaljump_level += 1f;
						}
						Nai.AddF(NAI.FLAG.BOTHERED, 90f);
					}
				}
				if (this.normaljump_level >= 4.5f)
				{
					Vector4 vector = new Vector4(0f, 0f, -1f, 0f);
					M2BlockColliderContainer.BCCLine bccline = null;
					for (int i = 0; i < 3; i++)
					{
						float num = 1.5707964f + (float)(1 - i) * 0.43633235f;
						M2BlockColliderContainer.BCCLine bccline2;
						Vector3 vector2 = this.Mp.checkThroughBccNearestHitPos(base.x, base.y, base.x + 16f * X.Cos(num), base.y - 16f * X.Sin(num), out bccline2, 0.15f, 0.15f, -1, false, false, null, true);
						if (vector2.z >= 16f)
						{
							M2BlockColliderContainer.BCCLine bccline3 = null;
							float num2 = vector2.x;
							if ((i == 0) ? (bccline2.aim == AIM.L) : (bccline2.aim == AIM.R))
							{
								bccline3 = ((i == 0) ? bccline2.LinkS : bccline2.LinkD);
								if (bccline3 == null || bccline3.foot_aim != AIM.B)
								{
									goto IL_0575;
								}
								vector2.Set(bccline2.x, bccline2.y + 0.25f, X.LENGTHXY2(base.x, base.y, bccline2.x, bccline2.y + 0.25f));
								if (!X.BTW(16f, vector2.z, 256f))
								{
									goto IL_0575;
								}
								num2 = ((i == 0) ? (bccline3.right - 0.5f) : (bccline3.x + 0.5f));
							}
							if (bccline3 == null)
							{
								bccline3 = this.Mp.getSideBcc((int)vector2.x, (int)vector2.y + 1, AIM.B);
							}
							if (bccline3 != null && CAim._XD(bccline3.foot_aim, 1) == 0 && X.Abs(X.angledifR(1.5707964f, this.Mp.GAR(base.x, base.y, vector2.x, vector2.y))) <= 0.62831855f)
							{
								float num3 = (float)((i == 1) ? 0 : (flag2 ? 90 : 20));
								if ((i == 0 && base.x > Nai.target_x) || (i == 2 && base.x < Nai.target_x))
								{
									num3 += 10f;
								}
								float num4 = bccline3.slopeBottomY(vector2.x);
								if (this.normal_too_near_target(num2, num4))
								{
									num3 += 30f - X.LENGTHXYS(Nai.target_x, Nai.target_y, num2, num4);
								}
								if (!this.ammo_empty && !this.canShootFrom(num2, num4))
								{
									num3 += 15f;
								}
								if (vector.z < 0f || vector.w > num3)
								{
									vector.Set(vector2.x, vector2.y, (float)((i == 0) ? 4 : ((i == 1) ? 1 : 5)), num3);
									bccline = bccline3;
								}
							}
						}
						IL_0575:;
					}
					if (vector.z >= 0f)
					{
						NaTicket naTicket = Nai.AddTicket(NAI.TYPE.GUARD, 100, true);
						naTicket.DepBCC = bccline;
						naTicket.Dep(vector.x, vector.y, null).SetAim((int)vector.z);
						this.normaljump_level = 0f;
						return true;
					}
					this.normaljump_level = 3.5f;
				}
				if (flag)
				{
					Nai.delay = X.Mx(Nai.delay, 20f);
				}
			}
			if (!flag)
			{
				Nai.suit_distance = (this.ammo_empty ? 12.5f : 7f);
				float num5;
				float num6;
				M2BlockColliderContainer.BCCLine bccline4;
				M2BlockColliderContainer.BCCLine bccline5;
				if (this.getLinearWalkableArea(out num5, out num6, out bccline4, out bccline5, 12f))
				{
					Vector4 vector3 = new Vector4(0f, 0f, -1f, 0f);
					for (int j = 0; j < 2; j++)
					{
						float num7 = ((j == 0) ? X.Mx(Nai.target_x - Nai.suit_distance, num5 + 0.3f) : X.Mn(Nai.target_x + Nai.suit_distance, num6 - 0.3f));
						float footableY = this.Mp.getFootableY(num7, (int)base.y, 12, true, -1f, false, true, true, 0f);
						if ((X.Abs(Nai.target_x - num7) >= 3f || X.Abs(Nai.target_lastfoot_bottom - footableY) >= 4f) && !Nai.isDecliningXy(num7, footableY, 0f, 0f))
						{
							float num8 = 0f;
							num8 += X.Abs(X.LENGTHXYS(num7, footableY, Nai.target_x, Nai.target_y) - Nai.suit_distance) * 3f;
							num8 += (float)((this.canShootFrom(num7, footableY) != this.ammo_empty) ? 0 : 20);
							if (vector3.z < 0f || vector3.w > num8)
							{
								vector3.Set(num7, footableY, (float)((j == 0) ? 0 : 2), num8);
							}
						}
					}
					if (vector3.z >= 0f)
					{
						if (X.Abs(vector3.x - base.x) < 0.4f)
						{
							this.normaljump_level += (float)(Nai.HasF(NAI.FLAG.WANDERING, false) ? 10 : 1);
							Nai.remTypeLock(NAI.TYPE.MAG);
							Nai.remTypeLock(NAI.TYPE.MAG_1);
							Nai.AddF(NAI.FLAG.WANDERING, 180f);
							return Nai.AddTicketB(NAI.TYPE.WAIT, 3, true);
						}
						if (Nai.HasF(NAI.FLAG.BOTHERED, false) && Nai.HasF(NAI.FLAG.INJECTED, false))
						{
							vector3.x = X.NI(base.x, vector3.x, X.NIXP(0.2f, 0.8f));
							Nai.RemF(NAI.FLAG.BOTHERED);
							Nai.RemF(NAI.FLAG.INJECTED);
						}
						Nai.AddTicket(NAI.TYPE.WALK, 3, true).Dep(vector3.x, vector3.y, null);
						return true;
					}
					else
					{
						this.normaljump_level += 4.5f;
						Nai.delay = X.Mx(Nai.delay, 20f);
					}
				}
			}
			return Nai.AddTicketB(NAI.TYPE.WAIT, 1, true);
		}

		private bool ammo_empty
		{
			get
			{
				return this.ammo == 0;
			}
		}

		private bool canShootFrom(float _x, float bottomy)
		{
			return this.Mp.canThroughBcc(_x, bottomy - 1.25f, this.Nai.target_x, this.Nai.target_y, 0.15f, 0.15f, -1, false, false, null, true, null);
		}

		private bool getLinearWalkableArea(out float area_left, out float area_right, out M2BlockColliderContainer.BCCLine LBcc, out M2BlockColliderContainer.BCCLine RBcc, float search_max = 12f)
		{
			M2BlockColliderContainer.BCCLine lastBCC = this.FootD.get_LastBCC();
			if (lastBCC == null)
			{
				area_left = (area_right = base.x);
				M2BlockColliderContainer.BCCLine bccline;
				RBcc = (bccline = null);
				LBcc = bccline;
				return false;
			}
			float num = X.Abs(base.x - this.Nai.target_x);
			float num2 = ((num < 4.2f && this.Nai.RANtk(7166) < ((num < 1.25f || this.Nai.HasF(NAI.FLAG.WANDERING, false)) ? 0.77f : (this.ammo_empty ? 0.04f : 0.15f))) ? (search_max * 0.75f + num) : X.Mx(0f, num - 2f));
			if (base.x < this.Nai.target_x)
			{
				lastBCC.getLinearWalkableArea(0f, out area_left, out area_right, out LBcc, out RBcc, base.x - search_max, base.x + num2);
			}
			else
			{
				lastBCC.getLinearWalkableArea(0f, out area_left, out area_right, out LBcc, out RBcc, base.x - num2, base.x + search_max);
			}
			return true;
		}

		public override bool readTicket(NaTicket Tk)
		{
			if (base.isOverDrive())
			{
				return this.readTicketOd(Tk);
			}
			NAI.TYPE type = Tk.type;
			if (type != NAI.TYPE.WALK)
			{
				switch (type)
				{
				case NAI.TYPE.MAG:
					return this.runGunShot(Tk.initProgress(this), Tk);
				case NAI.TYPE.MAG_0:
				case NAI.TYPE.MAG_2:
					break;
				case NAI.TYPE.MAG_1:
					return this.runGunRecharge(Tk.initProgress(this), Tk);
				case NAI.TYPE.GUARD:
					return this.runWireWalk(Tk.initProgress(this), Tk);
				case NAI.TYPE.GUARD_0:
					return this.runWireClose(Tk.initProgress(this), Tk);
				default:
					if (type == NAI.TYPE.WAIT)
					{
						base.AimToLr((X.xors(2) == 0) ? 0 : 2);
						Tk.after_delay = 20f + this.Nai.RANtk(840) * 30f;
						return false;
					}
					break;
				}
				return base.readTicket(Tk);
			}
			bool flag = Tk.initProgress(this);
			int num = base.walkThroughLift(flag, Tk, 20);
			if (num >= 0)
			{
				return num == 0;
			}
			return this.runWalkNormal(flag, Tk);
		}

		private bool runWalkNormal(bool init_flag, NaTicket Tk)
		{
			if (this.Nai.HasF(NAI.FLAG.BOTHERED, false))
			{
				this.Nai.AddF(NAI.FLAG.BOTHERED, 1f);
			}
			if (this.Nai.HasF(NAI.FLAG.INJECTED, false))
			{
				this.Nai.AddF(NAI.FLAG.INJECTED, 1f);
			}
			float num = 160f;
			if (init_flag)
			{
				float num2 = this.Nai.NIRANtk(0.11f, 0.135f, 4811);
				this.walk_st = X.IntC(X.Abs(Tk.depx - base.x) / num2);
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
				if (this.t > num || flag || NelNMechGolem.floort_danger_ground > this.Mp.floort + 5f)
				{
					if (base.isOverDrive() ? (this.Nai.target_foot_slen > 3f) : (this.Nai.target_len > 5f))
					{
						this.Nai.delay = this.Nai.NIRANtk(20f, 30f, 3189);
					}
					Tk.check_nearplace_error = (flag ? 2 : 0);
					if (flag && this.t < 5f)
					{
						this.Nai.addDeclineArea(Tk.depx - 1f, Tk.depy - 5f, 2f, 10f, 200f);
					}
					base.AimToPlayer();
					return false;
				}
				if (!base.isOverDrive() && this.t >= 30f && this.Nai.isDangerousWalk((num3 > 0f) ? AIM.R : AIM.L, true))
				{
					this.Nai.delay = this.Nai.NIRANtk(40f, 60f, 3189);
					return false;
				}
				this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, num3 * base.mpf_is_right, 0f, -1f, -1, 1, 0, -1, 0);
			}
			return true;
		}

		private float setR(float rotR, bool fine_Hinge_pos = true)
		{
			if (this.Anm.rotationR == rotR)
			{
				return rotR;
			}
			this.Anm.rotationR = rotR;
			if (this.Hinge != null)
			{
				this.Hinge.first_hinge_agR = rotR;
			}
			return rotR;
		}

		private bool runWireWalk(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.playSndPos("gimmick_keyboard", 1);
				this.SpSetPose("jump0", -1, null, false);
				this.Anm.rotationR = 0f;
				NelNGolemToyRainMaker.initHinge(this, this.Mp, this.Phy.getRigidbody(), this.WireShotShiftU, 16f, 0.15f, ref this.Hinge, ref this.OrbitRgd);
				this.Hinge.first_lock_rotation = true;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 20, true))
			{
				if (this.FD_MgDrawWireShot == null)
				{
					this.FD_MgRunWireShot = new MagicItem.FnMagicRun(this.MgRunWireShot);
					this.FD_MgDrawWireShot = new MagicItem.FnMagicRun(this.MgDrawWireShot);
				}
				MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunWireShot, this.FD_MgDrawWireShot);
				this.MhJumpWire = new MagicItemHandlerS(magicItem);
				Vector2 wireShotSPos = this.WireShotSPos;
				magicItem.sx = wireShotSPos.x;
				magicItem.sy = wireShotSPos.y;
				magicItem.sa = ((Tk.aim == 1) ? 1.5707964f : this.Mp.GAR(magicItem.sx, magicItem.sy, Tk.depx, Tk.depy));
				this.setR(magicItem.sa - 1.5707964f, true);
				this.SpSetPose("jump1", -1, null, false);
				magicItem.run(0f);
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (!this.MhJumpWire.isActive(this))
				{
					return false;
				}
				if (this.MhJumpWire.Mg.phase == 5)
				{
					Tk.Recreate(NAI.TYPE.GUARD_0, 100, false, null);
					return true;
				}
				if (this.MhJumpWire.Mg.phase >= 10)
				{
					if (Tk.Progress(ref this.t, 16, true))
					{
						this.walk_st = 0;
						base.PtcVar("by", (double)(base.mbottom - 0.2f)).PtcST("mgolem_jumpwire_jumpinit", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
						this.Phy.addLockGravityFrame(10);
						this.FootD.initJump(false, false, false);
						base.throw_ray = true;
						this.SpSetPose("jump2", -1, null, false);
						if (CAim._XD(Tk.aim, 1) != 0)
						{
							Tk.prog = PROG.PROG5;
						}
						else
						{
							Tk.prog = PROG.PROG1;
							this.setR(0f, true);
							bool flag = NelNMechGolem.floort_danger_ground > 0f && this.Mp.floort < NelNMechGolem.floort_danger_ground + 10f;
							this.walk_time = X.Mn(this.Nai.NIRANtk(2.2f, 3.6f, 115), this.Hinge.max_length * this.Nai.NIRANtk(0.2f, 0.33f, 1274)) * (flag ? 0.6f : 1f);
						}
					}
				}
				else
				{
					this.t = 0f;
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (!this.MhJumpWire.isActive(this) || this.MhJumpWire.Mg.phase < 10 || !(this.MhJumpWire.Mg.Other is M2BlockColliderContainer.BCCLine) || (base.hasFoot() && this.t >= 10f))
				{
					Tk.prog = PROG.PROG4;
					this.t = 0f;
					this.walk_st = 0;
					return true;
				}
				this.Phy.addLockGravityFrame(2);
				if (this.walk_st == 0)
				{
					float num = 0.25f * X.NI(3, 1, X.ZSIN(this.t, 6f));
					this.MhJumpWire.Mg.phase = 11;
					this.MhJumpWire.Mg.t = 0f;
					this.Hinge.max_length -= num * this.Mp.CLENB * 0.015625f;
					float num2 = num * X.Cos(1.5707964f);
					float num3 = -num * X.Sin(1.5707964f);
					this.FootD.initJump(false, false, false);
					this.Phy.addFoc(FOCTYPE.WALK, num2, num3, -1f, -1, 1, 0, -1, 0);
					this.Hinge.LgtReset(true, new Vector2(num2, -num3) * (this.Mp.CLENB * 0.015625f));
					if (this.Hinge.max_length < this.walk_time)
					{
						base.throw_ray = false;
						this.walk_st = 1;
						this.t = 0f;
					}
				}
				if (this.walk_st == 1)
				{
					float num4 = (1f - X.ZLINE(this.t, 20f)) * 0.05f;
					this.MhJumpWire.Mg.phase = 11;
					this.MhJumpWire.Mg.t = 0f;
					if (num4 > 0f)
					{
						this.Hinge.max_length += num4 * this.Mp.CLENB * 0.015625f;
						this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._GRAVITY_LOCK, num4 * X.Cos(-1.5707964f), -num4 * X.Sin(-1.5707964f), -1f, -1, 1, 0, -1, 0);
						this.Hinge.LgtReset(true, default(Vector2));
					}
					bool flag2 = NelNMechGolem.floort_danger_ground > 0f && this.Mp.floort < NelNMechGolem.floort_danger_ground + this.Nai.NIRANtk(40f, 80f, 1319);
					if (this.t >= 30f && ((this.Nai.autotargetted_me && (!flag2 || this.Nai.RANtk(768) < 0.2f)) || (!flag2 && this.Nai.isPrGaraakiState())))
					{
						Tk.prog = PROG.PROG4;
					}
					if (flag2)
					{
						this.t = X.Mn(this.t, 30f);
					}
					if (this.t >= this.Nai.NIRANtk(60f, 140f, 2314))
					{
						Tk.prog = PROG.PROG4;
					}
				}
			}
			if (Tk.prog == PROG.PROG4)
			{
				if (base.hasFoot())
				{
					this.MhJumpWire.destruct(this);
					return false;
				}
				float num5 = this.Nai.getMpfEscapeSide(0f, 0f, 1.8f, 0.8f, 3f, 1f, true, this.Nai.RANtk(681) < 0.8f) * this.Nai.NIRANtk(0.2f, 0.3f, 1741);
				float num6 = -0.15f;
				this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._RELEASE, num5, num6, -1f, 0, 20, 0, -1, 0);
				this.walk_st = 1;
				Tk.prog = PROG.PROG5;
				this.t = 0f;
				this.SpSetPose("jump3", -1, null, false);
				base.PtcVar("agR", (double)this.Mp.GAR(0f, 0f, num5, num6)).PtcST("mgolem_jumpwire_release", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (Tk.prog == PROG.PROG5)
			{
				if (this.walk_st == 0)
				{
					float num7 = 0.22f * X.NI(3, 1, X.ZSIN(this.t, 6f));
					float num8 = num7;
					float num9 = num7;
					float num10 = this.Anm.rotationR + 1.5707964f;
					if (!this.MhJumpWire.isActive(this) || this.MhJumpWire.Mg.phase < 10 || !(this.MhJumpWire.Mg.Other is M2BlockColliderContainer.BCCLine) || (base.hasFoot() && this.t >= 10f))
					{
						this.walk_st = 1;
					}
					else
					{
						this.MhJumpWire.Mg.phase = 11;
						this.MhJumpWire.Mg.t = 0f;
						M2BlockColliderContainer.BCCLine bccline = this.MhJumpWire.Mg.Other as M2BlockColliderContainer.BCCLine;
						if (CAim._XD(bccline.foot_aim, 1) != 0)
						{
							if (base.y <= X.Mx(bccline.y + 0.5f, Tk.depy))
							{
								this.walk_st = 1;
							}
							else
							{
								float num11 = this.Mp.GAR(base.x, base.y, this.MhJumpWire.Mg.sx, this.MhJumpWire.Mg.sy);
								float num12 = this.Mp.GAR(0f, 0f, num7 * X.Cos(num11), -num7 * X.Sin(num11) - 0.15f);
								float num13 = X.angledifR(num11, num12);
								this.setR(num11 - 1.5707964f, false);
								num8 *= X.Abs(X.Cos(num13));
								num10 = num11;
							}
						}
						else
						{
							num10 = this.Mp.GAR(base.x, base.y, this.MhJumpWire.Mg.sx, this.MhJumpWire.Mg.sy);
							this.setR(num10 - 1.5707964f, false);
							if (base.y <= X.Mx(bccline.y, Tk.depy + this.Nai.NIRANtk(3f, 5f, 4133)))
							{
								this.walk_st = 1;
							}
						}
					}
					float num14 = num9 * X.Cos(num10);
					float num15 = -num9 * X.Sin(num10);
					if (this.walk_st == 1 || (this.t > 20f && base.vy > 0f))
					{
						this.walk_st = 1;
						this.t = 0f;
						this.SpSetPose("jump3", -1, null, false);
						base.PtcVar("agR", (double)this.Mp.GAR(0f, 0f, num14, num15)).PtcST("mgolem_jumpwire_release", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._RELEASE, num14, num15 - 0.05f, -1f, 0, 20, 0, -1, 0);
						base.throw_ray = false;
					}
					else
					{
						this.Hinge.max_length -= num8 * this.Mp.CLENB * 0.015625f;
						if (this.Hinge.max_length <= 0f)
						{
							base.PtcST("mgolem_jumpwire_close_quit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							this.MhJumpWire.destruct(this);
						}
						else
						{
							this.Hinge.LgtReset(true, new Vector2(num14, -num15) * (this.Mp.CLENB * 0.015625f));
						}
						this.FootD.initJump(false, false, false);
						this.Phy.addLockGravityFrame(5);
						this.Phy.addFoc(FOCTYPE.WALK, num14, num15, -1f, -1, 1, 0, -1, 0);
					}
				}
				else
				{
					this.setR(0f, false);
					if (this.MhJumpWire.isActive(this))
					{
						this.Hinge.max_length -= 0.08f * this.Mp.CLENB * 0.015625f;
						if (this.Hinge.max_length <= 0f)
						{
							base.PtcST("mgolem_jumpwire_close_quit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							base.killPtc("mgolem_jumpwire_close", false);
							this.MhJumpWire.destruct(this);
						}
						else
						{
							this.Hinge.LgtReset(true, default(Vector2));
							if (base.hasFoot() && this.SpPoseIs("stand"))
							{
								this.SpSetPose("jump0", -1, null, false);
								base.PtcST("mgolem_jumpwire_close", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
							}
						}
					}
					else if (base.hasFoot())
					{
						Tk.AfterDelay(25f);
						return false;
					}
				}
			}
			return true;
		}

		private bool runWireClose(bool init_flag, NaTicket Tk)
		{
			bool flag = true;
			if (!this.MhJumpWire.isActive(this) || this.Hinge == null)
			{
				flag = false;
			}
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				this.SpSetPose("jump0", -1, null, false);
				this.setR(0f, true);
			}
			if (flag)
			{
				if (this.walk_st == 0 && this.MhJumpWire.Mg.phase == 5)
				{
					this.walk_st = 1;
					base.PtcST("mgolem_jumpwire_close", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				}
				this.OrbitRgd.velocity = Vector2.zero;
				this.Hinge.max_length -= ((X.Abs(this.MhJumpWire.Mg.sx - base.x) < 0.5f) ? 0.15f : ((this.MhJumpWire.Mg.phase == 5) ? 0.07f : 0.01f)) * this.Mp.CLENB * 0.015625f;
				if (this.Hinge.max_length <= 0f)
				{
					flag = false;
				}
				else
				{
					Vector2 wireShotSPos = this.WireShotSPos;
					if (X.LENGTHXYS(wireShotSPos.x, wireShotSPos.y * 0.5f, this.MhJumpWire.Mg.sx, this.MhJumpWire.Mg.sy * 0.5f) < 0.5f)
					{
						flag = false;
					}
					else
					{
						this.Hinge.LgtReset(true, default(Vector2));
					}
				}
			}
			if (!flag)
			{
				this.MhJumpWire.destruct(this);
				this.SpSetPose("stand", -1, null, false);
				base.killPtc("mgolem_jumpwire_close", false);
				base.PtcST("mgolem_jumpwire_close_quit", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				if (!this.ammo_empty)
				{
					Tk.AfterDelay(this.Nai.NIRANtk(40f, 65f, 3923));
				}
				else
				{
					Tk.AfterDelay(this.Nai.NIRANtk(10f, 20f, 3923));
				}
				return false;
			}
			return true;
		}

		private Vector2 WireShotSPos
		{
			get
			{
				return new Vector2(base.x, base.y) + this.WireShotShift;
			}
		}

		private Vector2 WireShotShift
		{
			get
			{
				return new Vector2(0f, -this.sizey * 0.9f);
			}
		}

		private Vector2 WireShotShiftU
		{
			get
			{
				Vector2 wireShotShift = this.WireShotShift;
				return new Vector2(wireShotShift.x, -wireShotShift.y) * (this.Mp.CLENB * 0.015625f);
			}
		}

		private bool MgRunWireShot(MagicItem Mg, float fcnt)
		{
			Map2d mp = Mg.Mp;
			float ts = Map2d.TS;
			float base_TS = this.base_TS;
			if (this.Hinge == null || base.destructed)
			{
				return false;
			}
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.Ray.RadiusM(0.15f).HitLock(40f, null);
				Mg.Ray.projectile_power = -100;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.Ray.LenM(0.125f);
				Mg.Ray.check_hit_wall = true;
				Mg.Ray.check_other_hit = false;
				Mg.calcAimPos(false);
				Mg.sz = 0f;
				Mg.dx = X.Cos(Mg.sa) * 0.65f;
				Mg.dy = -X.Sin(Mg.sa) * 0.65f;
				Mg.dz = 0f;
				Mg.da = 0f;
				Mg.efpos_s = (Mg.raypos_s = true);
				Mg.aimagr_calc_vector_d = true;
				this.Hinge.end_gravity_scale = 0f;
				this.OrbitRgd.mass = (this.Hinge.mass = 0.025f);
				this.Hinge.max_length = 16f * mp.CLENB * 0.015625f;
				this.Hinge.BaseHingeAnchor = this.WireShotShiftU;
				this.Hinge.LgtReset(false, default(Vector2)).PosReset();
				this.Hinge.end_gravity_scale = 0f;
				this.Hinge.maxDistanceOnly = true;
				this.Hinge.wire_gravity_scale = 0f;
				this.Hinge.enabled = true;
				this.OrbitRgd.sharedMaterial = MTRX.PmdM2Fric;
				this.OrbitRgd.rotation = Mg.sa * 0.31830987f * 180f;
				Mg.PtcST("mgolem_jumpwire_shot", PTCThread.StFollow.NO_FOLLOW, false);
				this.MgSpdToRgd(Mg);
			}
			else if (Mg.phase != 11)
			{
				this.RgdToMgSpd(Mg);
			}
			if (fcnt == 0f)
			{
				return true;
			}
			if (!this.Hinge.enabled)
			{
				return false;
			}
			if (Mg.phase == 1 || Mg.phase == 2)
			{
				if (Mg.phase == 1 && (this.state != NelEnemy.STATE.STAND || !this.Nai.isFrontType(NAI.TYPE.GUARD, PROG.ACTIVE)))
				{
					Mg.phase = 2;
					this.OrbitRgd.angularVelocity = (float)X.MPF(Mg.dx < 0f) * (1f / X.NIXP(45f, 85f)) * 60f * 360f;
				}
				Mg.calcAimPos(false);
				if (Mg.t < 40f && Mg.da == 0f)
				{
					Mg.dx = X.Cos(Mg.sa) * 0.65f;
					Mg.dy = -X.Sin(Mg.sa) * 0.65f;
					Mg.aim_agR = Mg.sa;
					Mg.calced_aim_pos = true;
					this.MgSpdToRgd(Mg);
					this.OrbitRgd.rotation = Mg.sa * 0.31830987f * 180f;
				}
				if (Mg.t < 90f)
				{
					if (Mg.phase == 1 && Mg.t >= 3f)
					{
						float num = Mg.sx + 0.25f * X.Cos(Mg.sa);
						float num2 = Mg.sy - 0.25f * X.Sin(Mg.sa);
						M2BlockColliderContainer.BCCLine bccline;
						if (!mp.canStandAndNoBlockSlope((int)num, (int)num2) && mp.checkThroughBccNearestHitPos(Mg.sx, Mg.sy, num, num2, out bccline, 0.15f, 0.15f, -1, false, false, null, true).z >= 0f && bccline.foot_aim != AIM.B)
						{
							Mg.killEffect();
							Vector3 vector = bccline.crosspoint(Mg.sx, Mg.sy, num, num2, 0.15f, 0.15f);
							if (vector.z >= 2f)
							{
								Mg.sx = vector.x;
								Mg.sy = vector.y;
							}
							else
							{
								Mg.sx = X.NI(num, Mg.sx, 0.5f);
								Mg.sy = X.NI(num2, Mg.sy, 0.5f);
							}
							Mg.dx = (Mg.dy = 0f);
							Mg.sa = Mg.aim_agR;
							Mg.PtcST("mgolem_jumpwire_hit", PTCThread.StFollow.NO_FOLLOW, false);
							this.Hinge.end_gravity_scale = 0f;
							this.Hinge.wire_gravity_scale = 0.3f;
							this.OrbitRgd.angularVelocity = 0f;
							this.OrbitRgd.rotation = Mg.sa * 0.31830987f * 180f;
							this.MgSpdToRgd(Mg);
							Mg.phase = 11;
							Mg.t = -50f;
							Mg.Other = bccline;
							Mg.MnSetRay(Mg.Ray, 0, Mg.aim_agR, 0f);
							Vector2 wireShotSPos = this.WireShotSPos;
							this.Hinge.max_length = X.LENGTHXY(Mg.sx, Mg.sy, wireShotSPos.x, wireShotSPos.y) * mp.CLENB * 0.015625f;
							return true;
						}
					}
				}
				else
				{
					Mg.phase = 2;
				}
				if (Mg.t >= 58f || Mg.da > 0f || Mg.phase == 2)
				{
					this.Hinge.maxDistanceOnly = false;
					this.Hinge.end_gravity_scale = 0.4f;
					this.Hinge.wire_gravity_scale = this.Hinge.end_gravity_scale;
					Mg.sa = X.VALWALKANGLER(Mg.sa, this.OrbitRgd.rotation * 3.1415927f / 180f, 0.009424779f * fcnt);
					bool flag = Mg.t >= 200f;
					if (flag || X.LENGTHXYS(0f, 0f, Mg.dx, Mg.dy) < 0.014f)
					{
						Mg.da += fcnt;
						if (flag || Mg.da >= 20f)
						{
							Mg.killEffect();
							Mg.phase = 5;
							Mg.t = 0f;
							this.OrbitRgd.sharedMaterial = MTRX.PmdM2Bouncy;
						}
					}
					else
					{
						Mg.da = 0.01f;
						if (Mg.dz == 2f)
						{
							Mg.dz = 1f;
						}
					}
				}
				else
				{
					Mg.Ray.clearTempReflect();
					Mg.MnSetRay(Mg.Ray, 0, Mg.aim_agR, 0f);
					HITTYPE hittype = Mg.Ray.Cast(true, null, false);
					if ((hittype & (HITTYPE)4194336) != HITTYPE.NONE)
					{
						if (Mg.reflectAgR(Mg.Ray, ref Mg.aim_agR, 0.25f))
						{
							Mg.dx += X.Cos(Mg.aim_agR) * 0.13f;
							Mg.dy += X.Abs(X.Sin(Mg.aim_agR) * 0.13f);
							this.OrbitRgd.angularVelocity = (float)X.MPF(Mg.dx < 0f) * (1f / X.NIXP(25f, 45f)) * 60f * 360f;
						}
						Mg.phase = 2;
					}
					else if ((hittype & HITTYPE.WALL) != HITTYPE.NONE)
					{
						Mg.da = X.Mx(Mg.da, 0.01f);
					}
				}
			}
			int phase = Mg.phase;
			if (Mg.phase >= 10)
			{
				if (this.state != NelEnemy.STATE.STAND || !this.Nai.isFrontType(NAI.TYPE.GUARD, PROG.ACTIVE))
				{
					Mg.phase = 5;
					this.Hinge.maxDistanceOnly = false;
					this.Hinge.end_gravity_scale = 0.4f;
					this.Hinge.wire_gravity_scale = this.Hinge.end_gravity_scale;
					Mg.t = 0f;
					this.OrbitRgd.sharedMaterial = MTRX.PmdM2Bouncy;
					this.OrbitRgd.angularVelocity = (float)X.MPF(X.Cos(Mg.sa) > 0f) * (1f / X.NIXP(45f, 85f)) * 60f * 360f;
					return true;
				}
				if (Mg.phase == 11)
				{
					this.OrbitRgd.mass = (this.Hinge.mass = 9000f);
					this.OrbitRgd.angularVelocity = 0f;
					this.OrbitRgd.rotation = Mg.sa * 0.31830987f * 180f;
					this.Hinge.end_gravity_scale = 0f;
					Vector2 mapPos = Mg.Ray.getMapPos(0f);
					Mg.sx = mapPos.x;
					Mg.sy = mapPos.y;
					Mg.dx = 0f;
					Mg.dy = 0f;
					this.MgSpdToRgd(Mg);
					if (Mg.t >= 7f)
					{
						Mg.phase = 10;
						Mg.t = 0f;
					}
				}
				else
				{
					this.OrbitRgd.mass = (this.Hinge.mass = 0.025f);
					this.Hinge.end_gravity_scale = 0.25f;
				}
			}
			return true;
		}

		private void MgSpdToRgd(MagicItem Mg)
		{
			if (this.Hinge != null && Mg != null && this.Mp != null)
			{
				this.OrbitRgd.velocity = new Vector2(this.Mp.mapvxToUVelocityx(Mg.dx * Map2d.TS), this.Mp.mapvyToUVelocityy(Mg.dy * Map2d.TS));
			}
		}

		private void RgdToMgSpd(MagicItem Mg)
		{
			if (this.Hinge != null && Mg != null && this.Mp != null)
			{
				Vector2 velocity = this.OrbitRgd.velocity;
				Vector2 vector = this.OrbitRgd.transform.localPosition;
				Mg.dx = this.Mp.uVelocityxToMapx(velocity.x / Map2d.TS);
				Mg.dy = this.Mp.uVelocityyToMapy(velocity.y / Map2d.TS);
				Mg.sx = this.Mp.uxToMapx(vector.x);
				Mg.sy = this.Mp.uyToMapy(vector.y);
			}
		}

		private bool MgDrawWireShot(MagicItem Mg, float fcnt)
		{
			EffectItem ef = Mg.Ef;
			MeshDrawer meshImg = ef.GetMeshImg("", this.Anm.getMI(), BLEND.NORMAL, false);
			if (meshImg.getTriMax() == 0)
			{
				meshImg.base_z -= 0.01f;
			}
			meshImg.initForImg(this.SqJumpNeedle.getImage(0, 0), 0).RotaGraph(0f, 0f, 1f, this.OrbitRgd.rotation / 180f * 3.1415927f, null, false);
			MeshDrawer mesh = ef.GetMesh("", uint.MaxValue, BLEND.NORMAL, true);
			float base_x = mesh.base_x;
			float base_y = mesh.base_y;
			Vector2 vector;
			if (this.Hinge.enabled && this.Hinge.DrawBegin(out vector))
			{
				mesh.Col = C32.d2c(2009910476U);
				mesh.base_x = 0f;
				mesh.base_y = 0f;
				Vector2 wireShotSPos = this.WireShotSPos;
				if (Mg.phase == 1 || Mg.phase >= 10)
				{
					mesh.Line(this.Mp.map2globalux(Mg.sx), this.Mp.map2globaluy(Mg.sy), this.Mp.map2globalux(wireShotSPos.x), this.Mp.map2globaluy(wireShotSPos.y), 0.03125f, true, 0f, 0f);
				}
				else
				{
					mesh.Line(vector.x, vector.y, this.Mp.map2globalux(wireShotSPos.x), this.Mp.map2globaluy(wireShotSPos.y), 0.03125f, true, 0f, 0f);
					Vector2 vector2;
					while (this.Hinge.DrawNext(out vector2))
					{
						mesh.Line(vector.x, vector.y, vector2.x, vector2.y, 0.03125f, true, 0f, 0f);
						vector = vector2;
					}
				}
			}
			return true;
		}

		private bool runGunShot(bool init_flag, NaTicket Tk)
		{
			int num = this.runGunShotInner(init_flag, Tk);
			if (num < 0)
			{
				this.Nai.AddF(NAI.FLAG.BOTHERED, 180f).AddF(NAI.FLAG.INJECTED, 180f);
			}
			if (num == 1 && this.ammo_empty)
			{
				this.Nai.AddF(NAI.FLAG.ATTACKED, X.NIXP(200f, 300f));
			}
			if (num == 0)
			{
				return true;
			}
			if (num == 1)
			{
				Tk.AfterDelay(X.NIXP(55f, 70f));
			}
			return false;
		}

		private int runGunShotInner(bool init_flag, NaTicket Tk)
		{
			MagicItem magicItem;
			if (init_flag)
			{
				base.AimToPlayer();
				this.walk_st = 20 - X.xors(12);
				this.t = 120f - X.NIXP(8f, 18f) * (float)this.Summoner.countActiveEnemy((NelEnemy N) => N is NelNMechGolem && !N.isOverDrive() && N.getAI().isFrontType(NAI.TYPE.MAG, PROG.ACTIVE), true);
				base.PtcST("mgolem_gunshot_init", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				this.SpSetPose("shot0", -1, null, false);
				this.EadShotHand.active = true;
				if (this.FD_MgDrawGunShot == null)
				{
					this.FD_MgRunGunShot = new MagicItem.FnMagicRun(this.MgRunGunShot);
					this.FD_MgDrawGunShot = new MagicItem.FnMagicRun(this.MgDrawGunShot);
				}
				magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunGunShot, this.FD_MgDrawGunShot);
				this.MhGun = new MagicItemHandlerS(magicItem);
			}
			else
			{
				if (!this.MhGun.isActive(this))
				{
					return -1;
				}
				magicItem = this.MhGun.Mg;
			}
			if (this.walk_st > -1000)
			{
				float num = X.angledifR(this.gun_agR, magicItem.sa);
				if (X.Abs(num) < 0.006283186f)
				{
					this.gun_agR = magicItem.sa;
				}
				else
				{
					this.gun_agR += 3.1415927f * (0.0017f * X.NI(15, 1, X.ZLINE(magicItem.t, 45f))) * (float)X.MPF(num > 0f) * this.TS;
					if (X.Abs(num) > 0.15707964f && !this.Nai.isAttackableLength(2f, -4f, 4f, false))
					{
						magicItem.dz = 0f;
					}
				}
				if (NelNMechGolem.floort_danger_ground > 0f && this.Mp.floort < NelNMechGolem.floort_danger_ground + 10f)
				{
					return -1;
				}
				if (this.t >= 200f)
				{
					if (base.mpf_is_right > 0f != base.x < this.Nai.target_x)
					{
						this.walk_st -= 5;
						this.t = 194f;
					}
					else if (magicItem.dz < 4f)
					{
						this.t = 194f;
						this.walk_st--;
					}
					else
					{
						if (this.canShootFrom(base.x, base.mbottom))
						{
							this.t = 0f;
							base.killPtc("mgolem_gunshot_init", false);
							magicItem.phase = 10;
							this.walk_time = (magicItem.sa = this.gun_agR);
							this.SpSetPose("shot2", -1, null, false);
							this.EadShotHand.active = true;
							this.ammo -= 1;
							this.walk_st = -1000;
							this.MhGun.release();
							return 0;
						}
						this.walk_st -= 2;
						this.t = 200f - X.NIXP(20f, 50f);
					}
					if (this.walk_st <= 0)
					{
						this.walk_st = -1;
						return -1;
					}
				}
			}
			else if (this.t >= 48f)
			{
				return 1;
			}
			return 0;
		}

		private bool fnDrawShotHand(MeshDrawer Md, EnemyAnimator.EnemyAdditionalDrawFrame Ead)
		{
			PxlFrame pxlFrame;
			if (this.Anm.poseIs("shot1", true))
			{
				if (this.Anm.cframe == 0)
				{
					return true;
				}
				pxlFrame = this.SqShotParts.getFrame(1);
			}
			else
			{
				if (!this.Anm.poseIs("shot2", true))
				{
					if (!this.Anm.poseIs("shot0", true))
					{
						Ead.active = false;
					}
					return true;
				}
				pxlFrame = this.SqShotParts.getFrame((this.Anm.cframe == 0) ? 2 : 1);
			}
			float num;
			if (this.MhGun.isActive(this))
			{
				num = this.gun_agR;
			}
			else
			{
				if (this.walk_st > -1000)
				{
					Ead.active = false;
					return true;
				}
				num = this.walk_time + X.ZSIN(this.t, 22f) * base.mpf_is_right * 65f / 180f * 3.1415927f;
			}
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md = this.AnmMM.PopNextMd(true);
			PxlLayer layerByName = this.Anm.getCurrentDrawnFrame().getLayerByName("point_pivot");
			PxlLayer layer = pxlFrame.getLayer(4);
			PxlLayer layer2 = pxlFrame.getLayer(3);
			PxlLayer layer3 = pxlFrame.getLayer(2);
			Md.TranslateP(-layer.x, layer.y, false);
			float num2;
			if (base.mpf_is_right > 0f)
			{
				num2 = X.angledifR(0f, num) / 1.3194689f;
				Md.Scale(-1f, 1f, false);
			}
			else
			{
				num2 = -X.angledifR(3.1415927f, num) / 1.3194689f;
				num -= 3.1415927f;
			}
			Md.Rotate(num, false);
			Md.TranslateP(layerByName.x, -layerByName.y, false);
			Md.setCurrentMatrix(currentMatrix, true);
			Md.RotaL(0f, 0f, pxlFrame.getLayer(1), false, false, 0);
			for (int i = 5; i < pxlFrame.countLayers(); i++)
			{
				Md.RotaL(0f, 0f, pxlFrame.getLayer(i), false, false, 0);
			}
			MeshDrawer meshDrawer = this.AnmMM.PopNextMd(false);
			meshDrawer.setCurrentMatrix(Md.getCurrentMatrix(), false);
			meshDrawer.initForImg(pxlFrame.getLayer(0).Img, 0);
			float num3 = (-26f + ((num2 > 0f) ? (-16f * num2) : (-14f * num2))) / 180f * 3.1415927f;
			float num4 = ((num2 > 0f) ? (5f * num2) : (7f * num2));
			meshDrawer.Line(layer2.x + num4, -layer2.y, layer2.x + 11f * X.Cos(num3), -layer2.y + 11f * X.Sin(num3), 5f, false, 0f, 0f);
			num3 = (-4f + ((num2 > 0f) ? (-10f * num2) : (5f * num2))) / 180f * 3.1415927f;
			num4 = ((num2 > 0f) ? (1f * num2) : (6f * num2));
			meshDrawer.Line(layer3.x + num4, -layer3.y, layer3.x + 6f * X.Cos(num3), -layer3.y + 6f * X.Sin(num3), 5f, false, 0f, 0f);
			return true;
		}

		public float gun_agR
		{
			get
			{
				return this.MnShot.GetHit(0).agR;
			}
			set
			{
				MagicNotifiear.MnHit hit = this.MnShot.GetHit(0);
				if (hit.agR != value)
				{
					hit.agR = value;
					if (this.EadShotHand.active)
					{
						this.Anm.need_fine_mesh = true;
					}
				}
			}
		}

		private bool MgRunGunShot(MagicItem Mg, float fcnt)
		{
			MagicNotifiear mnShot = this.MnShot;
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.Ray.check_hit_wall = true;
				Mg.phase = 1;
				Mg.Atk0 = this.AtkShotHit;
				Mg.sa = this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y);
				Mg.sz = 3f;
				Mg.dz = 0f;
				Mg.efpos_s = (Mg.raypos_s = (Mg.aimagr_calc_s = true));
				Mg.Ray.RadiusM(0.15f);
				Mg.projectile_power = 1;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.NONE;
				Mg.Ray.check_other_hit = true;
				Mg.Ray.Atk = Mg.Atk0;
				this.gun_agR = -1.5707964f + base.mpf_is_right * 0.3f * 1.5707964f;
				Mg.sz = 0f;
			}
			if (fcnt == 0f)
			{
				return true;
			}
			if (Mg.phase < 10 && (this.state != NelEnemy.STATE.STAND || !this.Nai.isFrontType(NAI.TYPE.MAG, PROG.ACTIVE)))
			{
				return false;
			}
			if (Mg.phase == 1)
			{
				Mg.sz -= fcnt;
				Mg.dz += fcnt;
				if (Mg.sz <= 0f)
				{
					Mg.sz = 3f;
					float num = this.Nai.RANtk(965) * 900f + Mg.t;
					float num2 = X.NIL(0f, 2.5f, X.LENGTHXYS(base.x, base.y, this.Nai.target_x, this.Nai.target_y) - 5f, 7f) * ((this.Nai.isPrGaraakiState() || !this.Nai.isPrAlive()) ? 0.2f : 1f);
					Mg.sa = this.Mp.GAR(base.x, base.y, this.Nai.target_x + (num2 + 0.2f) * X.COSI(num, 97f), this.Nai.target_y + (num2 + 0.5f) * X.COSI(num, 125f));
					Mg.sa = X.LRangleClip(Mg.sa, 1.3194689f, -1);
					this.MnShot._0.CalcReachable(Mg.Ray, new Vector2(Mg.sx, Mg.sy), 0f, null);
				}
			}
			if (Mg.phase == 10)
			{
				Mg.phase = 11;
				Mg.t = 0f;
				Mg.PtcST("mgolem_gunshot_shoot", PTCThread.StFollow.FOLLOW_S, false);
				Mg.Ray.LenM(0.35f);
				Mg.Ray.check_other_hit = true;
				Mg.sz = 0f;
			}
			if (Mg.phase == 11)
			{
				if (Mg.t >= 90f)
				{
					return false;
				}
				if (Mg.Ray.reflected)
				{
					Mg.reflectAgR(Mg.Ray, ref Mg.sa, 0.25f);
					this.gun_agR = Mg.sa;
				}
				Mg.MnSetRay(Mg.Ray, 0, Mg.sa, 0f);
				HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(0.2f * (float)X.MPF(Mg.Ray.difmapx > 0f), 0f), HITTYPE.NONE);
				if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
				{
					return Mg.kill(Mg.Ray.lenmp);
				}
				if ((hittype & HITTYPE.WALL_AND_BREAK) != HITTYPE.NONE)
				{
					Mg.PtcST("mgolem_gunshot_hit", PTCThread.StFollow.NO_FOLLOW, false);
					return false;
				}
				mnShot.RayShift(Mg.Ray, 0, ref Mg.sx, ref Mg.sy, fcnt);
				Mg.sz -= fcnt;
				if (Mg.sz <= 0f)
				{
					Mg.sz = 3f;
					Mg.PtcST("mgolem_gunshot_fly", PTCThread.StFollow.NO_FOLLOW, false);
				}
			}
			return true;
		}

		private bool MgDrawGunShot(MagicItem Mg, float fcnt)
		{
			MagicNotifiear mnShot = this.MnShot;
			if (Mg.phase <= 1)
			{
				MeshDrawer meshDrawer = Mg.Ef.GetMesh("", MTRX.MtrMeshNormal, true);
				mnShot.drawTo(meshDrawer, this.Mp, new Vector2(Mg.sx, Mg.sy), 0f, 0f, false, Mg.t * 2f, null, null);
			}
			if (Mg.phase == 11)
			{
				MeshDrawer meshDrawer = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
				MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, true);
				float num = 1.25f + 0.2f * X.COSI(Mg.t, 8.3f) + 0.05f * X.COSI(Mg.t, 7.11f);
				if (Mg.t < 24f)
				{
					num *= 1f + 2.2f * (1f - X.ZSIN2(Mg.t, 24f));
				}
				float num2 = (num - 1f) * 2f + 1f;
				meshImg.Scale(num2, num2 * 0.3f, false).Rotate(Mg.sa, false);
				meshImg.Col = meshImg.ColGrd.Set(4288526009U).blend(4279650404U, 0.5f + 0.5f * X.COSI(Mg.t, 12.35f)).C;
				meshImg.initForImg(MTRX.EffBlurCircle245, 0).Rect(-50f, 0f, 120f, 120f, false);
				meshDrawer.Scale(num2, num2 * 0.3f, false).Rotate(Mg.sa, false);
				meshDrawer.Col = meshDrawer.ColGrd.Set(4294578021U).blend(4294945433U, 0.5f + 0.5f * X.COSI(Mg.t, 10.68f)).C;
				meshDrawer.initForImg(MTRX.EffBlurCircle245, 0).Rect(-30f, 0f, 100f, 100f, false);
				meshDrawer.initForImg(MTRX.EffCircle128, 0).Rect(-18f, 0f, 55f, 40f, false);
			}
			return true;
		}

		private bool runGunRecharge(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.SpSetPose("recharge", -1, null, false);
				this.t = 0f;
				this.walk_st = 0;
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
			}
			if (this.t >= 110f && this.walk_st == 0)
			{
				this.walk_st = 1;
				this.ammo = 3;
				base.MpConsume(this.McsRecharge, null, 1f, 1f);
			}
			if (this.t >= 155f)
			{
				this.Nai.addTypeLock(NAI.TYPE.MAG_0, 300f);
				return false;
			}
			return true;
		}

		private bool od_grab_attackable
		{
			get
			{
				return this.Nai.isAttackableLength(2f, -4f, 2f, false);
			}
		}

		private bool od_grab_attackable_wide
		{
			get
			{
				return this.Nai.isAttackableLength(4f, -4f, 2f, false);
			}
		}

		private bool considerOverDrive(NAI Nai)
		{
			bool flag = Nai.hasPT(1, false);
			bool flag2 = Nai.isPrTortured();
			if (Nai.HasF(NAI.FLAG.OVERDRIVED, true))
			{
				if (!this.normal_too_near_target())
				{
					return Nai.AddTicketB(NAI.TYPE.PUNCH, 128, true);
				}
				if (Nai.tkC(314, 0.5f, true))
				{
					return Nai.AddTicketB(NAI.TYPE.PUNCH_0, 128, true);
				}
				Nai.AddTicketB(NAI.TYPE.WAIT, 128, true);
				return true;
			}
			else
			{
				if (!Nai.HasF(NAI.FLAG.WANDERING, false) && Nai.tkC(314, 0.85f, true) && !flag2)
				{
					if (!this.normal_too_near_target())
					{
						return Nai.AddTicketB(NAI.TYPE.PUNCH, 128, true);
					}
					if (Nai.tkC(314, 0.5f, true))
					{
						return Nai.AddTicketB(NAI.TYPE.PUNCH_0, 128, true);
					}
				}
				if (!flag2 && Nai.tkC(4178, 0.88f, true))
				{
					Nai.AddTicketB(NAI.TYPE.WAIT, 1, true);
					return true;
				}
				bool flag3 = Nai.HasF(NAI.FLAG.ATTACKED, false) && (Nai.isPrAlive() || Nai.tkC(5025, 0.7f, true));
				if (!flag3 && Nai.isPrGaraakiState() && !Nai.isPrTortured() && this.od_grab_attackable_wide && !Nai.hasTypeLock(NAI.TYPE.GUARD_2))
				{
					return Nai.AddTicketB(NAI.TYPE.GUARD_2, 128, true);
				}
				if (!flag)
				{
					if (this.shield_useable && Nai.canNoticeThat(1f))
					{
						return Nai.AddTicketB(NAI.TYPE.GUARD, 128, true);
					}
					if (((Nai.tkC(4881, 0.15f, true) && this.od_grab_attackable_wide) || (Nai.tkC(4881, 0.3f, true) && this.od_grab_attackable)) && !flag3 && !Nai.hasTypeLock(NAI.TYPE.GUARD_1))
					{
						return Nai.AddTicketB(NAI.TYPE.GUARD_1, 128, true);
					}
				}
				if (flag3)
				{
					Nai.delay = 2f;
					return false;
				}
				if (Nai.tkC(2345, 0.3f, true) && !Nai.hasTypeLock(NAI.TYPE.MAG_0) && base.Useable(this.McsOdMissile, 1f, 0f))
				{
					return Nai.AddTicketB(NAI.TYPE.MAG_0, 128, true);
				}
				if (this.od_wait_continue < 3 || !Nai.tkC(3555, 0.88f, true))
				{
					if (!flag)
					{
						if (this.normal_too_near_target())
						{
							if (Nai.tkC(524, 0.2f, true))
							{
								return Nai.AddTicketB(NAI.TYPE.PUNCH_0, 128, true);
							}
						}
						else if (Nai.tkC(524, 0.35f, true))
						{
							return Nai.AddTicketB(NAI.TYPE.PUNCH, 128, true);
						}
						Nai.AddTicketB(NAI.TYPE.WAIT, 1, true);
					}
					Nai.delay = X.NIXP(10f, 40f);
					return true;
				}
				if (this.od_grab_attackable_wide)
				{
					return Nai.AddTicketB(NAI.TYPE.GUARD_1, 128, true);
				}
				if (base.Useable(this.McsOdArrow, 1f, 0f) && (this.od_wait_continue >= 6 || this.Mp.canThroughBcc(base.x, base.y, Nai.target_x, Nai.target_y, 0.1f, 0.1f, -1, false, false, null, true, null)))
				{
					return Nai.AddTicketB(NAI.TYPE.GUARD, 128, true);
				}
				return Nai.AddTicketB(NAI.TYPE.PUNCH_0, 128, true);
			}
		}

		private bool readTicketOd(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type - NAI.TYPE.PUNCH > 1)
			{
				switch (type)
				{
				case NAI.TYPE.MAG_0:
					return this.runOdMissile(Tk.initProgress(this), Tk);
				case NAI.TYPE.MAG_1:
				case NAI.TYPE.MAG_2:
				case NAI.TYPE.GUARD_0:
					break;
				case NAI.TYPE.GUARD:
					return this.runOdArrowShield(Tk.initProgress(this), Tk);
				case NAI.TYPE.GUARD_1:
				case NAI.TYPE.GUARD_2:
					return this.runOdGrab(Tk.initProgress(this), Tk, false);
				default:
					if (type == NAI.TYPE.WAIT)
					{
						if (Tk.initProgress(this))
						{
							base.AimToPlayer();
							this.t = 300f - (20f + this.Nai.RANtk(840) * 30f);
						}
						if (this.shield_useable && this.Nai.canNoticeThat(0.4f))
						{
							Tk.Recreate(NAI.TYPE.GUARD, 128, true, null);
							return true;
						}
						return this.t < 300f;
					}
					break;
				}
				return base.readTicket(Tk);
			}
			return this.runOdRotate(Tk.initProgress(this), Tk, Tk.type == NAI.TYPE.PUNCH);
		}

		private bool runOdMissile(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				base.AimToPlayer();
				this.walk_st = 0;
				this.SpSetPose("od_missile0", -1, null, false);
				base.PtcST("mgolem_od_mispod_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
				this.Nai.RemF(NAI.FLAG.ATTACKED);
				this.Danger.enabled = false;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (!this.Anm.poseIs("od_missile0", true))
				{
					this.SpSetPose("od_missile0", -1, null, false);
				}
				PxlSequence currentSequence = this.Anm.getCurrentSequence();
				if (this.Anm.cframe >= currentSequence.loop_to)
				{
					if (this.walk_st == 0)
					{
						this.walk_st = 1;
						this.t = 0f;
						if (this.FD_MgRunOdMisisle == null)
						{
							this.FD_MgRunOdMisisle = new MagicItem.FnMagicRun(this.MgRunOdMisisle);
							this.FD_MgDrawOdMisisle = new MagicItem.FnMagicRun(this.MgDrawOdMisisle);
						}
						if (NelNMechGolem.Amis_layer_name == null)
						{
							NelNMechGolem.Amis_layer_name = new string[32];
							for (int i = 31; i >= 0; i--)
							{
								NelNMechGolem.Amis_layer_name[i] = "missile_" + i.ToString();
							}
						}
						int num = 32;
						for (int j = 0; j < num; j++)
						{
							MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunOdMisisle, this.FD_MgDrawOdMisisle);
							magicItem.phase = 1000 + j;
							magicItem.sz = -1f;
							magicItem.dz = base.scaleX;
							magicItem.efpos_s = true;
							magicItem.Atk0 = this.AtkOdMissile;
						}
					}
					if (Tk.Progress(ref this.t, 75, true))
					{
						this.t = 100f;
						this.walk_st = 0;
						M2BlockColliderContainer.BCCLine sideBcc = this.Mp.getSideBcc((int)this.Nai.target_x, (int)this.Nai.target_y, AIM.T);
						if (this.Nai.target_y < base.mtop - 2f)
						{
							this.walk_time = 1.5707964f;
						}
						else
						{
							this.walk_time = ((base.mpf_is_right > 0f != base.x < this.Nai.target_x) ? (1.5707964f - base.mpf_is_right * 1.5707964f) : this.Mp.GAR(base.x, base.y, this.Nai.target_x, (sideBcc == null) ? (this.Nai.target_y - 2f) : X.NI(this.Nai.target_y, sideBcc.slopeBottomY(this.Nai.target_x), 0.5f)));
							this.walk_time = ((X.Cos(this.walk_time) >= 0f != base.mpf_is_right >= 0f) ? 1.5707964f : X.LRangleClip(this.walk_time, 1.5707964f, -1));
							if (this.walk_time < 0f && this.Nai.target_y < base.mtop)
							{
								this.walk_time = ((base.mpf_is_right >= 0f) ? 0f : 3.1415927f);
							}
						}
						this.SpSetPose("od_missile1", -1, null, false);
					}
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_st < 32)
				{
					if (this.t >= 100f)
					{
						this.t = 95f;
						this.walk_st++;
						this.Anm.animReset(X.xors(X.Mn(5, this.walk_st)), false);
					}
				}
				else
				{
					if (!this.Nai.HasF(NAI.FLAG.ATTACKED, false))
					{
						this.Nai.AddF(NAI.FLAG.ATTACKED, 200f);
						base.MpConsume(this.McsOdMissile, null, 1f, 0f);
					}
					if (Tk.Progress(ref this.t, 260, true))
					{
						this.SpSetPose("od_missile2", -1, null, false);
					}
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, 100, true))
			{
				this.Nai.RemF(NAI.FLAG.ATTACKED);
				this.Nai.AddF(NAI.FLAG.ATTACKED, 50f);
				return false;
			}
			return true;
		}

		private bool MgRunOdMisisle(MagicItem Mg, float fcnt)
		{
			if (Mg.phase >= 1000)
			{
				bool flag = Mg.sz < 0f;
				bool flag2 = false;
				int num = Mg.phase - 1000;
				if (base.destructed || this.state != NelEnemy.STATE.STAND || !this.Nai.isFrontType(NAI.TYPE.MAG_0, PROG.ACTIVE))
				{
					return false;
				}
				if (this.Nai.isFrontType(NAI.TYPE.MAG_0, PROG.PROG0))
				{
					int num2 = num % 16 * 2 + ((num >= 16) ? 1 : 0);
					flag2 = this.walk_st > num2;
				}
				if (Mg.sz != (float)this.Anm.cframe || flag2)
				{
					Mg.sz = (float)this.Anm.cframe;
					PxlLayer layerByName = this.Anm.getCurrentDrawnFrame().getLayerByName(NelNMechGolem.Amis_layer_name[num]);
					Vector2 mapPosForLayer = this.Anm.getMapPosForLayer(layerByName);
					Mg.dx = mapPosForLayer.x;
					Mg.dy = mapPosForLayer.y;
				}
				if (flag || flag2)
				{
					Mg.sx = Mg.dx;
					Mg.sy = Mg.dy;
					Mg.efpos_s = true;
				}
				else
				{
					Mg.sx = X.VALWALK(Mg.sx, Mg.dx, 0.04f * fcnt);
					Mg.sy = X.VALWALK(Mg.sy, Mg.dy, 0.04f * fcnt);
				}
				if (!flag2)
				{
					return true;
				}
				int num3 = num % 16;
				Mg.da = X.NIXP(20f, 60f);
				Mg.dx = (Mg.dy = 0f);
				Mg.sz = 1f;
				Mg.phase = 0;
				Mg.sa = this.walk_time + (-0.5f + (float)num3 / 16f * 1.65f) * 20f / 180f * 3.1415927f * (float)X.MPF(base.mpf_is_right < 0f);
				Mg.aim_agR = Mg.sa;
				Mg.PtcST("mgolem_od_mispod_shotone", PTCThread.StFollow.NO_FOLLOW, false);
				NelNGolemToyMisPod.MgMissileInit(Mg, (num3 % 4 < 2) ? 1 : 0);
				Mg.MpConsume(this.McsOdMissile, 0.03125f);
			}
			return NelNGolemToyMisPod.MgRun(Mg, fcnt, 2, 25f);
		}

		private bool MgDrawOdMisisle(MagicItem Mg, float fcnt)
		{
			if (Mg.phase < 1000)
			{
				return NelNGolemToyMisPod.MgDraw(Mg, fcnt, this.SqOdMissile, this.Anm.getMI());
			}
			int num = Mg.phase - 1000;
			float num2 = (float)((num % 16 * 2 + ((num >= 16) ? 1 : 0)) * 2);
			MeshDrawer mesh = Mg.Ef.GetMesh("", MTRX.MtrMeshAdd, false);
			float num3 = Mg.t - num2;
			if (num3 <= 0f)
			{
				return true;
			}
			float num4 = (0.6f + 0.4f * X.ZSIN(num3, 35f)) * 80f * (0.9f + 0.1f * X.COSI(num3, 13.7f));
			mesh.Col = mesh.ColGrd.Set(4293464148U).blend(4283891739U, 0.5f + 0.375f * X.COSI(num3, 11.4f) + 0.125f * X.COSI(num3, 7.3f)).mulA(X.ZPOW(num3, 35f))
				.C;
			MTRX.Halo.Set(num4 * 0.15f, num4, 0.7f, 0.3f, 1f, 2f);
			MTRX.Halo.Dent(0.7f);
			MTRX.Halo.drawTo(mesh, 0f, 0f, 0.7853982f, false, 3);
			return true;
		}

		private void setOdEyeBlinkEd()
		{
			this.Mp.setED("mgolem_rotate_eye_0", this.FD_eye0, 0f);
			this.Mp.setED("mgolem_rotate_eye_1", this.FD_eye1, 0f);
			this.Mp.setED("mgolem_rotate_eye_2", this.FD_eye2, 0f);
		}

		public bool runOdRotate(bool init_flag, NaTicket Tk, bool is_closer = false)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = (is_closer ? (-2) : (3 + X.xors(3)));
				this.walk_time = 0f;
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.SpSetPose("od_role_prepare", -1, null, false);
				this.setOdEyeBlinkEd();
				base.playSndPos("mgolem_od_rotate0", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C, null);
				this.Danger.enabled = false;
			}
			if (Tk.prog <= PROG.PROG1)
			{
				NelNMechGolem.floort_danger_ground = this.Mp.floort;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 130, base.hasFoot()))
			{
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
				base.Size((float)this.Od.od_size_pixel_y, (float)this.Od.od_size_pixel_y, ALIGN.CENTER, ALIGNY.MIDDLE, false);
				base.PtcST("mgolem_rotate_ground_spark", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				Tk.prog = (base.hasFoot() ? PROG.PROG1 : PROG.PROG0);
				this.MhJumpWire = new MagicItemHandlerS(base.tackleInit(this.AtkTackleBodyOd, 0f, 0f, this.sizey + 0.4f, true, false));
				this.MhJumpWire.Mg.projectile_power = -50;
				this.MhJumpWire.Mg.phase = 5;
				this.MhJumpWire.Mg.Ray.HitLock(70f, null);
				this.MhJumpWire.Mg.da = 60f;
				this.SpSetPose("od_roling", -1, null, false);
				if (this.SndLoopWalk == null)
				{
					this.SndLoopWalk = this.Mp.M2D.Snd.createInterval(this.snd_key, "mgolem_od_rotate1", 182f, this, 0f, 128);
				}
			}
			if (Tk.prog == PROG.PROG0 || Tk.prog == PROG.PROG1)
			{
				if (!this.MhJumpWire.isActive(this))
				{
					Tk.prog = PROG.PROG5;
					this.walk_st = 0;
					this.Phy.addFoc(FOCTYPE.KNOCKBACK | FOCTYPE._CLIFF_STOPPER, -base.mpf_is_right * 0.4f, 0f, -1f, 0, 4, 60, 30, 0);
				}
				else
				{
					if (base.wallHitted(this.aim))
					{
						this.OdWallHitBump();
						if (this.walk_st < 0)
						{
							this.walk_st--;
							M2BlockColliderContainer.BCCLine lastBCC = this.FootD.get_LastBCC();
							if ((this.normal_too_near_target() && (lastBCC == null || lastBCC.isLinearWalkableTo(this.Nai.TargetLastBcc, 6) != 0)) || this.Nai.RANtk(3151) < X.ZLINE((float)(-(float)this.walk_st - 3), 4f) + X.ZLINE(this.t - 340f, 700f))
							{
								Tk.prog = PROG.PROG5;
								this.walk_st = 0;
							}
						}
						else
						{
							int num = this.walk_st - 1;
							this.walk_st = num;
							if (num <= 0 || this.Nai.RANtk(3151) < X.ZLINE(this.t - 180f, 480f))
							{
								Tk.prog = PROG.PROG5;
								this.walk_st = 0;
							}
						}
						if (Tk.prog <= PROG.PROG1)
						{
							this.setAim((base.mpf_is_right > 0f) ? AIM.L : AIM.R, false);
							this.PtcHld.killPtc("mgolem_rotate_ground_spark", false);
							if (Tk.prog == PROG.PROG1)
							{
								base.PtcST("mgolem_rotate_ground_spark", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
							}
							this.AtkTackleBodyOd.BurstDir(base.mpf_is_right > 0f);
							this.FootD.initJump(false, false, false);
							this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._RELEASE, base.mpf_is_right * 0.1f, -0.2f, -1f, 0, 20, 0, 1, 0);
						}
						else
						{
							this.Phy.addFoc(FOCTYPE.KNOCKBACK | FOCTYPE._CLIFF_STOPPER, -base.mpf_is_right * 0.3f, 0f, -1f, 0, 4, 60, 20, 0);
						}
					}
					if (Tk.prog <= PROG.PROG1)
					{
						this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, base.mpf_is_right * 0.15f, 0f, -1f, -1, 1, 0, -1, 0);
					}
				}
				if (Tk.prog == PROG.PROG1)
				{
					if (!base.hasFoot())
					{
						Tk.prog = PROG.PROG0;
						this.PtcHld.killPtc("mgolem_rotate_ground_spark", false);
					}
					else
					{
						if (this.walk_time <= 0f)
						{
							this.walk_time = X.NIXP(10f, 30f);
							this.Mp.DropCon.setGroundBreaker(base.x, base.mbottom + 0.15f, 1f, 1f, M2DropObjectReader.Get("snake_ground_break_out", false));
						}
						this.walk_time -= this.TS;
						if (this.walk_st < 0)
						{
							this.MhJumpWire.Mg.da -= this.TS;
							M2BlockColliderContainer.BCCLine lastBCC2 = this.FootD.get_LastBCC();
							if (this.MhJumpWire.Mg.da <= 0f)
							{
								if (X.XORSP() < (this.od_grab_attackable_wide ? 0.7f : 0.2f) + 0.125f * (float)(-(float)this.walk_st - 1) && this.normal_too_near_target() && (lastBCC2 == null || lastBCC2.isLinearWalkableTo(this.Nai.TargetLastBcc, 6) != 0))
								{
									this.Phy.addFoc(FOCTYPE.KNOCKBACK | FOCTYPE._CLIFF_STOPPER, base.mpf_is_right * 0.15f * 0.75f, 0f, -1f, 0, 2, 50, 20, 0);
									Tk.prog = PROG.PROG5;
									this.walk_st = 0;
								}
								else
								{
									this.MhJumpWire.Mg.da = X.NIXP(20f, 50f);
								}
							}
						}
					}
				}
				else if (Tk.prog == PROG.PROG0 && base.hasFoot())
				{
					Tk.prog = PROG.PROG1;
					this.walk_time = 0f;
					this.MhJumpWire.Mg.da = 0f;
					base.PtcVar("sy", (double)base.mbottom).PtcST("mgolem_rotate_ground_land_bump", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					base.PtcST("mgolem_rotate_ground_spark", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				}
			}
			if (Tk.prog == PROG.PROG5)
			{
				if (this.walk_st == 0)
				{
					this.can_hold_tackle = false;
					this.t = 0f;
					this.walk_st++;
					this.SpSetPose("od_role2", -1, null, false);
					this.PtcHld.killPtc("mgolem_rotate_ground_spark", false);
					if (this.SndLoopWalk != null)
					{
						this.SndLoopWalk.destruct();
						this.SndLoopWalk = null;
					}
					base.remF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
					base.addF(NelEnemy.FLAG.CHECK_ENLARGE);
				}
				else if (this.t >= 80f)
				{
					this.Nai.RemF(NAI.FLAG.ATTACKED);
					this.Nai.AddF(NAI.FLAG.ATTACKED, 90f);
					return false;
				}
			}
			return true;
		}

		private void OdWallHitBump()
		{
			base.PtcVar("sx", (double)(base.x + base.mpf_is_right * this.sizex)).PtcST("mgolem_rotate_ground_land_wall", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			this.AtkCeilDrop.Caster = this;
			MgNCeilDrop.setCeilDrop(this.Mp, this.AtkCeilDrop, base.mg_hit, this.Nai.TargetLastBcc, base.x, base.y, 35f, 14, 80f, 180f, 0.25f);
		}

		public bool drawBinderRotateEye(M2DrawBinder Ed, EffectItem Ef, string lname)
		{
			if (base.destructed)
			{
				return false;
			}
			float num;
			if (this.isFrontRotate())
			{
				NaTicket naTicket = this.Nai.getCurTicket();
				if (naTicket == null || naTicket.prog > PROG.ACTIVE)
				{
					return false;
				}
				num = 45f;
			}
			else
			{
				if (!this.Nai.isFrontType(NAI.TYPE.GUARD_1, PROG.ACTIVE) && !this.Nai.isFrontType(NAI.TYPE.GUARD_2, PROG.ACTIVE))
				{
					return false;
				}
				NaTicket naTicket = this.Nai.getCurTicket();
				if (naTicket == null || naTicket.prog > PROG.PROG0)
				{
					return false;
				}
				num = 70f;
			}
			PxlLayer layerByName = this.Anm.getCurrentDrawnFrame().getLayerByName(lname);
			if (layerByName == null)
			{
				return true;
			}
			Vector2 mapPosForLayer = this.Anm.getMapPosForLayer(layerByName);
			Ef.x = mapPosForLayer.x;
			Ef.y = mapPosForLayer.y;
			if (!Ed.isinCamera(Ef, 4f, 4f))
			{
				return true;
			}
			MeshDrawer mesh = Ef.GetMesh("", MTRX.MtrMeshAdd, false);
			this.EfPtcOnceRotateEye.drawTo(mesh, mesh.base_px_x, mesh.base_px_y, num, 60, Ed.t, 0f);
			return true;
		}

		public bool shield_useable
		{
			get
			{
				return (!this.Nai.isPrGaraakiState() || this.Nai.tkC(3899, 0.25f, true)) && ((this.Nai.isPrShotGunEnable(1f, false) && this.od_grab_attackable_wide) || this.Danger.isDanger());
			}
		}

		public bool runOdArrowShield(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.Danger.enabled = true;
				this.t = 0f;
				base.PtcST("mgolem_od_shield0", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				this.SpSetPose("od_shield0", -1, null, false);
				this.Nai.RemF(NAI.FLAG.ATTACKED);
				base.AimToPlayer();
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.t >= 12f)
				{
					this.Nai.AddF(NAI.FLAG.INJECTED, 3f);
				}
				if (Tk.Progress(ref this.t, 80, true))
				{
					this.t = 300f - X.NIXP(30f, 80f);
					this.walk_st = 1;
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.Nai.AddF(NAI.FLAG.INJECTED, 3f);
				if (this.t >= 300f)
				{
					bool flag = this.Nai.hasTypeLock(NAI.TYPE.GUARD) || !base.Useable(this.McsOdArrow, 1f, 0f);
					bool flag2 = !this.Nai.is_aiming_target || flag;
					if (!this.Nai.isPrGaraakiState() && this.od_grab_attackable_wide && X.XORSP() < (flag2 ? 0.55f : 0.2f) + (float)(this.walk_st - 1) * 0.05f)
					{
						base.killPtc("mgolem_od_shield0", false);
						Tk.Recreate(NAI.TYPE.GUARD_2, 128, true, null);
						return this.runOdGrab(true, Tk, true);
					}
					if (X.XORSP() < ((!this.shield_useable) ? 0.7f : 0f) + 0.097f * (float)this.walk_st)
					{
						base.killPtc("mgolem_od_shield0", false);
						if (flag && this.od_grab_attackable_wide)
						{
							Tk.Recreate(NAI.TYPE.GUARD_2, 128, true, null);
							return this.runOdGrab(true, Tk, true);
						}
						if (flag2)
						{
							this.SpSetPose("od_shield3", -1, null, false);
							Tk.AfterDelay(20f);
							this.Nai.AddF(NAI.FLAG.ATTACKED, X.NIXP(45f, 60f));
							this.Nai.AddF(NAI.FLAG.BOTHERED, 50f);
							return false;
						}
						this.Nai.AddF(NAI.FLAG.INJECTED, 75f);
						Tk.prog = PROG.PROG1;
						this.t = 0f;
						this.walk_st = 0;
						this.SpSetPose("od_shield1", -1, null, false);
						this.prepareLaser(false);
						base.playSndPos("mgolem_shield1", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C, null);
					}
					else
					{
						this.walk_st++;
						this.t = 300f - X.NIXP(10f, 40f);
					}
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, 75, true))
			{
				this.walk_st = 0;
				this.t = 100f;
				this.SpSetPose("od_shield2", -1, null, false);
			}
			if (Tk.prog == PROG.PROG2)
			{
				if (this.walk_st < 20)
				{
					if (this.t >= 100f)
					{
						this.walk_st++;
						this.t = 96f;
						base.nM2D.MGC.FindMg(NelNMechGolem.FD_ProgressOdArrowForWalkSt, this);
						if (this.walk_st >= 8)
						{
							this.walk_st = 20;
							this.t = 0f;
						}
					}
				}
				else if (Tk.Progress(ref this.t, 160, true))
				{
					base.nM2D.MGC.FindMg(NelNMechGolem.FD_QuitOdArrow, this);
					this.walk_st = 0;
					base.MpConsume(this.McsOdArrow, null, 1f, 1f);
				}
			}
			if (Tk.prog == PROG.PROG3 && this.t >= 80f)
			{
				this.od_arrow_attr = (byte)(((int)(this.od_arrow_attr + 1) + X.xors(4)) % 4);
				this.SpSetPose("od_shield3", -1, null, false);
				Tk.AfterDelay(X.NIXP(45f, 60f));
				if (X.XORSP() < 0.5f)
				{
					this.Nai.addTypeLock(NAI.TYPE.GUARD, 320f);
				}
				base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				return false;
			}
			return true;
		}

		public bool runOdGrab(bool init_flag, NaTicket Tk, bool fast_attack = false)
		{
			if (init_flag)
			{
				if (this.Nai.isPrTortured() && this.Nai.tkC(6335, 0.75f, true))
				{
					this.Nai.addTypeLock(NAI.TYPE.GUARD_1, 120f);
					this.Nai.addTypeLock(NAI.TYPE.GUARD_2, 120f);
					Tk.after_delay = X.NIXP(30f, 70f);
					return false;
				}
				this.Danger.enabled = false;
				this.walk_st = 1 + (((float)this.hp < (float)this.maxhp * 0.5f && this.Nai.tkC(8551, 0.78f, true)) ? 1 : 0);
				this.walk_time = X.NIXP(35f, 65f);
				if (fast_attack || (base.hp_ratio < 0.4f && this.Nai.tkC(4419, 0.5f, true)))
				{
					this.walk_time = 15f;
				}
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				base.AimToPlayer();
				Tk.prog = PROG.PROG0;
				this.t = 0f;
				this.walk_st--;
				this.SpSetPose("od_atk0", -1, null, false);
				this.setOdEyeBlinkEd();
				base.PtcST("mgolem_od_grab0", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.Anm.timescale = 1f;
				this.Anm.animReset(0, false);
			}
			if (Tk.prog == PROG.PROG0 && this.t >= this.walk_time)
			{
				this.Anm.timescale = 0f;
				if (Tk.Progress(ref this.t, (int)this.walk_time + 25, true))
				{
					this.Anm.timescale = 1f;
					this.SpSetPose("od_atk1", -1, null, false);
					base.PtcST("mgolem_od_grab1", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CLIFF_STOPPER | FOCTYPE._CHECK_WALL, base.mpf_is_right * ((!this.od_grab_attackable || this.Nai.tkC(8891, 0.4f, true)) ? 0.4f : 0.15f), 0f, -1f, 0, 1, 18, -1, 0);
				}
			}
			if (Tk.prog == PROG.PROG1 && Tk.Progress(ref this.t, 10, true))
			{
				base.PtcST("mgolem_od_grab2", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				base.tackleInit(this.AtkOdGrab, this.TkiOdGrab);
			}
			if (Tk.prog == PROG.PROG2)
			{
				if (this.t >= 20f)
				{
					this.can_hold_tackle = false;
				}
				if (this.t >= 60f)
				{
					if (this.walk_st > 0 && (this.od_grab_attackable || (this.od_grab_attackable_wide && this.Nai.tkC(955, 0.6f, true))))
					{
						Tk.prog = PROG.ACTIVE;
						this.walk_time = 35f;
						return true;
					}
					Tk.prog = PROG.PROG5;
				}
			}
			if (Tk.prog == PROG.PROG5)
			{
				base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
				this.SpSetPose("od_atk2", -1, null, false);
				base.PtcST("mgolem_od_grab_end", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
				Tk.AfterDelay(60f);
				if (this.Nai.tkC(1678, 0.75f, true))
				{
					this.Nai.addTypeLock(NAI.TYPE.GUARD_1, 190f);
				}
				if (Tk.type == NAI.TYPE.GUARD_2 && this.Nai.tkC(6515, 0.35f, true))
				{
					this.Nai.addTypeLock(NAI.TYPE.GUARD_2, 250f);
				}
				this.Nai.RemF(NAI.FLAG.ATTACKED);
				this.Nai.AddF(NAI.FLAG.ATTACKED, 40f);
				return false;
			}
			return true;
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
			Abm.get_Gacha().activate(PrGachaItem.TYPE.REP, 12, 63U);
			this.walk_st = -2000;
			return true;
		}

		public override bool runAbsorb()
		{
			if (!base.isOverDrive())
			{
				return false;
			}
			this.Nai.fineTargetPosition(this.TS);
			if (this.Absorb.target_pose == "torture_mechgolem0" || this.Absorb.target_pose == "torture_mechgolem0r" || this.walk_st == -2000)
			{
				PR pr = base.AimPr as PR;
				if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f))
				{
					return false;
				}
				if (this.walk_st == -2000)
				{
					float t = this.t;
					this.Absorb.changeTorturePose((this.t < 30f) ? "torture_mechgolem0" : "torture_mechgolem0r", true, true, -1, -1);
					this.t = 200f - X.NIXP(70f, 115f) * X.NIL(0.4f, 1f, base.hp_ratio + 0.2f, 1f);
					this.walk_st = (2 + X.xors(7)) * ((X.XORSP() < ((this.t >= 30f) ? 1f : (this.is_alive ? 0.25f : 0.06f))) ? 3 : 1);
					this.walk_st = X.Mx(5, this.walk_st);
					this.Absorb.setKirimomiReleaseDir((int)this.aim);
					this.walk_time = 0f;
				}
				if (this.t >= 200f)
				{
					int num = this.walk_st - 1;
					this.walk_st = num;
					if (num >= 0)
					{
						this.t = 200f - X.NIXP(30f, 50f);
						pr.PtcST("mgolem_absorb_squeeze", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Mp.DropCon.setBlood(pr, 15, MTR.col_blood, 0.2f, true);
						base.applyAbsorbDamageTo(pr, this.AtkAbsorbMain, true, false, true, 0f, true, null, false);
						this.Anm.animReset(this.Anm.cframe + X.xors(3), true);
					}
					else
					{
						this.Absorb.changeTorturePose("torture_mechgolem1", true, true, -1, -1);
						this.walk_st = 0;
						this.t = 0f;
						this.walk_time = 0f;
					}
				}
			}
			else
			{
				if (this.walk_time > 0f)
				{
					this.walk_time = 0f;
					this.Absorb.changeTorturePose((this.Anm.poseIs("od_torture_mechgolem2d2", true) == X.XORSP() < 0.3f) ? "torture_mechgolem2d" : "torture_mechgolem2d2", true, true, -1, -1);
					this.Anm.animReset(X.xors(4), false);
				}
				if ((this.Anm.cframe >= 6 && this.Anm.poseIs("od_torture_mechgolem2d", true)) || this.Anm.poseIs("od_torture_mechgolem2d2", false))
				{
					PR pr2 = base.AimPr as PR;
					if (pr2 != null && this.Absorb.isActive(pr2, this, true) && !pr2.isUnconscious())
					{
						this.Absorb.changeTorturePose("torture_mechgolem2", true, true, -1, -1);
						this.Anm.animReset(X.xors(4), false);
					}
				}
				if (this.walk_st == 0)
				{
					this.t = 0f;
					this.walk_st = 1;
					base.playSndPos("mgolem_laser_prepare", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C, null);
				}
				if (this.walk_st == 1 && this.t >= 40f)
				{
					this.walk_st = 2;
					this.t = 0f;
					this.prepareLaser(true);
					base.playSndPos("mgolem_shield1", base.x, base.y, PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C, null);
				}
				if (this.walk_st == 2 && this.t >= 75f)
				{
					this.walk_st = 10;
					this.t = 100f;
				}
				if (10 <= this.walk_st && this.walk_st < 20 && this.t >= 100f)
				{
					this.walk_st++;
					this.t = 92f;
					base.nM2D.MGC.FindMg(NelNMechGolem.FD_ProgressOdArrowForWalkSt, this);
					if (this.walk_st >= 14)
					{
						this.walk_st = 20;
						this.t = 0f;
					}
				}
				if (this.walk_st == 20 && this.t >= 160f)
				{
					this.walk_st = 21;
					PR pr3 = base.AimPr as PR;
					float num2 = X.NIXP(1f, 1.33f);
					if (pr3 == null || !this.Absorb.isActive(pr3, this, true) || !this.Absorb.checkTargetAndLength(pr3, 3f))
					{
						num2 *= 0.33f;
					}
					this.t = 300f - num2 * 80f;
					base.nM2D.MGC.FindMg(NelNMechGolem.FD_QuitOdArrow, this);
				}
				if (this.walk_st >= 21 && this.t >= 300f)
				{
					PR pr4 = base.AimPr as PR;
					if (pr4 == null || !this.Absorb.isActive(pr4, this, true) || !this.Absorb.checkTargetAndLength(pr4, 3f))
					{
						return false;
					}
					if (X.XORSP() < ((!this.Nai.isPrAlive()) ? 0.08f : 0.2f))
					{
						this.walk_st = -2000;
					}
					else
					{
						if ((!this.Nai.isPrAlive() && X.XORSP() < 0.3f) || (this.od_arrow_attr_grab != 3 && this.Nai.tkC(815, 0.75f, true)))
						{
							this.od_arrow_attr_grab = 3;
						}
						else
						{
							this.od_arrow_attr_grab = (byte)(((int)(this.od_arrow_attr_grab + 1) + X.xors(4)) % 4);
						}
						this.walk_st = 0;
					}
				}
			}
			return true;
		}

		private void prepareLaser(bool is_absorb)
		{
			if (this.FD_MgRunOdArrow == null)
			{
				this.FD_MgRunOdArrow = new MagicItem.FnMagicRun(this.MgRunOdArrow);
				this.FD_MgDrawOdArrow = new MagicItem.FnMagicRun(this.MgDrawOdLaser);
			}
			if (this.ALaserMn == null)
			{
				this.ALaserMn = new MagicNotifiear[8];
				for (int i = 0; i < 8; i++)
				{
					this.ALaserMn[i] = new MagicNotifiear(1).AddHit(new MagicNotifiear.MnHit
					{
						type = MagicNotifiear.HIT.CIRCLE,
						maxt = 1f,
						thick = 0.11880001f,
						accel = 0f,
						accel_mint = 160f,
						accel_maxt = 0f,
						v0 = 0f,
						penetrate = false,
						aim_fixed = true,
						wall_hit = true,
						other_hit = false,
						cast_on_autotarget = true,
						draw_only_line = true
					});
				}
			}
			int num = 0;
			int num2 = 8;
			int num3 = (int)this.od_arrow_attr;
			if (is_absorb)
			{
				num = 10;
				num2 /= 2;
				num3 = (int)this.od_arrow_attr_grab;
			}
			for (int j = 0; j < num2; j++)
			{
				MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_BEAM, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRunOdArrow, this.FD_MgDrawOdArrow);
				int num4 = j % 4;
				magicItem.Mn = this.ALaserMn[j];
				magicItem.Mn._0.penetrate_only_mv = !is_absorb;
				magicItem.Mn._0.no_draw_wide = is_absorb;
				magicItem.dz = (float)(num + j);
				NelNGolemToyBow.prepareMagic(this, ref this.ALaserAtk, magicItem, num3, 0.5f);
				if (num3 == 3 && magicItem.Atk0.EpDmg != null)
				{
					magicItem.Atk0.EpDmg.vagina = 5;
					magicItem.Atk0.EpDmg.uterus = 4;
					magicItem.Atk0.EpDmg.urethra = 3;
					magicItem.Atk0.EpDmg.anal = 15;
					magicItem.Atk0.EpDmg.other = 1;
				}
				magicItem.Ray.HitLock((float)((num3 == 3) ? 4 : 31) + (float)((num3 == 3) ? 59 : 75) * ((float)num4 / 3f), null);
				magicItem.Atk0.nodamage_time = 0;
				magicItem.run(0f);
			}
		}

		private bool getOdArrowPos(MagicItem Mg, int id, out Vector2 P, out float def_agR, out bool slow_angle_walk)
		{
			P = default(Vector2);
			def_agR = 0f;
			slow_angle_walk = true;
			bool flag = true;
			float num3;
			if (id < 10)
			{
				int num = id / 2;
				int num2 = id % 2;
				PxlLayer layerByName = this.Anm.getCurrentDrawnFrame().getLayerByName((num2 == 0) ? "pod_arm_arrow_0" : "pod_arm_arrow_1");
				if (layerByName == null)
				{
					return false;
				}
				switch (num)
				{
				case 0:
					P = this.Anm.getMapPosForLayer(layerByName, 31f, 55f);
					break;
				case 1:
					P = this.Anm.getMapPosForLayer(layerByName, 65f, 50f);
					break;
				case 2:
					P = this.Anm.getMapPosForLayer(layerByName, 122f, 44f);
					break;
				case 3:
					P = this.Anm.getMapPosForLayer(layerByName, 154f, 40f);
					break;
				default:
					return false;
				}
				def_agR = (float)((num2 == 0) ? 35 : (-10)) + (-23f + 46f * ((float)num / 3f));
				num3 = 0.62831855f;
				flag = Mg.phase >= 3;
			}
			else
			{
				if (id >= 20)
				{
					return false;
				}
				PxlLayer layerByName2 = this.Anm.getCurrentDrawnFrame().getLayerByName("pod_arm_arrow_0");
				if (layerByName2 == null)
				{
					return false;
				}
				switch (id)
				{
				case 10:
					P = this.Anm.getMapPosForLayer(layerByName2, 68f, 28f);
					break;
				case 11:
					P = this.Anm.getMapPosForLayer(layerByName2, 71f, 54f);
					break;
				case 12:
					P = this.Anm.getMapPosForLayer(layerByName2, 75f, 108f);
					break;
				case 13:
					P = this.Anm.getMapPosForLayer(layerByName2, 78f, 134f);
					break;
				default:
					return false;
				}
				def_agR = 0f;
				num3 = 0.75398225f;
			}
			def_agR = def_agR / 180f * 3.1415927f;
			if (base.mpf_is_right < 0f)
			{
				def_agR = 3.1415927f - def_agR;
			}
			float num4 = this.Nai.target_x;
			float num5 = this.Nai.target_y;
			if (base.AimPr is PR)
			{
				Vector3 hipPos = (base.AimPr as PR).getAnimator().getHipPos();
				num4 = hipPos.x;
				num5 = hipPos.y;
			}
			float num6 = this.Mp.GAR(P.x, P.y, num4, num5 + ((id >= 10) ? (-0.07f) : 0f));
			float num7 = X.angledifR(def_agR, num6);
			if (X.Abs(num7) <= num3)
			{
				if (flag)
				{
					slow_angle_walk = !base.isAbsorbState() || this.Absorb == null || !this.Absorb.isActive();
					def_agR = X.correctangleR(def_agR + num7);
				}
			}
			else if (Mg.phase == 3)
			{
				def_agR += 1.5707964f * base.mpf_is_right;
			}
			return true;
		}

		private bool MgRunOdArrow(MagicItem Mg, float fcnt)
		{
			if (base.destructed || !this.canHoldMagic(Mg))
			{
				return false;
			}
			int num = NelNGolemToyBow.MgRunArrowLaser(Mg, fcnt, 4f);
			if (num == 0)
			{
				return false;
			}
			bool flag = false;
			if (Mg.phase == 1)
			{
				Mg.phase = 2;
				flag = true;
				Mg.t = 10f;
			}
			if (Mg.da <= 0f && Mg.phase < 4)
			{
				Mg.da = 4f;
				Vector2 vector;
				float num2;
				bool flag2;
				if (!this.getOdArrowPos(Mg, (int)Mg.dz, out vector, out num2, out flag2))
				{
					return false;
				}
				Mg.dx = vector.x;
				Mg.dy = vector.y;
				if (flag)
				{
					Mg.Mn._0.agR = num2;
					Mg.sx = Mg.dx;
					Mg.sy = Mg.dy;
				}
				else
				{
					Mg.Mn._0.agR = X.VALWALKANGLER(Mg.Mn._0.agR, num2, (flag2 ? 0.0045f : 0.012f) * fcnt * 3.1415927f);
				}
			}
			Mg.sx = X.VALWALK(Mg.sx, Mg.dx, 0.08f * base.scaleX * fcnt);
			Mg.sy = X.VALWALK(Mg.sy, Mg.dy, 0.08f * base.scaleY * fcnt);
			Mg.da -= fcnt;
			if (num == 2 && Mg.phase <= 3)
			{
				Mg.Mn._0.v0 = 18f;
				Mg.Mn._0.CalcReachable(Mg.Ray, new Vector2(Mg.sx, Mg.sy), 0f, null);
				Mg.Mn._0.accel_maxt = ((Mg.Mn._0.v0 > Mg.Mn._0.len) ? 0.6f : 0f);
				Mg.Mn._0.v0 = Mg.Mn._0.len;
			}
			return true;
		}

		private bool MgDrawOdLaser(MagicItem Mg, float fcnt)
		{
			if (base.destructed)
			{
				return false;
			}
			NelNGolemToyBow.MgDrawArrow(Mg, fcnt, true);
			float t = Mg.t;
			if (Mg.phase == 2)
			{
				BList<Color32> blist = Mg.Other as BList<Color32>;
				if (blist == null)
				{
					return true;
				}
				MeshDrawer mesh = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
				MeshDrawer mesh2 = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.SUB, false);
				float num = X.ZPOW(t, 30f);
				float num2 = X.ZPOW(1f - X.ZLINE(75f - t, 25f));
				float num3 = num * (20f + 4f * X.ZIGZAGI(t, 16f + 30f * (1f - X.ZSINV(t, 38f)) - 7f * X.ZSIN(t - 45f, 20f))) + num2 * 18f;
				mesh2.Col = mesh2.ColGrd.Set(blist[2]).mulA(X.ZSIN(t, 30f) * 0.66f + X.ZPOW(1f - X.ZLINE(75f - t, 30f)) * 0.34f).mulA(0.5f)
					.C;
				mesh2.ColGrd.mulA(0f);
				mesh.Col = mesh.ColGrd.Set(blist[0]).blend(uint.MaxValue, num * (0.1f + 0.1f * X.COSI(t + (float)(Mg.id * 18), 17.1f - (float)(Mg.id % 8) * 0.5f)) + num2 * 0.4f).C;
				float num4 = 15f;
				float num5 = 50f;
				if (Mg.dz >= 10f)
				{
					float num6 = -0.06283176f;
					float num7 = ((Mg.dz >= 10f) ? 3.1415927f : 6.2831855f);
					float num8 = -base.mpf_is_right * 3f;
					if (base.mpf_is_right < 0f)
					{
						num6 = 3.1415927f - num6;
					}
					mesh2.BlurArc2(num8, 0f, num4, num6 - num7 * 0.5f, num6 + num7 * 0.5f, 200f, num5, MTRX.cola.Set(mesh2.Col), mesh2.ColGrd, 0, -1000f);
					mesh.Arc(num8, 0f, num3, num6 - num7 * 0.5f, num6 + num7 * 0.5f, 0f);
				}
				else
				{
					mesh2.BlurPoly2(0f, 0f, num4, 0f, 24, 200f, num5, MTRX.cola.Set(mesh2.Col), mesh2.ColGrd);
					mesh.Poly(0f, 0f, num3, 0f, 24, 0f, false, 0f, 0f);
				}
			}
			if (Mg.phase == 3 && Mg.t < 8f)
			{
				if (!(Mg.Other is BList<Color32>))
				{
					return true;
				}
				MeshDrawer mesh3 = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
				MeshDrawer mesh4 = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.SUB, false);
				float num9 = 94f;
				float num10 = num9 + 7f;
				if (Mg.dz >= 10f)
				{
					float num11 = -0.06283176f;
					float num12 = ((Mg.dz >= 10f) ? 3.1415927f : 6.2831855f);
					float num13 = -base.mpf_is_right * 3f;
					if (base.mpf_is_right < 0f)
					{
						num11 = 3.1415927f - num11;
					}
					mesh4.Arc(num13, 0f, num10, num11 - num12 * 0.5f, num11 + num12 * 0.5f, 0f);
					mesh3.Arc(num13, 0f, num9, num11 - num12 * 0.5f, num11 + num12 * 0.5f, 0f);
				}
				else
				{
					mesh4.Poly(0f, 0f, num10, 0f, 30, 0f, false, 0f, 0f);
					mesh3.Poly(0f, 0f, num9, 0f, 30, 0f, false, 0f, 0f);
				}
			}
			return true;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			if (base.isOverDrive())
			{
				Aattr.Add(MGATTR.ACME);
				Aattr.Add(MGATTR.ACME);
				Aattr.Add(MGATTR.ACME);
				Aattr.Add(MGATTR.ACME);
				Aattr.Add(MGATTR.FIRE);
				Aattr.Add(MGATTR.ICE);
				Aattr.Add(MGATTR.THUNDER);
				return;
			}
			Aattr.Add(MGATTR.MUD);
		}

		public bool isFrontRotate()
		{
			return this.Nai.isFrontType(NAI.TYPE.PUNCH_0, PROG.ACTIVE) || this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE);
		}

		public override bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			if (Atk == this.AtkTackleBodyOd && this.isFrontRotate() && HitMv != null && HitMv.Mv is PR)
			{
				this.walk_st = ((this.walk_st >= 0) ? X.Mn(this.walk_st, 0) : (this.walk_st - 2));
			}
			if (HitMv != null && Atk.PublishMagic != null && Atk.PublishMagic.kind == MGKIND.BASIC_BEAM && base.isAbsorbState() && Atk.PublishMagic.dz >= 10f && base.AimPr != null && HitMv.Mv == base.AimPr)
			{
				this.walk_time += 1f;
				if (X.XORSP() < 0.3f)
				{
					base.AimPr.TeCon.setQuake(5f, 3, 3f, 0);
				}
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
			if (!apply_execute)
			{
				return nelAttackInfo;
			}
			nelAttackInfo.shuffleHpMpDmg(this, 1f, 1f, -1000, -1000);
			if (this.applyDamage(nelAttackInfo, false) > 0)
			{
				if (!base.isOverDrive())
				{
					this.normaljump_level = 9f;
					this.Nai.delay = 0f;
				}
				else
				{
					this.od_wait_continue += 1;
					this.NoDamage.Add(nelAttackInfo.ndmg, (float)X.Mx(90, nelAttackInfo.nodamage_time * 2));
				}
				return nelAttackInfo;
			}
			return null;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			if (base.isOverDrive())
			{
				if (this.Nai.HasF(NAI.FLAG.INJECTED, false))
				{
					base.addF(NelEnemy.FLAG.DMG_EFFECT_SHIELD);
				}
			}
			else if (this.Nai.isFrontType(NAI.TYPE.MAG, PROG.ACTIVE) && X.XORSP() < 0.6f)
			{
				this.normaljump_level += 4f;
			}
			return base.applyDamage(Atk, force);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return base.applyHpDamageRatio(Atk) * (base.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD) ? 0.1f : 1f);
		}

		public override int getMpDamageValue(NelAttackInfo Atk, int val)
		{
			return (int)((float)base.getMpDamageValue(Atk, val) * (base.hasF(NelEnemy.FLAG.DMG_EFFECT_SHIELD) ? 0f : 1f));
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null)
			{
				if (!base.isOverDrive())
				{
					if (Tk.type == NAI.TYPE.WALK || Tk.type == NAI.TYPE.BACKSTEP || Tk.type == NAI.TYPE.WALK_TO_WEED)
					{
						if (base.SpPoseIs("walk", "od_walk"))
						{
							this.SpSetPose("stand", -1, null, false);
						}
						this.Nai.addTypeLock(Tk.type, 40f);
					}
					if (Tk.type == NAI.TYPE.WALK)
					{
						this.Nai.AddF(NAI.FLAG.POWERED, 120f);
						this.Nai.RemF(NAI.FLAG.WANDERING);
					}
					if (Tk.type == NAI.TYPE.MAG_1)
					{
						this.Nai.addTypeLock(NAI.TYPE.MAG_1, 220f);
					}
					if (Tk.type == NAI.TYPE.WAIT && this.Nai.isAttackableLength(2f + this.sizex, -2.5f, 2.5f, false))
					{
						this.normaljump_level += 2f;
					}
				}
				else
				{
					if (this.SndLoopWalk != null)
					{
						this.SndLoopWalk.destruct();
						this.SndLoopWalk = null;
					}
					if (Tk.type == NAI.TYPE.MAG_0)
					{
						this.Nai.addTypeLock(NAI.TYPE.MAG_0, 400f);
						if (!this.Nai.HasF(NAI.FLAG.ATTACKED, false))
						{
							this.Nai.AddF(NAI.FLAG.ATTACKED, 200f);
							base.MpConsume(this.McsOdMissile, null, 1f, 1f);
						}
					}
					if (Tk.type == NAI.TYPE.PUNCH_0 || Tk.type == NAI.TYPE.PUNCH)
					{
						base.addF(NelEnemy.FLAG.CHECK_ENLARGE);
						this.Nai.AddF(NAI.FLAG.WANDERING, X.NIXP(700f, 1800f));
					}
					if (Tk.type == NAI.TYPE.WAIT)
					{
						if (this.od_wait_continue < 6)
						{
							M2BlockColliderContainer.BCCLine lastBCC = this.FootD.get_LastBCC();
							if (lastBCC == null)
							{
								this.od_wait_continue += 4;
							}
							else
							{
								int num = X.IntC(this.sizex);
								float num2;
								float num3;
								M2BlockColliderContainer.BCCLine bccline;
								M2BlockColliderContainer.BCCLine bccline2;
								lastBCC.getLinearWalkableArea(5f, out num2, out num3, out bccline, out bccline2, (float)(num + 6));
								if (num3 - num2 < (float)(num * 2 + 4) && lastBCC.isLinearWalkableTo(this.Nai.TargetLastBcc, 8) == 0)
								{
									this.od_wait_continue += 3;
								}
								else
								{
									this.od_wait_continue += 1;
								}
							}
						}
					}
					else if (!this.Nai.HasF(NAI.FLAG.BOTHERED, false))
					{
						this.od_wait_continue = 0;
					}
					if (this.Danger != null)
					{
						this.Danger.enabled = true;
					}
				}
				this.Anm.fnFineFrame = null;
			}
			base.throw_ray = false;
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			this.Anm.rotationR = 0f;
			if (this.Hinge != null && !this.MhJumpWire.isActive(this))
			{
				this.Hinge.enabled = false;
			}
			this.can_hold_tackle = false;
			this.Anm.timescale = 1f;
			this.Phy.walk_xspeed = 0f;
			base.remF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
			base.remF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			base.quitTicket(Tk);
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			if (this.Nai == null || !this.is_alive || !this.canAbsorbContinue())
			{
				return false;
			}
			if (Mg.kind == MGKIND.TACKLE)
			{
				return this.canAbsorbContinue() && this.can_hold_tackle;
			}
			if (Mg.kind == MGKIND.BASIC_BEAM)
			{
				if (base.isAbsorbState())
				{
					return X.BTW(10f, Mg.dz, 14f);
				}
				if (this.Nai.isFrontType(NAI.TYPE.GUARD, PROG.ACTIVE))
				{
					return X.BTW(0f, Mg.dz, 8f);
				}
			}
			return false;
		}

		protected NelAttackInfo AtkShotHit = new NelAttackInfo
		{
			hpdmg0 = 35,
			split_mpdmg = 16,
			burst_vx = 0.03f,
			huttobi_ratio = 0.15f,
			parryable = true,
			Beto = BetoInfo.Mud,
			attr = MGATTR.MUD
		}.Torn(0.01f, 0.04f);

		private MagicNotifiear MnShot = new MagicNotifiear(2).AddHit(new MagicNotifiear.MnHit
		{
			type = MagicNotifiear.HIT.CIRCLE,
			maxt = 90f,
			accel = 0f,
			v0 = 0.35f,
			thick = 0.15f,
			draw_only_line = true,
			cast_on_autotarget = true,
			penetrate_only_mv = true,
			penetrate = false,
			aim_fixed = true
		});

		private NOD.MpConsume McsRecharge = NOD.getMpConsume("mgolem_recharge");

		private NOD.MpConsume McsOdMissile = NOD.getMpConsume("mgolem_od_missile");

		private NOD.MpConsume McsOdArrow = NOD.getMpConsume("mgolem_od_arrow");

		private const float normal_walk_spd_min = 0.11f;

		private const float normal_walk_spd_max = 0.135f;

		private byte ammo = 3;

		private const int AMMO_MAX = 3;

		private const float normaljump_agR = 0.43633235f;

		private const float normaljump_max_agR = 0.62831855f;

		private const float normaljump_reach_len = 16f;

		private const float normaljump_needle_radius = 0.15f;

		private const float normaljump_min_len = 4f;

		private const float normalshot_x_range = 7f;

		private const float normalshot_y_range = 6f;

		private const float normalshot_radius = 0.15f;

		private const float normalshot_spd = 0.35f;

		private const float normalshot_maxt = 90f;

		private const float normalshot_rangeR = 1.3194689f;

		private EnemyAnimatorMultiMech AnmMM;

		private HingeCreator Hinge;

		private Rigidbody2D OrbitRgd;

		private PxlSequence SqJumpNeedle;

		private PxlSequence SqShotParts;

		private PxlSequence SqOdMissile;

		private MagicItemHandlerS MhJumpWire;

		private MagicItemHandlerS MhGun;

		private EnemyAnimator.EnemyAdditionalDrawFrame EadShotHand;

		public float normaljump_level;

		public const float normaljump_threshold = 4.5f;

		protected NelAttackInfo AtkOdMissile = new NelAttackInfo
		{
			hpdmg0 = 16,
			split_mpdmg = 7,
			burst_vx = 0.04f,
			huttobi_ratio = 0.02f,
			attr = MGATTR.BOMB,
			Beto = BetoInfo.Lava.Pow(55, false),
			shield_break_ratio = -4f,
			parryable = true
		}.Torn(0.02f, 0.14f);

		private const float od_rotate_xspd = 0.15f;

		private const float knockback_od_tackle = 1.2f;

		protected NelAttackInfo AtkTackleBodyOd = new NelAttackInfo
		{
			hpdmg0 = 10,
			split_mpdmg = 2,
			mpdmg0 = 4,
			burst_vx = 0.38f,
			burst_center = 0.01f,
			shield_break_ratio = 1.5f,
			knockback_len = 1.2f,
			huttobi_ratio = 0.8f,
			press_state_replace = 3,
			nodamage_time = 14,
			shield_success_nodamage = 20f,
			Beto = BetoInfo.Normal,
			parryable = true
		}.Torn(0.02f, 0.2f);

		protected NelAttackInfo AtkCeilDrop = new NelAttackInfo
		{
			hpdmg0 = 14,
			split_mpdmg = 4,
			burst_vx = 0.04f,
			burst_center = 0.01f,
			huttobi_ratio = 0f,
			shield_success_nodamage = 14f,
			Beto = BetoInfo.Ground,
			parryable = true
		}.Torn(0.02f, 0.2f);

		private NOD.TackleInfo TkiOdGrab = NOD.getTackle("mgolem_od_grab_0");

		protected NelAttackInfo AtkOdGrab = new NelAttackInfo
		{
			hpdmg0 = 14,
			ndmg = NDMG.GRAB,
			split_mpdmg = 3,
			Beto = BetoInfo.Grab,
			is_penetrate_grab_attack = true,
			shield_break_ratio = 1.5f
		};

		protected NelAttackInfo AtkAbsorbMain = new NelAttackInfo
		{
			split_mpdmg = 8,
			hpdmg0 = 8,
			attr = MGATTR.BITE_V,
			Beto = BetoInfo.Blood
		}.Torn(0.03f, 0.05f);

		private const int OD_LASER_MAX = 8;

		private NelAttackInfo[] ALaserAtk;

		private MagicNotifiear[] ALaserMn;

		public const float od_arrow_reach_len = 18f;

		public const float od_arrow_power = 0.5f;

		public const float od_arrow_radius = 0.11880001f;

		public const float od_arrow_shoot_time = 160f;

		public const float od_arrow_charge_time = 75f;

		private const float od_arrow_shoot_after_time = 80f;

		private const float od_arrow_rangeR = 0.62831855f;

		private const float od_arrow_rangeR_absorb = 0.75398225f;

		private const float mispod_rangeR = 0.6911504f;

		private const int OD_MISSILE_MAX = 32;

		private const int OD_MISSILE_MAXH = 16;

		private const NAI.TYPE NTYPE_JUMPWIRE = NAI.TYPE.GUARD;

		private const NAI.TYPE NTYPE_JUMPWIRE_CLOSE = NAI.TYPE.GUARD_0;

		private const NAI.TYPE NTYPE_GUNSHOT = NAI.TYPE.MAG;

		private const NAI.TYPE NTYPE_RECHARGE = NAI.TYPE.MAG_1;

		private const NAI.TYPE NTYPE_OD_MISSILE = NAI.TYPE.MAG_0;

		private const NAI.TYPE NTYPE_OD_ROTATE_CLOSER = NAI.TYPE.PUNCH;

		private const NAI.TYPE NTYPE_OD_ROTATE = NAI.TYPE.PUNCH_0;

		private const NAI.TYPE NTYPE_OD_GRAB = NAI.TYPE.GUARD_1;

		private const NAI.TYPE NTYPE_OD_GRAB2 = NAI.TYPE.GUARD_2;

		private const NAI.TYPE NTYPE_OD_ARROW = NAI.TYPE.GUARD;

		private static string[] Amis_layer_name;

		private static float floort_danger_ground = 0f;

		private byte od_wait_continue;

		private byte od_arrow_attr_grab;

		private byte od_arrow_attr;

		private NASDangerChecker Danger;

		private const int PRI_SHOT = 100;

		private const int PRI_OD = 128;

		private const int PRI_MOVE = 3;

		private const int PRI_WAIT = 1;

		public MagicItem.FnMagicRun FD_MgRunWireShot;

		public MagicItem.FnMagicRun FD_MgDrawWireShot;

		public MagicItem.FnMagicRun FD_MgRunGunShot;

		public MagicItem.FnMagicRun FD_MgDrawGunShot;

		public MagicItem.FnMagicRun FD_MgRunOdMisisle;

		public MagicItem.FnMagicRun FD_MgDrawOdMisisle;

		private M2DrawBinder.FnEffectBind FD_eye0;

		private M2DrawBinder.FnEffectBind FD_eye1;

		private M2DrawBinder.FnEffectBind FD_eye2;

		private EfParticleOnce EfPtcOnceRotateEye;

		private M2SndInterval SndLoopWalk;

		private static MagicItem.FnCheckMagicFrom FD_ProgressOdArrowForWalkSt = delegate(MagicItem Mg, M2MagicCaster Caster)
		{
			if (Mg.kind == MGKIND.BASIC_BEAM && Mg.Caster == Caster && Mg.phase < 3)
			{
				NelNMechGolem nelNMechGolem = Caster as NelNMechGolem;
				if (Mg.dz < (float)nelNMechGolem.walk_st)
				{
					Mg.phase = 3;
					Mg.t = 1f;
					Mg.PtcVar("agR", (double)Mg.aim_agR).PtcVarS("attr", FEnum<MGATTR>.ToStr(Mg.Atk0.attr).ToString()).PtcST("golemtoy_arrow_shot_rotate", PTCThread.StFollow.FOLLOW_S, false);
					return true;
				}
			}
			return false;
		};

		private static MagicItem.FnCheckMagicFrom FD_QuitOdArrow = delegate(MagicItem Mg, M2MagicCaster Caster)
		{
			if (Mg.kind == MGKIND.BASIC_BEAM && Mg.Caster == Caster)
			{
				Mg.phase = 4;
				Mg.t = 0f;
			}
			return false;
		};

		public MagicItem.FnMagicRun FD_MgRunOdArrow;

		public MagicItem.FnMagicRun FD_MgDrawOdArrow;
	}
}
