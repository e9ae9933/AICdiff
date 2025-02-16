using System;
using UnityEngine;

namespace XX
{
	public class TransFader
	{
		public static void drawShapeOne(TFKEY key, MeshDrawer Md, float tz, float pw = 0f, float ph = 0f)
		{
			float num = TransFader.Pt.x - pw / 2f;
			float num2 = TransFader.Pt.y - ph / 2f;
			switch (key)
			{
			case TFKEY.SD:
			{
				float num3 = TransFader.SD_RWH1 - TransFader.SD_RWHH1 * tz;
				Md.Tri(0).Tri(2).Tri(1)
					.Tri(0)
					.Tri(3)
					.Tri(2);
				Md.PosD(num, num2 + TransFader.SD_RWH1, null);
				Md.PosD(num - TransFader.SD_RWHH1 * tz, num2 + num3, null);
				Md.PosD(num, num2 + TransFader.SD_RWH1 * (1f - tz), null);
				Md.PosD(num + TransFader.SD_RWHH1 * tz, num2 + num3, null);
				return;
			}
			case TFKEY.SD2:
			{
				float num4 = num;
				float num3 = num2 + (float)TransFader.SD_RWHH;
				tz = X.ZSIN(tz);
				int sd_RWHH = TransFader.SD_RWHH;
				Md.Tri(0).Tri(2).Tri(1)
					.Tri(0)
					.Tri(3)
					.Tri(2);
				for (int i = 0; i < 4; i++)
				{
					Vector2 vector = X.XYLR(num4, num3, (float)TransFader.SD_RWHH * 0.5f, 1.5707964f * (float)i);
					vector = X.XYLR(vector.x, vector.y, (float)TransFader.SD_RWHH * 0.5f, 1.5707964f * (float)i + (1f - tz) * 3.1415927f);
					Md.PosD(vector.x, vector.y, null);
				}
				return;
			}
			case TFKEY.SYASEN:
			case TFKEY.SYASEN2:
			{
				float num5 = (((key == TFKEY.SYASEN) ? TransFader.SYASEN_L : TransFader.SYASEN2_L) + 1f) * tz;
				Md.Tri(0).Tri(1).Tri(2)
					.Tri(0)
					.Tri(2)
					.Tri(3);
				Md.PosD(TransFader.Pt.x - pw / 2f, 0f - ph / 2f, null);
				Md.PosD(0f - pw / 2f, TransFader.Pt.x - ph / 2f, null);
				Md.PosD(0f - pw / 2f, TransFader.Pt.x + num5 - ph / 2f, null);
				Md.PosD(TransFader.Pt.x + num5 - pw / 2f, 0f - ph / 2f, null);
				return;
			}
			case TFKEY.DOT1:
				Md.RectBL(num, num2, 1f, 1f, false);
				return;
			case TFKEY.RECT28:
			{
				float num4 = num + 14f;
				float num3 = num2 + 14f;
				float num6 = 28f * tz;
				Md.Rect(num4, num3, num6, num6, false);
				return;
			}
			case TFKEY.RECT56:
			{
				float num4 = num + 28f;
				float num3 = num2 + 28f;
				float num6 = 56f * tz;
				Md.Rect(num4, num3, num6, num6, false);
				return;
			}
			case TFKEY.MOVE_BLIND_0:
			{
				float num7 = X.ZPOW(tz);
				float num4 = ((pw > 0f) ? X.NAIBUN_I(0f, TransFader.Pt.x, X.ZSIN(tz)) : TransFader.Pt.x);
				Md.RectBL(num4 - pw / 2f, num2, TransFader.PtShape.x * num7, ph, false);
				return;
			}
			case TFKEY.MOVE_BLIND_1:
			{
				float num7 = X.ZPOW(tz);
				float num3 = ((ph > 0f) ? X.NAIBUN_I(0f, TransFader.Pt.y, X.ZSIN(tz)) : TransFader.Pt.y);
				float num6 = TransFader.PtShape.y * num7;
				Md.RectBL(num, num3 - num6 - ph / 2f, pw, num6, false);
				return;
			}
			case TFKEY.MOVE_BLIND_2:
			{
				float num7 = X.ZPOW(tz);
				float num4 = ((pw > 0f) ? X.NAIBUN_I(pw, TransFader.Pt.x, X.ZSIN(tz)) : TransFader.Pt.x);
				float num6 = TransFader.PtShape.x * num7;
				Md.RectBL(num4 - num6 - pw / 2f, num2, num6, ph, false);
				return;
			}
			case TFKEY.MOVE_BLIND_3:
			{
				float num7 = X.ZPOW(tz);
				float num3 = ((ph > 0f) ? X.NAIBUN_I(ph, TransFader.Pt.y, X.ZSIN(tz)) : TransFader.Pt.y);
				Md.RectBL(num, num3 - ph / 2f, pw, TransFader.PtShape.y * num7, false);
				return;
			}
			case TFKEY.SC_STARDROP:
			{
				uint ran = X.GETRAN2((int)TransFader.Pt.x, 23 + (int)TransFader.PtShape.z % 19);
				float num8 = X.ZSIN(X.RAN(ran, 2871)) * 0.53f;
				tz = (tz - num8) / (1f - num8);
				if (tz > 0f)
				{
					float num9 = (TransFader.PtShape.x + TransFader.PtShape.y) / 2f;
					float num10 = num9 * (0.3f + 0.6f * X.RAN(ran, 7731));
					float num11 = num10 * (X.ZSIN(tz, 0.3f) * 1.2f - X.ZCOS(tz - 0.3f, 0.7f) * 0.2f);
					float num12 = X.ZPOW(tz) * (ph + (num10 + 83f * X.RAN(ran, 4914)));
					float num13 = (0.04f + 0.2f * X.RAN(ran, 5927)) * (float)X.MPF(ran % 2U == 1U) * 6.2831855f * tz;
					Md.Star(num - num12, num2 - num12, num11, num13, 5, 0.6f, 0f, false, 0f, 0f);
					float num14 = 0.1f + X.ZSIN(X.RAN(ran, 2871)) * 0.28f;
					float num15 = 2f + num9 * 1f * X.ZCOS((tz - num14) / (1f - num14));
					Md.Line(num + num15, num2 + num15, num - num12, num2 - num12, num15, false, 0f, 0f);
				}
				return;
			}
			default:
				return;
			}
		}

		public static bool scrollPoint(TFKEY key, float pw, float ph, bool _x = false, bool _y = false)
		{
			if (!_x || !_y)
			{
				switch (key)
				{
				case TFKEY.SD:
				case TFKEY.SD2:
					if (!_x && !_y)
					{
						if (TransFader.Pt.x > pw)
						{
							if (TransFader.Pt.y > ph - (float)TransFader.SD_RWHH)
							{
								return false;
							}
							_y = true;
						}
						else
						{
							_x = true;
						}
					}
					if (_y)
					{
						TransFader.Pt.y = TransFader.Pt.y + (float)TransFader.SD_RWHH;
						TransFader.Pt.x = (float)((X.IntR(TransFader.Pt.y / (float)TransFader.SD_RWHH) % 2 > 0) ? (-(float)TransFader.SD_RWHH) : (-(float)TransFader.SD_RWH));
					}
					else
					{
						if (!_x)
						{
							return false;
						}
						TransFader.Pt.x = TransFader.Pt.x + (float)TransFader.SD_RWH;
					}
					break;
				case TFKEY.SYASEN:
				case TFKEY.SYASEN2:
				{
					if (!_x && !_y)
					{
						if (TransFader.Pt.x > pw + ph)
						{
							return false;
						}
						_x = true;
					}
					if (!_x)
					{
						return false;
					}
					float num = ((key == TFKEY.SYASEN) ? TransFader.SYASEN_L : TransFader.SYASEN2_L);
					TransFader.Pt.x = TransFader.Pt.x + num;
					TransFader.Pt.y = TransFader.Pt.y + num;
					break;
				}
				case TFKEY.DOT1:
					if (!_x && !_y)
					{
						if (TransFader.Pt.x > pw)
						{
							if (TransFader.Pt.y > ph)
							{
								return false;
							}
							_y = true;
						}
						else
						{
							_x = true;
						}
					}
					if (_x)
					{
						TransFader.Pt.x = TransFader.Pt.x + 1f;
					}
					else
					{
						if (!_y)
						{
							return false;
						}
						TransFader.Pt.y = TransFader.Pt.y + 1f;
						TransFader.Pt.x = 0f;
					}
					break;
				case TFKEY.RECT28:
					if (!_x && !_y)
					{
						if (TransFader.Pt.x > pw)
						{
							if (TransFader.Pt.y > ph)
							{
								return false;
							}
							_y = true;
						}
						else
						{
							_x = true;
						}
					}
					if (_x)
					{
						TransFader.Pt.x = TransFader.Pt.x + 28f;
					}
					else
					{
						if (!_y)
						{
							return false;
						}
						TransFader.Pt.y = TransFader.Pt.y + 28f;
						TransFader.Pt.x = 0f;
					}
					break;
				case TFKEY.RECT56:
					if (!_x && !_y)
					{
						if (TransFader.Pt.x > pw)
						{
							if (TransFader.Pt.y > ph)
							{
								return false;
							}
							_y = true;
						}
						else
						{
							_x = true;
						}
					}
					if (_x)
					{
						TransFader.Pt.x = TransFader.Pt.x + 56f;
					}
					else
					{
						if (!_y)
						{
							return false;
						}
						TransFader.Pt.y = TransFader.Pt.y + 56f;
						TransFader.Pt.x = 0f;
					}
					break;
				case TFKEY.MOVE_BLIND_0:
					TransFader.PtShape.x = 12f + 20f * X.ZLINE(1f - TransFader.Pt.x / (pw * 0.8f));
					if (!_x && !_y)
					{
						if (TransFader.Pt.x < 0f)
						{
							return false;
						}
						_x = true;
					}
					if (!_x)
					{
						return false;
					}
					TransFader.Pt.x = TransFader.Pt.x - TransFader.PtShape.x;
					break;
				case TFKEY.MOVE_BLIND_1:
					TransFader.PtShape.y = 12f + 20f * X.ZLINE(TransFader.Pt.y, ph * 0.8f);
					if (!_x && !_y)
					{
						if (TransFader.Pt.y > ph)
						{
							return false;
						}
						_y = true;
					}
					if (!_y)
					{
						return false;
					}
					TransFader.Pt.y = TransFader.Pt.y + TransFader.PtShape.y;
					break;
				case TFKEY.MOVE_BLIND_2:
					TransFader.PtShape.x = 12f + 20f * X.ZLINE(TransFader.Pt.x, pw * 0.8f);
					if (!_x && !_y)
					{
						if (TransFader.Pt.x > pw)
						{
							return false;
						}
						_x = true;
					}
					if (!_x)
					{
						return false;
					}
					TransFader.Pt.x = TransFader.Pt.x + TransFader.PtShape.x;
					break;
				case TFKEY.MOVE_BLIND_3:
					TransFader.PtShape.y = 12f + 20f * X.ZLINE(1f - TransFader.Pt.y / (ph * 0.8f));
					if (!_x && !_y)
					{
						if (TransFader.Pt.y < 0f)
						{
							return false;
						}
						_y = true;
					}
					if (!_y)
					{
						return false;
					}
					TransFader.Pt.y = TransFader.Pt.y - TransFader.PtShape.y;
					break;
				case TFKEY.SC_STARDROP:
				{
					TransFader.PtShape.y = TransFader.PtShape.x;
					uint ran = X.GETRAN2(49 + (int)TransFader.PtShape.z * 3, 13);
					TransFader.PtShape.x = 40f + 144f * X.ZSINV(X.RAN(ran, 5487));
					if (!_x && !_y)
					{
						if (TransFader.Pt.x < -80f)
						{
							return false;
						}
						_x = true;
					}
					if (!_x)
					{
						return false;
					}
					TransFader.Pt.x = TransFader.Pt.x - (TransFader.PtShape.x + TransFader.PtShape.y) / 2f;
					TransFader.PtShape.z = TransFader.PtShape.z + 1f;
					break;
				}
				default:
					return false;
				}
				return true;
			}
			TransFader.PtSht.x = (TransFader.PtSht.y = 0f);
			switch (key)
			{
			case TFKEY.SD:
			case TFKEY.SD2:
				TransFader.PtShape.x = (float)TransFader.SD_RWHH;
				TransFader.PtShape.y = 0f;
				TransFader.Pt.x = (float)(-(float)TransFader.SD_RWHH);
				TransFader.Pt.y = (float)(-(float)TransFader.SD_RWHH);
				return true;
			case TFKEY.SYASEN:
			case TFKEY.SYASEN2:
			case TFKEY.DOT1:
			case TFKEY.MOVE_BLIND_1:
			case TFKEY.MOVE_BLIND_2:
				TransFader.PtShape.Set(0f, 0f, 0f);
				TransFader.Pt.x = 0f;
				TransFader.Pt.y = 0f;
				return true;
			case TFKEY.RECT28:
				TransFader.PtShape.Set(0f, 0f, 0f);
				TransFader.Pt.x = 0f;
				TransFader.Pt.y = 0f;
				TransFader.PtSht.x = -14f;
				TransFader.PtSht.y = -14f;
				return true;
			case TFKEY.RECT56:
				TransFader.PtShape.Set(0f, 0f, 0f);
				TransFader.Pt.x = 0f;
				TransFader.Pt.y = 0f;
				TransFader.PtSht.x = -28f;
				TransFader.PtSht.y = -28f;
				return true;
			case TFKEY.MOVE_BLIND_0:
				TransFader.PtShape.Set(0f, 0f, 0f);
				TransFader.Pt.x = pw;
				TransFader.Pt.y = 0f;
				return true;
			case TFKEY.MOVE_BLIND_3:
				TransFader.PtShape.Set(0f, 0f, 0f);
				TransFader.Pt.x = 0f;
				TransFader.Pt.y = ph;
				return true;
			case TFKEY.SC_STARDROP:
				TransFader.PtShape.Set(60f, 50f, 0f);
				TransFader.Pt.x = pw + ph + 80f;
				TransFader.Pt.y = ph;
				TransFader.PtSht.x = ph;
				TransFader.PtSht.y = ph / 2f;
				return true;
			default:
				return true;
			}
		}

		private static void initDrawLevel(TFKEY anim_key, TFANIM anim_id, float pw, float ph, float af, float maxt, float ran_level = 0f, float oneitem_maxt_level = 0.3f)
		{
			if (anim_id == TFANIM.WHOLE)
			{
				TransFader.anm_maxt_far = 0f;
				TransFader.anm_maxt_one = maxt;
				TransFader.anm_t_ran = 0;
			}
			else
			{
				float num = oneitem_maxt_level;
				if (anim_key == TFKEY.DOT1)
				{
					num = 0f;
				}
				TransFader.anm_maxt_one = (float)X.Mx(1, X.IntC(maxt * num));
				TransFader.anm_t_ran = X.IntC((maxt - TransFader.anm_maxt_one) * ran_level);
				TransFader.anm_maxt_far = X.Mx(0f, maxt - TransFader.anm_maxt_one - (float)TransFader.anm_t_ran);
				if (anim_id == TFANIM.DISSOLVE || ran_level > 0f)
				{
					int num2 = IN.totalframe - (int)af;
					TransFader.anm_ran = (int)(X.GETRAN2(num2, 7 + num2 % 47) % 1975852U);
				}
			}
			TransFader.getDrawLevel(anim_id, 0, pw, ph, af, maxt);
		}

		private static float getDrawLevel(TFANIM anim_id, int len, float pw, float ph, float af, float maxt)
		{
			float num = 0f;
			uint num2 = 0U;
			if (len > 0 && TransFader.anm_t_ran > 0)
			{
				num2 = X.GETRAN2(TransFader.anm_ran + (int)(TransFader.Pt.x * 1.43f + TransFader.Pt.y + 1.37f), 2 + TransFader.anm_ran % 112 + (int)(TransFader.Pt.x * 2.1f + TransFader.Pt.y + 3.11f) % 251);
				num2 = (uint)((float)TransFader.anm_t_ran * X.RAN(num2, 2302));
			}
			float num3 = TransFader.Pt.x - TransFader.PtSht.x;
			float num4 = TransFader.Pt.y - TransFader.PtSht.y;
			switch (anim_id)
			{
			case TFANIM.L2R:
			case TFANIM.R2L:
				if (len == 0)
				{
					TransFader.anm_len = pw;
					TransFader.anm_sx = ((anim_id == TFANIM.L2R) ? 0f : pw);
					return -1f;
				}
				num = X.Abs(num3 - TransFader.anm_sx) / TransFader.anm_len * TransFader.anm_maxt_far;
				break;
			case TFANIM.T2B:
			case TFANIM.B2T:
				if (len == 0)
				{
					TransFader.anm_len = ph;
					TransFader.anm_sy = ((anim_id == TFANIM.B2T) ? 0f : ph);
					return -1f;
				}
				num = X.Abs(num4 - TransFader.anm_sy) / TransFader.anm_len * TransFader.anm_maxt_far;
				break;
			case TFANIM.LT2RB:
			case TFANIM.TR2BL:
			case TFANIM.BL2TR:
			case TFANIM.RB2LT:
				if (len == 0)
				{
					TransFader.anm_len = X.LENGTHXYS(0f, 0f, pw, ph);
					TransFader.anm_sx = ((anim_id == TFANIM.LT2RB || anim_id == TFANIM.BL2TR) ? 0f : pw);
					TransFader.anm_sy = ((anim_id == TFANIM.LT2RB || anim_id == TFANIM.TR2BL) ? ph : 0f);
					return -1f;
				}
				num = X.LENGTHXYS(0f, 0f, X.Abs(num3 - TransFader.anm_sx), X.Abs(num4 - TransFader.anm_sy)) / TransFader.anm_len * TransFader.anm_maxt_far;
				break;
			case TFANIM.EYE_OPEN:
			case TFANIM.EYE_CLOSE:
				if (len == 0)
				{
					TransFader.anm_len = ph * 0.5f;
					TransFader.anm_sy = ph * 0.5f;
					return -1f;
				}
				if (anim_id == TFANIM.EYE_OPEN)
				{
					num = X.Abs(num4 - TransFader.anm_sy);
				}
				else
				{
					num = X.Mn(num4, ph - num4);
				}
				num = num / TransFader.anm_len * TransFader.anm_maxt_far;
				break;
			case TFANIM.DOOR_OPEN:
			case TFANIM.DOOR_CLOSE:
				if (len == 0)
				{
					TransFader.anm_len = pw * 0.5f;
					TransFader.anm_sx = pw * 0.5f;
					return -1f;
				}
				if (anim_id == TFANIM.DOOR_OPEN)
				{
					num = X.Abs(num3 - TransFader.anm_sx);
				}
				else
				{
					num = X.Mn(num3, pw - num3);
				}
				num = num / TransFader.anm_len * TransFader.anm_maxt_far;
				break;
			case TFANIM.EXPAND:
			case TFANIM.CONTRACT:
				if (len == 0)
				{
					TransFader.anm_len = X.LENGTHXYS(0f, 0f, pw * 0.5f, ph * 0.5f);
					TransFader.anm_sx = pw * 0.5f;
					TransFader.anm_sy = ph * 0.5f;
					return -1f;
				}
				if (anim_id == TFANIM.EXPAND)
				{
					num = X.LENGTHXYS(0f, 0f, X.Abs(num3 - TransFader.anm_sx), X.Abs(num4 - TransFader.anm_sy));
				}
				else
				{
					num = X.LENGTHXYS(0f, 0f, X.Mn(num3, pw - num3), X.Mn(num4, ph - num4));
				}
				num = num / TransFader.anm_len * TransFader.anm_maxt_far;
				break;
			case TFANIM.NANAMEDOOR_LT_RB:
			case TFANIM.NANAMEDOOR_TR_BL:
				if (len == 0)
				{
					TransFader.anm_len = (pw + ph) / 2f + 1f;
					TransFader.anm_sx = 0f;
					TransFader.anm_sy = ((anim_id == TFANIM.NANAMEDOOR_LT_RB) ? 0f : ph);
					return -1f;
				}
				num = X.Mn(X.LENGTHXYS(0f, 0f, X.Abs(num3 - TransFader.anm_sx), X.Abs(num4 - TransFader.anm_sy)), X.LENGTHXYS(0f, 0f, X.Abs(pw - num3 - TransFader.anm_sx), X.Abs(ph - num4 - TransFader.anm_sy)));
				num = num / TransFader.anm_len * TransFader.anm_maxt_far;
				break;
			case TFANIM.DISSOLVE:
				if (len == 0)
				{
					return -1f;
				}
				num2 = X.GETRAN2(TransFader.anm_ran + (int)(num3 * 1.3f + num4 + 1.97f), 2 + TransFader.anm_ran % 232 + (int)(num3 * 2.6f + num4 + 5.11f) % 191);
				num = TransFader.anm_maxt_far * X.RAN(num2, 91179);
				num2 = 0U;
				break;
			case TFANIM.ARROW_L:
			case TFANIM.ARROW_T:
			case TFANIM.ARROW_R:
			case TFANIM.ARROW_B:
				if (len == 0)
				{
					TransFader.anm_len = ((anim_id == TFANIM.ARROW_L || anim_id == TFANIM.ARROW_R) ? X.LENGTHXYS(0f, 0f, pw, ph * 0.5f) : X.LENGTHXYS(0f, 0f, pw * 0.5f, ph));
					TransFader.anm_sx = ((anim_id == TFANIM.ARROW_L) ? 0f : ((anim_id == TFANIM.ARROW_R) ? pw : (pw * 0.5f)));
					TransFader.anm_sy = ((anim_id == TFANIM.ARROW_B) ? 0f : ((anim_id == TFANIM.ARROW_T) ? ph : (ph * 0.5f)));
					return -1f;
				}
				num = X.LENGTHXYS(0f, 0f, X.Abs(num3 - TransFader.anm_sx), X.Abs(num4 - TransFader.anm_sy)) / TransFader.anm_len * TransFader.anm_maxt_far;
				break;
			}
			float num5 = af - num - num2;
			if (num5 >= 0f)
			{
				return num5 / TransFader.anm_maxt_one;
			}
			return -1f;
		}

		public static Matrix4x4 MtrP()
		{
			return Matrix4x4.Translate(TransFader.Pt);
		}

		public static Matrix4x4 MtrP(Vector2 dPt)
		{
			return Matrix4x4.Translate(dPt);
		}

		public TransFader(TFKEY _key, float _maxt, float _pw, float _ph, float _scale = 1f)
		{
			this.key = _key;
			this.maxt = _maxt;
			this.pw = _pw;
			this.ph = _ph;
			this.scale = _scale;
			this.len = -1;
			this.resetAnim(TFANIM.NOSET, false, 0f, 0);
		}

		public TransFader WH(float _pw = -1000f, float _ph = -1000f)
		{
			if (_pw != -1000f)
			{
				this.pw = _pw;
			}
			if (_ph != -1000f)
			{
				this.ph = _ph;
			}
			this.draw_flg = true;
			return this;
		}

		public TransFader resetAnim(TFANIM _anim_id = TFANIM.NOSET, bool _reverse = false, float _random_level = 0f, int _delay = 0)
		{
			this.af = (float)(-(float)_delay);
			if (_anim_id != TFANIM.NOSET)
			{
				this.anim_id = _anim_id;
			}
			this.anim_reverse = _reverse;
			this.draw_flg = true;
			this.random_level = _random_level;
			return this;
		}

		public bool redraw(MeshDrawer Md, float fcnt)
		{
			if (!this.draw_flg)
			{
				return false;
			}
			this.draw_flg = ((this.maxt > 30f) ? (this.af < this.maxt - 10f) : (this.af < this.maxt));
			TransFader.scrollPoint(this.key, this.pw, this.ph, true, true);
			TransFader.initDrawLevel(this.key, this.anim_id, this.pw, this.ph, this.af, this.maxt, this.random_level, this.oneitem_maxt_level);
			if (!this.do_not_shape_clear)
			{
				Md.clear(false, false);
			}
			if (this.anim_reverse || this.af >= 0f)
			{
				int num = ((this.fnShapeColor != null) ? 1 : 0);
				do
				{
					float num2 = TransFader.getDrawLevel(this.anim_id, this.len, this.pw, this.ph, this.af, this.maxt);
					if (num2 >= 0f)
					{
						if (num2 > 1f)
						{
							num2 = (float)(this.anim_reverse ? (-1) : 1);
						}
						else
						{
							num2 = (this.anim_reverse ? (1f - num2) : num2);
						}
						if (num2 >= 0f)
						{
							if (num >= 1)
							{
								num = (this.fnShapeColor(this, fcnt, Md, TransFader.Pt, this.af) ? 2 : 1);
							}
							if (num != 1)
							{
								TransFader.drawShapeOne(this.key, Md, num2, this.pw, this.ph);
							}
							if (this.anim_reverse)
							{
								this.draw_flg = true;
							}
							else
							{
								this.draw_flg = this.draw_flg || num2 < 1f;
							}
						}
					}
					else if (this.anim_reverse)
					{
						if (num >= 1)
						{
							num = (this.fnShapeColor(this, fcnt, Md, TransFader.Pt, this.af) ? 2 : 1);
						}
						if (num != 1)
						{
							TransFader.drawShapeOne(this.key, Md, 1f, this.pw, this.ph);
						}
						this.draw_flg = true;
					}
					if (num == 2)
					{
						num = 1;
					}
				}
				while (TransFader.scrollPoint(this.key, this.pw, this.ph, false, false));
			}
			if (!this.do_not_shape_clear)
			{
				Md.updateForMeshRenderer(false);
			}
			this.af += fcnt;
			return this.draw_flg;
		}

		public bool redrawDAF(MeshDrawer Md, bool flag, int fcnt = 1)
		{
			if (!flag)
			{
				return this.draw_flg;
			}
			return this.redraw(Md, (float)fcnt);
		}

		public static TFKEY getRandomKey()
		{
			return (TFKEY)(X.xors() % 12U);
		}

		public static TFANIM getRandomAnim()
		{
			return (TFANIM)(X.xors() % 22U);
		}

		public float getRestTime()
		{
			return this.maxt - this.af;
		}

		public bool isReverse()
		{
			return this.anim_reverse;
		}

		public bool isFinished()
		{
			return !this.draw_flg;
		}

		public static readonly int SD_RWH = 54;

		public static readonly int SD_RWHH = 27;

		public static readonly float SD_RWH1 = (float)TransFader.SD_RWH + 1.7f;

		public static readonly float SD_RWHH1 = TransFader.SD_RWH1 * 0.5f;

		private static readonly float SYASEN_L = 18f;

		private static readonly float SYASEN2_L = 26f;

		public static readonly int SN_RW = 168;

		public static readonly int SN_RH = TransFader.SN_RW / 2;

		public static readonly int SN_RWH = TransFader.SN_RW / 2;

		public static readonly int SN_RHH = TransFader.SN_RH / 2;

		public static readonly float SN_LT = 6f;

		public static readonly float SN_LTH = TransFader.SN_LT * 0.5f;

		private static Vector2 Pt;

		private static Vector2 PtSht;

		private static Vector3 PtShape;

		private int len;

		private TFKEY key;

		protected float pw;

		protected float ph;

		private float scale;

		private float maxt;

		public float af;

		private TFANIM anim_id;

		protected bool anim_reverse;

		protected float random_level;

		public Color32 Col;

		private bool draw_flg = true;

		public bool do_not_shape_clear;

		public float oneitem_maxt_level = 0.3f;

		public FnTransFaderColor fnShapeColor;

		private static float anm_len;

		private static int anm_ran;

		private static float anm_sx;

		private static float anm_sy;

		private static float anm_maxt_far;

		private static float anm_maxt_one;

		private static int anm_t_ran;
	}
}
