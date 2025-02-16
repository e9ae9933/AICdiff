using System;
using evt;
using m2d;
using XX;

namespace nel
{
	public class NelNPuppyEvent : NelNPuppy
	{
		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.Nai.fnSearchPlayer = (float f) => null;
			this.Nai.fnSleepLogic = new NAI.FnNaiLogic(this.considerSleep);
		}

		public override bool isRingOut()
		{
			return false;
		}

		protected bool considerSleep(NAI Nai)
		{
			return Nai.AddTicketB(NAI.TYPE.WAIT, 128, true);
		}

		public override bool readTicket(NaTicket Tk)
		{
			return Tk.type == NAI.TYPE.WAIT && this.runWait(Tk.initProgress(this), Tk);
		}

		public bool runWait(bool init_flag, NaTicket Tk)
		{
			if (init_flag)
			{
				base.throw_ray = true;
				this.walk_time = 0f;
				this.setAim(AIM.L, false);
				this.Phy.addLockMoverHitting(HITLOCK.EVENT, -1f);
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (base.M2D.Cam.isCoveringMp(base.x - 0.12f, base.y, base.x + 0.12f, base.y, 0f))
				{
					if (Tk.Progress(ref this.t, 35, true))
					{
						this.setAim(AIM.R, false);
						this.SpSetPose("walk", -1, null, false);
						this.FlgSmall.Add("ATK");
						this.floating = true;
						this.Phy.addLockWallHitting(this, -1f);
						base.createWalkSnd();
						if (TX.valid(this.jump_event))
						{
							EV.stack(this.jump_event, 0, -1, null, null);
						}
					}
				}
				else
				{
					this.t = 0f;
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				this.Phy.setWalkXSpeed(base.mpf_is_right * 0.23f * 2f * X.ZLINE(this.t + 5f, 18f), true, false);
				if (!base.M2D.Cam.isCoveringMp(base.x - 2f, base.y, base.x + 2f, base.y, 0f))
				{
					if (Tk.Progress(ref this.t, 20, true))
					{
						base.disappearing = true;
						base.deactivateWalkSnd();
					}
				}
				else
				{
					this.t = 0f;
				}
			}
			return true;
		}

		public override bool initDeath()
		{
			return false;
		}

		public override bool createTicketFromEvent(StringHolder rER)
		{
			if (!base.createTicketFromEvent(rER))
			{
				this.jump_event = rER._1;
			}
			return true;
		}

		private string jump_event;
	}
}
