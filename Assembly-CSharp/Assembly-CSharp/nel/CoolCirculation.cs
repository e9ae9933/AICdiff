using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class CoolCirculation
	{
		public CoolCirculation()
		{
			this.ACci = new List<CoolCirculation.CCI>(4);
		}

		public void remake(int count)
		{
			this.ACci.Clear();
			int num = 1;
			CoolCirculation.CCI cci = null;
			int num2 = 0;
			CoolCirculation.small_gen = 0;
			while (this.ACci.Count < count)
			{
				CoolCirculation.CCI cci2 = new CoolCirculation.CCI();
				float num3 = X.XORSP();
				if (num3 < 0.125f)
				{
					cci2.shape = CoolCirculation.CSHAPE.STAR;
				}
				else if (num3 < 0.2f)
				{
					cci2.shape = CoolCirculation.CSHAPE.DOT;
				}
				else
				{
					cci2.shape = (CoolCirculation.CSHAPE)X.xors(3);
				}
				cci2.sedai = ((cci != null) ? (cci.sedai + 1) : 0);
				cci2.fill = X.XORSP() < ((cci2.sedai > 0) ? 0.38f : 0.14f);
				cci2.sp_kaku = ((X.XORSP() < 0.2f) ? 3 : ((2 + X.xors(3)) * 2));
				cci2.sp_r = X.NIXP(this.min_sp_radius, this.max_sp_radius);
				cci2.draw_scale = ((X.XORSP() < 0.25f) ? 0.25f : 1f);
				num3 = X.XORSP();
				if (num3 < 100f)
				{
					cci2.pos = CoolCirculation.CPOS.CENTER;
				}
				else if (num3 < 0.6f)
				{
					cci2.pos = CoolCirculation.CPOS.FOCUS_LR;
				}
				else
				{
					cci2.pos = CoolCirculation.CPOS.CENTER;
				}
				cci2.ps_lgt = X.NIXP(this.min_ps_length, this.max_ps_length);
				bool flag = false;
				if (cci != null)
				{
					int num4 = cci.APos.Length;
				}
				if (cci == null)
				{
					if (X.XORSP() < 0.8f)
					{
						cci2.APos = new Vector3[]
						{
							new Vector3(0f, 0f, 1.5707964f)
						};
					}
					else
					{
						cci2.APos = this.CirclationSetting((X.XORSP() < 0.2f) ? 3 : ((2 + X.xors(3)) * 2), 0, null);
						cci2.sp_r /= X.NIXP(1.5f, 3f);
						flag = true;
					}
				}
				else
				{
					cci2.APos = cci.ListUpConnectPos();
					if (X.XORSP() < 0.125f)
					{
						cci2.APos = this.CirclationSetting((X.XORSP() < 0.2f) ? 3 : ((2 + X.xors(3)) * 2), cci.sedai, cci2.APos);
						cci2.sp_r /= X.NIXP(3.5f, 6f);
						flag = true;
					}
					else
					{
						cci2.sp_r /= X.NIXP(2.5f, 5f);
						flag = true;
					}
				}
				int num5 = cci2.APos.Length;
				if (flag)
				{
					cci2.sedai++;
					if (cci2.draw_scale >= 0.85f)
					{
						CoolCirculation.CSHAPE shape = cci2.shape;
						if (shape == CoolCirculation.CSHAPE.RADIATION || shape == CoolCirculation.CSHAPE.STAR)
						{
							cci2.ps_kaku = 1;
						}
					}
				}
				if (num * num5 >= 240 || (cci2.shape == CoolCirculation.CSHAPE.DOT && num5 >= 12))
				{
					if (++num2 >= 3)
					{
						cci = null;
						num = 1;
						num2 = 0;
					}
				}
				else
				{
					num *= num5;
					this.ACci.Add(cci2);
					if (cci == null && cci2.pos == CoolCirculation.CPOS.CENTER && cci2.shape == CoolCirculation.CSHAPE.CIRCLE)
					{
						cci2.fill = false;
					}
					if (num >= 120)
					{
						cci = null;
						num = 1;
						num2 = 0;
					}
					else if (cci2.pos != CoolCirculation.CPOS.FOCUS_LR)
					{
						if (X.XORSP() < ((cci2.sedai > 0) ? (0.66f / (float)(cci2.sedai + 1)) : 0.88f))
						{
							cci = cci2;
						}
						else if (cci != null && X.XORSP() < 0.27f)
						{
							cci = null;
							num = 1;
							num2 = 0;
						}
					}
				}
			}
		}

		private Vector3[] CirclationSetting(int cnt, int sedai, Vector3[] APos_Base = null)
		{
			Vector3[] array = new Vector3[cnt];
			bool flag = X.XORSP() < 0.3f;
			float num = X.NIXP(this.min_sp_radius, this.max_sp_radius) * 0.4f;
			if ((CoolCirculation.small_gen & (1 << sedai)) == 0 && X.XORSP() < 0.3f)
			{
				num *= 0.25f;
				CoolCirculation.small_gen |= 1 << sedai;
			}
			for (int i = 0; i < cnt; i++)
			{
				float num2 = 1.5707964f - 6.2831855f * (float)i / (float)cnt;
				array[i] = new Vector3(num * X.Cos(num2), num * X.Sin(num2), num2 + (flag ? 0f : 3.1415927f));
			}
			if (APos_Base != null)
			{
				Vector3[] array2 = new Vector3[cnt * APos_Base.Length];
				int num3 = APos_Base.Length;
				int num4 = 0;
				for (int j = 0; j < cnt; j++)
				{
					Vector3 vector = array[j];
					for (int k = 0; k < num3; k++)
					{
						array2[num4++] = vector + APos_Base[k];
					}
				}
				return array2;
			}
			return array;
		}

		public void drawTo(MeshDrawer Md)
		{
			int count = this.ACci.Count;
			for (int i = 0; i < count; i++)
			{
				CoolCirculation.CCI cci = this.ACci[i];
				int num = cci.APos.Length;
				float num2 = cci.sp_r * cci.draw_scale;
				float num3 = num2 * this.draw_ratio;
				for (int j = 0; j < 2; j++)
				{
					Md.Col = ((j == 0) ? MTRX.ColBlack : MTRX.ColWhite);
					for (int k = 0; k < num; k++)
					{
						Vector3 vector = cci.APos[k];
						if (j == 0)
						{
							if (cci.fill)
							{
								if (cci.shape == CoolCirculation.CSHAPE.CIRCLE)
								{
									Md.Circle(vector.x, vector.y, num3, 0f, false, 0f, 0f);
								}
								else if (cci.shape == CoolCirculation.CSHAPE.POLY)
								{
									Md.Poly(vector.x, vector.y, num3, vector.z, cci.sp_kaku, 0f, false, 0f, 0f);
								}
								else if (cci.shape == CoolCirculation.CSHAPE.STAR)
								{
									float num4 = 6.2831855f / (float)cci.sp_kaku;
									float num5 = num3 / 2f;
									for (int l = 0; l < cci.sp_kaku; l++)
									{
										float num6 = vector.z + num4 * (float)l;
										Md.Poly(vector.x + num5 * X.Cos(num6), vector.y + num5 * X.Sin(num6), num5, num6, 4, 0f, false, 0f, 0f);
									}
								}
							}
						}
						else
						{
							Md.Col = MTRX.ColWhite;
							if (cci.shape == CoolCirculation.CSHAPE.CIRCLE)
							{
								if (this.draw_ratio >= 1f)
								{
									Md.Circle(vector.x, vector.y, num2, 2f, false, 0f, 0f);
								}
								else
								{
									Md.Arc(vector.x, vector.y, num2, vector.z + 1.5707964f - 1.5707964f * this.draw_ratio, vector.z + 1.5707964f + 1.5707964f * this.draw_ratio, 2f);
									Md.Arc(vector.x, vector.y, num2, vector.z - 1.5707964f - 1.5707964f * this.draw_ratio, vector.z - 1.5707964f + 1.5707964f * this.draw_ratio, 2f);
								}
							}
							else if (cci.shape == CoolCirculation.CSHAPE.POLY)
							{
								Md.Poly(vector.x, vector.y, num3, vector.z, cci.sp_kaku, 2f, false, 0f, 0f);
							}
							else if (cci.shape == CoolCirculation.CSHAPE.RADIATION)
							{
								float num7 = 6.2831855f / (float)cci.sp_kaku;
								float num8 = num3;
								for (int m = 0; m < cci.sp_kaku; m++)
								{
									float num9 = vector.z + num7 * (float)m;
									Md.Line(vector.x, vector.y, vector.x + X.Cos(num9) * num8, vector.y + X.Sin(num9) * num8, 2f, false, 0f, 0f);
								}
							}
							else if (cci.shape == CoolCirculation.CSHAPE.STAR)
							{
								float num10 = 6.2831855f / (float)cci.sp_kaku;
								float num11 = num3 / 2f;
								for (int n = 0; n < cci.sp_kaku; n++)
								{
									float num12 = vector.z + num10 * (float)n;
									Md.Poly(vector.x + num11 * X.Cos(num12), vector.y + num11 * X.Sin(num12), num11, num12, 4, 2f, false, 0f, 0f);
								}
							}
							else if (cci.shape == CoolCirculation.CSHAPE.DOT)
							{
								Md.Circle(vector.x, vector.y, this.dot_radius * cci.draw_scale * this.draw_ratio, 0f, false, 0f, 0f);
							}
						}
					}
				}
			}
		}

		private List<CoolCirculation.CCI> ACci;

		public float min_sp_radius = 120f;

		public float max_sp_radius = 150f;

		public float min_ps_length = 60f;

		public float max_ps_length = 280f;

		public float dot_radius = 8f;

		public float draw_ratio = 1f;

		private static int small_gen;

		private enum CSHAPE
		{
			CIRCLE,
			RADIATION,
			POLY,
			DOT,
			STAR
		}

		private enum CPOS
		{
			CENTER,
			CIRCULATION,
			FOCUS_LR
		}

		private class CCI
		{
			public Vector3[] ListUpConnectPos()
			{
				if (this.pos == CoolCirculation.CPOS.FOCUS_LR)
				{
					return new Vector3[]
					{
						new Vector3(0f, 0f, 1.5707964f)
					};
				}
				int num = this.APos.Length;
				Vector3[] array = new Vector3[this.sp_kaku * num];
				int num2 = 0;
				bool flag = X.XORSP() < 0.3f;
				float num3 = 1f;
				if (X.XORSP() < 0.2f)
				{
					num3 = X.NIXP(1.15f, 1.3f);
				}
				for (int i = 0; i < num; i++)
				{
					Vector3 vector = this.APos[i];
					for (int j = 0; j < this.sp_kaku; j++)
					{
						Vector3 vector2 = default(Vector3);
						float num4 = -6.2831855f * (float)j / (float)this.sp_kaku;
						vector2.x += this.sp_r * X.Cos(num4) * num3;
						vector2.y += this.sp_r * X.Sin(num4) * num3;
						vector2.z += num4 + (flag ? 0f : 3.1415927f);
						array[num2++] = vector2 + vector;
					}
				}
				return array;
			}

			public CoolCirculation.CSHAPE shape;

			public CoolCirculation.CPOS pos;

			public bool fill = true;

			public int sp_kaku;

			public int ps_kaku;

			public float draw_scale = 1f;

			public float sp_r;

			public float ps_lgt;

			public Vector3[] APos;

			public int sedai;
		}
	}
}
