using System;
using evt;
using XX;

namespace m2d
{
	public class M2LpTutoArea : M2LpNearCheck
	{
		public M2LpTutoArea(string __key, int _i, M2MapLayer L)
			: base(__key, 0, L)
		{
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			META meta = new META(this.comment);
			this.tuto_key = meta.GetSi(0, "tuto_msg");
			this.tuto_if = meta.GetSi(1, "tuto_msg");
			this.tuto_time = meta.GetI("tuto_time", -1, 0);
			this.Atuto_pos = meta.Get("tuto_pos");
			this.Atuto_cap = meta.Get("tuto_cap");
			this.deactivating_enable = !meta.GetB("pre_on", true);
			this.enabled_ = !this.deactivating_enable;
			this.tuto_activated = false;
			this.T_RECHECK = (float)(TX.valid(this.tuto_if) ? 30 : 60);
			this.T_FADE = 1f;
		}

		public override bool nearCheck(M2Mover Mv, NearTicket NTk)
		{
			return this.enabled_ && base.nearCheck(Mv, NTk);
		}

		public override void initEnter(M2Mover Mv)
		{
			if (!this.enabled_)
			{
				return;
			}
			if (!base.activated && (!TX.valid(this.tuto_if) || TX.evalI(this.tuto_if) != 0))
			{
				this.tutoActivate();
			}
			base.initEnter(Mv);
		}

		public override void quitEnter(M2Mover Mv)
		{
			this.tutoDectivate();
			base.quitEnter(Mv);
		}

		protected virtual string getShowTutoKey()
		{
			string text = this.tuto_key;
			if (text != null && (text == "Tuto_run_right" || text == "Tuto_run_left"))
			{
				if (M2MoverPr.jump_press_reverse)
				{
					return null;
				}
				if (M2MoverPr.double_tap_running)
				{
					text += "_and_dtap";
				}
			}
			return text;
		}

		private void tutoActivate()
		{
			if (this.tuto_activated || !this.enabled_)
			{
				return;
			}
			this.tuto_activated = true;
			ITutorialBox tutoBox = EV.TutoBox;
			if (tutoBox != null)
			{
				string showTutoKey = this.getShowTutoKey();
				if (showTutoKey != null)
				{
					tutoBox.AddText(TX.Get(showTutoKey, ""), this.tuto_time, "");
					if (this.Atuto_cap != null && this.Atuto_cap.Length >= 2)
					{
						int num = this.Atuto_cap.Length / 2 * 2;
						for (int i = 0; i < num; i += 2)
						{
							EvImg pic = EV.Pics.getPic(this.Atuto_cap[i], true, true);
							if (pic == null || pic.PF == null)
							{
								X.de("M2LpTutoArea 不明なイメージ: " + this.Atuto_cap[i], null);
							}
							else
							{
								tutoBox.AddImage(pic.PF, this.Atuto_cap[i + 1]);
							}
						}
					}
					if (this.Atuto_pos != null)
					{
						if (this.Atuto_pos.Length == 1)
						{
							tutoBox.setPosition(this.Atuto_pos[0].ToUpper(), this.Atuto_pos[0].ToUpper());
							return;
						}
						tutoBox.setPosition(this.Atuto_pos[0].ToUpper(), this.Atuto_pos[1].ToUpper());
					}
				}
			}
		}

		private void tutoDectivate()
		{
			if (this.tuto_activated)
			{
				ITutorialBox tutoBox = EV.TutoBox;
				if (tutoBox != null)
				{
					tutoBox.RemText(true, false);
				}
				this.tuto_activated = false;
			}
		}

		public override void activate()
		{
			this.enabled = !this.deactivating_enable;
		}

		public override void deactivate()
		{
			this.enabled = this.deactivating_enable;
		}

		public bool enabled
		{
			get
			{
				return this.enabled_;
			}
			set
			{
				if (this.enabled == value)
				{
					return;
				}
				this.enabled_ = value;
				if (value)
				{
					base.recheckNM();
					return;
				}
				this.tutoDectivate();
			}
		}

		public override bool recheck(M2Mover Mv)
		{
			if (!base.recheck(Mv))
			{
				return false;
			}
			if (TX.valid(this.tuto_if))
			{
				if (TX.evalI(this.tuto_if) != 0)
				{
					this.tutoActivate();
				}
				else
				{
					this.tutoDectivate();
				}
			}
			return true;
		}

		protected string tuto_key = "";

		private string tuto_if = "";

		private int tuto_time = -1;

		private string[] Atuto_pos;

		private string[] Atuto_cap;

		private bool tuto_activated;

		private bool deactivating_enable;

		private bool enabled_ = true;
	}
}
