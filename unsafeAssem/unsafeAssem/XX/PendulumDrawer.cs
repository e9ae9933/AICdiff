using System;
using UnityEngine;

namespace XX
{
	public class PendulumDrawer
	{
		public PendulumDrawer recalc(bool force = false)
		{
			if (!this.need_recalc && !force)
			{
				return this;
			}
			this.need_recalc = false;
			float num = X.SINI((float)this.accept_t, (float)(this.intv_t * 2));
			this.hrhb_accept_agR = X.NI(0f, this.hrhb_agR, num);
			this.sheight = (1f - X.Sin(1.5707964f - this.hrhb_agR)) * this.gen_lgt;
			float num2 = X.Cos(1.5707964f - this.hrhb_agR) * this.gen_lgt * 2f;
			this.kaku_line = (int)(num2 / 8f);
			this.ignore_count = 0;
			this.t = 0f;
			return this;
		}

		public PendulumDrawer resetTime()
		{
			this.t = 0f;
			this.ignore_count = 0;
			this.syuuki = (this.accept_clicked = (int)(this.accept = 0));
			return this;
		}

		public PendulumDrawer resetTime(int delay_until_first_tap, bool auto_fix = false)
		{
			this.recalc(false);
			int num = this.intv_t - this.accept_t;
			if (auto_fix && (float)delay_until_first_tap < (float)num * 0.6f)
			{
				this.t = 0f;
			}
			else
			{
				this.t = (float)(num - delay_until_first_tap);
			}
			this.syuuki = (this.accept_clicked = (int)(this.accept = 0));
			return this;
		}

		public PendulumDrawer fixCenter()
		{
			if (this.isAccepting(true))
			{
				this.syuuki++;
			}
			this.t = (float)(this.intv_t * this.syuuki - this.accept_t);
			this.syuuki--;
			this.accept_clicked = this.syuuki;
			this.accept = (byte)(((int)this.accept & -4) | 2);
			this.accept |= 48;
			return this;
		}

		public bool run(float fcnt)
		{
			this.t += fcnt;
			if (this.t < 0f)
			{
				this.accept = 0;
				this.syuuki = -1;
				return false;
			}
			bool flag = false;
			int num = (int)(this.t / (float)this.intv_t);
			int num2 = (int)(this.t - (float)(this.intv_t * num));
			if ((this.accept & 12) == 0 && num2 >= this.intv_t / 2 - this.accept_t)
			{
				this.accept |= 4;
				flag = true;
			}
			if ((this.accept & 3) == 0 && num2 >= this.intv_t - this.accept_t * 2)
			{
				this.accept |= 1;
			}
			if ((this.accept & 48) == 0 && num2 >= this.intv_t - this.accept_t)
			{
				this.accept |= 16;
				flag = true;
				if (this.play_tap_sound_decline == 0)
				{
					SND.Ui.play(this.snd_tick, false);
				}
			}
			if (this.syuuki != num)
			{
				this.syuuki = num;
				bool flag2 = (this.accept & 3) == 1;
				this.accept = (flag2 ? 64 : 0);
				if (flag2)
				{
					this.ignore_count++;
				}
				if ((this.play_tap_sound_decline & 3) > 0)
				{
					this.play_tap_sound_decline -= 1;
				}
			}
			return flag;
		}

		public bool Tap()
		{
			if (this.accept_clicked < this.syuuki && (this.accept & 3) == 1)
			{
				this.executeTap();
				return true;
			}
			return false;
		}

		public bool Tap(out float tap_score, float t_maxscore_range, bool fix_if_max_score = true)
		{
			if (this.accept_clicked < this.syuuki && (this.accept & 3) == 1)
			{
				int num = (int)(this.t - (float)(this.intv_t * this.syuuki));
				t_maxscore_range *= 0.5f;
				float num2 = (float)(this.intv_t - this.accept_t);
				tap_score = 1f - X.ZLINE(X.Abs((float)num - num2) - t_maxscore_range, (float)this.accept_t - t_maxscore_range);
				this.executeTap();
				if (fix_if_max_score && tap_score == 1f)
				{
					this.t = (float)((this.syuuki + 1) * this.intv_t - this.accept_t);
				}
				return true;
			}
			tap_score = 0f;
			return false;
		}

		private void executeTap()
		{
			this.accept_clicked = this.syuuki;
			this.accept = (byte)((int)(this.accept | 2) & -65);
			this.play_tap_sound_decline = (byte)(((int)this.play_tap_sound_decline & -4) | 2);
			this.ignore_count = 0;
		}

		public void drawTo(MeshDrawer Md, float x, float y)
		{
			this.recalc(false);
			float num = X.SINI(this.t + (float)this.accept_t, (float)(this.intv_t * 2));
			float num2 = X.NI(-1.5707964f, -1.5707964f - this.hrhb_agR * (float)X.MPF(num > 0f), X.Abs(num));
			float num3 = X.Cos(num2);
			float num4 = X.Sin(num2);
			float num5 = -num4;
			float num6 = num3;
			float num7 = ((this.t < 0f) ? 0.5f : 1f);
			float num8 = y - this.sheight * 0.5f + this.gen_lgt;
			float num9 = x + num3 * this.gen_lgt;
			float num10 = num8 + num4 * this.gen_lgt;
			float num11 = num9 - num3 * this.draw_gen_lgt;
			float num12 = num10 - num4 * this.draw_gen_lgt;
			Md.Tri(0, 2, 1, false).Tri(0, 3, 2, false);
			Md.ColGrd.Set(this.ColGen).setA(0f);
			Md.Col = C32.MulA(this.ColGen, num7);
			Md.PosD(num9 - num5, num10 - num6, null).PosD(num9 + num5, num10 + num6, null).PosD(num11 + num5, num12 + num6, Md.ColGrd)
				.PosD(num11 - num5, num12 - num6, Md.ColGrd);
			bool flag = (this.accept & 3) > 0;
			float num13 = this.accept_area_thick + this.accept_area_r;
			Md.Col = C32.MulA(flag ? this.ColStoneBorderOn : this.ColStoneBorderOff, num7);
			Md.Circle(num9, num10, num13 + 1.5f, 0f, false, 0f, 0f);
			Md.Col = C32.MulA(flag ? this.ColStoneOn : this.ColStoneOff, num7);
			Md.Circle(num9, num10, num13, 0f, false, 0f, 0f);
			if (this.fnDrawStone != null)
			{
				this.fnDrawStone(this, Md, num9, num10, num2 + 1.5707964f, num2, true);
			}
			Md.Col = C32.MulA(flag ? this.ColAccpOn : this.ColAccpOff, num7);
			this.drawAcceptArea(Md, x, y, this.accept_area_r, this.accept_area_thick, 0f, 0f);
			if (this.draw_center_line)
			{
				Md.Rect(x, num8 - this.gen_lgt, this.accept_area_thick, this.accept_area_r * 2f, false);
			}
			if (this.fnDrawStone != null)
			{
				this.fnDrawStone(this, Md, num9, num10, num2 + 1.5707964f, num2, false);
			}
		}

		public void drawAcceptArea(MeshDrawer Md, float x, float y, float area_radius, float thick, float grd_level_in = 0f, float grd_level_out = 0f)
		{
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? Md.ColGrd : MTRX.cola.Set(Md.Col).blend(Md.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? Md.ColGrd : MTRX.colb.Set(Md.Col).blend(Md.ColGrd, grd_level_in));
			}
			float num = y - this.sheight * 0.5f + this.gen_lgt;
			if (thick <= 0f)
			{
				Md.PosD(x, num - this.gen_lgt - area_radius * 0.5f, c2);
				int num2 = this.kaku_rd * 2 + this.kaku_line * 2;
				for (int i = 0; i < num2; i++)
				{
					int num3 = i + 1;
					if (num3 == num2)
					{
						num3 = 0;
					}
					Md.Tri(i, num3, -1, false);
				}
			}
			else
			{
				int num4 = this.kaku_rd * 2 + this.kaku_line * 2;
				for (int j = 0; j < num4; j++)
				{
					int num5 = j + 1;
					if (num5 == num4)
					{
						num5 = 0;
					}
					Md.Tri(j, num4 + j, num4 + num5, false).Tri(j, num4 + num5, num5, false);
				}
			}
			for (int k = ((thick <= 0f) ? 1 : 0); k < 2; k++)
			{
				C32 c3 = ((k == 0) ? c2 : c);
				for (int l = 0; l < 4; l++)
				{
					int num6 = l / 2;
					float num7;
					float num9;
					float num10;
					int num11;
					float num12;
					float num13;
					if (l % 2 == 0)
					{
						num7 = (float)X.MPF(num6 == 1) * (1.5707964f + this.hrhb_accept_agR);
						float num8 = -1.5707964f + (float)X.MPF(num6 == 1) * this.hrhb_accept_agR;
						num9 = (x + this.gen_lgt * X.Cos(num8)) * 0.015625f;
						num10 = (num + this.gen_lgt * X.Sin(num8)) * 0.015625f;
						num11 = this.kaku_rd;
						num12 = -3.1415927f / (float)num11;
						num13 = (area_radius + (float)k * thick) * 0.015625f;
					}
					else
					{
						num9 = x * 0.015625f;
						num10 = num * 0.015625f - area_radius * (float)X.MPF(num6 == 1) * 0.015625f;
						num13 = (this.gen_lgt + (float)k * thick * (float)X.MPF(num6 == 1)) * 0.015625f;
						num7 = -1.5707964f + this.hrhb_accept_agR * (float)X.MPF(num6 == 1);
						num12 = this.hrhb_accept_agR * 2f / (float)this.kaku_line * (float)X.MPF(num6 == 0);
						num11 = this.kaku_line;
					}
					for (int m = 0; m < num11; m++)
					{
						Md.Pos(num9 + X.Cos(num7) * num13, num10 + X.Sin(num7) * num13, c3);
						num7 += num12;
					}
				}
			}
		}

		public int getSyuuki()
		{
			if (this.t >= 0f)
			{
				return this.syuuki;
			}
			return -1;
		}

		public bool isAccepting(bool alloc_already_tapped = false)
		{
			return (alloc_already_tapped && (this.accept & 3) == 3) || (this.accept & 3) == 1;
		}

		public bool decline_under_sound
		{
			get
			{
				return (this.play_tap_sound_decline & 4) > 0;
			}
			set
			{
				if (value)
				{
					this.play_tap_sound_decline |= 4;
					return;
				}
				this.play_tap_sound_decline = (byte)((int)this.play_tap_sound_decline & -5);
			}
		}

		public bool getTurned(bool break_flag = true)
		{
			if ((this.accept & 12) == 4)
			{
				if (break_flag)
				{
					this.accept |= 8;
				}
				return true;
			}
			return false;
		}

		public bool getUnderPass(bool break_flag = true)
		{
			if ((this.accept & 48) == 16)
			{
				if (break_flag)
				{
					this.accept |= 32;
				}
				return true;
			}
			return false;
		}

		public bool getIgnoredTap(bool break_flag = true)
		{
			if ((this.accept & 64) != 0)
			{
				if (break_flag)
				{
					this.accept = (byte)((int)this.accept & -65);
				}
				return true;
			}
			return false;
		}

		public float getUnderBeatFrame()
		{
			if (this.t < (float)(this.intv_t - this.accept_t))
			{
				return 0f;
			}
			return (this.t + (float)this.accept_t) % (float)this.intv_t;
		}

		public float gen_lgt = 270f;

		public float hrhb_agR = 0.62831855f;

		public int intv_t = 80;

		public int accept_t = 7;

		public float draw_gen_lgt = 60f;

		public float t;

		public float accept_area_r = 12f;

		public float accept_area_thick = 2.5f;

		public int kaku_rd = 6;

		public int ignore_count;

		public const string tick_snd_default = "pendulum_tick";

		public string snd_tick = "pendulum_tick";

		public PendulumDrawer.FnDrawStone fnDrawStone;

		public bool need_recalc = true;

		private float hrhb_accept_agR;

		private float sheight;

		public int kaku_line = 4;

		private byte play_tap_sound_decline;

		private int syuuki;

		private int accept_clicked;

		public Color32 ColGen = MTRX.ColWhite;

		public Color32 ColAccpOff = C32.d2c(4291480266U);

		public Color32 ColAccpOn = C32.d2c(4294049624U);

		public Color32 ColStoneOff = C32.d2c(4281874488U);

		public Color32 ColStoneOn = C32.d2c(4285098345U);

		public Color32 ColStoneBorderOff = C32.d2c(4287598479U);

		public Color32 ColStoneBorderOn = C32.d2c(4294049624U);

		public bool draw_center_line;

		public byte accept;

		private const byte BIT_UNDERPASS = 16;

		private const byte BIT_UNDERPASS_BROKEN = 32;

		private const byte BIT_IGNORED = 64;

		private const byte BIT_TURNBACK_BROKEN = 8;

		private const byte BIT_TURNBACK = 4;

		private const byte BIT_TAPPED = 2;

		private const byte BIT_INPUT_ENABLE = 1;

		private const byte BIT_INPUT_TAP = 3;

		public delegate void FnDrawStone(PendulumDrawer Pdr, MeshDrawer Md, float x, float y, float agR, float pd_agR, bool is_bottom);
	}
}
