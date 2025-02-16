using System;
using System.IO;
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

		public M2EventContainer EvCon
		{
			get
			{
				return this.Mp.getEventContainer();
			}
		}

		public override void initAction(bool normal_map)
		{
			if (!this.canSetEvent())
			{
				return;
			}
			this.closeAction(false);
			if (this.Mp.getEventContainer() == null)
			{
				this.Mp.prepareCommand(null);
			}
			this.target_script = this.Meta.GetSi(0, "script");
			this.EvCon.addReloadListener(this);
		}

		protected virtual M2EventItem createEvent()
		{
			if (this.Ev != null)
			{
				this.Mp.destructEvent(this.Ev);
			}
			return this.EvCon.CreateAndAssign(base.unique_key);
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
			if (this.target_script != null)
			{
				string text = NKT.readStreamingText(Path.Combine("evt/___m2d_cmd", this.target_script + ".cmd"), false);
				if (TX.valid(text))
				{
					CsvReaderA csvReaderA = new CsvReaderA(text, false);
					csvReaderA.VarCon = Mp.M2D.curMap.getEventContainer().VarCon;
					while (M2EventContainer.readContainerCmd(this.Mp, csvReaderA, this.Ev, false, false))
					{
					}
				}
			}
			return true;
		}

		public override void closeAction(bool when_map_close)
		{
			M2EventContainer evCon = this.EvCon;
			if (evCon != null)
			{
				evCon.remReloadListener(this);
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

		public string target_script;

		public const string script_path = "evt/___m2d_cmd";
	}
}
