using System;
using Better;

namespace XX
{
	public class TransEffecterItem : EffectItem
	{
		public TransEffecterItem(TransEffecter _TE)
			: base(_TE, "te", 0f, 0f, 0f, 0, 0)
		{
		}

		public TransEffecterItem Kind(TEKIND _kind)
		{
			this.kind = _kind;
			if (!TransEffecterItem.OKindTitle.TryGetValue(this.kind, out this.title))
			{
				this.title = (TransEffecterItem.OKindTitle[this.kind] = this.kind.ToString().ToLower());
			}
			return this;
		}

		public int kind_top
		{
			get
			{
				return (int)(this.kind / (TEKIND)10);
			}
		}

		public override EffectItem initEffect(string type_name = "")
		{
			this.af = -1f - this.af;
			return base.initEffect(null);
		}

		public static bool fnRunDraw__default(EffectItem E)
		{
			return false;
		}

		public static bool fnRunDraw_quake_vib(EffectItem E)
		{
			if (E.time == 0 || E.af >= (float)E.time)
			{
				return false;
			}
			float num = X.NAIBUN_I(E.x, E.y, X.ZLINE(E.af / (float)E.time));
			uint ran = X.GETRAN2(E.f0, (int)(E.index % 13U));
			float num2 = (float)X.IntR(num * X.SINI(X.RAN(ran, 1490) * 100f + E.af, 3.7f));
			float num3 = (float)X.IntR(num * X.SINI(X.RAN(ran, 1502) * 100f + E.af, 9.53f));
			TransEffecterItem.TE.translatePos(num2, num3);
			return true;
		}

		public static bool fnRunDraw_quake_sinh(EffectItem E)
		{
			if (E.time == 0 || E.af >= (float)E.time)
			{
				return false;
			}
			float num = X.NAIBUN_I(E.x, E.y, E.af / (float)E.time);
			if (E.z == 0f)
			{
				E.z = 31f;
			}
			num *= X.Sin0(E.af / E.z);
			TransEffecterItem.TE.translatePos(num, 0f);
			return true;
		}

		public static bool fnRunDraw_quake_sinv(EffectItem E)
		{
			if (E.time == 0 || E.af >= (float)E.time)
			{
				return false;
			}
			float num = X.NAIBUN_I(E.x, E.y, E.af / (float)E.time);
			if (E.z == 0f)
			{
				E.z = 31f;
			}
			num *= X.Sin0(E.af / E.z);
			TransEffecterItem.TE.translatePos(0f, num);
			return true;
		}

		public static bool fnRunDraw_fadein(EffectItem E)
		{
			if (E.time == 0)
			{
				return false;
			}
			if (E.time >= 0 && E.af >= (float)E.time)
			{
				return false;
			}
			TransEffecterItem.TE.mulColor(EffectItem.Col1.White().setA1(E.z + (1f - E.z) * X.ZLINE(E.af, (float)X.Abs(E.time))), true);
			return true;
		}

		public static bool fnRunDraw_fadeout(EffectItem E)
		{
			if (E.time < 0)
			{
				return false;
			}
			if (E.af >= (float)E.time)
			{
				return true;
			}
			TransEffecterItem.TE.mulColor(EffectItem.Col1.White().setA1(E.z + (1f - E.z) * (1f - X.ZLINE(E.af, (float)X.Abs(E.time)))), true);
			return true;
		}

		public static bool fnRunDraw_fadeout_in(EffectItem E)
		{
			if (E.time == 0)
			{
				return false;
			}
			if (E.time >= 0 && E.af >= (float)E.time)
			{
				return false;
			}
			float num = X.ZLINE(E.af, (float)X.Abs(E.time));
			TransEffecterItem.TE.mulColor(EffectItem.Col1.White().mulA(E.z + (1f - E.z) * (1f - X.ZLINE(num, 0.5f) + X.ZLINE(num - 0.5f, 0.5f))), true);
			return true;
		}

		public static bool fnRunDraw_color_fadeout(EffectItem E)
		{
			if (E.time == 0)
			{
				return false;
			}
			if (E.z >= 0f && E.af >= E.z)
			{
				return false;
			}
			TransEffecterItem.TE.mulColor(EffectItem.Col1.White().blend((uint)E.time, E.y * (1f - X.ZLINE(E.af, E.z))), false);
			return true;
		}

		private static bool execute_dmg_blink(EffectItem E, bool fading, float white_scr = 0f)
		{
			if (E.z == 0f)
			{
				return false;
			}
			if (E.z > 0f && E.af >= E.z)
			{
				return false;
			}
			float num = X.ZLINE(E.af, E.z);
			if (!fading)
			{
				num = 0f + X.ZSIN(num, 0.2f) * 0.4f + X.ZCOS(num - 0.7f, 0.3f) * 0.6f;
			}
			num = X.Scr(num, 1f - (float)E.EF.getParticleCount(E, 128) / 128f);
			uint maxValue = uint.MaxValue;
			uint num2 = 0U;
			float num3 = E.x;
			float num4 = E.y;
			MGATTR time = (MGATTR)E.time;
			uint num5;
			uint num6;
			switch (time)
			{
			case MGATTR.WIP:
				num5 = 4288413821U;
				num6 = 4288813855U;
				num4 *= 0f;
				goto IL_059B;
			case MGATTR.GRAB:
				num5 = 4285863180U;
				num6 = 4285298020U;
				num *= 0.7f + 0.3f * X.COSI(E.af, 26f);
				num4 *= 0f;
				goto IL_059B;
			case MGATTR.CUT_H:
				num5 = 4294927979U;
				num6 = 0U;
				goto IL_059B;
			case MGATTR.STAB:
			case MGATTR.PRESS:
			case MGATTR.PRESS_PENETRATE:
				break;
			case MGATTR.BITE:
			case MGATTR.BITE_V:
				num5 = 4294901760U;
				num6 = 0U;
				num *= 0.7f + 0.3f * X.COSI(E.af, 26f);
				num4 *= 0f;
				goto IL_059B;
			case MGATTR.BOMB:
				if (TransEffecterItem.TE.is_player)
				{
					num5 = 4294928686U;
					num6 = 2001619530U;
					num *= 0.7f + 0.3f * X.COSI(E.af, E.z);
					num4 *= 1f - X.ZSIN(E.af, E.z * 0.35f);
					goto IL_059B;
				}
				num5 = 4281671401U;
				num6 = 4285519615U;
				num *= 0.7f + 0.3f * X.COSI(E.af, 26f);
				num3 *= 0.5f;
				num4 *= 0.8f;
				goto IL_059B;
			default:
				switch (time)
				{
				case MGATTR.FIRE:
					num5 = 4294928686U;
					num6 = 2001619530U;
					num *= 0.7f + 0.3f * X.COSI(E.af, E.z);
					num4 *= 1f - X.ZSIN(E.af, E.z * 0.35f);
					goto IL_059B;
				case MGATTR.ICE:
					if (X.ANM((int)E.af, 3, 7.2f) == 0)
					{
						num5 = 4291752959U;
					}
					else
					{
						num5 = 4281454591U;
					}
					num6 = 1996501720U;
					num *= 0.7f + 0.3f * X.COSI(E.af, E.z);
					num3 *= 0.75f + 0.25f * X.COSI(E.af, 17f);
					num4 *= 1f - X.ZSIN(E.af, E.z * 0.35f);
					goto IL_059B;
				case MGATTR.THUNDER:
				{
					int num7 = X.ANM((int)E.af, 3, 7.2f);
					if (num7 == 0)
					{
						num5 = 4293521587U;
						num6 = 2007018766U;
					}
					else if (num7 == 1)
					{
						num5 = 4287361180U;
						num6 = 1998083374U;
					}
					else
					{
						num5 = 4288541680U;
						num6 = 1996575143U;
					}
					num *= 0.7f + 0.3f * X.COSI(E.af, E.z);
					num3 *= 0.75f + 0.25f * X.COSI(E.af, 17f);
					num4 *= 0.75f + 0.25f * X.COSI(E.af + 33f, 21.4f);
					goto IL_059B;
				}
				case MGATTR.POISON:
					num5 = 4283730516U;
					num6 = 4283246368U;
					num3 *= 1f - X.ZSIN(E.af, E.z * 0.35f);
					goto IL_059B;
				default:
					switch (time)
					{
					case MGATTR.ABSORB:
					case MGATTR.ABSORB_V:
						num5 = 4292906685U;
						num6 = 4292505259U;
						num *= 0.7f + 0.3f * X.COSI(E.af, 16f);
						goto IL_059B;
					case MGATTR.WORM:
						num5 = 4288056721U;
						num6 = 4293321187U;
						num *= 0.7f + 0.3f * X.COSI(E.af, 16f);
						goto IL_059B;
					case MGATTR.CURE_HP:
						num5 = 4286709654U;
						num6 = 4290051970U;
						num *= 0.7f + 0.15f * X.COSI(E.af, 55f);
						goto IL_059B;
					case MGATTR.CURE_MP:
						num5 = 4288872428U;
						num6 = 4287364095U;
						num *= 0.7f + 0.15f * X.COSI(E.af, 55f);
						goto IL_059B;
					case MGATTR.MUD:
						num5 = 4290295431U;
						num6 = 4285098058U;
						num4 *= 0.5f;
						goto IL_059B;
					case MGATTR.ACME:
						num5 = 4292378566U;
						num6 = 1150896521U;
						num = X.ZSIN(E.af - E.z * 0.5f, E.z * 0.5f);
						num3 *= (0.75f + 0.25f * X.COSI(E.af, 17f)) * num;
						num4 *= (0.75f + 0.25f * X.COSI(E.af + 33f, 21.4f)) * num;
						goto IL_059B;
					}
					break;
				}
				break;
			}
			num5 = 4294430671U;
			num6 = 4292700007U;
			IL_059B:
			if (num3 > 0f)
			{
				float num8 = 1f;
				if (num3 > 1f)
				{
					num8 = 2f - num3;
					num3 = 1f;
				}
				TransEffecterItem.TE.mulColor(EffectItem.Col1.Set(num5).multiply(C32.d2c(num5), num8, false).blend(num6, X.ANMP((int)E.af, 7, 1f))
					.blend(maxValue, X.Scr(num, 1f - num3)), false);
			}
			if (num4 > 0f || white_scr > 0f)
			{
				float num9 = 0f;
				if (num4 > 1f)
				{
					num9 = num4 - 1f;
					if (fading)
					{
						num9 *= 1f - num;
					}
					num4 = 1f;
				}
				EffectItem.Col1.Set(num5).blend(num6, X.ANMP((int)E.af, 7, 1f)).blend(num2, X.Scr(fading ? X.Scr(num, 1f - num4) : (1f - num4), num9));
				if (white_scr > 0f)
				{
					EffectItem.Col1.multiply(white_scr, false);
				}
				TransEffecterItem.TE.addColor(EffectItem.Col1, true);
			}
			return true;
		}

		public static bool fnRunDraw_dmg_blink(EffectItem E)
		{
			return TransEffecterItem.execute_dmg_blink(E, false, 0f);
		}

		public static bool fnRunDraw_dmg_blink_fading(EffectItem E)
		{
			return TransEffecterItem.execute_dmg_blink(E, true, 0f);
		}

		public static bool fnRunDraw_dmg_blink_enemy(EffectItem E)
		{
			float num = (float)((int)(E.af / 3f) * 3);
			return TransEffecterItem.execute_dmg_blink(E, false, X.saturate(-0.25f + 0.9f * X.COSI(num, 2.83f) + 0.35f * X.COSI(num + 8f, 6.21f)));
		}

		public static bool fnRunDraw_color_blink(EffectItem E)
		{
			if (E.af >= E.y + E.x)
			{
				return false;
			}
			float num = ((E.af < E.x) ? X.ZLINE(E.af, E.x) : (1f - X.ZLINE(E.af - E.x, E.y))) * E.z;
			EffectItem.Col1.Set((uint)(E.time & 16777215));
			TransEffecterItem.TE.mulColor(EffectItem.Col2.White().blend(4278190080U, num), false);
			TransEffecterItem.TE.addColor(EffectItem.Col1.blend(0U, 1f - num), false);
			return true;
		}

		public static bool fnRunDraw_gas_color_blink(EffectItem E)
		{
			if (E.af >= E.y + E.x)
			{
				return false;
			}
			float num = ((E.af < E.x) ? X.ZSIN(E.af, E.x * 0.75f) : (1f - X.ZLINE(E.af - E.x, E.y))) * E.z;
			EffectItem.Col1.Set((uint)(E.time & 16777215));
			TransEffecterItem.TE.mulColor(EffectItem.Col2.White().blend(4278190080U, num), false);
			TransEffecterItem.TE.addColor(EffectItem.Col1.blend(0U, 1f - num), false);
			return true;
		}

		public static bool fnRunDraw_color_blink_add(EffectItem E)
		{
			if (E.af >= E.y + E.x)
			{
				return false;
			}
			float num = ((E.af < E.x) ? X.ZLINE(E.af, E.x) : (1f - X.ZLINE(E.af - E.x, E.y)));
			EffectItem.Col1.Set((uint)(E.time & 16777215)).multiply(E.z, true);
			TransEffecterItem.TE.addColor(EffectItem.Col1.blend(0U, 1f - num), false);
			return true;
		}

		public static bool fnRunDraw_color_blink_add_fadeout(EffectItem E)
		{
			if (E.time == 0)
			{
				return false;
			}
			if (E.z >= 0f && E.af >= E.z)
			{
				return false;
			}
			float num = E.af % E.x;
			float num2 = (1f - X.ZSINV(num, E.x)) * X.ZLINE(E.z - E.af, E.z * 0.25f) * E.y;
			TransEffecterItem.TE.addColor(EffectItem.Col1.Black().blend((uint)(E.time | -16777216), num2), false);
			return true;
		}

		public static bool fnRunDraw_color_blink_bush(EffectItem E)
		{
			if (E.af >= E.y + E.x)
			{
				return false;
			}
			float num = ((E.af < E.x) ? 1f : (0.25f * (1f - X.ZLINE(E.af - E.x, E.y))));
			EffectItem.Col1.Set((uint)(E.time & 16777215)).multiply(E.z, true);
			TransEffecterItem.TE.addColor(EffectItem.Col1.blend(0U, 1f - num), false);
			return true;
		}

		public static bool fnRunDraw_ui_dmg_darken(EffectItem E)
		{
			if (E.z == 0f)
			{
				return false;
			}
			if (E.af >= E.z)
			{
				return false;
			}
			float num = X.ZLINE(E.af, E.z);
			float num2 = E.y;
			num = X.ZSIN(num, 0.1f) - X.ZSINV(num - 0.1f, 0.1f) + X.ZSIN(num - 0.1f, 0.3f) * 0.7f + X.ZPOW(num - 0.2f, 0.8f) * 0.3f;
			MGATTR time = (MGATTR)E.time;
			uint num3;
			switch (time)
			{
			case MGATTR.WIP:
			case MGATTR.BITE:
			case MGATTR.BITE_V:
				num3 = 4294901760U;
				goto IL_0245;
			case MGATTR.GRAB:
				num3 = 4285098089U;
				goto IL_0245;
			case MGATTR.CUT_H:
				num3 = 4294927979U;
				goto IL_0245;
			case MGATTR.STAB:
			case MGATTR.PRESS:
			case MGATTR.PRESS_PENETRATE:
				goto IL_023F;
			case MGATTR.BOMB:
				break;
			default:
				switch (time)
				{
				case MGATTR.FIRE:
					break;
				case MGATTR.ICE:
					num3 = EffectItem.Col1.Set(4287299579U).blend(1426076376U, X.ZSIN(num)).rgba;
					goto IL_0245;
				case MGATTR.THUNDER:
				{
					int num4 = X.ANM((int)E.af, 3, 7.2f);
					if (num4 == 0)
					{
						num3 = 4293521587U;
						goto IL_0245;
					}
					if (num4 == 1)
					{
						num3 = 4287361180U;
						goto IL_0245;
					}
					num3 = 4288541680U;
					goto IL_0245;
				}
				case MGATTR.POISON:
					num3 = 4290267271U;
					goto IL_0245;
				default:
					switch (time)
					{
					case MGATTR.ABSORB:
					case MGATTR.ABSORB_V:
						num3 = 4292906685U;
						num *= 0.7f + 0.3f * X.COSI(E.af, 16f);
						goto IL_0245;
					case MGATTR.WORM:
						num3 = 4288716960U;
						num *= 0.5f + 0.2f * X.COSI(E.af, 16f);
						goto IL_0245;
					case MGATTR.EATEN:
					case MGATTR.CURE_HP:
					case MGATTR.CURE_MP:
						goto IL_023F;
					case MGATTR.MUD:
						num3 = 4285098058U;
						goto IL_0245;
					case MGATTR.ACME:
						num3 = 4294939072U;
						num *= 0.8f + 0.2f * X.COSI(E.af, 16f);
						goto IL_0245;
					default:
						goto IL_023F;
					}
					break;
				}
				break;
			}
			num3 = EffectItem.Col1.Set(4294931507U).blend(4282778628U, X.ZSIN(num)).rgba;
			goto IL_0245;
			IL_023F:
			num3 = 4294288805U;
			IL_0245:
			float num5 = 1f;
			if (num2 > 1f)
			{
				num5 = 2f - num2;
				num2 = 1f;
			}
			TransEffecterItem.TE.mulColor(EffectItem.Col1.Set(num3).multiply(C32.d2c(num3), num5, false).blend(MTRX.ColWhite, X.Scr(1f - num2 * (1f - num), 1f - (float)E.EF.getParticleCount(E, 128) / 128f)), false);
			return true;
		}

		public static bool fnRunDraw_appear_from(EffectItem E)
		{
			if (E.time == 0)
			{
				return false;
			}
			if (E.time >= 0 && E.af >= (float)E.time)
			{
				return false;
			}
			float num = (1f - X.ZSIN(E.af, (float)X.Abs(E.time))) * E.y;
			float num2 = (float)CAim._XD((int)E.x, 1) * num;
			float num3 = (float)CAim._YD((int)E.x, 1) * num;
			TransEffecterItem.TE.translatePos(num2, num3);
			return true;
		}

		public static bool fnRunDraw_disappear_pow_to(EffectItem E)
		{
			if (E.time == 0)
			{
				return false;
			}
			if (E.time >= 0 && E.af >= (float)E.time)
			{
				return false;
			}
			float num = X.ZPOW3(E.af, (float)X.Abs(E.time)) * E.y;
			float num2 = (float)CAim._XD((int)E.x, 1) * num;
			float num3 = (float)CAim._YD((int)E.x, 1) * num;
			TransEffecterItem.TE.translatePos(num2, num3);
			return true;
		}

		public static bool fnRunDraw_evade_blink(EffectItem E)
		{
			if (E.time == 0)
			{
				return false;
			}
			if (E.time > 0 && E.af >= (float)E.time)
			{
				return false;
			}
			TransEffecterItem.TE.mulColor(EffectItem.Col1.Set((X.ANM((int)E.af, 2, 3f) == 0) ? 4290493371U : uint.MaxValue), false);
			return true;
		}

		public static bool fnRunDraw_bounce_zoom_in(EffectItem E)
		{
			if (E.z <= 0f)
			{
				return false;
			}
			if (E.af < E.z)
			{
				TransEffecterItem.TE.scaleMul(1f + (E.x - 1f) * (1f - X.ZPOW(E.z - E.af, E.z)));
			}
			else
			{
				float num = E.af - E.z;
				if (num >= E.y)
				{
					return false;
				}
				TransEffecterItem.TE.scaleMul(1f + (E.x - 1f) * (1f - X.ZPOW(num, E.y)));
			}
			return true;
		}

		public static bool fnRunDraw_bounce_zoom_in2(EffectItem E)
		{
			if (E.z <= 0f)
			{
				return false;
			}
			if (E.af < E.z)
			{
				TransEffecterItem.TE.scaleMul(1f + (E.x - 1f) * X.ZLINE(E.af, E.z));
			}
			else
			{
				float num = E.af - E.z;
				if (num >= E.y)
				{
					return false;
				}
				TransEffecterItem.TE.scaleMul(1f + (E.x - 1f) * (1f - X.ZSIN2(num, E.y)));
			}
			return true;
		}

		public static bool fnRunDraw_bounce_zoom_in_fix(EffectItem E)
		{
			if (E.z <= 0f)
			{
				return false;
			}
			if (E.af < E.z)
			{
				TransEffecterItem.TE.scaleMul(1f + (E.x - 1f + 0.5f) * (1f - X.ZPOW(E.z - E.af, E.z)));
			}
			else
			{
				float num = E.af - E.z;
				if (num < E.y)
				{
					TransEffecterItem.TE.scaleMul(X.NI(E.x + 0.5f, E.x, X.ZPOW(num, E.y)));
				}
			}
			return true;
		}

		public static bool fnRunDraw_enlarge_bouncy(EffectItem E)
		{
			if (E.z <= 0f)
			{
				return false;
			}
			if (E.af < E.z)
			{
				float num = X.ZLINE(E.af, E.z);
				float num2 = E.x - 1f;
				float num3 = E.y - 1f;
				TransEffecterItem.TE.scaleMul(0.9f - 0.1f * X.ZSIN(num, 0.24f) + (0.2f + num2) * X.ZSIN(num - 0.24f, 0.4f) - num2 * X.ZCOS(num - 0.62f, 0.38f), 0.85f + (0.15f + num3) * X.ZSIN2(num, 0.3f) - (num3 + 0.1f) * X.ZSIN(num - 0.28f, 0.27f) + 0.1f * X.ZSINV(num - 0.5f, 0.5f));
				return true;
			}
			return false;
		}

		public static bool fnRunDraw_absorb_bouncy(EffectItem E)
		{
			if (E.z <= 0f)
			{
				return false;
			}
			if (E.af < E.z)
			{
				float num = X.ZLINE(E.af, E.z);
				float num2 = E.x - 1f;
				float num3 = E.y - 1f;
				TransEffecterItem.TE.scaleMul(1f - num2 * 0.5f * X.ZSIN(num, 0.24f) + num2 * 1.5f * X.ZSIN(num - 0.24f, 0.4f) - num2 * X.ZCOS(num - 0.62f, 0.38f), 1f - num3 * 0.5f * X.ZSIN2(num, 0.43f) + num3 * 1.5f * X.ZSIN(num - 0.48f, 0.57f) - num3 * X.ZSINV(num - 0.5f, 0.5f));
				return true;
			}
			return false;
		}

		public static bool fnRunDraw_enlarge_transform(EffectItem E)
		{
			if (E.z <= 0f)
			{
				return false;
			}
			float num = E.x - 1f;
			float num2 = E.y - 1f;
			if (E.af < E.z)
			{
				float num3 = X.ZSINV(E.af, E.z);
				TransEffecterItem.TE.scaleMul(1f + num * num3, 1f + num2 * num3);
			}
			else
			{
				float num4 = 1f - X.ZSIN(E.af - E.z, (float)E.time);
				if (num4 <= 0f)
				{
					return false;
				}
				TransEffecterItem.TE.scaleMul(1f + num * num4, 1f + num2 * num4);
			}
			return true;
		}

		public static bool fnRunDraw_initcarry_bounce(EffectItem E)
		{
			if (E.time <= 0)
			{
				return false;
			}
			if (E.af < (float)E.time)
			{
				float num = X.ZSIN(E.af, (float)E.time);
				TransEffecterItem.TE.translatePos(0f, -X.Sin(num * 3.1415927f) * E.y);
				return true;
			}
			return false;
		}

		public static TransEffecter TE;

		public TEKIND kind = TEKIND._DEFAULT;

		private static BDic<TEKIND, string> OKindTitle = new BDic<TEKIND, string>(32);
	}
}
