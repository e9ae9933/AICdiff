using System;
using evt;
using XX;

namespace m2d
{
	public class M2EventSetterLP : IListenerEvcReload
	{
		public M2EventSetterLP(M2LabelPoint _LP)
		{
			this.LP = _LP;
			this.LP.Mp.getEventContainer().addReloadListener(this);
		}

		public Map2d Mp
		{
			get
			{
				return this.LP.Mp;
			}
		}

		public M2EventContainer EVC
		{
			get
			{
				return this.LP.Mp.getEventContainer();
			}
		}

		public void closeEvent()
		{
			if (this.Ev != null)
			{
				this.Mp.destructEvent(this.Ev);
				this.LP.Mp.getEventContainer().remReloadListener(this);
				this.Ev = null;
			}
		}

		public bool EvtM2Reload(Map2d Mp)
		{
			this.setEvent();
			return true;
		}

		public M2EventItem setEvent(Func<M2LabelPoint, M2EventItem> FnCreateLp, string addition)
		{
			this.addition_command = addition;
			this.FnCreateLp = FnCreateLp;
			return this.setEvent();
		}

		public M2EventItem setEvent()
		{
			if (this.FnCreateLp == null)
			{
				this.Ev = this.EVC.CreateAndAssign("_LP_" + this.LP.unique_key);
			}
			else
			{
				this.Ev = this.FnCreateLp(this.LP);
			}
			this.Ev.clear();
			this.Ev.setToArea(this.LP);
			CsvReaderA csvReaderA = new CsvReaderA(TX.add(this.LP.command ?? "", this.addition_command, "\n"), EV.getVariableContainer());
			while (csvReaderA.read())
			{
				M2EventContainer.readContainerCmd(this.Mp, csvReaderA, this.Ev, false, true);
			}
			return this.Ev;
		}

		public readonly M2LabelPoint LP;

		private M2EventItem Ev;

		public Func<M2LabelPoint, M2EventItem> FnCreateLp;

		public string addition_command;
	}
}
