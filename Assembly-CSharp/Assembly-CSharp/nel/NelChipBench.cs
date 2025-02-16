using System;
using evt;
using m2d;
using nel.gm;
using XX;

namespace nel
{
	public class NelChipBench : NelChipEvent
	{
		public NelChipBench(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base("bench_", _Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			this.marg_mp_lr = (float)(-(float)this.iwidth) / base.CLEN / 4f * 1.35f;
		}

		public override void initAction(bool normal_map)
		{
			if (base.active_removed)
			{
				return;
			}
			this.player_aim = base.Meta.getDirsI("bench", this.rotation, this.flip, 0, -1);
			this.bottom_fix_ceil = base.Meta.GetB("bottom_fix_ceil", false);
			M2CImgDrawerBench m2CImgDrawerBench = base.CastDrawer<M2CImgDrawerBench>();
			if (m2CImgDrawerBench != null)
			{
				this.scale = m2CImgDrawerBench._scale;
			}
			this.shift_pixel = base.Meta.GetNm("bench", 0f, 1);
			this.bottom_raise_px = base.Meta.GetNm("bottom_raise_px", 0f, 0);
			base.initAction(normal_map);
			base.NM2D.WDR.getNightingale().checkBench(this, -1f, 0f, true);
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.Wm = null;
		}

		public void initSitDown(PR Pr, bool do_save, bool set_effect = true)
		{
			if (!EV.isActive(false) && Pr.isOnBenchAndCanShowModal())
			{
				UiBenchMenu.showModalOfflineOnBench();
			}
			int num = 0;
			if (do_save)
			{
				Pr.cureMpNotHunger(false);
				num = UiBenchMenu.ExecuteBenchCmd(-1, 0, true, false, false);
			}
			this.fineIcon();
			if (do_save && CFG.autosave_on_bench && SCN.canSave(true))
			{
				COOK.autoSave(base.NM2D, true, false).ShowDelay((float)num);
			}
			if (set_effect)
			{
				Pr.PtcST("bench_sitdown", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				Pr.TeCon.setColorBlinkAdd(4f, 50f, 0.5f, 8585074, 0);
			}
		}

		public void fineIcon()
		{
			if (this.Wm == null)
			{
				this.Wm = new WMIconCreator(this, WMIcon.TYPE.BENCH, null, this.Mp.key + "_" + this.Lay.name + "_bench");
				this.Wm.notice();
			}
		}

		public bool IconIs(WMIcon Ico)
		{
			return this.Wm != null && Ico == this.Wm.getIcon();
		}

		public override bool EvtM2Reload(Map2d Mp)
		{
			base.EvtM2Reload(Mp);
			this.Ev.check_desc_name = "EV_access_sitdown";
			string text = "STOP_LETTERBOX\nVALOTIZE\nDENY_SKIP\nBENCH_SITDOWN % " + this.mapcx.ToString() + " " + this.mapcy.ToString();
			this.Ev.assign("talk", text, true);
			return true;
		}

		public int player_aim = -1;

		public float shift_pixel;

		public float scale;

		public float bottom_raise_px;

		public bool bottom_fix_ceil;

		private WMIconCreator Wm;
	}
}
