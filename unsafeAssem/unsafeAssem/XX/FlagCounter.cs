using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;

namespace XX
{
	public class FlagCounter<T>
	{
		public FlagCounter(int alloc = 4)
		{
			this.OItem = new BDic<T, float>(alloc);
		}

		public FlagCounter<T> Clear()
		{
			this.OItem.Clear();
			return this;
		}

		public virtual FlagCounter<T> Add(T val, float maxt = 180f)
		{
			float num;
			if (this.OItem.TryGetValue(val, out num))
			{
				maxt = ((num < 0f) ? num : ((maxt < 0f) ? maxt : X.Mx(maxt, num)));
			}
			this.OItem[val] = maxt;
			return this;
		}

		public FlagCounter<T> Add(FlagCounter<T> Src, float multiple = 1f)
		{
			foreach (KeyValuePair<T, float> keyValuePair in Src.OItem)
			{
				this.Add(keyValuePair.Key, keyValuePair.Value * multiple);
			}
			return this;
		}

		public FlagCounter<T> Rem(T val)
		{
			this.OItem.Remove(val);
			return this;
		}

		public bool Has(T val)
		{
			return this.OItem.ContainsKey(val);
		}

		public bool TimeIsLess(T Key, float t)
		{
			float num;
			return this.OItem.TryGetValue(Key, out num) && (num < 0f || t < num);
		}

		public bool NotHas(T val)
		{
			return this.OItem.Count != 0 && (this.OItem.Count != 1 || !this.Has(val));
		}

		public T getMaxTimeCategory()
		{
			float num = 0f;
			T t = default(T);
			foreach (KeyValuePair<T, float> keyValuePair in this.OItem)
			{
				if (num < keyValuePair.Value)
				{
					num = X.Mx(keyValuePair.Value, num);
					t = keyValuePair.Key;
				}
			}
			return t;
		}

		public bool Has2(T val1, T val2)
		{
			return this.Has(val1) || this.Has(val2);
		}

		public float getTotal()
		{
			float num = 0f;
			foreach (KeyValuePair<T, float> keyValuePair in this.OItem)
			{
				if (keyValuePair.Value >= 0f)
				{
					num += keyValuePair.Value;
				}
			}
			return num;
		}

		public int Count
		{
			get
			{
				return this.OItem.Count;
			}
		}

		public BDic<T, float> getRawObject()
		{
			return this.OItem;
		}

		public T GetFirst()
		{
			using (Dictionary<T, float>.Enumerator enumerator = this.OItem.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					KeyValuePair<T, float> keyValuePair = enumerator.Current;
					return keyValuePair.Key;
				}
			}
			return default(T);
		}

		public float getTime(T categ, float def_val = 0f)
		{
			float num;
			if (this.OItem.TryGetValue(categ, out num))
			{
				return num;
			}
			return def_val;
		}

		public void writeBinaryTo(ByteArray Ba, Func<T, int> FConvertT2Int)
		{
			int count = this.OItem.Count;
			Ba.writeUShort((ushort)count);
			foreach (KeyValuePair<T, float> keyValuePair in this.OItem)
			{
				Ba.writeInt(FConvertT2Int(keyValuePair.Key));
				Ba.writeFloat(keyValuePair.Value);
			}
		}

		public FlagCounter<T> readBinaryFrom(ByteArray Ba, Func<int, T> FConvertInt2T)
		{
			this.Clear();
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				T t = FConvertInt2T(Ba.readInt());
				this.Add(t, Ba.readFloat());
			}
			return this;
		}

		protected BDic<T, float> OItem;
	}
}
