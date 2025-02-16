using System;
using System.Collections.Generic;
using System.IO;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class SceneTitleTemp : MonoBehaviourAutoRun
	{
		public float link_x
		{
			get
			{
				return -IN.wh + 70f;
			}
		}

		public float link_y
		{
			get
			{
				return -IN.hh + 65f;
			}
		}

		private int FIRST_LOGO_DELAY
		{
			get
			{
				if (!this.first_startup)
				{
					return 40;
				}
				return 100;
			}
		}

		public void Awake()
		{
			X.dli("SCENE TITLE AWAKEN ", null);
		}

		private void loadGameContents()
		{
			MTR.initG();
			SceneGame.contents_loaded = true;
		}

		private bool isGameMaterialPrepared
		{
			get
			{
				return MTR.preparedG && EV.material_prepared && NEL.loaded;
			}
		}

		private void changeSceneToGame()
		{
			IN.LoadScene("SceneGameMainBk");
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			TX.releaseBannerIcon();
			X.dl("title scene disposed", null, false, false);
			SVD.releaseAllThumbs();
			SND.unloadSheets(null, "TITLE_LOAD");
			IN.FlgUiUse.Rem("TITLE");
			if (SceneTitleTemp.Instance == this)
			{
				SceneTitleTemp.Instance = null;
			}
			if (this.MdLogo != null)
			{
				this.MdLogo.destruct();
			}
			if (this.Mti != null)
			{
				this.Mti = MTI.ReleaseContainer("MTI_title", "title");
			}
			if (this.need_fine_event_text)
			{
				CFG.refineAllLanguageCache(true);
				this.need_fine_event_text = false;
			}
			if (this.padcheck > SceneTitleTemp.PADCHECK.CHECK)
			{
				MGV.pad_checked = true;
			}
			MTRX.clearStorageListener();
		}

		public void fineTexts()
		{
			this.logo_z = IN.hh * 0.18f;
			this.TxCp.TargetFont = TX.getDefaultFont();
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add("Copyright (c) 2020- ").Add((!TX.isFamilyDefault()) ? "NanameHacha" : "ななめはっちゃー").Add("\n@hinayua_r18 & @HashinoMizuha");
				this.TxCp.Txt(stb);
			}
			if (this.state == SceneTitleTemp.STATE.SENSITIVE_ANNOUNCE)
			{
				this.TxOnePoint.text_content = TX.Get("Title_first_tutorial", "");
				this.TxVer.text_content = TX.Get("Title_Announce_For_Sensitive", "");
			}
			else
			{
				this.TxVer.text_content = NEL.version;
				if ((this.state == SceneTitleTemp.STATE.TOP || this.state == SceneTitleTemp.STATE.ERRORLOG || this.state == SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER) && this.BxTop.isActive())
				{
					BtnContainerRunner btnContainerRunner = this.BxTop.Get("top_categ", false) as BtnContainerRunner;
					btnContainerRunner.BCon.RemakeT<aBtnNel>(this.Atop_btn_keys, "");
					btnContainerRunner.BCon.setValue("-1");
					btnContainerRunner.Get(this.selection_index).Select(false);
					if (SVD.exists_file_count == 0)
					{
						btnContainerRunner.Get(1).SetLocked(true, true, false);
					}
				}
				this.TxOnePoint.text_content = TX.Get("KeyHelp_title_" + this.state.ToString().ToLower(), "");
				this.t_desc = X.Mn(this.t_desc, 0f);
			}
			this.TxOnePoint.TargetFont = TX.getDefaultFont();
			this.TxVer.TargetFont = TX.getDefaultFont();
			if (this.state == SceneTitleTemp.STATE.SENSITIVE_ANNOUNCE || this.state == SceneTitleTemp.STATE.TOP || this.state == SceneTitleTemp.STATE.ERRORLOG || this.state == SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER)
			{
				int num = this.ABtLang.Length;
				for (int i = 0; i < num; i++)
				{
					this.ABtLang[i].getBtn().SetChecked(TX.familyIs(this.ABtLang[i].getBtn().title), true);
				}
			}
		}

		public void remakeLangVar()
		{
			using (BList<string> blist = X.objKeysB<string, TX.TXFamily>(TX.getWholeTextFamilyObject()))
			{
				int count = blist.Count;
				this.ABtLang = new ButtonSkinRowLangNel[count];
				this.DsLang.bgcol = MTRX.ColTrnsp;
				this.DsLang.animate_maxt = 0;
				this.DsLang.add(new DsnDataP("", false)
				{
					text = "<key ltab />",
					TxCol = MTRX.ColWhite,
					size = 16f,
					swidth = 30f,
					sheight = this.DsLang.get_sheight_px(),
					html = true
				});
				for (int i = 0; i < count; i++)
				{
					TX.TXFamily familyByName = TX.getFamilyByName(blist[i]);
					aBtnNel aBtnNel = this.DsLang.addButtonT<aBtnNel>(new DsnDataButton
					{
						title = familyByName.key,
						name = "lang_" + familyByName.key,
						skin = "row_dark_lang",
						skin_title = familyByName.full_name,
						click_snd = "",
						w = 100f,
						h = this.DsLang.get_sheight_px(),
						unselectable = 2,
						def = TX.familyIs(familyByName.key),
						fnClick = new FnBtnBindings(this.fnClickLang)
					});
					IN.setZ(aBtnNel.transform, -0.2f);
					ButtonSkinRowLangNel buttonSkinRowLangNel = (this.ABtLang[i] = aBtnNel.get_Skin() as ButtonSkinRowLangNel);
					buttonSkinRowLangNel.icon_scale = 1f;
					buttonSkinRowLangNel.icon_top = true;
					buttonSkinRowLangNel.ICON_MARGIN = 0;
					buttonSkinRowLangNel.row_left_px = 0;
					buttonSkinRowLangNel.fix_text_size = 10f;
					buttonSkinRowLangNel.addTextureIco(familyByName.getBannerIcon());
					buttonSkinRowLangNel.alignx = ALIGN.CENTER;
					buttonSkinRowLangNel.aligny = ALIGNY.MIDDLE;
				}
			}
			this.DsLang.add(new DsnDataP("", false)
			{
				text = "<key rtab />",
				TxCol = MTRX.ColWhite,
				size = 16f,
				swidth = 30f,
				sheight = this.DsLang.get_sheight_px(),
				html = true
			});
			this.DsLang.bind();
			this.DsLink.bind();
			IN.PosP2(this.DsLang.transform, IN.wh - 38f - (this.DsLang.get_swidth_px() - this.DsLang.use_w) / 2f, -IN.hh - 100f);
			IN.PosP2(this.DsLink.transform, this.link_x - 120f, this.link_y);
		}

		private void initSensitiveAnnounce()
		{
			this.TxOnePoint = IN.CreateGob(base.gameObject, "-tx_op").AddComponent<TextRenderer>();
			this.TxOnePoint.size = 18f;
			this.TxOnePoint.alignx = ALIGN.CENTER;
			this.TxOnePoint.aligny = ALIGNY.MIDDLE;
			this.TxOnePoint.BorderCol(3707764736U);
			this.TxOnePoint.html_mode = true;
			this.TxOnePoint.alpha = 0f;
			IN.PosP2(this.TxOnePoint.transform, 0f, -IN.hh + 130f);
			this.TxCp = IN.CreateGob(base.gameObject, "-tx_cp").AddComponent<TextRenderer>();
			this.TxCp.size = 14f;
			this.TxCp.alignx = ALIGN.LEFT;
			this.TxCp.aligny = ALIGNY.MIDDLE;
			this.TxCp.Col(MTRX.ColWhite);
			this.TxCp.alpha = 0f;
			IN.PosP2(this.TxCp.transform, -IN.wh + 30f, -IN.hh + 39f);
			this.DsLang = IN.CreateGob(base.gameObject, "-lang").AddComponent<Designer>();
			this.DsLang.Smallest();
			this.DsLang.alignx = ALIGN.RIGHT;
			this.DsLang.WH(900f, 50f);
			this.DsLink = IN.CreateGob(base.gameObject, "-link").AddComponent<Designer>();
			this.DsLink.Smallest();
			this.DsLink.bgcol = MTRX.ColTrnsp;
			this.DsLink.item_margin_x_px = 4f;
			this.DsLink.alignx = ALIGN.LEFT;
			this.DsLink.WH((30f + this.DsLink.item_margin_x_px) * 3f - this.DsLink.item_margin_x_px, 40f);
			aBtn aBtn = this.DsLink.addButton(new DsnDataButton
			{
				skin = "mini_link",
				title = TX.Get("discord_url", ""),
				w = 30f,
				h = 30f,
				fnClick = ButtonSkinUrl.fnClick
			});
			MImage mimage = this.Mti.LoadImage("icon_discord");
			(aBtn.get_Skin() as ButtonSkinMiniLink).setTexture(mimage.Tx, mimage.getMtr(BLEND.NORMAL, -1), 0.3f, uint.MaxValue);
			aBtn aBtn2 = this.DsLink.addButton(new DsnDataButton
			{
				skin = "mini_link",
				title = TX.Get("twitter_url", ""),
				w = 30f,
				h = 30f,
				fnClick = ButtonSkinUrl.fnClick
			});
			mimage = this.Mti.LoadImage("icon_twitter");
			(aBtn2.get_Skin() as ButtonSkinMiniLink).setTexture(mimage.Tx, mimage.getMtr(BLEND.NORMAL, -1), 0.3f, uint.MaxValue);
			aBtn aBtn3 = this.DsLink.addButton(new DsnDataButton
			{
				skin = "mini_link",
				title = TX.Get("bilibili_url", ""),
				w = 30f,
				h = 30f,
				fnClick = ButtonSkinUrl.fnClick
			});
			mimage = this.Mti.LoadImage("icon_bilibili");
			(aBtn3.get_Skin() as ButtonSkinMiniLink).setTexture(mimage.Tx, mimage.getMtr(BLEND.NORMAL, -1), 0.51f, uint.MaxValue);
			this.remakeLangVar();
			this.DsLink.alpha = 0f;
			this.DsLang.alpha = 0f;
			this.TxVer = IN.CreateGob(base.gameObject, "-tx_ver").AddComponent<TextRenderer>();
			this.TxVer.size = 20f;
			this.TxVer.alignx = ALIGN.CENTER;
			this.TxVer.aligny = ALIGNY.MIDDLE;
			this.TxVer.Col(MTRX.ColWhite);
			this.TxVer.alpha = 0f;
			this.TxVer.text_content = TX.Get("Title_Announce_For_Sensitive", "");
			IN.PosP2(this.TxVer.transform, 0f, 98f);
			this.DsBlack = IN.CreateGob(base.gameObject, "-first_ask_Designer").AddComponent<Designer>();
			this.DsBlack.animate_maxt = 30;
			this.DsBlack.bgcol = MTRX.ColTrnsp;
			this.DsBlack.Smallest();
			this.DsBlack.margin_in_lr = 20f;
			this.DsBlack.WH(IN.w * 0.75f, 60f);
			IN.PosP(this.DsBlack.transform, 0f, -IN.hh + 190f, -2f);
			this.DsBlack.radius = 200f;
			this.DsBlack.alignx = ALIGN.CENTER;
			this.DsBlack.init();
			this.DsBlack.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				name = "sensitive_ask",
				skin = "normal_dark",
				titles = this.Asensitive_btns,
				w = this.DsBlack.use_w / 2f - 20f,
				h = this.DsBlack.get_sheight_px(),
				clms = 2,
				margin_w = 40f,
				margin_h = 0f,
				default_focus = 1,
				fnClick = delegate(aBtn V)
				{
					X.sensitive_level = ((V.title == "&&btn_sensitive_on") ? 1 : 0);
					this.t = -1f;
					this.DsLang.alpha = 1f;
					this.DsLink.alpha = 1f;
					this.DsBlack.hide();
					return true;
				},
				fnHover = delegate(aBtn V)
				{
					this.selection_index = ((V.title == "&&btn_sensitive_on") ? 1 : 0);
					return true;
				}
			});
			this.selection_index = (X.SENSITIVE ? 1 : 0);
			this.DsBlack.alpha = 0f;
			this.DsBlack.gameObject.SetActive(false);
			this.fineTexts();
			this.t = 0f;
		}

		private void initDsBlackAfter()
		{
			this.DsBlack.stencil_ref = 70;
			this.DsBlack.margin_in_tb = 15f;
			this.DsBlack.bgcol = C32.d2c(2281701376U);
			this.DsBlack.WH(IN.w * 0.52f + 40f, 60f);
			this.DsBlack.Clear();
			this.DsBlack.init();
		}

		private void initBxContainer()
		{
			if (this.BxCon != null)
			{
				return;
			}
			this.BxCon = IN.CreateGob(base.gameObject, "-BxCon").AddComponent<UiBoxDesignerFamily>();
			this.BxCon.auto_deactive_gameobject = false;
			IN.setZ(this.BxCon.transform, -0.125f);
		}

		private void initButtons()
		{
			this.initBxContainer();
			int num = (int)(IN.w * 0.7f);
			this.BxTop = this.BxCon.Create("top", 0f, -IN.hh + 134f, (float)num, 54f, 1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxTop.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.BxTop.anim_time(30);
			this.BxTop.Focusable(false, false, null);
			this.BxTop.use_button_connection = true;
			this.BxTop.margin_in_lr = 20f;
			this.BxTop.margin_in_tb = 12f;
			this.BxTop.item_margin_x_px = 0f;
			this.BxTop.selectable_loop |= 1;
			this.BxTop.btn_height = 30f;
			this.BxCon.setAutoActivate(this.BxTop, false);
			this.BxTop.bkg_scale(true, false, false);
			BtnContainerRadio<aBtn> btnContainerRadio = this.BxTop.addRadioT<aBtnNel>(new DsnDataRadio
			{
				name = "top_categ",
				skin = "row_center",
				keys = this.Atop_btn_keys,
				click_snd = "enter",
				clms = 4,
				w = ((float)num - 40f - 4f) / 4f,
				h = 30f,
				margin_w = 0,
				margin_h = 0,
				navi_loop = 1,
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangedTopCateg),
				fnHover = delegate(aBtn V)
				{
					this.selection_index = X.isinStr(this.Atop_btn_keys, V.title, -1);
					return true;
				},
				def = -1
			});
			btnContainerRadio.APool = new List<aBtn>(4);
			if (SVD.exists_file_count == 0)
			{
				btnContainerRadio.Get(1).SetLocked(true, true, false);
			}
			this.BxR = this.BxCon.Create("right", 0f, 0f, 540f, IN.h - 240f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxR.anim_time(25);
			this.BxR.Focusable(false, false, null);
			this.BxR.use_button_connection = true;
			this.BxR.box_stencil_ref_mask = 225;
			this.BxDesc = this.BxCon.Create("desc", 0f, 0f, 200f, 200f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxDesc.anim_time(18);
			this.BxCon.setAutoActivate(this.BxDesc, false);
			this.BxCon.deactivate(true);
		}

		protected override bool runIRD(float fcnt)
		{
			bool flag = false;
			if (this.state == SceneTitleTemp.STATE.FIRST_LOAD)
			{
				SceneGame.for_debug = false;
				MTRX.init1();
				base.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
				this.Mti = MTI.LoadContainer("MTI_title", "title");
				SND.loadSheets("saveload", "svd");
				this.state = SceneTitleTemp.STATE.FIRST_DARK;
			}
			if (this.state == SceneTitleTemp.STATE.FIRST_DARK)
			{
				if (MTRX.prepared && MTR.prepare1)
				{
					BGM.clearHalfFlag();
					EV.loadEV();
					SceneGame.contents_loaded = true;
					this.state = SceneTitleTemp.STATE.EV_LOAD;
					BGM.load("BGM_title", "grazia", false);
					MTR.initT();
				}
			}
			else if (this.state == SceneTitleTemp.STATE.EV_LOAD)
			{
				if (MTR.preparedT)
				{
					MTR.finePxlLoadSpeed();
					SVD.prepareList(true);
					if (!X.DEBUGNOCFG)
					{
						CFG.loadSdFile(true, true);
						MGV.loadSdFile();
					}
					if (!MGV.pad_checked)
					{
						this.padcheck = SceneTitleTemp.PADCHECK.CHECK;
					}
					MTR.initGPxls(true);
					this.first_startup = X.DEBUGNOCFG || CFG.first_startup;
					this.changeState(SceneTitleTemp.STATE.PXLS_FIRST_LOAD);
				}
			}
			else if (this.state == SceneTitleTemp.STATE.PXLS_FIRST_LOAD)
			{
				this.t += fcnt;
				if (this.t >= 8f)
				{
					NEL.Instance.Vib.clear();
					PxlsLoader.loadSpeed = 0.015625f;
					IN.FlgUiUse.Add("TITLE");
					this.changeState(SceneTitleTemp.STATE.SENSITIVE_ANNOUNCE);
					this.initSensitiveAnnounce();
					this.t = 0f;
					if (!this.first_startup)
					{
						this.initButtons();
					}
				}
			}
			else if (this.state == SceneTitleTemp.STATE.SENSITIVE_ANNOUNCE)
			{
				bool flag2 = !this.first_startup;
				if (!flag2)
				{
					if (this.t >= 0f)
					{
						this.t += fcnt;
						int num = (this.first_startup ? 60 : 4);
						if (this.t >= (float)num)
						{
							if (!this.DsBlack.gameObject.activeSelf)
							{
								this.DsBlack.gameObject.SetActive(true);
								(this.DsBlack.Get("sensitive_ask", false) as BtnContainerRunner).Get(X.SENSITIVE ? 1 : 0).Select(false);
							}
							if (X.D)
							{
								float num2 = X.ZLINE(this.t - (float)num, 30f);
								this.DsBlack.alpha = num2;
								this.TxOnePoint.alpha = (0.6f - 0.4f * X.COSIT(110f)) * num2;
							}
						}
						if (X.D && this.t <= 60f)
						{
							float num3 = X.ZLINE(this.t, 50f);
							this.TxCp.alpha = num3;
							this.TxVer.alpha = num3;
							this.DsLang.alpha = (this.DsLink.alpha = X.ZLINE(num3, 0.5f));
						}
					}
					else if (X.D)
					{
						this.t -= fcnt;
						float num4 = X.ZLINE(24f + this.t, 24f);
						this.TxCp.alpha = num4;
						this.TxVer.alpha = num4;
						this.DsBlack.alpha = num4;
						if (this.TxOnePoint.alpha > 0f)
						{
							this.TxOnePoint.alpha = (0.6f - 0.4f * X.COSIT(110f)) * num4;
						}
						if (num4 <= 0f)
						{
							flag2 = true;
						}
					}
				}
				if (flag2)
				{
					if (this.first_startup)
					{
						this.initButtons();
					}
					this.changeState(SceneTitleTemp.STATE.TOP);
					int exists_file_count = SVD.exists_file_count;
					this.selection_index = ((SVD.exists_file_count != 0) ? 1 : 0);
					BGM.replace(0f, 0f, true, false);
					BGM.setOverrideKey("maintitle");
					this.t = 0f;
					this.t_desc = (float)(-(float)this.FIRST_LOGO_DELAY);
				}
			}
			else if (this.state >= SceneTitleTemp.STATE.TOP && this.state < SceneTitleTemp.STATE.QUIT)
			{
				if (this.state == SceneTitleTemp.STATE.TOP)
				{
					if (!this.BxTop.isActive())
					{
						if (IN.kettei3() || this.t >= (float)this.FIRST_LOGO_DELAY)
						{
							bool debugannounce = X.DEBUGANNOUNCE;
							if (debugannounce && (global::XX.Logger.error_occur_path != null || COOK.error_loaded_index >= 0))
							{
								this.changeState(SceneTitleTemp.STATE.ERRORLOG);
							}
							else if (debugannounce && TX.valid(SND.isSndErrorOccured(false)))
							{
								this.changeState(SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER);
							}
							else
							{
								this.BxTop.activate();
								(this.BxTop.Get("top_categ", false) as BtnContainerRunner).Get(this.selection_index).Select(false);
								this.t_desc = X.Mx(this.t_desc, 0f);
							}
						}
					}
					else if (IN.isCancelPD())
					{
						if (aBtn.isSelecting("&&btn_quit"))
						{
							this.changeState(SceneTitleTemp.STATE.QUIT);
							return true;
						}
						(this.BxTop.Get("top_categ", false) as BtnContainerRunner).Get("&&btn_quit").Select(false);
						SND.Ui.play("cancel", false);
					}
					this.runDebugKeyInput();
				}
				else if (this.state == SceneTitleTemp.STATE.ERRORLOG || this.state == SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER)
				{
					if (IN.isCancelPD())
					{
						if (aBtn.isSelecting("&&Submit"))
						{
							aBtn.PreSelected.ExecuteOnSubmitKey();
							return true;
						}
						(this.BxR.Get("errorlog_bcon", false) as BtnContainerRunner).Get("&&Confirm").Select(false);
						SND.Ui.play("cancel", false);
					}
				}
				else if (this.state == SceneTitleTemp.STATE.CONFIG)
				{
					if (this.EditCfg.ui_state == CFG.STATE.KEYCON)
					{
						this.changeState(SceneTitleTemp.STATE.KEYCON);
					}
					else if (!this.EditCfg.runBoxDesignerEdit())
					{
						if (this.SubmitBtn == null || aBtn.PreSelected == this.SubmitBtn)
						{
							SND.Ui.play("enter", false);
							this.modeSubmit();
						}
						else
						{
							this.SubmitBtn.Select(false);
						}
					}
				}
				else if (this.state == SceneTitleTemp.STATE.SVD_SELECT)
				{
					if (this.EditSvd.ui_state == SVD.STATE.LOAD_SUCCESS)
					{
						COOK.clear(false);
						COOK.setLoadTarget(SVD.GetFile(SVD.last_focused, true), this.EditSvd.ignore_svd_cfg);
						this.changeState(SceneTitleTemp.STATE.START_GAME);
						return true;
					}
					if (!this.EditSvd.runBoxDesignerEdit(false))
					{
						if (this.CancelBtn == null || aBtn.PreSelected == this.CancelBtn)
						{
							SND.Ui.play("cancel", false);
							this.modeCancel();
						}
						else
						{
							this.CancelBtn.Select(false);
							SND.Ui.play("cancel", false);
						}
					}
				}
				else if (this.state == SceneTitleTemp.STATE.KEYCON)
				{
					if (!this.KC.isActive())
					{
						this.changeState(SceneTitleTemp.STATE.CONFIG);
					}
				}
				else if (this.state == SceneTitleTemp.STATE.DIFF_SELECT)
				{
					if (!this.BxDiff.isActive())
					{
						this.changeState(SceneTitleTemp.STATE.TOP);
					}
					else if (this.BxDiff.isDecided())
					{
						COOK.clear(true);
						this.changeState(SceneTitleTemp.STATE.START_GAME);
						return true;
					}
				}
				this.t += fcnt;
				if (this.t >= 10f && this.MdLogo == null)
				{
					this.initTitleLogo();
				}
				if (X.D && this.t >= 10f)
				{
					flag = this.redrawLogo(this.t - 10f, false);
					if (this.t <= 80f)
					{
						float num5 = X.ZLINE(this.t - 10f, 50f);
						this.TxCp.alpha = num5;
						if (!this.first_startup)
						{
							this.DsLang.alpha = (this.DsLink.alpha = num5);
						}
						IN.PosP2(this.TxOnePoint.transform, 0f, -IN.hh + 60f + 30f * X.ZSIN(num5));
					}
				}
				this.t_desc += fcnt;
				if (X.D && this.TxOnePoint.alpha < 1f)
				{
					float num6 = X.ZLINE(this.t_desc, 14f);
					this.TxOnePoint.alpha = num6;
				}
			}
			else if (this.state == SceneTitleTemp.STATE.QUIT || this.state == SceneTitleTemp.STATE.START_GAME)
			{
				this.t += fcnt;
				int num7 = ((this.state == SceneTitleTemp.STATE.QUIT) ? 30 : 52);
				if (this.t >= (float)num7)
				{
					if (this.state == SceneTitleTemp.STATE.QUIT)
					{
						IN.quitGame();
						return true;
					}
					if (this.state == SceneTitleTemp.STATE.START_GAME && this.t >= (float)num7)
					{
						if (this.t == (float)num7)
						{
							this.loadGameContents();
						}
						PxlsLoader.loadSpeed = 2f;
						if (this.t >= (float)(num7 + 4) && this.isGameMaterialPrepared)
						{
							this.changeSceneToGame();
							return true;
						}
						if (X.D)
						{
							NEL.loadDrawing(this.MdLogo);
						}
					}
				}
				if (this.t <= (float)(num7 + 5) && X.D)
				{
					float num8 = 1f - X.ZLINE(this.t + 2f, (float)num7);
					this.TxCp.alpha = num8;
					this.TxVer.alpha = num8;
					this.DsLang.alpha = (this.DsLink.alpha = num8);
					this.DsBlack.alpha = num8;
					flag = this.redrawLogoFilter(num8);
				}
			}
			if (flag)
			{
				this.MdLogo.updateForMeshRenderer(true);
			}
			this.runPadCheck(fcnt, this.padcheck >= SceneTitleTemp.PADCHECK.CHECK && (this.state == SceneTitleTemp.STATE.TOP || this.state == SceneTitleTemp.STATE.ERRORLOG || this.state == SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER || this.state == SceneTitleTemp.STATE.SENSITIVE_ANNOUNCE || this.state == SceneTitleTemp.STATE.CONFIG || this.state == SceneTitleTemp.STATE.SVD_SELECT || this.state == SceneTitleTemp.STATE.DIFF_SELECT));
			if ((this.state == SceneTitleTemp.STATE.SVD_SELECT || this.state == SceneTitleTemp.STATE.CONFIG) && X.D)
			{
				float num9 = X.ZLINE(this.t_desc, 30f);
				this.DsBlack.alpha = num9;
			}
			if (this.state == SceneTitleTemp.STATE.TOP || this.state == SceneTitleTemp.STATE.ERRORLOG || this.state == SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER || this.state == SceneTitleTemp.STATE.SENSITIVE_ANNOUNCE)
			{
				if (IN.isLTabPD())
				{
					this.languageShift(-1);
				}
				if (IN.isRTabPD())
				{
					this.languageShift(1);
				}
				if (this.t_lang < 0f)
				{
					this.t_lang = 0f;
					this.DsLang.bind();
					this.DsLink.bind();
				}
				if (this.t_lang < 35f)
				{
					this.t_lang += fcnt;
					float num10 = X.ZLINE(this.t_lang, 30f);
					IN.PosP2(this.DsLang.transform, IN.wh - 38f - this.DsLang.get_swidth_px() / 2f, -IN.hh - 50f + 90f * X.ZSIN(num10));
					IN.PosP2(this.DsLink.transform, this.link_x - 120f + 120f * X.ZSIN(num10), this.link_y);
				}
			}
			else if (this.DsLang != null)
			{
				if (this.t_lang >= 0f)
				{
					this.t_lang = -1f;
					this.DsLang.hide();
					this.DsLink.hide();
				}
				if (this.t_lang > -35f)
				{
					this.t_lang -= fcnt;
					float num11 = X.ZPOW(30f + this.t_lang, 30f);
					IN.PosP2(this.DsLang.transform, IN.wh - 38f - this.DsLang.get_swidth_px() / 2f, -IN.hh - 50f + 90f * num11);
					IN.PosP2(this.DsLink.transform, this.link_x - 120f + 120f * num11, this.link_y);
				}
			}
			if (this.M2D != null && !this.M2D.loadBasicMaterialProgress(30) && this.state >= SceneTitleTemp.STATE.TOP)
			{
				this.M2D = null;
			}
			return true;
		}

		private void runDebugKeyInput()
		{
		}

		private void languageShift(int d)
		{
			int num = TX.getCurrentFamilyIndex();
			num = X.MMX(0, num + d, TX.countFamilies() - 1);
			this.DsLang.getBtn(num).ExecuteOnClick();
		}

		private void changeState(SceneTitleTemp.STATE _state)
		{
			IN.clearPushDown(true);
			SceneTitleTemp.STATE state = this.state;
			if (state == SceneTitleTemp.STATE.TOP)
			{
				this.BxTop.deactivate();
			}
			if (state == SceneTitleTemp.STATE.CONFIG)
			{
				this.EditCfg.deactivateDesigner();
				if (_state != SceneTitleTemp.STATE.KEYCON)
				{
					this.EditCfg = this.EditCfg.destruct();
				}
				this.BxR.deactivate();
				this.BxDesc.deactivate();
			}
			if (state == SceneTitleTemp.STATE.ERRORLOG)
			{
				this.BxR.deactivate();
				if (global::XX.Logger.error_occur_path != null)
				{
					global::XX.Logger.error_occur_path = null;
					if (_state == SceneTitleTemp.STATE.TOP && TX.valid(SND.isSndErrorOccured(false)))
					{
						_state = SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER;
					}
				}
				COOK.error_loaded_index = -1;
			}
			if (state == SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER)
			{
				this.BxR.deactivate();
				SND.isSndErrorOccured(true);
			}
			if (state == SceneTitleTemp.STATE.SVD_SELECT)
			{
				this.EditSvd.deactivateDesigner();
				this.EditSvd = this.EditSvd.destruct();
				this.BxR.deactivate();
				this.BxDesc.deactivate();
			}
			if (state == SceneTitleTemp.STATE.KEYCON && this.KC != null)
			{
				if (this.KC.result == KeyConDesignerNel.RESULT.SUCCESS)
				{
					this.config_editted = true;
				}
				this.KC.deactivate();
				if (this.KC.padinput_updated == 3)
				{
					this.padcheck = SceneTitleTemp.PADCHECK.NOUSE;
				}
			}
			this.state = _state;
			if (this.state == SceneTitleTemp.STATE.TOP)
			{
				this.BxDesc.deactivate();
				if (this.DsBlack.stencil_ref != 70)
				{
					this.initDsBlackAfter();
				}
				this.DsBlack.hide();
				this.BxR.Focusable(false, false, null);
				BtnContainerRunner btnContainerRunner = this.BxTop.Get("top_categ", false) as BtnContainerRunner;
				if (state != SceneTitleTemp.STATE.SENSITIVE_ANNOUNCE)
				{
					this.BxTop.activate();
					btnContainerRunner.Get(this.selection_index).Select(false);
				}
				else
				{
					this.DsBlack.Clear();
					this.DsBlack.alpha = 0f;
				}
				btnContainerRunner.setValue("-1");
				this.DsBlack.gameObject.SetActive(false);
			}
			if (this.state >= SceneTitleTemp.STATE.TOP)
			{
				this.t_desc = X.Mn(this.t_desc, 0f);
				this.TxOnePoint.text_content = TX.Get("KeyHelp_title_" + this.state.ToString().ToLower(), "");
			}
			if (this.state == SceneTitleTemp.STATE.ERRORLOG)
			{
				this.remakeErrorLog(1);
			}
			if (this.state == SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER)
			{
				this.remakeErrorLog(1);
			}
			if (this.state == SceneTitleTemp.STATE.CONFIG)
			{
				this.BxR.Focusable(true, false, null);
				if (state != SceneTitleTemp.STATE.KEYCON)
				{
					this.remakeSumitCancelButton(true, true, 0);
					this.BxR.Clear();
					this.BxR.WH(630f, IN.h - 240f);
					this.BxR.activate();
					this.BxR.positionD(-190f, 90f, 0, 50f);
					this.BxR.box_stencil_ref_mask = -1;
					this.BxR.margin_in_tb = 11f;
					this.BxR.margin_in_lr = 28f;
					this.BxR.use_scroll = true;
					this.BxR.item_margin_x_px = 14f;
					this.BxR.item_margin_y_px = 18f;
					this.BxR.alignx = ALIGN.CENTER;
					this.EditCfg = new CFG(this.BxR, this.BxDesc, this.DsBlack, true, false, null);
					this.DsBlack.gameObject.SetActive(true);
					this.DsBlack.activate();
				}
				else
				{
					this.BxR.activate();
					this.DsBlack.activate();
					this.EditCfg.resume();
				}
				this.BxR.Focus();
			}
			if (this.state == SceneTitleTemp.STATE.SVD_SELECT)
			{
				this.BxR.Focusable(true, false, null);
				this.remakeSumitCancelButton(false, true, -320);
				this.BxR.Clear();
				this.BxR.WH(630f, IN.h - 230f);
				this.BxR.activate();
				this.BxR.positionD(170f, 90f, 0, 50f);
				this.BxR.box_stencil_ref_mask = -1;
				this.BxR.use_scroll = false;
				this.EditSvd = new SVD(this.BxR, this.BxDesc, (int)MGV.last_saved, this.DsBlack, true, true, this.config_editted);
				this.BxR.Focus();
				this.DsBlack.gameObject.SetActive(true);
				this.DsBlack.activate();
			}
			if (this.state == SceneTitleTemp.STATE.KEYCON)
			{
				if (this.KC != null)
				{
					Object.Destroy(this.KC);
				}
				this.KC = new GameObject("KC").AddComponent<KeyConDesignerNel>();
				this.KC.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
				IN.setZ(this.KC.transform, -4.25f);
				this.KC.activate();
				this.BxCon.deactivate(false);
			}
			if (this.state == SceneTitleTemp.STATE.DIFF_SELECT)
			{
				if (this.BxDiff != null)
				{
					this.BxDiff.destruct(true);
				}
				this.BxDiff = new UiTitleDifficultyConfirm(null, -4.25f, this.MIdifficulty);
			}
			if (this.state == SceneTitleTemp.STATE.QUIT || this.state == SceneTitleTemp.STATE.START_GAME)
			{
				this.t = 0f;
				this.DsBlack.deactivate();
				if (this.state == SceneTitleTemp.STATE.QUIT && !this.first_startup)
				{
					bool flag = this.config_editted;
				}
				if (this.state == SceneTitleTemp.STATE.START_GAME)
				{
					PxlsLoader.loadSpeed = 2f;
					BGM.setOverrideKey("_");
					BGM.stop(false, false);
					SND.Ui.play("sd_initialize", false);
				}
			}
		}

		private void remakeErrorLog(int first_select = 1)
		{
			this.BxR.Focusable(false, false, null);
			this.BxR.Clear();
			float num = IN.h - 240f;
			float num2 = 0f;
			if (global::XX.Logger.loaded_file_error_flag != global::XX.Logger.ERRORFLAG.NONE)
			{
				num2 = 70f;
			}
			num += num2;
			this.BxR.WH(880f, num);
			this.BxR.activate();
			this.BxR.positionD(0f, 40f, 3, 50f);
			this.BxR.margin_in_tb = 30f;
			this.BxR.margin_in_lr = 60f;
			this.BxR.use_scroll = false;
			this.BxR.alignx = ALIGN.CENTER;
			this.BxR.init();
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.state == SceneTitleTemp.STATE.ERRORLOG)
				{
					if (global::XX.Logger.error_occur_path != null)
					{
						stb.AddTxA("Title_error_announce", false);
						stb.Add("\n").AddTxA("Title_error_announce_filepath", false);
						using (STB stb2 = TX.PopBld(null, 0))
						{
							stb2.AddPath(global::XX.Logger.error_occur_path, 0.5f);
							stb.TxRpl(stb2);
						}
						stb.Ret("\n").Ret("\n");
						if ((global::XX.Logger.loaded_file_error_flag & global::XX.Logger.ERRORFLAG.MISSING_METHOD) != global::XX.Logger.ERRORFLAG.NONE)
						{
							stb.Add(NEL.error_tag_thin, "<font size=\"110%\">");
							stb.AddTxA("Title_error_announce_broken_dll", false);
							stb.TxRpl(IN.getApplicationName("AliceInCradle"));
							stb.Add("</font>", NEL.error_tag_thin_close);
							goto IL_0250;
						}
						stb.AddTxA("Title_error_announce_discord", false);
						goto IL_0250;
					}
					else
					{
						if (COOK.error_loaded_index < 0)
						{
							goto IL_0250;
						}
						stb.Add(NEL.error_tag);
						stb.AddTxA("SVD_Alert_load_failure", false).Add(NEL.error_tag_close).Ret("\n");
						stb.AddTxA("Title_error_announce_filepath", false);
						using (STB stb3 = TX.PopBld(null, 0))
						{
							stb3.AddPath(Path.Combine(SVD.getDir(), SVD.getFileName((int)COOK.error_loaded_index)), 0.5f);
							stb.TxRpl(stb3);
							goto IL_0250;
						}
					}
				}
				stb.AddTxA("Title_error_no_sound_driver", false);
				stb.TxRpl(NEL.error_tag + SND.isSndErrorOccured(false) + NEL.error_tag_close);
				IL_0250:
				this.BxR.addP(new DsnDataP("", false)
				{
					Stb = stb,
					alignx = ALIGN.CENTER,
					swidth = this.BxR.use_w,
					text_auto_wrap = true,
					size = 18f,
					TxCol = C32.d2c(4283780170U),
					html = true,
					sheight = 260f + num2
				}, false);
			}
			this.BxR.Br();
			SceneTitleTemp.addDiscordLink(this.BxR);
			this.BxR.Br();
			this.BxR.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				name = "errorlog_bcon",
				titles = new string[] { "&&Title_Show_Log", "&&Confirm" },
				margin_w = 30f,
				w = 360f,
				h = 38f,
				fnClick = new FnBtnBindings(this.fnClickErrorLog)
			}).Get(first_select).Select(false);
		}

		public static void addDiscordLink(UiBoxDesigner BxR)
		{
			BxR.P(TX.Get("discord_url_is_here", ""), ALIGN.RIGHT, 240f, false, 32f, "").XSh(40f);
			BxR.addButton(new DsnDataButton
			{
				skin = "url",
				title = TX.Get("discord_url", ""),
				w = BxR.use_w - 40f,
				h = 32f,
				fnClick = ButtonSkinUrl.fnClick
			});
		}

		private void remakeSumitCancelButton(bool use_submit, bool use_cancel, int shiftx_px = 0)
		{
			BtnContainer<aBtn> btnContainer = SceneTitleTemp.remakeSumitCancelButtonS(this.DsBlack, delegate(aBtn V)
			{
				if (V.title == "&&Submit")
				{
					this.modeSubmit();
				}
				if (V.title == "&&Cancel")
				{
					this.modeCancel();
				}
				return true;
			}, use_submit, use_cancel, 0);
			this.CancelBtn = null;
			this.SubmitBtn = btnContainer.Get("&&Submit");
			if (use_cancel)
			{
				this.CancelBtn = btnContainer.Get("&&Cancel");
			}
			if (this.SubmitBtn == null)
			{
				this.SubmitBtn = this.CancelBtn;
			}
		}

		public static BtnContainer<aBtn> remakeSumitCancelButtonS(Designer Ds, FnBtnBindings BtnClick, bool use_submit, bool use_cancel, int shiftx_px = 0)
		{
			Ds.Clear();
			IN.PosP2(Ds.transform, (float)shiftx_px, -IN.hh + 140f);
			string[] array;
			if (use_submit && use_cancel)
			{
				array = new string[] { "&&Submit", "&&Cancel" };
			}
			else if (use_submit)
			{
				array = new string[] { "&&Submit" };
			}
			else
			{
				array = new string[] { "&&Cancel" };
			}
			Ds.alignx = ALIGN.CENTER;
			Ds.init();
			return Ds.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				name = "submittion",
				skin = "normal_dark",
				titles = array,
				w = IN.w * 0.4f * 0.5f - 20f,
				h = 30f,
				clms = array.Length,
				margin_w = 40f,
				margin_h = 0f,
				fnClick = BtnClick
			});
		}

		private void modeSubmit()
		{
			if (this.state == SceneTitleTemp.STATE.CONFIG)
			{
				this.EditCfg.submitData();
				this.changeState(SceneTitleTemp.STATE.TOP);
				this.config_editted = true;
				this.fineTexts();
				this.need_fine_event_text = true;
			}
		}

		private void modeCancel()
		{
			SND.Ui.play("cancel", false);
			SceneTitleTemp.STATE state = this.state;
			if (state == SceneTitleTemp.STATE.CONFIG)
			{
				this.EditCfg.revertData();
				this.changeState(SceneTitleTemp.STATE.TOP);
				return;
			}
			if (state != SceneTitleTemp.STATE.SVD_SELECT)
			{
				return;
			}
			this.changeState(SceneTitleTemp.STATE.TOP);
		}

		private bool fnChangedTopCateg(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (cur_value < 0)
			{
				return true;
			}
			string title = _B.Get(cur_value).title;
			if (title != null)
			{
				if (!(title == "&&btn_new_game"))
				{
					if (!(title == "&&btn_continue"))
					{
						if (!(title == "&&btn_option"))
						{
							if (title == "&&btn_quit")
							{
								this.changeState(SceneTitleTemp.STATE.QUIT);
							}
						}
						else
						{
							this.changeState(SceneTitleTemp.STATE.CONFIG);
						}
					}
					else
					{
						this.changeState(SceneTitleTemp.STATE.SVD_SELECT);
					}
				}
				else
				{
					this.changeState(SceneTitleTemp.STATE.DIFF_SELECT);
				}
			}
			return true;
		}

		private bool fnClickErrorLog(aBtn B)
		{
			string title = B.title;
			if (title != null && title == "&&Title_Show_Log")
			{
				if (global::XX.Logger.error_occur_path != null)
				{
					NKT.openInExplorer(global::XX.Logger.error_occur_path);
				}
				else if (COOK.error_loaded_index >= 0)
				{
					NKT.openInExplorer(Path.Combine(SVD.getDir(), SVD.getFileName((int)COOK.error_loaded_index)));
				}
			}
			else
			{
				this.changeState(SceneTitleTemp.STATE.TOP);
			}
			return true;
		}

		private bool fnClickLang(aBtn B)
		{
			string text = this.DsLang.getName(B);
			if (!TX.valid(text) || !TX.isStart(text, "lang_", 0))
			{
				SND.Ui.play("tool_spoit_quit", false);
				return true;
			}
			text = TX.slice(text, 5);
			if (TX.familyIs(text))
			{
				SND.Ui.play("tool_spoit_quit", false);
				return true;
			}
			SND.Ui.play("tool_chip_select", false);
			TX.changeFamily(text);
			if (this.state == SceneTitleTemp.STATE.SENSITIVE_ANNOUNCE && this.DsBlack != null && this.DsBlack.isActive())
			{
				BtnContainer<aBtn> bcon = (this.DsBlack.Get("sensitive_ask", false) as BtnContainerRunner).BCon;
				bcon.RemakeT<aBtnNel>(this.Asensitive_btns, "");
				bcon.Get(this.selection_index).Select(false);
			}
			if (this.state == SceneTitleTemp.STATE.ERRORLOG || this.state == SceneTitleTemp.STATE.ERRORLOG_SNDDRIVER)
			{
				this.remakeErrorLog((aBtn.PreSelected != null && aBtn.PreSelected.title == "&&Title_Show_Log") ? 0 : 1);
			}
			this.fineTexts();
			this.need_fine_event_text = true;
			return true;
		}

		private void initTitleLogo()
		{
			this.MIlogo = this.Mti.LoadImage("title_logo");
			this.MIbg = this.Mti.LoadImage("key_bg");
			this.MIpic = this.Mti.LoadImage("key_noel");
			this.MIdifficulty = this.Mti.LoadImage("difficulty");
			this.ClsDr = new ClosingEyeDrawer();
			this.ClsDr.bounds_hh = IN.hh * 2.5f * 0.015625f;
			float num = 0.1f;
			this.MdLogo = MeshDrawer.prepareMeshRenderer(base.gameObject, this.MIlogo.getMtr(BLEND.NORMAL, -1), 0.001f, -1, null, false, false);
			this.MaLogo = new MdArranger(this.MdLogo);
			this.MdLogo.chooseSubMesh(0, false, true);
			this.MdLogo.base_z = num - 0f;
			this.MdLogo.setMaterial(this.MIbg.getMtr(BLEND.NORMAL, -1), false);
			this.MdLogo.chooseSubMesh(1, false, true);
			this.MdLogo.base_z = num - 0.01f;
			this.MdLogo.setMaterial(this.MIpic.getMtr(BLEND.NORMAL, -1), false);
			this.MdLogo.chooseSubMesh(4, false, true);
			this.MdLogo.base_z = num - 0.04f;
			this.MdLogo.setMaterial(this.MIlogo.getMtr(BLEND.NORMAL, -1), false);
			this.MdLogo.chooseSubMesh(2, false, true);
			this.MdLogo.base_z = num - 0.02f;
			this.MdLogo.setMaterial(MTRX.MtrMeshAdd, false);
			this.MdLogo.chooseSubMesh(3, false, true);
			this.MdLogo.base_z = num - 0.03f;
			this.MdLogo.setMaterial(MTRX.MtrMeshNormal, false);
			this.MdLogo.connectRendererToTriMulti(base.gameObject.GetComponent<MeshRenderer>());
			this.TxVer.size = 14f;
			this.TxVer.html_mode = true;
			this.TxVer.alignx = ALIGN.RIGHT;
			this.TxVer.aligny = ALIGNY.BOTTOM;
			this.TxVer.alpha = 0f;
			this.TxVer.text_content = NEL.version;
		}

		private bool redrawLogo(float t, bool force = false)
		{
			float num = X.ZSIN(t, 120f);
			float num2 = X.ZSIN(t - 30f, 80f);
			if (!force && t >= 124f)
			{
				return false;
			}
			this.MdLogo.clear(false, false);
			this.MdLogo.chooseSubMesh(0, false, true).initForImg(this.MIbg.Tx);
			this.MdLogo.DrawCen(0f, -X.NI(-90f, 90f, num), null);
			this.MdLogo.chooseSubMesh(1, false, true).initForImg(this.MIpic.Tx);
			this.MdLogo.DrawCen(420f, -X.NI(-250f, 160f, num), null);
			float num3 = X.ZCOS(t - 10f, 50f);
			if (num3 < 1f)
			{
				this.MdLogo.chooseSubMesh(2, false, true);
				this.MdLogo.Col = C32.MulA(4287730065U, (1f - num3) * 0.7f);
				this.MdLogo.Rect(0f, 0f, IN.w + 20f, IN.h + 20f, false);
			}
			float num4 = X.ZPOW(t - 10f, 22f);
			this.MdLogo.chooseSubMesh(3, false, true);
			this.MdLogo.Col = this.MdLogo.ColGrd.Set(4282004532U).mulA(1f - 0.5f * num4).C;
			this.ClsDr.drawTo(this.MdLogo, 0f, 0f, IN.w * 1.2f, IN.h * 1.5f, 1f - num4 * 0.7f);
			this.MdLogo.chooseSubMesh(4, false, true).initForImg(this.MIlogo.Tx);
			this.MaLogo.SetWhole(false);
			if (num2 > 0f)
			{
				this.MdLogo.Col = this.MdLogo.ColGrd.White().mulA(num2).C;
				this.TxVer.alpha = num2;
				float num5 = this.logo_z * num2;
				this.MdLogo.DrawCen(0f, num5, null);
				IN.PosP2(this.TxVer.transform, 342f, -139f + num5);
			}
			return true;
		}

		private bool redrawLogoFilter(float tz)
		{
			if (this.MaFilter == null)
			{
				this.MaFilter = new MdArranger(this.MdLogo);
				this.redrawLogo(1000f, true);
				this.MdLogo.chooseSubMesh(3, false, false);
				this.MaFilter.SetWhole(false);
			}
			this.MdLogo.chooseSubMesh(4, false, false).initForImg(this.MIlogo.Tx);
			this.MaLogo.revertVerAndTriIndexSaved(false);
			this.MdLogo.Col = this.MdLogo.ColGrd.White().mulA(tz).C;
			this.MdLogo.DrawCen(0f, this.logo_z, null);
			this.MdLogo.chooseSubMesh(3, false, false);
			this.MaFilter.revertVerAndTriIndexSaved(false);
			this.MdLogo.Col = this.MdLogo.ColGrd.Set(4282004532U).mulA(1f - tz).C;
			this.MdLogo.Rect(0f, 0f, IN.w + 20f, IN.h + 20f, false);
			return true;
		}

		private void runPadCheck(float fcnt, bool can_init)
		{
			float num = -IN.hh + 100f;
			if (can_init)
			{
				bool flag = false;
				if (this.padcheck == SceneTitleTemp.PADCHECK.CHECK)
				{
					string currentControlScheme = IN.getCurrentKeyAssignObject().PlayerCon.currentControlScheme;
					if (currentControlScheme == "Gamepad" || currentControlScheme == "HID")
					{
						flag = true;
						this.padcheck = ((IN.connected_hid_device != null) ? SceneTitleTemp.PADCHECK.DIRECT : SceneTitleTemp.PADCHECK.XINPUT);
					}
				}
				if (this.padcheck > SceneTitleTemp.PADCHECK.CHECK)
				{
					SceneTitleTemp.PADCHECK padcheck = ((IN.connected_hid_device != null) ? SceneTitleTemp.PADCHECK.DIRECT : SceneTitleTemp.PADCHECK.XINPUT);
					if (padcheck != this.padcheck)
					{
						this.padcheck = padcheck;
						flag = true;
					}
					if (flag)
					{
						this.t_padchk = 0f;
						if (this.BxPadChk == null)
						{
							this.initBxContainer();
							this.BxPadChk = this.BxCon.Create("padchk", -IN.wh + 220f - 38f, num, 220f, 38f, 0, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
							this.BxCon.setAutoActivate(this.BxPadChk, false);
							this.BxPadChk.getBox().frametype = UiBox.FRAMETYPE.DARK_SIMPLE;
							this.BxPadChk.Small();
							this.BxPadChk.margin_in_lr = 16f;
							this.BxPadChk.init();
							this.BxPadChk.addP(new DsnDataP("", false)
							{
								name = "padp",
								TxCol = MTRX.ColWhite,
								size = 16f,
								html = true,
								text = " ",
								swidth = this.BxPadChk.swidth,
								sheight = this.BxPadChk.sheight,
								alignx = ALIGN.LEFT
							}, false);
						}
						this.BxPadChk.activate();
						this.BxPadChk.Get("padp", false).setValue(TX.Get((this.padcheck == SceneTitleTemp.PADCHECK.DIRECT) ? "PadCheck_DirectInput" : "PadCheck_XInput", ""));
						this.BxPadChk.posSetDA(-IN.wh + 110f - 19f, num, 2, 60f, true);
						KEY currentKeyAssignObject = IN.getCurrentKeyAssignObject();
						currentKeyAssignObject.is_xinput = this.padcheck == SceneTitleTemp.PADCHECK.XINPUT;
						currentKeyAssignObject.clearKEY(false, true);
						currentKeyAssignObject.clearPushDown(true);
					}
				}
			}
			if (this.t_padchk >= 0f && this.t_padchk < 150f)
			{
				this.t_padchk += fcnt;
				if (this.t_padchk >= 150f || !can_init)
				{
					this.t_padchk = 150f;
					this.BxPadChk.position(-IN.wh - 110f, num, -1000f, -1000f, false);
				}
			}
		}

		private const bool no_picture = false;

		private MeshDrawer MdLogo;

		private MdArranger MaLogo;

		private MdArranger MaFilter;

		private MTI Mti;

		private MImage MIlogo;

		private MImage MIpic;

		private MImage MIbg;

		private MImage MIdifficulty;

		private ClosingEyeDrawer ClsDr;

		private SceneTitleTemp.STATE state;

		private float t;

		private bool config_editted;

		private int selection_index;

		private float t_desc = -30f;

		private float t_lang = 30f;

		private float t_padchk = -1f;

		private UiBoxDesignerFamily BxCon;

		private UiBoxDesigner BxTop;

		private UiBoxDesigner BxR;

		private UiBoxDesigner BxDesc;

		private UiBoxDesigner BxPadChk;

		private TextRenderer TxCp;

		private TextRenderer TxVer;

		private TextRenderer TxOnePoint;

		private UiTitleDifficultyConfirm BxDiff;

		private CFG EditCfg;

		private SVD EditSvd;

		private bool first_startup;

		private KeyConDesignerNel KC;

		public static SceneTitleTemp Instance;

		private bool need_fine_event_text;

		private aBtn CancelBtn;

		private aBtn SubmitBtn;

		private Designer DsBlack;

		private Designer DsLang;

		private Designer DsLink;

		private ButtonSkinRowLangNel[] ABtLang;

		private const float btn_height = 30f;

		private const float top_marg_y = 12f;

		private const float top_marg_x = 20f;

		private const int MAXT_PADCHK_T = 150;

		private const int PADCHK_FADE_T = 22;

		private M2DBase M2D;

		private float logo_z;

		private SceneTitleTemp.PADCHECK padcheck;

		private readonly string[] Asensitive_btns = new string[] { "&&btn_sensitive_off", "&&btn_sensitive_on" };

		private readonly string[] Atop_btn_keys = new string[] { "&&btn_new_game", "&&btn_continue", "&&btn_option", "&&btn_quit" };

		public enum PADCHECK
		{
			NOUSE,
			CHECK,
			XINPUT,
			DIRECT
		}

		private enum MESH_L
		{
			BG,
			PIC,
			ADD,
			EYE,
			LOGO
		}

		private enum STATE
		{
			FIRST_LOAD,
			FIRST_DARK,
			EV_LOAD,
			PXLS_FIRST_LOAD,
			SENSITIVE_ANNOUNCE,
			TOP,
			ERRORLOG,
			ERRORLOG_SNDDRIVER,
			CONFIG,
			KEYCON,
			SVD_SELECT,
			DIFF_SELECT,
			QUIT,
			START_GAME
		}
	}
}
