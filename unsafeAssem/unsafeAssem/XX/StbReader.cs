using System;

namespace XX
{
	public class StbReader : STB
	{
		public StbReader(int capacity = 0, string _source = null)
			: base(capacity)
		{
			if (_source != null)
			{
				this.initSource(_source);
			}
		}

		public void initSource(string _source)
		{
			this.source = _source;
			this.seek_set();
		}

		public void seek_set()
		{
			this.cur_line = -1;
			this.cur_seek_pos = 0;
		}

		public int get_cur_line()
		{
			return this.cur_line;
		}

		public bool readCorrectly(out int lcs, out int lce, bool comment_clip = true)
		{
			int length = this.source.Length;
			base.Clear();
			if (this.source == null || this.cur_seek_pos >= length)
			{
				lce = (lcs = 0);
				return false;
			}
			lcs = this.cur_seek_pos;
			this.cur_line++;
			lce = lcs;
			while (lce < length)
			{
				if (this.source[lce] == '\n')
				{
					this.cur_seek_pos = lce + 1;
					break;
				}
				if (this.source[lce] == '\r')
				{
					this.cur_seek_pos = lce + 1;
					if (this.cur_seek_pos < length - 1 && this.source[this.cur_seek_pos + 1] == '\n')
					{
						this.cur_seek_pos++;
						break;
					}
					break;
				}
				else
				{
					lce++;
				}
			}
			if (lce >= length)
			{
				this.cur_seek_pos = length;
			}
			base.EnsureCapacity(lce - lcs);
			for (int i = lcs; i < lce; i++)
			{
				base.Add(this.source[i]);
			}
			lce -= lcs;
			lcs = 0;
			if (comment_clip)
			{
				base.csvAdjust(ref lcs, out lce, -1);
			}
			else
			{
				base.TrimSpace(lcs, out lcs, out lce, -1);
			}
			return true;
		}

		private string source;

		private int cur_line = -1;

		private int cur_seek_pos;
	}
}
