using System;
using m2d;
using XX;

namespace nel
{
	public static class DIFF
	{
		public static float shotgun_gsaver_mpback_ratio
		{
			get
			{
				return DIFF.V(0.55f, 0.25f, 0f);
			}
		}

		public static float V(float v_eas, float v_nor)
		{
			if (DIFF.I != 0)
			{
				return v_nor;
			}
			return v_eas;
		}

		public static float V(float v_eas, float v_nor, float v_har)
		{
			if (DIFF.I == 0)
			{
				return v_eas;
			}
			if (DIFF.I != 1)
			{
				return v_har;
			}
			return v_nor;
		}

		public static float recover_holded_mp_gsave(PR Pr, AttackInfo Atk)
		{
			float num;
			if (DIFF.I == 0)
			{
				num = (DIFF.isMapDmg(Atk) ? 1f : 0.75f);
			}
			else if (DIFF.I == 1)
			{
				num = 0.4f;
			}
			else
			{
				num = 0f;
			}
			return X.saturate(num + Pr.getRE(RecipeManager.RPI_EFFECT.LOST_MP_WHEN_CHANTING) * DIFF.V(0.5f, 0.3f, 0.05f));
		}

		public static float recover_mp_gsave(PR Pr, AttackInfo Atk)
		{
			float num;
			if (DIFF.I == 0)
			{
				num = (DIFF.isMapDmg(Atk) ? 1f : 0.75f);
			}
			else if (DIFF.I == 1)
			{
				num = 0.3f;
			}
			else
			{
				num = 0.2f;
			}
			return num;
		}

		public static int getPrDamageVal(int val, NelAttackInfoBase Atk, PR Pr)
		{
			if (DIFF.I == 0)
			{
				val = (int)X.Mx(1f, (float)val * 0.8f * (Pr.isAbsorbState() ? 0.9f : 1f) * (DIFF.isMapDmg(Atk) ? 0.7f : 1f));
			}
			return val;
		}

		public static float hp1_secure_damage_ratio
		{
			get
			{
				return DIFF.V(0.5f, 1.25f, 2f);
			}
		}

		public static float recover_hp_gsave(AttackInfo Atk)
		{
			if (DIFF.I >= 2)
			{
				return 0.2f;
			}
			bool flag = DIFF.isMapDmg(Atk);
			if (DIFF.I == 1)
			{
				if (!flag)
				{
					return 0.45f;
				}
				return 0.8f;
			}
			else
			{
				if (!flag)
				{
					return 0.75f;
				}
				return 0.95f;
			}
		}

		public static bool isMapDmg(AttackInfo Atk)
		{
			return Atk == null || M2NoDamageManager.isMapDamageKey(Atk.ndmg) || Atk.attr == MGATTR.WORM;
		}

		public static bool reduceApplyingHp(PR Pr, AttackInfo Atk, ref float t_hdr, ref int dmgval, out float extend_lock_t, out float extend_lock_t_max, out float damage_turnbak_to_gshp)
		{
			extend_lock_t = (extend_lock_t_max = 0f);
			damage_turnbak_to_gshp = 1f;
			if (DIFF.I >= 2)
			{
				return false;
			}
			bool flag = DIFF.isMapDmg(Atk);
			bool flag2 = false;
			if (t_hdr > 0f || (DIFF.I == 0 && Pr.isSleepingDownState()))
			{
				flag2 = true;
				extend_lock_t = DIFF.V(90f, 160f);
				extend_lock_t_max = DIFF.V(120f, 220f);
				damage_turnbak_to_gshp = (flag ? DIFF.V(0.07f, 0.4f) : (Pr.isGSHpDamageSlowDown() ? DIFF.V(0.3f, 0.6f) : DIFF.V(0.58f, 1f)));
				dmgval = X.Mx(1, (int)((float)dmgval * DIFF.V(0.4f, 0.66f)));
			}
			float num = (float)(flag ? 2 : 1);
			t_hdr = X.Mn(t_hdr + num * DIFF.V(50f, 30f), X.Mx(t_hdr, num * DIFF.V(85f, 35f)));
			return flag2;
		}

		public static float lock_hp_gsaver_time(PR Pr, AttackInfo Atk, int hp = -1000)
		{
			if (hp == -1000)
			{
				hp = (int)Pr.get_hp();
			}
			float num = (((float)hp < Pr.get_maxhp() * 0.2f) ? DIFF.V(0.66f, 0.7f, 1f) : 1f);
			return (float)X.IntC(DIFF.V(200f, 280f, 320f) * num);
		}

		public static float lock_mp_gsaver_time(PR Pr, AttackInfo Atk)
		{
			float num = DIFF.V(200f, 300f, 390f);
			if (Atk == null)
			{
				num *= 0.75f;
			}
			return (float)X.IntC(num);
		}

		public static bool mana_mp_add_gsaver
		{
			get
			{
				return DIFF.I == 0;
			}
		}

		public static float lostMpChanting_screen_value(PR Pr, NelAttackInfoBase Atk)
		{
			float num = DIFF.V(0.4f, 0.25f, 0f);
			if (Pr.GSaver.hp_damage_reduce_active)
			{
				num += DIFF.V(0.2f, 0.125f);
			}
			return num;
		}

		public static float TS_hp_frame_first(PR Pr)
		{
			float num = 1f;
			if (Pr.Ser.has(SER.HP_REDUCE))
			{
				num *= DIFF.V(1.6f, 1.3f, 0.5f);
			}
			if (DIFF.I < 2 && Pr.canFastCureGSaver())
			{
				num *= DIFF.V(1.66f, 1.33f);
			}
			return num;
		}

		public static float TS_mp_frame_first(PR Pr)
		{
			float num = 1f;
			if (Pr.Ser.has(SER.MP_REDUCE))
			{
				num *= DIFF.V(1.6f, 1.3f, 0.5f);
			}
			if (DIFF.I < 2 && Pr.canFastCureGSaver())
			{
				num *= DIFF.V(1.33f, 1.15f);
			}
			return num;
		}

		public static bool cureHpFromGSaver(PR Pr, ref float gsaver_limit, ref float t_lock)
		{
			int num = (int)Pr.get_hp();
			int num2 = (int)gsaver_limit;
			if (num < num2)
			{
				t_lock += DIFF.V(16f, 30f, 60f);
				return true;
			}
			if (num > num2)
			{
				gsaver_limit += 1f;
				t_lock += 3f;
			}
			else
			{
				int num3 = (int)(Pr.get_maxhp() * DIFF.V(0.5f, 0.25f, 0f));
				if (num2 < num3)
				{
					gsaver_limit += 1f;
					if (Pr.Ser.has(SER.HP_REDUCE))
					{
						t_lock += DIFF.V(24f, 58f);
					}
					else
					{
						t_lock += DIFF.V(36f, 80f);
					}
					return true;
				}
				t_lock = -60f;
			}
			return false;
		}

		public static bool cureMpFromGSaver(PR Pr, ref float gsaver_limit, ref float t_lock)
		{
			int num = (int)Pr.get_mp();
			int num2 = (int)gsaver_limit;
			if (num < num2)
			{
				t_lock += DIFF.V(22f, 35f, 90f);
				return true;
			}
			if (num > num2)
			{
				gsaver_limit += 1f;
				t_lock += 3f;
			}
			else
			{
				int num3 = (int)DIFF.V(Pr.get_maxmp() * 0.5f, Pr.get_maxmp() * 0.15f + 1f, 0f);
				if (num2 < num3)
				{
					gsaver_limit += 1f;
					if (Pr.Ser.has(SER.MP_REDUCE))
					{
						t_lock += DIFF.V(40f, 50f);
					}
					else
					{
						t_lock += DIFF.V(45f, 80f);
					}
					return true;
				}
				t_lock = -60f;
			}
			return false;
		}

		public static int mp_egg_lock_adding(PR Pr, float pre_total)
		{
			float num = Pr.GSaver.GsMp.saved_gauge_value - pre_total;
			if (num <= 0f)
			{
				return 0;
			}
			return (int)(num * DIFF.V(1f, 0.33f));
		}

		public static float player_hit_first_velocity_x(float vx)
		{
			int i = DIFF.I;
			if (i == 0)
			{
				return vx * 1.2f;
			}
			if (i != 1)
			{
				return X.absMn(vx * 2.5f, 0.3f);
			}
			return X.absMn(vx * 1.75f, 0.125f);
		}

		public static bool damage_cliff_stop
		{
			get
			{
				return DIFF.I == 0;
			}
		}

		public static bool alloc_useitem_in_magic
		{
			get
			{
				return DIFF.I == 0;
			}
		}

		public static float spike_burst_vx_ratio(M2Attackable MvA)
		{
			if (!MvA.hasFoot())
			{
				return DIFF.V(0.25f, 0.55f, 1f);
			}
			return DIFF.V(0.25f, 1f, 2f);
		}

		public static bool gacha_failure_decline
		{
			get
			{
				return DIFF.I == 0;
			}
		}

		public static bool gacha_failure_no_reset
		{
			get
			{
				return DIFF.I <= 1;
			}
		}

		public static float gacha_corrupt_level
		{
			get
			{
				return DIFF.V(0.2f, 0.6f, 1f);
			}
		}

		public static float gacha_punch_ser_screen_value
		{
			get
			{
				return DIFF.V(0.5f, 0.2f, 0f);
			}
		}

		public static float gacha_punch_screen_value
		{
			get
			{
				return DIFF.V(0.34f, 0.17f, 0f);
			}
		}

		public static void fixGachaInput(AbsorbManagerContainer Con, PrGachaItem Gacha, ref PrGachaItem.TYPE _type, ref int need_count, ref uint _key_alloc_bit)
		{
			if (DIFF.I == 0)
			{
				_type = PrGachaItem.TYPE.REP;
				_key_alloc_bit &= 48U;
				need_count = X.Mn(need_count, 5);
				return;
			}
			if (DIFF.I == 1)
			{
				int length = Con.Length;
				bool flag = false;
				int num = 0;
				bool flag2 = false;
				for (int i = 0; i < length; i++)
				{
					PrGachaItem gacha = Con.GetManagerItem(i).get_Gacha();
					if (gacha != Gacha && gacha != null && gacha.isUseable())
					{
						flag2 = true;
						if (gacha.type != PrGachaItem.TYPE.REP)
						{
							flag = true;
						}
						if ((gacha.get_key_alloc_bit() & 15U) != 0U)
						{
							num++;
						}
					}
				}
				if (flag2)
				{
					need_count = X.Mn(need_count, 6);
				}
				if (flag)
				{
					_type = PrGachaItem.TYPE.REP;
				}
				if (num >= 2)
				{
					_key_alloc_bit &= 4294967280U;
				}
			}
		}

		public static float box_drop_ratio(int saved_i)
		{
			if (saved_i == 0)
			{
				return 0.5f;
			}
			if (saved_i != 1)
			{
				return 1f;
			}
			return 0.8f;
		}

		public static bool mist_damage_applyable(PR Pr, int count)
		{
			if (DIFF.I >= 2)
			{
				return true;
			}
			int num;
			if (DIFF.I == 1)
			{
				num = 100 - count * 25;
			}
			else
			{
				num = 125 - count * 20;
				if (num < 40)
				{
					num = ((count % 2 == 0) ? 20 : 100);
				}
			}
			return num < 100 && X.xors(100) >= num;
		}

		public static float itembomb_pr_apply_damage_ratio()
		{
			if (DIFF.I == 2)
			{
				return 0.7f;
			}
			if (DIFF.I != 1)
			{
				return 0.2f;
			}
			return 0.5f;
		}

		public static float enemy_twicemana_hp_cure_ratio
		{
			get
			{
				if (DIFF.I == 2)
				{
					return 1f;
				}
				if (DIFF.I != 1)
				{
					return 0f;
				}
				return 0.5f;
			}
		}

		public static float enemy_twicemana_mp_cure_ratio
		{
			get
			{
				if (DIFF.I == 2)
				{
					return 2f;
				}
				if (DIFF.I != 1)
				{
					return 1.25f;
				}
				return 1.66f;
			}
		}

		public static float apply_damage_ratio_to_od_from_pr
		{
			get
			{
				if (DIFF.I != 0)
				{
					return 1f;
				}
				return 1.33f;
			}
		}

		public static float enemy_respawn_timescale_when_exists
		{
			get
			{
				if (DIFF.I == 2)
				{
					return 1f;
				}
				if (DIFF.I != 1)
				{
					return 0.33f;
				}
				return 0.5f;
			}
		}

		public static int orgasm_tap_max_count
		{
			get
			{
				return (int)DIFF.V(10f, 20f, 99f);
			}
		}

		public static int I = 1;

		public const int I_EAS = 0;

		public const int I_NOR = 1;

		public const int I_HAR = 2;

		public const int I__MAX = 3;
	}
}
