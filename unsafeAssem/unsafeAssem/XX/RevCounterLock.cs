using System;

namespace XX
{
	public class RevCounterLock : RevCounter
	{
		public override RevCounter Add(float addval, bool once_in_one_frame = false, bool soft = false)
		{
			if (this.lock_t > 0f)
			{
				return this;
			}
			return base.Add(addval, once_in_one_frame, soft);
		}

		public RevCounterLock Lock(float _lock_t, bool clear_val = false)
		{
			this.lock_t = X.Mx(this.lock_t, _lock_t);
			if (clear_val)
			{
				base.Clear();
			}
			return this;
		}

		public override RevCounter Clear()
		{
			this.lock_t = 0f;
			return base.Clear();
		}

		public RevCounterLock ClearLock()
		{
			this.lock_t = 0f;
			return this;
		}

		public bool has_lock
		{
			get
			{
				return this.lock_t > 0f;
			}
		}

		public override RevCounter Update(float fcnt)
		{
			if (this.lock_t > 0f)
			{
				this.lock_t = X.Mx(this.lock_t - fcnt, 0f);
				if (this.val <= 0f)
				{
					return this;
				}
			}
			return base.Update(fcnt);
		}

		public bool isLocked()
		{
			return this.lock_t > 0f;
		}

		protected float lock_t;
	}
}
