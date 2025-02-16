using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNGolemToyBow : NelNGolemToy
	{
		private MGATTR arrow_attr
		{
			get
			{
				if (!this.Mh.isActive(this) || this.Mh.Mg.Atk0 == null)
				{
					return MGATTR.NORMAL;
				}
				return this.Mh.Mg.Atk0.attr;
			}
		}

		private bool arrow_berserk
		{
			get
			{
				return this.arrow_attr != MGATTR.ACME;
			}
		}

		public float calcedR
		{
			get
			{
				return this.Mn.GetHit(0).agR;
			}
			set
			{
				this.Mn.GetHit(0).agR = value;
			}
		}

		public float reflecR
		{
			get
			{
				return this.Mn.GetHit(1).agR;
			}
			set
			{
				this.Mn.GetHit(1).agR = value;
			}
		}

		public override int create_count_normal
		{
			get
			{
				return 11;
			}
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.mainTexture = this.Anm.getMainTexture();
			this.Nai.attackable_length_top = -9f;
			this.Nai.attackable_length_x = 9f;
			this.Nai.attackable_length_bottom = 9f;
			this.enlarge_publish_damage_ratio = 1f;
			this.FD_MgRun = new MagicItem.FnMagicRun(this.MgRun);
			this.FD_MgDraw = new MagicItem.FnMagicRun(this.MgDraw);
			this.FD_phase_check = new MgNGeneralBeam.FnPhaseContinue(this.fnPhaseCheckBeam);
			this.AMvHit = new List<M2Ray.M2RayHittedItem>(4);
			this.shoot_time = X.NI(100, 220, base.mp_ratio);
			this.shoot_after_delay = X.NI(80, 140, base.mp_ratio);
			this.bow_main_attr = X.Mn((int)(this.smn_xorsp * 4f), 3);
			this.SqPod = this.Anm.getCurrentCharacter().getPoseByName("bow").getSequence(0);
			this.SqPodClose = this.Anm.getCurrentCharacter().getPoseByName("bow_release").getSequence(0);
			if (this.ABuf == null)
			{
				this.ABuf = new List<M2BlockColliderContainer.BCCHitInfo>();
			}
			this.Nai.can_progress_delay_if_ticket_exists = true;
		}

		private void decideAttr()
		{
			if (!this.Mh.isActive(this))
			{
				return;
			}
			MgNGeneralBeam.BATTR battr;
			if (!MgNGeneralBeam.nattr2battr(this.nattr, out battr))
			{
				battr = (MgNGeneralBeam.BATTR)this.bow_main_attr;
				if (this.shoot_count > 2 && X.XORSP() < 0.15f)
				{
					battr = (battr + 1 + (byte)X.xors(3)) % MgNGeneralBeam.BATTR.SLIMY;
				}
			}
			if (this.ALaserAtk == null)
			{
				this.ALaserAtk = new NelAttackInfo[5];
			}
			MgNGeneralBeam.prepareMagic(this, this.ALaserAtk, this.Mh.Mg, battr, 1f);
			this.shoot_count++;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			if (this.bow_main_attr >= 0)
			{
				Aattr.Add((MGATTR)this.bow_main_attr);
				Aattr.Add((MGATTR)this.bow_main_attr);
			}
		}

		protected override void initBorn()
		{
			base.initBorn();
			this.AnmT.allow_check_main = true;
			this.dep_shotR = (this.shotR = 1.5707964f);
			this.Anm.setPose("bow_stand", -1);
			this.Nai.AddF(NAI.FLAG.GAZE, 180f);
			this.cannot_move = true;
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
			this.Mh.destruct(this);
			this.playing_rotating_spd = false;
			base.destruct();
		}

		protected override bool considerNormal(NAI Nai)
		{
			if (!base.create_finished)
			{
				return true;
			}
			if (Nai.cant_access_to_pr())
			{
				return Nai.AddTicketB(NAI.TYPE.WAIT, 10, true);
			}
			return Nai.AddTicketB(NAI.TYPE.PUNCH, 128, true);
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
				float num = X.angledifR(this.shotR, this.dep_shotR);
				if (X.Abs(num) < 0.009424779f)
				{
					this.shotR = this.dep_shotR;
					this.playing_rotating_spd = false;
					return;
				}
				this.playing_rotating_spd = true;
				this.shotR = base.VALWALK(this.shotR, this.shotR + num, 0.031730086f);
				this.Anm.need_fine_mesh = true;
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
			if (Tk.type == NAI.TYPE.PUNCH)
			{
				return this.runPunch(Tk.initProgress(this), Tk);
			}
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			this.Mh.destruct(this);
			this.pod_burst_pos = 0f;
			base.quitTicket(Tk);
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
					this.SndRot = this.playSndPos("golemtoy_arrow_rotate", 1);
					return;
				}
				this.SndRot.Stop();
				this.SndRot = null;
			}
		}

		public bool runPunch(bool init_flag, NaTicket Tk)
		{
			MagicItem magicItem;
			if (init_flag)
			{
				this.t = 0f;
				this.walk_st = 0;
				this.walk_time = 0f;
				magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRun, this.FD_MgDraw);
				this.need_recalc_length = true;
				this.Mh = new MagicItemHandlerS(magicItem);
				magicItem.Mn = this.Mn;
				magicItem.Ray.check_hitlock_manual = true;
				magicItem.Ray.hittype |= HITTYPE.ONLY_FIRST_BREAKER;
				magicItem.Ray.hit_target_max = 2;
				this.Mn._0.accel_mint = this.shoot_time;
				this.Anm.setPose("bow_charge", -1);
				if (this.pod_cframe_ > 0)
				{
					Tk.prog = PROG.PROG0;
					this.decideAttr();
				}
				else
				{
					this.playSndPos("golemtoy_arrow_reload0", 1);
				}
				this.dep_shotR = 1.5707964f;
			}
			else if (Tk.prog < PROG.PROG4)
			{
				if (!this.Mh.isActive(this))
				{
					return false;
				}
				magicItem = this.Mh.Mg;
			}
			else
			{
				magicItem = null;
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (this.t < 55f)
				{
					this.pod_pos += X.Cos(this.t / 55f * 1.5707964f) * 0.25f * this.TS;
				}
				else
				{
					if (this.walk_st == 0)
					{
						this.walk_st = 1;
						this.decideAttr();
						base.PtcVarS("attr", this.arrow_attr).PtcVar("by", (double)base.mbottom).PtcST("golemtoy_arrow_reload", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Anm.setPose("bow_stand", -1);
					}
					float num = this.t - 55f;
					this.pod_pos = base.VALWALK(this.pod_pos, 12.5f, 1.4f + X.ZSIN(num, 22f) * 3f);
					if (Tk.Progress(ref this.t, 0, num >= 12f))
					{
						this.walk_st = 0;
						this.pod_pos = 0f;
						this.dep_shotR = 1.5707964f;
						this.pod_cframe = 1;
						this.playSndPos("golemtoy_arrow_activated", 1);
					}
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.t >= 10f)
				{
					this.pod_pos = base.VALWALK(this.pod_pos, 25f, 0.5f);
				}
				if (Tk.Progress(ref this.t, 60, true))
				{
					this.walk_st = 0;
					this.pod_pos = 25f;
					this.t = 4f;
				}
			}
			if (Tk.prog == PROG.PROG1 && this.t >= 4f)
			{
				this.t = 0f;
				if (this.calcArrowDirection(magicItem, true))
				{
					Tk.prog = PROG.PROG2;
					this.walk_st = 0;
					return true;
				}
			}
			if (Tk.prog == PROG.PROG2)
			{
				if (this.walk_st == 0)
				{
					if (!this.playing_rotating_spd)
					{
						if (X.Abs(X.angledifR(this.shotR, this.calcedR)) >= 0.009424779f)
						{
							this.dep_shotR = this.calcedR;
						}
						else
						{
							this.MgReposition(magicItem);
							magicItem.phase = 2;
							magicItem.t = 0f;
							this.walk_st = 1;
							this.t = 0f;
							this.playSndPos("golemtoy_arrow_prepare_0", 1);
						}
					}
				}
				else if (this.walk_st == 1)
				{
					this.pod_cframe = X.IntR(X.NIL(1f, (float)(this.SqPod.countFrames() - 1), this.t, 40f));
					if (this.t >= 35f)
					{
						this.Anm.setPose("bow_charge", -1);
						magicItem.PtcVar("agR", (double)this.calcedR).PtcVar("time", 90.0).PtcVarS("attr", this.arrow_attr)
							.PtcST("golemtoy_arrow_prepare_1", PTCThread.StFollow.NO_FOLLOW, false);
						this.walk_st = 2;
						this.t = 0f;
					}
				}
				else if (this.walk_st == 2 && Tk.Progress(ref this.t, 90, true))
				{
					this.pod_burst_pos = 20f;
					this.pod_cframe = -(this.SqPodClose.countFrames() - 1);
					this.Anm.need_fine_mesh = true;
					this.walk_st = 0;
					magicItem.phase = 3;
					if (this.arrow_berserk)
					{
						magicItem.Ray.hittype |= HITTYPE.PR_AND_EN;
					}
					magicItem.t = 1f;
					magicItem.killEffect();
					magicItem.PtcVar("agR", (double)this.calcedR).PtcVarS("attr", this.arrow_attr).PtcST("golemtoy_arrow_shot", PTCThread.StFollow.FOLLOW_S, false);
					if (this.reflection_shot)
					{
						magicItem.PtcVar("agR", (double)this.reflecR).PtcVarS("attr", this.arrow_attr).PtcST("golemtoy_arrow_reflec", PTCThread.StFollow.FOLLOW_D, false);
					}
					this.Anm.setPose("bow_stand", -1);
				}
			}
			if (Tk.prog == PROG.PROG3)
			{
				if (this.pod_burst_pos != 0f)
				{
					this.Anm.need_fine_mesh = true;
					this.pod_burst_pos = base.VALWALK(this.pod_burst_pos, 0f, 2.4f - 1.2f * X.ZSIN(this.t, 20f));
				}
				if (Tk.Progress(ref this.t, 0, magicItem.t >= this.shoot_time))
				{
					magicItem.killEffect();
					magicItem.phase = 4;
					magicItem.t = 0f;
					this.walk_st = 0;
					this.walk_time = (float)X.MPF(X.angledifR(this.calcedR, 1.5707964f) > 0f);
					this.pod_burst_pos = 0f;
					this.Mh.release();
				}
			}
			if (Tk.prog == PROG.PROG4)
			{
				if (this.walk_st == 0)
				{
					float num2 = 1f - X.ZSIN(this.t, 35f);
					this.pod_cframe = X.IntR(X.NI(0, -(this.SqPodClose.countFrames() - 1), num2));
					if (num2 > 0f)
					{
						this.shotR += num2 * 0.0013f * 3.1415927f * this.walk_time;
						this.dep_shotR = this.shotR;
						this.Anm.need_fine_mesh = true;
					}
					if (this.t >= this.shoot_after_delay)
					{
						this.t = 0f;
						this.pod_cframe = 0;
						this.walk_st = 1;
						this.dep_shotR = 1.5707964f;
					}
				}
				else if (Tk.Progress(ref this.t, 20, !this.playing_rotating_spd))
				{
					this.pod_cframe = 0;
					Tk.AfterDelay(10f);
					return false;
				}
			}
			return true;
		}

		private bool calcArrowDirection(MagicItem Mg, bool recalc = true)
		{
			if (Mg == null)
			{
				return false;
			}
			if (recalc)
			{
				if (this.walk_st == 0)
				{
					this.calcedR = this.Mp.GAR(base.x, base.y, this.Nai.target_x, this.Nai.target_y) + X.XORSPS() * 1.5707964f;
					this.walk_st = X.MPF(X.xors(2) == 0) * (6 + X.xors(5));
				}
				else
				{
					this.calcedR += 0.12566371f * (float)X.MPF(this.walk_st > 0);
					this.walk_st++;
				}
			}
			Vector2 vector = this.PodStartPos(this.calcedR);
			if (recalc && !this.Mp.canStand((int)vector.x, (int)vector.y))
			{
				return false;
			}
			this.HitPos.Set(0f, 0f, -1f);
			if (this.Mp.canThroughBcc(vector.x, vector.y, vector.x + 40f * X.Cos(this.calcedR), vector.y - 40f * X.Sin(this.calcedR), 0.18f, 0.18f, -1, false, true, null, false, this.ABuf) || this.ABuf.Count == 0)
			{
				this.Mn._1.no_draw = true;
				this.Mn._0.v0 = (this.Mn._0.len = 40f);
				this.Mn._0.accel_maxt = 0f;
				this.Mn._1.v0 = (this.Mn._1.len = 0f);
			}
			else
			{
				M2BlockColliderContainer.BCCLine bccline;
				this.HitPos = this.calcHit(vector, out bccline, this.calcedR);
				float housenagR = bccline.housenagR;
				float num = X.angledifR(housenagR, this.calcedR + 3.1415927f);
				this.reflecR = housenagR - num;
				this.Mn._0.v0 = (this.Mn._0.len = this.HitPos.z);
				this.Mn._0.accel_maxt = 1f;
				this.Mn._1.no_draw = false;
				if (this.Mp.canThroughBcc(this.HitPos.x + 0.29000002f * X.Cos(this.reflecR), this.HitPos.y - 0.29000002f * X.Sin(this.reflecR), this.HitPos.x + 40f * X.Cos(this.reflecR), this.HitPos.y - 40f * X.Sin(this.reflecR), 0.18f, 0.18f, -1, false, true, null, false, this.ABuf) || this.ABuf.Count == 0)
				{
					this.Mn._1.v0 = (this.Mn._1.len = 40f);
					this.Mn._1.accel_maxt = 0f;
				}
				else
				{
					M2BlockColliderContainer.BCCLine bccline2;
					Vector3 vector2 = this.calcHit(this.HitPos, out bccline2, this.reflecR);
					this.Mn._1.v0 = (this.Mn._1.len = vector2.z);
					this.Mn._1.accel_maxt = 1f;
				}
			}
			NelNGolemToyBow.CRES cres = (NelNGolemToyBow.CRES)0;
			if (recalc)
			{
				Mg.Ray.PosMap(vector).LenM(0.305f).AngleR(this.calcedR)
					.LenM(this.Mn._0.len);
				if (this.calcCasted(Mg, ref cres) && this.reflection_shot)
				{
					Mg.Ray.PosMap(this.HitPos).AngleR(this.reflecR).LenM(this.Mn._1.len);
					this.calcCasted(Mg, ref cres);
				}
				if (!this.playing_rotating_spd)
				{
					this.dep_shotR = this.calcedR;
				}
			}
			return (cres & NelNGolemToyBow.CRES.PR) != (NelNGolemToyBow.CRES)0 && (cres & NelNGolemToyBow.CRES.ME) == (NelNGolemToyBow.CRES)0;
		}

		private Vector3 calcHit(Vector2 Ps, out M2BlockColliderContainer.BCCLine HitBcc, float beamR)
		{
			Vector3 vector = new Vector3(0f, 0f, -1f);
			HitBcc = null;
			for (int i = this.ABuf.Count - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCHitInfo bcchitInfo = this.ABuf[i];
				float num = X.LENGTHXY2(Ps.x, Ps.y, bcchitInfo.x, bcchitInfo.y);
				if (vector.z < 0f || vector.z > num)
				{
					vector.Set(bcchitInfo.x, bcchitInfo.y, num);
					HitBcc = bcchitInfo.Hit;
				}
			}
			vector.z = Mathf.Sqrt(vector.z) - 0.18f;
			vector.x -= 0.18f * X.Cos(beamR);
			vector.y += 0.18f * X.Sin(beamR);
			return vector;
		}

		private bool calcCasted(MagicItem Mg, ref NelNGolemToyBow.CRES hit)
		{
			Mg.Ray.Cast(false, null, true);
			for (int i = Mg.Ray.getHittedMax() - 1; i >= 0; i--)
			{
				M2Ray.M2RayHittedItem hitted = Mg.Ray.GetHitted(i);
				if (hitted.Mv == this)
				{
					if (this.arrow_berserk)
					{
						hit |= NelNGolemToyBow.CRES.ME;
						break;
					}
				}
				else if (hitted.Mv is M2MoverPr)
				{
					hit |= NelNGolemToyBow.CRES.PR;
				}
				else if (hitted.Mv is NelEnemy && this.arrow_berserk)
				{
					if ((hit & NelNGolemToyBow.CRES.OTHER) != (NelNGolemToyBow.CRES)0)
					{
						hit |= NelNGolemToyBow.CRES.ME;
						break;
					}
					hit |= NelNGolemToyBow.CRES.OTHER;
				}
			}
			return (hit & NelNGolemToyBow.CRES.ME) == (NelNGolemToyBow.CRES)0;
		}

		public bool reflection_shot
		{
			get
			{
				return this.HitPos.z >= 0f;
			}
		}

		private Vector2 PodStartPos(float agR)
		{
			return this.PodStartPos(this.SqPod.getFrame(this.SqPod.countFrames() - 1).getLayer(1), agR);
		}

		private Vector2 PodStartPos(PxlLayer L, float agR)
		{
			Vector2 vector = X.ROTV2e(new Vector2(L.x + 6f, -L.y), agR);
			vector.y += this.pod_pos * 0.5f;
			vector *= base.scaleY * this.Mp.rCLEN;
			float num = (this.drawx + this.getSpShiftX()) * this.Mp.rCLEN + vector.x;
			float num2 = (this.drawy - this.getSpShiftY()) * this.Mp.rCLEN - vector.y;
			return new Vector2(num, num2);
		}

		private void MgReposition(MagicItem Mg)
		{
			this.need_recalc_length = false;
			Vector2 vector = this.PodStartPos(this.calcedR);
			float scaleX = base.scaleX;
			float num = X.Cos(this.calcedR);
			float num2 = X.Sin(this.calcedR);
			Mg.sx = vector.x;
			Mg.sy = vector.y;
			Mg.calced_aim_pos = true;
			if (this.reflection_shot)
			{
				Mg.dx = Mg.sx + num * this.HitPos.z;
				Mg.dy = Mg.sy - num2 * this.HitPos.z;
				float num3 = X.correctangleR(this.calcedR + 3.1415927f);
				Mg.da = num3 + X.angledifR(num3, this.reflecR) * 0.5f;
				return;
			}
			Mg.dx = 0f;
			Mg.dy = 0f;
		}

		private bool MgRun(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (this.need_recalc_length)
			{
				this.MgReposition(Mg);
				this.calcArrowDirection(Mg, false);
			}
			this.AMvHit.Clear();
			int num = MgNGeneralBeam.MgRunArrowBeam(Mg, fcnt, 5f, this.FD_phase_check);
			if (num == 2)
			{
				this.need_recalc_length = true;
			}
			return num >= 1;
		}

		private bool fnPhaseCheckBeam(int phase)
		{
			if (this.AMvHit.Count > 0 && this.Mh.isActive(this))
			{
				MagicItem mg = this.Mh.Mg;
				MagicNotifiear.MnHit hit = mg.Mn.GetHit(phase);
				float num = hit.len;
				float num2 = num;
				float num3 = num * num;
				float num4 = num3;
				Vector2 mapPos = mg.Ray.getMapPos(0f);
				for (int i = this.AMvHit.Count - 1; i >= 0; i--)
				{
					M2Ray.M2RayHittedItem m2RayHittedItem = this.AMvHit[i];
					if ((m2RayHittedItem.type & HITTYPE.PR_AND_EN) != HITTYPE.NONE)
					{
						float num5 = X.LENGTHXY2(mapPos.x, mapPos.y, this.Mp.uxToMapx(base.M2D.effectScreenx2ux(m2RayHittedItem.hit_ux)), this.Mp.uyToMapy(base.M2D.effectScreeny2uy(m2RayHittedItem.hit_uy)));
						if (num5 < num3)
						{
							num3 = num5;
						}
					}
				}
				this.AMvHit.Clear();
				if (num3 < num4)
				{
					if (phase == 0)
					{
						mg.dx = 0f;
						mg.dy = 0f;
					}
					num = X.q_rsqrt(num3);
					hit.len = X.Mn(num2, num + 0.25f);
					for (int j = phase + 1; j < mg.Mn.Count; j++)
					{
						mg.Mn.GetHit(j).len = 0f;
					}
					return false;
				}
			}
			return true;
		}

		public override bool initPublishAtk(MagicItem Mg, NelAttackInfo Atk, HITTYPE hittype, M2Ray.M2RayHittedItem HitMv)
		{
			if (HitMv != null && HitMv.Mv is M2Attackable)
			{
				this.AMvHit.Add(HitMv);
			}
			return true;
		}

		private bool MgDraw(MagicItem Mg, float fcnt)
		{
			return this.Mp != null && MgNGeneralBeam.MgDrawArrowS(Mg, fcnt, false);
		}

		public override Vector2 getAimPos(MagicItem Mg)
		{
			return new Vector2(Mg.sx + X.Cos(this.calcedR) * 8f, Mg.sy - X.Sin(this.calcedR) * 8f);
		}

		public override void makeBone(List<Vector3> ABone)
		{
			float num = 2.7f * base.CLEN;
			int num2 = 4;
			float num3 = -0.5f * num;
			for (int i = 0; i < num2; i++)
			{
				ABone.Add(new Vector3(num3, (i == 1 || i == 2) ? 5.2f : 6f, (float)(i * 3)));
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

		public float pod_pos
		{
			get
			{
				return this.pod_pos_;
			}
			set
			{
				if (this.pod_pos == value)
				{
					return;
				}
				this.pod_pos_ = value;
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
			Md.Col = this.Anm.getTeMulColor();
			MGATTR arrow_attr = this.arrow_attr;
			if (this.pod_cframe_ <= 0 || arrow_attr == MGATTR.NORMAL)
			{
				Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, uint.MaxValue, false, 0);
				return;
			}
			Color32 color;
			Color32 color2;
			Color32 color3;
			MgNGeneralBeam.mgattr2col(arrow_attr, out color, out color2, out color3);
			Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, 1U, false, 0);
			Md.Col = color;
			Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, 2U, false, 0);
			Md.Col = this.Anm.getTeMulColor();
			Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, 4294967292U, false, 0);
		}

		private float shotR;

		private PxlSequence SqPod;

		private PxlSequence SqPodClose;

		public const int ATTR_MAX = 4;

		public const int ATTR_MAX2 = 5;

		private int pod_cframe_;

		private int bow_main_attr = -1;

		private M2SoundPlayerItem SndWalk;

		private float dep_shotR;

		private bool through_front;

		private float pod_pos_;

		private const float dep_pod_pos = 25f;

		private float pod_burst_pos;

		private const float dep_pod_maxt = 60f;

		private const float dep_pod_startt = 10f;

		private const float dep_pod_slide_ax = 0.5f;

		public const float reach_len = 40f;

		public const float arrow_radius = 0.18f;

		private const float charge_time = 35f;

		private const float charge_time_2 = 90f;

		private float shoot_time;

		private float shoot_after_delay;

		private bool need_recalc_length;

		private M2SoundPlayerItem SndRot;

		private MagicItemHandlerS Mh;

		private int shoot_count;

		private MagicItem.FnMagicRun FD_MgRun;

		private MagicItem.FnMagicRun FD_MgDraw;

		private Texture mainTexture;

		private List<M2BlockColliderContainer.BCCHitInfo> ABuf;

		private Vector3 HitPos = new Vector3(0f, 0f, -1f);

		public const int MPHASE_OFFLINE = 1;

		public const int MPHASE_CHARGE = 2;

		public const int MPHASE_SHOOT = 3;

		public const int MPHASE_RELEASE = 4;

		private NelAttackInfo[] ALaserAtk;

		private MagicNotifiear Mn = new MagicNotifiear(2).AddHit(new MagicNotifiear.MnHit
		{
			type = MagicNotifiear.HIT.CIRCLE,
			maxt = 1f,
			thick = 0.18f,
			accel = 0f,
			v0 = 0f,
			penetrate_only_mv = false,
			aim_fixed = true
		}).AddHit(new MagicNotifiear.MnHit
		{
			type = MagicNotifiear.HIT.CIRCLE,
			maxt = 1f,
			aim_fixed = true,
			thick = 0.18f,
			accel = 0f,
			penetrate_only_mv = false,
			v0 = 0f
		});

		public const int PRI_WALK = 10;

		public const int PRI_ATK = 128;

		private MgNGeneralBeam.FnPhaseContinue FD_phase_check;

		private List<M2Ray.M2RayHittedItem> AMvHit;

		private enum CRES
		{
			PR = 1,
			ME,
			OTHER = 4
		}
	}
}
