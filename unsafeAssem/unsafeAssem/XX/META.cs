using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX
{
	public class META : IVariableObject
	{
		public META()
		{
		}

		public META(string com)
		{
			if (com != null)
			{
				this.Load(com, null);
			}
		}

		public void setValue(string value)
		{
			this.Load(value, null);
		}

		public META ClearLoad(string com, FnMetaLoad fnMetaLoad = null)
		{
			if (this.D != null)
			{
				this.D.Clear();
			}
			this.Load(com, fnMetaLoad);
			return this;
		}

		public META Load(string com, FnMetaLoad fnMetaLoad = null)
		{
			if (com == "")
			{
				return this;
			}
			META.CR.parseText(com);
			if (this.D == null)
			{
				this.D = new BDic<string, string[]>(META.CR.getLength());
			}
			while (META.CR.read())
			{
				if (fnMetaLoad != null)
				{
					fnMetaLoad(META.CR.cmd, this, this.D);
				}
				else if (META.CR.clength == 1)
				{
					this.Add(META.CR.cmd, META.Astr1);
				}
				else if (META.CR.clength == 2)
				{
					this.Add(META.CR.cmd, new string[] { META.CR._1 });
				}
				else if (META.CR.clength > 0)
				{
					this.Add(META.CR.cmd, META.CR.slice(1, -1000));
				}
			}
			return this;
		}

		public META Add(string d, string v = "1")
		{
			string[] array;
			if (!(v == "1"))
			{
				(array = new string[1])[0] = v;
			}
			else
			{
				array = META.Astr1;
			}
			return this.Add(d, array);
		}

		public virtual META Add(string d, string[] Av)
		{
			if (this.D == null)
			{
				this.D = new BDic<string, string[]>(1);
			}
			this.D[d] = Av;
			return this;
		}

		public META Del(string k)
		{
			if (this.D != null)
			{
				this.D.Remove(k);
			}
			return this;
		}

		public string[] Get(string _s)
		{
			META.Abuf.Clear();
			META.Abuf.Add(_s);
			return this.Get(META.Abuf);
		}

		public string[] Get(string s1, string s2)
		{
			META.Abuf.Clear();
			META.Abuf.Add(s1);
			META.Abuf.Add(s2);
			return this.Get(META.Abuf);
		}

		public string[] Get(List<string> Akeys)
		{
			int count = Akeys.Count;
			int num = count;
			for (int i = 0; i < count; i++)
			{
				string text = Akeys[i];
				if (text.IndexOf("!") == 0)
				{
					if (this.D != null && this.D.ContainsKey(TX.slice(text, 1)))
					{
						return null;
					}
					num--;
				}
			}
			if (num == 0)
			{
				return META.Astr1;
			}
			if (this.D == null || this.D.Count == 0)
			{
				return null;
			}
			for (int i = 0; i < count; i++)
			{
				string text = Akeys[i];
				string[] array = X.Get<string, string[]>(this.D, text);
				if (array != null)
				{
					return array;
				}
			}
			return null;
		}

		public string slice_join(string key, int si = 0, string delimiter = " ")
		{
			string[] array = ((this.D != null) ? X.Get<string, string[]>(this.D, key) : null);
			if (array == null)
			{
				return "";
			}
			string text = "";
			int num = array.Length;
			for (int i = si; i < num; i++)
			{
				text = TX.add(text, array[i], delimiter);
			}
			return text;
		}

		public string GetS(string _s)
		{
			return this.GetSi(0, _s);
		}

		public string GetS(string _s, string _s1)
		{
			return this.GetSi(0, _s, _s1);
		}

		public string GetSi(int i, string args)
		{
			string[] array = this.Get(args);
			if (array != null && array.Length > i)
			{
				return array[i];
			}
			return null;
		}

		public string GetSi(int i, string s1, string s2)
		{
			string[] array = this.Get(s1, s2);
			if (array != null && array.Length > i)
			{
				return array[i];
			}
			return null;
		}

		public string GetJoin(string join_splitter, string s1)
		{
			string[] array = this.Get(s1);
			if (array != null && array.Length != 0)
			{
				return TX.join<string>(join_splitter, array, 0, -1);
			}
			return null;
		}

		public float GetNm(string key, float nan_default = 0f, int index = 0)
		{
			string[] array = this.Get(key);
			if (array != null && array.Length > index)
			{
				return X.Nm(array[index], nan_default, false);
			}
			return nan_default;
		}

		public bool GetNm(string key, out float result, float nan_default = 0f, int index = 0)
		{
			string[] array = this.Get(key);
			if (array != null && array.Length > index)
			{
				result = X.Nm(array[index], -100000f, false);
				if (result != -100000f)
				{
					return true;
				}
			}
			result = nan_default;
			return false;
		}

		public float[] GetNmA(string s1, string s2)
		{
			return this.GetNmAv(this.Get(s1, s2));
		}

		public float[] GetNmA(string s1)
		{
			return this.GetNmAv(this.Get(s1));
		}

		private float[] GetNmAv(string[] v)
		{
			if (v != null)
			{
				float[] array = new float[v.Length];
				for (int i = v.Length - 1; i >= 0; i--)
				{
					array[i] = X.Nm(v[i], 0f, false);
				}
				return array;
			}
			return null;
		}

		public int GetI(string key, int nan_default = 0, int index = 0)
		{
			string[] array = this.Get(key);
			if (array != null && array.Length > index)
			{
				return X.NmI(array[index], nan_default, false, false);
			}
			return nan_default;
		}

		public int GetIE(string key, int nan_default = 0, int index = 0)
		{
			string[] array = this.Get(key);
			if (array != null && array.Length > index)
			{
				return TX.evalI(array[index]);
			}
			return nan_default;
		}

		public bool GetB(string key, bool default_f = false)
		{
			return this.GetI(key, default_f ? 1 : 0, 0) != 0;
		}

		public bool GetBE(string key, bool default_f = false)
		{
			return this.GetIE(key, default_f ? 1 : 0, 0) != 0;
		}

		public int[] getDirs(string meta_key, int rotation = 0, bool flip = false, int start_i = 0)
		{
			string[] array = this.Get(meta_key);
			if (array == null)
			{
				return null;
			}
			int num = array.Length;
			int[] array2 = new int[num - start_i];
			for (int i = start_i; i < num; i++)
			{
				int num2 = CAim.parseString(array[i], 0);
				if (num2 >= 0)
				{
					if (flip)
					{
						num2 = (int)CAim.get_aim2(0f, 0f, (float)(-(float)CAim._XD(num2, 1)), (float)CAim._YD(num2, 1), false);
					}
					switch (rotation)
					{
					case 1:
						num2 = (int)CAim.get_clockwise2((AIM)num2, false);
						break;
					case 2:
						num2 = (int)CAim.get_opposite((AIM)num2);
						break;
					case 3:
						num2 = (int)CAim.get_clockwise2((AIM)num2, true);
						break;
					}
				}
				array2[i - start_i] = num2;
			}
			return array2;
		}

		public int getDirsI(string meta_key, int rotation = 0, bool flip = false, int index = 0, int def = -1)
		{
			int[] dirs = this.getDirs(meta_key, rotation, flip, 0);
			if (dirs == null || !X.BTW(0f, (float)index, (float)dirs.Length))
			{
				return def;
			}
			return dirs[index];
		}

		public BDic<string, string[]> getDataObject()
		{
			return this.D;
		}

		public META Copy(META SrcMeta, string meta_key)
		{
			BDic<string, string[]> dataObject = SrcMeta.getDataObject();
			string[] array = ((dataObject == null) ? null : X.Get<string, string[]>(dataObject, meta_key));
			if (array == null)
			{
				if (this.D != null)
				{
					this.D.Remove(meta_key);
				}
			}
			else
			{
				this.Add(meta_key, array);
			}
			return this;
		}

		public virtual META CopyAll(META SrcMeta)
		{
			BDic<string, string[]> dataObject = SrcMeta.getDataObject();
			if (dataObject == null)
			{
				return this;
			}
			if (this.D == null)
			{
				this.D = new BDic<string, string[]>(dataObject.Count);
			}
			foreach (KeyValuePair<string, string[]> keyValuePair in dataObject)
			{
				this.D[keyValuePair.Key] = keyValuePair.Value;
			}
			return this;
		}

		public META CopyAllReplacing(META SrcMeta)
		{
			if (this.D != null)
			{
				this.D.Clear();
			}
			return this.CopyAll(SrcMeta);
		}

		public bool CopyTo(string _s, List<float> A, float _def = -1f)
		{
			string[] array = this.Get(_s);
			if (array == null)
			{
				return false;
			}
			while (A.Count < array.Length)
			{
				A.Add(0f);
			}
			for (int i = array.Length - 1; i >= 0; i--)
			{
				A[i] = X.Nm(array[i], _def, false);
			}
			return true;
		}

		public virtual bool isSame(META Src)
		{
			if (Src == this)
			{
				return true;
			}
			if (Src.D == null || this.D == null)
			{
				return Src.D == null && this.D == null;
			}
			if (Src.D.Count != this.D.Count)
			{
				return false;
			}
			foreach (KeyValuePair<string, string[]> keyValuePair in this.D)
			{
				string[] value = keyValuePair.Value;
				string[] array;
				if (!Src.D.TryGetValue(keyValuePair.Key, out array))
				{
					return false;
				}
				if (value.Length != array.Length)
				{
					return false;
				}
				for (int i = value.Length - 1; i >= 0; i--)
				{
					if (value[i] != array[i])
					{
						return false;
					}
				}
			}
			return true;
		}

		public virtual bool isEmpty()
		{
			return this.D == null || this.D.Count == 0;
		}

		public string getValueString()
		{
			string text = "";
			if (this.D == null)
			{
				return text;
			}
			foreach (KeyValuePair<string, string[]> keyValuePair in this.D)
			{
				if (text != "")
				{
					text += "\n";
				}
				text = text + keyValuePair.Key + " \t " + TX.join<string>(" ", keyValuePair.Value, 0, -1);
			}
			return text;
		}

		public GameObject getGob()
		{
			return null;
		}

		protected BDic<string, string[]> D;

		private static CsvReader CR = new CsvReader(null, CsvReader.RegSpace, false);

		private static List<string> Abuf = new List<string>(4);

		protected static string[] Astr1 = new string[] { "1" };
	}
}
