using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpActivator : NelLp, ILinerReceiver
	{
		public M2LpActivator(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			META meta = new META(this.comment);
			this.immediate = meta.GetB("immediate", false);
			this.load_layer = meta.GetS("load_layer");
			this.only_inner_chip = meta.GetB("only_inner_chip", false);
		}

		public void activateLiner(bool immediate)
		{
			this.setActivated(true, immediate);
		}

		public void deactivateLiner(bool immediate)
		{
			this.setActivated(false, immediate);
		}

		public override void activate()
		{
			this.setActivated(true, false);
		}

		public override void deactivate()
		{
			this.setActivated(false, false);
		}

		public void setActivated(bool value, bool _immediate = false)
		{
			if (this.activated_ == value)
			{
				return;
			}
			this.activated_ = value;
			if (!this.only_inner_chip)
			{
				int length = this.Lay.LP.Length;
				for (int i = 0; i < length; i++)
				{
					ILinerReceiver linerReceiver = this.Lay.LP.Get(i) as ILinerReceiver;
					if (linerReceiver != null && !(linerReceiver is M2LpActivator))
					{
						if (value)
						{
							linerReceiver.activateLiner(this.immediate);
						}
						else
						{
							linerReceiver.deactivateLiner(this.immediate);
						}
					}
				}
			}
			else
			{
				List<M2Puts> Aact = new List<M2Puts>(4);
				this.Lay.Mp.getAllPointMetaPutsTo(this.mapx, this.mapy, this.mapw, this.maph, null, delegate(M2Puts Cp, List<M2Puts> APt)
				{
					if (Cp.Lay != this.Lay)
					{
						return false;
					}
					if (!Cp.active_closed && Aact.IndexOf(Cp) == -1)
					{
						Aact.Add(Cp);
						if (Cp is IActivatable)
						{
							if (value)
							{
								(Cp as IActivatable).activate();
							}
							else
							{
								(Cp as IActivatable).deactivate();
							}
						}
						else if (value)
						{
							Cp.activateToDrawer();
						}
						else
						{
							Cp.deactivateToDrawer();
						}
					}
					return false;
				});
			}
			if (value && TX.valid(this.load_layer))
			{
				M2MapLayer layer = this.Mp.getLayer(this.load_layer);
				if (layer != null)
				{
					layer.loadLayerFromEvent();
				}
			}
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			if (RcEffect != null)
			{
				RcEffect.Set((float)this.mapx, (float)this.mapy, (float)this.mapw, (float)this.maph);
			}
			return true;
		}

		private bool activated_;

		private bool immediate;

		private bool only_inner_chip;

		private string load_layer;
	}
}
