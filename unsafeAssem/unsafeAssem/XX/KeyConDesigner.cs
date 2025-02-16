using System;
using System.Collections.Generic;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace XX
{
	public class KeyConDesigner<T> : Designer where T : aBtn
	{
		protected override void Awake()
		{
			base.Awake();
			this.InputCon = IN.ClonePlayerInput(base.gameObject, false, true);
			this.Hides = IN.CreateGob(base.gameObject, "-hide").AddComponent<HideScreenClickable>();
			IN.setZ(this.Hides.transform, -0.04f);
			this.Hides.enabled = false;
			this.radius = 0f;
			this.w = IN.w + 20f;
			this.h = IN.h + 20f;
			this.margin_in_lr = 160f;
			this.margin_in_tb = 30f;
			base.item_margin_y_px = 12f;
			this.use_button_connection = false;
			this.animate_scaling_x = (this.animate_scaling_y = false);
			this.GobCanvas = new GameObject("GUI Canvas");
			this.GobCanvas.layer = IN.LAY("UI");
			this.GobCanvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
			CURS.Omazinai();
		}

		public override void destruct()
		{
			if (this.KA != null)
			{
				this.KA.destruct();
			}
			if (this.IconMenu != null)
			{
				this.IconMenu.destruct();
				this.IconMenu = null;
			}
			if (this.GobCanvas != null)
			{
				IN.DestroyOne(this.GobCanvas);
				this.GobCanvas = null;
			}
			InputActionRebindingExtensions.RebindingOperation rebind = this.Rebind;
			if (rebind != null)
			{
				rebind.Dispose();
			}
			this.Rebind = null;
			this.MainScb = null;
			base.destruct();
		}

		protected override void destroyMe()
		{
			base.gameObject.SetActive(false);
		}

		public override Designer activate()
		{
			base.activate();
			if (this.state != KeyConDesigner<T>.STATE.NORMAL)
			{
				this.changeState(KeyConDesigner<T>.STATE.NORMAL);
			}
			if (this.KA != null)
			{
				this.KA.destruct();
			}
			this.KA = IN.cloneKeyAssign(this.InputCon);
			this.BaFirst = this.KA.getSaveString(null);
			this.CurSelector = null;
			this.CurMenuTarget = null;
			if (!this.inner_created)
			{
				this.createInner();
			}
			aBtn aBtn = base.Get((IN.isPadMode() ? "p" : "k") + "_input_" + this.KA.getInputName(0), false) as aBtn;
			aBtn aBtn2 = base.Get((IN.isPadMode() ? "p" : "k") + "_input_" + this.KA.getInputName(IN.getInputCount() - 1), false) as aBtn;
			aBtn.Select(true);
			DesignerRowMem rowManager = this.MainScb.getRowManager();
			IDesignerBlock designerBlock = base.Get("underbtn", false) as IDesignerBlock;
			List<aBtn> list = new List<aBtn>();
			designerBlock.AddSelectableItems(list, false);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				aBtn aBtn3 = list[i];
				aBtn.setNaviT(aBtn3, true, true);
				aBtn2.setNaviB(aBtn3, true, true);
			}
			rowManager.setNaviToRow(0, AIM.T, designerBlock, true, true, false).setNaviLastRow(AIM.B, designerBlock, true, true, false);
			return this;
		}

		public override Designer deactivate()
		{
			if (this.state != KeyConDesigner<T>.STATE.NORMAL)
			{
				this.changeState(KeyConDesigner<T>.STATE.NORMAL);
			}
			base.deactivate();
			if (this.IconMenu != null)
			{
				this.IconMenu.destruct();
				this.IconMenu = null;
			}
			return this;
		}

		protected override void kadomaruRedraw(float _t, bool update_mesh = true)
		{
			if (this.MdKadomaru == null || this.MainScb == null)
			{
				return;
			}
			float num = X.ZSIN(_t, (float)X.Mx(1, this.animate_maxt));
			this.MdKadomaru.Col = base.bgcol;
			this.MdKadomaru.Col.a = (byte)((float)this.MdKadomaru.Col.a * this.alpha_ * num);
			this.MdKadomaru.Box(0f, 0f, this.w, this.h, 0f, false);
			Vector3 localPosition = this.MainScb.getTransform().localPosition;
			Vector3 localPosition2 = this.Row.getTransform().localPosition;
			float num2 = 24f + this.COLUMNHEADER_H;
			this.MdKadomaru.Col = this.TextColor;
			this.MdKadomaru.Col.a = (byte)((float)this.MdKadomaru.Col.a * this.alpha_ * num);
			this.MdKadomaru.Box((localPosition.x + localPosition2.x) * 64f, (localPosition.y + localPosition2.y) * 64f + num2 / 2f, this.MainScb.get_swidth_px(), this.MainScb.get_sheight_px() + num2, 1f, false);
			if (update_mesh)
			{
				this.MdKadomaru.updateForMeshRenderer(false);
			}
		}

		public void createInner()
		{
			this.inner_created = true;
			base.addP(new DsnDataP("", false)
			{
				text = TX.Get("Keyconfig_title", ""),
				swidth = base.use_w,
				sheight = 30f,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				TxCol = this.TextColor
			}, false);
			base.Br();
			this.createConfigColumnHeader();
			float num = this.h - this.margin_in_tb * 2f - this.COLUMNHEADER_H - 110f;
			Designer designer = this.addTab("keycon_inner", base.use_w, num, base.use_w, num, true);
			designer.stencil_ref = 11;
			designer.scrolling_margin_in_lr = 24f;
			designer.scrolling_margin_in_tb = 20f;
			designer.margin_in_lr = 0f;
			designer.margin_in_tb = 0f;
			designer.item_margin_y_px = 28f;
			base.checkInit();
			this.MainScb = designer;
			designer.XSh(55f).addP(new DsnDataP("", false)
			{
				swidth = designer.use_w - 8f,
				alignx = ALIGN.CENTER,
				size = 18f,
				text = TX.Get("Keyconfig_category_ui", ""),
				sheight = 38f
			}, false);
			designer.Br();
			using (BList<string> blist = this.KA.PopInputNameArray())
			{
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					if (i == 15)
					{
						designer.XSh(55f).addP(new DsnDataP("", false)
						{
							swidth = designer.use_w - 8f,
							alignx = ALIGN.CENTER,
							size = 18f,
							text = TX.Get("Keyconfig_category_game", ""),
							sheight = 38f
						}, false);
						designer.Br();
					}
					this.createConfigRow(blist[i]);
				}
			}
			base.endTab(true);
			this.checkDuplicate(true, true);
			BtnContainer<aBtn> btnContainer = base.Br().addButtonMultiT<T>(new DsnDataButtonMulti
			{
				name = "underbtn",
				titles = new string[] { "submit", "reset", "cancel" },
				skin_title = new string[]
				{
					TX.Get("Submit", ""),
					TX.Get("Reset", "") + "...",
					TX.Get("Cancel", "")
				},
				skin = this.btn_skin,
				click_snd = "",
				navi_auto_fill = false,
				fnClick = new FnBtnBindings(this.fnClickUnderButton),
				w = this.under_button_width,
				h = this.under_button_height,
				clms = 3
			});
			this.SubmitBtn = btnContainer.Get("submit");
			this.ResetBtn = btnContainer.Get("reset");
			this.CancelTargetBtn = btnContainer.Get("cancel");
		}

		public void checkDuplicate(bool check_key = true, bool check_pad = true)
		{
			int inputCount = IN.getInputCount();
			if (check_key)
			{
				KEY.SIMKEY simkey = this.KA.hasDuplicateInput(true, false);
				for (int i = 0; i < inputCount; i++)
				{
					string inputName = IN.getInputName(i);
					KEY.SIMKEY simkey2 = ((inputName == "SUBMIT2") ? KEY.SIMKEY.SUBMIT : ((inputName == "CANCEL2") ? KEY.SIMKEY.CANCEL : FEnum<KEY.SIMKEY>.Parse(inputName, (KEY.SIMKEY)0)));
					IVariableObject variableObject = base.Get("k_dupe_" + inputName, false);
					if (variableObject != null)
					{
						variableObject.setValue(((simkey & simkey2) != (KEY.SIMKEY)0) ? "<img mesh=\"alert_triangle\" tx_color />" : "\u3000");
					}
				}
			}
			if (check_pad)
			{
				KEY.SIMKEY simkey3 = this.KA.hasDuplicateInput(false, true);
				for (int j = 0; j < inputCount; j++)
				{
					string inputName2 = IN.getInputName(j);
					KEY.SIMKEY simkey4 = ((inputName2 == "SUBMIT2") ? KEY.SIMKEY.SUBMIT : ((inputName2 == "CANCEL2") ? KEY.SIMKEY.CANCEL : FEnum<KEY.SIMKEY>.Parse(inputName2, (KEY.SIMKEY)0)));
					IVariableObject variableObject2 = base.Get("p_dupe_" + inputName2, false);
					if (variableObject2 != null)
					{
						variableObject2.setValue(((simkey3 & simkey4) != (KEY.SIMKEY)0) ? "<img mesh=\"alert_triangle\" tx_color />" : "\u3000");
					}
				}
			}
		}

		public void createConfigColumnHeader()
		{
			base.XSh(this.ROWHEADER_W + 65f);
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("Keyconfig_column_header_keyboard", false);
				stb.Add("<fiximg mesh=\"KC_keyboard\" tx_color alpha=\"0.4\" margin=\"-10\" behind=\"1\" x=\"0\" y=\"-16\"/>");
				base.addP(new DsnDataP("", false)
				{
					Stb = stb,
					swidth = 38f + this.INPUT_W + this.SELECTOR_W + base.item_margin_x_px * 2f - 20f,
					sheight = this.COLUMNHEADER_H,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					TxCol = this.TextColor,
					size = 18f,
					TxBorderCol = this.TextBorderColor,
					html = true
				}, false);
				stb.Clear();
				stb.AddTxA("Keyconfig_column_header_keyboard", false);
				stb.Add("<fiximg mesh=\"KC_pad\" tx_color alpha=\"0.4\" margin=\"-10\" behind=\"1\" x=\"0\" y=\"-16\"/>");
				base.XSh(this.KEY_AND_PAD_MARGIN + 0f);
				base.addP(new DsnDataP("", false)
				{
					Stb = stb,
					swidth = 38f + this.INPUT_W + this.SELECTOR_W + base.item_margin_x_px,
					sheight = this.COLUMNHEADER_H,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					TxCol = this.TextColor,
					size = 18f,
					TxBorderCol = this.TextBorderColor,
					html = true
				}, false);
			}
			base.Br();
		}

		public void createConfigRow(string tx_key)
		{
			this.createConfigRowName(tx_key);
			T t = this.createConfigRowSelectorKeyboard(tx_key);
			base.XSh(this.KEY_AND_PAD_MARGIN);
			T t2 = this.createConfigRowSelectorPad(tx_key);
			if (t != null && t2 != null)
			{
				t.setNaviR(t2, true, true);
			}
			if (t != null && t2 == null && this.PreBtn_Pad_Input != null)
			{
				t.setNaviR(this.PreBtn_Pad_Input, false, true);
			}
			base.Br();
		}

		public void createConfigRowName(string tx_key)
		{
			if (tx_key == "SUBMIT2" || tx_key == "CANCEL2")
			{
				base.XSh(this.ROWHEADER_W + base.item_margin_x_px + 20f);
				return;
			}
			base.addP(new DsnDataP("", false)
			{
				text = TX.Get("Keyconfig_name_" + tx_key.ToLower(), ""),
				swidth = this.ROWHEADER_W,
				sheight = 40f,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				TxCol = this.TextColor
			}, false);
			base.XSh(30f);
		}

		public T createConfigRowSelectorKeyboard(string tx_key)
		{
			base.addP(new DsnDataP("", false)
			{
				name = "k_dupe_" + tx_key,
				TxCol = this.ColDupe,
				TxBorderCol = C32.d2c(4278190080U),
				html = true,
				text = "\u3000",
				text_margin_x = 0f,
				text_margin_y = 0f,
				swidth = 38f,
				sheight = 40f,
				aligny = ALIGNY.MIDDLE,
				alignx = ALIGN.RIGHT
			}, false);
			T t = this.addButtonT<T>(new DsnDataButton
			{
				name = "k_input_" + tx_key,
				title = tx_key,
				w = this.INPUT_W,
				h = 40f,
				skin = this.inputname_skin,
				skin_title = this.SkinTitle(this.KA.getKeyboardRawInputTitle(tx_key)),
				click_snd = "",
				hover_to_select = true,
				navi_auto_fill = false,
				fnClick = new FnBtnBindings(this.fnInitConfigKeyboard),
				z_push_click = true
			});
			if (this.PreBtn_Kb_Input != null)
			{
				this.PreBtn_Kb_Input.setNaviB(t, true, true);
			}
			this.PreBtn_Kb_Input = t;
			Designer designer = this.CurTab.addTab("iconlabel_k_" + tx_key, this.SELECTOR_W, 40f, this.SELECTOR_W, 40f, false).Small();
			designer.margin_in_lr = (designer.margin_in_tb = 0f);
			designer.item_margin_x_px = 0f;
			designer.item_margin_y_px = 2f;
			base.XSh(this.SELECTOR_W - this.SELECTOR_LABEL_W - 1f);
			T t2 = this.addButtonT<T>(new DsnDataButton
			{
				name = "k_label_" + tx_key,
				title = tx_key,
				w = this.SELECTOR_LABEL_W,
				h = 18f,
				skin = this.label_skin,
				skin_title = this.SkinTitle(this.KA.getKeyboardIconLabel(tx_key)),
				click_snd = "",
				hover_to_select = true,
				navi_auto_fill = false,
				fnClick = new FnBtnBindings(this.fnInitKeyboardLabel),
				z_push_click = true
			});
			if (this.PreBtn_Kb_Label != null)
			{
				this.PreBtn_Kb_Label.setNaviB(t2, true, true);
			}
			this.PreBtn_Kb_Label = t2;
			if (this.KA.isNoIconInput(tx_key))
			{
				t.setNaviR(t2, true, true);
				this.CurTab.endTab(true);
				return t2;
			}
			T t3 = base.Br().addButtonT<T>(new DsnDataButton
			{
				name = "k_ico_" + tx_key,
				title = tx_key,
				w = this.SELECTOR_ICON_W,
				h = 18f,
				skin = this.icon_skin,
				skin_title = this.KA.getKeyboardIconNum(tx_key).ToString(),
				click_snd = "",
				hover_to_select = true,
				navi_auto_fill = false,
				fnClick = new FnBtnBindings(this.fnInitKeyboardIcon),
				z_push_click = true
			});
			t.setNaviR(t3, true, true);
			t3.setNaviR(t2, true, true);
			if (this.PreBtn_Kb_Icon != null)
			{
				this.PreBtn_Kb_Icon.setNaviB(t3, true, true);
			}
			this.PreBtn_Kb_Icon = t3;
			this.CurTab.endTab(true);
			return t2;
		}

		public T createConfigRowSelectorPad(string tx_key)
		{
			base.addP(new DsnDataP("", false)
			{
				name = "p_dupe_" + tx_key,
				TxCol = this.ColDupe,
				TxBorderCol = C32.d2c(4278190080U),
				text = "\u3000",
				html = true,
				swidth = 38f,
				sheight = 40f,
				aligny = ALIGNY.MIDDLE,
				alignx = ALIGN.RIGHT
			}, false);
			T t = this.addButtonT<T>(new DsnDataButton
			{
				name = "p_input_" + tx_key,
				title = tx_key,
				w = this.INPUT_W,
				h = 40f,
				skin = this.inputname_skin,
				skin_title = this.KA.getPadRawInputTitle(tx_key),
				click_snd = "",
				hover_to_select = true,
				navi_auto_fill = false,
				fnClick = new FnBtnBindings(this.fnInitConfigPad),
				z_push_click = true
			});
			if (this.PreBtn_Pad_Input != null)
			{
				this.PreBtn_Pad_Input.setNaviB(t, true, true);
			}
			this.PreBtn_Pad_Input = t;
			Designer designer = this.CurTab.addTab("iconlabel_p_" + tx_key, this.SELECTOR_W, 40f, this.SELECTOR_W, 40f, false).Small();
			designer.margin_in_lr = (designer.margin_in_tb = 0f);
			designer.item_margin_x_px = 0f;
			designer.item_margin_y_px = 2f;
			base.XSh(this.SELECTOR_W - this.SELECTOR_LABEL_W - 1f);
			T t2 = this.addButtonT<T>(new DsnDataButton
			{
				name = "p_label_" + tx_key,
				title = tx_key,
				w = this.SELECTOR_LABEL_W,
				h = 18f,
				skin = this.label_skin,
				skin_title = this.SkinTitle(this.KA.getPadIconLabel(tx_key)),
				click_snd = "",
				hover_to_select = true,
				navi_auto_fill = false,
				fnClick = new FnBtnBindings(this.fnInitPadLabel),
				z_push_click = true
			});
			if (this.PreBtn_Pad_Label != null)
			{
				this.PreBtn_Pad_Label.setNaviB(t2, true, true);
			}
			this.PreBtn_Pad_Label = t2;
			if (this.KA.isNoIconInput(tx_key))
			{
				t.setNaviR(t2, true, true);
				this.CurTab.endTab(true);
				return t;
			}
			T t3 = base.Br().addButtonT<T>(new DsnDataButton
			{
				name = "p_ico_" + tx_key,
				title = tx_key,
				w = this.SELECTOR_ICON_W,
				h = 18f,
				skin = this.icon_skin,
				skin_title = this.KA.getPadIconNum(tx_key).ToString(),
				click_snd = "",
				hover_to_select = true,
				navi_auto_fill = false,
				fnClick = new FnBtnBindings(this.fnInitPadIcon),
				z_push_click = true
			});
			t.setNaviR(t3, true, true);
			t3.setNaviR(t2, true, true);
			if (this.PreBtn_Pad_Icon != null)
			{
				this.PreBtn_Pad_Icon.setNaviB(t3, true, true);
			}
			this.PreBtn_Pad_Icon = t3;
			this.CurTab.endTab(true);
			return t;
		}

		protected string SkinTitle(string s)
		{
			if (s != null && s.Length != 0)
			{
				return s;
			}
			return " ";
		}

		protected virtual void changeState(KeyConDesigner<T>.STATE _st)
		{
			if (this.state == KeyConDesigner<T>.STATE.KEYCON || this.state == KeyConDesigner<T>.STATE.PADCON)
			{
				if (this.CurSelector != null)
				{
					if (_st == KeyConDesigner<T>.STATE.NORMAL)
					{
						this.CurSelector.Select(true);
					}
					this.CurSelector.SetChecked(false, true);
					this.CurSelector = null;
				}
				InputActionRebindingExtensions.RebindingOperation rebind = this.Rebind;
				this.Rebind = null;
				if (rebind != null)
				{
					rebind.Cancel();
				}
				if (rebind != null)
				{
					rebind.Dispose();
				}
				this.padinput_hold_axis = (this.padinput_hold_t = 0f);
				IN.holdArrowInput();
			}
			if (this.state == KeyConDesigner<T>.STATE.ICONMENU_KB || this.state == KeyConDesigner<T>.STATE.ICONMENU_PAD)
			{
				if (this.CurMenuTarget != null)
				{
					if (_st == KeyConDesigner<T>.STATE.NORMAL)
					{
						this.CurMenuTarget.Select(true);
					}
					this.CurMenuTarget.SetChecked(false, true);
					this.CurMenuTarget = null;
				}
				if (this.IconMenu != null && this.IconMenu.isActive())
				{
					this.IconMenu.hide(false, false);
				}
			}
			if (this.state == KeyConDesigner<T>.STATE.NORMAL && _st != KeyConDesigner<T>.STATE.NORMAL)
			{
				base.hide();
			}
			else if (this.state != KeyConDesigner<T>.STATE.NORMAL && _st == KeyConDesigner<T>.STATE.NORMAL)
			{
				base.bind();
			}
			if (this.state == KeyConDesigner<T>.STATE.RESET_PROMPT && _st != KeyConDesigner<T>.STATE.RESET_PROMPT)
			{
				if (this.DsnResetPrompt != null)
				{
					this.DsnResetPrompt.deactivate();
				}
				this.ResetBtn.Select(true);
			}
			else if (this.state != KeyConDesigner<T>.STATE.RESET_PROMPT && _st == KeyConDesigner<T>.STATE.RESET_PROMPT)
			{
				this.DsnResetPrompt = this.createResetPrompt(null);
			}
			this.state = _st;
			if (this.state == KeyConDesigner<T>.STATE.PADCON)
			{
				this.padcon_set_t = 0;
			}
			this.state_t = -1f;
			this.cannot_default_cancel = true;
			IN.clearPushDown(false);
			if (_st == KeyConDesigner<T>.STATE.NORMAL)
			{
				this.Hides.enabled = false;
			}
		}

		protected virtual bool fnInitConfigKeyboard(aBtn B)
		{
			if (this.state == KeyConDesigner<T>.STATE.KEYCON || this.state_t < 20f)
			{
				return false;
			}
			if (this.state != KeyConDesigner<T>.STATE.NORMAL)
			{
				this.changeState(this.state);
			}
			this.changeState(KeyConDesigner<T>.STATE.KEYCON);
			SND.Ui.play("tool_hand_init", false);
			this.CurSelector = B;
			B.SetChecked(true, true);
			B.Deselect(true);
			return true;
		}

		protected bool fnInitKeyboardIcon(aBtn B)
		{
			if (this.state == KeyConDesigner<T>.STATE.ICONMENU_KB)
			{
				return true;
			}
			this.createIconMenu();
			this.CurMenuTarget = B;
			this.CurMenuTarget.SetChecked(true, true);
			int keyboardIconNum = this.KA.getKeyboardIconNum(B.title);
			this.IconMenu.setValue((1 << this.KA.getKeyboardIconNum(B.title)).ToString());
			this.IconMenu.showBottom(B, null);
			this.changeState(KeyConDesigner<T>.STATE.ICONMENU_KB);
			this.IconMenu.Get(X.MMX(0, keyboardIconNum, this.IconMenu.Length - 1)).Select(true);
			return true;
		}

		public void FnCompleteRebind(string name, bool is_pad, bool is_canceled)
		{
			if (this.Rebind == null)
			{
				return;
			}
			if (!is_canceled)
			{
				this.inputNameReset(this.CurSelector.title, !is_pad, is_pad, true);
				SND.Ui.play("value_assign", false);
				this.checkDuplicate(!is_pad, is_pad);
			}
			else
			{
				SND.Ui.play("tool_hand_quit", false);
			}
			this.changeState(KeyConDesigner<T>.STATE.NORMAL);
			this.KA.quitRebinding(name);
		}

		protected virtual void createIconMenu()
		{
			if (this.IconMenu == null)
			{
				this.IconMenu = new BtnMenu<T>("icon_menu", 300f, 20f, 0);
				this.IconMenu.clms = 6;
			}
			if (this.IconMenu.Length == 0)
			{
				this.IconMenu.snd_decide = "value_assign";
				this.IconMenu.navi_loop = 3;
				this.IconMenu.Make("0", this.iconmenu_skin);
				PxlFrame sqFImgKCIcon = MTRX.SqFImgKCIcon;
				for (int i = 0; i < sqFImgKCIcon.countLayers(); i++)
				{
					this.IconMenu.Make((i + 1).ToString(), this.iconmenu_skin);
				}
				this.IconMenu.addSelectedFn(new BtnMenu<T>.FnMenuSelectedBindings(this.fnSelectedIconMenu));
			}
		}

		protected virtual bool fnInitKeyboardLabel(aBtn B)
		{
			return true;
		}

		protected virtual bool fnInitConfigPad(aBtn B)
		{
			if (this.state == KeyConDesigner<T>.STATE.PADCON || this.state_t < 20f)
			{
				return false;
			}
			if (this.state != KeyConDesigner<T>.STATE.NORMAL)
			{
				this.changeState(this.state);
			}
			this.changeState(KeyConDesigner<T>.STATE.PADCON);
			SND.Ui.play("tool_hand_init", false);
			this.CurSelector = B;
			this.padinput_hold_t = 0f;
			this.padinput_hold_axis = 0f;
			B.SetChecked(true, true);
			B.Deselect(true);
			return true;
		}

		protected virtual void padInputHoldUpdated(float _hold_t, float _holdaxis)
		{
			this.padinput_hold_t = _hold_t;
			this.padinput_hold_axis = _holdaxis;
		}

		protected bool fnInitPadIcon(aBtn B)
		{
			if (this.state == KeyConDesigner<T>.STATE.ICONMENU_PAD)
			{
				return true;
			}
			this.createIconMenu();
			this.CurMenuTarget = B;
			this.CurMenuTarget.SetChecked(true, true);
			int padIconNum = this.KA.getPadIconNum(B.title);
			this.IconMenu.setValue((1 << padIconNum).ToString());
			this.IconMenu.showBottom(B, null);
			this.changeState(KeyConDesigner<T>.STATE.ICONMENU_PAD);
			this.IconMenu.Get(X.MMX(0, padIconNum, this.IconMenu.Length - 1)).Select(true);
			return true;
		}

		protected virtual bool fnInitPadLabel(aBtn B)
		{
			return true;
		}

		public virtual Designer createResetPrompt(Designer Dsn = null)
		{
			if (Dsn == null)
			{
				Dsn = new GameObject("_ResetPrompt").AddComponent<Designer>();
				Dsn.Small();
				Dsn.WH(600f, 400f);
			}
			Dsn.item_margin_x_px = 0f;
			Dsn.item_margin_y_px = 0f;
			Dsn.margin_in_lr = 40f;
			Dsn.margin_in_tb = 30f;
			Dsn.activate();
			Dsn.Clear();
			Dsn.alignx = ALIGN.CENTER;
			Dsn.init();
			float num = Dsn.use_w * 0.5f - 50f;
			float num2 = Dsn.use_h - 62f - 46f;
			Designer designer2;
			Designer designer = (designer2 = Dsn.addTab("TabK", num, num2, num, num2, false));
			designer.Small();
			designer.item_margin_y_px = 12f;
			designer.margin_in_lr = (designer.margin_in_tb = 0f);
			Dsn.addP(new DsnDataP("", false)
			{
				text = TX.Get("Keyconfig_title_header_keyboard", ""),
				size = 20f,
				swidth = num,
				sheight = 80f,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				TxCol = this.TextColorPrompt,
				html = true
			}, false);
			Dsn.Br();
			BtnContainerRadio<aBtn> btnContainerRadio = KeyConDesigner<T>.resetPromptForReset(designer, this.KA, this.radio_skin, this.checkbox_skin, false, true);
			Dsn.endTab(true);
			Dsn.addHr(new DsnDataHr
			{
				draw_width_rate = 0.8f,
				swidth = num2,
				Col = C32.MulA(this.TextColorPrompt, 0.6f),
				margin_t = 24f,
				margin_b = 25f,
				line_height = 1f,
				vertical = true
			});
			Designer designer4;
			Designer designer3 = (designer4 = Dsn.addTab("TabP", num, num2, num, num2, false));
			designer3.Small();
			designer3.item_margin_y_px = 12f;
			designer3.margin_in_lr = (designer3.margin_in_tb = 0f);
			Dsn.addP(new DsnDataP("", false)
			{
				text = TX.Get("Keyconfig_title_header_pad", ""),
				size = 20f,
				swidth = num,
				sheight = 80f,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				TxCol = this.TextColorPrompt,
				html = true
			}, false);
			Dsn.Br();
			BtnContainerRadio<aBtn> btnContainerRadio2 = KeyConDesigner<T>.resetPromptForReset(designer3, this.KA, this.radio_skin, this.checkbox_skin, true, true);
			Dsn.endTab(true);
			Dsn.addHr(new DsnDataHr
			{
				line_height = 1f,
				margin_t = 30f,
				margin_b = 31f
			});
			BtnContainer<aBtn> btnContainer = Dsn.addButtonMultiT<T>(new DsnDataButtonMulti
			{
				navi_loop = 1,
				clms = 2,
				margin_w = 30f,
				w = 240f,
				h = 30f,
				skin = this.btn_skin_prompt,
				name = "reset_submit",
				titles = new string[] { "&&Reset", "&&Cancel" },
				fnClick = new FnBtnBindings(this.fnClickResetPrompt)
			});
			btnContainer.Get(0).click_snd = "reset_var";
			aBtn btn = designer2.getBtn("key_reset_label");
			aBtn btn2 = designer4.getBtn("pad_reset_label");
			btn.setNaviR(btn2, true, true).setNaviR(btn, true, true);
			btn.setNaviB(btnContainer.Get(0), true, true);
			btn.setNaviR(btn2, true, true).setNaviL(btn2, true, true);
			btnContainer.Get(0).setNaviB(btnContainerRadio.Get(0), true, true);
			btn2.setNaviB(btnContainer.Get(1), true, true);
			btnContainer.Get(1).setNaviB(btnContainerRadio2.Get(0), true, true);
			for (int i = 0; i < 3; i++)
			{
				aBtn aBtn = btnContainerRadio.Get(X.Mn(btnContainerRadio.Length - 1, i));
				aBtn aBtn2 = btnContainerRadio2.Get(X.Mn(btnContainerRadio2.Length - 1, i));
				aBtn.setNaviR(aBtn2, true, i < 2);
				aBtn.setNaviL(aBtn2, true, i < 2);
			}
			(IN.isPadMode() ? btnContainerRadio2 : btnContainerRadio).Get(0).Select(true);
			return Dsn;
		}

		public static BtnContainerRadio<aBtn> resetPromptForReset(Designer Dsn, KEY KA, string radio_skin, string check_skin, bool for_pad, bool add_reset_label = true)
		{
			DsnDataRadio dsnDataRadio = new DsnDataRadio
			{
				name = (for_pad ? "pad_reset" : "key_reset"),
				skin = radio_skin,
				margin_h = 0,
				margin_w = 0,
				w = Dsn.use_w,
				h = 30f,
				clms = 1
			};
			if (for_pad)
			{
				if (IN.connected_hid_device != null)
				{
					dsnDataRadio.keys = new string[] { "&&KeyCon_Reset_Pad_XI", "&&KeyCon_Reset_Pad_DI", "&&KeyCon_No_Reset" };
				}
				else
				{
					dsnDataRadio.keys = new string[] { "&&KeyCon_Reset_Pad_XI", "&&KeyCon_No_Reset" };
				}
			}
			else
			{
				dsnDataRadio.keys = new string[] { "&&Reset", "&&KeyCon_No_Reset" };
			}
			int num;
			if (for_pad != IN.isPadMode())
			{
				num = dsnDataRadio.keys.Length - 1;
			}
			else
			{
				num = ((dsnDataRadio.keys.Length == 3 && !KA.is_xinput) ? 1 : 0);
			}
			dsnDataRadio.def = num;
			BtnContainerRadio<aBtn> btnContainerRadio = Dsn.addRadioT<T>(dsnDataRadio);
			if (dsnDataRadio.keys.Length < 3)
			{
				Dsn.addHr(new DsnDataHr().H(30f - Dsn.item_margin_y_px));
			}
			if (add_reset_label)
			{
				Dsn.Br();
				DsnDataButton dsnDataButton = new DsnDataButton();
				dsnDataButton.name = (for_pad ? "pad_reset_label" : "key_reset_label");
				dsnDataButton.title = "&&KeyCon_Reset_Label";
				dsnDataButton.skin = check_skin;
				dsnDataButton.w = Dsn.use_w;
				dsnDataButton.h = 28f;
				dsnDataButton.fnClick = delegate(aBtn V)
				{
					V.SetChecked(!V.isChecked(), true);
					return true;
				};
				Dsn.addButtonT<T>(dsnDataButton).setNaviT(btnContainerRadio.Get(btnContainerRadio.Length - 1), true, false);
			}
			return btnContainerRadio;
		}

		private bool fnClickResetPrompt(aBtn B)
		{
			if (this.DsnResetPrompt == null)
			{
				return true;
			}
			if (B.title == "&&Reset")
			{
				Designer tab = this.DsnResetPrompt.getTab("TabK");
				Designer tab2 = this.DsnResetPrompt.getTab("TabP");
				if (tab != null && tab2 != null)
				{
					bool flag = false;
					bool flag2 = false;
					if (X.NmI(tab.getValue("key_reset"), -1, true, false) == 0)
					{
						flag = true;
					}
					BtnContainerRunner btnContainerRunner = tab2.Get("pad_reset", false) as BtnContainerRunner;
					int num = X.NmI(tab2.getValue("pad_reset"), -1, true, false);
					if (num == 0)
					{
						this.KA.is_xinput = true;
						flag2 = true;
					}
					if (num == 1 && btnContainerRunner.BCon.Length == 3)
					{
						this.KA.is_xinput = false;
						flag2 = true;
					}
					bool flag3 = this.KA.clearDefaultLabel(X.NmI(tab.getValue("key_reset_label"), 0, true, false) != 0, X.NmI(tab2.getValue("pad_reset_label"), 0, true, false) != 0);
					if (flag || flag2)
					{
						flag3 = true;
						this.KA.clearKEY(flag, flag2);
						this.checkDuplicate(flag, flag2);
					}
					if (flag3)
					{
						int inputCount = IN.getInputCount();
						for (int i = 0; i < inputCount; i++)
						{
							this.inputNameReset(IN.getInputName(i), true, true, false);
						}
					}
					this.resetPromptAfter(flag, flag2, flag3);
				}
			}
			this.changeState(KeyConDesigner<T>.STATE.NORMAL);
			return true;
		}

		protected virtual void resetPromptAfter(bool reset_keyboard, bool reset_pad, bool resetted)
		{
		}

		public override bool run(float fcnt)
		{
			if (!this.inner_created)
			{
				this.activate();
			}
			base.run(fcnt);
			if (this.state == KeyConDesigner<T>.STATE.ICONMENU_KB || this.state == KeyConDesigner<T>.STATE.ICONMENU_PAD)
			{
				if (this.CurMenuTarget == null)
				{
					this.changeState(KeyConDesigner<T>.STATE.NORMAL);
				}
				else if (!this.IconMenu.isActive())
				{
					this.changeState(KeyConDesigner<T>.STATE.NORMAL);
				}
			}
			if (this.state == KeyConDesigner<T>.STATE.RESET_PROMPT)
			{
				if (this.DsnResetPrompt == null)
				{
					this.changeState(KeyConDesigner<T>.STATE.NORMAL);
				}
				else
				{
					BtnContainerRunner btnContainerRunner = this.DsnResetPrompt.Get("reset_submit", false) as BtnContainerRunner;
					if (btnContainerRunner == null)
					{
						this.changeState(KeyConDesigner<T>.STATE.NORMAL);
					}
					else if (IN.isCancel())
					{
						aBtn aBtn = btnContainerRunner.Get(0);
						if (aBtn.isSelected())
						{
							this.changeState(KeyConDesigner<T>.STATE.NORMAL);
						}
						else
						{
							SND.Ui.play("cancel", false);
							aBtn.Select(true);
						}
					}
				}
			}
			if (this.state == KeyConDesigner<T>.STATE.KEYCON)
			{
				if (this.CurSelector == null)
				{
					this.changeState(KeyConDesigner<T>.STATE.NORMAL);
				}
				else
				{
					this.CurSelector.runPushingBlink(false);
				}
				if (this.state_t >= 3f)
				{
					if (this.Rebind == null)
					{
						this.Rebind = this.KA.initRebinding(this.CurSelector.title, false, new KEY.FnRebindListener(this.FnCompleteRebind));
					}
					if (IN.isCancelOn(4) || IN.isSubmitOn(4))
					{
						this.changeState(KeyConDesigner<T>.STATE.NORMAL);
					}
				}
				else if (IN.isCancelOn(0) || IN.isSubmitOn(0))
				{
					this.state_t = 0f;
				}
			}
			if (this.state == KeyConDesigner<T>.STATE.PADCON)
			{
				if (this.CurSelector == null)
				{
					this.changeState(KeyConDesigner<T>.STATE.NORMAL);
				}
				else
				{
					this.CurSelector.runPushingBlink(false);
					if (this.state_t >= 5f)
					{
						if (this.Rebind == null)
						{
							this.Rebind = this.KA.initRebinding(this.CurSelector.title, true, new KEY.FnRebindListener(this.FnCompleteRebind));
						}
						if (IN.isCancelOn(4) || IN.isSubmitOn(4))
						{
							this.changeState(KeyConDesigner<T>.STATE.NORMAL);
						}
					}
					else if (IN.isCancelOn(0) || IN.isSubmitOn(0))
					{
						this.state_t = 0f;
					}
				}
			}
			this.state_t += fcnt;
			return true;
		}

		protected override void runDefaultCancel()
		{
			if (this.cannot_default_cancel && !IN.isCancelOn(0))
			{
				this.cannot_default_cancel = false;
			}
			if (this.state == KeyConDesigner<T>.STATE.NORMAL && !this.cannot_default_cancel)
			{
				base.runDefaultCancel();
			}
		}

		protected void inputNameResetAll(bool keyboard = true, bool pad = true, bool assign_changed_manual = false)
		{
			int inputCount = IN.getInputCount();
			for (int i = 0; i < inputCount; i++)
			{
				this.inputNameReset(IN.getInputName(i), keyboard, pad, assign_changed_manual);
			}
		}

		protected virtual void inputNameReset(string title, bool keyboard = true, bool pad = true, bool assign_changed_manual = false)
		{
			if (keyboard)
			{
				(base.Get("k_input_" + title, false) as aBtn).setSkinTitle(this.KA.getKeyboardRawInputTitle(title));
				(base.Get("k_label_" + title, false) as aBtn).setSkinTitle(this.KA.getKeyboardIconLabel(title));
				aBtn aBtn = base.Get("k_ico_" + title, false) as aBtn;
				if (aBtn != null)
				{
					aBtn.setSkinTitle(this.KA.getKeyboardIconNum(title).ToString());
				}
			}
			if (pad)
			{
				aBtn aBtn = base.Get("p_input_" + title, false) as aBtn;
				if (aBtn == null)
				{
					return;
				}
				aBtn.setSkinTitle(this.KA.getPadRawInputTitle(title));
				(base.Get("p_label_" + title, false) as aBtn).setSkinTitle(this.KA.getPadIconLabel(title));
				aBtn = base.Get("p_ico_" + title, false) as aBtn;
				if (aBtn != null)
				{
					aBtn.setSkinTitle(this.KA.getPadIconNum(title).ToString());
				}
			}
		}

		private bool fnSelectedIconMenu(BtnMenu<T> Menu, int selected, string selected_title)
		{
			if (this.CurMenuTarget != null)
			{
				string title = this.CurMenuTarget.title;
				if (this.state == KeyConDesigner<T>.STATE.ICONMENU_PAD)
				{
					this.KA.setPadIcon(title, X.NmI(selected_title, 0, false, false));
					this.inputNameResetAll(false, true, false);
					(base.Get("p_ico_" + title, false) as aBtn).setSkinTitle(selected_title);
				}
				else if (this.state == KeyConDesigner<T>.STATE.ICONMENU_KB)
				{
					this.KA.setKeyboardIcon(title, X.NmI(selected_title, 0, false, false));
					this.inputNameResetAll(true, false, false);
					(base.Get("k_ico_" + title, false) as aBtn).setSkinTitle(selected_title);
				}
				this.changeState(KeyConDesigner<T>.STATE.NORMAL);
			}
			return true;
		}

		protected virtual bool fnClickUnderButton(aBtn B)
		{
			if (this.state != KeyConDesigner<T>.STATE.NORMAL)
			{
				this.changeState(this.state);
				return true;
			}
			string title = B.title;
			if (title != null)
			{
				if (!(title == "submit"))
				{
					if (!(title == "cancel"))
					{
						if (title == "reset")
						{
							SND.Ui.play("enter_small", false);
							this.changeState(KeyConDesigner<T>.STATE.RESET_PROMPT);
						}
					}
					else
					{
						if (!base.isActive())
						{
							return true;
						}
						SND.Ui.play("cancel", false);
						this.deactivate();
					}
				}
				else
				{
					SND.Ui.play("enter_small", false);
					this.submitWhole(true);
					B.Deselect(true);
				}
			}
			return true;
		}

		protected virtual void submitWhole(bool execute = false)
		{
			IN.submitKeyAssign(this.KA, execute);
			if (execute)
			{
				this.deactivate();
			}
		}

		private bool inner_created;

		private PlayerInput InputCon;

		public Color32 TextColor = MTRX.ColWhite;

		public Color32 TextColorPrompt = MTRX.ColWhite;

		public Color32 TextBorderColor = MTRX.ColBlack;

		public Color32 ColDupe = C32.d2c(4292507337U);

		public float COLUMNHEADER_H = 50f;

		public float ROWHEADER_W = 160f;

		public float INPUT_W = 140f;

		public float SELECTOR_W = 140f;

		public float SELECTOR_LABEL_W = 90f;

		public float SELECTOR_ICON_W = 70f;

		public float KEY_AND_PAD_MARGIN = 22f;

		public const float LEFT_TX_W = 38f;

		public const float ROW_H = 40f;

		public const float ROW_LABEL = 18f;

		public const float ROW_ICON = 18f;

		public float under_button_width = 140f;

		public float under_button_height = 24f;

		public string inputname_skin = "";

		public string label_skin = "";

		public string icon_skin = "";

		public string btn_skin = "";

		public string btn_skin_prompt = "";

		public string radio_skin = "radio_string";

		public string checkbox_skin = "checkbox_string";

		public string iconmenu_skin = "";

		public int padcon_set_t;

		private InputActionRebindingExtensions.RebindingOperation Rebind;

		public bool cannot_default_cancel;

		protected KEY KA;

		protected ByteArray BaFirst;

		protected Designer MainScb;

		protected Designer DsnResetPrompt;

		protected float padinput_hold_t;

		protected float padinput_hold_axis;

		protected GameObject GobCanvas;

		private float state_t;

		protected KeyConDesigner<T>.STATE state;

		protected aBtn CurSelector;

		protected BtnMenu<T> IconMenu;

		private aBtn CurMenuTarget;

		protected HideScreenClickable Hides;

		private T PreBtn_Kb_Input;

		private T PreBtn_Kb_Label;

		private T PreBtn_Kb_Icon;

		private T PreBtn_Pad_Input;

		private T PreBtn_Pad_Label;

		private T PreBtn_Pad_Icon;

		protected aBtn SubmitBtn;

		protected aBtn ResetBtn;

		protected enum STATE
		{
			NORMAL,
			KEYCON,
			PADCON,
			KEYLABEL,
			PADLABEL,
			RESET_PROMPT,
			ICONMENU_KB,
			ICONMENU_PAD,
			CONFIRM_CHECK
		}
	}
}
