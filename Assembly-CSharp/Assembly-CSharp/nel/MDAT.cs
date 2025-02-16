using System;
using m2d;
using XX;

namespace nel
{
	public static class MDAT
	{
		public static int getWormEggLayableMax(PR Pr)
		{
			return X.Mn(Pr.NM2D.NightCon.getBattleCount() - 2, 6);
		}

		public static NelAttackInfo AtkShieldLariatHit()
		{
			return new NelAttackInfo
			{
				hpdmg0 = 9,
				knockback_len = 0.8f
			};
		}

		public static int getPrDamageVal(int val, NelAttackInfoBase Atk, PR Pr)
		{
			if (!Atk.fix_damage)
			{
				float num = 1f * (((!Pr.isAbsorbState() && Pr.isDownState()) || !Pr.is_alive) ? X.NIXP(1.2f, 1.5f) : 1f);
				val = X.IntC((float)val * num);
			}
			if (val > 0)
			{
				val = DIFF.getPrDamageVal(val, Atk, Pr);
			}
			return val;
		}

		public static bool canApplySplitMpDamage(NelAttackInfoBase Atk, PR Pr)
		{
			return !Pr.isPuzzleManagingMp() && (Pr.Skill.hasMagic() || Pr.isAbsorbState() || Pr.isTrappedState() || Pr.Ser.hasBit(21928560704UL));
		}

		public static int getMpDamageValue(NelAttackInfoBase Atk, PR Pr, float val)
		{
			float num = 1f * (Pr.magic_chanting ? 1f : 1f) * ((Pr.isDamagingOrKo() && !Pr.isAbsorbState()) ? ((Pr.get_mp() <= 0f) ? 2f : 1.33f) : 1f);
			if (Pr.Skill.hasMagic() || Pr.magic_chanting)
			{
				num *= Pr.Ser.chantMpSplitRate();
				num *= Pr.lost_mp_in_chanting_ratio;
			}
			return X.IntC((Pr.is_alive ? X.NI(0.5f, 1f, Pr.mp_ratio) : 1f) * val * num);
		}

		public static float getManaDesire(NelM2Attacker Mv, M2Attackable Mpos, int mana_hit, DRect Rc, int already_absorbing_count)
		{
			if (!Mpos.is_alive && !Mpos.overkill)
			{
				return 0f;
			}
			if ((mana_hit & 1) == 0 && Mpos is M2MoverPr)
			{
				return 0f;
			}
			if ((mana_hit & 2) == 0 && Mpos is NelEnemy)
			{
				return 0f;
			}
			float num = Mv.getMpDesireRatio(already_absorbing_count * 4);
			if (num < 0f)
			{
				return 0f;
			}
			float mpDrainRate = Mv.getSer().getMpDrainRate();
			if (mpDrainRate != 1f)
			{
				num /= mpDrainRate;
			}
			float num2 = 0f;
			bool flag = (mana_hit & 1024) != 0;
			if (!flag)
			{
				if ((mana_hit & 4099) == 4097)
				{
					num2 += 4f;
				}
				else if (num >= 1f)
				{
					return 0f;
				}
				num2 += 1f + X.Mx(Mpos.sizex, Mpos.sizey) + X.ZLINE(1f - num) * 8f;
			}
			else
			{
				num2 = 2f + X.Mx(Mpos.sizex, Mpos.sizey) + X.ZLINE(1f - num) * 2f;
			}
			float lengthInContainer = Rc.getLengthInContainer(Mpos.x, Mpos.y, 1f);
			if (lengthInContainer > num2)
			{
				return 0f;
			}
			if (flag)
			{
				return X.Mx(0f, 6f - X.Mx(0f, lengthInContainer - X.Mx(Mpos.sizex, Mpos.sizey)));
			}
			float mpDesireMaxValue = Mv.getMpDesireMaxValue();
			if (mpDesireMaxValue > 0f)
			{
				return mpDesireMaxValue * X.Pow((1f - num) * 2f + 1f, 2) / 9f;
			}
			return 0f;
		}

		public static float calcBurstFaintedRatio(PR Pr, MagicSelector.KindData MK, float execute_count)
		{
			if (MK == null)
			{
				return 0f;
			}
			int reduce_mp = MK.reduce_mp;
			float num = X.ZSIN((float)reduce_mp - Pr.get_mp(), (float)reduce_mp * 0.75f);
			num = num * ((execute_count == 0f) ? 0.125f : 1f) + execute_count * 0.333f;
			int num2 = Pr.Ser.getLevel(SER.OVERRUN_TIRED) + 1;
			float num3 = X.NIL(0f, 0.8f, (float)num2, 5f);
			return num + num3 * X.saturate(num);
		}

		public static float progressBurstExecuteCount(float t)
		{
			if (t <= 1f)
			{
				return X.NIL(1f, 3f, t, 1f);
			}
			if (t <= 3f)
			{
				return X.NIL(3f, 7f, t - 1f, 2f);
			}
			if (t <= 7f)
			{
				return X.NIL(7f, 10f, t - 3f, 4f);
			}
			return 1f;
		}

		public static void initMapDamage(M2MapDamageContainer MDMG)
		{
			for (int i = 0; i < 5; i++)
			{
				NelAttackInfo nelAttackInfo = new NelAttackInfo
				{
					attr = MGATTR.NORMAL,
					ndmg = NDMG.MAPDAMAGE,
					huttobi_ratio = -100f,
					shield_break_ratio = 0f,
					Beto = null,
					hpdmg_current = -1000,
					mpdmg_current = -1000
				};
				M2MapDamageContainer.FnIsMDIUseable fnIsMDIUseable = null;
				nelAttackInfo.Torn(0f, 0f).PeeApply(0f);
				MAPDMG mapdmg = (MAPDMG)i;
				switch (mapdmg)
				{
				case MAPDMG.SPIKE:
					nelAttackInfo.attr = MGATTR.CUT_H;
					nelAttackInfo.Torn(0.11111111f, 0.25f);
					nelAttackInfo.Beto = BetoInfo.Blood;
					fnIsMDIUseable = delegate(M2MapDamageContainer.M2MapDamageItem MDI, MAPDMG kind, AttackInfo _Atk, M2BlockColliderContainer.BCCLine Bcc, M2Attackable MvA, float efx, float efy)
					{
						NelAttackInfo nelAttackInfo2 = _Atk as NelAttackInfo;
						nelAttackInfo2.hpdmg0 = MvA.ratingHpDamageVal(0.05f);
						nelAttackInfo2.Burst(X.NIXP(0.08f, 0.12f) * DIFF.spike_burst_vx_ratio(MvA), 0f);
						bool flag = MvA.canGoToSide(AIM.L, 0.4f, -0.1f, false, false, false);
						bool flag2 = MvA.canGoToSide(AIM.R, 0.4f, -0.1f, false, false, false);
						nelAttackInfo2.BurstDir((float)((flag && !flag2) ? (-1) : ((flag2 && !flag) ? 1 : X.MPFXP())));
						nelAttackInfo2.nodamage_time = (int)X.NIXP(70f, 100f);
						nelAttackInfo2.split_mpdmg = 0;
						if (!MvA.is_alive)
						{
							nelAttackInfo2.split_mpdmg = 3;
							if (X.XORSP() < 0.7f)
							{
								nelAttackInfo2.nodamage_time = (int)((float)nelAttackInfo2.nodamage_time * 0.3f);
							}
						}
						return nelAttackInfo2;
					};
					break;
				case MAPDMG.THUNDER:
				case MAPDMG.THUNDER_STATIC:
					nelAttackInfo.ndmg = ((mapdmg == MAPDMG.THUNDER_STATIC) ? NDMG.MAPDAMAGE_THUNDER_A : NDMG.MAPDAMAGE_THUNDER);
					nelAttackInfo.Torn(0.071428575f, 0.125f);
					nelAttackInfo.attr = MGATTR.THUNDER;
					nelAttackInfo.Beto = BetoInfo.Thunder;
					nelAttackInfo.shield_break_ratio = -1f;
					nelAttackInfo.PeeApply(40f);
					nelAttackInfo.nodamage_time = 60;
					nelAttackInfo.shield_success_nodamage = 25f;
					fnIsMDIUseable = delegate(M2MapDamageContainer.M2MapDamageItem MDI, MAPDMG kind, AttackInfo _Atk, M2BlockColliderContainer.BCCLine Bcc, M2Attackable MvA, float efx, float efy)
					{
						NelAttackInfo nelAttackInfo3 = _Atk as NelAttackInfo;
						MDI.ui_fade_key = "damage_thunder";
						nelAttackInfo3.huttobi_ratio = (float)((MvA is M2MoverPr) ? 1000 : 0);
						nelAttackInfo3.hpdmg0 = MvA.ratingHpDamageVal(0.1f);
						nelAttackInfo3.CenterXy(efx, efy, 0.12f).HitXy(efx, efy, false);
						nelAttackInfo3.Burst(0.16f * (float)X.MPF(efx < MvA.x), -0.04f);
						nelAttackInfo3.split_mpdmg = 0;
						if (!MvA.is_alive)
						{
							nelAttackInfo3.split_mpdmg = 12;
						}
						return nelAttackInfo3;
					};
					break;
				case MAPDMG.FIRE:
					nelAttackInfo.CopyFrom(MDAT.AtkCandleTouch);
					break;
				case MAPDMG.LAVA:
					nelAttackInfo.attr = MGATTR.FIRE;
					nelAttackInfo.nodamage_time = 20;
					nelAttackInfo.ndmg = NDMG.MAPDAMAGE_LAVA;
					nelAttackInfo.hpdmg0 = 25;
					nelAttackInfo.burst_vy = -0.17f;
					nelAttackInfo.shield_break_ratio = 0f;
					nelAttackInfo.SerDmg = new FlagCounter<SER>(4).Add(SER.BURNED, 500f);
					nelAttackInfo.Beto = BetoInfo.Lava;
					nelAttackInfo.Torn(0.08f, 0.2f);
					fnIsMDIUseable = delegate(M2MapDamageContainer.M2MapDamageItem MDI, MAPDMG kind, AttackInfo _Atk, M2BlockColliderContainer.BCCLine Bcc, M2Attackable MvA, float efx, float efy)
					{
						NelAttackInfo nelAttackInfo4 = _Atk as NelAttackInfo;
						MDI.ui_fade_key = ((MvA is PR && X.XORSP() < 0.5f) ? "burned" : null);
						nelAttackInfo4.hpdmg0 = MvA.ratingHpDamageVal(0.125f);
						if (MvA is NelEnemy)
						{
							nelAttackInfo4.hpdmg0 = X.Mn(25, nelAttackInfo4.hpdmg0);
						}
						if (!MvA.is_alive && X.XORSP() < 0.3f)
						{
							nelAttackInfo4.split_mpdmg = 8;
						}
						return nelAttackInfo4;
					};
					break;
				}
				if (fnIsMDIUseable == null)
				{
					MDMG.AssignAtkData(mapdmg, nelAttackInfo, Array.Empty<M2MapDamageContainer.FnIsMDIUseable>());
				}
				else
				{
					MDMG.AssignAtkData(mapdmg, nelAttackInfo, new M2MapDamageContainer.FnIsMDIUseable[] { fnIsMDIUseable });
				}
			}
		}

		public static void initMagicItem(MagicItem Mg, out bool init_aimpos_to_d)
		{
			MGContainer mgc = Mg.MGC;
			Mg.casttime = 0f;
			Mg.mp_crystalize = 0.5f;
			init_aimpos_to_d = true;
			if ((Mg.hittype & MGHIT.IMMEDIATE) == (MGHIT)0)
			{
				Mg.phase = -14;
			}
			MGKIND kind = Mg.kind;
			if (kind > MGKIND.BASIC_SHOT)
			{
				if (kind <= MGKIND.ITEMBOMB_NORMAL)
				{
					if (kind == MGKIND.BASIC_BEAM)
					{
						goto IL_0DF4;
					}
					switch (kind)
					{
					case MGKIND.CANDLE_SHOT:
						mgc.initFunc(Mg);
						Mg.Atk0 = MDAT.AtkCandleTouch;
						goto IL_0FB8;
					case MGKIND.ICE_SHOT:
						mgc.initFunc(Mg);
						goto IL_0FB8;
					case MGKIND.CEILDROP:
						mgc.initFunc(Mg);
						goto IL_0FB8;
					default:
						if (kind != MGKIND.ITEMBOMB_NORMAL)
						{
							goto IL_0F70;
						}
						break;
					}
				}
				else if (kind <= MGKIND.ITEMBOMB_MAGIC)
				{
					if (kind != MGKIND.ITEMBOMB_LIGHT && kind != MGKIND.ITEMBOMB_MAGIC)
					{
						goto IL_0F70;
					}
				}
				else
				{
					if (kind == MGKIND.EF_WORM_PUBLISH)
					{
						Mg.hittype |= MGHIT.IMMEDIATE;
						Mg.hittype &= (MGHIT)(-8193);
						Mg.initFuncNoDraw((MagicItem Mg, float fcnt) => MagicItem.runEfWormPublish(Mg, fcnt));
						goto IL_0FB8;
					}
					if (kind != MGKIND.EF_WHOLEMAP)
					{
						goto IL_0F70;
					}
					Mg.hittype |= MGHIT.IMMEDIATE;
					Mg.hittype &= (MGHIT)(-8193);
					goto IL_0FB8;
				}
				Mg.hittype |= MGHIT.IMMEDIATE;
				Mg.hittype &= (MGHIT)(-8193);
				NelAttackInfo nelAttackInfo = (Mg.Atk0 = mgc.makeAtk());
				mgc.Notf.GetForCaster(Mg, MGKIND.ITEMBOMB_NORMAL);
				mgc.initFunc(Mg);
				Mg.changeRay(mgc.makeRay(Mg, 0f, true, false));
				goto IL_0FB8;
			}
			if (kind <= MGKIND.PR_BURST)
			{
				switch (kind)
				{
				case MGKIND.WHITEARROW:
				{
					Mg.hittype |= MGHIT.CHANTED;
					Mg.explode_pos_c = true;
					NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 60;
					nelAttackInfo2.huttobi_ratio = 1.35f;
					nelAttackInfo2.split_mpdmg = 2;
					nelAttackInfo2.attack_max0 = 1;
					nelAttackInfo2 = (Mg.Atk1 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 88;
					nelAttackInfo2.huttobi_ratio = 1.35f;
					nelAttackInfo2.split_mpdmg = 14;
					nelAttackInfo2.attack_max0 = 1;
					Mg.padvib_enable = true;
					MagicSelector.initMagic(Mg);
					nelAttackInfo2 = (Mg.Atk2 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 15;
					nelAttackInfo2.huttobi_ratio = 2.25f;
					nelAttackInfo2.burst_center = -0.1f;
					nelAttackInfo2.split_mpdmg = 35;
					nelAttackInfo2.attack_max0 = 1;
					mgc.Notf.GetForCaster(Mg);
					if ((Mg.hittype & MGHIT.IMMEDIATE) > (MGHIT)0)
					{
						mgc.initFunc(Mg);
						Mg.casttime = 0f;
						goto IL_0FB8;
					}
					Mg.phase = -7;
					goto IL_0FB8;
				}
				case MGKIND.FIREBALL:
				{
					Mg.hittype |= MGHIT.CHANTED;
					NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 64;
					nelAttackInfo2.huttobi_ratio = 2f;
					nelAttackInfo2.burst_center = -0.4f;
					nelAttackInfo2.damage_randomize_min = 0.65f;
					nelAttackInfo2.split_mpdmg = 2;
					nelAttackInfo2 = (Mg.Atk1 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 70;
					nelAttackInfo2.split_mpdmg = 2;
					nelAttackInfo2.centerblur_damage_ratio = 0.8f;
					nelAttackInfo2.burst_center = -0.25f;
					nelAttackInfo2.damage_randomize_min = 0.35f;
					nelAttackInfo2.attack_max0 = 5;
					nelAttackInfo2.pr_myself_fire = true;
					Mg.padvib_enable = true;
					MagicSelector.initMagic(Mg);
					mgc.Notf.GetForCaster(Mg);
					if ((Mg.hittype & MGHIT.IMMEDIATE) > (MGHIT)0)
					{
						mgc.initFunc(Mg);
						Mg.casttime = 0f;
						goto IL_0FB8;
					}
					Mg.phase = -42;
					goto IL_0FB8;
				}
				case MGKIND.DROPBOMB:
				{
					Mg.hittype |= MGHIT.CHANTED;
					NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 15;
					nelAttackInfo2.huttobi_ratio = 2.45f;
					nelAttackInfo2.burst_vy = -0.12f;
					nelAttackInfo2.split_mpdmg = 17;
					nelAttackInfo2.attack_max0 = 3;
					nelAttackInfo2.attr = MGATTR.BOMB;
					nelAttackInfo2.SerDmg = mgc.makeSDmg().Add(SER.EATEN, 50f);
					MagicSelector.initMagic(Mg);
					Mg.explode_pos_c = true;
					mgc.Notf.GetForCaster(Mg);
					if ((Mg.hittype & MGHIT.IMMEDIATE) > (MGHIT)0)
					{
						mgc.initFunc(Mg);
						Mg.sz = 2f;
						Mg.casttime = 0f;
						goto IL_0FB8;
					}
					Mg.phase = -(int)Mg.Mn._0.maxt;
					goto IL_0FB8;
				}
				case MGKIND.THUNDERBOLT:
				{
					Mg.hittype |= MGHIT.CHANTED;
					NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 35;
					nelAttackInfo2.huttobi_ratio = 0f;
					nelAttackInfo2.burst_vx = 0.15f;
					nelAttackInfo2.burst_vy = 0f;
					nelAttackInfo2.split_mpdmg = 2;
					nelAttackInfo2.attr = MGATTR.THUNDER;
					nelAttackInfo2.SerDmg = mgc.makeSDmg().Add(SER.PARALYSIS, 40f);
					nelAttackInfo2.pee_apply100 = 15f;
					nelAttackInfo2 = (Mg.Atk1 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 70;
					nelAttackInfo2.huttobi_ratio = 2f;
					nelAttackInfo2.burst_vx = 0.17f;
					nelAttackInfo2.burst_vy = -0.11f;
					nelAttackInfo2.split_mpdmg = 14;
					nelAttackInfo2.damage_randomize_min = 0.5f;
					nelAttackInfo2.attr = MGATTR.THUNDER;
					nelAttackInfo2.SerDmg = mgc.makeSDmg().Add(SER.PARALYSIS, 40f);
					nelAttackInfo2.pee_apply100 = 30f;
					nelAttackInfo2.Torn(0.04f, 0.14f);
					MagicSelector.initMagic(Mg);
					mgc.Notf.GetForCaster(Mg);
					if ((Mg.hittype & MGHIT.IMMEDIATE) > (MGHIT)0)
					{
						mgc.initFunc(Mg);
						Mg.casttime = 0f;
						goto IL_0FB8;
					}
					goto IL_0FB8;
				}
				case MGKIND.POWERBOMB:
				{
					Mg.hittype |= MGHIT.CHANTED;
					NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
					nelAttackInfo2.hpdmg0 = 20;
					nelAttackInfo2.huttobi_ratio = 1.45f;
					nelAttackInfo2.burst_center = 1f;
					nelAttackInfo2.burst_vx = 0.04f;
					nelAttackInfo2.burst_vy = -0.02f;
					nelAttackInfo2.split_mpdmg = 0;
					nelAttackInfo2.attack_max0 = 20;
					nelAttackInfo2.attr = MGATTR.BOMB;
					nelAttackInfo2.SerDmg = mgc.makeSDmg().Add(SER.EATEN, 9f);
					MagicSelector.initMagic(Mg);
					Mg.explode_pos_c = true;
					mgc.Notf.GetForCaster(Mg);
					if ((Mg.hittype & MGHIT.IMMEDIATE) > (MGHIT)0)
					{
						mgc.initFunc(Mg);
						Mg.casttime = 0f;
						goto IL_0FB8;
					}
					Mg.phase = -(int)Mg.Mn._0.maxt;
					goto IL_0FB8;
				}
				default:
					switch (kind)
					{
					case MGKIND.PR_PUNCH:
					case MGKIND.PR_SHOTGUN:
					case MGKIND.PR_COMET:
					case MGKIND.PR_DASHPUNCH:
					case MGKIND.PR_SHIELD_COUNTER:
					{
						float num = (float)X.MPF(CAim._XD(Mg.Caster.getAimForCaster(), 1) >= 0);
						Mg.sx = X.Abs(0.75f) * num;
						Mg.sy = 0.55f;
						Mg.sz = 0.45f;
						NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
						nelAttackInfo2.hpdmg0 = 7;
						nelAttackInfo2.split_mpdmg = X.IntR(X.NIXP(0.5f, 1f) * 18f);
						nelAttackInfo2.attr = MGATTR.NORMAL;
						nelAttackInfo2.burst_vx = 0f;
						nelAttackInfo2.burst_vy = 0f;
						nelAttackInfo2.attack_max0 = 1;
						nelAttackInfo2.hit_ptcst_name = "pr_cane_hit";
						nelAttackInfo2.fnHitEffectPre = delegate(AttackInfo Atk, IM2RayHitAble Hit)
						{
							NelAttackInfo nelAttackInfo3 = Atk as NelAttackInfo;
							if (nelAttackInfo3.Caster != null)
							{
								PTCThreadRunner.PreVar("ax", (double)CAim._XD(nelAttackInfo3.Caster.getAimForCaster(), 1));
							}
						};
						nelAttackInfo2.knockback_len = Mg.sz + Mg.sy + 0.45f;
						Mg.hittype |= MGHIT.NORMAL_ATTACK;
						Mg.mp_crystalize = 0f;
						Mg.initFuncNoDraw((MagicItem Mg, float fcnt) => MagicItem.runTackle(Mg, fcnt));
						init_aimpos_to_d = false;
						Mg.changeRay(mgc.makeRay(Mg, 0f, (Mg.hittype & MGHIT.EN) == (MGHIT)0, true));
						Mg.Ray.HitLock(-1f, null);
						if (Mg.kind == MGKIND.PR_DASHPUNCH)
						{
							Mg.sx = X.Abs(2.92f) * num;
							Mg.sz = 0.66f;
							Mg.sy = 0.16f;
							nelAttackInfo2.knockback_len = 0.8f;
							nelAttackInfo2.burst_vx = num * 0.3f;
							nelAttackInfo2.burst_vy = -0.04f;
							nelAttackInfo2.attack_max0 = 3;
							nelAttackInfo2.split_mpdmg = 9;
							Mg.phase = 1;
						}
						if (Mg.kind == MGKIND.PR_SHOTGUN)
						{
							Mg.sy -= 0.125f;
							Mg.sz += 0.125f;
							Mg.hittype |= MGHIT.CHANTED;
						}
						if (Mg.kind == MGKIND.PR_COMET)
						{
							nelAttackInfo2.knockback_len = 0f;
							nelAttackInfo2.attack_max0 = -1;
							nelAttackInfo2.hit_ptcst_name = "pr_comet_hit";
							nelAttackInfo2.burst_vy = -0.13f;
							nelAttackInfo2.split_mpdmg = 2;
							Mg.sx = 0f;
							Mg.sy = 0.9f;
							Mg.sz = 0.5f;
							Mg.phase = 1;
							goto IL_0FB8;
						}
						goto IL_0FB8;
					}
					case MGKIND.PR_SLIDING:
					{
						float num2 = (float)X.MPF(CAim._XD(Mg.Caster.getAimForCaster(), 1) >= 0);
						M2Mover m2Mover = Mg.Caster as M2Mover;
						Mg.sx = X.Abs(0.75f) * num2;
						Mg.sy = ((m2Mover != null) ? m2Mover.get_sizey() : 0.25f);
						Mg.sz = 0.35f;
						NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
						nelAttackInfo2.hpdmg0 = 0;
						nelAttackInfo2.attr = MGATTR.NORMAL;
						nelAttackInfo2.burst_vx = 0f;
						nelAttackInfo2.burst_vy = 0f;
						nelAttackInfo2.attack_max0 = -1;
						nelAttackInfo2.hit_ptcst_name = "pr_sliding_hit";
						nelAttackInfo2.knockback_len = 0.2f;
						Mg.hittype |= MGHIT.NORMAL_ATTACK;
						Mg.changeRay(mgc.makeRay(Mg, 0f, (Mg.hittype & MGHIT.EN) == (MGHIT)0, false));
						Mg.Ray.HitLock(-1f, null);
						Mg.initFuncNoDraw((MagicItem Mg, float fcnt) => MagicItem.runTackle(Mg, fcnt));
						init_aimpos_to_d = false;
						goto IL_0FB8;
					}
					case MGKIND.PR_WHEEL:
					{
						Mg.sx = 0f;
						Mg.sy = 0f;
						Mg.sz = 1.9f;
						NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
						nelAttackInfo2.hpdmg0 = 3;
						nelAttackInfo2.split_mpdmg = 2;
						nelAttackInfo2.attr = MGATTR.NORMAL;
						nelAttackInfo2.burst_vx = 0.18f;
						nelAttackInfo2.burst_vy = 0.08f;
						nelAttackInfo2.attack_max0 = 1;
						nelAttackInfo2.hit_ptcst_name = "pr_wheel_hit";
						nelAttackInfo2.fnHitEffectPre = delegate(AttackInfo Atk, IM2RayHitAble Hit)
						{
							NelAttackInfo nelAttackInfo4 = Atk as NelAttackInfo;
							if (nelAttackInfo4.Caster != null)
							{
								PTCThreadRunner.PreVar("ax", (double)CAim._XD(nelAttackInfo4.Caster.getAimForCaster(), 1));
							}
						};
						nelAttackInfo2.knockback_len = 0f;
						Mg.hittype |= MGHIT.NORMAL_ATTACK;
						Mg.initFuncNoDraw((MagicItem Mg, float fcnt) => MagicItem.runTackle(Mg, fcnt));
						init_aimpos_to_d = false;
						Mg.changeRay(mgc.makeRay(Mg, 0f, true, false));
						Mg.Ray.HitLock(5f, null);
						Mg.phase = 1;
						Mg.mp_crystalize = 0f;
						goto IL_0FB8;
					}
					case MGKIND.PR_SHIELD_BUSH:
					{
						Mg.sx = 0f;
						Mg.sy = 0f;
						Mg.sz = -2.8f;
						NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
						nelAttackInfo2.hpdmg0 = 3;
						nelAttackInfo2.split_mpdmg = 0;
						nelAttackInfo2.attr = MGATTR.NORMAL;
						nelAttackInfo2.burst_vx = 0.13f;
						nelAttackInfo2.burst_vy = -0.12f;
						nelAttackInfo2.attack_max0 = -1;
						nelAttackInfo2.knockback_len = 0f;
						Mg.hittype |= MGHIT.NORMAL_ATTACK;
						Mg.initFuncNoDraw((MagicItem Mg, float fcnt) => MagicItem.runTackle(Mg, fcnt));
						init_aimpos_to_d = false;
						Mg.changeRay(mgc.makeRay(Mg, 0f, false, false));
						Mg.Ray.HitLock(-1f, null);
						Mg.phase = 1;
						Mg.mp_crystalize = 0f;
						goto IL_0FB8;
					}
					case MGKIND.PR_SHIELD_LARIAT:
					{
						float num3 = (float)X.MPF(CAim._XD(Mg.Caster.getAimForCaster(), 1) >= 0);
						Mg.sx = 1.1f * num3;
						Mg.sy = -0.5f;
						Mg.dx = 1f * num3;
						Mg.sz = -1.6f;
						NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
						nelAttackInfo2.hpdmg0 = 2;
						nelAttackInfo2.split_mpdmg = 0;
						nelAttackInfo2.attr = MGATTR.NORMAL;
						nelAttackInfo2.burst_vx = 0.16f;
						nelAttackInfo2.burst_vy = -0.07f;
						nelAttackInfo2.attack_max0 = -1;
						nelAttackInfo2.knockback_len = 0f;
						nelAttackInfo2.hit_ptcst_name = "hit_shield_atk";
						Mg.hittype |= MGHIT.NORMAL_ATTACK;
						Mg.initFuncNoDraw((MagicItem Mg, float fcnt) => MagicItem.runTackle(Mg, fcnt));
						init_aimpos_to_d = false;
						Mg.changeRay(mgc.makeRay(Mg, 0f, false, false));
						Mg.Ray.HitLock(-1f, null);
						Mg.phase = 1;
						Mg.mp_crystalize = 0f;
						goto IL_0FB8;
					}
					default:
					{
						if (kind != MGKIND.PR_BURST)
						{
							goto IL_0F70;
						}
						NelAttackInfo nelAttackInfo2 = (Mg.Atk0 = mgc.makeAtk());
						nelAttackInfo2.hpdmg0 = 20;
						nelAttackInfo2.split_mpdmg = 5;
						nelAttackInfo2.ignore_nodamage_time = true;
						nelAttackInfo2.burst_center = 0.5f;
						nelAttackInfo2.attr = MGATTR.NORMAL;
						nelAttackInfo2.burst_vx = 1.4f;
						nelAttackInfo2.burst_vy = -0.8f;
						nelAttackInfo2.attack_max0 = -1;
						nelAttackInfo2.huttobi_ratio = 5f;
						nelAttackInfo2.ndmg = NDMG.GRAB_PENETRATE;
						Mg.hittype |= MGHIT.NORMAL_ATTACK;
						MagicSelector.initMagic(Mg);
						Mg.changeRay(mgc.makeRay(Mg, 2.4f, true, true));
						Mg.Ray.HitLock(-1f, null);
						mgc.initFunc(Mg);
						Mg.casttime = 0f;
						Mg.hittype |= MGHIT.CHANTED;
						goto IL_0FB8;
					}
					}
					break;
				}
			}
			else
			{
				if (kind == MGKIND.TACKLE)
				{
					Mg.hittype |= MGHIT.NORMAL_ATTACK;
					Mg.initFuncNoDraw((MagicItem Mg, float fcnt) => MagicItem.runTackle(Mg, fcnt));
					init_aimpos_to_d = false;
					goto IL_0FB8;
				}
				if (kind == MGKIND.GROUND_SHOCKWAVE)
				{
					Mg.hittype |= MGHIT.IMMEDIATE;
					Mg.hittype &= (MGHIT)(-8193);
					Mg.sz = 2.4f;
					Mg.sa = 0.6f;
					Mg.changeRay(mgc.makeRay(Mg, 0.02f, false, false));
					mgc.initFunc(Mg);
					goto IL_0FB8;
				}
				if (kind != MGKIND.BASIC_SHOT)
				{
					goto IL_0F70;
				}
			}
			IL_0DF4:
			Mg.initFuncNoDraw(MagicItem.FD_runOnlyMakeRay);
			goto IL_0FB8;
			IL_0F70:
			Mg.hittype |= MGHIT.IMMEDIATE;
			Mg.hittype &= (MGHIT)(-8193);
			X.dl("不明な魔法タイプ: " + Mg.kind.ToString(), null, false, false);
			IL_0FB8:
			if (Mg.is_chanted_magic && PUZ.IT.isPuzzleManagingMp())
			{
				Mg.mp_crystalize = 0f;
				if (Mg.casttime > 0f)
				{
					Mg.casttime = 20f;
				}
				if (Mg.reduce_mp > 0)
				{
					Mg.reduce_mp = (int)(64f * (float)((Mg.kind == MGKIND.PR_BURST) ? 2 : 1));
				}
			}
		}

		public static void initBurstAtkForScapeCat(MagicItem Mg)
		{
			Mg.reduce_mp = 0;
			Mg.Atk0.tired_time_to_super_armor = 45f;
			Mg.Atk0.hpdmg0 = 15;
			Mg.Atk0.mpdmg0 = 0;
			Mg.Atk0.split_mpdmg = 0;
		}

		public static void getScapecatReversalHpMp(int grade, PR Pr, out int hp, out int mp, out float mp_gauge_cure_ratio, out int add_danger)
		{
			float num = (float)grade / 4f;
			hp = X.IntC(((Pr == null) ? 100f : Pr.get_maxhp()) * X.NI(0.2f, 1f, num));
			mp = X.IntC(((Pr == null) ? 100f : Pr.get_maxmp()) * X.NI(0.15f, 1f, num));
			add_danger = grade + 3;
			mp_gauge_cure_ratio = X.NI(0.15f, 0.5f, num);
		}

		public static bool isUsingItem(MagicItem Mg)
		{
			MGKIND kind = Mg.kind;
			return kind == MGKIND.ITEMBOMB_NORMAL || kind == MGKIND.ITEMBOMB_LIGHT || kind == MGKIND.ITEMBOMB_MAGIC;
		}

		public static int ExplodePrepare(MagicItem Mg)
		{
			MGKIND kind = Mg.kind;
			return X.Mx(0, -Mg.phase);
		}

		public static int getMagicExplodeAfterDelay(MagicItem Mg)
		{
			return 31;
		}

		public static int getExplodeCircleReleaseTime(MagicItem Mg)
		{
			return 18;
		}

		public static bool initShotGun(MagicItem MgShot, MagicItem MgHold, float mp_hold, float my_mp)
		{
			if (MgHold.casttime <= 0f || MgHold.reduce_mp <= 0 || MgHold.Atk0 == null)
			{
				return false;
			}
			NelAttackInfo atk = MgShot.Atk0;
			float num = -1f;
			float num2 = 1f;
			float num3 = 1.5f;
			MagicSelector.KindData kindData = MagicSelector.getKindData(MgHold.kind);
			if (kindData != null)
			{
				num3 = kindData.shotgun_ratio;
			}
			MGKIND kind = MgShot.kind;
			if (kind != MGKIND.PR_WHEEL)
			{
				if (kind == MGKIND.PR_DASHPUNCH)
				{
					num3 *= 1f;
				}
			}
			else
			{
				num = X.ZSINV(mp_hold, (float)MgHold.reduce_mp) * 0.75f;
				num3 = num3 / 1.5f * num;
				num2 = 0.5f;
				atk.attack_max0 = -1;
				atk.hit_ptcst_name = "";
				MgShot.phase = 1;
			}
			if (num < 0f)
			{
				num = X.ZPOW(mp_hold, (float)MgHold.reduce_mp);
				num3 *= num;
				num2 = X.Mx(1f, num * 2f);
				float num4 = 0.3f + 0.7f * num;
				atk.Burst(1.3f * num4, -0.3f * num4);
				if (MgShot.kind == MGKIND.PR_PUNCH)
				{
					atk.split_mpdmg = X.IntR(X.NI((float)atk.split_mpdmg, 32f, num));
				}
			}
			atk.hpdmg0 = X.IntC(num3 * (float)MgHold.Atk0.hpdmg0);
			if (MgHold.Atk0.SerDmg != null)
			{
				atk.SerDmg = MgShot.MGC.makeSDmg();
				atk.SerDmg.Add(MgHold.Atk0.SerDmg, num2);
			}
			atk.attr = MgHold.Atk0.attr;
			MgShot.reduce_mp = X.IntC(X.Mn((float)MgHold.reduce_mp * X.ZLINE(MgHold.t, MgHold.casttime), my_mp));
			MgShot.da = num;
			MgShot.crystalize_neutral_ratio = MgHold.crystalize_neutral_ratio + (1f - MgHold.crystalize_neutral_ratio) * X.NI(0f, 0.5f, num);
			return true;
		}

		public static void applyWormTrapDamage(PR Pr, int count = -1, bool decline_additional_effect = false)
		{
			if (count < 0)
			{
				for (int i = 0; i < 7; i++)
				{
					MDAT.applyWormTrapDamage(Pr, i, decline_additional_effect);
				}
				return;
			}
			int num = count + 1 - 7;
			NelAttackInfo nelAttackInfo = ((count < 7 || (!Pr.is_alive && Pr.overkill)) ? ((Pr.get_mp() <= Pr.get_maxmp() * 0.2f + 15f) ? (Pr.is_alive ? MDAT.AtkWormTrapDamage : ((X.xors() % 2U == 0U) ? MDAT.AtkWormTrapDamage : MDAT.AtkWormTrap)) : MDAT.AtkWormTrap) : null);
			Pr.applyWormTrapDamage(nelAttackInfo, num, decline_additional_effect);
			if (Pr.isPuzzleManagingMp())
			{
				return;
			}
			if (num == 0 || (num > 7 && nelAttackInfo != null))
			{
				Pr.applyEggPlantDamage(0.2f, PrEggManager.CATEG.WORM, true, (num == 0) ? MDAT.worm_eggplant_ratio : 0.04f);
			}
		}

		public static NelAttackInfo getAtkPressDamage(IPresserBehaviour Press, M2Attackable Mv, AIM aim)
		{
			NelAttackInfo atkPressDamage = MDAT.AtkPressDamage;
			atkPressDamage.hpdmg0 = X.IntC((float)Mv.ratingHpDamageVal((Mv is PR) ? (PUZ.IT.barrier_active ? 0.125f : 0.4f) : 1f));
			atkPressDamage.nodamage_time = (Mv.is_alive ? 240 : 80);
			atkPressDamage.fix_damage = true;
			atkPressDamage._apply_knockback_current = true;
			atkPressDamage.shuffleHpMpDmg(Mv, 1f, 1f, -1000, -1000);
			atkPressDamage.Caster = null;
			atkPressDamage.Beto = (CFG.sp_use_uipic_press_gimmick ? MDAT.BetoPressUiPress : MDAT.BetoPressDefault);
			atkPressDamage.CenterXy(Mv.x + X.Mx(0f, Mv.sizex - 0.25f) * (float)CAim._XD(aim, 1), Mv.y - X.Mx(0f, Mv.sizey - 0.25f) * (float)CAim._YD(aim, 1), 0f);
			return atkPressDamage;
		}

		public static float crystalizeRatio(M2MagicCaster Mv, float r)
		{
			if (PUZ.IT.isPuzzleManagingMp())
			{
				return 0f;
			}
			if (PUZ.IT.barrier_active || !(Mv is M2MoverPr))
			{
				return r;
			}
			if ((Mv as M2MoverPr).enemy_targetted <= 0 && !EnemySummoner.isActiveBorder())
			{
				return 1f;
			}
			return r;
		}

		public const int maxhp_noel_default = 150;

		public const int maxmp_noel_default = 200;

		public const float explode_release_pos = 1.8f;

		public const float pr_mp_hunger_ratio = 0.15f;

		public const float pr_hp_hunger_ratio = 0.2f;

		public const float pr_mp_hunder_chant_speed = 0.25f;

		public static readonly int[] Apr_ep_threshold = new int[] { 500, 700, 900, 950 };

		public const int pr_egg_release_margin = 90;

		public const int pr_egg_item_count_min = 2;

		public const int pr_egg_item_count_rand_range = 3;

		public const int pr_egg_item_count_ratio_range = 4;

		private const int pr_egg_worm_max = 6;

		public const float pr_egg_progress_on_battle = 0.3f;

		public const float pr_egg_item_grade_ratio = 0.3f;

		public const float pr_juice_grade_dlevel_multiple = 1.35f;

		public const float pr_egg_grade_dlevel_multiple = 1.8f;

		public const int one_mana_mp = 4;

		public const float mana_pop_mp_min = 20f;

		public const float mana_pop_mp_max = 30f;

		public const float desire_max = 9f;

		public const float burst_crack_damage = 0f;

		public const float burst_crack_damage_empty = 7f;

		public const int pr_burst_tired_delay = 320;

		public const int HP_CRACK_MAX = 5;

		public const float MP_OVERUSED_REDUCE_TS = 0.06666667f;

		public const float MP_OVERUSED_ALLOC_MAX = 180f;

		public const float MP_OVERUSED_ALLOC_MIN = 100f;

		public const int MP_OVERUSED_ALLOC_MIN_LEVEL = 4;

		public const int DRAIN_LOCK_OVERHOLD_MIN = 50;

		public const int DRAIN_LOCK_OVERHOLD_MAX = 120;

		public const int CHECKJUICE_ABSORB = 4;

		public const int CHECKJUICE_ABSORB_DEAD = 1;

		public const int CHECKJUICE_NORMAL = 3;

		public const int CHECKJUICE_WATER_ADD = 60;

		public const int CHECKJUICE_ORGASM_WATER_ADD = 75;

		public const int CHECKJUICE_WATER_CONSUME = 3;

		public const int CHECKJUICE_WATER_CONSUME_FILL = 50;

		public const float water_in_stomach_reduce_ratio = 1.35f;

		public const float pee_dmg_to_stomach_water_drunk = 0.05f;

		public const float pee_dmg_to_stomach_water_drunk_cache = 0.08f;

		public const int water_drunk_near_level = 72;

		public const int water_drunk_almost_level = 93;

		public const float water_drunk_level2pee_zpow_divide = 10f;

		public const float water_drunk_level2pee_multiple = 70f;

		public const float water_drunk_level2pee_minus = 3.5f;

		public const float PR_GUARD_MAXT = 160f;

		public const float PR_GUARD_RECOVER_MAXT = 100f;

		public const float SPLIT_PUNCH_MAX = 18f;

		public const float SPLIT_PUNCH_SHOTGUN_MAX = 32f;

		public const int burst_mana_absorb_lock = 200;

		public const float burst_execute_reduce_fcnt = 0.0027777778f;

		public const float burst_reduce_speed_in_magic = 0.33f;

		public const int worm_damage_count = 7;

		public const float worm_plant_val = 0.2f;

		private static float worm_eggplant_ratio = 0.15f;

		public const float mana_desire_multiple_default = 9f;

		public const float mana_desire_multiple_enemy_od = 12f;

		public const float mana_desire_multiple_enemy_od_other = 2f;

		public const float mana_desire_multiple_default_enemy = 10f;

		public const float enemy_kill_mana_splash_ratio = 0.25f;

		public const float enemy_kill_mana_splash_min_mp_ratio = 0.125f;

		public const float ep_reduce_punch = 10f;

		public const float ep_reduce_punch_hit = 15f;

		public const float ep_max = 1000f;

		public const float ep_thresh_musturb = 400f;

		public const float ep_auto_reduce_threshold_ratio = 0.3f;

		public const float ep_auto_reduce = 0.14285715f;

		public const int ep_lock_time_normal = 40;

		public const int ep_sage_margin_for_category = 720;

		public const int ep_reduce_while_orgasm = 300;

		public const int ep_reduce_while_orgasm_frustrated = 800;

		public const float ep_dsex_ratio = 3.5f;

		public const int explode_prepare_delay_default = 14;

		public const int dropbomb_set_count = 2;

		public const int dropbomb_exist_max = 4;

		public const int O2_MAX = 100;

		public const float O2_dmg = 0.625f;

		public const float O2_cure_stand = 1f;

		public const float O2_cure_walking = 0.41666666f;

		public const float O2_cure_not_normal = 0.125f;

		public const byte BREATHE_STOP_DELAY = 6;

		public const byte BREATHE_DAMAGE_LOCK = 45;

		public const int WATER_CHOKE_TIME = 200;

		public const float WATER_CHOKE_HP_DMG_RATE = 0.334f;

		public const int WATER_CHOKE_HP_DMG_PUZZ_MANAGING = 5;

		public const float O2_cure_choking_ratio = 0.25f;

		public const float shotgun_dmg_ratio_default = 1.5f;

		public const float water_choke_damage_in_absorb = 0.1f;

		public const int projectile_power_mag = 100;

		public const float EH_long_reach = 1.4f;

		public const int ser_tired_time = 240;

		public const float press_damage_ratio = 0.4f;

		public const float press_damage_puzz_ratio = 0.125f;

		public const float lava_hp_dmg_ratio = 0.125f;

		public const int oazuke_thresh_ep = 700;

		public const float oazuke_occur_ratio = 0.0625f;

		public const int enemy_slipdmg_burned = 6;

		public const float ser_frozen_maxt_0 = 1600f;

		public const float ser_frozen_maxt_2 = 3200f;

		public const float physic_dmg_apply_in_frozen_0 = 1.5f;

		public const float physic_dmg_apply_in_frozen_2 = 2.5f;

		public const int physic_dmg_apply_progress_time = 240;

		public const float bomb_self_explode_ratio = 0.2f;

		public const int FROZEN_GACHA_PROGRESS = 120;

		public const int FROZEN_GACHA_PROGRESS_L2 = 180;

		public static readonly NelAttackInfo AtkWormTrap = new NelAttackInfo
		{
			hpdmg0 = 1,
			mpdmg0 = 4,
			attr = MGATTR.WORM,
			shield_break_ratio = 0f,
			fix_damage = true,
			SerDmg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 5f),
			EpDmg = new EpAtk(21, "worms")
			{
				vagina = 4,
				cli = 3,
				mouth = 6,
				ear = 3,
				urethra = 4,
				anal = 2
			}
		};

		public static readonly NelAttackInfo AtkWormTrapDamage = new NelAttackInfo
		{
			hpdmg0 = 5,
			attr = MGATTR.WORM,
			fix_damage = true,
			SerDmg = MDAT.AtkWormTrap.SerDmg,
			EpDmg = MDAT.AtkWormTrap.EpDmg,
			shield_break_ratio = 0f
		};

		public const int ENEMY_LAVA_HPDMG_MIN = 25;

		public static readonly NelAttackInfo AtkMapLava = new NelAttackInfo
		{
			attr = MGATTR.FIRE,
			nodamage_time = 20,
			ndmg = NDMG.MAPDAMAGE_LAVA,
			hpdmg0 = 25,
			burst_vy = -0.17f,
			shield_break_ratio = 0f,
			SerDmg = new FlagCounter<SER>(4).Add(SER.BURNED, 500f),
			Beto = BetoInfo.Lava
		}.Torn(0.08f, 0.2f);

		public static readonly NelAttackInfo AtkMapLavaForEn = new NelAttackInfo
		{
			attr = MGATTR.FIRE,
			nodamage_time = 200,
			ndmg = NDMG.MAPDAMAGE_LAVA,
			hpdmg0 = 10,
			huttobi_ratio = 2.2f,
			SerDmg = new FlagCounter<SER>(4).Add(SER.BURNED, 80f)
		};

		public static readonly NelAttackInfo AtkSlipDmgForEn = new NelAttackInfo
		{
			attr = MGATTR.FIRE,
			nodamage_time = 0,
			fix_damage = true,
			hpdmg0 = 10
		};

		public static readonly NelAttackInfo AtkParalysis = new NelAttackInfo
		{
			attr = MGATTR.THUNDER,
			nodamage_time = 0,
			huttobi_ratio = -1000f,
			ndmg = NDMG.SER_PARALYSIS,
			hpdmg0 = 0,
			burst_vx = 0.1f,
			shield_break_ratio = 0f
		};

		public static readonly NelAttackInfo AtkCandleTouch = new NelAttackInfo
		{
			hpdmg0 = 11,
			burst_vx = 0.004f,
			nodamage_time = 70,
			shield_break_ratio = 0f,
			ndmg = NDMG.MAPDAMAGE_THUNDER,
			huttobi_ratio = -100f,
			attr = MGATTR.FIRE,
			SerDmg = new FlagCounter<SER>(4).Add(SER.BURNED, 20f),
			Beto = BetoInfo.Lava.Pow(15, false)
		}.Torn(0.03f, 0.08f);

		public const int candle_touch_hitlock_t = 40;

		private static BetoInfo BetoPressDefault = BetoInfo.Blood.Pow(100, false).Level(58f, true).Thread(1, true);

		private static BetoInfo BetoPressUiPress = BetoInfo.Ground.Pow(100, false).Level(40f, true).Thread(1, true);

		private static readonly NelAttackInfo AtkPressDamage = new NelAttackInfo
		{
			nodamage_time = 240,
			fix_damage = true,
			burst_vy = 0.1f,
			attr = MGATTR.PRESS_PENETRATE,
			ndmg = NDMG.PRESSDAMAGE,
			huttobi_ratio = 100f,
			shield_break_ratio = 0f,
			Beto = MDAT.BetoPressDefault
		}.Torn(0.08f, 0.16f);

		public static readonly FlagCounter<SER> SerWhenLayEgg = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 60f);

		public static readonly FlagCounter<SER> SerWhenLayEggFox = new FlagCounter<SER>(4).Add(SER.SEXERCISE, 30f).Add(SER.BURNED, 50f);

		public static readonly EpAtk EpAtkLayEgg = new EpAtk(20, "other")
		{
			uterus = 20
		};

		public static readonly EpAtk EpAtkLayEgg2 = new EpAtk(40, "other")
		{
			uterus = 20
		};

		public static readonly EpAtk EpAtkDebug = new EpAtk(500, "other")
		{
			cli = 20
		};

		public static readonly EpAtk EpAtkMasturbate = new EpAtk(10, "masturbate")
		{
			cli = 10,
			vagina = 2
		};

		public static readonly EpAtk EpAtkMasturbate2 = new EpAtk(10, "masturbate")
		{
			cli = 10,
			vagina = 2,
			breast = 4
		};

		public static readonly EpAtk EpAtkMasturbate_Road = new EpAtk(10, "masturbate")
		{
			cli = 10,
			vagina = 4
		};

		public static readonly EpAtk EpAtkMasturbate_Road2 = new EpAtk(10, "masturbate")
		{
			cli = 10,
			vagina = 6
		};

		public static readonly NelAttackInfo AtkMasturbate_road = new NelAttackInfo
		{
			attr = MGATTR.ACME,
			mpdmg0 = 2,
			mpdmg_current = 2,
			shield_break_ratio = 0f
		};
	}
}
