using System;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class UiBoxDesignerFamilyEvent : UiBoxDesignerFamily, IEventListener
	{
		public virtual bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			return false;
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end)
			{
				this.deactivate(false);
			}
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				if (!this.active && this.auto_deactive_gameobject)
				{
					Object.Destroy(this);
				}
				return false;
			}
			return true;
		}

		protected override bool runner_assigned
		{
			set
			{
				if (this.runner_assigned_ == value)
				{
					return;
				}
				base.runner_assigned = value;
				if (value)
				{
					EV.addListener(this);
					return;
				}
				EV.remListener(this);
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			base.DestroyDesigners();
		}
	}
}
