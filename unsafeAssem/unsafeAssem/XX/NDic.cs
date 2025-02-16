using System;
using Better;
using UnityEngine;

namespace XX
{
	public class NDic<T> : BDic<string, T> where T : class
	{
		public NDic(string _name, int _max, out int _capacity)
			: base(_capacity = X.Mx(_max, PlayerPrefs.GetInt("__NDic__" + _name)))
		{
			this.name = "__NDic__" + _name;
		}

		public NDic(string _name, int _max = 0)
		{
			int num;
			this..ctor(_name, _max, out num);
		}

		public void scriptFinalize()
		{
			PlayerPrefs.SetInt(this.name, base.Count);
			IN.save_prefs = true;
		}

		public T Get(string key)
		{
			return X.Get<string, T>(this, key);
		}

		public T Get(StringKey key)
		{
			return X.Get<T>(this, key);
		}

		public readonly string name;
	}
}
