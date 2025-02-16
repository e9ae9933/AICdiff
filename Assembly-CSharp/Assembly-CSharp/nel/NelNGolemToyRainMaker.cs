using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelNGolemToyRainMaker : NelNGolemToy
	{
		public override int create_count_normal
		{
			get
			{
				return 9;
			}
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.Nai.fnSearchPlayer = new NAI.FnSearchPlayer(NAI.SearchNoel);
			this.apply_len = X.NI(4, 8, base.mp_ratio);
			this.hinge_len = X.NI(5, 7, base.mp_ratio);
			this.orbit_size = X.NI(0.6f, 1.2f, base.mp_ratio);
			this.FD_MgRun = new MagicItem.FnMagicRun(this.MgRun);
			this.FD_MgDraw = new MagicItem.FnMagicRun(this.MgDraw);
			this.MtrBase = this.Anm.getMainMtr();
			this.FD_BreakerDropObject = new M2DropObject.FnDropObjectDraw(this.BreakerDropObject);
			this.SqOrbit = this.Anm.getCurrentCharacter().getPoseByName("rainmaker_orbit").getSequence(0);
		}

		protected override void initBorn()
		{
			base.initBorn();
			this.cannot_move = true;
			this.Nai.AddF(NAI.FLAG.GAZE, 180f);
			this.Anm.setPose("rainmaker", 1);
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			this.stopSndWalk();
			if (this.Hinge != null)
			{
				base.M2D.DeassignPauseable(this.OrbitRgd);
				this.Hinge.destruct();
				IN.DestroyOne(this.OrbitRgd.gameObject);
			}
			base.destruct();
		}

		protected override bool considerNormal(NAI Nai)
		{
			if (!base.create_finished)
			{
				return true;
			}
			if (Nai.HasF(NAI.FLAG.GAZE, true))
			{
				return Nai.AddTicketB(NAI.TYPE.GAZE, 128, true);
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
			bool create_finished = base.create_finished;
		}

		public override bool readTicket(NaTicket Tk)
		{
			if (!base.create_finished)
			{
				return false;
			}
			NAI.TYPE type = Tk.type;
			if (type == NAI.TYPE.PUNCH)
			{
				return this.runPunch(Tk.initProgress(this), Tk);
			}
			if (type - NAI.TYPE.GAZE <= 1)
			{
				return this.runGaze(Tk.initProgress(this), Tk);
			}
			return base.readTicket(Tk);
		}

		public override void quitTicket(NaTicket Tk)
		{
			if (this.Hinge != null)
			{
				this.Hinge.enabled = false;
			}
			this.Phy.walk_xspeed = 0f;
			this.stopSndWalk();
			base.quitTicket(Tk);
		}

		private void createSndWalk()
		{
			if (this.SndWalk == null)
			{
				this.SndWalk = this.playSndPos("golemtoy_rm_gas", 2);
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

		public bool runGaze(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
			}
			return this.t < 200f;
		}

		public static void initHinge(M2Mover BaseMv, Map2d curMap, Rigidbody2D BaseRgd, Vector2 TargetShiftPosU, float hinge_map_len, float orbit_size, ref HingeCreator Hinge, ref Rigidbody2D OrbitRgd)
		{
			if (Hinge == null)
			{
				Hinge = new HingeCreator(BaseMv.key, curMap.M2D.getPauser());
				GameObject gameObject = new GameObject(BaseMv.key + "_orbit");
				gameObject.layer = IN.LAY("ChipsUCol");
				gameObject.transform.SetParent(curMap.gameObject.transform, false);
				gameObject.transform.localPosition = BaseMv.transform.localPosition + TargetShiftPosU;
				Hinge.BaseHingeAnchor = TargetShiftPosU;
				OrbitRgd = gameObject.AddComponent<Rigidbody2D>();
				OrbitRgd.sharedMaterial = MTRX.PmdM2Bouncy;
				Hinge.mass = 0.0001f;
				curMap.M2D.AssignPauseable(OrbitRgd);
				float num = orbit_size * curMap.CLENB * 0.015625f;
				gameObject.AddComponent<CircleCollider2D>().radius = num * 0.8f;
				Hinge.BE(BaseRgd, OrbitRgd);
				Hinge.use_max_range = false;
				Hinge.link_enable_end = true;
				Hinge.enabled = false;
				Hinge.max_length = hinge_map_len * curMap.CLENB * 0.015625f;
				Hinge.CreateGob();
				return;
			}
			Hinge.BaseHingeAnchor = TargetShiftPosU;
			Hinge.PosReset();
			Hinge.enabled = false;
		}

		public bool runPunch(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				this.t = 0f;
				if (base.AimPr as PRNoel == null)
				{
					return false;
				}
				NelNGolemToyRainMaker.initHinge(this, this.Mp, this.Phy.getRigidbody(), this.TargetShiftPosU, this.hinge_len, this.orbit_size, ref this.Hinge, ref this.OrbitRgd);
				this.OrbitRgd.mass = this.Hinge.mass * 10f;
				this.Hinge.end_gravity_scale = base.base_gravity * 0.2f;
				this.Anm.setPose("rainmaker_charge", -1);
				base.PtcST("golemtoy_rm_shot_charge", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
				if (this.Mg != null)
				{
					this.Mg.kill(-1f);
					this.Mg = null;
				}
			}
			if (Tk.prog == PROG.ACTIVE && Tk.Progress(ref this.t, 100, true))
			{
				this.Hinge.enabled = true;
				this.Anm.setPose("rainmaker_shot", -1);
				base.PtcST("golemtoy_rm_shot", PtcHolder.PTC_HOLD.NO_HOLD, PTCThread.StFollow.FOLLOW_T);
				this.Mg = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, base.mg_hit | MGHIT.IMMEDIATE).initFunc(this.FD_MgRun, this.FD_MgDraw);
				this.Mg.run(0f);
				this.walk_time = 0f;
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.Mg != null)
				{
					PRNoel prnoel = base.AimPr as PRNoel;
					if (prnoel != null)
					{
						this.walk_time -= this.TS;
						if (this.walk_time <= 0f || prnoel.hasJamming())
						{
							this.walk_time = 30f;
							float current_apply_len = this.current_apply_len;
							if (prnoel != null && current_apply_len > 0f)
							{
								float num;
								float num2;
								prnoel.getMouthPosition(out num, out num2);
								if (X.chkLEN(this.Mg.sx, this.Mg.sy, num, num2, current_apply_len + 0.25f))
								{
									prnoel.addJamming();
								}
							}
						}
					}
				}
				else
				{
					this.Hinge.enabled = false;
					if (this.t >= 130f)
					{
						Tk.AfterDelay(140f);
						return false;
					}
				}
			}
			return true;
		}

		public override Vector2 getTargetPos()
		{
			return new Vector2(base.x, base.y - base.scaleY * this.target_hgt_);
		}

		public Vector2 TargetShiftPosU
		{
			get
			{
				return new Vector2(0f, base.scaleY * this.target_hgt_ * this.Mp.CLEN * 0.015625f);
			}
		}

		public float target_hgt
		{
			get
			{
				return this.target_hgt_;
			}
			set
			{
				if (this.target_hgt == value)
				{
					return;
				}
				this.target_hgt_ = value;
				if (this.Hinge != null)
				{
					this.Hinge.BaseHingeAnchor = this.TargetShiftPosU;
				}
			}
		}

		private void MgSpdToRgd()
		{
			if (this.Hinge != null && this.Mg != null && this.Mp != null)
			{
				this.OrbitRgd.velocity = new Vector2(this.Mp.mapvxToUVelocityx(this.Mg.dx * this.base_TS), this.Mp.mapvyToUVelocityy(this.Mg.dy * this.base_TS));
			}
		}

		private void RgdToMgSpd()
		{
			if (this.Hinge != null && this.Mg != null && this.Mp != null)
			{
				Vector2 velocity = this.OrbitRgd.velocity;
				Vector2 vector = this.OrbitRgd.transform.localPosition;
				this.Mg.dx = this.Mp.uVelocityxToMapx(velocity.x / this.base_TS);
				this.Mg.dy = this.Mp.uVelocityyToMapy(velocity.y / this.base_TS);
				this.Mg.sx = this.Mp.uxToMapx(vector.x);
				this.Mg.sy = this.Mp.uyToMapy(vector.y);
			}
		}

		private bool MgRun(MagicItem Mg, float fcnt)
		{
			Map2d mp = Mg.Mp;
			float num = Map2d.TS * this.base_TS;
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.Ray.RadiusM(this.orbit_size).HitLock(40f, null);
				Mg.Ray.projectile_power = -50;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.Ray.LenM(0.125f);
				Mg.Ray.check_hit_wall = true;
				Mg.Ray.check_other_hit = false;
				Mg.calcAimPos(false);
				Vector2 targetPos = this.getTargetPos();
				Mg.sx = targetPos.x;
				Mg.sy = targetPos.y;
				Mg.sz = 0f;
				Mg.sa = 0f;
				Mg.dx = 0f;
				Mg.dy = -0.12f;
				Mg.dz = 0f;
				Mg.da = 1.5707964f;
				Mg.efpos_s = (Mg.raypos_s = true);
				Mg.aimagr_calc_vector_d = true;
				this.Hinge.end_gravity_scale = base.base_gravity * 0.2f;
				this.MgSpdToRgd();
			}
			else
			{
				if (Mg.Ray == null)
				{
					return false;
				}
				if (this.Mp == null || base.isStunned())
				{
					if (Mg.phase < 21)
					{
						Mg.Ray.RadiusM(this.orbit_size * 0.5f);
						this.stopSndWalk();
						float num2 = mp.GAR(base.x, base.y, Mg.sx, Mg.sy);
						Mg.dx += 0.06f * X.Cos(num2);
						Mg.dy -= 0.03f * X.Sin(num2);
						Mg.killEffect();
						Mg.phase = 21;
						Mg.t = 0f;
					}
				}
				else
				{
					this.RgdToMgSpd();
				}
				if (Mg.phase >= 20 && this.SndWalk != null)
				{
					this.stopSndWalk();
					Mg.Ray.RadiusM(this.orbit_size * ((Mg.phase == 21) ? 0.5f : 1f));
					Mg.killEffect();
				}
				float num3 = 0f;
				float num4 = 0f;
				if (Mg.phase == 1)
				{
					Mg.sz += (1f - X.ZLINE(Mg.t, 70f)) / 2f;
					this.Hinge.end_gravity_scale = base.base_gravity * 0.4f;
					num *= 20f;
					if (Mg.t >= 70f)
					{
						Mg.t = 0f;
						Mg.phase = 2;
					}
					else
					{
						num4 = -0.12f * (1f - 0.96f * X.ZLINE(Mg.t, 70f));
					}
				}
				if (Mg.phase == 2 || Mg.phase == 3)
				{
					this.createSndWalk();
					if (Mg.phase == 2)
					{
						Mg.phase = 3;
						this.Hinge.end_gravity_scale = base.base_gravity * 0.12f;
						Mg.Ray.RadiusM(this.orbit_size);
						Mg.PtcST("golemtoy_rm_orbit", PTCThread.StFollow.FOLLOW_S, false);
						Mg.PtcVar("saf", 50.0).PtcVar("size", (double)(this.apply_len * mp.CLENB)).PtcST("golemtoy_rm_orbit_aula", PTCThread.StFollow.FOLLOW_S, false);
						Mg.t = 0f;
						Mg.sa = 0f;
					}
					Mg.sz += X.ZLINE(Mg.t, 90f) / 4f;
					num *= 0.08f * (0.02f + 0.98f * X.ZLINE(Mg.t, 60f));
					float num5 = mp.GAR(Mg.sx, Mg.sy, this.Nai.target_x, this.Nai.target_y);
					float num6 = X.angledifR(Mg.da, num5);
					Mg.sa = base.VALWALK(Mg.sa, (float)X.MPF(num6 > 0f) * 0.011f * 3.1415927f, 0.001256637f);
					Mg.da += Mg.sa;
					float num7 = this.Nai.target_x - X.Cos(num5) * (this.apply_len * 0.3f);
					float num8 = this.Nai.target_y + X.Mn(X.Sin(num5) * 3f - 1f, -this.apply_len * 0.25f);
					float num9 = mp.GAR(Mg.sx, Mg.sy, num7, num8);
					num3 = X.Cos(num9) * 0.27f;
					num4 = -X.Sin(num9) * 0.11f;
				}
				if (Mg.phase == 20)
				{
					Mg.sz += (1f - X.ZLINE(Mg.t, 40f)) / 2f;
					Mg.da += (float)X.MPF(Mg.dx < 0f) * 0.034f * 3.1415927f * (1f - 0.8f * X.ZLINE(Mg.t, 40f));
					num *= 0.05f;
					if (Mg.t >= 40f)
					{
						Mg.t = 0f;
						Mg.phase = 2;
						Mg.dz = (Mg.sa = 0f);
						this.OrbitRgd.mass = this.Hinge.mass * 10f;
					}
				}
				if (Mg.phase == 21)
				{
					Mg.da += (float)X.MPF(Mg.dx < 0f) * (0.037699115f + X.Abs(Mg.sa));
					Mg.dy += num * 0.008f;
					Mg.sx += Mg.dx;
					Mg.sy += Mg.dy;
					num = 0f;
				}
				if (num > 0f)
				{
					Mg.dx = X.VALWALK(Mg.dx, num3, num * 0.06f);
					Mg.dy = X.VALWALK(Mg.dy, num4, num * 0.08f);
					Vector2 targetPos2 = this.getTargetPos();
					if (!X.chkLEN(targetPos2.x, targetPos2.y, Mg.sx + Mg.dx, Mg.sy + Mg.dy, this.hinge_len))
					{
						float num10 = mp.GAR(Mg.sx + Mg.dx, Mg.sy + Mg.dy, targetPos2.x, targetPos2.y);
						Mg.dx += X.Cos(num10) * 0.088f;
						Mg.dy -= X.Sin(num10) * 0.088f;
					}
					this.MgSpdToRgd();
					float num11 = -0.1875f;
					this.Hinge.LastHingeAnchor = new Vector2(num11 * X.Cos(Mg.da), num11 * X.Sin(Mg.da));
				}
				Mg.calcAimPos(false);
				Mg.MnSetRay(Mg.Ray, 0, Mg.da, 0f);
				HITTYPE hittype = Mg.Ray.Cast(true, null, false);
				if ((hittype & (HITTYPE)4194336) != HITTYPE.NONE && Mg.phase <= 20)
				{
					Mg.phase = 20;
					Mg.t = 0f;
					if (Mg.reflectAgR(Mg.Ray, ref Mg.aim_agR, 0.25f))
					{
						Mg.dx += X.Cos(Mg.aim_agR) * 0.13f;
						Mg.dy += X.Abs(X.Sin(Mg.aim_agR) * 0.13f);
					}
					this.MgSpdToRgd();
					this.OrbitRgd.mass = 0.1f;
					this.Hinge.end_gravity_scale = base.base_gravity * 0.8f;
				}
				if ((hittype & HITTYPE.WALL) != HITTYPE.NONE && Mg.phase >= 20)
				{
					this.stopSndWalk();
					this.Mg = null;
					Mg.PtcVar("cx", (double)Mg.sx).PtcVar("cy", (double)Mg.sy).PtcVar("size", (double)(1.2f * base.scaleX))
						.PtcST("machine_die", PTCThread.StFollow.NO_FOLLOW, false);
					M2DropObjectReader.GetAndSet(Mg.Mp, "golemtoy_orbit_break", Mg.sx, Mg.sy, 1f, 1f, 0f, this.FD_BreakerDropObject, this.SqOrbit.getFrame(0));
					return false;
				}
			}
			return true;
		}

		private bool MgDraw(MagicItem Mg, float fcnt)
		{
			EffectItem ef = Mg.Ef;
			MeshDrawer mesh = ef.GetMesh("", this.MtrBase, false);
			if (mesh.getTriMax() == 0)
			{
				mesh.base_z -= 0.01f;
			}
			mesh.Scale(base.scaleX, base.scaleY, false);
			mesh.initForImg(this.SqOrbit.getImage((int)(Mg.sz % 4f), 0), 0).RotaGraph(0f, 0f, 1f, Mg.da, null, false);
			MeshDrawer mesh2 = ef.GetMesh("", uint.MaxValue, BLEND.ADD, true);
			float base_x = mesh2.base_x;
			float base_y = mesh2.base_y;
			Vector2 vector;
			if (this.Hinge.enabled && this.Hinge.DrawBegin(out vector))
			{
				mesh2.Col = C32.d2c(2013265919U);
				mesh2.base_x = 0f;
				mesh2.base_y = 0f;
				mesh2.Col = MTRX.ColWhite;
				Vector2 vector2;
				while (this.Hinge.DrawNext(out vector2))
				{
					mesh2.Line(vector.x, vector.y, vector2.x, vector2.y, 0.015625f, true, 0f, 0f);
					vector = vector2;
				}
			}
			mesh2.base_x = base_x;
			mesh2.base_y = base_y;
			if (Mg.phase == 3)
			{
				MeshDrawer mesh3 = ef.GetMesh("", 2583691263U, BLEND.SUB, true);
				if (this.Rpl == null)
				{
					this.Rpl = new ShockRippleDrawer(MTRX.SqEfPattern.getImage(0, 0), MTR.MIiconP);
					this.Rpl.DivideCount(1);
					this.Rpl.thick_randomize = 0.88f;
					this.Rpl.radius_randomize_px = 4f;
					this.Rpl.gradation_focus = -1f;
				}
				int num = (int)(Mg.t / 6f);
				if (Mg.dz <= (float)num)
				{
					Mg.dz = (float)(num + 1);
					this.Rpl.Ran(X.GETRAN2(num, 2 + num % 4));
				}
				uint ran = this.Rpl.ran0;
				mesh3.Col = mesh3.ColGrd.Set(1999440851).blend(2010405643U, 0.5f).C;
				mesh2.Col = mesh2.ColGrd.Set(3436909485U).blend(3426343564U, 0.5f).C;
				float num2 = X.ZSIN(Mg.t, 30f);
				float num3 = this.current_apply_len * this.Mp.CLENB;
				this.Rpl.drawTo(mesh3, 0f, 0f, num3, 12f * num2, 0f, 0f);
				this.Rpl.drawTo(mesh2, 0f, 0f, num3, 8f * num2, 0f, 0f);
			}
			return true;
		}

		public float current_apply_len
		{
			get
			{
				if (this.Mg == null || this.Mp == null || this.Mg.phase != 3)
				{
					return 0f;
				}
				return X.ZSIN(this.Mg.t, 120f) * this.apply_len;
			}
		}

		public override void addTortureUIFadeKeyFoGO(List<string> A, List<MGATTR> Aattr)
		{
		}

		public override bool runStun()
		{
			if (this.t <= 0f)
			{
				this.SpSetPose("rainmaker_stun", -1, null, false);
			}
			return base.runStun();
		}

		public bool BreakerDropObject(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			return EnemyAnimator.BreakerDropObject(Dro, Ef, Ed, this.MtrBase);
		}

		public override void makeBone(List<Vector3> ABone)
		{
			float num = 0.9f * base.CLEN;
			int num2 = 2;
			float num3 = -0.5f * num;
			for (int i = 0; i < num2; i++)
			{
				ABone.Add(new Vector3(num3, 6f, (float)(i * 14)));
				num3 += num / (float)(num2 - 1);
			}
		}

		public MagicItem.FnMagicRun FD_MgRun;

		public MagicItem.FnMagicRun FD_MgDraw;

		private Rigidbody2D OrbitRgd;

		private M2SoundPlayerItem SndWalk;

		private HingeCreator Hinge;

		private MagicItem Mg;

		private PxlSequence SqOrbit;

		private float apply_len;

		private float hinge_len;

		private float orbit_size;

		private float target_hgt_ = 0.73f;

		private const float debuff_apply_maxt = 120f;

		private M2DropObject.FnDropObjectDraw FD_BreakerDropObject;

		private const float mass_ratio_default = 10f;

		protected Material MtrBase;

		public const int PRI_ATK = 128;

		public const int PRI_WALK = 5;

		private ShockRippleDrawer Rpl;
	}
}
