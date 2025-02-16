using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel.gm
{
	internal class UiGMCItem : UiGMC
	{
		internal UiGMCItem(UiGameMenu _GM, UiBoxDesigner _BxCmd, UiBoxDesigner _BxItmv, UiItemManageBoxSlider _ItemMng, UiItemManageBoxSlider _ItemMngMv)
			: base(_GM, CATEG.ITEM, false, 2, 1, 0, 0, 1f, 1f)
		{
			this.BxCmd = _BxCmd;
			this.BxItmv = _BxItmv;
			this.IMNG = base.M2D.IMNG;
			this.ItemMng = _ItemMng;
			this.ItemMngMv = _ItemMngMv;
			this.APoolEvacuated = this.GM.APoolEvacuatedItem;
		}

		public override bool initAppearMain()
		{
			if (base.initAppearMain())
			{
				this.fineTopArea();
				this.RTabBar.lr_input = true;
				return true;
			}
			base.M2D.QUEST.fineAutoItemCollection(false);
			this.BxR.item_margin_x_px = 0f;
			this.BxR.item_margin_y_px = 0f;
			this.RTabBar = ColumnRowNel.NCreateT<aBtnNel>(this.BxR, "ctg_tab", "row_tab", (int)UiGMCItem.item_tab, FEnum<UiGMCItem.ITEM_CTG>.ToStrListUp(3, "&&Item_Tab_", true), new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnItemTabChanged), this.BxR.use_w, 0f, false, false);
			if (!base.M2D.canAccesableToHouseInventory())
			{
				this.RTabBar.Get(1).setSkinTitle(TX.Get("Inventory_reel_booster", ""));
			}
			this.TabR = this.BxR.addTab("status_area", this.BxR.use_w, this.BxR.use_h - 10f, this.BxR.use_w, this.BxR.use_h - 25f, false);
			this.TabR.Smallest();
			this.TabR.margin_in_tb = 6f;
			this.BxR.endTab(false);
			this.FbKD = this.BxR.Br().addP(new DsnDataP("", false)
			{
				html = true,
				name = "kd_itemsel",
				text = "\u3000",
				size = 14f,
				swidth = this.BxR.use_w,
				TxCol = C32.d2c(4283780170U)
			}, false);
			this.initItemTab(true, false);
			return true;
		}

		internal override void initAppearWhole()
		{
			base.initAppearWhole();
			this.fineTopArea();
		}

		protected override bool initAppearSubAreaInner(UiBoxDesigner Ds, int i, bool is_top)
		{
			Ds.alignx = ALIGN.CENTER;
			if (base.initAppearSubAreaInner(Ds, i, is_top))
			{
				return true;
			}
			if (i == 0)
			{
				Ds.add(new DsnDataP("", false)
				{
					name = "money",
					text = " ",
					swidth = Ds.use_w - 20f,
					alignx = ALIGN.LEFT,
					size = 20f,
					html = true,
					TxCol = C32.d2c(4283780170U)
				});
			}
			else
			{
				Ds.add(new DsnDataP("", false)
				{
					name = "bottle",
					text = " ",
					swidth = Ds.use_w - 20f,
					alignx = ALIGN.LEFT,
					size = 20f,
					html = true,
					TxCol = C32.d2c(4283780170U)
				});
			}
			return true;
		}

		private STB money_counter_string(STB Stb)
		{
			Stb.AddTxA("Money_Counter", false).TxRpl(CoinStorage.getCount(CoinStorage.CTYPE.GOLD));
			return Stb;
		}

		private STB bottle_stock_string(STB Stb)
		{
			Stb.AddTxA("EmptyBottle_Counter", false).TxRpl(this.IMNG.countEmptyBottle()).TxRpl(this.IMNG.countBottle().ToString());
			return Stb;
		}

		private void fineTopArea()
		{
			UiBoxDesigner uiBoxDesigner = this.GM.TopTab.GetDesigner(0);
			using (STB stb = TX.PopBld(null, 0))
			{
				FillBlock fillBlock;
				if (uiBoxDesigner != null && (fillBlock = uiBoxDesigner.Get("money", false) as FillBlock) != null)
				{
					fillBlock.Txt(this.money_counter_string(stb.Clear()));
				}
				uiBoxDesigner = this.GM.TopTab.GetDesigner(1);
				if (uiBoxDesigner != null && (fillBlock = uiBoxDesigner.Get("bottle", false) as FillBlock) != null)
				{
					fillBlock.Txt(this.bottle_stock_string(stb.Clear()));
				}
			}
		}

		public override void quitAppear()
		{
			this.quitUiReel(false);
			this.quitLunch(false);
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Rem("ITEMMOVE");
			}
			this.StReelSpliced = null;
			base.quitAppear();
		}

		internal override void initEdit()
		{
			this.BxDesc.WH(UiGMCItem.desc_item_w, UiGMC.bounds_h - base.right_last_top_row_height - base.right_last_btm_row_height);
			this.initItemTab(false, true);
			if (this.ItemMng.Inventory != null)
			{
				this.fineSafeAreaKd((this.ItemMng.Inventory.SelectedRow != null) ? this.ItemMng.Inventory.SelectedRow.getItemData() : null);
			}
			this.BxDesc.positionD(-UiGMC.bounds_wh + UiGMCItem.desc_item_w / 2f - 20f, this.GM.BXR_Y_TRANSLATED, 0, 30f);
			this.GM.item_modified = true;
			this.istate = UiGameMenu.STATE.EDIT;
		}

		internal override void quitEdit()
		{
			if (this.istate == UiGameMenu.STATE.ITEMMOVE)
			{
				this.quitItemMove();
			}
			this.FnSpecialCommandPrepare = null;
			this.FnSpecialItemRowInitAfter = null;
			this.GM.FlgStatusHide.Rem("ITEM");
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Rem("ITEMMOVE");
			}
			this.quitWLinkAttachmentChoose();
			this.ItemMng.blurDesc();
			this.ItemMngMv.blurDesc();
			base.M2D.WDR.setFineBellFlag();
		}

		public void attachCommandPrepareFn(UiItemManageBox.FnCommandPrepare FD_CommandPrepare)
		{
			UiItemManageBox itemMng = this.ItemMng;
			this.FnSpecialCommandPrepare = FD_CommandPrepare;
			itemMng.fnCommandPrepare = FD_CommandPrepare;
		}

		public void deattachCommandPrepareFn(UiItemManageBox.FnCommandPrepare FD_CommandPrepare)
		{
			if (this.ItemMng.fnCommandPrepare == FD_CommandPrepare)
			{
				this.ItemMng.fnCommandPrepare = null;
			}
			if (this.FnSpecialCommandPrepare == FD_CommandPrepare)
			{
				this.FnSpecialCommandPrepare = null;
			}
		}

		public void attachRowInitAfterFn(UiItemManageBox.FnItemRowInitAfter FD_RowInitAfter, bool execute_to_exist_rows = true)
		{
			UiItemManageBox itemMng = this.ItemMng;
			this.FnSpecialItemRowInitAfter = FD_RowInitAfter;
			itemMng.fnItemRowInitAfter = FD_RowInitAfter;
			if (execute_to_exist_rows && FD_RowInitAfter != null && this.ItemMng.Inventory != null)
			{
				int itemRowBtnCount = this.ItemMng.Inventory.getItemRowBtnCount();
				for (int i = 0; i < itemRowBtnCount; i++)
				{
					aBtnItemRow itemRowBtnByIndex = this.ItemMng.Inventory.getItemRowBtnByIndex(i);
					if (itemRowBtnByIndex != null)
					{
						this.ItemMng.fnItemRowInitAfter(itemRowBtnByIndex, itemRowBtnByIndex.getItemRow());
					}
				}
			}
		}

		public void deattachRowInitAfterFn(UiItemManageBox.FnItemRowInitAfter FD_RowInitAfter)
		{
			if (this.ItemMng.fnItemRowInitAfter == FD_RowInitAfter)
			{
				this.ItemMng.fnItemRowInitAfter = null;
			}
			if (this.FnSpecialItemRowInitAfter == FD_RowInitAfter)
			{
				this.FnSpecialItemRowInitAfter = null;
			}
		}

		internal override void releaseEvac()
		{
			if (this.ItemMng.Inventory != null)
			{
				this.APoolEvacuated = this.ItemMng.Inventory.EvacuateButtonsFromPreviousManager(true) ?? this.APoolEvacuated;
			}
			if (this.ItemMngMv.Inventory != null)
			{
				this.ItemMngMv.Inventory.EvacuateButtonsFromPreviousManager(true);
			}
			this.ItemMng.quitDesigner(false, false);
			this.ItemMngMv.quitDesigner(false, false);
			this.ItemMng.clearFn();
			this.ItemMngMv.clearFn();
			if (this.GM.APoolEvacuatedItem == null || this.GM.APoolEvacuatedItem == this.APoolEvacuated)
			{
				this.GM.APoolEvacuatedItem = this.APoolEvacuated;
			}
			else if (this.APoolEvacuated != null)
			{
				for (int i = this.APoolEvacuated.Count - 1; i >= 0; i--)
				{
					if (this.APoolEvacuated[i] != null)
					{
						IN.DestroyE(this.APoolEvacuated[i].gameObject);
					}
				}
				this.APoolEvacuated = null;
			}
			this.quitUiReel(false);
			this.quitLunch(false);
			if (this.IMvCon != null)
			{
				this.IMvCon.destruct();
				this.IMvCon = null;
			}
			this.StReelSpliced = null;
			base.releaseEvac();
		}

		private void initItemTab(bool clear = false, bool on_edit = false)
		{
			UiItemManageBoxSlider itemMng = this.ItemMng;
			if (clear)
			{
				if (itemMng.Inventory != null)
				{
					this.APoolEvacuated = itemMng.Inventory.EvacuateButtonsFromPreviousManager(true) ?? this.APoolEvacuated;
				}
				this.TabR.Clear();
				itemMng.quitDesigner(false, false);
			}
			bool flag = base.M2D.canAccesableToHouseInventory();
			if (UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.HOUSE && !flag)
			{
				if (clear)
				{
					this.initReelList(this.TabR);
					this.fineSafeAreaKd(null);
				}
				if (on_edit)
				{
					this.BxCmd.deactivate();
					this.prepareDescForReelList();
					this.BxDesc.activate();
					this.selectReelList();
				}
				return;
			}
			itemMng.Pr = null;
			itemMng.fnRunUsePost = null;
			itemMng.item_row_skin = "normal";
			itemMng.fnCommandPrepare = this.FnSpecialCommandPrepare;
			itemMng.fnItemRowInitAfter = this.FnSpecialItemRowInitAfter;
			itemMng.fnItemRowRemakedAfter = null;
			itemMng.fnDetailPrepare = new UiItemManageBox.FnDetailPrepare(this.fnHoverItemRow);
			itemMng.fnWholeRowsPrepare = null;
			itemMng.fnGradeFocusChange = null;
			itemMng.fnDescAddition = new UiItemManageBox.FnDescAddition(UiGMCItem.fnDescAddition);
			itemMng.manager_auto_run_on_select_row = false;
			itemMng.DefaultTargetInventory = null;
			itemMng.topright_desc_width = 0f;
			itemMng.APoolEvacuated = this.APoolEvacuated ?? itemMng.APoolEvacuated;
			itemMng.row_height = 30f;
			UiBoxDesigner uiBoxDesigner = null;
			itemMng.slice_height = 10f;
			itemMng.topright_desc_width = 60f;
			itemMng.use_topright_counter = false;
			if (on_edit)
			{
				this.BxCmd.deactivate();
				uiBoxDesigner = this.BxDesc;
				this.GM.initBxCmdClearing();
				itemMng.Pr = this.Pr;
				itemMng.fnRunUsePost = new Func<string, NelItem, ItemStorage.ObtainInfo, bool, bool>(this.ItemUsePost);
				itemMng.fnCommandKeysPrepare = new UiItemManageBox.FnCommandKeysPrepare(this.fnItemCommandKeysPrepare);
				itemMng.fnCommandBtnExecuted = new UiItemManageBox.FnCommandBtnExecuted(this.fnItemCommandExecuted);
			}
			UiGMCItem.ITEM_CTG item_CTG = UiGMCItem.item_tab;
			ItemStorage itemStorage;
			if (item_CTG != UiGMCItem.ITEM_CTG.HOUSE)
			{
				if (item_CTG == UiGMCItem.ITEM_CTG.PRECIOUS)
				{
					itemStorage = this.IMNG.getInventoryPrecious();
					itemMng.fnWholeRowsPrepare = new UiItemManageBox.FnWholeRowsPrepare(this.IMNG.prepareDefaultPreciousRows);
				}
				else
				{
					itemMng.use_topright_counter = true;
					itemStorage = this.IMNG.getInventory();
				}
			}
			else
			{
				itemStorage = this.IMNG.getHouseInventory();
			}
			if (this.istate == UiGameMenu.STATE._MAP_MARKER)
			{
				this.initMngForWLinkAttachmentChoose(false);
			}
			itemMng.cmd_w = 320f;
			itemMng.effect_confusion = base.effect_confusion;
			itemMng.title_text_content = "";
			itemMng.stencil_ref = base.bxr_stencil_default;
			itemMng.do_not_remake_desc_box = false;
			itemMng.ParentBoxDesigner = this.BxR;
			itemMng.initDesigner(itemStorage, this.TabR, uiBoxDesigner, this.BxCmd, false, on_edit, true);
			if (this.APoolEvacuated == null)
			{
				this.APoolEvacuated = itemMng.APoolEvacuated;
			}
			if (this.istate == UiGameMenu.STATE._MAP_MARKER)
			{
				this.selectFirstAppearLine();
			}
		}

		private void fineSafeAreaKd(NelItem Itm)
		{
			IVariableObject variableObject = this.BxR.Get("kd_itemsel", false);
			if (variableObject == null)
			{
				return;
			}
			bool flag = base.M2D.canAccesableToHouseInventory();
			using (STB stb = TX.PopBld(null, 0))
			{
				if (flag)
				{
					stb.Add(this.safe_area_initialize_enable ? TX.Get("KD_start_manage", "") : "\u3000");
				}
				if (UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.MAIN && Itm != null && Itm.useable)
				{
					stb.AppendTxA("KD_ItemSel_assign_start", " ");
				}
				if (this.IMNG.has_recipe_collection)
				{
					stb.AppendTxA("KD_go_to_def_in_catalog", " ");
				}
				variableObject.setValue(stb.ToString());
			}
		}

		public void fnHoverItemRow(NelItem Itm, ItemStorage.ObtainInfo Obt, ItemStorage.IRow IR)
		{
			this.fineSafeAreaKd(Itm);
		}

		private bool safe_area_initialize_enable
		{
			get
			{
				return this.GM.isEditState() && (UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.HOUSE || UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.MAIN) && !this.ItemMng.isUsingState();
			}
		}

		public static string fnDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW desc_row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int _default_count)
		{
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (desc_row == UiItemManageBox.DESC_ROW.NAME && nelM2DBase.canAccesableToHouseInventory())
			{
				def_string = def_string + "  <font size=\"12\">" + UiGMCItem.getInventoryCountString(Itm, -1, 3) + "</font>";
			}
			return def_string;
		}

		public static string getInventoryCountString(NelItem Itm, int grade, int using_iv_bits = 3)
		{
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				UiGMCItem.getInventoryCountString(stb, Itm, grade, using_iv_bits);
				text = stb.ToString();
			}
			return text;
		}

		public static STB getInventoryCountString(STB Stb, NelItem Itm, int grade, int using_iv_bits = 3)
		{
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			STB stb2;
			using (STB stb = TX.PopBld(null, 0))
			{
				for (int i = 0; i < 2; i++)
				{
					if ((using_iv_bits & (1 << i)) != 0)
					{
						if (stb.Length > 0)
						{
							stb.Add('/');
						}
						ItemStorage itemStorage = ((i == 0) ? nelM2DBase.IMNG.getInventory() : nelM2DBase.IMNG.getHouseInventory());
						string text = ((i == 0) ? "IconNoel0" : "house_inventory");
						int reduceable = itemStorage.getReduceable(Itm, grade);
						stb.Add("<img mesh=\"", text, "\" width=\"22\" height=\"24\" />x").Add(reduceable);
					}
				}
				stb2 = Stb.AddTxA("inventory_count", false).TxRpl(stb);
			}
			return stb2;
		}

		private bool fnItemTabChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (this.ItemMng.isUsingState())
			{
				return false;
			}
			UiGMCItem.ITEM_CTG item_CTG;
			FEnum<UiGMCItem.ITEM_CTG>.TryParse(TX.slice(_B.Get(cur_value).title, "&&Item_Tab_".Length).ToUpper(), out item_CTG, true);
			if (UiGMCItem.item_tab != item_CTG)
			{
				UiGMCItem.item_tab = item_CTG;
				this.initItemTab(true, this.GM.isEditState());
			}
			return true;
		}

		internal override GMC_RES runEdit(float fcnt, bool handle)
		{
			if (this.LunchTime != null)
			{
				if (!this.LunchTime.isActive())
				{
					this.quitLunch(true);
				}
				else
				{
					this.LunchTime.runIRD(fcnt);
				}
				return GMC_RES.CONTINUE;
			}
			if (this.UiReel != null)
			{
				if (!this.UiReel.isActive())
				{
					this.quitUiReel(true);
				}
				return GMC_RES.CONTINUE;
			}
			if (this.istate == UiGameMenu.STATE.ITEMMOVE)
			{
				if (this.IMvCon == null || !this.IMvCon.runItemMove(fcnt))
				{
					this.quitItemMove();
				}
				return GMC_RES.CONTINUE;
			}
			bool flag = false;
			if (handle && base.M2D.isRbkPD())
			{
				this.quitWLinkAttachmentChoose();
				bool flag2;
				if (this.isEdittingReelList())
				{
					flag2 = this.GM.initRecipeBook("Item", null);
				}
				else
				{
					flag2 = this.GM.initRecipeBook((this.ItemMng.Inventory.SelectedRow != null) ? this.ItemMng.Inventory.SelectedRow.getItemData() : null, null);
				}
				if (flag2)
				{
					return GMC_RES.QUIT_GM;
				}
			}
			if (IN.isUiSortPD())
			{
				flag = true;
			}
			if (this.isEdittingReelList())
			{
				this.RTabBar.lr_input = true;
				if (!this.runEditReelList())
				{
					return GMC_RES.BACK_CATEGORY;
				}
			}
			else
			{
				bool flag3 = true;
				if (this.istate == UiGameMenu.STATE._MAP_MARKER)
				{
					flag3 = base.M2D.canAccesableToHouseInventory() || this.ItemMng.Inventory != this.IMNG.getInventory();
				}
				flag3 = flag3 && !this.ItemMng.isUsingState();
				if (this.istate == UiGameMenu.STATE._USEITEMSEL)
				{
					if (!this.runEditUSel(null))
					{
						this.BxCmd.deactivate();
					}
					else
					{
						flag3 = false;
					}
				}
				else
				{
					if (this.ItemMng.Inventory == this.IMNG.getInventory())
					{
						aBtnItemRow aBtnItemRow = (this.ItemMng.isUsingState() ? this.ItemMng.getUsingRowBtn() : this.ItemMng.getSelectingRowBtn());
						if (aBtnItemRow != null && aBtnItemRow.getItemData().useable && IN.isItmU(1) && this.runEditUSel(aBtnItemRow))
						{
							this.RTabBar.lr_input = false;
							return GMC_RES.CONTINUE;
						}
					}
					if (!flag && IN.isUiAddPD() && base.M2D.canAccesableToHouseInventory() && this.safe_area_initialize_enable)
					{
						if (this.IMNG.canSwitchItemMove() && this.FnSpecialCommandPrepare == null)
						{
							this.initItemMove();
						}
						else
						{
							SND.Ui.play("locked", false);
						}
						return GMC_RES.CONTINUE;
					}
					if (!this.ItemMng.runEditItem())
					{
						if (this.ItemMng.quit_whole_ui)
						{
							return GMC_RES.QUIT_GM;
						}
						if (this.istate != UiGameMenu.STATE._MAP_MARKER)
						{
							return GMC_RES.BACK_CATEGORY;
						}
						this.quitWLinkAttachmentChoose();
					}
				}
				this.RTabBar.lr_input = flag3;
			}
			if (!this.ItemMng.quit_whole_ui)
			{
				return GMC_RES.CONTINUE;
			}
			return GMC_RES.QUIT_GM;
		}

		private bool ItemUsePost(string cmd_key, NelItem Itm, ItemStorage.ObtainInfo Obt, bool need_repos_cmd)
		{
			if (Itm == NelItem.Bottle || (Itm.is_water && need_repos_cmd))
			{
				this.fineTopArea();
			}
			if (Itm.is_water)
			{
				this.GM.resetEpStatUI();
			}
			if (cmd_key == "drink")
			{
				this.GM.resetStomachUI();
				this.GM.resetEpStatUI();
			}
			return true;
		}

		private List<string> fnItemCommandKeysPrepare(UiBoxDesigner BxCmd, aBtnItemRow BRow, List<string> Adefault)
		{
			NelItem itemData = BRow.getItemData();
			BRow.getItemInfo();
			string text = itemData.key;
			if (text != null && text == "recipe_collection")
			{
				this.GM.initRecipeBook(itemData, null);
				return null;
			}
			this.GM.need_cmd_remake = true;
			BxCmd.WH(1f, 1f);
			BxCmd.wh_animZero(true, true);
			if (itemData.is_food && !itemData.is_water)
			{
				Adefault.Insert(0, "lunchtime");
				if (BRow.getItemRow().has_wlink && this.ItemMng.Inventory == this.IMNG.getInventory())
				{
					Adefault.Insert(1, "takeout_food");
					Adefault.Remove("drop");
				}
				else
				{
					Adefault.Insert(1, "pack_in_box");
				}
			}
			else if (itemData == NelItem.LunchBox && !BRow.getItemRow().has_wlink)
			{
				Adefault.Insert(0, "pack_lunch");
			}
			if (itemData.is_reelmbox)
			{
				Adefault.Insert(0, "reel_open_once");
				Adefault.Insert(1, "reel_open_all");
			}
			text = itemData.key;
			if (text != null)
			{
				if (!(text == "special_inventory0"))
				{
					if (text == "scapecat")
					{
						if (base.M2D.GameOver != null && base.M2D.GameOver.isScapecatEnabled())
						{
							Adefault.Insert(0, "respawn");
						}
						else
						{
							Adefault.Insert(0, "respawn_useless");
						}
					}
				}
				else
				{
					Adefault.Insert(0, "inventory_expand");
				}
			}
			int num = Adefault.IndexOf("Cancel");
			if (itemData.useable && UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.MAIN)
			{
				Adefault.Insert(num++, "itemsel");
			}
			if (base.M2D.canAccesableToHouseInventory() && (UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.MAIN || UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.HOUSE))
			{
				Adefault.Insert(num++, "move");
			}
			return Adefault;
		}

		private void fnItemCommandExecuted(aBtnItemRow BRow, string s)
		{
			NelItem itemData = BRow.getItemData();
			ItemStorage.ObtainInfo itemInfo = BRow.getItemInfo();
			if (!(s == "itemsel") && !(s == "move") && this.ItemMng.canUseItemCheck(itemData, true))
			{
				return;
			}
			string text = null;
			if (s != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(s);
				if (num <= 1773637334U)
				{
					if (num <= 881576750U)
					{
						if (num != 407568404U)
						{
							if (num != 881576750U)
							{
								goto IL_0759;
							}
							if (!(s == "lunchtime"))
							{
								goto IL_0759;
							}
							if (EnemySummoner.isActiveBorder() && !UiLunchTimeBase.isUseable(itemData))
							{
								this.ItemMng.errorMessageToDesc(TX.Get("Alert_cannot_eat_in_battle", ""));
								return;
							}
							this.quitLunch(false);
							this.LunchTime = IN.CreateGob(this.GM.gameObject, "-Lunch").AddComponent<UiLunchTime>();
							this.LunchTime.enabled = false;
							this.LunchTime.default_select_item_key = itemData.key;
							this.LunchTime.transform.position = new Vector3(0f, 0f, this.GM.transform.localPosition.z - 0.25f);
							this.LunchTime.addExternal(this.IMNG.getInventoryIngredientArray(false));
							IN.setZ(this.BxDesc.transform, 0f);
							this.BxCmd.deactivate();
							this.GM.setPosType(UiGameMenu.POSTYPE.KEYCON);
							this.BxR.hide();
							this.BxDesc.hide();
							this.GM.FlgStatusHide.Add("ITEM");
							goto IL_0759;
						}
						else
						{
							if (!(s == "move"))
							{
								goto IL_0759;
							}
							if (this.IMNG.canSwitchItemMove())
							{
								this.ItemMng.changeStateToSelect();
								this.initItemMove();
								goto IL_0759;
							}
							SND.Ui.play("locked", false);
							goto IL_0759;
						}
					}
					else if (num != 1291299852U)
					{
						if (num != 1578023866U)
						{
							if (num != 1773637334U)
							{
								goto IL_0759;
							}
							if (!(s == "respawn_useless"))
							{
								goto IL_0759;
							}
							goto IL_06CB;
						}
						else if (!(s == "reel_open_once"))
						{
							goto IL_0759;
						}
					}
					else
					{
						if (!(s == "pack_in_box"))
						{
							goto IL_0759;
						}
						text = this.IMNG.createWlinkPack(NelItem.LunchBox, this.ItemMng.Inventory, BRow.getItemRow(), this.ItemMng.get_grade_cursor());
						if (text == null)
						{
							SND.Ui.play("pack_in_lunch_box", false);
							this.ItemMng.changeStateToSelect();
							goto IL_0759;
						}
						if (text == "empty")
						{
							text = TX.Get("Alert_no_lunchbox", "");
							goto IL_0759;
						}
						goto IL_0759;
					}
				}
				else if (num <= 2816628215U)
				{
					if (num != 2509396266U)
					{
						if (num != 2675386431U)
						{
							if (num != 2816628215U)
							{
								goto IL_0759;
							}
							if (!(s == "takeout_food"))
							{
								goto IL_0759;
							}
							text = this.IMNG.removeWLink(this.ItemMng.Inventory, BRow.getItemRow());
							if (text == null)
							{
								SND.Ui.play("reset_var", false);
								this.ItemMng.changeStateToSelect();
								goto IL_0759;
							}
							goto IL_0759;
						}
						else
						{
							if (!(s == "respawn"))
							{
								goto IL_0759;
							}
							goto IL_06CB;
						}
					}
					else if (!(s == "reel_open_all"))
					{
						goto IL_0759;
					}
				}
				else if (num != 3058141622U)
				{
					if (num != 3481680743U)
					{
						if (num != 3998009336U)
						{
							goto IL_0759;
						}
						if (!(s == "inventory_expand"))
						{
							goto IL_0759;
						}
						if (this.ItemMng.Inventory != this.IMNG.getInventory())
						{
							this.ItemMng.errorMessageToDesc(TX.Get("Alert_cannot_inventory_target_different", ""));
							goto IL_0759;
						}
						if (!this.IMNG.increaseInenvoryCapacity((int)itemData.value, 24))
						{
							this.ItemMng.errorMessageToDesc(TX.Get("Alert_cannot_inventory_enlarge", ""));
							goto IL_0759;
						}
						UIPicture.Instance.useItem(itemData, "inventory_expand");
						this.ItemMng.fineTopRightCounter();
						if (this.ItemMng.Inventory.Reduce(itemData, 1, -1, true))
						{
							this.ItemMng.changeStateToSelect();
							this.ItemMng.Inventory.fineRows(false);
							goto IL_0759;
						}
						goto IL_0759;
					}
					else
					{
						if (!(s == "pack_lunch"))
						{
							goto IL_0759;
						}
						this.ItemMng.changeStateToSelect();
						this.initWLinkAttachmentChoose(itemData, -1);
						goto IL_0759;
					}
				}
				else
				{
					if (!(s == "itemsel"))
					{
						goto IL_0759;
					}
					this.runEditUSel(this.ItemMng.getUsingRowBtn());
					goto IL_0759;
				}
				if (EnemySummoner.isActiveBorder())
				{
					this.ItemMng.errorMessageToDesc(TX.Get("Alert_reel_cannot_open_in_battle", ""));
					return;
				}
				ReelManager reelManager = this.IMNG.getReelManager();
				reelManager.clearItemReelCache();
				reelManager.destructGob();
				List<NelItemEntry> list = base.M2D.IMNG.clearItemReelProgressMem(false);
				Dictionary<NelItem, ItemStorage.ObtainInfo> wholeInfoDictionary = this.IMNG.getInventory().getWholeInfoDictionary();
				bool flag = false;
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in wholeInfoDictionary)
				{
					if (!flag && (!(s == "reel_open_once") || keyValuePair.Value == itemInfo) && keyValuePair.Key.is_reelmbox)
					{
						ReelManager.ItemReelContainer ir = ReelManager.GetIR(keyValuePair.Key);
						if (ir != null)
						{
							int num2 = 0;
							while (num2 < 5 && !flag)
							{
								int num3 = keyValuePair.Value.getCount(num2);
								while (--num3 >= 0)
								{
									reelManager.assignCurrentItemReel(ir, false);
									list.Add(new NelItemEntry(keyValuePair.Key, 1, (byte)num2));
									if (s == "reel_open_once")
									{
										flag = true;
										break;
									}
								}
								num2++;
							}
						}
					}
				}
				if (!reelManager.hasItemReelCache())
				{
					base.M2D.IMNG.clearItemReelProgressMem(true);
					return;
				}
				this.quitUiReel(false);
				this.UiReel = reelManager.initUiState(ReelManager.MSTATE.PREPARE, null, true);
				this.UiReel.create_strage = true;
				base.M2D.IMNG.initItemReelUI(this.UiReel);
				this.UiReel.prepareMBoxDrawer();
				IN.setZ(this.BxDesc.transform, 0f);
				this.BxCmd.deactivate();
				this.GM.setPosType(UiGameMenu.POSTYPE.KEYCON);
				this.BxDesc.deactivate();
				this.BxR.hide();
				this.GM.FlgStatusHide.Add("ITEM");
				goto IL_0759;
				IL_06CB:
				if (base.M2D.GameOver != null && base.M2D.GameOver.isScapecatEnabled())
				{
					int grade_cursor = this.ItemMng.get_grade_cursor();
					this.GM.deactivate(false);
					this.ItemMng.Inventory.Reduce(itemData, 1, grade_cursor, true);
					base.M2D.GameOver.executeScapecatRespawn(grade_cursor);
				}
				else
				{
					this.ItemMng.errorMessageToDesc(TX.Get("Alert_bench_execute_scenario_locked", ""));
					SND.Ui.play("locked", false);
				}
			}
			IL_0759:
			if (text != null)
			{
				this.ItemMng.errorMessageToDesc(text);
			}
		}

		private void quitLunch(bool do_not_destruct_element = false)
		{
			if (this.LunchTime != null)
			{
				this.GM.quitLunch(this.LunchTime, do_not_destruct_element);
				this.LunchTime = null;
				this.BxDesc.activate();
				this.ItemMng.changeStateToSelect();
				this.GM.FlgStatusHide.Rem("ITEM");
			}
		}

		private void quitUiReel(bool do_not_destruct_element = false)
		{
			if (this.UiReel != null)
			{
				if (!do_not_destruct_element)
				{
					this.IMNG.getReelManager().destructGob();
				}
				else
				{
					UiReelManager uiReel = this.UiReel;
				}
				this.UiReel = null;
				this.GM.FlgStatusHide.Rem("ITEM");
				if (this.GM.isEditState(this))
				{
					this.IMNG.getReelManager().digestObtainedMoney().digestObtainedItem(false);
					this.fineTopArea();
					this.GM.quitUiTemporaryHide();
					this.BxDesc.activate();
					this.ItemMng.changeStateToSelect();
				}
			}
		}

		private void initMngForWLinkAttachmentChoose(bool redo = false)
		{
			this.ItemMng.fnCommandPrepare = new UiItemManageBox.FnCommandPrepare(this.executeWLinkAttachmentChoose);
			this.ItemMng.fnCommandKeysPrepare = null;
			this.ItemMng.fnItemRowInitAfter = new UiItemManageBox.FnItemRowInitAfter(this.fnWLinkRowAfter);
			if (redo)
			{
				this.ItemMng.redoFnItemRowAfter(null);
			}
		}

		private void initWLinkAttachmentChoose(NelItem _AttachSrc, int grade = -1)
		{
			this.AttachSrc = _AttachSrc;
			this.StAttachSrc = this.ItemMng.Inventory;
			this.attach_src_grade = this.ItemMng.get_grade_cursor();
			this.istate = UiGameMenu.STATE._MAP_MARKER;
			if (this.GM.initBxCmd(this.istate, out this.BxCmd))
			{
				this.BxCmd.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
				this.BxCmd.margin_in_lr = 20f;
				this.BxCmd.margin_in_tb = 32f;
				this.BxCmd.WH(290f, 130f);
				this.BxCmd.item_margin_x_px = 20f;
				this.BxCmd.item_margin_y_px = 10f;
				this.BxCmd.selectable_loop = 2;
				this.BxCmd.alignx = ALIGN.CENTER;
				this.BxCmd.Focusable(false, true, null);
				this.BxCmd.init();
				this.BxCmd.posSetA(660f, 30f, false);
				this.BxCmd.addP(new DsnDataP("", false)
				{
					name = "wlink_p",
					text = " ",
					html = true,
					text_auto_condense = true,
					swidth = this.BxCmd.use_w,
					sheight = this.BxCmd.use_h,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					TxCol = C32.d2c(4283780170U),
					size = 18f,
					lineSpacing = 1.6f
				}, false);
			}
			else
			{
				this.BxCmd.activate();
			}
			FillBlock fillBlock = this.BxCmd.Get("wlink_p", false) as FillBlock;
			if (fillBlock != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					using (STB stb2 = TX.PopBld(null, 0))
					{
						NelItemEntry.getLocalizedNameS(stb2, this.AttachSrc, 1, grade, 0, 2, false);
						stb.AddTxA("Item_prompt_pack_lunch", false).TxRpl(stb2);
						fillBlock.Txt(stb);
					}
				}
			}
			this.initMngForWLinkAttachmentChoose(true);
			this.selectFirstAppearLine();
			this.BxR.Focus();
			this.ItemMng.unlinkCmdWindow();
		}

		private void selectFirstAppearLine()
		{
			int itemRowBtnCount = this.ItemMng.Inventory.getItemRowBtnCount();
			for (int i = 0; i < itemRowBtnCount; i++)
			{
				aBtnItemRow itemRowBtnByIndex = this.ItemMng.Inventory.getItemRowBtnByIndex(i);
				if (!itemRowBtnByIndex.isLocked())
				{
					itemRowBtnByIndex.Select(true);
					return;
				}
			}
		}

		private bool executeWLinkAttachmentChoose(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow BRow)
		{
			if (BRow.isLocked() || this.AttachSrc == null)
			{
				return false;
			}
			ItemStorage.ObtainInfo info = this.StAttachSrc.getInfo(this.AttachSrc);
			string text = this.IMNG.createWlinkPack(this.StAttachSrc, (info != null) ? info.UnlinkRow : null, this.attach_src_grade, IMng.Inventory, BRow.getItemRow(), -1);
			if (text != null)
			{
				this.ItemMng.errorMessageToDesc(text);
				return false;
			}
			SND.Ui.play("pack_in_lunch_box", false);
			this.quitWLinkAttachmentChoose();
			return false;
		}

		private void fnWLinkRowAfter(aBtnItemRow B, ItemStorage.IRow Row)
		{
			bool flag;
			B.SetLocked(Row.has_wlink || !B.getItemData().connectableWLink(this.AttachSrc, out flag), true, true);
		}

		private void quitWLinkAttachmentChoose()
		{
			if (this.istate != UiGameMenu.STATE._MAP_MARKER)
			{
				return;
			}
			this.AttachSrc = null;
			if (this.BxCmd.isActive())
			{
				this.BxCmd.posSetA(660f, -50f, 660f, 30f, true);
				this.BxCmd.deactivate();
			}
			this.ItemMng.linkCmdWindow(this.BxCmd);
			this.istate = UiGameMenu.STATE.EDIT;
			this.ItemMng.fnCommandKeysPrepare = new UiItemManageBox.FnCommandKeysPrepare(this.fnItemCommandKeysPrepare);
			this.ItemMng.fnCommandPrepare = this.FnSpecialCommandPrepare;
			this.ItemMng.fnItemRowInitAfter = this.FnSpecialItemRowInitAfter;
			int itemRowBtnCount = this.ItemMng.Inventory.getItemRowBtnCount();
			for (int i = 0; i < itemRowBtnCount; i++)
			{
				this.ItemMng.Inventory.getItemRowBtnByIndex(i).SetLocked(false, true, false);
			}
		}

		private bool runEditUSel(aBtnItemRow InitD = null)
		{
			UseItemSelector usel = this.IMNG.USel;
			if (this.ItemMng.isUsingState())
			{
				this.ItemMng.changeStateToSelect();
			}
			if (InitD != null)
			{
				this.quitWLinkAttachmentChoose();
				bool flag = false;
				if (this.GM.initBxCmd(UiGameMenu.STATE._USEITEMSEL, out this.BxCmd))
				{
					this.BxCmd.getBox().frametype = UiBox.FRAMETYPE.DARK;
					this.BxCmd.margin_in_lr = 20f;
					this.BxCmd.margin_in_tb = 52f;
					this.BxCmd.WH(540f, 640f);
					this.BxCmd.item_margin_x_px = 20f;
					this.BxCmd.item_margin_y_px = 10f;
					this.BxCmd.selectable_loop = 2;
					this.BxCmd.alignx = ALIGN.CENTER;
					this.BxCmd.init();
					this.BxCmd.positionD(420f, -20f, 2, 250f);
					flag = true;
				}
				InitD.SetChecked(true, true);
				this.BxCmd.activate();
				this.BxCmd.Focus();
				usel.initUi(this.BxCmd, InitD.getItemData(), flag);
				this.ItemMng.can_handle = false;
				this.istate = UiGameMenu.STATE._USEITEMSEL;
				return true;
			}
			if (!this.BxCmd.isActive())
			{
				this.istate = UiGameMenu.STATE.EDIT;
				return false;
			}
			if (!usel.runUi())
			{
				usel.deactivate();
				this.ItemMng.can_handle = true;
				aBtnItemRow selectingRowBtn = this.ItemMng.getSelectingRowBtn();
				if (selectingRowBtn != null)
				{
					selectingRowBtn.Select(true);
					selectingRowBtn.SetChecked(false, true);
				}
				this.BxR.Focus();
				this.istate = UiGameMenu.STATE.EDIT;
				return false;
			}
			this.BxCmd.Focus();
			return true;
		}

		private void initReelList(Designer Tab)
		{
			string[] array = X.makeToStringed<int>(X.makeCountUpArray(this.IMNG.getReelManager().getReelVector().Count, 0, 1));
			Tab.addRadioT<aBtnNel>(new DsnDataRadio
			{
				name = "list",
				keys = array,
				w = Tab.use_w - 24f,
				navi_loop = 2,
				h = 38f,
				fnHover = new FnBtnBindings(this.fineReelDetail),
				fnMakingAfter = new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnMakingReelDetail),
				click_snd = "tool_hand_init",
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnReelRowSelected),
				SCA = new ScrollAppend(239, Tab.use_w, Tab.use_h, 4f, 6f, 0)
			}.RowMode("reelinfo"));
		}

		private void prepareDescForReelList()
		{
			Designer designer = this.BxDesc.getTab("R_info");
			if (designer == null)
			{
				this.BxDesc.Clear();
				this.BxDesc.alignx = ALIGN.CENTER;
				this.BxDesc.init();
				this.BxDesc.addTab("R_info", this.BxDesc.use_w, this.BxDesc.use_h, this.BxDesc.use_w, this.BxDesc.use_h, false);
				this.BxDesc.endTab(true);
				designer = this.BxDesc.getTab("R_info");
				designer.Clear();
				designer.Smallest();
				designer.init();
				Designer designer2 = designer;
				DsnDataRadio dsnDataRadio = new DsnDataRadio();
				dsnDataRadio.name = "list";
				dsnDataRadio.keys = new string[0];
				dsnDataRadio.w = designer.use_w - 24f;
				dsnDataRadio.navi_loop = 2;
				dsnDataRadio.h = 32f;
				dsnDataRadio.unselectable = 2;
				dsnDataRadio.fnChanged = (BtnContainerRadio<aBtn> _BCon, int pre, int cur) => false;
				dsnDataRadio.SCA = new ScrollAppend(239, designer.use_w, designer.use_h, 4f, 6f, 0);
				designer2.addRadioT<aBtnNel>(dsnDataRadio.RowMode("reel_pict"));
				if (aBtn.PreSelected != null && aBtn.PreSelected.get_Skin() is ButtonSkinNelReelInfo)
				{
					this.fineReelDetail(aBtn.PreSelected);
				}
			}
		}

		private bool fnMakingReelDetail(BtnContainer<aBtn> _BCon, aBtn _B)
		{
			List<ReelExecuter> reelVector = this.IMNG.getReelManager().getReelVector();
			int num = X.NmI(_B.title, 0, false, false);
			(_B.get_Skin() as ButtonSkinNelReelInfo).initReel(reelVector[num]);
			return true;
		}

		private bool fineReelDetail(aBtn B)
		{
			Designer tab = this.BxDesc.getTab("R_info");
			if (tab == null)
			{
				return true;
			}
			List<ReelExecuter> reelVector = this.IMNG.getReelManager().getReelVector();
			int num = X.NmI(B.title, 0, false, false);
			ReelExecuter.ETYPE etype = reelVector[num].getEType();
			string[] aeffect = ReelManager.OAreel_content[(int)etype].Aeffect;
			(tab.Get("list", false) as BtnContainerRunner).BCon.RemakeT<aBtnNel>(aeffect, "reel_pict");
			return true;
		}

		private bool fnReelRowSelected(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (this.BtnSelReel != null || this.FnSpecialCommandPrepare != null)
			{
				return false;
			}
			if (this.GM.initBxCmd(UiGameMenu.STATE.ITEMMOVE, out this.BxCmd))
			{
				this.BxCmd.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
				this.BxCmd.margin_in_lr = 40f;
				this.BxCmd.margin_in_tb = 22f;
				this.BxCmd.WH(340f, 56f + this.BxCmd.margin_in_tb * 2f);
				this.BxCmd.item_margin_x_px = 20f;
				this.BxCmd.selectable_loop = 2;
				this.BxCmd.alignx = ALIGN.CENTER;
				this.BxCmd.init();
				this.BxCmd.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
				{
					titles = new string[] { "&&Submit_discard", "&&Cancel" },
					name = "reel_discard",
					skin = "row_center",
					clms = 1,
					w = this.BxCmd.use_w,
					h = 28f,
					margin_h = 0f,
					margin_w = 0f,
					fnClick = new FnBtnBindings(this.fnClickReelCmd),
					default_focus = 1
				});
			}
			else
			{
				this.BxCmd.activate();
				BtnContainerRunner btnContainerRunner = this.BxCmd.Get("reel_discard", false) as BtnContainerRunner;
				if (btnContainerRunner != null)
				{
					btnContainerRunner.setValue("0");
				}
			}
			this.BtnSelReel = _B.Get(cur_value);
			Vector3 position = this.BtnSelReel.transform.position;
			this.BxCmd.posSetDA(position.x * 64f + 350f, position.y * 64f, 0, 50f, true);
			BtnContainerRunner btnContainerRunner2 = this.BxCmd.Get("reel_discard", false) as BtnContainerRunner;
			btnContainerRunner2.setValue("-1");
			btnContainerRunner2.Get("&&Cancel").Select(true);
			this.BxR.hide();
			this.BxCmd.Focus();
			return true;
		}

		private bool runEditReelList()
		{
			if (IN.isCancel() || this.BxCategory.isFocused())
			{
				if (!(this.BtnSelReel != null))
				{
					return false;
				}
				this.cancelEditReelList(true);
			}
			this.BtnSelReel != null;
			return true;
		}

		private void cancelEditReelList(bool check_2 = false)
		{
			if (check_2)
			{
				(this.BxCmd.Get("reel_discard", false) as BtnContainerRunner).BCon.Get(1).SetChecked(true, true);
			}
			this.BxCmd.deactivate();
			BtnContainer<aBtn> bcon = (this.BxR.getTab("status_area").Get("list", false) as BtnContainerRunner).BCon;
			(bcon as BtnContainerRadio<aBtn>).setValue(-1, false);
			if (this.BtnSelReel != null)
			{
				int num = X.NmI(this.BtnSelReel.title, 0, false, false);
				this.BtnSelReel = null;
				if (bcon.Length > 0)
				{
					bcon.Get(X.MMX(0, num, bcon.Length - 1)).Select(true);
				}
			}
			this.BxR.bind();
			this.BxR.Focus();
			IN.clearSubmitPushDown(true);
			IN.clearCancelPushDown(true);
			SND.Ui.play("cancel", false);
		}

		private void selectReelList()
		{
			BtnContainer<aBtn> bcon = (this.BxR.getTab("status_area").Get("list", false) as BtnContainerRunner).BCon;
			if (bcon.Length > 0)
			{
				bcon.Get(0).Select(true);
			}
		}

		private bool fnClickReelCmd(aBtn B)
		{
			string title = B.title;
			if (title != null && title == "&&Submit_discard" && this.BtnSelReel != null)
			{
				List<ReelExecuter> reelVector = this.IMNG.getReelManager().getReelVector();
				int num = X.NmI(this.BtnSelReel.title, 0, false, false);
				if (X.BTW(0f, (float)num, (float)reelVector.Count))
				{
					reelVector.RemoveAt(num);
				}
				Designer tab = this.BxR.getTab("status_area");
				tab.Clear();
				this.initReelList(tab);
				SND.Ui.play("reset_var", false);
			}
			this.cancelEditReelList(false);
			return false;
		}

		internal void initItemMove()
		{
			SND.Ui.play("editor_open", false);
			this.quitWLinkAttachmentChoose();
			if (UiGMCItem.item_tab > UiGMCItem.ITEM_CTG.HOUSE)
			{
				this.RTabBar.setValue(0, true);
			}
			if (this.IMvCon == null)
			{
				this.IMvCon = new UiItemMove();
				this.IMvCon.fnUsingSliderDesc = new UiItemMove.FnUsingSliderDesc(this.fnItemMoveSliderDesc);
			}
			UiGameMenu.POSTYPE postype = ((UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.MAIN) ? UiGameMenu.POSTYPE.ITEMMOVE_L : UiGameMenu.POSTYPE.ITEMMOVE_R);
			this.BxCmd.deactivate();
			this.TopTab.deactivate(false);
			this.BtmTab.deactivate(false);
			this.GM.item_modified = true;
			this.GM.need_cmd_remake = true;
			this.GM.setPosType(postype);
			this.GM.right_last_top_row_height = (this.GM.right_last_btm_row_height = 0f);
			this.istate = UiGameMenu.STATE.ITEMMOVE;
			this.BxItmv.activate();
			this.IMvCon.stencil_ref = base.bxr_stencil_default;
			this.IMvCon.M2D = base.M2D;
			UiBoxDesigner uiBoxDesigner;
			UiBoxDesigner uiBoxDesigner2;
			if (this.GM.postype == UiGameMenu.POSTYPE.ITEMMOVE_L)
			{
				uiBoxDesigner = this.BxR;
				uiBoxDesigner2 = this.BxItmv;
			}
			else
			{
				uiBoxDesigner = this.BxItmv;
				uiBoxDesigner2 = this.BxR;
			}
			this.RTabBar = null;
			this.IMvCon.APoolEvacuated = this.APoolEvacuated;
			this.IMvCon.effect_confusion = base.effect_confusion;
			this.IMvCon.createItemMoveBox(0f, this.GM.BXR_Y_TRANSLATED, uiBoxDesigner, uiBoxDesigner2, base.M2D.IMNG.getInventory(), base.M2D.IMNG.getHouseInventory(), this.ItemMng, this.ItemMngMv, this.BxDesc, this.BxCmd);
			this.fineItemMoverBoxPosSlide();
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable == null)
			{
				return;
			}
			flgUiEffectDisable.Add("ITEMMOVE");
		}

		private void quitItemMove()
		{
			if (this.IMvCon == null)
			{
				return;
			}
			this.IMvCon.quitItemMove();
			if (this.IMvCon.cmd_recreated)
			{
				this.GM.need_cmd_remake = true;
			}
			UiGMCItem.item_tab = (this.IMvCon.current0 ? UiGMCItem.ITEM_CTG.MAIN : UiGMCItem.ITEM_CTG.HOUSE);
			this.releaseEvac();
			if (UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.HOUSE == (this.GM.postype == UiGameMenu.POSTYPE.ITEMMOVE_L))
			{
				this.BxR.getBox().tradeBoxPosition(this.BxItmv.getBox(), true);
				this.fineItemMoverBoxPosSlide();
			}
			this.BxItmv.deactivate();
			this.BxCmd.deactivate();
			this.istate = UiGameMenu.STATE.EDIT;
			this.GM.BxRRemake(true);
			this.GM.setPosType(UiGameMenu.POSTYPE.ITEM);
			this.initAppearWhole();
			this.initEdit();
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Rem("ITEMMOVE");
			}
			this.BxR.Focus();
		}

		public void fineItemMoverBoxPosSlide()
		{
			this.BxItmv.posSetA(this.BxItmv.getBox().get_deperture_x(), this.BxItmv.getBox().get_deperture_y() - IN.h - 120f, -1000f, -1000f, true);
		}

		private void fnItemMoveSliderDesc(ItemStorage Inv, STB Stb, NelItem Itm, int grade)
		{
			UiGMCItem.getInventoryCountString(Stb, Itm, grade, 1);
			Stb.Add(" ", "<img mesh=\"arrow_nel_5\" width=\"32\" height=\"18\" />", " ");
			UiGMCItem.getInventoryCountString(Stb, Itm, grade, 2);
		}

		public bool isEdittingReelList()
		{
			return this.GM.isEditState(this) && UiGMCItem.item_tab == UiGMCItem.ITEM_CTG.HOUSE && !base.M2D.canAccesableToHouseInventory();
		}

		public void reveal(NelItem Itm)
		{
			if (Itm.is_precious)
			{
				this.RTabBar.setValue(2, true);
			}
			using (BList<aBtnItemRow> blist = this.ItemMng.Inventory.PopGetItemRowBtnsFor(Itm))
			{
				if (blist.Count > 0)
				{
					blist[0].Select(true);
				}
			}
		}

		public static float desc_item_w
		{
			get
			{
				return 360f;
			}
		}

		public static float desc_item_w_mv
		{
			get
			{
				return 300f;
			}
		}

		private NelItemManager IMNG;

		private UiItemManageBoxSlider ItemMng;

		private UiItemManageBoxSlider ItemMngMv;

		private ColumnRowNel RTabBar;

		private FillBlock FbKD;

		private UiItemManageBox.FnCommandPrepare FnSpecialCommandPrepare;

		private UiItemManageBox.FnItemRowInitAfter FnSpecialItemRowInitAfter;

		private NelItem AttachSrc;

		private ItemStorage StAttachSrc;

		private int attach_src_grade;

		private UiLunchTime LunchTime;

		private UiReelManager UiReel;

		private ItemStorage StReelSpliced;

		private Designer TabR;

		private UiBoxDesigner BxCmd;

		private UiGameMenu.STATE istate;

		internal static UiGMCItem.ITEM_CTG item_tab;

		private List<aBtn> APoolEvacuated;

		private UiBoxDesigner BxItmv;

		private const float wlink_cmd_x = 660f;

		private const float wlink_cmd_y = 30f;

		private aBtn BtnSelReel;

		private UiItemMove IMvCon;

		internal enum ITEM_CTG
		{
			MAIN,
			HOUSE,
			PRECIOUS,
			_MAX
		}
	}
}
