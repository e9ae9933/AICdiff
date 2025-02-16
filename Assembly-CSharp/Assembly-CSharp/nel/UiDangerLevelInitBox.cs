using System;
using System.Collections.Generic;
using evt;
using m2d;
using XX;

namespace nel
{
	public sealed class UiDangerLevelInitBox : UiEvtDesignerBox
	{
		protected override string reserve_key
		{
			get
			{
				return "DLVI";
			}
		}

		protected override float main_w
		{
			get
			{
				if (!this.danger_home_enable)
				{
					return IN.w * 0.4f;
				}
				return IN.w * 0.5f;
			}
		}

		protected override float main_h
		{
			get
			{
				if (!this.danger_home_enable)
				{
					return IN.h * 0.5f;
				}
				return IN.h * 0.62f;
			}
		}

		private bool danger_home_enable
		{
			get
			{
				return (this.useable_tab_bits & 3U) == 3U;
			}
		}

		protected override void Awake()
		{
			this.M2D = M2DBase.Instance as NelM2DBase;
			EV.getVariableContainer().define("_dep_override", "", true);
		}

		public override void destruct()
		{
			Designer.EvacuateMem.Destroy(this.AEvcTab);
			base.destruct();
		}

		public UiDangerLevelInitBox Init(WholeMapItem DepWM, List<string> _Amoveable_home_wm, M2LpMapTransferWarp _LpFrom)
		{
			int reached_night_level = (int)DepWM.reached_night_level;
			this.LpFrom = _LpFrom;
			this.Amoveable_home_wm = _Amoveable_home_wm;
			if (this.Amoveable_home_wm.Count - ((this.Amoveable_home_wm.IndexOf(this.M2D.WM.CurWM.Mp.key) >= 0) ? 1 : 0) <= 0)
			{
				this.Amoveable_home_wm = null;
			}
			else
			{
				this.Amoveable_home_wm.Insert(0, DepWM.Mp.key);
			}
			this.useable_tab_bits = ((reached_night_level >= 16) ? 2U : 0U) | ((this.Amoveable_home_wm != null) ? 1U : 0U);
			base.Awake();
			this.BxC.item_margin_x_px = 0f;
			this.submit_to_cancel = false;
			if ((this.useable_tab_bits & 1U) != 0U)
			{
				this.BxC.margin_in_tb = 48f;
				base.Init();
				this.BxC.addP(new DsnDataP("", false)
				{
					name = "warpTo",
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					TxCol = C32.d2c(4294966715U),
					Col = C32.d2c(4282004532U),
					radius = 52f,
					swidth = this.BxC.use_w * 0.7f,
					sheight = 30f,
					html = true,
					size = 18f,
					text = "<img mesh=\"IconNoel0\" /> " + TX.Get("deperture_dialog", "")
				}, false);
				this.BxC.Br();
			}
			else
			{
				base.Init();
			}
			int num = X.bit_count(this.useable_tab_bits);
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				bool flag = true;
				for (int i = 0; i < 2; i++)
				{
					if ((this.useable_tab_bits & (1U << i)) != 0U)
					{
						if (flag)
						{
							flag = false;
							this.current_tab = (UiDangerLevelInitBox.TAB)i;
						}
						if (i == 1)
						{
							blist.Add(DepWM.localized_name);
						}
						else
						{
							blist.Add(TX.Get("deperture_" + FEnum<UiDangerLevelInitBox.TAB>.ToStr((UiDangerLevelInitBox.TAB)i).ToLower(), ""));
						}
					}
				}
				if (this.danger_home_enable)
				{
					this.current_tab = UiDangerLevelInitBox.TAB.DANGER;
				}
				if (num > 1)
				{
					this.RTabBar = ColumnRowNel.NCreateT<aBtnNel>(this.BxC, "ctg_tab", "row_tab", (int)this.current_tab, blist.ToArray(), new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnTabChanged), this.BxC.use_w * 0.9f, 0f, false, false);
					this.RTabBar.LR_valotile = true;
				}
			}
			this.BxC.Br();
			float use_w = this.BxC.use_w;
			float num2 = this.BxC.use_h - 40f;
			if ((this.useable_tab_bits & 2U) != 0U)
			{
				Designer designer = this.BxC.addTab("DANGER", use_w, num2, use_w, num2, false);
				designer.Smallest();
				designer.margin_in_tb = 10f;
				designer.item_margin_x_px = this.BxC.item_margin_x_px;
				designer.item_margin_y_px = this.BxC.item_margin_y_px;
				if (this.danger_home_enable)
				{
					designer.margin_in_tb = 30f;
					designer.margin_in_lr = 40f;
				}
				int num3 = 0;
				int num4 = 0;
				while (num4 < 3 && reached_night_level >= 16 * (num4 + 1))
				{
					num3 += 3;
					num4++;
				}
				this.BxC.addP(new DsnDataP("", false)
				{
					text = TX.Get("Depert_you_can_do_fast_travel", ""),
					size = 16f,
					swidth = this.BxC.use_w,
					TxCol = C32.d2c(4283780170U)
				}, false);
				this.BxC.Br();
				this.BxC.Hr(0.5f, 18f, 22f, 1f);
				this.BxC.Br();
				this.Slider = this.BxC.addSliderCT(new UiItemManageBoxSlider.DsnDataSliderIMB(null, null)
				{
					name = "create_count",
					valintv = 1f,
					mn = 0f,
					mx = (float)X.Mx(0, num3 - 1),
					fnClick = new FnBtnBindings(this.fnClickedBtn),
					w = 1f,
					h = 44f,
					default_focus = 1,
					fnDescConvert = new FnDescConvert(this.fnDescConvertSlider)
				}, this.BxC.use_w - 2f, null, false);
				this.BxC.Br();
				this.BxC.addHr(new DsnDataHr().H(4f));
				this.BxC.Br();
				this.BxC.endTab(true);
			}
			if ((this.useable_tab_bits & 1U) != 0U)
			{
				Designer designer2 = this.BxC.addTab("HOME", use_w, num2, use_w, num2, false);
				designer2.Smallest();
				designer2.margin_in_lr = 20f;
				designer2.margin_in_tb = 0f;
				designer2.item_margin_x_px = this.BxC.item_margin_x_px;
				designer2.item_margin_y_px = this.BxC.item_margin_y_px;
				designer2.init();
				this.BConHome = this.BxC.addRadioT<aBtnNel>(new DsnDataRadio
				{
					name = "home_list",
					def = -1,
					w = designer2.use_w - 20f,
					h = 24f,
					margin_h = -1000,
					margin_w = 0,
					fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnSelectHomeList),
					skin = "row_center",
					locked_click = true,
					SCA = new ScrollAppend(this.BxC.box_stencil_ref_mask + 1, designer2.use_w, designer2.use_h, 4f, 6f, 0),
					keysL = this.Amoveable_home_wm,
					fnMakingAfter = delegate(BtnContainer<aBtn> BCon, aBtn B)
					{
						string text = this.Amoveable_home_wm[B.carr_index];
						WholeMapItem wholeMapItem = this.M2D.WM.getWholeMapDescriptionObject()[text];
						ButtonSkinNelUi buttonSkinNelUi = B.get_Skin() as ButtonSkinNelUi;
						if (wholeMapItem == this.M2D.WM.CurWM)
						{
							B.SetLocked(true, true, false);
						}
						else if (!wholeMapItem.safe_area)
						{
							this.bconhome_def = B.carr_index;
							B.WH(B.w, B.h * 1.6f);
							buttonSkinNelUi.fix_text_size = 20f;
						}
						buttonSkinNelUi.bottom_line = true;
						using (STB stb = TX.PopBld(null, 0))
						{
							if (wholeMapItem.safe_area)
							{
								stb.Add("<img mesh=\"", "house_inventory", "\" width=\"32\" height=\"24\" />");
							}
							stb.Add(wholeMapItem.localized_name);
							buttonSkinNelUi.setTitleTextS(stb);
						}
						return true;
					}
				});
				this.BxC.endTab(true);
			}
			this.cancel_key = "&&Cancel";
			this.CancelBtn = this.BxC.addButtonT<aBtnNel>(new DsnDataButton
			{
				skin = "normal",
				title = this.cancel_key,
				name = this.cancel_key,
				w = 260f,
				h = 30f,
				fnClick = new FnBtnBindings(this.fnClickedBtn)
			});
			this.BxC.positionD(X.Mx(0f, (!this.M2D.GM.isActive()) ? this.M2D.ui_shift_x : 0f), 0f, 1, 30f);
			this.fineTabVisibility();
			return this;
		}

		public string fnDescConvertSlider(string str)
		{
			int num = X.NmI(str, 0, false, false);
			int num2 = num / 3 + 1;
			int num3 = num % 3;
			if (num3 == 0)
			{
				return TX.GetA("Depert_from_noon", num2.ToString(), this.val2nightLevel(num).ToString());
			}
			if (num3 != 1)
			{
				return TX.GetA("Depert_from_night", num2.ToString(), this.val2nightLevel(num).ToString());
			}
			return TX.GetA("Depert_from_evening", num2.ToString(), this.val2nightLevel(num).ToString());
		}

		public int val2nightLevel(int val)
		{
			int num = val / 3;
			int num2 = 0;
			int num3 = val % 3;
			if (num3 != 1)
			{
				if (num3 == 2)
				{
					num2 = 9;
				}
			}
			else
			{
				num2 = 5;
			}
			return num * 16 + num2;
		}

		private bool fnClickedBtn(aBtn B)
		{
			string title = B.title;
			string text;
			if (title != null && title == "&&Cancel")
			{
				text = "-1";
			}
			else
			{
				int num = this.val2nightLevel(X.IntR(this.Slider.getValue()));
				text = num.ToString();
				SND.Ui.play("enter", false);
				if (num > 0)
				{
					SND.Ui.play("nightlevel_up", false);
				}
			}
			if (text != null)
			{
				base.prompt_result = text;
				this.deactivate(false);
			}
			return true;
		}

		public bool fnTabChanged(BtnContainer<aBtn> BCon, int pre, int cur)
		{
			if (cur < 0)
			{
				return true;
			}
			int num = X.bit_on_index(this.useable_tab_bits, cur);
			if (num < 0)
			{
				return false;
			}
			this.current_tab = (UiDangerLevelInitBox.TAB)num;
			this.fineTabVisibility();
			return true;
		}

		public void fineTabVisibility()
		{
			if (this.RTabBar != null)
			{
				this.BxC.rowRemakeCheck(false);
				Designer.EvacuateContainer evacuateContainer = new Designer.EvacuateContainer(this.BxC, this.BxC.EvacuateMemory(this.AEvcTab, null, true), true);
				this.AEvcTab = evacuateContainer.AEvc;
				this.BxC.init();
				this.BxC.alignx = ALIGN.CENTER;
				this.BxC.ReassignEvacuatedMemory(null, evacuateContainer.OnamedObject, false);
				int num = this.AEvcTab.Count;
				string text = "TAB__" + this.current_tab.ToString();
				for (int i = 0; i < 3; i++)
				{
					int j = 0;
					while (j < num)
					{
						Designer.EvacuateMem evacuateMem = this.AEvcTab[j];
						if (evacuateMem.Blk == this.CancelBtn)
						{
							if (i != 2)
							{
								j++;
								continue;
							}
						}
						else if (evacuateMem.Blk is Designer)
						{
							if (i != 1)
							{
								j++;
								continue;
							}
							if (this.BxC.getName(evacuateMem.Blk as IVariableObject) != text)
							{
								j++;
								continue;
							}
						}
						this.BxC.ReassignEvacuatedMemory(evacuateMem);
						this.AEvcTab.RemoveAt(j);
						num--;
						if (evacuateMem.name == "warpTo" || evacuateMem.Blk is Designer)
						{
							this.BxC.Br();
						}
					}
				}
			}
			UiDangerLevelInitBox.TAB tab = this.current_tab;
			if (tab != UiDangerLevelInitBox.TAB.HOME)
			{
				if (tab == UiDangerLevelInitBox.TAB.DANGER)
				{
					this.Slider.Select(false);
					this.CancelBtn.setNaviT(this.Slider, true, true);
					this.CancelBtn.setNaviB(this.Slider, true, true);
				}
			}
			else if (this.BConHome != null)
			{
				int num2 = X.Mx(0, this.Amoveable_home_wm.IndexOf(this.M2D.WM.CurWM.Mp.key));
				this.BConHome.Get(((this.useable_tab_bits & 2U) == 0U) ? this.bconhome_def : num2).Select(false);
				this.BConHome.setValue(num2, false);
				this.CancelBtn.setNaviT(this.BConHome.Get(this.BConHome.Length - 1), true, true);
				this.CancelBtn.setNaviB(this.BConHome.Get(0), true, true);
			}
			IN.clearPushDown(true);
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				return false;
			}
			ColumnRowNel rtabBar = this.RTabBar;
			return true;
		}

		public bool fnSelectHomeList(BtnContainer<aBtn> BCon, int pre, int cur)
		{
			if (cur < 0)
			{
				return true;
			}
			aBtn aBtn = BCon.Get(cur);
			if (aBtn == null || aBtn.isLocked())
			{
				return false;
			}
			WholeMapItem wholeMapItem = X.Get<string, WholeMapItem>(this.M2D.WM.getWholeMapDescriptionObject(), BCon.Get(cur).title);
			if (wholeMapItem == null)
			{
				return false;
			}
			if (!wholeMapItem.safe_area)
			{
				if ((this.useable_tab_bits & 2U) != 0U)
				{
					this.RTabBar.setValue(X.bit_index(this.useable_tab_bits, 2U), true);
					return false;
				}
				base.prompt_result = "0";
				this.deactivate(false);
			}
			else
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					WholeMapItem wholeMapItem2;
					WholeMapItem.WMTransferPoint.WMRectItem depertureRectForHomeSwitch = wholeMapItem.getDepertureRectForHomeSwitch(this.LpFrom.getDepertureDestWMI(), this.M2D.WM.CurWM, out wholeMapItem2);
					if (depertureRectForHomeSwitch == null)
					{
						X.de("拠点間移動の DepertureRect (IN) 算出に失敗", null);
					}
					else
					{
						WholeMapItem.WMTransferPoint.WMRectItem depertureRect = wholeMapItem2.getDepertureRect(depertureRectForHomeSwitch.Mp, -2, depertureRectForHomeSwitch.another_key_for_worp);
						if (depertureRect == null)
						{
							X.de("拠点間移動の DepertureRect (OUT) 算出に失敗", null);
						}
						else
						{
							stb.AR("ADD_MAPFLUSH_FLAG \n");
							this.LpFrom.WriteGoOutOtherAreaAfter(stb, wholeMapItem2, wholeMapItem, true, "!" + depertureRectForHomeSwitch.key, depertureRect);
							EvReader evReader = new EvReader("%M2D_HOME_SWITCH", 0, null, null);
							evReader.parseText(stb.ToString());
							EV.stackReader(evReader, -1);
						}
					}
				}
				base.prompt_result = "-1";
				this.deactivate(false);
			}
			return true;
		}

		private const float btn_h = 30f;

		private string cancel_key;

		private aBtnMeter Slider;

		private List<string> Amoveable_home_wm;

		public const string var_deperture_override = "_dep_override";

		private const uint BIT_HOME = 1U;

		private const uint BIT_DANGER = 2U;

		private UiDangerLevelInitBox.TAB current_tab;

		private uint useable_tab_bits;

		private List<Designer.EvacuateMem> AEvcTab;

		private BtnContainerRadio<aBtn> BConHome;

		private M2LpMapTransferWarp LpFrom;

		private int bconhome_def;

		private ColumnRowNel RTabBar;

		private enum TAB
		{
			HOME,
			DANGER,
			_MAX
		}
	}
}
