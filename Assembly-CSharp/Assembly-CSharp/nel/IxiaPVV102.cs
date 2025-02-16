using System;
using evt;
using m2d;
using XX;

namespace nel
{
	public class IxiaPVV102 : M2CastableEventBase
	{
		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			base.M2D.loadMaterialSnd("m2darea_insect");
		}

		public override void destruct()
		{
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
			for (int i = 0; i < count_movers; i++)
			{
				NelNSnake nelNSnake = this.Mp.getMv(i) as NelNSnake;
				if (!(nelNSnake == null) && this.Target == null)
				{
					if (!base.initRenderTicket())
					{
						return false;
					}
					this.target_defined = true;
					this.Target = (this.TargetS = nelNSnake);
					this.Mp.M2D.Snd.Environment.AddLoop("areasnd_insect", this.snd_key, this.Target.x, this.Target.y, 6f, 6f, 8f, 8f, null);
					this.TargetS.initIxiaHold();
					base.initTorturePose("torture_worm");
					this.Par.setAim(AIM.R, false);
				}
			}
			return this.target_defined;
		}

		public override void EvtRead(StringHolder rER)
		{
			int num = rER.Int(2, -1);
			this.changeState((IxiaPVV102.STATE)num);
		}

		private void changeState(IxiaPVV102.STATE st)
		{
			if (!this.target_defined)
			{
				st = IxiaPVV102.STATE.WAITING;
			}
			IxiaPVV102.STATE state = this.state;
			this.state = st;
			this.t = 0f;
			this.mp_hold = 0f;
			base.fineHoldMagicTime();
			if (st <= IxiaPVV102.STATE.QUITING)
			{
				switch (st)
				{
				case IxiaPVV102.STATE.WAITING_NOEL_ATTACK:
					this.TargetS.initIxiaWaitingNoelAttack();
					return;
				case IxiaPVV102.STATE.EVENT_ACTIVATE:
					if (state < st)
					{
						this.fasten = 0f;
						EV.stack("s11_2a_2", 0, -1, null, null);
					}
					this.TargetS.initIxiaHitEvent();
					return;
				case IxiaPVV102.STATE.PRE_FLYING:
					this.TargetS.initIxiaPreThrow();
					this.fasten = 0f;
					return;
				default:
					if (st != IxiaPVV102.STATE.QUITING)
					{
						return;
					}
					this.Anm.order = M2Mover.DRAW_ORDER.N_BACK0;
					return;
				}
			}
			else
			{
				if (st == IxiaPVV102.STATE.BATTLE)
				{
					this.fasten = 0f;
					return;
				}
				if (st != IxiaPVV102.STATE.BATTLE_FINISHED)
				{
					return;
				}
				this.fasten = 0f;
				this.SpSetPose("weak_magic", -1, null, false);
				return;
			}
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed || !this.target_defined || this.Par.destructed)
			{
				return;
			}
			if (this.state < IxiaPVV102.STATE.BATTLE && this.Target.destructed)
			{
				if (!base.destructed)
				{
					this.Mp.removeMover(this);
				}
				return;
			}
			IxiaPVV102.STATE state = this.state;
			if (state <= IxiaPVV102.STATE.QUITING)
			{
				switch (state)
				{
				case IxiaPVV102.STATE.WAITING:
				case IxiaPVV102.STATE.WAITING_NOEL_ATTACK:
					base.basicAbsorbProgress(60f);
					if (this.state == IxiaPVV102.STATE.WAITING_NOEL_ATTACK && this.Target.get_hp() < this.Target.get_maxhp())
					{
						this.changeState(IxiaPVV102.STATE.EVENT_ACTIVATE);
						return;
					}
					break;
				case IxiaPVV102.STATE.EVENT_ACTIVATE:
					break;
				case IxiaPVV102.STATE.PRE_FLYING:
					if (this.fasten == 0f)
					{
						if (this.TargetS.initIxiaAlreadyThrown(true))
						{
							this.Par.initJump();
							this.fasten = 1f;
							this.Mp.M2D.Snd.Environment.RemLoop("areasnd_insect", this.snd_key);
							base.playVo("dmg_hktb");
							base.quitTorturePose(M2Mover.DRAW_ORDER.PR0);
							this.SpSetPose("dmg_hktb_b", -1, null, false);
							this.playSndPos("swing_big_2", 1);
							this.Par.setAim(AIM.L, false);
							this.ParPhy.addFoc(FOCTYPE.RESIZE, -0.8f, -0.1f, -1f, -1, 1, 0, -1, 0);
							this.ParPhy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._RELEASE, 0f, -0.04f, -1f, -1, 1, 0, -1, 0);
							this.ParPhy.addFoc(FOCTYPE.DAMAGE | FOCTYPE._CHECK_WALL, -0.38f, 0f, -1f, 0, 8, 50, 30, 0);
							if ((int)(this.Target.mbottom + 0.25f) > (int)(this.Pr.mbottom + 0.25f))
							{
								this.Pr.assignMoveScript("w20 >+[" + (X.MPF(this.Pr.x < this.Par.x) * 170).ToString() + ",0 <<0.18]", false);
								return;
							}
						}
					}
					else
					{
						if (this.fasten == 1f && this.ParPhy.Mv.wallHittedA())
						{
							this.fasten = 2f;
							this.Mp.M2D.Cam.Qu.SinH(6f, 20f, 3f, 0).Vib(7f, 12f, 2f, 0);
							this.playSndPos("pr_hit_wall", 1);
							this.SpSetPose("hitwall", -1, null, false);
							this.ParPhy.remFoc(FOCTYPE.DAMAGE, true);
							this.ParPhy.addFoc(FOCTYPE.DAMAGE, 0.06f, 0f, -1f, 0, 2, 28, -1, 0);
							base.playVo("dmgl");
						}
						if ((this.fasten == 1f || this.fasten == 2f) && this.ParPhy.hasFoot())
						{
							this.fasten = 3f;
							this.playSndPos("prko_ground", 1);
							this.SpSetPose("down_b", -1, null, false);
							return;
						}
					}
					break;
				default:
					if (state != IxiaPVV102.STATE.QUITING)
					{
						return;
					}
					if (this.t >= 70f)
					{
						this.changeState(IxiaPVV102.STATE.BATTLE);
						this.TargetS.quitIxiaHold();
						this.Anm.order = M2Mover.DRAW_ORDER.N_TOP0;
						return;
					}
					break;
				}
			}
			else
			{
				if (state != IxiaPVV102.STATE.BATTLE)
				{
					return;
				}
				if (this.fasten >= 0f && this.fasten <= 1f && !this.Pr.is_alive)
				{
					this.message_bits |= 4U;
					base.showMessage("s11_3a", "i_noel_dead", 280f);
					this.SpSetPose("weak", -1, null, false);
					this.mp_hold = 0f;
					base.fineHoldMagicTime();
					this.fasten = -100f;
				}
				if (this.Target.destructed && 0f <= this.fasten)
				{
					this.changeState(IxiaPVV102.STATE.BATTLE_FINISHED);
					return;
				}
				if (this.fasten == 0f)
				{
					if (this.Target.isStunned())
					{
						this.t = 0f;
						this.fasten = 1f;
						this.message_bits &= 4294967294U;
					}
					else if (this.t >= 120f)
					{
						if (this.TargetS.isDigging() && (this.message_bits & 1U) == 0U)
						{
							this.message_bits |= 1U;
							base.showMessage("s11_3a", "i_use_bomb", 280f);
						}
						else if (X.XORSP() < 0.04f || this.Pr.isAbsorbState())
						{
							this.message_bits &= 4294967294U;
						}
						this.t = 0f;
					}
				}
				if (this.fasten == 1f)
				{
					if (!this.Target.isStunned())
					{
						base.aimToMv(this.Target);
						this.t = 0f;
						this.fasten = 0f;
						this.message_bits &= 4294967293U;
					}
					else if (this.t >= 5f)
					{
						base.initChant();
						this.t = 0f;
						this.fasten = 2f;
						this.SpSetPose("weak_magic", -1, null, false);
						if ((this.message_bits & 2U) == 0U)
						{
							this.message_bits |= 2U;
							base.showMessage("s11_3a", "i_bomb_success", 280f);
						}
					}
				}
				if (this.fasten == 2f)
				{
					if (!this.Target.isStunned() || this.CurMg == null)
					{
						if (this.t >= 15f)
						{
							this.t = 0f;
							this.fasten = 0f;
							if (this.CurMg != null && this.CurMg.isPreparingCircle)
							{
								base.sleepMagic();
							}
							else
							{
								this.SpSetPose("weak", -1, null, false);
							}
						}
					}
					else if (base.magic_explodable())
					{
						if (this.t >= 15f)
						{
							this.t = 0f;
							this.fasten = 3f;
							this.SpSetPose("magic_prepare_t", -1, null, false);
						}
					}
					else
					{
						this.t = 0f;
					}
				}
				if (this.fasten == 3f && this.t >= 20f)
				{
					this.t = 0f;
					this.fasten = 4f;
					this.SpSetPose("magic_t", -1, null, false);
					base.explodeMagic();
				}
				if (this.fasten == 4f && this.t >= 110f)
				{
					this.t = 20f;
					this.fasten = 1f;
					this.SpSetPose("weak", -1, null, false);
					this.mp_hold = 0f;
					base.fineHoldMagicTime();
				}
			}
		}

		private NelNSnake TargetS;

		private IxiaPVV102.STATE state;

		private uint message_bits;

		private enum STATE
		{
			WAITING,
			WAITING_NOEL_ATTACK,
			EVENT_ACTIVATE,
			PRE_FLYING,
			QUITING = 10,
			BATTLE,
			BATTLE_FINISHED = 20
		}
	}
}
