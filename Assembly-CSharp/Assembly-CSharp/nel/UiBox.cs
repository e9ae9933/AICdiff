using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class UiBox : MsgBox
	{
		public static UiBox Create(GameObject Base, string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f)
		{
			UiBox uiBox = ((Base != null) ? IN.CreateGob(Base, "-" + name) : new GameObject(name)).AddComponent<UiBox>();
			if (appear_dir_aim < 0)
			{
				uiBox.position(pixel_x, pixel_y, -1000f, -1000f, false);
			}
			else
			{
				uiBox.position(pixel_x - (float)CAim._XD(appear_dir_aim, 1) * appear_len, pixel_y - (float)CAim._YD(appear_dir_aim, 1) * appear_len, pixel_x, pixel_y, false);
			}
			uiBox.swh(pixel_w, pixel_h);
			uiBox.ColliderMargin(0f);
			return uiBox;
		}

		public static bool FocusTo(UiBox Bx)
		{
			if (Bx == null || !Bx.use_focus_)
			{
				return false;
			}
			if (Bx.isFocused())
			{
				return true;
			}
			if (Bx != null && Bx.FocusParent != null && Bx.FocusParent != Bx)
			{
				return UiBox.FocusTo(Bx.FocusParent);
			}
			UiBox curFocus = UiBox.CurFocus;
			if (UiBox.CurFocus != null && !UiBox.BlurFrom(UiBox.CurFocus))
			{
				return false;
			}
			if (Bx != null)
			{
				if (!Bx.runFn(Bx.AFnFocus))
				{
					if (curFocus != null)
					{
						UiBox.FocusTo(curFocus);
					}
					return false;
				}
				UiBox.CurFocus = Bx;
				Bx.use_focus_ = true;
				if (Bx.mouse_click_focus_ == 1)
				{
					Bx.mouse_click_focus_ = 2;
				}
			}
			else
			{
				UiBox.CurFocus = null;
			}
			return true;
		}

		public static bool BlurFrom(UiBox Bx)
		{
			if (Bx == null)
			{
				return true;
			}
			if (Bx.FocusParent != null)
			{
				return UiBox.BlurFrom(Bx);
			}
			if (!Bx.isFocused())
			{
				return true;
			}
			if (!UiBox.CurFocus.runFn(UiBox.CurFocus.AFnBlur))
			{
				return false;
			}
			if (Bx.mouse_click_focus_ == 1)
			{
				Bx.mouse_click_focus_ = 2;
			}
			UiBox.CurFocus = null;
			return true;
		}

		public override void OnDestroy()
		{
			if (UiBox.CurFocus == this)
			{
				UiBox.BlurFrom(this);
			}
			if (UiBox.AMouseDownBox != null)
			{
				UiBox.AMouseDownBox.Remove(this);
			}
			base.OnDestroy();
		}

		protected override void Awake()
		{
			base.Awake();
			this.lmarg = (this.rmarg = 24f);
			this.tmarg = (this.bmarg = 18f);
			base.bkg_scale(false, false, false);
			this.gradation(new Color32[]
			{
				UiBox.ColBottom,
				UiBox.ColTop
			}, null);
			this.frametype_ = UiBox.FRAMETYPE.MAIN;
			this.NowColEmblem = UiBox.ColEmb;
			this.collider_margin_pixel = 4f;
			this.maxt_pos = 17;
			this.maxt_pos_hide = 22;
		}

		public override MsgBox gradation(Color32[] Acol = null, float[] Alvls = null)
		{
			base.gradation(Acol, Alvls);
			this.frametype_ = UiBox.FRAMETYPE.NO_OVERRIDE;
			return this;
		}

		public UiBox fineFocusColor()
		{
			if (this.frametype_ == UiBox.FRAMETYPE.NO_OVERRIDE)
			{
				return this;
			}
			float num = ((!this.use_focus_) ? 1f : ((this.focus_t >= 0) ? X.ZSIN((float)this.focus_t, 20f) : X.ZSIN((float)(20 + this.focus_t), 20f)));
			return this.fineFocusColor(num, 1f);
		}

		public UiBox fineFocusColor(float tz, float alpha = 1f)
		{
			if (this.Agrd_col != null)
			{
				if (this.frametype_ == UiBox.FRAMETYPE.DARK || this.frametype_ == UiBox.FRAMETYPE.DARK_SIMPLE)
				{
					this.Agrd_col[0] = MTRX.colb.Set(UiBox.DColBottom_Offline).blend(UiBox.DColBottom, tz).mulA(alpha)
						.C;
					this.Agrd_col[1] = MTRX.colb.Set(UiBox.DColTop_Offline).blend(UiBox.DColTop, tz).mulA(alpha)
						.C;
					this.NowColEmblem = MTRX.colb.Set(UiBox.DColEmb_Offline).blend(UiBox.DColEmb, tz).mulA(alpha)
						.C;
				}
				else
				{
					this.Agrd_col[0] = MTRX.colb.Set(UiBox.ColBottom_Offline).blend(UiBox.ColBottom, tz).mulA(alpha)
						.C;
					this.Agrd_col[1] = MTRX.colb.Set(UiBox.ColTop_Offline).blend(UiBox.ColTop, tz).mulA(alpha)
						.C;
					this.NowColEmblem = MTRX.colb.Set(UiBox.ColEmb_Offline).blend(UiBox.ColEmb, tz).mulA(alpha)
						.C;
				}
				this.need_bg_redraw_flag = true;
			}
			return this;
		}

		public Color32 getTopColor()
		{
			if (this.Agrd_col == null)
			{
				return this.bg_col.C;
			}
			return this.Agrd_col[this.Agrd_col.Length - 1];
		}

		public override MsgBox wh(float _w, float _h)
		{
			if (_h < 0f)
			{
				_h = _w;
			}
			if (!base.isActive() || X.BTWW(0f, this.t, 1f))
			{
				this.sw = 0f;
				this.dw = 0f;
			}
			else if (this.dt >= 0f)
			{
				this.sw = this.dw;
				this.sh = this.dh;
				this.pos_animate_sin_when_active = true;
			}
			this.wh_animate_cos = this.sw > 0f && this.sh > 0f;
			this._bkg_scale |= 8U;
			if (_w >= 0f)
			{
				this.w = _w;
				this.dw = _w;
			}
			if (_h >= 0f)
			{
				this.dh = (this.h = _h);
			}
			this.dt = X.Mn(this.dt, 0f);
			base.resetCollider();
			return this;
		}

		public override MsgBox wh_anim(float _w, float _h = -1f, bool anim_w = true, bool anim_h = true)
		{
			this.wh(_w, _h);
			if (!anim_w)
			{
				this.sw = this.dw;
			}
			if (!anim_h)
			{
				this.sh = this.dh;
			}
			return this;
		}

		public override MsgBox make(string _tex)
		{
			base.make(_tex);
			if (this.MdEmb == null && this.frametype != UiBox.FRAMETYPE.NONE)
			{
				this.MdEmb = this.MMRD.Make(this.EmbMtr);
				this.StencilRefMask(this.stencil_ref_mask_, true);
			}
			return this;
		}

		public int stencil_ref_mask
		{
			get
			{
				return this.stencil_ref_mask_;
			}
			set
			{
				this.StencilRefMask(value, false);
			}
		}

		public UiBox StencilRefMask(int value, bool force = false)
		{
			if (value == this.stencil_ref_mask_ && !force)
			{
				return this;
			}
			this.stencil_ref_mask_ = value;
			if (this.MdBkg != null)
			{
				if (value < 0)
				{
					if (this.MdBkg.getMaterial().shader != MTRX.ShaderMesh)
					{
						this.MMRD.setMaterial(this.MdBkg, MTRX.getMtr(BLEND.NORMAL, -1), false);
					}
				}
				else
				{
					Material mtr = MTRX.getMtr(BLEND.MASK, this.stencil_ref_mask);
					this.MMRD.setMaterial(this.MdBkg, mtr, false);
				}
			}
			return this;
		}

		public override MsgBox run(float fcnt, bool force_draw = false)
		{
			if (UiBox.AMouseDownBox != null)
			{
				if (UiBox.AMouseDownBox.Count == 0 || UiBox.click_focus_decline_by_designer)
				{
					UiBox.AMouseDownBox = null;
				}
				else if (UiBox.AMouseDownBox[0] == this)
				{
					UiBox uiBox = null;
					float num = 0f;
					int count = UiBox.AMouseDownBox.Count;
					for (int i = 0; i < count; i++)
					{
						float z = UiBox.AMouseDownBox[i].transform.position.z;
						if (uiBox == null || num > z)
						{
							uiBox = UiBox.AMouseDownBox[i];
							num = z;
						}
					}
					if (uiBox != null)
					{
						UiBox.FocusTo(uiBox);
					}
					UiBox.AMouseDownBox = null;
				}
			}
			if (!base.run(fcnt, force_draw))
			{
				return null;
			}
			bool flag = force_draw || X.D;
			if (base.visible && this.ginitted)
			{
				if (this.FocusParent != null || this.use_focus_)
				{
					if (this.isFocused())
					{
						if (this.focus_t < 0)
						{
							this.focus_t = X.Mx(0, 20 + this.focus_t);
							this.use_focus_ = true;
						}
					}
					else if (this.focus_t >= 0)
					{
						this.focus_t = X.Mn(-1, -20 + this.focus_t);
					}
				}
				if (!UiBox.click_focus_decline_by_designer && this.mouse_click_focus_ > 0 && base.isActive())
				{
					if (IN.isMouseOn() && this.isMouseOver() && !UiBox.FlgLockFocus.isActive())
					{
						if (this.mouse_click_focus_ == 1)
						{
							this.mouse_click_focus_ = 2;
							if (UiBox.AMouseDownBox == null)
							{
								UiBox.AMouseDownBox = new List<UiBox>();
							}
							UiBox.AMouseDownBox.Insert(0, this);
						}
					}
					else if (this.mouse_click_focus_ == 2)
					{
						this.mouse_click_focus_ = 1;
					}
				}
				if (flag)
				{
					if (this.t >= 0f)
					{
						if (this.dt < 0f)
						{
							this.dt = 0f;
							this.redrawBg(0f, 0f, true);
						}
						if (!base.show_delaying)
						{
							this.dt += (float)X.AF;
							if (this.dt <= 25f)
							{
								this.redrawBg(0f, 0f, true);
							}
						}
					}
					else
					{
						this.dt = -1f;
						this.redrawBg(0f, 0f, true);
					}
					if ((this.use_focus_ || X.BTW(-20f, (float)this.focus_t, 20f)) && !base.show_delaying)
					{
						if (this.focus_t >= 0)
						{
							if (this.focus_t < 25)
							{
								this.focus_t += X.AF;
								this.fineFocusColor();
							}
						}
						else if (this.focus_t > -25)
						{
							this.focus_t -= X.AF;
							this.fineFocusColor();
						}
					}
				}
				if (this.need_bg_redraw_flag)
				{
					this.redrawBgExecute();
				}
			}
			return this;
		}

		protected override void calcPosition(float tzs = -1f)
		{
			if (tzs < 0f && this.t >= 0f && this.pos_animate_sin_when_active)
			{
				base.calcPosition(X.ZSIN((float)this.post, (float)this.maxt_pos));
				return;
			}
			base.calcPosition(tzs);
		}

		protected override MsgBox redrawBg(float w = 0f, float h = 0f, bool update_mrd = true)
		{
			this.need_bg_redraw_flag = true;
			return this;
		}

		private UiBox redrawBgExecute()
		{
			float num = ((this.t >= 0f) ? (this.wh_animate_cos ? X.ZCOS(this.dt, 20f * this.resize_speed_when_active) : X.ZSIN2(this.dt, 20f)) : X.ZSIN2((float)this.t_hide + this.t, (float)this.t_hide));
			float num2 = X.NI((this.t >= 0f) ? this.sw : 0f, this.dw, num);
			float num3 = X.NI((this.t >= 0f) ? this.sh : 0f, this.dh, num);
			base.redrawBg(num2 + this.lmarg + this.rmarg, num3 + this.tmarg + this.bmarg, true);
			this.need_bg_redraw_flag = false;
			if (this.frametype_ != UiBox.FRAMETYPE.NONE && this.frametype_ != UiBox.FRAMETYPE.NO_OVERRIDE)
			{
				bool flag = (base.isActive() ? ((this._bkg_scale & 8U) > 0U) : ((this._bkg_scale & 16U) > 0U));
				num = ((this.t >= 0f) ? X.ZSIN((this.t - (float)this.delayt) / (float)this._appear_time - 0.25f, 0.75f) : X.ZSINV(((float)this._appear_time + this.t) / (float)this._appear_time - 0.5f, 0.5f));
				this.MdEmb.Col = MTRX.cola.Set(this.NowColEmblem).setA1((!flag && base.isActive()) ? 1f : num).C;
				float num4;
				float num5;
				if (flag)
				{
					num4 = num2 + this.lmarg + this.rmarg;
					num5 = num3 + this.tmarg + this.bmarg;
				}
				else
				{
					float num6 = X.MMX(38f, X.Mn(base.swidth, base.sheight) * 0.2f, 80f);
					num4 = base.swidth + 4f + (1f - num) * num6;
					num5 = base.sheight + 2f + (1f - num) * num6;
				}
				if (this.frametype_ == UiBox.FRAMETYPE.DARK)
				{
					for (int i = 0; i < 4; i++)
					{
						this.MdEmb.Identity().Scale((float)X.MPF(i < 2), (float)X.MPF(i % 2 == 1), false);
						this.MdEmb.initForImg(MTRX.ItemGetDescFrame, 0).DrawScaleGraph2(-4f, num3 * 0.5f - 32f + 12f, 1f, 0f, 1f, 1f, null);
					}
					this.MdEmb.Identity();
				}
				else if (this.frametype_ == UiBox.FRAMETYPE.ONELINE)
				{
					MTRX.TalkerFrame3.DrawTo(this.MdEmb, 0f, 0f, num4, num5);
				}
				else if (this.frametype_ != UiBox.FRAMETYPE.DARK_SIMPLE)
				{
					MTRX.MsgFrame3.DrawTo(this.MdEmb, 0f, 0f, num4, num5);
				}
				this.MdEmb.updateForMeshRenderer(false);
			}
			return this;
		}

		public UiBox.FRAMETYPE frametype
		{
			get
			{
				return this.frametype_;
			}
			set
			{
				if (value == this.frametype_)
				{
					return;
				}
				bool flag = value == UiBox.FRAMETYPE.DARK || value == UiBox.FRAMETYPE.DARK_SIMPLE;
				bool flag2 = this.frametype_ == UiBox.FRAMETYPE.DARK || this.frametype_ == UiBox.FRAMETYPE.DARK_SIMPLE;
				this.frametype_ = value;
				if (flag != flag2)
				{
					this.fineFocusColor();
				}
				if (this.MdEmb != null)
				{
					Material embMtr = this.EmbMtr;
					if (this.MdEmb.getMaterial() != embMtr)
					{
						this.MMRD.setMaterial(this.MdEmb, embMtr, false);
					}
					if (this.frametype_ == UiBox.FRAMETYPE.NONE || this.frametype_ == UiBox.FRAMETYPE.NO_OVERRIDE)
					{
						this.MdEmb.clear(false, false);
						return;
					}
					this.redrawBgExecute();
				}
			}
		}

		private Material EmbMtr
		{
			get
			{
				if (this.frametype_ != UiBox.FRAMETYPE.DARK)
				{
					return MTRX.MIicon.getMtr(BLEND.MUL, -1);
				}
				return MTRX.MIicon.getMtr(BLEND.NORMAL, -1);
			}
		}

		public override MsgBox deactivate()
		{
			this.sh = 0f;
			this.sw = 0f;
			if (base.isActive() && UiBox.CurFocus == this)
			{
				UiBox.BlurFrom(this);
			}
			if (UiBox.AMouseDownBox != null)
			{
				UiBox.AMouseDownBox.Remove(this);
			}
			if (this.mouse_click_focus_ == 2)
			{
				this.mouse_click_focus_ = 1;
			}
			this.pos_animate_sin_when_active = false;
			base.deactivate();
			return this;
		}

		public UiBox addFocusFn(FnUiBoxBindings Fn)
		{
			aBtn.addFnT<FnUiBoxBindings>(ref this.AFnFocus, Fn);
			return this;
		}

		public UiBox addBlurFn(FnUiBoxBindings Fn)
		{
			aBtn.addFnT<FnUiBoxBindings>(ref this.AFnBlur, Fn);
			return this;
		}

		protected bool runFn(FnUiBoxBindings[] AFn)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				FnUiBoxBindings fnUiBoxBindings = AFn[i];
				if (fnUiBoxBindings == null)
				{
					return flag;
				}
				flag = fnUiBoxBindings(this) && flag;
			}
			return flag;
		}

		public bool isFocused()
		{
			return UiBox.CurFocus == this || (this.FocusParent != null && this.FocusParent.isFocused());
		}

		public bool mouse_click_focus
		{
			get
			{
				return this.mouse_click_focus_ > 0;
			}
			set
			{
				this.mouse_click_focus_ = (value ? 1 : 0);
			}
		}

		public bool isMouseOver()
		{
			if (!IN.use_mouse)
			{
				return false;
			}
			Vector3 vector = base.transform.InverseTransformPoint(IN.getMousePos(null));
			float num = (base.swidth / 2f + ((this.collider_margin_pixel != -1000f) ? this.collider_margin_pixel : 0f)) * 0.015625f;
			float num2 = (base.sheight / 2f + ((this.collider_margin_pixel != -1000f) ? this.collider_margin_pixel : 0f)) * 0.015625f;
			return X.BTW(-num, vector.x, num) && X.BTW(-num2, vector.y, num2);
		}

		public bool use_focus
		{
			get
			{
				return this.use_focus_;
			}
			set
			{
				if (this.use_focus_ == value)
				{
					return;
				}
				if (!value)
				{
					this.use_focus_ = false;
					if (this.focus_t < 0)
					{
						this.focus_t = 0;
						return;
					}
				}
				else
				{
					this.use_focus_ = true;
					if (this.focus_t >= 0 && this.isFocused())
					{
						this.focus_t = -1;
					}
				}
			}
		}

		private static UiBox CurFocus;

		protected float sw;

		protected float sh;

		protected float dw;

		protected float dh;

		public bool wh_animate_cos;

		public bool pos_animate_sin_when_active;

		public const float WH_ANIMATE_MAXT = 20f;

		private float dt = -1f;

		public float resize_speed_when_active = 1f;

		public static Color32 ColTop = C32.d2c(3873891291U);

		public static Color32 ColBottom = C32.d2c(3872180932U);

		public static Color32 ColTop_Offline = C32.d2c(3720264367U);

		public static Color32 ColBottom_Offline = C32.d2c(3718750871U);

		public static Color32 ColEmb = C32.d2c(3713356622U);

		public static Color32 ColEmb_Offline = C32.d2c(3716250994U);

		public static Color32 DColTop = C32.d2c(3858759680U);

		public static Color32 DColBottom = C32.d2c(3863694160U);

		public static Color32 DColTop_Offline = C32.d2c(3712107086U);

		public static Color32 DColBottom_Offline = C32.d2c(3712699216U);

		public static Color32 DColEmb = C32.d2c(uint.MaxValue);

		public static Color32 DColEmb_Offline = C32.d2c(3439329279U);

		private Color32 NowColEmblem;

		private int stencil_ref_mask_;

		public UiBox FocusParent;

		private MeshDrawer MdEmb;

		private bool use_focus_;

		private int focus_t;

		private bool need_bg_redraw_flag;

		private const int FOCUS_MAXT = 20;

		private byte mouse_click_focus_;

		public static bool click_focus_decline_by_designer;

		private static List<UiBox> AMouseDownBox;

		private FnUiBoxBindings[] AFnFocus;

		private FnUiBoxBindings[] AFnBlur;

		public static Flagger FlgLockFocus = new Flagger(null, null);

		private UiBox.FRAMETYPE frametype_ = UiBox.FRAMETYPE.MAIN;

		public enum FRAMETYPE : byte
		{
			NONE,
			MAIN,
			ONELINE,
			DARK,
			DARK_SIMPLE,
			NO_OVERRIDE
		}
	}
}
