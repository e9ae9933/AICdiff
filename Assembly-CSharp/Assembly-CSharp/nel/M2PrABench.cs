using System;
using m2d;
using nel.gm;
using PixelLiner;
using XX;

namespace nel
{
	internal sealed class M2PrABench : M2PrARunner
	{
		public M2PrABench(PR _Pr)
			: base(_Pr)
		{
			this.PrM = this.Pr as PRMain;
		}

		public override M2PrARunner activate()
		{
			PR.STATE state = base.state;
			if (state - PR.STATE.BENCH <= 1)
			{
				this.Pr.checkOazuke();
				this.Phy.addLockMoverHitting(HITLOCK.EVADE, -1f);
			}
			COOK.FlgTimerStop.Add("BENCH");
			base.addD(M2MoverPr.DECL.THROW_RAY);
			return this;
		}

		public override void quitSPPR(PR Pr, PR.STATE aftstate)
		{
			COOK.FlgTimerStop.Rem("BENCH");
			Pr.recheck_emot = true;
			if (!Pr.isBenchState(aftstate))
			{
				base.Anm.setPose(base.poseIs("bench_must_orgasm_2") ? "must_orgasm_2" : "bench2stand", -1, false);
				UIBase.Instance.gameMenuSlide(false, false);
				UIBase.Instance.gameMenuBenchSlide(false, false);
				UiBenchMenu.hideModalOfflineOnBench();
			}
		}

		public bool initBenchSitDown(NelChipBench Cp, bool is_load_after = false, bool force_to_state_reset = false)
		{
			this.Phy.clampSpeed(FOCTYPE.WALK, -1f, -1f, 1f);
			this.Phy.killSpeedForce(true, true, true, false, false);
			this.Pr.Skill.BurstSel.clearExecuteCount();
			if (this.Pr.isBombHoldingState())
			{
				this.Pr.Skill.BombCancelCheck(null, true);
				this.Pr.changeState(PR.STATE.NORMAL);
			}
			if (Cp != null)
			{
				if (this.Pr.canJump() && X.Abs(this.Pr.x - Cp.mapcx) < 2f)
				{
					this.Pr.moveBy(Cp.mapcx - this.Pr.x, 0f, true);
				}
				this.Phy.addLockGravityFrame(2);
				this.FootD.rideInitTo(Cp, false);
				if (Cp.player_aim != -1)
				{
					base.setAim((AIM)Cp.player_aim, false);
				}
				if (!is_load_after)
				{
					this.Pr.CureBench();
				}
				SVD.sFile currentFile = COOK.getCurrentFile();
				if (currentFile != null)
				{
					currentFile.assignRevertPosition(base.NM2D);
				}
				if (base.Mp.floort > 5f && !base.hasD(M2MoverPr.DECL.INIT_A))
				{
					base.Anm.setPose("stand2bench", -1, false);
				}
				this.Pr.Skill.killHoldMagic(false, false);
				this.Pr.Skill.BurstSel.clearExecuteCount();
				PR.STATE state = (is_load_after ? PR.STATE.BENCH_LOADAFTER : PR.STATE.BENCH);
				if (base.state != state || force_to_state_reset)
				{
					this.Pr.changeState(state);
				}
			}
			else
			{
				if (!base.isOnBench(true))
				{
					return false;
				}
				PxlSequence currentSequence = base.Anm.getCurrentSequence();
				if (!base.Anm.poseIs("stand2bench", "bench"))
				{
					base.Anm.setPose("bench", -1, false);
				}
				if (base.Anm.getCurrentSequence() != currentSequence)
				{
					base.Anm.animReset(base.Anm.getCurrentSequence().loop_to);
				}
			}
			this.Pr.GSaver.FineAll(true);
			if (!base.hasD(M2MoverPr.DECL.INIT_A))
			{
				this.Pr.recheck_emot_in_gm = true;
			}
			return true;
		}

		public override bool runPreSPPR(PR Pr, float fcnt, ref float t_state)
		{
			bool flag = false;
			PR.STATE state = base.state;
			if (state - PR.STATE.BENCH <= 1)
			{
				Bench.P("Bench-1");
				if (t_state <= 0f)
				{
					NelChipBench nelChipBench = Pr.getNearBench(false, false);
					if (nelChipBench == null)
					{
						Pr.recheck_emot = true;
						if (t_state < 40f)
						{
							return false;
						}
					}
					if (base.state == PR.STATE.BENCH_LOADAFTER)
					{
						Pr.fineFrozenAppearance();
						if (nelChipBench != null)
						{
							this.PrM.initBenchSitDown(nelChipBench, true, false);
						}
						if (base.Mp.floort >= 50f)
						{
							base.addD(M2MoverPr.DECL.FLAG1);
						}
						base.Anm.animReset(base.Anm.getCurrentSequence().loop_to);
					}
					else
					{
						UiBenchMenu.auto_start_temp_disable = false;
					}
					if (nelChipBench != null && t_state < 40f && this.FootD.get_Foot() != nelChipBench)
					{
						this.FootD.rideInitTo(nelChipBench, false);
					}
					if (Pr.poseIsBenchMusturbOrgasm())
					{
						Pr.VO.breath_key = "breath_e";
					}
					base.Ser.checkSer();
				}
				if (!base.hasD(M2MoverPr.DECL.FLAG1) && !base.hasD(M2MoverPr.DECL.FLAG_PRESS))
				{
					base.addD(M2MoverPr.DECL.FLAG1);
					UiBenchMenu.defineEvents(Pr, false);
				}
				if (t_state >= 18f && !base.hasD(M2MoverPr.DECL.INIT_A))
				{
					base.addD(M2MoverPr.DECL.INIT_A);
					if (base.state != PR.STATE.BENCH_LOADAFTER)
					{
						Pr.CureBench();
					}
					Pr.Skill.initS();
					NelChipBench nelChipBench = Pr.getNearBench(false, false);
					if (nelChipBench == null)
					{
						base.addD(M2MoverPr.DECL.FLAG_PRESS);
					}
					else if (base.state != PR.STATE.BENCH_LOADAFTER)
					{
						nelChipBench.initSitDown(Pr, true, true);
					}
					else
					{
						nelChipBench.initSitDown(Pr, false, false);
						Pr.recheck_emot = true;
					}
				}
				Bench.Pend("Bench-1");
				Bench.P("Bench-2");
				flag = t_state >= 10f;
				if (UiBenchMenu.checkStackForceEvent(Pr, t_state >= 27f, !base.NM2D.GM.isActive()))
				{
					flag = false;
				}
				Bench.Pend("Bench-2");
			}
			M2PrABench.checkProgressBench(Pr, flag);
			return true;
		}

		public static bool checkProgressBench(PR Pr, bool can_finish)
		{
			if (can_finish)
			{
				if (IN.isMenuO(0) || IN.isMenuU() || Pr.NM2D.GM.isActive())
				{
					Pr.addD(M2MoverPr.DECL.FLAG0);
					Pr.remD((M2MoverPr.DECL)3145728);
				}
				else if (Pr.hasD(M2MoverPr.DECL.FLAG0))
				{
					Pr.remD(M2MoverPr.DECL.FLAG0);
				}
				else
				{
					if (Pr.isActionPD())
					{
						Pr.addD(M2MoverPr.DECL.FLAG_PD);
					}
					if (Pr.hasD(M2MoverPr.DECL.FLAG_PD) && (!Map2d.can_handle || !Pr.isActionO(0) || Pr.isActionO(12)))
					{
						Pr.addD(M2MoverPr.DECL.FLAG_PRESS);
					}
				}
			}
			else
			{
				Pr.remD((M2MoverPr.DECL)3145728);
			}
			if (Pr.hasD(M2MoverPr.DECL.FLAG_PRESS))
			{
				Pr.changeState(PR.STATE.NORMAL);
				return false;
			}
			return true;
		}

		private PRMain PrM;
	}
}
