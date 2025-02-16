using System;
using UnityEngine;

namespace XX
{
	public class ObjPool<T> where T : Behaviour
	{
		public ObjPool(string _attach_suffix, Transform _Base, int array_max, int init_pool_count = -1)
		{
			this.attach_suffix = _attach_suffix;
			this.Init(_Base, array_max, init_pool_count);
		}

		public ObjPool(GameObject _Prefab, Transform _Base, int array_max, int init_pool_count = -1)
		{
			this.Prefab = _Prefab;
			this.Init(_Base, array_max, init_pool_count);
		}

		private void Init(Transform _Base, int array_max, int init_pool_count = -1)
		{
			this.Base = _Base;
			if (init_pool_count < 0)
			{
				init_pool_count = array_max;
			}
			if (array_max <= 0)
			{
				array_max = 1;
			}
			this.AItem = new T[array_max];
			this.AItemOff = new T[array_max];
			this.Pool(init_pool_count, default(T));
		}

		public T Pool(int count = 1, T _Item = default(T))
		{
			T t = default(T);
			while (--count >= 0)
			{
				if (this.LEN_OFFLINE >= this.AItemOff.Length)
				{
					Array.Resize<T>(ref this.AItemOff, this.LEN_OFFLINE + 16);
				}
				if (_Item == null)
				{
					if (this.Prefab != null)
					{
						_Item = IN.AttachZero(this.Prefab, null).GetComponent<T>();
						if (this.Base != null)
						{
							_Item.transform.SetParent(this.Base, false);
							_Item.gameObject.layer = this.Base.gameObject.layer;
						}
					}
					else
					{
						GameObject gameObject = new GameObject(((this.Base != null) ? (this.Base.name + "-") : "") + this.attach_suffix + "." + this.count_all.ToString());
						if (this.Base != null)
						{
							gameObject.transform.SetParent(this.Base, false);
							gameObject.layer = this.Base.gameObject.layer;
						}
						_Item = gameObject.AddComponent<T>();
					}
				}
				T[] aitemOff = this.AItemOff;
				int len_OFFLINE = this.LEN_OFFLINE;
				this.LEN_OFFLINE = len_OFFLINE + 1;
				aitemOff[len_OFFLINE] = _Item;
				t = _Item;
				this.DeactivateItem(_Item);
				_Item = default(T);
			}
			return t;
		}

		public ObjPool<T> Alloc(int count = 1)
		{
			this.Pool(count - this.LEN_OFFLINE, default(T));
			return this;
		}

		public virtual T ActivateItem(T _Item)
		{
			_Item.gameObject.SetActive(true);
			return _Item;
		}

		public virtual T DeactivateItem(T _Item)
		{
			_Item.gameObject.SetActive(false);
			return _Item;
		}

		public T GetOff(int i)
		{
			if (i >= this.LEN_OFFLINE)
			{
				return this.AItem[i - this.LEN_OFFLINE];
			}
			return this.AItemOff[i];
		}

		public T GetOn(int i)
		{
			if (i >= this.LEN_ACT)
			{
				return this.AItemOff[i - this.LEN_ACT];
			}
			return this.AItem[i];
		}

		public int count_act
		{
			get
			{
				return this.LEN_ACT;
			}
		}

		public int count_all
		{
			get
			{
				return this.LEN_ACT + this.LEN_OFFLINE;
			}
		}

		public T Next()
		{
			bool flag;
			return this.Next(out flag);
		}

		public T Next(out bool new_pooled)
		{
			new_pooled = false;
			if (this.LEN_ACT >= this.AItem.Length)
			{
				Array.Resize<T>(ref this.AItem, this.LEN_ACT + 16);
			}
			if (this.LEN_OFFLINE == 0)
			{
				this.Pool(1, default(T));
				new_pooled = true;
			}
			T[] aitem = this.AItem;
			int num = this.LEN_ACT;
			this.LEN_ACT = num + 1;
			int num2 = num;
			T[] aitemOff = this.AItemOff;
			num = this.LEN_OFFLINE - 1;
			this.LEN_OFFLINE = num;
			T t = (aitem[num2] = aitemOff[num]);
			this.AItemOff[this.LEN_OFFLINE] = default(T);
			this.ActivateItem(t);
			return t;
		}

		public ObjPool<T> Release(T _Item)
		{
			int num = this.isin(_Item);
			if (num >= 0)
			{
				X.shiftEmpty<T>(this.AItem, 1, num, -1);
				this.Pool(1, _Item);
				this.LEN_ACT--;
			}
			return this;
		}

		public int isin(T _Item)
		{
			return X.isinC<T>(this.AItem, _Item);
		}

		public void setEnabled<TheType>(bool f) where TheType : Behaviour
		{
			for (int i = 0; i < this.LEN_ACT; i++)
			{
				TheType component = this.AItem[i].GetComponent<TheType>();
				if (component != null)
				{
					component.enabled = f;
				}
			}
		}

		public void ReleaseAll()
		{
			for (int i = 0; i < this.LEN_ACT; i++)
			{
				T t = this.AItem[i];
				this.Pool(1, t);
				this.AItem[i] = default(T);
			}
			this.LEN_ACT = 0;
		}

		public void destruct()
		{
			this.ReleaseAll();
			for (int i = 0; i < this.LEN_OFFLINE; i++)
			{
				IN.DestroyOne(this.AItemOff[i].gameObject);
				this.AItemOff[i] = default(T);
			}
			this.LEN_OFFLINE = (this.LEN_ACT = 0);
		}

		protected T[] AItemOff;

		protected T[] AItem;

		public string attach_suffix = "";

		private Transform Base;

		protected GameObject Prefab;

		protected int LEN_OFFLINE;

		protected int LEN_ACT;
	}
}
