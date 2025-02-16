using System;
using System.Collections.Generic;
using Better;

namespace XX
{
	public class HashP
	{
		public HashP(int Capacity = 8)
		{
			this.OHash = new BDic<string, StringKey>(Capacity);
		}

		public void Clear()
		{
			this.OHash.Clear();
		}

		public StringKey Get(string s)
		{
			StringKey stringKey;
			if (this.OHash.TryGetValue(s, out stringKey))
			{
				return stringKey;
			}
			Dictionary<string, StringKey> ohash = this.OHash;
			StringKey stringKey2 = new StringKey(s);
			ohash[s] = stringKey2;
			return stringKey2;
		}

		public bool Has(string s)
		{
			StringKey stringKey;
			return this.OHash.TryGetValue(s, out stringKey);
		}

		public HashP Set(string s, string val = null)
		{
			this.OHash[s] = new StringKey(val ?? s);
			return this;
		}

		public StringKey Get(ReplacableString S, VariableP VP)
		{
			if (S.use_vp)
			{
				STB vpBaked = S.Bake(VP).VpBaked;
				return this.Get(vpBaked, 0, -1);
			}
			return this.Get(S.ToString());
		}

		public StringKey Get(STB Stb, int char_i = 0, int char_len = -1)
		{
			if (char_len < 0)
			{
				char_len = Stb.Length;
			}
			char_len += char_i;
			foreach (KeyValuePair<string, StringKey> keyValuePair in this.OHash)
			{
				StringKey value = keyValuePair.Value;
				if (Stb.Equals(char_i, char_len, value.Value, false))
				{
					return value;
				}
			}
			string text = Stb.get_slice(char_i, char_len);
			Dictionary<string, StringKey> ohash = this.OHash;
			string text2 = text;
			StringKey stringKey = new StringKey(text);
			ohash[text2] = stringKey;
			return stringKey;
		}

		public BDic<string, StringKey> OHash;
	}
}
