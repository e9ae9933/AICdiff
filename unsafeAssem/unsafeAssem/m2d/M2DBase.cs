using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Better;
using evt;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2DBase : IEventListener, IEventWaitListener, IM2DebugTarget, IHkdsFollowableContainer, IEfPInteractale
	{
		public M2ImageAtlas.AtlasRect RectWhite
		{
			get
			{
				return this.IMGS.Atlas.RectWhite;
			}
		}

		public float t_lock_check_push_up
		{
			get
			{
				return this.t_lock_check_push_up_;
			}
			set
			{
				this.t_lock_check_push_up_ = X.Mx(this.t_lock_check_push_up_, value);
			}
		}

		public M2DBase(M2ImageContainer _IMGS = null, bool it_is_basic_M2D = true)
		{
			this.IMGS = _IMGS;
			Map2d.TSbase = (Map2d.TSbasepr = 1f);
			M2PxlPointContainer.clear();
			M2Mover.clearIndexCount();
			this.Pauser = new PAUSER();
			this.SorterChip = new SORT<M2Puts>(null);
			if (this.IMGS == null)
			{
				this.IMGS = new M2ImageContainer(this);
			}
			this.DGN = new DungeonContainer(this);
			this.OPxlLayer = new BDic<string, PxlLayer>();
			this.OM2d = new NDic<Map2d>("M2DBase", 0);
			this.Snd = new M2SoundPlayer(this);
			this.MDMGCon = new M2MapDamageContainer();
			M2MoveTicket.initS();
			this.FlgWarpEventNotInjectable = new Flagger(delegate(FlaggerT<string> V)
			{
				this.FlaggerWarpEventNotInjectable(true);
			}, delegate(FlaggerT<string> V)
			{
				this.FlaggerWarpEventNotInjectable(false);
			});
			this.Amaterial_loaded = new List<M2DBase.LMtr>();
			this.AValotAddition = new List<IValotileSetable>(4);
			if (M2DBase.MtiShader == null)
			{
				M2DBase.MtiShader = new MTI("MTI_ShaderM2D", null);
			}
			this.Hash = new HashP(16);
			this.FlgRenderAfter = new Flagger(null, null);
			this.FlagValotStabilize = new Flagger(new FlaggerT<string>.FnFlaggerCall(this.FlaggerValotEnabled), delegate(FlaggerT<string> V)
			{
				this.next_valotile_activate = true;
			});
			this.FlgHideWholeScreen = new Flagger(delegate(FlaggerT<string> V)
			{
				this.FlaggerHideWholeScreen(true);
			}, delegate(FlaggerT<string> V)
			{
				this.FlaggerHideWholeScreen(false);
			});
			IN.getGUICamera();
			this.FD_WindowSizeChanged = new IN.FnWindowSizeChanged(this.fineWindowSizeChanged);
			IN.FD_WindowSizeChanged = (IN.FnWindowSizeChanged)Delegate.Combine(IN.FD_WindowSizeChanged, this.FD_WindowSizeChanged);
			if (it_is_basic_M2D)
			{
				M2DBase.Instance = this;
			}
		}

		public virtual void initGameObject(GameObject _GobBase, float cam_w, float cam_h, bool execute_load_finalize = true)
		{
			this.GobBase = _GobBase;
			M2DropObjectReader.initDroScript();
			this.Cam = new M2Camera(this);
			this.Cam.setWH(cam_w, cam_h, false, false);
			if (execute_load_finalize)
			{
				this.LoadAfterWhenMapScene();
			}
		}

		protected virtual void destructMaps()
		{
			if (this.curMap != null)
			{
				try
				{
					this.curMap.close(true, true);
				}
				catch (Exception)
				{
				}
			}
			foreach (KeyValuePair<string, Map2d> keyValuePair in this.OM2d)
			{
				try
				{
					if (!keyValuePair.Value.is_whole)
					{
						keyValuePair.Value.close(true, true);
					}
				}
				catch (Exception)
				{
				}
			}
			try
			{
				this.changeMap(null);
			}
			catch (Exception)
			{
			}
		}

		public virtual void destruct(bool execute_destruct_maps = true)
		{
			Map2d.TSbase = (Map2d.TSbasepr = 1f);
			try
			{
				if (this.EvLsn != null)
				{
					this.EvLsn.destruct();
					this.EvLsn = null;
				}
			}
			catch
			{
			}
			TX.removeListenerEval(this);
			EV.remListener(this);
			EV.remWaitListener("M2D_LOADING");
			if (execute_destruct_maps)
			{
				this.destructMaps();
			}
			BGM.remHalfFlag("DGN");
			M2DBase.material_flush_flag = M2DBase.FLUSH._ALL;
			SND.flush();
			if (!X.DEBUGNOSND)
			{
				BGM.stop(false, true);
			}
			try
			{
				this.Acurrent_async_loading = null;
				this.checkFlush(true);
				this.closeCamera();
				this.Cam.clearCameraCompoent();
				this.Cam.destruct();
			}
			catch
			{
			}
			try
			{
				M2PxlPointContainer.clear();
				this.Snd.destruct();
				this.Snd = null;
			}
			catch
			{
			}
			try
			{
				M2MobGenerator.destructMOBG();
			}
			catch
			{
			}
			IN.FD_WindowSizeChanged = (IN.FnWindowSizeChanged)Delegate.Remove(IN.FD_WindowSizeChanged, this.FD_WindowSizeChanged);
			this.IMGS.destruct();
			this.DGN.destruct();
			if (M2DBase.Instance == this)
			{
				M2DBase.Instance = null;
			}
		}

		public bool isDestructed()
		{
			return this.Snd == null;
		}

		public bool loadBasicMaterialProgress(int cnt = -1)
		{
			while (cnt < 0 || --cnt >= 0)
			{
				if (this.pstate == M2DBase.PREPARE.NO_LOAD)
				{
					string text = NKT.readStreamingText("m2d/__m2d_list.dat", false);
					this.LoadMapList(text);
					M2DBase.MtiShader.addLoadKey("M2D", true);
					cnt = X.Mn(cnt, 0);
					this.pstate = M2DBase.PREPARE.PREPARE_CHIPS;
					Bench.mark("loadM2D - LoadChipList", false, false);
				}
				else if (this.pstate == M2DBase.PREPARE.PREPARE_CHIPS)
				{
					PxlsLoader.loadSpeed = X.Mn(64f, PxlsLoader.loadSpeed);
					this.IMGS.Atlas.initCspAtlas(M2DBase.Achip_pxl_key[0]);
					if (this.fnYSortFunction == null)
					{
						this.fnYSortFunction = new fnMapItemYSortFunction(this.fnYSortFunctionDefault);
					}
					IN.AssignPauseable(this.Pauser);
					this.IMGS.prepareChipsScript();
					this.pstate = M2DBase.PREPARE.PREPARE_CHIPS_READING;
				}
				else if (this.pstate == M2DBase.PREPARE.PREPARE_CHIPS_READING)
				{
					if (!this.IMGS.progressChipsScriptRead((cnt < 0) ? (-1) : (cnt + 1)))
					{
						this.pstate = M2DBase.PREPARE.PREPARE_CHIPS_COMP;
					}
					else
					{
						cnt = X.Mn(cnt, 0);
					}
				}
				else if (this.pstate == M2DBase.PREPARE.PREPARE_CHIPS_COMP)
				{
					this.pstate = M2DBase.PREPARE.LOAD_EV;
					Bench.mark("loadM2D - LoadMapList", false, false);
				}
				else if (this.pstate == M2DBase.PREPARE.LOAD_EV)
				{
					if (!EV.material_prepared || !PxlsLoader.isLoadCompletedAll() || !M2DBase.MtiShader.isAsyncLoadFinished())
					{
						return true;
					}
					M2DBase.MtiShader.LoadAllShader();
					this.pstate = M2DBase.PREPARE.LOAD_AFTER;
				}
				else
				{
					if (this.pstate == M2DBase.PREPARE.LOAD_AFTER)
					{
						Bench.mark("loadM2D - LoadAfter", false, false);
						this.LoadAfter();
						Bench.mark(null, false, false);
						this.pstate = M2DBase.PREPARE.INIT_FIRST_EVENT;
						this.transferring_game_stopping = true;
						break;
					}
					break;
				}
			}
			if (this.pstate == M2DBase.PREPARE.INIT_FIRST_EVENT && this.GobBase != null)
			{
				this.initEvent();
				if (EV.Log != null)
				{
					EV.Log.initPerson();
				}
				if (!this.event_eval_inited)
				{
					this.event_eval_inited = true;
					this.createListenerEval(0);
					EV.addListener(this);
					EV.addHkdsFollowableListener(this);
				}
				this.pstate = M2DBase.PREPARE.COMPLETE;
			}
			return this.pstate < M2DBase.PREPARE.INIT_FIRST_EVENT;
		}

		protected virtual void initEvent()
		{
			EV.initEvent(null, null, this.TutoBox = new GameObject("Ev-Tutorial").AddComponent<TutorialBox>());
		}

		public virtual void stackFirstEvent()
		{
			if (this.EvLsn == null && EV.initDebugger(false))
			{
				this.EvLsn = new M2EvDebugListener(this);
			}
			if (this.EvLsn != null)
			{
				this.EvLsn.ListenerInitialize();
			}
			EV.flushExternal();
			EV.addWaitListener("MAP_TRANSFER", this);
			EV.stackFirstEvent();
			EV.stack("__INITM2D", 0, -1, null, null);
			PxlsLoader.loadSpeed = 1f;
		}

		public virtual int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		private void LoadMapList(string lt_text)
		{
			CsvReader csvReader = new CsvReader(lt_text, CsvReader.RegSpace, true);
			while (csvReader.read())
			{
				string text = X.basename_noext(csvReader.cmd);
				if (!(text == ""))
				{
					this.OM2d[text] = new Map2d(this, text, false);
				}
			}
			this.OM2d.scriptFinalize();
		}

		public CsvReader readMapBody(Map2d M)
		{
			byte[] array = NKT.readSpecificFileBinary(Path.Combine(Application.streamingAssetsPath, "m2d/" + M.key + ".tmap"), 0, 0, false);
			if (array != null)
			{
				M.prepareBodyByteArray(array);
				return null;
			}
			string text = NKT.readStreamingText("m2d/" + M.key + ".csv", false);
			return M.prepareBodyCsvReader(text);
		}

		public void readMapEventContent(Map2d M, bool reloading = false)
		{
			string text = M2DBase.readMapEventContent(M.key);
			if (reloading)
			{
				M.reloadCommand(text);
				return;
			}
			M.prepareCommand(text);
		}

		public static string readMapEventContent(string key)
		{
			return NKT.readStreamingText("m2d/" + key + ".cmd", true);
		}

		protected virtual void LoadAfter()
		{
		}

		protected virtual void LoadAfterWhenMapScene()
		{
			this.loadBasicMaterialProgress(0);
		}

		public virtual bool loadAdditionalMaterialForChip(M2ChipImage I)
		{
			METACImg meta = I.Meta;
			if (meta == null)
			{
				return false;
			}
			I.loaded_additional_material = true;
			string[] array = meta.Get("_load_pxl");
			bool flag = false;
			if (array != null)
			{
				if (array.Length < 2)
				{
					X.de("メタ指定エラー: _load_pxl <title> <Path> が必要", null);
				}
				else
				{
					this.loadMaterialPxl(array[0], array[1], true, true);
					flag = true;
				}
			}
			array = meta.Get("_load_snd");
			if (array != null)
			{
				this.loadMaterialSnd(array);
				flag = true;
			}
			return flag;
		}

		public virtual void loadMaterialForLabelPoint(Map2d Mp, Map2d.LpLoader _Loader)
		{
		}

		protected bool hasLMtr(M2DBase.LM_TYPE type, string key)
		{
			return M2DBase.hasLMtr(type, key, this.Amaterial_loaded);
		}

		protected static bool hasLMtr(M2DBase.LM_TYPE type, string key, List<M2DBase.LMtr> A)
		{
			for (int i = A.Count - 1; i >= 0; i--)
			{
				M2DBase.LMtr lmtr = A[i];
				if (lmtr.type == type && lmtr.key == key)
				{
					return true;
				}
			}
			return false;
		}

		public void addLMtrEntry(M2DBase.LM_TYPE type, string key)
		{
			if (!this.hasLMtr(type, key))
			{
				this.Amaterial_loaded.Add(new M2DBase.LMtr(type, key));
			}
		}

		public void removeLMtrEntryManual(M2DBase.LM_TYPE type, string key)
		{
			for (int i = this.Amaterial_loaded.Count - 1; i >= 0; i--)
			{
				M2DBase.LMtr lmtr = this.Amaterial_loaded[i];
				if (lmtr.type == type && lmtr.key == key)
				{
					this.Amaterial_loaded.RemoveAt(i);
				}
			}
		}

		public bool loadMaterialPxl(string title, string text_asset_path, bool autoFlipX = true, bool load_external = true)
		{
			PxlCharacter pxlCharacter;
			return this.loadMaterialPxl(title, text_asset_path, out pxlCharacter, autoFlipX, load_external);
		}

		public bool loadMaterialPxl(string title, string text_asset_path, out PxlCharacter PcOut, bool autoFlipX = true, bool load_external = true)
		{
			bool flag = false;
			if (!this.hasLMtr(M2DBase.LM_TYPE.PXL, title))
			{
				if (text_asset_path == "")
				{
					text_asset_path = title;
				}
				this.Amaterial_loaded.Add(new M2DBase.LMtr(M2DBase.LM_TYPE.PXL, title));
				PcOut = MTRX.loadMtiPxc(title, text_asset_path, "M2D", autoFlipX, load_external);
				flag = true;
			}
			else
			{
				PcOut = PxlsLoader.getPxlCharacter(title);
			}
			if (this.Acurrent_async_loading != null)
			{
				this.Acurrent_async_loading.Add(new M2DBase.LMtr(M2DBase.LM_TYPE.PXL, title));
			}
			return flag;
		}

		public bool loadMaterialSnd(string title)
		{
			bool flag = false;
			if (!this.hasLMtr(M2DBase.LM_TYPE.SND, title))
			{
				this.Amaterial_loaded.Add(new M2DBase.LMtr(M2DBase.LM_TYPE.SND, title));
				SND.loadSheets(title, "m2d");
				flag = true;
			}
			if (this.Acurrent_async_loading != null)
			{
				this.Acurrent_async_loading.Add(new M2DBase.LMtr(M2DBase.LM_TYPE.SND, title));
			}
			return flag;
		}

		public bool loadMaterialSnd(string[] Atitle)
		{
			if (Atitle == null)
			{
				return false;
			}
			int num = Atitle.Length;
			for (int i = 0; i < num; i++)
			{
				this.loadMaterialSnd(Atitle[i]);
			}
			return false;
		}

		public string loadMaterialTxt(string title)
		{
			string text = null;
			if (!this.hasLMtr(M2DBase.LM_TYPE.TXT, title))
			{
				this.Amaterial_loaded.Add(new M2DBase.LMtr(M2DBase.LM_TYPE.SND, title));
				text = TX.getResource(title, ".csv", false);
			}
			if (this.Acurrent_async_loading != null)
			{
				this.Acurrent_async_loading.Add(new M2DBase.LMtr(M2DBase.LM_TYPE.TXT, title));
			}
			return text;
		}

		public bool loadMaterialMTIOneImage(string path, string image_path = null)
		{
			bool flag = false;
			if (!this.hasLMtr(M2DBase.LM_TYPE.MTI, path))
			{
				this.Amaterial_loaded.Add(new M2DBase.LMtr(M2DBase.LM_TYPE.MTI, path));
				MTI.LoadContainerOneImage(path, "M2D", image_path);
				flag = true;
			}
			return flag;
		}

		public bool loadMaterialMTISpine(string path, string atlas_key)
		{
			bool flag = false;
			if (!this.hasLMtr(M2DBase.LM_TYPE.MTI, path))
			{
				this.Amaterial_loaded.Add(new M2DBase.LMtr(M2DBase.LM_TYPE.MTI, path));
				MTI.LoadContainerSpine(path, atlas_key, "M2D");
				flag = true;
			}
			return flag;
		}

		public virtual void setFlushAllFlag(bool only_game_reload)
		{
			M2DBase.material_flush_flag |= M2DBase.FLUSH._ALL;
		}

		public virtual void setFlushOtherMatFlag()
		{
			M2DBase.material_flush_flag |= M2DBase.FLUSH.MATERIAL;
		}

		public void setGfTempFlushFlagOnMapChange(bool f)
		{
			if (f)
			{
				M2DBase.material_flush_flag |= M2DBase.FLUSH.GF_TEMP_CLEAR;
				return;
			}
			M2DBase.material_flush_flag &= ~M2DBase.FLUSH.GF_TEMP_CLEAR;
		}

		protected virtual void checkFlush(bool can_check_map = true)
		{
			if (M2DBase.material_flush_flag == (M2DBase.FLUSH)0)
			{
				return;
			}
			if ((M2DBase.material_flush_flag & M2DBase.FLUSH.MATERIAL) != (M2DBase.FLUSH)0)
			{
				int count = this.Amaterial_loaded.Count;
				bool flag = false;
				using (BList<M2DBase.LMtr> blist = ListBuffer<M2DBase.LMtr>.Pop((this.Acurrent_async_loading != null) ? this.Acurrent_async_loading.Count : 2))
				{
					for (int i = 0; i < count; i++)
					{
						M2DBase.LMtr lmtr = this.Amaterial_loaded[i];
						if (this.Acurrent_async_loading != null && M2DBase.hasLMtr(lmtr.type, lmtr.key, this.Acurrent_async_loading))
						{
							blist.Add(lmtr);
						}
						else if (lmtr.type == M2DBase.LM_TYPE.PXL)
						{
							PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter(lmtr.key);
							if (pxlCharacter != null)
							{
								M2PxlPointContainer.clear(pxlCharacter);
								pxlCharacter.releaseLoadedExternalTexture(false, null);
								MTI.ReleaseContainer(pxlCharacter.external_png_header + ".bytes.texture_0", "M2D");
								MTRX.releaseMI(pxlCharacter, true);
							}
						}
						else if (lmtr.type == M2DBase.LM_TYPE.SND)
						{
							flag = true;
							SND.unloadSheets(lmtr.key, "m2d");
						}
						else if (lmtr.type == M2DBase.LM_TYPE.MTI)
						{
							MTI.ReleaseContainer(lmtr.key, "M2D");
						}
					}
					this.Amaterial_loaded.Clear();
					this.Amaterial_loaded.AddRange(blist);
				}
				this.flushOtherMaterial();
				this.flushCachedPxlMaterial();
				this.IMGS.Atlas.flushPxl();
				M2DBase.material_flush_flag &= ~M2DBase.FLUSH.MATERIAL;
				M2DBase.material_flush_flag |= M2DBase.FLUSH.RELOAD_AREA_MATERIAL;
				if (flag)
				{
					SND.flush();
				}
			}
			if (can_check_map && (M2DBase.material_flush_flag & M2DBase.FLUSH.MAP) != (M2DBase.FLUSH)0 && this.curMap == null)
			{
				this.flushMaps(MAPMODE.NORMAL);
			}
		}

		public void closeAllSubMapCaches(bool check_other_submap_opening = true)
		{
			if (check_other_submap_opening)
			{
				foreach (KeyValuePair<string, Map2d> keyValuePair in this.OM2d)
				{
					if (!keyValuePair.Value.is_whole && (keyValuePair.Value.mode == MAPMODE.NORMAL || keyValuePair.Value.mode == MAPMODE.TEMP))
					{
						keyValuePair.Value.closeSubMaps(false);
					}
				}
			}
			foreach (KeyValuePair<string, Map2d> keyValuePair2 in this.OM2d)
			{
				if (!keyValuePair2.Value.is_whole && keyValuePair2.Value.is_submap)
				{
					keyValuePair2.Value.close(false, true);
				}
			}
		}

		public virtual void flushMaps(MAPMODE nextmap_open_mode = MAPMODE.NORMAL)
		{
			M2DBase.material_flush_flag &= ~M2DBase.FLUSH.MAP;
			if (this.curMap != null)
			{
				if (!this.curMap.is_whole)
				{
					if (this.AEvacuatedMover == null)
					{
						this.EvacuateCoreMover(null);
					}
					this.curMap.close(false, true);
				}
				this.curMap = null;
			}
			foreach (KeyValuePair<string, Map2d> keyValuePair in this.OM2d)
			{
				if (!keyValuePair.Value.is_whole && !keyValuePair.Value.is_submap && keyValuePair.Value.has_loaded_layer)
				{
					if (this.ALoader != null && nextmap_open_mode != MAPMODE.CLOSED)
					{
						bool flag = false;
						for (int i = this.ALoader.Count - 1; i >= 0; i--)
						{
							if (this.ALoader[i].Mp == keyValuePair.Value)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							continue;
						}
					}
					keyValuePair.Value.close(false, true);
				}
			}
			this.closeAllSubMapCaches(false);
			if (this.Cam.CurDgn != null && this.Cam.CurDgn.key != "_editor" && this.curMap == null)
			{
				this.Cam.CurDgn.closeCamera();
				this.Cam.CurDgn = null;
				this.DGN.recreateMaterial();
			}
			EV.flushExternal();
			X.dl("全てのマップを再構築", null, false, true);
		}

		private void flushOtherMaterial()
		{
			this.IMGS.flushAdditinal();
		}

		protected virtual void flushCachedPxlMaterial()
		{
		}

		public bool isMaterialLoading(bool no_consider_loader = false)
		{
			return (!no_consider_loader && (this.ALoader != null || this.IMGS.Atlas.isLoading())) || !SND.loaded;
		}

		public bool isLoaderLoading()
		{
			return this.ALoader != null;
		}

		public Map2d getLoaderTargetMap()
		{
			if (this.ALoader == null || this.ALoader.Count <= 0)
			{
				return null;
			}
			return this.ALoader[0].Mp;
		}

		public virtual void initMapMaterialASync(Map2d _Mp, int transfering, bool manual_mode = false)
		{
			if (this.ALoader == null)
			{
				this.ALoader = new List<M2MapMaterialLoader>(1);
			}
			if (transfering > 0 || (this.curMap == null && !_Mp.is_whole))
			{
				if (this.curMap == null)
				{
					this.transferring_game_stopping = true;
				}
				transfering = X.Mx(transfering, 1);
				if (this.Acurrent_async_loading == null)
				{
					this.Acurrent_async_loading = new List<M2DBase.LMtr>();
				}
				this.IMGS.Atlas.initAsyncLoad();
				M2DBase.material_flush_flag |= M2DBase.FLUSH.GF_TEMP_CLEAR;
			}
			if (manual_mode)
			{
				this.transfer_mode |= M2DBase.MAP_TRANSFER.MANUAL;
			}
			for (int i = this.ALoader.Count - 1; i >= 0; i--)
			{
				M2MapMaterialLoader m2MapMaterialLoader = this.ALoader[i];
				if (m2MapMaterialLoader.Mp == _Mp)
				{
					if (transfering > 0)
					{
						m2MapMaterialLoader.next_map = true;
					}
					if (transfering > 1 || (M2DBase.material_flush_flag & M2DBase.FLUSH.MAP) != (M2DBase.FLUSH)0)
					{
						m2MapMaterialLoader.flushing = true;
					}
					return;
				}
			}
			M2MapMaterialLoader m2MapMaterialLoader2 = new M2MapMaterialLoader(_Mp, true)
			{
				next_map = (transfering > 0),
				flushing = (transfering > 1 || (M2DBase.material_flush_flag & M2DBase.FLUSH.MAP) > (M2DBase.FLUSH)0)
			};
			this.ALoader.Add(m2MapMaterialLoader2);
		}

		protected virtual bool initMapBgm(Map2d NextMap = null, bool replace_bgm = false)
		{
			if (NextMap == null)
			{
				NextMap = this.curMap;
			}
			if (NextMap == null)
			{
				return false;
			}
			Dungeon dgn = this.DGN.getDgn(NextMap);
			return this.initMapBgm(dgn, replace_bgm);
		}

		protected bool initMapBgm(IBgmManageArea Mng, bool replace_bgm = false)
		{
			string text = "";
			string text2 = null;
			if (Mng.getCueAndSheet(ref text, ref text2))
			{
				if (TX.valid(text))
				{
					this.bgm_replaced = (BGM.load(text2, text, true) ? 128 : 0) | this.bgm_replaced;
					if ((this.bgm_replaced & 128) != 0)
					{
						if (replace_bgm)
						{
							BGM.replace(30f, -1f, true, true);
							this.bgm_replaced = BGM.bgm_replace_id | 64;
						}
						else
						{
							BGM.fadeout(0f, 140f, true);
						}
					}
					else if (replace_bgm && BGM.frontBGMIs(text2, text))
					{
						BGM.fadein(100f, 90f);
					}
				}
				else if (replace_bgm)
				{
					BGM.fadeout(0f, 30f, true);
					this.bgm_replaced = 0;
				}
				else
				{
					BGM.replace(30f, 0f, false, true);
				}
				return true;
			}
			return false;
		}

		public virtual void reloadTexture(Texture PreTx, Texture nTx)
		{
			this.DGN.reloadTexture(nTx);
			bool flag = false;
			if (this.curMap != null && !flag)
			{
				this.curMap.reloadWholePxls(true);
			}
		}

		public virtual void prepareImageAtlasFinalize()
		{
		}

		public void stockSubMap(M2SubMap Sm)
		{
			Map2d targetMap = Sm.getTargetMap();
			if (targetMap.gameObject == null)
			{
				return;
			}
			targetMap.gameObject.SetActive(false);
			targetMap.gameObject.transform.SetParent(this.GobBase.transform, false);
		}

		public bool popStockedForSubMap(M2SubMap Sm)
		{
			Map2d targetMap = Sm.getTargetMap();
			if (targetMap.mode == MAPMODE.SUBMAP && targetMap.gameObject != null && targetMap.gameObject.transform.parent == this.GobBase.transform)
			{
				targetMap.gameObject.transform.SetParent(Sm.getBaseMap().gameObject.transform, false);
				targetMap.gameObject.SetActive(true);
				if (Sm.needReentry(targetMap.SubMapData))
				{
					targetMap.need_reentry_flag = true;
				}
				targetMap.SubMapData = Sm;
				targetMap.initEffect(Sm);
				return true;
			}
			return false;
		}

		public virtual void releaseUnstabilizeDrawer(Map2d Mp)
		{
		}

		public M2PxlAnimator createBasicPxlAnimator(M2Mover Mov, string chara_key, string pose_key = "stand", bool auto_start = false)
		{
			return this.curMap.createPxlAnimator<M2PxlAnimator>(Mov, chara_key, pose_key, auto_start);
		}

		public M2PxlAnimatorRT createBasicPxlAnimatorForRenderTicket(M2Mover Mov, string chara_key, string pose_key = "stand", bool auto_start = false, M2Mover.DRAW_ORDER order = M2Mover.DRAW_ORDER.PR0)
		{
			return this.curMap.createPxlAnimator<M2PxlAnimatorRT>(Mov, chara_key, pose_key, auto_start).initRenderTicket(order);
		}

		public virtual void mapClosed(Map2d Mp, ref IEffectSetter EF, ref IEffectSetter EFT, ref IEffectSetter EFC)
		{
		}

		public virtual void mapActionInitted(Map2d Mp)
		{
		}

		public virtual void mapActionClosed(Map2d Mp)
		{
		}

		public virtual void FlaggerValotEnabled(FlaggerT<string> F)
		{
			this.next_valotile_activate = false;
			EV.use_valotile = false;
			for (int i = this.AValotAddition.Count - 1; i >= 0; i--)
			{
				this.AValotAddition[i].use_valotile = false;
			}
		}

		public virtual void FlaggerValotDisabled(FlaggerT<string> F)
		{
			this.next_valotile_activate = false;
			EV.use_valotile = !X.DEBUGSTABILIZE_DRAW;
			for (int i = this.AValotAddition.Count - 1; i >= 0; i--)
			{
				this.AValotAddition[i].use_valotile = !X.DEBUGSTABILIZE_DRAW;
			}
		}

		protected virtual void FlaggerHideWholeScreen(bool _flag)
		{
			this.Cam.finalize_screen_enabled = !_flag;
		}

		protected virtual void FlaggerWarpEventNotInjectable(bool _flag)
		{
		}

		public void addValotAddition(IValotileSetable Vlt)
		{
			if (X.DEBUGSTABILIZE_DRAW)
			{
				return;
			}
			this.AValotAddition.Add(Vlt);
			Vlt.use_valotile = true;
			if (this.FlagValotStabilize.isActive())
			{
				Vlt.use_valotile = false;
			}
		}

		public void remValotAddition(IValotileSetable Vlt)
		{
			this.AValotAddition.Remove(Vlt);
		}

		public virtual string checkWaterFootSound(M2Mover Mv, float x, float y, ref string snd_type)
		{
			return snd_type;
		}

		public virtual void configReconsidered(M2Pt[,] AAPt, int _l, int _t, int _r, int _b)
		{
		}

		public virtual void initEffect(Map2d Mp, ref IEffectSetter EF, ref IEffectSetter EFT)
		{
		}

		public virtual void initChipEffect(Map2d Mp, M2SubMap SM, ref IEffectSetter EFC, ref M2DrawBinderContainer _EFDC)
		{
		}

		public virtual Map2d changeMap(Map2d newM)
		{
			Map2d map2d = this.curMap;
			if (this.curMap != null)
			{
				if (this.auto_evacuate)
				{
					this.AEvacuatedMover = this.EvacuateCoreMover(null);
				}
				this.curMap.close();
				if (this.TutoBox != null)
				{
					this.TutoBox.RemText(true, true);
				}
				this.curMap = null;
			}
			this.Snd.clear();
			if (newM == null)
			{
				this.Omazinai();
				if (map2d != null)
				{
					X.dli("map_close:: " + map2d.key, null);
				}
				Resources.UnloadUnusedAssets();
				M2Mover.clearIndexCount();
				GC.Collect();
				M2MobGenerator.ClearTextureS();
				this.pre_drawn_totalframe = 0;
				this.ev_mobtype = "";
				return null;
			}
			if (newM.opened)
			{
				newM.close();
			}
			this.t_lock_check_push_up_ = 0f;
			this.FlgWarpEventNotInjectable.Clear();
			bool flag = (M2DBase.material_flush_flag & M2DBase.FLUSH.MAP) > (M2DBase.FLUSH)0;
			this.checkFlush(true);
			this.curMap = newM;
			this.MDMGCon.clear();
			M2MoveTicket.initS();
			X.dli("map_open:: " + newM.key + (newM.binary_loading_mode ? "" : "(CR)"), null);
			newM.open(this.GobBase, MAPMODE.NORMAL, null);
			this.Acurrent_async_loading = null;
			this.IMGS.Atlas.quitAsyncLoad();
			this.Cam.MovRender.initS(newM);
			string[] array = newM.Meta.Get("mobtype");
			if (array != null && array.Length != 0)
			{
				this.ev_mobtype = array[0];
			}
			this.pre_map_active = false;
			if (flag)
			{
				this.Omazinai();
			}
			if (this.AEvacuatedMover != null && this.auto_evacuate)
			{
				this.EvacuateCoreMover(this.AEvacuatedMover);
				this.AEvacuatedMover = null;
			}
			if (this.EvLsn != null)
			{
				this.EvLsn.initS(this.curMap);
			}
			if (M2MobGenerator.Instance != null)
			{
				M2MobGenerator.Instance.initS(this.curMap);
			}
			this.initMapBgm(newM, true);
			return newM;
		}

		public void Omazinai()
		{
			CURS.Omazinai();
		}

		public virtual void closeCamera()
		{
			this.Cam.init(null);
			BGM.remHalfFlag("DGN");
			if (this.Cam.CurDgn != null)
			{
				this.Cam.CurDgn.closeCamera();
				this.Cam.CurDgn = null;
			}
		}

		public virtual void initCameraAfter(Map2d M)
		{
			this.fineDgnHalfBgm(true);
			if (!Map2d.editor_decline_lighting)
			{
				if (this.FlagValotStabilize.isActive())
				{
					this.FlaggerValotEnabled(this.FlagValotStabilize);
					return;
				}
				this.FlaggerValotDisabled(this.FlagValotStabilize);
			}
		}

		protected virtual void fineDgnHalfBgm(bool flag = true)
		{
			bool flag2 = false;
			if (this.curMap != null && this.curMap.Dgn != null)
			{
				flag2 = flag && this.curMap.Dgn.isHalfBgm(this.curMap);
			}
			if (flag2)
			{
				BGM.addHalfFlag("DGN");
				return;
			}
			BGM.remHalfFlag("DGN");
		}

		public virtual void cameraFinedImmediately()
		{
		}

		public void prepareAll(bool only_modif_date)
		{
			if (!only_modif_date)
			{
				foreach (KeyValuePair<string, Map2d> keyValuePair in this.OM2d)
				{
					keyValuePair.Value.prepared = true;
				}
			}
		}

		public Map2d Get(string key, bool no_error = false)
		{
			if (key == null)
			{
				return this.curMap;
			}
			Map2d map2d = X.Get<string, Map2d>(this.OM2d, key);
			if (map2d != null)
			{
				return map2d;
			}
			if (!no_error)
			{
				X.de("Map2d " + key + "が見つかりません。 ", null);
			}
			return null;
		}

		public virtual string getMapTitle(Map2d Mp = null)
		{
			Mp = Mp ?? this.curMap;
			if (Mp == null)
			{
				return null;
			}
			return TX.Get("MAP_" + Mp.key, "");
		}

		public virtual Dungeon getDgn(Map2d Mp)
		{
			return this.DGN.getDgn(Mp);
		}

		public Dungeon getDgnByKey(string key)
		{
			return this.DGN.getDgn(key);
		}

		public virtual bool isWholeMap(Map2d Mp)
		{
			return this.WholeMap_ == Mp;
		}

		public virtual Map2d getWholeFor(Map2d Mp, bool alloc_empty = false)
		{
			return this.WholeMap_;
		}

		public void reload(string key = "")
		{
			if (!(key == ""))
			{
				Map2d map2d = this.curMap;
			}
			else
			{
				this.Get(key, false);
			}
		}

		private bool fnYSortFunctionDefault(M2DrawItem Im, M2DrawItem Mv)
		{
			return Im.y > Mv.y;
		}

		public Vector2 getMousePosToMapPos()
		{
			Vector3 vector = this.GobBase.transform.InverseTransformPoint(IN.getMousePos(this.Cam.get_FinalCamera()));
			Vector3 vector2 = vector * 64f;
			vector2.x = X.MMX(0f, this.curMap.uxToMapx(vector.x), (float)this.curMap.clms - 0.02f);
			vector2.y = X.MMX(0f, this.curMap.uyToMapy(vector.y), (float)this.curMap.rows - 0.02f);
			return vector2;
		}

		public virtual bool isCenterPlayer(M2Mover Mv)
		{
			return this.Cam.isBaseMover(Mv);
		}

		public float screenx2ux(float x)
		{
			return (x - IN.wh) * 0.015625f / this.Cam.base_scale + this.Cam.PosMainTransform.x;
		}

		public float screeny2uy(float y)
		{
			return (y - IN.hh) * 0.015625f / this.Cam.base_scale + this.Cam.PosMainTransform.y;
		}

		public virtual float ux2effectScreenx(float x)
		{
			return x * this.Cam.base_scale;
		}

		public virtual float uy2effectScreeny(float y)
		{
			return y * this.Cam.base_scale;
		}

		public virtual float effectScreenx2ux(float x)
		{
			return x / this.Cam.base_scale;
		}

		public virtual float effectScreeny2uy(float y)
		{
			return y / this.Cam.base_scale;
		}

		public virtual float mapx2ui_layer_x(float x)
		{
			return this.ux2effectScreenx(this.curMap.map2ux(x)) - this.Cam.PosMainTransform.x + (-this.ui_shift_x * this.Cam.getScaleRev() + this.ui_shift_x) * 0.015625f;
		}

		public virtual float mapy2ui_layer_y(float y)
		{
			return this.uy2effectScreeny(this.curMap.map2uy(y)) - this.Cam.PosMainTransform.y;
		}

		public virtual float ui_shift_x
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
				this.ui_shift_x_ = value;
				this.Cam.fineBaseShiftPixel(-this.ui_shift_x_, 0f, false);
			}
		}

		public bool use_valotile
		{
			get
			{
				return !X.DEBUGSTABILIZE_DRAW && !this.FlagValotStabilize.isActive();
			}
		}

		public void AssignPauseable(MonoBehaviour Mb)
		{
			this.Pauser.Assign(Mb);
			if (this.stopping_game_)
			{
				Mb.enabled = false;
			}
		}

		public void AssignPauseable(Rigidbody2D R2D)
		{
			PauseMemItemRigidbody pauseMemItemRigidbody = this.Pauser.Assign(R2D);
			if (this.stopping_game_ && pauseMemItemRigidbody != null)
			{
				pauseMemItemRigidbody.Pause();
			}
		}

		public void AssignPauseable(PAUSER Ps)
		{
			if (Ps == null)
			{
				return;
			}
			this.Pauser.Assign(Ps);
			if (this.stopping_game_)
			{
				Ps.Pause();
			}
		}

		public void AssignPauseableP(IPauseable Ps)
		{
			if (Ps == null)
			{
				return;
			}
			this.Pauser.AddP(Ps);
			if (this.stopping_game_)
			{
				Ps.Pause();
			}
		}

		public void DeassignPauseable(object Ps)
		{
			this.Pauser.Deassign(Ps);
		}

		public void ClearPauseable()
		{
			this.Pauser.Clear();
		}

		public bool stopping_game
		{
			get
			{
				return this.stopping_game_;
			}
			set
			{
				if (this.stopping_game == value)
				{
					return;
				}
				this.stopping_game_ = value;
				if (value)
				{
					this.PauseMem(true);
					return;
				}
				this.ResumeMem(true);
			}
		}

		public void PauseMem(bool particle_setter_stop = true)
		{
			this.Pauser.Pause();
		}

		public void ResumeMem(bool particle_setter_resume = true)
		{
			this.Pauser.Resume();
		}

		public PAUSER getPauser()
		{
			return this.Pauser;
		}

		public BDic<string, Map2d> getMapObject()
		{
			return this.OM2d;
		}

		public virtual IEffectSetter getEffectTop()
		{
			return null;
		}

		public virtual bool run(float fcnt)
		{
			Bench.P("MapMain");
			this.pre_map_active = this.curMap.run(fcnt);
			Bench.Pend("MapMain");
			return this.pre_map_active;
		}

		public virtual bool runPost(float fcnt)
		{
			if (this.curMap == null)
			{
				return false;
			}
			if (this.valot_stabilize_ev_on_end)
			{
				if (EV.isActive(false))
				{
					this.valot_stabilize_ev_on_end = false;
				}
				else if (EV.isEverythingStasis())
				{
					this.valot_stabilize_ev_on_end = false;
					this.FlagValotStabilize.Rem("EV");
				}
			}
			Bench.P("P1");
			this.Snd.run();
			Bench.Pend("P1");
			Bench.P("P2");
			if ((this.bgm_replaced & 64) != 0 && this.curMap.load_cover_disable && !EV.isActive(false))
			{
				if ((this.bgm_replaced & 63) == (BGM.bgm_replace_id & 63))
				{
					BGM.fadein(100f, 0f);
				}
				this.bgm_replaced = 0;
			}
			Bench.Pend("P2");
			Bench.P("P3");
			this.curMap.runPost();
			Bench.Pend("P3");
			Bench.P("P4");
			if (X.D)
			{
				this.curMap.drawCheck((float)X.AF * Map2d.TSbase);
			}
			Bench.Pend("P4");
			Bench.P("P7");
			if (this.ALoader != null && (this.transfer_mode & M2DBase.MAP_TRANSFER.MANUAL) == M2DBase.MAP_TRANSFER.AUTO)
			{
				this.loadMapAsync();
			}
			if (this.t_lock_check_push_up_ > 0f && !IN.isCheckO(0))
			{
				this.t_lock_check_push_up_ = X.Mx(this.t_lock_check_push_up_ - fcnt, 0f);
			}
			Bench.Pend("P7");
			return true;
		}

		public virtual bool runui_enabled
		{
			get
			{
				return true;
			}
		}

		public virtual bool runPostForDraw(float fcnt, bool draw_flag = true, bool draw_finalaizecam_flag = true)
		{
			if (this.pre_drawn_totalframe == IN.totalframe)
			{
				return false;
			}
			this.pre_drawn_totalframe = IN.totalframe;
			if (Map2d.editor_decline_lighting)
			{
				draw_finalaizecam_flag = (draw_flag = true);
			}
			else if (EV.bg_full_filled)
			{
				draw_finalaizecam_flag = (draw_flag = false);
			}
			Bench.P("P CamRenderWholeCamera");
			if (this.next_valotile_activate)
			{
				this.FlaggerValotDisabled(null);
			}
			if (!EV.isStoppingGameDraw() && this.curMap != null)
			{
				M2MobGenerator.prepareAnimationWhole();
				this.Cam.RenderWholeCamera(draw_flag, draw_finalaizecam_flag);
			}
			Bench.Pend("P CamRenderWholeCamera");
			return true;
		}

		public virtual void runPhysics(float fcnt)
		{
			if (this.curMap == null)
			{
				return;
			}
			Map2d.TScur = fcnt;
			this.curMap.runPhysics(Map2d.TScur);
		}

		public bool loadMapAsync()
		{
			bool flag = false;
			if (this.ALoader == null)
			{
				return this.isMaterialLoading(true) || this.IMGS.Atlas.prepareAtlasProgress(350) || global::XX.Logger.scene_changing;
			}
			if ((M2DBase.material_flush_flag & M2DBase.FLUSH._LOCK_ASYNC_LOAD) == M2DBase.FLUSH._LOCK_ASYNC_LOAD && (M2DBase.material_flush_flag & M2DBase.FLUSH._LOCK_ASYNC_ALREADY) == (M2DBase.FLUSH)0)
			{
				if (!this.transferring_game_stopping)
				{
					return true;
				}
				if (this.curMap != null)
				{
					this.changeMap(null);
				}
				if (this.IMGS.Atlas.hasLoadingPxc())
				{
					return true;
				}
				this.checkFlush(false);
				if ((M2DBase.material_flush_flag & M2DBase.FLUSH.MAP) != (M2DBase.FLUSH)0)
				{
					this.flushMaps(MAPMODE.CLOSED);
				}
				M2DBase.material_flush_flag |= M2DBase.FLUSH._LOCK_ASYNC_ALREADY;
				return true;
			}
			else
			{
				Map2d map2d = null;
				bool flag2 = true;
				if (this.ALoader != null)
				{
					for (int i = this.ALoader.Count - 1; i >= 0; i--)
					{
						M2MapMaterialLoader m2MapMaterialLoader = this.ALoader[i];
						if (m2MapMaterialLoader.read((this.curMap == null) ? 1600 : 400))
						{
							flag2 = false;
							break;
						}
						if (m2MapMaterialLoader.next_map && map2d == null)
						{
							map2d = m2MapMaterialLoader.Mp;
						}
					}
				}
				if (!this.isMaterialLoading(flag2))
				{
					if (this.curMap != null && map2d != null)
					{
						if ((this.transfer_mode & M2DBase.MAP_TRANSFER.MANUAL) != M2DBase.MAP_TRANSFER.AUTO && (this.transfer_mode & M2DBase.MAP_TRANSFER.MAP_CLOSE_PREPARED) == M2DBase.MAP_TRANSFER.AUTO)
						{
							return true;
						}
						this.changeMap(null);
						flag = true;
					}
					return flag || EV.isLoading(false) || this.loadMapAsyncFinalize(map2d);
				}
				return true;
			}
		}

		protected virtual bool loadMapAsyncFinalize(Map2d Mp_Next)
		{
			if (this.curMap == null && (M2DBase.material_flush_flag & M2DBase.FLUSH._ALL) != (M2DBase.FLUSH)0)
			{
				this.transferring_game_stopping = true;
				if (this.IMGS.Atlas.hasLoadingPxc())
				{
					return true;
				}
				this.checkFlush(true);
			}
			bool flag;
			if (this.IMGS.Atlas.prepareAtlasProgress(out flag, 350) || flag)
			{
				this.transferring_game_stopping = true;
				return true;
			}
			this.ALoader = null;
			if (Mp_Next != null && Mp_Next != this.curMap)
			{
				M2DBase.material_flush_flag &= ~M2DBase.FLUSH._LOCK_ASYNC_ALREADY;
				this.changeMap(Mp_Next);
			}
			return global::XX.Logger.scene_changing;
		}

		public virtual bool transferring_game_stopping
		{
			get
			{
				return (this.transfer_mode & M2DBase.MAP_TRANSFER.GAME_STOP) == M2DBase.MAP_TRANSFER.GAME_STOP;
			}
			set
			{
				if (value)
				{
					this.transfer_mode |= M2DBase.MAP_TRANSFER.GAME_STOP;
					return;
				}
				if (this.transferring_game_stopping)
				{
					this.transfer_mode = M2DBase.MAP_TRANSFER.AUTO;
					if (!EV.isActive(false))
					{
						this.flushGF();
					}
				}
			}
		}

		public float effect_variable0
		{
			get
			{
				return this.effect_variable0_;
			}
			set
			{
				if (value == this.effect_variable0_)
				{
					return;
				}
				this.effect_variable0_ = value;
				this.fineMaterialColor(false);
			}
		}

		public virtual void fineMaterialColor(bool execute)
		{
			Map2d map2d = this.curMap;
		}

		public virtual void setCameraTransparentColor(Color32 C)
		{
			if (!this.isDestructed() && this.Cam != null)
			{
				this.Cam.transparent_color = C;
			}
		}

		public Material getWithLightTextureMaterial(MImage MI)
		{
			return this.curMap.Dgn.getWithLightTextureMaterial(MI, null, -1);
		}

		public virtual TxEvalListenerContainer createListenerEval(int cap_fn = 0)
		{
			TxEvalListenerContainer txEvalListenerContainer = TX.createListenerEval(this, cap_fn + 10, true);
			txEvalListenerContainer.Add("Exist_mover", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap != null)
				{
					TX.InputE((float)((this.curMap.getMoverByName(X.Get<string>(Aargs, 0), false) != null) ? 1 : 0));
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("map_x", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap != null)
				{
					Vector2 pos = this.curMap.getPos(X.Get<string>(Aargs, 0), 0f, 0f, null);
					TX.InputE((pos.x != -1000f) ? pos.x : 0f);
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("map_y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap != null)
				{
					Vector2 pos2 = this.curMap.getPos(X.Get<string>(Aargs, 0), 0f, 0f, null);
					TX.InputE((pos2.x != -1000f) ? pos2.y : 0f);
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("lp_foc_x", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap == null || Aargs.Count == 0)
				{
					return;
				}
				M2LabelPoint m2LabelPoint = null;
				if (Aargs.Count >= 2)
				{
					M2MapLayer layer = this.curMap.getLayer(Aargs[0]);
					if (layer == null)
					{
						X.de("レイヤーが不明:" + Aargs[0], null);
					}
					else
					{
						m2LabelPoint = layer.LP.Get(Aargs[1], true, false);
					}
				}
				else
				{
					m2LabelPoint = this.curMap.getLabelPoint(Aargs[0]);
				}
				TX.InputE((m2LabelPoint != null) ? m2LabelPoint.mapfocx : (M2DBase.Instance.Cam.x * this.curMap.rCLEN));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("lp_foc_y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap == null || Aargs.Count == 0)
				{
					return;
				}
				M2LabelPoint m2LabelPoint2 = null;
				if (Aargs.Count >= 2)
				{
					M2MapLayer layer2 = this.curMap.getLayer(Aargs[0]);
					if (layer2 == null)
					{
						X.de("レイヤーが不明:" + Aargs[0], null);
					}
					else
					{
						m2LabelPoint2 = layer2.LP.Get(Aargs[1], true, false);
					}
				}
				else
				{
					m2LabelPoint2 = this.curMap.getLabelPoint(Aargs[0]);
				}
				TX.InputE((m2LabelPoint2 != null) ? m2LabelPoint2.mapfocy : (M2DBase.Instance.Cam.y * this.curMap.rCLEN));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("cam_x", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(M2DBase.Instance.Cam.x * this.curMap.rCLEN);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("cam_y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(M2DBase.Instance.Cam.y * this.curMap.rCLEN);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_current_x", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE(M2EventCommand.EvMV.x);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_current_y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE(M2EventCommand.EvMV.y);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_current_sizex", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE(M2EventCommand.EvMV.sizex);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_current_sizey", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE(M2EventCommand.EvMV.sizey);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_has_foot", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE((float)(M2EventCommand.EvMV.hasFoot() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_walkable_mpf", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap == null)
				{
					return;
				}
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				M2Phys physic = M2EventCommand.EvMV.getPhysic();
				if (physic == null || !physic.hasFoot())
				{
					TX.InputE(0f);
					return;
				}
				M2Mover evMV = M2EventCommand.EvMV;
				M2BlockColliderContainer.BCCLine footBCC = physic.getFootManager().get_FootBCC();
				if (footBCC == null)
				{
					TX.InputE(0f);
					return;
				}
				float num = 1f;
				if (Aargs.Count > 0)
				{
					num = X.Nm(Aargs[0], num, false);
				}
				float num2;
				float num3;
				M2BlockColliderContainer.BCCLine bccline;
				M2BlockColliderContainer.BCCLine bccline2;
				footBCC.getLinearWalkableArea(0f, out num2, out num3, out bccline, out bccline2, num);
				float num4 = X.Abs(num2 - evMV.x);
				float num5 = X.Abs(num3 - evMV.x);
				if (num4 < num && num5 < num)
				{
					TX.InputE(0f);
					return;
				}
				if (num4 < num)
				{
					TX.InputE(1f);
					return;
				}
				if (num5 < num)
				{
					TX.InputE(-1f);
					return;
				}
				TX.InputE(evMV.mpf_is_right);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("CLEN", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(this.CLEN);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("warp_not_injectable", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)(this.FlgWarpEventNotInjectable.isActive() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("blood_restrict", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(1f - (float)M2DBase.blood_weaken / 100f);
			}, Array.Empty<string>());
			return txEvalListenerContainer;
		}

		public virtual bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 1286108617U)
				{
					if (num <= 735139189U)
					{
						if (num != 177837449U)
						{
							if (num != 226500968U)
							{
								if (num != 735139189U)
								{
									goto IL_0482;
								}
								if (!(cmd == "DEF_CURMAP"))
								{
									goto IL_0482;
								}
								if (TX.valid(rER._1) && this.curMap != null)
								{
									ER.VarCon.define(rER._1, this.curMap.key, true);
								}
								return true;
							}
							else
							{
								if (!(cmd == "STOP_DGN_HALF_BGM"))
								{
									goto IL_0482;
								}
								this.fineDgnHalfBgm(false);
								return true;
							}
						}
						else if (!(cmd == "DEFEAT_EVENT2"))
						{
							goto IL_0482;
						}
					}
					else if (num <= 1002657295U)
					{
						if (num != 997752879U)
						{
							if (num != 1002657295U)
							{
								goto IL_0482;
							}
							if (!(cmd == "#AUTO_BGM_REPLACE"))
							{
								goto IL_0482;
							}
							if (rER.Nm(1, 0f) != 0f)
							{
								this.bgm_replaced |= 64;
							}
							else
							{
								this.bgm_replaced = (byte)((int)this.bgm_replaced & -65);
							}
							return true;
						}
						else
						{
							if (!(cmd == "VALOTIZE"))
							{
								goto IL_0482;
							}
							if (rER.Nm(1, 1f) != 0f)
							{
								this.FlagValotStabilize.Rem("EV");
							}
							else
							{
								this.FlagValotStabilize.Add("EV");
							}
							return true;
						}
					}
					else if (num != 1025204630U)
					{
						if (num != 1286108617U)
						{
							goto IL_0482;
						}
						if (!(cmd == "ITEM_DEACTIVATE"))
						{
							goto IL_0482;
						}
						this.curMap.evItemActivate(rER._1, -1);
						return true;
					}
					else
					{
						if (!(cmd == "INIT_MAP_MATERIAL"))
						{
							goto IL_0482;
						}
						Map2d map2d = this.Get(rER._1, false);
						if (map2d == null)
						{
							return rER.tError("Map2d が不明: " + rER._1);
						}
						this.initMapMaterialASync(map2d, rER.Int(2, 1), true);
						return true;
					}
				}
				else if (num <= 2123024112U)
				{
					if (num != 1460610478U)
					{
						if (num != 1656570214U)
						{
							if (num != 2123024112U)
							{
								goto IL_0482;
							}
							if (!(cmd == "ADD_MAPFLUSH_FLAG"))
							{
								goto IL_0482;
							}
							M2DBase.material_flush_flag |= M2DBase.FLUSH.MATERIAL | M2DBase.FLUSH.MAP;
							if (this.ALoader != null)
							{
								for (int i = this.ALoader.Count - 1; i >= 0; i--)
								{
									this.ALoader[i].flushing = true;
								}
							}
							return true;
						}
						else
						{
							if (!(cmd == "ITEM_ACTIVATE"))
							{
								goto IL_0482;
							}
							this.curMap.evItemActivate(rER._1, 1);
							return true;
						}
					}
					else
					{
						if (!(cmd == "HIDDEN_WHOLE_SCREEN"))
						{
							goto IL_0482;
						}
						if (rER.Nm(1, 1f) != 0f)
						{
							this.FlgHideWholeScreen.Add("EV");
						}
						else
						{
							this.FlgHideWholeScreen.Rem("EV");
						}
						return true;
					}
				}
				else if (num <= 2793803244U)
				{
					if (num != 2443818774U)
					{
						if (num != 2793803244U)
						{
							goto IL_0482;
						}
						if (!(cmd == "SEND_EVENT_CORRUPTION"))
						{
							goto IL_0482;
						}
						M2EventContainer eventContainer = this.curMap.getEventContainer();
						if (eventContainer == null)
						{
							return rER.tError("EVC is empty");
						}
						if (eventContainer.stackSpecialCommand(rER._1, null, true))
						{
							EV.moduleInitAfter(ER, null, false);
						}
						return true;
					}
					else
					{
						if (!(cmd == "START_DGN_HALF_BGM"))
						{
							goto IL_0482;
						}
						this.fineDgnHalfBgm(true);
						return true;
					}
				}
				else if (num != 3006393793U)
				{
					if (num != 3714866345U)
					{
						goto IL_0482;
					}
					if (!(cmd == "DEF_MOBTYPE"))
					{
						goto IL_0482;
					}
					if (TX.valid(rER._1))
					{
						ER.VarCon.define(rER._1, this.ev_mobtype ?? "", true);
					}
					return true;
				}
				else if (!(cmd == "DEFEAT_EVENT"))
				{
					goto IL_0482;
				}
				if (this.isGameOverActive())
				{
					if (rER.cmd == "DEFEAT_EVENT")
					{
						ER.seek_set(ER.getLength());
					}
					return true;
				}
				if (rER.cmd == "DEFEAT_EVENT2")
				{
					EV.moduleInit(ER, rER._1, rER.slice(2, -1000), false);
				}
				else
				{
					EV.changeEvent(rER._1, 0, rER.slice(2, -1000));
				}
				return true;
			}
			IL_0482:
			return this.curMap != null && this.curMap.evReadMap(ER, rER, skipping);
		}

		public virtual bool isGameOverActive()
		{
			return this.curMap == null || this.curMap.Pr == null || !this.curMap.Pr.is_alive;
		}

		public void flushGF()
		{
			if ((M2DBase.material_flush_flag & M2DBase.FLUSH.GF_TEMP_CLEAR) != (M2DBase.FLUSH)0)
			{
				M2DBase.material_flush_flag &= ~M2DBase.FLUSH.GF_TEMP_CLEAR;
				GF.flushGfc(false);
			}
		}

		public virtual bool EvtOpen(bool is_start)
		{
			if (is_start)
			{
				this.FlagValotStabilize.Rem("EV");
			}
			CsvVariableContainer variableContainer = EV.getVariableContainer();
			if (variableContainer != null)
			{
				variableContainer.define("mobtype", this.ev_mobtype, true);
			}
			if (this.curMap != null)
			{
				return this.curMap.evOpenMap(is_start);
			}
			this.flushGF();
			return true;
		}

		public virtual bool EvtClose(bool is_end)
		{
			if (is_end)
			{
				this.Cam.do_not_consider_decline_area = false;
				this.flushGF();
				this.valot_stabilize_ev_on_end = true;
				this.FlgRenderAfter.Rem("EV");
				this.FlgHideWholeScreen.Rem("EV");
				this.fineDgnHalfBgm(true);
			}
			return this.curMap == null || this.curMap.evCloseMap(is_end);
		}

		public bool EvtMoveCheck()
		{
			return this.curMap == null || EV.isStoppingGame() || this.curMap.evMoveCheck();
		}

		public virtual IHkdsFollowable FindHkdsFollowableObject(string key)
		{
			if (REG.match(key, M2EventCommand.RegCmdChangeMov))
			{
				return M2EventCommand.getEventTargetMover(REG.R1);
			}
			return null;
		}

		public bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			V = Vector3.zero;
			return false;
		}

		public virtual bool readPtcScript(PTCThread rER)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num > 1910067188U)
				{
					if (num <= 2747429439U)
					{
						if (num != 2210515005U)
						{
							if (num != 2747429439U)
							{
								goto IL_0526;
							}
							if (!(cmd == "%QU_HANDSHAKE_NEAR"))
							{
								goto IL_0526;
							}
							float num2 = 1f - X.ZPOW(X.LENGTHXYS(this.Cam.x * this.curMap.rCLEN, this.Cam.y * this.curMap.rCLEN, rER.Nm(1, 0f), rER.Nm(2, 0f)) - 5f, 15f);
							if (num2 > 0f)
							{
								this.Cam.Qu.HandShake(rER.Nm(3, 0f), rER.Nm(4, 1f), rER.Nm(5, -1f) * num2, rER.Int(6, 0));
							}
							return true;
						}
						else if (!(cmd == "%EFT"))
						{
							goto IL_0526;
						}
					}
					else if (num != 3692468687U)
					{
						if (num != 3732683515U)
						{
							if (num != 3853545659U)
							{
								goto IL_0526;
							}
							if (!(cmd == "%EF"))
							{
								goto IL_0526;
							}
							rER.setEffectTo(this.curMap.getEffect(), rER.cmd == "%EF", 1, null);
							return true;
						}
						else if (!(cmd == "%TOP"))
						{
							goto IL_0526;
						}
					}
					else
					{
						if (!(cmd == "%DRO"))
						{
							goto IL_0526;
						}
						M2DropObjectReader.checkDRO(this.curMap, rER);
						return true;
					}
					rER.setEffectTo(this.curMap.getEffectTop(), rER.cmd == "%EFT", 1, null);
					return true;
				}
				if (num <= 363198355U)
				{
					if (num != 363095538U)
					{
						if (num == 363198355U)
						{
							if (cmd == "%SND")
							{
								if (X.DEBUGNOSND)
								{
									return true;
								}
								bool flag = false;
								M2SoundPlayerItem m2SoundPlayerItem;
								if (rER.clength >= 3)
								{
									bool flag2;
									StringKey soundKey = rER.getSoundKey(1, 0, out flag2, out flag);
									m2SoundPlayerItem = this.curMap.playSnd(soundKey, rER.key, rER._N2, (rER.clength >= 4) ? rER._N3 : (this.Cam.y / this.curMap.CLEN), 1);
								}
								else
								{
									bool flag3;
									m2SoundPlayerItem = M2DBase.playSnd(rER.getSoundKey(1, rER.clength - 1, out flag3, out flag));
								}
								if (flag)
								{
									rER.StockSound(m2SoundPlayerItem);
								}
								return true;
							}
						}
					}
					else if (cmd == "%QU_SINV_NEAR")
					{
						float num3 = 1f - X.ZPOW(X.LENGTHXYS(this.Cam.x * this.curMap.rCLEN, this.Cam.y * this.curMap.rCLEN, rER.Nm(1, 0f), rER.Nm(2, 0f)) - 5f, 15f);
						if (num3 > 0f)
						{
							this.Cam.Qu.SinV(rER.Nm(3, 0f) * num3, (float)rER.Int(4, 1), rER.Nm(5, -1f) * num3, rER.Int(6, 0));
						}
						return true;
					}
				}
				else if (num != 370810836U)
				{
					if (num != 634670334U)
					{
						if (num == 1910067188U)
						{
							if (cmd == "%QU_SINH_NEAR")
							{
								float num4 = 1f - X.ZPOW(X.LENGTHXYS(this.Cam.x * this.curMap.rCLEN, this.Cam.y * this.curMap.rCLEN, rER.Nm(1, 0f), rER.Nm(2, 0f)) - 5f, 15f);
								if (num4 > 0f)
								{
									this.Cam.Qu.SinH(rER.Nm(3, 0f) * num4, (float)rER.Int(4, 1), rER.Nm(5, -1f) * num4, rER.Int(6, 0));
								}
								return true;
							}
						}
					}
					else if (cmd == "%DRO_GROUNDBREAK")
					{
						this.curMap.DropCon.setGroundBreaker(rER.Nm(2, 0f), rER.Nm(3, 0f), rER.Nm(4, 1f), rER.Nm(5, 1f), M2DropObjectReader.Get(rER.getHash(1), false));
						return true;
					}
				}
				else if (cmd == "%AGDT")
				{
					AttackGhostDrawer agd = EfParticleManager.GetAGD(rER.getHash(1));
					if (agd == null)
					{
						return false;
					}
					EffectItem effectItem = rER.setEffectTo(this.curMap.getEffectTop(), true, 2, agd.FD_EfDraw);
					if (effectItem != null)
					{
						effectItem.setFunction(agd.FD_EfDraw, rER._2);
						return false;
					}
					return false;
				}
			}
			IL_0526:
			if (this.Cam.Qu.readPtcScript(rER, 1f))
			{
				return true;
			}
			return false;
		}

		public bool isSoundActive(SndPlayer S)
		{
			M2SoundPlayerItem m2SoundPlayerItem = S as M2SoundPlayerItem;
			return m2SoundPlayerItem != null && m2SoundPlayerItem.key == this.getSoundKey();
		}

		public string getSoundKey()
		{
			return "";
		}

		public static Shader getShd(string shader_name)
		{
			return M2DBase.MtiShader.LoadShader(shader_name);
		}

		public static Material newMtr(string shader_name)
		{
			return MTRX.newMtr(M2DBase.getShd(shader_name));
		}

		public bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			return true;
		}

		public virtual void assignTalkableObject(M2MoverPr FromMv, int x, int y, AIM k, List<IM2TalkableObject> ATk)
		{
		}

		public virtual void changeTalkTarget(IM2TalkableObject ChangedTo)
		{
		}

		public virtual void colliderUpdated()
		{
		}

		public virtual void mainMvIntPosChanged(bool pos4 = false)
		{
		}

		public virtual int getSF(string key)
		{
			return 0;
		}

		public virtual void setSF(string key, int v)
		{
		}

		public static M2SoundPlayerItem playSnd(string header, string cue_key)
		{
			return M2DBase.Instance.Snd.play(header, cue_key);
		}

		public static M2SoundPlayerItem playSnd(string cue_key)
		{
			return M2DBase.playSnd("", cue_key);
		}

		public virtual bool needInitMaterial(Map2d DestMap)
		{
			return this.curMap == null || this.DGN.getDgn(DestMap) != this.curMap.Dgn;
		}

		public bool EvtWait(bool is_first = false)
		{
			bool flag = this.isMaterialLoading(false);
			if (flag && (this.transfer_mode & M2DBase.MAP_TRANSFER.MANUAL) != M2DBase.MAP_TRANSFER.AUTO)
			{
				if (this.curMap != null && this.ALoader != null)
				{
					this.loadMapAsync();
				}
				this.transfer_mode |= M2DBase.MAP_TRANSFER.MAP_CLOSE_PREPARED;
			}
			if (!flag)
			{
				EV.stopGMain(true);
			}
			return flag;
		}

		public virtual bool isSafeArea()
		{
			return false;
		}

		protected List<M2Mover> EvacuateCoreMover(List<M2Mover> AEvacuated = null)
		{
			if (this.curMap == null)
			{
				return null;
			}
			if (AEvacuated == null)
			{
				List<M2Mover> list = new List<M2Mover>();
				M2Mover[] vectorMover = this.curMap.getVectorMover();
				for (int i = this.curMap.count_movers - 1; i >= 0; i--)
				{
					M2Mover m2Mover = vectorMover[i];
					if (m2Mover.do_not_destruct_when_remove)
					{
						if (list.IndexOf(m2Mover) == -1)
						{
							list.Add(m2Mover);
						}
						this.curMap.removeMover(m2Mover);
					}
				}
				return list;
			}
			for (int j = AEvacuated.Count - 1; j >= 0; j--)
			{
				M2Mover m2Mover2 = AEvacuated[j];
				this.curMap.assignMover(m2Mover2, true);
			}
			return null;
		}

		public void AddToCoreMover(M2Mover Mv)
		{
			if (this.AEvacuatedMover == null)
			{
				this.AEvacuatedMover = new List<M2Mover>(1);
			}
			if (this.AEvacuatedMover.IndexOf(Mv) == -1)
			{
				this.AEvacuatedMover.Add(Mv);
			}
			if (this.curMap != null)
			{
				this.curMap.removeMover(Mv);
			}
		}

		public MImage MIchip
		{
			get
			{
				return this.IMGS.MIchip;
			}
		}

		public bool has_mapflush_flag
		{
			get
			{
				return (M2DBase.material_flush_flag & M2DBase.FLUSH.MAP) > (M2DBase.FLUSH)0;
			}
		}

		public bool has_pxlflush_flag
		{
			get
			{
				return (M2DBase.material_flush_flag & M2DBase.FLUSH.MATERIAL) > (M2DBase.FLUSH)0;
			}
		}

		public virtual void fineWindowSizeChanged(int width, int height)
		{
			if (!this.isDestructed() && this.Cam != null)
			{
				this.Cam.fineRenderedTextureAntiAlias();
			}
		}

		public bool debug_listener_created
		{
			get
			{
				return this.EvLsn != null && this.EvLsn.isAwaken();
			}
		}

		public bool debug_listener_active
		{
			get
			{
				return this.EvLsn != null && this.EvLsn.isActive();
			}
		}

		public static readonly string[] Achip_pxl_key = new string[]
		{
			"MapChip__general", "MapChip_forest", "MapChip_house", "MapChip_forest_scape", "MapChip_treearea_bg", "MapChip_mech", "MapChip_mush", "MapChip_worm", "MapChip_sea", "MapChip_ruins",
			"MapChip_glacier", "MapChip_grazia", "MapChip_interior"
		};

		public const bool FORCE_CSP_READ_IF_COUNT_WRONG = false;

		public static readonly string[] Achip_pxlpose_necessary = new string[] { "whole", "_import", "_anim_breakable_skin" };

		public const string ext_list_old = ".csv";

		public const string chip_pxl_dir = "MapChips/";

		public const string data_dir = "m2d/";

		public const string data_dir_streaming = "StreamingAssets/m2d/";

		public const string ext_cmd = ".cmd";

		public const string ext_list_dat = ".dat";

		public const string ext_csv = ".csv";

		public const string ext_binary = ".tmap";

		public const string chiplist_name = "__m2d_chips";

		public const string whole_map_key = "__whole";

		public readonly int map_object_layer = LayerMask.NameToLayer("Chips");

		public const string mob_pxl_dir = "MapChars/";

		public const string TagGround = "Ground";

		public const string TagBlock = "Block";

		public const string TagMoverEn = "MoverEn";

		public const string TagMoverPr = "MoverPr";

		public static readonly Regex RegFindShift = new Regex("\\+?(\\-?\\d+)[ \\,]+\\+?(\\-?\\d+)");

		public static readonly Regex RegFindMover = new Regex("(?:\\#< *)(\\%?[\\$\\w]+)(?: *>)?");

		public static readonly Regex RegFindPointLayer = new Regex("^(\\w+) *\\.\\. *");

		public static readonly Regex RegFindPointLayerIndex = new Regex("^(\\d+)\\~\\.\\. *");

		public static bool cfg_simplify_bg_drawing = true;

		public static bool load_stop_running = false;

		public static byte blood_weaken = 0;

		public const byte BLOOD_WEAKEN_MAX = 100;

		public readonly int default_mover_layer;

		public static readonly bool use_dgn = false;

		public readonly float CLEN = 28f;

		private readonly NDic<Map2d> OM2d;

		private readonly BDic<string, PxlLayer> OPxlLayer;

		public readonly M2ImageContainer IMGS;

		public readonly DungeonContainer DGN;

		public readonly M2MapDamageContainer MDMGCon;

		protected M2EvDebugListener EvLsn;

		private static MTI MtiShader;

		private byte draw_count;

		private byte draw_count_ef;

		public static readonly string LOAD_FAM = "M2D";

		public static readonly int LOADLV_MAP_PRE = 8;

		public static readonly int LOADLV_IMG = 10;

		public static readonly int LOADLV_CHIP = 9;

		public static readonly int LOADLV_AFTER = 16;

		public Map2d curMap;

		public M2Camera Cam;

		public float gravity;

		public float gravity_max_spd = 0.23f;

		public readonly bool draw_shadow;

		protected float ui_shift_x_;

		public bool nest_layers;

		public bool no_draw_bkgnd;

		public bool pre_map_active;

		private bool stopping_game_;

		public bool chips_noconsider_draw_sort;

		public bool event_eval_inited;

		public fnMapItemYSortFunction fnYSortFunction;

		public SORT<M2Puts> SorterChip;

		protected TutorialBox TutoBox;

		public int move_repeat_time = 1;

		private readonly PAUSER Pauser;

		public GameObject GobBase;

		public M2SoundPlayer Snd;

		protected byte bgm_replaced;

		private List<M2DBase.LMtr> Acurrent_async_loading;

		private List<M2DBase.LMtr> Amaterial_loaded;

		public HashP Hash;

		public M2BorderCldCreator ColliderCreator = new M2BorderCldCreator(null, 0, 0, 1f, false);

		public M2BlockColliderContainer BufferBCC;

		public readonly Flagger FlgWarpEventNotInjectable;

		public readonly Flagger FlgHideWholeScreen;

		private int pre_drawn_totalframe;

		private float t_lock_check_push_up_;

		protected static M2DBase.FLUSH material_flush_flag = (M2DBase.FLUSH)0;

		public static M2DBase Instance;

		protected List<M2MapMaterialLoader> ALoader;

		private float effect_variable0_;

		private readonly Map2d WholeMap_;

		public M2ImageContainer.FnCreateChipPrepartion FnCreateChipPreparation = new M2ImageContainer.FnCreateChipPrepartion(M2CImgDrawer.CreateDrawerPreparationDefault);

		public M2LabelPoint.FnCreateLp FnCreateLp = M2LabelPoint.CreateLpDefault;

		protected List<M2Mover> AEvacuatedMover;

		public Flagger FlgRenderAfter;

		public Flagger FlagValotStabilize;

		protected bool auto_evacuate = true;

		protected bool next_valotile_activate;

		private bool valot_stabilize_ev_on_end;

		private readonly List<IValotileSetable> AValotAddition;

		public IN.FnWindowSizeChanged FD_WindowSizeChanged;

		public string ev_mobtype;

		public M2DBase.MAP_TRANSFER transfer_mode;

		private M2DBase.PREPARE pstate;

		public enum LM_TYPE
		{
			PXL,
			SND,
			MTI,
			TXT
		}

		protected struct LMtr
		{
			public LMtr(M2DBase.LM_TYPE _type, string _key)
			{
				this.key = _key;
				this.type = _type;
			}

			public readonly M2DBase.LM_TYPE type;

			public readonly string key;
		}

		[Flags]
		public enum FLUSH
		{
			MATERIAL = 1,
			MAP = 2,
			INIT_GAME = 4,
			CHANGE_AREA = 8,
			RELOAD_AREA_MATERIAL = 16,
			_ALL = 31,
			GF_TEMP_CLEAR = 32,
			_LOCK_ASYNC_ALREADY = 64,
			_LOCK_ASYNC_LOAD = 7
		}

		private enum PREPARE
		{
			NO_LOAD,
			PREPARE_CHIPS,
			PREPARE_CHIPS_READING,
			PREPARE_CHIPS_COMP,
			LOAD_EV,
			LOAD_AFTER,
			INIT_FIRST_EVENT,
			COMPLETE
		}

		public enum MAP_TRANSFER
		{
			AUTO,
			MANUAL,
			MAP_CLOSE_PREPARED,
			GAME_STOP
		}
	}
}
