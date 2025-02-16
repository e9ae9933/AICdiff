using System;
using System.Text.RegularExpressions;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpMapTransfer : M2LpMapTransferBase
	{
		public M2LpMapTransfer(string _aim, string _jump_key, M2MapLayer L, string __key)
			: base(__key, 0, L)
		{
			this.aim = _aim;
			this.jump_key = _jump_key;
		}

		private void recalcMeta()
		{
			if (this.Meta != null)
			{
				this.walk_in = this.Meta.GetB("walk_in", false);
				this.sync_pos = this.Meta.GetB("sync_pos", false);
			}
		}

		public override string comment
		{
			set
			{
				if (this.comment == value)
				{
					return;
				}
				base.comment = value;
				if (this.Meta == null)
				{
					this.Meta = new META(this.comment);
				}
				else
				{
					this.Meta.ClearLoad(this.comment, null);
				}
				this.recalcMeta();
			}
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.recalcMeta();
		}

		public static void closeMap()
		{
		}

		public override void getDepertureRect()
		{
			this.DepertureRect = this.M2D.WM.CurWM.getDepertureRect(this.Mp, CAim.parseString(this.aim, 0), this.jump_key);
		}

		protected override void event_script_transfer_foot_TB(STB Stb, int deperture_aim)
		{
			if (this.walk_in)
			{
				Stb.AR("IFDEF __walk_in_success {");
				Stb.AR("#MS_ % 'q +G F'");
				Stb.AR("DENY_SKIP");
				Stb.AR("WAIT_MOVE");
				Stb.AR("PR_KEY_SIMULATE $__walk_in_success");
				Stb.AR("WAIT 50");
				Stb.AR("GOTO _FOOT_END");
				Stb.AR("}");
			}
			base.event_script_transfer_foot_TB(Stb, deperture_aim);
			if (this.walk_in)
			{
				Stb.AR("LABEL _FOOT_END");
			}
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			base.initAction(normal_map);
			Map2d mp = this.Lay.Mp;
			M2EventContainer eventContainer = mp.getEventContainer();
			if (eventContainer.Get(this.key, true, true) == null)
			{
				if (this.DepertureRect == null)
				{
					X.de("DepRect が不明: " + this.key, null);
					return;
				}
				if (this.aim == "B" && this.maph < 5)
				{
					X.de("下行きの Transfer は5以上の高さを用意して下さい", null);
				}
				this.Ev = eventContainer.CreateAndAssign(this.key);
				this.Ev.setToArea(this);
				this.Ev.aim = (AIM)CAim.parseString(this.aim, 0);
				if (CAim._XD(this.Ev.aim, 1) != 0)
				{
					int i;
					for (i = 0; i < 3; i++)
					{
						int num = this.mapy + this.maph - 1 + i;
						bool flag = false;
						for (int j = 0; j < 2; j++)
						{
							if (!mp.canStand((this.Ev.aim == AIM.L) ? (this.mapx + this.mapw - 1 - j) : (this.mapx + j), num))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
					if (i > 0)
					{
						this.Ev.Size(this.Ev.sizex * this.Ev.CLENM, (this.Ev.sizey + (float)i * 0.5f) * this.Ev.CLENM, ALIGN.CENTER, ALIGNY.TOP, true);
					}
				}
				WholeMapItem.WMTransferPoint.WMRectItem depertureRect = this.M2D.WM.CurWM.getDepertureRect(mp, (int)this.Ev.aim, this.jump_key);
				if (depertureRect != null)
				{
					depertureRect.Mp.prepared = true;
					if (depertureRect.sync_pos)
					{
						this.sync_pos = true;
					}
					if (depertureRect.walk_in)
					{
						this.walk_in = true;
					}
					using (STB stb = TX.PopBld(null, 0))
					{
						base.event_script_transfer_head(stb, this.aim, true, true, null, -1, this.sync_pos);
						stb.AR("COOK_ADD_WALK_COUNT ", depertureRect.Mp.key);
						stb.AR("INIT_MAP_MATERIAL ", depertureRect.Mp.key, " 1");
						stb.AR("INIT_MAP_BGM ", depertureRect.Mp.key);
						stb.AR("__walk_in=", this.walk_in ? "1" : "0");
						base.event_script_transfer_body(stb, this.aim, this.jump_key, this.walk_in);
						base.event_script_transfer_foot(stb, null, this.sync_pos);
						this.Ev.assign("STAND", stb.ToString(), true);
					}
					base.fineSF();
					if (CAim._XD(CAim.parseString(this.aim, 0), 1) == 0)
					{
						this.Ev.stand_extend_map_w = 0.25f;
						return;
					}
				}
				else
				{
					X.de("Tranfer" + this.key + " が正しくセットされていません。 WholeMap の該当レイヤーを保存して下さい。 ", null);
				}
			}
		}

		public const string transfer_lp_header = "Exit";

		public static readonly Regex RegTransferLp_After = new Regex("_Exit([LTRB])_([^\\s ]+)");

		public static readonly Regex RegTransferLp = new Regex("^Exit([LTRB])_([^\\s ]+)");

		public static readonly Regex RegTransferDoorLp = new Regex("^ExitD_([^\\s ]+)");

		public static readonly Regex RegWorpLp = new Regex("^ExitW([LTRB])_([^\\s ]+)");

		public static readonly Regex RegForAimCalc = new Regex("^Exit[A-Z]([LTRB])");

		public const string grep_key = "lp,Exit";

		private string aim;

		private string jump_key;

		public bool walk_in;

		public bool sync_pos;
	}
}
