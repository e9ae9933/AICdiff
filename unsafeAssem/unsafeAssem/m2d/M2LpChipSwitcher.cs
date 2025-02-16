using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public class M2LpChipSwitcher : M2LabelPoint
	{
		public M2LpChipSwitcher(string __key, int _i, M2MapLayer L)
			: base(__key, _i, L)
		{
		}

		public override void initActionPre()
		{
			this.ATarget = new List<M2Puts>(4);
			int count_chips = this.Lay.count_chips;
			for (int i = 0; i < count_chips; i++)
			{
				M2Puts chipByIndex = this.Lay.getChipByIndex(i);
				if (chipByIndex.isOnCamera(this.x, base.y, base.w, base.h))
				{
					this.ATarget.Add(chipByIndex);
					chipByIndex.arrangeable = true;
				}
			}
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			META meta = new META(this.comment);
			this.is_active = meta.GetIE("pre_on", 1, 0) != 0;
			if (!this.is_active)
			{
				this.fineVisible();
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (!this.is_active)
			{
				this.is_active = true;
				this.fineVisible();
			}
		}

		private void fineVisible()
		{
			if (this.ATarget == null)
			{
				return;
			}
			for (int i = this.ATarget.Count - 1; i >= 0; i--)
			{
				M2Puts m2Puts = this.ATarget[i];
				if (!this.is_active)
				{
					m2Puts.addActiveRemoveKey(this.ToString(), false);
				}
				else
				{
					m2Puts.remActiveRemoveKey(this.ToString(), false);
				}
			}
		}

		public override void activate()
		{
			if (!this.is_active)
			{
				this.is_active = true;
				this.fineVisible();
			}
		}

		public override void deactivate()
		{
			if (this.is_active)
			{
				this.is_active = false;
				this.fineVisible();
			}
		}

		private List<M2Puts> ATarget;

		private bool is_active;
	}
}
