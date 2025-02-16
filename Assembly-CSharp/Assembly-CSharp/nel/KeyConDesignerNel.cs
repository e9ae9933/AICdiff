using System;
using UnityEngine;
using UnityEngine.InputSystem;
using XX;

namespace nel
{
	public sealed class KeyConDesignerNel : KeyConDesigner<aBtnNel>
	{
		protected override void Awake()
		{
			base.Awake();
			this.Fam = base.gameObject.AddComponent<UiBoxDesignerFamily>();
			this.Fam.base_z = -0.1f;
			this.Fam.auto_deactive_gameobject = false;
			this.TextColor = C32.d2c(4293321691U);
			this.TextColorPrompt = C32.d2c(4283780170U);
			this.TextBorderColor = C32.d2c(3142929482U);
			this.inputname_skin = "keycon_input";
			this.icon_skin = "keycon_icon";
			this.label_skin = "keycon_label";
			this.btn_skin = "normal_dark";
			this.btn_skin_prompt = "normal";
			this.checkbox_skin = "checkbox";
			this.radio_skin = "radio";
			this.iconmenu_skin = "keycon_iconmenu";
			this.scroll_border_color = 4291611332U;
			this.under_button_width = 190f;
			this.under_button_height = 32f;
			if (!MGV.pad_checked)
			{
				this.padinput_updated = 1;
			}
		}

		protected override void createIconMenu()
		{
			if (this.IconMenu == null)
			{
				this.IconMenu = new BtnMenu<aBtnNel>("icon_menu", 50f, 50f, MTRX.SqFImgKCIcon.countLayers() + 1);
				this.IconMenu.clms = 6;
				this.IconMenu.set_navi = true;
			}
			base.createIconMenu();
		}

		public override Designer activate()
		{
			base.activate();
			this.result = KeyConDesignerNel.RESULT.NOTYET;
			this.CurLabelTarget = null;
			return this;
		}

		public override Designer deactivate()
		{
			base.deactivate();
			this.Fam.deactivate(false);
			this.CurLabelTarget = null;
			this.Hides.enabled = false;
			if (this.result == KeyConDesignerNel.RESULT.NOTYET)
			{
				this.result = KeyConDesignerNel.RESULT.CANCEL;
			}
			if (this.result == KeyConDesignerNel.RESULT.SUCCESS && this.padinput_updated == 2)
			{
				this.padinput_updated = 3;
				MGV.pad_checked = true;
				X.dl("Padinput Updated", null, false, false);
				MGV.saveSdFile(null);
			}
			NEL.stopPressingSound("KEYCON");
			return this;
		}

		public override void destruct()
		{
			NEL.stopPressingSound("KEYCON");
			base.destruct();
		}

		protected override void changeState(KeyConDesigner<aBtnNel>.STATE _st)
		{
			if ((this.state == KeyConDesigner<aBtnNel>.STATE.KEYCON || _st == KeyConDesigner<aBtnNel>.STATE.NORMAL) && this.UbInputKb != null && this.UbInputKb.isActive())
			{
				this.UbInputKb.deactivate();
			}
			if ((this.state == KeyConDesigner<aBtnNel>.STATE.PADCON || _st == KeyConDesigner<aBtnNel>.STATE.NORMAL) && this.UbInputPad != null && this.UbInputPad.isActive())
			{
				this.UbInputPad.deactivate();
				NEL.RemSubmitHoldSnd("_KeyCon");
			}
			if ((this.state == KeyConDesigner<aBtnNel>.STATE.KEYLABEL || _st == KeyConDesigner<aBtnNel>.STATE.NORMAL) && this.UbLabelKb != null && this.UbLabelKb.isActive())
			{
				this.UbLabelKb.deactivate();
			}
			if ((this.state == KeyConDesigner<aBtnNel>.STATE.PADLABEL || _st == KeyConDesigner<aBtnNel>.STATE.NORMAL) && this.UbLabelPad != null && this.UbLabelPad.isActive())
			{
				this.UbLabelPad.deactivate();
			}
			if (this.state == KeyConDesigner<aBtnNel>.STATE.CONFIRM_CHECK)
			{
				if (this.BaFirst != null)
				{
					this.BaFirst.position = 0UL;
					IN.submitKeyAssign(this.BaFirst, false);
					SND.Ui.play("close_ui", false);
					this.SubmitBtn.Select(false);
				}
				NEL.stopPressingSound("KEYCON");
				if (this.UbConfirm != null)
				{
					int inputCount = IN.getInputCount();
					for (int i = 0; i < inputCount; i++)
					{
						string inputName = IN.getInputName(i);
						FillBlock fillBlock = this.UbConfirm.Get("inputcheck_" + inputName, false) as FillBlock;
						if (fillBlock != null)
						{
							fillBlock.use_valotile = false;
						}
					}
					this.UbConfirm.deactivate();
				}
				this.ConfirmSkin = null;
			}
			if (_st == KeyConDesigner<aBtnNel>.STATE.NORMAL && this.CurLabelTarget != null)
			{
				this.CurLabelTarget.SetChecked(false, true);
				this.CurLabelTarget.Select(false);
				this.CurLabelTarget = null;
			}
			base.changeState(_st);
		}

		protected override bool fnInitConfigKeyboard(aBtn B)
		{
			if (!base.fnInitConfigKeyboard(B))
			{
				return false;
			}
			this.createInputBox(ref this.UbInputKb, false, B);
			return true;
		}

		protected override bool fnInitConfigPad(aBtn B)
		{
			if (!base.fnInitConfigPad(B))
			{
				return false;
			}
			this.createInputBox(ref this.UbInputPad, true, B);
			return true;
		}

		private UiBoxDesigner createInputBox(ref UiBoxDesigner Ub, bool is_pad, IDesignerBlock ShowTo)
		{
			if (Ub == null)
			{
				Ub = this.Fam.Create("_input_" + (is_pad ? "pad" : "kb"), 0f, 0f, 460f, 150f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
				Ub.Small();
				Ub.use_button_connection = false;
				Ub.margin_in_tb = 20f;
				Ub.item_margin_y_px = 19f;
				Ub.alignx = ALIGN.CENTER;
				Ub.addImg(new DsnDataImg
				{
					name = "p",
					MI = MTRX.MIicon,
					text = TX.Get("Keyconfig_box_input_" + (is_pad ? "pad" : "kb"), ""),
					alignx = ALIGN.CENTER,
					swidth = Ub.use_w,
					sheight = 65f,
					size = 18f,
					html = true,
					TxCol = C32.d2c(4283780170U),
					TxBorderCol = C32.d2c(4293321691U),
					FnDrawInFIB = delegate(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
					{
						return this.redrawInputFI(Md, FI, alpha, ref update_meshdrawer, is_pad);
					}
				});
				Ub.Br();
				Ub.addButton(new DsnDataButton
				{
					w = 180f,
					h = 26f,
					title = "cancel",
					click_snd = "",
					skin_title = TX.Get("Cancel", ""),
					unselectable = 2,
					fnClick = new FnBtnBindings(this.fnClickInputtingDialog)
				});
			}
			IN.setZ(Ub.transform, -0.15f);
			Vector3 vector = Designer.showBottom(Ub, ShowTo, ALIGN.CENTER, false);
			int num = 3;
			if (vector.y < -0.9375f)
			{
				vector = Designer.showTop(Ub, ShowTo, ALIGN.CENTER, false);
				num = 1;
			}
			else
			{
				vector.y -= 0.0625f;
			}
			vector = Designer.ScreenInner(Ub, vector, false);
			Ub.positionD(vector.x * 64f, vector.y * 64f, num, 40f);
			Ub.activate();
			this.Hides.ClearRects().IgnoreRect(Ub, vector.x, vector.y, 10f).activate();
			UiBoxDesigner.FocusTo(Ub);
			return Ub;
		}

		private bool redrawInputFI(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer, bool is_pad)
		{
			update_meshdrawer = true;
			if (!Md.hasMultipleTriangle())
			{
				Md.chooseSubMesh(1, false, true);
				Md.setMaterial(MTRX.MtrMeshNormal, false);
				Md.chooseSubMesh(0, false, false);
				Md.connectRendererToTriMulti(FI.getMeshRenderer());
			}
			Md.chooseSubMesh(0, false, true);
			Md.Col = C32.MulA(4283780170U, ((this.padinput_hold_axis > 0f || this.padinput_hold_t > 0f) ? 0.13f : 0.35f) * alpha);
			Md.RotaPF(0f, -16f, 1f, 1f, 0f, MTRX.getPF((!is_pad) ? "KC_keyboard" : "KC_pad"), false, false, false, uint.MaxValue, false, 0);
			Md.chooseSubMesh(1, false, true);
			Md.Col = C32.MulA(4283780170U, alpha);
			if (this.padinput_hold_axis > 0f)
			{
				float num = FI.get_swidth_px() * 0.18f;
				Md.Poly(num, 0f, 38f, 0f, 40, 1f, false, 0f, 0f);
				Md.Poly(num, 0f, 36f * this.padinput_hold_axis, 0f, 40, 0f, false, 0f, 0f);
			}
			if (this.padinput_hold_t > 0f)
			{
				float num2 = -FI.get_swidth_px() * 0.18f;
				float num3 = 90f;
				float num4 = 22f;
				Md.Box(num2, -20f, num3, num4, 1f, false);
				Md.RectBL(num2 - num3 * 0.5f, -20f - num4 * 0.5f, num3 * this.padinput_hold_t, num4, false);
			}
			if (is_pad)
			{
				if (this.padinput_hold_t > 0f)
				{
					FI.text_content = TX.Get("Keyconfig_box_input_pad_hold", "") + "\u3000\u3000\u3000\u3000\u3000\n";
					FI.text_alpha = 1f;
				}
				else
				{
					FI.text_content = TX.Get("Keyconfig_box_input_pad", "");
					FI.text_alpha = 1f;
				}
			}
			return true;
		}

		private bool fnClickInputtingDialog(aBtn B)
		{
			if (B.title == "cancel")
			{
				this.changeState(KeyConDesigner<aBtnNel>.STATE.NORMAL);
				SND.Ui.play("tool_hand_quit", false);
			}
			if (B.title == "submit")
			{
				this.labelAssignInput();
			}
			return true;
		}

		private void labelAssignInput()
		{
			if (this.state == KeyConDesigner<aBtnNel>.STATE.KEYLABEL && this.CurLabelTarget != null)
			{
				string text = TX.trim(this.UbLabelKb.getValue("label_field"));
				this.KA.setKeyboardLabel(this.CurLabelTarget.title, text);
				base.inputNameResetAll(true, false, false);
			}
			if (this.state == KeyConDesigner<aBtnNel>.STATE.PADLABEL && this.CurLabelTarget != null)
			{
				string text2 = TX.trim(this.UbLabelPad.getValue("label_field"));
				this.KA.setPadLabel(this.CurLabelTarget.title, text2);
				base.inputNameResetAll(false, true, false);
			}
			SND.Ui.play("value_assign", false);
			this.changeState(KeyConDesigner<aBtnNel>.STATE.NORMAL);
		}

		protected override bool fnInitKeyboardLabel(aBtn B)
		{
			if (this.state == KeyConDesigner<aBtnNel>.STATE.KEYLABEL)
			{
				return true;
			}
			Designer designer = this.createLabelBox(ref this.UbLabelKb, false, B);
			this.CurLabelTarget = B;
			LabeledInputField labeledInputField = designer.Get("label_field", false) as LabeledInputField;
			labeledInputField.setValue(this.KA.getKeyboardIconLabel(B.title));
			SND.Ui.play("tool_hand_init", false);
			B.SetChecked(true, true);
			B.Deselect(true);
			this.changeState(KeyConDesigner<aBtnNel>.STATE.KEYLABEL);
			labeledInputField.SelectAndFocus();
			return true;
		}

		protected override bool fnInitPadLabel(aBtn B)
		{
			if (this.state == KeyConDesigner<aBtnNel>.STATE.PADLABEL)
			{
				return true;
			}
			Designer designer = this.createLabelBox(ref this.UbLabelPad, false, B);
			this.CurLabelTarget = B;
			LabeledInputField labeledInputField = designer.Get("label_field", false) as LabeledInputField;
			labeledInputField.setValue(this.KA.getPadIconLabel(B.title), true, false);
			SND.Ui.play("tool_hand_init", false);
			B.SetChecked(true, true);
			B.Deselect(true);
			this.changeState(KeyConDesigner<aBtnNel>.STATE.PADLABEL);
			labeledInputField.SelectAndFocus();
			return true;
		}

		protected override void inputNameReset(string title, bool keyboard = true, bool pad = true, bool assign_changed_manual = false)
		{
			if (pad && assign_changed_manual && this.padinput_updated == 1)
			{
				this.padinput_updated = 2;
			}
			base.inputNameReset(title, keyboard, pad, assign_changed_manual);
		}

		private UiBoxDesigner createLabelBox(ref UiBoxDesigner Ub, bool is_pad, IDesignerBlock ShowTo)
		{
			if (Ub == null)
			{
				Ub = this.Fam.Create("_label_" + (is_pad ? "pad" : "kb"), 0f, 0f, 560f, 200f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
				Ub.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
				Ub.Small();
				Ub.use_button_connection = false;
				Ub.margin_in_tb = 27f;
				Ub.margin_in_lr = 40f;
				Ub.item_margin_y_px = 4f;
				Ub.P(TX.Get("Keyconfig_label_field_caption", ""), ALIGN.LEFT, Ub.use_w, false, 0f, "");
				Ub.Br();
				aBtn Bclear = null;
				FnFldBindings fnFldBindings = delegate(LabeledInputField LI)
				{
					Bclear.setSkinTitle((LI.text != "") ? "×" : "Def.");
					return true;
				};
				LabeledInputField labeledInputField = Ub.addInput(new DsnDataInput
				{
					name = "label_field",
					bounds_w = Ub.use_w - 120f,
					navi_auto_fill = false,
					alloc_empty = true,
					return_blur = false,
					fnChangedDelay = fnFldBindings,
					size = 20
				});
				Bclear = Ub.addButtonT<aBtnNel>(new DsnDataButton
				{
					title = "×",
					click_snd = "reset_var",
					fnClick = new FnBtnBindings(this.fnClickAddString),
					w = 100f,
					h = 24f,
					navi_auto_fill = false
				});
				Ub.Br().addHr(new DsnDataHr().H(10f));
				BtnContainer<aBtn> btnContainer = Ub.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
				{
					titles = new string[] { "←", "↓", "↑", "→", "\uff3f", "◯", "☓", "□", "△" },
					w = 30f,
					h = 30f,
					clms = 5,
					click_snd = "tool_sel_init",
					navi_loop = 0,
					fnClick = new FnBtnBindings(this.fnClickAddString),
					margin_w = 10f,
					margin_h = 10f,
					navi_auto_fill = false
				});
				BtnContainer<aBtn> btnContainer2 = Ub.XSh(30f).addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
				{
					titles = new string[] { "submit", "cancel" },
					name = "label_submit",
					skin_title = new string[]
					{
						TX.Get("Submit", ""),
						TX.Get("Cancel", "")
					},
					click_snd = "",
					fnClick = new FnBtnBindings(this.fnClickInputtingDialog),
					w = 120f,
					h = 24f,
					margin_w = 5f,
					margin_h = 0f,
					navi_auto_fill = false
				});
				Ub.getRowManager().setNaviLastRow(AIM.T, labeledInputField, true, true, false).setNaviLastRow(AIM.B, labeledInputField, true, true, false);
				labeledInputField.setNaviR(Bclear, true, true);
				labeledInputField.setNaviB(btnContainer2.Get(0), true, true);
				Bclear.setNaviB(btnContainer2.Get(1), true, true);
				btnContainer.Get(8).setNaviR(btnContainer2.Get(0), false, true);
				btnContainer.Get(4).setNaviR(btnContainer2.Get(0), true, true);
				btnContainer.Get(0).setNaviL(btnContainer2.Get(btnContainer2.Length - 1), true, true);
				btnContainer.Get(5).setNaviL(btnContainer2.Get(btnContainer2.Length - 1), false, true);
			}
			Vector3 vector = Designer.showBottom(Ub, ShowTo, ALIGN.CENTER, false);
			int num = 3;
			if (vector.y < -0.9375f)
			{
				vector = Designer.showTop(Ub, ShowTo, ALIGN.CENTER, false);
				num = 1;
			}
			else
			{
				vector.y -= 0.0625f;
			}
			vector = Designer.ScreenInner(Ub, vector, false);
			Ub.positionD(vector.x * 64f, vector.y * 64f, num, 40f);
			Ub.activate();
			this.Hides.ClearRects().IgnoreRect(Ub, vector.x, vector.y, 10f).activate();
			IN.clearPushDown(false);
			UiBoxDesigner.FocusTo(Ub);
			return Ub;
		}

		private bool fnClickAddString(aBtn B)
		{
			LabeledInputField labeledInputField = null;
			if (this.state == KeyConDesigner<aBtnNel>.STATE.KEYLABEL)
			{
				labeledInputField = this.UbLabelKb.Get("label_field", false) as LabeledInputField;
			}
			if (this.state == KeyConDesigner<aBtnNel>.STATE.PADLABEL)
			{
				labeledInputField = this.UbLabelPad.Get("label_field", false) as LabeledInputField;
			}
			if (labeledInputField != null)
			{
				if (B.title == "×")
				{
					if (B.get_Skin().title == "Def.")
					{
						if (this.state == KeyConDesigner<aBtnNel>.STATE.KEYLABEL && this.CurLabelTarget != null)
						{
							labeledInputField.setValue(this.KA.getKbDisplayLabelDefault(this.CurLabelTarget.title), true, false);
						}
						if (this.state == KeyConDesigner<aBtnNel>.STATE.PADLABEL && this.CurLabelTarget != null)
						{
							labeledInputField.setValue(this.KA.getPadDisplayLabelDefault(this.CurLabelTarget.title), true, false);
						}
					}
					else
					{
						labeledInputField.setValue("", true, false);
					}
				}
				else
				{
					labeledInputField.setValue(B.title, true, false);
				}
			}
			return true;
		}

		public override bool run(float fcnt)
		{
			switch (this.state)
			{
			case KeyConDesigner<aBtnNel>.STATE.KEYCON:
			case KeyConDesigner<aBtnNel>.STATE.PADCON:
				if (!this.Hides.isActive())
				{
					SND.Ui.play("tool_hand_quit", false);
					this.changeState(KeyConDesigner<aBtnNel>.STATE.NORMAL);
				}
				break;
			case KeyConDesigner<aBtnNel>.STATE.KEYLABEL:
			case KeyConDesigner<aBtnNel>.STATE.PADLABEL:
				if (!this.Hides.isActive())
				{
					SND.Ui.play("cancel", false);
					this.changeState(KeyConDesigner<aBtnNel>.STATE.NORMAL);
				}
				else if (IN.isCancelPD())
				{
					if (LabeledInputField.CurFocused != null)
					{
						LabeledInputField.CurFocused.Blur();
					}
					else
					{
						aBtn aBtn = (((this.state == KeyConDesigner<aBtnNel>.STATE.KEYLABEL) ? this.UbLabelKb : this.UbLabelPad).Get("label_submit", false) as BtnContainerRunner).Get(0);
						if (aBtn.isSelected())
						{
							SND.Ui.play("tool_hand_quit", false);
							this.labelAssignInput();
						}
						else
						{
							aBtn.Select(false);
						}
					}
				}
				break;
			case KeyConDesigner<aBtnNel>.STATE.CONFIRM_CHECK:
				if (!this.Hides.isActive())
				{
					SND.Ui.play("tool_hand_quit", false);
					this.changeState(KeyConDesigner<aBtnNel>.STATE.NORMAL);
				}
				else
				{
					PlayerInput playerCon = IN.getCurrentKeyAssignObject().PlayerCon;
					if (playerCon.currentControlScheme.IndexOf("Keyboard") >= 0)
					{
						IN.getCurrentKeyAssignObject().pad_mode = false;
					}
					else if (playerCon.currentControlScheme != "Pen")
					{
						IN.getCurrentKeyAssignObject().pad_mode = true;
					}
					int inputCount = IN.getInputCount();
					for (int i = 0; i < inputCount; i++)
					{
						string inputName = IN.getInputName(i);
						int keyInputByName = IN.getKeyInputByName(inputName);
						if (keyInputByName != -1000)
						{
							if (keyInputByName > 0)
							{
								FillBlock fillBlock = this.UbConfirm.Get("inputcheck_" + inputName, false) as FillBlock;
								if (!(fillBlock == null))
								{
									if (((ulong)this.confirm_bits & (ulong)(1L << (i & 31))) == 0UL)
									{
										SND.Ui.play("tool_eraser_init", false);
									}
									this.confirm_bits |= 1U << i;
									fillBlock.setAlpha(0.55f + 0.2f * X.COSI((float)(i + 10 + IN.totalframe), 14f));
								}
							}
							else if (((ulong)this.confirm_bits & (ulong)(1L << (i & 31))) != 0UL && keyInputByName <= 0)
							{
								this.confirm_bits &= ~(1U << i);
								FillBlock fillBlock2 = this.UbConfirm.Get("inputcheck_" + inputName, false) as FillBlock;
								if (!(fillBlock2 == null))
								{
									SND.Ui.play("tool_eraser_quit", false);
									fillBlock2.setAlpha(1f);
								}
							}
						}
					}
					if (this.ConfirmSkin != null)
					{
						if (NEL.confirmHold("KEYCON", ref this.confirm_cancel_hold_time, 40, this.ConfirmCancelSkin, true, IN.isCancelOn(0) ? 1 : 0, false))
						{
							this.ConfirmCancelSkin.getBtn().SetChecked(true, true);
							this.changeState(KeyConDesigner<aBtnNel>.STATE.NORMAL);
						}
						else if (NEL.confirmHold("KEYCON", ref this.confirm_hold_time, 100, this.ConfirmSkin, false, IN.isSubmitOn(0) ? 1 : 0, true))
						{
							SND.Ui.play("saved", false);
							this.BaFirst = null;
							base.submitWhole(true);
							CFG.saveKeyConfigFileOnly();
							this.result = KeyConDesignerNel.RESULT.SUCCESS;
						}
					}
					else if (IN.isCancelPD())
					{
						this.ConfirmCancelSkin.getBtn().SetChecked(true, true);
						this.changeState(KeyConDesigner<aBtnNel>.STATE.NORMAL);
					}
				}
				break;
			}
			return base.run(fcnt);
		}

		protected override void padInputHoldUpdated(float _hold_t, float _holdaxis)
		{
			if (this.padinput_hold_t != _hold_t)
			{
				bool flag = this.padinput_hold_t == 0f;
				bool flag2 = _hold_t == 0f;
				if (flag != flag2)
				{
					if (flag)
					{
						NEL.AddSubmitHoldSnd("_KeyCon");
					}
					else
					{
						NEL.RemSubmitHoldSnd("_KeyCon");
					}
				}
			}
			FillImageBlock fillImageBlock = this.UbInputPad.Get("p", false) as FillImageBlock;
			if (fillImageBlock != null)
			{
				fillImageBlock.redraw_flag = true;
			}
			base.padInputHoldUpdated(_hold_t, _holdaxis);
		}

		public override Designer createResetPrompt(Designer Dsn = null)
		{
			if (this.UbResetPrompt == null)
			{
				(this.UbResetPrompt = this.Fam.Create("_reset", 0f, 0f, 700f, 400f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX)).Small();
			}
			return base.createResetPrompt(this.UbResetPrompt);
		}

		protected override void resetPromptAfter(bool reset_keyboard, bool reset_pad, bool resetted)
		{
			if (reset_pad && this.padinput_updated == 1)
			{
				this.padinput_updated = 2;
			}
			base.resetPromptAfter(reset_keyboard, reset_pad, resetted);
		}

		protected override void submitWhole(bool execute = false)
		{
			if (this.state == KeyConDesigner<aBtnNel>.STATE.CONFIRM_CHECK)
			{
				return;
			}
			base.submitWhole(false);
			this.confirm_dupe = this.KA.hasDuplicateInput(true, true);
			MTRX.OFontStorage[MTRX.getCabinFont()].Fine();
			bool flag = this.confirm_dupe > (KEY.SIMKEY)0;
			if (this.UbConfirm == null)
			{
				MTRX.OFontStorage[MTRX.getCabinFont()].Fine();
				UiBoxDesigner uiBoxDesigner = (this.UbConfirm = this.Fam.Create("_confirm", 0f, 0f, 960f, 650f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX));
				uiBoxDesigner.alignx = ALIGN.CENTER;
				uiBoxDesigner.margin_in_tb = 32f;
				uiBoxDesigner.item_margin_x_px = 5f;
				uiBoxDesigner.item_margin_y_px = 0f;
				int inputCount = IN.getInputCount();
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < inputCount; j++)
					{
						string inputName = IN.getInputName(j);
						int keyInputByName = IN.getKeyInputByName(inputName);
						if (i == 0 && j == 15)
						{
							this.UbConfirm.Br();
						}
						if (keyInputByName != -1000 && i == 0 == (TX.isStart(inputName, "SUBMIT", 0) || TX.isStart(inputName, "CANCEL", 0)))
						{
							if (i == 1 && inputName == "M_NEUTRAL")
							{
								this.UbConfirm.Br();
							}
							using (STB stb = TX.PopBld(null, 0))
							{
								uiBoxDesigner.addP(new DsnDataP("", false)
								{
									name = "inputcheck_" + inputName,
									TxCol = MTRX.ColBlack,
									TxBorderCol = MTRX.ColWhite,
									Stb = this.submitP_text(stb, inputName, false),
									size = 14f,
									swidth = (float)((i == 1) ? 155 : 220),
									sheight = 78f,
									lineSpacing = 2.4f,
									html = true,
									text_auto_condense = true
								}, false).use_valotile = true;
							}
						}
					}
					this.UbConfirm.Br();
				}
				uiBoxDesigner.Br();
				uiBoxDesigner.alignx = ALIGN.LEFT;
				uiBoxDesigner.P("--", ALIGN.CENTER, uiBoxDesigner.use_w, true, 48f, "_confirm_p");
				uiBoxDesigner.alignx = ALIGN.CENTER;
				BtnContainer<aBtn> btnContainer = uiBoxDesigner.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
				{
					name = "underbtn",
					titles = new string[] { "submit_whole", "cancel" },
					skin_title = new string[]
					{
						TX.Get("Submit", ""),
						TX.Get("Cancel", "")
					},
					skin = "normal",
					click_snd = "",
					fnClick = new FnBtnBindings(this.fnClickInputtingDialog),
					navi_auto_fill = false,
					w = this.under_button_width,
					h = this.under_button_height,
					locked = 3,
					unselectable = 2
				});
				this.ConfirmSkin = btnContainer.Get(0).get_Skin() as ButtonSkinNormalNel;
				this.ConfirmCancelSkin = btnContainer.Get(1).get_Skin() as ButtonSkinNormalNel;
			}
			else
			{
				this.ConfirmSkin = this.ConfirmCancelSkin.getBtn().Container.GetButton(0).get_Skin() as ButtonSkinNormalNel;
				int inputCount2 = IN.getInputCount();
				for (int k = 0; k < inputCount2; k++)
				{
					string inputName2 = IN.getInputName(k);
					FillBlock fillBlock = this.UbConfirm.Get("inputcheck_" + inputName2, false) as FillBlock;
					if (!(fillBlock == null))
					{
						fillBlock.use_valotile = true;
						using (STB stb2 = TX.PopBld(null, 0))
						{
							fillBlock.Txt("");
							fillBlock.Txt(this.submitP_text(stb2, inputName2, false));
						}
					}
				}
			}
			this.UbConfirm.setValueTo("_confirm_p", " ");
			this.UbConfirm.setValueTo("_confirm_p", flag ? (TX.Get("Keyconfig_prompt_has_duplicate", "") + "\u3000" + TX.Get("KD_cancel", "")) : TX.Get("Keyconfig_prompt_confirm", ""));
			this.ConfirmSkin.hold_level = 0f;
			this.ConfirmCancelSkin.hold_level = 0f;
			this.ConfirmSkin.getBtn().SetChecked(false, true).SetLocked(true, true, false);
			this.ConfirmCancelSkin.getBtn().SetChecked(false, true).SetLocked(false, true, false);
			this.confirm_hold_time = -20;
			this.confirm_cancel_hold_time = -20;
			this.confirm_bits = 0U;
			Vector2 vector = new Vector2(0f, 0.3125f);
			this.UbConfirm.positionD(vector.x * 64f, vector.y * 64f, 1, 40f);
			this.UbConfirm.activate();
			UiBoxDesigner.FocusTo(this.UbConfirm);
			this.Hides.ClearRects().IgnoreRect(this.UbConfirm, vector.x, vector.y, 10f).activate();
			this.changeState(KeyConDesigner<aBtnNel>.STATE.CONFIRM_CHECK);
			if (flag)
			{
				this.ConfirmSkin = null;
			}
		}

		private STB submitP_text(STB Stb, string tx_key, bool pressing)
		{
			Stb.Add(" <font size=\"28\"><key ", tx_key, " />");
			if (tx_key == "SUBMIT" || tx_key == "CANCEL")
			{
				Stb.Add("<key ", tx_key, "2 />");
			}
			Stb.Add("</font> \n");
			KEY.SIMKEY simkey;
			bool flag = FEnum<KEY.SIMKEY>.TryParse(tx_key, out simkey, true) && (this.confirm_dupe & simkey) > (KEY.SIMKEY)0;
			if (flag)
			{
				Stb.AddTxA("lunch_tag_dupe", false).Add("<font color=\"", pressing ? "ff:#4D424B" : ("0x+" + C32.Color32ToCodeText(this.ColDupe)), "\">");
			}
			Stb.AddTxA("Keyconfig_name_" + tx_key.ToLower(), false);
			if (flag)
			{
				Stb.Add("</font>");
			}
			return Stb;
		}

		private UiBoxDesignerFamily Fam;

		private UiBoxDesigner UbInputKb;

		private UiBoxDesigner UbInputPad;

		private UiBoxDesigner UbLabelKb;

		private UiBoxDesigner UbLabelPad;

		private UiBoxDesigner UbConfirm;

		private UiBoxDesigner UbResetPrompt;

		private KEY.SIMKEY confirm_dupe;

		private aBtn CurLabelTarget;

		private uint confirm_bits;

		private int confirm_hold_time = -20;

		private int confirm_cancel_hold_time;

		private ButtonSkinNormalNel ConfirmSkin;

		private ButtonSkinNormalNel ConfirmCancelSkin;

		private const int confirm_hold_time_maxt = 100;

		private const int confirm_cancel_time_maxt = 40;

		public KeyConDesignerNel.RESULT result;

		public byte padinput_updated;

		public enum RESULT
		{
			NOTYET,
			SUCCESS,
			CANCEL
		}
	}
}
