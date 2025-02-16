using System;
using System.Collections.Generic;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel.mgm.smncr
{
	public class UiSmnCreator : IEventWaitListener, IRunAndDestroy
	{
		private float enemyw
		{
			get
			{
				return IN.w - 45f;
			}
		}

		private float enemyy
		{
			get
			{
				return -IN.hh + 40f + 10f;
			}
		}

		private float kdy
		{
			get
			{
				return this.enemyy + 30f;
			}
		}

		public UiSmnCreator(GameObject Parent, M2LpUiSmnCreator _LpArea)
		{
			this.DsFam = IN.CreateGobGUI(Parent, "-UiSmnCreator").AddComponent<UiBoxDesignerFamily>();
			IN.setZAbs(this.DsFam.gameObject.transform, -1.9000001f);
			this.DsFam.enabled = false;
			this.DsFam.auto_deactive_gameobject = false;
			this.DsFam.auto_activate = 0U;
			this.stabilize_key = "SMNC_" + IN.totalframe.ToString();
			this.LpArea = _LpArea;
			this.save_key = TX.slice(this.LpArea.key, "SmnC_".Length);
			this.M2D = this.LpArea.nM2D;
		}

		public void deactivate()
		{
			this.DsFam.auto_deactive_gameobject = true;
			IN.FlgUiUse.Rem("SMNC");
			CURS.Active.Rem("SMNC");
			CURS.Rem("NORMAL", "");
			if (this.MvCam != null)
			{
				this.Mp.M2D.Cam.assignBaseMover(this.Mp.Pr, -1);
				this.Mp.removeMover(this.MvCam);
				this.MvCam = null;
			}
			this.changeState(UiSmnCreator.STATE.OFFLINE);
		}

		public void destruct()
		{
			IN.FlgUiUse.Rem("SMNC");
			CURS.Active.Rem("SMNC");
			CURS.Rem("NORMAL", "");
			this.M2D.FlagValotStabilize.Rem(this.stabilize_key);
			this.popDangerLevel();
			IN.DestroyE(this.DsFam.gameObject);
			if (this.SED != null)
			{
				this.SED.destruct();
			}
			if (this.EnEditor != null)
			{
				this.EnEditor.destruct();
			}
			IN.remRunner(this);
			this.runner_assigned_ = false;
			this.deactivate();
		}

		private void changeState(UiSmnCreator.STATE stt)
		{
			if (stt == this.state)
			{
				return;
			}
			UiSmnCreator.STATE state = this.state;
			this.state = stt;
			this.t_state = 0f;
			if (!this.runner_assigned_)
			{
				this.runner_assigned_ = true;
				IN.addRunner(this);
			}
			bool flag = false;
			if (state == UiSmnCreator.STATE.OFFLINE)
			{
				this.LpArea.initEdit(this, out this.SmncMng);
				this.SmncMng.reload(false);
				this.min_scale = X.Mn(1f, X.Mn(this.wholeshow_w ? ((IN.w - 60f) / (this.Mp.CLENB * (float)this.LpArea.mapw)) : 1f, this.wholeshow_h ? ((IN.h - 80f) / (this.Mp.CLENB * (float)this.LpArea.maph)) : 1f));
				this.M2D.PE.animateScaleTo(this.min_scale, 40);
				this.M2D.Cam.assignBaseMover(this.MvCam, -1);
			}
			if (state == UiSmnCreator.STATE.PREPARE)
			{
				IN.FlgUiUse.Add("SMNC");
				CURS.Active.Add("SMNC");
				CURS.Set("NORMAL", "tl_cross");
				this.AFiles = COOK.Mgm.getSmncFileContainer(this.save_key, false);
				SmncFile currentFile = this.LpArea.getCurrentFile();
				if (currentFile != null && currentFile != this.CurFile && this.AFiles.IndexOf(currentFile) >= 0)
				{
					flag = true;
					this.CurFile = currentFile;
				}
				this.M2D.FlagValotStabilize.Add(this.stabilize_key);
				this.DsFam.gameObject.SetActive(true);
				this.DsFam.activate();
				UIBase.FlgLetterBoxFade.Add(this.stabilize_key);
				this.SED = new SmncStageEditor(this.LpArea, this.SmncMng, this.DsFam, this.MvCam)
				{
					auto_run = false,
					enabled_file_export = this.enabled_file_export,
					enable_plant_tx_key = "Smnc_stageedit_plant",
					camera_min_scale = this.min_scale,
					Alistbtn_addition = new string[] { "&&Smnc_KD_enemyedit_garage", "&&Smnc_KD_start_battle_garage" },
					FD_ListBtnClicked = delegate(aBtn B)
					{
						if (B.title == "&&Smnc_KD_start_battle_garage")
						{
							this.changeState(UiSmnCreator.STATE.BATTLE_CONFIRM);
						}
						else if (B.title == "&&Smnc_KD_enemyedit_garage")
						{
							this.changeState(UiSmnCreator.STATE.ENEMY_EDIT);
						}
						return false;
					}
				};
				SmncStageEditor sed = this.SED;
				sed.FD_stateChangeListener = (SmncStageEditor.FnStateChangeListener)Delegate.Combine(sed.FD_stateChangeListener, new SmncStageEditor.FnStateChangeListener(this.fnChangeSedState));
				this.popDangerLevel();
			}
			if (state == UiSmnCreator.STATE.FILESEL && this.BxFile != null)
			{
				if (this.state != UiSmnCreator.STATE.CONFIRM_FILEREM && this.state != UiSmnCreator.STATE.ERROR)
				{
					this.BxFile.deactivate();
				}
				else
				{
					this.BxFile.hide();
				}
			}
			if (state == UiSmnCreator.STATE.SCREATE)
			{
				if (this.BxFile != null)
				{
					this.BxFile.deactivate();
				}
				this.SED.deactivate();
			}
			if (state == UiSmnCreator.STATE.ENEMY_EDIT)
			{
				if (this.EnEditor != null)
				{
					this.EnEditor.deactivate();
				}
				this.fineEnemyList();
			}
			if ((state == UiSmnCreator.STATE.CONFIRM_FILEREM || state == UiSmnCreator.STATE.ERROR) && this.BxConfirm != null)
			{
				this.BxConfirm.deactivate();
			}
			if (state == UiSmnCreator.STATE.BATTLE_CONFIRM && this.BattleConfirm != null)
			{
				this.BattleConfirm.pre_state1 = (int)this.state;
				this.BattleConfirm.deactivate();
			}
			byte b = 2;
			this.need_fine_kd = true;
			if (this.state != UiSmnCreator.STATE.OFFLINE)
			{
				this.deactivated_with_summoner = 0;
			}
			switch (this.state)
			{
			case UiSmnCreator.STATE.OFFLINE:
				SND.Ui.play("cancel", false);
				this.DsFam.deactivate(false);
				this.M2D.PE.animateScaleTo(1f, 40);
				this.dialog_activated = false;
				UIBase.FlgLetterBoxFade.Rem(this.stabilize_key);
				if (!this.LpArea.chip_inserted && !this.M2D.transferring_game_stopping)
				{
					this.LpArea.insertChipsDecide();
				}
				if (this.MvCam != null)
				{
					this.Mp.M2D.Cam.assignBaseMover(this.Mp.Pr, -1);
				}
				if (this.BxKD != null)
				{
					this.BxKD.posSetDA(0f, this.kdy, 1, 90f, true);
				}
				if (this.SED != null)
				{
					this.SED.hideMesh();
				}
				break;
			case UiSmnCreator.STATE.PREPARE:
				this.need_fine_kd = false;
				break;
			case UiSmnCreator.STATE.FILESEL:
				this.createFileSel();
				this.dialog_activated = false;
				b = 1;
				break;
			case UiSmnCreator.STATE.CONFIRM_FILEREM:
				SND.Ui.play("tool_hand_init", false);
				this.createConfirmFileRem();
				b = 1;
				break;
			case UiSmnCreator.STATE.SCREATE:
				this.SED.activate(this.CurFile, (byte)this.AFiles.IndexOf(this.CurFile));
				b = 2;
				break;
			case UiSmnCreator.STATE.ENEMY_EDIT:
				if (this.EnEditor == null)
				{
					this.EnEditor = new UiSmncEnemyEditor(this, this.BxEnemy, this.DsFam, this.LpArea);
				}
				this.EnEditor.activate(this.CurFile);
				b = byte.MaxValue;
				break;
			case UiSmnCreator.STATE.BATTLE_CONFIRM:
				if (this.BattleConfirm == null)
				{
					this.BattleConfirm = new UiSmncBattleConfirm(this.DsFam, this.LpArea, this.SmncMng.target_type)
					{
						FD_BattleConfirm = new UiSmncBattleConfirm.FnBattleConfirm(this.fnSummonerInit)
					};
				}
				this.BattleConfirm.activate(this.CurFile);
				this.BattleConfirm.pre_state0 = (int)state;
				this.BattleConfirm.pre_state1 = 0;
				b = 1;
				break;
			}
			if (flag)
			{
				this.initFileSelection(this.CurFile, true);
			}
			this.fineUseEnemyBox(b);
			IN.clearPushDown(true);
		}

		public void fnChangeSedState(SmncStageEditor.STATE state, SmncStageEditor.STATE prestate)
		{
			byte b = 1;
			if (state != SmncStageEditor.STATE.OFFLINE)
			{
				if (state - SmncStageEditor.STATE.MAKENEW_MOVE <= 1)
				{
					b = 0;
				}
				this.fineUseEnemyBox(b);
				this.need_fine_kd = true;
				return;
			}
		}

		private void fineUseEnemyBox(byte use_en_box)
		{
			if (use_en_box == 1)
			{
				this.BxKD.posSetA(-1000f, -1000f, 0f, this.kdy, this.BxKD.getBox().get_deperture_y() == this.kdy);
				this.BxEnemy.activate();
			}
			if (use_en_box == 0 || use_en_box == 255)
			{
				this.BxKD.posSetA(-1000f, -1000f, 0f, this.enemyy - 10f, this.BxKD.getBox().get_deperture_y() != this.kdy);
			}
			if (use_en_box == 0)
			{
				this.BxEnemy.posSetDA(0f, this.enemyy, 1, 52f, true);
				this.BxEnemy.deactivate();
			}
		}

		public bool run(float fcnt)
		{
			bool flag = this.DsFam.runIRD(fcnt);
			switch (this.state)
			{
			case UiSmnCreator.STATE.OFFLINE:
				if (!flag)
				{
					if (this.deactivated_with_summoner == 1)
					{
						this.deactivated_with_summoner = 2;
						this.M2D.FlagValotStabilize.Rem(this.stabilize_key);
					}
					return true;
				}
				break;
			case UiSmnCreator.STATE.PREPARE:
				if (this.SmncMng.initMap())
				{
					this.changeState(UiSmnCreator.STATE.FILESEL);
				}
				break;
			case UiSmnCreator.STATE.FILESEL:
				if (this.dialog_activated)
				{
					return true;
				}
				if (IN.isMenuPD(1))
				{
					this.changeState(UiSmnCreator.STATE.BATTLE_CONFIRM);
				}
				else
				{
					if (IN.isCancelPD())
					{
						if (aBtn.PreSelected == this.BConF.Get("&&Cancel"))
						{
							this.changeState(UiSmnCreator.STATE.OFFLINE);
							break;
						}
						this.BConF.Get("&&Cancel").Select(true);
						SND.Ui.play("cancel", false);
					}
					if (IN.isUiAddPD() && !(aBtn.PreSelected == null) && !TX.isStart(aBtn.PreSelected.title, "&&", 0) && this.CurFile != null)
					{
						if (this.AFiles.Count >= 240)
						{
							SND.Ui.play("locked", false);
							CURS.limitVib(aBtn.PreSelected, AIM.R);
						}
						else
						{
							SND.Ui.play("tool_redo", false);
							int num = X.MMX(0, this.AFiles.IndexOf(this.CurFile) + 1, this.AFiles.Count);
							SmncFile smncFile = new SmncFile(this.CurFile);
							this.AFiles.Insert(num, smncFile);
							this.CurFile = smncFile;
							this.need_remake_files = true;
							this.createFileSel();
							this.initFileSelection(this.CurFile, true);
						}
					}
					if (IN.isUiRemPD() && !(aBtn.PreSelected == null) && !TX.isStart(aBtn.PreSelected.title, "&&", 0) && this.CurFile != null)
					{
						this.changeState(UiSmnCreator.STATE.CONFIRM_FILEREM);
					}
				}
				break;
			case UiSmnCreator.STATE.ERROR:
			case UiSmnCreator.STATE.CONFIRM_FILEREM:
				if (IN.isCancelPD())
				{
					((this.BxConfirm.Get("btns", false) as BtnContainerRunner).BCon as BtnContainerRadio<aBtn>).Get("&&Cancel").ExecuteOnSubmitKey();
				}
				break;
			case UiSmnCreator.STATE.SCREATE:
				if (this.SED.isHeadListState() && IN.isMenuPD(1))
				{
					this.changeState(UiSmnCreator.STATE.BATTLE_CONFIRM);
				}
				else if (!this.SED.run(fcnt))
				{
					this.changeState(UiSmnCreator.STATE.FILESEL);
				}
				else
				{
					if (this.SED.need_fine_kd)
					{
						this.need_fine_kd = true;
					}
					if (this.SED.isHeadListState() && IN.isUiSortPD())
					{
						this.changeState(UiSmnCreator.STATE.ENEMY_EDIT);
					}
				}
				break;
			case UiSmnCreator.STATE.ENEMY_EDIT:
				if (this.EnEditor.isFrontState() && IN.isMenuPD(1))
				{
					this.changeState(UiSmnCreator.STATE.BATTLE_CONFIRM);
				}
				else if (!this.EnEditor.run(fcnt))
				{
					if (this.BattleConfirm != null && this.BattleConfirm.pre_state1 == 6)
					{
						this.changeState(UiSmnCreator.STATE.BATTLE_CONFIRM);
						this.BattleConfirm.pre_state0 = 5;
					}
					else
					{
						this.changeState(UiSmnCreator.STATE.SCREATE);
					}
				}
				break;
			case UiSmnCreator.STATE.BATTLE_CONFIRM:
				if (IN.isUiSortPD())
				{
					this.changeState(UiSmnCreator.STATE.ENEMY_EDIT);
				}
				else if (!this.BattleConfirm.run(fcnt))
				{
					this.changeState((UiSmnCreator.STATE)this.BattleConfirm.pre_state0);
					if (this.state == UiSmnCreator.STATE.ENEMY_EDIT)
					{
						this.BattleConfirm.pre_state1 = 0;
					}
				}
				break;
			}
			if (this.need_fine_kd)
			{
				this.fineKD();
			}
			this.t_state += fcnt;
			return true;
		}

		public void createFileSel()
		{
			if (this.BxFile == null)
			{
				this.BxFile = this.DsFam.Create("BxFile", -IN.wh + 17f + 115f, IN.hh - 17f - 200f, 230f, 400f, 2, 255f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.BxFile.margin_in_tb = 36f;
				this.BxFile.margin_in_lr = 10f;
				this.BxFile.box_stencil_ref_mask = -1;
				this.BxFile.init();
				Designer bxFile = this.BxFile;
				DsnDataRadio dsnDataRadio = new DsnDataRadio();
				dsnDataRadio.name = "file";
				dsnDataRadio.w = this.BxFile.use_w - 18f;
				dsnDataRadio.h = 22f;
				dsnDataRadio.navi_loop = 2;
				dsnDataRadio.margin_h = 0;
				dsnDataRadio.margin_w = 0;
				dsnDataRadio.skin = "row";
				dsnDataRadio.click_snd = "enter";
				dsnDataRadio.fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangedFile);
				dsnDataRadio.all_function_same = true;
				dsnDataRadio.fnHover = new FnBtnBindings(this.fnHoverFileRow);
				dsnDataRadio.fnGenerateKeys = new FnGenerateRemakeKeys(this.fnGenerateKeysFiles);
				dsnDataRadio.fnMakingAfter = delegate(BtnContainer<aBtn> BCon, aBtn B)
				{
					if (!TX.isStart(B.title, "&&", 0))
					{
						B.setSkinTitle(TX.GetA("Smnc_ui_file", (X.NmI(B.title, 0, false, false) + 1).ToString()));
					}
					if (B.title == "Smnc_load_file")
					{
						B.setSkinTitle("<img mesh=\"directory\" tx_color/>" + TX.Get("Smnc_load_file", ""));
					}
					if (B.title == "&&Cancel")
					{
						B.click_snd = "";
					}
					return true;
				};
				dsnDataRadio.APoolEvacuated = new List<aBtn>(this.AFiles.Count);
				dsnDataRadio.SCA = new ScrollAppend(9, this.BxFile.use_w, this.BxFile.use_h, 2f, 2f, 10);
				this.BConF = bxFile.addRadioT<aBtnNel>(dsnDataRadio);
				this.BxEnemy = this.DsFam.Create("BxEnemy", 0f, this.enemyy, this.enemyw, 40f, 1, 70f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.BxEnemy.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
				this.BxEnemy.margin_in_lr = 20f;
				this.BxEnemy.margin_in_tb = 10f;
				this.BxEnemy.item_margin_x_px = 18f;
				float use_h = this.BxEnemy.use_h;
				this.BxEnemy.P(TX.Get("Smnc_title_enemy", ""), ALIGN.LEFT, -1f, false, 0f, "");
				Designer designer = this.BxEnemy.addTab("EnemyList", this.BxEnemy.use_w, use_h, this.BxEnemy.use_w, use_h, true);
				designer.Smallest();
				designer.init();
				this.FbEnemyList = designer.addP(new DsnDataP("", false)
				{
					name = "enemylist",
					text_margin_x = 0f,
					text_margin_y = 0f,
					aligny = ALIGNY.MIDDLE,
					size = 16f,
					sheight = designer.use_h,
					text = " ",
					TxCol = NEL.ColText,
					text_auto_wrap = false,
					text_auto_condense = false
				}, false);
				this.BxEnemy.endTab(true);
				this.BxKD = this.DsFam.Create("BxKD", 0f, this.kdy, this.enemyw, 30f, 1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.BxKD.getBox().frametype = UiBox.FRAMETYPE.NO_OVERRIDE;
				this.BxKD.getBox().col(MTRX.ColTrnsp);
				this.BxKD.Smallest();
				this.BxKD.margin_in_lr = 38f;
				this.BxKD.init();
				this.FbKD = this.BxKD.addP(new DsnDataP("", false)
				{
					name = "kd",
					text_margin_x = 0f,
					aligny = ALIGNY.MIDDLE,
					alignx = ALIGN.RIGHT,
					swidth = this.BxKD.use_w,
					sheight = this.BxKD.use_h,
					text = " ",
					TxCol = MTRX.ColWhite,
					html = true,
					TxBorderCol = MTRX.ColBlack,
					text_auto_wrap = false,
					text_auto_condense = false
				}, false);
				this.need_remake_files = false;
			}
			if (this.need_remake_files)
			{
				this.need_remake_files = false;
				this.BConF.RemakeT<aBtnNel>(null, "");
			}
			this.BxFile.activate();
			this.BxKD.activate();
			this.BxFile.bind();
			this.BConF.setValue(-1, false);
			if (this.CurFile != null)
			{
				this.BConF.Get(X.MMX(0, this.AFiles.IndexOf(this.CurFile), this.BConF.Length - 1)).Select(true);
				return;
			}
			int num = (int)this.AFiles.first_file;
			SmncFile currentFile = this.LpArea.getCurrentFile();
			if (currentFile != null)
			{
				num = this.AFiles.IndexOf(currentFile);
			}
			this.BConF.Get(X.MMX(0, num, this.AFiles.Count)).Select(true);
		}

		private void fnGenerateKeysFiles(BtnContainerBasic BCon, List<string> Adest)
		{
			int count = this.AFiles.Count;
			for (int i = 0; i < count; i++)
			{
				Adest.Add(i.ToString());
			}
			Adest.Add("&&Smnc_ui_files_create");
			if (this.enabled_file_export)
			{
				Adest.Add("Smnc_load_file");
			}
			Adest.Add("&&Cancel");
		}

		private bool fnChangedFile(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (cur_value < 0 || this.dialog_activated)
			{
				return true;
			}
			if (this.state != UiSmnCreator.STATE.FILESEL)
			{
				return false;
			}
			if (this.smnc_io_lock >= IN.totalframe)
			{
				return false;
			}
			aBtn aBtn = _B.Get(cur_value);
			string title = aBtn.title;
			if (title != null)
			{
				if (title == "&&Cancel")
				{
					this.changeState(UiSmnCreator.STATE.OFFLINE);
					return true;
				}
				if (!(title == "&&Smnc_ui_files_create"))
				{
					if (title == "Smnc_load_file")
					{
						if (this.AFiles.Count >= 240)
						{
							SND.Ui.play("locked", false);
							CURS.limitVib(aBtn, AIM.R);
							return false;
						}
						if (this.enabled_file_export)
						{
							this.dialog_activated = true;
							SmncFile.readFromFile(this.M2D, new Action<SmncFile, string>(this.fnFinishedLoadFile));
							return true;
						}
						return true;
					}
				}
				else
				{
					if (this.AFiles.Count >= 240)
					{
						SND.Ui.play("locked", false);
						CURS.limitVib(aBtn, AIM.R);
						return false;
					}
					SmncFile smncFile = new SmncFile(4, 4, 4, 4, 4);
					this.AFiles.Add(smncFile);
					this.need_remake_files = true;
					this.initFileSelection(smncFile, false);
					this.changeState(UiSmnCreator.STATE.SCREATE);
					return true;
				}
			}
			if (this.CurFile != null)
			{
				if ((this.CurFile.reward_flags & SmncFile.REWARD._UPDATE_ENEMIES_027) != (SmncFile.REWARD)0 && (this.CurFile.reward_flags & SmncFile.REWARD._UPDATE_ENEMIES_027_END) == (SmncFile.REWARD)0)
				{
					this.changeState(UiSmnCreator.STATE.ENEMY_EDIT);
				}
				else
				{
					this.changeState(UiSmnCreator.STATE.SCREATE);
				}
			}
			return true;
		}

		private bool fnHoverFileRow(aBtn B)
		{
			if (!TX.isStart(B.title, "&&", 0))
			{
				int num = X.NmI(B.title, -1, false, false);
				if (num >= 0)
				{
					this.initFileSelection(this.AFiles[num], false);
				}
			}
			return true;
		}

		public void initFileSelection(SmncFile File, bool force = false)
		{
			if (File == this.CurFile && !force)
			{
				return;
			}
			this.closeFileSelection();
			this.CurFile = File;
			int num = this.AFiles.IndexOf(this.CurFile);
			this.AFiles.first_file = (byte)X.Mx(0, num);
			this.fineEnemyList();
			this.SED.initFile(this.CurFile, (byte)num);
		}

		public void closeFileSelection()
		{
			if (this.CurFile == null)
			{
				return;
			}
			this.SED.quitFile(this.CurFile);
			this.CurFile = null;
		}

		private void fineEnemyList()
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				int count = this.CurFile.Aen_list.Count;
				int num = 0;
				for (int i = 0; i < count; i++)
				{
					if (i > 0)
					{
						stb.Add(", ");
					}
					SmncFile.EnemyInfo enemyInfo = this.CurFile.Aen_list[i];
					enemyInfo.copyDescTo(stb);
					num += enemyInfo.count;
				}
				stb.Add(" ").AddTxA("Smnc_ui_enemy_total_sum", false).TxRpl(num);
				this.FbEnemyList.Txt(stb);
				Designer tab = this.BxEnemy.getTab("EnemyList");
				if (tab != null)
				{
					tab.RowRemakeHeightRecalc(this.FbEnemyList, null);
					tab.getScrollBox().startAutoScroll(120);
					tab.getScrollBox().show_scroll_bar = false;
				}
			}
		}

		private void createSimpleConfirmBox(out FillBlock P, out BtnContainerRadio<aBtn> BCon)
		{
			if (this.BxConfirm == null)
			{
				this.BxConfirm = this.DsFam.Create("BxConfirm", 0f, 20f, IN.w * 0.6f, IN.h * 0.4f, 1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.BxConfirm.margin_in_tb = 56f;
				this.BxConfirm.margin_in_lr = 70f;
				this.BxConfirm.alignx = ALIGN.CENTER;
				this.BxConfirm.init();
				this.BxConfirm.addP(new DsnDataP("", false)
				{
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					text = " ",
					size = 18f,
					TxCol = NEL.ColText,
					name = "confirm_p",
					swidth = this.BxConfirm.use_w,
					sheight = this.BxConfirm.use_h * 0.7f
				}, false);
				this.BxConfirm.Br();
				this.BxConfirm.addRadioT<aBtnNel>(new DsnDataRadio
				{
					name = "btns",
					w = this.BxConfirm.use_w / 2f - 40f,
					h = 36f,
					navi_loop = 3,
					margin_h = 0,
					margin_w = 18,
					skin = "normal",
					clms = 2,
					click_snd = "",
					fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangedConfirm),
					all_function_same = true
				});
			}
			P = this.BxConfirm.Get("confirm_p", false) as FillBlock;
			BtnContainerRunner btnContainerRunner = this.BxConfirm.Get("btns", false) as BtnContainerRunner;
			BCon = btnContainerRunner.BCon as BtnContainerRadio<aBtn>;
			this.BxConfirm.activate();
		}

		private void createConfirmFileRem()
		{
			FillBlock fillBlock;
			BtnContainerRadio<aBtn> btnContainerRadio;
			this.createSimpleConfirmBox(out fillBlock, out btnContainerRadio);
			fillBlock.Txt(TX.GetA("Smnc_confirm_fileremove", (this.AFiles.IndexOf(this.CurFile) + 1).ToString()));
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				blist.Add("&&Cancel");
				blist.Add("&&Smnc_confirm_fileremove_btn");
				btnContainerRadio.RemakeLT<aBtnNel>(blist, "");
				this.BxConfirm.reboundCarrForBtnMulti(btnContainerRadio, true);
			}
			aBtn aBtn = btnContainerRadio.Get(0);
			aBtn aBtn2 = btnContainerRadio.Get(1);
			aBtn.click_snd = "cancel";
			aBtn2.click_snd = "reset_var";
			btnContainerRadio.setValue(-1, true);
			aBtn.Select(true);
		}

		private bool fnChangedConfirm(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (cur_value < 0)
			{
				return true;
			}
			if (this.state != UiSmnCreator.STATE.CONFIRM_FILEREM && this.state != UiSmnCreator.STATE.ERROR)
			{
				return false;
			}
			string title = _B.Get(cur_value).title;
			if (title != null)
			{
				if (!(title == "&&Cancel"))
				{
					if (title == "&&Smnc_confirm_fileremove_btn")
					{
						int num = this.AFiles.IndexOf(this.CurFile);
						if (num < 0)
						{
							return false;
						}
						this.AFiles.RemoveAt(num);
						this.need_remake_files = true;
						this.closeFileSelection();
						if (this.AFiles.Count > 0)
						{
							this.initFileSelection(this.AFiles[X.MMX(0, num, this.AFiles.Count - 1)], false);
						}
						this.changeState(UiSmnCreator.STATE.FILESEL);
					}
				}
				else
				{
					this.changeState(UiSmnCreator.STATE.FILESEL);
				}
			}
			return true;
		}

		private void fineKD()
		{
			this.need_fine_kd = false;
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.state == UiSmnCreator.STATE.SCREATE)
				{
					this.SED.fineKD(stb);
					if (this.SED.isHeadListState())
					{
						stb.Add(" ").AddTxA("Smnc_KD_enemyedit_garage", false);
						this.addKDStartFight(stb);
					}
				}
				else
				{
					UiSmnCreator.STATE state = this.state;
					if (state != UiSmnCreator.STATE.FILESEL)
					{
						if (state != UiSmnCreator.STATE.ENEMY_EDIT)
						{
							if (state != UiSmnCreator.STATE.BATTLE_CONFIRM)
							{
								stb.Set(" ");
							}
							else
							{
								stb.AddTxA("Smnc_KD_enemyedit_garage", false);
							}
						}
						else if (!this.EnEditor.fineKD(stb))
						{
							this.addKDStartFight(stb);
						}
					}
					else if (this.CurFile != null)
					{
						stb.AddTxA("Smnc_KD_filesel", false);
						this.addKDStartFight(stb);
					}
					else
					{
						stb.AddTxA("Smnc_KD_submit", false);
					}
				}
				this.FbKD.Txt(stb);
			}
		}

		private void addKDStartFight(STB Stb)
		{
			Stb.Add(" ").AddTxA("Smnc_KD_start_battle_garage", false);
		}

		public void hideTemporaryMesh()
		{
			if (this.SED != null)
			{
				this.SED.hideMesh();
			}
		}

		private void fnFinishedLoadFile(SmncFile _File, string _error)
		{
			if (_error != null && _error != "Canceled." && _File == null)
			{
				SND.Ui.play("locked", false);
				this.changeState(UiSmnCreator.STATE.ERROR);
				this.createConfirmErrorBox(_error);
				X.dl(_error, null, false, true);
			}
			else
			{
				if (_File != null)
				{
					SND.Ui.play("recipe_drop", false);
					this.AFiles.Add(_File);
					this.need_remake_files = true;
					this.initFileSelection(_File, false);
					this.createFileSel();
				}
				else
				{
					SND.Ui.play("cancel", false);
				}
				this.changeState(UiSmnCreator.STATE.FILESEL);
			}
			this.dialog_activated = false;
			this.smnc_io_lock = IN.totalframe + 5;
			this.BConF.setValue(-1, false);
		}

		private void createConfirmErrorBox(string _error)
		{
			FillBlock fillBlock;
			BtnContainerRadio<aBtn> btnContainerRadio;
			this.createSimpleConfirmBox(out fillBlock, out btnContainerRadio);
			fillBlock.Txt(_error);
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				blist.Add("&&Cancel");
				btnContainerRadio.RemakeLT<aBtnNel>(blist, "");
				this.BxConfirm.reboundCarrForBtnMulti(btnContainerRadio, true);
			}
			aBtn aBtn = btnContainerRadio.Get(0);
			aBtn.click_snd = "cancel";
			btnContainerRadio.setValue(-1, true);
			aBtn.Select(true);
		}

		private bool fnSummonerInit(int danger, uint weather)
		{
			EV.getVariableContainer().define("_result", "1", true);
			this.LpArea.EvtClose(true);
			this.deactivated_with_summoner = 1;
			return true;
		}

		public void popDangerLevel()
		{
			this.LpArea.popDangerLevel();
		}

		public void eventActivation(StringHolder rER)
		{
			if (this.MvCam == null)
			{
				this.MvCam = this.LpArea.Mp.createMover<M2Mover>("UiSmnCreatorCamera_" + IN.totalframe.ToString(), this.LpArea.mapfocx, this.LpArea.mapfocy, true, false);
			}
			this.changeState(UiSmnCreator.STATE.PREPARE);
			this.LpArea.ev_returnback = (TX.valid(rER._3) ? rER._3 : EV.getCurrentEvent().name);
			EV.initWaitFn(this, 0);
			EV.getVariableContainer().define("_result", "0", true);
		}

		public bool isPreparing()
		{
			return this.state == UiSmnCreator.STATE.PREPARE;
		}

		bool IEventWaitListener.EvtWait(bool is_first)
		{
			return is_first || this.state != UiSmnCreator.STATE.OFFLINE || this.M2D.transferring_game_stopping;
		}

		public M2ChipImage getPlantImage(SmncFile.PlantInfo Plant)
		{
			if (this.SED != null)
			{
				return this.SED.getPlantImage(Plant);
			}
			return null;
		}

		public override string ToString()
		{
			return "UiSmnCreator";
		}

		public Map2d Mp
		{
			get
			{
				return this.LpArea.Mp;
			}
		}

		public bool wholeshow_w = true;

		public bool wholeshow_h = true;

		public bool enabled_file_export;

		private UiBoxDesignerFamily DsFam;

		private SmncStageEditorManager SmncMng;

		public const string ev_cmd = "SMNCREATOR";

		public readonly string save_key;

		public readonly M2LpUiSmnCreator LpArea;

		public readonly NelM2DBase M2D;

		private byte deactivated_with_summoner;

		private const float filew = 230f;

		private const float fileh = 400f;

		private const float enemyh = 40f;

		private const float kdh = 30f;

		private const int FILE_MAX = 240;

		private UiBoxDesigner BxFile;

		private UiBoxDesigner BxEnemy;

		private UiBoxDesigner BxKD;

		private UiBoxDesigner BxConfirm;

		private BtnContainerRadio<aBtn> BConF;

		private FillBlock FbEnemyList;

		private FillBlock FbKD;

		private SmncFileContainer AFiles;

		private SmncFile CurFile;

		private SmncStageEditor SED;

		private UiSmncEnemyEditor EnEditor;

		private UiSmncBattleConfirm BattleConfirm;

		private float min_scale = 1f;

		private bool need_remake_files = true;

		public bool need_fine_kd;

		private float t_state = 50f;

		private UiSmnCreator.STATE state;

		private M2Mover MvCam;

		private bool dialog_activated;

		public int smnc_io_lock;

		private bool runner_assigned_;

		private readonly string stabilize_key;

		private const string btn_load_file_title = "Smnc_load_file";

		private enum STATE
		{
			OFFLINE,
			PREPARE,
			FILESEL,
			ERROR,
			CONFIRM_FILEREM,
			SCREATE,
			ENEMY_EDIT,
			BATTLE_CONFIRM
		}
	}
}
