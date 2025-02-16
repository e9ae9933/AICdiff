using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpTimer : NelLp, IPuzzRevertable, IRunAndDestroy, IPuzzActivationListener, ILinerReceiver
	{
		public M2LpTimer(string _key, int _index, M2MapLayer _Lay)
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
			string[] array = this.Meta.Get("switch");
			if (array != null)
			{
				this.switch_id_bits = 0;
				for (int i = array.Length - 1; i >= 0; i--)
				{
					this.switch_id_bits |= 1 << X.NmI(array[i], 0, false, false);
				}
			}
			else
			{
				this.switch_id_bits = 1;
			}
			this.next = 0f;
			this.count = (this.first_count = this.Meta.GetI("count", -1, 0));
			this.maxt = this.Meta.GetNm("time", 0f, 0);
			this.pre_on = this.Meta.GetB("pre_on", false);
			this.immediate = this.Meta.GetB("immediate", false);
			if (this.Meta.GetB("receiving", false))
			{
				this.first_count = this.count;
			}
			else
			{
				this.first_count = 0;
			}
			this.ATimer = new List<M2CImgDrawerTimer>(1);
			this.Lay.getAreaCastDrawer<M2CImgDrawerTimer>(this.mapx, this.mapy, this.mapw, this.maph, this.ATimer);
			this.ManageArea = PUZ.IT.isBelongTo(this);
			if (!this.receiving_mode && this.ManageArea == null)
			{
				this.runner_assigned = true;
				this.next = -1f;
			}
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					if (this.next == 0f)
					{
						this.next = -1f;
					}
					if (this.receiving_mode)
					{
						this.count = this.first_count;
					}
					this.Mp.addRunnerObject(this);
					return;
				}
				this.next = 0f;
				this.Mp.remRunnerObject(this);
			}
		}

		public void changePuzzleActivation(bool activated)
		{
			if (!this.receiving_mode)
			{
				this.runner_assigned = activated;
				if (activated)
				{
					this.next = -1f;
				}
			}
		}

		public bool run(float fcnt)
		{
			if (this.next == 0f)
			{
				return true;
			}
			if (this.next < 0f)
			{
				if (this.count == 0)
				{
					return true;
				}
				this.next = this.Mp.floort + this.maxt;
				if (this.pre_on)
				{
					this.activated = true;
				}
				for (int i = this.ATimer.Count - 1; i >= 0; i--)
				{
					this.ATimer[i].activate(this.maxt);
				}
			}
			if (this.Mp.floort >= this.next)
			{
				this.activated = !this.activated_;
				if (this.count > 0)
				{
					int num = this.count - 1;
					this.count = num;
					if (num == 0)
					{
						this.next = 0f;
						if (this.receiving_mode)
						{
							this.runner_assigned = false;
							return true;
						}
						return true;
					}
				}
				this.next = this.Mp.floort + this.maxt;
				for (int j = this.ATimer.Count - 1; j >= 0; j--)
				{
					this.ATimer[j].activate(this.maxt);
				}
			}
			return true;
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
				int num = 5;
				for (int i = 0; i < num; i++)
				{
					if ((this.switch_id_bits & (1 << i)) != 0)
					{
						if (value)
						{
							PuzzLiner.activateConnector(i, this.immediate || this.Mp.floort < 10f);
						}
						else
						{
							PuzzLiner.deactivateConnector(i, this.immediate || this.Mp.floort < 10f, false);
						}
					}
				}
			}
		}

		public void activateLiner(bool immediate)
		{
			if (this.receiving_mode)
			{
				this.runner_assigned = true;
			}
		}

		public void deactivateLiner(bool immediate)
		{
			if (this.receiving_mode)
			{
				this.runner_assigned = false;
			}
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			return true;
		}

		public bool receiving_mode
		{
			get
			{
				return this.first_count != 0;
			}
		}

		public void destruct()
		{
			this.changePuzzleActivation(false);
		}

		public bool isActive()
		{
			return this.runner_assigned;
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			Rvi.x = this.next;
			Rvi.y = (float)(this.activated ? 1 : 0);
			Rvi.time = this.count;
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			this.next = (this.runner_assigned_ ? Rvi.x : 0f);
			this.activated = Rvi.y != 0f;
			this.count = Rvi.time;
		}

		private int switch_id_bits;

		private int count;

		private int first_count;

		private float next;

		private float maxt;

		private bool immediate;

		private META Meta;

		private List<M2CImgDrawerTimer> ATimer;

		private M2LpPuzzManageArea ManageArea;

		private bool runner_assigned_;

		private bool activated_;

		private bool pre_on;
	}
}
