using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XX
{
	public class LabeledInputField : aBtn
	{
		public static bool focus_exist
		{
			get
			{
				return LabeledInputField.focus_exist_t > 0f;
			}
		}

		protected override void StartBtn()
		{
			base.StartBtn();
			base.addDownFn(new FnBtnBindings(this.fnFocusOnClick));
			if (LabeledInputField.Border == null)
			{
				LabeledInputField.Border = new RectOffset(1, 1, 1, 1);
			}
			if (this.text_pre_saved_for_numeric == "")
			{
				this.text_pre_saved_for_numeric = this.text;
			}
			this.text_pre = this.text;
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			if (this.w <= 0f)
			{
				this.w = 240f;
			}
			if (this.h <= 0f)
			{
				this.h = (float)this.size * 0.95f * (float)this.multi_line + (float)this.size * 0.6f;
			}
			if (this.LbSkin == null)
			{
				string skin = this.skin;
				if (skin != null && skin == "qs")
				{
					this.LbSkin = new ButtonSkinForLabelQsSelector(this, this.w, this.h);
				}
				if (this.LbSkin == null)
				{
					this.LbSkin = new ButtonSkinForLabel(this, this.w, this.h);
				}
			}
			this.LbSkin.setTitle(this.label);
			this.Skin = this.LbSkin;
			return this.LbSkin;
		}

		public string label
		{
			get
			{
				return this.title;
			}
			set
			{
				this.title = value;
				if (this.LbSkin != null)
				{
					this.LbSkin.setTitle(this.title);
				}
			}
		}

		public override void OnPointerEnter()
		{
			this.hover_to_select = false;
			base.OnPointerEnter();
		}

		private bool fnFocusOnClick(aBtn B)
		{
			this.ExecuteOnSubmitKey();
			return true;
		}

		public override void ExecuteOnSubmitKey()
		{
			if (this.focused <= 0)
			{
				this.Focus();
			}
		}

		public LabeledInputField setWH(float _w, float _h = 0f)
		{
			if (_w != 0f)
			{
				this.w = _w;
			}
			if (_h != 0f)
			{
				this.h = _h;
			}
			return this.setLabel(null);
		}

		public LabeledInputField setBoundsWH(float _w, float _h = 0f)
		{
			if (this.LbSkin == null)
			{
				this.setLabel(this.label);
			}
			if (_w != 0f)
			{
				this.w = _w - this.LbSkin.getTextSwidth();
			}
			if (_h != 0f)
			{
				this.h = _h;
			}
			return this.setLabel(null);
		}

		public LabeledInputField setLabel(string _string = null)
		{
			if (_string != null)
			{
				this.label = _string;
			}
			if (this.HovCurs == null)
			{
				base.setHoverManager(new HoverCursManager("CARRET", "tl_carret"));
			}
			if (this.LbSkin == null)
			{
				this.makeButtonSkin(this.skin);
			}
			else
			{
				this.LbSkin.WHPx(this.w, this.h).setTitle(this.label);
			}
			this.Skin = this.LbSkin;
			return this;
		}

		private void OnGUI()
		{
			if (!this.isGUIactive() || this.LbSkin == null || !base.isActive())
			{
				this.gui_text = null;
				return;
			}
			bool flag = false;
			Event current = Event.current;
			if (this.multi_line <= 1 && current.type == EventType.KeyDown && this.return_blur && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter))
			{
				this.Blur();
				this.runFn(this.AFnReturn);
				IN.clearPushDown(true);
				return;
			}
			if (current.type == EventType.KeyDown)
			{
				if (current.keyCode == KeyCode.Escape)
				{
					this.Blur();
					IN.clearPushDown(true);
					return;
				}
				if (current.command && current.keyCode != KeyCode.X && current.keyCode != KeyCode.V && current.keyCode != KeyCode.Z && current.keyCode != KeyCode.Y)
				{
					flag = true;
				}
				else if (!this.runFn(this.AFnInputtingKeyDown, current.keyCode))
				{
					flag = true;
				}
			}
			if (this.BaseCamera == null)
			{
				this.BaseCamera = IN.getGUICamera();
			}
			if (LabeledInputField.GUISkin == null)
			{
				LabeledInputField.GUISkin = new GUIStyle(GUI.skin.textField);
			}
			this.Stl = LabeledInputField.GUISkin;
			this.Stl.fontSize = (int)((float)this.size * (IN.screen_ortho_flag ? 1f : IN.pixel_scale));
			this.Stl.font = TX.getDefaultFont().Target;
			if (this.multi_line > 1)
			{
				this.Stl.alignment = ((this.alignx_ == ALIGN.LEFT) ? TextAnchor.UpperLeft : ((this.alignx_ == ALIGN.CENTER) ? TextAnchor.UpperCenter : TextAnchor.UpperRight));
			}
			else
			{
				this.Stl.alignment = ((this.alignx_ == ALIGN.LEFT) ? TextAnchor.MiddleLeft : ((this.alignx_ == ALIGN.CENTER) ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight));
			}
			if (this.gui_text == null)
			{
				this.gui_text = this.text_;
			}
			string name = base.gameObject.name;
			GUI.SetNextControlName(name);
			Rect fieldBounds = this.LbSkin.getFieldBounds(true);
			string text;
			if (this.multi_line > 1 || !this.return_blur)
			{
				text = ((this.max_len >= 0) ? GUI.TextArea(fieldBounds, this.gui_text, this.Stl) : GUI.TextArea(fieldBounds, this.gui_text, this.max_len, this.Stl));
			}
			else
			{
				text = ((this.max_len >= 0) ? GUI.TextField(fieldBounds, this.gui_text, this.Stl) : GUI.TextField(fieldBounds, this.gui_text, this.max_len, this.Stl));
			}
			if (flag)
			{
				text = this.gui_text;
			}
			if (this.focused >= 2)
			{
				GUI.FocusControl(name);
			}
			if (this.focused == 3 && GUI.GetNameOfFocusedControl() == name)
			{
				TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
				textEditor.OnFocus();
				textEditor.cursorIndex = 0;
				textEditor.selectIndex = text.Length;
			}
			if (this.text != text)
			{
				bool flag2 = false;
				if (this.multi_line == 1 && text.IndexOf("\n") >= 0)
				{
					text = text.Replace("\n", "");
					flag2 = true;
				}
				if (this.focused <= 0)
				{
					this.Focus();
				}
				if (!base.isLocked() && this.LbSkin.CharInputted(text, this.gui_text))
				{
					this.gui_text = text;
					if (this.runFn(this.AFnChanged))
					{
						this.text = text;
						this.need_check_numeric = true;
						this.changed_delay = this.changed_delay_maxt;
						this.need_alloc_chars = true;
					}
				}
				if (flag2)
				{
					this.Blur();
				}
			}
			if (this.focused > 1)
			{
				this.focused = 1;
			}
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (!base.isActive())
			{
				return true;
			}
			if (this.focused > 0)
			{
				if (!this.isPushed() && IN.isMousePushDown(1))
				{
					this.Blur();
				}
				else if (IN.isCancel())
				{
					this.Blur();
				}
				else
				{
					LabeledInputField.focus_exist_t = 3f;
				}
			}
			else if (!base.isFocused() && (base.isSelected() || base.isHovered()) && IN.isSubmit())
			{
				this.SelectAndFocus();
			}
			if (this.changed_delay > 0)
			{
				if (this.changed_delay == 1)
				{
					this.executeChangedDelay();
				}
				else
				{
					this.changed_delay--;
				}
			}
			return true;
		}

		protected override void simulateNaviTranslation(int aim = -1)
		{
			if (this.focused > 0)
			{
				return;
			}
			base.simulateNaviTranslation(aim);
		}

		protected void executeChangedDelay()
		{
			if (this.changed_delay > 0)
			{
				this.changed_delay = 0;
				this.checkAllocChars();
				this.checkNumeric(true);
				if (!this.runFn(this.AFnChangedDelay))
				{
					this.text = this.text_pre;
					return;
				}
				this.text_pre_saved_for_numeric = this.text;
			}
		}

		public void SelectAndFocus()
		{
			if (!base.isActive())
			{
				return;
			}
			base.Select(true);
			this.ExecuteOnSubmitKey();
			if (this.focused == 2)
			{
				this.focused = 3;
			}
		}

		protected void Focus()
		{
			if (this.focused > 0 || !base.isActive())
			{
				return;
			}
			if (LabeledInputField.CurFocused != null && LabeledInputField.CurFocused != this)
			{
				LabeledInputField.CurFocused.Blur();
			}
			LabeledInputField.CurFocused = this;
			LabeledInputField.focus_exist_t = 3f;
			this.text_pre = this.text;
			this.focused = 2;
			this.runFn(this.AFnFocus);
			IN.clearPushDown(false);
			X.SCLOCK("INPUTFIELD");
			base.fine_flag = true;
		}

		public void Blur()
		{
			this.executeChangedDelay();
			if (this.focused > 0)
			{
				GUI.FocusControl("");
				this.focused = 0;
				base.fine_flag = true;
				this.gui_text = null;
				IN.clearPushDown(false);
				X.REMLOCK("INPUTFIELD");
				if (!this.alloc_empty && this.text == "")
				{
					this.text = this.text_pre;
				}
				else if (!this.runFn(this.AFnBlur))
				{
					this.text = this.text_pre;
				}
				else
				{
					this.text = this.text;
				}
				this.LbSkin.setTitle(this.label);
			}
			if (LabeledInputField.CurFocused == this)
			{
				LabeledInputField.CurFocused = null;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (LabeledInputField.CurFocused == this)
			{
				LabeledInputField.CurFocused = null;
			}
		}

		public void blurDecide()
		{
			if (this.need_alloc_chars)
			{
				this.checkAllocChars();
			}
			if (this.need_check_numeric)
			{
				this.checkNumeric(true);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			this.Blur();
			this.blurDecide();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (this.LbSkin != null)
			{
				this.LbSkin.setTitle(this.label);
			}
		}

		public string checkAllocChars()
		{
			if (this.RegAllocChar == null || !this.need_alloc_chars)
			{
				return this.text;
			}
			char[] array = this.text.ToCharArray();
			int num = array.Length;
			int num2 = num;
			for (int i = num - 1; i >= 0; i--)
			{
				char c = array[i];
				if (!this.RegAllocChar.Match(c.ToString()).Success)
				{
					X.shiftNotInput<char>(array, 1, i, num);
					num--;
				}
			}
			if (num < num2)
			{
				Array.Resize<char>(ref array, num);
				this.text = new string(array);
			}
			this.need_alloc_chars = false;
			this.need_check_numeric = true;
			return this.text;
		}

		public string checkNumeric(bool changing_text = false)
		{
			if (!this.alloc_empty && this.text == "")
			{
				this.text = this.text_pre_saved_for_numeric;
			}
			if (!this.integer && !this.number_mode && !this.hex_integer)
			{
				return this.text;
			}
			if (!this.need_check_numeric)
			{
				if (changing_text)
				{
					this.text = this.text_pre_saved_for_numeric;
				}
				return this.text_pre_saved_for_numeric;
			}
			string text = this.text;
			if (this.hex_integer || this.integer)
			{
				try
				{
					if (text == "")
					{
						text = this.text_pre_saved_for_numeric;
					}
					this.text_pre_saved_for_numeric = (this.hex_integer ? ((uint)X.MMX(this.min, uint.Parse(text, NumberStyles.AllowHexSpecifier), this.max)).ToString("x") : ((int)X.MMX(this.min, (double)int.Parse(text), this.max)).ToString());
					goto IL_013F;
				}
				catch
				{
					goto IL_013F;
				}
			}
			if (this.number_mode)
			{
				try
				{
					if (text == "")
					{
						text = this.text_pre_saved_for_numeric;
					}
					double num = double.Parse(text, CultureInfo.InvariantCulture);
					this.text_pre_saved_for_numeric = X.MMX(this.min, num, this.max).ToString();
				}
				catch
				{
				}
			}
			IL_013F:
			if (changing_text)
			{
				this.text = this.text_pre_saved_for_numeric;
			}
			this.need_check_numeric = false;
			return this.text_pre_saved_for_numeric;
		}

		public LabeledInputField addChangedFn(FnFldBindings Fn)
		{
			return this.addFn(ref this.AFnChanged, Fn);
		}

		public LabeledInputField addChangedDelayFn(FnFldBindings Fn)
		{
			return this.addFn(ref this.AFnChangedDelay, Fn);
		}

		public LabeledInputField addFocusFn(FnFldBindings Fn)
		{
			return this.addFn(ref this.AFnFocus, Fn);
		}

		public LabeledInputField addBlurFn(FnFldBindings Fn)
		{
			return this.addFn(ref this.AFnBlur, Fn);
		}

		public LabeledInputField addReturnFn(FnFldBindings Fn)
		{
			return this.addFn(ref this.AFnReturn, Fn);
		}

		public LabeledInputField addInputtingKeyDown(FnFldKeyInputBindings Fn)
		{
			aBtn.addFnT<FnFldKeyInputBindings>(ref this.AFnInputtingKeyDown, Fn);
			return this;
		}

		protected LabeledInputField addFn(ref FnFldBindings[] AFn, FnFldBindings Fn)
		{
			aBtn.addFnT<FnFldBindings>(ref AFn, Fn);
			return this;
		}

		protected bool runFn(FnFldBindings[] AFn)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				FnFldBindings fnFldBindings = AFn[i];
				if (fnFldBindings == null)
				{
					return flag;
				}
				flag = fnFldBindings(this) && flag;
			}
			return flag;
		}

		protected bool runFn(FnFldKeyInputBindings[] AFn, KeyCode key)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				FnFldKeyInputBindings fnFldKeyInputBindings = AFn[i];
				if (fnFldKeyInputBindings == null)
				{
					return flag;
				}
				flag = fnFldKeyInputBindings(this, key) && flag;
				if (!flag)
				{
					break;
				}
			}
			return flag;
		}

		public override void bind()
		{
			ButtonSkin skin = this.Skin;
			base.bind();
		}

		public override void hide()
		{
			this.checkAllocChars();
			this.OnDisable();
			base.hide();
		}

		public override string getValueString()
		{
			this.checkAllocChars();
			this.blurDecide();
			return this.text;
		}

		public bool use_desc
		{
			get
			{
				return this.LbSkin.desc_aim_bit != 0;
			}
			set
			{
				if (value)
				{
					this.LbSkin.desc_aim_bit = this.desc_aim_bit;
				}
				else
				{
					this.LbSkin.desc_aim_bit = 0;
				}
				if (this.LbSkin.isDescShowing())
				{
					this.LbSkin.fine_flag = true;
				}
			}
		}

		public override void setValue(string s)
		{
			if (this.gui_text == null && this.LbSkin != null && !this.LbSkin.CharInputted(s, this.text))
			{
				return;
			}
			this.text = s;
			if (this.gui_text != null)
			{
				this.gui_text = s;
			}
			this.need_alloc_chars = (this.need_check_numeric = true);
			this.checkAllocChars();
			this.checkNumeric(true);
			this.text_pre = this.text_;
		}

		public void setValue(string s, bool call_changed_delay, bool force_fine_skin_text = false)
		{
			if (this.gui_text == null && this.LbSkin != null && !this.LbSkin.CharInputted(s, this.text_))
			{
				return;
			}
			this.text = s;
			if (this.gui_text != null)
			{
				this.gui_text = s;
			}
			if (force_fine_skin_text && this.isGUIactive())
			{
				this.LbSkin.setText(s);
			}
			this.need_alloc_chars = (this.need_check_numeric = true);
			this.checkAllocChars();
			this.checkNumeric(true);
			if (!this.isGUIactive())
			{
				this.text_pre = this.text_;
			}
			if (call_changed_delay)
			{
				this.changed_delay = 1;
				this.executeChangedDelay();
			}
		}

		public string text
		{
			get
			{
				return this.text_;
			}
			set
			{
				if (this.gui_text == null && this.LbSkin != null && !this.LbSkin.CharInputted(value, this.text_))
				{
					return;
				}
				this.text_ = value;
				if (this.LbSkin != null && !this.isGUIactive())
				{
					this.LbSkin.setText(this.text_);
				}
			}
		}

		public ALIGN alignx
		{
			get
			{
				return this.alignx_;
			}
			set
			{
				if (this.alignx == value)
				{
					return;
				}
				this.alignx_ = value;
				if (this.LbSkin != null)
				{
					this.LbSkin.fine_flag = true;
					this.LbSkin.fineAlign();
				}
			}
		}

		public override aBtn SetLocked(bool f, bool fine_flag = true, bool no_change_binding = false)
		{
			return base.SetLocked(f, fine_flag, true);
		}

		public bool isGUIactive()
		{
			return (base.isHovered() || this.focused > 0) && base.isActive() && base.enabled;
		}

		public override float get_swidth_px()
		{
			if (this.Skin == null)
			{
				return this.w;
			}
			return this.Skin.swidth;
		}

		public override float get_sheight_px()
		{
			if (this.Skin == null)
			{
				return this.h;
			}
			return this.Skin.sheight;
		}

		public int max_len = -1;

		public bool label_top;

		private string text_ = "";

		public int size = 20;

		public Camera BaseCamera;

		public Regex RegAllocChar;

		public int multi_line = 1;

		public bool integer;

		public bool hex_integer;

		public bool number_mode;

		public bool return_blur = true;

		public bool alloc_empty = true;

		public double min = -2147483648.0;

		public double max = 2147483647.0;

		public const int TX_MARGIN = 6;

		private ALIGN alignx_ = ALIGN.LEFT;

		public string text_pre = "";

		public int desc_aim_bit = 10;

		private FnFldBindings[] AFnChanged;

		private FnFldBindings[] AFnChangedDelay;

		private FnFldBindings[] AFnBlur;

		private FnFldBindings[] AFnReturn;

		private FnFldKeyInputBindings[] AFnInputtingKeyDown;

		private FnFldBindings[] AFnFocus;

		private int focused;

		private int changed_delay;

		public int changed_delay_maxt = 80;

		private bool need_alloc_chars;

		private bool need_check_numeric = true;

		private string gui_text;

		private string text_pre_saved_for_numeric = "";

		protected ButtonSkinForLabel LbSkin;

		public static RectOffset Border;

		public GUIStyle Stl;

		public static LabeledInputField CurFocused;

		public static float focus_exist_t;

		private static GUIStyle GUISkin;
	}
}
