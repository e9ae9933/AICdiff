using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Better;
using UnityEngine;

namespace XX
{
	public static class X
	{
		public static bool SENSITIVE
		{
			get
			{
				return X.sensitive_level > 0;
			}
		}

		public static bool SP_SENSITIVE
		{
			get
			{
				return X.sensitive_level > 1;
			}
		}

		public static void init1()
		{
			X.ppu = 64f;
			X.OKey2Text = new BDic<string, string>();
			X.ScreenLockInit();
		}

		public static DebugLogBlockBase dli(string tx, DebugLogBlockBase Blk = null)
		{
			return X.dl(tx, Blk, false, true);
		}

		public static DebugLogBlockBase dl(string tx, DebugLogBlockBase Blk = null, bool is_error = false, bool is_important = false)
		{
			if (!X.DEBUGANNOUNCE && !is_important && !is_error)
			{
				return null;
			}
			if (is_error)
			{
				Logger.de(tx, true, null);
			}
			else
			{
				Logger.dl(tx);
			}
			return Blk;
		}

		public static DebugLogBlockBase de(string tx, DebugLogBlockBase Blk = null)
		{
			Logger.de(tx, true, null);
			return Blk;
		}

		public static DebugLogBlockBase de(string tx, string v, DebugLogBlockBase Blk = null)
		{
			return Blk;
		}

		public static void loadDebug()
		{
			string text = NKT.readStreamingText("_debug.txt", false);
			if (TX.noe(text))
			{
				return;
			}
			CsvReader csvReader = new CsvReader(text, CsvReader.RegSpace, false);
			bool flag = true;
			while (csvReader.read())
			{
				if (flag)
				{
					if (csvReader.cmd == "<DEBUG>")
					{
						if (csvReader._B1)
						{
							X.DEBUG = true;
						}
						flag = false;
					}
				}
				else
				{
					if (!X.DEBUG)
					{
						break;
					}
					if (csvReader.cmd == "nosnd")
					{
						if (!TX.valid(SND.isSndErrorOccured(false)))
						{
							X.DEBUGNOSND = csvReader.Nm(1, 0f) != 0f;
						}
					}
					else
					{
						try
						{
							FieldInfo fieldInfo = typeof(X).GetField("DEBUG" + csvReader.cmd.ToUpper());
							if (fieldInfo != null)
							{
								fieldInfo.SetValue(null, csvReader.Nm(1, 0f) != 0f);
							}
							else
							{
								fieldInfo = typeof(X).GetField(csvReader.cmd.ToUpper());
								if (fieldInfo != null)
								{
									fieldInfo.SetValue(null, csvReader.Nm(1, 0f) != 0f);
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			Logger.fineTimeStampViewer();
		}

		public static void setSpriteAlpha(SpriteRenderer Spr, float value01 = 1f)
		{
			Color color = Spr.color;
			color.a = value01;
			Spr.color = color;
		}

		public static ulong NowTime()
		{
			return (ulong)(DateTime.Now - X.TimeEpoch).TotalSeconds;
		}

		public static float RAN(uint ran, int f)
		{
			return (ran & 16777215U) % (uint)(f & 65535) / (float)f;
		}

		public static float RANS(uint ran, int f)
		{
			return (X.RAN(ran, f) - 0.5f) * 2f;
		}

		public static float RANSH(uint ran, int f)
		{
			return X.RAN(ran, f) - 0.5f;
		}

		public static bool RANB(uint ran, int f, float thresh = 0.5f)
		{
			return (ran & 16777215U) % (uint)(f & 65535) < thresh * (float)f;
		}

		public static float Pow2(float a)
		{
			return a * a;
		}

		public static Vector3 RANBORDER(float w, float h, float ran01)
		{
			ran01 = X.frac2(ran01);
			float num = (w + h) * 2f * ran01;
			if (num < w)
			{
				return new Vector3(-0.5f * w + num, -0.5f * h, 3f);
			}
			num -= w;
			if (num < h)
			{
				return new Vector3(0.5f * w, -0.5f * h + num, 2f);
			}
			num -= h;
			if (num < w)
			{
				return new Vector3(0.5f * w - num, 0.5f * h, 1f);
			}
			num -= w;
			return new Vector3(-0.5f * w, 0.5f * h - num, 0f);
		}

		public static float Pow(float a, int b = 2)
		{
			if (b == 2)
			{
				return a * a;
			}
			if (b == 0)
			{
				return 1f;
			}
			float num = a;
			while (b > 1)
			{
				a *= num;
				b--;
			}
			return a;
		}

		public static float Pow(float a, float b)
		{
			if (b != 0.5f)
			{
				return Mathf.Pow(a, b);
			}
			return Mathf.Sqrt(a);
		}

		public static FnZoom getFnZoom(string fnname)
		{
			if (X.OFnZoom == null)
			{
				X.OFnZoom = new BDic<string, FnZoom>();
				X.OFnZoom["Z0"] = new FnZoom(X.Z0);
			}
			FnZoom fnZoom;
			if (!X.OFnZoom.TryGetValue(fnname, out fnZoom))
			{
				MethodInfo method = Type.GetType("XX.X").GetMethod(fnname, new Type[] { typeof(float) });
				fnZoom = (FnZoom)Delegate.CreateDelegate(typeof(FnZoom), method);
				if (fnZoom == null)
				{
					fnZoom = X.OFnZoom["Z0"];
				}
				X.OFnZoom[fnname] = fnZoom;
			}
			return fnZoom;
		}

		public static float Scr(float v1, float v2)
		{
			return 1f - (1f - v1) * (1f - v2);
		}

		public static float Scr2(float v1, float v2)
		{
			if (v1 >= 0f)
			{
				return 1f - (1f - v1) * (1f - v2);
			}
			return v1 + v2;
		}

		public static float MulOrScr(float v1, float v2)
		{
			if (v2 < 1f)
			{
				return v1 * v2;
			}
			return X.Scr(v1, v2 - 1f);
		}

		public static float ScrPow(float v1, int c = 2)
		{
			float num = 1f - v1;
			while (--c >= 1)
			{
				v1 = 1f - (1f - v1) * num;
			}
			return v1;
		}

		public static float Scr(float v1)
		{
			return 1f - (1f - v1) * (1f - v1);
		}

		public static float Z0(float num)
		{
			return 0f;
		}

		public static float Z1(float num)
		{
			return 1f;
		}

		public static float Zone(float num)
		{
			return (float)((num >= 0f) ? 1 : 0);
		}

		public static float ZSIN(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return X.Sin(X.MMX(0f, num, 1f) * 0.5f * 3.1415927f);
			}
			return 1f;
		}

		public static float ZSIN2(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return 1f - X.Pow(1f - X.Sin(X.MMX(0f, num, 1f) * 0.5f * 3.1415927f), 2);
			}
			return 1f;
		}

		public static float ZSIN3(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return 1f - X.Pow(1f - X.Sin(X.MMX(0f, num, 1f) * 0.5f * 3.1415927f), 3);
			}
			return 1f;
		}

		public static float ZSINN(float num, float power)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return 1f - X.Pow(1f - X.Sin(X.MMX(0f, num, 1f) * 0.5f * 3.1415927f), power);
			}
			return 1f;
		}

		public static float ZCOS(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return (-X.Cos(X.MMX(0f, num, 1f) * 3.1415927f) + 1f) / 2f;
			}
			return 1f;
		}

		public static float ZPOW(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return X.Pow(X.MMX(0f, num, 1f), 2);
			}
			return 1f;
		}

		public static float ZPOW3(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return X.Pow(X.MMX(0f, num, 1f), 3);
			}
			return 1f;
		}

		public static float ZSINV(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return 1f - X.Sin((1f - X.MMX(0f, num, 1f)) * 0.5f * 3.1415927f);
			}
			return 1f;
		}

		public static float ZSINV2(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return 1f - X.ZSIN2(1f - num);
			}
			return 1f;
		}

		public static float ZSINV3(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return 1f - X.ZSIN3(1f - num);
			}
			return 1f;
		}

		public static float ZPOWV(float num)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < 1f)
			{
				return 1f - X.ZPOW(1f - num);
			}
			return 1f;
		}

		public static float ZLINE(float num)
		{
			return X.MMX(0f, num, 1f);
		}

		public static float ZPOWN(float num, float power)
		{
			return X.Pow(X.MMX(0f, num, 1f), power);
		}

		public static float ZEXPLODE(float num)
		{
			return 0.86f * X.ZSIN3(num, 0.23f) + 0.14f * X.ZSIN(num);
		}

		public static float ZBOUNCE(float num, float _mx)
		{
			return 1.12f * X.ZSIN2(num, _mx * 0.56f) - 0.12f * X.ZCOS(num - _mx * 0.56f, _mx * 0.44f);
		}

		public static float ZSIN(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return X.Sin(X.MMX(0f, num / _mx, 1f) * 0.5f * 3.1415927f);
			}
			return 1f;
		}

		public static float ZSIN2(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return 1f - X.Pow(1f - X.Sin(X.MMX(0f, num / _mx, 1f) * 0.5f * 3.1415927f), 2);
			}
			return 1f;
		}

		public static float ZSIN3(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return 1f - X.Pow(1f - X.Sin(X.MMX(0f, num / _mx, 1f) * 0.5f * 3.1415927f), 3);
			}
			return 1f;
		}

		public static float ZSINN(float num, float _mx, float power)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return 1f - X.Pow(1f - X.Sin(X.MMX(0f, num / _mx, 1f) * 0.5f * 3.1415927f), power);
			}
			return 1f;
		}

		public static float ZCOS(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return (-X.Cos(X.MMX(0f, num / _mx, 1f) * 3.1415927f) + 1f) / 2f;
			}
			return 1f;
		}

		public static float ZPOW(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return X.Pow(X.MMX(0f, num / _mx, 1f), 2);
			}
			return 1f;
		}

		public static float ZPOW3(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return X.Pow(X.MMX(0f, num / _mx, 1f), 3);
			}
			return 1f;
		}

		public static float ZSINV(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return 1f - X.Sin((1f - X.MMX(0f, num / _mx, 1f)) * 0.5f * 3.1415927f);
			}
			return 1f;
		}

		public static float ZSINV2(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return 1f - X.ZSIN2(_mx - num, _mx);
			}
			return 1f;
		}

		public static float ZSINV3(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return 1f - X.ZSIN3(_mx - num, _mx);
			}
			return 1f;
		}

		public static float ZPOWV(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return 1f - X.ZPOW(_mx - num, _mx);
			}
			return 1f;
		}

		public static float ZLINE(float num, float _mx)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return X.MMX(0f, num / _mx, 1f);
			}
			return 1f;
		}

		public static float ZPOWN(float num, float _mx, float power)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return X.Pow(X.MMX(0f, num / _mx, 1f), power);
			}
			return 1f;
		}

		public static float ZPOP(float num, float _mx, float over = 0.125f)
		{
			if (num <= 0f)
			{
				return 0f;
			}
			if (num < _mx)
			{
				return X.ZSIN2(num, _mx * 0.44f) * (1f + over) - X.ZSIN(num - 0.44f, _mx * 0.56f) * over;
			}
			return 1f;
		}

		public static float RZSIN(float num)
		{
			return 1f - X.ZSIN(num);
		}

		public static float RZSIN2(float num)
		{
			return 1f - X.ZSIN2(num);
		}

		public static float RZSIN3(float num)
		{
			return 1f - X.ZSIN3(num);
		}

		public static float RZSINN(float num, float power)
		{
			return 1f - X.ZSINN(num, power);
		}

		public static float RZCOS(float num)
		{
			return 1f - X.ZCOS(num);
		}

		public static float RZPOW(float num)
		{
			return 1f - X.ZPOW(num);
		}

		public static float RZPOW3(float num)
		{
			return 1f - X.ZPOW3(num);
		}

		public static float RZSINV(float num)
		{
			return 1f - X.ZSINV(num);
		}

		public static float RZSINV2(float num)
		{
			return 1f - X.ZSINV2(num);
		}

		public static float RZSINV3(float num)
		{
			return 1f - X.ZSINV3(num);
		}

		public static float RZPOWV(float num)
		{
			return 1f - X.ZPOWV(num);
		}

		public static float RZLINE(float num)
		{
			return 1f - X.ZLINE(num);
		}

		public static float RZPOWN(float num, float power)
		{
			return 1f - X.ZPOWN(num, power);
		}

		public static float RZEXPLODE(float num)
		{
			return 1f - X.ZEXPLODE(num);
		}

		public static float GA01(float xloc1, float yloc1, float xloc2, float yloc2)
		{
			return X.GAR(xloc1, yloc1, xloc2, yloc2) / 6.2831855f;
		}

		public static float GAR(float xloc1, float yloc1, float xloc2, float yloc2)
		{
			return Mathf.Atan2(yloc2 - yloc1, xloc2 - xloc1);
		}

		public static float GAR2(float xloc1, float yloc1, float xloc2, float yloc2)
		{
			return X.ATAN2(yloc2 - yloc1, xloc2 - xloc1);
		}

		public static float GAR2(Vector2 V)
		{
			return X.ATAN2(V.y, V.x);
		}

		public static float ATAN2(float _y, float _x)
		{
			float num = X.Abs(_x);
			float num2 = X.Abs(_y);
			if (num2 == 0f)
			{
				if (_x <= 0f)
				{
					return -3.1415927f;
				}
				return 0f;
			}
			else
			{
				if (num != 0f)
				{
					bool flag = num2 < num;
					float num3;
					if (flag)
					{
						num3 = num2 / num;
					}
					else
					{
						num3 = num / num2;
					}
					int num4 = (int)(num3 * (num3 * (num3 * (829f * num3 - 2011f) - 58f) + 5741f));
					if (flag)
					{
						if (_x > 0f && _y < 0f)
						{
							num4 *= -1;
						}
						if (_x < 0f)
						{
							if (_y > 0f)
							{
								num4 = 18000 - num4;
							}
							if (_y < 0f)
							{
								num4 -= 18000;
							}
						}
					}
					if (!flag)
					{
						if (_x > 0f)
						{
							if (_y > 0f)
							{
								num4 = 9000 - num4;
							}
							if (_y < 0f)
							{
								num4 -= 9000;
							}
						}
						if (_x < 0f)
						{
							if (_y > 0f)
							{
								num4 += 9000;
							}
							if (_y < 0f)
							{
								num4 = -num4 - 9000;
							}
						}
					}
					return (float)num4 / 18000f * 3.1415927f;
				}
				if (_y <= 0f)
				{
					return -1.5707964f;
				}
				return 1.5707964f;
			}
		}

		public static AIM getTanArea(float _agR, float rect_tan)
		{
			float num = X.Abs(X.Tan(_agR));
			if (_agR < -1.5707964f)
			{
				if (num >= rect_tan)
				{
					return AIM.B;
				}
				return AIM.L;
			}
			else if (_agR < 0f)
			{
				if (num >= rect_tan)
				{
					return AIM.B;
				}
				return AIM.R;
			}
			else if (_agR < 1.5707964f)
			{
				if (num >= rect_tan)
				{
					return AIM.T;
				}
				return AIM.R;
			}
			else
			{
				if (num >= rect_tan)
				{
					return AIM.T;
				}
				return AIM.L;
			}
		}

		public static float Cos(float x)
		{
			float num = 1f;
			if (x < 0f)
			{
				x = -x;
			}
			x -= (float)((int)(x * 0.31830987f * 0.5f)) * 6.2831855f;
			if (x >= 3.1415927f)
			{
				x = 6.2831855f - x;
			}
			if (x >= 1.5707964f)
			{
				x = 3.1415927f - x;
				num = -1f;
			}
			if (x > 0.7853982f)
			{
				x = 1.5707964f - x;
				float num2 = x * x;
				return x * (1f - num2 * 0.16666667f + num2 * num2 * 0.008333334f) * num;
			}
			float num3 = x * x;
			return (1f - num3 * 0.5f + num3 * num3 * 0.041666668f - num3 * num3 * num3 * 0.0013888889f) * num;
		}

		public static float Sin(float x)
		{
			if (x != 0f)
			{
				return X.Cos(x - 1.5707964f);
			}
			return 0f;
		}

		public static float Tan(float x)
		{
			return X.Sin(x) / X.Cos(x);
		}

		public static float Cos0(float x)
		{
			return X.Cos(x * 3.1415927f * 2f);
		}

		public static float Sin0(float x)
		{
			return X.Sin(x * 3.1415927f * 2f);
		}

		public static float Tan0(float x)
		{
			return X.Sin0(x) / X.Cos0(x);
		}

		public static float Cos360(float x)
		{
			return X.Cos(x / 360f * 3.1415927f * 2f);
		}

		public static float Sin360(float x)
		{
			return X.Sin(x / 360f * 3.1415927f * 2f);
		}

		public static float frac(float x)
		{
			return x - (float)((int)x);
		}

		public static float fracMn(float x)
		{
			float num = X.frac(x);
			return X.Mn(num, 1f - num);
		}

		public static float frac2(float x)
		{
			if (x < 0f)
			{
				x = (float)(2 - (int)x);
			}
			return x - (float)((int)x);
		}

		public static int Mx(int v1, int v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static int Mn(int v1, int v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static uint Mx(uint v1, uint v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static uint Mn(uint v1, uint v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static byte Mx(byte v1, byte v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static byte Mn(byte v1, byte v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static float Mx(float v1, float v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static float Mn(float v1, float v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static double Mx(double v1, double v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static double Mn(double v1, double v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static long Mx(long v1, long v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static long Mn(long v1, long v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static ulong Mx(ulong v1, ulong v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static ulong Mn(ulong v1, ulong v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static short Mx(short v1, short v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static short Mn(short v1, short v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static ushort Mx(ushort v1, ushort v2)
		{
			if (v1 >= v2)
			{
				return v1;
			}
			return v2;
		}

		public static ushort Mn(ushort v1, ushort v2)
		{
			if (v1 <= v2)
			{
				return v1;
			}
			return v2;
		}

		public static float MPSecureAdd(float v1, float addition)
		{
			if (v1 < 0f)
			{
				return X.Mn(0f, v1 + addition);
			}
			return X.Mx(0f, v1 + addition);
		}

		public static float[] NAIBUN(float[] A1, float[] A2, float v)
		{
			return new float[]
			{
				A1[0] * (1f - v) + A2[0] * v,
				A1[1] * (1f - v) + A2[1] * v
			};
		}

		public static float[] BEZIER(float[] P0, float[] P1, float[] P2, float[] P3, float rate_t, float[] ret = null)
		{
			float[] array = X.NAIBUN(P0, P1, rate_t);
			float[] array2 = X.NAIBUN(P1, P2, rate_t);
			float[] array3 = X.NAIBUN(P2, P3, rate_t);
			float[] array4 = X.NAIBUN(array, array2, rate_t);
			float[] array5 = X.NAIBUN(array2, array3, rate_t);
			if (ret == null)
			{
				ret = new float[2];
			}
			ret[0] = X.NAIBUN_I(array4[0], array5[0], rate_t);
			ret[1] = X.NAIBUN_I(array4[1], array5[1], rate_t);
			return ret;
		}

		public static float BEZIER_I(float p0x, float p1x, float p2x, float p3x, float rate_t)
		{
			float num = X.NAIBUN_I(p0x, p1x, rate_t);
			float num2 = X.NAIBUN_I(p1x, p2x, rate_t);
			float num3 = X.NAIBUN_I(p2x, p3x, rate_t);
			float num4 = X.NAIBUN_I(num, num2, rate_t);
			float num5 = X.NAIBUN_I(num2, num3, rate_t);
			return X.NAIBUN_I(num4, num5, rate_t);
		}

		public static float PARABOLA(float syouten_x, float syouten_y, float Px, float Py, float xpos)
		{
			return (Py - syouten_y) / X.Pow(Px - syouten_x, 2) * X.Pow(xpos - syouten_x, 2) + syouten_y;
		}

		public static float NAIBUN_I(float v1, float v2, float f = 0.5f)
		{
			return v1 * (1f - f) + v2 * f;
		}

		public static float NI(float v1, float v2, float f = 0.5f)
		{
			return v1 * (1f - f) + v2 * f;
		}

		public static Vector2 NI(Vector2 v1, Vector2 v2, float f = 0.5f)
		{
			return v1 * (1f - f) + v2 * f;
		}

		public static Vector3 NI(Vector3 v1, Vector3 v2, float f = 0.5f)
		{
			return v1 * (1f - f) + v2 * f;
		}

		public static float NI3(float v0, float v1, float v2, float level)
		{
			if (level > 2f)
			{
				level -= (float)((int)(level / 2f));
			}
			if (level > 1f)
			{
				return X.NI(v2, v0, X.ZLINE(level - 1f));
			}
			if (level < 0.5f)
			{
				return X.NI(v0, v1, X.ZLINE(level * 2f));
			}
			return X.NI(v1, v2, X.ZLINE((level - 0.5f) * 2f));
		}

		public static float NI3(float[] v0, float level)
		{
			if (v0.Length == 2)
			{
				return X.NI(v0[0], v0[1], level);
			}
			return X.NI3(v0[0], v0[1], v0[2], level);
		}

		public static float NI(int v1, int v2, float f)
		{
			return (float)v1 * (1f - f) + (float)v2 * f;
		}

		public static float NIL(float v1, float v2, float f, float div = 1f)
		{
			f = X.ZLINE(f, div);
			return v1 * (1f - f) + v2 * f;
		}

		public static float NIXP(float v1, float v2)
		{
			float num = X.XORSP();
			return v1 * (1f - num) + v2 * num;
		}

		public static float Abs(float num)
		{
			if (num <= 0f)
			{
				return -num;
			}
			return num;
		}

		public static int Abs(int num)
		{
			if (num <= 0)
			{
				return -num;
			}
			return num;
		}

		public static int Int(float num)
		{
			return Mathf.FloorToInt(num);
		}

		public static int IntU(float num)
		{
			return Mathf.FloorToInt(num);
		}

		public static int IntN(float num)
		{
			if (num >= 0f)
			{
				return X.Int(num);
			}
			return X.IntC(num);
		}

		public static int IntNC(float num)
		{
			if (num <= 0f)
			{
				return X.Int(num);
			}
			return X.IntC(num);
		}

		public static int IntR(float num)
		{
			return Mathf.RoundToInt(num);
		}

		public static int IntC(float num)
		{
			return Mathf.CeilToInt(num);
		}

		public static float Nm(string num, float isNaNdefault = 0f, bool emptyDefault = false)
		{
			if (num == null)
			{
				return isNaNdefault;
			}
			if (num.Length >= 2 && num[0] == '(' && num[num.Length - 1] == ')')
			{
				num = num.Substring(1, num.Length - 2);
			}
			if (num.Length == 0 && emptyDefault)
			{
				return isNaNdefault;
			}
			float num2;
			if (!float.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out num2))
			{
				return isNaNdefault;
			}
			if (!float.IsNaN(num2))
			{
				return num2;
			}
			return isNaNdefault;
		}

		public static int NmI(string num, int isNaNdefault = 0, bool emptyDefault = false, bool hex_mode = false)
		{
			if (num == null)
			{
				return isNaNdefault;
			}
			if (num.Length >= 2 && num[0] == '(' && num[num.Length - 1] == ')')
			{
				num = num.Substring(1, num.Length - 2);
			}
			if (num.Length == 0 && emptyDefault)
			{
				return isNaNdefault;
			}
			if (hex_mode && TX.isStart(num, "0x", 0))
			{
				return X.NmI(num.Substring(2), isNaNdefault, emptyDefault, hex_mode);
			}
			if (!hex_mode && num.IndexOf(".") >= 0)
			{
				return (int)X.Nm(num, (float)isNaNdefault, emptyDefault);
			}
			int num2;
			if (hex_mode ? int.TryParse(num, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out num2) : int.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out num2))
			{
				return num2;
			}
			return isNaNdefault;
		}

		public static uint NmUI(string num, uint isNaNdefault = 0U, bool emptyDefault = false, bool hex_mode = false)
		{
			if (num == null)
			{
				return isNaNdefault;
			}
			if (num.Length >= 2 && num[0] == '(' && num[num.Length - 1] == ')')
			{
				num = num.Substring(1, num.Length - 2);
			}
			if (num.Length == 0 && emptyDefault)
			{
				return isNaNdefault;
			}
			if (hex_mode)
			{
				if (TX.isStart(num, "0x", 0))
				{
					return X.NmUI(num.Substring(2), isNaNdefault, emptyDefault, hex_mode);
				}
				if (num.IndexOf(":#") == 2)
				{
					return X.NmUI(num.Substring(4), isNaNdefault, emptyDefault, hex_mode) | (X.NmUI(TX.slice(num, 0, 2), isNaNdefault, emptyDefault, hex_mode) << 24);
				}
			}
			uint num2;
			if (uint.TryParse(num, hex_mode ? NumberStyles.AllowHexSpecifier : NumberStyles.Integer, CultureInfo.InvariantCulture, out num2))
			{
				return num2;
			}
			return isNaNdefault;
		}

		public static float Nm(float nf, float isNaNdefault = 0f)
		{
			if (!float.IsNaN(nf))
			{
				return nf;
			}
			return isNaNdefault;
		}

		public static float Nm(int nf, float isNaNdefault = 0f)
		{
			if (!float.IsNaN((float)nf))
			{
				return (float)nf;
			}
			return isNaNdefault;
		}

		public static float Nm(char nf, float isNaNdefault = 0f)
		{
			if (!float.IsNaN((float)nf))
			{
				return (float)nf;
			}
			return isNaNdefault;
		}

		public static T Get<T>(List<T> A, int index = 0)
		{
			if (A != null && X.BTW(0f, (float)index, (float)A.Count))
			{
				return A[index];
			}
			return default(T);
		}

		public static T Get<T>(List<T> A, int index, T _default)
		{
			if (A != null && X.BTW(0f, (float)index, (float)A.Count))
			{
				return A[index];
			}
			return _default;
		}

		public static float GetNm(List<string> A, float isNaNdefault = 0f, bool emptyDefault = false, int index = 0)
		{
			if (A != null && X.BTW(0f, (float)index, (float)A.Count))
			{
				return X.Nm(A[index], isNaNdefault, emptyDefault);
			}
			return isNaNdefault;
		}

		public static int GetNmI(List<string> A, int isNaNdefault = 0, bool emptyDefault = false, int index = 0)
		{
			if (A != null && X.BTW(0f, (float)index, (float)A.Count))
			{
				return X.NmI(A[index], isNaNdefault, emptyDefault, false);
			}
			return isNaNdefault;
		}

		public static int NmI(char[] Achar, int char_from = 0, int invalid_default = 0)
		{
			int num = Achar.Length;
			int num2 = 0;
			if (char_from >= num)
			{
				return invalid_default;
			}
			int i = char_from;
			while (i < num)
			{
				int num3 = TX.ToNum(Achar[i]);
				if (num3 >= 0)
				{
					num2 = num2 * 10 + num3;
					i++;
				}
				else
				{
					if (i != char_from)
					{
						return num2;
					}
					return invalid_default;
				}
			}
			return num2;
		}

		public static T Get1<T>(T[] A, int i, T def, bool return_def_ifnull) where T : class
		{
			if (A == null || i < 0 || i >= A.Length)
			{
				return def;
			}
			T t = A[i];
			if (!return_def_ifnull || t != null)
			{
				return t;
			}
			return def;
		}

		public static T Get1<T>(List<T> A, int i, T def, bool return_def_ifnull) where T : class
		{
			if (A == null || i < 0 || i >= A.Count)
			{
				return def;
			}
			T t = A[i];
			if (!return_def_ifnull || t != null)
			{
				return t;
			}
			return def;
		}

		public static string Get1S(string[] A, int i, string def = "", bool return_def_ifnull_or_empty = false)
		{
			if (A == null || i < 0 || i >= A.Length)
			{
				return def;
			}
			string text = A[i];
			if (!return_def_ifnull_or_empty || (text != null && !(text == "")))
			{
				return text;
			}
			return def;
		}

		public static T2 Get<T, T2>(BDic<T, T2> O, T key) where T2 : class
		{
			T2 t;
			if (!O.TryGetValue(key, out t))
			{
				return default(T2);
			}
			return t;
		}

		public static T2 Get<T2>(BDic<string, T2> O, StringKey key) where T2 : class
		{
			T2 t;
			if (!O.TryGetValue(key, out t))
			{
				return default(T2);
			}
			return t;
		}

		public static T2 Get<T, T2>(BDic<T, T2> O, T key, T2 Default) where T2 : struct
		{
			T2 t;
			if (!O.TryGetValue(key, out t))
			{
				return Default;
			}
			return t;
		}

		public static T2 GetS<T, T2>(BDic<T, T2> O, T key, T2 Default)
		{
			T2 t;
			if (!O.TryGetValue(key, out t))
			{
				return Default;
			}
			return t;
		}

		public static bool pushIdentical<T>(List<T> A, T value)
		{
			if (A.IndexOf(value) == -1)
			{
				A.Add(value);
				return true;
			}
			return false;
		}

		public static void RemoveByVal<T, T2>(BDic<T, T2> O, T2 value) where T2 : class
		{
			T t = default(T);
			bool flag = false;
			foreach (KeyValuePair<T, T2> keyValuePair in O)
			{
				if (keyValuePair.Value == value)
				{
					flag = true;
					t = keyValuePair.Key;
					break;
				}
			}
			if (flag)
			{
				O.Remove(t);
			}
		}

		public static bool GetKeyByValue<T, T2>(BDic<T, T2> O, T2 value, out T key) where T2 : class
		{
			key = default(T);
			foreach (KeyValuePair<T, T2> keyValuePair in O)
			{
				if (keyValuePair.Value == value)
				{
					key = keyValuePair.Key;
					return true;
				}
			}
			return false;
		}

		public static bool GetKeyByValueS<T, T2>(BDic<T, T2> O, T2 value, out T key) where T2 : IEquatable<T>
		{
			key = default(T);
			foreach (KeyValuePair<T, T2> keyValuePair in O)
			{
				if (keyValuePair.Value.Equals(value))
				{
					key = keyValuePair.Key;
					return true;
				}
			}
			return false;
		}

		public static T GetKeyByIndex<T, T2>(BDic<T, T2> O, int i)
		{
			foreach (KeyValuePair<T, T2> keyValuePair in O)
			{
				if (i-- <= 0)
				{
					return keyValuePair.Key;
				}
			}
			return default(T);
		}

		public static float MMX(float minnum, float thenum, float maxnum)
		{
			return X.Mx(minnum, X.Mn(thenum, maxnum));
		}

		public static float MMX2(float minnum, float thenum, float maxnum)
		{
			if (minnum <= maxnum)
			{
				return X.Mx(minnum, X.Mn(thenum, maxnum));
			}
			return (minnum + maxnum) * 0.5f;
		}

		public static float saturate(float thenum)
		{
			return X.Mx(0f, X.Mn(thenum, 1f));
		}

		public static double MMX(double minnum, double thenum, double maxnum)
		{
			return X.Mx(minnum, X.Mn(thenum, maxnum));
		}

		public static double saturate(double thenum)
		{
			return X.Mx(0.0, X.Mn(thenum, 1.0));
		}

		public static int MMX(int minnum, int thenum, int maxnum)
		{
			return X.Mx(minnum, X.Mn(thenum, maxnum));
		}

		public static int saturate(int thenum)
		{
			return X.Mx(0, X.Mn(thenum, 1));
		}

		public static uint MMX(uint minnum, uint thenum, uint maxnum)
		{
			return X.Mx(minnum, X.Mn(thenum, maxnum));
		}

		public static int MPF(bool n)
		{
			if (!n)
			{
				return -1;
			}
			return 1;
		}

		public static int MPFXP()
		{
			if ((X.xors() & 1U) != 1U)
			{
				return -1;
			}
			return 1;
		}

		public static int MPFXP(float _ran)
		{
			if (_ran >= 0.5f)
			{
				return -1;
			}
			return 1;
		}

		public static float getArrayRealPos(float[] _array, float _val, float minval = 0f)
		{
			int num = _array.Length;
			if (num <= 1)
			{
				return 0f;
			}
			float num2 = minval;
			for (int i = 0; i < num; i++)
			{
				float num3 = _array[i];
				if (_val < num3)
				{
					return (float)(i - 1) + ((num3 == num2) ? 0f : ((_val - num2) / (num3 - num2)));
				}
				num2 = num3;
			}
			return (float)num;
		}

		public static float getArrayRealNum(float[] _array, float _real)
		{
			int num = X.MMX(0, X.Int(_real), _array.Length - 1);
			int num2 = X.MMX(0, X.Int(_real + 1f), _array.Length - 1);
			float num3 = _real - (float)num;
			return _array[num] * (1f - num3) + _array[num2] * num3;
		}

		public static float getArrayRealNum(int[] _array, float _real)
		{
			int num = X.MMX(0, X.Int(_real), _array.Length - 1);
			int num2 = X.MMX(0, X.Int(_real + 1f), _array.Length - 1);
			float num3 = _real - (float)num;
			return (float)_array[num] * (1f - num3) + (float)_array[num2] * num3;
		}

		public static string spr0(int x, int keta, char suppresschar = '0')
		{
			STB stb = TX.PopBld(null, 0);
			stb.spr0(x, keta, suppresschar);
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			return text;
		}

		public static string spr_after(float _val, int kurai)
		{
			if (kurai <= 0)
			{
				return ((int)_val).ToString();
			}
			string text;
			if (_val == 0f)
			{
				text = "0.";
				for (int i = 0; i < kurai; i++)
				{
					text += "0";
				}
				return text;
			}
			string text2 = ((_val > 0f) ? "" : "-");
			_val *= (float)((int)X.Pow(10f, kurai));
			text = ((int)X.Abs(_val)).ToString();
			while (text.Length <= kurai)
			{
				text = "0" + text;
			}
			return text2 + ((text.Length - kurai > 0) ? TX.slice(text, 0, text.Length - kurai) : "0") + "." + TX.slice(text, text.Length - kurai);
		}

		public static string spr_after(Vector2 V, int kurai)
		{
			return X.spr_after(V.x, kurai) + ", " + X.spr_after(V.y, kurai);
		}

		public static string spr_space_fill(string val, int dep_length, char fill = ' ')
		{
			while (val.Length < dep_length)
			{
				val = fill.ToString() + val;
			}
			return val;
		}

		public static string spr_percentage(float val01, int after_kurai, string suffix = "%")
		{
			if (after_kurai == 0)
			{
				return X.spr0((val01 < 0.5f) ? X.IntC(val01 * 100f) : ((int)(val01 * 100f)), 3, ' ') + suffix;
			}
			return X.spr_space_fill(X.spr_after((float)((val01 < 0.5f) ? X.IntC(val01 * 1000f) : ((int)(val01 * 1000f))) / 10f, after_kurai), after_kurai + 4, ' ') + suffix;
		}

		public static float VALWALK(float var_value, float depvar, float speed)
		{
			if (var_value < depvar)
			{
				var_value += X.Mn(speed, depvar - var_value);
			}
			else if (var_value > depvar)
			{
				var_value += X.Mx(-speed, depvar - var_value);
			}
			return var_value;
		}

		public static int VALWALK(int var_value, int depvar, int speed)
		{
			if (var_value < depvar)
			{
				var_value += X.Mn(speed, depvar - var_value);
			}
			else if (var_value > depvar)
			{
				var_value += X.Mx(-speed, depvar - var_value);
			}
			return var_value;
		}

		public static float VALWALKANGLER(float var_value, float depvar, float speed)
		{
			float num = X.angledifR(var_value, depvar);
			var_value = X.VALWALK(var_value, var_value + num, speed);
			return X.correctangleR(var_value);
		}

		public static float VALWALKMXR(float var_value, float depvar, float speed, float max_ratio = 0.25f)
		{
			float num = depvar - var_value;
			if (num > 0f)
			{
				var_value += X.Mn(X.Mx(num * 0.05f, X.Mn(num * max_ratio, speed)), num);
			}
			else if (num < 0f)
			{
				var_value += X.Mx(X.Mn(num * 0.05f, X.Mx(num * max_ratio, -speed)), num);
			}
			return var_value;
		}

		public static float MULWALK(float var_double, float depdouble, float speed)
		{
			return var_double + (depdouble - var_double) * speed;
		}

		public static float MULWALKF(float var_double, float depdouble, float speed, float fcnt)
		{
			return var_double + (depdouble - var_double) * ((fcnt > 1f) ? (1f - X.Pow(1f - speed, fcnt)) : speed);
		}

		public static float MULWALKMN(float var_double, float depdouble, float speed, float minvalue)
		{
			return var_double + X.absMn((depdouble - var_double) * speed, minvalue);
		}

		public static float MULWALKMX(float var_double, float depdouble, float speed, float maxvalue)
		{
			float num = var_double + (depdouble - var_double) * speed;
			if (var_double < depdouble)
			{
				return X.MMX(num, var_double + maxvalue, depdouble);
			}
			return X.MMX(depdouble, var_double - maxvalue, num);
		}

		public static float MULWALKMNA(float var_double, float depdouble, float speed, float maxvalue)
		{
			if (X.Abs(var_double - depdouble) < maxvalue)
			{
				var_double = depdouble;
			}
			else
			{
				float num = X.absMx((depdouble - var_double) * speed, maxvalue);
				var_double += num;
			}
			return var_double;
		}

		public static float MULWALKANGLE(float var_double, float depdouble, float speed)
		{
			var_double += X.angledif(var_double, depdouble) * speed;
			return X.correctangle(var_double);
		}

		public static float MULWALKANGLEMN(float var_double, float depdouble, float speed, float minvalue)
		{
			float num = X.angledif(var_double, depdouble) * speed;
			if (X.BTWW(depdouble - 0.0001f, var_double, depdouble + 0.0001f))
			{
				var_double = depdouble;
			}
			else
			{
				var_double += X.absmin(num, minvalue);
			}
			var_double = X.correctangle(var_double);
			return var_double;
		}

		public static float MULWALKANGLEMX(float var_double, float depdouble, float speed, float minvalue)
		{
			float num = X.absmax(X.angledif(var_double, depdouble) * speed, minvalue);
			float num2 = X.Abs(num);
			if (X.BTWW(depdouble - num2, var_double, depdouble + num2))
			{
				var_double = depdouble;
			}
			else
			{
				var_double += num;
			}
			var_double = X.correctangle(var_double);
			return var_double;
		}

		public static float MULWALKANGLER(float var_double, float depdouble, float speed)
		{
			var_double /= 6.2831855f;
			var_double = X.MULWALKANGLE(var_double, depdouble / 6.2831855f, speed);
			var_double *= 6.2831855f;
			return var_double;
		}

		public static float absmin(float val, float abs_minval)
		{
			if (abs_minval < 0f)
			{
				abs_minval = -abs_minval;
			}
			return X.MMX(-abs_minval, val, abs_minval);
		}

		public static float absmax(float val, float abs_maxval)
		{
			return X.Mx(X.Abs(val), X.Abs(abs_maxval)) * (float)((val > 0f) ? 1 : (-1));
		}

		public static float absMn(float val, float abs_minval)
		{
			if (abs_minval < 0f)
			{
				abs_minval = -abs_minval;
			}
			return X.absmin(val, abs_minval);
		}

		public static float absMx(float val, float abs_maxval)
		{
			return X.absmax(val, abs_maxval);
		}

		public static float absMMX(float abs_minval, float val, float abs_maxval)
		{
			return X.MMX(X.Abs(abs_minval), X.Abs(val), X.Abs(abs_maxval)) * (float)X.MPF(val > 0f);
		}

		public static DRect rectExpand(DRect Rc, float _x, float _y, float _w, float _h)
		{
			float right = Rc.right;
			float bottom = Rc.bottom;
			if (_w < 0f)
			{
				_x += _w;
				_w = -_w;
			}
			if (_h < 0f)
			{
				_y += _h;
				_h = -_h;
			}
			Rc.x = X.Mn(Rc.x, _x);
			Rc.y = X.Mn(Rc.y, _y);
			Rc.width = X.Mx(right, X.Mx(Rc.right, _x + _w)) - Rc.x;
			Rc.height = X.Mx(bottom, X.Mx(Rc.bottom, _y + _h)) - Rc.y;
			return Rc;
		}

		public static DRect rectExpandRc(DRect Rc, DRect Rc2)
		{
			if (Rc2 == null)
			{
				return Rc;
			}
			return X.rectExpand(Rc, Rc2.left, Rc2.top, Rc2.width, Rc2.height);
		}

		public static DRect rectMultiply(DRect Rc, float _x, float _y, float _w, float _h)
		{
			Rect rect = Rc.getRect();
			float num = X.Mx(Rc.x, _x);
			float num2 = X.Mx(Rc.y, _y);
			float num3 = X.Mn(Rc.right, _x + _w);
			float num4 = X.Mn(Rc.bottom, _y + _h);
			Rc.Set(num, num2, num3 - num, num4 - num2);
			if (num3 < num)
			{
				if (_w > 0f)
				{
					Rc.x = X.MMX(_x, Rc.x, _x + _w);
				}
				else if (rect.width > 0f)
				{
					Rc.x = X.MMX(rect.x, Rc.x, rect.xMax);
				}
				Rc.width = 0f;
			}
			if (num4 < num2)
			{
				if (_h > 0f)
				{
					Rc.y = X.MMX(_y, Rc.y, _y + _h);
				}
				else if (rect.height > 0f)
				{
					Rc.y = X.MMX(rect.y, Rc.y, rect.yMax);
				}
				Rc.height = 0f;
			}
			return Rc;
		}

		public static Rect rectMultiply(Rect Rc, float _x, float _y, float _w, float _h)
		{
			Rect rect = Rc;
			float num = X.Mx(Rc.x, _x);
			float num2 = X.Mx(Rc.y, _y);
			float num3 = X.Mn(Rc.xMax, _x + _w);
			float num4 = X.Mn(Rc.yMax, _y + _h);
			Rc.Set(num, num2, num3 - num, num4 - num2);
			if (num3 < num)
			{
				if (_w > 0f)
				{
					Rc.x = X.MMX(_x, Rc.x, _x + _w);
				}
				else if (rect.width > 0f)
				{
					Rc.x = X.MMX(rect.x, Rc.x, rect.xMax);
				}
				Rc.width = 0f;
			}
			if (num4 < num2)
			{
				if (_h > 0f)
				{
					Rc.y = X.MMX(_y, Rc.y, _y + _h);
				}
				else if (rect.height > 0f)
				{
					Rc.y = X.MMX(rect.y, Rc.y, rect.yMax);
				}
				Rc.height = 0f;
			}
			return Rc;
		}

		public static DRect rectMultiplyRc(DRect Rc, DRect Rc2)
		{
			if (Rc2 == null)
			{
				return Rc;
			}
			return X.rectMultiply(Rc, Rc2.left, Rc2.top, Rc2.width, Rc2.height);
		}

		public static Rect rectMultiply(Rect Rc, float _x, float _y)
		{
			Rc.x *= _x;
			Rc.y *= _y;
			Rc.width *= _x;
			Rc.height *= _y;
			return Rc;
		}

		public static Rect rectDivide(Rect Rc, float _x, float _y)
		{
			Rc.x /= _x;
			Rc.y /= _y;
			Rc.width /= _x;
			Rc.height /= _y;
			return Rc;
		}

		public static int yakusu(int a, int b)
		{
			if (a < b)
			{
				int num = b;
				int num2 = a;
				a = num;
				b = num2;
			}
			for (int num3 = a % b; num3 != 0; num3 = a % b)
			{
				a = b;
				b = num3;
			}
			return b;
		}

		public static float LENGTHXY(float the_x1, float the_y1, float the_x2 = 0f, float the_y2 = 0f)
		{
			return Mathf.Sqrt(X.Pow2(the_x1 - the_x2) + X.Pow2(the_y1 - the_y2));
		}

		public static float LENGTHXY2(float the_x1, float the_y1, float the_x2, float the_y2)
		{
			return X.Pow2(the_x1 - the_x2) + X.Pow2(the_y1 - the_y2);
		}

		public static float LENGTHXY2(Vector2 pos1, Vector2 pos2)
		{
			return X.Pow2(pos1.x - pos2.x) + X.Pow2(pos1.y - pos2.y);
		}

		public static Vector3 crosspoint(float x1s, float y1s, float x1e, float y1e, float x2s, float y2s, float x2e, float y2e)
		{
			Vector4 vector = new Vector4(-(y1e - y1s), x1e - x1s, -(y2e - y2s), x2e - x2s);
			float num = vector.w * vector.x - vector.y * vector.z;
			if (num == 0f)
			{
				return Vector3.zero;
			}
			Vector2 vector2 = new Vector2((x1e * y1s - y1e * x1s) / num, (x2e * y2s - y2e * x2s) / num);
			Vector3 one = Vector3.one;
			one.x = ((x2s == x2e) ? x2s : ((x1s == x1e) ? x1s : (vector.w * vector2[0] + -vector.y * vector2[1])));
			one.y = ((y2s == y2e) ? y2s : ((y1s == y1e) ? y1s : (-vector.z * vector2[0] + vector.x * vector2[1])));
			return one;
		}

		public static void calcLineGraphVariable(float x0, float y0, float x1, float y1, out float la, out float lb, out float lc, out float la_div)
		{
			if (x0 == x1)
			{
				la = (la_div = 1f);
				lb = 0f;
				lc = -x0;
				return;
			}
			if (y1 == y0)
			{
				la = (la_div = 0f);
				lb = 1f;
				lc = -y0;
				return;
			}
			lb = 1f;
			float num = 1f / (y1 - y0);
			float num2 = 1f / (x1 - x0);
			la = (y0 - y1) * num2;
			la_div = (x0 - x1) * num;
			lc = -la * x0 - y0;
		}

		public static bool vec_vec_X(float bx, float by, float cx, float cy, float px, float py, float qx, float qy)
		{
			float num = px - bx;
			float num2 = py - by;
			float num3 = cx - bx;
			float num4 = cy - by;
			float num5 = qx - bx;
			float num6 = qy - by;
			float num7 = qx - px;
			float num8 = qy - py;
			float num9 = cx - px;
			float num10 = cy - py;
			return (num * num4 - num2 * num3) * (num5 * num4 - num6 * num3) < 0f && (-num * num8 + num2 * num7) * (num9 * num8 - num10 * num7) < 0f;
		}

		public static bool vec_rec_X(float px, float py, float qx, float qy, float rx, float ry, float rr, float rb)
		{
			return X.vec_vec_X(px, py, qx, qy, rx, ry, rr, ry) || X.vec_vec_X(px, py, qx, qy, rr, ry, rr, rb) || X.vec_vec_X(px, py, qx, qy, rr, rb, rx, rb) || X.vec_vec_X(px, py, qx, qy, rx, rb, rx, ry);
		}

		public static float LENGTHXYS(float the_x1, float the_y1, float the_x2, float the_y2)
		{
			return X.Abs(the_x1 - the_x2) + X.Abs(the_y1 - the_y2);
		}

		public static float LENGTH_SIZE(float x1, float size1, float x2, float size2)
		{
			return X.Mx(X.Abs(x1 - x2) - (size1 + size2), 0f);
		}

		public static float LENGTHXYN(float the_x1, float the_y1, float the_x2, float the_y2)
		{
			return X.Mx(X.Abs(the_x1 - the_x2), X.Abs(the_y1 - the_y2));
		}

		public static float LENGTHXYRECT(float x1, float y1, Rect R)
		{
			return X.LENGTHXYBD(x1, y1, R.x, R.y, R.x + R.width, R.y + R.height);
		}

		public static float LENGTHXYBD(float x1, float y1, float lx, float ty, float rx, float by)
		{
			float num = X.MMX(lx, x1, rx);
			float num2 = X.MMX(ty, y1, by);
			return X.LENGTHXY(num, num2, x1, y1);
		}

		public static float LENGTHXYRECTS2(float l1, float t1, float r1, float b1, float l2, float t2, float r2, float b2)
		{
			float num;
			if (X.isCovering(l1, r1, l2, r2, 0f))
			{
				num = 0f;
			}
			else if (r1 <= l2)
			{
				num = l2 - r1;
			}
			else if (l1 >= r2)
			{
				num = l1 - r2;
			}
			else
			{
				num = X.Abs((l1 + r1) / 2f - (l2 + r2) / 2f);
			}
			float num2;
			if (X.isCovering(t1, b1, t2, b2, 0f))
			{
				num2 = 0f;
			}
			else if (b1 <= t2)
			{
				num2 = t2 - b1;
			}
			else if (t1 >= b2)
			{
				num2 = t1 - b2;
			}
			else
			{
				num2 = X.Abs((t1 + b1) / 2f - (t2 + b2) / 2f);
			}
			return num + num2;
		}

		public static float LENGTHXYOW(float the_x1, float the_y1, float the_x2, float the_y2, float sizex, float sizey)
		{
			return Mathf.Sqrt(X.LENGTHXYOW2(the_x1, the_y1, the_x2, the_y2, sizex, sizey));
		}

		public static float LENGTHXYOW2(float the_x1, float the_y1, float the_x2, float the_y2, float sizex, float sizey)
		{
			float num = the_x2 - the_x1;
			float num2 = the_y2 - the_y1;
			object obj = ((-sizex < num && num < sizex) ? 0f : ((num > 0f) ? (num - sizex) : (-sizex - num)));
			float num3 = ((-sizey < num2 && num2 < sizey) ? 0f : ((num2 > 0f) ? (num2 - sizey) : (-sizey - num2)));
			object obj2 = obj;
			return obj2 * obj2 + num3 * num3;
		}

		public static bool chkLEN(float the_x1, float the_y1, float the_x2, float the_y2, float r)
		{
			return X.chkLEN2(the_x1, the_y1, the_x2, the_y2, r * r);
		}

		public static bool chkLEN(float the_x1, float the_y1, float the_x2, float the_y2, Vector2 Dif)
		{
			return X.chkLEN2(the_x1, the_y1, the_x2, the_y2, Dif.x * Dif.x + Dif.y * Dif.y);
		}

		public static bool chkLEN2(float the_x1, float the_y1, float the_x2, float the_y2, float r2)
		{
			return X.Pow2(the_x1 - the_x2) + X.Pow2(the_y1 - the_y2) <= r2;
		}

		public static bool chkLENW(float the_x1, float the_y1, float the_x2, float the_y2, float r, float r2 = 0f)
		{
			return X.chkLENW0(the_x1, the_y1, the_x2, the_y2, r, r2) && X.chkLEN(the_x1, the_y1, the_x2, the_y2, r + r2);
		}

		public static bool chkLENW0(float the_x1, float the_y1, float the_x2, float the_y2, float r, float r2 = 0f)
		{
			return the_x1 + r >= the_x2 - r2 && the_x1 - r <= the_x2 + r2 && the_y1 + r >= the_y2 - r2 && the_y1 - r <= the_y2 + r2;
		}

		public static bool chkLENOW(float the_x1, float the_y1, float the_x2, float the_y2, float sizex, float sizey, float r = 0f)
		{
			float num = the_x2 - the_x1;
			float num2 = the_y2 - the_y1;
			if (r + sizex < num)
			{
				return false;
			}
			if (-r - sizex > num)
			{
				return false;
			}
			if (r + sizey < num2)
			{
				return false;
			}
			if (-r - sizey > num2)
			{
				return false;
			}
			if (r <= 0f)
			{
				return true;
			}
			float num3 = 1f / X.Pow2(r);
			return ((-sizex < num && num < sizex) ? 0f : (X.Pow2((num > 0f) ? (num - sizex) : (-sizex - num)) * num3)) + ((-sizey < num2 && num2 < sizey) ? 0f : (X.Pow2((num2 > 0f) ? (num2 - sizey) : (-sizey - num2)) * num3)) <= 1f;
		}

		public static bool chkLENRectCirc(float x, float y, float r, float b, float circ_x, float circ_y, float radius)
		{
			bool flag = X.BTW(x - radius, circ_x, r + radius);
			bool flag2 = X.BTW(y - radius, circ_y, b + radius);
			return (flag || flag2) && ((flag && X.BTW(y, circ_y, b)) || (X.BTW(x, circ_x, r) && flag2) || X.chkLEN(x, y, circ_x, circ_y, radius) || X.chkLEN(x, b, circ_x, circ_y, radius) || X.chkLEN(r, b, circ_x, circ_y, radius) || X.chkLEN(r, y, circ_x, circ_y, radius));
		}

		public static bool chkLENLineCirc(float x1, float y1, float x2, float y2, float circ_x, float circ_y, float radius)
		{
			float num;
			return X.LENGTHXY2_LINECIRC(x1, y1, x2, y2, circ_x, circ_y, out num, radius) && num < radius * radius;
		}

		public static bool LENGTHXY2_LINECIRC(float x1, float y1, float x2, float y2, float circ_x, float circ_y, out float len2, float margin = 0f)
		{
			float num = x1 - circ_x;
			float num2 = x2 - circ_x;
			float num3 = y1 - circ_y;
			float num4 = y2 - circ_y;
			if ((num * num2 > 0f && X.Abs(num) > margin && X.Abs(num2) > margin) || (num3 * num4 > 0f && X.Abs(num3) > margin && X.Abs(num4) > margin))
			{
				len2 = -1f;
				return false;
			}
			float num5 = 0f;
			float num6 = -1f;
			float num7 = 0f;
			float num8 = 0f;
			X.calcLineGraphVariable(x1, y1, x2, y2, out num5, out num6, out num7, out num8);
			len2 = X.Pow2(num5 * circ_x + num6 * circ_y + num7) / (num5 * num5 + num6 * num6);
			return true;
		}

		public static bool isCovering(float l1, float r1, float l2, float r2, float expand = 0f)
		{
			if (expand != 0f)
			{
				float num = (l1 + r1) / 2f;
				l1 = X.Mn(l1 - expand, num);
				r1 = X.Mx(r1 + expand, num);
			}
			if (r1 - l1 < r2 - l2)
			{
				float num2 = l2;
				float num3 = l1;
				l1 = num2;
				l2 = num3;
				float num4 = r2;
				num3 = r1;
				r1 = num4;
				r2 = num3;
			}
			return X.BTW(l1, l2, r1) || X.BTWM(l1, r2, r1);
		}

		public static bool isCoveringR(float l1, float r1, float l2, float r2, float expand = 0f)
		{
			if (l1 > r1)
			{
				float num = r1;
				float num2 = l1;
				l1 = num;
				r1 = num2;
			}
			if (l2 > r2)
			{
				float num3 = r2;
				float num2 = l2;
				l2 = num3;
				r2 = num2;
			}
			return X.isCovering(l1, r1, l2, r2, expand);
		}

		public static Vector3 BorderRectAtAngleR(float w, float h, float agR)
		{
			agR = X.correctangleR(agR);
			float num = X.Cos(agR);
			float num2 = X.Sin(agR);
			float num3 = w * 0.5f;
			float num4 = h * 0.5f;
			if (num == 0f)
			{
				return new Vector3(0f, num4 * (float)X.MPF(agR > 0f), (agR > 0f) ? 1 : 3);
			}
			float num5 = X.Abs(num2 / num);
			float num6 = h / w;
			if (num5 > num6)
			{
				return new Vector3(num4 / num5 * (float)X.MPF(num > 0f), num4 * (float)X.MPF(agR > 0f), (agR > 0f) ? 1 : 3);
			}
			float num7 = (float)X.MPF(num > 0f);
			return new Vector3(num3 * num7, num3 * (float)X.MPF(num2 > 0f), (num7 > 0f) ? 2 : 0);
		}

		public static bool isContaining(float l1, float r1, float l2, float r2, float expand = 0f)
		{
			if (expand != 0f)
			{
				float num = (l1 + r1) / 2f;
				l1 = X.Mn(l1 - expand, num);
				r1 = X.Mx(r1 + expand, num);
			}
			return X.BTW(l1, l2, r1) && (l2 == r2 || X.BTWM(l1, r2, r1));
		}

		public static bool isContainingPolygon(float x, float y, List<Vector2> AP, float expand = 0f, float xyline_shift_x = 200f, float xyline_shift_y = 0f, int ARMX = -1)
		{
			if (ARMX < 0)
			{
				ARMX = AP.Count;
			}
			if (ARMX <= 1)
			{
				return false;
			}
			float num = AP[0].x;
			float num2 = AP[0].x;
			float num3 = AP[0].y;
			float num4 = AP[0].y;
			for (int i = 1; i < ARMX; i++)
			{
				Vector2 vector = AP[i];
				num = X.Mn(num, vector.x);
				num2 = X.Mx(num2, vector.x);
				num3 = X.Mn(num3, vector.y);
				num4 = X.Mx(num4, vector.y);
			}
			if (!X.isCovering(x - expand, x + expand, num, num2, 0f) || !X.isCovering(y - expand, y + expand, num3, num4, 0f))
			{
				return false;
			}
			Vector2 vector2 = AP[ARMX - 1];
			float num5 = x + xyline_shift_x;
			float num6 = y + xyline_shift_y;
			int num7 = 0;
			for (int j = 0; j < ARMX; j++)
			{
				Vector2 vector3 = AP[j];
				if (X.vec_vec_X(x, y, num5, num6, vector2.x, vector2.y, vector3.x, vector3.y))
				{
					num7++;
				}
				vector2 = vector3;
			}
			if (num7 % 2 == 1)
			{
				return true;
			}
			if (expand > 0f)
			{
				vector2 = AP[ARMX - 1];
				for (int k = 0; k < ARMX; k++)
				{
					Vector2 vector4 = AP[k];
					if (X.chkLENLineCirc(vector2.x, vector2.y, vector4.x, vector4.y, x, y, expand))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static Vector3 getUvFrom3Point(Vector2 Q, Vector2 P0, Vector2 P1, Vector2 P2, Vector2 Uv0, Vector2 Uv1, Vector2 Uv2)
		{
			Vector2 vector = P1 - P0;
			Vector2 vector2 = P2 - P0;
			if (vector.Equals(Vector2.zero) || vector2.Equals(Vector2.zero))
			{
				return new Vector3(Uv0.x, Uv0.x, -1f);
			}
			Vector2 vector3 = Q - P0;
			float num;
			float num2;
			if (vector.x == 0f)
			{
				if (vector2.x == 0f)
				{
					return new Vector3(Uv0.x, Uv0.x, -1f);
				}
				num = vector3.x / vector2.x;
				num2 = (vector3.y - vector2.y * num) / vector.y;
			}
			else if (vector.y == 0f)
			{
				if (vector2.y == 0f)
				{
					return new Vector3(Uv0.x, Uv0.x, -1f);
				}
				num = vector3.y / vector2.y;
				num2 = (vector3.x - vector2.x * num) / vector.x;
			}
			else
			{
				float num3 = vector2.y / vector.y - vector2.x / vector.x;
				if (num3 == 0f)
				{
					return new Vector3(Uv0.x, Uv0.x, -1f);
				}
				num = (vector3.y / vector.y - vector3.x / vector.x) / num3;
				num2 = (vector3.x - vector2.x * num) / vector.x;
			}
			Vector2 vector4 = Uv1 - Uv0;
			Vector2 vector5 = Uv2 - Uv0;
			return Uv0 + num2 * vector4 + num * vector5;
		}

		public static int factrial(int v, int st = 1)
		{
			if (v <= st)
			{
				return 1;
			}
			int num = 1;
			for (int i = st; i <= v; i++)
			{
				num *= i;
			}
			return num;
		}

		public static int comb(int va, int vb)
		{
			if (vb >= va || vb <= 0 || va <= 0)
			{
				return 1;
			}
			if (vb > va / 2)
			{
				vb = va - vb;
			}
			return X.factrial(va, va - vb + 1) / X.factrial(vb, 1);
		}

		public static uint comb_bits(int va, int vb)
		{
			return X.Xors.comb_bits(va, vb);
		}

		public static int beki_min(float dep)
		{
			int num = 4;
			int i = X.IntC(dep);
			while (i > num)
			{
				num *= 2;
			}
			return num;
		}

		public static int beki_min(int dep)
		{
			int num = 4;
			while (dep > num)
			{
				num *= 2;
			}
			return num;
		}

		public static int beki_cnt(uint dep)
		{
			return (int)Mathf.Log(dep, 2f);
		}

		public static int beki_cntC(uint dep)
		{
			return X.IntC(Mathf.Log(dep, 2f));
		}

		public static int max_bit(uint v)
		{
			if (v == 0U)
			{
				return -1;
			}
			for (int i = 0; i < 64; i++)
			{
				uint num = 1U << i;
				v &= ~num;
				if (v == 0U)
				{
					return i;
				}
			}
			return -1;
		}

		public static int bit_count(uint v)
		{
			if (v == 0U)
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < 64; i++)
			{
				uint num2 = 1U << i;
				uint num3 = v & ~num2;
				if (num3 != v)
				{
					v = num3;
					num++;
					if (v == 0U)
					{
						return num;
					}
				}
			}
			return num;
		}

		public static int bit_on_index(uint v, int i)
		{
			if (v == 0U)
			{
				return -1;
			}
			for (int j = 0; j < 64; j++)
			{
				uint num = 1U << j;
				uint num2 = v & ~num;
				if (num2 != v)
				{
					v = num2;
					if (i-- == 0)
					{
						return j;
					}
					if (v == 0U)
					{
						return -1;
					}
				}
			}
			return -1;
		}

		public static int bit_index(uint v, uint b)
		{
			if (v == 0U)
			{
				return -1;
			}
			int num = 0;
			for (int i = 0; i < 64; i++)
			{
				uint num2 = 1U << i;
				uint num3 = v & ~num2;
				if (num3 != v)
				{
					v = num3;
					if (b == num2)
					{
						return num;
					}
					num++;
					if (v == 0U)
					{
						return -1;
					}
				}
			}
			return -1;
		}

		public static int keta_count(int v)
		{
			int num = 0;
			if (v < 0)
			{
				v = -v;
				num++;
			}
			while (v >= 10)
			{
				v /= 10;
				num++;
			}
			return num + 1;
		}

		public static string ListUpBits(uint t, Func<uint, string> FD_uintToString = null)
		{
			if (t == 0U)
			{
				return "0";
			}
			STB stb = TX.PopBld(null, 0);
			uint num = 1U;
			while (t != 0U)
			{
				if ((t & num) != 0U)
				{
					if (stb.Length != 0)
					{
						stb += " | ";
					}
					if (FD_uintToString == null)
					{
						stb.Add(num);
					}
					else
					{
						stb.Add(FD_uintToString(num));
					}
					t &= ~num;
				}
				num <<= 1;
			}
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			return text;
		}

		public static List<T> getCombination<T>(List<T> ASrc, int vb, uint ran0, List<T> ADest = null, bool no_sort = false)
		{
			if (ADest == null)
			{
				ADest = new List<T>(vb);
			}
			if (vb <= 0)
			{
				return ADest;
			}
			if (ASrc.Count <= vb)
			{
				ADest.AddRange(ASrc);
				return ADest;
			}
			bool flag = false;
			int count = ASrc.Count;
			if (ADest.Capacity < vb)
			{
				ADest.Capacity = vb;
			}
			if (vb > count / 2)
			{
				flag = true;
				vb = count - vb;
			}
			if (X.ABufComb == null)
			{
				X.ABufComb = new List<int>(vb);
			}
			else if (X.ABufComb.Capacity < vb)
			{
				X.ABufComb.Capacity = vb;
			}
			List<int> abufComb = X.ABufComb;
			abufComb.Clear();
			int num = ASrc.Count;
			uint num2 = ran0;
			int num3 = 0;
			while (vb > 0)
			{
				num2 = X.GETRAN2(num2 + 43U + num2 % 19U, num2 % 11U + 2U);
				int num4 = (int)((float)num * X.RAN(num2, 1680 + vb * 113));
				while (--num4 >= 0)
				{
					num3++;
					while (abufComb.IndexOf(num3 % count) >= 0)
					{
						num3++;
					}
				}
				abufComb.Add(num3 % count);
				while (abufComb.IndexOf(num3 % count) >= 0)
				{
					num3++;
				}
				num3 %= count;
				num--;
				vb--;
			}
			if (no_sort && !flag)
			{
				int count2 = abufComb.Count;
				for (int i = 0; i < count2; i++)
				{
					ADest.Add(ASrc[abufComb[i]]);
				}
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					if (abufComb.IndexOf(j) >= 0 != flag)
					{
						ADest.Add(ASrc[j]);
					}
				}
			}
			return ADest;
		}

		public static List<int> getCombinationI(int va, int vb, uint ran0, List<int> ADest = null)
		{
			if (ADest == null)
			{
				if (X.ABufComb == null)
				{
					X.ABufComb = new List<int>(vb);
				}
				ADest = X.ABufComb;
			}
			ADest.Clear();
			if (vb <= 0)
			{
				return ADest;
			}
			if (va <= vb)
			{
				return null;
			}
			bool flag = false;
			if (ADest.Capacity < vb)
			{
				ADest.Capacity = vb;
			}
			if (vb > va / 2)
			{
				flag = true;
				vb = va - vb;
			}
			int num = va;
			uint num2 = ran0;
			int num3 = 0;
			while (vb > 0)
			{
				num2 = X.GETRAN2(num2 + 43U + num2 % 19U, num2 % 11U + 2U);
				int num4 = (int)((float)num * X.RAN(num2, 1680 + vb * 113));
				while (--num4 >= 0)
				{
					num3++;
					while (ADest.IndexOf(num3 % va) >= 0)
					{
						num3++;
					}
				}
				ADest.Add(num3 % va);
				while (ADest.IndexOf(num3 % va) >= 0)
				{
					num3++;
				}
				num3 %= va;
				num--;
				vb--;
			}
			if (flag)
			{
				if (ADest.Capacity < va)
				{
					ADest.Capacity = va;
				}
				num = ADest.Count;
				for (int i = 0; i < va; i++)
				{
					if (ADest.IndexOf(i) == -1)
					{
						ADest.Add(i);
					}
				}
				ADest.RemoveRange(0, num);
			}
			return ADest;
		}

		public static string basename(string filename)
		{
			if (REG.match(filename, new Regex("([^\\/]*)$")))
			{
				return REG.R1;
			}
			return filename;
		}

		public static string basenamePB(string filename)
		{
			if (REG.match(filename, new Regex("([^\\/\\\\\\¥]*)$")))
			{
				return REG.R1;
			}
			return filename;
		}

		public static string basename_noquery(string filename)
		{
			if (REG.match(filename, new Regex("\\?.*$")))
			{
				filename = REG.leftContext;
			}
			if (REG.match(filename, new Regex("([^\\/]*)$")))
			{
				return REG.R1;
			}
			return filename;
		}

		public static string basenamePB_noquery(string filename)
		{
			if (REG.match(filename, new Regex("\\?.*$")))
			{
				filename = REG.leftContext;
			}
			if (REG.match(filename, new Regex("([^\\/\\\\\\¥]*)$")))
			{
				return REG.R1;
			}
			return filename;
		}

		public static string basename_noext(string filename)
		{
			filename = X.basename_noquery(filename);
			if (REG.match(filename, new Regex("\\..*$")))
			{
				return REG.leftContext;
			}
			return filename;
		}

		public static string basenamePB_noext(string filename)
		{
			filename = X.basenamePB_noquery(filename);
			if (REG.match(filename, new Regex("\\..*$")))
			{
				return REG.leftContext;
			}
			return filename;
		}

		public static string noext(string filename)
		{
			if (REG.match(filename, new Regex("\\..*$")))
			{
				return REG.leftContext;
			}
			return filename;
		}

		public static string dirname(string filename)
		{
			if (REG.match(filename, new Regex("([^\\/]*)$")))
			{
				return REG.leftContext;
			}
			return "";
		}

		public static string dirnamePB(string filename)
		{
			if (REG.match(filename, new Regex("([^\\/\\\\\\¥]*)$")))
			{
				return REG.leftContext;
			}
			return "";
		}

		public static string upper_dirname(string filename)
		{
			if (REG.match(filename, new Regex("\\/([^\\/]*)$")))
			{
				return REG.leftContext;
			}
			return "";
		}

		public static Vector3 ROTV3e(Vector3 dary, float agR)
		{
			float x = dary.x;
			float y = dary.y;
			if (agR == 0f)
			{
				return dary;
			}
			dary.x = x * X.Cos(agR) - y * X.Sin(agR);
			dary.y = x * X.Sin(agR) + y * X.Cos(agR);
			return dary;
		}

		public static Vector2 ROTV2e(Vector2 dary, float agR)
		{
			if (agR != 0f)
			{
				return X.ROTV2e(dary, X.Cos(agR), X.Sin(agR));
			}
			return dary;
		}

		public static Vector2 ROTV2e(Vector2 dary, float _cos, float _sin)
		{
			float x = dary.x;
			float y = dary.y;
			dary.x = x * _cos - y * _sin;
			dary.y = x * _sin + y * _cos;
			return dary;
		}

		public static Matrix4x4 RotMxZXY360(float z360, float x360, float y360)
		{
			return Matrix4x4.Rotate(Quaternion.Euler(0f, y360, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(x360, 0f, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, z360));
		}

		public static float[] expandArrayFine(float expand)
		{
			return new float[] { expand, expand, expand, expand };
		}

		public static float[] expandArrayFine(params float[] Aexpand)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			switch (Aexpand.Length)
			{
			case 0:
				break;
			case 1:
				num2 = (num = (num3 = (num4 = Aexpand[0])));
				break;
			case 2:
				num4 = (num2 = Aexpand[1]);
				num3 = (num = Aexpand[0]);
				break;
			case 3:
				num2 = Aexpand[0];
				num4 = Aexpand[2];
				num3 = (num = Aexpand[1]);
				break;
			default:
				num2 = Aexpand[1];
				num4 = Aexpand[3];
				num3 = Aexpand[2];
				num = Aexpand[0];
				break;
			}
			return new float[] { num, num2, num3, num4 };
		}

		public static Vector2 XYLR(float x, float y, float l, float r)
		{
			return new Vector2(x + l * X.Cos(r), y + l * X.Sin(r));
		}

		public static float correctangle(float angle)
		{
			if (angle >= -0.5f)
			{
				return angle - (float)((int)(angle + 0.5f));
			}
			return angle + (float)((int)(0.5f - angle));
		}

		public static float correctangle360(float angle)
		{
			while (angle >= 180f)
			{
				angle -= 360f;
			}
			while (angle < -180f)
			{
				angle += 360f;
			}
			return angle;
		}

		public static float correctangleR(float angle)
		{
			while (angle >= 3.1415927f)
			{
				angle -= 6.2831855f;
			}
			while (angle < -3.1415927f)
			{
				angle += 6.2831855f;
			}
			return angle;
		}

		public static float angledif(float a1, float a2)
		{
			float num = X.correctangle(a2) - X.correctangle(a1);
			if (X.Abs(num) > X.Abs(num - 1f))
			{
				return num - 1f;
			}
			if (X.Abs(num) > X.Abs(num + 1f))
			{
				return num + 1f;
			}
			return num;
		}

		public static float angledifR(float a1, float a2)
		{
			float num = X.correctangleR(a2) - X.correctangleR(a1);
			if (X.Abs(num) > X.Abs(num - 6.2831855f))
			{
				return num - 6.2831855f;
			}
			if (X.Abs(num) > X.Abs(num + 6.2831855f))
			{
				return num + 6.2831855f;
			}
			return num;
		}

		public static float LRangleClip(float aR, float range_halfR, int is_right = -1)
		{
			float num = X.angledifR(0f, aR);
			float num2 = X.angledifR(3.1415927f, aR);
			if (is_right < 0)
			{
				is_right = ((X.Abs(num) < X.Abs(num2)) ? 1 : 0);
			}
			if (is_right > 0)
			{
				return 0f + X.absMn(num, range_halfR);
			}
			return 3.1415927f + X.absMn(num2, range_halfR);
		}

		public static bool difmpf(float a, float b)
		{
			return a != b && (a == 0f || b == 0f || a > 0f != b > 0f);
		}

		public static float COSI(float changenum, float divnum)
		{
			return X.Cos(changenum / divnum * 3.1415927f * 2f);
		}

		public static float COSIT(float divnum)
		{
			return X.Cos((float)IN.totalframe / divnum * 3.1415927f * 2f);
		}

		public static float COSIT_floating(float ratio = 1f)
		{
			return X.Cos((float)IN.totalframe / (120f * ratio) * 3.1415927f * 2f);
		}

		public static float COSIT1(float divnum)
		{
			return 0.5f + 0.5f * X.Cos((float)IN.totalframe / divnum * 3.1415927f * 2f);
		}

		public static float SINI(float changenum, float divnum)
		{
			return X.Sin(changenum / divnum * 3.1415927f * 2f);
		}

		public static float SINIT(float divnum)
		{
			return X.Sin((float)IN.totalframe / divnum * 3.1415927f * 2f);
		}

		public static float ZIGZAGI(float changenum, float divnum)
		{
			changenum %= divnum;
			float num = divnum * 0.5f;
			return X.ZLINE(changenum, num) - X.ZLINE(changenum - num, num);
		}

		public static float BOUNCE(float t, float in_air_time)
		{
			if (t <= 0f || t >= in_air_time)
			{
				return 0f;
			}
			return -X.Pow(2f * t / in_air_time - 1f, 2) + 1f;
		}

		public static bool BTW(float num1, float hensu, float num2)
		{
			return num1 <= hensu && hensu < num2;
		}

		public static bool BTWRV(float num1, float hensu, float num2)
		{
			if (num1 >= num2)
			{
				return num2 <= hensu && hensu < num1;
			}
			return num1 <= hensu && hensu < num2;
		}

		public static bool BTWWRV(float num1, float hensu, float num2)
		{
			if (num1 >= num2)
			{
				return num2 <= hensu && hensu <= num1;
			}
			return num1 <= hensu && hensu <= num2;
		}

		public static bool BTWM(float num1, float hensu, float num2)
		{
			return num1 < hensu && hensu <= num2;
		}

		public static bool BTWW(float num1, float hensu, float num2)
		{
			return num1 <= hensu && hensu <= num2;
		}

		public static bool BTWS(float num1, float hensu, float num2)
		{
			return num1 < hensu && hensu < num2;
		}

		public static bool ITV(int num_int, int interval)
		{
			return num_int % interval == 0;
		}

		public static float ITVL(int num_int, int interval)
		{
			return (float)(num_int % interval) / (float)interval;
		}

		public static int ANM(int genzai, int maisu, float speed)
		{
			return (int)((float)genzai % (speed * (float)maisu) / speed);
		}

		public static int ANMT(int maisu, float speed)
		{
			return (int)((float)IN.totalframe % (speed * (float)maisu) / speed);
		}

		public static float ANMP(int genzai, int speed, float _max = 1f)
		{
			return (float)(genzai % speed) / (float)speed * _max;
		}

		public static float ANMPT(int speed = 36, float _max = 1f)
		{
			return (float)(IN.totalframe % speed) / (float)speed * _max;
		}

		public static int ANML(int genzai, int maisu, int speed, int loopto = 0)
		{
			return X.Mx(0, genzai / speed - loopto) % (maisu - loopto) + X.Mn(genzai / speed, loopto);
		}

		public static uint xors()
		{
			return X.Xors.get0();
		}

		public static int xors(int i)
		{
			return (int)(X.Xors.get0() % (uint)i);
		}

		public static int xorsi()
		{
			return (int)(X.Xors.get0() & 2147483647U);
		}

		public static uint checksum(string s, int multiply = 1)
		{
			uint num = 0U;
			int length = s.Length;
			for (int i = 0; i < length; i++)
			{
				num += (uint)((uint)s[i] << (i & 7 & 31));
			}
			return (uint)((ulong)num * (ulong)((long)multiply));
		}

		public static uint randA(uint i)
		{
			return X.Xors.randA[(int)(i & 127U)] & 134217727U;
		}

		public static float ranNm(float f1, float r)
		{
			return f1 + r * (-1f + 2f * X.XORSP());
		}

		public static float XORSP()
		{
			return X.Xors.getP();
		}

		public static float XORSPS()
		{
			return (X.XORSP() - 0.5f) * 2f;
		}

		public static float XORSPSam(float am)
		{
			if (am >= 0f)
			{
				return 0f;
			}
			float num = (X.XORSP() - am - (0.5f - am * 0.5f)) * 2f;
			return num + (float)((num > 0f) ? 1 : (-1)) * am;
		}

		public static float XORSPSH()
		{
			return X.XORSP() - 0.5f;
		}

		public static uint GETRAN2(int seed1, int seed2)
		{
			return X.Xors.get2((uint)seed1, (uint)seed2);
		}

		public static uint GETRAN3(int seed1, int seedi, int i_add = 517961)
		{
			return (X.Xors.get2((uint)seed1, (uint)(seedi * 7)) + (uint)(i_add * seedi)) & 134217727U;
		}

		public static uint GETRAN2(uint seed1, uint seed2)
		{
			return X.Xors.get2(seed1, seed2);
		}

		public static bool isExistBoolean(int x, int y, bool[,] Ab)
		{
			return Ab[x, y];
		}

		public static int isinStr(string[] V, string val, int len = -1)
		{
			if (len < 0)
			{
				len = V.Length;
			}
			for (int i = 0; i < len; i++)
			{
				if (val == V[i])
				{
					return i;
				}
			}
			return -1;
		}

		public static int isinStr(List<string> V, string val, int len = -1)
		{
			if (len < 0)
			{
				len = V.Count;
			}
			for (int i = 0; i < len; i++)
			{
				if (val == V[i])
				{
					return i;
				}
			}
			return -1;
		}

		public static int isinCP<T>(T[] V, T val, int len) where T : IComparable
		{
			for (int i = 0; i < len; i++)
			{
				if (val.Equals(V[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static int isinS<T>(T[] V, T val) where T : struct
		{
			int num = V.Length;
			for (int i = 0; i < num; i++)
			{
				if (val.Equals(V[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static int isinS<T>(T[] V, T val, int len) where T : struct
		{
			for (int i = 0; i < len; i++)
			{
				if (val.Equals(V[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static int isina<T>(T[] V, Func<T, bool> FnCheck, int len = -1)
		{
			if (len < 0)
			{
				len = V.Length;
			}
			for (int i = 0; i < len; i++)
			{
				if (FnCheck(V[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static int isinC<T>(T[] V, T val) where T : class
		{
			if (typeof(T) == typeof(string))
			{
				return X.isinStr(V as string[], val as string, -1);
			}
			int num = V.Length;
			for (int i = 0; i < num; i++)
			{
				if (V[i] == val)
				{
					return i;
				}
			}
			return -1;
		}

		public static int isinC<T>(List<T> V, T val) where T : class
		{
			if (typeof(T) == typeof(string))
			{
				return X.isinStr(V as string[], val as string, -1);
			}
			int count = V.Count;
			for (int i = 0; i < count; i++)
			{
				if (V[i] == val)
				{
					return i;
				}
			}
			return -1;
		}

		public static int isinC<T>(T[] V, T val, int len) where T : class
		{
			if (typeof(T) == typeof(string))
			{
				return X.isinStr(V as string[], val as string, len);
			}
			for (int i = 0; i < len; i++)
			{
				if (V[i] == val)
				{
					return i;
				}
			}
			return -1;
		}

		public static T2[] map<T, T2>(T[] V, Func<T, T2> FnMap)
		{
			int num = V.Length;
			T2[] array = new T2[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = FnMap(V[i]);
			}
			return array;
		}

		public static T getHighest<T>(T[] V, Func<T, float> FnCalcScore, int len = -1, float thresh_num = -1000f) where T : class
		{
			if (len < 0)
			{
				len = V.Length;
			}
			if (len == 0)
			{
				return default(T);
			}
			T t = default(T);
			float num = 0f;
			for (int i = 0; i < len; i++)
			{
				T t2 = V[i];
				float num2 = FnCalcScore(t2);
				if (num2 != thresh_num && (t == null || num2 > num))
				{
					num = num2;
					t = t2;
				}
			}
			return t;
		}

		public static T getLowest<T>(T[] V, Func<T, float> FnCalcScore, int len = -1, float thresh_num = -1000f) where T : class
		{
			if (len < 0)
			{
				len = V.Length;
			}
			if (len == 0)
			{
				return default(T);
			}
			T t = default(T);
			float num = 0f;
			for (int i = 0; i < len; i++)
			{
				T t2 = V[i];
				float num2 = FnCalcScore(t2);
				if (num2 != thresh_num && (t == null || num2 < num))
				{
					num = num2;
					t = t2;
				}
			}
			return t;
		}

		public static void Each<T>(T[] V, Action<T, int> FnMap, int len = -1)
		{
			if (len < 0)
			{
				len = V.Length;
			}
			for (int i = 0; i < len; i++)
			{
				FnMap(V[i], i);
			}
		}

		public static T2[] map<T, T2>(List<T> V, Func<T, T2> FnMap)
		{
			int count = V.Count;
			T2[] array = new T2[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = FnMap(V[i]);
			}
			return array;
		}

		public static T isinOC<T, Tw>(BDic<T, Tw> O, Tw val) where T : class where Tw : class
		{
			foreach (KeyValuePair<T, Tw> keyValuePair in O)
			{
				if (keyValuePair.Value == val)
				{
					return keyValuePair.Key;
				}
			}
			return default(T);
		}

		public static T[] objKeys<T, T2>(BDic<T, T2> O)
		{
			T[] array;
			using (BList<T> blist = X.objKeysB<T, T2>(O))
			{
				array = blist.ToArray();
			}
			return array;
		}

		public static BList<T> objKeysB<T, T2>(BDic<T, T2> O)
		{
			BList<T> blist = ListBuffer<T>.Pop(O.Count);
			foreach (KeyValuePair<T, T2> keyValuePair in O)
			{
				blist.Add(keyValuePair.Key);
			}
			return blist;
		}

		public static List<T> objKeys<T, T2>(BDic<T, T2> O, List<T> Adest)
		{
			if (Adest.Capacity < O.Count + Adest.Count)
			{
				Adest.Capacity = O.Count + Adest.Count;
			}
			foreach (KeyValuePair<T, T2> keyValuePair in O)
			{
				Adest.Add(keyValuePair.Key);
			}
			return Adest;
		}

		public static BDic<T, T2> objMerge<T, T2>(BDic<T, T2> Dest, BDic<T, T2> Src, bool no_overwrite = false)
		{
			if (Src == null)
			{
				return Dest;
			}
			if (Dest == null)
			{
				Dest = new BDic<T, T2>();
				no_overwrite = false;
			}
			foreach (KeyValuePair<T, T2> keyValuePair in Src)
			{
				if (!no_overwrite || !Dest.ContainsKey(keyValuePair.Key))
				{
					Dest[keyValuePair.Key] = keyValuePair.Value;
				}
			}
			return Dest;
		}

		public static T[] copyWithoutEmpty<T>(T[] A, T[] Aadd = null)
		{
			return X.concat<T>(A, Aadd, X.countNotEmpty<T>(A), (Aadd == null) ? 0 : X.countNotEmpty<T>(Aadd));
		}

		public static T[] concat<T>(T[] A, T[] Aadd = null, int cnt = -1, int cntb = -1)
		{
			cnt = ((cnt < 0) ? A.Length : cnt);
			T[] array;
			if (Aadd == null)
			{
				array = new T[cnt];
				Array.Copy(A, 0, array, 0, cnt);
			}
			else
			{
				cntb = ((cntb < 0) ? Aadd.Length : cntb);
				array = new T[cnt + cntb];
				Array.Copy(A, 0, array, 0, cnt);
				Array.Copy(Aadd, 0, array, cnt, cntb);
			}
			return array;
		}

		public static T[,] concat<T>(T[,] A)
		{
			int length = A.Length;
			T[,] array = new T[A.GetLength(0), A.GetLength(1)];
			Array.Copy(A, 0, array, 0, length);
			return array;
		}

		public static T2[] concatCast<T, T2>(T[] A) where T2 : T
		{
			int num = A.Length;
			T2[] array = new T2[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (T2)((object)A[i]);
			}
			return array;
		}

		public static T[] subtract<T>(T[] A, int src_index, int cnt)
		{
			src_index = X.Mn(src_index, A.Length);
			cnt = X.Mn(A.Length - src_index, cnt);
			T[] array = new T[cnt];
			if (src_index < A.Length)
			{
				Array.Copy(A, src_index, array, 0, cnt);
			}
			return array;
		}

		public static T[] subtract<T>(T[] A, int src_index)
		{
			return X.subtract<T>(A, src_index, A.Length - src_index);
		}

		public static T[] slice<T>(T[] A, int src_index, int after)
		{
			if (after < 0)
			{
				after = A.Length + after;
			}
			return X.subtract<T>(A, src_index, after - src_index);
		}

		public static T[] slice<T>(T[] A, int src_index)
		{
			return X.subtract<T>(A, src_index, A.Length - src_index);
		}

		public static void splice<T>(ref T[] A, int s, int cnt)
		{
			int num = A.Length;
			if (s < 0)
			{
				cnt += s;
				s = 0;
			}
			cnt = X.Mn(num - s, cnt);
			T[] array = new T[num - cnt];
			for (int i = 0; i < s; i++)
			{
				array[i] = A[i];
			}
			for (int i = s + cnt; i < num; i++)
			{
				array[i - cnt] = A[i];
			}
			A = array;
		}

		public static void spliceEmpty<T>(T[] A, int s, int cnt) where T : class
		{
			int num = A.Length;
			if (s < 0)
			{
				cnt += s;
				s = 0;
			}
			cnt = X.Mn(num - s, cnt);
			for (int i = s + cnt; i < num; i++)
			{
				A[i - cnt] = A[i];
			}
			for (int j = num - cnt; j < num; j++)
			{
				A[j] = default(T);
			}
		}

		public static void spliceStruct<T>(T[] A, int s, int cnt) where T : struct
		{
			int num = A.Length;
			if (s < 0)
			{
				cnt += s;
				s = 0;
			}
			cnt = X.Mn(num - s, cnt);
			for (int i = s + cnt; i < num; i++)
			{
				A[i - cnt] = A[i];
			}
		}

		public static T[] clrA<T>(T[] A) where T : class
		{
			if (A == null)
			{
				return null;
			}
			for (int i = A.Length - 1; i >= 0; i--)
			{
				A[i] = default(T);
			}
			return null;
		}

		public static void push<T>(ref T[] A, T Item, int pos = -1)
		{
			if (A == null)
			{
				A = new T[] { Item };
				return;
			}
			int num = A.Length;
			Array.Resize<T>(ref A, num + 1);
			pos = ((pos < 0) ? (num + 1 + pos) : pos);
			int i = num - 1;
			while (i >= pos)
			{
				A[i + 1] = A[i--];
			}
			A[pos] = Item;
		}

		public static T pop<T>(ref T[] A)
		{
			int num = A.Length;
			if (num == 0)
			{
				throw new ArgumentException("X::pop - range error");
			}
			T t = A[num - 1];
			Array.Resize<T>(ref A, num - 1);
			return t;
		}

		public static T pop<T>(List<T> A)
		{
			int count = A.Count;
			if (count == 0)
			{
				throw new ArgumentException("X::pop - range error");
			}
			T t = A[count - 1];
			A.RemoveAt(count - 1);
			return t;
		}

		public static void AddDef<T>(List<T> A, T Item, int cnt = 1)
		{
			while (--cnt >= 0)
			{
				A.Add(Item);
			}
		}

		public static void pushA<T>(ref T[] A, T[] AItem)
		{
			int num = AItem.Length;
			int num2 = 0;
			if (A == null)
			{
				A = new T[num];
			}
			else
			{
				num2 = A.Length;
				Array.Resize<T>(ref A, num2 + num);
			}
			for (int i = 0; i < num; i++)
			{
				A[num2 + i] = AItem[i];
			}
		}

		public static void pushA<T>(ref T[] A, T[] AItem, int dest_max, int src_max)
		{
			int num;
			if (A == null)
			{
				num = src_max;
				A = new T[src_max];
				dest_max = 0;
			}
			else
			{
				num = dest_max + src_max;
				if (A.Length < num)
				{
					Array.Resize<T>(ref A, num);
				}
			}
			int num2 = 0;
			while (dest_max < num)
			{
				A[dest_max] = AItem[num2++];
				dest_max++;
			}
		}

		public static List<T> ToList<T>(T[] A)
		{
			List<T> list = new List<T>(A.Length);
			list.AddRange(A);
			return list;
		}

		public static void pushAConvert<T, T2>(ref T[] A, T2[] AItem) where T : class, T2 where T2 : class
		{
			int num = AItem.Length;
			int num2 = 0;
			if (A == null)
			{
				A = new T[num];
			}
			else
			{
				num2 = A.Length;
				Array.Resize<T>(ref A, num2 + num);
			}
			for (int i = 0; i < num; i++)
			{
				A[num2 + i] = AItem[i] as T;
			}
		}

		public static int pushToEmpty<T>(T[] A, T Item, int count = 1) where T : class
		{
			if (A == null)
			{
				return count;
			}
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				if (A[i] == null)
				{
					IL_004E:
					while (i < num && count > 0)
					{
						A[i++] = Item;
						count--;
						while (i < num && A[i] != null)
						{
							i++;
						}
					}
					return count;
				}
			}
			goto IL_004E;
		}

		public static bool emptySpecific<T>(T[] A, T Item, int a_len = -1) where T : class
		{
			if (A == null)
			{
				return false;
			}
			int num = ((a_len >= 0) ? a_len : A.Length);
			for (int i = 0; i < num; i++)
			{
				if (A[i] == Item)
				{
					X.shiftEmpty<T>(A, 1, i, -1);
					return true;
				}
			}
			return false;
		}

		public static int emptySpecificA<T>(T[] A, T[] Item, int i_len = -1) where T : class
		{
			if (A == null)
			{
				return 0;
			}
			int num = ((i_len >= 0) ? i_len : Item.Length);
			int num2 = X.countNotEmpty<T>(A);
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				if (Item[i] != null && X.emptySpecific<T>(A, Item[i], num2 - num3))
				{
					num3++;
				}
			}
			return num3;
		}

		public static int removeSpecificA<T>(List<T> A, T[] AItem) where T : class
		{
			if (A == null)
			{
				return 0;
			}
			int num = 0;
			int num2 = AItem.Length;
			for (int i = 0; i < num2; i++)
			{
				T t = AItem[i];
				if (t != null)
				{
					int num3 = A.IndexOf(t);
					if (num3 >= 0)
					{
						A.RemoveAt(num3);
						num++;
					}
				}
			}
			return num;
		}

		public static int removeSpecificA<T>(List<T> A, List<T> AItem) where T : class
		{
			if (A == null)
			{
				return 0;
			}
			int num = 0;
			int count = AItem.Count;
			for (int i = 0; i < count; i++)
			{
				T t = AItem[i];
				if (t != null)
				{
					int num2 = A.IndexOf(t);
					if (num2 >= 0)
					{
						A.RemoveAt(num2);
						num++;
					}
				}
			}
			return num;
		}

		public static bool pushToEmptyRI<T>(ref T[] A, T Item, int len = -1) where T : class
		{
			int num = ((len < 0) ? A.Length : len);
			int i;
			for (i = 0; i < num; i++)
			{
				if (A[i] == Item)
				{
					return false;
				}
				if (A[i] == null)
				{
					A[i] = Item;
					return true;
				}
			}
			if (i >= A.Length)
			{
				Array.Resize<T>(ref A, X.Mx(2, A.Length) * 2);
			}
			A[i] = Item;
			return true;
		}

		public static bool pushToEmptyS<T>(ref T[] A, T Item, ref int seek_start, int max_slip = 16)
		{
			int num = A.Length;
			if (seek_start >= num)
			{
				Array.Resize<T>(ref A, max_slip + seek_start);
			}
			T[] array = A;
			int num2 = seek_start;
			seek_start = num2 + 1;
			array[num2] = Item;
			return true;
		}

		public static bool pushToEmptySA<T>(ref T[] A, T[] Item, ref int seek_start, int max_slip = 16, int item_len = -1)
		{
			int num = ((item_len == -1) ? Item.Length : item_len);
			int num2 = seek_start + num;
			if (num2 >= num)
			{
				Array.Resize<T>(ref A, max_slip + num2);
			}
			for (int i = 0; i < num; i++)
			{
				T t = Item[i];
				if (t != null)
				{
					T[] array = A;
					int num3 = seek_start;
					seek_start = num3 + 1;
					array[num3] = t;
				}
			}
			return true;
		}

		public static bool pushToEmptyR<T>(ref T[] A, T Item, int seek_start = 0) where T : class
		{
			int num = A.Length;
			for (int i = seek_start; i < num; i++)
			{
				if (A[i] == null)
				{
					A[i] = Item;
					return true;
				}
			}
			Array.Resize<T>(ref A, X.Mx(2, num) * 2);
			A[num] = Item;
			return true;
		}

		public static bool unshiftToEmptyR<T>(ref T[] A, T Item, int pos = 0) where T : class
		{
			int num = A.Length;
			for (int i = pos; i < num; i++)
			{
				if (A[i] == null)
				{
					for (int j = i - 1; j >= pos; j--)
					{
						A[j + 1] = A[j];
					}
					A[pos] = Item;
					return true;
				}
			}
			Array.Resize<T>(ref A, X.Mx(2, num) * 2);
			for (int k = num - 1; k >= pos; k++)
			{
				A[k + 1] = A[k];
			}
			A[pos] = Item;
			return true;
		}

		public static bool unshiftToEmptyR(ref string[] A, string Item, int pos = 0)
		{
			int num = A.Length;
			for (int i = pos; i < num; i++)
			{
				if (A[i] == null)
				{
					for (int j = i - 1; j >= pos; j--)
					{
						A[j + 1] = A[j];
					}
					A[pos] = Item;
					return true;
				}
			}
			Array.Resize<string>(ref A, X.Mx(2, num) * 2);
			for (int k = num - 1; k >= pos; k++)
			{
				A[k + 1] = A[k];
			}
			A[pos] = Item;
			return true;
		}

		public static int countNotEmpty<T>(T[] A)
		{
			int num = A.Length;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				if (A[i] == null)
				{
					return num2;
				}
				num2++;
			}
			return num2;
		}

		public static void unshiftToEmptyA<T>(T[] A, T[] AItem) where T : class
		{
			int num = X.countNotEmpty<T>(A);
			int num2 = X.countNotEmpty<T>(AItem);
			int num3 = A.Length - num2;
			for (int i = num - 1; i >= 0; i--)
			{
				if (i < num3)
				{
					A[i + num2] = A[i];
				}
			}
			for (int j = num2 - 1; j >= 0; j--)
			{
				A[j] = AItem[j];
			}
		}

		public static int pushToEmptyA<T>(T[] A, T[] AItem) where T : class
		{
			int num = A.Length;
			int num2 = X.countNotEmpty<T>(AItem);
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				if (A[i] == null)
				{
					IL_0065:
					while (i < num && num3 < num2)
					{
						if (AItem[num3] != null)
						{
							A[i++] = AItem[num3];
							while (i < num && A[i] != null)
							{
								i++;
							}
						}
						num3++;
					}
					return num2 - num3;
				}
			}
			goto IL_0065;
		}

		public static T shift<T>(ref T[] A, int count = 1)
		{
			int num = A.Length;
			T t = A[count - 1];
			for (int i = count; i < num; i++)
			{
				A[i - count] = A[i];
			}
			Array.Resize<T>(ref A, X.Mx(0, num - count));
			return t;
		}

		public static void shiftEmpty<T>(T[] A, int count = 1, int first = 0, int ARMX = -1)
		{
			if (ARMX < 0)
			{
				ARMX = A.Length;
			}
			count = X.Mn(ARMX - first, count);
			if (count <= 0)
			{
				return;
			}
			int num = ARMX - count;
			int num2 = count + 1;
			for (int i = count + first; i < ARMX; i++)
			{
				T t = A[i];
				A[i - count] = t;
				if (t == null && --num2 <= 0)
				{
					return;
				}
			}
			for (int j = num; j < ARMX; j++)
			{
				A[j] = default(T);
			}
		}

		public static T shiftNotInput1<T>(T[] A, int first, ref int ARMX)
		{
			T t = A[first];
			X.shiftNotInput<T>(A, 1, first, ARMX);
			int num = ARMX - 1;
			ARMX = num;
			A[num] = t;
			return t;
		}

		public static T shiftNotInput<T>(T[] A, int count = 1, int first = 0, int ARMX = -1)
		{
			T t = A[first + count - 1];
			if (ARMX < 0)
			{
				ARMX = A.Length;
			}
			int num = count + 1;
			for (int i = count + first; i < ARMX; i++)
			{
				T t2 = A[i];
				A[i - count] = t2;
				if (t2 == null && --num <= 0)
				{
					return t;
				}
			}
			return t;
		}

		public static void shiftNotInput<T>(List<T> A, int count, int first, int ARMX)
		{
			if (ARMX < 0)
			{
				ARMX = A.Count;
			}
			int num = count + 1;
			for (int i = count + first; i < ARMX; i++)
			{
				T t = A[i];
				A[i - count] = t;
				if (t == null && --num <= 0)
				{
					return;
				}
			}
		}

		public static void unshift<T>(ref T[] A, T val, int pos = 0)
		{
			int num = A.Length;
			T[] array = new T[num + 1];
			for (int i = 0; i < pos; i++)
			{
				array[i] = A[i];
			}
			for (int j = pos; j < num; j++)
			{
				array[j + 1] = A[j];
			}
			A = array;
			A[pos] = val;
		}

		public static void unshiftEmpty<T>(T[] A, T val, int pos = 0, int cnt = 1, int ARMX = -1)
		{
			if (ARMX < 0)
			{
				ARMX = X.countNotEmpty<T>(A);
			}
			int num = A.Length - cnt;
			for (int i = ARMX - 1; i >= pos; i--)
			{
				if (i < num)
				{
					A[i + cnt] = A[i];
				}
			}
			for (int j = pos + cnt - 1; j >= pos; j--)
			{
				A[j] = val;
			}
		}

		public static void unshiftEmptyA<T>(T[] A, T[] AItem, int pos = 0) where T : class
		{
			int num = X.countNotEmpty<T>(A);
			int num2 = X.countNotEmpty<T>(AItem);
			int num3 = A.Length - num2;
			for (int i = num - 1; i >= pos; i--)
			{
				if (i < num3)
				{
					A[i + num2] = A[i];
				}
			}
			num2 = X.Mn(num2, A.Length - pos);
			for (int j = 0; j < num2; j++)
			{
				A[j + pos] = AItem[j];
			}
		}

		public static void shuffle<T>(T[] A, int arraymax = -1)
		{
			arraymax = ((arraymax < 0) ? A.Length : arraymax);
			int num = (arraymax - 1) * 23;
			for (int i = 0; i < num; i++)
			{
				int num2 = Random.Range(0, arraymax);
				int num3 = Random.Range(0, arraymax);
				int num4 = num2;
				int num5 = num3;
				T t = A[num3];
				T t2 = A[num2];
				A[num4] = t;
				A[num5] = t2;
			}
		}

		public static void shuffle<T>(List<T> A, int arraymax = -1, XorsMaker Xors = null)
		{
			arraymax = ((arraymax < 0) ? A.Count : arraymax);
			int num = (arraymax - 1) * 23;
			for (int i = 0; i < num; i++)
			{
				int num2 = ((Xors != null) ? ((int)((ulong)Xors.get0() % (ulong)((long)arraymax))) : Random.Range(0, arraymax));
				int num3 = ((Xors != null) ? ((int)((ulong)Xors.get0() % (ulong)((long)arraymax))) : Random.Range(0, arraymax));
				int num4 = num2;
				int num5 = num3;
				T t = A[num3];
				T t2 = A[num2];
				A[num4] = t;
				A[num5] = t2;
			}
		}

		public static void shuffleEmpty<T>(T[] A) where T : class
		{
			for (int i = A.Length - 1; i >= 1; i--)
			{
				if (A[i] != null)
				{
					X.shuffle<T>(A, i + 1);
					return;
				}
			}
		}

		public static void reverse<T>(T[] A, int arraymax = -1)
		{
			arraymax = ((arraymax < 0) ? A.Length : arraymax);
			int num = arraymax / 2;
			for (int i = 0; i < num; i++)
			{
				int num2 = i;
				int num3 = arraymax - 1 - i;
				T t = A[arraymax - 1 - i];
				T t2 = A[i];
				A[num2] = t;
				A[num3] = t2;
			}
		}

		public static int removeDupe<T>(List<T> A) where T : class
		{
			int num = A.Count;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				T t = A[i];
				for (int j = num - 1; j > i; j--)
				{
					if (t == A[j])
					{
						A.RemoveAt(j);
						num--;
						num2++;
					}
				}
			}
			return num2;
		}

		public static T[] copyTwoDimensionArray<T>(T[] Dest, int dclms, int drows, T[] Src, int sclms, int srows, bool fill_outside)
		{
			if (Src == null || sclms == 0 || srows == 0)
			{
				return Dest;
			}
			if (fill_outside)
			{
				for (int i = 0; i < drows; i++)
				{
					int num = ((i < srows) ? i : (srows - 1));
					for (int j = 0; j < dclms; j++)
					{
						int num2 = ((j < sclms) ? j : (sclms - 1));
						Dest[i * dclms + j] = Src[num * sclms + num2];
					}
				}
			}
			else
			{
				int num3 = X.Mn(dclms, sclms);
				int num4 = X.Mn(drows, srows);
				for (int k = 0; k < num4; k++)
				{
					for (int l = 0; l < num3; l++)
					{
						try
						{
							Dest[k * dclms + l] = Src[k * sclms + l];
						}
						catch
						{
						}
					}
				}
			}
			return Dest;
		}

		public static T[,] copyTwoDimensionArray<T>(T[,] Dest, int depx, int depy, T[,] Src, int srcx = 0, int srcy = 0)
		{
			if (Src == null)
			{
				return Dest;
			}
			int num = X.Mn(Dest.GetLength(0) - depx, Src.GetLength(0) - srcx);
			int num2 = X.Mn(Dest.GetLength(1) - depy, Src.GetLength(1) - srcy);
			int num3 = depy;
			int num4 = srcy;
			int i = 0;
			while (i < num2)
			{
				if (num3 >= 0 && num3 < Dest.GetLength(1) && num4 >= 0)
				{
					int num5 = depx;
					int num6 = srcx;
					int j = 0;
					while (j < num)
					{
						if (num5 >= 0 && num5 < Dest.GetLength(0) && num6 >= 0)
						{
							Dest[num5, num3] = Src[num6, num4];
						}
						j++;
						num5++;
						num6++;
					}
				}
				i++;
				num3++;
				num4++;
			}
			return Dest;
		}

		public static float sum(float[] A)
		{
			float num = 0f;
			int num2 = A.Length;
			for (int i = 0; i < num2; i++)
			{
				num += A[i];
			}
			return num;
		}

		public static float sum(List<float> A)
		{
			float num = 0f;
			int count = A.Count;
			for (int i = 0; i < count; i++)
			{
				num += A[i];
			}
			return num;
		}

		public static int sum(int[] A)
		{
			int num = 0;
			int num2 = A.Length;
			for (int i = 0; i < num2; i++)
			{
				num += A[i];
			}
			return num;
		}

		public static int sum(byte[] A)
		{
			int num = 0;
			int num2 = A.Length;
			for (int i = 0; i < num2; i++)
			{
				num += (int)A[i];
			}
			return num;
		}

		public static byte get_max(byte[] A)
		{
			byte b = 0;
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				b = ((b < A[i]) ? A[i] : b);
			}
			return b;
		}

		public static int sumBits(int[] A)
		{
			int num = 0;
			int num2 = A.Length;
			for (int i = 0; i < num2; i++)
			{
				num |= 1 << A[i];
			}
			return num;
		}

		public static int sumBits(string[] A)
		{
			int num = 0;
			int num2 = A.Length;
			for (int i = 0; i < num2; i++)
			{
				int num3 = X.NmI(A[i], -1, true, false);
				if (num3 >= 0)
				{
					num |= 1 << num3;
				}
			}
			return num;
		}

		public static T[] makeEmptyArray<T>(int num, T youso, T[] concatArray = null)
		{
			T[] array = new T[num + ((concatArray != null) ? concatArray.Length : 0)];
			for (int i = 0; i < num; i++)
			{
				array[i] = youso;
			}
			if (concatArray != null)
			{
				int num2 = concatArray.Length;
				for (int i = 0; i < num2; i++)
				{
					array[i + num] = concatArray[i];
				}
			}
			return array;
		}

		public static int[] makeCountUpArray(int num, int start = 0, int cup = 1)
		{
			if (num < 0)
			{
				num = (int)((float)(-(float)num - start + 1) / (float)cup);
			}
			int[] array = new int[num];
			int num2 = 0;
			while (num > 0)
			{
				num--;
				array[num2++] = start;
				start += cup;
			}
			return array;
		}

		public static List<int> makeCountUpArray(List<int> Ar, int num, int start = 0, int cup = 1)
		{
			if (num < 0)
			{
				num = (int)((float)(-(float)num - start + 1) / (float)cup);
			}
			if (Ar.Capacity < Ar.Count + num)
			{
				Ar.Capacity = Ar.Count + num;
			}
			while (num > 0)
			{
				num--;
				Ar.Add(start);
				start += cup;
			}
			return Ar;
		}

		public static string[] makeToStringed<T>(T[] A)
		{
			int num = A.Length;
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = A[i].ToString();
			}
			return array;
		}

		public static T[] ALLV<T>(T[] A, T v)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = v;
			}
			return A;
		}

		public static T[] ALLN<T>(T[] A) where T : class
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = default(T);
			}
			return A;
		}

		public static float[] ALLM1(float[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = -1f;
			}
			return A;
		}

		public static int[] ALLM1(int[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = -1;
			}
			return A;
		}

		public static float[] ALL0(float[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = 0f;
			}
			return A;
		}

		public static ushort[] ALL0(ushort[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = 0;
			}
			return A;
		}

		public static Vector2[] ALL0(Vector2[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = Vector2.zero;
			}
			return A;
		}

		public static int[] ALL0(int[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = 0;
			}
			return A;
		}

		public static byte[] ALL0(byte[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = 0;
			}
			return A;
		}

		public static uint[] ALL0(uint[] A)
		{
			int num = A.Length;
			for (int i = 0; i < num; i++)
			{
				A[i] = 0U;
			}
			return A;
		}

		public static List<int> ALL0(List<int> A)
		{
			int count = A.Count;
			for (int i = 0; i < count; i++)
			{
				A[i] = 0;
			}
			return A;
		}

		public static string K2T(string k, string t)
		{
			X.OKey2Text[k] = t;
			return k;
		}

		public static string K2TTX(string k)
		{
			X.OKey2Text[k] = TX.Get(k, k);
			return k;
		}

		public static string T2K(string k)
		{
			return X.GetS<string, string>(X.OKey2Text, k, k);
		}

		public static string T2K(string k, string def)
		{
			return X.GetS<string, string>(X.OKey2Text, k, def);
		}

		public static int SortStringWithNumber(string a, string b, int start_si = 0)
		{
			if (a == b)
			{
				return 0;
			}
			if (a == null || b == null)
			{
				if (a != null)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				int length = a.Length;
				int length2 = b.Length;
				int num = X.Mn(length, length2);
				int i = start_si;
				while (i < num)
				{
					char c = a[i];
					char c2 = b[i];
					if (c != c2)
					{
						float num2;
						bool flag = TX.Nm(a, out num2, i, -1);
						float num3;
						bool flag2 = TX.Nm(b, out num3, i, -1);
						if (!flag || !flag2)
						{
							if (c >= c2)
							{
								return 1;
							}
							return -1;
						}
						else
						{
							if (num2 >= num3)
							{
								return 1;
							}
							return -1;
						}
					}
					else
					{
						i++;
					}
				}
				if (length >= length2)
				{
					return 1;
				}
				return -1;
			}
		}

		public static string fineIndividualName<T>(T[] AMtr, string newname, T MyMtr = default(T)) where T : class, IIdvName
		{
			string text;
			using (BList<T> blist = ListBuffer<T>.Pop(AMtr.Length))
			{
				blist.AddRange(AMtr);
				text = X.fineIndividualName<T>(blist, newname, MyMtr);
			}
			return text;
		}

		public static string fineIndividualName<T>(List<T> AMtr, string newname, T MyMtr = default(T)) where T : class, IIdvName
		{
			string text = "Layer";
			int num = 1;
			bool flag = true;
			bool flag2 = true;
			int count = AMtr.Count;
			if (newname == "")
			{
				newname = text;
			}
			string text2 = newname;
			if (count <= 0)
			{
				return newname;
			}
			if (REG.match(newname, REG.RegSuffixNumber))
			{
				newname = REG.leftContext;
				num = X.NmI(REG.R1, 0, false, false);
			}
			string text3 = newname;
			while (flag2)
			{
				flag2 = false;
				text3 = (flag ? text2 : (((newname != "") ? newname : text) + ((num > 0) ? ("_" + num.ToString()) : "")));
				flag = false;
				for (int i = 0; i < count; i++)
				{
					if (AMtr[i] != MyMtr && AMtr[i] != null && AMtr[i].get_individual_key() == text3)
					{
						num++;
						flag2 = true;
						break;
					}
				}
			}
			return text3;
		}

		public static string fineIndividualName<T>(BDic<string, T> OMtr, string newname, T MyMtr = default(T)) where T : class, IIdvName
		{
			string text = "Layer";
			int num = 1;
			bool flag = true;
			bool flag2 = true;
			if (newname == "")
			{
				newname = text;
			}
			string text2 = newname;
			if (OMtr.Count <= 0)
			{
				return newname;
			}
			if (REG.match(newname, REG.RegSuffixNumber))
			{
				newname = REG.leftContext;
				num = X.NmI(REG.R1, 0, false, false);
			}
			string text3 = newname;
			while (flag2)
			{
				flag2 = false;
				text3 = (flag ? text2 : (((newname != "") ? newname : text) + ((num > 0) ? ("_" + num.ToString()) : "")));
				flag = false;
				foreach (KeyValuePair<string, T> keyValuePair in OMtr)
				{
					T value = keyValuePair.Value;
					if (value != MyMtr && value.get_individual_key() == text3)
					{
						num++;
						flag2 = true;
						break;
					}
				}
			}
			return text3;
		}

		private static void ScreenLockInit()
		{
			X.ScreenLockMono = new FlaggerC<MonoBehaviour>(null, null);
			X.ScreenLockStr = new FlaggerT<string>(null, null);
		}

		public static bool SCLOCK()
		{
			return X.ScreenLockMono.isActive() || X.ScreenLockStr.isActive();
		}

		public static void SCLOCK(MonoBehaviour B)
		{
			X.ScreenLockMono.Add(B);
		}

		public static void SCLOCK(string B)
		{
			X.ScreenLockStr.Add(B);
		}

		public static void REMLOCK(MonoBehaviour B)
		{
			X.ScreenLockMono.Rem(B);
		}

		public static void REMLOCK(string B)
		{
			X.ScreenLockStr.Rem(B);
		}

		public static bool chkSCLOCK(params MonoBehaviour[] AMono)
		{
			return X.ScreenLockStr.isActive() || X.ScreenLockMono.isActive(AMono);
		}

		public static bool chkSCLOCK(params string[] Astr)
		{
			return X.ScreenLockMono.isActive() || X.ScreenLockStr.isActive(Astr);
		}

		public static IN _stage;

		public static bool DEBUG = true;

		public static bool DEBUGNOCFG = false;

		public static bool DEBUGANNOUNCE = false;

		public static bool DEBUGNOSND = false;

		public static bool DEBUG_PLAYER = false;

		public static bool DEBUGALLSKILL = false;

		public static bool DEBUGNOEVENT = false;

		public static bool DEBUGNOVOICE = false;

		public static bool DEBUGRELOADMTR = false;

		public static bool DEBUGTIMESTAMP = false;

		public static bool DEBUGALBUMUNLOCK = false;

		public static bool DEBUGSTABILIZE_DRAW = false;

		public static bool DEBUGMIGHTY = false;

		public static bool DEBUGNODAMAGE = false;

		public static bool DEBUGWEAK = false;

		public static bool DEBUGBENCHMARK = false;

		public static bool DEBUGSUPERCYCLONE = false;

		public static bool ENG_MODE = false;

		public static bool DEBUGSPEFFECT = false;

		public static byte sensitive_level = 0;

		private const string debug_data_path = "_debug.txt";

		public const uint keyconfig_cfg_header = 308066463U;

		public const float rPI = 0.31830987f;

		public const float PI = 3.1415927f;

		public const double PId = 3.1415927410125732;

		public const float PIH = 1.5707964f;

		public const float PIHH = 0.7853982f;

		public const float PIHM = -1.5707964f;

		public const float PIHHM = -0.7853982f;

		public const float PI2 = 6.2831855f;

		public const float PI3_4 = 2.3561945f;

		public const float PI2_3 = 2.0943952f;

		public const float PI4_3 = 4.1887903f;

		public const float VAL_E = 2.7182817f;

		public const float SQRT2 = 1.4142135f;

		public const float SQRT3 = 1.7320508f;

		public const float SQRT5 = 2.236068f;

		public const float TAN_22_5 = 0.41421357f;

		public const float TAN_67_5 = 2.4142137f;

		public const float R1_8 = 0.125f;

		public const float R1_16 = 0.0625f;

		public const float R1_32 = 0.03125f;

		public const float R1_64 = 0.015625f;

		public const float R1_60 = 0.016666668f;

		public const float R1_30 = 0.033333335f;

		public const float R1_128 = 0.0078125f;

		public const float R1_100 = 0.01f;

		public const float R1_256 = 0.00390625f;

		public const float R1_512 = 0.001953125f;

		public const float R1_1024 = 0.0009765625f;

		public const float RM_8 = 0.875f;

		public const float RM_16 = 0.9375f;

		public const float RM_32 = 0.96875f;

		public const float RM_64 = 0.984375f;

		public const float RM_128 = 0.9921875f;

		public const float RM_256 = 0.99609375f;

		public const float RM_512 = 0.9980469f;

		public const float RM_1024 = 0.99902344f;

		public const float TAN_45 = 1f;

		public const float TAN_M45 = -1f;

		public const float LOG2 = 0.6931472f;

		public static float ppu;

		public static XorsMaker Xors = new XorsMaker(0U, false);

		public static bool D = true;

		public static int AF = 1;

		public static int AF_EF = 1;

		public static bool v_sync = false;

		public static bool D_EF = true;

		public static float EF_LEVEL_NORMAL = 1f;

		public static float EF_LEVEL_UI = 1f;

		public static float EF_TIMESCALE_UI = 1f;

		public static DateTime TimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public static BDic<string, string> OKey2Text;

		private static BDic<string, FnZoom> OFnZoom;

		private static List<int> ABufComb;

		public static Comparison<int> FnSortIntager = (int a, int b) => a - b;

		public static Comparison<int> FnSortIntagerR = (int a, int b) => b - a;

		public static Comparison<float> FnSortFloat = delegate(float a, float b)
		{
			if (a < b)
			{
				return -1;
			}
			if (a != b)
			{
				return 1;
			}
			return 0;
		};

		public static Comparison<string> FnSortStringWithNumber = (string a, string b) => X.SortStringWithNumber(a, b, 0);

		private static FlaggerC<MonoBehaviour> ScreenLockMono;

		private static FlaggerT<string> ScreenLockStr;
	}
}
