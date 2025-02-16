using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class UiGuildDepartBox : UiBoxDesigner
	{
		public UiGuildDepartBox createUi(NelM2DBase _M2D)
		{
			if (this.M2D != null)
			{
				return this;
			}
			this.M2D = _M2D;
			this.Abtn_keys = new List<string>(4);
			this.Abtn_keys.Add("0");
			this.Abtn_keys.Add("1");
			this.Abtn_keys.Add("2");
			this.Abtn_keys.Add("3");
			this.DetailSelectMain = null;
			this.margin_in_lr = 40f;
			this.margin_in_tb = 22f;
			base.item_margin_y_px = 0f;
			base.box_stencil_ref_mask = 180;
			this.init();
			float use_w = base.use_w;
			this.WmCtr = new FieldGuideWmController(this.M2D, new Func<bool>(base.isActive)).createUiTo(this, use_w, base.use_h - 26f - 48f - 10f - 1f - 16f - 24f);
			this.FbKD = base.Br().addP(new DsnDataP("", false)
			{
				sheight = 24f,
				swidth = use_w,
				alignx = ALIGN.CENTER,
				text = " ",
				size = 14f,
				TxCol = NEL.ColText,
				html = true
			}, false);
			base.Br().addHr(new DsnDataHr
			{
				line_height = 1f,
				margin_b = 5f,
				margin_t = 5f,
				draw_width_rate = 0.88f
			});
			Designer designer = this.addTab("B", use_w, base.use_h, use_w, base.use_h, false);
			designer.Smallest();
			designer.margin_in_tb = 8f;
			designer.margin_in_lr = 18f;
			designer.alignx = ALIGN.CENTER;
			designer.init();
			this.FbLB = designer.addP(new DsnDataP("", false)
			{
				sheight = designer.use_h,
				swidth = use_w - 460f - 30f - designer.margin_in_lr * 2f,
				alignx = ALIGN.LEFT,
				text_margin_x = 10f,
				text = " ",
				TxCol = NEL.ColText,
				html = true,
				size = 14f,
				text_auto_wrap = true,
				text_auto_condense = true
			}, false);
			Designer designer2 = designer;
			DsnDataRadio dsnDataRadio = new DsnDataRadio();
			dsnDataRadio.name = "rb";
			dsnDataRadio.skin = "normal";
			dsnDataRadio.keysL = this.Abtn_keys;
			dsnDataRadio.w = 230f;
			dsnDataRadio.h = 24f;
			dsnDataRadio.clms = 2;
			dsnDataRadio.margin_w = 14;
			dsnDataRadio.margin_h = 14;
			dsnDataRadio.navi_loop = 3;
			dsnDataRadio.fnHover = delegate(aBtn B)
			{
				this.DetailSelectMain = B;
				return true;
			};
			dsnDataRadio.fnChanged = delegate(BtnContainerRadio<aBtn> BCon, int pre, int cur)
			{
				if (cur >= 0)
				{
					if (this.FnClickedCmd != null && !this.FnClickedCmd(BCon.Get(cur)))
					{
						return false;
					}
					this.decided_btn = BCon.Get(cur).title;
				}
				return true;
			};
			dsnDataRadio.fnMakingAfter = delegate(BtnContainer<aBtn> BCon, aBtn B)
			{
				if (B.title == "&&Confirm" || B.title == "&&Cancel")
				{
					B.click_snd = "";
				}
				return true;
			};
			this.BConRB = designer2.addRadioT<aBtnNel>(dsnDataRadio);
			this.BConRB.APool = new List<aBtn>();
			base.endTab(true);
			return this;
		}

		public UiGuildDepartBox activateButtons(bool btn_submit, UiGuildDepartBox.RECIEVE btn_recieve, bool btn_deliver, WholeMapItem WM, object _Target = null, bool show_target_without_know_icon = false)
		{
			this.Abtn_keys.Clear();
			this.WmCtr.clearTarget();
			this.decided_btn = null;
			this.cancel_btn = null;
			if (btn_submit)
			{
				this.cancel_btn = ((btn_recieve == UiGuildDepartBox.RECIEVE.RECIEVE) ? "&&Cancel" : "&&Confirm");
				this.Abtn_keys.Add(this.cancel_btn);
			}
			switch (btn_recieve)
			{
			case UiGuildDepartBox.RECIEVE.RECIEVE:
				this.Abtn_keys.Add("&&Guild_Btn_recieve_quest");
				this.FbLB.Txt(TX.Get("Guild_recieving_ask", ""));
				break;
			case UiGuildDepartBox.RECIEVE.ABORT:
				this.Abtn_keys.Add("&&Guild_Btn_abort_quest_ask");
				this.FbLB.Txt(TX.Get("Guild_recieving_already", ""));
				break;
			case UiGuildDepartBox.RECIEVE.ABORT_TEMP:
				this.Abtn_keys.Add("&&Guild_Btn_abort_quest_temp");
				this.FbLB.Txt(TX.Get("Guild_recieving_temp", ""));
				break;
			default:
				this.FbLB.Txt("");
				break;
			}
			if (btn_deliver)
			{
				this.Abtn_keys.Add("&&Guild_Btn_deliver");
			}
			if (_Target != null && this.M2D.IMNG.has_recipe_collection)
			{
				this.Abtn_keys.Add("&&Guild_Btn_recipebook");
			}
			this.BConRB.RemakeT<aBtnNel>(this.Abtn_keys.ToArray(), "normal");
			base.reboundCarrForBtnMulti(this.BConRB, true);
			this.BConRB.setValue(-1, false);
			this.fine_map_kd = (this.need_reset_position = true);
			this.activate();
			this.WmCtr.show_target_without_know_icon = show_target_without_know_icon;
			if (WM != null)
			{
				this.WmCtr.WmSkin.setWholeMapTarget(WM, (float)(WM.Mp.clms / 2), (float)(WM.Mp.rows / 2));
			}
			this.WmCtr.PickupTarget = _Target;
			if (this.BConRB.Length > 0)
			{
				this.BConRB.Get(0).Select(true);
			}
			return this;
		}

		public void runUsing(float fcnt)
		{
			if (aBtn.PreSelected == this.WmSkin.getBtn())
			{
				byte b = 0;
				if (!this.WmCtr.runEdit(fcnt, true, ref b) || (this.WmSkin.is_detail && IN.isSubmit()))
				{
					IN.clearPushDown(true);
					SND.Ui.play("cursor", false);
					if (this.DetailSelectMain != null)
					{
						this.DetailSelectMain.Select(true);
					}
					else if (this.BConRB.Length > 0)
					{
						this.BConRB.Get(0).Select(true);
					}
					this.fine_map_kd = true;
				}
				else if (b > 0)
				{
					this.fine_map_kd = true;
				}
			}
			else if (this.cancelable && IN.isCancel())
			{
				this.deactivate();
			}
			else if (IN.isUiSortPD() && this.WmCtr != null)
			{
				this.DetailSelectMain = aBtn.PreSelected;
				this.WmCtr.initEdit();
				SND.Ui.play("toggle_button_open", false);
				this.fine_map_kd = true;
			}
			if (this.WmCtr.runAppearing())
			{
				this.fine_map_kd = true;
			}
			if (this.need_reset_position)
			{
				this.need_reset_position = false;
				Vector2 vector;
				if (this.WmCtr.getDescPointAverage(out vector))
				{
					this.WmCtr.WmSkin.SetCenter(vector.x, vector.y, false, true);
				}
			}
			if (this.fine_map_kd)
			{
				this.fineMapKD();
			}
		}

		public void ExecuteReceiveQuest()
		{
			aBtn aBtn = this.BConRB.Get("&&Guild_Btn_recieve_quest");
			if (aBtn != null)
			{
				aBtn.ExecuteOnSubmitKey();
			}
		}

		public void ExecuteAbortQuest()
		{
			aBtn aBtn = this.BConRB.Get("&&Guild_Btn_abort_quest_temp");
			if (aBtn != null)
			{
				aBtn.ExecuteOnSubmitKey();
				return;
			}
			aBtn = this.BConRB.Get("&&Guild_Btn_abort_quest_ask");
			if (aBtn != null)
			{
				aBtn.ExecuteOnSubmitKey();
			}
		}

		public override Designer deactivate()
		{
			if (base.isActive())
			{
				base.deactivate();
				if (this.BConRB != null && this.decided_btn == null)
				{
					this.decided_btn = "";
					if (this.cancel_btn != null)
					{
						aBtn aBtn = this.BConRB.Get(this.cancel_btn);
						if (aBtn != null)
						{
							this.BConRB.setValue(this.BConRB.getIndex(aBtn), false);
						}
					}
				}
			}
			return this;
		}

		private void fineMapKD()
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				this.WmCtr.fineMapKD(stb, true, false);
				if (aBtn.PreSelected == this.WmSkin.getBtn())
				{
					stb.Add(" ");
					if (this.WmSkin.is_detail)
					{
						stb.Add("<key submit/>");
					}
					stb.AddTxA("Guild_cmd_map_return", false);
				}
				this.FbKD.Txt(stb);
				this.fine_map_kd = false;
			}
		}

		public void SelectRB(string btn_key)
		{
			if (this.BConRB == null || this.BConRB.Length == 0)
			{
				return;
			}
			base.Focus();
			this.BConRB.setValue(-1, true);
			if (btn_key != null)
			{
				aBtn aBtn = this.BConRB.Get(btn_key);
				if (aBtn != null)
				{
					aBtn.Select(true);
					return;
				}
			}
			this.BConRB.Get(0).Select(true);
		}

		public override Designer activate()
		{
			base.activate();
			if (this.BConRB != null)
			{
				this.BConRB.setValue(-1, true);
			}
			return this;
		}

		public bool isFocusingToMapArea()
		{
			return this.WmCtr.isFocusingToMapArea();
		}

		public void errorMessageToDesc(string desc)
		{
			if (this.FbLB != null)
			{
				SND.Ui.play("locked", false);
				this.FbLB.text_content = NEL.error_tag + desc + NEL.error_tag_close;
			}
		}

		public ButtonSkinWholeMapArea WmSkin
		{
			get
			{
				if (this.WmCtr == null)
				{
					return null;
				}
				return this.WmCtr.WmSkin;
			}
		}

		private NelM2DBase M2D;

		public static float outw = IN.w * 0.66f;

		public static float outh = IN.h * 0.8f;

		private const float btnh = 24f;

		private const int btn_margin_y = 14;

		private const float bottom_margin_tb = 8f;

		private const float btnw = 230f;

		private const float line_h = 10f;

		private const float kd_h = 24f;

		public bool cancelable = true;

		private FillBlock FbKD;

		private FillBlock FbLB;

		private BtnContainerRadio<aBtn> BConRB;

		public FnBtnBindings FnClickedCmd;

		private FieldGuideWmController WmCtr;

		private aBtn DetailSelectMain;

		private List<string> Abtn_keys;

		private bool need_reset_position;

		private bool fine_map_kd;

		public string decided_btn;

		private string cancel_btn;

		public enum RECIEVE
		{
			RECIEVE,
			ABORT,
			ABORT_TEMP,
			NONE
		}
	}
}
