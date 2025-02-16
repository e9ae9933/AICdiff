using System;
using System.Collections.Generic;
using Better;
using m2d;
using nel.gm;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class EpManager : IGachaListener
	{
		public EpManager(PR _Pr)
		{
			this.Pr = _Pr;
			this.Cutin = new EpOrgasmCutin(this.Pr);
			this.Atotal_exp = new byte[11];
			this.Atotal_exp_default = new byte[11];
			this.Atotal_exp_default[0] = 15;
			this.Atotal_exp_default[1] = 4;
			this.Atotal_exp_default[3] = 1;
			this.Atotal_exp_default[8] = 15;
			this.Atotal_orgasmed = new int[11];
			this.Ases_orgasmable = new float[12];
			this.Aorgasm_locked_stack = new List<EpManager.OrgasmInfo>(3);
			this.SituCon = new EpSituation();
			this.EhBounce = new EffectHandler<EffectItem>(4, null);
			this.PeZoom = new EffectHandlerPE(1);
			this.PeSlow = new EffectHandlerPE(1);
			this.Suppressor = new EpSuppressor();
			this.At_announce = new float[2];
			this.newGame();
			this.MtrHeartPattern = MTR.newMtr("Hachan/ShaderGDTPatternEpBg");
			this.MtrHeartPattern.SetTexture("_MainTex", MTRX.MIicon.Tx);
			this.FD_drawEffect = new FnEffectRun(this.drawEffect);
		}

		public void newGame()
		{
			X.ALL0(this.Atotal_exp);
			X.ALL0(this.Atotal_orgasmed);
			X.ALL0(this.Ases_orgasmable);
			this.Osituation_orgasmed = new BDic<string, int>();
			this.Oegg_layed = new BDic<PrEggManager.CATEG, int>();
			this.pee_count = 0;
			this.orgasm_individual_count = 0;
			this.t_orgasm_cutin_tikatika = 0f;
			this.SituCon.newGame();
			this.splash_lock = (this.go_unlock_thresh = false);
			this.Cutin.Clear();
			this.flushCurrentBattle();
			this.bt_exp_added = (this.bt_orgasm = (this.bt_applied = (this.bt_corrupt_acmemist = (EPCATEG_BITS)0)));
			this.orgasm_oazuke = (this.attachable_ser_oazuke = false);
			this.lead_to_orgasm = (EPCATEG_BITS)0;
			this.cure_ep_after_orgasm = (this.cure_ep_after_orgasm_one = 0f);
			this.t_oazuke = -1f;
			this.ep = (float)this.Pr.ep;
			this.t_ptc = (float)(this.t_ptc_ui = -1);
			this.t_corrupt_gacha_mist = 0f;
			this.breath_key_ = null;
			this.t_crack_cure = (this.gacha_hit_stocked = 0f);
			this.dsex_orgasm = (this.dsex_orgasm_pre = 0);
			this.multiple_orgasm = 0;
			this.crack_cure_count = (this.crack_cure_once = 0);
			this.t_sage = (this.t_lock = (this.t_lock_orgasmlocked = 0f));
			this.hold_orgasm_after_t = 0f;
			this.At_announce[0] = (this.At_announce[1] = 0f);
			this.Aorgasm_locked_stack.Clear();
			if (this.EfUi != null)
			{
				this.EfUi.destruct();
				this.EfUi = null;
			}
			this.EhBounce.release(false);
			this.PeZoom.release(false);
			this.PeSlow.release(false);
			this.releaseEffect();
		}

		public void destruct()
		{
			IN.DestroyOne(this.MtrHeartPattern);
			this.MtrHeartPattern = null;
		}

		public void initS()
		{
			this.t_orgasm_cutin_tikatika = 0f;
		}

		public void flushCurrentSession()
		{
			for (int i = 0; i < 11; i++)
			{
				this.Atotal_exp[i] = X.Mn(20, this.Atotal_exp[i]);
			}
			this.flushCurrentBattle();
			this.Suppressor.Clear();
			this.splash_lock = false;
			this.cure_ep_after_orgasm = 0f;
			this.bt_exp_added = (this.bt_orgasm = (this.bt_applied = (this.bt_corrupt_acmemist = (EPCATEG_BITS)0)));
			this.orgasm_oazuke = false;
		}

		public void flushCurrentBattle()
		{
			if (this.Pr.Ser == null)
			{
				return;
			}
			if (this.Pr.Ser.has(SER.FRUSTRATED))
			{
				this.t_oazuke = 0f;
			}
			this.dsex_orgasm = 0;
			this.splash_lock = false;
			this.hold_orgasm_after_t = 0f;
			this.bt_corrupt_acmemist = (EPCATEG_BITS)0;
			this.lead_to_orgasm = (EPCATEG_BITS)0;
			if (this.ep >= 950f)
			{
				this.ep = 950f;
				this.fineCounter();
			}
			this.recalcOrgasmable();
		}

		public void recalcOrgasmable()
		{
			X.ALL0(this.Ases_orgasmable);
			int num = 0;
			float num2 = 1f;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 11; j++)
				{
					float num3 = (float)(this.Atotal_exp[j] + this.Atotal_exp_default[j]);
					if (i == 0)
					{
						num += (int)X.Mn(num3, 20f);
					}
					else
					{
						num3 *= num2;
						this.Ases_orgasmable[j] = 1f - Mathf.Pow(1f - X.NI3(0.08f, 0.33f, 0.6f, X.ZLINE(num3, 40f)), 0.06666667f);
						if (j != 8)
						{
							this.Ases_orgasmable[11] = X.Mx(this.Ases_orgasmable[j], this.Ases_orgasmable[11]);
						}
					}
				}
				if (i == 0)
				{
					num2 = ((num > 60) ? (60f / (float)num) : 1f);
				}
			}
		}

		public int getNoelJuiceQualityAdd()
		{
			return X.xors(X.Mx(1, X.IntC(this.Ases_orgasmable[6] + this.Ases_orgasmable[3] + this.Ases_orgasmable[7] + this.Ases_orgasmable[9])));
		}

		public int getNoelEggQualityAdd()
		{
			return X.xors(X.Mx(1, X.IntC(this.Ases_orgasmable[8] + this.Ases_orgasmable[2] + this.Ases_orgasmable[7])));
		}

		private void checkOazuke(bool over_coming_ep)
		{
			if (!SCN.occurableOazuke() || !this.attachable_ser_oazuke)
			{
				return;
			}
			float num = 0f;
			for (int i = 0; i < 11; i++)
			{
				if ((this.bt_exp_added & (EPCATEG_BITS)(1 << i)) != (EPCATEG_BITS)0)
				{
					if (over_coming_ep)
					{
						num += ((this.Atotal_exp[i] - 8 <= 0) ? 0.015f : 0.2f);
					}
					else
					{
						num += X.NI(0.03f, 0.08f, X.Mx(0f, (float)(this.Atotal_exp[i] - 8) / 12f));
					}
				}
			}
			if (this.ep >= 950f && this.Pr.Ser.has(SER.FORBIDDEN_ORGASM))
			{
				num += 0.25f;
			}
			num = X.Mn(1f, num);
			if (this.Suppressor.Count > 0)
			{
				num = X.ZLINE(num) * X.NI(0.33f, 0f, X.ZLINE((float)this.Suppressor.Count, 4f));
			}
			if (this.Pr.isDoubleSexercise())
			{
				num += 0.25f;
			}
			if (X.XORSP() < num)
			{
				this.Pr.Ser.Add(SER.FRUSTRATED, -1, 99, false);
				this.attachable_ser_oazuke = false;
				this.t_oazuke = 0f;
			}
		}

		private int getOrgasmAbsorbTapCount()
		{
			float num = (float)this.Pr.Ser.getLevel(SER.ORGASM_AFTER);
			if (num >= 5f)
			{
				num = (num - 5f) * 0.5f + 5f;
				if (num >= 8f)
				{
					num = (num - 8f) * 0.5f + 8f;
				}
				if (num >= 12f)
				{
					num = (num - 12f) * 0.5f + 12f;
				}
			}
			return (int)X.Mn((float)DIFF.orgasm_tap_max_count, (float)(5 + X.Mx(0, this.Suppressor.Sum() - 1) * 2) * ((float)(this.orgasm_oazuke ? 3 : 1) + num));
		}

		public bool canAbsorbContinue()
		{
			return this.t_sage > 0f && Map2d.can_handle && this.Pr.isLayingEggOrOrgasm() && !this.Pr.EggCon.isLaying();
		}

		public void absorbFinished(bool abort)
		{
			if (!abort)
			{
				this.quitOrgasmAfterTime();
				this.changedFine();
			}
			else if (this.AbsorbOrgasm != null)
			{
				if (this.canAbsorbContinue())
				{
					this.gacha_hit_stocked = X.Mx(this.gacha_hit_stocked, this.AbsorbOrgasm.get_Gacha().getCount(true));
				}
				else
				{
					this.gacha_hit_stocked = 0f;
				}
			}
			this.AbsorbOrgasm = null;
		}

		private float calcNormalEpErection(EPCATEG target, float tval)
		{
			float num = (float)(this.Atotal_exp[(int)target] + this.Atotal_exp_default[(int)target]);
			num += (this.Pr.Ser.has(SER.SEXERCISE) ? 10f : 0f);
			int num2 = this.Suppressor.Sum();
			if (num2 == 0)
			{
				tval *= X.NI3(0.5f, 1f, 2f, X.ZLINE(num, 40f));
			}
			else
			{
				tval *= X.NI3(1f, 1f, 2f, X.ZLINE(num, 40f)) * X.NI(1f, 0.125f, X.ZLINE((float)(num2 - 3), 12f));
			}
			return tval * this.Pr.Ser.EpApplyRatio();
		}

		private float getLeadToOrgasmRatio(EPCATEG target)
		{
			float num = this.Ases_orgasmable[(int)target];
			int num2 = this.Suppressor.Sum();
			if (num2 != 0 || this.t_sage > 0f)
			{
				num = X.NI(num, this.Ases_orgasmable[11], X.ZLINE((float)(num2 - 2), 8f));
			}
			num += ((this.Pr.Ser.has(SER.SEXERCISE) || (!this.Pr.is_alive && X.XORSP() < X.NI(0.33f, 1f, X.ZLINE((float)this.Suppressor.Count, 7f)))) ? X.NI(0.08f, 0.2f, num) : 0f);
			num += (this.Pr.isDoubleSexercise() ? 0.4f : (this.Pr.Ser.has(SER.FRUSTRATED) ? 0.125f : 0f));
			float orgasmableRatio = this.Suppressor.getOrgasmableRatio(target);
			return num * ((orgasmableRatio == 1f) ? 1f : (this.Suppressor.getOrgasmableRatio(target) - 0.004f));
		}

		public int getExperienceCount(EPCATEG categ)
		{
			return (int)this.Atotal_exp[(int)categ];
		}

		public float getExperienceLevel(EPCATEG categ)
		{
			return X.ZLINE((float)this.Atotal_exp[(int)categ], 20f);
		}

		private bool checkMultipleOrgasm(EpAtk Atk)
		{
			return Atk.multiple_orgasm != 0f && X.XORSP() < Atk.multiple_orgasm * X.Pow(1f - X.ZLINE((float)(this.multiple_orgasm - 1), 9f) * 0.5f - X.ZLINE((float)(this.multiple_orgasm - 10), 40f) * 0.48f, 2);
		}

		public bool applyEpDamage(EpAtk Atk, M2Attackable AttackedBy = null, EPCATEG_BITS bits = EPCATEG_BITS._ALL, float lead_orgasm_multiple = 1f, bool can_execute_orgasm = true)
		{
			bool target_max = Atk.target_max != 0;
			int num = 0;
			EPCATEG epcateg;
			if (!target_max)
			{
				epcateg = EPCATEG.OTHER;
				num = 1024;
			}
			else
			{
				epcateg = this.getTarget(Atk, bits, out num, true);
			}
			bool flag = false;
			if (this.t_sage >= 0f || this.checkMultipleOrgasm(Atk))
			{
				float num2 = (float)Atk.val;
				bool flag2 = false;
				if (Atk.situation_key == "masturbate" && EnemySummoner.isActiveBorder())
				{
					this.attachable_ser_oazuke = false;
				}
				if (this.go_unlock_thresh)
				{
					num2 *= X.NIXP(2f, 4.5f);
				}
				else if (Atk.situation_key != "masturbate")
				{
					if (this.t_sage >= 0f && !this.attachable_ser_oazuke && X.XORSP() <= 0.2f + (float)(1 + this.Pr.Ser.getLevel(SER.SEXERCISE)) * 0.2f)
					{
						this.attachable_ser_oazuke = true;
					}
					if (this.ep >= 950f)
					{
						if ((this.lead_to_orgasm & (EPCATEG_BITS)(1 << (int)epcateg)) == (EPCATEG_BITS)0)
						{
							float num3 = this.getLeadToOrgasmRatio(epcateg);
							num3 = X.Scr(num3, (this.t_sage < 0f) ? 0.6f : ((num3 < 0.02f) ? (0.02f * (float)(this.Pr.Ser.getLevel(SER.SEXERCISE) + 1)) : 0f));
							if (this.t_lock_orgasmlocked > 0f)
							{
								num3 *= 0.25f;
							}
							if (X.XORSP() < num3 * lead_orgasm_multiple)
							{
								if (this.lead_to_orgasm == (EPCATEG_BITS)0 && !this.Pr.isOrgasmLocked(false))
								{
									UILog.Instance.AddAlertTX("EP_preparing_orgasm", UILogRow.TYPE.ALERT_EP2);
									this.bt_corrupt_acmemist = (EPCATEG_BITS)0;
								}
								this.lead_to_orgasm |= (EPCATEG_BITS)(1 << (int)epcateg);
							}
						}
						if (this.lead_to_orgasm != (EPCATEG_BITS)0)
						{
							if (this.Pr.Ser.has(SER.FORBIDDEN_ORGASM) && this.t_sage >= 0f)
							{
								this.t_oazuke = -1f;
								this.t_lock = X.Mx(this.t_lock, 40f);
								num2 = 0f;
								this.checkOazuke(true);
								this.corruptLog("corrupt_forbidden", 0.08f, false);
							}
							else
							{
								num2 = 7f;
								if ((float)this.Pr.ep + num2 >= 1000f && (this.lead_to_orgasm & (EPCATEG_BITS)num) == (EPCATEG_BITS)0)
								{
									num2 = X.MMX(0f, num2, (float)(999 - this.Pr.ep));
								}
							}
						}
						else
						{
							num2 = 0f;
							this.t_lock = X.Mx(this.t_lock, 40f);
						}
					}
					else
					{
						num2 = this.calcNormalEpErection(epcateg, num2);
						num2 = X.MMX(0f, num2, (float)(950 - this.Pr.ep));
					}
				}
				num2 = (float)X.IntR(num2);
				if (num2 > 0f)
				{
					this.Pr.ep += (int)((this.t_sage < 0f) ? 1000f : num2);
					if (!can_execute_orgasm)
					{
						this.Pr.ep = X.Mn(this.Pr.ep, 999);
					}
					if (this.bt_corrupt_acmemist != (EPCATEG_BITS)0 && X.XORSP() < 0.05f)
					{
						this.bt_corrupt_acmemist = (EPCATEG_BITS)0;
					}
					this.t_oazuke = 0f;
					if (this.Pr.isAbsorbState() && this.AbsorbOrgasm == null)
					{
						bool flag3 = false;
						if (!flag3 && this.Pr.Ser.getLevel(SER.ORGASM_AFTER) >= 1 && (this.bt_orgasm & (EPCATEG_BITS)(1 << (int)epcateg)) != (EPCATEG_BITS)0)
						{
							this.corruptLog("corrupt_orgasm_after", 0.16f, this.Pr.getAbsorbContainer().CorruptGacha(1f, true));
							flag3 = true;
						}
						if (this.t_sage >= 0f)
						{
							if (this.Pr.Ser.has(SER.FRUSTRATED))
							{
								this.Pr.Ser.Add(SER.FRUSTRATED, (this.Pr.ep >= 1000) ? (-1) : 100, 99, false);
								if ((this.bt_exp_added & (EPCATEG_BITS)(1 << (int)epcateg)) != (EPCATEG_BITS)0)
								{
									this.corruptLog("corrupt_frustrated", 0.16f, this.Pr.getAbsorbContainer().CorruptGacha(5f, true));
									flag3 = true;
								}
							}
							if (!flag3 && this.t_corrupt_gacha_mist > 0f && (this.bt_corrupt_acmemist & (EPCATEG_BITS)(1 << (int)epcateg)) != (EPCATEG_BITS)0)
							{
								this.corruptLog("corrupt_acme_mist", 0.16f, false);
								this.bt_corrupt_acmemist |= (EPCATEG_BITS)(1 << (int)epcateg);
							}
						}
					}
					if (this.Pr.ep >= 1000)
					{
						if (this.Pr.isOrgasmLocked(false))
						{
							this.Aorgasm_locked_stack.Add(new EpManager.OrgasmInfo
							{
								categ = epcateg,
								situation = Atk.situation_key
							});
							this.Pr.ep = 850;
							this.lead_to_orgasm = (EPCATEG_BITS)0;
							this.t_lock_orgasmlocked = 250f;
							this.Pr.Ser.Add(SER.ORGASM_STACK, 30, 99, false);
							this.fineCounter();
						}
						else
						{
							this.initOrgasm(Atk, false);
							flag2 = true;
							if (AttackedBy is NelEnemy)
							{
								UiBenchMenu.enemy_orgasm = true;
							}
							else if (this.Pr.isBenchState())
							{
								UiBenchMenu.orgasm_onemore = true;
							}
						}
					}
					else
					{
						this.fineCounter();
					}
					if (this.EfUi != null && !this.go_unlock_thresh)
					{
						this.EfUi.time = (int)this.Pr.Mp.floort + ((this.t_sage != 0f) ? 12 : 0);
					}
					flag = true;
				}
				if (CFG.ui_sensitive_description > 0)
				{
					float num4 = (float)CFG.ui_sensitive_description / 10f;
					if (!flag2 && X.XORSP() < X.NI(X.NI(0.04f, 0.75f, num4 * num4), X.NI(0.04f, 0.75f, 1f - X.Pow(1f - num4, 2)), X.ZLINE(this.ep - 350f, 650f)))
					{
						this.setAlert(false, epcateg.ToString().ToLower(), Atk.situation_key.ToLower());
					}
				}
			}
			EPCATEG_BITS epcateg_BITS = (EPCATEG_BITS)(1 << (int)epcateg);
			this.bt_applied |= epcateg_BITS;
			if ((this.bt_exp_added & epcateg_BITS) == (EPCATEG_BITS)0 && (this.Suppressor.Count > 0 || this.ep >= (float)this.exp_add_ep) && this.Atotal_exp[(int)epcateg] < 40)
			{
				byte[] atotal_exp = this.Atotal_exp;
				EPCATEG epcateg2 = epcateg;
				atotal_exp[(int)epcateg2] = atotal_exp[(int)epcateg2] + 1;
				this.bt_exp_added |= epcateg_BITS;
			}
			return flag;
		}

		public bool progressOrgasmStack(bool is_first, out bool orgasmed)
		{
			orgasmed = false;
			if (this.Aorgasm_locked_stack.Count == 0)
			{
				return false;
			}
			EpManager.OrgasmInfo orgasmInfo = this.Aorgasm_locked_stack[0];
			EPCATEG_BITS epcateg_BITS = (EPCATEG_BITS)(1 << (int)orgasmInfo.categ);
			this.lead_to_orgasm |= epcateg_BITS;
			this.Pr.ep += 5;
			if (this.Pr.ep >= 1000)
			{
				orgasmed = true;
				this.initOrgasm(orgasmInfo.situation, epcateg_BITS, false);
				this.Pr.ep = 950 - (int)X.NIXP(0f, 40f);
				this.Aorgasm_locked_stack.RemoveAt(0);
			}
			this.fineCounter();
			return true;
		}

		private EPCATEG getTarget(EpAtk Atk, EPCATEG_BITS bits, out int apply_bits, bool consider_cfg_boost = true)
		{
			apply_bits = (int)(Atk.target_bits & bits);
			EPCATEG epcateg = EPCATEG._ALL_TARG;
			if (apply_bits == 0)
			{
				apply_bits = (int)Atk.target_bits;
			}
			float num = 0f;
			for (int i = 0; i < 11; i++)
			{
				if ((apply_bits & (1 << i)) != 0)
				{
					num += (float)Atk.Get(i) * (consider_cfg_boost ? CFGSP.getEpApplyRatio((EPCATEG)i) : 1f);
				}
			}
			if (num > 0f)
			{
				float num2 = X.XORSP() * num;
				EPCATEG epcateg2 = EPCATEG._ALL_TARG;
				for (int j = 0; j < 11; j++)
				{
					if ((apply_bits & (1 << j)) != 0)
					{
						epcateg2 = (EPCATEG)j;
						float num3 = (float)Atk.Get(j) * (consider_cfg_boost ? CFGSP.getEpApplyRatio((EPCATEG)j) : 1f);
						if (num2 < num3)
						{
							epcateg = epcateg2;
							break;
						}
						num2 -= num3;
					}
				}
				if (epcateg == EPCATEG._ALL_TARG)
				{
					epcateg = ((epcateg2 != EPCATEG._ALL_TARG) ? epcateg2 : Atk.getRandom());
				}
				return epcateg;
			}
			if (consider_cfg_boost)
			{
				return this.getTarget(Atk, EPCATEG_BITS._ALL, out apply_bits, false);
			}
			return Atk.getRandom();
		}

		public void corruptLog(string tx_key, float ratio, bool force_flag = false)
		{
			if (force_flag)
			{
				ratio = 1f;
			}
			if (X.XORSP() < ratio * (float)CFG.ui_sensitive_description / 10f)
			{
				UILog.Instance.AddAlertTX(tx_key, UILogRow.TYPE.ALERT_EP2);
			}
		}

		public void fineCounter()
		{
			float num = this.t_lock;
			if (this.ep > (float)this.Pr.ep)
			{
				this.t_lock = X.Mn(0f, this.t_lock);
			}
			else
			{
				if (this.ep >= (float)this.Pr.ep)
				{
					return;
				}
				this.t_lock = X.Mx(this.t_lock, 40f);
			}
			int num2 = (int)(this.t_lock - num);
			if (this.Pr.Ser.has(SER.ORGASM_AFTER) && num2 > 0)
			{
				this.Pr.Ser.Add(SER.ORGASM_AFTER, num2, 0, false);
			}
			this.ep = (float)this.Pr.ep;
			this.Pr.Ser.checkSer();
			this.fineEffect(false);
		}

		public int exp_add_ep
		{
			get
			{
				if (!this.Pr.Ser.has(SER.SEXERCISE))
				{
					return 850;
				}
				return 700;
			}
		}

		public void addEggLayCount(PrEggManager.CATEG categ, int cnt = 1)
		{
			this.Oegg_layed[categ] = X.Get<PrEggManager.CATEG, int>(this.Oegg_layed, categ, 0) + cnt;
		}

		public void initOrgasmCumming(EPCATEG_BITS target_bits, bool adding = true)
		{
			int num = X.bit_count((uint)target_bits);
			EPCATEG_BITS epcateg_BITS = ((num <= 1) ? target_bits : ((EPCATEG_BITS)(1 << X.bit_on_index((uint)target_bits, X.xors(num)))));
			this.lead_to_orgasm = (adding ? (this.lead_to_orgasm | epcateg_BITS) : epcateg_BITS);
		}

		public EPCATEG addMasturbateCountImmediate(string situation_key, int multiple_orgasm = 1)
		{
			EPCATEG epcateg;
			if (this.lead_to_orgasm == (EPCATEG_BITS)0)
			{
				epcateg = EPCATEG.OTHER;
			}
			else
			{
				int num = X.bit_count((uint)this.lead_to_orgasm);
				int num2 = X.bit_on_index((uint)this.lead_to_orgasm, X.xors(num));
				epcateg = (EPCATEG)((num2 < 0) ? 10 : num2);
			}
			string text = EpSituation.situ_write_replace(situation_key.ToUpper());
			this.lead_to_orgasm = (EPCATEG_BITS)0;
			if (REG.match(text, REG.RegSuffixNumber))
			{
				text = REG.leftContext;
			}
			if (text == "OTHER")
			{
				text = this.SituCon.getRandomKey();
			}
			this.SituCon.addTempSituation(text, multiple_orgasm, false);
			if (TX.valid(text))
			{
				this.Osituation_orgasmed[text] = X.Get<string, int>(this.Osituation_orgasmed, text, 0) + 1;
			}
			return epcateg;
		}

		public void initOrgasm(EpAtk Atk, bool skip_effect = false)
		{
			this.initOrgasm(Atk.situation_key, Atk.target_bits, skip_effect);
		}

		public void initOrgasm(string situation_key, EPCATEG_BITS target_bits, bool skip_effect = false)
		{
			this.Pr.SttInjector.need_check_on_runpre = true;
			bool flag = situation_key == "masturbate";
			if (flag)
			{
				this.initOrgasmCumming(target_bits, false);
			}
			bool flag2 = this.t_sage < 0f;
			EPCATEG epcateg = this.addMasturbateCountImmediate(situation_key, this.multiple_orgasm + 1);
			if (this.multiple_orgasm == 0)
			{
				this.orgasm_individual_count++;
			}
			bool flag3 = EnemySummoner.isActiveBorder();
			if (flag3)
			{
				this.splash_lock = false;
			}
			this.Suppressor.Has(epcateg);
			this.cure_ep_after_orgasm = (float)(this.Pr.Ser.has(SER.FRUSTRATED) ? 800 : 300);
			this.cure_ep_after_orgasm_one = this.cure_ep_after_orgasm / 600f;
			this.bt_orgasm |= this.bt_exp_added;
			this.bt_exp_added = (this.bt_applied = (EPCATEG_BITS)0);
			this.bt_corrupt_acmemist = (EPCATEG_BITS)0;
			this.Atotal_orgasmed[(int)epcateg]++;
			if (this.multiple_orgasm == 0)
			{
				this.Atotal_exp[(int)epcateg] = (byte)X.Mn(40, (int)(this.Atotal_exp[(int)epcateg] + ((this.Atotal_exp[(int)epcateg] >= 20) ? 2 : 1)));
			}
			this.Suppressor.Orgasmed(epcateg, 540);
			this.t_oazuke = 0f;
			this.attachable_ser_oazuke = false;
			this.recalcOrgasmable();
			int count = this.Suppressor.Count;
			this.Pr.Ser.Add(SER.ORGASM_INITIALIZE, -1, 99, false);
			this.Pr.Ser.Cure(SER.ORGASM_AFTER);
			this.Pr.getAbsorbContainer().CorruptGacha(5f, true);
			bool flag4 = this.orgasm_oazuke;
			if (this.Pr.isDoubleSexercise())
			{
				this.dsex_orgasm++;
				flag4 = true;
			}
			if (flag4 && this.hold_orgasm_after_t == 0f)
			{
				this.hold_orgasm_after_t = 520f;
			}
			this.dsex_orgasm_pre = this.dsex_orgasm;
			if (flag)
			{
				if (this.Pr.Ser.has(SER.FRUSTRATED))
				{
					this.Pr.Ser.Cure(SER.FRUSTRATED);
					this.orgasm_oazuke = false;
				}
				if (this.dsex_orgasm > 0)
				{
					this.dsex_orgasm /= 2;
				}
			}
			this.At_announce[0] = 0f;
			this.At_announce[1] = X.Mx(300f, this.At_announce[1]);
			this.t_orgasm_cutin_tikatika += 1f;
			this.t_sage = -this.calcOrgasmInitDelayTime();
			if (this.go_unlock_thresh)
			{
				this.ep = (float)(this.Pr.ep = 750);
			}
			else if (flag && this.Pr.enemy_targetted == 0)
			{
				this.ep = (float)(this.Pr.ep = 0);
			}
			else
			{
				this.ep = (float)(this.Pr.ep = this.calcOrgasmTurnEp());
			}
			if (!this.splash_lock)
			{
				this.crack_cure_count += (int)X.Mx(1f, (float)this.calcOrgasmCureCrack(epcateg) * (flag ? 0.33f : 1f));
				this.crack_cure_once = X.Mx(1, X.Mn(this.crack_cure_count / 4, flag ? 1 : 2));
			}
			this.setAlert(true, epcateg.ToString().ToLower(), situation_key.ToLower());
			this.fineEffect(true);
			if (!skip_effect)
			{
				PostEffect it = PostEffect.IT;
				this.Pr.VO.playOrgasm(flag);
				if (this.Pr.UP.isActive())
				{
					this.PeZoom.release(false);
					this.EhBounce.release(false);
					if (!this.go_unlock_thresh)
					{
						float num = 1.125f;
						if (this.multiple_orgasm == 0)
						{
							if (!this.Pr.LockCntOccur.isLocked(PR.OCCUR.HITSTOP_ORGASM))
							{
								this.PeSlow.Set(it.setSlow(X.NI(80, 20, X.ZSIN((float)(count - 2), 3f)), X.Scr(X.NI(0.22f, 0.6f, X.ZSIN((float)(count - 1), 3f)), (this.Aorgasm_locked_stack.Count > 0) ? 0.5f : 0f), 0));
								this.Pr.LockCntOccur.Add(PR.OCCUR.HITSTOP_ORGASM, 240f);
							}
							else
							{
								this.PeSlow.Set(it.setSlow(28f, 0.88f, 0));
							}
							TransEffecterItem transEffecterItem = this.Pr.M2D.Cam.TeCon.setBounceZoomIn(num, 40f, 0);
							PostEffect.IT.addTimeFixedEffect(transEffecterItem, 0.5f);
							this.EhBounce.Set(transEffecterItem);
							this.Pr.LockCntOccur.Add(PR.OCCUR.NOELJUICE_ZOOMIN, 140f);
						}
						else
						{
							this.PeSlow.deactivate(true);
						}
						PostEffectItem postEffectItem = it.setPE(POSTM.ZOOM2_EATEN, 50f, 1f, 0);
						this.PeZoom.Set(postEffectItem);
						it.addTimeFixedEffect(postEffectItem, 0.6f);
						it.addTimeFixedEffect(it.setPEabsorbed(POSTM.BGM_LOWER, 10f, 300f, 1f, 0), 1f);
						it.addTimeFixedEffect(it.setPEabsorbed(POSTM.SND_VOLUME_REDUCE, 10f, 200f, 0.75f, 0), 1f);
						if (flag)
						{
							it.addTimeFixedEffect(this.Pr.TeCon.setQuakeSinH(4f, 50, 19f, 1f, 0), 0.7f);
						}
						else
						{
							it.addTimeFixedEffect(this.Pr.TeCon.setQuake(4f, 20, 1f, 0), 1f);
							it.addTimeFixedEffect(this.Pr.TeCon.setQuakeSinH(5f, 90, 24f, 1f, 0), 0.7f);
						}
						this.Pr.defineParticlePreVariableVagina();
						this.Pr.PtcVar("is_down", (double)(this.Pr.isPoseDown(false) ? 1 : 0));
						this.Pr.PtcHld.PtcSTTimeFixed("ep_orgasm", 0.85f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
					}
					EffectNel effect = UIBase.Instance.getEffect();
					float num2 = (UIBase.Instance.event_center_uipic ? 1f : UIBase.uipic_mpf);
					bool flag5;
					if (num2 >= 0f)
					{
						flag5 = this.Pr.drawx >= this.Pr.M2D.Cam.x + 32f;
					}
					else
					{
						flag5 = this.Pr.drawx > this.Pr.M2D.Cam.x - 32f;
					}
					float num3 = num2 * (170f + ((!UIBase.Instance.showing_pict) ? 340f : 0f) + (IN.w - 340f) / 2f) / UIPicture.Instance.PosSyncSlide.z + (float)X.MPF(!flag5) * 0.2f * IN.w;
					effect.PtcSTsetVar("cx", (double)num3).PtcSTsetVar("dwh", 210.0).PtcST("ep_orgasm_ui", null, PTCThread.StFollow.NO_FOLLOW, null);
					if (!flag)
					{
						this.Cutin.setE(effect, num3, 0f, flag5, this.t_orgasm_cutin_tikatika >= (this.Pr.is_alive ? 20f : 8f) && X.XORSP() < X.NIL(1f, 0.5f, (float)this.multiple_orgasm, 8f));
					}
					else
					{
						this.Pr.UP.TeCon.setQuakeSinH(4f, 70, 22f, 1f, 0);
						this.Pr.UP.applyMasturbate(0.75f, 4f, 80f);
					}
				}
				SND.Ui.play("orgasm", true);
			}
			if (!this.splash_lock)
			{
				if (situation_key == "masturbate" && this.Pr.NM2D.IMNG.hasEmptyBottle())
				{
					this.Pr.JuiceCon.executeSplashNoelJuice(true, false, 1, false, false, flag2, false);
				}
				else
				{
					this.Pr.JuiceCon.checkNoelJuiceOrgasm(X.ZLINE((float)(count - 2), 5f) * 0.5f + X.ZLINE((float)this.multiple_orgasm, 3f) * 0.8f, flag2);
				}
			}
			if (!flag3)
			{
				this.splash_lock = true;
			}
			this.Pr.Ser.checkSer();
			this.multiple_orgasm++;
			if (skip_effect || this.go_unlock_thresh)
			{
				this.quitOrgasmInitTime();
			}
		}

		private float calcOrgasmInitDelayTime()
		{
			return X.NI(120, 80, X.ZSIN((float)(this.Suppressor.Count - 1), 6f));
		}

		private int calcOrgasmTurnEp()
		{
			return 500;
		}

		private int calcOrgasmCureCrack(EPCATEG maincateg)
		{
			float num = X.NI(8f, 0.5f, X.ZLINE((float)this.Suppressor.Sum(), 8f));
			return X.Mx(1, X.IntR(num * X.NI(0.5f, 1f, this.getExperienceLevel(maincateg))));
		}

		public void cureOrgasmAfter()
		{
			if (this.t_sage < 0f)
			{
				this.t_sage = 1f;
				this.Pr.Ser.Cure(SER.ORGASM_INITIALIZE);
				this.multiple_orgasm = 0;
			}
			if (this.t_crack_cure != 0f && this.crack_cure_count != 0)
			{
				this.Pr.GaugeBrk.Cure(this.crack_cure_count, true);
				if (this.t_crack_cure < 0f)
				{
					UILog.Instance.AddAlertTX("Alert_cure_crack", UILogRow.TYPE.ALERT);
				}
				this.t_crack_cure = 0f;
			}
			if (this.t_sage > 0f)
			{
				this.t_sage = (this.t_lock = (this.t_lock_orgasmlocked = 0f));
				this.flushCurrentBattle();
				this.Pr.Ser.Cure(SER.FRUSTRATED);
				this.Pr.Ser.Cure(SER.ORGASM_INITIALIZE);
				if (!this.Pr.Ser.has(SER.ORGASM_AFTER))
				{
					this.Pr.Ser.Add(SER.ORGASM_AFTER, -1, 99, false);
				}
				this.Suppressor.Clear();
				this.fineEffect(true);
				this.cure_ep_after_orgasm = 0f;
			}
			this.PeSlow.deactivate(false);
			this.PeZoom.deactivate(false);
		}

		private void quitOrgasmInitTime()
		{
			this.t_sage = 1f;
			this.fineEffect(true);
			this.Pr.Ser.Cure(SER.ORGASM_INITIALIZE);
			if (this.Pr.Ser.has(SER.FRUSTRATED))
			{
				this.orgasm_oazuke = true;
				this.Pr.Ser.Cure(SER.FRUSTRATED);
			}
			int num = -1;
			this.Pr.Ser.Add(SER.ORGASM_AFTER, num, this.multiple_orgasm - 1 + (this.orgasm_oazuke ? 1 : 0) + ((this.dsex_orgasm_pre > 0) ? X.Mn(this.dsex_orgasm_pre + 1, 9) : 0), false);
			if (this.multiple_orgasm > 1)
			{
				UILog.Instance.AddAlert(TX.GetA("EP_multiple_orgasm", this.multiple_orgasm.ToString() ?? ""), UILogRow.TYPE.ALERT_EP);
			}
			this.multiple_orgasm = 0;
			if (!this.M2D.FlgWarpEventNotInjectable.isActive())
			{
				this.SituCon.flushLastExSituationTemp();
			}
			this.PeSlow.deactivate(true);
		}

		private void quitOrgasmAfterTime()
		{
			this.PeZoom.deactivate(false);
			this.cure_ep_after_orgasm = (this.cure_ep_after_orgasm_one = 0f);
			this.orgasm_oazuke = false;
			this.t_sage = 0f;
			this.gacha_hit_stocked = 0f;
			if (this.hold_orgasm_after_t > 0f)
			{
				this.hold_orgasm_after_t = -200f;
			}
			IN.clearPushDown(true);
			this.Pr.getSkillManager().punch_decline_time = 7;
		}

		public M2MoverPr.DECL decline
		{
			get
			{
				if (this.t_sage == 0f)
				{
					return (M2MoverPr.DECL)0;
				}
				return M2MoverPr.DECL.STOP_COMMAND;
			}
		}

		public void run(float fcnt)
		{
			if (this.t_sage < 0f)
			{
				this.PeZoom.fine(100);
				this.t_sage += fcnt;
				if (this.t_sage >= 0f)
				{
					this.quitOrgasmInitTime();
				}
			}
			else if (this.t_crack_cure != 0f)
			{
				bool flag = this.t_crack_cure < 0f;
				this.t_crack_cure = X.VALWALK(this.t_crack_cure, 0f, fcnt);
				if (this.t_crack_cure == 0f)
				{
					this.t_crack_cure = 0f;
					int num = X.Mn(this.crack_cure_once, this.crack_cure_count);
					this.Pr.GaugeBrk.Cure(num, true);
					this.crack_cure_count -= num;
					this.crack_cure_once = X.Mn(X.Mx(1, X.Mn(this.crack_cure_count / 3, this.crack_cure_once)), this.crack_cure_count);
					if (flag)
					{
						UILog.Instance.AddAlertTX("Alert_cure_crack", UILogRow.TYPE.ALERT);
					}
					this.t_crack_cure = (float)((this.crack_cure_count == 0) ? 0 : 20);
				}
			}
			float num2 = 0f;
			bool flag2 = false;
			if (this.t_lock_orgasmlocked > 0f)
			{
				this.t_lock_orgasmlocked = X.Mx(this.t_lock_orgasmlocked - fcnt, 0f);
			}
			if (this.t_lock > 0f)
			{
				this.t_lock = X.Mx(this.t_lock - fcnt, 0f);
			}
			else
			{
				if (this.ep > 0f && this.ep <= 300f && this.Pr.isManipulateState())
				{
					num2 = fcnt * 0.14285715f;
				}
				if (!this.Pr.Ser.has(SER.FRUSTRATED))
				{
					if (this.t_oazuke < 0f && this.t_sage == 0f && !EnemySummoner.isActiveBorder() && this.Pr.isDoubleSexercise())
					{
						this.t_oazuke = 0f;
					}
					if (this.t_oazuke >= 0f && this.t_sage == 0f)
					{
						if (X.BTWW((float)(this.Pr.isDoubleSexercise() ? 500 : this.exp_add_ep), this.ep, 950f))
						{
							this.t_oazuke += fcnt;
							if (this.t_oazuke >= 160f)
							{
								this.t_oazuke = -1f;
								this.checkOazuke(false);
							}
						}
						else if (this.ep > 950f)
						{
							if (this.Pr.isManipulateState())
							{
								this.t_oazuke += fcnt;
								if (this.t_oazuke >= 40f)
								{
									this.ep = 950f;
									flag2 = true;
									this.t_oazuke = -1f;
									this.checkOazuke(true);
								}
							}
						}
						else
						{
							this.t_oazuke = -1f;
						}
					}
				}
				else if (this.t_sage == 0f)
				{
					if (!this.Pr.Ser.has(SER.SHAMED_EP))
					{
						this.Pr.Ser.Cure(SER.FRUSTRATED);
						this.t_oazuke = -1f;
					}
					else if (!X.SENSITIVE && this.attachable_ser_oazuke && ((this.Pr.isBenchState() && SCN.occurableOazuke()) || this.Pr.isMasturbating()))
					{
						this.Pr.Ser.Add(SER.FRUSTRATED, -1, 99, false);
						this.attachable_ser_oazuke = false;
					}
					else if (this.t_oazuke >= 0f && !X.SENSITIVE && this.Pr.canStartFrustratedMasturbate(true))
					{
						this.t_oazuke += fcnt;
						if (this.t_oazuke >= (float)(((!PUZ.IT.barrier_active && this.Pr.enemy_targetted == 0) || this.Pr.isDoubleSexercise()) ? 50 : 130))
						{
							if (this.Pr is PRMain)
							{
								(this.Pr as PRMain).initMasturbation(true, false);
							}
							this.t_oazuke = -200f;
						}
					}
					else if (this.t_oazuke > 0f)
					{
						this.t_oazuke = X.Mx(this.t_oazuke - fcnt * 2f, 0f);
					}
					else if (this.t_oazuke < 0f)
					{
						this.t_oazuke = X.VALWALK(this.t_oazuke, 0f, fcnt);
					}
				}
			}
			if (this.t_sage > 0f)
			{
				this.PeZoom.fine(100);
				if (X.XORSP() < 0.03f)
				{
					this.Pr.DMGE.publishVaginaSplash(MTR.col_blood_ep, 5, 1.02f);
				}
				if (!Map2d.can_handle || !this.Pr.isLayingEggOrOrgasm() || this.Pr.EggCon.isLaying())
				{
					if (this.t_sage > 1f)
					{
						this.t_sage = 1f;
						if (this.dsex_orgasm_pre > 1)
						{
							this.dsex_orgasm_pre = X.Mx(this.dsex_orgasm_pre - 1, 1);
						}
					}
				}
				else
				{
					if (this.cure_ep_after_orgasm > 0f)
					{
						num2 = X.Mx(num2, this.cure_ep_after_orgasm_one);
						this.cure_ep_after_orgasm -= num2;
					}
					this.t_sage += fcnt;
					float num3 = (this.Pr.is_alive ? 40f : 140f);
					if (this.t_sage >= num3)
					{
						bool flag3 = false;
						if (!this.Pr.Ser.has(SER.ORGASM_AFTER))
						{
							flag3 = true;
						}
						else if (this.AbsorbOrgasm == null)
						{
							if (!this.Pr.is_alive)
							{
								flag3 = true;
							}
							else
							{
								this.AbsorbOrgasm = this.Pr.getAbsorbContainer().initSpecialGachaNotDiffFix(this.Pr, this, PrGachaItem.TYPE.REP_AFTER_ORGASM, this.getOrgasmAbsorbTapCount(), KEY.SIMKEY.Z, true);
								if (this.AbsorbOrgasm != null)
								{
									this.AbsorbOrgasm.Con.no_error_on_miss_input = true;
									this.AbsorbOrgasm.Con.timeout = 0;
									this.AbsorbOrgasm.kirimomi_release = false;
									this.AbsorbOrgasm.get_Gacha().addCountAbs(this.gacha_hit_stocked, 0.875f);
									if (this.Pr.Rebagacha != null)
									{
										this.Pr.Rebagacha.need_reposit = true;
									}
								}
							}
						}
						if (flag3)
						{
							this.quitOrgasmAfterTime();
							flag2 = true;
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < 2; i++)
				{
					float num4 = ((i == 0) ? 0.5f : 0.85f);
					if (this.At_announce[i] > 0f)
					{
						if (this.ep < 1000f * num4)
						{
							this.At_announce[i] = X.Mx(this.At_announce[i] - fcnt, 0f);
						}
					}
					else if (this.ep >= 1000f * num4)
					{
						this.At_announce[i] = 200f;
						UILog.Instance.AddAlertTX("Alert_ep_thresh" + i.ToString(), UILogRow.TYPE.ALERT_EP2);
					}
				}
			}
			if (this.t_corrupt_gacha_mist > 0f)
			{
				this.t_corrupt_gacha_mist = X.Mx(0f, this.t_corrupt_gacha_mist - fcnt);
				if (this.Pr.isAbsorbState() && this.AbsorbOrgasm == null)
				{
					this.Pr.getAbsorbContainer().CorruptGacha(0.041666668f, false);
				}
			}
			if (num2 > 0f)
			{
				this.ep = X.Mx(0f, this.ep - num2);
				flag2 = true;
			}
			if (flag2)
			{
				this.changedFine();
			}
			else if (this.ep == 0f && this.EfUi != null)
			{
				this.fineEffect(false);
			}
			if (this.t_ptc >= 0f)
			{
				this.t_ptc -= fcnt;
				if (this.t_ptc <= 0f)
				{
					float num5 = this.ep_ratio;
					if (this.t_sage < 0f)
					{
						num5 = 1f;
					}
					this.t_ptc = X.NI(110, 50, (num5 - 0.5f) / 0.5f);
					if (UIBase.FlgUiEffectGmDisable == null && !UIBase.FlgUiEffectGmDisable.isActive())
					{
						this.Pr.PtcVar("ep", (double)num5).PtcST("ep_smoke", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					}
				}
			}
			this.Suppressor.run(fcnt);
			if (this.t_orgasm_cutin_tikatika > 0f)
			{
				this.t_orgasm_cutin_tikatika -= fcnt * 0.0033333334f;
			}
			if (this.hold_orgasm_after_t > 0f)
			{
				this.hold_orgasm_after_t -= fcnt;
				if (this.hold_orgasm_after_t <= 0f)
				{
					this.hold_orgasm_after_t = -200f;
				}
			}
			else if (this.hold_orgasm_after_t < 0f && (this.t_sage == 0f || this.t_sage > 1f))
			{
				this.hold_orgasm_after_t = X.VALWALK(this.hold_orgasm_after_t, 0f, fcnt);
			}
			if (this.dsex_orgasm > 0 && this.Suppressor.Count == 0 && this.t_sage == 0f && !this.Pr.Ser.hasBit(50331648UL))
			{
				this.dsex_orgasm = 0;
			}
		}

		public void lockOazuke(float _t = 0f)
		{
			if (_t <= 0f || !this.Pr.Ser.has(SER.FRUSTRATED))
			{
				if (this.t_oazuke > 0f)
				{
					this.t_oazuke = X.Mn(this.t_oazuke, 5f);
				}
				return;
			}
			if (this.t_oazuke > 0f)
			{
				this.t_oazuke = -_t;
				return;
			}
			this.t_oazuke = X.Mn(this.t_oazuke, -_t);
		}

		public bool hold_orgasm_after
		{
			get
			{
				return this.t_sage < 0f || (this.t_sage > 0f && this.hold_orgasm_after_t > 0f);
			}
		}

		private void changedFine()
		{
			this.Pr.ep = X.IntC(this.ep);
			this.Pr.Ser.checkSer();
			this.fineEffect(false);
		}

		public float get_orgasm_absorb_time()
		{
			if (this.t_sage <= 0f)
			{
				return 0f;
			}
			return this.t_sage - 40f;
		}

		public void runUi()
		{
			if (this.t_ptc_ui >= 0 && this.Pr.NM2D.Ui.ui_particle_active)
			{
				this.t_ptc_ui--;
				if (this.t_ptc_ui <= 0)
				{
					float num = this.ep_ratio;
					if (this.t_sage < 0f)
					{
						num = 1f;
					}
					this.t_ptc_ui = (int)X.NI(70, 20, (num - 0.5f) / 0.5f);
					if (UIBase.FlgUiEffectGmDisable == null || !UIBase.FlgUiEffectGmDisable.isActive())
					{
						UIBase.Instance.getEffect().PtcSTsetVar("ep", (double)num).PtcST("ep_smoke_ui", null, PTCThread.StFollow.NO_FOLLOW, null);
					}
				}
			}
		}

		public bool isOrgasm()
		{
			return this.t_sage != 0f;
		}

		public bool isOrgasmStarted(int t)
		{
			return this.t_sage != 0f && this.t_sage < (float)t;
		}

		public bool isOrgasmInitTime()
		{
			return this.t_sage < 0f;
		}

		public int getOrgasmedTotal()
		{
			return X.sum(this.Atotal_orgasmed);
		}

		public int getOrgasmedCountForSituation(string situation)
		{
			return X.Get<string, int>(this.Osituation_orgasmed, situation.ToUpper(), 0);
		}

		public int getOrgasmedIndividualTotal()
		{
			return this.orgasm_individual_count;
		}

		public void quitOrasmSageTime(bool cure_ser)
		{
			if (this.t_sage < 0f)
			{
				this.quitOrgasmInitTime();
			}
			if (this.t_sage > 0f)
			{
				this.quitOrgasmAfterTime();
			}
			this.Pr.Ser.Cure(SER.ORGASM_INITIALIZE);
			if (cure_ser)
			{
				this.Pr.Ser.Cure(SER.ORGASM_AFTER);
			}
		}

		public bool individual
		{
			get
			{
				return true;
			}
		}

		public void corruptGachaMist(float t)
		{
			this.t_corrupt_gacha_mist = X.Mx(this.t_corrupt_gacha_mist, t);
		}

		public bool is_in_corrupt_gacha_mist
		{
			get
			{
				return this.t_corrupt_gacha_mist > 0f;
			}
		}

		public bool use_effect
		{
			get
			{
				return this.t_sage != 0f || this.ep > 300f;
			}
		}

		public void releaseEffect()
		{
			if (this.EfUi != null)
			{
				this.EfUi.destruct();
				this.EfUi = null;
			}
		}

		public void fineEffect(bool fine_time = false)
		{
			if (this.use_effect)
			{
				if (this.EfUi != null && (ulong)this.EfUi.index != (ulong)((long)this.efui_index))
				{
					this.EfUi.destruct();
					this.EfUi = null;
				}
				if (this.EfUi == null)
				{
					this.EfUi = UIBase.Instance.getEffect().setEffectWithSpecificFn("epmanager_draweffect", 0f, 0f, 0f, 0, 0, this.FD_drawEffect);
					if (this.EfUi == null)
					{
						return;
					}
					this.efui_index = (int)this.EfUi.index;
					this.t_ptc = (float)(this.t_ptc_ui = 0);
				}
				if (fine_time && !this.go_unlock_thresh)
				{
					this.EfUi.time = (int)this.Pr.Mp.floort;
				}
			}
			else if (this.EfUi != null)
			{
				this.t_ptc = (float)(this.t_ptc_ui = -1);
				this.EfUi.af = 0f;
				this.EfUi.z = 1f;
				this.EfUi = null;
			}
			if (this.crack_cure_count > 0 && this.t_crack_cure <= 0f)
			{
				this.t_crack_cure = ((this.t_crack_cure == 0f) ? (-50f) : X.Mx(-50f, this.t_crack_cure));
			}
		}

		private bool drawEffect(EffectItem E)
		{
			float num = ((E.z == 0f) ? X.ZLINE(E.af - 60f, 120f) : (1f - X.ZLINE(E.af, 90f)));
			if (num <= 0f)
			{
				return E.z == 0f;
			}
			if (UIBase.FlgUiEffectGmDisable != null && UIBase.FlgUiEffectGmDisable.isActive())
			{
				return true;
			}
			float num2 = (this.go_unlock_thresh ? 1f : X.ZLINE(this.Pr.Mp.floort - (float)E.time, 33f));
			MeshDrawer mesh = E.GetMesh("ep", this.MtrHeartPattern, true);
			mesh.base_z += 0.002f;
			if (UIBase.Instance.event_center_uipic)
			{
				float gamemenu_slide_z = UIBase.Instance.gamemenu_slide_z;
				UIBase.Instance.setBaseToScreenCenter(mesh);
				mesh.base_x -= X.NI((IN.wh + 340f) * 0.015625f, (IN.wh - 170f) * 0.015625f, gamemenu_slide_z);
			}
			mesh.ColGrd.Set(3439274452U);
			float num3 = 1f;
			float num4 = this.ep_ratio;
			float num7;
			float num8;
			float num9;
			if (this.t_sage < 0f)
			{
				mesh.ColGrd.Set(3439294397U);
				float num5 = X.COSIT(45f);
				mesh.setMtrColor("_ColorA", C32.MulA(mesh.ColGrd.C, num));
				mesh.setMtrColor("_ColorB", C32.MulA(mesh.ColGrd.blend(3426025499U, 0.5f + num5 * 0.125f + X.COSIT(9.34f) * 0.125f).C, num));
				float num6 = X.NI(1f, 0.85f + 0.1f * X.COSIT(130f) + 0.025f * X.COSIT(2.83f) + 0.025f * X.COSIT(5.13f), num2);
				mesh.Col = mesh.ColGrd.White().mulA(num6).C;
				num7 = 0.5f + 0.12f * X.COSIT(90f);
				num8 = 1f - X.ANMPT(70, 1f);
				num9 = X.ZLINE(1f - X.ZSIN(num2) * 0.45f + X.COSIT(21f) * 0.04f);
				num4 = X.NI(1f, num4, X.ZLINE(1f + this.t_sage / this.calcOrgasmInitDelayTime() - 0.5f, 0.5f));
			}
			else if (this.t_sage > 0f)
			{
				mesh.ColGrd.Set(3437263270U);
				mesh.setMtrColor("_ColorA", C32.MulA(mesh.ColGrd.C, num));
				mesh.setMtrColor("_ColorB", C32.MulA(mesh.ColGrd.blend(3430242404U, num2).C, num));
				float num10 = num * (0.8f + 0.1f * X.COSIT(170f) + 0.022f * X.COSIT(3.83f) + 0.022f * X.COSIT(5.41f));
				num8 = X.ANMPT(120, 1f);
				num7 = 2f - num8;
				if (num2 < 1f)
				{
					num7 += (1f - num2) * (0.22f + X.COSIT(43f) * 0.03f + X.COSIT(4.78f) * 0.03f);
				}
				num9 = X.NI(1f - num8, 1f, (1f - num2) * 0.6f);
				mesh.Col = mesh.ColGrd.White().mulA(num10 * X.NI(1f - X.ZLINE(num9 - 0.75f, 0.25f), 1f, X.ZSIN2(1f - num2))).C;
			}
			else
			{
				float num11 = X.ANMPT((int)X.NI(120, 55, X.ZLINE(num4 - 0.4f, 0.4f)), 1f);
				num3 = 1f - num11 * 0.25f;
				float num12 = X.ZSINV(num4 - 0.3f, 0.59999996f);
				float num13 = num * X.NI(1f, 1f - 0.75f * num11, num2) * X.NI(0.55f, 0.9f, num12);
				mesh.setMtrColor("_ColorA", mesh.ColGrd.Set(3438402791U).blend(3439289524U, num12).mulA(num)
					.C);
				mesh.setMtrColor("_ColorB", mesh.ColGrd.Set(1153607605).blend(2008911270U, X.ZPOW(num12 - 0.2f, 0.8f)).mulA(num)
					.C);
				num8 = 1f - X.ANMPT((int)X.NI(120, 85, X.ZLINE(num4 - 0.4f, 0.5f)), 1f);
				num7 = 0f;
				if (num2 < 1f)
				{
					num7 += (1f - num2) * (0.14f + X.COSIT(73f) * 0.014f + X.COSIT(6.78f) * 0.015f);
				}
				num9 = X.ANMPT((int)X.NI(120, 65, X.ZLINE(num4 - 0.4f, 0.6f)), 1f);
				mesh.Col = mesh.ColGrd.White().mulA(num13 * X.NI(1f - X.ZLINE(num9 - 0.75f, 0.25f), 1f, X.ZSIN2(1f - num2))).C;
			}
			PxlSequence sqPatHeartVoltage = MTR.SqPatHeartVoltage;
			mesh.PatternFill4GDTPat(-170f, -IN.hh - 2f, 340f, (IN.h + 2f) * num4 * num3, sqPatHeartVoltage.getImage(X.Mn((int)((float)sqPatHeartVoltage.countFrames() * num9), sqPatHeartVoltage.countFrames() - 1), 0), 2f, num7, num8);
			return true;
		}

		private void addP(Designer Bx, string t, ALIGN _alignx = ALIGN.LEFT, float _swidth = 0f, bool _html = false, float _sheight = 0f, string _name = "")
		{
			UiBoxDesigner.addPTo(Bx, t, _alignx, _swidth, _html, _sheight, _name, -1);
		}

		public void addGMConditionDescript(Designer Bx)
		{
			if (!this.M2D.FlgWarpEventNotInjectable.isActive())
			{
				this.SituCon.flushLastExSituationTemp();
			}
			int orgasmedTotal = this.getOrgasmedTotal();
			if (X.DEBUG)
			{
				this.addP(Bx, TX.GetA("GM_ep_debug", ((int)this.ep).ToString() + "/" + 1000f.ToString(), orgasmedTotal.ToString()), ALIGN.LEFT, 0f, false, 0f, "");
				Bx.Br();
			}
			if (this.t_sage != 0f)
			{
				this.addP(Bx, TX.Get("GM_ep_level_after_orgasm", ""), ALIGN.LEFT, 0f, true, 0f, "");
				Bx.Br();
			}
			else if (this.ep > 300f)
			{
				this.addP(Bx, TX.GetA("GM_ep_level_" + X.Mn((int)(this.ep_ratio * 10f), 9).ToString(), ((int)(this.ep_ratio * 100f)).ToString() + "%"), ALIGN.LEFT, 0f, true, 0f, "");
				Bx.Br();
			}
			else if (this.getOrgasmedIndividualTotal() >= 5)
			{
				this.addP(Bx, TX.Get("GM_ep_level_0", ""), ALIGN.LEFT, 0f, true, 0f, "");
				Bx.Br();
			}
			DsnDataP dsnDataP = new DsnDataP("", false)
			{
				text_auto_condense = true,
				swidth = Bx.use_w - 30f,
				size = 14f,
				alignx = ALIGN.LEFT,
				Col = C32.d2c(4283780170U),
				html = true
			};
			DsnDataP dsnDataP2 = new DsnDataP("", false)
			{
				text_auto_condense = true,
				swidth = Bx.use_w,
				size = 16f,
				alignx = ALIGN.LEFT,
				TxCol = C32.d2c(4283780170U),
				html = true
			};
			DsnDataHr dsnDataHr = new DsnDataHr
			{
				margin_t = UiLunchTimeBase.costbx_h * 0.33f,
				margin_b = UiLunchTimeBase.costbx_h * 0.33f + 6f,
				draw_width_rate = 0.6f,
				swidth = Bx.use_w
			};
			bool flag = false;
			if (orgasmedTotal >= 1)
			{
				string text = "";
				for (int i = 0; i < 11; i++)
				{
					float num = X.MMX(0f, (float)this.Atotal_exp[i] / 20f, 2f);
					string text2 = "EP_Targ_";
					EPCATEG epcateg = (EPCATEG)i;
					string text3 = TX.Get(text2 + epcateg.ToString().ToLower(), "???");
					if (num == 0f)
					{
						text = TX.add(text, string.Concat(new string[]
						{
							"<font color=\"",
							C32.codeToCodeText(4290689711U),
							"\">",
							TX.GetA("GM_ep_experience2_nothing", text3),
							"</font>"
						}), "\n");
					}
					else
					{
						string text4 = ((int)(num * 100f)).ToString() + "%";
						if (num > 1f)
						{
							text4 = string.Concat(new string[]
							{
								"<font color=\"",
								C32.codeToCodeText(4294966715U),
								"\">",
								text4,
								"</font>"
							});
						}
						text = TX.add(text, TX.GetA("GM_ep_experience2", text3, this.Atotal_orgasmed[i].ToString() ?? "", text4), "\n");
					}
				}
				Bx.add(dsnDataP2.Text("<font color=\"ff:#EE1358\"><img mesh=\"alert_ep\" width=\"28\" tx_color />" + TX.Get("GM_ep_title_sensitivity", "") + "</font>")).Br();
				Bx.XSh(20f).add(dsnDataP.Text(text)).Br();
				flag = true;
			}
			if (this.Pr.EggCon.isActive())
			{
				if (flag)
				{
					Bx.Br().addHr(dsnDataHr);
				}
				flag = true;
				Bx.Br().add(dsnDataP2.Text("<font color=\"ff:#EE1358\"><img mesh=\"alert_ep\" width=\"28\" tx_color />" + TX.Get("GM_ep_title_current_egg", "") + "</font>")).Br();
				int category_count = this.Pr.EggCon.category_count;
				Color32 color = C32.d2c(Bx.effect_confusion ? 4289571750U : 4283780170U);
				for (int j = 0; j < category_count; j++)
				{
					PrEggManager.PrEggItem eggItem = this.Pr.EggCon.getEggItem(j);
					string eggLocalizedTitle = EpManager.getEggLocalizedTitle(eggItem.categ);
					UiBoxDesigner.addPTo(Bx, eggLocalizedTitle, ALIGN.CENTER, 130f, true, 0f, "", -1);
					string text5;
					if (!eggItem.no_auto_getout)
					{
						float num2 = X.ZLINE((float)eggItem.val_absorbed_view, eggItem.val);
						if (!eggItem.isLayingEgg())
						{
							num2 = X.Mn(num2, 0.995f);
						}
						else
						{
							num2 = 1f;
						}
						text5 = TX.GetA("GM_Egg_amount_and_feeded", X.IntR(eggItem.val).ToString(), X.IntR(num2 * 100f).ToString());
						text5 = text5 + "\n" + TX.Get("GM_Egg_desc_" + ((int)(num2 * 3f)).ToString(), "");
					}
					else
					{
						text5 = TX.GetA("GM_Egg_amount", X.IntR(eggItem.val).ToString());
						text5 = text5 + "\n" + (eggItem.isLayingEgg() ? TX.Get("GM_Egg_desc_no_getout_laying", "") : TX.Get("GM_Egg_desc_no_getout", ""));
					}
					Bx.addP(new DsnDataP("", false)
					{
						text = text5,
						alignx = ALIGN.LEFT,
						swidth = Bx.use_w - 10f,
						html = true,
						size = 14f,
						text_auto_wrap = true,
						Col = color
					}, false);
					Bx.Br();
				}
			}
			if (orgasmedTotal > 0)
			{
				if (flag)
				{
					Bx.Br().addHr(dsnDataHr);
				}
				flag = true;
				Bx.Br().add(dsnDataP2.Text("<font color=\"ff:#EE1358\"><img mesh=\"alert_ep\" width=\"28\" tx_color />" + TX.Get("GM_ep_title_experience", "") + "</font>")).Br();
				string text6 = "";
				foreach (KeyValuePair<string, int> keyValuePair in this.Osituation_orgasmed)
				{
					if (keyValuePair.Key == "masturbate".ToUpper())
					{
						text6 = TX.add(text6, TX.GetA("GM_ep_experience_genre", TX.Get("GM_ep_situation_masturbate", ""), keyValuePair.Value.ToString()), "\n");
					}
					else
					{
						text6 = TX.add(text6, TX.GetA("GM_ep_experience_genre", TX.Get("Enemy_" + keyValuePair.Key.ToUpper(), ""), keyValuePair.Value.ToString()), "\n");
					}
				}
				Bx.XSh(20f).add(dsnDataP.Text(text6)).Br();
			}
			if (flag)
			{
				Bx.Br().addHr(dsnDataHr);
			}
			flag = true;
			Bx.Br().add(dsnDataP2.Text("<font color=\"ff:#EE1358\"><img mesh=\"alert_ep\" width=\"28\" tx_color />" + TX.Get("GM_ep_title_last_memory", "") + "</font>")).Br();
			int descCount = this.SituCon.getDescCount();
			for (int k = 0; k < descCount; k++)
			{
				string descByIndex = this.SituCon.getDescByIndex(k);
				Bx.XSh(40f).addP(new DsnDataP("", false)
				{
					text = TX.ReplaceTX(descByIndex, false),
					alignx = ALIGN.LEFT,
					swidth = Bx.use_w - 10f,
					html = true,
					size = 14f,
					text_auto_wrap = true,
					TxCol = C32.d2c(4283780170U)
				}, false);
				Bx.Br();
			}
			if (this.Oegg_layed.Count > 0)
			{
				for (int l = 0; l < 2; l++)
				{
					string text7 = "";
					foreach (KeyValuePair<PrEggManager.CATEG, int> keyValuePair2 in this.Oegg_layed)
					{
						if (l == 1 == PrEggManager.is_liquid(keyValuePair2.Key))
						{
							if (l == 0)
							{
								TX tx;
								if ((tx = TX.getTX("GM_ep_experience_egg_" + keyValuePair2.Key.ToString(), true, true, null)) != null)
								{
									text7 = TX.add(text7, TX.GetA(tx, keyValuePair2.Value.ToString()), "\n");
								}
								else
								{
									text7 = TX.add(text7, TX.GetA("GM_ep_experience_egg", EpManager.getEggLocalizedTitle(keyValuePair2.Key), keyValuePair2.Value.ToString()), "\n");
								}
							}
							else
							{
								text7 = TX.add(text7, TX.GetA("GM_ep_experience_liquid", EpManager.getEggLocalizedTitle(keyValuePair2.Key), (keyValuePair2.Value * 2).ToString()), "\n");
							}
						}
					}
					if (text7 != "")
					{
						if (flag)
						{
							Bx.Br().addHr(dsnDataHr);
						}
						flag = true;
						Bx.Br().add(dsnDataP2.Text("<font color=\"ff:#EE1358\"><img mesh=\"alert_ep\" width=\"28\" tx_color />" + TX.Get((l == 0) ? "GM_ep_title_egg_total" : "GM_ep_title_liquid", "") + "</font>")).Br();
						Bx.XSh(20f).add(dsnDataP.Text(text7)).Br();
					}
				}
			}
			Bx.Br();
			if (flag)
			{
				Bx.Br().addHr(dsnDataHr);
			}
			flag = true;
			float swidth = dsnDataP2.swidth;
			dsnDataP2.swidth = 0f;
			Bx.add(dsnDataP2.Text("<font color=\"ff:#EE1358\"><img mesh=\"alert_ep\" width=\"28\" tx_color />" + TX.Get("GM_ep_title_water_drunk_level", "") + "</font>"));
			Bx.XSh(40f);
			Bx.addImg(new DsnDataImg
			{
				text = TX.GetA("GM_ep_water_drunk_level", this.Pr.JuiceCon.water_drunk.ToString()),
				alignx = ALIGN.LEFT,
				swidth = Bx.use_w - 70f,
				html = true,
				size = 14f,
				text_auto_wrap = true,
				TxCol = C32.d2c(4283780170U),
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.FnDrawInFIBPeeSlot),
				MI = MTRX.MIicon
			});
			if (this.pee_count > 0)
			{
				Bx.Br();
				dsnDataP2.swidth = Bx.use_w * 0.5f;
				Bx.add(dsnDataP2.Text("<font color=\"ff:#EE1358\"><img mesh=\"alert_ep\" width=\"28\" tx_color />" + TX.Get("GM_ep_title_pee", "") + "</font>"));
				Bx.XSh(40f).addP(new DsnDataP("", false)
				{
					text = TX.GetA("GM_ep_experience_pee_count", this.pee_count.ToString()),
					alignx = ALIGN.LEFT,
					swidth = Bx.use_w - 10f,
					html = true,
					size = 14f,
					text_auto_wrap = true,
					TxCol = C32.d2c(4283780170U)
				}, false);
				Bx.Br();
			}
			dsnDataP2.swidth = swidth;
		}

		private bool FnDrawInFIBPeeSlot(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (FI.get_text_swidth_px(false) <= 0f)
			{
				return false;
			}
			int stock = NelItem.NoelJuice.stock;
			if (this.PeeSlotDr == null)
			{
				this.PeeSlotDr = new SlotDrawer(MTR.SqPeeSlot.getFrame(0), 0, 0);
				this.PeeSlotDr.ReturnIntv(stock, 28, 0);
			}
			this.PeeSlotDr.clearFill().Fill(MTR.SqPeeSlot.getFrame(1), 0, this.Pr.JuiceCon.juice_stock);
			float swidthByCount = this.PeeSlotDr.getSWidthByCount(stock);
			float sheightByCount = this.PeeSlotDr.getSHeightByCount(stock);
			Md.Col = C32.MulA(MTRX.ColWhite, alpha);
			this.PeeSlotDr.drawTo(Md, -FI.get_swidth_px() * 0.5f + FI.get_text_swidth_px(false) * 0.5f + 38f, 0f, 1f, 1f, stock, swidthByCount, sheightByCount);
			return true;
		}

		public static string getEggLocalizedTitle(PrEggManager.CATEG categ)
		{
			TX tx;
			if ((tx = TX.getTX("GM_Egg_categ_" + categ.ToString().ToUpper(), true, true, null)) != null)
			{
				return tx.text;
			}
			return TX.Get("Enemy_" + categ.ToString().ToUpper(), "");
		}

		public void setAlert(bool is_orgasm, string target_s, string situation_key)
		{
			string text = (is_orgasm ? "EP_finish_" : "EP_process_");
			TX tx = TX.getTX(text + situation_key + "_" + target_s, true, true, TX.getCurrentFamily());
			if (tx == null)
			{
				tx = TX.getTX(text + situation_key, true, true, TX.getCurrentFamily());
				if (tx == null)
				{
					tx = TX.getTX(text + "general_" + target_s, true, true, TX.getCurrentFamily());
					if (tx == null)
					{
						tx = TX.getTX(text + "general_" + X.xors(3).ToString(), true, false, null);
					}
				}
			}
			string text2 = situation_key.ToUpper();
			if (REG.match(text2, REG.RegSuffixNumber))
			{
				text2 = REG.leftContext;
			}
			UILog.Instance.AddAlert(TX.GetA(tx, TX.Get("Enemy_" + text2, ""), TX.Get("EP_Targ_" + target_s, "")), is_orgasm ? UILogRow.TYPE.ALERT_EP2 : UILogRow.TYPE.ALERT_EP);
		}

		public static UILogRow callOtherAlert(string tx_key, float add_lvl, float ratio_basic = 0.2f, float ratio_by_cfg = 1f)
		{
			float num = (float)CFG.ui_sensitive_description / 10f;
			num = ratio_basic + ratio_by_cfg * (1f - (1f - num) * (1f - add_lvl));
			if (X.XORSP() < num)
			{
				return UILog.Instance.AddAlert(TX.Get(tx_key, ""), UILogRow.TYPE.ALERT_EP);
			}
			return null;
		}

		public void addPeeCount()
		{
			this.pee_count++;
		}

		public float ep_ratio
		{
			get
			{
				return (float)this.Pr.ep / 1000f;
			}
		}

		public void initFrustrationDebug()
		{
			this.t_oazuke = 0f;
		}

		public int masturbate_count
		{
			get
			{
				return X.Get<string, int>(this.Osituation_orgasmed, "masturbate".ToUpper(), 0);
			}
		}

		public void readBinaryFrom(ByteReader Ba)
		{
			this.newGame();
			int num = Ba.readByte();
			this.ep = Ba.readFloat();
			this.ep = X.Mn(this.ep, 950f);
			if (num == 0)
			{
				this.t_lock = Ba.readFloat();
				Ba.readInt();
				Ba.readInt();
				Ba.readDictionaryTFloat<int>((int V) => V);
				Ba.readDictionaryTFloat<int>((int V) => V);
				Ba.readDictionaryTInt<int>((int V) => V);
				new FlagCounter<int>(4).readBinaryFrom(Ba, (int V) => V).readBinaryFrom(Ba, (int V) => V).readBinaryFrom(Ba, (int V) => V);
			}
			else
			{
				this.bt_exp_added = (EPCATEG_BITS)Ba.readUInt();
				this.bt_applied = (EPCATEG_BITS)Ba.readUInt();
				this.bt_orgasm = (EPCATEG_BITS)Ba.readUInt();
				Ba.readByteA(ref this.Atotal_exp, true);
				Ba.readIntA(ref this.Atotal_orgasmed);
				if (num >= 2)
				{
					int num2 = (int)Ba.readUShort();
					this.Osituation_orgasmed = new BDic<string, int>(num2);
					for (int i = 0; i < num2; i++)
					{
						string text = EpSituation.situ_write_replace(Ba.readString("utf-8", false));
						this.Osituation_orgasmed[text] = Ba.readInt();
					}
					this.SituCon.readBinaryFrom(Ba, num >= 5);
					if (num < 5)
					{
						Ba.readUInt();
						Ba.readUInt();
					}
					this.pee_count = (int)Ba.readUInt();
					this.Oegg_layed = new BDic<PrEggManager.CATEG, int>(Ba.readDictionaryTInt<PrEggManager.CATEG>((int V) => (PrEggManager.CATEG)V));
					if (num >= 3)
					{
						this.orgasm_individual_count = (int)Ba.readUInt();
						if (num >= 4)
						{
							int num3 = Ba.readByte();
							this.splash_lock = (num3 & 1) != 0;
							this.attachable_ser_oazuke = (num3 & 2) != 0;
						}
					}
				}
			}
			if (num < 5)
			{
				this.SituCon.flushLastExSituationTemp();
			}
			if (num < 3)
			{
				this.orgasm_individual_count = X.sum(this.Atotal_orgasmed);
			}
			this.Pr.ep = X.IntC(this.ep);
			this.flushCurrentBattle();
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(5);
			Ba.writeFloat(this.ep);
			Ba.writeUInt((uint)this.bt_exp_added);
			Ba.writeUInt((uint)this.bt_applied);
			Ba.writeUInt((uint)this.bt_orgasm);
			Ba.writeByteA(this.Atotal_exp, 2);
			Ba.writeIntA(this.Atotal_orgasmed);
			Ba.writeDictionary(this.Osituation_orgasmed);
			this.SituCon.writeBinaryTo(Ba);
			Ba.writeUInt((uint)this.pee_count);
			Ba.writeDictionaryT<PrEggManager.CATEG>(this.Oegg_layed, (PrEggManager.CATEG V) => (int)V);
			Ba.writeUInt((uint)this.orgasm_individual_count);
			Ba.writeByte((this.splash_lock ? 1 : 0) | (this.attachable_ser_oazuke ? 2 : 0));
		}

		public EpAtk copyOverkillPower()
		{
			EpAtk epAtk = new EpAtk(40, "other");
			for (int i = 0; i < 11; i++)
			{
				if ((this.bt_applied & (EPCATEG_BITS)(1 << i)) != (EPCATEG_BITS)0)
				{
					epAtk.Set(i, 1);
				}
			}
			return epAtk;
		}

		public NelM2DBase M2D
		{
			get
			{
				return this.Pr.NM2D;
			}
		}

		public readonly PR Pr;

		private const int ep_max = 1000;

		private const int cure_count_normal = 2;

		private const int cure_count_mastur = 1;

		private const float cure_crack_masturbation_ratio = 0.33f;

		public const float breathe_effect_thresh = 0.5f;

		private const float sexercie_orgasmable_add_min = 0.08f;

		private const float sexercie_orgasmable_add_max = 0.2f;

		private const float sexercie_exp_adding = 10f;

		public const float suppress_ratio = 0.33f;

		public const int suppress_delay = 540;

		private const int exp_add_ep_normal = 850;

		private const int ep_oazuke_in_dsex = 500;

		private const int exp_add_ep_sexercise = 700;

		private const int ep_coming_add = 7;

		public const int EP_COMING = 950;

		private const float HOLD_OGRASM_MAXT = 520f;

		private const float HOLD_OGRASM_LOCKT = 200f;

		private const int corrupt_frustrated = 5;

		private const int corrupt_orgasm_after = 1;

		private const float corrupt_gacha_mist_level = 0.041666668f;

		public const string SITUATION_MASTURBATE = "masturbate";

		public const string SITUATION_MASTURBATE_UPPER = "MASTURBATE";

		public const string SITUATION_OTHER = "OTHER";

		private EPCATEG_BITS lead_to_orgasm;

		private float hold_orgasm_after_t;

		private int dsex_orgasm;

		private int dsex_orgasm_pre;

		private readonly List<EpManager.OrgasmInfo> Aorgasm_locked_stack;

		public const int categ_max = 11;

		private const byte EXP_MAX = 20;

		private const byte EXP_MAX_IN_SESSION = 40;

		private const int EXP_ORGASMABLE_MAX = 60;

		private byte[] Atotal_exp;

		private int[] Atotal_orgasmed;

		private byte[] Atotal_exp_default;

		private BDic<string, int> Osituation_orgasmed;

		private BDic<PrEggManager.CATEG, int> Oegg_layed;

		private int pee_count;

		public readonly EpSituation SituCon;

		private int orgasm_individual_count;

		private float t_corrupt_gacha_mist;

		public bool splash_lock;

		private float t_orgasm_cutin_tikatika;

		private const float T_ORGASM_CUTIN_TIKA_FCNT = 0.0033333334f;

		private const float ORGASM_CUTIN_TIKA_THRESHOLD = 20f;

		private const float ORGASM_CUTIN_TIKA_THRESHOLD_NOTALIVE = 8f;

		private float[] Ases_orgasmable;

		private EpSuppressor Suppressor;

		private EPCATEG_BITS bt_exp_added;

		private EPCATEG_BITS bt_applied;

		private EPCATEG_BITS bt_orgasm;

		private EPCATEG_BITS bt_corrupt_acmemist;

		private bool orgasm_oazuke;

		public float cure_ep_after_orgasm;

		public float cure_ep_after_orgasm_one;

		private const float orgasm_gacha_appear_time = 40f;

		private AbsorbManager AbsorbOrgasm;

		private float gacha_hit_stocked;

		public bool attachable_ser_oazuke;

		private float ep;

		private float t_oazuke;

		private int crack_cure_count;

		private int crack_cure_once;

		private float t_crack_cure;

		private float t_ptc;

		private Material MtrHeartPattern;

		private readonly EffectHandlerPE PeZoom;

		private readonly EffectHandler<EffectItem> EhBounce;

		private readonly EffectHandlerPE PeSlow;

		private SlotDrawer PeeSlotDr;

		private const float announce_ratio_0 = 0.5f;

		private const float announce_ratio_1 = 0.85f;

		private float t_sage;

		private float t_lock;

		private float t_lock_orgasmlocked;

		private int multiple_orgasm;

		private float[] At_announce;

		private EffectItem EfUi;

		private int efui_index = -1;

		private int t_ptc_ui;

		public readonly EpOrgasmCutin Cutin;

		private string breath_key_ = "";

		private FnEffectRun FD_drawEffect;

		public bool go_unlock_thresh;

		private struct OrgasmInfo
		{
			public EPCATEG categ;

			public string situation;
		}
	}
}
