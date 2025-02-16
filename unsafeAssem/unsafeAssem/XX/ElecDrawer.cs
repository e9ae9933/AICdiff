using System;
using UnityEngine;

namespace XX
{
	public class ElecDrawer
	{
		public ElecDrawer()
		{
			this.ran = X.Xors.get0();
		}

		public ElecDrawer Ran(uint _ran)
		{
			this.ran = _ran;
			return this;
		}

		public ElecDrawer DivideWidth(float _v)
		{
			this.divide_w = X.Mx(4f, _v) * 0.015625f;
			return this;
		}

		public ElecDrawer JumpHeight(float s, float d = -1000f)
		{
			this.jump_hs = s * 0.015625f;
			this.jump_hd = ((d == -1000f) ? this.jump_hs : (d * 0.015625f));
			return this;
		}

		public ElecDrawer JumpRatio(float _v)
		{
			this.jump_ratio = _v;
			return this;
		}

		public ElecDrawer BallRadius(float s, float d = -1000f)
		{
			this.sball_r = s * 0.015625f;
			this.dball_r = ((d == -1000f) ? this.sball_r : (d * 0.015625f));
			return this;
		}

		public ElecDrawer Thick(float s, float d = -1000f)
		{
			this.thicks = s * 0.015625f;
			this.thickd = ((d == -1000f) ? this.thicks : (d * 0.015625f));
			return this;
		}

		public ElecDrawer finePos(float _sx, float _sy, float _dx, float _dy)
		{
			this.sx = _sx * 0.015625f;
			this.sy = _sy * 0.015625f;
			this.dx = _dx * 0.015625f;
			this.dy = _dy * 0.015625f;
			this.RANNEXT();
			this.t = X.Mx(0f, this.t - this.refine_time);
			this.sdlen = X.LENGTHXY(this.sx, this.sy, this.dx, this.dy);
			this.count = X.IntR(this.sdlen / this.divide_w) + 1;
			if (this.count > 1)
			{
				this.baseAgR = X.GAR(this.sx, this.sy, this.dx, this.dy);
			}
			if (this.AVc == null)
			{
				this.AVc = new Vector3[this.count];
				this.Aran = new uint[this.count + 1];
			}
			else if (this.AVc.Length < this.count)
			{
				Array.Resize<Vector3>(ref this.AVc, this.count);
				Array.Resize<uint>(ref this.Aran, this.count + 1);
			}
			for (int i = 0; i < this.count; i++)
			{
				this.AVc[i].Set(0f, 0f, 0f);
			}
			float num = 0f;
			this.Aran[0] = this.RANNEXT();
			for (int j = 1; j < this.count; j++)
			{
				Vector3[] avc = this.AVc;
				this.Aran[j] = (this._ran = this.RANNEXT());
				float num2 = 0f;
				if (this.RAN(1787) < this.jump_ratio)
				{
					if (num != 0f && this.RAN(1422) < 0.77f)
					{
						num2 = num;
					}
					else if (num == 0f)
					{
						num2 = (float)X.MPF(this.RAN(1163) < 0.5f);
					}
					else
					{
						num2 = -num;
					}
					num = num2;
				}
				this.AVc[j].y = (this.AVc[j - 1].z = num2);
			}
			this.Aran[this.count] = this.RANNEXT();
			return this;
		}

		public bool need_fine_pos
		{
			get
			{
				return this.t >= this.refine_time || this.t < 0f;
			}
		}

		private uint RANNEXT()
		{
			return this.ran = X.GETRAN2((int)(this.ran % 1024U), (int)(4U + this.ran % 111U));
		}

		private float RAN(int f)
		{
			return (float)((ulong)this._ran % (ulong)((long)f)) / (float)f;
		}

		private float RANS(int f, float v = 1f)
		{
			return ((float)((ulong)this._ran % (ulong)((long)f)) / (float)f - 0.5f) * 2f * v;
		}

		private ElecDrawer Tri(int i, int i2, int i3)
		{
			this.Md.Tri(i).Tri(i2).Tri(i3);
			return this;
		}

		private ElecDrawer Pos(float x, float y, bool outs = false)
		{
			this.Md.Pos(x, y, outs ? this.Md.ColGrd : null);
			this.poscnt++;
			return this;
		}

		private void pvReset()
		{
			this.pv0 = 0;
			this.pv1 = 1;
			this.pv2 = 2;
			this.poscnt = 0;
		}

		private void PosCnt()
		{
			this.pv0 -= this.poscnt;
			this.pv1 -= this.poscnt;
			this.pv2 -= this.poscnt;
			this.poscnt = 0;
		}

		public void drawTz(MeshDrawer _Md, float extend, bool identity = true)
		{
			this.Md = _Md;
			this.Md.Rotate(this.baseAgR, false).Translate(this.sx, this.sy, false);
			this.poscnt = 0;
			float num = X.ZSIN(1f - extend);
			float num2 = 1f + X.ZSIN(extend) * 0.4f;
			float num3 = X.Mn(this.divide_w, this.sdlen);
			if (this.count == 1)
			{
				this.drawBall(-2f, X.Mx(this.sball_r, this.dball_r), false, extend);
			}
			else
			{
				float num4 = 0f;
				float num5 = 0f;
				for (int i = 1; i < this.count; i++)
				{
					float num6 = X.ZPOW((float)i / (float)(this.count - 1));
					float num7 = X.NI(this.thicks, this.thickd, num6) * num;
					float y = this.AVc[i].y;
					float z = this.AVc[i].z;
					if (i == 1)
					{
						this.drawBall(y, this.sball_r, false, extend);
					}
					this._ran = this.Aran[i];
					float num8 = X.NI(this.jump_hs, this.jump_hd, num6) * (1f + X.ZSIN(extend) * (0.36f + this.RAN(2418) * 0.6f));
					if (y == 0f)
					{
						float num9 = 0f;
						if (num4 == 0f)
						{
							num9 = ((i == 0) ? 0.4f : 0.1f) + this.RAN(930) * 0.3f;
							this.Tri(this.pv1, 1, this.pv0).Tri(this.pv0, 1, 0).Tri(this.pv0, 0, this.pv2)
								.Tri(this.pv2, 0, 2);
						}
						else
						{
							this.Tri(this.pv1, this.pv0, 2).Tri(this.pv0, 0, 2).Tri(this.pv0, this.pv2, 0)
								.Tri(this.pv2, 1, 0);
						}
						this.pvReset();
						float num10 = num5 + (0.1f + this.RANS(2059, 0.18f) * num2 + num9) * num3;
						float num11 = num10 + num4 * (num7 * (0.35f + this.RANS(1070, 0.09f)));
						float num12 = num10 - num4 * (num7 * (0.35f + this.RANS(2822, 0.09f)));
						float num13 = num7 * this.RANS(921, 0.3f);
						float num14 = num13 + num7 * (0.48f + this.RANS(2726, 0.1f));
						float num15 = num13 - num7 * (0.48f + this.RANS(660, 0.1f));
						this.Pos(num10, num13, false).Pos(num11, num14, true).Pos(num12, num15, true);
						this.PosCnt();
						float num16 = 0f;
						if (i == this.count - 1)
						{
							num16 = -0.4f - this.RAN(1123) * 0.2f;
						}
						this.Tri(this.pv1, 1, this.pv0).Tri(this.pv0, 1, 0).Tri(this.pv0, 0, this.pv2)
							.Tri(this.pv2, 0, 2);
						this.pvReset();
						float num17 = ((float)i + this.RANS(1587, 0.4f)) * num3 + num3 * (-0.1f + 0.24f * this.RAN(2658) * num2 + num16);
						float num18 = num17 - z * (num7 * (0.35f + this.RANS(1115, 0.09f)));
						float num19 = num17 + z * (num7 * (0.35f + this.RANS(1988, 0.09f)));
						float num20 = num7 * this.RANS(1651, 0.3f);
						float num21 = num20 + num7 * (0.48f + this.RANS(1268, 0.1f));
						float num22 = num20 - num7 * (0.48f + this.RANS(1743, 0.1f));
						this.Pos(num17, num20, false).Pos(num18, num21, true).Pos(num19, num22, true);
						this.PosCnt();
						num5 = num17;
					}
					else
					{
						float num23 = y * num8 * (0.2f + this.RAN(1069) * 1.4f);
						float num24 = (float)((y == num4) ? 0 : X.MPF(num4 < y));
						float num25 = 0f;
						if (num4 == 0f)
						{
							this.Tri(this.pv1, 1, this.pv0).Tri(this.pv0, 1, 0).Tri(this.pv0, 0, this.pv2)
								.Tri(this.pv2, 0, 2);
						}
						else
						{
							this.Tri(this.pv1, this.pv0, 2).Tri(this.pv0, 0, 2).Tri(this.pv0, this.pv2, 0)
								.Tri(this.pv2, 1, 0);
							if (num4 == y)
							{
								num25 = 0.1f + this.RAN(930) * 0.3f;
							}
						}
						this.pvReset();
						float num26 = num5 + (-0.06f + this.RAN(1723) * 0.23f * num2 + num25) * num3;
						float num27 = num26 - num24 * (num7 * (0.35f + this.RANS(806, 0.09f)));
						float num28 = num26 + num24 * (num7 * (0.35f + this.RANS(567, 0.09f)));
						float num29 = num23;
						float num30 = num29 + num7 * (0.41f + this.RANS(1077, 0.17f));
						float num31 = num29 - num7 * (0.41f + this.RANS(1884, 0.17f));
						this.Pos(num26, num29, false).Pos(num27, num30, true).Pos(num28, num31, true);
						this.PosCnt();
						float num32 = (float)((y == z) ? 0 : X.MPF(y < z));
						this.Tri(this.pv1, 2, this.pv0).Tri(this.pv0, 2, 0).Tri(this.pv0, 0, this.pv2)
							.Tri(this.pv2, 0, 1);
						this.pvReset();
						float num33 = ((float)i - 0.07f + this.RAN(2973) * 0.19f * num2) * num3 + num3 * (-0.15f + 0.34f * this.RAN(727) * num2);
						float num34 = num33 + num32 * (num7 * (0.35f + this.RANS(455, 0.09f)));
						float num35 = num33 - num32 * (num7 * (0.35f + this.RANS(1438, 0.09f)));
						float num36 = y * num8 * (0.2f + this.RAN(2220) * 1.4f);
						float num37 = num36 - num7 * (0.41f + this.RANS(2182, 0.17f));
						float num38 = num36 + num7 * (0.41f + this.RANS(2065, 0.17f));
						this.Pos(num33, num36, false).Pos(num34, num37, true).Pos(num35, num38, true);
						this.PosCnt();
						num5 = num33;
					}
					num4 = y;
				}
				this.drawBall((num4 == 0f) ? 2f : num4, this.dball_r, true, extend);
			}
			if (identity)
			{
				this.Md.Translate(-this.sx, -this.sy, false).Rotate(-this.baseAgR, false);
			}
		}

		public void draw(MeshDrawer _Md, float fcnt = 1f, bool identity = true)
		{
			float num = X.ZLINE(this.t, this.refine_time);
			this.drawTz(_Md, num, identity);
			this.t += fcnt;
		}

		private void drawBall(float connect_y, float radius, bool pos_d, float extend)
		{
			this._ran = (pos_d ? this.Aran[this.count] : this.Aran[0]);
			float num = this.RAN(1288);
			uint num2 = 0U;
			int num3 = ((connect_y == -2f) ? this.kaku : (this.kaku - 1));
			if (num < this.ball_halo_2_ratio)
			{
				int num4 = (int)(this.RAN(540) * (float)num3);
				num2 |= 1U << num4;
				num4 = (num4 + (int)(this.RAN(1298) * (float)(num3 - 1))) % num3;
				num2 |= 1U << num4;
			}
			else if (num < this.ball_halo_1_ratio)
			{
				int num5 = (int)(this.RAN(540) * (float)num3);
				num2 |= 1U << num5;
			}
			float num6;
			if (connect_y != -2f)
			{
				num2 = (num2 << 1) | 1U;
				num6 = 1.5707964f * connect_y + this.RANS(988, 0.25132743f);
			}
			else
			{
				num6 = 6.2831855f * this.RAN(710);
			}
			float num7 = 6.2831855f / (float)this.kaku;
			int num8 = 1;
			int num9 = 1;
			for (int i = 0; i < this.kaku; i++)
			{
				int num10 = num8;
				bool flag = ((i == 0) ? (((ulong)num2 & (ulong)(1L << ((this.kaku - 1) & 31))) > 0UL) : (((ulong)num2 & (ulong)(1L << ((i - 1) & 31))) > 0UL));
				bool flag2 = ((ulong)num2 & (ulong)(1L << (i & 31))) > 0UL;
				int num11;
				if (flag2)
				{
					if (i == 0 && connect_y != -2f)
					{
						if (pos_d)
						{
							if (connect_y == 2f)
							{
								this.Tri(this.pv1, num8 + 2, this.pv0).Tri(this.pv0, num8 + 2, num8).Tri(this.pv0, num8, this.pv2)
									.Tri(this.pv2, num8, num8 + 1);
							}
							else
							{
								this.Tri(this.pv1, this.pv0, num8 + 1).Tri(this.pv0, num8, num8 + 1).Tri(this.pv0, this.pv2, num8)
									.Tri(this.pv2, num8 + 2, num8);
							}
						}
						this.Tri(0, num8 + 1, num8).Tri(0, num8, num8 + 2);
						num11 = num8 + 2;
						num8 += 3;
					}
					else
					{
						this.Tri(0, num8 + 2, num8 + 1).Tri(num8 + 1, num8 + 2, num8).Tri(0, num8 + 1, num8 + 3)
							.Tri(num8 + 1, num8, num8 + 3);
						num11 = num8 + 3;
						num8 += 4;
					}
				}
				else
				{
					num11 = num8;
					num8++;
				}
				if (i == 0)
				{
					num9 = num11;
				}
				else if (flag)
				{
					this.Tri(0, num11, num10 - 2);
				}
				else
				{
					this.Tri(0, num11, num10 - 1);
				}
				if (i == this.kaku - 1)
				{
					this.Tri(0, num9, flag2 ? (num11 - 1) : num10);
				}
			}
			float num12 = (pos_d ? this.sdlen : 0f);
			float num13 = (float)(pos_d ? 0 : 0);
			float num14 = X.NI(this.halo_thick_rate, this.halo_thick_rate_extend, extend);
			this.Pos(num12, num13, false);
			this.pvReset();
			float num15 = radius * (1f - X.ZSIN(extend));
			for (int j = 0; j < this.kaku; j++)
			{
				float num16 = num6 + num7;
				bool flag3 = ((ulong)num2 & (ulong)(1L << (j & 31))) > 0UL;
				float num17 = num15 * (1f + this.RANS(498 + j * 347, 0.1f));
				float num18 = X.Cos(num6);
				float num19 = X.Sin(num6);
				if (flag3)
				{
					if (j == 0 && connect_y != -2f)
					{
						this.Pos(num12 + num17 * num18, num13 + num17 * num19, false);
					}
					else
					{
						float num20 = X.NI(this.halo_rate, this.halo_rate_extend, extend) * num17;
						this.Pos(num12 + num20 * num18, num13 + num20 * num19, true);
						this.Pos(num12 + num17 * 0.8f * num18, num13 + num17 * 0.8f * num19, false);
					}
					float num21 = num6 - num7;
					float num22 = num15 * (1f + this.RANS(498 + (j - 1 + this.kaku) % this.kaku * 347, 0.1f));
					float num23 = num15 * (1f + this.RANS(498 + (j + 1) % this.kaku * 347, 0.1f));
					this.Pos(num12 + X.NI(num17 * num18, num23 * X.Cos(num16), 0.8f), num13 + X.NI(num17 * num19, num23 * X.Sin(num16), num14), true);
					this.Pos(num12 + X.NI(num17 * num18, num22 * X.Cos(num21), 0.8f), num13 + X.NI(num17 * num19, num22 * X.Sin(num21), num14), true);
				}
				else
				{
					this.Pos(num12 + num17 * num18, num13 + num17 * num19, true);
				}
				num6 = num16;
			}
			this.PosCnt();
		}

		public uint ran;

		public uint[] Aran;

		public float divide_w = 0.703125f;

		public float jump_hs = 0.46875f;

		public float jump_hd = 0.46875f;

		public float jump_ratio = 0.3f;

		public float thicks = 0.234375f;

		public float thickd = 0.234375f;

		public float refine_time = 20f;

		private float sx;

		private float sy;

		private float dx;

		private float dy;

		private float baseAgR;

		private int count;

		private float sdlen;

		public float sball_r = 0.359375f;

		public float dball_r = 0.703125f;

		public float halo_rate = 2.5f;

		public float halo_rate_extend = 4f;

		public float halo_thick_rate = 0.2f;

		public float halo_thick_rate_extend = 0.04f;

		public int kaku = 5;

		public float ball_halo_2_ratio = 0.22f;

		public float ball_halo_1_ratio = 0.6f;

		private Vector3[] AVc;

		public float t = -1f;

		private MeshDrawer Md;

		private int pv0;

		private int pv1;

		private int pv2;

		private int poscnt;

		private uint _ran;
	}
}
