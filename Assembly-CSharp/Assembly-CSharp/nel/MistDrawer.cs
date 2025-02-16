using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public sealed class MistDrawer
	{
		public MistDrawer(MistManager.MistKind _K)
		{
			this.K = _K;
			this.Rc = new DRect("");
			this.APoint = new List<MistDrawer.MstPt>();
			this.ADrawPoint = new MistDrawer.MstPtToDraw[16];
			this.FD_fnDraw = new M2DrawBinder.FnEffectBind(this.fnDraw);
			if (MistDrawer.RcBuf == null)
			{
				MistDrawer.RcBuf = new DRect("Buf");
			}
		}

		public void releaseEffect()
		{
			if (this.Ed != null)
			{
				this.draw_pt_max = 0;
				this.APoint.Clear();
				this.Ed = null;
			}
		}

		public MistDrawer destruct()
		{
			this.releaseEffect();
			this.APoint.Clear();
			this.need_fine_reposit = false;
			return null;
		}

		public void addPoint(Map2d _Mp, int mapx, int mapy, int level255, int wind_value = 32639)
		{
			this.Rc.Expand((float)mapx, (float)mapy, 1f, 1f, false);
			List<MistDrawer.MstPt> apoint = this.APoint;
			int num = this.mstpt_index;
			this.mstpt_index = num + 1;
			apoint.Add(new MistDrawer.MstPt(mapx, mapy, num, level255 >= this.K.apply_level255, wind_value));
			this.need_fine_reposit = true;
			if (this.Ed != null && this.Ed.Mp != _Mp)
			{
				this.releaseEffect();
			}
			if (this.Ed == null)
			{
				this.Ed = _Mp.setED("MistDraw", this.FD_fnDraw, 0f);
				this.Mp = _Mp;
			}
			this.fast_thresh = this.Mp.floort;
			this.pre_reposit_t = X.Mn(this.Mp.floort + 20f, this.pre_reposit_t);
		}

		public void fineWind(Map2d _Mp, int mapx, int mapy, int wind_value)
		{
			try
			{
				int num = 0;
				for (;;)
				{
					int num2 = this.indexOf(mapx, mapy, num);
					if (num2 < 0 || num2 < num)
					{
						break;
					}
					this.APoint[num2].wind_value = wind_value;
					if (this.need_fine_reposit)
					{
						break;
					}
					num = num2 + 1;
				}
			}
			catch
			{
			}
		}

		public void remPoint(int mapx, int mapy)
		{
			try
			{
				int num = 0;
				for (;;)
				{
					int num2 = this.indexOf(mapx, mapy, num);
					if (num2 < 0)
					{
						break;
					}
					this.APoint.RemoveAt(num2);
					num = num2;
				}
			}
			catch
			{
			}
		}

		public bool fnDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.Ed != null && this.Ed != Ed)
			{
				return false;
			}
			if (this.need_set_alpha == 2)
			{
				this.need_set_alpha = 0;
			}
			bool flag = false;
			if (this.pre_reposit_t < 0f || this.Mp.floort - this.pre_reposit_t > 0f)
			{
				if (this.need_fine_reposit)
				{
					this.APoint.Sort((MistDrawer.MstPt a, MistDrawer.MstPt b) => MistDrawer.sortMstPt(a, b));
					this.need_fine_reposit = false;
				}
				if (this.need_set_alpha == 0)
				{
					this.need_set_alpha = 1;
				}
				int count = this.APoint.Count;
				this.pre_reposit_t = this.Mp.floort + 40f;
				if (count > 0)
				{
					byte b2 = (byte)(2 << ((this.byte2 ? 2 : 0) & 31));
					for (int i = 0; i < count; i++)
					{
						MistDrawer.MstPt mstPt = this.APoint[i];
						mstPt.s2 = (byte)((int)mstPt.s2 & ~(3 << ((this.byte2 ? 0 : 2) & 31)));
						if (((mstPt.s2 >> ((this.byte2 ? 2 : 0) & 31)) & 3) == 2)
						{
							mstPt.a = 0f;
						}
						else
						{
							mstPt.a = X.NIXP(0.6f, 0.8f);
							bool flag2 = false;
							int num;
							int num2;
							int num3;
							if ((num = this.indexOf(mstPt.x + 1, mstPt.y, i + 1)) >= 0 && (num2 = this.indexOf(mstPt.x, mstPt.y + 1, i + 1)) >= 0 && (num3 = this.indexOf(mstPt.x + 1, mstPt.y + 1, i + 1)) >= 0)
							{
								mstPt.s2 = (byte)((int)mstPt.s2 | (1 << ((this.byte2 ? 2 : 0) & 31)));
								MistDrawer.MstPt mstPt2 = this.APoint[num];
								mstPt2.s2 |= b2;
								MistDrawer.MstPt mstPt3 = this.APoint[num2];
								mstPt3.s2 |= b2;
								MistDrawer.MstPt mstPt4 = this.APoint[num3];
								mstPt4.s2 |= b2;
								flag2 = true;
							}
							if (this.draw_pt_max < this.ADrawPoint.Length && this.ADrawPoint[this.draw_pt_max] != null)
							{
								MistDrawer.MstPtToDraw[] adrawPoint = this.ADrawPoint;
								int num4 = this.draw_pt_max;
								this.draw_pt_max = num4 + 1;
								adrawPoint[num4].Set(mstPt, flag2, this.Mp.floort);
							}
							else
							{
								X.pushToEmptyS<MistDrawer.MstPtToDraw>(ref this.ADrawPoint, new MistDrawer.MstPtToDraw(mstPt, flag2, this.Mp.floort), ref this.draw_pt_max, 16);
							}
						}
					}
					this.byte2 = !this.byte2;
				}
				flag = true;
			}
			bool flag3 = !Ed.isinCamera(this.Rc.cx, this.Rc.cy, this.Rc.width * 0.5f + 2f, this.Rc.height * 0.5f + 2f);
			if (flag3 && !flag)
			{
				return true;
			}
			bool flag4 = false;
			MeshDrawer meshDrawer = null;
			int j = 0;
			int num5 = this.draw_pt_max;
			int num6 = 0;
			while (j < num5)
			{
				int num7 = j - num6;
				MistDrawer.MstPtToDraw mstPtToDraw = ((num6 == 0) ? this.ADrawPoint[j] : this.ADrawPoint[num7].Set(this.ADrawPoint[j]));
				if (meshDrawer == null)
				{
					meshDrawer = Ef.GetMesh("", MTR.MIiconL.getMtr(BLEND.NORMAL, -1), true);
				}
				if (!this.drawOneSmoke(meshDrawer, mstPtToDraw, flag3))
				{
					num6++;
				}
				else
				{
					flag4 = true;
				}
				j++;
			}
			if (num6 > 0)
			{
				this.draw_pt_max -= num6;
			}
			if (!flag4 && Ed == this.Ed)
			{
				this.Ed = null;
			}
			return flag4;
		}

		private bool drawOneSmoke(MeshDrawer Md, MistDrawer.MstPtToDraw Pt, bool cam_out)
		{
			uint ran = X.GETRAN2(Pt.i + (int)Pt.f0 * 13, Pt.i % 7);
			float num = this.Mp.floort - Pt.f0 - 20f * X.RAN(ran, 1561);
			if (Pt.f0 < this.fast_thresh)
			{
				num *= 2f;
			}
			if (num <= 0f)
			{
				return true;
			}
			if (num >= 70f)
			{
				return false;
			}
			bool is_big = Pt.is_big;
			Pt.a = X.VALWALK(Pt.a, Pt.da, 0.038f * (float)X.AF_EF * Map2d.TSbase);
			if (cam_out || Pt.a == 0f)
			{
				return true;
			}
			float num2 = X.ZLINE(num, 70f);
			Md.Col = Md.ColGrd.Set(this.K.color0).blend(this.K.color1, X.RAN(ran, 2940)).mulA(Pt.a * X.ZLINE(2f - 2f * num2))
				.C;
			float num3 = (float)(is_big ? 2 : 1);
			float num4 = 0f;
			float num5 = 0f;
			if (Pt.wind_value != 32639)
			{
				int num6;
				if ((num6 = Pt.wind_x) != 0)
				{
					Pt.wind_apply_level_x += (float)X.AF_EF * Map2d.TSbase * (float)X.MPF(num6 > 0);
				}
				if ((num6 = Pt.wind_y) != 0)
				{
					Pt.wind_apply_level_y += (float)X.AF_EF * Map2d.TSbase * (float)X.MPF(num6 > 0);
				}
			}
			if (Pt.wind_apply_level_x != 0f)
			{
				num4 = Pt.wind_apply_level_x * 0.04f;
			}
			if (Pt.wind_apply_level_y != 0f)
			{
				num5 = Pt.wind_apply_level_y * 0.04f;
			}
			Md.base_x = this.Mp.ux2effectScreenx(this.Mp.map2ux((float)Pt.x + 0.5f + num4));
			Md.base_y = this.Mp.uy2effectScreeny(this.Mp.map2uy((float)Pt.y + 0.5f + num5));
			Md.Scale(num3, num3, false);
			float num7 = 6.2831855f * X.RAN(ran, 961);
			float num8 = this.Mp.CLENB * X.NI(0.4f, 1f, X.RAN(ran, 710)) * num2;
			float num9 = X.NI(0.7f, 1f, X.RAN(ran, 2285));
			Md.RotaPF((-0.25f + 0.5f * X.RAN(ran, 749)) * this.Mp.CLENB + num8 * X.Cos(num7), (-0.25f + 0.5f * X.RAN(ran, 939)) * this.Mp.CLENB + num8 * X.Sin(num7), num9, num9, 6.2831855f * X.RAN(ran, 1892) - X.NI(-0.13f, 0.13f, X.RAN(ran, 984)) * 3.1415927f * num2, MTR.AEfSmokeL[(int)((float)MTR.AEfSmokeL.Length * X.RAN(ran, 1867))], false, false, false, uint.MaxValue, false, 0);
			Md.Identity();
			return true;
		}

		private int indexOf(int x, int y, int si = 0)
		{
			if (this.need_fine_reposit)
			{
				return this.APoint.FindIndex((MistDrawer.MstPt V) => V.x == x && V.y == y);
			}
			int count = this.APoint.Count;
			for (int i = si; i < count; i++)
			{
				MistDrawer.MstPt mstPt = this.APoint[i];
				if (mstPt.y > y)
				{
					return -1;
				}
				if (mstPt.y == y)
				{
					if (mstPt.x == x)
					{
						return i;
					}
					if (mstPt.x > x)
					{
						return -1;
					}
				}
			}
			return -1;
		}

		private static int sortMstPt(MistDrawer.MstPt Pa, MistDrawer.MstPt Pb)
		{
			if (Pa.y == Pb.y)
			{
				if (Pa.x < Pb.x)
				{
					return -1;
				}
				if (Pa.x != Pb.x)
				{
					return 1;
				}
				return 0;
			}
			else
			{
				if (Pa.y >= Pb.y)
				{
					return 1;
				}
				return -1;
			}
		}

		public void fineAlpha(int x, int y, int level255)
		{
			if (this.need_set_alpha == 0)
			{
				return;
			}
			if (this.need_set_alpha == 1)
			{
				this.need_set_alpha = 2;
			}
			this.fineAlpha(this.indexOf(x, y, 0), level255);
		}

		public void fineAlpha(int i, int level255)
		{
			if (i < 0)
			{
				return;
			}
			this.APoint[i].active = level255 >= this.K.apply_level255;
		}

		public readonly MistManager.MistKind K;

		private Map2d Mp;

		private DRect Rc;

		private static DRect RcBuf;

		public bool need_fine_reposit;

		public byte need_set_alpha;

		public M2DrawBinder Ed;

		private int mstpt_index;

		private float pre_reposit_t = -1f;

		private bool byte2;

		private List<MistDrawer.MstPt> APoint;

		private MistDrawer.MstPtToDraw[] ADrawPoint;

		private int draw_pt_max;

		public float fast_thresh;

		private M2DrawBinder.FnEffectBind FD_fnDraw;

		private sealed class MstPt
		{
			public MstPt(int _x, int _y, int _i, bool _active, int _wind_value = 32639)
			{
				this.x = _x;
				this.y = _y;
				this.i = _i;
				this.active = _active;
				this.wind_value = _wind_value;
			}

			public int x;

			public int y;

			public byte s2;

			public int i;

			public int wind_value;

			public bool active = true;

			public float a = 1f;
		}

		private sealed class MstPtToDraw
		{
			public MstPtToDraw(MistDrawer.MstPt P, bool _is_big, float _f0)
			{
				this.Set(P, _is_big, _f0);
			}

			public MistDrawer.MstPtToDraw Set(MistDrawer.MstPt P, bool _is_big, float _f0)
			{
				this.Src = P;
				this.wind_apply_level_x = (this.wind_apply_level_y = 0f);
				this.is_big = _is_big;
				this.f0 = _f0;
				return this;
			}

			public MistDrawer.MstPtToDraw Set(MistDrawer.MstPtToDraw P)
			{
				this.Src = P.Src;
				this.wind_apply_level_x = P.wind_apply_level_x;
				this.wind_apply_level_y = P.wind_apply_level_y;
				this.is_big = P.is_big;
				this.f0 = P.f0;
				return this;
			}

			public int wind_value
			{
				get
				{
					return this.Src.wind_value;
				}
			}

			public int x
			{
				get
				{
					return this.Src.x;
				}
			}

			public int y
			{
				get
				{
					return this.Src.y;
				}
			}

			public int i
			{
				get
				{
					return this.Src.i;
				}
			}

			public float da
			{
				get
				{
					return this.Src.a * (this.Src.active ? 1f : (this.Src.a * 0.5f));
				}
			}

			public int wind_x
			{
				get
				{
					return (this.Src.wind_value & 65280) >> 9;
				}
				set
				{
					this.Src.wind_value = (this.Src.wind_value & 255) | (X.MMX(0, value + 127, 255) << 8);
				}
			}

			public int wind_y
			{
				get
				{
					return (this.Src.wind_value & 255) - 127;
				}
				set
				{
					this.Src.wind_value = (this.Src.wind_value & 65280) | X.MMX(0, value + 127, 255);
				}
			}

			public bool is_big;

			public float a;

			public MistDrawer.MstPt Src;

			public float f0;

			public float wind_apply_level_x;

			public float wind_apply_level_y;
		}
	}
}
