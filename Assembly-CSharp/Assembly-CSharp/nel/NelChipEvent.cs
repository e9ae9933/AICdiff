using System;
using m2d;
using XX;

namespace nel
{
	public class NelChipEvent : NelChip, IActivatable, IListenerEvcReload
	{
		public NelChipEvent(string _event_header, M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			this.event_header = _event_header;
			base.arrangeable = true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (!this.canSetEvent())
			{
				return;
			}
			this.Mp.addActivateItem(this);
			this.Mp.getEventContainer().addReloadListener(this);
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.Mp.remActivateItem(this);
			M2EventContainer eventContainer = this.Mp.getEventContainer();
			if (eventContainer != null)
			{
				eventContainer.remReloadListener(this);
				this.Mp.destructEvent(this.Ev);
			}
			this.Ev = null;
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
			return true;
		}

		protected virtual M2EventItem fnMakeEventT(Map2d Mp)
		{
			return Mp.getEventContainer().CreateAndAssignT<M2EventItem>(this.event_key, false);
		}

		public virtual void activate()
		{
			this.activateToDrawer();
		}

		public virtual void deactivate()
		{
			this.deactivateToDrawer();
		}

		public string event_key
		{
			get
			{
				return this.event_header + "_" + base.unique_key;
			}
		}

		public string getActivateKey()
		{
			return this.event_key;
		}

		public virtual bool canSetEvent()
		{
			return true;
		}

		public M2EventItem getEventMover()
		{
			return this.Ev;
		}

		protected M2EventItem Ev;

		protected string event_header = "";

		public float marg_mp_lr;

		public float marg_mp_t;

		public float marg_mp_b;
	}
}
