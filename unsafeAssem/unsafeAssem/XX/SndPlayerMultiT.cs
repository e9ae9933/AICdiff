using System;
using System.Collections.Generic;

namespace XX
{
	public class SndPlayerMultiT<T> where T : SndPlayer
	{
		public SndPlayerMultiT(int capacity)
		{
			this.APlayer = new List<T>(capacity);
		}

		public T this[int index]
		{
			get
			{
				return this.APlayer[index];
			}
			set
			{
				this.APlayer[index] = value;
			}
		}

		public virtual void destruct()
		{
			int count = this.APlayer.Count;
			for (int i = 0; i < count; i++)
			{
				T t = this.APlayer[i];
				if (t != null)
				{
					t.Dispose();
				}
			}
			this.APlayer.Clear();
		}

		public virtual SndPlayerMultiT<T> flush(string remove_header, int start_index = 0)
		{
			int count = this.APlayer.Count;
			for (int i = start_index; i < count; i++)
			{
				T t = this.APlayer[i];
				if (t != null && (!t.isPlaying() || remove_header == null || t.key.IndexOf(remove_header) == 0))
				{
					t.Dispose();
					this.APlayer[i] = default(T);
				}
			}
			return this;
		}

		public virtual SndPlayerMultiT<T> kill(string remove_header)
		{
			int count = this.APlayer.Count;
			for (int i = 0; i < count; i++)
			{
				T t = this.APlayer[i];
				if (t != null && (remove_header == null || t.key.IndexOf(remove_header) == 0))
				{
					t.Stop();
				}
			}
			return this;
		}

		public List<T> APlayer;

		public bool need_update_flag;
	}
}
