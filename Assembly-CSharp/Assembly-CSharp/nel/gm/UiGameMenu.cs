using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel.gm
{
	public sealed class UiGameMenu : UiBoxDesignerFamily
	{
		internal float extend_right_w
		{
			get
			{
				return this.bounds_w - 20f;
			}
		}

		public float bounds_h
		{
			get
			{
				return IN.h - 130f;
			}
		}

		public float right_box_center_x
		{
			get
			{
				return this.bounds_wh - this.right_w / 2f;
			}
		}

		internal Flagger FlgStatusHide
		{
			get
			{
				return UIStatus.FlgStatusHide;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.ItemMng = new UiItemManageBoxSlider(this.M2D.IMNG, base.transform);
			this.ItemMngMv = new UiItemManageBoxSlider(this.M2D.IMNG, base.transform);
			this.bounds_wh = this.bounds_w / 2f;
			this.right_w = this.bounds_w - 190f - 20f;
			float num = (-IN.wh + this.bounds_wh + 20f) * 0.015625f;
			this.auto_deactive_gameobject = false;
			this.AGmcCache = new UiGMC[11];
			IN.Pos(base.transform, num, 0f, -4.125f);
			this.BxCategory = base.Create("category", 0f, 0f, 190f, 485f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxCategory.RowBtnMode(38f).DelayT(this.BXR_DELAYT);
			this.BxCategory.anim_time(22);
			this.BxCategory.Focusable(true, true, null);
			this.BxR = base.Create("right", 0f, 0f, this.right_w, this.bounds_h, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxR.anim_time(22);
			this.BxR.Focusable(true, true, null);
			this.BxR.use_button_connection = true;
			this.bxr_stencil_default = this.BxR.box_stencil_ref_mask;
			this.BxItmv = base.Create("itmv", 0f, 0f, this.right_w, this.bounds_h, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxItmv.anim_time(22);
			this.BxItmv.Focusable(true, false, null);
			this.BxItmv.use_button_connection = true;
			base.setAutoActivate(this.BxItmv, false);
			this.BxDesc = base.Create("desc", 0f, 0f, 200f, 200f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxDesc.anim_time(14);
			this.BxDesc.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.BxDesc.box_stencil_ref_mask = 230;
			this.BxDesc.alignx = ALIGN.CENTER;
			IN.setZ(this.BxDesc.transform, -0.58f);
			base.setAutoActivate(this.BxDesc, false);
			this.BxCmd = base.Create("cmd", 0f, 0f, 200f, 200f, 0, 0f, UiBoxDesignerFamily.MASKTYPE.BOX);
			IN.setZ(this.BxCmd.transform, -0.65f);
			this.BxCmd.anim_time(14);
			this.BxCmd.margin_in_lr = 38f;
			this.BxCmd.margin_in_tb = 31f;
			this.BxCmd.item_margin_y_px = 0f;
			this.BxCmd.Focusable(true, true, null);
			base.setAutoActivate(this.BxCmd, false);
			this.remakeLeftCategories(false);
			this.TopTab = IN.CreateGob(base.gameObject, "-toptab").AddComponent<UiGameMenuTopTab>();
			this.TopTab.initializeGM(this);
			this.BtmTab = IN.CreateGob(base.gameObject, "-btmtab").AddComponent<UiGameMenuTopTab>();
			this.BtmTab.initializeGM(this);
			this.deactivate(true);
		}

		internal void remakeLeftCategories(bool remaking = true)
		{
			if (remaking)
			{
				this.BxCategory.Clear();
				this.BxCategory.init();
			}
			for (int i = 0; i < 10; i++)
			{
				Designer bxCategory = this.BxCategory;
				DsnDataButton dsnDataButton = new DsnDataButton();
				dsnDataButton.name = "categ_" + i.ToString();
				dsnDataButton.title = "categ_" + i.ToString();
				dsnDataButton.skin = "ui_category";
				string text = "GameMenu_Category_Title_";
				CATEG categ = (CATEG)i;
				dsnDataButton.skin_title = TX.Get(text + categ.ToString().ToLower(), "");
				dsnDataButton.w = this.BxCategory.use_w;
				dsnDataButton.h = (this.BxCategory.h - this.BxCategory.margin_in_tb) / 10f - 8f;
				dsnDataButton.hover_to_select = true;
				dsnDataButton.navi_auto_fill = false;
				dsnDataButton.fnHover = new FnBtnBindings(this.fnHoverCategory);
				dsnDataButton.fnOut = new FnBtnBindings(this.fnOutCategory);
				dsnDataButton.fnClick = new FnBtnBindings(this.fnClickCategory);
				aBtn aBtn = bxCategory.addButton(dsnDataButton);
				ButtonSkinNelUi buttonSkinNelUi = aBtn.get_Skin() as ButtonSkinNelUi;
				if (i == 7)
				{
					this.BenchSkin = buttonSkinNelUi;
				}
				if (this.waiting_categ_for_ == (CATEG)i)
				{
					buttonSkinNelUi.hilighted = true;
				}
				aBtn.hover_snd = "";
				this.BxCategory.Br();
			}
			this.ui_lang = (byte)TX.getCurrentFamilyIndex();
			this.refineLeftCategoriesHilight();
		}

		internal void refineLeftCategoriesHilight()
		{
			(this.BxCategory.getBtn("categ_" + 6.ToString()).get_Skin() as ButtonSkinNelUi).show_new_icon = (this.M2D.QUEST.hasNewProg(QuestTracker.CATEG._ALL) ? 1 : 0);
		}

		internal CATEG waiting_categ_for
		{
			get
			{
				return this.waiting_categ_for_;
			}
			set
			{
				if (value != this.waiting_categ_for_)
				{
					if (this.waiting_categ_for_ != CATEG._NOUSE)
					{
						Designer bxCategory = this.BxCategory;
						string text = "categ_";
						int num = (int)this.waiting_categ_for_;
						aBtn btn = bxCategory.getBtn(text + num.ToString());
						if (btn != null)
						{
							(btn.get_Skin() as ButtonSkinNelUi).hilighted = false;
						}
					}
					this.waiting_categ_for_ = value;
					if (this.waiting_categ_for_ != CATEG._NOUSE)
					{
						Designer bxCategory2 = this.BxCategory;
						string text2 = "categ_";
						int num = (int)this.waiting_categ_for_;
						aBtn btn2 = bxCategory2.getBtn(text2 + num.ToString());
						if (btn2 != null)
						{
							(btn2.get_Skin() as ButtonSkinNelUi).hilighted = true;
						}
					}
				}
			}
		}

		public void newGame()
		{
			this.ReopenTarget = null;
			this.waiting_categ_for = CATEG._NOUSE;
			this.releaseGMCInstance();
			UiGMC.newGame();
			this.item_evacuate_prepare = 0;
		}

		public void newGameLoadAfter()
		{
			NelItemManager imng = this.M2D.IMNG;
			this.item_evacuate_prepare = X.Mx(imng.getInventory().getVisibleRowCount() + imng.getHouseInventory().getVisibleRowCount(), imng.getInventoryPrecious().getVisibleRowCount()) + 8;
			if (this.APoolEvacuatedItem == null)
			{
				this.APoolEvacuatedItem = new List<aBtn>(this.item_evacuate_prepare);
			}
			else
			{
				this.item_evacuate_prepare = X.Mx(0, this.item_evacuate_prepare - this.APoolEvacuatedItem.Count);
			}
			if (this.item_evacuate_prepare > 0)
			{
				if (this.APoolEvacuatedItem.Capacity < this.item_evacuate_prepare)
				{
					this.APoolEvacuatedItem.Capacity = this.item_evacuate_prepare;
				}
				base.gameObject.SetActive(true);
				this.auto_deactive_gameobject = false;
			}
		}

		public override void destruct()
		{
			base.destruct();
			if (this.APoolEvacuatedItem != null)
			{
				for (int i = this.APoolEvacuatedItem.Count - 1; i >= 0; i--)
				{
					try
					{
						IN.DestroyE(this.APoolEvacuatedItem[i].gameObject);
					}
					catch
					{
					}
				}
				this.APoolEvacuatedItem = null;
			}
		}

		internal void setPosType(UiGameMenu.POSTYPE _t)
		{
			bool flag = this.BenchMenu != null && this.BenchMenu.isTempWaiting();
			if (this.postype_ == UiGameMenu.POSTYPE.SVD || this.postype_ == UiGameMenu.POSTYPE.ITEM)
			{
				IN.setZ(this.BxR.transform, 0f);
				if (!flag && !this.category_to_quit)
				{
					this.BxCategory.bind();
				}
			}
			float w = this.BxR.w;
			if (this.postype_ == UiGameMenu.POSTYPE.MAP_EXTEND)
			{
				this.BxR.WHanim(this.right_w, this.BxR.get_sheight_px(), true, false);
				IN.setZ(this.BxR.transform, 0f);
				if (!flag && !this.category_to_quit)
				{
					this.BxCategory.bind();
				}
			}
			UiGameMenu.POSTYPE postype = this.postype_;
			if (this.postype_ == UiGameMenu.POSTYPE.ITEMMOVE_L || this.postype_ == UiGameMenu.POSTYPE.ITEMMOVE_R)
			{
				IN.setZ(this.BxR.transform, 0f);
				this.BxR.Focusable(true, true, null);
			}
			if (this.postype_ == UiGameMenu.POSTYPE.BENCH)
			{
				if (!flag && !this.category_to_quit)
				{
					this.BxCategory.bind();
				}
				this.BxR.bind();
				this.M2D.BlurSc.addFlag("UIGM");
				UIBase.Instance.gameMenuBenchSlide(false, false);
			}
			int num = ((this.postype_ != UiGameMenu.POSTYPE.OFFLINE) ? (-1) : 2);
			float right_box_center_x = this.right_box_center_x;
			switch (_t)
			{
			case UiGameMenu.POSTYPE.OFFLINE:
				this.BxCategory.position(-this.bounds_w * 1.6f, 50f, -1000f, -1000f, false);
				this.BxR.position(this.bounds_w * 2.4f, 50f, -1000f, -1000f, false);
				break;
			case UiGameMenu.POSTYPE.KEYCON:
				this.BxCategory.positionD(-this.bounds_w * 1.6f, 50f, -1, 30f);
				this.BxR.positionD(this.bounds_w * 2.4f, 50f, -1, 30f);
				break;
			case UiGameMenu.POSTYPE.NORMAL:
				this.setPosNormalPosition(flag, false);
				break;
			case UiGameMenu.POSTYPE.BENCH_TEMP:
				this.BxR.positionD(0f, this.BXR_Y_TRANSLATED, num, 230f);
				break;
			case UiGameMenu.POSTYPE.SVD:
				if (!flag)
				{
					this.BxCategory.posSetDA(-this.bounds_w * 1.6f, 50f, 2, 20f, true);
				}
				this.BxCategory.hide();
				this.BxR.positionD(right_box_center_x + 190f, this.BXR_Y_TRANSLATED, num, 230f);
				IN.setZ(this.BxR.transform, -0.5f);
				break;
			case UiGameMenu.POSTYPE.ITEM:
				if (!flag)
				{
					this.BxCategory.positionD(-this.bounds_w * 1.6f, 50f, -1, 30f);
				}
				this.BxCategory.hide();
				this.BxR.positionD(right_box_center_x + 130f, this.BXR_Y_TRANSLATED, num, 230f);
				IN.setZ(this.BxR.transform, -0.5f);
				break;
			case UiGameMenu.POSTYPE.ITEMMOVE_L:
			case UiGameMenu.POSTYPE.ITEMMOVE_R:
				this.BxR.Focusable(true, false, null);
				this.BxCategory.positionD(-this.bounds_w * 1.6f, 50f, -1, 30f);
				this.BxCategory.hide();
				IN.setZ(this.BxR.transform, -0.5f);
				IN.setZ(this.BxItmv.transform, -0.5f);
				break;
			case UiGameMenu.POSTYPE.BENCH:
				this.BxCategory.positionD(-this.bounds_w * 1.6f, 50f, -1, 30f);
				this.BxR.positionD(right_box_center_x + 1140f, this.BXR_Y_TRANSLATED, num, 230f);
				this.BxCategory.hide();
				this.BxR.hide();
				this.M2D.BlurSc.remFlag("UIGM");
				this.M2D.BlurSc.temporary = true;
				UIBase.Instance.gameMenuBenchSlide(true, false);
				IN.clearPushDown(true);
				this.M2D.hideAreaTitle(true);
				this.M2D.Cam.fineImmediately();
				break;
			case UiGameMenu.POSTYPE.MAP_EXTEND:
				this.BxCategory.positionD(-this.bounds_w * 1.6f, 50f, -1, 30f);
				this.BxR.positionD(0f, this.BXR_Y_TRANSLATED, -1, 0f);
				this.BxCategory.hide();
				this.BxR.WHanim(this.extend_right_w, this.BxR.get_sheight_px(), true, false);
				IN.setZ(this.BxR.transform, -0.5f);
				break;
			}
			if (this.AppearC != null && w != this.BxR.w)
			{
				this.AppearC.containerResized();
			}
			this.postype_ = _t;
		}

		public void setPosNormalPosition(bool bench_temp, bool use_da = false)
		{
			float num = -this.bounds_wh + 95f;
			float num2 = 50f;
			float right_box_center_x = this.right_box_center_x;
			float bxr_Y_TRANSLATED = this.BXR_Y_TRANSLATED;
			if (!bench_temp)
			{
				if (use_da)
				{
					this.BxCategory.posSetDA(num, num2, 1, 140f, true);
				}
				else
				{
					this.BxCategory.positionD(num, num2, (this.state != UiGameMenu.STATE.OFFLINE) ? (-1) : 1, 140f);
				}
			}
			if (use_da)
			{
				this.BxR.posSetDA(right_box_center_x, bxr_Y_TRANSLATED, 2, 230f, true);
				return;
			}
			this.BxR.positionD(right_box_center_x, bxr_Y_TRANSLATED, 2, 230f);
		}

		internal float BXR_Y_TRANSLATED
		{
			get
			{
				return 45f + (this.right_last_btm_row_height - this.right_last_top_row_height) / 2f;
			}
		}

		private void changeState(UiGameMenu.STATE st)
		{
			UiGameMenu.STATE state = this.state;
			if (st == UiGameMenu.STATE.CATEGORY)
			{
				if (this.category_to_quit_)
				{
					this.deactivate(false);
					return;
				}
			}
			else if (state == UiGameMenu.STATE.EDIT && st != UiGameMenu.STATE.EDIT && this.EditC != null)
			{
				this.EditC.hideEditTemporary();
			}
			if (st != UiGameMenu.STATE.EDIT && st != UiGameMenu.STATE.ITEMMOVE && this.BxDesc.isActive())
			{
				this.BxDesc.deactivate();
			}
			if (st == UiGameMenu.STATE.QUIT_GAME_TO_QUIT || st == UiGameMenu.STATE.QUIT_GAME_TO_TITLE)
			{
				this.createHideScreen();
				this.LdHd.activate();
				this.quitAppearCategory();
				this.setPosType(UiGameMenu.POSTYPE.OFFLINE);
				base.deactivate(false);
				this.active = true;
			}
			if (st == UiGameMenu.STATE.LOAD_GAME)
			{
				this.createHideScreen();
				this.FlgStatusHide.Add("LOADGAME");
				this.LdHd.activate();
				this.quitAppearCategory();
				this.setPosType(UiGameMenu.POSTYPE.OFFLINE);
				Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
				if (flgUiEffectDisable != null)
				{
					flgUiEffectDisable.Add("UIGM");
				}
				base.deactivate(false);
				this.active = true;
				BGM.stop(false, false);
				SND.Ui.play("sd_initialize", false);
			}
			if (this.isMainRightState(st))
			{
				UiGameMenu.POSTYPE postype = ((this.BenchMenu != null && this.BenchMenu.isTempWaiting()) ? UiGameMenu.POSTYPE.BENCH_TEMP : UiGameMenu.POSTYPE.NORMAL);
				float right_box_center_x = this.right_box_center_x;
				if (st == UiGameMenu.STATE.CATEGORY)
				{
					this.quitCategoryEdit();
					if (this.waiting_categ_for_ != CATEG._NOUSE)
					{
						this.BxCategory.bind();
					}
					if (state == UiGameMenu.STATE.EDIT && this.select_categ != this.appear_categ)
					{
						this.BxCategory.getBtn((int)this.select_categ).Select(false);
						this.appearCategory(this.select_categ, false);
					}
					else if (this.appear_categ != CATEG._NOUSE)
					{
						this.BxCategory.getBtn((int)this.appear_categ).Select(false);
					}
					else
					{
						this.BxCategory.getBtn(0).Select(false);
					}
					UiBoxDesigner.FocusTo(this.BxCategory);
				}
				if (st == UiGameMenu.STATE.EDIT)
				{
					UiBoxDesigner.FocusTo(this.BxR);
					if (this.EditFocusInitTo != null && !this.EditFocusInitTo.destructed)
					{
						this.EditFocusInitTo.Select(true);
					}
					CATEG categ = this.edit_categ;
					if (categ != CATEG.ITEM)
					{
						switch (categ)
						{
						case CATEG.MAP:
							postype = UiGameMenu.POSTYPE.MAP_EXTEND;
							goto IL_0304;
						case CATEG.BENCH:
						{
							if (this.BenchMenu != null)
							{
								if (!this.BenchMenu.isActive())
								{
									this.BenchMenu.destruct();
									this.BenchMenu = null;
								}
								else
								{
									this.BenchMenu.activateTemp(this.cmd_categ, false);
								}
							}
							if (this.BenchMenu == null)
							{
								this.BenchMenu = new UiBenchMenu(this, this.BxCmd, this.BenchChip, this.cmd_categ);
							}
							Flagger flgHideLog = UIBase.FlgHideLog;
							if (flgHideLog != null)
							{
								flgHideLog.Rem("UIGM");
							}
							Flagger flgUiEffectGmDisable = UIBase.FlgUiEffectGmDisable;
							if (flgUiEffectGmDisable != null)
							{
								flgUiEffectGmDisable.Rem("UIGM");
							}
							this.M2D.FlgRenderAfter.Rem("UIGM");
							BGM.remHalfFlag("UIGM");
							postype = UiGameMenu.POSTYPE.BENCH;
							goto IL_0304;
						}
						case CATEG.SVD_SELECT:
							postype = UiGameMenu.POSTYPE.SVD;
							goto IL_0304;
						}
						if (this.BxDesc.isActive())
						{
							this.BxDesc.deactivate();
						}
					}
					else
					{
						postype = UiGameMenu.POSTYPE.ITEM;
					}
					IL_0304:
					this.EditC = this.AppearC;
					this.state = st;
					if (postype != this.postype_)
					{
						this.setPosType(postype);
					}
					if (this.EditC != null)
					{
						this.EditC.initEdit();
					}
				}
				else
				{
					this.state = st;
					if (postype != this.postype_)
					{
						this.setPosType(postype);
					}
				}
			}
			else
			{
				this.state = st;
				this.TopTab.deactivate(false);
				this.BtmTab.deactivate(false);
			}
			IN.clearPushDown(true);
		}

		private void createHideScreen()
		{
			if (this.LdHd == null)
			{
				this.LdHd = new GameObject("Load Hide Screen").AddComponent<HideScreen>();
				this.LdHd.gameObject.layer = IN.LAY(IN.gui_layer_name);
				this.LdHd.Col = C32.d2c(4282004532U);
			}
			IN.setZAbs(this.LdHd.transform, -9.496f);
		}

		private bool isMainRightState(UiGameMenu.STATE st)
		{
			return st == UiGameMenu.STATE.CATEGORY || st == UiGameMenu.STATE.EDIT || st == UiGameMenu.STATE.ITEMMOVE;
		}

		public void initS(Map2d M)
		{
			UiGameMenu.need_whole_map_reentry = true;
			this.BenchChip = null;
		}

		public override UiBoxDesignerFamily activate()
		{
			this.M2D.WDR.walkAround(false);
			this.releaseGMCInstance();
			this.M2D.IMNG.BoxDeactivateFinalize();
			this.M2D.QUEST.need_fine_auto_check_item_collection = true;
			this.M2D.FlagValotStabilize.Add("UIGM");
			IN.use_mouse = false;
			this.waiting_categ_for = CATEG._NOUSE;
			this.item_modified = false;
			SND.Ui.play("tool_prmp_init", false);
			BGM.addHalfFlag("UIGM");
			this.M2D.NightCon.deactivate(true);
			this.M2D.Ui.QuestBox.FlgHide.Add("UIGM");
			this.M2D.need_blursc_hiding_whole = true;
			UIPictureBase.FlgStopAutoFade.Add("UIGM");
			UIBase.FlgHideLog.Add("UIGM");
			UIBase.FlgUiEffectGmDisable.Add("UIGM");
			this.M2D.FlgRenderAfter.Add("UIGM");
			IN.FlgUiUse.Add("UIGM");
			COOK.FlgTimerStop.Add("UIGM");
			EV.getMessageContainer().checkHideVisibilityTemporary(true, true);
			if (EV.isActive(false))
			{
				EV.TutoBox.addActiveFlag("UIGM");
			}
			this.M2D.PauseMem(true);
			this.auto_deactive_gameobject = (this.fine_bench_modal = false);
			UIBase.Instance.gameMenuSlide(true, false);
			this.FlgStatusHide.Rem("LOADGAME");
			this.FlgStatusHide.Rem("ITEM");
			this.FlgStatusHide.Rem("CFG");
			base.activate();
			this.setPosNormalPosition(false, true);
			this.postype_ = UiGameMenu.POSTYPE.OFFLINE;
			UiBoxDesigner.FocusTo(this.BxCategory);
			UiGameMenuTopTab.BoxSpeedSet(this.BxR.getBox(), false);
			this.check_evt_handle_mode = false;
			UiGameMenu.handle = true;
			this.HandleObject = null;
			this.need_one_activate_for_wait = false;
			UiSkillManageBox.event_focus_waiting_tab = 0U;
			this.category_to_quit_ = false;
			this.af = 0f;
			this.Pr = ((this.M2D.curMap != null) ? (this.M2D.curMap.getKeyPr() as PR) : null);
			this.pr_on_bench = (this.effect_confusion = false);
			this.M2D.IMNG.USel.deactivate();
			if (this.Pr != null)
			{
				this.effect_confusion = this.Pr.Ser.getLevel(SER.CONFUSE) >= 2;
				this.Pr.getSkillManager().initMenu();
				NelChipBench nearBench = this.Pr.getNearBench(false, false);
				if (nearBench != null)
				{
					nearBench.fineIcon();
					this.pr_on_bench = (this.BenchSkin.hilighted = true);
					if (this.BenchChip != nearBench)
					{
						this.BenchChip = nearBench;
						this.select_categ = CATEG.BENCH;
					}
				}
				else
				{
					this.pr_on_bench = (this.BenchSkin.hilighted = false);
					this.BenchChip = null;
				}
			}
			this.can_use_fasttravel = this.pr_on_bench && SCN.isBenchCmdEnable("fast_travel") && Map2d.can_handle;
			if (this.select_categ == CATEG._NOUSE)
			{
				this.select_categ = ((this.appear_categ == CATEG._NOUSE) ? CATEG.STAT : this.appear_categ);
			}
			this.appear_categ = this.select_categ;
			this.changeState(UiGameMenu.STATE.CATEGORY);
			if (this.LdHd != null)
			{
				IN.DestroyOne(this.LdHd.gameObject);
				this.LdHd = null;
			}
			this.edit_categ = CATEG._NOUSE;
			this.appearCategory(this.select_categ, true);
			this.BxR.getBox().scaling_alpha_set(false, true);
			this.refineLeftCategoriesHilight();
			this.BxDesc.hide();
			this.M2D.BlurSc.addFlag("UIGM");
			this.M2D.FlagOpenGm.Add("UIGM");
			X.SCLOCK(this);
			CURS.Omazinai();
			this.clearMapCancelingJump();
			this.clearQuestCancelingJump();
			this.ReopenTarget = null;
			NEL.Instance.Vib.clear(PadVibManager.VIB.IN_GAME);
			return this;
		}

		internal void clearQuestCancelingJump()
		{
			if (this.AGmcCache[5] != null)
			{
				(this.AGmcCache[5] as UiGMCMap).clearQuestCancelingJump();
			}
		}

		internal void clearMapCancelingJump()
		{
			if (this.AGmcCache[6] != null)
			{
				(this.AGmcCache[6] as UiGMCScenario).clearMapCancelingJump();
			}
		}

		public void releaseGMCInstance()
		{
			if (this.AGmcCache != null)
			{
				for (int i = this.AGmcCache.Length - 1; i >= 0; i--)
				{
					UiGMC.releaseGMCInstance(ref this.AGmcCache[i]);
				}
			}
		}

		public UiBoxDesignerFamily activateNormal()
		{
			if (!this.isActive())
			{
				if (this.ReopenTarget is NelItem)
				{
					NelItem nelItem = this.ReopenTarget as NelItem;
					this.activateItem();
					if (this.AGmcCache[1] is UiGMCItem)
					{
						(this.AGmcCache[1] as UiGMCItem).reveal(nelItem);
					}
					return this;
				}
				if (this.ReopenTarget is WMIconDescription)
				{
					WMIconDescription wmiconDescription = (WMIconDescription)this.ReopenTarget;
					this.activateMap();
					if (this.AGmcCache[5] is UiGMCMap)
					{
						(this.AGmcCache[5] as UiGMCMap).reveal(wmiconDescription);
					}
					return this;
				}
				if (this.ReopenTarget is string)
				{
					string text = (string)this.ReopenTarget;
					if (text != null && text == "Item")
					{
						this.activateItem();
						return this;
					}
					if (TX.isStart(text, "CATEGORY.", 0))
					{
						this.activate();
						CATEG categ;
						if (FEnum<CATEG>.TryParse(TX.slice(text, 9), out categ, true))
						{
							this.select_categ = categ;
							Designer bxCategory = this.BxCategory;
							string text2 = "categ_";
							int num = (int)categ;
							bxCategory.getBtn(text2 + num.ToString()).Select(false);
						}
						return this;
					}
				}
				this.activate();
			}
			return this;
		}

		public UiBoxDesignerFamily activateMap()
		{
			if (!this.isActive())
			{
				this.select_categ = CATEG.MAP;
				this.activate();
			}
			else
			{
				this.BxCategory.getBtn(5).Select(false);
				SND.Ui.play("enter_small", false);
			}
			this.initCategoryEdit(CATEG.MAP, false);
			if (!Map2d.can_handle)
			{
				this.category_to_quit = true;
			}
			UiGMCMap uiGMCMap = this.AGmcCache[5] as UiGMCMap;
			if (uiGMCMap != null)
			{
				QuestTracker.QuestDeperture frontDepert = this.M2D.Ui.QuestBox.getFrontDepert();
				if (frontDepert.isActiveMap())
				{
					uiGMCMap.reveal(frontDepert.WmDepert, false);
				}
			}
			return this;
		}

		public UiBoxDesignerFamily activateItem()
		{
			if (!this.isActive())
			{
				this.select_categ = CATEG.ITEM;
				this.activate();
			}
			else
			{
				this.BxCategory.getBtn(1).Select(false);
				SND.Ui.play("enter_small", false);
			}
			this.initCategoryEdit(CATEG.ITEM, false);
			if (!Map2d.can_handle)
			{
				this.category_to_quit = true;
			}
			return this;
		}

		public UiBoxDesignerFamily activateItemMove()
		{
			if (!this.isActive())
			{
				this.activate();
			}
			this.BxCategory.getBtn(1).Select(false);
			UiGMCItem uiGMCItem = this.initCategoryEdit(CATEG.ITEM, false) as UiGMCItem;
			if (uiGMCItem != null && this.IMNG.canSwitchItemMove())
			{
				uiGMCItem.initItemMove();
			}
			return this;
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			this.clearMapCancelingJump();
			this.clearQuestCancelingJump();
			if (this.isActive())
			{
				this.quitAppearCategory();
				UIBase.Instance.gameMenuSlide(false, this.state == UiGameMenu.STATE.LOAD_GAME);
				if (this.state != UiGameMenu.STATE.LOAD_GAME)
				{
					SND.Ui.play((this.ReopenTarget != null) ? "enter_small" : "close_ui", false);
				}
				PR.PunchDecline(9, false);
			}
			if (this.M2D.GameOver == null)
			{
				IN.clearPushDown(true);
			}
			else
			{
				IN.clearPushDown(false);
				IN.clearMenuPushDown(true);
			}
			if (this.BenchMenu != null)
			{
				this.BenchMenu.deactivateEdit(true);
			}
			if (this.LdHd != null)
			{
				this.LdHd.deactivate(false);
			}
			if (UIStatus.Instance != null)
			{
				UIPicture.Instance.alpha = 1f;
			}
			if (this.postype_ == UiGameMenu.POSTYPE.NORMAL)
			{
				this.setPosNormalPosition(false, true);
			}
			UiGameMenu.handle = false;
			this.HandleObject = null;
			this.waiting_categ_for = CATEG._NOUSE;
			this.category_to_quit_ = false;
			UiSkillManageBox.event_focus_waiting_tab = 0U;
			this.cmd_categ = UiGameMenu.STATE.OFFLINE;
			this.state = UiGameMenu.STATE.OFFLINE;
			UiGameMenuTopTab.BoxSpeedSet(this.BxR.getBox(), false);
			if (UIPictureBase.FlgStopAutoFade != null)
			{
				UIPictureBase.FlgStopAutoFade.Rem("UIGM");
			}
			this.quitCategoryEdit();
			this.BxR.wh_animZero(true, true);
			this.right_last_top_row_height = 0f;
			this.right_last_btm_row_height = 0f;
			base.deactivate(immediate);
			this.af = (immediate ? (-25f) : X.Mn(-1f, -25f + this.af));
			this.TopTab.deactivate(true);
			this.BtmTab.deactivate(true);
			X.REMLOCK(this);
			this.M2D.ResumeMem(true);
			COOK.FlgTimerStop.Rem("UIGM");
			IN.FlgUiUse.Rem("UIGM");
			BGM.remHalfFlag("UIGM");
			if (this.M2D.Ui != null && this.M2D.Ui.QuestBox != null)
			{
				this.M2D.Ui.QuestBox.FlgHide.Rem("UIGM");
				this.FlgStatusHide.Rem("LOADGAME");
			}
			this.M2D.BlurSc.remFlag("UIGM");
			this.M2D.FlagOpenGm.Rem("UIGM");
			if (EV.getMessageContainer() != null)
			{
				EV.getMessageContainer().checkHideVisibilityTemporary(false, true);
			}
			if (EV.TutoBox != null)
			{
				EV.TutoBox.remActiveFlag("UIGM");
			}
			this.M2D.BlurSc.temporary = false;
			this.M2D.Cam.fineImmediately();
			this.M2D.IMNG.digestDiscardStack(this.M2D.PlayerNoel);
			this.need_one_activate_for_wait = false;
			if (this.pr_on_bench && this.M2D.curMap != null)
			{
				if (this.fine_bench_modal)
				{
					this.fine_bench_modal = false;
					UiBenchMenu.showModalOfflineOnBench();
				}
				PRNoel playerNoel = this.M2D.PlayerNoel;
				if (playerNoel != null)
				{
					playerNoel.EpCon.cureOrgasmAfter();
				}
			}
			this.M2D.fineSentToHouseInv();
			if (this.item_modified)
			{
				this.M2D.IMNG.getInventory().fineRows(false);
				if (this.M2D.canAccesableToHouseInventory())
				{
					this.M2D.IMNG.getHouseInventory().fineRows(false);
				}
				this.item_modified = false;
			}
			if (immediate)
			{
				this.M2D.FlagValotStabilize.Rem("UIGM");
			}
			if (UIBase.FlgHideLog != null)
			{
				UIBase.FlgHideLog.Rem("UIGM");
				Flagger flgUiEffectGmDisable = UIBase.FlgUiEffectGmDisable;
				if (flgUiEffectGmDisable != null)
				{
					flgUiEffectGmDisable.Rem("UIGM");
				}
				Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
				if (flgUiEffectDisable != null)
				{
					flgUiEffectDisable.Rem("UIGM");
				}
			}
			this.M2D.FlgRenderAfter.Rem("UIGM");
			return this;
		}

		protected override bool runner_assigned
		{
			set
			{
				if (this.runner_assigned_ == value)
				{
					return;
				}
				base.runner_assigned = value;
				if (!value)
				{
					this.auto_deactive_gameobject = true;
				}
			}
		}

		private bool fnHoverCategory(aBtn B)
		{
			if (!UiGameMenu.handle || this.category_to_quit_)
			{
				return false;
			}
			if (UiBox.FlgLockFocus.isActive())
			{
				return false;
			}
			UiBoxDesigner.FocusTo(this.BxCategory);
			if (B.title.IndexOf("categ_") == 0)
			{
				CATEG categ = (CATEG)X.NmI(TX.slice(B.title, 6), 0, false, false);
				this.select_categ = categ;
				if (categ == this.appear_categ)
				{
					return true;
				}
				if (this.state != UiGameMenu.STATE.CATEGORY)
				{
					return true;
				}
				this.appearCategory(categ, false);
			}
			return true;
		}

		private bool fnOutCategory(aBtn B)
		{
			if (B.title.IndexOf("categ_") == 0)
			{
				X.NmI(TX.slice(B.title, 6), 0, false, false);
			}
			return true;
		}

		private bool fnClickCategory(aBtn B)
		{
			if (!UiGameMenu.handle || (this.check_evt_handle_mode ? (this.af <= 15f) : (this.af <= 5f)) || this.category_to_quit_)
			{
				return false;
			}
			if (UiBox.FlgLockFocus.isActive())
			{
				return false;
			}
			UiBoxDesigner.FocusTo(this.BxCategory);
			if (B.title.IndexOf("categ_") == 0 && !EV.isStoppingGameHandle())
			{
				CATEG categ = (CATEG)X.NmI(TX.slice(B.title, 6), 0, false, false);
				if (this.waiting_categ_for == CATEG._NOUSE || this.waiting_categ_for == categ)
				{
					if (this.state != UiGameMenu.STATE.CATEGORY)
					{
						this.changeState(UiGameMenu.STATE.CATEGORY);
					}
					this.initCategoryEdit(categ, false);
				}
			}
			return true;
		}

		private void quitCategoryEdit()
		{
			if (this.EditC != null)
			{
				this.EditC.quitEdit();
			}
			if (this.edit_categ == CATEG.CONFIG)
			{
				CFG.endEdit();
				this.resetConditionUI();
				this.M2D.Ui.fineUiDmgCounterDraw();
				if (this.Pr.EggCon.total > 0 && CFG.isSpEnabled("threshold_pregnant"))
				{
					this.Pr.UP.recheck_emot = true;
					this.Pr.recheck_emot_in_gm = true;
				}
				if ((int)this.ui_lang != TX.getCurrentFamilyIndex())
				{
					this.remakeLeftCategories(true);
					CFG.refineAllLanguageCache(true);
					this.BxR.Clear();
					for (int i = 0; i < 10; i++)
					{
						if (this.AGmcCache[i] != null)
						{
							this.AGmcCache[i].releaseEvac();
						}
					}
					this.AGmcCache[(int)this.appear_categ].initAppearWhole();
				}
			}
			if (this.edit_categ != CATEG._NOUSE)
			{
				this.BxCategory.getBtn((int)this.edit_categ).SetChecked(false, true);
			}
			if (this.edit_categ == CATEG.STAT)
			{
				Designer tab = this.BxR.getTab("status_area");
				if (tab != null)
				{
					tab.scroll_area_selectable = this.state == UiGameMenu.STATE.EDIT;
				}
			}
			if (this.edit_categ == CATEG.MAP && this.BxDesc.isActive())
			{
				this.BxDesc.deactivate();
			}
			CATEG categ = this.edit_categ;
			if (this.edit_categ == CATEG.BENCH)
			{
				if (this.BenchMenu != null)
				{
					this.BenchMenu.deactivateEdit(false);
				}
				BGM.addHalfFlag("UIGM");
				if (UIBase.FlgHideLog != null)
				{
					UIBase.FlgHideLog.Add("UIGM");
					UIBase.FlgUiEffectGmDisable.Add("UIGM");
				}
				this.M2D.FlgRenderAfter.Add("UIGM");
				if (this.Pr.EpCon.SituCon.flushLastExSituationTemp() && this.AGmcCache[0] is UiGMCStat)
				{
					(this.AGmcCache[0] as UiGMCStat).resetEpStat();
				}
			}
			this.edit_categ = CATEG._NOUSE;
			this.EditC = null;
			if (this.waiting_categ_for_ != CATEG._NOUSE && this.edit_categ == this.waiting_categ_for_)
			{
				this.BxCategory.hide();
			}
		}

		internal UiGMC initCategoryEdit(CATEG ct, bool set_to_select_category = false)
		{
			if (set_to_select_category)
			{
				this.select_categ = ct;
			}
			if (ct != this.edit_categ)
			{
				this.changeState(UiGameMenu.STATE.CATEGORY);
				this.edit_categ = ct;
				this.appearCategory(this.edit_categ, false);
			}
			if (this.AppearC != null && !this.AppearC.canInitEdit())
			{
				CURS.limitVib(this.BxCategory.getBtn((int)ct), AIM.R);
				SND.Ui.play("locked", false);
				return null;
			}
			if (ct != CATEG.MAP)
			{
				this.BxDesc.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			}
			if (ct == CATEG.BENCH && (!this.pr_on_bench || this.BenchChip == null || this.Pr == null || !UiBenchMenu.canSwitchToBenchMenu(this.Pr)))
			{
				CURS.limitVib(this.BxCategory.getBtn((int)ct), AIM.R);
				SND.Ui.play("locked", false);
				return null;
			}
			this.BxCategory.getBtn((int)ct).SetChecked(true, true);
			UiBox.FocusTo(this.BxR.getBox());
			this.changeState(UiGameMenu.STATE.EDIT);
			if (this.waiting_categ_for_ != CATEG._NOUSE && this.edit_categ == this.waiting_categ_for_)
			{
				this.BxCategory.hide();
			}
			return this.EditC;
		}

		private void quitAppearCategory()
		{
			if (this.EditC != null)
			{
				this.quitCategoryEdit();
			}
			if (this.AppearC != null)
			{
				this.AppearC.quitAppear();
				this.AppearC = null;
			}
			this.appear_categ = CATEG._NOUSE;
		}

		internal void BxRRemake(bool force = false)
		{
			this.BxR.Clear();
			this.BxR.margin_in_lr = 28f;
			this.BxR.box_stencil_ref_mask = -1;
			this.BxR.item_margin_x_px = 14f;
			this.BxR.item_margin_y_px = 18f;
			this.BxR.alignx = ALIGN.LEFT;
			this.BxR.use_scroll = false;
			bool flag = false;
			float num = this.TopTab.activateState(this.AppearC, true, this.right_box_center_x, 45f);
			if (num != this.right_last_top_row_height)
			{
				this.right_last_top_row_height = num;
				flag = true;
			}
			float num2 = this.BtmTab.activateState(this.AppearC, false, this.right_box_center_x, 45f);
			if (num2 != this.right_last_btm_row_height)
			{
				this.right_last_btm_row_height = num2;
				flag = true;
			}
			float num3 = ((this.BenchMenu != null && this.BenchMenu.isTempWaiting() && this.appear_categ == CATEG.MAP) ? this.extend_right_w : this.right_w);
			if (force || flag || num3 != this.BxR.get_swidth_px())
			{
				this.BxR.WHanim(num3, this.bounds_h - num - num2, true, true);
				if (this.isMainRightState(this.state))
				{
					UiGameMenuTopTab.BoxSpeedSet(this.BxR.getBox(), true);
					this.BxR.position(this.right_box_center_x, this.BXR_Y_TRANSLATED, -1000f, -1000f, false);
				}
			}
		}

		private void appearCategory(CATEG ct, bool force = false)
		{
			IN.clearPushDown(true);
			if (!force && this.appear_categ == ct)
			{
				return;
			}
			if (!force && this.appear_categ != ct && this.af >= (float)(this.BXR_DELAYT + 2))
			{
				SND.Ui.play("tool_changegear", false);
			}
			this.quitAppearCategory();
			this.EditFocusInitTo = null;
			this.appear_categ = ct;
			switch (ct)
			{
			case CATEG.STAT:
			{
				UiGMC uiGMC;
				if ((uiGMC = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC = (this.AGmcCache[(int)ct] = new UiGMCStat(this));
				}
				this.AppearC = uiGMC;
				break;
			}
			case CATEG.ITEM:
			{
				UiGMC uiGMC2;
				if ((uiGMC2 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC2 = (this.AGmcCache[(int)ct] = new UiGMCItem(this, this.BxCmd, this.BxItmv, this.ItemMng, this.ItemMngMv));
				}
				this.AppearC = uiGMC2;
				break;
			}
			case CATEG.MAGIC:
			{
				UiGMC uiGMC3;
				if ((uiGMC3 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC3 = (this.AGmcCache[(int)ct] = new UiGMCMagic(this));
				}
				this.AppearC = uiGMC3;
				break;
			}
			case CATEG.SKILL:
			{
				UiGMC uiGMC4;
				if ((uiGMC4 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC4 = (this.AGmcCache[(int)ct] = new UiGMCSkill(this));
				}
				this.AppearC = uiGMC4;
				break;
			}
			case CATEG.ENHANCER:
			{
				UiGMC uiGMC5;
				if ((uiGMC5 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC5 = (this.AGmcCache[(int)ct] = new UiGMCEnhancer(this));
				}
				this.AppearC = uiGMC5;
				break;
			}
			case CATEG.MAP:
			{
				UiGMC uiGMC6;
				if ((uiGMC6 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC6 = (this.AGmcCache[(int)ct] = new UiGMCMap(this, this.M2D.Marker));
				}
				this.AppearC = uiGMC6;
				break;
			}
			case CATEG.SCENARIO:
			{
				UiGMC uiGMC7;
				if ((uiGMC7 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC7 = (this.AGmcCache[(int)ct] = new UiGMCScenario(this));
				}
				this.AppearC = uiGMC7;
				break;
			}
			case CATEG.BENCH:
			{
				UiGMC uiGMC8;
				if ((uiGMC8 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC8 = (this.AGmcCache[(int)ct] = new UiGMCBench(this));
				}
				this.AppearC = uiGMC8;
				break;
			}
			case CATEG.SVD_SELECT:
			{
				UiGMC uiGMC9;
				if ((uiGMC9 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC9 = (this.AGmcCache[(int)ct] = new UiGMCSvd(this));
				}
				this.AppearC = uiGMC9;
				break;
			}
			case CATEG.CONFIG:
			{
				UiGMC uiGMC10;
				if ((uiGMC10 = this.AGmcCache[(int)ct]) == null)
				{
					uiGMC10 = (this.AGmcCache[(int)ct] = new UiGMCCfg(this));
				}
				this.AppearC = uiGMC10;
				break;
			}
			default:
				this.BxR.init();
				this.BxR.P(TX.Get("Category_under_construction", ""), ALIGN.LEFT, 0f, false, 0f, "").Br();
				break;
			}
			this.BxRRemake(force);
			if (this.AppearC != null)
			{
				this.AppearC.initAppearWhole();
			}
			if (this.waiting_categ_for_ != CATEG._NOUSE)
			{
				if (ct != this.waiting_categ_for_)
				{
					this.BxR.hide();
					return;
				}
				this.BxR.bind();
			}
		}

		public override bool runIRD(float fcnt)
		{
			if (this.item_evacuate_prepare > 0)
			{
				int num = X.Mn(30, this.item_evacuate_prepare);
				for (int i = 0; i < num; i++)
				{
					this.item_evacuate_prepare--;
					aBtnItemRow aBtnItemRow = IN.CreateGob(base.gameObject, "-child").AddComponent<aBtnItemRow>();
					aBtnItemRow.default_stencil_ref = this.bxr_stencil_default;
					aBtnItemRow.h = 30f;
					aBtnItemRow.w = this.right_w - 30f;
					aBtnItemRow.initializeSkin("normal", "");
					aBtnItemRow.gameObject.SetActive(false);
					this.APoolEvacuatedItem.Add(aBtnItemRow);
				}
			}
			switch (this.state)
			{
			case UiGameMenu.STATE.CATEGORY:
				if (!this.M2D.isRbkPD() || !this.initRecipeBook("CATEGORY." + this.select_categ.ToString(), null))
				{
					if (UiGameMenu.handle && this.af >= 1f)
					{
						if (IN.isCancel() && this.waiting_categ_for == CATEG._NOUSE)
						{
							this.deactivate(false);
							break;
						}
						if (IN.isMapPD(1) && (this.waiting_categ_for == CATEG._NOUSE || this.waiting_categ_for == CATEG.MAP))
						{
							this.activateMap();
							break;
						}
						if (IN.isItmU(1) && (this.waiting_categ_for == CATEG._NOUSE || this.waiting_categ_for == CATEG.ITEM))
						{
							this.activateItem();
							break;
						}
					}
					if (this.BxR.isFocused())
					{
						if (this.appear_categ != CATEG._NOUSE && UiGameMenu.handle && (this.waiting_categ_for == CATEG._NOUSE || this.waiting_categ_for == this.appear_categ))
						{
							if (this.initCategoryEdit(this.appear_categ, false) == null)
							{
								UiBox.FocusTo(this.BxCategory.getBox());
							}
						}
						else
						{
							UiBox.FocusTo(this.BxCategory.getBox());
						}
					}
					else
					{
						if (UiGameMenu.handle && this.waiting_categ_for_ != CATEG._NOUSE && this.appear_categ != this.waiting_categ_for_ && (IN.isSubmitPD(1) || IN.isCancelPD()))
						{
							aBtn preSelected = aBtn.PreSelected;
							if (preSelected == null || !TX.isStart(preSelected.title, "categ_", 0))
							{
								Designer bxCategory = this.BxCategory;
								string text = "categ_";
								int num2 = (int)this.appear_categ;
								aBtn btn = bxCategory.getBtn(text + num2.ToString());
								if (btn != null)
								{
									btn.Select(false);
									IN.clearPushDown(true);
								}
							}
						}
						if (this.af == (float)this.BXR_DELAYT && this.appear_categ != CATEG._NOUSE)
						{
							this.BxCategory.getBtn((int)this.appear_categ).Deselect(true).Select(false);
						}
					}
				}
				break;
			case UiGameMenu.STATE.EDIT:
				if (this.edit_categ == CATEG.BENCH)
				{
					if (this.BenchMenu == null || !this.BenchMenu.isActive())
					{
						this.changeState(UiGameMenu.STATE.CATEGORY);
						this.checkCancelingJump();
					}
				}
				else
				{
					if (UiGameMenu.handle && !CtSetter.hasFocus() && (this.waiting_categ_for == CATEG._NOUSE || this.waiting_categ_for != this.edit_categ) && (this.EditC == null || this.EditC.cancelable))
					{
						if (IN.isCancel())
						{
							this.changeState(UiGameMenu.STATE.CATEGORY);
							SND.Ui.play("cancel", false);
							this.checkCancelingJump();
							break;
						}
					}
					else if (this.BxCategory.isFocused())
					{
						UiBox.FocusTo(this.BxR.getBox());
					}
					if (this.EditC != null)
					{
						switch (this.EditC.runEdit(fcnt, UiGameMenu.handle))
						{
						case GMC_RES.BACK_CATEGORY:
							this.changeState(UiGameMenu.STATE.CATEGORY);
							SND.Ui.play("cancel", false);
							this.checkCancelingJump();
							break;
						case GMC_RES.QUIT_GM:
							this.changeState(UiGameMenu.STATE.CATEGORY);
							SND.Ui.play("cancel", false);
							this.deactivate(false);
							return true;
						case GMC_RES.LOAD_GAME:
							this.changeState(UiGameMenu.STATE.LOAD_GAME);
							return true;
						case GMC_RES.QUIT_TO_TITLE:
							this.changeState(UiGameMenu.STATE.QUIT_GAME_TO_TITLE);
							return true;
						case GMC_RES.QUIT_GAME:
							this.changeState(UiGameMenu.STATE.QUIT_GAME_TO_QUIT);
							return true;
						}
					}
				}
				break;
			case UiGameMenu.STATE.LOAD_GAME:
				if (this.LdHd.full_shown)
				{
					if (this.M2D.curMap != null)
					{
						this.M2D.restartGame(true);
						SceneGame.restartGameSceneASync(this.M2D, true, "");
					}
					else
					{
						this.deactivate(false);
						this.auto_deactive_gameobject = false;
						this.af = -125f;
						this.M2D.FlagValotStabilize.Rem("UIGM");
					}
				}
				break;
			case UiGameMenu.STATE.QUIT_GAME_TO_TITLE:
			case UiGameMenu.STATE.QUIT_GAME_TO_QUIT:
				if (this.LdHd.full_shown)
				{
					this.M2D.quitGame((this.state == UiGameMenu.STATE.QUIT_GAME_TO_TITLE) ? "SceneTitle" : null);
					return true;
				}
				break;
			}
			bool flag = base.runIRD(fcnt);
			bool flag2 = true;
			if (this.af >= 0f)
			{
				this.Pr.Ser.checkSerExecute(false, true);
				if (this.af < 25f)
				{
					this.af += fcnt;
				}
				if (this.AppearC != null)
				{
					this.AppearC.runAppearing();
				}
				this.checkHandle();
				if (!this.FlgStatusHide.isActive())
				{
					UIStatus.showHold(20, true);
				}
			}
			else if (this.af > -25f)
			{
				this.af -= fcnt;
			}
			else
			{
				flag2 = false;
			}
			if (this.BenchMenu != null && !this.BenchMenu.run(fcnt))
			{
				this.BenchMenu.destruct();
				this.BenchMenu = null;
			}
			if (this.af < 0f && this.LdHd != null)
			{
				if (this.LdHd.isActive())
				{
					this.LdHd.deactivate(false);
					flag2 = true;
				}
				else if (!this.LdHd.full_hidden)
				{
					flag2 = true;
				}
			}
			if (!flag && !flag2)
			{
				if (this.LdHd != null)
				{
					IN.DestroyOne(this.LdHd.gameObject);
					this.LdHd = null;
				}
				this.M2D.FlagValotStabilize.Rem("UIGM");
				this.releaseGMCInstance();
				if (this.item_evacuate_prepare <= 0)
				{
					this.M2D.need_blursc_hiding_whole = true;
					base.gameObject.SetActive(false);
					return false;
				}
			}
			return true;
		}

		private bool checkCancelingJump()
		{
			if (this.AGmcCache[5] is UiGMCMap && (this.AGmcCache[5] as UiGMCMap).executeQuestCancelingJump())
			{
				this.select_categ = CATEG.MAP;
				return true;
			}
			if (this.AGmcCache[6] is UiGMCScenario && (this.AGmcCache[6] as UiGMCScenario).executeMapCancelingJump())
			{
				this.select_categ = CATEG.SCENARIO;
				return true;
			}
			return this.BenchMenu != null && this.BenchMenu.isTempWaiting() && this.initCategoryEdit(CATEG.BENCH, false) != null;
		}

		private void checkHandle()
		{
			bool flag = !this.check_evt_handle_mode || !EV.isStoppingGameHandle();
			if (UiGameMenu.handle != flag)
			{
				UiGameMenu.handle = flag;
				if (flag)
				{
					if (this.HandleObject != null && !this.HandleObject.destructed)
					{
						try
						{
							this.HandleObject.Select(false);
						}
						catch
						{
						}
					}
					this.HandleObject = null;
					return;
				}
				this.HandleObject = aBtn.PreSelected;
				if (this.HandleObject != null)
				{
					this.HandleObject.Deselect(true);
				}
			}
		}

		public bool isFading()
		{
			return this.af < 30f;
		}

		public bool initRecipeBook(object _ReopenTarget, object RecipeBookTarget = null)
		{
			if (!this.IMNG.has_recipe_collection)
			{
				return false;
			}
			if (EV.isActive(false))
			{
				SND.Ui.play("locked", false);
				return false;
			}
			bool flag = true;
			if (this.isActive())
			{
				if (this.state != UiGameMenu.STATE.CATEGORY)
				{
					this.changeState(UiGameMenu.STATE.CATEGORY);
				}
				this.ReopenTarget = _ReopenTarget;
				this.deactivate(false);
			}
			else
			{
				flag = false;
			}
			UiFieldGuide.NextRevealAtAwake = RecipeBookTarget ?? this.ReopenTarget;
			EV.stack("___GM/alchemy_recipe_book", 0, -1, new string[] { flag ? "1" : "0" }, null);
			return true;
		}

		internal bool initBxCmd(UiGameMenu.STATE state, out UiBoxDesigner BxCmd)
		{
			BxCmd = this.BxCmd;
			if (state == this.cmd_categ)
			{
				return false;
			}
			this.initBxCmd(state);
			return true;
		}

		internal UiBoxDesigner initBxCmdClearing()
		{
			return this.initBxCmd(UiGameMenu.STATE.OFFLINE);
		}

		internal UiBoxDesigner initBxCmd(UiGameMenu.STATE state)
		{
			this.cmd_categ = state;
			this.BxCmd.Clear();
			this.BxCmd.use_button_connection = true;
			this.BxCmd.Focusable(true, true, null);
			this.BxCmd.activate();
			this.BxCmd.WH(0f, 0f);
			this.BxCmd.wh_animZero(true, true);
			this.BxCmd.margin_in_lr = 38f;
			this.BxCmd.margin_in_tb = 31f;
			this.BxCmd.selectable_loop = 1;
			this.BxCmd.item_margin_x_px = 0f;
			this.BxCmd.item_margin_y_px = 0f;
			this.BxCmd.box_stencil_ref_mask = -1;
			this.BxCmd.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			UiBoxDesigner.FocusTo(this.BxCmd);
			return this.BxCmd;
		}

		internal void resetConditionUI()
		{
			if (this.AGmcCache[0] is UiGMCStat)
			{
				(this.AGmcCache[0] as UiGMCStat).resetCondition();
			}
		}

		internal void resetStomachUI()
		{
			if (this.AGmcCache[0] is UiGMCStat)
			{
				(this.AGmcCache[0] as UiGMCStat).resetStomach();
			}
		}

		internal void resetEpStatUI()
		{
			if (this.AGmcCache[0] is UiGMCStat)
			{
				(this.AGmcCache[0] as UiGMCStat).resetEpStat();
			}
		}

		internal void quitLunch(UiLunchTime LunchTime, bool do_not_destruct_element = false)
		{
			if (LunchTime != null)
			{
				if (LunchTime.consumeEaten() > 0)
				{
					this.resetStomachUI();
				}
				if (!do_not_destruct_element)
				{
					Object.Destroy(LunchTime.gameObject);
				}
				else
				{
					LunchTime.enabled = true;
				}
				if (this.state == UiGameMenu.STATE.EDIT && this.edit_categ == CATEG.ITEM)
				{
					this.quitUiTemporaryHide();
				}
			}
		}

		internal void quitUiTemporaryHide()
		{
			if (this.postype_ == UiGameMenu.POSTYPE.KEYCON)
			{
				this.BxR.bind();
				this.BxDesc.bind();
				IN.setZ(this.BxDesc.transform, -0.58f);
				this.setPosType(UiGameMenu.POSTYPE.ITEM);
			}
		}

		public void evOpen(bool is_start)
		{
			if (!this.isActive() || !is_start)
			{
				return;
			}
			if (this.BenchMenu != null)
			{
				this.BenchMenu.isTempWaiting();
			}
		}

		public void evClose(bool is_end)
		{
			if (!this.isActive() || !is_end)
			{
				return;
			}
			if (this.state == UiGameMenu.STATE.EDIT && this.edit_categ == CATEG.BENCH && this.BenchMenu != null && this.BenchMenu.isTempWaiting())
			{
				this.BenchMenu.activateTemp(this.cmd_categ, true);
				UIBase.Instance.gameMenuSlide(true, false);
				UIBase.Instance.gameMenuBenchSlide(true, false);
			}
			this.M2D.FlagValotStabilize.Add("UIGM");
		}

		public IEventWaitListener WaitFnGMStopGame()
		{
			return new UiGameMenu.WaitListenerGMStoppingGame(this);
		}

		public IEventWaitListener WaitFnGMActivate()
		{
			return new UiGameMenu.WaitListenerGMActivate(this, true);
		}

		public bool EvtRead(StringHolder rER)
		{
			string _ = rER._1;
			if (_ != null)
			{
				if (_ == "WAIT")
				{
					if (rER._2 == "UIGM_ACTIVATE")
					{
						EV.initWaitFn(this.WaitFnGMActivate(), 0);
					}
					else if (REG.match(rER._2, new Regex("OPENTAB_(.+)")))
					{
						CATEG categ;
						if (FEnum<CATEG>.TryParse(REG.R1, out categ, true))
						{
							EV.initWaitFn(new UiGameMenu.WaitListenerGMOpenTab(this, categ), 0);
						}
					}
					else if (REG.match(rER._2, new Regex("OPENSKILLTAB_(.+)")))
					{
						SkillManager.SKILL_CTG skill_CTG;
						if (FEnum<SkillManager.SKILL_CTG>.TryParse(REG.R1, out skill_CTG, true))
						{
							EV.initWaitFn(new UiGameMenu.WaitListenerGMOpenSkillTab(this, skill_CTG), 0);
						}
					}
					else if (REG.match(rER._2, new Regex("SKILLENABLE_(.+)")))
					{
						string[] array = TX.split(REG.R1, "|");
						List<PrSkill> list = new List<PrSkill>(array.Length);
						for (int i = array.Length - 1; i >= 0; i--)
						{
							PrSkill prSkill = SkillManager.Get(array[i]);
							if (prSkill == null)
							{
								rER.tError("不明なスキル: " + array[i]);
							}
							else
							{
								list.Add(prSkill);
							}
						}
						EV.initWaitFn(new UiGameMenu.WaitListenerGMSkillEnable(this, list.ToArray()), 0);
					}
					else if (rER._2 == "UIGM_ACTIVATE_EVENT_RUN_IN_MENU")
					{
						EV.initWaitFn(new UiGameMenu.WaitListenerGMActivate(this, false), 0);
					}
					return true;
				}
				if (_ == "HIDEBTN")
				{
					this.BxCategory.hide();
					this.BxR.hide();
					return true;
				}
				if (_ == "DEACTIVATE")
				{
					if (this.isActive())
					{
						this.deactivate(false);
					}
					return true;
				}
				if (!(_ == "CHOOSE_ITEM"))
				{
					if (_ == "CATEGORY_DEFAULT")
					{
						if (this.isActive())
						{
							this.BxCategory.getBtn(0).Select(false);
						}
						else
						{
							this.select_categ = CATEG._NOUSE;
						}
						return true;
					}
					if (_ == "WAIT_NIGHTINGALE")
					{
						bool flag = false;
						if (this.BenchChip != null)
						{
							float num;
							this.M2D.IMNG.StmNoel.progress(1f, true, out num, true, false);
							this.M2D.IMNG.StmNoel.Pr.progressWaterDrunkCache(num, false, false);
							WanderingNPC nightingale = this.M2D.WDR.getNightingale();
							if (nightingale.checkBench(this.BenchChip, 1f, 3f, true))
							{
								flag = true;
							}
							else
							{
								nightingale.walkAround();
								if (nightingale.checkBench(this.BenchChip, 1f, 4.5f, true))
								{
									flag = true;
								}
							}
						}
						if (!flag)
						{
							UILog.Instance.AddAlertTX("Alert_no_nightingale", UILogRow.TYPE.ALERT);
						}
						return true;
					}
				}
				else
				{
					this.activateItem();
					NelItem byId = NelItem.GetById(rER._2, true);
					if (byId == null)
					{
						return rER.tError(rER._2);
					}
					EV.initWaitFn(new UiGameMenu.WaitListenerGMItemChoose(this, byId, rER.getIndex(3, "_result", true), rER._B4), 0);
					return true;
				}
			}
			return false;
		}

		public bool general_button_handleable
		{
			get
			{
				return UiGameMenu.handle && !(this.check_evt_handle_mode ? (this.af <= 15f) : (this.af <= 5f)) && !this.category_to_quit_ && this.waiting_categ_for == CATEG._NOUSE;
			}
		}

		public bool category_to_quit
		{
			get
			{
				return this.category_to_quit_;
			}
			set
			{
				this.category_to_quit_ = value;
				if (value && this.isActive())
				{
					this.BxCategory.hide();
				}
			}
		}

		public NelItemManager IMNG
		{
			get
			{
				return this.M2D.IMNG;
			}
		}

		public UiBenchMenu getBenchMenu()
		{
			return this.BenchMenu;
		}

		internal UiGameMenu.POSTYPE postype
		{
			get
			{
				return this.postype_;
			}
		}

		public bool isStoppingGame()
		{
			return this.isActive() && this.postype_ != UiGameMenu.POSTYPE.BENCH;
		}

		public bool isBenchMenuActive()
		{
			return this.isActive() && this.BenchMenu != null && this.BenchMenu.isActive();
		}

		public bool isEditState()
		{
			return this.state == UiGameMenu.STATE.EDIT;
		}

		internal bool isEditState(UiGMC Gmc)
		{
			return this.state == UiGameMenu.STATE.EDIT && Gmc == this.EditC;
		}

		internal bool need_cmd_remake
		{
			set
			{
				if (value)
				{
					this.cmd_categ = UiGameMenu.STATE.OFFLINE;
				}
			}
		}

		internal bool bench_done_cmd
		{
			set
			{
				if (this.BenchMenu != null)
				{
					this.BenchMenu.done_cmd = value;
				}
			}
		}

		internal bool cmdCategIs(UiGameMenu.STATE state)
		{
			return this.cmd_categ == state;
		}

		internal bool isShowingGMC(UiGMC Gmc)
		{
			return Gmc == this.AppearC;
		}

		public UiBoxDesigner getRightBox()
		{
			return this.BxR;
		}

		internal PR Pr;

		internal bool effect_confusion;

		internal UiBoxDesigner BxCategory;

		internal UiBoxDesigner BxR;

		internal UiBoxDesigner BxDesc;

		internal UiBoxDesigner BxCmd;

		internal UiBoxDesigner BxItmv;

		internal UiGameMenuTopTab TopTab;

		internal UiGameMenuTopTab BtmTab;

		internal float bounds_w = IN.w - 340f - 60f;

		public float bounds_wh;

		public float right_w;

		internal float right_wh;

		internal float right_last_top_row_height;

		internal float right_last_btm_row_height;

		internal const float btn_w = 190f;

		internal const float btn_h = 38f;

		internal const float desc_z = -0.58f;

		public const float desc_item_w = 360f;

		public const float desc_item_w_mv = 300f;

		public const float cmd_w_count_select = 390f;

		internal const float cmd_w = 320f;

		public const string str_arrow5_icon = "<img mesh=\"arrow_nel_5\" width=\"32\" height=\"18\" />";

		private int BXR_DELAYT = 8;

		private float af = -25f;

		private const float ANIMATE_MAXT = 25f;

		internal aBtn EditFocusInitTo;

		private CATEG select_categ;

		private CATEG edit_categ = CATEG._NOUSE;

		private CATEG appear_categ = CATEG._NOUSE;

		private CATEG waiting_categ_for_ = CATEG._NOUSE;

		internal UiGameMenu.STATE state;

		private UiItemManageBoxSlider ItemMng;

		private UiItemManageBoxSlider ItemMngMv;

		internal int bxr_stencil_default;

		public const int ITEM_MARGIN_X = 14;

		public const int ITEM_MARGIN_Y = 18;

		public const float BXR_Y = 45f;

		internal NelM2DBase M2D;

		internal bool pr_on_bench;

		internal bool can_use_fasttravel;

		private UiGMC AppearC;

		private UiGMC EditC;

		private UiGMC[] AGmcCache;

		private UiBenchMenu BenchMenu;

		public static bool need_whole_map_reentry = true;

		private UiGameMenu.STATE cmd_categ;

		private UiGameMenu.POSTYPE postype_;

		private ButtonSkinNelUi BenchSkin;

		private HideScreen LdHd;

		internal byte ui_lang;

		public NelChipBench BenchChip;

		public bool fine_bench_modal;

		internal bool item_modified;

		public List<aBtn> APoolEvacuatedItem;

		private int item_evacuate_prepare;

		public static bool handle;

		private aBtn HandleObject;

		private bool category_to_quit_;

		private bool need_one_activate_for_wait;

		private bool check_evt_handle_mode;

		private object ReopenTarget;

		internal enum STATE
		{
			OFFLINE,
			CATEGORY,
			EDIT,
			ITEMMOVE,
			LOAD_GAME,
			BENCH,
			QUIT_GAME_TO_TITLE,
			QUIT_GAME_TO_QUIT,
			_USEITEMSEL,
			_MAP_ENEMY,
			_MAP_FASTTRAVEL,
			_MAP_MARKER,
			_ITEM_WLINK_TARGET = 11
		}

		internal enum POSTYPE
		{
			OFFLINE,
			KEYCON,
			NORMAL,
			BENCH_TEMP,
			SVD,
			ITEM,
			ITEMMOVE_L,
			ITEMMOVE_R,
			BENCH,
			MAP_EXTEND
		}

		public class WaitListenerGMStoppingGame : IEventWaitListener
		{
			public WaitListenerGMStoppingGame(UiGameMenu _GM)
			{
				this.GM = _GM;
			}

			public virtual bool EvtWait(bool is_first = false)
			{
				return this.GM.isStoppingGame();
			}

			private readonly UiGameMenu GM;
		}

		public class WaitListenerGMActivate : IEventWaitListener
		{
			public WaitListenerGMActivate(UiGameMenu _GM, bool _wait_until_gm_close = true)
			{
				this.GM = _GM;
				this.wait_until_gm_close = _wait_until_gm_close;
			}

			public virtual bool EvtWait(bool is_first = false)
			{
				if (is_first)
				{
					this.GM.M2D.FlagOpenGm.Rem("EV");
				}
				if (is_first && (this.wait_until_gm_close || !this.GM.isActive()))
				{
					this.GM.need_one_activate_for_wait = true;
				}
				if (this.GM.need_one_activate_for_wait || (this.wait_until_gm_close && this.GM.isActive()))
				{
					return true;
				}
				if (!this.wait_until_gm_close)
				{
					this.GM.check_evt_handle_mode = true;
					this.GM.checkHandle();
				}
				return false;
			}

			private readonly UiGameMenu GM;

			public readonly bool wait_until_gm_close;
		}

		internal class WaitListenerGMOpenTab : IEventWaitListener
		{
			internal WaitListenerGMOpenTab(UiGameMenu _GM, CATEG _categ)
			{
				this.GM = _GM;
				this.categ = _categ;
				this.GM.waiting_categ_for = this.categ;
			}

			public virtual bool EvtWait(bool is_first = false)
			{
				return this.GM.edit_categ != this.categ;
			}

			private readonly UiGameMenu GM;

			private readonly CATEG categ;
		}

		public class WaitListenerGMOpenSkillTab : IEventWaitListener
		{
			public WaitListenerGMOpenSkillTab(UiGameMenu _GM, SkillManager.SKILL_CTG _categ)
			{
				this.GM = _GM;
				this.categ = _categ;
				UiSkillManageBox.event_focus_waiting_tab = (uint)this.categ;
			}

			public virtual bool EvtWait(bool is_first = false)
			{
				UiGMCSkill uiGMCSkill = this.GM.AGmcCache[3] as UiGMCSkill;
				return uiGMCSkill == null || !uiGMCSkill.isTabOpening(this.categ);
			}

			private readonly UiGameMenu GM;

			private readonly SkillManager.SKILL_CTG categ;
		}

		public class WaitListenerGMSkillEnable : IEventWaitListener
		{
			public WaitListenerGMSkillEnable(UiGameMenu _GM, PrSkill[] _ASkill)
			{
				this.GM = _GM;
				this.ASkill = _ASkill;
			}

			public virtual bool EvtWait(bool is_first = false)
			{
				for (int i = this.ASkill.Length - 1; i >= 0; i--)
				{
					if (!this.ASkill[i].enabled)
					{
						return true;
					}
				}
				return false;
			}

			private readonly UiGameMenu GM;

			private readonly PrSkill[] ASkill;
		}

		public class WaitListenerGMItemChoose : IEventWaitListener
		{
			public WaitListenerGMItemChoose(UiGameMenu _GM, NelItem Itm, string _var_name, bool _alloc_other_selection = false)
			{
				this.GM = _GM;
				this.GM.category_to_quit = true;
				this.TargetItm = Itm;
				this.alloc_other_selection = _alloc_other_selection;
				this.GMItm = this.GM.AGmcCache[1] as UiGMCItem;
				this.var_name = _var_name;
				EV.getVariableContainer().define(this.var_name, "0", true);
				if (this.GMItm != null)
				{
					this.GMItm.attachCommandPrepareFn(new UiItemManageBox.FnCommandPrepare(this.fnItemChoosedImmediately));
					if (!this.alloc_other_selection)
					{
						this.GMItm.attachRowInitAfterFn(new UiItemManageBox.FnItemRowInitAfter(this.fnRowInitAfterNotTarget), true);
					}
				}
			}

			public bool EvtWait(bool is_first = false)
			{
				return this.GMItm != null && (is_first || (this.GM.isActive() && this.GM.edit_categ == CATEG.ITEM));
			}

			public bool fnItemChoosedImmediately(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow RowB)
			{
				NelItem itemData = RowB.getItemData();
				if (!this.alloc_other_selection && itemData != this.TargetItm)
				{
					SND.Ui.play("locked", false);
					CURS.limitVib(RowB, AIM.R);
					return false;
				}
				EV.getVariableContainer().define(this.var_name, (itemData == this.TargetItm) ? "1" : "0", true);
				this.GM.deactivate(false);
				return false;
			}

			public void fnRowInitAfterNotTarget(aBtnItemRow B, ItemStorage.IRow IRow)
			{
				B.SetLocked(IRow.Data != this.TargetItm, true, false);
			}

			private readonly UiGameMenu GM;

			private readonly UiGMCItem GMItm;

			private readonly NelItem TargetItm;

			private string var_name;

			public bool alloc_other_selection;
		}
	}
}
