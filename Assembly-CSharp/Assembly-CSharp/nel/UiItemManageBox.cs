using System;
using System.Collections.Generic;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class UiItemManageBox
	{
		public UiItemManageBox clearFn()
		{
			this.fnDetailPrepare = null;
			this.fnDescAddition = null;
			this.fnGradeFocusChange = null;
			this.fnItemRowInitAfter = null;
			this.fnWholeRowsPrepare = null;
			this.fnSortInjectMng = null;
			this.fnCommandPrepare = null;
			this.fnCommandKeysPrepare = null;
			this.fnCommandBtnExecuted = null;
			return this;
		}

		public UiItemManageBox(NelItemManager _IMNG, Transform _TrsEvacuateTo = null)
		{
			this.IMNG = _IMNG;
			this.TrsEvacuateTo = _TrsEvacuateTo;
		}

		public void initDesigner(ItemStorage _Inventory, Designer BxMain = null, UiBoxDesigner _BxDesc = null, UiBoxDesigner _BxCmd = null, bool fine_r = false, bool activate_state = true, bool select_button_on_state_change = true)
		{
			if (this.BxR != BxMain)
			{
				this.BxR = BxMain;
				if (this.BxR is UiBoxDesigner)
				{
					this.ParentBoxDesigner = this.BxR as UiBoxDesigner;
				}
				fine_r = true;
			}
			this.can_handle = true;
			this.quit_whole_ui = false;
			if (_Inventory != null && _Inventory != this.Inventory)
			{
				if (this.Inventory != null)
				{
					this.Inventory.releaseDesigner();
				}
				this.Inventory = _Inventory;
				if (!this.Inventory.auto_splice_zero_row)
				{
					this.OTouchGrade = new BDic<NelItem, uint>();
					if (this.Inventory.grade_split)
					{
						int wholeRowCount = this.Inventory.getWholeRowCount();
						for (int i = 0; i < wholeRowCount; i++)
						{
							ItemStorage.IRow rowByIndex = this.Inventory.getRowByIndex(i);
							uint num;
							if (this.OTouchGrade.TryGetValue(rowByIndex.Data, out num))
							{
								this.OTouchGrade[rowByIndex.Data] = num | (1U << (int)rowByIndex.splitted_grade);
							}
							else
							{
								this.OTouchGrade[rowByIndex.Data] = 1U << (int)rowByIndex.splitted_grade;
							}
						}
					}
				}
				else
				{
					this.OTouchGrade = null;
				}
				this.ASupportStorage = null;
				fine_r = true;
			}
			this.animation_immediate_flag = false;
			this.grade_cursor_memory = -1;
			if (fine_r && this.Inventory != null && this.BxR != null)
			{
				this.initDesignerContentMain();
			}
			if (this.BxDesc != _BxDesc)
			{
				this.BxDesc = _BxDesc;
				this.initEditItem();
			}
			this.BxCmd = _BxCmd;
			if (this.BxR != null && activate_state)
			{
				this.changeState(UiItemManageBox.STATE.SELECT, select_button_on_state_change);
			}
		}

		private void changeState(UiItemManageBox.STATE _st, bool select_button_on_state_change = true)
		{
			if (this.Inventory == null)
			{
				return;
			}
			UiItemManageBox.STATE state = this.state;
			if (state == UiItemManageBox.STATE.USING)
			{
				if (this.UsingTarget != null && this.OTouchGrade != null && this.ASupportStorage_ != null)
				{
					int num = this.ASupportStorage_.Length;
					uint num2 = X.Get<NelItem, uint>(this.OTouchGrade, this.UsingTarget, 0U);
					for (int i = 0; i < num; i++)
					{
						ItemStorage.ObtainInfo info = this.ASupportStorage_[i].getInfo(this.UsingTarget);
						if (info != null)
						{
							num2 |= info.getGradeUsingBit();
						}
					}
					this.OTouchGrade[this.UsingTarget] = num2;
				}
				if (this.ItemUsingRow != null)
				{
					this.Inventory.BlurCheckedRow();
					this.ItemSelectRow = this.ItemUsingRow;
					this.ItemSelectRow.Select(false);
				}
				this.closeUsingState();
				this.ItemUsingRow = null;
				this.UsingTarget = null;
				IN.clearPushDown(false);
			}
			this.state = _st;
			this.animation_immediate_flag = false;
			UiItemManageBox.STATE state2 = this.state;
			if (state2 != UiItemManageBox.STATE.SELECT)
			{
				if (state2 == UiItemManageBox.STATE.USING)
				{
					if (this.ItemUsingRow != null)
					{
						this.UsingTarget = this.ItemUsingRow.getItemData();
						this.ItemUsingRow.SetChecked(false, true).Select(true);
						this.ItemSelectRow = null;
						if (this.OTouchGrade != null && !this.OTouchGrade.ContainsKey(this.UsingTarget))
						{
							uint num3 = this.ItemUsingRow.getItemInfo().getGradeUsingBit();
							if (this.ASupportStorage_ != null)
							{
								int num4 = this.ASupportStorage_.Length;
								for (int j = 0; j < num4; j++)
								{
									ItemStorage.ObtainInfo info2 = this.ASupportStorage_[j].getInfo(this.UsingTarget);
									if (info2 != null)
									{
										num3 |= info2.getGradeUsingBit();
									}
								}
							}
							this.OTouchGrade[this.UsingTarget] = num3;
						}
					}
					if (this.BxCmd != null)
					{
						this.BxCmd.Focusable(true, true, null).Focus();
					}
					this.runEditItem(true, true, select_button_on_state_change);
					this.need_blur_checked_row = false;
					IN.clearPushDown(false);
					if (this.hide_list_buttons_when_using)
					{
						this.Inventory.rowcontainer_alloc_wheel = false;
						return;
					}
				}
			}
			else
			{
				if (this.BxCmd != null)
				{
					this.BxCmd.deactivate();
				}
				if (state == UiItemManageBox.STATE.USING && this.hide_list_buttons_when_using)
				{
					this.Inventory.bindBCon();
				}
				this.runEditItem(false, true, select_button_on_state_change);
				if (this.ParentBoxDesigner != null)
				{
					this.ParentBoxDesigner.Focus();
				}
				this.need_blur_checked_row = true;
				if (this.hide_list_buttons_when_using)
				{
					this.Inventory.rowcontainer_alloc_wheel = true;
				}
			}
		}

		public void blurDesc()
		{
			if (this.state == UiItemManageBox.STATE.USING)
			{
				this.changeState(UiItemManageBox.STATE.SELECT, true);
			}
			if (this.BxDesc != null)
			{
				this.BxDesc.effect_confusion = false;
			}
			this.changeState(UiItemManageBox.STATE.OFFLINE, true);
			this.BxDesc = null;
			this.BxCmd = null;
			this.BConItemStars = null;
			this.FbDesc = null;
		}

		public void unlinkCmdWindow()
		{
			this.BxCmd = null;
		}

		public void linkCmdWindow(UiBoxDesigner _BxCmd)
		{
			this.BxCmd = _BxCmd;
		}

		public void quitDesigner(bool fine_rows = false, bool no_evacuate_clear = false)
		{
			this.blurDesc();
			if (no_evacuate_clear && this.Inventory != null)
			{
				this.Inventory.EvacuateButtonsFromPreviousManager(true);
			}
			if (this.BxR != null)
			{
				this.BxR = null;
				if (this.Inventory != null)
				{
					this.Inventory.releaseDesigner();
					if (fine_rows)
					{
						this.Inventory.fineRows(false);
					}
					this.Inventory = null;
				}
				if (this.APoolEvacuated != null)
				{
					for (int i = this.APoolEvacuated.Count - 1; i >= 0; i--)
					{
						IN.DestroyE(this.APoolEvacuated[i].gameObject);
					}
					this.APoolEvacuated.Clear();
				}
			}
		}

		public bool canOpenEdit()
		{
			return this.Inventory != null && this.Inventory.SelectedRow != null;
		}

		public float inventory_cap_x
		{
			get
			{
				return this.BxR.w / 2f - 42f;
			}
		}

		public float inventory_cap_y
		{
			get
			{
				return this.BxR.h / 2f - 18f;
			}
		}

		protected virtual void initDesignerContentMain()
		{
			this.APoolEvacuated = this.Inventory.EvacuateButtonsFromPreviousManager(false) ?? this.APoolEvacuated;
			if (this.APoolEvacuated == null)
			{
				this.APoolEvacuated = new List<aBtn>(this.Inventory.getVisibleRowCount());
			}
			this.BxR.Clear();
			this.BxR.init();
			this.BxR.effect_confusion = this.effect_confusion;
			float use_w = this.BxR.use_w;
			if (TX.valid(this.title_text_content))
			{
				bool flag = !this.use_topright_counter && this.Inventory.sort_button_bits == 0;
				this.BxR.addP(new DsnDataP("", false)
				{
					text = this.title_text_content,
					html = true,
					TxCol = C32.d2c(4283780170U),
					swidth = (flag ? this.BxR.use_w : 0f),
					alignx = (flag ? ALIGN.CENTER : ALIGN.LEFT)
				}, false);
				this.BxR.Br();
			}
			Designer designer = this.BxR.addTab("item_inventory", use_w, this.BxR.h - this.slice_height - 10f, use_w, this.BxR.h - this.slice_height, false);
			designer.scroll_area_selectable = true;
			designer.radius = 0f;
			this.Inventory.createRowsTo(this, designer, new FnBtnBindings(this.fnClickItemRow), this.stencil_ref, this.item_row_skin);
		}

		public void itemRowInit(aBtnItemRow B, ItemStorage.IRow Row)
		{
			B.setItem(this, this.Inventory, Row);
			if (this.fnItemRowInitAfter != null)
			{
				this.fnItemRowInitAfter(B, Row);
			}
			if (this.isUsingState() && this.hide_list_buttons_when_using)
			{
				B.hide();
			}
		}

		public void redoFnItemRowAfter(UiItemManageBox.FnItemRowInitAfter _fnItemRowInitAfter = null)
		{
			_fnItemRowInitAfter = _fnItemRowInitAfter ?? this.fnItemRowInitAfter;
			if (_fnItemRowInitAfter == null)
			{
				return;
			}
			int itemRowBtnCount = this.Inventory.getItemRowBtnCount();
			for (int i = 0; i < itemRowBtnCount; i++)
			{
				aBtnItemRow itemRowBtnByIndex = this.Inventory.getItemRowBtnByIndex(i);
				this.fnItemRowInitAfter(itemRowBtnByIndex, itemRowBtnByIndex.getItemRow());
			}
		}

		public ItemStorage[] ASupportStorage
		{
			get
			{
				return this.ASupportStorage_;
			}
			set
			{
				this.ASupportStorage_ = value;
			}
		}

		public bool use_touch_grade_memory
		{
			get
			{
				return this.OTouchGrade != null;
			}
			set
			{
				if (value && this.OTouchGrade == null)
				{
					this.OTouchGrade = new BDic<NelItem, uint>();
				}
			}
		}

		public void touchGrade(ItemStorage St)
		{
			if (this.OTouchGrade == null)
			{
				return;
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in St.getWholeInfoDictionary())
			{
				this.touchGrade(keyValuePair.Key, keyValuePair.Value);
			}
		}

		public void touchGrade(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			if (this.OTouchGrade == null)
			{
				return;
			}
			this.OTouchGrade[Itm] = X.Get<NelItem, uint>(this.OTouchGrade, Itm, 0U) | Obt.getGradeUsingBit();
		}

		public bool isGradeTouched(NelItem Itm, ItemStorage.ObtainInfo Obt, int grade, bool check_support_storage = false)
		{
			return (this.getGradeAvailable(Itm, Obt, check_support_storage) & (1U << grade)) > 0U;
		}

		public uint getGradeAvailable(NelItem Itm, ItemStorage.ObtainInfo Obt, bool check_support_storage = false)
		{
			uint num = ((Obt != null) ? Obt.getGradeUsingBit() : 0U);
			if (this.OTouchGrade != null)
			{
				num |= X.Get<NelItem, uint>(this.OTouchGrade, Itm, 0U);
			}
			if (check_support_storage && this.ASupportStorage_ != null)
			{
				int num2 = this.ASupportStorage_.Length;
				for (int i = 0; i < num2; i++)
				{
					ItemStorage.ObtainInfo info = this.ASupportStorage_[i].getInfo(Itm);
					if (info != null)
					{
						num |= info.getGradeUsingBit();
					}
				}
			}
			return num;
		}

		private void initEditItem()
		{
			this.ItemSelectRow = null;
			this.BConItemStars = null;
			this.FbDesc = null;
			if (this.BxDesc == null)
			{
				return;
			}
			this.BxDesc.activate();
			if (this.do_not_remake_desc_box && this.BxDesc.Get("item_name", false) != null)
			{
				BtnContainerRunner btnContainerRunner = this.BxDesc.Get("item_stars", false) as BtnContainerRunner;
				if (btnContainerRunner != null)
				{
					this.BConItemStars = btnContainerRunner.BCon as BtnContainerRadio<aBtn>;
				}
				this.FbDesc = this.BxDesc.Get("item_desc", false) as FillBlock;
				return;
			}
			this.BxDesc.Clear();
			this.BxDesc.effect_confusion = this.effect_confusion;
			this.BxDesc.item_margin_y_px = 0f;
			this.BxDesc.margin_in_lr = 14f;
			this.BxDesc.margin_in_tb = 22f;
			this.BxDesc.init();
			UiItemManageBox.createItemDescDesigner(this.BxDesc, this.use_grade_stars, this.use_under_description, out this.BConItemStars, out this.FbDesc);
		}

		public static void createItemDescDesigner(Designer BxDesc, bool use_grade_stars, bool use_under_description, out BtnContainerRadio<aBtn> BConItemStars, out FillBlock FbDesc)
		{
			BxDesc.addP(new DsnDataP("", false)
			{
				name = "item_name",
				text = "  ",
				alignx = ALIGN.LEFT,
				TxCol = C32.d2c(4283780170U),
				swidth = BxDesc.use_w,
				size = 18f,
				html = true,
				text_auto_wrap = false
			}, false);
			BxDesc.Br();
			DsnDataHr dsnDataHr = new DsnDataHr
			{
				draw_width_rate = 1f,
				margin_t = 6f,
				margin_b = 7f,
				swidth = BxDesc.use_w,
				Col = C32.d2c(4283780170U)
			};
			BxDesc.addHr(dsnDataHr);
			BxDesc.alignx = ALIGN.CENTER;
			if (use_grade_stars)
			{
				int num = 5;
				string[] array = new string[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = "grade_" + i.ToString();
				}
				Designer designer = BxDesc.Br();
				DsnDataRadio dsnDataRadio = new DsnDataRadio();
				dsnDataRadio.unselectable = 2;
				dsnDataRadio.name = "item_stars";
				dsnDataRadio.def = -1;
				dsnDataRadio.keys = array;
				dsnDataRadio.skin = "item_grade_star";
				dsnDataRadio.clms = 1;
				dsnDataRadio.w = BxDesc.use_w;
				dsnDataRadio.h = 20f;
				dsnDataRadio.margin_h = 0;
				dsnDataRadio.margin_w = 0;
				dsnDataRadio.fnChanged = delegate(BtnContainerRadio<aBtn> BCon, int pre_v, int cur_v)
				{
					aBtn aBtn = BCon.Get(cur_v);
					if (aBtn == null)
					{
						return true;
					}
					ButtonSkinItemGrade buttonSkinItemGrade = aBtn.get_Skin() as ButtonSkinItemGrade;
					UiItemManageBox currentManager;
					return buttonSkinItemGrade != null && (currentManager = buttonSkinItemGrade.getCurrentManager()) != null && currentManager.clickGradeStarBtn(aBtn);
				};
				BConItemStars = designer.addRadioT<aBtnNel>(dsnDataRadio);
				BxDesc.Br().addP(new DsnDataP("", false)
				{
					name = "grade_manipulate",
					text = "  ",
					alignx = ALIGN.CENTER,
					TxCol = C32.d2c(4283780170U),
					swidth = BxDesc.use_w,
					text_auto_wrap = false,
					html = true,
					sheight = 20f,
					size = 14f
				}, false);
				BxDesc.Br().addHr(dsnDataHr);
			}
			else
			{
				BConItemStars = null;
			}
			BxDesc.Br();
			float num2 = 200f;
			if (!use_grade_stars && !use_under_description)
			{
				num2 = BxDesc.use_h - 5f;
			}
			else
			{
				if (!use_grade_stars)
				{
					num2 += 130f;
				}
				num2 = X.Mn(num2, BxDesc.use_h * 0.6f);
			}
			BxDesc.addP(new DsnDataP("", false)
			{
				name = "item_detail",
				text = "  ",
				swidth = BxDesc.use_w - 20f,
				sheight = num2,
				text_auto_wrap = true,
				size = 16f,
				html = true,
				letterSpacing = 0.94f,
				alignx = ALIGN.LEFT,
				TxCol = C32.d2c(4283780170U)
			}, true);
			if (use_under_description)
			{
				BxDesc.Br().addHr(dsnDataHr);
				BxDesc.Br();
				FbDesc = BxDesc.addP(new DsnDataP("", false)
				{
					name = "item_desc",
					text = "  ",
					text_auto_wrap = true,
					swidth = BxDesc.use_w - 20f,
					size = 16f,
					sheight = X.Mx(30f, X.Mn(BxDesc.use_h, 130f)),
					html = true,
					letterSpacing = 0.94f,
					alignx = ALIGN.LEFT,
					TxCol = C32.d2c(4283780170U)
				}, false);
			}
			else
			{
				FbDesc = null;
			}
			BxDesc.Br();
		}

		public void fineDescValue(string html_text)
		{
			UiItemManageBox.fineDescValueS(this.FbDesc, html_text);
		}

		private static void fineDescValueS(FillBlock FbDesc, string html_text)
		{
			if (FbDesc != null && !FbDesc.textIs(html_text))
			{
				FbDesc.text_content = html_text;
				bool flag = TX.isEnglishLang();
				FbDesc.lineSpacing = ((TX.countLine(html_text) >= (flag ? 6 : 8)) ? (flag ? 0.8f : 1.04f) : 1.25f);
			}
		}

		public void blurDescTarget(bool auto_run = true)
		{
			this.ItemSelectRow = null;
			if (auto_run)
			{
				this.runEditItem();
			}
		}

		public bool runEditItem()
		{
			return this.state != UiItemManageBox.STATE.OFFLINE && this.runEditItem(this.state == UiItemManageBox.STATE.USING, false, true);
		}

		protected virtual bool runEditItem(bool using_mode, bool first = false, bool execute_select_button = true)
		{
			bool flag = false;
			if (!using_mode)
			{
				if (first)
				{
					if (this.BConItemStars != null)
					{
						this.BxDesc.Get("grade_manipulate", false).setValue(" ");
						this.BConItemStars.setValue(-1, false);
					}
					if (this.grade_cursor_memory >= 0)
					{
						this.grade_cursor = this.grade_cursor_memory;
					}
					this.grade_cursor_memory = -1;
					this.fineItemDetail(using_mode, -1, false, false);
					this.ItemSelectRow = null;
					this.need_blur_checked_row = true;
				}
				if (IN.isCancel())
				{
					SND.Ui.play("cancel", false);
					return false;
				}
				if (this.Inventory.SelectedRow == null)
				{
					return this.alloc_exist_selection;
				}
				if (this.need_blur_checked_row && !first)
				{
					this.Inventory.BlurCheckedRow();
					this.need_blur_checked_row = false;
				}
				if (IN.isUiSortPD())
				{
					this.Inventory.progressSortByLshKey();
				}
				if (this.Inventory.SelectedRow != this.ItemSelectRow)
				{
					ItemStorage.ObtainInfo obtainInfo = ((this.ItemSelectRow != null) ? this.ItemSelectRow.getItemRow().Info : null);
					ItemStorage.ObtainInfo obtainInfo2 = ((this.Inventory.SelectedRow != null) ? this.Inventory.SelectedRow.getItemRow().Info : null);
					bool flag2 = true;
					if (obtainInfo == obtainInfo2)
					{
						this.ItemSelectRow = this.Inventory.SelectedRow;
						this.Inventory.RowReveal(this.ItemSelectRow.transform, false);
						flag2 = this.Inventory.grade_split;
					}
					else
					{
						if (this.ItemSelectRow == null && execute_select_button)
						{
							this.Inventory.SelectedRow.Select(true);
						}
						this.ItemSelectRow = this.Inventory.SelectedRow;
					}
					if (execute_select_button && !this.ItemSelectRow.isSelected())
					{
						this.ItemSelectRow.Select(true);
					}
					if (this.animation_immediate_flag)
					{
						this.Inventory.ScrollAnimationFinalize();
					}
					if (flag2 && this.ItemSelectRow != null)
					{
						NelItem itemData = this.ItemSelectRow.getItemData();
						int num = (this.Inventory.grade_split ? ((int)this.ItemSelectRow.getItemRow().splitted_grade) : this.ItemSelectRow.getItemRow().top_grade);
						if (this.BxDesc != null)
						{
							this.fineItemDetail(using_mode, this.Inventory.grade_split ? num : (-1), true, this.DetailLockRow == null || itemData != this.DetailLockRow.getItemData());
						}
						if (this.fnDetailPrepare != null)
						{
							this.fnDetailPrepare(itemData, this.ItemSelectRow.getItemInfo(), this.ItemSelectRow.getItemRow());
						}
						flag = true;
					}
				}
				this.animation_immediate_flag = false;
			}
			else
			{
				this.fineUsingTarget();
				ItemStorage.ObtainInfo obtainInfo3 = ((this.ItemUsingRow == null) ? null : this.ItemUsingRow.getItemRow().Info);
				if (first)
				{
					if (this.ItemUsingRow != null)
					{
						if (this.OTouchGrade != null)
						{
							this.touchGrade(this.ItemUsingRow.getItemData(), obtainInfo3);
						}
						if (X.bit_count(this.getGradeAvailable(this.ItemUsingRow.getItemData(), obtainInfo3, true)) >= 2)
						{
							if (this.BConItemStars != null)
							{
								this.BxDesc.Get("grade_manipulate", false).setValue("<key ltab/>/<key rtab/>");
							}
							this.grade_cursor_memory = -1;
						}
						else
						{
							this.grade_cursor_memory = this.grade_cursor;
						}
						flag = true;
						if (this.Pr != null)
						{
							this.grade_cursor = this.ItemUsingRow.getItemData().autoFixGradeSelection(obtainInfo3, this.grade_cursor, this.Pr);
						}
						this.checkGradeShift(using_mode, this.ItemUsingRow.getItemData(), obtainInfo3, 0, true, true);
					}
				}
				else
				{
					if (IN.isCancel() || (this.BxCmd != null && !this.BxCmd.isFocused()))
					{
						aBtn aBtn = ((this.BxCmd == null) ? null : this.BxCmd.getBtn("Cancel"));
						if (aBtn != null && aBtn.isActive())
						{
							aBtn.ExecuteOnSubmitKey();
						}
						else
						{
							this.changeState(UiItemManageBox.STATE.SELECT, true);
							SND.Ui.play("cancel", false);
						}
						return true;
					}
					if (obtainInfo3 != null)
					{
						int num2 = 0;
						if (IN.isLTabPD())
						{
							num2 = -1;
						}
						if (IN.isRTabPD())
						{
							num2 = 1;
						}
						if (num2 != 0)
						{
							SND.Ui.play("tool_gradation", false);
							this.checkGradeShift(using_mode, this.ItemUsingRow.getItemData(), obtainInfo3, num2, false, false);
						}
					}
				}
			}
			if (flag)
			{
				this.fineItemStarsCount(using_mode);
			}
			if (this.DetailLockRow != null && !first)
			{
				this.DetailLockRow = null;
			}
			return true;
		}

		private void fineUsingTarget()
		{
			if (this.UsingTarget != null && (this.ItemUsingRow == null || this.ItemUsingRow.getItemData() != this.UsingTarget))
			{
				this.ItemUsingRow = this.Inventory.reselectTargetRow(this.UsingTarget, true, -1);
			}
		}

		public string getDescStr(UiItemManageBox.DESC_ROW d_type)
		{
			if (this.Inventory.SelectedRow != null)
			{
				aBtnItemRow selectedRow = this.Inventory.SelectedRow;
				return this.getDescStr(selectedRow.getItemRow(), d_type, selectedRow.getItemRow().Info.top_grade);
			}
			return this.getDescStr(null, d_type, -1);
		}

		public string getDescStr(NelItem Itm, UiItemManageBox.DESC_ROW d_type, int grade, ItemStorage.ObtainInfo Obt, int count = 0)
		{
			return UiItemManageBox.getDescStrS(this.Inventory, Itm, d_type, grade, Obt, count, this.detail_main_item_effect, this.fnDescAddition, null);
		}

		public string getDescStr(ItemStorage.IRow IR, UiItemManageBox.DESC_ROW d_type, int grade)
		{
			if (IR == null)
			{
				return UiItemManageBox.getDescStrS(this.Inventory, null, d_type, grade, null, 0, this.detail_main_item_effect, this.fnDescAddition, null);
			}
			return UiItemManageBox.getDescStrS(this.Inventory, IR.Data, d_type, grade, IR.Info, IR.total, this.detail_main_item_effect, this.fnDescAddition, IR);
		}

		public static string getDescStrS(ItemStorage Inventory, NelItem Itm, UiItemManageBox.DESC_ROW d_type, int grade, ItemStorage.ObtainInfo Obt, int count = 0, bool detail_main_item_effect = true, UiItemManageBox.FnDescAddition fnDescAddition = null, ItemStorage.IRow IR = null)
		{
			string text;
			switch (d_type)
			{
			case UiItemManageBox.DESC_ROW.NAME:
				text = Itm.getLocalizedName((grade < 0) ? Obt.top_grade : grade, Inventory);
				if (Inventory.FD_RowNameAddition != null && IR != null)
				{
					text = Inventory.FD_RowNameAddition(IR, Inventory, text);
					goto IL_00E5;
				}
				goto IL_00E5;
			case UiItemManageBox.DESC_ROW.GRADE:
				text = ((Obt == null) ? "0" : Obt.getCount(grade).ToString());
				goto IL_00E5;
			case UiItemManageBox.DESC_ROW.DETAIL:
				text = Itm.getDetail(Inventory, grade, Obt, detail_main_item_effect, true, true);
				goto IL_00E5;
			case UiItemManageBox.DESC_ROW.ROW_COUNT:
				text = Itm.getCountString(count, Inventory);
				goto IL_00E5;
			case UiItemManageBox.DESC_ROW.DESC:
				text = Itm.getDescLocalized(Inventory, grade);
				goto IL_00E5;
			case UiItemManageBox.DESC_ROW.TOPRIGHT_COUNTER:
				text = (Inventory.infinit_stockable ? "" : (Inventory.getVisibleRowCount().ToString() + "/" + Inventory.row_max.ToString()));
				goto IL_00E5;
			}
			text = "";
			IL_00E5:
			if (fnDescAddition != null && (IR == null || !IR.is_fake_row))
			{
				string text2 = fnDescAddition(Itm, d_type, text, grade, Obt, count);
				if (text2 != null)
				{
					text = text2;
				}
			}
			return text;
		}

		public void fineItemStarsCount(bool using_mode)
		{
			if (this.BConItemStars == null)
			{
				return;
			}
			int num = 5;
			NelItem nelItem = null;
			ItemStorage.ObtainInfo obtainInfo = null;
			if (!this.getCurrentFocusItem(using_mode, out nelItem, out obtainInfo))
			{
				if (!using_mode)
				{
					return;
				}
				nelItem = this.UsingTarget;
			}
			for (int i = 0; i < num; i++)
			{
				(this.BConItemStars.Get(i).get_Skin() as ButtonSkinItemGrade).fineItem(nelItem, obtainInfo, i, this);
			}
		}

		private bool checkGradeShiftOne(ref int g, NelItem Itm, ItemStorage.ObtainInfo Obt, int grade_shift, bool check_support_storage = true)
		{
			bool flag = false;
			if (grade_shift == 0 && (this.Inventory.auto_splice_zero_row || !this.Inventory.grade_split))
			{
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 5; j++)
					{
						int num = (g + ((j == 0) ? 0 : ((j + 1) / 2 * X.MPF(j % 2 == 1))) + 10) % 5;
						if (!((i == 0) ? (Obt == null || Obt.getCount(num) == 0) : (!this.isGradeTouched(Itm, Obt, num, false))))
						{
							g = num;
							flag = true;
							break;
						}
					}
					if (flag || this.OTouchGrade == null || !check_support_storage)
					{
						break;
					}
				}
			}
			else
			{
				for (int k = 0; k < 5; k++)
				{
					int num2;
					if (grade_shift == 0)
					{
						num2 = (g + ((k == 0) ? 0 : ((k + 1) / 2 * X.MPF(k % 2 == 1))) + 10) % 5;
					}
					else
					{
						num2 = (g + grade_shift * k + 10) % 5;
					}
					for (int l = 0; l < 2; l++)
					{
						if (!((l == 0) ? (Obt == null || Obt.getCount(num2) == 0) : (!this.isGradeTouched(Itm, Obt, num2, check_support_storage))))
						{
							g = num2;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				check_support_storage = false;
			}
			g = (g + 5) % 5;
			if (!flag && check_support_storage && this.ASupportStorage_ != null)
			{
				int num3 = this.ASupportStorage_.Length;
				for (int m = 0; m < num3; m++)
				{
					if (this.checkGradeShiftOne(ref g, Itm, this.ASupportStorage_[m].getInfo(Itm), grade_shift, false))
					{
						return true;
					}
				}
			}
			return flag;
		}

		protected void checkGradeShift(bool using_mode, NelItem Itm, ItemStorage.ObtainInfo Obt, int grade_shift, bool contain_current_grade = false, bool force = false)
		{
			int num = this.grade_cursor + (contain_current_grade ? 0 : grade_shift);
			this.checkGradeShiftOne(ref num, Itm, Obt, grade_shift, true);
			if (this.grade_cursor != num || force)
			{
				this.grade_cursor = num;
				if (this.BxDesc != null)
				{
					this.BConItemStars.setValue(this.grade_cursor, false);
					if (using_mode)
					{
						this.fineItemDetail(using_mode, this.grade_cursor, true, false);
					}
				}
				if (this.Inventory.grade_split)
				{
					aBtnItemRow aBtnItemRow = this.Inventory.reselectTargetRow(Itm, this.Inventory.getIRowForGrade(Itm, Obt, this.grade_cursor, null), true);
					if (aBtnItemRow != null)
					{
						this.ItemUsingRow = aBtnItemRow;
					}
				}
				this.repositItemUsingCommand();
				this.changeGradeCursorInUsing(Itm, Obt, this.grade_cursor);
			}
		}

		private bool clickGradeStarBtn(aBtn B)
		{
			if (!this.isUsingState())
			{
				if (this.ItemSelectRow == null)
				{
					return false;
				}
				ItemStorage.ObtainInfo itemInfo = this.ItemSelectRow.getItemInfo();
				if (itemInfo == null)
				{
					return false;
				}
				int num = X.NmI(TX.slice(B.title, 6), this.grade_cursor, false, false);
				if (!this.isGradeTouched(this.ItemSelectRow.getItemData(), itemInfo, num, true) && itemInfo.getCount(num) == 0)
				{
					return false;
				}
				this.grade_cursor = num;
				this.ItemSelectRow.ExecuteOnClick();
				if (this.isUsingState() && this.UsingTarget != null && this.ItemUsingRow != null && this.grade_cursor != num)
				{
					this.grade_cursor = num;
					this.checkGradeShift(true, this.ItemUsingRow.getItemData(), this.ItemUsingRow.getItemInfo(), 1, true, true);
				}
			}
			else
			{
				if (this.UsingTarget == null || this.ItemUsingRow == null)
				{
					return false;
				}
				ItemStorage.ObtainInfo itemInfo2 = this.ItemUsingRow.getItemInfo();
				if (itemInfo2 == null)
				{
					return false;
				}
				int num2 = X.NmI(TX.slice(B.title, 6), this.grade_cursor, false, false);
				if (this.grade_cursor == num2 || (!this.isGradeTouched(this.ItemUsingRow.getItemData(), itemInfo2, num2, true) && itemInfo2.getCount(num2) == 0))
				{
					return false;
				}
				this.grade_cursor = num2;
				this.checkGradeShift(true, this.ItemUsingRow.getItemData(), itemInfo2, 0, true, true);
			}
			return true;
		}

		protected virtual void changeGradeCursorInUsing(NelItem Itm, ItemStorage.ObtainInfo Obt, int grade)
		{
			if (this.fnGradeFocusChange != null)
			{
				this.fnGradeFocusChange(Itm, Obt, grade);
			}
		}

		public bool getCurrentFocusItem(bool using_mode, out NelItem Itm, out ItemStorage.ObtainInfo Obt)
		{
			if (using_mode)
			{
				this.fineUsingTarget();
				Itm = ((this.ItemUsingRow != null) ? this.ItemUsingRow.getItemData() : this.UsingTarget);
				Obt = ((this.ItemUsingRow != null) ? this.ItemUsingRow.getItemRow().Info : ((Itm != null) ? this.Inventory.getInfo(Itm) : null));
			}
			else
			{
				Itm = ((this.ItemSelectRow != null) ? this.ItemSelectRow.getItemData() : null);
				Obt = ((this.ItemSelectRow != null) ? this.ItemSelectRow.getItemRow().Info : null);
			}
			return Itm != null;
		}

		public bool getCurrentFocusItem(bool using_mode, out ItemStorage.IRow IR)
		{
			if (using_mode)
			{
				this.fineUsingTarget();
				IR = ((this.ItemUsingRow != null) ? this.ItemUsingRow.getItemRow() : null);
			}
			else
			{
				IR = ((this.ItemSelectRow != null) ? this.ItemSelectRow.getItemRow() : null);
			}
			return IR != null;
		}

		private void fineItemDetail(bool using_mode, int grade, bool fine_name = false, bool fine_desc = false)
		{
			if (this.BxDesc == null)
			{
				return;
			}
			ItemStorage.IRow row;
			if (!this.getCurrentFocusItem(using_mode, out row))
			{
				return;
			}
			UiItemManageBox.fineItemDetailS(this.BxDesc, this.FbDesc, this.Inventory, row.Data, row.Info, grade, fine_name, fine_desc, this.detail_main_item_effect, this.fnDescAddition, row);
		}

		public static void fineItemDetailS(Designer BxDesc, FillBlock FbDesc, ItemStorage Inventory, NelItem Itm, ItemStorage.ObtainInfo Obt, int grade, bool fine_name = false, bool fine_desc = false, bool detail_main_item_effect = true, UiItemManageBox.FnDescAddition fnDescAddition = null, ItemStorage.IRow IR = null)
		{
			string descStrS = UiItemManageBox.getDescStrS(Inventory, Itm, UiItemManageBox.DESC_ROW.DETAIL, grade, Obt, 0, detail_main_item_effect, fnDescAddition, null);
			FillBlock fillBlock = BxDesc.Get("item_detail", false) as FillBlock;
			if (fillBlock != null)
			{
				fillBlock.lineSpacing = ((TX.countLine(descStrS) >= 6) ? 1.06f : 1.38f);
				fillBlock.setValue(descStrS);
				BxDesc.RowRemakeHeightRecalc(fillBlock, null);
				if (FbDesc != null)
				{
					FbDesc.aligny = ((fillBlock.get_sheight_px() > fillBlock.heightPixel) ? ALIGNY.TOP : ALIGNY.MIDDLE);
				}
			}
			if (fine_name)
			{
				IVariableObject variableObject = BxDesc.Get("item_name", false);
				if (variableObject != null)
				{
					variableObject.setValue(UiItemManageBox.getDescStrS(Inventory, Itm, UiItemManageBox.DESC_ROW.NAME, grade, Obt, 0, detail_main_item_effect, fnDescAddition, IR));
				}
			}
			if (fine_desc && FbDesc != null)
			{
				UiItemManageBox.fineDescValueS(FbDesc, UiItemManageBox.getDescStrS(Inventory, Itm, UiItemManageBox.DESC_ROW.DESC, grade, Obt, 0, detail_main_item_effect, fnDescAddition, null));
			}
		}

		private bool fnClickItemRow(aBtn B)
		{
			if (!this.can_handle || this.Inventory == null)
			{
				return false;
			}
			if (this.state == UiItemManageBox.STATE.USING)
			{
				this.changeState(UiItemManageBox.STATE.SELECT, true);
			}
			aBtnItemRow aBtnItemRow = B as aBtnItemRow;
			if (aBtnItemRow == null || aBtnItemRow.getItemRow() == null || aBtnItemRow.getItemRow().is_fake_row)
			{
				SND.Ui.play("talk_progress", false);
				return false;
			}
			this.ItemUsingRow = aBtnItemRow;
			if (this.BxCmd == null)
			{
				this.UsingTarget = aBtnItemRow.getItemData();
				this.need_blur_checked_row = true;
				if (this.fnCommandPrepare != null)
				{
					this.fnCommandPrepare(this, null, this.ItemUsingRow);
				}
				this.UsingTarget = null;
				this.ItemUsingRow = null;
				return false;
			}
			if (this.Inventory.grade_split)
			{
				this.grade_cursor = (int)aBtnItemRow.getItemRow().splitted_grade;
			}
			this.changeState(UiItemManageBox.STATE.USING, true);
			if (!this.fineItemUsingCommand(aBtnItemRow))
			{
				this.ItemUsingRow = null;
				this.changeState(UiItemManageBox.STATE.SELECT, true);
				return false;
			}
			return true;
		}

		protected virtual bool fineItemUsingCommand(aBtnItemRow Bi)
		{
			int num = -1;
			if (Bi == null)
			{
				return false;
			}
			if (this.BxCmd == null)
			{
				if (this.fnCommandPrepare != null)
				{
					this.fnCommandPrepare(this, null, this.ItemUsingRow);
				}
				return false;
			}
			if (this.BxCmd.isActive() && this.BxCmd.initted)
			{
				BtnContainer<aBtn> btnContainer = this.BxCmd.getBtnContainer();
				num = ((btnContainer != null) ? btnContainer.getIndex(aBtn.PreSelected) : (-1));
			}
			this.BxCmd.Clear();
			this.BxCmd.activate();
			this.BxCmd.WH(0f, 0f);
			this.BxCmd.margin_in_lr = 38f;
			this.BxCmd.margin_in_tb = 31f;
			this.BxCmd.selectable_loop = 1;
			this.BxCmd.item_margin_x_px = 0f;
			this.BxCmd.item_margin_y_px = 0f;
			this.BxCmd.box_stencil_ref_mask = -1;
			this.BxCmd.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			UiBoxDesigner.FocusTo(this.BxCmd);
			this.Inventory.FocusSelectedRowTo(Bi, true);
			if (this.fnCommandPrepare == null || this.fnCommandKeysPrepare != null)
			{
				this.BxCmd.WH(this.cmd_w, this.BxCmd.h);
				this.BxCmd.init();
				if (!this.createBasicCommandTo(this.BxCmd, (this.IMNG != null) ? this.IMNG.M2D : null, this.ItemUsingRow))
				{
					if (this.BxCmd != null)
					{
						this.BxCmd.deactivate(true);
					}
				}
				else
				{
					this.BxCmd.WH(this.cmd_w, this.BxCmd.maxh_pixel + this.BxCmd.margin_in_tb * 2f);
					this.BxCmd.getBtn(X.MMX(0, num, this.BxCmd.getBtnContainer().Length - 1)).Select(false);
					if (this.fnCommandPrepare != null && !this.fnCommandPrepare(this, this.BxCmd, this.ItemUsingRow))
					{
						return false;
					}
					this.repositItemUsingCommand();
				}
			}
			else if (!this.fnCommandPrepare(this, this.BxCmd, this.ItemUsingRow))
			{
				return false;
			}
			if (this.hide_list_buttons_when_using)
			{
				this.Inventory.hideBCon();
			}
			return true;
		}

		public void repositItemUsingCommand()
		{
			if (this.BxCmd == null)
			{
				return;
			}
			Vector3 vector;
			if (this.BConItemStars != null)
			{
				vector = this.BConItemStars.Get(this.grade_cursor).transform.position;
			}
			else if (this.ItemUsingRow != null)
			{
				vector = this.ItemUsingRow.transform.position;
			}
			else
			{
				vector = Vector3.zero;
			}
			this.BxCmd.positionD(vector.x * 64f + (94f + this.grade_text_area_width) * 0.5f + 220f + this.cmd_w * 0.5f, vector.y * 64f + 25f + 10f - this.BxCmd.h * 0.5f, 0, 20f);
		}

		public bool fineCount(NelItem Itm, ItemStorage.ObtainInfo Obt, bool quit_on_zero = false)
		{
			IVariableObject variableObject = ((this.BxDesc != null) ? this.BxDesc.Get("item_name", false) : null);
			if (this.state == UiItemManageBox.STATE.USING)
			{
				this.fineUsingTarget();
				Itm = this.UsingTarget;
				if (this.ItemUsingRow != null)
				{
					Obt = this.ItemUsingRow.getItemRow().Info;
				}
				if (this.ItemUsingRow != this.Inventory.SelectedRow)
				{
					bool flag = true;
					if (this.Inventory.SelectedRow == null)
					{
						flag = false;
					}
					else if (this.Inventory.SelectedRow.getItemData() != Itm)
					{
						flag = false;
					}
					if (!flag)
					{
						if (this.Inventory.auto_splice_zero_row)
						{
							this.ItemUsingRow = null;
						}
						if (!quit_on_zero)
						{
							this.fineItemStarsCount(true);
							this.fineItemDetail(true, this.grade_cursor, false, false);
							if (variableObject != null)
							{
								variableObject.setValue(this.getDescStr(Itm, UiItemManageBox.DESC_ROW.NAME, this.grade_cursor, Obt, 0));
							}
							if (this.Inventory.auto_update_topright_counter)
							{
								this.fineTopRightCounter();
							}
						}
						return false;
					}
					this.ItemUsingRow = this.Inventory.SelectedRow;
				}
				this.checkGradeShift(true, Itm, Obt, 0, true, false);
			}
			if (this.Inventory.auto_update_topright_counter)
			{
				this.fineTopRightCounter();
			}
			this.fineItemStarsCount(true);
			if (variableObject != null)
			{
				variableObject.setValue(this.getDescStr(Itm, UiItemManageBox.DESC_ROW.NAME, this.grade_cursor, Obt, 0));
			}
			return true;
		}

		public bool fineCount(bool quit_on_zero = false)
		{
			NelItem nelItem = null;
			ItemStorage.ObtainInfo obtainInfo = null;
			return this.getCurrentFocusItem(this.state == UiItemManageBox.STATE.USING, out nelItem, out obtainInfo) && this.fineCount(nelItem, obtainInfo, quit_on_zero);
		}

		private void closeUsingState()
		{
			NelItem nelItem;
			ItemStorage.ObtainInfo obtainInfo;
			this.getCurrentFocusItem(true, out nelItem, out obtainInfo);
			this.closeUsingState(nelItem ?? this.UsingTarget, obtainInfo);
		}

		protected virtual void closeUsingState(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			if (Itm == null)
			{
				return;
			}
			if (this.fnGradeFocusChange != null)
			{
				this.fnGradeFocusChange(Itm, Obt, -1);
			}
		}

		private bool createBasicCommandTo(UiBoxDesigner Ds, NelM2DBase M2D, aBtnItemRow ItmRow)
		{
			Ds.alignx = ALIGN.CENTER;
			List<string> list = new List<string>();
			NelItem itemData = ItmRow.getItemData();
			itemData.prepareResource(M2D);
			this.Inventory.FocusSelectedRowTo(ItmRow, true);
			if (this.Pr != null)
			{
				if (itemData.is_water && itemData.is_food)
				{
					if (!this.Pr.is_alive || this.Pr.NM2D.Iris.isWaiting(this.Pr) || this.Pr.MyStomach.getEatableLevel((float)itemData.RecipeInfo.DishInfo.cost) <= 0f)
					{
						list.Add("drink_useless");
					}
					else
					{
						list.Add("drink");
					}
				}
				else if (itemData.useable)
				{
					list.Add((itemData.Use(this.Pr, this.Inventory, 0, null) > 0 && this.Pr.canUseItem(itemData, this.Inventory, true) == "") ? "use" : "useless");
				}
			}
			list.Add("Cancel");
			if (itemData.is_water && !this.Inventory.water_stockable && !itemData.is_noel_water)
			{
				list.Add("discard_water");
			}
			else if (itemData.dropable || ItmRow.getItemInfo().total <= 0)
			{
				list.Add("drop");
				if (!itemData.is_noel_water && itemData.stock > 1)
				{
					list.Add("discard_row");
				}
			}
			if (this.fnCommandKeysPrepare != null)
			{
				list = this.fnCommandKeysPrepare(Ds, ItmRow, list);
				if (list == null)
				{
					return false;
				}
			}
			int count = list.Count;
			aBtn aBtn = null;
			aBtn aBtn2 = null;
			for (int i = 0; i < count; i++)
			{
				string text = list[i];
				string text2 = (TX.isStart(text, "&&", 0) ? TX.slice(text, 2) : ((text == "Cancel") ? text : ("Item_cmd_" + text)));
				aBtn aBtn3 = Ds.addButton(new DsnDataButton
				{
					name = text,
					title = text,
					skin = "row",
					skin_title = TX.Get(text2, ""),
					w = Ds.use_w,
					h = 28f,
					fnClick = new FnBtnBindings(this.fnClickItemCmd)
				});
				if (text == "Cancel")
				{
					aBtn3.click_snd = "cancel";
					aBtn3.click_to_select = false;
				}
				else
				{
					if (text == "useless")
					{
						aBtn3.click_snd = "locked";
					}
					if (text == "drop" || text == "discard_row")
					{
						aBtn3.click_snd = "reset_var";
					}
					if (text == "discard_water")
					{
						aBtn3.click_snd = "discard_water";
					}
				}
				Ds.Br();
				if (aBtn2 == null)
				{
					aBtn2 = aBtn3;
				}
				if (aBtn != null)
				{
					aBtn3.setNaviT(aBtn, true, true);
				}
				aBtn = aBtn3;
			}
			if (count > 1)
			{
				aBtn2.setNaviT(aBtn, true, true);
			}
			return true;
		}

		public bool canUseItemCheck(NelItem Itm, bool for_using = true)
		{
			string text = this.Pr.canUseItem(Itm, this.Inventory, for_using);
			if (text != "")
			{
				this.errorMessageToDesc(text);
				return true;
			}
			return false;
		}

		public bool fnClickItemCmd(aBtn B)
		{
			if (B.title != "Cancel")
			{
				if (this.UsingTarget == null)
				{
					return false;
				}
				this.fineUsingTarget();
				if (this.ItemUsingRow == null || this.ItemUsingRow != this.Inventory.SelectedRow)
				{
					return false;
				}
			}
			NelItem usingTarget = this.UsingTarget;
			int num = this.grade_cursor;
			aBtnItemRow itemUsingRow = this.ItemUsingRow;
			ItemStorage.ObtainInfo obtainInfo;
			if (itemUsingRow == null)
			{
				obtainInfo = null;
			}
			else
			{
				ItemStorage.IRow itemRow = itemUsingRow.getItemRow();
				obtainInfo = ((itemRow != null) ? itemRow.Info : null);
			}
			ItemStorage.ObtainInfo obtainInfo2 = obtainInfo;
			string title = B.title;
			if (title != null)
			{
				uint num2 = <PrivateImplementationDetails>.ComputeStringHash(title);
				bool flag;
				if (num2 <= 1469170932U)
				{
					if (num2 <= 584542933U)
					{
						if (num2 != 359939985U)
						{
							if (num2 != 584542933U)
							{
								goto IL_04C4;
							}
							if (!(title == "discard_water"))
							{
								goto IL_04C4;
							}
						}
						else
						{
							if (!(title == "drink"))
							{
								goto IL_04C4;
							}
							if (usingTarget.is_food && usingTarget.is_water)
							{
								this.Pr.MyStomach.addEffect(usingTarget.RecipeInfo.DishInfo, true, true);
								this.Pr.UP.useItem(usingTarget, "food");
								flag = this.Inventory.Reduce(usingTarget, 1, num, true);
								this.ItemUsingRow = null;
								if (this.fnRunUsePost != null)
								{
									this.fnRunUsePost(B.title, usingTarget, obtainInfo2, flag);
								}
								this.changeState(UiItemManageBox.STATE.SELECT, true);
								return false;
							}
							return true;
						}
					}
					else if (num2 != 900713019U)
					{
						if (num2 != 1469170932U)
						{
							goto IL_04C4;
						}
						if (!(title == "use"))
						{
							goto IL_04C4;
						}
					}
					else
					{
						if (!(title == "Cancel"))
						{
							goto IL_04C4;
						}
						this.changeState(UiItemManageBox.STATE.SELECT, true);
						return true;
					}
				}
				else if (num2 <= 2685783248U)
				{
					if (num2 != 2591508230U)
					{
						if (num2 != 2685783248U)
						{
							goto IL_04C4;
						}
						if (!(title == "drink_useless"))
						{
							goto IL_04C4;
						}
						this.errorMessageToDesc(TX.Get("Item_desc_drink_cannot_use", ""));
						return false;
					}
					else if (!(title == "discard_row"))
					{
						goto IL_04C4;
					}
				}
				else if (num2 != 2846199180U)
				{
					if (num2 != 4166810253U)
					{
						goto IL_04C4;
					}
					if (!(title == "useless"))
					{
						goto IL_04C4;
					}
					string text = this.Pr.canUseItem(usingTarget, this.Inventory, true);
					if (text == "")
					{
						text = TX.Get("Item_desc_cannot_use", "");
					}
					this.errorMessageToDesc(text);
					return this.canUseItemCheck(usingTarget, true);
				}
				else if (!(title == "drop"))
				{
					goto IL_04C4;
				}
				if (this.canUseItemCheck(usingTarget, B.title == "use"))
				{
					return true;
				}
				if (obtainInfo2.total <= 0)
				{
					this.Inventory.getWholeInfoDictionary().Remove(usingTarget);
					this.Inventory.fineRows(false);
					this.changeState(UiItemManageBox.STATE.SELECT, true);
					return true;
				}
				if (obtainInfo2.getCount(num) == 0)
				{
					return true;
				}
				if (this.IMNG != null)
				{
					this.IMNG.fine_drop_state = true;
				}
				flag = false;
				if (B.title == "use")
				{
					flag = this.Inventory.Reduce(usingTarget, 1, num, true);
					if (usingTarget.Use(this.Pr, this.Inventory, this.grade_cursor, this.Pr) == 2)
					{
						this.quit_whole_ui = true;
					}
					UIPicture.Instance.useItem(usingTarget, null);
				}
				else if (B.title == "drop")
				{
					if (this.cannotDropable(usingTarget, num, true))
					{
						return false;
					}
					if (this.IMNG != null)
					{
						flag = this.Inventory.Reduce(usingTarget, 1, num, true);
						this.IMNG.DiscardStack(usingTarget, 1, num);
					}
				}
				else
				{
					if (this.cannotDropable(usingTarget, num, true))
					{
						return false;
					}
					using (BList<int> blist = ListBuffer<int>.Pop(5))
					{
						this.Inventory.ReduceOneRow(usingTarget, num, true, blist);
						flag = true;
						if (this.IMNG != null)
						{
							int count = blist.Count;
							for (int i = 0; i < count; i++)
							{
								int num3 = blist[i];
								this.IMNG.DiscardStack(usingTarget, num3, i);
							}
						}
					}
				}
				if (this.fnRunUsePost != null)
				{
					this.fnRunUsePost(B.title, usingTarget, obtainInfo2, flag);
				}
				if (B.title == "use" && usingTarget.Use(this.Pr, this.Inventory, num, null) == 0)
				{
					flag = true;
				}
				if (!this.fineCount(usingTarget, obtainInfo2, true) || this.quit_whole_ui)
				{
					this.ItemUsingRow = null;
					this.changeState(UiItemManageBox.STATE.SELECT, true);
					return true;
				}
				if (flag && !this.fineItemUsingCommand(this.ItemUsingRow))
				{
					this.ItemUsingRow = null;
					this.changeState(UiItemManageBox.STATE.SELECT, true);
					return true;
				}
				return true;
			}
			IL_04C4:
			if (this.fnCommandBtnExecuted != null)
			{
				this.fnCommandBtnExecuted(this.ItemUsingRow, B.title);
			}
			return true;
		}

		public void fineTopRightCounter()
		{
			if (this.Inventory != null && this.Inventory.hasTopRightCuonter())
			{
				this.Inventory.setTopRightCounter(this.getDescStr(UiItemManageBox.DESC_ROW.TOPRIGHT_COUNTER));
			}
		}

		public void changeStateToSelect()
		{
			this.changeState(UiItemManageBox.STATE.SELECT, true);
		}

		public bool cannotDropable(NelItem Data, int target_grade, bool set_errormsg = false)
		{
			if (M2DBase.Instance == null || M2DBase.Instance.curMap == null)
			{
				return false;
			}
			if (this.IMNG.isHoldingItemByPR(Data, target_grade))
			{
				if (set_errormsg)
				{
					this.errorMessageToDesc(TX.Get("Item_desc_decline_undropable_now", ""));
				}
				return true;
			}
			string s = M2DBase.Instance.curMap.Meta.GetS("cannot_dropable");
			if (TX.noe(s))
			{
				return false;
			}
			if (TX.eval(s, "") != 0.0)
			{
				if (set_errormsg)
				{
					this.errorMessageToDesc(TX.Get("Item_desc_decline_undropable", ""));
				}
				return true;
			}
			return false;
		}

		public void errorMessageToDesc(string desc)
		{
			if (this.BxDesc != null)
			{
				SND.Ui.play("locked", false);
				if (this.FbDesc != null)
				{
					this.DetailLockRow = ((this.state == UiItemManageBox.STATE.USING) ? (this.ItemUsingRow ?? this.ItemSelectRow) : this.ItemSelectRow);
					this.fineDescValue(NEL.error_tag + desc + NEL.error_tag_close);
				}
			}
		}

		public BDic<NelItem, uint> getTouchedGradeDictionary()
		{
			return this.OTouchGrade;
		}

		public void mergeTouchedGradeDictionary(BDic<NelItem, uint> O)
		{
			UiItemManageBox.mergeTouchedGradeDictionary(this.OTouchGrade, O);
		}

		public static void mergeTouchedGradeDictionary(BDic<NelItem, uint> ODest, BDic<NelItem, uint> OSrc)
		{
			if (ODest != null && OSrc != null)
			{
				foreach (KeyValuePair<NelItem, uint> keyValuePair in OSrc)
				{
					uint num;
					if (ODest.TryGetValue(keyValuePair.Key, out num))
					{
						ODest[keyValuePair.Key] = num | keyValuePair.Value;
					}
					else
					{
						ODest[keyValuePair.Key] = keyValuePair.Value;
					}
				}
			}
		}

		public aBtnItemRow getSelectingRowBtn()
		{
			return this.ItemSelectRow;
		}

		public aBtnItemRow getUsingRowBtn()
		{
			return this.ItemUsingRow;
		}

		public NelItem SelectingTarget
		{
			get
			{
				if (!(this.ItemSelectRow != null))
				{
					return null;
				}
				return this.ItemSelectRow.getItemData();
			}
		}

		public bool isUsingState()
		{
			return this.state == UiItemManageBox.STATE.USING;
		}

		public void BlurSelectingRowBtn()
		{
			this.ItemSelectRow = null;
		}

		public int get_grade_cursor()
		{
			return this.grade_cursor;
		}

		public readonly NelItemManager IMNG;

		private aBtnItemRow ItemSelectRow;

		protected aBtnItemRow ItemUsingRow;

		public NelItem UsingTarget;

		protected int grade_cursor;

		public ItemStorage Inventory;

		public UiBoxDesigner ParentBoxDesigner;

		private Designer BxR;

		private UiBoxDesigner BxDesc;

		protected UiBoxDesigner BxCmd;

		public Transform TrsEvacuateTo;

		public List<aBtn> APoolEvacuated;

		public PR Pr;

		public string title_text_content;

		public bool effect_confusion;

		public bool use_grade_stars = true;

		public bool use_under_description = true;

		public bool selectable_loop = true;

		public bool auto_select_on_adding_row = true;

		public byte lr_slide_row = 10;

		public bool manager_auto_run_on_select_row;

		public bool do_not_remake_desc_box;

		public bool detail_main_item_effect = true;

		public bool can_handle = true;

		public bool use_topright_counter = true;

		public bool quit_whole_ui;

		public bool hide_list_buttons_when_using;

		public string item_row_skin = "normal";

		public float slice_height = 120f;

		public float row_height = 30f;

		public float topright_desc_width;

		public bool animation_immediate_flag;

		public int stencil_ref = -1;

		public float cmd_w = 320f;

		public float grade_text_area_width = 24f;

		public bool alloc_exist_selection = true;

		public UiItemManageBox.FnDetailPrepare fnDetailPrepare;

		public UiItemManageBox.FnDescAddition fnDescAddition;

		public UiItemManageBox.FnGradeFocusChange fnGradeFocusChange;

		public UiItemManageBox.FnItemRowInitAfter fnItemRowInitAfter;

		public UiItemManageBox.FnWholeRowsPrepare fnWholeRowsPrepare;

		public UiItemManageBox.FnSortOverride fnSortInjectMng;

		public UiItemManageBox.FnCommandPrepare fnCommandPrepare;

		public UiItemManageBox.FnCommandKeysPrepare fnCommandKeysPrepare;

		public UiItemManageBox.FnCommandBtnExecuted fnCommandBtnExecuted;

		private ItemStorage[] ASupportStorage_;

		private int grade_cursor_memory = -1;

		private aBtnItemRow DetailLockRow;

		protected BDic<NelItem, uint> OTouchGrade;

		public Func<string, NelItem, ItemStorage.ObtainInfo, bool, bool> fnRunUsePost;

		public bool need_blur_checked_row;

		protected UiItemManageBox.STATE state;

		private BtnContainerRadio<aBtn> BConItemStars;

		private FillBlock FbDesc;

		public delegate void FnDetailPrepare(NelItem Itm, ItemStorage.ObtainInfo Obt, ItemStorage.IRow IR);

		public delegate void FnWholeRowsPrepare(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest);

		public delegate string FnDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count);

		public delegate string FnRowNameAddition(ItemStorage.IRow Row, ItemStorage Storage, string def_string);

		public delegate int FnRowIconAddition(ItemStorage.IRow Row, ItemStorage Storage, int def_ico);

		public delegate void FnGradeFocusChange(NelItem Itm, ItemStorage.ObtainInfo Obt, int grade);

		public delegate void FnItemRowInitAfter(aBtnItemRow B, ItemStorage.IRow IRow);

		public delegate bool FnCommandPrepare(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow RowB);

		public delegate List<string> FnCommandKeysPrepare(UiBoxDesigner BxCmd, aBtnItemRow RowB, List<string> Adefault);

		public delegate void FnCommandBtnExecuted(aBtnItemRow RowB, string s);

		public delegate bool FnSortOverride(ItemStorage.IRow Ra, ItemStorage.IRow Rb, ItemStorage.SORT_TYPE sort_type, out int ret);

		protected enum STATE
		{
			OFFLINE,
			SELECT,
			USING
		}

		public enum DESC_ROW
		{
			NAME,
			GRADE,
			ROW_PRICE,
			DETAIL,
			ROW_COUNT,
			DESC,
			TOPRIGHT_COUNTER
		}
	}
}
