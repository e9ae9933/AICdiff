using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;

namespace XX
{
	public class FloatCounter<T>
	{
		public FloatCounter(int alloc = 4)
		{
			this.OItem = new BDic<T, FloatCounter<T>.FloatCounterItem>(alloc);
			this.AOffline = new List<FloatCounter<T>.FloatCounterItem>(alloc);
			this.Alloc(alloc);
		}

		public FloatCounter<T> Clear(bool completely = false)
		{
			foreach (KeyValuePair<T, FloatCounter<T>.FloatCounterItem> keyValuePair in this.OItem)
			{
				if (this.ADeleted != null)
				{
					this.ADeleted.Add(keyValuePair.Key);
				}
				this.AOffline.Add(keyValuePair.Value);
			}
			if (completely && this.ADeleted != null)
			{
				this.ADeleted.Clear();
			}
			this.OItem.Clear();
			this.AItmForDelete.Clear();
			return this;
		}

		private FloatCounter<T> Alloc(int store = 1)
		{
			while (this.AOffline.Count < store)
			{
				this.AOffline.Add(new FloatCounter<T>.FloatCounterItem());
			}
			return this;
		}

		private FloatCounter<T>.FloatCounterItem Pop()
		{
			this.Alloc(1);
			FloatCounter<T>.FloatCounterItem floatCounterItem = this.AOffline[this.AOffline.Count - 1];
			this.AOffline.RemoveAt(this.AOffline.Count - 1);
			return floatCounterItem;
		}

		public FloatCounter<T> Add(T val, float _level, float maxt = 180f, FloatCounter<T>.FnNoReduce fnNoReduce = null)
		{
			FloatCounter<T>.FloatCounterItem floatCounterItem;
			if (!this.OItem.TryGetValue(val, out floatCounterItem))
			{
				floatCounterItem = (this.OItem[val] = this.Pop());
			}
			floatCounterItem.Set(maxt, _level, fnNoReduce, null);
			return this;
		}

		public FloatCounter<T> AddAF(T val, float _level, float maxt = 180f, List<FloatCounter<T>.FnNoReduce> AfnNoReduce = null)
		{
			FloatCounter<T>.FloatCounterItem floatCounterItem;
			if (!this.OItem.TryGetValue(val, out floatCounterItem))
			{
				floatCounterItem = (this.OItem[val] = this.Pop());
			}
			floatCounterItem.Set(maxt, _level, null, AfnNoReduce);
			return this;
		}

		public bool Merge(T val, float _level, float maxt = 180f, FloatCounter<T>.FnNoReduce fnNoReduce = null)
		{
			FloatCounter<T>.FloatCounterItem floatCounterItem;
			if (this.OItem.TryGetValue(val, out floatCounterItem))
			{
				floatCounterItem.Merge(maxt, _level, fnNoReduce);
				return false;
			}
			floatCounterItem = (this.OItem[val] = this.Pop());
			floatCounterItem.Set(maxt, _level, fnNoReduce, null);
			return true;
		}

		public FloatCounter<T> Add(FloatCounter<T> Src, float multiple_level = 1f, float multiple_time = 1f)
		{
			foreach (KeyValuePair<T, FloatCounter<T>.FloatCounterItem> keyValuePair in Src.OItem)
			{
				float t = keyValuePair.Value.t;
				this.AddAF(keyValuePair.Key, keyValuePair.Value.level * multiple_level, (t < 0f) ? t : (t * multiple_time), keyValuePair.Value.AFnCannotReduce);
			}
			return this;
		}

		public FloatCounter<T> Rem(T val)
		{
			FloatCounter<T>.FloatCounterItem floatCounterItem;
			if (this.OItem.TryGetValue(val, out floatCounterItem))
			{
				this.OItem.Remove(val);
				if (this.ADeleted != null)
				{
					this.ADeleted.Add(val);
				}
				this.AOffline.Add(floatCounterItem);
			}
			return this;
		}

		public bool Has(T val)
		{
			return this.OItem.ContainsKey(val);
		}

		public bool NotHas(T val)
		{
			return this.OItem.Count != 0 && (this.OItem.Count != 1 || !this.Has(val));
		}

		public T getMaxTimeCategory()
		{
			float num = 0f;
			T t = default(T);
			foreach (KeyValuePair<T, FloatCounter<T>.FloatCounterItem> keyValuePair in this.OItem)
			{
				FloatCounter<T>.FloatCounterItem value = keyValuePair.Value;
				if (num < value.t)
				{
					num = value.t;
					t = keyValuePair.Key;
				}
			}
			return t;
		}

		public bool clipByLevel(float level)
		{
			bool flag = false;
			this.AItmForDelete.Clear();
			foreach (KeyValuePair<T, FloatCounter<T>.FloatCounterItem> keyValuePair in this.OItem)
			{
				if (keyValuePair.Value.level <= level)
				{
					this.AItmForDelete.Add(keyValuePair.Key);
					flag = true;
				}
			}
			for (int i = this.AItmForDelete.Count - 1; i >= 0; i--)
			{
				T t = this.AItmForDelete[i];
				this.AOffline.Add(this.OItem[t]);
				this.OItem.Remove(t);
				if (this.ADeleted != null)
				{
					this.ADeleted.Add(t);
				}
			}
			return flag;
		}

		public bool run(float fcnt)
		{
			bool flag = false;
			this.AItmForDelete.Clear();
			foreach (KeyValuePair<T, FloatCounter<T>.FloatCounterItem> keyValuePair in this.OItem)
			{
				FloatCounter<T>.FloatCounterItem value = keyValuePair.Value;
				if (value.t == -1000f)
				{
					value.t = -1001f;
				}
				else if (value.t <= -1000f || value.t > 0f)
				{
					value.t -= fcnt;
					if (value.checkReduce(keyValuePair.Key) && value.t <= 0f)
					{
						this.AItmForDelete.Add(keyValuePair.Key);
						flag = true;
					}
				}
			}
			for (int i = this.AItmForDelete.Count - 1; i >= 0; i--)
			{
				T t = this.AItmForDelete[i];
				this.AOffline.Add(this.OItem[t]);
				this.OItem.Remove(t);
				if (this.ADeleted != null)
				{
					this.ADeleted.Add(t);
				}
			}
			return flag;
		}

		public float getMaxLevel()
		{
			float num = 0f;
			foreach (KeyValuePair<T, FloatCounter<T>.FloatCounterItem> keyValuePair in this.OItem)
			{
				num = X.Mx(num, keyValuePair.Value.level);
			}
			return num;
		}

		public FloatCounter<T> CopyFrom(FloatCounter<T> Flg, float level = 1f)
		{
			this.Clear(false);
			this.Add(Flg, level, 1f);
			return this;
		}

		public BDic<T, FloatCounter<T>.FloatCounterItem> getRawObject()
		{
			return this.OItem;
		}

		public int Count
		{
			get
			{
				return this.OItem.Count;
			}
		}

		public void writeBinaryTo(ByteArray Ba, Func<T, int> FConvertT2Int)
		{
			int count = this.OItem.Count;
			Ba.writeUShort((ushort)count);
			foreach (KeyValuePair<T, FloatCounter<T>.FloatCounterItem> keyValuePair in this.OItem)
			{
				Ba.writeInt(FConvertT2Int(keyValuePair.Key));
				Ba.writeFloat(keyValuePair.Value.level);
				Ba.writeFloat(keyValuePair.Value.t);
			}
		}

		public FloatCounter<T> readBinaryFrom(ByteArray Ba, Func<int, T> FConvertInt2T)
		{
			this.Clear(true);
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				T t = FConvertInt2T(Ba.readInt());
				this.Add(t, Ba.readFloat(), Ba.readFloat(), null);
			}
			return this;
		}

		protected BDic<T, FloatCounter<T>.FloatCounterItem> OItem;

		private List<FloatCounter<T>.FloatCounterItem> AOffline;

		private List<T> AItmForDelete = new List<T>(4);

		public List<T> ADeleted;

		public delegate bool FnNoReduce(T value);

		public sealed class FloatCounterItem
		{
			public FloatCounter<T>.FloatCounterItem Set(float maxt, float _level, FloatCounter<T>.FnNoReduce _FnCannotReduce = null, List<FloatCounter<T>.FnNoReduce> _AFnCannotReduce = null)
			{
				this.t = maxt;
				this.level = _level;
				if (_FnCannotReduce != null)
				{
					this.AFnCannotReduce.Add(_FnCannotReduce);
				}
				if (_AFnCannotReduce != null)
				{
					this.AFnCannotReduce.AddRange(_AFnCannotReduce);
				}
				return this;
			}

			public FloatCounter<T>.FloatCounterItem Merge(float maxt, float _level, FloatCounter<T>.FnNoReduce _FnCannotReduce = null)
			{
				if (maxt > -1000f && this.t > -1000f)
				{
					this.t = ((maxt < 0f || this.t < 0f) ? (-1f) : X.Mx(maxt, this.t));
				}
				else
				{
					this.t = -1000f;
				}
				this.level = X.Mx(_level, this.level);
				if (_FnCannotReduce != null && this.AFnCannotReduce.IndexOf(_FnCannotReduce) == -1)
				{
					this.AFnCannotReduce.Add(_FnCannotReduce);
				}
				return this;
			}

			public bool checkReduce(T Key)
			{
				int count = this.AFnCannotReduce.Count;
				for (int i = 0; i < count; i++)
				{
					if (this.AFnCannotReduce[i](Key))
					{
						return false;
					}
				}
				return true;
			}

			public float level;

			public float t;

			public List<FloatCounter<T>.FnNoReduce> AFnCannotReduce = new List<FloatCounter<T>.FnNoReduce>(1);
		}
	}
}
