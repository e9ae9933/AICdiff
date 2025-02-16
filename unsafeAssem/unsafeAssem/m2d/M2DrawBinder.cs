using System;
using XX;

namespace m2d
{
	public class M2DrawBinder : IRunAndDestroy
	{
		public M2DrawBinder(M2DrawBinderContainer _Con, uint _index)
		{
			this.Con = _Con;
			this.index = _index;
		}

		public M2Camera Cam
		{
			get
			{
				return this.Con.Mp.M2D.Cam;
			}
		}

		public M2DrawBinder Set(string _name, M2DrawBinder.FnEffectBind _fnDraw, float saf = 0f)
		{
			this.fnDraw = _fnDraw;
			this.name = _name;
			this.t = -saf;
			this.f0 = IN.totalframe;
			return this;
		}

		public bool run(float fcnt)
		{
			if (this.fnDraw == null)
			{
				return false;
			}
			if (this.t < 0f)
			{
				this.t = X.Mn(this.t + fcnt, 0f);
				return true;
			}
			this.t += fcnt;
			return true;
		}

		public void efDraw(EffectItem Ef)
		{
			if (this.t >= 0f && this.fnDraw != null)
			{
				string text = Bench.P(this.name);
				Ef.af = this.t;
				Ef.f0 = this.f0;
				Ef.index = this.index;
				try
				{
					if (!this.fnDraw(Ef, this))
					{
						this.destruct();
					}
				}
				catch (Exception ex)
				{
					X.de(ex.ToString(), null);
					this.destruct();
				}
				Bench.Pend(text);
			}
		}

		public bool isinCamera(EffectItem Ef, float map_extend_w, float map_extend_h)
		{
			return this.isinCamera(Ef.x, Ef.y, map_extend_w, map_extend_h);
		}

		public bool isinCameraP(EffectItem Ef, float pixel_extend_w, float pixel_extend_h)
		{
			float num = ((this.Con == this.Mp.EFD) ? this.Mp.rCLENB : this.Mp.rCLEN);
			return this.isinCamera(Ef.x, Ef.y, pixel_extend_w * num, pixel_extend_h * num);
		}

		public bool isinCamera(float mpx, float mpy, float extend_w, float extend_h)
		{
			M2SubMap subMapData = this.Con.SubMapData;
			if (subMapData != null)
			{
				return subMapData.isinCamera(mpx - extend_w, mpy - extend_h, mpx + extend_w, mpy + extend_h, 0f);
			}
			return this.Cam.isCoveringMp(mpx - extend_w, mpy - extend_h, mpx + extend_w, mpy + extend_h, 0f);
		}

		public Map2d Mp
		{
			get
			{
				return this.Con.Mp;
			}
		}

		public void destruct()
		{
			this.fnDraw = null;
		}

		public readonly M2DrawBinderContainer Con;

		public readonly uint index;

		public float t;

		public string name;

		public int f0;

		public M2DrawBinder.FnEffectBind fnDraw;

		public delegate bool FnEffectBind(EffectItem Ef, M2DrawBinder Ed);
	}
}
