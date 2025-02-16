using System;
using m2d;
using XX;

namespace nel
{
	public class M2LpPuzzManageArea : NelLpRunner, IPuzzRevertable, IPuzzActivationListener
	{
		public M2LpPuzzManageArea(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.auto_activate = true;
			string[] array = this.Meta.Get("auto_activate_if");
			this.auto_touch_barrier_lit = this.Meta.GetB("auto_touch_barrier_lit", false);
			this.box_manageable_len = this.Meta.GetI("box_manageable_len", 0, 0);
			this.no_manage_switch_bits = 0;
			this.clear_mist = this.Meta.GetB("clear_mist", false);
			this.cannot_walkout_when_active = this.Meta.GetBE("cannot_walkout_when_active", false);
			if (array != null)
			{
				this.auto_activate = TX.eval(TX.join<string>(" ", array, 0, -1), "") != 0.0;
			}
			array = this.Meta.Get("no_manage_switch");
			if (array != null)
			{
				for (int i = array.Length - 1; i >= 0; i--)
				{
					this.no_manage_switch_bits |= 1 << X.NmI(array[i], 0, false, false);
				}
			}
			if (this.auto_activate)
			{
				this.Mp.addRunnerObject(this);
			}
		}

		public override bool run(float fcnt)
		{
			if (!this.Mp.canHandle())
			{
				return true;
			}
			M2MoverPr keyPr = this.Mp.getKeyPr();
			if (keyPr == null)
			{
				return true;
			}
			if (this.t >= 0f)
			{
				if (!this.barrier_active)
				{
					base.nM2D.Puz.activateBarrier(this);
				}
				if (!base.isContainingMover(keyPr, 0.5f * base.CLEN))
				{
					this.t -= fcnt;
					if (this.t <= 0f)
					{
						this.t = 0f;
						this.deactivate();
					}
				}
				else
				{
					this.t = 30f;
				}
			}
			else if (base.isContainingMover(keyPr, 0f))
			{
				this.t += fcnt;
				if (this.t >= 0f || this.barrier_active || base.isContainingMover(keyPr, -0.4f * base.CLEN))
				{
					this.t = -1f;
					this.activate();
				}
			}
			else if (this.t > -30f)
			{
				this.t = X.Mx(-30f, this.t - fcnt);
			}
			return true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (normal_map)
			{
				this.magic_count = this.Meta.GetI("magic", -1, 0);
				if (this.MvKiller == null && this.Meta.GetI("killing_not_active", 1, 0) != 0)
				{
					this.MvKiller = this.Mp.createMover<M2MoverMagicKiller>("magic_killer_" + base.unique_key, base.mapcx, base.mapcy, true, false);
					this.MvKiller.setToArea(this);
				}
			}
			base.nM2D.Puz.initRevertableObject(this.Mp, this);
			this.first_activation_switch_bits = 0;
			string[] array = this.Meta.Get("first_switch");
			if (array != null)
			{
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					int num2 = X.NmI(array[i], -1, true, false);
					if (num2 >= 0)
					{
						this.first_activation_switch_bits |= 1 << num2;
					}
				}
			}
			PUZ.IT.addPaListener(this);
		}

		public void changePuzzleActivation(bool activated)
		{
			if (this.MvKiller != null)
			{
				this.MvKiller.magic_killable = !activated;
			}
			if (this.cannot_walkout_when_active)
			{
				this.Mp.considerConfig4(0, 0, this.Mp.clms, this.Mp.rows);
				this.Mp.need_update_collider = true;
			}
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			if (!base.nM2D.Puz.hasSnapShot())
			{
				Rvi.time = this.first_activation_switch_bits;
			}
			else
			{
				Rvi.time = 0;
			}
			for (int i = 0; i < 5; i++)
			{
				Rvi.time |= (PUZ.isActivePuzzleId(i) ? (1 << i) : 0);
			}
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvt)
		{
			int num = Rvt.time;
			if (this.MvKiller == null)
			{
				return;
			}
			if (!base.nM2D.Puz.hasSnapShot())
			{
				num &= ~this.first_activation_switch_bits;
			}
			bool flag = false;
			for (int i = 0; i < 5; i++)
			{
				if ((this.no_manage_switch_bits & (1 << i)) == 0)
				{
					bool flag2 = (num & (1 << i)) != 0;
					if (flag2 != PUZ.isActivePuzzleId(i))
					{
						if (flag2)
						{
							PuzzLiner.activateConnector(i, true);
						}
						else
						{
							flag = PuzzLiner.deactivateConnector(i, true, true) || flag;
						}
					}
				}
			}
			if (flag)
			{
				PuzzLiner.recheckLiner(-1, true);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (this.MvKiller != null)
			{
				this.Mp.removeMover(this.MvKiller);
				this.MvKiller = null;
			}
		}

		public override void considerConfig4(int _l, int _t, int _r, int _b, M2Pt[,] AAPt)
		{
			if (this.cannot_walkout_when_active && PUZ.IT.barrier_active)
			{
				int num = this.mapx + this.mapw;
				int num2 = this.mapy + this.maph;
				for (int i = _l; i < _r; i++)
				{
					bool flag = X.BTW((float)this.mapx, (float)i, (float)num);
					for (int j = _t; j < _b; j++)
					{
						if (!flag || !X.BTW((float)this.mapy, (float)j, (float)num2))
						{
							CCON.calcConfigManual(ref AAPt[i, j], 128);
						}
					}
				}
			}
		}

		public override void activate()
		{
			if (this.t >= 0f)
			{
				return;
			}
			this.t = 30f;
			base.nM2D.Puz.activateBarrier(this);
		}

		public bool isActivePuzzleArea()
		{
			return base.nM2D.Puz.isActivePuzzleArea(this);
		}

		public override void deactivate()
		{
			if (this.t < 0f)
			{
				return;
			}
			this.t = -30f;
			base.nM2D.Puz.deactivateBarrier(this, true);
		}

		public bool isManageableSwitchId(int i)
		{
			return (this.no_manage_switch_bits & (1 << i)) == 0;
		}

		public bool barrier_active
		{
			get
			{
				return base.nM2D.Puz.barrier_active;
			}
		}

		public const int T_PR_IN = 30;

		private float t = -30f;

		private M2MoverMagicKiller MvKiller;

		public int magic_count = -1;

		private int first_activation_switch_bits;

		private bool auto_activate = true;

		private bool cannot_walkout_when_active;

		private int no_manage_switch_bits;

		public bool auto_touch_barrier_lit;

		public bool clear_mist;

		public int box_manageable_len;
	}
}
