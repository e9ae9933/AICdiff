using System;
using m2d;

namespace nel
{
	public class NelChipPuzzleSwitch : NelChip, IActivatable
	{
		public NelChipPuzzleSwitch(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null, string meta_switch_id_name = "puzzle_switch")
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			this.puzzle_id = base.Meta.GetI(meta_switch_id_name, 0, 0);
			base.arrangeable = true;
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.MvHit != null)
			{
				this.Mp.removeMover(this.MvHit);
				this.MvHit = null;
			}
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			PuzzLiner.Assign(this);
			bool flag = false;
			if (this.MvHit != null)
			{
				flag = this.MvHit.isActive();
				this.Mp.removeMover(this.MvHit);
				this.MvHit = null;
			}
			M2LabelPointSwitchContainer m2LabelPointSwitchContainer = this.Mp.getLabelPoint((M2LabelPoint V) => V.isConveringMapXy((float)this.mapx, (float)this.mapy, (float)(this.mapx + this.clms), (float)(this.mapy + this.rows)) && V is M2LabelPointSwitchContainer) as M2LabelPointSwitchContainer;
			this.MvHit = this.Mp.createMover<M2PuzzleSwitch>(base.unique_key + "-mv", this.mapcx, this.mapcy, false, true);
			this.MvHit.initPuzzleSwitch(this.Mp.M2D, this, this.puzzle_id, m2LabelPointSwitchContainer);
			if (flag)
			{
				this.MvHit.activate(true, false);
			}
		}

		public virtual void fineActivation()
		{
		}

		public virtual void activate()
		{
			if (this.MvHit != null)
			{
				this.MvHit.activate();
			}
		}

		public virtual void deactivate()
		{
			if (this.MvHit != null)
			{
				this.MvHit.deactivate();
			}
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		public M2PuzzleSwitch getMover()
		{
			return this.MvHit;
		}

		public virtual bool can_hit()
		{
			return true;
		}

		protected M2PuzzleSwitch MvHit;

		public int puzzle_id;
	}
}
