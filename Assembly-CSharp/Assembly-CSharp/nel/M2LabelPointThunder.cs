using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel
{
	public class M2LabelPointThunder : M2LabelPoint, IPuzzSwitchListener
	{
		public virtual string main_tag_name
		{
			get
			{
				return "thunder_shooter";
			}
		}

		public M2LabelPointThunder(string _key, int _index, M2MapLayer _Lay, NelChipThunderShooter _Created = null)
			: base(_key, _index, _Lay)
		{
			this.after_created = _Created != null;
			if (_Created != null)
			{
				this.ACp = new List<NelChipThunderShooter>(1);
				_Created.assignBoundsLp(this, false, false);
			}
		}

		public override void initActionPre()
		{
			this.hitlock_maxt = 95f;
			if (this.ACp == null)
			{
				META meta = new META(this.comment);
				this.hitlock_maxt = (float)meta.GetI("hitlock", 95, 0);
				this.hit_aim = meta.getDirsI("aim", 0, false, 0, -1);
				List<M2Puts> allPointMetaPutsTo = this.Mp.getAllPointMetaPutsTo(this.mapx - 1, this.mapy - 1, this.mapw + 2, this.maph + 2, null, this.main_tag_name);
				if (allPointMetaPutsTo != null)
				{
					for (int i = allPointMetaPutsTo.Count - 1; i >= 0; i--)
					{
						NelChipThunderShooter nelChipThunderShooter = allPointMetaPutsTo[i] as NelChipThunderShooter;
						if (nelChipThunderShooter != null && nelChipThunderShooter.Lay == this.Lay)
						{
							nelChipThunderShooter.assignBoundsLp(this, true, true);
						}
					}
				}
			}
		}

		public void assignChip(NelChipThunderShooter Cp)
		{
			if (this.ACp == null)
			{
				this.ACp = new List<NelChipThunderShooter>(1);
			}
			if (this.ACp.IndexOf(Cp) == -1)
			{
				this.ACp.Add(Cp);
			}
		}

		public void fineSize(DRect Rc, int aim)
		{
			if (CAim._XD(aim, 1) != 0)
			{
				base.Set(Rc.x * base.CLEN, (float)((int)Rc.y) * base.CLEN, Rc.width * base.CLEN, (float)X.Mx(1, X.IntC(Rc.bottom) - (int)Rc.y) * base.CLEN);
			}
			else
			{
				base.Set((float)((int)Rc.x) * base.CLEN, Rc.y * base.CLEN, (float)X.Mx(1, X.IntC(Rc.right) - (int)Rc.x) * base.CLEN, Rc.height * base.CLEN);
			}
			this.finePos();
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.OHitLock = null;
			string areasnd_que = this.areasnd_que;
			if (areasnd_que != null)
			{
				this.Mp.M2D.Snd.Environment.RemLoop(areasnd_que, base.unique_key);
			}
			this.ACp = null;
		}

		private string areasnd_que
		{
			get
			{
				if (this.ACp == null || this.ACp.Count == 0)
				{
					return null;
				}
				return this.ACp[0].areasnd_cue;
			}
		}

		private bool snd_active
		{
			get
			{
				return this.SndItem != null;
			}
			set
			{
				if (value == this.snd_active)
				{
					return;
				}
				string areasnd_que = this.areasnd_que;
				if (areasnd_que == null)
				{
					this.SndItem = null;
					return;
				}
				if (value)
				{
					this.SndItem = this.Mp.M2D.Snd.Environment.AddLoop(areasnd_que, base.unique_key, base.mapcx, base.mapcy, 6f, 6f, (float)this.mapw * 0.5f + 6f, (float)this.maph * 0.5f + 6f, null);
					return;
				}
				this.Mp.M2D.Snd.Environment.RemLoop(areasnd_que, base.unique_key);
				this.SndItem = null;
			}
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.after_created)
			{
				this.pre_on = true;
				return;
			}
			META meta = new META(this.comment);
			this.pre_on = meta.GetI("pre_on", 1, 0) != 0;
			this.activate_switch_id = meta.GetI("activate_switch_id", -1, 0);
		}

		public void fineEnable()
		{
			if (this.ACp == null)
			{
				return;
			}
			bool flag = (this.snd_active = this.liner_enabled != this.pre_on);
			for (int i = this.ACp.Count - 1; i >= 0; i--)
			{
				this.ACp[i].SetEnable(flag);
			}
		}

		public void changePuzzleSwitchActivation(int id, bool activating)
		{
			if (id == this.activate_switch_id)
			{
				this.liner_enabled = activating;
				this.fineEnable();
			}
		}

		public void linerActivating(bool activating)
		{
			this.liner_enabled = activating;
			this.fineEnable();
		}

		public readonly bool after_created;

		private bool liner_enabled;

		public bool pre_on = true;

		public int hit_aim = -1;

		public BDic<int, float> OHitLock;

		private List<NelChipThunderShooter> ACp;

		private M2SndLoopItem SndItem;

		public float hitlock_maxt = 95f;

		public int activate_switch_id = -1;

		private readonly DRect PreRect = new DRect("Pre");
	}
}
