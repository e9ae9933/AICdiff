using System;
using System.Collections.Generic;
using evt;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class UiSkillManageBox
	{
		public static void newGame()
		{
			UiSkillManageBox.current_tab = SkillManager.SKILL_CTG.PUNCH;
			if (UiSkillManageBox.ASelected == null)
			{
				UiSkillManageBox.ASelected = new PrSkill[5];
				return;
			}
			global::XX.X.ALLN<PrSkill>(UiSkillManageBox.ASelected);
		}

		public static void readBinaryFrom(ByteArray Ba)
		{
			UiSkillManageBox.newGame();
			int num = (int)Ba.readUByte();
			UiSkillManageBox.current_tab = (SkillManager.SKILL_CTG)(1 << global::XX.X.MMX(0, num, 4));
			int num2 = (int)Ba.readUByte();
			for (int i = 0; i < num2; i++)
			{
				PrSkill byId = SkillManager.GetById(Ba.readUShort(), null);
				if (i < UiSkillManageBox.ASelected.Length)
				{
					UiSkillManageBox.ASelected[i] = byId;
				}
			}
		}

		public static void writeBinaryTo(ByteArray Ba)
		{
			if (UiSkillManageBox.ASelected == null)
			{
				UiSkillManageBox.newGame();
			}
			Ba.writeByte(global::XX.X.beki_cnt((uint)UiSkillManageBox.current_tab));
			int num = UiSkillManageBox.ASelected.Length;
			Ba.writeByte(num);
			for (int i = 0; i < num; i++)
			{
				PrSkill prSkill = UiSkillManageBox.ASelected[i];
				Ba.writeUShort((prSkill == null) ? 0 : prSkill.id);
			}
		}

		public static string[] getTabKeys()
		{
			if (UiSkillManageBox.Atab_keys != null)
			{
				return UiSkillManageBox.Atab_keys;
			}
			int num = 5;
			string[] array = (UiSkillManageBox.Atab_keys = new string[num]);
			for (int i = 0; i < num; i++)
			{
				array[i] = TX.Get("Skill_tab_title_" + ((SkillManager.SKILL_CTG)(1 << i)).ToString().ToLower(), "");
			}
			return array;
		}

		public static void releaseTabKeysCache()
		{
			UiSkillManageBox.Atab_keys = null;
		}

		public UiSkillManageBox()
		{
			if (UiSkillManageBox.ASelected == null)
			{
				UiSkillManageBox.ASelected = new PrSkill[5];
			}
		}

		public UiSkillManageBox destruct()
		{
			return null;
		}

		public void deactivateDesigner()
		{
			this.DList = null;
			this.DsDesc = null;
			this.DListTab = null;
			this.FirstFocus_ = null;
			this.RTab = null;
		}

		public aBtn FirstFocus
		{
			get
			{
				if (this.FirstFocus_ == null && this.RTab != null)
				{
					int num = global::XX.X.MMX(0, global::XX.X.beki_cnt((uint)UiSkillManageBox.current_tab), 4);
					return this.RTab.Get(num);
				}
				return this.FirstFocus_;
			}
		}

		public void activateEdit()
		{
			this.focused_time = 0f;
			this.need_recheck_connection = false;
		}

		public void deactivateEdit()
		{
			if (this.need_recheck_connection)
			{
				this.need_recheck_connection = false;
				M2PrSkill.resetSkillConnectionWhole(false, false, false);
			}
		}

		public UiSkillManageBox CreateTo(Designer Ds, Func<PrSkill, bool> _FnEnableCheckBoxClick)
		{
			this.DsDesc = null;
			this.DListTab = null;
			this.DList = null;
			this.FnEnableCheckBoxClick = _FnEnableCheckBoxClick;
			this.focused_time = 0f;
			this.FirstFocus_ = null;
			float use_w = Ds.use_w;
			float use_h = Ds.use_h;
			Ds.item_margin_x_px = 0f;
			Ds.item_margin_y_px = 0f;
			int num = global::XX.X.MMX(0, global::XX.X.beki_cnt((uint)UiSkillManageBox.current_tab), 4);
			Dictionary<string, PrSkill> skillDictionary = SkillManager.getSkillDictionary();
			this.RTab = ColumnRow.CreateT<aBtnNel>(Ds, "ctg_tab", "row_tab", -1, UiSkillManageBox.getTabKeys(), new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnTabChanged), use_w, 0f, true, false).LrInput(false);
			Designer designer = (this.DListTab = Ds.Br().addTab("_tab_skill_rows", use_w, Ds.use_h - 4f, use_w, Ds.use_h - 4f, true));
			designer.Smallest();
			designer.bgcol = C32.MulA(4290689711U, 0.25f);
			this.DList = null;
			Ds.endTab(true);
			this.Anew_count = new int[5];
			foreach (KeyValuePair<string, PrSkill> keyValuePair in skillDictionary)
			{
				PrSkill value = keyValuePair.Value;
				if (!value.first_visible && value.visible && value.new_icon)
				{
					for (int i = 0; i < 5; i++)
					{
						if ((value.category & (SkillManager.SKILL_CTG)(1 << i)) != (SkillManager.SKILL_CTG)0)
						{
							int[] anew_count = this.Anew_count;
							int num2 = i;
							int num3 = anew_count[num2];
							anew_count[num2] = num3 + 1;
							if (num3 == 0)
							{
								(this.RTab.Get(i).get_Skin() as ButtonSkinNelTab).new_circle = true;
							}
						}
					}
				}
			}
			this.RTab.setValue(num, true);
			return this;
		}

		public void removeNewFlag()
		{
		}

		internal void CreateDescTo(Designer DsDesc)
		{
			this.DsDesc = DsDesc;
			DsDesc.Clear();
			DsDesc.init();
			DsDesc.item_margin_x_px = 40f;
			float use_w = DsDesc.use_w;
			float use_h = DsDesc.use_h;
			DsDesc.addImg(new DsnDataImg
			{
				name = "thumbnail",
				swidth = global::XX.X.Mx(120f, use_w * 0.22f),
				sheight = use_h - 1f,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				Col = MTRX.ColWhite,
				MI = MTR.MIiconL,
				UvRect = new Rect(0f, 0f, 0f, 0f)
			});
			Designer designer = DsDesc.addTab("_descript", DsDesc.use_w, use_h - 1f, DsDesc.use_w, use_h - 1f, false);
			designer.Smallest();
			designer.alignx = ALIGN.CENTER;
			DsDesc.addP(new DsnDataP("", false)
			{
				name = "desc",
				TxCol = C32.d2c(4283780170U),
				swidth = designer.use_w,
				sheight = use_h - 40f,
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.MIDDLE,
				text = "\u3000",
				html = true
			}, false);
			(DsDesc.Br().addButtonT<aBtnNel>(new DsnDataButton
			{
				name = "skill_enable",
				title = "skill_enable",
				skin_title = "&&CheckBox_Use",
				w = designer.use_w * 0.35f,
				h = 38f,
				hover_to_select = false,
				click_to_select = false,
				skin = "checkbox",
				fnClick = new FnBtnBindings(this.fnClickDescCheck)
			}).get_Skin() as ButtonSkinCheckBox).setScale(1f);
			DsDesc.endTab(true);
			this.recheckDescEnable(true);
		}

		private bool fnTabChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (EV.isStoppingGameHandle())
			{
				return false;
			}
			if (UiSkillManageBox.event_focus_waiting_tab != 0U && 1U << pre_value == UiSkillManageBox.event_focus_waiting_tab)
			{
				return false;
			}
			UiSkillManageBox.current_tab = (SkillManager.SKILL_CTG)(1 << cur_value);
			if (this.DListTab == null)
			{
				return true;
			}
			this.DListTab.Clear();
			this.DListTab.Smallest().init();
			this.DList = this.DListTab.Br().addRadioT<aBtnNelRowSkill>(new DsnDataRadio
			{
				name = "skill_rows",
				clms = 1,
				margin_w = 0,
				margin_h = 0,
				navigated_to_click = true,
				w = this.DListTab.use_w,
				h = 38f,
				fnGenerateKeys = new FnGenerateRemakeKeys(this.fnGenerateSkillKeys),
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnSkillChanged)
			});
			if (this.DList != null)
			{
				int num = global::XX.X.MMX(0, global::XX.X.beki_cnt((uint)UiSkillManageBox.current_tab), 4);
				string text = ((UiSkillManageBox.ASelected[num] != null) ? UiSkillManageBox.ASelected[num].key : null);
				if (this.DList.Length > 0)
				{
					for (int i = this.DList.Length - 1; i >= 0; i--)
					{
						(this.DList.Get(i) as aBtnNelRowSkill).setContainer(this);
					}
					this.DList.Get(0).setNaviT(_B.Get(cur_value), true, true);
					this.DList.Get(this.DList.Length - 1).setNaviB(_B.Get(cur_value), true, true);
					if (TX.valid(text))
					{
						this.DList.setValue(text);
					}
					else
					{
						this.DList.setValue(0, true);
						UiSkillManageBox.ASelected[num] = SkillManager.Get(this.DList.GetButton(0).title);
					}
					this.fnSkillClicked(this.DList.GetButton(this.DList.getValue()));
				}
				else
				{
					this.fnSkillClicked(null);
				}
				int length = _B.Length;
				for (int j = 0; j < length; j++)
				{
					if (this.DList.Length > 0)
					{
						if (j != cur_value)
						{
							_B.Get(j).setNaviB(this.DList.Get(0), false, true).setNaviT(this.DList.Get(this.DList.Length - 1), false, true);
						}
					}
					else
					{
						_B.Get(j).setNaviB(null, false, true).setNaviT(null, false, true);
					}
				}
			}
			return true;
		}

		public void fnGenerateSkillKeys(BtnContainerBasic BaCon, List<string> Adest)
		{
			if (UiSkillManageBox.current_tab == (SkillManager.SKILL_CTG)0)
			{
				return;
			}
			foreach (KeyValuePair<string, PrSkill> keyValuePair in SkillManager.getSkillDictionary())
			{
				PrSkill value = keyValuePair.Value;
				if ((value.visible || global::XX.X.DEBUGALLSKILL) && (value.category & UiSkillManageBox.current_tab) != (SkillManager.SKILL_CTG)0)
				{
					Adest.Add(value.key);
				}
			}
		}

		private bool fnSkillChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (EV.isStoppingGameHandle())
			{
				return false;
			}
			int num = global::XX.X.MMX(0, global::XX.X.beki_cnt((uint)UiSkillManageBox.current_tab), 4);
			aBtn button = _B.GetButton(cur_value);
			if (button != null)
			{
				UiSkillManageBox.ASelected[num] = SkillManager.Get(button.title);
				this.fnSkillClicked(button);
			}
			return true;
		}

		private bool fnSkillClicked(aBtn B)
		{
			if (EV.isStoppingGameHandle())
			{
				return false;
			}
			PrSkill prSkill = null;
			if (B != null && B is aBtnNelRowSkill)
			{
				if (B != this.FirstFocus_)
				{
					this.FirstFocus_ = B as aBtnNelRowSkill;
					this.focused_time = 0f;
				}
				prSkill = this.FirstFocus_.Sk;
			}
			else
			{
				this.FirstFocus_ = null;
				this.focused_time = 0f;
			}
			this.CurSel = prSkill;
			this.need_fine_desc_enable = true;
			return true;
		}

		public void recheckDescEnable(bool force = false)
		{
			if ((force || this.need_fine_desc_enable) && this.DsDesc != null)
			{
				this.need_fine_desc_enable = false;
				FillImageBlock fillImageBlock = this.DsDesc.Get("thumbnail", false) as FillImageBlock;
				if (fillImageBlock != null)
				{
					if (this.CurSel == null)
					{
						fillImageBlock.gameObject.SetActive(false);
					}
					else
					{
						fillImageBlock.gameObject.SetActive(true);
						fillImageBlock.redraw_flag = true;
						fillImageBlock.FnDraw = null;
						fillImageBlock.PF = this.CurSel.getPF();
					}
				}
				IVariableObject variableObject = this.DsDesc.Get("desc", false);
				if (variableObject != null)
				{
					if (this.CurSel == null)
					{
						variableObject.setValue("");
					}
					else
					{
						variableObject.setValue("<b><font size=\"18\">" + this.CurSel.tutorial_desc + "</font></b>\n\n" + this.CurSel.descript);
					}
				}
				aBtnNel aBtnNel = this.DsDesc.Get("skill_enable", false) as aBtnNel;
				if (aBtnNel != null)
				{
					if (this.CurSel == null || this.CurSel.always_enable)
					{
						aBtnNel.hide();
						aBtnNel.gameObject.SetActive(false);
						return;
					}
					aBtnNel.gameObject.SetActive(true);
					aBtnNel.bind();
					aBtnNel.SetChecked(this.CurSel.enabled, true);
				}
			}
		}

		public bool canEnableCheckClick(PrSkill Sk)
		{
			return this.FnEnableCheckBoxClick == null || this.FnEnableCheckBoxClick(Sk);
		}

		public void fnSkillEnableChanged(PrSkill Sk, bool change_list, bool change_desc)
		{
			if (Sk == null || Sk.always_enable)
			{
				return;
			}
			this.need_recheck_connection = true;
			if (change_list)
			{
				aBtnNelRowSkill aBtnNelRowSkill = this.DList.Get(Sk.key) as aBtnNelRowSkill;
				if (aBtnNelRowSkill != null && aBtnNelRowSkill.UseCheck != null)
				{
					aBtnNelRowSkill.UseCheck.SetChecked(Sk.enabled, true);
				}
			}
			if (Sk == this.CurSel && change_desc && this.DsDesc != null)
			{
				aBtnNel aBtnNel = this.DsDesc.Get("skill_enable", false) as aBtnNel;
				if (aBtnNel != null)
				{
					aBtnNel.SetChecked(Sk.enabled, true);
				}
			}
		}

		private bool fnClickDescCheck(aBtn B)
		{
			if (EV.isStoppingGameHandle())
			{
				return false;
			}
			PrSkill curSel = this.CurSel;
			if (curSel != null && !curSel.always_enable && (curSel.visible || global::XX.X.DEBUGALLSKILL))
			{
				if (!this.canEnableCheckClick(curSel))
				{
					return false;
				}
				B.SetChecked(!B.isChecked(), true);
				curSel.enabled = B.isChecked();
				this.fnSkillEnableChanged(curSel, true, false);
			}
			return true;
		}

		public bool isTabOpening(SkillManager.SKILL_CTG c)
		{
			return this.RTab != null && UiSkillManageBox.current_tab == c;
		}

		public void runAppearSkillManager()
		{
			this.recheckDescEnable(false);
		}

		public bool runSkillManager(bool handle)
		{
			if (this.RTab == null)
			{
				return false;
			}
			if (handle && this.RTab.runLRInput(-2))
			{
				if (this.FirstFocus_ != null)
				{
					this.FirstFocus_.Select(false);
				}
				else
				{
					this.RTab.Get(this.RTab.getValue()).Select(false);
				}
			}
			if (this.FirstFocus_ != null && this.FirstFocus_.Sk.new_icon)
			{
				this.focused_time += 1f;
				PrSkill sk = this.FirstFocus_.Sk;
				if (this.focused_time >= (float)(((sk.category & SkillManager.SKILL_CTG.HPMP) != (SkillManager.SKILL_CTG)0) ? 2 : 24))
				{
					sk.new_icon = false;
					for (int i = 0; i < 5; i++)
					{
						if (this.Anew_count[i] > 0 && (sk.category & (SkillManager.SKILL_CTG)(1 << i)) != (SkillManager.SKILL_CTG)0)
						{
							int[] anew_count = this.Anew_count;
							int num = i;
							int num2 = anew_count[num] - 1;
							anew_count[num] = num2;
							if (num2 == 0)
							{
								(this.RTab.Get(i).get_Skin() as ButtonSkinNelTab).new_circle = false;
							}
						}
					}
				}
			}
			return true;
		}

		public static SkillManager.SKILL_CTG current_tab = SkillManager.SKILL_CTG.PUNCH;

		public static uint event_focus_waiting_tab = 0U;

		public static PrSkill[] ASelected;

		private static string[] Atab_keys;

		private Designer DsDesc;

		private ColumnRow RTab;

		private Designer DListTab;

		private BtnContainerRadio<aBtn> DList;

		private PrSkill CurSel;

		private aBtnNelRowSkill FirstFocus_;

		private Func<PrSkill, bool> FnEnableCheckBoxClick;

		private int[] Anew_count;

		public float focused_time;

		private bool need_recheck_connection;

		public bool need_fine_desc_enable;
	}
}
