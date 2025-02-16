using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel.mgm.smncr
{
	public class UiSmncEnemyEditor
	{
		private float bx_w
		{
			get
			{
				return IN.w * 0.58f;
			}
		}

		private float bx_h
		{
			get
			{
				return IN.h * 0.58f;
			}
		}

		private UiSmncEnemyEditor.STATE state
		{
			get
			{
				return this.state_;
			}
			set
			{
				if (this.state == value)
				{
					return;
				}
				this.state_ = value;
				this.Con.need_fine_kd = true;
			}
		}

		public UiSmncEnemyEditor(UiSmnCreator _Con, UiBoxDesigner _Bx, UiBoxDesignerFamily _DsFam, M2LpUiSmnCreator _LpArea)
		{
			this.Con = _Con;
			this.Bx = _Bx;
			this.DsFam = _DsFam;
			this.LpArea = _LpArea;
			this.M2D = this.LpArea.nM2D;
			this.DesPrevious = new Designer.EvacuateContainer(this.Bx, true);
			this.DesCurrent = default(Designer.EvacuateContainer);
			this.bx_pre_w = this.Bx.w;
			this.bx_pre_h = this.Bx.h;
			this.bx_pre_dx = this.Bx.getBox().get_deperture_x();
			this.bx_pre_dy = this.Bx.getBox().get_deperture_y();
			this.LpArea.nM2D.hideAreaTitle(false);
			this.AdefeatSrc = UiEnemyDex.getListupDefeated(null, true, false);
			this.AdefeatCur = new List<EnemyEntry>(this.AdefeatSrc.Count);
			this.createUi();
			this.Unlocker = new UiSmncEnemyUnlocker(this, this.DsFam, this.LpArea);
		}

		public void destruct()
		{
			this.DesPrevious.release(null);
			this.DesCurrent.release(null);
			this.Unlocker.destruct();
		}

		private void createUi()
		{
			this.Bx.Clear();
			this.Bx.WHanim(this.bx_w, this.bx_h, true, true);
			this.Bx.margin_in_lr = 30f;
			this.Bx.margin_in_tb = 30f;
			this.Bx.item_margin_x_px = 3f;
			this.Bx.item_margin_y_px = 8f;
			this.Bx.alignx = ALIGN.CENTER;
			this.Bx.init();
			float num = this.bx_h - 150f;
			this.TabLT = this.Bx.addTab("LT", this.btn_w, num, 0f, 0f, false);
			this.TabLT.Smallest();
			this.CR_LT = ColumnRow.CreateT<aBtnNel>(this.TabLT, "lt_tab", "row_tab", 0, new string[] { "&&Smnc_ui_enemy_edit_kind", "&&Smnc_ui_enemy_edit_attr" }, delegate(BtnContainerRadio<aBtn> BCon, int pre, int cur)
			{
				if (cur < 0 || this.state != UiSmncEnemyEditor.STATE.LIST)
				{
					return false;
				}
				this.cur_tab = (UiSmncEnemyEditor.TAB)cur;
				this.fineTab(false);
				return true;
			}, 0f, 22f, false, false);
			this.CR_LT.LR_valotile = true;
			this.TabLT.Br();
			this.BConLT = this.TabLT.addRadioT<aBtnNel>(new DsnDataRadio
			{
				w = this.btn_w - 40f,
				h = 20f,
				margin_h = 0,
				all_function_same = true,
				navi_loop = 1,
				skin = "row_smnc_enemy",
				name = "bcon_enemies",
				locked_click = false,
				click_snd = "tool_drag_init",
				fnMakingAfter = delegate(BtnContainer<aBtn> BCon, aBtn B)
				{
					if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND)
					{
						int carr_index = B.carr_index;
						(B.get_Skin() as ButtonSkinNelSmncEnemy).initEnemy(this, this.AdefeatCur[carr_index]);
					}
					else
					{
						FEnum<ENATTR>.Parse(B.title, ENATTR.NORMAL);
						(B.get_Skin() as ButtonSkinNelSmncEnemy).initNormal(" ");
					}
					return true;
				},
				APoolEvacuated = new List<aBtn>(this.AdefeatSrc.Count),
				fnGenerateKeys = new FnGenerateRemakeKeys(this.fnGenerateLT),
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangedEnemyEntry),
				fnHover = new FnBtnBindings(this.fnHoverEntry),
				fnOut = new FnBtnBindings(this.fnOutEntry),
				navi_auto_fill = false,
				SCA = new ScrollAppend(14, this.btn_w, num - 22f, 4f, 6f, 0)
			});
			this.Bx.endTab(false);
			this.Bx.addHr(new DsnDataHr
			{
				swidth = num,
				Col = NEL.ColText,
				vertical = true,
				draw_width_rate = 0.95f,
				margin_b = 6f,
				margin_t = 6f
			});
			this.TabRT = this.Bx.addTab("TabRT", this.Bx.use_w, num, this.Bx.use_w, num, false);
			this.TabRT.Smallest();
			this.FbThumb = FillImageEnThumbBlock.makeEnemyThumbnail(this.TabRT, this.TabRT.use_w, this.TabRT.use_h - 24f, (ENEMYID)0U);
			this.TabRT.Br();
			this.LRSlider = new TabLRSlider(this.TabRT, 0f, 0f, false)
			{
				alloc_add_rem_input = true,
				alloc_lr_arrow_input = true,
				alloc_ltab_rtab_input = false
			};
			this.LRSlider.setL();
			this.FbCount = this.TabRT.addP(new DsnDataP("", false)
			{
				name = "maxcount_indv",
				size = (float)(X.ENG_MODE ? 14 : 18),
				swidth = this.LRSlider.between_bounds_w,
				TxCol = NEL.ColText,
				text = " ",
				html = true,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE
			}, false);
			this.LRSlider.setR();
			this.LRSlider.FD_slided = new Func<int, bool>(this.fnLRSlide);
			this.LRSlider.use_valotile = true;
			this.Bx.endTab(true);
			this.Bx.Br();
			this.Bx.addHr(new DsnDataHr
			{
				Col = NEL.ColText,
				draw_width_rate = 0.86f,
				margin_b = 6f,
				margin_t = 6f
			});
			this.Bx.Br();
			this.BSliderMaxcount = this.Bx.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
			{
				name = "maxappear",
				title = "maxappear",
				fnChanged = new aBtnMeter.FnMeterBindings(this.fnChangedSlider),
				def = 0f,
				mn = 0f,
				mx = 99f,
				valintv = 1f,
				h = 22f,
				fnHover = new FnBtnBindings(this.fnHoverEntry),
				fnDescConvert = new FnDescConvert(this.fnDescConvertMaxAppear)
			}, this.Bx.use_w * 0.66f, null, false);
			this.Bx.Br();
			this.BCancel = this.Bx.addButtonT<aBtnNel>(new DsnDataButton
			{
				w = this.Bx.use_w * 0.8f,
				h = 24f,
				title = "&&Cancel",
				skin = "row_center",
				click_snd = "cancel",
				fnHover = new FnBtnBindings(this.fnHoverEntry),
				fnClick = delegate(aBtn B)
				{
					this.deactivate();
					return true;
				}
			});
		}

		private void fnGenerateLT(BtnContainerBasic BCon, List<string> A)
		{
			if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND)
			{
				int count = this.AdefeatCur.Count;
				for (int i = 0; i < count; i++)
				{
					A.Add(i.ToString());
				}
				return;
			}
			EnemyAttr.listupAttr(A);
		}

		public void activate(SmncFile _CurFile)
		{
			if (this.state == UiSmncEnemyEditor.STATE.OFFLINE)
			{
				this.state = UiSmncEnemyEditor.STATE.LIST;
				if (!this.DesPrevious.valid)
				{
					this.DesPrevious = new Designer.EvacuateContainer(this.Bx, true);
				}
				if (this.DesCurrent.valid)
				{
					this.DesCurrent.reassign(this.Bx);
					this.LRSlider.setVisible(true);
					this.DesCurrent = default(Designer.EvacuateContainer);
					this.Bx.WHanim(this.bx_w, this.bx_h, true, true);
				}
				this.Bx.position(0f, 0f, -1000f, -1000f, false);
				this.BConLT.setValue(-1, true);
				this.Bx.Focusable(false, false, null);
				this.Bx.Focus();
				SND.Ui.play("enter_small", false);
			}
			this.CR_LT.lr_input = true;
			this.SkAnim = null;
			int num = -1;
			this.Con.need_fine_kd = true;
			if (this.CurFile != _CurFile)
			{
				if (this.need_fine_dex_list)
				{
					this.need_fine_dex_list = false;
					this.AdefeatSrc.Clear();
					UiEnemyDex.getListupDefeated(this.AdefeatSrc, true, false);
				}
				if (this.cur_tab != UiSmncEnemyEditor.TAB.KIND)
				{
					this.cur_tab = UiSmncEnemyEditor.TAB.KIND;
					this.CR_LT.setValue((int)this.cur_tab, false);
				}
				this.CurFile = _CurFile;
				this.CurFile.need_fine_nattr_valid = true;
				this.AdefeatCur.Clear();
				bool flag = (this.CurFile.reward_flags & SmncFile.REWARD._UPDATE_ENEMIES_027) > (SmncFile.REWARD)0;
				int count = this.AdefeatSrc.Count;
				for (int i = 0; i < count; i++)
				{
					UiEnemyDex.DefeatData defeatData = this.AdefeatSrc[i];
					EnemyEntry enemyEntry = new EnemyEntry(this.LpArea.nM2D, defeatData, defeatData.id);
					int num2;
					if (!enemyEntry.available_enemy && (X.DEBUGALBUMUNLOCK || flag || this.CurFile.FindEmemy(enemyEntry.id, out num2).valid))
					{
						if (flag)
						{
							enemyEntry.unlock = UiSmncEnemyEditor.UNLK.UNLOCK_NEW;
						}
						else
						{
							enemyEntry.unlock = UiSmncEnemyEditor.UNLK.UNLOCK;
						}
					}
					if (enemyEntry.unlock == UiSmncEnemyEditor.UNLK.UNLOCK_NEW && num == -1)
					{
						num = i;
					}
					this.AdefeatCur.Add(enemyEntry);
				}
				if (flag)
				{
					this.CurFile.reward_flags |= SmncFile.REWARD._UPDATE_ENEMIES_027_END;
				}
				this.BConLT.RemakeT<aBtnNel>(null, "");
				if (num >= 0)
				{
					this.SkAnim = this.BConLT.Get(num).get_Skin() as ButtonSkinNelSmncEnemy;
				}
			}
			this.fineEntryRowAll();
			aBtn aBtn = null;
			this.BNotLocked = null;
			if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND)
			{
				int count2 = this.AdefeatCur.Count;
				for (int j = 0; j < count2; j++)
				{
					if (!(aBtn == null) && !(this.BNotLocked == null))
					{
						break;
					}
					aBtn aBtn2 = this.BConLT.Get(j);
					if (this.BNotLocked == null && !aBtn2.isLocked())
					{
						this.BNotLocked = aBtn2;
					}
					int num3;
					if (this.GetInfo(this.AdefeatCur[j].id, out num3, false).valid && aBtn == null)
					{
						aBtn = aBtn2;
					}
				}
			}
			else
			{
				aBtn = this.BConLT.Get(this.attr_index_hover);
			}
			this.Bx.rowRemakeCheck(false);
			if (aBtn == null)
			{
				if (this.BNotLocked != null)
				{
					aBtn = this.BNotLocked;
				}
				else
				{
					aBtn = this.BConLT.Get(0);
				}
			}
			if (this.SkAnim != null)
			{
				this.PreSelectRB = aBtn;
				if (aBtn.PreSelected != null)
				{
					aBtn.PreSelected.Deselect(true);
				}
				this.state = UiSmncEnemyEditor.STATE.LIST_UNLOCKING_ANIM_AUTO;
				this.initUnlockAnimate(this.SkAnim);
			}
			else if (aBtn != null)
			{
				aBtn.Select(true);
				this.BConLT.OuterScrollBox.getScrollBox().reveal(aBtn.PreSelected, false, REVEALTYPE.ALWAYS);
			}
			this.fineTab(true);
		}

		private bool UnlockEntry(ENEMYID id, int index = -1, bool write_dex = true, bool immediate = false)
		{
			if (index < 0)
			{
				int count = this.AdefeatCur.Count;
				for (int i = 0; i < count; i++)
				{
					if (this.AdefeatCur[i].id == id)
					{
						index = i;
						break;
					}
				}
			}
			if (index < 0)
			{
				return false;
			}
			EnemyEntry enemyEntry = this.AdefeatCur[index];
			this.AdefeatCur[index] = enemyEntry;
			if (this.StbEnemies == null)
			{
				this.StbEnemies = new STB();
			}
			this.StbEnemies.Append(NDAT.getEnemyName(id, true), " / ");
			if (write_dex)
			{
				enemyEntry.Unlock(this);
			}
			else if (!enemyEntry.DData.smnc_unlocked)
			{
				enemyEntry.DData.smnc_unlocked = true;
				enemyEntry.DData.smnc_unlocked_new = true;
			}
			return true;
		}

		public void deactivateTemp()
		{
			this.Bx.deactivate();
		}

		public void activateTemp()
		{
			this.Bx.activate();
		}

		public void deactivate()
		{
			if (this.state != UiSmncEnemyEditor.STATE.OFFLINE)
			{
				if (this.BxC != null)
				{
					this.BxC.deactivate();
				}
				this.SkAnim = null;
				this.state = UiSmncEnemyEditor.STATE.OFFLINE;
				if (!this.DesCurrent.valid)
				{
					this.LRSlider.setVisible(false);
					this.DesCurrent = new Designer.EvacuateContainer(this.Bx, true);
				}
				if (this.DesPrevious.valid)
				{
					this.DesPrevious.reassign(this.Bx);
					this.DesPrevious = default(Designer.EvacuateContainer);
					this.Bx.position(this.bx_pre_dx, this.bx_pre_dy, -1000f, -1000f, false);
					this.Bx.WHanim(this.bx_pre_w, this.bx_pre_h, true, true);
				}
				if (this.CurFile != null)
				{
					for (int i = this.CurFile.Aen_list.Count - 1; i >= 0; i--)
					{
						if (this.CurFile.Aen_list[i].count == 0)
						{
							this.CurFile.Aen_list.RemoveAt(i);
						}
					}
					using (BList<int> blist = ListBuffer<int>.Pop(0))
					{
						foreach (KeyValuePair<ENEMYID, int> keyValuePair in this.CurFile.Oid2appear)
						{
							int num;
							this.CurFile.FindEmemy(keyValuePair.Key, out num);
							if (num < 0)
							{
								blist.Add((int)keyValuePair.Key);
							}
						}
						for (int j = blist.Count - 1; j >= 0; j--)
						{
							this.CurFile.Oid2appear.Remove((ENEMYID)blist[j]);
						}
						blist.Clear();
						foreach (KeyValuePair<ENATTR, int> keyValuePair2 in this.CurFile.Oen_attr_count)
						{
							if (keyValuePair2.Value <= 0)
							{
								blist.Add((int)keyValuePair2.Key);
							}
						}
						for (int k = blist.Count - 1; k >= 0; k--)
						{
							this.CurFile.Oen_attr_count.Remove((ENATTR)blist[k]);
						}
					}
				}
			}
		}

		private void fineTab(bool only_navi = false)
		{
			if (!only_navi)
			{
				this.BConLT.RemakeT<aBtnNel>(null, "");
				this.FbThumb.gameObject.SetActive(this.cur_tab == UiSmncEnemyEditor.TAB.KIND);
				this.fineEntryRowAll();
				this.fineRightSlider();
				if (aBtn.PreSelected == null)
				{
					aBtn aBtn;
					if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND && this.en_index_hover >= 0)
					{
						aBtn = this.BConLT.Get(this.en_index_hover);
					}
					else if (this.cur_tab == UiSmncEnemyEditor.TAB.ATTR && this.attr_index_hover >= 0)
					{
						aBtn = this.BConLT.Get(this.attr_index_hover);
					}
					else
					{
						aBtn = this.BConLT.Get(0);
					}
					aBtn.Select(true);
					this.BConLT.OuterScrollBox.getScrollBox().reveal(aBtn, false, REVEALTYPE.ALWAYS);
				}
				else
				{
					if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND)
					{
						this.en_index_hover = -1;
					}
					if (this.cur_tab == UiSmncEnemyEditor.TAB.ATTR)
					{
						this.attr_index_hover = -1;
					}
				}
			}
			this.BSliderMaxcount.setNaviT(this.BConLT.Get(this.BConLT.Length - 1), true, true);
			this.BCancel.setNaviB(this.BConLT.Get(0), true, true);
		}

		private SmncFile.EnemyInfo GetInfo(ENEMYID id, out int en_index, bool _add_if_not_exists = false)
		{
			int count = this.CurFile.Aen_list.Count;
			en_index = -1;
			for (int i = 0; i < count; i++)
			{
				SmncFile.EnemyInfo enemyInfo = this.CurFile.Aen_list[i];
				if (enemyInfo.id == id)
				{
					en_index = i;
					return enemyInfo;
				}
			}
			if (_add_if_not_exists)
			{
				SmncFile.EnemyInfo enemyInfo2 = new SmncFile.EnemyInfo
				{
					id = id,
					mp_min100 = 30,
					mp_max100 = 40,
					count = 0
				};
				this.CurFile.need_fine_nattr_valid = true;
				this.CurFile.Aen_list.Add(enemyInfo2);
				this.CurFile.Aen_list.Sort((SmncFile.EnemyInfo A, SmncFile.EnemyInfo B) => UiSmncEnemyEditor.sortInfo(A, B));
				return this.GetInfo(id, out en_index, false);
			}
			return default(SmncFile.EnemyInfo);
		}

		private static int sortInfo(SmncFile.EnemyInfo A, SmncFile.EnemyInfo B)
		{
			if (A.id == B.id)
			{
				return 0;
			}
			if (A.id >= B.id)
			{
				return 1;
			}
			return -1;
		}

		private void fineRightSlider()
		{
			int num2;
			if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND)
			{
				if (this.en_index_hover < 0)
				{
					this.LRSlider.lr_input = 0;
					return;
				}
				EnemyEntry enemyEntry = this.AdefeatCur[this.en_index_hover];
				int num;
				SmncFile.EnemyInfo info = this.GetInfo(enemyEntry.id, out num, false);
				if (!enemyEntry.available_enemy)
				{
					this.LRSlider.lr_input = 0;
					num2 = -1;
					if (enemyEntry.hidden)
					{
						this.FbCount.Txt(" ");
					}
					else
					{
						this.FbCount.Txt(TX.Get("KD_check_unlock_how", ""));
					}
				}
				else
				{
					this.LRSlider.lr_input = (byte)(((info.count > 0) ? 1 : 0) | ((info.count < 99) ? 2 : 0));
					num2 = info.count;
				}
			}
			else
			{
				if (this.attr_index_hover < 0)
				{
					this.LRSlider.lr_input = 0;
					return;
				}
				ENATTR enattr = FEnum<ENATTR>.Parse(this.BConLT.Get(this.attr_index_hover).title, ENATTR.NORMAL);
				num2 = X.Get<ENATTR, int>(this.CurFile.Oen_attr_count, enattr);
				this.LRSlider.lr_input = (byte)(((num2 > 0) ? 1 : 0) | ((num2 < 99) ? 2 : 0));
			}
			if (num2 >= 0)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AddTxA("Smnc_ui_enemy_total", false);
					stb.AddTxA("Smnc_ui_enemy_count", false).TxRpl(num2);
					this.FbCount.Txt(stb);
				}
			}
		}

		public bool fnLRSlide(int move)
		{
			this.CurFile.need_fine_nattr_valid = true;
			if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND)
			{
				if (this.en_index_hover < 0)
				{
					return false;
				}
				EnemyEntry enemyEntry = this.AdefeatCur[this.en_index_hover];
				if (!enemyEntry.available_enemy)
				{
					return false;
				}
				int num;
				SmncFile.EnemyInfo info = this.GetInfo(enemyEntry.id, out num, true);
				if (info.count > 0 && move < 0)
				{
					info.count--;
				}
				else
				{
					if (info.count >= 99 || move <= 0)
					{
						return false;
					}
					info.count++;
				}
				this.CurFile.Aen_list[num] = info;
				this.fineRightSlider();
				this.fineEntryRow(this.en_index_hover);
			}
			else
			{
				if (this.attr_index_hover < 0)
				{
					return false;
				}
				ENATTR enattr = FEnum<ENATTR>.Parse(this.BConLT.Get(this.attr_index_hover).title, ENATTR.NORMAL);
				int num2 = X.Get<ENATTR, int>(this.CurFile.Oen_attr_count, enattr);
				if (num2 > 0 && move < 0)
				{
					num2--;
				}
				else
				{
					if (num2 >= 99 || move <= 0)
					{
						return false;
					}
					num2++;
				}
				this.CurFile.Oen_attr_count[enattr] = num2;
				this.fineRightSlider();
				this.fineEntryRow(this.attr_index_hover);
			}
			return true;
		}

		public bool fnHoverEntry(aBtn B)
		{
			if (this.state != UiSmncEnemyEditor.STATE.LIST)
			{
				return false;
			}
			bool flag = false;
			ENATTR enattr;
			if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND)
			{
				if (X.NmI(B.title, -1, false, false) >= 0)
				{
					this.en_index_hover = B.carr_index;
					EnemyEntry enemyEntry = this.AdefeatCur[this.en_index_hover];
					if (enemyEntry.available_enemy && enemyEntry.DData.smnc_unlocked_new)
					{
						enemyEntry.Sk.hoverHideNewIcon();
					}
					this.FbThumb.InitEnemy(this.LpArea.nM2D, B.isLocked() ? ((ENEMYID)0U) : enemyEntry.id);
				}
				else
				{
					flag = true;
				}
			}
			else if (FEnum<ENATTR>.TryParse(B.title, out enattr, true))
			{
				this.attr_index_hover = B.carr_index;
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				if (this.en_index_hover >= 0)
				{
					this.en_index_hover = -1 - this.en_index_hover;
				}
				if (this.attr_index_hover >= 0)
				{
					this.attr_index_hover = -1 - this.attr_index_hover;
				}
			}
			this.fineRightSlider();
			return true;
		}

		public bool fnOutEntry(aBtn B)
		{
			ButtonSkinNelSmncEnemy buttonSkinNelSmncEnemy = B.get_Skin() as ButtonSkinNelSmncEnemy;
			if (buttonSkinNelSmncEnemy != null)
			{
				buttonSkinNelSmncEnemy.blurHideNewIcon();
			}
			return true;
		}

		private void fineEntryRowAll()
		{
			int length = this.BConLT.Length;
			using (STB stb = TX.PopBld(null, 0))
			{
				for (int i = 0; i < length; i++)
				{
					this.fineEntryRow(i, stb);
				}
			}
			this.BSliderMaxcount.setValue((float)this.CurFile.maxappear, false);
		}

		private void fineEntryRow(int index)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				this.fineEntryRow(index, stb);
			}
		}

		private void fineEntryRow(int i, STB Stb)
		{
			Stb.Clear();
			aBtn aBtn = this.BConLT.Get(i);
			if (this.cur_tab == UiSmncEnemyEditor.TAB.KIND)
			{
				EnemyEntry enemyEntry = this.AdefeatCur[i];
				if (enemyEntry.hidden)
				{
					return;
				}
				int num;
				SmncFile.EnemyInfo info = this.GetInfo(enemyEntry.id, out num, false);
				if (info.count != 0)
				{
					Stb.AddTxA("Smnc_ui_enemy_row", false).TxRpl(enemyEntry.localized_name).TxRpl(info.mp_min100)
						.TxRpl(info.mp_max100)
						.TxRpl(info.count);
				}
				else
				{
					Stb.Add("<font alpha=0.5>", enemyEntry.localized_name, "</font>");
				}
			}
			else
			{
				ENATTR enattr = FEnum<ENATTR>.Parse(this.BConLT.Get(i).title, ENATTR.NORMAL);
				int num2 = X.Get<ENATTR, int>(this.CurFile.Oen_attr_count, enattr);
				if (num2 == 0)
				{
					Stb.Add("<font alpha=0.5>");
				}
				Stb.Add(EnemyAttr.getLocalizedName(enattr)).Add(": ");
				Stb.AddTxA("Smnc_ui_enemy_count", false).TxRpl(X.Get<ENATTR, int>(this.CurFile.Oen_attr_count, enattr));
				if (num2 == 0)
				{
					Stb.Add("</font>");
				}
			}
			(aBtn.get_Skin() as ButtonSkinRow).setTitleTextS(Stb);
		}

		public void fnDescConvertMaxAppear(STB Stb)
		{
			int num = Stb.NmI(0, -1, 0);
			Stb.Clear();
			Stb.AddTxA("Smnc_ui_enemy_appear_whole", false);
			if (num == 0)
			{
				Stb.TxRpl(TX.Get("Smnc_ui_enemy_appear_infinity", ""));
				return;
			}
			Stb.TxRpl(num);
		}

		public void createConfirmBox()
		{
			float num = IN.w * 0.6f;
			float num2 = IN.h * 0.54f;
			if (this.BxD == null)
			{
				this.BxD = this.DsFam.Create("Confirm", 0f, 0f, num, num2, 1, 50f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.BxD.margin_in_lr = 60f;
				this.BxD.margin_in_tb = 50f;
				this.BxD.item_margin_y_px = 20f;
				this.BxD.alignx = ALIGN.CENTER;
				this.BxD.init();
				this.BxD.addP(new DsnDataP("", false)
				{
					text = TX.Get("Smnc_ui_enemy_kind_unlock_count_auto", ""),
					size = 18f,
					TxCol = NEL.ColText,
					swidth = this.BxD.use_w * 0.65f,
					sheight = this.BxD.use_h * 0.4f,
					alignx = ALIGN.LEFT
				}, false);
				this.BxD.Br();
				this.BxD.addP(new DsnDataP("", false)
				{
					text = " ",
					name = "enemies",
					TxCol = C32.MulA(4283780170U, 0.5f),
					size = 13f,
					text_auto_wrap = true,
					swidth = this.BxD.use_w * 0.7f,
					alignx = ALIGN.LEFT
				}, false);
				this.BxD.Br();
				this.BxD.addButtonT<aBtnNel>(new DsnDataButton
				{
					name = "submit",
					title = "submit",
					skin_title = "&&Submit",
					w = 220f,
					h = 30f,
					fnClick = delegate(aBtn B)
					{
						IN.clearPushDown(true);
						this.BxD.deactivate();
						this.quitContentAndChangeToListState(true);
						this.StbEnemies.Clear();
						return true;
					}
				});
			}
			FillBlock fillBlock = this.BxD.Get("enemies", false) as FillBlock;
			this.BxD.activate();
			fillBlock.Txt(this.StbEnemies);
			this.BxD.RowRemakeHeightRecalc(fillBlock, null);
			this.BxD.cropBounds(num, num2);
			aBtn btn = this.BxD.getBtn("submit");
			btn.Select(true);
			btn.SetChecked(false, true);
			this.Bx.Focusable(true, true, null);
			this.Bx.hide();
			this.BxD.Focus();
			this.FbThumb.use_valotile = false;
		}

		public void runConfirm()
		{
			if (IN.isCancelPD() || this.Bx.isFocused())
			{
				this.BxD.getBtn("submit").ExecuteOnSubmitKey();
			}
		}

		public bool activateConfirm()
		{
			if (this.StbEnemies == null || this.StbEnemies.Length == 0)
			{
				return false;
			}
			this.state = UiSmncEnemyEditor.STATE.CONFIRM;
			this.createConfirmBox();
			this.CR_LT.lr_input = false;
			this.LRSlider.lr_input = 0;
			this.Bx.hide();
			return true;
		}

		public bool run(float fcnt)
		{
			if (this.state == UiSmncEnemyEditor.STATE.OFFLINE)
			{
				return false;
			}
			if ((this.state & UiSmncEnemyEditor.STATE._FIELD_GUIDE) != UiSmncEnemyEditor.STATE.OFFLINE)
			{
				if (!this.RecipeBook.isActive())
				{
					this.RecipeBook = null;
					IN.clearSubmitPushDown(true);
					this.state &= (UiSmncEnemyEditor.STATE)127;
					if (this.state == UiSmncEnemyEditor.STATE.CONTENT_UNLOCK)
					{
						this.Unlocker.activateTemp();
						if (!this.Unlocker.isActiveItemMove())
						{
							this.activateTemp();
						}
					}
					else
					{
						this.activateTemp();
						if (this.state == UiSmncEnemyEditor.STATE.CONTENT)
						{
							this.BxC.activate();
							this.BxC.Focus();
						}
						else
						{
							this.Bx.Focus();
						}
					}
					this.PreSelectRB.Select(true);
				}
				return true;
			}
			if (this.M2D.isRbkPD() && this.fineFieldGuideReveal())
			{
				if (this.RecipeBook != null)
				{
					IN.DestroyOne(this.RecipeBook.gameObject);
				}
				this.PreSelectRB = aBtn.PreSelected;
				this.RecipeBook = new GameObject("RBK" + IN.totalframe.ToString()).AddComponent<UiFieldGuide>();
				SND.Ui.play("tool_hand_init", false);
				this.deactivateTemp();
				this.Unlocker.deactivateTemp();
				this.state |= UiSmncEnemyEditor.STATE._FIELD_GUIDE;
				IN.setZAbs(this.RecipeBook.transform, this.Bx.transform.position.z - 0.125f);
			}
			else
			{
				if (this.state == UiSmncEnemyEditor.STATE.LIST)
				{
					if (IN.isCancel())
					{
						this.deactivate();
						SND.Ui.play("cancel", false);
						return false;
					}
					if (((this.cur_tab == UiSmncEnemyEditor.TAB.KIND) ? this.en_index_hover : this.attr_index_hover) >= 0)
					{
						this.LRSlider.runLRInput(-2, false);
					}
				}
				if (this.state == UiSmncEnemyEditor.STATE.CONFIRM)
				{
					this.runConfirm();
					return true;
				}
				if (this.isUnlockAnimState())
				{
					this.runUnlockAnimate(fcnt);
					return true;
				}
				if (this.state == UiSmncEnemyEditor.STATE.CONTENT && (IN.isCancel() || this.Bx.isFocused()))
				{
					SND.Ui.play("cancel", false);
					this.quitContentEdit();
				}
				if (this.state == UiSmncEnemyEditor.STATE.CONTENT_UNLOCK)
				{
					this.Unlocker.runMain(fcnt, false);
				}
			}
			return true;
		}

		public bool fineFieldGuideReveal()
		{
			if (this.state == UiSmncEnemyEditor.STATE.CONTENT || this.state == UiSmncEnemyEditor.STATE.OFFLINE || this.state == UiSmncEnemyEditor.STATE.CONFIRM || this.isUnlockAnimState() || this.Unlocker.isTransition())
			{
				return false;
			}
			if (this.state == UiSmncEnemyEditor.STATE.LIST || !this.Unlocker.fineFieldGuideReveal())
			{
				if (this.cur_tab != UiSmncEnemyEditor.TAB.KIND)
				{
					return false;
				}
				if (!X.BTW(0f, (float)this.en_index_hover, (float)this.AdefeatCur.Count))
				{
					return false;
				}
				EnemyEntry enemyEntry = this.AdefeatCur[this.en_index_hover];
				if (enemyEntry.hidden)
				{
					return false;
				}
				UiFieldGuide.NextRevealAtAwake = enemyEntry.id;
			}
			return true;
		}

		private bool fnChangedEnemyEntry(BtnContainerRadio<aBtn> BCon, int pre, int cur)
		{
			if (cur < 0)
			{
				return true;
			}
			if (this.state != UiSmncEnemyEditor.STATE.LIST || this.cur_tab != UiSmncEnemyEditor.TAB.KIND)
			{
				return false;
			}
			EnemyEntry enemyEntry = this.AdefeatCur[cur];
			if (enemyEntry.hidden)
			{
				SND.Ui.play("locked", false);
				CURS.limitVib(BCon.Get(cur), AIM.R);
				return false;
			}
			if (this.en_index_hover != cur)
			{
				if (!this.fnHoverEntry(BCon.Get(cur)))
				{
					return false;
				}
				this.en_index_hover = cur;
			}
			this.Bx.Focusable(true, true, null);
			this.FbThumb.use_valotile = false;
			this.CR_LT.lr_input = false;
			this.LRSlider.lr_input = 0;
			if (enemyEntry.locked)
			{
				this.state = UiSmncEnemyEditor.STATE.CONTENT_UNLOCK;
				this.Unlocker.activateMain(enemyEntry);
			}
			else
			{
				this.state = UiSmncEnemyEditor.STATE.CONTENT;
				int num;
				SmncFile.EnemyInfo info = this.GetInfo(enemyEntry.id, out num, true);
				if (this.BxC == null)
				{
					this.createContentUi();
				}
				Vector3 vector = BCon.Get(cur).transform.position * 64f;
				this.BxC.activate();
				this.BxC.posSetDA(vector.x + this.BxC.w * 0.5f + 110f, vector.y, 0, 40f, false);
				(this.BxC.Get("total", false) as aBtnMeterNel).setValue((float)info.count, false);
				(this.BxC.Get("mp_min", false) as aBtnMeterNel).setValue((float)info.mp_min100, false);
				(this.BxC.Get("mp_max", false) as aBtnMeterNel).setValue((float)info.mp_max100, false);
				(this.BxC.Get("appear", false) as aBtnMeterNel).setValue((float)X.Get<ENEMYID, int>(this.CurFile.Oid2appear, enemyEntry.id), false);
				this.Bx.hide();
				this.BxC.Focus();
				(this.BxC.Get("total", false) as aBtnMeterNel).Select(true);
				using (STB stb = TX.PopBld(null, 0))
				{
					this.Unlocker.CopyTerm(stb, enemyEntry);
					this.PTerm.Txt(stb);
				}
				this.BxC.RowRemakeHeightRecalc(this.PTerm, null);
				this.BxC.cropBounds(380f, 250f);
			}
			return true;
		}

		private void quitContentEdit()
		{
			if (this.state != UiSmncEnemyEditor.STATE.CONTENT)
			{
				return;
			}
			this.BxC.deactivate();
			this.quitContentAndChangeToListState(true);
		}

		private aBtn quitContentAndChangeToListState(bool reselect = true)
		{
			if (!this.Bx.isActive())
			{
				this.Bx.activate();
			}
			this.Bx.Focusable(false, false, null);
			this.Bx.Focus();
			this.Bx.bind();
			this.state = UiSmncEnemyEditor.STATE.LIST;
			this.fineEntryRow(this.en_index_hover);
			this.BConLT.setValue(-1, true);
			aBtn aBtn = this.BConLT.Get(this.en_index_hover);
			if (reselect)
			{
				aBtn.Select(true);
				this.CR_LT.lr_input = true;
				this.fineRightSlider();
			}
			else
			{
				this.CR_LT.lr_input = false;
				this.LRSlider.lr_input = 0;
			}
			this.FbThumb.use_valotile = true;
			return aBtn;
		}

		private void createContentUi()
		{
			this.BxC = this.DsFam.Create("EnemyContent", 0f, 0f, 380f, 250f, 0, 50f, UiBoxDesignerFamily.MASKTYPE.BOX);
			IN.setZ(this.BxC.transform, this.Bx.transform.localPosition.z - 0.25f);
			this.BxC.box_stencil_ref_mask = 20;
			this.BxC.Focusable(true, false, null);
			this.BxC.margin_in_lr = 50f;
			this.BxC.margin_in_tb = 30f;
			this.BxC.item_margin_y_px = 4f;
			this.BxC.alignx = ALIGN.CENTER;
			this.BxC.selectable_loop = 2;
			this.BxC.init();
			float num = this.BxC.use_w - 20f;
			this.BxC.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
			{
				name = "total",
				title = "total",
				fnDescConvert = delegate(STB Stb)
				{
					this.fnDescConvertContent("total", Stb);
				},
				fnChanged = new aBtnMeter.FnMeterBindings(this.fnChangedSlider),
				def = 0f,
				mn = 0f,
				mx = 99f,
				valintv = 1f
			}, num, null, false);
			this.BxC.Br();
			this.BxC.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
			{
				name = "mp_min",
				title = "mp_min",
				fnDescConvert = delegate(STB Stb)
				{
					this.fnDescConvertContent("mp_min", Stb);
				},
				fnChanged = new aBtnMeter.FnMeterBindings(this.fnChangedSlider),
				def = 0f,
				mn = 0f,
				mx = 100f,
				valintv = 5f
			}, num, null, false);
			this.BxC.Br();
			this.BxC.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
			{
				name = "mp_max",
				title = "mp_max",
				fnDescConvert = delegate(STB Stb)
				{
					this.fnDescConvertContent("mp_max", Stb);
				},
				fnChanged = new aBtnMeter.FnMeterBindings(this.fnChangedSlider),
				def = 0f,
				mn = 0f,
				mx = 100f,
				valintv = 5f
			}, num, null, false);
			this.BxC.Br();
			this.BxC.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
			{
				name = "appear",
				title = "appear",
				fnDescConvert = delegate(STB Stb)
				{
					this.fnDescConvertContent("appear", Stb);
				},
				fnChanged = new aBtnMeter.FnMeterBindings(this.fnChangedSlider),
				def = 0f,
				mn = 0f,
				mx = 99f,
				valintv = 1f
			}, num, null, false);
			this.BxC.Br();
			this.PTerm = this.BxC.addP(new DsnDataP("", false)
			{
				name = "term",
				text = " ",
				TxCol = C32.MulA(4283780170U, 0.5f),
				size = 13f,
				text_margin_x = 28f,
				swidth = this.BxC.use_w,
				sheight = 50f
			}, false);
		}

		public void fnDescConvertContent(string title, STB Stb)
		{
			int num = Stb.NmI(0, -1, 0);
			Stb.Clear();
			if (title != null)
			{
				if (title == "total")
				{
					Stb.AddTxA("Smnc_ui_enemy_total", false);
					Stb.AddTxA("Smnc_ui_enemy_count", false).TxRpl(num);
					return;
				}
				if (title == "mp_min")
				{
					Stb.AddTxA("Smnc_ui_enemy_mp_min_ratio", false).TxRpl(num);
					return;
				}
				if (title == "mp_max")
				{
					Stb.AddTxA("Smnc_ui_enemy_mp_max_ratio", false).TxRpl(num);
					return;
				}
			}
			Stb.AddTxA("Smnc_ui_enemy_appear", false);
			if (num == 0)
			{
				Stb.TxRpl(TX.Get("Smnc_ui_enemy_appear_infinity", ""));
				return;
			}
			Stb.TxRpl(num);
		}

		private bool fnChangedSlider(aBtnMeter _B, float pre_value, float cur_value_f)
		{
			int num = X.IntR(cur_value_f);
			if (_B.title == "maxappear")
			{
				if (this.state != UiSmncEnemyEditor.STATE.LIST)
				{
					return false;
				}
				this.CurFile.maxappear = num;
			}
			else
			{
				if (this.state != UiSmncEnemyEditor.STATE.CONTENT || this.en_index_hover < 0)
				{
					return false;
				}
				int num2;
				SmncFile.EnemyInfo info = this.GetInfo(this.AdefeatCur[this.en_index_hover].id, out num2, true);
				string title = _B.title;
				if (title != null)
				{
					if (!(title == "total"))
					{
						if (!(title == "mp_min"))
						{
							if (!(title == "mp_max"))
							{
								if (title == "appear")
								{
									this.CurFile.Oid2appear[info.id] = num;
									return true;
								}
							}
							else
							{
								info.mp_max100 = num;
							}
						}
						else
						{
							info.mp_min100 = num;
						}
					}
					else
					{
						this.CurFile.need_fine_nattr_valid = true;
						info.count = num;
					}
				}
				this.CurFile.Aen_list[num2] = info;
			}
			return true;
		}

		internal void quitUnlocker(EnemyEntry Unlock_Entry = null)
		{
			if (this.state == UiSmncEnemyEditor.STATE.CONTENT_UNLOCK)
			{
				bool flag = Unlock_Entry != null;
				aBtn aBtn = this.quitContentAndChangeToListState(!flag);
				if (flag)
				{
					this.PreSelectRB = aBtn;
					this.state = UiSmncEnemyEditor.STATE.LIST_UNLOCKING_ANIM;
					this.initUnlockAnimate(Unlock_Entry.Sk);
				}
			}
		}

		private void initUnlockAnimate(ButtonSkinNelSmncEnemy SkAnim)
		{
			this.SkAnim = SkAnim;
			SkAnim.UnlockAnimate();
			this.BConLT.OuterScrollBox.getScrollBox().reveal(SkAnim.getBtn(), false, REVEALTYPE.ALWAYS);
		}

		private void runUnlockAnimate(float fcnt)
		{
			if (this.SkAnim == null)
			{
				int count = this.AdefeatCur.Count;
				for (int i = 0; i < count; i++)
				{
					EnemyEntry enemyEntry = this.AdefeatCur[i];
					if (enemyEntry.unlock == UiSmncEnemyEditor.UNLK.UNLOCK_NEW)
					{
						this.SkAnim = enemyEntry.Sk;
						break;
					}
				}
				if (this.SkAnim == null)
				{
					IN.clearPushDown(true);
					if (this.state == UiSmncEnemyEditor.STATE.LIST_UNLOCKING_ANIM_AUTO && this.activateConfirm())
					{
						return;
					}
					this.state = UiSmncEnemyEditor.STATE.LIST;
					SND.Ui.play("cursor", false);
					if (this.PreSelectRB != null)
					{
						this.PreSelectRB.Select(true);
						this.PreSelectRB = null;
					}
					else if (this.BNotLocked != null)
					{
						this.BNotLocked.Select(true);
					}
					else
					{
						this.BConLT.Get(0).Select(true);
					}
					this.CR_LT.lr_input = true;
					return;
				}
				else
				{
					this.initUnlockAnimate(this.SkAnim);
				}
			}
			int carr_index = this.SkAnim.getBtn().carr_index;
			EnemyEntry enemyEntry2 = this.AdefeatCur[carr_index];
			bool flag;
			if (!this.SkAnim.runUnlockAnimate(fcnt * IN.skippingTS(), out flag))
			{
				this.SkAnim = null;
				return;
			}
			if (flag)
			{
				if (!enemyEntry2.hidden)
				{
					enemyEntry2.unlock = UiSmncEnemyEditor.UNLK.UNLOCK;
				}
				this.fineEntryRow(carr_index);
				this.UnlockEntry(enemyEntry2.id, carr_index, true, false);
			}
		}

		internal EnemyEntry GetEntry(ENEMYID id)
		{
			int count = this.AdefeatCur.Count;
			for (int i = 0; i < count; i++)
			{
				EnemyEntry enemyEntry = this.AdefeatCur[i];
				if (enemyEntry.id == id)
				{
					return enemyEntry;
				}
			}
			return null;
		}

		internal bool fineKD(STB Stb)
		{
			UiSmncEnemyEditor.STATE state = this.state;
			if (state - UiSmncEnemyEditor.STATE.LIST_UNLOCKING_ANIM > 1)
			{
				return state - UiSmncEnemyEditor.STATE.CONFIRM <= 2;
			}
			if (this.M2D.IMNG.has_recipe_collection)
			{
				Stb.AddTxA("Guild_Btn_recipebook", false);
			}
			Stb.Append("<key cancel/><key menu/>", " ").AddTxA("Select_skip", false);
			return true;
		}

		public override string ToString()
		{
			return "UiSmncEnemyEditor";
		}

		public bool isUnlockAnimState()
		{
			return this.state == UiSmncEnemyEditor.STATE.LIST_UNLOCKING_ANIM || this.state == UiSmncEnemyEditor.STATE.LIST_UNLOCKING_ANIM_AUTO;
		}

		public bool isContentEdit()
		{
			return this.state == UiSmncEnemyEditor.STATE.CONTENT;
		}

		public bool isFrontState()
		{
			return this.state == UiSmncEnemyEditor.STATE.LIST;
		}

		public Map2d Mp
		{
			get
			{
				return this.LpArea.Mp;
			}
		}

		public readonly UiBoxDesigner Bx;

		public readonly NelM2DBase M2D;

		public readonly UiSmnCreator Con;

		private UiBoxDesigner BxC;

		private UiBoxDesigner BxD;

		public readonly UiBoxDesignerFamily DsFam;

		private readonly M2LpUiSmnCreator LpArea;

		private readonly UiSmncEnemyUnlocker Unlocker;

		private Designer.EvacuateContainer DesPrevious;

		private Designer.EvacuateContainer DesCurrent;

		private float bx_pre_w;

		private float bx_pre_h;

		private float bx_pre_dx;

		private float bx_pre_dy;

		private float btn_w = IN.w * 0.3f;

		private UiFieldGuide RecipeBook;

		private aBtn PreSelectRB;

		private aBtn BNotLocked;

		private FillBlock PTerm;

		public bool need_fine_dex_list;

		private UiSmncEnemyEditor.STATE state_;

		private UiSmncEnemyEditor.TAB cur_tab;

		private Designer TabLT;

		private BtnContainerRadio<aBtn> BConLT;

		private ColumnRow CR_LT;

		private FillImageEnThumbBlock FbThumb;

		private FillBlock FbCount;

		private TabLRSlider LRSlider;

		private aBtnMeterNel BSliderMaxcount;

		private aBtnNel BCancel;

		private Designer TabRT;

		private SmncFile CurFile;

		private int en_index_hover;

		private int attr_index_hover;

		private ButtonSkinNelSmncEnemy SkAnim;

		private readonly List<UiEnemyDex.DefeatData> AdefeatSrc;

		private List<EnemyEntry> AdefeatCur;

		private STB StbEnemies;

		private const float confirm_w = 380f;

		private const float confirm_h = 250f;

		public enum UNLK
		{
			INVALID,
			HIDDEN,
			LOCKED,
			LOCKED_UNLOCKABLE,
			UNLOCK,
			UNLOCK_NEW
		}

		private enum STATE : byte
		{
			OFFLINE,
			LIST,
			LIST_UNLOCKING_ANIM,
			LIST_UNLOCKING_ANIM_AUTO,
			CONFIRM,
			CONTENT,
			CONTENT_UNLOCK,
			_FIELD_GUIDE = 128
		}

		private enum TAB
		{
			KIND,
			ATTR
		}
	}
}
