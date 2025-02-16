using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class M2LpDrawBridgeContainer : NelLp, IRunAndDestroy
	{
		public M2LpDrawBridgeContainer(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.Root = null;
			List<M2Puts> list = null;
			list = this.Mp.getAllPointMetaPutsTo(this.mapx, this.mapy, this.mapw, this.maph, list, "drawbridge", "drawbridge_piece");
			META meta = new META(this.comment);
			this.activate_flag = meta.GetBE("pre_on", false);
			if (list != null)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					M2Puts m2Puts = list[i];
					if (m2Puts is NelChipDrawBridge)
					{
						list.RemoveAt(i);
						this.Root = m2Puts as NelChipDrawBridge;
					}
					else
					{
						m2Puts.arrangeable = true;
					}
				}
			}
			if (this.Root != null && list != null && list.Count > 0)
			{
				this.Root.initCord(this, list);
				return;
			}
			this.Root = null;
		}

		public bool run(float fcnt)
		{
			return this.Root != null && this.Root.redrawBridge();
		}

		public void destruct()
		{
		}

		public override void activate()
		{
			base.activate();
			if (!this.activate_flag)
			{
				this.activate_flag = true;
				if (this.Root != null)
				{
					this.Mp.addRunnerObject(this);
				}
			}
		}

		public override void deactivate()
		{
			base.activate();
			if (!this.activate_flag)
			{
				this.activate_flag = true;
				if (this.Root != null)
				{
					this.Mp.addRunnerObject(this);
					this.Root.setChipConfigActivation(false);
				}
			}
		}

		public NelChipDrawBridge Root;

		public bool activate_flag;
	}
}
