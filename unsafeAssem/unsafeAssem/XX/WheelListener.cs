using System;
using UnityEngine;

namespace XX
{
	public sealed class WheelListener : IRunAndDestroy
	{
		public WheelListener(bool _wheel_scale_mode)
		{
			this.runner_assigned = true;
			this.wheel_scale_mode = _wheel_scale_mode;
			this.Showing = new DRect("WheelListener");
		}

		public void destruct()
		{
			try
			{
				this.hide();
			}
			catch
			{
			}
		}

		public bool run(float fcnt)
		{
			if (this.state == WheelListener.STATE.OFFLINE)
			{
				this.runner_assigned_ = false;
				return false;
			}
			Vector3 zero = Vector3.zero;
			IClickable clickable;
			if (this.B == null)
			{
				this.initCheckState(ref zero, false);
			}
			else if (this.B.isActive() && this.B.isFocused())
			{
				this.initCheckState(ref zero, false);
			}
			else if (this.alloc_hovering_wheel && this.B.getClickable(IN.getMousePos(null), out clickable))
			{
				this.initCheckState(ref zero, true);
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if ((this.state & WheelListener.STATE.WHEEL) != WheelListener.STATE.OFFLINE)
			{
				float y = IN.MouseWheel.y;
				flag3 = true;
				if (y != 0f)
				{
					this.wheel_t = 8f;
					float num = y * this.z_scroll_level;
					this.z_changed_whole += num;
					if (this.wheel_scale_mode)
					{
						zero.z = this.start_z + this.z_changed_whole - this.z;
						if ((this.state & WheelListener.STATE.GRAB) == WheelListener.STATE.OFFLINE)
						{
							this.state |= WheelListener.STATE.GRAB_SLIP;
						}
					}
					else
					{
						zero.y += num * this.y_scroll_level;
						flag2 = true;
					}
				}
				else
				{
					this.wheel_t -= fcnt;
					if (this.wheel_t <= 0f)
					{
						this.wheel_t = 0f;
						this.quitWheelState(false);
					}
				}
			}
			bool flag4 = false;
			if ((this.state & WheelListener.STATE.GRAB) != WheelListener.STATE.OFFLINE)
			{
				Vector2 vector = IN.getMousePos(null) - this.StartMousePos;
				if (IN.isMouseOn())
				{
					Vector2 vector2 = this.StartCPos - vector * (new Vector2(this.x_scroll_level, this.y_scroll_level) / this.z_scale);
					zero.x = vector2.x - this.Showing.cx;
					zero.y = vector2.y - this.Showing.cy;
					this.Grab_Moved = vector2;
					flag4 = true;
				}
				else
				{
					this.quitGrabState(false);
					this.runFnScaBindings(this.AFnChangedFinished, vector.x, vector.y, 0f);
				}
			}
			if ((this.state & WheelListener.STATE.GRAB_SLIP) != WheelListener.STATE.OFFLINE)
			{
				flag2 = (flag = true);
			}
			if (this.finePositionAndScale(ref zero, fcnt, flag3, flag4, flag2, flag))
			{
				this.runFnScaBindings(this.AFnChanged, this.Showing.cx, this.Showing.cy, this.z_scale);
			}
			else
			{
				this.state &= (WheelListener.STATE)(-9);
			}
			return true;
		}

		private bool finePositionAndScale(ref Vector3 Moved, float fcnt, bool wheeling, bool grabing, bool recheck_after_xy = true, bool recheck_after_z = true)
		{
			bool flag = false;
			bool flag2 = grabing;
			if (Moved.z != 0f || recheck_after_z)
			{
				float num = this.calcZ_min(this.moveable_margin_x, this.moveable_margin_y, 0f);
				float num2 = (wheeling ? (this.start_z + this.z_changed_whole) : this.z);
				if (num != -1000f)
				{
					float num3 = this.calcZ_min(grabing ? this.seeable_margin_x : 0f, grabing ? this.seeable_margin_y : 0f, wheeling ? this.seeable_margin_z : 0f);
					if (num2 < num)
					{
						if (num3 == num)
						{
							num2 = num;
						}
						else
						{
							num2 = X.NI(num, num3, X.ZPOWV(X.Abs(num2 - num), X.Abs(num3 - num) * 3f));
						}
						float num4 = num2 - this.start_z;
						Moved.z += num4 - this.z_changed_whole;
						this.z_changed_whole = num4;
					}
				}
				if (this.max_z_ != -1000f)
				{
					float num5 = this.max_z_;
					float num6 = num5 + (wheeling ? this.seeable_margin_z : 0f);
					if (num2 > num5)
					{
						if (num5 == num6)
						{
							num2 = num5;
						}
						else
						{
							num2 = X.NI(num5, num6, X.ZPOWV(X.Abs(num2 - num5), X.Abs(num6 - num5) * 3f));
						}
						float num7 = num2 - this.start_z;
						Moved.z += num7 - this.z_changed_whole;
						this.z_changed_whole = num7;
					}
				}
				if (Moved.z != 0f)
				{
					if (float.IsNaN(this.z))
					{
						this.z = 1f;
					}
					if (!wheeling && fcnt != 0f)
					{
						this.z = X.MULWALKMNA(this.z, this.z + Moved.z, 0.15f * fcnt, 0.023f);
					}
					else
					{
						this.z += Moved.z;
					}
					this.z_scale_ = -1f;
					float z_scale = this.z_scale;
					recheck_after_xy = (flag = true);
					flag2 = true;
					if (grabing)
					{
						this.initGrab();
					}
				}
			}
			if ((Moved.y != 0f || flag || recheck_after_xy) && this.Showing.height != 0f && this.Movable.height != 0f)
			{
				float num8 = this.scaled_top + Moved.y;
				float num9 = this.Movable.yMax + this.moveable_margin_y;
				float num10 = this.scaled_bottom + Moved.y;
				float num11 = this.Movable.yMin - this.moveable_margin_y;
				if (num8 > num9 && num10 < num11)
				{
					Moved.y = X.NI(num9, num11, 0.5f) - this.Showing.cy;
				}
				else
				{
					if (num8 > num9)
					{
						float num12 = this.Movable.yMax + this.moveable_margin_y + (flag2 ? this.seeable_margin_y : 0f);
						if (num12 == num9)
						{
							num8 = num9;
						}
						else
						{
							num8 = X.NI(num9, num12, X.ZPOWV(X.Abs(num8 - num9), X.Abs(num12 - num9) * 3f));
						}
						Moved.y = num8 - this.scaled_top;
					}
					if (num10 < num11)
					{
						float num13 = this.Movable.yMin - this.moveable_margin_y - (flag2 ? this.seeable_margin_y : 0f);
						if (num13 == num11)
						{
							num10 = num11;
						}
						else
						{
							num10 = X.NI(num11, num13, X.ZPOWV(X.Abs(num10 - num11), X.Abs(num13 - num11) * 3f));
						}
						Moved.y = num10 - this.scaled_bottom;
					}
				}
				if (Moved.y != 0f && !grabing && fcnt != 0f)
				{
					Moved.y = X.absMx(Moved.y * 0.15f * fcnt, X.absMn(0.02f * X.Abs(this.y_scroll_level), X.Abs(Moved.y)));
				}
			}
			else
			{
				Moved.y = 0f;
			}
			if ((Moved.x != 0f || flag || recheck_after_xy) && this.Showing.width != 0f && this.Movable.width != 0f)
			{
				float num14 = this.scaled_right + Moved.x;
				float num15 = this.Movable.xMax + this.moveable_margin_x;
				float num16 = this.scaled_left + Moved.x;
				float num17 = this.Movable.xMin - this.moveable_margin_x;
				if (num14 > num15 && num16 < num17)
				{
					Moved.x = X.NI(num15, num17, 0.5f) - this.Showing.cx;
				}
				else
				{
					if (num14 > num15)
					{
						float num18 = this.Movable.xMax + this.moveable_margin_x + (flag2 ? this.seeable_margin_x : 0f);
						if (num18 == num15)
						{
							num14 = num15;
						}
						else
						{
							num14 = X.NI(num15, num18, X.ZPOWV(X.Abs(num14 - num15), X.Abs(num18 - num15) * 3f));
						}
						Moved.x = num14 - this.scaled_right;
					}
					if (num16 < num17)
					{
						float num19 = this.Movable.xMin - this.moveable_margin_x - (flag2 ? this.seeable_margin_x : 0f);
						if (num19 == num17)
						{
							num16 = num17;
						}
						else
						{
							num16 = X.NI(num17, num19, X.ZPOWV(X.Abs(num16 - num17), X.Abs(num19 - num17) * 3f));
						}
						Moved.x = num16 - this.scaled_left;
					}
				}
				if (Moved.x != 0f && !grabing && fcnt != 0f)
				{
					Moved.x = X.absMx(Moved.x * 0.15f * fcnt, X.absMn(0.02f * X.Abs(this.x_scroll_level), X.Abs(Moved.x)));
				}
			}
			else
			{
				Moved.x = 0f;
			}
			return this.resetShowingArea(this.Showing.cx + Moved.x, this.Showing.cy + Moved.y, -1000f, -1000f, -1000f) || flag;
		}

		private bool initCheckState(ref Vector3 Moved, bool only_wheel = false)
		{
			bool flag = false;
			if ((this.state & WheelListener.STATE.WHEEL) == WheelListener.STATE.OFFLINE && this.z_scroll_level != 0f && (only_wheel || this.B == null || this.B.isHovered()) && IN.MouseWheel.y != 0f)
			{
				this.start_z = this.z;
				this.state |= WheelListener.STATE.WHEEL;
				this.z_changed_whole = 0f;
				flag = true;
			}
			if (!only_wheel)
			{
				bool flag2 = this.keyboard_translate != 0f;
				if (this.z_keyboard_shift_scaling != 0f && IN.isUiShiftO())
				{
					flag2 = false;
					bool flag3 = false;
					float num = this.z;
					if (IN.isTO(0))
					{
						flag3 = (flag = true);
						num += -this.z_keyboard_shift_scaling;
					}
					if (IN.isBO(0))
					{
						flag3 = (flag = true);
						num += this.z_keyboard_shift_scaling;
					}
					if (IN.isL())
					{
						flag3 = (flag = true);
						float num2 = num;
						num = (float)X.Int(num);
						if (num == num2)
						{
							num -= 1f;
						}
					}
					if (IN.isR())
					{
						flag3 = (flag = true);
						float num3 = num;
						num = (float)X.IntC(num);
						if (num == num3)
						{
							num += 1f;
						}
					}
					if (flag3)
					{
						num = X.Mx(this.calcZ_min(0f, 0f, 0f), num);
						if (this.max_z_ != -1000f)
						{
							num = X.Mn(num, this.max_z_);
						}
						Moved.z += num - this.z;
					}
				}
				if (flag2)
				{
					if (IN.isLO(0))
					{
						Moved.x += this.keyboard_translate * this.x_scroll_level;
						this.state |= WheelListener.STATE.GRAB_SLIP;
						flag = true;
					}
					if (IN.isRO(0))
					{
						Moved.x -= this.keyboard_translate * this.x_scroll_level;
						this.state |= WheelListener.STATE.GRAB_SLIP;
						flag = true;
					}
					if (IN.isTO(0))
					{
						Moved.y -= this.keyboard_translate * this.y_scroll_level;
						this.state |= WheelListener.STATE.GRAB_SLIP;
						flag = true;
					}
					if (IN.isBO(0))
					{
						Moved.y += this.keyboard_translate * this.y_scroll_level;
						this.state |= WheelListener.STATE.GRAB_SLIP;
						flag = true;
					}
				}
				if (this.grab_enabled && IN.isMousePushDown() && IN.Click.get_CurrentHover() == null && this.B == null)
				{
					this.initGrab();
					flag = true;
				}
			}
			return flag;
		}

		public WheelListener quitAllState(bool call_bindings = true, bool finalize_grab_slip = true)
		{
			call_bindings = call_bindings && (this.state & (WheelListener.STATE)14) > WheelListener.STATE.OFFLINE;
			this.quitWheelState(false).quitGrabState(false);
			this.quitGrabSlipState(false, finalize_grab_slip);
			if (call_bindings)
			{
				this.runFnScaBindings(this.AFnChanged, this.Showing.cx, this.Showing.cy, this.z_scale);
			}
			return this;
		}

		public WheelListener quitWheelState(bool call_bindings = true)
		{
			if ((this.state & WheelListener.STATE.WHEEL) != WheelListener.STATE.OFFLINE)
			{
				this.state &= (WheelListener.STATE)(-3);
				if (call_bindings)
				{
					this.runFnScaBindings(this.AFnChanged, this.Showing.cx, this.Showing.cy, this.z_scale);
				}
				this.z_changed_whole = 0f;
				this.start_z = this.z;
				this.wheel_t = 0f;
				this.state |= WheelListener.STATE.GRAB_SLIP;
			}
			return this;
		}

		public WheelListener quitGrabState(bool call_bindings = true)
		{
			if ((this.state & WheelListener.STATE.GRAB) != WheelListener.STATE.OFFLINE)
			{
				this.state &= (WheelListener.STATE)(-5);
				this.state |= WheelListener.STATE.GRAB_SLIP;
				if (call_bindings)
				{
					this.runFnScaBindings(this.AFnChanged, this.Showing.cx, this.Showing.cy, this.z_scale);
				}
			}
			return this;
		}

		public WheelListener quitGrabSlipState(bool call_bindings = true, bool finalize_grab_slip = true)
		{
			if ((this.state & WheelListener.STATE.GRAB_SLIP) != WheelListener.STATE.OFFLINE)
			{
				this.state &= (WheelListener.STATE)(-9);
				if (finalize_grab_slip)
				{
					Vector3 zero = Vector3.zero;
					if (this.finePositionAndScale(ref zero, 0f, false, false, true, true) && call_bindings)
					{
						this.runFnScaBindings(this.AFnChanged, this.Showing.cx, this.Showing.cy, this.z_scale);
					}
				}
			}
			return this;
		}

		public void hide()
		{
			this.quitAllState(false, true);
			this.state = WheelListener.STATE.OFFLINE;
			this.z_changed_whole = 0f;
			this.runner_assigned = false;
		}

		public void bind()
		{
			this.runner_assigned = true;
			this.state |= WheelListener.STATE.ONLINE;
		}

		public void bindToBtn(aBtn _B)
		{
			this.B = _B;
			if (this.grab_enabled)
			{
				this.B.addDownFn(new FnBtnBindings(this.FnGrabInit));
			}
		}

		private bool FnGrabInit(aBtn _B = null)
		{
			if (this.grab_enabled)
			{
				this.initGrab();
			}
			return true;
		}

		public void initGrab()
		{
			if (this.isActive())
			{
				this.quitGrabSlipState(false, false);
				this.state |= WheelListener.STATE.GRAB;
				this.StartMousePos = IN.getMousePos(null);
				this.StartCPos = new Vector2(this.Showing.cx, this.Showing.cy);
				this.Grab_Moved = Vector2.zero;
			}
		}

		public bool resetShowingArea(float cx, float cy, float _z_scale = -1000f, float show_width = -1000f, float show_height = -1000f)
		{
			bool flag = false;
			if (show_width != -1000f && show_width != this.Showing.w)
			{
				float cx2 = this.Showing.cx;
				this.Showing.w = show_width;
				this.Showing.x = cx2 - this.Showing.w * 0.5f;
				flag = true;
			}
			if (show_height != -1000f && show_height != this.Showing.h)
			{
				float cy2 = this.Showing.cy;
				this.Showing.h = show_height;
				this.Showing.y = cy2 - this.Showing.h * 0.5f;
				flag = true;
			}
			if (_z_scale != -1000f && this.z_scale != _z_scale)
			{
				this.z_scale = _z_scale;
				flag = true;
			}
			if (cx != -1000f && this.Showing.cx != cx)
			{
				this.Showing.x = cx - this.Showing.w * 0.5f;
				flag = true;
			}
			if (cy != -1000f && this.Showing.cy != cy)
			{
				this.Showing.y = cy - this.Showing.h * 0.5f;
				flag = true;
			}
			return flag;
		}

		public void shiftPosition(float x, float y, bool shift_showing = true, bool shift_moveable = false)
		{
			if (shift_showing)
			{
				this.Showing.x += x;
				this.Showing.y += y;
			}
			if (shift_moveable)
			{
				this.Movable.x = this.Movable.x + x;
				this.Movable.y = this.Movable.y + y;
			}
		}

		private float calcZ_min(float margin_x = 0f, float margin_y = 0f, float margin_z = 0f)
		{
			if (this.min_scale != -1000f)
			{
				return this.min_scale;
			}
			float num = (this.calc_super_min_scale ? ((this.max_z_scale <= 0f) ? this.z_scale : this.max_z_scale) : (-1000f));
			if (this.wheel_scale_mode)
			{
				if (this.Movable.width > 0f && this.Showing.width > 0f)
				{
					float num2 = this.Movable.width + (this.moveable_margin_x + margin_x) * 2f;
					float num3 = this.Showing.w / num2;
					num = (this.calc_super_min_scale ? X.Mn(num, num3) : X.Mx(num, num3));
				}
				if (this.Movable.height > 0f && this.Showing.height > 0f)
				{
					float num4 = this.Movable.height + (this.moveable_margin_y + margin_y) * 2f;
					float num5 = this.Showing.h / num4;
					num = (this.calc_super_min_scale ? X.Mn(num, num5) : X.Mx(num, num5));
				}
			}
			if (num < 0f)
			{
				return num;
			}
			return Mathf.Log(num, 2f) - margin_z;
		}

		public WheelListener addChangedFn(FnWheelBindings Fn)
		{
			aBtn.addFnT<FnWheelBindings>(ref this.AFnChanged, Fn);
			return this;
		}

		public WheelListener addChangedFinishedFn(FnWheelBindings Fn)
		{
			aBtn.addFnT<FnWheelBindings>(ref this.AFnChangedFinished, Fn);
			return this;
		}

		private bool runFnScaBindings(FnWheelBindings[] AFn, float x, float y, float z)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				FnWheelBindings fnWheelBindings = AFn[i];
				if (fnWheelBindings == null)
				{
					return flag;
				}
				flag = fnWheelBindings(this, x, y, z) && flag;
			}
			return flag;
		}

		public float z_scale
		{
			get
			{
				if (!this.wheel_scale_mode)
				{
					return this.z_scale_ = this.z;
				}
				if (this.z_scale_ != 0f)
				{
					if (this.z >= 0f)
					{
						this.z_scale_ = 1f + this.z;
					}
					else
					{
						this.z_scale_ = Mathf.Pow(2f, this.z);
					}
				}
				return this.z_scale_;
			}
			set
			{
				if (value < 0f)
				{
					this.z_scale_ = -1f;
					return;
				}
				if (!this.wheel_scale_mode)
				{
					this.z_scale_ = value;
					this.z = value;
					return;
				}
				this.z_scale_ = value;
				if (value >= 1f)
				{
					this.z = value - 1f;
					return;
				}
				this.z = Mathf.Log(value, 2f);
			}
		}

		public float max_z_scale
		{
			get
			{
				if (!this.wheel_scale_mode || this.max_z_ <= -1000f)
				{
					return this.max_z_scale_ = this.max_z_;
				}
				if (this.max_z_scale_ != 0f)
				{
					if (this.max_z_ >= 0f)
					{
						this.max_z_scale_ = 1f + this.max_z_;
					}
					else
					{
						this.max_z_scale_ = Mathf.Pow(2f, this.max_z_);
					}
				}
				return this.max_z_scale_;
			}
			set
			{
				if (value < 0f)
				{
					this.max_z_ = -1000f;
					return;
				}
				if (!this.wheel_scale_mode)
				{
					this.max_z_scale_ = value;
					this.max_z_ = value;
					return;
				}
				this.max_z_scale_ = value;
				if (value >= 1f)
				{
					this.max_z_ = value - 1f;
					return;
				}
				this.max_z_ = Mathf.Log(value, 2f);
			}
		}

		public void setMinZScale(float scale_real)
		{
			if (scale_real >= 1f)
			{
				this.min_scale = scale_real - 1f;
				return;
			}
			this.min_scale = Mathf.Log(scale_real, 2f);
		}

		public float scaled_top
		{
			get
			{
				return this.Showing.cy + this.Showing.height * 0.5f / this.z_scale;
			}
		}

		public float scaled_bottom
		{
			get
			{
				return this.Showing.cy - this.Showing.height * 0.5f / this.z_scale;
			}
		}

		public float scaled_right
		{
			get
			{
				return this.Showing.cx + this.Showing.width * 0.5f / this.z_scale;
			}
		}

		public float scaled_left
		{
			get
			{
				return this.Showing.cx - this.Showing.width * 0.5f / this.z_scale;
			}
		}

		public bool isActive()
		{
			return (this.state & WheelListener.STATE.ONLINE) > WheelListener.STATE.OFFLINE;
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

		public bool isGrabing()
		{
			return (this.state & WheelListener.STATE.GRAB) > WheelListener.STATE.OFFLINE;
		}

		public bool grab_enabled
		{
			get
			{
				return this.t_grab >= 0f;
			}
			set
			{
				this.t_grab = (float)(value ? 0 : (-1));
			}
		}

		public override string ToString()
		{
			return "<WheelListener>";
		}

		public float x_scroll_level = 1f;

		public float y_scroll_level = 1f;

		public float z_scroll_level = 0.02f;

		public float moveable_margin_x = 0.9375f;

		public float moveable_margin_y = 0.9375f;

		public float seeable_margin_x = 0.9375f;

		public float seeable_margin_y = 0.9375f;

		public float seeable_margin_z = 0.2f;

		public bool calc_super_min_scale;

		public bool alloc_hovering_wheel;

		private float max_z_ = -1000f;

		public float min_scale = -1000f;

		private float max_z_scale_ = -1f;

		public float keyboard_translate;

		public float z_keyboard_shift_scaling;

		public Rect Movable = new Rect(-IN.wh, -IN.hh, IN.w, IN.h);

		public readonly DRect Showing;

		private bool runner_assigned_;

		private float start_z;

		private float z_changed_whole;

		private float wheel_t = -1f;

		private const int WHEEL_MAXT = 8;

		private FnWheelBindings[] AFnChanged;

		private FnWheelBindings[] AFnChangedFinished;

		private aBtn B;

		public Vector2 StartMousePos;

		private Vector2 StartCPos;

		private Vector2 Grab_Moved;

		private WheelListener.STATE state = WheelListener.STATE.ONLINE;

		private float z = 1f;

		private float z_scale_ = 1f;

		private float t_grab;

		private readonly bool wheel_scale_mode;

		private enum STATE
		{
			OFFLINE,
			ONLINE,
			WHEEL,
			GRAB = 4,
			GRAB_SLIP = 8
		}
	}
}
