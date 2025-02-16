using System;
using evt;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public sealed class M2PrMasturbate : ISpecialPrRunner, IGachaListener, IEventWaitListener
	{
		public M2PrMasturbate(PR _Pr, bool _on_bench = true)
		{
			this.Pr = _Pr;
			this.M2D = this.Pr.NM2D;
			this.Anm = this.Pr.getAnimator();
			this.on_bench = _on_bench;
			this.PE = new EffectHandlerPE(2);
			if (_on_bench)
			{
				(M2DBase.Instance as NelM2DBase).FlagOpenGm.Add("MASTURBATE");
			}
			this.pre_orgasm_count = this.Pr.EpCon.masturbate_count;
			EV.isActive(false);
		}

		public M2PrMasturbate init(int delay = 0, bool is_first = false)
		{
			if (X.SENSITIVE)
			{
				return null;
			}
			this.t = (float)(-1000 - delay);
			this.Pr.addD(M2MoverPr.DECL.ORGASM_INJECTABLE);
			this.intro_pose_title = null;
			if (this.Pr.isBenchState())
			{
				this.type = M2PrMasturbate.TYPE.ON_BENCH;
			}
			else
			{
				this.type = M2PrMasturbate.TYPE.ON_ROAD;
			}
			int num = this.level;
			this.level = 0;
			for (int i = MDAT.Apr_ep_threshold.Length - ((num >= 2) ? 1 : 2); i >= 0; i--)
			{
				if (this.Pr.ep >= MDAT.Apr_ep_threshold[i])
				{
					this.level = i;
					break;
				}
			}
			if (!this.PE.has(POSTM.ZOOM2))
			{
				this.PE.Set(PostEffect.IT.setPE(POSTM.ZOOM2, 130f, 1f, 0));
			}
			if (this.level < 3)
			{
				this.Pr.EpCon.quitOrasmSageTime(true);
			}
			if (EnemySummoner.isActiveBorder())
			{
				this.Pr.EpCon.attachable_ser_oazuke = false;
			}
			if (this.level < 3 && this.type == M2PrMasturbate.TYPE.ON_BENCH)
			{
				this.prepareTxKD(true);
			}
			if (this.TkKD != null)
			{
				this.TkKD.hold_blink = false;
				this.TkKD.need_fine = true;
			}
			if (is_first)
			{
				this.Pr.VO.breath_key = "breath_e";
			}
			return this;
		}

		public void absorbFinished(bool abort)
		{
			if (!abort)
			{
				this.tapped = -1;
			}
			this.AbsorbMng = null;
		}

		public bool canAbsorbContinue()
		{
			return this.AbsorbMng != null && this.Pr.isMasturbateState();
		}

		public bool individual
		{
			get
			{
				return true;
			}
		}

		public void quitSPPR(PR Pr, PR.STATE aftstate)
		{
			this.AbsorbMng = null;
			this.prepareTxKD(false);
			this.canceling = false;
			this.PE.deactivate(true);
			this.t = -1140f;
			this.level = -1;
			this.intro_pose_title = null;
			(M2DBase.Instance as NelM2DBase).FlagOpenGm.Rem("MASTURBATE");
			if (this.isFinished() && !Pr.isBenchState(aftstate))
			{
				Pr.cureMpNotHunger(false);
			}
		}

		public bool runPreSPPR(PR Pr, float fcnt, ref float t_state)
		{
			Pr.clipCrouchTime(1f);
			Pr.getAbsorbContainer().runAbsorbPr(Pr, this.t, Pr.TS);
			if (this.is_bench)
			{
				if (!this.runMusturbate(Pr.TS))
				{
					if (!Pr.poseIsBenchMusturbOrgasm())
					{
						this.Anm.setPose("bench_shamed", -1, false);
					}
					if (!EV.isActive(false) && !this.M2D.GM.isBenchMenuActive())
					{
						UIBase.Instance.gameMenuSlide(false, false);
						UIBase.Instance.gameMenuBenchSlide(false, false);
					}
					this.M2D.FlagOpenGm.Rem("MASTURBATE");
					Pr.changeState(PR.STATE.BENCH_LOADAFTER);
					Pr.addD(M2MoverPr.DECL.INIT_A);
					if (this.M2D.GM.isBenchMenuActive() && !EV.isActive(false))
					{
						this.M2D.GM.evClose(true);
					}
					return true;
				}
			}
			else if (!Pr.canStartFrustratedMasturbate(false) || !this.runMusturbate(Pr.TS))
			{
				return false;
			}
			return true;
		}

		private bool runMusturbate(float fcnt)
		{
			if (X.SENSITIVE)
			{
				return false;
			}
			if (this.t < -1000f)
			{
				this.t = X.Mn(this.t + fcnt, -1000f);
				return this.level >= 0;
			}
			if (this.t == -1000f)
			{
				if (this.level < 0)
				{
					return true;
				}
				this.t = -1f;
				this.tapped = 0;
				this.intro_pose_title = null;
				int i = this.level;
				this.Pr.VO.breath_key = null;
				if (i == 4)
				{
					this.Pr.SpSetPose(this.is_bench ? "bench_must_orgasm" : "must_orgasm", -1, null, false);
					this.t = 0f;
				}
				else
				{
					PrPoseContainer.PoseInfo poseInfo = null;
					while (i >= 0)
					{
						poseInfo = this.Pr.getAnimator().getPoseInfo((this.is_bench ? "bench_must" : "must") + i.ToString());
						if (poseInfo != null)
						{
							break;
						}
						i--;
					}
					if (poseInfo == null)
					{
						return false;
					}
					if (this.Pr.getAnimator().getPoseInfo((this.is_bench ? "bench_must" : "must") + i.ToString() + "l") == null)
					{
						this.t = 0f;
						this.Pr.SpSetPose(poseInfo.title, -1, null, false);
					}
					else
					{
						this.t = -1f;
						this.intro_pose_title = poseInfo.title;
						this.Pr.SpSetPose(poseInfo.title, -1, null, false);
						this.Pr.playSndAbs("cloth_off");
					}
					if (this.level == 1)
					{
						this.Pr.recheck_emot = true;
					}
				}
				if (this.level >= 3 && !this.PE.has(POSTM.ZOOM2_EATEN))
				{
					this.PE.Set(PostEffect.IT.setPE(POSTM.ZOOM2_EATEN, 200f, 1f, 0));
				}
			}
			this.PE.fine(100);
			if (this.t < 0f && (this.intro_pose_title == null || !this.Anm.poseIs(this.intro_pose_title) || this.Anm.isAnimEnd()))
			{
				this.t = 0f;
				if (this.intro_pose_title != null && !this.Anm.poseIs(this.intro_pose_title + "l"))
				{
					this.Pr.SpSetPose(this.intro_pose_title + "l", -1, null, false);
				}
			}
			if (this.t == 0f)
			{
				if (fcnt == 0f)
				{
					return true;
				}
				this.ep_0 = (float)this.Pr.ep;
				this.tapped = 0;
				if (this.level >= 3)
				{
					this.ef_intv = 35f;
					this.Pr.PtcVar("level", (double)this.level).PtcST("masturbate_hit_s", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				else
				{
					AbsorbManagerContainer absorbContainer = this.Pr.getAbsorbContainer();
					int num = 8 * ((this.level == 0 && !this.Pr.Ser.has(SER.SHAMED_EP)) ? 2 : 1);
					this.AbsorbMng = this.Pr.getAbsorbContainer().initSpecialGachaNotDiffFix(this.Pr, this, PrGachaItem.TYPE.PENDULUM_ONNIE, num, KEY.SIMKEY.Z, true);
					if (this.AbsorbMng == null)
					{
						return false;
					}
					absorbContainer.no_error_on_miss_input = true;
					absorbContainer.timeout = 0;
					this.AbsorbMng.kirimomi_release = false;
					this.AbsorbMng.target_pose = null;
					this.Gacha = this.AbsorbMng.get_Gacha();
					this.Pdr = this.Gacha.getPendulumDrawer();
					this.ep_add = (float)(MDAT.Apr_ep_threshold[this.level + 1] - this.Pr.ep) / (float)num;
					this.ef_intv = (float)((this.level == 2) ? 44 : ((this.level == 1) ? 60 : 72));
					this.Pdr.intv_t = (int)this.ef_intv;
					this.Pdr.need_recalc = true;
					PxlSequence currentSequence = this.Anm.getCurrentSequence();
					int num2 = 2;
					int num3 = ((currentSequence.loop_to == 0) ? 0 : currentSequence.getFrame(currentSequence.loop_to - 1).endf60);
					int num4 = currentSequence.getDuration() - num3;
					this.Anm.timescale = (float)num4 / (this.ef_intv * (float)num2);
					this.Pdr.resetTime(12, false);
					if (this.Pr.Rebagacha != null)
					{
						this.Pr.Rebagacha.need_reposit = true;
					}
				}
				if (this.level <= 3)
				{
					this.Pr.recheck_emot = true;
				}
			}
			if (this.t >= 0f)
			{
				this.t += fcnt;
				if (this.level < 3)
				{
					int num5 = this.tapped;
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					if (this.AbsorbMng == null)
					{
						flag2 = (flag = true);
						if (this.tapped >= 0)
						{
							return false;
						}
						num5 = -1;
					}
					else
					{
						if (this.Pdr.getUnderPass(true))
						{
							flag2 = true;
						}
						num5 = (this.Gacha.isFinished() ? (-1) : X.Abs((int)this.Gacha.getCount(false)));
						if (this.Pdr.getIgnoredTap(true) && (this.Gacha.getCountLevel() >= 0.5f || this.level >= 2))
						{
							flag = true;
						}
					}
					if (num5 < 0 || num5 > this.tapped)
					{
						float num6 = (float)(MDAT.Apr_ep_threshold[this.level + 1] - ((num5 < 0) ? 0 : 1));
						int num7 = (int)((num5 < 0) ? (num6 - (float)this.Pr.ep) : (X.Mn(this.ep_0 + (float)num5 * this.ep_add, num6) - (float)this.Pr.ep));
						if (num7 > 0)
						{
							EpAtk epAtk;
							if (this.type == M2PrMasturbate.TYPE.ON_BENCH)
							{
								epAtk = ((this.level >= 1) ? MDAT.EpAtkMasturbate2 : MDAT.EpAtkMasturbate);
							}
							else
							{
								epAtk = ((this.level >= 1) ? MDAT.EpAtkMasturbate_Road2 : MDAT.EpAtkMasturbate_Road);
							}
							epAtk.val = num7;
							this.Pr.EpCon.applyEpDamage(epAtk, null, EPCATEG_BITS._ALL, 1f, true);
						}
						if (this.type == M2PrMasturbate.TYPE.ON_ROAD)
						{
							this.Pr.DMG.splitMpByDamage(MDAT.AtkMasturbate_road, MDAT.AtkMasturbate_road.mpdmg0, MANA_HIT.EN | MANA_HIT.FALL | MANA_HIT.FROM_ABSORB_SPLIT, 0, 0f, null, false);
						}
						this.Pr.PtcVar("level", (double)this.level).PtcST("masturbate_hit_s", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Pr.UP.applyMasturbate(0.4f + 0.17f * (float)this.level, 5f, (float)(30 - this.level * 8));
						flag = true;
						this.Pdr.decline_under_sound = true;
						this.Pr.Mp.DropCon.setLoveJuice(this.Pr, 4, 2868903935U, 1f, false);
						if (num5 < 0)
						{
							flag3 = true;
						}
						else
						{
							this.tapped = num5;
						}
					}
					if (flag2)
					{
						this.M2D.Snd.play((this.level <= 1) ? "kuchu" : "kuchul");
					}
					if (flag)
					{
						this.Pr.VO.playMasturbVo(this.level);
					}
					if (flag3)
					{
						this.init(20, false);
					}
				}
				else if (this.level == 3)
				{
					int num8 = X.IntC(this.t / this.ef_intv);
					if (num8 > this.tapped)
					{
						if ((float)num8 >= 12f)
						{
							this.Pr.Mp.DropCon.setLoveJuice(this.Pr, 20, 2868903935U, 1f, false);
							EpAtk epAtkMasturbate = MDAT.EpAtkMasturbate2;
							epAtkMasturbate.val = X.IntC(1000f - (float)this.Pr.ep);
							this.Pr.EpCon.applyEpDamage(epAtkMasturbate, null, EPCATEG_BITS._ALL, 1f, true);
							this.changeLevel(4, 20);
							return true;
						}
						this.Pr.VO.playMasturbVo(3);
						this.M2D.Snd.play("kuchul");
						this.Pr.Mp.DropCon.setLoveJuice(this.Pr, 6, 2868903935U, 1f, false);
						this.Pr.UP.applyMasturbate(0.33f, 40f, 12f);
						this.Pr.PtcVar("level", 3.0).PtcST("masturbate_hit_s", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.tapped = num8;
					}
				}
				else
				{
					if (this.t >= 100f && this.PE.Count != 0)
					{
						this.PE.deactivate(true);
						this.Pr.cureMpNotHunger(false);
						this.Pr.recheck_emot = true;
					}
					if (this.t >= 270f)
					{
						return false;
					}
				}
			}
			if (this.level < 3 && this.type == M2PrMasturbate.TYPE.ON_BENCH)
			{
				if (!this.canceling)
				{
					if (IN.isCancelPD() || IN.isMenuPD(1))
					{
						this.canceling = true;
					}
				}
				else if (IN.isCancelOn(0) || IN.isMenuO(0))
				{
					if (this.TkKD != null)
					{
						this.TkKD.hold_blink = true;
					}
					if (IN.isCancelOn(60) || IN.isMenuO(60))
					{
						this.Pr.recheck_emot = (this.Pr.recheck_emot_in_gm = true);
						return false;
					}
				}
				else
				{
					if (this.TkKD != null)
					{
						this.TkKD.hold_blink = false;
					}
					this.canceling = false;
				}
			}
			return true;
		}

		public void changeLevel(int l, int delay = 0)
		{
			this.level = l;
			this.t = (float)(-1000 - delay);
		}

		public UIPictureBase.EMSTATE checkUiPlayerState(UIPictureBase.EMSTATE def_state)
		{
			if (X.SENSITIVE)
			{
				return def_state;
			}
			def_state &= UIPictureBase.EMSTATE.TORNED;
			if (this.Pr.Ser.has(SER.ORGASM_AFTER))
			{
				def_state |= UIPictureBase.EMSTATE.ORGASM;
			}
			else
			{
				int num = this.level;
				if (num < 0)
				{
					for (int i = MDAT.Apr_ep_threshold.Length - 1; i >= 0; i--)
					{
						if (this.Pr.ep >= MDAT.Apr_ep_threshold[i])
						{
							num = i;
							break;
						}
					}
				}
				if (num >= 4)
				{
					def_state |= UIPictureBase.EMSTATE.ORGASM;
				}
				else if (num == 1 && this.t < 0f)
				{
					def_state |= UIPictureBase.EMSTATE.BATTLE;
				}
				else
				{
					if ((num & 2) != 0)
					{
						def_state |= UIPictureBase.EMSTATE.STUNNED;
					}
					if ((num & 1) != 0)
					{
						def_state |= UIPictureBase.EMSTATE.SMASH;
					}
				}
			}
			return def_state;
		}

		bool IEventWaitListener.EvtWait(bool is_first)
		{
			if (!is_first && this != this.Pr.SpRunner)
			{
				CsvVariableContainer variableContainer = EV.getVariableContainer();
				if (variableContainer != null)
				{
					variableContainer.define("_masturb_success", (this.pre_orgasm_count < this.Pr.EpCon.masturbate_count) ? "1" : "0", true);
				}
				return false;
			}
			return true;
		}

		public void prepareTxKD(bool flag)
		{
			if (flag)
			{
				if (this.TkKD == null)
				{
					if (this.FD_KeyDesc == null)
					{
						this.FD_KeyDesc = new TxKeyDesc.FnGetKD(this.getKD);
					}
					this.TkKD = this.M2D.TxKD.AddTicket(170, this.FD_KeyDesc, this);
					this.TkKD.showable_front_ui = true;
					return;
				}
			}
			else if (this.TkKD != null)
			{
				this.TkKD = this.TkKD.destruct();
			}
		}

		public void getKD(STB Stb, object Target)
		{
			if (this.level < 3)
			{
				Stb.AddTxA("Mstb_KeyHelp", false);
			}
		}

		public float get_time()
		{
			return this.t;
		}

		public bool isActive()
		{
			return this.level >= 0;
		}

		public bool is_bench
		{
			get
			{
				return this.type == M2PrMasturbate.TYPE.ON_BENCH;
			}
		}

		public bool isFinished()
		{
			return this.level >= 4;
		}

		private float t = -2000f;

		private int level = -1;

		private int tapped;

		private M2PrMasturbate.TYPE type;

		private AbsorbManager AbsorbMng;

		private PrGachaItem Gacha;

		public readonly PR Pr;

		public readonly NelM2DBase M2D;

		public readonly NoelAnimator Anm;

		public readonly bool on_bench;

		private string intro_pose_title;

		private float ep_0;

		private float ep_add;

		private float ef_intv = 60f;

		private const int LEVEL_ORGASM = 4;

		public const int LEVEL_TO_COME = 3;

		private const float COMING_TAP = 12f;

		private const float WAIT_DELAY = 140f;

		private PendulumDrawer Pdr;

		private TxKeyDesc.KDTicket TkKD;

		private EffectHandlerPE PE;

		private bool canceling;

		public readonly int pre_orgasm_count;

		public const string varname_orgasm_success = "_masturb_success";

		public const UIPictureBase.EMSTATE BENCH_MASTURB_READ_STATE = UIPictureBase.EMSTATE.TORNED;

		private TxKeyDesc.FnGetKD FD_KeyDesc;

		private enum TYPE
		{
			ON_BENCH,
			ON_ROAD
		}
	}
}
