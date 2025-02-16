using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using m2d;
using nel.gm;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class NelM2DBase : M2DBase
	{
		public NelM2DBase(M2ImageContainer _IMGS = null, bool it_is_basic_M2D = true)
			: base(_IMGS, it_is_basic_M2D)
		{
			COOK.reloading = false;
			if (NelM2DBase.first_activate)
			{
				NelM2DBase.first_activate = false;
				EventLineDebugger.AFlagCheck.Add(NelM2DBase.RegEvalSf);
				EventLineDebugger.AFnFlagReplace.Add(() => TX.InputE((float)COOK.getSF(REG.R1)).ToString());
				EventLineDebugger.AFlagCheck.Add(NelM2DBase.RegEvalSfEvt);
				EventLineDebugger.AFnFlagReplace.Add(() => TX.InputE((float)COOK.getSF("EV_" + REG.R1)).ToString());
			}
			this.MGC = new MGContainer(this);
			this.FnCreateLp = NelLp.NelCreateLabelPoint;
			this.FnCreateChipPreparation = NelChip.NelChipPreparation;
			this.Iris = new IrisOutManager(this);
			this.Tips = new NelTipsManager(this);
			this.ENMTR = new EnMtrManager(this);
			EnemySummoner.initEnemySummoner();
			UiBenchMenu.initBenchMenu();
			this.CheckPoint = new PlayerCheckPoint();
			this.NightCon = new NightController(this);
			this.IMNG = new NelItemManager(this);
			this.Puz = new PUZ(this);
			this.STAIN = new StainManager(this);
			this.WIND = new WindManager(this);
			this.WDR = new WanderingManager(this);
			this.FCutin = new FallenCutin(this);
			this.Marker = new UiGmMapMarker(this);
			this.DARKSPOT = new DarkSpotEffect(this);
			this.GUILD = new GuildManager(this);
			this.QUEST = new QuestTracker(this);
			this.FlagRain = new Flagger(delegate(FlaggerT<string> V)
			{
				if (this.PefRain != null)
				{
					this.PefRain.y = 90f;
					this.PefRain.deactivate(false);
				}
				this.PefRain = this.PE.setPEfadeinout(POSTM.RAIN, 90f, -1f, 1f, 0);
			}, delegate(FlaggerT<string> V)
			{
				if (this.PefRain != null)
				{
					this.PefRain.y = 90f;
					this.PefRain.deactivate(false);
				}
				this.PefRain = null;
			});
			this.FlgStopEvtMsgValotizeOverride = new Flagger(delegate(FlaggerT<string> V)
			{
				EV.valotile_overwrite_msg = false;
			}, delegate(FlaggerT<string> V)
			{
				EV.valotile_overwrite_msg = true;
			});
			this.FlagOpenGm = new Flagger(delegate(FlaggerT<string> V)
			{
				this.can_open_gamemenu = false;
			}, delegate(FlaggerT<string> V)
			{
				this.can_open_gamemenu = true;
			});
			this.FlgFastTravelDeclined = new Flagger(null, null);
			this.FlgAreaTitleHide = new Flagger(null, null);
			this.FD_EvDebuggerActive = delegate(bool flag)
			{
				if (flag)
				{
					this.FlagValotStabilize.Add("EVDEBUGGER");
					return;
				}
				this.FlagValotStabilize.Rem("EVDEBUGGER");
			};
			EV.ActionEvDebugEnableChanged = (Action<bool>)Delegate.Combine(EV.ActionEvDebugEnableChanged, this.FD_EvDebuggerActive);
			this.auto_evacuate = false;
			this.no_publish_juice = false;
			this.Freezer = new M2FreezeCamera(this);
		}

		public override void initGameObject(GameObject _GobBase, float cam_w, float cam_h)
		{
			base.initGameObject(_GobBase, cam_w, cam_h);
			this.TxKD.z_bottom = 9.05f;
			this.TxKD.z_front = -4.25f;
			this.IMNG.initGameObject();
			this.BlurSc = new GameObject("BlurGM").AddComponent<BlurScreen>();
			this.BlurSc.setZ(-3.25f);
			this.BlurSc.base_z = 0f;
			this.BlurSc.use_valotile = true;
			this.BlurSc.FnCompletelyVisible = delegate(bool _flag)
			{
				if (!_flag)
				{
					this.need_blursc_hiding_whole = false;
					this.FlgHideWholeScreen.Rem("__BLURSC");
					this.Ui.FlgFrontLog.Rem("__BLURSC");
					return;
				}
				this.need_blursc_hiding_whole = true;
			};
			this.GM = new GameObject("GM").AddComponent<UiGameMenu>();
			this.GM.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			this.GM.gameObject.SetActive(false);
			this.MapTitle = new GameObject("M2MapTitle").AddComponent<M2MapTitle>();
			this.MapTitle.M2D = this;
			this.AreaTitle = new GameObject("M2AreaTitle").AddComponent<M2AreaTitle>();
			this.AreaTitle.M2D = this;
			this.PE = new PostEffect(this, 48);
			MDAT.initMapDamage(this.MDMGCon);
		}

		public override void initEvent()
		{
			EV.initEvent(new NelMSGContainer(), null, this.TutoBox = new GameObject("Ev-Tutorial").AddComponent<TutorialBox>());
			EV.valotile_overwrite_msg = !this.FlgStopEvtMsgValotizeOverride.isActive();
			EV.addListener(NEL.Instance);
			EV.addWaitListener("ITEMDESC", this.IMNG.get_DescBox());
			EV.addWaitListener("REELMNG", this.IMNG.getReelManager());
			EV.addWaitListener("NIGHTCON", this.NightCon);
			EV.addWaitListener("UIGM_ACTIVATE", this.GM.WaitFnGMActivate());
			NEL.createTextLog();
			PuzzLiner.initG(this);
			UIPicture.Instance.CutinMng.initEvent();
		}

		public override void stackFirstEvent()
		{
			if (this.EvLsn == null && EV.initDebugger(false))
			{
				this.EvLsn = new NelEvDebugListener(this);
			}
			if (this.EvtListener == null)
			{
				this.NEvtListener = new NelM2DEventListener(this);
				this.EvtListener = this.NEvtListener;
			}
			base.stackFirstEvent();
		}

		public void newGame()
		{
			this.IMNG.newGame();
			this.FCutin.Clear(false);
			this.WM.clearAccess();
			this.NightCon.clear();
			this.WDR.newGame();
			EnemySummonerManager.releaseAll();
			this.CheckPoint.newGame();
			this.GM.newGame();
			this.QUEST.newGame();
			QuestTracker.newGameS();
			this.GUILD.newGame();
			TRMManager.newGame();
			this.FlgFastTravelDeclined.Clear();
			if (this.EvtListener != null)
			{
				this.EvtListener.newGame();
			}
			this.saveposition_rewrite = false;
		}

		public NelM2DBase restartGame(bool clear_gameover = true)
		{
			this.reel_content_send_to_house_inventory = false;
			UIPicture.tecon_TS = 1f;
			M2DBase.material_flush_flag |= M2DBase.FLUSH._ALL;
			UILog.uilog_frame_base_speed = 1f;
			if (clear_gameover)
			{
				COOK.reloading = true;
			}
			bool curMap = this.curMap != null;
			this.Cam.setEditorSetPosAndScale(this.Cam.x, this.Cam.y, 1f);
			if (curMap)
			{
				this.changeMap(null);
			}
			this.Iris.clear();
			if (this.EvtListener != null)
			{
				this.EvtListener.newGame();
			}
			this.saveposition_rewrite = false;
			this.Freezer.deactivate();
			if (clear_gameover)
			{
				if (this.GameOver != null)
				{
					this.GameOver = this.GameOver.destruct();
				}
				COOK.newGame(this, false);
				this.PEClear();
				this.TxKD.Clear();
			}
			else
			{
				M2DBase.material_flush_flag &= ~(M2DBase.FLUSH.INIT_GAME | M2DBase.FLUSH.CHANGE_AREA);
				this.NightCon.clearWithoutNightLevel();
			}
			if (this.PlayerNoel != null && this.PlayerNoel.EggCon != null)
			{
				this.PlayerNoel.EggCon.fineActivate();
			}
			UILog.Instance.fineFonts();
			return this;
		}

		public void initGameOver()
		{
			if (this.GameOver != null && !this.GameOver.isActive())
			{
				this.GameOver = this.GameOver.destruct();
			}
			if (this.GameOver == null)
			{
				this.GameOver = new UiGO(this);
			}
		}

		public void restartFromGameOver()
		{
			PRNoel playerNoel = this.PlayerNoel;
			BGM.fadeout(100f, 0f, false);
			UIPicture.changePlayer(playerNoel);
			this.Cam.assignBaseMover(playerNoel, 1);
			playerNoel.recoverFromGameOver();
			this.GameOver.deactivate();
			base.PauseMem(true);
			base.ResumeMem(true);
		}

		public void quitGame(string change_scene)
		{
			global::XX.Logger.scene_changing = true;
			COOK.reloading = true;
			try
			{
				this.destruct(true);
			}
			catch (Exception ex)
			{
				Debug.Log(ex);
			}
			this.PE.quitGame();
			try
			{
				IN.DestroyOne(this.GobBase);
			}
			catch (Exception ex2)
			{
				Debug.Log(ex2);
			}
			if (change_scene != null)
			{
				IN.LoadScene("SceneTitle");
				return;
			}
			IN.quitGame();
		}

		public override void destruct(bool execute_destruct_maps = true)
		{
			if (this.destructed)
			{
				return;
			}
			this.destructed = true;
			try
			{
				if (this.PlayerNoel != null)
				{
					NoelAnimator animator = this.PlayerNoel.getAnimator();
					if (animator != null)
					{
						animator.prepareDestruct();
						if (animator.FlgDropCane != null)
						{
							animator.FlgDropCane.Clear();
						}
					}
				}
			}
			catch
			{
			}
			try
			{
				this.GM.releaseGMCInstance();
			}
			catch
			{
			}
			EnemySummonerManager.releaseAll();
			this.GUILD.destruct();
			if (execute_destruct_maps)
			{
				this.destructMaps();
			}
			try
			{
				this.flushCachedPxlMaterial();
				this.Ui.destruct();
				M2LpMapTransfer.closeMap();
				UiRestRoom.ReleaseInstance();
				EV.ActionEvDebugEnableChanged = (Action<bool>)Delegate.Remove(EV.ActionEvDebugEnableChanged, this.FD_EvDebuggerActive);
			}
			catch
			{
			}
			try
			{
				this.MGC.initS(null);
				this.PE.destruct();
				this.Puz.destruct();
				this.NightCon.destruct();
				EV.remListener(NEL.Instance);
				this.FCutin.destruct();
				IMessageContainer messageContainer = EV.getMessageContainer();
				if (messageContainer != null)
				{
					messageContainer.destructGob();
				}
			}
			catch
			{
			}
			try
			{
				this.QUEST.destruct();
				this.NightCon.clear();
				if (this.GameOver != null)
				{
					this.GameOver = this.GameOver.destruct();
				}
				this.SndRain = null;
				this.EfRain = null;
				this.Freezer.destruct();
			}
			catch
			{
			}
			BetobetoManager.quitBetobetoManager();
			base.destruct(false);
		}

		public override void setFlushAllFlag(bool only_game_reload)
		{
			if (only_game_reload && this.WM.init_game)
			{
				M2DBase.material_flush_flag |= M2DBase.FLUSH.INIT_GAME;
				return;
			}
			M2DBase.material_flush_flag |= M2DBase.FLUSH._ALL;
		}

		protected override void checkFlush(bool can_check_map = true)
		{
			if ((M2DBase.material_flush_flag & M2DBase.FLUSH.MATERIAL) != (M2DBase.FLUSH)0)
			{
				EnemySummoner.FlushAll();
			}
			base.checkFlush(can_check_map);
		}

		protected override void flushCachedPxlMaterial()
		{
			base.flushCachedPxlMaterial();
			this.IMNG.USel.prepareResource(this);
			this.ENMTR.destruct();
			EnemyAnimator.FlushCachedFrames();
			NelNUni.FlushPxlData();
			NelNSponge.FlushPxlData();
		}

		public override void flushMaps(MAPMODE nextmap_open_mode = MAPMODE.NORMAL)
		{
			PRNoel prNoel = this.getPrNoel();
			if (prNoel != null && prNoel.EpCon != null)
			{
				prNoel.EpCon.flushCurrentSession();
			}
			M2CImgDrawerFootBell.flush();
			COOK.map_walk_count = 0;
			base.flushMaps(nextmap_open_mode);
		}

		public void prepareSvTexture(string key, bool immediate = false)
		{
			BetobetoManager betoMng = this.PlayerNoel.BetoMng;
			if (betoMng != null)
			{
				Texture mainTexture = UIPicture.Instance.MtrSpine.mainTexture;
				betoMng.prepareTexture(key, immediate);
				UIPicture.Instance.MtrSpine.mainTexture = mainTexture;
			}
		}

		public override void LoadAfter()
		{
			base.LoadAfter();
			this.WM = new WholeMapManager(this);
			this.WM.reloadScript();
			this.WA = new WAManager();
		}

		public override void initGameObjectAfter()
		{
			this.WM.PrepareMapData();
			base.initGameObjectAfter();
		}

		public override void cameraFinedImmediately()
		{
			this.NightCon.cameraFinedImmediately();
		}

		public override void initCameraAfter(Map2d M)
		{
			this.PE.releaseMesh();
			base.initCameraAfter(M);
			MTR.initCameraAfter();
			this.FCutin.initCameraAfter();
			if (!X.DEBUGSTABILIZE_DRAW)
			{
				this.Cam.assignRenderFunc(new CameraRenderBinderFunc(M.ToString() + "::EF-bottom", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (this.EF == null)
					{
						return true;
					}
					this.EF.RenderOneSide(true, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, 420f), M.Dgn.effect_layer_bottom, false, null);
				this.Cam.assignRenderFunc(new CameraRenderBinderFunc(M.ToString() + "::EF-top", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (this.EF == null)
					{
						return true;
					}
					this.EF.RenderOneSide(false, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, 120f), M.Dgn.effect_layer_top, false, null);
				this.Cam.assignRenderFunc(new CameraRenderBinderFunc(M.ToString() + "::EFT-bottom", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (this.EFT == null)
					{
						return true;
					}
					this.EFT.RenderOneSide(true, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, 55f), this.Cam.getFinalRenderedLayer(), false, null);
				this.Cam.assignRenderFunc(new CameraRenderBinderFunc(M.ToString() + "::EFT-top", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (this.EFT == null)
					{
						return true;
					}
					this.EFT.RenderOneSide(false, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, 35f), this.Cam.getFinalRenderedLayer(), false, null);
			}
			M2Shield.resetMaterial();
			EnemyAnimator.resetShieldMaterial();
			this.need_night_fine = 2;
		}

		public override void fineDgnHalfBgm(bool flag = true)
		{
			base.fineDgnHalfBgm(flag);
			bool flag2 = false;
			if (this.curMap != null && this.curMap.Dgn != null)
			{
				flag2 = flag && this.curMap.Dgn.isLowerBgm(this.curMap);
			}
			if (flag2)
			{
				if (this.PeLowerBgm == null)
				{
					this.PeLowerBgm = this.PE.setPE(POSTM.BGM_LOWER, 80f, 0.95f, 0);
					return;
				}
			}
			else if (this.PeLowerBgm != null)
			{
				this.PeLowerBgm.deactivate(false);
				this.PeLowerBgm = null;
			}
		}

		public void fineDgnNightLevel(bool force = false, bool redraw = false)
		{
			if (this.curMap != null && this.curMap.Dgn != null)
			{
				float num = (this.curMap.Dgn.no_rain_fall ? 0f : this.rain_level_);
				if (force || this.need_night_fine >= 2 || this.curMap.Dgn.night_level != this.NightCon.night_level || this.curMap.Dgn.rain_level != num)
				{
					this.curMap.Dgn.night_level = this.NightCon.night_level % 2f;
					this.curMap.Dgn.rain_level = num;
					this.fineMaterialColor(true);
					if (redraw)
					{
						this.curMap.drawCheck(0f);
					}
				}
				this.fineRainEffect();
				this.need_night_fine = 0;
				return;
			}
			if (this.need_night_fine < 2)
			{
				this.need_night_fine = 1;
			}
		}

		public override void fineMaterialColor(bool execute = false)
		{
			if (execute)
			{
				this.curMap.fineMaterialNightColor(null, null);
				this.Ui.changeBgMaterial(null, null, null);
				return;
			}
			this.need_night_fine = 2;
		}

		public override void setCameraTransparentColor(Color32 C)
		{
			if (!base.isDestructed() && this.Cam != null)
			{
				this.Cam.transparent_color = C;
				this.Ui.changeBgMaterial(null, null, null);
			}
		}

		private void PEClear()
		{
			this.PE.clear();
			Map2d.TSbase = 1f;
			Map2d.TSbasepr = 1f;
			this.getPrNoel().Ser.releaseEffect(true);
			if (this.PefRain != null)
			{
				this.PefRain = this.PE.setPEfadeinout(POSTM.RAIN, 1f, -1f, 1f, 0);
			}
		}

		public override Map2d changeMap(Map2d M)
		{
			this.PlayerNoel.endS();
			if (this.curMap != null)
			{
				this.AEvacuatedMover = base.EvacuateCoreMover(null);
			}
			if (M == this.curMap && M != null)
			{
				this.changeMap(null);
			}
			if (this.Mana != null)
			{
				this.Mana.destruct();
				this.Mana = null;
			}
			this.EfRain = null;
			this.RainFoot = null;
			this.STAIN.clear();
			this.CheckPoint.initS(M);
			CoinStorage.initS();
			M2LpMapTransfer.closeMap();
			this.NightCon.UiDg.deactivate(true);
			PuzzLiner.initS(M);
			this.WIND.clear();
			EnemySummoner.initS();
			UIBase.Instance.getEffect().spliceNotActiveMeshes();
			this.initSWholeMap(M);
			if (this.curMap != null)
			{
				UiBenchMenu.endS(false);
				if (this.Ui != null)
				{
					this.Ui.QuestBox.HideInactivePositionStack(this.curMap);
				}
			}
			this.WDR.walkAround(false);
			if (M == null)
			{
				COOK.addTimer(this, false);
				this.FlgFastTravelDeclined.Rem("EVENT");
				this.DARKSPOT.endS();
				this.IMNG.endS();
				this.FCutin.endS();
			}
			else
			{
				SupplyManager.initS(M);
				if (this.WM.CurWM != null)
				{
					this.ev_mobtype = this.WM.CurWM.ev_mobtype ?? "";
				}
			}
			this.no_publish_juice = false;
			base.changeMap(M);
			this.MIST = ((M == null) ? null : new MistManager(M));
			if (this.GameOver != null)
			{
				this.GameOver.initS();
			}
			this.IMNG.initS();
			this.Puz.initS(M);
			this.GM.initS(M);
			this.MGC.initS(M);
			this.recheck_activation_flags = true;
			if (M == null)
			{
				this.WM.endS(null);
				this.Ui.endS();
				this.Ui.transition_darken = true;
				SCN.need_fine_pvv = true;
				this.Mana = null;
				this.MIST = null;
				this.ENMTR.destruct();
				M2CImgDrawerFootBell.flush();
				M2LpSummon.NearLpSmn = null;
				this.Freezer.deactivate();
				return null;
			}
			this.default_mist_pose = 0;
			this.pr_mp_consume_ratio = 1f;
			this.QUEST.need_fine_pos = true;
			COOK.resetCacheTimer(this);
			PostEffect.setable_camera_scale = true;
			this.NightCon.initS(M);
			this.Mana = new M2ManaContainer(M);
			if (this.WM.refine_area_title)
			{
				this.WM.refine_area_title = false;
				this.AreaTitle.init(this.WM.pre_area_title, this.WM.CurWM.dark_area, 0f, true, false);
				if (!COOK.reloading)
				{
					this.saveposition_rewrite = true;
				}
			}
			this.MapTitle.initS(M);
			if ((M2DBase.material_flush_flag & M2DBase.FLUSH.CHANGE_AREA) != (M2DBase.FLUSH)0)
			{
				M2DBase.material_flush_flag &= ~M2DBase.FLUSH.CHANGE_AREA;
				this.WDR.fineFirstPosition(true);
			}
			if (!Map2d.editor_decline_lighting)
			{
				this.fineDgnNightLevel(true, false);
			}
			this.fineRainEffect();
			this.IMNG.initSAfter();
			if (this.AEvacuatedMover != null)
			{
				base.EvacuateCoreMover(this.AEvacuatedMover);
				this.AEvacuatedMover = null;
			}
			SVD.sFile currentFile = COOK.getCurrentFile();
			currentFile.revertPos();
			if (currentFile.first_load)
			{
				currentFile.playInit();
			}
			this.Ui.QuestBox.FlgHide.Rem("BATTLE");
			this.Ui.fineDmgCounterRunable(false);
			NEL.Instance.Vib.clear(PadVibManager.VIB.IN_GAME);
			this.WM.initSAfter(M);
			if (M.DmgCntCon != null)
			{
				M.DmgCntCon.FnEffectEnable = this.Ui.FD_MapDmgCntConEffectEnable;
			}
			return M;
		}

		private void initSWholeMap(Map2d Mp)
		{
			this.WM.initS(Mp);
			if (this.WM.flush_material)
			{
				this.WM.flush_material = false;
				if ((M2DBase.material_flush_flag & M2DBase.FLUSH.INIT_GAME) != (M2DBase.FLUSH)0)
				{
					M2DBase.material_flush_flag &= ~M2DBase.FLUSH.INIT_GAME;
					M2DBase.material_flush_flag |= M2DBase.FLUSH.MATERIAL | M2DBase.FLUSH.MAP | M2DBase.FLUSH.RELOAD_AREA_MATERIAL;
				}
				else
				{
					M2DBase.material_flush_flag |= M2DBase.FLUSH.MATERIAL | M2DBase.FLUSH.MAP | M2DBase.FLUSH.CHANGE_AREA;
				}
			}
			else if ((M2DBase.material_flush_flag & M2DBase.FLUSH.INIT_GAME) != (M2DBase.FLUSH)0)
			{
				M2DBase.material_flush_flag &= ~M2DBase.FLUSH.INIT_GAME;
				M2DBase.material_flush_flag |= M2DBase.FLUSH.RELOAD_AREA_MATERIAL;
			}
			if ((M2DBase.material_flush_flag & M2DBase.FLUSH.RELOAD_AREA_MATERIAL) != (M2DBase.FLUSH)0)
			{
				M2DBase.material_flush_flag &= ~M2DBase.FLUSH.RELOAD_AREA_MATERIAL;
				this.WM.initWmLoadMaterial();
			}
			if (this.WM.changed_area)
			{
				this.need_night_fine = 2;
			}
		}

		public override void prepareImageAtlasFinalize()
		{
			base.prepareImageAtlasFinalize();
			MTR.PxlM2DGeneral = MTR.PxlM2DGeneral ?? PxlsLoader.getPxlCharacter(M2DBase.Achip_pxl_key[0]);
		}

		public override void initMapMaterialASync(Map2d _Mp, int transfering, bool manual_mode = false)
		{
			base.initMapMaterialASync(_Mp, transfering, manual_mode);
			if (this.Ui != null && transfering > 0)
			{
				this.Ui.transition_darken = false;
			}
		}

		protected override bool loadMapAsyncFinalize(Map2d Mp_Next)
		{
			if (Mp_Next != null && this.curMap == null && (this.WM.CurWM == null || this.WM.CurWM.CurMap != Mp_Next))
			{
				this.initSWholeMap(Mp_Next);
			}
			if (!base.loadMapAsyncFinalize(Mp_Next))
			{
				if (Mp_Next != null)
				{
					UiSVD.unloadSound();
				}
				return false;
			}
			return true;
		}

		public void hideAreaTitle(bool immediate = false)
		{
			this.AreaTitle.deactivate(immediate);
		}

		private void fineRainEffect()
		{
			float num = (this.curMap.Dgn.no_rain_fall ? 0f : this.rain_level_);
			if (num > 0f && this.curMap != null && this.curMap.Dgn != null && this.curMap.Dgn.use_rain && this.EfRain == null)
			{
				this.EfRain = this.EF.setE("dungeon_rain", 0f, 0f, num, 0, 0);
				this.RainFoot = new RainEffector(this);
			}
			if (this.EfRain != null)
			{
				this.EfRain.z = num;
			}
			if (num == 0f)
			{
				this.killRainEffect();
				return;
			}
			if (this.SndRain == null)
			{
				this.SndRain = this.Snd.play("areasnd_rain");
			}
			this.SndRain.setVolManual(this.rain_level, true);
		}

		private void killRainEffect()
		{
			if (this.EfRain != null)
			{
				this.EfRain.z = 0f;
			}
			this.EfRain = null;
			if (this.RainFoot != null)
			{
				this.RainFoot.destruct();
			}
			this.RainFoot = null;
			if (this.SndRain != null)
			{
				this.SndRain.Stop();
				this.SndRain = null;
			}
		}

		public override bool readPtcScript(PTCThread rER)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2462827663U)
				{
					if (num != 1574621873U)
					{
						if (num != 1838106216U)
						{
							if (num != 2462827663U)
							{
								goto IL_02A7;
							}
							if (!(cmd == "%PE"))
							{
								goto IL_02A7;
							}
							POSTM postm;
							if (!FEnum<POSTM>.TryParse(rER._1, out postm, true))
							{
								rER.tError("不明なPOSTM: " + rER._1);
								return true;
							}
							float num2 = rER.Nm(2, 40f);
							float num3 = rER.Nm(3, 1f);
							int num4 = rER.Int(4, 1);
							this.PE.setPE(postm, num2, num3, num4);
							return true;
						}
						else
						{
							if (!(cmd == "%GET_LOCK_MGATTR"))
							{
								goto IL_02A7;
							}
							rER.Def(rER.getHash(1), 0f);
							return true;
						}
					}
					else if (!(cmd == "%PVIB2"))
					{
						goto IL_02A7;
					}
				}
				else if (num <= 3459549804U)
				{
					if (num != 3385964291U)
					{
						if (num != 3459549804U)
						{
							goto IL_02A7;
						}
						if (!(cmd == "%SET_LOCK_MGATTR"))
						{
							goto IL_02A7;
						}
						return true;
					}
					else
					{
						if (!(cmd == "%PE_fadeinout"))
						{
							goto IL_02A7;
						}
						POSTM postm;
						if (!FEnum<POSTM>.TryParse(rER._1, out postm, true))
						{
							rER.tError("不明なPOSTM: " + rER._1);
							return true;
						}
						float num2 = rER.Nm(2, 40f);
						float num5 = rER.Nm(3, 40f);
						float num3 = rER.Nm(4, 1f);
						int num4 = rER.Int(5, 1);
						this.PE.setPEfadeinout(postm, num2, num5, num3, num4);
						return true;
					}
				}
				else if (num != 3767826492U)
				{
					if (num != 4233139481U)
					{
						goto IL_02A7;
					}
					if (!(cmd == "%PVIB"))
					{
						goto IL_02A7;
					}
				}
				else
				{
					if (!(cmd == "%PVIB_NEAR"))
					{
						goto IL_02A7;
					}
					float num6 = 1f - X.ZPOW(X.LENGTHXYS(this.Cam.x * this.curMap.rCLEN, this.Cam.y * this.curMap.rCLEN, rER.Nm(1, 0f), rER.Nm(2, 0f)) - 5f, 15f);
					if (num6 > 0f)
					{
						NEL.PadVib(rER._3, num6);
					}
					return true;
				}
				for (int i = 1; i < rER.clength; i++)
				{
					NEL.PadVib(rER.getIndex(i), 1f);
				}
				return true;
			}
			IL_02A7:
			return base.readPtcScript(rER);
		}

		public override void mapClosed(Map2d Mp, ref IEffectSetter _EF, ref IEffectSetter _EFT, ref IEffectSetter _EFC)
		{
			if (_EF != null)
			{
				if (_EF == this.EF)
				{
					this.EF = null;
				}
				_EF.destruct();
				_EF = null;
			}
			if (_EFT != null)
			{
				if (_EFT == this.EFT)
				{
					this.EFT = null;
				}
				_EFT.destruct();
				_EFT = null;
			}
			if (_EFC != null)
			{
				_EFC.destruct();
				_EFC = null;
			}
		}

		public override void configReconsidered(M2Pt[,] AAPt, int _l, int _t, int _r, int _b)
		{
			if (this.MIST != null)
			{
				this.MIST.configReconsidered(AAPt, _l, _t, _r, _b);
			}
		}

		public override void mapActionInitted(Map2d Mp)
		{
			MTR.mapActionInitted();
			this.IMNG.digestDropObjectCache();
			PostEffectItem.t_heartbeat = 0f;
		}

		public override void mapActionClosed(Map2d Mp)
		{
			if (this.MIST != null)
			{
				this.MIST.closeAction();
			}
		}

		public override void mainMvIntPosChanged(bool pos4 = false)
		{
			base.mainMvIntPosChanged(pos4);
			if (pos4)
			{
				this.IMNG.need_fine_center_position = true;
			}
		}

		public override void initChipEffect(Map2d M, M2SubMap SM, ref IEffectSetter _EFC, ref M2DrawBinderContainer _EFDC)
		{
			Dungeon dgn = M.Dgn;
			EffectNelMapChip effectNelMapChip;
			if (_EFC == null)
			{
				_EFC = (effectNelMapChip = new EffectNelMapChip(this, SM, M.gameObject, 4));
				dgn.initChipEffect<EffectItemNel>(M, effectNelMapChip, new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel));
				if (X.DEBUGSTABILIZE_DRAW)
				{
					MultiMeshRenderer multiMeshRenderer = IN.CreateGob(M.gameObject, "-EFC").AddComponent<MultiMeshRenderer>();
					IN.setZAbs(multiMeshRenderer.gameObject.transform, 0f);
					effectNelMapChip.assignMMRDForMeshDrawerContainer(multiMeshRenderer);
				}
				else
				{
					effectNelMapChip.no_graphic_render = (effectNelMapChip.draw_gl_only = true);
				}
			}
			else
			{
				effectNelMapChip = _EFC as EffectNelMapChip;
				if (effectNelMapChip != null)
				{
					effectNelMapChip.deassignRender();
					dgn.initChipEffect<EffectItemNel>(M, effectNelMapChip, new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel));
					if (effectNelMapChip.Length > 0)
					{
						effectNelMapChip.assignToCamera();
					}
				}
			}
			effectNelMapChip.mapPosAutoFix(M);
			if (effectNelMapChip != null && _EFDC == null)
			{
				_EFDC = new M2DrawBinderContainer(M, SM, effectNelMapChip, "EFDC");
				return;
			}
			_EFDC.SetContainer(M, SM, effectNelMapChip);
		}

		public override void initEffect(Map2d M, ref IEffectSetter _EF, ref IEffectSetter _EFT)
		{
			Dungeon dgn = M.Dgn;
			if (!M.is_submap)
			{
				EffectNel effectNel;
				_EF = (effectNel = (this.EF = new EffectNel(M, 512)));
				effectNel.effect_name = "EF[" + M.ToString() + "]";
				effectNel.initEffect("M2d EF", dgn.EffectCamera, new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel), EFCON_TYPE.NORMAL);
				effectNel.setLayer(dgn.effect_layer_top, dgn.effect_layer_bottom);
				effectNel.topBaseZ = 120f;
				effectNel.bottomBaseZ = 420f;
				EffectNel effectNel2;
				_EFT = (this.EFT = (effectNel2 = new EffectNel(M, 80)));
				effectNel2.effect_name = "EFT[" + M.ToString() + "]";
				effectNel2.initEffect("M2d EFT", this.Cam.get_FinalCamera(), new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel), EFCON_TYPE.NORMAL);
				effectNel2.setLayer(this.Cam.getFinalRenderedLayer(), this.Cam.getFinalSourceRenderedLayer());
				effectNel2.topBaseZ = 35f;
				effectNel2.bottomBaseZ = 55f;
				if (X.DEBUGSTABILIZE_DRAW)
				{
					MultiMeshRenderer multiMeshRenderer = IN.CreateGob(M.gameObject, "-EF").AddComponent<MultiMeshRenderer>();
					IN.setZAbs(multiMeshRenderer.gameObject.transform, 0f);
					multiMeshRenderer.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
					effectNel.assignMMRDForMeshDrawerContainer(multiMeshRenderer);
					MultiMeshRenderer multiMeshRenderer2 = IN.CreateGob(M.gameObject, "-EFT").AddComponent<MultiMeshRenderer>();
					IN.setZAbs(multiMeshRenderer.gameObject.transform, 0f);
					multiMeshRenderer2.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
					effectNel2.assignMMRDForMeshDrawerContainer(multiMeshRenderer2);
					return;
				}
				effectNel.no_graphic_render = (effectNel.draw_gl_only = true);
				effectNel2.no_graphic_render = (effectNel2.draw_gl_only = true);
			}
		}

		public override void loadMaterialForLabelPoint(Map2d Mp, Map2d.LpLoader _Loader)
		{
			if (TX.headerIs(_Loader.key, "Summon", 0, '_', false))
			{
				EnemySummoner enemySummoner = EnemySummoner.Get(EnemySummoner.Lp2smn(_Loader.key), false);
				if (enemySummoner != null)
				{
					enemySummoner.loadMaterial(Mp, false);
				}
			}
			if (TX.isStart(_Loader.key, "SmnC_", 0))
			{
				this.IMGS.Atlas.prepareChipImageDirectory("mgplant/", false);
			}
			if (TX.isStart(_Loader.key, "Water", 0))
			{
				this.IMGS.Atlas.prepareChipImageDirectory("water/", false);
			}
		}

		public bool need_uipic_remake
		{
			get
			{
				return UIPicture.Instance != null && UIPicture.Instance.need_uipic_texture_remake;
			}
			set
			{
				if (UIPicture.Instance != null)
				{
					UIPicture.Instance.need_uipic_texture_remake = value;
				}
			}
		}

		public override bool isCenterPlayer(M2Mover Mv)
		{
			return Mv == this.getPrNoel();
		}

		public override void FlaggerValotEnabled(FlaggerT<string> F)
		{
			base.FlaggerValotEnabled(F);
			Flagger flgValotileDisable = UIBase.FlgValotileDisable;
			if (flgValotileDisable != null)
			{
				flgValotileDisable.Add("M2D");
			}
			this.BlurSc.use_valotile = false;
			this.AreaTitle.valotile_enabled = false;
			this.IMNG.use_valotile = false;
			this.Cam.stabilizeFinalizeValot(true);
		}

		public override void FlaggerValotDisabled(FlaggerT<string> F)
		{
			base.FlaggerValotDisabled(F);
			bool flag = !X.DEBUGSTABILIZE_DRAW;
			this.BlurSc.use_valotile = flag;
			if (flag)
			{
				Flagger flgValotileDisable = UIBase.FlgValotileDisable;
				if (flgValotileDisable != null)
				{
					flgValotileDisable.Rem("M2D");
				}
			}
			else
			{
				Flagger flgValotileDisable2 = UIBase.FlgValotileDisable;
				if (flgValotileDisable2 != null)
				{
					flgValotileDisable2.Add("M2D");
				}
			}
			this.IMNG.use_valotile = flag;
			this.AreaTitle.valotile_enabled = flag;
			this.Cam.stabilizeFinalizeValot(!flag);
		}

		protected override void FlaggerHideWholeScreen(bool _flag)
		{
			base.FlaggerHideWholeScreen(_flag);
			if (_flag)
			{
				Flagger flgFrontLog = this.Ui.FlgFrontLog;
				if (flgFrontLog != null)
				{
					flgFrontLog.Add("__HIDEWHOLESCREEN");
				}
			}
			else
			{
				Flagger flgFrontLog2 = this.Ui.FlgFrontLog;
				if (flgFrontLog2 != null)
				{
					flgFrontLog2.Rem("__HIDEWHOLESCREEN");
				}
				Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
				if (flgUiEffectDisable != null)
				{
					flgUiEffectDisable.Rem("__HIDEWHOLESCREEN");
				}
			}
			this.fineHideEffectDisableByHideScreen();
		}

		public void fineHideEffectDisableByHideScreen()
		{
			if (!this.Ui.event_center_uipic && this.FlgHideWholeScreen.isActive())
			{
				Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
				if (flgUiEffectDisable == null)
				{
					return;
				}
				flgUiEffectDisable.Add("__HIDEWHOLESCREEN");
				return;
			}
			else
			{
				Flagger flgUiEffectDisable2 = UIBase.FlgUiEffectDisable;
				if (flgUiEffectDisable2 == null)
				{
					return;
				}
				flgUiEffectDisable2.Rem("__HIDEWHOLESCREEN");
				return;
			}
		}

		protected override void FlaggerWarpEventNotInjectable(bool _flag)
		{
			base.FlaggerWarpEventNotInjectable(_flag);
			if (!_flag)
			{
				this.flushLastExSituationTemp();
			}
		}

		public override bool isGameOverActive()
		{
			return this.GameOver != null;
		}

		public override bool EvtOpen(bool is_start)
		{
			base.EvtOpen(is_start);
			if (is_start)
			{
				this.menu_open_ &= NelM2DBase.MENU_OPEN._CANNOT_OPEN;
				this.FlagOpenGm.Add("EV");
				COOK.FlgTimerStop.Add("EV");
				this.FlgAreaTitleHide.Rem("EVENT");
				NelMSGResource.reload_source_if_undefined = true;
				this.NightCon.UiDg.deactivate(true);
				if (this.curMap != null)
				{
					PR pr = this.curMap.getKeyPr() as PR;
					if (pr != null && pr.isOnBench(false))
					{
						UiBenchMenu.hideModalOfflineOnBench();
					}
				}
			}
			this.GM.evOpen(is_start);
			this.Ui.fnEvOpenUI(is_start);
			return true;
		}

		public void areaTitleHideProgress()
		{
			this.AreaTitle.hideProgress();
		}

		public override bool EvtClose(bool is_end)
		{
			this.GM.evClose(is_end);
			base.EvtClose(is_end);
			if (is_end)
			{
				NEL.EvtClose();
				SCN.newGame();
				UiRestRoom.ReleaseInstance();
				PR.PunchDecline(18, false);
				this.IMNG.hideWindows();
				UiRestRoom.evQuit();
				this.BlurSc.remFlag("__EVENT");
				UIBase.FlgUiEffectDisable.Rem("EVENT");
				this.MapTitle.FlgNotShow.Rem("EVENT");
				this.FlgAreaTitleHide.Rem("EVENT");
				Flagger flgFrontLog = this.Ui.FlgFrontLog;
				if (flgFrontLog != null)
				{
					flgFrontLog.Rem("EVENT");
				}
				COOK.FlgTimerStop.Rem("EV");
				this.FlagOpenGm.Rem("EV");
				if (this.curMap != null)
				{
					PR pr = this.curMap.getKeyPr() as PR;
					if (pr != null)
					{
						if (pr.isOnBenchAndCanShowModal())
						{
							UiBenchMenu.showModalOfflineOnBench();
							UIBase.Instance.gameMenuSlide(false, false);
							UIBase.Instance.gameMenuBenchSlide(false, false);
						}
						if (this.Cam.getBaseMover() != pr)
						{
							this.Cam.assignBaseMover(pr, -1);
						}
						M2BlockColliderContainer.BCCLine footBCC = pr.getFootManager().get_FootBCC();
						if (footBCC != null)
						{
							this.CheckPoint.fineFoot(pr, footBCC, false);
						}
					}
				}
				this.IMNG.getReelManager().digestObtainedMoney().digestObtainedItem(true);
				this.fineSentToHouseInv();
				if (this.QUEST.need_fine_pos && this.curMap != null)
				{
					this.QUEST.positionNoticeCheck(this.curMap);
				}
				if (this.saveposition_rewrite && this.curMap != null)
				{
					this.saveposition_rewrite = false;
					COOK.getCurrentFile().assignRevertPosition(this);
				}
			}
			this.Ui.fnEvCloseUI(is_end);
			return true;
		}

		public void refineAllLanguageFonts(bool evt_check)
		{
			if (evt_check && this.curMap != null)
			{
				this.curMap.getEventContainer().fineCheckDescName();
			}
			if (UILog.Instance != null)
			{
				UILog.Instance.fineFonts();
				this.Ui.QuestBox.fineFonts();
				if (this.AreaTitle.isActive() && this.WM.CurWM != null)
				{
					this.AreaTitle.init(this.WM.pre_area_title, this.WM.CurWM.dark_area, 0f, true, true);
				}
				else
				{
					this.AreaTitle.fineFonts();
				}
			}
			this.PlayerNoel.refineAllLanguageCache();
			IMessageContainer messageContainer = EV.getMessageContainer();
			if (messageContainer != null)
			{
				messageContainer.fineTargetFont();
			}
		}

		public override void assignTalkableObject(M2MoverPr FromMv, int x, int y, AIM k, List<IM2TalkableObject> ATk)
		{
			base.assignTalkableObject(FromMv, x, y, k, ATk);
			this.IMNG.assignTalkableObject(FromMv, x, y, k, ATk);
		}

		public override void changeTalkTarget(IM2TalkableObject ChangedTo)
		{
			this.IMNG.need_fine_target_window = true;
		}

		public override void colliderUpdated()
		{
			if (this.RainFoot != null)
			{
				this.RainFoot.need_reconsider = true;
			}
			this.STAIN.recheckWholeBcc(this.curMap.BCC);
			this.WIND.recheckLength();
		}

		public override bool run(float fcnt)
		{
			this.pre_map_active = false;
			if (this.need_blursc_hiding_whole)
			{
				this.need_blursc_hiding_whole = false;
				if (this.BlurSc.isCompletelyShown() && !this.GM.isStoppingGame())
				{
					this.FlgHideWholeScreen.Add("__BLURSC");
					this.Ui.FlgFrontLog.Add("__BLURSC");
				}
				else
				{
					this.FlgHideWholeScreen.Rem("__BLURSC");
					this.Ui.FlgFrontLog.Rem("__BLURSC");
				}
			}
			if (this.GM.isStoppingGame())
			{
				return false;
			}
			if (this.GameOver != null)
			{
				Bench.P("runPre GO");
				if (!this.GameOver.run(fcnt))
				{
					this.GameOver = this.GameOver.destruct();
				}
				Bench.Pend("runPre GO");
				if (this.curMap == null)
				{
					return false;
				}
			}
			if (base.run(fcnt))
			{
				if (this.recheck_activation_flags && this.Ui != null)
				{
					this.recheck_activation_flags = false;
					if (!this.Ui.gameObject.activeSelf)
					{
						this.Ui.gameObject.SetActive(true);
					}
				}
				if (Map2d.TS != 0f)
				{
					Bench.P("runPre MGC");
					this.MGC.run(Map2d.TS);
					Bench.Pend("runPre MGC");
					Bench.P("runPre Mana");
					this.Mana.run(Map2d.TS);
					Bench.Pend("runPre Mana");
				}
				Bench.P("runPre 3");
				this.NightCon.run(Map2d.TS);
				Bench.Pend("runPre 3");
				Bench.P("runPre 4");
				if (this.curMap != null && !this.curMap.need_update_collider && !this.curMap.need_init_action && this.MIST.isFirstProgressRunning())
				{
					this.MIST.set_water_particle = false;
					int num = 10;
					while (--num >= 0)
					{
						if (!this.MIST.runMistWater(true))
						{
							this.MIST.clearFirstProgress();
							this.MIST.runCheckWaterManagementStatic();
							break;
						}
						if (!this.MIST.isFirstProgressRunning())
						{
							this.MIST.clearFirstProgress();
							break;
						}
					}
					this.MIST.fineDrawLevelWaterWatcher();
					this.MIST.set_water_particle = true;
				}
				PuzzLiner.run(this.curMap);
				Bench.Pend("runPre 4");
				if (!EV.bg_full_filled)
				{
					Bench.P("runPre 5");
					this.MIST.run();
					Bench.Pend("runPre 5");
					Bench.P("IMNG");
					this.IMNG.run(fcnt);
					Bench.Pend("IMNG");
				}
				Bench.P("PE");
				this.PE.runDraw(1f, true);
				Bench.Pend("PE");
				Bench.P("runPre DrawEf");
				if (X.D_EF)
				{
					this.PE.redrawOnlyMesh();
					this.NightCon.runWeather((float)X.AF_EF, this.curMap);
				}
				Bench.Pend("runPre DrawEf");
				Bench.P("runPre Draw");
				if (X.D)
				{
					if (this.PefRain != null)
					{
						if (this.PefRain.FnDef != null)
						{
							this.PefRain.fine(120);
						}
						else
						{
							this.PefRain = null;
						}
					}
					if (this.PeLowerBgm != null)
					{
						this.PeLowerBgm.fine(120);
					}
				}
				Bench.Pend("runPre Draw");
				Bench.P("runPre Other");
				if (this.AreaTitle.t >= 0f)
				{
					this.AreaTitle.run(fcnt);
				}
				Bench.Pend("runPre Other");
				return true;
			}
			if (this.NEvtListener.areatitle_hide_progress)
			{
				this.NEvtListener.areatitle_hide_progress = false;
				this.AreaTitle.hideProgress();
			}
			this.recheck_activation_flags = true;
			if (this.Ui != null && Map2d.editor_decline_lighting)
			{
				if (this.Ui.gameObject.activeSelf)
				{
					this.Ui.gameObject.SetActive(false);
				}
				this.IMNG.run(fcnt);
			}
			else if (!Map2d.editor_decline_lighting)
			{
				if (X.D_EF)
				{
					this.PE.redrawOnlyMesh();
				}
				this.PE.refineMaterialAlpha();
			}
			return this.GameOver != null && this.GameOver.isActive();
		}

		public override bool runPost(float fcnt)
		{
			if (X.D)
			{
				BetobetoManager.runAll(X.AF);
			}
			Bench.P("runPost 1");
			if (this.need_night_fine != 0)
			{
				this.fineDgnNightLevel(false, false);
			}
			if (this.curMap != null)
			{
				this.Iris.runPost(this.curMap.getKeyPr() as PR, Map2d.can_handle && this.pre_map_active);
			}
			Bench.Pend("runPost 1");
			Bench.P("runPost 4");
			bool flag = false;
			if (base.runPost(fcnt) && this.pre_map_active)
			{
				if ((this.menu_open_ & (NelM2DBase.MENU_OPEN)127) != NelM2DBase.MENU_OPEN.NONE)
				{
					if (this.GM.isActive())
					{
						this.menu_open_ &= NelM2DBase.MENU_OPEN._CANNOT_OPEN;
					}
					else
					{
						switch (this.menu_open_)
						{
						case NelM2DBase.MENU_OPEN.OPEN:
							this.menu_open_ = NelM2DBase.MENU_OPEN.NONE;
							this.GM.activateNormal();
							break;
						case NelM2DBase.MENU_OPEN.OPEN_MAP:
							this.menu_open_ = NelM2DBase.MENU_OPEN.NONE;
							this.GM.activateMap();
							break;
						case NelM2DBase.MENU_OPEN.OPEN_ITEM:
							this.menu_open_ = NelM2DBase.MENU_OPEN.NONE;
							this.GM.activateItem();
							break;
						}
					}
				}
				flag = true;
			}
			else
			{
				this.menu_open_ &= NelM2DBase.MENU_OPEN._CANNOT_OPEN;
			}
			Bench.Pend("runPost 4");
			return flag;
		}

		public override bool runui_enabled
		{
			get
			{
				return this.Ui.enabled;
			}
		}

		public override bool runPostForDraw(float fcnt, bool draw_flag = true, bool draw_finalaizecam_flag = true)
		{
			Bench.P("runPost Ui");
			this.Ui.run(fcnt);
			Bench.Pend("runPost Ui");
			if (this.GM.isStoppingGame() || this.curMap == null)
			{
				draw_flag = false;
			}
			else if (!this.pre_map_active && (this.GameOver == null || !this.GameOver.isActive()))
			{
				draw_flag = false;
			}
			return base.runPostForDraw(fcnt, draw_flag, draw_flag || this.isUiGmFading());
		}

		public bool can_open_gamemenu
		{
			get
			{
				return this.menu_open_ != NelM2DBase.MENU_OPEN._CANNOT_OPEN;
			}
			set
			{
				if (!value)
				{
					this.menu_open_ = NelM2DBase.MENU_OPEN._CANNOT_OPEN;
					return;
				}
				if (this.menu_open_ != NelM2DBase.MENU_OPEN._CANNOT_OPEN)
				{
					return;
				}
				this.menu_open_ = NelM2DBase.MENU_OPEN.NONE;
			}
		}

		public NelM2DBase.MENU_OPEN menu_open
		{
			set
			{
				if (this.menu_open_ == NelM2DBase.MENU_OPEN._CANNOT_OPEN || (base.transferring_game_stopping && value != NelM2DBase.MENU_OPEN.NONE))
				{
					return;
				}
				this.menu_open_ = value;
			}
		}

		public override M2DBase.MAP_TRANSFER transfer_mode
		{
			set
			{
				bool transferring_game_stopping = base.transferring_game_stopping;
				base.transfer_mode = value;
				if (!transferring_game_stopping && base.transferring_game_stopping && (M2DBase.material_flush_flag & M2DBase.FLUSH._FLUSH) == M2DBase.FLUSH._FLUSH)
				{
					if (this.Tips.fineEnabled())
					{
						this.Tips.activate();
						return;
					}
				}
				else if (transferring_game_stopping && !base.transferring_game_stopping)
				{
					this.Tips.autoDeact();
				}
			}
		}

		public bool isRbkPD()
		{
			return IN.isMapPD(1) && IN.isItmO(0) && this.IMNG.has_recipe_collection;
		}

		public override IEffectSetter getEffectTop()
		{
			return this.EFT;
		}

		public bool isUiGmFading()
		{
			return this.GM.isFading();
		}

		public static MeshDrawer PostEffectMesh(Material Mtr, EffectItem E, bool bottom_flag = true)
		{
			M2Camera cam = M2DBase.Instance.Cam;
			bool flag;
			MeshDrawer meshDrawer = M2DBase.Instance.getEffectTop().MeshInit(E.title, E.x, E.y, EffectItem.Col1.Transparent(), Mtr, out flag, bottom_flag, null, false);
			if (Mtr.shader == MTR.ShaderShiftImage)
			{
				meshDrawer.Col = MTRX.ColWhite;
			}
			else
			{
				meshDrawer.Col = MTRX.ColTrnsp;
			}
			meshDrawer.initForPostEffect(cam.getFinalizedTexture(), cam.PosMainTransform.x, cam.PosMainTransform.y);
			return meshDrawer;
		}

		public float rain_level
		{
			get
			{
				return this.rain_level_;
			}
			set
			{
				if (this.rain_level_ == value)
				{
					return;
				}
				this.rain_level_ = value;
				if (this.need_night_fine < 1)
				{
					this.need_night_fine = 1;
				}
			}
		}

		public override float ui_shift_x
		{
			get
			{
				return this.ui_shift_x_;
			}
			set
			{
				if (value == this.ui_shift_x_)
				{
					return;
				}
				base.ui_shift_x = value;
				this.PE.fineUiShift();
				this.Tips.reposit();
				if (this.AreaTitle.isActive())
				{
					this.AreaTitle.finePos();
				}
				if (this.GameOver != null)
				{
					this.GameOver.TxReposit();
				}
				this.NightCon.UiDg.repositGob(false);
			}
		}

		public override int getSF(string key)
		{
			return COOK.getSF(key);
		}

		public override void setSF(string key, int v)
		{
			COOK.setSF(key, v);
		}

		public override bool isSafeArea()
		{
			return this.WM.CurWM != null && this.WM.CurWM.safe_area;
		}

		public bool isDangerItemDisable()
		{
			return this.WM.CurWM != null && this.WM.CurWM.safe_area && !EnemySummoner.isActiveBorder() && !PUZ.IT.barrier_active;
		}

		public string cantFastTravel()
		{
			if (this.FlgFastTravelDeclined.isActive())
			{
				return "Alert_bench_execute_scenario_locked";
			}
			if (this.NightCon.debug_allow_night_travel && base.debug_listener_created)
			{
				return null;
			}
			if ((!this.NightCon.isNight() || this.NightCon.getBattleCount() <= 0) && !this.NightCon.hasWeather(WeatherItem.WEATHER.THUNDER))
			{
				return null;
			}
			return "Alert_cannot_fast_travel";
		}

		public bool canAccesableToHouseInventory()
		{
			return this.WM.CurWM != null && this.WM.CurWM.safe_area;
		}

		public override string getMapTitle(Map2d Mp = null)
		{
			return this.getMapTitle(Mp, null, true);
		}

		public string getMapTitle(Map2d Mp, WholeMapItem _Wm, bool wm_kakko = true)
		{
			Mp = Mp ?? this.curMap;
			string mapTitle = base.getMapTitle(Mp);
			if (!TX.noe(mapTitle))
			{
				return mapTitle;
			}
			if (_Wm == null)
			{
				_Wm = ((Mp == this.curMap) ? this.WM.CurWM : this.WM.GetWholeFor(Mp, true));
			}
			if (_Wm == null)
			{
				return "";
			}
			if (!wm_kakko)
			{
				return _Wm.localized_name;
			}
			return "(" + _Wm.localized_name + ")";
		}

		public string getMapTitleTxKey(Map2d Mp = null, WholeMapItem _Wm = null)
		{
			Mp = Mp ?? this.curMap;
			string text = ((Mp != null) ? ("MAP_" + Mp.key) : null);
			if (text != null && TX.getTX(text, true, true, null) != null)
			{
				return text;
			}
			if (_Wm == null)
			{
				_Wm = ((Mp == this.curMap) ? this.WM.CurWM : this.WM.GetWholeFor(Mp, true));
			}
			if (_Wm != null)
			{
				return "Area_" + _Wm.text_key;
			}
			return null;
		}

		public void flushLastExSituationTemp()
		{
			if (this.curMap == null)
			{
				return;
			}
			int count_players = this.curMap.count_players;
			for (int i = 0; i < count_players; i++)
			{
				PR pr = this.curMap.getPr(i) as PR;
				if (pr != null)
				{
					pr.EpCon.SituCon.flushLastExSituationTemp();
				}
			}
		}

		public override Dungeon getDgn(Map2d Mp)
		{
			if (this.WM.isWholeMap(Mp))
			{
				return base.getDgnByKey("_");
			}
			Mp.prepareMeta();
			string s = Mp.Meta.GetS("dgn");
			if (!TX.noe(s))
			{
				Dungeon dgnByKey = base.getDgnByKey(s);
				if (dgnByKey.key == s)
				{
					return dgnByKey;
				}
			}
			WholeMapItem wholeFor = this.WM.GetWholeFor(Mp, true);
			if (wholeFor != null)
			{
				Dungeon dgn = wholeFor.getDgn(Mp, false);
				if (dgn != null)
				{
					return dgn;
				}
			}
			return base.getDgn(Mp);
		}

		public override Map2d getWholeFor(Map2d Mp, bool alloc_empty = false)
		{
			WholeMapItem wholeMapItem = this.getWholeMapItem(Mp, alloc_empty);
			if (wholeMapItem == null)
			{
				return null;
			}
			return wholeMapItem.Mp;
		}

		public WholeMapItem getWholeMapItem(Map2d Mp, bool alloc_empty = false)
		{
			return this.WM.GetWholeFor(Mp, alloc_empty);
		}

		public override bool isWholeMap(Map2d Mp)
		{
			return this.WM.isWholeMap(Mp);
		}

		public override bool needInitMaterial(Map2d DestMap)
		{
			return this.curMap == null || DestMap == null || this.WM.GetWholeFor(DestMap, false) != this.WM.CurWM;
		}

		public override bool initMapBgm(Map2d NextMap = null, bool replace_bgm = false)
		{
			WholeMapItem wholeFor = this.WM.GetWholeFor(NextMap, false);
			string text = null;
			string scenarioBGM = SCN.getScenarioBGM(NextMap, wholeFor, ref text);
			if (scenarioBGM != null)
			{
				if (scenarioBGM == "")
				{
					BGM.fadeout(0f, 30f, true);
				}
				else
				{
					BGM.load(scenarioBGM, text, true);
					if (replace_bgm)
					{
						BGM.replace(90f, -1f, true, true);
						this.bgm_replaced = BGM.bgm_replace_id | 64;
					}
				}
				return true;
			}
			bool flag = false;
			if (base.initMapBgm(NextMap, replace_bgm))
			{
				flag = true;
			}
			else
			{
				if (NextMap == null)
				{
					NextMap = this.curMap;
				}
				if (NextMap == null)
				{
					return false;
				}
				if (base.initMapBgm(wholeFor, replace_bgm))
				{
					flag = true;
				}
			}
			if (NextMap == null)
			{
				NextMap = this.curMap;
			}
			if (NextMap != null && replace_bgm)
			{
				BGM.setOverrideKey(NextMap.Meta.GetS("block_override"), false);
			}
			return flag;
		}

		public override string checkWaterFootSound(M2Mover Mv, float x, float y, ref string snd_type)
		{
			if (!(Mv is PR))
			{
				return this.MIST.checkWaterFootSound(x, y, ref snd_type);
			}
			return snd_type;
		}

		public void fineSentToHouseInv()
		{
			if (this.reel_content_send_to_house_inventory)
			{
				this.reel_content_send_to_house_inventory = false;
				UILog.Instance.AddAlertTX("Alert_reel_content_has_been_sent", UILogRow.TYPE.ALERT);
			}
		}

		public void drawCamCenterDoughnut(MeshDrawer Md, float cen_mapx, float cen_mapy, float inner_circle_ratio_x = 0.5f, float inner_circle_ratio_y = 0.5f)
		{
			Md.base_x = this.ux2effectScreenx(this.curMap.pixel2ux(this.Cam.x));
			Md.base_y = this.uy2effectScreeny(this.curMap.pixel2uy(this.Cam.y));
			float num = (cen_mapx * this.CLEN - this.Cam.x) * this.curMap.base_scale;
			float num2 = -(cen_mapy * this.CLEN - this.Cam.y) * this.curMap.base_scale;
			float num3 = IN.w + 16f;
			float num4 = IN.h + 16f;
			if (Md.ColGrd.a == Md.Col.a)
			{
				Md.Rect(0f, 0f, num3, num4, false);
				return;
			}
			Md.InnerCircle(0f, 0f, num3, num4, num, num2, num3 * inner_circle_ratio_x, num4 * inner_circle_ratio_y, num3 * inner_circle_ratio_x / 4f, num4 * inner_circle_ratio_y / 4f, Md.ColGrd.a > 0, false, 0f, 1f);
		}

		public PRNoel getPrNoel()
		{
			return this.PlayerNoel;
		}

		public readonly MGContainer MGC;

		public readonly NelTipsManager Tips;

		private static bool first_activate = true;

		public UiGameMenu GM;

		public UIBase Ui;

		public EffectNel EF;

		public EffectNel EFT;

		public readonly StainManager STAIN;

		public MistManager MIST;

		public readonly WindManager WIND;

		public WholeMapManager WM;

		public readonly WanderingManager WDR;

		public WAManager WA;

		public readonly GuildManager GUILD;

		public readonly QuestTracker QUEST;

		public readonly EnMtrManager ENMTR;

		public MultiMeshRenderer PostMMRD;

		public M2ManaContainer Mana;

		public readonly NelItemManager IMNG;

		public readonly IrisOutManager Iris;

		public readonly UiGmMapMarker Marker;

		public readonly DarkSpotEffect DARKSPOT;

		public PlayerCheckPoint CheckPoint;

		public PostEffect PE;

		public readonly PUZ Puz;

		public readonly NightController NightCon;

		private bool recheck_activation_flags = true;

		public bool check_torture;

		public bool no_publish_juice;

		internal M2AreaTitle AreaTitle;

		internal M2MapTitle MapTitle;

		public readonly M2FreezeCamera Freezer;

		private EffectItem EfRain;

		private PostEffectItem PefRain;

		public M2SoundPlayerItem SndRain;

		public Flagger FlagOpenGm;

		public Flagger FlagRain;

		public Flagger FlgFastTravelDeclined;

		public Flagger FlgAreaTitleHide;

		public readonly Flagger FlgStopEvtMsgValotizeOverride;

		public BlurScreen BlurSc;

		public bool need_blursc_hiding_whole;

		private float rain_level_;

		public byte need_night_fine;

		private RainEffector RainFoot;

		public UiGO GameOver;

		public NelM2DEventListener NEvtListener;

		private bool saveposition_rewrite;

		public int default_mist_pose;

		public float pr_mp_consume_ratio = 1f;

		public bool reel_content_send_to_house_inventory;

		public static readonly Regex RegEvalSf = new Regex("^S[Ff]\\[([A-Za-z0-9\\.\\/_]+)\\]");

		public static readonly Regex RegEvalSfEvt = new Regex("^S[Ff]E[Vv][Tt]\\[([A-Za-z0-9\\.\\/_]+)\\]");

		private PostEffectItem PeLowerBgm;

		private NelM2DBase.MENU_OPEN menu_open_;

		private bool destructed;

		private readonly Action<bool> FD_EvDebuggerActive;

		public readonly FallenCutin FCutin;

		public PRNoel PlayerNoel;

		public enum MENU_OPEN : byte
		{
			NONE,
			OPEN,
			OPEN_MAP,
			OPEN_ITEM,
			_CANNOT_OPEN = 128
		}
	}
}
