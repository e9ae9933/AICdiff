using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNMage : NelEnemyMagicable
	{
		public override void Awake()
		{
			if (this.Anm == null)
			{
				this.Anm = new EnemyAnimator(this, new EnemyAnimator.FnCreate(NelNMage.EnemyFrameDataWithCane.CreateCane), new EnemyAnimator.FnFineFrame(this.fnFineAnimationFrame), true);
			}
			base.Awake();
		}

		private void fnFineAnimationFrame(EnemyFrameDataBasic nF, PxlFrame F)
		{
			NelNMage.EnemyFrameDataWithCane enemyFrameDataWithCane = nF as NelNMage.EnemyFrameDataWithCane;
			if (enemyFrameDataWithCane.CaneL != null)
			{
				this.TargetPos = enemyFrameDataWithCane.Target;
				this.Anm.layer_mask &= ~(1U << enemyFrameDataWithCane.cane_index);
			}
			else
			{
				this.TargetPos = Vector2.zero;
			}
			if (this.DrawerMagicCane != null)
			{
				this.DrawerMagicCane.CaneL = enemyFrameDataWithCane.CaneL;
				this.DrawerMagicCane.need_fine_mesh = true;
			}
		}

		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 50f;
			this.McsWarp = NOD.getMpConsume("mage_warp");
			ENEMYID id = this.id;
			this.id = ENEMYID.MAGE_0;
			NOD.BasicData basicData = NOD.getBasicData("MAGE_0");
			this.McsChant = NOD.getMpConsume("mage_chant_0");
			base.base_gravity = 0f;
			base.appear(_Mp, basicData);
			this.max_castable_count = this.maxmp / this.McsChant.consume;
			this.no_apply_gas_damage = (this.no_apply_map_damage = true);
			this.Nai.consider_only_onfoot = false;
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 12f;
			this.Nai.attackable_length_top = -10f;
			this.Nai.attackable_length_bottom = 10f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnly;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = NAI.FD_SleepOnly;
			this.Nai.suit_distance = 7.5f;
			this.DrawerMagicCane = new EnemyMeshDrawerMagicCane(this, this.max_castable_count);
			this.Nai.initWalkRegionAir().initDummy();
			this.DrawerMagicCane.prepareMesh(this.Anm.getMI(), this.TeCon).prepareRendetTicket(null, null, null);
			this.Chaser = new NASAirChaser(this, new NASAirChaser.FnProgressWalk(this.fnChaserProgressWalk))
			{
				fnGetPointAddition = new M2ChaserAir.FnGetPointAddition(this.fnChaserGetPointAddition)
			};
			base.addF(NelEnemy.FLAG.NO_AUTO_LANDFALL_POSE_SET);
			this.absorb_weight = 1;
		}

		protected override MGKIND get_magic_kind()
		{
			return MGKIND.THUNDERBOLT;
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (this.warp_time > 0f)
			{
				this.warp_time = X.Mx(this.warp_time - this.TS, 0f);
			}
		}

		protected override bool runSummoned()
		{
			float t = this.t;
			if (this.t >= 60f && this.walk_st == 0)
			{
				this.Nai.delay = 40f;
				this.SpSetPose("rotate2stand", -1, null, false);
			}
			return base.runSummoned();
		}

		public override void destruct()
		{
			base.destruct();
			if (this.DrawerMagicCane != null)
			{
				this.DrawerMagicCane.destruct();
				this.DrawerMagicCane = null;
			}
		}

		public override bool runOverDriveActivate()
		{
			bool flag = base.runOverDriveActivate();
			if (this.t <= 0f)
			{
				this.Anm.setDuration(this.Od.transform_duration);
			}
			if (flag)
			{
				this.Nai.delay = 45f;
			}
			return flag;
		}

		public bool runOverDriveAppeal(NaTicket Tk)
		{
			if (!base.isOverDrive())
			{
				return false;
			}
			if (Tk.initProgress(this))
			{
				base.AimToPlayer();
				this.SpSetPose("od_appeal", -1, null, false);
				OverDriveManager.initGrowl(this);
				this.t = (this.walk_time = 0f);
			}
			if (this.t >= 100f && this.walk_time == 0f)
			{
				this.SpSetPose("stand", -1, null, false);
				this.walk_time = 1f;
			}
			return this.t < 130f;
		}

		private bool is_magic_explode_interesting()
		{
			return this.Nai.isFrontType(NAI.TYPE.MAG_0, PROG.ACTIVE) || (!this.Nai.hasPriorityTicket(150, false, false) && this.Mp.canThroughXy(base.x, base.y, this.Nai.target_x, this.Nai.target_y, 0f) && this.Nai.fine_target_pos_lock <= 0f);
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.here_dangerous)
			{
				Nai.AddF(NAI.FLAG.ESCAPE, 180f);
			}
			if (Nai.fine_target_pos_lock <= 0f && X.LENGTHXYS(base.x, base.y, Nai.target_x, Nai.target_y) > 20f && Nai.RANtk(443) < 0.8f)
			{
				Nai.AddTicket(NAI.TYPE.WARP, 150, true);
				return true;
			}
			if (Nai.isPrGaraakiState() && this.has_chantable_mp() && X.chkLEN(base.x, base.y, Nai.target_x, Nai.target_y, 12f))
			{
				if (base.magic_explodable())
				{
					if (this.is_magic_explode_interesting() && Nai.fnBasicMagic(Nai, 150, 0f, 100f, 0f, 0f, 7145, false))
					{
						return true;
					}
					if (this.canWarp())
					{
						Nai.AddTicket(NAI.TYPE.WARP, 150, true);
						return true;
					}
				}
				else if (Nai.fnBasicMagic(Nai, 150, 86f, 0f, 0f, 0f, 7145, false))
				{
					return true;
				}
				if (Nai.fnAwakeBasicHead(Nai, NAI.TYPE.GAZE))
				{
					return true;
				}
			}
			else
			{
				bool flag = Nai.isPrAttacking(1f) || Nai.RANtk(398) < 0.33f;
				if ((Nai.isFrontType(NAI.TYPE.MAG, PROG.ACTIVE) || flag) && this.isPrNear(base.x, base.y))
				{
					Nai.AddF(NAI.FLAG.ESCAPE, 180f);
				}
				if (Nai.HasF(NAI.FLAG.ESCAPE, true))
				{
					if (Nai.isPrMagicExploded(1f) || flag)
					{
						if (this.canWarp())
						{
							Nai.AddTicket(NAI.TYPE.WARP, 150, true);
							return true;
						}
					}
					else if (!Nai.hasPriorityTicket(120, true, false))
					{
						Nai.AddTicket(NAI.TYPE.BACKSTEP, 120, true);
						return true;
					}
				}
				if (Nai.fnAwakeBasicHead(Nai, NAI.TYPE.GAZE))
				{
					return true;
				}
				if (!Nai.hasPriorityTicket(6, false, false) && this.has_chantable_mp())
				{
					bool flag2 = base.AimPr is PR && (base.AimPr as PR).magic_chanting;
					if (base.magic_explodable() && X.chkLEN(base.x, base.y, Nai.target_x, Nai.target_y, 12f))
					{
						if (Nai.RANtk(944) < (flag2 ? 0.08f : 0.5f))
						{
							if (this.is_magic_explode_interesting() && Nai.fnBasicMagic(Nai, 150, 0f, 100f, 0f, 0f, 7145, false))
							{
								return true;
							}
							if (this.canWarp())
							{
								Nai.AddTicket(NAI.TYPE.WARP, 150, true);
								return true;
							}
						}
					}
					else if (Nai.fnBasicMagic(Nai, 6, (float)(flag2 ? 10 : (X.chkLEN(base.x, base.y, Nai.target_x, Nai.target_y, 17f) ? 35 : 0)), 0f, 0f, 0f, 7145, false))
					{
						return true;
					}
				}
			}
			if (!Nai.hasPriorityTicket(0, false, false))
			{
				if (base.mp_ratio < 0.5f)
				{
					NaTicket naTicket = Nai.AddTicketSearchAndGetManaWeed(0, 10f, -8f, 8f, 0.9f, 0f, this.sizey + 1.52f, true);
					if (naTicket != null)
					{
						if (naTicket.type == NAI.TYPE.PUNCH_WEED)
						{
							naTicket.priority = 150;
						}
						return true;
					}
				}
				if (!Nai.hasTypeLock(NAI.TYPE.WALK))
				{
					Nai.AddTicket(NAI.TYPE.WALK, 1, true).CheckNearPlaceError(2).Dep(Nai.target_x, Nai.target_y, null);
				}
			}
			return true;
		}

		public override bool has_chantable_mp()
		{
			return this.Useable(this.McsChant, X.Mx(1f, this.Nai.RANtk(3441) * 2.5f), 0f);
		}

		public bool canWarp()
		{
			return this.Useable(this.McsWarp, 1f, (float)(this.McsChant.consume + 6)) && !this.Nai.hasPriorityTicket(150, false, false) && !this.Nai.hasTypeLock(NAI.TYPE.WARP);
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
			case NAI.TYPE.WALK_TO_WEED:
				break;
			case NAI.TYPE.PUNCH:
			case NAI.TYPE.PUNCH_0:
			case NAI.TYPE.PUNCH_1:
			case NAI.TYPE.PUNCH_2:
				goto IL_00E1;
			case NAI.TYPE.PUNCH_WEED:
				return this.runPunchWeed(Tk);
			case NAI.TYPE.MAG:
				return this.runMagicCharge(Tk);
			case NAI.TYPE.MAG_0:
				return this.runMagicExplode(Tk);
			default:
				switch (type)
				{
				case NAI.TYPE.BACKSTEP:
					break;
				case NAI.TYPE.WARP:
					return this.runWarp(Tk);
				case NAI.TYPE.GAZE:
					this.SpSetPose("stand", -1, null, false);
					base.readTicket(Tk);
					return Tk.t < 20f + this.Nai.RANtk(445) * 40f;
				case NAI.TYPE.WAIT:
					base.readTicket(Tk);
					return Tk.t < 20f + this.Nai.RANtk(445) * 40f;
				default:
					goto IL_00E1;
				}
				break;
			}
			return this.runWalk(Tk);
			IL_00E1:
			return base.readTicket(Tk);
		}

		protected bool runWalk(NaTicket Tk)
		{
			bool flag = Tk.type == NAI.TYPE.BACKSTEP;
			bool flag2 = Tk.initProgress(this);
			if (flag2)
			{
				base.base_gravity = 0f;
				Tk.prog = PROG.PROG5;
				this.walk_st = 0;
			}
			if (Tk.prog == PROG.PROG5)
			{
				this.Chaser.Clear(false);
				this.Chaser.run_speed = 0.07f;
				this.walk_time = (float)(flag ? (flag2 ? (-40) : (-80)) : 0);
				Tk.prog = PROG.PROG4;
				this.walk_st = 0;
			}
			if (Tk.prog == PROG.PROG4)
			{
				Tk.prog = PROG.ACTIVE;
				if (Tk.type == NAI.TYPE.WALK_TO_WEED)
				{
					this.Chaser.setDest(Tk.depx, Tk.depy - 0.2f);
				}
				else
				{
					float num = 5.5f;
					int num2 = 3;
					float num3 = base.x;
					float num4 = base.y;
					float num5;
					if (flag)
					{
						num2 = 8;
						num = 7f;
						num5 = X.XORSP() * 6.2831855f;
					}
					else
					{
						this.Chaser.setDest(Tk.depx, Tk.depy);
						num5 = this.Mp.GAR(Tk.depx, Tk.depy, base.x, base.y) + X.XORSPS() * 0.38f * 3.1415927f;
						float num6 = X.NIXP(0.85f - (float)this.walk_st * 0.06f, 1.15f) * this.Nai.suit_distance;
						num3 = Tk.depx + X.Cos(num5) * num6;
						num4 = Tk.depy - X.Sin(num5) * num6;
					}
					for (int i = 0; i < 30; i++)
					{
						float num7 = num3 + num * X.Cos(num5);
						float num8 = num4 + num * X.Sin(num5);
						if (!X.chkLEN(this.Nai.target_x, this.Nai.target_y, num7, num8, 6.5f))
						{
							if (this.Nai.isAreaSafe(num7, num8, 0.125f, 0.25f, true, true, true) && !this.Nai.isDecliningXy(num7, num8, 0.5f, 0.5f))
							{
								this.Chaser.setDest(num7, num8);
								if (--num2 <= 0)
								{
									break;
								}
							}
							num += 0.3f;
							num5 += (0.5f + X.XORSP()) / 7f * 6.2831855f * 5f;
						}
					}
				}
				if (!this.Chaser.hasDestPoint())
				{
					int num9 = this.walk_st + 1;
					this.walk_st = num9;
					if (num9 >= 5)
					{
						if (flag)
						{
							this.Nai.AddF(NAI.FLAG.ESCAPE, 180f);
						}
						this.walkRandomJump();
						return Tk.error();
					}
					return true;
				}
				else
				{
					if (this.walk_time >= 0f && (this.isPrNear(base.x, base.y) || this.walk_st == 1))
					{
						Tk.prog = PROG.PROG0;
					}
					this.walk_st = 0;
				}
			}
			M2ChaserBaseFuncs.CHSRES chsres = this.Chaser.Walk(this.TS);
			if (chsres == M2ChaserBaseFuncs.CHSRES.ERROR)
			{
				this.walkRandomJump();
				return Tk.error();
			}
			if (chsres == M2ChaserBaseFuncs.CHSRES.REACHED)
			{
				if (this.SpPoseIs("walk"))
				{
					this.SpSetPose("walk2stand", -1, null, false);
				}
				Tk.after_delay = 20f;
				return false;
			}
			if (this.walk_time >= 0f && this.isPrNear(base.x, base.y) && Tk.prog != PROG.PROG0 && this.Nai.RANtk(4944) < 0.6f)
			{
				this.Nai.AddF(NAI.FLAG.ESCAPE, 180f);
				this.Phy.remFoc(FOCTYPE.WALK, true);
				return Tk.error();
			}
			bool flag3 = false;
			M2ChaserAir.ChaserReachedDepert chaserReachedDepert;
			if ((chsres == M2ChaserBaseFuncs.CHSRES.FOUND || chsres == M2ChaserBaseFuncs.CHSRES.PROGRESS) && this.Chaser.getCalcedDepert(out chaserReachedDepert) && this.isPrNear(chaserReachedDepert.x, chaserReachedDepert.y))
			{
				flag3 = true;
			}
			if (flag3)
			{
				if (this.Chaser.getCalcedDepertCound() <= 1)
				{
					Tk.prog = PROG.PROG0;
				}
				else
				{
					this.Chaser.skipThisDepert();
				}
			}
			if (Tk.prog == PROG.PROG0 || this.walk_time < 0f)
			{
				this.walk_time += this.TS;
				if (this.walk_time >= 14f)
				{
					if (Tk.type == NAI.TYPE.BACKSTEP)
					{
						this.Nai.AddF(NAI.FLAG.ESCAPE, 180f);
						this.Phy.remFoc(FOCTYPE.WALK, true);
						return false;
					}
					Tk.prog = PROG.PROG5;
					Tk.type = NAI.TYPE.BACKSTEP;
					this.walk_time = 0f;
					this.walk_st = 1;
				}
			}
			return true;
		}

		private void fnChaserGetPointAddition(ref M2ChaserAir.ChaserReachedDepert Depert)
		{
			Vector2 vector = new Vector2(Depert.x, Depert.y);
			Depert.point_addition += (X.LENGTHXY2(vector.x, vector.y, this.Nai.target_x, this.Nai.target_y) - this.Nai.suit_distance * this.Nai.suit_distance) * 3f;
		}

		private void walkRandomJump()
		{
			float num = X.NIXP(0.1f, 0.15f);
			float num2 = this.Mp.GAR(base.x, base.y, base.x - X.NIXP(2f, 4f) * (float)X.MPF(base.x < this.Nai.target_x), this.Nai.target_y - 1.5f);
			this.Phy.addFoc(FOCTYPE.WALK, num * X.Cos(num2), -num * X.Sin(num2), -1f, 0, 8, 40, -1, 0);
			this.Nai.delay = 50f;
			if (this.SpPoseIs("walk"))
			{
				this.SpSetPose("walk2stand", -1, null, false);
			}
		}

		private bool fnChaserProgressWalk(M2ChaserAir Chaser, M2ChaserAir.ChaserReachedDepert Depert, Vector2 Next, float agR, bool pos_updated)
		{
			if (this.Chaser.warp_flag)
			{
				return true;
			}
			float num = 0.07f * X.Cos(agR);
			this.Phy.addFoc(FOCTYPE.WALK, num, -0.07f * X.Sin(agR), -1f, -1, 1, 0, -1, 0);
			if (num != 0f && num > 0f != base.mpf_is_right > 0f)
			{
				base.AimToLr((num > 0f) ? 2 : 0);
			}
			this.SpSetPose("walk", -1, null, false);
			return false;
		}

		private bool isPrNear(float x, float y)
		{
			Vector2 runExpectPos = this.Nai.getRunExpectPos();
			return X.chkLEN(x, y, runExpectPos.x, runExpectPos.y, 3.5f);
		}

		protected bool runWarp(NaTicket Tk)
		{
			if (Tk.initProgress(this))
			{
				base.base_gravity = 0f;
				this.Chaser.Clear(true);
				this.Chaser.run_speed = 0f;
				if (base.AimPr == null)
				{
					return false;
				}
				this.t = (float)(100 - (this.Nai.isPrMagicExploded(1f) ? X.Mx(-5 + X.xors(19), 0) : X.xors(7)));
				float num = 6f;
				int num2 = 10;
				float target_x = this.Nai.target_x;
				float target_y = this.Nai.target_y;
				float num3 = X.XORSP() * 6.2831855f;
				for (int i = 0; i < 30; i++)
				{
					float num4 = target_x + num * X.Cos(num3);
					float num5 = target_y + num * X.Sin(num3);
					if (!X.chkLEN(this.Nai.target_x, this.Nai.target_y, num4, num5, 4.5f))
					{
						if (this.Nai.isAreaSafe(num4, num5, 0.125f, 0.25f, true, true, true))
						{
							this.Chaser.setDest(num4, num5);
							if (--num2 <= 0)
							{
								break;
							}
						}
						num += 0.3f;
						num3 += (0.5f + X.XORSP()) / 7f * 6.2831855f * 5f;
					}
				}
				if (!this.Chaser.hasDestPoint())
				{
					return Tk.error();
				}
				this.SpSetPose("warp", -1, null, false);
			}
			M2ChaserBaseFuncs.CHSRES chsres = this.Chaser.Walk(this.TS);
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.t >= 115f)
				{
					this.warp_time = 10f;
					base.throw_ray = true;
				}
				if (Tk.Progress(ref this.t, 122, true))
				{
					base.PtcST("enemy_warp", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.MpConsume(this.McsWarp, null, 1f, 1f);
				}
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, 13, true))
			{
				M2ChaserAir.ChaserReachedDepert chaserReachedDepert;
				if (chsres == M2ChaserBaseFuncs.CHSRES.FOUND && this.Chaser.getCalcedDepert(out chaserReachedDepert))
				{
					NelEnemy.warpParticleSetting(this.Mp, base.x, base.y, chaserReachedDepert.x, chaserReachedDepert.y, "enemy_warp_star", 2.25f);
					this.setTo(chaserReachedDepert.x, chaserReachedDepert.y);
					this.Phy.killSpeedForce(true, true, true, false, false);
					this.Phy.walk_xspeed = 0f;
					base.base_gravity = 0f;
					base.throw_ray = true;
				}
				base.PtcST("enemy_warp_out", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.SpSetPose("warp_2", -1, null, false);
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.t >= 13f)
				{
					base.throw_ray = false;
				}
				if (this.t >= 50f)
				{
					this.Phy.killSpeedForce(true, true, true, false, false);
					this.Phy.walk_xspeed = 0f;
					this.Nai.addTypeLock(NAI.TYPE.WARP, 100f);
					return false;
				}
			}
			return true;
		}

		public int FnChaserSortPlayerWarp(M2ChaserRootBase<M2AirRegion> pA, M2ChaserRootBase<M2AirRegion> pB)
		{
			return 0;
		}

		public bool runMagicCharge(NaTicket Tk)
		{
			if (base.AimPr == null)
			{
				return false;
			}
			if (Tk.initProgress(this))
			{
				base.AimToPlayer();
				base.base_gravity = 0f;
				this.t = 0f;
				this.walk_time = 0f;
			}
			if (this.t >= 14f)
			{
				if (Tk.prog < PROG.PROG0)
				{
					Tk.prog = PROG.PROG0;
					this.SpSetPose("chant", -1, null, false);
					base.initChant();
				}
				base.progressChant();
				if (base.magic_explodable())
				{
					this.walk_time += this.TS;
					if (this.walk_time >= 20f)
					{
						if (!this.Nai.HasF(NAI.FLAG.ESCAPE, false) && X.chkLEN(base.x, base.y, this.Nai.target_x, this.Nai.target_y, 13f) && (!(base.AimPr is PR) || !(base.AimPr as PR).magic_chanting))
						{
							Tk.Recreate(NAI.TYPE.MAG_0, 150, true, null);
							return true;
						}
						return false;
					}
				}
			}
			return true;
		}

		public bool runMagicExplode(NaTicket Tk)
		{
			if (this.CurMg == null)
			{
				return false;
			}
			if (Tk.initProgress(this))
			{
				base.base_gravity = 0f;
				this.t = 0f;
				this.FixAimPos.Set(this.Nai.target_x, this.Nai.target_y);
				if (this.CurMg.Mn != null && this.CurMg.Mn._0.fnFixTargetMoverPos != null)
				{
					this.CurMg.Mn._0.fnFixTargetMoverPos(this.CurMg, this, this.Nai.target_x, this.Nai.target_y, ref this.FixAimPos, -1);
					this.Nai.target_x = this.FixAimPos.x;
					this.Nai.target_y = this.FixAimPos.y;
				}
				base.AimToPlayer();
				this.SpSetPose("magic_init", -1, null, false);
			}
			if (Tk.prog < PROG.PROG0)
			{
				if (this.t >= 24f)
				{
					Tk.prog = PROG.PROG0;
					this.SpSetPose("magic_shot", -1, null, false);
					if (!base.explodeMagic(true))
					{
						return false;
					}
					this.t = 0f;
				}
				else
				{
					this.Nai.fine_target_pos_lock = 2f;
				}
			}
			if (Tk.prog < PROG.PROG1 && this.t >= 80f)
			{
				Tk.prog = PROG.PROG1;
				this.SpSetPose("rotate2stand", -1, null, false);
				Tk.after_delay = 60f;
				return false;
			}
			return true;
		}

		public bool runPunchWeed(NaTicket Tk)
		{
			if (Tk.initProgress(this))
			{
				this.t = 0f;
				this.SpSetPose("attack_0", -1, null, false);
				this.Phy.addFoc(FOCTYPE.WALK, 0f, 0.07f, -1f, 0, 1, 20, -1, 0);
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 30, true))
			{
				base.PtcST("mage_small_punch_swing", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.SpSetPose("attack_1", -1, null, false);
				base.tackleInit(this.AtkSmallPunch, this.TkiSmallPunch, MGHIT.AUTO);
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 4f)
				{
					this.can_hold_tackle = false;
				}
				if (Tk.Progress(ref this.t, 50, true))
				{
					this.SpSetPose("attack2stand", -1, null, false);
					Tk.after_delay = 20f;
					return false;
				}
			}
			return true;
		}

		public override void quitTicket(NaTicket Tk)
		{
			base.quitTicket(Tk);
			base.disappearing = false;
			if (this.SpPoseIs("walk"))
			{
				this.SpSetPose("walk2stand", -1, null, false);
				return;
			}
			if (!base.SpPoseIs("walk2stand", "warp_2", "rotate2stand", "damage"))
			{
				this.SpSetPose("stand", -1, null, false);
			}
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			int num = base.applyDamage(Atk, force);
			if (num > 0 || force)
			{
				base.base_gravity = 0.66f;
			}
			return num;
		}

		public override bool runDamageSmall()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.SpSetPose("damage", 1, null, false);
			}
			if (base.base_gravity == 0f)
			{
				base.base_gravity = 0.66f;
			}
			if (base.hasFoot() && !base.runDamageSmall())
			{
				base.base_gravity = 0f;
				this.SpSetPose("stand", -1, null, false);
				return false;
			}
			return true;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			Aattr.Add(MGATTR.THUNDER);
		}

		public override Vector2 getAimPos(MagicItem Mg)
		{
			if (this.Nai != null)
			{
				return new Vector2(this.Nai.target_x, this.Nai.target_y);
			}
			return Mg.PosA;
		}

		public override int getAimDirection()
		{
			return -1;
		}

		public override Vector2 getTargetPos()
		{
			if (this.Mp == null)
			{
				return new Vector2(base.x, base.y);
			}
			float cane_scale = this.DrawerMagicCane.cane_scale;
			return new Vector2(base.x + this.TargetPos.x * 64f * this.Mp.rCLENB * cane_scale, base.y - this.TargetPos.y * 64f * this.Mp.rCLENB * cane_scale);
		}

		protected override bool noHitableAttack()
		{
			return this.warp_time > 0f || base.noHitableAttack();
		}

		public override float getEnlargeLevel()
		{
			if (base.isOverDrive())
			{
				return base.getEnlargeLevel();
			}
			return 1f + X.ZLINE((float)(this.mp / this.McsChant.consume), (float)this.max_castable_count);
		}

		public override float enlarge_level_to_anim_scale(float r = -1f)
		{
			float num = base.enlarge_level_to_anim_scale(r);
			if (this.DrawerMagicCane != null)
			{
				this.DrawerMagicCane.cane_scale = num;
				this.DrawerMagicCane.dance_object_count = this.mp / this.McsChant.consume;
			}
			if (!base.isOverDrive())
			{
				return 1f;
			}
			return num;
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle;
		}

		private const float normal_walk_spd = 0.07f;

		protected NelAttackInfo AtkSmallPunch = new NelAttackInfo
		{
			hpdmg0 = 1,
			huttobi_ratio = -100f,
			knockback_len = 0.3f,
			parryable = true
		};

		protected NOD.TackleInfo TkiSmallPunch = NOD.getTackle("mage_small_punch");

		public float warp_time;

		public Vector2 TargetPos;

		public Vector2 FixAimPos;

		public int max_castable_count;

		private NOD.MpConsume McsChant;

		private NOD.MpConsume McsWarp;

		private EnemyMeshDrawerMagicCane DrawerMagicCane;

		private NASAirChaser Chaser;

		private const int PRI_SHOT = 6;

		private const int PRI_GUARD = 100;

		private const int PRI_ESCAPE = 120;

		private const int PRI_WARP = 150;

		public sealed class EnemyFrameDataWithCane : EnemyFrameDataBasic
		{
			public EnemyFrameDataWithCane(EnemyAnimator Anm, PxlFrame F)
				: base(Anm, F)
			{
				int num = F.countLayers();
				for (int i = 0; i < num; i++)
				{
					PxlLayer layer = F.getLayer(i);
					if (layer.isImport() && TX.isStart(layer.name, "cane", 0))
					{
						this.cane_index = i;
						Matrix4x4 transformMatrix = layer.getTransformMatrix();
						this.CaneL = layer;
						Vector3 vector = transformMatrix.MultiplyPoint3x4(new Vector3((float)(-(float)layer.Img.width) * 0.5f + 16f, (float)layer.Img.height * 0.5f - 28f, 0f) * 0.015625f);
						this.Target = vector;
						return;
					}
				}
			}

			public static NelNMage.EnemyFrameDataWithCane CreateCane(EnemyAnimator Anm, PxlFrame F)
			{
				return new NelNMage.EnemyFrameDataWithCane(Anm, F);
			}

			public PxlLayer CaneL;

			public Vector2 Target;

			public int cane_index = -1;
		}
	}
}
