using System;
using PixelLiner;
using XX;

namespace m2d
{
	public static class CCON
	{
		public static bool canStand(int t)
		{
			return CCON.canStand(t, null);
		}

		public static bool canStand(int t, M2Mover Mv)
		{
			return t < 110 || CCON.isLift(t) || (t == 201 || t == 199);
		}

		public static bool mistPassable(int t, byte consider_stopper = 0)
		{
			return (CCON.canStand(t, null) && (consider_stopper == 0 || ((consider_stopper == 1) ? (t != 31 && t != 62 && t != 63) : ((consider_stopper == 2) ? (t != 31 && t != 123) : (t != 31 && t != 62 && t != 63 && t != 123))))) || t == 124;
		}

		public static bool canStandAndNoBlockSlope(int t)
		{
			return CCON.canStand(t) && !CCON.isBlockSlope(t);
		}

		public static void calcConfigManual(ref M2Pt Pos, int nc)
		{
			if (Pos == null)
			{
				Pos = new M2Pt(0, nc);
				return;
			}
			Pos.calcConfigManual(nc);
		}

		public static int calcConfig(ref int c, int nc)
		{
			if (c > nc)
			{
				c = CCON.calcConfig(ref nc, c);
				return c;
			}
			if (nc == 60)
			{
				return c;
			}
			if (c == 4 || c == 0)
			{
				c = nc;
			}
			if (c == 20 && nc == 120)
			{
				c = 122;
			}
			else if (c == 61 && nc == 120)
			{
				c = 126;
			}
			else if ((c == 62 || c == 31) && nc == 120)
			{
				c = 123;
			}
			else if (c == 61 && nc == 62)
			{
				c = 63;
			}
			else if ((c == 120 || c == 122) && nc == 125)
			{
				c = 128;
			}
			else if ((c == 120 || c == 122) && nc == 155)
			{
				c = 160;
			}
			else if (c == 4 || c == 0)
			{
				c = nc;
			}
			else if (CCON.isLift(c) && !CCON.isLift(nc))
			{
				c = nc;
			}
			else
			{
				c = ((c < nc) ? nc : c);
			}
			return c;
		}

		public static bool canFootOn(int t, M2FootManager FootD = null)
		{
			return t >= 64;
		}

		public static bool isEmpty(int t)
		{
			return t <= 4;
		}

		public static bool canStandAndNoBox(int t)
		{
			return CCON.canStand(t) && !CCON.isBox(t);
		}

		public static bool isCeil(int t)
		{
			return t == 160 || t == 31 || t == 60 || t == 120 || t == 155 || CCON.isLiftSlope(t) || t == 123;
		}

		public static bool isFloor(int t)
		{
			return t == 10 || t == 140;
		}

		public static bool isLift(int t)
		{
			return t == 120 || t == 121 || t == 110 || t == 122 || t == 126 || t == 201 || t == 123 || CCON.isLiftSlope(t);
		}

		public static bool isDangerous(int t)
		{
			return CCON.isUntouchable(t) || CCON.isWater(t);
		}

		public static bool isWater(int t)
		{
			return t == 61;
		}

		public static bool isSlope(int t)
		{
			return X.BTW(64f, (float)t, 110f);
		}

		public static bool isBlockSlope(int t)
		{
			return X.BTW(64f, (float)t, 110f) && !X.BTW(79f, (float)t, 95f);
		}

		public static bool isLiftSlope(int t)
		{
			return X.BTW(79f, (float)t, 95f);
		}

		public static bool isSlopeLT2RB(int t)
		{
			return X.BTW(64f, (float)t, 90f);
		}

		public static bool isSlopeBL2TR(int t)
		{
			return X.BTW(90f, (float)t, 110f);
		}

		public static bool isUntouchable(int t)
		{
			return t == 200 || t == 201 || t == 20 || t == 199 || t == 122 || t == 125 || t == 155;
		}

		public static bool isUntouchableFoot(int t)
		{
			return t == 200 || t == 201 || t == 20 || t == 199 || t == 125 || t == 155;
		}

		public static bool isBox(int t)
		{
			return t == 121 || t == 30 || t == 201 || t == 14 || t == 110 || t == 199;
		}

		public static bool isRealBox(int t)
		{
			return t == 121 || t == 30 || t == 201;
		}

		public static bool isImitationBox(int t)
		{
			return t == 14 || t == 110 || t == 199;
		}

		public static bool isPushed(int t)
		{
			return !CCON.canStand(t) || CCON.isBox(t);
		}

		public static float getSlopeLevel(int t, bool is_right)
		{
			switch (t)
			{
			case 64:
			case 80:
				if (is_right)
				{
					return 0.5f;
				}
				return 0f;
			case 65:
			case 79:
				if (is_right)
				{
					return 1f;
				}
				return 0.5f;
			case 66:
				if (is_right)
				{
					return 0.33333334f;
				}
				return 0f;
			case 67:
				if (is_right)
				{
					return 0.6666667f;
				}
				return 0.33333334f;
			case 68:
				if (is_right)
				{
					return 1f;
				}
				return 0.6666667f;
			case 69:
				if (is_right)
				{
					return 0.25f;
				}
				return 0f;
			case 70:
				if (is_right)
				{
					return 0.5f;
				}
				return 0.25f;
			case 71:
				if (is_right)
				{
					return 0.75f;
				}
				return 0.5f;
			case 72:
				if (is_right)
				{
					return 1f;
				}
				return 0.75f;
			case 73:
				if (is_right)
				{
					return 0.2f;
				}
				return 0f;
			case 74:
				if (is_right)
				{
					return 0.4f;
				}
				return 0.2f;
			case 75:
				if (is_right)
				{
					return 0.6f;
				}
				return 0.4f;
			case 76:
				if (is_right)
				{
					return 0.8f;
				}
				return 0.6f;
			case 77:
				if (is_right)
				{
					return 1f;
				}
				return 0.8f;
			case 78:
			case 81:
				if (is_right)
				{
					return 1f;
				}
				return 0f;
			case 92:
			case 96:
				if (is_right)
				{
					return 0.5f;
				}
				return 1f;
			case 93:
			case 97:
				if (is_right)
				{
					return 0f;
				}
				return 0.5f;
			case 94:
			case 95:
				if (is_right)
				{
					return 0f;
				}
				return 1f;
			case 98:
				if (is_right)
				{
					return 0.6666667f;
				}
				return 1f;
			case 99:
				if (is_right)
				{
					return 0.33333334f;
				}
				return 0.6666667f;
			case 100:
				if (is_right)
				{
					return 0f;
				}
				return 0.33333334f;
			case 101:
				if (is_right)
				{
					return 0.75f;
				}
				return 1f;
			case 102:
				if (is_right)
				{
					return 0.5f;
				}
				return 0.75f;
			case 103:
				if (is_right)
				{
					return 0.25f;
				}
				return 0.5f;
			case 104:
				if (is_right)
				{
					return 0f;
				}
				return 0.25f;
			case 105:
				if (is_right)
				{
					return 0.8f;
				}
				return 1f;
			case 106:
				if (is_right)
				{
					return 0.6f;
				}
				return 0.8f;
			case 107:
				if (is_right)
				{
					return 0.4f;
				}
				return 0.6f;
			case 108:
				if (is_right)
				{
					return 0.2f;
				}
				return 0.4f;
			case 109:
				if (is_right)
				{
					return 0f;
				}
				return 0.2f;
			}
			return 0f;
		}

		public static float getTiltLevel(int t)
		{
			switch (t)
			{
			case 64:
			case 65:
			case 79:
			case 80:
				return 50f;
			case 66:
			case 67:
			case 68:
				return 33f;
			case 69:
			case 70:
			case 71:
			case 72:
				return 25f;
			case 73:
			case 74:
			case 75:
			case 76:
			case 77:
				return 20f;
			case 78:
			case 81:
				return 100f;
			case 92:
			case 93:
			case 96:
			case 97:
				return -50f;
			case 94:
			case 95:
				return -100f;
			case 98:
			case 99:
			case 100:
				return -33f;
			case 101:
			case 102:
			case 103:
			case 104:
				return -25f;
			case 105:
			case 106:
			case 107:
			case 108:
			case 109:
				return -20f;
			}
			return 0f;
		}

		public static float getTiltLevel01(int t)
		{
			switch (t)
			{
			case 64:
			case 65:
			case 79:
			case 80:
				return 0.5f;
			case 66:
			case 67:
			case 68:
				return 0.33333334f;
			case 69:
			case 70:
			case 71:
			case 72:
				return 0.25f;
			case 73:
			case 74:
			case 75:
			case 76:
			case 77:
				return 0.2f;
			case 78:
			case 81:
				return 1f;
			case 92:
			case 93:
			case 96:
			case 97:
				return -0.5f;
			case 94:
			case 95:
				return -1f;
			case 98:
			case 99:
			case 100:
				return -0.33333334f;
			case 101:
			case 102:
			case 103:
			case 104:
				return -0.25f;
			case 105:
			case 106:
			case 107:
			case 108:
			case 109:
				return -0.2f;
			}
			return 0f;
		}

		public static int slopeFlip(int t)
		{
			switch (t)
			{
			case 64:
				return 97;
			case 65:
				return 96;
			case 66:
				return 100;
			case 67:
				return 99;
			case 68:
				return 98;
			case 69:
				return 104;
			case 70:
				return 103;
			case 71:
				return 102;
			case 72:
				return 101;
			case 73:
				return 109;
			case 74:
				return 108;
			case 75:
				return 107;
			case 76:
				return 106;
			case 77:
				return 105;
			case 78:
				return 95;
			case 79:
				return 92;
			case 80:
				return 93;
			case 81:
				return 94;
			case 92:
				return 79;
			case 93:
				return 80;
			case 94:
				return 81;
			case 95:
				return 78;
			case 96:
				return 65;
			case 97:
				return 64;
			case 98:
				return 68;
			case 99:
				return 67;
			case 100:
				return 66;
			case 101:
				return 72;
			case 102:
				return 71;
			case 103:
				return 70;
			case 104:
				return 69;
			case 105:
				return 77;
			case 106:
				return 76;
			case 107:
				return 75;
			case 108:
				return 74;
			case 109:
				return 73;
			}
			return t;
		}

		public static void DrawInit(MeshDrawer _Md, MeshDrawer _MdI, float _CLEN, float scale = 1f)
		{
			CCON.Md = _Md;
			CCON.Md.Col = MTRX.ColWhite;
			CCON.MdI = _MdI;
			CCON.clen = _CLEN;
			CCON.clenh = _CLEN / 2f;
			CCON.scl = 1f / scale;
			CCON.SqIco = MTRX.SqM2dIcon;
		}

		public static void DrawQuit()
		{
			CCON.Md = null;
			CCON.MdI = null;
		}

		private static void subMesh(int i)
		{
			if (CCON.Md == CCON.MdI && CCON.Md.getCurrentSubMeshIndex() != i)
			{
				CCON.Md.chooseSubMesh(i, false, false);
			}
			CCON.Md.Col = MTRX.ColWhite;
		}

		public static void draw(M2Pt Pt, float x, float y)
		{
			CCON.draw((Pt != null) ? Pt.cfg : (-1), x, y, Pt);
		}

		public static void draw(int c, float x, float y, M2Pt Pt = null)
		{
			if (c <= 109)
			{
				if (c <= 4)
				{
					if (c == 0)
					{
						CCON.subMesh(0);
						x += CCON.clenh;
						y += CCON.clenh;
						CCON.Md.Col = C32.d2c(4286216826U);
						CCON.Md.Line(x + 4f, y + 4f, x - 4f, y - 4f, 2f, false, 0f, 0f);
						CCON.Md.Line(x - 4f, y + 4f, x + 4f, y - 4f, 2f, false, 0f, 0f);
						goto IL_10C7;
					}
					if (c != 4)
					{
						goto IL_10C7;
					}
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(5, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
					goto IL_10C7;
				}
				else
				{
					if (c == 10)
					{
						CCON.subMesh(1);
						CCON.MdI.initForImg(CCON.SqIco.getImage(5, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
						CCON.MdI.initForImg(CCON.SqIco.getImage(3, 0), 0).DrawScaleGraph2(x + 1f, y + 1f, 0f, 0f, CCON.scl, CCON.scl, null);
						goto IL_10C7;
					}
					if (c == 20)
					{
						CCON.subMesh(1);
						CCON.MdI.initForImg(CCON.SqIco.getImage(0, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
						goto IL_10C7;
					}
					switch (c)
					{
					case 30:
						CCON.subMesh(1);
						CCON.MdI.initForImg(CCON.SqIco.getImage(2, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
						goto IL_10C7;
					case 31:
						CCON.subMesh(1);
						CCON.MdI.initForImg(CCON.SqIco.getImage(12, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
						goto IL_10C7;
					case 32:
					case 33:
					case 34:
					case 35:
					case 36:
					case 37:
					case 38:
					case 39:
					case 40:
					case 41:
					case 42:
					case 43:
					case 44:
					case 45:
					case 46:
					case 47:
					case 48:
					case 49:
					case 50:
					case 51:
					case 52:
					case 53:
					case 54:
					case 55:
					case 56:
					case 57:
					case 58:
					case 59:
					case 82:
					case 83:
					case 84:
					case 85:
					case 86:
					case 87:
					case 88:
					case 89:
					case 90:
					case 91:
						goto IL_10C7;
					case 60:
						CCON.subMesh(1);
						CCON.MdI.initForImg(CCON.SqIco.getImage(5, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
						CCON.MdI.initForImg(CCON.SqIco.getImage(4, 0), 0).DrawScaleGraph2(x + 1f, y + 1f, 0f, 0f, CCON.scl, CCON.scl, null);
						goto IL_10C7;
					case 61:
					case 63:
						break;
					case 62:
						CCON.subMesh(1);
						CCON.MdI.initForImg(CCON.SqIco.getImage(10, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
						goto IL_10C7;
					case 64:
					case 80:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0f, 0.5f);
						if (CCON.isLift(c))
						{
							CCON.subMesh(1);
							CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
							goto IL_10C7;
						}
						goto IL_10C7;
					case 65:
					case 79:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.5f, 1f);
						if (CCON.isLift(c))
						{
							CCON.subMesh(1);
							CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
							goto IL_10C7;
						}
						goto IL_10C7;
					case 66:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0f, 0.333f);
						goto IL_10C7;
					case 67:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.333f, 0.666f);
						goto IL_10C7;
					case 68:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.666f, 1f);
						goto IL_10C7;
					case 69:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0f, 0.25f);
						goto IL_10C7;
					case 70:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.25f, 0.5f);
						goto IL_10C7;
					case 71:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.5f, 0.75f);
						goto IL_10C7;
					case 72:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.75f, 1f);
						goto IL_10C7;
					case 73:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0f, 0.2f);
						goto IL_10C7;
					case 74:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.2f, 0.4f);
						goto IL_10C7;
					case 75:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.4f, 0.6f);
						goto IL_10C7;
					case 76:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.6f, 0.8f);
						goto IL_10C7;
					case 77:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.8f, 1f);
						goto IL_10C7;
					case 78:
					case 81:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0f, 1f);
						if (CCON.isLift(c))
						{
							CCON.subMesh(1);
							CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
							goto IL_10C7;
						}
						goto IL_10C7;
					case 92:
					case 96:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 1f, 0.5f);
						if (CCON.isLift(c))
						{
							CCON.subMesh(1);
							CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
							goto IL_10C7;
						}
						goto IL_10C7;
					case 93:
					case 97:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.5f, 0f);
						if (CCON.isLift(c))
						{
							CCON.subMesh(1);
							CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
							goto IL_10C7;
						}
						goto IL_10C7;
					case 94:
					case 95:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 1f, 0f);
						if (CCON.isLift(c))
						{
							CCON.subMesh(1);
							CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
							goto IL_10C7;
						}
						goto IL_10C7;
					case 98:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 1f, 0.666f);
						goto IL_10C7;
					case 99:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.666f, 0.333f);
						goto IL_10C7;
					case 100:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.333f, 0f);
						goto IL_10C7;
					case 101:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 1f, 0.75f);
						goto IL_10C7;
					case 102:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.75f, 0.5f);
						goto IL_10C7;
					case 103:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.5f, 0.25f);
						goto IL_10C7;
					case 104:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.25f, 0f);
						goto IL_10C7;
					case 105:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 1f, 0.8f);
						goto IL_10C7;
					case 106:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.8f, 0.6f);
						goto IL_10C7;
					case 107:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.6f, 0.4f);
						goto IL_10C7;
					case 108:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.4f, 0.2f);
						goto IL_10C7;
					case 109:
						CCON.subMesh(0);
						CCON.drawSlope(x, y, 0.2f, 0f);
						goto IL_10C7;
					default:
						goto IL_10C7;
					}
				}
			}
			else if (c <= 155)
			{
				switch (c)
				{
				case 120:
				case 123:
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
					if (c == 123)
					{
						CCON.MdI.initForImg(CCON.SqIco.getImage(10, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
						goto IL_10C7;
					}
					goto IL_10C7;
				case 121:
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
					CCON.MdI.initForImg(CCON.SqIco.getImage(2, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
					goto IL_10C7;
				case 122:
				case 127:
					goto IL_10C7;
				case 124:
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(9, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
					goto IL_10C7;
				case 125:
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(8, 0), 0).DrawScaleGraph2(x + 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
					goto IL_10C7;
				case 126:
					break;
				case 128:
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(6, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
					goto IL_10C7;
				default:
					if (c == 140)
					{
						CCON.subMesh(1);
						CCON.MdI.initForImg(CCON.SqIco.getImage(6, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
						CCON.MdI.initForImg(CCON.SqIco.getImage(3, 0), 0).DrawScaleGraph2(x + 1f, y + 1f, 0f, 0f, CCON.scl, CCON.scl, null);
						goto IL_10C7;
					}
					if (c != 155)
					{
						goto IL_10C7;
					}
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(8, 0), 0).DrawScaleGraph2(x + 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
					CCON.MdI.initForImg(CCON.SqIco.getImage(4, 0), 0).DrawScaleGraph2(x + 1f, y + 1f, 0f, 0f, CCON.scl, CCON.scl, null);
					goto IL_10C7;
				}
			}
			else
			{
				if (c == 160)
				{
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(6, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
					CCON.MdI.initForImg(CCON.SqIco.getImage(4, 0), 0).DrawScaleGraph2(x + 1f, y + 1f, 0f, 0f, CCON.scl, CCON.scl, null);
					goto IL_10C7;
				}
				if (c == 200)
				{
					CCON.subMesh(1);
					CCON.MdI.initForImg(CCON.SqIco.getImage(1, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
					goto IL_10C7;
				}
				if (c != 201)
				{
					goto IL_10C7;
				}
				CCON.subMesh(1);
				CCON.MdI.initForImg(CCON.SqIco.getImage(2, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
				CCON.MdI.initForImg(CCON.SqIco.getImage(1, 0), 0).DrawScaleGraph2(x + CCON.clen - 9f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
				goto IL_10C7;
			}
			CCON.subMesh(0);
			CCON.Md.Col = C32.d2c(4280780287U);
			CCON.Md.Drip(x + 10f, y + 10f, 4f * CCON.scl, 7f * CCON.scl, 0f, 0f, 0f);
			CCON.Md.Col = C32.d2c(4289588479U);
			CCON.Md.Drip(x + 10f, y + 10f, 4.5f * CCON.scl, 7.5f * CCON.scl, 2f, 0f, 0f);
			if (CCON.isLift(c))
			{
				CCON.subMesh(1);
				CCON.MdI.initForImg(CCON.SqIco.getImage(7, 0), 0).DrawScaleGraph2(x + 1f, y + CCON.clen - 1f, 0f, 1f, CCON.scl, CCON.scl, null);
			}
			if (c == 63)
			{
				CCON.subMesh(1);
				CCON.MdI.initForImg(CCON.SqIco.getImage(10, 0), 0).DrawScaleGraph2(x + CCON.clen - 1f, y + 1f, 1f, 0f, CCON.scl, CCON.scl, null);
			}
			IL_10C7:
			if (Pt != null && Pt.dangerous)
			{
				CCON.subMesh(1);
				CCON.MdI.initForImg(CCON.SqIco.getImage(11, 0), 0).DrawScaleGraph2(x + CCON.clen * 0.5f, y + CCON.clen * 0.5f, 0.5f, 0.5f, CCON.scl, CCON.scl, null);
			}
		}

		private static void drawSlope(float x, float y, float llv, float rlv)
		{
			float num = y + CCON.clen - llv * CCON.clen;
			float num2 = y + CCON.clen - rlv * CCON.clen;
			float num3 = x + CCON.clen;
			CCON.Md.Col = C32.d2c(2857256782U);
			CCON.Md.Line(x, num, num3, num2, 2f * CCON.scl, false, 0f, 0f);
			CCON.Md.Arc(x, num, 5f * CCON.scl, -1.5707964f, 1.5707964f, 0f);
			CCON.Md.Arc(num3, num2, 5f * CCON.scl, 1.5707964f, 4.712389f, 0f);
			CCON.Md.Col = C32.d2c(4294225661U);
			CCON.Md.Line(x, num, num3, num2, 0.75f * CCON.scl, false, 0f, 0f);
			CCON.Md.Arc(x, num, 3f * CCON.scl, -1.5707964f, 1.5707964f, 0f);
			CCON.Md.Arc(num3, num2, 3f * CCON.scl, 1.5707964f, 4.712389f, 0f);
		}

		public static BtnMenu<T> prepareMenuCfg<T>(BtnMenu<T> MenuCfg, Func<int, bool> FnSelected) where T : aBtn
		{
			MenuCfg.clms = 2;
			CCON.MenuMake<T>(MenuCfg, "FENCE", "フェンス");
			CCON.MenuMake<T>(MenuCfg, "SPIKE_TOP", "トゲ(上)");
			CCON.MenuMake<T>(MenuCfg, "SPIKE", "トゲ");
			CCON.MenuMake<T>(MenuCfg, "BLAZING_LIFT", "接触不可の箱");
			CCON.MenuMake<T>(MenuCfg, "BLAZING", "接触不可");
			CCON.MenuMake<T>(MenuCfg, "BLOCK", "壁");
			CCON.MenuMake<T>(MenuCfg, "LIFT_BOX", "移動ボックス");
			CCON.MenuMake<T>(MenuCfg, "LIFT", "すり抜け床");
			CCON.MenuMake<T>(MenuCfg, "WATER_CUT_LIFT", "すり抜け床(水せき止め)");
			CCON.MenuMake<T>(MenuCfg, "GAS_STOPPER", "流体せき止め");
			CCON.MenuMake<T>(MenuCfg, "MISTFILL_STOPPER", "横方向の水せき止め");
			CCON.MenuMake<T>(MenuCfg, "WATER", "水");
			CCON.MenuMake<T>(MenuCfg, "BOX_UNDER", "移動ボックス下部");
			CCON.MenuMake<T>(MenuCfg, "THUNDER", "通行可被ダメージ");
			CCON.MenuMake<T>(MenuCfg, "GROUND", "通過(下)");
			CCON.MenuMake<T>(MenuCfg, "THROUGH", "通過");
			CCON.MenuMake<T>(MenuCfg, "NOIMAGE", "画像なし");
			CCON.MenuMake<T>(MenuCfg, "----", "----");
			CCON.MenuMake<T>(MenuCfg, "SL_0_100", "＼リフト (0_100)");
			CCON.MenuMake<T>(MenuCfg, "SL_0_50", "＼リフト (0_50)");
			CCON.MenuMake<T>(MenuCfg, "SL_50_100", "＼リフト (50_100)");
			CCON.MenuMake<T>(MenuCfg, "SL_100_0", "／リフト (100_0)");
			CCON.MenuMake<T>(MenuCfg, "SL_50_0", "／リフト (50_0)");
			CCON.MenuMake<T>(MenuCfg, "SL_100_50", "／リフト (100_50)");
			CCON.MenuMake<T>(MenuCfg, "S_0_100", "＼ (0_100)");
			CCON.MenuMake<T>(MenuCfg, "S_0_50", "＼ (0_50)");
			CCON.MenuMake<T>(MenuCfg, "S_50_100", "＼ (50_100)");
			CCON.MenuMake<T>(MenuCfg, "S_0_33", "＼ (0_33)");
			CCON.MenuMake<T>(MenuCfg, "S_33_66", "＼ (33_66)");
			CCON.MenuMake<T>(MenuCfg, "S_66_100", "＼ (66_100)");
			CCON.MenuMake<T>(MenuCfg, "S_0_25", "＼ (0_25)");
			CCON.MenuMake<T>(MenuCfg, "S_25_50", "＼ (25_50)");
			CCON.MenuMake<T>(MenuCfg, "S_50_75", "＼ (50_75)");
			CCON.MenuMake<T>(MenuCfg, "S_75_100", "＼ (75_100)");
			CCON.MenuMake<T>(MenuCfg, "S_0_20", "＼ (0_20)");
			CCON.MenuMake<T>(MenuCfg, "S_20_40", "＼ (20_40)");
			CCON.MenuMake<T>(MenuCfg, "S_40_60", "＼ (40_60)");
			CCON.MenuMake<T>(MenuCfg, "S_60_80", "＼ (60_80)");
			CCON.MenuMake<T>(MenuCfg, "S_80_100", "＼ (80_100)");
			CCON.MenuMake<T>(MenuCfg, "S_100_0", "／ (100_0)");
			CCON.MenuMake<T>(MenuCfg, "S_50_0", "／ (50_0)");
			CCON.MenuMake<T>(MenuCfg, "S_100_50", "／ (100_50)");
			CCON.MenuMake<T>(MenuCfg, "S_33_0", "／ (33_0)");
			CCON.MenuMake<T>(MenuCfg, "S_66_33", "／ (66_33)");
			CCON.MenuMake<T>(MenuCfg, "S_100_66", "／ (100_66)");
			CCON.MenuMake<T>(MenuCfg, "S_25_0", "／ (25_0)");
			CCON.MenuMake<T>(MenuCfg, "S_50_25", "／ (50_25)");
			CCON.MenuMake<T>(MenuCfg, "S_75_50", "／ (75_50)");
			CCON.MenuMake<T>(MenuCfg, "S_100_75", "／ (100_75)");
			CCON.MenuMake<T>(MenuCfg, "S_20_0", "／ (20_0)");
			CCON.MenuMake<T>(MenuCfg, "S_40_20", "／ (40_20)");
			CCON.MenuMake<T>(MenuCfg, "S_60_40", "／ (60_40)");
			CCON.MenuMake<T>(MenuCfg, "S_80_60", "／ (80_60)");
			CCON.MenuMake<T>(MenuCfg, "S_100_80", "／ (100_80)");
			if (FnSelected != null)
			{
				MenuCfg.addSelectedFn((BtnMenu<T> _Menu, int i, string cur) => FnSelected(CCON.getStringToValue(cur)));
			}
			return MenuCfg;
		}

		private static void MenuMake<T>(BtnMenu<T> MenuCfg, string s, string a) where T : aBtn
		{
			T t = MenuCfg.Make(s, "");
			if (t != null)
			{
				t.get_Skin().setTitle(s + ":" + a);
			}
		}

		public static int getStringToValue(string key)
		{
			string text = key.ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 2100620882U)
				{
					if (num <= 1176132556U)
					{
						if (num <= 282773384U)
						{
							if (num <= 101434029U)
							{
								if (num <= 39245041U)
								{
									if (num != 25305664U)
									{
										if (num != 39245041U)
										{
											return 0;
										}
										if (!(text == "THUNDER"))
										{
											return 0;
										}
										return 20;
									}
									else if (!(text == "SLOPE_20_0"))
									{
										return 0;
									}
								}
								else if (num != 54615845U)
								{
									if (num != 73109128U)
									{
										if (num != 101434029U)
										{
											return 0;
										}
										if (!(text == "SL_0_100"))
										{
											return 0;
										}
										return 81;
									}
									else
									{
										if (!(text == "BLAZING_LIFT"))
										{
											return 0;
										}
										return 201;
									}
								}
								else
								{
									if (!(text == "S_0_100"))
									{
										return 0;
									}
									return 78;
								}
							}
							else if (num <= 244259572U)
							{
								if (num != 181233417U)
								{
									if (num != 182079643U)
									{
										if (num != 244259572U)
										{
											return 0;
										}
										if (!(text == "SLOPE_60_80"))
										{
											return 0;
										}
										return 76;
									}
									else
									{
										if (!(text == "SLOPE_LIFT_50_0"))
										{
											return 0;
										}
										return 93;
									}
								}
								else
								{
									if (!(text == "SPIKE_TOP"))
									{
										return 0;
									}
									return 155;
								}
							}
							else if (num != 255627245U)
							{
								if (num != 270239378U)
								{
									if (num != 282773384U)
									{
										return 0;
									}
									if (!(text == "SLOPE_80_60"))
									{
										return 0;
									}
									return 106;
								}
								else if (!(text == "S_20_0"))
								{
									return 0;
								}
							}
							else
							{
								if (!(text == "SLOPE_0_50"))
								{
									return 0;
								}
								return 64;
							}
							return 109;
						}
						if (num <= 970847422U)
						{
							if (num <= 660469755U)
							{
								if (num != 292659428U)
								{
									if (num != 498864251U)
									{
										if (num != 660469755U)
										{
											return 0;
										}
										if (!(text == "S_75_100"))
										{
											return 0;
										}
										return 72;
									}
									else if (!(text == "SLOPE_0_100"))
									{
										return 0;
									}
								}
								else
								{
									if (!(text == "SLOPE_50_100"))
									{
										return 0;
									}
									return 65;
								}
							}
							else if (num != 754705520U)
							{
								if (num != 862566365U)
								{
									if (num != 970847422U)
									{
										return 0;
									}
									if (!(text == "S_66_33"))
									{
										return 0;
									}
									return 99;
								}
								else
								{
									if (!(text == "SLOPE_66_100"))
									{
										return 0;
									}
									return 68;
								}
							}
							else
							{
								if (!(text == "SLOPE_40_20"))
								{
									return 0;
								}
								return 108;
							}
						}
						else if (num <= 1150335578U)
						{
							if (num != 988480510U)
							{
								if (num != 1004279213U)
								{
									if (num != 1150335578U)
									{
										return 0;
									}
									if (!(text == "SL_50_100"))
									{
										return 0;
									}
									return 79;
								}
								else
								{
									if (!(text == "LIFT_ON_THUNDER"))
									{
										return 0;
									}
									return 122;
								}
							}
							else
							{
								if (!(text == "GROUND"))
								{
									return 0;
								}
								return 10;
							}
						}
						else if (num != 1155739620U)
						{
							if (num != 1162228748U)
							{
								if (num != 1176132556U)
								{
									return 0;
								}
								if (!(text == "SLOPE_100_50"))
								{
									return 0;
								}
								return 96;
							}
							else
							{
								if (!(text == "BLAZING"))
								{
									return 0;
								}
								return 200;
							}
						}
						else
						{
							if (!(text == "BLAZING_IMITATE_BOX"))
							{
								return 0;
							}
							return 199;
						}
						return 78;
					}
					if (num > 1700806885U)
					{
						if (num <= 1771364073U)
						{
							if (num <= 1734266300U)
							{
								if (num != 1718926927U)
								{
									if (num != 1722937870U)
									{
										if (num != 1734266300U)
										{
											return 0;
										}
										if (!(text == "S_0_20"))
										{
											return 0;
										}
									}
									else
									{
										if (!(text == "GAS_STOPPER"))
										{
											return 0;
										}
										return 62;
									}
								}
								else
								{
									if (!(text == "S_100_75"))
									{
										return 0;
									}
									return 101;
								}
							}
							else if (num != 1743134708U)
							{
								if (num != 1769406879U)
								{
									if (num != 1771364073U)
									{
										return 0;
									}
									if (!(text == "S_25_0"))
									{
										return 0;
									}
									return 104;
								}
								else
								{
									if (!(text == "S_100_66"))
									{
										return 0;
									}
									return 98;
								}
							}
							else
							{
								if (!(text == "SLOPE_20_40"))
								{
									return 0;
								}
								return 74;
							}
						}
						else if (num <= 2050288025U)
						{
							if (num != 1784746252U)
							{
								if (num != 2009184877U)
								{
									if (num != 2050288025U)
									{
										return 0;
									}
									if (!(text == "SLOPE_0_25"))
									{
										return 0;
									}
									return 69;
								}
								else
								{
									if (!(text == "S_75_50"))
									{
										return 0;
									}
									return 102;
								}
							}
							else
							{
								if (!(text == "S_0_33"))
								{
									return 0;
								}
								return 66;
							}
						}
						else if (num != 2063314167U)
						{
							if (num != 2073669687U)
							{
								if (num != 2100620882U)
								{
									return 0;
								}
								if (!(text == "SLOPE_0_20"))
								{
									return 0;
								}
							}
							else
							{
								if (!(text == "S_100_80"))
								{
									return 0;
								}
								return 105;
							}
						}
						else
						{
							if (!(text == "SLOPE_25_0"))
							{
								return 0;
							}
							return 104;
						}
						return 73;
					}
					if (num <= 1571447962U)
					{
						if (num <= 1482982998U)
						{
							if (num != 1234084081U)
							{
								if (num != 1265580442U)
								{
									if (num != 1482982998U)
									{
										return 0;
									}
									if (!(text == "LIFT_BOX"))
									{
										return 0;
									}
									return 121;
								}
								else
								{
									if (!(text == "THROUGH"))
									{
										return 0;
									}
									return 4;
								}
							}
							else
							{
								if (!(text == "SLOPE_LIFT_0_50"))
								{
									return 0;
								}
								return 80;
							}
						}
						else if (num != 1491694078U)
						{
							if (num != 1510398878U)
							{
								if (num != 1571447962U)
								{
									return 0;
								}
								if (!(text == "S_60_80"))
								{
									return 0;
								}
								return 76;
							}
							else
							{
								if (!(text == "WATER_ON_GAS_STOPPER"))
								{
									return 0;
								}
								return 63;
							}
						}
						else
						{
							if (!(text == "S_40_20"))
							{
								return 0;
							}
							return 108;
						}
					}
					else if (num <= 1633453491U)
					{
						if (num != 1596686709U)
						{
							if (num != 1614197320U)
							{
								if (num != 1633453491U)
								{
									return 0;
								}
								if (!(text == "S_0_50"))
								{
									return 0;
								}
								return 64;
							}
							else
							{
								if (!(text == "CEIL"))
								{
									return 0;
								}
								return 160;
							}
						}
						else
						{
							if (!(text == "SPIKE"))
							{
								return 0;
							}
							return 125;
						}
					}
					else if (num != 1659478976U)
					{
						if (num != 1683933443U)
						{
							if (num != 1700806885U)
							{
								return 0;
							}
							if (!(text == "S_100_0"))
							{
								return 0;
							}
							return 95;
						}
						else if (!(text == "S_0_25"))
						{
							return 0;
						}
					}
					else
					{
						if (!(text == "SLOPE_33_66"))
						{
							return 0;
						}
						return 67;
					}
					return 69;
				}
				if (num <= 3147378157U)
				{
					if (num > 2523747006U)
					{
						if (num <= 2971087770U)
						{
							if (num <= 2671137910U)
							{
								if (num != 2588825631U)
								{
									if (num != 2611462139U)
									{
										if (num != 2671137910U)
										{
											return 0;
										}
										if (!(text == "FENCE"))
										{
											return 0;
										}
										return 124;
									}
									else
									{
										if (!(text == "SLOPE_100_0"))
										{
											return 0;
										}
										return 95;
									}
								}
								else
								{
									if (!(text == "SLOPE_75_50"))
									{
										return 0;
									}
									return 102;
								}
							}
							else if (num != 2721451859U)
							{
								if (num != 2909901304U)
								{
									if (num != 2971087770U)
									{
										return 0;
									}
									if (!(text == "SL_100_50"))
									{
										return 0;
									}
									return 92;
								}
								else
								{
									if (!(text == "IMITATE_BOX"))
									{
										return 0;
									}
									return 14;
								}
							}
							else if (!(text == "S_80_100"))
							{
								return 0;
							}
						}
						else if (num <= 3093605763U)
						{
							if (num != 2988726098U)
							{
								if (num != 3068678016U)
								{
									if (num != 3093605763U)
									{
										return 0;
									}
									if (!(text == "BOX_UNDER"))
									{
										return 0;
									}
									return 30;
								}
								else
								{
									if (!(text == "IMITATE_LIFT_BOX"))
									{
										return 0;
									}
									return 110;
								}
							}
							else
							{
								if (!(text == "S_33_66"))
								{
									return 0;
								}
								return 67;
							}
						}
						else if (num != 3098994933U)
						{
							if (num != 3106582530U)
							{
								if (num != 3147378157U)
								{
									return 0;
								}
								if (!(text == "SL_100_0"))
								{
									return 0;
								}
								return 94;
							}
							else
							{
								if (!(text == "S_33_0"))
								{
									return 0;
								}
								return 100;
							}
						}
						else if (!(text == "SLOPE_80_100"))
						{
							return 0;
						}
						return 77;
					}
					if (num <= 2258162232U)
					{
						if (num <= 2218211310U)
						{
							if (num != 2113218716U)
							{
								if (num != 2153922262U)
								{
									if (num != 2218211310U)
									{
										return 0;
									}
									if (!(text == "SLOPE_0_33"))
									{
										return 0;
									}
									return 66;
								}
								else
								{
									if (!(text == "S_25_50"))
									{
										return 0;
									}
									return 70;
								}
							}
							else
							{
								if (!(text == "SLOPE_33_0"))
								{
									return 0;
								}
								return 100;
							}
						}
						else if (num != 2219275255U)
						{
							if (num != 2236299714U)
							{
								if (num != 2258162232U)
								{
									return 0;
								}
								if (!(text == "SLOPE_60_40"))
								{
									return 0;
								}
								return 107;
							}
							else
							{
								if (!(text == "S_20_40"))
								{
									return 0;
								}
								return 74;
							}
						}
						else
						{
							if (!(text == "SLOPE_50_0"))
							{
								return 0;
							}
							return 97;
						}
					}
					else
					{
						if (num <= 2435349182U)
						{
							if (num != 2323682296U)
							{
								if (num != 2389954129U)
								{
									if (num != 2435349182U)
									{
										return 0;
									}
									if (!(text == "S_50_25"))
									{
										return 0;
									}
								}
								else
								{
									if (!(text == "SLOPE_50_75"))
									{
										return 0;
									}
									return 71;
								}
							}
							else if (!(text == "SLOPE_50_25"))
							{
								return 0;
							}
							return 103;
						}
						if (num != 2457852692U)
						{
							if (num != 2468525823U)
							{
								if (num != 2523747006U)
								{
									return 0;
								}
								if (!(text == "S_80_60"))
								{
									return 0;
								}
								return 106;
							}
							else if (!(text == "SLOPE_LIFT_100_0"))
							{
								return 0;
							}
						}
						else
						{
							if (!(text == "SLOPE_66_33"))
							{
								return 0;
							}
							return 99;
						}
					}
					return 94;
				}
				if (num <= 3609181501U)
				{
					if (num <= 3315628790U)
					{
						if (num <= 3288288169U)
						{
							if (num != 3275168139U)
							{
								if (num != 3275168587U)
								{
									if (num != 3288288169U)
									{
										return 0;
									}
									if (!(text == "SLOPE_100_80"))
									{
										return 0;
									}
									return 105;
								}
								else
								{
									if (!(text == "SL_0_50"))
									{
										return 0;
									}
									return 80;
								}
							}
							else
							{
								if (!(text == "S_66_100"))
								{
									return 0;
								}
								return 68;
							}
						}
						else if (num != 3291408952U)
						{
							if (num != 3305491988U)
							{
								if (num != 3315628790U)
								{
									return 0;
								}
								if (!(text == "S_60_40"))
								{
									return 0;
								}
								return 107;
							}
							else if (!(text == "SLOPE_40_60"))
							{
								return 0;
							}
						}
						else
						{
							if (!(text == "FRONT"))
							{
								return 0;
							}
							return 60;
						}
					}
					else if (num <= 3499726384U)
					{
						if (num != 3352465375U)
						{
							if (num != 3353660517U)
							{
								if (num != 3499726384U)
								{
									return 0;
								}
								if (!(text == "SLOPE_LIFT_100_50"))
								{
									return 0;
								}
								return 92;
							}
							else
							{
								if (!(text == "S_50_0"))
								{
									return 0;
								}
								return 97;
							}
						}
						else
						{
							if (!(text == "SLOPE_LIFT_0_100"))
							{
								return 0;
							}
							return 81;
						}
					}
					else if (num != 3559295664U)
					{
						if (num != 3592550977U)
						{
							if (num != 3609181501U)
							{
								return 0;
							}
							if (!(text == "SLOPE_100_75"))
							{
								return 0;
							}
							return 101;
						}
						else
						{
							if (!(text == "SLOPE_100_66"))
							{
								return 0;
							}
							return 98;
						}
					}
					else
					{
						if (!(text == "WATER"))
						{
							return 0;
						}
						return 61;
					}
				}
				else if (num <= 3997906498U)
				{
					if (num <= 3810852344U)
					{
						if (num != 3708375537U)
						{
							if (num != 3772758714U)
							{
								if (num != 3810852344U)
								{
									return 0;
								}
								if (!(text == "SLOPE_25_50"))
								{
									return 0;
								}
								return 70;
							}
							else if (!(text == "S_40_60"))
							{
								return 0;
							}
						}
						else
						{
							if (!(text == "SLOPE_75_100"))
							{
								return 0;
							}
							return 72;
						}
					}
					else if (num != 3971371275U)
					{
						if (num != 3984199682U)
						{
							if (num != 3997906498U)
							{
								return 0;
							}
							if (!(text == "S_50_100"))
							{
								return 0;
							}
							return 65;
						}
						else
						{
							if (!(text == "S_100_50"))
							{
								return 0;
							}
							return 96;
						}
					}
					else
					{
						if (!(text == "HOLE"))
						{
							return 0;
						}
						return 140;
					}
				}
				else if (num <= 4129733789U)
				{
					if (num != 4072346824U)
					{
						if (num != 4095393212U)
						{
							if (num != 4129733789U)
							{
								return 0;
							}
							if (!(text == "SL_50_0"))
							{
								return 0;
							}
							return 93;
						}
						else
						{
							if (!(text == "LIFT"))
							{
								return 0;
							}
							return 120;
						}
					}
					else
					{
						if (!(text == "SLOPE_LIFT_50_100"))
						{
							return 0;
						}
						return 79;
					}
				}
				else if (num != 4137383977U)
				{
					if (num != 4210612418U)
					{
						if (num != 4246493391U)
						{
							return 0;
						}
						if (!(text == "S_50_75"))
						{
							return 0;
						}
						return 71;
					}
					else
					{
						if (!(text == "BLOCK"))
						{
							return 0;
						}
						return 128;
					}
				}
				else
				{
					if (!(text == "WATER_CUT_LIFT"))
					{
						return 0;
					}
					return 123;
				}
				return 75;
			}
			return 0;
		}

		public const int BLAZING_LIFT = 201;

		public const int BLAZING = 200;

		public const int BLAZING_IMITATE_BOX = 199;

		public const int CEIL = 160;

		public const int HOLE = 140;

		public const int BLOCK = 128;

		public const int SPIKE = 125;

		public const int SPIKE_TOP = 155;

		public const int LIFT = 120;

		public const int LIFT_BOX = 121;

		public const int LIFT_ON_THUNDER = 122;

		public const int WATER_CUT_LIFT = 123;

		public const int LIFT_ON_WATER = 126;

		public const int FENCE = 124;

		public const int IMITATE_LIFT_BOX = 110;

		public const int SLOPE_0_50 = 64;

		public const int SLOPE_50_100 = 65;

		public const int SLOPE_0_33 = 66;

		public const int SLOPE_33_66 = 67;

		public const int SLOPE_66_100 = 68;

		public const int SLOPE_0_25 = 69;

		public const int SLOPE_25_50 = 70;

		public const int SLOPE_50_75 = 71;

		public const int SLOPE_75_100 = 72;

		public const int SLOPE_0_20 = 73;

		public const int SLOPE_20_40 = 74;

		public const int SLOPE_40_60 = 75;

		public const int SLOPE_60_80 = 76;

		public const int SLOPE_80_100 = 77;

		public const int SLOPE_0_100 = 78;

		public const int SLOPE_LIFT_50_100 = 79;

		public const int SLOPE_LIFT_0_50 = 80;

		public const int SLOPE_LIFT_0_100 = 81;

		public const int SLOPE_LIFT_100_50 = 92;

		public const int SLOPE_LIFT_50_0 = 93;

		public const int SLOPE_LIFT_100_0 = 94;

		public const int SLOPE_100_0 = 95;

		public const int SLOPE_50_0 = 97;

		public const int SLOPE_100_50 = 96;

		public const int SLOPE_33_0 = 100;

		public const int SLOPE_66_33 = 99;

		public const int SLOPE_100_66 = 98;

		public const int SLOPE_25_0 = 104;

		public const int SLOPE_50_25 = 103;

		public const int SLOPE_75_50 = 102;

		public const int SLOPE_100_75 = 101;

		public const int SLOPE_20_0 = 109;

		public const int SLOPE_40_20 = 108;

		public const int SLOPE_60_40 = 107;

		public const int SLOPE_80_60 = 106;

		public const int SLOPE_100_80 = 105;

		public const int WATER_ON_GAS_STOPPER = 63;

		public const int GAS_STOPPER = 62;

		public const int WATER = 61;

		public const int FRONT = 60;

		public const int MISTFILL_STOPPER = 31;

		public const int GROUND = 10;

		public const int BOX_UNDER = 30;

		public const int IMITATE_BOX = 14;

		public const int THUNDER = 20;

		public const int THROUGH = 4;

		public const int NOIMAGE = 0;

		private static MeshDrawer Md;

		private static MeshDrawer MdI;

		private static float clen;

		private static float clenh;

		private static float scl;

		private static PxlSequence SqIco;

		private enum ICO
		{
			THUNDER,
			BLAZE,
			BOX,
			GROUND,
			CEIL,
			THROUGH,
			BLOCK,
			LIFT,
			SPIKE,
			FENCE,
			GAS_STOPPER,
			BIKKURI,
			MISTFILL_STOPPER
		}
	}
}
