using System;
using UnityEngine;
using XX;

namespace nel
{
	public class UiBoxDesigner : DesignerWhite, IAlphaSetable, IDesignerPosSetableBlock
	{
		protected override void Awake()
		{
			if (this.Bx != null)
			{
				return;
			}
			base.Awake();
			this.scroll_normal_color = 4283780170U;
			this.scroll_push_color = 4278246796U;
			this.margin_in_lr = 20f;
			this.margin_in_tb = 30f;
			this.use_scroll = false;
			this.use_button_connection = true;
			base.bgcol = MTRX.ColTrnsp;
			this.Bx = base.gameObject.AddComponent<UiBox>();
			this.Bx.mouse_click_focus = false;
			this.Bx.enabled = false;
			this.Bx.addFocusFn(new FnUiBoxBindings(this.fnBoxFocused));
			this.Bx.addBlurFn(new FnUiBoxBindings(this.fnBoxBlured));
			this.Bx.ColliderMargin(0f);
			this.Bx.use_focus = false;
			this.animate_maxt = 36;
		}

		public override void OnEnable()
		{
			base.OnEnable();
			this.Bx.enabled = false;
		}

		public UiBoxDesigner position(float _sx, float _sy, float _dx = -1000f, float _dy = -1000f, bool no_reset_time = false)
		{
			this.Bx.position(_sx, _sy, _dx, _dy, no_reset_time);
			return this;
		}

		public UiBoxDesigner positionD(float pixel_x, float pixel_y, int appear_dir_aim = -1, float appear_len = 30f)
		{
			if (appear_dir_aim < 0)
			{
				this.Bx.position(pixel_x, pixel_y, -1000f, -1000f, false);
			}
			else
			{
				this.Bx.position(pixel_x - (float)CAim._XD(appear_dir_aim, 1) * appear_len, pixel_y - (float)CAim._YD(appear_dir_aim, 1) * appear_len, pixel_x, pixel_y, false);
			}
			return this;
		}

		public void posSetA(float _dx, float _dy, bool no_reset_time = true)
		{
			this.Bx.posSetA(_dx, _dy, no_reset_time);
		}

		public void posSetA(float _sx, float _sy, float _dx, float _dy, bool no_reset_time = true)
		{
			this.Bx.posSetA(_sx, _sy, _dx, _dy, no_reset_time);
		}

		public UiBoxDesigner posSetDA(float pixel_x, float pixel_y, int appear_dir_aim = -1, float appear_len = 30f, bool no_reset_time = true)
		{
			if (appear_dir_aim < 0)
			{
				this.Bx.posSetA(pixel_x, pixel_y, no_reset_time);
			}
			else
			{
				this.Bx.posSetA(pixel_x - (float)CAim._XD(appear_dir_aim, 1) * appear_len, pixel_y - (float)CAim._YD(appear_dir_aim, 1) * appear_len, pixel_x, pixel_y, no_reset_time);
			}
			return this;
		}

		public UiBoxDesigner fineMove()
		{
			this.Bx.fineMove();
			return this;
		}

		public UiBoxDesigner SetParent(Transform Trs, bool world_position_stays = true)
		{
			this.Bx.SetParent(Trs, world_position_stays);
			return this;
		}

		public UiBoxDesigner margin(params float[] Aexpand)
		{
			this.Bx.margin(Aexpand);
			return this;
		}

		public UiBoxDesigner bkg_scale(bool _x = true, bool _y = false, bool do_not_use_zsin = false)
		{
			this.Bx.bkg_scale(_x, _y, do_not_use_zsin);
			return this;
		}

		public UiBoxDesigner Focusable(bool flag = true, bool _click_focusflag = true, UiBox FocusParent = null)
		{
			if (flag)
			{
				this.click_focusflag = _click_focusflag;
				this.mode = UiBoxDesigner.DSN_MODE.FOCUSABLE;
				this.Bx.use_focus = true;
				this.Bx.mouse_click_focus = this.click_focusflag;
				this.Bx.FocusParent = FocusParent;
			}
			else
			{
				UiBox.click_focus_decline_by_designer = false;
				this.click_focusflag = false;
				this.Bx.use_focus = false;
				this.Bx.mouse_click_focus = false;
			}
			return this;
		}

		public UiBoxDesigner position_max_time(int t)
		{
			this.Bx.position_max_time(t, -1);
			return this;
		}

		public UiBoxDesigner appear_time(int t)
		{
			this.Bx.appear_time(t);
			return this;
		}

		public UiBoxDesigner anim_time(int t)
		{
			this.Bx.position_max_time(t, -1);
			this.Bx.appear_time(t);
			return this;
		}

		public UiBoxDesigner RowBtnMode(float _btn_height = 42f)
		{
			this.mode = UiBoxDesigner.DSN_MODE.ROW_BTNS;
			this.Bx.use_focus = true;
			this.margin_in_tb = 50f;
			this.margin_in_lr = 3f;
			this.btn_height = _btn_height;
			base.item_margin_y_px = 1f;
			this.selectable_loop |= 2;
			return this;
		}

		public override Designer init()
		{
			if (!base.initted)
			{
				this.Bx.make(null);
			}
			base.init();
			return this;
		}

		public override Designer WH(float _wpx = 0f, float _hpx = 0f)
		{
			base.WH(_wpx, _hpx);
			this.Bx.swh(_wpx, _hpx);
			return this;
		}

		public UiBoxDesigner WHanim(float _wpx, float _hpx = -1f, bool anim_w = true, bool anim_h = true)
		{
			base.WH(_wpx, _hpx);
			this.Bx.swh_anim(_wpx, _hpx, anim_w, anim_h);
			return this;
		}

		public UiBoxDesigner wh_animZero(bool anim_w = true, bool anim_h = true)
		{
			this.Bx.wh_animZero(anim_w, anim_h, 0f);
			return this;
		}

		public override Designer Clear()
		{
			base.Clear();
			this.wh_animZero(true, true);
			return this;
		}

		public Designer innerWH(float _wpx = 0f, float _hpx = 0f)
		{
			return this.WH(_wpx + this.Bx.lmarg + this.Bx.rmarg, _hpx + this.Bx.tmarg + this.Bx.bmarg);
		}

		public UiBoxDesigner DelayT(int time)
		{
			this.Bx.delayt = time;
			this.animate_maxt += time * 2;
			return this;
		}

		public float swidth
		{
			get
			{
				return this.Bx.swidth;
			}
		}

		public float sheight
		{
			get
			{
				return this.Bx.sheight;
			}
		}

		public override bool run(float fcnt)
		{
			if (!base.initted)
			{
				return false;
			}
			if (!this.Bx.run(fcnt, false))
			{
				return false;
			}
			if (this.Bx.show_delaying)
			{
				this.kadomaruRedraw(-1f, true);
				if (this.animate_t >= 0f)
				{
					this.animate_t = 0f;
				}
				return true;
			}
			base.run(fcnt);
			if (X.D && this.animate_t > (float)(this.animate_maxt + 5) && this.Bx.show_animating)
			{
				this.setAlpha(this.alpha_, true);
			}
			return true;
		}

		protected override void destroyMe()
		{
		}

		public int box_stencil_ref_mask
		{
			get
			{
				return this.Bx.stencil_ref_mask;
			}
			set
			{
				UiBox bx = this.Bx;
				this.stencil_ref = value;
				bx.stencil_ref_mask = value;
			}
		}

		protected override bool fnDragInitScroll(ScrollBox Scr, aBtn B)
		{
			if (this.mouse_click_focus)
			{
				UiBox.click_focus_decline_by_designer = true;
			}
			return true;
		}

		protected override bool fnDragQuitScroll(ScrollBox Scr, aBtn B)
		{
			UiBox.click_focus_decline_by_designer = false;
			return true;
		}

		public override Designer activate()
		{
			base.activate();
			this.Bx.activate();
			this.setAlpha(0.995f);
			return this;
		}

		public override Designer deactivate()
		{
			base.deactivate();
			this.Bx.deactivate();
			CURS.Active.Rem(this.gob_name);
			return this;
		}

		public Designer deactivate(bool immediate)
		{
			this.deactivate();
			if (immediate && this.Bx.didGraphicInit())
			{
				this.Bx.fineMove();
				this.animate_t = (float)(-(float)this.animate_maxt);
				this.kadomaruRedraw(0f, true);
			}
			return this;
		}

		public Designer hideContent()
		{
			base.deactivate();
			return this;
		}

		public override void setAlpha(float value)
		{
			this.setAlpha(value, false);
		}

		public void setAlpha(float value, bool force)
		{
			if (this.alpha_ != value || force)
			{
				this.alpha_ = value;
				if (this.Row != null)
				{
					this.Row.setAlpha(value * this.Bx.alpha * this.Bx.alpha);
				}
			}
		}

		protected override void kadomaruRedraw(float _t, bool update_mesh = true)
		{
			this.Row.setAlpha(this.Bx.show_delaying ? 0f : (this.alpha_ * this.Bx.alpha * this.Bx.alpha));
		}

		public UiBoxDesigner innerFadeRestart()
		{
			return this;
		}

		public override aBtn addButton(DsnDataButton Mde)
		{
			return this.addButtonT<aBtnNel>(Mde);
		}

		public override T addButtonT<T>(DsnDataButton Mde)
		{
			if (Mde.h == 0f && this.mode == UiBoxDesigner.DSN_MODE.ROW_BTNS)
			{
				Mde.h = this.btn_height;
			}
			return base.addButtonT<T>(Mde);
		}

		public override BtnContainer<aBtn> addChecksT<T>(DsnDataChecks Mde)
		{
			return base.addChecksT<T>(Mde);
		}

		public override BtnContainerRadio<aBtn> addRadioT<T>(DsnDataRadio Mde)
		{
			return base.addRadioT<T>(Mde);
		}

		public override BtnContainer<aBtn> addButtonMulti(DsnDataButtonMulti Mde)
		{
			return this.addButtonMultiT<aBtnNel>(Mde);
		}

		public override LabeledInputField addInput(DsnDataInput Mde)
		{
			return base.addInputT<LabeledInputFieldNel>(Mde);
		}

		public override Designer addSlider(DsnDataSlider Mde)
		{
			base.addSliderT<aBtnMeterNel>(Mde);
			return this;
		}

		protected override FillBlock setPInner(DsnDataP Mde, FillBlock Fl)
		{
			base.setPInner(Mde, Fl);
			Fl.alpha = (this.Bx.show_delaying ? 0f : (this.alpha_ * this.Bx.alpha * this.Bx.alpha));
			return Fl;
		}

		public void fineAllFillBlockAlpha(Designer Target = null)
		{
			if (Target == null)
			{
				Target = this;
			}
			float num = (this.Bx.show_delaying ? 0f : (this.alpha_ * this.Bx.alpha * this.Bx.alpha));
			DesignerRowMem rowManager = Target.getRowManager();
			int block_count = rowManager.block_count;
			for (int i = 0; i < block_count; i++)
			{
				FillBlock fillBlock = rowManager.GetBlockByIndex(i) as FillBlock;
				if (fillBlock != null)
				{
					fillBlock.alpha = num;
				}
			}
		}

		public aBtnMeterNel addSliderCT(DsnDataSlider Mde, float setter_width = 114f, aBtn SelectedOverride = null, bool use_valotile = false)
		{
			float num = ((Mde.w <= 0f) ? 200f : Mde.w);
			bool flag = Mde.skin == "invisible";
			if (flag)
			{
				num = (Mde.w = 1f);
			}
			if (num + setter_width + base.item_margin_x_px > base.use_w)
			{
				base.Br();
			}
			aBtnMeterNel aBtnMeterNel = base.addSliderT<aBtnMeterNel>(Mde);
			CtSetterMeter ctSetterMeter = aBtnMeterNel.createSetter(null);
			if (use_valotile)
			{
				ctSetterMeter.use_valotile = true;
			}
			ctSetterMeter.swidth = setter_width;
			ctSetterMeter.lr_reverse = Mde.lr_reverse;
			base.addItem(null, ctSetterMeter, null, Mde, -0.01f, true);
			if (ctSetterMeter.auto_selected)
			{
				aBtnMeterNel.z_push_click = false;
			}
			bool active_drag = aBtnMeterNel.active_drag;
			if (flag)
			{
				ctSetterMeter.arrow_margin = 8;
				aBtnMeterNel.get_Skin().curs_level_x = 0.6f * setter_width;
			}
			ctSetterMeter.SelectedOverride = SelectedOverride;
			return aBtnMeterNel;
		}

		public UiBoxDesigner PC(string t, float _swidth = 0f, bool _html = false)
		{
			return this.P(t, ALIGN.CENTER, _swidth, _html, 0f, "");
		}

		public UiBoxDesigner P(string t, ALIGN _alignx = ALIGN.LEFT, float _swidth = 0f, bool _html = false, float _sheight = 0f, string _name = "")
		{
			UiBoxDesigner.addPTo(this, t, _alignx, _swidth, _html, _sheight, _name, -1);
			return this;
		}

		public static FillBlock addPTo(Designer Ds, string t, ALIGN _alignx = ALIGN.LEFT, float _swidth = 0f, bool _html = false, float _sheight = 0f, string _name = "", int _text_auto_wrap = -1)
		{
			FillBlock fillBlock;
			using (STB stb = TX.PopBld(t, 0))
			{
				fillBlock = UiBoxDesigner.addPTo(Ds, stb, _alignx, _swidth, _html, _sheight, _name, _text_auto_wrap);
			}
			return fillBlock;
		}

		public static FillBlock addPTo(Designer Ds, STB Stb, ALIGN _alignx = ALIGN.LEFT, float _swidth = 0f, bool _html = false, float _sheight = 0f, string _name = "", int _text_auto_wrap = -1)
		{
			DsnDataP dsnDataP = new DsnDataP("", false)
			{
				Stb = Stb,
				name = _name,
				size = ((_sheight == 0f) ? 18f : X.Mn(_sheight - 4f, 18f)),
				alignx = _alignx,
				Col = MTRX.ColTrnsp,
				TxCol = C32.d2c(4283780170U),
				swidth = ((_swidth == 0f) ? Ds.use_w : _swidth),
				sheight = _sheight,
				html = _html
			};
			if (_text_auto_wrap >= 0)
			{
				dsnDataP.text_auto_wrap = _text_auto_wrap != 0;
			}
			return Ds.addP(dsnDataP, false);
		}

		public UiBoxDesigner Hr(float _width_ratio = 0.6f, float _margin_t = 18f, float _margin_b = 26f, float line_height = 1f)
		{
			base.Br();
			base.addHr(new DsnDataHr
			{
				draw_width_rate = _width_ratio,
				swidth = base.use_w,
				Col = C32.d2c(2857717320U),
				margin_t = _margin_t,
				margin_b = _margin_t,
				line_height = line_height
			});
			base.Br();
			return this;
		}

		public UiBoxDesigner HrV(float _height_ratio = 0.6f, float _margin_l = 22f, float _margin_r = 22f, float line_height = 1f)
		{
			base.addHr(new DsnDataHr
			{
				draw_width_rate = _height_ratio,
				swidth = ((this.Row != null) ? this.Row.current_row_height : base.use_h),
				Col = C32.d2c(2857717320U),
				margin_t = _margin_l,
				margin_b = _margin_r,
				line_height = line_height,
				vertical = true
			});
			return this;
		}

		public static MeshDrawer createStencilTB(UiBoxDesigner BxC, out GameObject GobMd, float margin_shift = 42f, MdArranger Ma = null)
		{
			GobMd = IN.CreateGob(BxC, "-md");
			MeshDrawer meshDrawer = MeshDrawer.prepareMeshRenderer(GobMd, MTRX.MIicon.getMtr(BLEND.NORMALST, BxC.box_stencil_ref_mask), -0.1f, -1, null, false, false);
			meshDrawer.Col = C32.d2c(4283780170U);
			UiBoxDesigner.createStencilTB(meshDrawer, BxC.h, margin_shift, Ma != null);
			if (Ma != null)
			{
				Ma.setMdTarget(meshDrawer);
				Ma.SetWhole(true);
			}
			return meshDrawer;
		}

		public static void createStencilTB(MeshDrawer Md, float h, float margin_shift = 42f, bool update_temporary = false)
		{
			float num = h * 0.5f - margin_shift;
			for (int i = 0; i < 4; i++)
			{
				Md.Identity().Scale((float)X.MPF(i < 2), (float)X.MPF(i == 1 || i == 2), false);
				Md.initForImg(MTRX.ItemGetDescFrame, 0);
				Md.DrawScaleGraph2(-4f, num, 1f, 0f, 1f, 1f, null);
			}
			Md.Identity();
			Md.updateForMeshRenderer(update_temporary);
		}

		public override bool use_valotile
		{
			set
			{
				base.use_valotile = value;
				this.Bx.use_valotile = value;
			}
		}

		protected bool fnBoxFocused(UiBox Bx)
		{
			return this.runFn(this.AFnFocus);
		}

		protected bool fnBoxBlured(UiBox Bx)
		{
			return this.runFn(this.AFnBlur);
		}

		public UiBox getBox()
		{
			return this.Bx;
		}

		public UiBoxDesigner addFocusFn(UiBoxDesigner.FnUiBoxDesignerBindings Fn)
		{
			aBtn.addFnT<UiBoxDesigner.FnUiBoxDesignerBindings>(ref this.AFnFocus, Fn);
			return this;
		}

		public UiBoxDesigner addBlurFn(UiBoxDesigner.FnUiBoxDesignerBindings Fn)
		{
			aBtn.addFnT<UiBoxDesigner.FnUiBoxDesignerBindings>(ref this.AFnBlur, Fn);
			return this;
		}

		protected bool runFn(UiBoxDesigner.FnUiBoxDesignerBindings[] AFn)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				UiBoxDesigner.FnUiBoxDesignerBindings fnUiBoxDesignerBindings = AFn[i];
				if (fnUiBoxDesignerBindings == null)
				{
					return flag;
				}
				flag = fnUiBoxDesignerBindings(this) && flag;
			}
			return flag;
		}

		public bool isFocused()
		{
			return this.Bx.isFocused();
		}

		public void Focus()
		{
			UiBox.FocusTo(this.Bx);
		}

		public string gob_name
		{
			get
			{
				if (this.gob_name_ == null)
				{
					this.gob_name_ = base.gameObject.name;
				}
				return this.gob_name_;
			}
		}

		public static bool FocusTo(UiBoxDesigner Bx)
		{
			return UiBox.FocusTo(Bx.Bx);
		}

		public static bool BlurFrom(UiBoxDesigner Bx)
		{
			return UiBox.BlurFrom(Bx.Bx);
		}

		public bool mouse_click_focus
		{
			get
			{
				return this.Bx.mouse_click_focus;
			}
			set
			{
				this.Bx.mouse_click_focus = value;
			}
		}

		public string gob_name_;

		protected UiBox Bx;

		private UiBoxDesigner.DSN_MODE mode;

		public float btn_height = 32f;

		public const float TX_SIZE = 18f;

		public const uint text_color = 4283780170U;

		private bool click_focusflag;

		private UiBoxDesigner.FnUiBoxDesignerBindings[] AFnFocus;

		private UiBoxDesigner.FnUiBoxDesignerBindings[] AFnBlur;

		public const uint hr_color = 2857717320U;

		public delegate bool FnUiBoxDesignerBindings(UiBoxDesigner _B);

		private enum DSN_MODE
		{
			NORMAL,
			FOCUSABLE,
			ROW_BTNS
		}
	}
}
