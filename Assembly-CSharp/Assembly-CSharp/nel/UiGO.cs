using System;
using System.Collections.Generic;
using evt;
using m2d;
using nel.gm;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiGO : IEventWaitListener, IEventListener
	{
		public UiGO(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.Pr = this.M2D.getPrNoel();
			EV.addWaitListener("NM2D_GAMEOVER", this);
			this.EfHn = new EffectHandlerPE(2);
			this.Log_Death = UILog.Instance.AddAlertTX("Alert_gameover", UILogRow.TYPE.ALERT_HUNGER);
			this.At_input_ui = new int[3];
			EV.addListener(this);
			if (CFGSP.gameover_counter > 0)
			{
				this.prepareContinuer();
				IN.PosP(this.Continuer.transform, this.M2D.ui_shift_x, 0f, -3.2f);
				this.t = -90f;
			}
			this.PeZoom = PostEffect.IT.setPE(POSTM.ZOOM2, 150f, (float)((this.t < 0f) ? 0 : 1), 30);
		}

		public void initS()
		{
			this.EfHn.release(false);
			this.PeCloseEye = null;
		}

		public void deactivate()
		{
			this.executeScapecatRespawnAfter();
			UIPicture.tecon_TS = 1f;
			if (this.Hd != null)
			{
				this.M2D.remValotAddition(this.Hd);
				IN.DestroyOne(this.Hd.gameObject);
				this.Hd = null;
			}
			if (this.Continuer != null)
			{
				this.Continuer.destruct();
				this.Continuer = null;
			}
			if (this.Hd2 != null)
			{
				this.Hd2.deactivate(false);
			}
			this.EfHn.deactivateSpecific(POSTM.TS_SLOW, false);
			this.EfHn.deactivateSpecific(POSTM.ZOOM2, false);
			this.PeZoom = null;
			if (this.Log_Death != null)
			{
				this.Log_Death.deactivate(false);
			}
			this.M2D.Freezer.deactivate();
			if (this.TkKD != null)
			{
				this.TkKD = this.TkKD.destruct();
			}
			if (this.TkKDScapeCat != null)
			{
				this.TkKDScapeCat = this.TkKDScapeCat.destruct();
			}
			this.Log_Death = null;
			EV.remListener(this);
			this.changeState(UiGO.STATE.DEACTIVATE);
		}

		public UiGO destruct()
		{
			PostEffect.setable_camera_scale = true;
			if (this.TkKD != null)
			{
				this.TkKD = this.TkKD.destruct();
			}
			if (this.TkKDScapeCat != null)
			{
				this.TkKDScapeCat = this.TkKDScapeCat.destruct();
			}
			EV.remWaitListener(this);
			EV.remListener(this);
			if (this.EfHn != null)
			{
				this.EfHn.release(false);
				this.EfHn = null;
			}
			this.PeZoom = null;
			this.M2D.Freezer.deactivate();
			this.M2D.FlgWarpEventNotInjectable.Rem("GO");
			if (this.Hd != null)
			{
				this.M2D.remValotAddition(this.Hd);
				IN.DestroyOne(this.Hd.gameObject);
				this.Hd = null;
			}
			if (this.Continuer != null)
			{
				this.Continuer.destruct();
				this.Continuer = null;
			}
			if (this.Hd2 != null)
			{
				this.M2D.remValotAddition(this.Hd2);
				IN.DestroyOne(this.Hd2.gameObject);
				this.Hd2 = null;
			}
			return null;
		}

		private void prepareKDTicket(bool force_prepare_scapesel_kd = false)
		{
			if (this.TkKD != null)
			{
				return;
			}
			this.TkKD = this.M2D.TxKD.AddTicket(150, new TxKeyDesc.FnGetKD(this.fnTopKD), this);
			NelItem byId = NelItem.GetById("scapecat", false);
			ItemStorage itemStorage = null;
			int num = 0;
			if (byId != null)
			{
				ItemStorage[] inventoryArray = this.M2D.IMNG.getInventoryArray();
				for (int i = inventoryArray.Length - 1; i >= 0; i--)
				{
					ItemStorage itemStorage2 = inventoryArray[i];
					ItemStorage.ObtainInfo info = itemStorage2.getInfo(byId);
					if (info != null && info.total > 0)
					{
						itemStorage = itemStorage2;
						num = info.top_grade;
						break;
					}
				}
			}
			if (itemStorage != null)
			{
				this.ScapeSel = new ScapecatSelector(this.Pr, num, byId, itemStorage);
			}
			if (itemStorage != null || force_prepare_scapesel_kd)
			{
				this.TkKDScapeCat = this.M2D.TxKD.AddTicket(151, new TxKeyDesc.FnGetKD(this.fnScapeCatKD), this);
				this.TkKDScapeCat.top_center = true;
			}
			this.TxReposit();
		}

		private void changeState(UiGO.STATE _state)
		{
			UiGO.STATE state = this.state;
			if (this.TkKD == null && (_state == UiGO.STATE.TOP || _state == UiGO.STATE.PAUSE))
			{
				this.prepareKDTicket(false);
				if (this.ScapeSel != null)
				{
					this.t_cat = 0f;
				}
			}
			if (state == UiGO.STATE.PAUSE)
			{
				this.M2D.Freezer.deactivate();
			}
			if (state == UiGO.STATE.TOP)
			{
				if (_state != UiGO.STATE.GIVEUP)
				{
					this.EfHn.deactivate(true);
					this.PeCloseEye = null;
				}
				if (this.ScapeSel != null && this.ScapeSel.isActive())
				{
					this.ScapeSel.runDeactivating(true);
				}
			}
			if (this.Continuer != null && !this.Continuer.prepared)
			{
				this.Continuer.recalcSpeed();
			}
			X.ALL0(this.At_input_ui);
			this.state = _state;
			this.t = -1f;
			switch (_state)
			{
			case UiGO.STATE.TOP:
				if (this.PeCloseEye != null)
				{
					this.PeCloseEye = null;
					this.EfHn.deactivate(true);
				}
				if (this.TkKD != null)
				{
					this.TkKD.need_fine = true;
				}
				break;
			case UiGO.STATE.PAUSE:
				this.M2D.Freezer.activate(this.Pr);
				if (this.TkKDScapeCat != null)
				{
					this.TkKDScapeCat = this.TkKDScapeCat.destruct();
				}
				break;
			case UiGO.STATE.GIVEUP:
				if (this.PeCloseEye != null)
				{
					this.PeCloseEye.destruct();
					this.PeCloseEye = null;
				}
				if (this.PeZoom != null)
				{
					this.PeZoom.destruct();
					this.PeZoom = null;
				}
				if (this.TkKDScapeCat != null)
				{
					this.TkKDScapeCat = this.TkKDScapeCat.destruct();
				}
				if (this.TkKD != null)
				{
					this.TkKD = this.TkKD.destruct();
				}
				if (this.Continuer != null)
				{
					this.Continuer.destruct();
					this.Continuer = null;
				}
				this.M2D.QUEST.summonerFinished(null, true, false);
				this.Pr.Mp.DmgCntCon.TS = 2.5f;
				this.Pr.EpCon.go_unlock_thresh = true;
				this.M2D.FlgWarpEventNotInjectable.Add("GO");
				UIPicture.tecon_TS = 4f;
				BGM.fadeout(0f, 90f, false);
				this.Pr.setVoiceOverrideAllowLevel(0.7f);
				this.EfHn.deactivate(true);
				PostEffect.setable_camera_scale = true;
				this.PeZoom = null;
				PostEffect.IT.clear();
				PostEffect.IT.run(0f);
				this.M2D.Ui.fineDmgCounterRunable(true);
				this.is_trapped = this.Pr.isWormTrapped();
				this.EfHn.Set(PostEffect.IT.setSlowFading(0f, -1f, 0f, 0));
				if (this.Pr.isFacingEnemy() || (EnemySummoner.isActiveBorder() && EnemySummoner.ActivePlayer.getRestCount() > 0) || this.is_trapped)
				{
					this.t = (float)((int)(-(int)X.NIXP(350f, 550f)));
				}
				this.t_ovk = 0f;
				this.t_ovk_a = -X.NIXP(10f, 60f);
				this.t_ovk_d = -X.NIXP(30f, 70f);
				this.t_ovk_e = -X.NIXP(20f, 90f);
				if (EnemySummoner.isActiveBorder())
				{
					if (EnemySummoner.ActiveScript.puppetrevenge_enable)
					{
						this.puppet_revenge_enabled = true;
						this.after_water_progress = this.M2D.IMNG.battleFinishProgress(4, true);
						PuppetRevenge.losedInRevengeBattle(this.M2D);
					}
					else
					{
						EnemySummoner.ActivePlayer.touchAllReelsObtain();
						this.after_water_progress = this.M2D.IMNG.battleFinishProgress(EnemySummoner.ActiveScript.grade, true);
					}
				}
				this.GameoverMapJumpTo(null);
				EV.stack("__M2D_GAMEOVER", 0, -1, new string[]
				{
					this.M2D.curMap.key,
					((EnemySummoner.isActiveBorder() ? 1 : 0) | (this.worm_damaged ? 2 : 0)).ToString(),
					EnemySummoner.isActiveBorder() ? (EnemySummoner.ActiveScript.fatal_key ?? "") : ""
				}, null);
				this.EpVar = this.Pr.EpCon.copyOverkillPower();
				this.Hd = this.CreateHide(0);
				this.Hd.immediateFillColor();
				IN.setZ(this.Hd.transform, 9f);
				this.Log_Death = null;
				this.Afade_key = new List<string>(5);
				this.Agvu_attr = new List<MGATTR>(5);
				if (EnemySummoner.isActiveBorder())
				{
					EnemySummoner.ActiveScript.listUpUIFadeKey(this.Afade_key, this.Agvu_attr, this.Pr.EpCon.SituCon);
					this.Afade_key.Add("damage");
					this.Afade_key.Add("damage");
					this.Afade_key.Add("damage");
					this.Afade_key.Add("crouch");
				}
				else if (this.is_trapped)
				{
					this.Agvu_attr.Add(MGATTR.WORM);
				}
				if (this.is_trapped)
				{
					this.Afade_key.Add("insected");
				}
				if (this.Agvu_attr.Count == 0)
				{
					this.Agvu_attr.Add(MGATTR.NORMAL);
				}
				if (this.Afade_key.Count == 0)
				{
					this.Afade_key.Add("damage");
				}
				break;
			case UiGO.STATE.DEACTIVATE:
				this.Pr.EpCon.go_unlock_thresh = false;
				this.M2D.FlgWarpEventNotInjectable.Rem("GO");
				this.M2D.flushLastExSituationTemp();
				break;
			}
			if (this.TkKD != null)
			{
				this.TkKD.need_fine = true;
			}
		}

		public HideScreen CreateHide(int id)
		{
			HideScreen hideScreen = new GameObject("GO Hide Screen_" + id.ToString()).AddComponent<HideScreen>();
			hideScreen.gameObject.layer = IN.LAY(IN.gui_layer_name);
			hideScreen.Col = C32.d2c(4282004532U);
			this.M2D.addValotAddition(hideScreen);
			hideScreen.activate();
			return hideScreen;
		}

		private void fnTopKD(STB Stb, object Target)
		{
			if (this.state == UiGO.STATE.TOP)
			{
				if (this.ScapeSel != null)
				{
					this.fnScapeCatKD(Stb, Target);
					Stb.Ret("\n");
				}
				Stb.AddTxA("GO_KeyHelp_top", false);
				bool flag = true;
				if (CFGSP.go_cheat && this.PeCloseEye == null)
				{
					if (flag)
					{
						Stb.Ret("\n");
					}
					else
					{
						Stb.Add(" ");
					}
					Stb.Add("<key ltab/> ").AddTxA("KD_go_escape_from_bind", false);
					Stb.Add(" <key rtab/> ").AddTxA("KD_shuffle_enemy_position", false);
				}
			}
		}

		private void fnScapeCatKD(STB Stb, object Target)
		{
			if (this.ScapeSel != null)
			{
				Stb.Add("<key z/>+<key x/> ");
			}
			else
			{
				Stb.Add("<key_s z/>+<key_s x/> ");
			}
			Stb.AddTxA("Item_cmd_respawn", false);
			if (this.state == UiGO.STATE.WAIT)
			{
				Stb.Add("  <key cancel/><key c/>").AddTxA("Select_skip", false);
			}
		}

		public bool run(float fcnt)
		{
			if (this.Log_Death != null)
			{
				this.Log_Death = this.Log_Death.hold();
			}
			bool flag = true;
			bool flag2 = !this.M2D.Iris.isIrisActive();
			if (this.PeZoom != null)
			{
				this.PeZoom.fine(120);
			}
			switch (this.state)
			{
			case UiGO.STATE.WAIT:
				this.EfHn.fine(100);
				if (Map2d.can_handle)
				{
					if (this.Pr.isMapO(0))
					{
						this.M2D.Freezer.activate(this.Pr);
					}
					if (this.M2D.Freezer.isActive())
					{
						this.changeState(UiGO.STATE.PAUSE);
					}
					else
					{
						bool flag3 = true;
						if (this.continuer_active)
						{
							if (!this.runScapeSel(ref flag, ref flag2))
							{
								break;
							}
							if (this.ScapeSel != null && this.ScapeSel.isActive())
							{
								if (this.TkKDScapeCat != null)
								{
									this.TkKDScapeCat.hold_blink = false;
									break;
								}
								break;
							}
							else
							{
								if (this.t < 0f)
								{
									break;
								}
								if (!this.Continuer.prepared)
								{
									this.Continuer.recalcSpeed();
									this.prepareKDTicket(true);
									if (this.TkKDScapeCat != null)
									{
										this.TkKDScapeCat.size = 16f;
										this.TxReposit();
									}
								}
								if (!this.Continuer.isTimerOvered())
								{
									flag3 = false;
									this.t = 0f;
									bool flag4 = IN.isCancelOn(20) || IN.isTargettingO(0);
									if (this.TkKDScapeCat != null)
									{
										this.TkKDScapeCat.hold_blink = IN.isCancelOn(0) || IN.isTargettingO(0);
									}
									this.Continuer.run(fcnt * (flag4 ? 2.5f : 1f));
								}
								else
								{
									flag3 = true;
									this.Continuer.run(fcnt);
									if (this.t >= 130f)
									{
										this.changeState(UiGO.STATE.TOP);
									}
								}
							}
						}
						if (flag3 && (IN.ketteiOn() || this.Pr.isAtkO(0) || this.Pr.isMagicO(0) || IN.isLTabO() || IN.isRTabO() || IN.isUiSortO() || this.Pr.isItmO(0) || this.Pr.isCancelU() || this.Pr.isCancelO(0) || this.t >= 120f))
						{
							this.changeState(UiGO.STATE.TOP);
						}
					}
				}
				break;
			case UiGO.STATE.TOP:
				this.checkLockInput(ref this.At_input_ui[0], (this.At_input_ui[0] <= 0) ? IN.isUiSortPD() : IN.isUiSortO());
				this.checkLockInput(ref this.At_input_ui[1], (this.At_input_ui[1] <= 0) ? IN.isLTabPD() : IN.isLTabO());
				this.checkLockInput(ref this.At_input_ui[2], (this.At_input_ui[2] <= 0) ? IN.isRTabPD() : IN.isRTabO());
				this.EfHn.fine(100);
				if (this.PeCloseEye != null)
				{
					this.PeCloseEye.x = X.ZLINE(this.t - 20f, 220f);
				}
				if (this.t - 20f >= 100f)
				{
					this.TkKD.startFadein();
					if (this.t - 20f >= 220f)
					{
						this.changeState(UiGO.STATE.GIVEUP);
						break;
					}
				}
				else
				{
					if (this.t >= 20f)
					{
						if (!this.runScapeSel(ref flag, ref flag2))
						{
							break;
						}
						if (Map2d.can_handle && flag2 && this.Pr.isCancelO(0))
						{
							if (this.PeCloseEye == null)
							{
								this.EfHn.Set(this.PeCloseEye = PostEffect.IT.setPE(POSTM.GO_CLOSE_EYE, 50f, 0.01f, 0));
								if (CFGSP.go_cheat)
								{
									this.TkKD.need_fine = true;
								}
							}
							this.TkKD.hold_blink = true;
						}
						else
						{
							this.t -= 3f * fcnt;
							this.TkKD.hold_blink = false;
							if (this.t < 20f)
							{
								this.t = 20f;
								if (this.PeCloseEye != null)
								{
									this.PeCloseEye = null;
									this.EfHn.deactivate(true);
									if (CFGSP.go_cheat)
									{
										this.TkKD.need_fine = true;
									}
								}
							}
						}
					}
					if (Map2d.can_handle && flag)
					{
						if (this.checkLockInput(0))
						{
							this.changeState(UiGO.STATE.PAUSE);
							break;
						}
						if (((this.t_lock_up_input > 0f) ? IN.isSubmitUp(-1) : IN.isSubmitPD(1)) && !this.TkKD.isShaking())
						{
							this.TkKD.startShake();
						}
					}
				}
				if (this.PeCloseEye == null && flag && CFGSP.go_cheat && this.t >= 10f)
				{
					if (this.checkLockInput(1))
					{
						this.executeSpCheat(false);
					}
					if (this.checkLockInput(2))
					{
						this.executeSpCheat(true);
					}
				}
				break;
			case UiGO.STATE.PAUSE:
				this.EfHn.fine(100);
				if (Map2d.can_handle && IN.isUiSortPD())
				{
					this.M2D.Cam.resetBaseMoverIfFocusing(this.Pr, false);
					this.M2D.Freezer.deactivate();
					PostEffect.setable_camera_scale = false;
				}
				if (!this.M2D.Freezer.isActive())
				{
					this.changeState(UiGO.STATE.TOP);
				}
				break;
			case UiGO.STATE.SCAPECAT_REVERSAL_AFTER:
				if (this.t < 100f && !this.executeScapecatRespawn())
				{
					this.t = -fcnt;
				}
				if (this.t >= 260f)
				{
					this.executeScapecatRespawnAfter();
				}
				break;
			case UiGO.STATE.GIVEUP:
				this.EfHn.fine(100);
				if (this.t < -1f)
				{
					this.runGiveup();
				}
				if (this.t >= 0f && this.Hd2 == null)
				{
					if (EV.getCurrentWaitListener() != null && EV.getCurrentWaitListener() != this)
					{
						this.t = -1f;
					}
					else
					{
						UIPicture.tecon_TS = 1f;
						this.Hd2 = this.CreateHide(1);
						this.Hd2.HIDE_MAXT = 60;
						IN.setZ(this.Hd2.transform, -9.49f);
						this.M2D.initMapMaterialASync(this.ReturnBackMp, 2, true);
					}
				}
				if (this.t >= 90f)
				{
					if (EnemySummoner.isActiveBorder())
					{
						this.Pr.EpCon.SituCon.addTempSituation("&&GM_ep_situation_gameover_battle_demon", 1, true);
					}
					else
					{
						this.Pr.EpCon.SituCon.addTempSituation("&&GM_ep_situation_gameover", 1, true);
					}
					this.M2D.restartGame(false);
					UiBenchMenu.gameovered = true;
					this.M2D.transferring_game_stopping = true;
					this.changeState(UiGO.STATE.GIVEUP_MAP_TRANSITION);
				}
				break;
			case UiGO.STATE.GIVEUP_MAP_TRANSITION:
				if (this.M2D.curMap == null)
				{
					this.t = -fcnt;
				}
				if (this.M2D.curMap != null && this.t >= 3f)
				{
					this.M2D.restartFromGameOver();
					if (this.is_trapped)
					{
						this.M2D.MGC.setMagic(this.Pr, MGKIND.EF_WORM_PUBLISH, MGHIT.AUTO);
					}
					if (this.puppet_revenge_enabled && PuppetRevenge.losedInRevengeBattleAfter(this.M2D))
					{
						UILog.Instance.AddAlertTX("Alert_item_stolen", UILogRow.TYPE.ALERT_PUPPET);
					}
					if (this.after_water_progress > 0f)
					{
						this.M2D.PlayerNoel.JuiceCon.progressWaterDrunkCache(this.after_water_progress, false, false);
					}
				}
				break;
			case UiGO.STATE.DEACTIVATE:
				if (this.Hd2 == null || this.Hd2.full_hidden)
				{
					return false;
				}
				break;
			}
			if (this.continuer_active)
			{
				if (this.state > UiGO.STATE.WAIT)
				{
					this.Continuer.run(fcnt * (float)((this.M2D.Freezer.isActive() || (this.ScapeSel != null && this.ScapeSel.isActive())) ? 0 : 1));
				}
				if (this.Continuer.isTimerOvered() && this.TkKDScapeCat != null)
				{
					this.TkKDScapeCat = this.TkKDScapeCat.destruct();
					this.t_cat = -2f;
				}
				if (this.PeZoom != null)
				{
					this.PeZoom.x = X.ZLINE(this.Continuer.progress_level, 0.5f);
				}
				this.Continuer.base_alpha = X.VALWALK(this.Continuer.base_alpha, this.M2D.Freezer.isActive() ? 0.33f : 1f, 0.04f);
			}
			else if (this.PeZoom != null)
			{
				this.PeZoom.x = 1f;
			}
			this.t += fcnt;
			this.t_lock_up_input = X.VALWALK(this.t_lock_up_input, 0f, fcnt);
			if (this.t_cat >= 0f && this.ScapeSel != null)
			{
				this.t_cat += fcnt;
				if (this.t_cat >= 320f)
				{
					this.t_cat = -1f;
					if (this.TkKDScapeCat != null)
					{
						this.TkKDScapeCat = this.TkKDScapeCat.destruct();
					}
				}
			}
			return true;
		}

		public bool EvtWait(bool is_first = false)
		{
			return this.state == UiGO.STATE.PAUSE || this.state == UiGO.STATE.GIVEUP || (this.state == UiGO.STATE.GIVEUP_MAP_TRANSITION && this.t <= 1f);
		}

		public bool isGivingUp()
		{
			return this.state == UiGO.STATE.GIVEUP;
		}

		public bool isWaitingGiveup()
		{
			return this.isActive() && this.state < UiGO.STATE.GIVEUP;
		}

		private void checkLockInput(ref int input_t, bool is_inputting)
		{
			if (is_inputting)
			{
				input_t = ((input_t < 1) ? 1 : 2);
				return;
			}
			if (input_t != 0)
			{
				input_t = ((input_t > 0) ? (-1) : 0);
			}
		}

		private bool checkLockInput(int input_id)
		{
			if (this.t_lock_up_input <= 0f)
			{
				return this.At_input_ui[input_id] == 1;
			}
			return this.At_input_ui[input_id] == -1;
		}

		private bool runScapeSel(ref bool check_sorting, ref bool check_cancelo)
		{
			if (this.ScapeSel == null)
			{
				return true;
			}
			if (Map2d.can_handle & check_cancelo)
			{
				bool flag;
				if (this.ScapeSel.isActive())
				{
					flag = IN.isMagicO(0) && IN.isAtkO(0);
				}
				else
				{
					flag = (IN.isMagicO(0) && IN.isAtkPD(1)) || (IN.isMagicPD(1) && IN.isAtkO(0));
				}
				if (IN.isMagicO(0) || IN.isAtkO(0))
				{
					this.t_lock_up_input = 4f;
				}
				if (flag)
				{
					check_cancelo = false;
					check_sorting = false;
					this.ScapeSel.runActivating();
					this.TkKD.stopShake();
					if (this.ScapeSel.decided)
					{
						this.executeScapecatRespawn(-1);
						return false;
					}
				}
				else if (this.ScapeSel.isActive())
				{
					this.ScapeSel.runDeactivating(true);
				}
			}
			else if (this.ScapeSel.isActive())
			{
				this.ScapeSel.runDeactivating(true);
			}
			return true;
		}

		private void runGiveup()
		{
			this.t_ovk += 1f;
			this.Pr.TeCon.clear();
			float num = X.NI(1, 4, X.ZPOW(this.t_ovk - 100f, 560f));
			UILog.uilog_frame_base_speed = num;
			bool flag = false;
			float num2 = this.t_ovk_a + 1f;
			this.t_ovk_a = num2;
			if (num2 >= 0f)
			{
				this.t_ovk_a = -X.NI(80f, 40f, X.ZPOW(X.XORSP())) / ((float)X.Mn(3, this.Pr.enemy_targetted) + 2f) / num;
				if (this.EpVar != null && X.XORSP() < 0.2f)
				{
					this.AtkAbsorb.EpDmg = this.EpVar;
				}
				else
				{
					this.AtkAbsorb.EpDmg = null;
				}
				this.AtkAbsorb.attr = ((X.XORSP() < 0.5f) ? MGATTR.ABSORB : this.Agvu_attr[X.xors(this.Agvu_attr.Count)]);
				float num3 = X.XORSP();
				if (num3 < 0.6f)
				{
					this.Pr.PtcST("player_absorbed_basic", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				else if (num3 < 0.8f)
				{
					this.Pr.PtcST("player_absorbed_fatal", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
				flag = flag || X.XORSP() < 0.24f;
				this.Pr.applyAbsorbDamage(this.AtkAbsorb, true, X.XORSP() < 0.7f, null, false);
				this.clearEf();
				this.GiveUpUPDamage(this.AtkAbsorb.attr);
			}
			num2 = this.t_ovk_d + 1f;
			this.t_ovk_d = num2;
			if (num2 >= 0f)
			{
				this.t_ovk_d = -X.NI(90f, 50f, X.ZPOW(X.XORSP())) / ((float)X.Mn(2, this.Pr.enemy_targetted) + 2f) / num;
				this.AtkDamage.burst_vy = ((X.XORSP() < 0.2f) ? (-0.08f) : 0f);
				this.AtkDamage.burst_vx = 0f;
				this.AtkDamage.attr = this.Agvu_attr[X.xors(this.Agvu_attr.Count)];
				this.Pr.DMG.applyDamage(this.AtkDamage, true, null, false, false);
				flag = true;
				if (X.XORSP() < 0.3f)
				{
					this.Pr.BetoMng.Check(BetoInfo.Normal, false, true);
				}
				this.GiveUpUPDamage(this.AtkDamage.attr);
			}
			num2 = this.t_ovk_e + 1f;
			this.t_ovk_e = num2;
			if (num2 >= 0f)
			{
				this.t_ovk_e = -X.NI(30f, 60f, X.ZPOW(X.XORSP())) / ((float)X.Mn(2, this.Pr.enemy_targetted) + 2f) / num;
				if (this.Pr.EggCon.total > 0)
				{
					this.Pr.EggCon.check_holded_mp += (float)X.IntC((float)this.Pr.EggCon.total / 40f);
					if (!this.Pr.EggCon.need_fine_mp)
					{
						this.Pr.EggCon.need_fine_mp = true;
					}
				}
			}
			if (flag)
			{
				SND.Ui.play("hit_go", false);
			}
			this.Pr.processLayingOrOrgasm(num);
			this.Pr.runSpecialObject(num);
		}

		private void GiveUpUPDamage(MGATTR attr)
		{
			int num = X.xors(4);
			this.Pr.GSaver.removeHeadDelay();
			this.Pr.GSaver.penetrateHpDamageReduce();
			this.Pr.UP.applyDamage(attr, X.XORSPS() * 6f, X.XORSPS() * 6f, (((num & 1) != 0) ? UIPictureBase.EMSTATE.SMASH : UIPictureBase.EMSTATE.NORMAL) | (((num & 2) != 0) ? UIPictureBase.EMSTATE.ABSORBED : UIPictureBase.EMSTATE.NORMAL), X.XORSP() < 0.6f, this.Afade_key[X.xors(this.Afade_key.Count)], true);
		}

		private void clearEf()
		{
			this.M2D.EF.clear();
			this.M2D.EFT.clear();
		}

		private void prepareContinuer()
		{
			this.Continuer = new UiGOContinuer(this.M2D, false);
			this.Continuer.countdown_snd = "gameover_countdown";
		}

		public void TxReposit()
		{
			if (CFGSP.gameover_counter > 0 && this.Continuer == null && this.state < UiGO.STATE.GIVEUP)
			{
				this.prepareContinuer();
				this.Continuer.recalcSpeed();
			}
			if (this.Continuer != null)
			{
				IN.PosP2(this.Continuer.transform, this.M2D.ui_shift_x, 0f);
				if (this.Continuer.prepared)
				{
					if (CFGSP.gameover_counter == 0)
					{
						if (this.Continuer.isActive())
						{
							this.Continuer.deactivate(true);
							if (this.TkKDScapeCat != null)
							{
								this.TkKDScapeCat = this.TkKDScapeCat.destruct();
							}
						}
					}
					else
					{
						this.t_cat = -2f;
						this.Continuer.activate();
						if (this.TkKDScapeCat == null && !this.Continuer.isTimerOvered())
						{
							this.TkKDScapeCat = this.M2D.TxKD.AddTicket(151, new TxKeyDesc.FnGetKD(this.fnScapeCatKD), this);
						}
						this.Continuer.recalcSpeed();
					}
				}
				this.M2D.TxKD.need_reposit = true;
			}
			if (this.TkKD != null)
			{
				this.TkKD.top_center = CFG.go_text_pos_top;
				this.TkKD.size = (CFG.go_text_pos_top ? 18f : 14f);
			}
			if (this.TkKDScapeCat != null)
			{
				this.TkKDScapeCat.top_center = true;
				if (this.continuer_active)
				{
					this.t_cat = -2f;
				}
				this.TkKDScapeCat.shift_pixel_y = (this.continuer_active ? (-IN.hh * 1.15f) : 0f);
			}
		}

		public bool isActive()
		{
			return this.state != UiGO.STATE.DEACTIVATE;
		}

		public void executeScapecatRespawn(int grade = -1)
		{
			if (this.state <= UiGO.STATE.TOP && this.ScapeSel != null)
			{
				this.EfHn.release(false);
				if (this.TkKDScapeCat != null && this.Continuer == null)
				{
					this.TkKDScapeCat = this.TkKDScapeCat.destruct();
					this.t_cat = -1f;
				}
				if (this.M2D.Iris.isWaiting(this.Pr))
				{
					this.M2D.Iris.ForceWakeupInput(true);
					this.M2D.Iris.runPost(this.Pr, true);
				}
				else if (PUZ.IT.barrier_active || (this.Pr.isDamagingOrKo() && !EnemySummoner.isActiveBorder()))
				{
					this.Pr.releaseFromIrisOut(true, PR.STATE.DAMAGE_L_LAND, "dmg_down2", false);
				}
				if (grade < 0)
				{
					grade = this.ScapeSel.itm_grade;
					this.ScapeSel.InvItemFrom.Reduce(this.ScapeSel.TargetItem, 1, grade, true);
				}
				else
				{
					this.ScapeSel.itm_grade = grade;
					this.ScapeSel.runDeactivating(true);
				}
				this.changeState(UiGO.STATE.SCAPECAT_REVERSAL_AFTER);
				this.executeScapecatRespawn();
			}
		}

		private bool executeScapecatRespawn()
		{
			if (this.M2D.curMap != null && !this.M2D.transferring_game_stopping)
			{
				if (this.PeZoom != null)
				{
					this.PeZoom.z = 40f;
					this.PeZoom.deactivate(false);
					this.PeZoom = null;
				}
				int num = this.Pr.executeScapecatRespawn(this.ScapeSel.itm_grade, true);
				if (this.ScapeSel != null)
				{
					this.ScapeSel.add_danger = num;
				}
				this.t = 100f;
				return true;
			}
			return false;
		}

		public void executeScapecatRespawnAfter()
		{
			if (this.ScapeSel != null && this.state <= UiGO.STATE.SCAPECAT_REVERSAL_AFTER)
			{
				if (this.ScapeSel != null && this.M2D.NightCon.isNoelJuiceExplodable())
				{
					this.M2D.NightCon.addAdditionalDangerLevelStock(this.ScapeSel.add_danger);
				}
				this.ScapeSel.deactivate();
				this.ScapeSel = null;
				this.deactivate();
			}
		}

		public bool isScapecatEnabled()
		{
			return this.state <= UiGO.STATE.TOP && this.ScapeSel != null && this.Pr.canUseScapecat();
		}

		public void setLockInput(float t = 20f)
		{
			this.t_lock_up_input = X.Mx(this.t_lock_up_input, t);
		}

		public override string ToString()
		{
			return "<UiGameOver>";
		}

		public bool continuer_active
		{
			get
			{
				return this.Continuer != null && CFGSP.gameover_counter > 0;
			}
		}

		bool IEventListener.EvtRead(EvReader ER, StringHolder rER, int skipping)
		{
			if (rER.cmd == "UI_GAMEOVER")
			{
				string _ = rER._1;
				if (_ != null)
				{
					if (!(_ == "NO_NFBT"))
					{
						if (_ == "SET_TO_GIVEUP")
						{
							if (this.isActive() && this.state < UiGO.STATE.GIVEUP)
							{
								this.changeState(UiGO.STATE.GIVEUP);
							}
						}
					}
					else if (this.state == UiGO.STATE.GIVEUP)
					{
						this.t = X.Mx(this.t, 0f);
					}
				}
				return true;
			}
			if (rER.cmd == "GAMEOVER_MAP_JUMP_TO")
			{
				this.GameoverMapJumpTo(rER._1);
				return true;
			}
			return false;
		}

		bool IEventListener.EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		bool IEventListener.EvtClose(bool is_first_or_end)
		{
			return true;
		}

		int IEventListener.EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		bool IEventListener.EvtMoveCheck()
		{
			return true;
		}

		public void GameoverMapJumpTo(string key)
		{
			if (TX.noe(key))
			{
				this.ReturnBackMp = this.M2D.curMap;
				SVD.sFile currentFile = COOK.getCurrentFile();
				currentFile.revert_pos = true;
				this.ReturnBackMp = this.M2D.Get(currentFile.last_map_key, false) ?? this.ReturnBackMp;
				return;
			}
			Map2d map2d = this.M2D.Get(key, false);
			if (map2d == null)
			{
				return;
			}
			this.ReturnBackMp = map2d;
			SVD.sFile currentFile2 = COOK.getCurrentFile();
			if (currentFile2 != null)
			{
				currentFile2.revert_pos = false;
			}
		}

		public void executeSpCheat(bool do_shuffle = false)
		{
			if (this.Pr.isAbsorbState())
			{
				this.Pr.getAbsorbContainer().releaseFromTarget(this.Pr);
			}
			if (this.Pr.isWormTrapped())
			{
				this.Pr.executeReleaseFromTrapByDamage(true, true);
			}
			if (!do_shuffle)
			{
				return;
			}
			SND.Ui.play("en_summoned", false);
			Map2d curMap = this.M2D.curMap;
			int count_movers = curMap.count_movers;
			int i = 0;
			while (i < count_movers)
			{
				M2Attackable m2Attackable = curMap.getMv(i) as M2Attackable;
				if (m2Attackable is PR)
				{
					if (!m2Attackable.is_alive)
					{
						goto IL_00AA;
					}
				}
				else if (m2Attackable is NelEnemy && !(m2Attackable as NelEnemy).do_not_shuffle_on_cheat)
				{
					goto IL_00AA;
				}
				IL_0190:
				i++;
				continue;
				IL_00AA:
				Vector2 vector;
				if (EnemySummoner.isActiveBorder())
				{
					M2LpSummon summonedArea = EnemySummoner.ActiveScript.getSummonedArea();
					vector = M2Rect.getWalkableS(curMap, summonedArea.mapcx, summonedArea.mapcy, (float)(summonedArea.mapx + 2), (float)(summonedArea.mapy + 2), (float)(summonedArea.mapx + summonedArea.mapw - 2), (float)(summonedArea.mapy + summonedArea.maph - 2), true, m2Attackable.sizex, m2Attackable.sizey);
				}
				else
				{
					vector = M2Rect.getWalkableS(curMap, (float)curMap.clms * 0.5f, (float)curMap.rows * 0.5f, (float)(curMap.crop + 2), (float)(curMap.crop + 2), (float)(curMap.clms - curMap.crop - 2), (float)(curMap.rows - curMap.crop - 2), true, m2Attackable.sizex, m2Attackable.sizey);
				}
				m2Attackable.setTo(vector.x, vector.y);
				goto IL_0190;
			}
		}

		private UiGO.STATE state;

		private float t;

		private float t_cat = -2f;

		private float t_lock_up_input;

		private const float MAXT_CAT = 320f;

		private readonly PR Pr;

		private readonly NelM2DBase M2D;

		private EffectHandlerPE EfHn;

		private PostEffectItem PeZoom;

		private PostEffectItem PeCloseEye;

		public const float ef_maxt = 180f;

		public const float push_need_maxt = 100f;

		public bool worm_damaged;

		private bool puppet_revenge_enabled;

		private List<string> Afade_key;

		private List<MGATTR> Agvu_attr;

		private float after_water_progress;

		private float ovk_fcnt;

		private float t_ovk_d;

		private float t_ovk_a;

		private float t_ovk_e;

		private float t_ovk;

		private int[] At_input_ui;

		private HideScreen Hd;

		private HideScreen Hd2;

		private bool is_trapped;

		private EpAtk EpVar;

		private Map2d ReturnBackMp;

		private ScapecatSelector ScapeSel;

		private NelAttackInfo AtkAbsorb = new NelAttackInfo
		{
			hpdmg0 = 7,
			split_mpdmg = 7,
			attr = MGATTR.ABSORB,
			nodamage_time = 0,
			setable_UP = false,
			ndmg = NDMG.GAMEOVER
		};

		private NelAttackInfo AtkDamage = new NelAttackInfo
		{
			split_mpdmg = 7,
			attr = MGATTR.NORMAL,
			hpdmg0 = 15,
			nodamage_time = 0,
			ndmg = NDMG.GAMEOVER,
			setable_UP = false
		};

		private UILogRow Log_Death;

		private TxKeyDesc.KDTicket TkKD;

		private TxKeyDesc.KDTicket TkKDScapeCat;

		private UiGOContinuer Continuer;

		private const float TS_DMGCNT_IN_GIVINGUP = 2.5f;

		private enum STATE
		{
			WAIT,
			TOP,
			PAUSE,
			SCAPECAT_REVERSAL_AFTER,
			GIVEUP,
			GIVEUP_MAP_TRANSITION,
			DEACTIVATE
		}
	}
}
