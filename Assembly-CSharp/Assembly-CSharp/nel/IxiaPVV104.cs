using System;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class IxiaPVV104 : M2CastableEventBase
	{
		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			base.M2D.loadMaterialSnd("m2darea_insect");
			this.initTarget();
		}

		public override void destruct()
		{
			this.play_insect_snd = false;
			if (this.Mp != null)
			{
				this.Mp.M2D.Snd.Environment.RemLoop("areasnd_insect", this.snd_key);
			}
			base.destruct();
		}

		protected override bool initTarget()
		{
			int count_movers = this.Mp.count_movers;
			if (this.Mp.cmd_reload_flg != 0U)
			{
				return false;
			}
			int i = 0;
			while (i < count_movers)
			{
				NelNNusiCage nelNNusiCage = this.Mp.getMv(i) as NelNNusiCage;
				if (!(nelNNusiCage == null) && this.Target == null)
				{
					if (!base.initRenderTicket())
					{
						return false;
					}
					this.target_defined = true;
					this.Target = (this.TargetS = nelNNusiCage);
					this.Boss = this.TargetS.Parent as NelNBoss_Nusi;
					this.Summoner = this.Target.Summoner;
					this.play_insect_snd = true;
					this.ParPhy.addLockGravity(this.Target, 0f, -1f);
					this.ParPhy.addLockWallHitting(this.Target, -1f);
					this.ParPhy.addLockMoverHitting(HITLOCK.EVENT, -1f);
					this.Par.setAim(this.TargetS.aim, false);
					this.TargetS.assignIxia(this.Par, this);
					this.SpSetPose("nusi_wait", -1, null, false);
					this.Pr = this.Mp.Pr as PR;
					this.burst_useable = MagicSelector.isObtained(MGKIND.PR_BURST);
					break;
				}
				else
				{
					i++;
				}
			}
			return this.target_defined;
		}

		public bool play_insect_snd
		{
			get
			{
				return this.play_insect_snd_;
			}
			set
			{
				if (this.play_insect_snd == value)
				{
					return;
				}
				this.play_insect_snd_ = value;
				if (value)
				{
					this.SndLoop = this.Mp.M2D.Snd.Environment.AddLoop("areasnd_insect", this.snd_key, this.Target.x, this.Target.y, 6f, 6f, 8f, 8f, null);
					return;
				}
				this.Mp.M2D.Snd.Environment.RemLoop("areasnd_insect", this.snd_key);
			}
		}

		public override void EvtRead(StringHolder rER)
		{
			IxiaPVV104.STATE state = (IxiaPVV104.STATE)rER.Int(2, -1);
			if (state == IxiaPVV104.STATE.CAGE_APPEAL_EXECUTE)
			{
				state = IxiaPVV104.STATE.CAGE_APPEAL;
				if (this.TargetS != null)
				{
					this.TargetS.initIxiaHold();
					this.Boss.initFaintStun();
				}
			}
			if (state != this.state)
			{
				this.changeState(state);
			}
		}

		private void changeState(IxiaPVV104.STATE st)
		{
			if (!this.target_defined)
			{
				st = IxiaPVV104.STATE.WAITING;
			}
			IxiaPVV104.STATE state = this.state;
			if (state != IxiaPVV104.STATE.WAITING)
			{
				if (state == IxiaPVV104.STATE.CAGE_APPEAL_ABSORB)
				{
					base.quitTorturePose(M2Mover.DRAW_ORDER.PR0);
					this.SpSetPose("nusi_wait", -1, null, false);
					this.play_insect_snd = false;
					goto IL_0054;
				}
				if (state != IxiaPVV104.STATE.CAGE_CLOSED)
				{
					goto IL_0054;
				}
			}
			if (this.walk_st >= 20)
			{
				EV.TutoBox.RemText(true, false);
			}
			IL_0054:
			this.state = st;
			this.t = 0f;
			this.walk_st = 0;
			this.mp_hold = 0f;
			this.Par.setAim(this.Boss.aim, false);
			base.abortMagic();
			base.fineHoldMagicTime();
			base.hideTuto();
			if (this.EfSpot != null)
			{
				this.EfSpot.z = -1f;
				this.EfSpot = null;
			}
			if (this.PeSlow != null)
			{
				this.PeSlow.deactivate(false);
				this.PeSlow = null;
			}
			switch (st)
			{
			case IxiaPVV104.STATE.CAGE_APPEAL:
				this.TargetS.anmtype = NelNBoss_Nusi.MA_CAGE.APPEAL;
				this.SpSetPose("nusi_wait", -1, null, false);
				return;
			case IxiaPVV104.STATE.CAGE_APPEAL_EXECUTE:
				break;
			case IxiaPVV104.STATE.CAGE_APPEAL_ABSORB:
				this.play_insect_snd = true;
				this.mem_t = 0f;
				this.TargetS.need_fine_position = true;
				base.initTorturePose("torture_nusi_atk");
				return;
			case IxiaPVV104.STATE.CAGE_OPEN_PREPARE:
			case IxiaPVV104.STATE.CAGE_CLOSED:
				this.SpSetPose("weak", -1, null, false);
				this.Par.setAim(CAim.mpfLR(this.Par.x < this.Boss.x), false);
				return;
			case IxiaPVV104.STATE.END:
				this.Par.setAim(CAim.mpfLR(this.Par.x < this.Boss.x), false);
				break;
			default:
				return;
			}
		}

		public void changeStateToAbsorb()
		{
			this.changeState(IxiaPVV104.STATE.CAGE_APPEAL_ABSORB);
		}

		public bool isAbsorbing()
		{
			return this.state == IxiaPVV104.STATE.CAGE_APPEAL_ABSORB;
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (this.state == IxiaPVV104.STATE.END)
			{
				if (this.Par == null || this.Par.destructed)
				{
					this.Mp.removeMover(this);
					return;
				}
				return;
			}
			else
			{
				if (!this.target_defined || base.destructed)
				{
					return;
				}
				if (this.PeSlow != null)
				{
					this.PeSlow.fine(120);
				}
				switch (this.state)
				{
				case IxiaPVV104.STATE.WAITING:
				case IxiaPVV104.STATE.CAGE_CLOSED:
					if (this.state == IxiaPVV104.STATE.CAGE_CLOSED && ((this.walk_st == -1 && this.TargetS.cage_return_back) || this.walk_st == -2))
					{
						base.showMessage("s13_ixia_battle", "i_0" + (6 + X.xors(3)).ToString(), 120f);
						this.walk_st = 0;
					}
					if (this.TargetS.anmtype == NelNBoss_Nusi.MA_CAGE.APPEAL_OPEN)
					{
						this.changeState(IxiaPVV104.STATE.CAGE_OPEN_PREPARE);
						return;
					}
					if (this.TargetS.anmtype == NelNBoss_Nusi.MA_CAGE.APPEAL)
					{
						this.changeState(IxiaPVV104.STATE.CAGE_APPEAL);
						return;
					}
					if (this.Boss.isFaintCounterExecuting(true) && this.burst_useable)
					{
						if (this.Pr != null)
						{
							if (this.Pr.is_alive && this.walk_st < 20)
							{
								if (this.Pr.isAbsorbState())
								{
									this.walk_st++;
									if (this.walk_st == 20)
									{
										this.mem_t = X.Mx(1f, this.Pr.get_hp());
										base.setTutoTX("Tuto_magic_burst");
										if (this.Boss.isPlayerCounterUnsuccess())
										{
											this.PeSlow = PostEffect.IT.setSlow(30f, 0.5f, 0);
										}
										this.EfSpot = this.Mp.getEffectTop().setE("spot_darken", 340f, 0f, 0f, 60, 0);
										return;
									}
								}
							}
							else if (this.walk_st == 20)
							{
								if (this.Pr.get_hp() < this.mem_t)
								{
									if (this.PeSlow != null)
									{
										this.PeSlow.deactivate(false);
										this.PeSlow = null;
									}
									this.walk_st = 21;
									return;
								}
							}
							else if (this.walk_st >= 21 && !this.Pr.is_alive)
							{
								base.hideTuto();
								if (this.EfSpot != null)
								{
									this.EfSpot.z = -1f;
									this.EfSpot = null;
									return;
								}
							}
						}
					}
					else if (this.Pr.is_alive && this.burst_useable && this.Pr.Ser.has(SER.BURNED) && this.Pr.Ser.punchAllow())
					{
						this.walk_st++;
						if (this.walk_st == 20)
						{
							this.walk_st = 21;
							base.setTutoTX("Tuto_magic_burst");
							return;
						}
					}
					else if (this.walk_st >= 21)
					{
						base.hideTuto();
						this.walk_st = 0;
						return;
					}
					break;
				case IxiaPVV104.STATE.CAGE_APPEAL:
				case IxiaPVV104.STATE.CAGE_APPEAL_ABSORB:
					if (this.state == IxiaPVV104.STATE.CAGE_APPEAL_ABSORB && base.basicAbsorbProgress(60f) && this.mem_t == 0f && Map2d.can_handle)
					{
						this.mem_t = 1f;
						if (this.Boss.isPlayerCounterUnsuccess() && !this.burst_useable)
						{
							base.showMessage("s13_ixia_battle", "i_getout", 340f);
						}
						else
						{
							base.showMessage("s13_ixia_battle", "i_0" + X.xors(3).ToString(), 110f);
						}
					}
					if (this.Boss.isFaintCounterExecuting(false) && this.first_absorb)
					{
						this.first_absorb = false;
						this.Summoner.getSummonedArea().parseType("0");
					}
					if (this.TargetS.anmtype != NelNBoss_Nusi.MA_CAGE.APPEAL)
					{
						if (this.Boss.isFaintCured(false))
						{
							this.changeState(IxiaPVV104.STATE.CAGE_CLOSED);
							this.walk_st = -2;
							return;
						}
						this.changeState(IxiaPVV104.STATE.WAITING);
						return;
					}
					break;
				case IxiaPVV104.STATE.CAGE_APPEAL_EXECUTE:
					break;
				case IxiaPVV104.STATE.CAGE_OPEN_PREPARE:
					if (this.walk_st == 0 && this.TargetS.cage_opened)
					{
						this.walk_st = 1;
						this.t = 0f;
						this.mem_t = 0f;
					}
					if (this.walk_st == 1 && this.t >= 30f)
					{
						if (this.mem_t == 0f)
						{
							this.mem_t = 1f;
							base.showMessage("s13_ixia_battle", "i_0" + (3 + X.xors(3)).ToString(), 90f);
						}
						this.SpSetPose("weak_magic", -1, null, false);
						if (this.Boss.is_alive)
						{
							base.initChant();
							this.walk_st = 2;
						}
						else
						{
							this.walk_st = 10;
						}
					}
					if (this.walk_st == 2)
					{
						if (base.magic_explodable())
						{
							if (this.t >= 25f)
							{
								this.t = 0f;
								this.walk_st = 3;
								this.SpSetPose("magic_prepare_t", -1, null, false);
							}
						}
						else
						{
							this.t = 0f;
						}
					}
					if (this.walk_st == 3 && this.t >= 20f)
					{
						this.t = 0f;
						this.walk_st = 4;
						this.SpSetPose("magic_t", -1, null, false);
						base.explodeMagic();
					}
					if (this.walk_st == 4 && this.t >= 90f)
					{
						this.t = 10f;
						this.walk_st = 1;
						this.SpSetPose("weak", -1, null, false);
						this.mp_hold = 0f;
						base.fineHoldMagicTime();
					}
					if (this.TargetS.anmtype != NelNBoss_Nusi.MA_CAGE.APPEAL_OPEN)
					{
						this.changeState(IxiaPVV104.STATE.CAGE_CLOSED);
						this.walk_st = -1;
					}
					break;
				default:
					return;
				}
				return;
			}
		}

		protected override MGKIND get_magic_kind()
		{
			return MGKIND.WHITEARROW;
		}

		public override Vector2 getAimPos(MagicItem Mg)
		{
			return new Vector2(this.Boss.x, this.Boss.y - 0.04f);
		}

		public void cageKilled()
		{
			this.changeState(IxiaPVV104.STATE.END);
			this.ParPhy.remLockGravity(this.Target);
			this.ParPhy.remLockWallHitting(this.Target);
			this.SpSetPose("weak", -1, null, false);
		}

		private M2SndLoopItem SndLoop;

		private EnemySummoner Summoner;

		private IxiaPVV104.STATE state;

		private NelNNusiCage TargetS;

		private NelNBoss_Nusi Boss;

		private bool burst_useable;

		private EffectItem EfSpot;

		private PostEffectItem PeSlow;

		private uint message_bits;

		private bool play_insect_snd_;

		private bool first_absorb = true;

		private float mem_t;

		private int walk_st;

		private const string ev_name = "s13_ixia_battle";

		public enum STATE
		{
			WAITING,
			CAGE_APPEAL,
			CAGE_APPEAL_EXECUTE,
			CAGE_APPEAL_ABSORB,
			CAGE_OPEN_PREPARE,
			CAGE_CLOSED,
			END
		}
	}
}
