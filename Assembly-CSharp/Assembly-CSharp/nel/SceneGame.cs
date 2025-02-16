using System;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class SceneGame : MonoBehaviourAutoRun
	{
		public void Awake()
		{
			X.dli("SCENE GAME AWAKEN", null);
			this.GobHideScreen = new GameObject("HideScreen");
			this.GobHideScreen.layer = IN.LAY(IN.gui_layer_name);
			IN.setZ(this.GobHideScreen.transform, -9.41f);
			SceneGame.InitG();
		}

		private void prepareHideScreen()
		{
			this.MdHideScreen = MeshDrawer.prepareMeshRenderer(this.GobHideScreen, MTRX.getMtr(BLEND.NORMAL, -1), 0f, -1, null, true, false);
			this.fillBlack(1f, false);
		}

		public static void InitG()
		{
			SceneGame.uistatus_fine_load = true;
		}

		public void fillBlack(float alpha = 1f, bool daia_only = false)
		{
			SceneGame.fillBlack(this.MdHideScreen, alpha, daia_only);
		}

		public static void fillBlack(MeshDrawer MdHideScreen, float alpha, bool daia_only)
		{
			MdHideScreen.Col = MdHideScreen.ColGrd.Set(4282004532U).mulA(alpha).C;
			if (daia_only)
			{
				NEL.loadDrawingFill(MdHideScreen);
			}
			else
			{
				MdHideScreen.Rect(0f, 0f, IN.w + 4f, IN.h + 4f, false);
			}
			if (alpha == 1f)
			{
				NEL.loadDrawing(MdHideScreen);
			}
			MdHideScreen.updateForMeshRenderer(false);
		}

		public override void OnDestroy()
		{
			if (SceneGame.M2D != null)
			{
				SceneGame.M2D.destruct(true);
			}
			if (this.MdHideScreen != null)
			{
				this.MdHideScreen.destruct();
			}
			SceneGame.M2D = null;
			COOK.clear(false);
			MTRX.clearStorageListener();
			base.OnDestroy();
		}

		public static NelM2DBase prepareM2DObject()
		{
			if (SceneGame.M2D != null)
			{
				SceneGame.M2D.destruct(true);
			}
			SceneGame.M2D = new NelM2DBase(null, true);
			return SceneGame.M2D;
		}

		protected override bool runIRD(float fcnt)
		{
			if (this.t == -1f)
			{
				if (SceneGame.contents_loaded)
				{
					this.t = 2f;
					this.prepareHideScreen();
				}
				else
				{
					this.t = 0f;
				}
			}
			else if (this.t == 0f)
			{
				if (MTRX.prepared && MTR.prepare1 && MTR.preparedG)
				{
					EV.loadEV();
					COOK.clear(false);
					this.t = 1f;
					this.prepareHideScreen();
				}
			}
			else if (this.t == 1f)
			{
				if (EV.material_prepared)
				{
					if (!X.DEBUGNOCFG)
					{
						CFG.loadSdFile(true, true);
						MGV.loadSdFile();
					}
					SVD.prepareList(true);
					this.t = 2f;
				}
				else
				{
					this.fillBlack(1f, false);
				}
			}
			else if (this.t == 2f)
			{
				if (SceneGame.M2D == null)
				{
					SceneGame.prepareM2DObject();
				}
				if (!SceneGame.M2D.loadBasicMaterialProgress(200) && MTR.preparedG)
				{
					SVD.prepareList(false);
					this.Pr = this.GobNoel.GetComponent<PRNoel>();
					SceneGame.M2D.PlayerNoel = this.Pr;
					SceneGame.M2D.initGameObject(base.gameObject, IN.wh, IN.hh, true);
					new GameObject("UI").AddComponent<UIBase>();
					SceneGame.restartGameSceneASync(SceneGame.M2D, false, SceneGame.contents_loaded ? "" : "");
					if (COOK.error_loaded_index >= 0)
					{
						base.enabled = false;
						SceneGame.M2D.quitGame("SceneTitle");
						return true;
					}
					if (!SceneGame.M2D.transferring_game_stopping)
					{
						this.GobHideScreen.SetActive(false);
					}
					this.t = 3f;
					SceneGame.M2D.stackFirstEvent();
				}
				else
				{
					this.fillBlack(1f, false);
				}
			}
			else if (SceneGame.M2D.transferring_game_stopping)
			{
				if (this.t < 60f)
				{
					this.t = 3f;
				}
				EV.do_not_draw_loading_circle = true;
				if (!SceneGame.M2D.loadMapAsync())
				{
					SceneGame.fill_black_all = false;
					Resources.UnloadUnusedAssets();
					SceneGame.M2D.transferring_game_stopping = false;
					if (SceneGame.uistatus_fine_load)
					{
						this.t = 3f;
						SceneGame.uistatus_fine_load = false;
						UIStatus.Instance.fineLoad();
						EV.stack("__INITNEWGAME", 0, -1, null, null);
					}
					EV.do_not_draw_loading_circle = false;
					if (this.t == 3f)
					{
						this.t = 4f;
					}
					else
					{
						this.MdHideScreen.clear(false, false);
						this.GobHideScreen.SetActive(false);
					}
					COOK.reloading = false;
					if (SceneGame.M2D.Ui != null)
					{
						SceneGame.M2D.Ui.transition_darken = true;
					}
					SceneGame.M2D.fineMaterialColor(false);
				}
				else
				{
					if (UIBase.Instance != null)
					{
						UIBase.Instance.run(fcnt);
					}
					if (!this.GobHideScreen.activeSelf)
					{
						this.GobHideScreen.SetActive(true);
					}
					this.fillBlack(1f, !SceneGame.fill_black_all);
				}
			}
			else
			{
				if (IN.enable_vsync)
				{
					M2Phys.fixed_updating = true;
				}
				SceneGame.M2D.run(fcnt);
				SceneGame.M2D.runPost(fcnt);
				if (!IN.enable_vsync)
				{
					SceneGame.M2D.runPostForDraw(fcnt, true, true);
				}
				bool can_handle = Map2d.can_handle;
				if (this.t < 60f)
				{
					this.t += fcnt;
					if (this.t >= 60f || SceneGame.for_debug)
					{
						this.GobHideScreen.SetActive(false);
					}
					else
					{
						this.fillBlack(1f - X.ZLINE(this.t - 4f, 56f), false);
					}
				}
			}
			return true;
		}

		public void Update()
		{
			if (IN.enable_vsync && this.t >= 4f && !SceneGame.M2D.transferring_game_stopping)
			{
				SceneGame.M2D.runPostForDraw(Time.unscaledDeltaTime * 60f, true, true);
			}
		}

		public void FixedUpdate()
		{
			if (this.t >= 4f && !SceneGame.M2D.transferring_game_stopping)
			{
				SceneGame.M2D.runPhysics(1f);
			}
		}

		public static bool restartGameSceneASync(NelM2DBase M2D, bool again = false, string def_map = "")
		{
			COOK.reloading = true;
			if (M2D.curMap != null)
			{
				M2D.changeMap(null);
			}
			UILog.Instance.clear();
			BGM.clearHalfFlag();
			PRNoel prNoel = M2D.getPrNoel();
			IN.Pos2(prNoel.transform, 0f, 0f);
			SceneGame.svd_reading = true;
			bool flag = true;
			if (again)
			{
				SceneGame.InitG();
			}
			NEL.createTextLog();
			if (!COOK.initGameScene(M2D))
			{
				Map2d map2d = (TX.valid(def_map) ? M2D.Get(def_map, false) : null);
				map2d = map2d ?? M2D.Get(COOK.first_map_key, false);
				if (M2D.curMap != map2d)
				{
					M2D.initMapMaterialASync(map2d, 2, false);
				}
				M2D.setFlushAllFlag(true);
				M2D.AddToCoreMover(prNoel);
				prNoel.newGame();
				EnhancerManager.fineEnhancerStorage(M2D.IMNG.getInventoryPrecious(), M2D.IMNG.getInventoryEnhancer());
				flag = false;
			}
			else if (TX.valid(def_map))
			{
				Map2d map2d2 = M2D.Get(def_map, false);
				if (map2d2 != null && map2d2 != prNoel.Mp)
				{
					M2D.initMapMaterialASync(map2d2, 2, false);
				}
			}
			M2D.setGfTempFlushFlagOnMapChange(false);
			M2D.IMNG.fineSpecialNoelRow(prNoel);
			NelMSGResource.initResource(false);
			SceneGame.fill_black_all = true;
			M2D.BlurSc.deactivate(true);
			M2D.transferring_game_stopping = true;
			SceneGame.svd_reading = false;
			if (again)
			{
				EV.stack("__INITM2D", 0, -1, null, null);
			}
			return flag;
		}

		private static NelM2DBase M2D;

		[SerializeField]
		private GameObject GobNoel;

		public static bool for_debug = false;

		private const string go_to_map = "";

		private PRNoel Pr;

		private float t = -1f;

		private GameObject GobHideScreen;

		private MeshDrawer MdHideScreen;

		public static bool contents_loaded = false;

		public static bool svd_reading = false;

		public static bool fill_black_all = false;

		public static bool uistatus_fine_load = true;
	}
}
