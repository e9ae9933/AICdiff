using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNPentapod : NelEnemy
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			ENEMYID id = this.id;
			this.id = ENEMYID.PENTAPOD_0;
			NOD.BasicData basicData = NOD.getBasicData("PENTAPOD_0");
			base.appear(_Mp, basicData);
			this.Anm.order_front = M2Mover.DRAW_ORDER.N_TOP_EF0;
			this.neck_len_cur = this.neck_len * 0.5f;
			this.Nai.awake_length = num;
			this.Nai.suit_distance = 5f;
			this.Nai.suit_distance_garaaki_ratio = 0.6f;
			this.Nai.attackable_length_x = 1.25f;
			this.Nai.attackable_length_top = -5f;
			this.Nai.attackable_length_bottom = 6f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.AtkTackleP0.Prepare(this, true);
			this.AtkAbsorb.Prepare(this, false);
			this.NasJumper = new NASSmallJumper(this, 0.066f, 6f)
			{
				FD_InitWalkMain = delegate(NaTicket Tk)
				{
					this.EnHead.AimToPlayer();
					this.walk_time = 0f;
					return true;
				},
				jumpable_x_range_max = 2f,
				FD_JumpInit = new NASSmallJumper.FnJumpInit(this.fnJumpInit),
				FD_RunWalking = new NAI.FnTicketRun(this.fnWalking)
			};
			this.absorb_weight = 2;
		}

		public override void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			base.quitSummonAndAppear(clearlock_on_summon);
			if (this.EnHead == null)
			{
				this.EnHead = NelEnemyNested.CreateNest(this, (this.id + 256U).ToString(), base.mp_ratio, 6) as NelNPentapodHead;
				this.EnHead.NestTeColor(true).NestManaAbsorb(true);
				this.EnHead.moveBy(base.x - this.EnHead.x, base.y - 3f - this.EnHead.y, false);
				this.mana_desire_multiple = 0.33f;
				this.EnHead.mana_desire_multiple = 1f - this.mana_desire_multiple;
				this.ANeck = new NelNPentapod.EnemyMeshDrawerPentaNeck[2];
				PxlSequence sequence = this.Anm.getCurrentCharacter().getPoseByName("_stroke").getSequence(0);
				for (int i = 0; i < 2; i++)
				{
					this.ANeck[i] = new NelNPentapod.EnemyMeshDrawerPentaNeck(this, this.EnHead, sequence.getImage(0, i));
				}
			}
		}

		public override void destruct()
		{
			if (this.AnmB != null)
			{
				this.AnmB.destruct();
				this.AnmB = null;
			}
			base.destruct();
			if (this.ANeck != null)
			{
				for (int i = 0; i < 2; i++)
				{
					this.ANeck[i].destruct();
				}
				this.ANeck = null;
			}
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
				this.Phy.remLockWallHitting(this);
				this.Anm.showToFront(false, false);
				this.Anm.fnFineFrame = null;
				if (st == NelEnemy.STATE.STAND)
				{
					this.Phy.addFoc(FOCTYPE.JUMP, X.XORSPS() * 0.05f, -0.14f, -1f, 0, 4, 40, 1, 0);
					if (this.SpPoseIs("absorb"))
					{
						this.SpSetPose("jump", -1, null, false);
					}
				}
				else if (this.SpPoseIs("absorb"))
				{
					this.SpSetPose("stand", -1, null, false);
				}
				this.Nai.addTypeLock(NAI.TYPE.PUNCH, 400f);
				this.Nai.addTypeLock(NAI.TYPE.MAG, 200f);
				this.far_suitlen_count = 4;
				if (this.AnmB != null)
				{
					this.AnmB.alpha = 0f;
				}
			}
			if (st == NelEnemy.STATE.STAND)
			{
				if (this.SpPoseIs(this.posename_damage_huttobi) || this.SpPoseIs(this.posename_damage))
				{
					this.SpSetPose("stand", -1, null, false);
				}
				if (state == NelEnemy.STATE.DAMAGE || state == NelEnemy.STATE.DAMAGE_HUTTOBI)
				{
					this.Nai.delay = 30f;
					this.Nai.addTypeLock(NAI.TYPE.PUNCH, 240f);
					this.Nai.addTypeLock(NAI.TYPE.MAG, 150f);
					this.far_suitlen_count = X.Mx(this.far_suitlen_count, 2 + X.xors(3));
				}
			}
			return this;
		}

		public override void runPost()
		{
			if (this.ANeck != null)
			{
				for (int i = 0; i < 2; i++)
				{
					this.ANeck[i].checkFrame(this.TS, false);
				}
			}
			base.runPost();
		}

		public override void setWalkXSpeed(float value, bool consider_water_scale = true, bool force_onfoot = false)
		{
			float num = base.x - this.EnHead.x;
			float num2 = this.neck_len * 0.5f;
			if (num > num2)
			{
				float num3 = X.NIL(1f, 0.33f, num - num2, num2);
				value *= num3;
			}
			this.Phy.setWalkXSpeed(value, consider_water_scale, force_onfoot);
		}

		public override bool runDamageSmall()
		{
			return base.runDamageSmall() || this.EnHead.getState() == NelEnemy.STATE.DAMAGE;
		}

		public override bool runDamageHuttobi()
		{
			return base.runDamageHuttobi() || this.EnHead.getState() == NelEnemy.STATE.DAMAGE_HUTTOBI;
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai, NAI.TYPE.GAZE))
			{
				return true;
			}
			if (this.EnHead.getState() == NelEnemy.STATE.STAND && !Nai.hasPriorityTicket(133, false, false))
			{
				if (this.Useable(this.McsBeam, 1.25f, 0f) && !this.EnHead.getAI().hasPriorityTicket(133, false, false) && !Nai.hasTypeLock(NAI.TYPE.MAG))
				{
					Vector3 beamHeadPos = this.getBeamHeadPos();
					if (beamHeadPos.z > 0f)
					{
						NAI.TYPE type = ((beamHeadPos.z == 2f) ? NAI.TYPE.MAG_0 : NAI.TYPE.MAG);
						this.EnHead.getAI().AddTicket(type, 133, true).Dep(beamHeadPos, null);
						Nai.AddTicket(type, 133, true).Dep(beamHeadPos, null);
						return true;
					}
					Nai.addTypeLock(NAI.TYPE.MAG, 100f);
				}
				if (this.Useable(this.McsAtk, 1.25f, 0f) && !Nai.hasTypeLock(NAI.TYPE.PUNCH))
				{
					float num = 1f;
					if (this.grabbing_mode)
					{
						num = 10f;
					}
					if (Nai.isAttackableLength(false) && !Nai.isPrAbsorbed() && Nai.RANtk(2387) < (Nai.isPrGaraakiState() ? ((Nai.suit_distance_garaaki_ratio <= 0.06f) ? 0.88f : 0.6f) : 0.2f) * num)
					{
						Nai.AddTicket(NAI.TYPE.PUNCH, 133, true).Dep(Nai.target_x, Nai.target_y, null);
						return true;
					}
				}
			}
			if (!Nai.hasPriorityTicket(5, false, false))
			{
				if (!this.Useable(this.McsBeam, 2f + (Nai.HasF(NAI.FLAG.POWERED, false) ? 1f : (Nai.RANtk(4733) * 2f)), 0f) && Nai.AddTicketSearchAndGetManaWeed(5, 12f, -8f, 8f, this.sizex + 0.05f, -this.sizey * 0.3f, this.sizey * 0.3f, true) != null)
				{
					return true;
				}
				if (Nai.AddMoveTicketToTarget(2f, 0f, 5, true, NAI.TYPE.WALK) != null)
				{
					return true;
				}
			}
			return Nai.fnBasicMove(Nai);
		}

		private bool considerOverDrive(NAI Nai)
		{
			return true;
		}

		public Vector3 getBeamHeadPos()
		{
			bool flag = !this.Mp.canThroughBcc(base.x, base.y, this.EnHead.x, this.EnHead.y, this.EnHead.sizex * 1.05f, -1000f, -1, true, true, null, false, null);
			float num = X.MMX(this.neck_len * 0.3f, X.LENGTHXYQ(base.x, base.y, this.EnHead.x, this.EnHead.y), this.neck_len);
			float num2 = this.Mp.GAR(base.x, base.y, this.EnHead.x, this.EnHead.y);
			float num3 = X.XORSP() * 3.1415927f;
			float num4 = base.x + X.Cos(num2) * num;
			float num5 = base.y - X.Sin(num2) * num;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			float num9 = this.neck_len;
			for (int i = 0; i < 16; i++)
			{
				float num10 = X.Cos(num3) * num9 * 0.66f;
				float num11 = num5 - X.Sin(num3) * num9 * 0.11f;
				if (num10 < this.EnHead.x != this.Nai.target_x < this.EnHead.x)
				{
					num10 *= 0.5f;
				}
				num10 += num4;
				if (this.Nai.isAreaSafe(num10, num11, 0f, 0f, false, true, true) && X.Abs(num10 - this.Nai.target_x) >= 1.5f)
				{
					float num12 = X.LENGTHXY2(num10, num11, this.Nai.target_x, this.Nai.target_y);
					if (X.BTW(X.Pow(3f, 2), num12, X.Pow(11.44f, 2)) && num8 < num12 && (flag || this.Mp.canThroughBcc(num10, num11, this.EnHead.x, this.EnHead.y, this.EnHead.sizex * 1.05f, -1000f, -1, false, true, null, false, null)) && this.Mp.canThroughBcc(num10, num11, this.Nai.target_x, this.Nai.target_y, 0.2f, -1000f, -1, false, false, null, false, null))
					{
						num8 = num12;
						num6 = num10;
						num7 = num11;
					}
				}
				num3 += 1.1623893f;
				num9 += 0.04f;
			}
			if (num8 > 0f)
			{
				return new Vector3(num6, num7, (float)(flag ? 2 : 1));
			}
			return Vector3.zero;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			A.Add("torture_backinj");
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			switch (type)
			{
			case NAI.TYPE.WALK:
			case NAI.TYPE.WALK_TO_WEED:
				if (!this.NasJumper.runWalk(Tk.initProgress(this), Tk, ref this.t))
				{
					if (Tk.type == NAI.TYPE.WALK_TO_WEED)
					{
						this.Nai.AddF(NAI.FLAG.POWERED, 120f);
					}
					return false;
				}
				return true;
			case NAI.TYPE.PUNCH:
			case NAI.TYPE.PUNCH_WEED:
				return this.runPunch(Tk.initProgress(this), Tk, Tk.type == NAI.TYPE.PUNCH_WEED);
			case NAI.TYPE.PUNCH_0:
			case NAI.TYPE.PUNCH_1:
			case NAI.TYPE.PUNCH_2:
				break;
			case NAI.TYPE.MAG:
			case NAI.TYPE.MAG_0:
				return this.runMagBeam(Tk.initProgress(this), Tk);
			default:
				if (type - NAI.TYPE.GAZE <= 1)
				{
					base.AimToLr((X.xors(2) == 0) ? 0 : 2);
					Tk.after_delay = 30f + this.Nai.RANtk(840) * 40f;
					return false;
				}
				break;
			}
			return base.readTicket(Tk);
		}

		private bool fnJumpInit(NaTicket Tk, ref Vector4 VJump)
		{
			if (VJump.w == 0f)
			{
				return true;
			}
			this.EnHead.getPhysic().addFoc(FOCTYPE.JUMP, 0f, VJump.y * 0.75f, -1f, 0, 1, (int)VJump.w, -1, 0);
			return true;
		}

		private bool fnWalking(NaTicket Tk)
		{
			this.walk_time += this.TS;
			if (this.walk_time >= 5f)
			{
				this.walk_time -= 5f;
				this.playSndPos("tentacle_foot", 1);
			}
			return true;
		}

		private bool runMagBeam(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
			}
			return this.t < 30f || this.EnHead.getAI().isFrontType(Tk.type, PROG.ACTIVE);
		}

		private bool runPunch(bool init_flag, NaTicket Tk, bool is_simple)
		{
			if (init_flag)
			{
				this.t = 0f;
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 14, true))
			{
				base.setSkipLift((int)(Tk.depy - 0.05f), false);
				this.FootD.initJump(false, true, false);
				this.SpSetPose("jump", -1, null, false);
				base.PtcVar("by", (double)(base.y + this.sizey * 0.75f)).PtcST("pentapod_jump_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				float num = Tk.depx - base.x + this.Nai.NIRANtk(-0.35f, 0.35f, 3152);
				this.Phy.addFocToSmooth(FOCTYPE.WALK, base.x + X.absMn(num, 0.8f), base.y, 30, 5, 0, -1f);
				float num2 = this.Nai.NIRANtk(-0.14f, -0.16f, 4387);
				this.Phy.addFoc(FOCTYPE.JUMP, 0f, num2, -1f, 0, 1, 29, -1, 0);
				this.EnHead.getPhysic().addFoc(FOCTYPE.JUMP, 0f, num2 * 0.8f, -1f, 0, 1, 29, -1, 0);
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.Phy.addLockGravityFrame(2);
				if (Tk.Progress(ref this.t, 30, true))
				{
					if (is_simple)
					{
						this.SpSetPose("attack0_0", -1, null, false);
						this.Phy.initSoftFall(0.66f, 20f);
						this.AtkTackleP0.is_grab_attack = false;
						MagicItem magicItem = base.tackleInit(this.AtkTackleP0, this.TkAtk, MGHIT.AUTO);
						this.MpConsume(this.McsAtk, magicItem, 1f, 1f);
						Tk.prog = PROG.PROG2;
					}
					else
					{
						this.SpSetPose("attack1_0", -1, null, false);
						base.PtcST("pentapod_atk_open", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				this.Phy.addLockGravityFrame(2);
				if (Tk.Progress(ref this.t, 45, true))
				{
					this.Phy.initSoftFall(0.25f, 1f);
					this.AtkTackleP0.is_grab_attack = true;
					MagicItem magicItem2 = base.tackleInit(this.AtkTackleP0, this.TkAtk, MGHIT.AUTO);
					this.MpConsume(this.McsAtk, magicItem2, 1f, 1f);
				}
			}
			if (Tk.prog == PROG.PROG2 && base.hasFoot())
			{
				this.can_hold_tackle = false;
				this.Phy.quitSoftFall(0f);
				this.SpSetPose("land", -1, null, false);
				this.Nai.delay = 120f;
				return false;
			}
			return true;
		}

		public override void quitTicket(NaTicket Tk)
		{
			this.Anm.timescale = 1f;
			if (Tk != null)
			{
				if (Tk.type == NAI.TYPE.WALK || Tk.type == NAI.TYPE.WALK_TO_WEED)
				{
					this.NasJumper.quitTicket(Tk);
					if (!this.Nai.HasF(NAI.FLAG.POWERED, false))
					{
						this.Nai.delay += this.Nai.NIRANtk(80f, 120f, 3341);
					}
					if (this.SpPoseIs("walk"))
					{
						this.SpSetPose("stand", -1, null, false);
					}
				}
				if (Tk.type == NAI.TYPE.PUNCH_WEED)
				{
					this.Nai.RemF(NAI.FLAG.POWERED);
				}
				if (Tk.type == NAI.TYPE.PUNCH_WEED || Tk.type == NAI.TYPE.PUNCH)
				{
					this.Phy.remFoc(FOCTYPE.JUMP, true);
					this.Phy.remFoc(FOCTYPE.WALK, true);
					if (Tk.type == NAI.TYPE.PUNCH)
					{
						this.Nai.addTypeLock(NAI.TYPE.PUNCH, X.NIXP(160f, 260f));
						this.Nai.addTypeLock(NAI.TYPE.MAG, X.NIXP(200f, 240f));
						this.far_suitlen_count = X.Mx(this.far_suitlen_count, 3 + X.xors(3));
					}
				}
				if (Tk.type == NAI.TYPE.MAG_0 || Tk.type == NAI.TYPE.MAG)
				{
					this.far_suitlen_count = X.Mx(this.far_suitlen_count, 4 + X.xors(8));
					if (!this.Nai.isPrGaraakiState())
					{
						this.Nai.addTypeLock(NAI.TYPE.MAG, X.NIXP(160f, 220f));
						float num = 1f;
						if (this.grabbing_mode)
						{
							num = 0f;
						}
						this.Nai.addTypeLock(NAI.TYPE.PUNCH, X.NIXP(100f, 120f) * num);
					}
					else
					{
						this.Nai.addTypeLock(NAI.TYPE.MAG, X.NIXP(230f, 400f));
					}
					this.Nai.delay += this.Nai.NIRANtk(50f, 80f, 3296);
				}
				if (this.far_suitlen_count == 0 && base.AimPr is PR && (base.AimPr as PR).getAbsorbContainer().current_pose_priority >= 20)
				{
					this.far_suitlen_count = 1;
				}
				this.Nai.suit_distance = ((this.far_suitlen_count > 0) ? (5f * X.NIXP(1.4f, 2f)) : 5f);
				this.Nai.suit_distance_garaaki_ratio = 0.6f;
				if ((this.far_suitlen_count == 0 || this.Nai.isPrGaraakiState()) && this.Nai.RANtk(1394) < 0.4f && !this.Nai.isPrAbsorbed() && (this.Nai.isPrGaraakiState() || !this.Nai.hasTypeLock(NAI.TYPE.PUNCH)))
				{
					this.Nai.suit_distance_garaaki_ratio = 0.04f;
				}
				if (this.far_suitlen_count > 0)
				{
					this.far_suitlen_count--;
				}
				this.Phy.quitSoftFall(0f);
			}
			this.skip_lift_mapy = 0;
			this.can_hold_tackle = false;
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			base.quitTicket(Tk);
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle;
		}

		public override void fineEnlargeScale(float r = -1f, bool set_effect = false, bool resize_moveby = false)
		{
			base.fineEnlargeScale(r, set_effect, resize_moveby);
			this.neck_len = X.NI(3f, 4.3f, this.enlarge_level - 1f);
			if (this.ANeck != null)
			{
				for (int i = 1; i >= 0; i--)
				{
					this.ANeck[i].fineScale();
				}
				this.EnHead.fineEnlargeScale(-1f, false, false);
			}
			if (this.AnmB != null)
			{
				this.AnmB.fineAnimatorOffset(-1f);
			}
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (this.state != NelEnemy.STATE.STAND || this.Absorb != null || Abm.Con.current_pose_priority >= 20)
			{
				return false;
			}
			PR pr = MvTarget as PR;
			if (pr == null || !base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			Abm.kirimomi_release = true;
			this.Phy.killSpeedForce(true, true, true, false, false);
			Abm.get_Gacha().activate(PrGachaItem.TYPE.SEQUENCE, 3, 63U);
			Abm.get_Gacha().SoloPositionPixel = new Vector3(0f, 30f, 0f);
			Abm.target_pose = "absorbed_downb";
			Abm.pose_priority = 20;
			Abm.release_from_publish_count = true;
			Abm.publish_float = true;
			Abm.no_shuffle_aim = true;
			pr.fine_frozen_replace = true;
			Abm.uipicture_fade_key = "torture_backinj";
			this.absorb_pos_fix_maxt = 11;
			this.Phy.addLockWallHitting(this, 140f);
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.canAbsorbContinue() || this.EnHead.getState() != NelEnemy.STATE.STAND)
			{
				return false;
			}
			if (this.Absorb.Con.current_pose_priority > 20)
			{
				return false;
			}
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.Anm.showToFront(true, false);
				this.Absorb.setAbmPose(AbsorbManager.ABM_POSE.DOWN, true);
				this.Absorb.randomisePos(14f / this.Mp.CLENB);
				this.SpSetPose("absorb", -1, null, false);
				this.setAim(CAim.get_aim2(0f, 0f, -this.Absorb.getPublishXD(), (float)(-1 + X.xors(3)), false), false);
				this.walk_st = 0;
				this.walk_time = (float)(20 + X.xors(22));
				if (this.AnmB == null)
				{
					this.AnmB = new EnemyAnimator(this, null, null, false);
					this.FD_fnFineAbsorbFrame = new EnemyAnimator.FnFineFrame(this.fnFineAbsorbFrame);
					this.AnmB.initS(this.Anm);
				}
				this.AnmB.showToFront(false, false);
				this.Anm.fnFineFrame = this.FD_fnFineAbsorbFrame;
				this.AnmB.alpha = 1f;
			}
			this.Phy.addLockGravityFrame(3);
			if (this.walk_st < 100)
			{
				this.walk_time -= this.TS;
				if (this.walk_time <= 0f)
				{
					if (this.walk_st >= 30)
					{
						return false;
					}
					if (this.walk_st == 0)
					{
						this.EnHead.getAI().AimPr = this.Absorb.getTargetMover();
						this.EnHead.getAI().AddTicket(NAI.TYPE.GUARD, 132, true);
					}
					base.PtcST("absorb_atk_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					float num = X.NIXP(4f, 6f) * (float)X.MPFXP();
					float num2 = X.NIXP(20f, 25f);
					if (X.XORSP() < 0.7f)
					{
						pr.TeCon.setQuakeSinH(num, 44, num2, 0f, 0);
						this.TeCon.setQuakeSinH(num * 0.7f, 44, num2, 0f, 0);
					}
					if (this.walk_st % 2 == 0 || !pr.is_alive)
					{
						base.runAbsorb();
						base.applyAbsorbDamageTo(pr, this.AtkAbsorb, false, false, false, 0f, false, null, false, true);
					}
					this.Anm.randomizeFrame(0.5f, 0.5f);
					this.walk_time += (float)(18 + X.xors(9) + ((this.walk_st % 6 == 2) ? 44 : 0));
					this.walk_st++;
				}
			}
			else if (!this.EnHead.getAI().isFrontType(NAI.TYPE.GUARD, PROG.ACTIVE))
			{
				return false;
			}
			return true;
		}

		public bool initHeadAbsorbInjection()
		{
			if (this.state != NelEnemy.STATE.ABSORB || this.walk_st >= 100)
			{
				return false;
			}
			this.Absorb.target_pose = "Inject2_nohat";
			this.Absorb.emstate_attach = UIPictureBase.EMSTATE.PROG0;
			M2Attackable targetMover = this.Absorb.getTargetMover();
			if (targetMover is PR)
			{
				(targetMover as PR).fine_frozen_replace = true;
			}
			targetMover.SpSetPose("Inject2_nohat", -1, null, false);
			this.walk_st = 100;
			this.t = 1f;
			return true;
		}

		private void fnFineAbsorbFrame(EnemyFrameDataBasic nF, PxlFrame F)
		{
			if (this.AnmB != null && this.AnmB.alpha > 0f)
			{
				this.Anm.layer_mask = 1U;
				this.AnmB.fineCurrentFrameData();
			}
		}

		public override float auto_target_priority(M2Mover CalcFrom)
		{
			return 1f;
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			base.addF((NelEnemy.FLAG)327680);
			return base.applyDamage(Atk, force);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			return base.applyHpDamageRatio(Atk) * 0.33f;
		}

		private bool grabbing_mode = true;

		private NelNPentapodHead EnHead;

		public const int DR_NECK_MAX = 2;

		private NelNPentapod.EnemyMeshDrawerPentaNeck[] ANeck;

		public const float NECK_LEN_MIN = 3f;

		public const float NECK_LEN_MAX = 4.3f;

		private const float SUIT_DIST_GARAAKI_DEF = 0.6f;

		private const float SUIT_DIST_GARAAKI_NEAR = 0.04f;

		public float neck_len;

		public float neck_len_cur;

		public const float apply_damage_foot_ratio = 0.33f;

		public const float SUIT_DISTANCE = 5f;

		private const float WALK_XSPEED = 0.066f;

		public const float BEAM_MAXLEN = 13f;

		private int far_suitlen_count;

		public const NAI.TYPE NTYPE_BEAM = NAI.TYPE.MAG;

		public const NAI.TYPE NTYPE_BEAM_IGNOREWALL = NAI.TYPE.MAG_0;

		private EnemyAnimator AnmB;

		private EnemyAnimator.FnFineFrame FD_fnFineAbsorbFrame;

		public NOD.MpConsume McsBeam = NOD.getMpConsume("pentapod_beam");

		private NOD.MpConsume McsAtk = NOD.getMpConsume("pentapod_atk");

		private NOD.TackleInfo TkAtk = NOD.getTackle("pentapod_atk");

		private EnAttackInfo AtkTackleP0 = new EnAttackInfo
		{
			hpdmg0 = 7,
			is_grab_attack = true,
			knockback_len = 0.2f,
			parryable = true
		};

		protected EnAttackInfo AtkAbsorb = new EnAttackInfo
		{
			split_mpdmg = 2,
			attr = MGATTR.GRAB,
			Beto = BetoInfo.Absorbed
		};

		private const int PRI_BEAM = 133;

		private const int PRI_WALK = 5;

		private NASSmallJumper NasJumper;

		private const int ABSORB_PRI = 20;

		public const string pr_pose_injection = "Inject2_nohat";

		private sealed class EnemyMeshDrawerPentaNeck : EnemyMeshDrawer
		{
			public EnemyMeshDrawerPentaNeck(NelNPentapod _MvB, NelNPentapodHead _MvH, PxlImage _MainImg)
				: base(_MvB.Mp)
			{
				this.MvB = _MvB;
				this.MvH = _MvH;
				this.ran0 = X.xors();
				this.MainImg = _MainImg;
				EnemyAttr.initAnimator(this.MvB, this);
				this.MvB.RegisterToTeCon(this, null, null);
				base.prepareMesh(this.MvB.Anm.getMI(), this.TeCon).prepareRendetTicket(null, null, null);
				this.b_dirR = 1.5707964f;
				this.h_dirR = -1.5707964f;
				this.checkframe_on_drawing = false;
				this.fineScale();
			}

			public void fineScale()
			{
			}

			private float headx
			{
				get
				{
					return this.MvH.drawx * this.Mp.rCLEN;
				}
			}

			private float heady
			{
				get
				{
					return this.MvH.drawy * this.Mp.rCLEN;
				}
			}

			private float basex
			{
				get
				{
					return this.MvB.drawx * this.Mp.rCLEN;
				}
			}

			private float basey
			{
				get
				{
					return this.MvB.drawy * this.Mp.rCLEN;
				}
			}

			public override bool checkFrame(float TS, bool force = false)
			{
				base.checkFrame(TS, force);
				Vector3 neckBasePointInCanvas = this.getNeckBasePointInCanvas();
				float num = X.LENGTHXYQ(this.basex, this.basey, this.headx, this.heady);
				float num2 = this.b_dirR + this.MvB.Anm.rotationR;
				float num3 = 1.5707964f + this.MvB.Anm.rotationR + X.COSI(this.Mp.floort + X.RAN(this.ran0, 1008) * 300f, 240f + X.RAN(this.ran0, 1344) * 100f) * 0.04f * 3.1415927f;
				float num4 = this.Mp.GAR(this.headx, this.heady, this.basex, this.basey) + X.COSI(this.Mp.floort + X.RAN(this.ran0, 5615) * 300f, 280f + X.RAN(this.ran0, 2447) * 70f) * 0.09f * 3.1415927f;
				if (num < this.MvB.neck_len)
				{
					float num5 = X.ZPOW(this.MvB.neck_len - num, this.MvB.neck_len);
					num3 += num5 * 0.36f * 3.1415927f * this.MvB.mpf_is_right;
					num4 -= num5 * 0.38f * 3.1415927f * this.MvB.mpf_is_right;
				}
				num2 = X.VALWALK(num2, num3 + neckBasePointInCanvas.z * 0.3f, TS * 0.006f * 3.1415927f);
				this.h_dirR = X.VALWALK(this.h_dirR, num4, TS * 0.006f * 3.1415927f);
				this.b_dirR = num2 - this.MvB.Anm.rotationR;
				this.need_fine_mesh = true;
				return true;
			}

			public Vector3 getNeckBasePointInCanvas()
			{
				PxlFrame currentDrawnFrame = this.MvB.Anm.getCurrentDrawnFrame();
				if (currentDrawnFrame != null)
				{
					PxlLayer layerByName = currentDrawnFrame.getLayerByName("point_hip");
					if (layerByName != null)
					{
						return new Vector3(layerByName.x, layerByName.y, -layerByName.rotR);
					}
				}
				return new Vector3(0f, 155f, 0f);
			}

			public override Matrix4x4 getAfterMultipleMatrix(float scalex, float scaley, bool ignore_rot = false)
			{
				M2PxlAnimatorRT mainAnimator = this.MvB.getAnimator().getMainAnimator();
				return base.getAfterMultipleMatrix(scalex, scaley, ignore_rot) * Matrix4x4.Translate(new Vector3(mainAnimator.offsetPixelX, mainAnimator.offsetPixelY) * 0.015625f);
			}

			public override Vector3 getTransformScale()
			{
				return new Vector3(1f, 1f, 1f);
			}

			public override Vector2 getTransformPosition()
			{
				return this.MvB.Anm.getTransformPosition();
			}

			public override bool noNeedDraw()
			{
				return this.MvB.disappearing || (!this.Mp.M2D.Cam.isCoveringMp(this.basex - this.MvB.sizex - 1.5f, this.basey - this.MvB.sizey - 1.5f, this.basex + this.MvB.sizex + 1.5f, this.basey + this.MvB.sizey + 1.5f, 0f) && !this.Mp.M2D.Cam.isCoveringMp(this.headx - this.MvH.sizex - 1.5f, this.heady - this.MvH.sizey - 1.5f, this.headx + this.MvH.sizex + 1.5f, this.heady + this.MvH.sizey + 1.5f, 0f)) || base.noNeedDraw();
			}

			protected override void redrawBodyMeshInner()
			{
				this.need_fine_mesh = false;
				this.Md.clear(false, false);
				base.BasicColorInit(this.Md);
				this.MvB.Anm.MdUv23(this.Md, 1f, 1f);
				Vector3 neckBasePointInCanvas = this.getNeckBasePointInCanvas();
				Vector2 mapPosInPxlCanvas = this.MvB.getAnimator().getMapPosInPxlCanvas(neckBasePointInCanvas.x, neckBasePointInCanvas.y);
				this.Md.TranslateP((mapPosInPxlCanvas.x - this.basex) * this.Mp.CLEN, -(mapPosInPxlCanvas.y - this.basey) * this.Mp.CLEN, false);
				float num = this.MvB.neck_len * 0.3f;
				float num2 = this.MvB.neck_len * 0.32f;
				if (this.MvB.mpf_is_right < 0f)
				{
					float num3 = num2;
					float num4 = num;
					num = num3;
					num2 = num4;
				}
				for (int i = 0; i < 12; i++)
				{
					this.Md.TriRectBL(i, i + 1, i + 14, i + 13);
				}
				float num5 = this.b_dirR + 1.5707964f;
				float num6 = ((this.MvH.mpf_is_right > 0f) ? 0f : 3.1415927f) + this.MvH.getAnimator().rotationR;
				float num7 = this.h_dirR - 1.5707964f;
				float num8 = 0.078125f * this.MvB.Anm.scaleX;
				float num9 = (this.headx - this.basex) * this.Mp.CLEN * 0.015625f;
				float num10 = -(this.heady - this.basey) * this.Mp.CLEN * 0.015625f;
				float num11 = X.Cos(this.b_dirR);
				float num12 = X.Sin(this.b_dirR);
				float num13 = X.Cos(num6 + 3.1415927f);
				float num14 = X.Sin(num6 + 3.1415927f);
				float num15 = X.Cos(this.h_dirR);
				float num16 = X.Sin(this.h_dirR);
				float y = this.MainImg.RectIUv.y;
				float height = this.MainImg.RectIUv.height;
				for (int j = 0; j < 2; j++)
				{
					float num17 = ((j == 0) ? num : num2) * this.Mp.CLEN * 0.015625f;
					float num18 = (float)X.MPF(j == 0);
					float num19 = X.Cos(num5) * num8 * num18;
					float num20 = X.Sin(num5) * num8 * num18;
					float num21 = num9 + X.Cos(num7) * num8 * num18;
					float num22 = num10 + X.Sin(num7) * num8 * num18;
					float num23 = num19 + num11 * num17;
					float num24 = num20 + num12 * num17;
					float num25 = num21 + num15 * num17 * 0.33f + num13 * num17 * 0.7f;
					float num26 = num22 + num16 * num17 * 0.33f + num14 * num17 * 0.7f;
					float num27 = ((j == 0) ? this.MainImg.RectIUv.xMin : this.MainImg.RectIUv.xMax);
					this.Md.uvRectN(num27, y);
					this.Md.Pos(num19, num20, null);
					for (int k = 1; k < 12; k++)
					{
						float num28 = (float)k * 0.083333336f;
						this.Md.uvRectN(num27, y + height * num28);
						this.Md.Pos(X.BEZIER_I(num19, num23, num25, num21, num28), X.BEZIER_I(num20, num24, num26, num22, num28), null);
					}
					this.Md.uvRectN(num27, y + height);
					this.Md.Pos(num21, num22, null);
				}
				this.Md.allocUv23(0, true);
			}

			private readonly NelNPentapod MvB;

			private readonly NelNPentapodHead MvH;

			private readonly PxlImage MainImg;

			private uint ran0;

			public float b_bz_len = 2.2f;

			public float h_bz_len = 2.2f;

			public float b_dirR;

			public float h_dirR;
		}
	}
}
