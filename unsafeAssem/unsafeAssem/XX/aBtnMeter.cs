using System;
using UnityEngine;

namespace XX
{
	public class aBtnMeter : aBtn
	{
		protected override void Awake()
		{
			base.Awake();
			base.addOutFn(delegate(aBtn B)
			{
				(B as aBtnMeter).t_submit_holding = 0f;
				return true;
			});
		}

		public aBtnMeter initMeter(float _minval, float _maxval, float _valintv, float _def, float width_pixel = 0f)
		{
			if (this.w <= 0f)
			{
				this.w = 64f;
			}
			this.minval = _minval;
			this.maxval = _maxval;
			this.valintv = _valintv;
			this.default_value = _def;
			this.w = ((width_pixel > 0f) ? width_pixel : this.w);
			this.valcnt = (this.maxval - this.minval) / this.valintv;
			if (this.split_integer)
			{
				this.valcnt = (float)X.Int(this.valcnt);
				this.valrange = this.valintv * this.valcnt;
			}
			else
			{
				this.valrange = this.maxval - this.minval;
			}
			this.curind = X.MMX(0f, this.valueToIndex(this.default_value), this.valcnt);
			if (this.split_integer)
			{
				this.curind = (float)X.IntR(this.curind);
			}
			if (this.SkinMt != null)
			{
				this.SkinMt.clearMemoriCache();
				this.setValue(this.getValue(), false);
			}
			else if (this.minval == (float)((int)this.minval) && this.maxval == (float)((int)this.maxval) && this.valintv == (float)((int)this.valintv))
			{
				this.return_integer = true;
			}
			base.fine_flag = true;
			return this;
		}

		public aBtnMeter initAsCheckBox(string[] _Adesc_keys, int _def, float width_pixel = 0f, bool force_multi_checker = false)
		{
			this.split_integer = true;
			this.initMeter(0f, (float)(_Adesc_keys.Length - 1), 1f, (float)_def, 0f);
			this.Adesc_keys = _Adesc_keys;
			this.checkbox_mode = (force_multi_checker ? 2 : 1);
			return this;
		}

		public override ButtonSkin initializeSkin(string _skin, string _title = "")
		{
			this.px_bar_width = X.Mn(this.w, this.px_bar_width);
			if (this.valcnt == 0f)
			{
				this.initMeter(this.minval, this.maxval, this.valintv, this.default_value, this.w);
			}
			return base.initializeSkin(_skin, _title);
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			this.click_snd = "";
			if (key != null && key == "scroll")
			{
				return this.SkinMt = new ButtonSkinMeterScroll(this, this.w, this.h);
			}
			if (this.checkbox_mode > 0)
			{
				return this.SkinMt = new ButtonSkinMeterAsCheckBox(this, this.w, this.h, this.checkbox_mode == 2);
			}
			return this.SkinMt = new ButtonSkinMeterNormal(this, this.w, this.h);
		}

		public aBtn addChangedFn(aBtnMeter.FnMeterBindings Fn)
		{
			aBtn.addFnT<aBtnMeter.FnMeterBindings>(ref this.AFnValChanged, Fn);
			return this;
		}

		private bool runFnMeterBindings(aBtnMeter.FnMeterBindings[] AFn, float pre_value, float cur_value)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				aBtnMeter.FnMeterBindings fnMeterBindings = AFn[i];
				if (fnMeterBindings == null)
				{
					return flag;
				}
				flag = fnMeterBindings(this, pre_value, cur_value) && flag;
			}
			return flag;
		}

		public CtSetterMeter createSetter(CtSetterMeter Assign = null)
		{
			if (this.Setter != null)
			{
				return this.Setter;
			}
			this.Setter = Assign ?? new CtSetterMeter(this);
			this.z_push_click = true;
			base.addDownFn(new FnBtnBindings(this.deactivateSetter));
			return this.Setter;
		}

		private bool deactivateSetter(aBtn B = null)
		{
			if (this.Setter != null && this.Setter.isActive())
			{
				this.Setter.deactivate();
			}
			return true;
		}

		public override void ExecuteOnSubmitKey()
		{
			if (this.Setter == null)
			{
				if (this.submit_holding)
				{
					this.t_submit_holding = -1f;
					this.SkinMt.setCursLevelToMeterPos();
					SND.Ui.play("tool_drag_init", false);
					return;
				}
				base.ExecuteOnSubmitKey();
				return;
			}
			else
			{
				if (!this.Setter.isActive())
				{
					base.Deselect(true);
					this.Setter.activate(true);
					return;
				}
				if (!this.active_drag && this.auto_setter_focus)
				{
					base.ExecuteOnSubmitKey();
					return;
				}
				this.Setter.deactivate();
				base.Select(false);
				return;
			}
		}

		protected override void simulateNaviTranslation(int aim = -1)
		{
			if (this.auto_setter_focus && this.Setter != null)
			{
				if (aim == 2 || IN.isR())
				{
					this.simulateLRtoSetter(this, AIM.R);
					return;
				}
				if (aim == 0 || IN.isL())
				{
					this.simulateLRtoSetter(this, AIM.L);
					return;
				}
			}
			else if (this.t_submit_holding_ != 0f)
			{
				int num = 0;
				if (aim == 2 || IN.isR())
				{
					num++;
				}
				if (aim == 0 || IN.isL())
				{
					num--;
				}
				if (num != 0)
				{
					float currentIndex = this.getCurrentIndex();
					float num2 = X.MMX(0f, currentIndex + (float)num, this.valcnt);
					if (currentIndex != num2)
					{
						this.changingTo(num2);
					}
				}
				if (aim == 1 || aim == 3 || IN.isT() || IN.isB())
				{
					aBtnMeter.submitHodingCancel();
				}
				return;
			}
			base.simulateNaviTranslation(aim);
		}

		public bool simulateLRtoSetter(aBtn B, AIM a)
		{
			return this.simulateLRtoSetter(B, a, false);
		}

		public bool simulateLRtoSetter(aBtn B, AIM a, bool execute_meter_slide)
		{
			if (this.Setter == null)
			{
				return false;
			}
			if (a == AIM.R || a == AIM.L)
			{
				IN.use_mouse = false;
				if (!this.Setter.isActive())
				{
					this.Setter.activate(false);
				}
				this.Setter.auto_selected = true;
				if (execute_meter_slide)
				{
					this.Setter.executeSlide((float)((a == AIM.R) ? 1 : (-1)));
				}
				return true;
			}
			return false;
		}

		public override void hide()
		{
			base.hide();
			this.t_submit_holding = 0f;
			this.deactivateSetter(null);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.deactivateSetter(null);
			if (this.Setter != null)
			{
				this.Setter.destruct();
				this.Setter = null;
			}
		}

		public float getValue()
		{
			return this.indexToValue(this.curind);
		}

		public virtual void setValue(float v, bool no_fine = false)
		{
			this.curind = this.valueToIndex(v);
			if (!no_fine)
			{
				base.fine_flag = true;
				if (this.Setter != null)
				{
					this.Setter.fineValue(v.ToString(), false);
				}
			}
		}

		public bool setValueAndCallFunc(float v, bool no_fine = false)
		{
			if (!this.runFnMeterBindings(this.AFnValChanged, this.indexToValue(this.curind), v))
			{
				return false;
			}
			this.setValue(v, base.fine_flag);
			return true;
		}

		public float valueToLevel(float _val)
		{
			return X.MMX(0f, (_val - this.minval) / this.valrange, 1f);
		}

		private float valueToIndex(float _val)
		{
			return this.levelToIndex(this.valueToLevel(_val));
		}

		private float levelToIndex(float _level)
		{
			if (!this.split_integer)
			{
				return _level * this.valcnt;
			}
			return (float)Mathf.RoundToInt(_level * this.valcnt);
		}

		public float levelToValue(float _level)
		{
			return this.indexToValue(this.levelToIndex(_level));
		}

		public float indexToLevel(float _ind)
		{
			return _ind / this.valcnt;
		}

		public float indexToValue(float _ind)
		{
			float num = ((_ind <= 0f) ? this.minval : ((_ind >= this.valcnt) ? this.maxval : (this.indexToLevel(_ind) * this.valrange + this.minval)));
			if (!this.split_integer)
			{
				return num;
			}
			return (float)X.IntR(num / this.valintv) * this.valintv;
		}

		public float getCurrentIndex()
		{
			return this.curind;
		}

		public override string getValueString()
		{
			return this.getValue().ToString();
		}

		public override void setValue(string s)
		{
			float num = X.Nm(s, this.minval - 1f, false);
			if (num < this.minval)
			{
				return;
			}
			this.setValue(num, false);
		}

		public virtual string getDescForValue(float val)
		{
			string text;
			if (this.Adesc_keys != null)
			{
				text = this.Adesc_keys[(int)val % this.Adesc_keys.Length];
			}
			else
			{
				text = ((val == (float)((int)val) || this.return_integer) ? val.ToString() : X.spr_after(val, 1));
			}
			if (this.fnDescConvert == null)
			{
				return text;
			}
			return this.fnDescConvert(text);
		}

		protected override void OnSelect()
		{
			bool flag = base.isSelected();
			base.OnSelect();
			if (!flag && base.isSelected() && this.Setter != null)
			{
				if (!this.auto_setter_focus && !base.isNaviSetted(AIM.L) && !base.isNaviSetted(AIM.R))
				{
					this.auto_setter_focus = true;
				}
				if (this.auto_setter_focus)
				{
					if (!this.Setter.isActive())
					{
						this.Setter.activate(false);
					}
					this.Setter.auto_selected = true;
				}
			}
		}

		public override void OnDeselect()
		{
			if (this.Setter != null && this.Setter.isActive() && this.Setter.auto_selected)
			{
				this.Setter.deactivate();
			}
			this.t_submit_holding = 0f;
			base.OnDeselect();
		}

		public override bool run(float fcnt)
		{
			if (this.Setter != null && this.Setter.isActive())
			{
				this.Setter.run(fcnt);
			}
			if (this.pushed > 0)
			{
				Vector2 vector = IN.getMousePos(IN.getGUICamera()) - this.PosFirst;
				if (!vector.Equals(this.PreVecDrag))
				{
					float indexDragCarried = this.getIndexDragCarried(vector, true);
					float num = X.MMX(0f, this.first_clicked_curind + indexDragCarried, this.valcnt);
					if (num != this.curind)
					{
						this.changingTo(num);
					}
					this.PreVecDrag = vector;
				}
			}
			else if (this.t_submit_holding_ != 0f)
			{
				this.t_submit_holding_ += fcnt * (float)X.MPF(this.t_submit_holding_ > 0f);
				bool flag = false;
				if (this.t_submit_holding_ < 0f)
				{
					if (!IN.isSubmitOn(0))
					{
						if (this.t_submit_holding_ <= -20f)
						{
							flag = true;
						}
						else
						{
							this.t_submit_holding_ = -this.t_submit_holding_;
						}
					}
				}
				else if (IN.isCancelPD())
				{
					flag = true;
				}
				if (flag)
				{
					IN.clearCancelPushDown(true);
					this.t_submit_holding = 0f;
					SND.Ui.play("tool_drag_quit", false);
				}
			}
			return base.run(fcnt);
		}

		private void changingTo(float dind)
		{
			bool flag = true;
			if (this.AFnValChanged != null)
			{
				flag = this.runFnMeterBindings(this.AFnValChanged, this.indexToValue(this.curind), this.indexToValue(dind));
			}
			if (flag)
			{
				this.curind = dind;
				base.fine_flag = true;
				if (this.slider_dragging_snd != "")
				{
					SND.Ui.play(this.slider_dragging_snd, false);
				}
				if (this.t_submit_holding != 0f)
				{
					this.SkinMt.setCursLevelToMeterPos();
					return;
				}
			}
			else if (this.Setter != null)
			{
				this.Setter.fineValue(this.curind.ToString(), false);
			}
		}

		public float getIndexDragCarried(Vector2 VecDrag, bool consider_integer = true)
		{
			float num = (VecDrag.x * this.MemoriVecH.y - this.MemoriVecH.x * VecDrag.y) / (this.MemoriVec.x * this.MemoriVecH.y - this.MemoriVecH.x * this.MemoriVec.y);
			if (this.split_integer && consider_integer)
			{
				num = (float)X.IntR(num);
			}
			return num;
		}

		public float PosLRmagnitude
		{
			get
			{
				return (this.PosRight - this.PosLeft).magnitude;
			}
		}

		public float PosLRmagnitudeDivCount
		{
			get
			{
				return (this.PosRight - this.PosLeft).magnitude / this.valcnt;
			}
		}

		public float get_valrange()
		{
			return this.valrange;
		}

		public void resetDraggingPos()
		{
			this.PosFirst = IN.getMousePos(IN.getGUICamera());
		}

		public override bool OnPointerDown()
		{
			if (this.px_bar_width >= this.w || !this.active_drag)
			{
				return false;
			}
			if (this.Skin == null)
			{
				return false;
			}
			if (!base.OnPointerDown())
			{
				return false;
			}
			this.t_submit_holding = 0f;
			this.first_clicked_curind = this.curind;
			this.PosFirst = IN.getMousePos(IN.getGUICamera());
			this.PosLeft = this.SkinMt.getGlobalLeftPos();
			this.PosRight = this.SkinMt.getGlobalRightPos();
			this.MemoriVec = (this.PosRight - this.PosLeft) / this.valcnt;
			this.MemoriVecH = this.SkinMt.getHousenVec();
			if (this.SkinMt.pointDownFirstJump(ref this.curind, this.PosLeft, this.PosFirst - this.PosLeft))
			{
				this.PosFirst = this.SkinMt.getGlobalPtrCenterPos();
			}
			this.PreVecDrag = new Vector2(0f, 0f);
			if (this.start_drag_snd != "")
			{
				if (base.isLocked())
				{
					SND.Ui.play("talk_progress", false);
					return false;
				}
				SND.Ui.play(this.start_drag_snd, false);
			}
			return true;
		}

		public override void OnPointerUp(bool clicking)
		{
			if (this.pushed > 0 && this.quit_drag_snd != "" && !base.isLocked())
			{
				SND.Ui.play(this.quit_drag_snd, false);
			}
			base.OnPointerUp(clicking);
		}

		public bool setter_valotile
		{
			get
			{
				return this.Setter != null && this.Setter.use_valotile;
			}
			set
			{
				if (this.Setter != null)
				{
					this.Setter.use_valotile = value;
				}
			}
		}

		public float t_submit_holding
		{
			get
			{
				return this.t_submit_holding_;
			}
			set
			{
				if (this.t_submit_holding == value)
				{
					return;
				}
				this.t_submit_holding_ = value;
				if (value == 0f)
				{
					this.SkinMt.curs_level_x = 0.8f;
					this.SkinMt.curs_level_y = -0.7f;
					CURS.focusOnBtn(this, base.isHovered());
				}
			}
		}

		public bool isSubmitHolding()
		{
			return this.t_submit_holding != 0f;
		}

		public override bool isPushed()
		{
			return base.isPushed() || (this.Setter != null && this.Setter.isActive());
		}

		public static bool submitHodingCancel()
		{
			if (aBtn.PreSelected is aBtnMeter)
			{
				aBtnMeter aBtnMeter = aBtn.PreSelected as aBtnMeter;
				if (aBtnMeter.isSubmitHolding())
				{
					IN.clearCancelPushDown(true);
					SND.Ui.play("tool_drag_quit", false);
					aBtnMeter.t_submit_holding = 0f;
					return true;
				}
			}
			return false;
		}

		public CtSetter getCtSetter()
		{
			return this.Setter;
		}

		public float get_setter_swidth()
		{
			if (this.Setter == null)
			{
				return 0f;
			}
			return this.Setter.get_swidth_px();
		}

		public override float get_swidth_px()
		{
			if (this.vertical)
			{
				return base.get_sheight_px();
			}
			return base.get_swidth_px();
		}

		public override float get_sheight_px()
		{
			if (this.vertical)
			{
				return base.get_swidth_px();
			}
			return base.get_sheight_px();
		}

		public float default_value;

		public float minval;

		public float maxval = 64f;

		public float valintv = 4f;

		public bool return_integer;

		public FnBtnMeterLine fnBtnMeterLine;

		public bool split_integer = true;

		public string slider_dragging_snd = "";

		public string start_drag_snd = "";

		public string quit_drag_snd = "";

		public bool active_drag = true;

		public bool auto_setter_focus;

		public bool vertical;

		public bool submit_holding;

		public float px_bar_width;

		public string[] Adesc_keys;

		public byte checkbox_mode;

		private float first_clicked_curind;

		private Vector2 PosFirst;

		private Vector2 PosLeft;

		private Vector2 PosRight;

		private Vector2 MemoriVec;

		private Vector2 MemoriVecH;

		private Vector2 PreVecDrag;

		protected CtSetterMeter Setter;

		public float valcnt;

		private float valrange;

		private float curind;

		protected ButtonSkinMeterNormal SkinMt;

		public FnDescConvert fnDescConvert;

		private float t_submit_holding_;

		public const float CURS_LEVEL_X_DEF = 0.8f;

		public const float CURS_LEVEL_Y_DEF = -0.7f;

		private aBtnMeter.FnMeterBindings[] AFnValChanged;

		public delegate bool FnMeterBindings(aBtnMeter _B, float pre_value, float cur_value);
	}
}
