using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipPuzzleSaisenSwitch : NelChipPuzzleSwitch, IListenerEvcReload
	{
		public NelChipPuzzleSaisenSwitch(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img, "puzzle_saisenswitch")
		{
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.MvHit != null)
			{
				this.Mp.getEventContainer().addReloadListener(this);
			}
		}

		public virtual bool EvtM2Reload(Map2d Mp)
		{
			if (this.Ev != null)
			{
				Mp.destructEvent(this.Ev);
			}
			this.Ev = this.fnMakeEventT(Mp);
			this.Ev.clear();
			float num = (float)this.iwidth / base.CLEN + this.marg_mp_lr * 2f;
			float num2 = (float)this.iheight / base.CLEN + this.marg_mp_t + this.marg_mp_b;
			this.Ev.Size(num / 2f * base.CLEN, num2 / 2f * base.CLEN, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			this.Ev.setTo(this.mapcx, this.mapcy - (float)this.iheight / base.CLEN / 2f - this.marg_mp_t + num2 / 2f);
			this.Ev.event_cy = X.Mn(this.mapcy - 0.25f, this.mbottom - 0.75f);
			this.Ev.talk_with_magic_key = true;
			this.Ev.check_desc_name = "EV_access_manipulate";
			this.fineEventContent();
			return true;
		}

		public override void fineActivation()
		{
			this.fineEventContent();
		}

		public void fineEventContent()
		{
			if (this.Ev == null || this.MvHit == null)
			{
				return;
			}
			if (this.MvHit != null && this.MvHit.isActive() && !this.MvHit.isBelongPuzArea(true))
			{
				this.Ev.trigger_visible = false;
				this.Ev.remove("check");
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AR("STOP_LETTERBOX");
				stb.AR("VALOTIZE");
				stb.AR("TUTO_REM_ACTIVE_FLAG");
				stb.AR("DENY_SKIP");
				stb.Add(this.MvHit.isActive() ? "CHIP_DEACTIVATE " : "CHIP_ACTIVATE ", this.Lay.name).Add(" ", this.index, "").AR("");
				stb.AR("#MS_ % 'P[interact2stand] w15'");
				stb.AR("WAIT_MOVE");
				this.Ev.assign("check", stb.ToString(), false);
			}
		}

		protected M2EventItem fnMakeEventT(Map2d Mp)
		{
			return Mp.getEventContainer().CreateAndAssignT<M2EventItem>(this.event_key, false);
		}

		public string event_key
		{
			get
			{
				return "Switch_" + this.puzzle_id.ToString() + "_" + base.unique_key;
			}
		}

		public override void activate()
		{
			base.activate();
			this.fineEventContent();
		}

		public override void deactivate()
		{
			base.deactivate();
			this.fineEventContent();
		}

		public override bool can_hit()
		{
			return false;
		}

		private M2EventItem Ev;

		public float marg_mp_lr = 0.2f;

		public float marg_mp_t = 0.8f;

		public float marg_mp_b;
	}
}
