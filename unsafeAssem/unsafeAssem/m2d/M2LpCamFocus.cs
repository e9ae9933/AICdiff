using System;
using XX;

namespace m2d
{
	public class M2LpCamFocus : M2LpNearCheck
	{
		public M2LpCamFocus(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			META meta = new META(this.comment);
			this.alloc_in_ev = true;
			this.T_FADE = -1f;
			this.T_RECHECK = 10f;
			this.focus_level_x = meta.GetNm("level", 1f, 0);
			this.focus_level_y = meta.GetNm("level", this.focus_level_x, 1);
			this.focus_shift_x = meta.GetNm("shift", 0f, 0);
			this.focus_shift_y = meta.GetNm("shift", 0f, 1);
			this.only_foot = meta.GetI("only_foot", 0, 0);
			this.priority = meta.GetI("priority", 0, 0);
			base.initAction(normal_map);
		}

		public override void closeAction(bool when_map_close = false)
		{
			base.closeAction(when_map_close);
			if (!when_map_close)
			{
				return;
			}
			this.active = true;
		}

		public override void deactivate()
		{
			this.active = false;
		}

		public override void activate()
		{
			this.active = true;
		}

		public override void initEnter(M2Mover Mv)
		{
			if (!base.activated)
			{
				this.Mp.M2D.Cam.addFocusArea(this);
			}
			base.initEnter(Mv);
		}

		public override void quitEnter(M2Mover Mv)
		{
			if (base.activated)
			{
				this.Mp.M2D.Cam.remFocusArea(this);
			}
			base.quitEnter(Mv);
		}

		public float focus_level_x = 1f;

		public float focus_level_y = 1f;

		public float focus_shift_x;

		public float focus_shift_y;

		public int only_foot;

		public int priority;
	}
}
