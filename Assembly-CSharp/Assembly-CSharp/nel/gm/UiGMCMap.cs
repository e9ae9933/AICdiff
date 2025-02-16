using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel.gm
{
	internal class UiGMCMap : UiGMC
	{
		internal UiGMCMap(UiGameMenu _GM, UiGmMapMarker _Marker)
			: base(_GM, CATEG.MAP, false, 0, 0, 0, 0, 1f, 1f)
		{
			this.Marker = _Marker;
			this.WmCtr = new UiWmSkinController(this.GM.M2D);
			this.WmCtr.Marker = this.Marker;
			this.WmCtr.use_mapname_box = true;
		}

		public override bool initAppearMain()
		{
			this.BxR.alignx = ALIGN.CENTER;
			base.M2D.QUEST.fineAutoItemCollection(false);
			if (base.initAppearMain())
			{
				this.WmCtr.initAppearAgain();
				return true;
			}
			this.wmsmn_setup = false;
			WholeMapItem curWM = base.M2D.WM.CurWM;
			this.see_nightingale = base.M2D.WDR.getNightingale().isEnable() && base.M2D.IMNG.getInventory().getCount(NelItem.GetById("nightingale_bell", false), -1) > 0;
			this.BxR.box_stencil_ref_mask = -1;
			curWM.remake(false);
			this.BxR.margin_in_lr = 30f;
			this.BxR.item_margin_x_px = 2f;
			this.BxR.item_margin_y_px = 0f;
			this.FD_Target = null;
			this.FD_ReturnBack = default(WMIconDescription);
			this.BxR.init();
			float use_w = this.BxR.use_w;
			curWM.fixPlayerPosOnWM(this.Pr, ref this.first_map_center_x, ref this.first_map_center_y);
			aBtnNelMapArea aBtnNelMapArea = this.BxR.addButtonT<aBtnNelMapArea>(new DsnDataButton
			{
				name = "map_area",
				title = "map_area",
				skin = "whole_map_area",
				w = use_w,
				h = 510f,
				fnDown = new FnBtnBindings(this.fnWholeMapAreaDragInit)
			});
			this.BxR.Br();
			this.TutoP = this.BxR.addP(new DsnDataP("", false)
			{
				text = "\u3000",
				alignx = ALIGN.CENTER,
				swidth = use_w,
				html = true,
				name = "tuto",
				size = 15f,
				TxCol = C32.d2c(4283780170U)
			}, false);
			this.WmSkin = aBtnNelMapArea.get_Skin() as ButtonSkinWholeMapArea;
			this.WmSkin.fast_travel_active = (this.WmCtr.can_use_fasttravel = this.can_use_fasttravel);
			ButtonSkinWholeMapArea wmSkin = this.WmSkin;
			wmSkin.FD_FnDrawIcon = (ButtonSkinWholeMapArea.FnDrawIcon)Delegate.Combine(wmSkin.FD_FnDrawIcon, new ButtonSkinWholeMapArea.FnDrawIcon(this.FnDrawNightingale));
			this.WmSkin.setWholeMapTarget(curWM, this.first_map_center_x, this.first_map_center_y);
			this.WmCtr.initAppear(this.WmSkin, this.first_map_center_x, this.first_map_center_y);
			this.WmSkin.FnQuitDragging = new Action<bool, bool, bool>(this.fnQuitWholeMapDrag);
			return true;
		}

		internal override void initEdit()
		{
			this.gres = GMC_RES.CONTINUE;
			this.FD_Target = null;
			this.FD_ReturnBack = default(WMIconDescription);
			this.need_tuto_msg = 1;
			if (this.WmCtr != null)
			{
				this.WmCtr.initEdit();
			}
		}

		internal override void quitEdit()
		{
			this.gres = GMC_RES.BACK_CATEGORY;
			this.FD_Target = null;
			this.FD_ReturnBack = default(WMIconDescription);
			this.QuestCancelingMapJump.WM = null;
			if (this.WmCtr != null)
			{
				this.WmCtr.quitEdit(true);
			}
			if (this.TutoP != null)
			{
				this.TutoP.text_content = "";
			}
		}

		public override void quitAppear()
		{
			base.quitAppear();
			this.QuestCon = null;
			this.wmsmn_setup = false;
			if (this.WmCtr != null)
			{
				this.WmCtr.quitAppear();
			}
		}

		internal override void releaseEvac()
		{
			if (this.WmCtr != null)
			{
				this.WmCtr.destruct();
			}
			base.releaseEvac();
		}

		public override void containerResized()
		{
			if (this.WmSkin != null && this.gres == GMC_RES.BACK_CATEGORY)
			{
				aBtn btn = this.WmSkin.getBtn();
				btn.WH(this.BxR.w - this.BxR.margin_in_lr * 2f, this.WmSkin.sheight);
				this.BxR.RowRemakeHeightRecalc(btn, null);
			}
		}

		public bool can_use_fasttravel
		{
			get
			{
				return this.GM.can_use_fasttravel;
			}
		}

		public NelChipBench BenchChip
		{
			get
			{
				return this.GM.BenchChip;
			}
		}

		private void fineKD(IVariableObject P)
		{
			this.FD_Target = null;
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.WmSkin.is_detail)
				{
					Vector2 cursorMapPos = this.WmSkin.getCursorMapPos();
					WholeMapItem.WMItem wmiCurPos = this.WmSkin.WmiCurPos;
					WholeMapItem wholeMapTarget = this.WmSkin.getWholeMapTarget();
					if (this.WmSkin.fast_travel_active && this.BenchChip != null)
					{
						stb.AddTxA(this.WmSkin.FastTravelFocused.valid ? (this.BenchChip.IconIs(this.WmSkin.FastTravelFocused.get_Icon()) ? "GM_desc_fasttravel_here" : "GM_desc_map_2") : "GM_desc_map_2_no_marker", false);
					}
					else
					{
						this.WmCtr.fineMapKD(stb);
						if (this.Marker.marker_enabled)
						{
							stb.Add("  ");
							int markerAt = wholeMapTarget.getMarkerAt((int)cursorMapPos.x, (int)cursorMapPos.y, wmiCurPos);
							if (!this.can_use_fasttravel || wholeMapTarget != base.M2D.WM.CurWM)
							{
								stb.AddTxA((markerAt > -2) ? "GM_KD_map_0" : "GM_KD_map_0_no_marker", false);
							}
							else
							{
								stb.AddTxA((markerAt > -2) ? "GM_desc_map_1" : "GM_desc_map_1_no_marker", false);
							}
						}
					}
					NightController.SummonerData summonerData = null;
					WMIcon wmicon;
					EnemySummoner currentFocusEnemySummoner = this.WmCtr.getCurrentFocusEnemySummoner(out summonerData, out wmicon);
					if (wmiCurPos != null)
					{
						this.FD_ReturnBack = new WMIconDescription(wmicon, this.WmSkin.getWholeMapTarget().text_key, wmiCurPos.Lay.name);
					}
					if (currentFocusEnemySummoner != null && wmiCurPos != null)
					{
						this.prepareSummonerDesc();
						this.BxDesc.activate();
						if (this.WmSkin.is_zoomin)
						{
							FillBlock fillBlock = this.BxDesc.Get("summoner_desc", false) as FillBlock;
							Designer tab = this.BxDesc.getTab("ZM1").getTab("treasure");
							if (fillBlock != null && tab != null && this.SmnWmTarget1 != currentFocusEnemySummoner)
							{
								this.SmnWmTarget1 = currentFocusEnemySummoner;
								this.BxDesc.Get("summoner_title", false).setValue(currentFocusEnemySummoner.name_localized);
								using (STB stb2 = TX.PopBld(null, 0))
								{
									this.SmnWmTarget1.getDescription(wmiCurPos.SrcMap, stb2, false);
									fillBlock.Txt(stb2);
								}
								tab.Clear();
								tab.init();
								currentFocusEnemySummoner.recreateMBoxList(tab, base.M2D.NightCon.alreadyCleardInThisSession(currentFocusEnemySummoner, wmiCurPos.SrcMap));
								if (summonerData.summoner_is_night_real != currentFocusEnemySummoner.only_night)
								{
									summonerData.summoner_is_night = currentFocusEnemySummoner.only_night;
									this.WmSkin.fine_flag = (this.WmSkin.redraw_marker = true);
								}
							}
						}
						else if (this.SmnWmTarget0 != currentFocusEnemySummoner)
						{
							this.SmnWmTarget0 = currentFocusEnemySummoner;
							(this.BxDesc.Get("summoner_title0", false) as FillBlock).setValue(currentFocusEnemySummoner.name_localized);
						}
						if (!base.M2D.IMNG.has_recipe_collection)
						{
							goto IL_0375;
						}
						this.FD_Target = currentFocusEnemySummoner;
						using (STB stb3 = TX.PopBld(null, 0))
						{
							stb3.AddTxA("KD_go_to_def_in_catalog", false).Ret("\n").Append(stb, " ", 0, -1);
							stb.Clear().Set(stb3);
							goto IL_0375;
						}
					}
					if (this.BxDesc.isActive())
					{
						this.BxDesc.deactivate();
					}
					IL_0375:
					if (this.FD_Target == null)
					{
						List<QuestTracker.QuestProgress> focusdQuestArray = this.WmSkin.getFocusdQuestArray();
						if (focusdQuestArray != null)
						{
							int count = focusdQuestArray.Count;
							for (int i = 0; i < count; i++)
							{
								QuestTracker.QuestProgress questProgress = focusdQuestArray[i];
								if (questProgress.Q.getFieldGuideTarget(questProgress.phase, out this.FD_Target))
								{
									break;
								}
							}
						}
					}
				}
				else
				{
					this.WmCtr.fineMapKD(stb);
					if (this.BxDesc.isActive())
					{
						this.BxDesc.deactivate();
					}
				}
				if (P is FillBlock)
				{
					(P as FillBlock).Txt(stb);
				}
				else
				{
					P.setValue(stb.ToString());
				}
			}
		}

		private void prepareSummonerDesc()
		{
			bool flag = false;
			if (!this.wmsmn_setup)
			{
				this.wmsmn_setup = true;
				this.SmnWmTarget1 = null;
				this.SmnWmTarget0 = null;
				this.BxDesc.Clear();
				this.BxDesc.item_margin_x_px = 0f;
				this.BxDesc.item_margin_y_px = 2f;
				this.BxDesc.margin_in_lr = (this.BxDesc.margin_in_tb = 0f);
				this.BxDesc.alignx = ALIGN.CENTER;
				this.BxDesc.activate();
				this.BxDesc.init();
				Designer designer = this.BxDesc.addTab("ZM0", 340f, 62f, 340f, 62f, false);
				designer.Smallest();
				designer.margin_in_lr = 10f;
				designer.margin_in_tb = 12f;
				this.BxDesc.addP(new DsnDataP("", false)
				{
					text = " ",
					name = "summoner_title0",
					TargetFont = TX.getTitleFont(),
					swidth = designer.use_w,
					sheight = 24f,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					size = 22f,
					text_auto_condense = true,
					text_auto_wrap = false,
					TxCol = MTRX.ColWhite
				}, false);
				designer.Br();
				this.BxDesc.addP(new DsnDataP("", false)
				{
					text = TX.Get("KD_map_zoomin", ""),
					swidth = designer.use_w,
					sheight = 20f,
					alignx = ALIGN.LEFT,
					aligny = ALIGNY.MIDDLE,
					html = true,
					text_margin_x = 20f,
					size = 14f,
					TxCol = MTRX.ColWhite
				}, false);
				this.BxDesc.endTab(true);
				Designer designer2 = this.BxDesc.addTab("ZM1", 440f, 460f, 440f, 460f, false);
				designer2.Smallest();
				designer2.margin_in_lr = 40f;
				designer2.margin_in_tb = 51f;
				designer2.item_margin_y_px = 2f;
				designer2.init();
				this.BxDesc.addP(new DsnDataP("", false)
				{
					text = " ",
					name = "summoner_title",
					TargetFont = TX.getTitleFont(),
					swidth = designer2.use_w,
					sheight = 40f,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					size = 28f,
					TxCol = MTRX.ColWhite
				}, false);
				this.BxDesc.Hr(0.7f, 12f, 15f, 1f);
				this.BxDesc.addP(new DsnDataP("", false)
				{
					text = " ",
					name = "summoner_desc",
					swidth = designer2.use_w,
					sheight = 140f,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					size = 16f,
					html = true,
					text_auto_wrap = true,
					TxCol = MTRX.ColWhite
				}, false);
				this.BxDesc.Hr(0.7f, 12f, 15f, 1f);
				designer2.Br();
				float use_w = designer2.use_w;
				float use_h = designer2.use_h;
				Designer designer3 = designer2.addTab("treasure", use_w, use_h, use_w, use_h, false);
				designer3.Small();
				designer3.margin_in_lr = 0f;
				designer3.margin_in_tb = 5f;
				designer3.item_margin_y_px = 30f;
				designer2.endTab(true);
				this.BxDesc.endTab(true);
				flag = true;
			}
			DesignerRowMem.DsnMem designerBlockMemory = this.BxDesc.getDesignerBlockMemory(this.BxDesc.getTab("ZM0"));
			DesignerRowMem.DsnMem designerBlockMemory2 = this.BxDesc.getDesignerBlockMemory(this.BxDesc.getTab("ZM1"));
			if (designerBlockMemory2.active != this.WmSkin.is_zoomin || flag)
			{
				designerBlockMemory2.active = this.WmSkin.is_zoomin;
				designerBlockMemory.active = !this.WmSkin.is_zoomin;
				if (this.WmSkin.is_zoomin)
				{
					this.BxDesc.WH(440f, 460f);
					this.BxDesc.getBox().frametype = UiBox.FRAMETYPE.DARK;
					this.BxDesc.posSetDA(580f, 5f, 2, 60f, true);
				}
				else
				{
					this.BxDesc.posSetDA(this.BxR.w * 0.5f - 20f, -this.BxR.h * 0.5f + 95f, 2, 60f, true);
					this.BxDesc.WH(340f, 62f);
					this.BxDesc.getBox().frametype = UiBox.FRAMETYPE.DARK_SIMPLE;
				}
				this.BxDesc.row_remake_flag = true;
				this.BxDesc.rowRemakeCheck(false);
			}
		}

		internal override void runAppearing()
		{
			if (this.WmCtr != null)
			{
				this.WmCtr.runAppearing();
			}
		}

		internal override GMC_RES runEdit(float fcnt, bool handle)
		{
			if (this.WmSkin == null)
			{
				return GMC_RES.BACK_CATEGORY;
			}
			if (this.gres == GMC_RES.CONTINUE)
			{
				float num = this.GM.extend_right_w - this.BxR.margin_in_lr * 2f;
				if (this.WmSkin.swidth < num && X.D)
				{
					aBtn btn = this.WmSkin.getBtn();
					btn.WH(X.MULWALKMX(this.WmSkin.swidth, num, 0.3f * (float)X.AF, (float)(4 * X.AF)), this.WmSkin.sheight);
					this.BxR.RowRemakeHeightRecalc(btn, null);
				}
				if (this.WmSkin.is_wa && this.GM.cmdCategIs(UiGameMenu.STATE._MAP_MARKER))
				{
					this.GM.need_cmd_remake = true;
				}
				if (!this.WmCtr.runEdit(fcnt, handle, ref this.need_tuto_msg))
				{
					this.gres = GMC_RES.BACK_CATEGORY;
				}
				else if (handle && base.M2D.isRbkPD() && this.FD_Target != null)
				{
					this.WmSkin.getCursorMapPos();
					if (this.GM.initRecipeBook(this.FD_ReturnBack, this.FD_Target))
					{
						return GMC_RES.QUIT_GM;
					}
				}
				else if (!IN.isUiShiftO() && IN.isSubmit())
				{
					if (!this.WmSkin.fast_travel_active)
					{
						if (this.initMarkerSelect())
						{
							return GMC_RES.CONTINUE;
						}
						CURS.limitVib(this.WmSkin.getBtn(), AIM.R);
						SND.Ui.play("toggle_button_limit", false);
					}
					else
					{
						if (this.executeFastTravelConfirm())
						{
							return GMC_RES.CONTINUE;
						}
						CURS.limitVib(this.WmSkin.getBtn(), AIM.R);
						SND.Ui.play("toggle_button_limit", false);
					}
				}
			}
			else if (this.gres == GMC_RES.LOAD_GAME)
			{
				if (IN.isCancel() || !this.BxCmd.isFocused())
				{
					aBtn btn2 = this.BxCmd.getBtn("Cancel");
					if (btn2 != null)
					{
						btn2.ExecuteOnSubmitKey();
					}
					else
					{
						this.quitCmd(false);
					}
				}
				else if (base.M2D.isRbkPD() && this.initRecipeBookInCmd())
				{
					return GMC_RES.QUIT_GM;
				}
			}
			if (this.gres == GMC_RES.BACK_CATEGORY)
			{
				return GMC_RES.BACK_CATEGORY;
			}
			if (this.need_tuto_msg != 0)
			{
				if (this.TutoP != null)
				{
					this.fineKD(this.TutoP);
				}
				this.need_tuto_msg = 0;
			}
			return GMC_RES.CONTINUE;
		}

		private bool fnWholeMapAreaDragInit(aBtn B)
		{
			if (this.WmSkin != null && this.GM.isEditState())
			{
				this.WmSkin.dragInit();
			}
			return true;
		}

		private void fnQuitWholeMapDrag(bool aborted, bool shortclick, bool doubleclick)
		{
			if (!aborted && this.GM.isEditState() && this.WmSkin != null)
			{
				this.quitCmd(true);
				this.need_tuto_msg = 2;
				this.WmCtr.fineDragAfter(aborted, shortclick, doubleclick);
				if (!this.WmSkin.is_wa)
				{
					if (!this.WmSkin.fast_travel_active)
					{
						if (doubleclick)
						{
							this.initMarkerSelect();
							return;
						}
					}
					else if (shortclick)
					{
						this.executeFastTravelConfirm();
					}
				}
			}
		}

		public static float map2meshx(float mappos_x, float center_mapx, float size)
		{
			return WholeMapItem.map2meshx(mappos_x, center_mapx, size);
		}

		public static float map2meshy(float mappos_y, float center_mapy, float size)
		{
			return WholeMapItem.map2meshy(mappos_y, center_mapy, size);
		}

		public void FnDrawNightingale(ButtonSkinWholeMapArea WmSkin, MeshDrawer MdIco, float blink_alpha, float mappos_x, float mappos_y, float cell_size)
		{
			if (this.see_nightingale && WmSkin.getWholeMapTarget() == base.M2D.WM.CurWM)
			{
				WanderingNPC nightingale = base.M2D.WDR.getNightingale();
				Vector2 position = nightingale.getPosition();
				float num = UiGMCMap.map2meshx(position.x, mappos_x, cell_size);
				float num2 = UiGMCMap.map2meshy(position.y, mappos_y, cell_size);
				MdIco.Col = MdIco.ColGrd.White().setA1(blink_alpha).C;
				MdIco.RotaPF(num, num2, 1f, 1f, 0f, MTRX.getPF("IconNightingale"), false, false, false, uint.MaxValue, false, 0);
				if (!nightingale.isPositionDecided())
				{
					NEL.drawPointCurs(MdIco, num, num2, cell_size * base.M2D.WDR.getNightingale().catchable, blink_alpha, NEL.POINT_CURS.SUN_S);
				}
			}
		}

		private bool initMarkerSelect()
		{
			Vector2 cursorMapPos = this.WmSkin.getCursorMapPos();
			WholeMapItem wholeMapItem = (this.WmSkin.is_wa ? null : this.WmSkin.getWholeMapTarget());
			int num = ((this.Marker.marker_enabled && wholeMapItem != null) ? wholeMapItem.getMarkerAt((int)cursorMapPos.x, (int)cursorMapPos.y) : (-2));
			if (num >= -1 || this.WmSkin.hasFocusedQuest())
			{
				SND.Ui.play("tool_hand_init", false);
				this.Marker.FnClick = new FnBtnBindings(this.FnClickMarker);
				this.Marker.setFocusByMarkerId(num);
				if (this.FD_fnQuestBtnDefineOnMap == null)
				{
					this.FD_fnQuestBtnDefineOnMap = new UiQuestCard.FnQuestBtnDefine(this.fnQuestBtnDefineOnMap);
				}
				bool flag = false;
				if (this.GM.initBxCmd(UiGameMenu.STATE._MAP_MARKER, out this.BxCmd))
				{
					this.BxCmd.use_button_connection = false;
					this.BxCmd.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
					int num2 = 1;
					using (BList<string> blist = ListBuffer<string>.Pop(0))
					{
						int num3 = 1;
						if (this.Marker.marker_enabled)
						{
							this.Marker.getCheckKeys(blist, true, out num3);
							num2 = X.IntC((float)blist.Count / (float)num3);
						}
						this.BxCmd.margin_in_tb = 8f;
						this.BxCmd.WH(340f, (60f - this.BxCmd.margin_in_tb * 2f) * (float)num2 + this.BxCmd.margin_in_tb * 2f);
						this.BxCmd.margin_in_lr = 15f;
						this.BxCmd.item_margin_x_px = 20f;
						this.BxCmd.item_margin_y_px = 0f;
						this.BxCmd.selectable_loop = 1;
						this.BxCmd.alignx = ALIGN.CENTER;
						this.BxCmd.init();
						if (this.Marker.marker_enabled)
						{
							BtnContainerRadio<aBtn> btnContainerRadio = this.Marker.makeRadioTo(this.BxCmd, blist, num3, 44f, 5, false, true);
							this.marker_box_h = this.BxCmd.margin_in_tb * 2f + btnContainerRadio.get_sheight_px();
						}
						else
						{
							this.marker_box_h = 60f;
						}
					}
					this.QuestCon = this.BxCmd.Br().addTab("marker_quest", 340f - this.BxCmd.margin_in_lr * 2f, 1f, 0f, 1f, false);
					this.QuestCon.Small();
					this.QuestCon.scrollbox_bottom_margin = 0f;
					this.QuestCon.margin_in_tb = 12f;
					this.BxCmd.endTab(true);
				}
				else
				{
					this.QuestCon = this.BxCmd.getTab("marker_quest");
					flag = this.QuestCon.use_scroll;
					this.BxCmd.activate();
				}
				this.BxCmd.Focus();
				bool flag2 = false;
				List<QuestTracker.QuestProgress> focusdQuestArray = this.WmSkin.getFocusdQuestArray();
				UiQuestCard uiQuestCard = null;
				if (focusdQuestArray == null || focusdQuestArray.Count == 0)
				{
					if (flag)
					{
						this.QuestCon.Clear();
						this.QuestCon.stencil_ref = -1;
						this.QuestCon.use_scroll = false;
						this.QuestCon.WH(340f - this.BxCmd.margin_in_lr * 2f, 0f);
						this.QuestCon.init();
						this.BxCmd.WH(340f, this.marker_box_h);
						flag2 = (this.BxCmd.row_remake_flag = true);
					}
				}
				else
				{
					using (BList<Designer.EvacuateMem> blist2 = ListBuffer<Designer.EvacuateMem>.Pop(0))
					{
						if (!flag)
						{
							this.QuestCon.Clear();
							this.QuestCon.stencil_ref = 230;
							this.QuestCon.WH(520f - this.BxCmd.margin_in_lr * 2f, 1f);
							this.QuestCon.use_scroll = true;
							this.QuestCon.init();
							if (this.Marker.marker_enabled)
							{
								this.QuestCon.addHr(new DsnDataHr
								{
									margin_t = 14f,
									margin_b = 14f - this.QuestCon.margin_in_tb,
									Col = C32.d2c(4283780170U)
								});
							}
							flag2 = (this.BxCmd.row_remake_flag = true);
						}
						else
						{
							this.QuestCon.EvacuateMemory(blist2, (Designer Ds, DesignerRowMem.DsnMem DMem) => DMem.Blk is UiQuestCard, false);
							using (BList<Designer.EvacuateMem> blist3 = ListBuffer<Designer.EvacuateMem>.Pop(0))
							{
								this.QuestCon.EvacuateMemory(blist3, (Designer Ds, DesignerRowMem.DsnMem DMem) => DMem.Blk is DesignerHr, false);
								this.QuestCon.Clear();
								this.QuestCon.ReassignEvacuatedMemory(blist3, null, false);
							}
							this.QuestCon.WH(520f - this.BxCmd.margin_in_lr * 2f, 1f);
						}
						this.QuestCon.Br();
						int count = focusdQuestArray.Count;
						for (int i = 0; i < count; i++)
						{
							QuestTracker.QuestProgress questProgress = focusdQuestArray[i];
							Designer.EvacuateMem evacuateMem = null;
							UiQuestCard uiQuestCard2;
							if (blist2.Count > i)
							{
								evacuateMem = blist2[i];
								uiQuestCard2 = evacuateMem.Blk as UiQuestCard;
							}
							else
							{
								uiQuestCard2 = this.QuestCon.addTabT<UiQuestCard>("tab_" + i.ToString(), this.QuestCon.use_w, 0f, this.QuestCon.use_w, 50f, false);
								uiQuestCard2.FD_QuestBtnDefine = this.FD_fnQuestBtnDefineOnMap;
								uiQuestCard2.small_mode = true;
							}
							uiQuestCard2.QM = base.M2D.QUEST;
							if (uiQuestCard == null)
							{
								uiQuestCard = uiQuestCard2;
							}
							if (!uiQuestCard2.Is(questProgress.Q))
							{
								uiQuestCard2.initQ(this.QuestCon.stencil_ref, questProgress, new FnBtnBindings(this.fnClickQuestBtn));
								if (evacuateMem != null)
								{
									uiQuestCard2.cropBounds(-1f, 0f);
								}
								this.QuestCon.row_remake_flag = true;
							}
							uiQuestCard2.BelongDesigner = this.QuestCon;
							if (evacuateMem == null)
							{
								this.QuestCon.endTab(true);
							}
							else
							{
								this.QuestCon.ReassignEvacuatedMemory(evacuateMem);
							}
							this.QuestCon.Br();
						}
						for (int j = blist2.Count - 1; j >= count; j--)
						{
							blist2[j].destructObject();
						}
					}
					if (this.QuestCon.WHchecking(-1f, X.MMX(140f, this.QuestCon.scroll_inner_height + this.QuestCon.margin_in_tb * 2f, 240f)) || flag2)
					{
						this.BxCmd.RowRemakeHeightRecalc(this.QuestCon, null);
					}
					this.BxCmd.rowRemakeCheck(false);
					this.BxCmd.cropBounds(340f, this.marker_box_h);
				}
				this.gres = GMC_RES.LOAD_GAME;
				BtnContainerRunner btnContainerRunner = this.BxCmd.Get("_mapmarker_radio", false) as BtnContainerRunner;
				using (BList<aBtn> blist4 = ListBuffer<aBtn>.Pop(0))
				{
					if (btnContainerRunner != null)
					{
						if (num < -1)
						{
							btnContainerRunner.setValue("-1");
							btnContainerRunner.BCon.setLockedAll(true);
							if (uiQuestCard == null || uiQuestCard.SelectFirstButton(null) == null)
							{
								btnContainerRunner.Get(0).Select(true);
							}
						}
						else
						{
							btnContainerRunner.BCon.setLockedAll(false);
							btnContainerRunner.BCon.copyVectorTo(blist4);
							if (this.Marker.focus + 1 < btnContainerRunner.BCon.Length)
							{
								btnContainerRunner.setValue((this.Marker.focus + 1).ToString());
								btnContainerRunner.Get(this.Marker.focus + 1).Select(true);
							}
							else
							{
								btnContainerRunner.setValue("0");
								btnContainerRunner.Get(0).Select(true);
							}
						}
						btnContainerRunner.BCon.clearNaviAll(10U, true);
					}
					else if (uiQuestCard != null)
					{
						uiQuestCard.SelectFirstButton(null);
					}
					if (uiQuestCard != null)
					{
						UiQuestCard.relinkNaviAll(this.QuestCon, blist4);
					}
				}
				Vector2 vector = base.InverseTransformPoint(this.WmSkin.local2global(this.WmSkin.Map2Upos(this.WmSkin.getCursorMapPos(), false), true));
				float right_box_center_x = this.GM.right_box_center_x;
				float num4 = X.MMX(right_box_center_x - 200f, vector.x * 64f, right_box_center_x + 200f);
				if (vector.y > 0.703125f || uiQuestCard != null)
				{
					this.BxCmd.positionD(num4, vector.y * 64f - 20f - this.BxCmd.get_sheight_px() * 0.5f, 3, 18f);
				}
				else
				{
					this.BxCmd.positionD(num4, vector.y * 64f + 20f + this.BxCmd.get_sheight_px() * 0.5f, 1, 18f);
				}
				return true;
			}
			return false;
		}

		private void fnQuestBtnDefineOnMap(UiQuestCard Tab, List<string> Acmd_list)
		{
			if (Acmd_list.IndexOf("reveal_map") >= 0 && this.WmSkin.is_detail)
			{
				QuestTracker.QuestDeperture currentDepert = Tab.getCurrentDepert();
				if (currentDepert.isActiveMap() && currentDepert.wm_key == this.WmSkin.getWholeMapTarget().text_key)
				{
					Acmd_list.Remove("reveal_map");
				}
			}
			Acmd_list.Add("&&button_quest_jump");
		}

		public bool executeFastTravelConfirm()
		{
			if (!this.WmSkin.FastTravelFocused.valid || this.BenchChip == null || this.BenchChip.IconIs(this.WmSkin.FastTravelFocused.get_Icon()))
			{
				return false;
			}
			SND.Ui.play("tool_hand_init", false);
			if (this.GM.initBxCmd(UiGameMenu.STATE._MAP_FASTTRAVEL, out this.BxCmd))
			{
				this.QuestCon = null;
				this.BxCmd.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
				this.BxCmd.margin_in_lr = 40f;
				this.BxCmd.margin_in_tb = 22f;
				this.BxCmd.WH(340f, 56f + this.BxCmd.margin_in_tb * 2f);
				this.BxCmd.item_margin_x_px = 20f;
				this.BxCmd.selectable_loop = 2;
				this.BxCmd.alignx = ALIGN.CENTER;
				this.BxCmd.init();
				Designer bxCmd = this.BxCmd;
				DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
				dsnDataButtonMulti.name = "fasttravel_btns";
				dsnDataButtonMulti.titles = new string[] { "&&Btn_move", "&&Cancel" };
				dsnDataButtonMulti.skin = "row_center";
				dsnDataButtonMulti.clms = 1;
				dsnDataButtonMulti.w = this.BxCmd.use_w;
				dsnDataButtonMulti.h = 28f;
				dsnDataButtonMulti.margin_h = 0f;
				dsnDataButtonMulti.margin_w = 0f;
				dsnDataButtonMulti.fnClick = new FnBtnBindings(this.fnClickFastTravelCmd);
				dsnDataButtonMulti.default_focus = 1;
				dsnDataButtonMulti.fnMaking = delegate(BtnContainer<aBtn> BCon, aBtn B)
				{
					B.click_snd = ((B.title == "&&Cancel") ? "cancel" : "enter_small");
					return true;
				};
				bxCmd.addButtonMultiT<aBtnNel>(dsnDataButtonMulti);
			}
			else
			{
				this.BxCmd.setValueTo("fasttravel_btns", "");
				this.BxCmd.activate();
			}
			this.BxCmd.Focus();
			this.BxCmd.selectDefSel();
			this.gres = GMC_RES.LOAD_GAME;
			Vector2 vector = base.InverseTransformPoint(this.WmSkin.local2global(this.WmSkin.Map2Upos(this.WmSkin.getCursorMapPos(), false), true));
			float right_box_center_x = base.right_box_center_x;
			float num = X.MMX(right_box_center_x - 200f, vector.x * 64f, right_box_center_x + 200f);
			if (vector.y > 0.703125f)
			{
				this.BxCmd.positionD(num, vector.y * 64f - 20f - this.BxCmd.get_sheight_px() * 0.5f, 3, 18f);
			}
			else
			{
				this.BxCmd.positionD(num, vector.y * 64f + 20f + this.BxCmd.get_sheight_px() * 0.5f, 1, 18f);
			}
			return true;
		}

		private bool FnClickMarker(aBtn B)
		{
			WholeMapItem wholeMapTarget = this.WmSkin.getWholeMapTarget();
			if (!B.isLocked() && wholeMapTarget != null && wholeMapTarget.setMarkerAt(this.WmSkin.getCursorMapPos(), this.Marker.getMarkerId(B.title), true))
			{
				this.WmSkin.redraw_marker = true;
			}
			this.quitCmd(true);
			return true;
		}

		private bool fnClickFastTravelCmd(aBtn B)
		{
			B.SetChecked(true, true);
			if (B.title == "&&Cancel")
			{
				this.quitCmd(false);
				return false;
			}
			this.GM.bench_done_cmd = false;
			WMIconPosition fastTravelFocused = this.WmSkin.FastTravelFocused;
			if (!fastTravelFocused.valid || this.BenchChip == null || this.BenchChip.IconIs(fastTravelFocused.get_Icon()))
			{
				return true;
			}
			UiBenchMenu.ExecuteFastTravel(fastTravelFocused, null, null, null);
			return true;
		}

		private void quitCmd(bool no_snd = false)
		{
			if (this.BxCmd != null)
			{
				this.BxCmd.deactivate();
				if (!no_snd)
				{
					SND.Ui.play("cancel", false);
				}
			}
			this.QuestCon = null;
			this.BxR.Focus();
			this.WmSkin.getBtn().Select(true);
			if (this.gres == GMC_RES.LOAD_GAME)
			{
				this.gres = GMC_RES.CONTINUE;
			}
		}

		private bool fnClickQuestBtn(aBtn B)
		{
			string title = B.title;
			if (title != null)
			{
				if (!(title == "reveal_map"))
				{
					if (!(title == "&&button_quest_jump"))
					{
						if (!(title == "quest_tracking"))
						{
							if (title == "&&KD_go_to_def_in_catalog")
							{
								this.initRecipeBookInCmd();
								return true;
							}
						}
						else if (this.WmSkin != null)
						{
							this.WmSkin.redraw_marker = true;
						}
					}
					else
					{
						if (this.WmSkin == null)
						{
							return false;
						}
						if (this.QuestCon == null)
						{
							return false;
						}
						UiQuestCard uiQuestCard = UiQuestCard.getTabForButton(this.QuestCon, B);
						if (uiQuestCard == null)
						{
							return false;
						}
						QuestTracker.Quest quest = uiQuestCard.getQuest();
						QuestTracker.CATEG categ;
						UiGameMenu.SCENARIO_CTG scenario_CTG;
						if ((quest.categ & QuestTracker.CATEG.MAIN) != (QuestTracker.CATEG)0)
						{
							categ = QuestTracker.CATEG.MAIN;
							scenario_CTG = UiGameMenu.SCENARIO_CTG.MAIN_QUEST;
						}
						else
						{
							categ = QuestTracker.CATEG.SUB;
							scenario_CTG = UiGameMenu.SCENARIO_CTG.SUB_QUEST;
						}
						base.M2D.QUEST.UiDefaultFocusChange(categ, quest);
						this.GM.clearMapCancelingJump();
						this.quitCmd(false);
						ButtonSkinWholeMapArea.PosMemory positionMemory = this.WmSkin.getPositionMemory();
						UiGMCScenario uiGMCScenario = this.GM.initCategoryEdit(CATEG.SCENARIO, true) as UiGMCScenario;
						if (uiGMCScenario != null)
						{
							this.QuestCancelingMapJump = positionMemory;
							uiGMCScenario.initScnTab(scenario_CTG);
						}
					}
				}
				else
				{
					if (this.WmSkin == null || this.QuestCon == null)
					{
						return false;
					}
					UiQuestCard uiQuestCard = UiQuestCard.getTabForButton(this.QuestCon, B);
					if (uiQuestCard == null)
					{
						return false;
					}
					this.quitCmd(false);
					this.WmSkin.reveal(uiQuestCard.getCurrentDepert().WmDepert(uiQuestCard.Prog, uiQuestCard.Prog.phase), false, false);
				}
			}
			return true;
		}

		public bool initRecipeBookInCmd()
		{
			object obj = null;
			UiQuestCard tabForButton = base.M2D.QUEST.getTabForButton(aBtn.PreSelected);
			if (tabForButton != null)
			{
				QuestTracker.QuestProgress prog = tabForButton.Prog;
				prog.Q.getFieldGuideTarget(prog.phase, out obj);
			}
			return this.GM.initRecipeBook(this.FD_ReturnBack, obj ?? this.FD_Target);
		}

		public void clearQuestCancelingJump()
		{
			this.QuestCancelingMapJump.WM = null;
		}

		public bool executeQuestCancelingJump()
		{
			if (this.QuestCancelingMapJump.WM != null)
			{
				this.GM.initCategoryEdit(CATEG.MAP, false);
				if (this.GM.isShowingGMC(this))
				{
					this.WmSkin.setWholeMapTarget(this.QuestCancelingMapJump);
					return true;
				}
			}
			return false;
		}

		public void reveal(WmDeperture Depert, bool no_first_delay = false)
		{
			if (this.WmSkin != null)
			{
				this.WmSkin.reveal(Depert, no_first_delay, false);
			}
		}

		public void reveal(WMIconDescription Depert)
		{
			if (this.WmSkin != null && TX.valid(Depert.wm_key))
			{
				WholeMapItem byTextKey = base.M2D.WM.GetByTextKey(Depert.wm_key);
				WholeMapItem.WMItem wmitem = ((byTextKey != null) ? byTextKey.GetWmi(base.M2D.Get(Depert.map_key, true), null) : null);
				if (wmitem != null)
				{
					if (Depert.Icon != null)
					{
						Vector2 mapWmPos = Depert.Icon.getMapWmPos(wmitem);
						this.WmSkin.SetCenter(wmitem.Rc.x + mapWmPos.x, wmitem.Rc.y + mapWmPos.y, false, true);
						return;
					}
					this.WmSkin.SetCenter(wmitem.Rc.cx, wmitem.Rc.cy, false, true);
				}
			}
		}

		private bool wmsmn_setup;

		private float first_map_center_x;

		private float first_map_center_y;

		private bool see_nightingale;

		private byte need_tuto_msg;

		private EnemySummoner SmnWmTarget0;

		private EnemySummoner SmnWmTarget1;

		private UiGmMapMarker Marker;

		private UiBoxDesigner BxCmd;

		private Designer QuestCon;

		private UiWmSkinController WmCtr;

		private ButtonSkinWholeMapArea WmSkin;

		private FillBlock TutoP;

		private float marker_box_h;

		private object FD_Target;

		private WMIconDescription FD_ReturnBack;

		private const float marker_def_h = 60f;

		private GMC_RES gres = GMC_RES.BACK_CATEGORY;

		internal ButtonSkinWholeMapArea.PosMemory QuestCancelingMapJump;

		private UiQuestCard.FnQuestBtnDefine FD_fnQuestBtnDefineOnMap;
	}
}
