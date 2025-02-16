using System;
using UnityEngine;

namespace XX
{
	public class PrefsHolderPP : PrefsHolder
	{
		public override float GetFloat(string key, float _default = 0f)
		{
			return PlayerPrefs.GetFloat(key, _default);
		}

		public override string GetString(string key, string _default = null)
		{
			return PlayerPrefs.GetString(key, _default);
		}

		public override void SetFloat(string key, float _value)
		{
			PlayerPrefs.SetFloat(key, _value);
			IN.save_prefs = true;
		}

		public override void SetString(string key, string _value)
		{
			PlayerPrefs.SetString(key, _value);
			IN.save_prefs = true;
		}

		public override void SetBytes(string key, byte[] ABa)
		{
			PlayerPrefs.SetString(key, Convert.ToBase64String(ABa, 0, ABa.Length));
			IN.save_prefs = true;
		}

		public override void DeleteKey(string key)
		{
			PlayerPrefs.DeleteKey(key);
		}

		public override bool GetBytes(string key, out byte[] Aba, bool convert = true)
		{
			string @string = this.GetString(key, null);
			Aba = null;
			if (TX.noe(@string))
			{
				return false;
			}
			bool flag;
			try
			{
				if (convert)
				{
					Aba = Convert.FromBase64String(@string);
				}
				flag = true;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		public override void DeleteAll()
		{
			PlayerPrefs.DeleteAll();
		}
	}
}
