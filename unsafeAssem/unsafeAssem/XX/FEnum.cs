using System;
using Better;

namespace XX
{
	public static class FEnum<T> where T : struct
	{
		private static void Init()
		{
			if (FEnum<T>.Ostr != null)
			{
				return;
			}
			int num = 8;
			try
			{
				num = (int)Enum.Parse(typeof(T), "_MAX") + 1;
			}
			catch
			{
			}
			FEnum<T>.Init(num);
		}

		private static void Init(int capacity)
		{
			FEnum<T>.Ostr = new BDic<string, T>(capacity);
		}

		public static T Parse(string str, T def = default(T))
		{
			T t;
			if (!FEnum<T>.TryParse(str, out t, true))
			{
				return def;
			}
			return t;
		}

		public static bool TryParse(string str, out T ret, bool no_error = true)
		{
			if (FEnum<T>.Ostr == null)
			{
				FEnum<T>.Init();
			}
			if (FEnum<T>.Ostr.TryGetValue(str, out ret))
			{
				return true;
			}
			if (Enum.TryParse<T>(str, out ret))
			{
				FEnum<T>.Ostr[str] = ret;
				return true;
			}
			if (!no_error)
			{
				X.de(typeof(T).ToString() + "::TryParse: 不明なエントリ " + str, null);
			}
			ret = default(T);
			return false;
		}

		public static string ToStr(T t)
		{
			if (FEnum<T>.Otostr == null)
			{
				FEnum<T>.Otostr = new BDic<T, string>(8);
			}
			string text;
			if (FEnum<T>.Otostr.TryGetValue(t, out text))
			{
				return text;
			}
			return FEnum<T>.Otostr[t] = t.ToString();
		}

		public static string[] ToStrListUp(int max = -1, string prefix = null, bool tolower = true)
		{
			if (max < 0)
			{
				try
				{
					max = (int)Enum.Parse(typeof(T), "_MAX");
				}
				catch
				{
					return null;
				}
			}
			ushort num = (ushort)((tolower ? 28672 : 0) | max);
			if (FEnum<T>.Apre_ls != null && num == FEnum<T>.pre_ls_max && prefix == FEnum<T>.pre_ls_prefix)
			{
				return FEnum<T>.Apre_ls;
			}
			FEnum<T>.pre_ls_max = num;
			FEnum<T>.pre_ls_prefix = prefix;
			string[] array = new string[max];
			for (int i = 0; i < max; i++)
			{
				string text = FEnum<T>.ToStr((T)((object)Enum.ToObject(typeof(T), i)));
				if (tolower)
				{
					text = text.ToLower();
				}
				if (TX.valid(prefix))
				{
					text = prefix + text;
				}
				array[i] = text;
			}
			return array;
		}

		private static BDic<string, T> Ostr;

		private static BDic<T, string> Otostr;

		private static ushort pre_ls_max;

		private static string pre_ls_prefix;

		private static string[] Apre_ls;
	}
}
