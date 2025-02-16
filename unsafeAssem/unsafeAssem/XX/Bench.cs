using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public static class Bench
	{
		public static void mark(string _stream_key = null, bool checking_others = false, bool use_profiler = false)
		{
		}

		public static int getTime()
		{
			return (int)(Time.realtimeSinceStartup * 1000f);
		}

		public static void clearClosure()
		{
			Bench.Aclosure.Clear();
			Bench.p_depth = 0;
		}

		public static string P(string s)
		{
			s = (TX.valid(s) ? s : "()");
			Bench.Aclosure.Add(s);
			Bench.p_depth++;
			return s;
		}

		private static void P()
		{
			if (Bench.p_depth > 0)
			{
				Bench.p_depth--;
			}
			if (Bench.Aclosure.Count > 0)
			{
				Bench.Aclosure.RemoveAt(Bench.Aclosure.Count - 1);
			}
		}

		public static void Pend(string s)
		{
			if (Bench.p_depth > 0)
			{
				Bench.p_depth--;
			}
			if (Bench.Aclosure.Count > 0)
			{
				Bench.Aclosure.RemoveAt(Bench.Aclosure.Count - 1);
			}
		}

		public static void P_allClose()
		{
			Bench.clearClosure();
		}

		public static STB CopyCurrentClosure(STB Dest, string prefix)
		{
			Dest += prefix;
			int count = Bench.Aclosure.Count;
			for (int i = 0; i < count; i++)
			{
				Dest += Bench.Aclosure[i];
				if (i == count - 1)
				{
					break;
				}
				Dest += " => ";
			}
			return Dest;
		}

		public static string now_stream_key;

		private const string def_profile = "()";

		private static List<string> Aclosure = new List<string>(16);

		private static int p_depth;
	}
}
