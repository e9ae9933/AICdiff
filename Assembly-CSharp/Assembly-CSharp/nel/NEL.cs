using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using nel.fatal;
using nel.mgm;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NEL : MonoBehaviour, IEventListener
	{
		public static string version
		{
			get
			{
				string text = "ver " + Application.version;
				if (MGV.temp_kisekae_max > 1)
				{
					text += "R";
				}
				return text + "\n<font size=\"10\"> (25/02/01 early access version IX)</font>";
			}
		}

		public static Color32 ColText
		{
			get
			{
				return C32.d2c(4283780170U);
			}
		}

		public static Color32 ColFocus
		{
			get
			{
				return C32.d2c(4294966715U);
			}
		}

		public void Start()
		{
			if (NEL.loaded)
			{
				global::UnityEngine.Object.Destroy(this);
				return;
			}
			if (NEL.Instance != null && NEL.Instance != this)
			{
				NEL.Instance = this;
				return;
			}
			CFG.clearSpVariable();
			MTR.init1();
			IN.MainDarkColor = 4282004532U;
			MTRX.auto_load_efparticle = false;
			MTRX.text_color = 4283780170U;
			TextRenderer.keyassign_text_col = 4293321691U;
			TextRenderer.keyassign_text_border_col = 4278190080U;
			EvSmallWin.boxcol_top = 4293321691U;
			EvSmallWin.boxcol_btm = 4291611332U;
			CoinStorage.init();
			NEL.Instance = this;
			ButtonSkinCommand.FnDrawAdditionalIcon = new Action<ButtonSkinCommand, MeshDrawer>(NEL.FnDrawAdditionalIconEvtCommand);
		}

		public void Update()
		{
			if (X.DEBUGRELOADMTR && this.Nad == null)
			{
				this.Nad = IN._stage.gameObject.AddComponent<NelActiveDebugger>();
			}
			if (!this.auto_material_load)
			{
				if (MTRX.prepared && MTR.prepare1)
				{
					this.LoadFinalize();
					if (this.Vib == null)
					{
						this.Vib = new PadVibManager();
					}
				}
				return;
			}
			if (this.t == 0 && IN._stage != null && MTRX.prepared && MTR.prepare1 && MTR.preparedT)
			{
				this.t = 1;
				MTR.initG();
				if (this.Vib == null)
				{
					this.Vib = new PadVibManager();
					this.Vib.base_level = (float)CFG.vib_level * 0.01f;
					return;
				}
			}
			else if (this.t == 1 && MTRX.prepared)
			{
				if (!X.DEBUGNOCFG)
				{
					CFG.loadSdFile(true, true);
				}
				this.LoadFinalize();
			}
		}

		private void LoadFinalize()
		{
			base.enabled = false;
			if (NEL.loaded)
			{
				return;
			}
			if (!X.DEBUGNOSND && SND.Ui != null)
			{
				NEL.ConfirmSndSubmit = new SndPlayer("submit", SndPlayer.SNDTYPE.SND);
				NEL.ConfirmSndCancel = new SndPlayer("cancel", SndPlayer.SNDTYPE.SND);
				NEL.FlgSubmitSnd = new Flagger(delegate(FlaggerT<string> V)
				{
					NEL.ConfirmSndSubmit.playDefault();
				}, delegate(FlaggerT<string> V)
				{
					NEL.ConfirmSndSubmit.Stop();
				});
				NEL.FlgCancelSnd = new Flagger(delegate(FlaggerT<string> V)
				{
					NEL.ConfirmSndCancel.playDefault();
				}, delegate(FlaggerT<string> V)
				{
					NEL.ConfirmSndCancel.Stop();
				});
				SND.GetCue("ui_btn_hold", NEL.ConfirmSndSubmit);
				SND.GetCue("hold_pressing_loop", NEL.ConfirmSndCancel);
			}
			NEL.loaded = true;
		}

		public void OnDestroy()
		{
			if (NEL.Instance == this)
			{
				NEL.Instance = null;
			}
			if (this.Vib != null)
			{
				this.Vib.destruct();
			}
		}

		public static void stopPressingSound(string unique_key)
		{
			if (NEL.ConfirmSndSubmit != null)
			{
				NEL.FlgSubmitSnd.Rem(unique_key);
				NEL.FlgCancelSnd.Rem(unique_key);
			}
		}

		public static NelEvTextLog createTextLog()
		{
			if (EV.Log == null)
			{
				EV.Log = new NelEvTextLog(192);
			}
			return EV.Log as NelEvTextLog;
		}

		public static void AddSubmitHoldSnd(string key)
		{
			if (NEL.FlgSubmitSnd != null)
			{
				NEL.FlgSubmitSnd.Add(key);
			}
		}

		public static void RemSubmitHoldSnd(string key)
		{
			if (NEL.FlgSubmitSnd != null)
			{
				NEL.FlgSubmitSnd.Rem(key);
			}
		}

		public static bool flushState()
		{
			bool flag = false;
			if (COOK.getSF("_BATTLE_STARTED") > 0)
			{
				COOK.clearSFbyHeader("ISUPPLY_");
				COOK.clearSFbyHeader("TBOX_FLUSH_");
				CoinStorage.flush();
				flag = true;
			}
			COOK.setSF("_BATTLE_STARTED", 0);
			GF.flushGfc(true);
			return flag;
		}

		public static string nel_num(int t, bool zero_empty = false)
		{
			if (t == 0)
			{
				if (!zero_empty)
				{
					return ".";
				}
				return "";
			}
			else
			{
				if (t < 10)
				{
					return t.ToString();
				}
				if (t < 100)
				{
					return ((t < 20) ? "" : (t / 10).ToString()) + "0" + NEL.nel_num(t % 10, true);
				}
				if (t < 1000)
				{
					return ((t < 200) ? "" : (t / 100).ToString()) + "!" + NEL.nel_num(t % 100, true);
				}
				if (t < 10000)
				{
					return ((t < 2000) ? "" : (t / 1000).ToString()) + "#" + NEL.nel_num(t % 1000, true);
				}
				if (t < 100000)
				{
					return ((t < 20000) ? "" : (t / 10000).ToString()) + "$" + NEL.nel_num(t % 10000, true);
				}
				return NEL.nel_num(t / 100000, true) + NEL.nel_num(t % 100000, true);
			}
		}

		public static void nel_num(STB Stb, int t, bool zero_empty = false)
		{
			if (t == 0)
			{
				Stb += (zero_empty ? "" : ".");
				return;
			}
			if (t < 10)
			{
				Stb += t.ToString();
				return;
			}
			if (t < 100)
			{
				if (t >= 20)
				{
					Stb += t / 10;
				}
				Stb += "0";
				NEL.nel_num(Stb, t % 10, true);
				return;
			}
			if (t < 1000)
			{
				if (t >= 200)
				{
					Stb += t / 100;
				}
				Stb += "!";
				NEL.nel_num(Stb, t % 100, true);
				return;
			}
			if (t < 10000)
			{
				if (t >= 2000)
				{
					Stb += t / 1000;
				}
				Stb += "#";
				NEL.nel_num(Stb, t % 1000, true);
				return;
			}
			if (t < 100000)
			{
				if (t >= 20000)
				{
					Stb += t / 10000;
				}
				Stb += "$";
				NEL.nel_num(Stb, t % 10000, true);
				return;
			}
			NEL.nel_num(Stb, t / 100000, true);
			NEL.nel_num(Stb, t % 100000, true);
		}

		public static bool readLocalizeTxItemScript(CsvReader CR, TX.TXFamily TxFam, ref TX curTX)
		{
			string cmd = CR.cmd;
			if (cmd != null)
			{
				if (!(cmd == "%ITEM"))
				{
					if (!(cmd == "%RECIPE_REPLACE"))
					{
						if (cmd == "%ITEMREEL")
						{
							TX.getTX("_ItemReel_name_" + CR._1, false, false, TxFam).replaceTextContents(CR.slice_join(2, " ", ""));
							return true;
						}
					}
					else
					{
						if (!NelItem.preparedData())
						{
							return true;
						}
						if (NelItem.GetById(CR._1, false) == null)
						{
							X.de("不明なアイテム: " + CR._1, null);
							curTX = null;
							return true;
						}
						TX.getTX("_NelItem_nameR_" + CR._1, false, false, TxFam).replaceTextContents(CR.slice_join(2, " ", ""));
						return true;
					}
				}
				else
				{
					if (!NelItem.preparedData())
					{
						return true;
					}
					if (NelItem.GetById(CR._1, false) == null)
					{
						X.de("不明なアイテム: " + CR._1, null);
						curTX = null;
						return true;
					}
					TX.getTX("_NelItem_name_" + CR._1, false, false, TxFam).replaceTextContents(CR.slice_join(2, " ", ""));
					curTX = TX.getTX("_NelItem_desc_" + CR._1, false, false, TxFam);
					return true;
				}
			}
			return false;
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2345100994U)
				{
					if (num <= 985817265U)
					{
						if (num <= 460275000U)
						{
							if (num != 272785799U)
							{
								if (num != 409441053U)
								{
									if (num != 460275000U)
									{
										goto IL_096E;
									}
									if (!(cmd == "LUNCHSTORE"))
									{
										goto IL_096E;
									}
									StoreManager storeManager = StoreManager.Get(rER._1, true);
									if (storeManager == null)
									{
										return rER.tError("店IDが不明: " + rER._1);
									}
									UiLunchStore uiLunchStore = IN.CreateGob(base.gameObject, "-Lunch").AddComponent<UiLunchStore>();
									uiLunchStore.addExternal(storeManager);
									IN.setZ(uiLunchStore.transform, -4.05f);
									EV.initWaitFn(uiLunchStore, 0);
									return true;
								}
								else
								{
									if (!(cmd == "UIBOX_MONEY_ACTIVATE"))
									{
										goto IL_096E;
									}
									UiBoxDesignerFamilyEvent dsFamilyForEvent = NEL.getDsFamilyForEvent();
									UiBoxDesigner uiBoxDesigner = dsFamilyForEvent.Get<UiBoxMoney>();
									if (uiBoxDesigner == null)
									{
										uiBoxDesigner = dsFamilyForEvent.CreateT<UiBoxMoney>("Money", 0f, 0f, 30f, 30f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
									}
									uiBoxDesigner.activate();
									return true;
								}
							}
							else
							{
								if (!(cmd == "UI_GUILDQUEST"))
								{
									goto IL_096E;
								}
								if (M2DBase.Instance != null)
								{
									new UiGQManageBox(M2DBase.Instance as NelM2DBase, rER._1, rER._B2, null);
								}
								return true;
							}
						}
						else if (num != 529558681U)
						{
							if (num != 812547004U)
							{
								if (num != 985817265U)
								{
									goto IL_096E;
								}
								if (!(cmd == "UI_GUILDQUEST_REFINE"))
								{
									goto IL_096E;
								}
								if (M2DBase.Instance != null)
								{
									(M2DBase.Instance as NelM2DBase).GUILD.need_fine_flag = true;
									return true;
								}
								return true;
							}
							else if (!(cmd == "ALCHEMY"))
							{
								goto IL_096E;
							}
						}
						else
						{
							if (!(cmd == "FATAL"))
							{
								goto IL_096E;
							}
							FatalShower fatalShower = new FatalShower(rER._1, rER.getB(2, false));
							X.dli("fatal scene " + rER._1 + " start.", null);
							fatalShower.autoinit_z_in_wait_fn = -4.15f;
							EV.initWaitFn(fatalShower, 0);
							return true;
						}
					}
					else
					{
						if (num <= 1504606479U)
						{
							if (num != 1028107636U)
							{
								if (num != 1443838714U)
								{
									if (num != 1504606479U)
									{
										goto IL_096E;
									}
									if (!(cmd == "ALCHEMY_COFFEEMAKER"))
									{
										goto IL_096E;
									}
									NEL.defaultEventAlchemyInitialize<UiAlchemyCoffeeMaker>(rER, false, -1);
									return true;
								}
								else
								{
									if (!(cmd == "ALCHEMY_WORKBENCH"))
									{
										goto IL_096E;
									}
									NEL.defaultEventAlchemyInitialize<UiAlchemyWorkBench>(rER, false, -1);
									return true;
								}
							}
							else if (!(cmd == "ITEMSTORE_RELOAD_BASIC"))
							{
								goto IL_096E;
							}
						}
						else if (num <= 1870688857U)
						{
							if (num != 1511554586U)
							{
								if (num != 1870688857U)
								{
									goto IL_096E;
								}
								if (!(cmd == "ALCHEMY_RECIPE_BOOK"))
								{
									goto IL_096E;
								}
								UiAlchemyRecipeBook uiAlchemyRecipeBook = new GameObject("RBK" + IN.totalframe.ToString()).AddComponent<UiAlchemyRecipeBook>();
								if (TX.valid(rER._1))
								{
									EV.fixObjectPositionTo(uiAlchemyRecipeBook.transform, rER._1);
								}
								ItemStorage[] array = NEL.prepareTemporaryInventory();
								UiCraftBase uiCraftBase = uiAlchemyRecipeBook;
								object obj;
								if (array == null)
								{
									obj = null;
								}
								else
								{
									(obj = new ItemStorage[1])[0] = array[0];
								}
								uiCraftBase.InitManager(obj, null, 1, null);
								EV.initWaitFn(uiAlchemyRecipeBook, 0);
								return true;
							}
							else
							{
								if (!(cmd == "COOKING_TUTORIAL"))
								{
									goto IL_096E;
								}
								goto IL_0497;
							}
						}
						else if (num != 2096655831U)
						{
							if (num != 2345100994U)
							{
								goto IL_096E;
							}
							if (!(cmd == "UIALBUM"))
							{
								goto IL_096E;
							}
							GameObject gameObject = IN.CreateGobGUI(null, "ALBUM");
							UiAlbum uiAlbum = gameObject.AddComponent<UiAlbum>();
							IN.setZ(gameObject.transform, -3.3000002f);
							if (TX.valid(rER._1))
							{
								uiAlbum.setDefBGM(rER._1, rER._2);
							}
							return true;
						}
						else if (!(cmd == "ITEMSTORE"))
						{
							goto IL_096E;
						}
						StoreManager storeManager = StoreManager.Get(rER._1, true);
						if (storeManager == null)
						{
							return rER.tError("店IDが不明: " + rER._1);
						}
						if (rER.cmd == "ITEMSTORE_RELOAD_BASIC")
						{
							storeManager.need_reload_basic = true;
							return true;
						}
						UiItemStore uiItemStore = storeManager.AddStoreComponent(new GameObject("ItemStore" + IN.totalframe.ToString()));
						if (TX.valid(rER._2))
						{
							EV.fixObjectPositionTo(uiItemStore.transform, rER._2);
						}
						uiItemStore.InitManager(storeManager, NEL.prepareTemporaryInventory());
						EV.initWaitFn(uiItemStore, 0);
						return true;
					}
				}
				else if (num <= 3660252937U)
				{
					if (num <= 3003100174U)
					{
						if (num != 2598037374U)
						{
							if (num != 2728866695U)
							{
								if (num != 3003100174U)
								{
									goto IL_096E;
								}
								if (!(cmd == "LUNCHTIME"))
								{
									goto IL_096E;
								}
								UiLunchTime uiLunchTime = IN.CreateGob(base.gameObject, "-Lunch").AddComponent<UiLunchTime>();
								uiLunchTime.default_select_item_key = EV.getVariableContainer().Get("_final_cooked_item");
								IN.setZ(uiLunchTime.transform, -4.05f);
								EV.initWaitFn(uiLunchTime, 0);
								return true;
							}
							else if (!(cmd == "UI_MGMTIMER_ADDSCORE_TX"))
							{
								goto IL_096E;
							}
						}
						else if (!(cmd == "UI_MGMTIMER_ADDSCORE"))
						{
							goto IL_096E;
						}
						UiMgmTimer timerUi = this.GetTimerUi();
						if (timerUi != null)
						{
							timerUi.resetScoreEntry(rER._1, (float)rER.Int(3, 0), rER._2, (rER.cmd == "UI_MGMTIMER_ADDSCORE") ? UiMgmTimer.SCORE_TYPE.ICON_SUFFIX : UiMgmTimer.SCORE_TYPE.TEXT_SUFFIX);
							return true;
						}
						return true;
					}
					else if (num <= 3381585093U)
					{
						if (num != 3304894242U)
						{
							if (num != 3381585093U)
							{
								goto IL_096E;
							}
							if (!(cmd == "NUMCOUNTER"))
							{
								goto IL_096E;
							}
							EV.initWaitFn(new GameObject("UiDangerLevelInitBox").AddComponent<UiEvtNumCounter>().Init(rER.Int(1, 0), rER.Int(2, 0), rER.Int(3, 0), rER.Int(4, -1)), 0);
							return true;
						}
						else
						{
							if (!(cmd == "UIBOX_MONEY_DEACTIVATE"))
							{
								goto IL_096E;
							}
							UiBoxDesigner uiBoxDesigner2 = NEL.getDsFamilyForEvent().Get<UiBoxMoney>();
							if (uiBoxDesigner2 != null)
							{
								uiBoxDesigner2.deactivate();
								return true;
							}
							return true;
						}
					}
					else if (num != 3392875696U)
					{
						if (num != 3660252937U)
						{
							goto IL_096E;
						}
						if (!(cmd == "UICOOKING"))
						{
							goto IL_096E;
						}
						if (NEL.UiAlchemyObject == null)
						{
							return rER.tError("UiAlchemyObject が存在しません。 COOKING_TUTORIAL を実行しておく必要があります");
						}
						NEL.UiAlchemyObject.EvtRead(rER);
						return true;
					}
					else
					{
						if (!(cmd == "UI_MGMTIMER_ACTIVATE"))
						{
							goto IL_096E;
						}
						UiMgmTimer uiMgmTimer = this.GetTimerUi();
						if (uiMgmTimer != null)
						{
							uiMgmTimer.Gob.transform.SetParent(null);
							IN.DestroyOne(uiMgmTimer.Gob);
						}
						uiMgmTimer = new UiMgmTimer(M2DBase.Instance as NelM2DBase, "MGM_TIMER", 0);
						uiMgmTimer.transform.SetParent(EV.Instance.transform, false);
						EV.addListener(uiMgmTimer);
						uiMgmTimer.attach_effect = false;
						uiMgmTimer.activate(rER.IntE(1, 0), rER.IntE(2, 0));
						if (TX.valid(rER._3))
						{
							string text = rER._3;
							bool flag = false;
							if (TX.isStart(rER._3, '!'))
							{
								flag = true;
								text = TX.slice(rER._3, 1);
							}
							uiMgmTimer.gotoEventLabelWhenFinished(text, flag);
							return true;
						}
						return true;
					}
				}
				else
				{
					if (num > 3863891238U)
					{
						if (num <= 4063501937U)
						{
							if (num != 4024793197U)
							{
								if (num != 4063501937U)
								{
									goto IL_096E;
								}
								if (!(cmd == "ALCHEMY_RECIPE_BOOK2"))
								{
									goto IL_096E;
								}
								UiFieldGuide uiFieldGuide = new GameObject("RBK" + IN.totalframe.ToString()).AddComponent<UiFieldGuide>();
								if (TX.valid(rER._1))
								{
									EV.fixObjectPositionTo(uiFieldGuide.transform, rER._1);
								}
								EV.initWaitFn(uiFieldGuide, 0);
								return true;
							}
							else if (!(cmd == "ALCHEMY_TRM_TUTORIAL"))
							{
								goto IL_096E;
							}
						}
						else if (num != 4158024595U)
						{
							if (num != 4237227894U)
							{
								goto IL_096E;
							}
							if (!(cmd == "ALCHEMY_TRM"))
							{
								goto IL_096E;
							}
						}
						else
						{
							if (!(cmd == "UI_MGMTIMER_DEACTIVATE"))
							{
								goto IL_096E;
							}
							UiMgmTimer timerUi2 = this.GetTimerUi();
							if (timerUi2 != null)
							{
								timerUi2.deactivate(false);
								return true;
							}
							return true;
						}
						NEL.defaultEventAlchemyInitialize<UiAlchemyTRM>(rER, rER.cmd == "ALCHEMY_TRM_TUTORIAL", -1);
						return true;
					}
					if (num != 3665994684U)
					{
						if (num != 3802338263U)
						{
							if (num != 3863891238U)
							{
								goto IL_096E;
							}
							if (!(cmd == "<LOAD_DWARF>"))
							{
								goto IL_096E;
							}
							NelDwarfCharManager.reloadScript();
							return true;
						}
						else if (!(cmd == "COOKING"))
						{
							goto IL_096E;
						}
					}
					else
					{
						if (!(cmd == "PVIB"))
						{
							goto IL_096E;
						}
						for (int i = 1; i < rER.clength; i++)
						{
							NEL.PadVib(rER.getIndex(i), 1f);
						}
						return true;
					}
				}
				IL_0497:
				NEL.defaultEventAlchemyInitialize<UiAlchemy>(rER, rER.cmd == "COOKING_TUTORIAL", (rER.cmd == "ALCHEMY") ? 1 : 0);
				return true;
			}
			IL_096E:
			return MgmScoreHolder.EvtRead(ER, rER, skipping);
		}

		private UiMgmTimer GetTimerUi()
		{
			Transform transform = EV.Instance.transform;
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				Transform child = transform.GetChild(i);
				if (child.gameObject.name == "MGM_TIMER")
				{
					UiMgmTimerBehaviour component = child.GetComponent<UiMgmTimerBehaviour>();
					if (component != null)
					{
						return component.Timer;
					}
				}
			}
			return null;
		}

		private static T defaultEventAlchemyInitialize<T>(StringHolder rER, bool is_tutorial = false, int init_tab_index = -1) where T : UiCraftBase
		{
			T t = new GameObject(rER.cmd + "_" + IN.totalframe.ToString()).AddComponent<T>();
			if (TX.valid(rER._2))
			{
				EV.fixObjectPositionTo(t.transform, rER._2);
			}
			ItemStorage[] array = NEL.prepareTemporaryInventory();
			EvImg pic = EV.Pics.getPic(rER._1, true, true);
			UiCraftBase uiCraftBase = t;
			object obj;
			if (array == null)
			{
				obj = null;
			}
			else
			{
				(obj = new ItemStorage[1])[0] = array[0];
			}
			uiCraftBase.InitManager(obj, (array != null) ? array[1] : null, init_tab_index, (pic != null) ? pic.PF : null);
			if (!is_tutorial)
			{
				EV.initWaitFn(t, 0);
			}
			else
			{
				NEL.UiAlchemyObject = t;
				t.initEventManipulate();
			}
			return t;
		}

		public static UiBoxDesignerFamilyEvent getDsFamilyForEvent()
		{
			UiBoxDesignerFamilyEvent uiBoxDesignerFamilyEvent = NEL.Instance.GetComponent<UiBoxDesignerFamilyEvent>();
			if (uiBoxDesignerFamilyEvent == null)
			{
				uiBoxDesignerFamilyEvent = NEL.Instance.gameObject.AddComponent<UiBoxDesignerFamilyEvent>();
				uiBoxDesignerFamilyEvent.auto_deactive_gameobject = false;
				uiBoxDesignerFamilyEvent.base_z = -NEL.Instance.transform.position.z + -4f - 0.2f;
			}
			if (!uiBoxDesignerFamilyEvent.isActive())
			{
				uiBoxDesignerFamilyEvent.activate();
			}
			return uiBoxDesignerFamilyEvent;
		}

		public static void EvtClose()
		{
			NEL.ReleaseUiAlchemyObject();
		}

		public static void ReleaseUiAlchemyObject()
		{
			if (NEL.UiAlchemyObject != null)
			{
				NEL.UiAlchemyObject.deactivate(false);
				NEL.UiAlchemyObject = null;
			}
		}

		public static bool isCraftUiActive()
		{
			return NEL.UiAlchemyObject != null && NEL.UiAlchemyObject.isActive();
		}

		private static ItemStorage[] prepareTemporaryInventory()
		{
			if (M2DBase.Instance != null)
			{
				return null;
			}
			if (NEL.TemporaryInventory == null)
			{
				X.dl("テンポラリインベントリ作成", null, false, false);
				NEL.TemporaryInventory = new ItemStorage[2];
				ItemStorage itemStorage = (NEL.TemporaryInventory[0] = new ItemStorage("Inventory_noel", 99).clearAllItems(99));
				itemStorage.Add(NelItem.Bottle, 3, 0, true, true);
				itemStorage.Add(NelItem.GetById("fruit_apple0", false), 5, 1, true, true);
				itemStorage.Add(NelItem.GetById("fruit_apple0", false), 5, 4, true, true);
				itemStorage.Add(NelItem.GetById("fruit_pine0", false), 4, 3, true, true);
				itemStorage.Add(NelItem.GetById("mtr_bottle0", false), 2, 0, true, true);
				for (int i = 0; i < 3; i++)
				{
					itemStorage.Add(NelItem.GetById("mtr_wheat", false), 4, i, true, true);
					itemStorage.Add(NelItem.GetById("mtr_jelly0", false), 4, i, true, true);
					itemStorage.Add(NelItem.GetById("mtr_weed0", false), 4, i, true, true);
					itemStorage.Add(NelItem.GetById("mtr_lettuce", false), 4, i, true, true);
				}
				itemStorage.Add(NelItem.GetById("mtr_water0", false), 10, 2, true, true);
				itemStorage.fnAddable = (NelItem _Itm, bool ignore_area) => !_Itm.is_precious && !_Itm.is_food;
				itemStorage = (NEL.TemporaryInventory[1] = new ItemStorage("Inventory_precious", 99).clearAllItems(99));
				itemStorage.water_stockable = (itemStorage.infinit_stockable = true);
				itemStorage.Add(NelItem.GetById("precious_noel_cloth", false), 1, 4, true, true);
				itemStorage.Add(NelItem.GetById("precious_noel_shorts", false), 1, 4, true, true);
				itemStorage.Add(NelItem.GetById("Recipe_bread", false), 1, 0, true, true);
				itemStorage.Add(NelItem.GetById("Recipe_sandwich", false), 1, 0, true, true);
				itemStorage.fnAddable = (NelItem _Itm, bool ignore_area) => _Itm.is_precious && !_Itm.is_food;
			}
			return NEL.TemporaryInventory;
		}

		public static void createListenerEval()
		{
			TxEvalListenerContainer txEvalListenerContainer = TX.createListenerEval(NEL.Instance, 2, true);
			txEvalListenerContainer.Add("SER", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				string text = X.Get<string>(Aargs, 0);
				SER ser;
				if (!FEnum<SER>.TryParse(text.ToUpper(), out ser, true))
				{
					X.de("eval error: 不明な ser " + text, null);
					return;
				}
				TX.InputE((float)((int)ser));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SERBIT", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				string text2 = X.Get<string>(Aargs, 0);
				SER ser2;
				if (!FEnum<SER>.TryParse(text2.ToUpper(), out ser2, true))
				{
					X.de("eval error: 不明な ser " + text2, null);
					return;
				}
				TX.InputE((float)(1 << (int)ser2));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("cur_money", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				CoinStorage.CTYPE ctype = CoinStorage.CTYPE.GOLD;
				CoinStorage.CTYPE ctype2;
				if (Aargs.Count > 0 && FEnum<CoinStorage.CTYPE>.TryParse(Aargs[0], out ctype2, true))
				{
					ctype = ctype2;
				}
				TX.InputE(CoinStorage.getCount(ctype));
			}, Array.Empty<string>());
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end)
			{
				UiMgmScore.evQuit();
			}
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2345100994U)
				{
					if (num <= 1504606479U)
					{
						if (num != 812547004U)
						{
							if (num != 1443838714U)
							{
								if (num != 1504606479U)
								{
									goto IL_020C;
								}
								if (!(cmd == "ALCHEMY_COFFEEMAKER"))
								{
									goto IL_020C;
								}
							}
							else if (!(cmd == "ALCHEMY_WORKBENCH"))
							{
								goto IL_020C;
							}
						}
						else if (!(cmd == "ALCHEMY"))
						{
							goto IL_020C;
						}
					}
					else if (num != 1511554586U)
					{
						if (num != 2268747045U)
						{
							if (num != 2345100994U)
							{
								goto IL_020C;
							}
							if (!(cmd == "UIALBUM"))
							{
								goto IL_020C;
							}
							int num2;
							UiAlbum.loadMaterial(out num2);
							return num2;
						}
						else
						{
							if (!(cmd == "UI_RESTROOM_MENU"))
							{
								goto IL_020C;
							}
							EV.Pics.cacheReadFor("whole_general/arrow_r");
							return 0;
						}
					}
					else if (!(cmd == "COOKING_TUTORIAL"))
					{
						goto IL_020C;
					}
				}
				else if (num <= 3802338263U)
				{
					if (num != 2921794231U)
					{
						if (num != 3660252937U)
						{
							if (num != 3802338263U)
							{
								goto IL_020C;
							}
							if (!(cmd == "COOKING"))
							{
								goto IL_020C;
							}
						}
						else
						{
							if (!(cmd == "UICOOKING"))
							{
								goto IL_020C;
							}
							if (rER.clength > 2 && rER._1 == "ARROW_IMG")
							{
								return EV.Pics.cacheReadFor(rER._2);
							}
							return 0;
						}
					}
					else
					{
						if (!(cmd == "PVV"))
						{
							goto IL_020C;
						}
						return 0;
					}
				}
				else if (num != 3863891238U)
				{
					if (num != 4024793197U)
					{
						if (num != 4237227894U)
						{
							goto IL_020C;
						}
						if (!(cmd == "ALCHEMY_TRM"))
						{
							goto IL_020C;
						}
					}
					else if (!(cmd == "ALCHEMY_TRM_TUTORIAL"))
					{
						goto IL_020C;
					}
				}
				else
				{
					if (!(cmd == "<LOAD_DWARF>"))
					{
						goto IL_020C;
					}
					NelDwarfCharManager.loadPxl(false);
					return 0;
				}
				if (rER.clength > 1)
				{
					return EV.Pics.cacheReadFor(rER._1);
				}
				return 0;
			}
			IL_020C:
			return MgmScoreHolder.EvtCacheRead(ER, cmd, rER);
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		public static bool confirmHoldDef(string unique_key, ref int t, int maxt, ButtonSkinNormalNel Skin, bool is_cancel, bool change_lock_flag = false)
		{
			return NEL.confirmHold(unique_key, ref t, maxt, Skin, is_cancel, ((t > 0) ? (is_cancel ? IN.isCancelOn(0) : IN.isSubmitOn(0)) : (is_cancel ? IN.isCancelPD() : IN.isSubmitPD(1))) ? 1 : 0, change_lock_flag);
		}

		public static bool confirmHold(string unique_key, ref int t, int maxt, ButtonSkinNormalNel Skin, bool is_cancel, int fcnt_hold, bool change_lock_flag = false)
		{
			Flagger flagger = null;
			if (!X.DEBUGNOSND)
			{
				flagger = (is_cancel ? NEL.FlgCancelSnd : NEL.FlgSubmitSnd);
			}
			if (t < 0)
			{
				t++;
			}
			else
			{
				aBtn btn = Skin.getBtn();
				if (fcnt_hold > 0)
				{
					if (!btn.isChecked())
					{
						btn.SetChecked(true, true);
						if (change_lock_flag)
						{
							btn.SetLocked(false, true, false);
						}
						if (flagger != null)
						{
							flagger.Add(unique_key);
						}
					}
					t += fcnt_hold;
				}
				else
				{
					if (btn.isChecked())
					{
						btn.SetChecked(false, true);
						if (change_lock_flag)
						{
							btn.SetLocked(true, true, false);
						}
						if (flagger != null)
						{
							flagger.Rem(unique_key);
						}
					}
					t = X.VALWALK(t, 0, 6);
				}
				Skin.hold_level = (float)t / (float)maxt;
			}
			return t >= maxt;
		}

		public static TextRendererRBKeyDesc CreateBottomRightText(string tx_name, float z)
		{
			TextRendererRBKeyDesc textRendererRBKeyDesc = new GameObject(tx_name).AddComponent<TextRendererRBKeyDesc>();
			textRendererRBKeyDesc.gameObject.layer = IN.gui_layer;
			textRendererRBKeyDesc.Col(4293190884U).BorderCol(4282004532U);
			textRendererRBKeyDesc.html_mode = true;
			IN.setZ(textRendererRBKeyDesc.transform, z);
			if (M2DBase.Instance != null)
			{
				M2DBase.Instance.addValotAddition(textRendererRBKeyDesc);
			}
			return textRendererRBKeyDesc;
		}

		public static void CuteFrame(MeshDrawer Md, float x, float y, float w, float h, float title_wdt = -1000f)
		{
			float num = w * 0.5f;
			float num2 = h * 0.5f;
			NEL.CuteLine(Md, -num, -num2, 0f, false);
			NEL.CuteLine(Md, num, -num2, 0f, false);
			NEL.CuteLine(Md, -num, num2, 0f, false);
			NEL.CuteLine(Md, num, num2, 0f, false);
			NEL.CuteLine(Md, 0f, -num2, w - 24f, false);
			NEL.CuteLine(Md, -num, 0f, h - 24f, true);
			NEL.CuteLine(Md, num, 0f, h - 24f, true);
			if (title_wdt == -1000f)
			{
				NEL.CuteLine(Md, 0f, num2, w - 24f, false);
				return;
			}
			if (title_wdt > 0f)
			{
				float num3 = (w - title_wdt - 24f) * 0.5f;
				NEL.CuteLine(Md, -num + 12f, num2, num3, AIM.R);
				NEL.CuteLine(Md, num - 12f, num2, num3, AIM.L);
			}
		}

		public static void CuteLine(MeshDrawer Md, float x, float y, float len, bool is_v = false)
		{
			if (len <= 0f)
			{
				Md.initForImg(MTR.SqNelCuteLine.getImage(0, 0), 0).DrawCen(x, y, null);
				return;
			}
			PxlImage image = MTR.SqNelCuteLine.getImage(1, 0);
			PxlImage image2 = MTR.SqNelCuteLine.getImage(2, 0);
			float num = len * 0.5f;
			if (!is_v)
			{
				Md.initForImg(image, 0);
				Md.RotaGraph(x - num + 1.5f, y, 1f, 0f, null, false);
				Md.RotaGraph(x + num - 1.5f, y, 1f, 0f, null, true);
				Md.initForImg(image2, 0);
				Md.RotaGraph3(x, y, 0.5f, 0.5f, len - 4f, 1f, 0f, null, false);
				return;
			}
			Md.initForImg(image, 0);
			Md.RotaGraph(x, y - num + 1.5f, 1.5707964f, 0f, null, false);
			Md.RotaGraph(x, y + num - 1.5f, -1.5707964f, 0f, null, false);
			Md.initForImg(image2, 0);
			Md.RotaGraph3(x, y, 0.5f, 0.5f, len - 4f, 1f, 1.5707964f, null, false);
		}

		public static void CuteLine(MeshDrawer Md, float x, float y, float len, AIM a)
		{
			float num = (float)CAim._YD(a, 1);
			NEL.CuteLine(Md, x + (float)CAim._XD(a, 1) * len * 0.5f, y + num * len * 0.5f, len, num != 0f);
		}

		public static void CuteDaia(MeshDrawer Md, float x, float y, bool pushed)
		{
			if (pushed)
			{
				Md.KadomaruDaia(x, y, 6.5f, 2.2f, 2f, false, 0f, 0f, false);
				Md.KadomaruDaia(x, y, 3.5f, 1.8333334f, 0f, false, 0f, 0f, false);
				return;
			}
			Md.KadomaruDaia(x, y, 5.5f, 2.2f, 3f, false, 0f, 0f, false);
		}

		public static void QuestNoticeExc(MeshDrawer Md, float x, float y)
		{
			float num = 1f + X.Mx(0f, X.COSIT(100f) - 0.6f) / 0.39999998f * 0.25f;
			if (MTR.NoticeExcPF == null)
			{
				MTR.NoticeExcPF = MTRX.getPF("notice_exc");
			}
			Md.RotaPF(x, y, num, num, 0f, MTR.NoticeExcPF, false, false, false, uint.MaxValue, false, 0);
		}

		public static void drawPointCurs(MeshDrawer Md, float x, float y, float cell_size, float alpha = 1f, NEL.POINT_CURS type = NEL.POINT_CURS.LTRB)
		{
			if (type == NEL.POINT_CURS.LTRB)
			{
				float num = cell_size + 8f + (float)X.IntR(6f * X.Mx(X.Abs(X.COSIT(50f)) - 0.25f, 0f) / 0.75f);
				Md.Col = C32.MulA(uint.MaxValue, alpha);
				for (int i = 0; i < 4; i++)
				{
					Md.RotaPF(x + num * (float)CAim._XD(i, 1), y + num * (float)CAim._YD(i, 1), 1f, 1f, 0f, MTR.MeshCursPoint, false, false, false, uint.MaxValue, false, 0);
				}
				return;
			}
			if (type == NEL.POINT_CURS.SUN_S)
			{
				float num2 = 1f - X.ANMPT(400, 1f);
				float num3 = cell_size * (X.COSIT(100f) * 0.125f + 1f);
				Md.Col = C32.MulA(uint.MaxValue, alpha);
				for (int j = 0; j < 16; j++)
				{
					float num4 = (num2 + (float)j / 16f) * 6.2831855f;
					Md.RotaPF(x + num3 * X.Cos(num4), y + num3 * X.Sin(num4), 1f, 1f, 0f, MTR.MeshCursPoint, false, false, false, uint.MaxValue, false, 0);
				}
				return;
			}
			if (type == NEL.POINT_CURS.SUN || type == NEL.POINT_CURS.SUN_DISABLE)
			{
				Md.initForImg(MTRX.IconWhite, 0);
				float num5 = 1f - X.ANMPT(200, 1f);
				float num6 = cell_size * 0.6f;
				Md.Col = C32.MulA(4285824624U, alpha);
				Matrix4x4 currentMatrix = Md.getCurrentMatrix();
				x *= 0.015625f;
				y *= 0.015625f;
				for (int k = 0; k < 8; k++)
				{
					float num7 = (num5 + (float)k / 8f) * 6.2831855f;
					Md.Rotate(num7, false).Translate(x, y, false);
					if (type == NEL.POINT_CURS.SUN)
					{
						Md.BoxBL(num6 - 1f, -2f, num6 + 3f, 4f, 0f, false);
						Md.Col = C32.MulA(4283826059U, alpha);
						Md.BoxBL(num6, -1f, num6, 2f, 0f, false);
						Md.Col = C32.MulA(4285824624U, alpha);
					}
					else
					{
						Md.BoxBL(num6, -1f, num6, 2f, 0f, false);
					}
					Md.setCurrentMatrix(currentMatrix, false);
				}
			}
		}

		public static VoiceController prepareVoiceController(string key)
		{
			if (NEL.OPreparedVC == null)
			{
				NEL.OPreparedVC = new BDic<string, VoiceController>(2);
			}
			VoiceController voiceController = X.Get<string, VoiceController>(NEL.OPreparedVC, key);
			if (voiceController == null)
			{
				voiceController = (NEL.OPreparedVC[key] = new VoiceController(TX.getResource("Data/voice_" + key, ".csv", false), "cached_" + key, false));
				voiceController.readScript();
			}
			if (voiceController.destructed)
			{
				voiceController.reloadSheet();
			}
			return voiceController;
		}

		public static string error_tag
		{
			get
			{
				return "<font color=\"0x" + C32.codeToCodeText(4290582552U) + "\"><b>";
			}
		}

		public static string error_tag_thin
		{
			get
			{
				return "<font color=\"0x" + C32.codeToCodeText(4290582552U) + "\">";
			}
		}

		public static string error_img
		{
			get
			{
				return "<img mesh=\"alert\" color=\"0x" + C32.codeToCodeText(4290582552U) + "\" width=\"26\" />";
			}
		}

		public static string error_tag_close
		{
			get
			{
				return "</b></font>";
			}
		}

		public static string error_tag_thin_close
		{
			get
			{
				return "</font>";
			}
		}

		public static bool isErrorVib(float af, out float lv, out float x_vib, float x_intv = 14.4f)
		{
			lv = 0f;
			x_vib = 0f;
			if (af < 35f)
			{
				lv = 1f - X.ZPOW(af - 8f, 27f);
				x_vib = (float)X.IntR(X.SINI(af + 11f, 14f) * lv * x_intv);
				return true;
			}
			return false;
		}

		public static void loadDrawing(MeshDrawer Md)
		{
			NEL.loadDrawing(Md, IN.wh - 100f, -IN.hh + 80f, 26, 30f);
		}

		public static void loadDrawingFill(MeshDrawer Md)
		{
			Md.ColGrd.Set(Md.Col).setA(0f);
			Md.BlurPoly2(IN.wh - 100f, -IN.hh + 80f, 50f, 0f, 4, 400f, 90f, MTRX.cola.Set(Md.Col), Md.ColGrd);
		}

		public static void PadVib(string vib_key, float l = 1f)
		{
			if (NEL.Instance.Vib != null)
			{
				NEL.Instance.Vib.Add(vib_key, l);
			}
		}

		public static void loadDrawing(MeshDrawer Md, float dx, float dy, int sl, float daia_w)
		{
			int totalframe = IN.totalframe;
			Md.ColGrd.White();
			for (int i = 0; i < 4; i++)
			{
				int num = (52 + totalframe - 13 * i) % 52;
				float num2 = dx + (float)CAim._XD(i, sl);
				float num3 = dy + (float)CAim._YD(i, sl);
				if (num < 13)
				{
					Md.Col = Md.ColGrd.setA1(1f).C;
					Md.Daia(num2, num3, daia_w, daia_w, false);
				}
				else
				{
					float num4 = X.ZPOW((float)(26 - num), 13f);
					if (num4 >= 0f)
					{
						Md.Col = Md.ColGrd.setA1(num4 * 0.75f).C;
						Md.Daia(num2, num3, daia_w, daia_w, false);
					}
					Md.Col = Md.ColGrd.setA1(1f).C;
					Md.Daia2(num2, num3, daia_w / 2f - 1f, 2f, false);
				}
			}
		}

		public static void loadDrawingRow(MeshDrawer Md, float dx, float dy, uint color, int sl = 26, float daia_w = 30f, float alpha = 1f)
		{
			int totalframe = IN.totalframe;
			Md.ColGrd.Set(color);
			for (int i = 0; i < 4; i++)
			{
				int num = (52 + totalframe - 13 * i) % 52;
				float num2 = dx + (float)(sl * 4) * (-1.5f + (float)i);
				if (num < 13)
				{
					Md.Col = Md.ColGrd.setA1(1f * alpha).C;
					Md.Daia(num2, dy, daia_w, daia_w, false);
				}
				else
				{
					float num3 = X.ZPOW((float)(26 - num), 13f);
					if (num3 >= 0f)
					{
						Md.Col = Md.ColGrd.setA1(num3 * 0.75f * alpha).C;
						Md.Daia(num2, dy, daia_w, daia_w, false);
					}
					Md.Col = Md.ColGrd.setA1(1f * alpha).C;
					Md.Daia2(num2, dy, daia_w / 2f - 1f, 2f, false);
				}
			}
		}

		public static void drawQuestDepertPin(MeshDrawer Md, float dx, float dy, float alpha, float scale = 1f, float shadow_alpha_ratio = 0.4f, float quest_tracking_scale = 0f)
		{
			NEL.drawQuestDepertPinAnim(Md, 5, dx, dy - scale * 4f, alpha, scale, shadow_alpha_ratio, 1f, quest_tracking_scale);
		}

		public static void drawQuestDepertPinAnim(MeshDrawer Md, int af, float dx, float dy, float alpha, float scale = 1f, float shadow_alpha_ratio = 0.5f, float jumping_ratio = 1f, float quest_tracking_scale = 0f)
		{
			af %= 50;
			NEL.drawQuestDepertPinAnimOne(Md, (float)af, dx, dy, alpha, scale, shadow_alpha_ratio, jumping_ratio, quest_tracking_scale);
		}

		public static void drawQuestDepertPinAnimSwitching(MeshDrawer Md, int af, List<QuestTracker.QuestMapInfo> ACol, float dx, float dy, float alpha, float scale = 1f, float shadow_alpha_ratio = 0.5f, float jumping_ratio = 1f, float quest_tracking_scale = 1f, bool force_semi_transp = false)
		{
			if (ACol == null || ACol.Count <= 1)
			{
				bool flag = false;
				bool flag2 = force_semi_transp;
				if (ACol != null && ACol.Count != 0)
				{
					QuestTracker.QuestMapInfo questMapInfo = ACol[0];
					Md.Col = questMapInfo.C;
					flag = questMapInfo.tracking;
					flag2 = flag2 || questMapInfo.semitransp;
				}
				NEL.drawQuestDepertPinAnim(Md, af, dx, dy, alpha * (flag2 ? 0.35f : 1f), scale, shadow_alpha_ratio, jumping_ratio, flag ? quest_tracking_scale : 0f);
				return;
			}
			int count = ACol.Count;
			af %= 50 * count;
			int num = af % 50;
			int num2 = af / 50;
			QuestTracker.QuestMapInfo questMapInfo2;
			bool flag3;
			if (num < 9)
			{
				float num3 = X.ZLINE((float)num, 9f);
				questMapInfo2 = ACol[(num2 == 0) ? (count - 1) : (num2 - 1)];
				Md.Col = questMapInfo2.C;
				flag3 = force_semi_transp || questMapInfo2.semitransp;
				NEL.drawQuestDepertPinAnimOne(Md, (float)num, dx, dy, alpha * (1f - num3) * (flag3 ? 0.35f : 1f), 1f, shadow_alpha_ratio, jumping_ratio, questMapInfo2.tracking ? quest_tracking_scale : 0f);
				questMapInfo2 = ACol[num2];
				Md.Col = questMapInfo2.C;
				flag3 = force_semi_transp || questMapInfo2.semitransp;
				alpha *= num3;
			}
			else
			{
				questMapInfo2 = ACol[num2];
				flag3 = force_semi_transp || questMapInfo2.semitransp;
				Md.Col = questMapInfo2.C;
			}
			NEL.drawQuestDepertPinAnimOne(Md, (float)num, dx, dy, alpha * (flag3 ? 0.35f : 1f), 1f, shadow_alpha_ratio, jumping_ratio, questMapInfo2.tracking ? quest_tracking_scale : 0f);
		}

		public static void BouncyPinZT(out float scalex, out float scaley, out float y_ratio, float x_ext = 0.45f, float y_ext = -0.6f)
		{
			NEL.BouncyPinZ((float)(IN.totalframe % 50), out scalex, out scaley, out y_ratio, x_ext, y_ext);
		}

		public static void BouncyPinZ(float af, out float scalex, out float scaley, out float y_ratio, float x_ext = 0.45f, float y_ext = -0.6f)
		{
			float num = 1f - X.ZLINE(af, 5f) + X.ZLINE(af - 45f, 5f);
			y_ratio = X.ZPOWV(af - 3f, 22f) - X.ZPOW(af - 3f - 22f, 22f);
			scalex = 1f + num * 0.45f;
			scaley = 1f - num * 0.6f;
		}

		private static void drawQuestDepertPinAnimOne(MeshDrawer Md, float af, float dx, float dy, float alpha, float scale = 1f, float shadow_alpha_ratio = 0.5f, float jumping_ratio = 1f, float quest_tracking_scale = 1f)
		{
			float num = af / 50f;
			float num2;
			float num3;
			float num4;
			NEL.BouncyPinZ(af, out num2, out num3, out num4, 0.45f, -0.6f);
			num2 *= scale;
			num3 *= scale;
			float num5 = dy;
			dy += scale * 15f * num4 * jumping_ratio;
			Color32 col = Md.Col;
			float num6 = alpha * (1f - X.ZLINE(num4 - 0.4f, 0.6f) * 0.34f);
			if (shadow_alpha_ratio > 0f)
			{
				float base_x = Md.base_x;
				float base_y = Md.base_y;
				Md.base_x += dx * 0.015625f;
				Md.base_y += num5 * 0.015625f;
				Matrix4x4 currentMatrix = Md.getCurrentMatrix();
				Md.Col = C32.MulA(4283780170U, num6 * X.ZLINE(shadow_alpha_ratio * (1f - num4 * 0.3f)));
				Md.uvRect(Md.base_x - 1f, Md.base_y - 1f, 2f, 2f, MTRX.IconWhite, true, false);
				Md.Scale(1f, 0.5f, false);
				Md.Circle(0f, 0f, scale * (12f - num4 * 5f), 0f, false, 0f, 0f);
				Md.base_x = base_x;
				Md.base_y = base_y;
				Md.setCurrentMatrix(currentMatrix, false);
			}
			Md.Col = C32.MulA(MTRX.ColWhite, num6);
			Md.initForImg(MTR.PFQuestPinBorder.getLayer(0).Img, 0).RotaGraph3(dx, dy, 0.5f, 0f, num2, num3, 0f, null, false);
			Md.Col = C32.MulA(col, num6);
			Md.initForImg(MTR.PFQuestPin.getLayer(0).Img, 0).RotaGraph3(dx, dy + 1f * num3, 0.5f, 0f, num2, num3, 0f, null, false);
			if (quest_tracking_scale > 0f)
			{
				Md.Col = C32.MulA(col, alpha);
				Md.uvRect(dx * 0.015625f + Md.base_x - 2f, num5 * 0.015625f + Md.base_y - 2f, 4f, 4f, MTRX.IconWhite, true, false);
				NEL.drawQuestTrackingCircle(Md, dx, num5, scale * quest_tracking_scale);
			}
			Md.Col = col;
		}

		public static void drawQuestTrackingCircle(MeshDrawer Md, float dx, float dy, float scale = 1f)
		{
			float base_x = Md.base_x;
			float base_y = Md.base_y;
			Md.base_x += dx * 0.015625f;
			Md.base_y += dy * 0.015625f;
			float num = (float)(IN.totalframe % 60);
			float num2 = 44f * scale;
			float num3 = 10f * scale;
			for (int i = 0; i < 2; i++)
			{
				float num4 = ((i == 0) ? X.ZLINE(num, 40f) : X.ZLINE(num - 10f, 50f));
				if (X.BTWS(0f, num4, 1f))
				{
					Md.Poly(0f, 0f, num3 + num2 * X.ZSIN2(num4), 0f, 20, (num4 < 0.05f) ? 0f : ((1f - X.ZSIN(num4)) * 20f * scale), false, 0f, 0f);
				}
			}
			Md.base_x = base_x;
			Md.base_y = base_y;
		}

		public static void FnDrawAdditionalIconEvtCommand(ButtonSkinCommand Sk, MeshDrawer Md)
		{
			if (Sk.flags.IndexOf('Q') >= 0)
			{
				MeshDrawer meshDrawer = Sk.prepareIconMesh();
				Sk.fine_continue_flags |= 31U;
				NEL.QuestNoticeExc(meshDrawer, Sk.swidth * 0.5f - 20f, 0f);
			}
		}

		[SerializeField]
		private bool auto_material_load = true;

		private int t;

		public const float frame_thick = 3f;

		public const float inner_margin = 20f;

		public const float daia_wh = 5.5f;

		public const uint text_color = 4283780170U;

		public const uint text_color_offline = 4285095516U;

		public const uint text_error_color = 4290582552U;

		public const uint text_hidden_color = 4288252036U;

		public const uint focus_color = 4294966715U;

		public const uint text_color_locked = 4288057994U;

		public const uint bg_color_top = 4293321691U;

		public const uint bg_color_marked = 4278246796U;

		public const uint bg_color_bottom = 4291611332U;

		public const uint bg_color_top_offline = 4290689711U;

		public const uint bg_alpha = 3875536895U;

		public const uint bg_alpha_offline = 3724541951U;

		public const uint new_hilight_color = 4294926244U;

		public const uint new_hole_color = 4286477940U;

		public const uint filling_bg_color = 4282004532U;

		public static C32 FillingBgCol = new C32(4282004532U);

		public const uint dark_base_color = 3707764736U;

		public const uint dark_base_color_a1 = 4278190080U;

		public const uint magicslicer_add_color0 = 4294452465U;

		public const uint magicslicer_add_color1 = 4287561866U;

		public const uint bossslicer_add_color0 = 4292677391U;

		public const uint bossslicer_add_color1 = 4288903288U;

		public const uint magicslicer_sub_color0 = 4289853674U;

		public const uint magicslicer_sub_color1 = 4279530364U;

		public const string dll_name = ",Assembly-CSharp";

		public static NEL Instance;

		public PadVibManager Vib;

		private static SndPlayer ConfirmSndSubmit;

		private static Flagger FlgSubmitSnd;

		private static SndPlayer ConfirmSndCancel;

		private static Flagger FlgCancelSnd;

		public const string tag_inclease = "<font color=\"ff:#1335DF\">";

		public const string tag_declease = "<font color=\"ff:#DE148D\">";

		public static bool loaded = false;

		private NelActiveDebugger Nad;

		public const string sf_key_battle_started = "_BATTLE_STARTED";

		private const string NameGobUiForEvent = "__UI_FOR_EVENT";

		private static ItemStorage[] TemporaryInventory;

		private static UiCraftBase UiAlchemyObject;

		private static BDic<string, VoiceController> OPreparedVC;

		private const int pin_ext_maxt = 5;

		private const int pin_maxt = 50;

		public enum POINT_CURS
		{
			LTRB,
			SUN,
			SUN_DISABLE,
			SUN_S
		}
	}
}
