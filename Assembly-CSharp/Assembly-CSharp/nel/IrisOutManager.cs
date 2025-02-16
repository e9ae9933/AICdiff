using System;
using System.Collections.Generic;
using evt;
using XX;

namespace nel
{
	public class IrisOutManager
	{
		public IrisOutManager(NelM2DBase _M2D)
		{
			this.AMng = new List<IIrisOutListener>(2);
			this.M2D = _M2D;
		}

		public void clear()
		{
			this.AMng.Clear();
			if (this.PeIris != null)
			{
				this.PeIris.destruct();
				this.PeIris = null;
			}
			this.t = -1;
			this.force_wakeup_input = 0;
		}

		public IrisOutManager assignListener(IIrisOutListener Lsn)
		{
			if (this.AMng.IndexOf(Lsn) == -1)
			{
				this.AMng.Add(Lsn);
			}
			return this;
		}

		public IrisOutManager deassignListener(IIrisOutListener Lsn)
		{
			this.AMng.Remove(Lsn);
			return this;
		}

		public IrisOutManager ForceWakeupInput(bool immediate = false)
		{
			if (this.AMng.Count > 0 && this.t < 0)
			{
				this.force_wakeup_input = (immediate ? 2 : 1);
			}
			else if (immediate && this.t >= 0 && this.ActivatePr != null)
			{
				this.t = 180;
			}
			return this;
		}

		public bool isWaiting(PR Pr, IrisOutManager.IRISOUT_TYPE type)
		{
			bool flag = false;
			for (int i = this.AMng.Count - 1; i >= 0; i--)
			{
				IIrisOutListener irisOutListener = this.AMng[i];
				if (irisOutListener.getIrisOutKey() == type && irisOutListener.initSubmitIrisOut(Pr, false, ref flag))
				{
					return true;
				}
			}
			return false;
		}

		public bool isWaiting(PR Pr)
		{
			bool flag = false;
			for (int i = this.AMng.Count - 1; i >= 0; i--)
			{
				if (this.AMng[i].initSubmitIrisOut(Pr, false, ref flag))
				{
					return true;
				}
			}
			return false;
		}

		public bool isTrapping()
		{
			if (this.t < 0)
			{
				return this.AMng.Count > 0;
			}
			return this.ActivatePr != null;
		}

		public bool isIrisActive()
		{
			return this.t >= 0;
		}

		public void runPost(PR Pr, bool executable)
		{
			if (this.t < 0)
			{
				if (!executable || Pr == null || this.AMng.Count == 0 || EV.isActive(false))
				{
					return;
				}
				if (this.force_wakeup_input == 0 && !Pr.WakeUpInput(false, false))
				{
					return;
				}
				bool flag = false;
				bool flag2 = false;
				for (int i = this.AMng.Count - 1; i >= 0; i--)
				{
					flag = this.AMng[i].initSubmitIrisOut(Pr, true, ref flag2) || flag;
				}
				if (flag)
				{
					bool flag3 = this.force_wakeup_input == 2;
					this.force_wakeup_input = 0;
					IN.clearPushDown(true);
					this.ActivatePr = Pr;
					if (!flag2 && !flag3)
					{
						this.PeIris = this.M2D.PE.setPE(POSTM.IRISOUT, 60f, 1.05f, 0);
					}
					this.t = 0;
					if (flag3)
					{
						this.t = 180;
						this.runPost(Pr, true);
						return;
					}
				}
			}
			else
			{
				byte b = this.force_wakeup_input;
				if (this.PeIris != null)
				{
					this.PeIris.fine(120);
				}
				if (this.ActivatePr != null)
				{
					this.t++;
					if (this.PeIris != null)
					{
						this.PeIris.af = (float)this.t;
					}
					if (((this.t >= 64 && X.D_EF) || this.t >= 180) && Pr != null)
					{
						PR.STATE state = PR.STATE.DAMAGE_L_LAND;
						string text = "dmg_down2";
						this.event_keys = (IrisOutManager.IRISOUT_TYPE)0U;
						for (int j = this.AMng.Count - 1; j >= 0; j--)
						{
							IIrisOutListener irisOutListener = this.AMng[j];
							if (irisOutListener.warpInitIrisOut(Pr, ref state, ref text))
							{
								this.event_keys |= irisOutListener.getIrisOutKey();
							}
						}
						Pr.releaseFromIrisOut(true, state, text, true);
						this.t = 0;
						if (this.PeIris != null)
						{
							this.PeIris.x = 4f;
						}
						this.ActivatePr = null;
						return;
					}
				}
				else
				{
					int num = this.t + 1;
					this.t = num;
					if (num >= 3)
					{
						this.t = -1;
						if (this.PeIris != null)
						{
							this.PeIris.x = 1f;
							this.PeIris.deactivate(false);
						}
						this.PeIris = null;
						if (this.event_keys > (IrisOutManager.IRISOUT_TYPE)0U)
						{
							int num2 = X.beki_cnt(256U);
							string text2 = "|";
							for (int k = 0; k < num2; k++)
							{
								IrisOutManager.IRISOUT_TYPE irisout_TYPE = (IrisOutManager.IRISOUT_TYPE)(1 << k);
								if ((this.event_keys & irisout_TYPE) != (IrisOutManager.IRISOUT_TYPE)0U)
								{
									text2 = text2 + irisout_TYPE.ToString() + "|";
								}
							}
							PUZ.IT.callRevertEvent(true, text2);
						}
						this.event_keys = (IrisOutManager.IRISOUT_TYPE)0U;
					}
				}
			}
		}

		private List<IIrisOutListener> AMng;

		public readonly NelM2DBase M2D;

		private PostEffectItem PeIris;

		private PR ActivatePr;

		public byte force_wakeup_input;

		private IrisOutManager.IRISOUT_TYPE event_keys;

		public const int IRISOUT_FADET = 60;

		private int t = -1;

		public enum IRISOUT_TYPE : uint
		{
			WORM = 1U,
			WATER,
			PRESS = 4U,
			LAVA = 8U,
			_ALL = 255U
		}
	}
}
