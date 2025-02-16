using System;
using Better;
using XX;

namespace m2d
{
	public class LockCounter<T>
	{
		public LockCounter(int capacity = 4)
		{
			this.O = new BDic<T, float>(capacity);
		}

		public void clear()
		{
			this.O.Clear();
		}

		public bool isLocked(T key)
		{
			float num;
			if (this.O.TryGetValue(key, out num))
			{
				if (num < 0f || this.getFrame() < num)
				{
					return true;
				}
				this.O.Remove(key);
			}
			return false;
		}

		public LockCounter<T> Add(T key, float lock_time)
		{
			float num;
			if (this.O.TryGetValue(key, out num))
			{
				this.O[key] = ((lock_time < 0f) ? lock_time : X.Mx(num, this.getFrame() + lock_time));
			}
			else
			{
				this.O[key] = ((lock_time < 0f) ? lock_time : (this.getFrame() + lock_time));
			}
			return this;
		}

		public virtual float getFrame()
		{
			return (float)IN.totalframe;
		}

		private readonly BDic<T, float> O;
	}
}
