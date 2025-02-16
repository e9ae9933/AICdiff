using System;
using m2d;
using XX;

namespace nel
{
	public class PrVoiceController : M2VoiceController
	{
		public PrVoiceController(PR _Pr, VoiceController Src, string _unique_key)
			: base(_Pr, Src, _unique_key)
		{
			this.Pr = _Pr;
			this.EpCon = this.Pr.EpCon;
			this.AbsorbCon = this.Pr.getAbsorbContainer();
			this.Ser = this.Pr.Ser;
			this.LockBreathProgress = new Flagger(null, null);
		}

		public void newGame()
		{
			bool flag = this.LockBreathProgress.hasKey("EV");
			this.LockBreathProgress.Clear();
			this.breath_key_ = null;
			this.t_breath = -1;
			if (flag)
			{
				this.LockBreathProgress.Add("EV");
			}
		}

		public void recoverFromGameOver()
		{
			this.LockBreathProgress.Rem("EV");
		}

		public void evInit()
		{
			this.LockBreathProgress.Add("EV");
		}

		public void evQuit()
		{
			this.LockBreathProgress.Rem("EV");
		}

		public bool playVo(string family, bool no_use_post = false, bool force_mouth_override = false)
		{
			if (this.Pr.isFrozen())
			{
				byte frozen_dmg_voice = CFGSP.frozen_dmg_voice;
				if (frozen_dmg_voice <= 0 || X.XORSP() * 100f >= (float)frozen_dmg_voice)
				{
					return false;
				}
			}
			if (this.Pr.isStoneSer())
			{
				byte stone_dmg_voice = CFGSP.stone_dmg_voice;
				if (stone_dmg_voice <= 0 || X.XORSP() * 100f >= (float)stone_dmg_voice)
				{
					return false;
				}
			}
			if (family != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(family);
				if (num <= 1058693732U)
				{
					if (num <= 219797965U)
					{
						if (num != 35244156U)
						{
							if (num != 219797965U)
							{
								goto IL_0223;
							}
							if (!(family == "dmgx"))
							{
								goto IL_0223;
							}
							if (force_mouth_override || this.mouth_is_covered())
							{
								family = ((X.XORSP() < 0.2f) ? "mouth_fin" : "mouthl");
								goto IL_0223;
							}
							goto IL_0223;
						}
						else if (!(family == "dmgs"))
						{
							goto IL_0223;
						}
					}
					else if (num != 597717553U)
					{
						if (num != 1058693732U)
						{
							goto IL_0223;
						}
						if (!(family == "el"))
						{
							goto IL_0223;
						}
						goto IL_0185;
					}
					else
					{
						if (!(family == "cough"))
						{
							goto IL_0223;
						}
						if (this.isAnimationFrozen())
						{
							return false;
						}
						goto IL_0223;
					}
				}
				else
				{
					if (num <= 2341128772U)
					{
						if (num != 1176137065U)
						{
							if (num != 2341128772U)
							{
								goto IL_0223;
							}
							if (!(family == "orgasm"))
							{
								goto IL_0223;
							}
						}
						else
						{
							if (!(family == "es"))
							{
								goto IL_0223;
							}
							goto IL_016B;
						}
					}
					else if (num != 2602863636U)
					{
						if (num != 4179212881U)
						{
							goto IL_0223;
						}
						if (!(family == "dmgl"))
						{
							goto IL_0223;
						}
						goto IL_0185;
					}
					else if (!(family == "must_orgasm"))
					{
						goto IL_0223;
					}
					if (force_mouth_override || (this.mouth_is_covered() && X.XORSP() < X.NI(0.5f, 0.15f, (float)CFGSP.epdmg_vo_iku * 0.01f)))
					{
						family = "mouth_fin";
						goto IL_0223;
					}
					if (CFGSP.epdmg_vo_iku > 0 && X.xors(100) < (int)CFGSP.epdmg_vo_iku)
					{
						family = "orgasm_iku";
						goto IL_0223;
					}
					goto IL_0223;
				}
				IL_016B:
				if (force_mouth_override || this.mouth_is_covered())
				{
					family = "mouth";
					goto IL_0223;
				}
				goto IL_0223;
				IL_0185:
				if (force_mouth_override || this.mouth_is_covered())
				{
					family = "mouthl";
				}
			}
			IL_0223:
			return base.play(family, no_use_post) != null;
		}

		public void playAwkVo()
		{
			this.playVo(((this.Pr.EpCon.isOrgasm() || !this.is_alive) && X.XORSP() < 0.8f) ? "awkx" : "awk", false, false);
		}

		public void playDmgVo(NelAttackInfo Atk, int val, M2PrADmg.DMGRESULT res)
		{
			string text = "";
			bool flag = false;
			bool flag2 = false;
			if ((res & M2PrADmg.DMGRESULT._MAIN) == M2PrADmg.DMGRESULT.WORM)
			{
				text = ((X.XORSP() < 0.9f) ? "dmgl" : "dmgx");
				flag2 = true;
			}
			else if ((res & M2PrADmg.DMGRESULT._MAIN) == M2PrADmg.DMGRESULT.L)
			{
				flag = true;
				if (!this.is_alive)
				{
					if ((res & M2PrADmg.DMGRESULT._TO_DEAD) == M2PrADmg.DMGRESULT.MISS)
					{
						text = ((X.XORSP() < 0.9f) ? "dmgl" : "death");
					}
					else
					{
						text = "death";
					}
				}
				else
				{
					text = "dmgl";
				}
				if ((res & M2PrADmg.DMGRESULT._PENETRATE_ABSORB) != M2PrADmg.DMGRESULT.MISS)
				{
					text = "dmgx";
				}
				else if ((res & M2PrADmg.DMGRESULT._ABSORBING) != M2PrADmg.DMGRESULT.MISS)
				{
					flag2 = true;
					if (text == "dmgl" && (Atk.isAbsorbAttr() || (double)X.XORSP() < 0.4))
					{
						text = "el";
					}
				}
				if (Atk.attr == MGATTR.THUNDER && (double)X.XORSP() < 0.7)
				{
					text = "dmg_elec";
				}
			}
			else if ((res & M2PrADmg.DMGRESULT._MAIN) == M2PrADmg.DMGRESULT.S)
			{
				if (!this.is_alive)
				{
					if ((res & M2PrADmg.DMGRESULT._TO_DEAD) == M2PrADmg.DMGRESULT.MISS)
					{
						float num = X.XORSP();
						text = ((num < 0.14f) ? "dmgs" : ((num < 0.92f) ? "es" : "el"));
					}
					else
					{
						text = "el";
					}
				}
				else
				{
					text = "dmgs";
				}
				if ((res & M2PrADmg.DMGRESULT._PENETRATE_ABSORB) != M2PrADmg.DMGRESULT.MISS)
				{
					text = "el";
				}
				else if ((res & M2PrADmg.DMGRESULT._ABSORBING) != M2PrADmg.DMGRESULT.MISS)
				{
					flag2 = true;
					if (text == "dmgs" && (Atk.isAbsorbAttr() || (double)X.XORSP() < 0.4))
					{
						text = "es";
					}
				}
			}
			if ((float)val <= X.Mx(0f, 40f * (CFGSP.voice_for_pleasure - 0.8f) * X.ZLINE(this.ep - 300f, 300f)))
			{
				if (Atk.EpDmg != null && X.XORSP() < X.Scr(0.85f, X.ZLINE(CFGSP.voice_for_pleasure - 1f, 0.5f)))
				{
					if (flag && !this.AbsorbCon.isActive())
					{
						text = ((X.XORSP() < 0.6f) ? "el" : "es");
					}
					else
					{
						text = ((X.XORSP() < 0.15f) ? "el" : "es");
						if (this.vo_near_orgasm_replace())
						{
							text = this.vo_near_orgasm_2_must_replace();
						}
						if (this.EpCon.isOrgasm() && text == "es")
						{
							text = "after_orgasm";
						}
					}
				}
			}
			else if (!this.is_alive && text != "death" && base.isPlaying() && X.XORSP() < 0.6f)
			{
				text = "dmgx";
			}
			bool flag3 = false;
			if (text != null && (text == "dmgs" || text == "es" || text == "el") && Atk.EpDmg != null && Atk.EpDmg.only_anal && X.XORSP() < X.NIL(1.01f, 0.66f, this.ep - 600f, 400f))
			{
				text = "ehard";
			}
			if (CFGSP.epdmg_vo_mouth > 100 && flag2)
			{
				int num2 = (int)(CFGSP.epdmg_vo_mouth - 100);
				if (X.xors(120) < num2)
				{
					flag3 = true;
				}
			}
			if (TX.valid(text))
			{
				this.playVo(text, false, flag3);
			}
		}

		public void playWormTrapVo(NelAttackInfo Atk, int phase_count)
		{
			string text = ((phase_count < 0) ? "es" : ((phase_count == 0) ? "mouth_fin" : ((X.XORSP() < X.MulOrScr(0.125f, 2f - (float)CFGSP.epdmg_vo_mouth * 0.01f)) ? "es" : "mouth")));
			if (this.EpCon.isOrgasmInitTime())
			{
				text = "";
			}
			else if (!this.EpCon.isOrgasmInitTime())
			{
				if (Atk != null && Atk.EpDmg != null && this.vo_near_orgasm_replace())
				{
					text = this.vo_near_orgasm_2_must_replace();
				}
				else if (this.EpCon.isOrgasm() && (text == "es" || (text == "mouth" && X.XORSP() < 0.6f)))
				{
					text = "after_orgasm";
				}
			}
			if (!TX.noe(text))
			{
				this.playVo(text, false, false);
			}
		}

		public void playAbsorbDamageVo(NelAttackInfo Atk, bool execute_attack = false, bool mouth_damage = false, bool ep_added = false)
		{
			bool flag = execute_attack && X.XORSP() < 0.15f;
			float num = 0.88f;
			float num2 = 0f;
			if (mouth_damage && CFGSP.epdmg_vo_mouth > 100)
			{
				float num3 = (float)(CFGSP.epdmg_vo_mouth - 100) * 0.01f;
				num = X.Scr(num, num3);
				num2 = -num3 * 0.4f;
			}
			string text;
			if (mouth_damage && X.XORSP() < num)
			{
				text = (flag ? "mouthl" : "mouth");
			}
			else if (Atk.EpDmg != null && Atk.EpDmg.only_anal && X.XORSP() < X.NIL(1.01f, 0.66f, this.ep - 600f, 400f))
			{
				text = "ehard";
			}
			else
			{
				text = (flag ? "el" : "es");
			}
			if (ep_added && this.vo_near_orgasm_replace())
			{
				text = this.vo_near_orgasm_2_must_replace();
			}
			if (!this.EpCon.isOrgasmInitTime())
			{
				if (this.EpCon.isOrgasm() && (text == "es" || (text == "mouth" && X.XORSP() < 0.6f + num2)))
				{
					text = "after_orgasm";
				}
				if (text != null)
				{
					this.playVo(text, false, false);
				}
			}
		}

		public void playOrgasm(bool is_mastur)
		{
			this.playVo(is_mastur ? "must_orgasm" : "orgasm", false, false);
		}

		public void playJuiceVo()
		{
			this.playVo(this.getMouthVoiceReplace(0.7f, 1f) ? "mouth_split" : "split", false, false);
		}

		public void playParalysisVo()
		{
			float num = X.XORSP();
			this.playVo((num < 0.14f) ? "dmgs" : ((num < 0.92f) ? "es" : "el"), false, false);
		}

		public void playMgmEggRemoveVoIn()
		{
			if (X.XORSP() < 0.6f)
			{
				this.playVo(((float)this.Pr.ep >= 950000f) ? "mustll" : "must", false, false);
			}
		}

		public void playMgmEggRemoveVoOut()
		{
			this.playVo(((float)this.Pr.ep >= 950000f) ? "must_come" : "mustll", false, false);
		}

		public void playEggRemoveCutin(bool from_egg_remove)
		{
			this.playVo(from_egg_remove ? "dmgx_eggremove" : "dmgx", false, false);
		}

		public void playMarunomiBreath()
		{
			this.playVo(((float)this.Pr.ep < 500f) ? "breath_down" : (((float)this.Pr.ep >= 800f && X.XORSP() < 0.55f) ? "mustl" : "must"), false, false);
		}

		public void playMasturbVo(int level)
		{
			if (level == 3)
			{
				this.playVo("must_come", false, false);
				return;
			}
			if (level < 3)
			{
				this.playVo((level == 2) ? "mustll" : ((level == 1) ? "mustl" : "must"), false, false);
			}
		}

		public bool vo_near_orgasm_replace()
		{
			float voice_for_pleasure = CFGSP.voice_for_pleasure;
			bool flag;
			if (voice_for_pleasure == 0f)
			{
				flag = false;
			}
			else if (voice_for_pleasure <= 1f && (float)this.EpCon.getOrgasmedIndividualTotal() < 5f * (1f - X.ZLINE((float)CFGSP.epdmg_vo_iku * 0.01f - 0.25f, 0.75f)))
			{
				flag = false;
			}
			else
			{
				float num = 0.85f;
				float num2 = 0.6f;
				if (voice_for_pleasure > 1f)
				{
					num = X.NI(num, 0.3f, voice_for_pleasure - 1f);
					num2 = X.NI(num2, 1f, voice_for_pleasure - 1f);
				}
				else
				{
					num2 *= voice_for_pleasure;
				}
				flag = this.ep >= 1000f * num && X.XORSP() < num2;
			}
			return flag;
		}

		internal bool getMouthVoiceReplace(float base_ratio = 0.7f, float overwrite_sp_epdmg_vo_mouth_ratio = 1f)
		{
			bool flag = this.mouth_is_covered() && X.XORSP() < base_ratio * X.ZLINE((float)CFGSP.epdmg_vo_mouth, 100f);
			if (!flag && CFGSP.epdmg_vo_mouth > 100 && (this.Pr.isAbsorbState() || this.Pr.isWormTrapped()))
			{
				flag = (float)X.xors(100) < (float)(CFGSP.epdmg_vo_mouth - 100) * overwrite_sp_epdmg_vo_mouth_ratio;
			}
			return flag;
		}

		public string vo_near_orgasm_2_must_replace()
		{
			string text;
			if (CFGSP.voice_for_pleasure2m > 0f)
			{
				text = "near_orgasm";
			}
			else
			{
				float num = this.ep / 1000f;
				if (this.getMouthVoiceReplace(CFGSP.voice_for_pleasure2m, 0.6f + X.ZLINE(num - 0.5f, 0.5f) * 0.3f))
				{
					if ((float)X.xors(100) >= (float)CFGSP.epdmg_vo_iku * 0.25f)
					{
						return "must_mouth";
					}
					return "near_orgasm_iku";
				}
				else
				{
					float num2 = X.Scr(CFGSP.voice_for_pleasure2m, num * 0.125f);
					text = "near_orgasm";
					if (X.XORSP() < num2)
					{
						if (num >= 0.9f)
						{
							text = "must_come";
						}
						else if (num >= 0.75f)
						{
							text = "mustll";
						}
						else if (num >= 0.5f)
						{
							text = "mustl";
						}
						else
						{
							text = "must";
						}
					}
				}
			}
			if (text == "near_orgasm" && CFGSP.epdmg_vo_iku > 0 && X.xors(100) < (int)CFGSP.epdmg_vo_iku)
			{
				text = "near_orgasm_iku";
			}
			return text;
		}

		public void runUi()
		{
			if (this.t_breath >= 0 && !this.LockBreathProgress.isActive())
			{
				int num = this.t_breath - 1;
				this.t_breath = num;
				if (num <= 0)
				{
					if (!this.playVo(this.breath_key_, false, false))
					{
						this.t_breath = 40;
						return;
					}
					string text = this.breath_key_;
					if (text != null)
					{
						if (text == "breath_aft")
						{
							this.t_breath = (int)X.NIXP(90f, 104f);
							return;
						}
						if (text == "cough")
						{
							this.t_breath = (int)X.NIXP(20f, 30f);
							return;
						}
					}
					this.t_breath = (int)X.NIXP(80f, 90f);
				}
			}
		}

		public string breath_key
		{
			get
			{
				return this.breath_key_;
			}
			set
			{
				if (value == "")
				{
					value = null;
				}
				if (this.breath_key_ == value)
				{
					return;
				}
				this.breath_key_ = value;
				if (this.breath_key_ != null)
				{
					this.LockBreathProgress.Rem("EV");
					this.t_breath = (int)X.NIXP(10f, 30f);
					return;
				}
				this.t_breath = -1;
			}
		}

		public float ep
		{
			get
			{
				return (float)this.Pr.ep;
			}
		}

		public bool mouth_is_covered()
		{
			return this.Pr.mouth_is_covered();
		}

		public bool isAnimationFrozen()
		{
			return this.Pr.isAnimationFrozen();
		}

		public bool is_alive
		{
			get
			{
				return this.Pr.is_alive;
			}
		}

		public readonly PR Pr;

		public readonly M2Ser Ser;

		public readonly EpManager EpCon;

		public readonly AbsorbManagerContainer AbsorbCon;

		public readonly Flagger LockBreathProgress;

		private int t_breath = -1;

		private string breath_key_;
	}
}
