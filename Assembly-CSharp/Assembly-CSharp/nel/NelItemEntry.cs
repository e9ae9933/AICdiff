using System;
using System.Collections.Generic;
using XX;

namespace nel
{
	public class NelItemEntry
	{
		public NelItemEntry(NelItem _Data, int _count, byte _grade)
		{
			this.Data = _Data;
			this.count = _count;
			this.grade = _grade;
			if (this.count < 0)
			{
				this.count = this.Data.stock;
			}
		}

		public NelItemEntry(NelItemEntry E)
			: this(E.Data, E.count, E.grade)
		{
		}

		public NelItemEntry(string key)
		{
			this.Data = NelItem.GetById(key, false);
		}

		public bool isSame(NelItemEntry E)
		{
			return E != null && E.Data == this.Data && this.grade == E.grade;
		}

		public string getLocalizedName(int appear_grade_icon = 0, int appear_count = 2, bool grade_s = false)
		{
			return NelItemEntry.getLocalizedNameS(this.Data, this.count, (int)this.grade, appear_grade_icon, appear_count, grade_s);
		}

		public static string getLocalizedNameS(NelItem Data, int count, int grade, int appear_grade_icon = 0, int appear_count = 2, bool grade_s = false)
		{
			string text = Data.getLocalizedName(grade, null);
			if (appear_grade_icon >= 0 && grade >= appear_grade_icon)
			{
				text = string.Concat(new string[]
				{
					text,
					"<img mesh=\"nel_item_grade",
					grade_s ? "_s" : "",
					".",
					(5 + grade).ToString(),
					"\" width=\"38\" tx_color/>"
				});
			}
			if (appear_count >= 0 && count >= appear_count)
			{
				text = text + "x" + count.ToString();
			}
			return text;
		}

		public static List<NelItemEntry> Clone(List<ItemStorage.IRow> AIR)
		{
			List<NelItemEntry> list = new List<NelItemEntry>();
			int num = AIR.Count;
			using (BList<int> blist = ListBuffer<int>.Pop(0))
			{
				for (int i = 0; i < num; i++)
				{
					ItemStorage.IRow row = AIR[i];
					if (blist.IndexOf((int)row.Data.id) < 0)
					{
						blist.Add((int)row.Data.id);
						int num2 = 5;
						for (int j = 0; j < num2; j++)
						{
							int num3 = row.Info.getCount(j);
							if (num3 >= 0)
							{
								list.Add(new NelItemEntry(row.Data, num3, (byte)j));
							}
						}
					}
				}
			}
			return list;
		}

		public NelItem Data;

		public int count = 1;

		public byte grade;
	}
}
