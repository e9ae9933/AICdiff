﻿using System;
using XX;

namespace nel.gm
{
	internal class UiGMCScenario : UiGMC
	{
		internal UiGMCScenario(UiGameMenu _GM)
			: base(_GM, CATEG.SCENARIO, true, 0, 0, 0, 0, 1f, 1f)
		{
			this.AEvCon = new Designer.EvacuateContainer[3];
		}

		public override bool initAppearMain()
		{
			if (base.initAppearMain())
			{
				return true;
			}
			base.M2D.QUEST.fineAutoItemCollection(false);
			this.BxR.item_margin_x_px = 0f;
			this.BxR.item_margin_y_px = 4f;
			this.BxR.init();
			this.RTabBar = ColumnRowNel.NCreateT<aBtnNel>(this.BxR, "ctg_tab", "row_tab", (int)UiGMCScenario.scn_tab, FEnum<UiGMCScenario.SCN_CTG>.ToStrListUp(3, "&&Scenario_Tab_", true), new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnScnTabChanged), this.BxR.use_w, 0f, false, false);
			this.RTabBar.setNewIcon(1, base.M2D.QUEST.hasNewProg(QuestTracker.CATEG.MAIN)).setNewIcon(2, base.M2D.QUEST.hasNewProg(QuestTracker.CATEG.SUB)).LrInput(false);
			this.BxR.Br();
			this.MainTab = this.BxR.addTab("scn_area", this.BxR.use_w, this.BxR.use_h - 10f, this.BxR.use_w, this.BxR.use_h - 10f, true);
			this.MainTab.use_scroll = true;
			this.BxR.endTab(false);
			this.initScnTab();
			return true;
		}

		internal override void initEdit()
		{
			if (UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.MAIN_QUEST || UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.SUB_QUEST)
			{
				if (!base.M2D.QUEST.selectGmFirstButton() && this.RTabBar != null)
				{
					this.RTabBar.Get((int)UiGMCScenario.scn_tab).Select(false);
					return;
				}
			}
			else
			{
				this.DsShowerTextLog.getScrollBox().BView.Select(false);
			}
		}

		internal override void quitEdit()
		{
			if (aBtn.PreSelected != null)
			{
				aBtn.PreSelected.Deselect(true);
			}
			this.fineFocusedQProg();
			this.clearMapCancelingJump();
		}

		internal override void releaseEvac()
		{
			base.M2D.QUEST.releaseDesigner();
			for (int i = this.AEvCon.Length - 1; i >= 0; i--)
			{
				this.releaseEvac(ref this.AEvCon[i]);
			}
			base.releaseEvac();
		}

		public bool fnScnTabChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (!this.GM.isShowingGMC(this))
			{
				return false;
			}
			UiGMCScenario.SCN_CTG scn_CTG;
			FEnum<UiGMCScenario.SCN_CTG>.TryParse(TX.slice(_B.Get(cur_value).title, "&&Scenario_Tab_".Length).ToUpper(), out scn_CTG, true);
			if (UiGMCScenario.scn_tab != scn_CTG)
			{
				this.AEvCon[(int)UiGMCScenario.scn_tab] = new Designer.EvacuateContainer(this.MainTab, false);
				this.fineFocusedQProg();
				UiGMCScenario.scn_tab = scn_CTG;
				this.initScnTab();
			}
			return true;
		}

		internal void initScnTab(UiGMCScenario.SCN_CTG categ)
		{
			if (UiGMCScenario.scn_tab != categ)
			{
				this.RTabBar.setValue((int)categ, true);
			}
		}

		public void initScnTab()
		{
			this.fineFocusedQProg();
			Designer mainTab = this.MainTab;
			base.M2D.QUEST.releaseDesigner();
			this.MapCancelingQuestJump = null;
			mainTab.Clear();
			if (base.reassignEvacuated(ref this.AEvCon[(int)UiGMCScenario.scn_tab], null))
			{
				if (UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.MAIN_QUEST || UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.SUB_QUEST)
				{
					base.M2D.QUEST.reassign(mainTab, mainTab.getTab("-Quest"), this.tab2categ);
				}
			}
			else
			{
				mainTab.use_scroll = false;
				mainTab.scroll_area_selectable = false;
				mainTab.Small();
				mainTab.init();
				mainTab.effect_confusion = false;
				if (UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.LOG)
				{
					NelEvTextLog nelEvTextLog = NEL.createTextLog();
					this.DsShowerTextLog = nelEvTextLog.createContainer(mainTab, mainTab.use_h - 50f, -1);
					this.GM.EditFocusInitTo = this.DsShowerTextLog.getScrollBox().BView;
					mainTab.Br().addP(new DsnDataP("", false)
					{
						html = true,
						swidth = mainTab.use_w,
						size = 14f,
						text = TX.Get("TextLog_Desc", ""),
						TxCol = C32.d2c(4283780170U)
					}, false);
				}
				else
				{
					mainTab.item_margin_y_px = 1f;
					mainTab.margin_in_tb = 3f;
					base.M2D.QUEST.createSortButton(mainTab);
					mainTab.addHr(new DsnDataHr
					{
						Col = C32.d2c(4283780170U),
						swidth = mainTab.use_w,
						line_height = 1f,
						draw_width_rate = 1f,
						margin_t = 4f,
						margin_b = 3f
					});
					Designer designer = mainTab.addTab("-Quest", mainTab.use_w, mainTab.use_h - 10f, mainTab.use_w, 180f, true);
					designer.Smallest();
					designer.scrolling_margin_in_tb = 10f;
					designer.scrolling_margin_in_lr = 6f;
					designer.stencil_ref = 230;
					designer.item_margin_y_px = 4f;
					designer.margin_in_tb = 4f;
					QuestTracker.CATEG tab2categ = this.tab2categ;
					base.M2D.QUEST.createGmUi(designer, tab2categ, new FnBtnBindings(this.fnClickQuestBtn), new UiQuestCard.FnQuestBtnTouched(this.fnQuestBtnTouched));
					mainTab.endTab(true);
				}
			}
			if (this.GM.isEditState())
			{
				this.BxR.Focus();
				this.initEdit();
			}
		}

		internal override GMC_RES runEdit(float fcnt, bool handle)
		{
			if (!handle)
			{
				return GMC_RES.CONTINUE;
			}
			if (this.RTabBar != null)
			{
				this.RTabBar.runLRInput(-2);
			}
			if ((UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.MAIN_QUEST || UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.SUB_QUEST) && IN.isUiSortPD())
			{
				base.M2D.QUEST.progressSortByLshKey();
			}
			return GMC_RES.CONTINUE;
		}

		public bool fnClickQuestBtn(aBtn B)
		{
			string title = B.title;
			if (title != null)
			{
				if (!(title == "reveal_map"))
				{
					if (!(title == "quest_tracking"))
					{
					}
				}
				else
				{
					UiQuestCard tabForButton = base.M2D.QUEST.getTabForButton(B);
					if (tabForButton == null)
					{
						return false;
					}
					QuestTracker.Quest quest = tabForButton.getQuest();
					QuestTracker.QuestDeperture currentDepert = tabForButton.getCurrentDepert();
					if (!currentDepert.isActiveMap())
					{
						return false;
					}
					this.GM.clearQuestCancelingJump();
					UiGMCMap uiGMCMap = this.GM.initCategoryEdit(CATEG.MAP, true) as UiGMCMap;
					if (uiGMCMap != null)
					{
						this.MapCancelingQuestJump = quest;
						uiGMCMap.reveal(currentDepert.WmDepert, false);
					}
				}
			}
			return true;
		}

		public void fnQuestBtnTouched(aBtn B, UiQuestCard Tab)
		{
			this.fineFocusedQProg();
			this.PreFocusedQProg = Tab;
		}

		private void fineFocusedQProg()
		{
			if (this.PreFocusedQProg != null)
			{
				QuestTracker.QuestProgress prog = this.PreFocusedQProg.Prog;
				if (prog.new_icon)
				{
					prog.new_icon = false;
					try
					{
						this.PreFocusedQProg.need_redraw_background = true;
						if (this.RTabBar != null)
						{
							if (UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.MAIN_QUEST && !base.M2D.QUEST.hasNewProg(QuestTracker.CATEG.MAIN))
							{
								this.RTabBar.setNewIcon(1, false);
								this.GM.refineLeftCategoriesHilight();
							}
							if (UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.SUB_QUEST && !base.M2D.QUEST.hasNewProg(QuestTracker.CATEG.SUB))
							{
								this.RTabBar.setNewIcon(2, false);
								this.GM.refineLeftCategoriesHilight();
							}
						}
					}
					catch
					{
					}
				}
				this.PreFocusedQProg = null;
			}
		}

		private QuestTracker.CATEG tab2categ
		{
			get
			{
				if (UiGMCScenario.scn_tab == UiGMCScenario.SCN_CTG.MAIN_QUEST)
				{
					return QuestTracker.CATEG.MAIN;
				}
				return (QuestTracker.CATEG)(-2);
			}
		}

		public bool executeMapCancelingJump()
		{
			if (this.MapCancelingQuestJump != null)
			{
				this.GM.initCategoryEdit(this.categ, false);
				if (this.GM.isShowingGMC(this))
				{
					base.M2D.QUEST.getTabForQuest(this.MapCancelingQuestJump);
					return true;
				}
			}
			return false;
		}

		public void clearMapCancelingJump()
		{
			this.MapCancelingQuestJump = null;
		}

		private QuestTracker.Quest MapCancelingQuestJump;

		internal static UiGMCScenario.SCN_CTG scn_tab;

		private Designer MainTab;

		private UiQuestCard PreFocusedQProg;

		private ColumnRowNel RTabBar;

		private Designer DsShowerTextLog;

		protected Designer.EvacuateContainer[] AEvCon;

		internal enum SCN_CTG
		{
			LOG,
			MAIN_QUEST,
			SUB_QUEST,
			_MAX
		}
	}
}
