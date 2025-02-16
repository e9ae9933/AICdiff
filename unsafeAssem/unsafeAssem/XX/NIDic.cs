using System;
using Better;
using UnityEngine;

namespace XX
{
	public class NIDic<T, T2> : BDic<T, T2>
	{
		public NIDic(string _name)
			: base(PlayerPrefs.GetInt("__NIDic__" + _name))
		{
			this.name = "__NIDic__" + _name;
		}

		public void scriptFinalize()
		{
			PlayerPrefs.SetInt(this.name, base.Count);
			IN.save_prefs = true;
		}

		public readonly string name;
	}
}
