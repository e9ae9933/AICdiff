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
				tab_h = 28f;
			}
			TabLRSlider tabLRSlider = new TabLRSlider(Ds, use_w, tab_h, use_scroll)
			{
				click_snd = ""
			};
			tabLRSlider.setL();
			float num = tabLRSlider.between_bounds_w - 8f;
			int num2 = ((Akeys != null) ? Akeys.Length : 0);
			float num3 = tab_h;
			Designer designer = null;
			BtnContainer<aBtn>.FnBtnMakingBindings fnBtnMakingBindings = null;
			if (use_scroll)
			{
				designer = Ds.addTab(_name, num, tab_h, num, tab_h, true);
				designer.margin_in_lr = (designer.margin_in_tb = 0f);
				designer.scrolling_margin_in_lr = 6f;
				designer.scrolling_margin_in_tb = 2f;
				num -= designer.scrolling_margin_in_lr * 2f;
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
				clms = X.Mx(1, num2),
				margin_w = 0,
				margin_h = 0,
				w = num / (float)X.Mx(1, num2),
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
			columnRow.LRS = tabLRSlider;
			tabLRSlider.FD_slided = new Func<int, bool>(columnRow.fnSlided);
			for (int i = columnRow.Length - 1; i >= 0; i--)
			{
				columnRow.Get(i).click_snd = "tool_hand_init";
			}
			columnRow.carr_hiding_reverse = false;
			if (use_scroll)
			{
				Ds.endTab(true);
			}
			tabLRSlider.setR();
			columnRow.ParentTab = Ds;
			columnRow.ParentScrollTab = (use_scroll ? designer : Ds);
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

		public ColumnRow initOneBtnClmn(ColumnRow.FnOneBtnClmn Fn, string[] Atitle_keys)
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
					aBtn.addClickFn((aBtn _B) => this.LRS.runLRInput(1, true) || true);
					aBtn.unselectable(true);
					aBtn.gameObject.SetActive(aBtn.isChecked());
					aBtn.SetChecked(false, true);
					if (Atitle_keys != null)
					{
						aBtn.setSkinTitle(Atitle_keys[i]);
					}
				}
				objCarrierConBlockBtnContainer.resetCalced();
				objCarrierConBlockBtnContainer.CalcAllBtn(9999f, this.ABtn, length, false);
			}
			return this;
		}

		public void setRowMemoryVisible(bool visibility)
		{
			this.LRS.setVisible(visibility);
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
				return this.LRS.use_valotile;
			}
			set
			{
				this.LRS.use_valotile = value;
			}
		}

		public override bool runCarrier(float t, ObjCarrierCon _Carr = null)
		{
			bool flag = true;
			if (!base.carr_hiding_running)
			{
				flag = base.runCarrier(t, _Carr);
				if (this.LRS != null && this.LRS.lr_input > 0)
				{
					this.LRS.runLRInput(-2, false);
				}
			}
			return flag;
		}

		public ColumnRow LrInput(bool flag)
		{
			this.lr_input = flag;
			return this;
		}

		public bool runLRInput(int move = -2, bool manual_mode = false)
		{
			return this.LRS.runLRInput(move, manual_mode);
		}

		public bool lr_input
		{
			get
			{
				return this.LRS.lr_input > 0;
			}
			set
			{
				if (this.lr_input == value)
				{
					return;
				}
				this.LRS.lr_input = (value ? 3 : 0);
			}
		}

		private bool fnSlided(int move)
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
			IN.clearPushDown(false);
			return flag;
		}

		public const float tab_h = 28f;

		public const float tab_lr_input_width = 30f;

		public Designer ParentTab;

		public Designer ParentScrollTab;

		public const int SCROLL_MARGIN_LR = 6;

		private TabLRSlider LRS;

		private ColumnRow.FnOneBtnClmn fnOneBtnClmn;

		public delegate bool FnOneBtnClmn(int index);
	}
}
