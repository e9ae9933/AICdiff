using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace XX
{
	public sealed class KEY : IFontStorageListener
	{
		public PlayerInput PlayerCon { get; private set; }

		public int mvLA_out
		{
			get
			{
				return this.mvLA_out_ & 255;
			}
			set
			{
				this.mvLA_out_ = value;
			}
		}

		public int mvRA_out
		{
			get
			{
				return this.mvRA_out_ & 255;
			}
			set
			{
				this.mvRA_out_ = value;
			}
		}

		public int getInputCount()
		{
			return 30;
		}

		public string getInputName(int ipt)
		{
			return this.AInputs[ipt].name;
		}

		public bool fine_pad_input_label
		{
			set
			{
				if (value)
				{
					this.fine_input_label |= 2;
				}
			}
		}

		public KEY(PlayerInput PC, KEY Src = null)
		{
			this.PlayerCon = PC;
			int num = 30;
			this.AInputs = new KEY.InputHolder[num];
			for (int i = 0; i < num; i++)
			{
				KEY.InputHolder[] ainputs = this.AInputs;
				int num2 = i;
				InputActionAsset actions = this.PlayerCon.actions;
				KEY.IPT ipt = (KEY.IPT)i;
				ainputs[num2] = new KEY.InputHolder(actions[ipt.ToString()], i == 3 || i == 1);
			}
			this.relinkActions();
			this.ODefaultK = new BDic<string, KEY.KeyIcon>();
			this.ODefaultP = new BDic<string, KEY.KeyIcon>();
			this.clearKEY(true, true);
			if (Src != null)
			{
				this.copyFrom(Src);
			}
			MTRX.OFontStorage[MTRX.getCabinFont()].Add(this);
			this.PlayerCon.camera = IN.getGUICamera();
		}

		private void changedScheme(string scheme = null)
		{
			this.pad_mode = this.PlayerCon.currentControlScheme != "Keyboard&Mouse";
			this.pre_scheme = scheme ?? this.PlayerCon.currentControlScheme;
			this.FnSwitchedDevice();
		}

		private void relinkActions()
		{
			this.ActRet = this.PlayerCon.actions["Ret"];
			this.ActForRebind = this.PlayerCon.actions["ForRebind"];
			this.ActForRebind.RemoveBindingOverride(0);
			this.ActForRebind.Disable();
		}

		public void fineCurrentPadState()
		{
			if (!this.pad_mode)
			{
				return;
			}
			bool flag = false;
			ReadOnlyArray<InputDevice> devices = InputSystem.devices;
			for (int i = devices.Count - 1; i >= 0; i--)
			{
				string layout = devices[i].layout;
				if (layout.IndexOf("HID::") >= 0 || layout.IndexOf("pad") >= 0 || layout.IndexOf("stick") >= 0 || layout.IndexOf("Dual") >= 0)
				{
					flag = true;
					this.PlayerCon.SwitchCurrentControlScheme(new InputDevice[] { devices[i] });
					break;
				}
			}
			if (flag)
			{
				this.UpdateControleScheme();
			}
		}

		public void UpdateControleScheme()
		{
			int num = 30;
			this.PlayerCon.SwitchCurrentActionMap("Player");
			for (int i = 0; i < num; i++)
			{
				this.AInputs[i].UpdateAction(this.PlayerCon.currentActionMap.actions[i]);
			}
			this.relinkActions();
			this.fineAllInputLabel(true, true);
		}

		public void destruct()
		{
			MTRX.OFontStorage[MTRX.getCabinFont()].Rem(this);
			IN.DestroyOne(this.PlayerCon);
		}

		public void clearKEY(bool keyboard = true, bool pad = true)
		{
			this.jump_override = (KEY.JUMP_OVERRIDE)0;
			if (keyboard && pad)
			{
				this.PlayerCon.actions.RemoveAllBindingOverrides();
			}
			else if (keyboard || pad)
			{
				int num = 30;
				for (int i = 0; i < num; i++)
				{
					this.AInputs[i].RemoveOverrides(keyboard, pad);
				}
			}
			if (keyboard)
			{
				this.ODefaultK.Clear();
			}
			if (pad)
			{
				this.ODefaultP.Clear();
			}
			string text = "<HID::*>";
			bool flag = this.is_xinput;
			if (!flag)
			{
				if (IN.connected_hid_device != null)
				{
					text = "<" + IN.connected_hid_device + ">";
				}
				else
				{
					flag = (this.is_xinput = true);
				}
			}
			this.setKeyboardAndPadInput(KEY.IPT.SUBMIT, keyboard, pad, "z", flag ? "buttonSouth" : (text + "/button2"));
			this.setKeyboardAndPadInput(KEY.IPT.SUBMIT2, keyboard, pad, "space", flag ? "select" : (text + "/button9"));
			this.setKeyboardAndPadInput(KEY.IPT.CANCEL, keyboard, pad, "x", flag ? "buttonEast" : (text + "/button3"));
			this.setKeyboardAndPadInput(KEY.IPT.CANCEL2, keyboard, pad, "escape", flag ? "Start" : (text + "/button10"));
			this.setKeyboardAndPadInput(KEY.IPT.LA, keyboard, pad, "leftArrow", flag ? "leftStick/left" : "<Joystick>/stick/left");
			this.setKeyboardAndPadInput(KEY.IPT.TA, keyboard, pad, "upArrow", flag ? "leftStick/up" : "<Joystick>/stick/up");
			this.setKeyboardAndPadInput(KEY.IPT.RA, keyboard, pad, "rightArrow", flag ? "leftStick/right" : "<Joystick>/stick/right");
			this.setKeyboardAndPadInput(KEY.IPT.BA, keyboard, pad, "downArrow", flag ? "leftStick/down" : "<Joystick>/stick/down");
			this.setKeyboardAndPadInput(KEY.IPT.JUMP, keyboard, pad, "upArrow", flag ? "buttonSouth" : (text + "/button2"));
			this.setKeyboardAndPadInput(KEY.IPT.RUN, keyboard, pad, "space", flag ? "leftStickPress" : (text + "/button11"));
			this.setKeyboardAndPadInput(KEY.IPT.Z, keyboard, pad, "z", flag ? "buttonWest" : "<Joystick>/trigger");
			this.setKeyboardAndPadInput(KEY.IPT.X, keyboard, pad, "x", flag ? "buttonEast" : (text + "/button3"));
			this.setKeyboardAndPadInput(KEY.IPT.C, keyboard, pad, "c", flag ? "buttonNorth" : (text + "/button4"));
			this.setKeyboardAndPadInput(KEY.IPT.A, keyboard, pad, "a", flag ? "leftShoulder" : (text + "/button5"));
			this.setKeyboardAndPadInput(KEY.IPT.S, keyboard, pad, "s", flag ? "rightTrigger" : (text + "/button12"));
			this.setKeyboardAndPadInput(KEY.IPT.D, keyboard, pad, "d", flag ? "leftTrigger" : (text + "/button7"));
			this.setKeyboardAndPadInput(KEY.IPT.LSH, keyboard, pad, "leftShift", flag ? "rightShoulder" : (text + "/button6"));
			this.setKeyboardAndPadInput(KEY.IPT.MENU, keyboard, pad, "escape", flag ? "start" : (text + "/button10"));
			this.setKeyboardAndPadInput(KEY.IPT.LTAB, keyboard, pad, "q", flag ? "leftShoulder" : (text + "/button5"));
			this.setKeyboardAndPadInput(KEY.IPT.RTAB, keyboard, pad, "e", flag ? "rightShoulder" : (text + "/button6"));
			this.setKeyboardAndPadInput(KEY.IPT.SORT, keyboard, pad, "a", flag ? "rightTrigger" : (text + "/button8"));
			this.setKeyboardAndPadInput(KEY.IPT.ADD, keyboard, pad, "tab", flag ? "buttonNorth" : (text + "/4"));
			this.setKeyboardAndPadInput(KEY.IPT.REM, keyboard, pad, "leftCtrl", flag ? "buttonWest" : "<Joystick>/trigger");
			this.setKeyboardAndPadInput(KEY.IPT.SHIFT, keyboard, pad, "leftShift", flag ? "leftTrigger" : (text + "/button7"));
			this.setKeyboardAndPadInput(KEY.IPT.CHECK, keyboard, pad, "upArrow", flag ? "leftStick/up" : "<Joystick>/stick/up");
			this.setKeyboardAndPadInput(KEY.IPT.M_NEUTRAL, keyboard, pad, "f1", "rightStickPress");
			this.setKeyboardAndPadInput(KEY.IPT.MLA, keyboard, pad, "f2", "rightStick/left");
			this.setKeyboardAndPadInput(KEY.IPT.MTA, keyboard, pad, "f4", "rightStick/up");
			this.setKeyboardAndPadInput(KEY.IPT.MRA, keyboard, pad, "f5", "rightStick/right");
			this.setKeyboardAndPadInput(KEY.IPT.MBA, keyboard, pad, "f3", "rightStick/down");
			MTRX.OFontStorage[MTRX.getCabinFont()].Fine();
			this.fine_input_label = 3;
			this.fineSubmitAlias();
		}

		public void fineSubmitAlias()
		{
			this.recalcJumpOverride();
			this.finePressThreshold();
		}

		public void recalcJumpOverride()
		{
			this.jump_override = (KEY.JUMP_OVERRIDE)0;
			if (KEY.InputHolder.JumpInputIsSame(this.AInputs[15], this.AInputs[12], true))
			{
				this.jump_override |= KEY.JUMP_OVERRIDE.PAD_TA;
			}
			if (KEY.InputHolder.JumpInputIsSame(this.AInputs[15], this.AInputs[14], true))
			{
				this.jump_override |= KEY.JUMP_OVERRIDE.PAD_BA;
			}
			if (KEY.InputHolder.JumpInputIsSame(this.AInputs[15], this.AInputs[12], false))
			{
				this.jump_override |= KEY.JUMP_OVERRIDE.KB_TA;
			}
			if (KEY.InputHolder.JumpInputIsSame(this.AInputs[15], this.AInputs[14], false))
			{
				this.jump_override |= KEY.JUMP_OVERRIDE.KB_BA;
			}
		}

		public void finePressThreshold()
		{
			int num = 30;
			float num2 = this.stick_threshold;
			if (num2 < 0f)
			{
				num2 = 0.5f;
			}
			for (int i = 0; i < num; i++)
			{
				this.AInputs[i].finePressThreshold(num2);
			}
		}

		public void clearPress()
		{
			this.mvSUBMIT = 0f;
			this.mvCANCEL = 0f;
			this.mvZ = 0f;
			this.mvX = 0f;
			this.mvC = 0f;
			this.mvA = 0f;
			this.mvS = 0f;
			this.mvD = 0f;
			this.mvLSH = 0f;
			this.mvJUMP = 0f;
			this.mvMENU = 0f;
			this.mvRUN = 0f;
			this.mvCHECK = 0f;
			this.mvLA = (float)(this.mvLA_out_ = 0);
			this.mvRA = (float)(this.mvRA_out_ = 0);
			this.mvTA = 0f;
			this.mvBA = 0f;
			this.mvM_NEUTRAL = 0f;
			this.mvMLA = 0f;
			this.mvMRA = 0f;
			this.mvMTA = 0f;
			this.mvMBA = 0f;
		}

		public void clearPushDown(bool strong = false)
		{
			if (strong)
			{
				this.mvSUBMIT = ((this.mvSUBMIT >= -1024f) ? (-1024f) : this.mvSUBMIT);
				this.mvCHECK = ((this.mvCHECK >= -1024f) ? (-1024f) : this.mvCHECK);
				this.mvZ = ((this.mvZ >= -1024f) ? (-1024f) : this.mvZ);
				this.mvX = ((this.mvX >= -1024f) ? (-1024f) : this.mvX);
				this.mvC = ((this.mvC >= -1024f) ? (-1024f) : this.mvC);
				this.mvA = ((this.mvA >= -1024f) ? (-1024f) : this.mvA);
			}
			else
			{
				this.mvSUBMIT = ((this.mvSUBMIT == 1f) ? 2f : this.mvSUBMIT);
				this.mvCHECK = ((this.mvCHECK == 1f) ? 2f : this.mvCHECK);
				this.mvZ = ((this.mvZ == 1f) ? 2f : this.mvZ);
				this.mvX = ((this.mvX == 1f) ? 2f : this.mvX);
				this.mvC = ((this.mvC == 1f) ? 2f : this.mvC);
				this.mvA = ((this.mvA == 1f) ? 2f : this.mvA);
			}
			this.clearCancelPushDown(strong);
		}

		public void clearMagicPushDown(bool strong = true)
		{
			if (strong)
			{
				this.mvX = ((this.mvX >= -1024f) ? (-1024f) : this.mvX);
				return;
			}
			this.mvX = ((this.mvX == 1f) ? 2f : this.mvX);
		}

		public void clearMenuPushDown(bool strong = false)
		{
			if (strong)
			{
				this.mvMENU = ((this.mvMENU >= -1024f) ? (-1024f) : this.mvMENU);
				this.mvA = ((this.mvA >= -1024f) ? (-1024f) : this.mvA);
				return;
			}
			this.mvMENU = ((this.mvMENU == 1f) ? 2f : this.mvMENU);
			this.mvA = ((this.mvA == 1f) ? 2f : this.mvA);
		}

		public void clearArrowPD(uint bits, bool strong = false, int hold_update = 1)
		{
			if ((bits & 1U) != 0U)
			{
				this.mvLA = (strong ? ((this.mvLA >= -1024f) ? (-1024f) : this.mvLA) : ((1f <= this.mvLA && this.mvLA <= (float)hold_update) ? ((float)(hold_update + 1)) : this.mvLA));
			}
			if ((bits & 2U) != 0U)
			{
				this.mvTA = (strong ? ((this.mvTA >= -1024f) ? (-1024f) : this.mvTA) : ((1f <= this.mvTA && this.mvTA <= (float)hold_update) ? ((float)(hold_update + 1)) : this.mvTA));
			}
			if ((bits & 4U) != 0U)
			{
				this.mvRA = (strong ? ((this.mvRA >= -1024f) ? (-1024f) : this.mvRA) : ((1f <= this.mvRA && this.mvRA <= (float)hold_update) ? ((float)(hold_update + 1)) : this.mvRA));
			}
			if ((bits & 8U) != 0U)
			{
				this.mvBA = (strong ? ((this.mvBA >= -1024f) ? (-1024f) : this.mvBA) : ((1f <= this.mvBA && this.mvBA <= (float)hold_update) ? ((float)(hold_update + 1)) : this.mvBA));
			}
		}

		public void holdArrowInput()
		{
			this.clearArrowPD(15U, true, 1);
		}

		public void clearCancelPushDown(bool strong = false)
		{
			if (strong)
			{
				this.mvCANCEL = ((this.mvCANCEL >= -1024f) ? (-1024f) : this.mvCANCEL);
				this.mvMENU = ((this.mvMENU >= -1024f) ? (-1024f) : this.mvMENU);
				return;
			}
			this.mvCANCEL = ((this.mvCANCEL == 1f) ? 2f : this.mvCANCEL);
			this.mvMENU = ((this.mvMENU == 1f) ? 2f : this.mvMENU);
		}

		public void updatePressing(float fcnt)
		{
			if ((IN.totalframe & 31) == 0)
			{
				string currentControlScheme = this.PlayerCon.currentControlScheme;
				if (this.pre_scheme != currentControlScheme)
				{
					this.changedScheme(currentControlScheme);
				}
			}
			this.checkKPSubmit(fcnt, this.AInputs[0], this.AInputs[1], this.ActRet, ref this.mvSUBMIT, false);
			if (!LabeledInputField.focus_exist)
			{
				this.checkKP(fcnt, this.AInputs[2], this.AInputs[3], ref this.mvCANCEL, false);
			}
			else if (this.mvCANCEL > -1024f)
			{
				this.mvCANCEL = 0f;
			}
			if (this.turn_lrtb_input)
			{
				this.checkKParrow(fcnt, this.AInputs[11], ref this.mvRA, ref this.mvRA_out_, ref this.Apad_tilt_level[1]);
				this.checkKParrow(fcnt, this.AInputs[13], ref this.mvLA, ref this.mvLA_out_, ref this.Apad_tilt_level[0]);
				this.checkKP(fcnt, this.AInputs[12], ref this.mvBA, true);
				this.checkKP(fcnt, this.AInputs[14], ref this.mvTA, true);
				this.turn_lrtb_input = false;
			}
			else
			{
				this.checkKParrow(fcnt, this.AInputs[11], ref this.mvLA, ref this.mvLA_out_, ref this.Apad_tilt_level[0]);
				this.checkKParrow(fcnt, this.AInputs[13], ref this.mvRA, ref this.mvRA_out_, ref this.Apad_tilt_level[1]);
				this.checkKP(fcnt, this.AInputs[12], ref this.mvTA, true);
				this.checkKP(fcnt, this.AInputs[14], ref this.mvBA, true);
			}
			if (this.mvLA_out_ > 0)
			{
				this.mvLA_out_ += 256;
				if (this.mvLA_out_ >> 8 >= 16)
				{
					this.mvLA_out_ = 0;
				}
			}
			if (this.mvRA_out_ > 0)
			{
				this.mvRA_out_ += 256;
				if (this.mvRA_out_ >> 8 >= 16)
				{
					this.mvRA_out_ = 0;
				}
			}
			this.checkKP(fcnt, this.AInputs[16], ref this.mvRUN, false);
			this.checkKP(fcnt, this.AInputs[17], ref this.mvCHECK, false);
			this.checkKP(fcnt, this.AInputs[18], ref this.mvZ, false);
			this.checkKP(fcnt, this.AInputs[19], ref this.mvX, false);
			this.checkKP(fcnt, this.AInputs[20], ref this.mvC, false);
			this.checkKP(fcnt, this.AInputs[21], ref this.mvA, false);
			this.checkKP(fcnt, this.AInputs[22], ref this.mvS, false);
			this.checkKP(fcnt, this.AInputs[23], ref this.mvD, false);
			this.checkKP(fcnt, this.AInputs[24], ref this.mvLSH, false);
			if ((this.pad_mode ? (this.jump_override & KEY.JUMP_OVERRIDE.PAD_TA) : (this.jump_override & KEY.JUMP_OVERRIDE.KB_TA)) != (KEY.JUMP_OVERRIDE)0)
			{
				this.mvJUMP = this.mvTA;
			}
			else if ((this.pad_mode ? (this.jump_override & KEY.JUMP_OVERRIDE.PAD_BA) : (this.jump_override & KEY.JUMP_OVERRIDE.KB_BA)) != (KEY.JUMP_OVERRIDE)0)
			{
				this.mvJUMP = this.mvBA;
			}
			else
			{
				this.checkKP(fcnt, this.AInputs[15], ref this.mvJUMP, false);
			}
			this.checkKP(fcnt, this.AInputs[25], ref this.mvM_NEUTRAL, false);
			this.checkKP(fcnt, this.AInputs[26], ref this.mvMLA, false);
			this.checkKP(fcnt, this.AInputs[28], ref this.mvMRA, false);
			this.checkKP(fcnt, this.AInputs[29], ref this.mvMBA, false);
			this.checkKP(fcnt, this.AInputs[27], ref this.mvMTA, false);
			this.checkKP(fcnt, this.AInputs[4], ref this.mvMENU, false);
		}

		public InputActionRebindingExtensions.RebindingOperation initRebinding(string name, bool is_pad, KEY.FnRebindListener FnComplete)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				KEY.InputHolder Target = this.AInputs[(int)ipt];
				InputAction act = Target.Act;
				InputAction Rebinder = IN.getCurrentKeyAssignObject().ActForRebind;
				Rebinder.Disable();
				InputActionRebindingExtensions.RebindingOperation rebindingOperation = Rebinder.PerformInteractiveRebinding(0);
				if (!is_pad)
				{
					rebindingOperation.WithControlsHavingToMatchPath("<Keyboard>");
				}
				else
				{
					rebindingOperation.WithControlsExcluding("<Keyboard>");
				}
				rebindingOperation.WithControlsExcluding("<Pen>");
				rebindingOperation.WithControlsExcluding("<Keyboard>/enter");
				rebindingOperation.OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation operation)
				{
					Target.RebindUpdate(Rebinder, is_pad);
					if (!is_pad)
					{
						this.setKeyboardInput(ipt, null);
					}
					else
					{
						if (ipt <= KEY.IPT.SUBMIT2)
						{
							this.fineSubmitAlias();
						}
						this.setPadInput(ipt, null);
					}
					FnComplete(name, is_pad, false);
				});
				rebindingOperation.OnCancel(delegate(InputActionRebindingExtensions.RebindingOperation operation)
				{
					FnComplete(name, is_pad, true);
				});
				rebindingOperation.OnMatchWaitForAnother(0.1f);
				return rebindingOperation.Start();
			}
			return null;
		}

		public void quitRebinding(string name)
		{
			IN.getCurrentKeyAssignObject().ActForRebind.RemoveBindingOverride(0);
			IN.clearPushDown(true);
		}

		private bool checkKP(float fcnt, KEY.InputHolder H0, KEY.InputHolder H1, ref float mv, bool is_lrtb)
		{
			if (mv >= 1f)
			{
				if (H0.isOn(false) || H1.isOn(false))
				{
					mv += fcnt;
					return true;
				}
				mv = -X.Mn(1023f, mv);
			}
			else
			{
				bool flag = true;
				if (mv != 0f)
				{
					flag = false;
					if (mv >= -1023f)
					{
						mv = 0f;
					}
					else if (mv >= -1026f)
					{
						mv -= 1f;
					}
					else if (!H0.isOn(false) && !H1.isOn(false))
					{
						mv = 0f;
					}
				}
				if (!flag)
				{
					return false;
				}
				if (H0.isPDs() || H1.isPDs())
				{
					mv = 1f;
					return true;
				}
			}
			return false;
		}

		private bool checkKPSubmit(float fcnt, KEY.InputHolder H0, KEY.InputHolder H1, InputAction H2, ref float mv, bool is_lrtb)
		{
			if (mv >= 1f)
			{
				if (H0.isOn(false) || H1.isOn(false) || H2.IsPressed())
				{
					mv += fcnt;
					return true;
				}
				mv = -X.Mn(1023f, mv);
			}
			else
			{
				bool flag = !LabeledInputField.focus_exist;
				if (mv != 0f)
				{
					flag = false;
					if (mv >= -1023f || (!H0.isOn(false) && !H1.isOn(false) && !H2.IsPressed()))
					{
						mv = 0f;
					}
				}
				if (!flag)
				{
					return false;
				}
				if (H0.isPDs() || H1.isPDs() || H2.WasPressedThisFrame())
				{
					mv = 1f;
					return true;
				}
			}
			return false;
		}

		private bool checkKP(float fcnt, KEY.InputHolder H0, ref float mv, bool is_arrow = false)
		{
			if (mv >= 1f)
			{
				if (H0.isOn(false))
				{
					mv += fcnt;
					return true;
				}
				mv = -1f;
			}
			else
			{
				bool flag = true;
				if (mv != 0f)
				{
					flag = false;
					if (mv >= -1023f || !H0.isOn(false))
					{
						mv = 0f;
					}
				}
				if (!flag)
				{
					return false;
				}
				if (H0.isPDs())
				{
					mv = 1f;
					return true;
				}
			}
			return false;
		}

		private bool checkKParrow(float fcnt, KEY.InputHolder H0, ref float mv, ref int mv_out, ref float stick_tilt)
		{
			if (mv >= 1f)
			{
				if (H0.isOn(false))
				{
					stick_tilt = H0.readStickTilt();
					mv += fcnt;
					return true;
				}
				mv_out = (int)mv;
				mv = -1f;
				stick_tilt = 0f;
			}
			else
			{
				bool flag = true;
				if (mv != 0f)
				{
					flag = false;
					if (mv >= -1023f || !H0.isOn(false))
					{
						mv = 0f;
					}
				}
				if (!flag)
				{
					stick_tilt = 0f;
					return false;
				}
				if (H0.isPDs())
				{
					stick_tilt = H0.readStickTilt();
					mv = 1f;
					mv_out = 0;
					return true;
				}
				stick_tilt = 0f;
			}
			return false;
		}

		public bool checkPD_manual(KEY.SIMKEY key)
		{
			if (key <= KEY.SIMKEY.SORT)
			{
				if (key == KEY.SIMKEY.LTAB)
				{
					return this.AInputs[5].isPD();
				}
				if (key == KEY.SIMKEY.RTAB)
				{
					return this.AInputs[6].isPD();
				}
				if (key == KEY.SIMKEY.SORT)
				{
					return this.AInputs[7].isPD();
				}
			}
			else
			{
				if (key == KEY.SIMKEY.ADD)
				{
					return this.AInputs[9].isPD();
				}
				if (key == KEY.SIMKEY.REM)
				{
					return this.AInputs[10].isPD();
				}
				if (key == KEY.SIMKEY.SHIFT)
				{
					return this.AInputs[8].isPD();
				}
			}
			return false;
		}

		public bool checkO_manual(KEY.SIMKEY key)
		{
			if (key <= KEY.SIMKEY.SORT)
			{
				if (key == KEY.SIMKEY.LTAB)
				{
					return this.AInputs[5].isOn(true);
				}
				if (key == KEY.SIMKEY.RTAB)
				{
					return this.AInputs[6].isOn(true);
				}
				if (key == KEY.SIMKEY.SORT)
				{
					return this.AInputs[7].isOn(true);
				}
			}
			else
			{
				if (key == KEY.SIMKEY.ADD)
				{
					return this.AInputs[9].isOn(true);
				}
				if (key == KEY.SIMKEY.REM)
				{
					return this.AInputs[10].isOn(true);
				}
				if (key == KEY.SIMKEY.SHIFT)
				{
					return this.AInputs[8].isOn(true);
				}
			}
			return false;
		}

		public bool isReturnPD()
		{
			return !LabeledInputField.focus_exist && this.ActRet.WasPressedThisFrame();
		}

		public KEY copyFrom(KEY Src)
		{
			Src.fineAllInputLabel(false, false);
			this.fineAllInputLabel(false, false);
			this.pad_mode = Src.pad_mode;
			int num = 30;
			for (int i = 0; i < num; i++)
			{
				this.AInputs[i].copyFrom(Src.AInputs[i]);
			}
			for (int j = 0; j < 2; j++)
			{
				BDic<string, KEY.KeyIcon> bdic = ((j == 0) ? this.ODefaultK : this.ODefaultP);
				Dictionary<string, KEY.KeyIcon> dictionary = ((j == 0) ? Src.ODefaultK : Src.ODefaultP);
				bdic.Clear();
				foreach (KeyValuePair<string, KEY.KeyIcon> keyValuePair in dictionary)
				{
					bdic[keyValuePair.Key] = new KEY.KeyIcon().Set(keyValuePair.Value);
				}
			}
			MTRX.OFontStorage[MTRX.getCabinFont()].Fine();
			this.fineSubmitAlias();
			this.fineAllInputLabel(true, true);
			this.changedScheme(null);
			return this;
		}

		public KEY countupId()
		{
			this.text_id += 2;
			if (this.text_id >= 65536)
			{
				this.text_id -= 65536;
			}
			return this;
		}

		public bool pad_mode
		{
			get
			{
				return (this.text_id & 1) != 0;
			}
			set
			{
				if (value == this.pad_mode)
				{
					return;
				}
				if (value)
				{
					this.text_id |= 1;
				}
				else
				{
					this.text_id &= -2;
				}
				this.FnSwitchedDevice();
			}
		}

		public int text_id_for_tx_renderer
		{
			get
			{
				if (KEY.keydesc_appearance == 0)
				{
					return this.text_id;
				}
				return (this.text_id & -2) | (int)(KEY.keydesc_appearance - 1);
			}
		}

		public int checkWholePadInput(string name, string[] Aignore, BDic<string, int> Ohld_cnt, out float holding, out float holdaxis)
		{
			holding = 0f;
			holdaxis = 0f;
			return 0;
		}

		public BList<string> PopInputNameArray()
		{
			BList<string> blist = ListBuffer<string>.Pop(30);
			for (int i = 0; i < 30; i++)
			{
				blist.Add(this.AInputs[i].name);
			}
			return blist;
		}

		public string getReplacedKeyboardInputTitle(string kc, bool raw_flag = false)
		{
			if (kc == null)
			{
				return "";
			}
			string text = kc.ToLower();
			TX tx = TX.getTX("Input_" + text, true, true, null);
			if (tx != null)
			{
				return tx.text;
			}
			return TX.HeadUpper(text);
		}

		public string getKeyboardRawInputTitle(string name)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				return this.getReplacedKeyboardInputTitle(this.AInputs[(int)ipt].keyboard_raw_input_title, false);
			}
			return "???";
		}

		public int getIconNumForText(string name, int pad_mode_auto = -1)
		{
			KEY.IPT ipt;
			if (!FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				return 0;
			}
			if (pad_mode_auto < 0)
			{
				pad_mode_auto = (this.pad_mode ? 1 : 0);
			}
			this.fineAllInputLabel(false, false);
			KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
			if (inputHolder.no_icon)
			{
				return 0;
			}
			if (pad_mode_auto == 0)
			{
				return inputHolder.key_icon;
			}
			return inputHolder.pad_icon;
		}

		public int getIconNumForText(string name, ref float left_margin, ref float right_margin, int pad_mode_auto = -1)
		{
			int iconNumForText = this.getIconNumForText(name, pad_mode_auto);
			right_margin = 16f;
			left_margin = (float)(X.BTWW(8f, (float)iconNumForText, 11f) ? 46 : 16);
			return iconNumForText;
		}

		public string getLabelForText(string name, int pad_mode_auto = -1)
		{
			if (pad_mode_auto < 0)
			{
				pad_mode_auto = (this.pad_mode ? 1 : 0);
			}
			return this.getLabelForText(name, pad_mode_auto != 0);
		}

		public string getLabelForText(string name, bool pad_mode)
		{
			KEY.IPT ipt;
			if (!FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				return "";
			}
			this.fineAllInputLabel(false, false);
			KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
			if (!pad_mode)
			{
				return inputHolder.key_label;
			}
			return inputHolder.pad_label;
		}

		public int getKeyboardIconNum(string name)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				this.fineAllInputLabel(false, false);
				return this.AInputs[(int)ipt].key_icon;
			}
			return 0;
		}

		public string getKeyboardIconLabel(string name)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				this.fineAllInputLabel(false, false);
				return this.AInputs[(int)ipt].key_label;
			}
			return "";
		}

		public string getPadRawInputTitle(string name)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
				string pad_raw_input_title = inputHolder.pad_raw_input_title;
				string text2;
				string text = this.getReplacedPadInputTitle(pad_raw_input_title, out text2, true);
				int num = this.pad_input_key_to_icon_index((text2 != "") ? text2 : pad_raw_input_title, true);
				if (num >= 1)
				{
					text = string.Concat(new string[]
					{
						"<img keyconfig=\"",
						(num - 1).ToString(),
						"\" scale=\"",
						((num == 17) ? 0.7 : 0.35).ToString(),
						"\" margin=\"4\" tx_color />",
						text
					});
				}
				else
				{
					string pad_raw_path = inputHolder.pad_raw_path;
					if (pad_raw_path != null && (pad_raw_path == "<Gamepad>/buttonEast" || pad_raw_path == "<Gamepad>/buttonWest" || pad_raw_path == "<Gamepad>/buttonSouth" || pad_raw_path == "<Gamepad>/buttonNorth") && !TX.isStart(text, "<img", 0))
					{
						text = TX.Get("PadInput_" + TX.slice(inputHolder.pad_raw_path, "<Gamepad>/button".Length), "") + " <font size=\"70%\">(" + text + ")</font>";
					}
				}
				return text;
			}
			return "???";
		}

		public string getReplacedPadInputTitle(string name, out string dirname, bool raw_mode = false)
		{
			dirname = "";
			if (TX.noe(name))
			{
				return "(None)";
			}
			int num = name.IndexOf("/");
			if (num >= 0)
			{
				string text = TX.slice(name, num + 1);
				string text2 = TX.slice(name, 0, num);
				name = text;
				dirname = text2;
			}
			if (name.IndexOf("button") == 0)
			{
				name = TX.slice(name, 6);
			}
			TX tx;
			if (!raw_mode)
			{
				tx = TX.getTX("PadInputL_" + name, true, true, null);
				if (tx != null)
				{
					return tx.text;
				}
			}
			tx = TX.getTX("PadInput_" + name, true, true, null);
			if (tx != null)
			{
				return tx.text;
			}
			return TX.HeadUpper(name);
		}

		public int getPadIconNum(string name)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				this.fineAllInputLabel(false, false);
				return this.AInputs[(int)ipt].pad_icon;
			}
			return 0;
		}

		public string getPadIconLabel(string name)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				this.fineAllInputLabel(false, false);
				return this.AInputs[(int)ipt].pad_label;
			}
			return "";
		}

		public bool isNoIconInput(string tx_key)
		{
			KEY.IPT ipt;
			return FEnum<KEY.IPT>.TryParse(tx_key, out ipt, true) && this.AInputs[(int)ipt].no_icon;
		}

		private void setKeyboardAndPadInput(KEY.IPT t, bool keyboard_reset, bool pad_reset, string k, string pad)
		{
			if (keyboard_reset)
			{
				this.setKeyboardInput(t, k);
			}
			if (pad_reset)
			{
				this.setPadInput(t, pad);
			}
		}

		private void setKeyboardInput(KEY.IPT t, string k)
		{
			if (k == "enter")
			{
				return;
			}
			KEY.InputHolder inputHolder = this.AInputs[(int)t];
			inputHolder.writeKey(k);
			k = inputHolder.keyboard_raw_input_title;
			KEY.KeyIcon keyIcon = X.Get<string, KEY.KeyIcon>(this.ODefaultK, k);
			if (keyIcon != null)
			{
				inputHolder.SetK(keyIcon);
				return;
			}
			inputHolder.key_label = this.getKbDisplayLabelDefault(k, false);
			string text = TX.HeadLower(k);
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 1535127855U)
				{
					if (num <= 1369374877U)
					{
						if (num <= 305218375U)
						{
							if (num != 17061501U)
							{
								if (num != 237643453U)
								{
									if (num != 305218375U)
									{
										goto IL_0411;
									}
									if (!(text == "rightShift"))
									{
										goto IL_0411;
									}
								}
								else
								{
									if (!(text == "numpadPeriod"))
									{
										goto IL_0411;
									}
									goto IL_0409;
								}
							}
							else
							{
								if (!(text == "numpadDivide"))
								{
									goto IL_0411;
								}
								goto IL_0409;
							}
						}
						else if (num != 629943691U)
						{
							if (num != 894689925U)
							{
								if (num != 1369374877U)
								{
									goto IL_0411;
								}
								if (!(text == "numpadEquals"))
								{
									goto IL_0411;
								}
								goto IL_0409;
							}
							else
							{
								if (!(text == "space"))
								{
									goto IL_0411;
								}
								inputHolder.key_icon = 2;
								inputHolder.key_label = "\uff3f";
								return;
							}
						}
						else if (!(text == "leftAlt"))
						{
							goto IL_0411;
						}
					}
					else if (num <= 1468017379U)
					{
						if (num != 1420235188U)
						{
							if (num != 1451239760U)
							{
								if (num != 1468017379U)
								{
									goto IL_0411;
								}
								if (!(text == "numpad5"))
								{
									goto IL_0411;
								}
								goto IL_0409;
							}
							else
							{
								if (!(text == "numpad4"))
								{
									goto IL_0411;
								}
								goto IL_0409;
							}
						}
						else if (!(text == "leftShift"))
						{
							goto IL_0411;
						}
					}
					else if (num <= 1501572617U)
					{
						if (num != 1484794998U)
						{
							if (num != 1501572617U)
							{
								goto IL_0411;
							}
							if (!(text == "numpad7"))
							{
								goto IL_0411;
							}
							goto IL_0409;
						}
						else
						{
							if (!(text == "numpad6"))
							{
								goto IL_0411;
							}
							goto IL_0409;
						}
					}
					else if (num != 1518350236U)
					{
						if (num != 1535127855U)
						{
							goto IL_0411;
						}
						if (!(text == "numpad1"))
						{
							goto IL_0411;
						}
						goto IL_0409;
					}
					else
					{
						if (!(text == "numpad0"))
						{
							goto IL_0411;
						}
						goto IL_0409;
					}
				}
				else if (num <= 2219393156U)
				{
					if (num <= 1652571188U)
					{
						if (num != 1551905474U)
						{
							if (num != 1568683093U)
							{
								if (num != 1652571188U)
								{
									goto IL_0411;
								}
								if (!(text == "numpad8"))
								{
									goto IL_0411;
								}
								goto IL_0409;
							}
							else
							{
								if (!(text == "numpad3"))
								{
									goto IL_0411;
								}
								goto IL_0409;
							}
						}
						else
						{
							if (!(text == "numpad2"))
							{
								goto IL_0411;
							}
							goto IL_0409;
						}
					}
					else if (num != 1669348807U)
					{
						if (num != 1846416286U)
						{
							if (num != 2219393156U)
							{
								goto IL_0411;
							}
							if (!(text == "numpadPlus"))
							{
								goto IL_0411;
							}
							goto IL_0409;
						}
						else if (!(text == "rightControl"))
						{
							goto IL_0411;
						}
					}
					else
					{
						if (!(text == "numpad9"))
						{
							goto IL_0411;
						}
						goto IL_0409;
					}
				}
				else if (num <= 2884204546U)
				{
					if (num != 2476717177U)
					{
						if (num != 2787020153U)
						{
							if (num != 2884204546U)
							{
								goto IL_0411;
							}
							if (!(text == "numpadEnter"))
							{
								goto IL_0411;
							}
							goto IL_0409;
						}
						else if (!(text == "leftCtrl"))
						{
							goto IL_0411;
						}
					}
					else if (!(text == "leftCommand"))
					{
						goto IL_0411;
					}
				}
				else if (num <= 3710900444U)
				{
					if (num != 2932374194U)
					{
						if (num != 3710900444U)
						{
							goto IL_0411;
						}
						if (!(text == "numpadMultiply"))
						{
							goto IL_0411;
						}
						goto IL_0409;
					}
					else if (!(text == "rightCommand"))
					{
						goto IL_0411;
					}
				}
				else if (num != 3909191772U)
				{
					if (num != 4231430636U)
					{
						goto IL_0411;
					}
					if (!(text == "numpadMinus"))
					{
						goto IL_0411;
					}
					goto IL_0409;
				}
				else if (!(text == "rightAlt"))
				{
					goto IL_0411;
				}
				inputHolder.key_icon = 2;
				return;
				IL_0409:
				inputHolder.key_icon = 3;
				return;
			}
			IL_0411:
			inputHolder.key_icon = 1;
		}

		public string getKbDisplayLabelDefault(string name)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
				return this.getKbDisplayLabelDefault(inputHolder.keyboard_raw_input_title, false);
			}
			return null;
		}

		private string getKbDisplayLabelDefault(string keyboard_raw_input_title, bool raw_flag)
		{
			return this.getReplacedKeyboardInputTitle(keyboard_raw_input_title, raw_flag);
		}

		public static bool x_shifting_icon(int icon_id)
		{
			return X.BTWW(7f, (float)icon_id, 11f) || icon_id == 3;
		}

		private void setPadInput(KEY.IPT t, string k)
		{
			KEY.InputHolder inputHolder = this.AInputs[(int)t];
			inputHolder.writePad(k);
			k = inputHolder.pad_raw_input_title;
			string text = k;
			KEY.KeyIcon keyIcon = X.Get<string, KEY.KeyIcon>(this.ODefaultP, text);
			if (keyIcon != null)
			{
				inputHolder.SetP(keyIcon);
				return;
			}
			string text2;
			inputHolder.pad_label = this.getPadDisplayLabelDefault(k, out text2);
			inputHolder.pad_icon = this.pad_input_key_to_icon_index((text2 != "") ? text2 : (TX.valid(inputHolder.pad_label) ? inputHolder.pad_label : k), false);
		}

		public void fineAllInputLabel(bool force_key = false, bool force_pad = false)
		{
			if ((this.fine_input_label & 1) != 0)
			{
				force_key = true;
			}
			if ((this.fine_input_label & 2) != 0)
			{
				force_pad = true;
			}
			if (!force_key && !force_pad)
			{
				return;
			}
			this.fine_input_label = 0;
			int num = 30;
			for (int i = 0; i < num; i++)
			{
				KEY.InputHolder inputHolder = this.AInputs[i];
				if (force_key)
				{
					this.setKeyboardInput((KEY.IPT)i, null);
				}
				if (force_pad)
				{
					this.setPadInput((KEY.IPT)i, null);
				}
			}
		}

		private string getPadDisplayLabelDefault(string pad_raw_input_title, out string dirname)
		{
			return this.getReplacedPadInputTitle(pad_raw_input_title, out dirname, false);
		}

		public string getPadDisplayLabelDefault(string name)
		{
			KEY.IPT ipt;
			if (FEnum<KEY.IPT>.TryParse(name, out ipt, true))
			{
				KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
				string text;
				return this.getPadDisplayLabelDefault(inputHolder.pad_raw_input_title, out text);
			}
			return null;
		}

		public bool clearDefaultLabel(bool key = true, bool pad = true)
		{
			if (!key && !pad)
			{
				return false;
			}
			if (key)
			{
				this.ODefaultK.Clear();
			}
			if (pad)
			{
				this.ODefaultP.Clear();
			}
			this.fine_input_label |= (key ? 1 : 0) | (pad ? 2 : 0);
			return true;
		}

		public int pad_input_key_to_icon_index(string k, bool raw_input = false)
		{
			if (!raw_input && k != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(k);
				if (num <= 1049176909U)
				{
					if (num <= 485174231U)
					{
						if (num <= 297952813U)
						{
							if (num <= 201608140U)
							{
								if (num != 182978943U)
								{
									if (num != 201608140U)
									{
										goto IL_051C;
									}
									if (!(k == "R1"))
									{
										goto IL_051C;
									}
									return 14;
								}
								else
								{
									if (!(k == "Start"))
									{
										goto IL_051C;
									}
									return 2;
								}
							}
							else if (num != 251940997U)
							{
								if (num != 297952813U)
								{
									goto IL_051C;
								}
								if (!(k == "select"))
								{
									goto IL_051C;
								}
								return 2;
							}
							else
							{
								if (!(k == "R2"))
								{
									goto IL_051C;
								}
								return 15;
							}
						}
						else if (num <= 400673902U)
						{
							if (num != 383896283U)
							{
								if (num != 400673902U)
								{
									goto IL_051C;
								}
								if (!(k == "L1"))
								{
									goto IL_051C;
								}
								return 12;
							}
							else
							{
								if (!(k == "L2"))
								{
									goto IL_051C;
								}
								return 13;
							}
						}
						else if (num != 468396612U)
						{
							if (num != 485174231U)
							{
								goto IL_051C;
							}
							if (!(k == "11"))
							{
								goto IL_051C;
							}
							return 10;
						}
						else if (!(k == "10"))
						{
							goto IL_051C;
						}
					}
					else if (num <= 839689206U)
					{
						if (num <= 555218067U)
						{
							if (num != 501951850U)
							{
								if (num != 555218067U)
								{
									goto IL_051C;
								}
								if (!(k == "ZL"))
								{
									goto IL_051C;
								}
								return 13;
							}
							else
							{
								if (!(k == "12"))
								{
									goto IL_051C;
								}
								return 11;
							}
						}
						else if (num != 806133968U)
						{
							if (num != 839689206U)
							{
								goto IL_051C;
							}
							if (!(k == "7"))
							{
								goto IL_051C;
							}
							return 13;
						}
						else
						{
							if (!(k == "5"))
							{
								goto IL_051C;
							}
							return 12;
						}
					}
					else if (num <= 893259077U)
					{
						if (num != 856466825U)
						{
							if (num != 893259077U)
							{
								goto IL_051C;
							}
							if (!(k == "Options"))
							{
								goto IL_051C;
							}
							return 2;
						}
						else
						{
							if (!(k == "6"))
							{
								goto IL_051C;
							}
							return 14;
						}
					}
					else if (num != 1007465396U)
					{
						if (num != 1024243015U)
						{
							if (num != 1049176909U)
							{
								goto IL_051C;
							}
							if (!(k == "Select"))
							{
								goto IL_051C;
							}
							return 2;
						}
						else
						{
							if (!(k == "8"))
							{
								goto IL_051C;
							}
							return 15;
						}
					}
					else if (!(k == "9"))
					{
						goto IL_051C;
					}
					return 2;
				}
				if (num <= 1779989750U)
				{
					if (num <= 1697318111U)
					{
						if (num <= 1334144472U)
						{
							if (num != 1058546637U)
							{
								if (num != 1334144472U)
								{
									goto IL_051C;
								}
								if (!(k == "Share"))
								{
									goto IL_051C;
								}
								return 2;
							}
							else
							{
								if (!(k == "ZR"))
								{
									goto IL_051C;
								}
								return 15;
							}
						}
						else if (num != 1411432780U)
						{
							if (num != 1697318111U)
							{
								goto IL_051C;
							}
							if (!(k == "start"))
							{
								goto IL_051C;
							}
							return 2;
						}
						else
						{
							if (!(k == "leftTrigger"))
							{
								goto IL_051C;
							}
							return 13;
						}
					}
					else if (num <= 1761926707U)
					{
						if (num != 1726105803U)
						{
							if (num != 1761926707U)
							{
								goto IL_051C;
							}
							if (!(k == "RT"))
							{
								goto IL_051C;
							}
							return 15;
						}
						else
						{
							if (!(k == "LB"))
							{
								goto IL_051C;
							}
							return 12;
						}
					}
					else if (num != 1763212131U)
					{
						if (num != 1779989750U)
						{
							goto IL_051C;
						}
						if (!(k == "button12"))
						{
							goto IL_051C;
						}
						return 11;
					}
					else if (!(k == "button11"))
					{
						goto IL_051C;
					}
				}
				else if (num <= 2848586808U)
				{
					if (num <= 2131034325U)
					{
						if (num != 2095213421U)
						{
							if (num != 2131034325U)
							{
								goto IL_051C;
							}
							if (!(k == "RB"))
							{
								goto IL_051C;
							}
							return 14;
						}
						else
						{
							if (!(k == "LT"))
							{
								goto IL_051C;
							}
							return 13;
						}
					}
					else if (num != 2427478531U)
					{
						if (num != 2848586808U)
						{
							goto IL_051C;
						}
						if (!(k == "share"))
						{
							goto IL_051C;
						}
						return 2;
					}
					else
					{
						if (!(k == "rightTrigger"))
						{
							goto IL_051C;
						}
						return 15;
					}
				}
				else if (num <= 3607893173U)
				{
					if (num != 3373006507U)
					{
						if (num != 3607893173U)
						{
							goto IL_051C;
						}
						if (!(k == "R"))
						{
							goto IL_051C;
						}
						return 14;
					}
					else
					{
						if (!(k == "L"))
						{
							goto IL_051C;
						}
						return 12;
					}
				}
				else if (num != 3616964664U)
				{
					if (num != 3792611837U)
					{
						if (num != 4012403877U)
						{
							goto IL_051C;
						}
						if (!(k == "options"))
						{
							goto IL_051C;
						}
						return 2;
					}
					else
					{
						if (!(k == "rightShoulder"))
						{
							goto IL_051C;
						}
						return 14;
					}
				}
				else
				{
					if (!(k == "leftShoulder"))
					{
						goto IL_051C;
					}
					return 12;
				}
				return 10;
			}
			IL_051C:
			if (k != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(k);
				if (num <= 2126255620U)
				{
					if (num <= 881114153U)
					{
						if (num <= 527024624U)
						{
							if (num != 91826501U)
							{
								if (num != 527024624U)
								{
									goto IL_085A;
								}
								if (!(k == "leftStick"))
								{
									goto IL_085A;
								}
								goto IL_0845;
							}
							else
							{
								if (!(k == "rightStick/up"))
								{
									goto IL_085A;
								}
								return 9;
							}
						}
						else if (num != 730684821U)
						{
							if (num != 869412772U)
							{
								if (num != 881114153U)
								{
									goto IL_085A;
								}
								if (!(k == "dpad/right"))
								{
									goto IL_085A;
								}
							}
							else
							{
								if (!(k == "rightStick/down"))
								{
									goto IL_085A;
								}
								return 9;
							}
						}
						else
						{
							if (!(k == "Plus"))
							{
								goto IL_085A;
							}
							return 5;
						}
					}
					else if (num <= 1757831241U)
					{
						if (num != 938826173U)
						{
							if (num != 996955572U)
							{
								if (num != 1757831241U)
								{
									goto IL_085A;
								}
								if (!(k == "rightStick/left"))
								{
									goto IL_085A;
								}
								return 9;
							}
							else if (!(k == "dpad/up"))
							{
								goto IL_085A;
							}
						}
						else
						{
							if (!(k == "leftStick/right"))
							{
								goto IL_085A;
							}
							goto IL_0845;
						}
					}
					else if (num != 1760883434U)
					{
						if (num != 2113007854U)
						{
							if (num != 2126255620U)
							{
								goto IL_085A;
							}
							if (!(k == "dpad"))
							{
								goto IL_085A;
							}
						}
						else
						{
							if (!(k == "rightStickPress"))
							{
								goto IL_085A;
							}
							return 11;
						}
					}
					else
					{
						if (!(k == "rightStick/right"))
						{
							goto IL_085A;
						}
						return 9;
					}
				}
				else if (num <= 3395209529U)
				{
					if (num <= 2803379527U)
					{
						if (num != 2166136261U)
						{
							if (num != 2482153640U)
							{
								if (num != 2803379527U)
								{
									goto IL_085A;
								}
								if (!(k == "rightStick"))
								{
									goto IL_085A;
								}
								return 9;
							}
							else
							{
								if (!(k == "leftStick/left"))
								{
									goto IL_085A;
								}
								goto IL_0845;
							}
						}
						else
						{
							if (k == null)
							{
								goto IL_085A;
							}
							if (k.Length != 0)
							{
								goto IL_085A;
							}
							return 0;
						}
					}
					else if (num != 2991474140U)
					{
						if (num != 3388260431U)
						{
							if (num != 3395209529U)
							{
								goto IL_085A;
							}
							if (!(k == "touchpadButton"))
							{
								goto IL_085A;
							}
							return 17;
						}
						else
						{
							if (!(k == "Minus"))
							{
								goto IL_085A;
							}
							return 4;
						}
					}
					else if (!(k == "dpad/left"))
					{
						goto IL_085A;
					}
				}
				else if (num <= 3924649288U)
				{
					if (num != 3624025213U)
					{
						if (num != 3907550535U)
						{
							if (num != 3924649288U)
							{
								goto IL_085A;
							}
							if (!(k == "leftStick/up"))
							{
								goto IL_085A;
							}
							goto IL_0845;
						}
						else
						{
							if (!(k == "stick"))
							{
								goto IL_085A;
							}
							return 7;
						}
					}
					else
					{
						if (!(k == "leftStick/down"))
						{
							goto IL_085A;
						}
						goto IL_0845;
					}
				}
				else if (num != 4072609730U)
				{
					if (num != 4104197953U)
					{
						if (num != 4285979527U)
						{
							goto IL_085A;
						}
						if (!(k == "leftStickPress"))
						{
							goto IL_085A;
						}
						return 10;
					}
					else if (!(k == "dpad/down"))
					{
						goto IL_085A;
					}
				}
				else if (!(k == "hat"))
				{
					goto IL_085A;
				}
				if (!raw_input)
				{
					return 16;
				}
				return 5;
				IL_0845:
				if (!raw_input)
				{
					return 7;
				}
				return 8;
			}
			IL_085A:
			if (!raw_input)
			{
				return 6;
			}
			return 0;
		}

		public void setKeyboardIcon(string key, int v)
		{
			KEY.IPT ipt;
			if (!FEnum<KEY.IPT>.TryParse(key, out ipt, true))
			{
				return;
			}
			KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
			inputHolder.key_icon = v;
			string keyboard_raw_input_title = inputHolder.keyboard_raw_input_title;
			KEY.KeyIcon keyIcon = X.Get<string, KEY.KeyIcon>(this.ODefaultK, keyboard_raw_input_title);
			if (keyIcon == null)
			{
				keyIcon = (this.ODefaultK[keyboard_raw_input_title] = new KEY.KeyIcon().SetToDef(inputHolder));
			}
			keyIcon.key_icon = v;
			this.fine_input_label |= 1;
		}

		public void setPadIcon(string key, int v)
		{
			KEY.IPT ipt;
			if (!FEnum<KEY.IPT>.TryParse(key, out ipt, true))
			{
				return;
			}
			KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
			inputHolder.pad_icon = v;
			string pad_raw_input_title = inputHolder.pad_raw_input_title;
			KEY.KeyIcon keyIcon = X.Get<string, KEY.KeyIcon>(this.ODefaultP, pad_raw_input_title);
			if (keyIcon == null)
			{
				keyIcon = (this.ODefaultP[pad_raw_input_title] = new KEY.KeyIcon().SetToDef(inputHolder));
			}
			keyIcon.pad_icon = v;
			this.fine_input_label |= 2;
		}

		public void setKeyboardLabel(string key, string v)
		{
			KEY.IPT ipt;
			if (!FEnum<KEY.IPT>.TryParse(key, out ipt, true))
			{
				return;
			}
			KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
			inputHolder.key_label = v;
			string keyboard_raw_input_title = inputHolder.keyboard_raw_input_title;
			KEY.KeyIcon keyIcon = X.Get<string, KEY.KeyIcon>(this.ODefaultK, keyboard_raw_input_title);
			if (keyIcon == null)
			{
				keyIcon = (this.ODefaultK[keyboard_raw_input_title] = new KEY.KeyIcon().SetToDef(inputHolder));
			}
			keyIcon.key_label = v;
			this.fine_input_label |= 1;
		}

		public void setPadLabel(string key, string v)
		{
			KEY.IPT ipt;
			if (!FEnum<KEY.IPT>.TryParse(key, out ipt, true))
			{
				return;
			}
			KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
			inputHolder.pad_label = v;
			string pad_raw_input_title = inputHolder.pad_raw_input_title;
			KEY.KeyIcon keyIcon = X.Get<string, KEY.KeyIcon>(this.ODefaultP, pad_raw_input_title);
			if (keyIcon == null)
			{
				keyIcon = (this.ODefaultP[pad_raw_input_title] = new KEY.KeyIcon().SetToDef(inputHolder));
			}
			keyIcon.pad_label = v;
			this.fine_input_label |= 2;
		}

		public bool isRunningInput(float thresh, bool is_right)
		{
			float num = this.Apad_tilt_level[is_right ? 1 : 0];
			float num2 = ((this.stick_threshold < 0f) ? 0.5f : this.stick_threshold);
			return num > X.Scr(thresh, num2);
		}

		public void getStringForListener(STB Stb)
		{
			int num = 30;
			for (int i = 0; i < num; i++)
			{
				KEY.InputHolder inputHolder = this.AInputs[i];
				Stb.Add(inputHolder.key_label);
				Stb.Add(inputHolder.pad_label);
			}
		}

		public void entryMesh()
		{
		}

		public static void DrawKeyBackIconTo(MeshDrawer Md, float x, float y, float w, string idstr, int pad_mode_auto = -1)
		{
			int keyAssignIconId = IN.getKeyAssignIconId(idstr, pad_mode_auto);
			float num = w * 0.5f * 0.4f;
			if (keyAssignIconId == 0)
			{
				return;
			}
			Md.Identity();
			float base_z = Md.base_z;
			switch (keyAssignIconId)
			{
			case 1:
			case 2:
			case 4:
			case 5:
			{
				float num2 = ((keyAssignIconId == 2) ? 1.1f : 1f);
				float num3 = ((keyAssignIconId == 1) ? 1f : ((keyAssignIconId == 2) ? 0.65f : 0.33f));
				Md.KadomaruRect(x, y, w * num2, w * num3, num, 2f, false, 1f, 1f, false);
				if (keyAssignIconId == 5)
				{
					Md.KadomaruRect(x, y, w * num3, w * num2, num, 2f, false, 1f, 1f, false);
				}
				Md.KadomaruRect(x, y, w * num2 - 2f, w * num3 - 2f, num, 0f, false, 0f, 0f, false);
				if (keyAssignIconId == 5)
				{
					Md.KadomaruRect(x, y, w * num3 - 2f, w * num2 - 2f, num, 0f, false, 0f, 0f, false);
				}
				break;
			}
			case 3:
			{
				float num4 = w * 0.3f;
				num /= 2.5f;
				for (int i = 0; i < 9; i++)
				{
					float num5 = x + w * 0.333f * (float)(i % 3 - 1);
					float num6 = y + w * 0.333f * (float)(i / 3 - 1);
					Md.KadomaruRect(num5, num6, num4, num4, num, 2f, false, 1f, 1f, false);
					Md.KadomaruRect(num5, num6, num4 - 2f, num4 - 2f, num, 0f, false, 0f, 0f, false);
				}
				break;
			}
			case 6:
				w *= 1.125f;
				Md.KadomaruRect(x, y, w, w, num, 2f, false, 1f, 1f, false);
				Md.KadomaruRect(x, y, w - 2f, w - 2f, num, 0f, false, 0f, 0f, false);
				break;
			case 12:
			case 13:
			case 14:
			case 15:
			{
				int num7 = keyAssignIconId % 2;
				break;
			}
			}
			Md.base_z = base_z;
		}

		private bool checkDupe(bool ck, bool cp, KEY.IPT a, KEY.IPT b)
		{
			return this.AInputs[(int)a].isDupe(this.AInputs[(int)b], ck, cp, false);
		}

		private bool checkDupe(bool ck, bool cp, KEY.IPT a, KEY.IPT b, KEY.IPT c)
		{
			return this.AInputs[(int)a].isDupe(this.AInputs[(int)b], ck, cp, false) || this.AInputs[(int)a].isDupe(this.AInputs[(int)c], ck, cp, false);
		}

		public KEY.SIMKEY hasDuplicateInput(bool check_key = true, bool check_pad = true)
		{
			KEY.SIMKEY simkey = (KEY.SIMKEY)0;
			if (this.checkDupe(check_key, check_pad, KEY.IPT.SUBMIT, KEY.IPT.CANCEL, KEY.IPT.CANCEL2) || this.checkDupe(check_key, check_pad, KEY.IPT.SUBMIT2, KEY.IPT.CANCEL, KEY.IPT.CANCEL2))
			{
				simkey |= KEY.SIMKEY.SUBMIT | KEY.SIMKEY.CANCEL;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.Z, KEY.IPT.X))
			{
				simkey |= KEY.SIMKEY.Z | KEY.SIMKEY.X;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.Z, KEY.IPT.JUMP))
			{
				simkey |= KEY.SIMKEY.Z | KEY.SIMKEY.JUMP;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.X, KEY.IPT.JUMP))
			{
				simkey |= KEY.SIMKEY.X | KEY.SIMKEY.JUMP;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.Z, KEY.IPT.LSH))
			{
				simkey |= KEY.SIMKEY.Z | KEY.SIMKEY.LSH;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.X, KEY.IPT.LSH))
			{
				simkey |= KEY.SIMKEY.X | KEY.SIMKEY.LSH;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.JUMP, KEY.IPT.LSH))
			{
				simkey |= KEY.SIMKEY.LSH | KEY.SIMKEY.JUMP;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.LTAB, KEY.IPT.RTAB))
			{
				simkey |= KEY.SIMKEY.LTAB | KEY.SIMKEY.RTAB;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.LTAB, KEY.IPT.SORT))
			{
				simkey |= KEY.SIMKEY.LTAB | KEY.SIMKEY.SORT;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.LTAB, KEY.IPT.SHIFT))
			{
				simkey |= KEY.SIMKEY.LTAB | KEY.SIMKEY.SHIFT;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.LTAB, KEY.IPT.ADD))
			{
				simkey |= KEY.SIMKEY.LTAB | KEY.SIMKEY.ADD;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.LTAB, KEY.IPT.REM))
			{
				simkey |= KEY.SIMKEY.LTAB | KEY.SIMKEY.REM;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.RTAB, KEY.IPT.SORT))
			{
				simkey |= KEY.SIMKEY.RTAB | KEY.SIMKEY.SORT;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.RTAB, KEY.IPT.SHIFT))
			{
				simkey |= KEY.SIMKEY.RTAB | KEY.SIMKEY.SHIFT;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.RTAB, KEY.IPT.ADD))
			{
				simkey |= KEY.SIMKEY.RTAB | KEY.SIMKEY.ADD;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.RTAB, KEY.IPT.REM))
			{
				simkey |= KEY.SIMKEY.RTAB | KEY.SIMKEY.REM;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.SORT, KEY.IPT.SHIFT))
			{
				simkey |= KEY.SIMKEY.SORT | KEY.SIMKEY.SHIFT;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.SORT, KEY.IPT.ADD))
			{
				simkey |= KEY.SIMKEY.SORT | KEY.SIMKEY.ADD;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.SORT, KEY.IPT.REM))
			{
				simkey |= KEY.SIMKEY.SORT | KEY.SIMKEY.REM;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.SHIFT, KEY.IPT.ADD))
			{
				simkey |= KEY.SIMKEY.ADD | KEY.SIMKEY.SHIFT;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.SHIFT, KEY.IPT.REM))
			{
				simkey |= KEY.SIMKEY.REM | KEY.SIMKEY.SHIFT;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.REM, KEY.IPT.ADD))
			{
				simkey |= KEY.SIMKEY.ADD | KEY.SIMKEY.REM;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.X, KEY.IPT.CHECK))
			{
				simkey |= KEY.SIMKEY.X | KEY.SIMKEY.CHECK;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.LA, KEY.IPT.TA))
			{
				simkey |= KEY.SIMKEY.L | KEY.SIMKEY.T;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.LA, KEY.IPT.RA))
			{
				simkey |= KEY.SIMKEY.L | KEY.SIMKEY.R;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.LA, KEY.IPT.BA))
			{
				simkey |= KEY.SIMKEY.L | KEY.SIMKEY.B;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.TA, KEY.IPT.RA))
			{
				simkey |= KEY.SIMKEY.R | KEY.SIMKEY.T;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.TA, KEY.IPT.BA))
			{
				simkey |= KEY.SIMKEY.T | KEY.SIMKEY.B;
			}
			if (this.checkDupe(check_key, check_pad, KEY.IPT.RA, KEY.IPT.BA))
			{
				simkey |= KEY.SIMKEY.R | KEY.SIMKEY.B;
			}
			return simkey;
		}

		public bool isDupe(KEY.IPT ia, KEY.IPT ib, byte is_pad = 2)
		{
			if (is_pad >= 2)
			{
				is_pad = (this.pad_mode ? 1 : 0);
			}
			return this.AInputs[(int)ia].isDupe(this.AInputs[(int)ib], is_pad == 0, is_pad == 1, true);
		}

		public ByteArray getSaveString(ByteArray Ba = null)
		{
			if (Ba == null)
			{
				Ba = new ByteArray(0U);
			}
			int num = 30;
			Ba.writeUInt(308066463U);
			Ba.writeByte(118 | (this.pad_mode ? 1 : 0) | (this.is_xinput ? 8 : 0));
			Ba.writeByte(1);
			Ba.writeString(this.PlayerCon.actions.SaveBindingOverridesAsJson(), "utf-8");
			Ba.writeByte(num);
			for (int i = 0; i < num; i++)
			{
				KEY.InputHolder inputHolder = this.AInputs[i];
				string name = inputHolder.name;
				Ba.writeString(name, "utf-8");
				Ba.writeString(inputHolder.key_label, "utf-8");
				Ba.writeByte(inputHolder.key_icon);
				Ba.writeString(inputHolder.pad_label, "utf-8");
				Ba.writeByte(inputHolder.pad_icon);
			}
			for (int j = 0; j < 2; j++)
			{
				BDic<string, KEY.KeyIcon> bdic = ((j == 0) ? this.ODefaultK : this.ODefaultP);
				using (BList<string> blist = X.objKeysB<string, KEY.KeyIcon>(bdic))
				{
					num = X.Mn(blist.Count, 255);
					Ba.writeByte(num);
					for (int k = 0; k < num; k++)
					{
						KEY.KeyIcon keyIcon = bdic[blist[k]];
						Ba.writeString(blist[k], "utf-8");
						Ba.writeString(keyIcon.key_label, "utf-8");
						Ba.writeByte(keyIcon.key_icon);
						Ba.writeString(keyIcon.pad_label, "utf-8");
						Ba.writeByte(keyIcon.pad_icon);
					}
				}
			}
			Ba.writeFloat(this.stick_threshold);
			return Ba;
		}

		public bool readSaveString(ByteArray Ba, bool apply_pad_mode)
		{
			MTRX.OFontStorage[MTRX.getCabinFont()].Fine();
			try
			{
				if (Ba.readUInt() != 308066463U)
				{
					throw new Exception("Header Error");
				}
				this.fine_input_label = 3;
				int num = Ba.readByte();
				bool flag = (num & 8) != 0;
				bool flag2 = (num & 16) == 0;
				bool flag3 = (num & 32) != 0;
				bool flag4 = (num & 64) != 0;
				int num2 = 0;
				if (flag4)
				{
					num2 = Ba.readByte();
				}
				if (!flag2)
				{
					this.clearKEY(true, true);
					this.is_xinput = flag;
					this.PlayerCon.actions.LoadBindingOverridesFromJson(Ba.readString("utf-8", false), true);
				}
				else
				{
					this.clearKEY(true, false);
					this.is_xinput = true;
				}
				int num3 = Ba.readByte();
				for (int i = 0; i < num3; i++)
				{
					string text = Ba.readString("utf-8", false);
					string text2 = Ba.readString("utf-8", false);
					int num4 = Ba.readByte();
					string text3 = Ba.readString("utf-8", false);
					int num5 = Ba.readByte();
					ushort num6 = 0;
					if (flag2)
					{
						num6 = Ba.readUShort();
						Ba.readString("utf-8", false);
					}
					KEY.IPT ipt;
					if (FEnum<KEY.IPT>.TryParse(text, out ipt, true))
					{
						KEY.InputHolder inputHolder = this.AInputs[(int)ipt];
						if ((num & 4) == 0 && text2 != null)
						{
							uint num7 = <PrivateImplementationDetails>.ComputeStringHash(text2);
							if (num7 > 1565561622U)
							{
								if (num7 <= 2848837449U)
								{
									if (num7 != 2227967961U)
									{
										if (num7 != 2848837449U)
										{
											goto IL_0266;
										}
										if (!(text2 == "LeftArrow"))
										{
											goto IL_0266;
										}
										goto IL_0244;
									}
									else if (!(text2 == "Up Arrow"))
									{
										goto IL_0266;
									}
								}
								else if (num7 != 3583141561U)
								{
									if (num7 != 4104663468U)
									{
										goto IL_0266;
									}
									if (!(text2 == "Down Arrow"))
									{
										goto IL_0266;
									}
									goto IL_025F;
								}
								else if (!(text2 == "UpArrow"))
								{
									goto IL_0266;
								}
								text2 = "↑";
								goto IL_0266;
							}
							if (num7 <= 951029660U)
							{
								if (num7 != 850645478U)
								{
									if (num7 != 951029660U)
									{
										goto IL_0266;
									}
									if (!(text2 == "Right Arrow"))
									{
										goto IL_0266;
									}
								}
								else if (!(text2 == "RightArrow"))
								{
									goto IL_0266;
								}
								text2 = "→";
								goto IL_0266;
							}
							if (num7 != 1383447753U)
							{
								if (num7 != 1565561622U)
								{
									goto IL_0266;
								}
								if (!(text2 == "DownArrow"))
								{
									goto IL_0266;
								}
								goto IL_025F;
							}
							else if (!(text2 == "Left Arrow"))
							{
								goto IL_0266;
							}
							IL_0244:
							text2 = "←";
							goto IL_0266;
							IL_025F:
							text2 = "↓";
						}
						IL_0266:
						if (num2 == 0 && text2 != null && text2 == "Space")
						{
							text2 = "\uff3f";
						}
						inputHolder.key_label = text2;
						inputHolder.key_icon = num4;
						if (flag2)
						{
							KeyCode keyCode = (KeyCode)num6;
							string text4 = keyCode.ToString();
							if (text4 != num6.ToString())
							{
								if (TX.isStart(text4, "Alpha", 0))
								{
									text4 = TX.slice(text4, 5);
								}
								else if (TX.isStart(text4, "Keypad", 0))
								{
									text4 = "numpad" + TX.HeadLower(TX.slice(text4, 6));
								}
								else
								{
									keyCode = (KeyCode)num6;
									if (keyCode != KeyCode.RightControl)
									{
										if (keyCode == KeyCode.LeftControl)
										{
											text4 = "leftCtrl";
										}
										else
										{
											text4 = TX.HeadLower(text4);
										}
									}
									else
									{
										text4 = "rightCtrl";
									}
								}
								inputHolder.writeKey(text4);
							}
						}
						else
						{
							inputHolder.pad_label = text3;
							inputHolder.pad_icon = num5;
						}
						if (ipt == KEY.IPT.SUBMIT && num2 == 0)
						{
							this.AInputs[17].Set(inputHolder);
						}
					}
					else
					{
						X.de("不明なインプット指定子:" + text, null);
					}
				}
				if (num2 == 0)
				{
					this.ODefaultK["Space"] = new KEY.KeyIcon
					{
						key_label = "\uff3f",
						key_icon = 1,
						pad_label = "A",
						pad_icon = 5
					};
				}
				int num8 = 0;
				while (num8 < 2 && Ba.bytesAvailable > 0UL)
				{
					BDic<string, KEY.KeyIcon> bdic = ((num8 == 0) ? this.ODefaultK : this.ODefaultP);
					num3 = Ba.readByte();
					int j = 0;
					while (j < num3)
					{
						KEY.KeyIcon keyIcon = new KEY.KeyIcon();
						string text5 = Ba.readString("utf-8", false);
						keyIcon.key_label = Ba.readString("utf-8", false);
						keyIcon.key_icon = Ba.readByte();
						keyIcon.pad_label = Ba.readString("utf-8", false);
						keyIcon.pad_icon = Ba.readByte();
						if (!flag2)
						{
							goto IL_046B;
						}
						if (TX.isStart(text5, "K_", 0))
						{
							text5 = TX.slice(text5, 2);
							goto IL_046B;
						}
						IL_04A7:
						j++;
						continue;
						IL_046B:
						if (num8 == 0 && text5 == "Space" && keyIcon.key_label == "Space")
						{
							keyIcon.key_label = "";
						}
						bdic[text5] = keyIcon;
						goto IL_04A7;
					}
					if (flag2)
					{
						break;
					}
					num8++;
				}
				if (flag3)
				{
					this.stick_threshold = Ba.readFloat();
				}
			}
			catch (Exception ex)
			{
				X.de("KEY 読み込みエラー発生: " + ex.ToString(), null);
				return false;
			}
			this.changedScheme(null);
			this.fineSubmitAlias();
			return true;
		}

		public static string getRandomKey(int v)
		{
			switch (v)
			{
			case 0:
				return "LA";
			case 1:
				return "TA";
			case 2:
				return "RA";
			case 3:
				return "BA";
			case 4:
				return "Z";
			case 5:
				return "X";
			case 6:
				return "C";
			default:
				return "Z";
			}
		}

		public static string getRandomKeyBit(uint alloc_bit = 63U)
		{
			if (alloc_bit == 63U)
			{
				return KEY.getRandomKey(X.xors(6));
			}
			int num = X.bit_count(alloc_bit);
			if (num <= 0)
			{
				return "Z";
			}
			int num2 = X.xors(num);
			return KEY.getRandomKey(X.bit_on_index(alloc_bit, num2));
		}

		public static uint getRandomKeyBitsLRTB(int count)
		{
			if (count >= 4)
			{
				return 15U;
			}
			int[] array = X.makeCountUpArray(4, 0, 1);
			X.shuffle<int>(array, -1);
			uint num = 0U;
			for (int i = 0; i < count; i++)
			{
				num |= 1U << array[i];
			}
			return num;
		}

		public static MODIF getModifier(Keyboard KC = null)
		{
			if (KC == null)
			{
				KC = Keyboard.current;
			}
			if (KC == null)
			{
				return MODIF.NONE;
			}
			return (KC.leftAltKey.isPressed ? MODIF.OPT : MODIF.NONE) | (KC.leftShiftKey.isPressed ? MODIF.SHIFT : MODIF.NONE) | (KC.leftCommandKey.isPressed ? MODIF.CMD : MODIF.NONE) | (KC.leftCtrlKey.isPressed ? MODIF.CTRL : MODIF.NONE);
		}

		public void addSwitchedDeviceFunc(Action Fn)
		{
			if (Fn != null)
			{
				this.FnSwitchedDevice = (Action)Delegate.Combine(this.FnSwitchedDevice, Fn);
			}
		}

		private KEY.InputHolder[] AInputs;

		private InputAction ActRet;

		private InputAction ActForRebind;

		public float mvSUBMIT;

		public float mvCANCEL;

		public float mvZ;

		public float mvX;

		public float mvC;

		public float mvA;

		public float mvS;

		public float mvD;

		public float mvLSH;

		public float mvJUMP;

		public float mvMENU;

		public float mvCHECK;

		public float mvRUN;

		public float mvLA;

		public float mvRA;

		private int mvLA_out_;

		private int mvRA_out_;

		public float mvTA;

		public float mvBA;

		public float mvM_NEUTRAL;

		public float mvMLA;

		public float mvMRA;

		public float mvMTA;

		public float mvMBA;

		private int text_id;

		public bool turn_lrtb_input;

		public readonly float[] Apad_tilt_level = new float[2];

		public static byte keydesc_appearance;

		public bool is_xinput = true;

		public const int input_keys_game_index = 15;

		private const int CTR_KEY = 0;

		private const int CTR_PAD = 1;

		private BDic<string, KEY.KeyIcon> ODefaultK;

		private BDic<string, KEY.KeyIcon> ODefaultP;

		private byte fine_input_label = 3;

		private KEY.JUMP_OVERRIDE jump_override;

		public const float default_stick_threshold = 0.5f;

		public float stick_threshold = -1f;

		private string pre_scheme = "";

		public Action FnSwitchedDevice = delegate
		{
		};

		private const float pad_tilt_fix_level = 100f;

		public const int randomise_key_max = 6;

		public const uint randomise_key_max_bit = 63U;

		public const uint randomise_key_zx = 48U;

		public const uint randomise_key_z = 16U;

		public const uint randomise_key_ltrb = 15U;

		public delegate void FnRebindListener(string name, bool is_pad, bool is_canceled);

		[Flags]
		public enum SIMKEY
		{
			L = 1,
			R = 2,
			T = 4,
			B = 8,
			LA = 1,
			RA = 2,
			TA = 4,
			BA = 8,
			Z = 16,
			X = 32,
			C = 64,
			LSH = 128,
			ESC = 256,
			SUBMIT = 512,
			CANCEL = 1024,
			LTAB = 2048,
			RTAB = 4096,
			MENU = 8192,
			MAP = 16384,
			ITEM = 32768,
			RUN = 65536,
			JUMP = 131072,
			SORT = 262144,
			ADD = 524288,
			REM = 1048576,
			SHIFT = 2097152,
			CHECK = 4194304,
			_EVHANDLE = 4195840,
			_RANDOMISE = 127
		}

		public enum IPT
		{
			SUBMIT,
			SUBMIT2,
			CANCEL,
			CANCEL2,
			MENU,
			LTAB,
			RTAB,
			SORT,
			SHIFT,
			ADD,
			REM,
			LA,
			TA,
			RA,
			BA,
			JUMP,
			RUN,
			CHECK,
			Z,
			X,
			C,
			A,
			S,
			D,
			LSH,
			M_NEUTRAL,
			MLA,
			MTA,
			MRA,
			MBA,
			_MAX
		}

		private class KeyIcon
		{
			public KEY.KeyIcon Set(KEY.KeyIcon Src)
			{
				this.key_label = Src.key_label;
				this.key_icon = (this.no_icon ? 0 : Src.key_icon);
				this.pad_label = Src.pad_label;
				this.pad_icon = (this.no_icon ? 0 : Src.pad_icon);
				return this;
			}

			public KEY.KeyIcon SetK(KEY.KeyIcon Src)
			{
				this.key_label = Src.key_label;
				this.key_icon = (this.no_icon ? 0 : Src.key_icon);
				return this;
			}

			public KEY.KeyIcon SetP(KEY.KeyIcon Src)
			{
				this.pad_label = Src.pad_label;
				this.pad_icon = (this.no_icon ? 0 : Src.pad_icon);
				return this;
			}

			public KEY.KeyIcon SetToDef(KEY.KeyIcon Src)
			{
				this.key_label = Src.key_label;
				this.pad_label = Src.pad_label;
				if (!Src.no_icon)
				{
					this.key_icon = (this.no_icon ? 0 : Src.key_icon);
					this.pad_icon = (this.no_icon ? 0 : Src.pad_icon);
				}
				return this;
			}

			public bool no_icon;

			public string key_label = "";

			public int key_icon = 1;

			public string pad_label = "";

			public int pad_icon = 5;
		}

		private sealed class InputHolder : KEY.KeyIcon
		{
			public InputHolder(InputAction _Act, bool _no_icon = false)
			{
				this.Act = _Act;
				this.no_icon = _no_icon;
			}

			public void UpdateAction(InputAction _Act)
			{
				this.Act = _Act;
			}

			public string name
			{
				get
				{
					return this.Act.name;
				}
			}

			public bool getBI_KB(out int bi)
			{
				bi = 0;
				return bi >= 0 && this.Act.bindings.Count > bi;
			}

			public bool getBI_Pad(out int bi)
			{
				bi = 1;
				return bi >= 0 && this.Act.bindings.Count > bi;
			}

			public void RemoveOverrides(bool keyboard, bool pad)
			{
				int num;
				if (keyboard && this.getBI_KB(out num))
				{
					InputBinding inputBinding = this.Act.bindings[num];
					inputBinding.overridePath = null;
					this.Act.ApplyBindingOverride(num, inputBinding);
				}
				int num2;
				if (pad && this.getBI_Pad(out num2))
				{
					InputBinding inputBinding2 = this.Act.bindings[1];
					inputBinding2.overridePath = null;
					this.Act.ApplyBindingOverride(num2, inputBinding2);
				}
			}

			public void writeKey(string key)
			{
				int num;
				if (this.getBI_KB(out num))
				{
					InputBinding inputBinding = this.Act.bindings[num];
					if (key != null)
					{
						inputBinding.overridePath = "<Keyboard>/" + key;
						this.Act.ApplyBindingOverride(num, inputBinding);
					}
				}
			}

			public string keyboard_raw_input_title
			{
				get
				{
					int num;
					if (this.getBI_KB(out num))
					{
						string text;
						string text2;
						return this.Act.GetBindingDisplayString(num, out text, out text2, (InputBinding.DisplayStringOptions)0).Replace(" ", "");
					}
					return null;
				}
			}

			public void writePad(string key)
			{
				int num;
				if (this.getBI_Pad(out num))
				{
					InputBinding inputBinding = this.Act.bindings[num];
					if (key == null)
					{
						return;
					}
					inputBinding.overridePath = ((key.IndexOf(">/") >= 0) ? key : ("<Gamepad>/" + key));
					this.Act.ApplyBindingOverride(num, inputBinding);
				}
			}

			public static bool JumpInputIsSame(KEY.InputHolder Jump, KEY.InputHolder HdSrc, bool is_pad)
			{
				int num3;
				int num4;
				if (is_pad)
				{
					int num;
					int num2;
					if (Jump.getBI_Pad(out num) && HdSrc.getBI_Pad(out num2))
					{
						return Jump.Act.bindings[num].effectivePath == HdSrc.Act.bindings[num2].effectivePath;
					}
				}
				else if (Jump.getBI_KB(out num3) && HdSrc.getBI_KB(out num4))
				{
					return Jump.Act.bindings[num3].effectivePath == HdSrc.Act.bindings[num4].effectivePath;
				}
				return false;
			}

			public string pad_raw_input_title
			{
				get
				{
					int num;
					if (!this.getBI_Pad(out num))
					{
						return null;
					}
					string text;
					string text2;
					string bindingDisplayString = this.Act.GetBindingDisplayString(num, out text, out text2, (InputBinding.DisplayStringOptions)0);
					if (text2 != null && bindingDisplayString != null)
					{
						if (text2 != null)
						{
							uint num2 = <PrivateImplementationDetails>.ComputeStringHash(text2);
							if (num2 <= 2139186104U)
							{
								if (num2 <= 1411432780U)
								{
									if (num2 != 297952813U)
									{
										if (num2 != 1411432780U)
										{
											return text2;
										}
										if (!(text2 == "leftTrigger"))
										{
											return text2;
										}
									}
									else if (!(text2 == "select"))
									{
										return text2;
									}
								}
								else if (num2 != 1697318111U)
								{
									if (num2 != 2106031334U)
									{
										if (num2 != 2139186104U)
										{
											return text2;
										}
										if (!(text2 == "buttonEast"))
										{
											return text2;
										}
									}
									else if (!(text2 == "buttonSouth"))
									{
										return text2;
									}
								}
								else if (!(text2 == "start"))
								{
									return text2;
								}
							}
							else if (num2 <= 2866927184U)
							{
								if (num2 != 2427478531U)
								{
									if (num2 != 2866927184U)
									{
										return text2;
									}
									if (!(text2 == "buttonNorth"))
									{
										return text2;
									}
								}
								else if (!(text2 == "rightTrigger"))
								{
									return text2;
								}
							}
							else if (num2 != 3616964664U)
							{
								if (num2 != 3792611837U)
								{
									if (num2 != 4167788306U)
									{
										return text2;
									}
									if (!(text2 == "buttonWest"))
									{
										return text2;
									}
								}
								else if (!(text2 == "rightShoulder"))
								{
									return text2;
								}
							}
							else if (!(text2 == "leftShoulder"))
							{
								return text2;
							}
							return bindingDisplayString;
						}
						return text2;
					}
					return text2 ?? bindingDisplayString;
				}
			}

			public void copyFrom(KEY.InputHolder Src)
			{
				base.Set(Src);
				for (int i = 0; i < 2; i++)
				{
					InputBinding inputBinding = Src.Act.bindings[i];
					this.Act.ApplyBindingOverride(i, Src.Act.bindings[i]);
				}
			}

			public void RebindUpdate(InputAction Src, bool is_pad)
			{
				int num2;
				if (!is_pad)
				{
					int num;
					if (this.getBI_KB(out num))
					{
						InputBinding inputBinding = this.Act.bindings[num];
						inputBinding.overridePath = Src.bindings[0].effectivePath;
						this.Act.ApplyBindingOverride(num, inputBinding);
						return;
					}
				}
				else if (this.getBI_Pad(out num2))
				{
					InputBinding inputBinding2 = this.Act.bindings[num2];
					inputBinding2.overridePath = Src.bindings[0].effectivePath;
					this.Act.ApplyBindingOverride(num2, inputBinding2);
				}
			}

			public bool isOn(bool ignore_thresh = false)
			{
				if (!ignore_thresh && this.stick_check_threshold >= 0f)
				{
					return this.Act.ReadValue<float>() >= this.stick_check_threshold;
				}
				return this.Act.IsPressed();
			}

			public bool isPD()
			{
				return this.Act.WasPressedThisFrame();
			}

			public bool isPDs()
			{
				if (this.stick_check_threshold >= 0f)
				{
					return this.Act.ReadValue<float>() >= this.stick_check_threshold;
				}
				return this.Act.WasPressedThisFrame();
			}

			public float readStickTilt()
			{
				return this.Act.ReadValue<float>();
			}

			public string pad_raw_path
			{
				get
				{
					int num;
					if (this.getBI_Pad(out num))
					{
						return this.Act.bindings[num].effectivePath;
					}
					return "";
				}
			}

			public string kb_raw_path
			{
				get
				{
					int num;
					if (this.getBI_KB(out num))
					{
						return this.Act.bindings[num].effectivePath;
					}
					return "";
				}
			}

			public bool isDupe(KEY.InputHolder Src, bool ck, bool cp, bool check_effective_path = false)
			{
				for (int i = 0; i < 2; i++)
				{
					if (!((i == 0) ? (!ck) : (!cp)))
					{
						InputBinding inputBinding = this.Act.bindings[i];
						InputBinding inputBinding2 = Src.Act.bindings[i];
						if (check_effective_path ? (inputBinding.effectivePath == inputBinding2.effectivePath) : (inputBinding.overridePath == inputBinding2.overridePath))
						{
							return true;
						}
					}
				}
				return false;
			}

			public void copyPathTo(ref InputBinding Dest, bool is_pad)
			{
				int num2;
				if (!is_pad)
				{
					int num;
					if (this.getBI_KB(out num))
					{
						Dest.overridePath = this.Act.bindings[num].effectivePath;
						return;
					}
				}
				else if (this.getBI_Pad(out num2))
				{
					Dest.overridePath = this.Act.bindings[num2].effectivePath;
				}
			}

			public void finePressThreshold(float stick_press_point)
			{
				int num;
				if (this.getBI_Pad(out num))
				{
					string effectivePath = this.Act.bindings[num].effectivePath;
					if (effectivePath.IndexOf("stick") >= 0 || effectivePath.IndexOf("Stick") >= 0)
					{
						this.stick_check_threshold = X.Mx(0.001f, stick_press_point);
						return;
					}
					this.stick_check_threshold = -1f;
				}
			}

			public InputAction Act;

			private float stick_check_threshold = -1f;
		}

		private enum JUMP_OVERRIDE : byte
		{
			KB_TA = 1,
			KB_BA,
			PAD_TA = 4,
			PAD_BA = 8
		}
	}
}
