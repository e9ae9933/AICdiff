using System;
using System.Collections.Generic;
using Better;

namespace XX
{
	public static class ListBuffer<T>
	{
		private static void Init()
		{
			if (ListBuffer<T>.AItems == null)
			{
				ListBuffer<T>.AItems = new List<BList<T>>(1);
			}
		}

		public static void Stock(int cnt = 1, int capacity = 0)
		{
			ListBuffer<T>.Init();
			while (--cnt >= 0)
			{
				ListBuffer<T>.AItems.Add(new BList<T>(capacity));
			}
		}

		public static BList<T> Pop(List<T> Src)
		{
			BList<T> blist = ListBuffer<T>.Pop((Src != null) ? Src.Count : 0);
			if (Src != null)
			{
				blist.AddRange(Src);
			}
			return blist;
		}

		public static BList<T> Pop(T[] Src)
		{
			BList<T> blist = ListBuffer<T>.Pop((Src != null) ? Src.Length : 0);
			if (Src != null)
			{
				blist.AddRange(Src);
			}
			return blist;
		}

		public static BList<T> Pop(int capacity = 0)
		{
			ListBuffer<T>.Init();
			while (ListBuffer<T>.AItems.Count <= ListBuffer<T>.using_cnt)
			{
				ListBuffer<T>.Stock(X.Mx(ListBuffer<T>.using_cnt - ListBuffer<T>.AItems.Count + 1, 1), capacity);
			}
			List<BList<T>> aitems = ListBuffer<T>.AItems;
			int num = ListBuffer<T>.using_cnt;
			ListBuffer<T>.using_cnt = num + 1;
			BList<T> blist = aitems[num];
			blist.Clear();
			if (blist.Capacity < capacity)
			{
				blist.Capacity = capacity;
			}
			return blist;
		}

		public static BList<T> Release(BList<T> Target)
		{
			if (Target == null || ListBuffer<T>.AItems == null)
			{
				return null;
			}
			Target.Clear();
			int num = ListBuffer<T>.AItems.IndexOf(Target);
			if (num >= 0 && num < ListBuffer<T>.using_cnt)
			{
				ListBuffer<T>.AItems.RemoveAt(num);
				ListBuffer<T>.AItems.Insert(--ListBuffer<T>.using_cnt, Target);
			}
			return null;
		}

		public static void Release<T2>(BDic<T2, BList<T>> ODict)
		{
			foreach (KeyValuePair<T2, BList<T>> keyValuePair in ODict)
			{
				ListBuffer<T>.Release(keyValuePair.Value);
			}
			ODict.Clear();
		}

		private static int using_cnt;

		private static List<BList<T>> AItems;
	}
}
