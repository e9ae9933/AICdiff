using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class aBtn : MonoBehaviour, IDesignerBlock, IPauseable, IVariableObject, IAlphaSetable, IValotileSetable, IClickable, IRunAndDestroy
	{
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

		public virtual void resetToPool(bool clear_func = true)
		{
			if (this.isSelected())
			{
				this.Deselect(true);
			}
			this.hide();
			if (aBtn.OfflineSelected == this)
			{
				aBtn.OfflineSelected = null;
			}
			this.runner_assigned = false;
			if (clear_func)
			{
				this.AFnClick = null;
				this.AFnDown = null;
				this.AFnUp = null;
				this.AFnHover = null;
				this.AFnOut = null;
			}
			this.SetChecked(false, true);
			base.gameObject.SetActive(false);
			this.autoBind = true;
			this.autoSelect = false;
			this.active = -1;
			if (this.pushed > -1000)
			{
				this.pushed = (clear_func ? (-1001) : (-1000));
			}
			this.hovered = aBtn.HOVER_BIT.NONE;
			this.checked_ = 0f;
			this.locked_ = 0;
			this.removeFromBelongScroll();
			this.fine_collider_flag = true;
			this.af = -1;
			this.do_not_tip_on_navi_loop = false;
			this.clearNavi(15U, false);
		}

		public bool CopyFunctionFrom(aBtn B)
		{
			if (B == null || B == this)
			{
				return false;
			}
			this.AFnClick = B.AFnClick;
			this.AFnDown = B.AFnDown;
			this.AFnUp = B.AFnUp;
			this.AFnHover = B.AFnHover;
			this.AFnOut = B.AFnOut;
			return true;
		}

		public void removeFromBelongScroll()
		{
			ScrollBox belongScroll = this.BelongScroll;
			if (belongScroll != null)
			{
				belongScroll.remClickable(this, true);
			}
			IN.Click.remClickable(this, true);
			this.fine_collider_flag = true;
		}

		protected virtual void Awake()
		{
		}

		protected virtual void OnEnable()
		{
			this.fine_collider_flag = true;
			this.runner_assigned = true;
		}

		protected virtual void OnDisable()
		{
			this.removeFromBelongScroll();
			this.runner_assigned = false;
		}

		public aBtn WH(float _w, float _h)
		{
			this.w = _w;
			this.h = _h;
			if (this.Skin != null)
			{
				this.Skin.WHPx(_w, _h);
				this.fine_flag = true;
			}
			return this;
		}

		public virtual ButtonSkin initializeSkin(string _skin, string _title = "")
		{
			this.skin = ((_skin == null) ? this.skin : _skin);
			if (TX.valid(_title))
			{
				this.title = _title;
			}
			if (this.skin != "")
			{
				if (this.Skin == null)
				{
					this.Skin = this.makeButtonSkin(this.skin);
					if (this.Skin != null && this.title != "")
					{
						this.Skin.setTitle(this.title);
					}
					if (!this.isActive())
					{
						this.Skin.fine_flag = true;
						this.Skin.bindChanged(false);
					}
				}
				this.Skin.fine_flag = true;
			}
			return this.Skin;
		}

		public virtual ButtonSkin makeButtonSkin(string key)
		{
			if (key != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(key);
				if (num <= 1919382533U)
				{
					if (num > 848251934U)
					{
						if (num <= 1141775739U)
						{
							if (num != 1093042769U)
							{
								if (num != 1141775739U)
								{
									goto IL_0568;
								}
								if (!(key == "row"))
								{
									goto IL_0568;
								}
							}
							else
							{
								if (!(key == "normal_hilighted"))
								{
									goto IL_0568;
								}
								this.click_snd = "enter";
								return this.Skin = new ButtonSkinNormalHilighted(this, this.w, this.h);
							}
						}
						else if (num != 1210527768U)
						{
							if (num != 1892056626U)
							{
								if (num != 1919382533U)
								{
									goto IL_0568;
								}
								if (!(key == "row_center"))
								{
									goto IL_0568;
								}
							}
							else
							{
								if (!(key == "box"))
								{
									goto IL_0568;
								}
								this.click_snd = "enter_small";
								return this.Skin = new ButtonSkinBox(this, this.w, this.h);
							}
						}
						else
						{
							if (!(key == "row_toggle"))
							{
								goto IL_0568;
							}
							this.click_snd = "talk_progress";
							return this.Skin = new ButtonSkinRowToggle(this, this.w, this.h);
						}
						this.click_snd = "enter_small";
						ButtonSkinRow buttonSkinRow = new ButtonSkinRow(this, this.w, this.h);
						if (this.skin == "row_center")
						{
							buttonSkinRow.alignx = ALIGN.CENTER;
						}
						return this.Skin = buttonSkinRow;
					}
					if (num <= 104373016U)
					{
						if (num != 69902392U)
						{
							if (num == 104373016U)
							{
								if (key == "checkbox")
								{
									this.click_snd = "talk_progress";
									return this.Skin = new ButtonSkinCheckBox(this, this.w, this.h);
								}
							}
						}
						else if (key == "checkbox_string")
						{
							this.click_snd = "talk_progress";
							return this.Skin = new ButtonSkinCheckBoxStr(this, this.w, this.h);
						}
					}
					else if (num != 179494412U)
					{
						if (num == 848251934U)
						{
							if (key == "url")
							{
								this.click_snd = "talk_progress";
								return this.Skin = new ButtonSkinUrl(this, this.w, this.h);
							}
						}
					}
					else if (key == "radio_string")
					{
						this.click_snd = "talk_progress";
						return this.Skin = new ButtonSkinRadioStr(this, this.w, this.h);
					}
				}
				else if (num <= 2523074583U)
				{
					if (num <= 2324299930U)
					{
						if (num != 2251659123U)
						{
							if (num == 2324299930U)
							{
								if (key == "mini")
								{
									this.click_snd = "talk_progress";
									return this.Skin = new ButtonSkinMini(this, this.w, this.h);
								}
							}
						}
						else if (key == "mini_link")
						{
							this.click_snd = "enter_small";
							return this.Skin = new ButtonSkinMiniLink(this, this.w, this.h);
						}
					}
					else if (num != 2472102578U)
					{
						if (num == 2523074583U)
						{
							if (key == "crop")
							{
								this.click_snd = "";
								return this.Skin = new ButtonSkinCrop(this, this.w, this.h);
							}
						}
					}
					else if (key == "command")
					{
						this.click_snd = "enter";
						return this.Skin = new ButtonSkinCommand(this, this.w, this.h);
					}
				}
				else if (num <= 2923437748U)
				{
					if (num != 2865238487U)
					{
						if (num == 2923437748U)
						{
							if (key == "radio")
							{
								this.click_snd = "talk_progress";
								return this.Skin = new ButtonSkinRadio(this, this.w, this.h);
							}
						}
					}
					else if (key == "popper")
					{
						this.click_snd = "enter";
						return this.Skin = new ButtonSkinPopper(this, this.w, this.h);
					}
				}
				else if (num != 2950907697U)
				{
					if (num != 3728535457U)
					{
						if (num == 3867909202U)
						{
							if (key == "normal")
							{
								this.click_snd = ((this.title == "cancel") ? "cancel" : "enter");
								return this.Skin = new ButtonSkinNormal(this, this.w, this.h);
							}
						}
					}
					else if (key == "kadomaru_icon")
					{
						this.click_snd = "enter_small";
						return this.Skin = new ButtonSkinKadomaruIcon(this, this.w, this.h);
					}
				}
				else if (key == "transparent")
				{
					return this.Skin = new ButtonSkinTransparent(this, this.w, this.h);
				}
			}
			IL_0568:
			X.de("不明なボタンスキン: " + key, null);
			return null;
		}

		protected void Start()
		{
			this.runner_assigned = true;
		}

		protected virtual void StartBtn()
		{
			int num = this.pushed;
			if (this.pushed <= -1000)
			{
				this.pushed = 0;
			}
			this.initializeSkin(this.skin, "");
			if (this.autoBind)
			{
				this.bind();
				if (this.autoSelect)
				{
					this.Select(true);
				}
				this.autoBind = false;
			}
			this.fine_collider_flag = true;
		}

		public virtual void bind()
		{
			if (this.locked_ == 2)
			{
				return;
			}
			bool flag = this.active >= 1;
			if (this.active < 0)
			{
				this.fine_collider_flag = true;
			}
			if ((this.pushed < 0 && this.pushed > -1000) || (!this.btn_enabled && this.pushed > 0))
			{
				this.pushed = 0;
				this.Fine(false);
			}
			this.btn_enabled = true;
			if (this.Skin != null && !flag)
			{
				this.Skin.fine_flag = true;
				if (this.Skin.fine_on_binding_changing)
				{
					this.Skin.Fine();
				}
				this.Skin.bindChanged(true);
			}
		}

		public virtual void hide()
		{
			this.autoBind = false;
			bool flag = this.active >= 1;
			if (this.HovCurs != null)
			{
				this.HovCurs.blur();
			}
			if (this.hovered != aBtn.HOVER_BIT.NONE)
			{
				CURS.Rem(this.hover_curs_category, "");
				this.hovered = aBtn.HOVER_BIT.NONE;
				this.fine_flag = true;
			}
			if (flag && this.pushed > 0)
			{
				this.pushed = 0;
				this.Fine(false);
			}
			if (aBtn.PreSelected == this)
			{
				CURS.Rem(this.focus_curs_category, "");
				if (aBtn.OfflineSelected == null)
				{
					aBtn.OfflineSelected = this;
				}
				aBtn.PreSelected = null;
			}
			try
			{
				this.btn_enabled = false;
			}
			catch
			{
			}
			if (this.Skin != null && flag)
			{
				this.Skin.fine_flag = true;
				this.Skin.bindChanged(false);
			}
		}

		private void StartCheck()
		{
			if (this.pushed <= -1000)
			{
				Bench.P("StartBtn");
				this.StartBtn();
				Bench.Pend("StartBtn");
			}
		}

		public virtual bool run(float fcnt)
		{
			this.StartCheck();
			if (this.active > 0)
			{
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
				if (aBtn.OfflineSelected == this)
				{
					this.Select(false);
				}
				if ((this.hovered & (aBtn.HOVER_BIT)5) != aBtn.HOVER_BIT.NONE)
				{
					if (!IN.use_mouse)
					{
						this.hovered &= (aBtn.HOVER_BIT)(-6);
						this.Fine(false);
					}
				}
				else if ((this.hovered & aBtn.HOVER_BIT.HOV_NOSEL) != aBtn.HOVER_BIT.NONE && IN.use_mouse)
				{
					this.hovered |= (((this.hovered & aBtn.HOVER_BIT.HOV_MOFF) != aBtn.HOVER_BIT.NONE) ? aBtn.HOVER_BIT.HOV : aBtn.HOVER_BIT.NONE) | (((this.hovered & aBtn.HOVER_BIT.HOV_INNER_MOFF) != aBtn.HOVER_BIT.NONE) ? aBtn.HOVER_BIT.HOV_INNER : aBtn.HOVER_BIT.NONE);
					this.Fine(false);
				}
				if (this.isSelected() || aBtn.PreSelected == this)
				{
					if (0 < (this.navi_setted & 15) && (this.navi_setted & 15) < 15 && this.navi_auto_fill)
					{
						this.fillEmptyInNavigation();
					}
					if (!IN.isMouseOn())
					{
						this.simulateNaviTranslation(-1);
					}
				}
				this.runPushingBlink(false);
				if (IN.isSubmit() && (this.isSelected() || (this.hover_to_select && aBtn.PreSelected == this)) && this.pushed == 0 && this.z_push_click)
				{
					if (!this.isLocked() || this.locked_click)
					{
						this.ExecuteOnSubmitKey();
					}
					else
					{
						if (!X.DEBUGNOSND)
						{
							SND.Ui.play("locked", false);
						}
						CURS.limitVib(this, AIM.R);
					}
				}
				if (this.isMouseOver() && !this.isPushed() && this.HovCurs != null)
				{
					this.chkeckHoverCursState(true);
				}
			}
			if (this.af >= 0 && this.Skin != null && this.Skin.alpha > 0f)
			{
				this.af++;
			}
			else if (this.af < 0)
			{
				this.af--;
			}
			if (this.Skin != null && this.Skin.fine_flag && X.D)
			{
				bool flag = true;
				if (this.BelongScroll != null)
				{
					flag = this.BelongScroll.isShowing(this, this.w + 50f, this.h + 50f, 0f, 0f);
				}
				if (flag)
				{
					Bench.P("SkinFine");
					this.Skin.Fine();
					Bench.Pend("SkinFine");
				}
			}
			if (this.active >= -1 && base.gameObject.activeSelf && base.gameObject.activeInHierarchy)
			{
				return true;
			}
			this.runner_assigned_ = false;
			return false;
		}

		public void runPushingBlink(bool immediate = false)
		{
			if (this.pushed < 0)
			{
				if (!immediate)
				{
					int num = this.pushed + 1;
					this.pushed = num;
					if (num != 0)
					{
						return;
					}
				}
				this.pushed = 0;
				this.Fine(false);
			}
		}

		public HoverCursManager getHoverManager()
		{
			return this.HovCurs;
		}

		public aBtn setHoverManager(HoverCursManager Hvc = null)
		{
			if (this.HovCurs != null)
			{
				this.HovCurs.blur();
			}
			this.HovCurs = Hvc;
			if (this.isMouseOver() && !this.isPushed() && Hvc != null)
			{
				this.chkeckHoverCursState(true);
			}
			return this;
		}

		public string chkeckHoverCursState(bool check_curs = false)
		{
			if (this.HovCurs == null)
			{
				return "";
			}
			return this.HovCurs.checkStateCursManager(check_curs);
		}

		public void Fine(bool immediate = false)
		{
			if (this.Skin != null && !this.destructed)
			{
				this.Skin.fine_flag = true;
				if (immediate)
				{
					this.Skin.Fine();
				}
			}
		}

		public virtual void OnPointerEnter()
		{
			if (this.active <= 0)
			{
				return;
			}
			bool flag = (this.hovered & (aBtn.HOVER_BIT)(-769)) != aBtn.HOVER_BIT.NONE;
			this.hovered |= (IN.use_mouse ? aBtn.HOVER_BIT.HOV : aBtn.HOVER_BIT.NONE) | aBtn.HOVER_BIT.HOV_MOFF;
			if (flag || (this.hovered & (aBtn.HOVER_BIT)(-769)) <= aBtn.HOVER_BIT.NONE)
			{
				return;
			}
			if (this.hover_snd != "")
			{
				SND.Ui.play(this.hover_snd, false);
			}
			if (this.runFn(this.AFnHover))
			{
				this.FineHover();
				return;
			}
			this.hovered &= (aBtn.HOVER_BIT)(-258);
		}

		public void FineHover()
		{
			this.Fine(false);
			if (this.hover_to_select && IN.use_mouse)
			{
				this.Select(false);
			}
		}

		public void Select(bool move_scrollbox = true)
		{
			if (this.destructed)
			{
				return;
			}
			if (this.autoBind || this.pushed <= -1000)
			{
				this.StartBtn();
			}
			if (move_scrollbox)
			{
				ScrollBox belongScroll = this.BelongScroll;
				if (belongScroll != null && belongScroll.show_scroll_bar)
				{
					belongScroll.revealN(this, true);
				}
			}
			this.OnSelect();
		}

		protected virtual void OnSelect()
		{
			if (this.active <= 0)
			{
				aBtn.OfflineSelected = this;
				return;
			}
			aBtn.OfflineSelected = null;
			if ((this.hovered & aBtn.HOVER_BIT.SEL) != aBtn.HOVER_BIT.NONE)
			{
				return;
			}
			bool flag = (this.hovered & (aBtn.HOVER_BIT)5) > aBtn.HOVER_BIT.NONE;
			this.hovered |= aBtn.HOVER_BIT.SEL;
			if (aBtn.PreSelected != null)
			{
				aBtn.PreSelected.Deselect(true);
			}
			aBtn.PreSelected = this;
			CURS.focusOnBtn(this, this.isHovered());
			if (flag)
			{
				return;
			}
			if (TX.valid(this.hover_snd))
			{
				SND.Ui.play(this.hover_snd, false);
			}
			if (this.runFn(this.AFnHover))
			{
				this.Fine(false);
			}
			else
			{
				this.hovered &= (aBtn.HOVER_BIT)(-3);
			}
			if (0 <= (this.navi_setted & 15) && (this.navi_setted & 15) < 15 && this.navi_auto_fill)
			{
				this.fillEmptyInNavigation();
			}
		}

		public void OnPointerExit()
		{
			int num = (int)(this.hovered & (aBtn.HOVER_BIT)(-769));
			this.hovered &= (aBtn.HOVER_BIT)(-770);
			if (num > 0)
			{
				if (this.hovered == aBtn.HOVER_BIT.NONE)
				{
					this.runFn(this.AFnOut);
				}
				this.Fine(false);
				if (this.HovCurs != null)
				{
					this.HovCurs.blur();
				}
			}
		}

		public virtual void OnDeselect()
		{
			bool flag = this.hovered > aBtn.HOVER_BIT.NONE;
			this.hovered &= (aBtn.HOVER_BIT)(-3);
			if (flag && this.hovered == aBtn.HOVER_BIT.NONE)
			{
				this.runFn(this.AFnOut);
				this.Fine(false);
			}
		}

		public virtual bool OnPointerDown()
		{
			if (LabeledInputField.CurFocused != null && LabeledInputField.CurFocused != this)
			{
				LabeledInputField.CurFocused.Blur();
			}
			if (this.active <= 0 || this.pushed <= -1000)
			{
				return false;
			}
			aBtn preSelected = aBtn.PreSelected;
			if (this.click_to_select)
			{
				this.Select(true);
			}
			else if (preSelected == this)
			{
				this.Deselect(true);
			}
			int num = this.pushed;
			this.pushed = 1;
			if (!this.runFn(this.AFnDown))
			{
				this.pushed = num;
				return false;
			}
			this.Fine(false);
			return true;
		}

		public virtual void OnPointerUp(bool clicking)
		{
			if (this.active <= 0 || this.pushed <= -1000)
			{
				return;
			}
			this.pushed = 0;
			if (clicking)
			{
				this.ExecuteOnClick();
			}
			this.runFn(this.AFnUp);
			this.Fine(false);
		}

		public bool getClickable(Vector2 PosU, out IClickable Res)
		{
			Res = null;
			if (this.Skin == null || !this.clickable)
			{
				return false;
			}
			if (this.Skin.canClickable(PosU))
			{
				Res = this;
				return true;
			}
			return false;
		}

		public virtual void ExecuteOnSubmitKey()
		{
			if (this.active <= 0)
			{
				return;
			}
			this.StartCheck();
			this.pushedAnimSimulate();
			this.ExecuteOnClick();
		}

		public void pushedAnimSimulate()
		{
			if (this.pushed <= 0 && this.pushed > -1000)
			{
				if (this.pushed == 0)
				{
					this.Fine(false);
				}
				this.pushed = -6;
			}
		}

		public aBtn Deselect(bool remove_from_mem = true)
		{
			try
			{
				if (remove_from_mem && aBtn.PreSelected == this)
				{
					aBtn.PreSelected = null;
				}
				if (this.isSelected())
				{
					this.OnDeselect();
				}
			}
			catch
			{
			}
			return this;
		}

		public void ExecuteOnClick()
		{
			this.StartCheck();
			if (this.active <= 0)
			{
				return;
			}
			if (this.click_snd != "" && this.isLocked() && !this.locked_click)
			{
				SND.Ui.play("talk_progress", false);
				return;
			}
			if (this.runFn(this.AFnClick))
			{
				SND.Ui.play(this.click_snd, false);
			}
		}

		public aBtn addClickFn(FnBtnBindings Fn)
		{
			return this.addFn(ref this.AFnClick, Fn);
		}

		public aBtn addDownFn(FnBtnBindings Fn)
		{
			return this.addFn(ref this.AFnDown, Fn);
		}

		public aBtn addUpFn(FnBtnBindings Fn)
		{
			return this.addFn(ref this.AFnUp, Fn);
		}

		public aBtn addHoverFn(FnBtnBindings Fn)
		{
			return this.addFn(ref this.AFnHover, Fn);
		}

		public aBtn addOutFn(FnBtnBindings Fn)
		{
			return this.addFn(ref this.AFnOut, Fn);
		}

		protected aBtn addFn(ref FnBtnBindings[] AFn, FnBtnBindings Fn)
		{
			aBtn.addFnT<FnBtnBindings>(ref AFn, Fn);
			return this;
		}

		public static void addFnT<T>(ref T[] AFn, T[] Fn) where T : class
		{
			if (Fn == null)
			{
				return;
			}
			int num = Fn.Length;
			for (int i = 0; i < num; i++)
			{
				aBtn.addFnT<T>(ref AFn, Fn[i]);
			}
		}

		public static void addFnT<T>(ref T[] AFn, T Fn) where T : class
		{
			if (Fn == null)
			{
				return;
			}
			if (AFn == null)
			{
				AFn = new T[8];
			}
			int i = 0;
			int num = AFn.Length;
			while (i < num)
			{
				T t = AFn[i];
				if (t == null)
				{
					AFn[i] = Fn;
					return;
				}
				if (t == Fn)
				{
					return;
				}
				i++;
			}
			Array.Resize<T>(ref AFn, num + 8);
			AFn[i] = Fn;
		}

		protected bool runFn(FnBtnBindings[] AFn)
		{
			if (this.destructed || AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				FnBtnBindings fnBtnBindings = AFn[i];
				if (fnBtnBindings == null)
				{
					return flag;
				}
				flag = fnBtnBindings(this) && flag;
			}
			return flag;
		}

		public void destruct()
		{
		}

		protected virtual void OnDestroy()
		{
			if (this.Skin != null)
			{
				this.Skin.destruct();
			}
			this.runner_assigned = false;
			this.removeFromBelongScroll();
			this.Skin = null;
			try
			{
				this.hide();
			}
			catch
			{
			}
			this.active = -2;
		}

		public aBtn setNaviL(aBtn St, bool reversible = true, bool important = false)
		{
			if (St == null || this.destructed)
			{
				return this;
			}
			if (!important && (this.navi_setted & 16) > 0)
			{
				return this;
			}
			this.selectOnLeft = St;
			this.navi_setted |= 1;
			if (important)
			{
				this.navi_setted |= 16;
			}
			if (reversible && St != null)
			{
				St.setNaviR(this, false, important);
			}
			return this;
		}

		public aBtn setNaviR(aBtn St, bool reversible = true, bool important = false)
		{
			if (St == null || this.destructed)
			{
				return this;
			}
			if (!important && (this.navi_setted & 64) > 0)
			{
				return this;
			}
			this.selectOnRight = St;
			this.navi_setted |= 4;
			if (important)
			{
				this.navi_setted |= 64;
			}
			if (reversible && St != null)
			{
				St.setNaviL(this, false, important);
			}
			return this;
		}

		public aBtn setNaviT(aBtn St, bool reversible = true, bool important = false)
		{
			if (St == null || this.destructed)
			{
				return this;
			}
			if (!important && (this.navi_setted & 32) > 0)
			{
				return this;
			}
			this.selectOnUp = St;
			this.navi_setted |= 2;
			if (important)
			{
				this.navi_setted |= 32;
			}
			if (reversible && St != null)
			{
				St.setNaviB(this, false, important);
			}
			return this;
		}

		public aBtn setNaviB(aBtn St, bool reversible = true, bool important = false)
		{
			if (St == null || this.destructed)
			{
				return this;
			}
			if (!important && (this.navi_setted & 128) > 0)
			{
				return this;
			}
			this.selectOnDown = St;
			this.navi_setted |= 8;
			if (important)
			{
				this.navi_setted |= 128;
			}
			if (reversible && St != null)
			{
				St.setNaviT(this, false, important);
			}
			return this;
		}

		public aBtn secureNavi()
		{
			for (int i = 0; i < 4; i++)
			{
				if (this.getNaviAim(i) == null)
				{
					this.setNaviAim(i, this, false, true);
				}
			}
			return this;
		}

		public aBtn clearNavi(uint aim_bits = 15U, bool only_not_important = false)
		{
			if ((aim_bits & 8U) != 0U && (!only_not_important || (this.navi_setted & 128) == 0))
			{
				this.selectOnDown = null;
				this.navi_setted = (byte)((int)this.navi_setted & -137);
			}
			if ((aim_bits & 2U) != 0U && (!only_not_important || (this.navi_setted & 32) == 0))
			{
				this.selectOnUp = null;
				this.navi_setted = (byte)((int)this.navi_setted & -35);
			}
			if ((aim_bits & 1U) != 0U && (!only_not_important || (this.navi_setted & 16) == 0))
			{
				this.selectOnLeft = null;
				this.navi_setted = (byte)((int)this.navi_setted & -18);
			}
			if ((aim_bits & 4U) != 0U && (!only_not_important || (this.navi_setted & 64) == 0))
			{
				this.selectOnRight = null;
				this.navi_setted = (byte)((int)this.navi_setted & -69);
			}
			return this;
		}

		protected virtual void simulateNaviTranslation(int aim = -1)
		{
			aBtn aBtn = null;
			int num = aim;
			if (num == -1)
			{
				if (IN.isL())
				{
					aBtn = this.selectOnLeft;
					num = 0;
				}
				if (IN.isR())
				{
					aBtn = this.selectOnRight;
					num = 2;
				}
				if (IN.isT())
				{
					aBtn = this.selectOnUp;
					num = 1;
				}
				if (IN.isB())
				{
					aBtn = this.selectOnDown;
					num = 3;
				}
			}
			if (num != -1)
			{
				IN.use_mouse = false;
			}
			aBtn = this.simulateNaviTranslationInner(ref num, aBtn);
			if (aBtn != null)
			{
				while (aBtn != null && !aBtn.isActive() && aBtn != this)
				{
					aBtn aBtn2;
					switch (num)
					{
					case 0:
						aBtn2 = aBtn.selectOnLeft;
						break;
					case 1:
						aBtn2 = aBtn.selectOnUp;
						break;
					case 2:
						aBtn2 = aBtn.selectOnRight;
						break;
					default:
						aBtn2 = aBtn.selectOnDown;
						break;
					}
					aBtn = aBtn2;
				}
				if (aBtn != this && aBtn != null)
				{
					IN.clearCursDown();
					if (aBtn.hover_snd == null)
					{
						SND.Ui.play(aBtn.snd_cursor_navi_translate, false);
					}
					aBtn.Select(true);
					this.setNaviAim(num, aBtn, true, false);
					if (aBtn.navigated_to_click)
					{
						aBtn.ExecuteOnClick();
					}
					return;
				}
			}
			if (num != -1 && !this.do_not_tip_on_navi_loop)
			{
				SND.Ui.play(aBtn.snd_cursor_navi_translate_limit, false);
				CURS.limitVib(this, (AIM)num);
			}
		}

		protected virtual aBtn simulateNaviTranslationInner(ref int a, aBtn Dep)
		{
			return Dep;
		}

		public aBtn setNaviAim(int i, aBtn St, bool reversible = true, bool important = false)
		{
			if (i == 0)
			{
				return this.setNaviL(St, reversible, important);
			}
			if (i == 1)
			{
				return this.setNaviT(St, reversible, important);
			}
			if (i != 2)
			{
				return this.setNaviB(St, reversible, important);
			}
			return this.setNaviR(St, reversible, important);
		}

		public aBtn getNaviAim(int i)
		{
			if (i == 0)
			{
				return this.selectOnLeft;
			}
			if (i == 1)
			{
				return this.selectOnUp;
			}
			if (i != 2)
			{
				return this.selectOnDown;
			}
			return this.selectOnRight;
		}

		public bool isNaviImportantConnected(AIM i)
		{
			return ((int)this.navi_setted & (1 << (int)i << 4)) > 0;
		}

		public bool isNaviConnected(AIM i)
		{
			return ((int)this.navi_setted & (1 << (int)i)) > 0;
		}

		protected void fillEmptyInNavigation()
		{
			Bench.P("fillEmptyInNavigation");
			int num = 1 << base.gameObject.layer;
			float num2 = IN.wh * 0.015625f;
			float num3 = IN.hh * 0.015625f;
			GameObject gameObject = base.gameObject;
			RaycastHit2D[] array = new RaycastHit2D[16];
			RectTransform component = base.GetComponent<RectTransform>();
			Vector2 vector = default(Vector2);
			if (component != null)
			{
				vector.x = component.rect.width / 2f;
				vector.y = component.rect.height / 2f;
			}
			Vector2 vector2 = base.transform.position;
			for (int i = 0; i < 4; i++)
			{
				if (((int)this.navi_setted & (1 << i)) <= 0)
				{
					Vector2 vector3 = new Vector2((float)CAim._XD(i, 1), (float)CAim._YD(i, 1));
					Vector2 vector4 = vector3;
					vector4 *= 0.3f;
					Vector2 vector5 = vector2;
					if (component != null)
					{
						vector5 += vector * vector3 * 0.44f;
					}
					else
					{
						vector5 += vector4;
					}
					Vector2 vector6 = vector * 2f;
					if (vector3.x != 0f)
					{
						vector6.x = 0.15f;
						vector6.y *= 2.5f;
					}
					else
					{
						vector6.x *= 2.5f;
						vector6.y = 0.15f;
					}
					int num4 = X.IntC(((i == 0) ? (vector5.x + num2) : ((i == 1) ? (IN.h * 0.015625f - vector5.y - num3) : ((i == 2) ? (IN.w * 0.015625f - vector5.x - num2) : (vector5.y + num3)))) / 0.3f) + 1;
					float num5 = -1f;
					aBtn aBtn = null;
					for (int j = 0; j < num4; j++)
					{
						int num6 = Physics2D.BoxCastNonAlloc(vector5, vector6, 0f, vector3, array, 0.90000004f, num);
						for (int k = 0; k < num6; k++)
						{
							RaycastHit2D raycastHit2D = array[k];
							GameObject gameObject2 = raycastHit2D.collider.gameObject;
							if (!(gameObject2 == gameObject) && raycastHit2D.collider.enabled)
							{
								aBtn component2 = gameObject2.GetComponent<aBtn>();
								if (component2 != null && !component2.isNaviImportantConnected(CAim.get_opposite((AIM)i)))
								{
									AIM aim = CAim.get_opposite((AIM)i);
									component2.isNaviImportantConnected(aim);
									float num7 = X.LENGTHXY2(vector2, component2.transform.position);
									if (num5 < 0f || num5 > num7)
									{
										num5 = num7;
										aBtn = component2;
									}
								}
							}
						}
						vector6.x += (float)((vector3.x != 0f) ? 1 : 2) * 0.2f;
						vector6.y += (float)((vector3.y != 0f) ? 1 : 2) * 0.2f;
						vector5 += vector4;
					}
					if (aBtn != null)
					{
						this.setNaviAim(i, aBtn, false, false);
						this.navi_setted |= (byte)(1 << i);
					}
				}
			}
			for (int l = 0; l < 4; l++)
			{
				if (((int)this.navi_setted & (1 << l)) <= 0)
				{
					int num8 = (int)CAim.get_clockwise2((AIM)l, false);
					int num9 = (int)CAim.get_clockwise2((AIM)l, true);
					aBtn aBtn2 = null;
					aBtn naviAim = this.getNaviAim(num8);
					aBtn naviAim2 = this.getNaviAim(num9);
					int num10 = ((l == 1 || l == 3) ? CAim._YD(l, 1) : CAim._XD(l, 1));
					if (naviAim != null && naviAim2 != null)
					{
						Vector2 vector7 = naviAim.transform.position;
						Vector2 vector8 = naviAim2.transform.position;
						AIM aim2 = CAim.get_aim2(vector2, vector7, false);
						AIM aim3 = CAim.get_aim2(vector2, vector8, false);
						int num11 = ((l == 1 || l == 3) ? CAim._YD(aim2, 1) : CAim._XD(aim2, 1));
						int num12 = ((l == 1 || l == 3) ? CAim._YD(aim3, 1) : CAim._XD(aim3, 1));
						if (num11 == num10 && num12 != num10)
						{
							aBtn2 = naviAim;
						}
						else if (num11 != num10 && num12 == num10)
						{
							aBtn2 = naviAim2;
						}
						else if (num11 == num10 && num12 == num10)
						{
							aBtn2 = ((X.LENGTHXY2(vector2, vector7) < X.LENGTHXY2(vector2, vector8)) ? naviAim : naviAim2);
						}
					}
					else if (naviAim != null)
					{
						Vector2 vector9 = naviAim.transform.position;
						AIM aim4 = CAim.get_aim2(vector2, vector9, false);
						if (l != 1 && l != 3)
						{
							CAim._XD(aim4, 1);
						}
						else
						{
							CAim._YD(aim4, 1);
						}
						if ((l == 1 || l == 3) ? (CAim._YD(aim4, 1) == CAim._YD(l, 1)) : (CAim._XD(aim4, 1) == CAim._XD(l, 1)))
						{
							aBtn2 = naviAim;
						}
					}
					else if (naviAim2 != null)
					{
						Vector2 vector10 = naviAim2.transform.position;
						AIM aim5 = CAim.get_aim2(vector2, vector10, false);
						int num13 = ((l == 1 || l == 3) ? CAim._YD(aim5, 1) : CAim._XD(aim5, 1));
						if (num10 == num13)
						{
							aBtn2 = naviAim2;
						}
					}
					if (!(aBtn2 == null))
					{
						this.setNaviAim(l, aBtn2, false, false);
					}
				}
			}
			this.navi_setted |= 15;
			Bench.Pend("fillEmptyInNavigation");
		}

		public aBtn setSkinTitle(string _title)
		{
			if (this.Skin == null)
			{
				string text = this.title;
				this.initializeSkin(this.skin, _title);
				this.title = text;
			}
			else
			{
				this.Skin.setTitle(_title);
			}
			return this;
		}

		public aBtn clearUnselectable(bool fill_navi = false)
		{
			this.navi_auto_fill = true;
			this.hover_to_select = true;
			this.click_to_select = true;
			return this;
		}

		public aBtn unselectable(bool fill_navi = false)
		{
			this.z_push_click = false;
			this.navi_auto_fill = false;
			this.hover_to_select = false;
			this.click_to_select = false;
			if (fill_navi)
			{
				this.secureNavi();
			}
			return this;
		}

		public aBtn setLayer(int lay)
		{
			base.gameObject.layer = lay;
			if (this.Skin != null)
			{
				this.Skin.setLayer(lay);
			}
			return this;
		}

		public ButtonSkin get_Skin()
		{
			return this.Skin;
		}

		public float get_skin_alpha()
		{
			if (this.Skin == null)
			{
				return 0f;
			}
			return this.Skin.alpha;
		}

		public virtual int container_stencil_ref
		{
			get
			{
				if (this.Container != null && this.Container.stencil_ref >= 0)
				{
					return this.Container.stencil_ref;
				}
				return -1;
			}
		}

		public bool isActive()
		{
			return this.active == 1;
		}

		public bool isHovered()
		{
			return (this.hovered & (aBtn.HOVER_BIT)5) > aBtn.HOVER_BIT.NONE;
		}

		public bool isMouseOver()
		{
			return (this.hovered & (aBtn.HOVER_BIT)773) > aBtn.HOVER_BIT.NONE;
		}

		public bool isSelected()
		{
			return (this.hovered & aBtn.HOVER_BIT.SEL) > aBtn.HOVER_BIT.NONE;
		}

		public bool isFocused()
		{
			return this.isSelected() || this.isHovered();
		}

		public bool isHoveredOrPushOut()
		{
			return (this.isHovered() || this.isSelected()) && !this.isPushed();
		}

		public virtual bool isPushed()
		{
			return this.pushed != 0 && this.pushed > -1000;
		}

		public virtual bool isPushDown()
		{
			return (this.pushed > 0 && this.isHovered()) || (this.pushed < 0 && this.pushed > -1000);
		}

		public bool isChecked()
		{
			return this.checked_ == 1f;
		}

		public bool isLocked()
		{
			return this.locked_ != 0;
		}

		public float getFarLength()
		{
			return base.transform.position.z;
		}

		public string focus_curs_category
		{
			get
			{
				return "FOCUS";
			}
		}

		public string hover_curs_category
		{
			get
			{
				if (this.HovCurs != null)
				{
					return this.HovCurs.curs_category;
				}
				return "HOVER";
			}
		}

		public virtual aBtn SetChecked(bool f, bool fine_flag = true)
		{
			if (this.checked_ != 0f == f)
			{
				return this;
			}
			this.checked_ = (float)(f ? 1 : 0);
			if (fine_flag)
			{
				this.Fine(true);
			}
			return this;
		}

		public aBtn SetValue(float f, bool fine_flag = true)
		{
			this.checked_ = f;
			if (fine_flag)
			{
				this.Fine(true);
			}
			return this;
		}

		public virtual float val
		{
			get
			{
				return this.checked_;
			}
			set
			{
				this.SetValue(value, true);
			}
		}

		public virtual aBtn SetLocked(bool f, bool fine_flag = true, bool no_change_binding = false)
		{
			if (this.locked_click)
			{
				no_change_binding = true;
			}
			this.locked_ = (f ? (no_change_binding ? 1 : 2) : 0);
			if (fine_flag)
			{
				this.Fine(true);
			}
			if (this.Skin != null && this.Skin.isEnable() && !no_change_binding)
			{
				if (this.locked_ == 1 && this.isActive())
				{
					this.hide();
				}
				if (this.locked_ == 0 && !this.isActive())
				{
					this.bind();
				}
			}
			return this;
		}

		public virtual bool btn_enabled
		{
			get
			{
				return this.active > 0;
			}
			set
			{
				if (this.active == -2)
				{
					return;
				}
				if (value)
				{
					this.active = 1;
					this.af = X.Mx(this.af, 0);
					this.runner_assigned = true;
					return;
				}
				this.active = ((this.active == -1) ? (-1) : 0);
				this.af = X.Mn(this.af, -1);
			}
		}

		public bool fine_flag
		{
			get
			{
				return this.Skin != null && this.Skin.fine_flag;
			}
			set
			{
				if (this.Skin != null)
				{
					this.Skin.fine_flag = this.Skin.fine_flag || value;
				}
			}
		}

		public virtual float get_swidth_px()
		{
			if (this.Skin == null)
			{
				return this.w;
			}
			return this.Skin.swidth;
		}

		public virtual float get_sheight_px()
		{
			if (this.Skin == null)
			{
				return this.h;
			}
			return this.Skin.sheight;
		}

		public bool use_valotile
		{
			get
			{
				return this.Skin != null && this.Skin.use_valotile;
			}
			set
			{
				if (this.Skin == null)
				{
					return;
				}
				this.Skin.use_valotile = value;
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
				if (this.Container != null)
				{
					return this.Container.BelongScroll;
				}
				return null;
			}
			set
			{
				if (value != this.BelongScroll_)
				{
					this.removeFromBelongScroll();
					this.BelongScroll_ = value;
				}
			}
		}

		public void AddSelectableItems(List<aBtn> ASlc, bool only_front)
		{
			ASlc.Add(this);
		}

		public Transform getTransform()
		{
			return base.transform;
		}

		public void Pause()
		{
			this.hide();
		}

		public void Resume()
		{
			this.bind();
		}

		public bool destructed
		{
			get
			{
				return this.active == -2;
			}
		}

		public void setAlpha(float value)
		{
			if (this.Skin != null)
			{
				this.Skin.alpha = value;
				if (value <= 0.01f)
				{
					this.Skin.Fine();
				}
			}
		}

		public bool isNaviSetted(AIM a)
		{
			return ((int)this.navi_setted & (1 << (int)a)) != 0;
		}

		public virtual string getValueString()
		{
			if (!this.isChecked())
			{
				return "0";
			}
			return "1";
		}

		public virtual void setValue(string s)
		{
			this.SetChecked(X.NmI(s, 0, true, false) != 0, true);
		}

		public string btn_key
		{
			get
			{
				if (this.btn_key_ == null)
				{
					this.btn_key_ = "BTN: " + base.gameObject.name;
				}
				return this.btn_key_;
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

		public override string ToString()
		{
			return this.btn_key;
		}

		public int get_af()
		{
			return this.af;
		}

		public static bool isSelecting(string title)
		{
			return aBtn.PreSelected != null && aBtn.PreSelected.title == title;
		}

		public GameObject getGob()
		{
			return base.gameObject;
		}

		public bool clickable
		{
			get
			{
				return !this.destructed && (this.isActive() && this.Skin != null && this.Skin.alpha > 0f && base.enabled) && base.gameObject.activeSelf;
			}
		}

		public static STB convert_eng_title(STB Stb, string s)
		{
			if (s != null)
			{
				if (s == "ok")
				{
					return Stb.Set(s).ToUpper();
				}
				if (s == "cancel")
				{
					return Stb.Set("Cancel");
				}
				if (s == "omake")
				{
					return Stb.Set("Special");
				}
			}
			Stb.Set(s).ToUnityCase();
			return Stb;
		}

		public static string snd_cursor_navi_translate = "cursor";

		public static string snd_cursor_navi_translate_limit = "toggle_button_limit";

		public string skin = "normal";

		public string title = "";

		public string hover_snd;

		public string click_snd = "";

		public float w;

		public float h;

		public bool autoBind = true;

		public bool autoSelect;

		public bool hover_to_select = true;

		public bool click_to_select = true;

		public bool navi_auto_fill = true;

		public bool z_push_click = true;

		public bool navigated_to_click;

		public bool locked_click;

		public bool do_not_tip_on_navi_loop;

		public BtnContainerBasic Container;

		private ScrollBox BelongScroll_;

		public int carr_index;

		private aBtn selectOnLeft;

		private aBtn selectOnRight;

		private aBtn selectOnDown;

		private aBtn selectOnUp;

		protected ButtonSkin Skin;

		protected HoverCursManager HovCurs;

		private int active = -1;

		protected int pushed = -1000;

		private aBtn.HOVER_BIT hovered;

		private float checked_;

		private int locked_;

		protected int af = -1;

		public byte navi_setted;

		private bool runner_assigned_;

		public const int submitkey_push_hold = 6;

		private byte fine_collider_flag_ = 1;

		public static aBtn PreSelected;

		public static aBtn OfflineSelected;

		private FnBtnBindings[] AFnHover;

		private FnBtnBindings[] AFnOut;

		private FnBtnBindings[] AFnDown;

		private FnBtnBindings[] AFnUp;

		private FnBtnBindings[] AFnClick;

		private string btn_key_;

		private enum HOVER_BIT
		{
			NONE,
			HOV,
			SEL,
			HOV_INNER = 4,
			HOV_MOFF = 256,
			HOV_INNER_MOFF = 512,
			HOV_NOSEL = 768,
			_ALL = 1799
		}
	}
}
