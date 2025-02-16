using System;
using UnityEngine;

namespace XX
{
	public class ColumnRow : BtnContainerRadio<aBtn>
	{
		public ColumnRow(GameObject _Base, int default_val = 0, int alloc_size = 0, ObjCarrierCon _Carr = null, ScrollAppend SCA = null)
			: base(_Base, default_val, alloc_size, _Carr, SCA)
		{
		}

		public static ColumnRow CreateT<T>(Designer Ds, string _name, string _skin, int _def, string[] Akeys, BtnContainerRadio<aBtn>.FnRadioBindings FnChanged, float use_w = 0f, float tab_h = 0f, bool selectable = false, bool use_scroll = false) where T : aBtn
		{
			return ColumnRow.CreateT<T>((GameObject _Base, int default_val, int alloc_size, ObjCarrierCon _Carr, ScrollAppend SCA) => new ColumnRow(_Base, default_val, alloc_size, _Carr, SCA), Ds, _name, _skin, _def, Akeys, FnChanged, use_w, tab_h, selectable, use_scroll);
		}

		protected static ColumnRow CreateT<T>(BtnContainerRadio<aBtn>.FnCreateContainer FnContainerCreate, Designer Ds, string _name, string _skin, int _def, string[] Akeys, BtnContainerRadio<aBtn>.FnRadioBindings FnChanged, float use_w = 0f, float tab_h = 0f, bool selectable = false, bool use_scroll = false) where T : aBtn
		{
			if (use_w <= 0f)
			{
				use_w = Ds.use_w + use_w;
			}
			if (tab_h <= 0f)
			{
				tab_h = ColumnRow.tab_h;
			}
			if (ColumnRow.DsnLr == null)
			{
				ColumnRow.DsnLr = new DsnDataP("", false)
				{
					html = true,
					size = 14f,
					swidth = 30f,
					aligny = ALIGNY.MIDDLE
				};
			}
			ColumnRow.DsnLr.sheight = tab_h - (use_scroll ? 6.6000004f : 0f);
			DsnDataP dsnDataP = ColumnRow.DsnLr.Text("<key ltab/>");
			int num = ((Akeys != null) ? Akeys.Length : 0);
			FillBlock fillBlock = Ds.addP(dsnDataP, false);
			float num2 = (float)((int)(use_w - 60f - 8f - Ds.item_margin_x_px * 2f));
			float num3 = tab_h;
			Designer designer = null;
			BtnContainer<aBtn>.FnBtnMakingBindings fnBtnMakingBindings = null;
			if (use_scroll)
			{
				designer = Ds.addTab(_name, num2, tab_h, num2, tab_h, true);
				designer.margin_in_lr = (designer.margin_in_tb = 0f);
				designer.scrolling_margin_in_lr = 6f;
				designer.scrolling_margin_in_tb = 2f;
				num2 -= designer.scrolling_margin_in_lr * 2f;
				num3 -= designer.scrolling_margin_in_tb * 2f + 10f + 10f;
				fnBtnMakingBindings = delegate(BtnContainer<aBtn> BCon, aBtn B)
				{
					B.addClickFn((aBtn _B) => _B.Container.BelongScroll.reveal(_B, true, REVEALTYPE.ALWAYS));
					return true;
				};
			}
			DsnDataRadio dsnDataRadio = new DsnDataRadio
			{
				name = _name,
				skin = _skin,
				def = _def,
				clms = X.Mx(1, num),
				margin_w = 0,
				margin_h = 0,
				w = num2 / (float)X.Mx(1, num),
				h = num3,
				keys = Akeys,
				navi_loop = 1,
				fnChanged = FnChanged,
				fnCreateContainer = FnContainerCreate,
				fnMaking = fnBtnMakingBindings
			};
			if (selectable)
			{
				dsnDataRadio.z_push_click = (dsnDataRadio.navigated_to_click = true);
			}
			else
			{
				dsnDataRadio.unselectable = 2;
			}
			ColumnRow columnRow = Ds.addRadioT<T>(dsnDataRadio) as ColumnRow;
			for (int i = columnRow.Length - 1; i >= 0; i--)
			{
				columnRow.Get(i).click_snd = "tool_hand_init";
			}
			columnRow.carr_hiding_reverse = false;
			if (use_scroll)
			{
				Ds.endTab(true);
			}
			FillBlock fillBlock2 = Ds.addP(dsnDataP.Text("<key rtab/>"), false);
			columnRow.ParentTab = Ds;
			columnRow.ParentScrollTab = (use_scroll ? designer : Ds);
			columnRow.PLeft = fillBlock;
			columnRow.PRight = fillBlock2;
			columnRow.lr_input = true;
			Ds.Br();
			return columnRow;
		}

		public override BtnContainer<aBtn> bind(bool apply_to_skin = false, bool clear_navigation = false)
		{
			base.bind(apply_to_skin, clear_navigation);
			this.carr_hiding_offline = true;
			return this;
		}

		public ColumnRow initOneBtnClmn(ColumnRow.FnOneBtnClmn Fn)
		{
			this.fnOneBtnClmn = Fn;
			ObjCarrierConBlockBtnContainer<aBtn> objCarrierConBlockBtnContainer = this.getMainCarr() as ObjCarrierConBlockBtnContainer<aBtn>;
			if (objCarrierConBlockBtnContainer != null && objCarrierConBlockBtnContainer.set_clmn > 1)
			{
				float num = (objCarrierConBlockBtnContainer.bounds_w + objCarrierConBlockBtnContainer.item_w) * 64f;
				float num2 = (objCarrierConBlockBtnContainer.bounds_h + objCarrierConBlockBtnContainer.item_h) * 64f;
				objCarrierConBlockBtnContainer.item_w = objCarrierConBlockBtnContainer.bounds_w;
				objCarrierConBlockBtnContainer.item_h = objCarrierConBlockBtnContainer.bounds_h;
				objCarrierConBlockBtnContainer.Intv(0f, 0f, 1);
				int length = this.Length;
				for (int i = 0; i < length; i++)
				{
					aBtn aBtn = base.Get(i);
					aBtn.WH(num, num2);
					aBtn.addClickFn((aBtn _B) => this.runLRInput(1) || true);
					aBtn.unselectable(true);
					aBtn.gameObject.SetActive(aBtn.isChecked());
					aBtn.SetChecked(false, true);
				}
				objCarrierConBlockBtnContainer.resetCalced();
				objCarrierConBlockBtnContainer.CalcAllBtn(9999f, this.ABtn, length, false);
			}
			return this;
		}

		public void setRowMemoryVisible(bool visibility)
		{
			this.ParentTab.getDesignerBlockMemory(this.PLeft).active = visibility;
			this.ParentTab.getDesignerBlockMemory(this.PRight).active = visibility;
			Designer parentTab = this.ParentTab;
			IDesignerBlock designerBlock;
			if (!(this.ParentTab == this.ParentScrollTab))
			{
				IDesignerBlock parentScrollTab = this.ParentScrollTab;
				designerBlock = parentScrollTab;
			}
			else
			{
				designerBlock = this;
			}
			parentTab.getDesignerBlockMemory(designerBlock).active = visibility;
		}

		public bool LR_valotile
		{
			get
			{
				return this.PLeft.use_valotile;
			}
			set
			{
				this.PLeft.use_valotile = value;
				this.PRight.use_valotile = value;
			}
		}

		public override bool runCarrier(float t, ObjCarrierCon _Carr = null)
		{
			bool flag = true;
			if (!base.carr_hiding_running)
			{
				flag = base.runCarrier(t, _Carr);
				if (this.lr_input_)
				{
					this.runLRInput(-2);
				}
			}
			if (X.D)
			{
				this.fineAnm(1);
			}
			return flag;
		}

		public ColumnRow LrInput(bool flag)
		{
			this.lr_input = flag;
			return this;
		}

		public bool lr_input
		{
			get
			{
				return this.lr_input_;
			}
			set
			{
				if (this.lr_input == value)
				{
					return;
				}
				this.lr_input_ = value;
				if (value)
				{
					if (this.t_anm > -1024 && this.t_anm != 0)
					{
						this.fineAnm(15);
					}
					this.t_anm = -1025;
				}
			}
		}

		public void fineAnm(int fcnt)
		{
			if (this.t_anm > -1024)
			{
				if (this.t_anm > 0)
				{
					this.t_anm = X.Mx(this.t_anm - fcnt, 0);
					this.PRight.transform.localScale = Vector2.one * X.NIL(1f, 2f, (float)this.t_anm, 15f);
					return;
				}
				if (this.t_anm < 0)
				{
					this.t_anm = X.Mn(this.t_anm + fcnt, 0);
					this.PLeft.transform.localScale = Vector2.one * X.NIL(1f, 2f, (float)(-(float)this.t_anm), 15f);
				}
			}
		}

		public bool runLRInput(int move = -2)
		{
			if (this.t_anm <= -1024)
			{
				this.t_anm++;
				if (this.t_anm > -1024)
				{
					this.t_anm = 0;
				}
			}
			else
			{
				if (move <= -2)
				{
					move = 0;
					if (IN.isRTabPD())
					{
						move++;
					}
					else if (IN.isLTabPD())
					{
						move--;
					}
				}
				if (move != 0)
				{
					int num = (this.Length + base.getValue() + move) % this.Length;
					bool flag = true;
					if (this.fnOneBtnClmn != null)
					{
						flag = this.fnOneBtnClmn(num);
						if (flag)
						{
							SND.Ui.play(base.Get(num).click_snd, false);
							this.selected = num;
							for (int i = 0; i < this.Length; i++)
							{
								aBtn aBtn = base.Get(i);
								if (i == this.selected)
								{
									aBtn.gameObject.SetActive(true);
									aBtn.SetChecked(false, true);
									this.selected_key = aBtn.title;
									if (aBtn.isHovered())
									{
										aBtn.OnPointerExit();
									}
								}
								else
								{
									aBtn.gameObject.SetActive(false);
								}
							}
						}
					}
					else
					{
						base.Get(num).ExecuteOnClick();
					}
					if (flag)
					{
						if (this.t_anm != 0 && this.t_anm > 0 != move > 0)
						{
							this.fineAnm(15);
						}
						this.t_anm = move * 15;
						this.fineAnm(0);
					}
					IN.clearPushDown(false);
					return true;
				}
			}
			return false;
		}

		public const float tab_lr_input_width = 30f;

		public static float tab_h = 28f;

		private bool lr_input_;

		public static DsnDataP DsnLr;

		public FillBlock PLeft;

		public FillBlock PRight;

		public Designer ParentTab;

		public Designer ParentScrollTab;

		public const int ANM_MAXT = 15;

		public const int SCROLL_MARGIN_LR = 6;

		public int t_anm;

		private ColumnRow.FnOneBtnClmn fnOneBtnClmn;

		public delegate bool FnOneBtnClmn(int index);
	}
}
