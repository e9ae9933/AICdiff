using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using evt;
using m2d;
using nel.gm;
using PixelLiner;
using PixelLiner.PixelLinerLib;
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
		}

		public override void initGameObject(GameObject _GobBase, float cam_w, float cam_h, bool execute_load_finalize = true)
		{
			base.initGameObject(_GobBase, cam_w, cam_h, false);
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
			if (execute_load_finalize)
			{
				this.LoadAfterWhenMapScene();
			}
		}

		protected override void initEvent()
		{
			EV.initEvent(new NelMSGContainer(), null, this.TutoBox = new GameObject("Ev-Tutorial").AddComponent<TutorialBox>());
			EV.valotile_overwrite_msg = !this.FlgStopEvtMsgValotizeOverride.isActive();
			EV.addListener(NEL.Instance);
			EV.addWaitListener("ITEMDESC", this.IMNG.get_DescBox());
			EV.addWaitListener("REELMNG", this.IMNG.getReelManager());
			EV.addWaitListener("NIGHTCON", this.NightCon);
			EV.addWaitListener("UIGM_ACTIVATE", this.GM.WaitFnGMActivate());
			NEL.createTextLog().Clear();
			PuzzLiner.initG(this);
		}

		public override void stackFirstEvent()
		{
			if (this.EvLsn == null && EV.initDebugger(false))
			{
				this.EvLsn = new NelEvDebugListener(this);
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
			this.CheckPoint.newGame();
			this.GM.newGame();
			this.QUEST.newGame();
			TRMManager.newGame();
			this.FlgFastTravelDeclined.Clear();
			this.need_autosave = false;
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
			this.need_autosave = false;
			this.saveposition_rewrite = false;
			if (clear_gameover)
			{
				if (this.GameOver != null)
				{
					this.GameOver = this.GameOver.destruct();
				}
				COOK.newGame(this, false);
				this.PEClear();
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
			this.relaseEnemyDarkTexture();
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
				betoMng.prepareTexture(key, immediate);
			}
		}

		protected override void LoadAfter()
		{
			base.LoadAfter();
			this.WM = new WholeMapManager(this);
			this.WM.reloadScript();
			this.WA = new WAManager();
		}

		protected override void LoadAfterWhenMapScene()
		{
			this.WM.PrepareMapData();
			base.LoadAfterWhenMapScene();
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
			if (!global::XX.X.DEBUGSTABILIZE_DRAW)
			{
				this.Cam.assignRenderFunc(new CameraRenderBinderFunc(M.ToString() + "::EF-bottom", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (this.EF == null)
					{
						return true;
					}
					this.EF.RenderOneSide(true, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, 315f), M.Dgn.effect_layer_bottom, false, null);
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

		protected override void fineDgnHalfBgm(bool flag = true)
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
			this.NightCon.deactivate(true);
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
				NelM2DBase.AEnemyDark = null;
				M2CImgDrawerFootBell.flush();
				this.relaseEnemyDarkTexture();
				M2LpSummon.NearLpSmn = null;
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
			if (this.Ui != null)
			{
				this.Ui.transition_darken = false;
			}
		}

		protected override bool loadMapAsyncFinalize(Map2d Mp_Next)
		{
			if (this.curMap == null && (this.WM.CurWM == null || this.WM.CurWM.CurMap != Mp_Next))
			{
				this.initSWholeMap(Mp_Next);
			}
			if (!base.loadMapAsyncFinalize(Mp_Next))
			{
				SVD.unloadSound();
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
				if (!(cmd == "%PE"))
				{
					if (!(cmd == "%PE_fadeinout"))
					{
						if (cmd == "%PVIB" || cmd == "%PVIB2")
						{
							for (int i = 1; i < rER.clength; i++)
							{
								NEL.PadVib(rER.getIndex(i), 1f);
							}
							return true;
						}
						if (cmd == "%PVIB_NEAR")
						{
							float num = 1f - global::XX.X.ZPOW(global::XX.X.LENGTHXYS(this.Cam.x * this.curMap.rCLEN, this.Cam.y * this.curMap.rCLEN, rER.Nm(1, 0f), rER.Nm(2, 0f)) - 5f, 15f);
							if (num > 0f)
							{
								NEL.PadVib(rER._3, num);
							}
							return true;
						}
					}
					else
					{
						POSTM postm;
						if (!FEnum<POSTM>.TryParse(rER._1, out postm, true))
						{
							rER.tError("不明なPOSTM: " + rER._1);
							return true;
						}
						float num2 = rER.Nm(2, 40f);
						float num3 = rER.Nm(3, 40f);
						float num4 = rER.Nm(4, 1f);
						int num5 = rER.Int(5, 1);
						this.PE.setPEfadeinout(postm, num2, num3, num4, num5);
						return true;
					}
				}
				else
				{
					POSTM postm;
					if (!FEnum<POSTM>.TryParse(rER._1, out postm, true))
					{
						rER.tError("不明なPOSTM: " + rER._1);
						return true;
					}
					float num2 = rER.Nm(2, 40f);
					float num4 = rER.Nm(3, 1f);
					int num5 = rER.Int(4, 1);
					this.PE.setPE(postm, num2, num4, num5);
					return true;
				}
			}
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
				if (global::XX.X.DEBUGSTABILIZE_DRAW)
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
				effectNel.bottomBaseZ = 315f;
				EffectNel effectNel2;
				_EFT = (this.EFT = (effectNel2 = new EffectNel(M, 80)));
				effectNel2.effect_name = "EFT[" + M.ToString() + "]";
				effectNel2.initEffect("M2d EFT", this.Cam.get_FinalCamera(), new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel), EFCON_TYPE.NORMAL);
				effectNel2.setLayer(this.Cam.getFinalRenderedLayer(), this.Cam.getFinalSourceRenderedLayer());
				effectNel2.topBaseZ = 35f;
				effectNel2.bottomBaseZ = 55f;
				if (global::XX.X.DEBUGSTABILIZE_DRAW)
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
			bool flag = !global::XX.X.DEBUGSTABILIZE_DRAW;
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

		public override int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			string cmd2 = rER.cmd;
			if (cmd2 != null)
			{
				if (cmd2 == "PREPARE_SV_TEXTURE")
				{
					this.prepareSvTexture(rER._1, false);
					return 0;
				}
				if (cmd2 == "TX_BOARD")
				{
					if (rER._1.IndexOf("<<<") >= 0)
					{
						NelMSGContainer.checkHereDocument(rER._1, ER.name, null, rER, true, null, true);
					}
				}
			}
			return 0;
		}

		public override bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2034944466U)
				{
					if (num <= 1017903279U)
					{
						if (num <= 437340060U)
						{
							if (num <= 218985409U)
							{
								if (num <= 111508916U)
								{
									if (num <= 64859142U)
									{
										if (num != 52259617U)
										{
											if (num != 64859142U)
											{
												goto IL_3E56;
											}
											if (!(cmd == "SUMMONER_DEFEAT_NIGHT_PROGRESS"))
											{
												goto IL_3E56;
											}
											EnemySummoner enemySummoner = EnemySummoner.Get(rER._1, true);
											if (enemySummoner == null)
											{
												return rER.tError("不明な Summoner : " + rER._1);
											}
											if (this.NightCon.SummonerDefeated(enemySummoner, this.curMap, rER.Int(3, -1)) > 0 && rER.Nm(2, 1f) != 0f)
											{
												this.NightCon.showNightLevelAdditionUI();
											}
											return true;
										}
										else
										{
											if (!(cmd == "DISABLESKILL_NOANNOUNCE"))
											{
												goto IL_3E56;
											}
											goto IL_1C7F;
										}
									}
									else if (num != 81073948U)
									{
										if (num != 111508916U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "COFFEEMAKER_SET"))
										{
											goto IL_3E56;
										}
										Map2d map2d = base.Get(rER._1, false);
										if (map2d != null && SCN.isWNpcEnable(this, WanderingManager.TYPE.COF))
										{
											this.WDR.Get(WanderingManager.TYPE.COF).setCurrentPos(map2d, false);
										}
										return true;
									}
									else
									{
										if (!(cmd == "CONFIRM_WAIT_NIGHTINGALE"))
										{
											goto IL_3E56;
										}
										EV.initWaitFn(new GameObject("UiConfirmAreaChange").AddComponent<UiWarpConfirm>().Init(UiWarpConfirm.CTYPE.WAIT_NIGHTINGALE, null, null), 0);
										return true;
									}
								}
								else if (num <= 177837449U)
								{
									if (num != 167214176U)
									{
										if (num != 177837449U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "DEFEAT_EVENT2"))
										{
											goto IL_3E56;
										}
										goto IL_3C03;
									}
									else
									{
										if (!(cmd == "ENABLESKILL_NOANNOUNCE"))
										{
											goto IL_3E56;
										}
										goto IL_1C7F;
									}
								}
								else if (num != 188643197U)
								{
									if (num != 211388987U)
									{
										if (num != 218985409U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "GETITEM_SUPPLIER"))
										{
											goto IL_3E56;
										}
										goto IL_1868;
									}
									else
									{
										if (!(cmd == "UIGM"))
										{
											goto IL_3E56;
										}
										this.FlagOpenGm.Rem("EV");
										this.GM.EvtRead(rER);
										return true;
									}
								}
								else
								{
									if (!(cmd == "QU_HANDSHAKE"))
									{
										goto IL_3E56;
									}
									this.Cam.Qu.HandShake(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, 1f), rER.Int(4, 0));
									return true;
								}
							}
							else if (num <= 306224295U)
							{
								if (num <= 231640342U)
								{
									if (num != 224446392U)
									{
										if (num != 231640342U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "PR_ASSIGN_REVERT_POSITION"))
										{
											goto IL_3E56;
										}
										COOK.getCurrentFile().assignRevertPosition(this);
										return true;
									}
									else
									{
										if (!(cmd == "ABORT_PR_MAGIC"))
										{
											goto IL_3E56;
										}
										this.PlayerNoel.getSkillManager().killHoldMagic(false);
										return true;
									}
								}
								else if (num != 269575565U)
								{
									if (num != 306224295U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "WM_CHANGE_SAVE_NIGHT_PROGRESS"))
									{
										goto IL_3E56;
									}
									WholeMapItem byTextKey = this.WM.GetByTextKey(rER._1);
									if (byTextKey == null)
									{
										return rER.tError("不明な text_key WM: " + rER._1);
									}
									if (!byTextKey.safe_area)
									{
										byTextKey.reached_night_level = (ushort)global::XX.X.Mx((int)byTextKey.reached_night_level, this.NightCon.getDangerMeterVal(true));
									}
									return true;
								}
								else if (!(cmd == "GETMONEY"))
								{
									goto IL_3E56;
								}
							}
							else if (num <= 365229814U)
							{
								if (num != 340025110U)
								{
									if (num != 365229814U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "TITLECALL_HIDE"))
									{
										goto IL_3E56;
									}
									if (rER._B1)
									{
										this.AreaTitle.deactivate(true);
									}
									else
									{
										this.AreaTitle.hideProgress();
										this.areatitle_hide_progress = true;
									}
									return true;
								}
								else
								{
									if (!(cmd == "CONFIRM_AREA_CHANGE"))
									{
										goto IL_3E56;
									}
									global::XX.X.dl("WM " + rER._1 + " に移動します...", null, false, false);
									WholeMapItem wholeDescriptionByName = this.WM.GetWholeDescriptionByName(rER._1, false);
									UiWarpConfirm.CTYPE ctype = UiWarpConfirm.checkUseConfirm(this.WM.CurWM, wholeDescriptionByName);
									if (ctype != UiWarpConfirm.CTYPE.OFFLINE)
									{
										EV.initWaitFn(new GameObject("UiConfirmAreaChange").AddComponent<UiWarpConfirm>().Init(ctype, null, wholeDescriptionByName), 0);
									}
									else
									{
										EV.getVariableContainer().define("_result", "1", true);
									}
									return true;
								}
							}
							else if (num != 394793250U)
							{
								if (num != 404121968U)
								{
									if (num != 437340060U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "ENEMY_ADD_TICKET"))
									{
										goto IL_3E56;
									}
									if (!(M2EventCommand.EvMV is NelEnemy))
									{
										return rER.tError("EvMV が NelEnemy ではない");
									}
									(M2EventCommand.EvMV as NelEnemy).createTicketFromEvent(rER);
									return true;
								}
								else
								{
									if (!(cmd == "GETITEM"))
									{
										goto IL_3E56;
									}
									goto IL_1868;
								}
							}
							else
							{
								if (!(cmd == "DANGER_ADDITIONAL"))
								{
									goto IL_3E56;
								}
								int num2 = rER.Int(1, 5);
								this.NightCon.addAdditionalDangerLevel(ref num2, 0, true);
								return true;
							}
						}
						else if (num <= 738197758U)
						{
							if (num <= 648008802U)
							{
								if (num <= 483038532U)
								{
									if (num != 448273782U)
									{
										if (num != 483038532U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "DEFINE_WHOLEMAP"))
										{
											goto IL_3E56;
										}
										ER.VarCon.define(TX.valid(rER._1) ? rER._1 : "_", (this.WM.CurWM != null) ? this.WM.CurWM.text_key : "", true);
										return true;
									}
									else
									{
										if (!(cmd == "PRE_FLUSH_MAP"))
										{
											goto IL_3E56;
										}
										this.WDR.flush();
										return true;
									}
								}
								else if (num != 578731911U)
								{
									if (num != 648008802U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "PR_OMORASHI"))
									{
										goto IL_3E56;
									}
									PR pr = M2EventCommand.EvMV as PR;
									if (pr == null)
									{
										return rER.tError("M2EventCommand.EvMV が PR ではない");
									}
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
										pr.obtainSplashedNoelJuice(emwet, rER.Int(3, 0));
									}
									else if (pr.juice_stock > 0 || flag)
									{
										pr.executeSplashNoelJuice(false, true, 0, flag3, flag2, flag6, flag4);
									}
									return true;
								}
								else
								{
									if (!(cmd == "TOUCHITEM"))
									{
										goto IL_3E56;
									}
									NelItem byId = NelItem.GetById(rER._1, true);
									if (byId == null)
									{
										ER.de("不明なアイテム id:" + rER._1);
										return true;
									}
									byId.touchObtainCount();
									return true;
								}
							}
							else if (num <= 675895584U)
							{
								if (num != 666278659U)
								{
									if (num != 675895584U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "MANA_CLEAR"))
									{
										goto IL_3E56;
									}
									this.Mana.clear();
									return true;
								}
								else if (!(cmd == "GETMONEY_BOX"))
								{
									goto IL_3E56;
								}
							}
							else if (num != 706067804U)
							{
								if (num != 723036878U)
								{
									if (num != 738197758U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "PE"))
									{
										goto IL_3E56;
									}
									POSTM postm;
									if (!FEnum<POSTM>.TryParse(rER._1, out postm, true))
									{
										rER.tError("不明なPOSTM: " + rER._1);
										return true;
									}
									int num3 = rER.Int(2, 40);
									if (num3 < 0)
									{
										if (this.OPEevent == null)
										{
											return true;
										}
										PostEffectItem postEffectItem;
										if (this.OPEevent.TryGetValue(postm, out postEffectItem))
										{
											num3 = rER.Int(3, (int)postEffectItem.z);
											rER.Nm(4, postEffectItem.x);
											postEffectItem.deactivate(false);
											this.OPEevent.Remove(postm);
										}
									}
									else
									{
										if (this.OPEevent == null)
										{
											this.OPEevent = new BDic<POSTM, PostEffectItem>(1);
										}
										PostEffectItem postEffectItem2;
										if (!this.OPEevent.TryGetValue(postm, out postEffectItem2))
										{
											float num4 = rER.Nm(3, 1f);
											postEffectItem2 = this.PE.setPE(postm, (float)num3, num4, 0);
											if (postEffectItem2 != null)
											{
												this.OPEevent[postm] = postEffectItem2;
											}
										}
										else
										{
											float num5 = rER.Nm(3, postEffectItem2.x);
											postEffectItem2.x = num5;
											postEffectItem2.z = (float)num3;
										}
									}
									return true;
								}
								else
								{
									if (!(cmd == "SAVE_SAFEAREA_DEPERTURE"))
									{
										goto IL_3E56;
									}
									COOK.getCurrentFile().safe_area_memory = rER._1 + " " + rER._2;
									return true;
								}
							}
							else
							{
								if (!(cmd == "INIT_ITEM_REEL"))
								{
									goto IL_3E56;
								}
								if (this.NightCon.isUiActive())
								{
									this.NightCon.deactivate(true);
								}
								ReelManager.ItemReelContainer ir = ReelManager.GetIR(rER._1, false);
								if (ir == null)
								{
									global::XX.X.de("アイテムリール取得失敗:" + rER._1, null);
									return true;
								}
								ReelManager reelManager = new ReelManager(this.IMNG.getReelManager()).assignCurrentItemReel(ir, false);
								UiReelManager uiReelManager = reelManager.initUiState(ReelManager.MSTATE.OPENING_AUTO, null, true);
								if (!rER._B2)
								{
									uiReelManager.autodecide_progressable = false;
								}
								if (this.curMap != null && TX.valid(rER._3))
								{
									reelManager.assignDropLp(this.curMap.getLabelPoint(rER._3));
								}
								EV.initWaitFn(reelManager, 0);
								return true;
							}
						}
						else if (num <= 799739536U)
						{
							if (num <= 744731785U)
							{
								if (num != 740914389U)
								{
									if (num != 744731785U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "NEL_MAP_TRANSFER"))
									{
										goto IL_3E56;
									}
									M2LpMapTransferBase.executeTransfer(base.Get(rER._1, false), rER._2, rER._3);
									if (this.curMap != null)
									{
										EV.initWaitFn(this.curMap, 0);
									}
									return true;
								}
								else
								{
									if (!(cmd == "QU_VIB"))
									{
										goto IL_3E56;
									}
									this.Cam.Qu.Vib(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
									return true;
								}
							}
							else if (num != 795641719U)
							{
								if (num != 799739536U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "SER_APPLY_NEAR_PEE"))
								{
									goto IL_3E56;
								}
								PR pr2 = M2EventCommand.EvMV as PR;
								if (pr2 == null)
								{
									return rER.tError("M2EventCommand.EvMV が PR ではない");
								}
								if (rER.Int(1, 0) >= 1 && pr2.water_drunk < 93)
								{
									pr2.water_drunk = 93;
									pr2.Ser.checkSerExecute(true, true);
								}
								else if (pr2.water_drunk < 72)
								{
									pr2.water_drunk = 72;
									pr2.Ser.checkSerExecute(true, true);
								}
								return true;
							}
							else
							{
								if (!(cmd == "BENCH_RELOAD"))
								{
									goto IL_3E56;
								}
								UiBenchMenu.defineEvents(this.getPrNoel(), rER.getB(1, false));
								return true;
							}
						}
						else if (num <= 925299078U)
						{
							if (num != 908082156U)
							{
								if (num != 925299078U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "MAPTITLE_HIDE"))
								{
									goto IL_3E56;
								}
								this.MapTitle.deactivate(false, true);
								return true;
							}
							else
							{
								if (!(cmd == "UIALERT_ITEMHOLDOVER"))
								{
									goto IL_3E56;
								}
								NelItem byId2 = NelItem.GetById(rER._1, true);
								if (byId2 == null)
								{
									ER.de("不明なアイテム id:" + rER._1);
									return true;
								}
								UILog.Instance.AddAlert(TX.GetA("Alert_item_holdover", byId2.getLocalizedName(rER.Int(2, 0), null)), UILogRow.TYPE.ALERT);
								return true;
							}
						}
						else if (num != 950167350U)
						{
							if (num != 969352503U)
							{
								if (num != 1017903279U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "WEATHER_PROGRESS"))
								{
									goto IL_3E56;
								}
								if (rER._B1 && base.debug_listener_created && this.NightCon.debug_lock_weather)
								{
									return true;
								}
								this.NightCon.weatherShuffle();
								return true;
							}
							else
							{
								if (!(cmd == "NIGHTINGALE_SHUFFLE"))
								{
									goto IL_3E56;
								}
								this.WDR.getNightingale().blurDecided(rER._1);
								return true;
							}
						}
						else
						{
							if (!(cmd == "BENCH_SITDOWN"))
							{
								goto IL_3E56;
							}
							M2EventCommand.saveMv();
							this.NightCon.deactivate(true);
							rER.tError(M2EventCommand.focusEventMover(rER._1, false));
							if (M2EventCommand.EvMV == null)
							{
								rER.tError("移動スクリプト定義対象 M2Mover 未定義");
							}
							else if (M2EventCommand.EvMV is PR)
							{
								PR pr3 = M2EventCommand.EvMV as PR;
								int num6 = (int)rER._N2;
								M2Chip m2Chip = ((num6 == -1000) ? pr3.getNearBench(true, true) : this.curMap.findChip(num6, (int)rER._N3, "bench"));
								if (m2Chip != null)
								{
									NelChipBench nelChipBench = m2Chip as NelChipBench;
									if (nelChipBench != null)
									{
										pr3.initBenchSitDown(nelChipBench, rER._N4 != 0f, false);
									}
									else
									{
										rER.tError(string.Concat(new string[]
										{
											"対象ベンチが座標 ",
											rER._N2.ToString(),
											",",
											rER._N3.ToString(),
											"に存在しませんでした"
										}));
									}
								}
								else
								{
									rER.tError(string.Concat(new string[]
									{
										"対象ベンチが座標 ",
										rER._N2.ToString(),
										",",
										rER._N3.ToString(),
										"に存在しませんでした"
									}));
								}
							}
							else
							{
								rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
							}
							M2EventCommand.restoreMv();
							return true;
						}
						int num7 = rER.Int(1, 1);
						CoinStorage.addCount(num7, CoinStorage.CTYPE.GOLD, true);
						if (rER.cmd == "GETMONEY_BOX")
						{
							this.IMNG.get_DescBox().addTaskFocusMoney(num7);
						}
						return true;
					}
					if (num <= 1512790309U)
					{
						if (num <= 1153231774U)
						{
							if (num <= 1102987273U)
							{
								if (num <= 1089914396U)
								{
									if (num != 1039455231U)
									{
										if (num != 1089914396U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "CONFIRM_LOAD_SUCCESS"))
										{
											goto IL_3E56;
										}
										if (COOK.error_loaded_index >= 0)
										{
											this.change_scene_on_ev_quit = "SceneTitle";
											EV.getVariableContainer().define("_result", "0", true);
										}
										else
										{
											EV.getVariableContainer().define("_result", "-1", true);
										}
										return true;
									}
									else
									{
										if (!(cmd == "SF_SET"))
										{
											goto IL_3E56;
										}
										COOK.setSFcommandEval(rER._1, rER._2);
										return true;
									}
								}
								else if (num != 1098978204U)
								{
									if (num != 1102987273U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "HOUSE_FOOTBELL"))
									{
										goto IL_3E56;
									}
									if (rER.clength <= 2)
									{
										return rER.tError("サウンド名を列挙すること");
									}
									M2CImgDrawerFootBell.initializeBellPosition(rER._1, rER.slice(2, -1000));
									return true;
								}
								else
								{
									if (!(cmd == "EP_STATE_CLEAR"))
									{
										goto IL_3E56;
									}
									if (this.curMap.Pr is PR)
									{
										EpManager epCon = (this.curMap.Pr as PR).EpCon;
										epCon.newGame();
										epCon.fineCounter();
									}
									return true;
								}
							}
							else if (num <= 1113227260U)
							{
								if (num != 1111155619U)
								{
									if (num != 1113227260U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "UIP_VALOTIZE"))
									{
										goto IL_3E56;
									}
									if (rER._B1)
									{
										UIBase.FlgUiEffectDisable.Rem("EVENT");
									}
									else
									{
										UIBase.FlgUiEffectDisable.Add("EVENT");
									}
									return true;
								}
								else
								{
									if (!(cmd == "GAMEOVER_MAP_JUMP_TO"))
									{
										goto IL_3E56;
									}
									if (this.GameOver == null)
									{
										return rER.tError("UiGO がありません");
									}
									this.GameOver.GameoverMapJumpTo(rER._1);
									return true;
								}
							}
							else if (num != 1116662464U)
							{
								if (num != 1137130770U)
								{
									if (num != 1153231774U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "DEFINE_SF"))
									{
										goto IL_3E56;
									}
									ER.VarCon.define(rER._1, COOK.getSF(rER._2).ToString(), true);
									return true;
								}
								else
								{
									if (!(cmd == "MV_CURE"))
									{
										goto IL_3E56;
									}
									float n = rER._N1;
									float n2 = rER._N2;
									float n3 = rER._N3;
									if (!(M2EventCommand.EvMV is M2Attackable))
									{
										return rER.tError("M2EventCommand.EvMV が M2Attackable ではない");
									}
									M2Attackable m2Attackable = M2EventCommand.EvMV as M2Attackable;
									if (n > 0f)
									{
										m2Attackable.cureHp((int)n);
									}
									if (n2 > 0f)
									{
										m2Attackable.cureMp((int)n2);
									}
									if (m2Attackable is PR)
									{
										if (n3 != 0f)
										{
											(m2Attackable as PR).CureBench();
										}
										(m2Attackable as PR).recheck_emot = true;
									}
									else if (m2Attackable is NelEnemy && n3 != 0f)
									{
										(m2Attackable as NelEnemy).getSer().CureAll(true);
									}
									return true;
								}
							}
							else
							{
								if (!(cmd == "QU_SINV"))
								{
									goto IL_3E56;
								}
								this.Cam.Qu.SinV(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
								return true;
							}
						}
						else if (num <= 1231243708U)
						{
							if (num <= 1188747710U)
							{
								if (num != 1164727111U)
								{
									if (num != 1188747710U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "MIST_CLEAR"))
									{
										goto IL_3E56;
									}
									if (this.MIST != null)
									{
										this.MIST.clear(true);
									}
									return true;
								}
								else
								{
									if (!(cmd == "PR_ABSORB_EV_ASSIGN"))
									{
										goto IL_3E56;
									}
									EV.getVariableContainer().define("_absorb_result", "0", true);
									if (!(M2EventCommand.EvMV is PR))
									{
										rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
									}
									else if ((M2EventCommand.EvMV as PR).eventAbsorbBind(rER))
									{
										EV.getVariableContainer().define("_absorb_result", "1", true);
									}
									return true;
								}
							}
							else if (num != 1199814306U)
							{
								if (num != 1231243708U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "NEL_EXECUTE_FAST_TRAVEL"))
								{
									goto IL_3E56;
								}
								Map2d map2d2 = base.Get(rER._1, true);
								if (map2d2 == null)
								{
									rER.tError("マップが見つかりません: " + rER._1);
									return true;
								}
								M2LpMapTransferBase.executeTransferFastTravel(map2d2, rER.Int(2, 0), rER.Int(3, 0), rER.Int(4, 40));
								return true;
							}
							else
							{
								if (!(cmd == "QUEST_REMOVE"))
								{
									goto IL_3E56;
								}
								this.QUEST.remove(rER._1);
								return true;
							}
						}
						else if (num <= 1469531706U)
						{
							if (num != 1297749395U)
							{
								if (num != 1469531706U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "CFG_SET"))
								{
									goto IL_3E56;
								}
								CFG.changeConfigValue(rER._1, rER.Nm(2, 0f), null);
								return true;
							}
							else
							{
								if (!(cmd == "PR_OUTFIT"))
								{
									goto IL_3E56;
								}
								if (!(M2EventCommand.EvMV is PRNoel))
								{
									return rER.tError("M2EventCommand.EvMV が PR ではない");
								}
								PRNoel prnoel = M2EventCommand.EvMV as PRNoel;
								NoelAnimator.OUTFIT outfit;
								if (Enum.TryParse<NoelAnimator.OUTFIT>(rER._1, out outfit))
								{
									prnoel.setOutfitType(outfit, false, true);
									return true;
								}
								return rER.tError("不明なEnum " + rER._1);
							}
						}
						else if (num != 1488682444U)
						{
							if (num != 1497418882U)
							{
								if (num != 1512790309U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "ALLOW_FASTTRAVEL"))
								{
									goto IL_3E56;
								}
								this.FlgFastTravelDeclined.Rem("EVENT");
								return true;
							}
							else
							{
								if (!(cmd == "BENCH_UIPIC_SLIDE"))
								{
									goto IL_3E56;
								}
								bool flag7 = rER.Nm(1, 1f) != 0f;
								UIBase.Instance.gameMenuSlide(flag7, false);
								UIBase.Instance.gameMenuBenchSlide(flag7, false);
								return true;
							}
						}
						else
						{
							if (!(cmd == "WAIT_PR_EXPLODE_BURST"))
							{
								goto IL_3E56;
							}
							EV.initWaitFn(this.getPrNoel().getWaitListenerNoelBurst(), 0);
							return true;
						}
					}
					else if (num <= 1696697169U)
					{
						if (num <= 1619991034U)
						{
							if (num <= 1545025978U)
							{
								if (num != 1523898194U)
								{
									if (num != 1545025978U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "GETITEM_BOX"))
									{
										goto IL_3E56;
									}
								}
								else
								{
									if (!(cmd == "ENGINE_NNEA"))
									{
										goto IL_3E56;
									}
									MvNelNNEAListener.ReadEvtS(ER, rER, this.curMap);
									return true;
								}
							}
							else if (num != 1584889086U)
							{
								if (num != 1619991034U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "QU_SINH"))
								{
									goto IL_3E56;
								}
								this.Cam.Qu.SinH(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
								return true;
							}
							else
							{
								if (!(cmd == "ITEMMNG_POP_BYTES"))
								{
									goto IL_3E56;
								}
								ByteArray eventCacheBa = this.IMNG.EventCacheBa;
								if (eventCacheBa != null)
								{
									eventCacheBa.position = 0UL;
									this.IMNG.readBinaryFrom(eventCacheBa, false, true, false);
									this.IMNG.digestDropObjectCache();
								}
								else
								{
									global::XX.X.de("アイテムキャッシュ Ba がありません", null);
								}
								return true;
							}
						}
						else if (num <= 1648323204U)
						{
							if (num != 1637866189U)
							{
								if (num != 1648323204U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "REEL_FLUSH"))
								{
									goto IL_3E56;
								}
								if (this.NightCon.isUiActive())
								{
									this.NightCon.deactivate(true);
								}
								List<ItemStorage.IRow> list = new List<ItemStorage.IRow>();
								ReelManager reelManager2 = this.IMNG.getReelManager();
								if (this.IMNG.getInventory().getItemCountFn((ItemStorage.IRow IR) => IR.Data.is_reelmbox, list) > 0)
								{
									List<NelItemEntry> list2 = NelItemEntry.Clone(list);
									reelManager2.clearItemReelCache();
									reelManager2.assignCurrentItemReel(list2, false, true);
									this.IMNG.getInventory().Reduce(list2);
									UiReelManager uiReelManager2 = reelManager2.initUiState(rER._B1 ? ReelManager.MSTATE.REMOVE_REELS : ReelManager.MSTATE.OPENING, null, true);
									uiReelManager2.manual_deactivatable = false;
									uiReelManager2.after_clearreels = true;
									uiReelManager2.create_strage = true;
									uiReelManager2.no_draw_hidescreen = this.GameOver != null && this.GameOver.EvtWait(false);
									uiReelManager2.play_snd = !uiReelManager2.no_draw_hidescreen;
									uiReelManager2.prepareMBoxDrawer();
									if (rER._B2)
									{
										uiReelManager2.UiPictureStabilize();
									}
								}
								else if (reelManager2.getReelVector().Count > 0)
								{
									UiReelManager uiReelManager3 = reelManager2.initUiState(ReelManager.MSTATE.REMOVE_REELS, null, true);
									uiReelManager3.after_clearreels = true;
									uiReelManager3.create_strage = true;
									uiReelManager3.no_draw_hidescreen = this.GameOver != null && this.GameOver.EvtWait(false);
									uiReelManager3.play_snd = !uiReelManager3.no_draw_hidescreen;
									if (rER._B2)
									{
										uiReelManager3.UiPictureStabilize();
									}
								}
								else
								{
									this.IMNG.getReelManager().clearReels(false, true);
								}
								return true;
							}
							else
							{
								if (!(cmd == "PR_CURE"))
								{
									goto IL_3E56;
								}
								bool b = rER._B1;
								bool b2 = rER._B2;
								bool b3 = rER._B3;
								bool b4 = rER._B4;
								bool b5 = rER._B5;
								int num8 = rER.Int(6, 0);
								for (int i = this.curMap.count_players - 1; i >= 0; i--)
								{
									M2MoverPr pr4 = this.curMap.getPr(i);
									if (b)
									{
										pr4.cureHp((int)pr4.get_maxhp());
									}
									if (pr4 is PR)
									{
										PR pr5 = pr4 as PR;
										if (b)
										{
											pr5.setHpCrack(0);
										}
										if (b2)
										{
											pr5.cureFull(true, true, b4, false);
										}
										else if (b4)
										{
											pr5.EggCon.clear(true);
										}
										if (b3)
										{
											pr5.CureBench();
										}
										pr5.recheck_emot = true;
										if (b && b2)
										{
											pr5.water_drunk = global::XX.X.Mn(15, pr5.water_drunk);
											pr5.water_drunk_cache = 0;
										}
										if (num8 > 0)
										{
											pr5.cureSerDrunk1((float)num8);
										}
										pr5.Ser.checkSer();
									}
									else if (b2)
									{
										pr4.cureMp((int)pr4.get_maxmp());
									}
								}
								if (b5)
								{
									UiBenchMenu.executeOtherCommand("shower_clean_cure_cloth", false);
								}
								return true;
							}
						}
						else if (num != 1669423574U)
						{
							if (num != 1684521277U)
							{
								if (num != 1696697169U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "REMITEM_NOANNOUNCE"))
								{
									goto IL_3E56;
								}
								goto IL_19DC;
							}
							else
							{
								if (!(cmd == "DARKSPOT"))
								{
									goto IL_3E56;
								}
								goto IL_3B27;
							}
						}
						else
						{
							if (!(cmd == "NIGHTINGALE_SET"))
							{
								goto IL_3E56;
							}
							Map2d map2d3 = base.Get(rER._1, false);
							if (map2d3 != null && SCN.isWNpcEnable(this, WanderingManager.TYPE.NIG))
							{
								this.WDR.getNightingale().setCurrentPos(map2d3, false);
							}
							return true;
						}
					}
					else if (num <= 1788732065U)
					{
						if (num <= 1780750093U)
						{
							if (num != 1698386919U)
							{
								if (num != 1780750093U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "WAIT_PR_EXPLODE_MAGIC"))
								{
									goto IL_3E56;
								}
								EV.initWaitFn(this.getPrNoel().getWaitListenerNoelExplodeMagic(), 0);
								return true;
							}
							else
							{
								if (!(cmd == "INIT_MAP_BGM"))
								{
									goto IL_3E56;
								}
								Map2d map2d4 = base.Get(rER._1, false);
								if (map2d4 == null)
								{
									return rER.tError("Map2d が不明: " + rER._1);
								}
								this.initMapBgm(map2d4, false);
								return true;
							}
						}
						else if (num != 1783620490U)
						{
							if (num != 1788732065U)
							{
								goto IL_3E56;
							}
							if (!(cmd == "GETITEM_NOANNOUNCE"))
							{
								goto IL_3E56;
							}
						}
						else
						{
							if (!(cmd == "WA_DEPERTURE"))
							{
								goto IL_3E56;
							}
							this.WA.depertAssign(rER._1, rER._2, rER._3, rER._4);
							return true;
						}
					}
					else if (num <= 1878547986U)
					{
						if (num != 1789280585U)
						{
							if (num != 1878547986U)
							{
								goto IL_3E56;
							}
							if (!(cmd == "PR_GACHA"))
							{
								goto IL_3E56;
							}
							PR pr6 = M2EventCommand.EvMV as PR;
							if (pr6 == null)
							{
								return rER.tError("M2EventCommand.EvMV が PR ではない");
							}
							PrGachaItem.TYPE type;
							if (!FEnum<PrGachaItem.TYPE>.TryParse(rER._1, out type, true))
							{
								return rER.tError("不正なGacha TYPE:" + rER._1);
							}
							int num9 = rER.Int(2, 0);
							if (num9 <= 0)
							{
								return rER.tError("不正なtap_count:" + rER._2);
							}
							M2PrGachaEventHandler m2PrGachaEventHandler = new M2PrGachaEventHandler(pr6, type, num9);
							m2PrGachaEventHandler.Init(rER);
							EV.initWaitFn(m2PrGachaEventHandler, 0);
							return true;
						}
						else
						{
							if (!(cmd == "PR_ACTIVATE_THROW_RAY"))
							{
								goto IL_3E56;
							}
							if (!(M2EventCommand.EvMV is PR))
							{
								rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
							}
							else
							{
								(M2EventCommand.EvMV as PR).activateThrowRayForEvent(rER._B1);
							}
							return true;
						}
					}
					else if (num != 1879117195U)
					{
						if (num != 1894169261U)
						{
							if (num != 2034944466U)
							{
								goto IL_3E56;
							}
							if (!(cmd == "PR_VOICE"))
							{
								goto IL_3E56;
							}
							PR pr7;
							if (TX.valid(rER._2))
							{
								pr7 = M2EventCommand.getEventTargetMover(rER._2) as PR;
							}
							else
							{
								pr7 = M2EventCommand.EvMV as PR;
							}
							if (pr7 == null)
							{
								rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
							}
							else
							{
								pr7.playVo(rER._1, false, false);
							}
							return true;
						}
						else
						{
							if (!(cmd == "REMSKILL_NOANNOUNCE"))
							{
								goto IL_3E56;
							}
							goto IL_1C4B;
						}
					}
					else
					{
						if (!(cmd == "UI_FRONTLOG"))
						{
							goto IL_3E56;
						}
						if (rER.Nm(1, 1f) != 0f)
						{
							this.Ui.FlgFrontLog.Add("EVENT");
						}
						else
						{
							this.Ui.FlgFrontLog.Rem("EVENT");
						}
						return true;
					}
					IL_1868:
					NelItem byId3 = NelItem.GetById(rER._1, true);
					if (byId3 == null)
					{
						ER.de("不明なアイテム id:" + rER._1);
						return true;
					}
					int num10 = (int)rER.Nm(2, 1f);
					if (num10 < 0)
					{
						num10 = byId3.stock;
					}
					this.Ui.FlgFrontLog.Add("EVENT");
					int num11 = (int)rER.Nm(3, 0f);
					this.IMNG.getItem(byId3, num10, num11, rER.cmd != "GETITEM_NOANNOUNCE", rER.cmd != "GETITEM_SUPPLIER", false, false);
					if (rER.cmd == "GETITEM_BOX")
					{
						ItemDescBox descBox = this.IMNG.get_DescBox();
						if (byId3.is_enhancer)
						{
							descBox.addTaskFocus(EnhancerManager.Get(byId3), false);
						}
						else if (TX.isStart(byId3.key, "skillbook_", 0))
						{
							descBox.addTaskFocus(SkillManager.Get(byId3), false);
						}
						else
						{
							List<NelItemEntry> list3 = new List<NelItemEntry>(1)
							{
								new NelItemEntry(byId3, num10, (byte)num11)
							};
							descBox.addTaskFocus(list3);
						}
					}
					if (byId3.key == "enhancer_slot")
					{
						EnhancerManager.fineEnhancerStorage(this.IMNG.getInventoryPrecious(), this.IMNG.getInventoryEnhancer());
					}
					if (byId3.key == "recipe_collection")
					{
						this.IMNG.has_recipe_collection = true;
					}
					return true;
				}
				else
				{
					if (num <= 3140054473U)
					{
						if (num <= 2499757005U)
						{
							if (num <= 2295247786U)
							{
								if (num <= 2090980678U)
								{
									if (num <= 2054504399U)
									{
										if (num != 2035097996U)
										{
											if (num != 2054504399U)
											{
												goto IL_3E56;
											}
											if (!(cmd == "SETMAGIC_NOMANA"))
											{
												goto IL_3E56;
											}
										}
										else
										{
											if (!(cmd == "DANGER_LEVEL_INIT_BOX"))
											{
												goto IL_3E56;
											}
											M2LpMapTransferWarp m2LpMapTransferWarp = this.curMap.getPoint(rER._2, false) as M2LpMapTransferWarp;
											if (m2LpMapTransferWarp == null)
											{
												return true;
											}
											WholeMapItem byTextKey2 = this.WM.GetByTextKey(rER._1);
											List<string> list4 = new List<string>(2);
											if (SCN.getMovableHomeWholeMap(list4) > 0 || byTextKey2.reached_night_level >= 16)
											{
												EV.initWaitFn(new GameObject("UiDangerLevelInitBox").AddComponent<UiDangerLevelInitBox>().Init(byTextKey2, list4, m2LpMapTransferWarp), 0);
											}
											else
											{
												EV.getVariableContainer().define("_result", "0", true);
											}
											return true;
										}
									}
									else if (num != 2071059468U)
									{
										if (num != 2090980678U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "SER_APPLY"))
										{
											goto IL_3E56;
										}
										SER ser;
										if (!FEnum<SER>.TryParse(rER._1, out ser, true))
										{
											return rER.tError("不明なSER: " + rER._1);
										}
										int num12 = rER.Int(2, -1);
										int num13 = rER.Int(3, 99);
										if (M2EventCommand.EvMV is PR)
										{
											PR pr8 = M2EventCommand.EvMV as PR;
											pr8.Ser.Add(ser, num12, num13, false);
											pr8.recheck_emot = true;
										}
										else
										{
											if (!(M2EventCommand.EvMV is NelEnemy))
											{
												return rER.tError("M2EventCommand.EvMV が Ser を持っていない");
											}
											(M2EventCommand.EvMV as NelEnemy).getSer().Add(ser, num12, num13, false);
										}
										return true;
									}
									else
									{
										if (!(cmd == "BENCHMARK"))
										{
											goto IL_3E56;
										}
										TX.eval("SkillEnable[guard]", "");
										Bench.mark(rER._1, false, false);
										if (rER._1 == "txcheck")
										{
											int num14 = 20000;
											for (int j = 0; j < num14; j++)
											{
												TX.eval("NoelCasting[WHITEARROW]", "");
												TX.eval("SkillEnable[guard]", "");
											}
										}
										Bench.mark("", false, false);
										return true;
									}
								}
								else if (num <= 2268338539U)
								{
									if (num != 2099010007U)
									{
										if (num != 2268338539U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "WA_RECORD"))
										{
											goto IL_3E56;
										}
										this.WA.Touch(rER._1, rER._2, false, true);
										return true;
									}
									else
									{
										if (!(cmd == "SUMMONER_ACTIVATE"))
										{
											goto IL_3E56;
										}
										global::XX.X.dl("OPEN SUMMONER FROM EVENT!", null, false, false);
										M2LpSummon m2LpSummon = this.curMap.getPoint("Summon_" + rER._1, false) as M2LpSummon;
										if (m2LpSummon == null)
										{
											return rER.tError("Summon_" + rER._1 + " がありません");
										}
										m2LpSummon.openSummoner(M2EventCommand.EvMV, null, false);
										return true;
									}
								}
								else if (num != 2268747045U)
								{
									if (num != 2290201319U)
									{
										if (num != 2295247786U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "EPSITU_B"))
										{
											goto IL_3E56;
										}
										goto IL_250F;
									}
									else
									{
										if (!(cmd == "TRM_FINE"))
										{
											goto IL_3E56;
										}
										TRMManager.fineExecute(true);
										return true;
									}
								}
								else
								{
									if (!(cmd == "UI_RESTROOM_MENU"))
									{
										goto IL_3E56;
									}
									UiRestRoom uiRestRoom = UiRestRoom.CreateInstance();
									UiRestRoom.RSTATE rstate = UiRestRoom.RSTATE.OFFLINE;
									FEnum<UiRestRoom.RSTATE>.TryParse(rER._1, out rstate, true);
									uiRestRoom.Init(rstate);
									EV.initWaitFn(uiRestRoom, 0);
									return true;
								}
							}
							else if (num <= 2417376634U)
							{
								if (num <= 2333218688U)
								{
									if (num != 2310641853U)
									{
										if (num != 2333218688U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "GM_ALLOW_OPEN"))
										{
											goto IL_3E56;
										}
										this.FlagOpenGm.Rem("EV");
										return true;
									}
									else
									{
										if (!(cmd == "SVT_FLUSH"))
										{
											goto IL_3E56;
										}
										BetobetoManager.flushSpecificSvTexture(rER._1);
										return true;
									}
								}
								else if (num != 2357528558U)
								{
									if (num != 2417376634U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "TX_BOARD"))
									{
										goto IL_3E56;
									}
									string text = NelMSGContainer.checkHereDocument(rER._1, ER.name, "", (ER == rER) ? ER : null, true, null, false);
									if (text == null)
									{
										int num15 = rER._1.IndexOf("*");
										if (num15 == -1)
										{
											TX tx = TX.getTX(rER._1, true, false, null);
											if (tx == null)
											{
												return true;
											}
											text = tx.text;
										}
										else
										{
											using (BList<string> blist = ListBuffer<string>.Pop(0))
											{
												string text4;
												string text5;
												if (num15 == 0)
												{
													string text2 = ER.name;
													string text3 = TX.slice(rER._1, 1);
													text4 = text2;
													text5 = text3;
												}
												else
												{
													string text2 = TX.slice(rER._1, 0, num15);
													string text6 = TX.slice(rER._1, num15 + 1);
													text4 = text2;
													text5 = text6;
												}
												if (NelMSGResource.getContent(text4 + " " + text5, blist, false, false))
												{
													text = blist[0];
												}
												else
												{
													text = "<Unknown label: " + rER._1 + ">";
												}
											}
										}
									}
									NelItemManager.POPUP popup = NelItemManager.POPUP.FRAMED_BOARD;
									if (TX.valid(rER._2))
									{
										FEnum<NelItemManager.POPUP>.TryParse(rER._2, out popup, true);
									}
									if (rER._3.IndexOf('C') >= 0)
									{
										popup |= NelItemManager.POPUP._TX_CENTER;
									}
									this.IMNG.showPopUpAbs(popup | NelItemManager.POPUP._FOCUS_MODE, text, 0f, 50f);
									EV.initWaitFn(this.IMNG.get_DescBox(), 0);
									return true;
								}
								else
								{
									if (!(cmd == "DANGER"))
									{
										goto IL_3E56;
									}
									this.NightCon.applyDangerousFromEvent(rER.Int(1, 0), rER._B2, false);
									return true;
								}
							}
							else if (num <= 2453517722U)
							{
								if (num != 2421215947U)
								{
									if (num != 2453517722U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "BENCHMARK2"))
									{
										goto IL_3E56;
									}
									Bench.mark("txcheck2", false, true);
									Bench.P("txcheck2");
									List<string> list5 = new List<string>(1) { "WHITEARROW" };
									int num16 = 5000;
									for (int k = 0; k < num16; k++)
									{
										TX.evalLsnConvert("NoelCasting", list5);
									}
									Bench.mark("", false, false);
									Bench.Pend("txcheck2");
									return true;
								}
								else
								{
									if (!(cmd == "UI_ENABLE"))
									{
										goto IL_3E56;
									}
									this.Ui.setEnabled(true);
									return true;
								}
							}
							else if (num != 2463182076U)
							{
								if (num != 2498028900U)
								{
									if (num != 2499757005U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "TITLECALL"))
									{
										goto IL_3E56;
									}
									this.areatitle_hide_progress = false;
									this.AreaTitle.init(TX.Get(rER._1, ""), this.WM.CurWM.dark_area, rER.Nm(2, 0f), rER._B3, false);
									return true;
								}
								else
								{
									if (!(cmd == "PR_MASTURB"))
									{
										goto IL_3E56;
									}
									PR pr9 = M2EventCommand.EvMV as PR;
									ER.VarCon.define("_masturb_success", "-1", true);
									if (pr9 == null)
									{
										return rER.tError("M2EventCommand.EvMV が PR ではない");
									}
									M2PrMasturbate m2PrMasturbate = pr9.initMasturbation(true, false);
									if (m2PrMasturbate != null)
									{
										EV.initWaitFn(m2PrMasturbate, rER.Int(1, 0));
									}
									return true;
								}
							}
							else
							{
								if (!(cmd == "GETSKILL"))
								{
									goto IL_3E56;
								}
								goto IL_1BEB;
							}
						}
						else if (num <= 2806365720U)
						{
							if (num <= 2616714340U)
							{
								if (num <= 2561770047U)
								{
									if (num != 2511127767U)
									{
										if (num != 2561770047U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "GET_TRM_REWARD_BOX"))
										{
											goto IL_3E56;
										}
										ItemDescBox descBox2 = this.IMNG.get_DescBox();
										TRMManager.TRMReward currentReward = TRMManager.CurrentReward;
										if (currentReward != null)
										{
											descBox2.addTaskFocus(currentReward);
											TRMManager.CurrentReward = null;
										}
										return true;
									}
									else
									{
										if (!(cmd == "ENABLESKILL"))
										{
											goto IL_3E56;
										}
										goto IL_1C7F;
									}
								}
								else if (num != 2599844165U)
								{
									if (num != 2616714340U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "DARKSPOT_DEACTIVATE"))
									{
										goto IL_3E56;
									}
									goto IL_3B27;
								}
								else
								{
									if (!(cmd == "DANGER_MANUAL"))
									{
										goto IL_3E56;
									}
									this.NightCon.changeTo(rER.Nm(1, 0f), (rER.Nm(2, 0f) != 0f) ? 0 : 260);
									return true;
								}
							}
							else if (num <= 2697762179U)
							{
								if (num != 2650818810U)
								{
									if (num != 2697762179U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "COOK_ADD_WALK_COUNT"))
									{
										goto IL_3E56;
									}
									Map2d map2d5 = base.Get(rER._1, true);
									if (map2d5 != null && this.DGN.getDgn(map2d5) != this.Cam.CurDgn)
									{
										if (COOK.map_walk_count >= 30)
										{
											M2DBase.material_flush_flag |= M2DBase.FLUSH.MATERIAL | M2DBase.FLUSH.MAP;
										}
										else
										{
											COOK.map_walk_count++;
										}
									}
									else if (COOK.map_walk_count >= 50)
									{
										M2DBase.material_flush_flag |= M2DBase.FLUSH.MATERIAL | M2DBase.FLUSH.MAP;
									}
									else
									{
										COOK.map_walk_count += 4;
									}
									return true;
								}
								else
								{
									if (!(cmd == "PE_FADEINOUT"))
									{
										goto IL_3E56;
									}
									POSTM postm2;
									if (!FEnum<POSTM>.TryParse(rER._1, out postm2, true))
									{
										rER.tError("不明なPOSTM: " + rER._1);
										return true;
									}
									float num17 = rER.Nm(2, 40f);
									float num18 = rER.Nm(3, 40f);
									PostEffectItem postEffectItem3 = this.PE.setPEfadeinout(postm2, num17, num18, rER.Nm(4, 1f), rER.Int(5, 0));
									if (postEffectItem3 != null)
									{
										postEffectItem3.fine((int)(num17 * 2f + num18 + 120f));
									}
									return true;
								}
							}
							else if (num != 2791652511U)
							{
								if (num != 2803222434U)
								{
									if (num != 2806365720U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "UI_DISABLE"))
									{
										goto IL_3E56;
									}
									this.Ui.setEnabled(false);
									return true;
								}
								else
								{
									if (!(cmd == "EPSITU_FLUSH"))
									{
										goto IL_3E56;
									}
									this.flushLastExSituationTemp();
									return true;
								}
							}
							else
							{
								if (!(cmd == "AUTO_SAVE_BENCH"))
								{
									goto IL_3E56;
								}
								if (this.curMap == null || !(this.curMap.Pr is PR))
								{
									return rER.tError("PR ではない");
								}
								PR pr10 = this.curMap.Pr as PR;
								using (BList<M2Puts> blist2 = ListBuffer<M2Puts>.Pop(1))
								{
									this.curMap.getAllPointMetaPutsTo((int)pr10.x - 1, (int)(pr10.mbottom - 0.25f), 3, 1, blist2, "bench");
									if (blist2.Count == 0)
									{
										return rER.tError("近場にベンチがありません (ファストトラベルの失敗？)");
									}
									if (CFG.autosave_on_bench && SCN.canSave(true))
									{
										COOK.autoSave(this, true, false);
									}
									M2BlockColliderContainer.BCCLine lastBCC = pr10.getFootManager().get_LastBCC();
									this.CheckPoint.fineFoot(pr10, lastBCC, true);
								}
								return true;
							}
						}
						else if (num <= 2876823571U)
						{
							if (num <= 2863196886U)
							{
								if (num != 2829750464U)
								{
									if (num != 2863196886U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "SETMAGIC"))
									{
										goto IL_3E56;
									}
								}
								else
								{
									if (!(cmd == "GM_ITEMMOVE"))
									{
										goto IL_3E56;
									}
									this.FlagOpenGm.Rem("EV");
									if (this.menu_open_ != NelM2DBase.MENU_OPEN._CANNOT_OPEN)
									{
										this.menu_open_ = NelM2DBase.MENU_OPEN.NONE;
									}
									if (this.GM != null)
									{
										this.GM.activateItemMove();
										this.GM.category_to_quit = true;
										EV.initWaitFn(this.GM.WaitFnGMStopGame(), 0);
									}
									return true;
								}
							}
							else if (num != 2871566880U)
							{
								if (num != 2876823571U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "ITEMMNG_PUSH_BYTES"))
								{
									goto IL_3E56;
								}
								this.IMNG.EventCacheBa = this.IMNG.writeBinaryTo(null);
								return true;
							}
							else
							{
								if (!(cmd == "CLEAR_TREASURE_BOX_WM_CACHE"))
								{
									goto IL_3E56;
								}
								this.WM.CurWM.treasureBoxOpened(this.curMap);
								return true;
							}
						}
						else if (num <= 2923762633U)
						{
							if (num != 2877243756U)
							{
								if (num != 2923762633U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "SUMMONER_ACTIVATE_EFFECT"))
								{
									goto IL_3E56;
								}
								M2LpSummon.ACTV_EFFECT actv_EFFECT = M2LpSummon.ACTV_EFFECT.NORMAL;
								if (rER._1 != "" && !FEnum<M2LpSummon.ACTV_EFFECT>.TryParse(rER._1, out actv_EFFECT, true))
								{
									return rER.tError("不明なM2LpSummon.ACTV_EFFECT: " + rER._1);
								}
								M2LpSummon.summonerActivateEffect(this.curMap, actv_EFFECT, this.PlayerNoel.x, this.PlayerNoel.y, 16f);
								return true;
							}
							else
							{
								if (!(cmd == "REMSKILL"))
								{
									goto IL_3E56;
								}
								goto IL_1C4B;
							}
						}
						else if (num != 2978429305U)
						{
							if (num != 3006393793U)
							{
								if (num != 3140054473U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "SF_EVT_SET"))
								{
									goto IL_3E56;
								}
								COOK.setSFcommandEval("EV_" + ER.name, TX.noe(rER._1) ? "1" : rER._1);
								return true;
							}
							else
							{
								if (!(cmd == "DEFEAT_EVENT"))
								{
									goto IL_3E56;
								}
								goto IL_3C03;
							}
						}
						else
						{
							if (!(cmd == "TE_COLORBLINK_"))
							{
								goto IL_3E56;
							}
							try
							{
								rER.tError(M2EventCommand.focusEventMover(rER._1, false));
								if (M2EventCommand.EvMV is M2Attackable)
								{
									(M2EventCommand.EvMV as M2Attackable).TeCon.setColorBlink((float)rER.Int(2, 0), (float)rER.Int(3, 0), rER.Nm(4, 1f), rER.IntE(5, 16777215), rER.Int(6, 0));
								}
							}
							catch
							{
								rER.tError("TE_ パースエラー");
							}
							return true;
						}
						MGKIND mgkind;
						if (FEnum<MGKIND>.TryParse(rER._1, out mgkind, true))
						{
							M2MagicCaster m2MagicCaster = M2EventCommand.EvMV as M2MagicCaster;
							if (m2MagicCaster == null)
							{
								m2MagicCaster = this.PlayerNoel;
							}
							MagicItem magicItem = this.MGC.setMagic(m2MagicCaster, mgkind, ((rER._2 == "EN") ? MGHIT.EN : ((rER._2 == "EN|PR" || rER._2 == "PR|EN") ? MGHIT.BERSERK : MGHIT.PR)) | MGHIT.IMMEDIATE);
							Vector2 pos = this.curMap.getPos(rER._3, rER.Nm(4, 0f), rER.Nm(5, 0f), null);
							magicItem.sx = pos.x;
							magicItem.sy = pos.y;
							if (rER.cmd == "SETMAGIC_NOMANA")
							{
								magicItem.reduce_mp = 0;
							}
						}
						else
						{
							rER.tError("MGKIND 不明:" + rER._1);
						}
						return true;
					}
					if (num <= 3569452120U)
					{
						if (num <= 3236607995U)
						{
							if (num <= 3185205341U)
							{
								if (num <= 3154114209U)
								{
									if (num != 3141388709U)
									{
										if (num != 3154114209U)
										{
											goto IL_3E56;
										}
										if (!(cmd == "GM_DENY_OPEN"))
										{
											goto IL_3E56;
										}
										this.FlagOpenGm.Add("EV");
										return true;
									}
									else
									{
										if (!(cmd == "NEED_FINE_DEPERTURE"))
										{
											goto IL_3E56;
										}
										return true;
									}
								}
								else if (num != 3154698976U)
								{
									if (num != 3185205341U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "GETSKILL_NOANNOUNCE"))
									{
										goto IL_3E56;
									}
									goto IL_1BEB;
								}
								else
								{
									if (!(cmd == "NOEL_OUTFIT_TURNBACK"))
									{
										goto IL_3E56;
									}
									if (this.PlayerNoel.outfit_type == NoelAnimator.OUTFIT.BABYDOLL)
									{
										this.PlayerNoel.setOutfitType(NoelAnimator.OUTFIT.NORMAL, false, true);
									}
									return true;
								}
							}
							else if (num <= 3197668728U)
							{
								if (num != 3195761152U)
								{
									if (num != 3197668728U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "KILL_PR_MAGIC"))
									{
										goto IL_3E56;
									}
									this.MGC.killAllPlayerMagic(null, null);
									return true;
								}
								else
								{
									if (!(cmd == "PR_BETO"))
									{
										goto IL_3E56;
									}
									if (!(M2EventCommand.EvMV is PR))
									{
										rER.tError("移動スクリプト定義対象 M2Mover が PR ではありません ");
									}
									else if (!(M2EventCommand.EvMV as PR).BetoMng.addBetoFromEv(rER._1))
									{
										rER.tError("不明なBetoInfo:" + rER._1);
									}
									return true;
								}
							}
							else if (num != 3199224305U)
							{
								if (num != 3215738269U)
								{
									if (num != 3236607995U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "ENGINE"))
									{
										goto IL_3E56;
									}
									M2AttackableEventManipulatable.ReadEvtS(ER, rER, this.curMap);
									return true;
								}
								else
								{
									if (!(cmd == "HIDE_BLURSC"))
									{
										goto IL_3E56;
									}
									this.BlurSc.remFlag("__EVENT");
									return true;
								}
							}
							else if (!(cmd == "QUEST_FINISH"))
							{
								goto IL_3E56;
							}
						}
						else if (num <= 3327087531U)
						{
							if (num <= 3283502208U)
							{
								if (num != 3281843679U)
								{
									if (num != 3283502208U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "REMITEM"))
									{
										goto IL_3E56;
									}
									goto IL_19DC;
								}
								else
								{
									if (!(cmd == "UIALERT"))
									{
										goto IL_3E56;
									}
									UILogRow.TYPE type2 = UILogRow.TYPE.ALERT;
									if (rER.clength >= 3)
									{
										UILogRow.TYPE type3;
										if (FEnum<UILogRow.TYPE>.TryParse(rER._2, out type3, true))
										{
											type2 = type3;
										}
										else
										{
											rER.tError("不明なUILogRow.TYPE: " + rER._2);
										}
									}
									UILog.Instance.AddAlert(TX.Get(rER._1, ""), type2);
									return true;
								}
							}
							else if (num != 3314111417U)
							{
								if (num != 3327087531U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "UI_RESTROOM_MENU_HILIGHT"))
								{
									goto IL_3E56;
								}
								UiRestRoom.setHilight(rER._1);
								return true;
							}
							else
							{
								if (!(cmd == "PUZZ_WARP"))
								{
									goto IL_3E56;
								}
								NelChipBarrierLit.eventWarp(rER.Int(1, -1), rER.Int(2, -1), rER.Int(3, -1));
								return true;
							}
						}
						else if (num <= 3371354388U)
						{
							if (num != 3344825186U)
							{
								if (num != 3371354388U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "REMOVEMAGIC"))
								{
									goto IL_3E56;
								}
								MGKIND mgkind2;
								if (!FEnum<MGKIND>.TryParse(rER._1U, out mgkind2, true))
								{
									rER.tError("MGKIND パースエラー:" + rER._1);
								}
								else
								{
									this.getPrNoel().Skill.setMagicObtainFlag(mgkind2, false);
									if (mgkind2 == MGKIND.PR_BURST)
									{
										this.getPrNoel().Skill.BurstSel.fineBurstMagic();
									}
								}
								return true;
							}
							else
							{
								if (!(cmd == "BENCH_CMD_EXECUTE"))
								{
									goto IL_3E56;
								}
								UiBenchMenu.ExecuteBenchCmd(rER._1, rER.Int(2, 0), true, rER._B3);
								return true;
							}
						}
						else if (num != 3374162258U)
						{
							if (num != 3479742772U)
							{
								if (num != 3569452120U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "BENCH_CMD_EXECUTE_O"))
								{
									goto IL_3E56;
								}
								int num19 = UiBenchMenu.executeOtherCommand(rER._1, false);
								if (TX.valid(rER._2))
								{
									EV.getVariableContainer().define(rER._2, num19.ToString(), true);
								}
								return true;
							}
							else
							{
								if (!(cmd == "PREPARE_SV_TEXTURE"))
								{
									goto IL_3E56;
								}
								EV.initWaitFn(this.PlayerNoel.BetoMng, 0);
								return true;
							}
						}
						else
						{
							if (!(cmd == "GM_OPEN"))
							{
								goto IL_3E56;
							}
							this.FlagOpenGm.Rem("EV");
							if (this.menu_open_ != NelM2DBase.MENU_OPEN._CANNOT_OPEN)
							{
								this.menu_open_ = NelM2DBase.MENU_OPEN.NONE;
							}
							if (this.GM != null)
							{
								this.GM.activateNormal();
							}
							return true;
						}
					}
					else if (num <= 3919384666U)
					{
						if (num <= 3718600314U)
						{
							if (num <= 3674120902U)
							{
								if (num != 3638850237U)
								{
									if (num != 3674120902U)
									{
										goto IL_3E56;
									}
									if (!(cmd == "PR_WATER_DRUNK_MX"))
									{
										goto IL_3E56;
									}
									for (int l = this.curMap.count_players - 1; l >= 0; l--)
									{
										PR pr11 = this.curMap.getPr(l) as PR;
										if (pr11 != null)
										{
											pr11.water_drunk = global::XX.X.Mx(rER.Int(1, 0), pr11.water_drunk);
											pr11.Ser.checkSer();
										}
									}
									return true;
								}
								else
								{
									if (!(cmd == "FLUSHED_MAP"))
									{
										goto IL_3E56;
									}
									UiGameMenu.need_whole_map_reentry = true;
									if (NEL.flushState())
									{
										StoreManager.flushSoldItemsAll();
									}
									this.PlayerNoel.cureMpNotHunger(false);
									this.PlayerNoel.EggCon.worm_total = 0;
									this.NightCon.clearWithoutNightLevel();
									this.NightCon.clearPuppetRevengeCache(true, null);
									this.WM.assignStoreFlushFlag(false, true);
									this.IMNG.getReelManager().flushObtainableReel();
									return true;
								}
							}
							else if (num != 3681775455U)
							{
								if (num != 3718600314U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "DENY_FASTTRAVEL"))
								{
									goto IL_3E56;
								}
								this.FlgFastTravelDeclined.Add("EVENT");
								return true;
							}
							else
							{
								if (!(cmd == "WALKCOUNT"))
								{
									goto IL_3E56;
								}
								COOK.map_walk_count = rER.Int(1, COOK.map_walk_count);
								return true;
							}
						}
						else if (num <= 3787413069U)
						{
							if (num != 3724400530U)
							{
								if (num != 3787413069U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "STOREADD"))
								{
									goto IL_3E56;
								}
								if (TX.valid(rER._1))
								{
									StoreManager storeManager = StoreManager.Get(rER._1, true);
									if (storeManager == null)
									{
										rER.tError("不明な店ID: " + rER._1);
									}
									else
									{
										NelItem byId4 = NelItem.GetById(rER._2, true);
										if (byId4 == null)
										{
											rER.tError("不明な店ID: " + rER._1);
										}
										else
										{
											storeManager.Add(byId4, rER.Int(3, 1), rER.Int(4, 0), TX.valid(rER._5) ? rER._5 : "_", false);
										}
									}
								}
								return true;
							}
							else
							{
								if (!(cmd == "GETMAGIC"))
								{
									goto IL_3E56;
								}
								MGKIND mgkind3;
								if (!FEnum<MGKIND>.TryParse(rER._1U, out mgkind3, true))
								{
									rER.tError("MGKIND パースエラー:" + rER._1);
								}
								else
								{
									this.IMNG.get_DescBox().addTaskFocus(mgkind3);
									this.getPrNoel().Skill.setMagicObtainFlag(mgkind3, true);
								}
								return true;
							}
						}
						else if (num != 3818058978U)
						{
							if (num != 3911183430U)
							{
								if (num != 3919384666U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "SHOW_BLURSC"))
								{
									goto IL_3E56;
								}
								this.BlurSc.addFlag("__EVENT");
								return true;
							}
							else
							{
								if (!(cmd == "AUTO_SAVE"))
								{
									goto IL_3E56;
								}
								if (CFG.autosave_on_scenario)
								{
									this.need_autosave = true;
								}
								return true;
							}
						}
						else
						{
							if (!(cmd == "CLOSE_DESCBOX"))
							{
								goto IL_3E56;
							}
							this.IMNG.get_DescBox().clearStack();
							return true;
						}
					}
					else if (num <= 4026835330U)
					{
						if (num <= 4003384557U)
						{
							if (num != 3999105776U)
							{
								if (num != 4003384557U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "DANGER_INITIALIZE_MEMORY"))
								{
									goto IL_3E56;
								}
								this.NightCon.first_battle_dlevel = this.NightCon.getDangerMeterVal(false);
								return true;
							}
							else
							{
								if (!(cmd == "DISABLESKILL"))
								{
									goto IL_3E56;
								}
								goto IL_1C7F;
							}
						}
						else if (num != 4013445799U)
						{
							if (num != 4019167028U)
							{
								if (num != 4026835330U)
								{
									goto IL_3E56;
								}
								if (!(cmd == "DEFAULT_MIST_POSE"))
								{
									goto IL_3E56;
								}
								this.default_mist_pose = rER.Int(1, 0);
								return true;
							}
							else
							{
								if (!(cmd == "SCN_MANUAL_BGM_REPLACE"))
								{
									goto IL_3E56;
								}
								SCN.manual_bgm_replace = ((rER.clength >= 2) ? rER._1 : null);
								return true;
							}
						}
						else
						{
							if (!(cmd == "TITLE_POS_SHIFT"))
							{
								goto IL_3E56;
							}
							this.AreaTitle.finePosPixelShift(rER._N1, rER._N2);
							return true;
						}
					}
					else if (num <= 4189103509U)
					{
						if (num != 4071042880U)
						{
							if (num != 4189103509U)
							{
								goto IL_3E56;
							}
							if (!(cmd == "BENCH_AUTO_EXECUTE"))
							{
								goto IL_3E56;
							}
							PR pr12 = M2EventCommand.EvMV as PR;
							if (pr12 == null)
							{
								return rER.tError("M2EventCommand.EvMV が PR ではない");
							}
							UiBenchMenu.auto_start_stack_disable = false;
							UiBenchMenu.checkStackForceEvent(pr12, true, true);
							return true;
						}
						else
						{
							if (!(cmd == "STOREFLUSH"))
							{
								goto IL_3E56;
							}
							if (TX.valid(rER._1))
							{
								StoreManager storeManager2 = StoreManager.Get(rER._1, true);
								if (storeManager2 == null)
								{
									rER.tError("不明な店ID: " + rER._1);
								}
								else
								{
									storeManager2.need_summon_flush = StoreManager.MODE.FLUSH;
									storeManager2.need_reload_basic = true;
									if (rER._B2)
									{
										storeManager2.countItems();
									}
								}
							}
							else
							{
								StoreManager.FlushWanderingStore(StoreManager.MODE.FLUSH);
							}
							return true;
						}
					}
					else if (num != 4212641147U)
					{
						if (num != 4250681545U)
						{
							if (num != 4294659430U)
							{
								goto IL_3E56;
							}
							if (!(cmd == "SUMMONER_TYPE"))
							{
								goto IL_3E56;
							}
							M2LpSummon m2LpSummon2 = this.curMap.getPoint("Summon_" + rER._1, false) as M2LpSummon;
							if (m2LpSummon2 == null)
							{
								return rER.tError("Summon_" + rER._1 + " がありません");
							}
							m2LpSummon2.parseType(rER.slice_join(2, " ", ""));
							return true;
						}
						else if (!(cmd == "QUEST_PROGRESS"))
						{
							goto IL_3E56;
						}
					}
					else
					{
						if (!(cmd == "EPSITU"))
						{
							goto IL_3E56;
						}
						goto IL_250F;
					}
					string text7;
					int num20;
					if (rER.cmd == "QUEST_PROGRESS")
					{
						text7 = rER._3;
						num20 = rER.Int(2, 0);
					}
					else
					{
						text7 = rER._2;
						num20 = 1000;
					}
					this.QUEST.updateQuest(rER._1, num20, text7.IndexOf('H') >= 0, text7.IndexOf('C') >= 0, text7.IndexOf('F') >= 0);
					return true;
					IL_1BEB:
					PrSkill prSkill = SkillManager.Get(rER._1);
					if (prSkill == null)
					{
						rER.tError("スキルが不明:" + rER._1);
					}
					else
					{
						if (rER.cmd != "GETSKILL_NOANNOUNCE")
						{
							this.IMNG.get_DescBox().addTaskFocus(prSkill, false);
						}
						prSkill.Obtain(rER._B2);
					}
					return true;
					IL_250F:
					if (this.curMap == null || !(this.curMap.Pr is PR))
					{
						return rER.tError("PR ではない");
					}
					EpSituation situCon = (this.curMap.Pr as PR).EpCon.SituCon;
					if (rER.cmd == "EPSITU")
					{
						situCon.addTempSituation(rER._1, rER.Int(2, 1), false);
					}
					else if (rER.cmd == "EPSITU_B")
					{
						situCon.addTempSituation(rER._1, rER.Int(2, 1), true);
					}
					return true;
				}
				IL_19DC:
				NelItem byId5 = NelItem.GetById(rER._1, true);
				if (byId5 == null)
				{
					ER.de("不明なアイテム id:" + rER._1);
					return true;
				}
				int num21 = rER.Int(2, 1);
				if (num21 < 0)
				{
					num21 = byId5.stock;
				}
				int num22 = rER.Int(3, 0);
				int num23 = this.IMNG.reduceItem(byId5, num21, num22);
				if (num23 > 0 && rER.cmd != "REMITEM_NOANNOUNCE")
				{
					string text8 = "Alert_item_reduced";
					if (TX.valid(rER._4))
					{
						text8 = rER._4;
					}
					UILog.Instance.AddAlert(TX.GetA(text8, byId5.getLocalizedName(num22, null), num23.ToString()), UILogRow.TYPE.ALERT).setIcon(MTR.AItemIcon[byId5.getIcon(this.IMNG.getHouseInventory(), null)], C32.c2d(byId5.getColor(this.IMNG.getHouseInventory())));
				}
				return true;
				IL_1C4B:
				PrSkill prSkill2 = SkillManager.Get(rER._1);
				if (prSkill2 == null)
				{
					rER.tError("スキルが不明:" + rER._1);
				}
				else
				{
					prSkill2.ReleaseObtain();
				}
				return true;
				IL_1C7F:
				PrSkill prSkill3 = SkillManager.Get(rER._1);
				if (prSkill3 == null)
				{
					rER.tError("スキルが不明:" + rER._1);
				}
				else
				{
					bool flag8 = rER.cmd.IndexOf("ENABLESKILL") == 0;
					if (prSkill3.enabled != flag8 && prSkill3.visible)
					{
						prSkill3.enabled = flag8;
						this.getPrNoel().Skill.resetSkillConnection(false, false, false);
					}
				}
				return true;
				IL_3B27:
				if (M2EventCommand.EvMV == null)
				{
					rER.tError("移動スクリプト定義対象 M2Mover がありません ");
					return true;
				}
				DarkSpotEffect.SPOT spot;
				if (!FEnum<DarkSpotEffect.SPOT>.TryParse(rER._1, out spot, true))
				{
					spot = DarkSpotEffect.SPOT.FILL;
				}
				if (rER.cmd == "DARKSPOT")
				{
					DarkSpotEffect.DarkSpotEffectItem darkSpotEffectItem = this.DARKSPOT.Set(M2EventCommand.EvMV, spot);
					darkSpotEffectItem.BaseCol = C32.d2c(global::XX.X.NmUI(rER._2, 4278190080U, false, true));
					darkSpotEffectItem.add_light_color = rER.Nm(3, 0f);
					darkSpotEffectItem.mul_light_color = rER.Nm(4, 1f);
					darkSpotEffectItem.sub_alpha = rER.Nm(5, 0.2f);
				}
				else
				{
					this.DARKSPOT.deactivateFor(M2EventCommand.EvMV, spot);
				}
				return true;
				IL_3C03:
				this.PlayerNoel.getSkillManager().killHoldMagic(false);
				return base.EvtRead(ER, rER, skipping);
			}
			IL_3E56:
			return this.Ui.fnEvReadUI(ER, rER, skipping) || base.EvtRead(ER, rER, skipping);
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
				this.areatitle_hide_progress = true;
				this.NightCon.deactivate(true);
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
				if (this.OPEevent != null)
				{
					foreach (KeyValuePair<POSTM, PostEffectItem> keyValuePair in this.OPEevent)
					{
						keyValuePair.Value.deactivate(false);
					}
					this.OPEevent = null;
				}
				this.IMNG.getReelManager().digestObtainedMoney().digestObtainedItem(true);
				this.fineSentToHouseInv();
				if (this.need_autosave && COOK.autoSave(this, false, false) != null)
				{
					this.need_autosave = false;
				}
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
			if (this.change_scene_on_ev_quit != null)
			{
				this.quitGame(this.change_scene_on_ev_quit);
				this.transferring_game_stopping = true;
			}
			return true;
		}

		public override TxEvalListenerContainer createListenerEval(int cap_fn = 0)
		{
			TxEvalListenerContainer txEvalListenerContainer = base.createListenerEval(cap_fn + 22);
			txEvalListenerContainer.Add("masturbate_count", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)this.getPrNoel().EpCon.masturbate_count);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("difficulty", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)DIFF.I);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("summoner_active", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(EnemySummoner.isActiveBorder() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_torned", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (global::XX.X.SENSITIVE)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE((float)(this.getPrNoel().BetoMng.is_torned ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_ep", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (global::XX.X.SENSITIVE)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE((float)this.getPrNoel().ep);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("pr_egged", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (!(M2EventCommand.EvMV is PR))
				{
					global::XX.X.de("M2EventCommand.EvMV が PR ではない", null);
					return;
				}
				TX.InputE((float)(M2EventCommand.EvMV as PR).EggCon.total);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_wetten", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (global::XX.X.SENSITIVE)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE(this.getPrNoel().BetoMng.wetten);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_cloth_dirty", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (CFG.ui_effect_dirty == 0 || CFG.ui_effect_density == 0)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE((float)(this.getPrNoel().BetoMng.isActive() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_carrying_box", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)((this.getPrNoel().getSkillManager().getCarryingBox() != null) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("stomach", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE(this.IMNG.StmNoel.eaten_cost);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("danger_level", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)this.NightCon.getDangerMeterVal(true));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("is_night", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(this.NightCon.isNight() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("load_version", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				SVD.sFile currentFile = COOK.getCurrentFile();
				TX.InputE((float)((currentFile != null) ? currentFile.version : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("summoner_barrier_active", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(EnemySummoner.isActiveBorder() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("summoner_defeated_this_session", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				NightController.SummonerData lpInfo = this.NightCon.GetLpInfo(EnemySummoner.Get(Aargs[0], true), base.Get((Aargs.Count == 1) ? Aargs[0] : Aargs[1], false), false);
				TX.InputE((float)((lpInfo != null && lpInfo.defeated_in_session) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noel_bote", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				float nm = global::XX.X.GetNm(Aargs, (float)CFG.sp_threshold_pregnant * 0.01f, true, 0);
				PRNoel prNoel = this.getPrNoel();
				TX.InputE((float)((prNoel.EggCon.total > (int)(prNoel.get_maxmp() * nm)) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("walk_xspeed", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				Map2d curMap = M2DBase.Instance.curMap;
				if (curMap == null || Aargs.Count == 0)
				{
					return;
				}
				M2Mover moverByName = curMap.getMoverByName(Aargs[0], false);
				TX.InputE((moverByName is PR) ? (moverByName as PR).get_walk_xspeed() : ((moverByName != null) ? moverByName.vx : 0f));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("crouch", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				Map2d curMap2 = M2DBase.Instance.curMap;
				if (curMap2 == null || Aargs.Count == 0)
				{
					return;
				}
				M2Mover moverByName2 = curMap2.getMoverByName(Aargs[0], false);
				if (moverByName2 is PR)
				{
					TX.InputE((float)((moverByName2 as PR).isPoseCrouch(false) ? 1 : 0));
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SF", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)COOK.getSF(global::XX.X.Get<string>(Aargs, 0)));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SfEvt", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)COOK.getSF("EV_" + global::XX.X.Get<string>(Aargs, 0, "")));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("spcfg_enable", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				TX.InputE((float)(CFG.isSpEnabled(Aargs[0]) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("fatal_watched", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				TX.InputE((float)(MGV.isFatalSceneAlreadyWatched(Aargs[0]) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("visitted", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				Map2d map2d = base.Get(Aargs[0], false);
				if (map2d == null)
				{
					return;
				}
				WholeMapItem wholeFor = this.WM.GetWholeFor(map2d, true);
				if (wholeFor != null)
				{
					TX.InputE((float)(wholeFor.isVisitted(map2d) ? 1 : 0));
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SkillHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				PrSkill prSkill = SkillManager.Get(Aargs[0]);
				if (prSkill == null)
				{
					global::XX.X.de("unknown skill: " + Aargs[0], null);
					return;
				}
				TX.InputE((float)(prSkill.visible ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("MagicHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				int num = 0;
				MGKIND mgkind;
				if (!FEnum<MGKIND>.TryParse(Aargs[0], out mgkind, true))
				{
					global::XX.X.de("MGKIND パースエラー:" + Aargs[0], null);
				}
				else
				{
					num = (MagicSelector.isObtained(mgkind) ? 1 : 0);
				}
				TX.InputE((float)num);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SkillEnable", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				PrSkill prSkill2 = SkillManager.Get(Aargs[0]);
				if (prSkill2 == null)
				{
					global::XX.X.de("unknown skill: " + Aargs[0], null);
					return;
				}
				TX.InputE((float)((prSkill2.visible && prSkill2.enabled) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("ItemHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				int nmI = global::XX.X.GetNmI(Aargs, -1, true, 1);
				NelItem byId = NelItem.GetById(Aargs[0], false);
				if (byId == null)
				{
					return;
				}
				ItemStorage[] inventoryArray = this.IMNG.getInventoryArray();
				int num2 = 0;
				for (int i = inventoryArray.Length - 1; i >= 0; i--)
				{
					num2 += inventoryArray[i].getCountMoreGrade(byId, nmI);
				}
				TX.InputE((float)num2);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("empty_bottle", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				ItemStorage[] inventoryArray2 = this.IMNG.getInventoryArray();
				int num3 = 0;
				for (int j = inventoryArray2.Length - 1; j >= 0; j--)
				{
					ItemStorage itemStorage = inventoryArray2[j];
					if (itemStorage == this.IMNG.getInventory())
					{
						num3 += itemStorage.getEmptyBottleCount();
					}
					else
					{
						num3 += itemStorage.getCount(NelItem.Bottle, -1);
					}
				}
				TX.InputE((float)num3);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("StoreItemCount", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				StoreManager storeManager = StoreManager.Get(Aargs[0], false);
				if (storeManager != null)
				{
					TX.InputE((float)storeManager.countItems());
					return;
				}
				TX.InputE(0f);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("NoelCasting", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				string text = Aargs[0];
				MGKIND mgkind2;
				if (!FEnum<MGKIND>.TryParse(text, out mgkind2, true))
				{
					global::XX.X.de("不明なMGKIND: " + text, null);
					return;
				}
				TX.InputE((float)(this.getPrNoel().isCastingSpecificMagic(mgkind2) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("item_capacity", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				NelItem byId2 = NelItem.GetById(Aargs[0], false);
				if (byId2 != null)
				{
					TX.InputE((float)this.IMNG.getInventory().getItemCapacity(byId2, false, false));
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("quest_progress", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				TX.InputE((float)this.QUEST.getProgress(Aargs[0]));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("noelRPI", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				RecipeManager.RPI_EFFECT rpi_EFFECT;
				if (!FEnum<RecipeManager.RPI_EFFECT>.TryParse(Aargs[0], out rpi_EFFECT, true))
				{
					global::XX.X.de("不明なRecipeManager.RPI_EFFECT: " + Aargs[0], null);
					return;
				}
				TX.InputE(this.getPrNoel().getRE(rpi_EFFECT));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SerHas", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				SER ser;
				if (!FEnum<SER>.TryParse(Aargs[0], out ser, true))
				{
					global::XX.X.de("不明なSER: " + Aargs[0], null);
					return;
				}
				TX.InputE((float)(this.getPrNoel().Ser.has(ser) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SerLevel", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				SER ser2;
				if (!FEnum<SER>.TryParse(Aargs[0], out ser2, true))
				{
					global::XX.X.de("不明なSER: " + Aargs[0], null);
					return;
				}
				TX.InputE((float)this.getPrNoel().Ser.getLevel(ser2));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("trm_has_newer_item", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)TRMManager.hasNewerItem(false));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("craft_ui_active", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(NEL.isCraftUiActive() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("alchemy_lectured", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(SCN.alchemy_lectured ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("CFG", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				if (Aargs.Count == 0)
				{
					return;
				}
				string text2 = Aargs[0];
				if (text2 != null)
				{
					if (text2 == "autorun")
					{
						TX.InputE(M2MoverPr.jump_press_reverse);
						return;
					}
					if (text2 == "stick_thresh")
					{
						TX.InputE(M2MoverPr.running_thresh);
						return;
					}
					if (!(text2 == "bgm_volume"))
					{
						return;
					}
					TX.InputE(SND.bgm_volume01);
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("nightingale_here", delegate(TxEvalListenerContainer _, List<string> Aargs)
			{
				TX.InputE((float)(this.WDR.isNightingaleHere(this.curMap) ? 1 : 0));
			}, Array.Empty<string>());
			EnemySummoner.prepareListenerEval(txEvalListenerContainer);
			return txEvalListenerContainer;
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
			IMessageContainer messageContainer = EV.getMessageContainer();
			if (messageContainer != null)
			{
				messageContainer.fineTargetFont();
			}
		}

		public override void assignTalkableObject(M2MoverPr FromMv, int x, int y, global::XX.AIM k, List<IM2TalkableObject> ATk)
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
					if (EnemySummoner.ActiveScript != null)
					{
						EnemySummoner.ActiveScript.check(Map2d.TS);
					}
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
				if (global::XX.X.D_EF)
				{
					this.PE.redrawOnlyMesh();
					this.NightCon.runWeather((float)global::XX.X.AF_EF, this.curMap);
				}
				Bench.Pend("runPre DrawEf");
				Bench.P("runPre Draw");
				if (global::XX.X.D)
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
					if (NelM2DBase.AEnemyDark != null)
					{
						int num2 = (int)this.curMap.floort >> 3;
						if (NelM2DBase.enemydark_t != num2)
						{
							NelM2DBase.enemydark_t = num2;
							this.fineEnemyDarkTexture(this.curMap);
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
			if (this.areatitle_hide_progress)
			{
				this.areatitle_hide_progress = false;
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
				if (global::XX.X.D_EF)
				{
					this.PE.redrawOnlyMesh();
				}
				this.PE.refineMaterialAlpha();
			}
			return this.GameOver != null && this.GameOver.isActive();
		}

		public override bool runPost(float fcnt)
		{
			if (global::XX.X.D)
			{
				BetobetoManager.runAll(global::XX.X.AF);
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
			Bench.P("runPost 3");
			if (this.OPEevent != null && (IN.totalframe & 63) == 0)
			{
				foreach (KeyValuePair<POSTM, PostEffectItem> keyValuePair in this.OPEevent)
				{
					if (keyValuePair.Value != null)
					{
						keyValuePair.Value.fine(120);
					}
				}
			}
			Bench.Pend("runPost 3");
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
				if (this.menu_open_ == NelM2DBase.MENU_OPEN._CANNOT_OPEN || this.transferring_game_stopping)
				{
					return;
				}
				this.menu_open_ = value;
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

		public void relaseEnemyDarkTexture()
		{
			NelM2DBase.AEnemyDark = null;
		}

		public void addEnemyDarkTexture(Material Mtr)
		{
			if (NelM2DBase.AEnemyDark == null)
			{
				NelM2DBase.AEnemyDark = new List<Material>(16);
			}
			MTRX.setMaterialST(Mtr, "_DarkTex", MTRX.SqEfPattern.getImage(9, 0), 0f);
			if (NelM2DBase.AEnemyDark.IndexOf(Mtr) == -1)
			{
				NelM2DBase.AEnemyDark.Add(Mtr);
			}
		}

		public void remEnemyDarkTexture(Material Mtr)
		{
			if (NelM2DBase.AEnemyDark != null)
			{
				NelM2DBase.AEnemyDark.Remove(Mtr);
			}
		}

		private void fineEnemyDarkTexture(Map2d _curMap)
		{
			if (NelM2DBase.AEnemyDark == null || _curMap == null)
			{
				return;
			}
			EnemyMeshDrawer.add_color_white_blend_level = 0.5f + global::XX.X.COSI(_curMap.floort, 7.4f) * 0.25f + global::XX.X.COSI(_curMap.floort, 11.3f) * 0.25f;
			float num = 256f * (0.5f + 0.2f * global::XX.X.SINI((float)NelM2DBase.enemydark_t, 332f) + 0.2f * global::XX.X.SINI((float)(NelM2DBase.enemydark_t + 190), 275f));
			float num2 = 256f * global::XX.X.frac(1f + global::XX.X.ANMP(NelM2DBase.enemydark_t, 149, 1f) + 0.2f * global::XX.X.COSI((float)NelM2DBase.enemydark_t, 393f));
			for (int i = NelM2DBase.AEnemyDark.Count - 1; i >= 0; i--)
			{
				Material material = NelM2DBase.AEnemyDark[i];
				material.SetFloat("_DarkOffsetX", num);
				material.SetFloat("_DarkOffsetY", num2);
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
				if (this.AreaTitle.isActive())
				{
					this.AreaTitle.finePos();
				}
				if (this.GameOver != null)
				{
					this.GameOver.TxReposit();
				}
				this.NightCon.repositGob(false);
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

		protected override bool initMapBgm(Map2d NextMap = null, bool replace_bgm = false)
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
				BGM.setOverrideKey(NextMap.Meta.GetS("block_override"));
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

		public readonly QuestTracker QUEST;

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

		public static int enemydark_t = -1;

		public static List<Material> AEnemyDark;

		public bool check_torture;

		private M2AreaTitle AreaTitle;

		internal M2MapTitle MapTitle;

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

		private bool need_autosave;

		private bool saveposition_rewrite;

		private string change_scene_on_ev_quit;

		public int default_mist_pose;

		public float pr_mp_consume_ratio = 1f;

		public bool reel_content_send_to_house_inventory;

		public static readonly Regex RegEvalSf = new Regex("^S[Ff]\\[([A-Za-z0-9\\.\\/_]+)\\]");

		public static readonly Regex RegEvalSfEvt = new Regex("^S[Ff]E[Vv][Tt]\\[([A-Za-z0-9\\.\\/_]+)\\]");

		private PostEffectItem PeLowerBgm;

		private BDic<POSTM, PostEffectItem> OPEevent;

		private NelM2DBase.MENU_OPEN menu_open_;

		private bool areatitle_hide_progress;

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
