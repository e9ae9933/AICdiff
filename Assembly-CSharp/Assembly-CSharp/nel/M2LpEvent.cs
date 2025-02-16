using System;
using m2d;
using XX;

namespace nel
{
	public class M2LpEvent : M2LabelPoint, IListenerEvcReload
	{
		public M2LpEvent(string __key, int _i, M2MapLayer L)
			: base(__key, _i, L)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.Meta = new META(this.comment);
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map || !this.canSetEvent())
			{
				return;
			}
			this.closeAction(false);
			this.Lay.Mp.getEventContainer().addReloadListener(this);
		}

		protected virtual M2EventItem createEvent()
		{
			if (this.Ev != null)
			{
				this.Mp.destructEvent(this.Ev);
			}
			return this.Lay.Mp.getEventContainer().CreateAndAssign(base.unique_key);
		}

		public virtual bool EvtM2Reload(Map2d Mp)
		{
			if (Mp.Dgn.is_editor)
			{
				return true;
			}
			this.Ev = this.createEvent();
			this.Ev.clear();
			this.Ev.Size((float)(this.mapw / 2) * base.CLEN, (float)(this.maph / 2) * base.CLEN, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			this.Ev.setTo(base.mapcx, base.mapcy);
			return true;
		}

		public override void closeAction(bool when_map_close)
		{
			M2EventContainer eventContainer = this.Mp.getEventContainer();
			if (eventContainer != null)
			{
				eventContainer.remReloadListener(this);
				this.Mp.destructEvent(this.Ev);
			}
			this.Ev = null;
		}

		public META getMeta()
		{
			return this.Meta;
		}

		public virtual bool canSetEvent()
		{
			return true;
		}

		protected META Meta;

		public M2EventItem Ev;
	}
}
