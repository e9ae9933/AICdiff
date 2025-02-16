using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiGOContinuer : IRunAndDestroy, IValotileSetable
	{
		public UiGOContinuer(M2DBase _M2D = null, bool draw_gl_only = false)
		{
			this.Gob = IN.CreateGobGUI(null, "-GoContinue");
			this.Md = MeshDrawer.prepareMeshRenderer(this.Gob, MTRX.MIicon.getMtr(-1), 0f, -1, null, true, draw_gl_only);
			this.Valot = this.Gob.GetComponent<ValotileRenderer>();
			this.M2D = _M2D;
			if (this.M2D != null)
			{
				this.M2D.addValotAddition(this);
			}
		}

		public void destruct()
		{
			if (this.M2D != null)
			{
				this.M2D.remValotAddition(this);
				IN.DestroyE(this.Gob);
			}
		}

		public void recalcSpeed()
		{
			this.recalcSpeed(CFGSP.gameover_counter);
		}

		public void recalcSpeed(byte _maxtype)
		{
			this.maxtime = _maxtype;
			this.speed_level = X.ZLINE((float)(this.maxtime - 1), 20f);
		}

		public void restartCount()
		{
			this.t_count = 0f;
			this.t = 0f;
			this.t_alp = 0f;
		}

		public bool run(float fcnt)
		{
			float num;
			if (this.t_count >= 0f)
			{
				num = fcnt * X.NI(1f, X.NI(1f, 0.188f, this.speed_level), X.ZSIN(this.t_count, 10f)) * 0.016666668f;
				int num2 = (int)this.t_count;
				this.t_count += num;
				int num3 = (int)this.t_count;
				if (num2 != num3)
				{
					this.need_redraw = true;
					this.t_alp = 0f;
					if (this.t_count >= 10f)
					{
						this.t_count = -1f;
						if (this.t >= 0f)
						{
							this.t = 0f;
						}
					}
					else if (this.countdown_snd != null)
					{
						SND.Ui.play(this.countdown_snd, false);
					}
				}
			}
			else
			{
				num = fcnt;
			}
			if (this.t_alp < 30f)
			{
				this.need_redraw = true;
				this.t_alp += fcnt;
			}
			this.need_redraw = true;
			if (this.t >= 0f)
			{
				this.need_redraw = true;
				this.t += fcnt;
			}
			else
			{
				this.t -= num;
				if (this.t <= -90f)
				{
					this.Md.clear(false, false);
					return true;
				}
			}
			if (X.D && this.need_redraw)
			{
				this.need_redraw = false;
				this.draw();
				this.Md.updateForMeshRenderer(false);
			}
			return true;
		}

		private void draw()
		{
			float num = ((this.t >= 0f) ? X.ZLINE(this.t, 90f) : X.ZLINE(90f + this.t, 90f));
			num *= this.base_alpha;
			BMListChars chrL = MTRX.ChrL;
			using (STB stb = TX.PopBld(null, 0))
			{
				float num2 = 0f;
				uint num3 = uint.MaxValue;
				if (this.t_count >= 0f)
				{
					if (this.t_count > 0f)
					{
						this.Md.Col = C32.WMulA(num * X.NIL(0.25f, 1f, this.t_alp, 30f));
						stb.Clear().Add((int)(10f - this.t_count));
						chrL.DrawScaleStringTo(this.Md, stb, 4f * X.COSI(this.t + 90f, 280f), 4f * X.COSI(this.t + 10f, 331f) + IN.h * 0.18f, 5f, 5f * (X.ZSIN3(this.t_alp, 15f) * 1.125f - X.ZCOS(this.t_alp - 15f, 15f) * 0.125f), ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
					}
					stb.Set("CONTINUE?");
					num *= 0.875f + 0.125f * X.COSI(this.t, 190f);
					if (X.ANMT(2, 20f) == 0)
					{
						num3 = 4290361785U;
					}
				}
				else
				{
					if (this.t > 55f)
					{
						num2 += X.ZSIN(this.t - 55f, 50f) * X.absMn(X.COSIT(200f), 0.74f) * 9f;
					}
					stb.Set("GAME OVER …");
				}
				this.Md.Col = C32.MulA(num3, num);
				num2 += ((this.t >= 0f) ? (-1f + X.ZSIN(this.t, 90f)) : 0f) * IN.h * 0.06f;
				chrL.DrawScaleStringTo(this.Md, stb, 0f, IN.h * 0.28f + num2, 3f, 3f, ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
			}
		}

		public void activate()
		{
			if (this.t < 0f)
			{
				this.Gob.SetActive(true);
				this.t = 0f;
			}
		}

		public void deactivate(bool immediate)
		{
			if (immediate)
			{
				this.t = -90f;
				this.Gob.SetActive(false);
				return;
			}
			if (this.t >= 0f)
			{
				this.t = -1f;
			}
		}

		public float base_alpha
		{
			get
			{
				return this.base_alpha_;
			}
			set
			{
				if (this.base_alpha == value)
				{
					return;
				}
				this.base_alpha_ = value;
				this.need_redraw = true;
			}
		}

		public float progress_level
		{
			get
			{
				if (this.t_count < 0f)
				{
					return 1f;
				}
				return X.ZLINE(this.t_count, 10f);
			}
		}

		public override string ToString()
		{
			return "UiGOContinuer";
		}

		public bool use_valotile
		{
			get
			{
				return this.Valot.enabled;
			}
			set
			{
				this.Valot.enabled = value;
			}
		}

		public bool isTimerOvered()
		{
			return this.t_count < 0f;
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public Transform transform
		{
			get
			{
				return this.Gob.transform;
			}
		}

		public bool prepared
		{
			get
			{
				return this.speed_level >= 0f;
			}
		}

		private float t;

		private float t_count;

		private float t_alp = 100f;

		private const float FADE_T = 90f;

		private const float maxt = 10f;

		private byte maxtime;

		private float speed_level = -1f;

		private bool need_redraw = true;

		private readonly M2DBase M2D;

		public readonly GameObject Gob;

		public readonly MeshDrawer Md;

		public readonly ValotileRenderer Valot;

		private float base_alpha_;

		public string countdown_snd;
	}
}
