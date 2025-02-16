using System;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpWaterSlicer : NelLp, ILinerReceiver, IPuzzRevertable
	{
		public M2LpWaterSlicer(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.Meta = new META(this.comment);
			this.activated = (this.pre_on = this.Meta.GetB("pre_on", false));
			if (base.nM2D.MIST != null)
			{
				base.nM2D.MIST.assignWaterSlicer(this);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (base.nM2D.MIST != null)
			{
				base.nM2D.MIST.deassignWaterSlicer(this);
			}
			this.activated_ = false;
		}

		public void activateLiner(bool immediate)
		{
			this.activated = !this.pre_on;
		}

		public void deactivateLiner(bool immediate)
		{
			this.activated = this.pre_on;
		}

		public bool activated
		{
			get
			{
				return this.activated_;
			}
			set
			{
				if (this.activated == value)
				{
					return;
				}
				this.activated_ = value;
				if (base.nM2D.MIST != null)
				{
					base.nM2D.MIST.fine_water_slicer = true;
				}
				if (this.Mp.floort >= 10f)
				{
					this.Mp.PtcST("water_slicer_activated", null, PTCThread.StFollow.NO_FOLLOW);
				}
			}
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			return false;
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			Rvi.time = (this.activated_ ? 1 : 0);
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			this.activated = Rvi.time != 0;
		}

		private META Meta;

		private bool activated_;

		private bool pre_on;
	}
}
