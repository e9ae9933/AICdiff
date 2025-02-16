using System;
using System.Text.RegularExpressions;
using evt;
using m2d;
using XX;

namespace nel
{
	public sealed class M2PrGachaEventHandler : IGachaListener, IEventWaitListener, IRunAndDestroy
	{
		public M2PrGachaEventHandler(PR _Pr, PrGachaItem.TYPE _gacha_type, int _tap_count)
		{
			this.Pr = _Pr;
			this.Mp = this.Pr.Mp;
			this.gacha_type = _gacha_type;
			this.tap_count = _tap_count;
			this.t = -1f;
			this.Mp.addRunnerObject(this);
			EV.addAllocEvHandleKey(KEY.SIMKEY.Z, true);
		}

		public void Init(StringHolder rER)
		{
			string _ = rER._3;
			this.failure_abort = _.IndexOf('F') >= 0;
			if (_.IndexOf('P') >= 0 && REG.match(_, M2PrGachaEventHandler.RegFlagPose))
			{
				this.target_pose_key = REG.R1;
			}
			if (_.IndexOf('D') >= 0 && REG.match(_, M2PrGachaEventHandler.RegFlagPendulumIntv))
			{
				this.pendulum_t = X.Nm(REG.R1, 0f, false);
			}
			if (_.IndexOf('D') >= 0 && REG.match(_, M2PrGachaEventHandler.RegFlagPendulumAcceptIntv))
			{
				this.pendulum_accept_t = X.Nm(REG.R1, 0f, false);
			}
			if (_.IndexOf('D') >= 0 && REG.match(_, M2PrGachaEventHandler.RegFlagPendulumIgnoreCount))
			{
				this.pendulum_ignore_count = X.NmI(REG.R1, 0, false, false);
			}
		}

		public void destruct()
		{
			if (this.AbsorbMng != null)
			{
				this.AbsorbMng.destruct();
			}
			this.AbsorbMng = null;
			this.Gacha = null;
		}

		public void deactivate()
		{
			EV.addAllocEvHandleKey(KEY.SIMKEY.Z, false);
			this.destruct();
			if (this.t >= 0f)
			{
				this.t = ((this.stt == M2PrGachaEventHandler.STATE.GACHA_SUCCESS || this.stt == M2PrGachaEventHandler.STATE.FAILURE_ABORT) ? (-30f) : (-1f));
			}
		}

		public bool run(float fcnt)
		{
			if (this.t >= 0f)
			{
				if (this.stt == M2PrGachaEventHandler.STATE.GACHA)
				{
					if (this.AbsorbMng == null || this.Gacha == null)
					{
						this.deactivate();
						return true;
					}
					if (this.Gacha != null)
					{
						if (this.pendulum_ignore_count > 0)
						{
							PendulumDrawer pendulumDrawer = this.Gacha.getPendulumDrawer();
							if (pendulumDrawer != null && pendulumDrawer.ignore_count >= this.pendulum_ignore_count)
							{
								this.Gacha.ErrorOccured(false, true);
							}
						}
						if (this.Gacha.isErrorInput() && this.failure_abort)
						{
							this.t = 0f;
							this.stt = M2PrGachaEventHandler.STATE.FAILURE_ABORT;
							return true;
						}
					}
				}
				if (this.stt == M2PrGachaEventHandler.STATE.FAILURE_ABORT && this.t >= 20f)
				{
					this.deactivate();
					return true;
				}
				this.t += fcnt;
			}
			else
			{
				if (this.stt == M2PrGachaEventHandler.STATE.GACHA_WAITING && this.Pr.isNormalState())
				{
					AbsorbManagerContainer absorbContainer = this.Pr.getAbsorbContainer();
					this.AbsorbMng = this.Pr.getAbsorbContainer().initSpecialGachaNotDiffFix(this.Pr, this, this.gacha_type, this.tap_count, KEY.SIMKEY.Z, true);
					if (this.AbsorbMng != null)
					{
						absorbContainer.timeout = 0;
						this.AbsorbMng.kirimomi_release = false;
						this.AbsorbMng.target_pose = this.target_pose_key;
						this.Gacha = this.AbsorbMng.get_Gacha();
						absorbContainer.renderable_on_evt_stop_ghandle = true;
						if (this.pendulum_t > 0f || this.pendulum_accept_t > 0f)
						{
							PendulumDrawer pendulumDrawer2 = this.Gacha.getPendulumDrawer();
							if (pendulumDrawer2 != null)
							{
								if (this.pendulum_t > 0f)
								{
									pendulumDrawer2.intv_t = (int)this.pendulum_t;
								}
								if (this.pendulum_accept_t > 0f)
								{
									pendulumDrawer2.accept_t = (int)this.pendulum_accept_t;
								}
								pendulumDrawer2.recalc(true);
							}
						}
						this.Pr.Skill.killHoldMagic(false, false);
						if (TX.valid(this.target_pose_key))
						{
							this.Pr.SpSetPose(this.target_pose_key, -1, this.target_pose_key, false);
							this.Pr.breakPoseFixOnWalk(2);
						}
						this.t = 0f;
						this.Pr.setStateToEvGacha();
						this.stt = M2PrGachaEventHandler.STATE.GACHA;
					}
					return true;
				}
				this.t -= fcnt;
				if (this.t <= -30f)
				{
					this.destruct();
					return false;
				}
			}
			return true;
		}

		public bool canAbsorbContinue()
		{
			return this.Pr.isEvGachaState() && EV.isActive(false);
		}

		public void absorbFinished(bool abort)
		{
			this.AbsorbMng = null;
			if (!abort)
			{
				this.stt = M2PrGachaEventHandler.STATE.GACHA_SUCCESS;
			}
			this.deactivate();
			if (this.t > -30f)
			{
				this.stt = M2PrGachaEventHandler.STATE.GACHA_WAITING;
			}
		}

		public bool individual
		{
			get
			{
				return true;
			}
		}

		public bool EvtWait(bool is_first = false)
		{
			if (this.Mp != this.Pr.Mp)
			{
				return false;
			}
			if (this.t <= -30f)
			{
				EV.getVariableContainer().define("_result", (this.stt == M2PrGachaEventHandler.STATE.GACHA_SUCCESS) ? "1" : "0", true);
				return false;
			}
			return true;
		}

		public readonly PR Pr;

		public readonly Map2d Mp;

		public string target_pose_key;

		private float t;

		private float pendulum_t = -1f;

		private float pendulum_accept_t = -1f;

		public int pendulum_ignore_count = -1;

		private M2PrGachaEventHandler.STATE stt;

		public PrGachaItem.TYPE gacha_type;

		public int tap_count;

		public const float MAXT = 30f;

		private AbsorbManager AbsorbMng;

		private PrGachaItem Gacha;

		private bool failure_abort;

		public const char FLAG_ABORT_ON_FAILURE = 'F';

		public const char FLAG_POSE = 'P';

		public const char FLAG_PENDULUM_INTV = 'D';

		private static Regex RegFlagPose = new Regex("P\\[([\\w]+)\\]");

		private static Regex RegFlagPendulumIntv = new Regex("D\\[([\\d\\.]+)\\]");

		private static Regex RegFlagPendulumAcceptIntv = new Regex("Da\\[([\\-\\d\\.]+)\\]");

		private static Regex RegFlagPendulumIgnoreCount = new Regex("Di\\[([\\-\\d]+)\\]");

		private enum STATE
		{
			GACHA_WAITING,
			GACHA,
			GACHA_SUCCESS,
			FAILURE_ABORT
		}
	}
}
