using System;
using UnityEngine;

namespace XX
{
	public class HaloDrawer
	{
		public HaloDrawer(float _w = -1f, float _h = -1f, float _headalp = -1f, float _grayalp = -1f, float _grayratio_w = -1f, float _grayratio_h = -1f)
		{
			this.CCen = new C32(uint.MaxValue);
			this.CGrd = new C32(16777215);
			if (this.w > 0f)
			{
				this.Set(_w, _h, _headalp, _grayalp, _grayratio_w, _grayratio_h);
			}
		}

		public HaloDrawer Col(Color32 Col)
		{
			this.CCen.Set(Col);
			this.CGrd.Set(Col).setA(0f);
			return this;
		}

		public HaloDrawer setA1(float a01)
		{
			this.CCen.setA1(a01);
			return this;
		}

		public HaloDrawer mulA(float a01)
		{
			this.CCen.mulA(a01);
			return this;
		}

		public HaloDrawer Set(float _w, float _h, float _headalp = -1f, float _grayalp = -1f, float _grayratio_w = -1f, float _grayratio_h = -1f)
		{
			this.w = _w * 0.015625f;
			this.h = _h * 0.015625f;
			this.headalp = ((_headalp < 0f) ? this.headalp : _headalp);
			this.grayalp = ((_grayalp < 0f) ? this.grayalp : _grayalp);
			this.grayratio_w = ((_grayratio_w < 0f) ? this.grayratio_w : _grayratio_w);
			this.grayratio_h = ((_grayratio_h < 0f) ? this.grayratio_h : _grayratio_h);
			return this;
		}

		public HaloDrawer Dent(float _dent)
		{
			this.dent = _dent;
			return this;
		}

		private HaloDrawer Pos(float x, float y, C32 C = null)
		{
			this.CurMd.Pos(this.base_x + x * this.cos_rotR - y * this.sin_rotR, this.base_y + x * this.sin_rotR + y * this.cos_rotR, C);
			return this;
		}

		public void allocTriVer(MeshDrawer Md, int reverse_cross, int multiple = 1, int kaku = 6)
		{
			int num = kaku * 18 + 6 + 9;
			int num2 = 3 + 4 * (kaku + 1);
			if ((reverse_cross & 1) > 0)
			{
				int num3 = num * 2;
				int num4 = num2 * 2;
				num = num3;
				num2 = num4;
			}
			if ((reverse_cross & 2) > 0)
			{
				int num3 = num * 2;
				int num5 = num2 * 2;
				num = num3;
				num2 = num5;
			}
			Md.allocTri(num * multiple, 60).allocVer(num2 * multiple, 64);
		}

		public HaloDrawer drawTo(MeshDrawer Md, float x, float y, float agR, bool no_divide_ppu = false, int reverse_cross = 1)
		{
			if (Md == null)
			{
				return this;
			}
			if (!no_divide_ppu)
			{
				x /= 64f;
				y /= 64f;
			}
			bool flag = false;
			if (this.CurMd == null)
			{
				this.CurMd = Md;
				flag = true;
				this.base_x = x;
				this.base_y = y;
			}
			this.cos_rotR = X.Cos(agR);
			this.sin_rotR = X.Sin(agR);
			int num = 2 + X.Mn(X.IntC(this.h * 64f / 11f), 4);
			int num2 = X.Mx(1, X.Int((float)num / (1f + this.grayratio_h)));
			int num3 = 2 + num * 2 + 1;
			float num4 = (float)num / 2f;
			if (flag)
			{
				this.allocTriVer(Md, reverse_cross, 1, num);
			}
			for (int i = 0; i < num; i++)
			{
				int num5 = 2 + i * 2;
				int num6 = 3 + i * 2;
				int num7 = 4 + i * 2;
				int num8 = ((i == num - 1) ? num7 : (5 + i * 2));
				if (i < num2)
				{
					Md.Tri(num5).Tri(num7).Tri(0);
					Md.Tri(num8).Tri(num6).Tri(0);
				}
				else
				{
					Md.Tri(num5).Tri(num7).Tri(1);
					Md.Tri(num8).Tri(num6).Tri(1);
					if (i == num2)
					{
						Md.Tri(num5).Tri(1).Tri(0);
						Md.Tri(num6).Tri(0).Tri(1);
					}
				}
				int num9 = num3 + i * 2;
				int num10 = num9 + 1;
				int num11 = num9 + 2;
				int num12 = ((i == num - 1) ? num11 : (num11 - 1));
				Md.Tri(num9).Tri(num7).Tri(num5);
				Md.Tri(num9).Tri(num11).Tri(num7);
				Md.Tri(num10).Tri(num6).Tri(num8);
				Md.Tri(num10).Tri(num8).Tri(num12);
			}
			int num13 = num3 + num * 2 + 1;
			Md.Tri(num13).Tri(2).Tri(3);
			Md.Tri(num13).Tri(num3).Tri(2);
			Md.Tri(num13).Tri(3).Tri(num3 + 1);
			this.Pos(0f, 0f, null);
			C32 c = MeshDrawer.ColBuf0.Set(Md.Col).mulA(this.headalp);
			this.Pos(this.h, 0f, c);
			for (int j = 0; j < 2; j++)
			{
				float num14 = this.h * (1f + this.grayratio_h * ((j == 0) ? 1f : 1.4f));
				float num15 = ((this.grayratio_w == 0f) ? this.w : (this.w / ((j == 0) ? 1f : this.grayratio_w)));
				c = MeshDrawer.ColBuf0.Set(Md.Col).mulA((j == 0) ? this.grayalp : 0f);
				for (int k = 0; k <= num; k++)
				{
					float num16 = (float)k / (float)num;
					float num17 = X.NAIBUN_I(0f, 1f, num16);
					float num18 = X.NAIBUN_I(1f, 0f, num16);
					if (this.dent >= 1f)
					{
						num17 = X.NI(num17 * num17, num17 * num17 * num17, this.dent - 1f) * num14;
						num18 = X.NI(num18 * num18, num18 * num18 * num18, this.dent - 1f) * num15;
					}
					else if (this.dent > 0f)
					{
						num17 = X.NI(num17, num17 * num17, this.dent) * num14;
						num18 = X.NI(num18, num18 * num18, this.dent) * num15;
					}
					this.Pos(num17, num18, c);
					if (k != num)
					{
						this.Pos(num17, -num18, c);
					}
				}
			}
			this.Pos(-this.h * 0.3f, 0f, null);
			if ((reverse_cross & 1) > 0)
			{
				this.drawTo(Md, 0f, 0f, agR + 3.1415927f, true, 0);
			}
			if ((reverse_cross & 2) > 0)
			{
				this.drawTo(Md, 0f, 0f, agR + 1.5707964f, true, 1);
			}
			if (flag)
			{
				this.CurMd = null;
			}
			return this;
		}

		public float w = 0.125f;

		public float h = 0.625f;

		public float headalp = 0.65f;

		public float grayalp = 0.3f;

		public float grayratio_w = 0.5f;

		public float grayratio_h = 0.5f;

		public float dent = 0.3f;

		public C32 CCen;

		public C32 CGrd;

		private float base_x;

		private float base_y;

		private float cos_rotR = 1f;

		private float sin_rotR;

		private MeshDrawer CurMd;
	}
}
