using System;
using System.Collections.Generic;
using m2d;
using nel.gm;
using UnityEngine;
using XX;

namespace nel
{
	public class UiItemMove
	{
		public float bounds_w
		{
			get
			{
				return UiGMC.bounds_w;
			}
		}

		public float bounds_wh
		{
			get
			{
				return UiGMC.bounds_wh;
			}
		}

		public float desc_item_w_mv
		{
			get
			{
				return UiGMCItem.desc_item_w_mv;
			}
		}

		public void createItemMoveBox(float cx, float cy, UiBoxDesigner Bx0, UiBoxDesigner Bx1, ItemStorage Inv0, ItemStorage Inv1, UiItemManageBoxSlider _Mng0, UiItemManageBoxSlider _Mng1, UiBoxDesigner _BxDesc, UiBoxDesigner _BxCmd)
		{
			if (this.M2D == null)
			{
				this.M2D = M2DBase.Instance as NelM2DBase;
			}
			float num = cx - this.bounds_wh + this.desc_item_w_mv / 2f - 20f;
			float num2 = (this.bounds_w - this.desc_item_w_mv + 20f + 290f) * 0.5f;
			this.cmd_recreated = false;
			this.Itm_lock_move_to_abs_near_by_switchng = null;
			this.t_itm_lock_abs = -1f;
			this.current_sliding_item = false;
			if (this.force_clear_designer || this.Mng0 != _Mng0 || this.Mng1 != _Mng1 || this.Inventory0 != Inv0 || this.Inventory1 != Inv1 || this.DsR0 != Bx0 || this.DsR1 != Bx1 || this.BxDesc != _BxDesc || this.BxCmd != _BxCmd)
			{
				this.Mng0 = _Mng0;
				this.Mng1 = _Mng1;
				this.DsR0 = Bx0;
				this.DsR1 = Bx1;
				this.Inventory0 = Inv0;
				this.Inventory1 = Inv1;
				this.BxDesc = _BxDesc;
				this.BxCmd = _BxCmd;
				this.Mng0.quitDesigner(false, true);
				this.Mng1.quitDesigner(false, true);
				this.BxDesc.Clear();
				this.BxDesc.activate();
				this.BxDesc.WH(this.desc_item_w_mv, this.bounds_h);
				UiItemManageBoxSlider.FnSlidedMeter <>9__0;
				UiItemManageBoxSlider.FnSlidedMeter <>9__1;
				for (int i = 0; i < 2; i++)
				{
					UiBoxDesigner uiBoxDesigner = ((i == 0) ? Bx0 : Bx1);
					ItemStorage itemStorage = ((i == 0) ? Inv0 : Inv1);
					uiBoxDesigner.position(num + this.desc_item_w_mv * 0.5f + (float)(10 * (i + 1)) + num2 * (0.5f + (float)i), cy, -1000f, -1000f, false);
					uiBoxDesigner.WH(num2, this.bounds_h);
					uiBoxDesigner.Clear();
					uiBoxDesigner.margin_in_lr = 28f;
					uiBoxDesigner.box_stencil_ref_mask = -1;
					uiBoxDesigner.item_margin_x_px = 14f;
					uiBoxDesigner.item_margin_y_px = 18f;
					uiBoxDesigner.alignx = ALIGN.LEFT;
					uiBoxDesigner.use_scroll = false;
					uiBoxDesigner.item_margin_x_px = 0f;
					uiBoxDesigner.item_margin_y_px = 0f;
					uiBoxDesigner.init();
					DsnDataP dsnDataP = NelDsn.PT(16, true);
					dsnDataP.name = "KD";
					dsnDataP.swidth = uiBoxDesigner.use_w;
					uiBoxDesigner.addP(dsnDataP, false);
					uiBoxDesigner.Br();
					Designer designer = uiBoxDesigner.addTab("status_area", uiBoxDesigner.use_w, uiBoxDesigner.use_h - 2f, uiBoxDesigner.use_w, uiBoxDesigner.use_h - 2f, false);
					designer.Smallest();
					designer.margin_in_tb = 6f;
					uiBoxDesigner.endTab(false);
					UiItemManageBoxSlider uiItemManageBoxSlider = ((i == 0) ? this.Mng0 : this.Mng1);
					uiItemManageBoxSlider.Pr = null;
					uiItemManageBoxSlider.fnRunUsePost = null;
					uiItemManageBoxSlider.fnCommandKeysPrepare = null;
					uiItemManageBoxSlider.fnCommandBtnExecuted = null;
					uiItemManageBoxSlider.fnItemRowRemakedAfter = this.fnItemRowRemakedAfter;
					uiItemManageBoxSlider.fnDescAddition = new UiItemManageBox.FnDescAddition(UiGMCItem.fnDescAddition);
					uiItemManageBoxSlider.fnCommandPrepare = new UiItemManageBox.FnCommandPrepare(this.fnItemMoveCommandPrepare);
					uiItemManageBoxSlider.fnDetailPrepare = null;
					UiItemManageBoxSlider.FnSlidedMeter fnSlidedMeter = null;
					if (this.fnSlidedMeterAfter != null)
					{
						if (i == 0)
						{
							UiItemManageBoxSlider.FnSlidedMeter fnSlidedMeter2;
							if ((fnSlidedMeter2 = <>9__0) == null)
							{
								fnSlidedMeter2 = (<>9__0 = (aBtnMeterNel B, int increase, int pre, int cur) => this.fnSlidedMeterAfter(Inv0, increase, pre, cur));
							}
							fnSlidedMeter = fnSlidedMeter2;
						}
						else
						{
							UiItemManageBoxSlider.FnSlidedMeter fnSlidedMeter3;
							if ((fnSlidedMeter3 = <>9__1) == null)
							{
								fnSlidedMeter3 = (<>9__1 = (aBtnMeterNel B, int increase, int pre, int cur) => this.fnSlidedMeterAfter(Inv1, increase, pre, cur));
							}
							fnSlidedMeter = fnSlidedMeter3;
						}
					}
					uiItemManageBoxSlider.fnSlidedMeterAfter = fnSlidedMeter;
					uiItemManageBoxSlider.fnGradeFocusChange = new UiItemManageBox.FnGradeFocusChange(this.fnItemMoveGradeFocusChange);
					uiItemManageBoxSlider.manager_auto_run_on_select_row = false;
					uiItemManageBoxSlider.item_row_skin = "normal";
					uiItemManageBoxSlider.cmd_w = 390f;
					uiItemManageBoxSlider.slice_height = 20f;
					uiItemManageBoxSlider.effect_confusion = this.effect_confusion;
					uiItemManageBoxSlider.title_text_content = "";
					uiItemManageBoxSlider.stencil_ref = this.stencil_ref;
					uiItemManageBoxSlider.ParentBoxDesigner = uiBoxDesigner;
					uiItemManageBoxSlider.do_not_remake_desc_box = true;
					uiItemManageBoxSlider.APoolEvacuated = this.APoolEvacuated;
					uiItemManageBoxSlider.initDesigner(itemStorage, uiBoxDesigner.getTab("status_area"), this.BxDesc, this.BxCmd, false, true, false);
					if (this.APoolEvacuated == null)
					{
						this.APoolEvacuated = uiItemManageBoxSlider.APoolEvacuated;
					}
					uiItemManageBoxSlider.DefaultTargetInventory = ((i == 0) ? Inv1 : Inv0);
				}
				this.Mng0.fnDetailPrepare = new UiItemManageBox.FnDetailPrepare(this.fnItemMoveDetailPrepareL);
				this.Mng1.fnDetailPrepare = new UiItemManageBox.FnDetailPrepare(this.fnItemMoveDetailPrepareR);
			}
			else
			{
				this.Inventory0.fineRows(false);
				this.Inventory1.fineRows(false);
				this.BxDesc.activate();
			}
			this.BxDesc.position(num, cy, -1000f, -1000f, false);
			this.AnotherRow = null;
			this.t_itm_lock_abs = 0f;
			this.fineItemMoveFocusTab(true);
		}

		public bool both_empty_rows
		{
			get
			{
				return this.Mng0 != null && this.Mng0.Inventory.getVisibleRowCount() == 0 && this.Mng1 != null && this.Mng1.Inventory.getVisibleRowCount() == 0;
			}
		}

		public void quitDesigner()
		{
			this.Mng0.quitDesigner(false, true);
			this.Mng1.quitDesigner(false, true);
		}

		public void quitItemMove()
		{
			this.Inventory0.fineRows(false);
			this.Inventory1.fineRows(false);
		}

		public void destruct()
		{
			this.releaseEvac(ref this.EvcItemMoveCmd);
		}

		private bool fnItemMoveCommandPrepare(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow BRow)
		{
			if (this.current1 == (IMng.Inventory == this.Inventory0))
			{
				return false;
			}
			this.Mng0.manager_auto_run_on_select_row = (this.Mng1.manager_auto_run_on_select_row = false);
			this.DsR1.Get("KD", false).setValue(" ");
			this.DsR0.Get("KD", false).setValue(" ");
			this.Mng0.can_handle = false;
			this.Mng1.can_handle = false;
			aBtnMeterNel aBtnMeterNel = null;
			this.cmd_recreated = true;
			UiItemManageBoxSlider uiItemManageBoxSlider;
			if (this.current0)
			{
				uiItemManageBoxSlider = this.Mng0;
				this.DsR1.hide();
			}
			else
			{
				uiItemManageBoxSlider = this.Mng1;
				this.DsR0.hide();
			}
			bool flag = this.current1;
			BxCmd.WHanim(390f, 150f, true, true);
			BxCmd.margin_in_lr = 26f;
			BxCmd.margin_in_tb = 38f;
			BxCmd.item_margin_x_px = 0f;
			BxCmd.item_margin_y_px = 12f;
			BxCmd.alignx = ALIGN.CENTER;
			BxCmd.init();
			if (this.SliderMde == null)
			{
				this.SliderMde = new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
				{
					name = "move_count",
					fnDescConvert = new FnDescConvert(this.fnUsingSliderDescInner),
					lr_reverse = flag,
					fnChanged = delegate(aBtnMeter _B, float pre_value, float cur_value)
					{
						UiItemManageBoxSlider uiItemManageBoxSlider2 = (this.current0 ? this.Mng0 : this.Mng1);
						if (uiItemManageBoxSlider2.fnChangeSlider(_B as aBtnMeterNel, pre_value, cur_value, this.SliderMde))
						{
							this.Itm_lock_move_to_abs_near_by_switchng = uiItemManageBoxSlider2.UsingTarget ?? uiItemManageBoxSlider2.SelectingTarget;
							this.t_itm_lock_abs = this.MAXT_ITM_LOCK_ABS;
							return true;
						}
						return false;
					},
					use_wheel = true
				};
				if (this.fnSlidedMeter != null)
				{
					this.SliderMde.fnSlidedMeter = delegate(aBtnMeterNel B, int increase, int pre, int cur)
					{
						UiItemManageBoxSlider uiItemManageBoxSlider3 = (this.current0 ? this.Mng0 : this.Mng1);
						return this.fnSlidedMeter(uiItemManageBoxSlider3.Inventory, increase, pre, cur);
					};
				}
			}
			bool flag2 = this.reassignEvacuated(ref this.EvcItemMoveCmd, BxCmd);
			if (flag2)
			{
				BxCmd.activate();
				aBtnMeterNel = BxCmd.Get("move_count", false) as aBtnMeterNel;
				if (aBtnMeterNel != null)
				{
					aBtnMeterNel.getCtSetter().lr_reverse = flag;
					uiItemManageBoxSlider.fineMeter(aBtnMeterNel, this.SliderMde);
					uiItemManageBoxSlider.assignSliderDsn(aBtnMeterNel, this.SliderMde);
				}
				else
				{
					flag2 = false;
				}
			}
			if (!flag2)
			{
				BxCmd.Clear();
				BxCmd.alignx = ALIGN.CENTER;
				float use_w = BxCmd.use_w;
				aBtnMeterNel = uiItemManageBoxSlider.createSlider(this.SliderMde, 1f);
				BxCmd.Br();
				DsnDataP dsnDataP = NelDsn.PT(18, true);
				dsnDataP.name = "error_fb";
				BxCmd.addP(dsnDataP, false);
				BxCmd.Br();
				aBtn aBtn = BxCmd.addButton(new DsnDataButton
				{
					name = "Cancel",
					title = "Cancel",
					skin_title = "&&Submit",
					h = 24f,
					w = use_w - 30f,
					fnClick = (aBtn B) => (this.current0 ? this.Mng0 : this.Mng1).fnClickItemCmd(B),
					skin = "row_center"
				});
				aBtn.setNaviT(aBtnMeterNel, true, true);
				aBtn.setNaviB(aBtnMeterNel, true, true);
			}
			ItemStorage defaultTargetInventory = uiItemManageBoxSlider.DefaultTargetInventory;
			string text = "";
			FillBlock fillBlock = BxCmd.Get("error_fb", false) as FillBlock;
			if (fillBlock != null)
			{
				NelItem itemData = BRow.getItemData();
				if (defaultTargetInventory.getItemCapacity(itemData, false, false) == 0)
				{
					if (!defaultTargetInventory.infinit_stockable && defaultTargetInventory.row_max <= defaultTargetInventory.getVisibleRowCount())
					{
						text = TX.Get("cannot_take_need_enough_room", "");
					}
					else if (!defaultTargetInventory.water_stockable && itemData.is_water)
					{
						text = TX.GetA("cannot_take_need_container_item", NelItem.Bottle.getLocalizedName(0));
					}
					else
					{
						text = TX.Get("cannot_take_other_reason", "");
					}
				}
				if (text == "")
				{
					fillBlock.text_content = "";
				}
				else
				{
					using (STB stb = TX.PopBld(null, 0))
					{
						fillBlock.Txt(stb.Add(NEL.error_tag, text, NEL.error_tag_close));
					}
				}
			}
			aBtnMeterNel.Select(true);
			return true;
		}

		private void fnItemMoveGradeFocusChange(NelItem Itm, ItemStorage.ObtainInfo Obt, int grade)
		{
			if (grade < 0)
			{
				if (!this.EvcItemMoveCmd.valid)
				{
					this.EvcItemMoveCmd = new Designer.EvacuateContainer(this.BxCmd, false);
				}
				UiBoxDesigner dsR = this.DsR0;
				UiBoxDesigner dsR2 = this.DsR1;
				UiBoxDesigner uiBoxDesigner = dsR;
				UiBoxDesigner uiBoxDesigner2 = dsR2;
				this.Mng0.can_handle = true;
				this.Mng1.can_handle = true;
				if (this.current0)
				{
					UiItemManageBoxSlider mng = this.Mng0;
					uiBoxDesigner2.bind();
				}
				else
				{
					UiItemManageBoxSlider mng2 = this.Mng1;
					uiBoxDesigner.bind();
				}
				this.fineItemMoveFocusTab(false);
				if (Itm.isEmptyLunchBox())
				{
					this.Mng0.Inventory.fineRows(false);
				}
			}
		}

		private void fineItemMoveFocusTab(bool fine_desc = false)
		{
			UiBoxDesigner dsR = this.DsR0;
			UiBoxDesigner dsR2 = this.DsR1;
			UiBoxDesigner uiBoxDesigner = dsR;
			UiBoxDesigner uiBoxDesigner2 = dsR2;
			if (this.current0 && this.Mng0.Inventory.getVisibleRowCount() == 0)
			{
				this.current1 = true;
			}
			if (this.current1 && this.Mng1.Inventory.getVisibleRowCount() == 0)
			{
				this.current1 = false;
			}
			aBtnItemRow aBtnItemRow;
			if (this.current0)
			{
				uiBoxDesigner.Focus();
				uiBoxDesigner2.setValueTo("KD", "<key rtab/>" + TX.Get(this.inventory1_tx_key, ""));
				uiBoxDesigner.setValueTo("KD", TX.GetA("KD_itemmove_whole", TX.Get(this.inventory1_tx_key, "")));
				aBtnItemRow = this.Mng1.Inventory.SelectedRow;
				this.Mng1.blurDescTarget(false);
				this.Mng0.manager_auto_run_on_select_row = false;
				aBtnItemRow aBtnItemRow2 = this.Mng0.Inventory.SelectedRow;
				if (aBtnItemRow2 != null)
				{
					aBtnItemRow2.Select(true);
				}
				this.Mng1.manager_auto_run_on_select_row = true;
				if (aBtnItemRow2 != null && fine_desc)
				{
					this.Mng0.blurDescTarget(true);
				}
			}
			else
			{
				uiBoxDesigner2.Focus();
				uiBoxDesigner.setValueTo("KD", "<key ltab/>" + TX.Get(this.inventory0_tx_key, ""));
				uiBoxDesigner2.setValueTo("KD", TX.GetA("KD_itemmove_whole", TX.Get(this.inventory0_tx_key, "")));
				aBtnItemRow = this.Mng0.Inventory.SelectedRow;
				this.Mng0.blurDescTarget(false);
				this.Mng1.manager_auto_run_on_select_row = false;
				aBtnItemRow aBtnItemRow2 = this.Mng1.Inventory.SelectedRow;
				if (aBtnItemRow2 != null)
				{
					aBtnItemRow2.Select(true);
				}
				this.Mng0.manager_auto_run_on_select_row = true;
				if (aBtnItemRow2 != null && fine_desc)
				{
					this.Mng1.blurDescTarget(true);
				}
			}
			this.AnotherRow = aBtnItemRow;
		}

		private void fnItemMoveHoverRow(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
		}

		private void fnUsingSliderDescInner(STB Stb)
		{
			if (this.fnUsingSliderDesc == null)
			{
				return;
			}
			UiItemManageBoxSlider uiItemManageBoxSlider = (this.current1 ? this.Mng1 : this.Mng0);
			NelItem usingTarget = uiItemManageBoxSlider.UsingTarget;
			if (usingTarget == null)
			{
				return;
			}
			int grade_cursor = uiItemManageBoxSlider.get_grade_cursor();
			Stb.Clear();
			this.fnUsingSliderDesc(uiItemManageBoxSlider.Inventory, Stb, usingTarget, grade_cursor);
		}

		public bool runItemMove(float fcnt)
		{
			UiItemManageBoxSlider uiItemManageBoxSlider = (this.current1 ? this.Mng1 : this.Mng0);
			uiItemManageBoxSlider.runEditItem();
			if (!uiItemManageBoxSlider.isUsingState())
			{
				if (this.t_itm_lock_abs > 0f)
				{
					this.t_itm_lock_abs = X.Mx(0f, this.t_itm_lock_abs - fcnt);
				}
				if (IN.isCancel())
				{
					return false;
				}
				if (IN.isUiAddPD())
				{
					aBtnItemRow selectedRow = uiItemManageBoxSlider.Inventory.SelectedRow;
					if (selectedRow != null && !selectedRow.is_fake_row)
					{
						ItemStorage inventory = (this.current0 ? this.Mng1 : this.Mng0).Inventory;
						ItemStorage inventory2 = uiItemManageBoxSlider.Inventory;
						int visibleRowCount = inventory.getVisibleRowCount();
						NelItem itemData = selectedRow.getItemData();
						int num = -1;
						if (inventory2.grade_split)
						{
							num = (int)selectedRow.getItemRow().splitted_grade;
						}
						int num2 = X.Mn(inventory2.getReduceable(itemData, num), X.Mn(selectedRow.getItemRow().total, inventory.getItemStockable(itemData)));
						ItemStorage.ObtainInfo itemInfo = selectedRow.getItemInfo();
						uiItemManageBoxSlider.animation_immediate_flag = true;
						int num3 = 0;
						this.current_sliding_item = true;
						for (int i = 0; i < num2; i++)
						{
							int num4 = (inventory2.grade_split ? num : (this.current0 ? itemInfo.min_grade : itemInfo.enough_grade));
							if (!this.IMNG.isHoldingItemByPR(itemData, num4))
							{
								if (this.fnSlidedMeter != null)
								{
									int count = inventory2.getCount(itemData, num4);
									if (this.fnSlidedMeter(inventory2, 1, count, count - 1) == count)
									{
										goto IL_01B2;
									}
								}
								if (inventory.Add(itemData, 1, num4, true, true) > 0)
								{
									inventory2.Reduce(itemData, 1, num4, true);
									num3++;
									this.Itm_lock_move_to_abs_near_by_switchng = itemData;
									this.t_itm_lock_abs = this.MAXT_ITM_LOCK_ABS;
								}
							}
							IL_01B2:;
						}
						this.current_sliding_item = false;
						if (num3 > 0)
						{
							inventory.fineSpecificRow(itemData);
							SND.Ui.play(this.current0 ? "tool_selrect" : "tool_sellasso", false);
							if (this.fnSlidedMeterAfter != null)
							{
								this.fnSlidedMeterAfter(inventory2, num3, 0, 0);
							}
						}
						else
						{
							CURS.limitVib(this.current0 ? AIM.R : AIM.L);
						}
						bool flag;
						if (itemData.isWLinkUser(out flag) && inventory == this.IMNG.getInventory() && visibleRowCount == inventory.getVisibleRowCount())
						{
							inventory.fineRows(false);
						}
					}
				}
				if (!IN.isUiSortPD())
				{
					if (IN.isLTabPD() || IN.isRTabPD())
					{
						bool flag2 = false;
						if (this.AnotherRow != null && !this.AnotherRow.destructed)
						{
							bool flag3 = this.current1;
							this.AnotherRow.Select(true);
							if (flag3 != this.current1)
							{
								flag2 = true;
							}
						}
						if (!flag2)
						{
							UiItemManageBoxSlider uiItemManageBoxSlider2 = (this.current0 ? this.Mng1 : this.Mng0);
							UiBoxDesigner dsR = this.DsR0;
							UiBoxDesigner dsR2 = this.DsR1;
							UiBoxDesigner uiBoxDesigner = dsR;
							UiBoxDesigner uiBoxDesigner2 = dsR2;
							Vector2 vector = ((uiItemManageBoxSlider.Inventory.SelectedRow == null) ? (this.current0 ? uiBoxDesigner : uiBoxDesigner2).transform.position : uiItemManageBoxSlider.Inventory.SelectedRow.transform.position);
							vector.x = (this.current0 ? uiBoxDesigner2 : uiBoxDesigner).transform.position.x;
							if (this.Itm_lock_move_to_abs_near_by_switchng != null)
							{
								if (this.t_itm_lock_abs > 0f && uiItemManageBoxSlider2.Inventory.SelectedRow != null)
								{
									uiItemManageBoxSlider2.Inventory.SelectedRow.Select(true);
									this.t_itm_lock_abs = this.MAXT_ITM_LOCK_ABS;
								}
								else
								{
									this.Itm_lock_move_to_abs_near_by_switchng = null;
								}
							}
							if (this.Itm_lock_move_to_abs_near_by_switchng == null)
							{
								this.t_itm_lock_abs = 0f;
								aBtnItemRow absNearBtn = uiItemManageBoxSlider2.Inventory.getAbsNearBtn(vector);
								if (absNearBtn != null)
								{
									absNearBtn.Select(true);
									flag2 = true;
								}
							}
						}
						if (!flag2)
						{
							SND.Ui.play("toggle_button_limit", false);
							CURS.limitVib(AIM.R);
						}
					}
					else if (IN.isUiShiftO())
					{
						UiItemManageBoxSlider uiItemManageBoxSlider3 = (this.current0 ? this.Mng1 : this.Mng0);
						NelItem nelItem = null;
						if (uiItemManageBoxSlider.getSelectingRowBtn() != null && !uiItemManageBoxSlider.isUsingState())
						{
							nelItem = uiItemManageBoxSlider.getSelectingRowBtn().getItemData();
						}
						using (BList<aBtnItemRow> blist = uiItemManageBoxSlider3.Inventory.PopGetItemRowBtnsFor(nelItem))
						{
							if (this.AnotherRow == null || !this.AnotherRow.isChecked())
							{
								if (blist == null || blist.Count == 0)
								{
									this.AnotherRow = null;
									if (IN.isUiShiftPD())
									{
										SND.Ui.play("toggle_button_limit", false);
										CURS.limitVib(AIM.R);
									}
								}
								else
								{
									this.AnotherRow = blist[0];
									uiItemManageBoxSlider3.Inventory.select_row_key = this.AnotherRow.getItemData().key;
									uiItemManageBoxSlider3.Inventory.SelectedRow = this.AnotherRow;
									if (IN.isUiShiftPD())
									{
										SND.Ui.play("toggle_button_open", false);
									}
									uiItemManageBoxSlider3.Inventory.RowReveal(blist[0].transform, false);
									blist[0].pushedAnimSimulate();
									if (blist.Count > 1)
									{
										uiItemManageBoxSlider3.Inventory.RowReveal(blist[blist.Count - 1].transform, false);
									}
								}
							}
						}
					}
				}
			}
			return true;
		}

		private void fnItemMoveDetailPrepareL(NelItem Itm, ItemStorage.ObtainInfo Obt, ItemStorage.IRow IR)
		{
			if (this.Itm_lock_move_to_abs_near_by_switchng != Itm && this.MAXT_ITM_LOCK_ABS - this.t_itm_lock_abs > 4f)
			{
				this.Itm_lock_move_to_abs_near_by_switchng = null;
			}
			if (this.t_itm_lock_abs >= 0f)
			{
				this.fnItemMoveDetailPrepare(false);
			}
		}

		private void fnItemMoveDetailPrepareR(NelItem Itm, ItemStorage.ObtainInfo Obt, ItemStorage.IRow IR)
		{
			if (this.Itm_lock_move_to_abs_near_by_switchng != Itm && this.MAXT_ITM_LOCK_ABS - this.t_itm_lock_abs > 4f)
			{
				this.Itm_lock_move_to_abs_near_by_switchng = null;
			}
			if (this.t_itm_lock_abs >= 0f)
			{
				this.fnItemMoveDetailPrepare(true);
			}
		}

		private void fnItemMoveDetailPrepare(bool to_1)
		{
			this.AnotherRow = null;
			if (this.current1 != to_1)
			{
				this.current1 = to_1;
				SND.Ui.play("tool_hand_init", false);
				this.fineItemMoveFocusTab(false);
			}
		}

		public bool isFocusingRight()
		{
			return this.current1;
		}

		public bool current0
		{
			get
			{
				return !this.current1;
			}
		}

		protected bool reassignEvacuated(ref Designer.EvacuateContainer EvCon, Designer Target = null)
		{
			if (!EvCon.valid)
			{
				return false;
			}
			EvCon.reassign(Target);
			return true;
		}

		protected void releaseEvac(ref Designer.EvacuateContainer EvCon)
		{
			if (EvCon.valid)
			{
				EvCon.release(null);
			}
		}

		public bool isUsingState()
		{
			return (this.current1 ? this.Mng1 : this.Mng0).isUsingState();
		}

		public NelItemManager IMNG
		{
			get
			{
				return this.M2D.IMNG;
			}
		}

		public override string ToString()
		{
			return "UiItemMove";
		}

		public string inventory0_tx_key = "Item_Tab_main";

		public string inventory1_tx_key = "Item_Tab_house";

		public bool effect_confusion;

		public int stencil_ref = 40;

		public List<aBtn> APoolEvacuated;

		public NelM2DBase M2D;

		public float bounds_h = UiGMC.bounds_h;

		public UiItemManageBox.FnStorageRun fnItemRowRemakedAfter;

		public UiItemMove.FnSlidedMeterIM fnSlidedMeter;

		public UiItemMove.FnSlidedMeterIM fnSlidedMeterAfter;

		public UiItemMove.FnUsingSliderDesc fnUsingSliderDesc;

		public bool force_clear_designer = true;

		private aBtnItemRow AnotherRow;

		private NelItem Itm_lock_move_to_abs_near_by_switchng;

		private float t_itm_lock_abs;

		private float MAXT_ITM_LOCK_ABS = 120f;

		private Designer.EvacuateContainer EvcItemMoveCmd;

		private bool current1;

		private UiItemManageBoxSlider Mng0;

		private UiItemManageBoxSlider Mng1;

		private UiBoxDesigner DsR0;

		private UiBoxDesigner DsR1;

		private UiBoxDesigner BxCmd;

		private UiBoxDesigner BxDesc;

		private ItemStorage Inventory0;

		private ItemStorage Inventory1;

		public bool cmd_recreated;

		public bool current_sliding_item;

		public const float DEFAULT_CX = -155f;

		private UiItemManageBoxSlider.DsnDataSliderIMB SliderMde;

		public delegate int FnSlidedMeterIM(ItemStorage Inv, int inclease, int pre_val, int cur_val);

		public delegate void FnUsingSliderDesc(ItemStorage Inv, STB Stb, NelItem Itm, int grade);
	}
}
