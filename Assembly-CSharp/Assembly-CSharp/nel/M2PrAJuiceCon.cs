using System;
using evt;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	internal sealed class M2PrAJuiceCon : M2PrAssistant
	{
		public int water_drunk { get; private set; }

		public M2PrAJuiceCon(PR _Pr)
			: base(_Pr)
		{
		}

		public override void newGame()
		{
			base.newGame();
			this.water_drunk = 30;
			this.water_drunk_cache = 0;
			this.pee_lock = 0;
			this.juice_stock = 0;
		}

		public void cureFromEvent(int min_water_drunk = 15)
		{
			this.water_drunk = X.Mn(min_water_drunk, this.water_drunk);
			this.water_drunk_cache = 0;
		}

		public int cureOnBench(bool with_excrete)
		{
			int num = 0;
			if (this.water_drunk > 0)
			{
				num = X.Mx(this.water_drunk, num);
				this.water_drunk = 0;
			}
			if (this.water_drunk_cache > 0)
			{
				num = X.Mx(this.water_drunk_cache, num);
				this.water_drunk_cache = 0;
			}
			base.Ser.Cure(SER.NEAR_PEE);
			if (with_excrete)
			{
				if (this.Pr.Ser.has(SER.DRUNK))
				{
					this.Pr.cureSerDrunk1(3000f);
					num = X.Mx(num, 1);
				}
				Stomach stmNoel = this.Pr.NM2D.IMNG.StmNoel;
				if (stmNoel.eaten_anything)
				{
					num = X.Mx(stmNoel.eaten_item_count, num);
					stmNoel.clear();
					stmNoel.finePrStatus(false);
				}
			}
			return num;
		}

		public void SplashFromEvent(StringHolder rER)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			if (rER.clength >= 2)
			{
				flag = rER._1.IndexOf("F") >= 0;
				flag2 = rER._1.IndexOf("N") >= 0;
				flag3 = rER._1.IndexOf("P") >= 0;
				flag4 = rER._1.IndexOf("C") >= 0;
				flag5 = rER._1.IndexOf("J") >= 0;
				flag6 = rER._1.IndexOf("B") >= 0;
			}
			if (flag5)
			{
				UIPictureBase.EMWET emwet;
				if (!FEnum<UIPictureBase.EMWET>.TryParse(rER._2, out emwet, true))
				{
					emwet = UIPictureBase.EMWET.PEE;
				}
				this.obtainSplashedNoelJuice(emwet, rER.Int(3, 0));
				return;
			}
			if (this.juice_stock > 0 || flag)
			{
				this.executeSplashNoelJuice(false, true, 0, flag3, flag2, flag6, flag4);
			}
		}

		public string getOmorashiEventName()
		{
			if (!this.Pr.UP.isActive() || EnemySummoner.isActiveBorder())
			{
				return null;
			}
			string text = ((base.NM2D.WM.CurWM != null && !base.NM2D.FlgWarpEventNotInjectable.isActive() && !this.Pr.isMasturbating()) ? base.NM2D.WM.CurWM.ev_omorashi : null);
			if (!TX.valid(text))
			{
				return null;
			}
			if (EV.isActive(false))
			{
				return null;
			}
			return text;
		}

		public void SerApplyNearPee(int ser_level)
		{
			if (ser_level >= 1 && this.water_drunk < 93)
			{
				this.water_drunk = 93;
				base.Ser.checkSerExecute(true, true);
				return;
			}
			if (this.water_drunk < 72)
			{
				this.water_drunk = 72;
				base.Ser.checkSerExecute(true, true);
			}
		}

		public void updateWaterDrunkFromEvent(int value)
		{
			this.Pr.JuiceCon.water_drunk = X.Mx(value, this.Pr.JuiceCon.water_drunk);
			this.Pr.Ser.checkSer();
		}

		public int ser_near_pee_level
		{
			get
			{
				if (this.water_drunk >= 93)
				{
					return 2;
				}
				if (this.water_drunk < 72)
				{
					return 0;
				}
				return 1;
			}
		}

		public Stomach MyStomach
		{
			get
			{
				return this.Pr.MyStomach;
			}
		}

		public void addWaterDrunkCache(float stomach_progress, float water_total, int _juice_server = -1)
		{
			this.water_drunk_cache += (int)(stomach_progress * 100f);
			float num = 0f;
			if (_juice_server < 0)
			{
				_juice_server = (base.getEH(ENHA.EH.juice_server) ? 1 : 0);
			}
			float num2 = X.NI(600, 200, (_juice_server > 0) ? (this.MyStomach.hasWater() ? 1f : 0.3f) : 0f);
			if ((float)this.water_drunk_cache >= num2)
			{
				num = X.NIL(0.4f, 1.4f, (float)this.water_drunk_cache - num2, 900f);
			}
			if (this.water_drunk >= 93)
			{
				num = X.Mx(num, 0.7f);
			}
			else if (this.water_drunk >= 72)
			{
				num = X.Mx(num, 0.25f);
			}
			if (num > 0f)
			{
				this.PeeLockReduceCheck(num);
				this.progressWaterDrunkCache(stomach_progress * num, false, false);
			}
		}

		public void progressWaterDrunkCache(float stomach_progress, bool is_battle_finish = false, bool is_water_drunk = false)
		{
			if (is_battle_finish)
			{
				stomach_progress = X.Mx(stomach_progress, 2f);
			}
			int num = (int)X.Mn((float)this.water_drunk_cache, stomach_progress * 100f);
			this.water_drunk_cache -= num;
			int num2 = X.IntC((float)num / 100f * 9f * (is_water_drunk ? 2.5f : 1f));
			if (is_battle_finish)
			{
				if (num2 < 20 && this.water_drunk < 80 && X.XORSP() < 0.66f)
				{
					num2 = X.Mx(num2, X.IntR(X.NIXP(5f, 10f)));
				}
				if (this.water_drunk >= 72)
				{
					num2 = X.Mx(num2, 5);
				}
			}
			bool flag = this.water_drunk >= 72;
			bool flag2 = this.water_drunk >= 93;
			int num3 = X.Mx(0, X.Mn(100 - this.water_drunk, num2));
			this.water_drunk += num3;
			this.juice_stock = X.Mn(NelItem.NoelJuice.stock - 1, this.juice_stock + (num2 - num3) / 8);
			bool flag3 = this.water_drunk >= 72;
			bool flag4 = this.water_drunk >= 93;
			base.Ser.checkSer();
			if (flag4 && !flag2)
			{
				UILog.Instance.AddAlert(TX.Get("Alert_almost_pee", ""), UILogRow.TYPE.ALERT);
				return;
			}
			if (flag3 && !flag)
			{
				UILog.Instance.AddAlert(TX.Get("Alert_near_pee", ""), UILogRow.TYPE.ALERT);
			}
		}

		private float drunk_basic_level(bool is_alive)
		{
			float num;
			if (is_alive)
			{
				if (this.water_drunk >= 93)
				{
					num = 0.45f;
				}
				else if (this.water_drunk >= 72)
				{
					num = 0.22f;
				}
				else
				{
					num = 0.1f;
				}
			}
			else if (this.water_drunk >= 93)
			{
				num = 1f;
			}
			else if (this.water_drunk >= 72)
			{
				num = 0.6f;
			}
			else
			{
				num = 0.33f;
			}
			return num * X.NIL(0.5f, 1f, (float)this.water_drunk, 100f);
		}

		public bool checkNoelJuice(float basic_rat100, bool force_pee_splach = false, bool execute = true, int eh_juice_server = -1)
		{
			float num = X.Scr(basic_rat100 / 150f, this.drunk_basic_level(base.is_alive) * 60f / 100f);
			if (eh_juice_server < 0)
			{
				eh_juice_server = (base.getEH(ENHA.EH.juice_server) ? 1 : 0);
			}
			if (eh_juice_server != 0)
			{
				num = X.Scr(num, 0.15f * (this.MyStomach.hasWater() ? 1f : 0.5f));
			}
			if (X.XORSP() < num)
			{
				this.juice_stock++;
				if (this.juice_stock >= NelItem.NoelJuice.stock)
				{
					if (base.Ser.getLevel(SER.NEAR_PEE) >= 2)
					{
						execute = false;
					}
					if (execute)
					{
						this.executeSplashNoelJuice(false, force_pee_splach, 0, false, false, false, false);
					}
					else
					{
						this.juice_stock = NelItem.NoelJuice.stock - 1;
					}
					return true;
				}
			}
			return false;
		}

		public void checkNoelJuiceOrgasm(float basic_rat01, bool no_item = false)
		{
			float num = basic_rat01 * X.NI(0.3f, 1f, X.XORSP()) * 0.4f;
			float num2 = this.drunk_basic_level(false) * 75f / 100f + X.Mx((!base.is_alive) ? ((float)NelItem.NoelJuice.stock * X.NIXP(0.4f, 1f)) : 0f, (float)this.juice_stock) / (float)NelItem.NoelJuice.stock;
			float num3 = X.Scr(num, num2);
			if (base.getEH(ENHA.EH.juice_server))
			{
				num3 = X.Scr(num3, 0.25f);
			}
			if (X.XORSP() < num3)
			{
				this.executeSplashNoelJuice(num > num2, false, 0, false, false, no_item, false);
			}
		}

		public void executeSplashNoelJuice(bool orgasm_splash = false, bool force_pee_splash = false, int quality_add = 0, bool no_ptc = false, bool no_snd = false, bool no_item = false, bool no_cutin = false)
		{
			bool flag = false;
			UIPictureBase.EMWET emwet;
			if (base.Ser.has(SER.MILKY))
			{
				emwet = UIPictureBase.EMWET.MILK;
			}
			else if (CFGSP.publish_juice == 0 && CFGSP.publish_milk == 0)
			{
				emwet = UIPictureBase.EMWET.NORMAL;
				no_item = true;
			}
			else if (CFGSP.publish_juice == 0)
			{
				emwet = UIPictureBase.EMWET.MILK;
			}
			else if (CFGSP.publish_milk == 0)
			{
				emwet = UIPictureBase.EMWET.NORMAL;
			}
			else
			{
				emwet = ((X.xors((int)(CFGSP.publish_milk + CFGSP.publish_juice)) < (int)CFGSP.publish_milk) ? UIPictureBase.EMWET.MILK : UIPictureBase.EMWET.NORMAL);
			}
			if (base.NM2D.no_publish_juice)
			{
				no_item = true;
			}
			if (!no_item && !base.isPuzzleManagingMp() && (this.IMNG.hasEmptyBottle() || base.NM2D.isSafeArea()))
			{
				flag = true;
				this.obtainSplashedNoelJuice(emwet, quality_add);
			}
			this.juice_stock = 0;
			if (!X.SENSITIVE && this.Pr.EpCon.isOrgasmStarted(90) && X.XORSP() < 0.8f)
			{
				orgasm_splash = true;
			}
			string omorashiEventName = this.getOmorashiEventName();
			int num = this.water_drunk;
			if (!X.SENSITIVE && orgasm_splash)
			{
				num = -1;
				UILog.Instance.AddAlertTX((emwet == UIPictureBase.EMWET.MILK) ? "Alert_orgasm_splash_milk" : "Alert_orgasm_splash", UILogRow.TYPE.ALERT_EP);
			}
			else if (!X.SENSITIVE && (num > 0 || !flag || force_pee_splash))
			{
				UILog.Instance.AddAlertTX((emwet == UIPictureBase.EMWET.MILK) ? "Alert_pee_milk" : "Alert_pee", UILogRow.TYPE.ALERT_GRAY);
				num = 1;
			}
			else if (flag)
			{
				UILog.Instance.AddLog(TX.Get("Log_supply_juice", ""));
			}
			else if (this.water_drunk < 72 && TX.noe(omorashiEventName))
			{
				if (X.sensitive_level >= 1)
				{
					this.water_drunk += 5;
					base.Ser.checkSer();
				}
				return;
			}
			if (emwet == UIPictureBase.EMWET.NORMAL && num > 0)
			{
				emwet = UIPictureBase.EMWET.PEE;
			}
			this.Pr.EpCon.addPeeCount();
			if (!base.NM2D.no_publish_juice)
			{
				this.pee_lock = 5;
			}
			this.splashNoelJuiceEffect(emwet, num, no_ptc, no_snd, no_cutin);
			if (TX.valid(omorashiEventName))
			{
				EV.stack(omorashiEventName, 0, -1, base.Mp.Meta.Get("ev_omorashi_keys"), null);
			}
		}

		public void obtainSplashedNoelJuice(UIPictureBase.EMWET type, int quality_add = 0)
		{
			if (!base.is_alive)
			{
				return;
			}
			NelItem nelItem = ((type == UIPictureBase.EMWET.MILK) ? NelItem.NoelMilk : NelItem.NoelJuice);
			this.IMNG.dropManual(nelItem, nelItem.stock, quality_add + this.IMNG.getNoelJuiceQuality(1.35f) + this.Pr.EpCon.getNoelJuiceQualityAdd(), base.x, base.y, 0f, 0f, new META("force_absorb 1\ndrop_invisible 1\nno_get_effect 1"), false, NelItemManager.TYPE.NORMAL);
		}

		private void splashNoelJuiceEffect(UIPictureBase.EMWET wet_type, int ef_water_drunk = -1000, bool no_ptc = false, bool no_snd = false, bool no_cutin = false)
		{
			if (ef_water_drunk == -1000)
			{
				ef_water_drunk = this.water_drunk;
			}
			if (!base.LockCntOccur.isLocked(PR.OCCUR.HITSTOP_NOELJUICE))
			{
				base.LockCntOccur.Add(PR.OCCUR.HITSTOP_NOELJUICE, 450f);
				PostEffect.IT.setSlow(70f, 0.125f, 0);
			}
			Vector3 hipPos = this.Pr.getHipPos();
			this.Pr.defineParticlePreVariableVagina();
			base.Mp.getEffect();
			if (!no_ptc)
			{
				base.PtcHld.Var("drunked", (double)ef_water_drunk).Var("_dx", (double)hipPos.x).Var("_dy", (double)hipPos.y);
				base.PtcHld.PtcSTTimeFixed((!X.SENSITIVE) ? "noel_juice_filled" : "noel_juice_filled_sensitive", 1f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW, false);
			}
			if (!no_snd)
			{
				base.playSndPos("sxx_paizuri", 16);
				base.playSndPos((wet_type == UIPictureBase.EMWET.MILK) ? "sxx_sperm_shot_delayed" : "split_pee", 16);
			}
			this.Pr.EpCon.SituCon.addTempSituation((wet_type == UIPictureBase.EMWET.MILK) ? "_MILK" : "_PEE", 1, false);
			if (!base.LockCntOccur.isLocked(PR.OCCUR.NOELJUICE_ZOOMIN))
			{
				base.LockCntOccur.Add(PR.OCCUR.NOELJUICE_ZOOMIN, 200f);
				PostEffect.IT.addTimeFixedEffect(base.NM2D.Cam.TeCon.setBounceZoomIn(1.5f, 40f, 0), 0.5f);
				PostEffect.IT.addTimeFixedEffect(base.NM2D.Cam.TeCon.setBounceZoomIn(1.2f, 10f, 0), 1f);
			}
			int num = X.Mn(this.water_drunk, 50);
			this.water_drunk -= num;
			int num2 = 50 - num;
			this.water_drunk_cache = (int)X.Mx(0f, (float)this.water_drunk_cache - (float)(num2 * 100) / 9f);
			float num3;
			this.MyStomach.progress((float)num * 0.05f + (float)num2 * 0.08f, true, out num3, true, true);
			if (!base.is_alive && X.XORSP() < 0.2f && this.water_drunk == 0)
			{
				this.water_drunk += 20;
			}
			base.Ser.Add(SER.SHAMED_WET, -1, 99, false);
			base.Ser.checkSer();
			this.Pr.VO.playJuiceVo();
			if (!X.SENSITIVE)
			{
				this.Pr.BetoMng.wetten = true;
				base.NM2D.IMNG.fineSpecialNoelRow(this.Pr);
			}
			if (!no_cutin)
			{
				this.Pr.UP.applyWetten(wet_type, false);
			}
			if (base.EggCon.no_getout_exist)
			{
				base.EggCon.forcePushout(true, false);
			}
		}

		public void PeeLockReduceCheck(float ratio)
		{
			if (base.getEH(ENHA.EH.juice_server))
			{
				ratio = X.Scr(ratio, 0.15f);
			}
			if (this.pee_lock > 0 && X.XORSP() < ratio)
			{
				this.pee_lock -= 1;
			}
		}

		public void initPublishKill(M2MagicCaster Target)
		{
			if (this.pee_lock > 0 && Target is NelEnemy)
			{
				this.PeeLockReduceCheck(0.66f);
			}
		}

		public NelItemManager IMNG
		{
			get
			{
				return base.NM2D.IMNG;
			}
		}

		public void applyYdrgDamage(NelAttackInfo Atk)
		{
			if (this.water_drunk < 70)
			{
				this.water_drunk += 3;
				base.Ser.checkSer();
			}
		}

		public void applyDamage(NelAttackInfo Atk)
		{
			if (this.pee_lock > 0)
			{
				if (Atk.Caster is NelEnemy)
				{
					this.PeeLockReduceCheck(0.05f);
					return;
				}
			}
			else
			{
				X.Mx(Atk.pee_apply100, X.ZPOW(this.MyStomach.water_drunk_level - 3.5f, 10f) * 70f);
				if (Atk.pee_apply100 > 0f)
				{
					this.checkNoelJuice(Atk.pee_apply100, true, true, -1);
				}
			}
		}

		public void applyAbsorbDamage(NelAttackInfo Atk)
		{
			if (Atk.Caster as NelEnemy != null)
			{
				this.PeeLockReduceCheck(0.025f);
			}
		}

		public void splitMpByDamage(bool apply_noeljuice)
		{
			if (this.water_drunk >= 72)
			{
				apply_noeljuice = true;
			}
			if (apply_noeljuice)
			{
				this.checkNoelJuice((float)(this.Pr.isAbsorbState() ? (base.is_alive ? 4 : 1) : 3), false, true, -1);
			}
		}

		public void readFromBytes(ByteReader Ba, int vers)
		{
			this.water_drunk = Ba.readInt();
			this.water_drunk_cache = Ba.readInt();
			this.juice_stock = (int)Ba.readUByte();
			this.pee_lock = Ba.readUByte();
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeInt(this.water_drunk);
			Ba.writeInt(this.water_drunk_cache);
			Ba.writeByte(this.juice_stock);
			Ba.writeByte((int)this.pee_lock);
		}

		public void readFromOldData(int water_drunk, int water_drunk_cache, int juice_stock, byte pee_lock)
		{
			this.water_drunk = water_drunk;
			this.water_drunk_cache = water_drunk_cache;
			this.juice_stock = juice_stock;
			this.pee_lock = pee_lock;
		}

		public int water_drunk_cache;

		public int juice_stock;

		public byte pee_lock;

		private const float stomach_progress_to_water_drunk_add = 9f;

		private const float stomach_overdrunk_multiple = 2.5f;

		public const int PEE_LOCK_MAX = 5;
	}
}
