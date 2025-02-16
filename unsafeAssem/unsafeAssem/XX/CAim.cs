using System;
using PixelLiner.PixelLinerCore;
using UnityEngine;

namespace XX
{
	public sealed class CAim
	{
		public static string[] Astring
		{
			get
			{
				if (CAim.Astring__ == null)
				{
					CAim.Astring__ = new string[] { "left", "top", "right", "bottom", "top left", "top right", "bottom left", "bottom right" };
				}
				return CAim.Astring__;
			}
		}

		public static AIM get_aim(float x1, float y1, float x2, float y2, bool aim_tetra = false)
		{
			if (!aim_tetra)
			{
				return CAim.get_aim2(x1, y1, x2, y2, false);
			}
			return CAim.get_aim_tetra(x1, y1, x2, y2);
		}

		public static AIM get_aim_tetra(float x1, float y1, float x2, float y2)
		{
			float num = x2 - x1;
			float num2 = y2 - y1;
			float num3 = X.Abs(num);
			float num4 = X.Abs(num2);
			if (num3 > num4)
			{
				if (num <= 0f)
				{
					return AIM.L;
				}
				return AIM.R;
			}
			else
			{
				if (num2 <= 0f)
				{
					return AIM.B;
				}
				return AIM.T;
			}
		}

		public static AIM get_aim_tetra(int x1, int y1, int x2, int y2)
		{
			int num = x2 - x1;
			int num2 = y2 - y1;
			int num3 = X.Abs(num);
			int num4 = X.Abs(num2);
			if (num3 > num4)
			{
				if (num <= 0)
				{
					return AIM.L;
				}
				return AIM.R;
			}
			else
			{
				if (num2 <= 0)
				{
					return AIM.B;
				}
				return AIM.T;
			}
		}

		public static AIM get_aim_tetraN(int x1, int y1, int x2, int y2)
		{
			int num = x2 - x1;
			int num2 = y2 - y1;
			if (num > 0)
			{
				if (num2 <= 0)
				{
					return AIM.RB;
				}
				return AIM.TR;
			}
			else
			{
				if (num2 <= 0)
				{
					return AIM.BL;
				}
				return AIM.LT;
			}
		}

		public static AIM get_aim2(Vector2 pos1, Vector2 pos2, bool plus_flag = false)
		{
			return CAim.get_aim2(pos1.x, pos1.y, pos2.x, pos2.y, plus_flag);
		}

		public static AIM get_aim2(float x1, float y1, float x2, float y2, bool plus_flag = false)
		{
			float num = x2 - x1;
			float num2 = y2 - y1;
			if (num == 0f)
			{
				if (num2 < 0f)
				{
					return AIM.B;
				}
				return AIM.T;
			}
			else if (num2 == 0f)
			{
				if (num < 0f)
				{
					return AIM.L;
				}
				return AIM.R;
			}
			else if (plus_flag)
			{
				float num3 = X.Abs(num);
				float num4 = X.Abs(num2);
				if (num3 > num4)
				{
					if (num < 0f)
					{
						return AIM.L;
					}
					return AIM.R;
				}
				else
				{
					if (num2 < 0f)
					{
						return AIM.B;
					}
					return AIM.T;
				}
			}
			else if (num > 0f)
			{
				if (num2 <= 0f)
				{
					return AIM.RB;
				}
				return AIM.TR;
			}
			else
			{
				if (num2 <= 0f)
				{
					return AIM.BL;
				}
				return AIM.LT;
			}
		}

		public static AIM get_aim_r(Vector2 pos1, Vector2 pos2)
		{
			return CAim.get_aim_r(pos1.x, pos1.y, pos2.x, pos2.y);
		}

		public static AIM get_aim_r(float x1, float y1, float x2, float y2)
		{
			float num = x2 - x1;
			float num2 = y2 - y1;
			float num3 = X.Abs(num);
			float num4 = X.Abs(num2);
			if (num4 < 0.41421357f * num3)
			{
				if (num <= 0f)
				{
					return AIM.L;
				}
				return AIM.R;
			}
			else if (num4 < 2.4142137f * num3)
			{
				if (num <= 0f)
				{
					if (num2 >= 0f)
					{
						return AIM.LT;
					}
					return AIM.BL;
				}
				else
				{
					if (num2 >= 0f)
					{
						return AIM.TR;
					}
					return AIM.RB;
				}
			}
			else
			{
				if (num2 >= 0f)
				{
					return AIM.T;
				}
				return AIM.B;
			}
		}

		public static AIM mpfLR(bool b)
		{
			if (!b)
			{
				return AIM.L;
			}
			return AIM.R;
		}

		public static AIM mpfTB(bool b)
		{
			if (!b)
			{
				return AIM.T;
			}
			return AIM.B;
		}

		public static float get_ag01(AIM _d, float plus = 0f)
		{
			if (_d == AIM.LT)
			{
				return 0.375f + plus;
			}
			if (_d == AIM.T)
			{
				return 0.25f + plus;
			}
			if (_d == AIM.TR)
			{
				return 0.125f + plus;
			}
			if (_d == AIM.R)
			{
				return 0f + plus;
			}
			if (_d == AIM.RB)
			{
				return -0.125f + plus;
			}
			if (_d == AIM.B)
			{
				return -0.25f + plus;
			}
			if (_d == AIM.BL)
			{
				return -0.375f + plus;
			}
			return -0.5f + plus;
		}

		public static float get_agR(AIM _d, float plus = 0f)
		{
			return CAim.get_ag01(_d, 0f) * 6.2831855f + plus;
		}

		public static bool valid(int _d)
		{
			return X.BTW(0f, (float)_d, 8f);
		}

		public static bool valid(uint _d)
		{
			return X.BTW(0f, (float)_d, 8f);
		}

		public static bool valid(AIM _d)
		{
			return true;
		}

		private static int XD4(int dir)
		{
			dir &= 3;
			if (dir != 3)
			{
				return -1 + dir;
			}
			return 0;
		}

		private static int YD4(int dir)
		{
			dir &= 3;
			if (dir != 0)
			{
				return 2 - dir;
			}
			return 0;
		}

		public static int XDN(int dir)
		{
			if ((dir & 1) != 1)
			{
				return -1;
			}
			return 1;
		}

		public static int YDN(int dir)
		{
			if (((dir >> 1) & 1) != 1)
			{
				return 1;
			}
			return -1;
		}

		public static int _XD(int _d, int v = 1)
		{
			return ((_d >= 4) ? CAim.XDN(_d - 4) : CAim.XD4(_d)) * v;
		}

		public static int _YD(int _d, int v = 1)
		{
			return ((_d >= 4) ? CAim.YDN(_d - 4) : CAim.YD4(_d)) * v;
		}

		public static int XDN(AIM dir)
		{
			return CAim.XDN((int)dir);
		}

		public static int YDN(AIM dir)
		{
			return CAim.YDN((int)dir);
		}

		public static int _XD(AIM _d, int v = 1)
		{
			return CAim._XD((int)_d, v);
		}

		public static int _YD(AIM _d, int v = 1)
		{
			return CAim._YD((int)_d, v);
		}

		public static int _XDhalf(int _d, int v = 1)
		{
			if (_d < 0)
			{
				return 0;
			}
			int num = CAim._XD(_d, 1);
			return ((num > 0) ? X.IntR((float)v * 0.5f) : ((int)((float)v * 0.5f))) * num;
		}

		public static int _YDhalf(int _d, int v = 1)
		{
			if (_d < 0)
			{
				return 0;
			}
			int num = CAim._YD(_d, 1);
			return ((num > 0) ? X.IntR((float)v * 0.5f) : ((int)((float)v * 0.5f))) * num;
		}

		public static int _XDNR(int _d, int v = 1)
		{
			_d %= 4;
			return CAim.XDN((_d == 0) ? 0 : ((_d == 1) ? 1 : ((_d == 2) ? 3 : 2))) * v;
		}

		public static int _YDNR(int _d, int v = 1)
		{
			_d %= 4;
			return CAim.YDN((_d == 0) ? 0 : ((_d == 1) ? 1 : ((_d == 2) ? 3 : 2))) * v;
		}

		public static Vector2 get_V2(AIM a)
		{
			return new Vector2((float)CAim._XD(a, 1), (float)CAim._YD(a, 1));
		}

		public static AIM get_clockwise(AIM _d, bool _anti = false)
		{
			switch (_d)
			{
			case AIM.L:
				if (!_anti)
				{
					return AIM.LT;
				}
				return AIM.BL;
			case AIM.T:
				if (!_anti)
				{
					return AIM.TR;
				}
				return AIM.LT;
			case AIM.R:
				if (!_anti)
				{
					return AIM.RB;
				}
				return AIM.TR;
			case AIM.B:
				if (!_anti)
				{
					return AIM.BL;
				}
				return AIM.RB;
			case AIM.LT:
				if (!_anti)
				{
					return AIM.T;
				}
				return AIM.L;
			case AIM.TR:
				if (!_anti)
				{
					return AIM.R;
				}
				return AIM.T;
			case AIM.RB:
				if (!_anti)
				{
					return AIM.B;
				}
				return AIM.R;
			}
			if (!_anti)
			{
				return AIM.L;
			}
			return AIM.B;
		}

		public static AIM get_clockwise2(AIM _d, bool _anti = false)
		{
			switch (_d)
			{
			case AIM.L:
				if (!_anti)
				{
					return AIM.T;
				}
				return AIM.B;
			case AIM.T:
				if (!_anti)
				{
					return AIM.R;
				}
				return AIM.L;
			case AIM.R:
				if (!_anti)
				{
					return AIM.B;
				}
				return AIM.T;
			case AIM.B:
				if (!_anti)
				{
					return AIM.L;
				}
				return AIM.R;
			case AIM.LT:
				if (!_anti)
				{
					return AIM.TR;
				}
				return AIM.BL;
			case AIM.TR:
				if (!_anti)
				{
					return AIM.RB;
				}
				return AIM.LT;
			case AIM.RB:
				if (!_anti)
				{
					return AIM.BL;
				}
				return AIM.TR;
			}
			if (!_anti)
			{
				return AIM.LT;
			}
			return AIM.RB;
		}

		public static AIM get_opposite(AIM _d)
		{
			if (_d >= AIM.LT)
			{
				return AIM.B - (_d - AIM.LT) + 4U;
			}
			return (_d + 2U) % AIM.LT;
		}

		public static AIM get_rotation(AIM _d, int k, bool rflag = true)
		{
			if (k == 0)
			{
				return _d;
			}
			int num = (k - 1) / 2;
			bool flag = k % 2 > 0 == rflag;
			if (num == 0)
			{
				return CAim.get_clockwise(_d, flag);
			}
			if (num == 1)
			{
				return CAim.get_clockwise2(_d, flag);
			}
			if (num == 2)
			{
				return CAim.get_clockwise3(_d, flag);
			}
			return CAim.get_opposite(_d);
		}

		public static AIM get_clockwise3(AIM _d, bool _anti = false)
		{
			return CAim.get_clockwise2(CAim.get_clockwise(_d, _anti), _anti);
		}

		public static AIM get_clockwiseN(AIM _d, int k)
		{
			if (k == 0)
			{
				return _d;
			}
			while (k >= 4)
			{
				k -= 8;
			}
			while (k < -4)
			{
				k += 8;
			}
			int num = X.Abs(k);
			bool flag = k < 0;
			if (num == 1)
			{
				return CAim.get_clockwise(_d, flag);
			}
			if (num == 2)
			{
				return CAim.get_clockwise2(_d, flag);
			}
			if (num != 3)
			{
				return CAim.get_opposite(_d);
			}
			return CAim.get_clockwise3(_d, flag);
		}

		public static bool is_naname(AIM _d)
		{
			return _d == AIM.LT || _d == AIM.BL || _d == AIM.TR || _d == AIM.RB;
		}

		public static bool nanameMarging(AIM _d, AIM _t)
		{
			int num = CAim._XD(_d, 1);
			int num2 = CAim._YD(_d, 1);
			int num3 = CAim._XD(_t, 1);
			int num4 = CAim._YD(_t, 1);
			return (num3 != 0 && num == num3) || (num4 != 0 && num2 == num4);
		}

		public static int get_dif(int _d1, int _d2, int _max = 2)
		{
			return CAim.get_dif((AIM)_d1, (AIM)_d2, _max);
		}

		public static int get_dif(AIM _d1, AIM _d2, int _max = 2)
		{
			if (_d1 == _d2)
			{
				return 0;
			}
			if (_max <= 1 || CAim.get_clockwise(_d1, false) == _d2)
			{
				return 1;
			}
			if (CAim.get_clockwise(_d1, true) == _d2)
			{
				return -1;
			}
			if (_max <= 2 || CAim.get_clockwise2(_d1, false) == _d2)
			{
				return 2;
			}
			if (CAim.get_clockwise2(_d1, true) == _d2)
			{
				return -2;
			}
			if (_max <= 3 || CAim.get_clockwise3(_d1, false) == _d2)
			{
				return 3;
			}
			if (CAim.get_clockwise3(_d1, true) == _d2)
			{
				return -3;
			}
			AIM aim = CAim.get_opposite(_d1);
			return 4;
		}

		public static int get_dif_tetra(AIM _d1, AIM _d2)
		{
			if (_d1 == _d2)
			{
				return 0;
			}
			if (CAim.get_clockwise2(_d1, false) == _d2)
			{
				return 2;
			}
			if (CAim.get_clockwise2(_d1, true) == _d2)
			{
				return -2;
			}
			return 4;
		}

		public static string parseInt(AIM a)
		{
			switch (a)
			{
			case AIM.L:
				return "L";
			case AIM.T:
				return "T";
			case AIM.R:
				return "R";
			case AIM.B:
				return "B";
			case AIM.LT:
				return "LT";
			case AIM.TR:
				return "TR";
			case AIM.RB:
				return "RB";
			}
			return "BL";
		}

		public static AIM toPxlAim(AIM a)
		{
			switch (a)
			{
			case AIM.L:
				return AIM.L;
			case AIM.T:
				return AIM.T;
			case AIM.R:
				return AIM.R;
			case AIM.B:
				return AIM.B;
			case AIM.LT:
				return AIM.LT;
			case AIM.TR:
				return AIM.TR;
			case AIM.RB:
				return AIM.RB;
			}
			return AIM.BL;
		}

		public static int parseString(string s, int def_aim = 0)
		{
			if (s != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(s);
				if (num <= 1599373397U)
				{
					if (num <= 1093381803U)
					{
						if (num <= 620062877U)
						{
							if (num <= 467490188U)
							{
								if (num != 401953830U)
								{
									if (num != 467490188U)
									{
										goto IL_053B;
									}
									if (!(s == "BA"))
									{
										goto IL_053B;
									}
									return 3;
								}
								else
								{
									if (!(s == "TA"))
									{
										goto IL_053B;
									}
									return 1;
								}
							}
							else if (num != 551378283U)
							{
								if (num != 620062877U)
								{
									goto IL_053B;
								}
								if (!(s == "TL"))
								{
									goto IL_053B;
								}
								return 4;
							}
							else
							{
								if (!(s == "BL"))
								{
									goto IL_053B;
								}
								return 6;
							}
						}
						else if (num <= 786264949U)
						{
							if (num != 653618115U)
							{
								if (num != 786264949U)
								{
									goto IL_053B;
								}
								if (!(s == "BR"))
								{
									goto IL_053B;
								}
								return 7;
							}
							else
							{
								if (!(s == "TR"))
								{
									goto IL_053B;
								}
								return 5;
							}
						}
						else if (num != 944060518U)
						{
							if (num != 1009493708U)
							{
								if (num != 1093381803U)
								{
									goto IL_053B;
								}
								if (!(s == "bl"))
								{
									goto IL_053B;
								}
								return 6;
							}
							else
							{
								if (!(s == "ba"))
								{
									goto IL_053B;
								}
								return 3;
							}
						}
						else
						{
							if (!(s == "ta"))
							{
								goto IL_053B;
							}
							return 1;
						}
					}
					else if (num <= 1211222494U)
					{
						if (num <= 1194444875U)
						{
							if (num != 1162169565U)
							{
								if (num != 1194444875U)
								{
									goto IL_053B;
								}
								if (!(s == "lb"))
								{
									goto IL_053B;
								}
								return 6;
							}
							else
							{
								if (!(s == "tl"))
								{
									goto IL_053B;
								}
								return 4;
							}
						}
						else if (num != 1195724803U)
						{
							if (num != 1211222494U)
							{
								goto IL_053B;
							}
							if (!(s == "la"))
							{
								goto IL_053B;
							}
						}
						else
						{
							if (!(s == "tr"))
							{
								goto IL_053B;
							}
							return 5;
						}
					}
					else if (num <= 1328268469U)
					{
						if (num != 1230265779U)
						{
							if (num != 1328268469U)
							{
								goto IL_053B;
							}
							if (!(s == "br"))
							{
								goto IL_053B;
							}
							return 7;
						}
						else
						{
							if (!(s == "rt"))
							{
								goto IL_053B;
							}
							return 5;
						}
					}
					else if (num != 1549040540U)
					{
						if (num != 1563552493U)
						{
							if (num != 1599373397U)
							{
								goto IL_053B;
							}
							if (!(s == "rb"))
							{
								goto IL_053B;
							}
							return 7;
						}
						else
						{
							if (!(s == "lt"))
							{
								goto IL_053B;
							}
							return 4;
						}
					}
					else
					{
						if (!(s == "ra"))
						{
							goto IL_053B;
						}
						return 2;
					}
				}
				else if (num <= 2520546146U)
				{
					if (num <= 2080701468U)
					{
						if (num <= 1742883422U)
						{
							if (num != 1726105803U)
							{
								if (num != 1742883422U)
								{
									goto IL_053B;
								}
								if (!(s == "LA"))
								{
									goto IL_053B;
								}
							}
							else
							{
								if (!(s == "LB"))
								{
									goto IL_053B;
								}
								return 6;
							}
						}
						else if (num != 1761926707U)
						{
							if (num != 2080701468U)
							{
								goto IL_053B;
							}
							if (!(s == "RA"))
							{
								goto IL_053B;
							}
							return 2;
						}
						else
						{
							if (!(s == "RT"))
							{
								goto IL_053B;
							}
							return 5;
						}
					}
					else if (num <= 2131034325U)
					{
						if (num != 2095213421U)
						{
							if (num != 2131034325U)
							{
								goto IL_053B;
							}
							if (!(s == "RB"))
							{
								goto IL_053B;
							}
							return 7;
						}
						else
						{
							if (!(s == "LT"))
							{
								goto IL_053B;
							}
							return 4;
						}
					}
					else if (num != 2486990908U)
					{
						if (num != 2503768527U)
						{
							if (num != 2520546146U)
							{
								goto IL_053B;
							}
							if (!(s == "↓"))
							{
								goto IL_053B;
							}
							return 3;
						}
						else if (!(s == "←"))
						{
							goto IL_053B;
						}
					}
					else
					{
						if (!(s == "↑"))
						{
							goto IL_053B;
						}
						return 1;
					}
				}
				else if (num <= 3507227459U)
				{
					if (num <= 3339451269U)
					{
						if (num != 2537323765U)
						{
							if (num != 3339451269U)
							{
								goto IL_053B;
							}
							if (!(s == "B"))
							{
								goto IL_053B;
							}
							return 3;
						}
						else
						{
							if (!(s == "→"))
							{
								goto IL_053B;
							}
							return 2;
						}
					}
					else if (num != 3373006507U)
					{
						if (num != 3507227459U)
						{
							goto IL_053B;
						}
						if (!(s == "T"))
						{
							goto IL_053B;
						}
						return 1;
					}
					else if (!(s == "L"))
					{
						goto IL_053B;
					}
				}
				else if (num <= 3876335077U)
				{
					if (num != 3607893173U)
					{
						if (num != 3876335077U)
						{
							goto IL_053B;
						}
						if (!(s == "b"))
						{
							goto IL_053B;
						}
						return 3;
					}
					else
					{
						if (!(s == "R"))
						{
							goto IL_053B;
						}
						return 2;
					}
				}
				else if (num != 3909890315U)
				{
					if (num != 4044111267U)
					{
						if (num != 4144776981U)
						{
							goto IL_053B;
						}
						if (!(s == "r"))
						{
							goto IL_053B;
						}
						return 2;
					}
					else
					{
						if (!(s == "t"))
						{
							goto IL_053B;
						}
						return 1;
					}
				}
				else if (!(s == "l"))
				{
					goto IL_053B;
				}
				return 0;
			}
			IL_053B:
			return X.NmI(s, def_aim, false, false);
		}

		public static int parseString(STB Stb, int char_s, int def_aim = 0)
		{
			if (Stb.Is('→', char_s))
			{
				return 2;
			}
			if (Stb.Is('↑', char_s))
			{
				return 1;
			}
			if (Stb.Is('←', char_s))
			{
				return 0;
			}
			if (Stb.Is('↓', char_s))
			{
				return 3;
			}
			int num = char_s + 1;
			if (Stb.IsU('L', char_s))
			{
				if (num < Stb.Length)
				{
					if (Stb.IsU('T', num))
					{
						return 4;
					}
					if (Stb.IsU('B', num))
					{
						return 6;
					}
				}
				return 0;
			}
			if (Stb.IsU('R', char_s))
			{
				if (num < Stb.Length)
				{
					if (Stb.IsU('T', num))
					{
						return 5;
					}
					if (Stb.IsU('B', num))
					{
						return 7;
					}
				}
				return 2;
			}
			if (Stb.IsU('T', char_s))
			{
				if (num < Stb.Length)
				{
					if (Stb.IsU('L', num))
					{
						return 4;
					}
					if (Stb.IsU('R', num))
					{
						return 5;
					}
				}
				return 1;
			}
			if (Stb.IsU('B', char_s))
			{
				if (num < Stb.Length)
				{
					if (Stb.IsU('L', num))
					{
						return 6;
					}
					if (Stb.IsU('R', num))
					{
						return 7;
					}
				}
				return 3;
			}
			int num2;
			STB.PARSERES parseres = Stb.Nm(char_s, out num2, -1, false);
			if (parseres == STB.PARSERES.DOUBLE)
			{
				return (int)STB.parse_result_double;
			}
			if (parseres != STB.PARSERES.INT)
			{
				return def_aim;
			}
			return STB.parse_result_int;
		}

		public static int fixDirectionByBits(ref int x, ref int y, int bits)
		{
			if (x == 0 && y == 0)
			{
				return -1;
			}
			AIM aim = CAim.get_aim2(0f, 0f, (float)x, (float)y, false);
			if ((bits & 255) == 255)
			{
				return (int)aim;
			}
			if ((bits & (1 << (int)aim)) == 0)
			{
				for (int i = 1; i <= 8; i++)
				{
					AIM aim2 = CAim.get_rotation(aim, i, true);
					if ((bits & (1 << (int)aim2)) != 0)
					{
						x = CAim._XD(aim2, 1);
						y = CAim._YD(aim2, 1);
						return (int)aim2;
					}
				}
				x = (y = 0);
				return -1;
			}
			return (int)aim;
		}

		private static string[] Astring__;

		private const AIM L = AIM.L;

		private const AIM T = AIM.T;

		private const AIM R = AIM.R;

		private const AIM B = AIM.B;

		private const AIM LT = AIM.LT;

		private const AIM TR = AIM.TR;

		private const AIM BL = AIM.BL;

		private const AIM RB = AIM.RB;

		private const AIM ALL = AIM.ALL;

		public const uint BIT_L = 1U;

		public const uint BIT_T = 2U;

		public const uint BIT_R = 4U;

		public const uint BIT_B = 8U;

		public const uint BIT_LTRB = 15U;

		public const uint BIT_L_R = 5U;

		public const uint BIT_T_B = 10U;

		public const uint BIT_LT = 16U;

		public const uint BIT_TR = 32U;

		public const uint BIT_BL = 64U;

		public const uint BIT_RB = 128U;

		public const uint BIT_ALL = 255U;
	}
}
