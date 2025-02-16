using System;
using System.Collections.Generic;
using XX;

namespace evt
{
	public class EvTLCommand
	{
		public EvTLCommand()
		{
			this.ACmd = new List<string[]>(0);
			this.Atime = new List<float>(0);
			this.BfReader = new StringHolder(CsvReader.RegSpace);
		}

		public void clear()
		{
			this.ACmd.Clear();
			this.Atime.Clear();
			this.active = false;
		}

		public void run(float fcnt)
		{
			if (!this.active)
			{
				return;
			}
			int num = this.Atime.Count;
			int i = 0;
			while (i < num)
			{
				if (this.Atime[i] <= 0f)
				{
					this.ACmd[i] = null;
				}
				List<float> atime = this.Atime;
				int num2 = i;
				atime[num2] -= fcnt;
				if (this.Atime[i] <= 0f)
				{
					this.execute(i);
					num--;
				}
				else
				{
					i++;
				}
			}
		}

		public void add(string[] Aadd, int t)
		{
			this.active = true;
			t = X.Mx(1, t);
			int count = this.ACmd.Count;
			int num = 0;
			while (num < count && this.Atime[num] <= (float)t)
			{
				num++;
			}
			this.ACmd.Insert(num, Aadd);
			this.Atime.Insert(num, (float)t);
		}

		public void executeAll()
		{
			if (this.active)
			{
				this.execute(-1);
			}
		}

		public void execute(int i)
		{
			if (!this.active)
			{
				return;
			}
			int count = this.ACmd.Count;
			if (i < 0)
			{
				for (i = 0; i < count; i++)
				{
					this.execute(0);
				}
				this.clear();
				return;
			}
			if (i < count)
			{
				bool flag = this.BfReader.ArrayInput(this.ACmd[i], true);
				this.ACmd.RemoveAt(i);
				this.Atime.RemoveAt(i);
				this.active = count >= 2;
				if (flag)
				{
					EV.preserveEventExecuted(EV.readOneLine(null, this.BfReader));
				}
			}
		}

		private List<string[]> ACmd;

		private List<float> Atime;

		private readonly StringHolder BfReader;

		private bool active;
	}
}
