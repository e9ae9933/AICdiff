using System;

namespace XX
{
	public class ElecTraceDrawer
	{
		public ElecTraceDrawer(float _maxt, float _interval)
		{
			this.maxt = _maxt;
			this.interval = _interval;
			int num = X.IntC(this.maxt / _interval);
			this.AItm = new ElecDrawer[num, 2];
			this.InCol0 = new C32();
			this.InCol1 = new C32();
			this.OutCol0 = new C32();
			this.OutCol1 = new C32();
			this.InColE0 = new C32();
			this.InColE1 = new C32();
			this.OutColE0 = new C32();
			this.OutColE1 = new C32();
		}

		public ElecTraceDrawer InnerColor(uint u)
		{
			this.InCol0.Set(u);
			this.InCol1.Set(u);
			this.InColE0.Set(u);
			this.InColE1.Set(u);
			return this;
		}

		public ElecTraceDrawer InnerColor(uint u0, uint u1)
		{
			this.InCol0.Set(u0);
			this.InCol1.Set(u1);
			this.InColE0.Set(u0);
			this.InColE1.Set(u1);
			return this;
		}

		public ElecTraceDrawer InnerEndColor(uint u)
		{
			this.InColE0.Set(u);
			this.InColE1.Set(u);
			return this;
		}

		public ElecTraceDrawer InnerEndColor(uint u0, uint u1)
		{
			this.InColE0.Set(u0);
			this.InColE1.Set(u1);
			return this;
		}

		public ElecTraceDrawer OutColor(uint u0)
		{
			this.OutCol0.Set(u0);
			this.OutCol1.Set(u0).setA(0f);
			this.OutColE0.Set(u0);
			this.OutColE1.Set(u0).setA(0f);
			return this;
		}

		public ElecTraceDrawer OutColor(uint u0, uint u1)
		{
			this.OutCol0.Set(u0);
			this.OutCol1.Set(u1);
			this.OutColE0.Set(u0);
			this.OutColE1.Set(u1);
			return this;
		}

		public ElecTraceDrawer OutEndColor(uint u0)
		{
			this.OutColE0.Set(u0);
			this.OutColE1.Set(u0).setA(0f);
			return this;
		}

		public ElecTraceDrawer OutEndColor(uint u0, uint u1)
		{
			this.OutColE0.Set(u0);
			this.OutColE1.Set(u1);
			return this;
		}

		public ElecTraceDrawer DivideWidth(float _v)
		{
			this.divide_w = _v * 0.015625f;
			return this;
		}

		public ElecTraceDrawer JumpHeight(float s, float d = -1000f)
		{
			this.jump_hs = s * 0.015625f;
			this.jump_hd = ((d == -1000f) ? this.jump_hs : (d * 0.015625f));
			return this;
		}

		public ElecTraceDrawer JumpRatio(float _v)
		{
			this.jump_ratio = _v;
			return this;
		}

		public ElecTraceDrawer BallRadius(float s, float d = -1000f)
		{
			this.sball_r = s * 0.015625f;
			this.dball_r = ((d == -1000f) ? this.sball_r : (d * 0.015625f));
			return this;
		}

		public ElecTraceDrawer Thick(float s, float d = -1000f)
		{
			this.thicks = s * 0.015625f;
			this.thickd = ((d == -1000f) ? this.thicks : (d * 0.015625f));
			return this;
		}

		public ElecTraceDrawer release()
		{
			this.index = -1;
			this.t = 0f;
			int length = this.AItm.GetLength(0);
			int num = 0;
			while (num < length && this.AItm[num, 0] != null)
			{
				this.AItm[num, 0].kaku = (this.AItm[num, 1].kaku = -1);
				num++;
			}
			return this;
		}

		public void Add(float _dx, float _dy)
		{
			int length = this.AItm.GetLength(0);
			if (this.index < 0)
			{
				this.cx = _dx;
				this.cy = _dy;
				for (int i = 0; i < length; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						float num = ((j == 1) ? this.out_scale : 1f);
						ElecDrawer elecDrawer = (this.AItm[i, j] = new ElecDrawer());
						elecDrawer.divide_w = this.divide_w;
						elecDrawer.jump_hs = this.jump_hs;
						elecDrawer.jump_hd = this.jump_hd;
						elecDrawer.jump_ratio = this.jump_ratio;
						elecDrawer.thicks = this.thicks * num;
						elecDrawer.thickd = this.thickd * num;
						elecDrawer.refine_time = ((this.min_minimize_level >= 1f) ? this.maxt : (this.maxt / (1f - this.min_minimize_level)));
						elecDrawer.sball_r = this.sball_r * num;
						elecDrawer.dball_r = this.dball_r * num;
						elecDrawer.halo_rate = this.halo_rate;
						elecDrawer.halo_rate_extend = this.halo_rate_extend;
						elecDrawer.halo_thick_rate = this.halo_thick_rate;
						elecDrawer.halo_thick_rate_extend = this.halo_thick_rate_extend;
						elecDrawer.kaku = -1;
						if (j == 1)
						{
							elecDrawer.ran = this.AItm[i, 0].ran;
						}
					}
				}
				this.index = 0;
				this.t = 0f;
			}
			else
			{
				this.t = (float)((int)X.Mx(1f, this.t - this.interval));
			}
			for (int k = 0; k < 2; k++)
			{
				ElecDrawer elecDrawer2 = this.AItm[this.index, k];
				elecDrawer2.kaku = this.kaku;
				elecDrawer2.finePos(this.cx, this.cy, _dx, _dy);
				elecDrawer2.t = 0f;
			}
			this.cx = X.MULWALK(this.cx, _dx, this.trace_pos_mulwalk);
			this.cy = X.MULWALK(this.cy, _dy, this.trace_pos_mulwalk);
			this.index = (this.index + 1) % length;
		}

		public void draw(MeshDrawer MdIn, MeshDrawer MdOut = null, float fcnt = 1f)
		{
			if (this.index < 0)
			{
				return;
			}
			if (MdOut == null)
			{
				MdOut = MdIn;
			}
			int length = this.AItm.GetLength(0);
			fcnt = ((this.min_minimize_level >= 1f) ? 0f : fcnt);
			C32 col = EffectItem.Col1;
			for (int i = 0; i < length; i++)
			{
				ElecDrawer elecDrawer = this.AItm[i, 0];
				if (elecDrawer.kaku >= 0)
				{
					float num = elecDrawer.t / this.maxt;
					if (num > 1f)
					{
						elecDrawer.kaku = -1;
					}
					else
					{
						ElecDrawer elecDrawer2 = this.AItm[i, 1];
						MdIn.ColGrd.Set(this.InCol1).blend(this.InColE1, num);
						MdIn.Col = col.Set(this.InCol0).blend(this.InColE0, num).C;
						elecDrawer.draw(MdIn, fcnt, false);
						if (MdIn == MdOut)
						{
							MdIn.Identity();
						}
						MdOut.ColGrd.Set(this.OutCol1).blend(this.OutColE1, num);
						MdOut.Col = col.Set(this.OutCol0).blend(this.OutColE0, num).C;
						elecDrawer2.draw(MdOut, fcnt, false);
						MdIn.Identity();
						if (MdIn != MdOut)
						{
							MdOut.Identity();
						}
					}
				}
			}
			this.t += fcnt;
		}

		public bool need_fine_pos
		{
			get
			{
				return this.t >= this.interval || this.index < 0;
			}
		}

		private ElecDrawer[,] AItm;

		private float t;

		private int index = -1;

		private C32 InCol0;

		private C32 InCol1;

		private C32 OutCol0;

		private C32 OutCol1;

		private C32 InColE0;

		private C32 InColE1;

		private C32 OutColE0;

		private C32 OutColE1;

		private float cx;

		private float cy;

		public float divide_w = 0.703125f;

		public float jump_hs = 0.46875f;

		public float jump_hd = 0.46875f;

		public float jump_ratio = 0.3f;

		public float thicks = 0.234375f;

		public float thickd = 0.234375f;

		public float sball_r = 0.421875f;

		public float dball_r = 1.328125f;

		public float halo_rate = 2.5f;

		public float halo_rate_extend = 4f;

		public float halo_thick_rate = 0.2f;

		public float halo_thick_rate_extend = 0.04f;

		public int kaku = 5;

		public float trace_pos_mulwalk = 0.8f;

		public float min_minimize_level;

		public float out_scale = 2.5f;

		private float maxt = 20f;

		private float interval = 20f;
	}
}
