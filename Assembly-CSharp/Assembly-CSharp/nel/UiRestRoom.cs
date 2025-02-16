using System;
using System.Collections.Generic;
using evt;
using m2d;
using nel.gm;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiRestRoom : UiBoxDesignerFamily, IEventWaitListener, IValotileSetable
	{
		public static UiRestRoom CreateInstance()
		{
			if (UiRestRoom.Instance == null)
			{
				UiRestRoom.Instance = new GameObject("UI_restroom").AddComponent<UiRestRoom>();
			}
			return UiRestRoom.Instance;
		}

		public static UiRestRoom ReleaseInstance()
		{
			if (UiRestRoom.Instance != null)
			{
				IN.DestroyOne(UiRestRoom.Instance.gameObject);
				UiRestRoom.Instance = null;
			}
			return null;
		}

		public static void setHilight(string key)
		{
			if (TX.noe(key))
			{
				if (UiRestRoom.Ahilight.Count > 0)
				{
					UiRestRoom.need_check_hilight = true;
				}
				UiRestRoom.Ahilight.Clear();
				return;
			}
			UiRestRoom.need_check_hilight = true;
			UiRestRoom.Ahilight.Add(key);
		}

		public static void evQuit()
		{
			UiRestRoom.Ahilight.Clear();
			UiRestRoom.need_check_hilight = false;
		}

		public override void destruct()
		{
			base.destruct();
			this.NM2D.remValotAddition(this);
		}

		protected override void Awake()
		{
			base.Awake();
			this.NM2D = M2DBase.Instance as NelM2DBase;
			this.auto_deactive_gameobject = false;
			base.gameObject.layer = IN.gui_layer;
			UiRestRoom.need_check_hilight = false;
			this.BxC = base.Create("cmd", -IN.w * 0.5f, 30f, 0f, 120f, 0, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxC.alignx = ALIGN.CENTER;
			this.BxC.Small();
			this.BxC.margin_in_lr = 20f;
			this.BxC.margin_in_tb = 30f;
			this.auto_activate = 0U;
			if (this.NM2D != null)
			{
				this.NM2D.loadMaterialSnd("ev_restroom");
				this.NM2D.addValotAddition(this);
			}
		}

		private void createCmdInner(UiRestRoom.RSTATE cp)
		{
			this.BxC.Clear();
			this.SelectBtn = null;
			this.cmd_praperd = cp;
			int num = (((cp & UiRestRoom.RSTATE.TOP_B) != UiRestRoom.RSTATE.OFFLINE) ? 1 : 0) + (((cp & UiRestRoom.RSTATE.TOP_T) != UiRestRoom.RSTATE.OFFLINE) ? 1 : 0);
			float num2 = 300f * (float)num + 20f * (float)(num - 1);
			float num3 = 0f;
			if ((cp & UiRestRoom.RSTATE.TOP_B) != UiRestRoom.RSTATE.OFFLINE)
			{
				num3 = X.Mx(num3, (float)(this.Acmds_b0.Length + 1) * 24f);
			}
			if ((cp & UiRestRoom.RSTATE.TOP_T) != UiRestRoom.RSTATE.OFFLINE)
			{
				num3 = X.Mx(num3, (float)(this.Acmds_t0.Length + 1) * 24f);
			}
			this.ABtnWhole = new List<aBtn>(num * 4);
			UiRestRoom.need_check_hilight = false;
			num3 += 107f;
			this.BxC.WH(num2 + 40f + 5f, num3);
			this.BxC.item_margin_x_px = 0f;
			this.BxC.item_margin_y_px = 0f;
			this.BxC.positionD(-IN.wh + num2 * 0.5f + 80f, 30f, 2, 60f);
			this.BxC.init();
			int num4 = 0;
			Designer designer = null;
			DsnDataButton dsnDataButton = new DsnDataButton
			{
				skin = "row_center",
				w = 300f,
				h = 24f,
				fnClick = new FnBtnBindings(this.fnClickToEventCmd)
			};
			for (int i = 0; i < 2; i++)
			{
				if ((cp & ((i == 0 || 2 != 0) ? UiRestRoom.RSTATE.TOP_B : UiRestRoom.RSTATE.OFFLINE)) != UiRestRoom.RSTATE.OFFLINE)
				{
					if (num4 > 0)
					{
						int num5 = 10;
						this.BxC.HrV(0.88f, (float)num5, (float)(20 - num5 - 1), 1f);
					}
					string[] array = ((i == 0) ? this.Acmds_b0 : this.Acmds_t0);
					Designer designer2 = this.BxC.addTab("btab-" + i.ToString(), 300f, 24f * (float)array.Length * 24f, 0f, 0f, false);
					designer2.Smallest();
					designer2.addP(new DsnDataP("", false)
					{
						swidth = 300f,
						sheight = 42f,
						alignx = ALIGN.CENTER,
						aligny = ALIGNY.MIDDLE,
						html = true,
						TxCol = C32.d2c(4283780170U),
						text = "<img mesh=\"" + ((i == 0) ? "restroom_shower" : "restroom_toilet") + "\" />"
					}, false);
					designer2.Br();
					int num6 = array.Length;
					aBtn aBtn = null;
					aBtn aBtn2 = null;
					for (int j = 0; j < num6; j++)
					{
						dsnDataButton.title = array[j];
						dsnDataButton.h = (float)((j == 0) ? 2 : 1) * 24f;
						aBtnNel aBtnNel = designer2.addButtonT<aBtnNel>(dsnDataButton);
						if (j > 0)
						{
							aBtn.setNaviB(aBtnNel, true, true);
						}
						else
						{
							aBtn2 = aBtnNel;
						}
						designer2.Br();
						(aBtnNel.get_Skin() as ButtonSkinRow).fix_text_size = (float)((j == 0) ? 16 : 14);
						aBtn = aBtnNel;
					}
					BtnContainer<aBtn> btnContainer = designer2.getBtnContainer();
					aBtn2.setNaviT(aBtn, true, true);
					if (designer != null)
					{
						int length = designer2.getBtnContainer().Length;
						int num7 = X.Mx(num6, length);
						for (int k = 0; k < num7; k++)
						{
							aBtn aBtn3 = designer.getBtnContainer().Get(X.Mn(k, length - 1));
							aBtn aBtn4 = btnContainer.Get(X.Mn(k, num6 - 1));
							aBtn3.setNaviL(aBtn4, true, true);
							aBtn3.setNaviR(aBtn4, true, true);
						}
					}
					designer = designer2;
					this.BxC.endTab(true);
					if (num4 == 0)
					{
						this.SelectBtn = aBtn2;
					}
					int num8 = array.Length;
					num4++;
					for (int l = 0; l < num8; l++)
					{
						aBtn aBtn5 = btnContainer.Get(l);
						this.ABtnWhole.Add(aBtn5);
						string title = aBtn5.title;
						aBtn5.locked_click = true;
						if (title == "&&Cancel")
						{
							aBtn5.click_snd = "cancel";
						}
						else
						{
							bool flag = SCN.isBenchCmdEnable(title);
							aBtn5.setSkinTitle((flag ? "" : "<shape lock tx_color/>") + TX.Get("Bench_Cmd_" + array[l], ""));
							if (!flag)
							{
								aBtn5.SetLocked(true, true, false);
							}
						}
						ButtonSkinNelUi buttonSkinNelUi = aBtn5.get_Skin() as ButtonSkinNelUi;
						if (buttonSkinNelUi != null && (UiRestRoom.Ahilight.IndexOf(title) >= 0 || UiRestRoom.Ahilight.IndexOf("&&" + title) >= 0))
						{
							buttonSkinNelUi.notice_exc = true;
						}
					}
				}
			}
		}

		private void recheckHilight()
		{
			for (int i = this.ABtnWhole.Count - 1; i >= 0; i--)
			{
				aBtn aBtn = this.ABtnWhole[i];
				ButtonSkinNelUi buttonSkinNelUi = aBtn.get_Skin() as ButtonSkinNelUi;
				string title = aBtn.title;
				if (buttonSkinNelUi != null)
				{
					buttonSkinNelUi.notice_exc = UiRestRoom.Ahilight.IndexOf(title) >= 0 || UiRestRoom.Ahilight.IndexOf("&&" + title) >= 0;
				}
			}
			UiRestRoom.need_check_hilight = false;
		}

		public override T CreateT<T>(string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f, UiBoxDesignerFamily.MASKTYPE mask = UiBoxDesignerFamily.MASKTYPE.BOX)
		{
			T t = base.CreateT<T>(name, pixel_x, pixel_y, pixel_w, pixel_h, appear_dir_aim, appear_len, UiBoxDesignerFamily.MASKTYPE.NO_MASK);
			t.Focusable(false, false, null);
			t.WHanim(t.get_swidth_px(), t.get_sheight_px(), false, false);
			return t;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.active;
		}

		public UiRestRoom Init(UiRestRoom.RSTATE _type)
		{
			this.Pr = M2DBase.Instance.curMap.Pr as PR;
			if (this.Pr == null)
			{
				X.de("UiRestRoom::Init: Pr が不明", null);
				return this;
			}
			this.activate();
			IN.setZ(base.transform, -4.8100004f);
			this.changeState(_type);
			if (this.pre_cmd_executed != null)
			{
				UiBenchMenu.ShowLogForOtherCommand(this.pre_cmd_executed, this.pre_cmd_result);
				this.pre_cmd_executed = null;
			}
			return this;
		}

		public void changeState(UiRestRoom.RSTATE _state)
		{
			this.state = _state;
			if (_state - UiRestRoom.RSTATE.TOP_B <= 2)
			{
				this.BxC.activate();
				if (this.cmd_praperd != _state)
				{
					this.createCmdInner(_state);
				}
				else if (UiRestRoom.need_check_hilight)
				{
					this.recheckHilight();
				}
				if (this.SelectBtn != null)
				{
					this.SelectBtn.Select(true);
				}
			}
		}

		public override UiBoxDesignerFamily activate()
		{
			base.activate();
			if (this.SelectBtn != null)
			{
				this.SelectBtn.SetChecked(false, true);
			}
			return this;
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				return false;
			}
			UiRestRoom.RSTATE rstate = this.state;
			if (rstate - UiRestRoom.RSTATE.TOP_B <= 2 && IN.isCancel())
			{
				SND.Ui.play("cancel", false);
				this.executeCancel();
			}
			return true;
		}

		private void executeCancel()
		{
			EV.getVariableContainer().define("_result", "0", true);
			this.deactivate(false);
		}

		private bool fnClickToEventCmd(aBtn B)
		{
			bool flag = true;
			int num = 0;
			if (B.title == "&&Cancel")
			{
				EV.getVariableContainer().define("_result", "0", true);
				flag = false;
			}
			else
			{
				string title = B.title;
				if (title != null && title == "restroom_cure_egged")
				{
					if (this.Pr.EggCon.total == 0)
					{
						UILog.Instance.AddAlertTX("Alert_no_need_lay_egg", UILogRow.TYPE.ALERT);
						return true;
					}
					flag = false;
				}
				else if (B.isLocked())
				{
					UILog.Instance.AddAlertTX("Alert_bench_execute_scenario_locked", UILogRow.TYPE.ALERT);
					return true;
				}
				EV.getVariableContainer().define("_result", B.title, true);
			}
			if (flag)
			{
				this.pre_cmd_executed = B.title;
				num = UiBenchMenu.executeOtherCommand(this.pre_cmd_executed, false);
			}
			this.SelectBtn = B;
			this.pre_cmd_result = num;
			EV.getVariableContainer().define("_result_cmd", this.pre_cmd_result.ToString(), true);
			B.SetChecked(true, true);
			this.deactivate(false);
			return true;
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			IN.clearPushDown(false);
			this.NM2D.remValotAddition(this);
			return base.deactivate(immediate);
		}

		private UiBoxDesigner BxC;

		private UiRestRoom.RSTATE state;

		private M2DBase NM2D;

		private static List<string> Ahilight = new List<string>(2);

		private string[] Acmds_b0 = new string[] { "shower_clean_cure_cloth", "shower_cure_cloth", "shower_clean", "shower" };

		private string[] Acmds_t0 = new string[] { "pee_excrete", "restroom_cure_egged", "cure_ep_sensitivity", "pee" };

		private const string bench_menu_prefix = "Bench_Cmd_";

		private const float margin_in_x = 20f;

		private const float margin_in_y = 30f;

		private const float margin_out_x = 40f;

		private const float btn_h = 24f;

		private const float btn_w = 300f;

		private const float hr_w = 20f;

		private static UiRestRoom Instance;

		private List<aBtn> ABtnWhole;

		private UiRestRoom.RSTATE cmd_praperd;

		private PR Pr;

		private aBtn SelectBtn;

		private static bool need_check_hilight = false;

		private string pre_cmd_executed;

		private int pre_cmd_result = -1;

		public enum RSTATE
		{
			OFFLINE,
			TOP_B,
			TOP_T,
			TOP_BT
		}
	}
}
