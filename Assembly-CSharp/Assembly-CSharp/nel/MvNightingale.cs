using System;
using evt;
using m2d;
using XX;

namespace nel
{
	public class MvNightingale : M2EventItem, IEventListener
	{
		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.FD_callbeat = new BGM.FnBeatAttack(this.callBeat);
			BGM.EvBeatCallBack += this.FD_callbeat;
			BGM.addUseBeatFlag("NIG");
			EV.addListener(this);
			this.checked_bcc = false;
			if (SCN.isNightingaleOnlyBag())
			{
				this.only_bag = true;
			}
		}

		public void appearAfter()
		{
			if (this.only_bag)
			{
				this.SpSetPose("only_bag", -1, null, false);
				base.check_desc_name = "EV_access_default_check";
				this.t_hmn = -2f;
			}
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			MvNightingale.fineAisac(-2f, null);
			base.destruct();
			EV.remListener(this);
			BGM.EvBeatCallBack -= this.FD_callbeat;
			BGM.remUseBeatFlag("NIG");
		}

		public override M2EventCommand execute(M2EventItem.CMD _key, IM2EvTrigger executed_by)
		{
			M2EventCommand m2EventCommand = base.execute(_key, executed_by);
			if (_key == M2EventItem.CMD.TALK)
			{
				this.t_hmn = -2f;
				MvNightingale.fineAisac(-2f, null);
			}
			return m2EventCommand;
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			if (!this.checked_bcc && this.Mp.BCC != null && this.Mp.BCC.is_prepared)
			{
				float footableY = this.Mp.getFootableY(base.x, (int)base.y, -12, true, base.y, true, true, true, 0f);
				this.checked_bcc = true;
				if (footableY >= 0f)
				{
					this.moveBy(0f, footableY - this.sizey - base.y, true);
				}
			}
			if (this.only_bag)
			{
				return;
			}
			if (this.Smn != EnemySummoner.ActiveScript)
			{
				this.Smn = EnemySummoner.ActiveScript;
				if (this.Smn == null)
				{
					MvNightingale.fineAisac(this.t_hmn = 0f, this);
					this.SpSetPose("harmonic", -1, null, false);
				}
				else
				{
					MvNightingale.fineAisac(-2f, null);
				}
			}
			if (this.Smn == null)
			{
				if (this.t_hmn >= 0f)
				{
					if (this.t_hmn < 60f)
					{
						this.t_hmn += 1f;
					}
					else if (this.t_hmn < 90f)
					{
						if (this.isNear())
						{
							float num = this.t_hmn + 1f;
							this.t_hmn = num;
							if (num >= 90f)
							{
								this.setAim((base.x < this.Mp.Pr.x) ? AIM.R : AIM.L, false);
								this.SpSetPose("greeting", -1, null, false);
							}
						}
						else
						{
							this.t_hmn = 60f;
						}
					}
					else
					{
						float num = this.t_hmn + 1f;
						this.t_hmn = num;
						if (num >= 115f)
						{
							MvNightingale.fineAisac(this.t_hmn = -1f, this);
						}
					}
					if ((IN.totalframe & 3) == 0)
					{
						MvNightingale.fineAisac(this.t_hmn, this);
						return;
					}
				}
				else
				{
					if (this.t_hmn <= -2f)
					{
						if (EV.isActive(false) || this.isNear())
						{
							this.t_hmn = -2f;
						}
						else
						{
							float num = this.t_hmn - 1f;
							this.t_hmn = num;
							if (num <= -130f)
							{
								MvNightingale.fineAisac(this.t_hmn = 0f, this);
								this.SpSetPose("harmonic", -1, null, false);
							}
						}
					}
					M2MoverPr pr = this.Mp.Pr;
					if (!EV.isActive(false) && pr != null && pr != this && X.Abs(pr.y - base.y) < 2.5f)
					{
						if (base.x < pr.x == (base.mpf_is_right == 1f))
						{
							this.t_turn = 0f;
							return;
						}
						this.t_turn += this.TS;
						if (this.t_turn >= 20f)
						{
							this.setAim((base.x < this.Mp.Pr.x) ? AIM.R : AIM.L, false);
							this.Anm.setAim((int)this.aim, -1);
							this.t_turn = 0f;
							return;
						}
					}
					else
					{
						this.t_turn = 0f;
					}
				}
			}
		}

		public bool isNear()
		{
			PR pr = this.Mp.Pr as PR;
			if (pr == null || pr == this)
			{
				return false;
			}
			float num = X.Abs(pr.x - base.x);
			float num2 = X.Abs(pr.y - base.y);
			if (num + num2 <= 9f)
			{
				pr.EpCon.lockOazuke();
			}
			return num < 4f && num2 < 2.5f;
		}

		private void callBeat(int beat_count)
		{
			if (this.t_hmn >= 0f && this.Anm != null)
			{
				int num = (this.Anm.cframe + 2) / 4 % 2;
				this.Anm.animReset(num * 4 + 2);
			}
		}

		public static void fineAisac(float t, M2Mover Mv = null)
		{
			if (t < 0f || Mv == null)
			{
				BGM.setAisac("vol", 0f);
				BGM.setAisac("vol2", 0f);
				BGM.setAisac("harmonic_pan", 0.5f);
				return;
			}
			M2MoverPr pr = Mv.Mp.Pr;
			float num = X.ZLINE(t, 60f) * (1f - X.ZLINE(t - 90f, 25f) * 0.75f);
			float x = pr.x;
			float y = pr.y;
			float num2 = X.ZLINE(X.Abs(Mv.x - x) - 3f, 8f) * (float)X.MPF(x < Mv.x);
			float num3 = X.ZLINE(X.Abs(Mv.y - y) - 3f, 8f);
			float num4 = X.NI(1f, 0.125f, (X.Abs(num2) + num3) * 0.5f) * num;
			BGM.setAisac("vol", num4);
			BGM.setAisac("vol2", num4);
			BGM.setAisac("harmonic_pan", 0.5f + num2 * 0.5f * num);
		}

		bool IEventListener.EvtRead(EvReader ER, StringHolder rER, int skipping)
		{
			if (rER.cmd == "NIGHTINGALE_CALLBACK")
			{
				if (this.only_bag)
				{
					this.only_bag = false;
					this.SpSetPose("stand", -1, null, false);
					base.check_desc_name = null;
					this.t_hmn = -2f;
				}
				return true;
			}
			return false;
		}

		bool IEventListener.EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		bool IEventListener.EvtClose(bool is_first_or_end)
		{
			return true;
		}

		int IEventListener.EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		bool IEventListener.EvtMoveCheck()
		{
			return true;
		}

		public float t_hmn;

		public EnemySummoner Smn;

		private const int T_BEWARE_START = 90;

		private const int T_BEWARE_BGM_STOP = 25;

		private float t_turn;

		private bool only_bag;

		private bool checked_bcc;

		private BGM.FnBeatAttack FD_callbeat;
	}
}
