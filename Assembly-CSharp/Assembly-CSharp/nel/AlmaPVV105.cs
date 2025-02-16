using System;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class AlmaPVV105 : M2CastableEventBase, IEventWaitListener
	{
		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			this.casting_time_scale = 0.56f;
			Mp.M2D.loadMaterialSnd("m2d_puzzle_timer");
		}

		public override void destruct()
		{
			if (this.EfC != null)
			{
				this.EfC.destruct();
				this.EfN.destruct();
				this.EfC = (this.EfN = null);
			}
			if (this.Lp != null)
			{
				this.Lp.destruct();
				this.Lp = null;
			}
			base.destruct();
		}

		protected override bool initTarget()
		{
			return false;
		}

		private void changeState(AlmaPVV105.STATE st)
		{
			AlmaPVV105.STATE state = this.stt;
			if (this.Lp == null)
			{
				return;
			}
			this.stt = st;
			this.t = 0f;
			this.walk_st = 0;
			if (st == AlmaPVV105.STATE.MAIN)
			{
				this.Par.setAim((base.x < this.Lp.mapcx) ? AIM.R : AIM.L, false);
				return;
			}
			if (st != AlmaPVV105.STATE.SEEING_NOEL)
			{
				return;
			}
			base.abortMagic();
			this.Par.quitMoveScript(true);
			this.t = 300f - X.NIXP(20f, 33f);
			this.Par.SpSetPose("stand", -1, null, false);
		}

		private float targetter_x
		{
			get
			{
				if (this.Lp == null)
				{
					return base.x;
				}
				return X.NI(this.x0, (float)(this.Lp.mapx + this.Lp.mapw), 0.7f);
			}
		}

		private float targetter_y
		{
			get
			{
				if (this.Lp == null)
				{
					return base.mbottom;
				}
				return (float)(this.Lp.mapy + this.Lp.maph);
			}
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (this.stt == AlmaPVV105.STATE.PREPARE)
			{
				if (this.Anm.isCharacterLoadFinished())
				{
					this.x0 = base.x;
					this.Lp = new M2LpMatoate("almapvv105_matoate", -1, this.Mp.getLayer(0));
					float num = base.mbottom - 6f;
					float num2 = base.x + 4f;
					this.Lp.Set((num2 - 4f) * base.CLEN, (num - 6f) * base.CLEN, 8f * base.CLEN, 12f * base.CLEN);
					this.SqTargetter = this.Anm.getCurrentCharacter().getPoseByName("_ground_targetter").getSequence(0);
					this.EfC = this.Mp.getEffectForChip().setEffectWithSpecificFn("almapvv105_targetter", -100f, 0f, 0f, 0, 0, new FnEffectRun(this.FnDrawTargetter));
					this.EfN = this.Mp.getEffect().setEffectWithSpecificFn("almapvv105_targetter_N", -100f, 0f, 0f, 0, 0, new FnEffectRun(this.FnDrawTargetter));
					this.movescript_walk = string.Concat(new string[]
					{
						"@x[",
						this.targetter_x.ToString(),
						"] P[walk~] >[ ",
						((this.targetter_x - 0.4f) * this.Mp.CLEN).ToString(),
						",",
						(base.y * this.Mp.CLEN).ToString(),
						":80 ] P[stand~]"
					});
					this.movescript_walk_back = string.Concat(new string[]
					{
						"@x[",
						this.targetter_x.ToString(),
						"] ! P[walk~] >[ ",
						(this.x0 * this.Mp.CLEN).ToString(),
						",",
						(base.y * this.Mp.CLEN).ToString(),
						":80 ] P[stand~]?"
					});
					this.changeState(AlmaPVV105.STATE.MAIN);
					this.t = 300f - X.NIXP(120f, 180f);
				}
			}
			else if (this.stt == AlmaPVV105.STATE.MAIN || this.stt == AlmaPVV105.STATE.MAIN_MTAT_FINISHED)
			{
				if (this.walk_st == 0 && this.t >= 300f)
				{
					this.walk_st = 3;
					this.initMatoate();
					this.t = 300f - X.NIXP(110f, 190f);
				}
				if (this.walk_st == 1)
				{
					if (!this.Par.isMoveScriptActive(false))
					{
						if (this.t >= 80f)
						{
							this.initMatoate();
							this.t = 0f;
							this.walk_st = 2;
							this.Par.assignMoveScript(this.movescript_walk_back, false);
						}
					}
					else
					{
						this.t = 0f;
					}
				}
				if (this.walk_st == 2)
				{
					if (!this.Par.isMoveScriptActive(false))
					{
						this.walk_st = 3;
						this.t = 300f - X.NIXP(60f, 90f);
					}
					else
					{
						this.t = 0f;
					}
				}
				if (this.walk_st == 3 && this.t >= 300f)
				{
					if (this.stt == AlmaPVV105.STATE.MAIN_MTAT_FINISHED)
					{
						this.changeState(AlmaPVV105.STATE.SEEING_NOEL);
						this.t = 300f;
					}
					else
					{
						M2MatoateTarget m2MatoateTarget = ((this.MtatPlayer != null) ? this.MtatPlayer.GetTarget(0) : null);
						if (m2MatoateTarget != null)
						{
							this.EfN.x = m2MatoateTarget.x;
							this.EfN.y = m2MatoateTarget.y;
							this.t = 0f;
							this.walk_st = 4;
							this.Par.setAim((base.x < this.Lp.mapcx) ? AIM.R : AIM.L, false);
							base.initChant();
							this.SpSetPose("chant", -1, null, false);
						}
						else
						{
							this.changeState(AlmaPVV105.STATE.SEEING_NOEL);
						}
					}
				}
				if (this.walk_st == 4 && base.magic_explodable())
				{
					this.t = 0f;
					this.walk_st = 5;
					this.SpSetPose("chant2magic", -1, null, false);
				}
				if (this.walk_st == 5 && this.t >= 24f)
				{
					base.explodeMagic();
					this.SpSetPose("magic", -1, null, false);
					this.walk_st = 6;
					this.t = 0f;
				}
				if (this.walk_st == 6 && this.t >= 94f)
				{
					this.Par.setAim((base.x < this.EfN.x) ? AIM.R : AIM.L, false);
					this.SpSetPose("stand", -1, null, false);
					this.walk_st = 7;
					this.t = 265f;
				}
				if (this.walk_st == 7 && this.t >= 300f)
				{
					if (this.stt == AlmaPVV105.STATE.MAIN_MTAT_FINISHED)
					{
						this.changeState(AlmaPVV105.STATE.SEEING_NOEL);
						this.t = 300f;
					}
					else if (this.MtatPlayer == null)
					{
						if (EV.isWaiting(this))
						{
							this.t = 300f - X.NIXP(130f, 210f);
							this.walk_st = 0;
						}
						else
						{
							this.t = 0f;
							this.walk_st = 1;
							this.Par.assignMoveScript(this.movescript_walk, false);
						}
					}
					else
					{
						this.t = 300f - X.NIXP(60f, 100f);
						this.walk_st = 3;
					}
				}
			}
			else if (this.stt == AlmaPVV105.STATE.SEEING_NOEL && this.t >= 300f)
			{
				this.t = 300f - X.NIXP(30f, 45f);
				if (this.Mp.Pr != null && !EV.isActive(false))
				{
					this.Par.setAim((base.x < this.Mp.Pr.x) ? AIM.R : AIM.L, false);
					if (X.Abs(this.Mp.Pr.x - base.x) >= 7f && X.XORSP() < 0.2f)
					{
						this.changeState(AlmaPVV105.STATE.MAIN);
						if (this.MtatPlayer == null)
						{
							this.walk_st = 1;
							this.Par.assignMoveScript(this.movescript_walk, false);
						}
						else
						{
							this.t = 270f;
						}
					}
				}
			}
			if (this.EfC != null && this.EfC.z != 0f)
			{
				float num3 = 0f;
				if (this.EfC.z == 1f)
				{
					num3 = 1f - X.ZLINE(this.EfC.af, 40f);
					if (num3 == 0f)
					{
						this.EfC.z = 0f;
					}
				}
				else if (this.EfC.z == 2f)
				{
					num3 = X.ZLINE(this.EfC.af, 30f);
				}
				if (num3 > 0f)
				{
					this.EfC.y += this.TS * num3 * 0.0027027028f;
					if (this.EfC.y >= 1f)
					{
						this.EfC.y -= 1f;
					}
				}
				this.EfC.x = num3;
				if (this.MtatPlayer != null)
				{
					M2MatoateTarget target = this.MtatPlayer.GetTarget(0);
					if (target != null)
					{
						this.EfN.x = X.VALWALK(this.EfN.x, target.x, this.TS * 0.08f);
						this.EfN.y = X.VALWALK(this.EfN.y, target.y, this.TS * 0.08f);
					}
				}
			}
		}

		private void initMatoate()
		{
			if (this.Lp == null)
			{
				return;
			}
			this.Par.setAim((base.x < this.Lp.mapcx) ? AIM.R : AIM.L, false);
			if (this.MtatPlayer != null)
			{
				return;
			}
			float num = 1f;
			int num2 = X.xors(3);
			MatoateReader matoateReader;
			if (num2 != 0)
			{
				if (num2 != 1)
				{
					matoateReader = this.Lp.initReader("alma105_2");
					num = 1.3f;
				}
				else
				{
					matoateReader = this.Lp.initReader("alma105_1");
				}
			}
			else
			{
				matoateReader = this.Lp.initReader("alma105_0");
			}
			matoateReader.GetI(0).rot_duration = X.NIXP(280f, 400f) * num;
			if (!this.Lp.initted)
			{
				this.Lp.Meta = new META();
				this.Lp.Meta.Add("pre_on", "0");
				this.Lp.Meta.Add("no_complete_effect", "1");
				this.Lp.initAction(true);
				this.Lp.EvtMatoateFinished += this.fnMatoateFinished;
			}
			this.Lp.activate();
			this.MtatPlayer = this.Lp.getPlayer();
			this.EfC.af = 0f;
			this.EfC.z = 2f;
			if (this.stt == AlmaPVV105.STATE.MAIN_MTAT_FINISHED && !EV.isActive(false))
			{
				this.stt = AlmaPVV105.STATE.MAIN;
			}
		}

		private void fnMatoateFinished(MatoateReader.MatoatePlayer<M2MatoateTarget> Player, bool successed)
		{
			if (this.isMainState())
			{
				this.MtatPlayer = null;
				if (this.EfC != null)
				{
					this.EfC.z = 1f;
					this.EfC.af = 0f;
				}
			}
		}

		protected override MGKIND get_magic_kind()
		{
			return MGKIND.WHITEARROW;
		}

		public override Vector2 getAimPos(MagicItem Mg)
		{
			if (this.EfN == null)
			{
				return new Vector2(base.x + base.mpf_is_right * 5f, base.y);
			}
			return new Vector2(this.EfN.x, this.EfN.y);
		}

		public bool isMainState()
		{
			return this.stt == AlmaPVV105.STATE.MAIN || this.stt == AlmaPVV105.STATE.MAIN_MTAT_FINISHED;
		}

		public override void EvtRead(StringHolder rER)
		{
			string _ = rER._2;
			if (_ != null)
			{
				if (!(_ == "TALKED"))
				{
					if (!(_ == "SET_TO_WAIT_PHASE"))
					{
						if (!(_ == "WAIT"))
						{
							return;
						}
						EV.initWaitFn(this, 0);
					}
					else if (this.stt != AlmaPVV105.STATE.SEEING_NOEL)
					{
						this.changeState(AlmaPVV105.STATE.SEEING_NOEL);
						return;
					}
				}
				else if (this.stt == AlmaPVV105.STATE.MAIN)
				{
					this.stt = AlmaPVV105.STATE.MAIN_MTAT_FINISHED;
					if (this.walk_st == 1 || this.walk_st == 2)
					{
						this.Par.quitMoveScript(true);
						return;
					}
				}
			}
		}

		public bool EvtWait(bool is_first = false)
		{
			if (!this.isMainState() || this.walk_st <= 4)
			{
				base.abortMagic();
				this.Par.quitMoveScript(true);
				if (this.stt != AlmaPVV105.STATE.SEEING_NOEL)
				{
					this.changeState(AlmaPVV105.STATE.SEEING_NOEL);
				}
				base.abortMagic();
				this.Par.quitMoveScript(true);
				this.Par.SpSetPose("stand", -1, null, false);
				return false;
			}
			return true;
		}

		public bool FnDrawTargetter(EffectItem Ef)
		{
			if (base.destructed)
			{
				return false;
			}
			if (Ef == this.EfN && this.EfC.x <= 0f)
			{
				return true;
			}
			float targetter_x = this.targetter_x;
			float num = this.targetter_y - (float)this.SqTargetter.height * 0.5f * this.Mp.rCLENB;
			if (!base.M2D.Cam.isCoveringMp(targetter_x, num, targetter_x, num, (Ef.z > 0f) ? (base.CLEN * 8f) : (base.CLEN * 2f)))
			{
				return true;
			}
			MeshDrawer meshDrawer = ((Ef == this.EfC) ? Ef.GetMeshImg("", MTRX.getMI(this.Anm.getCurrentCharacter()), BLEND.NORMAL, true) : Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true));
			bool flag;
			Vector3 vector = Ef.EF.calcMeshXY(targetter_x, num, meshDrawer, out flag);
			meshDrawer.base_x = vector.x;
			meshDrawer.base_y = vector.y;
			float num2 = 3.1415927f * (0.68f + 0.11f * X.Sin(this.EfC.y * 6.2831855f));
			if (Ef == this.EfC)
			{
				meshDrawer.Col = MTRX.ColWhite;
				meshDrawer.Scale(1f / this.Mp.base_scale, 1f / this.Mp.base_scale, false);
				for (int i = 2; i >= 0; i--)
				{
					float num3 = 0f;
					if (i == 1)
					{
						num3 = num2 - 1.5707964f;
					}
					meshDrawer.RotaPF(0f, 0f, 1f, 1f, num3, this.SqTargetter.getFrame(i), false, false, false, uint.MaxValue, false, 0);
				}
			}
			else
			{
				float x = this.EfC.x;
				meshDrawer.Rotate(num2, false);
				meshDrawer.getCurrentMatrix();
				meshDrawer.Col = meshDrawer.ColGrd.Set(4283691141U).mulA(x * (0.6f + 0.2f * X.COSI(this.Mp.floort, 3.7f) + 0.2f * X.COSI(this.Mp.floort, 5.11f))).C;
				meshDrawer.initForImg(MTRX.EffCircle128, 0).RotaGraph3(10f, 0f, 0.5f, 0.5f, 0.0546875f, 0.203125f, 0f, null, false);
				MeshDrawer meshImg = Ef.GetMeshImg("", MTR.MIiconP, BLEND.ADD, true);
				meshImg.base_x = meshDrawer.base_x;
				meshImg.base_y = meshDrawer.base_y;
				for (int j = 0; j < 20; j++)
				{
					float num4 = (this.EfN.af + (float)(j * 9)) % 180f;
					if (num4 >= 0f && num4 <= 70f)
					{
						float num5 = X.ZLINE(num4, 70f);
						uint ran = X.GETRAN2((int)(this.EfN.index + (uint)j), j % 7 + 2);
						meshImg.Col = meshDrawer.ColGrd.setA1(x * (X.ZLINE(num5, 0.2f) * X.ZLINE(1f - num5, 0.6f))).C;
						float num6 = -0.5f + X.RAN(ran, 675);
						float num7 = num2 + num6 * 3.1415927f * 0.127f;
						float num8 = X.NI(60, 120, X.RAN(ran, 550));
						float num9 = X.NI(45, 70, X.RAN(ran, 1185)) + X.NI(30, 66, X.RAN(ran, 673)) * num5 + num8 * 0.63f;
						float num10 = num6 * 7f;
						float num11 = X.NI(6, 12, X.RAN(ran, 2906));
						meshImg.Identity().Rotate(num7, false);
						MTRX.HaloI.drawTo(meshImg, 9, num9, num10, num8, num11, 0f, 1f, false);
						MTRX.HaloIA.drawTo(meshImg, 9, num9, num10, num8 * 1.4f, num11 * 1.4f, 0f, 1f, false);
					}
				}
			}
			return true;
		}

		public override string get_parent_name()
		{
			return "alma";
		}

		public override string get_voice_family_name()
		{
			return null;
		}

		private AlmaPVV105.STATE stt;

		public float x0;

		private int walk_st;

		private EffectItem EfC;

		private EffectItem EfN;

		private M2LpMatoate Lp;

		private const float lpw = 8f;

		private const float lph = 12f;

		private PxlSequence SqTargetter;

		private string movescript_walk;

		private string movescript_walk_back;

		private const float MATO_DURATION_T0 = 280f;

		private const float MATO_DURATION_T1 = 400f;

		private MatoateReader.MatoatePlayer<M2MatoateTarget> MtatPlayer;

		private const int WST_WAIT = 0;

		private const int WST_WALK_MACHINE = 1;

		private const int WST_WALK_MACHINE_BACK = 2;

		private const int WST_MATOATE_LAUNCHED = 3;

		private const int WST_MATOATE_MAG_INITTED = 4;

		private const int WST_MATOATE_MAG_EXPLODE_PREPARED = 5;

		private const int WST_MATOATE_MAG_EXPLODED = 6;

		private const int WST_MATOATE_MAG_EXPLODED2 = 7;

		public const float maxt_targetter_rot = 370f;

		public const float r_maxt_targetter_rot = 0.0027027028f;

		private const string ev_name = "alma";

		public enum STATE
		{
			PREPARE,
			MAIN,
			MAIN_MTAT_FINISHED,
			SEEING_NOEL,
			END
		}
	}
}
