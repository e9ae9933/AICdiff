using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public static class EnemyAttr
	{
		private static void initMcs()
		{
			EnemyAttr.McsSplashFire = NOD.getMpConsume("nattr_fire");
			EnemyAttr.McsSplashIce = NOD.getMpConsume("nattr_ice");
			EnemyAttr.McsSplashIceBullet = NOD.getMpConsume("nattr_ice_bullet");
			EnemyAttr.McsSplashThunder = NOD.getMpConsume("nattr_thunder");
			EnemyAttr.McsSplashAcme = NOD.getMpConsume("nattr_acme");
			EnemyAttr.McsSplashSlimy = NOD.getMpConsume("nattr_slimy");
		}

		public static ENATTR attachKind(ENATTR attachable_nattr, float smn_xorsp, int kind_max)
		{
			uint num = (uint)(smn_xorsp * 8944413f);
			int num2 = EnemyAttr.mattrCount(attachable_nattr);
			float num3 = (float)num2 / (float)X.bit_count(7936U);
			ENATTR enattr;
			if (kind_max == 1)
			{
				float num4 = X.Scr(0.51f, 1f - num3);
				float num5 = X.RAN(num, 1401);
				if (num5 < num4)
				{
					num5 /= num4;
					enattr = EnemyAttr.attr012atkdef(num5);
				}
				else
				{
					num5 = (num5 - num4) / (1f - num4);
					enattr = EnemyAttr.attr012mattr(num5, attachable_nattr, num2);
				}
			}
			else if (kind_max == 2)
			{
				X.RAN(num, 739);
				if (smn_xorsp < 0.125f)
				{
					enattr = ENATTR.INVISIBLE;
				}
				else if (X.RAN(num, 2107) <= X.Scr(0.25f, 1f - num3))
				{
					enattr = ENATTR.ATK | ENATTR.DEF;
				}
				else
				{
					enattr = EnemyAttr.attr012atkdef(X.RAN(num, 512)) | EnemyAttr.attr012mattr(X.RAN(num, 2181), attachable_nattr, num2);
				}
			}
			else
			{
				X.RAN(num, 1226);
				if (smn_xorsp < 0.125f)
				{
					enattr = ENATTR.INVISIBLE;
					enattr |= EnemyAttr.attachKind(attachable_nattr, smn_xorsp, 1);
				}
				else if (X.RAN(num, 2792) <= X.Scr(0.25f, 1f - num3))
				{
					enattr = ENATTR._AATTR;
				}
				else
				{
					enattr = EnemyAttr.attr012atkdef2(X.RAN(num, 517)) | EnemyAttr.attr012mattr(X.RAN(num, 2141), attachable_nattr, num2);
				}
			}
			return enattr;
		}

		private static ENATTR attr012atkdef(float v)
		{
			if (v < 0.35f)
			{
				return ENATTR.ATK;
			}
			if (v >= 0.7f)
			{
				return ENATTR.MP_STABLE;
			}
			return ENATTR.DEF;
		}

		private static ENATTR attr012atkdef2(float v)
		{
			if (v < 0.4f)
			{
				return ENATTR.ATK | ENATTR.DEF;
			}
			if (v >= 0.7f)
			{
				return ENATTR._MATTR_COUNT;
			}
			return ENATTR.DEF | ENATTR.MP_STABLE;
		}

		private static ENATTR attr012mattr(float v, ENATTR attachable_nattr, int mattr_c)
		{
			if (mattr_c >= 5)
			{
				if (v < 0.2f)
				{
					return ENATTR.FIRE;
				}
				if (v < 0.4f)
				{
					return ENATTR.ICE;
				}
				if (v < 0.6f)
				{
					return ENATTR.THUNDER;
				}
				if (v >= 0.8f)
				{
					return ENATTR.ACME;
				}
				return ENATTR.SLIMY;
			}
			else
			{
				if (mattr_c <= 0)
				{
					return ENATTR.NORMAL;
				}
				using (BList<int> blist = ListBuffer<int>.Pop(0))
				{
					if ((attachable_nattr & ENATTR.FIRE) != ENATTR.NORMAL)
					{
						blist.Add(256);
					}
					if ((attachable_nattr & ENATTR.ICE) != ENATTR.NORMAL)
					{
						blist.Add(512);
					}
					if ((attachable_nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
					{
						blist.Add(1024);
					}
					if ((attachable_nattr & ENATTR.ACME) != ENATTR.NORMAL)
					{
						blist.Add(4096);
					}
					if ((attachable_nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
					{
						blist.Add(2048);
					}
					v *= (float)blist.Count;
					for (int i = 0; i < blist.Count; i++)
					{
						if (v < (float)i || i == blist.Count - 1)
						{
							return (ENATTR)blist[i];
						}
					}
				}
				return ENATTR.NORMAL;
			}
		}

		public static int attrKindCount(ENATTR v)
		{
			return (((v & ENATTR._AATTR) != ENATTR.NORMAL) ? 1 : 0) + (((v & ENATTR._MATTR) != ENATTR.NORMAL) ? 1 : 0) + (((v & ENATTR.INVISIBLE) != ENATTR.NORMAL) ? 1 : 0);
		}

		public static int mattrCount(ENATTR v)
		{
			return X.bit_count((uint)(v & ENATTR._MATTR));
		}

		public static int mattrIndex(ENATTR v)
		{
			if ((v & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				return 0;
			}
			if ((v & ENATTR.ICE) != ENATTR.NORMAL)
			{
				return 1;
			}
			if ((v & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				return 2;
			}
			if ((v & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				return 3;
			}
			if ((v & ENATTR.ACME) != ENATTR.NORMAL)
			{
				return 4;
			}
			return -1;
		}

		public static int aattrCount(ENATTR v)
		{
			return X.bit_count((uint)(v & ENATTR._AATTR));
		}

		public static string getLocalizedName(ENATTR v)
		{
			if (v <= ENATTR.ICE)
			{
				switch (v)
				{
				case ENATTR.ATK:
					return TX.Get("Enattr_atk", "");
				case ENATTR.DEF:
					return TX.Get("Enattr_def", "");
				case ENATTR.ATK | ENATTR.DEF:
					break;
				case ENATTR.MP_STABLE:
					return TX.Get("Enattr_mp_stable", "");
				default:
					if (v == ENATTR.FIRE)
					{
						return TX.Get("Enattr_fire", "");
					}
					if (v == ENATTR.ICE)
					{
						return TX.Get("Enattr_ice", "");
					}
					break;
				}
			}
			else if (v <= ENATTR.SLIMY)
			{
				if (v == ENATTR.THUNDER)
				{
					return TX.Get("Enattr_thunder", "");
				}
				if (v == ENATTR.SLIMY)
				{
					return TX.Get("Enattr_slimy", "");
				}
			}
			else
			{
				if (v == ENATTR.ACME)
				{
					return TX.Get("Enattr_acme", "");
				}
				if (v == ENATTR.INVISIBLE)
				{
					return TX.Get("Enattr_invisible", "");
				}
			}
			return "";
		}

		public static MGATTR atk_attr(NelEnemy En, MGATTR _default = MGATTR.NORMAL)
		{
			if ((En.nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				return MGATTR.FIRE;
			}
			if ((En.nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				return MGATTR.ICE;
			}
			if ((En.nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				return MGATTR.THUNDER;
			}
			if ((En.nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				return MGATTR.ACME;
			}
			if ((En.nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				return MGATTR.SPERMA;
			}
			return _default;
		}

		public static BetoInfo beto_attr(NelEnemy En, BetoInfo _default = null)
		{
			if ((En.nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				return EnemyAttr.BetoFire;
			}
			if ((En.nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				return EnemyAttr.BetoIce;
			}
			if ((En.nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				return EnemyAttr.BetoThunder;
			}
			if ((En.nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				return EnemyAttr.BetoSlimy;
			}
			return _default;
		}

		public static void appearEnemy(NelEnemy En)
		{
			if (EnemyAttr.McsSplashAcme == null)
			{
				EnemyAttr.initMcs();
			}
			if (En.nattr == ENATTR.NORMAL)
			{
				return;
			}
			if ((En.nattr & ENATTR.ATK) != ENATTR.NORMAL)
			{
				En.addExperienceAtk(En.nM2D.NightCon.summoner_nattr_atk_add_experience(En));
			}
			if ((En.nattr & ENATTR.DEF) != ENATTR.NORMAL)
			{
				En.addExperienceDef(En.nM2D.NightCon.summoner_nattr_def_add_experience(En));
			}
			if ((En.nattr & ENATTR._MATTR) != ENATTR.NORMAL)
			{
				if (!En.nattr_invisible)
				{
					if ((En.nattr & ENATTR.FIRE) != ENATTR.NORMAL)
					{
						En.PtcST("enemy_nattr_fire", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
					}
					if ((En.nattr & ENATTR.ICE) != ENATTR.NORMAL)
					{
						En.PtcST("enemy_nattr_ice", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
					}
					if ((En.nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
					{
						En.PtcST("enemy_nattr_thunder", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
					}
					if ((En.nattr & ENATTR.ACME) != ENATTR.NORMAL)
					{
						En.PtcST("enemy_nattr_acme", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
						if (En.enlarge_publish_damage_ratio > 1f)
						{
							En.enlarge_publish_damage_ratio = (En.enlarge_publish_damage_ratio - 1f) * 0.5f + 1f;
						}
					}
					if ((En.nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
					{
						En.PtcST("enemy_nattr_slimy", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
						M2Phys physic = En.getPhysic();
						if (physic != null)
						{
							physic.ignore_web_reduce = true;
						}
					}
				}
			}
			else
			{
				bool nattr_mp_stable = En.nattr_mp_stable;
			}
			if (En.nattr_invisible)
			{
				En.PtcVar("mp_stable", (double)(En.nattr_mp_stable ? 1 : 0)).PtcST("enemy_nattr_invisible", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
			}
		}

		public static void initAnimator(NelEnemy En, EnemyMeshDrawer Anm)
		{
			Anm.is_evil = En.is_evil;
			if (En.is_evil)
			{
				Anm.white_parlin = En.nattr_mp_stable;
				Anm.nattr_invisible = En.nattr_invisible;
			}
			if ((En.nattr & ENATTR._MATTR) != ENATTR.NORMAL)
			{
				Anm.border_color = EnemyAttr.get_mcolor(En, 4294901760U);
				MTRX.cola.Set(Anm.base_color).blend(Anm.border_color, X.NIXP(0.7f, 0.95f));
				if (En.nattr_mp_stable)
				{
					MTRX.cola.blend(4294176489U, 0.85f);
					Anm.add_color_eye_fade_out = EnemyAttr.get_mcolor_sub(En, 4282952401U);
				}
				else
				{
					Anm.add_color_eye_fade_out = Anm.border_color;
				}
				Anm.base_color = MTRX.cola.rgba;
				return;
			}
			if (En.nattr_mp_stable)
			{
				Anm.base_color = 4294176489U;
				Anm.add_color_eye_fade_out = 4282756539U;
				Anm.border_color = MTRX.colb.Set(Anm.border_color).multiply(C32.d2c(4286450447U), 0.4f, false).rgba;
			}
		}

		public static FlagCounter<SER> atk_ser(NelEnemy En)
		{
			if ((En.nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				return EnemyAttr.SerDmgFire;
			}
			if ((En.nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				return EnemyAttr.SerDmgIce;
			}
			if ((En.nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				return EnemyAttr.SerDmgThunder;
			}
			if ((En.nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				return EnemyAttr.SerDmgAcme;
			}
			if ((En.nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				return EnemyAttr.SerDmgSlimy;
			}
			return null;
		}

		public static FlagCounter<SER> createSer(NelEnemy En, int apply_ratio)
		{
			return EnemyAttr.createSer(En.nattr, apply_ratio, null);
		}

		public static FlagCounter<SER> createSer(ENATTR nattr, int apply_ratio, FlagCounter<SER> Target = null)
		{
			apply_ratio = X.Mn(apply_ratio, 255);
			if ((nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				return (Target ?? new FlagCounter<SER>(1)).Add(SER.BURNED, (float)apply_ratio);
			}
			if ((nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				return (Target ?? new FlagCounter<SER>(1)).Add(SER.FROZEN, (float)apply_ratio);
			}
			if ((nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				return (Target ?? new FlagCounter<SER>(1)).Add(SER.PARALYSIS, (float)apply_ratio);
			}
			if ((nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				return (Target ?? new FlagCounter<SER>(1)).Add(SER.SEXERCISE, (float)X.Mn(255, apply_ratio * 2));
			}
			if ((nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				return (Target ?? new FlagCounter<SER>(1)).Add(SER.WEB_TRAPPED, (float)X.Mn(255, apply_ratio * 2));
			}
			return Target;
		}

		public static uint get_mcolor(NelEnemy En, uint def_color = 4294901760U)
		{
			return EnemyAttr.get_mcolor(En.nattr, def_color);
		}

		public static uint get_mcolor(ENATTR nattr, uint def_color = 4294901760U)
		{
			if ((nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				return 4294929174U;
			}
			if ((nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				return 4279758079U;
			}
			if ((nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				return 4291428119U;
			}
			if ((nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				return 4294907876U;
			}
			if ((nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				return 4292136370U;
			}
			return def_color;
		}

		public static uint get_mcolor2(NelEnemy En, uint def_color = 4286459952U)
		{
			return EnemyAttr.get_mcolor2(En.nattr, def_color);
		}

		public static uint get_mcolor2(ENATTR nattr, uint def_color = 4286459952U)
		{
			if ((nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				return 4289540352U;
			}
			if ((nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				return 4278191789U;
			}
			if ((nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				return 4281561651U;
			}
			if ((nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				return 4287304859U;
			}
			if ((nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				return 4288846959U;
			}
			return def_color;
		}

		public static uint get_mcolor_sub(NelEnemy En, uint def_color = 4282952401U)
		{
			return EnemyAttr.get_mcolor_sub(En.nattr, def_color);
		}

		public static uint get_mcolor_sub(ENATTR nattr, uint def_color = 4282952401U)
		{
			if ((nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				return 4280895974U;
			}
			if ((nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				return 4291931943U;
			}
			if ((nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				return 4291504337U;
			}
			if ((nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				return 4286230372U;
			}
			if ((nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				return 4285361798U;
			}
			return def_color;
		}

		public static float getSinkRatio(NelEnemy En)
		{
			float num = En.sink_ratio;
			if ((En.nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				num *= 2f;
			}
			return num;
		}

		public static EnAttackInfo PrepareAtk(NelEnemy En, EnAttackInfo Atk, bool overwrite_attr = true)
		{
			Atk.fnHitEffectAfter = EnemyAttr.NattrHitAfter;
			if ((En.nattr & ENATTR._MATTR) == ENATTR.NORMAL)
			{
				return Atk;
			}
			MGATTR mgattr = EnemyAttr.atk_attr(En, MGATTR.NORMAL);
			if (mgattr == Atk.attr)
			{
				if (Atk.hpdmg0 > 0)
				{
					Atk.hpdmg0 += X.Mn(15, Atk.hpdmg0);
				}
				else if (Atk.mpdmg0 > 0)
				{
					Atk.mpdmg0 += 15;
				}
				else if (Atk.split_mpdmg > 0)
				{
					Atk.split_mpdmg += 15;
				}
			}
			else
			{
				if (overwrite_attr)
				{
					Atk.attr = mgattr;
				}
				if (En.nattr_mp_stable)
				{
					Atk.split_mpdmg += 10;
				}
			}
			if ((En.nattr & ENATTR.ACME) != ENATTR.NORMAL && Atk.hpdmg0 >= 2)
			{
				int num = X.Mx(X.IntC((float)Atk.hpdmg0 * 0.5f), 1);
				int num2 = Atk.hpdmg0 - num;
				Atk.mpdmg0 += num2 + 6;
				Atk.hpdmg0 = num;
			}
			if ((En.nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				if (Atk.hpdmg0 >= 2)
				{
					Atk.hpdmg0 = X.Mx(X.IntC((float)Atk.hpdmg0 * 0.33f), 1);
				}
				if (Atk.mpdmg0 >= 2)
				{
					Atk.mpdmg0 = X.Mx(X.IntC((float)Atk.mpdmg0 * 0.66f), 1);
				}
			}
			if (Atk.EpDmg != null)
			{
				EnemyAttr.PrepareAtk(En, Atk.EpDmg);
			}
			return Atk;
		}

		public static EpAtk PrepareAtk(NelEnemy En, EpAtk Atk)
		{
			if ((En.nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				Atk.val = X.IntC((float)Atk.val * 1.5f);
				Atk.multiple_orgasm += 1f;
			}
			return Atk;
		}

		public static float applyHpDamageRatio(NelEnemy En, AttackInfo Atk)
		{
			if (Atk == null)
			{
				return 1f;
			}
			return EnemyAttr.applyHpDamageRatio(En, Atk.attr);
		}

		public static float applyHpDamageRatio(NelEnemy En, MGATTR attr)
		{
			float num = (En.nattr_mp_stable ? (En.isOverDrive() ? 0.9f : 0.66f) : 1f);
			if ((En.nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				num *= ((attr == MGATTR.ICE) ? 1.5f : ((attr == MGATTR.FIRE) ? 0f : ((attr == MGATTR.THUNDER) ? 0.5f : 1f)));
			}
			else if ((En.nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				num *= ((attr == MGATTR.FIRE) ? 1.5f : ((attr == MGATTR.ICE) ? 0f : ((attr == MGATTR.THUNDER) ? 0.5f : 1f)));
			}
			else if ((En.nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				num *= ((attr == MGATTR.THUNDER) ? 0f : ((attr == MGATTR.ICE || attr == MGATTR.FIRE) ? 0.5f : 1f));
			}
			else if ((En.nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				num *= ((attr == MGATTR.SPERMA) ? 0f : ((attr == MGATTR.FIRE) ? 1.5f : 1f));
			}
			else if ((En.nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				num *= ((attr == MGATTR.ACME) ? 0f : ((attr == MGATTR.FIRE || attr == MGATTR.ICE || attr == MGATTR.THUNDER) ? 1.5f : 1f));
			}
			return num;
		}

		public static void quitTicket(NelEnemy En, NaTicket Tk)
		{
			if ((En.nattr & ENATTR.MP_STABLE) != ENATTR.NORMAL)
			{
				En.getAI().delay += 70f * En.nattr_delay_extend_ratio;
				return;
			}
			if ((En.nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				En.getAI().delay += 30f * En.nattr_delay_extend_ratio;
			}
		}

		public static void Splash(NelEnemy En, float radius = 1f)
		{
			EnemyAttr.Splash(En, En.x, En.y + En.sizey - 0.15f, radius, 1f, 1f);
		}

		public static void Splash(NelEnemy En, float x, float y, float radius, float consume_ratio = 1f, float stain_maxt_ratio = 1f)
		{
			EnemyAttr.Splash(En, En.nattr, x, y, radius, consume_ratio, stain_maxt_ratio);
		}

		public static void Splash(NelEnemy En, ENATTR nattr, float x, float y, float radius, float consume_ratio = 1f, float stain_maxt_ratio = 1f)
		{
			if (!En.is_alive || (nattr & ENATTR._MATTR) == ENATTR.NORMAL)
			{
				return;
			}
			MagicItem magicItem = null;
			NOD.MpConsume mpConsume = null;
			float num = (En.isOverDrive() ? 2f : 1f) * stain_maxt_ratio;
			if ((nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				if (CCON.isWater(En.Mp.getConfig((int)x, (int)y)))
				{
					return;
				}
				En.PtcVar("radius", (double)radius).PtcVar("cx", (double)x).PtcVar("cy", (double)y);
				En.PtcST("enemy_nattr_splash_fire", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				if (En.Useable(EnemyAttr.McsSplashFire, consume_ratio, 0f))
				{
					magicItem = En.nM2D.MGC.setMagic(En, MGKIND.NATTR_SPLASH_FIRE, (MGHIT)1027);
					int num2 = X.Mx(1, X.IntC(X.NIL(1f, 0.5f, radius, 2f) * radius));
					if (num2 < 3 && X.XORSP() < 0.3f)
					{
						num2++;
					}
					int num3 = 0;
					while (num3 < num2 && En.Useable(EnemyAttr.McsSplashFire, consume_ratio, 0f))
					{
						float num4 = X.NIXP(0.08f, 0.12f);
						float num5 = 1.5707964f + X.XORSPS() * 3.1415927f * 0.45f;
						MagicItem magicItem2 = MgNCandleShot.addCandleShot(En.nM2D, En, En.mg_hit, x, y, X.Cos(num5) * num4, -X.Sin(num5) * num4 - 0.04f, X.NIXP(160f, 300f) * num, 15f);
						En.MpConsume(EnemyAttr.McsSplashFire, magicItem2, consume_ratio, 1f);
						num3++;
					}
				}
			}
			if ((nattr & ENATTR.THUNDER) != ENATTR.NORMAL && En.Useable(EnemyAttr.McsSplashThunder, consume_ratio, 0f))
			{
				magicItem = En.nM2D.MGC.setMagic(En, MGKIND.NATTR_SPLASH_THUNDER, (MGHIT)1027);
				mpConsume = EnemyAttr.McsSplashThunder;
				if (X.LENGTHXYS(En.x, En.y, x, y) < 0.025f)
				{
					magicItem.da = 1f;
				}
			}
			if ((nattr & ENATTR.ICE) != ENATTR.NORMAL && En.Useable(EnemyAttr.McsSplashIce, consume_ratio, 0f))
			{
				magicItem = En.nM2D.MGC.setMagic(En, MGKIND.NATTR_SPLASH_ICE, (MGHIT)1027);
				mpConsume = EnemyAttr.McsSplashIce;
				magicItem.dz = num;
			}
			if ((nattr & ENATTR.ACME) != ENATTR.NORMAL && En.Useable(EnemyAttr.McsSplashAcme, consume_ratio, 0f))
			{
				En.PtcVar("cx", (double)x).PtcVar("cy", (double)y).PtcST("enemy_attr_splash_acme", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				En.nM2D.MIST.addMistGenerator(NelNMush.MkAcme, NelNMush.MkAcme.calcAmount(X.IntR(200f * num * X.Mx(0.5f, radius)), 1.4f), (int)x, (int)y, false);
				En.MpConsume(x, y, EnemyAttr.McsSplashAcme, null, consume_ratio, 1f);
			}
			if ((nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				int num6 = X.Mx(1, X.IntC(radius * 1.5f + 0.3f));
				int num7 = 0;
				while (num7 < num6 && En.Useable(EnemyAttr.McsSplashSlimy, consume_ratio, 0f))
				{
					float num8 = X.NIXP(0.08f, 0.12f);
					float num9 = 1.5707964f + X.XORSPS() * 3.1415927f * 0.45f;
					MagicItem magicItem3 = MgNWebShot.addWebShot(En.nM2D, En, En.mg_hit, x, y, X.Cos(num9) * num8, -X.Sin(num9) * num8 - 0.04f, X.NIXP(140f, 220f) * num, 8f);
					En.MpConsume(EnemyAttr.McsSplashSlimy, magicItem3, consume_ratio, 1f);
					num7++;
				}
			}
			if (magicItem != null)
			{
				magicItem.sx = x;
				magicItem.sy = y;
				magicItem.dy = y;
				magicItem.sz = radius;
				if (mpConsume != null)
				{
					En.MpConsume(mpConsume, magicItem, consume_ratio, 1f);
				}
			}
		}

		public static void SplashS(NelEnemy En, float agR = -1000f)
		{
			EnemyAttr.SplashS(En, En.x, En.y + En.sizey + 0.15f, agR, 1f, 1f);
		}

		public static void SplashS(NelEnemy En, float x, float y, float agR, float consume_ratio = 1f, float stain_maxt_ratio = 1f)
		{
			EnemyAttr.SplashS(En, En.nattr, x, y, agR, consume_ratio, stain_maxt_ratio);
		}

		public static void SplashS(NelEnemy En, ENATTR nattr, float x, float y, float agR, float consume_ratio = 1f, float stain_maxt_ratio = 1f)
		{
			if (!En.is_alive || (nattr & ENATTR._MATTR) == ENATTR.NORMAL)
			{
				return;
			}
			float num = (En.isOverDrive() ? 2f : 1f) * stain_maxt_ratio;
			if (agR == -1000f)
			{
				if (X.LENGTHXYS(En.x, En.y, x, y) < 0.02f && En.hasFoot())
				{
					M2BlockColliderContainer.BCCLine footBCC = En.getFootManager().get_FootBCC();
					if (footBCC != null)
					{
						agR = footBCC.housenagR + X.XORSPS() * 0.3f;
					}
					else
					{
						agR = 3.1415927f * X.XORSPS();
					}
				}
				else
				{
					agR = 3.1415927f * X.XORSPS();
				}
			}
			if ((En.nattr & ENATTR.FIRE) != ENATTR.NORMAL && En.Useable(EnemyAttr.McsSplashFire, consume_ratio, 0f))
			{
				float num2 = X.NIXP(0.08f, 0.12f);
				MagicItem magicItem = MgNCandleShot.addCandleShot(En.nM2D, En, En.mg_hit, x, y, X.Cos(agR) * num2, -X.Sin(agR) * num2 - 0.04f, X.NIXP(160f, 300f) * num, 1f);
				En.MpConsume(EnemyAttr.McsSplashFire, magicItem, consume_ratio, 1f);
			}
			if ((En.nattr & ENATTR.ICE) != ENATTR.NORMAL && En.Useable(EnemyAttr.McsSplashIceBullet, consume_ratio, 0f))
			{
				MagicItem magicItem = MgNIceShot.addIceShot2(En.nM2D, En, En.mg_hit, x, y, 0.12f, agR, 0f, 0f);
				En.MpConsume(EnemyAttr.McsSplashIceBullet, magicItem, consume_ratio, 1f);
			}
			if ((En.nattr & ENATTR.THUNDER) != ENATTR.NORMAL && En.Useable(EnemyAttr.McsSplashIceBullet, consume_ratio, 0f))
			{
				MagicItem magicItem = MgNThunderBallShot.addThunderBallShot(En.nM2D, En, En.mg_hit, x, y, agR);
				En.MpConsume(EnemyAttr.McsSplashIceBullet, magicItem, consume_ratio, 1f);
			}
			if ((nattr & ENATTR.ACME) != ENATTR.NORMAL && En.Useable(EnemyAttr.McsSplashAcme, consume_ratio * 0.8f, 0f))
			{
				En.PtcVar("cx", (double)x).PtcVar("cy", (double)y).PtcST("enemy_attr_splash_acme", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				En.nM2D.MIST.addMistGenerator(NelNMush.MkAcmeS, NelNMush.MkAcme.calcAmount(X.IntR(170f * num), 1.4f), (int)x, (int)y, false);
				En.MpConsume(x, y, EnemyAttr.McsSplashAcme, null, consume_ratio * 0.8f, 1f);
			}
			if ((nattr & ENATTR.SLIMY) != ENATTR.NORMAL && En.Useable(EnemyAttr.McsSplashSlimy, consume_ratio, 0f))
			{
				float num3 = X.NIXP(0.08f, 0.12f);
				MagicItem magicItem = MgNWebShot.addWebShot(En.nM2D, En, En.mg_hit, x, y, X.Cos(agR) * num3, -X.Sin(agR) * num3, X.NIXP(140f, 220f) * num, 8f);
				En.MpConsume(EnemyAttr.McsSplashSlimy, magicItem, consume_ratio, 1f);
			}
		}

		public static void SplashSOnAir(NelEnemy En, float x, float y, float len_map, float agR, float xdf = 0f, float ydf = 0f, float consume_ratio = 1f, int shot_count = 1, float stain_maxt_ratio = 1f)
		{
			float num = X.Cos(agR);
			float num2 = X.Sin(agR);
			int num3 = X.Mx(1, X.IntC(len_map * 5f));
			float num4 = len_map * (1f / (float)num3);
			int num5 = X.xors(num3);
			for (int i = 0; i < shot_count; i++)
			{
				int j = 0;
				while (j < num3)
				{
					float num6 = (0.5f + (float)num5++) * num4;
					float num7 = x + num * num6;
					float num8 = y - num2 * num6;
					if (En.Mp.canStand((int)num7, (int)num8))
					{
						EnemyAttr.SplashS(En, num7 + X.XORSP() * xdf, num8 + X.XORSP() * ydf, agR + X.XORSPS() * 3.1415927f * 0.4f, consume_ratio, stain_maxt_ratio);
						if (shot_count > 1)
						{
							num5 = X.IntR((float)num5 + (float)num3 * X.NIXP(1f, 0.88f) / (float)shot_count) % num3;
							break;
						}
						break;
					}
					else
					{
						if (num5 >= num3)
						{
							num5 = 0;
						}
						j++;
					}
				}
			}
		}

		public static void HitAfter(NelEnemy En, IM2RayHitAble Target)
		{
			if (En != null && En.is_alive)
			{
				FlagCounter<SER> flagCounter = EnemyAttr.atk_ser(En);
				if (flagCounter != null)
				{
					if (Target is PR)
					{
						PR pr = Target as PR;
						float num = 1f;
						if ((En.nattr & ENATTR.FIRE) != ENATTR.NORMAL)
						{
							pr.BetoMng.Check(EnemyAttr.BetoFire, false, true);
						}
						if ((En.nattr & ENATTR.ICE) != ENATTR.NORMAL)
						{
							pr.BetoMng.Check(EnemyAttr.BetoIce, false, true);
						}
						if ((En.nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
						{
							pr.BetoMng.Check(EnemyAttr.BetoThunder, false, true);
							num *= 0.7f;
						}
						if (En.isAbsorbState())
						{
							num *= En.absorb_nattr_ser_apply;
						}
						pr.Ser.applySerDamage(flagCounter, num, -1);
					}
					if (Target is NelEnemy)
					{
						(Target as NelEnemy).applySerDamage(flagCounter, 0.5f, -1);
					}
				}
			}
		}

		public static void absorbDamagePrepare(NelEnemy En, PR Target, NelAttackInfo bAtk, bool nattr_replace_attr = true)
		{
			if ((En.nattr & ENATTR._MATTR) == ENATTR.NORMAL)
			{
				return;
			}
			if (X.XORSP() < (En.isOverDrive() ? 1f : 0.4f))
			{
				FlagCounter<SER> flagCounter = EnemyAttr.atk_ser(En);
				if (flagCounter != null)
				{
					Target.Ser.applySerDamage(flagCounter, 0.3f, -1);
				}
			}
			if ((En.nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				bAtk.hpdmg_current += 14;
			}
			if ((En.nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				bAtk.hpdmg_current += 4;
			}
			if ((En.nattr & ENATTR.THUNDER) != ENATTR.NORMAL)
			{
				bAtk.hpdmg_current += 12;
				bAtk.mpdmg_current += 5;
			}
			if ((En.nattr & ENATTR.ACME) != ENATTR.NORMAL)
			{
				bAtk.mpdmg_current += 10;
			}
			if (nattr_replace_attr)
			{
				bAtk.attr = EnemyAttr.atk_attr(En, bAtk.attr);
			}
		}

		public static bool fnRunSplashFire(MagicItem Mg, float fcnt)
		{
			if (Mg.sz == 0f)
			{
				return Mg.t < 20f;
			}
			if (Mg.phase == 0)
			{
				Mg.raypos_s = (Mg.efpos_s = true);
				Mg.phase = 1;
				Mg.Ray.RadiusM(Mg.sz);
				Mg.Ray.HitLock(-1f, null);
				NelAttackInfo nelAttackInfo = (Mg.Atk0 = Mg.MGC.makeAtk());
				nelAttackInfo.hpdmg0 = 20;
				nelAttackInfo.huttobi_ratio = 0.14f;
				nelAttackInfo.burst_vx = 0.15f;
				nelAttackInfo.burst_vy = 0f;
				nelAttackInfo.burst_center = 0.5f;
				nelAttackInfo.knockback_len = 1.2f;
				nelAttackInfo.split_mpdmg = 4;
				nelAttackInfo.attr = MGATTR.FIRE;
				nelAttackInfo.SerDmg = EnemyAttr.SerDmgFire;
				nelAttackInfo.Beto = EnemyAttr.BetoFire;
				nelAttackInfo.nodamage_time = 20;
				nelAttackInfo.ndmg = MDAT.AtkCandleTouch.ndmg;
				nelAttackInfo.Torn(0.04f, 0.14f);
			}
			if (Mg.t >= 10f)
			{
				return false;
			}
			Mg.MnSetRay(Mg.Ray, 0, 0f, 0f);
			Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
			return true;
		}

		public static bool setStain(NelM2DBase M2D, float x, float y, int sizeh, ENATTR nattr, float maxt, bool set_effect = true)
		{
			if ((nattr & ENATTR.FIRE) != ENATTR.NORMAL)
			{
				EnemyAttr.setStain(M2D, x, y, sizeh, StainItem.TYPE.FIRE, maxt, set_effect);
				return true;
			}
			if ((nattr & ENATTR.ICE) != ENATTR.NORMAL)
			{
				EnemyAttr.setStain(M2D, x, y, sizeh, StainItem.TYPE.ICE, maxt, set_effect);
				return true;
			}
			if ((nattr & ENATTR.SLIMY) != ENATTR.NORMAL)
			{
				EnemyAttr.setStain(M2D, x, y, sizeh, StainItem.TYPE.WEB, maxt, set_effect);
				return true;
			}
			return false;
		}

		public static void setStain(NelM2DBase M2D, float x, float y, int sizeh, StainItem.TYPE type, float maxt, bool set_effect = true)
		{
			if (set_effect)
			{
				if (type == StainItem.TYPE.FIRE)
				{
					M2D.curMap.getEffect().PtcSTsetVar("sx", (double)x).PtcSTsetVar("sy", (double)y)
						.PtcST("stain_basic_fire", M2D, PTCThread.StFollow.NO_FOLLOW, null);
				}
				if (type == StainItem.TYPE.WEB)
				{
					M2D.curMap.getEffect().PtcSTsetVar("sx", (double)x).PtcSTsetVar("sy", (double)y)
						.PtcST("stain_basic_web", M2D, PTCThread.StFollow.NO_FOLLOW, null);
				}
				if (type == StainItem.TYPE.ICE)
				{
					M2D.curMap.getEffect().PtcSTsetVar("cx", (double)x).PtcSTsetVar("cy", (double)y)
						.PtcST("stain_basic_ice", M2D, PTCThread.StFollow.NO_FOLLOW, null);
				}
			}
			M2D.STAIN.Set(x, y, type, AIM.B, maxt, null);
			for (int i = 1; i <= sizeh; i++)
			{
				M2D.STAIN.Set(x + (float)i, y, type, AIM.B, maxt, null);
				M2D.STAIN.Set(x - (float)i, y, type, AIM.B, maxt, null);
			}
		}

		public static bool fnRunSplashIce(MagicItem Mg, float fcnt)
		{
			if (Mg.sz == 0f)
			{
				return Mg.t < 20f;
			}
			if (Mg.phase == 0)
			{
				Mg.efpos_s = true;
				Mg.phase = 4;
				Mg.sa = 0f;
				Mg.Ray.RadiusM(0.56f);
				Mg.Ray.HitLock(30f, null);
				Mg.t = 11f;
				Mg.projectile_power = 1;
				float num = Mg.sz * 1.3f + 1.2f;
				Mg.da = (float)X.IntC(num / 0.7f);
				NelAttackInfo nelAttackInfo = (Mg.Atk0 = Mg.MGC.makeAtk());
				nelAttackInfo.hpdmg0 = 8;
				nelAttackInfo.huttobi_ratio = -1000f;
				nelAttackInfo.burst_vx = 0.08f;
				nelAttackInfo.burst_vy = 0f;
				nelAttackInfo.knockback_len = 0.3f;
				nelAttackInfo.attr = MGATTR.ICE;
				nelAttackInfo.nodamage_time = 20;
				nelAttackInfo.Beto = EnemyAttr.BetoIce;
				nelAttackInfo.ndmg = MDAT.AtkCandleTouch.ndmg;
			}
			if (Mg.t >= 11f)
			{
				Mg.t -= 11f;
				Mg.Atk0.SerDmg = (((int)Mg.sa % 2 == 0) ? EnemyAttr.SerDmgIce : null);
				int num2 = 0;
				float num3 = ((Mg.sa == 0f) ? 1.2f : 2.5f);
				using (BList<M2BlockColliderContainer.BCCHitInfo> blist = ListBuffer<M2BlockColliderContainer.BCCHitInfo>.Pop(0))
				{
					for (int i = ((Mg.sa == 0f) ? 1 : 0); i < 2; i++)
					{
						if (((i == 0) ? ((Mg.phase - 1) & 1) : ((Mg.phase - 1) & 2)) != 0)
						{
							float num4 = Mg.sx + (float)X.MPF(i == 0) * (Mg.sa * 0.7f);
							blist.Clear();
							if (Mg.Mp.canThroughBcc(num4, Mg.dy - num3, num4, Mg.dy + num3, 0.01f, 0.01f, 3, true, true, null, false, blist))
							{
								if (Mg.sa == 0f)
								{
									Mg.Ray.PosMap(Mg.sx, Mg.sy);
									Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
									Mg.PtcVar("cx", (double)Mg.sx).PtcVar("cy", (double)Mg.sy).PtcST("enemy_nattr_splash_ice", PTCThread.StFollow.NO_FOLLOW, false);
								}
							}
							else
							{
								num2 |= 1 << i;
								for (int j = 0; j < blist.Count; j++)
								{
									M2BlockColliderContainer.BCCLine hit = blist[j].Hit;
									if (X.BTW(hit.shifted_x, num4, hit.shifted_right))
									{
										float num5 = hit.slopeBottomY(num4) - 0.2f;
										if (j == 0)
										{
											Mg.Ray.PosMap(num4, num5);
											Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
											Mg.PtcVar("cx", (double)num4).PtcVar("cy", (double)num5).PtcST("enemy_nattr_splash_ice", PTCThread.StFollow.NO_FOLLOW, false);
											if (Mg.sa == 0f)
											{
												Mg.sy = num5;
											}
										}
										if (Mg.M2D.STAIN.Set(num4, num5, StainItem.TYPE.ICE, AIM.B, 440f * Mg.dz, hit) != null)
										{
											break;
										}
									}
								}
							}
						}
					}
				}
				if (num2 == 0)
				{
					return false;
				}
				if (Mg.sa == 0f)
				{
					Mg.phase = 4;
				}
				else
				{
					Mg.phase = num2 + 1;
				}
				Mg.sa += 1f;
				if (Mg.sa >= Mg.da)
				{
					return false;
				}
			}
			return true;
		}

		public static bool fnRunSplashThunder(MagicItem Mg, float fcnt)
		{
			if (Mg.sz == 0f)
			{
				return Mg.t < 20f;
			}
			if (Mg.phase == 0)
			{
				Mg.raypos_s = (Mg.efpos_s = true);
				Mg.phase = 1;
				float num = Mg.sz * 0.6f;
				Mg.Ray.RadiusM(num);
				Mg.Ray.HitLock(-1f, null);
				NelAttackInfo nelAttackInfo = (Mg.Atk0 = Mg.MGC.makeAtk());
				nelAttackInfo.hpdmg0 = 20;
				nelAttackInfo.huttobi_ratio = 50f;
				nelAttackInfo.burst_vx = 0.15f;
				nelAttackInfo.burst_vy = 0f;
				nelAttackInfo.knockback_len = 1.2f;
				nelAttackInfo.split_mpdmg = 9;
				nelAttackInfo.attr = MGATTR.THUNDER;
				nelAttackInfo.SerDmg = EnemyAttr.SerDmgThunder;
				nelAttackInfo.pee_apply100 = 15f;
				nelAttackInfo.Beto = EnemyAttr.BetoThunder;
				nelAttackInfo.nodamage_time = 25;
				nelAttackInfo.ndmg = NDMG.NATTR;
				bool flag = false;
				if (Mg.da == 1f)
				{
					M2Attackable m2Attackable = Mg.Caster as M2Attackable;
					if (m2Attackable != null && m2Attackable.hasFoot())
					{
						M2BlockColliderContainer.BCCLine footBCC = m2Attackable.getFootManager().get_FootBCC();
						if (footBCC != null)
						{
							Mg.da = footBCC.housenagR + X.XORSPS() * 3.1415927f * 0.04f;
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					float footableY = Mg.Mp.getFootableY(Mg.sa, (int)Mg.sy, 2, true, -1f, false, true, true, 0f);
					if (footableY >= 0f && X.Abs(Mg.sy - footableY) < 1.25f)
					{
						Mg.da = 1.5707964f + X.XORSPS() * 0.04f * 3.1415927f;
					}
					else
					{
						Mg.da = X.XORSPS() * 3.1415927f;
					}
				}
				nelAttackInfo.Torn(0.04f, 0.14f);
				Mg.PtcVar("cx", (double)Mg.sx).PtcVar("cy", (double)Mg.sy).PtcVar("agR", (double)Mg.da)
					.PtcVar("radius", (double)(num * 1.2f))
					.PtcVar("h", (double)(Mg.sz * 4.5f))
					.PtcST("enemy_nattr_splash_thunder", PTCThread.StFollow.NO_FOLLOW, false);
			}
			if (Mg.t >= 20f)
			{
				return false;
			}
			Mg.MnSetRay(Mg.Ray, 0, Mg.da, 0f);
			Mg.Ray.LenM(Mg.sz * 4f);
			Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
			return true;
		}

		public static void listupAttr(List<string> A)
		{
			A.Add("ATK");
			A.Add("DEF");
			A.Add("MP_STABLE");
			A.Add("FIRE");
			A.Add("ICE");
			A.Add("THUNDER");
			A.Add("SLIMY");
			A.Add("ACME");
			A.Add("INVISIBLE");
		}

		public const int default_nattr_addable_od = 32;

		public const int default_nattr_addable = 16;

		public const float absorb_mattr_replace_ratio = 0.4f;

		public const float absorb_mattr_replace_ratio_od = 1f;

		public const float ser_apply_ratio_to_enemy = 0.5f;

		public const float ser_apply_ratio_in_absorb = 0.3f;

		public const float mp_stable_consume_mp_ratio = 0.08f;

		public const float mp_stable_consume_mp_ratio_mattr = 0.15f;

		public const float mp_stable_damage_mp_ratio = 0.05f;

		public const float mp_stable_damage_hp_ratio = 0.66f;

		public const float mp_stable_damage_hp_ratio_od = 0.9f;

		public const float mp_stable_cure_hp_by_mana_ratio = 0.25f;

		public const float acme_multiple_epdmg = 1.5f;

		public const float acme_reduce_publish_damage_ratio = 0.5f;

		public const float stain_floor_fire_mint = 160f;

		public const float stain_floor_fire_maxt = 300f;

		public const float stain_floor_ice_maxt = 440f;

		public const int slimy_shot_count = 3;

		public const float floor_maxt_multiple_od = 2f;

		public const float slimy_floor_mint = 140f;

		public const float slimy_floor_maxt = 220f;

		public const float acme_publish_hpdmg_ratio = 0.5f;

		public const float slimy_publish_hpdmg_ratio = 0.33f;

		public const float slimy_publish_mpdmg_ratio = 0.66f;

		public const float mp_stable_drop_mp_ratio = 2f;

		public static readonly FlagCounter<SER> SerDmgFire = new FlagCounter<SER>(4).Add(SER.BURNED, 30f);

		public static readonly FlagCounter<SER> SerDmgIce = new FlagCounter<SER>(4).Add(SER.FROZEN, 25f);

		public static readonly FlagCounter<SER> SerDmgThunder = new FlagCounter<SER>(4).Add(SER.PARALYSIS, 40f);

		public static readonly FlagCounter<SER> SerDmgSlimy = new FlagCounter<SER>(4).Add(SER.WEB_TRAPPED, 28f);

		public static readonly FlagCounter<SER> SerDmgAcme = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 20f);

		public static readonly FlagCounter<SER> SerDmgSlimy100 = new FlagCounter<SER>(4).Add(SER.WEB_TRAPPED, 100f);

		public static readonly BetoInfo BetoFire = BetoInfo.Lava.Pow(30, false);

		public static readonly BetoInfo BetoThunder = BetoInfo.Thunder.Pow(25, false).Level(10f, true);

		public static readonly BetoInfo BetoIce = new BetoInfo(40f, C32.d2c(4290898170U), C32.d2c(4287147454U), 20f, BetoInfo.TYPE.LIQUID, 1f, 0, 0.2f);

		public static readonly BetoInfo BetoSlimy = new BetoInfo(80f, C32.d2c(uint.MaxValue), C32.d2c(3451043010U), 85f, BetoInfo.TYPE.LIQUID, 0.5f, 0, 0.2f);

		public static EnemyAttr.FnDelegateSetSplash[] AFnSplash = new EnemyAttr.FnDelegateSetSplash[]
		{
			delegate(NelEnemy En, float x, float y, float radius, float consume_ratio)
			{
				EnemyAttr.Splash(En, ENATTR.FIRE, x, y, radius, consume_ratio, 1f);
			},
			delegate(NelEnemy En, float x, float y, float radius, float consume_ratio)
			{
				EnemyAttr.Splash(En, ENATTR.ICE, x, y, radius, consume_ratio, 1f);
			},
			delegate(NelEnemy En, float x, float y, float radius, float consume_ratio)
			{
				EnemyAttr.Splash(En, ENATTR.THUNDER, x, y, radius, consume_ratio, 1f);
			},
			delegate(NelEnemy En, float x, float y, float radius, float consume_ratio)
			{
				EnemyAttr.Splash(En, ENATTR.SLIMY, x, y, radius, consume_ratio, 1f);
			},
			delegate(NelEnemy En, float x, float y, float radius, float consume_ratio)
			{
				EnemyAttr.Splash(En, ENATTR.ACME, x, y, radius, consume_ratio, 1f);
			}
		};

		private static NOD.MpConsume McsSplashFire;

		private static NOD.MpConsume McsSplashIce;

		private static NOD.MpConsume McsSplashIceBullet;

		private static NOD.MpConsume McsSplashThunder;

		private static NOD.MpConsume McsSplashAcme;

		private static NOD.MpConsume McsSplashSlimy;

		public static AttackInfo.FnHitAfter NattrHitAfter = delegate(AttackInfo bAtk, IM2RayHitAble Target, HITTYPE touched_hittpe)
		{
			if (touched_hittpe == HITTYPE.NONE)
			{
				return;
			}
			NelEnemy nelEnemy = (bAtk as EnAttackInfo).Caster as NelEnemy;
			if (nelEnemy != null)
			{
				EnemyAttr.HitAfter(nelEnemy, Target);
			}
		};

		public delegate void FnDelegateSetSplash(NelEnemy En, float x, float y, float radius, float consume_ratio);
	}
}
