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
				if (!this.Mh.isActive(this))
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
			NelNGolemToyBow.mainTexture = this.Anm.getMainTexture();
			this.Nai.attackable_length_top = -9f;
			this.Nai.attackable_length_x = 9f;
			this.Nai.attackable_length_bottom = 9f;
			this.enlarge_publish_damage_ratio = 1f;
			this.FD_MgRun = new MagicItem.FnMagicRun(this.MgRun);
			this.FD_MgDraw = new MagicItem.FnMagicRun(this.MgDraw);
			this.shoot_time = X.NI(100, 220, base.mp_ratio);
			this.shoot_after_delay = X.NI(80, 140, base.mp_ratio);
			this.bow_main_attr = (int)(X.RAN((uint)EnemySummoner.ran_session, 1443 + (EnemySummoner.ran_session & 63) + EnemySummoner.summoned_count * 277) * 4f);
			this.SqPod = this.Anm.getCurrentCharacter().getPoseByName("bow").getSequence(0);
			this.SqPodClose = this.Anm.getCurrentCharacter().getPoseByName("bow_release").getSequence(0);
			if (NelNGolemToyBow.ABuf == null)
			{
				NelNGolemToyBow.ABuf = new List<M2BlockColliderContainer.BCCHitInfo>();
			}
		}

		public static NelAttackInfo getAttackInfo(ref NelAttackInfo[] AAtk, int attr, float power = 1f)
		{
			if (AAtk == null)
			{
				AAtk = new NelAttackInfo[4];
			}
			switch (attr)
			{
			case 0:
			{
				NelAttackInfo nelAttackInfo;
				if ((nelAttackInfo = AAtk[0]) == null)
				{
					nelAttackInfo = (AAtk[0] = new NelAttackInfo
					{
						hpdmg0 = X.IntC(9f * power),
						split_mpdmg = 3,
						burst_vx = 0.05f,
						huttobi_ratio = 0.002f,
						attr = MGATTR.FIRE,
						shield_break_ratio = -7f,
						parryable = false,
						nodamage_time = 0,
						SerDmg = new FlagCounter<SER>(1).Add(SER.BURNED, 25f)
					}.Torn(0.015f, 0.11f));
				}
				return nelAttackInfo;
			}
			case 1:
			{
				NelAttackInfo nelAttackInfo;
				if ((nelAttackInfo = AAtk[1]) == null)
				{
					nelAttackInfo = (AAtk[1] = new NelAttackInfo
					{
						hpdmg0 = X.IntC(9f * power),
						split_mpdmg = 1,
						burst_vx = 0.01f,
						huttobi_ratio = 0.002f,
						attr = MGATTR.ICE,
						shield_break_ratio = -7f,
						parryable = false,
						nodamage_time = 0,
						SerDmg = new FlagCounter<SER>(1).Add(SER.FROZEN, 50f)
					}.Torn(0.004f, 0.007f));
				}
				return nelAttackInfo;
			}
			case 2:
			{
				NelAttackInfo nelAttackInfo;
				if ((nelAttackInfo = AAtk[2]) == null)
				{
					nelAttackInfo = (AAtk[2] = new NelAttackInfo
					{
						hpdmg0 = X.IntC(10f * power),
						split_mpdmg = 1,
						burst_vx = 0.03f,
						huttobi_ratio = 0.002f,
						attr = MGATTR.THUNDER,
						shield_break_ratio = -7f,
						parryable = false,
						nodamage_time = 0,
						SerDmg = new FlagCounter<SER>(1).Add(SER.PARALYSIS, 30f)
					}.Torn(0.015f, 0.11f));
				}
				return nelAttackInfo;
			}
			default:
			{
				NelAttackInfo nelAttackInfo;
				if ((nelAttackInfo = AAtk[3]) == null)
				{
					NelAttackInfo[] array = AAtk;
					int num = 3;
					NelAttackInfo nelAttackInfo2 = new NelAttackInfo();
					nelAttackInfo2.hpdmg0 = 0;
					nelAttackInfo2.mpdmg0 = X.IntC(8f * power);
					nelAttackInfo2.split_mpdmg = X.IntC(7f * power);
					nelAttackInfo2.burst_vx = 0.04f;
					nelAttackInfo2.huttobi_ratio = -1000f;
					nelAttackInfo2.attr = MGATTR.ACME;
					nelAttackInfo2.shield_break_ratio = 0.0001f;
					nelAttackInfo2.parryable = false;
					nelAttackInfo2.nodamage_time = 0;
					nelAttackInfo2.SerDmg = new FlagCounter<SER>(1).Add(SER.SEXERCISE, (float)X.IntC(70f * power));
					nelAttackInfo2.EpDmg = new EpAtk(30, "beam")
					{
						canal = 8,
						cli = 2
					}.MultipleOrgasm(0.4f);
					NelAttackInfo nelAttackInfo3 = nelAttackInfo2;
					array[num] = nelAttackInfo2;
					nelAttackInfo = nelAttackInfo3;
				}
				return nelAttackInfo;
			}
			}
		}

		public static void prepareMagic(M2MagicCaster En, ref NelAttackInfo[] ALaserAtk, MagicItem Mg, int attr, float power = 1f)
		{
			if (Mg.Other != null)
			{
				return;
			}
			BList<Color32> blist = ListBuffer<Color32>.Pop(3);
			Mg.Other = blist;
			switch (attr)
			{
			case 0:
				Mg.Atk0 = NelNGolemToyBow.getAttackInfo(ref ALaserAtk, attr, power);
				Mg.Ray.HitLock(22f, null);
				blist.Add(C32.d2c(4294942222U));
				blist.Add(C32.d2c(2004157701U));
				blist.Add(C32.d2c(4282020220U));
				break;
			case 1:
				Mg.Atk0 = NelNGolemToyBow.getAttackInfo(ref ALaserAtk, attr, power);
				Mg.Ray.HitLock(28f, null);
				blist.Add(C32.d2c(4280340989U));
				blist.Add(C32.d2c(1996823413U));
				blist.Add(C32.d2c(4288439086U));
				break;
			case 2:
				Mg.Atk0 = NelNGolemToyBow.getAttackInfo(ref ALaserAtk, attr, power);
				Mg.Ray.HitLock(20f, null);
				blist.Add(C32.d2c(4293524018U));
				blist.Add(C32.d2c(2001889584U));
				blist.Add(C32.d2c(4281532888U));
				break;
			case 3:
				Mg.Atk0 = NelNGolemToyBow.getAttackInfo(ref ALaserAtk, attr, power);
				Mg.Ray.HitLock(13f, null);
				blist.Add(C32.d2c(4294138793U));
				blist.Add(C32.d2c(2863860369U));
				blist.Add(C32.d2c(4282483037U));
				break;
			}
			Mg.Atk0.Caster = En;
		}

		private void decideAttr()
		{
			if (!this.Mh.isActive(this))
			{
				return;
			}
			int num = this.bow_main_attr;
			if (this.shoot_count > 2 && X.XORSP() < 0.15f)
			{
				num = (num + 1 + X.xors(3)) % 4;
			}
			NelNGolemToyBow.prepareMagic(this, ref this.ALaserAtk, this.Mh.Mg, num, 1f);
			this.shoot_count++;
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
			Aattr.Add((MGATTR)this.bow_main_attr);
			Aattr.Add((MGATTR)this.bow_main_attr);
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
				this.Mh = new MagicItemHandlerS(magicItem);
				magicItem.Mn = this.Mn;
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
			if (this.Mp.canThroughBcc(vector.x, vector.y, vector.x + 40f * X.Cos(this.calcedR), vector.y - 40f * X.Sin(this.calcedR), 0.18f, 0.18f, -1, false, true, null, false, NelNGolemToyBow.ABuf) || NelNGolemToyBow.ABuf.Count == 0)
			{
				this.Mn._1.no_draw = true;
				this.Mn._0.v0 = (this.Mn._0.len = 40f);
				this.Mn._0.accel_maxt = 0f;
				this.Mn._1.v0 = (this.Mn._1.len = 0f);
			}
			else
			{
				M2BlockColliderContainer.BCCLine bccline;
				this.HitPos = this.calcHit(vector, out bccline);
				float housenagR = bccline.housenagR;
				float num = X.angledifR(housenagR, this.calcedR + 3.1415927f);
				this.reflecR = housenagR - num;
				this.Mn._0.v0 = (this.Mn._0.len = this.HitPos.z);
				this.Mn._0.accel_maxt = 1f;
				this.Mn._1.no_draw = false;
				if (this.Mp.canThroughBcc(this.HitPos.x + 0.29000002f * X.Cos(this.reflecR), this.HitPos.y - 0.29000002f * X.Sin(this.reflecR), this.HitPos.x + 40f * X.Cos(this.reflecR), this.HitPos.y - 40f * X.Sin(this.reflecR), 0.18f, 0.18f, -1, false, true, null, false, NelNGolemToyBow.ABuf) || NelNGolemToyBow.ABuf.Count == 0)
				{
					this.Mn._1.v0 = (this.Mn._1.len = 40f);
					this.Mn._1.accel_maxt = 0f;
				}
				else
				{
					M2BlockColliderContainer.BCCLine bccline2;
					Vector3 vector2 = this.calcHit(this.HitPos, out bccline2);
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

		private Vector3 calcHit(Vector2 Ps, out M2BlockColliderContainer.BCCLine HitBcc)
		{
			Vector3 vector = new Vector3(0f, 0f, -1f);
			HitBcc = null;
			for (int i = NelNGolemToyBow.ABuf.Count - 1; i >= 0; i--)
			{
				M2BlockColliderContainer.BCCHitInfo bcchitInfo = NelNGolemToyBow.ABuf[i];
				float num = X.LENGTHXY2(Ps.x, Ps.y, bcchitInfo.x, bcchitInfo.y);
				if (vector.z < 0f || vector.z > num)
				{
					vector.Set(bcchitInfo.x, bcchitInfo.y, num);
					HitBcc = bcchitInfo.Hit;
				}
			}
			vector.z = Mathf.Sqrt(vector.z);
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
			Vector2 vector = this.PodStartPos(this.calcedR);
			float scaleX = base.scaleX;
			float num = X.Cos(this.calcedR);
			float num2 = X.Sin(this.calcedR);
			Mg.sx = vector.x + num * 0.02f * scaleX;
			Mg.sy = vector.y - num2 * 0.02f * scaleX;
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
			int num = NelNGolemToyBow.MgRunArrowLaser(Mg, fcnt, 10f);
			if (num == 2)
			{
				this.MgReposition(Mg);
				this.calcArrowDirection(Mg, false);
			}
			return num >= 1;
		}

		public static int MgRunArrowLaser(MagicItem Mg, float fcnt, float fine_intv = 10f)
		{
			int num = 1;
			if (Mg.phase == 0)
			{
				Mg.Ray.check_hit_wall = false;
				Mg.Ray.hittype |= (HITTYPE)16777280;
				Mg.phase = 1;
				Mg.efpos_s = (Mg.raypos_s = (Mg.aimagr_calc_s = true));
				Mg.projectile_power = 50;
				Mg.Ray.check_other_hit = true;
				Mg.Ray.Atk = Mg.Atk0;
				Mg.projectile_power = 100;
				Mg.sz = 0f;
			}
			if (Mg.phase >= 1 && Mg.Mn != null)
			{
				Mg.aim_agR = Mg.Mn._0.agR;
				Mg.calced_aim_pos = true;
			}
			if (Mg.phase >= 2)
			{
				if (Mg.sz <= 0f)
				{
					num = 2;
					Mg.sz = fine_intv;
				}
				Mg.sz -= fcnt;
			}
			if (Mg.phase == 3)
			{
				if (Mg.Mn == null)
				{
					return 0;
				}
				float num2 = Mg.sx;
				float num3 = Mg.sy;
				for (int i = 0; i < Mg.Mn.Count; i++)
				{
					MagicNotifiear.MnHit hit = Mg.Mn.GetHit(i);
					if (hit.len <= 0f)
					{
						break;
					}
					Mg.Ray.RadiusM(hit.thick);
					Mg.Ray.PosMap(num2, num3);
					Mg.Mn.SetRay(Mg.Ray, i, hit.agR, 0f);
					Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir(Mg.Ray.Dir.x > 0f), HITTYPE.NONE);
					if (i >= Mg.Mn.Count - 1)
					{
						break;
					}
					num2 += hit.len * X.Cos(hit.agR);
					num3 += -hit.len * X.Sin(hit.agR);
				}
			}
			if (Mg.phase != 4)
			{
				return num;
			}
			if (Mg.t >= 20f)
			{
				return 0;
			}
			return num;
		}

		private bool MgDraw(MagicItem Mg, float fcnt)
		{
			return this.Mp != null && NelNGolemToyBow.MgDrawArrow(Mg, fcnt, false);
		}

		public static bool MgDrawArrow(MagicItem Mg, float fcnt, bool charge_draw_top = false)
		{
			Map2d mp = Mg.Mp;
			M2MagicCaster caster = Mg.Atk0.Caster;
			MeshDrawer meshDrawer;
			if (Mg.phase == 2)
			{
				meshDrawer = Mg.Ef.GetMesh("", MTRX.MtrMeshNormal, !charge_draw_top);
				Mg.Mn.drawTo(meshDrawer, mp, new Vector2(Mg.sx, Mg.sy), Mg.t, 0f, false, Mg.t, null, null);
				return true;
			}
			if (Mg.phase != 3 && Mg.phase != 4)
			{
				return true;
			}
			BList<Color32> blist = Mg.Other as BList<Color32>;
			if (blist == null)
			{
				return true;
			}
			meshDrawer = Mg.Ef.GetMesh("", (Mg.phase == 3) ? MTR.MtrBeam : MTRX.MtrMeshNormal, false);
			MeshDrawer mesh = Mg.Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.ADD, -1), false);
			mesh.initForImg(MTRX.EffBlurCircle245, 0);
			if (meshDrawer.getTriMax() == 0 && Mg.phase == 3)
			{
				meshDrawer.base_z = mesh.base_z + 0.002f;
			}
			meshDrawer.base_x = (meshDrawer.base_y = (mesh.base_x = (mesh.base_y = 0f)));
			float agR = Mg.Mn._0.agR;
			meshDrawer.Rotate(agR, false).Translate(mp.map2globalux(Mg.sx), mp.map2globaluy(Mg.sy), false);
			Color color = blist[1];
			Color32 color2 = blist[0];
			C32 c = null;
			float num2;
			float num3;
			if (Mg.phase == 3)
			{
				float num = X.ZSIN2(Mg.t, 24f);
				if (num < 1f)
				{
					color = meshDrawer.ColGrd.Set(blist[1]).blend(uint.MaxValue, 1f - num).C;
				}
				color2 = meshDrawer.ColGrd.Set(color2).blend(uint.MaxValue, 1f - num).C;
				num2 = 1f + X.COSI(mp.floort, 7.33f) * 0.3f + X.COSI(mp.floort, 3.21f) * 0.22f;
				num3 = 11f * num2 * X.NIL(1f, 0.6f, Mg.t - 30f, Mg.Mn._0.accel_mint - 30f) + 43f * (1f - num);
				mesh.Col = mesh.ColGrd.Set(color2).blend(color, 0.5f + 0.3f * X.COSI(mp.floort, 14.33f) + 0.2f * X.COSI(mp.floort, 8.11f)).setA1(1f)
					.C;
			}
			else
			{
				float num = X.ZSIN2(Mg.t, 20f);
				color = meshDrawer.ColGrd.Set(blist[1]).blend(uint.MaxValue, 1f - 0.6f * num).C;
				color2 = meshDrawer.ColGrd.Set(color2).blend(uint.MaxValue, 1f - 0.3f * num).C;
				c = meshDrawer.ColGrd.Set(color);
				num2 = 1.5f * (1f - num);
				num3 = 23f * num2;
				mesh.Col = mesh.ColGrd.Set(color2).mulA(1f - num).C;
			}
			num3 *= 0.015625f;
			if (num3 <= 0f)
			{
				return true;
			}
			if (Mg.Mn._0.thick != 0.18f)
			{
				num3 *= Mg.Mn._0.thick / 0.18f;
			}
			meshDrawer.Uv2(color.r, color.g, true).Uv3(color.b, color.a, true);
			mesh.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
			NelNGolemToyBow.drawCirc(mesh, 80f, num2);
			MagicNotifiear.MnHit mnHit = Mg.Mn.GetHit(0);
			float num4 = Mg.sx;
			float num5 = Mg.sy;
			float num6 = Mg.Mn._0.len * mp.CLENB * 0.015625f;
			for (int i = 0; i < Mg.Mn.Count; i++)
			{
				meshDrawer.Col = color2;
				meshDrawer.TriRectBL(0).Tri(4, 0, 3, false).Tri(4, 3, 5, false);
				meshDrawer.uvRectN(0f, 0.5f).Pos(0f, 0f, null);
				meshDrawer.uvRectN(0f, 1f).Pos(0f, num3, c);
				meshDrawer.uvRectN(1f, 1f).Pos(num6, num3, c);
				meshDrawer.uvRectN(1f, 0.5f).Pos(num6, 0f, null);
				meshDrawer.uvRectN(0f, 0f).Pos(0f, -num3, c);
				meshDrawer.uvRectN(1f, 0f).Pos(num6, -num3, c);
				bool flag = i >= Mg.Mn.Count - 1;
				if (flag && mnHit.accel_maxt == 0f)
				{
					break;
				}
				num4 += mnHit.len * X.Cos(mnHit.agR);
				num5 += -mnHit.len * X.Sin(mnHit.agR);
				meshDrawer.Identity();
				meshDrawer.Translate(mp.map2globalux(num4), mp.map2globaluy(num5), false);
				mesh.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
				if (mnHit.accel_maxt > 0f)
				{
					NelNGolemToyBow.drawCirc(mesh, 80f * mnHit.accel_maxt, num2);
				}
				if (flag)
				{
					break;
				}
				MagicNotifiear.MnHit hit = Mg.Mn.GetHit(i + 1);
				if (hit.len <= 0f)
				{
					break;
				}
				meshDrawer.Rotate(hit.agR, true);
				mesh.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
				num6 = hit.len * mp.CLENB * 0.015625f;
				mnHit = hit;
			}
			meshDrawer.allocUv2(0, true).allocUv3(0, true);
			return true;
		}

		private static void drawCirc(MeshDrawer MdA, float circr, float tzr)
		{
			for (int i = 0; i <= 2; i++)
			{
				float num = circr + (1f + 0.3f * (float)i);
				MdA.Rect(0f, 0f, num * tzr, num * tzr, false);
			}
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
			Md.Col = this.Anm.getTeMulColor().C;
			BList<Color32> blist = (this.Mh.isActive(this) ? (this.Mh.Mg.Other as BList<Color32>) : null);
			if (this.pod_cframe_ <= 0 || blist == null)
			{
				Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, uint.MaxValue, false, 0);
				return;
			}
			Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, 1U, false, 0);
			Md.Col = blist[0];
			Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, 2U, false, 0);
			Md.Col = this.Anm.getTeMulColor().C;
			Md.RotaPF(num, num2, 1f, 1f, this.shotR, pxlFrame, false, true, false, 4294967292U, false, 0);
		}

		private float shotR;

		private PxlSequence SqPod;

		private PxlSequence SqPodClose;

		public const int ATTR_MAX = 4;

		private int pod_cframe_;

		private int bow_main_attr;

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

		private M2SoundPlayerItem SndRot;

		private MagicItemHandlerS Mh;

		private int shoot_count;

		private MagicItem.FnMagicRun FD_MgRun;

		private MagicItem.FnMagicRun FD_MgDraw;

		private static Texture mainTexture;

		private static List<M2BlockColliderContainer.BCCHitInfo> ABuf;

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
			penetrate = true,
			aim_fixed = true
		}).AddHit(new MagicNotifiear.MnHit
		{
			type = MagicNotifiear.HIT.CIRCLE,
			maxt = 1f,
			aim_fixed = true,
			thick = 0.18f,
			accel = 0f,
			v0 = 0f,
			penetrate = true
		});

		public const int PRI_WALK = 10;

		public const int PRI_ATK = 128;

		private enum CRES
		{
			PR = 1,
			ME,
			OTHER = 4
		}
	}
}
