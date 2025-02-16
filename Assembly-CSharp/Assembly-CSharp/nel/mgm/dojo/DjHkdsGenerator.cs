using System;
using System.Collections.Generic;
using XX;

namespace nel.mgm.dojo
{
	public class DjHkdsGenerator
	{
		public int grade
		{
			get
			{
				return X.MMX(0, X.IntR(X.NI(this.min_diff, this.max_diff, 0.75f)), 4);
			}
		}

		public DjHkdsGenerator()
		{
			this.APos = new List<DjHkdsGenerator.PosD>();
			this.AFade = new List<DjHkdsGenerator.PosD>();
			this.AStyle = new List<DjHkdsGenerator.PosD>();
		}

		internal void addPos(DjHkds.POS pos, float ratio)
		{
			for (int i = this.APos.Count - 1; i >= 0; i--)
			{
				if (this.APos[i].pos == (int)pos)
				{
					this.ratio_pos -= this.APos[i].ratio;
					this.APos.RemoveAt(i);
				}
			}
			if (ratio > 0f)
			{
				this.ratio_pos += ratio;
				this.APos.Add(new DjHkdsGenerator.PosD((int)pos, ratio));
			}
		}

		internal void addFade(DjHkds.FADE fade, float ratio)
		{
			for (int i = this.AFade.Count - 1; i >= 0; i--)
			{
				if (this.AFade[i].pos == (int)fade)
				{
					this.ratio_fade -= this.AFade[i].ratio;
					this.AFade.RemoveAt(i);
				}
			}
			if (ratio > 0f)
			{
				this.ratio_fade += ratio;
				this.AFade.Add(new DjHkdsGenerator.PosD((int)fade, ratio));
			}
		}

		internal void addStyle(DjHkds.STYLE style, float ratio)
		{
			for (int i = this.AStyle.Count - 1; i >= 0; i--)
			{
				if (this.AStyle[i].pos == (int)style)
				{
					this.ratio_style -= this.AStyle[i].ratio;
					this.AStyle.RemoveAt(i);
				}
			}
			if (ratio > 0f)
			{
				this.ratio_style += ratio;
				this.AStyle.Add(new DjHkdsGenerator.PosD((int)style, ratio));
			}
		}

		internal DjHkds.POS generatePos()
		{
			int num = this.APos.Count;
			if (num == 0)
			{
				return DjHkds.POS.N;
			}
			float num2 = X.XORSP() * this.ratio_pos;
			for (int i = 0; i < num - 1; i++)
			{
				DjHkdsGenerator.PosD posD = this.APos[i];
				if (num2 < posD.ratio)
				{
					return (DjHkds.POS)posD.pos;
				}
				num2 -= posD.ratio;
			}
			return (DjHkds.POS)this.APos[num - 1].pos;
		}

		internal DjHkds.FADE generateFade()
		{
			int num = this.AFade.Count;
			if (num == 0)
			{
				return DjHkds.FADE.N;
			}
			float num2 = X.XORSP() * this.ratio_fade;
			for (int i = 0; i < num - 1; i++)
			{
				DjHkdsGenerator.PosD posD = this.AFade[i];
				if (num2 < posD.ratio)
				{
					return (DjHkds.FADE)posD.pos;
				}
				num2 -= posD.ratio;
			}
			return (DjHkds.FADE)this.AFade[num - 1].pos;
		}

		internal DjHkds.STYLE generateStyle()
		{
			int num = this.AStyle.Count;
			if (num == 0)
			{
				return DjHkds.STYLE.N;
			}
			float num2 = X.XORSP() * this.ratio_style;
			for (int i = 0; i < num - 1; i++)
			{
				DjHkdsGenerator.PosD posD = this.AStyle[i];
				if (num2 < posD.ratio)
				{
					return (DjHkds.STYLE)posD.pos;
				}
				num2 -= posD.ratio;
			}
			return (DjHkds.STYLE)this.AStyle[num - 1].pos;
		}

		public const int count_default = 8;

		private readonly List<DjHkdsGenerator.PosD> APos;

		private float ratio_pos;

		private readonly List<DjHkdsGenerator.PosD> AFade;

		private readonly List<DjHkdsGenerator.PosD> AStyle;

		private float ratio_fade;

		private float ratio_style;

		public int count = 8;

		public float min_diff;

		public float max_diff = 3f;

		private struct PosD
		{
			public PosD(int _pos, float _ratio)
			{
				this.pos = _pos;
				this.ratio = _ratio;
			}

			public readonly int pos;

			public readonly float ratio;
		}
	}
}
