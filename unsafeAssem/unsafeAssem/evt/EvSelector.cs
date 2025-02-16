using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace evt
{
	public class EvSelector : MsgBox
	{
		protected override void Awake()
		{
			base.Awake();
			this.sel_w = this.SEL_W;
			this.ARow = new List<aBtn>();
			this.Atext = new List<string>();
			this.BCon = new BtnContainerRadio<aBtn>(base.gameObject, 0, 0, null, null);
			this.BCon.addChangedFn(new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangeFocus));
			this.BCon.value_return_name = true;
			base.gameObject.SetActive(false);
		}

		public float SEL_X
		{
			get
			{
				return 0f;
			}
		}

		public float SEL_Y
		{
			get
			{
				return 60f;
			}
		}

		public void evInit()
		{
			base.posSetA(0f, 0f, 0f, 0f, true);
			IN.Pos2(base.transform, 0f, 0f);
		}

		public EvSelector clear()
		{
			this.clms = (this.rows = 0);
			this.BCon.DestroyButtons(false, false);
			for (int i = this.ARow.Count - 1; i >= 0; i--)
			{
				if (!this.ARow[i].destructed)
				{
					IN.DestroyOne(this.ARow[i].gameObject);
				}
			}
			this.ARow.Clear();
			this.Atext.Clear();
			this.default_focus = -1;
			this.randam_talk_focus = null;
			this.position_key = null;
			this.result = (this.define_to = "");
			this.BtnCurs = (this.BtnCancel = null);
			this.sel_w = this.SEL_W;
			if (this.t > (float)(-(float)this.t_hide))
			{
				this.deactivate();
				base.fineMove();
			}
			return this;
		}

		public void evEnd()
		{
			this.selectQuit();
			this.ORandomTalkFocus = null;
		}

		public void setPos(string key)
		{
			if (TX.noe(key))
			{
				this.position_key = null;
				return;
			}
			Vector4 vector;
			if (TalkDrawer.getDefinedPosition(key, out vector))
			{
				if (base.isActive())
				{
					base.position(vector.x, vector.y, -1000f, -1000f, false);
				}
				else
				{
					IN.PosP2(base.transform, vector.x, vector.y);
					base.posSetA(vector.x, vector.y, vector.x, vector.y, true);
				}
				this.position_key = key;
			}
		}

		public aBtn addRow(string _txt, string key, string optional_key, bool use_checkmark = false)
		{
			if (this.ARow.Count == 0)
			{
				this.sel_w = this.SEL_W;
			}
			if (!TX.valid(key))
			{
				key = _txt;
			}
			aBtn aBtn = IN.CreateGob(this, "-bt_" + key).AddComponent<aBtn>();
			aBtn.w = (float)this.sel_w;
			aBtn.h = 28f;
			aBtn.z_push_click = true;
			aBtn.hover_to_select = true;
			aBtn.locked_click = false;
			aBtn.initializeSkin("command", key);
			aBtn.setSkinTitle((use_checkmark ? "<shape tx_color check />" : "") + _txt);
			aBtn.addClickFn(new FnBtnBindings(this.fnClickRow));
			aBtn.addHoverFn(new FnBtnBindings(this.fnHoverRow));
			ButtonSkinCommand buttonSkinCommand = aBtn.get_Skin() as ButtonSkinCommand;
			if (buttonSkinCommand != null)
			{
				buttonSkinCommand.flags = optional_key;
			}
			IN.setZ(aBtn.transform, -0.05f);
			this.ARow.Add(aBtn);
			this.Atext.Add(_txt);
			if (TX.hasSpecificChar(optional_key, 'C', true))
			{
				this.BtnCancel = aBtn;
			}
			if (TX.hasSpecificChar(optional_key, 'S', true))
			{
				aBtn.SetLocked(true, true, false);
			}
			else
			{
				aBtn.SetLocked(false, true, false);
			}
			if (!EV.canEventHandle())
			{
				aBtn.hide();
			}
			return aBtn;
		}

		public bool fnChangeFocus(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (!EV.canEventHandle())
			{
				return false;
			}
			aBtn button = _B.GetButton(cur_value);
			if (this.BtnCurs != button)
			{
				this.BtnCurs = button;
				button.Select(false);
			}
			return true;
		}

		public bool fnHoverRow(aBtn B)
		{
			if (!EV.canEventHandle())
			{
				return false;
			}
			if (B != this.BtnCurs)
			{
				this.BtnCurs = B;
				this.BCon.setValue(B);
				B.Select(false);
				if (IN.use_mouse)
				{
					IN.clearSubmitPushDown(true);
					IN.clearCancelPushDown(true);
				}
				this.default_focus = this.getDefaultFocusIndex(B.title);
			}
			return true;
		}

		public bool fnClickRow(aBtn B)
		{
			if (this.result != "" || this.t < 20f || B.isLocked())
			{
				return false;
			}
			this.result = B.title;
			this.BCon.setValue(B);
			return true;
		}

		public override MsgBox deactivate()
		{
			CURS.Active.Rem("EvSelector");
			base.deactivate();
			this.BCon.hide(false, false);
			return this;
		}

		public bool activate(string _define_to)
		{
			if (this.ARow.Count == 0)
			{
				return false;
			}
			this.define_to = _define_to;
			this.activate();
			return true;
		}

		public override MsgBox activate()
		{
			SND.Ui.play("paper", false);
			bool flag = false;
			if (this.clms == 0)
			{
				this.makeBxSel();
				flag = true;
			}
			base.activate();
			if (flag && this.position_key == null)
			{
				base.position(this.SEL_X, this.SEL_Y - 50f, this.SEL_X, this.SEL_Y, false);
			}
			CURS.Active.Add("EvSelector");
			if (EV.canEventHandle())
			{
				this.set_handle(true);
			}
			else
			{
				this.set_handle(false);
			}
			IN.clearPushDown(true);
			this.result = "";
			return this;
		}

		private void makeBxSel()
		{
			int num = this.ARow.Count;
			this.BCon.DestroyButtons(false, false);
			this.clms = (TX.isEnglishLang() ? (1 + num / 10) : X.IntC((float)(num + 3) / 10f));
			this.rows = X.IntC((float)num / (float)this.clms);
			this.Carr = new ObjCarrierCon();
			this.BCon.carr_fix_length = num;
			this.BCon.navi_loop = 3;
			float num2 = (float)this.SEL_W;
			for (int i = 0; i < num; i++)
			{
				int num3 = i / this.rows;
				int num4 = this.rows - 1 - i % this.rows;
				aBtn aBtn = this.ARow[i];
				ButtonSkinCommand buttonSkinCommand = aBtn.get_Skin() as ButtonSkinCommand;
				if (this.clms <= 2)
				{
					num2 = X.Mx(num2, buttonSkinCommand.getFirstDrawnTitleWidth() + 24f);
				}
				else
				{
					aBtn.WH(aBtn.w, aBtn.h);
				}
				this.BCon.addBtn(aBtn);
				buttonSkinCommand.use_valotile = EV.valotile_overwrite_msg || EV.use_valotile;
				aBtn.carr_index = num4 * this.clms + num3;
			}
			if (this.clms <= 2 && num2 > (float)this.SEL_W)
			{
				for (int i = 0; i < num; i++)
				{
					aBtn aBtn2 = this.ARow[i];
					aBtn2.WH(num2, aBtn2.h);
				}
			}
			this.Carr.BoundsPx(num2 * (float)(this.clms - 1), (float)(28 * (this.rows - 1)), this.clms);
			this.Carr.StartShiftPx(0f, 0f, 1, "");
			this.BCon.setCarrierContainer(this.Carr, true);
			this.BCon.runCarrier(100f, null);
			IN.clearPushDown(true);
			this.no_text = true;
			this.wh(num2 * (float)this.clms, this.h = (float)(28 * this.rows)).margin(new float[] { 26f, 18f }).make("");
			base.use_valotile = EV.valotile_overwrite_msg || EV.use_valotile;
			num = this.ARow.Count;
			IN.setZAbs(base.transform, -4.925f);
			if (!this.BtnCancel)
			{
				for (int i = 0; i < num; i++)
				{
					string text = this.Atext[i];
					aBtn aBtn3 = this.ARow[i];
					if (text != null && (text == "&&Select_skip" || text == "&&Select_no" || text == "&&Select_nope" || text == "&&Select_nothanks" || text == "&&Cancel"))
					{
						this.BtnCancel = aBtn3;
						i = num;
					}
				}
			}
			int num5 = this.considerRandomTalkFocus(false);
			if (num5 != -1)
			{
				aBtn button = this.BCon.GetButton(num5);
				if (button != null)
				{
					button.Select(false);
				}
			}
		}

		public int considerRandomTalkFocus(bool input_default_focus = true)
		{
			int num = this.default_focus;
			if (num == -1 && this.randam_talk_focus != null)
			{
				num = 0;
				bool flag = true;
				if (this.ORandomTalkFocus != null)
				{
					string text = X.Get<string, string>(this.ORandomTalkFocus, this.randam_talk_focus);
					if (text != null)
					{
						if (TX.isStart(text, "!\n", 0))
						{
							flag = false;
							text = TX.slice(text, 2);
						}
						num = X.Mx(this.getDefaultFocusIndex(text), 0);
					}
				}
				if (flag)
				{
					int num2 = num;
					int num3 = num;
					int count = this.ARow.Count;
					for (int i = 0; i < count; i++)
					{
						if (!this.hasCheckMark(num2) && !this.BCon.Get(i).isLocked())
						{
							num = num3;
							break;
						}
						num3++;
						num2 = (num2 + 1) % count;
					}
				}
			}
			if (input_default_focus)
			{
				this.default_focus = num;
			}
			return num;
		}

		public void setDefaultFocus(string key)
		{
			this.default_focus = this.getDefaultFocusIndex(key);
		}

		public int getDefaultFocusIndex(string key)
		{
			int num = this.Atext.IndexOf(key);
			if (num != -1)
			{
				return num;
			}
			for (int i = this.ARow.Count - 1; i >= 0; i--)
			{
				if (this.ARow[i].title == key)
				{
					return i;
				}
			}
			return -1;
		}

		public void initRandomTalkFocus(string rkey)
		{
			if (this.ORandomTalkFocus == null)
			{
				this.ORandomTalkFocus = new BDic<string, string>(1);
			}
			this.randam_talk_focus = rkey;
		}

		public bool hasCancelBtn()
		{
			return this.BtnCancel;
		}

		public bool hasCheckMark(int index)
		{
			return TX.isStart(this.ARow[index].get_Skin().title, "<shape tx_color check />", 0);
		}

		public void setCheckMark(int index)
		{
			if (X.BTW(0f, (float)index, (float)this.ARow.Count))
			{
				ButtonSkin skin = this.ARow[index].get_Skin();
				if (!TX.isStart(skin.title, "<shape tx_color check />", 0))
				{
					skin.setTitle("<shape tx_color check />" + skin.title);
				}
			}
		}

		private void selectQuit()
		{
			if (base.isActive())
			{
				if (this.result != "")
				{
					this.BCon.setValue(this.result);
					int value = this.BCon.getValue();
					if (this.randam_talk_focus != null && value >= 0)
					{
						this.ORandomTalkFocus[this.randam_talk_focus] = (this.hasCheckMark(value) ? "!\n" : "") + this.result;
					}
					if (EV.Log != null && value >= 0)
					{
						EV.Log.AddSelection(this.Atext[value], false);
					}
				}
				this.randam_talk_focus = null;
				this.deactivate();
				IN.clearPushDown(true);
			}
		}

		public void addLastSelectionToLog()
		{
			if (this.result != "")
			{
				int value = this.BCon.getValue();
				if (EV.Log != null && value >= 0)
				{
					EV.Log.AddSelection(this.Atext[value], true);
				}
			}
		}

		public override MsgBox run(float fcnt, bool force_draw = false)
		{
			if (!force_draw)
			{
				bool d = X.D;
			}
			if (this.clms == 0)
			{
				return base.run(fcnt, force_draw);
			}
			if (!base.visible)
			{
				return null;
			}
			base.run(fcnt, force_draw);
			if (this.result == "" && this.can_handle_)
			{
				if (this.BtnCurs == null)
				{
					if (this.BtnCancel != null && EV.INisCancel())
					{
						this.BCon.setValue(this.BCon.getIndex(this.BtnCancel), true);
						IN.clearPushDown(true);
					}
					else if (EV.INisKettei() || IN.isB() || IN.isR())
					{
						for (int i = 0; i < this.BCon.Length; i++)
						{
							if (!this.BCon.Get(i).isLocked())
							{
								this.BCon.setValue(i, true);
								break;
							}
						}
						IN.clearPushDown(true);
					}
					else if (IN.isT() || IN.isL())
					{
						for (int j = this.BCon.Length - 1; j >= 0; j--)
						{
							if (!this.BCon.Get(j).isLocked())
							{
								this.BCon.setValue(j, true);
								break;
							}
						}
						IN.clearPushDown(true);
					}
				}
				else if (this.t >= 20f && EV.INisCancel() && this.BtnCancel != null)
				{
					if (this.BtnCurs == this.BtnCancel)
					{
						this.result = this.BtnCancel.title;
					}
					else
					{
						this.BCon.setValue(this.BCon.getIndex(this.BtnCancel), true);
					}
					SND.Ui.play("cancel", false);
				}
			}
			if (this.result != "")
			{
				this.selectQuit();
			}
			return this;
		}

		public int countRows()
		{
			return this.Atext.Count;
		}

		public void set_handle(bool f)
		{
			if (this.clms == 0)
			{
				return;
			}
			if (!f)
			{
				if (this.BtnCurs != null)
				{
					this.BtnCurs.SetChecked(false, true);
				}
				this.BCon.hide(false, false);
				this.can_handle_ = false;
				return;
			}
			if (this.t >= 0f && this.BtnCurs != null)
			{
				this.BtnCurs.SetChecked(true, true);
			}
			this.BCon.bind(false, false);
			this.can_handle_ = true;
			if (base.isActive() && 0 <= this.default_focus && this.default_focus < this.BCon.Length)
			{
				this.BCon.Get(this.default_focus).Select(false);
			}
		}

		public bool can_handle
		{
			get
			{
				return this.can_handle_;
			}
		}

		public override bool use_valotile
		{
			get
			{
				return base.use_valotile;
			}
			set
			{
				base.use_valotile = value;
				if (this.BCon != null)
				{
					for (int i = this.BCon.Length - 1; i >= 0; i--)
					{
						this.BCon.Get(i).use_valotile = value;
					}
				}
			}
		}

		private BtnContainerRadio<aBtn> BCon;

		private ObjCarrierCon Carr;

		private List<aBtn> ARow;

		private List<string> Atext;

		public string result = "";

		private int SEL_W = 270;

		private int sel_w;

		private const int SEL_LH = 28;

		private const int T_CLICK_ALLOW = 20;

		public int default_focus = -1;

		private aBtn BtnCurs;

		private aBtn BtnCancel;

		private bool can_handle_;

		private int clms;

		private int rows;

		internal string define_to = "";

		private string position_key;

		public BDic<string, string> ORandomTalkFocus;

		public string randam_talk_focus;

		private const string checkmark_tag = "<shape tx_color check />";
	}
}
