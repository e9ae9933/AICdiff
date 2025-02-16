using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class M2PlayerStateHistory
	{
		public M2PlayerStateHistory(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.AHis = new List<M2PlayerStateHistory.PHis>(4);
		}

		public int getHisIndex(string _name)
		{
			int count = this.AHis.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.AHis[i].name == _name)
				{
					return i;
				}
			}
			return -1;
		}

		public void push(string _name, bool temporary = false)
		{
			if (TX.valid(_name))
			{
				int hisIndex = this.getHisIndex(_name);
				if (hisIndex >= 0)
				{
					this.AHis.RemoveRange(hisIndex, this.AHis.Count - hisIndex);
				}
			}
			this.AHis.Add(new M2PlayerStateHistory.PHis(_name, this.M2D.curMap, temporary));
		}

		public void pop(string _name = "")
		{
			int num = this.AHis.Count - 1;
			if (TX.valid(_name))
			{
				int hisIndex = this.getHisIndex(_name);
				if (hisIndex >= 0 && hisIndex < this.AHis.Count - 1)
				{
					num = hisIndex;
					this.AHis.RemoveRange(num + 1, this.AHis.Count - (num + 1));
				}
			}
			if (num < this.AHis.Count && num >= 0)
			{
				this.AHis[num].Revert(this.M2D.curMap);
				this.AHis.RemoveRange(num, this.AHis.Count - num);
			}
		}

		public void initS()
		{
			for (int i = this.AHis.Count - 1; i >= 0; i--)
			{
				if (this.AHis[i].temporary)
				{
					this.AHis.RemoveAt(i);
				}
			}
		}

		public const string KEY_PUZZLE = "puzzle_lp";

		public readonly NelM2DBase M2D;

		private List<M2PlayerStateHistory.PHis> AHis;

		private sealed class PHis
		{
			public PHis(string _name, Map2d Mp, bool _temporary = false)
			{
				this.APrHis = new List<M2PlayerStateHistory.PHis.PrHis>(2);
				this.name = _name;
				this.temporary = _temporary;
				int count_players = Mp.count_players;
				for (int i = 0; i < count_players; i++)
				{
					PR pr = Mp.getPr(i) as PR;
					if (!(pr == null) && !pr.initMemorize(this.name))
					{
						this.APrHis.Add(new M2PlayerStateHistory.PHis.PrHis(pr));
					}
				}
			}

			public void Revert(Map2d Mp)
			{
				int count_players = Mp.count_players;
				for (int i = 0; i < count_players; i++)
				{
					PR pr = Mp.getPr(i) as PR;
					for (int j = this.APrHis.Count - 1; j >= 0; j--)
					{
						M2PlayerStateHistory.PHis.PrHis prHis = this.APrHis[j];
						if (prHis.Pr == pr)
						{
							prHis.Revert(pr);
							break;
						}
					}
				}
			}

			private List<M2PlayerStateHistory.PHis.PrHis> APrHis;

			public string name;

			public bool temporary;

			private sealed class PrHis
			{
				public PrHis(PR _Pr)
				{
					this.Pr = _Pr;
					this.hp = this.Pr.get_hp();
					this.maxhp = this.Pr.get_maxhp();
					this.mp = this.Pr.get_mp();
					this.maxmp = this.Pr.get_maxmp();
				}

				public void Revert(PR Pr)
				{
				}

				public readonly PR Pr;

				private float hp;

				private float maxhp;

				private float mp;

				private float maxmp;
			}
		}
	}
}
