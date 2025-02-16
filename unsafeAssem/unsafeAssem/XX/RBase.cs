using System;

namespace XX
{
	public abstract class RBase<T> where T : class, IRunAndDestroy
	{
		public RBase(int alloc_cnt, bool _alloc_instance = true, bool _input_null_when_destruct = false, bool _send_destruct = false)
		{
			this.alloc_instance = _alloc_instance;
			this.AItems = new T[alloc_cnt];
			this.send_destruct = _send_destruct;
			if (!_input_null_when_destruct)
			{
				this.AItemsBuf = new T[8];
			}
			if (this.alloc_instance)
			{
				for (int i = 0; i < alloc_cnt; i++)
				{
					this.AItems[i] = this.Create();
				}
			}
		}

		public void Alloc(int cnt)
		{
			if (this.AItems.Length < cnt)
			{
				Array.Resize<T>(ref this.AItems, cnt);
			}
			for (int i = 0; i < cnt; i++)
			{
				if (this.AItems[i] == null)
				{
					this.AItems[i] = this.Create();
				}
			}
		}

		public abstract T Create();

		public void clearAt(int i)
		{
			if (i >= this.LEN)
			{
				return;
			}
			this.AItems[i].destruct();
			if (this.alloc_instance)
			{
				X.shiftNotInput1<T>(this.AItems, i, ref this.LEN);
				return;
			}
			T[] aitems = this.AItems;
			int num = 1;
			int len = this.LEN;
			this.LEN = len - 1;
			X.shiftEmpty<T>(aitems, num, i, len);
		}

		public virtual void clear()
		{
			if (this.AItems == null)
			{
				return;
			}
			if (!this.alloc_instance)
			{
				for (int i = this.AItems.Length - 1; i >= 0; i--)
				{
					T t = this.AItems[i];
					if (t != null)
					{
						t.destruct();
						this.AItems[i] = default(T);
					}
				}
			}
			this.LEN = 0;
			if (this.AItemsBuf != null)
			{
				X.clrA<T>(this.AItemsBuf);
			}
		}

		protected T Add(T Itm, int alloc_cnt = 64)
		{
			if (this.LEN >= this.AItems.Length)
			{
				Array.Resize<T>(ref this.AItems, this.LEN + alloc_cnt);
			}
			T[] aitems = this.AItems;
			int len = this.LEN;
			this.LEN = len + 1;
			aitems[len] = Itm;
			return Itm;
		}

		protected T Pop(int alloc_cnt = 64)
		{
			if (this.LEN >= this.AItems.Length)
			{
				int num = this.LEN + alloc_cnt;
				Array.Resize<T>(ref this.AItems, this.LEN + alloc_cnt);
				if (this.alloc_instance)
				{
					for (int i = this.LEN; i < num; i++)
					{
						this.AItems[i] = this.Create();
					}
				}
			}
			T t = this.AItems[this.LEN];
			if (t == null)
			{
				T[] aitems = this.AItems;
				int len = this.LEN;
				this.LEN = len + 1;
				return aitems[len] = this.Create();
			}
			this.LEN++;
			return t;
		}

		public virtual bool run(float fcnt)
		{
			int i = 0;
			int num = 0;
			if (fcnt == -1f)
			{
				fcnt = IN.deltaFrame;
			}
			while (i < this.LEN)
			{
				T t = ((num == 0) ? this.AItems[i] : (this.AItems[i - num] = this.AItems[i]));
				if (!t.run(fcnt))
				{
					if (this.AItemsBuf != null)
					{
						X.pushToEmptyS<T>(ref this.AItemsBuf, t, ref num, 4);
						if (this.send_destruct)
						{
							t.destruct();
						}
					}
					else
					{
						t.destruct();
						num++;
					}
				}
				i++;
			}
			if (num > 0)
			{
				i = this.LEN - num;
				int num2 = 0;
				if (this.AItemsBuf != null)
				{
					while (i < this.LEN)
					{
						this.AItems[i++] = this.AItemsBuf[num2];
						this.AItemsBuf[num2++] = default(T);
					}
				}
				else
				{
					while (i < this.LEN)
					{
						this.AItems[i++] = default(T);
					}
				}
				this.LEN -= num;
			}
			return this.LEN > 0;
		}

		private bool checkDupe(T Buf)
		{
			int num = 0;
			for (int i = 0; i < this.LEN; i++)
			{
				if (Buf == this.AItems[i] && ++num >= 2)
				{
					this.AItems[i] = Buf;
					return true;
				}
			}
			return false;
		}

		public virtual void destruct()
		{
			this.alloc_instance = false;
			this.clear();
			this.AItems = X.clrA<T>(this.AItems);
			this.AItemsBuf = X.clrA<T>(this.AItemsBuf);
		}

		public virtual bool isActive()
		{
			return this.LEN > 0;
		}

		public int Length
		{
			get
			{
				return this.LEN;
			}
		}

		protected T[] AItems;

		private T[] AItemsBuf;

		protected int LEN;

		protected bool alloc_instance;

		public bool send_destruct;
	}
}
