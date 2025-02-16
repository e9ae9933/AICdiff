using System;
using System.Collections.Generic;
using XX;

namespace evt
{
	public class EvMsgCommand
	{
		public EvMsgCommand()
		{
			this.ACmd = new List<string[]>();
			this.ACmd.Add(null);
			this.Atime = new List<int>();
			this.Atime.Add(-1);
			this.BfReader = new StringHolder(CsvReader.RegSpace);
		}

		public void clear()
		{
			this.ACmd.RemoveRange(1, this.ACmd.Count - 1);
			this.Atime.RemoveRange(1, this.Atime.Count - 1);
			this.Atime[0] = -1;
			this.ACmd[0] = null;
			this.active = false;
			this.num_cnt = 0;
		}

		public void set_execute_time(int kosu, int t)
		{
			if (this.num_cnt < 0 || this.Atime.Count < 1)
			{
				return;
			}
			if (kosu == 0)
			{
				this.Atime[0] = t;
				this.num_cnt = -1;
				return;
			}
			while (--kosu >= 0)
			{
				int num = this.num_cnt + 1;
				this.num_cnt = num;
				if (num >= this.Atime.Count)
				{
					return;
				}
				this.Atime[this.num_cnt] = t;
			}
		}

		public void time_shift(int t)
		{
			int count = this.Atime.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.Atime[i] > 0)
				{
					this.Atime[i] = X.Mx(1, this.Atime[i] + t);
				}
			}
		}

		public void run()
		{
			if (!this.active)
			{
				return;
			}
			int count = this.Atime.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.Atime[i] > 0)
				{
					this.Atime[i] = this.Atime[i] - 1;
					if (this.Atime[i] == 0)
					{
						this.execute(i);
						if (i == 0)
						{
							return;
						}
					}
				}
			}
		}

		public void add(string[] Astr)
		{
			this.active = true;
			this.ACmd.Add(Astr);
			this.Atime.Add(-1);
		}

		public void executeAll()
		{
			if (this.active)
			{
				this.execute(0);
			}
		}

		public bool execute(int i)
		{
			if (!this.active)
			{
				return false;
			}
			if (i <= 0)
			{
				int count = this.ACmd.Count;
				for (i = 1; i < count; i++)
				{
					this.execute(i);
				}
				this.clear();
				return true;
			}
			if (i < this.ACmd.Count)
			{
				bool flag = this.BfReader.ArrayInput(this.ACmd[i], true);
				this.ACmd[i] = null;
				this.Atime[i] = -1;
				if (flag)
				{
					EV.preserveEventExecuted(EV.readOneLine(null, this.BfReader));
					return true;
				}
			}
			return false;
		}

		public void executeProgress(int i)
		{
			if (!this.active)
			{
				return;
			}
			if (i <= 0)
			{
				this.execute(i);
				return;
			}
			int count = this.ACmd.Count;
			int num = 1;
			while (num < count && i > 0)
			{
				if (this.execute(num))
				{
					i--;
				}
				num++;
			}
		}

		public bool isActive()
		{
			return this.active;
		}

		private List<string[]> ACmd;

		private List<int> Atime;

		private StringHolder BfReader;

		private bool active;

		private int num_cnt;
	}
}
