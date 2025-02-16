using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX
{
	public class BorderDrawer<T> : BorderVectorListener
	{
		public BorderDrawer(Func<int, int, T[,], bool> fnIsExist, Func<float, float, BDVector> _FnCreateDefault = null)
			: base(_FnCreateDefault)
		{
			this.FnIsExist = fnIsExist;
		}

		public BorderDrawer<T> checkOutPath(T[,] Ab, bool no_clear_width_and_height = false)
		{
			int num = 0;
			int num2 = 0;
			if (!no_clear_width_and_height)
			{
				this.myleft = (this.mytop = 0);
				this.width = (uint)Ab.GetLength(0);
				this.height = (uint)Ab.GetLength(1);
			}
			int num3 = this.myleft;
			int num4 = this.mytop;
			this.clearPoints();
			int length = Ab.Length;
			uint num5 = 0U;
			BDic<uint, uint> obufUU = BorderDrawer<T>.ObufUU;
			BDic<uint, uint> obufUUData = BorderDrawer<T>.ObufUUData;
			BDic<uint, uint> obufUUData2 = BorderDrawer<T>.ObufUUData;
			obufUU.Clear();
			obufUUData.Clear();
			obufUUData2.Clear();
			uint num6 = 0U;
			uint num7 = this.width;
			int num8 = this.mytop;
			int num9 = ((this.height == 1U) ? 3 : 1);
			uint num10 = (uint)((ulong)(this.height - 1U) + (ulong)((long)this.mytop));
			uint num11 = (this.FnIsExist(this.myleft, this.mytop, Ab) ? 2U : 0U);
			int num12 = this.myleft;
			for (;;)
			{
				num11 = num11 | (((num9 & 1) > 0) ? 0U : (this.FnIsExist(num12, num8 - 1, Ab) ? 16U : 0U)) | (((num9 & 2) > 0) ? 0U : (this.FnIsExist(num12, num8 + 1, Ab) ? 8U : 0U)) | (((num7 -= 1U) != 0U) ? (this.FnIsExist(++num12, num8, Ab) ? 1U : 0U) : 0U);
				if ((num11 & 2U) > 0U)
				{
					num5 += 1U;
					uint num13 = (num7 << 13) | (uint)num8;
					if (num11 != 31U)
					{
						obufUU[num13] = num11;
						obufUUData2[num13] = num11;
						num6 += 1U;
					}
					obufUUData[num13] = num11;
				}
				if (num7 == 0U)
				{
					num7 = this.width;
					num12 = this.myleft;
					if ((long)(++num8) > (long)((ulong)num10))
					{
						break;
					}
					num9 = (((long)num8 >= (long)((ulong)num10)) ? 2 : 0);
					num11 = (this.FnIsExist(num12, num8, Ab) ? 2U : 0U);
				}
				else
				{
					num11 = (num11 << 1) & 6U;
				}
			}
			this.all_pixels = (int)num6;
			this.full_selected = this.all_pixels >= Ab.Length;
			this.chk_x = 0U;
			this.chk_y = 0U;
			this.chk_w = this.width;
			this.chk_h = this.height;
			bool flag = true;
			if (num6 != 0U)
			{
				while (num6 != 0U)
				{
					using (BList<uint> blist = X.objKeysB<uint, uint>(obufUU))
					{
						int i = blist.Count - 1;
						while (i >= 0)
						{
							uint num14 = blist[i];
							num11 = obufUU[num14];
							num8 = (int)(num14 & 8191U);
							int num15 = (int)((num14 >> 13) & 8191U);
							int num16 = num15;
							int num17 = num8;
							int num18;
							if ((num11 & 1U) == 0U)
							{
								num18 = 3;
							}
							else if ((num11 & 8U) == 0U)
							{
								num18 = 0;
							}
							else if ((num11 & 4U) == 0U)
							{
								num18 = 1;
							}
							else
							{
								if ((num11 & 16U) != 0U)
								{
									obufUU.Remove(num14);
									num6 -= 1U;
									i--;
									continue;
								}
								num18 = 2;
							}
							int num19 = num18;
							AIM aim = CAim.get_clockwise2((AIM)num18, true);
							uint num20 = this.Aaim2bit[num18];
							uint num21 = this.Aaim2bit[(int)aim];
							int num22 = CAim._XD(num18, 1);
							int num23 = -CAim._YD(num18, 1);
							this.first_point = 1;
							uint num24 = X.GetS<uint, uint>(obufUUData, num14, 0U);
							for (;;)
							{
								bool flag2 = num15 == num16 && num8 == num17 && this.first_point == 0;
								if (num11 < 31U)
								{
									num11 |= num21;
									if (num11 == 31U)
									{
										obufUU.Remove(num14);
										num6 -= 1U;
									}
									else
									{
										BDic<uint, uint> bdic = obufUU;
										uint num25 = num14;
										bdic[num25] |= num21;
									}
								}
								if (flag2 && num18 == num19)
								{
									break;
								}
								if ((num24 & num21) != 0U)
								{
									int num26 = ((num18 == 0 || num18 == 3) ? 1 : 0);
									int num27 = ((num18 == 0 || num18 == 1) ? 1 : 0);
									int num28 = (int)((long)num3 + (long)((ulong)this.width) - 1L - (long)num15 + (long)num26);
									int num29 = num8 + num27;
									this.AddVpOutPos((float)num28, (float)num29, num18, this.first_point | (num26 << 1) | (num27 << 2) | 8);
									this.first_point = 0;
									num18 = (int)CAim.get_clockwise2((AIM)num18, true);
									if (flag2 && num18 == num19)
									{
										break;
									}
									num20 = num21;
									aim = CAim.get_clockwise2((AIM)num18, true);
									num21 = this.Aaim2bit[(int)aim];
									num22 = CAim._XD(num18, 1);
									num23 = -CAim._YD(num18, 1);
									if (flag)
									{
										num = (this.myleft = num28);
										num2 = (this.mytop = num29);
										flag = false;
									}
									else
									{
										this.myleft = X.Mn(num28, this.myleft);
										this.mytop = X.Mn(num29, this.mytop);
										num = X.Mx(num28, num);
										num2 = X.Mx(num29, num2);
									}
								}
								else if ((num24 & num20) == 0U)
								{
									int num26 = ((num18 == 2 || num18 == 3) ? 1 : 0);
									int num27 = ((num18 == 0 || num18 == 3) ? 1 : 0);
									int num28 = (int)((long)num3 + (long)((ulong)this.width) - 1L - (long)num15 + (long)num26);
									int num29 = num8 + num27;
									this.AddVpOutPos((float)num28, (float)num29, num18, this.first_point | (num26 << 1) | (num27 << 2));
									this.first_point = 0;
									num21 = num20;
									num18 = (int)CAim.get_clockwise2((AIM)num18, false);
									aim = CAim.get_clockwise2((AIM)num18, true);
									num20 = this.Aaim2bit[num18];
									num22 = CAim._XD(num18, 1);
									num23 = -CAim._YD(num18, 1);
									if (flag)
									{
										num = (this.myleft = num28);
										num2 = (this.mytop = num29);
										flag = false;
										continue;
									}
									this.myleft = X.Mn(num28, this.myleft);
									this.mytop = X.Mn(num29, this.mytop);
									num = X.Mx(num28, num);
									num2 = X.Mx(num29, num2);
									continue;
								}
								num15 -= num22;
								num8 += num23;
								num14 = (uint)((num15 << 13) | num8);
								num24 = X.GetS<uint, uint>(obufUUData, num14, 0U);
								num11 = X.GetS<uint, uint>(obufUU, num14, num24 | 64U);
							}
							break;
						}
					}
				}
				this.width = (uint)(num - this.myleft);
				this.height = (uint)(num2 - this.mytop);
			}
			else
			{
				this.width = (this.height = 0U);
			}
			return this;
		}

		public int Length
		{
			get
			{
				if (this.VpOut != null)
				{
					return this.corner_max;
				}
				return 0;
			}
		}

		public BDCorner Get(int i)
		{
			return this.VpOut[i];
		}

		public BorderDrawer<T> clearPoints()
		{
			this.corner_max = 0;
			if (this.VpOut == null)
			{
				this.VpOut = new BDCorner[64];
			}
			return this;
		}

		public BorderDrawer<T> setRect(int x, int y, int w, int h)
		{
			this.clearPoints();
			this.myleft = x;
			this.mytop = y;
			this.width = (uint)w;
			this.height = (uint)h;
			this.AddVpOutPos((float)x, (float)y, 1, 1);
			this.AddVpOutPos((float)(x + w), (float)y, 2, 2);
			this.AddVpOutPos((float)(x + w), (float)(y + h), 3, 6);
			this.AddVpOutPos((float)x, (float)(y + h), 0, 4);
			return this;
		}

		private BDCorner AddVpOutPos(float vpx, float vpy, int a, int vpz)
		{
			BDCorner bdcorner;
			if (this.corner_max > 0 && (vpz & 1) == 0)
			{
				bdcorner = this.VpOut[this.corner_max - 1];
				if (vpx == bdcorner.x && vpy == bdcorner.y)
				{
					this.corner_max--;
				}
			}
			if (this.corner_max >= this.VpOut.Length)
			{
				Array.Resize<BDCorner>(ref this.VpOut, this.VpOut.Length + 64);
			}
			bdcorner = this.VpOut[this.corner_max];
			if (bdcorner == null)
			{
				bdcorner = (this.VpOut[this.corner_max] = new BDCorner(vpx, vpy, a, vpz));
			}
			else
			{
				bdcorner.Set(vpx, vpy, a, vpz);
			}
			this.corner_max++;
			return bdcorner;
		}

		public void makeCollider(PolygonCollider2D Cld, FnZoom fnCalcX, FnZoom fnCalcY, BorderDrawer<T>.FnCalcSlopePos fnSlope = null)
		{
			this.makePathes(new BorderVectorListener.PositionListenerPolygonCollider(Cld), fnCalcX, fnCalcY, fnSlope);
		}

		public void makePathes(BorderVectorListener.IPositionListener Cld, FnZoom fnCalcX, FnZoom fnCalcY, BorderDrawer<T>.FnCalcSlopePos fnSlope = null)
		{
			int num = 0;
			int num2 = 1;
			int[] array = new int[8];
			if (this.corner_max <= 0)
			{
				if (Cld != null)
				{
					Cld.setPathCount(0);
				}
				return;
			}
			for (int i = 1; i < this.corner_max; i++)
			{
				if ((this.VpOut[i].z & 1) > 0)
				{
					X.pushToEmptyS<int>(ref array, num2, ref num, 16);
					num2 = 0;
				}
				num2++;
			}
			X.pushToEmptyS<int>(ref array, num2, ref num, 16);
			if (Cld != null)
			{
				Cld.setPathCount(num);
			}
			num2 = 0;
			int num3 = 0;
			bool flag = true;
			bool flag2 = false;
			int num4 = array[num2];
			this.APosBuffer = this.APosBuffer ?? new List<BDVector>(num4);
			if (this.APosBuffer.Capacity < num4)
			{
				this.APosBuffer.Capacity = num4;
			}
			this.APosBuffer.Clear();
			if (fnSlope != null)
			{
				this.ASlopePosBuffer = this.ASlopePosBuffer ?? new List<BDVector>(2);
				this.ASlopePosBuffer.Clear();
			}
			for (int j = 0; j <= this.corner_max; j++)
			{
				BDCorner bdcorner = ((j == this.corner_max) ? null : this.VpOut[j]);
				if (j == this.corner_max || ((bdcorner.z & 1) > 0 && j > 0))
				{
					if (flag2)
					{
						BDVector.SlicePoints<BDVector>(this.APosBuffer, true);
					}
					if (Cld != null)
					{
						Cld.SetPath(num2, this.APosBuffer, fnCalcX, fnCalcY, this.APosBuffer.Count);
					}
					if (j == this.corner_max)
					{
						break;
					}
					num4 = array[++num2];
					this.APosBuffer.Clear();
					num3 = 0;
					flag = true;
					flag2 = false;
				}
				if (fnSlope != null)
				{
					this.defineCornerPos(bdcorner, j, num4, fnSlope, num3, ref flag, ref flag2);
				}
				else
				{
					this.APosBuffer.Add(this.FnCreateDefault(bdcorner.x, bdcorner.y));
				}
				num3++;
			}
		}

		private void defineCornerPos(BDCorner C, int vpout_index, int cur_ccnt, BorderDrawer<T>.FnCalcSlopePos fnSlope, int posi, ref bool first_pos, ref bool slice_points)
		{
			BDCorner bdcorner = this.VpOut[vpout_index - posi + (posi + 1) % cur_ccnt];
			int z = C.z;
			int z2 = C.z;
			int num = X.IntR(C.x) - ((bdcorner.aim == 0 || bdcorner.aim == 3) ? 1 : 0);
			int num2 = X.IntR(C.y) - ((bdcorner.aim == 0 || bdcorner.aim == 1) ? 1 : 0);
			List<BDVector> list = fnSlope(this.ASlopePosBuffer, num, num2, C, bdcorner);
			if (list == null)
			{
				this.APosBuffer.Add(this.FnCreateDefault(C.x, C.y));
			}
			else
			{
				slice_points = true;
				this.APosBuffer.AddRange(list);
			}
			this.ASlopePosBuffer.Clear();
			posi++;
		}

		public void executePos(BorderDrawer<T>.FnBorderExecuter fnExc)
		{
			for (int i = 0; i < this.corner_max; i++)
			{
				BDCorner bdcorner = this.VpOut[i];
				fnExc(bdcorner);
			}
		}

		public int myleft;

		public int mytop;

		public uint width;

		public uint height;

		private uint chk_x;

		private uint chk_y;

		private uint chk_w;

		private uint chk_h;

		private int all_pixels;

		public bool full_selected;

		private const uint BD_L = 4U;

		private const uint BD_T = 16U;

		private const uint BD_R = 1U;

		private const uint BD_C = 2U;

		private const uint BD_B = 8U;

		public readonly uint[] Aaim2bit = new uint[] { 4U, 16U, 1U, 8U };

		private int corner_max;

		private int first_point;

		private BDCorner[] VpOut;

		public Func<int, int, T[,], bool> FnIsExist;

		private static BDic<uint, uint> ObufUU = new BDic<uint, uint>();

		private static BDic<uint, uint> ObufUUData = new BDic<uint, uint>();

		private static BDic<uint, uint> ObufUUBorder = new BDic<uint, uint>();

		private List<BDVector> APosBuffer;

		private List<BDVector> ASlopePosBuffer;

		public delegate void FnBorderExecuter(BDCorner C);

		public delegate List<BDVector> FnCalcSlopePos(List<BDVector> ABuffer, int bx, int by, BDCorner C, BDCorner NextC);
	}
}
