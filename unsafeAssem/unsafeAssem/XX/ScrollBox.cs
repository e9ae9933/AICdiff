using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class ScrollBox : MonoBehaviourAutoRun, IPauseable, IDesignerBlock, IClickable
	{
		public void Awake()
		{
			this.Clicker = new CLICK(8);
			this.GobView = IN.CreateGob(this, "-ViewPort");
			this.GobView.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			this.BView = IN.CreateGob(base.gameObject, "-grab").AddComponent<aBtn>();
			this.BView.do_not_tip_on_navi_loop = true;
			this.BView.lock_collider_assign_to_click_manager = true;
			this.WhLis = new WheelListener(false);
			this.WhLis.Movable.Set(0f, 0f, 0f, 0f);
			this.WhLis.grab_enabled = true;
			this.WhLis.alloc_hovering_wheel = true;
			this.WhLis.moveable_margin_x = (this.WhLis.moveable_margin_y = 0f);
			this.WhLis.seeable_margin_x = (this.WhLis.seeable_margin_y = 0.46875f);
			this.WhLis.bindToBtn(this.BView);
			this.WhLis.addChangedFn(new FnWheelBindings(this.fnScrollAccessChanged));
			this.WhLis.y_scroll_level = -1f;
			this.WhLis.z_scroll_level = 0.25f;
			this.WhLis.keyboard_translate = -0.65625f;
			this.WhLis.bind();
			this.BView.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			this.area_selectable = this.area_selectable_;
			this.view_shift_z = -0.0625f;
			this.BView.enabled = false;
		}

		public override void OnDestroy()
		{
			if (this.MdBackground != null)
			{
				this.MdBackground.destruct();
			}
			this.MrdBackground = null;
			this.MdBackground = null;
			this.removeFromBelongScroll();
			if (this.WhLis != null)
			{
				this.WhLis.destruct();
			}
			this.BView = null;
			base.OnDestroy();
		}

		public void removeFromBelongScroll()
		{
			ScrollBox belongScroll = this.BelongScroll;
			if (belongScroll != null)
			{
				belongScroll.remClickable(this, false);
			}
			IN.Click.remClickable(this, false);
			this.fine_collider_flag = true;
		}

		public override void OnEnable()
		{
			base.OnEnable();
			if (this.MrdBackground != null)
			{
				this.MrdBackground.enabled = true;
			}
		}

		public override void OnDisable()
		{
			base.OnDisable();
			if (this.MrdBackground != null)
			{
				this.MrdBackground.enabled = false;
			}
			this.container_mouseover_frame_id = 0U;
			this.removeFromBelongScroll();
		}

		public ScrollBox WHAll(float _view_w = -1f, float _view_h = -1f, float _inner_w = -1f, float _inner_h = -1f)
		{
			if (_inner_w >= 0f)
			{
				this.inner_w = _inner_w;
			}
			if (_inner_h >= 0f)
			{
				this.inner_h = _inner_h;
			}
			this.WH(_view_w, _view_h);
			return this.FineExecute();
		}

		public ScrollBox WHAllPx(float _view_w = -1f, float _view_h = -1f, float _inner_w = -1f, float _inner_h = -1f)
		{
			if (_inner_w >= 0f)
			{
				this.inner_w = _inner_w * 0.015625f;
			}
			if (_inner_h >= 0f)
			{
				this.inner_h = _inner_h * 0.015625f;
			}
			this.WH(_view_w * 0.015625f, _view_h * 0.015625f);
			return this.FineExecute();
		}

		public ScrollBox WH(float _view_w = -1f, float _view_h = -1f)
		{
			bool flag = this.MtrMask == null;
			this.FineScrollAnim();
			if (_view_w >= 0f)
			{
				flag = flag || this.view_w != _view_w;
				this.view_w = _view_w;
			}
			if (_view_h >= 0f)
			{
				flag = flag || this.view_h != _view_h;
				this.view_h = _view_h;
			}
			if (this.MakeSlider("ScrollW", ref this.BScrollW, ref this.view_w, ref this.inner_w, this.scroll_x))
			{
				Transform transform = this.BScrollW.transform;
				Vector3 localPosition = transform.localPosition;
				localPosition.y = -this.view_h * 0.5f + 0.09375f;
				localPosition.x = -this.view_w * 0.5f + this.BScrollW.w * 0.015625f * 0.5f;
				localPosition.z = this.scrollbar_shift_z;
				transform.localPosition = localPosition;
				this.WhLis.Movable.x = -this.view_w * 0.5f;
				this.WhLis.Movable.width = this.inner_w;
			}
			else
			{
				this.WhLis.Movable.width = 0f;
			}
			if (this.MakeSlider("ScrollH", ref this.BScrollH, ref this.view_h, ref this.inner_h, this.scroll_y))
			{
				Transform transform2 = this.BScrollH.transform;
				Vector3 localPosition2 = transform2.localPosition;
				localPosition2.x = this.view_w * 0.5f - 0.09375f;
				localPosition2.y = this.view_h * 0.5f - this.BScrollH.w * 0.015625f * 0.5f;
				localPosition2.z = this.scrollbar_shift_z;
				transform2.localPosition = localPosition2;
				this.WhLis.Movable.y = -this.view_h * 0.5f;
				this.WhLis.Movable.height = this.inner_h;
			}
			else
			{
				this.WhLis.Movable.height = 0f;
			}
			if (flag && this.use_background && this.PatternSprite == null)
			{
				this.prepareBackground();
			}
			this.BView.enabled = true;
			this.BView.initializeSkin("transparent", "");
			this.BView.WH(this.view_w * 64f, this.view_h * 64f);
			ButtonSkin skin = this.BView.get_Skin();
			skin.curs_level_x = 1f - 44f / (this.view_w * 64f * 0.5f);
			skin.curs_level_y = -1f + 30f / (this.view_h * 64f * 0.5f);
			this.WhLis.resetShowingArea(-1000f, -1000f, -1000f, this.view_w, this.view_h);
			this.fineWhlisArea();
			this.fine_flag = 3;
			return this;
		}

		private void fineWhlisArea()
		{
			this.WhLis.resetShowingArea((this.moveable_x > 0f) ? (this.moveable_x * this.scroll_x) : 0f, (this.moveable_y > 0f) ? (this.moveable_y * this.scroll_y) : 0f, -1000f, -1000f, -1000f);
		}

		private bool fnScrollAccessChanged(WheelListener WhLis, float x, float y, float z)
		{
			if (this.moveable_x > 0f)
			{
				this.scroll_x = x / this.moveable_x;
			}
			if (this.moveable_y > 0f)
			{
				this.scroll_y = y / this.moveable_y;
			}
			this.fine_flag = 7;
			return true;
		}

		private ScrollBox FineExecuteInner(float _inner_w = -1f, float _inner_h = -1f)
		{
			if (_inner_w >= 0f)
			{
				this.inner_w = _inner_w;
			}
			if (_inner_h >= 0f)
			{
				this.inner_h = _inner_h;
			}
			this.WH(-1f, -1f);
			this.FineExecute();
			return this;
		}

		private bool MakeSlider(string mname, ref aBtnMeter BScroll, ref float view_wh, ref float inner_wh, float def_val)
		{
			if (view_wh > 0f)
			{
				inner_wh = X.Mx(view_wh, inner_wh);
				bool flag = false;
				if (BScroll == null)
				{
					GameObject gameObject = IN.CreateGob(base.gameObject, mname);
					BScroll = gameObject.AddComponent<aBtnMeter>();
					BScroll.addChangedFn(new aBtnMeter.FnMeterBindings(this.fineScroll));
					BScroll.addDownFn(new FnBtnBindings(this.fnQuitAnimating));
					BScroll.addUpFn(new FnBtnBindings(this.fnScrollDragQuit));
					BScroll.fnBtnMeterLine = new FnBtnMeterLine(this.fnBtnMeterLine);
					BScroll.default_value = def_val;
					flag = true;
				}
				else
				{
					BScroll.setValue(def_val, false);
				}
				BScroll.WH(X.Mx(10f, view_wh * 64f - 10f), 10f);
				BScroll.minval = 0f;
				BScroll.maxval = 1f;
				int num = X.Mx(2, X.IntC((inner_wh + view_wh) / view_wh));
				BScroll.valintv = 1f / (float)num;
				BScroll.valcnt = 0f;
				BScroll.lock_collider_assign_to_click_manager = true;
				BScroll.vertical = mname == "ScrollH";
				BScroll.split_integer = false;
				BScroll.px_bar_width = X.Mx(view_wh / inner_wh * BScroll.w, 10f);
				BScroll.initializeSkin("scroll", "");
				BScroll.return_integer = false;
				BScroll.click_snd = "";
				BScroll.unselectable(true);
				BScroll.slider_dragging_snd = "_drag_pull";
				if (base.enabled && this.show_scroll_bar_ && BScroll.px_bar_width < BScroll.w)
				{
					if (flag || !BScroll.isActive())
					{
						BScroll.bind();
					}
				}
				else if (flag || BScroll.isActive())
				{
					BScroll.hide();
				}
				return true;
			}
			if (BScroll != null)
			{
				global::UnityEngine.Object.Destroy(BScroll.gameObject);
				BScroll = null;
			}
			return false;
		}

		public void setSliderColor(Color32 _BarColor, Color32 _PushColor)
		{
			if (this.BScrollW != null)
			{
				(this.BScrollW.get_Skin() as ButtonSkinMeterScroll).setColor(_BarColor, _PushColor);
			}
			if (this.BScrollH != null)
			{
				(this.BScrollH.get_Skin() as ButtonSkinMeterScroll).setColor(_BarColor, _PushColor);
			}
		}

		public bool fnQuitAnimating(aBtn B)
		{
			this.scroll_anim_t = this.scroll_anim_maxt;
			SND.Ui.play("tool_drag_init", false);
			this.runFn(this.AFnDragInit, B);
			return true;
		}

		public bool fnScrollDragQuit(aBtn B)
		{
			this.fnQuitAnimating(B);
			SND.Ui.play("tool_drag_quit", false);
			this.runFn(this.AFnDragQuit, B);
			return true;
		}

		public ScrollBox AssignToInner(Transform Trs)
		{
			Trs.SetParent(this.GobView.transform, false);
			return this;
		}

		public ScrollBox AssignToInnerAt(Transform Trs, Vector2 LocalPosTopZero)
		{
			Trs.SetParent(this.GobView.transform, false);
			Vector3 localPosition = Trs.localPosition;
			localPosition.x = LocalPosTopZero.x;
			localPosition.y = -LocalPosTopZero.y;
			Trs.localPosition = localPosition;
			return this;
		}

		public float fnBtnMeterLine(aBtnMeter B, int index, float val)
		{
			if (val == 0f || val == 1f)
			{
				return 0f;
			}
			return 0.25f;
		}

		private bool fineScroll(aBtnMeter _B, float pre_value, float cur_value)
		{
			if (_B == this.BScrollW)
			{
				this.scroll_x = cur_value;
			}
			if (_B == this.BScrollH)
			{
				this.scroll_y = cur_value;
			}
			this.fineAutoScrollDelay(true);
			this.fine_flag |= 1;
			return true;
		}

		public ScrollBox translateScrollP(float pixel_x, float pixel_y, bool animate = false)
		{
			return this.setScrollLevelTo((this.BScrollW != null) ? X.ZLINE(this.scroll_x + pixel_x * 0.015625f / this.moveable_x) : this.scroll_x, (this.BScrollH != null) ? X.ZLINE(this.scroll_y - pixel_y * 0.015625f / this.moveable_y) : this.scroll_y, animate);
		}

		public ScrollBox setScrollLevelTo(float sc_x, float sc_y, bool animate = false)
		{
			this.fineAutoScrollDelay(true);
			if (!animate)
			{
				this.scroll_anim_t = this.scroll_anim_maxt;
				this.scroll_x = sc_x;
				this.scroll_y = sc_y;
				this.fine_flag |= 3;
			}
			else
			{
				this.scroll_anim_sx = this.scroll_x;
				this.scroll_anim_sy = this.scroll_y;
				this.scroll_anim_dx = sc_x;
				this.scroll_anim_dy = sc_y;
				this.scroll_anim_t = 0;
			}
			return this;
		}

		public bool isAnimating()
		{
			return this.scroll_anim_t < this.scroll_anim_maxt;
		}

		public ScrollBox FineScrollAnim()
		{
			if (this.isAnimating())
			{
				this.scroll_anim_t = this.scroll_anim_maxt;
				this.setScrollLevelTo(this.scroll_anim_dx, this.scroll_anim_dy, false);
			}
			return this;
		}

		public ScrollBox Fine(bool immediate = false)
		{
			this.fine_flag |= 3;
			if (immediate)
			{
				this.FineExecute();
			}
			return this;
		}

		private ScrollBox FineExecute()
		{
			float moveable_x = this.moveable_x;
			float moveable_y = this.moveable_y;
			bool flag = this.fine_flag != 0;
			if ((this.fine_flag & 1) > 0)
			{
				Vector3 localPosition = this.GobView.transform.localPosition;
				localPosition.x = -this.view_w * 0.5f - moveable_x * this.scroll_x;
				localPosition.y = this.view_h * 0.5f + moveable_y * this.scroll_y;
				this.GobView.transform.localPosition = localPosition;
				if ((this.fine_flag & 4) == 0)
				{
					this.fineWhlisArea();
				}
			}
			if ((this.fine_flag & 2) > 0)
			{
				if (this.BScrollW != null)
				{
					this.BScrollW.setValue(this.scroll_x, false);
					this.BScrollW.fine_flag = true;
				}
				if (this.BScrollH != null)
				{
					this.BScrollH.setValue(this.scroll_y, false);
					this.BScrollH.fine_flag = true;
				}
				this.prepareBackground();
			}
			else if (this.PatternSprite != null)
			{
				this.prepareBackground();
			}
			this.fine_flag = 0;
			if (flag)
			{
				this.runFn(this.AFn, null);
			}
			return this;
		}

		public void prepareBackground()
		{
			if (this.PatternSprite != null)
			{
				if (this.MtrMask == null)
				{
					this.MtrMask = MTRX.getMtr(BLEND.MASK, this.stencil_ref);
				}
				if (this.MdBackground == null)
				{
					this.MdBackground = MeshDrawer.prepareMeshRenderer(base.gameObject, this.MtrMask, 0.00125f, -1, null, false, false);
					this.MrdBackground = base.GetComponent<MeshRenderer>();
					this.MrdBackground.sharedMaterials = new Material[] { this.MtrMask };
				}
				this.MdBackground.use_cache = false;
				this.MdBackground.Col = MTRX.ColWhite;
				float num = this.scroll_x * this.moveable_x * 64f;
				float num2 = (1f - this.scroll_y) * this.moveable_y * 64f;
				this.MdBackground.PatternFill(-this.view_w / 2f * 64f, -this.view_h / 2f * 64f, this.view_w * 64f, this.view_h * 64f, this.PatternSprite, this.pattern_w_px, this.pattern_h_px, this.pattern_w_px - num * 0.5f % this.pattern_w_px, this.pattern_h_px - num2 * 0.5f % this.pattern_h_px);
			}
			else
			{
				if (this.MtrMask == null)
				{
					this.MtrMask = MTRX.getMtr(BLEND.MASK, this.stencil_ref);
				}
				if (this.MdBackground == null)
				{
					this.MdBackground = MeshDrawer.prepareMeshRenderer(base.gameObject, this.MtrMask, 0.00125f, -1, null, false, false);
					this.MrdBackground = base.GetComponent<MeshRenderer>();
					this.MrdBackground.sharedMaterials = new Material[] { this.MtrMask };
				}
				if (this.background_color >= 16777216U || this.stencil_ref >= 0)
				{
					this.MdBackground.Col = C32.d2c(this.background_color);
					this.MdBackground.Rect(0f, 0f, this.view_w, this.view_h, true);
				}
			}
			if (this.border_color >= 16777216U)
			{
				if (this.PatternSprite != null)
				{
					this.MdBackground.initForImg(MTRX.IconWhite, 0);
				}
				this.MdBackground.Col = C32.d2c(this.border_color);
				if (this.border_radius > 0U)
				{
					this.MdBackground.KadomaruRect(0f, 0f, this.view_w * 64f, this.view_h * 64f, this.border_radius, 1f, false, 0f, 0f, false);
				}
				else
				{
					this.MdBackground.Box(0f, 0f, this.view_w * 64f, this.view_h * 64f, 1f, false);
				}
			}
			this.MdBackground.updateForMeshRenderer(false);
		}

		protected override bool runIRD(float fcnt)
		{
			if (!this.prepared)
			{
				this.WH(-1f, -1f);
			}
			if (this.fine_flag > 0)
			{
				this.FineExecute();
			}
			if (this.fine_collider_flag_ == 1)
			{
				this.fine_collider_flag_ = 0;
				ScrollBox belongScroll = this.BelongScroll;
				if (belongScroll != null)
				{
					belongScroll.addClickable(this);
				}
				else
				{
					IN.Click.addClickable(this);
				}
			}
			if (this.scroll_anim_t < this.scroll_anim_maxt)
			{
				int num = this.scroll_anim_t + 1;
				this.scroll_anim_t = num;
				float num2 = X.ZSIN((float)num, (float)this.scroll_anim_maxt);
				this.scroll_x = X.NAIBUN_I(this.scroll_anim_sx, this.scroll_anim_dx, num2);
				this.scroll_y = X.NAIBUN_I(this.scroll_anim_sy, this.scroll_anim_dy, num2);
				this.fine_flag |= 3;
				if (this.t_auto_scroll != -1001f)
				{
					this.fineAutoScrollDelay(false);
				}
			}
			if (this.t_auto_scroll != -1000f && (this.moveable_x > 0.546875f || this.moveable_y > 0.546875f))
			{
				bool flag = this.t_auto_scroll > -1000f;
				if (this.t_auto_scroll == -1001f)
				{
					if (this.scroll_anim_t >= this.scroll_anim_maxt)
					{
						this.t_auto_scroll = 920f;
						flag = true;
					}
					else
					{
						flag = false;
					}
				}
				if (flag)
				{
					if (this.t_auto_scroll >= 1000f)
					{
						if (this.moveable_x > this.moveable_y)
						{
							if (this.scroll_x < 1f)
							{
								this.scroll_x = X.Mn(this.scroll_x + 0.015625f / this.moveable_x, 1f);
								this.t_auto_scroll = 1000f;
								this.fine_flag |= 3;
							}
							else
							{
								this.t_auto_scroll += fcnt;
								if (this.t_auto_scroll >= 1120f)
								{
									this.setScrollLevelTo(0f, this.scroll_y, true);
									this.t_auto_scroll = -1001f;
									this.fine_flag |= 3;
								}
							}
						}
						else if (this.scroll_y < 1f)
						{
							this.scroll_y = X.Mn(this.scroll_y + 0.015625f / this.moveable_y, 1f);
							this.t_auto_scroll = 1000f;
							this.fine_flag |= 3;
						}
						else
						{
							this.t_auto_scroll += fcnt;
							if (this.t_auto_scroll >= 1120f)
							{
								this.setScrollLevelTo(this.scroll_x, 0f, true);
								this.t_auto_scroll = -1001f;
								this.fine_flag |= 3;
							}
						}
					}
					else
					{
						this.t_auto_scroll += fcnt;
					}
				}
			}
			return true;
		}

		public void startAutoScroll(int start_delay = 120)
		{
			this.t_auto_scroll = (float)(1000 - start_delay);
		}

		public void stopAutoScroll()
		{
			this.t_auto_scroll = -1000f;
		}

		public void fineAutoScrollDelay(bool strong = true)
		{
			if (this.t_auto_scroll != -1000f)
			{
				if (0f <= this.t_auto_scroll && this.t_auto_scroll < 1000f && !strong)
				{
					this.t_auto_scroll = X.Mx(880f, this.t_auto_scroll);
					return;
				}
				this.t_auto_scroll = 880f;
			}
		}

		public Vector2 getAnimScrollingShift()
		{
			if (this.scroll_anim_t < this.scroll_anim_maxt)
			{
				return new Vector2((this.scroll_anim_dx - this.scroll_x) * this.moveable_x, -(this.scroll_anim_dy - this.scroll_y) * this.moveable_y);
			}
			return Vector2.zero;
		}

		public Vector2 getShift()
		{
			Vector2 animScrollingShift = this.getAnimScrollingShift();
			animScrollingShift.x += this.scroll_x * this.moveable_x + this.view_w * 0.5f;
			animScrollingShift.y -= this.scroll_y * this.moveable_y + this.view_h * 0.5f;
			return animScrollingShift;
		}

		public ScrollBox bind()
		{
			this.fine_flag = 3;
			base.enabled = true;
			if (this.BView != null)
			{
				this.BView.bind();
				this.WhLis.bind();
			}
			this.fineBinding();
			return this;
		}

		public void fineBinding()
		{
			if (this.BScrollW != null)
			{
				if (base.enabled && this.show_scroll_bar_ && this.BScrollW.px_bar_width < this.BScrollW.w)
				{
					this.BScrollW.bind();
				}
				else
				{
					this.BScrollW.hide();
				}
			}
			if (this.BScrollH != null)
			{
				if (base.enabled && this.show_scroll_bar_ && this.BScrollH.px_bar_width < this.BScrollH.w)
				{
					this.BScrollH.bind();
					return;
				}
				this.BScrollH.hide();
			}
		}

		public ScrollBox hide()
		{
			base.enabled = false;
			if (this.scroll_anim_t < this.scroll_anim_maxt)
			{
				this.scroll_anim_t = this.scroll_anim_maxt;
				this.setScrollLevelTo(this.scroll_anim_dx, this.scroll_anim_dy, false);
			}
			if (this.BView != null)
			{
				this.BView.hide();
				this.WhLis.hide();
			}
			this.fineBinding();
			this.container_mouseover_frame_id = 0U;
			return this;
		}

		public void addClickable(IClickable Clk)
		{
			this.Clicker.addClickable(Clk);
		}

		public bool remClickable(IClickable Clk, bool do_not_call_click = false)
		{
			return this.Clicker.remClickable(Clk, do_not_call_click);
		}

		public bool getClickable(Vector2 PosU, out IClickable Res)
		{
			Res = null;
			if (!base.enabled || this.BView == null)
			{
				return false;
			}
			if (!this.BView.getClickable(PosU, out Res))
			{
				return false;
			}
			this.container_mouseover_frame_id = IN.Click.mouse_checkng_frame_id;
			if (this.BScrollW != null && this.BScrollW.enabled && this.BScrollW.getClickable(PosU, out Res))
			{
				return true;
			}
			if (this.BScrollH != null && this.BScrollH.enabled && this.BScrollH.getClickable(PosU, out Res))
			{
				return true;
			}
			if (this.Clicker.checkAt(PosU, out Res))
			{
				return true;
			}
			Res = this.BView;
			return true;
		}

		public bool Has(IClickable Clk)
		{
			return Clk != null && this.Clicker.isin(Clk) >= 0;
		}

		public void OnPointerEnter()
		{
		}

		public void OnPointerExit()
		{
		}

		public bool OnPointerDown()
		{
			return false;
		}

		public void OnPointerUp(bool clicking)
		{
		}

		public ScrollBox reveal(Transform T, float posx = 0f, float posy = 0f, bool animate = true)
		{
			Vector3 vector = T.TransformPoint(new Vector2(posx, posy));
			vector.y += this.view_h / 2f;
			vector = this.GobView.transform.InverseTransformPoint(vector);
			float moveable_x = this.moveable_x;
			float moveable_y = this.moveable_y;
			this.setScrollLevelTo((moveable_x <= 0f) ? 0f : X.MMX(0f, vector.x / moveable_x, 1f), (moveable_y <= 0f) ? 0f : X.MMX(0f, -vector.y / moveable_y, 1f), animate);
			return this;
		}

		public bool isShowing(IDesignerBlock B, float margin_pixel_x = 0f, float margin_pixel_y = 0f, float shift_pixel_x = 0f, float shift_pixel_y = 0f)
		{
			float num = this.moveable_x * this.scroll_x;
			float num2 = -this.moveable_y * this.scroll_y;
			float num3 = num + this.view_w;
			float num4 = num2 - this.view_h;
			if (num <= X.Mx(0f, margin_pixel_x * 0.015625f))
			{
				num -= 30f;
			}
			if (num3 >= this.moveable_x + this.view_w - X.Mx(0f, margin_pixel_x * 0.015625f))
			{
				num3 += 30f;
			}
			if (num4 <= -(this.moveable_y + this.view_h) + X.Mx(0f, margin_pixel_y * 0.015625f))
			{
				num4 -= 30f;
			}
			if (num2 >= -X.Mx(0f, margin_pixel_y * 0.015625f))
			{
				num2 += 30f;
			}
			Transform transform = B.getTransform();
			Vector3 vector = this.GobView.transform.InverseTransformPoint(transform.TransformPoint(new Vector2(shift_pixel_x * 0.015625f, shift_pixel_y * 0.015625f)));
			return (this.moveable_x <= 0f || X.isCovering(num, num3, vector.x, vector.x, margin_pixel_x * 0.015625f)) && (this.moveable_y <= 0f || X.isCovering(num4, num2, vector.y, vector.y, margin_pixel_y * 0.015625f));
		}

		public bool isShowingContaining(IDesignerBlock B, float margin_pixel_x = 0f, float margin_pixel_y = 0f)
		{
			return this.isShowingContaining(B.getTransform(), B.get_swidth_px() * 0.5f + margin_pixel_x, B.get_sheight_px() * 0.5f + margin_pixel_y);
		}

		public bool isShowingContaining(Transform T, float margin_pixel_x = 0f, float margin_pixel_y = 0f)
		{
			float num = this.moveable_x * this.scroll_x;
			float num2 = -this.moveable_y * this.scroll_y;
			float num3 = num + this.view_w;
			float num4 = num2 - this.view_h;
			if (num3 < num)
			{
				num = (num3 = X.NI(num3, num, 0.5f));
			}
			if (num4 > num2)
			{
				num2 = (num4 = X.NI(num4, num2, 0.5f));
			}
			Vector3 vector = this.GobView.transform.InverseTransformPoint(T.TransformPoint(new Vector2(0f, 0f)));
			return (this.moveable_x <= 0f || X.isContaining(num, num3, vector.x - margin_pixel_x * 0.015625f, vector.x + margin_pixel_x * 0.015625f, 0f)) && (this.moveable_y <= 0f || X.isContaining(num4, num2, vector.y - margin_pixel_y * 0.015625f, vector.y + margin_pixel_y * 0.015625f, 0f));
		}

		public ScrollBox revealN(IDesignerBlock B, bool animate = true)
		{
			return this.reveal(B, animate, (REVEALTYPE)5);
		}

		public ScrollBox reveal(IDesignerBlock B, bool animate = true, REVEALTYPE type = REVEALTYPE.ALWAYS)
		{
			Vector3 vector = B.getTransform().TransformPoint(new Vector2(0f, 0f));
			vector = this.GobView.transform.InverseTransformPoint(vector);
			float num = (this.isAnimating() ? this.scroll_anim_dx : this.scroll_x);
			float num2 = (this.isAnimating() ? this.scroll_anim_dy : this.scroll_y);
			bool flag = (type & REVEALTYPE.CENTER) > REVEALTYPE.ALWAYS;
			if ((type & REVEALTYPE.IF_HIDING) != REVEALTYPE.ALWAYS && this.isShowingContaining(B, B.get_swidth_px() * 0.5f + 10f, B.get_sheight_px() * 0.5f + 10f))
			{
				return this;
			}
			bool flag2 = (type & REVEALTYPE.MINIMIZE) == REVEALTYPE.MINIMIZE;
			if (this.moveable_x > 0f)
			{
				float num3 = B.get_swidth_px() * 0.015625f * 0.5f + (flag ? 0f : 0.625f);
				float num4 = X.MMX(0f, (vector.x - (this.view_w - num3)) / this.moveable_x, 1f);
				float num5 = X.MMX(0f, (vector.x - num3) / this.moveable_x, 1f);
				if (num5 >= num4 || (!flag2 && flag))
				{
					num = (num4 + num5) / 2f;
				}
				else if (flag2)
				{
					float num6 = X.Abs(num4 - this.scroll_x);
					float num7 = X.Abs(num5 - this.scroll_x);
					float num8 = X.Abs((num4 + num5) / 2f - this.scroll_x);
					if (num6 > num7)
					{
						num = ((num7 > num8) ? ((num4 + num5) / 2f) : num5);
					}
					else
					{
						num = ((num6 > num8) ? ((num4 + num5) / 2f) : num4);
					}
				}
				else
				{
					num = num5;
				}
			}
			if (this.moveable_y > 0f)
			{
				float num9 = B.get_sheight_px() * 0.015625f * 0.5f + (flag ? 0f : 0.75f);
				float num10 = 1f / this.moveable_y;
				float num11 = (-vector.y + num9) * num10;
				float num12 = (-vector.y - num9) * num10;
				if (!flag2 && flag)
				{
					num2 = X.NI(num11, num12, 0.5f) - this.view_h * 0.5f * num10;
				}
				else
				{
					float num13 = num2 + this.view_h * 0.5f * num10;
					if (num11 < num13 != num12 < num13)
					{
						num2 = X.NI(num11, num12, 0.5f) - this.view_h * 0.5f * num10;
					}
					else
					{
						float num14 = (float)X.MPF(num13 < num11);
						if (flag2)
						{
							float num15;
							float num16;
							if (num14 < 0f)
							{
								num15 = num11 - num9 * num10;
								num16 = num12 - num9 * num10;
							}
							else
							{
								num15 = num11 + (-this.view_h + num9) * num10;
								num16 = num12 + (-this.view_h + num9) * num10;
							}
							if (num14 < 0f == num15 < num2 && num14 < 0f != num16 < num2)
							{
								num2 = num15;
							}
							else if (num14 < 0f != num15 < num2 && num14 < 0f == num16 < num2)
							{
								num2 = num16;
							}
							else if (X.Abs(num15 - num2) > X.Abs(num16 - num2))
							{
								num2 = num15;
							}
							else
							{
								num2 = num16;
							}
						}
						else if ((type & REVEALTYPE.IF_HIDING) != REVEALTYPE.ALWAYS)
						{
							if (num14 > 0f)
							{
								num2 = num12;
							}
							else
							{
								num2 = num11 - this.view_h * num10;
							}
						}
						else
						{
							num2 = X.NI(num11, num12, 0.5f) - this.view_h * 0.5f * num10;
						}
					}
				}
			}
			this.setScrollLevelTo(X.saturate(num), X.saturate(num2), animate);
			return this;
		}

		public void addDragInitFn(ScrollBox.FnScrollBoxBindings Fn)
		{
			aBtn.addFnT<ScrollBox.FnScrollBoxBindings>(ref this.AFnDragInit, Fn);
		}

		public void addDragQuitFn(ScrollBox.FnScrollBoxBindings Fn)
		{
			aBtn.addFnT<ScrollBox.FnScrollBoxBindings>(ref this.AFnDragQuit, Fn);
		}

		public void addChangedFn(ScrollBox.FnScrollBoxBindings Fn)
		{
			aBtn.addFnT<ScrollBox.FnScrollBoxBindings>(ref this.AFn, Fn);
		}

		public bool isDragging()
		{
			return (this.BScrollW != null && this.BScrollW.isPushed()) || (this.BScrollH != null && this.BScrollH.isPushed());
		}

		public void scrollBarDraggingPosReset()
		{
			if (this.BScrollW != null)
			{
				this.BScrollW.resetDraggingPos();
			}
			if (this.BScrollH != null)
			{
				this.BScrollH.resetDraggingPos();
			}
		}

		public aBtn get_BScrollH()
		{
			return this.BScrollH;
		}

		protected bool runFn(ScrollBox.FnScrollBoxBindings[] AFn, aBtn B)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				ScrollBox.FnScrollBoxBindings fnScrollBoxBindings = AFn[i];
				if (fnScrollBoxBindings == null)
				{
					return flag;
				}
				flag = fnScrollBoxBindings(this, B) && flag;
			}
			return flag;
		}

		public float moveable_x
		{
			get
			{
				return this.inner_w - this.view_w;
			}
		}

		public float moveable_y
		{
			get
			{
				return this.inner_h - this.view_h;
			}
		}

		public float scrolled_pixel_y
		{
			get
			{
				return X.Mx(0f, this.moveable_y) * this.scroll_y * 64f;
			}
			set
			{
				if (this.moveable_y <= 0f)
				{
					return;
				}
				this.scroll_y = X.saturate(value * 0.015625f / this.moveable_y);
				this.fine_flag |= 3;
			}
		}

		public float scrolled_pixel_x
		{
			get
			{
				return X.Mx(0f, this.moveable_x) * this.scroll_x * 64f;
			}
			set
			{
				if (this.moveable_x <= 0f)
				{
					return;
				}
				this.scroll_x = X.saturate(value * 0.015625f / this.moveable_x);
				this.fine_flag |= 3;
			}
		}

		public float scrolled_level_x
		{
			get
			{
				return this.scroll_x;
			}
			set
			{
				if (this.moveable_x <= 0f)
				{
					return;
				}
				this.scroll_x = X.saturate(value);
				this.fine_flag |= 3;
			}
		}

		public float scrolled_level_y
		{
			get
			{
				return this.scroll_y;
			}
			set
			{
				if (this.moveable_y <= 0f)
				{
					return;
				}
				this.scroll_y = X.saturate(value);
				this.fine_flag |= 3;
			}
		}

		public bool fine_collider_flag
		{
			get
			{
				return this.fine_collider_flag_ == 1;
			}
			set
			{
				if (this.fine_collider_flag_ >= 2)
				{
					return;
				}
				this.fine_collider_flag_ = (value ? 1 : 0);
			}
		}

		public bool lock_collider_assign_to_click_manager
		{
			get
			{
				return this.fine_collider_flag_ == 2;
			}
			set
			{
				this.fine_collider_flag_ = (value ? 2 : 1);
			}
		}

		public bool alloc_grab
		{
			get
			{
				return this.WhLis == null || this.WhLis.grab_enabled;
			}
			set
			{
				if (this.WhLis != null)
				{
					this.WhLis.grab_enabled = value;
				}
			}
		}

		public bool alloc_wheel
		{
			get
			{
				return this.WhLis == null || this.WhLis.z_scroll_level != 0f;
			}
			set
			{
				if (this.WhLis != null)
				{
					this.WhLis.z_scroll_level = (value ? 0.25f : 0f);
				}
			}
		}

		public float get_swidth_px()
		{
			return this.view_w * 64f;
		}

		public float get_sheight_px()
		{
			return this.view_h * 64f;
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		public void AddSelectableItems(List<aBtn> Al, bool only_front)
		{
		}

		public float view_shift_z
		{
			get
			{
				return this.view_shift_z_;
			}
			set
			{
				if (this.view_shift_z == value)
				{
					return;
				}
				this.view_shift_z_ = value;
				IN.setZ(this.BView.transform, this.view_shift_z_);
			}
		}

		public float get_inner_w()
		{
			return this.inner_w;
		}

		public float get_inner_h()
		{
			return this.inner_h;
		}

		public bool area_selectable
		{
			get
			{
				return this.area_selectable_;
			}
			set
			{
				this.area_selectable_ = value;
				if (this.BView != null)
				{
					this.BView.z_push_click = false;
					this.BView.navi_auto_fill = false;
					this.BView.hover_to_select = value;
					this.BView.click_to_select = value;
					if (!value)
					{
						this.BView.secureNavi();
						return;
					}
					this.BView.clearNavi(15U, false);
				}
			}
		}

		public bool show_scroll_bar
		{
			get
			{
				return this.show_scroll_bar_;
			}
			set
			{
				if (this.show_scroll_bar_ == value)
				{
					return;
				}
				this.show_scroll_bar_ = value;
				this.fineBinding();
			}
		}

		public ScrollBox BelongScroll
		{
			get
			{
				if (this.BelongScroll_ != null)
				{
					return this.BelongScroll_;
				}
				if (this.BelongDesigner != null)
				{
					return this.BelongDesigner.BelongScroll;
				}
				return null;
			}
			set
			{
				if (value != this.BelongScroll_)
				{
					this.removeFromBelongScroll();
					this.BelongScroll_ = value;
					this.fine_collider_flag = true;
				}
			}
		}

		public void Select()
		{
			if (this.area_selectable_ && this.BView != null)
			{
				this.BView.Select(true);
			}
		}

		public GameObject getViewArea()
		{
			return this.GobView;
		}

		public Vector2 getCenterPos()
		{
			return new Vector2(this.inner_w / 2f, this.inner_h / 2f);
		}

		public Vector2 getTopLeftPos()
		{
			return new Vector2(0f, 0f);
		}

		public Vector2 getTopLeftPosMargined(float margx, float margy)
		{
			return new Vector2(margx * 0.015625f, -margy * 0.015625f);
		}

		public bool use_background
		{
			get
			{
				return this.stencil_ref >= 0 || this.background_color >= 16777216U || this.PatternSprite != null || this.border_color >= 16777216U;
			}
		}

		public bool container_mouseover
		{
			get
			{
				return this.container_mouseover_frame_id == IN.Click.mouse_checkng_frame_id;
			}
		}

		public void Pause()
		{
			this.hide();
		}

		public void Resume()
		{
			this.bind();
		}

		public float getFarLength()
		{
			return base.transform.position.z + this.view_shift_z;
		}

		public bool prepared
		{
			get
			{
				return (this.inner_w <= 0f || !(this.BScrollW == null)) && (this.inner_h <= 0f || !(this.BScrollH == null));
			}
		}

		public Sprite PatternSprite;

		public uint background_color;

		public float pattern_w_px;

		public float pattern_h_px;

		public int stencil_ref = -1;

		public uint border_color;

		public uint border_radius;

		private bool show_scroll_bar_ = true;

		private const float wheel_scroll_level = 0.25f;

		private Material MtrMask;

		private BtnContainer<aBtn> Btns;

		private aBtnMeter BScrollW;

		private aBtnMeter BScrollH;

		private WheelListener WhLis;

		private GameObject GobView;

		private ScrollBox BelongScroll_;

		public Designer BelongDesigner;

		public aBtn BView;

		private float view_w;

		private float view_h;

		private float inner_w;

		private float inner_h;

		private float scroll_x;

		private float scroll_y;

		private float scroll_anim_sx;

		private float scroll_anim_sy;

		private float scroll_anim_dx;

		private float scroll_anim_dy;

		private int scroll_anim_maxt = CURS.T_CURSMOVE;

		private int scroll_anim_t = CURS.T_CURSMOVE;

		public float scrollbar_shift_z = -1f;

		public int fine_flag = 3;

		private const float bview_select_xmarg_pixel = 44f;

		private const float bview_select_ymarg_pixel = 30f;

		private bool area_selectable_;

		public uint container_mouseover_frame_id;

		private ScrollBox.FnScrollBoxBindings[] AFnDragInit;

		private ScrollBox.FnScrollBoxBindings[] AFn;

		private ScrollBox.FnScrollBoxBindings[] AFnDragQuit;

		private CLICK Clicker;

		public float t_auto_scroll = -1000f;

		public const int T_AUTO_SCROLL_START = 1000;

		public const int T_AUTO_SCROLL_DELAY = 120;

		private byte fine_collider_flag_ = 1;

		private float view_shift_z_;

		private MeshDrawer MdBackground;

		private MeshRenderer MrdBackground;

		public const float BALL_H = 10f;

		public const float MARGIN = 16f;

		public delegate bool FnScrollBoxBindings(ScrollBox Bx, aBtn B);
	}
}
