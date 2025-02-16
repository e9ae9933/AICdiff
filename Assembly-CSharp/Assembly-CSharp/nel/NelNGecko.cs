using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNGecko : NelEnemy
	{
		public override void appear(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.Ahitwall_lock = new float[4];
			this.kind = ENEMYKIND.DEVIL;
			float num = 9f;
			NOD.BasicData basicData;
			if (this.id == ENEMYID.GECKO_0_FLW)
			{
				this.id = ENEMYID.GECKO_0_FLW;
				basicData = NOD.getBasicData("GECKO_0_FLW");
			}
			else
			{
				this.id = ENEMYID.GECKO_0;
				basicData = NOD.getBasicData("GECKO_0");
			}
			this.auto_rot_on_damage = true;
			base.base_gravity = 0f;
			base.appear(_Mp, basicData);
			this.Anm.checkframe_on_drawing = false;
			this.Nai.busy_consider_intv = 12f;
			this.Nai.awake_length = num;
			this.Nai.attackable_length_x = 7f;
			this.Nai.attackable_length_top = -8f;
			this.Nai.attackable_length_bottom = 7f;
			this.Nai.fnSleepLogic = NAI.FD_SleepOnlyNearMana;
			this.Nai.fnAwakeLogic = new NAI.FnNaiLogic(this.considerNormal);
			this.Nai.fnOverDriveLogic = new NAI.FnNaiLogic(this.considerOverDrive);
			this.absorb_weight = 1;
			this.Nai.consider_only_onfoot = false;
			this.EadStand = new NelNGecko.DrawFrameStand(this.Anm.getCurrentCharacter().getPoseByName("walking").getSequence(2)
				.getFrame(0), this, this.Anm, 6);
			this.Anm.addAdditionalDrawer(this.EadStand);
			this.Anm.addAdditionalListener(this.EadStand);
		}

		public override void fineEnlargeScale(float r = -1f, bool set_effect = false, bool resize_moveby = false)
		{
			base.fineEnlargeScale(-1f, set_effect, resize_moveby);
			this.walk_spd = base.NIel(0.18f, 0.1f);
			this.escape_spd = base.NIel(0.32f, 0.22f);
			this.normaldamage_stun_t = base.NIel(1f, 1.6f) * this.knockback_time0;
			this.wind_radius = base.NIel(1.4f, 2.2f);
			this.wind_release_t = base.NIel(130f, 290f);
			this.grab_jump_delay = base.NIel(105f, 55f);
		}

		public bool isAttackableGrab()
		{
			return this.Nai.isAttackableLength(9f, -6f, 6f, false);
		}

		public override void quitSummonAndAppear(bool clearlock_on_summon = true)
		{
			base.quitSummonAndAppear(clearlock_on_summon);
			this.angleR = X.XORSPS() * 3.1415927f;
			this.EadStand.fixAngleR(3.1415927f * X.NIXP(0.012f, 0.044f) * (float)X.MPFXP());
		}

		private bool considerNormal(NAI Nai)
		{
			if (Nai.fnAwakeBasicHead(Nai))
			{
				return true;
			}
			Vector2 vector = new Vector2(-1f, -1f);
			if (Nai.HasF(NAI.FLAG.ABSORB_FINISHED, true))
			{
				Nai.addTypeLock(NAI.TYPE.PUNCH_0, 180f);
				Nai.addTypeLock(NAI.TYPE.PUNCH, 180f);
				Nai.addTypeLock(NAI.TYPE.MAG, 180f);
			}
			if (Nai.isPrGaraakiState())
			{
				if (vector.y < 0f)
				{
					this.GrabInitable(out vector, null);
				}
				if (vector.y > 0f)
				{
					Nai.AddTicket(NAI.TYPE.PUNCH_0, 128, true).Dep(vector, null);
					return true;
				}
			}
			if (Nai.RANtk(1381) < 0.5f && !Nai.hasPT(20, false) && !Nai.hasTypeLock(NAI.TYPE.PUNCH))
			{
				float num = Nai.NIRANtk(1f, 1.5f, 3913);
				if (Nai.isAttackableLength(Nai.attackable_length_x * num, Nai.attackable_length_top * num, Nai.attackable_length_bottom * num, false) && this.SpinInitable(true))
				{
					return Nai.AddTicketB(NAI.TYPE.PUNCH, 128, true);
				}
			}
			if (!Nai.isPrAbsorbed())
			{
				if (!Nai.hasPT(20, false) && Nai.RANtk(4024) < 0.3f)
				{
					if (vector.y < 0f)
					{
						this.GrabInitable(out vector, null);
					}
					if (vector.y > 0f)
					{
						Nai.AddTicket(NAI.TYPE.PUNCH_0, 20, true).Dep(vector, null);
						return true;
					}
				}
				if (base.Useable(this.McsWind, 1f, 0f) && !Nai.hasTypeLock(NAI.TYPE.MAG))
				{
					if (Nai.HasF(NAI.FLAG.BOTHERED, true))
					{
						if (vector.y < 0f)
						{
							this.GrabInitable(out vector, null);
						}
						if (vector.y > 0f)
						{
							Nai.AddTicket(NAI.TYPE.PUNCH_0, 20, true).Dep(vector, null);
							return true;
						}
						if (this.WindInitable())
						{
							return Nai.AddTicketB(NAI.TYPE.MAG, 128, true);
						}
					}
					else if (this.WindInitable() && Nai.fnBasicMagic(Nai, (Nai.RANtk(938) < 0.6f) ? 128 : 10, 70f, 0f, 0f, 0f, 7145, false))
					{
						return true;
					}
				}
				if (!Nai.hasTypeLock(NAI.TYPE.BACKSTEP) && ((Nai.autotargetted_me && Nai.RANtk(3228) < 0.4f) || Nai.isPrMagicExploded(1f) || Nai.isPrSpecialAttacking() || (Nai.isPrAttacking() && Nai.target_slen <= 2.8f)))
				{
					return Nai.AddTicketB(NAI.TYPE.BACKSTEP, 128, true);
				}
			}
			return Nai.AddTicketB(NAI.TYPE.WALK, 10, true);
		}

		private bool considerOverDrive(NAI Nai)
		{
			return true;
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			for (int i = 3; i >= 0; i--)
			{
				float num = this.Ahitwall_lock[i];
				if (num > 0f)
				{
					this.Ahitwall_lock[i] = base.VALWALK(num, 0f, 1f);
				}
			}
		}

		public override NelEnemy changeState(NelEnemy.STATE stt)
		{
			NelEnemy.STATE state = this.state;
			base.changeState(stt);
			if (stt == NelEnemy.STATE.STAND && !this.Ser.has(SER.EATEN))
			{
				base.base_gravity = 0f;
				this.SpSetPose("stand", -1, null, false);
				this.t = 1f;
			}
			return this;
		}

		public override bool readTicket(NaTicket Tk)
		{
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.MAG)
			{
				switch (type)
				{
				case NAI.TYPE.WALK:
					return this.runWalk(Tk.initProgress(this), Tk);
				case NAI.TYPE.WALK_TO_WEED:
					break;
				case NAI.TYPE.PUNCH:
					return this.runPunch_Spin(Tk.initProgress(this), Tk);
				case NAI.TYPE.PUNCH_0:
					return this.runPunch0_Grab(Tk.initProgress(this), Tk);
				default:
					if (type == NAI.TYPE.MAG)
					{
						return this.runMag_Wind(Tk.initProgress(this), Tk);
					}
					break;
				}
			}
			else
			{
				if (type == NAI.TYPE.BACKSTEP)
				{
					return this.runBackStep(Tk.initProgress(this), Tk);
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

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null && (Tk.type == NAI.TYPE.PUNCH_0 || Tk.type == NAI.TYPE.PUNCH))
			{
				this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 40f);
				this.Nai.addTypeLock(NAI.TYPE.PUNCH, 240f);
			}
			this.can_hold_tackle = false;
			base.remF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
			this.WindHn.destruct();
			base.quitTicket(Tk);
		}

		public bool runWalk(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("stand", -1, null, false);
				base.base_gravity = 0f;
				this.walk_st = 0;
				if (!this.Nai.isAttackableLength(false))
				{
					this.walk_time = X.correctangleR(this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y) + X.XORSPSam(0.25f) * 3.1415927f * 0.23f);
				}
				else
				{
					this.walk_time = X.XORSPS() * 3.1415927f;
				}
				this.angle_shift = (float)X.MPFXP();
			}
			float num = (float)((base.hit_wall_ != (HITWALL)0) ? 2 : 1);
			this.progressWalk(this.walk_spd, this.walk_time, 0.10367256f, num);
			if (Tk.prog == PROG.ACTIVE)
			{
				if (base.hit_wall_ == (HITWALL)0)
				{
					this.walk_time += this.angle_shift * 0.004f * 3.1415927f * this.TS;
				}
				if (this.t >= this.Nai.NIRANtk(120f, 170f, 8821))
				{
					Tk.AfterDelay(X.NIXP(30f, 80f));
					return false;
				}
				int num2 = this.calcDangerHitWall((int)base.hit_wall_);
				int num3 = 0;
				float num4 = X.correctangleR(this.angleR);
				for (int i = 0; i < 4; i++)
				{
					if ((num2 & (1 << i)) != 0)
					{
						if (++num3 >= 2)
						{
							Tk.prog = PROG.PROG0;
							this.t = 0f;
							this.getBackAgR(num4, out this.walk_time, true, true);
							this.walk_st = ~num2 & 15;
							break;
						}
						Vector2 vector;
						if (this.Nai.RANtk(4122 + i * 200) < 0.4f && this.GrabInitable(out vector, null))
						{
							Tk.Recreate(NAI.TYPE.PUNCH_0, 20, true, null);
							Tk.Dep(vector, null);
							return true;
						}
						float num5 = this.Nai.NIRANtk(0.2f, 0.4f, 8811);
						switch (i)
						{
						case 0:
							this.walk_time = this.angleR + ((num4 > 0f) ? (-3.1415927f) : 3.1415927f) * num5;
							break;
						case 1:
							this.walk_time = this.angleR + ((num4 > 1.5707964f || num4 < -1.5707964f) ? 3.1415927f : (-3.1415927f)) * num5;
							break;
						case 2:
							this.walk_time = this.angleR + ((num4 > 0f) ? 3.1415927f : (-3.1415927f)) * num5;
							break;
						case 3:
							this.walk_time = this.angleR + ((num4 > 1.5707964f || num4 < -1.5707964f) ? (-3.1415927f) : 3.1415927f) * num5;
							break;
						}
						if ((this.walk_st & (1 << i)) == 0)
						{
							this.walk_st |= 1 << i;
							this.angle_shift = (float)X.MPF(X.XORSP() < 0.5f);
						}
					}
				}
				this.walk_time = X.correctangleR(this.walk_time);
			}
			if (Tk.prog == PROG.PROG0 && ((this.walk_st & (int)base.hit_wall_) != 0 || Tk.Progress(ref this.t, (int)this.Nai.NIRANtk(100f, 160f, 4812), true)))
			{
				this.t = 0f;
				Tk.prog = PROG.ACTIVE;
				this.walk_st = 0;
				this.walk_time = X.XORSPS() * 3.1415927f;
				this.angle_shift = (float)X.MPF(X.XORSP() < 0.5f);
			}
			return true;
		}

		public bool WindInitable()
		{
			if (this.Nai.hasPT(20, false))
			{
				return false;
			}
			Vector3 vector = this.Mp.checkThroughBccNearestHitPos(base.x, base.y, this.Nai.target_x, this.Nai.target_y, this.wind_radius * 1.12f, this.wind_radius * 1.12f, -1, false, false, null, true);
			return vector.z < 0f || vector.z >= X.LENGTHXY2(base.x, base.y, this.Nai.target_x, this.Nai.target_y) - X.Pow2(this.wind_radius);
		}

		public override float getWindApplyLevel(WindItem Wind)
		{
			if (this.WindHn.isActive(Wind))
			{
				return 0f;
			}
			return base.getWindApplyLevel(Wind) * ((this.state == NelEnemy.STATE.STAND) ? 0.5f : 1f);
		}

		public override void positionChanged(float prex, float prey)
		{
			base.positionChanged(prex, prey);
			if (this.WindHn.isActive())
			{
				this.WindHn.Wind.setPos(base.x, base.y);
			}
		}

		public bool runMag_Wind(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
				base.addF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT);
				this.SpSetPose("act0", -1, null, false);
				this.playSndPos("gecko_wind_init", 1);
				this.walk_time = this.Mp.GAR(base.x, base.y, this.Nai.target_x + 0.7f * X.XORSPS(), this.Nai.target_y + 1.3f * X.XORSPS());
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				this.angleR = base.VALWALKANGLER(this.angleR, this.walk_time, X.NI(0.01f, 0.09f, X.ZPOW(this.t, 32f)) * 3.1415927f);
				if (Tk.Progress(ref this.t, 40, true))
				{
					this.angleR = this.walk_time;
					this.playSndPos("gecko_wind", 1);
					this.WindHn = base.nM2D.WIND.Add(base.x, base.y, this.wind_radius, this.walk_time, 7f, 0.22f, this.wind_release_t, 3f);
				}
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, (base.Useable(this.McsWind, 1f, 0f) ? ((int)this.wind_release_t) : 0) + 30, true))
			{
				this.WindHn.destruct();
				this.SpSetPose("act1", -1, null, false);
				this.angleR = 0f;
				base.base_gravity = 0.66f;
				base.MpConsume(this.McsWind, null, 1f, 1f);
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (base.hasFoot())
				{
					this.angleR = 0f;
					if (!this.Anm.poseIs("act2", true))
					{
						this.SpSetPose("act2", -1, null, false);
						this.playSndPos("gecko_foot", 1);
					}
					if (Tk.Progress(ref this.t, 20, true))
					{
						this.Nai.addTypeLock(NAI.TYPE.MAG, 80f);
						return false;
					}
				}
				else
				{
					this.SpSetPose("act1", -1, null, false);
					this.t = 0f;
				}
			}
			return true;
		}

		private bool SpinInitable(bool nearfoot_check = true)
		{
			if (base.base_gravity != 0f || this.Nai.isAttackableLength(2.5f, -1.4f - this.sizey, 1.4f + this.sizey, false))
			{
				return false;
			}
			if (X.Abs(X.angledifR(this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y), -1.5707964f)) < 0.40840703f)
			{
				return false;
			}
			if (nearfoot_check)
			{
				Vector3 vector = this.Mp.checkThroughBccNearestHitPos(base.x, base.y, this.Nai.target_x, this.Nai.target_y, 1.2f, 1.2f, -1, false, false, null, true);
				if (vector.z >= 0f && vector.z < X.LENGTHXY2(base.x, base.y, this.Nai.target_x, this.Nai.target_y) - 2f)
				{
					return false;
				}
				if (!this.Nai.isPrGaraakiState() && this.Nai.RANtk(3811) < 0.78f)
				{
					M2BlockColliderContainer.BCCLine targetLastBcc = this.Nai.TargetLastBcc;
					if (targetLastBcc != null && !targetLastBcc.is_lift && X.Abs(this.Nai.target_y - this.Nai.target_lastfoot_bottom) < this.Nai.sizey + 1.4f)
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool runPunch_Spin(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("prepare0", -1, null, false);
				this.Anm.setAim(AIM.R, 1);
				base.base_gravity = 0f;
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
				Tk.priority = 128;
				base.PtcST("gecho_spin_prepare", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				float num = this.Mp.GAR(base.x, base.y, this.Nai.target_x - this.Nai.target_sizex * this.Nai.RANStk(4315), this.Nai.target_y - this.Nai.target_sizey * this.Nai.RANStk(3199));
				if (Tk.Progress(ref this.t, 50, true))
				{
					if (!this.SpinInitable(false))
					{
						Tk.Recreate(NAI.TYPE.WALK, 10, true, null);
						return true;
					}
					this.PtcHld.killPtc("gecho_spin_prepare", false);
					this.playSndPos("gecko_spin_act", 1);
					this.SpSetPose("rotatk0", -1, null, false);
					this.angleR = base.VALWALKANGLER(this.angleR, num, 0.47123894f);
					this.t = 0f;
					this.walk_st = 0;
				}
				else
				{
					this.angleR = base.VALWALKANGLER(this.angleR, num, 0.025132744f);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_st == 0 && this.t >= 17f)
				{
					this.walk_st = 1;
					base.tackleInit(this.AtkSpin, this.TkSpin);
					base.PtcVar("scl", (double)this.enlarge_level).PtcST("gecho_spin_attack", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				}
				float num2 = X.NI(0, 1, X.ZPOW(this.t, 16f)) * 0.16f;
				float num3 = num2 * X.Cos(this.angleR);
				float num4 = -num2 * X.Sin(this.angleR);
				bool flag = false;
				M2BlockColliderContainer.BCCLine bccline;
				float num5;
				if (base.vx != 0f && !base.canGoToSideLB(out bccline, out num5, (base.vx > 0f) ? AIM.R : AIM.L, num2 * 1.5f, -0.1f, false, true, false))
				{
					num3 = -num3;
					flag = true;
				}
				int num6 = (base.Useable(this.TkSpin, 1f) ? 140 : 45);
				if (base.vy != 0f && !base.canGoToSideLB(out bccline, out num5, (base.vy > 0f) ? AIM.B : AIM.T, num2 * 1.5f, -0.1f, false, true, false))
				{
					if (base.vy > 0f)
					{
						this.t = 0f;
						this.walk_st = 0;
						this.setAim((num3 > 0f) ? AIM.R : AIM.L, false);
						this.PtcHld.killPtc("gecho_spin_attack", false);
						if (!base.Useable(this.TkSpin, 1f) || this.t < (float)(num6 - 20))
						{
							Tk.prog = PROG.PROG2;
							this.SpSetPose("stun", -1, null, false);
						}
						else
						{
							Tk.prog = PROG.PROG1;
							this.SpSetPose("act2", -1, null, false);
						}
						return true;
					}
					num4 = -num4;
					flag = true;
				}
				if (flag)
				{
					this.angleR = this.Mp.GAR(0f, 0f, num3, num4);
				}
				this.Phy.addFoc(FOCTYPE.WALK, num3, num4, -1f, -1, 1, 0, -1, 0);
				this.AtkSpin.BurstDir(num3 > 0f);
				if (Tk.Progress(ref this.t, num6, true))
				{
					this.PtcHld.killPtc("gecho_spin_attack", false);
					this.walk_st = 0;
					this.setAim((num3 > 0f) ? AIM.R : AIM.L, false);
					if (base.Useable(this.TkSpin, 1f))
					{
						Tk.prog = PROG.PROG1;
						this.SpSetPose("act1", -1, null, false);
					}
					else
					{
						Tk.prog = PROG.PROG2;
						this.SpSetPose("stun", -1, null, false);
					}
				}
			}
			if (Tk.prog >= PROG.PROG1)
			{
				base.base_gravity = 0.66f;
				this.can_hold_tackle = false;
				this.angleR = 0f;
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (base.hasFoot())
				{
					this.SpSetPose("act2", -1, null, false);
					if (Tk.Progress(ref this.t, 80, true))
					{
						return false;
					}
				}
				else
				{
					this.t = 0f;
				}
			}
			if (Tk.prog == PROG.PROG2)
			{
				if (this.walk_st == 0)
				{
					if (base.hasFoot())
					{
						if (this.t >= 180f)
						{
							this.walk_st = 1;
							this.t = 0f;
							this.SpSetPose("wakeup", -1, null, false);
						}
					}
					else
					{
						this.t = 0f;
					}
				}
				else if (this.t >= 25f)
				{
					return false;
				}
			}
			return true;
		}

		private bool GrabInitable(out Vector2 Dep, M2BlockColliderContainer.BCCLine Bcc = null)
		{
			Dep = Vector2.zero;
			if (this.Nai.isPrAbsorbed() || this.Nai.hasTypeLock(NAI.TYPE.PUNCH_0) || this.Nai.hasPT(20, false) || !this.isAttackableGrab())
			{
				return false;
			}
			bool flag = false;
			if (Bcc == null)
			{
				M2BlockColliderContainer.BCCLine targetLastBcc = this.Nai.TargetLastBcc;
				if (targetLastBcc.is_lift && !targetLastBcc.is_ladder)
				{
					Bcc = targetLastBcc;
				}
			}
			int num = (int)base.x;
			if (Bcc == null)
			{
				Bcc = this.Mp.getSideBcc(num, (int)base.y, AIM.B);
			}
			if (Bcc != null && X.BTW(Bcc.shifted_x - this.sizex * 0.5f, base.x, Bcc.shifted_right + this.sizex * 0.5f))
			{
				int num2 = (int)Bcc.slopeBottomY(base.x);
				if ((float)num2 < base.mbottom + 6f && (float)num2 >= base.mbottom - 2f && X.BTW((float)num2 - 4.6f, this.Nai.target_y, (float)(num2 + 1)))
				{
					M2BlockColliderContainer.BCCPos footStampChip = Bcc.getFootStampChip(0, base.x, (float)num2, 0f, 0f);
					if (!footStampChip.valid || !CCON.isDangerous(footStampChip.cfg))
					{
						flag = true;
						Dep.Set(base.x, (float)num2);
						for (int i = (int)base.mbottom; i <= num2; i++)
						{
							if (CCON.isDangerous(this.Mp.getConfig(num, i)))
							{
								flag = false;
								break;
							}
						}
					}
				}
			}
			if (!flag)
			{
				this.Nai.addTypeLock(NAI.TYPE.PUNCH_0, 30f);
			}
			return flag;
		}

		private void progressWalk(float spd, float dep_agR, float rotspdR, float scale)
		{
			this.angleR = base.VALWALKANGLER(this.angleR, this.walk_time, rotspdR * scale);
			this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL, spd * X.Cos(this.angleR), -spd * X.Sin(this.angleR), -2f, -1, 1, 0, -1, 0);
			this.EadStand.addTime(scale);
		}

		private bool getBackAgR(float angleR0, out float ret, bool calc_x = true, bool calc_y = true)
		{
			if (!calc_x && !calc_y)
			{
				ret = angleR0 + (float)X.MPFXP() * X.NIXP(0.23f, 0.79f) * 3.1415927f;
				return true;
			}
			bool flag = X.Cos(angleR0) > 0f;
			bool flag2 = X.Sin(angleR0) > 0f;
			int num = 12;
			float num2 = ((calc_x && calc_y) ? 0.5f : 0.15f);
			while (--num >= 0)
			{
				float num3 = angleR0 + num2 * 3.1415927f;
				if ((!calc_x || flag != X.Cos(num3) > 0f) && (!calc_y || flag2 != X.Sin(num3) > 0f))
				{
					ret = num3;
					return true;
				}
				float num4 = angleR0 - num2 * 3.1415927f;
				if ((!calc_x || flag != X.Cos(num4) > 0f) && (!calc_y || flag2 != X.Sin(num4) > 0f))
				{
					ret = num4;
					return true;
				}
				num2 += 0.1f;
			}
			ret = angleR0 + 3.1415927f;
			return false;
		}

		public bool runBackStep(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.SpSetPose("stand", -1, null, false);
				base.base_gravity = 0f;
				this.walk_st = (int)this.Nai.NIRANtk(100f, 220f, 4911);
				float num = this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y);
				float num2 = num + 1.5707964f + X.XORSPS() * 0.14f * 3.1415927f;
				float num3 = num - 1.5707964f + X.XORSPS() * 0.14f * 3.1415927f;
				float num4 = base.x + X.Cos(num2) * 4f;
				float num5 = base.y - X.Sin(num2) * 4f;
				float num6 = base.x + X.Cos(num3) * 4f;
				float num7 = base.y - X.Sin(num3) * 4f;
				bool flag = this.Mp.canThroughBcc(base.x, base.y, num4, num5, this.sizey, this.sizey, -1, false, false, null, true, null);
				bool flag2 = this.Mp.canThroughBcc(base.x, base.y, num6, num7, this.sizey, this.sizey, -1, false, false, null, true, null);
				if (!flag && !flag2)
				{
					this.walk_time = this.angleR;
				}
				else if (!flag)
				{
					this.walk_time = num3;
				}
				else if (!flag2)
				{
					this.walk_time = num2;
				}
				else
				{
					this.walk_time = ((X.LENGTHXY2(this.Nai.target_x, this.Nai.target_y, num4, num5) < X.LENGTHXY2(this.Nai.target_x, this.Nai.target_y, num6, num7)) ? num3 : num2);
				}
				this.walk_time = X.correctangleR(this.walk_time);
				this.angle_shift = (float)X.MPF(X.angledif(this.walk_time, num) < 0f);
			}
			this.progressWalk(this.escape_spd, this.walk_time, 0.10367256f, 2f);
			int num8 = this.calcDangerHitWall(this.calced_hitwall);
			if (!init_flag && num8 != 0 && !this.hitwall_lock_exists)
			{
				this.getBackAgR(this.angleR, out this.walk_time, (num8 & 5) != 0, (num8 & 10) != 0);
				this.lockHitWall(num8);
				this.walk_time = X.correctangleR(this.walk_time);
				this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y);
				this.angle_shift = 0f;
			}
			if (this.angle_shift != 0f)
			{
				this.walk_time += this.angle_shift * 0.019f * 3.1415927f * this.TS;
				this.angle_shift = base.VALWALK(this.angle_shift, 0f, 0.056548666f);
			}
			if (this.t >= (float)this.walk_st)
			{
				this.Nai.addTypeLock(NAI.TYPE.BACKSTEP, 120f);
				if (this.Nai.RANtk(192) < 0.4f)
				{
					this.Nai.AddF(NAI.FLAG.BOTHERED, 100f);
				}
				return false;
			}
			return true;
		}

		public bool runPunch0_Grab(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.angleR = 0f;
				base.base_gravity = 0.66f;
				base.addF((NelEnemy.FLAG)192);
				base.addF(NelEnemy.FLAG.DECLINE_ENLARGE_CHECKING);
				this.playSndPos("gecko_falling", 1);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (base.hasFoot())
				{
					base.AimToPlayer();
					this.SpSetPose("jumpatk0", -1, null, false);
					Tk.prog = PROG.PROG0;
					this.t = 0f;
					this.playSndPos("gecko_foot", 1);
				}
				else
				{
					this.SpSetPose("rotatk0", -1, null, false);
					this.t = 0f;
				}
			}
			if (Tk.prog == PROG.PROG0 && Tk.Progress(ref this.t, (int)(((!base.Useable(this.TkGrab, 1f)) ? 2.2f : 1f) * this.grab_jump_delay), base.hasFoot()))
			{
				Tk.priority = 128;
				float num = X.MMX(2f, base.mbottom - this.Nai.target_y, 4.6f);
				float num2 = X.Mn(X.Abs(this.Nai.target_x - base.x), 6f);
				if (!base.Useable(this.TkGrab, 1f))
				{
					num *= 0.5f;
					num2 *= 0.5f;
				}
				base.PtcST("gecko_grab_jump", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.NO_FOLLOW);
				this.jumpInit(num2 * 1.2f * base.mpf_is_right, 0f, num, true);
				base.tackleInit(this.AtkGrab, this.TkGrab);
				this.SpSetPose("jumpatk1", -1, null, false);
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (base.hasFoot())
				{
					this.can_hold_tackle = false;
					this.SpSetPose("act2", -1, null, false);
					if (Tk.Progress(ref this.t, 100, true))
					{
						return false;
					}
				}
				else
				{
					this.t = 0f;
				}
			}
			return true;
		}

		public override bool canHoldMagic(MagicItem Mg)
		{
			return this.Nai != null && this.is_alive && Mg.kind == MGKIND.TACKLE && this.canAbsorbContinue() && this.can_hold_tackle;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			A.Add("torture_romero");
		}

		public override bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvTarget = null, AbsorbManager Abm = null, bool penetrate = false)
		{
			if (this.state != NelEnemy.STATE.STAND || this.Absorb != null || Abm.Con.use_torture)
			{
				return false;
			}
			if (!base.initAbsorb(Atk, MvTarget, Abm, penetrate))
			{
				return false;
			}
			this.walk_st = (int)X.NIXP(10f, 15f);
			this.walk_time = 0f;
			Abm.get_Gacha().activate(PrGachaItem.TYPE.SEQUENCE, 3, 15U);
			Abm.kirimomi_release = true;
			Abm.release_from_publish_count = true;
			Abm.no_clamp_speed = true;
			this.Absorb.changeTorturePose("torture_romero_0", false, false, -1, -1);
			Abm.get_Gacha().SoloPositionPixel = new Vector3(0f, -192f, 0f);
			return true;
		}

		public override bool runAbsorb()
		{
			PR pr = base.AimPr as PR;
			if (pr == null || !this.Absorb.isActive(pr, this, true) || !this.Absorb.checkTargetAndLength(pr, 3f) || !this.canAbsorbContinue())
			{
				return false;
			}
			if (this.walk_st < 100 && this.t >= 40f)
			{
				float num = this.walk_time + 1f;
				this.walk_time = num;
				if (num >= (float)this.walk_st)
				{
					this.walk_st = 100;
					this.playSndPos("gecko_rotate", 1);
					this.Absorb.changeTorturePose("torture_romero_1", false, false, -1, -1);
				}
				else
				{
					base.applyAbsorbDamageTo(pr, this.AtkAbsorbGrab0, X.XORSP() < 0.8f, X.XORSP() < 0.7f, false, 0f, false, null, false);
					this.t = 40f - X.NIXP(20f, 30f);
				}
			}
			if (this.walk_st == 100 && this.Anm.isAnimEnd())
			{
				this.Absorb.uipicture_fade_key = "torture_romero";
				this.Absorb.changeTorturePose("torture_romero_2", false, false, -1, -1);
				this.t = 200f;
				this.walk_st = 101;
			}
			if (this.walk_st == 101 && this.t >= 100f)
			{
				this.playSndPos("gecko_grab_dmg", 1);
				base.applyAbsorbDamageTo(pr, this.AtkAbsorbGrabRomero, true, false, true, 0f, false, null, false);
				pr.UP.setFade(this.Absorb.uipicture_fade_key, UIPictureBase.EMSTATE.NORMAL, false, false, false);
				this.t = 100f - X.NIXP(35f, 40f);
				this.Anm.animReset(0, false);
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
			if (MDI.kind == MAPDMG.SPIKE && base.base_gravity == 0f)
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

		public override bool runDamageSmall()
		{
			if (this.t <= 0f)
			{
				base.remF(NelEnemy.FLAG.NO_AUTO_LAND_EFFECT);
				this.t = 0f;
				this.SpSetPose("damage", 1, null, false);
				base.base_gravity = 0.66f;
			}
			base.runDamageSmall();
			if (base.hasFoot())
			{
				if (this.Anm.poseIs("wakeup", true))
				{
					if (this.t >= 40f)
					{
						this.SpSetPose("walking", -1, null, false);
						this.angleR = ((base.mpf_is_right > 0f) ? 0f : (-3.1415927f));
						return false;
					}
				}
				else if (this.Anm.poseIs("stun", true))
				{
					if (this.Ser.has(SER.EATEN))
					{
						return false;
					}
					if (this.t >= this.normaldamage_stun_t)
					{
						this.SpSetPose("wakeup", -1, null, false);
						this.t = 1f;
					}
				}
				else
				{
					this.Anm.rotationR = 0f;
					this.Anm.rotationR_speed = 0f;
					this.SpSetPose("stun", -1, null, false);
				}
			}
			else
			{
				this.t = 1f;
				this.SpSetPose("damage", -1, null, false);
			}
			return true;
		}

		public override bool runStun()
		{
			if (this.t <= 0f)
			{
				this.t = 0f;
				this.SpSetPose("stun", 1, null, false);
				base.base_gravity = 0.66f;
			}
			return base.runStun();
		}

		public float angleR
		{
			get
			{
				return this.Anm.rotationR;
			}
			set
			{
				this.Anm.rotationR = value;
			}
		}

		public int calced_hitwall
		{
			get
			{
				int num = 0;
				int hit_wall_ = (int)base.hit_wall_;
				for (int i = 0; i < 4; i++)
				{
					if ((hit_wall_ & (1 << i)) != 0 && this.Ahitwall_lock[i] == 0f)
					{
						num |= 1 << i;
					}
				}
				return num;
			}
		}

		public int calcDangerHitWall(int hw)
		{
			hw &= 15;
			bool flag = this.Mp.isDangerousCfg((int)base.x, (int)base.y);
			bool flag2 = CCON.isWater(this.Mp.getConfig((int)base.x, (int)base.y));
			if (flag && flag2)
			{
				return hw;
			}
			for (int i = 0; i < 4; i++)
			{
				if ((hw & (1 << i)) == 0)
				{
					int num;
					int num2;
					switch (i)
					{
					case 0:
						num = (int)(this.Phy.rgdleft + 0.5f);
						num2 = (int)base.y;
						break;
					case 1:
						num2 = (int)(this.Phy.rgdtop + 0.5f);
						num = (int)base.x;
						break;
					case 2:
						num = (int)(this.Phy.rgdright - 0.5f);
						num2 = (int)base.y;
						break;
					default:
						num2 = (int)(this.Phy.rgdbottom - 0.5f);
						num = (int)base.x;
						break;
					}
					for (int j = 0; j < 2; j++)
					{
						if (!flag && this.Mp.isDangerousCfg(num, num2))
						{
							hw |= 1 << i;
							break;
						}
						if (!flag2 && CCON.isWater(this.Mp.getConfig(num, num2)))
						{
							hw |= 1 << i;
							break;
						}
						num += CAim._XD(j, 1);
						num2 -= CAim._YD(j, 1);
					}
				}
			}
			return hw;
		}

		public bool hitwall_lock_exists
		{
			get
			{
				for (int i = 0; i < 4; i++)
				{
					if (this.Ahitwall_lock[i] != 0f)
					{
						return true;
					}
				}
				return false;
			}
		}

		public void lockHitWall(int bits)
		{
			for (int i = 0; i < 4; i++)
			{
				if ((bits & (1 << i)) != 0)
				{
					this.Ahitwall_lock[i] = 15f;
				}
			}
		}

		private NelNGecko.DrawFrameStand EadStand;

		private float angle_shift;

		private float walk_spd;

		private float escape_spd;

		private float normaldamage_stun_t;

		private float wind_radius;

		private float wind_release_t;

		private float grab_jump_delay;

		private const float _gravity = 0.66f;

		private const float wind_apply_velocity = 0.22f;

		private const float spin_speed = 0.16f;

		private WindHandlerItem WindHn;

		private float[] Ahitwall_lock;

		private const float grab_jump_delay_min = 105f;

		private const float grab_jump_delay_max = 55f;

		private const float grab_jump_xrange = 6f;

		private const float grab_jump_high = 4.6f;

		private NOD.MpConsume McsWind = NOD.getMpConsume("gecko_wind");

		private NOD.TackleInfo TkGrab = NOD.getTackle("gecko_grab");

		private NOD.TackleInfo TkSpin = NOD.getTackle("gecko_spin");

		private const NAI.TYPE NT_GRAB = NAI.TYPE.PUNCH_0;

		private const NAI.TYPE NT_SPIN = NAI.TYPE.PUNCH;

		private const NAI.TYPE NT_WIND = NAI.TYPE.MAG;

		protected NelAttackInfo AtkGrab = new NelAttackInfo
		{
			hpdmg0 = 7,
			attr = MGATTR.GRAB,
			is_penetrate_grab_attack = true,
			knockback_len = 0.2f,
			parryable = true
		};

		protected NelAttackInfo AtkAbsorbGrab0 = new NelAttackInfo
		{
			split_mpdmg = 1,
			hpdmg0 = 4,
			attr = MGATTR.GRAB,
			Beto = BetoInfo.Grab,
			huttobi_ratio = -1000f
		}.Torn(0.02f, 0.025f);

		protected NelAttackInfo AtkAbsorbGrabRomero = new NelAttackInfo
		{
			split_mpdmg = 1,
			hpdmg0 = 7,
			pee_apply100 = 3f,
			huttobi_ratio = -1000f
		}.Torn(0.02f, 0.025f);

		protected NelAttackInfo AtkSpin = new NelAttackInfo
		{
			hpdmg0 = 7,
			split_mpdmg = 8,
			attr = MGATTR.WIP,
			parryable = true,
			burst_vx = 0.14f,
			huttobi_ratio = 0.03f,
			Beto = BetoInfo.DarkTornado
		};

		private const int PRI_ESCAPE = 128;

		private const int PRI_WALK = 10;

		private const int PRI_ATK = 20;

		protected class DrawFrameStand : EnemyAnimator.EnemyAdditionalDrawFrame, EnemyAnimator.IEnemyAnimListener
		{
			public DrawFrameStand(PxlFrame _F, NelNGecko _En, EnemyAnimator _Anm, int resolution = 6)
				: base(null, null, false)
			{
				this.En = _En;
				this.Anm = _Anm;
				this.Sq = _F.pSq;
				this.fnDraw = new EnemyAnimator.EnemyAdditionalDrawFrame.FnDrawEAD(this.fnDrawStand);
				this.AagR = new float[resolution];
				this.AagR_dif = new float[resolution];
			}

			public void fixAngleR(float dif_t = 0f)
			{
				int num = this.AagR.Length;
				float num2 = 0f;
				for (int i = 0; i < num; i++)
				{
					num2 += dif_t;
					this.AagR[i] = this.En.angleR + num2;
					this.AagR_dif[i] = num2;
				}
			}

			public Map2d Mp
			{
				get
				{
					return this.En.Mp;
				}
			}

			public override bool active
			{
				get
				{
					if (!this.Anm.poseIs("stand", true))
					{
						if (this.F != null)
						{
							this.af = 0;
							this.time_ = 0f;
							this.F = null;
							X.ALL0(this.AagR_dif);
							this.En.Phy.rgd_agR = 0f;
						}
						return false;
					}
					this.FineFrame();
					return true;
				}
			}

			public void addTime(float t)
			{
				this.FineFrame();
				t *= this.En.TS;
				this.time_ += t;
				this.Anm.need_fine_mesh = true;
				this.En.Phy.rgd_agR = this.AagR[this.AagR_dif.Length / 2] * 0.31830987f * 180f;
				while (this.time_ >= 2f)
				{
					this.time_ -= 2f;
					this.af = (this.af + 1) % this.Sq.countFrames();
					if (this.af == 1 || this.af == 6)
					{
						this.En.playSndPos("gecko_foot", 1);
					}
					this.F = this.Sq.getFrame(this.af);
					for (int i = this.AagR_dif.Length - 1; i >= 0; i--)
					{
						this.AagR_dif[i] = X.angledifR(this.AagR[i], (i == 0) ? this.En.angleR : this.AagR[i - 1]);
					}
				}
				for (int j = this.AagR_dif.Length - 1; j >= 0; j--)
				{
					float num = this.AagR_dif[j];
					if (X.Abs(num) <= 0.001f)
					{
						this.AagR[j] = X.VALWALKANGLER(this.AagR[j], this.En.angleR, 0.0037699114f);
					}
					else
					{
						float num2 = num * 0.15f * t;
						this.AagR_dif[j] = num - num2;
						this.AagR[j] = this.AagR[j] + num2;
					}
				}
			}

			public PxlFrame FineFrame()
			{
				if (this.F == null)
				{
					this.F = this.Sq.getFrame(this.af);
					float num = this.En.angleR + ((this.En.mpf_is_right < 0f) ? 3.1415927f : 0f);
					for (int i = this.AagR.Length - 1; i >= 0; i--)
					{
						this.AagR[i] = num;
					}
				}
				return this.F;
			}

			public bool fnDrawStand(MeshDrawer Md, EnemyAnimator.EnemyAdditionalDrawFrame Ead)
			{
				this.FineFrame();
				PxlImage img = this.F.getLayer(0).Img;
				int num = this.AagR.Length;
				float num2 = (float)img.width * 0.5f * 0.015625f;
				float num3 = 0f;
				float num4 = img.RectIUv.xMax;
				float num5 = (float)img.height * 0.5f * 0.015625f;
				float num6 = 1f / (float)num;
				float num7 = img.RectIUv.width * num6;
				float num8 = (float)img.width * num6 * 0.015625f;
				for (int i = -1; i < num; i++)
				{
					float num9 = ((i == -1) ? 0f : (this.AagR[i] - this.En.angleR));
					float num10 = X.Cos(num9);
					float num11 = X.Sin(num9);
					if (i >= 0)
					{
						Md.Tri(-2, 0, -1, false).Tri(0, 1, -1, false);
					}
					Md.uvRectN(num4, img.RectIUv.y).Pos(num2 + num11 * num5, num3 - num10 * num5, null);
					Md.uvRectN(num4, img.RectIUv.yMax).Pos(num2 - num11 * num5, num3 + num10 * num5, null);
					num2 -= num8 * num10;
					num3 -= num8 * num11;
					num4 -= num7;
				}
				return true;
			}

			public void fineFrameData(EnemyAnimator Anm, EnemyFrameDataBasic FrmData, bool created)
			{
			}

			public int createEyes(EnemyAnimator Anm, Matrix4x4 MxAfterMultiple, ref int eyepos_search)
			{
				if (!this.active)
				{
					return 0;
				}
				this.FineFrame();
				int num = this.F.countLayers();
				EnemyFrameDataBasic enemyFrameDataBasic;
				Anm.getFrameData(this.F, out enemyFrameDataBasic);
				for (int i = 0; i < num; i++)
				{
					if (enemyFrameDataBasic.isEye(i))
					{
						Anm.assignEyePosition(this.F.getLayer(i), MxAfterMultiple, ref eyepos_search);
					}
				}
				return 0;
			}

			public void checkFrame(EnemyAnimator Anm, float TS)
			{
			}

			public void drawEyeInnerInit(EnemyAnimator Anm)
			{
			}

			public void destruct()
			{
			}

			private NelNGecko En;

			private PxlSequence Sq;

			private EnemyAnimator Anm;

			public int af;

			private float time_;

			public const float anm_changet = 2f;

			private float[] AagR;

			private float[] AagR_dif;
		}
	}
}
