using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNGolemToyMisPod : NelNGolemToy
	{
		public override int create_count_normal
		{
			get
			{
				return 5;
			}
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			NelNGolemToyMisPod.MI = this.Anm.getMI();
			this.Nai.attackable_length_top = -9f;
			this.Nai.attackable_length_x = 9f;
			this.Nai.attackable_length_bottom = 9f;
			if (NelNGolemToyMisPod.FD_MgRun == null)
			{
				NelNGolemToyMisPod.FD_MgRun = (MagicItem Mg, float fcnt) => NelNGolemToyMisPod.MgRun(Mg, fcnt, 3, 8f);
				NelNGolemToyMisPod.FD_MgDraw = (MagicItem Mg, float fcnt) => NelNGolemToyMisPod.MgDraw(Mg, fcnt, NelNGolemToyMisPod.SqMis, NelNGolemToyMisPod.MI);
			}
			this.SqPod = this.Anm.getCurrentCharacter().getPoseByName("mispod").getSequence(0);
			this.SqPodClose = this.Anm.getCurrentCharacter().getPoseByName("mispod_close").getSequence(0);
			NelNGolemToyMisPod.SqMis = this.Anm.getCurrentCharacter().getPoseByName("missile").getSequence(0);
		}

		protected override void initBorn()
		{
			base.initBorn();
			this.AnmT.allow_check_main = true;
			this.dep_shotR = (this.shotR = ((this.aim == AIM.L) ? 3.1415927f : 0f));
			this.Anm.setPose("mispod_stand", -1);
			this.Anm.timescale = 0f;
			this.Nai.AddF(NAI.FLAG.GAZE, 180f);
			this.cannot_move = false;
		}

		public override void setDeathDro()
		{
			if (this.SqPod != null)
			{
				this.Anm.setBreakerDropObject("golemtoy_break", 0f, 0f, 0f, this.SqPod);
			}
			base.setDeathDro();
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			this.stopSndWalk();
			this.playing_rotating_spd = false;
			base.destruct();
		}

		protected override bool considerNormal(NAI Nai)
		{
			if (!base.create_finished)
			{
				return true;
			}
			if (this.missile_rest > 0 && base.hasF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL))
			{
				base.remF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
				if (this.pod_cframe >= 0)
				{
					return Nai.AddTicketB(NAI.TYPE.BACKSTEP, 128, true);
				}
				this.missile_rest = 0;
				this.pod_hide_bits = 0U;
			}
			if (Nai.cant_access_to_pr())
			{
				return Nai.AddTicketB(NAI.TYPE.WAIT, 5, true);
			}
			if (Nai.HasF(NAI.FLAG.GAZE, true))
			{
				return Nai.AddTicketB(NAI.TYPE.GAZE, 128, true);
			}
			if (!Nai.hasPT(128, false) && Nai.isAttackableLength(false) && !Nai.hasTypeLock(NAI.TYPE.PUNCH))
			{
				return Nai.AddTicketB(NAI.TYPE.PUNCH, 128, true);
			}
			return !Nai.hasPT(5, false) && Nai.AddTicketB(NAI.TYPE.WALK, 5, true);
		}

		public override void runPre()
		{
			base.runPre();
			if (this.Nai == null)
			{
				return;
			}
			if (base.create_finished)
			{
				if (this.calc_dep_t <= 0f)
				{
					this.calc_dep_t = (float)(this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE) ? 50 : 20);
					this.dep_shotR = this.Mp.GAR(base.x, base.y, this.Nai.target_x, X.Mn(this.Nai.target_y, base.y - 0.01f));
					this.through_front = this.Mp.canThroughBcc(base.x, base.y, this.Nai.target_x, this.Nai.target_y, 0.7f, 0.7f, -1, false, false, null, true, null);
					if (!this.through_front)
					{
						uint ran = X.GETRAN2((int)this.Mp.floort, 1 + this.index);
						float num = X.NI(0.2f, 0.35f, X.RAN(ran, 4431)) * 3.1415927f;
						if (this.dep_shotR < num)
						{
							this.dep_shotR += num;
						}
						else if (this.dep_shotR > 3.1415927f - num)
						{
							this.dep_shotR -= num;
						}
						else if (this.Nai.isFrontType(NAI.TYPE.PUNCH, PROG.ACTIVE))
						{
							float num2 = this.dep_shotR - num;
							float num3 = this.dep_shotR + num;
							Vector3 vector = this.Mp.BCC.crosspoint(base.x, base.y, base.x + 12f * X.Cos(num2), base.y - 12f * X.Sin(num2), 0.3f, 0.3f, null, true, null);
							Vector3 vector2 = this.Mp.BCC.crosspoint(base.x, base.y, base.x + 12f * X.Cos(num3), base.y - 12f * X.Sin(num3), 0.3f, 0.3f, null, true, null);
							if (vector.z < 2f && vector2.z < 2f)
							{
								this.dep_shotR += (float)X.MPF(X.RAN(ran, 688) < 0.5f) * num;
							}
							else if (vector.z < 2f)
							{
								this.dep_shotR = num2;
							}
							else if (vector2.z < 2f)
							{
								this.dep_shotR = num3;
							}
							else
							{
								this.dep_shotR = ((X.LENGTHXY2(base.x, base.y, vector.x, vector.y) < X.LENGTHXY2(base.x, base.y, vector2.x, vector2.y)) ? num3 : num2);
							}
						}
						else
						{
							this.dep_shotR += (float)X.MPF(X.RAN(ran, 688) < 0.5f) * num;
						}
					}
				}
				if (X.Abs(this.dep_shotR - this.shotR) < 0.009424779f)
				{
					this.playing_rotating_spd = false;
				}
				else
				{
					this.playing_rotating_spd = true;
					this.shotR = base.VALWALK(this.shotR, this.dep_shotR, 0.015079645f);
					this.Anm.need_fine_mesh = true;
				}
				this.calc_dep_t -= this.TS;
				this.Anm.timescale = (float)((this.Phy.walk_xspeed == 0f) ? 0 : 1);
			}
		}

		public override NelEnemy changeState(NelEnemy.STATE state)
		{
			NelEnemy.STATE state2 = this.state;
			base.changeState(state);
			return this;
		}

		public override bool readTicket(NaTicket Tk)
		{
			if (!base.create_finished)
			{
				return false;
			}
			NAI.TYPE type = Tk.type;
			if (type <= NAI.TYPE.PUNCH)
			{
				if (type == NAI.TYPE.WALK)
				{
					return this.runWalk(Tk.initProgress(this), Tk);
				}
				if (type == NAI.TYPE.PUNCH)
				{
					return this.runPunch(Tk.initProgress(this), Tk);
				}
			}
			else
			{
				if (type == NAI.TYPE.BACKSTEP)
				{
					return this.runBackStep(Tk.initProgress(this), Tk);
				}
				if (type == NAI.TYPE.GAZE)
				{
					return this.runGaze(Tk.initProgress(this), Tk, Tk.type == NAI.TYPE.GAZE);
				}
			}
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (Tk != null && Tk.type == NAI.TYPE.BACKSTEP)
			{
				base.remF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
			}
			this.pod_burst_pos = 0f;
			this.stopSndWalk();
			base.quitTicket(Tk);
		}

		private void createSndWalk()
		{
			if (this.SndWalk == null)
			{
				this.SndWalk = this.playSndPos("golemtoy_car", 1);
			}
		}

		private void stopSndWalk()
		{
			if (this.SndWalk != null && this.SndWalk.key == this.snd_key)
			{
				this.SndWalk.Stop();
				this.SndWalk = null;
			}
		}

		public bool playing_rotating_spd
		{
			get
			{
				return this.SndRot != null;
			}
			set
			{
				if (this.playing_rotating_spd == value)
				{
					return;
				}
				if (value)
				{
					this.SndRot = this.playSndPos("golemtoy_car_rotate", 1);
					return;
				}
				this.SndRot.Stop();
				this.SndRot = null;
			}
		}

		public bool runGaze(bool init_flag, NaTicket Tk, bool show_front)
		{
			if (init_flag)
			{
				this.t = 0f;
			}
			this.calc_dep_t = 20f;
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 45, true))
			{
				this.TeCon.setQuakeSinH(5f, 16, 5.43f, 1.3f, 0);
				this.stopSndWalk();
				base.PtcVar("by", (double)(base.y + this.sizey * 0.5f)).PtcVar("sizex", (double)(this.sizex * this.Mp.CLENB)).PtcVar("time", 150.0)
					.PtcST("mispod_act", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.pod_pos = X.NIL(0f, 40f, this.t, 150f);
				this.Anm.need_fine_mesh = true;
				if (Tk.Progress(ref this.t, 150, true))
				{
					this.PtcHld.killPtc("mispod_act", false);
					this.playSndPos("golemtoy_car_act2", 1);
					this.TeCon.setQuakeSinH(5f, 20, 6.33f, 1.3f, 0);
					Tk.AfterDelay(50f);
					this.Nai.addTypeLock(NAI.TYPE.PUNCH, 280f);
					return false;
				}
			}
			return true;
		}

		public float dep_xspeed
		{
			get
			{
				return X.NI(0.044f, 0.0133f, this.enlarge_level - 1f);
			}
		}

		public bool runWalk(bool init_flag, NaTicket Tk)
		{
			float mpf_is_right = base.mpf_is_right;
			if (Tk.prog <= PROG.ACTIVE)
			{
				if (init_flag || this.walk_time < 0f)
				{
					this.t = 0f;
					this.walk_time = 0f;
					base.PtcVar("by", (double)(base.y + this.sizey * 0.5f)).PtcVar("sizex", (double)(this.sizex * this.Mp.CLENB)).PtcST("mispod_movestart", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.TeCon.setQuakeSinH(7f, 14, 6.33f, 1.3f, 0);
					this.missile_rest = 0;
					this.pod_hide_bits = 0U;
				}
				if (init_flag)
				{
					this.t = 0f;
					this.walk_time = 0f;
					this.walk_st = 0;
				}
				this.Phy.walk_xspeed = base.VALWALK(this.Phy.walk_xspeed, 0f, 0.0036f);
				if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 24, true) && this.walk_st == 1 && X.XORSP() < 0.2f && !this.Nai.HasF(NAI.FLAG.ABSORB_FINISHED, false))
				{
					this.walk_st = 22 + (int)(X.XORSP() * 40f);
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.walk_time <= 0f)
				{
					this.walk_time = 30f;
					this.t = 0f;
					this.createSndWalk();
				}
				this.Phy.walk_xspeed = base.VALWALK(this.Phy.walk_xspeed, mpf_is_right * this.dep_xspeed, 0.0018f);
				AIM aim = this.aim;
				if (base.wallHittedA())
				{
					this.walk_time -= this.TS * 4f;
					aim = ((mpf_is_right > 0f) ? AIM.L : AIM.R);
					if (this.walk_time <= 0f)
					{
						this.walk_st = 1;
					}
				}
				else if (base.x < this.Nai.target_x != mpf_is_right > 0f && this.walk_st == 0)
				{
					aim = ((base.x < this.Nai.target_x) ? AIM.R : AIM.L);
					this.walk_time -= this.TS;
				}
				else
				{
					this.walk_time = base.VALWALK(this.walk_time, 30f, 0.33f);
				}
				if (this.walk_time <= 0f)
				{
					this.walk_time = -1f;
					Tk.prog = PROG.ACTIVE;
					this.setAim(aim, false);
					this.stopSndWalk();
					return true;
				}
				if (this.walk_st == 1)
				{
					M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
					if (footBCC != null && this.Nai.TargetLastBcc == footBCC)
					{
						this.walk_st = 0;
					}
				}
				if (this.walk_st >= 2 && this.t >= (float)this.walk_st)
				{
					Tk.Recreate(NAI.TYPE.PUNCH, 128, true, null);
					this.stopSndWalk();
					return true;
				}
			}
			return true;
		}

		public bool runPunch(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = -1;
				this.walk_time = 0f;
				this.pod_hide_bits = 0U;
				this.missile_rest = 0;
			}
			bool flag = Tk.prog < PROG.PROG1;
			if (X.Abs(this.Nai.target_sydif) < 1.4f && this.through_front && this.Nai.target_sxdif < 5f && Tk.prog < PROG.PROG2)
			{
				if (base.x < this.Nai.target_x)
				{
					this.Phy.walk_xspeed = base.VALWALK(this.Phy.walk_xspeed, -this.dep_xspeed, 0.0072f);
				}
				else
				{
					this.Phy.walk_xspeed = base.VALWALK(this.Phy.walk_xspeed, this.dep_xspeed, 0.0072f);
				}
			}
			else if (this.through_front || Tk.prog >= PROG.PROG1)
			{
				this.Phy.walk_xspeed = base.VALWALK(this.Phy.walk_xspeed, 0f, 0.00036f);
				flag = false;
			}
			else
			{
				if (this.Phy.walk_xspeed == 0f)
				{
					this.setAim((X.XORSP() < 0.5f) ? AIM.L : AIM.R, false);
				}
				this.Phy.walk_xspeed = base.VALWALK(base.mpf_is_right * this.dep_xspeed, 0f, 0.0018f);
				if (base.wallHitted(AIM.R))
				{
					this.Phy.walk_xspeed = -0.0018f;
					this.setAim(AIM.L, false);
				}
				else if (base.wallHitted(AIM.L))
				{
					this.Phy.walk_xspeed = 0.0018f;
					this.setAim(AIM.R, false);
				}
			}
			if (flag)
			{
				this.createSndWalk();
			}
			else
			{
				this.stopSndWalk();
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 20, true))
			{
				base.PtcST("golemtoy_car_openpad", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_C);
				this.TeCon.setQuakeSinH(3f, 10, 3.43f, 1.3f, 0);
				this.missile_rest = 4;
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.pod_cframe = (int)(1f + this.t / 48f * (float)(this.SqPod.countFrames() - 1));
				if (Tk.Progress(ref this.t, 0, this.pod_cframe >= this.SqPod.countFrames() - 1))
				{
					this.pod_cframe = this.SqPod.countFrames() - 1;
					this.PtcHld.killPtc("golemtoy_car_openpad", false);
					this.playSndPos("golemcar_prepared", 1);
					this.walk_st = 0;
				}
			}
			if (this.pod_burst_pos != 0f)
			{
				this.pod_burst_pos = base.VALWALK(this.pod_burst_pos, 0f, 1.5f);
				this.Anm.need_fine_mesh = true;
			}
			if (Tk.prog == PROG.PROG1)
			{
				this.calc_dep_t = 90f;
				if (this.walk_st == 0)
				{
					if (this.pod_hide_bits != 0U && !this.playing_rotating_spd)
					{
						float num = (float)X.MPF(base.mpf_is_right > 0f);
						if ((num > 0f) ? (this.shotR < 2.1991148f) : (this.shotR > 0.9424779f))
						{
							this.shotR += 0.009424779f * num;
							this.dep_shotR = this.shotR;
							this.Anm.need_fine_mesh = true;
						}
					}
					if (this.t >= 100f)
					{
						bool flag2 = 4 == this.missile_rest;
						PxlLayer pxlLayer = this.progressMissile();
						if (pxlLayer != null)
						{
							this.launchMissle(pxlLayer);
							this.pod_burst_pos = 20f;
							if (flag2)
							{
								this.setAim((base.x < this.Nai.target_x) ? AIM.R : AIM.L, false);
							}
							this.t = 80f;
						}
						else
						{
							this.t = 0f;
							this.walk_st = 1;
						}
					}
				}
				else if (Tk.Progress(ref this.t, 60, true))
				{
					this.pod_hide_bits = 0U;
					this.pod_cframe = -(this.SqPodClose.countFrames() - 1);
					this.playSndPos("golemtoy_car_closepad", 1);
					this.TeCon.setQuakeSinH(3f, 10, 3.43f, 1.3f, 0);
				}
			}
			if (Tk.prog == PROG.PROG2)
			{
				this.calc_dep_t = 3f;
				this.pod_cframe = -(int)((float)(this.SqPodClose.countFrames() - 1) * (1f - X.ZLINE(this.t, 40f)));
				if (this.t >= 65f)
				{
					this.pod_cframe = 0;
					this.pod_hide_bits = 0U;
					this.Nai.addTypeLock(NAI.TYPE.PUNCH, 280f);
					return false;
				}
			}
			return true;
		}

		private PxlLayer progressMissile()
		{
			PxlFrame frame = this.SqPod.getFrame(this.pod_cframe);
			int num = frame.countLayers();
			for (int i = 0; i < num; i++)
			{
				if ((this.pod_hide_bits & (1U << i)) == 0U)
				{
					PxlLayer layer = frame.getLayer(i);
					if (TX.isStart(layer.name, "mis", 0))
					{
						this.pod_hide_bits |= 1U << i;
						this.Anm.need_fine_mesh = true;
						this.missile_rest--;
						return layer;
					}
				}
			}
			return null;
		}

		private void launchMissle(PxlLayer L)
		{
			Vector2 vector = X.ROTV2e(new Vector2(L.x, -L.y), this.shotR);
			vector.y += this.pod_pos * 0.5f;
			vector *= base.scaleY * this.Mp.rCLEN;
			float num = (this.drawx + this.getSpShiftX()) * this.Mp.rCLEN + vector.x;
			float num2 = (this.drawy - this.getSpShiftY()) * this.Mp.rCLEN - vector.y;
			MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(NelNGolemToyMisPod.FD_MgRun, NelNGolemToyMisPod.FD_MgDraw);
			magicItem.sx = num;
			magicItem.sy = num2;
			magicItem.dx = (magicItem.dy = 0f);
			magicItem.dz = base.scaleX;
			magicItem.da = (float)(this.through_front ? 20 : 15);
			magicItem.sz = (float)(this.through_front ? 1 : 0);
			magicItem.Atk0 = this.AtkMissile;
			magicItem.sa = (magicItem.aim_agR = this.shotR);
		}

		public static void MgMissileInit(MagicItem Mg, int first_phase = 0)
		{
			Mg.Ray.check_hit_wall = true;
			Mg.phase = 1 + first_phase;
			Mg.efpos_s = (Mg.raypos_s = (Mg.aimagr_calc_s = true));
			Mg.wind_apply_s_level = 1f;
			Mg.Ray.RadiusM(0.15f);
			Mg.Ray.check_other_hit = true;
			Mg.Ray.Atk = Mg.Atk0;
			Mg.projectile_power = 100;
			Mg.t = 19f;
			Mg.PtcVar("cx", (double)Mg.sx).PtcVar("cy", (double)Mg.sy).PtcVar("agR", (double)Mg.sa)
				.PtcST("golemtoy_missile_launched", PTCThread.StFollow.FOLLOW_S, false);
		}

		public static bool MgRun(MagicItem Mg, float fcnt, int rot_count = 3, float rotate_recheck_interval = 8f)
		{
			if (Mg.phase == 0)
			{
				NelNGolemToyMisPod.MgMissileInit(Mg, 0);
			}
			if (Mg.phase >= 1)
			{
				Mg.sa = X.VALWALKANGLER(Mg.sa, Mg.aim_agR, ((Mg.da >= 0f) ? 0.003f : 0.012f) * 3.1415927f * fcnt);
				float num = 0.015f;
				bool flag = false;
				if (Mg.da >= 0f)
				{
					Mg.da -= fcnt;
					num = X.NI(num, 0.18f, X.ZLINE(Mg.t, 38f));
					if (Mg.da <= 0f)
					{
						if (Mg.phase >= 1 + rot_count)
						{
							flag = true;
						}
						else
						{
							NelNGolemToyMisPod.calcAimPos(Mg);
							Mg.sz = (float)(Mg.Mp.canThroughBcc(Mg.sx, Mg.sy, Mg.PosA.x, Mg.PosA.y, 0.3f, 0.3f, -1, false, false, null, true, null) ? 1 : 0);
							if (Mg.sz == 1f)
							{
								if (X.Abs(X.angledifR(Mg.aim_agR, Mg.sa)) < 0.47123894f)
								{
									Mg.da = 15f;
								}
								else
								{
									int num2 = Mg.phase + 1;
									Mg.phase = num2;
									if (num2 >= 1 + rot_count)
									{
										Mg.da = 120f;
										Mg.aim_agR = Mg.sa;
									}
									else
									{
										Mg.da = -60f;
										Mg.dy = -4f;
									}
								}
							}
							else
							{
								Mg.da = rotate_recheck_interval;
							}
						}
					}
				}
				else
				{
					num = X.NI(0.18f, num, X.ZLINE(60f + Mg.da, 20f));
					Mg.da += fcnt;
					Mg.dy += fcnt;
					if (Mg.da >= 0f)
					{
						NelNGolemToyMisPod.calcAimPos(Mg);
						Mg.t = 0f;
						Mg.da = 20f;
					}
					else if (Mg.dy >= 0f)
					{
						Mg.dy = -4f;
						NelNGolemToyMisPod.calcAimPos(Mg);
					}
				}
				if (!flag)
				{
					Mg.MnSetRay(Mg.Ray, 0, Mg.sa, 0f);
					float num3 = num * fcnt;
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
					Mg.sx += num4;
					Mg.sy += num5;
					if ((hittype & HITTYPE.WALL_AND_BREAK) != HITTYPE.NONE)
					{
						flag = true;
					}
					else if ((hittype & HITTYPE.REFLECTED) != HITTYPE.NONE)
					{
						Mg.reflectAgR(Mg.Ray, ref Mg.sa, 0.25f);
						Mg.da = (float)((Mg.da >= 0f) ? 50 : (-1));
					}
				}
				else
				{
					Mg.Ray.RadiusM(0.9f);
					Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
				}
				if (flag)
				{
					Mg.PtcVar("sx", (double)Mg.sx).PtcVar("sy", (double)Mg.sy).PtcST("missile_bomb", PTCThread.StFollow.NO_FOLLOW, false);
					return false;
				}
			}
			return true;
		}

		public static bool MgDraw(MagicItem Mg, float fcnt, PxlSequence SqMis, MImage MI)
		{
			int num = ((Mg.da < 0f) ? 8 : 4);
			PxlFrame frame = SqMis.getFrame(1 + X.ANM((int)Mg.t, 4, (float)num));
			MeshDrawer mesh = Mg.Ef.GetMesh("missile", MI.getMtr(BLEND.NORMAL, -1), false);
			MeshDrawer mesh2 = Mg.Ef.GetMesh("", MI.getMtr(BLEND.ADD, -1), false);
			mesh.Col = MTRX.ColWhite;
			mesh2.Col = MTRX.ColWhite;
			if (mesh.draw_triangle_count == 0)
			{
				mesh.base_z = mesh2.base_z - 0.001f;
			}
			float dz = Mg.dz;
			mesh.RotaPF(0f, 0f, dz, dz, Mg.sa, frame, false, false, false, 4294967294U, false, 0);
			mesh2.RotaPF(0f, 0f, dz, dz, Mg.sa, frame, false, false, false, 1U, false, 0);
			return true;
		}

		public override Vector2 getAimPos(MagicItem Mg)
		{
			if (this.Nai == null)
			{
				return new Vector2(Mg.sx + 2f * X.Cos(Mg.sa), Mg.sy - 2f * X.Sin(Mg.sa));
			}
			if (base.AimPr == null)
			{
				Vector2 targetPos = this.getTargetPos();
				targetPos.x += (float)(CAim._XD(this.aim, 1) * 4);
				return targetPos;
			}
			uint ran = X.GETRAN2(Mg.id, 4 + Mg.id % 3);
			return new Vector2(this.Nai.target_x + base.AimPr.sizex * 0.6f * (-1f + X.RAN(ran, 1578) * 2f), this.Nai.target_y + base.AimPr.sizey * 0.6f * (-1f + X.RAN(ran, 4389) * 2f));
		}

		private static void calcAimPos(MagicItem Mg)
		{
			NelEnemy nelEnemy = Mg.Caster as NelEnemy;
			if (nelEnemy == null || nelEnemy.getAI() == null)
			{
				Mg.aim_agR = Mg.sa;
				Mg.PosA = new Vector2(Mg.sx + 2f * X.Cos(Mg.sa), Mg.sy - 2f * X.Sin(Mg.sa));
				Mg.calced_aim_pos = true;
				return;
			}
			Mg.calcAimPos(false);
		}

		public bool runBackStep(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				this.walk_time = 0f;
				this.dep_shotR = this.shotR;
				base.PtcVar("sizex", (double)(this.sizex * base.CLENM)).PtcVar("sizey", (double)(this.sizey * base.CLENM)).PtcST("machine_error_elec", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.TeCon.setQuake(2f, 170, 1f, 15);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.walk_time == 0f && this.t < 10f && this.Phy.pre_force_velocity_x != 0f)
				{
					this.walk_time = (float)X.MPF(this.Phy.pre_force_velocity_x > 0f);
					this.Phy.addFoc(FOCTYPE.WALK, this.walk_time * 0.07f, 0f, -1f, 0, 10, 40, -1, 0);
				}
				if (Tk.Progress(ref this.t, 60, true))
				{
					this.PtcHld.killPtc("machine_error_elec", false);
					this.t = 100f;
				}
			}
			if (Tk.prog == PROG.PROG0 && this.t >= 100f)
			{
				if (this.missile_rest <= 0)
				{
					Tk.prog = PROG.PROG1;
					this.t = 0f;
					this.walk_st = 0;
				}
				else
				{
					this.t = 100f - X.NIXP(17f, 27f);
					this.shotR = this.dep_shotR + X.NIXP(-0.32f, 0.32f) * 3.1415927f;
					this.Anm.need_fine_mesh = true;
					this.AtkMySelf.Caster = this;
					this.AtkMySelf.AttackFrom = this;
					this.applyDamage(this.AtkMySelf, false);
					base.PtcVar("sx", (double)(base.x + X.NIXP(-0.7f, 0.7f))).PtcVar("sy", (double)(base.y - 40f * this.Mp.rCLEN * base.scaleY + X.NIXP(-0.7f, 0.7f))).PtcST("missile_selfbomb", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.progressMissile();
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.walk_st == 0)
				{
					if (this.t >= 20f)
					{
						this.t = 0f;
						this.walk_st = 1;
						this.playSndPos("golemtoy_car_closepad", 1);
					}
				}
				else
				{
					this.pod_cframe = -(int)((float)(this.SqPodClose.countFrames() - 1) * (1f - X.ZLINE(this.t, 40f)));
					if (Tk.Progress(ref this.t, 70, true))
					{
						base.remF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
						this.Nai.addTypeLock(NAI.TYPE.PUNCH, 280f);
						return false;
					}
				}
			}
			return true;
		}

		public override void checkTiredTime(ref int t0, NelAttackInfo Atk)
		{
			base.checkTiredTime(ref t0, Atk);
			if (this.missile_rest > 0)
			{
				t0 = X.Mn(t0, 10);
			}
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if ((Ray.hittype & HITTYPE.GUARD_IGNORE) != HITTYPE.NONE && !this.Ser.has(SER.EATEN) && this.missile_rest <= 0)
			{
				return RAYHIT.NONE;
			}
			return base.can_hit(Ray);
		}

		public override int applyDamage(NelAttackInfo Atk, bool force = false)
		{
			base.remF(NelEnemy.FLAG._DMG_EFFECT_BITS);
			int hp = this.hp;
			if (this.missile_rest > 0 && (!this.Nai.isFrontType(NAI.TYPE.BACKSTEP, PROG.ACTIVE) || Atk == this.AtkMySelf))
			{
				base.addF(NelEnemy.FLAG.DMG_EFFECT_CRITICAL);
				if (!this.Nai.isFrontType(NAI.TYPE.BACKSTEP, PROG.ACTIVE))
				{
					this.Nai.clearTicket(-1, true);
				}
			}
			return base.applyDamage(Atk, force);
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			Aattr.Add(MGATTR.BOMB);
		}

		public override void makeBone(List<Vector3> ABone)
		{
			float num = 3f * base.CLEN;
			int num2 = 4;
			float num3 = -0.5f * num;
			for (int i = 0; i < num2; i++)
			{
				ABone.Add(new Vector3(num3, (i == 1 || i == 2) ? 4.2f : 3f, (float)(i * 3)));
				num3 += num / (float)(num2 - 1);
			}
		}

		public override bool drawSpecial(MeshDrawer Md)
		{
			return false;
		}

		public int pod_cframe
		{
			get
			{
				return this.pod_cframe_;
			}
			set
			{
				if (this.pod_cframe == value)
				{
					return;
				}
				this.pod_cframe_ = value;
				this.Anm.need_fine_mesh = true;
			}
		}

		public override void drawAfter(MeshDrawer Md)
		{
			if (!base.create_finished)
			{
				return;
			}
			base.drawAfter(Md);
			PxlFrame pxlFrame;
			if (this.pod_cframe >= 0)
			{
				pxlFrame = this.SqPod.getFrame(X.Mn(this.pod_cframe, this.SqPod.countFrames() - 1));
			}
			else
			{
				pxlFrame = this.SqPodClose.getFrame(X.Mx(0, this.SqPodClose.countFrames() + this.pod_cframe));
			}
			float num = 0f;
			float num2 = this.pod_pos;
			if (this.pod_burst_pos != 0f)
			{
				num -= X.Cos(this.shotR) * this.pod_burst_pos;
				num2 -= X.Sin(this.shotR) * this.pod_burst_pos;
			}
			Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, uint.MaxValue & ~this.pod_hide_bits, false, 0);
		}

		private float shotR;

		private PxlSequence SqPod;

		private PxlSequence SqPodClose;

		private static PxlSequence SqMis;

		private int pod_cframe_;

		private uint pod_hide_bits;

		private M2SoundPlayerItem SndWalk;

		private float dep_shotR;

		private float calc_dep_t;

		private bool through_front;

		private int missile_rest;

		private const int missile_max = 4;

		private float pod_pos;

		private const float dep_pod_pos = 40f;

		private float pod_burst_pos;

		private M2SoundPlayerItem SndRot;

		private static MagicItem.FnMagicRun FD_MgRun;

		private static MagicItem.FnMagicRun FD_MgDraw;

		private static MImage MI;

		private const int MISSILE_COOLTIME = 280;

		private const float MISSILE_SHOT_DELAY = 100f;

		protected NelAttackInfo AtkMissile = new NelAttackInfo
		{
			hpdmg0 = 12,
			split_mpdmg = 7,
			burst_vx = 0.04f,
			huttobi_ratio = 0.02f,
			attr = MGATTR.BOMB,
			Beto = BetoInfo.Lava.Pow(55, false),
			shield_break_ratio = -30f,
			parryable = true
		}.Torn(0.015f, 0.11f);

		protected NelAttackInfo AtkMySelf = new NelAttackInfo
		{
			hpdmg0 = 42,
			attr = MGATTR.FIRE
		};

		public const int PRI_ATK = 128;

		public const int PRI_WALK = 5;

		private const float xspeed_ax = 0.0018f;
	}
}
