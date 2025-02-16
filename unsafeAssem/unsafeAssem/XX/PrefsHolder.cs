using System;

namespace XX
{
	public abstract class PrefsHolder
	{
		public abstract float GetFloat(string key, float _default = 0f);

		public abstract string GetString(string key, string _default = null);

		public abstract bool GetBytes(string key, out byte[] Aba, bool convert = true);

		public abstract void SetFloat(string key, float _value);

		public abstract void SetString(string key, string _value);

		public abstract void SetBytes(string key, byte[] ABa);

		public abstract void DeleteKey(string key);

		public abstract void DeleteAll();
	}
}
