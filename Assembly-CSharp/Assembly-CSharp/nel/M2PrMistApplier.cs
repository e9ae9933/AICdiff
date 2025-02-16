using System;
using m2d;
using XX;

namespace nel
{
	public sealed class M2PrMistApplier : M2PrAssistant, IIrisOutListener
	{
		public M2PrMistApplier(PR _Pr)
			: base(_Pr)
		{
			this.o2_point = 100f;
			this.t_water = 0f;
			this.HnPe = new EffectHandlerPE(2);
		}

		public void cureAll()
		{
			this.o2_point = 100f;
			this.t_water = (this.t_wd = 0f);
			this.ui_need_fine = true;
		}

		public M2PrMistApplier activate()
		{
			this.HnPe.release(false);
			this.HnPe.Set(this.PEBreatheStop = this.NM2D.PE.setPE(POSTM.GAS_APPLIED, 20f, 0.125f, 0));
			this.ui_need_fine = true;
			this.t_br = 1f;
			this.Pr.recheck_emot = true;
			return this;
		}

		public void deactivateEffect()
		{
			this.HnPe.deactivate(true);
			this.PEBreatheStop = null;
			this.PEWaterChokeZoom = null;
		}

		public M2PrMistApplier deactivate()
		{
			this.deactivateEffect();
			this.abortWaterChokeRelease();
			this.newGame();
			this.choken_released = false;
			this.Pr.recheck_emot = true;
			this.fineUi();
			if (this.LogRowChoking != null)
			{
				this.LogRowChoking.deactivate(false);
				this.LogRowChoking = null;
			}
			return this;
		}

		public void abortWaterChokeRelease()
		{
			this.NM2D.Iris.deassignListener(this);
			base.remD(M2MoverPr.DECL.WATER_CHOKE_FLOAT);
			this.removePe(ref this.PEWaterChokeZoom);
			this.Pr.recheck_emot = true;
			if (this.Pr.isWaterChokeState())
			{
				this.Pr.changeWaterChokeRelease();
			}
		}

		public void waterReleasedInChoking()
		{
			base.remD(M2MoverPr.DECL.WATER_CHOKE_FLOAT);
			if (this.isWaterChokeDamageAlreadyApplied())
			{
				this.choken_released = true;
				this.t_water = X.Mn(100f, this.t_water);
				return;
			}
			this.t_water = X.Mn(this.t_water, 150f);
		}

		public override void newGame()
		{
			this.o2_point = 100f;
			this.t_br = -1f;
			this.t_breathe_snd = 0f;
			this.t_water = (this.t_wd = 0f);
		}

		public bool setPE(ref PostEffectItem Pe, POSTM postm, float z_maxt = 40f, float x_level = 1f, int saf = 0)
		{
			if (!this.NM2D.Cam.isBaseMover(this.Pr))
			{
				return false;
			}
			if (Pe != null)
			{
				return true;
			}
			this.HnPe.deactivateSpecific(postm);
			EffectHandler<PostEffectItem> hnPe = this.HnPe;
			PostEffectItem postEffectItem;
			Pe = (postEffectItem = this.NM2D.PE.setPE(postm, z_maxt, x_level, saf));
			hnPe.Set(postEffectItem);
			return true;
		}

		private void removePe(ref PostEffectItem Pe)
		{
			if (Pe != null)
			{
				this.HnPe.deactivateSpecific(Pe);
				Pe = null;
			}
		}

		public M2PrMistApplier run(ref M2MoverPr.PR_MNP manip, float fcnt)
		{
			float num = 0.5f;
			float num2 = -1f;
			float num3 = this.t_water - 200f;
			if (num3 >= 0f)
			{
				float num4 = num3 - 100f;
				int num5 = this.Pr.applyWaterChokeDamage(num4 < 0f, !this.choken_released);
				if (num5 < 0)
				{
					this.t_water = 155f;
					num3 = this.t_water - 200f;
				}
				else
				{
					if (num4 < 0f && num5 == 1)
					{
						this.NM2D.Iris.assignListener(this);
						this.t_water = 300f;
						num3 = this.t_water - 200f;
						num4 = 0f;
						float num6;
						float num7;
						this.Pr.getMouthPosition(out num6, out num7);
						this.Pr.PtcVar("mx", (double)num6).PtcVar("my", (double)num7).PtcST("water_choked", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Pr.getFootManager().lockPlayFootStamp(70);
						this.Pr.UP.applyDamage(MGATTR.NORMAL, 2f, 22f, UIPictureBase.EMSTATE.NORMAL, false, null, false);
						this.choken_released = true;
						this.Pr.getFootManager().lockPlayFootStamp(10);
					}
					num2 = 1.5f - X.ZSIN(num4, 85f) * 1.125f;
					if (num4 <= 70f)
					{
						if (num4 >= 0f)
						{
							this.t_water = X.Mn(this.t_water + fcnt, 370f);
						}
					}
					else
					{
						this.t_water += fcnt;
					}
				}
			}
			else
			{
				if (this.t_br > 0f)
				{
					this.setPE(ref this.PEBreatheStop, POSTM.GAS_APPLIED, 20f, 0.125f, 0);
					if (this.Pr.isShieldOpening() && this.t_br <= 6f)
					{
						num = ((this.t_water > 0f || this.t_wd != 0f) ? 0.375f : 0.25f);
					}
					if (!this.Pr.isTrappedState() && this.t_wd >= 0f && (this.Pr.isShieldOpening() || base.isNormalBusy()))
					{
						if (this.t_br > 6f)
						{
							this.t_br = X.Mx(this.t_br - fcnt, 6f);
						}
					}
					else
					{
						this.t_br = X.Mx(0f, this.t_br - fcnt);
					}
					this.t_mdmg_ac_reduce = 0f;
				}
				else
				{
					float num8 = fcnt;
					float num9 = (((!base.isNormalState() && !this.Pr.isDownState() && !this.Pr.isWaterChokedReleaseState()) || this.Pr.magic_chanting) ? 0.125f : ((this.Pr.get_walk_xspeed() != 0f || !this.Pr.canJump()) ? 0.41666666f : 1f));
					if (this.o2_point < 100f)
					{
						if (this.LogRowChoking != null)
						{
							this.LogRowChoking.deactivate(false);
							this.LogRowChoking = null;
						}
						if (this.t_water > 0f && this.o2_point >= 50f)
						{
							num9 *= X.NI(1f, 0.5f, X.ZLINE(this.t_water, 50f));
						}
						this.o2_point = X.Mn(this.o2_point + num9 * fcnt, 100f);
						if (num9 > 0f && this.o2_point > 12.5f && this.Pr.isWaterChokeState())
						{
							this.abortWaterChokeRelease();
						}
						this.ui_need_fine = true;
					}
					else if (num9 == 1f)
					{
						num8 = fcnt * 4f;
					}
					if (this.mdmg_count > 0)
					{
						this.t_mdmg_ac_reduce += num8;
						if (this.t_mdmg_ac_reduce > 150f)
						{
							this.t_mdmg_ac_reduce -= 150f;
							this.mdmg_count -= 1;
						}
					}
					if (this.o2_point >= 100f && this.t_water == 0f && this.t_wd == 0f && this.mdmg_count == 0)
					{
						this.deactivate();
						return null;
					}
				}
				if (this.isFloatingWaterChoke())
				{
					num2 = 0.5f;
				}
			}
			if (this.t_wd > 0f)
			{
				this.t_wd = X.Mx(this.t_wd - fcnt, 0f);
				if (this.t_wd == 0f && this.isFloatingWaterChoke())
				{
					this.Pr.applyWaterChokeDamage(false, false);
					this.waterReleasedInChoking();
				}
			}
			if (this.t_water > 0f)
			{
				if (this.setPE(ref this.PEWaterChokeZoom, POSTM.ZOOM2, 60f, 0.125f, 0))
				{
					this.PEWaterChokeZoom.x = X.ZLINE(this.t_water, 200f) * (1f - X.ZLINE(this.o2_point, 25f));
				}
				if (this.t_wd > 0f && num2 >= 0f)
				{
					base.addD(M2MoverPr.DECL.WATER_CHOKE_FLOAT);
					if (this.Phy.isin_water && base.mtop >= (float)(base.Mp.crop + 2) && CCON.isWater(base.Mp.getConfig((int)base.x, (int)(base.mtop - 0.8f))))
					{
						IFootable foot = this.Pr.getFootManager().get_Foot();
						if (foot == null || foot is M2BlockColliderContainer.BCCLine)
						{
							this.Phy.addLockGravity(this.FootD, 0f, 2f);
							this.FootD.initJump(false, true, false);
							this.FootD.lockPlayFootStamp(20);
							if (!this.Pr.forceCrouch(false, false))
							{
								this.Phy.addFoc(FOCTYPE.JUMP, 0f, -0.009f * num2, -1f, -1, 1, 0, -1, 0);
							}
						}
					}
				}
				else
				{
					base.remD(M2MoverPr.DECL.WATER_CHOKE_FLOAT);
				}
			}
			if (this.t_water != 0f && num3 <= 0f && this.t_br <= 0f && this.o2_point >= 100f * ((this.Pr.hasFoot() && this.Pr.isNormalState() && !this.Pr.magic_chanting) ? 0.0625f : (this.Pr.isWaterChokedReleaseState() ? 0.33f : 0.675f)))
			{
				this.t_water = X.VALWALK(this.t_water, 0f, fcnt * 4f);
				this.ui_need_fine = true;
				if (this.t_water == 0f)
				{
					this.abortWaterChokeRelease();
				}
			}
			if (this.PEBreatheStop != null)
			{
				this.PEBreatheStop.x = X.VALWALK(this.PEBreatheStop.x, 1f - (1f - X.NI(0.25f, num, (float)((this.t_br > 0f) ? 1 : 0)) * (1f - X.ZPOW(this.o2_point - 50f, 50f))) * (1f - X.ZLINE(this.t_water - 40f, 160f) * 0.25f - (1f - X.ZSIN2(num3 - 100f, 70f)) * 0.35f), 0.014f);
			}
			if (this.ui_need_fine)
			{
				this.ui_need_fine = false;
				if (UIStatus.isPr(this.Pr))
				{
					UIStatus.Instance.fineO2();
				}
			}
			if (this.HnPe.Count > 0)
			{
				if (!this.NM2D.Cam.isBaseMover(this.Pr))
				{
					this.deactivateEffect();
				}
				else
				{
					this.HnPe.fine(20);
				}
			}
			if (this.t_voice >= 0f)
			{
				if (!this.Pr.isNormalState() && !this.Pr.isAbsorbState() && !this.Pr.isDamagingOrKo())
				{
					this.t_voice = 35f;
				}
				else
				{
					int num10 = (int)((this.t_voice - 35f) / 25f);
					this.t_voice += fcnt;
					int num11 = (int)((this.t_voice - 35f) / 25f);
					if (num11 >= 1 && num10 != num11)
					{
						if (num11 <= 2 || (num11 == 3 && X.XORSP() < 0.5f))
						{
							this.Pr.playVo("cough", false, false);
						}
						if (num11 >= 5)
						{
							this.t_voice = -1f;
						}
					}
				}
			}
			if (this.LogRowChoking != null && this.LogRowChoking.isHiding())
			{
				this.LogRowChoking = null;
			}
			return this;
		}

		public int applyGasDamage(MistManager.MistKind Mist, float level01)
		{
			if (base.Mp.floort < 40f)
			{
				return 0;
			}
			if (!this.isActive())
			{
				this.activate();
			}
			if (Mist.isWater())
			{
				this.t_wd = 7f;
				this.t_breathe_snd = 0f;
				if (this.Pr.Ser.has(SER.BURNED))
				{
					this.Pr.Ser.Cure(SER.BURNED);
				}
			}
			if (this.t_water >= 200f)
			{
				return 0;
			}
			if (Mist.damage_cooltime < 0 || X.DEBUGNODAMAGE)
			{
				this.t_br = X.Mx(5f, this.t_br);
				return 0;
			}
			this.t_br = X.Mx(this.t_br, 6f);
			if (level01 <= 0f || this.Pr.isShieldOpening() || !this.Pr.canApplyGasDamage())
			{
				UIStatus.Instance.showO2Gage(30);
				return 0;
			}
			if (this.t_br <= 6f)
			{
				if (this.o2_point > 0f)
				{
					float num = 1f;
					float num2;
					if (this.Pr.magic_chanting)
					{
						num2 = 3f;
						num = 0.5f;
					}
					else if (base.isDamagingOrKo() || this.Pr.isGaraakiState())
					{
						num2 = 2f;
						num = 0.7f;
					}
					else
					{
						num2 = 1f;
					}
					if (!Mist.isWater())
					{
						num2 *= X.NI(1f, 0.1f, this.Pr.getRE(RecipeManager.RPI_EFFECT.SMOKE_RESIST) * num);
					}
					if (num2 > 0f)
					{
						this.o2_point = X.Mx(0f, this.o2_point - 0.625f * num2 * 5f);
						this.ui_need_fine = true;
						if (this.o2_point > (float)Mist.apply_o2)
						{
							this.Pr.UP.applyGasDamage(Mist, false);
							return 0;
						}
					}
				}
				this.Pr.stopRunning(false, false);
				this.Pr.Skill.initMagicSleep(true, false);
				int num3 = 0;
				if (Mist.isWater())
				{
					if (this.t_water == 0f)
					{
						this.Pr.UP.applyGasDamage(Mist, true);
					}
					else if (this.t_water < 0f)
					{
						this.t_water = -this.t_water;
					}
					this.t_water += this.Pr.TS * 5f;
					if (!base.Mp.canHandle() && this.t_water < 200f)
					{
						this.t_water = X.Mn(this.t_water, 100f);
					}
					this.t_voice = -1f;
					this.ui_need_fine = true;
					this.Pr.Ser.Cure(SER.SLEEP);
					if (this.t_water >= 200f)
					{
						this.initChoke();
					}
				}
				else
				{
					this.t_br = 28f;
					if (!base.isNormalState() && base.isNormalBusy())
					{
						this.Pr.changeState(PR.STATE.NORMAL);
					}
					if (this.t_voice < 0f)
					{
						this.t_voice = 59f;
					}
					this.NM2D.Cam.Qu.SinV(6f, 50f, 0f, 0);
					this.NM2D.Cam.Qu.SinH(3f, 75f, 0f, -40);
					this.NM2D.PE.setPEabsorbed(POSTM.GAS_APPLIED, 2f, 40f, 0.5f, 0);
					this.t_mdmg_ac_reduce = 0f;
					this.Pr.UP.applyGasDamage(Mist, true);
					if (DIFF.mist_damage_applyable(this.Pr, (int)this.mdmg_count + Mist.adding_damage_count))
					{
						if (Mist.fnApply != null)
						{
							Mist.fnApply(Mist, this.Pr, level01);
						}
						num3 = 1;
					}
					if (this.mdmg_count < 100)
					{
						this.mdmg_count += 1;
					}
				}
				return num3;
			}
			this.Pr.UP.applyGasDamage(Mist, false);
			if (UIStatus.isPr(this.Pr))
			{
				UIStatus.Instance.showO2Gage(30);
			}
			return 0;
		}

		public bool initSubmitIrisOut(PR Pr, bool execute, ref bool no_iris_out)
		{
			if (Pr != this.Pr)
			{
				return false;
			}
			if (this.t_water >= 370f || Pr.isWaterChokeState())
			{
				if (execute)
				{
					this.t_water = 371f;
				}
				return true;
			}
			return false;
		}

		public bool warpInitIrisOut(PR Pr, ref PR.STATE changestate, ref string change_pose)
		{
			if (Pr != this.Pr)
			{
				return false;
			}
			if (this.t_water >= 200f || Pr.isWaterChokeState())
			{
				changestate = PR.STATE.WATER_CHOKED_RELEASE;
				change_pose = "water_choked_release2";
				this.waterReleasedInChoking();
				if (this.PEWaterChokeZoom != null)
				{
					this.PEWaterChokeZoom.x = 1f;
				}
			}
			return this.t_water > 0f;
		}

		public IrisOutManager.IRISOUT_TYPE getIrisOutKey()
		{
			return IrisOutManager.IRISOUT_TYPE.WATER;
		}

		public void showChokingLog()
		{
			if (this.LogRowChoking != null)
			{
				this.LogRowChoking.fineTime(false);
				return;
			}
			this.LogRowChoking = UILog.Instance.AddAlertTX("Damage_water_choke_in_absorbed", UILogRow.TYPE.ALERT_GRAY);
		}

		private void initChoke()
		{
			this.t_water = 200f;
		}

		public bool canOpenShield()
		{
			return this.t_br <= 20f && (this.t_water == 0f || this.t_br == 0f);
		}

		public bool isBreatheStop(bool ache = false, bool water_acke = false)
		{
			if (ache && water_acke && this.t_water > 0f)
			{
				return true;
			}
			if (!ache)
			{
				return this.t_br > 0f || this.t_water > 0f;
			}
			return this.t_br > 6f;
		}

		public void fineUi()
		{
			if (UIStatus.isPr(this.Pr))
			{
				UIStatus.Instance.fineO2(this.o2_point, X.Abs(this.t_water));
			}
		}

		public M2PrMistApplier destruct()
		{
			this.deactivate();
			return null;
		}

		public bool isWaterChoking()
		{
			return this.t_water > 0f;
		}

		public bool isWaterChokeDamageAlreadyApplied()
		{
			return this.t_water >= 300f || this.choken_released;
		}

		public bool canReleaseWaterChokeStun()
		{
			return this.t_water < 160f && !this.choken_released;
		}

		public bool waterDelayActive()
		{
			return this.t_wd > 0f;
		}

		public bool isCuring()
		{
			return this.t_wd <= 0f && this.t_br <= 0f;
		}

		public bool cannotRun()
		{
			return !this.canOpenShield();
		}

		public bool isFloatingWaterChoke()
		{
			return this.t_water >= 170f;
		}

		public bool isActive()
		{
			return this.t_br >= 0f;
		}

		public const int O2_MAX = 100;

		public const float O2_dmg = 0.625f;

		public const float O2_cure_stand = 1f;

		public const float O2_cure_walking = 0.41666666f;

		public const float O2_cure_not_normal = 0.125f;

		public const byte BREATHE_STOP_DELAY = 6;

		public const byte BREATHE_DAMAGE_LOCK = 45;

		public const int WATER_CHOKE_TIME = 200;

		public const float O2_cure_choking_ratio = 0.25f;

		public const int WATER_CHOKED_RELEASE_TIME = 120;

		public const float MAXT_MISTDMG_APPLY_COUNT_REDUCE = 150f;

		private const int input_time = 70;

		private UILogRow LogRowChoking;

		private float o2_point;

		private float t_br = -1f;

		private float t_water;

		private float t_wd;

		private float t_breathe_snd;

		private float t_mdmg_ac_reduce;

		private byte mdmg_count;

		private bool choken_released;

		private float t_voice = -1f;

		public bool ui_need_fine;

		private EffectHandlerPE HnPe;

		private PostEffectItem PEBreatheStop;

		private PostEffectItem PEWaterChokeZoom;
	}
}
