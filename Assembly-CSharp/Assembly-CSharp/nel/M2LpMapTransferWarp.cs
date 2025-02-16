using System;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpMapTransferWarp : M2LpMapTransferBase
	{
		public M2LpMapTransferWarp(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			this.use_out_collider = 1;
		}

		public override void getDepertureRect()
		{
			this.DepertureRect = (this.Mp.M2D as NelM2DBase).WM.CurWM.getDepertureRect(this.Mp, -2, this.key);
		}

		public WholeMapItem getDepertureDestWMI()
		{
			if (this.DepertureRect == null)
			{
				this.getDepertureRect();
			}
			return this.M2D.WM.GetWholeFor(this.DepertureRect.Mp, false);
		}

		public AIM getDestAim()
		{
			if (this.DepertureRect == null)
			{
				this.getDepertureRect();
			}
			return this.DepertureRect.getAim();
		}

		public override void event_script_transfer_fadein(STB Stb)
		{
			if (this.wm_change)
			{
				AIM aim = this.DepertureRect.getAim();
				Stb.AR("PIC_FILL ", base.screen_layer, " ff:#F1F1E9");
				Stb.AR("PIC_FADEIN ", base.screen_layer, " 10");
				Stb.Add("PIC_TFADE ", base.screen_layer, " ").Add((aim == AIM.L) ? "L2R" : ((aim == AIM.R) ? "R2L" : "EXPAND")).Ret("\n");
				return;
			}
			base.event_script_transfer_fadein(Stb);
		}

		public override void event_script_transfer_fadeout(STB Stb)
		{
			base.event_script_transfer_fadeout(Stb);
			if (this.wm_change)
			{
				AIM aim = this.DepertureRect.getAim();
				Stb.Add("PIC_TFADE ", base.screen_layer, " ").Add((aim == AIM.L) ? "L2R" : ((aim == AIM.R) ? "R2L" : "EXPAND")).Ret("\n");
			}
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			base.initAction(normal_map);
			if (this.DepertureRect == null)
			{
				return;
			}
			WholeMapItem depertureDestWMI = this.getDepertureDestWMI();
			Map2d mp = this.Lay.Mp;
			WholeMapItem wholeFor = this.M2D.WM.GetWholeFor(mp, false);
			this.wm_change = (this.flushing = this.M2D.WM.CurWM != depertureDestWMI);
			M2EventContainer eventContainer = mp.getEventContainer();
			if (!eventContainer.Get(this.key, true, true) && this.DepertureRect != null)
			{
				this.Ev = eventContainer.CreateAndAssign(this.key);
				this.Ev.setToArea(this);
				this.Ev.aim = this.DepertureRect.getAim();
				this.DepertureRect.Mp.prepared = true;
				using (STB stb = TX.PopBld(null, 0))
				{
					if (this.wm_change)
					{
						M2LpMapTransferWarp.GoOutOtherAreaScript(stb, wholeFor, depertureDestWMI, this.DepertureRect.Mp, this);
						stb.AR("PRE_FLUSH_MAP");
					}
					string text = this.Meta.GetSi(0, "jump_at");
					if (text != null)
					{
						text = "!" + text;
					}
					this.WriteGoOutOtherAreaAfter(stb, wholeFor, depertureDestWMI, this.wm_change && !wholeFor.safe_area && depertureDestWMI.safe_area, text, null);
					this.Ev.assign("CHECK", stb.ToString(), true);
					if (this.wm_change)
					{
						this.Ev.check_desc_name = TX.GetA("Destination", this.Meta.GetSi(0, "desc_name") ?? depertureDestWMI.localized_name);
					}
					else
					{
						this.Ev.check_desc_name = "EV_access_into";
					}
				}
				string si = this.Meta.GetSi(0, "useable");
				if (si != null && TX.eval(si, "") == 0.0)
				{
					this.Ev.setExecutableAll(false);
				}
			}
		}

		public void WriteGoOutOtherAreaAfter(STB Stb, WholeMapItem CurWmi, WholeMapItem DepWM, bool autosave, string jump_key = null, WholeMapItem.WMTransferPoint.WMRectItem DepRectOverwriting = null)
		{
			if (CurWmi == null)
			{
				Map2d mp = this.Lay.Mp;
				CurWmi = this.M2D.WM.GetWholeFor(mp, false);
			}
			WholeMapItem.WMTransferPoint.WMRectItem depertureRect = this.DepertureRect;
			if (DepRectOverwriting != null)
			{
				this.DepertureRect = DepRectOverwriting;
			}
			Stb.Add("SEND_EVENT_CORRUPTION ").AR("PRE_UNLOAD");
			if (!this.wm_change)
			{
				Stb.AR("COOK_ADD_WALK_COUNT ", this.DepertureRect.Mp.key);
			}
			base.event_script_transfer_head(Stb, "-2", this.Ev.aim == AIM.L || this.Ev.aim == AIM.R, false, null, -1, false);
			if (this.Ev.aim == AIM.B || this.Ev.aim == AIM.T)
			{
				Stb.Add("#MS_ % '>[ ", (int)(base.mapcx * base.CLEN), " ,+=0 :").Add(13).Add(" ] '")
					.Ret("\n");
			}
			if (CurWmi.safe_area && !DepWM.safe_area)
			{
				Stb.ARd("@___city_guild/__FLUSH_SAFE_AREA ", " ", CurWmi.text_key, DepWM.text_key, "0", null);
			}
			base.event_script_transfer_body(Stb, "-2", TX.valid(jump_key) ? jump_key : this.key, false);
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.wm_change)
				{
					if (DepRectOverwriting == null)
					{
						stb.ARd("WA_DEPERTURE", " ", CurWmi.text_key, this.Mp.key, DepWM.text_key, this.DepertureRect.Mp.key);
					}
					stb.ARd("CHANGE_EVENT2 __M2D_FLUSHED_AREA", " ", CurWmi.text_key, DepWM.text_key, this.Mp.key, this.DepertureRect.Mp.key);
					if (CurWmi.safe_area && !DepWM.safe_area)
					{
						stb.ARd("SAVE_SAFEAREA_DEPERTURE ", " ", this.DepertureRect.Mp.key, this.DepertureRect.another_key_for_worp, null, null);
						stb.AR("IFDEF _initialize_night_level {");
						stb.AR("   DANGER $_initialize_night_level 1");
						stb.AR("} ELSE {");
						stb.AR("   DANGER 0 1");
						stb.AR("}");
						stb.AR("DANGER_INITIALIZE_MEMORY");
					}
				}
				if (this.Ev.aim == AIM.B || this.Ev.aim == AIM.T)
				{
					Stb.Add("#MS_ % 'P[walk~~] W", 13, "'").Ret("\n");
					Stb.Add(stb).Ret("\n");
				}
				else
				{
					base.event_script_transfer_foot(Stb, stb, false);
				}
			}
			if (this.wm_change)
			{
				Stb.AR("PR_KEY_SIMULATE L 0");
				Stb.AR("PR_KEY_SIMULATE R 0");
				if (DepWM.safe_area)
				{
					Stb.AR("MV_CURE 0 0 1");
				}
				Stb.AR("WAIT_FN REELMNG");
				if (!CurWmi.safe_area && DepWM.safe_area)
				{
					Stb.ARd("@___city_guild/__FLUSH_SAFE_AREA ", " ", CurWmi.text_key, DepWM.text_key, "1", null);
				}
			}
			if (autosave)
			{
				Stb.AR("AUTO_SAVE");
			}
			if (DepRectOverwriting != null)
			{
				this.DepertureRect = depertureRect;
			}
		}

		public static void GoOutOtherAreaScript(STB Stb, WholeMapItem Wmi, WholeMapItem DepWM, Map2d DepMp, M2LpMapTransferWarp FromLp)
		{
			Stb.AR("STOP_LETTERBOX");
			Stb.AR("_result=1");
			Stb.Add("CHANGE_EVENT2 __M2D_GOOUT_OTHER_AREA ", Wmi.text_key, " ", DepWM.text_key).Ret("\n");
			Stb.AR("IF $_result'==0' SEEK_END");
			if (Wmi != null && Wmi.safe_area != DepWM.safe_area)
			{
				Stb.AR("_result=0");
				Stb.AR("CONFIRM_AREA_CHANGE ", DepWM.Mp.key);
				Stb.AR("IF $_result'==0' SEEK_END");
				if (Wmi.safe_area && !DepWM.safe_area)
				{
					Stb.AR("_result=0");
					if (FromLp != null)
					{
						Stb.Add("DANGER_LEVEL_INIT_BOX ", DepWM.text_key, " ").AR(FromLp.Lay.name, "..", FromLp.key);
					}
					Stb.AR("IF $_result'<0' SEEK_END");
					Stb.AR("IF $_result'>0' {");
					Stb.AR("   _initialize_night_level=$_result");
					Stb.AR("}");
				}
			}
			if (DepMp != null)
			{
				Stb.AR("INIT_MAP_BGM ", DepMp.key);
			}
			Stb.AR("ADD_MAPFLUSH_FLAG \n");
		}

		private bool wm_change;
	}
}
