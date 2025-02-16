using System;

namespace XX
{
	public class SResourceReader : IDisposable
	{
		public SResourceReader(string resource, bool _use_stb_from_tx = false)
		{
			this.use_stb_from_tx = _use_stb_from_tx;
			if (this.use_stb_from_tx)
			{
				this.Stb = TX.PopBld(null, 0);
			}
			else
			{
				this.Stb = new STB((resource != null) ? resource.Length : 64);
			}
			if (resource != null)
			{
				this.Stb.Set(resource);
			}
		}

		public void Begin()
		{
			this.cur_char = (this.line_index = 0);
		}

		public bool readCorrectly(STB StbOut)
		{
			while (this.cur_char < this.Stb.Length)
			{
				int num;
				this.Stb.ScrollLine(this.cur_char, out num, -1);
				StbOut.Clear().Add(this.Stb, this.cur_char, num - this.cur_char);
				this.line_index++;
				this.cur_char = num + 1;
				int num2;
				StbOut.TaleTrimSpace(StbOut.Length, out num2, 0);
				if (num2 > 0 && StbOut.Length > 0)
				{
					int num3 = 0;
					while (num3 < num2 && StbOut.isSpaceOrCommaOrTilde(num3))
					{
						num3++;
					}
					if (num3 < num2 && !StbOut.isStart("//", num3))
					{
						StbOut.Length = num2;
						if (num3 > 0)
						{
							StbOut.Splice(0, num3);
						}
						return true;
					}
				}
			}
			return false;
		}

		public void Dispose()
		{
			if (this.use_stb_from_tx)
			{
				TX.ReleaseBld(this.Stb);
			}
			this.use_stb_from_tx = false;
		}

		private readonly STB Stb;

		private bool use_stb_from_tx;

		private int cur_char;

		private int line_index;
	}
}
