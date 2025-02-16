using System;
using System.Collections.Generic;

namespace XX
{
	public class EffectHandler<T> where T : EffectItem
	{
		public EffectHandler(int capacity = 2, T _Ef = default(T))
		{
			this.AEf = new List<EffectHandler<T>.HandledItem>(capacity);
			this.Set(_Ef);
		}

		public int Count
		{
			get
			{
				return this.AEf.Count;
			}
		}

		public EffectHandler<T> Set(T _Ef)
		{
			if (_Ef == null)
			{
				return this;
			}
			this.AEf.Add(new EffectHandler<T>.HandledItem
			{
				Ef = _Ef,
				s_index = _Ef.index
			});
			return this;
		}

		public EffectHandler<T> release(bool no_destruct = false)
		{
			if (!no_destruct)
			{
				int count = this.AEf.Count;
				for (int i = 0; i < count; i++)
				{
					EffectHandler<T>.HandledItem handledItem = this.AEf[i];
					if (handledItem.isActive())
					{
						handledItem.Ef.destruct();
					}
				}
			}
			this.AEf.Clear();
			return this;
		}

		public bool has(string s)
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				EffectHandler<T>.HandledItem handledItem = this.AEf[i];
				if (handledItem.isActive() && handledItem.Ef.title == s)
				{
					return true;
				}
			}
			return false;
		}

		public bool isActive()
		{
			int count = this.AEf.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.AEf[i].isActive())
				{
					return true;
				}
			}
			return false;
		}

		protected List<EffectHandler<T>.HandledItem> AEf;

		protected struct HandledItem
		{
			public bool isActive()
			{
				return this.Ef != null && this.Ef.FnDef != null && this.Ef.index == this.s_index;
			}

			public T Ef;

			public uint s_index;
		}
	}
}
