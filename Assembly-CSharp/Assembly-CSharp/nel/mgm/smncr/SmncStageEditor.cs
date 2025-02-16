using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using UnityEngine.InputSystem;
using XX;

namespace nel.mgm.smncr
{
	public class SmncStageEditor : IRunAndDestroy
	{
		private float bx_listh
		{
			get
			{
				return IN.h * 0.6f;
			}
		}

		private float bx_listx
		{
			get
			{
				return (-IN.wh + 17f + 170f) * (float)X.MPF(this.stgolist_left);
			}
		}

		private int bx_slide_aim
		{
			get
			{
				if (!this.stgolist_left)
				{
					return 0;
				}
				return 2;
			}
		}

		private int bx_list_slide_w
		{
			get
			{
				return 365;
			}
		}

		private float bx_mknx
		{
			get
			{
				return (-IN.wh + 77f + 210f) * (float)X.MPF(this.stgolist_left);
			}
		}

		private float bx_mknh
		{
			get
			{
				return IN.h * 0.5f;
			}
		}

		private Vector2Int PlantCurs
		{
			get
			{
				return new Vector2Int((int)this.PosPlant.x, (int)(this.PosPlant.w - 0.01f - (float)this.LpArea.mapy));
			}
		}

		private Vector2Int PlantCursM
		{
			get
			{
				Vector2Int plantCurs = this.PlantCurs;
				plantCurs.x += this.LpArea.mapx;
				plantCurs.y += this.LpArea.mapy;
				return plantCurs;
			}
		}

		public SmncStageEditor(M2LpUiSmnCreator _LpArea, SmncStageEditorManager _Mng, UiBoxDesignerFamily _DsFam = null, M2Mover _MvCam = null)
		{
			this.LpArea = _LpArea;
			this.M2D.loadMaterialSnd("ev_city");
			this.Mng = _Mng;
			this.MvCam = _MvCam;
			this.camera_move_enabled = this.MvCam != null;
			if (_DsFam == null)
			{
				this.DsFam = IN.CreateGobGUI(null, "-SmncStageEditor").AddComponent<UiBoxDesignerFamily>();
				this.original_family = true;
			}
			else
			{
				this.DsFam = _DsFam;
			}
			this.WholeArea = IN.CreateGob(this.DsFam.gameObject, "-WholeArea").AddComponent<aBtn>();
			this.WholeArea.w = IN.w + 10f;
			this.WholeArea.h = IN.h + 10f;
			IN.setZ(this.WholeArea.transform, 0.5f);
			this.WholeArea.initializeSkin("transparent", "");
			this.WholeArea.unselectable(true);
			this.WholeArea.addDownFn(delegate(aBtn B)
			{
				this.area_drag_enabled = true;
				return true;
			});
			this.WholeArea.addUpFn(delegate(aBtn B)
			{
				this.area_drag_enabled = false;
				return true;
			});
		}

		public void destruct()
		{
			this.quitFile(this.CurFile);
			this.runner_assigned = false;
			if (this.original_family)
			{
				IN.DestroyE(this.DsFam.gameObject);
			}
			if (this.MdWeb != null)
			{
				IN.DestroyE(this.GobMdMkn);
				IN.DestroyE(this.GobMdMkn);
				IN.DestroyE(this.GobMdWeb);
			}
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					IN.addRunner(this);
					return;
				}
				IN.remRunner(this);
			}
		}

		public void activate(SmncFile _File = null, byte _file_index = 0)
		{
			if (_File != null && _File != this.CurFile)
			{
				this.initFile(_File, _file_index);
			}
			if (this.state == SmncStageEditor.STATE.OFFLINE && this.CurFile != null)
			{
				SmncStageEditorManager.StgObject stgObject;
				int num;
				if (this.Mng.OSteo.TryGetValue("_smnc_generate_pr", out stgObject) && !this.CurFile.FindStgo("_smnc_generate_pr", out num).valid)
				{
					stgObject.x = this.LpArea.mapw / 2;
					stgObject.y = this.LpArea.maph / 2;
					this.CurFile.Astgo.Insert(0, stgObject);
					this.need_redraw_base = true;
				}
				this.changeState(SmncStageEditor.STATE.LIST);
			}
		}

		public void deactivate()
		{
			if (this.state != SmncStageEditor.STATE.OFFLINE)
			{
				this.changeState(SmncStageEditor.STATE.OFFLINE);
			}
		}

		public void hideMesh()
		{
			if (this.MdWeb != null)
			{
				this.GobMdMkn.SetActive(false);
				this.GobMdWeb.SetActive(false);
				this.GobMdPut.SetActive(false);
			}
		}

		private bool changeState(SmncStageEditor.STATE stt)
		{
			if (stt == this.state)
			{
				return false;
			}
			if (stt == SmncStageEditor.STATE.MAKENEW && this.state == SmncStageEditor.STATE.LIST && (this.CurFile == null || this.CurFile.Astgo.Count >= 200))
			{
				SND.Ui.play("locked", false);
				return false;
			}
			SmncStageEditor.STATE state = this.state;
			this.state = stt;
			this.t_state = 0f;
			if (state == SmncStageEditor.STATE.OFFLINE)
			{
				if (this.auto_run)
				{
					this.runner_assigned = true;
				}
				this.stgolist_left = true;
				this.Mng.reload(false);
				this.prepareMesh();
				this.GobMdMkn.SetActive(true);
				this.GobMdWeb.SetActive(true);
				this.GobMdPut.SetActive(true);
				this.checkCamMovable();
			}
			if (state == SmncStageEditor.STATE.LIST)
			{
				this.BConL.use_valotile = false;
				if (this.state != SmncStageEditor.STATE.MAKENEW && this.state != SmncStageEditor.STATE.SAVING)
				{
					this.BxL.posSetDA(this.bx_listx, -1000f, this.bx_slide_aim, (float)this.bx_list_slide_w, true);
					this.BxL.deactivate();
				}
				else
				{
					this.BxL.hide();
				}
			}
			if (state == SmncStageEditor.STATE.MAKENEW_MOVE && this.state == SmncStageEditor.STATE.MAKENEW)
			{
				this.BxL.activate();
			}
			if (state == SmncStageEditor.STATE.MAKENEW)
			{
				this.BxMkn.deactivate();
			}
			if (state == SmncStageEditor.STATE.MOVE)
			{
				this.need_redraw_base = true;
			}
			bool flag = false;
			switch (this.state)
			{
			case SmncStageEditor.STATE.OFFLINE:
				this.StgoMaking = default(SmncStageEditorManager.StgObject);
				if (this.BxL != null)
				{
					this.BxL.deactivate();
				}
				break;
			case SmncStageEditor.STATE.LIST:
				if (this.LpArea.chip_inserted)
				{
					this.LpArea.removeDecidedChips();
					this.need_redraw_base = true;
				}
				this.StgoMaking = default(SmncStageEditorManager.StgObject);
				this.walk_aim = -1;
				if (state == SmncStageEditor.STATE.MAKENEW)
				{
					SND.Ui.play("cancel", false);
				}
				if (state == SmncStageEditor.STATE.OFFLINE)
				{
					SND.Ui.play("editor_open", false);
				}
				this.createStgoBox(this.stgo_pre_selected);
				if (this.need_consider_config)
				{
					this.finePlantAttach();
				}
				break;
			case SmncStageEditor.STATE.SAVING:
				this.CurFile.saveToFile((int)this.file_index, new Action<string, string>(this.fnFinishedSaveFile));
				break;
			case SmncStageEditor.STATE.MAKENEW:
				this.walk_aim = -1;
				if (state == SmncStageEditor.STATE.LIST)
				{
					SND.Ui.play("tool_changegear", false);
				}
				this.createMakeNewBox();
				break;
			case SmncStageEditor.STATE.MAKENEW_MOVE:
			case SmncStageEditor.STATE.MOVE:
				this.move_makenew = 0;
				flag = true;
				this.BxL.deactivate();
				this.t_state = 100f;
				if (this.state == SmncStageEditor.STATE.MOVE)
				{
					SND.Ui.play("tool_hand_init", false);
					this.need_redraw_base = true;
				}
				break;
			case SmncStageEditor.STATE.SET_PLANT:
				if (this.need_consider_config)
				{
					this.finePlantAttach();
				}
				flag = true;
				SND.Ui.play("tool_selrect", false);
				this.StgoMaking = default(SmncStageEditorManager.StgObject);
				this.PosPlant.z = -1f;
				this.need_redraw_mkn = true;
				this.hold_adding = 0;
				this.t_state = 100f;
				break;
			}
			if (flag)
			{
				if (!IN.use_mouse)
				{
					this.t_mouse = 0f;
				}
				this.MouseScroll = Vector2.zero;
				this.walk_aim = -1;
			}
			this.need_fine_kd = true;
			if (this.FD_stateChangeListener != null)
			{
				this.FD_stateChangeListener(this.state, state);
			}
			this.need_redraw_mkn = true;
			IN.clearPushDown(false);
			return true;
		}

		public void initFile(SmncFile File, byte index)
		{
			this.quitFile(this.CurFile);
			this.CurFile = File;
			this.file_index = index;
			this.CurFile.openFile();
			this.LpArea.initFile(File);
			this.need_redraw_base = true;
			this.need_recreate_stgo_list = (this.need_consider_config = true);
			this.redrawMkn();
		}

		public void quitFile(SmncFile File)
		{
			if (this.CurFile != null && File == this.CurFile)
			{
				this.CurFile = null;
				this.stgo_pre_selected = -1;
				this.need_redraw_base = true;
			}
		}

		public bool run(float fcnt)
		{
			this.PosMouseCur = IN.getMousePos(null);
			bool flag = false;
			switch (this.state)
			{
			case SmncStageEditor.STATE.OFFLINE:
				return false;
			case SmncStageEditor.STATE.LIST:
				if (IN.isUiRemPD() && X.BTW(0f, (float)this.stgo_pre_selected, (float)this.CurFile.Astgo.Count))
				{
					SmncStageEditorManager.StgObject stgObject = this.CurFile.Astgo[this.stgo_pre_selected];
					if (!this.stgo_removeable)
					{
						CURS.limitVib(this.BConL.Get(this.stgo_pre_selected), AIM.R);
						SND.Ui.play("locked", false);
					}
					else
					{
						this.CurFile.Astgo.RemoveAt(this.stgo_pre_selected);
						this.need_recreate_stgo_list = (this.need_consider_config = true);
						this.need_redraw_base = true;
						SND.Ui.play("reset_var", false);
						this.finePlantAttach();
					}
				}
				if (IN.isUiAddPD())
				{
					this.changeState(SmncStageEditor.STATE.MAKENEW);
				}
				else
				{
					if (this.need_recreate_stgo_list)
					{
						this.need_recreate_stgo_list = false;
						this.BConL.RemakeT<aBtnNel>(null, "");
						this.BConL.Get(X.MMX(0, this.stgo_pre_selected, this.BConL.Length - 1)).Select(true);
						this.fineStgoHeader();
					}
					if (this.enable_plant_tx_key != null && IN.isUiShiftPD())
					{
						if (X.BTW(0f, (float)this.stgo_pre_selected, (float)this.CurFile.Astgo.Count))
						{
							if (this.need_consider_config)
							{
								this.finePlantAttach();
							}
							SmncStageEditorManager.StgObject stgObject2 = this.CurFile.Astgo[this.stgo_pre_selected];
							this.PosPlant.x = (float)((int)stgObject2.cx);
							this.PosPlant.y = (float)((int)stgObject2.cy);
							this.PosPlant.y = this.Mp.getFootableY(this.PosPlant.x + (float)this.LpArea.mapx + 0.5f, (int)this.PosPlant.y + this.LpArea.mapy, this.LpArea.maph, true, -1f, true, true, false, 0f) - (float)this.LpArea.mapy;
						}
						this.changeState(SmncStageEditor.STATE.SET_PLANT);
					}
					else if (IN.isCancel())
					{
						if (aBtn.PreSelected == this.BConL.Get("&&Cancel"))
						{
							this.BConL.setValue(this.BConL.Length - 1, true);
						}
						else
						{
							this.BConL.Get("&&Cancel").Select(true);
							SND.Ui.play("cancel", false);
						}
					}
				}
				break;
			case SmncStageEditor.STATE.MAKENEW:
				if (IN.isCancel())
				{
					this.BConMkn.setValue(this.BConMkn.Length - 1, true);
					return true;
				}
				if (IN.isUiAddPD() && aBtn.PreSelected != null)
				{
					aBtn.PreSelected.ExecuteOnSubmitKey();
				}
				break;
			case SmncStageEditor.STATE.MAKENEW_MOVE:
				flag = true;
				this.runMoveState(fcnt, SmncStageEditor.STATE.MAKENEW, true);
				break;
			case SmncStageEditor.STATE.MOVE:
			{
				flag = true;
				int x = this.StgoMaking.x;
				int y = this.StgoMaking.y;
				int num = this.stgo_pre_selected;
				this.runMoveState(fcnt, SmncStageEditor.STATE.LIST, false);
				if (this.state == SmncStageEditor.STATE.MOVE && num >= 0 && this.StgoMaking.valid && (x != this.StgoMaking.x || y != this.StgoMaking.y))
				{
					int num2 = this.StgoMaking.x - x;
					int num3 = this.StgoMaking.y - y;
					for (int i = this.CurFile.Aplant.Count - 1; i >= 0; i--)
					{
						SmncFile.PlantInfo plantInfo = this.CurFile.Aplant[i];
						if (plantInfo.attach_index == num)
						{
							plantInfo.x += num2;
							plantInfo.y += num3;
							this.CurFile.Aplant[i] = plantInfo;
							this.need_redraw_base = true;
						}
					}
				}
				break;
			}
			case SmncStageEditor.STATE.SET_PLANT:
				flag = true;
				this.runPlantAttach(fcnt);
				break;
			}
			this.checkScrollMouse(flag);
			if (this.MdMkn != null)
			{
				this.redrawMkn();
			}
			this.t_state += fcnt;
			this.PosMousePre = this.PosMouseCur;
			return true;
		}

		private void createStgoBox(int select_index = -1)
		{
			if (this.BxL == null)
			{
				this.BxL = this.DsFam.Create("BxFile", this.bx_listx, IN.hh - 17f - this.bx_listh * 0.5f, 340f, this.bx_listh, this.bx_slide_aim, (float)this.bx_list_slide_w, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.BxL.margin_in_tb = 12f;
				this.BxL.margin_in_lr = 18f;
				this.BxL.item_margin_y_px = 0f;
				this.BxL.box_stencil_ref_mask = -1;
				this.BxL.init();
				this.FbL = this.BxL.addP(new DsnDataP("", false)
				{
					TxCol = NEL.ColText,
					text = " ",
					size = 14f,
					text_margin_x = 40f,
					text_margin_y = 4f,
					sheight = 17f,
					aligny = ALIGNY.MIDDLE,
					alignx = ALIGN.CENTER,
					swidth = this.BxL.use_w
				}, false);
				this.BxL.addHr(new DsnDataHr
				{
					draw_width_rate = 0.87f,
					margin_b = 10f,
					margin_t = 0f,
					Col = NEL.ColText
				});
				this.BConL = this.BxL.addRadioT<aBtnNel>(new DsnDataRadio
				{
					name = "list",
					w = this.BxL.use_w - 18f,
					h = 30f,
					margin_h = 0,
					margin_w = 0,
					skin = "row",
					click_snd = "enter_small",
					fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangedListStgo),
					navi_loop = 2,
					all_function_same = true,
					fnHover = new FnBtnBindings(this.fnHoverStgoRow),
					fnGenerateKeys = new FnGenerateRemakeKeys(this.fnGenerateKeysStgo),
					APoolEvacuated = new List<aBtn>(),
					fnMakingAfter = delegate(BtnContainer<aBtn> BCon, aBtn B)
					{
						if (!TX.isStart(B.title, "&&", 0))
						{
							int num = X.NmI(B.title, 0, false, false);
							B.setSkinTitle(this.CurFile.Astgo[num].localized_name);
						}
						if (B.title == "Smnc_save_file")
						{
							B.setSkinTitle("<img mesh=\"directory\" tx_color/>" + TX.Get("Smnc_save_file", ""));
						}
						if (B.title == "&&Smnc_stageedit_add")
						{
							B.click_snd = "";
						}
						if (B.title == "&&Cancel")
						{
							B.click_snd = "cancel";
						}
						return true;
					},
					SCA = new ScrollAppend(10, this.BxL.use_w, this.BxL.use_h - 15f, 4f, 10f, 10)
				});
				this.need_recreate_stgo_list = false;
				this.fineStgoHeader();
			}
			else
			{
				this.BxL.posSetA(this.bx_listx, -1000f, true);
			}
			if (this.need_recreate_stgo_list)
			{
				this.need_recreate_stgo_list = false;
				this.BConL.RemakeT<aBtnNel>(null, "");
				this.fineStgoHeader();
			}
			this.BConL.setValue(-1, true);
			this.BxL.activate();
			this.BxL.bind();
			this.BConL.use_valotile = true;
			this.BConL.Get(X.MMX(0, select_index, this.BConL.Length - 1)).Select(true);
		}

		private void fineStgoHeader()
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("Smnc_ui_file", false).TxRpl((int)(this.file_index + 1));
				stb.Add(": ", this.CurFile.Astgo.Count, "/").Add(200);
				this.FbL.Txt(stb);
			}
		}

		private void fnGenerateKeysStgo(BtnContainerBasic BCon, List<string> Adest)
		{
			int count = this.CurFile.Astgo.Count;
			for (int i = 0; i < count; i++)
			{
				Adest.Add(i.ToString());
			}
			Adest.Add("&&Smnc_stageedit_add");
			if (this.enable_plant_tx_key != null)
			{
				if (this.plant_set_btn_key == null)
				{
					this.plant_set_btn_key = "&&" + this.enable_plant_tx_key;
				}
				Adest.Add(this.plant_set_btn_key);
			}
			if (this.Alistbtn_addition != null)
			{
				Adest.AddRange(this.Alistbtn_addition);
			}
			if (this.enabled_file_export)
			{
				Adest.Add("Smnc_save_file");
			}
			Adest.Add("&&Cancel");
		}

		private bool fnChangedListStgo(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (cur_value < 0)
			{
				return true;
			}
			if (this.state != SmncStageEditor.STATE.LIST)
			{
				return false;
			}
			if (this.smnc_io_lock >= IN.totalframe)
			{
				return false;
			}
			string title = _B.Get(cur_value).title;
			if (title != null)
			{
				if (title == "&&Cancel")
				{
					SND.Ui.play("cancel", false);
					this.changeState(SmncStageEditor.STATE.OFFLINE);
					return true;
				}
				if (title == "&&Smnc_stageedit_add")
				{
					return this.changeState(SmncStageEditor.STATE.MAKENEW);
				}
				if (title == "Smnc_save_file")
				{
					return this.changeState(SmncStageEditor.STATE.SAVING);
				}
			}
			if (title == this.plant_set_btn_key)
			{
				return this.changeState(SmncStageEditor.STATE.SET_PLANT);
			}
			if (this.CurFile != null && X.BTW(0f, (float)cur_value, (float)this.CurFile.Astgo.Count))
			{
				this.StgoMaking = this.CurFile.Astgo[cur_value];
				SmncStageEditorManager.StgObject stgObject;
				if (this.Mng.OSteo.TryGetValue(this.StgoMaking.key, out stgObject))
				{
					this.StgoMaking.copyDictData(stgObject);
				}
				this.stgo_pre_selected = cur_value;
				this.StgoMovingPre = this.StgoMaking;
				this.changeState(SmncStageEditor.STATE.MOVE);
			}
			else if (this.FD_ListBtnClicked != null)
			{
				return this.FD_ListBtnClicked(_B.Get(cur_value));
			}
			return true;
		}

		private bool fnHoverStgoRow(aBtn B)
		{
			if (this.state != SmncStageEditor.STATE.LIST)
			{
				return false;
			}
			this.stgo_pre_selected = B.carr_index;
			this.need_redraw_mkn = true;
			this.need_fine_kd = true;
			if (!IN.use_mouse && X.BTW(0f, (float)this.stgo_pre_selected, (float)this.CurFile.Astgo.Count))
			{
				SmncStageEditorManager.StgObject stgObject = this.CurFile.Astgo[this.stgo_pre_selected];
				float x = this.M2D.Cam.UPos2Screen(new Vector2(this.Mp.map2globalux((float)(stgObject.x + this.LpArea.mapx)), 0f)).x;
				float x2 = this.M2D.Cam.UPos2Screen(new Vector2(this.Mp.map2globalux((float)(stgObject.x + stgObject.clms + this.LpArea.mapx)), 0f)).x;
				bool flag = x <= (-IN.wh + 220f) * 0.015625f;
				bool flag2 = x2 >= (IN.wh - 220f) * 0.015625f;
				if (this.stgolist_left)
				{
					if (flag && !flag2)
					{
						this.stgolist_left = false;
						this.BxL.posSetA(this.BxL.getBox().get_deperture_x(), this.BxL.getBox().get_deperture_y(), this.bx_listx, -1000f, false);
					}
				}
				else if (!flag && flag2)
				{
					this.stgolist_left = true;
					this.BxL.posSetA(this.BxL.getBox().get_deperture_x(), this.BxL.getBox().get_deperture_y(), this.bx_listx, -1000f, false);
				}
			}
			return true;
		}

		public bool stgo_removeable
		{
			get
			{
				if (X.BTW(0f, (float)this.stgo_pre_selected, (float)this.CurFile.Astgo.Count))
				{
					SmncStageEditorManager.StgObject stgObject = this.CurFile.Astgo[this.stgo_pre_selected];
					return !(stgObject.key == "_smnc_generate_pr") || !this.Mng.OSteo.ContainsKey(stgObject.key);
				}
				return false;
			}
		}

		private void createMakeNewBox()
		{
			if (this.BxMkn == null)
			{
				this.BxMkn = this.DsFam.Create("BxFile", this.bx_mknx, IN.hh - 57f - this.bx_mknh * 0.5f, 420f, this.bx_mknh, this.bx_slide_aim, 525f, UiBoxDesignerFamily.MASKTYPE.BOX);
				IN.setZ(this.BxMkn.transform, -0.5f);
				this.BxMkn.margin_in_tb = 16f;
				this.BxMkn.margin_in_lr = 30f;
				this.BxMkn.item_margin_y_px = 2f;
				this.BxMkn.box_stencil_ref_mask = -1;
				this.BxMkn.init();
				this.BxMkn.XSh(30f);
				this.BxMkn.P(TX.Get("Smnc_title_makenew_object", ""), ALIGN.LEFT, 0f, false, 0f, "");
				this.BxMkn.Br();
				this.BxMkn.addHr(new DsnDataHr
				{
					margin_b = 4f,
					margin_t = 4f,
					draw_width_rate = 1f
				});
				this.BxMkn.Br();
				this.BxMkn.XSh(20f);
				this.BConMkn = this.BxMkn.addRadioT<aBtnNel>(new DsnDataRadio
				{
					name = "mkn",
					w = this.BxMkn.use_w - 18f,
					h = 30f,
					margin_h = 0,
					margin_w = 0,
					skin = "row",
					click_snd = "tool_pencil",
					fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangedListMkn),
					navi_loop = 2,
					all_function_same = true,
					fnHover = new FnBtnBindings(this.fnHoverMknRow),
					fnGenerateKeys = new FnGenerateRemakeKeys(this.fnGenerateKeysMkn),
					APoolEvacuated = new List<aBtn>(),
					fnMakingAfter = delegate(BtnContainer<aBtn> BCon, aBtn B)
					{
						if (!TX.isStart(B.title, "&&", 0))
						{
							B.setSkinTitle(this.Mng.OSteo[B.title].localized_name);
						}
						if (B.title == "&&Cancel")
						{
							B.click_snd = "cancel";
						}
						return true;
					},
					SCA = new ScrollAppend(11, this.BxMkn.use_w, this.BxMkn.use_h, 2f, 2f, 10)
				});
			}
			else
			{
				this.BxMkn.posSetDA(this.bx_mknx, -1000f, this.bx_slide_aim, 525f, false);
			}
			this.BxMkn.activate();
			this.BConMkn.setValue(-1, true);
			aBtn aBtn = null;
			if (this.StgoMaking.valid)
			{
				aBtn = this.BConMkn.Get(this.StgoMaking.key);
			}
			else
			{
				if (this.stgo_pre_selected >= 0 && this.stgo_pre_selected < this.CurFile.Astgo.Count)
				{
					aBtn = this.BConMkn.Get(this.CurFile.Astgo[this.stgo_pre_selected].key);
				}
				if (aBtn == null)
				{
					aBtn = this.BConMkn.Get(0);
				}
			}
			aBtn.Select(true);
			this.BConMkn.BelongScroll.reveal(aBtn, false, REVEALTYPE.ALWAYS);
		}

		private void fnGenerateKeysMkn(BtnContainerBasic BCon, List<string> Adest)
		{
			foreach (KeyValuePair<string, SmncStageEditorManager.StgObject> keyValuePair in this.Mng.OSteo)
			{
				if (!(keyValuePair.Key == "_smnc_generate_pr"))
				{
					Adest.Add(keyValuePair.Key);
				}
			}
			Adest.Add("&&Cancel");
		}

		private bool fnChangedListMkn(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (cur_value < 0)
			{
				return true;
			}
			if (this.state != SmncStageEditor.STATE.MAKENEW)
			{
				return false;
			}
			string title = _B.Get(cur_value).title;
			if (title != null && title == "&&Cancel")
			{
				this.changeState(SmncStageEditor.STATE.LIST);
				return true;
			}
			return this.CurFile == null || this.changeState(SmncStageEditor.STATE.MAKENEW_MOVE);
		}

		private bool fnHoverMknRow(aBtn B)
		{
			if (this.state != SmncStageEditor.STATE.MAKENEW)
			{
				return false;
			}
			SmncStageEditorManager.StgObject stgObject;
			if (this.Mng.OSteo.TryGetValue(B.title, out stgObject))
			{
				if (this.StgoMaking.key != stgObject.key)
				{
					this.StgoMaking = stgObject;
					this.StgoMaking.x = (int)((float)this.LpArea.mapw * 0.5f - (float)this.StgoMaking.Mp.clms * 0.5f);
					this.StgoMaking.y = (int)((float)this.LpArea.maph * 0.5f - (float)this.StgoMaking.Mp.rows * 0.5f);
					this.need_redraw_mkn = true;
				}
			}
			else
			{
				this.StgoMaking = default(SmncStageEditorManager.StgObject);
				this.need_redraw_mkn = true;
			}
			return true;
		}

		private bool isSubmitMPD()
		{
			return (IN.isSubmit() || IN.isMousePushDown(1)) && (!IN.use_mouse || !IN.getK(Key.Space, -1));
		}

		private bool isSubmitMO()
		{
			return (IN.isSubmitOn(0) || IN.isMouseOn()) && (!IN.use_mouse || !IN.getK(Key.Space, -1));
		}

		private bool runMoveState(float fcnt, SmncStageEditor.STATE back_state, bool cancelable = true)
		{
			if ((cancelable && IN.isCancel()) || !this.StgoMaking.valid)
			{
				this.changeState(back_state);
				SND.Ui.play("cancel", false);
				return false;
			}
			int num = 0;
			int num2 = 0;
			bool flag = false;
			bool flag2 = this.StgoMaking.key != "_smnc_generate_pr";
			if (flag2 && IN.isUiAddPD())
			{
				if (!this.decideMakingStgo(true, false))
				{
					SND.Ui.play("locked", false);
					if (this.t_state >= 0f)
					{
						this.t_state = -114f;
					}
				}
			}
			else if (this.isSubmitMPD() || (!flag2 && IN.isUiAddPD()) || (!cancelable && IN.isCancel()))
			{
				if (this.decideMakingStgo(false, !cancelable && IN.isCancel()))
				{
					this.changeState(SmncStageEditor.STATE.LIST);
					return true;
				}
				SND.Ui.play("locked", false);
				if (this.t_state >= 0f)
				{
					this.t_state = -114f;
				}
			}
			if (this.alloc_flip && this.StgoMaking.flipable && IN.isUiSortPD())
			{
				SND.Ui.play("tool_selrect", false);
				this.StgoMaking.flip = !this.StgoMaking.flip;
				this.need_redraw_mkn = true;
			}
			this.moveLRTB(out num, this.StgoMaking.x, this.StgoMaking.Mp.clms, out num2, this.StgoMaking.y, this.StgoMaking.Mp.rows, out flag);
			if (num != 0 || num2 != 0)
			{
				this.t_mouse = 0f;
				this.walk_aim = (int)CAim.get_aim2(0f, 0f, (float)num, (float)num2, false);
				if (flag)
				{
					if (this.t_state >= 0f)
					{
						this.t_state = -114f;
					}
					SND.Ui.play("toggle_button_limit", false);
				}
				else
				{
					SND.Ui.play("toggle_button_close", false);
					this.t_state = 0f;
					this.StgoMaking.x = this.StgoMaking.x + num;
					this.StgoMaking.y = this.StgoMaking.y + num2;
				}
			}
			if (this.walk_aim >= 0 && (this.t_state < -100f || this.t_state < 16f))
			{
				this.need_redraw_mkn = true;
			}
			if (this.checkUseMouse(fcnt))
			{
				Vector2 vector = this.M2D.Cam.Screen2UPos(this.PosMouseCur);
				int num3 = (int)(this.Mp.uxToMapx(this.M2D.effectScreenx2ux(vector.x)) - (float)this.StgoMaking.Mp.clms * 0.5f);
				int num4 = (int)(this.Mp.uyToMapy(this.M2D.effectScreeny2uy(vector.y)) - (float)this.StgoMaking.Mp.rows * 0.5f);
				num3 = X.MMX(0, num3 - this.LpArea.mapx, this.LpArea.mapw - this.StgoMaking.clms);
				num4 = X.MMX(0, num4 - this.LpArea.mapy, this.LpArea.maph - this.StgoMaking.rows);
				if (num3 != this.StgoMaking.x || num4 != this.StgoMaking.y)
				{
					this.StgoMaking.x = num3;
					this.StgoMaking.y = num4;
					this.walk_aim = -1;
					this.t_state = 100f;
					this.need_redraw_mkn = true;
				}
			}
			if (X.BTW(-100f, this.t_state, -1f))
			{
				this.t_state = 100f;
			}
			return true;
		}

		private void moveLRTB(out int walk_xd, int posx, int clms, out int walk_yd, int posy, int rows, out bool error_occured)
		{
			error_occured = false;
			walk_xd = (walk_yd = 0);
			if (IN.isL())
			{
				walk_xd = -1;
				if (posx <= 0)
				{
					error_occured = true;
				}
			}
			else if (IN.isR())
			{
				walk_xd = 1;
				if (posx + clms >= this.LpArea.mapw)
				{
					error_occured = true;
				}
			}
			if (IN.isT())
			{
				walk_yd = -1;
				if (posy <= 0)
				{
					error_occured = true;
				}
			}
			else if (IN.isB())
			{
				walk_yd = 1;
				if (posy + rows >= this.LpArea.maph)
				{
					error_occured = true;
				}
			}
			if (this.camera_move_enabled && (walk_xd != 0 || walk_yd != 0))
			{
				posx += walk_xd + this.LpArea.mapx;
				posy += walk_yd + this.LpArea.mapy;
				float scaleRev = this.M2D.Cam.getScaleRev();
				float num = (this.M2D.Cam.get_w() * 0.5f * this.Mp.rCLEN - 1.25f) * scaleRev;
				float num2 = (this.M2D.Cam.get_h() * 0.5f * this.Mp.rCLEN - 1.25f) * scaleRev;
				int num3 = (int)(this.MvCam.x - num);
				int num4 = (int)(this.MvCam.x + num);
				int num5 = (int)(this.MvCam.y - num2);
				int num6 = (int)(this.MvCam.y + num2);
				if (walk_xd < 0 && posx < num3)
				{
					this.MvCam.moveBy((float)(posx - num3), 0f, true);
				}
				if (walk_xd > 0 && posx + clms > num4)
				{
					this.MvCam.moveBy((float)(posx + clms - num4), 0f, true);
				}
				if (walk_yd < 0 && posy < num5)
				{
					this.MvCam.moveBy(0f, (float)(posy - num5), true);
				}
				if (walk_yd > 0 && posy + rows > num6)
				{
					this.MvCam.moveBy(0f, (float)(posy + rows - num6), true);
				}
			}
		}

		private void checkScrollMouse(bool only_space_carry = false)
		{
			if (!this.camera_move_enabled)
			{
				return;
			}
			float y = IN.MouseWheel.y;
			if (y != 0f)
			{
				float scale = this.M2D.Cam.getScale(false);
				if ((y > 0f && scale < this.camera_max_scale) || (y < 0f && scale > this.camera_min_scale))
				{
					this.M2D.PE.animateScaleTo(X.MMX(this.camera_min_scale, scale + 0.02f * y, this.camera_max_scale), 0);
					this.checkCamMovable();
					float num = X.MMX(this.CamMovable.xMin, this.MvCam.x, this.CamMovable.xMax);
					float num2 = X.MMX(this.CamMovable.yMin, this.MvCam.y, this.CamMovable.yMax);
					if (num != this.MvCam.x || num2 != this.MvCam.y)
					{
						this.MvCam.moveBy(num - this.MvCam.x, num2 - this.MvCam.y, true);
					}
				}
				this.t_mouse = X.Mx(this.t_mouse, 20f);
			}
			if (this.area_drag_enabled && (!only_space_carry || IN.getK(Key.Space, -1)) && !this.PosMouseCur.Equals(this.PosMousePre))
			{
				this.t_mouse = X.Mx(this.t_mouse, 20f);
				Vector2 vector = (this.PosMouseCur - this.PosMousePre) * 64f * this.Mp.rCLENB * this.M2D.Cam.getScaleRev();
				float num3 = X.MMX(this.CamMovable.xMin, this.MvCam.x - vector.x, this.CamMovable.xMax);
				float num4 = X.MMX(this.CamMovable.yMin, this.MvCam.y + vector.y, this.CamMovable.yMax);
				if (num3 != this.MvCam.x || num4 != this.MvCam.y)
				{
					this.MvCam.setTo(num3, num4);
					this.M2D.Cam.fineImmediately();
				}
			}
		}

		private bool checkUseMouse(float fcnt)
		{
			if (IN.use_mouse)
			{
				Vector2 posMouseCur = this.PosMouseCur;
				if (this.t_mouse < 20f && !posMouseCur.Equals(this.PosMousePre))
				{
					this.t_mouse += fcnt;
				}
				if (this.t_mouse >= 20f)
				{
					if (this.camera_move_enabled)
					{
						float scaleRev = this.M2D.Cam.getScaleRev();
						float num = 0f;
						float num2 = 0f;
						if (posMouseCur.x >= (IN.wh - 80f) * 0.015625f && this.MvCam.x < this.CamMovable.xMax)
						{
							if (this.MouseScroll.x >= 14f)
							{
								num = X.Mn(0.1f * scaleRev, this.CamMovable.xMax - this.MvCam.x);
							}
							else
							{
								this.MouseScroll.x = X.Mx(0f, this.MouseScroll.x) + fcnt;
							}
						}
						else if (posMouseCur.x <= (-IN.wh + 80f) * 0.015625f && this.MvCam.x > this.CamMovable.xMin)
						{
							if (this.MouseScroll.x <= -14f)
							{
								num = X.Mx(-0.1f * scaleRev, this.CamMovable.xMin - this.MvCam.x);
							}
							else
							{
								this.MouseScroll.x = X.Mn(0f, this.MouseScroll.x) - fcnt;
							}
						}
						else
						{
							this.MouseScroll.x = 0f;
						}
						if (posMouseCur.y <= (-IN.hh + 65f) * 0.015625f && this.MvCam.y > this.CamMovable.yMin)
						{
							if (this.MouseScroll.y >= 14f)
							{
								num2 = X.Mn(0.1f * scaleRev, this.CamMovable.yMax - this.MvCam.y);
							}
							else
							{
								this.MouseScroll.y = X.Mx(0f, this.MouseScroll.y) + fcnt;
							}
						}
						else if (posMouseCur.y >= (IN.hh - 65f) * 0.015625f && this.MvCam.y < this.CamMovable.yMax)
						{
							if (this.MouseScroll.y <= -14f)
							{
								num2 = X.Mx(-0.1f * scaleRev, this.CamMovable.yMin - this.MvCam.y);
							}
							else
							{
								this.MouseScroll.y = X.Mn(0f, this.MouseScroll.y) - fcnt;
							}
						}
						else
						{
							this.MouseScroll.y = 0f;
						}
						if (num != 0f || num2 != 0f)
						{
							this.MvCam.moveBy(num, num2, true);
						}
					}
					return true;
				}
			}
			else
			{
				this.t_mouse = 0f;
				this.MouseScroll = Vector2.zero;
			}
			return false;
		}

		private void checkCamMovable()
		{
			if (!this.camera_move_enabled)
			{
				return;
			}
			float scaleRev = this.M2D.Cam.getScaleRev();
			this.M2D.Cam.getScale(false);
			float num = (this.M2D.Cam.get_w() * 0.5f * this.Mp.rCLEN - 2f) * scaleRev;
			float num2 = (this.M2D.Cam.get_h() * 0.5f * this.Mp.rCLEN - 2f) * scaleRev;
			float num3 = (float)this.LpArea.mapx + num;
			float num4 = (float)this.LpArea.mapy + num2;
			float num5 = (float)(this.LpArea.mapx + this.LpArea.mapw) - num;
			float num6 = (float)(this.LpArea.mapy + this.LpArea.maph) - num2;
			this.CamMovable = new Rect(num3, num4, num5 - num3, num6 - num4);
		}

		private void finePlantAttach()
		{
			if (!this.need_consider_config)
			{
				return;
			}
			this.need_consider_config = false;
			this.Mp.considerConfig4(this.LpArea.mapx, this.LpArea.mapy, this.LpArea.mapx + this.LpArea.mapw, this.LpArea.mapy + this.LpArea.maph);
			int count = this.CurFile.Aplant.Count;
			for (int i = 0; i < count; i++)
			{
				SmncFile.PlantInfo plantInfo = this.CurFile.Aplant[i];
				SmncFile.PlantInfo plantInfo2 = plantInfo;
				int x = plantInfo.x;
				int num;
				float num2;
				this.calcPlantFooting(ref x, plantInfo.y, out num, out num2);
				plantInfo.y = (int)(num2 - 0.01f - (float)this.LpArea.mapy);
				plantInfo.attach_index = num;
				if (plantInfo.x != x || plantInfo.y != plantInfo2.y || plantInfo.attach_index != plantInfo2.attach_index)
				{
					this.CurFile.Aplant[i] = plantInfo;
					this.need_redraw_base = true;
				}
			}
			for (int j = count - 1; j >= 0; j--)
			{
				SmncFile.PlantInfo plantInfo3 = this.CurFile.Aplant[j];
				for (int k = 0; k < j; k++)
				{
					if (this.CurFile.Aplant[k].isSame(plantInfo3))
					{
						this.CurFile.Aplant.RemoveAt(j);
						this.need_redraw_base = true;
						break;
					}
				}
			}
		}

		private bool runPlantAttach(float fcnt)
		{
			if (IN.isCancel())
			{
				this.changeState(SmncStageEditor.STATE.LIST);
				SND.Ui.play("cancel", false);
				return false;
			}
			int num = 0;
			int num2 = 0;
			bool flag = false;
			this.moveLRTB(out num, (int)this.PosPlant.x, 1, out num2, (int)this.PosPlant.y, 1, out flag);
			if (num != 0 || num2 != 0)
			{
				this.t_mouse = 0f;
				this.walk_aim = (int)CAim.get_aim2(0f, 0f, (float)num, (float)num2, false);
				if (flag)
				{
					if (this.t_state >= 0f)
					{
						this.t_state = -114f;
					}
					SND.Ui.play("toggle_button_limit", false);
				}
				else
				{
					SND.Ui.play("toggle_button_close", false);
					this.t_state = 0f;
					this.PosPlant.Set(this.PosPlant.x + (float)num, this.PosPlant.y + (float)num2, -1f, -1f);
				}
			}
			if (this.checkUseMouse(fcnt))
			{
				Vector2 vector = this.M2D.Cam.Screen2UPos(this.PosMouseCur);
				int num3 = (int)this.Mp.uxToMapx(this.M2D.effectScreenx2ux(vector.x));
				int num4 = (int)this.Mp.uyToMapy(this.M2D.effectScreeny2uy(vector.y));
				num3 = X.MMX(0, num3 - this.LpArea.mapx, this.LpArea.mapw - 1);
				num4 = X.MMX(0, num4 - this.LpArea.mapy, this.LpArea.maph - 1);
				if ((float)num3 != this.PosPlant.x || (float)num4 != this.PosPlant.y)
				{
					this.PosPlant.Set((float)num3, (float)num4, -1f, -1f);
					this.walk_aim = -1;
					this.t_state = 100f;
					this.need_redraw_mkn = true;
				}
			}
			if (this.PosPlant.z == -1f)
			{
				int num5 = (int)this.PosPlant.x;
				int num6;
				float num7;
				this.calcPlantFooting(ref num5, (int)this.PosPlant.y, out num6, out num7);
				this.PosPlant.x = (float)num5;
				this.PosPlant.z = (float)num6;
				this.PosPlant.w = num7;
			}
			if (this.PosPlant.w >= (float)this.LpArea.mapy)
			{
				byte b = 0;
				if (this.isSubmitMPD())
				{
					b = 3;
					this.hold_adding = byte.MaxValue;
				}
				else if (IN.isUiAddPD() || IN.isUiSortPD())
				{
					b = 1;
				}
				else if (IN.isUiRemPD())
				{
					b = 2;
				}
				else if (this.isSubmitMO() && (this.hold_adding == 1 || this.hold_adding == 2))
				{
					b = this.hold_adding;
				}
				if (b > 0)
				{
					int count = this.CurFile.Aplant.Count;
					Vector2Int plantCurs = this.PlantCurs;
					int x = plantCurs.x;
					int y = plantCurs.y;
					bool flag2 = false;
					for (int i = 0; i < count; i++)
					{
						SmncFile.PlantInfo plantInfo = this.CurFile.Aplant[i];
						if (plantInfo.x == x && plantInfo.y == y)
						{
							flag2 = true;
							if (b == 3)
							{
								b = 2;
								if (this.hold_adding == 255)
								{
									this.hold_adding = 2;
								}
							}
							if ((b & 2) != 0)
							{
								this.CurFile.Aplant.RemoveAt(i);
								SND.Ui.play("tool_drag_quit", false);
								this.need_redraw_base = true;
								break;
							}
						}
					}
					if (!flag2 && (b & 1) != 0)
					{
						if (this.CurFile.Aplant.Count >= 200)
						{
							if (this.t_state >= 0f)
							{
								this.t_state = -15f;
								SND.Ui.play("locked", false);
							}
						}
						else
						{
							this.Mp.PtcN("general_white_circle", (float)(this.LpArea.mapx + x) + 0.5f, (float)(this.LpArea.mapy + y) + 0.5f, 0f, 40, -3);
							SND.Ui.play("tool_drag_init", false);
							List<SmncFile.PlantInfo> aplant = this.CurFile.Aplant;
							SmncFile.PlantInfo plantInfo2 = default(SmncFile.PlantInfo);
							plantInfo2.x = x;
							plantInfo2.y = y;
							SmncFile curFile = this.CurFile;
							ushort id_count = curFile.id_count;
							curFile.id_count = id_count + 1;
							plantInfo2.id = id_count;
							plantInfo2.attach_index = (int)this.PosPlant.z;
							aplant.Add(plantInfo2);
							this.need_redraw_base = true;
						}
					}
				}
				if (this.hold_adding == 255)
				{
					this.hold_adding = 0;
				}
			}
			this.need_redraw_mkn = true;
			return true;
		}

		private void calcPlantFooting(ref int x, int y, out int z_index_stgo, out float map_pos_y)
		{
			x = X.MMX(0, x, this.LpArea.mapw - 1);
			float num = (float)(x + this.LpArea.mapx) + 0.5f;
			map_pos_y = this.Mp.getFootableY(num, y + this.LpArea.mapy, this.LpArea.maph, true, -1f, true, true, false, 0f);
			map_pos_y = X.MMX((float)this.LpArea.mapy, map_pos_y, (float)(this.LpArea.mapy + this.LpArea.maph));
			z_index_stgo = -1;
			int count = this.CurFile.Astgo.Count;
			for (int i = 0; i < count; i++)
			{
				if (CCON.canFootOn(this.CurFile.Astgo[i].getConfig(this.LpArea, (int)num, (int)map_pos_y), null))
				{
					z_index_stgo = i;
					return;
				}
			}
		}

		private void prepareMesh()
		{
			if (this.MdWeb != null)
			{
				this.GobMdPut.SetActive(true);
				this.GobMdMkn.SetActive(true);
				this.GobMdWeb.SetActive(true);
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				GameObject gameObject = IN.CreateGob(this.Mp.gameObject, "-SED-" + i.ToString());
				gameObject.layer = this.Mp.M2D.Cam.getFinalSourceRenderedLayer();
				MeshDrawer meshDrawer = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.MtrMeshNormal, 0f, -1, null, true, true);
				ValotileRenderer component = gameObject.GetComponent<ValotileRenderer>();
				this.M2D.Cam.connectToBinder(component);
				if (i == 0)
				{
					this.MdPut = meshDrawer;
					this.GobMdPut = gameObject;
				}
				else if (i == 1)
				{
					this.MdMkn = meshDrawer;
					this.GobMdMkn = gameObject;
				}
				else
				{
					this.MdWeb = meshDrawer;
					this.GobMdWeb = gameObject;
				}
			}
			this.MaMkn = new MdArranger(this.MdMkn);
			float num = this.Mp.pixel2meshx(this.LpArea.x);
			float num2 = this.Mp.pixel2meshx(this.LpArea.right);
			float num3 = this.Mp.pixel2meshy(this.LpArea.y);
			float num4 = this.Mp.pixel2meshy(this.LpArea.bottom);
			float num5 = num2 - num;
			float num6 = num3 - num4;
			this.MdWeb.Col = C32.MulA(MTRX.ColWhite, 0.25f);
			for (int j = this.LpArea.mapw; j >= 0; j--)
			{
				float num7 = num + this.Mp.CLEN * (float)j;
				this.MdWeb.RectBL(num7 - 0.5f, num4, 1f, num6, false);
			}
			for (int k = this.LpArea.maph; k >= 0; k--)
			{
				float num8 = num4 + this.Mp.CLEN * (float)k;
				this.MdWeb.RectBL(num, num8 - 0.5f, num5, 1f, false);
			}
			this.MdWeb.updateForMeshRenderer(false);
			this.APlantAtl = new M2ChipImage[11];
			for (int l = 10; l >= 0; l--)
			{
				M2ChipImage m2ChipImage = this.M2D.IMGS.Get("mgplant/mgplant_" + l.ToString());
				if (m2ChipImage != null)
				{
					this.APlantAtl[l] = m2ChipImage;
				}
			}
		}

		public void redrawMkn()
		{
			this.prepareMesh();
			if (this.M2D.transferring_game_stopping)
			{
				return;
			}
			bool flag = false;
			if (this.need_redraw_base && !this.LpArea.chip_inserted)
			{
				this.need_redraw_base = false;
				flag = true;
				if (!this.MdPut.hasMultipleTriangle())
				{
					this.MdPut.activate("", this.M2D.IMGS.MIchip.getMtr(BLEND.NORMAL, -1), true, MTRX.ColWhite, null);
					this.MdPut.chooseSubMesh(1, false, true);
					this.MdPut.setMaterial(MTRX.MtrMeshNormal, false);
				}
				else
				{
					this.MdPut.clear(false, false);
				}
				this.MdPut.Identity();
				this.MdPut.chooseSubMesh(0, false, true);
				if (this.CurFile != null)
				{
					this.MdPut.Col = MTRX.ColWhite;
					int num = this.CurFile.Astgo.Count;
					for (int i = 0; i < 2; i++)
					{
						for (int j = 0; j < num; j++)
						{
							if (this.state != SmncStageEditor.STATE.MOVE || j != this.stgo_pre_selected)
							{
								SmncStageEditorManager.StgObject stgObject = this.CurFile.Astgo[j];
								if (TX.isStart(stgObject.key, "_smnc_generate_", 0) == (i == 1))
								{
									this.drawTo(this.MdPut, stgObject, 0f, 0f);
								}
							}
						}
					}
					num = this.CurFile.Aplant.Count;
					Vector2Int plantCurs = this.PlantCurs;
					for (int k = 0; k < num; k++)
					{
						SmncFile.PlantInfo plantInfo = this.CurFile.Aplant[k];
						float num2 = 1f;
						if (this.state == SmncStageEditor.STATE.SET_PLANT && plantInfo.x == plantCurs.x && plantInfo.y == plantCurs.y)
						{
							num2 = 0.6f + 0.4f * X.COSIT(40f);
							this.need_redraw_base = true;
						}
						this.drawTo(this.MdPut, plantInfo, 0f, 0f, num2);
					}
				}
			}
			bool flag2 = false;
			if (this.need_redraw_mkn)
			{
				flag2 = true;
				this.need_redraw_mkn = false;
				if (!this.MdMkn.hasMultipleTriangle())
				{
					this.MdMkn.activate("", this.M2D.IMGS.MIchip.getMtr(BLEND.NORMAL, -1), true, MTRX.ColWhite, null);
					this.MdMkn.chooseSubMesh(1, false, true);
					this.MdMkn.setMaterial(MTRX.MtrMeshDashLine, false);
					this.MdMkn.chooseSubMesh(2, false, true);
					this.MdMkn.setMaterial(MTRX.MtrMeshNormal, false);
					this.MdMkn.chooseSubMesh(3, false, true);
					this.MdMkn.setMaterial(MTRX.MtrMeshStriped, false);
					this.MdMkn.chooseSubMesh(0, false, false);
					this.MdMkn.connectRendererToTriMulti(this.GobMdMkn.GetComponent<MeshRenderer>());
				}
				else
				{
					this.MdMkn.clear(false, false);
				}
				this.MdMkn.chooseSubMesh(0, false, true);
				this.MaMkn.Set(true);
				float num3 = 0f;
				float num4 = 0f;
				if (this.walk_aim >= 0 && this.isMovingState())
				{
					if (this.t_state < 0f)
					{
						num3 = X.COSIT(4.7f) * 2.5f;
						num4 = X.COSIT(3.15f) * 2.5f;
					}
					else if (this.t_state < 7f)
					{
						float num5 = 1f - X.ZSIN2(this.t_state, 7f);
						num3 = (float)(-(float)CAim._XD(this.walk_aim, 1)) * this.CLEN * num5;
						num4 = (float)CAim._YD(this.walk_aim, 1) * this.CLEN * num5;
					}
				}
				if (this.StgoMaking.valid)
				{
					this.drawTo(this.MdMkn, this.StgoMaking, num3, num4);
				}
				this.MaMkn.Set(false);
				if (this.state == SmncStageEditor.STATE.SET_PLANT)
				{
					Vector2Int plantCursM = this.PlantCursM;
					int num6 = plantCursM.y - this.LpArea.mapy;
					this.MdMkn.Col = C32.MulA(4290772879U, 0.4f);
					this.MdMkn.chooseSubMesh(3, false, true);
					this.MdMkn.StripedM(0.7853982f, 24f, 0.5f, 4);
					this.MdMkn.RectBL(this.Mp.map2meshx((float)plantCursM.x), this.Mp.map2meshy((float)(plantCursM.y + 1)), this.CLEN, this.CLEN, false);
					this.MdMkn.allocUv2(0, true);
					this.MdMkn.chooseSubMesh(2, false, false);
					this.MdMkn.Identity();
					this.MdMkn.TranslateP(this.Mp.map2meshx((float)plantCursM.x + 0.5f) + num3, this.Mp.map2meshy(this.PosPlant.y + (float)this.LpArea.mapy + 0.5f) + num4, false);
					Matrix4x4 currentMatrix = this.MdMkn.getCurrentMatrix();
					float num7 = this.CLEN * 0.5f + 2f + 8f * X.Mx(0.25f, X.Abs(X.COSIT(40f)));
					for (int l = 1; l >= 0; l--)
					{
						this.MdMkn.Col = C32.MulA((l == 1) ? 4281811502U : 4290772879U, 1f);
						for (int m = 0; m < 4; m++)
						{
							this.MdMkn.setCurrentMatrix(currentMatrix, false);
							this.MdMkn.Rotate((float)m * 1.5707964f, true);
							this.MdMkn.Kakko(-num7 - (float)(l * 2), num7 + (float)(l * 2), (float)(10 + l * 4), (float)(10 + l * 4), (float)(2 + l * 2), false);
						}
					}
					this.MdMkn.Identity();
					if (num6 != (int)this.PosPlant.y)
					{
						this.MdMkn.Col = C32.MulA(4290772879U, 0.66f);
						this.MdMkn.chooseSubMesh(1, false, false);
						int num8 = num6 - (int)this.PosPlant.y;
						float num9 = this.Mp.map2meshy((float)plantCursM.y + 0.5f);
						if (num8 < 0)
						{
							num9 += (float)num8 * this.CLEN;
							num8 *= -1;
						}
						this.MdMkn.RectDashedMBL(this.Mp.map2meshx((float)plantCursM.x + 0.5f) - 1.5f, num9, 3f, (float)num8 * this.CLEN, num8 * 8, -1f, 0.5f, false, false);
					}
				}
				if (this.state == SmncStageEditor.STATE.LIST && X.BTW(0f, (float)this.stgo_pre_selected, (float)this.CurFile.Astgo.Count))
				{
					SmncStageEditorManager.StgObject stgObject2 = this.CurFile.Astgo[this.stgo_pre_selected];
					this.MdMkn.chooseSubMesh(1, false, false);
					this.MdMkn.Col = this.MdMkn.ColGrd.Set(4282579455U).C;
					Vector2 mapCenterPos = stgObject2.getMapCenterPos(this.LpArea);
					this.MdMkn.RectDashedM(this.Mp.map2meshx(mapCenterPos.x), this.Mp.map2meshy(mapCenterPos.y), this.CLEN * (float)stgObject2.Mp.clms + 12f, this.CLEN * (float)stgObject2.Mp.rows + 12f, (stgObject2.Mp.rows + stgObject2.Mp.clms) * 3, 3f, 0.5f, false, false);
					this.MdMkn.chooseSubMesh(0, false, false);
				}
				else if (this.isMovingState() && this.StgoMaking.valid)
				{
					this.MdMkn.chooseSubMesh(1, false, false);
					this.MdMkn.Col = this.MdMkn.ColGrd.Set(2013265919).C;
					Vector2 mapCenterPos2 = this.StgoMaking.getMapCenterPos(this.LpArea);
					this.MdMkn.RectDashedM(this.Mp.map2meshx(mapCenterPos2.x), this.Mp.map2meshy(mapCenterPos2.y), this.CLEN * (float)this.StgoMaking.Mp.clms, this.CLEN * (float)this.StgoMaking.Mp.rows, (this.StgoMaking.Mp.rows + this.StgoMaking.Mp.clms) * 10, 1f, 0.33f, false, false);
					this.MdMkn.chooseSubMesh(0, false, false);
				}
			}
			if (this.StgoMaking.valid)
			{
				flag2 = true;
				this.MdMkn.ColGrd.Set(4283926271U).blend(867171583U, 0.5f + 0.5f * X.COSIT(140f));
				if (this.isMovingState() && this.t_state < 0f)
				{
					this.MdMkn.ColGrd.blend(4294901760U, 0.4f);
				}
				this.MaMkn.setColAll(this.MdMkn.ColGrd.C, false);
			}
			if (flag2)
			{
				this.MdMkn.updateForMeshRenderer(true);
			}
			if (flag)
			{
				this.MdPut.updateForMeshRenderer(true);
			}
		}

		private bool decideMakingStgo(bool continue_put = false, bool cancel_overwrite = false)
		{
			if (!this.StgoMaking.valid)
			{
				return false;
			}
			bool flag = this.state == SmncStageEditor.STATE.MAKENEW || this.state == SmncStageEditor.STATE.MAKENEW_MOVE;
			if (flag || continue_put)
			{
				if (this.CurFile.Astgo.Count >= 200)
				{
					return false;
				}
				int priority = this.StgoMaking.priority;
				int count = this.CurFile.Astgo.Count;
				int num = -1;
				for (int i = 0; i < count; i++)
				{
					SmncStageEditorManager.StgObject stgObject = this.CurFile.Astgo[i];
					if (flag || i != this.stgo_pre_selected)
					{
						if (stgObject.isSame(this.StgoMaking))
						{
							return false;
						}
						if (priority < stgObject.priority)
						{
							num = i;
							break;
						}
					}
				}
				if (num == -1)
				{
					if (flag || !X.BTW(0f, (float)this.stgo_pre_selected, (float)this.CurFile.Astgo.Count))
					{
						num = this.CurFile.Astgo.Count;
					}
					else
					{
						int num2 = this.stgo_pre_selected;
						byte b = this.move_makenew + 1;
						this.move_makenew = b;
						num = num2 + (int)b;
					}
				}
				this.CurFile.Astgo.Insert(num, this.StgoMaking);
				if (flag)
				{
					this.stgo_pre_selected = num;
				}
				this.need_recreate_stgo_list = (this.need_consider_config = true);
				this.need_redraw_base = true;
				SND.Ui.play("tool_sel_init", false);
			}
			else if (this.state == SmncStageEditor.STATE.MOVE)
			{
				int num3 = this.CurFile.Astgo.Count;
				for (int j = 0; j < num3; j++)
				{
					if (j != this.stgo_pre_selected && this.CurFile.Astgo[j].isSame(this.StgoMaking))
					{
						if (!cancel_overwrite)
						{
							return false;
						}
						int num4 = this.stgo_pre_selected;
						if (X.BTW(0f, (float)j, (float)this.CurFile.Astgo.Count))
						{
							this.CurFile.Astgo.RemoveAt(num4);
							num3--;
							this.stgo_pre_selected = j + ((j > num4) ? (-1) : 0);
							this.need_recreate_stgo_list = true;
						}
					}
				}
				this.need_consider_config = true;
				this.CurFile.Astgo[this.stgo_pre_selected] = this.StgoMaking;
				SND.Ui.play("tool_hand_quit", false);
			}
			this.finePlantAttach();
			return true;
		}

		private void drawTo(MeshDrawer Md, SmncStageEditorManager.StgObject Stgo, float x = 0f, float y = 0f)
		{
			if (!Stgo.valid || this.Mp == null)
			{
				return;
			}
			Vector2 mapCenterPos = Stgo.getMapCenterPos(this.LpArea);
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md.TranslateP(x + this.Mp.map2meshx(mapCenterPos.x), y + this.Mp.map2meshy(mapCenterPos.y), true);
			if (Stgo.flip)
			{
				Md.Scale(-1f, 1f, true);
			}
			int count_chips = Stgo.count_chips;
			for (int i = 0; i < count_chips; i++)
			{
				M2Puts puts = Stgo.getPuts(i);
				if (puts != null)
				{
					puts.draw(Md, 1, 0f, 0f, true);
				}
			}
			Md.setCurrentMatrix(currentMatrix, false);
		}

		private void drawTo(MeshDrawer Md, SmncFile.PlantInfo Plant, float x = 0f, float y = 0f, float alpha01 = 1f)
		{
			if (this.Mp == null)
			{
				return;
			}
			Vector2 mapPos = Plant.getMapPos(this.LpArea);
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md.Col = C32.WMulA(alpha01);
			Md.TranslateP(x + this.Mp.map2meshx(mapPos.x), y + this.Mp.map2meshy(mapPos.y), true);
			M2ChipImage plantImage = this.getPlantImage(Plant);
			if (plantImage != null && plantImage.initAtlasMd(Md, 0U))
			{
				Md.RotaGraph(0f, (float)plantImage.iheight * 0.5f, 1f, 0f, null, Plant.is_flip);
			}
			Md.setCurrentMatrix(currentMatrix, false);
		}

		private void fnFinishedSaveFile(string _path, string _error)
		{
			if (_error != null && _error != "Canceled.")
			{
				SND.Ui.play("locked", false);
				X.dl(_error, null, false, true);
			}
			else if (TX.valid(_path))
			{
				SND.Ui.play("recipe_fullfill", false);
			}
			else
			{
				SND.Ui.play("cancel", false);
			}
			if (this.state == SmncStageEditor.STATE.SAVING)
			{
				this.changeState(SmncStageEditor.STATE.LIST);
			}
			this.smnc_io_lock = IN.totalframe + 5;
			this.BConL.setValue(-1, true);
		}

		public void fineKD(STB Stb)
		{
			this.need_fine_kd = false;
			switch (this.state)
			{
			case SmncStageEditor.STATE.LIST:
				if (X.BTW(0f, (float)this.stgo_pre_selected, (float)this.CurFile.Astgo.Count))
				{
					Stb.AddTxA("Smnc_stageedit_move", false).Add(" ");
				}
				Stb.AddTxA("Smnc_stageedit_add", false).Add(" ");
				Stb.Add(this.stgo_removeable ? "<key rem/>" : "<key_s rem/>").AddTxA("Smnc_stageedit_rem", false);
				if (this.enable_plant_tx_key != null)
				{
					Stb.Add(" ").AddTxA(this.enable_plant_tx_key, false);
					return;
				}
				break;
			case SmncStageEditor.STATE.SAVING:
			case SmncStageEditor.STATE.MAKENEW:
				break;
			case SmncStageEditor.STATE.MAKENEW_MOVE:
			case SmncStageEditor.STATE.MOVE:
				Stb.AddTxA("Smnc_KD_stageedit_move", false).Add(" ");
				if (this.alloc_flip && this.StgoMaking.flipable)
				{
					Stb.AddTxA("Smnc_KD_stageedit_move_flip", false);
				}
				Stb.AddTxA("Smnc_KD_put_end", false).Add(" ");
				if (this.StgoMaking.key != "_smnc_generate_pr")
				{
					Stb.AddTxA("Smnc_KD_put_continue", false).Add(" ");
				}
				Stb.AddTxA("KD_cancel", false);
				break;
			case SmncStageEditor.STATE.SET_PLANT:
				Stb.AddTxA(this.plant_set_kd_key, false).Add(" ");
				Stb.AddTxA("Smnc_KD_plant_1", false).Add(" ");
				Stb.AddTxA("Smnc_KD_plant_2", false);
				return;
			default:
				return;
			}
		}

		public M2ChipImage getPlantImage(SmncFile.PlantInfo Plant)
		{
			if (this.APlantAtl != null)
			{
				return this.APlantAtl[(int)(Plant.id % 11)];
			}
			return null;
		}

		public override string ToString()
		{
			return "StageEditor";
		}

		public Map2d Mp
		{
			get
			{
				return this.LpArea.Mp;
			}
		}

		public float CLEN
		{
			get
			{
				return this.LpArea.Mp.CLEN;
			}
		}

		public float CLENB
		{
			get
			{
				return this.LpArea.Mp.CLENB;
			}
		}

		public NelM2DBase M2D
		{
			get
			{
				return this.LpArea.nM2D;
			}
		}

		public M2MapLayer Lay
		{
			get
			{
				return this.LpArea.Lay;
			}
		}

		public bool isHeadListState()
		{
			return this.state == SmncStageEditor.STATE.LIST;
		}

		public bool isMovingState()
		{
			return this.state == SmncStageEditor.STATE.MOVE || this.state == SmncStageEditor.STATE.MAKENEW_MOVE;
		}

		public bool auto_run = true;

		public string enable_plant_tx_key;

		public bool alloc_flip = true;

		public float camera_min_scale = 0.25f;

		public float camera_max_scale = 1f;

		public bool enabled_file_export;

		public FnBtnBindings FD_ListBtnClicked;

		public string[] Alistbtn_addition;

		public string plant_set_kd_key = "Smnc_KD_plant_0";

		public const string appear_point_pr_stgo_key = "_smnc_generate_pr";

		public const string appear_point_en_stgo_key = "_smnc_generate_en";

		public const string appear_point_en_fix_stgo_key = "_smnc_generate_en_fix";

		public const string header_appear_point = "_smnc_generate_";

		public readonly SmncStageEditorManager Mng;

		public readonly M2LpUiSmnCreator LpArea;

		public readonly UiBoxDesignerFamily DsFam;

		public readonly bool original_family;

		public float t_state = -100f;

		public UiBoxDesigner BxL;

		public FillBlock FbL;

		public BtnContainerRadio<aBtn> BConL;

		public UiBoxDesigner BxMkn;

		public BtnContainerRadio<aBtn> BConMkn;

		private GameObject GobMdWeb;

		private GameObject GobMdMkn;

		private GameObject GobMdPut;

		private MeshDrawer MdWeb;

		private MeshDrawer MdMkn;

		private MeshDrawer MdPut;

		private MdArranger MaMkn;

		private const int OBJECT_MAX = 200;

		private aBtn WholeArea;

		private const float bx_listw = 340f;

		private const float bx_mknw = 420f;

		private int walk_aim = -1;

		private float t_mouse;

		private Vector2 MouseScroll;

		private Vector2 PosMousePre;

		private Vector2 PosMouseCur;

		private Rect CamMovable;

		public bool area_drag_enabled;

		public int smnc_io_lock;

		public const int MGPLANT_MAX = 11;

		public const string img_mgplant_header = "mgplant/mgplant_";

		private M2ChipImage[] APlantAtl;

		public SmncStageEditor.FnStateChangeListener FD_stateChangeListener = delegate(SmncStageEditor.STATE _stt, SmncStageEditor.STATE prestate)
		{
		};

		private SmncStageEditor.STATE state;

		private SmncFile CurFile;

		private byte file_index;

		private byte move_makenew;

		private Vector4 PosPlant;

		private bool need_consider_config;

		private bool need_recreate_stgo_list;

		private bool need_redraw_mkn;

		private bool need_redraw_base;

		public bool need_fine_kd;

		private byte hold_adding;

		public int stgo_pre_selected = -1;

		private SmncStageEditorManager.StgObject StgoMaking;

		private SmncStageEditorManager.StgObject StgoMovingPre;

		public bool stgolist_left = true;

		private readonly bool camera_move_enabled;

		private readonly M2Mover MvCam;

		private bool runner_assigned_;

		private string plant_set_btn_key;

		private const string btn_save_file_title = "Smnc_save_file";

		public delegate void FnStateChangeListener(SmncStageEditor.STATE state, SmncStageEditor.STATE prestate);

		public enum STATE
		{
			OFFLINE,
			LIST,
			SAVING,
			MAKENEW,
			MAKENEW_MOVE,
			MOVE,
			SET_PLANT
		}
	}
}
